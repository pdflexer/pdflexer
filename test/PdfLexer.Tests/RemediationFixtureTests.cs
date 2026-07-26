using System.IO;
using System.Linq;
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

    private static PdfName[] GetChildTypes(PdfDictionary parent) =>
        parent.Get<PdfArray>(PdfName.K)!
            .Select(x => x.Resolve())
            .OfType<PdfDictionary>()
            .Select(x => x.Get<PdfName>(PdfName.S)!)
            .ToArray();

}
