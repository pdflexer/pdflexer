using BenchmarkDotNet.Attributes;
using PdfLexer.Serializers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PdfLexer.Benchmarks.Benchmarks
{
    [Config(typeof(BenchmarkConfig))]
    public class MergeBenchmark
    {
        public static string GetPathFromSegmentOfCurrent(string segment)
        {
            return GetPathFromSegment(segment, Environment.CurrentDirectory);
        }
        public static string GetPathFromSegment(string segment, string path)
        {
            var split = path.Split(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            int index = Array.FindLastIndex(split, t => t.Equals(segment, StringComparison.InvariantCultureIgnoreCase));
            if (index == -1)
            {
                throw new FileNotFoundException("Folder to set relative to not found");
            }
            return string.Join(Path.DirectorySeparatorChar.ToString(), split.Take(index + 1));

        }

        private List<byte[]> pdfs;
        private List<string> paths;
        private List<MemoryStream> mems;

        public MergeBenchmark()
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            var src = GetPathFromSegmentOfCurrent("PdfLexer.Benchmarks");
            var root = Path.GetFullPath(Path.Combine(src, ".."));
            var pdfRoot = Path.Combine(root, "test", "pdfs", "pdfjs");
            pdfs = new List<byte[]>();
            paths = new List<string>();
            mems = new List<MemoryStream>();
            Add("calgray");
            Add("canvas");
            Add("clippath");
            Add("franz");
            Add("issue925");

            // File.WriteAllBytes(Path.Combine(src, "output", "pdflexer.merge.pdf"), MergePdfLexer());
            // File.WriteAllBytes(Path.Combine(src, "output", "pdfpig.merge.pdf"), MergePdfPig());
            // using var fs = File.Create(Path.Combine(src, "output", "pdfsharp.merge.pdf"));
            // var ms = MergePdfSharp();
            // ms.CopyTo(fs);

            void Add(string name)
            {
                var path = Path.Combine(pdfRoot, name + ".pdf");
                paths.Add(path);
                var data = File.ReadAllBytes(path);
                pdfs.Add(data);
                mems.Add(new MemoryStream(data));
            }
        }

        [Benchmark(Baseline = true)]
        public MemoryStream MergePdfSharp()
        {
            var finished = new PdfSharp.Pdf.PdfDocument();
            foreach (var pdf in paths)
            {
                // pdf.Seek(0, SeekOrigin.Begin);
                using var doc = PdfSharp.Pdf.IO.PdfReader.Open(pdf, PdfSharp.Pdf.IO.PdfDocumentOpenMode.Import);
                for (var i = 0; i < doc.PageCount; i++)
                {
                    finished.AddPage(doc.Pages[i]);
                }
            }
            var ms = new MemoryStream();
            finished.Save(ms);
            return ms;
        }

        // [Benchmark()]
        public byte[] MergePdfPig()
        {
            var finished = new UglyToad.PdfPig.Writer.PdfDocumentBuilder();
            
            foreach (var pdf in pdfs)
            {
                using var doc = UglyToad.PdfPig.PdfDocument.Open(pdf);
                for (var i=0;i<doc.NumberOfPages;i++)
                {
                    finished.AddPage(doc, i+1);
                } 
            }
            return finished.Build(); 
        }

        [Benchmark()]
        public byte[] MergePdfLexerMem()
        {
            var finished = PdfDocument.Create();
            foreach (var pdf in paths)
            {
                var doc = PdfDocument.Open(File.ReadAllBytes(pdf));
                finished.Pages.AddRange(doc.Pages);
            }
            return finished.Save();
        }


        [Benchmark()]
        public MemoryStream MergePdfLexerMemStreamed()
        {
            var ms = new MemoryStream();
            var sw = new StreamingWriter(ms);
            foreach (var pdf in paths)
            {
                var doc = PdfDocument.Open(File.ReadAllBytes(pdf));
                doc.Pages.ForEach(p => sw.AddPage(p));
            }
            sw.Complete(new PdfDictionary());
            return ms;
        }

        [Benchmark()]
        public MemoryStream MergePdfLexerMapped()
        {
            var ms = new MemoryStream();
            var sw = new StreamingWriter(ms);
            foreach (var pdf in paths)
            {
                using var doc = PdfDocument.OpenMapped(pdf);
                doc.Pages.ForEach(p => sw.AddPage(p));
            }
            sw.Complete(new PdfDictionary());
            return ms;
        }

        // [Benchmark()]
        // public byte[] MergePdfLexerStream()
        // {
        //     var finished = PdfDocument.Create();
        //     foreach (var pdf in mems)
        //     {
        //         pdf.Seek(0, SeekOrigin.Begin);
        //         var doc = PdfDocument.Open(pdf);
        //         finished.Pages.AddRange(doc.Pages);
        //     }
        //     return finished.Save();
        // }
        // [Benchmark()]
        // public byte[] MergePdfLexerStreamEager()
        // {
        //     var finished = PdfDocument.Create();
        //     foreach (var pdf in mems)
        //     {
        //         pdf.Seek(0, SeekOrigin.Begin);
        //         var doc = PdfDocument.Open(pdf, new ParsingOptions {  Eagerness = Eagerness.FullEager });
        //         finished.Pages.AddRange(doc.Pages);
        //     }
        //     return finished.Save();
        // }
    }
}
