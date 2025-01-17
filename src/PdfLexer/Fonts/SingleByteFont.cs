﻿
namespace PdfLexer.Fonts;

internal class SingleByteFont : IReadableFont
{
    private Glyph?[] Glyphs;
    private Glyph NotDef;

    public bool IsVertical => false;

    public string Name { get; }

    public SingleByteFont(PdfName name, PdfDictionary dict, Glyph?[] glyphs, Glyph notdef)
    {
        Glyphs = glyphs;
        NativeObject = dict;
        Name = name.Value;
        NotDef = notdef;
    }

    public int GetGlyph(ReadOnlySpan<byte> data, int os, out Glyph glyph)
    {
        var c = (int)data[os];
        if (c < Glyphs.Length)
        {
            var g = Glyphs[c];
            if (g != null)
            {
                glyph = g;
                return 1;
            }
        }
        var updated = NotDef.Clone();
        updated.CodePoint = (uint)c;
        Glyphs[c] = updated;
        glyph = updated;
        return 1;
    }

    public static SingleByteFont Fallback { get; } = new SingleByteFont(
        "Fallback", null!,
        Predefined.HelveticaGlyphs.DefaultEncoding,
        new Glyph { Char = '\u0000', w0 = (float)0.278, IsWordSpace = false, BBox = new decimal[] { 0m, 0m, 0.278m, 0m }, Undefined = true });

    public PdfDictionary NativeObject { get; }
}
