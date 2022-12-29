using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using PdfLexer.Lexing;
using PdfLexer.Parsers;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics;
using System.Text;

namespace PdfLexer.Benchmarks.Benchmarks
{


    [Config(typeof(BenchmarkConfig))]
    public class SkipWhiteBench
    {
        internal static byte[] Terminators = new byte[16] { 0x00, 0x09, 0x0A, 0x0C, 0x0D, 0x20,
            (byte)'(', (byte)')', (byte)'<', (byte)'>', (byte)'[', (byte)']', (byte)'{', (byte)'}', (byte)'/', (byte)'%' };
        public static List<string> data = new List<string>
        {
            "     \r \n  \r\n       \r \t \t \n \n",
            "   \r \n  \r    \n       \r    \t \t \n    \n",
            "     \r \n    \r\n       \r \t \t \n       \n",
            " \r \n  \r\n     \r \t    \t \n \n",
            " <\r \n  \r\n     \r \t    \t \n \n",
            "<               ",
        };
        private List<byte[]> samples = new List<byte[]>();

        public static List<string> termData = new List<string>
        {
            "Hello/",
            "asdfasdf ",
            "asdfasdfasdf<",
            "HelloWorld ",
            "HillowWorkWorlWorlWOrl\r",
            "HillowWorkWorlWorlWOrlHillowWorkWorlWorlWOrl\r",
        };
        private List<byte[]> termSamples = new List<byte[]>();
        
        
        public SkipWhiteBench()
        {
            foreach (var item in data)
            {
                samples.Add(Encoding.ASCII.GetBytes(item));
            }
            foreach (var item in termData)
            {
                termSamples.Add(Encoding.ASCII.GetBytes(item));
            }
        }

        [Benchmark(Baseline = true)]
        public int Normal()
        {
            int count = 0;
            foreach (var item in samples)
            {
                var i = 0;
                CommonUtil.SkipWhiteSpace(item, ref i);
                count += i;
            }
            return count;
        }
               
        //[Benchmark()]
        public int Array()
        {
            int count = 0;
            foreach (var item in samples)
            {
                var i = 0;
                CommonUtil.SkipWhiteSpaceArray(item, ref i);
                count += i;
            }
            return count;
        }

        private static byte[] whitespace = new byte[6] { 0x00, 0x09, 0x0A, 0x0C, 0x0D, 0x20 };
        [Benchmark()]
        public int LocalArray()
        {
            Span<byte> white = whitespace;
            int count = 0;
            foreach (var item in samples)
            {
                var i = 0;
                for (; i < item.Length; i++)
                {
                    if (white.IndexOf(item[i]) > -1) {
                        continue;
                    }
                    break;
                }
                
                count += i;
            }
            return count;
        }

        //[Benchmark()]
        public int FindTerminatorArray()
        {
            Span<byte> terms = Terminators;
            int count = 0;
            foreach (var item in termSamples)
            {
                var i = 0;
                for (; i < item.Length; i++)
                {
                    if (terms.IndexOf(item[i]) > -1) {
                        break;
                    }
                }
                
                count += i;
            }
            return count;
        }

        //[Benchmark()]
        public int FindTerminatorAny()
        {
            Span<byte> terms = Terminators;
            int count = 0;
            foreach (var item in termSamples)
            {
                Span<byte> data = item;
                var r = data.IndexOfAny(terms);
                if (r == -1)
                {
                    r = data.Length;
                }
                count += r;
            }
            return count;
        }

        //[Benchmark()]
        public int FindTerminatorRaw()
        {
            int count = 0;
            foreach (var item in termSamples)
            {
                var i = 0;
                for (; i < item.Length; i++)
                {
                    var b = item[i];
                    if (b == 0x00 || b == 0x09 || b == 0x0A || b == 0x0C || b == 0x0D || b == 0x20
                            || b == (byte)'(' || b == (byte)')' || b == (byte)'<' || b == (byte)'>' || b == (byte)'[' || b == (byte)']'
                        || b == (byte)'{' || b == (byte)'}' || b == (byte)'/' || b == (byte)'%')
                    {
                        break;
                    }
                }
                count += i;
            }
            return count;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal unsafe static void ScanTokenEndAvx(ReadOnlySpan<byte> bytes, ref int pos)
        {
            ReadOnlySpan<byte> local = bytes;
            if (Avx2.IsSupported)
            {
                fixed (byte* p = TokenTerms256)
                {
                    Vector256<byte> termarray = Avx2.LoadVector256(p);

                    for (; pos < local.Length; pos++)
                    {
                        var b = local[pos];
                        var searcharray = Vector256.Create(b);
                        var equals = Avx2.CompareEqual(termarray, searcharray);
                        var result = Avx2.MoveMask(equals);
                        if (Avx2.MoveMask(equals) != 0)
                        {
                            return;
                        }
                    }
                }
            }
            else
            {
                CommonUtil.ScanTokenEnd(local, ref pos);
            }
        }

        internal static byte[] TokenTerms256 = new byte[32] { 0x00, 0x09, 0x0A, 0x0C, 0x0D, 0x20,
        (byte)'(', (byte)')', (byte)'<', (byte)'>', (byte)'[', (byte)']', (byte)'{', (byte)'}', (byte)'/', (byte)'%',
         (byte)'+', (byte)'-', (byte)'.', 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 57, 57, 57 };
    }
}
