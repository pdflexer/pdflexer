
using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;
using PdfLexer.Parsers;

[assembly:InternalsVisibleTo("PdfLexer.Tests")]

namespace PdfLexer
{

    public class CommonUtil
    {
        internal static byte[] eols = new byte[2] {(byte)'\r', (byte)'n' };
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

        public static Exception DisplayDataErrorException(ReadOnlySpan<byte> data, int i, string prefixInfo)
        {
            var count = data.Length > i + 25 ? 25 : data.Length - i;
            return new ApplicationException(prefixInfo + ": '" + Encoding.ASCII.GetString(data.Slice(i, count)) + "'");
        }

        public static Exception DisplayDataErrorException(ref SequenceReader<byte> reader, string prefixInfo)
        {
            var count = reader.Remaining > 25 ? 25 : reader.Remaining;
            return new ApplicationException(prefixInfo + ": '" + Encoding.ASCII.GetString(reader.Sequence.Slice(reader.Position, count).ToArray()) + "'");
        }

        public static Exception DisplayDataErrorException(ReadOnlySequence<byte> sequence, SequencePosition position, string prefixInfo)
        {
            var count = sequence.Length > 25 ? 25 : sequence.Length;
            
            return new ApplicationException(prefixInfo + ": '" + Encoding.ASCII.GetString(sequence.Slice(position, count).ToArray()) + "'");
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
        public static void SkipWhiteSpace(ReadOnlySpan<byte> bytes, ref int i)
        {
            ReadOnlySpan<byte> local = bytes;
            for (; i < local.Length; i++)
            {
                byte b = local[i];
                if (b == 0x00 ||
                    b == 0x09 ||
                    b == 0x0A ||
                    b == 0x0C ||
                    b == 0x0D ||
                    b == 0x20)
                {
                    continue;
                }

                return;
            }
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
    }
}