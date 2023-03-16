using PdfLexer.Validation;
using System.IO;
using Xunit;

namespace PdfLexer.Tests;

public class ValidationTests
{
    [Fact]
    public static void It_Validates()
    {
        using var ctx = new ParsingContext();
        var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
        var pdfRoot = Path.Combine(tp, "pdfs", "pdfjs");
        foreach (var pdf in Directory.GetFiles(pdfRoot, "*.pdf"))
        {
            var fi = new FileInfo(pdf);
            using var doc = PdfDocument.Open(pdf);
            var val = new PdfValidator(doc, fi.Length);
            val.Run();
        }
        
    }
}
