using PdfLexer.Operators;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace PdfLexer.Content
{
    public class TextState
    {
        public int Mode { get; internal set; }
        public float FontSize { get; internal set; }
        public float HorizontalScaling { get; internal set; }
        public float CharSpacing { get; internal set; }
        public float WordSpacing { get; internal set; }
        public float TextLeading { get; internal set; }
        public float TextHScale { get; internal set; }
        public float TextRise { get; internal set; }
        public PdfName FontName { get; internal set; }
        public object Font { get; internal set; } // todo
        // fontMatrix ??

        // marked content?
        // will need Apply for BMC BMCProps EMC

        Matrix4x4 TextMatrix { get; set; }
        Matrix4x4 TextRenderingMatrix { get; set; } // todo
                                                    //      Trm = [ T_fs*T_h  0       0 ] x Tm x CTM
                                                    //              0         T_fs    0
                                                    //              0         T_rise  1
        Matrix4x4 TextLineMatrix { get; set; } // set to Tm at beginning of a line of text

        public TextState()
        {
            TextMatrix = Matrix4x4.Identity;
            TextLineMatrix = Matrix4x4.Identity;
        }

        public int GetGlyph(ReadOnlySpan<byte> data, int pos, out Glyph info)
        {
            // TODO
            info = new Glyph
            {
                Char = (char)data[pos]
            };
            return 1;
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
            if (Mode == 0)
            {
                tx = (-tj / 1000) * FontSize * HorizontalScaling;
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

        public void ApplyShift(UnappliedGlyph glyph)
        {
            ApplyTj(glyph.Shift);
        }

        public void ApplyCharShift(UnappliedGlyph glyph)
        {
            Apply(glyph.Glyph);
        }

        public void Apply(Glyph info)
        {
            // shift
            float tx = 0f;
            float ty = 0f;
            if (Mode == 0)
            {
                // tx = ((w0-Tj/1000) * T_fs + T_c + T_w?) * Th
                // var s = (info.w0 - tj / 1000) * FontSize + CharSpacing; // Tj pre applied
                var s = (info.w0) * FontSize + CharSpacing;
                if (info.IsWordSpace) { s += WordSpacing; }
                tx = s * HorizontalScaling;
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
            // Tm = Tlm = [ 1  0  0 ] x Tlm
            //              0  1  0
            //              tx ty 1

            TextLineMatrix = new Matrix4x4(
                          1f, 0f, 0f, 0f,
                          0f, 1f, 0f, 0f,
                          (float)op.tx, (float)op.ty, 1f, 0f,
                          0f, 0f, 0f, 1f) * TextLineMatrix;

            TextMatrix = TextLineMatrix;
        }

        public void Apply(TD_Op op)
        {
            // -ty TL
            // tx, ty Td

            TextLineMatrix = new Matrix4x4(
                          1f, 0f, 0f, 0f,
                          0f, 1f, 0f, 0f,
                          (float)op.tx, (float)op.ty, 1f, 0f,
                          0f, 0f, 0f, 1f) * TextLineMatrix;

            TextMatrix = TextLineMatrix;
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
            TextLineMatrix = new Matrix4x4(
                          1f, 0f, 0f, 0f,
                          0f, 1f, 0f, 0f,
                          0f, -TextLeading, 1f, 0f,
                          0f, 0f, 0f, 1f) * TextLineMatrix;

            TextMatrix = TextLineMatrix;
        }


        public void Apply(Tj_Op op)
        {
            // TODO
        }

        public void Apply(doublequote_Op op)
        {
            WordSpacing = (float)op.aw;
            CharSpacing = (float)op.ac;
            Apply(new singlequote_Op(op.text));
        }

        public void Apply(singlequote_Op op)
        {
            // TODO
        }

        public void Apply(TJ_Op op)
        {
            foreach (var item in op.info)
            {
                if (item.Shift != 0m)
                {
                    // shift
                }
                else
                {
                    // string
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
            Mode = op.render;
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
            // TODO -> check Resources/ExtGState/{Name}/Font
            // holds array of [ font_iref size ] -> similar to Tf but iref instead of name
        }

        public void Apply(Tf_Op op)
        {
            FontSize = (float)op.size;
            if (Font != null && FontName == op.font)
            {
                return;
            }
            // TODO
            FontName = op.font;
            
        }

    }
}
