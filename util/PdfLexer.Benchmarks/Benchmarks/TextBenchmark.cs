using BenchmarkDotNet.Attributes;
using PDFiumCore;
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
            ReadTxtPdfLexer();
            ReadTxtPdfPig();
            ReadTxtPdfiumCore();
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
                        }
                        else
                        {
                            total++;
                            unchecked { chars += (int)reader.Glyph.Char; }
                        }
                    }

                }
            }
            unchecked { return chars + total; }
        }

        [Benchmark()]
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
        public unsafe uint ReadTxtPdfiumCore()
        {

            uint total = 0;
            uint chars = 0;
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
                        for (var c=0;c<cc;c++)
                        {
                            var cv = fpdf_text.FPDFTextGetUnicode(txt, c);
                            unchecked { chars += cv; }
                        }

                        fpdf_text.FPDFTextClosePage(txt);
                        fpdfview.FPDF_ClosePage(pg);
                    }


                    fpdfview.FPDF_CloseDocument(doc);

                }
            }
            unchecked { return chars + total; }
        }


    }
}
