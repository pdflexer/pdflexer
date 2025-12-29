using PdfLexer.DOM;
using PdfLexer.Writing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace PdfLexer.Tests;

public class StructuralBookmarkTests
{
    [Fact]
    public void Create_Linked_Structural_Bookmark()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        
        var structure = doc.Structure;
        var p = structure.AddParagraph("Title Paragraph");
        
        var outlines = new PdfLexer.DOM.OutlineBuilder();
        p.CreateBookmark("Section 1", outlines);
        doc.Outlines = outlines.GetRoot();

        using (var writer = page.GetWriter())
        {
            writer.BeginMarkedContent(p.GetNode());
            writer.Font(Base14.Helvetica, 12).TextMove(100, 700).Text("Section 1 Content");
            writer.EndMarkedContent();
        }

        var data = doc.Save();
        
        // Re-open and verify
        using var doc2 = PdfDocument.Open(data);
        var catalog = doc2.Catalog;
        
        Assert.True(catalog.ContainsKey(PdfName.Outlines), "Catalog missing Outlines");
        var outlineObj = catalog[PdfName.Outlines];
        var outlineDict = outlineObj.Resolve().GetValue<PdfDictionary>();
        
        Assert.True(outlineDict.ContainsKey(PdfName.First), "Outlines missing First link");
        var firstObj = outlineDict[PdfName.First];
        var firstBookmark = firstObj.Resolve().GetValue<PdfDictionary>();
        
        Assert.Equal("Section 1", firstBookmark.GetRequiredValue<PdfString>(PdfName.Title).Value);
        Assert.True(firstBookmark.ContainsKey(PdfName.SE), "Bookmark missing /SE link");
        
        var seObj = firstBookmark[PdfName.SE];
        var seDict = seObj.Resolve().GetValue<PdfDictionary>();
        Assert.Equal(PdfName.P, seDict[PdfName.S]);
    }
}
