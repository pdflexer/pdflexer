using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CliWrap;
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
        public void It_Loads_Pages(string pdfPath)
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var pdf = Path.Combine(tp, "pdfs", pdfPath);
            using var doc = PdfDocument.Open(File.ReadAllBytes(pdf), new ParsingOptions { LoadPageTree = true });

            var raw = new MemoryStream();
            doc.SaveTo(raw);
            File.WriteAllBytes("c:\\temp\\raw.pdf", raw.ToArray());
            using var docReRead = PdfDocument.Open(raw.ToArray(), new ParsingOptions { LoadPageTree = true });
            var ms = new MemoryStream();
            var ctx = new WritingContext(ms);
            ctx.Initialize(1.7m);
            ctx.Complete(doc.Trailer);
            using var docReRead2 = PdfDocument.Open(ms.ToArray(), new ParsingOptions { LoadPageTree = true });

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
        }

        [Fact]
        public void It_Reads_All_Pdf_JS()
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var pdfRoot = Path.Combine(tp, "pdfs", "pdfjs");
            var errors = new List<string>();
            foreach (var pdf in Directory.GetFiles(pdfRoot, "*.pdf"))
            {
                try
                {
                    var doc = PdfDocument.Open(File.ReadAllBytes(pdf));
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

        [Fact]
        public void It_Reads_All_Pdf_JS_Rebuild()
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var pdfRoot = Path.Combine(tp, "pdfs", "pdfjs", "need_repair");
            var errors = new List<string>();
            foreach (var pdf in Directory.GetFiles(pdfRoot, "*.pdf"))
            {
                var name = Path.GetFileNameWithoutExtension(pdf);
                if (name == "bug1020226" // bad page tree / structure, don't think this is something to handle by default
                    || name == "issue3371" || name == "pr6531_1" // bad compression
                    || name == "issue7229" // XRef table points to wrong object ... rebuilding might work but other pdfs have
                                           // refs to objects with incorret object number that are correct
                    // name == "issue9418" // need xref repair, os doesn't point to object
                        )
                {
                    continue;
                }
                try
                {
                    var doc = PdfDocument.Open(File.ReadAllBytes(pdf));
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
        public void It_Reads_And_Writes_All_Pdf_JS()
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var pdfRoot = Path.Combine(tp, "pdfs", "pdfjs");
            var errors = new List<string>();
            foreach (var pdf in Directory.GetFiles(pdfRoot, "*.pdf"))
            {
                try
                {
                    var data = File.ReadAllBytes(pdf);
                    using var doc = PdfDocument.Open(data);
                    using var ms = new MemoryStream();
                    doc.SaveTo(ms);
                    using var doc2 = PdfDocument.Open(ms.ToArray());

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

        [InlineData(true)]
        [InlineData(false)]
        [Theory]
        public void It_ReWrites_All_Pdf_JS_HashCheck(bool forceRebuild)
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var results = Path.Combine(tp, "results", "pighash");
            Directory.CreateDirectory(results);
            using var parseLog = new StreamWriter(Path.Combine(results, "run_log.txt"));
            var pdfRoot = Path.Combine(tp, "pdfs", "pdfjs");
            var errors = new List<string>();
            PdfDocument doc;
            byte[] d1 = null;
            byte[] d2 = null;
            byte[] d3 = null;
            byte[] d4 = null;
            decimal hc = 0;
            decimal hc2 = 0;
            decimal hc3 = 0;
            decimal hc4 = 0;
            foreach (var pdf in Directory.GetFiles(pdfRoot, "*.pdf"))
            {
                try
                {
                    if (pdf.Contains("Pages-tree-refs"))
                    {
                        // pdfpig can't handle the loop
                        continue;
                    }
                    if (pdf.Contains("xobject-image"))
                    {
                        // pdfpig misses contents due to bad stream length
                        continue;
                    }
                    if (pdf.Contains("issue5954"))
                    {
                        // known issue
                        continue;
                    }
                    if (pdf.Contains("issue6108"))
                    {
                        // pdfpig misses contents due to bad stream length
                        continue;
                    }
                    parseLog.WriteLine(pdf);
                    parseLog.Flush();
                    hc = hc2 = hc3 = hc4 = 0;
                    d1 = d2 = d3 = d4 = null;

                    d1 = File.ReadAllBytes(pdf);



                    try
                    {
                        // if pgpig can't read existin just ksip
                        hc = Util.GetDocumentHashCode(d1);
                    }
                    catch (Exception)
                    {
                        parseLog.WriteLine("Skipping PDF Pig exception.");
                        continue;
                    }

                    if (forceRebuild)
                    {
                        var copy = new byte[d1.Length + 10];
                        d1.CopyTo(copy, 10);
                        d1 = copy;
                    }

                    doc = PdfDocument.Open(d1);
                    if (doc.Trailer.ContainsKey(PdfName.Encrypt))
                    {
                        // don't support encryption currently
                        parseLog.WriteLine("Skipping encrypted.");
                        continue;
                    }
                    var ms = new MemoryStream();
                    doc.SaveTo(ms);
                    d2 = ms.ToArray();
                    using var doc2 = PdfDocument.Create();
                    doc2.Pages = doc.Pages;
                    ms = new MemoryStream();
                    doc2.SaveTo(ms);
                    d3 = ms.ToArray();
                    doc.Context.Options.ForceSerialize = true;
                    ms = new MemoryStream();
                    doc.SaveTo(ms);
                    d4 = ms.ToArray();
                    hc2 = Util.GetDocumentHashCode(d2);
                    hc3 = Util.GetDocumentHashCode(d3);
                    hc4 = Util.GetDocumentHashCode(d4);
                    Assert.Equal(hc, hc2);
                    Assert.Equal(hc, hc3);
                    Assert.Equal(hc, hc4);
                }
                catch (Exception e)
                {
                    parseLog.WriteLine($"{hc} {hc2} {hc3}");
                    var bp = Path.Combine(results, Path.GetFileNameWithoutExtension(pdf));
                    File.WriteAllBytes(bp + "_input.pdf", d1 ?? new byte[0]);
                    File.WriteAllBytes(bp + "_quicksave.pdf", d2 ?? new byte[0]);
                    File.WriteAllBytes(bp + "_pagecopy.pdf", d3 ?? new byte[0]);
                    File.WriteAllBytes(bp + "_rewrite.pdf", d4 ?? new byte[0]);
                    errors.Add(pdf + ": " + e.Message);
                }
            }
            if (errors.Any())
            {
                throw new ApplicationException(string.Join(Environment.NewLine, errors));
            }
        }

        //[Fact]
        public async Task It_ReWrites_PDF_JS()
        {
            bool copyPages = true;
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var results = Path.Combine(tp, "results", "pngjs");
            Directory.CreateDirectory(results);
            var pdfRoot = Path.Combine(tp, "pdfs", "pdfjs");
            var pngRoot = Path.Combine(tp, "pngjs");

            var errors = new List<string>();
            using var temp = new TemporaryWorkspace();
            foreach (var pdf in Directory.GetFiles(pdfRoot, "*.pdf"))
            {
                try
                {
                    using var doc = PdfDocument.Open(File.ReadAllBytes(pdf));
                    if (doc.Trailer.ContainsKey(PdfName.Encrypt))
                    {
                        continue;
                    }
                    doc.Trailer["/NewKey"] = new PdfString("NewValue");
                    var fn = Path.Combine(temp.TempPath, Path.GetFileName(pdf));
                    if (copyPages)
                    {
                        using var d2 = PdfDocument.Create();
                        d2.Pages = doc.Pages;
                        using (var fs = File.Create(fn))
                        {
                            d2.SaveTo(fs);
                        }
                    }
                    else
                    {
                        using (var fs = File.Create(fn))
                        {
                            doc.SaveTo(fs);
                        }
                    }


                    var stdOutBuffer = new StringBuilder();
                    var stdErrBuffer = new StringBuilder();

                    var result = await Cli.Wrap("node.exe")
                        .WithArguments("pdf2png.js " + pdf + " baseline.png")
                        .WithWorkingDirectory(pngRoot)
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
                        .WithArguments("pdf2png.js " + fn + " modified.png")
                        .WithWorkingDirectory(pngRoot)
                        .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                        .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
                        .ExecuteAsync();

                    var ce2 = Regex.Replace(stdErrBuffer.ToString(), "[0-9]{1,2}:[0-9]{1,2}:[0-9]{1,2}\\.[0-9]{0,3}", "");
                    ce2 = Regex.Replace(ce2, ":[0-9]{1,8}\\)", "");
                    var co2 = Regex.Replace(stdOutBuffer.ToString(), "[0-9]{1,2}:[0-9]{1,2}:[0-9]{1,2}\\.[0-9]{0,3}", "");
                    co2 = Regex.Replace(co2, ":[0-9]{1,8}\\)", "");

                    stdOutBuffer = new StringBuilder();

                    var result3 = await Cli.Wrap("node.exe")
                    .WithArguments("pngdiff.js baseline.png modified.png diff.png")
                    .WithWorkingDirectory(pngRoot)
                    .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                    .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                    .ExecuteAsync();


                    var error = "";
                    var diffOut = stdOutBuffer.ToString();
                    if (diffOut.Trim() != "0")
                    {
                        error += "Image Mismatch\n";
                    }

                    if (ce1 != ce2)
                    {
                        error += $"STDERR1:{ce1}\n";
                        error += $"STDERR2:{ce2}\n";
                    }
                    if (co1 != co2)
                    {
                        error += $"STDOUT1:{co1}\n";
                        error += $"STDOUT2:{co2}\n";
                    }

                    if (!string.IsNullOrEmpty(error))
                    {
                        errors.Add(error);
                        File.Copy(fn, Path.Combine(results, Path.GetFileName(pdf)));
                        File.Copy(Path.Combine(pngRoot, "diff.png"),
                            Path.Combine(results, Path.GetFileName(pdf) + ".png"));
                        File.WriteAllText(Path.Combine(results, Path.GetFileName(pdf) + ".txt"), error);
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
        public void It_Mega_Merges_Pdf_JS()
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var pdfRoot = Path.Combine(tp, "pdfs", "pdfjs");
            var errors = new List<string>();
            var merged = PdfDocument.Create();
            foreach (var pdf in Directory.GetFiles(pdfRoot, "*.pdf"))
            {
                using var doc = PdfDocument.Open(File.ReadAllBytes(pdf));
                merged.Pages.AddRange(doc.Pages);
            }
            var ms = new MemoryStream();
            merged.SaveTo(ms);
            using var doc2 = PdfDocument.Open(ms.ToArray());
            EnumerateObjects(doc2.Trailer, new HashSet<int>());
            File.WriteAllBytes("c:\\temp\\megamerge.pdf", ms.ToArray());
        }


        // [Fact]
        public void It_Rebuilds_PDF_JS()
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var pdfRoot = Path.Combine(tp, "pdfs", "pdfjs");
            foreach (var pdf in Directory.GetFiles(pdfRoot, "*.pdf"))
            {
                try
                {
                    var contents = File.ReadAllBytes(pdf);

                    var ctx = new ParsingContext();
                    var source = new InMemoryDataSource(ctx, contents);
                    var normal = ctx.XRefParser.LoadCrossReference(source);
                    var ms = new MemoryStream(contents);
                    var ctx2 = new ParsingContext();
                    var rebuilt = ctx2.XRefParser.BuildFromRawData(ms);
                    var nm = normal.Item1.Where(x => !x.IsFree).Select(x => x.Reference.GetId()).OrderBy(x => x).ToList();
                    var rb = rebuilt.Item1.Where(x => !x.IsFree).Select(x => x.Reference.GetId()).OrderBy(x => x).ToList();
                    var missing = normal.Item1.Where(x => !x.IsFree && !rebuilt.Item1.Any(y => y.Reference.Equals(x.Reference))).ToList();
                    Assert.True(nm.SequenceEqual(rb));
                }
                catch (Exception e)
                {

                }
            }
        }

        [Fact]
        public void It_Handles_Looped_Page_Tree()
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var pdf = Path.Combine(tp, "pdfs", "pdfjs", "Pages-tree-refs.pdf");
            var doc = PdfDocument.Open(File.ReadAllBytes(pdf));

            foreach (var item in doc.XrefEntries)
            {
                doc.Context.GetIndirectObject(item.Key);
            }
        }

        [Fact]
        public void It_Tracks_Generations_Quicksaved()
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var pdf = Path.Combine(tp, "pdfs", "pdfjs", "doc_actions.pdf");
            var doc = PdfDocument.Open(File.ReadAllBytes(pdf));
            doc.Trailer["/Dummy"] = PdfName.Count;
            var count = doc.XrefEntries.Where(x=>x.Value.Reference.Generation > 0).Count();
            var ms = new MemoryStream();
            doc.SaveTo(ms);
            var saved = PdfDocument.Open(ms.ToArray());
            var count2 = doc.XrefEntries.Where(x=>x.Value.Reference.Generation > 0).Count();
            Assert.Equal(count, count2);
        }

        [Fact]
        public void It_Repairs_Bad_Catalog_Ref()
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var pdf = Path.Combine(tp, "pdfs", "pdfjs", "need_repair", "issue9418.pdf");
            var doc = PdfDocument.Open(File.ReadAllBytes(pdf));
            Assert.Single(doc.Pages);
        }


        [Fact]
        public void It_Handles11()
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var pdf = Path.Combine(tp, "pdfs", "pdfjs", "issue918.pdf");
            var doc = PdfDocument.Open(File.ReadAllBytes(pdf));

            var read = new HashSet<int>();
            EnumerateObjects(doc.Catalog, read);
            // foreach (var item in doc.XrefEntries)
            // {
            //     doc.Context.GetIndirectObject(item.Key);
            // }
        }

        // [Fact]
        public async Task It_Handles12()
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var pdf = Path.Combine(tp, "pdfs", "pdfjs", "annotation-caret-ink.pdf");
            var doc = PdfDocument.Open(File.ReadAllBytes(pdf));

            var read = new HashSet<int>();
            EnumerateObjects(doc.Catalog, read);

            var copy = PdfDocument.Create();
            copy.Pages = doc.Pages;
            var ms = new MemoryStream();
            copy.SaveTo(ms);
            File.WriteAllBytes("c:\\temp\\temp.pdf", ms.ToArray());
            ms = new MemoryStream();
            doc.SaveTo(ms);
            using var doc2 = PdfDocument.Open(ms.ToArray());
            EnumerateObjects(doc2.Catalog, read);
            // foreach (var item in doc.XrefEntries)
            // {
            //     doc.Context.GetIndirectObject(item.Key);
            // }
        }

        // [Fact]
        public async Task It_Handles13()
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var pdf = Path.Combine(tp, "pdfs", "pdfjs", "annotation-polyline-polygon.pdf");
            var doc = PdfDocument.Open(File.ReadAllBytes(pdf));

            var read = new HashSet<int>();
            EnumerateObjects(doc.Catalog, read);

            var copy = PdfDocument.Create();
            copy.Pages = doc.Pages;
            var ms = new MemoryStream();
            copy.SaveTo(ms);
            File.WriteAllBytes("c:\\temp\\temp.pdf", ms.ToArray());
            ms = new MemoryStream();
            doc.SaveTo(ms);
            using var doc2 = PdfDocument.Open(ms.ToArray());
            EnumerateObjects(doc2.Catalog, read);
            // foreach (var item in doc.XrefEntries)
            // {
            //     doc.Context.GetIndirectObject(item.Key);
            // }
        }
        //
    }
}