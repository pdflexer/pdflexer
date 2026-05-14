using PdfLexer;
using PdfLexer.DOM;
using PdfLexer.Content;
using Xunit;

namespace PdfLexer.Tests;

public class Phase2AccessibilityTests
{
    [Fact]
    public void AddLabeledFormField_Sets_Up_Correct_Structure_And_Widget_Metadata()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        doc.ApplyAccessibilitySetup("en-US", "Form Test", PdfUaProfile.PdfUa1, strictConformance: true);

        var rect = new PdfRect<double>(100, 700, 200, 720);
        var widget = AnnotationFactory.CreateTextWidget(doc, page, rect, "firstName", "Enter First Name");
        
        // Use the new helper - it returns the Lbl context
        doc.Structure.AddLabeledFormField(widget, "First Name Label", "Tooltip Override");

        var root = doc.Structure.GetRoot();
        var form = root.Children[0];
        Assert.Equal("Form", form.Type);
        Assert.Equal("Lbl", form.Children[0].Type);
        
        Assert.Equal("Tooltip Override", widget.NativeObject.Get<PdfString>(PdfName.Contents)?.Value);
        Assert.Equal("Tooltip Override", widget.NativeObject.Get<PdfString>((PdfName)"TU")?.Value);
        Assert.Equal("Tooltip Override", widget.Field.Get<PdfString>((PdfName)"TU")?.Value);
    }

    [Fact]
    public void Ref_Helper_Associates_Nodes_By_ID()
    {
        using var doc = PdfDocument.Create();
        doc.ApplyAccessibilitySetup("en-US", "Ref Test", PdfUaProfile.PdfUa1, strictConformance: true);

        var target = doc.Structure.AddParagraph("Target").GetNode();
        doc.Structure.AddTOCI("TOC Entry").Ref(target);

        Assert.NotNull(target.ID);
        var toci = doc.Structure.GetRoot().Children[1];
        Assert.Contains(target.ID, toci.References);
    }

    [Fact]
    public void Strict_Save_Rejects_TOC_Without_PageLabels()
    {
        using var doc = PdfDocument.Create();
        doc.AddPage();
        doc.AddPage();
        doc.ApplyAccessibilitySetup("en-US", "TOC Test", PdfUaProfile.PdfUa1, strictConformance: true);

        doc.Structure.AddTOC();

        var ex = Assert.Throws<PdfAccessibilityConformanceException>(() => doc.SaveTo(new System.IO.MemoryStream()));
        Assert.Contains("requires PageLabels to be defined when a Table of Contents (TOC) is present", ex.Message);
    }

    [Fact]
    public void Strict_Save_Allows_TOC_With_PageLabels()
    {
        using var doc = PdfDocument.Create();
        doc.AddPage();
        doc.AddPage();
        doc.ApplyAccessibilitySetup("en-US", "TOC Test", PdfUaProfile.PdfUa1, strictConformance: true);

        doc.SetPageLabel(0, new PageLabel(PageLabelStyle.Decimal));
        doc.Structure.AddTOC();

        // Should not throw
        doc.SaveTo(new System.IO.MemoryStream());
    }

    [Fact]
    public void AddDocumentTitle_And_AddFENote_Work()
    {
        using var doc = PdfDocument.Create();
        doc.ApplyAccessibilitySetup("en-US", "Helpers Test", PdfUaProfile.PdfUa1, strictConformance: true);

        doc.Structure.AddDocumentTitle("Main Title");
        doc.Structure.AddFENote("Footnote");

        var root = doc.Structure.GetRoot();
        Assert.Equal("Title", root.Children[0].Type);
        Assert.Equal("FENote", root.Children[1].Type);
    }
}
