using BenchmarkDotNet.Attributes;
using DotNext.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig.Graphics.Operations.SpecialGraphicsState;

namespace PdfLexer.Benchmarks.Benchmarks
{
    [Config(typeof(BenchmarkConfig))]
    public class TokenEndBench
    {
        public static List<string> data = new List<string>
        {
            "<</BaseFont/USPSBarCode1/FirstChar 1/Type/Font/Encoding/WinAnsiEncoding/LastChar 255/Widths [0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 195 0 0 195 0 195 0 0 0 0 0 0 0 0 0 0 0 0 0 195 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0]/FontDescriptor 16 0 R/Subtype/TrueType>>",
            "<</BaseFont/ArialMT/FirstChar 1/Type/Font/Encoding/WinAnsiEncoding/LastChar 255/Widths [0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 277 277 354 556 556 889 666 190 333 333 389 583 277 333 277 277 556 556 556 556 556 556 556 556 556 556 277 277 583 583 583 556 1015 666 666 722 722 666 610 777 722 277 500 666 556 833 722 777 666 777 722 666 610 722 666 943 666 666 610 277 277 277 469 556 333 556 556 500 556 556 277 556 556 222 222 500 222 833 556 556 556 556 333 500 277 556 500 722 500 500 500 333 259 333 583 0 556 0 222 556 333 1000 556 556 333 1000 666 333 1000 0 610 0 0 222 222 333 333 350 556 1000 333 1000 500 333 943 0 500 666 277 333 556 556 556 556 259 556 333 736 370 556 583 333 736 552 399 548 333 333 333 576 537 333 333 333 365 556 833 833 833 610 666 666 666 666 666 666 1000 722 666 666 666 666 277 277 277 277 722 722 777 777 777 777 777 583 777 722 722 722 722 666 666 610 556 556 556 556 556 556 889 500 556 556 556 556 277 277 277 277 556 556 556 556 556 556 556 548 610 556 556 556 556 500 556 500]/FontDescriptor 17 0 R/Subtype/TrueType>>",
            "<</BaseFont/CourierNewPS-BoldMT/FirstChar 1/Type/Font/Encoding/WinAnsiEncoding/LastChar 255/Widths [0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 0 600 0 600 600 600 600 600 600 600 600 600 600 600 0 600 0 0 600 600 600 600 600 600 600 600 600 600 600 600 0 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600]/FontDescriptor 18 0 R/Subtype/TrueType>>",
            "<</Key (Test \\\\(Test) Test )/Key2 (\\216\\217)/Key3 (Test \\\rNextLine Test \\\rNextLine Test \\\rNextLine Test \\\rNextLine Test \\\rNextLine Test \\\rNextLineTest \\\rNextLineTest \\\rNextLine)>>",
            "<</FormType 1/Subtype/Form/BBox[0 0 612 792]/Resources<</XObject<</Xf19189 19 0 R>>/ProcSet[/PDF/Text/ImageB/ImageC/ImageI]>>/Type/XObject/Filter/FlateDecode/Length 53/Matrix[1 0 0 1 0 0]>>",
            "<</Type/XObject/DecodeParms<</K -1/Columns 20/Rows 20>>/Subtype/Image/Width 20/ColorSpace/DeviceGray/Filter/CCITTFaxDecode/BitsPerComponent 1/Length 115/Height 20>>",
            "<</FormType 1/Subtype/Form/BBox[0 0 612 792]/Resources<</XObject<</Xf19186 20 0 R>>/ProcSet[/PDF/Text/ImageB/ImageC/ImageI]>>/Type/XObject/Filter/FlateDecode/Length 53/Matrix[1 0 0 1 0 0]>>"
        };
        private List<byte[]> samples = new List<byte[]>();

        public TokenEndBench()
        {

        }

        [GlobalSetup]
        public void Setup()
        {
            foreach (var item in data)
            {
                samples.Add(Encoding.ASCII.GetBytes(item + item + item)); // increase size
            }
        }

        [Benchmark(Baseline = true)]
        public void AllOrs()
        {
            foreach (var item in samples)
            {
                ReadOnlySpan<byte> data = item;
                int loc = 0;
                while (loc < data.Length)
                {
                    int start = loc;
                    CommonUtil.ScanTokenEnd(data, ref loc);
                    if (loc == start) { loc++; }
                }
            }
        }

        [Benchmark()]
        public void Index()
        {
            foreach (var item in samples)
            {
                ReadOnlySpan<byte> data = item;
                int loc = 0;
                while (loc < data.Length)
                {
                    int start = loc;
                    CommonUtil.ScanTokenEnd2(data, ref loc);
                    if (loc == start) { loc++; }
                }
            }
        }

        [Benchmark()]
        public void Avx()
        {
            foreach (var item in samples)
            {
                ReadOnlySpan<byte> data = item;
                int loc = 0;
                while (loc < data.Length)
                {
                    int start = loc;
                    ScanTokenEndAvx(data, ref loc);
                    if (loc == start) { loc++; }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal unsafe static void ScanTokenEndAvx(ReadOnlySpan<byte> bytes, ref int pos)
        {
            ReadOnlySpan<byte> local = bytes;

            if (Avx2.IsSupported)
            {
                fixed (byte* p = TokenTerms256)
                {
                    Vector256<byte> termarray = Avx2.LoadVector256(p);

                    for (; pos < local.Length; pos++)
                    {
                        var b = local[pos];
                        var searcharray = Vector256.Create(b);
                        var equals = Avx2.CompareEqual(termarray, searcharray);
                        if (Avx2.MoveMask(equals) != 0)
                        {
                            return;
                        }
                    }
                }
            } else
            {
                CommonUtil.ScanTokenEnd(local, ref pos);
            }

            // comments
            // string literals
            // hex strings
        }

        internal unsafe static void Tokenize(ReadOnlySpan<byte> bytes)
        {
            fixed (byte* p = bytes)
            {
                Vector256<byte> data = Avx2.LoadVector256(p);

                var cchar = Vector256.Create((byte)'%');
                var equals = Avx2.CompareEqual(cchar, data);
                var cmask = Avx2.MoveMask(equals);
                if (cmask != 0)
                {
                    // comments
                }

                var schar = Vector256.Create((byte)'(');
                var sequals = Avx2.CompareEqual(cchar, data);
                var smask = Avx2.MoveMask(equals);
                if (smask != 0)
                {
                    // string literals
                }

                var hchar = Vector256.Create((byte)'<');
                var hequals = Avx2.CompareEqual(cchar, data);
                var hmask = Avx2.MoveMask(equals);
                if (hmask != 0)
                {
                    // string literals
                }

            }
        }

        internal static byte[] TokenTerms256 = new byte[32] { 0x00, 0x09, 0x0A, 0x0C, 0x0D, 0x20,
        (byte)'(', (byte)')', (byte)'<', (byte)'>', (byte)'[', (byte)']', (byte)'{', (byte)'}', (byte)'/', (byte)'%',
         (byte)'+', (byte)'-', (byte)'.', 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 57, 57, 57 };
    }
}
