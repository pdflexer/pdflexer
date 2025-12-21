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
}