using PdfLexer.Content;
using PdfLexer.Fonts;
using PdfLexer.TextLayout;
using PdfLexer.Writing;

namespace PdfLexer.TextLayout.FunctionalTests;

public class ChromePdfConformanceTests
{
    private const double PositionTolerance = 6.0;
    private const double SizeTolerance = 8.0;
    private static readonly string RobotoPath = "/workspace/test/Roboto-Regular.ttf";

    [ChromiumFact]
    public async Task ChromePdf_And_PdfLexerPdf_HaveComparableWordGeometry_ForExplicitLines()
    {
        var fixture = CreateFixture(new[]
        {
            new TextSegment("Hello  world\nNext line", new TextSegmentStyle("FixtureFont", 400, 14, Underline: true, LineSpacing: 18))
        });

        var chromePdf = await BrowserFixtureRenderer.RenderPdfAsync(fixture);
        var pdfLexerPdf = RenderWithPdfLexer(fixture);

        var chromeWords = ExtractWords(chromePdf);
        var libraryWords = ExtractWords(pdfLexerPdf);

        AssertWordGeometryComparable(chromeWords, libraryWords);
    }

    [ChromiumFact]
    public async Task ChromePdf_And_PdfLexerPdf_HaveComparableWordGeometry_ForWrappedText()
    {
        var fixture = CreateFixture(new[]
        {
            new TextSegment("This is a wrapped sample that should span multiple lines in both engines.", new TextSegmentStyle("FixtureFont", 400, 12, LineSpacing: 15))
        }, horizontalAlignment: TextHorizontalAlignment.Left, boxWidth: 150, boxHeight: 90);

        var chromePdf = await BrowserFixtureRenderer.RenderPdfAsync(fixture);
        var pdfLexerPdf = RenderWithPdfLexer(fixture);

        var chromeWords = ExtractWords(chromePdf);
        var libraryWords = ExtractWords(pdfLexerPdf);

        AssertWordGeometryComparable(chromeWords, libraryWords);
    }

    private static HtmlTextBoxFixture CreateFixture(
        IReadOnlyList<TextSegment> segments,
        TextHorizontalAlignment horizontalAlignment = TextHorizontalAlignment.Left,
        double pageWidth = 240,
        double pageHeight = 160,
        double boxLeft = 20,
        double boxTop = 20,
        double boxWidth = 180,
        double boxHeight = 100)
        => new(
            pageWidth,
            pageHeight,
            boxLeft,
            boxTop,
            boxWidth,
            boxHeight,
            File.ReadAllBytes(RobotoPath),
            segments,
            horizontalAlignment);

    private static byte[] RenderWithPdfLexer(HtmlTextBoxFixture fixture)
    {
        var fontData = File.ReadAllBytes(RobotoPath);
        var face = new TextFontFace("fixture-regular", "FixtureFont", 400, fontData);
        var library = new PdfTextLayoutFontLibrary(new[]
        {
            new PdfTextLayoutFontFace(face, TrueTypeFont.CreateWritableFont(fontData))
        });

        var request = new TextBoxLayoutRequest(
            fixture.BoxWidth,
            fixture.BoxHeight,
            library.CreateLayoutLibrary(),
            fixture.Segments)
        {
            HorizontalAlignment = fixture.HorizontalAlignment,
            OverflowMode = TextOverflowMode.Clip
        };

        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        page.MediaBox.LLx = 0;
        page.MediaBox.LLy = 0;
        page.MediaBox.URx = (decimal)fixture.PageWidth;
        page.MediaBox.URy = (decimal)fixture.PageHeight;

        var boxBottom = fixture.PageHeight - fixture.BoxTop - fixture.BoxHeight;
        using (var writer = page.GetWriter())
        {
            writer.WriteTextBox(
                new PdfRect<double>(fixture.BoxLeft, boxBottom, fixture.BoxLeft + fixture.BoxWidth, boxBottom + fixture.BoxHeight),
                request,
                library);
        }

        return doc.Save();
    }

    private static IReadOnlyList<ExtractedWord> ExtractWords(byte[] pdf)
    {
        using var doc = PdfDocument.Open(pdf);
        var page = doc.Pages[0];
        var words = new List<ExtractedWord>();
        var scanner = page.GetWordScanner();
        while (scanner.Advance())
        {
            var box = scanner.GetInfo().BoundingBox;
            words.Add(new ExtractedWord(scanner.CurrentWord, box));
        }

        return words;
    }

    private static void AssertWordGeometryComparable(IReadOnlyList<ExtractedWord> expected, IReadOnlyList<ExtractedWord> actual)
    {
        Assert.Equal(expected.Count, actual.Count);
        for (var i = 0; i < expected.Count; i++)
        {
            Assert.Equal(expected[i].Text, actual[i].Text);
            Assert.InRange(Math.Abs(expected[i].BoundingBox.LLx - actual[i].BoundingBox.LLx), 0, PositionTolerance);
            Assert.InRange(Math.Abs(expected[i].BoundingBox.LLy - actual[i].BoundingBox.LLy), 0, PositionTolerance);
            Assert.InRange(Math.Abs(expected[i].BoundingBox.Width() - actual[i].BoundingBox.Width()), 0, SizeTolerance);
            Assert.InRange(Math.Abs(expected[i].BoundingBox.Height() - actual[i].BoundingBox.Height()), 0, SizeTolerance);
        }
    }

    private sealed record ExtractedWord(string Text, PdfRect<double> BoundingBox);
}
