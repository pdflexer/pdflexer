namespace PdfLexer.TextLayout;

internal readonly record struct LayoutConstraints(
    double AvailableWidth,
    double AvailableHeight,
    TextOverflowMode OverflowMode,
    bool ClipToHeight);

internal sealed record BlockLayoutResult(
    TextBoxLayoutResult Layout,
    LayoutConstraints Constraints)
{
    public TextLayoutSize NaturalSize => Layout.NaturalSize;

    public TextLayoutSize VisibleSize => Layout.VisibleSize;

    public double NaturalWidth => Layout.NaturalWidth;

    public double NaturalHeight => Layout.NaturalHeight;

    public double VisibleWidth => Layout.VisibleWidth;

    public double VisibleHeight => Layout.VisibleHeight;
}

internal sealed record BlockFitResult<T>(
    T? FittedContent,
    T? RemainderContent,
    BlockLayoutResult FittedLayout,
    TextBreakKind BreakKind,
    bool HasRemainder);
