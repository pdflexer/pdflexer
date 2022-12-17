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

        uint cp = _encoding.GetCodePoint(data, os, out l);
        if (l == 0)
        {
            glyph = _glyphSet.notdef;
            // TODO set codepoint on new notdef glyph
            return _notdefBytes;
        }
        var cid = _encoding.GetCID(cp); // convert cp to cid, does nothing with identity

        glyph = _glyphSet.GetGlyph(cid);
        if (glyph == _glyphSet.notdef)
        {
            glyph = glyph.Clone();
            if (_cidInfo != null && _cidInfo.HasMapping && _cidInfo.TryGetFallback(cid, out var val))
            {
                glyph.MultiChar = val.MultiChar;
                glyph.Char = (char)val.Code;
                return l;
            }
            glyph.CodePoint = cp;
            glyph.CID = cid;
        }

        return l;
    }
}
