using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using PdfLexer.Lexing;
using PdfLexer.Parsers;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Text;

namespace PdfLexer.Benchmarks.Benchmarks
{
    
    
    [Config(typeof(BenchmarkConfig))]
    public class StringBenchmark
    {
        public static List<string> data = new List<string>
        {
            @"(D:20210111115333-05'00')",
            @"(RUBIKA::PDF\(F\)\nPX\(D\)\n)",
            @"(iTextSharp 4.1.6 by 1T3XT)",
            "(Strings may contain newlines\r\nand such.)",
            "(Strings may contain balanced parentheses () and special characters (*!*&}^% and so on).)",
            "(this string (contains nested (two levels)) parentheses)",
            "(this string <contains>)"
        };
        private List<byte[]> samples = new List<byte[]>();
        private List<MemoryStream> mss = new List<MemoryStream>();
        private MemoryStream allMs;
        public StringBenchmark()
        {
            foreach (var item in data)
            {
                samples.Add(Encoding.ASCII.GetBytes(item));
            }
            foreach (var item in samples)
            {
                mss.Add(new MemoryStream(item));
            }
            var all = string.Join("", data);
            allMs = new MemoryStream(Encoding.ASCII.GetBytes(all));
        }
        private StringParser parser = new StringParser(new ParsingContext());
        private List<PdfString> results = new List<PdfString>(10);
        private byte[] test = new byte[200];

        private static byte[] stringLiteralTerms = new byte[]
        {
            (byte) '(', (byte) ')', (byte) '\\'
        };

        [Benchmark(Baseline = true)]
        public List<PdfString> SpanString()
        {
            results.Clear();
            foreach (var item in samples)
            {
                results.Add(parser.Parse(item));
            }
            return results;
        }

        [Benchmark()]
        public List<PdfString> JustUtf()
        {
            results.Clear();
            foreach (var item in samples)
            {
                results.Add(new PdfString(Encoding.UTF8.GetString(item), PdfStringType.Literal, PdfTextEncodingType.Calculate));
            }
            return results;
        }

        //[Benchmark()]
        public List<PdfString> Sequence()
        {
            results.Clear();
            foreach (var item in samples)
            {
                var seq = new ReadOnlySequence<byte>(item);
                results.Add(parser.Parse(seq));
            }
            return results;
        }

        [Benchmark()]
        public int ScanOverData()
        {
            var count = 0;
            foreach (var item in samples)
            {
                for (var i=0;i<item.Length;i++)
                {
                    var b = item[i];
                    if (b == (byte) '(' || b == (byte) ')' || b == (byte) '\\')
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        [Benchmark()]
        public int SkipOverData()
        {
            var count = 0;
            foreach (var item in samples)
            {
                Span<byte> data = item;
                var pos = 0;
                while ((pos = data.IndexOfAny(stringLiteralTerms)) > -1)
                {
                    count++;
                    data = data.Slice(pos+1);
                }
            }
            return count;
        }

        // [Benchmark()]
        public List<PdfString> CopyTo()
        {
            results.Clear();
            foreach (var item in samples)
            {
                Span<byte> from = item;
                Span<byte> to = test;
                from.CopyTo(to);
            }
            return results;
        }

        // [Benchmark()]
        public List<PdfString> LoopCopy()
        {
            results.Clear();
            foreach (var item in samples)
            {
                Span<byte> from = item;
                Span<byte> to = test;
                for (var i=0;i<from.Length;i++)
                { 
                    to[i] = from[i];
                }
            }
            return results;
        }
        // LEGACY
        // [Benchmark()]
        // public List<PdfString> PipeReaderMs()
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

        // LEGACY
        // [Benchmark()]
        // public List<PdfString> PipeReaderMsBatch()
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

        // LEGACY
        // [Benchmark()]
        // public List<PdfString> PipeReaderMsSmallBuffer()
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
        // 
        // [Benchmark()]
        // public List<PdfString> PipeReaderMsSmallBufferBatch()
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
