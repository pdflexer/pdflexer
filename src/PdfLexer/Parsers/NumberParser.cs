using System;
using System.Buffers;
using System.Buffers.Text;
using System.Diagnostics;
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
        private static readonly byte[] numberTerminators = new byte[16] { 0x00, 0x09, 0x0A, 0x0C, 0x0D, 0x20,
            (byte)'(', (byte)')', (byte)'<', (byte)'>', (byte)'[', (byte)']', (byte)'{', (byte)'}', (byte)'/', (byte)'%' };
        private readonly ParsingContext _ctx;

        public NumberParser(ParsingContext ctx)
        {
            _ctx = ctx;
        }

        public PdfNumber Parse(ReadOnlySpan<byte> buffer)
        {
            if (!_ctx.CacheNumbers)
            {
                return GetResult(buffer);
            }

            if (buffer[0] == (byte) '+')
            {
                buffer = buffer.Slice(1);
            }

            // TODO move to buffer
            var array = ArrayPool<byte>.Shared.Rent(buffer.Length);
            buffer.CopyTo(array);
            var key = new ParsingContext.CacheItem
            {
                Data = array,
                Length = buffer.Length
            };
            if (_ctx.CachedNumbers.TryGetValue(key, out var cached))
            {
                ArrayPool<byte>.Shared.Return(array);
                return cached;
            }

            key.Data = buffer.ToArray();
            ArrayPool<byte>.Shared.Return(array);

            var value = GetResult(buffer);
            _ctx.CachedNumbers[key] = value;
            return value;
        }

        private PdfNumber GetResult(ReadOnlySpan<byte> buffer)
        {
            if (buffer.IndexOf((byte) '.') > -1)
            {
                return GetDecimal(buffer);
            }
            else if (buffer.Length > 9)
            {
                return GetLong(buffer);
            } else
            {
                return GetInt(buffer);
            }
        }

        public PdfNumber Parse(ReadOnlySpan<byte> buffer, int start, int length)
        {
            return Parse(buffer.Slice(start, length));
        }

        public PdfNumber Parse(in ReadOnlySequence<byte> sequence)
        {
            // TODO optimize
            return Parse(sequence.ToArray());
        }

        public PdfNumber Parse(in ReadOnlySequence<byte> sequence, long start, int length)
        {
            // TODO optimize
            return Parse(sequence.Slice(start, length).ToArray());
        }

        private PdfIntNumber GetInt(ReadOnlySpan<byte> buffer)
        {
            if (!Utf8Parser.TryParse(buffer, out int val, out int consumed))
            {
                throw new ApplicationException("Bad data for double number: " + Encoding.ASCII.GetString(buffer));
            }
            Debug.Assert(consumed == buffer.Length, "consumed == buffer.Length for int");

            return  new PdfIntNumber(val);
        }

        private PdfLongNumber GetLong(ReadOnlySpan<byte> buffer)
        {
            if (!Utf8Parser.TryParse(buffer, out long val, out int consumed))
            {
                throw new ApplicationException("Bad data for long number: " + Encoding.ASCII.GetString(buffer));
            }
            Debug.Assert(consumed == buffer.Length, "consumed == buffer.Length for long");

            return  new PdfLongNumber(val);
        }

        private PdfDecimalNumber GetDecimal(ReadOnlySpan<byte> buffer)
        {
            if (!Utf8Parser.TryParse(buffer, out decimal val, out int consumed))
            {
                throw new ApplicationException("Bad data for double number: " + Encoding.ASCII.GetString(buffer));
            }
            Debug.Assert(consumed == buffer.Length, "consumed == buffer.Length for double");

            return  new PdfDecimalNumber(val);
        }

    

        public static int CountNumberBytes(ReadOnlySpan<byte> bytes)
        {
            return bytes.IndexOfAny(numberTerminators);
        }
    }
}