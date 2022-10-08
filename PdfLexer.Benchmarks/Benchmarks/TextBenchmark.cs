using BenchmarkDotNet.Attributes;
using PdfLexer.Content;
using PdfLexer.Serializers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PdfLexer.Benchmarks.Benchmarks
{
    [Config(typeof(BenchmarkConfig))]
    public class TextBenchmark
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
        [Params("__bpl13210.pdf", "bug1669099", "issue1905", "__ecma262.pdf", "__gesamt.pdf", "__issue1133.pdf", "issue2128r")]
        public string testPdf;

        [GlobalSetup]
        public void Setup()
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            var src = GetPathFromSegmentOfCurrent("PdfLexer.Benchmarks");
            var root = Path.GetFullPath(Path.Combine(src, ".."));
            var pdfRoot = Path.Combine(root, "test", "pdfs", "pdfjs");
            pdfs = new List<byte[]>();
            paths = new List<string>();
            mems = new List<MemoryStream>();
            Add(testPdf);
            ReadTxtPdfLexer();
            ReadTxtPdfPig();
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
        public int ReadTxtPdfPig()
        {
            int total = 0;
            int chars = 0;
            foreach (var pdf in pdfs)
            {
                using var doc = UglyToad.PdfPig.PdfDocument.Open(pdf);
                for (var i = 0; i < doc.NumberOfPages; i++)
                {
                    var pg = doc.GetPage(i + 1);
                    foreach (var c in pg.Letters)
                    {
                        foreach (var p in c.Value) 
                        {
                            total++;
                            unchecked { chars += (int)p; }
                        }
                    }
                }
            }
            unchecked { return chars + total; }
        }



        [Benchmark()]
        public int ReadTxtPdfLexer()
        {

            int total = 0;
            int chars = 0;
            foreach (var pdf in pdfs)
            {
                using var doc = PdfDocument.Open(pdf);
                foreach (var page in doc.Pages)
                {
                    var reader = new TextScanner(doc.Context, page);
                    while (reader.Advance())
                    {
                        if (reader.Glyph.MultiChar != null)
                        {
                            foreach (var p in reader.Glyph.MultiChar)
                            {
                                total++;
                                unchecked { chars += (int)p; }
                            }
                        } else
                        {
                            total++;
                            unchecked { chars += (int)reader.Glyph.Char; }
                        }
                    }

                }
            }
            unchecked { return chars + total; }
        }


    }
}
