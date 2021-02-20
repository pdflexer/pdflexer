using BenchmarkDotNet.Running;
using PdfLexer.Benchmarks.Benchmarks;
using System;
using System.Reflection;

namespace PdfLexer.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            //var bench = new DictionaryBenchmark();
            //for (var i=0;i<100000;i++)
            //{
            //    bench.PdfLexerSpanNoCache();
            //    // bench.PdfLexerSequenceNoCache();
            //}
            BenchmarkSwitcher.FromAssembly(Assembly.GetEntryAssembly()).Run(args);
        }
    }
}
