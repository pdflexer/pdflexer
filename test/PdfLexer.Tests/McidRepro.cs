using PdfLexer;
using PdfLexer.DOM;
using PdfLexer.Writing;
using System.IO;
using System.Linq;
using Xunit;

namespace PdfLexer.Tests;

public class McidRepro
{
    // [Fact] TODO -> trac MCID across writing sessions
    public void Mcid_Should_Increment_On_Subsequent_PageWriter_Usage()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        
        // Pass 1: Write first marked content
        var p1 = new StructureNode { Type = "P" };
        using (var writer = new PageWriter<double>(page, PageWriteMode.Append))
        {
            writer.BeginMarkedContent(p1);
            writer.Font(Base14.TimesRoman, 10);
            writer.Text("Hello");
            writer.EndMarkedContent();
        }

        // Pass 2: Write second marked content
        var p2 = new StructureNode { Type = "P" };
        using (var writer = new PageWriter<double>(page, PageWriteMode.Append))
        {
            
            writer.BeginMarkedContent(p2);
            writer.Font(Base14.TimesRoman, 10);
            writer.Text("World");
            writer.EndMarkedContent();
        }

        // Verify MCIDs
        // content of p1 should be MCID 0
        // content of p2 should be MCID 1
        
        // Check p1
        Assert.Single(p1.ContentItems);
        Assert.Equal(0, p1.ContentItems[0].MCID);

        // Check p2
        Assert.Single(p2.ContentItems);
        // This is expected to fail if PageWriter resets MCID to 0
        Assert.Equal(1, p2.ContentItems[0].MCID); 
    }
}