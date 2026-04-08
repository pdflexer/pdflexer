namespace PdfLexer.TextLayout;

internal static class TextLayoutDiagnosticsBuilder
{
    public static TextLayoutLineDiagnostics? BuildLineDiagnostics(TextLayoutLine line, TextLayoutAnalysisContext? context)
    {
        if (context?.CaptureLineDiagnostics != true)
        {
            return null;
        }

        var requestedLineHeight = line.Runs
            .Select(run => run.RequestedLineHeight)
            .Where(height => height.HasValue)
            .Select(height => height!.Value)
            .DefaultIfEmpty()
            .Max();
        var naturalAscent = line.BaselineOffset;
        var naturalDescent = Math.Max(0d, line.Height - line.BaselineOffset);
        var naturalLineGap = Math.Max(0d, line.Height - naturalAscent - naturalDescent);
        var contributions = context.CaptureDetailedLineDiagnostics
            ? line.Runs
                .Select(run => new TextLayoutLineRunContribution(
                    run.Text,
                    run.FamilyName,
                    run.FontSize,
                    naturalAscent,
                    naturalDescent,
                    naturalLineGap))
                .ToArray()
            : null;

        return new TextLayoutLineDiagnostics(
            line.Height,
            naturalAscent,
            naturalDescent,
            naturalLineGap,
            line.BaselineOffset,
            requestedLineHeight > 0d && line.Height - requestedLineHeight > 0.001d,
            requestedLineHeight > 0d ? requestedLineHeight : null,
            contributions);
    }

    public static TextLayoutListDiagnostics BuildListDiagnostics(ResolvedListMetrics metrics)
        => new(
            metrics.ContentStart,
            metrics.MarkerColumnWidth,
            metrics.MarkerTextAreaWidth,
            metrics.MarkerGap,
            metrics.MarkerVisualWidth,
            metrics.MarkerFontSize);
}
