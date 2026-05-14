using PdfLexer.Content;
using PdfLexer.DOM;
using PdfLexer.Fonts;
using PdfLexer.Images;
using PdfLexer.Writing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Linq;
using Xunit;

namespace PdfLexer.Tests;

public class AccessibilityEndToEndTests
{
    private readonly IWritableFont font;

    public AccessibilityEndToEndTests()
    {
        var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
        var fontPath = System.IO.Path.Combine(tp, "Roboto-Regular.ttf");
        font = TrueTypeFont.CreateWritableFont(System.IO.File.ReadAllBytes(fontPath));
    }

    [Fact]
    public void EndToEnd_New_Document_Accessibility_Flow_RoundTrips_And_Is_Present_In_Fixture_Corpus()
    {
        using var image = new Image<Rgba32>(4, 4);
        image.Mutate(x => x.BackgroundColor(Color.Black));
        var pdfImage = image.CreatePdfImage();

        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        doc.ApplyAccessibilitySetup("en-US", "Accessible Report", PdfUaProfile.PdfUa2, strictConformance: true);

        var section = doc.Structure.AddSection("Quarterly accessibility report");
        var heading = section.AddHeader(1, "Accessible Report");
        var paragraph = heading.Back().AddParagraph("Summary").ActualText("Summary of the quarterly accessibility report");
        var table = paragraph.Back().AddTable("Quarterly results").TableSummary("Quarterly revenue by region");
        var headerRow = table.AddRow();
        var headerCell = headerRow.AddHeaderCell().ElementId("results-header").TableScope(StructureScope.Column);
        var dataCell = headerCell.Back().AddDataCell().TableHeaders("results-header");
        var figure = dataCell.Back().Back().Back().AddFigure("Revenue chart", "Bar chart showing quarterly revenue");
        var details = figure.Back().AddSection("Chart details");
        var detailsHeading = details.AddHeader(2, "Chart details");
        var link = details.Back().AddLink(
            page,
            new PdfRect<double>(40, 645, 180, 662),
            detailsHeading.GetNode(),
            "Jump to chart details",
            "Jump to chart details",
            "Jump to chart details");

        using (var writer = page.GetWriter())
        {
            writer.BeginMarkedContent(heading.GetNode());
            writer.Font(font, 16).TextMove(40, 760).Text("Accessible Report");
            writer.EndMarkedContent();

            writer.BeginMarkedContent(paragraph.GetNode());
            writer.Font(font, 12).TextMove(40, 738).Text("Summary");
            writer.EndMarkedContent();

            writer.BeginMarkedContent(headerCell.GetNode());
            writer.Font(font, 12).TextMove(40, 712).Text("Region");
            writer.EndMarkedContent();

            writer.BeginMarkedContent(dataCell.GetNode());
            writer.Font(font, 12).TextMove(120, 712).Text("North");
            writer.EndMarkedContent();

            writer.BeginMarkedContent(link.GetNode());
            writer.Font(font, 12).TextMove(40, 650).Text("Jump to chart details");
            writer.EndMarkedContent();

            writer.BeginMarkedContent(detailsHeading.GetNode());
            writer.Font(font, 12).TextMove(40, 620).Text("Chart details");
            writer.EndMarkedContent();
        }

        figure.BindImage(pdfImage, page);
        using (var writer = page.GetWriter())
        {
            writer.Image(pdfImage, 40, 580, 48, 48);
        }

        var bytes = doc.Save();
        using var saved = PdfDocument.Open(bytes);

        AccessibilityIntegrityAssert.HasDocumentSetup(saved, PdfUaProfile.PdfUa2);
        AccessibilityIntegrityAssert.HasBasicStructureIntegrity(saved);
        AccessibilityIntegrityAssert.HasPdf20RootNamespace(saved);
        AccessibilityIntegrityAssert.HasStructureDestinationLinks(saved);
        AccessibilityIntegrityAssert.HasAccessibleLinkDescriptions(saved);
        AccessibilityIntegrityAssert.HasOnlyTaggedOrArtifactContent(saved);
        var extracted = AccessibilityIntegrityAssert.ExtractTextWithPdfPig(bytes);
        Assert.Contains("Accessible Report", extracted);
        Assert.Contains("Jump to chart details", extracted);

        var allStructElems = AccessibilityIntegrityAssert.GetStructureElements(saved);
        Assert.Contains(allStructElems, x => x.Get<PdfName>(PdfName.S) == PdfName.Table);
        Assert.Contains(allStructElems, x => x.Get<PdfName>(PdfName.S) == PdfName.Figure && x.Get<PdfString>(PdfName.Alt)?.Value == "Bar chart showing quarterly revenue");

        var annotation = saved.Pages[0].NativeObject.Get<PdfArray>(PdfName.Annots)![0].Resolve().GetAs<PdfDictionary>();
        Assert.True(annotation.ContainsKey(PdfName.StructParent));

        var imageXObject = AccessibilityIntegrityAssert.GetFirstXObject(saved.Pages[0], PdfName.Image);
        Assert.True(imageXObject.Dictionary.ContainsKey(PdfName.StructParent));

        var fixtures = AccessibilityFixtureGenerator.GenerateAll();
        Assert.Contains(fixtures, x => x.FileName == AccessibilityFixtureGenerator.AccessibleReportFixtureFileName);
    }

    [Fact]
    public void EndToEnd_Retagged_Document_Preserves_Navigation_And_Is_Present_In_Fixture_Corpus()
    {
        using var source = PdfDocument.Create();
        source.AddPage();
        var chapterPage = source.AddPage();
        var destination = new PdfArray { chapterPage.NativeObject.Indirect(), PdfName.XYZ, 0, 700, PdfNull.Value };
        source.Catalog[PdfName.Names] = new PdfDictionary
        {
            [PdfName.Dests] = new PdfDictionary
            {
                [PdfName.Names] = new PdfArray
                {
                    new PdfString("chapter-1"),
                    destination
                }
            }
        };
        using var reopened = PdfDocument.Open(source.Save());
        reopened.ApplyAccessibilitySetup("en-US", "Retagged Navigation", PdfUaProfile.PdfUa2, strictConformance: true);
        var section = reopened.Structure.AddSection("Retagged content");
        var heading = section.AddHeader(1, "Contents");
        var paragraph = heading.Back().AddParagraph("Open Chapter 1");
        var chapter = section.Back().AddSection("Chapter 1");
        var chapterHeading = chapter.AddHeader(2, "Chapter 1");
        var chapterParagraph = chapterHeading.Back().AddParagraph("Retagged paragraph");
        var link = paragraph.Back().AddLink(
            reopened.Pages[0],
            new PdfRect<double>(40, 645, 150, 662),
            chapterHeading.GetNode(),
            "Jump to Chapter 1",
            "Jump to Chapter 1",
            "Jump to Chapter 1");

        using (var writer = reopened.Pages[0].GetWriter())
        {
            writer.BeginMarkedContent(heading.GetNode());
            writer.Font(font, 16).TextMove(40, 700).Text("Contents");
            writer.EndMarkedContent();

            writer.BeginMarkedContent(paragraph.GetNode());
            writer.Font(font, 12).TextMove(40, 675).Text("Open Chapter 1");
            writer.EndMarkedContent();

            writer.BeginMarkedContent(link.GetNode());
            writer.Font(font, 12).TextMove(40, 650).Text("Jump to Chapter 1");
            writer.EndMarkedContent();
        }

        using (var writer = reopened.Pages[1].GetWriter())
        {
            writer.BeginMarkedContent(chapterHeading.GetNode());
            writer.Font(font, 16).TextMove(40, 700).Text("Chapter 1");
            writer.EndMarkedContent();

            writer.BeginMarkedContent(chapterParagraph.GetNode());
            writer.Font(font, 12).TextMove(40, 675).Text("Retagged paragraph");
            writer.EndMarkedContent();
        }

        var bytes = reopened.Save();
        using var saved = PdfDocument.Open(bytes);

        AccessibilityIntegrityAssert.HasDocumentSetup(saved, PdfUaProfile.PdfUa2);
        AccessibilityIntegrityAssert.HasBasicStructureIntegrity(saved);
        AccessibilityIntegrityAssert.HasPdf20RootNamespace(saved);
        AccessibilityIntegrityAssert.HasStructureDestinationLinks(saved);
        AccessibilityIntegrityAssert.HasAccessibleLinkDescriptions(saved);
        AccessibilityIntegrityAssert.HasOnlyTaggedOrArtifactContent(saved);

        var names = saved.Catalog.Get<PdfDictionary>(PdfName.Names);
        Assert.NotNull(names);
        Assert.True(names!.ContainsKey(PdfName.Dests));

        var fixtures = AccessibilityFixtureGenerator.GenerateAll();
        Assert.Contains(fixtures, x => x.FileName == AccessibilityFixtureGenerator.RetaggedNavigationFixtureFileName);
    }

    [Fact]
    public void Structure_Authoring_Rejects_Already_Tagged_Documents_Without_Mutating_The_Document()
    {
        using var doc = PdfDocument.Create();
        doc.AddPage();
        var originalStructTreeRoot = new PdfDictionary
        {
            [PdfName.TYPE] = PdfName.StructTreeRoot,
            [PdfName.K] = new PdfArray()
        };
        doc.Catalog[PdfName.StructTreeRoot] = originalStructTreeRoot;
        var originalCatalogKeys = doc.Catalog.Keys.ToArray();
        var originalTrailerKeys = doc.Trailer.Keys.ToArray();

        var ex = Assert.Throws<PdfAccessibilitySetupException>(() => _ = doc.Structure);

        Assert.Contains("currently untagged", ex.Message);
        Assert.Same(originalStructTreeRoot, doc.Catalog[PdfName.StructTreeRoot]);
        Assert.Equal(originalCatalogKeys, doc.Catalog.Keys);
        Assert.Equal(originalTrailerKeys, doc.Trailer.Keys);
        Assert.False(doc.Catalog.ContainsKey(PdfName.MarkInfo));
        Assert.False(doc.Catalog.ContainsKey(PdfName.Metadata));
        Assert.False(doc.Catalog.ContainsKey(PdfName.Lang));
    }

    [Fact]
    public void Strict_PdfUa2_Save_Rejects_Internal_Page_Destination_Links()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        doc.ApplyAccessibilitySetup("en-US", "Strict", PdfUaProfile.PdfUa2, strictConformance: true);

        var section = doc.Structure.AddSection("Overview");
        var heading = section.AddHeader(1, "Overview");
        var paragraph = heading.Back().AddParagraph("Summary");
        var pageDestination = new PdfArray { page.NativeObject.Indirect(), PdfName.XYZ, 0, 760, PdfNull.Value };
        var link = paragraph.Back().AddLink(page, new PdfRect<double>(40, 710, 180, 728), pageDestination, "Jump to overview", "Jump to overview");

        using (var writer = page.GetWriter())
        {
            writer.BeginMarkedContent(heading.GetNode());
            writer.Font(font, 16).TextMove(40, 760).Text("Overview");
            writer.EndMarkedContent();

            writer.BeginMarkedContent(paragraph.GetNode());
            writer.Font(font, 12).TextMove(40, 734).Text("Summary");
            writer.EndMarkedContent();

            writer.BeginMarkedContent(link.GetNode());
            writer.Font(font, 12).TextMove(40, 710).Text("Jump to overview");
            writer.EndMarkedContent();
        }

        var ex = Assert.Throws<PdfAccessibilityConformanceException>(() => doc.Save());
        Assert.Contains("StructureNode destination", ex.Message);
    }

    [Fact]
    public void Strict_Accessibility_Save_Rejects_Invalid_List_Hierarchy()
    {
        using var doc = PdfDocument.Create();
        doc.AddPage();
        doc.ApplyAccessibilitySetup("en-US", "Invalid List", PdfUaProfile.PdfUa2, strictConformance: true);

        doc.Structure.AddList("Checklist").AddParagraph("This should have been an LI.");

        var ex = Assert.Throws<PdfAccessibilityConformanceException>(() => doc.Save());
        Assert.Contains("L elements may only contain LI", ex.Message);
    }

    [Fact]
    public void Strict_Accessibility_Save_Rejects_Untagged_Real_Content()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        doc.ApplyAccessibilitySetup("en-US", "Untagged", PdfUaProfile.PdfUa2, strictConformance: true);
        doc.Structure.AddParagraph("Tagged paragraph");

        using (var writer = page.GetWriter())
        {
            writer.Font(font, 12).TextMove(40, 760).Text("This text is not tagged.");
        }

        var ex = Assert.Throws<PdfAccessibilityConformanceException>(() => doc.Save());
        Assert.Contains("BeginMarkedContent", ex.Message);
    }
}
