using PdfLexer.DOM;
using System;

namespace PdfLexer.Fonts
{
    public interface IWritableFont
    {
        PdfFont GetPdfFont();
        double ConvertFromUnicode(ReadOnlySpan<char> word, Span<byte> content) { return 0; }
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
