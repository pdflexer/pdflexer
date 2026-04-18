using System.Buffers;
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
        var fit = Fit(request, context);
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
        => Layout(request, context: null);

    public TextBoxLayoutResult Layout(TextBoxLayoutRequest request, TextLayoutAnalysisContext? context)
        => LayoutCore(request, context);

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
        var fonts = GetFontCache(request.FontLibrary, context, out var ownsFonts);
        try
        {
            var formatting = ParagraphFormatter.Format(
                request,
                fonts,
                issues,
                new LayoutConstraints(contentWidth, contentHeight, request.OverflowMode, request.OverflowMode is TextOverflowMode.Clip or TextOverflowMode.Fragment),
                context);
            var measurement = formatting.Measurement;
            try
            {
                if (issues.Any(x => x.Kind is TextLayoutIssueKind.MissingFamily or TextLayoutIssueKind.MissingWeight or TextLayoutIssueKind.MissingGlyph)
                    && request.MissingFontBehavior == TextResolutionBehavior.Error)
                {
                    return CreateResult(request, measurement, Array.Empty<PositionedLine>(), issues, TextLayoutStatus.Error, 0, 0, transferGlyphBuffer: context is null);
                }

                var lines = measurement.AllLines.Span;
                var totalMeasuredHeight = formatting.MeasuredHeight;
                var fitsHeight = totalMeasuredHeight <= contentHeight + 0.0001d;
                var fitsWidth = true;
                for (var i = 0; i < lines.Length; i++)
                {
                    if (lines[i].MeasuredWidth > contentWidth + 0.0001d)
                    {
                        fitsWidth = false;
                        break;
                    }
                }

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
                return CreateResult(request, measurement, renderedLines, issues, status, measuredWidth, totalMeasuredHeight, transferGlyphBuffer: context is null);
            }
            finally
            {
                if (context is null)
                {
                    measurement.Dispose();
                }
            }
        }
        finally
        {
            if (ownsFonts)
            {
                fonts.Dispose();
            }
        }
    }

    public TextBoxFitResult Fit(TextBoxLayoutRequest request)
        => Fit(request, context: null);

    public TextBoxFitResult Fit(TextBoxLayoutRequest request, TextLayoutAnalysisContext? context)
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
        var fonts = GetFontCache(request.FontLibrary, context, out var ownsFonts);
        try
        {
            var formatting = ParagraphFormatter.Format(
                request,
                fonts,
                issues,
                new LayoutConstraints(contentWidth, contentHeight, request.OverflowMode, ClipToHeight: true),
                context);
            var measurement = formatting.Measurement;
            try
            {
                var fittingLines = formatting.VisibleLines;

                var fittingMeasuredWidth = 0d;
                var fittingMeasuredHeight = 0d;
                var fittingTokenCount = 0;
                for (var i = 0; i < fittingLines.Count; i++)
                {
                    var line = measurement.AllLines[fittingLines[i].LineIndex];
                    fittingMeasuredWidth = Math.Max(fittingMeasuredWidth, line.MeasuredWidth);
                    fittingMeasuredHeight += line.Height;
                    fittingTokenCount += line.TokenCount;
                }

                var fittingLayout = CreateResult(
                    request,
                    measurement,
                    fittingLines,
                    issues,
                    issues.Any(x => x.Kind is TextLayoutIssueKind.MissingFamily or TextLayoutIssueKind.MissingWeight or TextLayoutIssueKind.MissingGlyph)
                        ? TextLayoutStatus.Error
                        : TextLayoutStatus.Success,
                    fittingMeasuredWidth,
                    fittingMeasuredHeight,
                    transferGlyphBuffer: context is null);

                var hasRemainder = fittingTokenCount < measurement.Tokens.Count;
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

                var remainderRequest = BuildRemainderRequest(request, measurement.Tokens.Span, fittingTokenCount);
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
            finally
            {
                if (context is null)
                {
                    measurement.Dispose();
                }
            }
        }
        finally
        {
            if (ownsFonts)
            {
                fonts.Dispose();
            }
        }
    }

    private static ShapedFlatContent ShapeSegmentsIntoDescriptors(TextBoxLayoutRequest request, TextLayoutFontCache fonts, List<TextLayoutIssue> issues)
    {
        var segments = NormalizeSegments(request);
        using var tokenBuilder = new PooledBufferBuilder<TokenDescriptor>();
        using var glyphBuilder = new PooledBufferBuilder<TextLayoutGlyph>();
        using var sliceBuilder = new PooledBufferBuilder<TokenSlice>();

        for (var segmentIndex = 0; segmentIndex < segments.Length; segmentIndex++)
        {
            var segment = segments[segmentIndex];
            TryResolveFace(request, segment.Style, fonts, out var segmentFace, out _);
            var segmentMetrics = ResolveTokenMetrics(request.MetricPreference, segment.Style, segmentFace);

            sliceBuilder.Clear();
            Tokenize(segment.Text, sliceBuilder);
            var tokenSpans = sliceBuilder.WrittenSpan;

            for (var tokenIndex = 0; tokenIndex < tokenSpans.Length;)
            {
                var token = tokenSpans[tokenIndex];
                if (token.Kind == PreparedTokenKind.NewLine)
                {
                    tokenBuilder.Add(new TokenDescriptor(
                        segmentIndex,
                        token.Kind,
                        token.Start,
                        GetSegmentSourceStart(segment.Segment, token.Start),
                        token.Length,
                        segment.SourcePath,
                        segment.SourceNodeId,
                        segment.Style,
                        null,
                        0,
                        0,
                        0d,
                        0d,
                        segmentMetrics));
                    tokenIndex++;
                    continue;
                }

                var chunkStart = token.Start;
                var chunkEnd = token.End;
                var chunkEndIndex = tokenIndex + 1;
                while (chunkEndIndex < tokenSpans.Length && tokenSpans[chunkEndIndex].Kind != PreparedTokenKind.NewLine)
                {
                    chunkEnd = tokenSpans[chunkEndIndex].End;
                    chunkEndIndex++;
                }

                AppendShapedChunkTokens(
                    request,
                    fonts,
                    issues,
                    tokenBuilder,
                    glyphBuilder,
                    segmentIndex,
                    segment,
                    tokenSpans,
                    tokenIndex,
                    chunkEndIndex,
                    chunkStart,
                    chunkEnd - chunkStart);
                tokenIndex = chunkEndIndex;
            }
        }

        return new ShapedFlatContent
        {
            Segments = segments,
            Tokens = tokenBuilder.MoveToOwnedBuffer(),
            Glyphs = glyphBuilder.MoveToOwnedBuffer()
        };
    }

    private static void AppendShapedChunkTokens(
        TextBoxLayoutRequest request,
        TextLayoutFontCache fonts,
        List<TextLayoutIssue> issues,
        PooledBufferBuilder<TokenDescriptor> tokenBuilder,
        PooledBufferBuilder<TextLayoutGlyph> glyphBuilder,
        int segmentIndex,
        NormalizedSegment segment,
        ReadOnlySpan<TokenSlice> tokenSpans,
        int startIndex,
        int endIndex,
        int chunkStart,
        int chunkLength)
    {
        var style = segment.Style;
        if (!TryResolveFace(request, style, fonts, out var face, out var faceError))
        {
            issues.Add(faceError!);
            var missingMetrics = ResolveTokenMetrics(request.MetricPreference, style, null);
            for (var i = startIndex; i < endIndex; i++)
            {
                var token = tokenSpans[i];
                tokenBuilder.Add(new TokenDescriptor(
                    segmentIndex,
                    token.Kind,
                    token.Start,
                    GetSegmentSourceStart(segment.Segment, token.Start),
                    token.Length,
                    segment.SourcePath,
                    segment.SourceNodeId,
                    style,
                    null,
                    0,
                    0,
                    0d,
                    0d,
                    missingMetrics));
            }

            return;
        }

        var chunkText = segment.Text.AsSpan(chunkStart, chunkLength);
        var normalizedWhitespace = NormalizeShapingWhitespace(chunkText);
        var shapingText = normalizedWhitespace is null ? chunkText : normalizedWhitespace;
        if (!TryShapeWithFallback(request, fonts, style, shapingText, face!, out face, out var glyphs, out var missingGlyph))
        {
            issues.Add(new TextLayoutIssue(TextLayoutIssueKind.MissingGlyph, missingGlyph ?? $"Unable to shape segment '{new string(chunkText)}'.", segmentIndex, style.FamilyName, style.Weight, face?.FaceId));
            var fallbackMetrics = ResolveTokenMetrics(request.MetricPreference, style, face);
            for (var i = startIndex; i < endIndex; i++)
            {
                var token = tokenSpans[i];
                tokenBuilder.Add(new TokenDescriptor(
                    segmentIndex,
                    token.Kind,
                    token.Start,
                    GetSegmentSourceStart(segment.Segment, token.Start),
                    token.Length,
                    segment.SourcePath,
                    segment.SourceNodeId,
                    style,
                    face,
                    0,
                    0,
                    0d,
                    0d,
                    fallbackMetrics));
            }

            return;
        }

        var metrics = ResolveTokenMetrics(request.MetricPreference, style, face);
        var chunkGlyphs = glyphs.AsSpan();
        var charCount = chunkText.Length;
        var firstByChar = ArrayPool<int>.Shared.Rent(Math.Max(1, charCount));
        var lastByChar = ArrayPool<int>.Shared.Rent(Math.Max(1, charCount));
        Array.Fill(firstByChar, -1, 0, Math.Max(1, charCount));
        Array.Fill(lastByChar, -1, 0, Math.Max(1, charCount));

        try
        {
            for (var i = 0; i < chunkGlyphs.Length; i++)
            {
                var cluster = (int)chunkGlyphs[i].Cluster;
                if ((uint)cluster >= (uint)charCount)
                {
                    continue;
                }

                if (firstByChar[cluster] == -1)
                {
                    firstByChar[cluster] = i;
                }

                lastByChar[cluster] = i;
            }

            for (var i = startIndex; i < endIndex; i++)
            {
                var token = tokenSpans[i];
                var relativeStart = token.Start - chunkStart;
                var relativeEnd = token.End - chunkStart;
                var firstIndex = -1;
                var lastIndex = -1;
                for (var charIndex = relativeStart; charIndex < relativeEnd; charIndex++)
                {
                    if (firstByChar[charIndex] >= 0)
                    {
                        firstIndex = firstIndex == -1 ? firstByChar[charIndex] : Math.Min(firstIndex, firstByChar[charIndex]);
                    }

                    if (lastByChar[charIndex] >= 0)
                    {
                        lastIndex = Math.Max(lastIndex, lastByChar[charIndex]);
                    }
                }

                var glyphStart = glyphBuilder.Count;
                var width = 0d;
                if (firstIndex >= 0 && lastIndex >= firstIndex)
                {
                    var offsetX = chunkGlyphs[firstIndex].X;
                    for (var sourceIndex = firstIndex; sourceIndex <= lastIndex; sourceIndex++)
                    {
                        var glyph = chunkGlyphs[sourceIndex];
                        var advance = glyph.Advance;
                        if (sourceIndex == lastIndex && sourceIndex + 1 < chunkGlyphs.Length)
                        {
                            advance -= style.CharacterSpacing;
                        }

                        width += advance;
                        glyphBuilder.Add(glyph with
                        {
                            X = glyph.X - offsetX,
                            Advance = advance
                        });
                    }
                }

                tokenBuilder.Add(new TokenDescriptor(
                    segmentIndex,
                    token.Kind,
                    token.Start,
                    GetSegmentSourceStart(segment.Segment, token.Start),
                    token.Length,
                    segment.SourcePath,
                    segment.SourceNodeId,
                    style,
                    face,
                    glyphStart,
                    glyphBuilder.Count - glyphStart,
                    width,
                    width,
                    metrics));
            }
        }
        finally
        {
            ArrayPool<int>.Shared.Return(firstByChar);
            ArrayPool<int>.Shared.Return(lastByChar);
        }
    }

    private static bool TryResolveFace(
        TextBoxLayoutRequest request,
        TextSegmentStyle style,
        TextLayoutFontCache fonts,
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
        TextLayoutFontCache fonts,
        TextSegmentStyle style,
        ReadOnlySpan<char> text,
        TextFontFace initialFace,
        out TextFontFace? resolvedFace,
        out TextLayoutGlyph[] glyphs,
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
            if (AllGlyphsResolved(shaped))
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

    private static bool AllGlyphsResolved(IReadOnlyList<TextLayoutGlyph> glyphs)
    {
        for (var i = 0; i < glyphs.Count; i++)
        {
            if (glyphs[i].GlyphId == 0)
            {
                return false;
            }
        }

        return true;
    }

    private static TextLayoutGlyph[] Shape(HarfRustFont font, HarfRustShapeSession session, TextFontFace face, ReadOnlySpan<char> text, TextSegmentStyle style)
    {
        if (text.IsEmpty)
        {
            return Array.Empty<TextLayoutGlyph>();
        }

        using var result = session.Shape(font, text, guessSegmentProperties: true);

        var scale = style.FontSize / font.UnitsPerEm;
        var glyphs = new TextLayoutGlyph[result.Length];
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

            glyphs[i] = new TextLayoutGlyph(
                info.GlyphId,
                info.Cluster,
                penX + offsetX,
                offsetY,
                advance,
                offsetX,
                offsetY);

            penX += advance;
        }

        return glyphs;
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

    private static char[]? NormalizeShapingWhitespace(ReadOnlySpan<char> text)
    {
        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] != '\n' && char.IsWhiteSpace(text[i]) && text[i] != ' ')
            {
                var chars = text.ToArray();
                for (var j = i; j < chars.Length; j++)
                {
                    if (chars[j] != '\n' && char.IsWhiteSpace(chars[j]) && chars[j] != ' ')
                    {
                        chars[j] = ' ';
                    }
                }

                return chars;
            }
        }

        return null;
    }

    private static OwnedPooledBuffer<LineDescriptor> BuildLines(TextBoxLayoutRequest request, ReadOnlySpan<TokenDescriptor> tokens, double contentWidth)
    {
        using var lineBuilder = new PooledBufferBuilder<LineDescriptor>();
        var paragraphStrut = ResolveParagraphStrut(tokens);
        var lineFallbackStrut = paragraphStrut;
        var current = new LineAccumulator(0);

        for (var tokenIndex = 0; tokenIndex < tokens.Length; tokenIndex++)
        {
            var token = tokens[tokenIndex];
            if (token.Kind == PreparedTokenKind.NewLine)
            {
                FinalizeLine(request, tokens, lineBuilder, ref current, lineFallbackStrut, paragraphStrut);
                current = new LineAccumulator(tokenIndex + 1);
                continue;
            }

            var wouldOverflow = current.TokenCount > 0 && current.Width + token.Width > contentWidth;
            if (wouldOverflow)
            {
                FinalizeLine(request, tokens, lineBuilder, ref current, lineFallbackStrut, paragraphStrut);
                current = new LineAccumulator(token.Kind == PreparedTokenKind.Whitespace ? tokenIndex + 1 : tokenIndex);
                if (token.Kind == PreparedTokenKind.Whitespace)
                {
                    continue;
                }
            }

            current.TokenCount++;
            current.Width += token.Width;
            ApplyTokenMetrics(ref current, token);
            if (token.Kind == PreparedTokenKind.Text)
            {
                lineFallbackStrut = token.Metrics;
            }
        }

        FinalizeLine(request, tokens, lineBuilder, ref current, lineFallbackStrut, paragraphStrut);
        return lineBuilder.MoveToOwnedBuffer();
    }

    private static void FinalizeLine(
        TextBoxLayoutRequest request,
        ReadOnlySpan<TokenDescriptor> tokens,
        PooledBufferBuilder<LineDescriptor> lineBuilder,
        ref LineAccumulator current,
        TokenMetrics fallbackStrut,
        TokenMetrics paragraphStrut)
    {
        if (current.TokenCount == 0
            && lineBuilder.Count == 0
            && fallbackStrut.ExplicitLineHeight is null
            && fallbackStrut.Ascent == 0
            && fallbackStrut.Descent == 0
            && fallbackStrut.LineGap == 0)
        {
            return;
        }

        if (current.TokenCount == 0)
        {
            current.Ascent = fallbackStrut.Ascent;
            current.Descent = fallbackStrut.Descent;
            current.LineGap = fallbackStrut.LineGap;
            current.ExplicitLineHeight = fallbackStrut.ExplicitLineHeight;
            current.LineBoxSizing = fallbackStrut.LineBoxSizing;
        }

        ResolveLineBox(ref current, paragraphStrut);

        var measuredWidth = 0d;
        if (current.TokenCount > 0)
        {
            var trailingWhitespace = 0d;
            if (!request.PreserveTrailingWhitespaceInWidth)
            {
                for (var i = current.TokenStart + current.TokenCount - 1; i >= current.TokenStart; i--)
                {
                    if (tokens[i].Kind != PreparedTokenKind.Whitespace)
                    {
                        break;
                    }

                    trailingWhitespace += tokens[i].Width;
                }
            }

            measuredWidth = Math.Max(0d, current.Width - trailingWhitespace);
        }

        lineBuilder.Add(current.ToDescriptor(measuredWidth));
    }

    private static PositionedLine[] ClipLines(double contentHeight, IReadOnlyList<LineDescriptor> lines)
    {
        using var builder = new PooledBufferBuilder<PositionedLine>();
        var height = 0d;
        for (var lineIndex = 0; lineIndex < lines.Count; lineIndex++)
        {
            var next = height + lines[lineIndex].Height;
            if (builder.Count > 0 && next > contentHeight + 0.0001d)
            {
                break;
            }

            builder.Add(new PositionedLine(lineIndex, 0d, 0d));
            height = next;
        }

        return builder.ToArray();
    }

    private static PositionedLine[] ClipPositionedLines(double viewportTop, double viewportBottom, IReadOnlyList<LineDescriptor> lines, IReadOnlyList<PositionedLine> positionedLines)
    {
        using var builder = new PooledBufferBuilder<PositionedLine>();
        for (var i = 0; i < positionedLines.Count; i++)
        {
            var positioned = positionedLines[i];
            var line = lines[positioned.LineIndex];
            var lineTop = GetLineTop(positioned, line);
            var lineBottom = lineTop + line.Height;
            if (lineTop >= viewportTop - 0.0001d && lineBottom <= viewportBottom + 0.0001d)
            {
                builder.Add(positioned);
            }
        }

        return builder.ToArray();
    }

    private static double GetLineTop(PositionedLine positioned, LineDescriptor line)
        => positioned.BaselineY - line.BaselineOffset;

    private static PositionedLine[] PositionLines(TextBoxLayoutRequest request, IReadOnlyList<LineDescriptor> allLines, IReadOnlyList<PositionedLine> lines, double contentWidth, double contentHeight)
    {
        var renderedHeight = 0d;
        for (var i = 0; i < lines.Count; i++)
        {
            renderedHeight += allLines[lines[i].LineIndex].Height;
        }

        var edges = StyleResolver.Resolve(request.BoxStyle).Edges;
        var topOffset = request.VerticalAlignment switch
        {
            TextVerticalAlignment.Center => (contentHeight - renderedHeight) / 2d,
            TextVerticalAlignment.Bottom => contentHeight - renderedHeight,
            _ => 0d
        };

        var positioned = new PositionedLine[lines.Count];
        double y = edges.Insets.Top + topOffset;
        for (var i = 0; i < lines.Count; i++)
        {
            var lineIndex = lines[i].LineIndex;
            var line = allLines[lineIndex];
            var x = request.HorizontalAlignment switch
            {
                TextHorizontalAlignment.Center when line.MeasuredWidth < contentWidth => edges.Insets.Left + ((contentWidth - line.MeasuredWidth) / 2d),
                TextHorizontalAlignment.Right when line.MeasuredWidth < contentWidth => edges.Insets.Left + contentWidth - line.MeasuredWidth,
                _ => edges.Insets.Left
            };

            positioned[i] = new PositionedLine(lineIndex, x, y + line.BaselineOffset);
            y += line.Height;
        }

        return positioned;
    }

    private static TextBoxLayoutResult CreateResult(
        TextBoxLayoutRequest request,
        IntrinsicParagraphMeasurement measurement,
        IReadOnlyList<PositionedLine> renderedLines,
        IReadOnlyList<TextLayoutIssue> issues,
        TextLayoutStatus status,
        double measuredWidth,
        double measuredHeight,
        bool transferGlyphBuffer)
    {
        var edges = StyleResolver.Resolve(request.BoxStyle).Edges;
        var segments = new TextSegment[measurement.Segments.Count];
        for (var i = 0; i < measurement.Segments.Count; i++)
        {
            segments[i] = measurement.Segments[i].Segment;
        }

        var totalRunCount = 0;
        for (var i = 0; i < renderedLines.Count; i++)
        {
            totalRunCount += measurement.AllLines[renderedLines[i].LineIndex].TokenCount;
        }

        var runBuffer = new TextLayoutRunDescriptor[totalRunCount];
        var lineBuffer = new TextLayoutLineDescriptor[renderedLines.Count];
        double renderedContentWidth = 0d;
        double renderedContentHeight = 0d;
        var runIndex = 0;
        for (var i = 0; i < renderedLines.Count; i++)
        {
            var positionedLine = renderedLines[i];
            var line = measurement.AllLines[positionedLine.LineIndex];
            var tokenX = 0d;
            var lineRunStart = runIndex;
            for (var j = 0; j < line.TokenCount; j++)
            {
                var token = measurement.Tokens[line.TokenStart + j];
                runBuffer[runIndex++] = new TextLayoutRunDescriptor(
                    token.SegmentIndex,
                    token.SourceTextStart,
                    token.SourceLength,
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
                    tokenX,
                    positionedLine.BaselineY,
                    token.Width,
                    token.MeasuredWidth,
                    line.Height,
                    false,
                    token.Style.StrikeThrough,
                    token.SourceStart,
                    token.SourcePath,
                    token.SourceNodeId,
                    token.Style.LineSpacing,
                    token.GlyphStart,
                    token.GlyphCount);
                tokenX += token.Width;
            }

            lineBuffer[i] = new TextLayoutLineDescriptor(
                i,
                positionedLine.X,
                positionedLine.BaselineY,
                line.Width,
                line.MeasuredWidth,
                line.Height,
                line.BaselineOffset,
                lineRunStart,
                line.TokenCount);

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
            Issues = issues.ToArray(),
            Decorations = Array.Empty<TextLayoutDecoration>(),
            Segments = segments,
            GlyphBuffer = transferGlyphBuffer ? measurement.Glyphs.DetachArray() : measurement.Glyphs.Array,
            RunBuffer = runBuffer,
            LineBuffer = lineBuffer
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
        contentVersion.Add(run.SourceNodeId, StringComparer.Ordinal);
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

        var nodeId = run.SourceNodeId ?? run.SourcePath ?? $"Segments[{run.SegmentIndex}]";
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
    {
        var count = 0;
        for (var i = 0; i < plan.Root.Children.Count; i++)
        {
            count += plan.Root.Children[i].Children.Count;
        }

        if (count == 0)
        {
            return Array.Empty<TextLayoutSourceReference>();
        }

        var references = new TextLayoutSourceReference[count];
        var index = 0;
        for (var i = 0; i < plan.Root.Children.Count; i++)
        {
            var line = plan.Root.Children[i];
            for (var j = 0; j < line.Children.Count; j++)
            {
                references[index++] = line.Children[j].Source;
            }
        }

        return references;
    }

    private static IReadOnlyList<TextLayoutSourceReference> CollectFlatBoundaryReferences(TextLayoutPlan plan)
    {
        TextLayoutSourceReference? first = null;
        TextLayoutSourceReference? last = null;
        for (var i = 0; i < plan.Root.Children.Count; i++)
        {
            var line = plan.Root.Children[i];
            if (line.Children.Count == 0)
            {
                continue;
            }

            first ??= line.Children[0].Source;
            last = line.Children[^1].Source;
        }

        return first is null || last is null ? Array.Empty<TextLayoutSourceReference>() : new[] { first, last };
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

        TextLayoutSourceReference? fittedLast = null;
        for (var i = 0; i < fittedPlan.Root.Children.Count; i++)
        {
            var line = fittedPlan.Root.Children[i];
            if (line.Children.Count > 0)
            {
                fittedLast = line.Children[^1].Source;
            }
        }

        if (fittedLast is null)
        {
            return Array.Empty<TextLayoutContinuationReference>();
        }

        TextLayoutSourceReference? remainderFirst = null;
        if (remainderPlan is not null)
        {
            for (var i = 0; i < remainderPlan.Root.Children.Count; i++)
            {
                var line = remainderPlan.Root.Children[i];
                if (line.Children.Count == 0)
                {
                    continue;
                }

                remainderFirst = line.Children[0].Source;
                break;
            }
        }

        return new[]
        {
            new TextLayoutContinuationReference(
                MapContinuationKind(breakKind),
                fittedLast,
                remainderFirst,
                fittedLast.Path,
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

    private static TextBoxLayoutRequest BuildRemainderRequest(TextBoxLayoutRequest request, ReadOnlySpan<TokenDescriptor> preparedTokens, int consumedTokenCount)
    {
        if (consumedTokenCount <= 0)
        {
            return request;
        }

        var consumedBySegment = new int[request.Segments.Count];
        for (var i = 0; i < consumedTokenCount; i++)
        {
            var token = preparedTokens[i];
            consumedBySegment[token.SegmentIndex] += token.SourceLength;
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

    private static string NormalizeInputText(string text)
        => text.IndexOf('\r') >= 0
            ? text.Replace("\r", string.Empty, StringComparison.Ordinal)
            : text;

    private static NormalizedSegment[] NormalizeSegments(TextBoxLayoutRequest request)
    {
        var segments = new NormalizedSegment[request.Segments.Count];
        for (var i = 0; i < request.Segments.Count; i++)
        {
            var segment = request.Segments[i];
            var sourcePath = segment.SourcePath ?? $"Segments[{i}]";
            segments[i] = new NormalizedSegment(
                segment,
                NormalizeInputText(segment.Text),
                sourcePath,
                segment.SourceNodeId ?? sourcePath);
        }

        return segments;
    }

    private static void Tokenize(string text, PooledBufferBuilder<TokenSlice> tokens)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
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

    private static TokenMetrics ResolveParagraphStrut(ReadOnlySpan<TokenDescriptor> tokens)
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

    private static void ApplyTokenMetrics(ref LineAccumulator line, TokenDescriptor token)
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

    private static void ResolveLineBox(ref LineAccumulator line, TokenMetrics paragraphStrut)
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

    private readonly record struct NormalizedSegment(TextSegment Segment, string Text, string SourcePath, string SourceNodeId)
    {
        public TextSegmentStyle Style => Segment.Style;
    }

    private readonly record struct TokenDescriptor(
        int SegmentIndex,
        PreparedTokenKind Kind,
        int SourceTextStart,
        int SourceStart,
        int SourceLength,
        string SourcePath,
        string SourceNodeId,
        TextSegmentStyle Style,
        TextFontFace? Face,
        int GlyphStart,
        int GlyphCount,
        double Width,
        double MeasuredWidth,
        TokenMetrics Metrics);

    private readonly record struct LineDescriptor(
        int TokenStart,
        int TokenCount,
        double Width,
        double MeasuredWidth,
        double Height,
        double BaselineOffset,
        double Ascent,
        double Descent,
        double LineGap,
        double? ExplicitLineHeight,
        TextLineBoxSizing LineBoxSizing);

    private readonly record struct PositionedLine(int LineIndex, double X, double BaselineY);

    private struct LineAccumulator
    {
        public LineAccumulator(int tokenStart)
        {
            TokenStart = tokenStart;
        }

        public int TokenStart;
        public int TokenCount;
        public double Width;
        public double Ascent;
        public double Descent;
        public double LineGap;
        public double? ExplicitLineHeight;
        public TextLineBoxSizing LineBoxSizing;
        public double Height;
        public double BaselineOffset;

        public LineDescriptor ToDescriptor(double measuredWidth)
            => new(TokenStart, TokenCount, Width, measuredWidth, Height, BaselineOffset, Ascent, Descent, LineGap, ExplicitLineHeight, LineBoxSizing);
    }

    private sealed class GlyphSlice : IReadOnlyList<TextLayoutGlyph>
    {
        private readonly TextLayoutGlyph[] _glyphs;
        private readonly int _start;

        public GlyphSlice(TextLayoutGlyph[] glyphs, int start, int count)
        {
            _glyphs = glyphs;
            _start = start;
            Count = count;
        }

        public int Count { get; }

        public TextLayoutGlyph this[int index] => _glyphs[_start + index];

        public IEnumerator<TextLayoutGlyph> GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
            {
                yield return _glyphs[_start + i];
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            => GetEnumerator();
    }

    private sealed class ParagraphFormattingResult
    {
        public required IntrinsicParagraphMeasurement Measurement { get; init; }
        public required IReadOnlyList<PositionedLine> VisibleLines { get; init; }
        public required double MeasuredWidth { get; init; }
        public required double MeasuredHeight { get; init; }
    }

    private sealed class IntrinsicParagraphMeasurement : IDisposable
    {
        public required IReadOnlyList<TextLayoutIssue> Issues { get; init; }
        public required IReadOnlyList<NormalizedSegment> Segments { get; init; }
        public required OwnedPooledBuffer<TokenDescriptor> Tokens { get; init; }
        public required OwnedPooledBuffer<TextLayoutGlyph> Glyphs { get; init; }
        public required OwnedPooledBuffer<LineDescriptor> AllLines { get; init; }
        public required double MeasuredWidth { get; init; }
        public required double MeasuredHeight { get; init; }

        public void Dispose()
        {
            Tokens.Dispose();
            Glyphs.Dispose();
            AllLines.Dispose();
        }
    }

    private sealed class ShapedFlatContent : IDisposable
    {
        public required IReadOnlyList<NormalizedSegment> Segments { get; init; }
        public required OwnedPooledBuffer<TokenDescriptor> Tokens { get; init; }
        public required OwnedPooledBuffer<TextLayoutGlyph> Glyphs { get; init; }

        public void Dispose()
        {
            Tokens.Dispose();
            Glyphs.Dispose();
        }
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

    private readonly record struct TokenMetrics(
        double? ExplicitLineHeight,
        TextLineBoxSizing LineBoxSizing,
        double Ascent,
        double Descent,
        double LineGap);

    private readonly record struct TokenSlice(PreparedTokenKind Kind, int Start, int Length)
    {
        public int End => Start + Length;
    }

    private sealed class OwnedPooledBuffer<T> : IReadOnlyList<T>, IDisposable
    {
        private T[]? _buffer;

        public OwnedPooledBuffer(T[] buffer, int count)
        {
            _buffer = buffer;
            Count = count;
        }

        public int Count { get; }

        public ReadOnlySpan<T> Span
            => (_buffer ?? System.Array.Empty<T>()).AsSpan(0, Count);

        public T this[int index] => (_buffer ?? System.Array.Empty<T>())[index];

        public T[] Array
            => _buffer ?? System.Array.Empty<T>();

        public IEnumerator<T> GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
            {
                yield return this[i];
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            => GetEnumerator();

        public T[] DetachArray()
        {
            var buffer = _buffer ?? System.Array.Empty<T>();
            _buffer = null;
            return buffer;
        }

        public void Dispose()
        {
            var buffer = _buffer;
            _buffer = null;
            if (buffer is not null && buffer.Length > 0)
            {
                ArrayPool<T>.Shared.Return(buffer, RuntimeHelpers.IsReferenceOrContainsReferences<T>());
            }
        }
    }

    private sealed class PooledBufferBuilder<T> : IDisposable
    {
        private T[] _buffer;

        public PooledBufferBuilder(int initialCapacity = 16)
        {
            _buffer = ArrayPool<T>.Shared.Rent(initialCapacity);
        }

        public int Count { get; private set; }

        public ReadOnlySpan<T> WrittenSpan => _buffer.AsSpan(0, Count);

        public void Add(T value)
        {
            if (Count == _buffer.Length)
            {
                Grow();
            }

            _buffer[Count++] = value;
        }

        public void Clear()
        {
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                Array.Clear(_buffer, 0, Count);
            }

            Count = 0;
        }

        public T[] ToArray()
        {
            var copy = new T[Count];
            Array.Copy(_buffer, 0, copy, 0, Count);
            return copy;
        }

        public OwnedPooledBuffer<T> MoveToOwnedBuffer()
        {
            var buffer = _buffer;
            var count = Count;
            _buffer = Array.Empty<T>();
            Count = 0;
            return new OwnedPooledBuffer<T>(buffer, count);
        }

        public void Dispose()
        {
            var buffer = _buffer;
            _buffer = Array.Empty<T>();
            Count = 0;
            if (buffer.Length > 0)
            {
                ArrayPool<T>.Shared.Return(buffer, RuntimeHelpers.IsReferenceOrContainsReferences<T>());
            }
        }

        private void Grow()
        {
            var next = ArrayPool<T>.Shared.Rent(_buffer.Length * 2);
            Array.Copy(_buffer, 0, next, 0, Count);
            ArrayPool<T>.Shared.Return(_buffer, RuntimeHelpers.IsReferenceOrContainsReferences<T>());
            _buffer = next;
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
            public required ShapedFlatContent Content { get; init; }
        }

        private sealed class BreakOpportunityPlan
        {
            public required ShapedFlatContent Content { get; init; }
        }

        private sealed class ParagraphDecorationPlan
        {
            public static readonly ParagraphDecorationPlan Empty = new();
        }

        public static ParagraphFormattingResult Format(
            TextBoxLayoutRequest request,
            TextLayoutFontCache fonts,
            List<TextLayoutIssue> issues,
            LayoutConstraints constraints,
            TextLayoutAnalysisContext? context)
        {
            var intrinsic = MeasureIntrinsic(request, fonts, issues, constraints.AvailableWidth, context);
            var positioned = Position(request, intrinsic, constraints);
            _ = PlanDecorations(positioned);

            return new ParagraphFormattingResult
            {
                Measurement = intrinsic,
                VisibleLines = positioned.VisibleLines,
                MeasuredWidth = intrinsic.MeasuredWidth,
                MeasuredHeight = intrinsic.MeasuredHeight
            };
        }

        private static IntrinsicParagraphMeasurement MeasureIntrinsic(
            TextBoxLayoutRequest request,
            TextLayoutFontCache fonts,
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
            var allLines = BuildLines(request, breakPlan.Content.Tokens.Span, contentWidth);
            issues.AddRange(localIssues);

            double measuredWidth = 0d;
            double measuredHeight = 0d;
            for (var i = 0; i < allLines.Count; i++)
            {
                measuredWidth = Math.Max(measuredWidth, allLines[i].MeasuredWidth);
                measuredHeight += allLines[i].Height;
            }

            var measurement = new IntrinsicParagraphMeasurement
            {
                Issues = localIssues.ToArray(),
                Segments = breakPlan.Content.Segments,
                Tokens = breakPlan.Content.Tokens,
                Glyphs = breakPlan.Content.Glyphs,
                AllLines = allLines,
                MeasuredWidth = measuredWidth,
                MeasuredHeight = measuredHeight
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

        private static ShapedParagraph Shape(TextBoxLayoutRequest request, NormalizedParagraphInput input, TextLayoutFontCache fonts, List<TextLayoutIssue> issues)
            => new()
            {
                Content = ShapeSegmentsIntoDescriptors(request with { Segments = input.Segments }, fonts, issues)
            };

        private static BreakOpportunityPlan AnalyzeBreakOpportunities(ShapedParagraph paragraph)
            => new()
            {
                Content = paragraph.Content
            };

        private static ParagraphFormattingResult Position(TextBoxLayoutRequest request, IntrinsicParagraphMeasurement intrinsic, LayoutConstraints constraints)
        {
            PositionedLine[] visibleLines;
            if (constraints.ClipToHeight && request.VerticalAlignment != TextVerticalAlignment.Top)
            {
                var allLines = CreateFullLineSelection(intrinsic.AllLines.Count);
                var positionedAllLines = PositionLines(request, intrinsic.AllLines, allLines, constraints.AvailableWidth, constraints.AvailableHeight);
                var edges = StyleResolver.Resolve(request.BoxStyle).Edges;
                visibleLines = ClipPositionedLines(edges.Insets.Top, edges.Insets.Top + constraints.AvailableHeight, intrinsic.AllLines, positionedAllLines);
            }
            else
            {
                var unclippedLines = constraints.ClipToHeight ? ClipLines(constraints.AvailableHeight, intrinsic.AllLines) : CreateFullLineSelection(intrinsic.AllLines.Count);
                visibleLines = PositionLines(request, intrinsic.AllLines, unclippedLines, constraints.AvailableWidth, constraints.AvailableHeight);
            }

            return new ParagraphFormattingResult
            {
                Measurement = intrinsic,
                VisibleLines = visibleLines,
                MeasuredWidth = intrinsic.MeasuredWidth,
                MeasuredHeight = intrinsic.MeasuredHeight
            };
        }

        private static PositionedLine[] CreateFullLineSelection(int lineCount)
        {
            var lines = new PositionedLine[lineCount];
            for (var i = 0; i < lineCount; i++)
            {
                lines[i] = new PositionedLine(i, 0d, 0d);
            }

            return lines;
        }

        private static ParagraphDecorationPlan PlanDecorations(ParagraphFormattingResult positioned)
            => ParagraphDecorationPlan.Empty;

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

    private static TextLayoutFontCache GetFontCache(TextFontLibrary library, TextLayoutAnalysisContext? context, out bool ownsFonts)
    {
        if (context is not null)
        {
            ownsFonts = false;
            return context.GetFontCache(library);
        }

        ownsFonts = true;
        return new TextLayoutFontCache(library);
    }
}
