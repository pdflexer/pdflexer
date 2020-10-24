using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer.Objects.Parsers
{
    public class NameParser
    {
        private static byte[] nameTerminators = new byte[16] { 0x00, 0x09, 0x0A, 0x0C, 0x0D, 0x20,
            (byte)'(', (byte)')', (byte)'<', (byte)'>', (byte)'[', (byte)']', (byte)'{', (byte)'}', (byte)'/', (byte)'%' };
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