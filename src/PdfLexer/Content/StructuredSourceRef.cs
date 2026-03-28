namespace PdfLexer.Content;

public readonly record struct StructuredSourceRef(
    ulong StreamId,
    int OperatorStart,
    int OperatorLength);
