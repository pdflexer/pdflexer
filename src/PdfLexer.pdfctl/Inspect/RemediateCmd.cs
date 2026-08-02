using System.CommandLine;
using System.Diagnostics;
using System.Text.Json;
using PdfLexer.Remediation;

namespace PdfLexer.pdfctl.Inspect;

internal sealed class RemediateCmd
{
    public string? File { get; set; }

    public string Program { get; set; } = null!;

    public string? Output { get; set; }

    public bool DryRun { get; set; }

    public bool ValidateOnly { get; set; }

    public bool Authoring { get; set; }

    public bool VeraPdf { get; set; }

    public string VeraPdfCommand { get; set; } = "verapdf";

    public string? ExplainBinding { get; set; }

    public int? ExplainPage { get; set; }

    public static Command Create()
    {
        return new Command("remediate", "Applies a serialized remediation program to an untagged PDF")
        {
            new Option<string?>(new[] { "-f", "--file" })
            {
                Description = "Path to the source PDF. Required unless --validate-only is used."
            },
            new Option<string>(new[] { "-p", "--program" })
            {
                IsRequired = true,
                Description = "Path to a pdflexer.remediation.program.preview1 JSON file."
            },
            new Option<string?>(new[] { "-o", "--output" })
            {
                Description = "Path for the remediated PDF. Required unless --dry-run or --validate-only is used."
            },
            new Option<bool>("--dry-run", "Evaluate bindings and print diagnostics without writing output."),
            new Option<bool>("--validate-only", "Validate program declarations without parsing page content."),
            new Option<bool>("--authoring", "Run an authoring-mode dry-run and report incomplete work."),
            new Option<bool>("--verapdf", "Run veraPDF on the output PDF after a successful commit."),
            new Option<string>("--verapdf-command", () => "verapdf", "veraPDF executable path or command name."),
            new Option<string?>("--explain-binding", "Retain and print rejection traces for this binding (dry-run only)."),
            new Option<int?>("--explain-page", "Optional one-based page filter for --explain-binding.")
        };
    }

    public static int Handler(RemediateCmd cmd)
    {
        try
        {
            if (!Preflight(cmd)) return 4;
            return HandleProgram(cmd);
        }
        catch (JsonException ex)
        {
            Console.Error.WriteLine("error: malformed remediation JSON: " + ex.Message);
            return 2;
        }
        catch (InvalidDataException ex)
        {
            Console.Error.WriteLine("error: " + ex.Message);
            return 2;
        }
        catch (IOException ex)
        {
            Console.Error.WriteLine("error: " + ex.Message);
            return 5;
        }
    }

    private static bool Preflight(RemediateCmd cmd)
    {
        var dryRun = cmd.DryRun || cmd.Authoring;
        if (!cmd.ValidateOnly && string.IsNullOrWhiteSpace(cmd.File))
        {
            Console.Error.WriteLine("--file is required unless --validate-only is used.");
            return false;
        }
        if (!cmd.ValidateOnly && !dryRun && string.IsNullOrWhiteSpace(cmd.Output))
        {
            Console.Error.WriteLine("--output is required unless --dry-run or --validate-only is used.");
            return false;
        }
        if (cmd.ExplainBinding != null && !dryRun)
        {
            Console.Error.WriteLine("--explain-binding is dry-run only; use --dry-run or --authoring.");
            return false;
        }
        if (cmd.ValidateOnly && cmd.VeraPdf)
        {
            Console.Error.WriteLine("--verapdf cannot be used with --validate-only.");
            return false;
        }
        return true;
    }

    private static int HandleProgram(RemediateCmd cmd)
    {
        var program = SerializedRemediationProgram.Load(cmd.Program);
        var compiled = RemediationProgramCompiler.Compile(program);
        var declarationValidation = new ValidationReport(compiled.Errors);
        PrintValidation(declarationValidation);
        if (!declarationValidation.IsValid)
        {
            return 2;
        }

        Console.WriteLine($"program: id={program.Id} template={program.Template.Id}@{program.Template.Version} profile={program.Template.Profile}");
        for (var layer = 0; layer < compiled.Layers.Count; layer++)
        {
            Console.WriteLine($"dependency-layer: {layer} bindings={string.Join(",", compiled.Layers[layer].Select(x => x.Id))}");
        }

        if (cmd.ValidateOnly)
        {
            return 0;
        }

        using var pdf = PdfDocument.Open(cmd.File!);
        using var session = pdf.BeginRemediation(new RemediationSessionConfiguration
        {
            Profile = program.Template.Profile,
            RunMode = cmd.Authoring ? RemediationRunMode.Authoring : RemediationRunMode.Enforced
        });

        session.Use(compiled);

        var dryRun = cmd.DryRun || cmd.Authoring;
        var report = cmd.ExplainBinding == null
            ? dryRun ? session.DryRun() : session.Commit()
            : session.DryRun(new RemediationTraceRequest(
                new[] { cmd.ExplainBinding },
                cmd.ExplainPage is { } page ? page - 1 : null));
        PrintReport(report);
        if (report.RuntimeDiagnostics.Any(d => d.IsBlocking))
        {
            return 3;
        }

        if (dryRun)
        {
            return 0;
        }

        pdf.SaveTo(cmd.Output!);
        Console.WriteLine($"Wrote {cmd.Output}");

        if (cmd.VeraPdf)
        {
            return RunVeraPdf(cmd.VeraPdfCommand, cmd.Output!, program.Template.Profile);
        }

        return 0;
    }

    private static void PrintValidation(ValidationReport validation)
    {
        Console.WriteLine(validation.IsValid ? "Program validation: passed" : "Program validation: failed");
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
        Console.WriteLine($"Binding occurrences: {report.Claims.Count}");
        Console.WriteLine($"Skipped binding occurrences: {report.SkippedClaims.Count}");
        foreach (var warning in report.Warnings)
        {
            Console.WriteLine("warning: " + warning);
        }
        foreach (var binding in report.BindingEvaluations)
        {
            Console.WriteLine(
                $"binding: {binding.BindingId} layer={binding.DependencyLayer} " +
                $"slot={binding.ProgramSlot?.Path ?? "<none>"} artifact={binding.ArtifactId ?? "<none>"} " +
                $"considered={binding.InputsConsidered} matched={binding.InputsMatched} " +
                $"applied={binding.AppliedClaims} skipped={binding.SkippedClaims} " +
                $"cardinality={binding.CardinalityOutcome ?? "<none>"}");
        }
        foreach (var comparison in report.OrderComparisons)
        {
            Console.WriteLine(
                $"order: disposition={comparison.Disposition} " +
                $"container={comparison.ContainerSlot?.Path ?? "/"} " +
                $"first={comparison.FirstOccurrenceIdentity} second={comparison.SecondOccurrenceIdentity} " +
                $"source-inverted={comparison.SourceOrderInverted} " +
                $"geometry-inverted={comparison.GeometricOrderInverted}");
        }
        foreach (var diagnostic in report.RuntimeDiagnostics)
        {
            var output = diagnostic.IsBlocking ? Console.Error : Console.Out;
            output.WriteLine(
                $"diagnostic: disposition={diagnostic.Disposition} code={diagnostic.Code} " +
                $"scope={diagnostic.Scope} slot={diagnostic.ProgramSlot?.Path ?? "<none>"} " +
                $"binding={diagnostic.BindingId ?? "<none>"} message={diagnostic.Message}");
        }
        foreach (var outcome in report.Outcomes.Where(x => x.PageIndexes.Count > 1))
        {
            Console.WriteLine(
                $"cross-page-binding: binding={outcome.RuleId} pages=" +
                string.Join(",", outcome.PageIndexes.Select(x => x + 1)));
        }
        if (report.UnaccountedContent.Count > 0)
        {
            Console.WriteLine($"Unaccounted painting content: {report.UnaccountedContent.Count}");
            foreach (var item in report.UnaccountedContent)
            {
                Console.WriteLine(
                    $"unaccounted: page={item.PageIndex + 1} kind={item.CandidateKind} " +
                    $"candidate={item.CandidateId} source={item.SourceReference} " +
                    $"bounds={item.BoundingBox} relative={item.RelativeBoundingBox} " +
                    $"resource={item.ResourceIdentity ?? "<none>"} name={item.ResourceName ?? "<none>"} " +
                    $"reuse={item.ResourceUseCount} raw=\"{Preview(item.RawText ?? string.Empty)}\" " +
                    $"normalized=\"{Preview(item.NormalizedText ?? string.Empty)}\"");
            }
        }

        foreach (var occurrence in report.TemplateAssembly)
        {
            Console.WriteLine(
                $"template-assembly: slot={occurrence.SlotId} path={occurrence.TemplatePath} " +
                $"occurrence={occurrence.OccurrenceIndex} identity={occurrence.Identity} " +
                $"parent={occurrence.ParentIdentity} rule={occurrence.ProducingRuleId ?? "<synthesized>"} " +
                $"claim={occurrence.ProducingClaimReference ?? "<none>"} " +
                $"consumed={string.Join(",", occurrence.ConsumedClaimReferences)} " +
                $"synthesized={occurrence.Synthesized} opaque-interior={occurrence.OpaqueInterior}");
        }

        foreach (var difference in report.TemplateDifferences)
        {
            Console.WriteLine(
                $"template: kind={difference.Kind} slot={difference.SlotId ?? "<none>"} " +
                $"expected-path={difference.ExpectedPath ?? "<none>"} expected={difference.ExpectedValue ?? "<none>"} " +
                $"actual-path={difference.ActualPath ?? "<none>"} actual={difference.ActualValue ?? "<none>"} " +
                $"pages={string.Join(",", difference.PageIndexes.Select(x => x + 1))} " +
                $"rule={difference.RuleId ?? "<none>"} suppressed={difference.Suppressed}");
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

    private static int RunVeraPdf(string command, string output, PdfUaProfile profile)
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
        startInfo.ArgumentList.Add(profile == PdfUaProfile.PdfUa2 ? "ua2" : "ua1");
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
