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

    public string? ExplainRule { get; set; }

    public int? ExplainPage { get; set; }

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
            new Option<string>("--verapdf-command", () => "verapdf", "veraPDF executable path or command name."),
            new Option<string?>("--explain-rule", "Retain and print rejection traces for this rule (dry-run only)."),
            new Option<int?>("--explain-page", "Optional one-based page filter for --explain-rule.")
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
        if (cmd.ExplainRule != null && !cmd.DryRun)
        {
            Console.Error.WriteLine("--explain-rule is dry-run only.");
            return 4;
        }

        var report = cmd.ExplainRule == null
            ? cmd.DryRun ? session.DryRun() : session.Commit()
            : session.DryRun(new RemediationTraceRequest(
                new[] { cmd.ExplainRule },
                cmd.ExplainPage is { } page ? page - 1 : null));
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

    internal static void PrintReport(RemediationReport report)
    {
        Console.WriteLine($"Committed: {report.Committed}");
        Console.WriteLine($"Claims: {report.Claims.Count}");
        Console.WriteLine($"Skipped claims: {report.SkippedClaims.Count}");
        foreach (var rule in report.RuleEvaluations)
        {
            var count = rule.Total;
            Console.WriteLine(
                $"rule: {rule.RuleId} considered={count.InputsConsidered} matched={count.InputsMatched} " +
                $"applied={count.AppliedClaims} low-confidence={count.RejectedByConfidence} " +
                $"conflict={count.RejectedByConflict} overridden={count.OverriddenClaims}");
        }

        if (report.AutoArtifacts.Count > 0)
        {
            Console.WriteLine($"Auto-artifacts: {report.AutoArtifacts.Count}");
            foreach (var artifact in report.AutoArtifacts)
            {
                Console.WriteLine(
                    $"auto-artifact: {artifact.Disposition} page={artifact.PageIndex + 1} " +
                    $"kind={artifact.CandidateKind} candidate={artifact.CandidateId ?? "<none>"} " +
                    $"source={artifact.SourceReference} bounds={artifact.BoundingBox} " +
                    $"resource={artifact.ResourceIdentity ?? "<none>"} name={artifact.ResourceName ?? "<none>"} " +
                    $"reuse={artifact.ResourceUseCount} text=\"{Preview(artifact.Text)}\"");
            }
        }

        foreach (var diagnostic in report.Diagnostics)
        {
            var output = diagnostic.StartsWith("[SUPPRESSED]", StringComparison.Ordinal) ? Console.Out : Console.Error;
            output.WriteLine("diagnostic: " + diagnostic);
        }

        foreach (var trace in report.PredicateTraces)
        {
            Console.WriteLine(
                $"rejected: rule={trace.RuleId} page={trace.PageIndex + 1} candidate={trace.CandidateId} " +
                $"kind={trace.CandidateKind} raw=\"{Preview(trace.RawText ?? string.Empty)}\" " +
                $"normalized=\"{Preview(trace.NormalizedText ?? string.Empty)}\"");
            PrintTrace(trace.Trace, 1);
        }
    }

    private static void PrintTrace(PredicateTraceNode node, int depth)
    {
        var indent = new string(' ', depth * 2);
        var state = node.Evaluated ? node.Result?.ToString() ?? "unknown" : "skipped";
        var rejecting = node.RejectingAndOperand is { } operand ? $" rejecting-and-operand={operand}" : string.Empty;
        Console.WriteLine($"{indent}{node.Predicate}: {state}{rejecting}{(node.Reason == null ? string.Empty : " — " + node.Reason)}");
        foreach (var child in node.Children ?? Array.Empty<PredicateTraceNode>())
        {
            PrintTrace(child, depth + 1);
        }
    }

    private static string Preview(string text)
    {
        var normalized = string.Join(" ", text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        return normalized.Length <= 80 ? normalized : normalized[..77] + "...";
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
