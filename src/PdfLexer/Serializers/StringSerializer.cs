using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PdfLexer.Serializers
{
    public class StringSerializer : ISerializer<PdfString>
    {
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
            var buffer = ArrayPool<byte>.Shared.Rent((obj.Value?.Length ?? 0)*3+2); // overkill find better solution
            var i = GetBytes(obj, buffer);
            stream.Write(buffer, 0, i);
        }

        public int GetLiteralBytes(PdfString obj, Span<byte> data)
        {
            var ri = 0;
            ReadOnlySpan<int> escapes = escapeNeeded;
            //ReadOnlySpan<char> chars = obj.Value.AsSpan();
            data[ri++] = (byte)'(';
            int ei = 0;
            for (var i=0;i<obj.Value.Length;i++)
            {
                var c = (int)obj.Value[i];
                if (c == (int)'(' || c == (int)')') // wastes a little space if escaping not needed but better than forward searching
                {
                    data[ri++] = (byte)'\\';
                    data[ri++] = (byte)c;
                } else if ((ei = escapes.IndexOf(c)) > -1)
                {
                    data[ri++] = (byte)'\\';
                    data[ri++] = (byte)escaped[ei];
                } else if (c < 32 || c > 127) // non printable
                {
                    var b3 = c/64;
                    var b2 = (c - b3*64)/8;
                    var b1 = c%8;
                    data[ri++] = (byte)'\\';
                    data[ri++] = (byte)(b3+'0');
                    data[ri++] = (byte)(b2+'0');
                    data[ri++] = (byte)(b1+'0');

                } else 
                {
                    data[ri++] = (byte)c;
                }
            }
            data[ri++] = (byte)')';
            return ri;
        }

        public int GetBytes(PdfString obj, Span<byte> data)
        {
            if (obj.StringType == PdfStringType.Hex)
            {
                return GetHexBytes(obj, data);
            } else
            {
                 return GetLiteralBytes(obj, data);
            }
        }

        private static byte[] hexVals = { (byte) '0',(byte) '1',(byte) '2', (byte) '3',(byte) '4', (byte)'5',(byte) '6',(byte) '7', (byte)'8',(byte) '9',
                    (byte)'A', (byte)'B', (byte)'C', (byte)'D', (byte)'E', (byte)'F' };
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
