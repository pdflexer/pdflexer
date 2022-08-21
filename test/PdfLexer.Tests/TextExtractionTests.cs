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
    public class TextExtractionTests
    {
        [Fact]
        public void It_Reads_Type1_Embedded_No_Encoding() => RunSingle("bug859204.pdf");

        [Fact]
        public void It_Reads_Type1_Embedded_DiffEnc() => RunSingle("annotation-text-without-popup.pdf");

        [Fact]
        public void It_Reads_Type1_Embedded_No_Encoding_FF3() => RunSingle("issue11279.pdf");

        [Fact]
        public void It_Reads_Type1_B14() => RunSingle("asciihexdecode.pdf");

        [Fact]
        public void It_Reads_Type1_B14_Diff_Encoding() => RunSingle("franz.pdf");

        [Fact]
        public void It_Reads_Type1_B14_Diff_Encoding2() => RunSingle("issue11403_reduced.pdf");

        [Fact]
        public void It_Reads_Type1_B14_Diff_Base_Encoding() => RunSingle("issue5599.pdf");

        [Fact]
        public void It_Reads_Type1_Enc_Widths() => RunSingle("alphatrans.pdf");

        [Fact]
        public void It_Reads_Type1_Diff_Widths() => RunSingle("issue6127.pdf");

        [Fact]
        public void It_Reads_Type1_Diff_ToUnicode() => RunSingle("issue5238.pdf");

        [Fact]
        public void It_Reads_Type1_Embedded_WinAnsi() => RunSingle("bug1252420.pdf");

        [Fact]
        public void It_Reads_Type1_Embedded_MacRoman() => RunSingle("issue3207r.pdf");

        [Fact]
        public void It_Reads_Type1_Embedded_Diff_Base_WinAnsi_FF3() => RunSingle("083014_hanrei.pdf");

        private void RunSingle(string name)
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var pdfRoot = Path.Combine(tp, "pdfs", "pdfjs");
            var txtFile = Path.Combine(tp, "pdfs", "txt-values", name + ".txt");
            var pdfFile = Path.Combine(pdfRoot, name);
            using var pdf = PdfDocument.Open(pdfFile);
            var sb = new StringBuilder();
            int pg = 1;
            foreach (var page in pdf.Pages)
            {
                var reader = new TextScanner(pdf.Context, page);
                while (reader.Advance())
                {
                    var (x, y) = reader.GetCurrentTextPos();
                    if (reader.Glyph.MultiChar != null)
                    {
                        foreach (var c in reader.Glyph.MultiChar)
                        {
                            sb.Append($"{pg} {x:0.00} {y:0.0} {c} {reader.WasNewStatement} {reader.WasNewLine}\n");
                        }
                    } else
                    {
                        sb.Append($"{pg} {x:0.00} {y:0.0} {reader.Glyph.Char} {reader.WasNewStatement} {reader.WasNewLine}\n");
                    }
                }
                pg++;
            }

            Directory.CreateDirectory(Path.Combine(tp, "results", "txt-extract"));
            var output = Path.Combine(tp, "results", "txt-extract", name + ".txt");
            var result = sb.ToString();
            File.WriteAllText(output, result);

            string expected = null;
            if (File.Exists(txtFile))
            {
                expected = File.ReadAllText(txtFile);
            }

            Assert.Equal(expected, result);
        }
    }
}
