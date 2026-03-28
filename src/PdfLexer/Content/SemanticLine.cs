using System.Collections.ObjectModel;

namespace PdfLexer.Content;

public sealed class SemanticLine
{
    private PdfRect<double>? boundingBox;
    private PdfRect<double>? relativeBoundingBox;
    private PdfPoint<double>? relativePosition;
    private string? text;
    private IReadOnlyList<SemanticSourceRef>? sourceReferences;
    private readonly SemanticPageSpace pageSpace;

    internal SemanticLine(IReadOnlyList<SemanticWord> words, SemanticPageSpace pageSpace)
    {
        Words = new ReadOnlyCollection<SemanticWord>(words.ToList());
        SequenceIndex = words[0].SequenceIndex;
        this.pageSpace = pageSpace;
    }

    internal int SequenceIndex { get; }

    public IReadOnlyList<SemanticWord> Words { get; }
    public IReadOnlyList<SemanticSourceRef> SourceReferences => sourceReferences ??=
        new ReadOnlyCollection<SemanticSourceRef>(Words.SelectMany(x => x.SourceReferences).Distinct().ToList());
    public string Text => text ??= string.Join(" ", Words.Select(x => x.Text));
    public PdfPoint<double> Position => Words[0].Position;
    public PdfPoint<double> RelativePosition => relativePosition ??= pageSpace.Normalize(Position);
    public double Rotation => Words[0].Rotation;
    public double FontSize => Words.Average(x => x.FontSize);
    public PdfRect<double> BoundingBox => boundingBox ??= SemanticBounds.Union(Words.SelectMany(x => x.Characters).ToList());
    public PdfRect<double> RelativeBoundingBox => relativeBoundingBox ??= pageSpace.Normalize(BoundingBox);

    internal SemanticPageSpace PageSpace => pageSpace;
    internal double BaselineCoordinate => Words.Average(x => x.BaselineCoordinate);
    internal double InlineStart => Words.Min(x => x.InlineStart);
    internal double InlineEnd => Words.Max(x => x.InlineEnd);
}
