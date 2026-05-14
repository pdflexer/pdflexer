using System.Collections.ObjectModel;
using PdfLexer.Content;
using PdfLexer.Content.Model;
using PdfLexer.DOM;

namespace PdfLexer.Remediation;

internal sealed class PageRemediationState
{
    private readonly Dictionary<Stage, IReadOnlyList<RemediationClaim>> _claimSnapshots = new();
    private readonly List<string> _planDiagnostics = new();
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

    public IReadOnlyDictionary<Stage, IReadOnlyList<RemediationClaim>> ClaimSnapshots => _claimSnapshots;

    public IReadOnlyList<string> PlanDiagnostics => _planDiagnostics;

    public int AllocateMcid()
    {
        IsDirty = true;
        return _nextMcid++;
    }

    public void MarkDirty()
    {
        IsDirty = true;
    }

    public void SetClaimSnapshot(Stage stage, IReadOnlyList<RemediationClaim> claims)
    {
        _claimSnapshots[stage] = new ReadOnlyCollection<RemediationClaim>(claims.ToList());
    }

    public IReadOnlyList<RemediationClaim> GetClaimSnapshot(Stage stage)
    {
        return _claimSnapshots.TryGetValue(stage, out var claims)
            ? claims
            : Array.Empty<RemediationClaim>();
    }

    public void AddPlanDiagnostic(string diagnostic)
    {
        if (!string.IsNullOrWhiteSpace(diagnostic))
        {
            _planDiagnostics.Add(diagnostic);
        }
    }
}
