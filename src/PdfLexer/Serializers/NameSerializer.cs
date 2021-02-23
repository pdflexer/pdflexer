using System;
using System.Buffers;
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
            var size = obj.Value.Length;
            if (NeedsEscaping(obj))
            {
                foreach (var c in obj.Value)
                {
                    // TODO swith to || || || instead of indexof for perf
                    if (Array.IndexOf(needsEscaping,  c) > -1)
                    {
                        size += 2;
                    }
                }
            }
            // TODO well known values or grab from cache since we are saving key
            var buffer = nameBuffer;
            if (obj.Value.Length > nameBuffer.Length)
            {
                var array = ArrayPool<byte>.Shared.Rent(obj.Value.Length);
                Span<byte> larger = array;
                var count = GetBytes(obj, larger, false);
                stream.Write(count != larger.Length ? larger.Slice(0, count) : larger);
                ArrayPool<byte>.Shared.Return(array);
                return;
            }
            var written = GetBytes(obj, buffer, false);
            stream.Write(buffer, 0, written);
            return;
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
                        // TODO swith to || || || instead of indexof for perf
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
