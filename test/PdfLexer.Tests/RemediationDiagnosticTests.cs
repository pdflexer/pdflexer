using System;
using System.Linq;
using PdfLexer.Content;
using PdfLexer.Content.Model;
using PdfLexer.DOM;
using PdfLexer.Fonts;
using PdfLexer.Remediation;
using Xunit;

namespace PdfLexer.Tests;

public class RemediationDiagnosticTests
{
    [Fact]
    public void Diagnostic_UntaggedContent_FiresWhenFlagPolicyIsUsed()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
        {
            writer.Font(Standard14Font.GetHelvetica(), 12)
                .TextMove(50, 700).Text("Tagged")
                .TextMove(50, 680).Text("Untagged")
                .EndText();
        }

        using var session = doc.BeginRemediation(new RemediationSessionConfiguration
        {
            LeftoverPolicy = RemediationLeftoverPolicy.Flag,
            StrictConformance = false
        });

        var rule = new Rule(
            "test-rule",
            RemediationActions.Tag("P"),
            Predicates.Text.Equals("Tagged"),
            Granularity.Paragraph);

        var error = Assert.Throws<InvalidOperationException>(() => session.Commit(rule));
        Assert.Contains("UntaggedContent: Page 1 has", error.Message);
    }

    [Fact]
    public void Diagnostic_UntaggedContent_DowngradedBySuppression()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
        {
            writer.Font(Standard14Font.GetHelvetica(), 12)
                .TextMove(50, 700).Text("Tagged")
                .TextMove(50, 680).Text("Untagged")
                .EndText();
        }

        using var session = doc.BeginRemediation(new RemediationSessionConfiguration
        {
            LeftoverPolicy = RemediationLeftoverPolicy.Flag,
            DiagnosticStrictness = RemediationDiagnosticStrictness.Permissive,
            StrictConformance = false
        });

        session.Suppress(DiagnosticCode.UntaggedContent, "Page1", "Testing suppression");

        var rule = new Rule(
            "test-rule",
            RemediationActions.Tag("P"),
            Predicates.Text.Equals("Tagged"),
            Granularity.Paragraph);

        var report = session.Commit(rule);
        
        Assert.Contains(report.Diagnostics, d => d.Contains("[SUPPRESSED] UntaggedContent: Page 1 has"));
    }

    [Fact]
    public void Diagnostic_UntaggedContent_StrictModeIgnoresSuppression()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
        {
            writer.Font(Standard14Font.GetHelvetica(), 12)
                .TextMove(50, 700).Text("Tagged")
                .TextMove(50, 680).Text("Untagged")
                .EndText();
        }

        using var session = doc.BeginRemediation(new RemediationSessionConfiguration
        {
            LeftoverPolicy = RemediationLeftoverPolicy.Flag,
            DiagnosticStrictness = RemediationDiagnosticStrictness.Strict,
            StrictConformance = false
        });

        session.Suppress(DiagnosticCode.UntaggedContent, "Page1", "Testing suppression");

        var rule = new Rule(
            "test-rule",
            RemediationActions.Tag("P"),
            Predicates.Text.Equals("Tagged"),
            Granularity.Paragraph);

        var error = Assert.Throws<InvalidOperationException>(() => session.Commit(rule));
        Assert.Contains("[IGNORED-SUPPRESSION] UntaggedContent: Page 1 has", error.Message);
    }

    [Fact]
    public void Diagnostic_ReadingOrderDrift_FiresWhenClaimsAreReordered()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
        {
            writer.Font(Standard14Font.GetHelvetica(), 12)
                .TextMove(50, 680).Text("First")
                .TextMove(50, 700).Text("Second")
                .EndText();
        }

        using var session = doc.BeginRemediation(new RemediationSessionConfiguration
        {
            LeftoverPolicy = RemediationLeftoverPolicy.AutoArtifact,
            StrictConformance = false
        });

        var r1 = new Rule("r1", RemediationActions.Tag("P"), Predicates.Text.Equals("First"), Granularity.Word);
        var r2 = new Rule("r2", RemediationActions.Tag("P"), Predicates.Text.Equals("Second"), Granularity.Word);
        var swap = new Rule("swap", RemediationActions.ReorderSiblings(ClaimPredicate.Always, SiblingReorderMode.GeometryTopToBottom), stage: Stage.Refine);

        var error = Assert.Throws<InvalidOperationException>(() => session.Commit(r1, r2, swap));
        Assert.Contains("ReadingOrderDrift: Logical reading order drift detected on page 1", error.Message);
    }
}
