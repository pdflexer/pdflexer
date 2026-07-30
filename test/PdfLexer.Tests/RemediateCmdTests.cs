using System;
using System.Collections.Generic;
using System.IO;
using PdfLexer.Content;
using PdfLexer.DOM;
using PdfLexer.pdfctl.Inspect;
using PdfLexer.Remediation;
using Xunit;

namespace PdfLexer.Tests;

public class RemediateCmdTests
{
    [Fact]
    public void PrintReport_IncludesZeroMatchRuleAndAutoArtifactDetails()
    {
        var report = new RemediationReport(
            committed: false,
            appliedAccessibilitySetup: false,
            ruleEvaluations: new[]
            {
                new RuleEvaluationSummary(
                    "missing-total",
                    "invoice-v1",
                    Stage.Classify,
                    new RuleEvaluationCounts(4, 0, 0, 0, 0, 0),
                    new[]
                    {
                        new PageRuleEvaluationSummary(
                            0,
                            new RuleEvaluationCounts(4, 0, 0, 0, 0, 0))
                    }),
                new RuleEvaluationSummary(
                    "outer-section",
                    "invoice-v1",
                    Stage.Group,
                    new RuleEvaluationCounts(2, 2, 0, 0, 1, 0),
                    Array.Empty<PageRuleEvaluationSummary>(),
                    GroupPass: 10)
            },
            autoArtifacts: new[]
            {
                new RemediationAutoArtifactOutcome(
                    0,
                    new StructuredSourceRef(1, 2, 3),
                    "Invoice total that would otherwise be hidden",
                    new PdfRect<double>(1, 2, 3, 4),
                    RemediationAutoArtifactDisposition.Planned)
            });

        var original = Console.Out;
        using var output = new StringWriter();
        try
        {
            Console.SetOut(output);
            RemediateCmd.PrintReport(report);
        }
        finally
        {
            Console.SetOut(original);
        }

        var text = output.ToString();
        Assert.Contains("rule: missing-total stage=Classify group-pass=0 considered=4 matched=0 applied=0", text);
        Assert.Contains("rule: outer-section stage=Group group-pass=10 considered=2 matched=2 applied=1", text);
        Assert.Contains("Auto-artifacts: 1", text);
        Assert.Contains("auto-artifact: Planned page=1", text);
        Assert.Contains("Invoice total that would otherwise be hidden", text);
    }
}
