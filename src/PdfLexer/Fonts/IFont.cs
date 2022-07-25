using PdfLexer.DOM;
using System;
using System.Collections.Generic;

namespace PdfLexer.Fonts
{
    public interface IWritableFont
    {
        PdfDictionary GetPdfFont();
        // double ConvertFromUnicode(ReadOnlySpan<char> word, Span<byte> content);
        IEnumerable<(int ByteCount, double Width, double PrevKern)> ConvertFromUnicode(string text, int start, int length, byte[] buffer);
        bool SpaceIsWordSpace();
        // ascender?
        // descender?
        // LH -> ((this.ascender + gap - this.descender) / 1000) * size;
        double LineHeight { get; }
    }

    public interface IReadableFont
    {
        int GetGlyph(ReadOnlySpan<byte> data, int os, out Glyph glyph);
        bool IsVertical { get; }
    }
}
