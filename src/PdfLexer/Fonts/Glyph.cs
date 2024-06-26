﻿using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace PdfLexer.Fonts
{
    public class Glyph
    {
        // TODO unify unicode handling for char vs string
        public string? MultiChar { get; internal set; }
        public char Char { get; internal set; }
        public float w0 { get; internal set; }
        public float w1 { get; internal set; }
        public bool IsWordSpace { get; internal set; } // single byte character code 32 when simple font
                                                       // composite font if 32 is single byte code
        public decimal[]? BBox { get; internal set; }
        public uint? CodePoint { get; internal set; }
        public uint? CID { get; internal set; }
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

    public readonly struct GlyphOrShift
    {
        internal GlyphOrShift(Glyph? glyph, double shift, int bytes=0)
        {
            Glyph = glyph;
            Shift = shift;
            Bytes = bytes;
        }

        public GlyphOrShift(double shift)
        {
            Glyph = null;
            Shift = shift;
            Bytes = 0;
        }

        public GlyphOrShift(Glyph glyph)
        {
            Glyph = glyph;
            Bytes = 0;
        }

        public readonly Glyph? Glyph;
        public readonly double Shift;
        internal readonly int Bytes;
    }

    public readonly struct GlyphOrShift<T> where T : struct, IFloatingPoint<T>
    {
        internal GlyphOrShift(Glyph? glyph, T shift, int bytes = 0)
        {
            Glyph = glyph;
            Shift = shift;
            Bytes = bytes;
        }

        public GlyphOrShift(T shift)
        {
            Glyph = null;
            Shift = shift;
            Bytes = 0;
        }

        public GlyphOrShift(Glyph glyph)
        {
            Glyph = glyph;
            Bytes = 0;
            Shift = T.Zero;
        }

        public readonly Glyph? Glyph;
        public readonly T Shift;
        internal readonly int Bytes;
    }
}
