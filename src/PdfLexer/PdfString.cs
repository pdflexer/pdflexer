using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer
{
    public class PdfString : PdfObject
    {
        // TODO support lazy writing
        public PdfString(string value)
        {
            Value = value;
        }
        public string Value { get; }
        public override PdfObjectType Type => PdfObjectType.StringObj;
    }
}
