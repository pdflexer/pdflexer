using System;
using System.Collections.Generic;

using System.Text;

namespace PdfLexer.Content;

using PdfLexer.Fonts;

#if NET7_0_OR_GREATER

using System.Numerics;

internal class GfxState2<T> where T : struct, IFloatingPoint<T>
{
    public GfxState2()
    {
        CTM = GfxMatrix<T>.Identity;
        Text = new TxtState();
        // UpdateTRM();
    }
    
    internal GfxState? Prev { get; init; }
    internal IReadableFont? Font { get; init; }

    public GfxMatrix<T> CTM { get; init; }

    public int TextMode { get; init; }
    public T FontSize { get; init; }
    public T TextHScale { get; init; } = T.One;
    public T TextRise { get; init; }
    public T CharSpacing { get; init; }
    public T WordSpacing { get; init; }
    public T TextLeading { get; init; }
    public T LineWidth { get; init; } = T.One;
    public int LineCap { get; init; }
    public int LineJoin { get; init; }
    private static T ten = T.One + T.One + T.One + T.One + T.One + T.One + T.One + T.One + T.One + T.One; // gotta be better way
    public T MiterLimit { get; init; } = ten;
    public T Flatness { get; init; } = T.One;
    public d_Op? Dashing { get; init; }
    public PdfName? RenderingIntent { get; init; }
    public IPdfObject? ColorSpace { get; init; } = PdfName.DeviceGray;
    public IPdfObject? ColorSpaceStroking { get; init; } = PdfName.DeviceGray;
    public IPdfOperation? Color { get; init; }
    public IPdfOperation? ColorStroking { get; init; }
    public PdfDictionary? FontObject { get; init; }
    public PdfDictionary? ExtDict { get; init; }
    internal List<ClippingInfo>? Clipping { get; init; }

    // not part of real gfx state
    internal TxtState Text { get; init; }

    internal GfxMatrix<T> GetTranslation(GfxMatrix<T> desired)
    {
        CTM.Invert(out var iv);
        return desired * iv;
    }
}


internal record struct GfxMatrix<T> where T : struct, IFloatingPoint<T>
{
    public static readonly GfxMatrix<T> Identity = new GfxMatrix<T>
    {
        A = T.One,
        B = T.Zero,
        C = T.Zero,
        D = T.One,
        E = T.Zero,
        F = T.Zero
    };
    public T A { get; init; }
    public T B { get; init; }
    public T C { get; init; }
    public T D { get; init; }
    public T E { get; init; }
    public T F { get; init; }

    public GfxMatrix<T> Multiply(GfxMatrix<T> matrix)
    {
        var a = (A * matrix.A) + (B * matrix.C);
        var b = (A * matrix.B) + (B * matrix.D);
        var c = (C * matrix.A) + (D * matrix.C);
        var d = (C * matrix.B) + (D * matrix.D);
        var e = (E * matrix.A) + (F * matrix.C) + (matrix.E);
        var f = (E * matrix.B) + (F * matrix.D) + (matrix.F);
        return new GfxMatrix<T> { A = a, B = b, C = c, D = d, E = e, F = f };
    }

    public bool Invert(out GfxMatrix<T> matrix)
    {
        var det = (A * D) - (C * B);

        if (T.IsZero(det))
        {
            matrix = default;
            return false;
        }

        var invDet = T.One / det;

        matrix = new GfxMatrix<T>
        {
            A = D * invDet,
            B = -B * invDet,
            C = -C * invDet,
            D = A * invDet,
            E = (C * F - E * D) * invDet,
            F = (E * B - A * F) * invDet
        };
        return true;
    }

    /// <summary>
    /// Multiplies two matrices together and returns the resulting matrix.
    /// </summary>
    /// <param name="value1">The first source matrix.</param>
    /// <param name="value2">The second source matrix.</param>
    /// <returns>The product matrix.</returns>
    public static GfxMatrix<T> operator *(GfxMatrix<T> value1, GfxMatrix<T> value2)
    {
        
        var a = (value1.A * value2.A) + (value1.B * value2.C);
        var b = (value1.A * value2.B) + (value1.B * value2.D);
        var c = (value1.C * value2.A) + (value1.D * value2.C);
        var d = (value1.C * value2.B) + (value1.D * value2.D);
        var e = (value1.E * value2.A) + (value1.F * value2.C) + (value2.E);
        var f = (value1.E * value2.B) + (value1.F * value2.D) + (value2.F);
        return new GfxMatrix<T> { A = a, B = b, C = c, D = d, E = e, F = f };
    }
}

#endif