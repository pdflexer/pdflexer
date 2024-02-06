using PdfLexer.DOM;

namespace PdfLexer.Fonts;

public class TrueTypeEmbeddedFont : IPdfEmbeddableFont
{
    public string PostScriptName { get; internal set; } = null!;
    public PdfName DefaultEncoding { get; internal set; } = null!;
    public Dictionary<char, Glyph> Glyphs { get; set; } = null!;
    public FontDescriptor Descriptor { get; set; } = null!;

    public IWritableFont GetDefaultEncodedFont(UnknownCharHandling handling = default) => new TrueTypeSimpleWritableFont(this, handling);
    public IWritableFont GetType0EncodedFont(UnknownCharHandling handling = default) => new TrueTypeWritableFont(this, handling);
    public IWritableFont GetCustomEncodedFont(IEnumerable<char> characters, UnknownCharHandling handling = UnknownCharHandling.Error)
    {
        throw new NotImplementedException("Custom encoding not yet implemented for true type fonts");
    }
    internal int UnitsPerEm { get; set; }
    internal double LLx { get; set; }
    internal double LLy { get; set; }
    internal double URx { get; set; }
    internal double URy { get; set; }
    internal int ApproxStemV { get; set; }
    internal int Ascent { get; set; }
    internal int Descent { get; set; }
    internal int CapHeight { get; set; }
    internal bool Bold { get; set; }
    internal int FirstChar { get; set; }
    internal int LastChar { get; set; }
    internal double ItalicAngle { get; set; }
    internal bool FixedPitch { get; set; }
    internal int HorizontalMetricsCount { get; set; }
    public int GlyphCount { get; set; }
    internal int[] GlyphWidths { get; set; } = null!;
    internal double ToPDFGlyphSpace(double value) => (value * 1000) / UnitsPerEm;
}