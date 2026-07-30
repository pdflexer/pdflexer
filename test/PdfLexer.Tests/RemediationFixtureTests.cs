using System.IO;
using System.Linq;
using pdflexer.PdfiumRegressionTester;
using PdfLexer.Content.Model;
using PdfLexer.DOM;
using Xunit;

namespace PdfLexer.Tests;

public class RemediationFixtureTests
{
    [Fact]
    public void StrictInvoiceTableFixtureCommitsAndSavesAsPdfUa1()
    {
        var fixture = RemediationFixtureGenerator.GenerateStrictInvoiceUa1();

        Assert.True(fixture.Report.Committed);
        Assert.Empty(fixture.Report.TemplateDifferences);
        Assert.All(fixture.Report.AutoArtifacts, x => Assert.NotNull(x.InventoryItemId));
        using var document = PdfDocument.Open(fixture.Bytes);
        AccessibilityIntegrityAssert.HasDocumentSetup(document, PdfUaProfile.PdfUa1);
        AccessibilityIntegrityAssert.HasBasicStructureIntegrity(document);
        AccessibilityIntegrityAssert.HasValidTableAndListHierarchy(document);
    }

    [Fact]
    public void Remediation_Fixture_Generator_Writes_Input_And_Remediated_Ua1_Ua2_Corpus()
    {
        var fixtures = RemediationFixtureGenerator.GenerateAll();

        var expectedBaseNames = new[]
        {
            "invoice-like",
            "statement-like",
            "report-like",
            "form-like",
            "multi-column-sidebar",
            "mixed-page-sizes"
        };
        var expectedFileNames = expectedBaseNames.SelectMany(x => new[]
        {
            RemediationFixtureGenerator.GetFixtureFileName(x, PdfUaProfile.PdfUa1),
            RemediationFixtureGenerator.GetFixtureFileName(x, PdfUaProfile.PdfUa2)
        });

        Assert.Equal(expectedFileNames.OrderBy(x => x), fixtures.Select(x => x.FileName).OrderBy(x => x));
        Assert.Equal(6, fixtures.Select(x => x.InputPath).Distinct().Count());
        Assert.All(fixtures.Select(x => x.InputPath).Distinct(), path => Assert.True(File.Exists(path), path));

        foreach (var fixture in fixtures)
        {
            Assert.True(File.Exists(fixture.Path), fixture.Path);
            Assert.True(fixture.Report.Committed);
            Assert.Empty(fixture.Report.Diagnostics.Where(x => !x.StartsWith("[SUPPRESSED]")));
            Assert.Empty(fixture.Report.TemplateDifferences);
            Assert.All(fixture.Report.AutoArtifacts, x => Assert.NotNull(x.InventoryItemId));

            using var document = PdfDocument.Open(fixture.Bytes);
            AccessibilityIntegrityAssert.HasDocumentSetup(document, fixture.Profile);
            AccessibilityIntegrityAssert.HasBasicStructureIntegrity(document);
            AccessibilityIntegrityAssert.HasOnlyTaggedOrArtifactContent(document);
            if (fixture.Profile == PdfUaProfile.PdfUa2)
            {
                AccessibilityIntegrityAssert.HasPdf20RootNamespace(document);
            }

            Assert.Contains(
                AccessibilityIntegrityAssert.GetStructureElements(document),
                x => x.Get<PdfString>(PdfName.T)?.Value == "body-line");
        }
    }

    [Fact]
    public void Remediation_Fixtures_Capture_Expected_Anchor_Flow_And_Debug_Provenance()
    {
        var fixtures = RemediationFixtureGenerator.GenerateAll();

        var invoice = Assert.Single(fixtures, x => x.FileName == "invoice-like-ua2.pdf");
        Assert.Contains(invoice.Report.Claims, x => x.RuleId == "invoice-number");
        Assert.Equal(3, invoice.Report.Claims.Count(x => x.RuleId == "line-item-header-cell"));
        Assert.Equal(6, invoice.Report.Claims.Count(x => x.RuleId == "line-item-cell"));
        Assert.Equal(
            new[] { "line-items-header-spacing", "line-items-header-spacing" },
            invoice.Report.AutoArtifacts.Select(x => x.InventoryItemId).ToArray());

        using (var invoiceDocument = PdfDocument.Open(invoice.Bytes))
        {
            Assert.Contains(
                AccessibilityIntegrityAssert.GetStructureElements(invoiceDocument),
                x => x.Get<PdfString>(PdfName.T)?.Value == "invoice-number");
            AccessibilityIntegrityAssert.HasValidTableAndListHierarchy(invoiceDocument);
            var table = Assert.Single(
                AccessibilityIntegrityAssert.GetStructureElements(invoiceDocument),
                x => x.Get<PdfName>(PdfName.S) == PdfName.Table);
            var rows = table.Get<PdfArray>(PdfName.K)!
                .Select(x => x.Resolve())
                .OfType<PdfDictionary>()
                .ToList();
            Assert.Equal(3, rows.Count);
            Assert.Equal(new[] { (PdfName)"TH", (PdfName)"TH", (PdfName)"TH" }, GetChildTypes(rows[0]));
            Assert.Equal(new[] { (PdfName)"TD", (PdfName)"TD", (PdfName)"TD" }, GetChildTypes(rows[1]));
            Assert.Equal(new[] { (PdfName)"TD", (PdfName)"TD", (PdfName)"TD" }, GetChildTypes(rows[2]));
            Assert.Contains(invoiceDocument.Pages.Select(x => x.DumpDecodedContents()), x => x.Contains("/Artifact"));
        }

        var statement = Assert.Single(fixtures, x => x.FileName == "statement-like-ua2.pdf");
        Assert.Equal(3, statement.Report.Claims.Count(x => x.RuleId == "bill-to-address"));

        var mixed = Assert.Single(fixtures, x => x.FileName == "mixed-page-sizes-ua2.pdf");
        using var mixedDocument = PdfDocument.Open(mixed.Bytes);
        Assert.Equal(2, mixedDocument.Pages.Count);
        Assert.All(mixedDocument.Pages, page => Assert.Contains("/Artifact", page.DumpDecodedContents()));
    }

    [Fact]
    public void Remediation_Fixtures_Are_Visually_And_Glyph_Position_Invariant()
    {
        var fixtures = RemediationFixtureGenerator.GenerateAll();
        var diffRoot = Path.Combine(RemediationFixtureGenerator.FixtureRootPath, "visual-diffs");
        Directory.CreateDirectory(diffRoot);

        foreach (var fixture in fixtures)
        {
            var diffPrefix = Path.Combine(diffRoot, Path.GetFileNameWithoutExtension(fixture.FileName));
            var comparison = new Compare(diffPrefix).CompareAllPages(
                fixture.InputPath,
                fixture.Path,
                CompareMode.Exact);

            Assert.All(
                comparison,
                result => Assert.False(
                    result.HadChanges,
                    $"{fixture.FileName}: {result.Type}; {result.Error}; diff={result.DiffImage}"));

            using var baseline = PdfDocument.Open(fixture.InputPath);
            using var candidate = PdfDocument.Open(fixture.Path);
            Assert.Equal(baseline.Pages.Count, candidate.Pages.Count);
            for (var pageIndex = 0; pageIndex < baseline.Pages.Count; pageIndex++)
            {
                var baselineText = baseline.Pages[pageIndex]
                    .GetContentModel()
                    .Flatten()
                    .OfType<TextContent<double>>()
                    .ToArray();
                var candidateText = candidate.Pages[pageIndex]
                    .GetContentModel()
                    .Flatten()
                    .OfType<TextContent<double>>()
                    .ToArray();
                var baselineGlyphs = baselineText.SelectMany(x => x.EnumerateCharacters()).ToArray();
                var candidateGlyphs = candidateText.SelectMany(x => x.EnumerateCharacters()).ToArray();
                var baselineBoxes = baselineText.SelectMany(x => x.GetGlyphBoundingBoxes()).ToArray();
                var candidateBoxes = candidateText.SelectMany(x => x.GetGlyphBoundingBoxes()).ToArray();

                Assert.Equal(baselineGlyphs.Select(x => x.Char), candidateGlyphs.Select(x => x.Char));
                Assert.Equal(baselineGlyphs.Length, candidateGlyphs.Length);
                for (var glyphIndex = 0; glyphIndex < baselineGlyphs.Length; glyphIndex++)
                {
                    Assert.InRange(
                        candidateGlyphs[glyphIndex].XPos,
                        baselineGlyphs[glyphIndex].XPos - 0.000001,
                        baselineGlyphs[glyphIndex].XPos + 0.000001);
                    Assert.InRange(
                        candidateGlyphs[glyphIndex].YPos,
                        baselineGlyphs[glyphIndex].YPos - 0.000001,
                        baselineGlyphs[glyphIndex].YPos + 0.000001);
                }

                Assert.Equal(baselineBoxes.Length, candidateBoxes.Length);
                for (var glyphIndex = 0; glyphIndex < baselineBoxes.Length; glyphIndex++)
                {
                    AssertCoordinateEqual(baselineBoxes[glyphIndex].LLx, candidateBoxes[glyphIndex].LLx);
                    AssertCoordinateEqual(baselineBoxes[glyphIndex].LLy, candidateBoxes[glyphIndex].LLy);
                    AssertCoordinateEqual(baselineBoxes[glyphIndex].URx, candidateBoxes[glyphIndex].URx);
                    AssertCoordinateEqual(baselineBoxes[glyphIndex].URy, candidateBoxes[glyphIndex].URy);
                }
            }
        }
    }

    private static void AssertCoordinateEqual(double expected, double actual) =>
        Assert.InRange(actual, expected - 0.000001, expected + 0.000001);

    private static PdfName[] GetChildTypes(PdfDictionary parent) =>
        parent.Get<PdfArray>(PdfName.K)!
            .Select(x => x.Resolve())
            .OfType<PdfDictionary>()
            .Select(x => x.Get<PdfName>(PdfName.S)!)
            .ToArray();

}
