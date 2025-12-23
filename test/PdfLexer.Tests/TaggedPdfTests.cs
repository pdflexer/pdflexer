using PdfLexer.DOM;
using PdfLexer.Writing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace PdfLexer.Tests;

public class TaggedPdfTests
{
    [Fact]
    public void Create_Basic_Tagged_Pdf()
    {
        using var doc = PdfDocument.Create();
        var page1 = doc.AddPage();
        var page2 = doc.AddPage();

        var root = doc.Structure;
        Assert.NotNull(root);
        var p1 = root.AddParagraph("Page 1 Text");
        var p2 = root.AddParagraph("Page 2 Text");
        Assert.Equal(2, root.GetRoot().Children.Count);

        using (var writer = page1.GetWriter())
        {
            writer.BeginMarkedContent(p1.GetNode()); // P1
            writer.Font(Base14.Helvetica, 12).TextMove(100, 700).Text("Hello Page 1");
            writer.EndMarkedContent();
        }

        using (var writer = page2.GetWriter())
        {
            writer.BeginMarkedContent(p2.GetNode()); // P2
            writer.Font(Base14.Helvetica, 12).TextMove(100, 700).Text("Hello Page 2");
            writer.EndMarkedContent();
        }

        var data = doc.Save();
        
        // Re-open and verify
        using var doc2 = PdfDocument.Open(data);
        var catalog = doc2.Catalog;
        
        Assert.True(catalog.ContainsKey(PdfName.MarkInfo));
        var markInfo = catalog.Get<PdfDictionary>(PdfName.MarkInfo);
        Assert.NotNull(markInfo);
        Assert.Equal(PdfBoolean.True, markInfo[PdfName.Marked]);

        Assert.True(catalog.ContainsKey(PdfName.StructTreeRoot));
        var structTreeRoot = catalog.Get<PdfDictionary>(PdfName.StructTreeRoot);
        Assert.NotNull(structTreeRoot);

        // Verify page 1 and 2 have StructParents
        Assert.Equal(0, (PdfIntNumber)doc2.Pages[0].NativeObject[PdfName.StructParents]);
        Assert.Equal(1, (PdfIntNumber)doc2.Pages[1].NativeObject[PdfName.StructParents]);
    }
}
