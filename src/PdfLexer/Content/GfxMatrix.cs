using PdfLexer.Content.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer.Content;


public record struct GfxMatrix
{
    public GfxMatrix()
    {

    }
    public GfxMatrix(decimal a, decimal b, decimal c, decimal d, decimal e, decimal f)
    {
        A = a; B = b; C = c; D = d; E = e; F = f;
    }
    public static readonly GfxMatrix Identity = new GfxMatrix
    {
        A = 1,
        B = 0,
        C = 0,
        D = 1,
        E = 0,
        F = 0
    };
    public bool IsIdentity { get => this == Identity; }

    public decimal A { get; init; }
    public decimal B { get; init; }
    public decimal C { get; init; }
    public decimal D { get; init; }
    public decimal E { get; init; }
    public decimal F { get; init; }

    public GfxMatrix Multiply(GfxMatrix matrix)
    {
        var a = (A * matrix.A) + (B * matrix.C);
        var b = (A * matrix.B) + (B * matrix.D);
        var c = (C * matrix.A) + (D * matrix.C);
        var d = (C * matrix.B) + (D * matrix.D);
        var e = (E * matrix.A) + (F * matrix.C) + (matrix.E);
        var f = (E * matrix.B) + (F * matrix.D) + (matrix.F);
        return new GfxMatrix { A = a, B = b, C = c, D = d, E = e, F = f };
    }

    public bool Invert(out GfxMatrix matrix)
    {
        var det = (A * D) - (C * B);

        if (det == 0)
        {
            matrix = default;
            return false;
        }

        var invDet = 1m / det;

        matrix = new GfxMatrix
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

    public GfxMatrix Round(int decimals = 8)
    {
        return this with
        {
            A = Math.Round(A, decimals),
            B = Math.Round(B, decimals),
            C = Math.Round(C, decimals),
            D = Math.Round(D, decimals),
            E = Math.Round(E, decimals),
            F = Math.Round(F, decimals),
        };
    }

    /// <summary>
    /// Transforms the rectangle to it's representation
    /// in the current matrix.
    /// Note: in cases where the current matrix has skew
    /// the transformed rectangle is a parallelogram
    /// and this method returns a rectangle based on it's
    /// bounding box.
    /// </summary>
    /// <param name="rect"></param>
    /// <returns></returns>
    internal PdfRect GetTransformedBoundingBox(PdfRect rect)
    {
        if (B != 0 || C != 0)
        {
            var x1 = A * rect.LLx + C * rect.LLy + E;
            var y1 = B * rect.LLx + D * rect.LLy + F;
            var x2 = A * rect.URx + C * rect.URy + E;
            var y2 = B * rect.URx + D * rect.URy + F;
            var x11 = A * rect.LLx + C * rect.URy + E; // top left
            var y11 = B * rect.LLx + D * rect.URy + F; // top left
            var x22 = A * rect.URx + C * rect.LLy + E; // lower right
            var y22 = B * rect.URx + D * rect.LLy + F; // lower right
            x1 = Math.Min(x1, x11);
            y1 = Math.Min(y1, y11);
            x2 = Math.Max(x2, x22);
            y2 = Math.Max(y2, y22);
            return new PdfRect
            {
                LLx = Math.Min(x1, x2),
                LLy = Math.Min(y1, y2),
                URx = Math.Max(x1, x2),
                URy = Math.Max(y1, y2)
            };
        }
        else
        {
            var x1 = A * rect.LLx + E;
            var y1 = D * rect.LLy + F;
            var x2 = A * rect.URx + E;
            var y2 = D * rect.URy + F;
            return new PdfRect
            {
                LLx = x1,
                LLy = y1,
                URx = x2,
                URy = y2,
            };
        }

    }


    /// <summary>
    /// Multiplies two matrices together and returns the resulting matrix.
    /// </summary>
    /// <param name="value1">The first source matrix.</param>
    /// <param name="value2">The second source matrix.</param>
    /// <returns>The product matrix.</returns>
    public static GfxMatrix operator *(GfxMatrix value1, GfxMatrix value2)
    {

        var a = (value1.A * value2.A) + (value1.B * value2.C);
        var b = (value1.A * value2.B) + (value1.B * value2.D);
        var c = (value1.C * value2.A) + (value1.D * value2.C);
        var d = (value1.C * value2.B) + (value1.D * value2.D);
        var e = (value1.E * value2.A) + (value1.F * value2.C) + (value2.E);
        var f = (value1.E * value2.B) + (value1.F * value2.D) + (value2.F);
        return new GfxMatrix { A = a, B = b, C = c, D = d, E = e, F = f };
    }
}
