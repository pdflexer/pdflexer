using BenchmarkDotNet.Attributes;
using PdfLexer.IO;
using PdfLexer.Parsers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfLexer.Benchmarks.Benchmarks
{
    [Config(typeof(BenchmarkConfig))]
    public class FuncBenchmark
    {
        private byte[] data;
        public FuncBenchmark()
        {
            data = File.ReadAllBytes("C:\\temp\\PRIV\\Origrk.pdf");
        }
        [Benchmark(Baseline = true)]
        public int EagerEager()
        {
            using var doc = PdfDocument.Open(data).GetAwaiter().GetResult();
            doc.Context.IsEager = true;
            doc.Context.ShouldLoadIndirects = true;
            return Run(doc);
        }

        [Benchmark()]
        public int LazyLazy()
        {
            using var doc = PdfDocument.Open(data).GetAwaiter().GetResult();
            doc.Context.IsEager = false;
            doc.Context.ShouldLoadIndirects = false;
            return Run(doc);
        }

        private int Run(PdfDocument doc)
        {
            var root = doc.Trailer.GetRequiredValue<PdfDictionary>(PdfName.Root);
            var pages = root.GetRequiredValue<PdfDictionary>(PdfName.Pages);
            return EnumeratePages(pages).Count();
            
            IEnumerable<PdfDictionary> EnumeratePages(PdfDictionary dict)
            {
                var type = dict.GetRequiredValue<PdfName>(PdfName.TypeName);
                switch (type.Value)
                {
                    case "/Pages":
                        var kids = dict.GetRequiredValue<PdfArray>(PdfName.Kids);
                        foreach (var child in kids)
                        {
                            foreach (var pg in EnumeratePages(child.GetValue<PdfDictionary>())) 
                            {
                                yield return pg;
                            }
                        }
                        break;
                    case "/Page":
                        yield return dict;
                        break;
                }
            }
        }
    }
}
