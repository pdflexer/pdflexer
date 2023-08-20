using PdfLexer.Lexing;
using System.Buffers;
using System.Buffers.Text;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace PdfLexer.Content;

internal interface IFloatingPointConverter
{
    decimal ToDecimal<T>(T value) where T : IFloatingPoint<T>;
    double ToDouble<T>(T value) where T : IFloatingPoint<T>;
    T FromDecimal<T>(decimal value) where T : IFloatingPoint<T>;
    T FromDouble<T>(double value) where T : IFloatingPoint<T>;
    T FromPdfNumber<T>(PdfNumber value) where T : IFloatingPoint<T>;
    void Write<T>(T value, Stream stream) where T : IFloatingPoint<T>;
    T Parse<T>(ParsingContext ctx, ReadOnlySpan<byte> data, OperandInfo op);
}

internal class DecimalFPConverter : IFloatingPointConverter
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public decimal ToDecimal<T>(T value) where T : IFloatingPoint<T>
    {
        return Unsafe.As<T, decimal>(ref value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double ToDouble<T>(T value) where T : IFloatingPoint<T>
    {
        var val = Unsafe.As<T, decimal>(ref value);
        return (double)val;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T FromDecimal<T>(decimal value) where T : IFloatingPoint<T>
    {
        return Unsafe.As<decimal, T>(ref value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T FromDouble<T>(double value) where T : IFloatingPoint<T>
    {
        var dec = (decimal)value;
        return Unsafe.As<decimal, T>(ref dec);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T FromPdfNumber<T>(PdfNumber value) where T : IFloatingPoint<T>
    {
        var val = (decimal)value;
        return Unsafe.As<decimal, T>(ref val);
    }


    private static StandardFormat fmt = new StandardFormat('F', 10);
    public void Write<T>(T value, Stream stream) where T : IFloatingPoint<T>
    {
        var val = Unsafe.As<T, decimal>(ref value);
        Span<byte> buffer = stackalloc byte[35];
        if (!Utf8Formatter.TryFormat(val, buffer, out int bytes, fmt))
        {
            throw new ApplicationException("TODO: Unable to write decimal: " + val.ToString());
        }
        if (buffer.IndexOf((byte)'.') > -1)
        {
            for (; bytes > 0; bytes--)
            {
                var b = buffer[bytes - 1];
                if (b != (byte)'0')
                {
                    if (b == (byte)'.')
                    {
                        bytes--;
                    }
                    break;
                }
            }
        }
        stream.Write(buffer[..bytes]);
    }

    public T Parse<T>(ParsingContext ctx, ReadOnlySpan<byte> data, OperandInfo op)
    {
        if (!Utf8Parser.TryParse(data.Slice(op.StartAt, op.Length), out decimal val, out _))
        {
            ctx.Error("Bad decimal found in content stream: " + Encoding.ASCII.GetString(data.Slice(op.StartAt, op.Length)));
        }
        return Unsafe.As<decimal, T>(ref val);
    }
}

// TODO -> inlining probably does nothing the way it's used

internal class DoubleFPConverter : IFloatingPointConverter
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public decimal ToDecimal<T>(T value) where T : IFloatingPoint<T>
    {
        var db = Unsafe.As<T, double>(ref value);
        return (decimal)db;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T FromDecimal<T>(decimal value) where T : IFloatingPoint<T>
    {
        var db = (double)value;
        return Unsafe.As<double, T>(ref db);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double ToDouble<T>(T value) where T : IFloatingPoint<T>
    {
        return Unsafe.As<T, double>(ref value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T FromDouble<T>(double value) where T : IFloatingPoint<T>
    {
        return Unsafe.As<double, T>(ref value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T FromPdfNumber<T>(PdfNumber value) where T : IFloatingPoint<T>
    {
        var val = (double)value;
        return Unsafe.As<double, T>(ref val);
    }
    private static StandardFormat fmt = new StandardFormat('F', 10);
    public void Write<T>(T value, Stream stream) where T : IFloatingPoint<T>
    {
        var val = Unsafe.As<T, double>(ref value);
        Span<byte> buffer = stackalloc byte[35];
        if (!Utf8Formatter.TryFormat(val, buffer, out int bytes, fmt))
        {
            throw new ApplicationException("TODO: Unable to write double: " + val.ToString());
        }
        if (buffer.IndexOf((byte)'.') > -1)
        {
            for (; bytes > 0; bytes--)
            {
                var b = buffer[bytes - 1];
                if (b != (byte)'0')
                {
                    if (b == (byte)'.')
                    {
                        bytes--;
                    }
                    break;
                }
            }
        }
        stream.Write(buffer[..bytes]);
    }

    public T Parse<T>(ParsingContext ctx, ReadOnlySpan<byte> data, OperandInfo op)
    {
        if (!Utf8Parser.TryParse(data.Slice(op.StartAt, op.Length), out double val, out _))
        {
            ctx.Error("Bad decimal found in content stream: " + Encoding.ASCII.GetString(data.Slice(op.StartAt, op.Length)));
        }
        return Unsafe.As<double, T>(ref val);
    }
}
internal class FloatFPConverter : IFloatingPointConverter
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public decimal ToDecimal<T>(T value) where T : IFloatingPoint<T>
    {
        var db = Unsafe.As<T, float>(ref value);
        return (decimal)db;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T FromDecimal<T>(decimal value) where T : IFloatingPoint<T>
    {
        var db = (float)value;
        return Unsafe.As<float, T>(ref db);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double ToDouble<T>(T value) where T : IFloatingPoint<T>
    {
        var fl = Unsafe.As<T, float>(ref value);
        return (double)fl;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T FromDouble<T>(double value) where T : IFloatingPoint<T>
    {
        var fl = (float)value;
        return Unsafe.As<float, T>(ref fl);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T FromPdfNumber<T>(PdfNumber value) where T : IFloatingPoint<T>
    {
        var val = (float)value;
        return Unsafe.As<float, T>(ref val);
    }

    private static StandardFormat fmt = new StandardFormat('F', 10);
    public void Write<T>(T value, Stream stream) where T : IFloatingPoint<T>
    {
        var val = Unsafe.As<T, float>(ref value);
        Span<byte> buffer = stackalloc byte[35];
        if (!Utf8Formatter.TryFormat(val, buffer, out int bytes, fmt))
        {
            throw new ApplicationException("TODO: Unable to write double: " + val.ToString());
        }
        if (buffer.IndexOf((byte)'.') > -1)
        {
            for (; bytes > 0; bytes--)
            {
                var b = buffer[bytes - 1];
                if (b != (byte)'0')
                {
                    if (b == (byte)'.')
                    {
                        bytes--;
                    }
                    break;
                }
            }
        }
        stream.Write(buffer[..bytes]);
    }

    public T Parse<T>(ParsingContext ctx, ReadOnlySpan<byte> data, OperandInfo op)
    {
        if (!Utf8Parser.TryParse(data.Slice(op.StartAt, op.Length), out float val, out _))
        {
            ctx.Error("Bad decimal found in content stream: " + Encoding.ASCII.GetString(data.Slice(op.StartAt, op.Length)));
        }
        return Unsafe.As<float, T>(ref val);
    }
}

internal static class FPC<T> where T : IFloatingPoint<T>
{
    private static IFloatingPointConverter Create()
    {
        if (typeof(T) == typeof(decimal))
        {
            return new DecimalFPConverter();
        }
        else if (typeof(T) == typeof(double))
        {
            return new DoubleFPConverter();
        }
        else if (typeof(T) == typeof(float))
        {
            return new FloatFPConverter();
        }
        throw new NotImplementedException("Converter for FloatingPoint: " + typeof(T));
    }

    public static IFloatingPointConverter Util { get; } = Create();

    public static T V1000 { get; } = Util.FromDecimal<T>(1000m);
    public static T V100 { get; } = Util.FromDecimal<T>(100m);
    public static T V180 { get; } = Util.FromDecimal<T>(180m);
    public static T V2 { get; } = Util.FromDecimal<T>(2m);
    public static T V0_5 { get; } = Util.FromDecimal<T>(0.5m);
}