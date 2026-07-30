using PdfLexer.Content;

namespace PdfLexer.Remediation;

/// <summary>
/// Named region that flows from one boundary to another in reading/layout order.
/// </summary>
public sealed record FlowRegion(
    /// <summary>Stable flow-region identifier.</summary>
    string Id,
    /// <summary>Start boundary for the flowing region.</summary>
    FlowBoundary Start,
    /// <summary>End boundary for the flowing region.</summary>
    FlowBoundary End,
    /// <summary>Optional maximum extent from the start boundary.</summary>
    double? MaxExtent = null,
    /// <summary>Policy for crossing page boundaries.</summary>
    FlowContinuationPolicy ContinuationPolicy = FlowContinuationPolicy.CurrentPageOnly,
    /// <summary>Reading order mode used to select region candidates.</summary>
    FlowReadingOrderMode ReadingOrderMode = FlowReadingOrderMode.StructuredText,
    /// <summary>
    /// Optional inclusive page limit for a continued activation. The page containing the
    /// start boundary counts as page one.
    /// </summary>
    int? MaxPages = null)
{
    /// <summary>Validates declaration shape.</summary>
    public IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(Id))
        {
            errors.Add("Flow region id is required.");
        }

        if (MaxExtent is < 0)
        {
            errors.Add($"Flow region '{Id}' has negative max extent.");
        }
        if (ContinuationPolicy == FlowContinuationPolicy.ContinueUntilEnd && MaxExtent != null)
        {
            errors.Add($"Flow region '{Id}' cannot use max extent with ContinueUntilEnd.");
        }
        if (ContinuationPolicy == FlowContinuationPolicy.CurrentPageOnly && MaxPages != null)
        {
            errors.Add($"Flow region '{Id}' can only use max pages with ContinueUntilEnd.");
        }
        if (MaxPages is <= 0)
        {
            errors.Add($"Flow region '{Id}' max pages must be greater than zero.");
        }

        errors.AddRange(Start.Validate($"flow region '{Id}' start"));
        errors.AddRange(End.Validate($"flow region '{Id}' end"));
        return errors;
    }
}

/// <summary>
/// Boundary used to start or end a flow region.
/// </summary>
public abstract record FlowBoundary
{
    /// <summary>Creates a boundary from a named anchor.</summary>
    public static FlowBoundary Anchor(string anchorId) => new AnchorFlowBoundary(anchorId);

    /// <summary>Creates a boundary from a toleranced zone.</summary>
    public static FlowBoundary Zone(string zoneId) => new ZoneFlowBoundary(zoneId);

    /// <summary>Creates a boundary from the first candidate matching a predicate.</summary>
    public static FlowBoundary Matching(RemediationPredicate predicate) => new PredicateFlowBoundary(predicate);

    /// <summary>Boundary representing the page edge.</summary>
    public static FlowBoundary PageBoundary { get; } = new PageBoundaryFlowBoundary();

    internal abstract IReadOnlyList<string> Validate(string label);
}

/// <summary>Flow boundary resolved from a named anchor.</summary>
public sealed record AnchorFlowBoundary(string AnchorId) : FlowBoundary
{
    internal override IReadOnlyList<string> Validate(string label) =>
        string.IsNullOrWhiteSpace(AnchorId)
            ? new[] { $"{label} anchor id is required." }
            : Array.Empty<string>();
}

/// <summary>Flow boundary resolved from a toleranced zone.</summary>
public sealed record ZoneFlowBoundary(string ZoneId) : FlowBoundary
{
    internal override IReadOnlyList<string> Validate(string label) =>
        string.IsNullOrWhiteSpace(ZoneId)
            ? new[] { $"{label} zone id is required." }
            : Array.Empty<string>();
}

/// <summary>Flow boundary resolved from the first candidate matching a predicate.</summary>
public sealed record PredicateFlowBoundary(RemediationPredicate Predicate) : FlowBoundary
{
    internal override IReadOnlyList<string> Validate(string label) =>
        Predicate == null
            ? new[] { $"{label} predicate is required." }
            : Array.Empty<string>();
}

/// <summary>Flow boundary at the page edge.</summary>
public sealed record PageBoundaryFlowBoundary : FlowBoundary
{
    internal override IReadOnlyList<string> Validate(string label) => Array.Empty<string>();
}

/// <summary>
/// Page continuation policy for flow regions.
/// </summary>
public enum FlowContinuationPolicy
{
    /// <summary>Flow region is bounded to the current page.</summary>
    CurrentPageOnly,
    /// <summary>Flow region may continue until an end boundary is found.</summary>
    ContinueUntilEnd
}

/// <summary>
/// Candidate ordering mode used for flow-region evaluation.
/// </summary>
public enum FlowReadingOrderMode
{
    /// <summary>Use structured-text reading order.</summary>
    StructuredText,
    /// <summary>Use top-to-bottom geometric order.</summary>
    GeometryTopToBottom
}

/// <summary>Stable identity of one document-order activation of a flow region.</summary>
public readonly record struct FlowRegionInstanceId(string RegionId, int ActivationIndex)
{
    public override string ToString() => $"{RegionId}#{ActivationIndex}";
}

/// <summary>Role played by one page in a resolved flow-region activation.</summary>
public enum FlowRegionPageRole
{
    /// <summary>The activation starts on this page and continues.</summary>
    Start,
    /// <summary>The activation continues through this page.</summary>
    Continue,
    /// <summary>The activation ends on this page after starting on an earlier page.</summary>
    End,
    /// <summary>The activation starts and ends on this page.</summary>
    StartAndEnd
}

/// <summary>
/// Per-page resolved flow-region bounds and sequence limits.
/// </summary>
public sealed record FlowRegionResolution(
    /// <summary>Resolved flow-region identifier.</summary>
    string RegionId,
    /// <summary>Resolved region bounds.</summary>
    PdfRect<double> Bounds,
    /// <summary>First sequence index excluded from the region.</summary>
    int StartSequenceIndex,
    /// <summary>End sequence index excluded from the region.</summary>
    int EndSequenceIndex,
    /// <summary>Resolution confidence in the range [0, 1].</summary>
    double Confidence,
    /// <summary>Zero-based page index containing this segment.</summary>
    int PageIndex = -1,
    /// <summary>Activation containing this segment.</summary>
    FlowRegionInstanceId InstanceId = default,
    /// <summary>Role of this page within the activation.</summary>
    FlowRegionPageRole PageRole = FlowRegionPageRole.StartAndEnd,
    /// <summary>Ordering used for candidates in this activation.</summary>
    FlowReadingOrderMode ReadingOrderMode = FlowReadingOrderMode.StructuredText)
{
    /// <summary>Returns true when a candidate falls inside the sequence and geometry bounds.</summary>
    public bool Contains(RemediationCandidate candidate) =>
        (PageIndex < 0 || candidate.PageIndex < 0 || candidate.PageIndex == PageIndex) &&
        candidate.SequenceIndex > StartSequenceIndex &&
        candidate.SequenceIndex < EndSequenceIndex &&
        Bounds.CheckEnclosure(candidate.RelativeBoundingBox) == EncloseType.Full;
}
