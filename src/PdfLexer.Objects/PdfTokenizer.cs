using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using PdfLexer.Objects.Parsers;

namespace PdfLexer.Objects
{
    public static class PdfTokenizer
    {
        private static byte[] eolChars = new byte[] {(byte) '\r', (byte) '\n'};
        public static bool TryReadNextToken(this ref SequenceReader<byte> reader, bool isCompleted, out PdfTokenType type, out long start, out int length)
        {
            reader.AdvancePastAny(CommonUtil.whiteSpaces);

            start = reader.Consumed;
            length = 0;
            type = PdfTokenType.NullObj;
            if (!reader.TryPeek(out byte b))
            {
                return false;
            }

            while (b == (byte) '%')
            {
                // comments
                if (!reader.TryAdvanceToAny(eolChars, true))
                {
                    return false;
                }
                reader.AdvancePastAny(CommonUtil.whiteSpaces);
                start = reader.Consumed;

                if (!reader.TryPeek(out b))
                {
                    return false;
                }
            }

            try
            {
                switch (b)
                {
                    case (byte) 't':
                        if (reader.Remaining < 4)
                        {
                            if (isCompleted) { throw new ApplicationException("Unknown token"); }
                            return false;
                        }

                        if (reader.IsNext(BoolParser.truebytes, true))
                        {
                            type = PdfTokenType.BooleanObj;
                            return true;
                        }

                        if (reader.Remaining < 7)
                        {
                            if (isCompleted) { throw new ApplicationException("Unknown token"); }
                            return false;
                        }

                        if (reader.IsNext(XRefParser.trailer, true))
                        {
                            type = PdfTokenType.Trailer;
                            return true;
                        }

                        throw new ApplicationException("Unknown token");
                    case (byte) 'f':
                        if (reader.Remaining < 4)
                        {
                            if (isCompleted) { throw new ApplicationException("Unknown token"); }
                            return false;
                        }

                        if (reader.IsNext(BoolParser.falsebytes, true))
                        {
                            type = PdfTokenType.BooleanObj;
                            return true;
                        }

                        throw new ApplicationException("Unknown token");
                    case (byte) 'n':
                        if (reader.Remaining < 4)
                        {
                            if (isCompleted) { throw new ApplicationException("Unknown token"); }
                            return false;
                        }

                        if (reader.IsNext(NullParser.nullbytes, true))
                        {
                            type = PdfTokenType.NullObj;
                            return true;
                        }

                        type = PdfTokenType.NullObj;
                        throw new ApplicationException("Unknown token");
                    case (byte) '(':
                        type = PdfTokenType.StringObj;
                        if (StringParser.AdvancePastString(ref reader))
                        {
                            return true;
                        }

                        return false;
                    case (byte) '<':
                        if (reader.Remaining < 2)
                        {
                            if (isCompleted) { throw new ApplicationException("Unknown token"); }
                            return false;
                        }

                        if (reader.IsNext(NestedParser.startDict, true))
                        {
                            type = PdfTokenType.DictionaryStart;
                            return true;
                        }
                        type = PdfTokenType.StringObj;
                        return StringParser.AdvancePastString(ref reader);
                    case (byte) '/':
                        reader.TryRead(out _);
                        type = PdfTokenType.NameObj;
                        if (!reader.TryAdvanceToAny(NameParser.nameTerminators, false))
                        {
                            if (isCompleted)
                            {
                                reader.Advance(reader.Remaining);
                                return true;
                            }
                            reader.Rewind(1);
                            return false;
                        }
                        return true;
                    case (byte) '[':
                        reader.TryRead(out _);
                        type = PdfTokenType.ArrayStart;
                        return true;
                    case (byte) '-':
                    case (byte) '+':
                    case (byte) '.':
                    case (byte) '0':
                    case (byte) '1':
                    case (byte) '2':
                    case (byte) '3':
                    case (byte) '4':
                    case (byte) '5':
                    case (byte) '6':
                    case (byte) '7':
                    case (byte) '8':
                    case (byte) '9':
                        type = PdfTokenType.NumericObj;
                        reader.AdvancePastAny(CommonUtil.numeric);
                        if (reader.End && isCompleted)
                        {
                            return true;
                        } else if (reader.End)
                        {
                            return false;
                        }
                        return true;
                    case (byte) 'R':
                        reader.TryRead(out _);
                        type = PdfTokenType.IndirectRef;
                        return true;
                    case (byte) '>':
                        if (reader.Remaining < 2)
                        {
                            if (isCompleted)
                            {
                                throw new ApplicationException($"Bad token found '>'");
                            }
                            return false;
                        }
                        if (reader.IsNext(NestedParser.endDict, true))
                        {
                            type = PdfTokenType.DictionaryEnd;
                            return true;
                        }
                        throw new ApplicationException($"Bad token found '>'");
                    case (byte) ']':
                        reader.TryRead(out _);
                        type = PdfTokenType.ArrayEnd;
                        return true;
                    case (byte) 's':
                        type = PdfTokenType.StartStream;
                        if (reader.Remaining < 6)
                        {
                            if (isCompleted)
                            {
                                throw new ApplicationException($"Unknown object start: {(char) b}");
                            }
                            return false;
                        }
                        if (reader.IsNext(IndirectSequences.stream, true))
                        {
                            if (!reader.ReadRequiredByte(isCompleted, out var nxt))
                            {
                                reader.Rewind(6);
                                return false;
                            }
                            if (nxt == (byte) '\n')
                            {
                                return true;
                            }

                            if (nxt != (byte) '\r')
                            {
                                throw new ApplicationException($"Stream not followed by \\r\\n or \\n.");
                            }

                            if (!reader.ReadRequiredByte(isCompleted, out nxt))
                            {
                                reader.Rewind(7);
                                return false;
                            }

                            if (nxt != (byte) '\n')
                            {
                                throw new ApplicationException($"Stream not followed by \\r\\n or \\n.");
                            }

                            return true;
                        }
                        throw new ApplicationException($"Unknown object start: {(char) b}");
                    case (byte) 'e':
                        if (reader.Remaining < 6)
                        {
                            if (isCompleted) { throw new ApplicationException("Unknown token"); }
                            return false;
                        }

                        if (reader.IsNext(IndirectSequences.endobj, true))
                        {
                            type = PdfTokenType.EndObj;
                            return true;
                        }

                        if (reader.Remaining < 9)
                        {
                            if (isCompleted) { throw new ApplicationException("Unknown token"); }
                            return false;
                        }

                        if (reader.IsNext(IndirectSequences.endstream, true))
                        {
                            type = PdfTokenType.EndStream;
                            return true;
                        }
                        throw new ApplicationException($"Unknown object start: {(char) b}");
                    case (byte) 'o':
                        if (reader.Remaining < 3)
                        {
                            if (isCompleted) { throw new ApplicationException("Unknown token"); }
                            return false;
                        }

                        if (reader.IsNext(IndirectSequences.obj, true))
                        {
                            type = PdfTokenType.EndObj;
                            return true;
                        }
                        
                        throw new ApplicationException($"Unknown object start: {(char) b}");
                    default:
                        throw new ApplicationException($"Unknown object start: {(char) b}");
                }


            }
            finally
            {
                length = (int)(reader.Consumed - start);
            }
        }

        public static bool ReadRequiredByte(this ref SequenceReader<byte> reader, bool isCompleted, out byte value)
        {
            if (!reader.TryRead(out value))
            {
                if (isCompleted)
                {
                    throw new ApplicationException($"Unexpected token end.");
                }

                return false;
            }

            return true;
        }
        public static int ReadNextToken(ReadOnlySpan<byte> bytes, out PdfTokenType type, int startAt, out int length)
        {
            ReadOnlySpan<byte> buffer = bytes;
            type = PdfTokenType.NullObj;
            
            for (var i = startAt; i < buffer.Length; i++)
            {
                byte b = buffer[i];
                if (b == (byte)'%')
                {
                    // comments
                    var eol = buffer.Slice(i).IndexOfAny((byte)'\r', (byte)'\n');
                    if (eol == -1)
                    {
                        length = -1;
                        return -1;
                    }
                    i += eol - 1;
                    continue;
                }
                if (CommonUtil.IsWhiteSpace(b))
                {
                    continue;
                }
                switch (b)
                {
                    case (byte)'t':
                        if (buffer.Length > i + 2)
                        {
                            if (buffer[i + 1] == (byte)'r' && buffer[i + 2] == (byte)'u')
                            {
                                type = PdfTokenType.BooleanObj;
                                length = 4;
                                return i;
                            }
                            else if (buffer[i + 1] == (byte)'r' && buffer[i + 2] == (byte)'a')
                            {
                                type = PdfTokenType.Trailer;
                                length = 7;
                                return i;
                            }
                            else
                            {
                                throw new ApplicationException("Unknown token");
                            }
                        }
                        else
                        {
                            length = -1;
                            return -1;
                        }
                    case (byte)'f':
                        type = PdfTokenType.BooleanObj;
                        length = 5;
                        return i;
                    case (byte)'n':
                        type = PdfTokenType.NullObj;
                        length = 4;
                        return i;
                    case (byte) '(':
                        type = PdfTokenType.StringObj;
                        {
                            var read = StringParser.GetString(buffer.Slice(i), out var stringData);
                            if (!read)
                            {
                                length = -1;
                                return -1;
                            }
                            length = stringData.Length;
                        }

                        
                        return i;
                    case (byte)'<':
                        if (buffer.Length > i + 1)
                        {
                            if (buffer[i + 1] == (byte)'<')
                            {
                                type = PdfTokenType.DictionaryStart;
                                length = 2;
                                return i;
                            }
                            else
                            {
                                type = PdfTokenType.StringObj;
                                {
                                    var read = StringParser.GetString(buffer.Slice(i), out var stringData);
                                    if (!read)
                                    {
                                        length = -1;
                                        return -1;
                                    }
                                    length = stringData.Length;
                                }
                                return i;
                            }
                        }
                        else
                        {
                            length = -1;
                            return -1;
                        }
                    case (byte)'/':
                        type = PdfTokenType.NameObj;
                        length = NameParser.CountNameBytes(buffer, i);
                        return length == -1 ? -1 : i;
                    case (byte)'[':
                        type = PdfTokenType.ArrayStart;
                        length = 1;
                        return i;
                    case (byte)'-':
                    case (byte)'+':
                    case (byte)'.':
                    case (byte)'0':
                    case (byte)'1':
                    case (byte)'2':
                    case (byte)'3':
                    case (byte)'4':
                    case (byte)'5':
                    case (byte)'6':
                    case (byte)'7':
                    case (byte)'8':
                    case (byte)'9':
                        type = PdfTokenType.NumericObj;
                        length = NumberParser.CountNumberBytes(buffer.Slice(i));
                        return i;
                    case (byte)'R':
                        type = PdfTokenType.IndirectRef;
                        length = 1;
                        return i;
                    case (byte)'>':
                        if (buffer.Length > i + 1)
                        {
                            if (buffer[i + 1] == (byte)'>')
                            {
                                type = PdfTokenType.DictionaryEnd;
                                length = 2;
                                return i;
                            }
                            else
                            {
                                throw new ApplicationException($"Bad token found '>'");
                            }
                        }
                        else
                        {
                            length = -1;
                            return -1;
                        }
                    case (byte)']':
                        type = PdfTokenType.ArrayEnd;
                        length = 1;
                        return i;
                    default:
                        throw new ApplicationException($"Unknown object start: {(char)buffer[i]}");
                }
            }

            length = -1;
            return -1;
        }
    }
}
