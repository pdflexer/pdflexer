using PdfLexer.Content.Model;
using PdfLexer.DOM.ColorSpaces;
using PdfLexer.Fonts;

using System.Numerics;

namespace PdfLexer.Content;

public record GfxState<T> where T : struct, IFloatingPoint<T>
{
    public GfxState()
    {
        CTM = GfxMatrix<T>.Identity;
        Text = new TxtState<T>();
        // UpdateTRM();
    }

    internal GfxState<T>? Prev { get; init; }
    internal IReadableFont? Font { get; init; }
    internal PdfName? FontResourceName { get; init; }

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
    public d_Op<T>? Dashing { get; init; }
    public PdfName? RenderingIntent { get; init; }


    // TODO color model and replace existing
    internal IColorSpace ColorSpaceModel { get; init; } = DeviceGray.Instance;
    internal IColorSpace ColorSpaceStrokingModel { get; init; } = DeviceGray.Instance;
    internal IColor<T> ColorModel { get; init; } = DeviceGray.GetBlack<T>();
    internal IColor<T> ColorModelStroking { get; init; } = DeviceGray.GetBlack<T>();


    public IPdfObject? ColorSpace { get; init; } = PdfName.DeviceGray;
    public IPdfObject? ColorSpaceStroking { get; init; } = PdfName.DeviceGray;
    public IPdfOperation? Color { get; init; }
    public IPdfOperation? ColorStroking { get; init; }
    public PdfDictionary? FontObject { get; init; }
    public ExtGraphicsDict<T>? ExtDict { get; init; }
    internal List<IClippingSection<T>>? Clipping { get; init; }

    // not part of real gfx state
    internal TxtState<T> Text { get; init; }

    internal GfxMatrix<T> GetTranslation(GfxMatrix<T> desired)
    {
        CTM.Invert(out var iv);
        return desired * iv;
    }

    internal void UpdateTRM()
    {
        Text.TextRenderingMatrix = new GfxMatrix<T>(
          FontSize * TextHScale, T.Zero,
          T.Zero, FontSize,
          T.Zero, TextRise) * Text.TextMatrix * CTM;
    }

    internal PdfRect<T> GetGlyphBoundingBox(Glyph glyph)
    {

        T x = T.Zero, y = T.Zero, x2 = FPC<T>.Util.FromDecimal<T>((decimal)glyph.w0), y2 = T.Zero;
        if (glyph.BBox != null)
        {
            // TODO : should this be moved to fonts to decide fallback?
            x = FPC<T>.Util.FromPdfNumber<T>(glyph.BBox[0]);
            y = FPC<T>.Util.FromPdfNumber<T>(glyph.BBox[1]);
            x2 = FPC<T>.Util.FromPdfNumber<T>(glyph.BBox[2]);
            y2 = FPC<T>.Util.FromPdfNumber<T>(glyph.BBox[3]);
        }
        var bl = GfxMatrix<T>.Identity with { E = x, F = y } * Text.TextRenderingMatrix;

        var tr = GfxMatrix<T>.Identity with { E = x2, F = y2 } * Text.TextRenderingMatrix;

        return new PdfRect<T> { LLx = bl.E, LLy = bl.F, URx = tr.E, URy = tr.F };
    }

    internal void ShiftTextMatrix(T tx, T ty)
    {
        // Tm = [ 1  0  0 ] x Tm
        //        0  1  0
        //        tx ty 1

        Text.TextMatrix = GfxMatrix<T>.Identity with { E = tx, F = ty } * Text.TextMatrix;

        UpdateTRM();
    }

    internal void ShiftTextAndLineMatrix(T tx, T ty)
    {
        // Tm = Tlm = [ 1  0  0 ] x Tlm
        //              0  1  0
        //              tx ty 1

        Text.TextLineMatrix = GfxMatrix<T>.Identity with { E = tx, F = ty } * Text.TextLineMatrix;
        Text.TextMatrix = Text.TextLineMatrix;
        UpdateTRM();
    }


    internal void ApplyData(ReadOnlySpan<byte> data)
    {
        var font = Font;
        if (font == null)
        {
            font = SingleByteFont.Fallback;
            /// Ctx.Error("Font data before font set, falling back to helvetica");
        }

        var i = 0;
        while (i < data.Length)
        {
            i += font.GetGlyph(data, i, out var glyph);
            Apply(glyph);
        }
    }

    internal void Apply(Glyph info)
    {
        // shift
        T tx = T.Zero;
        T ty = T.Zero;
        if (!(Font?.IsVertical ?? false))
        {
            // tx = ((w0-Tj/1000) * T_fs + T_c + T_w?) * Th
            // var s = (info.w0 - tj / 1000) * FontSize + CharSpacing; // Tj pre applied
            var w0 = FPC<T>.Util.FromDouble<T>(info.w0);
            var s = w0 * FontSize + CharSpacing;
            if (info.IsWordSpace)
            {
                s += WordSpacing;
            }
            tx = s * TextHScale;
        }
        else
        {
            // ty = (w1-Tj/1000) * T_fs + T_c + T_w?)
            var w1 = FPC<T>.Util.FromDouble<T>(info.w1);
            var s = w1 * FontSize + CharSpacing;
            if (info.IsWordSpace) { s += WordSpacing; }
            ty = s;
        }

        ShiftTextMatrix(tx, ty);
    }

    internal void ApplyTj(T tj)
    {
        if (tj == T.Zero) { return; }
        T tx = T.Zero;
        T ty = T.Zero;
        if (!(Font?.IsVertical ?? false))
        {
            tx = (-tj / FPC<T>.V1000) * FontSize * TextHScale;
        }
        else
        {
            var s = (-tj / FPC<T>.V1000) * FontSize;
            ty = s;
        }

        ShiftTextMatrix(tx, ty);
    }

    internal void ApplyShift(GlyphOrShift<T> glyph)
    {
        ApplyTj(glyph.Shift);
    }

    internal void ApplyCharShift(GlyphOrShift<T> glyph)
    {
        if (glyph.Glyph == null) { return; }
        Apply(glyph.Glyph);
    }

    internal void Apply(GlyphOrShift<T> glyph)
    {
        if (glyph.Glyph != null)
        {
            Apply(glyph.Glyph);
        }
        else if (glyph.Shift != T.Zero)
        {
            ApplyTj(glyph.Shift);
        }
    }

    internal GfxState<T> ClipExcept(PdfRect<T> rect)
    {
        return this with { Clipping = Clipping.ClipExcept(rect) };
    }

    internal GfxState<T> Clip(PdfRect<T> rect, PdfRect<T> boundary)
    {
        return this with { Clipping = Clipping.Clip(rect, boundary) };
    }
}