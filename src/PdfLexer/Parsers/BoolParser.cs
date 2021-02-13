using System;
using System.Buffers;
using System.Diagnostics;
using System.Text;

namespace PdfLexer.Parsers
{
    public class BoolParser : IParser<PdfBoolean>
    {
        public static byte[] truebytes = new byte[4] { (byte)'t', (byte)'r', (byte)'u', (byte)'e' };
        public static byte[] falsebytes = new byte[5] { (byte)'f', (byte)'a', (byte)'l', (byte)'s', (byte)'e' };

        public PdfBoolean Parse(ReadOnlySpan<byte> buffer)
        {
            var fb = buffer[0];
            if (fb == (byte)'t')
            {
                Debug.Assert(buffer.Length == 4, "True bool was 4 bytes.");
                return PdfBoolean.True;
            }
            Debug.Assert(buffer.Length == 5, "False bool was 5 bytes.");
            return PdfBoolean.False;
        }

        public PdfBoolean Parse(ReadOnlySpan<byte> buffer, int start, int length)
        {
            var fb = buffer[start];
            if (fb == (byte)'t')
            {
                Debug.Assert(length == 4, "True bool was 4 bytes.");
                return PdfBoolean.True;
            }
            Debug.Assert(length == 5, "False bool was 5 bytes.");
            return PdfBoolean.False;
        }

        public PdfBoolean Parse(ref ReadOnlySequence<byte> sequence)
        {
            var fb = sequence.FirstSpan[0];
            if (fb == (byte)'t')
            {
                Debug.Assert(sequence.Length == 4, "True bool was 4 bytes.");
                return PdfBoolean.True;
            }
            Debug.Assert(sequence.Length == 5, "False bool was 5 bytes.");
            return PdfBoolean.False;
        }

        public PdfBoolean Parse(ref ReadOnlySequence<byte> sequence, long start, int length)
        {
            // can optimize
            var slice = sequence.Slice(start, length);
            return Parse(ref slice);
        }
    }
}