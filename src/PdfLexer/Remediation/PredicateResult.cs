namespace PdfLexer.Remediation;

using PdfLexer.Content;
using PdfLexer.Content.Model;
using PdfLexer.DOM;

/// <summary>
/// Result of evaluating a remediation predicate.
/// </summary>
public readonly record struct PredicateResult(bool IsMatch, double Confidence, string? Reason = null)
{
    /// <summary>Creates a matching predicate result.</summary>
    public static PredicateResult Match(double confidence = 1.0) => new(true, Clamp(confidence));

    /// <summary>Creates a non-matching predicate result.</summary>
    public static PredicateResult NoMatch(string? reason = null, double confidence = 1.0) => new(false, Clamp(confidence), reason);

    private static double Clamp(double value) => Math.Min(1.0, Math.Max(0.0, value));
}

/// <summary>
/// Runtime context used to evaluate candidate predicates and resolve anchors, zones, and flow regions.
/// </summary>
public sealed class RemediationEvaluationContext
{
    /// <summary>Creates an evaluation context.</summary>
    public RemediationEvaluationContext(
        IReadOnlyList<RemediationClaim>? claims = null,
        IReadOnlyDictionary<string, IReadOnlyList<RemediationClaim>>? claimsByRuleId = null,
        PdfRect<double>? pageBox = null,
        int pageIndex = -1,
        int pageCount = 0,
        RemediationSessionConfiguration? configuration = null,
        IReadOnlyDictionary<string, RemediationAnchor>? anchors = null,
        IReadOnlyDictionary<string, TolerancedZone>? tolerancedZones = null,
        IReadOnlyDictionary<string, FlowRegion>? flowRegions = null,
        IReadOnlyDictionary<string, AnchorResolution>? resolvedAnchors = null,
        IReadOnlyDictionary<string, TolerancedZoneResolution>? resolvedZones = null,
        IReadOnlyDictionary<string, FlowRegionResolution>? resolvedFlowRegions = null,
        StructuredTextPage? structuredText = null,
        List<string>? diagnostics = null)
    {
        Claims = claims ?? Array.Empty<RemediationClaim>();
        ClaimsByRuleId = claimsByRuleId ?? BuildClaimLookup(Claims);
        PageBox = pageBox ?? new PdfRect<double>(0, 0, 0, 0);
        PageIndex = pageIndex;
        PageCount = pageCount;
        Configuration = configuration ?? new RemediationSessionConfiguration();
        Anchors = anchors ?? new Dictionary<string, RemediationAnchor>();
        TolerancedZones = tolerancedZones ?? new Dictionary<string, TolerancedZone>();
        FlowRegions = flowRegions ?? new Dictionary<string, FlowRegion>();
        ResolvedAnchors = resolvedAnchors ?? new Dictionary<string, AnchorResolution>();
        ResolvedZones = resolvedZones ?? new Dictionary<string, TolerancedZoneResolution>();
        ResolvedFlowRegions = resolvedFlowRegions ?? new Dictionary<string, FlowRegionResolution>();
        StructuredText = structuredText;
        Diagnostics = diagnostics ?? new List<string>();
        _anchorResolver = new Lazy<AnchorResolver>(() => new AnchorResolver(this, Diagnostics));
        _flowRegionResolver = new Lazy<FlowRegionResolver>(() => new FlowRegionResolver(this, Diagnostics));
    }

    /// <summary>Empty evaluation context.</summary>
    public static RemediationEvaluationContext Empty { get; } = new();

    /// <summary>Claims visible to the current evaluation stage.</summary>
    public IReadOnlyList<RemediationClaim> Claims { get; }

    /// <summary>Claims grouped by rule id.</summary>
    public IReadOnlyDictionary<string, IReadOnlyList<RemediationClaim>> ClaimsByRuleId { get; }

    /// <summary>Current page box in normalized layout coordinates.</summary>
    public PdfRect<double> PageBox { get; }

    /// <summary>Zero-based page index.</summary>
    public int PageIndex { get; }

    /// <summary>Total page count for the document.</summary>
    public int PageCount { get; }

    /// <summary>Session configuration.</summary>
    public RemediationSessionConfiguration Configuration { get; }

    /// <summary>Declared anchors keyed by id.</summary>
    public IReadOnlyDictionary<string, RemediationAnchor> Anchors { get; }

    /// <summary>Declared toleranced zones keyed by id.</summary>
    public IReadOnlyDictionary<string, TolerancedZone> TolerancedZones { get; }

    /// <summary>Declared flow regions keyed by id.</summary>
    public IReadOnlyDictionary<string, FlowRegion> FlowRegions { get; }

    /// <summary>Pre-resolved anchors keyed by id.</summary>
    public IReadOnlyDictionary<string, AnchorResolution> ResolvedAnchors { get; }

    /// <summary>Pre-resolved zones keyed by id.</summary>
    public IReadOnlyDictionary<string, TolerancedZoneResolution> ResolvedZones { get; }

    /// <summary>Pre-resolved flow regions keyed by id.</summary>
    public IReadOnlyDictionary<string, FlowRegionResolution> ResolvedFlowRegions { get; }

    /// <summary>Structured text for the current page.</summary>
    public StructuredTextPage? StructuredText { get; }

    /// <summary>Diagnostics collected during evaluation.</summary>
    public List<string> Diagnostics { get; }

    private readonly Lazy<AnchorResolver> _anchorResolver;

    private readonly Lazy<FlowRegionResolver> _flowRegionResolver;

    /// <summary>Resolves a named anchor for the current page.</summary>
    public AnchorResolution? ResolveAnchor(string anchorId)
    {
        if (ResolvedAnchors.TryGetValue(anchorId, out var resolution))
        {
            return resolution;
        }

        return _anchorResolver.Value.Resolve(anchorId);
    }

    /// <summary>Resolves a declared toleranced zone for the current page.</summary>
    public TolerancedZoneResolution? ResolveTolerancedZone(string zoneId)
    {
        if (ResolvedZones.TryGetValue(zoneId, out var resolution))
        {
            return resolution;
        }

        if (!TolerancedZones.TryGetValue(zoneId, out var zone))
        {
            Diagnostics.Add($"Toleranced zone '{zoneId}' is not declared.");
            return null;
        }

        var baseBounds = zone.Bounds.Resolve(this, new RemediationCandidate(
            Granularity.Line,
            string.Empty,
            new PdfRect<double>(0, 0, 0, 0),
            new PdfRect<double>(0, 0, 0, 0),
            Array.Empty<StructuredCharacter>(),
            Array.Empty<StructuredSourceRef>(),
            0,
            0));
        var toleranced = zone.Tolerance == 0 ? baseBounds : baseBounds.Expand(zone.Tolerance);
        return new TolerancedZoneResolution(zone.Id, baseBounds, toleranced, zone.Tolerance, zone.ConfidenceBehavior);
    }

    /// <summary>Resolves a declared flow region for the current page.</summary>
    public FlowRegionResolution? ResolveFlowRegion(string regionId) =>
        ResolvedFlowRegions.TryGetValue(regionId, out var resolution)
            ? resolution
            : _flowRegionResolver.Value.Resolve(regionId);

    private static IReadOnlyDictionary<string, IReadOnlyList<RemediationClaim>> BuildClaimLookup(
        IReadOnlyList<RemediationClaim> claims)
    {
        return claims
            .GroupBy(x => x.RuleId, StringComparer.Ordinal)
            .ToDictionary(x => x.Key, x => (IReadOnlyList<RemediationClaim>)x.ToList(), StringComparer.Ordinal);
    }
}

/// <summary>
/// Engine claim produced by remediation rule evaluation.
/// </summary>
public sealed record RemediationClaim(
    string RuleId,
    Granularity Granularity,
    IReadOnlyList<RemediationCandidate> Candidates,
    string Tag,
    double Confidence = 1.0)
{
    /// <summary>Serialized selector or predicate description.</summary>
    public string SelectorDebugString { get; init; } = string.Empty;
    private readonly List<RemediationAppliedBinding> _appliedBindings = new();
    private readonly List<RemediationClaim> _relatedClaims = new();

    /// <summary>Stable claim identifier.</summary>
    public ClaimId ClaimId { get; init; } = ClaimId.New();

    /// <summary>Rule-set origin, when available.</summary>
    public string? RuleSetId { get; init; }

    /// <summary>Zero-based page index.</summary>
    public int PageIndex { get; init; } = -1;

    /// <summary>Lifecycle status for the claim.</summary>
    public ClaimStatus Status { get; init; } = ClaimStatus.Applied;

    /// <summary>Action associated with the claim.</summary>
    public RemediationAction? Action { get; init; }

    /// <summary>Action kind associated with the claim.</summary>
    public RemediationActionKind? ActionKind => Action?.Kind;

    /// <summary>Structure tag or artifact marker produced by the claim.</summary>
    public string ProducedTag => Action switch
    {
        TagRemediationAction tag => tag.Name.Value,
        ArtifactRemediationAction => "Artifact",
        TableRemediationAction => "Table",
        GroupRemediationAction group => group.ParentTag.Value,
        MergeRemediationAction merge => merge.TargetTag.Value,
        _ => Tag
    };

    /// <summary>Marked-content and structure bindings created for this claim.</summary>
    public IReadOnlyList<RemediationAppliedBinding> AppliedBindings => _appliedBindings;

    /// <summary>Existing claims consumed by this claim.</summary>
    public IReadOnlyList<RemediationClaim> RelatedClaims => _relatedClaims;

    /// <summary>Inline text ranges selected by this claim.</summary>
    public IReadOnlyList<RemediationTextRange> TextRanges { get; init; } =
        Candidates.SelectMany(x => x.TextRanges).ToArray();

    /// <summary>Union bounds for selected candidates.</summary>
    public PdfRect<double>? BoundingBox => Candidates.Count == 0 ? null : Union(Candidates.Select(x => x.BoundingBox));

    /// <summary>First selected candidate sequence index.</summary>
    public int FirstSequenceIndex => Candidates.Count == 0 ? int.MaxValue : Candidates.Min(x => x.SequenceIndex);

    /// <summary>Last selected candidate sequence index.</summary>
    public int LastSequenceIndex => Candidates.Count == 0 ? int.MinValue : Candidates.Max(x => x.SequenceIndex);

    private static PdfRect<double> Union(IEnumerable<PdfRect<double>> rects)
    {
        using var enumerator = rects.GetEnumerator();
        if (!enumerator.MoveNext())
        {
            return new PdfRect<double>(0, 0, 0, 0);
        }

        var result = enumerator.Current;
        while (enumerator.MoveNext())
        {
            var rect = enumerator.Current;
            result = new PdfRect<double>(
                Math.Min(result.LLx, rect.LLx),
                Math.Min(result.LLy, rect.LLy),
                Math.Max(result.URx, rect.URx),
                Math.Max(result.URy, rect.URy));
        }

        return result;
    }

    internal void AddAppliedBinding(RemediationAppliedBinding binding)
    {
        _appliedBindings.Add(binding);
    }

    internal void AddRelatedClaims(IEnumerable<RemediationClaim> claims)
    {
        _relatedClaims.AddRange(claims);
    }
}

/// <summary>
/// Binding between a remediation claim, marked content, and structure nodes.
/// </summary>
public sealed record RemediationAppliedBinding(
    string ProducedTag,
    IReadOnlyList<int> Mcids,
    StructureNode? StructureNode,
    MarkedContentGroup<double>? MarkedContentGroup,
    StructureNode? ParentStructureNode,
    IReadOnlyList<StructuredSourceRef> SourceReferences,
    PdfRect<double>? Bounds);

/// <summary>
/// Stable remediation claim identifier.
/// </summary>
public readonly record struct ClaimId(string Value)
{
    /// <summary>Creates a new claim identifier.</summary>
    public static ClaimId New() => new(Guid.NewGuid().ToString("N"));

    public override string ToString() => Value;
}

/// <summary>
/// Claim lifecycle status.
/// </summary>
public enum ClaimStatus
{
    /// <summary>Candidate claim not yet applied.</summary>
    Candidate,
    /// <summary>Claim was applied.</summary>
    Applied,
    /// <summary>Claim was skipped.</summary>
    Skipped,
    /// <summary>Claim conflicted with another claim.</summary>
    Conflicted,
    /// <summary>Claim was overridden by a later rule.</summary>
    Overridden,
    /// <summary>Claim failed validation or application.</summary>
    Failed
}
