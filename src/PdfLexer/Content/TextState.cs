using PdfLexer.Fonts;
using PdfLexer.Operators;
using PdfLexer.Parsers;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace PdfLexer.Content
{
    public class TextState
    {
        internal List<ScannerInfo> stack;
        internal GraphicsState GS { get; set; }

        public Matrix4x4 TextRenderingMatrix { get; private set; }
        //      Trm = [ T_fs*T_h  0       0 ] x Tm x CTM
        //              0         T_fs    0
        //              0         T_rise  1

        internal void UpdateTRM()
        {
            TextRenderingMatrix = new Matrix4x4(
              GS.FontSize * GS.TextHScale, 0, 0, 0,
              0, GS.FontSize, 0, 0,
              0, GS.TextRise, 1, 0,
              0, 0, 0, 1) * TextMatrix * GS.CTM;
        }

        Matrix4x4 TextMatrix { get; set; }
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
            UpdateTRM();
        }

        public int GetGlyph(ReadOnlySpan<byte> data, int pos, out Glyph info)
        {
            return GS.Font.GetGlyph(data, pos, out info);
        }

        internal void FillGlyphsFromRawString(ReadOnlySpan<byte> data, List<UnappliedGlyph> glyphs)
        {
            if (data.Length < 200)
            {
                Span<byte> writeBuffer = stackalloc byte[data.Length];
                var l = Ctx.StringParser.ConvertBytes(data, writeBuffer);
                FillGlyphsNoReset(writeBuffer.Slice(0, l), glyphs);
            }
            else
            {
                var rented = ArrayPool<byte>.Shared.Rent(data.Length);
                ReadOnlySpan<byte> spanned = rented;
                var l = Ctx.StringParser.ConvertBytes(data, rented);
                FillGlyphsNoReset(spanned.Slice(0, l), glyphs);
                ArrayPool<byte>.Shared.Return(rented);
            }
        }

        internal void FillGlyphsNoReset(ReadOnlySpan<byte> data, List<UnappliedGlyph> glyphs)
        {
            int i = 0;
            int u = 0;
            while (i < data.Length && (u = GetGlyph(data, i, out var glyph)) > 0)
            {
                glyphs.Add(new UnappliedGlyph(glyph, 0f));
                i += u;
            }
        }

        internal void FillGlyphs(ReadOnlySpan<byte> data, List<UnappliedGlyph> glyphs)
        {
            glyphs.Clear();
            FillGlyphsNoReset(data, glyphs);
        }

        internal void FillGlyphs(TJ_Op op, List<UnappliedGlyph> glyphs)
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

        internal void ApplyTj(float tj)
        {
            if (tj == 0f) { return; }
            float tx = 0f;
            float ty = 0f;
            if (!GS.Font.IsVertical)
            {
                tx = (-tj / 1000.0f) * GS.FontSize * GS.TextHScale; // TODO 1000 should be from fontmatrix?
            }
            else
            {
                var s = (-tj / 1000.0f) * GS.FontSize;
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
            if (!GS.Font.IsVertical)
            {
                // tx = ((w0-Tj/1000) * T_fs + T_c + T_w?) * Th
                // var s = (info.w0 - tj / 1000) * FontSize + CharSpacing; // Tj pre applied
                var s = (info.w0) * GS.FontSize + GS.CharSpacing;
                if (info.IsWordSpace) 
                { 
                    s += GS.WordSpacing;
                }
                tx = s * GS.TextHScale;
            }
            else
            {
                // ty = (w1-Tj/1000) * T_fs + T_c + T_w?)
                var s = (info.w1) * GS.FontSize + GS.CharSpacing;
                if (info.IsWordSpace) { s += GS.WordSpacing; }
                ty = s;
            }

            ShiftTextMatrix(tx, ty);
        }

        public (float llx, float lly, float urx, float ury) GetBoundingBox(Glyph glyph)
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
                          0f, 0f, 0f, 1f) * TextRenderingMatrix;

            var tr = new Matrix4x4(
              1f, 0f, 0f, 0f,
              0f, 1f, 0f, 0f,
              x2, y2, 1f, 0f,
              0f, 0f, 0f, 1f) * TextRenderingMatrix;

            return (bl.M31, bl.M32, tr.M31, tr.M32);
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

            UpdateTRM();
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
            UpdateTRM();
        }

        public void Apply(TD_Op op)
        {
            // -ty TL
            // tx, ty Td
            GS.TextLeading = (float)-op.ty;
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
            ShiftTextAndLineMatrix(0, -GS.TextLeading);
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
                i += GS.Font.GetGlyph(data, i, out var glyph);
                Apply(glyph);
            }
        }

        public void Apply(doublequote_Op op)
        {
            GS.WordSpacing = (float)op.aw;
            GS.CharSpacing = (float)op.ac;
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
            GS.TextRise = (float)op.rise;
            UpdateTRM();
        }

        public void Apply(TL_Op op)
        {
            GS.TextLeading = (float)op.leading;
        }

        public void Apply(Tr_Op op)
        {
            GS.TextMode = op.render;
        }

        public void Apply(Tw_Op op)
        {
            GS.WordSpacing = (float)op.wordSpace;
        }

        public void Apply(Tc_Op op)
        {
            GS.CharSpacing = (float)op.charSpace;
        }

        public void Apply(Tz_Op op)
        {
            GS.TextHScale = (float)op.scale / 100.0F;
            UpdateTRM();
        }

        public void Apply(gs_Op op)
        {
            var fa = GetFontFromGs(op.name);
            if (fa != null)
            {
                // holds array of [ font_iref size ] -> similar to Tf but iref instead of name
                // TODO harden
                GS.Font = Ctx.GetFont(fa[0]);
                GS.FontObject = fa[0].GetAs<PdfDictionary>();
                if (fa.Count > 1 && fa[1].GetPdfObjType() == PdfObjectType.NumericObj)
                {
                    GS.FontSize = fa[1].GetValue<PdfNumber>();
                }
            }
            UpdateTRM();
            // TODO any other text props in GS?
        }

        public void Apply(Tf_Op op)
        {
            GS.FontSize = (float)op.size;
            if (GS.Font != null && GS.FontName == op.font)
            {
                return;
            }
            GS.FontName = op.font;
            var obj = GetFontObj(GS.FontName);
            GS.FontObject = obj.GetAs<PdfDictionary>();
            GS.Font = Ctx.GetFont(obj);
            UpdateTRM();
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

            if (stack != null && stack.Count > 0)
            {
                for (var i=stack.Count-1;i>-1;i--)
                {
                    var dict = stack[i].Stream.Dictionary?.Get<PdfDictionary>(PdfName.Resources);
                    if (dict != null && dict.TryGetValue<PdfDictionary>(PdfName.Font, out var fd)
                            && fd.TryGetValue<PdfDictionary>(name, out var f))
                    {
                        return f;
                    }
                }
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
