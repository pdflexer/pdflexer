using System.Collections.ObjectModel;

namespace PdfLexer.Remediation;

/// <summary>
/// Declarative remediation rule that selects candidates with a predicate and applies one remediation action.
/// </summary>
public sealed record Rule
{
    /// <summary>
    /// Creates a remediation rule.
    /// </summary>
    public Rule(
        string id,
        RemediationAction action,
        RemediationPredicate? predicate = null,
        Granularity granularity = Granularity.Paragraph,
        PageSelector? pages = null,
        Stage stage = Stage.Classify,
        bool @override = false,
        double? minConfidence = null,
        RuleCardinality? cardinality = null)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Rule id is required.", nameof(id));
        }

        if (minConfidence is < 0 or > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(minConfidence), "Minimum confidence must be between 0 and 1.");
        }

        Id = id;
        Stage = stage;
        Granularity = granularity;
        Pages = pages ?? PageSelector.Every;
        Predicate = predicate ?? RemediationPredicate.Always;
        Action = action ?? throw new ArgumentNullException(nameof(action));
        Override = @override;
        MinConfidence = minConfidence;
        Cardinality = cardinality;
        Candidates = CandidateSelector.Text(granularity);
    }

    /// <summary>Creates a classify rule with an explicit typed candidate selector.</summary>
    public Rule(
        string id,
        RemediationAction action,
        RemediationPredicate? predicate,
        CandidateSelector candidates,
        PageSelector? pages = null,
        Stage stage = Stage.Classify,
        bool @override = false,
        double? minConfidence = null,
        RuleCardinality? cardinality = null)
        : this(id, action, predicate,
            candidates is CandidateSelector.TextSelector text ? text.Granularity : Granularity.Paragraph,
            pages, stage, @override, minConfidence, cardinality)
    {
        Candidates = candidates ?? throw new ArgumentNullException(nameof(candidates));
    }

    /// <summary>Caller-supplied stable identifier used for provenance, validation, and reports.</summary>
    public string Id { get; }

    /// <summary>Identifier of the rule set that supplied this rule, when composed through <see cref="RuleSet"/>.</summary>
    public string? RuleSetId { get; init; }

    /// <summary>Text normalization inherited from the containing rule set.</summary>
    public TextNormalizationOptions? TextNormalization { get; init; }

    /// <summary>Pipeline stage in which the rule runs.</summary>
    public Stage Stage { get; init; }

    /// <summary>Structured-text granularity targeted by the rule.</summary>
    public Granularity Granularity { get; init; }

    /// <summary>Typed candidate selector. Required for classify rules.</summary>
    public CandidateSelector Candidates { get; init; }

    /// <summary>Pages eligible for this rule.</summary>
    public PageSelector Pages { get; init; }

    /// <summary>Candidate predicate used to select structured-text/content candidates.</summary>
    public RemediationPredicate Predicate { get; init; }

    /// <summary>Action applied to candidates or existing claims matched by the rule.</summary>
    public RemediationAction Action { get; init; }

    /// <summary>Whether this rule can replace claims produced by earlier rules in the same stage.</summary>
    public bool Override { get; init; }

    /// <summary>Optional minimum confidence required for a match to be applied.</summary>
    public double? MinConfidence { get; init; }

    /// <summary>Optional expected count of inputs matched by this rule.</summary>
    public RuleCardinality? Cardinality { get; init; }

    /// <summary>
    /// Validates rule shape that does not require page parsing.
    /// </summary>
    public IReadOnlyList<string> ValidateShape()
    {
        var errors = new List<string>();
        if (Stage == Stage.Classify && Candidates == null)
        {
            errors.Add("Classify rules require a candidate selector.");
        }
        if (Stage != Stage.Classify && Candidates is CandidateSelector.ContentSelector)
        {
            errors.Add("Group and refine rules do not enumerate content candidates.");
        }
        if (Candidates is CandidateSelector.TextSelector text && text.Granularity != Granularity)
        {
            errors.Add($"Granularity '{Granularity}' conflicts with candidate selector granularity '{text.Granularity}'.");
        }
        Action.Validate(this, errors);
        return new ReadOnlyCollection<string>(errors);
    }
}

/// <summary>
/// Fixed remediation pipeline stage.
/// </summary>
public enum Stage
{
    /// <summary>Classifies raw structured-text/content candidates into leaf-level claims.</summary>
    Classify,
    /// <summary>Consumes applied claims to build structural parents such as lists, sections, or tables.</summary>
    Group,
    /// <summary>Refines existing claims with attributes, links, or reading-order adjustments.</summary>
    Refine
}

/// <summary>
/// Structured-text candidate granularity used by a rule.
/// </summary>
public enum Granularity
{
    /// <summary>Single structured character.</summary>
    Character,
    /// <summary>Single structured word.</summary>
    Word,
    /// <summary>Single structured line.</summary>
    Line,
    /// <summary>Single structured paragraph.</summary>
    Paragraph
}
