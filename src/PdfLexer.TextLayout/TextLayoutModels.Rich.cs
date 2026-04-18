namespace PdfLexer.TextLayout;

public readonly record struct NodeId(string Value)
{
    public static NodeId New()
        => new(Guid.NewGuid().ToString("N"));

    public override string ToString()
        => Value;
}

public sealed record RichContent(
    IReadOnlyList<RichTextBlock> Blocks);

public sealed record RichSliceCut(
    string Path,
    string? NodeId,
    TextBreakKind Kind,
    bool IsStartBoundary,
    bool IsEndBoundary)
{
    public RichSliceCut(
        string Path,
        TextBreakKind Kind,
        bool IsStartBoundary,
        bool IsEndBoundary)
        : this(Path, null, Kind, IsStartBoundary, IsEndBoundary)
    {
    }
}

public record RichLayoutSplitMetadata(
    string Path,
    string? NodeId,
    TextBreakKind Kind,
    TextFragmentBreakReason BreakReason = TextFragmentBreakReason.Overflow,
    bool IsForced = false)
{
    public RichLayoutSplitMetadata(
        string Path,
        TextBreakKind Kind,
        TextFragmentBreakReason BreakReason = TextFragmentBreakReason.Overflow,
        bool IsForced = false)
        : this(Path, null, Kind, BreakReason, IsForced)
    {
    }
}

public sealed record TableSplitMetadata(
    string TableBlockPath,
    string? TableBlockNodeId,
    int FittedBodyRowCount,
    int RemainingBodyRowCount,
    bool UsedContinuationHeader,
    bool UsedContinuationFooter,
    TextLayoutContinuationKind ContinuationKind = TextLayoutContinuationKind.TableRow,
    string? BoundaryParentPath = null,
    string? BoundaryParentNodeId = null,
    string? ContinuationParentPath = null,
    string? ContinuationParentNodeId = null) : RichLayoutSplitMetadata(TableBlockPath, TableBlockNodeId, TextBreakKind.TableRow, TextFragmentBreakReason.Overflow, false)
{
    public TableSplitMetadata(
        string TableBlockPath,
        int FittedBodyRowCount,
        int RemainingBodyRowCount,
        bool UsedContinuationHeader,
        bool UsedContinuationFooter,
        TextLayoutContinuationKind ContinuationKind = TextLayoutContinuationKind.TableRow,
        string? BoundaryParentPath = null,
        string? ContinuationParentPath = null)
        : this(
            TableBlockPath,
            null,
            FittedBodyRowCount,
            RemainingBodyRowCount,
            UsedContinuationHeader,
            UsedContinuationFooter,
            ContinuationKind,
            BoundaryParentPath,
            null,
            ContinuationParentPath,
            null)
    {
    }
}

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
    public TextFragmentBreak FragmentBreak { get; init; } = TextFragmentBreak.None;
    public int LineCount => FittingLayout.LineCount;
    public TextLayoutLineViewCollection LineViews => FittingLayout.LineViews;
    public TextLayoutLineView GetLine(int index) => FittingLayout.GetLine(index);
    public IReadOnlyList<TextLayoutDecoration> Decorations => FittingLayout.Decorations;
    public IReadOnlyList<TextLayoutLine> MaterializeLegacyLines() => FittingLayout.MaterializeLegacyLines();
    public TextBoxLayoutResult MaterializeLegacyLayout() => FittingLayout.MaterializeLegacyLayout();

    public TextBoxLayoutResult FittedLayout => FittingLayout;

    public RichContentSlice FittedContent => FittingSlice;

    public RichContentSlice? RemainderContent => RemainderSlice;

    public TextLayoutSize ConsumedSize => new(ConsumedWidth, ConsumedHeight);

    public TextFragmentResult<RichContentSlice> Fragment
        => new(FittingLayout, FittingSlice, RemainderSlice, FragmentBreak, HasRemainder);
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
    TextColor? BackgroundColor = null,
    bool StrikeThrough = false);

public sealed record ParagraphStyle(
    TextStyle? BaseTextStyle = null,
    TextHorizontalAlignment TextAlign = TextHorizontalAlignment.Left,
    double? LineHeight = null,
    TextLineBoxSizing LineBoxSizing = TextLineBoxSizing.AtLeastLineHeight,
    double MarginBlockEnd = 0,
    double MarginBlockStart = 0,
    double TextIndent = 0,
    TextWhiteSpaceMode WhiteSpace = TextWhiteSpaceMode.Normal,
    TextOverflowWrapMode OverflowWrap = TextOverflowWrapMode.Normal,
    TextWordBreakMode WordBreak = TextWordBreakMode.Normal);

public sealed record TableStyle(
    TextColor? BackgroundColor = null,
    TextColor? BorderColor = null,
    double BorderWidth = 0,
    TextColor? CellBorderColor = null,
    double CellBorderWidth = 0,
    double CellPadding = 4,
    double MarginBlockEnd = 0);

public sealed record TableRowStyle(
    TextColor? BackgroundColor = null,
    double? MinHeight = null,
    bool KeepTogether = false,
    TextFlowBreakBefore BreakBefore = TextFlowBreakBefore.Auto,
    TextFlowBreakAfter BreakAfter = TextFlowBreakAfter.Auto,
    TableRowSplitMode? SplitMode = null);

public sealed record TableColumnStyle(
    TextColor? BackgroundColor = null);

public enum TableSectionKind
{
    Header,
    Body,
    Footer
}

public enum TableRowSplitMode
{
    Auto,
    Avoid,
    BlockBoundary
}

public enum TableCellSplitMode
{
    Auto,
    Avoid,
    BlockBoundary
}

public abstract record TableWidthSpec;

public sealed record TableFixedWidth(double Points) : TableWidthSpec;

public sealed record TablePercentWidth(double Percent) : TableWidthSpec;

public sealed record TableAutoWidth() : TableWidthSpec;

public abstract record ColumnWidthSpec;

public sealed record ColumnFixedWidth(double Points) : ColumnWidthSpec;

public sealed record ColumnPercentWidth(double Percent) : ColumnWidthSpec;

public sealed record ColumnMinMaxWidth(double? MinPoints = null, double? MaxPoints = null) : ColumnWidthSpec;

public sealed record ColumnFlexWidth(double Weight = 1) : ColumnWidthSpec;

public sealed record ColumnAutoWidth() : ColumnWidthSpec;

public sealed record TableColumnDefinition(
    ColumnWidthSpec Width,
    string? Key = null,
    TableColumnStyle? Style = null);

public sealed record TablePaginationPolicy(
    bool RepeatHeaderRows = true,
    bool RepeatFooterRows = false,
    TableRowSplitMode RowSplitMode = TableRowSplitMode.Auto,
    TableCellSplitMode CellSplitMode = TableCellSplitMode.Auto,
    bool KeepHeaderWithNextRow = true,
    bool KeepFooterWithPreviousRow = true);

public sealed record TableLayoutSpec(
    TableWidthSpec? Width = null,
    TablePaginationPolicy? Pagination = null)
{
    public TableWidthSpec ResolvedWidth => Width ?? new TableAutoWidth();

    public TablePaginationPolicy ResolvedPagination => Pagination ?? new TablePaginationPolicy();
}

public sealed record TableCellStyle(
    TextColor? BackgroundColor = null,
    double? Padding = null,
    TextHorizontalAlignment? TextAlign = null,
    TextVerticalAlignment? VerticalAlign = null)
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

public abstract record InlineMark;

public sealed record BoldMark(int Weight = 700) : InlineMark;

public sealed record ItalicMark(bool Value = true) : InlineMark;

public sealed record UnderlineMark(bool Value = true) : InlineMark;

public sealed record StrikeThroughMark(bool Value = true) : InlineMark;

public sealed record FontFamilyMark(string FamilyName) : InlineMark;

public sealed record FontWeightMark(int Weight) : InlineMark;

public sealed record FontSizeMark(double FontSize) : InlineMark;

public sealed record ForegroundColorMark(TextColor? Color) : InlineMark;

public sealed record BackgroundColorMark(TextColor? Color) : InlineMark;

public sealed record CharacterSpacingMark(double Value) : InlineMark;

public sealed record WordSpacingMark(double Value) : InlineMark;

public sealed record LinkMark(string Href) : InlineMark;

public abstract record InlineNode(IReadOnlyList<InlineMark>? Marks = null)
{
    public IReadOnlyList<InlineMark> Marks { get; init; } = InlineMarkSet.Normalize(Marks);
}

public record TextInline(
    string Text,
    IReadOnlyList<InlineMark>? Marks = null) : InlineNode(Marks)
{
    public TextInline(string Text, TextStyle style)
        : this(Text, InlineMarkSet.FromTextStyle(style))
    {
    }
}

public sealed record TextRunNode(
    string Text,
    TextStyle Style) : TextInline(Text, Style);

public record BreakInline(
    IReadOnlyList<InlineMark>? Marks = null) : InlineNode(Marks);

public sealed record LineBreakNode() : BreakInline();

public sealed record TabInline(
    IReadOnlyList<InlineMark>? Marks = null) : InlineNode(Marks);

public sealed record FieldInline(
    NodeId Id,
    string FieldKind,
    string? FieldRef = null,
    string? DisplayText = null,
    IReadOnlyList<InlineMark>? Marks = null) : InlineNode(Marks)
{
    public FieldInline(string FieldKind, string? FieldRef = null, string? DisplayText = null, IReadOnlyList<InlineMark>? Marks = null)
        : this(NodeId.New(), FieldKind, FieldRef, DisplayText, Marks)
    {
    }
}

public sealed record ReferenceInline(
    NodeId Id,
    string ReferenceKind,
    string Target,
    string? DisplayText = null,
    IReadOnlyList<InlineMark>? Marks = null) : InlineNode(Marks)
{
    public ReferenceInline(string ReferenceKind, string Target, string? DisplayText = null, IReadOnlyList<InlineMark>? Marks = null)
        : this(NodeId.New(), ReferenceKind, Target, DisplayText, Marks)
    {
    }
}

public readonly record struct BlockPayloadMeasureContext(
    double AvailableWidth,
    double AvailableHeight,
    TextFontLibrary FontLibrary);

public interface IBlockPayload
{
    TextLayoutSize Measure(BlockPayloadMeasureContext context);

    string Serialize();
}

public interface IInlinePayload
{
    string DisplayText { get; }

    string Serialize();
}

public sealed record InlineObjectInline(
    NodeId Id,
    string ObjectKind,
    IInlinePayload Payload,
    IReadOnlyList<InlineMark>? Marks = null) : InlineNode(Marks)
{
    public InlineObjectInline(string ObjectKind, IInlinePayload Payload, IReadOnlyList<InlineMark>? Marks = null)
        : this(NodeId.New(), ObjectKind, Payload, Marks)
    {
    }
}

public abstract record RichTextBlock(NodeId Id)
{
    public TextFlowBreakBefore BreakBefore { get; init; } = TextFlowBreakBefore.Auto;
    public TextFlowBreakAfter BreakAfter { get; init; } = TextFlowBreakAfter.Auto;
    public TextFlowBreakInside BreakInside { get; init; } = TextFlowBreakInside.Auto;
    public bool KeepTogether { get; init; }
    public bool KeepWithNext { get; init; }
}

public sealed record ParagraphBlock(
    NodeId Id,
    IReadOnlyList<InlineNode> Inlines,
    ParagraphStyle? Style = null) : RichTextBlock(Id)
{
    public ParagraphBlock(
        IReadOnlyList<InlineNode> Inlines,
        ParagraphStyle? Style = null)
        : this(NodeId.New(), InlineMarkSet.NormalizeAdjacentText(Inlines), Style)
    {
    }
}

public sealed record HeadingBlock(
    NodeId Id,
    int Level,
    IReadOnlyList<InlineNode> Inlines,
    ParagraphStyle? Style = null) : RichTextBlock(Id)
{
    public HeadingBlock(
        int Level,
        IReadOnlyList<InlineNode> Inlines,
        ParagraphStyle? Style = null)
        : this(NodeId.New(), Level, InlineMarkSet.NormalizeAdjacentText(Inlines), Style)
    {
    }
}

public sealed record ListItemBlock(
    NodeId Id,
    IReadOnlyList<RichTextBlock> Blocks)
{
    public ListItemBlock(IReadOnlyList<RichTextBlock> Blocks)
        : this(NodeId.New(), Blocks)
    {
    }
}

public sealed record UnorderedListBlock(
    NodeId Id,
    IReadOnlyList<ListItemBlock> Items,
    double MarginBlockEnd = 0,
    string Marker = "\u2022",
    ListMarkerStyle MarkerStyle = ListMarkerStyle.Disc) : RichTextBlock(Id)
{
    public UnorderedListBlock(
        IReadOnlyList<ListItemBlock> Items,
        double MarginBlockEnd = 0,
        string Marker = "\u2022",
        ListMarkerStyle MarkerStyle = ListMarkerStyle.Disc)
        : this(NodeId.New(), Items, MarginBlockEnd, Marker, MarkerStyle)
    {
    }
}

public sealed record OrderedListBlock(
    NodeId Id,
    IReadOnlyList<ListItemBlock> Items,
    int StartIndex = 1,
    double MarginBlockEnd = 0,
    ListMarkerStyle MarkerStyle = ListMarkerStyle.Decimal) : RichTextBlock(Id)
{
    public OrderedListBlock(
        IReadOnlyList<ListItemBlock> Items,
        int StartIndex = 1,
        double MarginBlockEnd = 0,
        ListMarkerStyle MarkerStyle = ListMarkerStyle.Decimal)
        : this(NodeId.New(), Items, StartIndex, MarginBlockEnd, MarkerStyle)
    {
    }
}

public sealed record LayoutChild(
    IReadOnlyList<RichTextBlock> Blocks,
    double Weight = 1,
    double? FixedSize = null,
    TextVerticalAlignment VerticalAlignment = TextVerticalAlignment.Top,
    TextBoxStyle? BoxStyle = null);

public abstract record TableCellBlock(
    NodeId Id,
    IReadOnlyList<RichTextBlock> Blocks,
    int ColSpan = 1,
    int RowSpan = 1,
    TableCellStyle? Style = null);

public sealed record TableDataCellBlock(
    NodeId Id,
    IReadOnlyList<RichTextBlock> Blocks,
    int ColSpan = 1,
    int RowSpan = 1,
    TableCellStyle? Style = null) : TableCellBlock(Id, Blocks, ColSpan, RowSpan, Style)
{
    public TableDataCellBlock(
        IReadOnlyList<RichTextBlock> Blocks,
        int ColSpan = 1,
        int RowSpan = 1,
        TableCellStyle? Style = null)
        : this(NodeId.New(), Blocks, ColSpan, RowSpan, Style)
    {
    }
}

public sealed record TableHeaderCellBlock(
    NodeId Id,
    IReadOnlyList<RichTextBlock> Blocks,
    int ColSpan = 1,
    int RowSpan = 1,
    TableCellStyle? Style = null) : TableCellBlock(Id, Blocks, ColSpan, RowSpan, Style)
{
    public TableHeaderCellBlock(
        IReadOnlyList<RichTextBlock> Blocks,
        int ColSpan = 1,
        int RowSpan = 1,
        TableCellStyle? Style = null)
        : this(NodeId.New(), Blocks, ColSpan, RowSpan, Style)
    {
    }
}

public sealed record TableSectionBlock(
    NodeId Id,
    TableSectionKind Kind,
    IReadOnlyList<TableRowBlock> Rows)
{
    public TableSectionBlock(
        TableSectionKind Kind,
        IReadOnlyList<TableRowBlock> Rows)
        : this(NodeId.New(), Kind, Rows)
    {
    }
}

public sealed record TableRowBlock(
    NodeId Id,
    IReadOnlyList<TableCellBlock> Cells,
    TableRowStyle? Style = null)
{
    public TableRowBlock(
        IReadOnlyList<TableCellBlock> Cells,
        TableRowStyle? Style = null)
        : this(NodeId.New(), Cells, Style)
    {
    }
}

public sealed record TableBlock(
    NodeId Id,
    IReadOnlyList<TableColumnDefinition> Columns,
    IReadOnlyList<TableSectionBlock> Sections,
    TableStyle? Style = null,
    TableLayoutSpec? Layout = null) : RichTextBlock(Id)
{
    public TableBlock(
        IReadOnlyList<TableRowBlock> rows,
        TableStyle? Style = null,
        TableLayoutSpec? Layout = null)
        : this(NodeId.New(), Array.Empty<TableColumnDefinition>(), new[] { new TableSectionBlock(TableSectionKind.Body, rows) }, Style, Layout)
    {
    }

    public TableBlock(
        IReadOnlyList<TableColumnDefinition> Columns,
        IReadOnlyList<TableSectionBlock> Sections,
        TableStyle? Style = null,
        TableLayoutSpec? Layout = null)
        : this(NodeId.New(), Columns, Sections, Style, Layout)
    {
    }

    public IReadOnlyList<TableRowBlock> Rows => Sections.SelectMany(static x => x.Rows).ToArray();

    public TablePaginationPolicy Pagination => (Layout ?? new TableLayoutSpec()).ResolvedPagination;

    public TableWidthSpec Width => (Layout ?? new TableLayoutSpec()).ResolvedWidth;
}

public sealed record RowBlock(
    NodeId Id,
    IReadOnlyList<LayoutChild> Children,
    double? Height = null,
    LayoutContainerStyle? Style = null) : RichTextBlock(Id)
{
    public RowBlock(
        IReadOnlyList<LayoutChild> Children,
        double? Height = null,
        LayoutContainerStyle? Style = null)
        : this(NodeId.New(), Children, Height, Style)
    {
    }
}

public sealed record ColumnBlock(
    NodeId Id,
    IReadOnlyList<LayoutChild> Children,
    double? Height = null,
    LayoutContainerStyle? Style = null) : RichTextBlock(Id)
{
    public ColumnBlock(
        IReadOnlyList<LayoutChild> Children,
        double? Height = null,
        LayoutContainerStyle? Style = null)
        : this(NodeId.New(), Children, Height, Style)
    {
    }
}

public sealed record EmbeddedObjectBlock(
    NodeId Id,
    string ObjectKind,
    IBlockPayload Payload,
    TextBoxStyle? Style = null) : RichTextBlock(Id)
{
    public EmbeddedObjectBlock(
        string ObjectKind,
        IBlockPayload Payload,
        TextBoxStyle? Style = null)
        : this(NodeId.New(), ObjectKind, Payload, Style)
    {
    }
}

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
    public double? ListIndent { get; init; }
    public double? ListMarkerGap { get; init; }
    public TextBoxStyle BoxStyle { get; init; } = new();
    public TextFontMetricSource MetricPreference { get; init; } = TextFontMetricSource.None;
}

public static class InlineMarkSet
{
    public static IReadOnlyList<InlineMark> Normalize(IReadOnlyList<InlineMark>? marks)
    {
        if (marks is null || marks.Count == 0)
        {
            return Array.Empty<InlineMark>();
        }

        var byType = new Dictionary<Type, InlineMark>();
        foreach (var mark in marks)
        {
            byType[mark.GetType()] = mark;
        }

        return byType
            .OrderBy(static x => x.Key.FullName, StringComparer.Ordinal)
            .Select(static x => x.Value)
            .ToArray();
    }

    public static IReadOnlyList<InlineMark> FromTextStyle(TextStyle style, TextStyle? baseStyle = null)
    {
        var marks = new List<InlineMark>(10);
        var b = baseStyle;
        if (b is null || !string.Equals(b.FamilyName, style.FamilyName, StringComparison.Ordinal))
            marks.Add(new FontFamilyMark(style.FamilyName));
        if (b is null || b.Weight != style.Weight)
            marks.Add(new FontWeightMark(style.Weight));
        if (b is null || Math.Abs(b.FontSize - style.FontSize) > 0.0001d)
            marks.Add(new FontSizeMark(style.FontSize));
        if (style.Italic && (b is null || b.Italic != style.Italic))
            marks.Add(new ItalicMark(style.Italic));
        if (style.Underline && (b is null || b.Underline != style.Underline))
            marks.Add(new UnderlineMark(style.Underline));
        if (style.StrikeThrough && (b is null || b.StrikeThrough != style.StrikeThrough))
            marks.Add(new StrikeThroughMark(style.StrikeThrough));
        if (Math.Abs(style.CharacterSpacing) > 0.0001d && (b is null || Math.Abs(b.CharacterSpacing - style.CharacterSpacing) > 0.0001d))
            marks.Add(new CharacterSpacingMark(style.CharacterSpacing));
        if (Math.Abs(style.WordSpacing) > 0.0001d && (b is null || Math.Abs(b.WordSpacing - style.WordSpacing) > 0.0001d))
            marks.Add(new WordSpacingMark(style.WordSpacing));
        if (style.ForegroundColor is not null && (b is null || b.ForegroundColor != style.ForegroundColor))
            marks.Add(new ForegroundColorMark(style.ForegroundColor));
        if (style.BackgroundColor is not null && (b is null || b.BackgroundColor != style.BackgroundColor))
            marks.Add(new BackgroundColorMark(style.BackgroundColor));
        return Normalize(marks);
    }

    public static TextStyle Apply(TextStyle baseStyle, IReadOnlyList<InlineMark>? marks)
    {
        var resolved = baseStyle;
        foreach (var mark in Normalize(marks))
        {
            resolved = mark switch
            {
                BoldMark bold => resolved with { Weight = bold.Weight },
                ItalicMark italic => resolved with { Italic = italic.Value },
                UnderlineMark underline => resolved with { Underline = underline.Value },
                StrikeThroughMark strike => resolved with { StrikeThrough = strike.Value },
                FontFamilyMark family => resolved with { FamilyName = family.FamilyName },
                FontWeightMark weight => resolved with { Weight = weight.Weight },
                FontSizeMark size => resolved with { FontSize = size.FontSize },
                ForegroundColorMark foreground => resolved with { ForegroundColor = foreground.Color },
                BackgroundColorMark background => resolved with { BackgroundColor = background.Color },
                CharacterSpacingMark spacing => resolved with { CharacterSpacing = spacing.Value },
                WordSpacingMark spacing => resolved with { WordSpacing = spacing.Value },
                _ => resolved
            };
        }

        return resolved;
    }

    public static bool MarksEqual(IReadOnlyList<InlineMark>? left, IReadOnlyList<InlineMark>? right)
    {
        var normalizedLeft = Normalize(left);
        var normalizedRight = Normalize(right);
        if (normalizedLeft.Count != normalizedRight.Count)
        {
            return false;
        }

        for (var i = 0; i < normalizedLeft.Count; i++)
        {
            if (!EqualityComparer<InlineMark>.Default.Equals(normalizedLeft[i], normalizedRight[i]))
            {
                return false;
            }
        }

        return true;
    }

    public static IReadOnlyList<InlineNode> NormalizeAdjacentText(IReadOnlyList<InlineNode> inlines)
    {
        if (inlines.Count <= 1)
        {
            return inlines;
        }

        var normalized = new List<InlineNode>(inlines.Count);
        foreach (var inline in inlines)
        {
            if (inline is TextInline text && string.IsNullOrEmpty(text.Text))
            {
                continue;
            }

            if (inline is TextInline current &&
                normalized.LastOrDefault() is TextInline previous &&
                MarksEqual(previous.Marks, current.Marks))
            {
                normalized[^1] = previous with { Text = previous.Text + current.Text };
                continue;
            }

            normalized.Add(inline);
        }

        return normalized;
    }
}
