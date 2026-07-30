using System;
using System.Collections.Generic;
using System.Linq;
using PdfLexer.DOM;
using PdfLexer.Fonts;
using PdfLexer.Remediation;
using Xunit;

namespace PdfLexer.Tests;

public class RemediationGroupPassTests
{
    [Fact]
    public void GroupPasses_BuildNestedListAndPreserveLeafMcids()
    {
        using var doc = CreateFourLineDocument("Outer label", "Outer body", "Inner label", "Inner body");
        using var session = doc.BeginRemediation(Configuration("Nested list"));
        var rules = NestedListRules();

        var dryRun = session.DryRun(rules);
        Assert.Empty(dryRun.Diagnostics);
        Assert.Equal(new[] { 0, 10, 20, 30 },
            dryRun.RuleEvaluations.Where(x => x.Stage == Stage.Group).Select(x => x.GroupPass).ToArray());

        var report = session.Commit(rules);
        Assert.True(report.Committed);
        Assert.Empty(report.Diagnostics);

        var outerList = Assert.Single(doc.Structure.GetRoot().Children);
        Assert.Equal("L", outerList.Type);
        var outerItem = Assert.Single(outerList.Children);
        Assert.Equal("LI", outerItem.Type);
        Assert.Equal(new[] { "Lbl", "LBody", "L" }, outerItem.Children.Select(x => x.Type).ToArray());
        var innerList = outerItem.Children[2];
        var innerItem = Assert.Single(innerList.Children);
        Assert.Equal(new[] { "Lbl", "LBody" }, innerItem.Children.Select(x => x.Type).ToArray());

        Assert.Equal(new[] { 2, 3 }, Mcids(report, "inner-item"));
        Assert.Equal(new[] { 2, 3 }, Mcids(report, "inner-list"));
        Assert.Equal(new[] { 0, 1, 2, 3 }, Mcids(report, "outer-item"));
        Assert.Equal(new[] { 0, 1, 2, 3 }, Mcids(report, "outer-list"));
    }

    [Fact]
    public void PositionalPredicates_ResolveConsumedLowerPassClaims()
    {
        using var doc = CreateFourLineDocument("Anchor", "Body", "Unused one", "Unused two");
        using var session = doc.BeginRemediation(Configuration("Positional references"));

        var report = session.DryRun(
            TagLine("anchor", "P", "Anchor"),
            TagLine("body", "P", "Body"),
            TagLine("unused", "Artifact", "^Unused", artifact: true),
            new Rule(
                "anchor-parent",
                RemediationActions.Group("Div", ClaimPredicates.FromRule("anchor")),
                stage: Stage.Group),
            new Rule(
                "body-parent",
                RemediationActions.Group(
                    "Sect",
                    ClaimPredicates.FromRule("body").And(ClaimPredicates.AfterClaim("anchor"))),
                stage: Stage.Group,
                groupPass: 10));

        Assert.Empty(report.Diagnostics);
        Assert.Single(report.Claims, x => x.RuleId == "body-parent");
    }

    [Fact]
    public void ConsumedProducerReference_ExplainsZeroMatch()
    {
        using var doc = CreateFourLineDocument("A", "B", "C", "D");
        using var session = doc.BeginRemediation(Configuration("Consumed input"));

        var report = session.DryRun(
            TagLine("leaf", "P", "A"),
            TagLine("rest", "Artifact", "^(B|C|D)$", artifact: true),
            new Rule(
                "lower-parent",
                RemediationActions.Group("Div", ClaimPredicates.FromRule("leaf")),
                stage: Stage.Group),
            new Rule(
                "wrong-input",
                RemediationActions.Group("Sect", ClaimPredicates.FromRule("leaf")),
                stage: Stage.Group,
                groupPass: 10));

        Assert.Contains(report.Warnings, x =>
            x.Contains("wrong-input", StringComparison.Ordinal) &&
            x.Contains("lower-parent (pass 0)", StringComparison.Ordinal) &&
            x.Contains("Select the produced parent", StringComparison.Ordinal));
    }

    [Fact]
    public void SamePassAmbiguity_IsRejectedAndCannotBeSuppressed()
    {
        using var doc = CreateFourLineDocument("A", "B", "C", "D");
        using var session = doc.BeginRemediation(Configuration(
            "Ambiguity",
            RemediationDiagnosticStrictness.Permissive));
        session.Suppress(DiagnosticCode.GroupCompositionAmbiguous, "*", "must not suppress");

        var report = session.DryRun(
            TagLine("items", "P", "^(A|B|C|D)$"),
            new Rule(
                "first-parent",
                RemediationActions.Group("Div", ClaimPredicates.FromRule("items")),
                stage: Stage.Group),
            new Rule(
                "second-parent",
                RemediationActions.Group("Sect", ClaimPredicates.FromRule("items")),
                stage: Stage.Group));

        var diagnostic = Assert.Single(report.Diagnostics, x =>
            x.Contains(nameof(DiagnosticCode.GroupCompositionAmbiguous), StringComparison.Ordinal));
        Assert.DoesNotContain("SUPPRESSED", diagnostic, StringComparison.Ordinal);
        Assert.Contains("Later passes were not evaluated", diagnostic, StringComparison.Ordinal);
    }

    [Fact]
    public void DefensiveValidation_RejectsAncestorDescendantAndCycle()
    {
        using var doc = PdfDocument.Create();
        using var session = doc.BeginRemediation(Configuration(
            "Defensive validation",
            RemediationDiagnosticStrictness.Permissive));
        session.Suppress(DiagnosticCode.GroupCompositionAmbiguous, "*", "must not suppress");
        session.Suppress(DiagnosticCode.GroupCompositionCycle, "*", "must not suppress");

        var leaf = Claim("leaf", RemediationActions.Tag("P"));
        var parent = Claim("parent", RemediationActions.Group("Div", ClaimPredicate.Always));
        parent.AddRelatedClaims(new[] { leaf });
        var invalid = Claim("invalid", RemediationActions.Group("Sect", ClaimPredicate.Always), 10);
        invalid.AddRelatedClaims(new[] { parent, leaf });
        var diagnostics = new List<string>();
        session.ValidateGroupComposition(new[] { parent, invalid }, diagnostics);
        Assert.Contains(diagnostics, x =>
            x.Contains(nameof(DiagnosticCode.GroupCompositionAmbiguous), StringComparison.Ordinal) &&
            x.Contains("ancestor", StringComparison.Ordinal));
        Assert.DoesNotContain(diagnostics, x => x.Contains("SUPPRESSED", StringComparison.Ordinal));

        var first = Claim("cycle-a", RemediationActions.Group("Div", ClaimPredicate.Always));
        var second = Claim("cycle-b", RemediationActions.Group("Sect", ClaimPredicate.Always), 10);
        first.AddRelatedClaims(new[] { second });
        second.AddRelatedClaims(new[] { first });
        diagnostics.Clear();
        session.ValidateGroupComposition(new[] { first, second }, diagnostics);
        Assert.Contains(diagnostics, x => x.Contains(nameof(DiagnosticCode.GroupCompositionCycle), StringComparison.Ordinal));
        Assert.DoesNotContain(diagnostics, x => x.Contains("SUPPRESSED", StringComparison.Ordinal));
    }

    [Fact]
    public void TableAndMergeOutputs_ShareFrontierAndComposeIntoSection()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
        {
            writer.Font(Standard14Font.GetHelvetica(), 12)
                .TextMove(50, 720).Text("Heading")
                .TextMove(50, 680).Text("A1")
                .TextMove(220, 680).Text("B1")
                .TextMove(50, 650).Text("A2")
                .TextMove(220, 650).Text("B2")
                .TextMove(50, 610).Text("First fragment")
                .TextMove(50, 590).Text("Second fragment")
                .EndText();
        }

        using var session = doc.BeginRemediation(Configuration("Table and merge"));
        var report = session.Commit(
            TagLine("heading", "H1", "Heading"),
            new Rule(
                "cells",
                RemediationActions.Tag("Span"),
                Predicates.Text.Matches("^[AB][12]$"),
                CandidateSelector.Text(Granularity.Word)),
            new Rule(
                "fragments",
                RemediationActions.Tag("Span"),
                Predicates.Text.Matches("fragment$"),
                CandidateSelector.Text(Granularity.Line)),
            new Rule(
                "table",
                RemediationActions.TableOver(ClaimPredicates.FromRule("cells"), 100, 300),
                stage: Stage.Group),
            new Rule(
                "paragraph",
                RemediationActions.MergeTo("P", ClaimPredicates.FromRule("fragments")),
                stage: Stage.Group),
            new Rule(
                "section",
                RemediationActions.Group(
                    "Sect",
                    ClaimPredicates.FromRule("heading")
                        .Or(ClaimPredicates.FromRule("table"))
                        .Or(ClaimPredicates.FromRule("paragraph"))),
                stage: Stage.Group,
                groupPass: 10));

        Assert.True(report.Committed);
        Assert.Empty(report.Diagnostics);
        var section = Assert.Single(doc.Structure.GetRoot().Children);
        Assert.Equal("Sect", section.Type);
        Assert.Equal(new[] { "H1", "Table", "P" }, section.Children.Select(x => x.Type).ToArray());
        Assert.Equal(7, Mcids(report, "section").Length);
    }

    [Fact]
    public void OmittedAndExplicitPassZero_AreByteIdenticalAcrossComposedRuleSets()
    {
        Assert.Equal(PassZeroBytes(explicitPass: false), PassZeroBytes(explicitPass: true));
    }

    [Fact]
    public void HigherPassCannotConsumeMergedFragmentsOrSyntheticTableCells()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
        {
            writer.Font(Standard14Font.GetHelvetica(), 12)
                .TextMove(50, 700).Text("A1")
                .TextMove(220, 700).Text("B1")
                .TextMove(50, 670).Text("A2")
                .TextMove(220, 670).Text("B2")
                .TextMove(50, 620).Text("First fragment")
                .TextMove(50, 600).Text("Second fragment")
                .EndText();
        }

        using var session = doc.BeginRemediation(Configuration("Interior frontier"));
        var report = session.DryRun(
            new Rule(
                "cells",
                RemediationActions.Tag("Span"),
                Predicates.Text.Matches("^[AB][12]$"),
                CandidateSelector.Text(Granularity.Word)),
            new Rule(
                "fragments",
                RemediationActions.Tag("Span"),
                Predicates.Text.Matches("fragment$"),
                CandidateSelector.Text(Granularity.Line)),
            new Rule(
                "table",
                RemediationActions.TableOver(ClaimPredicates.FromRule("cells"), 100, 300),
                stage: Stage.Group),
            new Rule(
                "paragraph",
                RemediationActions.MergeTo("P", ClaimPredicates.FromRule("fragments")),
                stage: Stage.Group),
            new Rule(
                "merged-fragment-consumer",
                RemediationActions.Group("Div", ClaimPredicates.FromRule("fragments")),
                stage: Stage.Group,
                cardinality: RuleCardinality.Exactly(1),
                groupPass: 10),
            new Rule(
                "synthetic-cell-consumer",
                RemediationActions.Group("Div", ClaimPredicates.ClaimIs("TD")),
                stage: Stage.Group,
                cardinality: RuleCardinality.Exactly(1),
                groupPass: 10));

        Assert.DoesNotContain(report.Claims, x => x.RuleId is "merged-fragment-consumer" or "synthetic-cell-consumer");
        Assert.Contains(report.Warnings, x =>
            x.Contains("merged-fragment-consumer", StringComparison.Ordinal) &&
            x.Contains("paragraph (pass 0)", StringComparison.Ordinal));
        Assert.Contains(report.Diagnostics, x =>
            x.Contains(nameof(DiagnosticCode.RuleCardinalityMismatch), StringComparison.Ordinal) &&
            x.Contains("merged-fragment-consumer", StringComparison.Ordinal));
        Assert.Contains(report.Diagnostics, x =>
            x.Contains(nameof(DiagnosticCode.RuleCardinalityMismatch), StringComparison.Ordinal) &&
            x.Contains("synthetic-cell-consumer", StringComparison.Ordinal));
    }

    private static byte[] PassZeroBytes(bool explicitPass)
    {
        using var doc = CreateFourLineDocument("A", "B", "C", "D");
        var leaves = new RuleSet(
            "leaves",
            new Rule(
                "items",
                RemediationActions.Tag("P"),
                RemediationPredicate.Always,
                CandidateSelector.Text(Granularity.Line)));
        var group = explicitPass
            ? new Rule(
                "container",
                RemediationActions.Group("Div", ClaimPredicates.FromRule("items")),
                stage: Stage.Group,
                groupPass: 0)
            : new Rule(
                "container",
                RemediationActions.Group("Div", ClaimPredicates.FromRule("items")),
                stage: Stage.Group);
        var groups = new RuleSet("groups", group);
        using var session = doc.BeginRemediation(Configuration("Pass zero compatibility"))
            .Use(leaves, groups);
        var report = session.Commit();
        Assert.True(report.Committed);
        return doc.Save();
    }

    private static PdfDocument CreateFourLineDocument(params string[] lines)
    {
        var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        using var writer = page.GetWriter();
        writer.Font(Standard14Font.GetHelvetica(), 12);
        for (var i = 0; i < lines.Length; i++)
        {
            writer.TextMove(50, 720 - i * 30).Text(lines[i]);
        }
        writer.EndText();
        return doc;
    }

    private static Rule[] NestedListRules() => new[]
    {
        TagLine("outer-label", "Lbl", "Outer label"),
        TagLine("outer-body", "LBody", "Outer body"),
        TagLine("inner-label", "Lbl", "Inner label"),
        TagLine("inner-body", "LBody", "Inner body"),
        new Rule(
            "inner-item",
            RemediationActions.Group(
                "LI",
                ClaimPredicates.FromRule("inner-label").Or(ClaimPredicates.FromRule("inner-body"))),
            stage: Stage.Group),
        new Rule(
            "inner-list",
            RemediationActions.Group("L", ClaimPredicates.FromRule("inner-item")),
            stage: Stage.Group,
            groupPass: 10),
        new Rule(
            "outer-item",
            RemediationActions.Group(
                "LI",
                ClaimPredicates.FromRule("outer-label")
                    .Or(ClaimPredicates.FromRule("outer-body"))
                    .Or(ClaimPredicates.FromRule("inner-list"))),
            stage: Stage.Group,
            groupPass: 20),
        new Rule(
            "outer-list",
            RemediationActions.Group("L", ClaimPredicates.FromRule("outer-item")),
            stage: Stage.Group,
            groupPass: 30)
    };

    private static Rule TagLine(string id, string tag, string pattern, bool artifact = false) =>
        new(
            id,
            artifact ? RemediationActions.Artifact(ArtifactSubtype.Layout) : RemediationActions.Tag(tag),
            Predicates.Text.Matches(pattern),
            CandidateSelector.Text(Granularity.Line));

    private static RemediationSessionConfiguration Configuration(
        string title,
        RemediationDiagnosticStrictness strictness = RemediationDiagnosticStrictness.Strict) => new()
    {
        Language = "en-US",
        Title = title,
        StrictConformance = false,
        LeftoverPolicy = RemediationLeftoverPolicy.Flag,
        DiagnosticStrictness = strictness
    };

    private static RemediationClaim Claim(string id, RemediationAction action, int pass = 0) =>
        new(id, Array.Empty<RemediationCandidate>(), action.DebugString)
        {
            Action = action,
            GroupPass = pass,
            Status = ClaimStatus.Applied
        };

    private static int[] Mcids(RemediationReport report, string ruleId) =>
        Assert.Single(report.Claims, x => x.RuleId == ruleId)
            .AppliedBindings.SelectMany(x => x.Mcids).Distinct().OrderBy(x => x).ToArray();
}
