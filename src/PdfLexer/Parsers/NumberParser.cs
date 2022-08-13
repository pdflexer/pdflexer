using System.Buffers.Text;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace PdfLexer.Parsers;

internal class NumberParser : Parser<PdfNumber>
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
        if (buffer[0] == (byte)'+')
        {
            if (buffer.Length == 1) { return PdfCommonNumbers.Zero; } // adobe convention
            buffer = buffer.Slice(1);
        }

        if (buffer.Length == 1)
        {
            switch (buffer[0])
            {
                case (byte)'0':
                case (byte)'.': // adobe convention
                case (byte)'-': // adobe convention
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
            _ctx.Error("Bad data for int number: " + Encoding.ASCII.GetString(buffer));
            if (buffer.IndexOf((byte)'-') > 0)
            {
                // TODO -> adobe ignores
            }
            return 0;
        }
        Debug.Assert(consumed == buffer.Length, "consumed == buffer.Length for int");

        return val;
    }

    private long GetLong(ReadOnlySpan<byte> buffer)
    {
        if (!Utf8Parser.TryParse(buffer, out long val, out int consumed))
        {
            _ctx.Error("Bad data for long number: " + Encoding.ASCII.GetString(buffer));
            if (buffer.IndexOf((byte)'-') > 0)
            {
                // TODO -> adobe ignores
            }
            return 0;
        }
        Debug.Assert(consumed == buffer.Length, "consumed == buffer.Length for long");

        return val;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SkipNumber(ReadOnlySpan<byte> bytes, ref int i, out bool isDecimal)
    {
        // (lenient parsing consistent with adobe)
        // ignore line breaks (between +/- and data) - TODO
        // ignore double negative - done
        // single decimal point = 0 - done
        // single minus = 0 - done
        // add scientific notation, need to peak after E to see if 0-9,+,-  TODO
        // ignore minus sign in middle of number TODO

        var start = i;
        ReadOnlySpan<byte> local = bytes;
        isDecimal = false;
        for (; i < local.Length; i++)
        {
            var b = local[i];
            if (b == (byte)'.')
            {
                if (!isDecimal)
                {
                    isDecimal = true;
                    continue;
                }
                return;
            }
            else if (b > 47 && b < 58)
            {
                continue;
            } else if ((b == '+' || b == '-') && start == i)
            {
                continue;
            }
            return;
        }
    }
}
