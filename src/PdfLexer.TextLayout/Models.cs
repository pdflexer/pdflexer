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

public enum TextOverflowMode
{
    Visible,
    Clip,
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

public sealed record TextFontFace(
    string FaceId,
    string FamilyName,
    int Weight,
    byte[] FontData,
    uint FaceIndex = 0);

public sealed class TextFontLibrary
{
    private readonly Dictionary<string, Dictionary<int, TextFontFace>> _faces;

    public TextFontLibrary(IEnumerable<TextFontFace> faces)
    {
        ArgumentNullException.ThrowIfNull(faces);

        _faces = new Dictionary<string, Dictionary<int, TextFontFace>>(StringComparer.OrdinalIgnoreCase);
        foreach (var face in faces)
        {
            if (!_faces.TryGetValue(face.FamilyName, out var weights))
            {
                weights = new Dictionary<int, TextFontFace>();
                _faces[face.FamilyName] = weights;
            }

            weights[face.Weight] = face;
        }
    }

    public IReadOnlyCollection<string> FamilyNames => new ReadOnlyCollection<string>(_faces.Keys.OrderBy(x => x).ToList());

    public IEnumerable<TextFontFace> Faces => _faces.Values.SelectMany(x => x.Values);

    public bool TryResolve(string familyName, int weight, out TextFontFace? face)
    {
        face = null;
        if (!_faces.TryGetValue(familyName, out var weights))
        {
            return false;
        }

        return weights.TryGetValue(weight, out face);
    }

    public bool HasFamily(string familyName) => _faces.ContainsKey(familyName);
}

public sealed record TextSegmentStyle(
    string FamilyName,
    int Weight,
    double FontSize,
    bool Underline = false,
    double CharacterSpacing = 0,
    double WordSpacing = 0,
    double? LineSpacing = null);

public sealed record TextSegment(
    string Text,
    TextSegmentStyle Style);

public sealed record TextBoxLayoutRequest(
    double Width,
    double Height,
    TextFontLibrary FontLibrary,
    IReadOnlyList<TextSegment> Segments)
{
    public TextHorizontalAlignment HorizontalAlignment { get; init; } = TextHorizontalAlignment.Left;
    public TextVerticalAlignment VerticalAlignment { get; init; } = TextVerticalAlignment.Top;
    public TextOverflowMode OverflowMode { get; init; } = TextOverflowMode.Clip;
    public TextResolutionBehavior MissingFontBehavior { get; init; } = TextResolutionBehavior.Error;
    public TextResolutionBehavior MissingGlyphBehavior { get; init; } = TextResolutionBehavior.Error;
    public IReadOnlyList<string> FallbackFamilyNames { get; init; } = Array.Empty<string>();
    public bool PreserveTrailingWhitespaceInWidth { get; init; }
}

public sealed record TextLayoutIssue(
    TextLayoutIssueKind Kind,
    string Message,
    int? SegmentIndex = null,
    string? FamilyName = null,
    int? Weight = null,
    string? FaceId = null);

public sealed record TextLayoutGlyph(
    uint GlyphId,
    uint Cluster,
    double X,
    double Y,
    double Advance,
    double OffsetX,
    double OffsetY);

public sealed record TextLayoutRun(
    int SegmentIndex,
    string Text,
    string FaceId,
    string FamilyName,
    int Weight,
    double FontSize,
    bool Underline,
    double CharacterSpacing,
    double WordSpacing,
    double X,
    double BaselineY,
    double Width,
    double MeasuredWidth,
    double LineHeight,
    IReadOnlyList<TextLayoutGlyph> Glyphs);

public sealed record TextLayoutLine(
    int Index,
    double X,
    double BaselineY,
    double Width,
    double MeasuredWidth,
    double Height,
    double BaselineOffset,
    IReadOnlyList<TextLayoutRun> Runs);

public sealed class TextBoxLayoutResult
{
    public required TextLayoutStatus Status { get; init; }
    public required bool FitsWidth { get; init; }
    public required bool FitsHeight { get; init; }
    public required double MeasuredWidth { get; init; }
    public required double MeasuredHeight { get; init; }
    public required double RenderedWidth { get; init; }
    public required double RenderedHeight { get; init; }
    public required IReadOnlyList<TextLayoutLine> Lines { get; init; }
    public required IReadOnlyList<TextLayoutIssue> Issues { get; init; }

    public bool Success => Status == TextLayoutStatus.Success;
}
