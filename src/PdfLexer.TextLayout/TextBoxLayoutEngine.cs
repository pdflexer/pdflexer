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

        var issues = new List<TextLayoutIssue>();
        using var fonts = new FontCache(request.FontLibrary);

        var preparedTokens = PrepareTokens(request, fonts, issues);
        if (issues.Any(x => x.Kind is TextLayoutIssueKind.MissingFamily or TextLayoutIssueKind.MissingWeight or TextLayoutIssueKind.MissingGlyph)
            && request.MissingFontBehavior == TextResolutionBehavior.Error)
        {
            return CreateResult(request, Array.Empty<PreparedLine>(), issues, TextLayoutStatus.Error, 0, 0);
        }

        var lines = BuildLines(request, preparedTokens, issues);
        var totalMeasuredHeight = lines.Sum(x => x.Height);
        var fitsHeight = totalMeasuredHeight <= request.Height + 0.0001d;
        var fitsWidth = lines.All(x => x.MeasuredWidth <= request.Width + 0.0001d);

        var status = issues.Any(x => x.Kind is TextLayoutIssueKind.MissingFamily or TextLayoutIssueKind.MissingWeight or TextLayoutIssueKind.MissingGlyph)
            ? TextLayoutStatus.Error
            : (!fitsWidth || !fitsHeight ? TextLayoutStatus.Overflow : TextLayoutStatus.Success);

        if (status == TextLayoutStatus.Overflow && request.OverflowMode == TextOverflowMode.Error)
        {
            issues.Add(new TextLayoutIssue(TextLayoutIssueKind.Overflow, "Text content exceeds the target text box."));
            status = TextLayoutStatus.Error;
        }

        var renderedLines = request.OverflowMode == TextOverflowMode.Clip
            ? ClipLines(request, lines)
            : lines;

        if ((!fitsWidth || !fitsHeight) && status != TextLayoutStatus.Error)
        {
            issues.Add(new TextLayoutIssue(TextLayoutIssueKind.Overflow, "Text content exceeds the target text box."));
        }

        PositionLines(request, renderedLines);
        var measuredWidth = lines.Count == 0 ? 0d : lines.Max(x => x.MeasuredWidth);
        return CreateResult(request, renderedLines, issues, status, measuredWidth, totalMeasuredHeight);
    }

    private static List<PreparedToken> PrepareTokens(TextBoxLayoutRequest request, FontCache fonts, List<TextLayoutIssue> issues)
    {
        var tokens = new List<PreparedToken>();
        for (var segmentIndex = 0; segmentIndex < request.Segments.Count; segmentIndex++)
        {
            var segment = request.Segments[segmentIndex];
            foreach (var token in Tokenize(segment.Text))
            {
                if (token.Kind == PreparedTokenKind.NewLine)
                {
                    tokens.Add(new PreparedToken(segmentIndex, token.Text, segment.Style, token.Kind, null, Array.Empty<TextLayoutGlyph>(), 0, 0, GetLineHeight(segment.Style)));
                    continue;
                }

                var shaped = ShapeToken(request, fonts, issues, segmentIndex, token.Text, segment.Style, token.Kind == PreparedTokenKind.Whitespace);
                tokens.Add(shaped);
            }
        }

        return tokens;
    }

    private static PreparedToken ShapeToken(
        TextBoxLayoutRequest request,
        FontCache fonts,
        List<TextLayoutIssue> issues,
        int segmentIndex,
        string text,
        TextSegmentStyle style,
        bool isWhitespace)
    {
        if (!TryResolveFace(request, style, fonts, out var face, out var faceError))
        {
            issues.Add(faceError!);
            return new PreparedToken(segmentIndex, text, style, isWhitespace ? PreparedTokenKind.Whitespace : PreparedTokenKind.Text, null, Array.Empty<TextLayoutGlyph>(), 0, 0, GetLineHeight(style));
        }

        if (!TryShapeWithFallback(request, fonts, style, text, face!, out face, out var glyphs, out var missingGlyph))
        {
            issues.Add(new TextLayoutIssue(TextLayoutIssueKind.MissingGlyph, missingGlyph ?? $"Unable to shape segment '{text}'.", segmentIndex, style.FamilyName, style.Weight, face?.FaceId));
            return new PreparedToken(segmentIndex, text, style, isWhitespace ? PreparedTokenKind.Whitespace : PreparedTokenKind.Text, face, Array.Empty<TextLayoutGlyph>(), 0, 0, GetLineHeight(style));
        }

        var width = glyphs.Sum(x => x.Advance);
        return new PreparedToken(segmentIndex, text, style, isWhitespace ? PreparedTokenKind.Whitespace : PreparedTokenKind.Text, face, glyphs, width, width, GetLineHeight(style));
    }

    private static bool TryResolveFace(
        TextBoxLayoutRequest request,
        TextSegmentStyle style,
        FontCache fonts,
        out TextFontFace? face,
        out TextLayoutIssue? issue)
    {
        issue = null;
        if (request.FontLibrary.TryResolve(style.FamilyName, style.Weight, out face))
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
                    if (request.FontLibrary.TryResolve(fallback, style.Weight, out face))
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
                if (request.FontLibrary.TryResolve(fallback, style.Weight, out face))
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
        string text,
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
                if (request.FontLibrary.TryResolve(fallback, style.Weight, out var face))
                {
                    candidates.Add(face!);
                }
            }
        }

        foreach (var candidate in candidates)
        {
            var shaped = Shape(fonts.Get(candidate), candidate, text, style);
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
            glyphs = Shape(fonts.Get(candidates[0]), candidates[0], text, style);
            issue = $"No resolved font face could shape one or more glyphs for family '{style.FamilyName}' weight '{style.Weight}'.";
            return false;
        }

        issue = $"No font face was available for family '{style.FamilyName}' weight '{style.Weight}'.";
        return false;
    }

    private static IReadOnlyList<TextLayoutGlyph> Shape(HarfRustFont font, TextFontFace face, string text, TextSegmentStyle style)
    {
        if (string.IsNullOrEmpty(text))
        {
            return Array.Empty<TextLayoutGlyph>();
        }

        using var buffer = new HarfRustBuffer();
        buffer.AddString(text);
        buffer.GuessSegmentProperties();
        using var result = font.Shape(buffer);

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

    private static double GetAdditionalAdvance(string text, uint cluster, int glyphIndex, int glyphCount, TextSegmentStyle style)
    {
        var additional = glyphIndex + 1 < glyphCount ? style.CharacterSpacing : 0;
        if (cluster < text.Length && text[(int)cluster] == ' ')
        {
            additional += style.WordSpacing;
        }

        return additional;
    }

    private static List<PreparedLine> BuildLines(TextBoxLayoutRequest request, List<PreparedToken> tokens, List<TextLayoutIssue> issues)
    {
        var lines = new List<PreparedLine>();
        var current = new PreparedLine();

        foreach (var token in tokens)
        {
            if (token.Kind == PreparedTokenKind.NewLine)
            {
                FinalizeLine(request, lines, current);
                current = new PreparedLine();
                continue;
            }

            var wouldOverflow = current.Tokens.Count > 0 && current.Width + token.Width > request.Width;
            if (wouldOverflow)
            {
                FinalizeLine(request, lines, current);
                current = new PreparedLine();
            }

            current.Tokens.Add(token);
            current.Width += token.Width;
            current.Height = Math.Max(current.Height, token.LineHeight);
            current.BaselineOffset = Math.Max(current.BaselineOffset, token.Style.FontSize);
        }

        FinalizeLine(request, lines, current);
        return lines;
    }

    private static void FinalizeLine(TextBoxLayoutRequest request, List<PreparedLine> lines, PreparedLine current)
    {
        if (current.Tokens.Count == 0 && lines.Count == 0)
        {
            return;
        }

        if (current.Tokens.Count == 0)
        {
            current.Height = 0;
            current.BaselineOffset = 0;
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

    private static List<PreparedLine> ClipLines(TextBoxLayoutRequest request, List<PreparedLine> lines)
    {
        var clipped = new List<PreparedLine>();
        var height = 0d;
        foreach (var line in lines)
        {
            var next = height + line.Height;
            if (clipped.Count > 0 && next > request.Height + 0.0001d)
            {
                break;
            }

            clipped.Add(line);
            height = next;
        }

        return clipped;
    }

    private static void PositionLines(TextBoxLayoutRequest request, List<PreparedLine> lines)
    {
        var renderedHeight = lines.Sum(x => x.Height);
        var topOffset = request.VerticalAlignment switch
        {
            TextVerticalAlignment.Center when renderedHeight < request.Height => (request.Height - renderedHeight) / 2d,
            TextVerticalAlignment.Bottom when renderedHeight < request.Height => request.Height - renderedHeight,
            _ => 0d
        };

        double y = topOffset;
        foreach (var line in lines)
        {
            line.X = request.HorizontalAlignment switch
            {
                TextHorizontalAlignment.Center when line.MeasuredWidth < request.Width => (request.Width - line.MeasuredWidth) / 2d,
                TextHorizontalAlignment.Right when line.MeasuredWidth < request.Width => request.Width - line.MeasuredWidth,
                _ => 0d
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
        var lines = renderedLines
            .Select((line, index) => new TextLayoutLine(
                index,
                line.X,
                line.BaselineY,
                line.Width,
                line.MeasuredWidth,
                line.Height,
                line.BaselineOffset,
                line.Tokens.Select(token => new TextLayoutRun(
                    token.SegmentIndex,
                    token.Text,
                    token.Face?.FaceId ?? string.Empty,
                    token.Style.FamilyName,
                    token.Style.Weight,
                    token.Style.FontSize,
                    token.Style.Underline,
                    token.Style.CharacterSpacing,
                    token.Style.WordSpacing,
                    token.X,
                    token.BaselineY,
                    token.Width,
                    token.MeasuredWidth,
                    token.LineHeight,
                    token.Glyphs)).ToArray()))
            .ToArray();

        var renderedWidth = lines.Length == 0 ? 0d : lines.Max(x => x.MeasuredWidth);
        var renderedHeight = lines.Sum(x => x.Height);
        return new TextBoxLayoutResult
        {
            Status = status,
            FitsWidth = measuredWidth <= request.Width + 0.0001d,
            FitsHeight = measuredHeight <= request.Height + 0.0001d,
            MeasuredWidth = measuredWidth,
            MeasuredHeight = measuredHeight,
            RenderedWidth = renderedWidth,
            RenderedHeight = renderedHeight,
            Lines = lines,
            Issues = issues.ToArray()
        };
    }

    private static IEnumerable<(PreparedTokenKind Kind, string Text)> Tokenize(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            yield break;
        }

        var chars = new List<char>();
        PreparedTokenKind? kind = null;
        foreach (var ch in text)
        {
            if (ch == '\r')
            {
                continue;
            }

            if (ch == '\n')
            {
                if (chars.Count > 0 && kind != null)
                {
                    yield return (kind.Value, new string(chars.ToArray()));
                    chars.Clear();
                }

                yield return (PreparedTokenKind.NewLine, "\n");
                kind = null;
                continue;
            }

            var nextKind = char.IsWhiteSpace(ch) ? PreparedTokenKind.Whitespace : PreparedTokenKind.Text;
            if (kind != null && kind != nextKind)
            {
                yield return (kind.Value, new string(chars.ToArray()));
                chars.Clear();
            }

            kind = nextKind;
            chars.Add(ch);
        }

        if (chars.Count > 0 && kind != null)
        {
            yield return (kind.Value, new string(chars.ToArray()));
        }
    }

    private static double GetLineHeight(TextSegmentStyle style)
        => style.LineSpacing ?? style.FontSize;

    private enum PreparedTokenKind
    {
        Text,
        Whitespace,
        NewLine
    }

    private sealed class PreparedToken
    {
        public PreparedToken(int segmentIndex, string text, TextSegmentStyle style, PreparedTokenKind kind, TextFontFace? face, IReadOnlyList<TextLayoutGlyph> glyphs, double width, double measuredWidth, double lineHeight)
        {
            SegmentIndex = segmentIndex;
            Text = text;
            Style = style;
            Kind = kind;
            Face = face;
            Glyphs = glyphs;
            Width = width;
            MeasuredWidth = measuredWidth;
            LineHeight = lineHeight;
        }

        public int SegmentIndex { get; }
        public string Text { get; }
        public TextSegmentStyle Style { get; }
        public PreparedTokenKind Kind { get; }
        public TextFontFace? Face { get; }
        public IReadOnlyList<TextLayoutGlyph> Glyphs { get; }
        public double Width { get; }
        public double MeasuredWidth { get; }
        public double LineHeight { get; }
        public double X { get; set; }
        public double BaselineY { get; set; }
    }

    private sealed class PreparedLine
    {
        public List<PreparedToken> Tokens { get; } = new();
        public double Width { get; set; }
        public double MeasuredWidth { get; set; }
        public double Height { get; set; }
        public double BaselineOffset { get; set; }
        public double X { get; set; }
        public double BaselineY { get; set; }
    }

    private sealed class FontCache : IDisposable
    {
        private readonly Dictionary<string, HarfRustFont> _fonts = new(StringComparer.Ordinal);

        public FontCache(TextFontLibrary library)
        {
            Library = library;
        }

        public TextFontLibrary Library { get; }

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
            foreach (var font in _fonts.Values)
            {
                font.Dispose();
            }

            _fonts.Clear();
        }
    }
}
