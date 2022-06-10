using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CliWrap;
using PdfLexer.Content;
using PdfLexer.DOM;
using PdfLexer.IO;
using PdfLexer.Lexing;
using PdfLexer.Operators;
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
        public void It_Reads_All_Pdf_JS_Streamed()
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var pdfRoot = Path.Combine(tp, "pdfs", "pdfjs");
            var errors = new List<string>();
            foreach (var pdf in Directory.GetFiles(pdfRoot, "*.pdf"))
            {
                try
                {
                    using var fs = File.OpenRead(pdf);
                    var doc = PdfDocument.Open(fs);
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
        public void It_Reads_Text()
        {
            var errors = new List<string>();
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var pdfRoot = Path.Combine(tp, "pdfs", "pdfjs");
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
                    if (doc.Trailer.ContainsKey(PdfName.Encrypt))
                    {
                        // don't support encryption currently
                        continue;
                    }

                    foreach (var page in doc.Pages)
                    {
                        var reader = new TextScanner(doc.Context, page);
                        var sb = new StringBuilder();
                        while (reader.Advance())
                        {
                            sb.Append(reader.Glyph.C);
                        }
                        var str = sb.ToString();
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

        //[Fact]
        public void Manual_Test()
        {
            using var doc = PdfDocument.Open(File.ReadAllBytes("C:\\Users\\plamic01\\Downloads\\issue1111.pdf"));
            using var doc2 = PdfDocument.Create();
            var pg = doc.Pages[0];
            using var doc3 = PdfDocument.Create();
            doc3.Pages.Add(pg);
            var str = pg.Dictionary.GetRequiredValue<PdfStream>(PdfName.Contents);
            var copy = pg.Dictionary.CloneShallow();
            pg.Dictionary.Remove(new PdfName("Annots"));
            var decoded = str.Contents.GetDecodedData(doc.Context);
            var strCopy = new PdfStream(new PdfDictionary(), new PdfByteArrayStreamContents(decoded));
            copy[PdfName.Contents] = PdfIndirectRef.Create(strCopy);
            doc2.Pages.Add(copy);
            File.WriteAllBytes("C:\\Users\\plamic01\\Downloads\\issue1111-flat.pdf", doc2.Save());
            
            File.WriteAllBytes("C:\\Users\\plamic01\\Downloads\\issue1111-cp2.pdf", doc3.Save());
            doc.Trailer.Remove(new PdfName("/ID"));
            doc.Trailer.Remove(new PdfName("/Info"));
            var res = doc.Pages[0].Dictionary.GetRequiredValue<PdfDictionary>(PdfName.Resources);
            var fonts = res.GetRequiredValue<PdfDictionary>("/Font");
            res["/Font"] = new PdfDictionary {
                ["/wspe_F1"] = fonts["/wspe_F1"],
                // ["/wspe_F2"] = fonts["/wspe_F2"],
                // ["/wspe_F3"] = fonts["/wspe_F3"],
                ["/wspe_F4"] = fonts["/wspe_F4"],
                };
            // var empty = new DOM.PdfPage(new PdfDictionary());
            // empty.Dictionary[PdfName.Contents] = 
            //     PdfIndirectRef.Create(new PdfStream(new PdfDictionary(), new PdfByteArrayStreamContents(new byte[]{ })));
            // doc.Pages.Add(empty);
            File.WriteAllBytes("C:\\Users\\plamic01\\Downloads\\issue1111-cp3.pdf", doc.Save());
            // File.WriteAllBytes("C:\\Users\\plamic01\\Downloads\\issue1111-cp.pdf", doc.Save());
            
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

        // TODO remove? isn't testing anything really
        [Fact]
        public void It_Reads_And_Writes_All_Pdf_JS_Content_Streams()
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var pdfRoot = Path.Combine(tp, "pdfs", "pdfjs");
            var output = Path.Combine(pdfRoot, "outputStreams");
            Directory.CreateDirectory(output);
            var errors = new List<string>();
            foreach (var pdf in Directory.GetFiles(pdfRoot, "*.pdf"))
            {
                try
                {
                    var data = File.ReadAllBytes(pdf);
                    using var doc = PdfDocument.Open(data);
                    if (doc.Trailer.ContainsKey(PdfName.Encrypt))
                    {
                        // don't support encryption currently
                        continue;
                    }

                    ReWriteStreams(pdf, doc);

                    using var fs = File.Create(Path.Combine(output, Path.GetFileName(pdf)));
                    using var doc2 = PdfDocument.Create();

                    foreach (var kvp in doc.Trailer)
                    {
                        doc2.Trailer[kvp.Key] = kvp.Value;
                    }
                    doc2.Catalog = doc.Catalog;
                    doc2.Pages = doc.Pages;
                    doc2.SaveTo(fs);
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

        private static PdfPage ReWriteStream(PdfDocument doc, PdfPage page, bool clone)
        {
            if (!page.Dictionary.TryGetValue(PdfName.Contents, out var value))
            {
                return page;
            }

            var streamObj = value.Resolve();
            byte[] newData = null;
            switch (streamObj)
            {
                case PdfStream strObj:
                    {
                        using var str = strObj.Contents.GetDecodedStream(doc.Context);
                        var msc = new MemoryStream();
                        str.CopyTo(msc);
                        newData = CheckStreamData(msc.ToArray());
                        break;
                    }
                case PdfArray arrObj:
                    var streams = new List<PdfStream>();
                    foreach (var item in arrObj)
                    {
                        var arStr = item.Resolve();
                        if (arStr is PdfStream matched)
                        {
                            streams.Add(matched);
                        }
                        else
                        {
                            throw new ApplicationException("Non stream object found in contents array: " + arStr.Type);
                        }
                    }
                    // TODO support for split streams
                    var ms = new MemoryStream();
                    foreach (var str in streams)
                    {
                        str.Contents.GetDecodedStream(doc.Context).CopyTo(ms);
                        ms.WriteByte((byte)' ');
                    }
                    newData = CheckStreamData(ms.ToArray());
                    break;
                default:
                    throw new ApplicationException("Non stream or array object found in contents array:" + streamObj.Type);
            }

            if (clone)
            {
                page = page.Dictionary.CloneShallow();
            }
            var updatedStr = new PdfStream(new PdfDictionary(), new PdfByteArrayStreamContents(newData));
            page.Dictionary[PdfName.Contents] = PdfIndirectRef.Create(updatedStr);
            return page;

            byte[] CheckStreamData(byte[] pgStream)
            {
                //var txt = Encoding.ASCII.GetString(pgStream);
                var scanner = new ContentScanner(doc.Context, pgStream);
                var ops = new List<IPdfOperation>();
                PdfOperatorType nxt = PdfOperatorType.Unknown;
                while ((nxt = scanner.Peek()) != PdfOperatorType.EOC)
                {
                    var op = scanner.GetCurrentOperation();
                    if (op == null)
                    {

                    }
                    else
                    {
                        ops.Add(op);

                    }
                    scanner.SkipCurrent();
                }
                var ms = new MemoryStream();
                foreach (var op in ops)
                {
                    op.Serialize(ms);
                    ms.WriteByte((byte)'\n');
                }
                var rewritten = ms.ToArray();
                // var txt2 = Encoding.ASCII.GetString(rewritten);
                return rewritten;
            }
        }

        private static PdfPage ReWriteStream2(PdfDocument doc, PdfPage page, bool clone)
        {
            var scanner = new PageContentScanner(doc.Context, page, false);
            var ms = new MemoryStream();

            while (scanner.Peek() != PdfOperatorType.EOC)
            {
                var op = scanner.GetCurrentOperation();
                if (op != null)
                {
                    op.Serialize(ms);
                    ms.WriteByte((byte)'\n');
                } else
                {

                }
                
                scanner.SkipCurrent();
            }
            

            if (clone)
            {
                page = page.Dictionary.CloneShallow();
            }

            var updatedStr = new PdfStream(new PdfDictionary(), new PdfByteArrayStreamContents(ms.ToArray()));
            page.Dictionary[PdfName.Contents] = PdfIndirectRef.Create(updatedStr);
            return page;
        }

        private static void ReWriteStreams(string pdf, PdfDocument doc)
        {
            foreach (var page in doc.Pages)
            {
                ReWriteStream2(doc, page, false);
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

        [Fact]
        public void It_Reads_And_Writes_Zapf()
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var results = Path.Combine(tp, "results", "pighash");
            var result = Path.Combine(results, "ZapfDingbats_rewrite.pdf");
            Directory.CreateDirectory(results);
            var pdf = Path.Combine(tp, "pdfs", "pdfjs", "ZapfDingbats.pdf");
            var data = File.ReadAllBytes(pdf);
            using var doc = PdfDocument.Open(data);
            using var ms = new MemoryStream();
            doc.SaveTo(ms);
            using var doc2 = PdfDocument.Open(ms.ToArray());

            EnumerateObjects(doc2.Catalog, new HashSet<int>());
            File.WriteAllBytes(result, ms.ToArray());
        }

        [Fact]
        public void It_Enumerates_Stream()
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var pdf = Path.Combine(tp, "pdfs", "pdfjs", "issue1905.pdf");

            using var fs2 = File.OpenRead(pdf);
            using var doc = PdfDocument.Open(fs2);
            var pg = doc.Pages.First();
            var reader = new PageContentScanner(doc.Context, pg);
            while (reader.Peek() != PdfOperatorType.EOC)
            {
                var op = reader.GetCurrentOperation();
                reader.SkipCurrent();
            }
        }

        [Fact]
        public void It_Doesnt_Dedup_Different_Images()
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var pdf = Path.Combine(tp, "pdfs", "pdfjs", "issue1905.pdf");

            var orig = Util.GetDocumentHashCode(pdf);
            

            using var fs2 = File.OpenRead(pdf);
            using var lm = PdfDocument.OpenLowMemory(fs2);
            var mssw = new MemoryStream();
            using var writer = new StreamingWriter(mssw, true, true);
            foreach (var page in lm.Pages)
            {
                writer.AddPage(page);
            }
            writer.Complete(new PdfDictionary());
            var after = mssw.ToArray();
            var ah = Util.GetDocumentHashCode(after);

            Assert.Equal(orig, ah);
        }

        [InlineData(true)]
        [InlineData(false)]
        [Theory]
        public void It_Dedups_Same_Images(bool inMemory)
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var pdf = Path.Combine(tp, "pdfs", "pdfjs", "issue1905.pdf");
            var orig = File.ReadAllBytes(pdf);

            using var doc = PdfDocument.Create();
            var mssw = new MemoryStream();
            using var writer = new StreamingWriter(mssw, true, inMemory);

            // write same file twice
            using var fs = File.OpenRead(pdf);
            using var lm = PdfDocument.OpenLowMemory(fs);
            foreach (var page in lm.Pages)
            {
                writer.AddPage(page);
            }
            doc.Pages.AddRange(lm.Pages);
            using var second = PdfDocument.Open(orig);
            foreach (var page in second.Pages)
            {
                writer.AddPage(page);
            }
            doc.Pages.AddRange(second.Pages);
            
            writer.Complete(new PdfDictionary());
            var deDuped = mssw.ToArray();
            var nonDeDuped = doc.Save();

            var dd = Util.GetDocumentHashCode(deDuped);
            var ndd = Util.GetDocumentHashCode(nonDeDuped);

            Assert.Equal(dd, ndd);

            var or = Util.CountResources(orig);
            var ddr = Util.CountResources(deDuped);
            var nddr = Util.CountResources(nonDeDuped);
            Assert.Equal(or, ddr);
            Assert.Equal(nddr, ddr*2);
            Assert.True(deDuped.Length < nonDeDuped.Length);
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
            byte[] d5 = null;
            byte[] d6 = null;
            byte[] d7 = null;
            decimal hc = 0;
            decimal hc2 = 0;
            decimal hc3 = 0;
            decimal hc4 = 0;
            decimal hc5 = 0;
            decimal hc6 = 0;
            decimal hc7 = 0;
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
                    hc = hc2 = hc3 = hc4 = hc5 = hc6 = hc7 = 0;
                    d1 = d2 = d3 = d4 = d5 = d6 = d7 = null;

                    d1 = File.ReadAllBytes(pdf);

                    try
                    {
                        // if pgpig can't read existin just skip
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

                    bool skipStream = false;
                    using var fs = File.OpenRead(pdf);
                    
                    try
                    {
                        PdfDocument streamedDoc = PdfDocument.Open(fs);
                        using var sd = PdfDocument.Create();
                        sd.Pages = streamedDoc.Pages;
                        d6 = sd.Save();
                    } catch (NotImplementedException)
                    {
                        // haven't done repairs yet
                        skipStream = true;
                    }

                    if (!skipStream)
                    {
                        using var fs2 = File.OpenRead(pdf);
                        using var lm = PdfDocument.OpenLowMemory(fs2);
                        var mssw = new MemoryStream();
                        using var writer = new StreamingWriter(mssw);
                        foreach (var page in lm.Pages)
                        {
                            // var modified = ReWriteStream(lm, page, true);
                            writer.AddPage(page);
                        }
                        writer.Complete(new PdfDictionary());
                        d7 = mssw.ToArray();
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
                    doc = PdfDocument.Open(d1);
                    ReWriteStreams(pdf, doc);
                    ms = new MemoryStream();
                    using var doc4 = PdfDocument.Create();
                    doc4.Pages = doc.Pages;
                    doc4.SaveTo(ms);
                    d5 = ms.ToArray();

                    hc2 = Util.GetDocumentHashCode(d2);
                    hc3 = Util.GetDocumentHashCode(d3);
                    hc4 = Util.GetDocumentHashCode(d4);
                    hc5 = Util.GetDocumentHashCode(d5);
                    Assert.Equal(hc, hc2);
                    Assert.Equal(hc, hc3);
                    Assert.Equal(hc, hc4);
                    Assert.Equal(hc, hc5);
                    if (!skipStream)
                    {
                        hc6 = Util.GetDocumentHashCode(d6);
                        Assert.Equal(hc, hc6);
                        hc7 = Util.GetDocumentHashCode(d7);
                        Assert.Equal(hc, hc7);
                    }
                }
                catch (Exception e)
                {
                    parseLog.WriteLine($"{hc} {hc2} {hc3} {hc4} {hc5} {hc6} {hc7}");
                    var bp = Path.Combine(results, Path.GetFileNameWithoutExtension(pdf));
                    File.WriteAllBytes(bp + "_input.pdf", d1 ?? new byte[0]);
                    File.WriteAllBytes(bp + "_quicksave.pdf", d2 ?? new byte[0]);
                    File.WriteAllBytes(bp + "_pagecopy.pdf", d3 ?? new byte[0]);
                    File.WriteAllBytes(bp + "_rewrite.pdf", d4 ?? new byte[0]);
                    File.WriteAllBytes(bp + "_cstreams.pdf", d5 ?? new byte[0]);
                    File.WriteAllBytes(bp + "_fromStream.pdf", d6 ?? new byte[0]);
                    File.WriteAllBytes(bp + "_fromLMStream.pdf", d7 ?? new byte[0]);
                    errors.Add(pdf + ": " + e.Message);
                }
            }
            if (errors.Any())
            {
                throw new ApplicationException(string.Join(Environment.NewLine, errors));
            }
        }

        [Fact]
        public async Task It_Dedups_FreeCulture()
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var results = Path.Combine(tp, "results", "pighash");
            Directory.CreateDirectory(results);
            var pdfRoot = Path.Combine(tp, "pdfs", "pdfjs");
            var pdf = Path.Combine(pdfRoot, "freeculture.pdf");
            using var fs2 = File.OpenRead(pdf);
            using var lm = PdfDocument.OpenLowMemory(fs2);
            var mssw = new MemoryStream();
            using var writer = new StreamingWriter(mssw, true, true);
            foreach (var page in lm.Pages)
            {
                // var modified = ReWriteStream(lm, page, true);
                writer.AddPage(page);
            }
            writer.Complete(new PdfDictionary());
            File.WriteAllBytes(Path.Combine(results, "freeculture_out.pdf"), mssw.ToArray());
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
                foreach (var page in doc.Pages) { CommonUtil.RecursiveLoad(page.Dictionary); }
                merged.Pages.AddRange(doc.Pages);
            }
            var ms = new MemoryStream();
            merged.SaveTo(ms);
            using var doc2 = PdfDocument.Open(ms.ToArray(), new ParsingOptions { ThrowOnErrors = true });
            EnumerateObjects(doc2.Trailer, new HashSet<int>());
            // File.WriteAllBytes("c:\\temp\\megamerge.pdf", ms.ToArray());
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

            var hasher = new TreeHasher();
            var result = hasher.GetHash(doc.Trailer);
            var result2 = hasher.GetHash(doc.Trailer);
            Assert.Equal(result, result2);

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
            var count = doc.XrefEntries.Where(x => x.Value.Reference.Generation > 0).Count();
            var ms = new MemoryStream();
            doc.SaveTo(ms);
            var saved = PdfDocument.Open(ms.ToArray());
            var count2 = doc.XrefEntries.Where(x => x.Value.Reference.Generation > 0).Count();
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