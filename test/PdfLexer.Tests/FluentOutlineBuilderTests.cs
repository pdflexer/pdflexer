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
               .AddBookmark("Sub 1")
               .Back()
               .AddBookmark("Root 2");

        var root = builder.GetRoot();
        Assert.Equal(2, root.Children.Count);
        Assert.Equal("Section 1", root.Children[0].Title);
        Assert.Equal("Root 2", root.Children[1].Title);
    }
}