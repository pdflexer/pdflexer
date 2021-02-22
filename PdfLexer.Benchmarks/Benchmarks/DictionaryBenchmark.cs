using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using PdfLexer.Parsers;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Tokenization.Scanner;
using UglyToad.PdfPig.Tokens;

namespace PdfLexer.Benchmarks.Benchmarks
{
    
    [Config(typeof(BenchmarkConfig))]
    public class DictionaryBenchmark
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
        private List<MemoryStream> mss = new List<MemoryStream>();
        private MemoryStream allMs;
        public DictionaryBenchmark()
        {
            foreach (var item in data)
            {
                samples.Add(Encoding.ASCII.GetBytes(item));
            }
            foreach (var item in samples)
            {
                mss.Add(new MemoryStream(item));
            }
            foreach (var item in samples)
            {
                pigItems.Add(new ByteArrayInputBytes(item));
            }
            var all = string.Join("", data);
            allMs = new MemoryStream(Encoding.ASCII.GetBytes(all));
            
        }
        private DictionaryParser parser = new DictionaryParser(new ParsingContext());
        private DictionaryParser lazyParser = new DictionaryParser(new ParsingContext() { IsEager = false }); 
        private DictionaryParser noCache = new DictionaryParser(new ParsingContext() { CacheNumbers = false }); 
        private List<PdfDictionary> results = new List<PdfDictionary>(10);
        private List<DictionaryToken> pigResults = new List<DictionaryToken>(10);
        private List<ByteArrayInputBytes> pigItems = new List<ByteArrayInputBytes>();


        [Benchmark()]
        public List<PdfDictionary> PdfLexerSpan()
        {
            results.Clear();
            foreach (var item in samples)
            {
                results.Add(parser.Parse(item));
            }
            return results;
        }

        [Benchmark()]
        public List<PdfDictionary> PdfLexerSpanNoCache()
        {
            results.Clear();
            foreach (var item in samples)
            {
                results.Add(noCache.Parse(item));
            }
            return results;
        }

        [Benchmark()]
        public List<PdfDictionary> PdfLexerSpanLazy()
        {
            results.Clear();
            foreach (var item in samples)
            {
                results.Add(lazyParser.Parse(item));
            }
            return results;
        }

        // NO LONGER RELEVANT REALLY, not using a specific sequence parsers
        [Benchmark()]
        public List<PdfDictionary> PdfLexerSequence()
        {
            results.Clear();
            foreach (var item in samples)
            {
                var seq = new ReadOnlySequence<byte>(item);
                results.Add(parser.Parse(seq));
            }
            return results;
        }

        // NO LONGER RELEVANT REALLY, not using a specific sequence parsers
        // [Benchmark()]
        // public List<PdfDictionary> PdfLexerSequenceNoCache()
        // {
        //     results.Clear();
        //     foreach (var item in samples)
        //     {
        //         var seq = new ReadOnlySequence<byte>(item);
        //         results.Add(noCache.Parse(seq));
        //     }
        //     return results;
        // }
        // 
        // [Benchmark()]
        // public List<PdfDictionary> PdfLexerSequenceLazy()
        // {
        //     results.Clear();
        //     foreach (var item in samples)
        //     {
        //         var seq = new ReadOnlySequence<byte>(item);
        //         results.Add(lazyParser.Parse(seq));
        //     }
        //     return results;
        // }

        [Benchmark(Baseline = true)]
        public List<PdfDictionary> PdfPig()
        {
            pigResults.Clear();
            
            foreach (var item in pigItems)
            {
                item.Seek(0);
                var scanner = new CoreTokenScanner(item);
                if (!scanner.TryReadToken<DictionaryToken>(out var result))
                {
                    throw new Exception();
                }
                pigResults.Add(result);
            }
            return results;
        }

        

        // [Benchmark()]
        // public List<PdfDictionary> PipeReaderMs()
        // {
        //     results.Clear();
        //     foreach (var ms in mss)
        //     {
        //         ms.Position = 0;
        //         var reader = PipeReader.Create(ms);
        //         results.Add(parser.Parse(reader));
        //     }
        //     return results;
        // }
        // 
        // [Benchmark()]
        // public List<PdfDictionary> PipeReaderMsBatch()
        // {
        //     results.Clear();
        //     allMs.Position = 0;
        //     var reader = PipeReader.Create(allMs);
        //     for (var i = 0; i < 7; i++)
        //     {
        //         results.Add(parser.Parse(reader));
        //     }
        //     return results;
        // }

        // [Benchmark()]
        // public List<PdfDictionary> PipeReaderMsSmallBuffer()
        // {
        //     results.Clear();
        //     foreach (var ms in mss)
        //     {
        //         ms.Position = 0;
        //         var reader = PipeReader.Create(ms, new StreamPipeReaderOptions(bufferSize: 10, minimumReadSize: 5));
        //         results.Add(parser.Parse(reader));
        //     }
        //     return results;
        // }

        // [Benchmark()]
        // public List<PdfDictionary> PipeReaderMsSmallBufferBatch()
        // {
        //     results.Clear();
        //     allMs.Position = 0;
        //     var reader = PipeReader.Create(allMs, new StreamPipeReaderOptions(bufferSize: 10, minimumReadSize: 5));
        //     for (var i = 0; i < 7; i++)
        //     {
        //         results.Add(parser.Parse(reader));
        //     }
        //     return results;
        // }
    }
}
