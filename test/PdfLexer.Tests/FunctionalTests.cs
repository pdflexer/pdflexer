using System;
using System.IO;
using System.Threading.Tasks;
using PdfLexer.IO;
using PdfLexer.Parsers;
using Xunit;

namespace PdfLexer.Tests
{
    public class FunctionalTests
    {
        [InlineData("01_CMYK_OP/010_CMYK_OP_x3.pdf")]
        [InlineData("01_CMYK_OP/010_ReadMe_Ghent_Output_Patch.pdf")]
        [InlineData("01_CMYK_OP/011_Overprint-Mode_x3.pdf")]
        [InlineData("01_CMYK_OP/011_ReadMe_Ghent_Output_Patch.pdf")]
        [InlineData("02_Spot_OP/020_CMYKSpot_OP_x1a.pdf")]
        [InlineData("02_Spot_OP/020_ReadMe_Ghent_Output_Patch.pdf")]
        //[InlineData("mt200953a.pdf")] // bad xref // TODO test rebuilding
        [InlineData("ymj-46-585.pdf")]
        [Theory]
        public async Task It_Reads_Objects(string pdfPath)
        {
            var data = File.ReadAllBytes(Path.Combine("c:\\temp\\test-pdfs\\", pdfPath));
            var source = new InMemoryDataSource(data);
            var ctx = new ParsingContext();
            await ctx.Initialize(source);
            foreach (var item in ctx.XrefEntries)
            {
                ctx.GetIndirectObject(item.Key);
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

        //[Fact]
        public async Task It_Reads_All()
        {
            foreach (var pdf in Directory.GetFiles(@"C:\temp\test-pdfs\pp", "*.pdf"))
            {
                var data = File.ReadAllBytes(pdf);
                var source = new InMemoryDataSource(data);
                var ctx = new ParsingContext();
                await ctx.Initialize(source);
                foreach (var item in ctx.XrefEntries)
                {
                    ctx.GetIndirectObject(item.Key);
                }
            }
        }
    }
}