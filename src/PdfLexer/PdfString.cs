using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer
{
    public class PdfString : IPdfObject
    {
        // TODO support lazy writing
        public PdfString(string value)
        {
            Value = value;
        }
        public string Value { get; }
        public bool IsIndirect { get;set; }

        public PdfObjectType Type => PdfObjectType.StingObj;
    }
}
