using PdfLexer.Content;
using PdfLexer.Fonts;
using PdfLexer.Operators;
using PdfLexer.Parsers;
using PdfLexer.Writing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xunit;

namespace PdfLexer.Tests
{
    public class TextExtractionTests
    {
        public TextExtractionTests()
        {
            CMaps.AddKnownPdfCMaps();
        }
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

        [Fact]
        public void It_Reads_Type0_To_Unicode_NoEmbed() => RunSingle("issue5747.pdf");

        [Fact]
        public void It_Reads_Type0_To_Unicode_NoEmbed2() => RunSingle("issue2840.pdf");
        [Fact]
        public void It_Reads_Type0_To_Unicode_NoEmbed3() => RunSingle("issue5540.pdf");
        [Fact]
        public void It_Reads_Type0_GBKEUCH_KnownUnicode_NoEmbed() => RunSingle("issue2128r.pdf");
        [Fact]
        public void It_Reads_Type0_GBKEUCH_KnownUnicode_NoEmbed2() => RunSingle("issue3521.pdf");
        [Fact]
        public void It_Reads_Type0_UniGB_KnownUnicode_NoEmbed() => RunSingle("issue8372.pdf");
        [Fact]
        public void It_Reads_Type0_CMapEncoding_Missing_Range() => RunSingle("issue11768_reduced.pdf");
        [Fact]
        public void It_Reads_Type0_To_Unicode() => RunSingle("basicapi.pdf");
        [Fact]
        public void It_Reads_Type0_UNIJIS_KnownUnicode_NoEmbed2() => RunSingle("issue6286.pdf");
        [Fact]
        public void It_Reads_Type0_UNIJIS_KnownUnicode_NoEmbed() => RunSingle("mixedfonts.pdf");
        [Fact]
        public void It_Reads_Type0_Identity_Japan1() => RunSingle("arial_unicode_en_cidfont.pdf");
        [Fact]
        public void It_Reads_Type0_EUCJP1_KnownUnicode_NoEmbed() => RunSingle("noembed-eucjp.pdf");

        [Fact]
        public void It_Reads_Type0_Guessed_Unicode_CIDToGID() => RunSingle("bug1650302_reduced.pdf");
        [Fact]
        public void It_Reads_Type0_Vertical_Identity() => RunSingle("vertical.pdf");
        [Fact]
        public void It_Reads_Type0_Vertical_RKSJ() => RunSingle("issue11555.pdf");

        [Fact]
        public void It_Reads_Type0_TrueType_Embedded_CMap() => RunSingle("Test-plusminus.pdf");

        [Fact]
        public void It_Reads_Type0_ToUnicode_Identity() => RunSingle("issue12418_reduced.pdf");

        [Fact]
        public void It_Reads_Type0_ToUnicode_Identity2() => RunSingle("issue4402_reduced.pdf");

        [Fact]
        public void It_Reads_Type0_TrueType_Embedded_CMap2() => RunSingle("issue3323.pdf");
        [Fact]
        public void It_Reads_Type0_TrueType_Embedded_CMap3() => RunSingle("javauninstall-7r.pdf");

        [Fact]
        public void It_Reads_Type0_JIS() => RunSingle("noembed-jis7.pdf");
        [Fact]
        public void It_Reads_Type0_SJIS() => RunSingle("noembed-sjis.pdf");
        [Fact]
        public void It_Reads_Type0_Vert() => RunSingle("issue6387.pdf"); // bounding box estimate is not great here, font has huge bbox

        [Fact]
        public void It_Reads_With_GS_Op()
        {
            using var ctx = new ParsingContext();
            using var doc = PdfDocument.Create();
            var pg = doc.AddPage();
            {
                using var writer = pg.GetWriter();

                writer.Font(ContentWriter.Base14.Courier, 10)
                      .CustomOp(new gs_Op("GSName"))
                      .Text("Hello world");
                // writer.Complete();
            }
            var dict = Standard14Font.GetHelvetica().GetPdfFont();
            pg.Resources[PdfName.ExtGState] = new PdfDictionary()
            {
                ["GSName"] = new PdfDictionary
                {
                    [PdfName.Font] = new PdfArray { dict.Indirect(), new PdfIntNumber(15) }
                }
            };


            var scanner = pg.GetTextScanner(doc.Context);
            while (scanner.Advance())
            {
                Assert.Equal(15f, scanner.GraphicsState.FontSize);
            }
        }


        [Fact]
        public void It_Reads_Visually()
        {
            using var ctx = new ParsingContext();
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var pdfRoot = Path.Combine(tp, "pdfs", "pdfjs");
            var pdfFile = Path.Combine(pdfRoot, "issue1002.pdf");
            using var pdf = PdfDocument.Open(pdfFile);

            var text= pdf.Pages[0].GetTextVisually(pdf.Context);
            Assert.NotEqual(' ', text[text.Length - 2]);
        }

        private void RunSingle(string name)
        {
            // using var ctx = new ParsingContext();
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
                    }
                    else
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
