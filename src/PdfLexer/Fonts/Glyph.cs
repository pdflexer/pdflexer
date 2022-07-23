using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer.Fonts
{
    public class Glyph
    {
        public char Char { get; internal set; }
        public float w0 { get; internal set; }
        public float w1 { get; internal set; }
        public bool IsWordSpace { get; internal set; } // single byte character code 32 when simple font
                                                       // composite font if 32 is single byte code

        public decimal[] BBox { get; internal set; }

        // originalCharCode,
        // fontChar,
        // unicode,
        // accent,
        // width,
        // vmetric,
        // operatorListId,
        // isSpace,
        // isInFont

        public Glyph Clone()
        {
            return new Glyph
            {
                Char = Char,
                w0 = w0,
                w1 = w1,
                IsWordSpace = IsWordSpace,
                BBox = BBox
            };
        }
    }


    public readonly struct UnappliedGlyph
    {
        public UnappliedGlyph(Glyph glyph, float shift)
        {
            Glyph = glyph;
            Shift = shift;

        }
        public readonly Glyph Glyph;
        public readonly float Shift;
    }

}
