using System;
using System.Buffers;
using System.Buffers.Text;
using System.Diagnostics;
using System.Text;

namespace PdfLexer.Parsers
{
    internal enum StringStatus
    {
        None,
        ParsingLiteral,
        ParsingHex
    }
    public class StringParser : Parser<PdfString>
    {
        private static Encoding Iso88591 = Encoding.GetEncoding("ISO-8859-1"); // StandardEncoding
        private static Encoding Win1252 = CodePagesEncodingProvider.Instance.GetEncoding(1252); // WinAnsiEncoding
        private static Encoding MacRoman = CodePagesEncodingProvider.Instance.GetEncoding("macintosh"); // WinAnsiEncoding
        // PdfEncoding : ???-255 same, 20-126 same, 127 undefined, 0-21 same
        // MaxExpert??

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
            var rented = ArrayPool<byte>.Shared.Rent(buffer.Length);

            var length = ConvertBytes(buffer, rented);

            var encoding = PdfTextEncodingType.PdfDocument;
            if (length > 1 && rented[0] == 0xFE && rented[1] == 0xFF)
            {
                encoding = PdfTextEncodingType.UTF16BE;
                var str = new PdfString(Encoding.BigEndianUnicode.GetString(rented, 2, length-2),
                    buffer[0] == '(' ? PdfStringType.Literal : PdfStringType.Hex, encoding);
                ArrayPool<byte>.Shared.Return(rented);
                return str;
            }
            
            // TODO real PdfDocEncoded decoding
            var dictStr = new PdfString(Iso88591.GetString(rented, 0, length), buffer[0] == '(' ? PdfStringType.Literal : PdfStringType.Hex, encoding);
            ArrayPool<byte>.Shared.Return(rented);
            return dictStr;
        }

        public int ConvertBytes(ReadOnlySpan<byte> input, Span<byte> buffer)
        {
            var b = input[0];
            if (b == (byte)'(')
            {
                return ConvertLiteralBytes(input, buffer);
            } else if (b == (byte)'<')
            {
                return ConvertHexBytes(input, buffer);
            }
            throw new ApplicationException("Invalid string, first char not ( or <.");
        }

        private byte[] hexBuffer = new byte[2];
        private int ConvertHexBytes(ReadOnlySpan<byte> buffer, Span<byte> data)
        {
            bool completed = true;
            var di = 0;
            bool isLow = true;
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
                } else
                {
                    hexBuffer[1] = b;
                    
                    if (!Utf8Parser.TryParse(hexBuffer, out byte value, out int consumed, 'x'))
                    {
                        throw CommonUtil.DisplayDataErrorException(buffer, i, "Bad hex string data");
                    }
                    Debug.Assert(consumed == 2);
                    data[di++] = value;
                }
                isLow = !isLow;
            }

            if (!completed)
            {
                throw CommonUtil.DisplayDataErrorException(buffer, buffer.Length-1, "Unexpected hex string end");
            }

            if (!isLow)
            {
                hexBuffer[1] = (byte)'0';
                if (!Utf8Parser.TryParse(hexBuffer, out byte v, out int c, 'x'))
                {
                    throw CommonUtil.DisplayDataErrorException(buffer, buffer.Length-1, "Bad hex string data");
                }
                Debug.Assert(c == 2);
                data[di++] = v;
            }


            return di;
        }

        private int ConvertLiteralBytes_Bad(ReadOnlySpan<byte> buffer, Span<byte> output)
        {
            Span<byte> data = output;
            var total = 0;
            var lastRead = 0;
            var pos = 0;
            // while ((pos = buffer.IndexOfAny(stringLiteralTerms)) > -1)
            for (;pos<buffer.Length;)
            {
                var b = buffer[pos];
                switch (b)
                {
                    case (byte)'\\':

                        if (buffer.Length > pos+1)
                        {
                            var len = pos-lastRead;
                            var b2 = buffer[pos+1];
                            switch (b2)
                            {
                                case (byte)'n':
                                    buffer.Slice(lastRead, len).CopyTo(data.Slice(total));
                                    total += len;
                                    data[total++] = (byte)'\n';
                                    pos+=2; // skip byte
                                    lastRead = pos;
                                    break;
                                case (byte)'r':
                                    buffer.Slice(lastRead, len).CopyTo(data.Slice(total));
                                    total += len;
                                    data[total++] = (byte)'\r';
                                    pos+=2; // skip byte
                                    lastRead = pos;
                                    break;
                                case (byte)'t':
                                    buffer.Slice(lastRead, len).CopyTo(data.Slice(total));
                                    total += len;
                                    data[total++] = (byte)'\t';
                                    pos+=2;
                                    lastRead = pos;
                                    break;
                                case (byte)'b':
                                    buffer.Slice(lastRead, len).CopyTo(data.Slice(total));
                                    total += len;
                                    data[total++] = (byte)'\b';
                                    pos+=2;
                                    lastRead = pos;
                                    break;
                                case (byte)'f':
                                    buffer.Slice(lastRead, len).CopyTo(data.Slice(total));
                                    total += len;
                                    data[total++] = (byte)'\f';
                                    // AddToBuilder(buffer.Slice(0, pos));
                                    // builder.Append('\f');
                                    pos+=2;
                                    lastRead = pos;
                                    break;
                                case (byte)'(':
                                    buffer.Slice(lastRead, len).CopyTo(data.Slice(total));
                                    total += len;
                                    data[total++] = (byte)'(';
                                    pos+=2;
                                    lastRead = pos;
                                    break;
                                case (byte)')':
                                    buffer.Slice(lastRead, len).CopyTo(data.Slice(total));
                                    total += len;
                                    data[total++] = (byte)')';
                                    pos+=2;
                                    lastRead = pos;
                                    break;
                                case (byte)'\\':
                                    buffer.Slice(lastRead, len).CopyTo(data.Slice(total));
                                    total += len;
                                    data[total++] = (byte)'\\';
                                    pos+=2;
                                    lastRead = pos;
                                    break;
                                case (byte)'\r':
                                    if (buffer.Length > pos+2)
                                    {
                                        var b3 = buffer[pos+2];
                                        if (b3 == (byte)'\n')
                                        {
                                            buffer.Slice(lastRead, len).CopyTo(data.Slice(total));
                                            total += len;
                                            pos+=3;
                                            lastRead = pos;
                                            break;
                                        }
                                        buffer.Slice(lastRead, len).CopyTo(data.Slice(total));
                                        total += len;
                                        pos+=2;
                                        lastRead = pos;
                                        break;
                                    }
                                    throw CommonUtil.DisplayDataErrorException(buffer,pos,"String ended incorrectly");
                                case (byte)'\n':
                                    buffer.Slice(lastRead, len).CopyTo(data.Slice(total));
                                    total += len;
                                    pos+=2;
                                    lastRead = pos;
                                    break;
                                default:
                                    if (buffer.Length < pos+3)
                                    {
                                        buffer.Slice(lastRead, len).CopyTo(data.Slice(total));
                                        total += len;
                                        pos += 1;
                                        lastRead = pos;
                                        break;
                                    }
                                    {
                                        b2 = buffer[pos+1];
                                        var b3 = buffer[pos+2];
                                        if (b2 < 48 || b2 > 55 || b3 < 48 || b3 > 55)
                                        {
                                            buffer.Slice(lastRead, len).CopyTo(data.Slice(total));
                                            total += len;
                                            pos += 2;
                                            lastRead = pos;
                                            break;
                                        }

                                        if (buffer.Length > pos + 3)
                                        {
                                            byte b4 = buffer[pos+3];
                                            if (b4 < 48 || b4 > 55)
                                            {
                                                buffer.Slice(lastRead, len).CopyTo(data.Slice(total));
                                                total += len;
                                                data[total++] = (byte)(8*((int)b2-48)+((int)b3-48));
                                                pos +=3;
                                                lastRead = pos;
                                                break;
                                            }
                                            
                                            buffer.Slice(lastRead, len).CopyTo(data.Slice(total));
                                            total += len;

                                            var cc = (64*((int)b2-48)+8*((int)b3-48)+((int)b4-48)) & 0xFF; // 256 max allowed
                                            data[total++] = (byte)cc;
                                            pos +=4;
                                            lastRead = pos;
                                        } else
                                        {
                                            buffer.Slice(lastRead, len).CopyTo(data.Slice(total));
                                            total += len;
                                            data[total++] = (byte)(8*((int)b2-48)+((int)b3-48));
                                            pos +=2;
                                            lastRead = pos;
                                        }
                                        break;
                                    }
                            }
                        } else
                        {
                            throw CommonUtil.DisplayDataErrorException(buffer,pos,"String ended incorrectly");
                        }

                        continue;
                    case (byte)'(':
                        var lenn = pos - lastRead;
                        stringDepth++;
                        pos += 1;
                        if (stringDepth == 1)
                        {
                            lastRead = pos;
                        }
                        continue;
                    case (byte)')':
                        stringDepth--;
                        if (stringDepth == 0)
                        {
                            lenn = pos - lastRead;
                            buffer.Slice(lastRead, lenn).CopyTo(data.Slice(total));
                            total += lenn;
                            return total;
                        }
                        pos += 1;
                        continue;
                }
                pos += 1;
            }
            throw CommonUtil.DisplayDataErrorException(buffer,pos,"String ended incorrectly");
        }

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
                        if (buffer.Length > pos+1)
                        {
                            var b2 = buffer[pos+1];
                            switch (b2)
                            {
                                case (byte)'n':
                                    buffer.Slice(0, pos).CopyTo(data.Slice(total));
                                    total+= pos;
                                    data[total++] = (byte)'\n';
                                    pos+=1;
                                    break;
                                case (byte)'r':
                                    buffer.Slice(0, pos).CopyTo(data.Slice(total));
                                    total+= pos;
                                    data[total++] = (byte)'\r';
                                    pos+=1;
                                    break;
                                case (byte)'t':
                                    buffer.Slice(0, pos).CopyTo(data.Slice(total));
                                    total+= pos;
                                    data[total++] = (byte)'\t';
                                    pos+=1;
                                    break;
                                case (byte)'b':
                                    buffer.Slice(0, pos).CopyTo(data.Slice(total));
                                    total+= pos;
                                    data[total++] = (byte)'\b';
                                    pos+=1;
                                    break;
                                case (byte)'f':
                                    buffer.Slice(0, pos).CopyTo(data.Slice(total)); total+= pos;
                                    data[total++] = (byte)'\f';
                                    // AddToBuilder(buffer.Slice(0, pos));
                                    // builder.Append('\f');
                                    pos+=1;
                                    break;
                                case (byte)'(':
                                    buffer.Slice(0, pos).CopyTo(data.Slice(total));
                                    total+= pos;
                                    data[total++] = (byte)'(';
                                    pos+=1;
                                    break;
                                case (byte)')':
                                    buffer.Slice(0, pos).CopyTo(data.Slice(total));
                                    total+= pos;
                                    data[total++] = (byte)')';
                                    pos+=1;
                                    break;
                                case (byte)'\\':
                                    buffer.Slice(0, pos).CopyTo(data.Slice(total));
                                    total+= pos;
                                    data[total++] = (byte)'\\';
                                    pos+=1;
                                    break;
                                case (byte)'\r':
                                    if (buffer.Length > pos+2)
                                    {
                                        var b3 = buffer[pos+2];
                                        if (b3 == (byte)'\n')
                                        {
                                            buffer.Slice(0, pos).CopyTo(data.Slice(total));
                                            total+= pos;
                                            pos+=2;
                                            break;
                                        }
                                        buffer.Slice(0, pos).CopyTo(data.Slice(total));
                                        total+= pos;
                                        pos+=1;
                                        break;
                                    }
                                    throw CommonUtil.DisplayDataErrorException(buffer,pos,"String ended incorrectly");
                                case (byte)'\n':
                                    buffer.Slice(0, pos).CopyTo(data.Slice(total));
                                    total+= pos;
                                    pos+=1;
                                    break;
                                default:
                                    if (buffer.Length < pos+3)
                                    {
                                        buffer.Slice(0, pos).CopyTo(data.Slice(total));
                                        total+= pos;
                                        break;
                                    }
                                    {
                                        b2 = buffer[pos+1];
                                        var b3 = buffer[pos+2];
                                        if (b2 < 48 || b2 > 55 || b3 < 48 || b3 > 55)
                                        {
                                            buffer.Slice(0, pos).CopyTo(data.Slice(total));
                                            total+= pos;
                                            break;
                                        }

                                        if (buffer.Length > pos + 3)
                                        {
                                            byte b4 = buffer[pos+3];
                                            if (b4 < 48 || b4 > 55)
                                            {
                                                buffer.Slice(0, pos).CopyTo(data.Slice(total));
                                                total+= pos;
                                                data[total++] = (byte)(8*((int)b2-48)+((int)b3-48));
                                                pos +=2;
                                                break;
                                            }
                                            
                                            buffer.Slice(0, pos).CopyTo(data.Slice(total));
                                            total+= pos;

                                            var cc = (64*((int)b2-48)+8*((int)b3-48)+((int)b4-48)) & 0xFF; // 256 max allowed
                                            data[total++] = (byte)cc;
                                            pos +=3;
                                        } else
                                        {
                                            buffer.Slice(0, pos).CopyTo(data.Slice(total));
                                            total+= pos;
                                            data[total++] = (byte)(8*((int)b2-48)+((int)b3-48));
                                            pos +=2;
                                        }
                                        break;
                                    }
                            }
                        } else
                        {
                            throw CommonUtil.DisplayDataErrorException(buffer,pos,"String ended incorrectly");
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
                            total+= pos;
                        }
                        buffer = buffer.Slice(pos);
                        continue;
                    case (byte)')':
                        stringDepth--;
                        if (stringDepth == 0)
                        {
                            buffer.Slice(0, pos).CopyTo(data.Slice(total));
                            total+= pos;
                            return total;
                        }
                        pos += 1;
                        buffer.Slice(0, pos).CopyTo(data.Slice(total));
                        total+= pos;
                        buffer = buffer.Slice(pos);
                        continue;
                }
            }
            throw CommonUtil.DisplayDataErrorException(buffer,pos,"String ended incorrectly");
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
            } else if (b == (byte)'<')
            {
                return reader.TryAdvanceTo((byte) '>', true);
            }

            throw new ApplicationException("Invalid string, first char not ( or <.");
        }

        public static bool AdvancePastString(ReadOnlySpan<byte> data, ref int i)
        {
            var b = data[i];

            if (b == (byte)'(')
            {
                return  AdvancePastStringLiteral(data, ref i);
            } else if (b == (byte)'<')
            {
                var end = data.IndexOf((byte) '>');
                if (end == -1)
                {
                    return false;
                }

                i = end+1;
                return true;
            }

            throw new ApplicationException("Invalid string, first char not ( or <.");
        }
        internal static bool AdvancePastHexString(ReadOnlySpan<byte> data, ref int i)
        {
            var end = data.Slice(i).IndexOf((byte) '>');
            if (end == -1)
            {
                return false;
            }

            i = i+end+1;
            return true;
        }
        internal static bool AdvancePastStringLiteral(ReadOnlySpan<byte> data, ref int i)
        {
            // TODO data.IndexOfAny
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
                } else if (b == '(')
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
            reader.Rewind(reader.Consumed-orig);
            return false;
        }


    }
}