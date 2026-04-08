namespace PdfLexer.TextLayout;

internal readonly record struct ResolvedEdgeSizes(
    double Top,
    double Right,
    double Bottom,
    double Left)
{
    public double Horizontal => Left + Right;

    public double Vertical => Top + Bottom;

    public static ResolvedEdgeSizes Uniform(double value)
    {
        var resolved = Math.Max(0d, value);
        return new ResolvedEdgeSizes(resolved, resolved, resolved, resolved);
    }
}

internal readonly record struct ResolvedBorder(
    ResolvedEdgeSizes Widths,
    TextColor? Color)
{
    public bool HasVisibleStroke => Color is not null && (Widths.Top > 0d || Widths.Right > 0d || Widths.Bottom > 0d || Widths.Left > 0d);

    public double MaxWidth => Math.Max(Math.Max(Widths.Top, Widths.Right), Math.Max(Widths.Bottom, Widths.Left));

    public static ResolvedBorder Uniform(double width, TextColor? color)
        => new(ResolvedEdgeSizes.Uniform(width), color);
}

internal readonly record struct ResolvedBoxEdges(
    ResolvedEdgeSizes Padding,
    ResolvedEdgeSizes BorderWidths)
{
    public ResolvedEdgeSizes Insets => new(
        Padding.Top + BorderWidths.Top,
        Padding.Right + BorderWidths.Right,
        Padding.Bottom + BorderWidths.Bottom,
        Padding.Left + BorderWidths.Left);

    public double HorizontalInset => Insets.Horizontal;

    public double VerticalInset => Insets.Vertical;
}
