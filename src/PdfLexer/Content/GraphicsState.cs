using PdfLexer.Content;
using PdfLexer.Content.Model;
using PdfLexer.DOM;
using PdfLexer.DOM.ColorSpaces;
using PdfLexer.Fonts;
using PdfLexer.Graphics;
using PdfLexer.Writing;
using System.Numerics;

namespace PdfLexer.Content
{
    public record GfxState
    {
        public GfxState()
        {
            CTM = GfxMatrix.Identity;
            Text = new TxtState();
            UpdateTRM();
        }

        internal GfxState? Prev { get; init; }
        internal IReadableFont? Font { get; init; }

        public GfxMatrix CTM { get; init; }

        public int TextMode { get; init; }
        public decimal FontSize { get; init; }
        public decimal TextHScale { get; init; } = 1;
        public decimal TextRise { get; init; }
        public decimal CharSpacing { get; init; }
        public decimal WordSpacing { get; init; }
        public decimal TextLeading { get; init; }
        public decimal LineWidth { get; init; } = 1;
        public int LineCap { get; init; }
        public int LineJoin { get; init; }
        public decimal MiterLimit { get; init; } = 10;
        public decimal Flatness { get; init; } = 1;
        public d_Op? Dashing { get; init; }
        public PdfName? RenderingIntent { get; init; }
        public IPdfObject? ColorSpace { get; init; } = PdfName.DeviceGray;
        public IPdfObject? ColorSpaceStroking { get; init; } = PdfName.DeviceGray;
        public IPdfOperation? Color { get; init; }
        public IPdfOperation? ColorStroking { get; init; }
        public PdfDictionary? FontObject { get; init; }
        public ExtGraphicsDict? ExtDict { get; init; }
        internal List<IClippingSection>? Clipping { get; init; }

        // not part of real gfx state
        internal TxtState Text { get; init; }

        internal GfxMatrix GetTranslation(GfxMatrix desired)
        {
            CTM.Invert(out var iv);
            return desired * iv;
        }



        internal (decimal llx, decimal lly, decimal urx, decimal ury) GetBoundingBox(Glyph glyph)
        {

            decimal x = 0, y = 0, x2 = (decimal)glyph.w0, y2 = 0;
            if (glyph.BBox != null)
            {
                // TODO : should this be moved to fonts to decide fallback?
                x = glyph.BBox[0];
                y = glyph.BBox[1];
                x2 = glyph.BBox[2];
                y2 = glyph.BBox[3];
            }
            var bl = GfxMatrix.Identity with { E = x, F = y } * Text.TextRenderingMatrix;

            var tr = GfxMatrix.Identity with { E = x2, F = y2 } * Text.TextRenderingMatrix;

            return (bl.E, bl.F, tr.E, tr.F);
        }

        internal void ShiftTextMatrix(decimal tx, decimal ty)
        {
            // Tm = [ 1  0  0 ] x Tm
            //        0  1  0
            //        tx ty 1

            Text.TextMatrix = GfxMatrix.Identity with { E = tx, F = ty } * Text.TextMatrix;

            UpdateTRM();
        }

        internal void UpdateTRM()
        {
            Text.TextRenderingMatrix = new GfxMatrix(
              FontSize * TextHScale, 0,
              0, FontSize,
              0, TextRise) * Text.TextMatrix * CTM;
        }

        internal void ShiftTextAndLineMatrix(decimal tx, decimal ty)
        {
            // Tm = Tlm = [ 1  0  0 ] x Tlm
            //              0  1  0
            //              tx ty 1

            Text.TextLineMatrix = GfxMatrix.Identity with { E = tx, F = ty } * Text.TextLineMatrix;
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
            decimal tx = 0;
            decimal ty = 0;
            if (!(Font?.IsVertical ?? false))
            {
                // tx = ((w0-Tj/1000) * T_fs + T_c + T_w?) * Th
                // var s = (info.w0 - tj / 1000) * FontSize + CharSpacing; // Tj pre applied
                var s = ((decimal)info.w0) * FontSize + CharSpacing;
                if (info.IsWordSpace)
                {
                    s += WordSpacing;
                }
                tx = s * TextHScale;
            }
            else
            {
                // ty = (w1-Tj/1000) * T_fs + T_c + T_w?)
                var s = (decimal)(info.w1) * FontSize + CharSpacing;
                if (info.IsWordSpace) { s += WordSpacing; }
                ty = s;
            }

            ShiftTextMatrix(tx, ty);
        }
        internal void ApplyTj(decimal tj)
        {
            if (tj == 0) { return; }
            decimal tx = 0;
            decimal ty = 0;
            if (!(Font?.IsVertical ?? false))
            {
                tx = (-tj / 1000.0m) * FontSize * TextHScale;
            }
            else
            {
                var s = (-tj / 1000.0m) * FontSize;
                ty = s;
            }

            ShiftTextMatrix(tx, ty);
        }

        internal void ApplyShift(GlyphOrShift glyph)
        {
            ApplyTj(glyph.Shift);
        }

        internal void ApplyCharShift(GlyphOrShift glyph)
        {
            if (glyph.Glyph == null) { return; }
            Apply(glyph.Glyph);
        }
    }

    public record ExtGraphicsDict
    {
        public required GfxMatrix CTM { get; init; }
        public required PdfDictionary Dict { get; init; }
    }

    internal record ClippingInfo : IClippingSection
    {
        public ClippingInfo(GfxMatrix tm, List<SubPath> path, bool evenOdd)
        {
            TM = tm;
            Path = path;
            EvenOdd = evenOdd;
        }
        public List<SubPath> Path { get; set; }
        public GfxMatrix TM { get; set; }
        public bool EvenOdd { get; set; }

        public void Apply(ContentWriter writer)
        {
            foreach (var path in Path)
            {
                writer.SubPath(path);
            }

            if (EvenOdd)
            {
                W_Star_Op.WriteLn(writer.StreamWriter.Stream);
            }
            else
            {
                W_Op.WriteLn(writer.StreamWriter.Stream);
            }
            n_Op.WriteLn(writer.StreamWriter.Stream);
        }
    }

    internal interface IClippingSection
    {
        void Apply(ContentWriter writer);
        GfxMatrix TM { get; }
    }
    internal record TextClippingInfo : IClippingSection
    {
        public required List<GlyphOrShift> Glyphs { get; set; }
        public required GfxMatrix TM { get; set; }
        public GfxMatrix? LineMatrix { get; set; }
        public bool NewLine { get; set; }

        public void Apply(ContentWriter writer)
        {
            if (LineMatrix.HasValue)
            {
                writer.SetLinePosition(LineMatrix.Value);
            }
            else if (NewLine)
            {
                writer.Op(T_Star_Op.Value);
            }
            writer.WriteGlyphs(Glyphs);
        }
    }

    public class TxtState
    {
        public TxtState()
        {
            TextLineMatrix = GfxMatrix.Identity;
            TextMatrix = GfxMatrix.Identity;
        }
        public GfxMatrix TextRenderingMatrix { get; internal set; }
        //      Trm = [ T_fs*T_h  0       0 ] x Tm x CTM
        //              0         T_fs    0
        //              0         T_rise  1
        internal GfxMatrix TextMatrix { get; set; }
        internal GfxMatrix TextLineMatrix { get; set; }
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

            decimal? fsize = null;
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
            if (state.ExtDict != null)
            {
                // TODO -> dedup these or change extdict to list
                var existing = state.ExtDict.Dict.CloneShallow();
                foreach (var kvp in dict)
                {
                    existing[kvp.Key] = kvp.Value;
                }
                dict = existing;
            } else
            {
                if (!cache.TryGetValue(orig, out var cached))
                {
                    cache[orig] = dict;
                }
                else
                {
                    dict = cached;
                }
            }



            state = state with
            {
                LineWidth = lsobj == null ? state.LineWidth : lsobj,
                LineCap = lcobj == null ? state.LineCap : (int)lcobj,
                LineJoin = ljobj == null ? state.LineJoin : (int)ljobj,
                MiterLimit = mlobj == null ? state.MiterLimit : mlobj,
                RenderingIntent = riobj == null ? state.RenderingIntent : riobj,
                Flatness = flobj == null ? state.Flatness : flobj,
                Dashing = dop == null ? state.Dashing : dop,
                Font = fread == null ? state.Font : fread,
                FontObject = fdict == null ? state.FontObject : fdict,
                FontSize = fsize == null ? state.FontSize : fsize.Value,
                ExtDict = dict.Count > 0 ? new ExtGraphicsDict { Dict = dict, CTM = state.CTM } : null
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
            var val = new GfxMatrix(a, b, c, d, e, f);
            state = state with { CTM = val * state.CTM };
            state.UpdateTRM();
        }

        public static void Apply(ref GfxState state, GfxMatrix val)
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

        public static void WriteLn(GfxMatrix ctm, Stream stream)
        {
            WriteLn(
                ctm.A, ctm.B,
                ctm.C, ctm.D,
                ctm.E, ctm.F, stream);
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

    public interface IPatternableColor
    {
        IPdfObject? Pattern { get; }
    }

    public partial class SCN_Op : IPatternableColor
    {
        public void Apply(ref GfxState state)
        {
            state = state with { ColorStroking = this };
        }
        public IPdfObject? Pattern { get; set; } // hack for resource until adding better color handling
    }

    public partial class sc_Op
    {
        public void Apply(ref GfxState state)
        {
            state = state with { Color = this };
        }
    }

    public partial class scn_Op : IPatternableColor
    {
        public void Apply(ref GfxState state)
        {
            state = state with { Color = this };
        }
        public IPdfObject? Pattern { get; set; } // hack for resource until adding better color handling
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
            state = state with { LineWidth = lineWidth };
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
            state = state with { MiterLimit = miterLimit };
        }
    }

    public partial class i_Op
    {
        public void Apply(ref GfxState state)
        {
            state = state with { Flatness = flatness };
        }
    }

    public partial class d_Op
    {
        public void Apply(ref GfxState state)
        {
            state = state with { Dashing = this };
        }
    }

    public partial class ri_Op
    {
        public void Apply(ref GfxState state)
        {
            state = state with { RenderingIntent = intent };
        }
    }
}