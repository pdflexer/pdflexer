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
    public class WriteBenchmark
    {
        private byte[] data;
        private byte[] data2;
        public WriteBenchmark()
        {
            data = File.ReadAllBytes("C:\\temp\\PRIV\\Origrk.pdf");
            data2 = File.ReadAllBytes("C:\\temp\\test-pdfs\\kjped-53-661.pdf");
        }


        private MemoryStream ms = new MemoryStream();
        [Benchmark(Baseline = true)]
        public int LazyCopy()
        {
            using var doc = PdfDocument.Open(data, new ParsingOptions { LoadPageTree = false, Eagerness = Eagerness.Lazy });
            ms.Position = 0;
            var writer = new WritingContext(ms);
            writer.Complete(doc.Trailer);
            return 0;
        }

        [Benchmark()]
        public int LazySave()
        {
            using var doc = PdfDocument.Open(data, new ParsingOptions { LoadPageTree = false, Eagerness = Eagerness.Lazy });
            ms.Position = 0;
            doc.SaveTo(ms);
            return 0;
        }
    }
}
