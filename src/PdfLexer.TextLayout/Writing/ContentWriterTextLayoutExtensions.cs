using System.Numerics;
using PdfLexer.Content;
using PdfLexer.Fonts;
using PdfLexer.TextLayout;
using PdfLexer.Writing;

namespace PdfLexer.Writing;

public static class ContentWriterTextLayoutExtensions
{
    public static TextBoxLayoutResult MeasureTextBox<T>(this ContentWriter<T> writer, TextBoxLayoutRequest request)
        where T : struct, IFloatingPoint<T>
    {
        var engine = new TextBoxLayoutEngine();
        return engine.Layout(request);
    }

    public static TextBoxFitResult MeasureTextBoxFit<T>(this ContentWriter<T> writer, TextBoxLayoutRequest request)
        where T : struct, IFloatingPoint<T>
    {
        var engine = new TextBoxLayoutEngine();
        return engine.Fit(request);
    }

    public static TextBoxLayoutResult MeasureTextBox<T>(this ContentWriter<T> writer, RichTextBoxLayoutRequest request)
        where T : struct, IFloatingPoint<T>
    {
        var engine = new RichTextBoxLayoutEngine();
        return engine.Layout(request);
    }

    public static RichTextBoxFitResult MeasureTextBoxFit<T>(this ContentWriter<T> writer, RichTextBoxLayoutRequest request)
        where T : struct, IFloatingPoint<T>
    {
        var engine = new RichTextBoxLayoutEngine();
        return engine.Fit(request);
    }

    public static TextBoxLayoutResult WriteTextBox<T>(this ContentWriter<T> writer, PdfRect<T> area, TextBoxLayoutRequest request, PdfTextLayoutFontLibrary fontLibrary)
        where T : struct, IFloatingPoint<T>
    {
        ArgumentNullException.ThrowIfNull(fontLibrary);
        var engine = new TextBoxLayoutEngine();
        var layout = engine.Layout(request);
        return writer.WriteTextBox(area, layout, fontLibrary);
    }

    public static TextBoxLayoutResult WriteTextBox<T>(this ContentWriter<T> writer, PdfRect<T> area, RichTextBoxLayoutRequest request, PdfTextLayoutFontLibrary fontLibrary)
        where T : struct, IFloatingPoint<T>
    {
        ArgumentNullException.ThrowIfNull(fontLibrary);
        var engine = new RichTextBoxLayoutEngine();
        var layout = engine.Layout(request);
        return writer.WriteTextBox(area, layout, fontLibrary);
    }

    public static TextBoxLayoutResult WriteTextBox<T>(this ContentWriter<T> writer, PdfRect<T> area, TextBoxLayoutResult layout, PdfTextLayoutFontLibrary fontLibrary)
        where T : struct, IFloatingPoint<T>
    {
        ArgumentNullException.ThrowIfNull(fontLibrary);
        ArgumentNullException.ThrowIfNull(layout);

        if (layout.Status == TextLayoutStatus.Error)
        {
            return layout;
        }

        DrawBoxStyle(writer, area, layout.BoxStyle);
        DrawLayoutDecorations(writer, area, layout.Decorations);

        var underlines = new List<(double X1, double Y, double X2, double Thickness, TextColor Color)>();
        var backgrounds = new List<(double X, double Y, double Width, double Height, TextColor Color)>();
        var vectorBullets = new List<(double CenterX, double CenterY, double Radius, TextColor Color)>();

        for (var lineIndex = 0; lineIndex < layout.Lines.Count; lineIndex++)
        {
            var line = layout.Lines[lineIndex];
            var lineTop = ToDouble(area.URy) - (line.BaselineY - line.BaselineOffset);
            for (var runIndex = 0; runIndex < line.Runs.Count; runIndex++)
            {
                var run = line.Runs[runIndex];
                if (run.DrawAsVectorBullet)
                {
                    var foreground = run.ForegroundColor ?? new TextColor(0, 0, 0);
                    var bulletRectX = ToDouble(area.LLx) + line.X + run.X;
                    var bulletRectY = lineTop - run.LineHeight;
                    var centerX = bulletRectX + (run.MeasuredWidth / 2d);
                    var centerY = bulletRectY + (run.LineHeight / 2d);
                    var radius = Math.Max(0.5d, Math.Min(run.FontSize * 0.18d, Math.Min(run.MeasuredWidth * 0.45d, run.LineHeight * 0.3d)));
                    vectorBullets.Add((centerX, centerY, radius, foreground));
                    continue;
                }

                if (run.BackgroundColor is not TextColor background || run.MeasuredWidth <= 0)
                {
                    continue;
                }

                var decorationWidth = GetVisibleDecorationWidth(line, run);
                if (decorationWidth <= 0d)
                {
                    continue;
                }

                var rectX = ToDouble(area.LLx) + line.X + run.X;
                var (rectTop, rectHeight) = GetInlineHighlightMetrics(run);
                backgrounds.Add((rectX, ToDouble(area.URy) - rectTop - rectHeight, decorationWidth, rectHeight, background));
            }
        }

        foreach (var background in backgrounds)
        {
            writer.Save();
            writer.SetFillRGB(background.Color.R, background.Color.G, background.Color.B);
            writer.Rect(
                FromDouble<T>(background.X),
                FromDouble<T>(background.Y),
                FromDouble<T>(background.Width),
                FromDouble<T>(background.Height));
            writer.Fill(evenOdd: false);
            writer.Restore();
        }

        string? currentFaceId = null;
        double? currentFontSize = null;
        double? currentCharSpacing = null;
        double? currentWordSpacing = null;
        TextColor? currentFillColor = null;

        writer.BeginText();
        foreach (var line in layout.Lines)
        {
            foreach (var run in line.Runs)
            {
                if (run.DrawAsVectorBullet || string.IsNullOrEmpty(run.Text))
                {
                    continue;
                }

                if (!fontLibrary.TryGetWritableFont(run.FaceId, out var writableFont) || writableFont == null)
                {
                    throw new PdfLexerException($"No writable font was found for face '{run.FaceId}'.");
                }

                if (currentFaceId != run.FaceId || currentFontSize != run.FontSize)
                {
                    writer.Font(writableFont, FromDouble<T>(run.FontSize), false);
                    currentFaceId = run.FaceId;
                    currentFontSize = run.FontSize;
                }

                if (currentCharSpacing != 0d)
                {
                    writer.CharSpacing(T.Zero);
                    currentCharSpacing = 0d;
                }

                if (currentWordSpacing != 0d)
                {
                    writer.WordSpacing(T.Zero);
                    currentWordSpacing = 0d;
                }

                var foreground = run.ForegroundColor ?? new TextColor(0, 0, 0);
                if (currentFillColor != foreground)
                {
                    writer.SetFillRGB(foreground.R, foreground.G, foreground.B);
                    currentFillColor = foreground;
                }

                var x = FromDouble<T>(ToDouble(area.LLx) + line.X + run.X);
                var y = FromDouble<T>(ToDouble(area.URy) - run.BaselineY);
                writer.TextMove(x, y);

                var pdfGlyphs = writableFont.GetGlyphs(run.Text)
                    .Where(x => x.Glyph != null)
                    .ToList();
                var hbCumulativeAdv = BuildExpectedCharacterAdvances(run, pdfGlyphs);

                var arr = new List<GlyphOrShift<T>>(pdfGlyphs.Count * 2);
                double currentPdfPenX = 0d;
                int textIdx = 0;

                foreach (var pdfGlyphOrShift in pdfGlyphs)
                {
                    var glyph = pdfGlyphOrShift.Glyph!;
                    var pdfAdv = glyph.w0 * run.FontSize;
                    
                    var nextPdfPenX = currentPdfPenX + pdfAdv;
                    textIdx += glyph.MultiChar?.Length ?? 1;
                    var expectedHbX = hbCumulativeAdv[Math.Min(textIdx, hbCumulativeAdv.Length - 1)];

                    var delta = expectedHbX - nextPdfPenX;
                    
                    arr.Add(new GlyphOrShift<T>(glyph, T.Zero, pdfGlyphOrShift.Bytes));
                    
                    if (Math.Abs(delta) > 0.001)
                    {
                        var shiftTj = -(delta * 1000.0) / run.FontSize;
                        arr.Add(new GlyphOrShift<T>(null, FromDouble<T>(shiftTj), 0));
                        currentPdfPenX = expectedHbX;
                    }
                    else
                    {
                        currentPdfPenX = nextPdfPenX;
                    }
                }

                writer.WriteGlyphs(arr);

                if (run.Underline && run.MeasuredWidth > 0)
                {
                    var decorationWidth = GetVisibleDecorationWidth(line, run);
                    if (decorationWidth <= 0d)
                    {
                        continue;
                    }

                    var underlineYOffset = run.FontSize * 0.12d;
                    var underlineY = ToDouble(area.URy) - run.BaselineY - underlineYOffset;
                    var startX = ToDouble(area.LLx) + line.X + run.X;
                    underlines.Add((startX, underlineY, startX + decorationWidth, GetUnderlineThickness(run), foreground));
                }
            }
        }

        writer.EndText();

        foreach (var underline in underlines)
        {
            writer.Save();
            writer.SetStrokingRGB(underline.Color.R, underline.Color.G, underline.Color.B);
            writer.LineWidth(FromDouble<T>(underline.Thickness));
            writer.MoveTo(FromDouble<T>(underline.X1), FromDouble<T>(underline.Y));
            writer.LineTo(FromDouble<T>(underline.X2), FromDouble<T>(underline.Y));
            writer.Stroke();
            writer.Restore();
        }

        foreach (var bullet in vectorBullets)
        {
            writer.Save();
            writer.SetFillRGB(bullet.Color.R, bullet.Color.G, bullet.Color.B);
            writer.Circle(
                FromDouble<T>(bullet.CenterX),
                FromDouble<T>(bullet.CenterY),
                FromDouble<T>(bullet.Radius));
            writer.Fill(evenOdd: false);
            writer.Restore();
        }

        return layout;
    }

    private static (double Top, double Height) GetInlineHighlightMetrics(TextLayoutRun run)
    {
        var height = Math.Min(run.LineHeight, Math.Max(run.FontSize * 1.07d, run.FontSize + 1.5d));
        var top = run.BaselineY - (run.FontSize * 0.87d);
        return (top, height);
    }

    private static double GetUnderlineThickness(TextLayoutRun run)
        => Math.Max(0.75d, run.FontSize * 0.075d);

    private static double[] BuildExpectedCharacterAdvances(TextLayoutRun run, IReadOnlyList<GlyphOrShift> pdfGlyphs)
    {
        var cumulative = new double[run.Text.Length + 1];
        if (run.Text.Length == 0)
        {
            return cumulative;
        }

        var pdfCharAdvances = new double[run.Text.Length];
        var textIndex = 0;
        foreach (var glyphOrShift in pdfGlyphs)
        {
            var glyph = glyphOrShift.Glyph;
            if (glyph == null || textIndex >= run.Text.Length)
            {
                continue;
            }

            var charCount = Math.Max(1, glyph.MultiChar?.Length ?? 1);
            var charsRemaining = run.Text.Length - textIndex;
            charCount = Math.Min(charCount, charsRemaining);

            var width = glyph.w0 * run.FontSize;
            if (charCount == 1)
            {
                pdfCharAdvances[textIndex++] = width;
                continue;
            }

            var perCharWidth = width / charCount;
            for (var i = 0; i < charCount; i++)
            {
                pdfCharAdvances[textIndex++] = perCharWidth;
            }
        }

        if (run.Glyphs.Count == 0)
        {
            for (var i = 0; i < pdfCharAdvances.Length; i++)
            {
                cumulative[i + 1] = cumulative[i] + pdfCharAdvances[i];
            }

            return cumulative;
        }

        var clusterBase = run.Glyphs.Min(x => x.Cluster);
        var clusterAdvances = new Dictionary<int, double>();
        foreach (var hbGlyph in run.Glyphs)
        {
            var localCluster = hbGlyph.Cluster >= clusterBase ? hbGlyph.Cluster - clusterBase : 0u;
            var clusterIndex = (int)Math.Min(localCluster, (uint)(run.Text.Length - 1));
            clusterAdvances.TryGetValue(clusterIndex, out var current);
            clusterAdvances[clusterIndex] = current + hbGlyph.Advance;
        }

        var clusterStarts = clusterAdvances.Keys.OrderBy(x => x).ToArray();
        for (var i = 0; i < clusterStarts.Length; i++)
        {
            var start = clusterStarts[i];
            var end = i + 1 < clusterStarts.Length ? clusterStarts[i + 1] : run.Text.Length;
            end = Math.Max(start + 1, Math.Min(end, run.Text.Length));

            var clusterAdvance = clusterAdvances[start];
            var clusterPdfAdvance = 0d;
            for (var charIndex = start; charIndex < end; charIndex++)
            {
                clusterPdfAdvance += pdfCharAdvances[charIndex];
            }

            if (clusterPdfAdvance <= 0d)
            {
                var perCharAdvance = clusterAdvance / (end - start);
                for (var charIndex = start; charIndex < end; charIndex++)
                {
                    cumulative[charIndex + 1] = cumulative[charIndex] + perCharAdvance;
                }

                continue;
            }

            for (var charIndex = start; charIndex < end; charIndex++)
            {
                var share = pdfCharAdvances[charIndex] / clusterPdfAdvance;
                cumulative[charIndex + 1] = cumulative[charIndex] + (clusterAdvance * share);
            }
        }

        for (var i = 1; i < cumulative.Length; i++)
        {
            if (cumulative[i] < cumulative[i - 1])
            {
                cumulative[i] = cumulative[i - 1];
            }
        }

        return cumulative;
    }

    private static double GetVisibleDecorationWidth(TextLayoutLine line, TextLayoutRun run)
    {
        var remainingVisibleWidth = line.MeasuredWidth - run.X;
        if (remainingVisibleWidth <= 0d)
        {
            return 0d;
        }

        return Math.Max(0d, Math.Min(run.MeasuredWidth, remainingVisibleWidth));
    }

    private static void DrawLayoutDecorations<T>(ContentWriter<T> writer, PdfRect<T> area, IReadOnlyList<TextLayoutDecoration> decorations)
        where T : struct, IFloatingPoint<T>
    {
        foreach (var decoration in decorations)
        {
            switch (decoration)
            {
                case TextLayoutFillRectDecoration fill:
                    writer.Save();
                    writer.SetFillRGB(fill.Color.R, fill.Color.G, fill.Color.B);
                    AddDecorationRect(writer, area, fill.X, fill.Y, fill.Width, fill.Height, fill.Radius);
                    writer.Fill(evenOdd: false);
                    writer.Restore();
                    break;
                case TextLayoutStrokeRectDecoration stroke:
                    writer.Save();
                    writer.SetStrokingRGB(stroke.Color.R, stroke.Color.G, stroke.Color.B);
                    writer.LineWidth(FromDouble<T>(stroke.Thickness));
                    AddDecorationRect(writer, area, stroke.X, stroke.Y, stroke.Width, stroke.Height, stroke.Radius);
                    writer.Stroke();
                    writer.Restore();
                    break;
                case TextLayoutLineDecoration line:
                    writer.Save();
                    writer.SetStrokingRGB(line.Color.R, line.Color.G, line.Color.B);
                    writer.LineWidth(FromDouble<T>(line.Thickness));
                    writer.MoveTo(
                        FromDouble<T>(ToDouble(area.LLx) + line.X1),
                        FromDouble<T>(ToDouble(area.URy) - line.Y1));
                    writer.LineTo(
                        FromDouble<T>(ToDouble(area.LLx) + line.X2),
                        FromDouble<T>(ToDouble(area.URy) - line.Y2));
                    writer.Stroke();
                    writer.Restore();
                    break;
            }
        }
    }

    private static void DrawBoxStyle<T>(ContentWriter<T> writer, PdfRect<T> area, TextBoxStyle style)
        where T : struct, IFloatingPoint<T>
    {
        var borderWidth = Math.Max(0d, style.BorderWidth);
        var radius = Math.Max(0d, style.BorderRadius);

        if (style.BackgroundColor is TextColor background)
        {
            writer.Save();
            writer.SetFillRGB(background.R, background.G, background.B);
            AddBoxPath(writer, area, radius);
            writer.Fill(evenOdd: false);
            writer.Restore();
        }

        if (borderWidth > 0d && style.BorderColor is TextColor border)
        {
            var inset = borderWidth / 2d;
            var strokeRect = new PdfRect<T>(
                area.LLx + FromDouble<T>(inset),
                area.LLy + FromDouble<T>(inset),
                area.URx - FromDouble<T>(inset),
                area.URy - FromDouble<T>(inset));
            var strokeRadius = Math.Max(0d, radius - inset);

            writer.Save();
            writer.SetStrokingRGB(border.R, border.G, border.B);
            writer.LineWidth(FromDouble<T>(borderWidth));
            writer.LineJoin(JoinStyle.ROUND);
            AddBoxPath(writer, strokeRect, strokeRadius);
            writer.Stroke();
            writer.Restore();
        }
    }

    private static void AddBoxPath<T>(ContentWriter<T> writer, PdfRect<T> area, double radius)
        where T : struct, IFloatingPoint<T>
    {
        if (radius > 0d)
        {
            writer.RoundedRect(area, FromDouble<T>(radius));
            return;
        }

        writer.Rect(area);
    }

    private static void AddDecorationRect<T>(ContentWriter<T> writer, PdfRect<T> area, double x, double top, double width, double height, double radius)
        where T : struct, IFloatingPoint<T>
    {
        var rect = new PdfRect<T>(
            FromDouble<T>(ToDouble(area.LLx) + x),
            FromDouble<T>(ToDouble(area.URy) - top - height),
            FromDouble<T>(ToDouble(area.LLx) + x + width),
            FromDouble<T>(ToDouble(area.URy) - top));
        AddBoxPath(writer, rect, radius);
    }

    private static double ToDouble<T>(T value)
        where T : struct, IFloatingPoint<T>
        => double.CreateChecked(value);

    private static TNumber FromDouble<TNumber>(double value)
        where TNumber : struct, IFloatingPoint<TNumber>
        => TNumber.CreateChecked(value);
}
