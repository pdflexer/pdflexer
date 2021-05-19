using System;
using System.Buffers;
using System.Buffers.Text;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace PdfLexer.Parsers
{
    public class NumberParser : Parser<PdfNumber>
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

        public override PdfNumber Parse(ReadOnlySpan<byte> buffer)
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

            if (buffer.Length == 2 && buffer[0] == (byte)'-' && buffer[1] == (byte)'1')
            {
                return PdfCommonNumbers.MinusOne;
            }

            if (buffer[0] == (byte)'+')
            {
                buffer = buffer.Slice(1);
            }

            ulong key = default;
            if (_ctx.Options.CacheNumbers && _ctx.NumberCache.TryGetNumber(buffer, out key, out var result))
            {
                return result;
            }

            var value = GetResult(buffer);

            if (key > 0)
            {
                _ctx.NumberCache.AddValue(key, value);
            }

            return value;
        }

        private PdfNumber GetResult(ReadOnlySpan<byte> buffer)
        {
            if (buffer.Length > 9)
            {
                return new PdfLongNumber(GetLong(buffer));
            }
            else
            {
                return new PdfIntNumber(GetInt(buffer));
            }
        }

        private int GetInt(ReadOnlySpan<byte> buffer)
        {
            if (!Utf8Parser.TryParse(buffer, out int val, out int consumed))
            {
                throw new ApplicationException("Bad data for int number: " + Encoding.ASCII.GetString(buffer));
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SkipNumber(ReadOnlySpan<byte> bytes, ref int i, out bool isDecimal)
        {
            ReadOnlySpan<byte> local = bytes;
            isDecimal = false;
            for (; i < local.Length; i++)
            {
                var b = local[i];
                if (b == (byte)'.')
                {
                    isDecimal = true;
                    continue;
                }
                else if ((b > 47 && b < 58) || b == '+' || b == '-')
                {
                    continue;
                }

                return;
            }
        }
    }
}