using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer
{

    public enum PdfNumberType
    {
        Integer,
        Long,
        Double,
        Decimal
    }
    /// <summary>
    /// PDF Number object type.
    /// </summary>
    public abstract class PdfNumber : PdfObject
    {
        public override PdfObjectType Type => PdfObjectType.NumericObj;

        /// <summary>
        /// C# number type of the <see cref="PdfNumber"/>.
        /// </summary>
        public abstract PdfNumberType NumberType { get; }

        public static implicit operator long(PdfNumber num)
        {
            return num switch {
                PdfIntNumber val => (long)val.Value,
                PdfLongNumber val => (long)val.Value,
                _ => throw new ApplicationException("Unable to convert PdfNumber to long for type " + num.GetType())
            };
        }

        public static implicit operator int(PdfNumber num)
        {
            return num switch {
                PdfIntNumber val => val.Value,
                PdfLongNumber val => (int)val.Value,
                _ => throw new ApplicationException("Unable to convert PdfNumber to int for type " + num.GetType())
            };
        }

        public static implicit operator double(PdfNumber num)
        {
            return num switch {
                PdfIntNumber val => (double)val.Value,
                PdfLongNumber val => (double)val.Value,
                PdfDoubleNumber val => val.Value,
                PdfDecimalNumber val => (double)val.Value,
                _ => throw new ApplicationException("Unable to convert PdfNumber to decimal for type " + num.GetType())
            };
        }

        public static implicit operator decimal(PdfNumber num)
        {
            return num switch {
                PdfIntNumber val => (decimal)val.Value,
                PdfLongNumber val => (decimal)val.Value,
                PdfDoubleNumber val => (decimal)val.Value,
                PdfDecimalNumber val => val.Value,
                _ => throw new ApplicationException("Unable to convert PdfNumber to decimal for type " + num.GetType())
            };
        }
    }

    public static class PdfCommonNumbers
    {
        public static readonly PdfIntNumber Zero = new PdfIntNumber(0);
        public static readonly PdfIntNumber One = new PdfIntNumber(1);
        public static readonly PdfIntNumber Two = new PdfIntNumber(2);
        public static readonly PdfIntNumber Three = new PdfIntNumber(3);
        public static readonly PdfIntNumber Four = new PdfIntNumber(4);
        public static readonly PdfIntNumber Five = new PdfIntNumber(5);
        public static readonly PdfIntNumber Six = new PdfIntNumber(6);
        public static readonly PdfIntNumber Seven = new PdfIntNumber(7);
        public static readonly PdfIntNumber Eight = new PdfIntNumber(8);
        public static readonly PdfIntNumber Nine = new PdfIntNumber(9);
        public static readonly PdfIntNumber MinusOne = new PdfIntNumber(-1);
    }

    /// <summary>
    /// Pdf number object represented by a c# int.
    /// </summary>
    public class PdfIntNumber : PdfNumber
    {
        /// <summary>
        /// Value of pdf object.
        /// </summary>
        public int Value { get; }
        public PdfIntNumber(int value)
        {
            Value = value;
        }

        public override PdfNumberType NumberType => PdfNumberType.Integer;
    }

    /// <summary>
    /// Pdf number object represented by a c# long
    /// </summary>
    public class PdfLongNumber : PdfNumber
    {
        /// <summary>
        /// Value of pdf object.
        /// </summary>
        public long Value { get; }
        public PdfLongNumber(long value)
        {
            Value = value;
        }

        public override PdfNumberType NumberType => PdfNumberType.Long;
    }

    /// <summary>
    /// Pdf number object represented by a c# double
    /// </summary>
    public class PdfDoubleNumber : PdfNumber
    {
        /// <summary>
        /// Value of pdf object.
        /// </summary>
        public double Value { get; }
        public PdfDoubleNumber(double value)
        {
            Value = value;
        }

        public override PdfNumberType NumberType => PdfNumberType.Double;
    }

    /// <summary>
    /// Pdf number object represented by a c# decimal
    /// </summary>
    public class PdfDecimalNumber : PdfNumber
    {
        /// <summary>
        /// Value of pdf object.
        /// </summary>
        public decimal Value { get; }
        public PdfDecimalNumber(decimal value)
        {
            Value = value;
        }

        public override PdfNumberType NumberType => PdfNumberType.Decimal;
    }
}
