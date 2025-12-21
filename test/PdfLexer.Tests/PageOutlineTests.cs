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
}