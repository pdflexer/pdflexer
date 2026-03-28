using System.Collections.ObjectModel;

namespace PdfLexer.Content;

public sealed class StructuredTextPage
{
    private readonly StructuredPageSpace pageSpace;
    private readonly IReadOnlyList<StructuredWord> rawWords;
    private readonly IReadOnlyList<StructuredLine> rawContentLines;
    private readonly IReadOnlyList<StructuredLine> rawReadingLines;
    private readonly IReadOnlyList<StructuredParagraph> rawContentParagraphs;
    private readonly IReadOnlyList<StructuredParagraph> rawReadingParagraphs;
    private readonly Lazy<StructuredTextDedupData> deduplicated;

    internal StructuredTextPage(
        IReadOnlyList<StructuredCharacter> characters,
        StructuredPageSpace pageSpace,
        IReadOnlyList<StructuredWord> rawWords,
        IReadOnlyList<StructuredLine> rawContentLines,
        IReadOnlyList<StructuredLine> rawReadingLines,
        IReadOnlyList<StructuredParagraph> rawContentParagraphs,
        IReadOnlyList<StructuredParagraph> rawReadingParagraphs,
        TextOrder defaultOrder,
        StructuredTextMode defaultMode,
        Func<StructuredTextDedupData> dedupFactory)
    {
        this.pageSpace = pageSpace;
        Characters = new ReadOnlyCollection<StructuredCharacter>(characters.ToList());
        this.rawWords = new ReadOnlyCollection<StructuredWord>(rawWords.ToList());
        this.rawContentLines = new ReadOnlyCollection<StructuredLine>(rawContentLines.ToList());
        this.rawReadingLines = new ReadOnlyCollection<StructuredLine>(rawReadingLines.ToList());
        this.rawContentParagraphs = new ReadOnlyCollection<StructuredParagraph>(rawContentParagraphs.ToList());
        this.rawReadingParagraphs = new ReadOnlyCollection<StructuredParagraph>(rawReadingParagraphs.ToList());
        deduplicated = new Lazy<StructuredTextDedupData>(dedupFactory);
        DefaultOrder = defaultOrder;
        DefaultMode = defaultMode;
    }

    internal TextOrder DefaultOrder { get; }
    internal StructuredTextMode DefaultMode { get; }

    public IReadOnlyList<StructuredCharacter> Characters { get; }
    public PdfRect<double> RelativePageBox => pageSpace.RelativePageBox;
    public IReadOnlyList<StructuredWord> RawWords => rawWords;
    public IReadOnlyList<StructuredWord> DeduplicatedWords => deduplicated.Value.Words;
    public IReadOnlyList<StructuredWord> Words => GetWords(DefaultMode);
    public IReadOnlyList<StructuredLine> RawLines => GetLines(DefaultOrder, StructuredTextMode.Raw);
    public IReadOnlyList<StructuredLine> DeduplicatedLines => GetLines(DefaultOrder, StructuredTextMode.Deduplicated);
    public IReadOnlyList<StructuredLine> Lines => GetLines(DefaultOrder, DefaultMode);
    public IReadOnlyList<StructuredParagraph> RawParagraphs => GetParagraphs(DefaultOrder, StructuredTextMode.Raw);
    public IReadOnlyList<StructuredParagraph> DeduplicatedParagraphs => GetParagraphs(DefaultOrder, StructuredTextMode.Deduplicated);
    public IReadOnlyList<StructuredParagraph> Paragraphs => GetParagraphs(DefaultOrder, DefaultMode);

    public string GetText() => GetText(DefaultOrder, DefaultMode);

    public string GetText(TextOrder order) => GetText(order, DefaultMode);

    public string GetText(TextOrder order, StructuredTextMode mode)
    {
        var paragraphs = GetParagraphs(order, mode);
        if (paragraphs.Count > 0)
        {
            return string.Join(Environment.NewLine + Environment.NewLine, paragraphs.Select(x => x.Text));
        }

        var lines = GetLines(order, mode);
        return string.Join(Environment.NewLine, lines.Select(x => x.Text));
    }

    public IReadOnlyList<StructuredWord> GetWords(StructuredTextMode mode)
    {
        return mode == StructuredTextMode.Deduplicated ? deduplicated.Value.Words : rawWords;
    }

    public IReadOnlyList<StructuredLine> GetLines(TextOrder order, StructuredTextMode mode)
    {
        return (order, mode) switch
        {
            (TextOrder.Reading, StructuredTextMode.Deduplicated) => deduplicated.Value.ReadingLines,
            (TextOrder.Reading, _) => rawReadingLines,
            (_, StructuredTextMode.Deduplicated) => deduplicated.Value.ContentLines,
            _ => rawContentLines
        };
    }

    public IReadOnlyList<StructuredParagraph> GetParagraphs(TextOrder order, StructuredTextMode mode)
    {
        return (order, mode) switch
        {
            (TextOrder.Reading, StructuredTextMode.Deduplicated) => deduplicated.Value.ReadingParagraphs,
            (TextOrder.Reading, _) => rawReadingParagraphs,
            (_, StructuredTextMode.Deduplicated) => deduplicated.Value.ContentParagraphs,
            _ => rawContentParagraphs
        };
    }
}

internal sealed class StructuredTextDedupData
{
    public required IReadOnlyList<StructuredWord> Words { get; init; }
    public required IReadOnlyList<StructuredLine> ContentLines { get; init; }
    public required IReadOnlyList<StructuredLine> ReadingLines { get; init; }
    public required IReadOnlyList<StructuredParagraph> ContentParagraphs { get; init; }
    public required IReadOnlyList<StructuredParagraph> ReadingParagraphs { get; init; }
}
