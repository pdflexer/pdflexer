using PdfLexer.Content;
using PdfLexer.Fonts;
using PdfLexer.Tests;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PdfLexer.ImageTests
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
            using var doc = PdfDocument.Open(Path.Combine(pdfRoot, pdf));

            int i = 0;
            foreach (var page in doc.Pages)
            {
                var reader = new TextScanner(doc.Context, page);
                var sb = new StringBuilder();
                while (reader.Advance())
                {
                    var c = reader.Glyph.Char;
                    var (llx, lly, urx, ury) = reader.GetCurrentBoundingBox();
                    sb.AppendLine($"{llx:0.00} {lly:0.00} {urx:0.00} {ury:0.00} {c}");
                }
                var str = sb.ToString();
            }
        }

        [Fact]
        public void Test()
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var output = Path.Combine(tp, "results", "txt");
            var pdfRoot = Path.Combine(tp, "pdfs", "pdfjs");
            using var doc = PdfDocument.Create();
            var pg = doc.AddPage();
            pg.MediaBox.URx = 200;
            pg.MediaBox.URy = 100;

            {
                using var writer = pg.GetWriter();
                var tr = Standard14Font.GetTimesRoman();
                writer.Font(tr, 10).TextMove(20, 20).Text("Testing").EndText();
            }
            using var fo = File.Create(Path.Combine(output, "test.pdf"));
            doc.SaveTo(fo);
        }


    }
}
