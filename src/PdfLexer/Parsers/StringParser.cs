using System.Buffers;
using System.Buffers.Text;
using System.Diagnostics;
using System.Text;

namespace PdfLexer.Parsers;

internal enum StringStatus
{
    None,
    ParsingLiteral,
    ParsingHex
}
internal class StringParser : Parser<PdfString>
{
    internal static Encoding Iso88591 = Encoding.GetEncoding("ISO-8859-1"); // StandardEncoding
                                                                            // Require codepages nuget package... determine if really needed
                                                                            // private static Encoding Win1252 = CodePagesEncodingProvider.Instance.GetEncoding(1252); // WinAnsiEncoding
                                                                            // private static Encoding MacRoman = CodePagesEncodingProvider.Instance.GetEncoding("macintosh"); // WinAnsiEncoding
                                                                            // PdfEncoding : ???-255 same, 20-126 same, 127 undefined, 0-21 same
                                                                            // MaxExpert??
    private static int[] mapping = new int[]{
0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
0x2D8, 0x2C7, 0x2C6, 0x2D9, 0x2DD, 0x2DB, 0x2DA, 0x2DC, 0, 0, 0, 0, 0, 0, 0,
0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x2022, 0x2020, 0x2021, 0x2026, 0x2014,
0x2013, 0x192, 0x2044, 0x2039, 0x203A, 0x2212, 0x2030, 0x201E, 0x201C,
0x201D, 0x2018, 0x2019, 0x201A, 0x2122, 0xFB01, 0xFB02, 0x141, 0x152, 0x160,
0x178, 0x17D, 0x131, 0x142, 0x153, 0x161, 0x17E, 0, 0x20AC
    };

    // TODO: switch string types to based on pdf types (text, ascii, byte). Add enum to denote if it came from Hex or Literal
    // For Text type add enum to signify encoding type: UTF16BE or PdfEncoding
    // For creating byte string add overload to specify hex / literal, default to hex.
    // For creating text string add overload to specify hex / literal, default to literal
    // For creating text string add overload to specify UTF16BE or PdfEncoding, default to "Optimal", will scan to see if needs non-standard chars
    // - more performant to specify PdfEncoding (or 16) so it doesn't have to scan

    private readonly ParsingContext _ctx;
    private readonly Decoder _decoder;
    private int stringDepth = 0;
    private static byte[] stringLiteralTerms = new byte[]
    {
        (byte) '(', (byte) ')', (byte) '\\'
    };

    public StringParser(ParsingContext ctx)
    {
        _ctx = ctx;
        _decoder = Iso88591.GetDecoder();
    }

    public override PdfString Parse(ReadOnlySpan<byte> buffer)
    {
        if (buffer.Length < 200)
        {
            Span<byte> writeBuffer = stackalloc byte[buffer.Length];
            return Parse(buffer, writeBuffer);
        }

        var rented = ArrayPool<byte>.Shared.Rent(buffer.Length);

        var result = Parse(buffer, rented);
        ArrayPool<byte>.Shared.Return(rented);
        return result;
    }

    public byte[] ParseRaw(ReadOnlySpan<byte> buffer)
    {
        if (buffer.Length < 200)
        {
            Span<byte> writeBuffer = stackalloc byte[buffer.Length];
            var l = ConvertBytes(buffer, writeBuffer);
            return writeBuffer.Slice(0, l).ToArray();
        }
        else
        {
            var rented = ArrayPool<byte>.Shared.Rent(buffer.Length);
            var l = ConvertBytes(buffer, rented);
            var result = new byte[l];
            Array.Copy(rented, result, l);
            ArrayPool<byte>.Shared.Return(rented);
            return result;
        }
    }

    private PdfString Parse(ReadOnlySpan<byte> input, Span<byte> output)
    {
        var length = ConvertBytes(input, output);

        ReadOnlySpan<byte> converted = output;
        if (_ctx.IsEncrypted)
        {
            converted = _ctx.Decryption.Decrypt(_ctx.CurrentReference, Encryption.CryptoType.Strings, converted);

        }

        var encoding = PdfTextEncodingType.PdfDocument;
        if (length > 1 && converted[0] == 0xFE && converted[1] == 0xFF)
        {
            encoding = PdfTextEncodingType.UTF16BE;
            var str = new PdfString(Encoding.BigEndianUnicode.GetString(converted.Slice(2, length - 2)),
                input[0] == '(' ? PdfStringType.Literal : PdfStringType.Hex, encoding);
            return str;
        }

        // TODO real PdfDocEncoded decoding
        return new PdfString(Iso88591.GetString(converted.Slice(0, length)), input[0] == '(' ? PdfStringType.Literal : PdfStringType.Hex, encoding);
    }

    public int ConvertBytes(ReadOnlySpan<byte> input, Span<byte> buffer)
    {
        var b = input[0];
        if (b == (byte)'(')
        {
            return ConvertLiteralBytes(input, buffer);
        }
        else if (b == (byte)'<')
        {
            return ConvertHexBytes(input, buffer);
        }
        throw new ApplicationException("Invalid string, first char not ( or <.");
    }

    private int ConvertHexBytes(ReadOnlySpan<byte> buffer, Span<byte> data)
    {
        bool completed = true;
        var di = 0;
        bool isLow = true;
        Span<byte> hexBuffer = stackalloc byte[2];
        for (var i = 1; i < buffer.Length; i++)
        {
            var b = buffer[i];
            if (b == 0x00
               || b == 0x09
               || b == 0x0A
               || b == 0x0C
               || b == 0x0D
               || b == 0x20)
            {
                continue;
            }

            if (b == (byte)'>')
            {
                completed = true;
                break;
            }

            if (isLow)
            {
                hexBuffer[0] = b;
            }
            else
            {
                hexBuffer[1] = b;

                if (!Utf8Parser.TryParse(hexBuffer, out byte value, out int consumed, 'x'))
                {
                    _ctx.Error(CommonUtil.DisplayDataError(buffer, i, "Bad hex string data"));
                }
                else
                {
                    // Debug.Assert(consumed == 2);
                    data[di++] = value;
                }
            }
            isLow = !isLow;
        }

        if (!completed)
        {
            _ctx.Error(CommonUtil.DisplayDataError(buffer, buffer.Length - 1, "Unexpected hex string end"));
        }

        if (!isLow)
        {
            hexBuffer[1] = (byte)'0';
            if (!Utf8Parser.TryParse(hexBuffer, out byte v, out int c, 'x'))
            {
                _ctx.Error(CommonUtil.DisplayDataError(buffer, buffer.Length - 1, "Bad hex string data at end"));
            }
            else
            {
                Debug.Assert(c == 2);
                data[di++] = v;
            }

        }

        return di;
    }

    // TODO this is overly complicated
    private int ConvertLiteralBytes(ReadOnlySpan<byte> buffer, Span<byte> output)
    {
        Span<byte> data = output;
        var total = 0;
        var pos = 0;
        while ((pos = buffer.IndexOfAny(stringLiteralTerms)) > -1)
        {
            var b = buffer[pos];
            switch (b)
            {
                case (byte)'\\':
                    if (buffer.Length > pos + 1)
                    {
                        var b2 = buffer[pos + 1];
                        switch (b2)
                        {
                            case (byte)'n':
                                buffer.Slice(0, pos).CopyTo(data.Slice(total));
                                total += pos;
                                data[total++] = (byte)'\n';
                                pos += 1;
                                break;
                            case (byte)'r':
                                buffer.Slice(0, pos).CopyTo(data.Slice(total));
                                total += pos;
                                data[total++] = (byte)'\r';
                                pos += 1;
                                break;
                            case (byte)'t':
                                buffer.Slice(0, pos).CopyTo(data.Slice(total));
                                total += pos;
                                data[total++] = (byte)'\t';
                                pos += 1;
                                break;
                            case (byte)'b':
                                buffer.Slice(0, pos).CopyTo(data.Slice(total));
                                total += pos;
                                data[total++] = (byte)'\b';
                                pos += 1;
                                break;
                            case (byte)'f':
                                buffer.Slice(0, pos).CopyTo(data.Slice(total)); total += pos;
                                data[total++] = (byte)'\f';
                                // AddToBuilder(buffer.Slice(0, pos));
                                // builder.Append('\f');
                                pos += 1;
                                break;
                            case (byte)'(':
                                buffer.Slice(0, pos).CopyTo(data.Slice(total));
                                total += pos;
                                data[total++] = (byte)'(';
                                pos += 1;
                                break;
                            case (byte)')':
                                buffer.Slice(0, pos).CopyTo(data.Slice(total));
                                total += pos;
                                data[total++] = (byte)')';
                                pos += 1;
                                break;
                            case (byte)'\\':
                                buffer.Slice(0, pos).CopyTo(data.Slice(total));
                                total += pos;
                                data[total++] = (byte)'\\';
                                pos += 1;
                                break;
                            case (byte)'\r':
                                if (buffer.Length > pos + 2)
                                {
                                    var b3 = buffer[pos + 2];
                                    if (b3 == (byte)'\n')
                                    {
                                        buffer.Slice(0, pos).CopyTo(data.Slice(total));
                                        total += pos;
                                        pos += 2;
                                        break;
                                    }
                                    buffer.Slice(0, pos).CopyTo(data.Slice(total));
                                    total += pos;
                                    pos += 1;
                                    break;
                                }
                                throw CommonUtil.DisplayDataErrorException(buffer, pos, "String ended incorrectly");
                            case (byte)'\n':
                                buffer.Slice(0, pos).CopyTo(data.Slice(total));
                                total += pos;
                                pos += 1;
                                break;
                            default:
                                if (buffer.Length < pos + 3)
                                {
                                    buffer.Slice(0, pos).CopyTo(data.Slice(total));
                                    total += pos;
                                    break;
                                }
                                {
                                    b2 = buffer[pos + 1];
                                    var b3 = buffer[pos + 2];
                                    if (b2 < 48 || b2 > 55 || b3 < 48 || b3 > 55)
                                    {
                                        buffer.Slice(0, pos).CopyTo(data.Slice(total));
                                        total += pos;
                                        break;
                                    }

                                    if (buffer.Length > pos + 3)
                                    {
                                        byte b4 = buffer[pos + 3];
                                        if (b4 < 48 || b4 > 55)
                                        {
                                            buffer.Slice(0, pos).CopyTo(data.Slice(total));
                                            total += pos;
                                            data[total++] = (byte)(8 * ((int)b2 - 48) + ((int)b3 - 48));
                                            pos += 2;
                                            break;
                                        }

                                        buffer.Slice(0, pos).CopyTo(data.Slice(total));
                                        total += pos;

                                        var cc = (64 * ((int)b2 - 48) + 8 * ((int)b3 - 48) + ((int)b4 - 48)) & 0xFF; // 256 max allowed
                                        data[total++] = (byte)cc;
                                        pos += 3;
                                    }
                                    else
                                    {
                                        buffer.Slice(0, pos).CopyTo(data.Slice(total));
                                        total += pos;
                                        data[total++] = (byte)(8 * ((int)b2 - 48) + ((int)b3 - 48));
                                        pos += 2;
                                    }
                                    break;
                                }
                        }
                    }
                    else
                    {
                        throw CommonUtil.DisplayDataErrorException(buffer, pos, "String ended incorrectly");
                    }
                    pos += 1;
                    buffer = buffer.Slice(pos);
                    break;
                case (byte)'(':
                    stringDepth++;
                    pos += 1;
                    if (stringDepth > 1)
                    {
                        buffer.Slice(0, pos).CopyTo(data.Slice(total));
                        total += pos;
                    }
                    buffer = buffer.Slice(pos);
                    continue;
                case (byte)')':
                    stringDepth--;
                    if (stringDepth == 0)
                    {
                        buffer.Slice(0, pos).CopyTo(data.Slice(total));
                        total += pos;
                        return total;
                    }
                    pos += 1;
                    buffer.Slice(0, pos).CopyTo(data.Slice(total));
                    total += pos;
                    buffer = buffer.Slice(pos);
                    continue;
            }
        }
        throw CommonUtil.DisplayDataErrorException(buffer, pos, "String ended incorrectly");
    }

    public static bool TryAdvancePastString(ref SequenceReader<byte> reader)
    {
        if (!reader.TryPeek(out byte b))
        {
            return false;
        }

        if (b == (byte)'(')
        {
            return AdvancePastStringLiteral(ref reader);
        }
        else if (b == (byte)'<')
        {
            return reader.TryAdvanceTo((byte)'>', true);
        }

        throw new ApplicationException("Invalid string, first char not ( or <.");
    }
    public static bool AdvancePastString(ReadOnlySpan<byte> data, ref int i)
    {
        var b = data[i];

        if (b == (byte)'(')
        {
            return AdvancePastStringLiteral(data, ref i);
        }
        else if (b == (byte)'<')
        {
            var end = data.IndexOf((byte)'>');
            if (end == -1)
            {
                return false;
            }

            i = end + 1;
            return true;
        }

        throw new ApplicationException("Invalid string, first char not ( or <.");
    }
    internal static bool AdvancePastHexString(ReadOnlySpan<byte> data, ref int i)
    {
        var end = data.Slice(i).IndexOf((byte)'>');
        if (end == -1)
        {
            return false;
        }

        i = i + end + 1;
        return true;
    }
    internal static bool AdvancePastStringLiteral(ReadOnlySpan<byte> data, ref int i)
    {
        ReadOnlySpan<byte> local = data;
        var depth = 0;
        for (; i < local.Length; i++)
        {
            byte b = local[i];
            if (b == '\\')
            {
                if (!(local.Length > i + 1))
                {
                    return false;
                }
                i++;
                continue;
            }
            else if (b == '(')
            {
                depth++;
            }
            else if (b == ')')
            {
                depth--;
            }

            if (depth == 0)
            {
                i += 1;
                return true;
            }
        }
        return false;
    }

    internal static bool AdvancePastStringLiteralLong(ReadOnlySpan<byte> data, ref int i)
    {
        i += 1; // (
        var depth = 1;
        while (true)
        {
            var current = data.Slice(i);

            var ie = current.IndexOf((byte)')');
            if (ie == -1) { return false; }
            var es = 0;
            while (data[i + ie - es - 1] == (byte)'\\')
            {
                es++;
            }
            if (es % 2 != 0)
            {
                i += ie + 1;
                continue;
            }


            var ii = i;
            current = current.Slice(0, ie);
        CHECK_OP:
            var ib = current.IndexOf((byte)'(');
            if (ib != -1)
            {
                es = 0;
                while (data[ii + ib - es - 1] == (byte)'\\')
                {
                    es++;
                }
                if (es % 2 == 0)
                {
                    depth++;
                    current = current.Slice(ib + 1);
                    ii += ib + 1;
                    goto CHECK_OP;
                }

            }
            depth--;
            i += ie + 1;
            if (depth == 0)
            {
                return true;
            }
        }
    }
    internal static bool AdvancePastStringLiteral(ref SequenceReader<byte> reader)
    {
        var orig = reader.Consumed;
        var stringDepth = 0;
        while (reader.TryAdvanceToAny(stringLiteralTerms, false))
        {
            if (!reader.TryRead(out byte b))
            {
                return false;
            }
            switch (b)
            {
                case (byte)'\\':
                    if (!reader.TryRead(out _))
                    {
                        reader.Rewind(1);
                        return false;
                    }
                    break;
                case (byte)'(':
                    stringDepth++;
                    if (stringDepth == 1)
                    {
                        break;
                    }
                    continue;
                case (byte)')':
                    stringDepth--;
                    if (stringDepth == 0)
                    {
                        return true;
                    }
                    continue;
            }
        }
        reader.Rewind(reader.Consumed - orig);
        return false;
    }
}
