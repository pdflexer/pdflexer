using System.CommandLine;
using System.Diagnostics;
using PdfLexer.Remediation;

namespace PdfLexer.pdfctl.Inspect;

internal sealed class RemediateCmd
{
    public string File { get; set; } = null!;

    public string Rules { get; set; } = null!;

    public string? Output { get; set; }

    public bool DryRun { get; set; }

    public bool ValidateOnly { get; set; }

    public bool VeraPdf { get; set; }

    public string VeraPdfCommand { get; set; } = "verapdf";

    public static Command Create()
    {
        return new Command("remediate", "Applies serialized remediation rules to an untagged PDF")
        {
            new Option<string>(new[] { "-f", "--file" })
            {
                IsRequired = true,
                Description = "Path to the source PDF."
            },
            new Option<string>(new[] { "-r", "--rules" })
            {
                IsRequired = true,
                Description = "Path to a pdflexer.remediation.ruleset.v1 JSON file."
            },
            new Option<string?>(new[] { "-o", "--output" })
            {
                Description = "Path for the remediated PDF. Required unless --dry-run or --validate-only is used."
            },
            new Option<bool>("--dry-run", "Evaluate rules and print diagnostics without writing output."),
            new Option<bool>("--validate-only", "Validate rule shape without parsing page content."),
            new Option<bool>("--verapdf", "Run veraPDF on the output PDF after a successful commit."),
            new Option<string>("--verapdf-command", () => "verapdf", "veraPDF executable path or command name.")
        };
    }

    public static int Handler(RemediateCmd cmd)
    {
        var job = SerializedRemediationRules.Load(cmd.Rules);
        using var pdf = PdfDocument.Open(cmd.File);
        using var session = pdf.BeginRemediation(job.Session);

        var validation = session.Validate(job.RuleSet);
        PrintValidation(validation);
        if (!validation.IsValid)
        {
            return 2;
        }

        if (cmd.ValidateOnly)
        {
            return 0;
        }

        session.Use(job.RuleSet);
        var report = cmd.DryRun ? session.DryRun() : session.Commit();
        PrintReport(report);
        if (report.Diagnostics.Any(d => !d.StartsWith("[SUPPRESSED]", StringComparison.Ordinal)))
        {
            return 3;
        }

        if (cmd.DryRun)
        {
            return 0;
        }

        if (string.IsNullOrWhiteSpace(cmd.Output))
        {
            Console.Error.WriteLine("--output is required unless --dry-run or --validate-only is used.");
            return 4;
        }

        pdf.SaveTo(cmd.Output);
        Console.WriteLine($"Wrote {cmd.Output}");

        if (cmd.VeraPdf)
        {
            return RunVeraPdf(cmd.VeraPdfCommand, cmd.Output);
        }

        return 0;
    }

    private static void PrintValidation(ValidationReport validation)
    {
        Console.WriteLine(validation.IsValid ? "Rule validation: passed" : "Rule validation: failed");
        foreach (var warning in validation.Warnings)
        {
            Console.WriteLine("warning: " + warning);
        }

        foreach (var error in validation.Errors)
        {
            Console.Error.WriteLine("error: " + error);
        }
    }

    private static void PrintReport(RemediationReport report)
    {
        Console.WriteLine($"Committed: {report.Committed}");
        Console.WriteLine($"Claims: {report.Claims.Count}");
        Console.WriteLine($"Skipped claims: {report.SkippedClaims.Count}");
        foreach (var diagnostic in report.Diagnostics)
        {
            var output = diagnostic.StartsWith("[SUPPRESSED]", StringComparison.Ordinal) ? Console.Out : Console.Error;
            output.WriteLine("diagnostic: " + diagnostic);
        }
    }

    private static int RunVeraPdf(string command, string output)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = command,
            RedirectStandardError = true,
            RedirectStandardOutput = true
        };
        startInfo.ArgumentList.Add("--format");
        startInfo.ArgumentList.Add("text");
        startInfo.ArgumentList.Add("--flavour");
        startInfo.ArgumentList.Add("ua1");
        startInfo.ArgumentList.Add(output);

        using var process = Process.Start(startInfo);
        if (process == null)
        {
            Console.Error.WriteLine($"Could not start veraPDF command '{command}'.");
            return 5;
        }

        process.OutputDataReceived += (_, args) => { if (args.Data != null) Console.WriteLine(args.Data); };
        process.ErrorDataReceived += (_, args) => { if (args.Data != null) Console.Error.WriteLine(args.Data); };
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();
        return process.ExitCode;
    }
}
