using PdfLexer.DOM;
using PdfLexer.Content;
using PdfLexer.Writing;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace PdfLexer.Tests;

public class StructuralAttributeTests
{
    [Fact]
    public void StructuralBuilder_Adds_Table_Attributes()
    {
        var builder = new StructuralBuilder();
        builder.AddTable()
               .AddRow()
               .AddCell(header: true, colSpan: 2)
               .Back();

        var root = builder.GetRoot();
        var table = root.Children[0];
        Assert.Equal("Table", table.Type);
        
        var row = table.Children[0];
        Assert.Equal("TR", row.Type);
        
        var cell = row.Children[0];
        Assert.Equal("TH", cell.Type);
        
        var attr = cell.Attributes[0];
        Assert.Equal(PdfName.Table, attr[PdfName.O]);
        Assert.Equal(2, (PdfIntNumber)attr[PdfName.ColSpan]);
    }

    [Fact]
    public void StructuralBuilder_Adds_Layout_Attributes()
    {
        var builder = new StructuralBuilder();
        builder.AddParagraph()
               .AddLayoutAttributes(textAlign: "Center", width: 100.5)
               .Back();

        var root = builder.GetRoot();
        var p = root.Children[0];
        Assert.Equal("P", p.Type);
        
        var attr = p.Attributes[0];
        Assert.Equal(PdfName.Layout, attr[PdfName.O]);
        Assert.Equal(new PdfName("Center"), attr[PdfName.TextAlign]);
        Assert.Equal(100.5, (PdfDoubleNumber)attr[PdfName.Width]);
    }

    [Fact]
    public void StructuralBuilder_Adds_Layout_BoundingBox()
    {
        var builder = new StructuralBuilder();
        builder.AddFigure(altText: "Chart")
               .AddLayoutAttributes(width: 64, height: 64)
               .AddLayoutBoundingBox(new PdfRect<double>(40, 560, 104, 624))
               .Back();

        var figure = builder.GetRoot().Children[0];
        var attr = Assert.Single(figure.Attributes);
        Assert.Equal(PdfName.Layout, attr[PdfName.O]);
        Assert.Equal(64, (PdfDoubleNumber)attr[PdfName.Width]);
        Assert.Equal(64, (PdfDoubleNumber)attr[PdfName.Height]);

        var bbox = attr.Get<PdfArray>(PdfName.BBox);
        Assert.NotNull(bbox);
        Assert.Equal(4, bbox!.Count);
        Assert.Equal(40, (decimal)(PdfNumber)bbox[0]);
        Assert.Equal(560, (decimal)(PdfNumber)bbox[1]);
        Assert.Equal(104, (decimal)(PdfNumber)bbox[2]);
        Assert.Equal(624, (decimal)(PdfNumber)bbox[3]);
    }

    [Fact]
    public void StructuralSerializer_Serializes_Attributes()
    {
        var builder = new StructuralBuilder();
        builder.AddParagraph()
               .AddLayoutAttributes(textAlign: "Justify")
               .Back();

        var serializer = new StructuralSerializer();
        var result = serializer.ConvertToPdf(builder.GetRoot());
        
        var rootElem = result.Root.Get<PdfDictionary>(PdfName.K);
        var pElem = rootElem.Get<PdfDictionary>(PdfName.K);
        
        var attr = pElem.Get<PdfDictionary>(PdfName.A);
        Assert.NotNull(attr);
        Assert.Equal(PdfName.Layout, attr[PdfName.O]);
        Assert.Equal(new PdfName("Justify"), attr[PdfName.TextAlign]);
    }
}
