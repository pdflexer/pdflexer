using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using PdfLexer.Lexing;
using PdfLexer.Parsers;
using System;
using System.Buffers;
using System.Buffers.Text;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Text;

namespace PdfLexer.Benchmarks.Benchmarks
{
    
    /// <summary>
    /// Looked at using string to look up common number values but the utf8 parser is faster than
    /// just getting ascii.
    /// </summary>
    [Config(typeof(BenchmarkConfig))]
    public class MicroCacheBench
    {
        public static List<string> data = new List<string>
        {
            "1", "10", "2", "100", "20131", "121251288"
        };
        private List<byte[]> samples = new List<byte[]>();

        private char[] chars = new char[10];
        public MicroCacheBench()
        {
            foreach (var item in data)
            {
                samples.Add(Encoding.ASCII.GetBytes(item));
            }
        }

        [Benchmark(Baseline = true)]
        public int JustGetAscii()
        {
            var count = 0;
            foreach (var item in samples)
            {
                for (var i = 0; i < item.Length; i++)
                {
                    chars[i] = (char) item[i];
                }
                count += new String(chars, 0, item.Length).Length;
            }
            return count;
        }

        [Benchmark()]
        public int Utf8Parse()
        {
            var count = 0;
            foreach (var item in samples)
            {
                if (!Utf8Parser.TryParse(item, out int value, out _))
                {
                    throw new ApplicationException();
                }
                count += value;
            }
            return count;
        }
    }
}
