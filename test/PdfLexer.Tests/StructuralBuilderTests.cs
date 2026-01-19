using PdfLexer.DOM;
using Xunit;
using System.Linq;
using PdfLexer.Writing;

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
        var outlineBuilder = new PdfLexer.DOM.OutlineBuilder();
        
        structBuilder.AddHeader(1, "Chapter 1")
                     .CreateBookmark("Bookmark to Ch1", outlineBuilder);
                     
        var root = structBuilder.GetRoot();
        var h1 = root.Children[0];
        
        var outlineRoot = outlineBuilder.GetRoot();
        Assert.Single(outlineRoot.Children);
        Assert.Equal("Bookmark to Ch1", outlineRoot.Children[0].Title);
        Assert.Same(h1, outlineRoot.Children[0].StructureElement);
    }

    [Fact]
    public void Builder_should_support_all_standard_types()
    {
        var builder = new StructuralBuilder();
        builder.AddPart("Part")
               .AddSection("Section").Back()
               .AddDiv("Div").Back()
               .AddBlockQuote("Quote").Back()
               .AddCaption("Caption").Back()
               .AddTOC("TOC")
                   .AddTOCI("Item 1").Back()
               .Back()
               .AddList("List")
                   .AddListItem("Item")
                       .AddLabel("1.").Back()
                       .AddListBody("Content").Back()
                   .Back()
               .Back()
               .AddTable("Table")
                   .AddTableHead()
                       .AddRow()
                           .AddHeaderCell().Back()
                       .Back()
                   .Back()
                   .AddTableBody()
                       .AddRow()
                           .AddDataCell().Back()
                       .Back()
                   .Back()
                   .AddTableFoot().Back()
               .Back()
               .AddParagraph("P")
                   .AddSpan("Span", "en-US").Back()
                   .AddQuote("InlineQuote").Back()
                   .AddCode("Code").Back()
                   .AddFormula("Formula", "a=b").Back()
                   .AddReference("Ref").Back()
                   .AddNote("Note").Back()
                   .AddFigure("Fig", "Alt Text").Back()
                   .AddLink("Link", "Link Alt").Back()
               .Back();

        var root = builder.GetRoot();
        Assert.Equal("Document", root.Type);
        
        // Root has one child: Part
        Assert.Single(root.Children);
        var part = root.Children[0];
        Assert.Equal("Part", part.Type);

        // Verify children of Part
        var children = part.Children;
        Assert.Contains(children, c => c.Type == "Sect");
        Assert.Contains(children, c => c.Type == "Div");
        Assert.Contains(children, c => c.Type == "BlockQuote");
        Assert.Contains(children, c => c.Type == "Caption");
        Assert.Contains(children, c => c.Type == "TOC");
        Assert.Contains(children, c => c.Type == "L");
        Assert.Contains(children, c => c.Type == "Table");
        Assert.Contains(children, c => c.Type == "P");

        // Verify specific attributes
        var p = children.First(c => c.Type == "P");
        Assert.Contains(p.Children, c => c.Type == "Formula" && c.AlternateText == "a=b");
        Assert.Contains(p.Children, c => c.Type == "Figure" && c.AlternateText == "Alt Text");
        Assert.Contains(p.Children, c => c.Type == "Link" && c.AlternateText == "Link Alt");
        Assert.Contains(p.Children, c => c.Type == "Span" && c.Language == "en-US");
        Assert.Contains(p.Children, c => c.Type == "Link" && c.AlternateText == "Link Alt");
        Assert.Contains(p.Children, c => c.Type == "Span" && c.Language == "en-US");
    }

    [Fact]
    public void Builder_should_support_WriteContent()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        using var writer = new PageWriter<double>(page);

        var builder = new StructuralBuilder();
        builder.AddParagraph("My Paragraph")
               .WriteContent(writer, w => {
                   // simulates writing content
               });
        
        var root = builder.GetRoot();
        var p = root.Children[0];
        
        Assert.Single(p.ContentItems);
        Assert.Equal(0, p.ContentItems[0].MCID);
        Assert.Same(page, p.ContentItems[0].Page);
    }
}
