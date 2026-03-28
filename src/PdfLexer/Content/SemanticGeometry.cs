using PdfLexer.Fonts;

namespace PdfLexer.Content;

internal readonly struct SemanticGlyphMetrics
{
    public SemanticGlyphMetrics(Glyph glyph, bool isVertical)
    {
        W0 = glyph.w0;
        W1 = glyph.w1;
        IsWordSpace = glyph.IsWordSpace;
        IsVertical = isVertical;
        BBox = glyph.BBox == null ? null : (decimal[])glyph.BBox.Clone();
    }

    public float W0 { get; }
    public float W1 { get; }
    public bool IsWordSpace { get; }
    public bool IsVertical { get; }
    public decimal[]? BBox { get; }
}

internal readonly struct SemanticGlyphSnapshot
{
    public SemanticGlyphSnapshot(GfxMatrix<double> textRenderingMatrix, SemanticGlyphMetrics metrics, double fontSize)
    {
        TextRenderingMatrix = textRenderingMatrix;
        Metrics = metrics;
        FontSize = fontSize;
        Rotation = textRenderingMatrix.GetRotation();
        Origin = new PdfPoint<double> { X = textRenderingMatrix.E, Y = textRenderingMatrix.F };
        var (inlineX, inlineY) = GetAxis(textRenderingMatrix, metrics.IsVertical);
        InlineAxisX = inlineX;
        InlineAxisY = inlineY;
        PerpendicularAxisX = -inlineY;
        PerpendicularAxisY = inlineX;
        InlineAdvance = GetInlineAdvance(textRenderingMatrix, metrics);
    }

    public GfxMatrix<double> TextRenderingMatrix { get; }
    public SemanticGlyphMetrics Metrics { get; }
    public double FontSize { get; }
    public double Rotation { get; }
    public PdfPoint<double> Origin { get; }
    public double InlineAxisX { get; }
    public double InlineAxisY { get; }
    public double PerpendicularAxisX { get; }
    public double PerpendicularAxisY { get; }
    public double InlineAdvance { get; }

    public PdfRect<double> CreateBoundingBox()
    {
        double x = 0d;
        double y = 0d;
        double x2 = Metrics.W0;
        double y2 = 0d;
        if (Metrics.BBox != null)
        {
            x = (double)Metrics.BBox[0];
            y = (double)Metrics.BBox[1];
            x2 = (double)Metrics.BBox[2];
            y2 = (double)Metrics.BBox[3];
        }

        var bl = GfxMatrix<double>.Identity with { E = x, F = y } * TextRenderingMatrix;
        var tr = GfxMatrix<double>.Identity with { E = x2, F = y2 } * TextRenderingMatrix;

        return new PdfRect<double>
        {
            LLx = Math.Min(bl.E, tr.E),
            LLy = Math.Min(bl.F, tr.F),
            URx = Math.Max(bl.E, tr.E),
            URy = Math.Max(bl.F, tr.F)
        };
    }

    public double InlineCoordinate(PdfPoint<double> point)
    {
        return point.X * InlineAxisX + point.Y * InlineAxisY;
    }

    public double PerpendicularCoordinate(PdfPoint<double> point)
    {
        return point.X * PerpendicularAxisX + point.Y * PerpendicularAxisY;
    }

    private static (double X, double Y) GetAxis(GfxMatrix<double> matrix, bool isVertical)
    {
        var x = isVertical ? matrix.C : matrix.A;
        var y = isVertical ? matrix.D : matrix.B;
        var len = Math.Sqrt((x * x) + (y * y));
        if (len == 0d)
        {
            return (1d, 0d);
        }

        return (x / len, y / len);
    }

    private static double GetInlineAdvance(GfxMatrix<double> matrix, SemanticGlyphMetrics metrics)
    {
        var width = metrics.IsVertical ? metrics.W1 : metrics.W0;
        var x = metrics.IsVertical ? matrix.C : matrix.A;
        var y = metrics.IsVertical ? matrix.D : matrix.B;
        var scale = Math.Sqrt((x * x) + (y * y));
        if (scale == 0d)
        {
            scale = 1d;
        }

        return Math.Abs(width * scale);
    }
}

internal static class SemanticBounds
{
    public static PdfRect<double> Union(IReadOnlyList<SemanticCharacter> characters)
    {
        if (characters.Count == 0)
        {
            return new PdfRect<double> { LLx = 0d, LLy = 0d, URx = 0d, URy = 0d };
        }

        var first = characters[0].BoundingBox;
        var llx = first.LLx;
        var lly = first.LLy;
        var urx = first.URx;
        var ury = first.URy;
        for (var i = 1; i < characters.Count; i++)
        {
            var box = characters[i].BoundingBox;
            llx = Math.Min(llx, box.LLx);
            lly = Math.Min(lly, box.LLy);
            urx = Math.Max(urx, box.URx);
            ury = Math.Max(ury, box.URy);
        }

        return new PdfRect<double> { LLx = llx, LLy = lly, URx = urx, URy = ury };
    }
}
