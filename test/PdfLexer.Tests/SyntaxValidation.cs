#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PdfLexer.Tests;

internal static class SyntaxValidation
{
    private const string SkipMessage = "Skipping pdfcpu-backed syntax validation because `pdfcpu` was not found. Install `pdfcpu` or set PDFCPU_PATH to enable these checks.";

    public static string? GetSkipReason()
    {
        return ResolvePdfCpuPath() == null ? SkipMessage : null;
    }

    public static List<string> Validate(byte[] pdf)
    {
        var tmp = Path.GetTempPath();
        var pdfPath = Path.Combine(tmp, Guid.NewGuid().ToString() + ".pdf");
        try
        {
            File.WriteAllBytes(pdfPath, pdf);
            return Validate(pdfPath);
        } finally
        {
            File.Delete(pdfPath);
        }

    }
    public static List<string> Validate(string pdfPath)
    {
        var errs = new List<string>();
        var cpu = ResolvePdfCpuPath();
        if (cpu == null)
        {
            throw new InvalidOperationException("SyntaxValidation.Validate was called without pdfcpu being available. Mark the test with [PdfCpuFact].");
        }

        var fn = Path.GetFileName(pdfPath);

        var startInfo = new ProcessStartInfo();
        startInfo.WorkingDirectory = Path.GetDirectoryName(pdfPath);
        startInfo.RedirectStandardError = true;
        startInfo.RedirectStandardOutput = true;
        startInfo.FileName = cpu;
        startInfo.Arguments = $"validate -m strict \"{fn}\"";
        startInfo.UseShellExecute = false;
        var pdfcpu = Process.Start(startInfo);
        if (pdfcpu == null)
        {
            throw new InvalidOperationException($"Failed to start pdfcpu process '{cpu}'.");
        }
        pdfcpu.ErrorDataReceived += delegate (object sender, DataReceivedEventArgs e)
        {
            if (e?.Data == null || string.IsNullOrWhiteSpace(e.Data)) { return; }
            lock (errs)
            {
                errs.Add("pdfcpu stderr: " + e.Data + Environment.NewLine);
            }

        };
        pdfcpu.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e)
        {
            if (e?.Data == null || string.IsNullOrWhiteSpace(e.Data) || e.Data.Trim().StartsWith("validating") || e.Data.Contains("validation ok")) { return; }
            lock (errs)
            {
                errs.Add("pdfcpu stdout: " + e.Data + Environment.NewLine);
            }

        };
        pdfcpu.BeginErrorReadLine();
        pdfcpu.BeginOutputReadLine();

        pdfcpu.WaitForExit();
        if (pdfcpu.ExitCode != 0)
        {
            errs.Add("pdfcpu exit code: " + pdfcpu.ExitCode);
        }
        return errs;
    }

    private static string? ResolvePdfCpuPath()
    {
        var configured = Environment.GetEnvironmentVariable("PDFCPU_PATH");
        if (!string.IsNullOrWhiteSpace(configured))
        {
            if (File.Exists(configured))
            {
                return configured;
            }

            return null;
        }

        var path = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        var pathExts = (Environment.GetEnvironmentVariable("PATHEXT") ?? ".EXE;.CMD;.BAT;.COM")
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var dir in path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var candidate = Path.Combine(dir, "pdfcpu");
            if (File.Exists(candidate))
            {
                return candidate;
            }

            foreach (var ext in pathExts.Select(x => x.StartsWith('.') ? x : "." + x))
            {
                var withExt = candidate + ext;
                if (File.Exists(withExt))
                {
                    return withExt;
                }
            }
        }

        return null;
    }

}
