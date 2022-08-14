
namespace PdfLexer.Fonts;

internal class SingleByteFont : IReadableFont
{
    private Glyph?[] Glyphs;
    private Glyph NotDef;

    public bool IsVertical => false;

    public string Name { get; }

    public SingleByteFont(PdfName name, Glyph?[] glyphs, Glyph notdef)
    {
        Glyphs = glyphs;
        Name = name.Value;
        NotDef = notdef;
    }

    public int GetGlyph(ReadOnlySpan<byte> data, int os, out Glyph glyph)
    {
        glyph = NotDef;
        var c = (int)data[os];
        if (c < Glyphs.Length)
        {
            var g = Glyphs[c];
            if (g != null)
            {
                glyph = g;
            }
            else
            {

            }
        }
        return 1;
    }
}
