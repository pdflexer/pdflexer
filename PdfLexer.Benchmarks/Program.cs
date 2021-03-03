﻿using BenchmarkDotNet.Running;
using PdfLexer.Benchmarks.Benchmarks;
using PdfLexer.Serializers;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace PdfLexer.Benchmarks
{
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
            //for (var i = 0; i < 1000; i++)
            //{
            //    ms.Position = 0;
            //    var ctx = new WritingContext(ms);
            //    ctx.Initialize(1.7m);
            //    // doc.Catalog["/ModifiedMDP"] = PdfBoolean.True;
            //    // var ir = ctx.WriteIndirectObject(PdfIndirectRef.Create(doc.Catalog));
            //    // doc.Trailer["/Root"] = ir;
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
