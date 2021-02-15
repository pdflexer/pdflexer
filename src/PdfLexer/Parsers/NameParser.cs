using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;


namespace PdfLexer.Parsers
{
    public class NameParser : IParser<PdfName>
    {
        internal static byte[] NameTerminators = new byte[16] { 0x00, 0x09, 0x0A, 0x0C, 0x0D, 0x20,
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

        public PdfName Parse(in ReadOnlySequence<byte> sequence)
        {
            // TODO optimize
            return Parse(sequence.ToArray());
        }

        public PdfName Parse(in ReadOnlySequence<byte> sequence, long start, int length)
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


        private static byte[] delimiters = new byte[10] {
            (byte)'(', (byte)')', (byte)'<', (byte)'>', (byte)'[', (byte)']', (byte)'{', (byte)'}', (byte)'/', (byte)'%', };
        
    }
}