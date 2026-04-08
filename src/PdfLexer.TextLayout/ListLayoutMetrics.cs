namespace PdfLexer.TextLayout;

internal readonly record struct ResolvedListMetrics(
    double ContentStart,
    double MarkerColumnWidth,
    double MarkerTextAreaWidth,
    double MarkerGap,
    double MarkerVisualWidth,
    double MarkerFontSize);

internal static class ListLayoutMetricsResolver
{
    public static ResolvedListMetrics Build(
        double? explicitContentStart,
        double? explicitMarkerGap,
        double markerFontSize,
        double measuredMarkerWidth)
    {
        var autoGap = Math.Max(2d, Math.Round(markerFontSize * 0.25d, 3));
        var markerGap = explicitMarkerGap.HasValue
            ? Math.Max(0d, explicitMarkerGap.Value)
            : autoGap;
        var minContentStart = Math.Max(Math.Round(markerFontSize * 1.4d, 3), Math.Round(measuredMarkerWidth + markerGap, 3));
        var contentStart = explicitContentStart.HasValue
            ? Math.Max(0d, explicitContentStart.Value)
            : minContentStart;
        var markerColumnWidth = Math.Max(1d, contentStart);
        var markerTextAreaWidth = Math.Max(1d, markerColumnWidth - markerGap);

        return new ResolvedListMetrics(
            contentStart,
            markerColumnWidth,
            markerTextAreaWidth,
            markerGap,
            Math.Max(1d, measuredMarkerWidth),
            markerFontSize);
    }
}

internal static class OrderedListMarkerFormatter
{
    public static string Format(ListMarkerStyle markerStyle, int index, bool includeTrailingSpace = false)
    {
        var marker = markerStyle switch
        {
            ListMarkerStyle.LowerAlpha => $"{ToAlpha(index, upper: false)}.",
            ListMarkerStyle.UpperAlpha => $"{ToAlpha(index, upper: true)}.",
            _ => $"{index}.",
        };

        return includeTrailingSpace ? $"{marker} " : marker;
    }

    private static string ToAlpha(int index, bool upper)
    {
        if (index <= 0)
        {
            return index.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        Span<char> buffer = stackalloc char[16];
        var value = index;
        var pos = buffer.Length;
        while (value > 0)
        {
            value--;
            buffer[--pos] = (char)((upper ? 'A' : 'a') + (value % 26));
            value /= 26;
        }

        return new string(buffer[pos..]);
    }
}
