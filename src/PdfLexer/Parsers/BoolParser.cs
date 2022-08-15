using System.Diagnostics;

namespace PdfLexer.Parsers;

internal class BoolParser : Parser<PdfBoolean>
{
    public static byte[] truebytes = new byte[4] { (byte)'t', (byte)'r', (byte)'u', (byte)'e' };
    public static byte[] falsebytes = new byte[5] { (byte)'f', (byte)'a', (byte)'l', (byte)'s', (byte)'e' };

    public override PdfBoolean Parse(ReadOnlySpan<byte> buffer)
    {
        var fb = buffer[0];
        if (fb == (byte)'t')
        {
            Debug.Assert(buffer.Length == 4, "True bool was 4 bytes.");
            return PdfBoolean.True;
        }
        Debug.Assert(buffer.Length == 5, "False bool was 5 bytes.");
        return PdfBoolean.False;
    }
}
