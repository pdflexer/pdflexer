using pdflexer.PdfiumRegressionTester;
using PdfLexer.Content;
using PdfLexer.DOM;
using System.IO;
using Xunit;

namespace PdfLexer.Tests;

public class PageToFormTests
{
    [Fact]
    public void It_Creates_From_Simple_Page()
    {
        using var doc = PdfDocument.Create();
        var pg = doc.AddPage();
        {
            using var writer = pg.GetWriter();
            writer.Font(Writing.Base14.TimesRoman, 10)
                .TextMove(100, 100)
                .Text("Text");
        }

        RunScenario(pg);
    }

    [Fact]
    public void It_Creates_From_Positive_CropBox()
    {
        using var doc = PdfDocument.Create();
        var pg = doc.AddPage();
        pg.CropBox = PdfRectangle.FromContentModel(new PdfRect<double>(100, 100, 200, 200));
        {
            using var writer = pg.GetWriter();
            writer.Font(Writing.Base14.TimesRoman, 10)
                .TextMove(100, 100)
                .Text("Text");
        }

        RunScenario(pg);
    }


    [Fact]
    public void It_Creates_From_Negative_CropBox()
    {
        using var doc = PdfDocument.Create();
        var pg = doc.AddPage();
        pg.CropBox = PdfRectangle.FromContentModel(new PdfRect<double>(-100, -100, 200, 200));
        {
            using var writer = pg.GetWriter();
            writer.Font(Writing.Base14.TimesRoman, 10)
                .TextMove(100, 100)
                .Text("Text");
        }

        RunScenario(pg);
    }

    [InlineData(-270)]
    [InlineData(-180)]
    [InlineData(-90)]
    [InlineData(0)]
    [InlineData(90)]
    [InlineData(180)]
    [InlineData(270)]
    [Theory]
    public void It_Creates_From_Rotated(int rotation)
    {
        using var doc = PdfDocument.Create();
        var pg = doc.AddPage();
        pg.Rotate = rotation;
        {
            using var writer = pg.GetWriter();
            writer.Font(Writing.Base14.TimesRoman, 10)
                .TextMove(100, 100)
                .Text("Text");
        }

        var w = pg.CropBox.Width;
        var h = pg.CropBox.Height;
        if (rotation % 180 != 0)
        {
            var t = w;
            w = h;
            h = t;
        }

        RunVisualScenario("form_rotation_" + rotation, pg, w, h);
    }

    [InlineData(0)]
    [InlineData(90)]
    [InlineData(180)]
    [InlineData(270)]
    [Theory]
    public void It_Creates_From_Rotated_CropBox(int rotation)
    {
        using var doc = PdfDocument.Create();
        var pg = doc.AddPage();
        pg.CropBox = PdfRectangle.FromContentModel(new PdfRect<double>(100, 100, 400, 400));
        pg.Rotate = rotation;
        {
            using var writer = pg.GetWriter();
            writer.Font(Writing.Base14.TimesRoman, 10)
                .TextMove(100, 100)
                .Text("Text");
        }

        var w = pg.CropBox.Width;
        var h = pg.CropBox.Height;
        if (rotation % 180 != 0)
        {
            var t = w;
            w = h;
            h = t;
        }

        RunVisualScenario("form_rotation_cb_" + rotation, pg, w, h);
    }

    private void RunVisualScenario(string nm, PdfPage pg, double w, double h)
    {
        var root = PathUtil.GetPathFromSegmentOfCurrent("PdfLexer.Tests");
        var output = Path.Combine(root, "bin", "results");
        Directory.CreateDirectory(output);
        var b = Path.Combine(root, "bin", "results", nm + "_b.pdf");
        var c = Path.Combine(root, "bin", "results", nm + "_c.pdf");
        {
            using var doc = PdfDocument.Create();
            doc.Pages.Add(pg);
            using var fo = File.Create(b);
            doc.SaveTo(fo);
        }

        var pg2 = new PdfPage(w, h);
        {
            var xf = XObjForm.FromPage(pg);
            using var writer = pg2.GetWriter();
            writer.Form(xf, 0, 0);
        }
        {
            using var doc = PdfDocument.Create();
            doc.Pages.Add(pg2);
            using var fo = File.Create(c);
            doc.SaveTo(fo);
        }

        var comparer = new Compare(output + "\\" + nm);
        var result = comparer.CompareAllPages(b, c);
        Assert.False(result[0].HadChanges);
    }


    private void RunScenario(PdfPage pg)
    {
        PdfRect<double> nbb;
        GfxMatrix<double> ctm;
        {
            var scanner = pg.GetWordScanner();
            scanner.Advance();
            var w = scanner.CurrentWord;
            var bb = scanner.GetWordBoundingBox();
            ctm = scanner.GraphicsState.CTM;
            nbb = bb.NormalizeTo(pg.CropBox);
        }

        var pg2 = new PdfPage(PageSize.LETTER);
        {
            var xf = XObjForm.FromPage(pg);
            using var writer = pg2.GetWriter();
            writer.Form(xf, 0, 0);
        }

        PdfRect<double> nbb2;
        GfxMatrix<double> ctm2;
        {
            var scanner = pg.GetWordScanner();
            scanner.Advance();
            var w = scanner.CurrentWord;
            var bb = scanner.GetWordBoundingBox();
            ctm2 = scanner.GraphicsState.CTM;
            nbb2 = bb.NormalizeTo(pg.CropBox);
        }

        Assert.Equal(nbb.LLx, nbb2.LLx);
        Assert.Equal(nbb.LLy, nbb2.LLy);

        // rotation
        Assert.Equal(ctm.B, ctm2.B);
        Assert.Equal(ctm.C, ctm2.C);
    }
}
