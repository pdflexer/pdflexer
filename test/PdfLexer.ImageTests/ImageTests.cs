using PdfLexer.Content;
using PdfLexer.Images;
using PdfLexer.Tests;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace PdfLexer.ImageTests
{
    public class ImageTests
    {
        [Fact]
        public void It_Reads_8_bit_RGB_Large_Decode()
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var output = Path.Combine(tp, "results", "images");
            var pdfRoot = Path.Combine(tp, "imgs");
            RunSingle(pdfRoot, "sLMvJuB9piYicr7z0DcRIA.pdf", output);
        }

        [InlineData("0TxRvxWo5wUThisVd6EjFw.pdf")]
        [InlineData("2eHFKUx4drRxfDdv1P_d7g.pdf")]
        [Theory]
        public void It_Reads_8_bit_CMYK_Large_Decode(string name)
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var output = Path.Combine(tp, "results", "images");
            var pdfRoot = Path.Combine(tp, "imgs");
            RunSingle(pdfRoot, name, output, 10);
        }

        [InlineData("7-NAxWA8lNIhw2CECy0Qqw.pdf")]
        [InlineData("2eHFKUx4drRxfDdv1P_d7g.pdf")]
        [InlineData("dBmjJkJAMWsWDTRkT63FvA.pdf")]
        [InlineData("dlucL7gmL0XAg6Llvz7oCg.pdf")]
        [InlineData("E54Ol5_mDrYcHKJdKowWdg.pdf")]
        [InlineData("JupJ4frDifcoWdcuKEV-XA.pdf")]
        [InlineData("LrUpVnZ0SQZWkawizVTIwQ.pdf")]
        [InlineData("MpEF16rVGGyIIkO-vX24tA.pdf")]
        [InlineData("PPytoMstm4mLsZdJ82hp3Q.pdf")]
        [Theory]
        public void It_Reads_CCITT(string name)
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var output = Path.Combine(tp, "results", "images");
            var pdfRoot = Path.Combine(tp, "imgs");
            RunSingle(pdfRoot, name, output, 10);
        }

        [InlineData("JckBq0XMQZdsM1fgO7XjPQ.pdf")]
        [Theory]
        public void It_Reads_ICC(string name)
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var output = Path.Combine(tp, "results", "images");
            var pdfRoot = Path.Combine(tp, "imgs");
            RunSingle(pdfRoot, name, output, 10);
        }

        [Fact]
        public void It_Reads_1bit_Gray()
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var output = Path.Combine(tp, "results", "images");
            var pdfRoot = Path.Combine(tp, "imgs");
            RunSingle(pdfRoot, "5yg9-sgECmJR89OKC2QLeA.pdf", output);
        }

        [Fact]
        public void It_Reads_Combined_DCT()
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var output = Path.Combine(tp, "results", "images");
            var pdfRoot = Path.Combine(tp, "imgs");
            RunSingle(pdfRoot, "7DOO0uoEZXgO28xJAm-9dg.pdf", output);
        }

        [Fact]
        public void It_Reads_Combined_JPX()
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var output = Path.Combine(tp, "results", "images");
            var pdfRoot = Path.Combine(tp, "imgs");
            RunSingle(pdfRoot, "QG2V70Q-V3rvHLve3RzY7Q.pdf", output);
        }

        [Fact]
        public void It_Reads_Combined_CCITT()
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var output = Path.Combine(tp, "results", "images");
            var pdfRoot = Path.Combine(tp, "imgs");
            RunSingle(pdfRoot, "J-qcQC3LeqpK0CAVsUum-A.pdf", output);
        }

        [Fact]
        public void It_Reads_CCITT_Separation()
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var output = Path.Combine(tp, "results", "images");
            var pdfRoot = Path.Combine(tp, "imgs");
            RunSingle(pdfRoot, "KUCSixn8jpaapxFfnOveXA.pdf", output);
        }

        [Fact]
        public void It_Reads_CCITT_CalGray()
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var output = Path.Combine(tp, "results", "images");
            var pdfRoot = Path.Combine(tp, "imgs");
            RunSingle(pdfRoot, "YCWncT6VMiYqBthAvHNTCQ.pdf", output);
        }

        [Fact]
        public void It_Reads_Run_Length()
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var output = Path.Combine(tp, "results", "images");
            var pdfRoot = Path.Combine(tp, "imgs");
            RunSingle(pdfRoot, "eM--8R8Ddfg2EKavFEOmZA.pdf", output);
        }

        [Fact]
        public void It_Reads_DCT_Separation()
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var output = Path.Combine(tp, "results", "images");
            var pdfRoot = Path.Combine(tp, "imgs");
            RunSingle(pdfRoot, "EG7LutipHf7UdCK4TsbvQw.pdf", output);
        }
        [Fact]
        public void It_Reads_DCT_ICCBased()
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var output = Path.Combine(tp, "results", "images");
            var pdfRoot = Path.Combine(tp, "imgs");
            RunSingle(pdfRoot, "fWECJ7bDXYEj8QCAy_ajow.pdf", output);
        }

        [Fact]
        public void It_Reads_Inversed_Decode_RGB()
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var output = Path.Combine(tp, "results", "images");
            var pdfRoot = Path.Combine(tp, "imgs");
            RunSingle(pdfRoot, "41hB3b8_0gw2tJfzaFLXWg.pdf", output);
        }

        [Fact]
        public void It_Reads_DCT_CMYK_Decode()
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var output = Path.Combine(tp, "results", "images");
            var pdfRoot = Path.Combine(tp, "imgs");
            RunSingle(pdfRoot, "99qbYO5CWI7LxqAU6biTCQ.pdf", output);
            RunSingle(pdfRoot, "e0YM9Ygv7cPbDO-p7oQ4jg.pdf", output);
        }

        [Fact]
        public void It_Reads_ICC_RGB()
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var output = Path.Combine(tp, "results", "images");
            var pdfRoot = Path.Combine(tp, "imgs");
            RunSingle(pdfRoot, "BeYmT2VdZQwaoXn8HNvpLw.pdf", output);
        }

        [Fact]
        public void It_Reads_DCT_CalGray()
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var output = Path.Combine(tp, "results", "images");
            var pdfRoot = Path.Combine(tp, "imgs");
            RunSingle(pdfRoot, "hnrKIt4hMfNRn4SAIovTew.pdf", output);
        }

        [Fact]
        public void It_Reads_DCT_DeviceGray()
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var output = Path.Combine(tp, "results", "images");
            var pdfRoot = Path.Combine(tp, "imgs");
            RunSingle(pdfRoot, "lI-Xx3Tk9KdMqny5kSF2jA.pdf", output);
        }

        [Fact]
        public void It_Reads_DCT_CalRGB()
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var output = Path.Combine(tp, "results", "images");
            var pdfRoot = Path.Combine(tp, "imgs");
            RunSingle(pdfRoot, "JO-X5drIBmTAMhiBr7Lwrw.pdf", output);
        }


        [InlineData("LcDy-GQ2oC0mJ4pDz18ixw.pdf")]
        // [InlineData("SsGZumJPC4cJHwDRzPXmHg.pdf")] // license
        [Theory]
        public void It_Reads_CalRGB(string name)
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var output = Path.Combine(tp, "results", "images");
            var pdfRoot = Path.Combine(tp, "imgs");
            RunSingle(pdfRoot, name, output, 10);
        }

        [Fact]
        public void It_Reads_Indexed_CalRGB()
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var output = Path.Combine(tp, "results", "images");
            var pdfRoot = Path.Combine(tp, "imgs");
            RunSingle(pdfRoot, "ho0zlLILRSnotM7m42LRCA.pdf", output);
        }

        [Fact]
        public void It_Reads_1bit_Decode_Gray()
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var output = Path.Combine(tp, "results", "images");
            var pdfRoot = Path.Combine(tp, "imgs");
            RunSingle(pdfRoot, "2HgjgtHf1xlffjXZOOBi0g.pdf", output);
        }

        // bad pdf -> Interpolate colorspace
        // [Fact]
        // public void It_Reads_2bit_RGB()
        // {
        //     var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
        //     var output = Path.Combine(tp, "results", "images");
        //     var pdfRoot = Path.Combine(tp, "imgs");
        //     RunSingle(pdfRoot, "DQzbG6N-CTyHisNgTKUaKg.pdf", output);
        // }

        [Fact]
        public void It_Reads_1bit_JBIG()
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var output = Path.Combine(tp, "results", "images");
            var pdfRoot = Path.Combine(tp, "imgs");
            RunSingle(pdfRoot, "O1JQ1dXyB7eeZMdaVhOkgA.pdf", output);
        }

        [InlineData("4xll9c7JnKbQTu2ri5NxJw.pdf")]
        [InlineData("wPecqvLHknVfAp9vftfBdw.pdf")]
        [Theory]
        public void It_Reads_4bit_Indexed_RGB(string name)
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var output = Path.Combine(tp, "results", "images");
            var pdfRoot = Path.Combine(tp, "imgs");
            RunSingle(pdfRoot, name, output);
        }

        [InlineData("0RwzPoHNznBgJ6lC0cpwVg.pdf")]
        [Theory]
        public void It_Reads_JPX_CMYK(string name)
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var output = Path.Combine(tp, "results", "images");
            var pdfRoot = Path.Combine(tp, "imgs");
            RunSingle(pdfRoot, name, output);
        }
        [InlineData("HtTkH99tB4z_clGIQkg7pA.pdf")]
        [Theory]
        public void It_Reads_JPX_Gray(string name)
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var output = Path.Combine(tp, "results", "images");
            var pdfRoot = Path.Combine(tp, "imgs");
            RunSingle(pdfRoot, name, output);
        }
        [InlineData("li4DXp5pMEL1XtIGiXG9dA.pdf")]
        [Theory]
        public void It_Reads_JPX_RGB(string name)
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var output = Path.Combine(tp, "results", "images");
            var pdfRoot = Path.Combine(tp, "imgs");
            RunSingle(pdfRoot, name, output);
        }

        [InlineData("C5c1Gr99aHf2LvAtdm7tnA.pdf")]
        [InlineData("cakcytzO66TdICf494zRJQ.pdf")]
        [InlineData("hHSPtyfXwkECzZP4ot5ahQ.pdf")]
        [Theory]
        public void It_Reads_SMask(string name)
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var output = Path.Combine(tp, "results", "images");
            var pdfRoot = Path.Combine(tp, "imgs");
            RunSingle(pdfRoot, name, output);
        }

        [InlineData("CGrD8UireJxTcYGKA1p5gA.pdf")]
        [InlineData("FPCORjum9-IgOO8CrsdSng.pdf")]
        [InlineData("o-KEr8zfK9AmEaH6ogf29A.pdf")]
        [Theory]
        public void It_Reads_Indexed_Lab(string name)
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var output = Path.Combine(tp, "results", "images");
            var pdfRoot = Path.Combine(tp, "imgs");
            RunSingle(pdfRoot, name, output);
        }

        [InlineData("ytNxY-dzrqww-EFk6orraA.pdf")]
        [Theory]
        public void It_Reads_4bit_Indexed_Gray(string name)
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var output = Path.Combine(tp, "results", "images");
            var pdfRoot = Path.Combine(tp, "imgs");
            RunSingle(pdfRoot, name, output);
        }

        [InlineData("qIWYAcek_Y90nUQ4533r1Q.pdf")]
        [Theory]
        public void It_Reads_16bit_CMYK(string name)
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var output = Path.Combine(tp, "results", "images");
            var pdfRoot = Path.Combine(tp, "imgs");
            RunSingle(pdfRoot, name, output, 20);
        }

        // [Fact] img mask, bad png
        public void It_Reads_1bit_Gray_Decode()
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var output = Path.Combine(tp, "results", "images");
            var pdfRoot = Path.Combine(tp, "imgs");
            RunSingle(pdfRoot, "wz0oUd-kdi-W33Q2ViilBQ.pdf", output);
        }

        [Fact]
        public void It_Reads_8bit_RGB_InvertedDecode()
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var output = Path.Combine(tp, "results", "images");
            var pdfRoot = Path.Combine(tp, "imgs");
            RunSingle(pdfRoot, "41hB3b8_0gw2tJfzaFLXWg.pdf", output);
        }

        [InlineData("wlknxeMey9yhDZqRcqkTpA.pdf")]
        [InlineData("_ghM41_9Ma-nPMMSr7Kbfw.pdf")]
        [InlineData("ZPWJPix1lLiQzx_oayWLAg.pdf")]
        [Theory]
        public void It_Reads_8bit_Indexed_CMYK(string pdf)
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var output = Path.Combine(tp, "results", "images");
            var pdfRoot = Path.Combine(tp, "imgs");
            RunSingle(pdfRoot, pdf, output, 10);
        }

        private void RunSingle(string root, string pdfName, string output, int threshhold = 5)
        {
            Directory.CreateDirectory(output);
            var pdf = Path.Combine(root, pdfName);
            var doc = PdfDocument.Open(File.ReadAllBytes(pdf));
            foreach (var page in doc.Pages)
            {
                var imgRdr = new ImageScanner(doc.Context, page);
                imgRdr.Advance();
                if (!imgRdr.TryGetImage(out var img))
                {
                    throw new ApplicationException("Read failure");
                }

                var name = Path.GetFileNameWithoutExtension(pdf);
                using var isa = img.GetImage(doc.Context);
                var imgout = Path.Combine(output, name + ".png");
                isa.SaveAsPng(imgout);
                using var cl = Image.Load<Bgra32>(imgout);
                using var bl = Image.Load<Bgra32>(Path.Combine(root, Path.GetFileNameWithoutExtension(pdf) + ".png"));
                if (!RunCompare(bl, cl, Path.Combine(output, name + "_diff.png"), threshhold))
                {
                    throw new ApplicationException("Mismatch");
                }
            }
        }

        // [Fact]
        public void It_Reads_Images()
        {
            var errors = new List<string>();
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var pdfRoot = Path.Combine(tp, "imgs");
            foreach (var pdf in Directory.GetFiles(pdfRoot, "*.pdf"))
            {
                var name = Path.GetFileNameWithoutExtension(pdf);
                var types = new List<string>();
                try
                {
                    try
                    {
                        var doc = PdfDocument.Open(File.ReadAllBytes(pdf));
                        if (doc.Trailer.ContainsKey(PdfName.Encrypt))
                        {
                            // don't support encryption currently
                            continue;
                        }


                        int i = 0;
                        foreach (var page in doc.Pages)
                        {

                            var imgRdr = new ImageScanner(doc.Context, page);
                            while (imgRdr.Advance())
                            {
                                var (x, y, w, h) = imgRdr.GetCurrentSize();
                                if (w < 5 || h < 5) { continue; }
                                if (!imgRdr.TryGetImage(out var img))
                                {
                                    continue;
                                }
                                try
                                {
                                    using var isa = img.GetImage(doc.Context);
                                    isa.SaveAsPng($"c:\\temp\\imgout\\{Path.GetFileNameWithoutExtension(pdf)}_{i}.png");
                                    using var cl = Image.Load<Bgra32>($"c:\\temp\\imgout\\{Path.GetFileNameWithoutExtension(pdf)}_{i}.png");
                                    using var bl = Image.Load<Bgra32>(Path.Combine(pdfRoot, Path.GetFileNameWithoutExtension(pdf) + ".png"));

                                    if (!RunCompare(bl, cl, $"c:\\temp\\imgout\\{Path.GetFileNameWithoutExtension(pdf)}_{i}_diff.png", 10))
                                    {
                                        errors.Add(pdf + ": diff mismatch.");
                                        File.Copy(Path.Combine(pdfRoot, Path.GetFileNameWithoutExtension(pdf) + ".txt"),
                                            $"c:\\temp\\imgout\\{Path.GetFileNameWithoutExtension(pdf)}.txt", true);
                                    }
                                    i++;
                                }
                                catch (Exception ex)
                                {
                                    // dont fail for now
                                    // throw;
                                }
                            }
                        }
                    }
                    catch (NotSupportedException ex)
                    {
                        // for compressed object streams
                        if (ex.Message.Contains("encryption"))
                        {
                            continue;
                        }
                    }
                }
                catch (Exception e)
                {
                    errors.Add(pdf + ": " + e.Message);
                }
            }
            if (errors.Any())
            {
                throw new ApplicationException(string.Join(Environment.NewLine, errors));
            }

        }

        private static bool RunCompare(Image<Bgra32> imgB, Image<Bgra32> imgC, string output, int threshhold)
        {
            var w1 = imgB.Width;
            var w2 = imgB.Width;
            var h1 = imgC.Height;
            var h2 = imgC.Height;
            var w = Math.Max(w1, w2);
            var h = Math.Max(h1, h2);
            var maskImage = new Image<Bgr24>(w, h);
            var samepix = new Bgr24
            {
                B = 0,
                G = 0,
                R = 0,
            };
            var diffPix = new Bgr24
            {
                B = 255,
                G = 255,
                R = 255,
            };

            bool exact = true;
            for (var x = 0; x < w; x++)
            {
                var bmatch = x < w1;
                var cmatch = x < w2;
                for (var y = 0; y < h; y++)
                {
                    var both = bmatch && cmatch && y < h1 && y < h2;
                    if (!both)
                    {
                        exact = false;
                        maskImage[x, y] = diffPix;
                        continue;
                    }
                    var a = imgB[x, y];
                    var b = imgC[x, y];
                    if ((a.A == 0 && b.A == 0))
                    {
                        maskImage[x, y] = samepix;
                        continue;
                    }


                    if (a.R == b.R && a.G == b.G && a.B == b.B) // && a.A == b.A)
                    {
                        maskImage[x, y] = samepix;
                        continue;
                    }

                    var d = (byte)((Math.Abs(a.R - b.R) + Math.Abs(a.B - b.B) + Math.Abs(a.G - b.G)) / 3.0);

                    if (d < threshhold)
                    {
                        maskImage[x, y] = samepix;
                        continue;
                    }

                    exact = false;
                    maskImage[x, y] = new Bgr24
                    {
                        B = d,
                        G = d,
                        R = d
                    };
                }
            }

            if (!exact)
            {
                maskImage.SaveAsPng(output);
                return false;
            }
            return true;
        }

    }
}
