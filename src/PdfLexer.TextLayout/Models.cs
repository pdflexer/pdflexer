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

public sealed record TextBoxFitResult(
    TextBoxLayoutResult FittingLayout,
    TextBoxLayoutRequest? RemainderRequest,
    double ConsumedHeight,
    double ConsumedWidth,
    TextBreakKind BreakKind,
    bool HasRemainder);

public sealed record TextSegmentStyle(
    string FamilyName,
    int Weight,
    double FontSize,
    bool Underline = false,
    double CharacterSpacing = 0,
    double WordSpacing = 0,
    double? LineSpacing = null,
    bool Italic = false,
    TextColor? ForegroundColor = null,
    TextColor? BackgroundColor = null);

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
    public TextBoxStyle BoxStyle { get; init; } = new();
    public TextFontMetricSource MetricPreference { get; init; } = TextFontMetricSource.None;
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
    bool DrawAsVectorBullet = false);

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

public sealed record RichContent(
    IReadOnlyList<RichTextBlock> Blocks);

public sealed record RichSliceCut(
    string Path,
    TextBreakKind Kind,
    bool IsStartBoundary,
    bool IsEndBoundary);

public record RichLayoutSplitMetadata(
    string Path,
    TextBreakKind Kind);

public sealed record TableSplitMetadata(
    string TableBlockPath,
    int FittedBodyRowCount,
    int RemainingBodyRowCount,
    bool UsedContinuationHeader,
    bool UsedContinuationFooter) : RichLayoutSplitMetadata(TableBlockPath, TextBreakKind.TableRow);

public sealed record RichContentSlice(
    IReadOnlyList<RichTextBlock> Blocks,
    int StartOpenDepth = 0,
    int EndOpenDepth = 0,
    IReadOnlyList<RichSliceCut>? Cuts = null)
{
    public IReadOnlyList<RichSliceCut> CutMetadata { get; init; } = Cuts ?? Array.Empty<RichSliceCut>();

    public RichSliceCursor GetCursor() => new(this);

    public RichTextBoxLayoutRequest ToRequestLike(RichTextBoxLayoutRequest template)
        => template with { Blocks = Blocks };
}

public sealed record RichTextBoxFitResult(
    TextBoxLayoutResult FittingLayout,
    RichContentSlice FittingSlice,
    RichContentSlice? RemainderSlice,
    double ConsumedHeight,
    double ConsumedWidth,
    TextBreakKind BreakKind,
    bool HasRemainder,
    IReadOnlyList<RichLayoutSplitMetadata>? Metadata = null)
{
    public IReadOnlyList<RichLayoutSplitMetadata> SplitMetadata { get; init; } = Metadata ?? Array.Empty<RichLayoutSplitMetadata>();
}

public sealed record TextStyle(
    string FamilyName,
    int Weight,
    double FontSize,
    bool Italic = false,
    bool Underline = false,
    double CharacterSpacing = 0,
    double WordSpacing = 0,
    TextColor? ForegroundColor = null,
    TextColor? BackgroundColor = null);

public sealed record ParagraphStyle(
    TextHorizontalAlignment TextAlign = TextHorizontalAlignment.Left,
    double? LineHeight = null,
    double MarginBlockEnd = 0);

public sealed record TableStyle(
    TextColor? BackgroundColor = null,
    TextColor? BorderColor = null,
    double BorderWidth = 0,
    TextColor? CellBorderColor = null,
    double CellBorderWidth = 0,
    double CellPadding = 4,
    double MarginBlockEnd = 0);

public sealed record TableContinuationPolicy(
    IReadOnlyList<TableRowBlock>? HeaderRows = null,
    IReadOnlyList<TableRowBlock>? FooterRows = null)
{
    public IReadOnlyList<TableRowBlock> ContinuationHeaderRows { get; init; } = HeaderRows ?? Array.Empty<TableRowBlock>();
    public IReadOnlyList<TableRowBlock> ContinuationFooterRows { get; init; } = FooterRows ?? Array.Empty<TableRowBlock>();
}

public sealed record TableCellStyle(
    TextColor? BackgroundColor = null,
    double? Padding = null,
    TextHorizontalAlignment? TextAlign = null)
{
    public double ResolvePadding(TableStyle tableStyle)
        => Padding ?? tableStyle.CellPadding;
}

public sealed record LayoutContainerStyle(
    TextColor? BackgroundColor = null,
    TextColor? BorderColor = null,
    double BorderWidth = 0,
    double Padding = 0,
    double Gap = 0,
    double MarginBlockEnd = 0)
{
    public double Inset => Math.Max(0d, BorderWidth) + Math.Max(0d, Padding);

    public TextBoxStyle ToTextBoxStyle()
        => new(BackgroundColor, BorderColor, BorderWidth, 0, Padding);
}

public abstract record InlineNode;

public sealed record TextRunNode(
    string Text,
    TextStyle Style) : InlineNode;

public sealed record LineBreakNode() : InlineNode;

public abstract record RichTextBlock;

public sealed record ParagraphBlock(
    IReadOnlyList<InlineNode> Inlines,
    ParagraphStyle? Style = null) : RichTextBlock;

public sealed record HeadingBlock(
    int Level,
    IReadOnlyList<InlineNode> Inlines,
    ParagraphStyle? Style = null) : RichTextBlock;

public sealed record ListItemBlock(
    IReadOnlyList<RichTextBlock> Blocks);

public sealed record UnorderedListBlock(
    IReadOnlyList<ListItemBlock> Items,
    double MarginBlockEnd = 0,
    string Marker = "\u2022") : RichTextBlock;

public sealed record OrderedListBlock(
    IReadOnlyList<ListItemBlock> Items,
    int StartIndex = 1,
    double MarginBlockEnd = 0) : RichTextBlock;

public sealed record LayoutChild(
    IReadOnlyList<RichTextBlock> Blocks,
    double Weight = 1,
    double? FixedSize = null,
    TextVerticalAlignment VerticalAlignment = TextVerticalAlignment.Top,
    TextBoxStyle? BoxStyle = null);

public abstract record TableCellBlock(
    IReadOnlyList<RichTextBlock> Blocks,
    int ColSpan = 1,
    int RowSpan = 1,
    double? ColWidth = null,
    TableCellStyle? Style = null);

public sealed record TableDataCellBlock(
    IReadOnlyList<RichTextBlock> Blocks,
    int ColSpan = 1,
    int RowSpan = 1,
    double? ColWidth = null,
    TableCellStyle? Style = null) : TableCellBlock(Blocks, ColSpan, RowSpan, ColWidth, Style);

public sealed record TableHeaderCellBlock(
    IReadOnlyList<RichTextBlock> Blocks,
    int ColSpan = 1,
    int RowSpan = 1,
    double? ColWidth = null,
    TableCellStyle? Style = null) : TableCellBlock(Blocks, ColSpan, RowSpan, ColWidth, Style);

public sealed record TableRowBlock(
    IReadOnlyList<TableCellBlock> Cells);

public sealed record TableBlock(
    IReadOnlyList<TableRowBlock> Rows,
    TableStyle? Style = null,
    TableContinuationPolicy? ContinuationPolicy = null) : RichTextBlock;

public sealed record RowBlock(
    IReadOnlyList<LayoutChild> Children,
    double? Height = null,
    LayoutContainerStyle? Style = null) : RichTextBlock;

public sealed record ColumnBlock(
    IReadOnlyList<LayoutChild> Children,
    double? Height = null,
    LayoutContainerStyle? Style = null) : RichTextBlock;

public sealed record RichTextBoxLayoutRequest(
    double Width,
    double Height,
    TextFontLibrary FontLibrary,
    IReadOnlyList<RichTextBlock> Blocks)
{
    public TextVerticalAlignment VerticalAlignment { get; init; } = TextVerticalAlignment.Top;
    public TextOverflowMode OverflowMode { get; init; } = TextOverflowMode.Clip;
    public TextResolutionBehavior MissingFontBehavior { get; init; } = TextResolutionBehavior.Error;
    public TextResolutionBehavior MissingGlyphBehavior { get; init; } = TextResolutionBehavior.Error;
    public IReadOnlyList<string> FallbackFamilyNames { get; init; } = Array.Empty<string>();
    public bool PreserveTrailingWhitespaceInWidth { get; init; }
    public double ListIndent { get; init; } = 18d;
    public double ListMarkerGap { get; init; } = 2d;
    public TextBoxStyle BoxStyle { get; init; } = new();
    public TextFontMetricSource MetricPreference { get; init; } = TextFontMetricSource.None;
}

public sealed class RichSliceCursor
{
    private readonly List<(string Path, object Node)> _nodes;
    private int _index;

    internal RichSliceCursor(RichContentSlice slice)
    {
        _nodes = new List<(string Path, object Node)>();
        for (var i = 0; i < slice.Blocks.Count; i++)
        {
            AddBlock(slice.Blocks[i], $"Blocks[{i}]");
        }
    }

    public string? CurrentPath => _index >= 0 && _index < _nodes.Count ? _nodes[_index].Path : null;

    public object? CurrentNode => _index >= 0 && _index < _nodes.Count ? _nodes[_index].Node : null;

    public bool MoveNext()
    {
        if (_index + 1 >= _nodes.Count)
        {
            return false;
        }

        _index++;
        return true;
    }

    public bool MovePrevious()
    {
        if (_index <= 0)
        {
            return false;
        }

        _index--;
        return true;
    }

    public bool MoveToFirstChild()
    {
        var currentPath = CurrentPath;
        if (currentPath is null)
        {
            return false;
        }

        var prefix = currentPath + ".";
        for (var i = _index + 1; i < _nodes.Count; i++)
        {
            if (_nodes[i].Path.StartsWith(prefix, StringComparison.Ordinal))
            {
                _index = i;
                return true;
            }
        }

        return false;
    }

    public bool MoveToParent()
    {
        var currentPath = CurrentPath;
        if (string.IsNullOrEmpty(currentPath))
        {
            return false;
        }

        var dotIndex = currentPath.LastIndexOf('.');
        if (dotIndex <= 0)
        {
            return false;
        }

        var parentPath = currentPath[..dotIndex];
        for (var i = _index - 1; i >= 0; i--)
        {
            if (string.Equals(_nodes[i].Path, parentPath, StringComparison.Ordinal))
            {
                _index = i;
                return true;
            }
        }

        return false;
    }

    private void AddBlock(RichTextBlock block, string path)
    {
        _nodes.Add((path, block));
        switch (block)
        {
            case UnorderedListBlock unordered:
                for (var i = 0; i < unordered.Items.Count; i++)
                {
                    AddListItem(unordered.Items[i], $"{path}.Items[{i}]");
                }
                break;
            case OrderedListBlock ordered:
                for (var i = 0; i < ordered.Items.Count; i++)
                {
                    AddListItem(ordered.Items[i], $"{path}.Items[{i}]");
                }
                break;
            case TableBlock table:
                for (var rowIndex = 0; rowIndex < table.Rows.Count; rowIndex++)
                {
                    AddTableRow(table.Rows[rowIndex], $"{path}.Rows[{rowIndex}]");
                }
                break;
            case RowBlock row:
                for (var i = 0; i < row.Children.Count; i++)
                {
                    AddLayoutChild(row.Children[i], $"{path}.Children[{i}]");
                }
                break;
            case ColumnBlock column:
                for (var i = 0; i < column.Children.Count; i++)
                {
                    AddLayoutChild(column.Children[i], $"{path}.Children[{i}]");
                }
                break;
        }
    }

    private void AddListItem(ListItemBlock item, string path)
    {
        _nodes.Add((path, item));
        for (var i = 0; i < item.Blocks.Count; i++)
        {
            AddBlock(item.Blocks[i], $"{path}.Blocks[{i}]");
        }
    }

    private void AddLayoutChild(LayoutChild child, string path)
    {
        _nodes.Add((path, child));
        for (var i = 0; i < child.Blocks.Count; i++)
        {
            AddBlock(child.Blocks[i], $"{path}.Blocks[{i}]");
        }
    }

    private void AddTableRow(TableRowBlock row, string path)
    {
        _nodes.Add((path, row));
        for (var i = 0; i < row.Cells.Count; i++)
        {
            AddTableCell(row.Cells[i], $"{path}.Cells[{i}]");
        }
    }

    private void AddTableCell(TableCellBlock cell, string path)
    {
        _nodes.Add((path, cell));
        for (var i = 0; i < cell.Blocks.Count; i++)
        {
            AddBlock(cell.Blocks[i], $"{path}.Blocks[{i}]");
        }
    }
}
