using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PdfLexer.Benchmarks.Benchmarks
{
    [Config(typeof(BenchmarkConfig))]
    public class SplitBenchmark
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
        private List<MemoryStream> mems;

        public SplitBenchmark()
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            var src = GetPathFromSegmentOfCurrent("PdfLexer.Benchmarks");
            var pdfRoot = Path.Combine(src, "output");
            pdfs = new List<byte[]>();
            mems = new List<MemoryStream>();
            Add("pdflexer.merge");
            Add("pdfpig.merge");
            Add("pdfsharp.merge");
            
            void Add(string name)
            {
                var data = File.ReadAllBytes(Path.Combine(pdfRoot, name + ".pdf"));
                pdfs.Add(data);
                mems.Add(new MemoryStream(data));
            }
        }

        [Benchmark(Baseline = true)]
        public List<MemoryStream> SplitPdfSharp()
        {
            var results =  new List<MemoryStream>();
            foreach (var pdf in mems)
            {
                pdf.Seek(0, SeekOrigin.Begin);
                var doc = PdfSharp.Pdf.IO.PdfReader.Open(pdf, PdfSharp.Pdf.IO.PdfDocumentOpenMode.Import);

                for (var i=0; i<doc.PageCount; i++)
                {
                    var outputDocument = new PdfSharp.Pdf.PdfDocument();
                    outputDocument.Version = doc.Version;
                    outputDocument.Info.Title = doc.Info.Title;
                    outputDocument.Info.Creator = doc.Info.Creator;

                    // Add the page and save it
                    outputDocument.AddPage(doc.Pages[i]);
                    var ms = new MemoryStream();
                    outputDocument.Save(ms);
                    results.Add(ms);
                }
            }
            
            return results;
        }

        [Benchmark()]
        public List<byte[]> SplitPdfPig()
        {
            var results = new List<byte[]>();
            foreach (var pdf in pdfs)
            {
                var doc = UglyToad.PdfPig.PdfDocument.Open(pdf);
                for (var i = 0; i < doc.NumberOfPages; i++)
                {
                    var output = new UglyToad.PdfPig.Writer.PdfDocumentBuilder();
                    output.AddPage(doc, i+1);
                    results.Add(output.Build());
                }
            }
            return results;
        }

        [Benchmark()]
        public List<byte[]> SplitPdfLexer()
        {
            var results = new List<byte[]>();
            foreach (var pdf in pdfs)
            {
                var doc = PdfDocument.Open(pdf);
                for (var i=0;i<doc.Pages.Count;i++)
                {
                    var output = PdfDocument.Create();
                    output.Pages.Add(doc.Pages[i]);
                    results.Add(output.Save());
                }
            }
            return results;
        }

        [Benchmark()]
        public List<byte[]> SplitPdfLexerLazyStrings()
        {
            var results = new List<byte[]>();
            foreach (var pdf in pdfs)
            {
                var doc = PdfDocument.Open(pdf, new ParsingOptions() { LazyStrings = true });
                for (var i = 0; i < doc.Pages.Count; i++)
                {
                    var output = PdfDocument.Create();
                    output.Pages.Add(doc.Pages[i]);
                    results.Add(output.Save());
                }
            }
            return results;
        }
    }
}
