using System;
using System.Buffers;
using System.Buffers.Text;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace PdfLexer.Parsers
{
    public enum NumberType
    {
        Integer,
        Real
    }
    public class NumberParser : IParser<PdfNumber>
    {
        private static readonly byte[] numberTerminators = new byte[17] { 0x00, 0x09, 0x0A, 0x0C, 0x0D, 0x20, (byte)'.',
            (byte)'(', (byte)')', (byte)'<', (byte)'>', (byte)'[', (byte)']', (byte)'{', (byte)'}', (byte)'/', (byte)'%' };
        private static readonly byte[] decimalTerminators = new byte[16] { 0x00, 0x09, 0x0A, 0x0C, 0x0D, 0x20,
            (byte)'(', (byte)')', (byte)'<', (byte)'>', (byte)'[', (byte)']', (byte)'{', (byte)'}', (byte)'/', (byte)'%' };
        private readonly ParsingContext _ctx;

        public NumberParser(ParsingContext ctx)
        {
            _ctx = ctx;
        }

        public PdfNumber Parse(ReadOnlySpan<byte> buffer)
        {
            if (buffer.Length == 1)
            {
                switch (buffer[0])
                {
                    case (byte)'0':
                        return PdfCommonNumbers.Zero;
                    case (byte)'1':
                        return PdfCommonNumbers.One;
                    case (byte)'2':
                        return PdfCommonNumbers.Two;
                    case (byte)'3':
                        return PdfCommonNumbers.Three;
                    case (byte)'4':
                        return PdfCommonNumbers.Four;
                    case (byte)'5':
                        return PdfCommonNumbers.Five;
                    case (byte)'6':
                        return PdfCommonNumbers.Six;
                    case (byte)'7':
                        return PdfCommonNumbers.Seven;
                    case (byte)'8':
                        return PdfCommonNumbers.Eight;
                    case (byte)'9':
                        return PdfCommonNumbers.Nine;
                }
            }

            if (buffer.Length == 2 && buffer[0] == (byte)'-' && buffer[1] == (byte)'1') {
                return PdfCommonNumbers.MinusOne;
            }

            if (!_ctx.CacheNumbers)
            {
                return GetResult(buffer);
            }

            if (buffer[0] == (byte) '+')
            {
                buffer = buffer.Slice(1);
            }

            if (buffer.Length > 9)
            {
                return new PdfLongNumber(GetLong(buffer));
            } else
            {
                var val = GetInt(buffer);
                if (val > 1000)
                {
                    return new PdfIntNumber(val);
                }
                if (_ctx.CachedInts.TryGetValue(val, out var intObj))
                {
                    return intObj;
                } else
                {
                    var obj = new PdfIntNumber(val);
                    _ctx.CachedInts[val] = obj;
                    return obj;
                }
            }
        }

        private PdfNumber GetResult(ReadOnlySpan<byte> buffer)
        {
            if (buffer.Length > 9)
            {
                return new PdfLongNumber(GetLong(buffer));
            } else
            {
                return new PdfIntNumber(GetInt(buffer));
            }
        }

        public PdfNumber Parse(ReadOnlySpan<byte> buffer, int start, int length)
        {
            return Parse(buffer.Slice(start, length));
        }

        public PdfNumber Parse(in ReadOnlySequence<byte> sequence)
        {
            if (sequence.IsSingleSegment)
            {
                return Parse(sequence.FirstSpan);
            }
            // TODO optimize
            var len = (int)sequence.Length;
            var buffer = ArrayPool<byte>.Shared.Rent(len);
            sequence.CopyTo(buffer);
            Span<byte> buff = buffer;
            var result = Parse(buff.Slice(0,len));
            ArrayPool<byte>.Shared.Return(buffer);
            return result;
        }

        public PdfNumber Parse(in ReadOnlySequence<byte> sequence, long start, int length)
        {
            return Parse(sequence.Slice(start, length));
        }

        private int GetInt(ReadOnlySpan<byte> buffer)
        {
            if (!Utf8Parser.TryParse(buffer, out int val, out int consumed))
            {
                throw new ApplicationException("Bad data for double number: " + Encoding.ASCII.GetString(buffer));
            }
            Debug.Assert(consumed == buffer.Length, "consumed == buffer.Length for int");

            return val;
        }

        private long GetLong(ReadOnlySpan<byte> buffer)
        {
            if (!Utf8Parser.TryParse(buffer, out long val, out int consumed))
            {
                throw new ApplicationException("Bad data for long number: " + Encoding.ASCII.GetString(buffer));
            }
            Debug.Assert(consumed == buffer.Length, "consumed == buffer.Length for long");

            return val;
        }

        private decimal GetDecimal(ReadOnlySpan<byte> buffer)
        {
            if (!Utf8Parser.TryParse(buffer, out decimal val, out int consumed))
            {
                throw new ApplicationException("Bad data for double number: " + Encoding.ASCII.GetString(buffer));
            }
            Debug.Assert(consumed == buffer.Length, "consumed == buffer.Length for double");

            return val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SkipNumber(ReadOnlySpan<byte> bytes, ref int i, out bool isDecimal)
        {
            ReadOnlySpan<byte> local = bytes;
            isDecimal = false;
            ReadOnlySpan<byte> terms = numberTerminators;
            for (; i < local.Length; i++)
            {
                byte b = local[i];
                if (terms.IndexOf(b) == -1)
                {
                    continue;
                }
                if (b == (byte)'.')
                {
                    isDecimal = true;
                    continue;
                }

                return;
            }
        }
    }
}