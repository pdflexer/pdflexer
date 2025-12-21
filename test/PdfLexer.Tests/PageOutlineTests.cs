using System;
using System.Collections.Generic;
using System.Linq;
using PdfLexer.DOM;
using Xunit;

namespace PdfLexer.Tests;

public class PageOutlineTests
{
    [Fact]
    public void PdfPage_should_hold_bookmarks()
    {
        var page = new PdfPage();
        page.AddBookmark("Test");
        Assert.Single(page.Outlines);
        Assert.IsType<BookmarkNode>(page.Outlines[0]);
        Assert.Equal("Test", page.Outlines[0].Title);
    }
}