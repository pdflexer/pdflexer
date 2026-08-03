using System.Collections.ObjectModel;
using PdfLexer.Content;
using PdfLexer.Content.Model;
using PdfLexer.DOM;

namespace PdfLexer.Remediation;

internal sealed class PageRemediationState
{
    private int _nextMcid;

    public PageRemediationState(
        PdfPage page,
        int pageIndex,
        StructuredTextPage structuredText,
        IReadOnlyList<IContentNode<double>> originalContent,
        List<IContentNode<double>> workingContent)
    {
        Page = page;
        PageIndex = pageIndex;
        StructuredText = structuredText;
        OriginalContent = new ReadOnlyCollection<IContentNode<double>>(originalContent.ToList());
        WorkingContent = workingContent;
    }

    public PdfPage Page { get; }

    public int PageIndex { get; }

    public StructuredTextPage StructuredText { get; }

    public IReadOnlyList<IContentNode<double>> OriginalContent { get; }

    public List<IContentNode<double>> WorkingContent { get; }

    public bool IsDirty { get; private set; }

    public TextOwnershipIndex TextOwnership { get; } = new();

    public Dictionary<IContentItem<double>, RemediationClaim> ContentOwnership { get; } =
        new(ReferenceEqualityComparer.Instance);

    public Dictionary<PdfDictionary, RemediationClaim> AnnotationOwnership { get; } =
        new(ReferenceEqualityComparer.Instance);

    public IReadOnlyList<ContentRemediationCandidate> ContentCandidates { get; internal set; } =
        Array.Empty<ContentRemediationCandidate>();

    public IReadOnlyList<AnnotationRemediationCandidate> AnnotationCandidates { get; internal set; } =
        Array.Empty<AnnotationRemediationCandidate>();

    public IReadOnlyDictionary<Granularity, IReadOnlyList<TextRemediationCandidate>> TextCandidates { get; internal set; } =
        new Dictionary<Granularity, IReadOnlyList<TextRemediationCandidate>>();

    public int AllocateMcid()
    {
        IsDirty = true;
        return _nextMcid++;
    }

    public void MarkDirty()
    {
        IsDirty = true;
    }

}
