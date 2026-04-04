using PdfLexer.Content;
using PdfLexer.Fonts;
using PdfLexer.TextLayout;
using PdfLexer.Writing;

namespace PdfLexer.TextLayout.Tests;

public class TextBoxLayoutEngineTests
{
    private static readonly string RobotoPath = "/workspace/test/Roboto-Regular.ttf";

    [Fact]
    public void Layout_UsesExplicitNewlines_AndPreservesSpaces()
    {
        var request = CreateRequest(
            width: 200,
            height: 200,
            new TextSegment("Hello  world\nNext", new TextSegmentStyle("Roboto", 400, 12)));

        var engine = new TextBoxLayoutEngine();
        var result = engine.Layout(request);

        Assert.True(result.Success);
        Assert.Equal(2, result.Lines.Count);
        Assert.Contains(result.Lines[0].Runs, x => x.Text == "  ");
        Assert.Equal("Hello", result.Lines[0].Runs[0].Text);
        Assert.Equal("Next", result.Lines[1].Runs.Single().Text);
    }

    [Fact]
    public void Layout_UsesFallbackFamily_WhenConfigured()
    {
        var fallbackFace = CreateFace("roboto-regular", "RobotoFallback", 400);
        var library = new TextFontLibrary(new[] { fallbackFace });
        var request = new TextBoxLayoutRequest(
            200,
            100,
            library,
            new[] { new TextSegment("Hello", new TextSegmentStyle("MissingFamily", 400, 12)) })
        {
            MissingFontBehavior = TextResolutionBehavior.UseFallbackFamilies,
            FallbackFamilyNames = new[] { "RobotoFallback" }
        };

        var engine = new TextBoxLayoutEngine();
        var result = engine.Layout(request);

        Assert.True(result.Success);
        Assert.Equal("roboto-regular", result.Lines[0].Runs[0].FaceId);
    }

    [Fact]
    public void Layout_Fails_WhenWeightMissing_AndNoFallback()
    {
        var library = new TextFontLibrary(new[] { CreateFace("roboto-regular", "Roboto", 400) });
        var request = new TextBoxLayoutRequest(
            200,
            100,
            library,
            new[] { new TextSegment("Hello", new TextSegmentStyle("Roboto", 700, 12)) });

        var engine = new TextBoxLayoutEngine();
        var result = engine.Layout(request);

        Assert.Equal(TextLayoutStatus.Error, result.Status);
        Assert.Contains(result.Issues, x => x.Kind == TextLayoutIssueKind.MissingWeight);
    }

    [Fact]
    public void Layout_Fails_WhenGlyphMissing_InFailFastMode()
    {
        var request = CreateRequest(
            width: 200,
            height: 100,
            new TextSegment("Hello 漢", new TextSegmentStyle("Roboto", 400, 12)));

        var engine = new TextBoxLayoutEngine();
        var result = engine.Layout(request);

        Assert.Equal(TextLayoutStatus.Error, result.Status);
        Assert.Contains(result.Issues, x => x.Kind == TextLayoutIssueKind.MissingGlyph);
    }

    [Fact]
    public void Layout_ReportsMissingGlyph_WhenFallbackModeStillCannotResolve()
    {
        var fallbackFace = CreateFace("roboto-fallback", "RobotoFallback", 400);
        var request = new TextBoxLayoutRequest(
            200,
            100,
            new TextFontLibrary(new[]
            {
                CreateFace("roboto-regular", "Roboto", 400),
                fallbackFace
            }),
            new[] { new TextSegment("Hello 漢", new TextSegmentStyle("Roboto", 400, 12)) })
        {
            MissingGlyphBehavior = TextResolutionBehavior.UseFallbackFamilies,
            FallbackFamilyNames = new[] { "RobotoFallback" }
        };

        var engine = new TextBoxLayoutEngine();
        var result = engine.Layout(request);

        Assert.Equal(TextLayoutStatus.Error, result.Status);
        Assert.Contains(result.Issues, x => x.Kind == TextLayoutIssueKind.MissingGlyph);
    }

    [Fact]
    public void WriteTextBox_WritesMeasuredLayout_ToPdf()
    {
        var face = CreateFace("roboto-regular", "Roboto", 400);
        var fontLibrary = new PdfTextLayoutFontLibrary(new[]
        {
            new PdfTextLayoutFontFace(face, TrueTypeFont.CreateWritableFont(File.ReadAllBytes(RobotoPath)))
        });

        var request = new TextBoxLayoutRequest(
            120,
            120,
            fontLibrary.CreateLayoutLibrary(),
            new[]
            {
                new TextSegment("Hello\nWorld", new TextSegmentStyle("Roboto", 400, 12, Underline: true))
            });

        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        TextBoxLayoutResult result;
        using (var writer = page.GetWriter())
        {
            result = writer.WriteTextBox(new PdfRect<double>(20, 20, 140, 140), request, fontLibrary);
        }

        Assert.True(result.Lines.Count >= 2);
        var words = new Dictionary<string, double>();
        var scanner = page.GetWordScanner();
        while (scanner.Advance())
        {
            words[scanner.CurrentWord] = scanner.GetInfo().BoundingBox.LLy;
        }

        Assert.Contains("Hello", words.Keys);
        Assert.Contains("World", words.Keys);
        Assert.True(words["Hello"] > words["World"]);
    }

    private static TextBoxLayoutRequest CreateRequest(double width, double height, params TextSegment[] segments)
    {
        return new TextBoxLayoutRequest(
            width,
            height,
            new TextFontLibrary(new[] { CreateFace("roboto-regular", "Roboto", 400) }),
            segments);
    }

    private static TextFontFace CreateFace(string faceId, string familyName, int weight)
        => new(faceId, familyName, weight, File.ReadAllBytes(RobotoPath));
}
