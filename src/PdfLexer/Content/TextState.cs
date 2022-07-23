using PdfLexer.Fonts;
using PdfLexer.Operators;
using PdfLexer.Parsers;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace PdfLexer.Content
{
    public class TextState
    {
        public int TextMode { get; internal set; }

        private float __fontSize;
        public float FontSize { 
            get => __fontSize;
            internal set {
                __fontSize = value;
                UpdateTRM();
            } 
        }
        private float __textHScale;
        public float TextHScale { 
            get => __textHScale;
            internal set
            {
                __textHScale = value;
                UpdateTRM();
            }
        }
        private float __textRise;
        public float TextRise
        {
            get => __textRise; 
            internal set
            {
                __textRise = value;
                UpdateTRM();
            }
        }

        private Matrix4x4 __textMatrix;
        public Matrix4x4 TextMatrix
        {
            get => __textMatrix;
            internal set
            {
                __textMatrix = value;
                UpdateTRM();
            }
        }

        public float CharSpacing { get; internal set; }
        public float WordSpacing { get; internal set; }
        public float TextLeading { get; internal set; }

        public PdfName FontName { get; internal set; }
        public IReadableFont Font { get; internal set; } // todo
        // fontMatrix ??

        // marked content?
        // will need Apply for BMC BMCProps EMC


        private Matrix4x4 __ctm;
        internal Matrix4x4 CTM
        {
            get => __ctm;
            set
            {
                __ctm = value;
                UpdateTRM();
            }
        }

        Matrix4x4 TextRenderingMatrix { get; set; } // todo
                                                    //      Trm = [ T_fs*T_h  0       0 ] x Tm x CTM
                                                    //              0         T_fs    0
                                                    //              0         T_rise  1

        private void UpdateTRM()
        {
            return; // not used for now
            TextRenderingMatrix = new Matrix4x4(
              FontSize*TextHScale, 0,        0, 0,
              0,                   FontSize, 0, 0,
              0,                   TextRise, 1, 0,
              0,                   0,        0, 1) * TextMatrix * CTM;
        }

        Matrix4x4 TextLineMatrix { get; set; } // set to Tm at beginning of a line of text
        internal ParsingContext Ctx { get; }
        internal PdfDictionary PageResources { get; }
        internal PdfDictionary FormResources { get; set; }

        public TextState(ParsingContext ctx, PdfDictionary pageResources)
        {
            Ctx = ctx;
            PageResources = pageResources;
            TextMatrix = Matrix4x4.Identity;
            TextLineMatrix = Matrix4x4.Identity;
        }

        public void Apply(BT_Op op)
        {
            TextLineMatrix = TextMatrix = Matrix4x4.Identity;
        }

        public int GetGlyph(ReadOnlySpan<byte> data, int pos, out Glyph info)
        {
            return Font.GetGlyph(data, pos, out info);
        }

        public void FillGlyphs(Tj_Op op, List<UnappliedGlyph> glyphs) => FillGlyphs(op.text, glyphs);

        public void FillGlyphs(ReadOnlySpan<byte> data, List<UnappliedGlyph> glyphs)
        {
            glyphs.Clear();
            int i = 0;
            int u = 0;
            while (i < data.Length && (u = GetGlyph(data, i, out var glyph)) > 0)
            {
                glyphs.Add(new UnappliedGlyph(glyph, 0f));
                i += u;
            }
        }

        public void FillGlyphs(TJ_Op op, List<UnappliedGlyph> glyphs)
        {
            float offset = 0f;
            glyphs.Clear();
            foreach (var info in op.info)
            {
                if (info.Data != null)
                {
                    int i = 0;
                    int u = 0;
                    while (i < info.Data.Length && (u = GetGlyph(info.Data, i, out var glyph)) > 0)
                    {
                        glyphs.Add(new UnappliedGlyph(glyph, offset));
                        offset = 0f;
                        i += u;
                    }
                }
                else if (info.Shift != 0)
                {
                    offset += (float)info.Shift;
                }
            }
        }

        public void ApplyTj(float tj)
        {
            if (tj == 0f) { return; }
            float tx = 0f;
            float ty = 0f;
            if (!Font.IsVertical)
            {
                tx = (-tj / 1000) * FontSize * TextHScale;
            }
            else
            {
                var s = (-tj / 1000) * FontSize;
                ty = s;
            }

            ShiftTextMatrix(tx, ty);
        }

        public void Apply(UnappliedGlyph glyph)
        {
            if (glyph.Shift != 0) { ApplyTj(glyph.Shift); }
            Apply(glyph.Glyph);
        }

        internal void ApplyShift(UnappliedGlyph glyph)
        {
            ApplyTj(glyph.Shift);
        }

        internal void ApplyCharShift(UnappliedGlyph glyph)
        {
            Apply(glyph.Glyph);
        }

        public void Apply(Glyph info)
        {
            // shift
            float tx = 0f;
            float ty = 0f;
            if (!Font.IsVertical)
            {
                // tx = ((w0-Tj/1000) * T_fs + T_c + T_w?) * Th
                // var s = (info.w0 - tj / 1000) * FontSize + CharSpacing; // Tj pre applied
                var s = (info.w0) * FontSize + CharSpacing;
                if (info.IsWordSpace) { s += WordSpacing; }
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

        private void ShiftTextMatrix(float tx, float ty)
        {
            // Tm = [ 1  0  0 ] x Tm
            //        0  1  0
            //        tx ty 1

            TextMatrix = new Matrix4x4(
              1f, 0f, 0f, 0f,
              0f, 1f, 0f, 0f,
              tx, ty, 1f, 0f,
              0f, 0f, 0f, 1f) * TextMatrix;
        }

        public void Apply(Td_Op op)
        {
            ShiftTextAndLineMatrix((float)op.tx, (float)op.ty);
        }

        private void ShiftTextAndLineMatrix(float tx, float ty)
        {
            // Tm = Tlm = [ 1  0  0 ] x Tlm
            //              0  1  0
            //              tx ty 1

            TextLineMatrix = new Matrix4x4(
                          1f, 0f, 0f, 0f,
                          0f, 1f, 0f, 0f,
                          tx, ty, 1f, 0f,
                          0f, 0f, 0f, 1f) * TextLineMatrix;

            TextMatrix = TextLineMatrix;
        }

        public void Apply(TD_Op op)
        {
            // -ty TL
            // tx, ty Td
            TextLeading = (float)-op.ty;
            ShiftTextAndLineMatrix((float)op.tx, (float)op.ty);
        }

        public void Apply(Tm_Op op)
        {
            TextLineMatrix = new Matrix4x4(
                          (float)op.a, (float)op.b, 0f, 0f,
                          (float)op.c, (float)op.d, 0f, 0f,
                          (float)op.e, (float)op.f, 1f, 0f,
                          0f, 0f, 0f, 1f);

            TextMatrix = TextLineMatrix;
        }

        public void Apply(T_Star_Op op)
        {
            ShiftTextAndLineMatrix(0, -TextLeading);
        }


        public void Apply(Tj_Op op)
        {
            ApplyData(op.text);
        }

        private void ApplyData(ReadOnlySpan<byte> data)
        {
            var i = 0;
            while (i < data.Length)
            {
                i += Font.GetGlyph(data, i, out var glyph);
                Apply(glyph);
            }
        }

        public void Apply(doublequote_Op op)
        {
            WordSpacing = (float)op.aw;
            CharSpacing = (float)op.ac;
            Apply(new singlequote_Op(op.text));
        }

        public void Apply(singlequote_Op op)
        {
            Apply(T_Star_Op.Value);
            Apply(new Tj_Op(op.text));
        }

        public void Apply(TJ_Op op)
        {
            foreach (var item in op.info)
            {
                if (item.Shift != 0m)
                {
                    ApplyTj((float)item.Shift);
                }
                else
                {
                    ApplyData(item.Data);
                }
            }
        }

        public void Apply(Ts_Op op)
        {
            TextRise = (float)op.rise;
        }

        public void Apply(TL_Op op)
        {
            TextLeading = (float)op.leading;
        }

        public void Apply(Tr_Op op)
        {
            TextMode = op.render;
        }

        public void Apply(Tw_Op op)
        {
            WordSpacing = (float)op.wordSpace;
        }

        public void Apply(Tc_Op op)
        {
            CharSpacing = (float)op.charSpace;
        }

        public void Apply(Tz_Op op)
        {
            TextHScale = (float)op.scale;
        }

        public void Apply(gs_Op op)
        {
            var fa = GetFontFromGs(op.name);
            if (fa != null)
            {
                // holds array of [ font_iref size ] -> similar to Tf but iref instead of name
                // TODO harden
                Font = Ctx.GetFont(fa[0]);
                if (fa.Count > 1 && fa[1].GetPdfObjType() == PdfObjectType.NumericObj)
                {
                    FontSize = fa[1].GetValue<PdfNumber>();
                }
            }
            // TODO any other text props in GS?
        }

        public void Apply(Tf_Op op)
        {
            FontSize = (float)op.size;
            if (Font != null && FontName == op.font)
            {
                return;
            }
            FontName = op.font;
            Font = Ctx.GetFont(GetFontObj(FontName));
        }


        private PdfArray GetFontFromGs(PdfName name)
        {
            if (FormResources != null && FormResources.TryGetValue<PdfDictionary>(PdfName.ExtGState, out var gss)
                && gss.TryGetValue<PdfDictionary>(name, out var gs))
            {
                return gs.Get<PdfArray>(PdfName.Font);
            }

            if (PageResources.TryGetValue<PdfDictionary>(PdfName.ExtGState, out gss)
                && gss.TryGetValue<PdfDictionary>(name, out gs))
            {
                return gs.Get<PdfArray>(PdfName.Font);
            }

            return null;
        }

        private IPdfObject GetFontObj(PdfName name)
        {
            if (FormResources != null && FormResources.TryGetValue<PdfDictionary>(PdfName.Font, out var fonts) 
                && fonts.TryGetValue<PdfDictionary>(name, out var fnt))
            {
                return fnt;
            }

            if (PageResources.TryGetValue<PdfDictionary>(PdfName.Font, out fonts)
                && fonts.TryGetValue<PdfDictionary>(name, out fnt))
            {
                return fnt;
            }

            return null;
        }

    }
}
