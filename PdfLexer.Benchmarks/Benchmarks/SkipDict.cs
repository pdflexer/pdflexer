﻿using BenchmarkDotNet.Attributes;
using PdfLexer.Legacy;
using PdfLexer.Parsers;
using PdfLexer.Parsers.Nested;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer.Benchmarks.Benchmarks
{

    /// <summary>
    /// Looked at using string to look up common number values but the utf8 parser is faster than
    /// just getting ascii.
    /// </summary>
    [Config(typeof(BenchmarkConfig))]
    public class SkipDict
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
        private DictionaryParser parser = new DictionaryParser(new ParsingContext());

        private char[] chars = new char[10];
        public SkipDict()
        {
            foreach (var item in data)
            {
                samples.Add(Encoding.ASCII.GetBytes(item));
            }
        }
        private NestedSkipper skipper = new NestedSkipper();
        [Benchmark(Baseline = true)]
        public int SeqSkip()
        {
            
            var count = 0;
            foreach (var item in samples)
            {
                var seq = new ReadOnlySequence<byte>(item);
                var reader = new SequenceReader<byte>(seq);
                reader.Advance(2); // <<
                skipper.TryScanToEndOfDict(ref reader);
                count += (int)reader.Consumed;
            }
            return count;
        }

        [Benchmark()]
        public int SpanSkip()
        {

            var count = 0;
            foreach (var item in samples)
            {
                ReadOnlySpan<byte> span = item;
                int i = 0;
                NestedUtil.AdvanceToDictEnd(span, ref i, out bool _);
                count +=  i;
            }
            return count;
        }
    }
}
