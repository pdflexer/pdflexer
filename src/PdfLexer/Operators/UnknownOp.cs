using System.IO;

namespace PdfLexer.Operators
{
    public class Unkown_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.Unknown;

        public string op { get; }
        public byte[] allData { get; }
        public Unkown_Op(string op, byte[] allData)
        {
            this.op = op;
            this.allData = allData;
        }
        public void Serialize(Stream stream)
        {
            stream.Write(allData);
        }
    }
}
