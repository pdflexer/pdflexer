namespace PdfLexer.Content;

public sealed class DocstrumLikeStructuredTextGrouper : IStructuredTextGrouper
{
    public StructuredTextGroupingResult Group(StructuredTextGroupingInput input)
    {
        var options = input.Options;
        var contentLines = input.ContentLineCandidates != null
            ? StructuredTextGroupingHelpers.ReconcileLineWords(
                StructuredTextGroupingHelpers.CreateLines(input.ContentLineCandidates),
                options)
            : BuildInferredLines(input.Words, options);
        var contentParagraphs = StructuredTextGroupingHelpers.GroupParagraphs(contentLines, options);
        var readingColumns = BuildColumns(contentLines);
        var readingLines = new List<StructuredLine>(contentLines.Count);
        var readingParagraphs = new List<StructuredParagraph>();
        foreach (var column in readingColumns)
        {
            readingLines.AddRange(column);
            readingParagraphs.AddRange(StructuredTextGroupingHelpers.GroupParagraphs(column, options));
        }

        return StructuredTextGroupingHelpers.ToGroupingResult(contentLines, readingLines, contentParagraphs, readingParagraphs);
    }

    private static List<StructuredLine> BuildInferredLines(IReadOnlyList<StructuredWord> words, StructuredTextOptions options)
    {
        if (words.Count == 0)
        {
            return new List<StructuredLine>();
        }

        var medianFontSize = Median(words.Select(x => x.FontSize));
        var baselineTolerance = Math.Max(1d, medianFontSize * options.LineMergeMultiplier);
        var neighborGapTolerance = Math.Max(6d, medianFontSize * Math.Max(1.5d, options.LineGapMultiplier * 0.45d));
        var orderedWords = words
            .OrderByDescending(x => Math.Round(x.BaselineCoordinate, 4))
            .ThenBy(x => Math.Round(x.InlineStart, 4))
            .ThenBy(x => x.SequenceIndex)
            .ToList();
        var clusters = new List<WordLineCluster>();
        foreach (var word in orderedWords)
        {
            WordLineCluster? best = null;
            double bestScore = double.MaxValue;
            foreach (var cluster in clusters)
            {
                if (!cluster.CanAccept(word, baselineTolerance, neighborGapTolerance, options))
                {
                    continue;
                }

                var score = cluster.GetScore(word);
                if (score < bestScore)
                {
                    bestScore = score;
                    best = cluster;
                }
            }

            if (best == null)
            {
                clusters.Add(new WordLineCluster(word));
            }
            else
            {
                best.Add(word);
            }
        }

        var inferredLines = clusters
            .OrderBy(x => x.SequenceIndex)
            .Select(x => x.ToLine())
            .ToList();
        return StructuredTextGroupingHelpers.ReconcileLineWords(inferredLines, options);
    }

    private static List<List<StructuredLine>> BuildColumns(IReadOnlyList<StructuredLine> contentLines)
    {
        if (contentLines.Count <= 1)
        {
            return new List<List<StructuredLine>> { contentLines.ToList() };
        }

        var orderedByReading = StructuredTextGroupingHelpers.OrderLines(contentLines);
        var medianLineWidth = Median(orderedByReading.Select(x => x.BoundingBox.Width()));
        var medianLineHeight = Median(orderedByReading.Select(x => x.BoundingBox.Height()));
        var horizontalTolerance = Math.Max(12d, medianLineWidth * 0.18d);
        var verticalTolerance = Math.Max(6d, medianLineHeight * 1.5d);

        var columns = new List<ColumnCluster>();
        foreach (var line in orderedByReading)
        {
            var assigned = false;
            foreach (var column in columns)
            {
                if (!column.CanAccept(line, horizontalTolerance, verticalTolerance))
                {
                    continue;
                }

                column.Add(line);
                assigned = true;
                break;
            }

            if (!assigned)
            {
                columns.Add(new ColumnCluster(line));
            }
        }

        return columns
            .OrderBy(x => Math.Round(x.Left, 4))
            .ThenByDescending(x => Math.Round(x.Top, 4))
            .Select(x => x.GetReadingLines())
            .ToList();
    }

    private static double Median(IEnumerable<double> values)
    {
        var ordered = values
            .Where(x => x > 0d)
            .OrderBy(x => x)
            .ToList();
        if (ordered.Count == 0)
        {
            return 0d;
        }

        var middle = ordered.Count / 2;
        if (ordered.Count % 2 == 0)
        {
            return (ordered[middle - 1] + ordered[middle]) / 2d;
        }

        return ordered[middle];
    }

    private sealed class ColumnCluster
    {
        private readonly List<StructuredLine> lines = new();

        public ColumnCluster(StructuredLine first)
        {
            Add(first);
        }

        public double Left { get; private set; }
        public double Right { get; private set; }
        public double Top { get; private set; }
        public double Bottom { get; private set; }

        public void Add(StructuredLine line)
        {
            lines.Add(line);
            if (lines.Count == 1)
            {
                Left = line.BoundingBox.LLx;
                Right = line.BoundingBox.URx;
                Top = line.BoundingBox.URy;
                Bottom = line.BoundingBox.LLy;
                return;
            }

            Left = Math.Min(Left, line.BoundingBox.LLx);
            Right = Math.Max(Right, line.BoundingBox.URx);
            Top = Math.Max(Top, line.BoundingBox.URy);
            Bottom = Math.Min(Bottom, line.BoundingBox.LLy);
        }

        public bool CanAccept(StructuredLine line, double horizontalTolerance, double verticalTolerance)
        {
            var box = line.BoundingBox;
            var horizontalGap = box.LLx > Right
                ? box.LLx - Right
                : Left > box.URx
                    ? Left - box.URx
                    : 0d;
            if (horizontalGap > horizontalTolerance)
            {
                return false;
            }

            var overlap = Math.Min(Right, box.URx) - Math.Max(Left, box.LLx);
            if (overlap >= 0d)
            {
                return true;
            }

            var centerDelta = Math.Abs(((Left + Right) / 2d) - ((box.LLx + box.URx) / 2d));
            if (centerDelta <= horizontalTolerance)
            {
                return true;
            }

            var topDownGap = Bottom > box.URy ? Bottom - box.URy : box.LLy > Top ? box.LLy - Top : 0d;
            return topDownGap <= verticalTolerance && centerDelta <= horizontalTolerance * 1.5d;
        }

        public List<StructuredLine> GetReadingLines()
        {
            return StructuredTextGroupingHelpers.OrderLines(lines);
        }
    }

    private sealed class WordLineCluster
    {
        private readonly List<StructuredWord> words = new();

        public WordLineCluster(StructuredWord first)
        {
            Add(first);
        }

        public int SequenceIndex { get; private set; }
        private double Baseline { get; set; }
        private double Left { get; set; }
        private double Right { get; set; }
        private double Top { get; set; }
        private double Bottom { get; set; }
        private double FontSize { get; set; }
        private double Rotation { get; set; }

        public void Add(StructuredWord word)
        {
            words.Add(word);
            words.Sort(static (a, b) =>
            {
                var inline = Math.Round(a.InlineStart, 4).CompareTo(Math.Round(b.InlineStart, 4));
                return inline != 0 ? inline : a.SequenceIndex.CompareTo(b.SequenceIndex);
            });

            SequenceIndex = words.Min(x => x.SequenceIndex);
            Baseline = words.Average(x => x.BaselineCoordinate);
            Left = words.Min(x => x.BoundingBox.LLx);
            Right = words.Max(x => x.BoundingBox.URx);
            Top = words.Max(x => x.BoundingBox.URy);
            Bottom = words.Min(x => x.BoundingBox.LLy);
            FontSize = words.Average(x => x.FontSize);
            Rotation = words[0].Rotation;
        }

        public bool CanAccept(StructuredWord word, double baselineTolerance, double neighborGapTolerance, StructuredTextOptions options)
        {
            if (Math.Abs(word.Rotation - Rotation) > options.RotationToleranceDegrees)
            {
                return false;
            }

            if (Math.Abs(word.BaselineCoordinate - Baseline) > baselineTolerance)
            {
                return false;
            }

            var box = word.BoundingBox;
            var horizontalGap = box.LLx > Right
                ? box.LLx - Right
                : Left > box.URx
                    ? Left - box.URx
                    : 0d;
            return horizontalGap <= neighborGapTolerance;
        }

        public double GetScore(StructuredWord word)
        {
            var baselineDelta = Math.Abs(word.BaselineCoordinate - Baseline);
            var horizontalGap = word.BoundingBox.LLx > Right
                ? word.BoundingBox.LLx - Right
                : Left > word.BoundingBox.URx
                    ? Left - word.BoundingBox.URx
                    : 0d;
            return baselineDelta + (horizontalGap * 0.5d);
        }

        public StructuredLine ToLine()
        {
            return new StructuredLine(words, words[0].PageSpace);
        }
    }
}
