namespace PdfLexer.Content;

public sealed class StructuredCharacter
{
    private PdfRect<double>? boundingBox;
    private PdfRect<double>? relativeBoundingBox;
    private PdfPoint<double>? relativePosition;
    private readonly int sequenceIndex;
    private readonly StructuredPageSpace pageSpace;

    internal StructuredCharacter(
        char value,
        StructuredGlyphSnapshot snapshot,
        StructuredSourceRef sourceReference,
        int sourceCharacterIndex,
        int sequenceIndex,
        bool startsNewLine,
        StructuredPageSpace pageSpace)
    {
        Char = value;
        Snapshot = snapshot;
        SourceReference = sourceReference;
        SourceCharacterIndex = sourceCharacterIndex;
        this.sequenceIndex = sequenceIndex;
        StartsNewLine = startsNewLine;
        this.pageSpace = pageSpace;
    }

    internal StructuredGlyphSnapshot Snapshot { get; }
    internal bool StartsNewLine { get; }

    public char Char { get; }
    public StructuredSourceRef SourceReference { get; }
    public int SourceCharacterIndex { get; }
    public PdfPoint<double> Position => Snapshot.Origin;
    public PdfPoint<double> RelativePosition => relativePosition ??= pageSpace.Normalize(Position);
    public double Rotation => Snapshot.Rotation;
    public double FontSize => Snapshot.FontSize;
    public StructuredStyleSnapshot Style => Snapshot.Style;
    public string? FontName => Snapshot.Style.FontName;
    public int? FontWeight => Snapshot.Style.FontWeight;
    public bool? Italic => Snapshot.Style.Italic;
    public bool? IsGrayish => Snapshot.Style.IsGrayish;
    public PdfRect<double> BoundingBox => boundingBox ??= Snapshot.CreateBoundingBox();
    public PdfRect<double> RelativeBoundingBox => relativeBoundingBox ??= pageSpace.Normalize(BoundingBox);

    internal StructuredPageSpace PageSpace => pageSpace;
    public int SequenceIndex => sequenceIndex;
    internal double InlineStart => Snapshot.InlineCoordinate(Position);
    internal double InlineEnd => InlineStart + Snapshot.InlineAdvance;
    internal double BaselineCoordinate => Snapshot.PerpendicularCoordinate(Position);
}
