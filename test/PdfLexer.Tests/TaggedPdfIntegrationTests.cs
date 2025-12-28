using PdfLexer.DOM;
using PdfLexer.Fonts;
using PdfLexer.Writing;
using Xunit;
using System.IO;
using System.Linq;
using System;

namespace PdfLexer.Tests;

public class TaggedPdfIntegrationTests
{
    [Fact]
    public void MultiPage_TaggedPdf_Should_Be_Valid()
    {
        var doc = PdfDocument.Create();
        var sb = doc.Structure;
        var ob = new PdfLexer.DOM.OutlineBuilder();
        doc.Outlines = ob.GetRoot();

        var part = sb.AddElement("Part", "Main Document");
        
        // Page 1
        var page1 = doc.AddPage();
        var h1 = part.AddHeader(1, "Page 1 Header");
        h1.CreateBookmark("Jump to Page 1", ob);
        
        using (var writer = page1.GetWriter())
        {
            writer.BeginMarkedContent(h1.GetNode());
            writer.Font(Base14.Helvetica, 18).TextMove(50, 750).Text("Page 1 Header Content");
            writer.EndMarkedContent();
            
            var p1 = h1.Back().AddParagraph("Intro on Page 1");
            writer.BeginMarkedContent(p1.GetNode());
            writer.Font(Base14.Helvetica, 12).TextMove(0, -30).Text("This is a paragraph on page 1.");
            writer.EndMarkedContent();
        }

        // Page 2
        var page2 = doc.AddPage();
        var h2 = part.AddHeader(2, "Page 2 Header");
        h2.CreateBookmark("Jump to Page 2", ob);
        
        using (var writer = page2.GetWriter())
        {
            writer.BeginMarkedContent(h2.GetNode());
            writer.Font(Base14.Helvetica, 16).TextMove(50, 750).Text("Page 2 Sub-header Content");
            writer.EndMarkedContent();
            
            var p2 = h2.Back().AddParagraph("Content on Page 2");
            writer.BeginMarkedContent(p2.GetNode());
            writer.Font(Base14.Helvetica, 12).TextMove(0, -30).Text("This is more content but on page 2.");
            writer.EndMarkedContent();
        }


        Util.Save(doc, "MultiPage_TaggedPdf_Should_Be_Valid");

        var bytes = doc.Save();
        
        // Verification
        var reDoc = PdfDocument.Open(bytes);
        Assert.NotNull(reDoc.Catalog[PdfName.StructTreeRoot]);
        
        var markInfo = reDoc.Catalog.Get<PdfDictionary>(PdfName.MarkInfo);
        Assert.NotNull(markInfo);
        Assert.Equal(PdfBoolean.True, markInfo[PdfName.Marked]);

        var structRoot = reDoc.Catalog.Get<PdfDictionary>(PdfName.StructTreeRoot);
        // StructTreeRoot uses K, not Kids
        var k = structRoot[PdfName.K];
        Assert.NotNull(k);
        
        // Check ParentTree
        var parentTree = structRoot.Get<PdfDictionary>(PdfName.ParentTree);
        Assert.NotNull(parentTree);
        var nums = parentTree.Get<PdfArray>(PdfName.Nums);
        Assert.NotNull(nums);
        Assert.Equal(4, nums.Count); // 2 pages, each with (index, array)

        // Verify Page 1 StructParents
        var rePage1 = reDoc.Pages[0];
        Assert.True(rePage1.NativeObject.ContainsKey(PdfName.StructParents));
        
        // Verify Page 2 StructParents
        var rePage2 = reDoc.Pages[1];
        Assert.True(rePage2.NativeObject.ContainsKey(PdfName.StructParents));

        // Verify Bookmarks
        var outlines = reDoc.Catalog.Get<PdfDictionary>(PdfName.Outlines);
        Assert.NotNull(outlines);
        
        var first = outlines.Get<PdfDictionary>(PdfName.First);
        Assert.NotNull(first);
        Assert.Equal("Jump to Page 1", first.Get<PdfString>(PdfName.Title).Value);
        
        if (!first.ContainsKey(PdfName.SE))
        {
             var keys = string.Join(", ", first.Keys.Select(x => x.Value));
             throw new Exception("First bookmark missing /SE. Keys present: " + keys);
        }
        
        var se = first[PdfName.SE];
        if (se == null)
        {
             throw new Exception("/SE key present but value is C# null");
        }
        
        // se might be an IndirectRef, Resolve it
        var seDict = se.Resolve() as PdfDictionary;
        Assert.NotNull(seDict);
        Assert.Equal(PdfName.StructElem, seDict[PdfName.TYPE]);

        var last = outlines.Get<PdfDictionary>(PdfName.Last);
        Assert.NotNull(last);
        Assert.Equal("Jump to Page 2", last.Get<PdfString>(PdfName.Title).Value);
        Assert.NotNull(last[PdfName.SE]);
    }
}
