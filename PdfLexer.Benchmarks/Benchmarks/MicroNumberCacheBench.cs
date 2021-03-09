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
        private List<byte[]> samples = new List<byte[]>();

        public MicroCacheBench()
        {
            var rnd = new Random();
            for (var i = 0; i<20_000;i++)
            {
                samples.Add(Encoding.ASCII.GetBytes(rnd.Next(0, 9999999).ToString()));
            }
        }
        private ParsingContext Cacher = new ParsingContext(new ParsingOptions { CacheNumbers = true });
        private ParsingContext NoCacher = new ParsingContext(new ParsingOptions { CacheNumbers = false });


        [Benchmark(Baseline = true)]
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

        [Benchmark()]
        public int Cached()
        {
            var count = 0;
            foreach (var item in samples)
            {
                count = unchecked ( count + Cacher.NumberParser.Parse(item));
            }
            return count;
        }

        [Benchmark()]
        public int NoCached()
        {
            var count = 0;
            foreach (var item in samples)
            {
                count = unchecked ( count + NoCacher.NumberParser.Parse(item));
            }
            return count;
        }
    }
}
