namespace PdfLexer.Content;

public sealed class DocstrumLikeSemanticTextGrouper : ISemanticTextGrouper
{
    public SemanticTextGroupingResult Group(SemanticTextGroupingInput input)
    {
        var options = input.Options;
        var contentLines = input.ContentLineCandidates != null
            ? SemanticTextGroupingHelpers.ReconcileLineWords(
                SemanticTextGroupingHelpers.CreateLines(input.ContentLineCandidates),
                options)
            : BuildInferredLines(input.Words, options);
        var contentParagraphs = SemanticTextGroupingHelpers.GroupParagraphs(contentLines, options);
        var readingColumns = BuildColumns(contentLines);
        var readingLines = new List<SemanticLine>(contentLines.Count);
        var readingParagraphs = new List<SemanticParagraph>();
        foreach (var column in readingColumns)
        {
            readingLines.AddRange(column);
            readingParagraphs.AddRange(SemanticTextGroupingHelpers.GroupParagraphs(column, options));
        }

        return SemanticTextGroupingHelpers.ToGroupingResult(contentLines, readingLines, contentParagraphs, readingParagraphs);
    }

    private static List<SemanticLine> BuildInferredLines(IReadOnlyList<SemanticWord> words, SemanticExtractOptions options)
    {
        if (words.Count == 0)
        {
            return new List<SemanticLine>();
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
        return SemanticTextGroupingHelpers.ReconcileLineWords(inferredLines, options);
    }

    private static List<List<SemanticLine>> BuildColumns(IReadOnlyList<SemanticLine> contentLines)
    {
        if (contentLines.Count <= 1)
        {
            return new List<List<SemanticLine>> { contentLines.ToList() };
        }

        var orderedByReading = SemanticTextGroupingHelpers.OrderLines(contentLines);
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
        private readonly List<SemanticLine> lines = new();

        public ColumnCluster(SemanticLine first)
        {
            Add(first);
        }

        public double Left { get; private set; }
        public double Right { get; private set; }
        public double Top { get; private set; }
        public double Bottom { get; private set; }

        public void Add(SemanticLine line)
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

        public bool CanAccept(SemanticLine line, double horizontalTolerance, double verticalTolerance)
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

        public List<SemanticLine> GetReadingLines()
        {
            return SemanticTextGroupingHelpers.OrderLines(lines);
        }
    }

    private sealed class WordLineCluster
    {
        private readonly List<SemanticWord> words = new();

        public WordLineCluster(SemanticWord first)
        {
            Add(first);
        }

        public int SequenceIndex { get; private set; }
        private double Baseline { get; set; }
        private double Left { get; set; }
        private double Right { get; set; }
        private double Top { get; set; }
        private double Bottom { get; set; }
        private double Rotation { get; set; }
        private double AverageFontSize { get; set; }

        public void Add(SemanticWord word)
        {
            words.Add(word);
            if (words.Count == 1)
            {
                SequenceIndex = word.SequenceIndex;
                Baseline = word.BaselineCoordinate;
                Left = word.BoundingBox.LLx;
                Right = word.BoundingBox.URx;
                Top = word.BoundingBox.URy;
                Bottom = word.BoundingBox.LLy;
                Rotation = word.Rotation;
                AverageFontSize = word.FontSize;
                return;
            }

            SequenceIndex = Math.Min(SequenceIndex, word.SequenceIndex);
            Baseline = words.Average(x => x.BaselineCoordinate);
            Left = Math.Min(Left, word.BoundingBox.LLx);
            Right = Math.Max(Right, word.BoundingBox.URx);
            Top = Math.Max(Top, word.BoundingBox.URy);
            Bottom = Math.Min(Bottom, word.BoundingBox.LLy);
            AverageFontSize = words.Average(x => x.FontSize);
        }

        public bool CanAccept(SemanticWord word, double baselineTolerance, double neighborGapTolerance, SemanticExtractOptions options)
        {
            if (!SameRotation(Rotation, word.Rotation, options))
            {
                return false;
            }

            var effectiveBaselineTolerance = Math.Max(baselineTolerance, Math.Max(AverageFontSize, word.FontSize) * options.LineMergeMultiplier);
            if (Math.Abs(Baseline - word.BaselineCoordinate) > effectiveBaselineTolerance)
            {
                return false;
            }

            var horizontalGap = word.BoundingBox.LLx > Right
                ? word.BoundingBox.LLx - Right
                : Left > word.BoundingBox.URx
                    ? Left - word.BoundingBox.URx
                    : 0d;
            if (horizontalGap > neighborGapTolerance)
            {
                return false;
            }

            var centerDelta = Math.Abs((((Left + Right) / 2d)) - (((word.BoundingBox.LLx + word.BoundingBox.URx) / 2d)));
            var verticalOverlap = Math.Min(Top, word.BoundingBox.URy) - Math.Max(Bottom, word.BoundingBox.LLy);
            return verticalOverlap >= -Math.Max(2d, AverageFontSize * 0.4d) || centerDelta <= neighborGapTolerance;
        }

        public double GetScore(SemanticWord word)
        {
            var baselineDelta = Math.Abs(Baseline - word.BaselineCoordinate);
            var horizontalGap = word.BoundingBox.LLx > Right
                ? word.BoundingBox.LLx - Right
                : Left > word.BoundingBox.URx
                    ? Left - word.BoundingBox.URx
                    : 0d;
            return baselineDelta * 10d + horizontalGap;
        }

        public SemanticLine ToLine()
        {
            var ordered = words
                .OrderBy(x => Math.Round(x.InlineStart, 4))
                .ThenBy(x => x.SequenceIndex)
                .ToList();
            return new SemanticLine(ordered, ordered[0].PageSpace);
        }

        private static bool SameRotation(double previous, double current, SemanticExtractOptions options)
        {
            var delta = Math.Abs(previous - current);
            delta = Math.Min(delta, 360d - delta);
            return delta <= options.RotationToleranceDegrees;
        }
    }
}
