using BenchmarkDotNet.Attributes;
using PdfLexer.Parsers;
using PdfLexer.Parsers.Nested;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace PdfLexer.Benchmarks.Benchmarks
{

    /// <summary>
    /// Looked at using string to look up common number values but the utf8 parser is faster than
    /// just getting ascii.
    /// </summary>
    [Config(typeof(BenchmarkConfig))]
    public class AsyncLocalBench
    {

        private static AsyncLocal<object> al = new AsyncLocal<object>();
        private object local = new object();
        public AsyncLocalBench()
        {
            al.Value = new object();
        }
        // [Benchmark(Baseline = true)]
        // public int SeqSkip()
        // {
        //     
        //     var count = 0;
        //     foreach (var item in samples)
        //     {
        //         var seq = new ReadOnlySequence<byte>(item);
        //         var reader = new SequenceReader<byte>(seq);
        //         reader.Advance(2); // <<
        //         skipper.TryScanToEndOfDict(ref reader);
        //         count += (int)reader.Consumed;
        //     }
        //     return count;
        // }

        [Benchmark(Baseline = true)]
        public object Local() => local;

        [Benchmark()]
        public object AsyncLocal() => al.Value;
    }
}
