using System.IO;
using PdfLexer.DOM;
using Xunit;

namespace PdfLexer.Tests;

public class SerializationTests
{
    [Fact]
    public void Should_save_complex_hierarchy()
    {
        var doc = PdfDocument.Create();
        var p1 = doc.AddPage();
        var p2 = doc.AddPage();

        var builder = new OutlineBuilder();
        builder.AddSection("Section 1")
               .AddBookmark("S1-P1", p1)
               .AddSection("Section 1.1")
               .AddBookmark("S1.1-P2", p2);
        
        doc.Outlines = builder.GetRoot();

        var bytes = doc.Save();
        
        // Reload and verify (using low-level inspection since parser isn't ready)
        using var doc2 = PdfDocument.Open(bytes);
        var catalog = doc2.Catalog;
        var outlines = catalog.Get<PdfDictionary>(PdfName.Outlines);
        Assert.NotNull(outlines);
        
        var first = outlines.Get<PdfDictionary>(PdfName.First);
        Assert.NotNull(first);
        Assert.Equal("Section 1", first.Get<PdfString>(PdfName.Title)?.Value);
        
        var s1First = first.Get<PdfDictionary>(PdfName.First);
        Assert.NotNull(s1First);
        Assert.Equal("S1-P1", s1First.Get<PdfString>(PdfName.Title)?.Value);
        
        var s1Next = s1First.Get<PdfDictionary>(PdfName.Next);
        Assert.NotNull(s1Next);
        Assert.Equal("Section 1.1", s1Next.Get<PdfString>(PdfName.Title)?.Value);
        
        var s11First = s1Next.Get<PdfDictionary>(PdfName.First);
        Assert.NotNull(s11First);
        Assert.Equal("S1.1-P2", s11First.Get<PdfString>(PdfName.Title)?.Value);
    }
}
