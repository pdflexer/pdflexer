using System;
using System.IO;
using System.Linq;
using PdfLexer.DOM;
using PdfLexer.Fonts;
using PdfLexer.Remediation;
using PdfLexer.Writing;
using Xunit;

namespace PdfLexer.Tests;

public sealed class RemediationRegionTests
{
    [Fact]
    public void PreviewRegionSerializationIsByteStable()
    {
        var program = CreateProgram();
        using var first = new MemoryStream();
        SerializedRemediationProgram.Save(program, first);
        first.Position = 0;
        var loaded = SerializedRemediationProgram.Load(first);
        using var second = new MemoryStream();
        SerializedRemediationProgram.Save(loaded, second);

        Assert.Equal(first.ToArray(), second.ToArray());
        var tolerance = Assert.IsType<Region.Tolerance>(Assert.Single(loaded.Regions).Expression);
        Assert.IsType<Region.Fixed>(tolerance.Inner);
        Assert.Equal("footer", Assert.Single(loaded.RegionAccounting).RegionId);
    }

    [Fact]
    public void CompilerRejectsUnrestrictedTextAccounting()
    {
        var valid = CreateProgram();
        var program = new RemediationProgram(valid.Id, valid.Template, valid.Bindings,
            artifacts: new[] { new ArtifactDeclaration("pagination", ArtifactSubtype.Pagination, occurrence: new AssertionCount(0)) }, regions: valid.Regions,
            regionAccounting: new[]
            {
                new RegionArtifactAccounting("unsafe", "footer", "pagination", allowText: true)
            });

        var compiled = RemediationProgramCompiler.Compile(program);

        Assert.False(compiled.IsValid);
        Assert.Contains(compiled.Diagnostics, x => x.Message.Contains("must bound allowed text", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void FooterTextIsAbsorbedAsDeclaredPaginationArtifact()
    {
        using var document = PdfDocument.Create();
        var page = document.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
            writer.Font(Standard14Font.GetHelvetica(), 10)
                .TextMove(280, 24).Text("Page 1 of 1").EndText();
        using var session = document.BeginRemediation(new RemediationSessionConfiguration
        {
            StrictConformance = false
        });
        session.Use(CreateProgram());

        var report = session.DryRun();

        Assert.True(report.RegionAbsorptions.Count == 1, string.Join(Environment.NewLine, report.Diagnostics.Concat(report.RegionResolutions.Select(x => x.Bounds.ToString())).Concat(report.UnaccountedContent.Select(x => x.RelativeBoundingBox.ToString()))));
        var absorption = report.RegionAbsorptions[0];
        Assert.Equal("pagination", absorption.ArtifactId);
        Assert.Equal(ArtifactSubtype.Pagination, absorption.Subtype);
        Assert.Empty(report.UnaccountedContent);
        Assert.Contains(report.Claims, x => x.ArtifactId == "pagination" && x.Action is ArtifactRemediationAction);
    }

    [Fact]
    public void TextFailingAccountingGuardRemainsBlockingLeftover()
    {
        using var document = CreateTextDocument("Confidential total: 42");
        using var session = document.BeginRemediation(new RemediationSessionConfiguration
        {
            StrictConformance = false
        }).Use(CreateProgram());
        var report = session.DryRun();
        Assert.Empty(report.RegionAbsorptions);
        Assert.Single(report.UnaccountedContent);
        Assert.Contains(report.RuntimeDiagnostics, x =>
            x.Code == DiagnosticCode.PrescriptiveUnaccountedContent && x.IsBlocking);
    }

    [Fact]
    public void EqualPriorityAccountingIsAmbiguousAndCannotBeSuppressed()
    {
        var baseline = CreateProgram();
        var program = new RemediationProgram(baseline.Id, baseline.Template, baseline.Bindings,
            artifacts: baseline.Artifacts, regions: baseline.Regions,
            regionAccounting: new[]
            {
                new RegionArtifactAccounting("first", "footer", "pagination", allowText: true,
                    textPredicate: Predicates.Text.StartsWith("Page "), priority: 10),
                new RegionArtifactAccounting("second", "footer", "pagination", allowText: true,
                    textPredicate: Predicates.Text.StartsWith("Page "), priority: 10)
            });
        using var document = CreateTextDocument("Page 1 of 1");
        using var session = document.BeginRemediation(new RemediationSessionConfiguration
        {
            StrictConformance = false,
            DiagnosticStrictness = RemediationDiagnosticStrictness.Permissive
        }).Use(program);
        session.Suppress(DiagnosticCode.ProgramRegionAccountingAmbiguous,
            "Program:region-program:RegionAccounting", "probe");
        var report = session.DryRun();
        Assert.Empty(report.RegionAbsorptions);
        Assert.Contains(report.RuntimeDiagnostics, x =>
            x.Code == DiagnosticCode.ProgramRegionAccountingAmbiguous &&
            x.Disposition == RemediationDiagnosticDisposition.Error);
        Assert.Contains(report.RuntimeDiagnostics, x =>
            x.Code == DiagnosticCode.PrescriptiveUnaccountedContent && x.IsBlocking);
    }

    [Fact]
    public void StructuralOwnershipConflictingWithAccountingIsBlocking()
    {
        var baseline = CreateProgram();
        var program = new RemediationProgram(baseline.Id,
            new RemediationTemplate("regions", "1", PdfUaProfile.PdfUa1,
                new RemediationTemplateNode("Document", children: new[]
                {
                    new RemediationTemplateNode("P", "page")
                })),
            new[]
            {
                new BindingRule("page", BindingTarget.ToSlot(SlotRef.Absolute("/page")),
                    CandidateSelector.Text(Granularity.Line), Predicates.Text.StartsWith("Page "))
            }, artifacts: baseline.Artifacts, regions: baseline.Regions,
            regionAccounting: baseline.RegionAccounting);
        using var document = CreateTextDocument("Page 1 of 1");
        using var session = document.BeginRemediation(new RemediationSessionConfiguration
        {
            StrictConformance = false
        }).Use(program);
        var report = session.DryRun();
        Assert.Empty(report.RegionAbsorptions);
        Assert.Contains(report.RuntimeDiagnostics, x =>
            x.Code == DiagnosticCode.ProgramRegionAccountingConflict && x.IsBlocking);
    }

    [Fact]
    public void AnchoredRegionResolvesFromTextAnchor()
    {
        var program = EmptyProgram(
            anchors: new[] { RemediationAnchor.TextLabel("label", "Anchor") },
            regions: new[]
            {
                new RegionDeclaration("around-label",
                    new Region.Anchored("label", RegionPlacement.Around, 8))
            });
        using var document = CreateTextDocument("Anchor");
        using var session = document.BeginRemediation(new RemediationSessionConfiguration
        {
            StrictConformance = false
        }).Use(program);
        var resolution = Assert.Single(session.DryRun().RegionResolutions);
        Assert.Equal("around-label", resolution.RegionId);
        Assert.True(resolution.Bounds.URx > resolution.Bounds.LLx);
        Assert.True(resolution.Bounds.URy > resolution.Bounds.LLy);
    }

    [Fact]
    public void FlowRegionSupportsTwoActivationsOnOnePage()
    {
        var program = EmptyProgram(regions: new[]
        {
            new RegionDeclaration("sections", new Region.Flow(
                new RegionBoundary.Matching(CandidateSelector.Text(Granularity.Line),
                    Predicates.Text.StartsWith("Start")),
                new RegionBoundary.Matching(CandidateSelector.Text(Granularity.Line),
                    Predicates.Text.StartsWith("End"))))
        });
        using var document = PdfDocument.Create();
        var page = document.AddPage(PageSize.LETTER);
        WriteLine(700, "Start A");
        WriteLine(680, "Inside A");
        WriteLine(660, "End A");
        WriteLine(620, "Start B");
        WriteLine(600, "Inside B");
        WriteLine(580, "End B");

        void WriteLine(double y, string text)
        {
            using var writer = page.GetWriter();
            writer.Font(Standard14Font.GetHelvetica(), 10)
                .TextMove(50, y).Text(text).EndText();
        }
        using var session = document.BeginRemediation(new RemediationSessionConfiguration
        {
            StrictConformance = false
        }).Use(program);
        var report = session.DryRun();
        var resolutions = report.RegionResolutions;
        Assert.True(resolutions.Count == 2, string.Join(Environment.NewLine, report.Diagnostics));
        Assert.Equal(2, resolutions.Select(x => x.FlowActivationIdentity).Distinct().Count());
    }

    [Fact]
    public void HigherPriorityAccountingWinsOverlap()
    {
        var baseline = CreateProgram();
        var program = new RemediationProgram(baseline.Id, baseline.Template, baseline.Bindings,
            artifacts: baseline.Artifacts, regions: baseline.Regions,
            regionAccounting: new[]
            {
                new RegionArtifactAccounting("low", "footer", "pagination", allowText: true,
                    textPredicate: Predicates.Text.StartsWith("Page "), priority: 10),
                new RegionArtifactAccounting("high", "footer", "pagination", allowText: true,
                    textPredicate: Predicates.Text.StartsWith("Page "), priority: 20)
            });
        using var document = CreateTextDocument("Page 1 of 1");
        using var session = document.BeginRemediation(new RemediationSessionConfiguration
        {
            StrictConformance = false
        }).Use(program);
        var absorption = Assert.Single(session.DryRun().RegionAbsorptions);
        Assert.Equal("high", absorption.AccountingId);
    }

    [Fact]
    public void FixedRegionUsesRotatedCropBoxNormalizedSpace()
    {
        using var document = PdfDocument.Create();
        var page = document.AddPage();
        page.CropBox = new PdfRectangle(new PdfArray { 50, 100, 250, 400 });
        page.Rotate = 90;
        var program = EmptyProgram(regions: new[]
        {
            new RegionDeclaration("footer",
                new Region.Fixed(LayoutCoord.Zone(NamedLayoutZone.Footer)))
        });
        using var session = document.BeginRemediation(new RemediationSessionConfiguration
        {
            StrictConformance = false
        }).Use(program);
        var resolution = Assert.Single(session.DryRun().RegionResolutions);
        Assert.Equal(0, resolution.Bounds.LLx, 6);
        Assert.Equal(0, resolution.Bounds.LLy, 6);
        Assert.Equal(300, resolution.Bounds.URx, 6);
        Assert.Equal(72, resolution.Bounds.URy, 6);
    }

    private static PdfDocument CreateTextDocument(string text)
    {
        var document = PdfDocument.Create();
        var page = document.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
            writer.Font(Standard14Font.GetHelvetica(), 10)
                .TextMove(280, 24).Text(text).EndText();
        return document;
    }

    private static RemediationProgram EmptyProgram(
        RemediationAnchor[] anchors = null,
        RegionDeclaration[] regions = null) => new(
        "region-resolution",
        new RemediationTemplate("regions", "1", PdfUaProfile.PdfUa1,
            new RemediationTemplateNode("Document")),
        Array.Empty<BindingRule>(), anchors: anchors, regions: regions);

    private static RemediationProgram CreateProgram() => new(
        "region-program",
        new RemediationTemplate("regions", "1", PdfUaProfile.PdfUa1,
            new RemediationTemplateNode("Document")),
        Array.Empty<BindingRule>(),
        artifacts: new[]
        {
            new ArtifactDeclaration("pagination", ArtifactSubtype.Pagination,
                semanticSubtype: ArtifactSemanticSubtype.Footer)
        },
        regions: new[]
        {
            new RegionDeclaration("footer",
                new Region.Tolerance(new Region.Fixed(LayoutCoord.Zone(NamedLayoutZone.Footer)), 6))
        },
        regionAccounting: new[]
        {
            new RegionArtifactAccounting("footer-pages", "footer", "pagination",
                allowText: true,
                textPredicate: Predicates.Text.Matches("^Page [0-9]+( of [0-9]+)?$"),
                priority: 100)
        });
}
