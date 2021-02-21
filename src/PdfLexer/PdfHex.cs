using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer
{
    public class PdfHex : PdfObject
    {
        public PdfHex(byte[] value)
        {
            Value = value;
        }
        public byte[] Value {get;}
        public override PdfObjectType Type => PdfObjectType.HexObj;
    }
}
