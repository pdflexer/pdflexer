using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using PdfLexer.Parsers;

namespace PdfLexer
{
    /// <summary>
    /// PDF Null Object
    /// </summary>
    public class PdfNull : PdfObject
    {
        public PdfNull()  {  }
        internal static byte[] NullBytes = new byte[4] { (byte)'n', (byte)'u', (byte)'l', (byte)'l' };
        public static PdfNull Value { get; } = new PdfNull();
        public override PdfObjectType Type => PdfObjectType.NullObj;
    }
}
