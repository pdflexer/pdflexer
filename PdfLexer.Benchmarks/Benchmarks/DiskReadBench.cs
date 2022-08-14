using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PdfLexer.Benchmarks.Benchmarks
{
    [Config(typeof(BenchmarkConfig))]
    public class DiskReadBench
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

        private List<string> paths;

        // not sure how good of a measurement this is, reading same files a bunch 
        // probably not very realistic
        public DiskReadBench()
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            var src = GetPathFromSegmentOfCurrent("PdfLexer.Benchmarks");
            var root = Path.GetFullPath(Path.Combine(src, ".."));
            var pdfRoot = Path.Combine(root, "test", "pdfs", "pdfjs");
            paths = new List<string>();
            Add("__pdf.pdf");
            Add("__geothermal.pdf");
            Add("__issue11230.pdf");
            // Add("franz");
            // Add("issue925");

            void Add(string name)
            {
                paths.Add(Path.Combine(pdfRoot, name + ".pdf"));
            }
        }


        [Benchmark(Baseline = true)]
        public void RunMemory()
        {
            foreach (var pdf in paths)
            {
                var data = File.ReadAllBytes(pdf);
                using var doc = PdfDocument.Open(data);
                Read(doc);
            }
        }

        [Benchmark()]
        public void RunStream()
        {
            foreach (var pdf in paths)
            {
                using var ms = File.OpenRead(pdf);
                using var doc = PdfDocument.Open(ms);
                Read(doc);
            }
        }

        [Benchmark()]
        public void RunMapped()
        {
            foreach (var pdf in paths)
            {
                using var doc = PdfDocument.OpenMapped(pdf);
                Read(doc);
            }
        }

        private void Read(PdfDocument doc)
        {
            foreach (var page in doc.Pages)
            {
                RecurseDict(page.NativeObject, new HashSet<PdfDictionary>());
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
