using System;
using System.IO;
using System.Linq;
using PdfLexer.DOM;
using Xunit;

namespace PdfLexer.Tests;

public class OutlineTests
{
    [Fact]
    public void It_should_read_outlines_and_attach_to_pages()
    {
        var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
        var pdf = Path.Combine(tp, "pdfs", "pdfjs", "freeculture.pdf");
        using var doc = PdfDocument.Open(pdf);
        var outlines = doc.Outlines;
        Assert.NotNull(outlines);
        
        var page1 = doc.Pages[0];
        Assert.NotEmpty(page1.Outlines);
        Assert.Equal("COVER", page1.Outlines[0].Title);
    }

    [Fact]
    public void It_should_save_outlines_to_pdf()
    {
        var doc = PdfDocument.Create();
        var p1 = doc.AddPage();
        p1.AddBookmark("Page 1");
        
        var bytes = doc.Save();
        
        using var doc2 = PdfDocument.Open(bytes);
        Assert.NotNull(doc2.Outlines); // Trigger parsing
        Assert.Single(doc2.Pages[0].Outlines);
        Assert.Equal("Page 1", doc2.Pages[0].Outlines[0].Title);
    }

    [Fact]
    public void Page_copying_should_preserve_outlines()
    {
        var doc = PdfDocument.Create();
        var p1 = doc.AddPage();
        p1.AddBookmark("Page 1 Bookmark", null, "Chapter A");
        
        var doc2 = PdfDocument.Create();
        doc2.Pages.Add(p1); 
        
        var bytes = doc2.Save();
        using var doc3 = PdfDocument.Open(bytes);
        var outlines = doc3.Outlines; // Trigger parsing
        Assert.NotNull(outlines);
        
        Assert.Single(doc3.Pages[0].Outlines);
        var outline = doc3.Pages[0].Outlines[0];
        Assert.Equal("Page 1 Bookmark", outline.Title);
        Assert.Single(outline.Section);
        Assert.Equal("Chapter A", outline.Section[0]);
    }

    [Fact]
    public void Mixed_ordering_should_work()
    {
        var doc = PdfDocument.Create();
        var p1 = doc.AddPage();
        var p2 = doc.AddPage();
        
        p2.AddBookmark("Explicit 2", 10);
        p1.AddBookmark("Explicit 1", 5);
        p1.AddBookmark("Implicit 1");
        p2.AddBookmark("Implicit 2");
        
        var bytes = doc.Save();
        using var doc2 = PdfDocument.Open(bytes);
        var outlines = doc2.Outlines;
        
        Assert.Equal(4, outlines.Children.Count);
        Assert.Equal("Explicit 1", outlines.Children[0].Title);
        Assert.Equal("Explicit 2", outlines.Children[1].Title);
        Assert.Equal("Implicit 1", outlines.Children[2].Title);
        Assert.Equal("Implicit 2", outlines.Children[3].Title);
    }

    [Fact]
    public void Deep_nesting_should_work()
    {
        var doc = PdfDocument.Create();
        var p1 = doc.AddPage();
        p1.AddBookmark("Level 4", null, "A", "B", "C");
        
        var bytes = doc.Save();
        using var doc2 = PdfDocument.Open(bytes);
        var outlines = doc2.Outlines;
        
        Assert.Single(outlines.Children);
        var a = outlines.First;
        Assert.Equal("A", a.Title);
        
        Assert.Single(a.Children);
        var b = a.First;
        Assert.Equal("B", b.Title);
        
        Assert.Single(b.Children);
        var c = b.First;
        Assert.Equal("C", c.Title);
        
        Assert.Single(c.Children);
        var d = c.First;
        Assert.Equal("Level 4", d.Title);
    }
}