using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer
{
    public enum PdfNumberType
    {
        Integer,
        Long,
        Decimal
    }
    public abstract class PdfNumber : IPdfObject
    {
        public bool IsIndirect => false;
        public PdfObjectType Type => PdfObjectType.NumericObj;
        public abstract PdfNumberType NumberType { get; }
    }

    public class PdfIntNumber : PdfNumber
    {
        public int Value { get; }
        public PdfIntNumber(int value)
        {
            Value = value;
        }

        public override PdfNumberType NumberType => PdfNumberType.Integer;
    }

    public class PdfLongNumber : PdfNumber
    {
        public long Value { get; }
        public PdfLongNumber(long value)
        {
            Value = value;
        }

        public override PdfNumberType NumberType => PdfNumberType.Long;
    }

    public class PdfDecimalNumber : PdfNumber
    {
        public decimal Value { get; }
        public PdfDecimalNumber(decimal value)
        {
            Value = value;
        }

        public override PdfNumberType NumberType => PdfNumberType.Decimal;
    }
}
