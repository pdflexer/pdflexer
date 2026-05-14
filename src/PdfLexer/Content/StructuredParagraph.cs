using System.Collections.ObjectModel;

namespace PdfLexer.Content;

public sealed class StructuredParagraph
{
    private PdfRect<double>? boundingBox;
    private PdfRect<double>? relativeBoundingBox;
    private string? text;
    private IReadOnlyList<StructuredSourceRef>? sourceReferences;
    private readonly StructuredPageSpace pageSpace;

    internal StructuredParagraph(IReadOnlyList<StructuredLine> lines, StructuredPageSpace pageSpace)
    {
        Lines = new ReadOnlyCollection<StructuredLine>(lines.ToList());
        SequenceIndex = lines[0].SequenceIndex;
        this.pageSpace = pageSpace;
    }

    public int SequenceIndex { get; }

    public IReadOnlyList<StructuredLine> Lines { get; }
    public IReadOnlyList<StructuredSourceRef> SourceReferences => sourceReferences ??=
        new ReadOnlyCollection<StructuredSourceRef>(Lines.SelectMany(x => x.SourceReferences).Distinct().ToList());
    public string Text => text ??= string.Join(Environment.NewLine, Lines.Select(x => x.Text));
    public PdfRect<double> BoundingBox => boundingBox ??= StructuredBounds.Union(Lines.SelectMany(x => x.Words).SelectMany(x => x.Characters).ToList());
    public PdfRect<double> RelativeBoundingBox => relativeBoundingBox ??= pageSpace.Normalize(BoundingBox);
}
