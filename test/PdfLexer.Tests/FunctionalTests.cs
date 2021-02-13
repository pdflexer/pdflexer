using System;
using System.IO;
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
        [Theory]
        public void It_Reads_Objects(string pdfPath)
        {
           // var pdf = new Span<byte>(File.ReadAllBytes("../../../../../.test-pdfs/" + pdfPath));
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
    }
}