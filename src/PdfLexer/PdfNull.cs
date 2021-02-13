using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using PdfLexer.Parsers;

namespace PdfLexer
{
    public class PdfNull : IPdfObject
    {
        internal static byte[] NullBytes = new byte[4] { (byte)'n', (byte)'u', (byte)'l', (byte)'l' };
        public static PdfNull Value { get; } = new PdfNull();
        public bool IsIndirect => false;
        public PdfObjectType Type => PdfObjectType.NullObj;
        public void WriteObject(Stream stream)
        {
            stream.Write(NullBytes);
        }
    }
}
