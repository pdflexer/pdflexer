using System.IO;
using System.Text;
using PdfLexer.Content;
using PdfLexer.Fonts;
using PdfLexer.Writing;
using Xunit;

namespace PdfLexer.Tests;

public class AccessibilitySetupTests
{
    [Fact]
    public void ApplyAccessibilitySetup_Configures_PdfUa_Document_Defaults_And_RoundTrips()
    {
        using var doc = PdfDocument.Create();
        doc.AddPage();

        doc.ApplyAccessibilitySetup("en-US", "Accessible Title", PdfUaProfile.PdfUa1, strictConformance: false);

        using var ms = new MemoryStream();
        doc.SaveTo(ms);

        using var doc2 = PdfDocument.Open(ms.ToArray());
        Assert.Equal("en-US", doc2.Catalog.Get<PdfString>(PdfName.Lang)?.Value);

        var info = doc2.Trailer.Get<PdfDictionary>(PdfName.Info);
        Assert.NotNull(info);
        Assert.Equal("Accessible Title", info.Get<PdfString>(PdfName.Title)?.Value);

        var viewerPreferences = doc2.Catalog.Get<PdfDictionary>(PdfName.ViewerPreferences);
        Assert.NotNull(viewerPreferences);
        Assert.Equal(PdfBoolean.True, viewerPreferences[PdfName.DisplayDocTitle]);

        var markInfo = doc2.Catalog.Get<PdfDictionary>(PdfName.MarkInfo);
        Assert.NotNull(markInfo);
        Assert.Equal(PdfBoolean.True, markInfo[PdfName.Marked]);
        Assert.Equal(PdfBoolean.False, markInfo[PdfName.Suspects]);

        Assert.Equal(PdfName.S, doc2.Pages[0].NativeObject.Get<PdfName>(PdfName.Tabs));

        var metadata = doc2.Catalog[PdfName.Metadata].Resolve().GetAs<PdfStream>();
        var xml = Encoding.UTF8.GetString(metadata.Contents.GetDecodedData());
        Assert.Equal(PdfName.Metadata, metadata.Dictionary[PdfName.TYPE]);
        Assert.Equal(PdfName.XML, metadata.Dictionary[PdfName.Subtype]);
        Assert.Contains("<pdfuaid:part>1</pdfuaid:part>", xml);
        Assert.Contains("Accessible Title", xml);
        Assert.Contains("en-US", xml);
        AccessibilityIntegrityAssert.HasDocumentSetup(doc2, PdfUaProfile.PdfUa1);
    }

    [Fact]
    public void ApplyAccessibilitySetup_Updates_All_Pages_And_Preserves_Explicit_Tabs_Overrides()
    {
        using var doc = PdfDocument.Create();
        var page1 = doc.AddPage();
        var page2 = doc.AddPage();
        page2.NativeObject[PdfName.Tabs] = PdfName.R;

        doc.ApplyAccessibilitySetup("fr-CA", "Existing Doc", PdfUaProfile.PdfUa1, strictConformance: false);

        using var ms = new MemoryStream();
        doc.SaveTo(ms);

        using var doc2 = PdfDocument.Open(ms.ToArray());
        Assert.Equal(PdfName.S, doc2.Pages[0].NativeObject.Get<PdfName>(PdfName.Tabs));
        Assert.Equal(PdfName.R, doc2.Pages[1].NativeObject.Get<PdfName>(PdfName.Tabs));
        Assert.Equal("Existing Doc", doc2.Trailer.Get<PdfDictionary>(PdfName.Info)?.Get<PdfString>(PdfName.Title)?.Value);
        Assert.Equal("fr-CA", doc2.Catalog.Get<PdfString>(PdfName.Lang)?.Value);
        AccessibilityIntegrityAssert.HasDocumentSetup(doc2, PdfUaProfile.PdfUa1);
    }

    [Fact]
    public void ApplyAccessibilitySetup_PdfUa2_Writes_Revision_Metadata_And_Pdf20_Namespace()
    {
        using var doc = PdfDocument.Create();
        var page1 = doc.AddPage();
        var page2 = doc.AddPage();
        doc.ApplyAccessibilitySetup("en-US", "Accessible Title", PdfUaProfile.PdfUa2);

        var section = doc.Structure.AddSection("Overview");
        var heading = section.AddHeader(1, "Overview");
        var paragraph = heading.Back().AddParagraph("Summary");
        var destination = section.AddSection("Appendix").AddHeader(2, "Appendix");
        var link = paragraph.Back().AddLink(
            page1,
            new PdfRect<double>(40, 710, 200, 728),
            destination.GetNode(),
            "Jump to appendix",
            "Jump to appendix",
            "Jump to appendix");

        using (var writer = page1.GetWriter())
        {
            writer.BeginMarkedContent(heading.GetNode());
            writer.EndMarkedContent();

            writer.BeginMarkedContent(paragraph.GetNode());
            writer.EndMarkedContent();

            writer.BeginMarkedContent(link.GetNode());
            writer.EndMarkedContent();
        }

        using (var writer = page2.GetWriter())
        {
            writer.BeginMarkedContent(destination.GetNode());
            writer.EndMarkedContent();
        }

        var bytes = doc.Save();
        using var doc2 = PdfDocument.Open(bytes);
        AccessibilityIntegrityAssert.HasDocumentSetup(doc2, PdfUaProfile.PdfUa2);
        AccessibilityIntegrityAssert.HasPdf20RootNamespace(doc2);
        AccessibilityIntegrityAssert.HasStructureDestinationLinks(doc2);
        Assert.StartsWith("%PDF-2.0", Encoding.ASCII.GetString(bytes, 0, 8));
    }

    [Fact]
    public void BlankDocument_SetupThenAuthor_Flow_RemainsSupported()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage();

        doc.ApplyAccessibilitySetup("en-US", "Blank Authoring", PdfUaProfile.PdfUa1, strictConformance: false);

        var paragraph = doc.Structure.AddParagraph("Body").GetNode();
        using (var writer = page.GetWriter())
        {
            writer.BeginMarkedContent(paragraph);
            writer.Font(Base14.Helvetica, 12).TextMove(40, 700).Text("Body");
            writer.EndMarkedContent();
        }

        var bytes = doc.Save();
        using var reopened = PdfDocument.Open(bytes);
        AccessibilityIntegrityAssert.HasDocumentSetup(reopened, PdfUaProfile.PdfUa1);
        AccessibilityIntegrityAssert.HasBasicStructureIntegrity(reopened);
        AccessibilityIntegrityAssert.HasOnlyTaggedOrArtifactContent(reopened);
    }

    [Fact]
    public void ApplyAccessibilitySetup_Rejects_Already_Tagged_Documents_Without_Mutating_Document_Level_Setup()
    {
        using var doc = PdfDocument.Create();
        doc.AddPage();
        doc.Catalog[PdfName.StructTreeRoot] = new PdfDictionary();

        var ex = Assert.Throws<PdfAccessibilitySetupException>(() =>
            doc.ApplyAccessibilitySetup("en-US", "Tagged", PdfUaProfile.PdfUa1));

        Assert.Contains("currently untagged", ex.Message);
        Assert.False(doc.Catalog.ContainsKey(PdfName.Metadata));
        Assert.False(doc.Catalog.ContainsKey(PdfName.MarkInfo));
        Assert.False(doc.Catalog.ContainsKey(PdfName.Lang));
        Assert.False(doc.Trailer.ContainsKey(PdfName.Info));
    }

    [Fact]
    public void ApplyAccessibilitySetup_Preserves_Named_Destinations_And_Internal_Links_After_Save()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage();

        var dest = new PdfArray
        {
            page.NativeObject.Indirect(),
            PdfName.XYZ,
            new PdfIntNumber(0),
            new PdfIntNumber(0),
            PdfNull.Value
        };
        var dests = new PdfDictionary
        {
            [(PdfName)"Names"] = new PdfArray
            {
                new PdfString("chapter-1"),
                dest
            }
        };
        doc.Catalog[PdfName.Names] = new PdfDictionary
        {
            [(PdfName)"Dests"] = dests
        };

        page.NativeObject[PdfName.Annots] = new PdfArray
        {
            new PdfDictionary
            {
                [PdfName.Subtype] = PdfName.Link,
                [PdfName.Rect] = new PdfArray { 0, 0, 10, 10 },
                [PdfName.Dest] = new PdfString("chapter-1")
            }
        };

        doc.ApplyAccessibilitySetup("en-US", "Linked", PdfUaProfile.PdfUa1, strictConformance: false);

        using var ms = new MemoryStream();
        doc.SaveTo(ms);

        using var doc2 = PdfDocument.Open(ms.ToArray());
        var names = doc2.Catalog.Get<PdfDictionary>(PdfName.Names);
        Assert.NotNull(names);
        Assert.True(names.ContainsKey((PdfName)"Dests"));

        var annots = doc2.Pages[0].NativeObject.Get<PdfArray>(PdfName.Annots);
        Assert.NotNull(annots);
        var savedAnnot = annots[0].Resolve().GetAs<PdfDictionary>();
        Assert.Equal("chapter-1", savedAnnot.Get<PdfString>(PdfName.Dest)?.Value);
        Assert.Equal("Linked", doc2.Trailer.Get<PdfDictionary>(PdfName.Info)?.Get<PdfString>(PdfName.Title)?.Value);
        AccessibilityIntegrityAssert.HasDocumentSetup(doc2, PdfUaProfile.PdfUa1);
    }

    [Fact]
    public void Save_Strips_Link_Annotations_With_Unresolvable_Named_Destinations()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage();

        var dest = new PdfArray
        {
            page.NativeObject.Indirect(),
            PdfName.XYZ,
            new PdfIntNumber(0),
            new PdfIntNumber(0),
            PdfNull.Value
        };
        doc.Catalog[PdfName.Names] = new PdfDictionary
        {
            [PdfName.Dests] = new PdfDictionary
            {
                [PdfName.Names] = new PdfArray { new PdfString("known-target"), dest }
            }
        };

        page.NativeObject[PdfName.Annots] = new PdfArray
        {
            new PdfDictionary
            {
                [PdfName.Subtype] = PdfName.Link,
                [PdfName.Rect] = new PdfArray { 0, 0, 10, 10 },
                [PdfName.Dest] = new PdfString("known-target")
            },
            new PdfDictionary
            {
                [PdfName.Subtype] = PdfName.Link,
                [PdfName.Rect] = new PdfArray { 0, 0, 10, 10 },
                [PdfName.Dest] = new PdfString("ghost-target")
            },
            new PdfDictionary
            {
                [PdfName.Subtype] = PdfName.Link,
                [PdfName.Rect] = new PdfArray { 0, 0, 10, 10 },
                [PdfName.Dest] = (PdfName)"ghost-name"
            }
        };

        using var ms = new MemoryStream();
        doc.SaveTo(ms);

        using var doc2 = PdfDocument.Open(ms.ToArray());
        var annots = doc2.Pages[0].NativeObject.Get<PdfArray>(PdfName.Annots)!;
        Assert.Single(annots);
        Assert.Equal("known-target", annots[0].Resolve().GetAs<PdfDictionary>().Get<PdfString>(PdfName.Dest)?.Value);
    }

    [Fact]
    public void Save_Strips_Link_Annotations_With_Named_Destinations_When_No_Names_Tree()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage();

        page.NativeObject[PdfName.Annots] = new PdfArray
        {
            new PdfDictionary
            {
                [PdfName.Subtype] = PdfName.Link,
                [PdfName.Rect] = new PdfArray { 0, 0, 10, 10 },
                [PdfName.Dest] = new PdfString("any-target")
            }
        };

        using var ms = new MemoryStream();
        doc.SaveTo(ms);

        using var doc2 = PdfDocument.Open(ms.ToArray());
        var annots = doc2.Pages[0].NativeObject.Get<PdfArray>(PdfName.Annots);
        Assert.True(annots == null || annots.Count == 0);
    }
}
