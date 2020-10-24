using System;

namespace PdfLexer.Objects.Nested
{
    public class NestedUtils
    {
        public static int CountNestedBytes(ReadOnlySpan<byte> bytes)
        {
            var parser = new NestedParser(bytes);
            while (parser.Read()) { }
            if (parser.Completed())
            {
                return parser.GetFullLength();
            }
            return -1;
        }
    }
}