#nullable enable
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using PdfLexer.DOM;

namespace PdfLexer.Tests;

internal static class VeraPdfValidation
{
    private const string SkipMessage =
        "Skipping veraPDF-backed PDF/UA validation because `verapdf` was not found. " +
        "Install veraPDF or set VERAPDF_PATH to enable these checks.";

    public static string? GetSkipReason() => ResolvePath() == null ? SkipMessage : null;

    /// <summary>
    /// Validates a document, retrying only when the tool produced no verdict at all. veraPDF is an
    /// external process and occasionally aborts under repeated invocation; retrying a run that never
    /// completed is not the same as retrying a run that returned a real result, which is never retried.
    /// </summary>
    public static VeraPdfResult Validate(byte[] pdf, PdfUaProfile profile)
    {
        try
        {
            var result = RunOnce(pdf, profile);
            return result.ValidationPerformed && !result.ProcessingFailed ? result : RunOnce(pdf, profile);
        }
        catch (InvalidOperationException)
        {
            // No verdict was produced at all, so there is nothing to preserve by failing here.
            return RunOnce(pdf, profile);
        }
    }

    private static VeraPdfResult RunOnce(byte[] pdf, PdfUaProfile profile)
    {
        var executable = ResolvePath();
        if (executable == null)
        {
            throw new InvalidOperationException(
                "VeraPdfValidation.Validate was called without veraPDF being available. Mark the test with [VeraPdfFact].");
        }

        var pdfPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.pdf");
        try
        {
            // Flushed to disk explicitly: veraPDF is a separate process, and without this it
            // intermittently opens a partially written file and reports "not a valid PDF".
            using (var stream = new FileStream(pdfPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                stream.Write(pdf, 0, pdf.Length);
                stream.Flush(true);
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = executable,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            startInfo.ArgumentList.Add("--format");
            startInfo.ArgumentList.Add("xml");
            startInfo.ArgumentList.Add("--flavour");
            startInfo.ArgumentList.Add(profile == PdfUaProfile.PdfUa1 ? "ua1" : "ua2");
            startInfo.ArgumentList.Add(pdfPath);

            using var process = Process.Start(startInfo) ??
                throw new InvalidOperationException($"Could not start veraPDF command '{executable}'.");
            // Both pipes must be drained concurrently. Reading stdout to completion first deadlocks
            // whenever veraPDF fills the stderr buffer, which truncates the XML report and shows up as
            // a random fixture "failing" validation.
            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();
            var standardOutput = outputTask.GetAwaiter().GetResult();
            var standardError = errorTask.GetAwaiter().GetResult();
            process.WaitForExit();

            XDocument report;
            try
            {
                report = XDocument.Parse(standardOutput);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"veraPDF did not return a machine-readable XML report. Exit code: {process.ExitCode}. " +
                    $"stderr: {standardError}",
                    ex);
            }

            var validationReports = report
                .Descendants()
                .Where(x => x.Name.LocalName == "validationReport")
                .ToList();
            if (validationReports.Count == 0)
            {
                throw new InvalidOperationException(
                    $"veraPDF XML contained no validation reports. Exit code: {process.ExitCode}. stderr: {standardError}");
            }

            var expectedProfileName = profile == PdfUaProfile.PdfUa1
                ? "PDF/UA-1 validation profile"
                : "PDF/UA-2 + Tagged PDF validation profile";
            var details = validationReports
                .Select(x => x.Elements().FirstOrDefault(e => e.Name.LocalName == "details"))
                .ToList();
            var passedRules = details.Sum(x => ParseCount(x, "passedRules"));
            var failedRules = details.Sum(x => ParseCount(x, "failedRules"));
            var passedChecks = details.Sum(x => ParseCount(x, "passedChecks"));
            var failedChecks = details.Sum(x => ParseCount(x, "failedChecks"));
            var validationPerformed =
                validationReports.Count == 1 &&
                validationReports.All(x =>
                    string.Equals((string?)x.Attribute("jobEndStatus"), "normal", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals((string?)x.Attribute("profileName"), expectedProfileName, StringComparison.Ordinal)) &&
                passedRules + failedRules > 0 &&
                passedChecks + failedChecks > 0;
            var compliant = validationPerformed && validationReports.All(x =>
                string.Equals((string?)x.Attribute("isCompliant"), "true", StringComparison.OrdinalIgnoreCase));
            var batchSummary = report.Descendants().FirstOrDefault(x => x.Name.LocalName == "batchSummary");
            var processingFailed = batchSummary != null &&
                new[] { "failedToParse", "encrypted", "outOfMemory", "veraExceptions" }
                    .Any(name => int.TryParse((string?)batchSummary.Attribute(name), out var value) && value != 0);

            return new VeraPdfResult(
                compliant,
                validationPerformed,
                processingFailed,
                expectedProfileName,
                passedRules,
                failedRules,
                passedChecks,
                failedChecks,
                process.ExitCode,
                standardOutput,
                standardError);
        }
        finally
        {
            File.Delete(pdfPath);
        }
    }

    private static int ParseCount(XElement? element, string attributeName)
        => int.TryParse((string?)element?.Attribute(attributeName), out var value) ? value : 0;

    private static string? ResolvePath()
    {
        var configured = Environment.GetEnvironmentVariable("VERAPDF_PATH");
        if (!string.IsNullOrWhiteSpace(configured))
        {
            return File.Exists(configured) ? configured : null;
        }

        var path = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        foreach (var directory in path.Split(
                     Path.PathSeparator,
                     StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            foreach (var fileName in OperatingSystem.IsWindows()
                         ? new[] { "verapdf.bat", "verapdf.exe", "verapdf" }
                         : new[] { "verapdf" })
            {
                var candidate = Path.Combine(directory, fileName);
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }
        }

        return null;
    }
}

internal sealed record VeraPdfResult(
    bool IsCompliant,
    bool ValidationPerformed,
    bool ProcessingFailed,
    string ProfileName,
    int PassedRules,
    int FailedRules,
    int PassedChecks,
    int FailedChecks,
    int ExitCode,
    string Report,
    string StandardError);
