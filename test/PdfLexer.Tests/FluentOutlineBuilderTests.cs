using PdfLexer.DOM;
using Xunit;

namespace PdfLexer.Tests;

public class FluentOutlineBuilderTests
{
    [Fact]
    public void Builder_should_allow_fluent_chaining()
    {
        var builder = new OutlineBuilder();
        builder.AddSection("Section 1")
               .AddBookmark("Bookmark 1.1")
               .AddBookmark("Bookmark 1.2")
               .MoveToParent()
               .AddSection("Section 2")
               .AddBookmark("Bookmark 2.1");

        var root = builder.GetRoot();
        Assert.Equal(2, root.Children.Count);
        Assert.Equal("Section 1", root.Children[0].Title);
        Assert.Equal(2, root.Children[0].Children.Count);
        Assert.Equal("Section 2", root.Children[1].Title);
        Assert.Single(root.Children[1].Children);
    }
}
