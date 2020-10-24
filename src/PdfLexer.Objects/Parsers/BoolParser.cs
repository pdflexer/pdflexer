using System;
using System.Text;

namespace PdfLexer.Objects.Parsers
{
    public class BoolParser
    {
        public static byte[] truebytes = new byte[4] { (byte)'t', (byte)'r', (byte)'u', (byte)'e' };
        public static byte[] falsebytes = new byte[5] { (byte)'f', (byte)'a', (byte)'l', (byte)'s', (byte)'e' };
        public static bool GetBool(ReadOnlySpan<byte> bytes, out bool result, out int bytesUsed)
        {
            if (bytes[0] == 't')
            {
                if (bytes.Length < 4)
                {
                    result = false;
                    bytesUsed = 0;
                    return false;
                }
                if (bytes.IndexOf(truebytes) != 0)
                {
                    throw new ApplicationException($"Invalid bool object: {Encoding.ASCII.GetString(bytes.Slice(0, 4))}");
                }
                result = true;
                bytesUsed = 4;
                return true;

            }
            else if (bytes[0] == 'f')
            {
                if (bytes.Length < 5)
                {
                    result = false;
                    bytesUsed = 0;
                    return false;
                }
                if (bytes.IndexOf(falsebytes) != 0)
                {
                    throw new ApplicationException($"Invalid bool object: {Encoding.ASCII.GetString(bytes.Slice(0, 4))}");
                }
                result = false;
                bytesUsed = 5;
                return true;
            }
            else
            {
                throw new ApplicationException($"Invalid bool object.");
            }
        }
    }
}