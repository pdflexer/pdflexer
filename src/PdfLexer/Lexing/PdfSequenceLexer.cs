using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading.Tasks;
using PdfLexer.Parsers;
using PdfLexer.Parsers.Nested;
using PdfLexer.Parsers.Structure;

namespace PdfLexer.Lexing
{
    public static class PdfSequenceLexer
    {
        private static List<KeyValuePair<PdfTokenType, byte[]>> sTokens = new List<KeyValuePair<PdfTokenType, byte[]>>
        {
            new KeyValuePair<PdfTokenType, byte[]> (PdfTokenType.StartStream, IndirectSequences.stream),
            new KeyValuePair<PdfTokenType, byte[]> (PdfTokenType.StartXref, IndirectSequences.strartxref)
        };

        private static byte[] eolChars = new byte[] { (byte)'\r', (byte)'\n' };
        /// <summary>
        /// Attempts to tokenize and determine type of the next token / object from the reader.
        /// NOTE: String objects are considered one large token by the lexer.
        /// NOTE: Indirect object references are considered three tokens.
        /// </summary>
        /// <param name="reader">SequenceReader to read.</param>
        /// <param name="isCompleted">Signifies if the underlying ReadOnlySequence has read all available data.</param>
        /// <param name="type">Type of token found.</param>
        /// <param name="start">SequencePosition of token start.</param>
        /// <returns>If token was successfully lexed.</returns>
        public static bool TryReadNextToken(this ref SequenceReader<byte> reader, bool isCompleted, out PdfTokenType type, out SequencePosition start)
        {
            reader.AdvancePastAny(CommonUtil.WhiteSpaces);

            type = PdfTokenType.NullObj;
            if (!reader.TryPeek(out byte b))
            {
                start = reader.Position;
                return false;
            }

            // TODO support reading comments
            while (b == (byte)'%')
            {
                // comments
                if (!reader.TryAdvanceToAny(eolChars, true))
                {
                    start = reader.Position;
                    return false;
                }
                reader.AdvancePastAny(CommonUtil.WhiteSpaces);

                if (!reader.TryPeek(out b))
                {
                    start = reader.Position;
                    return false;
                }
            }

            start = reader.Position;

            switch (b)
            {
                case (byte)'t':
                    if (IsNext(ref reader, BoolParser.truebytes, true))
                    {
                        type = PdfTokenType.BooleanObj;
                        return true;
                    }

                    if (IsNext(ref reader, XRefParser.trailer, true))
                    {
                        type = PdfTokenType.Trailer;
                        return true;
                    }

                    return false;
                case (byte)'f':
                    if (IsNext(ref reader, BoolParser.falsebytes, true))
                    {
                        type = PdfTokenType.BooleanObj;
                        return true;
                    }

                    return false;
                case (byte)'n':
                    if (IsNext(ref reader, PdfNull.NullBytes, true))
                    {
                        type = PdfTokenType.NullObj;
                        return true;
                    }

                    return false;
                case (byte)'(':
                    type = PdfTokenType.StringStart;
                    return StringParser.TryAdvancePastString(ref reader);
                case (byte)'<':
                    if (IsNext(ref reader, PdfDictionary.start, true))
                    {
                        type = PdfTokenType.DictionaryStart;
                        return true;
                    }

                    type = PdfTokenType.StringStart;
                    return StringParser.TryAdvancePastString(ref reader);
                case (byte)'/':
                    reader.TryRead(out _);
                    type = PdfTokenType.NameObj;
                    if (!reader.TryAdvanceToAny(NameParser.NameTerminators, false))
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
                case (byte)'[':
                    {
                        type = PdfTokenType.ArrayStart;
                        reader.Advance(1);
                        return true;
                    }
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
                    reader.AdvancePastAny(CommonUtil.ints);
                    if (reader.End && isCompleted)
                    {
                        return true;
                    }
                    else if (reader.End)
                    {
                        return false;
                    }
                    if (!reader.TryPeek(out var dg))
                    {
                        return false;
                    }
                    if (dg != (byte)'.')
                    {
                        return true;
                    }
                    reader.Advance(1);
                    type = PdfTokenType.DecimalObj;
                    reader.AdvancePastAny(CommonUtil.ints);
                    if (reader.End && isCompleted)
                    {
                        return true;
                    }
                    else if (reader.End)
                    {
                        return false;
                    }

                    return true;
                case (byte)'R':
                    reader.TryRead(out _);
                    type = PdfTokenType.IndirectRef;
                    return true;
                case (byte)'>':
                    if (IsNext(ref reader, PdfDictionary.end, true))
                    {
                        type = PdfTokenType.DictionaryEnd;
                        return true;
                    }
                    return false;
                case (byte)']':
                    reader.TryRead(out _);
                    type = PdfTokenType.ArrayEnd;
                    return true;
                case (byte)'s':
                    if (IsNext(ref reader, IndirectSequences.stream, true))
                    {
                        type = PdfTokenType.StartStream;
                        if (!reader.ReadRequiredByte(isCompleted, out var nxt))
                        {
                            reader.Rewind(6);
                            return false;
                        }
                        if (nxt == (byte)'\n')
                        {
                            return true;
                        }

                        if (nxt != (byte)'\r')
                        {
                            throw CommonUtil.DisplayDataErrorException(ref reader, "Stream not followed by \\r\\n or \\n.");
                        }

                        if (!reader.ReadRequiredByte(isCompleted, out nxt))
                        {
                            reader.Rewind(7);
                            return false;
                        }

                        if (nxt != (byte)'\n')
                        {
                            throw CommonUtil.DisplayDataErrorException(ref reader, "Stream not followed by \\r\\n or \\n.");
                        }

                        return true;
                    }

                    if (IsNext(ref reader, IndirectSequences.strartxref, true))
                    {
                        type = PdfTokenType.StartXref;
                        return true;
                    }
                    return false;
                case (byte)'e':
                    if (IsNext(ref reader, IndirectSequences.endobj, true))
                    {
                        type = PdfTokenType.EndObj;
                        return true;
                    }

                    if (IsNext(ref reader, IndirectSequences.endstream, true))
                    {
                        type = PdfTokenType.EndStream;
                        return true;
                    }
                    return false;
                case (byte)'x':
                    if (IsNext(ref reader, XRefParser.xref, true))
                    {
                        type = PdfTokenType.Xref;
                        return true;
                    }

                    return false;
                case (byte)'o':
                    if (IsNext(ref reader, IndirectSequences.obj, true))
                    {
                        type = PdfTokenType.StartObj;
                        return true;
                    }

                    return false;
                default:
                    throw CommonUtil.DisplayDataErrorException(ref reader, "Unknown / invalid token");
            }

            bool IsNext(ref SequenceReader<byte> rdr, ReadOnlySpan<byte> data, bool advancePast = false)
            {
                if (rdr.Remaining < data.Length)
                {
                    if (isCompleted) { throw CommonUtil.DisplayDataErrorException(ref rdr, "Unknown / invalid token"); }
                    return false;
                }
                return rdr.IsNext(data, advancePast);
            }
        }
        internal static bool ReadRequiredByte(this ref SequenceReader<byte> reader, bool isCompleted, out byte value)
        {
            if (!reader.TryRead(out value))
            {
                if (isCompleted)
                {
                    throw CommonUtil.DisplayDataErrorException(ref reader, "Unknown / invalid token");
                }

                return false;
            }

            return true;
        }

    }
}
