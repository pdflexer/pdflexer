using System;
using System.Text;
using PdfLexer.Parsers;
using PdfLexer.Parsers.Structure;

namespace PdfLexer.Lexing
{
    public class PdfSpanLexer_Previous
    {
        /// <summary>
        /// Attempts to tokenize and determine type of the next token in the provided buffer.
        /// Note: If reaches end of span, while parsing a token, will assume end of token if valid.
        /// </summary>
        /// <param name="bytes">Data to read.</param>
        /// <param name="type">Type of token found.</param>
        /// <param name="length">Number of bytes in next token.</param>
        /// <returns>Start position of next token, or -1 if unsuccessful.</returns>
        public static int TryReadNextToken(ReadOnlySpan<byte> bytes, out PdfTokenType type, out int length) =>
            TryReadNextToken(bytes, out type, 0, out length);
        
        public static int TryReadNextToken(ReadOnlySpan<byte> bytes, out PdfTokenType type, int startAt, out int length)
        {
            ReadOnlySpan<byte> buffer = bytes;
            type = PdfTokenType.NullObj;
            var i = startAt;

            CommonUtil.SkipWhiteSpace(buffer, ref i);

            byte b = buffer[i];
            while (b == (byte)'%')
            {
                // comments
                var eol = buffer.Slice(i).IndexOfAny((byte)'\r', (byte)'\n');
                if (eol == -1)
                {
                    length = -1;
                    return -1;
                }
                i += eol;
                
                CommonUtil.SkipWhiteSpace(buffer, ref i);
                b = buffer[i];
            }

            switch (b)
            {
                case (byte) 't':

                    if (!(buffer.Length > i + 3))
                    {
                        length = -1;
                        return -1;
                    }

                    if (buffer.Slice(i).StartsWith(BoolParser.truebytes))
                    {
                        type = PdfTokenType.BooleanObj;
                        length = 4;
                        return i;
                    }

                    if (!(buffer.Length > i + 6))
                    {
                        length = -1;
                        return -1;
                    }

                    if (buffer.Slice(i).StartsWith(XRefParser.trailer))
                    {
                        type = PdfTokenType.Trailer;
                        length = 7;
                        return i;
                    }

                    throw UnknownTokenError(buffer);
                case (byte) 'f':
                    if (!(buffer.Length > i + 4))
                    {
                        length = -1;
                        return -1;
                    }

                    if (buffer.Slice(i).StartsWith(BoolParser.falsebytes))
                    {
                        type = PdfTokenType.BooleanObj;
                        length = 5;
                        return i;
                    }

                    throw UnknownTokenError(buffer);
                case (byte) 'n':
                    if (!(buffer.Length > i + 3))
                    {
                        length = -1;
                        return -1;
                    }

                    if (buffer.Slice(i).StartsWith(PdfNull.NullBytes))
                    {
                        type = PdfTokenType.NullObj;
                        length = 4;
                        return i;
                    }

                    throw UnknownTokenError(buffer);
                case (byte) '(':
                {
                    var se = i;
                    if (!StringParser.AdvancePastStringLiteral(buffer, ref se))
                    {
                        throw CommonUtil.DisplayDataErrorException(buffer, i, $"String literal end not found.");
                    }
                    type = PdfTokenType.StringStart;
                    length = se-i;
                    return i;
                }

                case (byte)'<':
                {
                    if (buffer.Length > i + 1 && buffer[i + 1] == (byte) '<')
                    {
                        type = PdfTokenType.DictionaryStart;
                        length = 2;
                        return i;
                    }

                    var se = i;
                    if (!StringParser.AdvancePastHexString(buffer, ref se))
                    {
                        throw CommonUtil.DisplayDataErrorException(buffer, i, $"Hex string end not found.");
                    }
                    type = PdfTokenType.StringStart;
                    length = se-i;
                    return i;
                }
                case (byte)'/':
                    var ne = i;
                    ne++;
                    type = PdfTokenType.NameObj;
                    NameParser.SkipName(buffer, ref ne);
                    length = ne-i;
                    return i;
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
                    var nume = i;
                    NumberParser.SkipNumber(buffer, ref nume, out bool isDecimal);
                    type = isDecimal ? PdfTokenType.DecimalObj : PdfTokenType.NumericObj;
                    length = nume-i;
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
                            throw UnknownTokenError(buffer);
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
                case (byte) 'e':

                    if (!(buffer.Length > i + 5))
                    {
                        length = -1;
                        return -1;
                    }

                    if (buffer.Slice(i).StartsWith(IndirectSequences.endobj))
                    {
                        type = PdfTokenType.EndObj;
                        length = 6;
                        return i;
                    }

                    if (!(buffer.Length > i + 8))
                    {
                        length = -1;
                        return -1;
                    }

                    if (buffer.Slice(i).StartsWith(IndirectSequences.endstream))
                    {
                        type = PdfTokenType.EndStream;
                        length = 9;
                        return i;
                    }

                    throw UnknownTokenError(buffer);
                case (byte) 's':
                    type = PdfTokenType.StartStream;
                    if (!(buffer.Length > i + 5))
                    {
                        length = -1;
                        return -1;
                    }

                    if (buffer.Slice(i).StartsWith(IndirectSequences.stream))
                    {
                        if (!(buffer.Length > i + 6))
                        {
                            length = -1;
                            return -1;
                        }

                        type = PdfTokenType.StartStream;
                        var nxt = buffer[i + 6];
                        if (nxt == (byte) '\n')
                        {
                            length = 7;
                            return i;
                        }

                        
                        if (nxt != (byte) '\r')
                        {
                            throw new ApplicationException($"Stream not followed by \\r\\n or \\n.");
                        }

                        if (!(buffer.Length > i + 7))
                        {
                            length = -1;
                            return -1;
                        }

                        nxt = buffer[i + 7];
                        if (nxt != (byte) '\n')
                        {
                            throw new ApplicationException($"Stream not followed by \\r\\n or \\n.");
                        }

                        length = 8;
                        return i;
                    }

                    throw UnknownTokenError(buffer);
                case (byte) 'x':
                    if (!(buffer.Length > i + 3))
                    {
                        length = -1;
                        return -1;
                    }

                    if (buffer.Slice(i).StartsWith(XRefParser.xref))
                    {
                        type = PdfTokenType.Xref;
                        length = 4;
                        return i;
                    }

                    throw UnknownTokenError(buffer);
                case (byte) 'o':
                    if (!(buffer.Length > i + 2))
                    {
                        length = -1;
                        return -1;
                    }

                    if (buffer.Slice(i).StartsWith(IndirectSequences.obj))
                    {
                        type = PdfTokenType.StartObj;
                        length = 3;
                        return i;
                    }

                    throw UnknownTokenError(buffer);
                default:
                    throw CommonUtil.DisplayDataErrorException(buffer, i, "Unknown token start");
            }

            Exception UnknownTokenError(ReadOnlySpan<byte> data)
            {
                var count = data.Length > i + 10 ? 10 : data.Length - i;
                return new ApplicationException(
                    "Unknown token: '" + Encoding.ASCII.GetString(data.Slice(i, count)) + "'");
            }
        }
    }
}
