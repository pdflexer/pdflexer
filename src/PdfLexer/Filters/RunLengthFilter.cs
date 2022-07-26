using System.IO;

namespace PdfLexer.Filters
{
    public class RunLengthFilter : IDecoder
    {

        public static RunLengthFilter Instance { get; } = new RunLengthFilter();
        public RunLengthFilter()
        {
        }

        public Stream Decode(Stream stream, PdfDictionary filterParams) => new RunLengthStream(stream);
    }

    internal class RunLengthStream : MinBufferStream
    {
        public RunLengthStream(Stream stream) : base(stream, 128)
        {

        }
        protected override int FillBuffer(byte[] outgoing, int offset, int count)
        {
            var b1 = inner.ReadByte();
            if (b1 == -1 || b1 == 128) { return -1; }
            var b2 = inner.ReadByte();
            if (b2 == -1) { return -1; }


            if (b1 < 128)
            {
                // copy n bytes
                outgoing[offset++] = (byte)b2;
                while (b1 > 0)
                {
                    var read = inner.Read(outgoing, offset, b1);
                    if (read == -1) { return -1; }
                    offset += read;
                    b1 -= read;
                }
                return b1 + 1;
            }
            else
            {
                int n = 257 - b1;
                for (var i = 0; i < n; i++)
                {
                    outgoing[offset++] = (byte)n;
                }
                return n;
            }
        }
    }
}
