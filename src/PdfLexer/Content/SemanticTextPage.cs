using System.Collections.ObjectModel;

namespace PdfLexer.Content;

public sealed class SemanticTextPage
{
    private readonly SemanticPageSpace pageSpace;
    private readonly IReadOnlyList<SemanticWord> rawWords;
    private readonly IReadOnlyList<SemanticLine> rawContentLines;
    private readonly IReadOnlyList<SemanticLine> rawReadingLines;
    private readonly IReadOnlyList<SemanticParagraph> rawContentParagraphs;
    private readonly IReadOnlyList<SemanticParagraph> rawReadingParagraphs;
    private readonly Lazy<SemanticTextDedupData> deduplicated;

    internal SemanticTextPage(
        IReadOnlyList<SemanticCharacter> characters,
        SemanticPageSpace pageSpace,
        IReadOnlyList<SemanticWord> rawWords,
        IReadOnlyList<SemanticLine> rawContentLines,
        IReadOnlyList<SemanticLine> rawReadingLines,
        IReadOnlyList<SemanticParagraph> rawContentParagraphs,
        IReadOnlyList<SemanticParagraph> rawReadingParagraphs,
        TextOrder defaultOrder,
        SemanticTextMode defaultMode,
        Func<SemanticTextDedupData> dedupFactory)
    {
        this.pageSpace = pageSpace;
        Characters = new ReadOnlyCollection<SemanticCharacter>(characters.ToList());
        this.rawWords = new ReadOnlyCollection<SemanticWord>(rawWords.ToList());
        this.rawContentLines = new ReadOnlyCollection<SemanticLine>(rawContentLines.ToList());
        this.rawReadingLines = new ReadOnlyCollection<SemanticLine>(rawReadingLines.ToList());
        this.rawContentParagraphs = new ReadOnlyCollection<SemanticParagraph>(rawContentParagraphs.ToList());
        this.rawReadingParagraphs = new ReadOnlyCollection<SemanticParagraph>(rawReadingParagraphs.ToList());
        deduplicated = new Lazy<SemanticTextDedupData>(dedupFactory);
        DefaultOrder = defaultOrder;
        DefaultMode = defaultMode;
    }

    internal TextOrder DefaultOrder { get; }
    internal SemanticTextMode DefaultMode { get; }

    public IReadOnlyList<SemanticCharacter> Characters { get; }
    public PdfRect<double> RelativePageBox => pageSpace.RelativePageBox;
    public IReadOnlyList<SemanticWord> RawWords => rawWords;
    public IReadOnlyList<SemanticWord> DeduplicatedWords => deduplicated.Value.Words;
    public IReadOnlyList<SemanticWord> Words => GetWords(DefaultMode);
    public IReadOnlyList<SemanticLine> RawLines => GetLines(DefaultOrder, SemanticTextMode.Raw);
    public IReadOnlyList<SemanticLine> DeduplicatedLines => GetLines(DefaultOrder, SemanticTextMode.Deduplicated);
    public IReadOnlyList<SemanticLine> Lines => GetLines(DefaultOrder, DefaultMode);
    public IReadOnlyList<SemanticParagraph> RawParagraphs => GetParagraphs(DefaultOrder, SemanticTextMode.Raw);
    public IReadOnlyList<SemanticParagraph> DeduplicatedParagraphs => GetParagraphs(DefaultOrder, SemanticTextMode.Deduplicated);
    public IReadOnlyList<SemanticParagraph> Paragraphs => GetParagraphs(DefaultOrder, DefaultMode);

    public string GetText() => GetText(DefaultOrder, DefaultMode);

    public string GetText(TextOrder order) => GetText(order, DefaultMode);

    public string GetText(TextOrder order, SemanticTextMode mode)
    {
        var paragraphs = GetParagraphs(order, mode);
        if (paragraphs.Count > 0)
        {
            return string.Join(Environment.NewLine + Environment.NewLine, paragraphs.Select(x => x.Text));
        }

        var lines = GetLines(order, mode);
        return string.Join(Environment.NewLine, lines.Select(x => x.Text));
    }

    public IReadOnlyList<SemanticWord> GetWords(SemanticTextMode mode)
    {
        return mode == SemanticTextMode.Deduplicated ? deduplicated.Value.Words : rawWords;
    }

    public IReadOnlyList<SemanticLine> GetLines(TextOrder order, SemanticTextMode mode)
    {
        return (order, mode) switch
        {
            (TextOrder.Reading, SemanticTextMode.Deduplicated) => deduplicated.Value.ReadingLines,
            (TextOrder.Reading, _) => rawReadingLines,
            (_, SemanticTextMode.Deduplicated) => deduplicated.Value.ContentLines,
            _ => rawContentLines
        };
    }

    public IReadOnlyList<SemanticParagraph> GetParagraphs(TextOrder order, SemanticTextMode mode)
    {
        return (order, mode) switch
        {
            (TextOrder.Reading, SemanticTextMode.Deduplicated) => deduplicated.Value.ReadingParagraphs,
            (TextOrder.Reading, _) => rawReadingParagraphs,
            (_, SemanticTextMode.Deduplicated) => deduplicated.Value.ContentParagraphs,
            _ => rawContentParagraphs
        };
    }
}

internal sealed class SemanticTextDedupData
{
    public required IReadOnlyList<SemanticWord> Words { get; init; }
    public required IReadOnlyList<SemanticLine> ContentLines { get; init; }
    public required IReadOnlyList<SemanticLine> ReadingLines { get; init; }
    public required IReadOnlyList<SemanticParagraph> ContentParagraphs { get; init; }
    public required IReadOnlyList<SemanticParagraph> ReadingParagraphs { get; init; }
}
