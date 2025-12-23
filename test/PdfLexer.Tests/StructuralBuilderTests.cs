using PdfLexer.DOM;
using Xunit;
using System.Linq;

namespace PdfLexer.Tests;

public class StructuralBuilderTests
{
    [Fact]
    public void Builder_should_create_hierarchical_tree()
    {
        var builder = new StructuralBuilder();
        builder.AddElement("Part")
               .AddHeader(1, "Main Header").Back()
               .AddParagraph("Intro paragraph").Back()
               .AddElement("Section")
                   .AddParagraph("Section content");

        var root = builder.GetRoot();
        Assert.Equal("Document", root.Type);
        Assert.Single(root.Children);
        
        var part = root.Children[0];
        Assert.Equal("Part", part.Type);
        Assert.Equal(3, part.Children.Count);
        
        Assert.Equal("H1", part.Children[0].Type);
        Assert.Equal("Main Header", part.Children[0].Title);
        
        Assert.Equal("P", part.Children[1].Type);
        Assert.Equal("Intro paragraph", part.Children[1].Title);
        
        Assert.Equal("Section", part.Children[2].Type);
        Assert.Single(part.Children[2].Children);
        Assert.Equal("P", part.Children[2].Children[0].Type);
        Assert.Equal("Section content", part.Children[2].Children[0].Title);
    }

    [Fact]
    public void Builder_should_integrate_with_OutlineBuilder()
    {
        var structBuilder = new StructuralBuilder();
        var outlineBuilder = new OutlineBuilder();
        
        structBuilder.AddHeader(1, "Chapter 1")
                     .CreateBookmark("Bookmark to Ch1", outlineBuilder);
                     
        var root = structBuilder.GetRoot();
        var h1 = root.Children[0];
        
        var outlineRoot = outlineBuilder.GetRoot();
        Assert.Single(outlineRoot.Children);
        Assert.Equal("Bookmark to Ch1", outlineRoot.Children[0].Title);
        Assert.Same(h1, outlineRoot.Children[0].StructureElement);
    }
}
