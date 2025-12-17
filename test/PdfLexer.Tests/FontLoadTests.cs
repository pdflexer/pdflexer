using PdfLexer.Content;
using PdfLexer.Fonts;
using PdfLexer.Writing;
using System.Collections.Generic;
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
        Assert.Contains("Hello World Φ", pg.GetTextVisually());
    }

    [Fact]
    public void It_Uses_Both_Types()
    {
        var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
        var fontPath = Path.Combine(tp, "Roboto-Regular.ttf");

        var ttf0 = TrueTypeFont.CreateType0WritableFont(File.ReadAllBytes(fontPath));
        var ttf1 = TrueTypeFont.CreateWritableFont(File.ReadAllBytes(fontPath));

        using var doc = PdfDocument.Create();
        var pg = doc.AddPage();
        {
            using var writer = pg.GetWriter();
            writer
                .Save()
                .Font(ttf0, 12)
                .Text("ABCDEFGHIJKLMNOP abcdefghijklmnop")
                .Restore()
                .Save()
                .Font(ttf1, 12)
                .Text("ABCDEFGHIJKLMNOP abcdefghijklmnop")
                .Restore();
        }

        var first = doc.Save();
        Assert.Empty(SyntaxValidation.Validate(first));
        var scanner = pg.GetTextScanner();
        var dict = new Dictionary<char, List<PdfRect<double>>>();
        while (scanner.Advance())
        {
            if (dict.TryGetValue(scanner.Glyph.Char, out var list))
            {
                list.Add(scanner.GetCurrentBoundingBox());
            }
            else
            {
                dict[scanner.Glyph.Char] = new List<PdfRect<double>> { scanner.GetCurrentBoundingBox() };
            }
        }

        foreach (var (c,v) in dict)
        {
            Assert.Equal(2, v.Count);
            var c1 = v[0];
            var c2 = v[1];
            Assert.Equal(c1.LLx, c2.LLx);
            Assert.Equal(c1.LLy, c2.LLy);
            Assert.Equal(c1.URx, c2.URx);
            Assert.Equal(c1.URy, c2.URy);
        }
    }

    [Fact]
    public void It_Doesnt_Duplicate_Fonts()
    {
        var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
        var fontPath = Path.Combine(tp, "Roboto-Regular.ttf");

        var ttf0 = TrueTypeFont.CreateType0WritableFont(File.ReadAllBytes(fontPath));
        var ttf1 = TrueTypeFont.CreateWritableFont(File.ReadAllBytes(fontPath));

        using var doc = PdfDocument.Create();
        var pg = doc.AddPage();
        {
            using var writer = pg.GetWriter();
            writer
                .Save()
                .Font(ttf0, 12)
                .Text("ABCDEFGHIJKLMNOP abcdefghijklmnop")
                .Restore()
                .Save()
                .Font(ttf1, 12)
                .Text("ABCDEFGHIJKLMNOP abcdefghijklmnop")
                .Restore();
        }
        var pg2 = doc.AddPage();
        {
            using var writer = pg2.GetWriter();
            writer
                .Save()
                .Font(ttf0, 12)
                .Text("ABCDEFGHIJKLMNOP abcdefghijklmnop")
                .Restore()
                .Save()
                .Font(ttf1, 12)
                .Text("ABCDEFGHIJKLMNOP abcdefghijklmnop")
                .Restore();
        }
        var data = doc.Save();
        using var doc2 = PdfDocument.Open(data);
        var p1f1 = doc2.Pages[0].Resources[PdfName.Font].GetAs<PdfDictionary>()["F1"].Resolve();
        var p1f2 = doc2.Pages[0].Resources[PdfName.Font].GetAs<PdfDictionary>()["F2"].Resolve();
        var p2f1 = doc2.Pages[1].Resources[PdfName.Font].GetAs<PdfDictionary>()["F1"].Resolve();
        var p2f2 = doc2.Pages[1].Resources[PdfName.Font].GetAs<PdfDictionary>()["F2"].Resolve();
        Assert.True(object.ReferenceEquals(p1f1, p2f1));
        Assert.True(object.ReferenceEquals(p1f2, p2f2));
    }
}
