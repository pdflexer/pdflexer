using System.Numerics;

namespace PdfLexer.Content;


public record struct GfxMatrix<T> where T : struct, IFloatingPoint<T>
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

    public GfxMatrix()
    {

    }

    public GfxMatrix(T a, T b, T c, T d, T e, T f)
    {
        A = a; B = b; C = c; D = d; E = e; F = f;
    }

    public T A { get; init; }
    public T B { get; init; }
    public T C { get; init; }
    public T D { get; init; }
    public T E { get; init; }
    public T F { get; init; }

    public bool IsIdentity { get => this == Identity; }

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


    public GfxMatrix<T> Round(int decimals = 8)
    {
        return this with
        {
            A = T.Round(A, decimals),
            B = T.Round(B, decimals),
            C = T.Round(C, decimals),
            D = T.Round(D, decimals),
            E = T.Round(E, decimals),
            F = T.Round(F, decimals),
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
    internal PdfRect<T> GetTransformedBoundingBox(PdfRect<T> rect)
    {
        if (B != T.Zero || C != T.Zero)
        {
            var x1 = A * rect.LLx + C * rect.LLy + E;
            var y1 = B * rect.LLx + D * rect.LLy + F;
            var x2 = A * rect.URx + C * rect.URy + E;
            var y2 = B * rect.URx + D * rect.URy + F;
            var x11 = A * rect.LLx + C * rect.URy + E; // top left
            var y11 = B * rect.LLx + D * rect.URy + F; // top left
            var x22 = A * rect.URx + C * rect.LLy + E; // lower right
            var y22 = B * rect.URx + D * rect.LLy + F; // lower right
            x1 = T.Min(x1, x11);
            y1 = T.Min(y1, y11);
            x2 = T.Max(x2, x22);
            y2 = T.Max(y2, y22);
            return new PdfRect<T>
            {
                LLx = T.Min(x1, x2),
                LLy = T.Min(y1, y2),
                URx = T.Max(x1, x2),
                URy = T.Max(y1, y2)
            };
        }
        else
        {
            var x1 = A * rect.LLx + E;
            var y1 = D * rect.LLy + F;
            var x2 = A * rect.URx + E;
            var y2 = D * rect.URy + F;
            return new PdfRect<T>
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
