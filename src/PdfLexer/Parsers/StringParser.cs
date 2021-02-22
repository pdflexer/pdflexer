using System;
using System.Buffers;
using System.Buffers.Text;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
        // PdfEncoding : 162-255 same, 20-126 same, 127 undefined, 0-21 same
        // MaxExpert??

        private StringStatus Status = StringStatus.None;
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
            var b = buffer[0];
            if (b == '(')
            {
                return ParseStringLiteral(buffer, out _);
            } else if (b == (byte)'<')
            {
                return ParseStringHex(buffer, out _);
            }
            throw new ApplicationException("Invalid string, first char not ( or <.");
        }

        private byte[] hexBuffer = new byte[2];
        private PdfString ParseStringHex(ReadOnlySpan<byte> buffer, out int length)
        {

            length = 0;
            bool completed = true;
            var data = ArrayPool<byte>.Shared.Rent((buffer.Length-2)/2);
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
                    length = i+1;
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

            var span = new Span<byte>(data).Slice(0, di);
            return new PdfString(Encoding.UTF8.GetString(span)); // TODO bytes?
        }

        private PdfString value;
        internal StringBuilder builder = new StringBuilder();
        internal PdfString GetCurrentString()
        {
            if (value == null)
            {
                var str = builder.ToString();
                builder.Clear();
                Status = StringStatus.None;
                return new PdfString(str);
            } else
            {
                var val = value;
                value = null;
                return val;
            }

        }
        private char[] charResults = new char[1];
        private byte[] charBuff = new byte[1];
        private PdfString ParseStringLiteral(ReadOnlySpan<byte> buffer, out int total)
        {
            builder.Clear();
            total = 0;
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
                                    AddToBuilder(buffer.Slice(0, pos));
                                    builder.Append('\n');
                                    pos+=1;
                                    break;
                                case (byte)'r':
                                    AddToBuilder(buffer.Slice(0, pos));
                                    builder.Append('\r');
                                    pos+=1;
                                    break;
                                case (byte)'t':
                                    AddToBuilder(buffer.Slice(0, pos));
                                    builder.Append('\t');
                                    pos+=1;
                                    break;
                                case (byte)'b':
                                    AddToBuilder(buffer.Slice(0, pos));
                                    builder.Append('\b');
                                    pos+=1;
                                    break;
                                case (byte)'f':
                                    AddToBuilder(buffer.Slice(0, pos));
                                    builder.Append('\f');
                                    pos+=1;
                                    break;
                                case (byte)'(':
                                    AddToBuilder(buffer.Slice(0, pos));
                                    builder.Append('(');
                                    pos+=1;
                                    break;
                                case (byte)')':
                                    AddToBuilder(buffer.Slice(0, pos));
                                    builder.Append(')');
                                    pos+=1;
                                    break;
                                case (byte)'\\':
                                    AddToBuilder(buffer.Slice(0, pos));
                                    builder.Append('\\');
                                    pos+=1;
                                    break;
                                case (byte)'\r':
                                    if (buffer.Length > pos+2)
                                    {
                                        var b3 = buffer[pos+2];
                                        if (b3 == (byte)'\n')
                                        {
                                            AddToBuilder(buffer.Slice(0, pos));
                                            pos+=2;
                                            break;
                                        }
                                        AddToBuilder(buffer.Slice(0, pos));
                                        pos+=1;
                                        break;
                                    }
                                    throw CommonUtil.DisplayDataErrorException(buffer,pos,"String ended incorrectly");
                                case (byte)'\n':
                                    AddToBuilder(buffer.Slice(0, pos));
                                    pos+=1;
                                    break;
                                default:
                                    if (buffer.Length < pos+3)
                                    {
                                        AddToBuilder(buffer.Slice(0, pos));
                                        break;
                                    }
                                    {
                                        b2 = buffer[pos+1];
                                        var b3 = buffer[pos+2];
                                        if (b2 < 48 || b2 > 55 || b3 < 48 || b3 > 55)
                                        {
                                            AddToBuilder(buffer.Slice(0, pos));
                                            break;
                                        }

                                        if (buffer.Length > pos + 3)
                                        {
                                            byte b4 = buffer[pos+3];
                                            if (b4 < 48 || b4 > 55)
                                            {
                                                AddToBuilder(buffer.Slice(0, pos));
                                                builder.Append((char)(8*((int)b2-48)+((int)b3-48)));
                                                pos +=2;
                                                break;
                                            }

                                            AddToBuilder(buffer.Slice(0, pos));
                                            var cc = (64*((int)b2-48)+8*((int)b3-48)+((int)b4-48)) & 0xFF; // 256 max allowed
                                            if (cc > 127)
                                            {
                                                charBuff[0]=(byte)cc;
                                                _decoder.GetChars(charBuff, charResults, true);
                                                builder.Append(charResults[0]);
                                            } else
                                            {
                                                builder.Append((char)cc);
                                            }
                                            pos +=3;
                                        } else
                                        {
                                            AddToBuilder(buffer.Slice(0, pos));
                                            builder.Append((char)(8*((int)b2-48)+((int)b3-48)));
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
                        total += pos;
                        break;
                    case (byte)'(':
                        stringDepth++;
                        pos += 1;
                        if (stringDepth > 1)
                        {
                            AddToBuilder(buffer.Slice(0, pos));

                        }
                        buffer = buffer.Slice(pos);
                        total += pos;
                        continue;
                    case (byte)')':
                        stringDepth--;
                        if (stringDepth == 0)
                        {
                            AddToBuilder(buffer.Slice(0, pos));
                            pos += 1;
                            total += pos;
                            return GetCurrentString();
                        }
                        pos += 1;
                        AddToBuilder(buffer.Slice(0, pos));
                        buffer = buffer.Slice(pos);
                        total += pos;
                        continue;
                }
            }
            throw CommonUtil.DisplayDataErrorException(buffer,pos,"String ended incorrectly");
        }
        

        private void AddToBuilder(ReadOnlySpan<byte> data)
        {
            // TODO optimize.. for now this is easy
            builder.Append(Iso88591.GetString(data));
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
            // TODO allow rentrancy?
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