using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PdfLexer.Benchmarks.Benchmarks
{
    [Config(typeof(BenchmarkConfig))]
    public class ReadingBenchmark
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

        public ReadingBenchmark()
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


        [Benchmark()]
        public void RunMemory()
        {
            foreach (var pdf in pdfs)
            {
                var data = new byte[pdf.Length];
                Array.Copy(pdf, data, pdf.Length);
                var doc = PdfDocument.Open(pdf);
                Read(doc);
            }
        }

        [Benchmark()]
        public void RunStream()
        {
            foreach (var pdf in pdfs)
            {
                var ms = new MemoryStream(pdf);
                var doc = PdfDocument.Open(ms);
                Read(doc);
            }
        }

        private void Read(PdfDocument doc)
        {
            foreach (var page in doc.Pages)
            {
                RecurseDict(page.Dictionary, new HashSet<PdfDictionary>());
            }
        }

        private void RecurseDict(PdfDictionary dict, HashSet<PdfDictionary> dicts)
        {
            if (dicts.Contains(dict)) { return; }
            dicts.Add(dict);
            foreach (var item in dict)
            {
                var nest = item.Value.Resolve();
                if (nest.Type == PdfObjectType.DictionaryObj)
                {
                    RecurseDict((PdfDictionary)nest, dicts);
                }
            }
        }
    }
}
