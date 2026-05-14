using PdfLexer.Fonts;
using PdfLexer.DOM;
using PdfLexer.Writing;
using System.IO;
using System.Linq;
using Xunit;

namespace PdfLexer.Tests;

public class AccessibilityTextOutputTests
{
    [Fact]
    public void Tagged_Standard14_Text_Emits_ToUnicode_And_Remains_Extractable()
    {
        using var doc = PdfDocument.Create();
        doc.ApplyAccessibilitySetup("en-US", "Standard14", strictConformance: false);
        var page = doc.AddPage();
        var paragraph = doc.Structure.AddParagraph("Hello tagged");

        using (var writer = page.GetWriter())
        {
            writer.BeginMarkedContent(paragraph.GetNode());
            writer.Font(Base14.Helvetica, 12).Text("Hello tagged");
            writer.EndMarkedContent();
        }

        var bytes = doc.Save();
        using var saved = PdfDocument.Open(bytes);
        var font = GetFirstFont(saved.Pages[0]);
        Assert.NotNull(font.Get<PdfStream>(PdfName.ToUnicode));
        var extracted = ExtractTextWithPdfPig(bytes);
        Assert.Contains("Hello", extracted);
        Assert.Contains("tagged", extracted);
    }

    [Fact]
    public void Tagged_TrueTypeSimple_Text_Emits_ToUnicode_And_Remains_Extractable()
    {
        var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
        var fontPath = Path.Combine(tp, "Roboto-Regular.ttf");
        var font = TrueTypeFont.CreateWritableFont(File.ReadAllBytes(fontPath));

        using var doc = PdfDocument.Create();
        doc.ApplyAccessibilitySetup("en-US", "TrueType Simple");
        var page = doc.AddPage();
        var paragraph = doc.Structure.AddParagraph("Hello simple");

        using (var writer = page.GetWriter())
        {
            writer.BeginMarkedContent(paragraph.GetNode());
            writer.Font(font, 12).Text("Hello simple");
            writer.EndMarkedContent();
        }

        var bytes = doc.Save();
        using var saved = PdfDocument.Open(bytes);
        var savedFont = GetFirstFont(saved.Pages[0]);
        Assert.NotNull(savedFont.Get<PdfStream>(PdfName.ToUnicode));
        Assert.Contains("Hello simple", ExtractTextWithPdfPig(bytes));
    }

    [Fact]
    public void Tagged_TrueTypeSimple_Text_Promotes_To_Type0_For_NonAscii()
    {
        var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
        var fontPath = Path.Combine(tp, "Roboto-Regular.ttf");
        var font = TrueTypeFont.CreateWritableFont(File.ReadAllBytes(fontPath));

        using var doc = PdfDocument.Create();
        doc.ApplyAccessibilitySetup("en-US", "Promotion");
        var page = doc.AddPage();
        var paragraph = doc.Structure.AddParagraph("Hello Phi");

        using (var writer = page.GetWriter())
        {
            writer.BeginMarkedContent(paragraph.GetNode());
            writer.Font(font, 12).Text("Hello Œ");
            writer.EndMarkedContent();
        }

        var bytes = doc.Save();
        using var saved = PdfDocument.Open(bytes);
        var fonts = saved.Pages[0].Resources.Get<PdfDictionary>(PdfName.Font)!;
        Assert.Contains(fonts.Values.Select(x => x.Resolve().GetAs<PdfDictionary>()), x => x.Get<PdfName>(PdfName.Subtype) == PdfName.Type0);
        Assert.Contains("Hello", ExtractTextWithPdfPig(bytes));
        Assert.Contains("Œ", ExtractTextWithPdfPig(bytes));
    }

    [Fact]
    public void Tagged_Standard14_Text_Throws_Typed_Exception_For_NonEncodable_Text()
    {
        using var doc = PdfDocument.Create();
        doc.ApplyAccessibilitySetup("en-US", "Standard14 Error", strictConformance: false);
        var page = doc.AddPage();
        var paragraph = doc.Structure.AddParagraph("Hello");

        using var writer = page.GetWriter();
        writer.BeginMarkedContent(paragraph.GetNode());
        writer.Font(Base14.Helvetica, 12);

        var ex = Assert.Throws<PdfAccessibilityTextException>(() => writer.Text("Hello €"));
        Assert.Contains("Type 0-capable font", ex.Message);
    }

    [Fact]
    public void Tagged_Type0_Text_Remains_Extractable()
    {
        var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
        var fontPath = Path.Combine(tp, "Roboto-Regular.ttf");
        var font = TrueTypeFont.CreateType0WritableFont(File.ReadAllBytes(fontPath));

        using var doc = PdfDocument.Create();
        doc.ApplyAccessibilitySetup("en-US", "Type0");
        var page = doc.AddPage();
        var paragraph = doc.Structure.AddParagraph("Hello Type0");

        using (var writer = page.GetWriter())
        {
            writer.BeginMarkedContent(paragraph.GetNode());
            writer.Font(font, 12).Text("Hello Œ");
            writer.EndMarkedContent();
        }

        var bytes = doc.Save();
        using var saved = PdfDocument.Open(bytes);
        var savedFont = GetFirstFont(saved.Pages[0]);
        Assert.Equal(PdfName.Type0, savedFont.Get<PdfName>(PdfName.Subtype));
        Assert.NotNull(savedFont.Get<PdfStream>(PdfName.ToUnicode));
        var extracted = ExtractTextWithPdfPig(bytes);
        Assert.Contains("Hello", extracted);
        Assert.Contains("Œ", extracted);
    }

    private static PdfDictionary GetFirstFont(PdfPage page)
    {
        return page.Resources.Get<PdfDictionary>(PdfName.Font)!.Values.First().Resolve().GetAs<PdfDictionary>();
    }

    private static string ExtractTextWithPdfPig(byte[] bytes)
    {
        using var doc = UglyToad.PdfPig.PdfDocument.Open(bytes);
        return string.Join("\n", doc.GetPages().Select(x => x.Text));
    }
}
