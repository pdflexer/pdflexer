using BenchmarkDotNet.Attributes;
using PDFiumCore;
using PdfLexer.Content;
using PdfLexer.Images;
using PdfLexer.Serializers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PdfLexer.Benchmarks.Benchmarks
{
    [Config(typeof(BenchmarkConfig))]
    public class ImageBenchmark
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

        private Dictionary<string, string> pdfsNames = new Dictionary<string, string>()
        {
            ["1bit_decode_gray"] = "2HgjgtHf1xlffjXZOOBi0g",
            ["1bit_gray"] = "5yg9-sgECmJR89OKC2QLeA",
            ["4bit_indexed_gray"] = "ytNxY-dzrqww-EFk6orraA",
            ["4bit_indexed_rgb"] = "4xll9c7JnKbQTu2ri5NxJw",
            ["ccitt_1"] = "2eHFKUx4drRxfDdv1P_d7g",
            ["8bit_rgb"] = "41hB3b8_0gw2tJfzaFLXWg",
            ["rbg_decode"] = "41hB3b8_0gw2tJfzaFLXWg"
        };


        [Params("1bit_decode_gray", "ccitt_1", "4bit_indexed_rgb", "rbg_decode")]
        public string testPdf;

        [GlobalSetup]
        public void Setup()
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            var src = GetPathFromSegmentOfCurrent("PdfLexer.Benchmarks");
            var root = Path.GetFullPath(Path.Combine(src, ".."));
            var pdfRoot = Path.Combine(root, "test", "imgs");
            pdfs = new List<byte[]>();
            Add(testPdf);
            fpdfview.FPDF_InitLibrary();
            PdfLexer();
            // PdfPig();
            PdfiumCore();
            void Add(string name)
            {
                var path = Path.Combine(pdfRoot, pdfsNames[name] + ".pdf");
                var data = File.ReadAllBytes(path);
                pdfs.Add(data);
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
                    var reader = new ImageScanner(doc.Context, page);
                    while (reader.Advance())
                    {
                        if (reader.TryGetImage(out var img))
                        {
                            var imageSharp = img.GetImage(doc.Context);
                            unchecked { chars += imageSharp.Width; }
                        }
                        else
                        {
                            throw new ApplicationException("oops");
                        }
                    }
                }
                unchecked { total += chars; }
            }
            return total;
        }

        // [Benchmark()] -> not supported
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
                    foreach (var img in pg.GetImages())
                    {

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
                fixed (byte* ap = pdf)
                {
                    IntPtr ptr = (IntPtr)ap;
                    var doc = fpdfview.FPDF_LoadMemDocument(ptr, pdf.Length, null);
                    var pgs = fpdfview.FPDF_GetPageCount(doc);
                    for (var p = 0; p < pgs; p++)
                    {
                        var pg = fpdfview.FPDF_LoadPage(doc, p);
                        var t = fpdf_edit.FPDFPageCountObjects(pg);
                        if (t == -1)
                        {
                            throw new ApplicationException("failed to get pdf object count.");
                        }


                        for (var i = 0; i < t; i++)
                        {
                            var obj = fpdf_edit.FPDFPageGetObject(pg, i);
                            var type = fpdf_edit.FPDFPageObjGetType(obj);
                            if (type == (int)PdfiumObjType.FPDF_PAGEOBJ_FORM)
                            {
                                Recurse(obj);
                            }
                            else if (type == (int)PdfiumObjType.FPDF_PAGEOBJ_IMAGE)
                            {
                                HandleSingleImage(obj);
                            }
                        }

                        void Recurse(FpdfPageobjectT formObj)
                        {
                            var c = fpdf_edit.FPDFFormObjCountObjects(formObj);
                            for (var i = 0; i < c; i++)
                            {
                                var obj = fpdf_edit.FPDFFormObjGetObject(formObj, (ushort)i);
                                var type = fpdf_edit.FPDFPageObjGetType(obj);
                                if (type == (int)PdfiumObjType.FPDF_PAGEOBJ_FORM)
                                {
                                    Recurse(obj);
                                }
                                else if (type == (int)PdfiumObjType.FPDF_PAGEOBJ_IMAGE)
                                {
                                    HandleSingleImage(obj);
                                }
                            }
                        }

                        void HandleSingleImage(FpdfPageobjectT obj)
                        {
                            float l = 0, b = 0, r = 0, t = 0;
                            var s = fpdf_edit.FPDFPageObjGetBounds(obj, ref l, ref b, ref r, ref t);
                            if (s == 0)
                            {
                                throw new ApplicationException("Failed to read image");
                            }

                            // var bmp = fpdf_edit.FPDFImageObjGetRenderedBitmap(doc, Instance, obj);
                            var bmp = fpdf_edit.FPDFImageObjGetBitmap(obj);
                            var w = fpdfview.FPDFBitmapGetWidth(bmp);
                            var h = fpdfview.FPDFBitmapGetHeight(bmp);
                            var str = fpdfview.FPDFBitmapGetStride(bmp);

                            var fmt = fpdfview.FPDFBitmapGetFormat(bmp);
                            var ptr = fpdfview.FPDFBitmapGetBuffer(bmp);
                            switch (fmt)
                            {
                                case 1:
                                case 2:
                                case 3:
                                    {
                                        using var img = CreateManagedImageByFmt(ptr, w, h, str, fmt);
                                        unchecked { total += img.Width; }
                                    }
                                    break;
                                case 4:
                                    {
                                        using var img = CreateManagedImage(ptr, w, h);
                                        unchecked { total += img.Width; }
                                    }
                                    break;
                                default:
                                    throw new ApplicationException("err");
                            }
                            fpdfview.FPDFBitmapDestroy(bmp);
                            return;
                        }

                        fpdfview.FPDF_ClosePage(pg);
                    }

                    fpdfview.FPDF_CloseDocument(doc);

                }
                unchecked { total += chars; }
            }
            return total;
        }

        private static unsafe Image<Bgra32> CreateManagedImageByFmt(IntPtr ptr, int width, int height, int stride, int fmt = 1)
        {
            var bpp = fmt switch
            {
                1 => 1,
                2 => 3,
                3 => 4,
                4 => 4,
                _ => throw new NotSupportedException($"bmp Fmt {fmt} not supported")
            };
            var span = new Span<byte>(ptr.ToPointer(), stride * height * bpp);
            var img = new Image<Bgra32>(width, height);
            var black = new Bgra32
            {
                B = 0,
                G = 0,
                R = 0,
                A = 255
            };
            var white = new Bgra32
            {
                B = 255,
                G = 255,
                R = 255,
                A = 255
            };

            for (var i = 0; i < height; i++)
            {
                var ls = stride * bpp * i;
                for (var w = 0; w < width; w++)
                {
                    var first = ls + w * bpp;
                    switch (fmt)
                    {
                        case 1:
                            var bt = span[first];
                            img[w, i] = bt < 127 ? black : white;
                            break;
                        case 2:
                        case 3:
                            {
                                var b1 = span[first];
                                var b2 = span[first + 1];
                                var b3 = span[first + 2];
                                img[w, i] = new Bgra32
                                {
                                    B = b1,
                                    G = b2,
                                    R = b3,
                                    A = 255
                                };
                                break;
                            }
                        case 4:
                            {
                                var b1 = span[first];
                                var b2 = span[first + 1];
                                var b3 = span[first + 2];
                                var b4 = span[first + 3];
                                img[w, i] = new Bgra32
                                {
                                    B = b1,
                                    G = b2,
                                    R = b3,
                                    A = b4
                                };
                                break;
                            }
                    }

                }
            }
            return img;
        }
        private static unsafe Image<Bgra32> CreateManagedImage(IntPtr ptr, int width, int height)
        {
            var image = Image.WrapMemory<Bgra32>(
                                ptr.ToPointer(),
                                width,
                                height);
            return image;
        }

        private enum PdfiumObjType
        {
            FPDF_PAGEOBJ_UNKNOWN = 0,
            FPDF_PAGEOBJ_TEXT = 1,
            FPDF_PAGEOBJ_PATH = 2,
            FPDF_PAGEOBJ_IMAGE = 3,
            FPDF_PAGEOBJ_SHADING = 4,
            FPDF_PAGEOBJ_FORM = 5
        }
    }
}
