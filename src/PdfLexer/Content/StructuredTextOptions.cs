namespace PdfLexer.Content;

public sealed class StructuredTextOptions
{
    public TextOrder Order { get; init; } = TextOrder.Reading;
    public StructuredTextMode Mode { get; init; } = StructuredTextMode.Raw;
    public IStructuredTextGrouper? Grouper { get; init; }
    public double DuplicateOverlapThreshold { get; init; } = 0.9d;
    public double DuplicateBaselineMultiplier { get; init; } = 0.35d;
    public double WordGapMultiplier { get; init; } = 0.5d;
    public double LineMergeMultiplier { get; init; } = 0.65d;
    public double LineGapMultiplier { get; init; } = 6d;
    public double ParagraphSpacingMultiplier { get; init; } = 1.75d;
    public double ParagraphIndentMultiplier { get; init; } = 2d;
    public double RotationToleranceDegrees { get; init; } = 5d;
}
