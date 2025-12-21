using System;
using System.IO;
using System.Linq;
using PdfLexer.DOM;
using Xunit;

namespace PdfLexer.Tests;

public class OutlineTests
{
    [Fact]
    public void It_should_read_outlines()
    {
        var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
        var pdf = Path.Combine(tp, "pdfs", "pdfjs", "freeculture.pdf");
        using var doc = PdfDocument.Open(pdf);
        var outlines = doc.Outlines;
        Assert.NotNull(outlines);
        Assert.NotEmpty(outlines.Children);
        
        var first = outlines.First;
        Assert.NotNull(first);
        Assert.Equal("COVER", first.Title);
    }

    [Fact]
    public void It_should_create_outlines()
    {
        var doc = PdfDocument.Create();
        var page1 = doc.AddPage();
        var page2 = doc.AddPage();

        var root = new PdfOutlineRoot();
        doc.Outlines = root;
        
        var item1 = new PdfOutlineItem { Title = "Page 1" };
        item1.GetPdfObject()[PdfName.Dest] = new PdfArray { page1.NativeObject, PdfName.XYZ, new PdfNull(), new PdfNull(), new PdfNull() };
        
        var item2 = new PdfOutlineItem { Title = "Page 2" };
        item2.GetPdfObject()[PdfName.Dest] = new PdfArray { page2.NativeObject, PdfName.XYZ, new PdfNull(), new PdfNull(), new PdfNull() };

        var c1 = new PdfOutlineItem { Title = "Child 1" };
        c1.GetPdfObject()[PdfName.Dest] = new PdfArray { page1.NativeObject, PdfName.XYZ, new PdfNull(), new PdfNull(), new PdfNull() };
        
        item1.Add(c1);
        root.Add(item1);
        root.Add(item2);

        var bytes = doc.Save();
        
        using var doc2 = PdfDocument.Open(bytes);
        var outlines = doc2.Outlines;
        Assert.NotNull(outlines);
        Assert.Equal(2, outlines.Children.Count);
        
        var first = outlines.First;
        Assert.NotNull(first);
        Assert.Equal("Page 1", first.Title);
        Assert.Equal(1, first.Children.Count);
        
        var second = first.Next;
        Assert.NotNull(second);
        Assert.Equal("Page 2", second.Title);
    }
}
