using System.Collections.ObjectModel;

namespace PdfLexer.TextLayout;

public enum TextHorizontalAlignment
{
    Left,
    Center,
    Right
}

public enum TextVerticalAlignment
{
    Top,
    Center,
    Bottom
}

public enum TextWhiteSpaceMode
{
    Normal,
    PreWrap
}

public enum TextOverflowWrapMode
{
    Normal,
    BreakWord,
    Anywhere
}

public enum TextWordBreakMode
{
    Normal,
    BreakAll,
    KeepAll
}

public enum TextLineBoxSizing
{
    AtLeastLineHeight,
    Exact
}

public enum ListMarkerStyle
{
    Disc,
    Circle,
    Square,
    Decimal,
    LowerAlpha,
    UpperAlpha
}

public enum TextOverflowMode
{
    Visible,
    Clip,
    Fragment,
    Error
}

public enum TextResolutionBehavior
{
    Error,
    UseFallbackFamilies
}

public enum TextLayoutStatus
{
    Success,
    Overflow,
    Error
}

public enum TextLayoutIssueKind
{
    MissingFamily,
    MissingWeight,
    MissingGlyph,
    Overflow
}

public readonly record struct TextColor(byte R, byte G, byte B);

public sealed record TextBoxStyle(
    TextColor? BackgroundColor = null,
    TextColor? BorderColor = null,
    double BorderWidth = 0,
    double BorderRadius = 0,
    double Padding = 0)
{
    public double Inset => Math.Max(0d, BorderWidth) + Math.Max(0d, Padding);
}

public enum TextFontMetricSource
{
    None,
    Typographic,
    HorizontalHeader,
    Windows
}

public sealed record TextFontFace(
    string FaceId,
    string FamilyName,
    int Weight,
    byte[] FontData,
    uint FaceIndex = 0,
    bool Italic = false,
    int UnitsPerEm = 0,
    double Ascent = 0,
    double Descent = 0,
    double LineGap = 0,
    TextFontMetricSource MetricSource = TextFontMetricSource.None,
    double TypographicAscent = 0,
    double TypographicDescent = 0,
    double TypographicLineGap = 0,
    double HorizontalHeaderAscent = 0,
    double HorizontalHeaderDescent = 0,
    double HorizontalHeaderLineGap = 0,
    double WindowsAscent = 0,
    double WindowsDescent = 0,
    double WindowsLineGap = 0)
{
    public bool HasLayoutMetrics => UnitsPerEm > 0 && Ascent > 0 && Descent < 0;

    public (double Ascent, double Descent, double LineGap, TextFontMetricSource Source) ResolveMetrics(TextFontMetricSource preference)
    {
        if (TryGetMetrics(preference, out var preferred))
        {
            return preferred;
        }

        if (TryGetMetrics(MetricSource, out var current))
        {
            return current;
        }

        if (TryGetMetrics(TextFontMetricSource.Typographic, out var typo))
        {
            return typo;
        }

        if (TryGetMetrics(TextFontMetricSource.HorizontalHeader, out var hhea))
        {
            return hhea;
        }

        if (TryGetMetrics(TextFontMetricSource.Windows, out var win))
        {
            return win;
        }

        return (Ascent, Descent, LineGap, MetricSource);
    }

    private bool TryGetMetrics(TextFontMetricSource source, out (double Ascent, double Descent, double LineGap, TextFontMetricSource Source) metrics)
    {
        metrics = source switch
        {
            TextFontMetricSource.Typographic => (TypographicAscent, TypographicDescent, TypographicLineGap, TextFontMetricSource.Typographic),
            TextFontMetricSource.HorizontalHeader => (HorizontalHeaderAscent, HorizontalHeaderDescent, HorizontalHeaderLineGap, TextFontMetricSource.HorizontalHeader),
            TextFontMetricSource.Windows => (WindowsAscent, WindowsDescent, WindowsLineGap, TextFontMetricSource.Windows),
            _ => (0d, 0d, 0d, TextFontMetricSource.None)
        };

        return UnitsPerEm > 0 && metrics.Ascent > 0 && metrics.Descent < 0;
    }
}

public sealed class TextFontLibrary
{
    private readonly Dictionary<string, Dictionary<int, Dictionary<bool, TextFontFace>>> _faces;

    public TextFontLibrary(IEnumerable<TextFontFace> faces)
    {
        ArgumentNullException.ThrowIfNull(faces);

        _faces = new Dictionary<string, Dictionary<int, Dictionary<bool, TextFontFace>>>(StringComparer.OrdinalIgnoreCase);
        foreach (var face in faces)
        {
            if (!_faces.TryGetValue(face.FamilyName, out var weights))
            {
                weights = new Dictionary<int, Dictionary<bool, TextFontFace>>();
                _faces[face.FamilyName] = weights;
            }

            if (!weights.TryGetValue(face.Weight, out var variants))
            {
                variants = new Dictionary<bool, TextFontFace>();
                weights[face.Weight] = variants;
            }

            variants[face.Italic] = face;
        }
    }

    public IReadOnlyCollection<string> FamilyNames => new ReadOnlyCollection<string>(_faces.Keys.OrderBy(x => x).ToList());

    public IEnumerable<TextFontFace> Faces => _faces.Values.SelectMany(x => x.Values).SelectMany(x => x.Values);

    public bool TryResolve(string familyName, int weight, out TextFontFace? face)
        => TryResolve(familyName, weight, italic: false, out face);

    public bool TryResolve(string familyName, int weight, bool italic, out TextFontFace? face)
    {
        face = null;
        if (!_faces.TryGetValue(familyName, out var weights))
        {
            return false;
        }

        if (!weights.TryGetValue(weight, out var variants))
        {
            return false;
        }

        return variants.TryGetValue(italic, out face)
            || variants.TryGetValue(false, out face)
            || variants.TryGetValue(true, out face);
    }

    public bool HasFamily(string familyName) => _faces.ContainsKey(familyName);
}

public readonly record struct TextLayoutSize(double Width, double Height);
