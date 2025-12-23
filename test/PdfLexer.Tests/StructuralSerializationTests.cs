using PdfLexer.DOM;
using PdfLexer.Writing;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace PdfLexer.Tests;

public class StructuralSerializationTests
{
    [Fact]
    public void StructuralSerializer_Converts_Tree_To_Pdf()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        var root = new StructureNode { Type = "Document" };
        var p = new StructureNode { Type = "P" };
        root.Children.Add(p);
        p.ContentItems.Add((page, 0));

        var serializer = new StructuralSerializer();
        var result = serializer.ConvertToPdf(root);
        var structTreeRoot = result.Root;

        Assert.Equal(PdfName.StructTreeRoot, structTreeRoot[PdfName.TYPE]);
        
        var rootElem = structTreeRoot.Get<PdfDictionary>(PdfName.Kids);
        Assert.Equal(PdfName.StructElem, rootElem[PdfName.TYPE]);
        Assert.Equal(PdfName.Document, rootElem[PdfName.S]);

        var pElem = rootElem.Get<PdfDictionary>(PdfName.Kids);
        Assert.Equal(PdfName.StructElem, pElem[PdfName.TYPE]);
        Assert.Equal(PdfName.P, pElem[PdfName.S]);
        Assert.Equal(0, (PdfIntNumber)pElem[PdfName.Kids]);
        Assert.Equal(page.NativeObject.Indirect(), pElem[PdfName.Pg]);

        var parentTree = structTreeRoot.Get<PdfDictionary>(PdfName.ParentTree);
        var nums = parentTree.Get<PdfArray>(PdfName.Nums);
        Assert.Equal(0, (PdfIntNumber)nums[0]);
        
        var pageArray = ((PdfIndirectRef)nums[1]).GetObject().GetValue<PdfArray>();
        // We need to find the indirect ref for P element again to compare
        // but we can just check it's an array for now or get it from rootElem[PdfName.Kids]
        var pRef = rootElem[PdfName.Kids];
        Assert.Equal(pRef, pageArray[0]);

        Assert.Equal(0, (PdfIntNumber)page.NativeObject[PdfName.StructParents]);
    }
}
