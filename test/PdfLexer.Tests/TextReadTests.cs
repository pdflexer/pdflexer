using PdfLexer.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PdfLexer.Tests
{
    public class TextReadTests
    {
        [InlineData("issue4304.pdf")]
        [Theory]
        public void It_Difference_Base14(string pdf)
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            // var output = Path.Combine(tp, "results", "images");
            var pdfRoot = Path.Combine(tp, "pdfs", "pdfjs");
            using var doc = PdfDocument.OpenMapped(Path.Combine(pdfRoot, pdf));

            int i = 0;
            foreach (var page in doc.Pages)
            {
                var reader = new TextScanner(doc.Context, page);
                var sb = new StringBuilder();
                while (reader.Advance())
                {
                    sb.Append(reader.Glyph.Char);
                    
                }
                var str = sb.ToString();

            }
        }
    }
}
