namespace PdfLexer.Content;

public interface ISemanticTextGrouper
{
    SemanticTextGroupingResult Group(SemanticTextGroupingInput input);
}

public sealed class SemanticTextGroupingInput
{
    public required IReadOnlyList<SemanticWord> Words { get; init; }
    public IReadOnlyList<IReadOnlyList<SemanticWord>>? ContentLineCandidates { get; init; }
    public required SemanticExtractOptions Options { get; init; }
}

public sealed class SemanticTextGroupingResult
{
    public required IReadOnlyList<IReadOnlyList<SemanticWord>> ContentLines { get; init; }
    public required IReadOnlyList<IReadOnlyList<SemanticWord>> ReadingLines { get; init; }
    public required IReadOnlyList<IReadOnlyList<IReadOnlyList<SemanticWord>>> ContentParagraphs { get; init; }
    public required IReadOnlyList<IReadOnlyList<IReadOnlyList<SemanticWord>>> ReadingParagraphs { get; init; }
}
