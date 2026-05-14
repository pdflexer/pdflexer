using System.Collections.ObjectModel;

namespace PdfLexer.Remediation;

/// <summary>
/// Named, ordered bundle of remediation rules and reusable rule metadata.
/// </summary>
public sealed class RuleSet
{
    /// <summary>
    /// Creates a rule set with rules only.
    /// </summary>
    public RuleSet(string id, IEnumerable<Rule> rules)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Rule set id is required.", nameof(id));
        }

        Id = id;
        Rules = new ReadOnlyCollection<Rule>(
            (rules ?? throw new ArgumentNullException(nameof(rules)))
            .Select(rule => rule with { RuleSetId = id })
            .ToList());
    }

    /// <summary>
    /// Creates a rule set with rules only.
    /// </summary>
    public RuleSet(string id, params Rule[] rules)
        : this(id, (IEnumerable<Rule>)rules)
    {
    }

    /// <summary>
    /// Creates a rule set with rules plus named anchors, toleranced zones, and flow regions.
    /// </summary>
    public RuleSet(
        string id,
        IEnumerable<Rule> rules,
        IEnumerable<RemediationAnchor> anchors,
        IEnumerable<TolerancedZone>? tolerancedZones = null,
        IEnumerable<FlowRegion>? flowRegions = null)
        : this(id, rules)
    {
        Anchors = new ReadOnlyCollection<RemediationAnchor>(
            (anchors ?? throw new ArgumentNullException(nameof(anchors))).ToList());
        TolerancedZones = new ReadOnlyCollection<TolerancedZone>(
            (tolerancedZones ?? Array.Empty<TolerancedZone>()).ToList());
        FlowRegions = new ReadOnlyCollection<FlowRegion>(
            (flowRegions ?? Array.Empty<FlowRegion>()).ToList());
    }

    /// <summary>Stable rule-set identifier used for composition and report provenance.</summary>
    public string Id { get; }

    /// <summary>Rules in deterministic evaluation order.</summary>
    public IReadOnlyList<Rule> Rules { get; }

    /// <summary>Named anchors available to rules in this set.</summary>
    public IReadOnlyList<RemediationAnchor> Anchors { get; } = Array.Empty<RemediationAnchor>();

    /// <summary>Tolerance-aware zones available to rules in this set.</summary>
    public IReadOnlyList<TolerancedZone> TolerancedZones { get; } = Array.Empty<TolerancedZone>();

    /// <summary>Flow regions available to rules in this set.</summary>
    public IReadOnlyList<FlowRegion> FlowRegions { get; } = Array.Empty<FlowRegion>();
}
