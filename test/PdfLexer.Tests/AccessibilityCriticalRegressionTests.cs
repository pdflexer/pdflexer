using System.Linq;
using System.Text;
using PdfLexer.Content;
using PdfLexer.Content.Model;
using PdfLexer.DOM;
using PdfLexer.Writing;
using Xunit;

namespace PdfLexer.Tests;

public class AccessibilityCriticalRegressionTests
{
    private const string UnicodeValue = "图表：季度收入 — Résumé — العربية — עברית";

    [Fact]
    public void TextStringFactory_Uses_Compact_Encoding_Only_For_Ascii()
    {
        Assert.Equal(PdfTextEncodingType.PdfDocument, PdfString.CreateTextString("ASCII").Encoding);
        Assert.Equal(PdfTextEncodingType.UTF16BE, PdfString.CreateTextString("Résumé").Encoding);
        Assert.Equal(PdfTextEncodingType.UTF16BE, PdfString.CreateTextString("—").Encoding);
        Assert.Equal(PdfTextEncodingType.UTF16BE, PdfString.CreateTextString("图表").Encoding);
    }

    [Fact]
    public void Accessibility_Text_Strings_RoundTrip_Without_Data_Loss()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        doc.ApplyAccessibilitySetup(UnicodeValue, UnicodeValue, strictConformance: false);

        var table = doc.Structure.AddTable(UnicodeValue)
            .ElementId(UnicodeValue)
            .Alt(UnicodeValue)
            .ActualText(UnicodeValue)
            .Expansion(UnicodeValue)
            .Lang(UnicodeValue)
            .TableSummary(UnicodeValue);

        doc.Structure.AddLink(
            page,
            new PdfRect<double>(10, 10, 100, 30),
            PdfName.Fit,
            UnicodeValue,
            UnicodeValue);

        var widget = AnnotationFactory.CreateTextWidget(
            doc,
            page,
            new PdfRect<double>(10, 40, 100, 60),
            UnicodeValue,
            UnicodeValue);
        doc.Structure.AddLabeledFormField(widget, UnicodeValue, UnicodeValue);

        using var saved = PdfDocument.Open(doc.Save());

        Assert.Equal(UnicodeValue, saved.Catalog.Get<PdfString>(PdfName.Lang)!.Value);
        Assert.Equal(UnicodeValue, saved.Trailer.Get<PdfDictionary>(PdfName.Info)!.Get<PdfString>(PdfName.Title)!.Value);

        var tableElement = AccessibilityIntegrityAssert.GetStructureElements(saved)
            .Single(x => x.Get<PdfName>(PdfName.S) == PdfName.Table);
        Assert.Equal(UnicodeValue, tableElement.Get<PdfString>(PdfName.ID)!.Value);
        Assert.Equal(UnicodeValue, tableElement.Get<PdfString>(PdfName.T)!.Value);
        Assert.Equal(UnicodeValue, tableElement.Get<PdfString>(PdfName.Alt)!.Value);
        Assert.Equal(UnicodeValue, tableElement.Get<PdfString>(PdfName.ActualText)!.Value);
        Assert.Equal(UnicodeValue, tableElement.Get<PdfString>(PdfName.E)!.Value);
        Assert.Equal(UnicodeValue, tableElement.Get<PdfString>(PdfName.Lang)!.Value);
        Assert.Equal(
            UnicodeValue,
            tableElement.Get<PdfDictionary>(PdfName.A)!.Get<PdfString>(PdfName.Summary)!.Value);

        var structRoot = saved.Catalog.Get<PdfDictionary>(PdfName.StructTreeRoot)!;
        var idNames = structRoot.Get<PdfDictionary>(PdfName.IDTree)!.Get<PdfArray>(PdfName.Names)!;
        Assert.Equal(UnicodeValue, idNames[0].GetAs<PdfString>().Value);

        var link = AccessibilityIntegrityAssert.GetAnnotations(saved, PdfName.Link).Single();
        Assert.Equal(UnicodeValue, link.Get<PdfString>(PdfName.Contents)!.Value);

        var savedWidget = AccessibilityIntegrityAssert.GetAnnotations(saved, PdfName.Widget).Single();
        var field = savedWidget[PdfName.Parent].Resolve().GetAs<PdfDictionary>();
        Assert.Equal(UnicodeValue, field.Get<PdfString>(PdfName.T)!.Value);
        Assert.Equal(UnicodeValue, field.Get<PdfString>((PdfName)"TU")!.Value);
        Assert.Equal(UnicodeValue, savedWidget.Get<PdfString>((PdfName)"TU")!.Value);
        Assert.Equal(UnicodeValue, savedWidget.Get<PdfString>(PdfName.Contents)!.Value);

        Assert.Equal(UnicodeValue, table.GetNode().Summary);
    }

    [Fact]
    public void PageWriter_Allocates_MCIDs_Across_Writer_Instances_And_Page_Wrappers()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        var first = doc.Structure.AddParagraph("First");
        var second = doc.Structure.AddParagraph("Second");

        using (var writer = page.GetWriter())
        {
            writer.BeginMarkedContent(first.GetNode()).EndMarkedContent();
        }

        var alias = new PdfPage(page.NativeObject);
        using (var writer = alias.GetWriter())
        {
            writer.BeginMarkedContent(second.GetNode()).EndMarkedContent();
        }

        Assert.Equal(0, first.GetNode().ContentItems.Single().MCID);
        Assert.Equal(1, second.GetNode().ContentItems.Single().MCID);

        using var saved = PdfDocument.Open(doc.Save());
        AccessibilityIntegrityAssert.HasBasicStructureIntegrity(saved);
        Assert.Contains("/P <</MCID 0>> BDC", saved.Pages[0].DumpDecodedContents());
        Assert.Contains("/P <</MCID 1>> BDC", saved.Pages[0].DumpDecodedContents());
    }

    [Theory]
    [InlineData(PageWriteMode.Append)]
    [InlineData(PageWriteMode.Pre)]
    public void PageWriter_Seeds_Append_And_Pre_From_Existing_Inline_And_Property_MCIDs(PageWriteMode mode)
    {
        using var doc = PdfDocument.Create();
        var page = CreatePageWithExistingMcids(doc);
        var node = new StructureNode { Type = "P" };

        using (var writer = page.GetWriter(mode))
        {
            writer.BeginMarkedContent(node).EndMarkedContent();
        }

        Assert.Equal(8, node.ContentItems.Single().MCID);
    }

    [Fact]
    public void PageWriter_Replace_Starts_A_Fresh_MCID_Namespace()
    {
        using var doc = PdfDocument.Create();
        var page = CreatePageWithExistingMcids(doc);
        var node = new StructureNode { Type = "P" };

        using (var writer = page.GetWriter(PageWriteMode.Replace))
        {
            writer.BeginMarkedContent(node).EndMarkedContent();
        }

        Assert.Equal(0, node.ContentItems.Single().MCID);
    }

    [Fact]
    public void Form_Allocator_Seeds_From_Existing_Form_Content_And_Shares_Wrapper_State()
    {
        var form = new XObjForm(100, 50)
        {
            Resources = new PdfDictionary(),
            Contents = new PdfByteArrayStreamContents(
                Encoding.ASCII.GetBytes("/Span <</MCID 5>> BDC EMC"))
        };

        Assert.Equal(6, McidAllocator.Allocate(form));
        Assert.Equal(7, McidAllocator.Allocate(new XObjForm(form.NativeObject)));
    }

    [Fact]
    public void Remediation_Allocator_Uses_Existing_Page_MCIDs()
    {
        using var source = PdfDocument.Create();
        CreatePageWithExistingMcids(source);
        using var doc = PdfDocument.Open(source.Save());
        using var session = doc.BeginRemediation(new PdfLexer.Remediation.RemediationSessionConfiguration
        {
            StrictConformance = false
        });

        Assert.Contains("/MCID 3", doc.Pages[0].DumpDecodedContents());
        Assert.Contains("/Existing BDC", doc.Pages[0].DumpDecodedContents());
        var existingMcids = doc.Pages[0].GetContentNodes()
            .OfType<MarkedContentGroup<double>>()
            .Select(x =>
                x.Tag.InlineProps?.Get<PdfNumber>(PdfName.MCID) ??
                x.Tag.PropList?.Get<PdfNumber>(PdfName.MCID))
            .Where(x => x != null)
            .Select(x => (int)x!)
            .OrderBy(x => x)
            .ToArray();
        Assert.Equal(new[] { 3, 7 }, existingMcids);
        Assert.Equal(8, session.AllocateMcid(doc.Pages[0]));
    }

    [Fact]
    public void StructuralSerializer_Rejects_Duplicate_Page_MCIDs()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        var root = new StructureNode { Type = "Document" };
        var first = new StructureNode { Type = "P" };
        var second = new StructureNode { Type = "P" };
        root.Children.Add(first);
        root.Children.Add(second);
        first.ContentItems.Add((page, 0));
        second.ContentItems.Add((page, 0));

        var error = Assert.Throws<PdfAccessibilityConformanceException>(
            () => new StructuralSerializer().ConvertToPdf(root));

        Assert.Contains("Page content MCID 0", error.Message);
    }

    [Fact]
    public void StructuralSerializer_Rejects_Duplicate_Form_MCIDs()
    {
        var form = new XObjForm(100, 50);
        var root = new StructureNode { Type = "Document" };
        var first = new StructureNode { Type = "Span" };
        var second = new StructureNode { Type = "Span" };
        root.Children.Add(first);
        root.Children.Add(second);
        first.XObjectContentItems.Add((form, 0));
        second.XObjectContentItems.Add((form, 0));

        var error = Assert.Throws<PdfAccessibilityConformanceException>(
            () => new StructuralSerializer().ConvertToPdf(root));

        Assert.Contains("Form XObject content MCID 0", error.Message);
    }

    private static PdfPage CreatePageWithExistingMcids(PdfDocument doc)
    {
        var page = doc.AddPage();
        page.Resources.GetOrCreateValue<PdfDictionary>((PdfName)"Properties")[(PdfName)"Existing"] =
            new PdfDictionary { [PdfName.MCID] = new PdfIntNumber(7) };
        page.NativeObject[PdfName.Contents] = new PdfStream(
            new PdfByteArrayStreamContents(
                Encoding.ASCII.GetBytes(
                    "/Span <</MCID 3>> BDC EMC /Span /Existing BDC EMC"))).Indirect();
        return page;
    }
}
