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
        public int GetBytes(PdfString obj, Span<byte> data)
        {
            var ri = 0;
            ReadOnlySpan<int> escapes = escapeNeeded;
            ReadOnlySpan<char> chars = obj.Value.AsSpan();
            data[ri++] = (byte)'(';
            int depth = 0;
            int ei = 0;
            for (var i=0;i<obj.Value.Length;i++)
            {
                var c = (int)obj.Value[i];
                if (c == (int)'(')
                {
                    if (chars.Slice(i).IndexOf(')') > -1)
                    {
                        data[ri++] = (byte)c;
                        depth++;
                    } else
                    {
                        data[ri++] = (byte)'\\';
                        data[ri++] = (byte)c;
                    }
                } else if (c == (int)')')
                {
                    if (depth > 0)
                    {
                        data[ri++] = (byte)c;
                        depth--;
                    } else
                    {
                        data[ri++] = (byte)'\\';
                        data[ri++] = (byte)c;
                    }
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

        // TODO add support for writing raw bytes
        public void WriteToStream(PdfString obj, Stream stream)
        {
            var buffer = ArrayPool<byte>.Shared.Rent((obj.Value?.Length ?? 0)*3+2); // overkill find better solution
            var i = GetBytes(obj, buffer);
            stream.Write(buffer, 0, i);
            // ReadOnlySpan<int> escapes = escapeNeeded;
            // ReadOnlySpan<char> chars = obj.Value.AsSpan();
            // stream.WriteByte((byte)'(');
            // int depth = 0;
            // int ei = 0;
            // for (var i=0;i<obj.Value.Length;i++)
            // {
            //     var c = (int)obj.Value[i];
            //     if (c == (int)'(')
            //     {
            //         if (chars.Slice(i).IndexOf(')') > -1)
            //         {
            //             stream.WriteByte((byte)c);
            //             depth++;
            //         } else
            //         {
            //             stream.WriteByte((byte)'\\');
            //             stream.WriteByte((byte)c);
            //         }
            //     } else if (c == (int)')')
            //     {
            //         if (depth > 0)
            //         {
            //             stream.WriteByte((byte)c);
            //             depth--;
            //         } else
            //         {
            //             stream.WriteByte((byte)'\\');
            //             stream.WriteByte((byte)c);
            //         }
            //     } else if ((ei = escapes.IndexOf(c)) > -1)
            //     {
            //         stream.WriteByte((byte)'\\');
            //         stream.WriteByte((byte)escaped[ei]);
            //     } else if (c < 32 || c > 127) // non printable
            //     {
            //         var b3 = c/64;
            //         var b2 = (c - b3*64)/8;
            //         var b1 = c%8;
            //         stream.WriteByte((byte)'\\');
            //         stream.WriteByte((byte)(b3+'0'));
            //         stream.WriteByte((byte)(b2+'0'));
            //         stream.WriteByte((byte)(b1+'0'));
            // 
            //     } else 
            //     {
            //         stream.WriteByte((byte)c);
            //     }
            // }
            // stream.WriteByte((byte)')');
        }
    }
}
