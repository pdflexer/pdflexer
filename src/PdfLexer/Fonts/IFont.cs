using PdfLexer.DOM;
using System;
using System.Collections.Generic;

namespace PdfLexer.Fonts
{
    public struct SizedChar
    {
        public int ByteCount;
        public double Width;
        public double PrevKern;
    }
    public interface IWritableFont
    {
        /// <summary>
        /// Returns the Pdf font dictionary object that represents this font.
        /// </summary>
        /// <returns></returns>
        PdfDictionary GetPdfFont();
        /// <summary>
        /// Reads chars at from a given string and writes each character individually into
        /// the provided buffer.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        IEnumerable<SizedChar> ConvertFromUnicode(string text, int start, int length, byte[] buffer);
        /// <summary>
        /// Determines if unicode space ' ' is considered a word space according to
        /// pdf spec.
        /// </summary>
        /// <returns></returns>
        bool SpaceIsWordSpace();
        /// <summary>
        /// Default line separation for this font.
        /// </summary>
        double LineHeight { get; }
    }

    public interface IReadableFont
    {
        int GetGlyph(ReadOnlySpan<byte> data, int os, out Glyph glyph);
        bool IsVertical { get; }
    }
}
