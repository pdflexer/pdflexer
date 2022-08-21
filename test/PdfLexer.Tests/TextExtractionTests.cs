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
        public void It_Reads_Type1_Embedded_Diff_Encoding_FF3() => RunSingle("issue11016_reduced.pdf");

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


        // [Fact]
        //public void It_Reads_TrueType_Bad_Base14() => RunSingle("issue6127.pdf");
        [Fact]
        public void It_Reads_TrueType_MacRoman_Embedded() => RunSingle("openoffice.pdf");

        [Fact]
        public void It_Reads_TrueType_MacRoman_NonEmbed() => RunSingle("issue6127.pdf");

        [Fact]
        public void It_Reads_TrueType_MacRoman_Embedded_ToUnicode() => RunSingle("issue8707.pdf");

        [Fact]
        public void It_Reads_TrueType_WinAnsi() => RunSingle("bug1671312_ArialNarrow.pdf");

        [Fact]
        public void It_Reads_TrueType_WinAnsi_Embedded() => RunSingle("annotation-border-styles.pdf");


        // TODO research, doens't look like extractable
        // [Fact]
        // public void It_Reads_TrueType_WinAnsi_Embedded_widths_mw() => RunSingle("bug1027533.pdf");

        [Fact]
        public void It_Reads_TrueType_WinAnsi_NonEmbed() => RunSingle("issue5470.pdf");

        [Fact]
        public void It_Reads_TrueType_WinAnsi_NonEmbed_ToUnicode() => RunSingle("issue4934.pdf");

        [Fact]
        public void It_Reads_TrueType_Identity_Embedded_ToUnicode() => RunSingle("issue5701.pdf");

        [Fact]
        public void It_Reads_TrueType_Diff_Embedded() => RunSingle("bug1669099.pdf");

        [Fact]
        public void It_Reads_TrueType_Diff_Embedded_ToUnicode() => RunSingle("bug1132849.pdf");

        [Fact]
        public void It_Reads_TrueType_None_Embedded_ToUnicode() => RunSingle("annotation-highlight-without-appearance.pdf");
        
        // [Fact]
        // public void It_Reads_TrueType_None_Embedded() => RunSingle("bug894572.pdf");

        [Fact]
        public void It_Reads_TrueType_None_NonEmbed_ToUnicode() => RunSingle("bug864847.pdf");

        [Fact]
        public void It_Reads_TrueType_WinAnsi_FF3() => RunSingle("issue4668.pdf");




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
