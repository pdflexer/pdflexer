namespace PdfLexer.Content;

public readonly record struct SemanticSourceRef(
    ulong StreamId,
    int OperatorStart,
    int OperatorLength);
