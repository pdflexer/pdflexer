namespace PdfLexer.Remediation;

/// <summary>
/// Compatibility-free empty flow index used by the shared predicate context. Preview programs do
/// not admit flow-region declarations; retaining this small internal value avoids teaching the
/// native candidate evaluator about a deferred capability.
/// </summary>
internal sealed class DocumentFlowIndex
{
    private readonly IReadOnlyDictionary<int, Dictionary<string, FlowRegionResolution>> _byPage;

    public DocumentFlowIndex(
        IReadOnlyDictionary<int, Dictionary<string, FlowRegionResolution>> byPage,
        IReadOnlyList<RemediationCandidate> candidates,
        IReadOnlyList<RemediationClaim> claims)
    {
        _byPage = byPage;
        Candidates = candidates;
    }

    public IReadOnlyList<RemediationCandidate> Candidates { get; }

    public IReadOnlyDictionary<string, FlowRegionResolution> ForPage(int pageIndex) =>
        _byPage.TryGetValue(pageIndex, out var resolutions)
            ? resolutions
            : new Dictionary<string, FlowRegionResolution>();

    public FlowRegionResolution? Find(string regionId, int pageIndex) =>
        _byPage.TryGetValue(pageIndex, out var resolutions) &&
        resolutions.TryGetValue(regionId, out var resolution)
            ? resolution
            : null;
}
