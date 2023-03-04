using PdfLexer.Validation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        var pdfFile = Path.Combine(pdfRoot, "issue1002.pdf");
        using var doc = PdfDocument.Open(pdfFile);
        doc.Trailer["Bad"] = new PdfString("Value");
        var val = new PdfValidator(doc.Trailer);
        val.Version = doc.PdfVersion;
        val.Run();
    }
}
