namespace PdfLexer.TextLayout;

internal abstract record TextLayoutDecorationIntent;

internal sealed record FillRectDecorationIntent(
    double X,
    double Y,
    double Width,
    double Height,
    TextColor Color,
    double Radius = 0d,
    string? SourcePath = null) : TextLayoutDecorationIntent;

internal sealed record StrokeRectDecorationIntent(
    double X,
    double Y,
    double Width,
    double Height,
    TextColor Color,
    double Thickness,
    double Radius = 0d,
    string? SourcePath = null) : TextLayoutDecorationIntent;

internal sealed record LineDecorationIntent(
    double X1,
    double Y1,
    double X2,
    double Y2,
    TextColor Color,
    double Thickness,
    string? SourcePath = null) : TextLayoutDecorationIntent;

internal sealed record InlineHighlightDecorationIntent(
    double X,
    double Y,
    double Width,
    double Height,
    TextColor Color);

internal sealed record TextStrokeDecorationIntent(
    double X1,
    double Y,
    double X2,
    double Thickness,
    TextColor Color);

internal sealed record VectorBulletDecorationIntent(
    double CenterX,
    double CenterY,
    double Radius,
    TextColor Color);

internal static class TextLayoutDecorationIntentMapper
{
    public static TextLayoutDecorationIntent ToIntent(TextLayoutDecoration decoration)
        => decoration switch
        {
            TextLayoutFillRectDecoration fill => new FillRectDecorationIntent(fill.X, fill.Y, fill.Width, fill.Height, fill.Color, fill.Radius, fill.SourcePath),
            TextLayoutStrokeRectDecoration stroke => new StrokeRectDecorationIntent(stroke.X, stroke.Y, stroke.Width, stroke.Height, stroke.Color, stroke.Thickness, stroke.Radius, stroke.SourcePath),
            TextLayoutLineDecoration line => new LineDecorationIntent(line.X1, line.Y1, line.X2, line.Y2, line.Color, line.Thickness, line.SourcePath),
            _ => throw new NotSupportedException($"Unsupported decoration type: {decoration.GetType().Name}")
        };

    public static TextLayoutDecoration ToDecoration(TextLayoutDecorationIntent intent)
        => intent switch
        {
            FillRectDecorationIntent fill => new TextLayoutFillRectDecoration(fill.X, fill.Y, fill.Width, fill.Height, fill.Color, fill.Radius, fill.SourcePath),
            StrokeRectDecorationIntent stroke => new TextLayoutStrokeRectDecoration(stroke.X, stroke.Y, stroke.Width, stroke.Height, stroke.Color, stroke.Thickness, stroke.Radius, stroke.SourcePath),
            LineDecorationIntent line => new TextLayoutLineDecoration(line.X1, line.Y1, line.X2, line.Y2, line.Color, line.Thickness, line.SourcePath),
            _ => throw new NotSupportedException($"Unsupported decoration intent type: {intent.GetType().Name}")
        };

    public static TextLayoutDecorationIntent Offset(TextLayoutDecorationIntent intent, double xOffset, double yOffset)
        => intent switch
        {
            FillRectDecorationIntent fill => fill with { X = fill.X + xOffset, Y = fill.Y + yOffset },
            StrokeRectDecorationIntent stroke => stroke with { X = stroke.X + xOffset, Y = stroke.Y + yOffset },
            LineDecorationIntent line => line with
            {
                X1 = line.X1 + xOffset,
                X2 = line.X2 + xOffset,
                Y1 = line.Y1 + yOffset,
                Y2 = line.Y2 + yOffset
            },
            _ => throw new NotSupportedException($"Unsupported decoration intent type: {intent.GetType().Name}")
        };

    public static double GetRight(TextLayoutDecorationIntent intent)
        => intent switch
        {
            FillRectDecorationIntent fill => fill.X + fill.Width,
            StrokeRectDecorationIntent stroke => stroke.X + stroke.Width,
            LineDecorationIntent line => Math.Max(line.X1, line.X2),
            _ => 0d
        };

    public static double GetBottom(TextLayoutDecorationIntent intent)
        => intent switch
        {
            FillRectDecorationIntent fill => fill.Y + fill.Height,
            StrokeRectDecorationIntent stroke => stroke.Y + stroke.Height,
            LineDecorationIntent line => Math.Max(line.Y1, line.Y2),
            _ => 0d
        };
}
