using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer.Fonts
{
    public class Glyph
    {
        // TODO unify unicode handling for char vs string
        public string MultiChar { get; internal set; }
        public char Char { get; internal set; }
        public float w0 { get; internal set; }
        public float w1 { get; internal set; }
        public bool IsWordSpace { get; internal set; } // single byte character code 32 when simple font
                                                       // composite font if 32 is single byte code
        public decimal[] BBox { get; internal set; }
        public uint? CodePoint { get; internal set; }
        public bool Undefined { get; set; }
        public Dictionary<char,float>? Kernings { get; set; }
        public string? Name { get; set; }
        public bool GuessedUnicode { get; set; }

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
                BBox = BBox,
                Undefined = Undefined,
                CodePoint = CodePoint,
                Kernings = Kernings,
                MultiChar = MultiChar,
                Name = Name,
                GuessedUnicode = GuessedUnicode
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
