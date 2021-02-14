using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer
{
    public abstract class PdfNumber : IPdfObject
    {
        public bool IsIndirect => false;
        public PdfObjectType Type => PdfObjectType.NumericObj;
    }

    public class PdfIntNumber : PdfNumber
    {
        public int Value { get; }
        public PdfIntNumber(int value)
        {
            Value = value;
        }
    }

    public class PdfRealNumber : PdfNumber
    {
        public decimal Value { get; }
        public PdfRealNumber(decimal value)
        {
            Value = value;
        }
    }
}
