namespace PdfLexer.Content;

public interface IStructuredTextGrouper
{
    StructuredTextGroupingResult Group(StructuredTextGroupingInput input);
}

public sealed class StructuredTextGroupingInput
{
    public required IReadOnlyList<StructuredWord> Words { get; init; }
    public IReadOnlyList<IReadOnlyList<StructuredWord>>? ContentLineCandidates { get; init; }
    public required StructuredTextOptions Options { get; init; }
}

public sealed class StructuredTextGroupingResult
{
    public required IReadOnlyList<IReadOnlyList<StructuredWord>> ContentLines { get; init; }
    public required IReadOnlyList<IReadOnlyList<StructuredWord>> ReadingLines { get; init; }
    public required IReadOnlyList<IReadOnlyList<IReadOnlyList<StructuredWord>>> ContentParagraphs { get; init; }
    public required IReadOnlyList<IReadOnlyList<IReadOnlyList<StructuredWord>>> ReadingParagraphs { get; init; }
}
