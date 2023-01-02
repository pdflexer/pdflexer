using BenchmarkDotNet.Attributes;
using PDFiumCore;
using PdfLexer.Content;
using PdfLexer.Operators;
using PdfLexer.Serializers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PdfLexer.Benchmarks.Benchmarks
{
    [Config(typeof(BenchmarkConfig))]
    public class ScannerBenchmark
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
        [Params( "bug1669099")]//, "__bpl13210.pdf", "issue1905", "__ecma262.pdf", "__gesamt.pdf", "__issue1133.pdf", "issue2128r")]
        public string testPdf;

        [GlobalSetup]
        public void Setup()
        {
            CMaps.AddKnownPdfCMaps();
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            var src = GetPathFromSegmentOfCurrent("PdfLexer.Benchmarks");
            var root = Path.GetFullPath(Path.Combine(src, ".."));
            var pdfRoot = Path.Combine(root, "test", "pdfs", "pdfjs");
            pdfs = new List<byte[]>();
            paths = new List<string>();
            mems = new List<MemoryStream>();
            Add(testPdf);
            V1();
            V2();
            void Add(string name)
            {
                var path = Path.Combine(pdfRoot, name + ".pdf");
                // var path = "c:\\temp\\vector2.pdf";
                paths.Add(path);
                var data = File.ReadAllBytes(path);
                pdfs.Add(data);
                mems.Add(new MemoryStream(data));
            }
        }


        [Benchmark(Baseline = true)]
        public int V1()
        {
            int total = 0;
            int chars = 0;
            foreach (var pdf in pdfs)
            {
                using var doc = PdfDocument.Open(pdf);
                foreach (var page in doc.Pages)
                {
                    var reader = new PageContentScanner(doc.Context, page);
                    PdfOperatorType op;
                    while ((op = reader.Peek()) != PdfOperatorType.EOC)
                    {
                        unchecked { chars += (int)op; }
                        reader.SkipCurrent();
                    }
                    // reader = new PageContentScanner(doc.Context, page);
                    // while ((op = reader.Peek()) != PdfOperatorType.EOC)
                    // {
                    //     unchecked { chars += (int)op; }
                    //     reader.SkipCurrent();
                    // }
                }
            }
            unchecked { return chars + total; }
        }

        [Benchmark()]
        public int V2()
        {
            int total = 0;
            int chars = 0;
            foreach (var pdf in pdfs)
            {
                using var doc = PdfDocument.Open(pdf);
                foreach (var page in doc.Pages)
                {
                    using var cache = new StreamBufferCache();
                    var reader = new PageContentScanner2(doc.Context, page);
                    while (reader.Advance())
                    {
                        unchecked { chars += (int)reader.CurrentOperator; }
                    }
                    // reader = new PageContentScanner2(doc.Context, page);
                    // while (reader.Advance())
                    // {
                    //     unchecked { chars += (int)reader.CurrentOperator; }
                    // }
                }
            }
            unchecked { return chars + total; }
        }
    }
}
