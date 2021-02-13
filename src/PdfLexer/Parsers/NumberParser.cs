using System;
using System.Buffers.Text;
using System.Diagnostics;
using System.Text;

namespace PdfLexer.Parsers
{
    public enum NumberType
    {
        Integer,
        Real
    }
    public class NumberParser
    {
        private static byte[] numberTerminators = new byte[16] { 0x00, 0x09, 0x0A, 0x0C, 0x0D, 0x20,
            (byte)'(', (byte)')', (byte)'<', (byte)'>', (byte)'[', (byte)']', (byte)'{', (byte)'}', (byte)'/', (byte)'%' };
        public static int CountNumberBytes(ReadOnlySpan<byte> bytes)
        {
            return bytes.IndexOfAny(numberTerminators);
        }

        public static int GetInt(ReadOnlySpan<byte> bytes, int offSet, out int value)
        {
            //Utf8Parser.TryParse(bytes, out int rs, out int bytesConsumed);
            // needs to be optimized if used widely
            value = 0;
            var data = bytes.Slice(offSet);
            var pos = data.IndexOfAny(numberTerminators);
            if (pos == -1)
            {
                return -1;
            }
            data = data.Slice(0, pos);
            var buffer = new Span<char>(new char[data.Length]);
            Encoding.ASCII.GetChars(data, buffer);
            value = int.Parse(buffer);
            return data.Length;
        }

        public static bool GetNumber(ReadOnlySpan<byte> bytes, out ReadOnlySpan<byte> numberBytes, out NumberType type)
        {
            type = NumberType.Integer;
            var pos = bytes.IndexOfAny(numberTerminators);
            if (pos == -1)
            {
                numberBytes = null;
                return false;
            }
            numberBytes = bytes.Slice(0, pos);
            return true;
            // for (var i=0;i<bytes.Length;i++)
            // {
            //     if (bytes[i] == '.')
            //     {
            //         type = NumberType.Real;
            //     } else if (Utils.IsWhiteSpace(bytes[i]))
            //     {
            //         numberBytes = bytes.Slice(0, i);
            //         return true;
            //     }
            // }
            // numberBytes = null;
            // return false;
        }

        public static float ParseReal(ReadOnlySpan<byte> bytes)
        {
            if (Utf8Parser.TryParse(bytes, out float value, out int consumed))
            {
                Debug.Assert(bytes.Length == consumed, "Float parsing read all bytes of span.");
                return value;
            }

            throw new ApplicationException("Failed parsing float from pdf: " + Encoding.ASCII.GetString(bytes));
        }

        public static float ParseReal(ReadOnlySpan<byte> bytes, Span<char> buffer)
        {
            Encoding.ASCII.GetChars(bytes, buffer);
            return float.Parse(buffer);
        }

        public static int ParseInt(ReadOnlySpan<byte> bytes)
        {
            var buffer = new Span<char>(new char[bytes.Length]);
            Encoding.ASCII.GetChars(bytes, buffer);
            return int.Parse(buffer);
        }

        public static int ParseInt(ReadOnlySpan<byte> bytes, Span<char> buffer)
        {
            Encoding.ASCII.GetChars(bytes, buffer);
            return int.Parse(buffer);
        }

        public static UInt64 ParseUInt64(ReadOnlySpan<byte> bytes)
        {
            var buffer = new Span<char>(new char[bytes.Length]);
            Encoding.ASCII.GetChars(bytes, buffer);
            return UInt64.Parse(buffer);
        }

        public static UInt64 ParseUInt64(ReadOnlySpan<byte> bytes, Span<char> buffer)
        {
            Encoding.ASCII.GetChars(bytes, buffer);
            return UInt64.Parse(buffer);
        }
    }
}