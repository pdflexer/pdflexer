using System;
using System.Buffers;
using System.Buffers.Text;
using System.IO;
using System.Text;
using PdfLexer.Parsers;

namespace PdfLexer.Serializers
{
    public class NumberSerializer : ISerializer<PdfNumber>
    {
        public void WriteToStream(PdfNumber obj, Stream stream)
        {
            var buff = ArrayPool<byte>.Shared.Rent(20);
            var count = GetBytes(obj, buff);
            stream.Write(buff, 0, count);
            ArrayPool<byte>.Shared.Return(buff);
        }

        public int GetBytes(PdfNumber obj, Span<byte> data)
        {
            if (obj is PdfIntNumber integer)
            {
                if (!Utf8Formatter.TryFormat(integer.Value, data, out var written))
                {
                    throw new ApplicationException("Unable for write PdfNumber integer: " + Encoding.ASCII.GetString(data));
                }

                return written;
            } else if (obj is PdfDecimalNumber dn)
            {
                if (!Utf8Formatter.TryFormat(dn.Value, data, out var written))
                {
                    throw new ApplicationException("Unable for write PdfNumber decimal: " + Encoding.ASCII.GetString(data));
                }
                return written;
            } else if (obj is PdfLongNumber ln)
            {
                if (!Utf8Formatter.TryFormat(ln.Value, data, out var written))
                {
                    throw new ApplicationException("Unable for write PdfNumber long: " + Encoding.ASCII.GetString(data));
                }
                return written;
            }
            
            throw new NotImplementedException("Unknown number type: " + obj.GetType());
        }
    }
}
