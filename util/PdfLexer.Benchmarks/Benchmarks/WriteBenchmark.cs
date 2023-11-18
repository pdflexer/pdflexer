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
            using var ctx = new ParsingContext(new ParsingOptions { Eagerness = Eagerness.FullEager });
            using var doc = PdfDocument.Open(data);
            ms.Position = 0;
            var writer = new WritingContext(ms);
            writer.Complete(doc.Trailer);
            return 0;
        }

        [Benchmark()]
        public int LazyCopy()
        {
            using var ctx = new ParsingContext(new ParsingOptions { LoadPageTree = false, Eagerness = Eagerness.Lazy });
            using var doc = PdfDocument.Open(data2);
            ms.Position = 0;
            var writer = new WritingContext(ms);
            writer.Complete(doc.Trailer);
            return 0;
        }
        [Benchmark()]
        public int LazyModify()
        {
            using var ctx = new ParsingContext(new ParsingOptions { LoadPageTree = true, Eagerness = Eagerness.Lazy });
            using var doc = PdfDocument.Open(data2);
            ms.Position = 0;
            doc.Pages[0].NativeObject[PdfName.Colors] = PdfCommonNumbers.Zero; // add dummy data to page
            doc.SaveTo(ms);
            return 0;
        }

        [Benchmark()]
        public int LazyModifyNoPages()
        {

            using var ctx = new ParsingContext(new ParsingOptions { LoadPageTree = false, Eagerness = Eagerness.Lazy });
            using var doc = PdfDocument.Open(data2);
            ms.Position = 0;
            doc.Trailer[PdfName.Colors] = PdfCommonNumbers.Zero; // add dummy data to page
            doc.SaveTo(ms);
            return 0;
        }

        //[Benchmark()]
        public int LazyCopyPageTree()
        {

            using var ctx = new ParsingContext(new ParsingOptions { LoadPageTree = true, Eagerness = Eagerness.Lazy });
            using var doc = PdfDocument.Open(data);
            ms.Position = 0;
            var writer = new WritingContext(ms);
            writer.Complete(doc.Trailer);
            return 0;
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
