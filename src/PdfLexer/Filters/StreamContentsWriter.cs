using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace PdfLexer.Filters
{
    public interface IStreamContentsWriter
    {
        Stream Stream { get; }
        PdfStreamContents Complete();
    }
    public class FlateWriter : IStreamContentsWriter
    {
        private MemoryStream ms;

        public Stream Stream { get; }

        public FlateWriter()
        {
            // can write to file in low mem mode
            ms = new MemoryStream();
            ms.WriteByte(120);
            ms.WriteByte(1);
            Stream = new DeflateStream(ms, CompressionLevel.Fastest, true);
        }

        public PdfStreamContents Complete()
        {
            Stream.Flush();
            // TODO calc this during writing
            ms.Seek(0, SeekOrigin.Begin);
            var cs = Calculate(ms, 65521);
            ms.Seek(0, SeekOrigin.End);
            ms.WriteByte((byte)(cs >> 24));
            ms.WriteByte((byte)(cs >> 16));
            ms.WriteByte((byte)(cs >> 8));
            ms.WriteByte((byte)(cs >> 0));

            return new PdfByteArrayStreamContents(ms.ToArray(), PdfName.FlateDecode, null);

            static int Calculate(Stream data, int modulus)
            {
                var s1 = 1;
                var s2 = 0;

                int b = 0;
                while ((b = data.ReadByte()) != -1)
                {
                    s1 = (s1 + b) % modulus;
                    s2 = (s1 + s2) % modulus;
                }
                return s2 * 65536 + s1;
            }
        }

        public static implicit operator Stream(FlateWriter str) => str.Stream;
    }
}
