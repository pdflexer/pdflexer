using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PdfLexer.Serializers
{
    public class StringSerializer : ISerializer<PdfString>
    {
        private static Encoding Iso88591 = Encoding.GetEncoding("ISO-8859-1");
        private static int[] escapeNeeded = new int[]
        {
            '\r', '\n', '\t', '\b', '\f', '\\'
        };
        private static int[] escaped = new int[]
        {
            'r', 'n', 't', 'b', 'f', '\\'
        };

        // TODO open up support for writing raw bytes for specific use cases
        public void WriteToStream(PdfString obj, Stream stream)
        {
            var buffer = ArrayPool<byte>.Shared.Rent((obj.Value?.Length ?? 0) * 8 + 16); // overkill find better solution
            var i = GetBytes(obj, buffer);
            stream.Write(buffer, 0, i);
            ArrayPool<byte>.Shared.Return(buffer);
        }

        public void WriteToStream(PdfTextEncodingType encoding, PdfStringType type, string value, Stream stream)
        {
            var buffer = ArrayPool<byte>.Shared.Rent((value?.Length ?? 0) * 8 + 16); // overkill find better solution
            var i = GetBytes(encoding, type, value, buffer);
            stream.Write(buffer, 0, i);
            ArrayPool<byte>.Shared.Return(buffer);
        }

        public void WriteToStream(ReadOnlySpan<byte> data, Stream stream)
        {
            var buffer = ArrayPool<byte>.Shared.Rent((data.Length) * 8 + 16); // overkill find better solution
            var i = ConvertLiteralBytes(data, buffer, true);
            stream.Write(buffer, 0, i);
            ArrayPool<byte>.Shared.Return(buffer);
        }

        [Obsolete]
        public int GetLiteralBytes(PdfString obj, Span<byte> data)
        {
            var ri = 0;
            ReadOnlySpan<int> escapes = escapeNeeded;
            //ReadOnlySpan<char> chars = obj.Value.AsSpan();
            data[ri++] = (byte)'(';
            int ei = 0;
            for (var i = 0; i < obj.Value.Length; i++)
            {
                var c = (int)obj.Value[i];
                if (c == (int)'(' || c == (int)')') // wastes a little space if escaping not needed but better than forward searching
                {
                    data[ri++] = (byte)'\\';
                    data[ri++] = (byte)c;
                }
                else if ((ei = escapes.IndexOf(c)) > -1)
                {
                    data[ri++] = (byte)'\\';
                    data[ri++] = (byte)escaped[ei];
                }
                else if (c < 32 || c > 126) // non printable
                {
                    // var b3 = c / 64;
                    // var b2 = (c - b3 * 64) / 8;
                    // var b1 = c % 8;
                    // data[ri++] = (byte)'\\';
                    // data[ri++] = (byte)(b3 + '0');
                    // data[ri++] = (byte)(b2 + '0');
                    // data[ri++] = (byte)(b1 + '0');
                    data[ri++] = (byte)'\\';
                    data[ri++] = (byte)((c >> 6) | '0'); // = (c / 64) + 48
                    data[ri++] = (byte)(((c >> 3) & 0b_0000_0111) | '0'); // = (c / 8) % 8 + 48
                    data[ri++] = (byte)((c & 0b_0000_0111) | '0'); // = (c % 8) + 48

                }
                else
                {
                    data[ri++] = (byte)c;
                }
            }
            data[ri++] = (byte)')';
            return ri;
        }

        internal static int ConvertLiteralBytes(ReadOnlySpan<byte> data, Span<byte> output, bool escapeNonPrintable)
        {
            var ri = 0;
            ReadOnlySpan<int> escapes = escapeNeeded;
            output[ri++] = (byte)'(';
            int ei = 0;
            for (var i = 0; i < data.Length; i++)
            {
                var b = data[i];
                if (b == '(' || b == ')') // wastes a little space if escaping not needed but better than forward searching
                {
                    output[ri++] = (byte)'\\';
                    output[ri++] = (byte)b;
                }
                else if ((ei = escapes.IndexOf(b)) > -1)
                {
                    output[ri++] = (byte)'\\';
                    output[ri++] = (byte)escaped[ei];
                }
                else if (escapeNonPrintable && (b < 32 || b > 126)) // non printable
                {
                    // var b3 = b / 64;
                    // var b2 = (b - b3 * 64) / 8;
                    // var b1 = b % 8;
                    // output[ri++] = (byte)'\\';
                    // output[ri++] = (byte)(b3 + '0');
                    // output[ri++] = (byte)(b2 + '0');
                    // output[ri++] = (byte)(b1 + '0');
                    output[ri++] = (byte)'\\';
                    output[ri++] = (byte)((b >> 6) | '0'); // = (c / 64) + 48
                    output[ri++] = (byte)(((b >> 3) & 0b_0000_0111) | '0'); // = (c / 8) % 8 + 48
                    output[ri++] = (byte)((b & 0b_0000_0111) | '0'); // = (c % 8) + 48
                }
                else
                {
                    output[ri++] = (byte)b;
                }
            }
            output[ri++] = (byte)')';
            return ri;
        }

        public int GetBytes(PdfString obj, Span<byte> data)
        {
            // TODO perf analysis
            var rented = ArrayPool<byte>.Shared.Rent(data.Length); // todo clean up... don't need this much
            Span<byte> existing = rented;
            if (obj.Encoding == PdfTextEncodingType.UTF16BE)
            {
                existing[0] = 0xFE;
                existing[1] = 0xFF;
                var count = Encoding.BigEndianUnicode.GetBytes(obj.Value, existing.Slice(2));
                existing = existing.Slice(0, count + 2);
            }
            else
            {
                // TODO real pdf doc encoding
                // TODO encoding guessing / calculation
                var count = Iso88591.GetBytes(obj.Value, existing);
                existing = existing.Slice(0, count);
            }

            if (obj.StringType == PdfStringType.Hex)
            {
                var val = ConvertHexBytes(existing, data);
                ArrayPool<byte>.Shared.Return(rented);
                return val;
            }
            else
            {
                var val = ConvertLiteralBytes(existing, data, obj.Encoding != PdfTextEncodingType.UTF16BE);
                ArrayPool<byte>.Shared.Return(rented);
                return val;
            }
        }

        public int GetBytes(PdfTextEncodingType encoding, PdfStringType type, string value, Span<byte> data)
        {
            // TODO perf analysis
            var rented = ArrayPool<byte>.Shared.Rent(data.Length); // todo clean up... don't need this much
            Span<byte> existing = rented;
            if (encoding == PdfTextEncodingType.UTF16BE)
            {
                existing[0] = 0xFE;
                existing[1] = 0xFF;
                var count = Encoding.BigEndianUnicode.GetBytes(value, existing.Slice(2));
                existing = existing.Slice(0, count + 2);
            }
            else
            {
                // TODO real pdf doc encoding
                // TODO encoding guessing / calculation
                var count = Iso88591.GetBytes(value, existing);
                existing = existing.Slice(0, count);
            }

            if (type == PdfStringType.Hex)
            {
                var val = ConvertHexBytes(existing, data);
                ArrayPool<byte>.Shared.Return(rented);
                return val;
            }
            else
            {
                var val = ConvertLiteralBytes(existing, data, encoding != PdfTextEncodingType.UTF16BE);
                ArrayPool<byte>.Shared.Return(rented);
                return val;
            }
        }

        private static byte[] hexVals = { (byte) '0',(byte) '1',(byte) '2', (byte) '3',(byte) '4', (byte)'5',(byte) '6',(byte) '7', (byte)'8',(byte) '9',
                    (byte)'A', (byte)'B', (byte)'C', (byte)'D', (byte)'E', (byte)'F' };

        private int ConvertHexBytes(ReadOnlySpan<byte> data, Span<byte> output)
        {
            var di = 0;
            output[di++] = (byte)'<';
            for (var i = 0; i < data.Length; i++)
            {
                var b = data[i];
                int high = ((b & 0xf0) >> 4);
                int low = (b & 0x0f);
                output[di++] = hexVals[high];
                output[di++] = hexVals[low];
            }
            output[di++] = (byte)'>';
            return di;
        }

        private int GetHexBytes(PdfString obj, Span<byte> data)
        {
            var di = 0;
            data[di++] = (byte)'<';
            for (var i = 0; i < obj.Value.Length; i++)
            {
                var b = obj.Value[i];
                int high = ((b & 0xf0) >> 4);
                int low = (b & 0x0f);
                data[di++] = hexVals[high];
                data[di++] = hexVals[low];
            }
            data[di++] = (byte)'>';
            return di;
        }
    }
}
