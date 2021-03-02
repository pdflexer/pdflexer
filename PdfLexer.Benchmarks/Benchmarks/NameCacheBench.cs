using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PdfLexer.Benchmarks
{
    public class NameCacheBench
    {
        private List<byte[]> data = new List<byte[]>();
        public NameCacheBench()
        {
            var names = new List<string>() { "/Test", "/FourFour", "/A", "/ABCD" };
            foreach (var name in names)
            {
                data.Add(Encoding.ASCII.GetBytes(name));
            }
        }
        [Benchmark(Baseline = true)]
        public ulong ByteLoop()
        {
            ulong total = 0;
            foreach (var item in data)
            {
                ulong result = 0;
                for(uint i=1;i<item.Length;i++)
                {
                    // result += Math.Pow(2,i)*item[i];
                }
                total = unchecked(total+result);
            }
            return total;
        }

        [Benchmark()]
        public ulong BitConvertered()
        {
            ulong total = 0;
            foreach (var item in data)
            {
                ulong result = 0;
                for(int i=1;i<item.Length;i++)
                {
                    result = result | ((ulong)item[i] << 8*(i-1));
                }
                total = unchecked(total+result);
            }
            return total;
        }

        //[Benchmark()]
        public ulong Casting()
        {
            ulong total = 0;
            Span<byte> buffer = stackalloc byte[8];
            foreach (var item in data)
            {
                Span<byte> it = item;
                if (it.Length > 9)
                {
                    it.CopyTo(buffer.Slice(1, 8));
                } else
                {
                    it.CopyTo(buffer.Slice(1));
                }
                
                var result = MemoryMarshal.Cast<byte, ulong>(buffer);
                total = unchecked(total+result[0]);
            }
            return total;
        }
    }
}
