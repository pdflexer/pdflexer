using System.IO;
using System.Linq;
using PdfLexer.DOM;
using PdfLexer.Fonts;
using Xunit;

namespace PdfLexer.Tests;

[Collection(VeraPdfTestCollection.Name)]
public class RemediationVeraPdfTests
{
    [VeraPdfFact]
    public void CorrectedStrictInvoiceTablePassesPdfUa1()
    {
        var fixture = RemediationFixtureGenerator.GenerateStrictInvoiceUa1();

        var result = VeraPdfValidation.Validate(fixture.Bytes, PdfUaProfile.PdfUa1);

        Assert.False(result.ProcessingFailed, result.StandardError);
        Assert.True(result.IsCompliant, result.Report);
    }

    public static TheoryData<string> CommittedFixtureNames()
    {
        var data = new TheoryData<string>();
        foreach (var fixture in RemediationFixtureGenerator.GenerateAll()
                     .Where(x => x.Outcome != FixtureOutcome.DiagnosesOnly))
        {
            data.Add(fixture.FileName);
        }

        return data;
    }

    [VeraPdfTheory]
    [MemberData(nameof(CommittedFixtureNames))]
    public void EveryCommittedFixtureValidatesForItsProfile(string fileName)
    {
        var fixture = Assert.Single(RemediationFixtureGenerator.GenerateAll(), x => x.FileName == fileName);

        var result = VeraPdfValidation.Validate(fixture.Bytes, fixture.Profile);

        Assert.False(result.ProcessingFailed, result.StandardError);
        Assert.True(result.IsCompliant, result.Report);
    }

    [VeraPdfFact]
    public void InvalidTdTrHierarchyFailsPdfUa1Baseline()
    {
        var pdf = CreateInvalidTableHierarchy();

        var result = VeraPdfValidation.Validate(pdf, PdfUaProfile.PdfUa1);

        Assert.False(result.ProcessingFailed, result.StandardError);
        Assert.False(result.IsCompliant, result.Report);
    }

    private static byte[] CreateInvalidTableHierarchy()
    {
        using var document = PdfDocument.Create();
        var page = document.AddPage(PageSize.LETTER);
        document.ApplyAccessibilitySetup(
            "en-US",
            "Intentional invalid table baseline",
            PdfUaProfile.PdfUa1,
            strictConformance: false);

        var table = document.Structure.AddTable();
        var outerCell = table.AddElement("TD");
        var invalidRow = outerCell.AddElement("TR");
        var innerCell = invalidRow.AddElement("TD");

        var testDir = PathUtil.GetPathFromSegmentOfCurrent("test");
        var font = TrueTypeFont.CreateWritableFont(
            File.ReadAllBytes(Path.Combine(testDir, "Roboto-Regular.ttf")));
        using (var writer = page.GetWriter())
        {
            writer.BeginMarkedContent(innerCell.GetNode());
            writer.Font(font, 12).TextMove(72, 700).Text("Invalid hierarchy");
            writer.EndMarkedContent();
        }

        return document.Save();
    }
}
