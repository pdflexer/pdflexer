using System.IO;

namespace PdfLexer.Operators
{
    public class InlineImage_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.Unknown;

        public PdfArray header { get; }
        public byte[] allData { get; }
        public InlineImage_Op(PdfArray header, byte[] allData)
        {
            this.header = header;
            this.allData = allData;
        }
        public void Serialize(Stream stream)
        {
            stream.Write(BI_Op.OpData);
            stream.WriteByte((byte)'\n');
            for (var i = 0; i < header.Count; i++)
            {
                var item = header[i];
                PdfOperator.Shared.SerializeObject(stream, item, x => x);
                if (i < header.Count - 1)
                {
                    stream.WriteByte((byte)' ');
                }
            }
            stream.WriteByte((byte)'\n');
            stream.Write(ID_Op.OpData);
            stream.WriteByte((byte)'\n');
            stream.Write(allData);
            stream.WriteByte((byte)'\n');
            stream.Write(EI_Op.OpData);
        }
    }

}
