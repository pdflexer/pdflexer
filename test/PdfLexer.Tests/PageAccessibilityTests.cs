using PdfLexer;
using PdfLexer.DOM;
using PdfLexer.Content;
using Xunit;

namespace PdfLexer.Tests;

public class PageAccessibilityTests
{
    [Fact]
    public void Newly_Added_Page_After_Setup_Has_Tabs_S()
    {
        using var doc = PdfDocument.Create();
        doc.ApplyAccessibilitySetup("en-US", "Tabs Test", PdfUaProfile.PdfUa1, strictConformance: true);
        
        var page = doc.AddPage();
        var tabs = page.NativeObject.Get<PdfName>(PdfName.Tabs);
        
        Assert.Equal(PdfName.S, tabs);
    }

    [Fact]
    public void Strict_Save_Rejects_Annotated_Page_Without_Tabs_S()
    {
        using var doc = PdfDocument.Create();
        doc.ApplyAccessibilitySetup("en-US", "Tabs Test", PdfUaProfile.PdfUa1, strictConformance: true);
        
        var page = doc.AddPage();
        // Manually break it
        page.NativeObject.Remove(PdfName.Tabs);
        
        // Add an annotation to trigger the requirement
        doc.Structure.AddLink(page, new PdfRect<double>(10, 10, 100, 20), PdfName.XYZ, "Link");

        var ex = Assert.Throws<PdfAccessibilityConformanceException>(() => doc.SaveTo(new System.IO.MemoryStream()));
        Assert.Contains("requires annotated pages to have /Tabs set to /S", ex.Message);
    }
}
