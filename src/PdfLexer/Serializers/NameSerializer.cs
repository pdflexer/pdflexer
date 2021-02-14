using System;
using System.IO;
using System.Text;

namespace PdfLexer.Serializers
{
    public class NameSerializer : ISerializer<PdfName>
    {
        private bool NeedsEscaping(PdfName obj)
        {
            bool escapeNeeded = false;
            if (obj.NeedsEscaping == null)
            {
                if (obj.Value.IndexOfAny(needsEscaping) > -1)
                {
                    escapeNeeded = true;
                }
            } else
            {
                escapeNeeded = obj.NeedsEscaping.Value;
            }

            return escapeNeeded;
        }

        private byte[] nameBuffer = new byte[100];
        public void WriteToStream(PdfName obj, Stream stream)
        {
            // TODO well known values or grab from cache since we are saving key
            if (!NeedsEscaping(obj))
            {
                var buffer = nameBuffer;
                if (obj.Value.Length > nameBuffer.Length)
                {
                    Span<byte> larger = stackalloc byte[obj.Value.Length];
                    var count = GetBytes(obj, larger, false);
                    stream.Write(count != buffer.Length ? larger.Slice(0, count) : buffer);
                    return;
                }
                var written = GetBytes(obj, buffer, false);
                stream.Write(buffer, 0, written);
                return;
            }

            int extra = 0;
            foreach (var c in obj.Value)
            {
                if (Array.IndexOf(needsEscaping,  c) > -1)
                {
                    extra += 2;
                }
            }

            {
                Span<byte> buffer = stackalloc byte[obj.Value.Length + extra];
                var count = GetBytes(obj, buffer, true);
                stream.Write(count != buffer.Length ? buffer.Slice(0, count) : buffer);
            }

        }

        public int GetBytes(PdfName obj, Span<byte> data, bool escapeNeeded)
        {
            if (!escapeNeeded)
            {
                return Encoding.ASCII.GetBytes(obj.Value, data);
            }
            else
            {
                // TODO look into this
                var ci = 0;
                for (var i=0;i<obj.Value.Length;i++)
                {
                    var cc = obj.Value[i];
                    if (i == 0)
                    {
                        data[ci++] = (byte)cc;
                    }
                    else
                    {
                        if (Array.IndexOf(needsEscaping, cc) > -1)
                        {
                            data[ci++] = (byte) '#';
                            var hex = ((int)cc).ToString("X2");
                            data[ci++] = (byte) hex[0];
                            data[ci++] = (byte) hex[1];
                        } else
                        {
                            data[ci++] = (byte) cc;
                        }
                    }
                }

                return ci;
            }
        }

        private static char[] needsEscaping = new char[]
        {
            (char) 0, (char) 9, (char) 10, (char) 12, (char) 13, (char) 32,
            '(', ')', '<', '>', '[', ']', '{', '}', '/', '%', '#'
        };

        // TODO well known values
        public int GetBytes(PdfName obj, Span<byte> data)
            => GetBytes(obj, data, NeedsEscaping(obj));
        
    }
}
