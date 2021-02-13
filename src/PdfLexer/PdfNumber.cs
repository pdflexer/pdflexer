using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer
{
    public class PdfNumber : IPdfObject
    {
        public bool IsIndirect => false;
        public PdfObjectType Type => PdfObjectType.NumericObj;
    }
}
