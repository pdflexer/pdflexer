using PdfLexer.Content;
using PdfLexer.Fonts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PdfLexer.Tests;

public class WritingTests
{
    [Fact]
    public void It_PrePends_Data()
    {
        using var doc = PdfDocument.Create();
        var pg = doc.AddPage();
        {
            using var writer = pg.GetWriter();
            writer
                .Save()
                .Font(Writing.Base14.TimesRoman, 12)
                .Text("Hello World")
                .Restore();
        }

        var first = doc.Save();
        Assert.Empty(SyntaxValidation.Validate(first));

        {
            using var writer = pg.GetWriter(Writing.PageWriteMode.Pre);
            writer
                .Save()
                .Font(Writing.Base14.TimesRoman, 12)
                .Text("New")
                .Restore();
        }

        var second = doc.Save();
        Assert.Empty(SyntaxValidation.Validate(second));
    }

    [Fact]
    public void It_Writes_Text_Box()
    {
        using var doc = PdfDocument.Create();
        var pg = doc.AddPage();
        {
            using var writer = pg.GetWriter();
            writer
                .Save()
                .Font(Writing.Base14.TimesRoman, 12)
                .TextBox(new Content.PdfRect<double>(10, 10, 100, 100), Writing.TextAlign.Center)
                .TextBoxWrite("Hello\nWorld")
                .TextBoxComplete();
            writer.Restore();
        }

        var first = doc.Save();
        Assert.Empty(SyntaxValidation.Validate(first));

        var dict = new Dictionary<string, double>();
        var words = pg.GetWordScanner();
        while (words.Advance())
        {
            dict[words.CurrentWord] = words.GetInfo().BoundingBox.LLy;
        }
        Assert.Contains("Hello", dict.Keys);
        Assert.Contains("World", dict.Keys);
        Assert.True(dict["Hello"] > dict["World"]);
    }

    [Fact]
    public void It_Writes_MultiByte_Text_Box()
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
                .TextBox(new Content.PdfRect<double>(10, 10, 100, 100), Writing.TextAlign.Center)
                .TextBoxWrite("Hello\nWorld")
                .TextBoxComplete();
            writer.Restore();
        }

        var first = doc.Save();
        Assert.Empty(SyntaxValidation.Validate(first));

        var dict = new Dictionary<string, double>();
        var words = pg.GetWordScanner();
        while (words.Advance())
        {
            dict[words.CurrentWord] = words.GetInfo().BoundingBox.LLy;
        }
        Assert.Contains("Hello", dict.Keys);
        Assert.Contains("World", dict.Keys);
        Assert.True(dict["Hello"] > dict["World"]);
    }

    [Fact]
    public void It_Writes_Text_Box_Center()
    {
        using var doc = PdfDocument.Create();

        var dx = 500;
        var dy = 500;
        var pg = doc.AddPage();
        {
            using var writer = pg.GetWriter();
            writer
                .Save()
                .Font(Writing.Base14.TimesRoman, 12)
                .TextMove(dx, dy)
                .TextBox(100, 100, Writing.TextAlign.Center)
                .TextBoxWrite("Hello")
                .TextBoxComplete(Writing.TextLayout.VerticalAlign.Center);
            writer.Restore();
        }

        var first = doc.Save();
        Assert.Empty(SyntaxValidation.Validate(first));

        var dict = new Dictionary<string, PdfRect<double>>();
        var words = pg.GetWordScanner();
        while (words.Advance())
        {
            dict[words.CurrentWord] = words.GetInfo().BoundingBox;
        }
        Assert.Contains("Hello", dict.Keys);
        var bb = dict["Hello"];
        var x = (100 - bb.Width()) / 2.0 + dx;
        Assert.True(Math.Abs(x - bb.LLx) < 1);
        var y = (100 - bb.Height()) / 2.0 + dy;
        Assert.True(Math.Abs(y - bb.LLy) < 5);
    }


    [Fact]
    public void It_Writes_Text_Box_LB()
    {
        using var doc = PdfDocument.Create();

        var dx = 500;
        var dy = 500;
        var pg = doc.AddPage();
        {
            using var writer = pg.GetWriter();
            writer
                .Save()
                .Font(Writing.Base14.TimesRoman, 12)
                .TextMove(dx, dy)
                .TextBox(100, 100, Writing.TextAlign.Left)
                .TextBoxWrite("Hello")
                .TextBoxComplete(Writing.TextLayout.VerticalAlign.Bottom);
            writer.Restore();
        }

        var first = doc.Save();
        Assert.Empty(SyntaxValidation.Validate(first));

        var dict = new Dictionary<string, PdfRect<double>>();
        var words = pg.GetWordScanner();
        while (words.Advance())
        {
            dict[words.CurrentWord] = words.GetInfo().BoundingBox;
        }
        Assert.Contains("Hello", dict.Keys);
        var bb = dict["Hello"];
        var x = dx;
        Assert.True(Math.Abs(x - bb.LLx) < 1);
        var y = dy;
        Assert.True(Math.Abs(y - bb.LLy) < 5);
    }

    [Fact]
    public void It_Writes_Text_Box_RT()
    {
        using var doc = PdfDocument.Create();

        var dx = 500;
        var dy = 500;
        var pg = doc.AddPage();
        {
            using var writer = pg.GetWriter();
            writer
                .Save()
                .Font(Writing.Base14.TimesRoman, 12)
                .TextMove(dx, dy)
                .TextBox(100, 100, Writing.TextAlign.Right)
                .TextBoxWrite("Hello")
                .TextBoxComplete(Writing.TextLayout.VerticalAlign.Top);
            writer.Restore();
        }

        var first = doc.Save();
        Assert.Empty(SyntaxValidation.Validate(first));

        var dict = new Dictionary<string, PdfRect<double>>();
        var words = pg.GetWordScanner();
        while (words.Advance())
        {
            dict[words.CurrentWord] = words.GetInfo().BoundingBox;
        }
        Assert.Contains("Hello", dict.Keys);
        var bb = dict["Hello"];
        var x = dx + 100 - bb.Width();
        Assert.True(Math.Abs(x - bb.LLx) < 1);
        var y = dy + 100 - bb.Height();
        Assert.True(Math.Abs(y - bb.URy) < 5);
    }
}
