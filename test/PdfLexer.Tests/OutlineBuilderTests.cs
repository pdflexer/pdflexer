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

    [Fact]
    public void It_should_build_tree_structure_from_sections()
    {
        var doc = PdfDocument.Create();
        var p1 = doc.AddPage();
        var p2 = doc.AddPage();
        
        p1.AddBookmark("Chapter 1");
        p2.AddBookmark("Section 1.1", null, "Chapter 1");
        
        var builder = new OutlineBuilder(doc);
        var tree = builder.BuildTree(builder.Aggregate());
        
        Assert.Single(tree.Children);
        var c1 = tree.Children[0];
        Assert.Equal("Chapter 1", c1.Title);
        Assert.Single(c1.Children);
        Assert.Equal("Section 1.1", c1.Children[0].Title);
    }

    [Fact]
    public void It_should_sort_outlines_correctly()
    {
        var doc = PdfDocument.Create();
        var p1 = doc.AddPage(); // Index 0
        var p2 = doc.AddPage(); // Index 1
        
        p1.AddBookmark("Second", order: 10);
        p2.AddBookmark("First", order: 5);
        p2.AddBookmark("Third");
        
        // Aggregated (page order): [Second (10, p0), First (5, p1), Third (null, p1)]
        // Expected Sorted:
        // 1. First (5, p1)
        // 2. Second (10, p0)
        // 3. Third (null, p1)
        
        var builder = new OutlineBuilder(doc);
        var tree = builder.BuildTree(builder.Aggregate());
        
        Assert.Equal(3, tree.Children.Count);
        Assert.Equal("First", tree.Children[0].Title);
        Assert.Equal("Second", tree.Children[1].Title);
        Assert.Equal("Third", tree.Children[2].Title);
    }

    [Fact]
    public void It_should_convert_tree_to_pdf_objects()
    {
        var doc = PdfDocument.Create();
        var p1 = doc.AddPage();
        p1.AddBookmark("Chapter 1");
        
        var builder = new OutlineBuilder(doc);
        var tree = builder.BuildTree(builder.Aggregate());
        
        var root = builder.ConvertToPdf(tree);
        
        Assert.NotNull(root);
        var rootObj = root.GetPdfObject();
        Assert.Equal(PdfName.Outlines, rootObj.Get<PdfName>(PdfName.TypeName));
        
        var first = root.First;
        Assert.NotNull(first);
        Assert.Equal("Chapter 1", first.Title);
    }
}
