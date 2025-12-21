using PdfLexer.DOM;
using Xunit;
using System.Collections.Generic;

namespace PdfLexer.Tests;

public class PageOutlineTests
{
    [Fact]
    public void It_should_hold_simple_outlines()
    {
        var outline = new PdfOutline
        {
            Title = "Chapter 1",
            Order = 1
        };

        Assert.Equal("Chapter 1", outline.Title);
        Assert.Equal(1, outline.Order);
        Assert.Null(outline.Section);
    }

    [Fact]
    public void It_should_hold_nested_outlines()
    {
        var outline = new PdfOutline
        {
            Title = "Section 1.1",
            Section = new List<string> { "Chapter 1" }
        };

        Assert.Equal("Section 1.1", outline.Title);
        Assert.Single(outline.Section);
        Assert.Equal("Chapter 1", outline.Section[0]);
    }

    [Fact]
    public void Page_should_hold_pdf_outlines()
    {
        var page = new PdfPage();
        var outline = new PdfOutline { Title = "Test" };
        
        // This will fail compilation if Outlines is List<PdfOutlineItem>
        page.Outlines.Add(outline);
        
        Assert.Single(page.Outlines);
        Assert.IsType<PdfOutline>(page.Outlines[0]);
        Assert.Equal("Test", page.Outlines[0].Title);
    }

    [Fact]
    public void AddBookmark_helper_should_work()
    {
        var page = new PdfPage();
        page.AddBookmark("Chapter 1");
        page.AddBookmark("Section 1.1", "Chapter 1");
        
        Assert.Equal(2, page.Outlines.Count);
        Assert.Equal("Chapter 1", page.Outlines[0].Title);
        Assert.Null(page.Outlines[0].Section);
        
        Assert.Equal("Section 1.1", page.Outlines[1].Title);
        Assert.Single(page.Outlines[1].Section);
        Assert.Equal("Chapter 1", page.Outlines[1].Section[0]);
    }
}
