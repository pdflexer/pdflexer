using System.Text.Json;
using PdfLexer;
using PdfLexer.Content;
using PdfLexer.DOM;
using PdfLexer.Writing;

namespace pdflexer.PdfiumRegressionTester;

internal sealed class SemanticTextTests
{
    private const double MostlyTextualThreshold = 0.85d;
    private const double MinAverageCharsPerWord = 2.5d;
    private const double MaxAverageCharsPerWord = 12d;
    private const double MinAverageWordsPerLine = 2d;
    private const double MaxAverageWordsPerLine = 20d;
    private const double MinAverageLinesPerParagraph = 1.3d;
    private const double MaxAverageLinesPerParagraph = 40d;
    private const int MinimumWordsForReview = 40;
    private const int MinimumLinesForReview = 8;
    private const int MinimumParagraphsForReview = 4;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public SemanticTextRunResult RunOne(string pdf, string output)
    {
        Directory.CreateDirectory(output);

        var name = Path.GetFileName(pdf);
        var baseName = Path.GetFileNameWithoutExtension(name);

        using var doc = PdfDocument.Open(File.ReadAllBytes(pdf));
        using var overlayDoc = PdfDocument.Create();

        var pages = new List<SemanticPageSnapshot>(doc.Pages.Count);
        var pageNumber = 1;
        foreach (var page in doc.Pages)
        {
            var raw = page.GetSemanticExtract(doc.Context, new SemanticExtractOptions
            {
                Order = TextOrder.Reading,
                Mode = SemanticTextMode.Raw
            });
            var deduplicated = page.GetSemanticExtract(doc.Context, new SemanticExtractOptions
            {
                Order = TextOrder.Reading,
                Mode = SemanticTextMode.Deduplicated
            });

            pages.Add(CreateSnapshot(pageNumber, raw, deduplicated));
            AddOverlayPage(overlayDoc, page, raw, deduplicated);
            pageNumber++;
        }

        var review = CreateDocumentReview(pages);
        var snapshot = new SemanticDocumentSnapshot
        {
            PdfName = name,
            PageCount = pages.Count,
            Pages = pages,
            Review = review
        };

        var snapshotPath = Path.Combine(output, $"{baseName}_semantic.json");
        File.WriteAllText(snapshotPath, JsonSerializer.Serialize(snapshot, JsonOptions));

        var overlayPath = Path.Combine(output, $"{baseName}_semantic_overlay.pdf");
        overlayDoc.SaveTo(overlayPath);
        var reviewPath = Path.Combine(output, $"{baseName}_semantic_review.txt");
        WriteReviewFile(reviewPath, snapshot);
        return new SemanticTextRunResult
        {
            SnapshotPath = snapshotPath,
            OverlayPath = overlayPath,
            ReviewPath = reviewPath,
            ReviewWarnings = review.Warnings
        };
    }

    private static SemanticPageSnapshot CreateSnapshot(int pageNumber, SemanticTextPage raw, SemanticTextPage deduplicated)
    {
        var rawContentText = raw.GetText(TextOrder.Content, SemanticTextMode.Raw);
        var rawReadingText = raw.GetText(TextOrder.Reading, SemanticTextMode.Raw);
        var deduplicatedContentText = deduplicated.GetText(TextOrder.Content, SemanticTextMode.Deduplicated);
        var deduplicatedReadingText = deduplicated.GetText(TextOrder.Reading, SemanticTextMode.Deduplicated);
        var rawWords = raw.RawWords.Select(CreateWordSnapshot).ToList();
        var deduplicatedWords = deduplicated.DeduplicatedWords.Select(CreateWordSnapshot).ToList();
        var rawContentLines = raw.GetLines(TextOrder.Content, SemanticTextMode.Raw).Select(CreateLineSnapshot).ToList();
        var rawReadingLines = raw.GetLines(TextOrder.Reading, SemanticTextMode.Raw).Select(CreateLineSnapshot).ToList();
        var deduplicatedContentLines = deduplicated.GetLines(TextOrder.Content, SemanticTextMode.Deduplicated).Select(CreateLineSnapshot).ToList();
        var deduplicatedReadingLines = deduplicated.GetLines(TextOrder.Reading, SemanticTextMode.Deduplicated).Select(CreateLineSnapshot).ToList();
        var rawContentParagraphs = raw.GetParagraphs(TextOrder.Content, SemanticTextMode.Raw).Select(CreateParagraphSnapshot).ToList();
        var rawReadingParagraphs = raw.GetParagraphs(TextOrder.Reading, SemanticTextMode.Raw).Select(CreateParagraphSnapshot).ToList();
        var deduplicatedContentParagraphs = deduplicated.GetParagraphs(TextOrder.Content, SemanticTextMode.Deduplicated).Select(CreateParagraphSnapshot).ToList();
        var deduplicatedReadingParagraphs = deduplicated.GetParagraphs(TextOrder.Reading, SemanticTextMode.Deduplicated).Select(CreateParagraphSnapshot).ToList();

        return new SemanticPageSnapshot
        {
            PageNumber = pageNumber,
            RawContentText = rawContentText,
            RawReadingText = rawReadingText,
            DeduplicatedContentText = deduplicatedContentText,
            DeduplicatedReadingText = deduplicatedReadingText,
            RawWordCount = raw.RawWords.Count,
            DeduplicatedWordCount = deduplicated.DeduplicatedWords.Count,
            RawWords = rawWords,
            DeduplicatedWords = deduplicatedWords,
            RawContentLines = rawContentLines,
            RawReadingLines = rawReadingLines,
            DeduplicatedContentLines = deduplicatedContentLines,
            DeduplicatedReadingLines = deduplicatedReadingLines,
            RawContentParagraphs = rawContentParagraphs,
            RawReadingParagraphs = rawReadingParagraphs,
            DeduplicatedContentParagraphs = deduplicatedContentParagraphs,
            DeduplicatedReadingParagraphs = deduplicatedReadingParagraphs,
            Review = CreatePageReview(pageNumber, deduplicatedWords, deduplicatedReadingLines, deduplicatedReadingParagraphs)
        };
    }

    private static SemanticWordSnapshot CreateWordSnapshot(SemanticWord word)
    {
        return new SemanticWordSnapshot
        {
            Text = word.Text,
            BoundingBox = CreateRect(word.BoundingBox),
            SourceReferences = word.SourceReferences.Select(CreateSourceReference).ToList()
        };
    }

    private static SemanticLineSnapshot CreateLineSnapshot(SemanticLine line)
    {
        return new SemanticLineSnapshot
        {
            Text = line.Text,
            BoundingBox = CreateRect(line.BoundingBox),
            SourceReferences = line.SourceReferences.Select(CreateSourceReference).ToList()
        };
    }

    private static SemanticParagraphSnapshot CreateParagraphSnapshot(SemanticParagraph paragraph)
    {
        return new SemanticParagraphSnapshot
        {
            Text = paragraph.Text,
            BoundingBox = CreateRect(paragraph.BoundingBox),
            SourceReferences = paragraph.SourceReferences.Select(CreateSourceReference).ToList()
        };
    }

    private static SemanticSourceReferenceSnapshot CreateSourceReference(SemanticSourceRef sourceReference)
    {
        return new SemanticSourceReferenceSnapshot
        {
            StreamId = sourceReference.StreamId,
            OperatorStart = sourceReference.OperatorStart,
            OperatorLength = sourceReference.OperatorLength
        };
    }

    private static SemanticRectSnapshot CreateRect(PdfRect<double> rect)
    {
        return new SemanticRectSnapshot
        {
            LLx = Round(rect.LLx),
            LLy = Round(rect.LLy),
            URx = Round(rect.URx),
            URy = Round(rect.URy)
        };
    }

    private static void AddOverlayPage(PdfDocument overlayDoc, PdfPage sourcePage, SemanticTextPage raw, SemanticTextPage deduplicated)
    {
        var page = overlayDoc.AddPage();
        page.MediaBox = sourcePage.MediaBox;

        var form = XObjForm.FromPage(sourcePage);
        using var writer = page.GetWriter();
        writer.Form(form);

        DrawRects(writer, raw.RawWords.Select(x => x.BoundingBox), 0, 180, 0, 0.35);
        DrawRects(writer, deduplicated.GetLines(TextOrder.Reading, SemanticTextMode.Deduplicated).Select(x => x.BoundingBox), 255, 140, 0, 0.6);
        DrawRects(writer, deduplicated.GetParagraphs(TextOrder.Reading, SemanticTextMode.Deduplicated).Select(x => x.BoundingBox), 0, 90, 220, 0.9);

        if (deduplicated.DeduplicatedWords.Count != raw.RawWords.Count)
        {
            DrawRects(writer, deduplicated.DeduplicatedWords.Select(x => x.BoundingBox), 180, 0, 180, 0.45);
        }
    }

    private static void DrawRects(
        ContentWriter<double> writer,
        IEnumerable<PdfRect<double>> rects,
        int r,
        int g,
        int b,
        double lineWidth)
    {
        writer.SetStrokingRGB(r, g, b);
        foreach (var rect in rects)
        {
            if (rect.Width() <= 0 || rect.Height() <= 0)
            {
                continue;
            }

            writer.LineWidth(lineWidth)
                .Rect(rect.LLx, rect.LLy, rect.Width(), rect.Height())
                .Stroke();
        }
    }

    private static SemanticReviewSnapshot CreateDocumentReview(IReadOnlyList<SemanticPageSnapshot> pages)
    {
        var words = pages.SelectMany(x => x.DeduplicatedWords).ToList();
        var lines = pages.SelectMany(x => x.DeduplicatedReadingLines).ToList();
        var paragraphs = pages.SelectMany(x => x.DeduplicatedReadingParagraphs).ToList();
        var stats = CreateStats(words, lines, paragraphs);
        var warnings = new List<string>();

        warnings.AddRange(EvaluateStats(stats, "document"));
        foreach (var page in pages.Where(x => x.Review.Warnings.Count > 0))
        {
            warnings.AddRange(page.Review.Warnings);
        }

        return new SemanticReviewSnapshot
        {
            Scope = "document",
            MostlyTextual = stats.MostlyTextual,
            Applicable = stats.Applicable,
            AverageCharsPerWord = stats.AverageCharsPerWord,
            AverageWordsPerLine = stats.AverageWordsPerLine,
            AverageLinesPerParagraph = stats.AverageLinesPerParagraph,
            WordCount = stats.WordCount,
            LineCount = stats.LineCount,
            ParagraphCount = stats.ParagraphCount,
            Warnings = warnings
        };
    }

    private static SemanticReviewSnapshot CreatePageReview(
        int pageNumber,
        IReadOnlyList<SemanticWordSnapshot> words,
        IReadOnlyList<SemanticLineSnapshot> lines,
        IReadOnlyList<SemanticParagraphSnapshot> paragraphs)
    {
        var stats = CreateStats(words, lines, paragraphs);
        return new SemanticReviewSnapshot
        {
            Scope = $"page {pageNumber}",
            MostlyTextual = stats.MostlyTextual,
            Applicable = stats.Applicable,
            AverageCharsPerWord = stats.AverageCharsPerWord,
            AverageWordsPerLine = stats.AverageWordsPerLine,
            AverageLinesPerParagraph = stats.AverageLinesPerParagraph,
            WordCount = stats.WordCount,
            LineCount = stats.LineCount,
            ParagraphCount = stats.ParagraphCount,
            Warnings = EvaluateStats(stats, $"page {pageNumber}")
        };
    }

    private static SemanticStats CreateStats(
        IReadOnlyList<SemanticWordSnapshot> words,
        IReadOnlyList<SemanticLineSnapshot> lines,
        IReadOnlyList<SemanticParagraphSnapshot> paragraphs)
    {
        var relevantChars = 0;
        var textualChars = 0;
        foreach (var word in words)
        {
            foreach (var ch in word.Text)
            {
                if (char.IsWhiteSpace(ch))
                {
                    continue;
                }

                relevantChars++;
                if (IsTextualCharacter(ch))
                {
                    textualChars++;
                }
            }
        }
        var mostlyTextual = relevantChars > 0 && ((double)textualChars / relevantChars) >= MostlyTextualThreshold;

        var avgCharsPerWord = words.Count == 0
            ? 0d
            : words.Average(x => x.Text.Count(ch => !char.IsWhiteSpace(ch)));
        var avgWordsPerLine = lines.Count == 0
            ? 0d
            : (double)words.Count / lines.Count;
        var avgLinesPerParagraph = paragraphs.Count == 0
            ? 0d
            : (double)lines.Count / paragraphs.Count;

        var applicable = mostlyTextual
            && words.Count >= MinimumWordsForReview
            && lines.Count >= MinimumLinesForReview
            && paragraphs.Count >= MinimumParagraphsForReview;

        return new SemanticStats
        {
            MostlyTextual = mostlyTextual,
            Applicable = applicable,
            AverageCharsPerWord = avgCharsPerWord,
            AverageWordsPerLine = avgWordsPerLine,
            AverageLinesPerParagraph = avgLinesPerParagraph,
            WordCount = words.Count,
            LineCount = lines.Count,
            ParagraphCount = paragraphs.Count
        };
    }

    private static List<string> EvaluateStats(SemanticStats stats, string scope)
    {
        var warnings = new List<string>();
        if (!stats.Applicable)
        {
            warnings.Add(GetNotApplicableReason(stats, scope));
            return warnings;
        }

        if (stats.AverageCharsPerWord < MinAverageCharsPerWord || stats.AverageCharsPerWord > MaxAverageCharsPerWord)
        {
            warnings.Add($"{scope}: average chars per word {stats.AverageCharsPerWord:0.00} is outside the normal range {MinAverageCharsPerWord:0.0}-{MaxAverageCharsPerWord:0.0}.");
        }

        if (stats.AverageWordsPerLine < MinAverageWordsPerLine || stats.AverageWordsPerLine > MaxAverageWordsPerLine)
        {
            warnings.Add($"{scope}: average words per line {stats.AverageWordsPerLine:0.00} is outside the normal range {MinAverageWordsPerLine:0.0}-{MaxAverageWordsPerLine:0.0}.");
        }

        if (stats.AverageLinesPerParagraph < MinAverageLinesPerParagraph || stats.AverageLinesPerParagraph > MaxAverageLinesPerParagraph)
        {
            warnings.Add($"{scope}: average lines per paragraph {stats.AverageLinesPerParagraph:0.00} is outside the normal range {MinAverageLinesPerParagraph:0.0}-{MaxAverageLinesPerParagraph:0.0}.");
        }

        return warnings;
    }

    private static void WriteReviewFile(string reviewPath, SemanticDocumentSnapshot snapshot)
    {
        using var writer = new StreamWriter(reviewPath, false);
        writer.WriteLine($"pdf: {snapshot.PdfName}");
        writer.WriteLine($"applicable: {snapshot.Review.Applicable}");
        writer.WriteLine($"mostly_textual: {snapshot.Review.MostlyTextual}");
        writer.WriteLine($"avg_chars_per_word: {snapshot.Review.AverageCharsPerWord:0.00}");
        writer.WriteLine($"avg_words_per_line: {snapshot.Review.AverageWordsPerLine:0.00}");
        writer.WriteLine($"avg_lines_per_paragraph: {snapshot.Review.AverageLinesPerParagraph:0.00}");
        writer.WriteLine($"word_count: {snapshot.Review.WordCount}");
        writer.WriteLine($"line_count: {snapshot.Review.LineCount}");
        writer.WriteLine($"paragraph_count: {snapshot.Review.ParagraphCount}");
        writer.WriteLine();

        if (!snapshot.Review.Applicable)
        {
            writer.WriteLine("review: heuristic review not applicable");
            foreach (var warning in snapshot.Review.Warnings)
            {
                writer.WriteLine($"- {warning}");
            }
        }
        else if (snapshot.Review.Warnings.Count == 0)
        {
            writer.WriteLine("review: no heuristic warnings");
        }
        else
        {
            writer.WriteLine("review warnings:");
            foreach (var warning in snapshot.Review.Warnings)
            {
                writer.WriteLine($"- {warning}");
            }
        }
    }

    private static string GetNotApplicableReason(SemanticStats stats, string scope)
    {
        if (!stats.MostlyTextual)
        {
            return $"{scope}: heuristic review skipped because the extracted text is not predominantly textual.";
        }

        if (stats.WordCount < MinimumWordsForReview)
        {
            return $"{scope}: heuristic review skipped because word count {stats.WordCount} is below the minimum {MinimumWordsForReview}.";
        }

        if (stats.LineCount < MinimumLinesForReview)
        {
            return $"{scope}: heuristic review skipped because line count {stats.LineCount} is below the minimum {MinimumLinesForReview}.";
        }

        return $"{scope}: heuristic review skipped because paragraph count {stats.ParagraphCount} is below the minimum {MinimumParagraphsForReview}.";
    }

    private static bool IsTextualCharacter(char ch)
    {
        return char.IsLetterOrDigit(ch) || char.IsPunctuation(ch) || char.IsSymbol(ch);
    }

    private static double Round(double value) => Math.Round(value, 3);
}

internal sealed class SemanticDocumentSnapshot
{
    public required string PdfName { get; init; }
    public required int PageCount { get; init; }
    public required List<SemanticPageSnapshot> Pages { get; init; }
    public required SemanticReviewSnapshot Review { get; init; }
}

internal sealed class SemanticPageSnapshot
{
    public required int PageNumber { get; init; }
    public required string RawContentText { get; init; }
    public required string RawReadingText { get; init; }
    public required string DeduplicatedContentText { get; init; }
    public required string DeduplicatedReadingText { get; init; }
    public required int RawWordCount { get; init; }
    public required int DeduplicatedWordCount { get; init; }
    public required List<SemanticWordSnapshot> RawWords { get; init; }
    public required List<SemanticWordSnapshot> DeduplicatedWords { get; init; }
    public required List<SemanticLineSnapshot> RawContentLines { get; init; }
    public required List<SemanticLineSnapshot> RawReadingLines { get; init; }
    public required List<SemanticLineSnapshot> DeduplicatedContentLines { get; init; }
    public required List<SemanticLineSnapshot> DeduplicatedReadingLines { get; init; }
    public required List<SemanticParagraphSnapshot> RawContentParagraphs { get; init; }
    public required List<SemanticParagraphSnapshot> RawReadingParagraphs { get; init; }
    public required List<SemanticParagraphSnapshot> DeduplicatedContentParagraphs { get; init; }
    public required List<SemanticParagraphSnapshot> DeduplicatedReadingParagraphs { get; init; }
    public required SemanticReviewSnapshot Review { get; init; }
}

internal sealed class SemanticWordSnapshot
{
    public required string Text { get; init; }
    public required SemanticRectSnapshot BoundingBox { get; init; }
    public required List<SemanticSourceReferenceSnapshot> SourceReferences { get; init; }
}

internal sealed class SemanticLineSnapshot
{
    public required string Text { get; init; }
    public required SemanticRectSnapshot BoundingBox { get; init; }
    public required List<SemanticSourceReferenceSnapshot> SourceReferences { get; init; }
}

internal sealed class SemanticParagraphSnapshot
{
    public required string Text { get; init; }
    public required SemanticRectSnapshot BoundingBox { get; init; }
    public required List<SemanticSourceReferenceSnapshot> SourceReferences { get; init; }
}

internal sealed class SemanticRectSnapshot
{
    public required double LLx { get; init; }
    public required double LLy { get; init; }
    public required double URx { get; init; }
    public required double URy { get; init; }
}

internal sealed class SemanticSourceReferenceSnapshot
{
    public required ulong StreamId { get; init; }
    public required int OperatorStart { get; init; }
    public required int OperatorLength { get; init; }
}

internal sealed class SemanticTextRunResult
{
    public required string SnapshotPath { get; init; }
    public required string OverlayPath { get; init; }
    public required string ReviewPath { get; init; }
    public required IReadOnlyList<string> ReviewWarnings { get; init; }
}

internal sealed class SemanticReviewSnapshot
{
    public required string Scope { get; init; }
    public required bool MostlyTextual { get; init; }
    public required bool Applicable { get; init; }
    public required double AverageCharsPerWord { get; init; }
    public required double AverageWordsPerLine { get; init; }
    public required double AverageLinesPerParagraph { get; init; }
    public required int WordCount { get; init; }
    public required int LineCount { get; init; }
    public required int ParagraphCount { get; init; }
    public required List<string> Warnings { get; init; }
}

internal sealed class SemanticStats
{
    public required bool MostlyTextual { get; init; }
    public required bool Applicable { get; init; }
    public required double AverageCharsPerWord { get; init; }
    public required double AverageWordsPerLine { get; init; }
    public required double AverageLinesPerParagraph { get; init; }
    public required int WordCount { get; init; }
    public required int LineCount { get; init; }
    public required int ParagraphCount { get; init; }
}
