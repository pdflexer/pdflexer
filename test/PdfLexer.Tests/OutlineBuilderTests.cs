using PdfLexer.DOM;
using PdfLexer.Writing;
using Xunit;
using System.Collections.Generic;

namespace PdfLexer.Tests;

public class OutlineBuilderTests
{
    [Fact]
    public void It_should_aggregate_outlines_from_pages()
    {
        var doc = PdfDocument.Create();
        var p1 = doc.AddPage();
        var p2 = doc.AddPage();
        
        p1.AddBookmark("B1");
        p2.AddBookmark("B2");
        
        // This will fail compilation initially
        var builder = new OutlineBuilder(doc);
        var aggregated = builder.Aggregate();
        
        Assert.Equal(2, aggregated.Count);
        Assert.Equal("B1", aggregated[0].Outline.Title);
        Assert.Equal(0, aggregated[0].PageIndex);
        
        Assert.Equal("B2", aggregated[1].Outline.Title);
        Assert.Equal(1, aggregated[1].PageIndex);
    }
}
