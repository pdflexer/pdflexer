using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        [InlineData("pdfjs/160F-2019.pdf")]
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

        // [Fact]
        public async Task It_Reads_All_Pdf_JS()
        {
            Trace.Listeners.Clear();
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

        // [Fact]
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