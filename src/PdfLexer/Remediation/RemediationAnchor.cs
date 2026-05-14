using System;
using System.Collections.Generic;
using System.Linq;
using PdfLexer.Content;

namespace PdfLexer.Remediation;

/// <summary>
/// Named location or region that can be referenced by remediation predicates and layout coordinates.
/// </summary>
public abstract record RemediationAnchor
{
    /// <summary>Stable anchor identifier.</summary>
    public abstract string Id { get; init; }

    /// <summary>Pages on which the anchor can resolve.</summary>
    public PageSelector Pages { get; init; } = PageSelector.Every;

    /// <summary>
    /// One-based occurrence to select after page/style/neighbor filtering.
    /// </summary>
    public int? Occurrence { get; init; }

    /// <summary>Optional style predicate used to disambiguate candidate anchor matches.</summary>
    public RemediationPredicate? Style { get; init; }

    /// <summary>Optional neighboring text used to disambiguate candidate anchor matches.</summary>
    public string? NeighborText { get; init; }

    /// <summary>Maximum distance for matching <see cref="NeighborText"/>.</summary>
    public double NeighborTolerance { get; init; } = 24;
    
    /// <summary>Debug representation used in validation and reports.</summary>
    public abstract string DebugString { get; }

    /// <summary>Creates an anchor by evaluating a predicate over candidates at one granularity.</summary>
    public static RemediationAnchor Selector(
        string id,
        Granularity granularity,
        RemediationPredicate predicate,
        AnchorSelection? selection = null) =>
        new PredicateAnchor(id, new[] { granularity }, predicate, selection ?? AnchorSelection.RequiredSingle);

    /// <summary>Creates an anchor by evaluating a predicate over candidates at one or more granularities.</summary>
    public static RemediationAnchor Selector(
        string id,
        IEnumerable<Granularity> granularities,
        RemediationPredicate predicate,
        AnchorSelection? selection = null) =>
        new PredicateAnchor(id, granularities.ToArray(), predicate, selection ?? AnchorSelection.RequiredSingle);

    /// <summary>Creates an anchor from a regular expression over candidate text.</summary>
    public static RemediationAnchor Regex(
        string id,
        string pattern,
        Granularity granularity = Granularity.Line,
        AnchorSelection? selection = null) =>
        Selector(id, granularity, Predicates.Text.Matches(pattern), selection);

    /// <summary>Creates an anchor from a text label.</summary>
    public static RemediationAnchor TextLabel(string id, string text, StringComparison comparison = StringComparison.Ordinal) =>
        new TextLabelAnchor(id, text, comparison);

    /// <summary>Creates an anchor from a previously produced rule claim.</summary>
    public static RemediationAnchor PriorClaim(string id, string ruleId) =>
        new PriorClaimAnchor(id, ruleId);

    /// <summary>Creates an anchor from a previously produced rule claim.</summary>
    public static RemediationAnchor FromPriorClaim(string id, string ruleId) => PriorClaim(id, ruleId);

    /// <summary>Creates an anchor from a configured named layout zone.</summary>
    public static RemediationAnchor DeclaredZone(string id, NamedLayoutZone zone) =>
        new DeclaredZoneAnchor(id, zone);

    /// <summary>Creates an anchor from a line containing table header text.</summary>
    public static RemediationAnchor TableHeader(string id, params string[] headers) =>
        new TableHeaderAnchor(id, headers);

    /// <summary>Creates an anchor from repeated text matched by a regular expression.</summary>
    public static RemediationAnchor RepeatedElement(string id, string pattern) =>
        new RepeatedElementAnchor(id, pattern);

    /// <summary>Creates an anchor from a fixed page-relative rectangle.</summary>
    public static RemediationAnchor Geometry(string id, PdfRect<double> bounds) =>
        new GeometryAnchor(id, bounds);
}

/// <summary>Anchor resolved by applying a remediation predicate to structured-text candidates.</summary>
public sealed record PredicateAnchor(
    string Id,
    IReadOnlyList<Granularity> Granularities,
    RemediationPredicate Predicate,
    AnchorSelection Selection) : RemediationAnchor
{
    public override string DebugString => $"PredicateAnchor({Id}, {Predicate.DebugString}, {Selection.DebugString})";
}

/// <summary>Anchor resolved from exact or comparison-based text label matching.</summary>
public sealed record TextLabelAnchor(string Id, string Text, StringComparison Comparison) : RemediationAnchor
{
    public override string DebugString => $"TextLabelAnchor({Id}, '{Text}')";
}

/// <summary>Anchor resolved from claims produced by another rule.</summary>
public sealed record PriorClaimAnchor(string Id, string RuleId) : RemediationAnchor
{
    public override string DebugString => $"PriorClaimAnchor({Id}, Rule='{RuleId}')";
}

/// <summary>Anchor resolved from a built-in named layout zone.</summary>
public sealed record DeclaredZoneAnchor(string Id, NamedLayoutZone Zone) : RemediationAnchor
{
    public override string DebugString => $"DeclaredZoneAnchor({Id}, {Zone})";
}

/// <summary>Anchor resolved from a line containing all declared table headers.</summary>
public sealed record TableHeaderAnchor(string Id, IReadOnlyList<string> Headers) : RemediationAnchor
{
    public override string DebugString => $"TableHeaderAnchor({Id}, [{string.Join(", ", Headers)}])";
}

/// <summary>Anchor resolved from repeated text matched by a regular expression.</summary>
public sealed record RepeatedElementAnchor(string Id, string Pattern) : RemediationAnchor
{
    public override string DebugString => $"RepeatedElementAnchor({Id}, '{Pattern}')";
}

/// <summary>Anchor resolved from a fixed page-relative rectangle.</summary>
public sealed record GeometryAnchor(string Id, PdfRect<double> Bounds) : RemediationAnchor
{
    public override string DebugString => $"GeometryAnchor({Id}, {Bounds})";
}

/// <summary>
/// Resolved anchor bounds and confidence for a page.
/// </summary>
public sealed record AnchorResolution(
    /// <summary>Resolved anchor identifier.</summary>
    string AnchorId,
    /// <summary>Resolved page-relative bounds.</summary>
    PdfRect<double> Bounds,
    /// <summary>Resolution confidence in the range [0, 1].</summary>
    double Confidence,
    /// <summary>Zero-based page index on which the anchor resolved.</summary>
    int PageIndex,
    /// <summary>Structured candidates that produced the anchor, when applicable.</summary>
    IReadOnlyList<RemediationCandidate>? Candidates = null);

/// <summary>How a candidate anchor declaration chooses from matching candidates.</summary>
public sealed record AnchorSelection(
    AnchorSelectionMode Mode,
    int? Index = null,
    string? AnchorId = null,
    AnchorDirection Direction = AnchorDirection.Any,
    double? MaxDistance = null)
{
    /// <summary>Requires exactly one matching candidate.</summary>
    public static AnchorSelection RequiredSingle { get; } = new(AnchorSelectionMode.RequiredSingle);

    /// <summary>Allows zero or one matching candidate.</summary>
    public static AnchorSelection OptionalSingle { get; } = new(AnchorSelectionMode.OptionalSingle);

    /// <summary>Selects the first matching candidate in reading order.</summary>
    public static AnchorSelection FirstInReadingOrder { get; } = new(AnchorSelectionMode.FirstInReadingOrder);

    /// <summary>Selects the last matching candidate in reading order.</summary>
    public static AnchorSelection LastInReadingOrder { get; } = new(AnchorSelectionMode.LastInReadingOrder);

    /// <summary>Selects the zero-based nth matching candidate in reading order.</summary>
    public static AnchorSelection NthInReadingOrder(int index) => new(AnchorSelectionMode.NthInReadingOrder, Index: index);

    /// <summary>Selects the nearest matching candidate to another anchor.</summary>
    public static AnchorSelection NearestToAnchor(
        string anchorId,
        AnchorDirection direction = AnchorDirection.Any,
        double? maxDistance = null) =>
        new(AnchorSelectionMode.NearestToAnchor, AnchorId: anchorId, Direction: direction, MaxDistance: maxDistance);

    /// <summary>Debug representation used in validation and reports.</summary>
    public string DebugString => Mode switch
    {
        AnchorSelectionMode.NthInReadingOrder => $"NthInReadingOrder({Index})",
        AnchorSelectionMode.NearestToAnchor => $"NearestToAnchor({AnchorId}, {Direction}, max={MaxDistance})",
        _ => Mode.ToString()
    };
}

/// <summary>Anchor candidate selection mode.</summary>
public enum AnchorSelectionMode
{
    /// <summary>Exactly one match is required.</summary>
    RequiredSingle,
    /// <summary>Zero or one match is allowed.</summary>
    OptionalSingle,
    /// <summary>The first matching candidate in reading order is selected.</summary>
    FirstInReadingOrder,
    /// <summary>The last matching candidate in reading order is selected.</summary>
    LastInReadingOrder,
    /// <summary>The zero-based nth matching candidate in reading order is selected.</summary>
    NthInReadingOrder,
    /// <summary>The nearest matching candidate to another anchor is selected.</summary>
    NearestToAnchor
}

/// <summary>Directional constraint used by nearest and bounded anchor selectors.</summary>
public enum AnchorDirection
{
    /// <summary>No directional constraint.</summary>
    Any,
    /// <summary>Candidate must be left of the reference bounds.</summary>
    Left,
    /// <summary>Candidate must be right of the reference bounds.</summary>
    Right,
    /// <summary>Candidate must be above the reference bounds.</summary>
    Above,
    /// <summary>Candidate must be below the reference bounds.</summary>
    Below
}
