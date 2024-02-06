using PdfLexer.Fonts;
using PdfLexer.Writing;
using System.IO;
using Xunit;

namespace PdfLexer.Tests;

public class FontLoadTests
{
    [Fact]
    public void It_Loads_TrueType()
    {
        var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
        var fontPath = Path.Combine(tp, "Roboto-Regular.ttf");

        var ttf = TrueTypeFont.CreateWritableFont(File.ReadAllBytes(fontPath));

        using var doc = PdfDocument.Create();
        var pg = doc.AddPage();
        {
            using var writer = pg.GetWriter();
            writer
                .Save()
                .Font(ttf, 12)
                .Text("Hello World")
                .Restore();
        }

        var first = doc.Save();
        Assert.Empty(SyntaxValidation.Validate(first));

        Assert.Contains("Hello World", pg.GetTextVisually());
    }

    [Fact]
    public void It_Loads_TrueType_Type0()
    {
        var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
        var fontPath = Path.Combine(tp, "Roboto-Regular.ttf");

        var ttf = TrueTypeFont.CreateType0WritableFont(File.ReadAllBytes(fontPath));

        using var doc = PdfDocument.Create();
        var pg = doc.AddPage();
        {
            using var writer = pg.GetWriter();
            writer
                .Save()
                .Font(ttf, 12)
                .Text("Hello World Φ")
                .Restore();
        }

        var first = doc.Save();
        Assert.Empty(SyntaxValidation.Validate(first));
        File.WriteAllBytes(tp + "/test.pdf", first);
        Assert.Contains("Hello World Φ", pg.GetTextVisually());
    }
}
