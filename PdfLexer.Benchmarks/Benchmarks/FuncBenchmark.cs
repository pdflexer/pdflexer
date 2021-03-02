using BenchmarkDotNet.Attributes;
using PdfLexer.IO;
using PdfLexer.Parsers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Tokens;

namespace PdfLexer.Benchmarks.Benchmarks
{
    [Config(typeof(BenchmarkConfig))]
    public class FuncBenchmark
    {
        private byte[] data;
        private byte[] data2;
        public FuncBenchmark()
        {
            data = File.ReadAllBytes("C:\\temp\\PRIV\\Origrk.pdf");
            data2 = File.ReadAllBytes("C:\\temp\\test-pdfs\\kjped-53-661.pdf");
        }
        [Benchmark(Baseline = true)]
        public int Eager()
        {
            using var doc = PdfDocument.Open(data, new ParsingOptions { Eagerness = Eagerness.FullEager }).GetAwaiter().GetResult();
            return Run(doc);
        }

        [Benchmark()]
        public int EagerNoCache()
        {
            using var doc = PdfDocument.Open(data, new ParsingOptions { Eagerness = Eagerness.FullEager, CacheNames = false }).GetAwaiter().GetResult();
            return Run(doc);
        }

        //[Benchmark()]
        public int EagerSmall()
        {
            using var doc = PdfDocument.Open(data2, new ParsingOptions { Eagerness = Eagerness.FullEager }).GetAwaiter().GetResult();
            return Run(doc);
        }

        [Benchmark()]
        public int Lazy()
        {
            using var doc = PdfDocument.Open(data, new ParsingOptions { Eagerness = Eagerness.Lazy }).GetAwaiter().GetResult();
            return Run(doc);
        }

        [Benchmark()]
        public int LazyNoCacheNames()
        {
            using var doc = PdfDocument.Open(data, new ParsingOptions { Eagerness = Eagerness.Lazy, CacheNames = false }).GetAwaiter().GetResult();
            return Run(doc);
        }

        
        //[Benchmark()]
        public int LazySmall()
        {
            using var doc = PdfDocument.Open(data2, new ParsingOptions { Eagerness = Eagerness.Lazy }).GetAwaiter().GetResult();
            return Run(doc);
        }


        //[Benchmark()]
        public int NoLoad()
        {
            using var doc = PdfDocument.Open(data, new ParsingOptions { LoadPageTree = false, Eagerness = Eagerness.Lazy }).GetAwaiter().GetResult();
            return 0;
        }

        [Benchmark()]
        public int PdfPig()
        {
            var doc = UglyToad.PdfPig.PdfDocument.Open(data);
            return WalkTree(doc.Structure.Catalog.PageTree, null).Count();
        }
        
        //[Benchmark()]
        public int PdfPigNoLoad()
        {
            var doc = UglyToad.PdfPig.PdfDocument.Open(data);
            return 0;
        }

        //[Benchmark()]
        public int PdfPigSmall()
        {
            var doc = UglyToad.PdfPig.PdfDocument.Open(data2);
            return WalkTree(doc.Structure.Catalog.PageTree, null).Count();
        }
        internal static IEnumerable<(DictionaryToken, IReadOnlyList<DictionaryToken>)> WalkTree(PageTreeNode node, List<DictionaryToken> parents=null)
        {
            if (parents == null)
            {
                parents = new List<DictionaryToken>();
            }

            if (node.IsPage)
            {
                yield return (node.NodeDictionary, parents);
                yield break;
            }

            parents = parents.ToList();
            parents.Add(node.NodeDictionary);
            foreach (var child in node.Children)
            {
                foreach (var item in WalkTree(child, parents))
                {
                    yield return item;
                }
            }
        }
        private int Run(PdfDocument doc)
        {
            return doc.Pages.Count;
        }
    }
}
