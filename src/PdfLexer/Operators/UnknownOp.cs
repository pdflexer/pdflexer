using System.IO;
using System.Numerics;
using System.Text;

namespace PdfLexer.Operators;

#if NET7_0_OR_GREATER
public class Unkown_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
#else
public class Unkown_Op : IPdfOperation
#endif

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

    public override string ToString()
    {
        return Encoding.ASCII.GetString(allData);
    }
}
