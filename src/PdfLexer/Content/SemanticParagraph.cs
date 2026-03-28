using System.Collections.ObjectModel;

namespace PdfLexer.Content;

public sealed class SemanticParagraph
{
    private PdfRect<double>? boundingBox;
    private PdfRect<double>? relativeBoundingBox;
    private string? text;
    private IReadOnlyList<SemanticSourceRef>? sourceReferences;
    private readonly SemanticPageSpace pageSpace;

    internal SemanticParagraph(IReadOnlyList<SemanticLine> lines, SemanticPageSpace pageSpace)
    {
        Lines = new ReadOnlyCollection<SemanticLine>(lines.ToList());
        SequenceIndex = lines[0].SequenceIndex;
        this.pageSpace = pageSpace;
    }

    internal int SequenceIndex { get; }

    public IReadOnlyList<SemanticLine> Lines { get; }
    public IReadOnlyList<SemanticSourceRef> SourceReferences => sourceReferences ??=
        new ReadOnlyCollection<SemanticSourceRef>(Lines.SelectMany(x => x.SourceReferences).Distinct().ToList());
    public string Text => text ??= string.Join(Environment.NewLine, Lines.Select(x => x.Text));
    public PdfRect<double> BoundingBox => boundingBox ??= SemanticBounds.Union(Lines.SelectMany(x => x.Words).SelectMany(x => x.Characters).ToList());
    public PdfRect<double> RelativeBoundingBox => relativeBoundingBox ??= pageSpace.Normalize(BoundingBox);
}
