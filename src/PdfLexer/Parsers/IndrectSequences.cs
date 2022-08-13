namespace PdfLexer.Parsers;

internal class IndirectSequences
{
    public static byte[] obj = new byte[] { (byte)'o', (byte)'b', (byte)'j' };
    public static byte[] endobj = new byte[] { (byte)'e', (byte)'n', (byte)'d',
        (byte)'o', (byte)'b', (byte)'j' };
    public static byte[] stream = new byte[] { (byte)'s', (byte)'t', (byte)'r',
        (byte)'e', (byte)'a', (byte)'m' };
    public static byte[] endstream = new byte[] { (byte)'e', (byte)'n', (byte)'d',(byte)'s', (byte)'t', (byte)'r',
        (byte)'e', (byte)'a', (byte)'m' };
    public static byte[] length = new byte[] { (byte)'/', (byte)'L', (byte)'e',
        (byte)'n', (byte)'g', (byte)'t', (byte)'h' };
    public static byte[] strartxref = new byte[] { (byte)'s', (byte)'t', (byte)'a',
        (byte)'r', (byte)'t', (byte)'x', (byte)'r', (byte)'e', (byte)'f' };
}
