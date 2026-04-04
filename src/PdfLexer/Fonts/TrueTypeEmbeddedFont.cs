using PdfLexer.DOM;

namespace PdfLexer.Fonts;

public enum FontLayoutMetricSource
{
    None,
    Typographic,
    HorizontalHeader,
    Windows
}

public readonly record struct FontLayoutMetrics(
    int UnitsPerEm,
    double Ascent,
    double Descent,
    double LineGap,
    FontLayoutMetricSource Source);

public readonly record struct FontMetricSourceSet(
    FontLayoutMetrics Typographic,
    FontLayoutMetrics HorizontalHeader,
    FontLayoutMetrics Windows);

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
    internal int LineGap { get; set; }
    internal int TypoAscent { get; set; }
    internal int TypoDescent { get; set; }
    internal int TypoLineGap { get; set; }
    internal int WindowsAscent { get; set; }
    internal int WindowsDescent { get; set; }
    internal int HorizontalHeaderAscent { get; set; }
    internal int HorizontalHeaderDescent { get; set; }
    internal int HorizontalHeaderLineGap { get; set; }
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

    public FontLayoutMetrics GetLayoutMetrics()
    {
        if (TypoAscent > 0 && TypoDescent < 0)
        {
            return new FontLayoutMetrics(UnitsPerEm, TypoAscent, TypoDescent, TypoLineGap, FontLayoutMetricSource.Typographic);
        }

        if (HorizontalHeaderAscent > 0 && HorizontalHeaderDescent < 0)
        {
            return new FontLayoutMetrics(UnitsPerEm, HorizontalHeaderAscent, HorizontalHeaderDescent, HorizontalHeaderLineGap, FontLayoutMetricSource.HorizontalHeader);
        }

        if (WindowsAscent > 0 && WindowsDescent < 0)
        {
            return new FontLayoutMetrics(UnitsPerEm, WindowsAscent, WindowsDescent, 0, FontLayoutMetricSource.Windows);
        }

        return new FontLayoutMetrics(UnitsPerEm, 0, 0, 0, FontLayoutMetricSource.None);
    }

    public FontMetricSourceSet GetMetricSources()
        => new(
            new FontLayoutMetrics(UnitsPerEm, TypoAscent, TypoDescent, TypoLineGap, FontLayoutMetricSource.Typographic),
            new FontLayoutMetrics(UnitsPerEm, HorizontalHeaderAscent, HorizontalHeaderDescent, HorizontalHeaderLineGap, FontLayoutMetricSource.HorizontalHeader),
            new FontLayoutMetrics(UnitsPerEm, WindowsAscent, WindowsDescent, 0, FontLayoutMetricSource.Windows));
}
