using System;
using System.Buffers;
using System.IO;
using System.Text;

namespace PdfLexer.Serializers
{
    public class NameSerializer : ISerializer<PdfName>
    {
        public void WriteToStream(PdfName obj, Stream stream)
        {
            // TODO look into writing from cache?
            if (obj.Value.Length < 50)
            {
                Span<byte> buffer = stackalloc byte[100];
                var written = GetBytes(obj, buffer);
                stream.Write(buffer.Slice(0, written));   
            } else
            {
                var buffer = ArrayPool<byte>.Shared.Rent(obj.Value.Length*3);
                var written = GetBytes(obj, buffer);
                stream.Write(buffer, 0, written);   
                ArrayPool<byte>.Shared.Return(buffer);
            }
            return;
        }

        public int GetBytes(PdfName obj, Span<byte> data)
        {
            if (obj.CacheValue > 0)
            {
                return WriteCached(obj, data);
            }

            data[0] = (byte)'/';
            var ci = 1; // TODO perf analysis
            for (var i=1;i<obj.Value.Length;i++)
            {
                var cc = obj.Value[i];
                if (cc == (char) 0 || cc ==  (char) 9 || cc ==  (char) 10 || cc == (char) 12 
                    || cc == (char) 13 || cc == (char) 32 || cc =='(' || cc == ')' || cc == '<' 
                    || cc == '>' || cc == '[' || cc == ']' || cc == '{' || cc == '}' || cc == '/' 
                    || cc == '%' || cc == '#')
                {
                    data[ci++] = (byte) '#';
                    var hex = ((int)cc).ToString("X2"); // TODO can make this better as well
                    data[ci++] = (byte) hex[0];
                    data[ci++] = (byte) hex[1];
                } else
                {
                    data[ci++] = (byte) cc;
                }
            }

            return ci;
        }

        public int WriteCached(PdfName obj, Span<byte> data)
        {
            data[0] = (byte)'/';
            ulong val = obj.CacheValue;
            for (var i=1;i<9;i++)
            {
                var cv = (byte)(val & 0xFF);
                if (cv == 0)
                {
                    return i;
                }
                data[i] = cv;
                val = val >> 8;
            }
            return 9;
        }

        private static char[] needsEscaping = new char[]
        {
            (char) 0, (char) 9, (char) 10, (char) 12, (char) 13, (char) 32,
            '(', ')', '<', '>', '[', ']', '{', '}', '/', '%', '#'
        };        
    }
}
