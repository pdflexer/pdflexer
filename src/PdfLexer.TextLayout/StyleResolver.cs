namespace PdfLexer.TextLayout;

internal static class StyleResolver
{
    public static ComputedParagraphStyle Resolve(ParagraphStyle? style)
    {
        var resolved = style ?? new ParagraphStyle();
        return new ComputedParagraphStyle(
            resolved.BaseTextStyle ?? new TextStyle("Helvetica", 400, 12),
            resolved.TextAlign,
            resolved.LineHeight,
            resolved.LineBoxSizing,
            resolved.MarginBlockEnd,
            resolved.MarginBlockStart,
            resolved.TextIndent,
            resolved.WhiteSpace,
            resolved.OverflowWrap,
            resolved.WordBreak);
    }

    public static ComputedInlineStyle Resolve(TextStyle style)
        => new(
            style.FamilyName,
            style.Weight,
            style.FontSize,
            style.Italic,
            style.Underline,
            style.StrikeThrough,
            style.CharacterSpacing,
            style.WordSpacing,
            style.ForegroundColor,
            style.BackgroundColor);

    public static ComputedBoxStyle Resolve(TextBoxStyle? style)
    {
        var resolved = style ?? new TextBoxStyle();
        return new ComputedBoxStyle(
            resolved.BackgroundColor,
            ResolvedBorder.Uniform(resolved.BorderWidth, resolved.BorderColor),
            Math.Max(0d, resolved.BorderRadius),
            ResolvedEdgeSizes.Uniform(resolved.Padding));
    }

    public static ComputedContainerStyle Resolve(LayoutContainerStyle? style)
    {
        var resolved = style ?? new LayoutContainerStyle();
        return new ComputedContainerStyle(
            Resolve(resolved.ToTextBoxStyle()),
            Math.Max(0d, resolved.Gap),
            Math.Max(0d, resolved.MarginBlockEnd));
    }

    public static ComputedTableStyle Resolve(TableStyle? style)
    {
        var resolved = style ?? new TableStyle();
        return new ComputedTableStyle(
            resolved.BackgroundColor,
            ResolvedBorder.Uniform(resolved.BorderWidth, resolved.BorderColor),
            ResolvedBorder.Uniform(resolved.CellBorderWidth, resolved.CellBorderColor ?? resolved.BorderColor),
            ResolvedEdgeSizes.Uniform(resolved.CellPadding),
            Math.Max(0d, resolved.MarginBlockEnd));
    }

    public static ComputedTableRowStyle Resolve(TableRowStyle? style, TablePaginationPolicy pagination)
    {
        var resolved = style ?? new TableRowStyle();
        return new ComputedTableRowStyle(
            resolved.BackgroundColor,
            resolved.MinHeight,
            resolved.KeepTogether,
            resolved.BreakBefore,
            resolved.BreakAfter,
            resolved.SplitMode ?? pagination.RowSplitMode);
    }

    public static ComputedTableColumnStyle Resolve(TableColumnStyle? style)
    {
        var resolved = style ?? new TableColumnStyle();
        return new ComputedTableColumnStyle(resolved.BackgroundColor);
    }

    public static ComputedTableCellStyle Resolve(TableCellStyle? style, ComputedTableStyle tableStyle)
    {
        var resolved = style ?? new TableCellStyle();
        var padding = resolved.Padding.HasValue
            ? ResolvedEdgeSizes.Uniform(resolved.Padding.Value)
            : tableStyle.CellPadding;
        return new ComputedTableCellStyle(
            resolved.BackgroundColor,
            padding,
            resolved.TextAlign,
            resolved.VerticalAlign ?? TextVerticalAlignment.Top);
    }

    public static ComputedListStyle Resolve(UnorderedListBlock list)
        => new(
            list.Marker,
            list.MarkerStyle,
            IsVectorBulletMarker(list.Marker, list.MarkerStyle),
            Math.Max(0d, list.MarginBlockEnd));

    public static ComputedOrderedListStyle Resolve(OrderedListBlock list)
        => new(list.StartIndex, Math.Max(0d, list.MarginBlockEnd), list.MarkerStyle);

    public static TextSegmentStyle ToSegmentStyle(ComputedInlineStyle inline, ComputedParagraphStyle paragraph)
        => new(
            inline.FamilyName,
            inline.Weight,
            inline.FontSize,
            inline.Underline,
            inline.CharacterSpacing,
            inline.WordSpacing,
            paragraph.LineHeight,
            paragraph.LineBoxSizing,
            inline.Italic,
            inline.ForegroundColor,
            inline.BackgroundColor,
            inline.StrikeThrough);

    public static TextStyle ToTextStyle(TextSegmentStyle style)
        => new(
            style.FamilyName,
            style.Weight,
            style.FontSize,
            style.Italic,
            style.Underline,
            style.CharacterSpacing,
            style.WordSpacing,
            style.ForegroundColor,
            style.BackgroundColor,
            style.StrikeThrough);

    public static TextStyle ResolveInlineTextStyle(ComputedParagraphStyle paragraph, InlineNode inline)
        => InlineMarkSet.Apply(paragraph.BaseTextStyle, inline.Marks);

    private static bool IsVectorBulletMarker(string markerText, ListMarkerStyle markerStyle)
        => markerStyle == ListMarkerStyle.Disc && string.Equals(markerText, "\u2022", StringComparison.Ordinal);
}
