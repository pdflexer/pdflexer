using System.Collections.ObjectModel;

namespace PdfLexer.Remediation;

/// <summary>A named, page-scoped region expression used by a remediation program.</summary>
public sealed record RegionDeclaration
{
    public RegionDeclaration(string id, Region expression, PageSelector? pages = null)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Region id is required.", nameof(id));
        Id = id;
        Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        Pages = pages ?? PageSelector.Every;
    }

    public string Id { get; }
    public Region Expression { get; }
    public PageSelector Pages { get; }
}

/// <summary>Closed algebra of fixed, anchor-derived, flowing, and toleranced regions.</summary>
public abstract record Region
{
    private Region() { }

    /// <summary>A page-relative rectangle resolved from a layout coordinate.</summary>
    public sealed record Fixed : Region
    {
        public Fixed(LayoutCoord bounds) => Bounds = bounds ?? throw new ArgumentNullException(nameof(bounds));
        public LayoutCoord Bounds { get; }
    }

    /// <summary>A rectangle projected from a named anchor.</summary>
    public sealed record Anchored : Region
    {
        public Anchored(string anchorId, RegionPlacement placement, double extent)
        {
            if (string.IsNullOrWhiteSpace(anchorId)) throw new ArgumentException("Anchor id is required.", nameof(anchorId));
            if (!Enum.IsDefined(placement)) throw new ArgumentOutOfRangeException(nameof(placement));
            AnchorId = anchorId;
            Placement = placement;
            Extent = extent;
        }

        public string AnchorId { get; init; }
        public RegionPlacement Placement { get; }
        public double Extent { get; }
    }

    /// <summary>A document-order region delimited by start and end boundary evidence.</summary>
    public sealed record Flow : Region
    {
        public Flow(
            RegionBoundary start,
            RegionBoundary end,
            FlowContinuationPolicy continuationPolicy = FlowContinuationPolicy.CurrentPageOnly,
            FlowReadingOrderMode readingOrderMode = FlowReadingOrderMode.StructuredText,
            double? maxExtent = null,
            int? maxPages = null)
        {
            Start = start ?? throw new ArgumentNullException(nameof(start));
            End = end ?? throw new ArgumentNullException(nameof(end));
            if (!Enum.IsDefined(continuationPolicy)) throw new ArgumentOutOfRangeException(nameof(continuationPolicy));
            if (!Enum.IsDefined(readingOrderMode)) throw new ArgumentOutOfRangeException(nameof(readingOrderMode));
            ContinuationPolicy = continuationPolicy;
            ReadingOrderMode = readingOrderMode;
            MaxExtent = maxExtent;
            MaxPages = maxPages;
        }

        public RegionBoundary Start { get; init; }
        public RegionBoundary End { get; init; }
        public FlowContinuationPolicy ContinuationPolicy { get; }
        public FlowReadingOrderMode ReadingOrderMode { get; }
        public double? MaxExtent { get; }
        public int? MaxPages { get; }
    }

    /// <summary>A tolerance decoration over any non-toleranced region expression.</summary>
    public sealed record Tolerance : Region
    {
        public Tolerance(Region inner, double amount,
            ZoneConfidenceBehavior confidenceBehavior = ZoneConfidenceBehavior.DegradeOutsideBaseBounds)
        {
            Inner = inner ?? throw new ArgumentNullException(nameof(inner));
            if (!Enum.IsDefined(confidenceBehavior)) throw new ArgumentOutOfRangeException(nameof(confidenceBehavior));
            Amount = amount;
            ConfidenceBehavior = confidenceBehavior;
        }

        public Region Inner { get; init; }
        public double Amount { get; }
        public ZoneConfidenceBehavior ConfidenceBehavior { get; }
    }
}

/// <summary>How an anchor's bounds are projected into a region.</summary>
public enum RegionPlacement
{
    Above,
    Below,
    LeftOf,
    RightOf,
    Around
}

/// <summary>Boundary evidence for a flowing region.</summary>
public abstract record RegionBoundary
{
    private RegionBoundary() { }

    public sealed record Anchor : RegionBoundary
    {
        public Anchor(string anchorId)
        {
            if (string.IsNullOrWhiteSpace(anchorId)) throw new ArgumentException("Anchor id is required.", nameof(anchorId));
            AnchorId = anchorId;
        }
        public string AnchorId { get; init; }
    }

    public sealed record Region : RegionBoundary
    {
        public Region(string regionId)
        {
            if (string.IsNullOrWhiteSpace(regionId)) throw new ArgumentException("Region id is required.", nameof(regionId));
            RegionId = regionId;
        }
        public string RegionId { get; init; }
    }

    public sealed record Matching : RegionBoundary
    {
        public Matching(CandidateSelector candidates, RemediationPredicate? predicate = null)
        {
            Candidates = candidates ?? throw new ArgumentNullException(nameof(candidates));
            Predicate = predicate ?? RemediationPredicate.Always;
        }
        public CandidateSelector Candidates { get; }
        public RemediationPredicate Predicate { get; init; }
    }

    public sealed record Page : RegionBoundary
    {
        public static Page Instance { get; } = new();
        private Page() { }
    }
}

/// <summary>Guarded absorption of eligible painting content inside a named region.</summary>
public sealed record RegionArtifactAccounting
{
    public RegionArtifactAccounting(
        string id,
        string regionId,
        string artifactId,
        IEnumerable<RemediationCandidateKind>? candidateKinds = null,
        bool allowText = false,
        RemediationPredicate? textPredicate = null,
        int priority = 0)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Region accounting id is required.", nameof(id));
        if (string.IsNullOrWhiteSpace(regionId)) throw new ArgumentException("Region id is required.", nameof(regionId));
        if (string.IsNullOrWhiteSpace(artifactId)) throw new ArgumentException("Artifact id is required.", nameof(artifactId));
        Id = id;
        RegionId = regionId;
        ArtifactId = artifactId;
        CandidateKinds = new HashSet<RemediationCandidateKind>(candidateKinds ?? Array.Empty<RemediationCandidateKind>());
        AllowText = allowText;
        TextPredicate = textPredicate;
        Priority = priority;
    }

    public string Id { get; }
    public string RegionId { get; }
    public string ArtifactId { get; }
    public IReadOnlySet<RemediationCandidateKind> CandidateKinds { get; }
    public bool AllowText { get; }
    public RemediationPredicate? TextPredicate { get; }
    public int Priority { get; }
}
