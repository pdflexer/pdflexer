using System.Collections.ObjectModel;
using PdfLexer.DOM;

namespace PdfLexer.Content;

internal static class SemanticTextBuilder
{
    private static readonly ISemanticTextGrouper DefaultGrouper = new HeuristicSemanticTextGrouper();

    public static SemanticTextPage Build(ParsingContext ctx, PdfPage page, SemanticExtractOptions? options)
    {
        options ??= new SemanticExtractOptions();
        var grouper = options.Grouper ?? DefaultGrouper;
        var pageSpace = new SemanticPageSpace(page);
        var characters = ProjectCharacters(ctx, page, pageSpace);
        var fragments = GroupWords(characters, options);
        var rawLayout = GroupWordsIntoLayout(fragments, options, grouper);
        var rawWords = rawLayout.ContentLines.SelectMany(x => x.Words).ToList();

        return new SemanticTextPage(
            characters,
            pageSpace,
            rawWords,
            rawLayout.ContentLines,
            rawLayout.ReadingLines,
            rawLayout.ContentParagraphs,
            rawLayout.ReadingParagraphs,
            options.Order,
            options.Mode,
            () => BuildDeduplicated(rawLayout.ContentLines, options, grouper));
    }

    private static List<SemanticCharacter> ProjectCharacters(ParsingContext ctx, PdfPage page, SemanticPageSpace pageSpace)
    {
        var characters = new List<SemanticCharacter>();
        var scanner = new TextScanner(ctx, page);
        var sequenceIndex = 0;
        while (scanner.Advance())
        {
            var textRenderingMatrix = scanner.GraphicsState.Text.TextRenderingMatrix;
            var glyph = scanner.Glyph;
            var isVertical = scanner.GraphicsState.Font?.IsVertical ?? false;
            var metrics = new SemanticGlyphMetrics(glyph, isVertical);
            var snapshot = new SemanticGlyphSnapshot(textRenderingMatrix, metrics, scanner.GraphicsState.FontSize);
            var emitted = glyph.MultiChar ?? glyph.Char.ToString();
            for (var i = 0; i < emitted.Length; i++)
            {
                var sourceReference = new SemanticSourceRef(
                    scanner.Scanner.CurrentStreamId,
                    scanner.TxtOpStart,
                    scanner.TxtOpLength);
                characters.Add(new SemanticCharacter(
                    emitted[i],
                    snapshot,
                    sourceReference,
                    sequenceIndex,
                    scanner.WasNewLine && i == 0,
                    pageSpace));
                sequenceIndex++;
            }
        }

        return characters;
    }

    private static List<SemanticWord> GroupWords(IReadOnlyList<SemanticCharacter> characters, SemanticExtractOptions options)
    {
        var words = new List<SemanticWord>();
        var current = new List<SemanticCharacter>();
        SemanticCharacter? previous = null;
        var hasExplicitBreakBefore = false;
        foreach (var character in characters)
        {
            if (char.IsWhiteSpace(character.Char))
            {
                FlushWord(current, words, hasExplicitBreakBefore);
                hasExplicitBreakBefore = true;
                previous = null;
                continue;
            }

            if (current.Count > 0 && previous != null && ShouldBreakWord(previous, character, options))
            {
                FlushWord(current, words, hasExplicitBreakBefore);
                hasExplicitBreakBefore = false;
            }

            current.Add(character);
            previous = character;
        }

        FlushWord(current, words, hasExplicitBreakBefore);
        return words;
    }

    private static SemanticTextLayout GroupWordsIntoLayout(
        IReadOnlyList<SemanticWord> words,
        SemanticExtractOptions options,
        ISemanticTextGrouper grouper,
        IReadOnlyList<IReadOnlyList<SemanticWord>>? contentLineCandidates = null)
    {
        var grouped = grouper.Group(new SemanticTextGroupingInput
        {
            Words = words,
            ContentLineCandidates = contentLineCandidates,
            Options = options
        });
        return CreateLayout(grouped);
    }

    private static List<SemanticLine> DeduplicateLines(IReadOnlyList<SemanticLine> lines, SemanticExtractOptions options)
    {
        var deduplicated = new List<SemanticLine>(lines.Count);
        var duplicateIndex = new Dictionary<string, List<SemanticWord>>(StringComparer.Ordinal);
        foreach (var line in lines)
        {
            var filtered = new List<SemanticWord>(line.Words.Count);
            foreach (var word in line.Words)
            {
                if (TryGetDuplicate(word, duplicateIndex, options))
                {
                    continue;
                }

                filtered.Add(word);
                AddToDuplicateIndex(word, duplicateIndex);
            }

            if (filtered.Count > 0)
            {
                deduplicated.Add(new SemanticLine(filtered, filtered[0].PageSpace));
            }
        }

        return deduplicated;
    }

    private static SemanticTextDedupData BuildDeduplicated(
        IReadOnlyList<SemanticLine> rawContentLines,
        SemanticExtractOptions options,
        ISemanticTextGrouper grouper)
    {
        var deduplicatedContentLines = DeduplicateLines(rawContentLines, options);
        var layout = GroupWordsIntoLayout(
            deduplicatedContentLines.SelectMany(x => x.Words).ToList(),
            options,
            grouper,
            deduplicatedContentLines.Select(x => (IReadOnlyList<SemanticWord>)x.Words).ToList());

        return new SemanticTextDedupData
        {
            Words = new ReadOnlyCollection<SemanticWord>(layout.ContentLines.SelectMany(x => x.Words).ToList()),
            ContentLines = new ReadOnlyCollection<SemanticLine>(layout.ContentLines.ToList()),
            ReadingLines = new ReadOnlyCollection<SemanticLine>(layout.ReadingLines.ToList()),
            ContentParagraphs = new ReadOnlyCollection<SemanticParagraph>(layout.ContentParagraphs.ToList()),
            ReadingParagraphs = new ReadOnlyCollection<SemanticParagraph>(layout.ReadingParagraphs.ToList())
        };
    }

    private static SemanticTextLayout CreateLayout(SemanticTextGroupingResult grouped)
    {
        return new SemanticTextLayout(
            CreateLines(grouped.ContentLines, "content line"),
            CreateLines(grouped.ReadingLines, "reading line"),
            CreateParagraphs(grouped.ContentParagraphs, "content paragraph"),
            CreateParagraphs(grouped.ReadingParagraphs, "reading paragraph"));
    }

    private static List<SemanticLine> CreateLines(IReadOnlyList<IReadOnlyList<SemanticWord>> lineWordGroups, string scope)
    {
        var lines = new List<SemanticLine>(lineWordGroups.Count);
        foreach (var wordGroup in lineWordGroups)
        {
            if (wordGroup.Count == 0)
            {
                throw new InvalidOperationException($"Semantic grouper returned an empty {scope}.");
            }

            lines.Add(new SemanticLine(wordGroup, wordGroup[0].PageSpace));
        }

        return lines;
    }

    private static List<SemanticParagraph> CreateParagraphs(
        IReadOnlyList<IReadOnlyList<IReadOnlyList<SemanticWord>>> paragraphLineWordGroups,
        string scope)
    {
        var paragraphs = new List<SemanticParagraph>(paragraphLineWordGroups.Count);
        foreach (var paragraphLineGroups in paragraphLineWordGroups)
        {
            var lines = CreateLines(paragraphLineGroups, scope + " line");
            if (lines.Count == 0)
            {
                throw new InvalidOperationException($"Semantic grouper returned an empty {scope}.");
            }

            paragraphs.Add(new SemanticParagraph(lines, lines[0].PageSpace));
        }

        return paragraphs;
    }

    private static bool ShouldBreakWord(SemanticCharacter previous, SemanticCharacter current, SemanticExtractOptions options)
    {
        if (current.StartsNewLine)
        {
            return true;
        }

        if (!SameRotation(previous.Rotation, current.Rotation, options))
        {
            return true;
        }

        var baselineDelta = Math.Abs(previous.BaselineCoordinate - current.BaselineCoordinate);
        var baselineTolerance = Math.Max(1d, Math.Max(previous.FontSize, current.FontSize) * options.LineMergeMultiplier);
        if (baselineDelta > baselineTolerance)
        {
            return true;
        }

        var gap = current.InlineStart - previous.InlineEnd;
        var gapTolerance = Math.Max(1d, Math.Max(previous.Snapshot.InlineAdvance, current.Snapshot.InlineAdvance) * options.WordGapMultiplier);
        return gap > gapTolerance || gap < -(Math.Max(previous.FontSize, current.FontSize) * options.WordGapMultiplier);
    }

    private static bool TryGetDuplicate(
        SemanticWord candidate,
        IReadOnlyDictionary<string, List<SemanticWord>> duplicateIndex,
        SemanticExtractOptions options)
    {
        if (!duplicateIndex.TryGetValue(candidate.Text, out var effectiveWords))
        {
            return false;
        }

        foreach (var existing in effectiveWords)
        {
            if (!SameRotation(existing.Rotation, candidate.Rotation, options))
            {
                continue;
            }

            var baselineTolerance = Math.Max(1d, Math.Max(existing.FontSize, candidate.FontSize) * options.DuplicateBaselineMultiplier);
            if (Math.Abs(existing.BaselineCoordinate - candidate.BaselineCoordinate) > baselineTolerance)
            {
                continue;
            }

            if (GetOverlapRatio(existing.BoundingBox, candidate.BoundingBox) < options.DuplicateOverlapThreshold)
            {
                continue;
            }

            return true;
        }

        return false;
    }

    private static void AddToDuplicateIndex(SemanticWord word, Dictionary<string, List<SemanticWord>> duplicateIndex)
    {
        if (!duplicateIndex.TryGetValue(word.Text, out var bucket))
        {
            bucket = new List<SemanticWord>();
            duplicateIndex[word.Text] = bucket;
        }

        bucket.Add(word);
    }

    private static double GetOverlapRatio(PdfRect<double> a, PdfRect<double> b)
    {
        var llx = Math.Max(a.LLx, b.LLx);
        var lly = Math.Max(a.LLy, b.LLy);
        var urx = Math.Min(a.URx, b.URx);
        var ury = Math.Min(a.URy, b.URy);
        if (urx <= llx || ury <= lly)
        {
            return 0d;
        }

        var intersection = (urx - llx) * (ury - lly);
        var areaA = Math.Max(0d, a.Width() * a.Height());
        var areaB = Math.Max(0d, b.Width() * b.Height());
        var minArea = Math.Min(areaA, areaB);
        if (minArea <= 0d)
        {
            return 0d;
        }

        return intersection / minArea;
    }

    private static bool SameRotation(double previous, double current, SemanticExtractOptions options)
    {
        var delta = Math.Abs(previous - current);
        delta = Math.Min(delta, 360d - delta);
        return delta <= options.RotationToleranceDegrees;
    }

    private static void FlushWord(List<SemanticCharacter> characters, List<SemanticWord> words, bool hasExplicitBreakBefore)
    {
        if (characters.Count == 0)
        {
            return;
        }

        words.Add(new SemanticWord(characters, characters[0].PageSpace, hasExplicitBreakBefore));
        characters.Clear();
    }
}

internal sealed class SemanticTextLayout
{
    public SemanticTextLayout(
        IReadOnlyList<SemanticLine> contentLines,
        IReadOnlyList<SemanticLine> readingLines,
        IReadOnlyList<SemanticParagraph> contentParagraphs,
        IReadOnlyList<SemanticParagraph> readingParagraphs)
    {
        ContentLines = contentLines;
        ReadingLines = readingLines;
        ContentParagraphs = contentParagraphs;
        ReadingParagraphs = readingParagraphs;
    }

    public IReadOnlyList<SemanticLine> ContentLines { get; }
    public IReadOnlyList<SemanticLine> ReadingLines { get; }
    public IReadOnlyList<SemanticParagraph> ContentParagraphs { get; }
    public IReadOnlyList<SemanticParagraph> ReadingParagraphs { get; }
}
