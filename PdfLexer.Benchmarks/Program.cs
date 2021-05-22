using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using PdfLexer.Benchmarks.Benchmarks;
using PdfLexer.Serializers;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace PdfLexer.Benchmarks
{
    internal class BenchmarkConfig : ManualConfig
    {
        public BenchmarkConfig()
        {
            AddDiagnoser(MemoryDiagnoser.Default);
            AddJob(Job.ShortRun.WithWarmupCount(5).WithIterationCount(25));
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            // var bench = new NameCacheBench();
            // bench.Casting();
            // var bench = new FuncBenchmark();
            //var data = File.ReadAllBytes("C:\\temp\\PRIV\\Origrk.pdf");
            //var doc = await PdfDocument.Open(data, new ParsingOptions { Eagerness = Eagerness.FullEager });
            //var ms = new MemoryStream();
            // var cs = new ContentStreamBenchmark();
            // var total = 0;
            // for (var i = 0; i < 100000; i++)
            // {
            //     total  += cs.PdfLexerSpan();
            // }
            // Console.WriteLine(total);
            //    ms.Position = 0;
            //    var ctx = new WritingContext(ms);
            //    ctx.Initialize(1.7m);
            // doc.Catalog["/ModifiedMDP"] = PdfBoolean.True;
            // var ir = ctx.WriteIndirectObject(PdfIndirectRef.Create(doc.Catalog));
            // doc.Trailer["/Root"] = ir;
            //    ctx.Complete(doc.Trailer);
            //}
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
