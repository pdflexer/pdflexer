using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PdfLexer.IO;
using PdfLexer.Parsers;
using PdfLexer.Serializers;
using Xunit;

namespace PdfLexer.Tests
{
    public class FunctionalTests
    {
        //[InlineData("01_CMYK_OP/010_CMYK_OP_x3.pdf")]
        // [InlineData("01_CMYK_OP/010_ReadMe_Ghent_Output_Patch.pdf")]
        // [InlineData("01_CMYK_OP/011_Overprint-Mode_x3.pdf")]
        // [InlineData("01_CMYK_OP/011_ReadMe_Ghent_Output_Patch.pdf")]
        // [InlineData("02_Spot_OP/020_CMYKSpot_OP_x1a.pdf")]
        // [InlineData("02_Spot_OP/020_ReadMe_Ghent_Output_Patch.pdf")]
        // //[InlineData("mt200953a.pdf")] // bad xref // TODO test rebuilding
        // [InlineData("ymj-46-585.pdf")]
        //[Theory]
        public async Task It_Reads_Objects(string pdfPath)
        {
            var data = File.ReadAllBytes(Path.Combine("c:\\temp\\test-pdfs\\", pdfPath));
            var doc = await PdfDocument.Open(data);
            foreach (var item in doc.XrefEntries)
            {
                doc.Context.GetIndirectObject(item.Key);
            }

            var ms = new MemoryStream();
            var ctx = new WritingContext(ms);
            ctx.Initialize(1.7m);
            // doc.Catalog["/ModifiedMDP"] = PdfBoolean.True;
            // var ir = ctx.WriteIndirectObject(PdfIndirectRef.Create(doc.Catalog));
            // doc.Trailer["/Root"] = ir;
            ctx.Complete(doc.Trailer);
            File.WriteAllBytes("c:\\temp\\dummy.tmp.pdf", ms.ToArray());
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
        [InlineData("pdfjs/160F-2019.pdf")]
        //[InlineData("C:\\temp\\PRIV\\Origrk.pdf")]
        // [InlineData("C:\\temp\\large.raw.pdf")]
        [Theory]
        public async Task It_Loads_Pages(string pdfPath)
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var pdf = Path.Combine(tp, "pdfs", pdfPath);
            using var doc = await PdfDocument.Open(File.ReadAllBytes(pdf), new ParsingOptions { LoadPageTree = true });

            var raw = new MemoryStream();
            doc.SaveTo(raw);
            File.WriteAllBytes("c:\\temp\\raw.pdf", raw.ToArray());
            using var docReRead = await PdfDocument.Open(raw.ToArray(), new ParsingOptions { LoadPageTree = true });
            var ms = new MemoryStream();
            var ctx = new WritingContext(ms);
            ctx.Initialize(1.7m);
            ctx.Complete(doc.Trailer);
            using var docReRead2 = await PdfDocument.Open(ms.ToArray(), new ParsingOptions { LoadPageTree = true });

            var c1 = GetCount(doc);
            var c2 = GetCount(docReRead);
            var c3 = GetCount(docReRead2);

            Assert.Equal(c1, c2);
            Assert.Equal(c1, c3);

            long GetCount(PdfDocument toCount)
            {
                long total = 0;
                foreach (PdfDictionary page in toCount.Pages)
                {
                    var content = page[PdfName.Contents];
                    content = content.Resolve();
                    if (content is PdfArray contentArray)
                    {
                        foreach (var str in contentArray)
                        {
                            var stream = str.GetValue<PdfStream>();
                            total += stream.Dictionary.GetRequiredValue<PdfNumber>(PdfName.Length);
                        }
                    }
                    else
                    {
                        total += content.GetValue<PdfStream>().Dictionary.GetRequiredValue<PdfNumber>(PdfName.Length);
                    }
                }
                return total;
            }



            // }
        }

        [Fact]
        public async Task It_Reads_All_Pdf_JS()
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var pdfRoot = Path.Combine(tp, "pdfs", "pdfjs");
            var errors = new List<string>();
            foreach (var pdf in Directory.GetFiles(pdfRoot, "*.pdf"))
            {
                try
                {
                    var doc = await PdfDocument.Open(File.ReadAllBytes(pdf));

                    foreach (var item in doc.XrefEntries)
                    {
                        doc.Context.GetIndirectObject(item.Key);
                    }

                }
                catch (Exception e)
                {
                    errors.Add(pdf + ": " + e.Message);
                }
            }
            if (errors.Any())
            {
                throw new ApplicationException(string.Join(Environment.NewLine, errors));
            }
        }

        [Fact]
        public async Task It_Repairs_Bad_Stream_Start()
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var pdf = Path.Combine(tp, "pdfs", "pdfjs", "need_repair", "issue7229.pdf");
            var doc = await PdfDocument.Open(File.ReadAllBytes(pdf));

            foreach (var item in doc.XrefEntries)
            {
                doc.Context.GetIndirectObject(item.Key);
            }
        }
    }
}