using PdfLexer.DOM;
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
    public void StructuralSerializer_Serializes_Attributes()
    {
        var builder = new StructuralBuilder();
        builder.AddParagraph()
               .AddLayoutAttributes(textAlign: "Justify")
               .Back();

        var serializer = new StructuralSerializer();
        var result = serializer.ConvertToPdf(builder.GetRoot());
        
        var rootElem = result.Root.Get<PdfDictionary>(PdfName.Kids);
        var pElem = rootElem.Get<PdfDictionary>(PdfName.Kids);
        
        var attr = pElem.Get<PdfDictionary>(PdfName.A);
        Assert.NotNull(attr);
        Assert.Equal(PdfName.Layout, attr[PdfName.O]);
        Assert.Equal(new PdfName("Justify"), attr[PdfName.TextAlign]);
    }
}
