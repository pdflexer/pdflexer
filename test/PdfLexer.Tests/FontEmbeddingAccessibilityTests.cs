using PdfLexer.Content.Model;
using PdfLexer.DOM;
using PdfLexer.Fonts;
using Xunit;

namespace PdfLexer.Tests;

public class FontEmbeddingAccessibilityTests
{
    [Fact]
    public void Strict_Accessibility_Save_Rejects_NonEmbedded_Fonts_In_Tagged_Content()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        doc.ApplyAccessibilitySetup("en-US", "Non-Embedded Font Test", PdfUaProfile.PdfUa1, strictConformance: true);

        var p = doc.Structure.AddParagraph("Tagged text");

        using (var writer = page.GetWriter())
        {
            writer.BeginMarkedContent(p.GetNode());
            // Helvetica is a Standard 14 font and is NOT embedded by default in this library
            writer.Font(Standard14Font.GetHelvetica(), 12).TextMove(40, 760).Text("Tagged with Helvetica");
            writer.EndMarkedContent();
        }

        var ex = Assert.Throws<PdfAccessibilityConformanceException>(() => doc.SaveTo(new System.IO.MemoryStream()));
        Assert.Contains("requires all rendered fonts for tagged content to be embedded", ex.Message);
    }

    [Fact]
    public void Strict_Accessibility_Save_Allows_Embedded_Fonts_In_Tagged_Content()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        doc.ApplyAccessibilitySetup("en-US", "Embedded Font Test", PdfUaProfile.PdfUa1, strictConformance: true);

        var p = doc.Structure.AddParagraph("Tagged text");

        // Use absolute path for reliability in this environment
        var fontPath = "/workspace/test/Roboto-Regular.ttf";
        if (!System.IO.File.Exists(fontPath))
        {
            // Fallback for local dev
            fontPath = "../../../../test/Roboto-Regular.ttf";
        }
        var fontData = System.IO.File.ReadAllBytes(fontPath);
        var font = TrueTypeFont.CreateWritableFont(fontData);

        using (var writer = page.GetWriter())
        {
            writer.BeginMarkedContent(p.GetNode());
            writer.Font(font, 12).TextMove(40, 760).Text("Tagged with Roboto");
            writer.EndMarkedContent();
        }

        // This should not throw if the font is correctly recognized as embedded
        doc.SaveTo(new System.IO.MemoryStream());
    }

    [Fact]
    public void Strict_Accessibility_Save_Allows_NonEmbedded_Fonts_In_Artifact_Content()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        doc.ApplyAccessibilitySetup("en-US", "Artifact Font Test", PdfUaProfile.PdfUa1, strictConformance: true);

        // Add a tagged element to satisfy the structure tree requirement
        var p = doc.Structure.AddParagraph("Dummy");

        using (var writer = page.GetWriter())
        {
            writer.BeginMarkedContent(p.GetNode());
            // No text here, just satisfying the tagging requirement for the element
            writer.EndMarkedContent();

            writer.BeginArtifact();
            writer.Font(Standard14Font.GetHelvetica(), 12).TextMove(40, 760).Text("Artifact with Helvetica");
            writer.EndMarkedContent();
        }

        // This should not throw because artifacts don't require embedded fonts for accessibility
        doc.SaveTo(new System.IO.MemoryStream());
    }
}
