namespace PdfLexer.Content;

public sealed class SemanticCharacter
{
    private PdfRect<double>? boundingBox;
    private PdfRect<double>? relativeBoundingBox;
    private PdfPoint<double>? relativePosition;
    private readonly int sequenceIndex;
    private readonly SemanticPageSpace pageSpace;

    internal SemanticCharacter(char value, SemanticGlyphSnapshot snapshot, SemanticSourceRef sourceReference, int sequenceIndex, bool startsNewLine, SemanticPageSpace pageSpace)
    {
        Char = value;
        Snapshot = snapshot;
        SourceReference = sourceReference;
        this.sequenceIndex = sequenceIndex;
        StartsNewLine = startsNewLine;
        this.pageSpace = pageSpace;
    }

    internal SemanticGlyphSnapshot Snapshot { get; }
    internal bool StartsNewLine { get; }

    public char Char { get; }
    public SemanticSourceRef SourceReference { get; }
    public PdfPoint<double> Position => Snapshot.Origin;
    public PdfPoint<double> RelativePosition => relativePosition ??= pageSpace.Normalize(Position);
    public double Rotation => Snapshot.Rotation;
    public double FontSize => Snapshot.FontSize;
    public PdfRect<double> BoundingBox => boundingBox ??= Snapshot.CreateBoundingBox();
    public PdfRect<double> RelativeBoundingBox => relativeBoundingBox ??= pageSpace.Normalize(BoundingBox);

    internal SemanticPageSpace PageSpace => pageSpace;
    internal int SequenceIndex => sequenceIndex;
    internal double InlineStart => Snapshot.InlineCoordinate(Position);
    internal double InlineEnd => InlineStart + Snapshot.InlineAdvance;
    internal double BaselineCoordinate => Snapshot.PerpendicularCoordinate(Position);
}
