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

        public static implicit operator long(PdfNumber num)
        {
            return num switch {
                PdfIntNumber val => (long)val.Value,
                PdfLongNumber val => (long)val.Value,
                _ => throw new ApplicationException("Unable to convert PdfNumber to long for type " + num.GetType())
            };
        }
    }

    public static class PdfCommonNumbers
    {
        public static PdfIntNumber Zero => new PdfIntNumber(0);
        public static PdfIntNumber One => new PdfIntNumber(1);
        public static PdfIntNumber Two => new PdfIntNumber(2);
        public static PdfIntNumber Three => new PdfIntNumber(3);
        public static PdfIntNumber Four => new PdfIntNumber(4);
        public static PdfIntNumber Five => new PdfIntNumber(5);
        public static PdfIntNumber Six => new PdfIntNumber(6);
        public static PdfIntNumber Seven => new PdfIntNumber(7);
        public static PdfIntNumber Eight => new PdfIntNumber(8);
        public static PdfIntNumber Nine => new PdfIntNumber(9);
        public static PdfIntNumber MinusOne => new PdfIntNumber(-1);
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
