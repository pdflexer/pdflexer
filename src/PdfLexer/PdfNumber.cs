
using System.Diagnostics.CodeAnalysis;

namespace PdfLexer;

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

    [return: NotNullIfNotNull("num")]
    public static implicit operator long?(PdfNumber? num)
    {
        if (num == null) { return null; }
        return (long)num;
    }

    public static implicit operator int(PdfNumber num)
    {
        return num switch {
            PdfIntNumber val => val.Value,
            PdfDoubleNumber val => (int)val.Value,
            PdfLongNumber val => (int)val.Value,
            _ => throw new ApplicationException("Unable to convert PdfNumber to int for type " + num.GetType())
        };
    }

    [return: NotNullIfNotNull("num")]
    public static implicit operator int?(PdfNumber? num)
    {
        if (num == null) { return null; }
        return (int)num;
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

    [return: NotNullIfNotNull("num")]
    public static implicit operator double?(PdfNumber? num)
    {
        if (num == null) { return null; }
        return (double)num;
    }

    [return: NotNullIfNotNull("num")]
    public static implicit operator float(PdfNumber num)
    {
        return num switch
        {
            PdfIntNumber val => (float)val.Value,
            PdfLongNumber val => (float)val.Value,
            PdfDoubleNumber val => (float)val.Value,
            PdfDecimalNumber val => (float)val.Value,
            _ => throw new ApplicationException("Unable to convert PdfNumber to float for type " + num.GetType())
        };
    }

    [return: NotNullIfNotNull("num")]
    public static implicit operator float?(PdfNumber? num)
    {
        if (num == null) { return null; }
        return (float)num;
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

    [return: NotNullIfNotNull("num")]
    public static implicit operator decimal?(PdfNumber? num)
    {
        if (num == null) { return null; }
        return (decimal)num;
    }

    public static implicit operator PdfNumber(int num)
    {
        return new PdfIntNumber(num);
    }

    public static implicit operator PdfNumber(float num)
    {
        return new PdfDoubleNumber(num);
    }
    public static implicit operator PdfNumber(decimal num)
    {
        return new PdfDecimalNumber(num);
    }

}

public static class PdfCommonNumbers
{
    public static readonly PdfIntNumber Zero = new(0);
    public static readonly PdfIntNumber One = new(1);
    public static readonly PdfIntNumber Two = new(2);
    public static readonly PdfIntNumber Three = new(3);
    public static readonly PdfIntNumber Four = new(4);
    public static readonly PdfIntNumber Five = new(5);
    public static readonly PdfIntNumber Six = new(6);
    public static readonly PdfIntNumber Seven = new(7);
    public static readonly PdfIntNumber Eight = new(8);
    public static readonly PdfIntNumber Nine = new(9);
    public static readonly PdfIntNumber MinusOne = new(-1);
}

/// <summary>
/// Pdf number object represented by a c# int.
/// </summary>
public class PdfIntNumber : PdfNumber, IEquatable<PdfIntNumber>
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

    public override bool Equals(object? obj)
    {
        if (obj == null) { return false; }
        return obj is PdfIntNumber num && Value.Equals(num?.Value);
    }

    public virtual bool Equals(PdfIntNumber? other)
    {
        return Value.Equals(other?.Value);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public static bool operator ==(PdfIntNumber? n1, PdfIntNumber? n2)
    {
        if (ReferenceEquals(n1, n2)) { return true; }
        if (n1 is null) { return false; }
        if (n2 is null) { return false; }
        return n1.Equals(n2);
    }

    public static bool operator !=(PdfIntNumber? n1, PdfIntNumber? n2) => !(n1 == n2);

    public override string ToString()
    {
        return Value.ToString();
    }
}

/// <summary>
/// Pdf number object represented by a c# long
/// </summary>
public class PdfLongNumber : PdfNumber, IEquatable<PdfLongNumber>
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
    public override bool Equals(object? obj)
    {
        if (obj == null) { return false; }
        return obj is PdfLongNumber num && Value.Equals(num?.Value);
    }

    public virtual bool Equals(PdfLongNumber? other)
    {
        return Value.Equals(other?.Value);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public static bool operator ==(PdfLongNumber? n1, PdfLongNumber? n2)
    {
        if (ReferenceEquals(n1, n2)) { return true; }
        if (n1 is null) { return false; }
        if (n2 is null) { return false; }
        return n1.Equals(n2);
    }

    public static bool operator !=(PdfLongNumber? n1, PdfLongNumber? n2) => !(n1 == n2);

    public override string ToString()
    {
        return Value.ToString();
    }
}

/// <summary>
/// Pdf number object represented by a c# double
/// </summary>
public class PdfDoubleNumber : PdfNumber, IEquatable<PdfDoubleNumber>
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
    public override bool Equals(object? obj)
    {
        if (obj == null) { return false; }
        return obj is PdfLongNumber num && Value.Equals(num?.Value);
    }

    public virtual bool Equals(PdfDoubleNumber? other)
    {
        return Value.Equals(other?.Value);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public static bool operator ==(PdfDoubleNumber? n1, PdfDoubleNumber? n2)
    {
        if (ReferenceEquals(n1, n2)) { return true; }
        if (n1 is null) { return false; }
        if (n2 is null) { return false; }
        return n1.Equals(n2);
    }

    public static bool operator !=(PdfDoubleNumber? n1, PdfDoubleNumber? n2) => !(n1 == n2);

    public override string ToString()
    {
        return Value.ToString();
    }
}

/// <summary>
/// Pdf number object represented by a c# decimal
/// </summary>
public class PdfDecimalNumber : PdfNumber, IEquatable<PdfDecimalNumber>
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

    public override bool Equals(object? obj)
    {
        if (obj == null) { return false; }
        return obj is PdfLongNumber num && Value.Equals(num?.Value);
    }

    public virtual bool Equals(PdfDecimalNumber? other)
    {
        return Value.Equals(other?.Value);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public static bool operator ==(PdfDecimalNumber? n1, PdfDecimalNumber? n2)
    {
        if (ReferenceEquals(n1, n2)) { return true; }
        if (n1 is null) { return false; }
        if (n2 is null) { return false; }
        return n1.Equals(n2);
    }

    public static bool operator !=(PdfDecimalNumber? n1, PdfDecimalNumber? n2) => !(n1 == n2);

    public override string ToString()
    {
        return Value.ToString();
    }
}
