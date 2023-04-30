using PdfLexer.DOM;
using PdfLexer.Lexing;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using static DotNext.Generic.BooleanConst;

namespace PdfLexer.Content;





#if NET7_0_OR_GREATER


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
        return Unsafe.As<T, double>(ref value);
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

    public void Write<T>(T value, Stream stream) where T : IFloatingPoint<T>
    {
        var val = Unsafe.As<T, decimal>(ref value);
        Span<byte> buffer = stackalloc byte[35];
        if (!Utf8Formatter.TryFormat(val, buffer, out int bytes))
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

    public void Write<T>(T value, Stream stream) where T : IFloatingPoint<T>
    {
        var val = Unsafe.As<T, double>(ref value);
        Span<byte> buffer = stackalloc byte[35];
        if (!Utf8Formatter.TryFormat(val, buffer, out int bytes))
        {
            throw new ApplicationException("TODO: Unable to write double: " + val.ToString());
        }
        stream.Write(buffer[..bytes]);

        // TODO round / truncate?
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
        throw new NotImplementedException("Converter for FloatingPoint: " + typeof(T));
    }

    public static IFloatingPointConverter Util { get; } = Create();

    public static T V1000 { get; } = Util.FromDecimal<T>(1000m);
    public static T V100 { get; } = Util.FromDecimal<T>(100m);
    public static T V180 { get; } = Util.FromDecimal<T>(180m);
    public static T V2 { get; } = Util.FromDecimal<T>(2m);
    public static T V0_5 { get; } = Util.FromDecimal<T>(0.5m);
}

public record class PdfRect : PdfRect<double> { }
public record class PdfRect<T> where T : IFloatingPoint<T>
{
    public required T LLx { get; init; }
    public required T LLy { get; init; }
    public required T URx { get; init; }
    public required T URy { get; init; }

    public bool Intersects(PdfRect<T> rect)
    {
        if (rect.LLx > URx) return false;
        if (rect.LLy > URy) return false;
        if (rect.URx < LLx) return false;
        if (rect.URy < LLy) return false;
        return true;
    }

    public EncloseType CheckEnclosure(PdfRect<T> rect)
    {
        if (rect.LLx > URx || rect.LLy > URy || rect.URx < LLx || rect.URy < LLy) return EncloseType.None;
        if (rect.LLx < LLx || rect.LLy < LLy || rect.URx > URx || rect.URy > URy) return EncloseType.Partial;
        return EncloseType.Full;
    }


    public PdfRect<T> NormalizeTo(PdfPage pg) => NormalizeTo(pg.CropBox);

    public PdfRect<T> NormalizeTo(PdfRectangle rect)
    {
        var tr = Convert(rect);
        var x = T.Min(tr.LLx, tr.URx);
        var y = T.Min(tr.LLy, tr.URy);
        if (x == T.Zero && y == T.Zero) { return this; }
        return new PdfRect<T> { LLx = LLx - x, LLy = LLy - y, URx = URx - x, URy = URy - y };
    }

    public static PdfRect<T> Convert(PdfRectangle rect)
    {
        var x1 = FPC<T>.Util.FromDecimal<T>(rect.LLx);
        var x2 = FPC<T>.Util.FromDecimal<T>(rect.URx);
        var y1 = FPC<T>.Util.FromDecimal<T>(rect.LLy);
        var y2 = FPC<T>.Util.FromDecimal<T>(rect.URy);
        return new PdfRect<T> { LLx = x1, LLy = y1, URx = x2, URy = y2 };
    }
}

#else

public record class PdfRect
{
    public required decimal LLx { get; init; }
    public required decimal LLy { get; init; }
    public required decimal URx { get; init; }
    public required decimal URy { get; init; }

    public bool Intersects(PdfRect rect)
    {
        if (rect.LLx > URx) return false;
        if (rect.LLy > URy) return false;
        if (rect.URx < LLx) return false;
        if (rect.URy < LLy) return false;
        return true;
    }

    public EncloseType CheckEnclosure(PdfRect rect)
    {
        if (rect.LLx > URx || rect.LLy > URy || rect.URx < LLx || rect.URy < LLy) return EncloseType.None;
        if (rect.LLx < LLx || rect.LLy < LLy || rect.URx > URx || rect.URy > URy) return EncloseType.Partial;
        return EncloseType.Full;
    }


    public PdfRect Normalize(PdfPage pg) => Normalize(pg.CropBox);

    public PdfRect Normalize(PdfRectangle rect)
    {
        var x = Math.Min((decimal)rect.LLx, (decimal)rect.URx);
        var y = Math.Min((decimal)rect.LLy, (decimal)rect.URy);
        if (x == 0 && y == 0) { return this; }
        return new PdfRect { LLx = LLx - x, LLy = LLy - y, URx = URx - x, URy = URy - y };
    }
}

#endif


public enum EncloseType
{
    Full,
    Partial,
    None
}

internal enum ContentType
{
    Text,
    Paths,
    Image,
    Form,
    Shading,
    // MarkedPoint
}
