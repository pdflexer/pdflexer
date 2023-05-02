using BenchmarkDotNet.Attributes;
using PDFiumCore;
using PdfLexer.Content;
using PdfLexer.Content.Model;
using PdfLexer.Serializers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace PdfLexer.Benchmarks.Benchmarks
{
    [Config(typeof(BenchmarkConfig))]
    public class ModelBenchmark
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

        // "issue2128r" pdflexer need to research
        [Params("__bpl13210.pdf", "bug1669099.pdf", "issue1905.pdf")] //, "__ecma262.pdf", "__gesamt.pdf", "__issue1133.pdf", "issue2128r.pdf")]
        public string testPdf;

        [GlobalSetup]
        public void Setup()
        {
            CMaps.AddKnownPdfCMaps();
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            var src = GetPathFromSegmentOfCurrent("PdfLexer.Benchmarks");
            var root = Path.GetFullPath(Path.Combine(src, "..", ".."));
            var pdfRoot = Path.Combine(root, "test", "pdfs", "pdfjs");
            pdfs = new List<byte[]>();
            paths = new List<string>();
            mems = new List<MemoryStream>();
            Add(testPdf);
            void Add(string name)
            {
                var path = Path.Combine(pdfRoot, name);
                // var path = "c:\\temp\\vector2.pdf";
                paths.Add(path);
                var data = File.ReadAllBytes(path);
                pdfs.Add(data);
                mems.Add(new MemoryStream(data));
            }
        }


        [Benchmark(Baseline = true)]
        public decimal ParseModelDouble()
        {
            return ParseModel<double>();
        }

        [Benchmark()]
        public decimal ParseModelFloat()
        {
            return ParseModel<float>();
        }

        [Benchmark()]
        public decimal ParseModelDecimal()
        {
            return ParseModel<decimal>();
        }

        private decimal ParseModel<T>() where T : struct, IFloatingPoint<T>
        {
            T total = T.Zero;
            foreach (var pdf in pdfs)
            {
                using var doc = PdfDocument.Open(pdf);
                foreach (var page in doc.Pages)
                {
                    var parser = new ContentModelParser<T>(doc.Context, page, true);
                    var val = parser.Parse();
                    foreach (var item in val)
                    {
                        var rect = item.GetBoundingBox();
                        unchecked { total += rect.LLx; }
                    }
                }
            }
            return FPC<T>.Util.ToDecimal(total);
        }
    }
}
