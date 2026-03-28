namespace PdfLexer.Content;

public sealed class HeuristicStructuredTextGrouper : IStructuredTextGrouper
{
    public StructuredTextGroupingResult Group(StructuredTextGroupingInput input)
    {
        var options = input.Options;
        var contentLines = input.ContentLineCandidates != null
            ? StructuredTextGroupingHelpers.ReconcileLineWords(
                StructuredTextGroupingHelpers.CreateLines(input.ContentLineCandidates),
                options)
            : StructuredTextGroupingHelpers.ReconcileLineWords(
                StructuredTextGroupingHelpers.GroupLines(input.Words, options),
                options);
        var contentParagraphs = StructuredTextGroupingHelpers.GroupParagraphs(contentLines, options);
        var (readingLines, readingParagraphs) = StructuredTextGroupingHelpers.BuildReadingStructure(contentLines, options);
        return StructuredTextGroupingHelpers.ToGroupingResult(contentLines, readingLines, contentParagraphs, readingParagraphs);
    }
}

public sealed class ContentOrderStructuredTextGrouper : IStructuredTextGrouper
{
    public StructuredTextGroupingResult Group(StructuredTextGroupingInput input)
    {
        var options = input.Options;
        var contentLines = input.ContentLineCandidates != null
            ? StructuredTextGroupingHelpers.ReconcileLineWords(
                StructuredTextGroupingHelpers.CreateLines(input.ContentLineCandidates),
                options)
            : StructuredTextGroupingHelpers.ReconcileLineWords(
                StructuredTextGroupingHelpers.GroupLines(input.Words, options),
                options);
        var contentParagraphs = StructuredTextGroupingHelpers.GroupParagraphs(contentLines, options);
        return StructuredTextGroupingHelpers.ToGroupingResult(contentLines, contentLines, contentParagraphs, contentParagraphs);
    }
}

internal static class StructuredTextGroupingHelpers
{
    public static StructuredTextGroupingResult ToGroupingResult(
        IReadOnlyList<StructuredLine> contentLines,
        IReadOnlyList<StructuredLine> readingLines,
        IReadOnlyList<StructuredParagraph> contentParagraphs,
        IReadOnlyList<StructuredParagraph> readingParagraphs)
    {
        return new StructuredTextGroupingResult
        {
            ContentLines = contentLines.Select(x => (IReadOnlyList<StructuredWord>)x.Words).ToList(),
            ReadingLines = readingLines.Select(x => (IReadOnlyList<StructuredWord>)x.Words).ToList(),
            ContentParagraphs = contentParagraphs
                .Select(x => x.Lines.Select(y => (IReadOnlyList<StructuredWord>)y.Words).ToList() as IReadOnlyList<IReadOnlyList<StructuredWord>>)
                .ToList(),
            ReadingParagraphs = readingParagraphs
                .Select(x => x.Lines.Select(y => (IReadOnlyList<StructuredWord>)y.Words).ToList() as IReadOnlyList<IReadOnlyList<StructuredWord>>)
                .ToList()
        };
    }

    public static List<StructuredLine> CreateLines(IReadOnlyList<IReadOnlyList<StructuredWord>> wordGroups)
    {
        var lines = new List<StructuredLine>(wordGroups.Count);
        foreach (var wordGroup in wordGroups)
        {
            if (wordGroup.Count == 0)
            {
                continue;
            }

            lines.Add(new StructuredLine(wordGroup, wordGroup[0].PageSpace));
        }

        return lines;
    }

    public static List<StructuredLine> GroupLines(IReadOnlyList<StructuredWord> words, StructuredTextOptions options)
    {
        var lines = new List<StructuredLine>();
        var current = new List<StructuredWord>();
        StructuredWord? previous = null;
        foreach (var word in words)
        {
            if (current.Count > 0 && previous != null && ShouldBreakLine(previous, word, options))
            {
                lines.Add(new StructuredLine(current, current[0].PageSpace));
                current = new List<StructuredWord>();
            }

            current.Add(word);
            previous = word;
        }

        if (current.Count > 0)
        {
            lines.Add(new StructuredLine(current, current[0].PageSpace));
        }

        return lines;
    }

    public static List<StructuredLine> OrderLines(IReadOnlyList<StructuredLine> lines)
    {
        return lines
            .OrderByDescending(x => Math.Round(x.BaselineCoordinate, 4))
            .ThenBy(x => Math.Round(x.InlineStart, 4))
            .ThenBy(x => x.SequenceIndex)
            .ToList();
    }

    public static List<StructuredLine> ReconcileLineWords(IReadOnlyList<StructuredLine> lines, StructuredTextOptions options)
    {
        var reconciled = new List<StructuredLine>(lines.Count);
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

            reconciled.Add(new StructuredLine(visualWords, visualWords[0].PageSpace));
        }

        return reconciled;
    }

    public static List<StructuredParagraph> GroupParagraphs(IReadOnlyList<StructuredLine> lines, StructuredTextOptions options)
    {
        var paragraphs = new List<StructuredParagraph>();
        var current = new List<StructuredLine>();
        StructuredLine? previous = null;
        foreach (var line in lines)
        {
            if (current.Count > 0 && previous != null && ShouldBreakParagraph(previous, line, options))
            {
                paragraphs.Add(new StructuredParagraph(current, current[0].PageSpace));
                current = new List<StructuredLine>();
            }

            current.Add(line);
            previous = line;
        }

        if (current.Count > 0)
        {
            paragraphs.Add(new StructuredParagraph(current, current[0].PageSpace));
        }

        return paragraphs;
    }

    public static (List<StructuredLine> ReadingLines, List<StructuredParagraph> ReadingParagraphs) BuildReadingStructure(
        IReadOnlyList<StructuredLine> contentLines,
        StructuredTextOptions options)
    {
        var readingLines = OrderLines(contentLines);
        var readingParagraphs = GroupParagraphs(readingLines, options);
        return (readingLines, readingParagraphs);
    }

    private static bool ShouldBreakLine(StructuredWord previous, StructuredWord current, StructuredTextOptions options)
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

    private static List<StructuredWord> CreateVisualWords(IReadOnlyList<StructuredWord> fragments, StructuredTextOptions options)
    {
        var words = new List<StructuredWord>();
        var currentCharacters = new List<StructuredCharacter>();
        var currentHasExplicitBreakBefore = fragments[0].HasExplicitBreakBefore;
        StructuredCharacter? previous = null;
        StructuredWord? previousFragment = null;

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

    private static bool ShouldPreserveFragmentBoundary(StructuredWord previous, StructuredWord current, StructuredTextOptions options)
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

    private static bool ShouldBreakVisualWord(StructuredCharacter previous, StructuredCharacter current, StructuredTextOptions options)
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

    private static bool ShouldBreakParagraph(StructuredLine previous, StructuredLine current, StructuredTextOptions options)
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

    private static bool SameRotation(double previous, double current, StructuredTextOptions options)
    {
        var delta = Math.Abs(previous - current);
        delta = Math.Min(delta, 360d - delta);
        return delta <= options.RotationToleranceDegrees;
    }

    private static void FlushWord(List<StructuredCharacter> characters, List<StructuredWord> words, bool hasExplicitBreakBefore)
    {
        if (characters.Count == 0)
        {
            return;
        }

        words.Add(new StructuredWord(characters, characters[0].PageSpace, hasExplicitBreakBefore));
        characters.Clear();
    }
}
