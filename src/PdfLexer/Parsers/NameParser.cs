using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;


namespace PdfLexer.Parsers
{
    public class NameParser : IParser<PdfName>
    {
        public static byte[] nameTerminators = new byte[16] { 0x00, 0x09, 0x0A, 0x0C, 0x0D, 0x20,
            (byte)'(', (byte)')', (byte)'<', (byte)'>', (byte)'[', (byte)']', (byte)'{', (byte)'}', (byte)'/', (byte)'%' };


        public PdfName Parse(ReadOnlySpan<byte> buffer)
        {
            if (buffer.IndexOf((byte) '#') == -1)
            {
                return ParseFastNoHex(buffer);
            }

            return ParseWithHex(buffer, 0, buffer.Length);
        }

        public PdfName Parse(ReadOnlySpan<byte> buffer, int start, int length)
        {
            var part = buffer.Slice(start, length);
            if (part.IndexOf((byte) '#') == -1)
            {
                return ParseFastNoHex(part);
            }

            return ParseWithHex(buffer, start, length);
        }

        public PdfName Parse(ref ReadOnlySequence<byte> sequence)
        {
            // TODO optimize
            return Parse(sequence.ToArray());
        }

        public PdfName Parse(ref ReadOnlySequence<byte> sequence, long start, int length)
        {
            // TODO optimize
            return Parse(sequence.Slice(start, length).ToArray());
        }

        private PdfName ParseFastNoHex(ReadOnlySpan<byte> buffer)
        {
            // TODO lookup for commonly used names
            Span<char> chars = stackalloc char[buffer.Length];
            Encoding.ASCII.GetChars(buffer, chars);
            return new PdfName(new String(chars));
        }

        private PdfName ParseWithHex(ReadOnlySpan<byte> buffer, int start, int length)
        {
            var ci = 0;
            Span<char> chars = stackalloc char[length];
            for (var i = start; i < start + length; i++)
            {
                var c = buffer[i];
                if (c == (byte) '#')
                {
                    var hex = "" + (char) buffer[i + 1] + (char) buffer[i + 2];
                    var value = (char)Convert.ToInt32(hex, 16); // ugh
                    i += 2;
                    chars[ci++] = value;
                }
                else
                {
                    chars[ci++] = (char)c;
                }
            }
            return new PdfName(new String(chars.Slice(0,ci)));
        }

#if NET50
        public static string ParseName(in ReadOnlySequence<byte> data)
        {
            return Encoding.ASCII.GetString(in data);

        }
#endif
        public static int CountNameBytes(ReadOnlySpan<byte> bytes)
        {
            if (bytes[0] != '/')
            {
                throw new ApplicationException("Invalid name. Does not start with /");
            }
            var end = bytes.Slice(1).IndexOfAny(nameTerminators);
            if (end != -1) { end++; }
            return end;
        }

        public static int CountNameBytes(ReadOnlySpan<byte> bytes, int offset)
        {
            if (bytes[offset] != (byte)'/')
            {
                throw new ApplicationException("Invalid name. Does not start with /");
            }
            var end = bytes.Slice(offset + 1).IndexOfAny(nameTerminators);
            if (end != -1) { end++; }
            return end;
        }
        public static bool GetNameBytes(ReadOnlySpan<byte> bytes, out ReadOnlySpan<byte> nameBytes)
        {
            var end = CountNameBytes(bytes);
            if (end == -1)
            {
                nameBytes = null;
                return false;
            }
            else
            {
                nameBytes = bytes.Slice(0, end);
                return true;
            }
        }

        private static byte[] delimiters = new byte[10] {
            (byte)'(', (byte)')', (byte)'<', (byte)'>', (byte)'[', (byte)']', (byte)'{', (byte)'}', (byte)'/', (byte)'%', };

        public static string ParseName(ReadOnlySpan<byte> bytes)
        {
            ParseName(bytes, out Span<char> result);
            return result.ToString();
        }

        public static int ParseName(ReadOnlySpan<byte> bytes, out Span<char> result)
        {
            if (bytes[0] != '/')
            {
                throw new ApplicationException("Invalid name. Does not start with /");
            }
            List<int> hexes = null;
            for (var i = 1; i < bytes.Length; i++)
            {
                if (Array.IndexOf(delimiters, bytes[i]) > -1)
                {
                    throw new ApplicationException("Invalid name. Contains delimiter");
                }
                if (bytes[i] == '#')
                {
                    if (hexes == null)
                    {
                        hexes = new List<int>();
                    }
                    hexes.Add(i);
                    i += 2;
                    continue;
                }
                if (CommonUtil.IsWhiteSpace(bytes[i]))
                {
                    result = ReplaceHex(bytes.Slice(1, i - 1), hexes);
                    return i;
                }
            }
            result = ReplaceHex(bytes.Slice(1), hexes);
            return bytes.Length;
        }

        private static Span<char> ReplaceHex(ReadOnlySpan<byte> raw, List<int> hexLocs)
        {
            var buffer = new Span<char>(new char[raw.Length]);
            Encoding.ASCII.GetChars(raw, buffer);
            if (hexLocs == null)
            {
                return buffer;
            }
            for (var i = hexLocs.Count - 1; i > -1; i--)
            {
                if (hexLocs[i] + 2 > raw.Length)
                {
                    throw new ApplicationException("Invalid name. Contains hex # without hex numbers.");
                }
                // TODO: evaluate performance
                var hex = buffer.Slice(hexLocs[i], 2);
                var value = (char)Convert.ToInt32(hex.ToString(), 16); // ugh
                buffer[hexLocs[i] - 1] = value;
                var rest = buffer.Slice(hexLocs[i] + 2);
                rest.CopyTo(buffer.Slice(hexLocs[i]));
            }
            return buffer.Slice(0, raw.Length - hexLocs.Count * 2);
        }


    }
}