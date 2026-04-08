namespace PdfLexer.TextLayout;

public sealed record RichContent(
    IReadOnlyList<RichTextBlock> Blocks);

public sealed record RichSliceCut(
    string Path,
    TextBreakKind Kind,
    bool IsStartBoundary,
    bool IsEndBoundary);

public record RichLayoutSplitMetadata(
    string Path,
    TextBreakKind Kind,
    TextFragmentBreakReason BreakReason = TextFragmentBreakReason.Overflow,
    bool IsForced = false);

public sealed record TableSplitMetadata(
    string TableBlockPath,
    int FittedBodyRowCount,
    int RemainingBodyRowCount,
    bool UsedContinuationHeader,
    bool UsedContinuationFooter,
    TextLayoutContinuationKind ContinuationKind = TextLayoutContinuationKind.TableRow,
    string? BoundaryParentPath = null,
    string? ContinuationParentPath = null) : RichLayoutSplitMetadata(TableBlockPath, TextBreakKind.TableRow, TextFragmentBreakReason.Overflow, false);

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

public abstract record InlineNode;

public sealed record TextRunNode(
    string Text,
    TextStyle Style) : InlineNode;

public sealed record LineBreakNode() : InlineNode;

public abstract record RichTextBlock
{
    public TextFlowBreakBefore BreakBefore { get; init; } = TextFlowBreakBefore.Auto;
    public TextFlowBreakAfter BreakAfter { get; init; } = TextFlowBreakAfter.Auto;
    public TextFlowBreakInside BreakInside { get; init; } = TextFlowBreakInside.Auto;
}

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
    string Marker = "\u2022",
    ListMarkerStyle MarkerStyle = ListMarkerStyle.Disc) : RichTextBlock;

public sealed record OrderedListBlock(
    IReadOnlyList<ListItemBlock> Items,
    int StartIndex = 1,
    double MarginBlockEnd = 0,
    ListMarkerStyle MarkerStyle = ListMarkerStyle.Decimal) : RichTextBlock;

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
    TableCellStyle? Style = null);

public sealed record TableDataCellBlock(
    IReadOnlyList<RichTextBlock> Blocks,
    int ColSpan = 1,
    int RowSpan = 1,
    TableCellStyle? Style = null) : TableCellBlock(Blocks, ColSpan, RowSpan, Style);

public sealed record TableHeaderCellBlock(
    IReadOnlyList<RichTextBlock> Blocks,
    int ColSpan = 1,
    int RowSpan = 1,
    TableCellStyle? Style = null) : TableCellBlock(Blocks, ColSpan, RowSpan, Style);

public sealed record TableSectionBlock(
    TableSectionKind Kind,
    IReadOnlyList<TableRowBlock> Rows);

public sealed record TableRowBlock(
    IReadOnlyList<TableCellBlock> Cells,
    TableRowStyle? Style = null);

public sealed record TableBlock(
    IReadOnlyList<TableColumnDefinition> Columns,
    IReadOnlyList<TableSectionBlock> Sections,
    TableStyle? Style = null,
    TableLayoutSpec? Layout = null) : RichTextBlock
{
    public TableBlock(
        IReadOnlyList<TableRowBlock> rows,
        TableStyle? Style = null,
        TableLayoutSpec? Layout = null)
        : this(Array.Empty<TableColumnDefinition>(), new[] { new TableSectionBlock(TableSectionKind.Body, rows) }, Style, Layout)
    {
    }

    public IReadOnlyList<TableRowBlock> Rows => Sections.SelectMany(static x => x.Rows).ToArray();

    public TablePaginationPolicy Pagination => (Layout ?? new TableLayoutSpec()).ResolvedPagination;

    public TableWidthSpec Width => (Layout ?? new TableLayoutSpec()).ResolvedWidth;
}

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
    public double? ListIndent { get; init; }
    public double? ListMarkerGap { get; init; }
    public TextBoxStyle BoxStyle { get; init; } = new();
    public TextFontMetricSource MetricPreference { get; init; } = TextFontMetricSource.None;
}
