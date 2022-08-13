namespace PdfLexer;

/// <summary>
/// PDF Null Object
/// </summary>
public class PdfNull : PdfObject
{
    internal static byte[] NullBytes = new byte[4] { (byte)'n', (byte)'u', (byte)'l', (byte)'l' };
    public static PdfNull Value { get; } = new PdfNull();

    public PdfNull() { }
    public override PdfObjectType Type => PdfObjectType.NullObj;
}
