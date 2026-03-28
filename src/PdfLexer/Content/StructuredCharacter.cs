namespace PdfLexer.Content;

public sealed class StructuredCharacter
{
    private PdfRect<double>? boundingBox;
    private PdfRect<double>? relativeBoundingBox;
    private PdfPoint<double>? relativePosition;
    private readonly int sequenceIndex;
    private readonly StructuredPageSpace pageSpace;

    internal StructuredCharacter(char value, StructuredGlyphSnapshot snapshot, StructuredSourceRef sourceReference, int sequenceIndex, bool startsNewLine, StructuredPageSpace pageSpace)
    {
        Char = value;
        Snapshot = snapshot;
        SourceReference = sourceReference;
        this.sequenceIndex = sequenceIndex;
        StartsNewLine = startsNewLine;
        this.pageSpace = pageSpace;
    }

    internal StructuredGlyphSnapshot Snapshot { get; }
    internal bool StartsNewLine { get; }

    public char Char { get; }
    public StructuredSourceRef SourceReference { get; }
    public PdfPoint<double> Position => Snapshot.Origin;
    public PdfPoint<double> RelativePosition => relativePosition ??= pageSpace.Normalize(Position);
    public double Rotation => Snapshot.Rotation;
    public double FontSize => Snapshot.FontSize;
    public PdfRect<double> BoundingBox => boundingBox ??= Snapshot.CreateBoundingBox();
    public PdfRect<double> RelativeBoundingBox => relativeBoundingBox ??= pageSpace.Normalize(BoundingBox);

    internal StructuredPageSpace PageSpace => pageSpace;
    internal int SequenceIndex => sequenceIndex;
    internal double InlineStart => Snapshot.InlineCoordinate(Position);
    internal double InlineEnd => InlineStart + Snapshot.InlineAdvance;
    internal double BaselineCoordinate => Snapshot.PerpendicularCoordinate(Position);
}
