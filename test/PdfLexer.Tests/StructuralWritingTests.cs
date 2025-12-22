using PdfLexer.DOM;
using PdfLexer.Writing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PdfLexer.Tests;

public class StructuralWritingTests
{
    [Fact]
    public void PageWriter_Writes_Correct_MarkedContent_For_Nodes()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        var node = new StructureNode { Type = "P" };
        
        using (var writer = page.GetWriter())
        {
            writer.BeginMarkedContent(node);
            writer.Font(Base14.Helvetica, 12).TextMove(100, 700).Text("Tagged Text");
            writer.EndMarkedContent();
        }
        
        Assert.Single(node.ContentItems);
        Assert.Equal(0, node.ContentItems[0].MCID);
        var contents = page.Contents.First().Contents.GetDecodedStream();
        using var reader = new StreamReader(contents);
        var text = reader.ReadToEnd();
        
        Assert.Contains("/P <</MCID 0>> BDC", text);
        Assert.Contains("EMC", text);
    }

    [Fact]
    public void PageWriter_Increments_MCID()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        var p1 = new StructureNode { Type = "P" };
        var p2 = new StructureNode { Type = "P" };
        
        using (var writer = page.GetWriter())
        {
            writer.BeginMarkedContent(p1).EndMarkedContent();
            writer.BeginMarkedContent(p2).EndMarkedContent();
        }
        
        Assert.Equal(0, p1.ContentItems[0].MCID);
        Assert.Equal(1, p2.ContentItems[0].MCID);
    }

    [Fact]
    public void PageWriter_Writes_Artifacts()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        
        using (var writer = page.GetWriter())
        {
            writer.BeginArtifact(PdfName.Pagination);
            writer.EndMarkedContent();
        }
        
        var contents = page.Contents.First().Contents.GetDecodedStream();
        using var reader = new StreamReader(contents);
        var text = reader.ReadToEnd();
        
        Assert.Contains("/Artifact <</Type/Pagination>> BDC", text);
        Assert.Contains("EMC", text);
    }

    [Fact]
    public void PageWriter_Writes_Simple_Artifacts()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage();

        using (var writer = page.GetWriter())
        {
            writer.BeginArtifact();
            writer.EndMarkedContent();
        }

        var contents = page.Contents.First().Contents.GetDecodedStream();
        using var reader = new StreamReader(contents);
        var text = reader.ReadToEnd();

        Assert.Contains("/Artifact BMC", text);
        Assert.Contains("EMC", text);
    }
}
