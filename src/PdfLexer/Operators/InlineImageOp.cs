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
            foreach (var item in header)
            {
                PdfOperator.Shared.SerializeObject(stream, item, x => x);
                stream.WriteByte((byte)' ');
            }
            stream.Write(ID_Op.OpData);
            stream.WriteByte((byte)'\n');
            stream.Write(allData);
            stream.Write(EI_Op.OpData);
        }
    }

}
