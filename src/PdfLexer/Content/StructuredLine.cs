using System.Collections.ObjectModel;

namespace PdfLexer.Content;

public sealed class StructuredLine
{
    private PdfRect<double>? boundingBox;
    private PdfRect<double>? relativeBoundingBox;
    private PdfPoint<double>? relativePosition;
    private string? text;
    private IReadOnlyList<StructuredSourceRef>? sourceReferences;
    private readonly StructuredPageSpace pageSpace;

    internal StructuredLine(IReadOnlyList<StructuredWord> words, StructuredPageSpace pageSpace)
    {
        Words = new ReadOnlyCollection<StructuredWord>(words.ToList());
        SequenceIndex = words[0].SequenceIndex;
        this.pageSpace = pageSpace;
    }

    internal int SequenceIndex { get; }

    public IReadOnlyList<StructuredWord> Words { get; }
    public IReadOnlyList<StructuredSourceRef> SourceReferences => sourceReferences ??=
        new ReadOnlyCollection<StructuredSourceRef>(Words.SelectMany(x => x.SourceReferences).Distinct().ToList());
    public string Text => text ??= string.Join(" ", Words.Select(x => x.Text));
    public PdfPoint<double> Position => Words[0].Position;
    public PdfPoint<double> RelativePosition => relativePosition ??= pageSpace.Normalize(Position);
    public double Rotation => Words[0].Rotation;
    public double FontSize => Words.Average(x => x.FontSize);
    public PdfRect<double> BoundingBox => boundingBox ??= StructuredBounds.Union(Words.SelectMany(x => x.Characters).ToList());
    public PdfRect<double> RelativeBoundingBox => relativeBoundingBox ??= pageSpace.Normalize(BoundingBox);

    internal StructuredPageSpace PageSpace => pageSpace;
    internal double BaselineCoordinate => Words.Average(x => x.BaselineCoordinate);
    internal double InlineStart => Words.Min(x => x.InlineStart);
    internal double InlineEnd => Words.Max(x => x.InlineEnd);
}
