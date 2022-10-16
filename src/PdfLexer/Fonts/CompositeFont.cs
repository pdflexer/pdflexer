using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer.Fonts;

internal class CompositeFont : IReadableFont
{
    private readonly CMap _encoding;
    private readonly CMap? _cidInfo;
    private readonly GlyphSet _glyphSet;
    private readonly int _notdefBytes;

    public bool IsVertical { get; }
    public string Name { get; }

    public CompositeFont(string name, CMap encoding, GlyphSet glyphSet, int notdefBytes, bool vertical, CMap? cidInfo = null)
    {
        Name = name;
        IsVertical = vertical;
        _encoding = encoding;
        _cidInfo = cidInfo;
        _glyphSet = glyphSet;
        _notdefBytes = notdefBytes;
    }

    public int GetGlyph(ReadOnlySpan<byte> data, int os, out Glyph glyph)
    {
        int l;
        uint c;

        c = _encoding.GetCodePoint(data, os, out l);
        if (l == 0)
        {
            glyph = _glyphSet.notdef;
            return _notdefBytes;
        }
        c = _encoding.GetCID(c); // convert cp to cid, does nothing with identity

        glyph = _glyphSet.GetGlyph(c);
        if (glyph == _glyphSet.notdef && _cidInfo != null && _cidInfo.HasMapping)
        {
            if (_cidInfo.TryGetFallback(c, out var val))
            {
                glyph = glyph.Clone();
                glyph.MultiChar = val.MultiChar;
                glyph.Char = (char)val.Code;
                glyph.BBox = _glyphSet.notdef.BBox;
            }
        }

        return l;
    }
}
