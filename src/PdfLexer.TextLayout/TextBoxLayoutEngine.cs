using System.Runtime.CompilerServices;
using HarfRust;

namespace PdfLexer.TextLayout;

public sealed class TextBoxLayoutEngine
{
    public TextLayoutPlan Analyze(TextBoxLayoutRequest request)
        => Analyze(request, context: null);

    public TextLayoutPlan Analyze(TextBoxLayoutRequest request, TextLayoutAnalysisContext? context)
    {
        var layout = LayoutCore(request, context);
        return new TextLayoutPlan
        {
            Kind = TextLayoutPlanKind.FlatText,
            Root = BuildFlatPlanRoot(layout, context),
            RenderPlan = new TextLayoutRenderPlan
            {
                Layout = layout
            }
        };
    }

    public TextLayoutFitPlan AnalyzeFit(TextBoxLayoutRequest request)
        => AnalyzeFit(request, context: null);

    public TextLayoutFitPlan AnalyzeFit(TextBoxLayoutRequest request, TextLayoutAnalysisContext? context)
    {
        var fullPlan = Analyze(request, context);
        var fit = Fit(request);
        var fittedRequest = BuildFittedRequest(request, fit.RemainderRequest);
        var fittedPlan = TextLayoutPlanSlicer.Slice(fullPlan, fit.FittedLayout);

        TextLayoutPlan? remainderPlan = null;
        if (fit.RemainderRequest is not null)
        {
            remainderPlan = Analyze(fit.RemainderRequest, context);
        }

        return new TextLayoutFitPlan
        {
            FittedSelection = new TextLayoutPlanSelection
            {
                Plan = fittedPlan,
                SourceReferences = CollectFlatSourceReferences(fittedPlan),
                BoundaryReferences = CollectFlatBoundaryReferences(fittedPlan),
                Continuations = BuildFlatContinuations(fittedPlan, remainderPlan, fit.BreakKind, fit.HasRemainder),
                FragmentMetadata = BuildFragmentMetadata(fit.BreakKind, fit.FragmentBreak, BuildFlatContinuations(fittedPlan, remainderPlan, fit.BreakKind, fit.HasRemainder)),
                StartLineIndex = fittedPlan.Root.StartLineIndex,
                EndLineIndexExclusive = fittedPlan.Root.EndLineIndexExclusive
            },
            RemainderSelection = remainderPlan is null
                ? null
                : new TextLayoutPlanSelection
                {
                    Plan = remainderPlan,
                    SourceReferences = CollectFlatSourceReferences(remainderPlan),
                    BoundaryReferences = CollectFlatBoundaryReferences(remainderPlan),
                    Continuations = Array.Empty<TextLayoutContinuationReference>(),
                    FragmentMetadata = new TextLayoutFragmentMetadata(TextFragmentBreak.None),
                    StartLineIndex = remainderPlan.Root.StartLineIndex,
                    EndLineIndexExclusive = remainderPlan.Root.EndLineIndexExclusive
                },
            BreakKind = fit.BreakKind,
            HasRemainder = fit.HasRemainder,
            FragmentBreak = fit.FragmentBreak,
            Materializer = new FlatFitPlanMaterializer(fittedRequest, fit.RemainderRequest)
        };
    }

    public TextLayoutFitPlan AnalyzeFragment(TextBoxLayoutRequest request)
        => AnalyzeFragment(request, context: null);

    public TextLayoutFitPlan AnalyzeFragment(TextBoxLayoutRequest request, TextLayoutAnalysisContext? context)
    {
        if (request.OverflowMode == TextOverflowMode.Fragment)
        {
            return AnalyzeFit(request, context);
        }

        var plan = Analyze(request, context);
        return new TextLayoutFitPlan
        {
            FittedSelection = new TextLayoutPlanSelection
            {
                Plan = plan,
                SourceReferences = CollectFlatSourceReferences(plan),
                BoundaryReferences = CollectFlatBoundaryReferences(plan),
                Continuations = Array.Empty<TextLayoutContinuationReference>(),
                FragmentMetadata = new TextLayoutFragmentMetadata(TextFragmentBreak.None),
                StartLineIndex = plan.Root.StartLineIndex,
                EndLineIndexExclusive = plan.Root.EndLineIndexExclusive
            },
            RemainderSelection = null,
            BreakKind = TextBreakKind.None,
            HasRemainder = false,
            FragmentBreak = TextFragmentBreak.None,
            Materializer = new FlatFitPlanMaterializer(request, null)
        };
    }

    public TextBoxFitResult Fragment(TextBoxLayoutRequest request)
    {
        if (request.OverflowMode != TextOverflowMode.Fragment)
        {
            var layout = Layout(request);
            return new TextBoxFitResult(
                layout,
                null,
                Math.Max(0d, layout.NaturalHeight - StyleResolver.Resolve(request.BoxStyle).Edges.VerticalInset),
                Math.Max(0d, layout.NaturalWidth - StyleResolver.Resolve(request.BoxStyle).Edges.HorizontalInset),
                TextBreakKind.None,
                false)
            {
                FittedRequest = request,
                FragmentBreak = TextFragmentBreak.None
            };
        }

        return Fit(request);
    }

    public TextBoxLayoutResult Layout(TextBoxLayoutRequest request)
        => LayoutCore(request, context: null);

    private TextBoxLayoutResult LayoutCore(TextBoxLayoutRequest request, TextLayoutAnalysisContext? context)
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

        var resolvedBoxStyle = StyleResolver.Resolve(request.BoxStyle);
        var contentWidth = GetContentWidth(request.Width, resolvedBoxStyle);
        var contentHeight = GetContentHeight(request.Height, resolvedBoxStyle);
        if (contentWidth <= 0 || contentHeight <= 0)
        {
            return CreateNoContentAreaResult(request);
        }

        var issues = new List<TextLayoutIssue>();
        using var fonts = new FontCache(request.FontLibrary);

        var formatting = ParagraphFormatter.Format(
            request,
            fonts,
            issues,
            new LayoutConstraints(contentWidth, contentHeight, request.OverflowMode, request.OverflowMode is TextOverflowMode.Clip or TextOverflowMode.Fragment),
            context);
        var preparedTokens = formatting.Tokens;
        if (issues.Any(x => x.Kind is TextLayoutIssueKind.MissingFamily or TextLayoutIssueKind.MissingWeight or TextLayoutIssueKind.MissingGlyph)
            && request.MissingFontBehavior == TextResolutionBehavior.Error)
        {
            return CreateResult(request, Array.Empty<PreparedLine>(), issues, TextLayoutStatus.Error, 0, 0);
        }

        var lines = formatting.AllLines;
        var totalMeasuredHeight = formatting.MeasuredHeight;
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

        var renderedLines = formatting.VisibleLines;

        if ((!fitsWidth || !fitsHeight) && status != TextLayoutStatus.Error)
        {
            issues.Add(new TextLayoutIssue(TextLayoutIssueKind.Overflow, "Text content exceeds the target text box."));
        }

        var measuredWidth = formatting.MeasuredWidth;
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

        var resolvedBoxStyle = StyleResolver.Resolve(request.BoxStyle);
        var contentWidth = GetContentWidth(request.Width, resolvedBoxStyle);
        var contentHeight = GetContentHeight(request.Height, resolvedBoxStyle);
        if (contentWidth <= 0 || contentHeight <= 0)
        {
            return new TextBoxFitResult(CreateNoContentAreaResult(request), request, 0d, 0d, TextBreakKind.None, true);
        }

        var issues = new List<TextLayoutIssue>();
        using var fonts = new FontCache(request.FontLibrary);

        var formatting = ParagraphFormatter.Format(
            request,
            fonts,
            issues,
            new LayoutConstraints(contentWidth, contentHeight, request.OverflowMode, ClipToHeight: true),
            context: null);
        var preparedTokens = formatting.Tokens;
        var fittingLines = formatting.VisibleLines;

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
                fittingLayout.NaturalHeight - resolvedBoxStyle.Edges.VerticalInset,
                fittingLayout.NaturalWidth - resolvedBoxStyle.Edges.HorizontalInset,
                TextBreakKind.None,
                false)
            {
                FittedRequest = request,
                FragmentBreak = TextFragmentBreak.None
            };
        }

        var remainderRequest = BuildRemainderRequest(request, preparedTokens, fittingTokenCount);
        var breakKind = fittingLines.Count == 0 ? TextBreakKind.Line : TextBreakKind.Line;
        return new TextBoxFitResult(
            fittingLayout,
            remainderRequest,
            fittingLayout.NaturalHeight - resolvedBoxStyle.Edges.VerticalInset,
            fittingLayout.NaturalWidth - resolvedBoxStyle.Edges.HorizontalInset,
            breakKind,
            true)
        {
            FittedRequest = BuildFittedRequest(request, remainderRequest),
            FragmentBreak = new TextFragmentBreak(TextFragmentBreakReason.Overflow, breakKind, false)
        };
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
                    tokens.Add(new PreparedToken(
                        segmentIndex,
                        GetSegmentSourceStart(segment, token.Start),
                        token.Length,
                        segment.SourcePath ?? $"Segments[{segmentIndex}]",
                        token.GetText(normalizedText),
                        segment.Style,
                        token.Kind,
                        null,
                        Array.Empty<TextLayoutGlyph>(),
                        0,
                        0,
                        metrics));
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

                AppendShapedChunkTokens(request, fonts, issues, tokens, segmentIndex, segment, normalizedText, tokenSpans, tokenIndex, chunkEndIndex, chunkStart, chunkEnd - chunkStart);
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
        TextSegment segment,
        string sourceText,
        IReadOnlyList<TokenSlice> tokenSpans,
        int startIndex,
        int endIndex,
        int chunkStart,
        int chunkLength)
    {
        var style = segment.Style;
        var sourcePath = segment.SourcePath ?? $"Segments[{segmentIndex}]";
        if (!TryResolveFace(request, style, fonts, out var face, out var faceError))
        {
            issues.Add(faceError!);
            var missingMetrics = ResolveTokenMetrics(request.MetricPreference, style, null);
            for (var i = startIndex; i < endIndex; i++)
            {
                var token = tokenSpans[i];
                tokens.Add(new PreparedToken(
                    segmentIndex,
                    GetSegmentSourceStart(segment, token.Start),
                    token.Length,
                    sourcePath,
                    token.GetText(sourceText),
                    style,
                    token.Kind,
                    null,
                    Array.Empty<TextLayoutGlyph>(),
                    0,
                    0,
                    missingMetrics));
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
                tokens.Add(new PreparedToken(
                    segmentIndex,
                    GetSegmentSourceStart(segment, token.Start),
                    token.Length,
                    sourcePath,
                    token.GetText(sourceText),
                    style,
                    token.Kind,
                    face,
                    Array.Empty<TextLayoutGlyph>(),
                    0,
                    0,
                    fallbackMetrics));
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

            tokens.Add(new PreparedToken(
                segmentIndex,
                GetSegmentSourceStart(segment, token.Start),
                token.Length,
                sourcePath,
                token.GetText(sourceText),
                style,
                token.Kind,
                face,
                tokenGlyphs,
                width,
                width,
                metrics));
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
            current.LineBoxSizing = fallbackStrut.LineBoxSizing;
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

    private static List<PreparedLine> ClipPositionedLines(double viewportTop, double viewportBottom, List<PreparedLine> lines)
    {
        var clipped = new List<PreparedLine>();
        foreach (var line in lines)
        {
            var lineTop = GetLineTop(line);
            var lineBottom = GetLineBottom(line);
            if (lineTop >= viewportTop - 0.0001d && lineBottom <= viewportBottom + 0.0001d)
            {
                clipped.Add(line);
            }
        }

        return clipped;
    }

    private static double GetLineTop(PreparedLine line)
        => line.BaselineY - line.BaselineOffset;

    private static double GetLineBottom(PreparedLine line)
        => GetLineTop(line) + line.Height;

    private static void PositionLines(TextBoxLayoutRequest request, List<PreparedLine> lines, double contentWidth, double contentHeight)
    {
        var renderedHeight = lines.Sum(x => x.Height);
        var edges = StyleResolver.Resolve(request.BoxStyle).Edges;
        var topOffset = request.VerticalAlignment switch
        {
            TextVerticalAlignment.Center => (contentHeight - renderedHeight) / 2d,
            TextVerticalAlignment.Bottom => contentHeight - renderedHeight,
            _ => 0d
        };

        double y = edges.Insets.Top + topOffset;
        foreach (var line in lines)
        {
            line.X = request.HorizontalAlignment switch
            {
                TextHorizontalAlignment.Center when line.MeasuredWidth < contentWidth => edges.Insets.Left + ((contentWidth - line.MeasuredWidth) / 2d),
                TextHorizontalAlignment.Right when line.MeasuredWidth < contentWidth => edges.Insets.Left + contentWidth - line.MeasuredWidth,
                _ => edges.Insets.Left
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
        var edges = StyleResolver.Resolve(request.BoxStyle).Edges;
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
                    false,
                    token.Style.StrikeThrough,
                    token.SourceStart,
                    token.SourceLength,
                    token.SourcePath,
                    token.Style.LineSpacing);
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
            MeasuredWidth = measuredWidth + edges.HorizontalInset,
            MeasuredHeight = measuredHeight + edges.VerticalInset,
            RenderedWidth = renderedContentWidth + edges.HorizontalInset,
            RenderedHeight = renderedContentHeight + edges.VerticalInset,
            BoxStyle = request.BoxStyle,
            Lines = lines,
            Issues = issues.ToArray(),
            Decorations = Array.Empty<TextLayoutDecoration>()
        };
    }

    private static TextLayoutPlanNode BuildFlatPlanRoot(TextBoxLayoutResult layout, TextLayoutAnalysisContext? context)
    {
        var lineNodes = new TextLayoutPlanNode[layout.Lines.Count];
        for (var lineIndex = 0; lineIndex < layout.Lines.Count; lineIndex++)
        {
            var line = layout.Lines[lineIndex];
            var runNodes = new TextLayoutPlanNode[line.Runs.Count];
            for (var runIndex = 0; runIndex < line.Runs.Count; runIndex++)
            {
                var run = line.Runs[runIndex];
                runNodes[runIndex] = new TextLayoutPlanNode
                {
                    Kind = TextLayoutNodeKind.Run,
                    Source = CreateRunSourceReference(run),
                    NaturalSize = new TextLayoutSize(run.MeasuredWidth, run.LineHeight),
                    VisibleSize = new TextLayoutSize(run.Width, run.LineHeight),
                    Children = Array.Empty<TextLayoutPlanNode>(),
                    LineDiagnostics = null
                };
            }

            var (lineContentVersion, lineStyleVersion) = GetChildVersions(runNodes);

            lineNodes[lineIndex] = new TextLayoutPlanNode
            {
                Kind = TextLayoutNodeKind.Line,
                Source = new TextLayoutSourceReference($"Lines[{lineIndex}]", NodeId: $"Lines[{lineIndex}]", ContentVersion: lineContentVersion, StyleVersion: lineStyleVersion),
                NaturalSize = new TextLayoutSize(line.MeasuredWidth, line.Height),
                VisibleSize = new TextLayoutSize(line.Width, line.Height),
                Children = runNodes,
                StartLineIndex = lineIndex,
                EndLineIndexExclusive = lineIndex + 1,
                LineDiagnostics = TextLayoutDiagnosticsBuilder.BuildLineDiagnostics(line, context)
            };
        }

        var (rootContentVersion, rootStyleVersion) = GetChildVersions(lineNodes);

        return new TextLayoutPlanNode
        {
            Kind = TextLayoutNodeKind.Root,
            Source = new TextLayoutSourceReference("Root", NodeId: "Root", ContentVersion: rootContentVersion, StyleVersion: rootStyleVersion),
            NaturalSize = layout.NaturalSize,
            VisibleSize = layout.VisibleSize,
            Children = lineNodes,
            StartLineIndex = lineNodes.Length == 0 ? null : 0,
            EndLineIndexExclusive = lineNodes.Length
        };
    }

    private static TextLayoutSourceReference CreateRunSourceReference(TextLayoutRun run)
    {
        var contentVersion = new HashCode();
        contentVersion.Add(run.SourcePath, StringComparer.Ordinal);
        contentVersion.Add(run.SourceStart);
        contentVersion.Add(run.SourceLength);
        contentVersion.Add(run.Text, StringComparer.Ordinal);

        var styleVersion = new HashCode();
        styleVersion.Add(run.FaceId, StringComparer.Ordinal);
        styleVersion.Add(run.FamilyName, StringComparer.Ordinal);
        styleVersion.Add(run.Weight);
        styleVersion.Add(run.FontSize);
        styleVersion.Add(run.Italic);
        styleVersion.Add(run.Underline);
        styleVersion.Add(run.StrikeThrough);
        styleVersion.Add(run.CharacterSpacing);
        styleVersion.Add(run.WordSpacing);
        styleVersion.Add(run.ForegroundColor);
        styleVersion.Add(run.BackgroundColor);

        var nodeId = run.SourcePath ?? $"Segments[{run.SegmentIndex}]";
        return new TextLayoutSourceReference(
            run.SourcePath ?? $"Segments[{run.SegmentIndex}]",
            run.SegmentIndex,
            run.SourceStart,
            run.SourceLength,
            nodeId,
            contentVersion.ToHashCode(),
            styleVersion.ToHashCode());
    }

    private static (int ContentVersion, int StyleVersion) GetChildVersions(IReadOnlyList<TextLayoutPlanNode> children)
    {
        var contentVersion = new HashCode();
        var styleVersion = new HashCode();
        foreach (var child in children)
        {
            contentVersion.Add(child.Source.ContentVersion);
            styleVersion.Add(child.Source.StyleVersion);
        }

        return (contentVersion.ToHashCode(), styleVersion.ToHashCode());
    }

    private static IReadOnlyList<TextLayoutSourceReference> CollectFlatSourceReferences(TextLayoutPlan plan)
        => plan.Root.Children
            .SelectMany(line => line.Children)
            .Select(run => run.Source)
            .ToArray();

    private static IReadOnlyList<TextLayoutSourceReference> CollectFlatBoundaryReferences(TextLayoutPlan plan)
    {
        var runs = plan.Root.Children.SelectMany(line => line.Children).ToArray();
        if (runs.Length == 0)
        {
            return Array.Empty<TextLayoutSourceReference>();
        }

        return new[] { runs[0].Source, runs[^1].Source };
    }

    private static IReadOnlyList<TextLayoutContinuationReference> BuildFlatContinuations(
        TextLayoutPlan fittedPlan,
        TextLayoutPlan? remainderPlan,
        TextBreakKind breakKind,
        bool hasRemainder)
    {
        if (!hasRemainder)
        {
            return Array.Empty<TextLayoutContinuationReference>();
        }

        var fittedRuns = fittedPlan.Root.Children.SelectMany(line => line.Children).ToArray();
        var remainderRuns = remainderPlan?.Root.Children.SelectMany(line => line.Children).ToArray() ?? Array.Empty<TextLayoutPlanNode>();
        if (fittedRuns.Length == 0)
        {
            return Array.Empty<TextLayoutContinuationReference>();
        }

        return new[]
        {
            new TextLayoutContinuationReference(
                MapContinuationKind(breakKind),
                fittedRuns[^1].Source,
                remainderRuns.FirstOrDefault()?.Source,
                fittedRuns[^1].Source.Path,
                TextFragmentBreakReason.Overflow,
                false)
        };
    }

    private static TextLayoutFragmentMetadata BuildFragmentMetadata(
        TextBreakKind breakKind,
        TextFragmentBreak fragmentBreak,
        IReadOnlyList<TextLayoutContinuationReference> continuations)
        => new(fragmentBreak, continuations.FirstOrDefault());

    private static TextLayoutContinuationKind MapContinuationKind(TextBreakKind breakKind)
        => breakKind switch
        {
            TextBreakKind.Paragraph => TextLayoutContinuationKind.Paragraph,
            TextBreakKind.ListItem => TextLayoutContinuationKind.ListItem,
            TextBreakKind.TableRow => TextLayoutContinuationKind.TableRow,
            TextBreakKind.ContainerChild => TextLayoutContinuationKind.ContainerChild,
            _ => TextLayoutContinuationKind.Line
        };

    private static TextBoxLayoutRequest BuildFittedRequest(TextBoxLayoutRequest request, TextBoxLayoutRequest? remainderRequest)
    {
        if (remainderRequest is null)
        {
            return request;
        }

        var fittedSegments = new List<TextSegment>();
        var remainderSegments = remainderRequest.Segments;
        var remainderIndex = 0;
        for (var i = 0; i < request.Segments.Count; i++)
        {
            var original = request.Segments[i];
            var remainder = remainderIndex < remainderSegments.Count ? remainderSegments[remainderIndex] : null;
            if (remainder is not null && SameSourceSegment(original, remainder))
            {
                var consumedLength = Math.Max(0, original.Text.Length - remainder.Text.Length);
                if (consumedLength > 0)
                {
                    fittedSegments.Add(original with
                    {
                        Text = original.Text[..consumedLength],
                        SourceLength = original.SourceLength.HasValue ? Math.Min(original.SourceLength.Value, consumedLength) : consumedLength
                    });
                }

                remainderIndex++;
                break;
            }

            fittedSegments.Add(original);
        }

        return request with { Segments = fittedSegments };
    }

    private static bool SameSourceSegment(TextSegment left, TextSegment right)
        => string.Equals(left.SourcePath ?? string.Empty, right.SourcePath ?? string.Empty, StringComparison.Ordinal)
            && Equals(left.Style, right.Style);

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

            int? sourceStart = original.SourceStart.HasValue ? original.SourceStart.Value + consumed : null;
            int? sourceLength = original.SourceLength.HasValue ? Math.Max(0, original.SourceLength.Value - consumed) : null;
            segments.Add(original with
            {
                Text = original.Text[consumed..],
                SourceStart = sourceStart,
                SourceLength = sourceLength
            });
        }

        return request with { Segments = segments };
    }

    private static double GetContentWidth(double width, TextBoxStyle style)
        => GetContentWidth(width, StyleResolver.Resolve(style));

    private static double GetContentHeight(double height, TextBoxStyle style)
        => GetContentHeight(height, StyleResolver.Resolve(style));

    private static double GetContentWidth(double width, ComputedBoxStyle style)
        => width - style.Edges.HorizontalInset;

    private static double GetContentHeight(double height, ComputedBoxStyle style)
        => height - style.Edges.VerticalInset;

    private static int GetSegmentSourceStart(TextSegment segment, int tokenStart)
        => (segment.SourceStart ?? 0) + tokenStart;

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
            return new TokenMetrics(explicitLineHeight, style.LineBoxSizing, ascent, descent, lineGap);
        }

        return new TokenMetrics(explicitLineHeight, style.LineBoxSizing, style.FontSize, 0d, 0d);
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

        return new TokenMetrics(null, TextLineBoxSizing.AtLeastLineHeight, 0d, 0d, 0d);
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

        if (token.Metrics.LineBoxSizing == TextLineBoxSizing.AtLeastLineHeight)
        {
            line.LineBoxSizing = TextLineBoxSizing.AtLeastLineHeight;
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

        line.Height = line.LineBoxSizing switch
        {
            TextLineBoxSizing.Exact when line.ExplicitLineHeight.HasValue && line.ExplicitLineHeight.Value > 0d => line.ExplicitLineHeight.Value,
            TextLineBoxSizing.AtLeastLineHeight when line.ExplicitLineHeight.HasValue && line.ExplicitLineHeight.Value > 0d
                => Math.Max(line.ExplicitLineHeight.Value, naturalLineHeight),
            _ => naturalLineHeight
        };

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
        public PreparedToken(int segmentIndex, int sourceStart, int sourceLength, string sourcePath, string text, TextSegmentStyle style, PreparedTokenKind kind, TextFontFace? face, IReadOnlyList<TextLayoutGlyph> glyphs, double width, double measuredWidth, TokenMetrics metrics)
        {
            SegmentIndex = segmentIndex;
            SourceStart = sourceStart;
            SourceLength = sourceLength;
            SourcePath = sourcePath;
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
        public int SourceStart { get; }
        public int SourceLength { get; }
        public string SourcePath { get; }
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

    private sealed class ParagraphFormattingResult
    {
        public required IReadOnlyList<PreparedToken> Tokens { get; init; }
        public required List<PreparedLine> AllLines { get; init; }
        public required List<PreparedLine> VisibleLines { get; init; }
        public required double MeasuredWidth { get; init; }
        public required double MeasuredHeight { get; init; }
    }

    private sealed class IntrinsicParagraphMeasurement
    {
        public required IReadOnlyList<TextLayoutIssue> Issues { get; init; }
        public required IReadOnlyList<PreparedToken> Tokens { get; init; }
        public required List<PreparedLine> AllLines { get; init; }
        public required double MeasuredWidth { get; init; }
        public required double MeasuredHeight { get; init; }
    }

    private sealed class FlatFitPlanMaterializer : ITextLayoutFitPlanMaterializer
    {
        private readonly TextBoxLayoutRequest _fittedRequest;
        private readonly TextBoxLayoutRequest? _remainderRequest;

        public FlatFitPlanMaterializer(TextBoxLayoutRequest fittedRequest, TextBoxLayoutRequest? remainderRequest)
        {
            _fittedRequest = fittedRequest;
            _remainderRequest = remainderRequest;
        }

        public TextBoxLayoutRequest MaterializeFittedRequest(TextBoxLayoutRequest template)
            => template with { Segments = _fittedRequest.Segments };

        public TextBoxLayoutRequest? MaterializeRemainderRequest(TextBoxLayoutRequest template)
            => _remainderRequest is null ? null : template with { Segments = _remainderRequest.Segments };

        public RichTextBoxLayoutRequest MaterializeFittedRequest(RichTextBoxLayoutRequest template)
            => throw new NotSupportedException("Flat-text fit plans cannot materialize rich-text requests.");

        public RichTextBoxLayoutRequest? MaterializeRemainderRequest(RichTextBoxLayoutRequest template)
            => throw new NotSupportedException("Flat-text fit plans cannot materialize rich-text requests.");
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
        public TextLineBoxSizing LineBoxSizing { get; set; } = TextLineBoxSizing.Exact;
        public double Height { get; set; }
        public double BaselineOffset { get; set; }
        public double X { get; set; }
        public double BaselineY { get; set; }
    }

    private readonly record struct TokenMetrics(
        double? ExplicitLineHeight,
        TextLineBoxSizing LineBoxSizing,
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

    private static class ParagraphFormatter
    {
        private sealed class NormalizedParagraphInput
        {
            public required IReadOnlyList<TextSegment> Segments { get; init; }
        }

        private sealed class ShapedParagraph
        {
            public required IReadOnlyList<PreparedToken> Tokens { get; init; }
        }

        private sealed class BreakOpportunityPlan
        {
            public required IReadOnlyList<PreparedToken> Tokens { get; init; }
        }

        private sealed class PositionedParagraph
        {
            public required List<PreparedLine> AllLines { get; init; }
            public required List<PreparedLine> VisibleLines { get; init; }
        }

        private sealed class ParagraphDecorationPlan
        {
            public static readonly ParagraphDecorationPlan Empty = new();
        }

        public static ParagraphFormattingResult Format(
            TextBoxLayoutRequest request,
            FontCache fonts,
            List<TextLayoutIssue> issues,
            LayoutConstraints constraints,
            TextLayoutAnalysisContext? context)
        {
            var intrinsic = MeasureIntrinsic(request, fonts, issues, constraints.AvailableWidth, context);
            var positioned = Position(request, intrinsic, constraints);
            _ = PlanDecorations(positioned);

            return new ParagraphFormattingResult
            {
                Tokens = intrinsic.Tokens,
                AllLines = positioned.AllLines,
                VisibleLines = positioned.VisibleLines,
                MeasuredWidth = intrinsic.MeasuredWidth,
                MeasuredHeight = intrinsic.MeasuredHeight
            };
        }

        private static IntrinsicParagraphMeasurement MeasureIntrinsic(
            TextBoxLayoutRequest request,
            FontCache fonts,
            List<TextLayoutIssue> issues,
            double contentWidth,
            TextLayoutAnalysisContext? context)
        {
            var key = BuildIntrinsicParagraphMeasurementCacheKey(request, contentWidth);
            if (context?.IntrinsicMeasurements.TryGet(key, out var cachedEntry) == true
                && cachedEntry?.Value is IntrinsicParagraphMeasurement cached)
            {
                issues.AddRange(cached.Issues);
                return cached;
            }

            var localIssues = new List<TextLayoutIssue>();
            var normalized = Normalize(request);
            var shapedParagraph = Shape(request, normalized, fonts, localIssues);
            var breakPlan = AnalyzeBreakOpportunities(shapedParagraph);
            var allLines = BuildLines(request, breakPlan, localIssues, contentWidth);
            issues.AddRange(localIssues);

            var measurement = new IntrinsicParagraphMeasurement
            {
                Issues = localIssues.ToArray(),
                Tokens = shapedParagraph.Tokens,
                AllLines = allLines,
                MeasuredWidth = allLines.Count == 0 ? 0d : allLines.Max(x => x.MeasuredWidth),
                MeasuredHeight = allLines.Sum(x => x.Height)
            };

            context?.IntrinsicMeasurements.Set(
                key,
                new IntrinsicMeasurementCacheEntry
                {
                    NaturalSize = new TextLayoutSize(measurement.MeasuredWidth, measurement.MeasuredHeight),
                    VisibleSize = new TextLayoutSize(measurement.MeasuredWidth, measurement.MeasuredHeight),
                    Value = measurement
                });
            return measurement;
        }

        private static NormalizedParagraphInput Normalize(TextBoxLayoutRequest request)
            => new()
            {
                Segments = request.Segments
            };

        private static ShapedParagraph Shape(TextBoxLayoutRequest request, NormalizedParagraphInput input, FontCache fonts, List<TextLayoutIssue> issues)
            => new()
            {
                Tokens = PrepareTokens(request with { Segments = input.Segments }, fonts, issues)
            };

        private static BreakOpportunityPlan AnalyzeBreakOpportunities(ShapedParagraph paragraph)
            => new()
            {
                Tokens = paragraph.Tokens
            };

        private static List<PreparedLine> BuildLines(TextBoxLayoutRequest request, BreakOpportunityPlan breakPlan, List<TextLayoutIssue> issues, double contentWidth)
            => TextBoxLayoutEngine.BuildLines(request, (List<PreparedToken>)breakPlan.Tokens, issues, contentWidth);

        private static PositionedParagraph Position(TextBoxLayoutRequest request, IntrinsicParagraphMeasurement intrinsic, LayoutConstraints constraints)
        {
            var allLines = CloneLines(intrinsic.AllLines);
            List<PreparedLine> visibleLines;
            if (constraints.ClipToHeight && request.VerticalAlignment != TextVerticalAlignment.Top)
            {
                PositionLines(request, allLines, constraints.AvailableWidth, constraints.AvailableHeight);
                var edges = StyleResolver.Resolve(request.BoxStyle).Edges;
                visibleLines = ClipPositionedLines(edges.Insets.Top, edges.Insets.Top + constraints.AvailableHeight, allLines);
            }
            else
            {
                visibleLines = constraints.ClipToHeight ? ClipLines(constraints.AvailableHeight, allLines) : allLines;
                PositionLines(request, visibleLines, constraints.AvailableWidth, constraints.AvailableHeight);
            }

            return new PositionedParagraph
            {
                AllLines = allLines,
                VisibleLines = visibleLines
            };
        }

        private static ParagraphDecorationPlan PlanDecorations(PositionedParagraph positioned)
            => ParagraphDecorationPlan.Empty;

        private static List<PreparedLine> CloneLines(IReadOnlyList<PreparedLine> source)
        {
            var clone = new List<PreparedLine>(source.Count);
            foreach (var line in source)
            {
                var clonedLine = new PreparedLine
                {
                    Width = line.Width,
                    MeasuredWidth = line.MeasuredWidth,
                    Ascent = line.Ascent,
                    Descent = line.Descent,
                    LineGap = line.LineGap,
                    ExplicitLineHeight = line.ExplicitLineHeight,
                    LineBoxSizing = line.LineBoxSizing,
                    Height = line.Height,
                    BaselineOffset = line.BaselineOffset,
                    X = line.X,
                    BaselineY = line.BaselineY
                };

                foreach (var token in line.Tokens)
                {
                    clonedLine.Tokens.Add(new PreparedToken(
                        token.SegmentIndex,
                        token.SourceStart,
                        token.SourceLength,
                        token.SourcePath,
                        token.Text,
                        token.Style,
                        token.Kind,
                        token.Face,
                        token.Glyphs,
                        token.Width,
                        token.MeasuredWidth,
                        token.Metrics));
                }

                clone.Add(clonedLine);
            }

            return clone;
        }

        private static IntrinsicMeasurementCacheKey BuildIntrinsicParagraphMeasurementCacheKey(TextBoxLayoutRequest request, double contentWidth)
        {
            var contentVersion = new HashCode();
            var styleVersion = new HashCode();
            foreach (var segment in request.Segments)
            {
                contentVersion.Add(segment.SourcePath, StringComparer.Ordinal);
                contentVersion.Add(segment.SourceStart);
                contentVersion.Add(segment.SourceLength);
                contentVersion.Add(segment.Text, StringComparer.Ordinal);

                styleVersion.Add(segment.Style.FamilyName, StringComparer.Ordinal);
                styleVersion.Add(segment.Style.Weight);
                styleVersion.Add(segment.Style.FontSize);
                styleVersion.Add(segment.Style.Italic);
                styleVersion.Add(segment.Style.Underline);
                styleVersion.Add(segment.Style.StrikeThrough);
                styleVersion.Add(segment.Style.CharacterSpacing);
                styleVersion.Add(segment.Style.WordSpacing);
                styleVersion.Add(segment.Style.LineSpacing);
                styleVersion.Add(segment.Style.LineBoxSizing);
                styleVersion.Add(segment.Style.ForegroundColor);
                styleVersion.Add(segment.Style.BackgroundColor);
            }

            styleVersion.Add(RuntimeHelpers.GetHashCode(request.FontLibrary));
            foreach (var fallback in request.FallbackFamilyNames)
            {
                styleVersion.Add(fallback, StringComparer.Ordinal);
            }

            var nodeId = request.Segments.Count == 0
                ? "Segments"
                : string.Join("|", request.Segments.Select(static (segment, index) => segment.SourcePath ?? $"Segments[{index}]"));

            return new IntrinsicMeasurementCacheKey(
                IntrinsicMeasurementUnitKind.Paragraph,
                nodeId,
                contentVersion.ToHashCode(),
                styleVersion.ToHashCode(),
                contentWidth,
                BuildFlatLayoutModeFlags(request));
        }

        private static int BuildFlatLayoutModeFlags(TextBoxLayoutRequest request)
        {
            var flags = 0;
            flags |= request.PreserveTrailingWhitespaceInWidth ? 1 << 0 : 0;
            flags |= ((int)request.MissingFontBehavior & 0x3) << 1;
            flags |= ((int)request.MissingGlyphBehavior & 0x3) << 3;
            flags |= ((int)request.HorizontalAlignment & 0x3) << 5;
            flags |= ((int)request.VerticalAlignment & 0x3) << 7;
            flags |= ((int)request.MetricPreference & 0x7) << 9;
            return flags;
        }
    }
}
