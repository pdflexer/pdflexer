namespace PdfLexer.Content;

public sealed class HeuristicSemanticTextGrouper : ISemanticTextGrouper
{
    public SemanticTextGroupingResult Group(SemanticTextGroupingInput input)
    {
        var options = input.Options;
        var contentLines = input.ContentLineCandidates != null
            ? SemanticTextGroupingHelpers.ReconcileLineWords(
                SemanticTextGroupingHelpers.CreateLines(input.ContentLineCandidates),
                options)
            : SemanticTextGroupingHelpers.ReconcileLineWords(
                SemanticTextGroupingHelpers.GroupLines(input.Words, options),
                options);
        var contentParagraphs = SemanticTextGroupingHelpers.GroupParagraphs(contentLines, options);
        var (readingLines, readingParagraphs) = SemanticTextGroupingHelpers.BuildReadingStructure(contentLines, options);
        return SemanticTextGroupingHelpers.ToGroupingResult(contentLines, readingLines, contentParagraphs, readingParagraphs);
    }
}

public sealed class ContentOrderSemanticTextGrouper : ISemanticTextGrouper
{
    public SemanticTextGroupingResult Group(SemanticTextGroupingInput input)
    {
        var options = input.Options;
        var contentLines = input.ContentLineCandidates != null
            ? SemanticTextGroupingHelpers.ReconcileLineWords(
                SemanticTextGroupingHelpers.CreateLines(input.ContentLineCandidates),
                options)
            : SemanticTextGroupingHelpers.ReconcileLineWords(
                SemanticTextGroupingHelpers.GroupLines(input.Words, options),
                options);
        var contentParagraphs = SemanticTextGroupingHelpers.GroupParagraphs(contentLines, options);
        return SemanticTextGroupingHelpers.ToGroupingResult(contentLines, contentLines, contentParagraphs, contentParagraphs);
    }
}

internal static class SemanticTextGroupingHelpers
{
    public static SemanticTextGroupingResult ToGroupingResult(
        IReadOnlyList<SemanticLine> contentLines,
        IReadOnlyList<SemanticLine> readingLines,
        IReadOnlyList<SemanticParagraph> contentParagraphs,
        IReadOnlyList<SemanticParagraph> readingParagraphs)
    {
        return new SemanticTextGroupingResult
        {
            ContentLines = contentLines.Select(x => (IReadOnlyList<SemanticWord>)x.Words).ToList(),
            ReadingLines = readingLines.Select(x => (IReadOnlyList<SemanticWord>)x.Words).ToList(),
            ContentParagraphs = contentParagraphs
                .Select(x => x.Lines.Select(y => (IReadOnlyList<SemanticWord>)y.Words).ToList() as IReadOnlyList<IReadOnlyList<SemanticWord>>)
                .ToList(),
            ReadingParagraphs = readingParagraphs
                .Select(x => x.Lines.Select(y => (IReadOnlyList<SemanticWord>)y.Words).ToList() as IReadOnlyList<IReadOnlyList<SemanticWord>>)
                .ToList()
        };
    }

    public static List<SemanticLine> CreateLines(IReadOnlyList<IReadOnlyList<SemanticWord>> wordGroups)
    {
        var lines = new List<SemanticLine>(wordGroups.Count);
        foreach (var wordGroup in wordGroups)
        {
            if (wordGroup.Count == 0)
            {
                continue;
            }

            lines.Add(new SemanticLine(wordGroup, wordGroup[0].PageSpace));
        }

        return lines;
    }

    public static List<SemanticLine> GroupLines(IReadOnlyList<SemanticWord> words, SemanticExtractOptions options)
    {
        var lines = new List<SemanticLine>();
        var current = new List<SemanticWord>();
        SemanticWord? previous = null;
        foreach (var word in words)
        {
            if (current.Count > 0 && previous != null && ShouldBreakLine(previous, word, options))
            {
                lines.Add(new SemanticLine(current, current[0].PageSpace));
                current = new List<SemanticWord>();
            }

            current.Add(word);
            previous = word;
        }

        if (current.Count > 0)
        {
            lines.Add(new SemanticLine(current, current[0].PageSpace));
        }

        return lines;
    }

    public static List<SemanticLine> OrderLines(IReadOnlyList<SemanticLine> lines)
    {
        return lines
            .OrderByDescending(x => Math.Round(x.BaselineCoordinate, 4))
            .ThenBy(x => Math.Round(x.InlineStart, 4))
            .ThenBy(x => x.SequenceIndex)
            .ToList();
    }

    public static List<SemanticLine> ReconcileLineWords(IReadOnlyList<SemanticLine> lines, SemanticExtractOptions options)
    {
        var reconciled = new List<SemanticLine>(lines.Count);
        foreach (var line in lines)
        {
            var ordered = line.Words
                .OrderBy(x => Math.Round(x.InlineStart, 4))
                .ThenBy(x => x.SequenceIndex)
                .ToList();
            if (ordered.Count == 0)
            {
                continue;
            }

            var visualWords = CreateVisualWords(ordered, options);
            if (visualWords.Count == 0)
            {
                continue;
            }

            reconciled.Add(new SemanticLine(visualWords, visualWords[0].PageSpace));
        }

        return reconciled;
    }

    public static List<SemanticParagraph> GroupParagraphs(IReadOnlyList<SemanticLine> lines, SemanticExtractOptions options)
    {
        var paragraphs = new List<SemanticParagraph>();
        var current = new List<SemanticLine>();
        SemanticLine? previous = null;
        foreach (var line in lines)
        {
            if (current.Count > 0 && previous != null && ShouldBreakParagraph(previous, line, options))
            {
                paragraphs.Add(new SemanticParagraph(current, current[0].PageSpace));
                current = new List<SemanticLine>();
            }

            current.Add(line);
            previous = line;
        }

        if (current.Count > 0)
        {
            paragraphs.Add(new SemanticParagraph(current, current[0].PageSpace));
        }

        return paragraphs;
    }

    public static (List<SemanticLine> ReadingLines, List<SemanticParagraph> ReadingParagraphs) BuildReadingStructure(
        IReadOnlyList<SemanticLine> contentLines,
        SemanticExtractOptions options)
    {
        var readingLines = OrderLines(contentLines);
        var readingParagraphs = GroupParagraphs(readingLines, options);
        return (readingLines, readingParagraphs);
    }

    private static bool ShouldBreakLine(SemanticWord previous, SemanticWord current, SemanticExtractOptions options)
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
        var maxFont = Math.Max(previous.FontSize, current.FontSize);
        return gap > (maxFont * options.LineGapMultiplier);
    }

    private static List<SemanticWord> CreateVisualWords(IReadOnlyList<SemanticWord> fragments, SemanticExtractOptions options)
    {
        var words = new List<SemanticWord>();
        var currentCharacters = new List<SemanticCharacter>();
        var currentHasExplicitBreakBefore = fragments[0].HasExplicitBreakBefore;
        SemanticCharacter? previous = null;
        SemanticWord? previousFragment = null;

        foreach (var fragment in fragments)
        {
            if (currentCharacters.Count > 0 &&
                (fragment.HasExplicitBreakBefore || (previousFragment != null && ShouldPreserveFragmentBoundary(previousFragment, fragment, options))))
            {
                FlushWord(currentCharacters, words, currentHasExplicitBreakBefore);
                currentHasExplicitBreakBefore = fragment.HasExplicitBreakBefore;
                previous = null;
            }

            foreach (var character in fragment.Characters
                         .OrderBy(x => Math.Round(x.InlineStart, 4))
                         .ThenBy(x => x.SequenceIndex))
            {
                if (currentCharacters.Count > 0 && previous != null && ShouldBreakVisualWord(previous, character, options))
                {
                    FlushWord(currentCharacters, words, currentHasExplicitBreakBefore);
                    currentHasExplicitBreakBefore = false;
                    previous = null;
                }

                currentCharacters.Add(character);
                previous = character;
            }

            previousFragment = fragment;
        }

        FlushWord(currentCharacters, words, currentHasExplicitBreakBefore);
        return words;
    }

    private static bool ShouldPreserveFragmentBoundary(SemanticWord previous, SemanticWord current, SemanticExtractOptions options)
    {
        if (!SameRotation(previous.Rotation, current.Rotation, options))
        {
            return true;
        }

        var baselineTolerance = Math.Max(1d, Math.Max(previous.FontSize, current.FontSize) * options.DuplicateBaselineMultiplier);
        if (Math.Abs(previous.BaselineCoordinate - current.BaselineCoordinate) > baselineTolerance)
        {
            return false;
        }

        return GetOverlapRatio(previous.BoundingBox, current.BoundingBox) >= options.DuplicateOverlapThreshold;
    }

    private static bool ShouldBreakVisualWord(SemanticCharacter previous, SemanticCharacter current, SemanticExtractOptions options)
    {
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
        if (gap <= 0d)
        {
            return false;
        }

        var previousWidth = Math.Max(1d, previous.BoundingBox.Width());
        var currentWidth = Math.Max(1d, current.BoundingBox.Width());
        var widthTolerance = Math.Max(previousWidth, currentWidth) * 0.35d;
        var fontTolerance = Math.Max(previous.FontSize, current.FontSize) * 0.18d;
        var gapTolerance = Math.Max(1d, Math.Max(widthTolerance, fontTolerance));
        return gap > gapTolerance;
    }

    private static bool ShouldBreakParagraph(SemanticLine previous, SemanticLine current, SemanticExtractOptions options)
    {
        if (!SameRotation(previous.Rotation, current.Rotation, options))
        {
            return true;
        }

        var lineGap = Math.Abs(previous.BaselineCoordinate - current.BaselineCoordinate);
        var spacingTolerance = Math.Max(previous.FontSize, current.FontSize) * options.ParagraphSpacingMultiplier;
        if (lineGap > spacingTolerance)
        {
            return true;
        }

        var indentDelta = Math.Abs(previous.InlineStart - current.InlineStart);
        return indentDelta > (Math.Max(previous.FontSize, current.FontSize) * options.ParagraphIndentMultiplier);
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
