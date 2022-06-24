using System;
using System.IO;

namespace PdfLexer.Serializers
{
    public class BoolSerializer : ISerializer<PdfBoolean>
    {
        public void WriteToStream(PdfBoolean obj, Stream stream)
        {
            if (obj.Value)
            {
                stream.Write(PdfBoolean.TrueBytes);
                return;
            }

            stream.Write(PdfBoolean.FalseBytes);
        }

        public int GetBytes(PdfBoolean obj, Span<byte> data)
        {
            if (obj.Value)
            {
                PdfBoolean.TrueBytes.CopyTo(data);
                return 4;
            }
            PdfBoolean.FalseBytes.CopyTo(data);
            return 5;
        }
    }
}
