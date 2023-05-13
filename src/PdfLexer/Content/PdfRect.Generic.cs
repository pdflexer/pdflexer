﻿using PdfLexer.DOM;
using PdfLexer.Lexing;
using System.Buffers.Text;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace PdfLexer.Content;

public record class PdfRect : PdfRect<double> { }
public record class PdfRect<T> where T : IFloatingPoint<T>
{
    public required T LLx { get; init; }
    public required T LLy { get; init; }
    public required T URx { get; init; }
    public required T URy { get; init; }

    public T Width() => URx - LLx;
    public T Height() => URy - LLy;

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
