using PdfLexer.DOM;
using PdfLexer.Writing;
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

        var rootElem = structTreeRoot.Get<PdfDictionary>(PdfName.K);
        Assert.Equal(PdfName.StructElem, rootElem[PdfName.TYPE]);
        Assert.Equal(PdfName.Document, rootElem[PdfName.S]);
        Assert.Same(structTreeRoot, rootElem[PdfName.P].Resolve());

        var pElem = rootElem.Get<PdfDictionary>(PdfName.K);
        Assert.Equal(PdfName.StructElem, pElem[PdfName.TYPE]);
        Assert.Equal(PdfName.P, pElem[PdfName.S]);
        Assert.Equal(0, (PdfIntNumber)pElem[PdfName.K]);
        Assert.Equal(page.NativeObject.Indirect(), pElem[PdfName.Pg]);

        var parentTree = structTreeRoot[PdfName.ParentTree].Resolve().GetAs<PdfDictionary>();
        var nums = parentTree.Get<PdfArray>(PdfName.Nums);
        Assert.Equal(0, (PdfIntNumber)nums[0]);

        var pageArray = nums[1].GetAs<PdfArray>();
        var pRef = rootElem[PdfName.K];
        Assert.Equal(pRef, pageArray[0]);

        Assert.Equal(0, (PdfIntNumber)page.NativeObject[PdfName.StructParents]);
    }

    [Fact]
    public void StructuralSerializer_Uses_Correct_Alt_Casing()
    {
        var root = new StructureNode { Type = "Document", Alt = "document alt" };

        var serializer = new StructuralSerializer();
        var result = serializer.ConvertToPdf(root);
        var rootElem = result.Root.Get<PdfDictionary>(PdfName.K);

        Assert.True(rootElem.ContainsKey(PdfName.Alt));
        Assert.False(rootElem.ContainsKey(PdfName.ALT));
        Assert.Equal("document alt", rootElem.Get<PdfString>(PdfName.Alt).Value);
    }

    [Fact]
    public void StructuralSerializer_Emits_Metadata_Maps_And_Accessibility_Attributes()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        var builder = new StructuralBuilder();
        var ns = builder.DeclareNamespace("https://example.com/ns");
        builder.MapRole("CustomFigure", "Figure")
            .AddClassDefinition("Hero", new PdfDictionary { [PdfName.O] = PdfName.Layout, [PdfName.Width] = new PdfIntNumber(200) });

        var figure = builder.AddElement("CustomFigure")
            .ElementId("fig-1")
            .Alt("Figure alt")
            .ActualText("Figure actual")
            .Expansion("Figure expansion")
            .Lang("en-US")
            .AddClass("Hero")
            .SetNamespace(ns)
            .References("table-cell")
            .GetNode();
        figure.ContentItems.Add((page, 0));

        var cellCtx = builder.AddTable("Table")
            .AddHeaderCell()
            .ElementId("table-cell")
            .TableScope(StructureScope.Column)
            .TableHeaders("fig-1")
            .TableSummary("Header summary")
            ;
        var cell = cellCtx.GetNode();
        cellCtx.Back()
            .Back()
            .GetNode();

        var list = builder.AddList()
            .ListNumbering(StructureListNumbering.UpperRoman)
            .GetNode();

        var serializer = new StructuralSerializer();
        var result = serializer.ConvertToPdf(builder.GetRoot());
        var structTreeRoot = result.Root;

        var idTree = structTreeRoot[PdfName.IDTree].Resolve().GetAs<PdfDictionary>();
        var names = idTree.Get<PdfArray>(PdfName.Names);
        Assert.Equal("fig-1", names[0].GetAs<PdfString>().Value);

        var roleMap = structTreeRoot.Get<PdfDictionary>(PdfName.RoleMap);
        Assert.Equal(PdfName.Figure, roleMap[(PdfName)"CustomFigure"]);

        var classMap = structTreeRoot.Get<PdfDictionary>(PdfName.ClassMap);
        Assert.True(classMap.ContainsKey((PdfName)"Hero"));

        var namespaces = structTreeRoot.Get<PdfArray>(PdfName.Namespaces);
        Assert.Single(namespaces);

        var figureDict = result.Map[figure].GetObject().GetAs<PdfDictionary>();
        Assert.Equal("Figure alt", figureDict.Get<PdfString>(PdfName.Alt).Value);
        Assert.Equal("Figure actual", figureDict.Get<PdfString>(PdfName.ActualText).Value);
        Assert.Equal("Figure expansion", figureDict.Get<PdfString>(PdfName.E).Value);
        Assert.Equal("en-US", figureDict.Get<PdfString>(PdfName.Lang).Value);
        Assert.True(figureDict.ContainsKey(PdfName.Ref));
        Assert.True(figureDict.ContainsKey(PdfName.Class));
        Assert.True(figureDict.ContainsKey(PdfName.NS));

        var cellDict = result.Map[cell].GetObject().GetAs<PdfDictionary>();
        var attrs = cellDict[PdfName.A].Resolve().GetAs<PdfDictionary>();
        Assert.Equal(PdfName.Table, attrs[PdfName.O]);
        Assert.Equal(PdfName.Column, attrs[PdfName.Scope]);
        Assert.True(attrs.ContainsKey(PdfName.Headers));
        Assert.Equal("Header summary", attrs.Get<PdfString>(PdfName.Summary).Value);

        var listDict = result.Map[list].GetObject().GetAs<PdfDictionary>();
        var listAttr = listDict[PdfName.A].Resolve().GetAs<PdfDictionary>();
        Assert.Equal("List", ((PdfName)listAttr[PdfName.O]).Value);
        Assert.Equal(PdfName.UpperRoman, listAttr[PdfName.ListNumbering]);
    }

    [Fact]
    public void StructuralSerializer_Expands_ParentTree_For_Object_And_XObject_Entries()
    {
        var root = new StructureNode { Type = "Document" };
        var link = new StructureNode { Type = "Link" };
        root.Children.Add(link);

        var annot = new PdfDictionary();
        var xObject = new PdfDictionary();
        link.ObjectReferences.Add(new StructureObjectReference(annot, 5));
        link.XObjectReferences.Add(new StructureXObjectReference(xObject, 6));

        var serializer = new StructuralSerializer();
        var result = serializer.ConvertToPdf(root);
        var parentTree = result.Root[PdfName.ParentTree].Resolve().GetAs<PdfDictionary>();
        var nums = parentTree.Get<PdfArray>(PdfName.Nums);

        Assert.Equal(5, (PdfIntNumber)nums[0]);
        Assert.Equal(result.Map[link], nums[1]);
        Assert.Equal(6, (PdfIntNumber)nums[2]);
        Assert.Equal(result.Map[link], nums[3]);
        Assert.Equal(5, (PdfIntNumber)annot[PdfName.StructParent]);
        Assert.Equal(6, (PdfIntNumber)xObject[PdfName.StructParents]);
    }
}
