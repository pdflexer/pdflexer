using PdfLexer.DOM;
using Xunit;

namespace PdfLexer.Tests;

public class StructureConformanceTests
{
    [Fact]
    public void Strict_Accessibility_Save_Rejects_Figure_Without_Alt_Or_ActualText()
    {
        using var doc = PdfDocument.Create();
        doc.AddPage();
        doc.ApplyAccessibilitySetup("en-US", "Figure Test", PdfUaProfile.PdfUa1, strictConformance: true);

        doc.Structure.AddFigure(); // Missing Alt and ActualText

        var ex = Assert.Throws<PdfAccessibilityConformanceException>(() => doc.SaveTo(new System.IO.MemoryStream()));
        Assert.Contains("Figure and Formula elements to have either an Alt or ActualText", ex.Message);
    }

    [Fact]
    public void Strict_Accessibility_Save_Allows_Figure_With_Alt()
    {
        using var doc = PdfDocument.Create();
        doc.AddPage();
        doc.ApplyAccessibilitySetup("en-US", "Figure Test", PdfUaProfile.PdfUa1, strictConformance: true);

        doc.Structure.AddFigure(altText: "A beautiful sunset");

        // Should not throw for missing Alt/ActualText
        doc.SaveTo(new System.IO.MemoryStream());
    }

    [Fact]
    public void Strict_Accessibility_Save_Rejects_Formula_Without_Alt_Or_ActualText()
    {
        using var doc = PdfDocument.Create();
        doc.AddPage();
        doc.ApplyAccessibilitySetup("en-US", "Formula Test", PdfUaProfile.PdfUa1, strictConformance: true);

        doc.Structure.AddElement("Formula"); // Missing Alt and ActualText

        var ex = Assert.Throws<PdfAccessibilityConformanceException>(() => doc.SaveTo(new System.IO.MemoryStream()));
        Assert.Contains("Figure and Formula elements to have either an Alt or ActualText", ex.Message);
    }

    [Fact]
    public void Strict_Accessibility_Save_Rejects_Note_Structure()
    {
        using var doc = PdfDocument.Create();
        doc.AddPage();
        doc.ApplyAccessibilitySetup("en-US", "Note Test", PdfUaProfile.PdfUa1, strictConformance: true);

        doc.Structure.AddNote("Ambiguous Note");

        var ex = Assert.Throws<PdfAccessibilityConformanceException>(() => doc.SaveTo(new System.IO.MemoryStream()));
        Assert.Contains("rejects the 'Note' structure type", ex.Message);
    }

    [Fact]
    public void Strict_Accessibility_Save_Rejects_Standard_To_Standard_RoleMapping()
    {
        using var doc = PdfDocument.Create();
        doc.AddPage();
        doc.ApplyAccessibilitySetup("en-US", "RoleMap Test", PdfUaProfile.PdfUa1, strictConformance: true);

        doc.Structure.MapRole("H1", "P"); // Mapping standard H1 to standard P

        var ex = Assert.Throws<PdfAccessibilityConformanceException>(() => doc.SaveTo(new System.IO.MemoryStream()));
        Assert.Contains("mapping from a standard structure type to another type", ex.Message);
    }

    [Fact]
    public void Strict_Accessibility_Save_Allows_Custom_To_Standard_RoleMapping()
    {
        using var doc = PdfDocument.Create();
        doc.AddPage();
        doc.ApplyAccessibilitySetup("en-US", "RoleMap Test", PdfUaProfile.PdfUa1, strictConformance: true);

        doc.Structure.MapRole("MyHeader", "H1"); // Custom to standard is fine

        // Should not throw
        doc.SaveTo(new System.IO.MemoryStream());
    }
}
