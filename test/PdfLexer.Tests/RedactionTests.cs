using PdfLexer.Content;
using PdfLexer.DOM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PdfLexer.Tests;

public class RedactionTests
{
    public RedactionTests()
    {
        CMaps.AddKnownPdfCMaps();
    }

    [Fact]
    public void It_Redacts()
    {
        var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
        var pdfRoot = Path.Combine(tp, "pdfs", "pdfjs");
        var pdf = Path.Combine(pdfRoot, "bug921409.pdf");
        using var doc = PdfDocument.Open(File.ReadAllBytes(pdf));
        var pg = doc.Pages[0];
        RunTest(tp, "redact.pdf", doc, pg, 's');
    }

    [Fact]
    public void It_Redacts_TJ()
    {
        var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
        var pdfRoot = Path.Combine(tp, "pdfs", "pdfjs");
        var pdf = Path.Combine(pdfRoot, "mixedfonts.pdf");
        using var doc = PdfDocument.Open(File.ReadAllBytes(pdf));
        var pg = doc.Pages[0];
        RunTest(tp, "redact_tj.pdf", doc, pg, 'e');
    }

    [Fact]
    public void It_Redacts_Form()
    {
        var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
        var pdfRoot = Path.Combine(tp, "pdfs", "pdfjs");
        var pdf = Path.Combine(pdfRoot, "tracemonkey.pdf");
        using var doc = PdfDocument.Open(File.ReadAllBytes(pdf));
        var pg = doc.Pages[1];
        RunTest(tp, "redact_form.pdf", doc, pg, 'e');
    }

    [Fact]
    public void It_Redacts_Page()
    {
        var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
        var pdfRoot = Path.Combine(tp, "pdfs", "pdfjs");
        var pdf = Path.Combine(pdfRoot, "tracemonkey.pdf");
        using var doc = PdfDocument.Open(File.ReadAllBytes(pdf));
        var pg = doc.Pages[0];
        RunTest(tp, "redact_pg.pdf", doc, pg, 'e');
    }

    private void RunTest(string tp, string name, PdfDocument doc, PdfPage pg, char c)
    {
        var redact = new Redactor(doc.Context, pg);
        var reader = new SimpleWordReader(doc.Context, pg, new HashSet<char> { '\n', ' ', '\r', '\t' });
        var sb = new StringBuilder();
        while (reader.Advance())
        {
            sb.Append(reader.CurrentWord + " ");
        }
        Assert.Contains(c, sb.ToString());

        var redacted = redact.RedactContent(i => i.Char == c);
        var od = PdfDocument.Create();
        od.Pages.Add(redacted);
        od.Pages.Add(pg);
        Directory.CreateDirectory(Path.Combine(tp, "results"));
        var output = Path.Combine(tp, "results", name);
        od.SaveTo(output);

        sb = new StringBuilder();
        reader = new SimpleWordReader(doc.Context, redacted, new HashSet<char> { '\n', ' ', '\r', '\t' });
        while (reader.Advance())
        {
            sb.Append(reader.CurrentWord + " ");
        }
        Assert.DoesNotContain(c, sb.ToString());
    }


    [InlineData("bug859204.pdf")]
    [InlineData("annotation-text-without-popup.pdf")]
    [Theory]
    public void It_Redacts_Page_Text_tests(string name)
    {
        var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
        var pdfRoot = Path.Combine(tp, "pdfs", "pdfjs");
        var pdfFile = Path.Combine(pdfRoot, name);
        using var pdf = PdfDocument.Open(pdfFile);
        var pg = pdf.Pages[0];
        RunTest(pdf, pg);
    }

    private void RunTest(PdfDocument doc, PdfPage pg)
    {
        var redact = new Redactor(doc.Context, pg);
        var reader = new TextScanner(doc.Context, pg);
        var dict = new Dictionary<char, int>();
        while (reader.Advance())
        {
            if (!dict.TryGetValue(reader.Glyph.Char, out var count))
            {
                count = 0;
            }
            dict[reader.Glyph.Char] = count + 1;
        }

        var kvp = dict.OrderByDescending(x => x.Value).First();

        var redacted = redact.RedactContent(i => i.Char == kvp.Key);

        dict = new Dictionary<char, int>();
        reader = new TextScanner(doc.Context, redacted);
        while (reader.Advance())
        {
            if (!dict.TryGetValue(reader.Glyph.Char, out var count))
            {
                count = 0;
            }
            dict[reader.Glyph.Char] = count + 1;
        }
        Assert.DoesNotContain(kvp.Key, dict.Keys);
    }
}
