using BenchmarkDotNet.Attributes;
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
        private List<MemoryStream> mems;

        public MergeBenchmark()
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            var src = GetPathFromSegmentOfCurrent("PdfLexer.Benchmarks");
            var root = Path.GetFullPath(Path.Combine(src, ".."));
            var pdfRoot = Path.Combine(root, "test", "pdfs", "pdfjs");
            pdfs = new List<byte[]>();
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
                var data = File.ReadAllBytes(Path.Combine(pdfRoot, name + ".pdf"));
                pdfs.Add(data);
                mems.Add(new MemoryStream(data));
            }
        }

        [Benchmark(Baseline = true)]
        public MemoryStream MergePdfSharp()
        {
            var finished = new PdfSharp.Pdf.PdfDocument();
            foreach (var pdf in mems)
            {
                pdf.Seek(0, SeekOrigin.Begin);
                var doc = PdfSharp.Pdf.IO.PdfReader.Open(pdf, PdfSharp.Pdf.IO.PdfDocumentOpenMode.Import);
                for (var i = 0; i < doc.PageCount; i++)
                {
                    finished.AddPage(doc.Pages[i]);
                }
            }
            var ms = new MemoryStream();
            finished.Save(ms);
            return ms;
        }

        [Benchmark()]
        public byte[] MergePdfPig()
        {
            var finished = new UglyToad.PdfPig.Writer.PdfDocumentBuilder();
            
            foreach (var pdf in pdfs)
            {
                var doc = UglyToad.PdfPig.PdfDocument.Open(pdf);
                for (var i=0;i<doc.NumberOfPages;i++)
                {
                    finished.AddPage(doc, i+1);
                } 
            }
            return finished.Build(); 
        }

        [Benchmark()]
        public byte[] MergePdfLexer()
        {
            var finished = PdfDocument.Create();
            foreach (var pdf in pdfs)
            {
                var doc = PdfDocument.Open(pdf);
                finished.Pages.AddRange(doc.Pages);
            }
            return finished.Save();
        }
    }
}
