using BenchmarkDotNet.Attributes;
using PdfLexer.IO;
using PdfLexer.Parsers;
using PdfLexer.Serializers;
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
        //[Benchmark(Baseline = true)]
        public int Eager()
        {
            using var doc = PdfDocument.Open(data, new ParsingOptions { Eagerness = Eagerness.FullEager });
            return Run(doc);
        }

        //[Benchmark()]
        public int EagerNoCache()
        {
            using var doc = PdfDocument.Open(data, new ParsingOptions { Eagerness = Eagerness.FullEager, CacheNames = false });
            return Run(doc);
        }

        //[Benchmark()]
        public int EagerSmall()
        {
            using var doc = PdfDocument.Open(data2, new ParsingOptions { Eagerness = Eagerness.FullEager });
            return Run(doc);
        }

        //[Benchmark(Baseline = true)]
        public int Lazy()
        {
            using var doc = PdfDocument.Open(data, new ParsingOptions { Eagerness = Eagerness.Lazy });
            return Run(doc);
        }

        //[Benchmark()]
        public int LazyNoCacheNames()
        {
            using var doc = PdfDocument.Open(data, new ParsingOptions { Eagerness = Eagerness.Lazy, CacheNames = false });
            return Run(doc);
        }


        //[Benchmark()]
        public int LazySmall()
        {
            using var doc = PdfDocument.Open(data2, new ParsingOptions { Eagerness = Eagerness.Lazy });
            return Run(doc);
        }


        //[Benchmark()]
        public int NoLoad()
        {
            using var doc = PdfDocument.Open(data, new ParsingOptions { LoadPageTree = false, Eagerness = Eagerness.Lazy });
            return 0;
        }

        //[Benchmark()]
        public int PdfPig()
        {
            var doc = UglyToad.PdfPig.PdfDocument.Open(data);
            return WalkTree(doc.Structure.Catalog.PageTree, null).Count();
        }

        private MemoryStream ms = new MemoryStream();
        [Benchmark(Baseline = true)]
        public int PdfPigCopy()
        {
            var doc = UglyToad.PdfPig.PdfDocument.Open(data2);
            ms.Position = 0;
            var builder = new UglyToad.PdfPig.Writer.PdfDocumentBuilder(ms);
            for (var i = 0; i < doc.NumberOfPages; i++)
            {
                builder.AddPage(doc, i + 1);
            }
            builder.Dispose();
            return 0;
        }

        //[Benchmark()]
        public int EagerCopy()
        {
            using var doc = PdfDocument.Open(data, new ParsingOptions { Eagerness = Eagerness.FullEager });
            ms.Position = 0;
            var writer = new WritingContext(ms);
            writer.Complete(doc.Trailer);
            return 0;
        }

        [Benchmark()]
        public int LazyCopy()
        {
            using var doc = PdfDocument.Open(data2, new ParsingOptions { LoadPageTree = false, Eagerness = Eagerness.Lazy });
            ms.Position = 0;
            var writer = new WritingContext(ms);
            writer.Complete(doc.Trailer);
            return 0;
        }
        [Benchmark()]
        public int LazyModify()
        {
            using var doc = PdfDocument.Open(data2, new ParsingOptions { LoadPageTree = true, Eagerness = Eagerness.Lazy });
            ms.Position = 0;
            doc.Pages[0].NativeObject[PdfName.Colors] = PdfCommonNumbers.Zero; // add dummy data to page
            doc.SaveTo(ms);
            return 0;
        }

        [Benchmark()]
        public int LazyModifyNoPages()
        {
            using var doc = PdfDocument.Open(data2, new ParsingOptions { LoadPageTree = false, Eagerness = Eagerness.Lazy });
            ms.Position = 0;
            doc.Trailer[PdfName.Colors] = PdfCommonNumbers.Zero; // add dummy data to page
            doc.SaveTo(ms);
            return 0;
        }

        //[Benchmark()]
        public int LazyCopyPageTree()
        {
            using var doc = PdfDocument.Open(data, new ParsingOptions { LoadPageTree = true, Eagerness = Eagerness.Lazy });
            ms.Position = 0;
            var writer = new WritingContext(ms);
            writer.Complete(doc.Trailer);
            return 0;
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
        internal static IEnumerable<(DictionaryToken, IReadOnlyList<DictionaryToken>)> WalkTree(PageTreeNode node, List<DictionaryToken> parents = null)
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
