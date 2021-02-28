using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PdfLexer.IO;
using PdfLexer.Parsers;
using Xunit;

namespace PdfLexer.Tests
{
    public class FunctionalTests
    {
        // [InlineData("01_CMYK_OP/010_CMYK_OP_x3.pdf")]
        // [InlineData("01_CMYK_OP/010_ReadMe_Ghent_Output_Patch.pdf")]
        // [InlineData("01_CMYK_OP/011_Overprint-Mode_x3.pdf")]
        // [InlineData("01_CMYK_OP/011_ReadMe_Ghent_Output_Patch.pdf")]
        // [InlineData("02_Spot_OP/020_CMYKSpot_OP_x1a.pdf")]
        // [InlineData("02_Spot_OP/020_ReadMe_Ghent_Output_Patch.pdf")]
        // //[InlineData("mt200953a.pdf")] // bad xref // TODO test rebuilding
        // [InlineData("ymj-46-585.pdf")]
        // [Theory]
        public async Task It_Reads_Objects(string pdfPath)
        {
            var data = File.ReadAllBytes(Path.Combine("c:\\temp\\test-pdfs\\", pdfPath));
            var doc = await PdfDocument.Open(data);
            foreach (var item in doc.XrefEntries)
            {
                doc.Context.GetIndirectObject(item.Key);
            }
           // var xrefStart = XRefParser.GetXrefTableOffset(pdf);
           // var refs = XRefTableParser.GetEntries(pdf, xrefStart, out PdfLazyDictionary trailer);
           // var lookup = new IndirectLookup(pdf, refs, 10);
           // foreach (var obj in refs)
           // {
           //     if (obj.Value.IsFree)
           //     {
           //         continue;
           //     }
           //     var data = lookup.GetIndirectObjectData((int)obj.Value.ObjectNumber, out var type);
           //     switch (type) {
           //         case PdfObjectType.DictionaryObj:
           //             _ = DictionaryParser.ParseLazyDictionary(data, 0);
           //             break;
           //     }
           // }
        }

        // [InlineData("01_CMYK_OP/010_CMYK_OP_x3.pdf")]
        // [InlineData("01_CMYK_OP/010_ReadMe_Ghent_Output_Patch.pdf")]
        // [InlineData("01_CMYK_OP/011_Overprint-Mode_x3.pdf")]
        // [InlineData("01_CMYK_OP/011_ReadMe_Ghent_Output_Patch.pdf")]
        // [InlineData("02_Spot_OP/020_CMYKSpot_OP_x1a.pdf")]
        // [InlineData("02_Spot_OP/020_ReadMe_Ghent_Output_Patch.pdf")]
        // //[InlineData("mt200953a.pdf")] // bad xref // TODO test rebuilding
        // [InlineData("ymj-46-585.pdf")]
        // [InlineData("C:\\temp\\PRIV\\Origrk.pdf")]
        // [Theory]
        public async Task It_Loads_Pages(string pdfPath)
        {
            var data = File.ReadAllBytes(Path.Combine("c:\\temp\\test-pdfs\\", pdfPath));
            var doc = await PdfDocument.Open(data);
            long total = 0;
            foreach (PdfDictionary page in doc.Pages)
            {
                var content = page[PdfName.Contents];
                if (content is PdfArray contentArray)
                {
                    foreach (var str in contentArray)
                    {
                        var stream = str.GetValue<PdfStream>();
                        total += stream.Dictionary.GetRequiredValue<PdfNumber>(PdfName.Length);
                    }
                } else
                {
                    total += content.GetValue<PdfStream>().Dictionary.GetRequiredValue<PdfNumber>(PdfName.Length);
                }
            }
           

           // }
        }

        //[Fact]
        public async Task It_Reads_All()
        {
            foreach (var pdf in Directory.GetFiles(@"C:\temp\test-pdfs\pp", "*.pdf"))
            {
                var doc = await PdfDocument.Open(File.ReadAllBytes(pdf));

                foreach (var item in doc.XrefEntries)
                {
                    doc.Context.GetIndirectObject(item.Key);
                }
            }
        }
    }
}