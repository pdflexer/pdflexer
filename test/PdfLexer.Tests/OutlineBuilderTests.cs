using PdfLexer.DOM;
using PdfLexer.Writing;
using Xunit;
using System.Collections.Generic;
using System.Linq;

namespace PdfLexer.Tests;

public class OutlineBuilderTests
{
    [Fact]
    public void Builder_should_allow_fluent_chaining()
    {
        var builder = new OutlineBuilder();
        builder.AddSection("Section 1")
               .AddBookmark("Bookmark 1.1")
               .AddBookmark("Bookmark 1.2")
               .Back()
               .AddSection("Section 2")
               .AddBookmark("Bookmark 2.1");
        
        var root = builder.GetRoot();
        Assert.Equal(2, root.Children.Count);
        Assert.Equal("Section 1", root.Children[0].Title);
        Assert.Equal(2, root.Children[0].Children.Count);
        Assert.Equal("Section 2", root.Children[1].Title);
        Assert.Single(root.Children[1].Children);
    }

    [Fact]
    public void Traversal_helpers_should_work()
    {
        var builder = new OutlineBuilder();
        builder.AddSection("S1")
               .AddBookmark("B1")
               .Back()
               .AddSection("S2")
               .AddBookmark("B2");
        
        var found = builder.FindNode("B2");
        Assert.NotNull(found);
        Assert.Equal("B2", found.Title);
        
        var leaves = builder.EnumerateLeaves().ToList();
        Assert.Equal(2, leaves.Count);
        Assert.Contains(leaves, x => x.Title == "B1");
        Assert.Contains(leaves, x => x.Title == "B2");
    }
}
