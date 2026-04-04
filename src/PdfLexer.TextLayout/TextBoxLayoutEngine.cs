using HarfRust;

namespace PdfLexer.TextLayout;

public sealed class TextBoxLayoutEngine
{
    public TextBoxLayoutResult Layout(TextBoxLayoutRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.FontLibrary);
        ArgumentNullException.ThrowIfNull(request.Segments);

        if (request.Width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(request), "Width must be greater than zero.");
        }

        if (request.Height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(request), "Height must be greater than zero.");
        }

        var contentWidth = GetContentWidth(request.Width, request.BoxStyle);
        var contentHeight = GetContentHeight(request.Height, request.BoxStyle);
        if (contentWidth <= 0 || contentHeight <= 0)
        {
            return CreateNoContentAreaResult(request);
        }

        var issues = new List<TextLayoutIssue>();
        using var fonts = new FontCache(request.FontLibrary);

        var preparedTokens = PrepareTokens(request, fonts, issues);
        if (issues.Any(x => x.Kind is TextLayoutIssueKind.MissingFamily or TextLayoutIssueKind.MissingWeight or TextLayoutIssueKind.MissingGlyph)
            && request.MissingFontBehavior == TextResolutionBehavior.Error)
        {
            return CreateResult(request, Array.Empty<PreparedLine>(), issues, TextLayoutStatus.Error, 0, 0);
        }

        var lines = BuildLines(request, preparedTokens, issues, contentWidth);
        var totalMeasuredHeight = lines.Sum(x => x.Height);
        var fitsHeight = totalMeasuredHeight <= contentHeight + 0.0001d;
        var fitsWidth = lines.All(x => x.MeasuredWidth <= contentWidth + 0.0001d);

        var status = issues.Any(x => x.Kind is TextLayoutIssueKind.MissingFamily or TextLayoutIssueKind.MissingWeight or TextLayoutIssueKind.MissingGlyph)
            ? TextLayoutStatus.Error
            : (!fitsWidth || !fitsHeight ? TextLayoutStatus.Overflow : TextLayoutStatus.Success);

        if (status == TextLayoutStatus.Overflow && request.OverflowMode == TextOverflowMode.Error)
        {
            issues.Add(new TextLayoutIssue(TextLayoutIssueKind.Overflow, "Text content exceeds the target text box."));
            status = TextLayoutStatus.Error;
        }

        var renderedLines = request.OverflowMode == TextOverflowMode.Clip
            ? ClipLines(contentHeight, lines)
            : lines;

        if ((!fitsWidth || !fitsHeight) && status != TextLayoutStatus.Error)
        {
            issues.Add(new TextLayoutIssue(TextLayoutIssueKind.Overflow, "Text content exceeds the target text box."));
        }

        PositionLines(request, renderedLines, contentWidth, contentHeight);
        var measuredWidth = lines.Count == 0 ? 0d : lines.Max(x => x.MeasuredWidth);
        return CreateResult(request, renderedLines, issues, status, measuredWidth, totalMeasuredHeight);
    }

    public TextBoxFitResult Fit(TextBoxLayoutRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.FontLibrary);
        ArgumentNullException.ThrowIfNull(request.Segments);

        if (request.Width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(request), "Width must be greater than zero.");
        }

        if (request.Height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(request), "Height must be greater than zero.");
        }

        var contentWidth = GetContentWidth(request.Width, request.BoxStyle);
        var contentHeight = GetContentHeight(request.Height, request.BoxStyle);
        if (contentWidth <= 0 || contentHeight <= 0)
        {
            return new TextBoxFitResult(CreateNoContentAreaResult(request), request, 0d, 0d, TextBreakKind.None, true);
        }

        var issues = new List<TextLayoutIssue>();
        using var fonts = new FontCache(request.FontLibrary);

        var preparedTokens = PrepareTokens(request, fonts, issues);
        var allLines = BuildLines(request, preparedTokens, issues, contentWidth);
        var fittingLines = ClipLines(contentHeight, allLines);
        PositionLines(request, fittingLines, contentWidth, contentHeight);

        var fittingMeasuredWidth = fittingLines.Count == 0 ? 0d : fittingLines.Max(x => x.MeasuredWidth);
        var fittingMeasuredHeight = fittingLines.Sum(x => x.Height);
        var fittingLayout = CreateResult(
            request,
            fittingLines,
            issues,
            issues.Any(x => x.Kind is TextLayoutIssueKind.MissingFamily or TextLayoutIssueKind.MissingWeight or TextLayoutIssueKind.MissingGlyph)
                ? TextLayoutStatus.Error
                : TextLayoutStatus.Success,
            fittingMeasuredWidth,
            fittingMeasuredHeight);

        var fittingTokenCount = fittingLines.Sum(x => x.Tokens.Count);
        var hasRemainder = fittingTokenCount < preparedTokens.Count;
        if (!hasRemainder)
        {
            return new TextBoxFitResult(
                fittingLayout,
                null,
                fittingMeasuredHeight,
                fittingMeasuredWidth,
                TextBreakKind.None,
                false);
        }

        var remainderRequest = BuildRemainderRequest(request, preparedTokens, fittingTokenCount);
        var breakKind = fittingLines.Count == 0 ? TextBreakKind.Line : TextBreakKind.Line;
        return new TextBoxFitResult(
            fittingLayout,
            remainderRequest,
            fittingMeasuredHeight,
            fittingMeasuredWidth,
            breakKind,
            true);
    }

    private static List<PreparedToken> PrepareTokens(TextBoxLayoutRequest request, FontCache fonts, List<TextLayoutIssue> issues)
    {
        var tokens = new List<PreparedToken>();
        for (var segmentIndex = 0; segmentIndex < request.Segments.Count; segmentIndex++)
        {
            var segment = request.Segments[segmentIndex];
            var normalizedText = segment.Text.Replace("\r", string.Empty, StringComparison.Ordinal);
            var tokenSpans = Tokenize(normalizedText);
            for (var tokenIndex = 0; tokenIndex < tokenSpans.Count;)
            {
                var token = tokenSpans[tokenIndex];
                if (token.Kind == PreparedTokenKind.NewLine)
                {
                    TryResolveFace(request, segment.Style, fonts, out var face, out _);
                    var metrics = ResolveTokenMetrics(request.MetricPreference, segment.Style, face);
                    tokens.Add(new PreparedToken(segmentIndex, token.GetText(normalizedText), segment.Style, token.Kind, null, Array.Empty<TextLayoutGlyph>(), 0, 0, metrics));
                    tokenIndex++;
                    continue;
                }

                var chunkStart = token.Start;
                var chunkEnd = token.End;
                var chunkEndIndex = tokenIndex + 1;
                while (chunkEndIndex < tokenSpans.Count && tokenSpans[chunkEndIndex].Kind != PreparedTokenKind.NewLine)
                {
                    chunkEnd = tokenSpans[chunkEndIndex].End;
                    chunkEndIndex++;
                }

                AppendShapedChunkTokens(request, fonts, issues, tokens, segmentIndex, segment.Style, normalizedText, tokenSpans, tokenIndex, chunkEndIndex, chunkStart, chunkEnd - chunkStart);
                tokenIndex = chunkEndIndex;
            }
        }

        return tokens;
    }

    private static void AppendShapedChunkTokens(
        TextBoxLayoutRequest request,
        FontCache fonts,
        List<TextLayoutIssue> issues,
        List<PreparedToken> tokens,
        int segmentIndex,
        TextSegmentStyle style,
        string sourceText,
        IReadOnlyList<TokenSlice> tokenSpans,
        int startIndex,
        int endIndex,
        int chunkStart,
        int chunkLength)
    {
        if (!TryResolveFace(request, style, fonts, out var face, out var faceError))
        {
            issues.Add(faceError!);
            var missingMetrics = ResolveTokenMetrics(request.MetricPreference, style, null);
            for (var i = startIndex; i < endIndex; i++)
            {
                var token = tokenSpans[i];
                tokens.Add(new PreparedToken(segmentIndex, token.GetText(sourceText), style, token.Kind, null, Array.Empty<TextLayoutGlyph>(), 0, 0, missingMetrics));
            }

            return;
        }

        var chunkText = sourceText.AsSpan(chunkStart, chunkLength);
        var shapingText = NormalizeShapingWhitespace(chunkText);
        if (!TryShapeWithFallback(request, fonts, style, shapingText, face!, out face, out var glyphs, out var missingGlyph))
        {
            issues.Add(new TextLayoutIssue(TextLayoutIssueKind.MissingGlyph, missingGlyph ?? $"Unable to shape segment '{new string(chunkText)}'.", segmentIndex, style.FamilyName, style.Weight, face?.FaceId));
            var fallbackMetrics = ResolveTokenMetrics(request.MetricPreference, style, face);
            for (var i = startIndex; i < endIndex; i++)
            {
                var token = tokenSpans[i];
                tokens.Add(new PreparedToken(segmentIndex, token.GetText(sourceText), style, token.Kind, face, Array.Empty<TextLayoutGlyph>(), 0, 0, fallbackMetrics));
            }

            return;
        }

        var metrics = ResolveTokenMetrics(request.MetricPreference, style, face);
        for (var i = startIndex; i < endIndex; i++)
        {
            var token = tokenSpans[i];
            var relativeStart = token.Start - chunkStart;
            var relativeEnd = token.End - chunkStart;
            var tokenGlyphs = SliceGlyphs(glyphs, relativeStart, relativeEnd, style.CharacterSpacing);
            var width = 0d;
            for (var glyphIndex = 0; glyphIndex < tokenGlyphs.Count; glyphIndex++)
            {
                width += tokenGlyphs[glyphIndex].Advance;
            }

            tokens.Add(new PreparedToken(segmentIndex, token.GetText(sourceText), style, token.Kind, face, tokenGlyphs, width, width, metrics));
        }
    }

    private static bool TryResolveFace(
        TextBoxLayoutRequest request,
        TextSegmentStyle style,
        FontCache fonts,
        out TextFontFace? face,
        out TextLayoutIssue? issue)
    {
        issue = null;
        if (request.FontLibrary.TryResolve(style.FamilyName, style.Weight, style.Italic, out face))
        {
            return true;
        }

        if (!request.FontLibrary.HasFamily(style.FamilyName))
        {
            issue = new TextLayoutIssue(TextLayoutIssueKind.MissingFamily, $"Font family '{style.FamilyName}' was not found.", null, style.FamilyName, style.Weight);
            if (request.MissingFontBehavior == TextResolutionBehavior.UseFallbackFamilies)
            {
                foreach (var fallback in request.FallbackFamilyNames)
                {
                    if (request.FontLibrary.TryResolve(fallback, style.Weight, style.Italic, out face))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        issue = new TextLayoutIssue(TextLayoutIssueKind.MissingWeight, $"Font weight '{style.Weight}' was not found for family '{style.FamilyName}'.", null, style.FamilyName, style.Weight);
        if (request.MissingFontBehavior == TextResolutionBehavior.UseFallbackFamilies)
        {
            foreach (var fallback in request.FallbackFamilyNames)
            {
                if (request.FontLibrary.TryResolve(fallback, style.Weight, style.Italic, out face))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool TryShapeWithFallback(
        TextBoxLayoutRequest request,
        FontCache fonts,
        TextSegmentStyle style,
        ReadOnlySpan<char> text,
        TextFontFace initialFace,
        out TextFontFace? resolvedFace,
        out IReadOnlyList<TextLayoutGlyph> glyphs,
        out string? issue)
    {
        issue = null;
        glyphs = Array.Empty<TextLayoutGlyph>();
        resolvedFace = null;

        var candidates = new List<TextFontFace>();
        candidates.Add(initialFace);

        if (request.MissingGlyphBehavior == TextResolutionBehavior.UseFallbackFamilies)
        {
            foreach (var fallback in request.FallbackFamilyNames)
            {
                if (request.FontLibrary.TryResolve(fallback, style.Weight, style.Italic, out var face))
                {
                    candidates.Add(face!);
                }
            }
        }

        foreach (var candidate in candidates)
        {
            var shaped = Shape(fonts.Get(candidate), fonts.Session, candidate, text, style);
            if (shaped.All(x => x.GlyphId != 0))
            {
                resolvedFace = candidate;
                glyphs = shaped;
                return true;
            }

            if (request.MissingGlyphBehavior == TextResolutionBehavior.Error)
            {
                issue = $"Font '{candidate.FamilyName}' weight '{candidate.Weight}' could not shape one or more glyphs.";
                resolvedFace = candidate;
                glyphs = shaped;
                return false;
            }
        }

        if (candidates.Count > 0)
        {
            resolvedFace = candidates[0];
            glyphs = Shape(fonts.Get(candidates[0]), fonts.Session, candidates[0], text, style);
            issue = $"No resolved font face could shape one or more glyphs for family '{style.FamilyName}' weight '{style.Weight}'.";
            return false;
        }

        issue = $"No font face was available for family '{style.FamilyName}' weight '{style.Weight}'.";
        return false;
    }

    private static IReadOnlyList<TextLayoutGlyph> Shape(HarfRustFont font, HarfRustShapeSession session, TextFontFace face, ReadOnlySpan<char> text, TextSegmentStyle style)
    {
        if (text.IsEmpty)
        {
            return Array.Empty<TextLayoutGlyph>();
        }

        using var result = session.Shape(font, text, guessSegmentProperties: true);

        var scale = style.FontSize / font.UnitsPerEm;
        var glyphs = new List<TextLayoutGlyph>(result.Length);
        var infos = result.GlyphInfos;
        var positions = result.GlyphPositions;
        double penX = 0;

        for (var i = 0; i < result.Length; i++)
        {
            var info = infos[i];
            var position = positions[i];
            var offsetX = position.XOffset * scale;
            var offsetY = position.YOffset * scale;
            var advance = position.XAdvance * scale;
            advance += GetAdditionalAdvance(text, info.Cluster, i, result.Length, style);

            glyphs.Add(new TextLayoutGlyph(
                info.GlyphId,
                info.Cluster,
                penX + offsetX,
                offsetY,
                advance,
                offsetX,
                offsetY));

            penX += advance;
        }

        return glyphs;
    }

    private static IReadOnlyList<TextLayoutGlyph> SliceGlyphs(IReadOnlyList<TextLayoutGlyph> glyphs, int start, int end, double characterSpacing)
    {
        if (glyphs.Count == 0 || end <= start)
        {
            return Array.Empty<TextLayoutGlyph>();
        }

        var firstIndex = -1;
        var lastIndex = -1;
        for (var i = 0; i < glyphs.Count; i++)
        {
            var cluster = (int)glyphs[i].Cluster;
            if (cluster < start || cluster >= end)
            {
                continue;
            }

            firstIndex = firstIndex == -1 ? i : firstIndex;
            lastIndex = i;
        }

        if (firstIndex == -1)
        {
            return Array.Empty<TextLayoutGlyph>();
        }

        var offsetX = glyphs[firstIndex].X;
        var tokenGlyphs = new TextLayoutGlyph[lastIndex - firstIndex + 1];
        for (int sourceIndex = firstIndex, targetIndex = 0; sourceIndex <= lastIndex; sourceIndex++, targetIndex++)
        {
            var glyph = glyphs[sourceIndex];
            var advance = glyph.Advance;
            if (sourceIndex == lastIndex && sourceIndex + 1 < glyphs.Count)
            {
                advance -= characterSpacing;
            }

            tokenGlyphs[targetIndex] = glyph with
            {
                X = glyph.X - offsetX,
                Advance = advance
            };
        }

        return tokenGlyphs;
    }

    private static double GetAdditionalAdvance(ReadOnlySpan<char> text, uint cluster, int glyphIndex, int glyphCount, TextSegmentStyle style)
    {
        var additional = glyphIndex + 1 < glyphCount ? style.CharacterSpacing : 0;
        if (cluster < text.Length && text[(int)cluster] == ' ')
        {
            additional += style.WordSpacing;
        }

        return additional;
    }

    private static char[] NormalizeShapingWhitespace(ReadOnlySpan<char> text)
    {
        var chars = text.ToArray();
        for (var i = 0; i < chars.Length; i++)
        {
            if (chars[i] != '\n' && char.IsWhiteSpace(chars[i]) && chars[i] != ' ')
            {
                chars[i] = ' ';
            }
        }

        return chars;
    }

    private static List<PreparedLine> BuildLines(TextBoxLayoutRequest request, List<PreparedToken> tokens, List<TextLayoutIssue> issues, double contentWidth)
    {
        var lines = new List<PreparedLine>();
        var current = new PreparedLine();
        var paragraphStrut = ResolveParagraphStrut(tokens);
        var lineFallbackStrut = paragraphStrut;

        foreach (var token in tokens)
        {
            if (token.Kind == PreparedTokenKind.NewLine)
            {
                FinalizeLine(request, lines, current, lineFallbackStrut, paragraphStrut);
                current = new PreparedLine();
                continue;
            }

            var wouldOverflow = current.Tokens.Count > 0 && current.Width + token.Width > contentWidth;
            if (wouldOverflow)
            {
                FinalizeLine(request, lines, current, lineFallbackStrut, paragraphStrut);
                current = new PreparedLine();

                // A wrapping whitespace token belongs to the end of the previous line, not the
                // beginning of the next one. Browsers do not render it as a visible leading indent.
                if (token.Kind == PreparedTokenKind.Whitespace)
                {
                    continue;
                }
            }

            current.Tokens.Add(token);
            current.Width += token.Width;
            ApplyTokenMetrics(current, token);
            if (token.Kind == PreparedTokenKind.Text)
            {
                lineFallbackStrut = token.Metrics;
            }
        }

        FinalizeLine(request, lines, current, lineFallbackStrut, paragraphStrut);
        return lines;
    }

    private static void FinalizeLine(TextBoxLayoutRequest request, List<PreparedLine> lines, PreparedLine current, TokenMetrics fallbackStrut, TokenMetrics paragraphStrut)
    {
        if (current.Tokens.Count == 0
            && lines.Count == 0
            && fallbackStrut.ExplicitLineHeight is null
            && fallbackStrut.Ascent == 0
            && fallbackStrut.Descent == 0
            && fallbackStrut.LineGap == 0)
        {
            return;
        }

        if (current.Tokens.Count == 0)
        {
            current.Ascent = fallbackStrut.Ascent;
            current.Descent = fallbackStrut.Descent;
            current.LineGap = fallbackStrut.LineGap;
            current.ExplicitLineHeight = fallbackStrut.ExplicitLineHeight;
        }

        ResolveLineBox(current, paragraphStrut);

        if (current.Tokens.Count == 0)
        {
            current.MeasuredWidth = 0;
            lines.Add(current);
            return;
        }

        var trailingWhitespace = 0d;
        if (!request.PreserveTrailingWhitespaceInWidth)
        {
            for (var i = current.Tokens.Count - 1; i >= 0; i--)
            {
                if (current.Tokens[i].Kind != PreparedTokenKind.Whitespace)
                {
                    break;
                }

                trailingWhitespace += current.Tokens[i].Width;
            }
        }

        current.MeasuredWidth = Math.Max(0, current.Width - trailingWhitespace);
        lines.Add(current);
    }

    private static List<PreparedLine> ClipLines(double contentHeight, List<PreparedLine> lines)
    {
        var clipped = new List<PreparedLine>();
        var height = 0d;
        foreach (var line in lines)
        {
            var next = height + line.Height;
            if (clipped.Count > 0 && next > contentHeight + 0.0001d)
            {
                break;
            }

            clipped.Add(line);
            height = next;
        }

        return clipped;
    }

    private static void PositionLines(TextBoxLayoutRequest request, List<PreparedLine> lines, double contentWidth, double contentHeight)
    {
        var renderedHeight = lines.Sum(x => x.Height);
        var inset = request.BoxStyle.Inset;
        var topOffset = request.VerticalAlignment switch
        {
            TextVerticalAlignment.Center => (contentHeight - renderedHeight) / 2d,
            TextVerticalAlignment.Bottom => contentHeight - renderedHeight,
            _ => 0d
        };

        double y = inset + topOffset;
        foreach (var line in lines)
        {
            line.X = request.HorizontalAlignment switch
            {
                TextHorizontalAlignment.Center when line.MeasuredWidth < contentWidth => inset + ((contentWidth - line.MeasuredWidth) / 2d),
                TextHorizontalAlignment.Right when line.MeasuredWidth < contentWidth => inset + contentWidth - line.MeasuredWidth,
                _ => inset
            };

            line.BaselineY = y + line.BaselineOffset;
            double x = 0d;
            foreach (var token in line.Tokens)
            {
                token.X = x;
                token.BaselineY = line.BaselineY;
                x += token.Width;
            }

            y += line.Height;
        }
    }

    private static TextBoxLayoutResult CreateResult(
        TextBoxLayoutRequest request,
        IReadOnlyList<PreparedLine> renderedLines,
        IReadOnlyList<TextLayoutIssue> issues,
        TextLayoutStatus status,
        double measuredWidth,
        double measuredHeight)
    {
        var inset = request.BoxStyle.Inset;
        var lines = new TextLayoutLine[renderedLines.Count];
        double renderedContentWidth = 0d;
        double renderedContentHeight = 0d;
        for (var i = 0; i < renderedLines.Count; i++)
        {
            var line = renderedLines[i];
            var runs = new TextLayoutRun[line.Tokens.Count];
            for (var j = 0; j < line.Tokens.Count; j++)
            {
                var token = line.Tokens[j];
                runs[j] = new TextLayoutRun(
                    token.SegmentIndex,
                    token.Text,
                    token.Face?.FaceId ?? string.Empty,
                    token.Style.FamilyName,
                    token.Style.Weight,
                    token.Style.FontSize,
                    token.Style.Italic,
                    token.Style.Underline,
                    token.Style.CharacterSpacing,
                    token.Style.WordSpacing,
                    token.Style.ForegroundColor,
                    token.Style.BackgroundColor,
                    token.X,
                    token.BaselineY,
                    token.Width,
                    token.MeasuredWidth,
                    line.Height,
                    token.Glyphs,
                    false);
            }

            lines[i] = new TextLayoutLine(
                i,
                line.X,
                line.BaselineY,
                line.Width,
                line.MeasuredWidth,
                line.Height,
                line.BaselineOffset,
                runs);

            if (line.MeasuredWidth > renderedContentWidth)
            {
                renderedContentWidth = line.MeasuredWidth;
            }

            renderedContentHeight += line.Height;
        }
        return new TextBoxLayoutResult
        {
            Status = status,
            FitsWidth = measuredWidth <= GetContentWidth(request.Width, request.BoxStyle) + 0.0001d,
            FitsHeight = measuredHeight <= GetContentHeight(request.Height, request.BoxStyle) + 0.0001d,
            MeasuredWidth = measuredWidth + (inset * 2d),
            MeasuredHeight = measuredHeight + (inset * 2d),
            RenderedWidth = renderedContentWidth + (inset * 2d),
            RenderedHeight = renderedContentHeight + (inset * 2d),
            BoxStyle = request.BoxStyle,
            Lines = lines,
            Issues = issues.ToArray(),
            Decorations = Array.Empty<TextLayoutDecoration>()
        };
    }

    private static TextBoxLayoutResult CreateNoContentAreaResult(TextBoxLayoutRequest request)
    {
        var issues = new[]
        {
            new TextLayoutIssue(TextLayoutIssueKind.Overflow, "Text box border and padding leave no available content area.")
        };

        return new TextBoxLayoutResult
        {
            Status = request.OverflowMode == TextOverflowMode.Error ? TextLayoutStatus.Error : TextLayoutStatus.Overflow,
            FitsWidth = false,
            FitsHeight = false,
            MeasuredWidth = request.Width,
            MeasuredHeight = request.Height,
            RenderedWidth = 0d,
            RenderedHeight = 0d,
            BoxStyle = request.BoxStyle,
            Lines = Array.Empty<TextLayoutLine>(),
            Issues = issues,
            Decorations = Array.Empty<TextLayoutDecoration>()
        };
    }

    private static TextBoxLayoutRequest BuildRemainderRequest(TextBoxLayoutRequest request, IReadOnlyList<PreparedToken> preparedTokens, int consumedTokenCount)
    {
        if (consumedTokenCount <= 0)
        {
            return request;
        }

        var consumedBySegment = new int[request.Segments.Count];
        for (var i = 0; i < consumedTokenCount; i++)
        {
            var token = preparedTokens[i];
            consumedBySegment[token.SegmentIndex] += token.Text.Length;
        }

        var segments = new List<TextSegment>();
        for (var i = 0; i < request.Segments.Count; i++)
        {
            var original = request.Segments[i];
            var consumed = Math.Min(consumedBySegment[i], original.Text.Length);
            if (consumed >= original.Text.Length)
            {
                continue;
            }

            segments.Add(original with { Text = original.Text[consumed..] });
        }

        return request with { Segments = segments };
    }

    private static double GetContentWidth(double width, TextBoxStyle style)
        => width - (style.Inset * 2d);

    private static double GetContentHeight(double height, TextBoxStyle style)
        => height - (style.Inset * 2d);

    private static List<TokenSlice> Tokenize(string text)
    {
        var tokens = new List<TokenSlice>();
        if (string.IsNullOrEmpty(text))
        {
            return tokens;
        }

        var start = -1;
        PreparedTokenKind? kind = null;

        for (var i = 0; i < text.Length; i++)
        {
            var ch = text[i];
            if (ch == '\n')
            {
                if (start >= 0 && kind != null)
                {
                    tokens.Add(new TokenSlice(kind.Value, start, i - start));
                    start = -1;
                }

                tokens.Add(new TokenSlice(PreparedTokenKind.NewLine, i, 1));
                kind = null;
                continue;
            }

            var nextKind = char.IsWhiteSpace(ch) ? PreparedTokenKind.Whitespace : PreparedTokenKind.Text;
            if (kind != null && kind != nextKind)
            {
                tokens.Add(new TokenSlice(kind.Value, start, i - start));
                start = i;
            }
            else if (kind is null)
            {
                start = i;
            }

            kind = nextKind;
        }

        if (start >= 0 && kind != null)
        {
            tokens.Add(new TokenSlice(kind.Value, start, text.Length - start));
        }

        return tokens;
    }

    private static TokenMetrics ResolveTokenMetrics(TextFontMetricSource metricPreference, TextSegmentStyle style, TextFontFace? face)
    {
        var explicitLineHeight = style.LineSpacing;
        if (face is { HasLayoutMetrics: true })
        {
            var metrics = face.ResolveMetrics(metricPreference);
            var ascent = style.FontSize * (metrics.Ascent / 1000d);
            var descent = style.FontSize * (Math.Abs(metrics.Descent) / 1000d);
            var lineGap = style.FontSize * (Math.Max(0d, metrics.LineGap) / 1000d);
            return new TokenMetrics(explicitLineHeight, ascent, descent, lineGap);
        }

        return new TokenMetrics(explicitLineHeight, style.FontSize, 0d, 0d);
    }

    private static TokenMetrics ResolveParagraphStrut(IReadOnlyList<PreparedToken> tokens)
    {
        foreach (var token in tokens)
        {
            if (token.Kind is PreparedTokenKind.NewLine or PreparedTokenKind.Whitespace)
            {
                continue;
            }

            return token.Metrics;
        }

        return new TokenMetrics(null, 0d, 0d, 0d);
    }

    private static void ApplyTokenMetrics(PreparedLine line, PreparedToken token)
    {
        line.Ascent = Math.Max(line.Ascent, token.Metrics.Ascent);
        line.Descent = Math.Max(line.Descent, token.Metrics.Descent);
        line.LineGap = Math.Max(line.LineGap, token.Metrics.LineGap);
        if (token.Metrics.ExplicitLineHeight.HasValue)
        {
            line.ExplicitLineHeight = Math.Max(line.ExplicitLineHeight ?? 0d, token.Metrics.ExplicitLineHeight.Value);
        }
    }

    private static void ResolveLineBox(PreparedLine line, TokenMetrics paragraphStrut)
    {
        var baselineAscent = line.Ascent;
        var baselineDescent = line.Descent;
        if (line.ExplicitLineHeight.HasValue && paragraphStrut.Ascent > 0d)
        {
            baselineAscent = paragraphStrut.Ascent;
            baselineDescent = paragraphStrut.Descent;
        }

        var naturalContentHeight = line.Ascent + line.Descent;
        var naturalLineHeight = naturalContentHeight + line.LineGap;

        line.Height = line.ExplicitLineHeight ?? naturalLineHeight;
        if (line.Height <= 0)
        {
            line.Height = naturalLineHeight;
        }

        var baselineContentHeight = baselineAscent + baselineDescent;
        var topLeading = Math.Max(0d, line.Height - baselineContentHeight) / 2d;
        line.BaselineOffset = topLeading + baselineAscent;
    }

    private enum PreparedTokenKind
    {
        Text,
        Whitespace,
        NewLine
    }

    private sealed class PreparedToken
    {
        public PreparedToken(int segmentIndex, string text, TextSegmentStyle style, PreparedTokenKind kind, TextFontFace? face, IReadOnlyList<TextLayoutGlyph> glyphs, double width, double measuredWidth, TokenMetrics metrics)
        {
            SegmentIndex = segmentIndex;
            Text = text;
            Style = style;
            Kind = kind;
            Face = face;
            Glyphs = glyphs;
            Width = width;
            MeasuredWidth = measuredWidth;
            Metrics = metrics;
        }

        public int SegmentIndex { get; }
        public string Text { get; }
        public TextSegmentStyle Style { get; }
        public PreparedTokenKind Kind { get; }
        public TextFontFace? Face { get; }
        public IReadOnlyList<TextLayoutGlyph> Glyphs { get; }
        public double Width { get; }
        public double MeasuredWidth { get; }
        public TokenMetrics Metrics { get; }
        public double X { get; set; }
        public double BaselineY { get; set; }
    }

    private sealed class PreparedLine
    {
        public List<PreparedToken> Tokens { get; } = new();
        public double Width { get; set; }
        public double MeasuredWidth { get; set; }
        public double Ascent { get; set; }
        public double Descent { get; set; }
        public double LineGap { get; set; }
        public double? ExplicitLineHeight { get; set; }
        public double Height { get; set; }
        public double BaselineOffset { get; set; }
        public double X { get; set; }
        public double BaselineY { get; set; }
    }

    private readonly record struct TokenMetrics(
        double? ExplicitLineHeight,
        double Ascent,
        double Descent,
        double LineGap);

    private readonly record struct TokenSlice(PreparedTokenKind Kind, int Start, int Length)
    {
        public int End => Start + Length;

        public string GetText(string source)
            => source.Substring(Start, Length);
    }

    private sealed class FontCache : IDisposable
    {
        private readonly Dictionary<string, HarfRustFont> _fonts = new(StringComparer.Ordinal);
        private readonly HarfRustShapeSession _session;

        public FontCache(TextFontLibrary library)
        {
            Library = library;
            _session = new HarfRustShapeSession(HarfRustBackend.Current);
        }

        public TextFontLibrary Library { get; }
        public HarfRustShapeSession Session => _session;

        public HarfRustFont Get(TextFontFace face)
        {
            if (!_fonts.TryGetValue(face.FaceId, out var font))
            {
                font = new HarfRustFont(face.FontData, face.FaceIndex, HarfRustBackend.Current);
                _fonts[face.FaceId] = font;
            }

            return font;
        }

        public void Dispose()
        {
            _session.Dispose();
            foreach (var font in _fonts.Values)
            {
                font.Dispose();
            }

            _fonts.Clear();
        }
    }
}
