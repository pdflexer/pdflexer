﻿using System;
using System.Text;
using PdfLexer.Parsers;

namespace PdfLexer.Lexing
{
    public class PdfSpanLexer
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
                    {
                    var current = buffer.Slice(i);
                    if (current.StartsWith(BoolParser.truebytes))
                    {
                        length = 4;
                        type = PdfTokenType.BooleanObj;
                        return i;
                    }

                    if (current.StartsWith(XRefParser.trailer))
                    {
                        length = 7;
                        type = PdfTokenType.Trailer;
                        return i;
                    }

                    break;
                    }
                case (byte) 'f':
                    if (buffer.Slice(i).StartsWith(BoolParser.falsebytes))
                    {
                        length = 5;
                        type = PdfTokenType.BooleanObj;
                        return i;
                    }

                    break;
                case (byte) 'n':
                    if (buffer.Slice(i).StartsWith(PdfNull.NullBytes))
                    {
                        length = 4;
                        type = PdfTokenType.NullObj;
                        return i;
                    }

                    break;
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
                    if (buffer.Length > i + 1 && buffer[i + 1] == (byte)'>')
                    {
                        type = PdfTokenType.DictionaryEnd;
                        length = 2;
                        return i;
                    }
                    type = PdfTokenType.EndString;
                    length = 1;
                    return i;
                case (byte)']':
                    type = PdfTokenType.ArrayEnd;
                    length = 1;
                    return i;
                case (byte) 'e':
                    {
                    var current = buffer.Slice(i);
                    if (current.StartsWith(IndirectSequences.endobj))
                    {
                        type = PdfTokenType.EndObj;
                        length = 6;
                        return i;
                    }

                    if (current.StartsWith(IndirectSequences.endstream))
                    {
                        type = PdfTokenType.EndStream;
                        length = 9;
                        return i;
                    }
                    }
                    break;
                case (byte) 's':
                    if (buffer.Slice(i).StartsWith(IndirectSequences.stream))
                    {
                        if (!(buffer.Length > i + 6))
                        {
                            throw CommonUtil.DisplayDataErrorException(buffer, i, $"Stream not followed by \\r\\n or \\n.");
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
                            throw CommonUtil.DisplayDataErrorException(buffer, i, $"Stream not followed by \\r\\n or \\n.");
                        }

                        if (!(buffer.Length > i + 7))
                        {
                            throw CommonUtil.DisplayDataErrorException(buffer, i, $"Stream not followed by \\r\\n or \\n.");
                        }

                        nxt = buffer[i + 7];
                        if (nxt != (byte) '\n')
                        {
                            throw CommonUtil.DisplayDataErrorException(buffer, i, $"Stream not followed by \\r\\n or \\n.");
                        }

                        length = 8;
                        return i;
                    }
                    break;
                case (byte) 'x':
                    if (buffer.Slice(i).StartsWith(XRefParser.xref))
                    {
                        length = 4;
                        type = PdfTokenType.Xref;
                        return i;
                    }
                    break;
                case (byte) 'o':
                    if (buffer.Slice(i).StartsWith(IndirectSequences.obj))
                    {
                        length = 3;
                        type = PdfTokenType.StartObj;
                        return i;
                    }
                    break;
                default:
                    break;
            }

            // Default tokenization
            ReadOnlySpan<byte> terms = CommonUtil.Terminators;
            var end = i;
            CommonUtil.ScanTokenEnd(buffer, ref end);
            length = end-i;
            type = PdfTokenType.Unknown;
            return i;
        }
    }
}
