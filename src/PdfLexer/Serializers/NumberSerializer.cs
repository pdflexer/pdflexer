using System.Buffers.Text;
using System.Text;

namespace PdfLexer.Serializers;

internal class NumberSerializer : ISerializer<PdfNumber>
{
    private byte[] miniBuffer = new byte[100];
    public void WriteToStream(PdfNumber obj, Stream stream)
    {
        var count = GetBytes(obj, miniBuffer);
        stream.Write(miniBuffer, 0, count);
    }

    public int GetBytes(PdfNumber obj, Span<byte> data)
    {
        if (obj.NumberType == PdfNumberType.Integer)
        {
            var integer = (PdfIntNumber)obj;
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
        else if (obj is PdfDoubleNumber db)
        {
            if (!Utf8Formatter.TryFormat(db.Value, data, out var written))
            {
                throw new ApplicationException("Unable for write PdfNumber doube: " + Encoding.ASCII.GetString(data));
            }
            return written;
        }


        throw new NotImplementedException("Unknown number type: " + obj.GetType());
    }
}
