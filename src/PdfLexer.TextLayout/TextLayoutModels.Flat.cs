namespace PdfLexer.TextLayout;

public sealed record TextBoxFitResult(
    TextBoxLayoutResult FittingLayout,
    TextBoxLayoutRequest? RemainderRequest,
    double ConsumedHeight,
    double ConsumedWidth,
    TextBreakKind BreakKind,
    bool HasRemainder)
{
    public TextFragmentBreak FragmentBreak { get; init; } = TextFragmentBreak.None;
    public TextBoxLayoutRequest? FittedRequest { get; init; }

    public TextBoxLayoutResult FittedLayout => FittingLayout;

    public TextBoxLayoutRequest? RemainderContent => RemainderRequest;

    public TextLayoutSize ConsumedSize => new(ConsumedWidth, ConsumedHeight);

    public TextFragmentResult<TextBoxLayoutRequest> Fragment
        => new(
            FittingLayout,
            FittedRequest ?? throw new InvalidOperationException("This fit result does not retain its fitted request."),
            RemainderRequest,
            FragmentBreak,
            HasRemainder);
}

public sealed record TextSegmentStyle(
    string FamilyName,
    int Weight,
    double FontSize,
    bool Underline = false,
    double CharacterSpacing = 0,
    double WordSpacing = 0,
    double? LineSpacing = null,
    TextLineBoxSizing LineBoxSizing = TextLineBoxSizing.AtLeastLineHeight,
    bool Italic = false,
    TextColor? ForegroundColor = null,
    TextColor? BackgroundColor = null,
    bool StrikeThrough = false);

public sealed record TextSegment(
    string Text,
    TextSegmentStyle Style,
    int? SourceStart = null,
    int? SourceLength = null,
    string? SourcePath = null,
    string? SourceNodeId = null);

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
    public TextBoxStyle BoxStyle { get; init; } = new();
    public TextFontMetricSource MetricPreference { get; init; } = TextFontMetricSource.None;
    public TextFlowBreakBefore BreakBefore { get; init; } = TextFlowBreakBefore.Auto;
    public TextFlowBreakAfter BreakAfter { get; init; } = TextFlowBreakAfter.Auto;
    public TextFlowBreakInside BreakInside { get; init; } = TextFlowBreakInside.Auto;
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
    bool Italic,
    bool Underline,
    double CharacterSpacing,
    double WordSpacing,
    TextColor? ForegroundColor,
    TextColor? BackgroundColor,
    double X,
    double BaselineY,
    double Width,
    double MeasuredWidth,
    double LineHeight,
    IReadOnlyList<TextLayoutGlyph> Glyphs,
    bool DrawAsVectorBullet = false,
    bool StrikeThrough = false,
    int? SourceStart = null,
    int? SourceLength = null,
    string? SourcePath = null,
    string? SourceNodeId = null,
    double? RequestedLineHeight = null);

public static class TextLayoutBuiltInFaces
{
    public const string UnorderedListMarkerFaceId = "__pdflexer_builtin_unordered_list_marker";
}

public abstract record TextLayoutDecoration;

public sealed record TextLayoutFillRectDecoration(
    double X,
    double Y,
    double Width,
    double Height,
    TextColor Color,
    double Radius = 0) : TextLayoutDecoration;

public sealed record TextLayoutStrokeRectDecoration(
    double X,
    double Y,
    double Width,
    double Height,
    TextColor Color,
    double Thickness,
    double Radius = 0) : TextLayoutDecoration;

public sealed record TextLayoutLineDecoration(
    double X1,
    double Y1,
    double X2,
    double Y2,
    TextColor Color,
    double Thickness) : TextLayoutDecoration;

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
    public required TextBoxStyle BoxStyle { get; init; }
    public required IReadOnlyList<TextLayoutLine> Lines { get; init; }
    public required IReadOnlyList<TextLayoutIssue> Issues { get; init; }
    public IReadOnlyList<TextLayoutDecoration> Decorations { get; init; } = Array.Empty<TextLayoutDecoration>();

    public bool Success => Status == TextLayoutStatus.Success;

    public double NaturalWidth => MeasuredWidth;

    public double NaturalHeight => MeasuredHeight;

    public double VisibleWidth => RenderedWidth;

    public double VisibleHeight => RenderedHeight;

    public TextLayoutSize NaturalSize => new(NaturalWidth, NaturalHeight);

    public TextLayoutSize VisibleSize => new(VisibleWidth, VisibleHeight);
}

public enum TextBreakKind
{
    None,
    Line,
    Paragraph,
    ListItem,
    TableRow,
    ContainerChild
}
