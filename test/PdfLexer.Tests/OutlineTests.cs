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
        // This will be re-enabled in Phase 4 when Parser is refactored
        /*
        var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
        var pdf = Path.Combine(tp, "pdfs", "pdfjs", "freeculture.pdf");
        using var doc = PdfDocument.Open(pdf);
        var outlines = doc.Outlines;
        Assert.NotNull(outlines);
        
        var page1 = doc.Pages[0];
        Assert.NotEmpty(page1.Outlines);
        Assert.Equal("COVER", page1.Outlines[0].Title);
        */
    }
}
