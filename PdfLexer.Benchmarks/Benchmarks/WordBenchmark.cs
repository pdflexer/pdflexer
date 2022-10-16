using BenchmarkDotNet.Attributes;
using PDFiumCore;
using PdfLexer.Content;
using PdfLexer.Serializers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PdfLexer.Benchmarks.Benchmarks
{
    [Config(typeof(BenchmarkConfig))]
    public class WordBenchmark
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
            CMaps.AddKnownPdfCMaps();
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            var src = GetPathFromSegmentOfCurrent("PdfLexer.Benchmarks");
            var root = Path.GetFullPath(Path.Combine(src, ".."));
            var pdfRoot = Path.Combine(root, "test", "pdfs", "pdfjs");
            pdfs = new List<byte[]>();
            paths = new List<string>();
            mems = new List<MemoryStream>();
            Add(testPdf);
            fpdfview.FPDF_InitLibrary();
            PdfLexer();
            PdfPig();
            PdfiumCore();
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
        public int PdfLexer()
        {

            int total = 0;
            int chars = 0;
            foreach (var pdf in pdfs)
            {
                using var doc = PdfDocument.Open(pdf);
                foreach (var page in doc.Pages)
                {
                    var reader = new SimpleWordReader(doc.Context, page);
                    while (reader.Advance())
                    {
                        unchecked { chars += reader.CurrentWord.Length; }
                    }
                }
                unchecked { total += chars; }
            }
            return total;
        }

        [Benchmark()]
        public int PdfPig()
        {
            int total = 0;
            int chars = 0;
            foreach (var pdf in pdfs)
            {
                using var doc = UglyToad.PdfPig.PdfDocument.Open(pdf);
                for (var i = 0; i < doc.NumberOfPages; i++)
                {
                    var pg = doc.GetPage(i + 1);
                    foreach (var w in pg.GetWords())
                    {
                        unchecked { chars += w.Text.Length; }
                    }
                }
                unchecked { total += chars; }
            }
            return total;
        }

        [Benchmark()]
        public unsafe int PdfiumCore()
        {

            int total = 0;
            int chars = 0;
            foreach (var pdf in pdfs)
            {
                fixed (byte* p = pdf)
                {
                    IntPtr ptr = (IntPtr)p;
                    var doc = fpdfview.FPDF_LoadMemDocument(ptr, pdf.Length, null);
                    var pgs = fpdfview.FPDF_GetPageCount(doc);
                    for (var i=0;i<pgs;i++)
                    {
                        var pg = fpdfview.FPDF_LoadPage(doc, i);
                        var txt = fpdf_text.FPDFTextLoadPage(pg);
                        var cc = fpdf_text.FPDFTextCountChars(txt);

                        Span<byte> text = new byte[cc * 2 + 1];
                        fixed (byte* ptrr = &text[0])
                        {
                            fpdf_text.FPDFTextGetText(txt, 0, cc, ref *(ushort*)ptrr);
                        }
                        var words = Encoding.Unicode.GetString(text).Split(' ');
                        foreach (var w in words)
                        {
                            unchecked { chars += w.Length; }
                        }

                        fpdf_text.FPDFTextClosePage(txt);
                        fpdfview.FPDF_ClosePage(pg);
                    }

                    fpdfview.FPDF_CloseDocument(doc);

                }
                unchecked { total += chars; }
            }
            return total;
        }
    }
}
