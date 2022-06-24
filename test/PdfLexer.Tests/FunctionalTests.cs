using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CliWrap;
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
                    EnumerateObjects(doc.Trailer, new HashSet<int>());
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

        private void EnumerateObjects(IPdfObject obj, HashSet<int> callStack)
        {
            if (obj is PdfIndirectRef ir)
            {
                if (callStack.Contains(ir.Reference.ObjectNumber))
                {
                    return;
                }
                callStack.Add(ir.Reference.ObjectNumber);
            }
            obj = obj.Resolve();

            if (obj is PdfArray arr)
            {
                foreach (var item in arr)
                {
                    EnumerateObjects(item, callStack);
                }
            }
            else if (obj is PdfDictionary dict)
            {
                foreach (var item in dict.Values)
                {
                    EnumerateObjects(item, callStack);
                }
            }
        }

        [Fact]
        public async Task It_Reads_And_Writes_All_Pdf_JS()
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var pdfRoot = Path.Combine(tp, "pdfs", "pdfjs");
            var errors = new List<string>();
            foreach (var pdf in Directory.GetFiles(pdfRoot, "*.pdf"))
            {
                try
                {
                    using var doc = await PdfDocument.Open(File.ReadAllBytes(pdf));
                    using var ms = new MemoryStream();
                    doc.SaveTo(ms);
                    using var doc2 = await PdfDocument.Open(ms.ToArray());

                    EnumerateObjects(doc2.Catalog, new HashSet<int>());
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
        public async Task It_ReWrites_PDF_JS()
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var pdfRoot = Path.Combine(tp, "pdfs", "pdfjs");
            var errors = new List<string>();
            using var temp = new TemporaryWorkspace();
            foreach (var pdf in Directory.GetFiles(pdfRoot, "*.pdf"))
            {
                try
                {
                    using var doc = await PdfDocument.Open(File.ReadAllBytes(pdf));
                    doc.Trailer["/NewKey"] = new PdfString("NewValue");
                    var fn = Path.Combine(temp.TempPath, Path.GetFileName(pdf));
                    using (var fs = File.Create(fn))
                    {
                        doc.SaveTo(fs);
                    }

                    var stdOutBuffer = new StringBuilder();
                    var stdErrBuffer = new StringBuilder();

                    var result = await Cli.Wrap("node.exe")
                        .WithArguments("pdf2png.js " + pdf)
                        .WithWorkingDirectory(@"C:\source\Github\pdflexer\test\pnger")
                        .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                        .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
                        .ExecuteAsync();

                    var ce1 = Regex.Replace(stdErrBuffer.ToString(), "[0-9]{1,2}:[0-9]{1,2}:[0-9]{1,2}\\.[0-9]{0,3}", "");
                    ce1 = Regex.Replace(ce1, ":[0-9]{1,8}\\)", "");
                    var co1 = Regex.Replace(stdOutBuffer.ToString(), "[0-9]{1,2}:[0-9]{1,2}:[0-9]{1,2}\\.[0-9]{0,3}", "");
                    co1 = Regex.Replace(co1, ":[0-9]{1,8}\\)", "");

                    stdOutBuffer = new StringBuilder();
                    stdErrBuffer = new StringBuilder();

                    var result2 = await Cli.Wrap("node.exe")
                        .WithArguments("pdf2png.js " + fn)
                        .WithWorkingDirectory(@"C:\source\Github\pdflexer\test\pnger")
                        .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                        .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
                        .ExecuteAsync();

                    var ce2 = Regex.Replace(stdErrBuffer.ToString(), "[0-9]{1,2}:[0-9]{1,2}:[0-9]{1,2}\\.[0-9]{0,3}", "");
                    ce2 = Regex.Replace(ce2, ":[0-9]{1,8}\\)", "");
                    var co2 = Regex.Replace(stdOutBuffer.ToString(), "[0-9]{1,2}:[0-9]{1,2}:[0-9]{1,2}\\.[0-9]{0,3}", "");
                    co2 = Regex.Replace(co2, ":[0-9]{1,8}\\)", "");

                    Assert.Equal(ce1, ce2);
                    Assert.Equal(co1, co2);

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
        public async Task It_Mega_Merges_Pdf_JS()
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var pdfRoot = Path.Combine(tp, "pdfs", "pdfjs");
            var errors = new List<string>();
            var merged = PdfDocument.Create();
            foreach (var pdf in Directory.GetFiles(pdfRoot, "*.pdf"))
            {
                using var doc = await PdfDocument.Open(File.ReadAllBytes(pdf));
                merged.Pages.AddRange(doc.Pages);
            }
            var ms = new MemoryStream();
            merged.SaveTo(ms);
            using var doc2 = await PdfDocument.Open(ms.ToArray());
            EnumerateObjects(doc2.Trailer, new HashSet<int>());
            File.WriteAllBytes("c:\\temp\\megamerge.pdf", ms.ToArray());
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

        [Fact]
        public async Task It_Handles_Looped_Page_Tree()
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var pdf = Path.Combine(tp, "pdfs", "pdfjs", "Pages-tree-refs.pdf");
            var doc = await PdfDocument.Open(File.ReadAllBytes(pdf));

            foreach (var item in doc.XrefEntries)
            {
                doc.Context.GetIndirectObject(item.Key);
            }
        }


        [Fact]
        public async Task It_Handles11()
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var pdf = Path.Combine(tp, "pdfs", "pdfjs", "issue918.pdf");
            var doc = await PdfDocument.Open(File.ReadAllBytes(pdf));

            var read = new HashSet<int>();
            EnumerateObjects(doc.Catalog, read);
            // foreach (var item in doc.XrefEntries)
            // {
            //     doc.Context.GetIndirectObject(item.Key);
            // }
        }

        [Fact]
        public async Task It_Handles12()
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var pdf = Path.Combine(tp, "pdfs", "pdfjs", "annotation-caret-ink.pdf");
            var doc = await PdfDocument.Open(File.ReadAllBytes(pdf));

            var read = new HashSet<int>();
            EnumerateObjects(doc.Catalog, read);

            var copy = PdfDocument.Create();
            copy.Pages = doc.Pages;
            var ms = new MemoryStream();
            copy.SaveTo(ms);
            File.WriteAllBytes("c:\\temp\\temp.pdf", ms.ToArray());
            ms = new MemoryStream();
            doc.SaveTo(ms);
            using var doc2 = await PdfDocument.Open(ms.ToArray());
            EnumerateObjects(doc2.Catalog, read);
            // foreach (var item in doc.XrefEntries)
            // {
            //     doc.Context.GetIndirectObject(item.Key);
            // }
        }
        [Fact]
        public async Task It_Handles13()
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var pdf = Path.Combine(tp, "pdfs", "pdfjs", "annotation-polyline-polygon.pdf");
            var doc = await PdfDocument.Open(File.ReadAllBytes(pdf));

            var read = new HashSet<int>();
            EnumerateObjects(doc.Catalog, read);

            var copy = PdfDocument.Create();
            copy.Pages = doc.Pages;
            var ms = new MemoryStream();
            copy.SaveTo(ms);
            File.WriteAllBytes("c:\\temp\\temp.pdf", ms.ToArray());
            ms = new MemoryStream();
            doc.SaveTo(ms);
            using var doc2 = await PdfDocument.Open(ms.ToArray());
            EnumerateObjects(doc2.Catalog, read);
            // foreach (var item in doc.XrefEntries)
            // {
            //     doc.Context.GetIndirectObject(item.Key);
            // }
        }
        //
    }
}