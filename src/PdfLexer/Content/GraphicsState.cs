using PdfLexer.Content;
using PdfLexer.Content.Model;
using PdfLexer.DOM;
using PdfLexer.DOM.ColorSpaces;
using PdfLexer.Fonts;
using PdfLexer.Graphics;
using System.Numerics;

namespace PdfLexer.Content
{
    public record GfxState
    {
        public GfxState()
        {
            CTM = Matrix4x4.Identity;
            Text = new TxtState();
            UpdateTRM();
        }

        internal GfxState? Prev { get; init; }
        internal IReadableFont? Font { get; init; }

        public Matrix4x4 CTM { get; init; }

        public int TextMode { get; init; }
        public float FontSize { get; init; }
        public float TextHScale { get; init; } = 1f;
        public float TextRise { get; init; }
        public float CharSpacing { get; init; }
        public float WordSpacing { get; init; }
        public float TextLeading { get; init; }
        public float LineWidth { get; init; }
        public int LineCap { get; init; }
        public int LineJoin { get; init; }
        public float MiterLimit { get; init; }
        public float Flatness { get; init; }
        public d_Op? Dashing { get; init; }
        public PdfName? RenderingIntent { get; init; }
        public IPdfObject? ColorSpace { get; init; } = PdfName.DeviceGray;
        public IPdfObject? ColorSpaceStroking { get; init; } = PdfName.DeviceGray;
        public IPdfOperation? Color { get; init; }
        public IPdfOperation? ColorStroking { get; init; }
        public PdfDictionary? FontObject { get; init; }
        public PdfDictionary? ExtDict { get; init; }
        internal ClippingInfo? Clipping { get; init; }

        // not part of real gfx state
        internal TxtState Text { get; init; }

        internal Matrix4x4 GetTranslation(Matrix4x4 desired)
        {
            Matrix4x4.Invert(CTM, out var iv);
            return desired * iv;
        }

        internal (float llx, float lly, float urx, float ury) GetBoundingBox(Glyph glyph)
        {

            float x = 0, y = 0, x2 = glyph.w0, y2 = 0;
            if (glyph.BBox != null)
            {
                // TODO : should this be moved to fonts to decide fallback?
                x = (float)glyph.BBox[0];
                y = (float)glyph.BBox[1];
                x2 = (float)glyph.BBox[2];
                y2 = (float)glyph.BBox[3];
            }
            var bl = new Matrix4x4(
                          1f, 0f, 0f, 0f,
                          0f, 1f, 0f, 0f,
                          x, y, 1f, 0f,
                          0f, 0f, 0f, 1f) * Text.TextRenderingMatrix;

            var tr = new Matrix4x4(
              1f, 0f, 0f, 0f,
              0f, 1f, 0f, 0f,
              x2, y2, 1f, 0f,
              0f, 0f, 0f, 1f) * Text.TextRenderingMatrix;

            return (bl.M31, bl.M32, tr.M31, tr.M32);
        }

        internal void ShiftTextMatrix(float tx, float ty)
        {
            // Tm = [ 1  0  0 ] x Tm
            //        0  1  0
            //        tx ty 1

            Text.TextMatrix = new Matrix4x4(
              1f, 0f, 0f, 0f,
              0f, 1f, 0f, 0f,
              tx, ty, 1f, 0f,
              0f, 0f, 0f, 1f) * Text.TextMatrix;

            UpdateTRM();
        }

        internal void UpdateTRM()
        {
            Text.TextRenderingMatrix = new Matrix4x4(
              FontSize * TextHScale, 0, 0, 0,
              0, FontSize, 0, 0,
              0, TextRise, 1, 0,
              0, 0, 0, 1) * Text.TextMatrix * CTM;
        }

        internal void ShiftTextAndLineMatrix(float tx, float ty)
        {
            // Tm = Tlm = [ 1  0  0 ] x Tlm
            //              0  1  0
            //              tx ty 1

            Text.TextLineMatrix = new Matrix4x4(
                          1f, 0f, 0f, 0f,
                          0f, 1f, 0f, 0f,
                          tx, ty, 1f, 0f,
                          0f, 0f, 0f, 1f) * Text.TextLineMatrix;

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
            float tx = 0f;
            float ty = 0f;
            if (!(Font?.IsVertical ?? false))
            {
                // tx = ((w0-Tj/1000) * T_fs + T_c + T_w?) * Th
                // var s = (info.w0 - tj / 1000) * FontSize + CharSpacing; // Tj pre applied
                var s = (info.w0) * FontSize + CharSpacing;
                if (info.IsWordSpace)
                {
                    s += WordSpacing;
                }
                tx = s * TextHScale;
            }
            else
            {
                // ty = (w1-Tj/1000) * T_fs + T_c + T_w?)
                var s = (info.w1) * FontSize + CharSpacing;
                if (info.IsWordSpace) { s += WordSpacing; }
                ty = s;
            }

            ShiftTextMatrix(tx, ty);
        }
        internal void ApplyTj(float tj)
        {
            if (tj == 0f) { return; }
            float tx = 0f;
            float ty = 0f;
            if (!(Font?.IsVertical ?? false))
            {
                tx = (-tj / 1000.0f) * FontSize * TextHScale;
            }
            else
            {
                var s = (-tj / 1000.0f) * FontSize;
                ty = s;
            }

            ShiftTextMatrix(tx, ty);
        }

        internal void ApplyShift(UnappliedGlyph glyph)
        {
            ApplyTj(glyph.Shift);
        }

        internal void ApplyCharShift(UnappliedGlyph glyph)
        {
            if (glyph.Glyph == null) { return; }
            Apply(glyph.Glyph);
        }
    }

    internal class ClippingInfo
    {
        public ClippingInfo(SubPath path, bool evenOdd)
        {
            Path = path;
            EvenOdd = evenOdd;
        }
        public SubPath Path { get; set; }
        public bool EvenOdd { get; set; }
    }

    public class TxtState
    {
        public TxtState()
        {
            TextLineMatrix = Matrix4x4.Identity;
            TextMatrix = Matrix4x4.Identity;
        }
        public Matrix4x4 TextRenderingMatrix { get; internal set; }
        //      Trm = [ T_fs*T_h  0       0 ] x Tm x CTM
        //              0         T_fs    0
        //              0         T_rise  1
        internal Matrix4x4 TextMatrix { get; set; }
        internal Matrix4x4 TextLineMatrix { get; set; }
    }

    public class GraphicsState
    {
        public GraphicsState()
        {
            CTM = Matrix4x4.Identity;
        }
        internal GraphicsState? Prev { get; set; }
        public Matrix4x4 CTM { get; set; }

        public (float x, float y, float w, float h) GetCurrentSize()
        {
            return (CTM.M31, CTM.M32, CTM.M11, CTM.M22);
        }

        // a b 0  ->  0 1 - -> 1 0 (translation) -> x 0 (scale)
        // c d 0      2 3 -    0 1                  0 y
        // e f 1      4 5 -    x y                  0 0
        //
        // 
        // M11 M12 M13
        // M21 M22 M23
        // M31 M32 M33
        //

        //  cosq   sinq (rotation) | 1    tana (skew)
        //  -sinq  cosq              tanb 1
        //  0      0                 0    0 
        //

        // rotation -> 180/Math.PI * Math.Atan2(d, c) - 90
        //          -> 180/Math.PI * Math.Atan2(b, a)


        public int TextMode { get; internal set; }
        public float FontSize { get; internal set; }
        public float TextHScale { get; internal set; } = 1f;
        public float TextRise { get; internal set; }
        public float CharSpacing { get; internal set; }
        public float WordSpacing { get; internal set; }
        public float TextLeading { get; internal set; }
        public PdfName? FontResourceName { get; internal set; }
        public string? FontName { get; internal set; }
        public FontFlags FontFlags { get; internal set; }
        public PdfDictionary? FontObject { get; internal set; }
        public IReadableFont? Font { get; internal set; }
        internal IClippingPath? Clipping { get; set; }

        public Matrix4x4 GetTranslation(Matrix4x4 desired)
        {
            Matrix4x4.Invert(CTM, out var iv);
            return desired * iv;
        }

        public void Apply(Matrix4x4 cm)
        {
            CTM = cm * CTM;
        }

        public GraphicsState CloneNoPrev()
        {
            return new GraphicsState
            {
                CTM = CTM,
                TextHScale = TextHScale,
                TextLeading = TextLeading,
                TextMode = TextMode,
                TextRise = TextRise,
                CharSpacing = CharSpacing,
                WordSpacing = WordSpacing,
                Font = Font,
                FontObject = FontObject,
                FontResourceName = FontResourceName,
                FontName = FontName,
                FontFlags = FontFlags,
                FontSize = FontSize,
                Clipping = Clipping,
            };
        }

    }
}

namespace PdfLexer.Operators
{
    public partial class gs_Op
    {
        public void Apply(ref GfxState state)
        {
            throw new NotSupportedException();
        }

        public void Apply(ref GfxState state, PdfDictionary dict, PdfDictionary resources, ParsingContext ctx, Dictionary<PdfDictionary, PdfDictionary> cache)
        {
            var orig = dict;
            dict = dict.CloneShallow();
            if (dict.TryGetValue<PdfNumber>("LS", out var lsobj, false))
            {
                dict.Remove("LS");
            }

            if (dict.TryGetValue<PdfNumber>("LC", out var lcobj, false))
            {
                dict.Remove("LC");
            }

            if (dict.TryGetValue<PdfNumber>("LJ", out var ljobj, false))
            {
                dict.Remove("LJ");
            }

            if (dict.TryGetValue<PdfNumber>("ML", out var mlobj, false))
            {
                dict.Remove("ML");
            }

            if (dict.TryGetValue<PdfName>("RI", out var riobj, false))
            {
                dict.Remove("RI");
            }

            if (dict.TryGetValue<PdfNumber>("FL", out var flobj, false))
            {
                dict.Remove("FL");
            }

            d_Op? dop = null;
            if (dict.TryGetValue<PdfArray>("D", out var dobj, false))
            {
                dict.Remove("D");
                if (dobj.Count > 1 && dobj[0] is PdfArray dashes && dobj[1] is PdfNumber dp)
                {
                    dop = new d_Op(dashes, dp);
                }
            }

            float? fsize = null;
            PdfDictionary? fdict = null;
            IReadableFont? fread = null;
            if (dict.TryGetValue<PdfArray>(PdfName.Font, out var fobj, false))
            {
                dict.Remove(PdfName.Font);
                if (fobj.Count > 0 && fobj[0].Resolve() is PdfDictionary fdv)
                {
                    fdict = fdv;
                    fread = ctx.GetFont(fdv);
                }
                if (fobj.Count > 1 && fobj[1].Resolve() is PdfNumber fz)
                {
                    fsize = fz;
                }
            }

            if (!cache.TryGetValue(orig, out var cached))
            {
                cache[orig] = dict;
            } else
            {
                dict = cached;
            }

            state = state with { 
                LineWidth = lsobj == null ? state.LineWidth : (float)lsobj,
                LineCap = lcobj == null ? state.LineCap : (int)lcobj,
                LineJoin = ljobj == null ? state.LineJoin : (int)ljobj,
                MiterLimit = mlobj == null ? state.MiterLimit: (float)mlobj,
                RenderingIntent = riobj == null ? state.RenderingIntent : riobj,
                Flatness = flobj == null ? state.Flatness : flobj,
                Dashing = dop == null ? state.Dashing : dop,
                Font = fread == null ? state.Font : fread,
                FontObject = fdict == null ? state.FontObject : fdict,
                FontSize = fsize == null ? state.FontSize : fsize.Value,
                ExtDict = dict.Count > 0 ? dict : null
            };
            state.UpdateTRM();
        }
    }

    public partial class Q_Op
    {
        public void Apply(ref GraphicsState state)
        {
            if (state.Prev == null)
            {
                // err
                state = new GraphicsState();
                return;
            }
            state = state.Prev;
        }

        public void Apply(ref GfxState state)
        {
            if (state.Prev == null)
            {
                // err
                state = new GfxState();
                return;
            }
            state = state.Prev with { Text = state.Text };
            state.UpdateTRM();
        }
    }

    public partial class q_Op
    {
        public void Apply(ref GraphicsState state)
        {
            var prev = state;
            state = state.CloneNoPrev();
            state.Prev = prev;
        }

        public void Apply(ref GfxState state)
        {
            state = state with { Prev = state };
        }
    }

    public partial class cm_Op
    {
        public void Apply(ref GraphicsState state)
        {
            // new = cm x ctm
            var val = new Matrix4x4(
                          (float)a, (float)b, 0f, 0f,
                          (float)c, (float)d, 0f, 0f,
                          (float)e, (float)f, 1f, 0f,
                          0f, 0f, 0f, 1f);
            state.CTM = val * state.CTM;
        }

        public void Apply(ref GfxState state)
        {
            // new = cm x ctm
            var val = new Matrix4x4(
                          (float)a, (float)b, 0f, 0f,
                          (float)c, (float)d, 0f, 0f,
                          (float)e, (float)f, 1f, 0f,
                          0f, 0f, 0f, 1f);
            state = state with { CTM = val * state.CTM };
            state.UpdateTRM();
        }

        public static void Apply(ref GfxState state, Matrix4x4 val)
        {
            state = state with { CTM = val * state.CTM };
            state.UpdateTRM();
        }

        public static void WriteLn(Matrix4x4 ctm, Stream stream)
        {
            WriteLn(
                (decimal)ctm.M11, (decimal)ctm.M12,
                (decimal)ctm.M21, (decimal)ctm.M22,
                (decimal)ctm.M31, (decimal)ctm.M32, stream);
        }
    }

    public partial class CS_Op
    {
        public void Apply(ref GfxState state)
        {
            throw new NotSupportedException();
        }
    }

    public partial class cs_Op
    {
        public void Apply(ref GfxState state)
        {
            throw new NotSupportedException();
        }
    }

    public partial class SC_Op
    {
        public void Apply(ref GfxState state)
        {
            state = state with { ColorStroking = this };
        }
    }

    public partial class SCN_Op
    {
        public void Apply(ref GfxState state)
        {
            state = state with { ColorStroking = this };
        }
    }

    public partial class sc_Op
    {
        public void Apply(ref GfxState state)
        {
            state = state with { Color = this };
        }
    }

    public partial class scn_Op
    {
        public void Apply(ref GfxState state)
        {
            state = state with { Color = this };
        }
    }

    public partial class G_Op
    {
        public void Apply(ref GfxState state)
        {
            state = state with { ColorSpaceStroking = PdfName.DeviceGray, ColorStroking = new SC_Op(new List<decimal> { this.gray }) }; // TODO revisit how this is handled
        }
    }

    public partial class g_Op
    {
        public void Apply(ref GfxState state)
        {
            state = state with { ColorSpace = PdfName.DeviceGray, Color = new sc_Op(new List<decimal> { this.gray }) };  // TODO revisit how this is handled
        }
    }

    public partial class RG_Op
    {
        public void Apply(ref GfxState state)
        {
            state = state with { ColorSpaceStroking = PdfName.DeviceRGB, ColorStroking = new SC_Op(new List<decimal> { r, g, b }) };
        }
    }

    public partial class rg_Op
    {
        public void Apply(ref GfxState state)
        {
            state = state with { ColorSpace = PdfName.DeviceRGB, Color = new sc_Op(new List<decimal> { r, g, b }) };   // TODO revisit how this is handled
        }
    }


    public partial class K_Op
    {
        public void Apply(ref GfxState state)
        {
            state = state with { ColorSpaceStroking = PdfName.DeviceCMYK, ColorStroking = new SC_Op(new List<decimal> { c, m, y, k }) }; // TODO revisit how this is handled
        }
    }

    public partial class k_Op
    {
        public void Apply(ref GfxState state)
        {
            state = state with { ColorSpace = PdfName.DeviceCMYK, Color = new sc_Op(new List<decimal> { c, m, y, k }) }; // TODO revisit how this is handled
        }
    }

    public partial class w_Op
    {
        public void Apply(ref GfxState state)
        {
            state = state with { LineWidth = (float)lineWidth };
        }
    }

    public partial class J_Op
    {
        public void Apply(ref GfxState state)
        {
            state = state with { LineCap = lineCap };
        }
    }

    public partial class j_Op
    {
        public void Apply(ref GfxState state)
        {
            state = state with { LineJoin = lineJoin };
        }
    }

    public partial class M_Op
    {
        public void Apply(ref GfxState state)
        {
            state = state with { MiterLimit = (float)miterLimit };
        }
    }

    public partial class i_Op
    {
        public void Apply(ref GfxState state)
        {
            state = state with { Flatness = (float)flatness };
        }
    }

    public partial class d_Op
    {
        public void Apply(ref GfxState state)
        {
            state = state with { Dashing = this };
        }
    }
}