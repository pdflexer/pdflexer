using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfLexer.Benchmarks.Benchmarks
{
    [Config(typeof(BenchmarkConfig))]
    public class CastingBench
    {
        private List<IPdfObject> objects = new List<IPdfObject>();

        public CastingBench()
        {
            objects.Add(new PdfArray());
            objects.Add(new PdfDictionary());
            objects.Add(PdfNull.Value);
            objects.Add(PdfBoolean.True);
            objects.Add(new PdfStream(null, null));
            objects.Add(new PdfName("Test"));
            objects.Add(new PdfIntNumber(1));
            objects.Add(new PdfString("Test", PdfStringType.Literal, PdfTextEncodingType.PdfDocument));
            objects.Add(PdfIndirectRef.Create(new PdfName("A")));
        }

        [Benchmark(Baseline = true)]
        public int Pattern()
        {
            int total = 0;
            foreach (var obj in objects)
            {
                switch (obj)
                {
                    case PdfArray array:
                        total += 1;
                        continue;
                    case PdfNull nll:
                        total += 2;
                        continue;
                    case PdfBoolean bl:
                        total += 3;
                        continue;
                    case PdfStream str:
                        total += 4;
                        continue;
                    case PdfDictionary dict:
                        total += 5;
                        continue;
                    case PdfName name:
                        total += 6;
                        continue;
                    case PdfNumber no:
                        total += 7;
                        continue;
                    case PdfString str:
                        total += 8;
                        continue;
                    case PdfIndirectRef ir:
                        total += 9;
                        continue;
                }
            }
            return total;
        }

        [Benchmark()]
        public int Enum()
        {
            int total = 0;
            foreach (var obj in objects)
            {
                switch (obj.Type)
                {
                    case PdfObjectType.ArrayObj:
                        var arr = (PdfArray)obj;
                        total += 1;
                        continue;
                    case PdfObjectType.NullObj:
                        var nll = (PdfNull)obj;
                        total += 2;
                        continue;
                    case PdfObjectType.BooleanObj:
                        var bll = (PdfBoolean)obj;
                        total += 3;
                        continue;
                    case PdfObjectType.StreamObj:
                        var stream = (PdfStream)obj;
                        total += 4;
                        continue;
                    case PdfObjectType.DictionaryObj:
                        var dict = (PdfDictionary)obj;
                        total += 5;
                        continue;
                    case PdfObjectType.NameObj:
                        var name = (PdfName)obj;
                        total += 6;
                        continue;
                    case PdfObjectType.NumericObj:
                        var num = (PdfNumber)obj;
                        total += 7;
                        continue;
                    case PdfObjectType.StringObj:
                        var str = (PdfString)obj;
                        total += 8;
                        continue;
                    case PdfObjectType.IndirectRefObj:
                        var ir = (PdfIndirectRef)obj;
                        total += 9;
                        continue;
                }
            }
            return total;
        }
    }
}
