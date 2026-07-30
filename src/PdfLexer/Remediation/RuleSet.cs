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
    public RuleSet(
        string id,
        IEnumerable<Rule> rules,
        TextNormalizationOptions? textNormalization = null,
        RemediationStructuralTemplate? structuralTemplate = null,
        IEnumerable<RemediationArtifactInventoryItem>? artifacts = null)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Rule set id is required.", nameof(id));
        }

        Id = id;
        TextNormalization = textNormalization ?? TextNormalizationOptions.Default;
        StructuralTemplate = structuralTemplate;
        Artifacts = new ReadOnlyCollection<RemediationArtifactInventoryItem>(
            (artifacts ?? Array.Empty<RemediationArtifactInventoryItem>())
            .Select(item => item with { RuleSetId = id })
            .ToList());
        Rules = new ReadOnlyCollection<Rule>(
            (rules ?? throw new ArgumentNullException(nameof(rules)))
            .Select(rule => rule with
            {
                RuleSetId = id,
                TextNormalization = rule.TextNormalization ?? TextNormalization
            })
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
        IEnumerable<FlowRegion>? flowRegions = null,
        TextNormalizationOptions? textNormalization = null,
        IEnumerable<RemediationSemanticAssertion>? assertions = null,
        RemediationStructuralTemplate? structuralTemplate = null,
        IEnumerable<RemediationArtifactInventoryItem>? artifacts = null)
        : this(id, rules, textNormalization, structuralTemplate, artifacts)
    {
        Anchors = new ReadOnlyCollection<RemediationAnchor>(
            (anchors ?? throw new ArgumentNullException(nameof(anchors)))
            .Select(anchor => anchor with
            {
                RuleSetId = id,
                TextNormalization = TextNormalization
            })
            .ToList());
        TolerancedZones = new ReadOnlyCollection<TolerancedZone>(
            (tolerancedZones ?? Array.Empty<TolerancedZone>()).ToList());
        FlowRegions = new ReadOnlyCollection<FlowRegion>(
            (flowRegions ?? Array.Empty<FlowRegion>()).ToList());
        Assertions = new ReadOnlyCollection<RemediationSemanticAssertion>(
            (assertions ?? Array.Empty<RemediationSemanticAssertion>()).ToList());
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

    /// <summary>Default text normalization inherited by rules that do not specify an override.</summary>
    public TextNormalizationOptions TextNormalization { get; }

    /// <summary>Semantic expectations evaluated against planned and materialized output.</summary>
    public IReadOnlyList<RemediationSemanticAssertion> Assertions { get; } =
        Array.Empty<RemediationSemanticAssertion>();

    /// <summary>Optional closed structural template owned by this rule set.</summary>
    public RemediationStructuralTemplate? StructuralTemplate { get; }

    /// <summary>
    /// Declared page furniture contributed by this rule set. Items from every composed rule set merge
    /// into one effective inventory; once any set declares an item, every produced artifact must match
    /// something declared.
    /// </summary>
    public IReadOnlyList<RemediationArtifactInventoryItem> Artifacts { get; } =
        Array.Empty<RemediationArtifactInventoryItem>();
}
