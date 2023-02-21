using System.Buffers;
using System.Buffers.Text;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace PdfLexer.Parsers;

internal class NameParser : Parser<PdfName>
{
    private static Encoding Iso88591 = Encoding.GetEncoding("ISO-8859-1"); // StandardEncoding
    internal static byte[] NameTerminators = new byte[16] { 0x00, 0x09, 0x0A, 0x0C, 0x0D, 0x20,
        (byte)'(', (byte)')', (byte)'<', (byte)'>', (byte)'[', (byte)']', (byte)'{', (byte)'}', (byte)'/', (byte)'%' };
    private ParsingContext _ctx;

    public NameParser(ParsingContext ctx)
    {
        _ctx = ctx;
    }

    public override PdfName Parse(ReadOnlySpan<byte> buffer)
    {
        ulong key = 0;
        if (_ctx.Options.CacheNames)
        {
            if (_ctx.NameCache.TryGetName(buffer, out key, out var result))
            {
                return result;
            }
        }
        var val = buffer.IndexOf((byte)'#') == -1 ? ParseFastNoHex(buffer) : ParseWithHex(buffer, 1, buffer.Length-1);
        if (key > 0)
        {
            _ctx.NameCache.AddValue(key, val);
        }
        return val;
    }

    private PdfName ParseFastNoHex(ReadOnlySpan<byte> buffer)
    {
        // note, pdf1.7 spec names should be treated at utf-8 for cases where they need a text representation
        return new PdfName(Iso88591.GetString(buffer.Slice(1)), false);
    }

    private PdfName ParseWithHex(ReadOnlySpan<byte> buffer, int start, int length)
    {
        if (buffer.Length < 50)
        {
            Span<byte> write = stackalloc byte[buffer.Length];
            return ParseWithHex(buffer, write, start, length);
        }

        var rented = ArrayPool<byte>.Shared.Rent(buffer.Length);
        var name = ParseWithHex(buffer, rented, start, length);
        ArrayPool<byte>.Shared.Return(rented);
        return name;
    }

    private PdfName ParseWithHex(ReadOnlySpan<byte> buffer, Span<byte> writebuffer, int start, int length)
    {
        var ci = 0;
        Span<byte> bytes = writebuffer;
        for (var i = start; i < start + length; i++)
        {
            var c = buffer[i];
            if (c == (byte)'#')
            {
                if (buffer.Length < i + 3 || !Utf8Parser.TryParse(buffer.Slice(i + 1, 2), out byte v, out int ic, 'x'))
                {
                    _ctx.Error("Invalid hex found, ignoring: " + Encoding.ASCII.GetString(buffer.Slice(i, Math.Min(buffer.Length - i - 1, 3))));
                    i += 2;
                } else
                {
                    Debug.Assert(ic == 2);
                    i += 2;
                    bytes[ci++] = v;
                }
            }
            else
            {
                bytes[ci++] = c;
            }
        }

        if (ci <= 1)
        {
            // garbage name resulted in no chars
            return Err;
        }
        // note, pdf1.7 spec names should be treated at utf-8 for cases where they need a text representation
        var name = new PdfName(Iso88591.GetString(bytes.Slice(0, ci)), true);
        return name;
    }
    private static readonly PdfName Err = new PdfName("Err");
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SkipName(ReadOnlySpan<byte> bytes, ref int i)
    {
        ReadOnlySpan<byte> local = bytes;
        ReadOnlySpan<byte> terms = NameTerminators;
        for (; i < local.Length; i++)
        {
            byte b = local[i];
            if (terms.IndexOf(b) == -1)
            {
                continue;
            }

            return;
        }
    }

}
