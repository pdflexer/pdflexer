using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer.Fonts;

internal class GlyphSet
{
    public Glyph notdef { get; }
    private Glyph[] b1;
    private Dictionary<uint, Glyph> bm;

    public GlyphSet(Glyph[] b1, Dictionary<uint, Glyph> glyphs, Glyph notdef)
    {
        this.notdef = notdef;
        this.b1 = b1;
        this.bm = glyphs;
    }
    public GlyphSet(IEnumerable<Glyph> glyphs, Glyph notdef)
    {
        this.notdef = notdef;
        b1 = new Glyph[256];
        bm = new Dictionary<uint, Glyph>();
        foreach (var g in glyphs)
        {
            if (g.CodePoint < 256)
            {
                b1[g.CodePoint.Value] = g;
            }
            else
            {
                bm[g.CodePoint ?? 0] = g;
            }
        }
    }

    public void Add(uint charCode, Glyph g)
    {
        bm[charCode] = g;
        if (charCode < 256)
        {
            b1[charCode] = g;
        }
    }

    public Glyph GetGlyph(uint charCode)
    {
        Glyph? glyph;
        if (charCode < 256 && b1 != null)
        {
            glyph = b1[charCode];
        }
        else
        {
            bm.TryGetValue(charCode, out glyph);
        }
        glyph ??= notdef;
        return glyph;
    }
}
