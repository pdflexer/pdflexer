using PdfLexer.DOM;
using System;
using System.Numerics;

namespace PdfLexer.Content;

public record struct PdfPoint<T> where T : struct, IFloatingPoint<T>
{
    public PdfPoint()
    {

    }

    [SetsRequiredMembers]
    public PdfPoint(T x, T y)
    {
        X = x;
        Y = y;
    }
    public required T X { get; init; }
    public required T Y { get; init; }

    public bool Intersects(PdfRect<T> rect)
    {
        if (rect.LLx > X) return false;
        if (rect.LLy > Y) return false;
        if (rect.URx < X) return false;
        if (rect.URy < Y) return false;
        return true;
    }

    public PdfPoint<T> NormalizeTo(PdfPage pg) => NormalizeTo(pg.CropBox);

    public PdfPoint<T> NormalizeToTopLeft(PdfPage pg) => NormalizeToTopLeft(pg.CropBox);

    public PdfPoint<T> NormalizeTo(PdfRectangle rect)
    {
        var tr = rect.ToContentModel<T>();
        var x = T.Min(tr.LLx, tr.URx);
        var y = T.Min(tr.LLy, tr.URy);
        if (x == T.Zero && y == T.Zero) { return this; }
        return new PdfPoint<T> { X = X - x, Y = Y - y };
    }

    public PdfPoint<T> NormalizeToTopLeft(PdfRectangle rect)
    {
        var tr = rect.ToContentModel<T>();
        var x = T.Min(tr.LLx, tr.URx);
        var y = T.Max(tr.LLy, tr.URy);
        return new PdfPoint<T> { X = X - x, Y = Y + y };
    }

    public override string ToString()
    {
        return $"({X}, {Y})";
    }

    
}
public static class PdfPoint
{
    public static PdfPoint<T> Create<T>(T x, T y) where T : struct, IFloatingPoint<T>
    {
        return new PdfPoint<T> { X = x , Y = y };
    }

    public static PdfPoint<double> Create(int x, int y)
    {
        return new PdfPoint<double> { X = x, Y = y };
    }
}
