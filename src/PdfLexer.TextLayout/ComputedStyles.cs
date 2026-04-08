namespace PdfLexer.TextLayout;

internal readonly record struct ComputedInlineStyle(
    string FamilyName,
    int Weight,
    double FontSize,
    bool Italic,
    bool Underline,
    bool StrikeThrough,
    double CharacterSpacing,
    double WordSpacing,
    TextColor? ForegroundColor,
    TextColor? BackgroundColor);

internal readonly record struct ComputedParagraphStyle(
    TextHorizontalAlignment TextAlign,
    double? LineHeight,
    TextLineBoxSizing LineBoxSizing,
    double MarginBlockEnd,
    double MarginBlockStart,
    double TextIndent,
    TextWhiteSpaceMode WhiteSpace,
    TextOverflowWrapMode OverflowWrap,
    TextWordBreakMode WordBreak);

internal readonly record struct ComputedListStyle(
    string MarkerText,
    ListMarkerStyle MarkerStyle,
    bool UseVectorMarker,
    double MarginBlockEnd);

internal readonly record struct ComputedOrderedListStyle(
    int StartIndex,
    double MarginBlockEnd,
    ListMarkerStyle MarkerStyle);

internal readonly record struct ComputedBoxStyle(
    TextColor? BackgroundColor,
    ResolvedBorder Border,
    double BorderRadius,
    ResolvedEdgeSizes Padding)
{
    public ResolvedBoxEdges Edges => new(Padding, Border.Widths);
}

internal readonly record struct ComputedContainerStyle(
    ComputedBoxStyle Box,
    double Gap,
    double MarginBlockEnd);

internal readonly record struct ComputedTableStyle(
    TextColor? BackgroundColor,
    ResolvedBorder Border,
    ResolvedBorder CellBorder,
    ResolvedEdgeSizes CellPadding,
    double MarginBlockEnd);

internal readonly record struct ComputedTableRowStyle(
    TextColor? BackgroundColor,
    double? MinHeight,
    bool KeepTogether,
    TextFlowBreakBefore BreakBefore,
    TextFlowBreakAfter BreakAfter,
    TableRowSplitMode SplitMode);

internal readonly record struct ComputedTableColumnStyle(
    TextColor? BackgroundColor);

internal readonly record struct ComputedTableCellStyle(
    TextColor? BackgroundColor,
    ResolvedEdgeSizes Padding,
    TextHorizontalAlignment? TextAlign,
    TextVerticalAlignment VerticalAlign);
