using PdfLexer.Operators;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace PdfLexer.Content
{
    public class TextState
    {
        int Mode { get; set; }
        float FontSize { get; set; }
        float HorizontalScaling { get; set; }
        float CharSpacing { get; set; }
        float WordSpacing { get; set; }
        float TextLeading { get; set; }
        Matrix4x4 TextMatrix { get; set; }
        Matrix4x4 TextRenderingMatrix { get; set; } // todo
                                                    // Tm = Tlm = [ T_fs*T_h  0       0 ] x Tm x CTM
                                                    //              0         T_fs    0
                                                    //              0         T_rise  1
        Matrix4x4 TextLineMatrix { get; set; }

        public TextState()
        {
            TextMatrix = Matrix4x4.Identity;
            TextLineMatrix = Matrix4x4.Identity;
        }

        public int GetGlyph(ReadOnlySpan<byte> data, int pos, out Glyph info)
        {
            info = default;
            return 0;
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

    }
}
