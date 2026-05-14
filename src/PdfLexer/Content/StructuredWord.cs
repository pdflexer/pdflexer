using System.Collections.ObjectModel;
using System.Text;

namespace PdfLexer.Content;

public sealed class StructuredWord
{
    private PdfRect<double>? boundingBox;
    private PdfRect<double>? relativeBoundingBox;
    private PdfPoint<double>? relativePosition;
    private IReadOnlyList<StructuredSourceRef>? sourceReferences;
    private readonly StructuredPageSpace pageSpace;

    internal StructuredWord(
        IReadOnlyList<StructuredCharacter> characters,
        StructuredPageSpace pageSpace,
        bool hasExplicitBreakBefore = false)
    {
        Characters = new ReadOnlyCollection<StructuredCharacter>(characters.ToList());
        this.pageSpace = pageSpace;
        var sb = new StringBuilder(characters.Count);
        foreach (var character in characters)
        {
            sb.Append(character.Char);
        }

        Text = sb.ToString();
        SequenceIndex = characters[0].SequenceIndex;
        StartsNewLine = characters[0].StartsNewLine;
        HasExplicitBreakBefore = hasExplicitBreakBefore;
    }

    public int SequenceIndex { get; }
    internal bool StartsNewLine { get; }
    internal bool HasExplicitBreakBefore { get; }

    public string Text { get; }
    public IReadOnlyList<StructuredCharacter> Characters { get; }
    public IReadOnlyList<StructuredSourceRef> SourceReferences => sourceReferences ??=
        new ReadOnlyCollection<StructuredSourceRef>(Characters.Select(x => x.SourceReference).Distinct().ToList());
    public PdfPoint<double> Position => Characters[0].Position;
    public PdfPoint<double> RelativePosition => relativePosition ??= pageSpace.Normalize(Position);
    public double Rotation => Characters[0].Rotation;
    public double FontSize => Characters.Average(x => x.FontSize);
    public PdfRect<double> BoundingBox => boundingBox ??= StructuredBounds.Union(Characters);
    public PdfRect<double> RelativeBoundingBox => relativeBoundingBox ??= pageSpace.Normalize(BoundingBox);

    internal StructuredPageSpace PageSpace => pageSpace;
    internal double InlineStart => Characters[0].InlineStart;
    internal double InlineEnd => Characters[^1].InlineEnd;
    internal double BaselineCoordinate => Characters.Average(x => x.BaselineCoordinate);
}
