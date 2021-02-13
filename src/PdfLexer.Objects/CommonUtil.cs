
using System;
using System.Runtime.CompilerServices;
using PdfLexer.Objects.Parsers;

namespace PdfLexer.Objects
{

    public class CommonUtil
    {
        internal static byte[] whiteSpaces = new byte[6] { 0x00, 0x09, 0x0A, 0x0C, 0x0D, 0x20 };
        public static byte[] numeric = new byte[13] { (byte)'0', (byte)'1', (byte)'2', (byte)'3', (byte)'4', (byte)'5', (byte)'6',
        (byte)'7', (byte)'8', (byte)'9', (byte)'.', (byte)'-', (byte)'+'};
        private static byte[] ints = new byte[10] { (byte)'0', (byte)'1', (byte)'2', (byte)'3', (byte)'4', (byte)'5', (byte)'6',
        (byte)'7', (byte)'8', (byte)'9'};
        // special characters that allow us to stop scanning and confirm the token is a numeric
        private static byte[] numTers = new byte[11] { (byte)'.',
            (byte)'(', (byte)')', (byte)'<', (byte)'>', (byte)'[', (byte)']', (byte)'{', (byte)'}', (byte)'/', (byte)'%' };
        public static bool IsWhiteSpace(ReadOnlySpan<char> chars, int location)
        {
            var c = chars[location];
            return c == 0x00
                   || c == 0x09
                   || c == 0x0A
                   || c == 0x0C
                   || c == 0x0D
                   || c == 0x20;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWhiteSpace(ReadOnlySpan<byte> bytes, int location)
        {
            var b = bytes[location];
            return b == 0x00
                   || b == 0x09
                   || b == 0x0A
                   || b == 0x0C
                   || b == 0x0D
                   || b == 0x20;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWhiteSpace(byte item)
        {
            return item == 0x00 ||
                   item == 0x09 ||
                   item == 0x0A ||
                   item == 0x0C ||
                   item == 0x0D ||
                   item == 0x20;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SkipWhiteSpaces(ReadOnlySpan<byte> bytes, int location)
        {
            ReadOnlySpan<byte> localBuffer = bytes;
            for (var i = location; i < bytes.Length; i++)
            {
                byte val = localBuffer[i];
                if (val != 0x00 &&
                    val != 0x09 &&
                    val != 0x0A &&
                    val != 0x0C &&
                    val != 0x0D &&
                    val != 0x20)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Scans the byte span for the next token and determines the PdfTokenType. If 
        /// no token is found, a byte position of -1 is returned.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="type"></param>
        /// <returns>Byte position for start of object.</returns>
        public static int FindNextToken(ReadOnlySpan<byte> bytes, out PdfTokenType type, int startAt = 0)
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
                        return -1;
                    }
                    i += eol - 1;
                    continue;
                }
                if (IsWhiteSpace(b))
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
                                return i;
                            }
                            else if (buffer[i + 1] == (byte)'r' && buffer[i + 2] == (byte)'a')
                            {
                                type = PdfTokenType.Trailer;
                                return i;
                            }
                            else
                            {
                                throw new ApplicationException("Unknown token");
                            }
                        }
                        else
                        {
                            return -1;
                        }
                    case (byte)'f':
                        type = PdfTokenType.BooleanObj;
                        return i;
                    case (byte)'n':
                        type = PdfTokenType.NullObj;
                        return i;
                    case (byte)'(':
                        type = PdfTokenType.StringObj;
                        return i;
                    case (byte)'<':
                        if (buffer.Length > i + 1)
                        {
                            if (buffer[i + 1] == (byte)'<')
                            {
                                type = PdfTokenType.DictionaryStart;
                                return i;
                            }
                            else
                            {
                                type = PdfTokenType.StringObj;
                                return i;
                            }
                        }
                        else
                        {
                            return -1;
                        }
                    case (byte)'/':
                        type = PdfTokenType.NameObj;
                        return i;
                    case (byte)'[':
                        type = PdfTokenType.ArrayStart;
                        return i;
                    case (byte)'-':
                    case (byte)'+':
                    case (byte)'.':
                        type = PdfTokenType.NumericObj;
                        return i;
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
                        // TODO: need to look into optimizing this
                        int state = 0;
                        bool inWhiteSpace = false;
                        for (var j = i + 1; j < buffer.Length; j++)
                        {
                            byte bw = buffer[j];
                            if (IsWhiteSpace(bw))
                            {
                                if (!inWhiteSpace)
                                {
                                    state++;
                                    inWhiteSpace = true;
                                }
                                continue;
                            }
                            else
                            {
                                inWhiteSpace = false;
                            }

                            if (state == 0)
                            {
                                if (Array.IndexOf(numTers, bw) > -1)
                                {
                                    type = PdfTokenType.NumericObj;
                                    return i;
                                }
                            }
                            else if (state == 1)
                            {
                                if (Array.IndexOf(ints, bw) == -1)
                                {
                                    // non int field, can't be indirect object
                                    type = PdfTokenType.NumericObj;
                                    return i;
                                }
                            }
                            else if (state == 2)
                            {
                                if (bw == (byte)'R')
                                {
                                    type = PdfTokenType.IndirectRef;
                                    return i;
                                }
                                else
                                {
                                    type = PdfTokenType.NumericObj;
                                    return i;
                                }
                            }
                        }
                        return -1;
                    case (byte)'>':
                        if (buffer.Length > i + 1)
                        {
                            if (buffer[i + 1] == (byte)'>')
                            {
                                type = PdfTokenType.DictionaryEnd;
                                return i;
                            }
                            else
                            {
                                throw new ApplicationException($"Bad token found '>'");
                            }
                        }
                        else
                        {
                            return -1;
                        }
                    case (byte)']':
                        type = PdfTokenType.ArrayEnd;
                        return i;
                    default:
                        throw new ApplicationException($"Unknown object start: {(char)buffer[i]}");
                }
            }
            return -1;
        }


        
    }
}