using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PdfLexer.Content.Model;
using PdfLexer.DOM;
using PdfLexer.Fonts;
using PdfLexer.Remediation;
using PdfLexer.Writing;
using Xunit;

namespace PdfLexer.Tests;

public class RemediationArtifactInventoryTests
{
    [Fact]
    public void JsonV1_ParsesInventoryAndRuleBinding()
    {
        const string json = """
        {
          "schema": "pdflexer.remediation.ruleset.v1",
          "id": "invoice",
          "zones": [{ "id": "footer", "bounds": { "kind": "marginRelative", "bottom": 42 } }],
          "artifacts": [
            { "id": "page-footer", "subtype": "pagination", "zone": "footer",
              "pages": { "kind": "every" } },
            { "id": "letterhead-rule", "subtype": "layout", "minCount": 0, "maxCount": 3 }
          ],
          "rules": [{
            "id": "footer", "artifact": "page-footer",
            "candidates": { "kind": "text", "granularity": "line" },
            "action": { "kind": "artifact", "subtype": "pagination" }
          }]
        }
        """;

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var ruleSet = SerializedRemediationRules.Load(stream).RuleSet;

        var footer = ruleSet.Artifacts[0];
        Assert.Equal("page-footer", footer.Id);
        Assert.Equal(ArtifactSubtype.Pagination, footer.Subtype);
        Assert.Equal("footer", footer.ZoneId);
        Assert.Equal(AssertionCount.Exactly(1), footer.Occurrence);
        Assert.Equal("invoice", footer.RuleSetId);
        Assert.Equal(new AssertionCount(0, 3), ruleSet.Artifacts[1].Occurrence);
        Assert.Equal("page-footer", Assert.Single(ruleSet.Rules).Artifact);
    }

    [Fact]
    public void DeclarationValidation_RejectsDuplicateIdsAcrossComposedRuleSets()
    {
        var report = SerializedRemediationRules.ValidateDeclarations(
            new RuleSet("boilerplate", Array.Empty<Rule>(), artifacts: new[] { Item("page-footer") }),
            new RuleSet("invoice", Array.Empty<Rule>(), artifacts: new[] { Item("page-footer") }));

        Assert.Contains(report.Errors, x => x.Contains("duplicated", StringComparison.Ordinal));
    }

    [Fact]
    public void DeclarationValidation_RejectsUnknownZone()
    {
        var report = SerializedRemediationRules.ValidateDeclarations(
            new RuleSet("set", Array.Empty<Rule>(), artifacts: new[] { Item("page-footer", zoneId: "footer") }));

        Assert.Contains(report.Errors, x => x.Contains("unknown toleranced zone", StringComparison.Ordinal));
    }

    [Fact]
    public void DeclarationValidation_RejectsSameSubtypeWithoutBothZones()
    {
        var report = SerializedRemediationRules.ValidateDeclarations(RuleSet(
            Array.Empty<Rule>(),
            Item("page-header", zoneId: "footer"),
            Item("page-footer")));

        var error = Assert.Single(report.Errors, x => x.Contains("unambiguous", StringComparison.Ordinal));
        Assert.Contains("page-header", error);
        Assert.Contains("page-footer", error);
    }

    [Fact]
    public void DeclarationValidation_AllowsSameSubtypeWhenBothZoned()
    {
        var ruleSet = new RuleSet(
            "set",
            Array.Empty<Rule>(),
            Array.Empty<RemediationAnchor>(),
            new[]
            {
                new TolerancedZone("footer", LayoutCoord.MarginRelative(bottom: 42)),
                new TolerancedZone("header", LayoutCoord.MarginRelative(top: 42))
            },
            artifacts: new[] { Item("page-header", zoneId: "header"), Item("page-footer", zoneId: "footer") });

        Assert.True(SerializedRemediationRules.ValidateDeclarations(ruleSet).IsValid);
    }

    [Fact]
    public void DeclarationValidation_RejectsArtifactBindingWithoutInventory()
    {
        var report = SerializedRemediationRules.ValidateDeclarations(
            new RuleSet("set", FooterArtifactRule() with { Artifact = "page-footer" }));

        Assert.Contains(report.Errors, x => x.Contains("no artifact inventory is declared", StringComparison.Ordinal));
    }

    [Fact]
    public void DeclarationValidation_RejectsSlotAndArtifactOnOneRule()
    {
        var report = SerializedRemediationRules.ValidateDeclarations(RuleSet(
            new[] { FooterArtifactRule() with { Artifact = "page-footer", Slot = "body" } },
            Item("page-footer", zoneId: "footer")));

        Assert.Contains(report.Errors, x => x.Contains("cannot bind both", StringComparison.Ordinal));
    }

    [Fact]
    public void DeclarationValidation_RejectsUnknownItemAndNonArtifactActionAndSubtypeMismatch()
    {
        var report = SerializedRemediationRules.ValidateDeclarations(RuleSet(
            new[]
            {
                FooterArtifactRule("unknown-binding") with { Artifact = "nope" },
                new Rule("tagging", RemediationActions.Tag("P"),
                    candidates: CandidateSelector.Text(Granularity.Line), artifact: "page-footer"),
                FooterArtifactRule("wrong-subtype", ArtifactSubtype.Background) with { Artifact = "page-footer" }
            },
            Item("page-footer", zoneId: "footer")));

        Assert.Contains(report.Errors, x => x.Contains("unknown artifact inventory item", StringComparison.Ordinal));
        Assert.Contains(report.Errors, x => x.Contains("does not produce an artifact", StringComparison.Ordinal));
        Assert.Contains(report.Errors, x => x.Contains("requires 'Pagination'", StringComparison.Ordinal));
    }

    [Fact]
    public void NoInventory_LeavesAbsorbedTextUntypedAndReportsNothing()
    {
        using var document = CreateDocument(("Body text", 90, 700), ("Page 1 of 1", 90, 20));
        using var session = document.BeginRemediation(CommitConfiguration())
            .Use(new RuleSet("set", BodyRule()));

        var report = session.Commit();

        Assert.True(report.Committed);
        Assert.Empty(report.TemplateDifferences);
        var absorbed = Assert.Single(report.AutoArtifacts);
        Assert.Null(absorbed.InventoryItemId);
        Assert.Null(ArtifactSubtypeOf(document.Pages[0]));
    }

    [Fact]
    public void AbsorbedText_InDeclaredZone_AdoptsSubtypeAndRecordsItem()
    {
        using var document = CreateDocument(("Body text", 90, 700), ("Page 1 of 1", 90, 20));
        using var session = document.BeginRemediation(CommitConfiguration())
            .Use(RuleSet(BodyRule(), Item("page-footer", zoneId: "footer")));

        var report = session.Commit();

        Assert.True(report.Committed);
        Assert.Empty(report.TemplateDifferences);
        Assert.Equal("page-footer", Assert.Single(report.AutoArtifacts).InventoryItemId);
        Assert.Equal("Pagination", ArtifactSubtypeOf(document.Pages[0]));
    }

    [Fact]
    public void AbsorbedText_OutsideDeclaredZone_ReportsUndeclared()
    {
        using var document = CreateDocument(("Body text", 90, 700), ("Stray total", 90, 400));
        using var session = document.BeginRemediation(DryRunConfiguration())
            .Use(RuleSet(BodyRule(), Item("page-footer", zoneId: "footer")));

        var report = session.DryRun();

        var difference = Assert.Single(
            report.TemplateDifferences,
            x => x.Kind == RemediationTemplateDifferenceKind.UndeclaredArtifact);
        Assert.Equal(DiagnosticCode.ArtifactUndeclared, difference.DiagnosticCode);
        Assert.Equal("untyped", difference.ActualValue);
        Assert.Contains(report.Diagnostics, x => x.StartsWith("ArtifactUndeclared", StringComparison.Ordinal));
    }

    [Fact]
    public void ZonelessItem_NeverAbsorbsText()
    {
        using var document = CreateDocument(("Body text", 90, 700), ("Page 1 of 1", 90, 20));
        using var session = document.BeginRemediation(DryRunConfiguration())
            .Use(RuleSet(BodyRule(), Item("page-footer")));

        var report = session.DryRun();

        Assert.Null(Assert.Single(report.AutoArtifacts).InventoryItemId);
        Assert.Contains(
            report.TemplateDifferences,
            x => x.Kind == RemediationTemplateDifferenceKind.UndeclaredArtifact);
        Assert.Contains(
            report.TemplateDifferences,
            x => x.Kind == RemediationTemplateDifferenceKind.MissingDeclaredArtifact &&
                 x.SlotId == "page-footer");
    }

    [Fact]
    public void RuleProducedArtifact_OutsideInventory_ReportsUndeclared()
    {
        using var document = CreateDocument(("Body text", 90, 700), ("Watermark", 90, 400));
        using var session = document.BeginRemediation(DryRunConfiguration())
            .Use(RuleSet(
                new[]
                {
                    BodyRule(),
                    new Rule("mark", RemediationActions.Artifact(ArtifactSubtype.Background),
                        Predicates.Text.Contains("Watermark"), CandidateSelector.Text(Granularity.Line))
                },
                Item("page-footer", zoneId: "footer")));

        var report = session.DryRun();

        var difference = Assert.Single(
            report.TemplateDifferences,
            x => x.Kind == RemediationTemplateDifferenceKind.UndeclaredArtifact);
        Assert.Equal("Background", difference.ActualValue);
        Assert.Equal("mark", difference.RuleId);
    }

    [Fact]
    public void DeclaredFurniture_AbsentWhereRequired_ReportsMissing()
    {
        using var document = CreateDocument(("Body text", 90, 700));
        using var session = document.BeginRemediation(DryRunConfiguration())
            .Use(RuleSet(BodyRule(), Item("page-footer", zoneId: "footer")));

        var report = session.DryRun();

        var difference = Assert.Single(report.TemplateDifferences);
        Assert.Equal(RemediationTemplateDifferenceKind.MissingDeclaredArtifact, difference.Kind);
        Assert.Equal("Artifact:page-footer", difference.ExpectedPath);
        Assert.Equal("exactly 1", difference.ExpectedValue);
        Assert.Equal("0", difference.ActualValue);
    }

    [Fact]
    public void DeclaredFurniture_LegitimatelyAbsent_IsAccepted()
    {
        using var document = CreateDocument(("Body text", 90, 700));
        using var session = document.BeginRemediation(DryRunConfiguration())
            .Use(RuleSet(BodyRule(), Item("page-footer", zoneId: "footer", occurrence: new AssertionCount(0, 1))));

        Assert.Empty(session.DryRun().TemplateDifferences);
    }

    [Fact]
    public void FirstPageFurniture_IsNotExpectedOnLaterPages()
    {
        using var document = PdfDocument.Create();
        WritePage(document, ("Body text", 90, 700), ("Page 1 of 2", 90, 20));
        WritePage(document, ("Body text", 90, 700));
        using var session = document.BeginRemediation(DryRunConfiguration())
            .Use(RuleSet(BodyRule(), Item("letterhead", zoneId: "footer", pages: PageSelector.First)));

        Assert.Empty(session.DryRun().TemplateDifferences);
    }

    [Fact]
    public void RunawayZone_ReportsOccurrenceViolation()
    {
        using var document = CreateDocument(
            ("Body text", 90, 700), ("Page 1 of 1", 90, 20), ("Confidential", 300, 20));
        using var session = document.BeginRemediation(DryRunConfiguration())
            .Use(RuleSet(BodyRule(), Item("page-footer", zoneId: "footer")));

        var report = session.DryRun();

        var difference = Assert.Single(report.TemplateDifferences);
        Assert.Equal(RemediationTemplateDifferenceKind.ArtifactOccurrenceViolation, difference.Kind);
        Assert.Equal("page-footer", difference.SlotId);
        Assert.Equal("2", difference.ActualValue);
    }

    [Fact]
    public void UnionComposition_HonorsItemFromSecondRuleSet()
    {
        using var document = CreateDocument(("Body text", 90, 700), ("Page 1 of 1", 90, 20));
        using var session = document.BeginRemediation(DryRunConfiguration()).Use(
            new RuleSet(
                "boilerplate",
                Array.Empty<Rule>(),
                Array.Empty<RemediationAnchor>(),
                new[] { new TolerancedZone("footer", LayoutCoord.MarginRelative(bottom: 42), Tolerance: 6) },
                artifacts: new[] { Item("page-footer", zoneId: "footer") }),
            new RuleSet("invoice", BodyRule()));

        var report = session.DryRun();

        Assert.Empty(report.TemplateDifferences);
        Assert.Equal("page-footer", Assert.Single(report.AutoArtifacts).InventoryItemId);
    }

    [Theory]
    [InlineData(DiagnosticCode.ArtifactUndeclared, "Artifact:Undeclared:Page1")]
    [InlineData(DiagnosticCode.ArtifactMissingDeclared, "Artifact:page-footer:Page1")]
    public void ArtifactDiagnostics_SuppressIndependentlyAtTheirOwnScope(DiagnosticCode code, string scope)
    {
        using var document = CreateDocument(("Body text", 90, 700), ("Stray total", 90, 400));
        using var session = document.BeginRemediation(new RemediationSessionConfiguration
        {
            StrictConformance = false,
            LeftoverPolicy = RemediationLeftoverPolicy.AutoArtifact,
            DiagnosticStrictness = RemediationDiagnosticStrictness.Permissive
        })
            .Use(RuleSet(BodyRule(), Item("page-footer", zoneId: "footer")))
            .Suppress(code, scope, "Reviewed variant.");

        var report = session.DryRun();

        Assert.Contains(report.TemplateDifferences, x => x.DiagnosticCode == code && x.Suppressed);
        Assert.Contains(report.TemplateDifferences, x => x.DiagnosticCode != code && !x.Suppressed);
        Assert.Contains(report.Diagnostics, x => x.StartsWith($"[SUPPRESSED] {code}", StringComparison.Ordinal));
    }

    [Fact]
    public void ArtifactSuppression_IsIgnoredUnderStrictStrictness()
    {
        using var document = CreateDocument(("Body text", 90, 700));
        using var session = document.BeginRemediation(new RemediationSessionConfiguration
        {
            StrictConformance = false,
            LeftoverPolicy = RemediationLeftoverPolicy.AutoArtifact,
            DiagnosticStrictness = RemediationDiagnosticStrictness.Strict
        })
            .Use(RuleSet(BodyRule(), Item("page-footer", zoneId: "footer")))
            .Suppress(DiagnosticCode.ArtifactMissingDeclared, "*", "Reviewed variant.");

        var report = session.DryRun();

        Assert.False(Assert.Single(report.TemplateDifferences).Suppressed);
        Assert.Contains(report.Diagnostics, x => x.StartsWith("[IGNORED-SUPPRESSION]", StringComparison.Ordinal));
    }

    private static RemediationArtifactInventoryItem Item(
        string id,
        ArtifactSubtype subtype = ArtifactSubtype.Pagination,
        PageSelector? pages = null,
        string? zoneId = null,
        AssertionCount? occurrence = null) => new(id, subtype, pages, zoneId, occurrence);

    private static Rule BodyRule() =>
        new("body", RemediationActions.Tag("P"), Predicates.Text.Contains("Body"),
            CandidateSelector.Text(Granularity.Line));

    private static Rule FooterArtifactRule(
        string id = "footer",
        ArtifactSubtype subtype = ArtifactSubtype.Pagination) =>
        new(id, RemediationActions.Artifact(subtype), candidates: CandidateSelector.Text(Granularity.Line));

    private static RuleSet RuleSet(Rule rule, params RemediationArtifactInventoryItem[] items) =>
        RuleSet(new[] { rule }, items);

    private static RuleSet RuleSet(Rule[] rules, params RemediationArtifactInventoryItem[] items) =>
        new(
            "set",
            rules,
            Array.Empty<RemediationAnchor>(),
            new[] { new TolerancedZone("footer", LayoutCoord.MarginRelative(bottom: 42), Tolerance: 6) },
            artifacts: items);

    private static RemediationSessionConfiguration DryRunConfiguration() => new()
    {
        StrictConformance = false,
        LeftoverPolicy = RemediationLeftoverPolicy.AutoArtifact
    };

    private static RemediationSessionConfiguration CommitConfiguration() => new()
    {
        Language = "en-US",
        Title = "Artifact inventory",
        StrictConformance = false,
        LeftoverPolicy = RemediationLeftoverPolicy.AutoArtifact
    };

    private static PdfDocument CreateDocument(params (string Text, double X, double Y)[] items)
    {
        var document = PdfDocument.Create();
        WritePage(document, items);
        return document;
    }

    private static void WritePage(PdfDocument document, params (string Text, double X, double Y)[] items)
    {
        var page = document.AddPage(PageSize.LETTER);
        using var writer = page.GetWriter();
        foreach (var item in items)
        {
            writer.Save().Font(Standard14Font.GetHelvetica(), 12)
                .TextMove(item.X, item.Y).Text(item.Text).EndText().Restore();
        }
    }

    /// <summary>Subtype on the artifact wrapper emitted for absorbed content, or null when untyped.</summary>
    private static string? ArtifactSubtypeOf(PdfPage page) => page.GetContentNodes<double>()
        .OfType<MarkedContentGroup<double>>()
        .Where(x => x.Tag.Name == PdfName.Artifact)
        .Select(x => x.Tag.InlineProps?.Get<PdfName>(PdfName.TYPE)?.Value)
        .FirstOrDefault();
}
