using System;
using System.Text;

namespace PdfLexer.Objects.Parsers
{
    public class NullParser
    {
        public static byte[] nullbytes = new byte[4] { (byte)'n', (byte)'u', (byte)'l', (byte)'l' };
        public static bool GetNull(ReadOnlySpan<byte> bytes, out ReadOnlySpan<byte> nullBytes)
        {
            if (bytes.Length < 4)
            {
                nullBytes = null;
                return false;
            }
            if (bytes.IndexOf(nullbytes) != 0)
            {
                throw new ApplicationException($"Invalid null object: {Encoding.ASCII.GetString(bytes.Slice(0, 4))}");
            }
            nullBytes = bytes.Slice(0, 4);
            return true;
        }
    }
}