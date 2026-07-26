namespace PdfLexer.Remediation;

/// <summary>
/// Expected number of inputs matched by a remediation rule.
/// </summary>
public sealed record RuleCardinality
{
    /// <summary>Creates a rule-cardinality constraint.</summary>
    public RuleCardinality(
        int minMatches,
        int? maxMatches = null,
        RuleCardinalityScope scope = RuleCardinalityScope.Document)
    {
        if (minMatches < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(minMatches));
        }

        if (maxMatches is < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxMatches));
        }

        if (maxMatches < minMatches)
        {
            throw new ArgumentOutOfRangeException(nameof(maxMatches), "Maximum matches must be greater than or equal to minimum matches.");
        }

        if (minMatches == 0 && maxMatches == null)
        {
            throw new ArgumentException("A cardinality constraint must declare a positive minimum or a maximum.");
        }

        MinMatches = minMatches;
        MaxMatches = maxMatches;
        Scope = scope;
    }

    /// <summary>Minimum accepted match count.</summary>
    public int MinMatches { get; }

    /// <summary>Maximum accepted match count, or null when unbounded.</summary>
    public int? MaxMatches { get; }

    /// <summary>Whether the constraint is evaluated per document or per selected page.</summary>
    public RuleCardinalityScope Scope { get; }

    /// <summary>Requires exactly <paramref name="matches"/> matches.</summary>
    public static RuleCardinality Exactly(int matches, RuleCardinalityScope scope = RuleCardinalityScope.Document) =>
        new(matches, matches, scope);

    /// <summary>Requires at least <paramref name="matches"/> matches.</summary>
    public static RuleCardinality AtLeast(int matches, RuleCardinalityScope scope = RuleCardinalityScope.Document) =>
        new(matches, null, scope);

    /// <summary>Requires at most <paramref name="matches"/> matches.</summary>
    public static RuleCardinality AtMost(int matches, RuleCardinalityScope scope = RuleCardinalityScope.Document) =>
        new(0, matches, scope);

    /// <summary>Requires a match count within the inclusive range.</summary>
    public static RuleCardinality Between(int minMatches, int maxMatches, RuleCardinalityScope scope = RuleCardinalityScope.Document) =>
        new(minMatches, maxMatches, scope);

    internal bool Accepts(int observed) =>
        observed >= MinMatches && (MaxMatches == null || observed <= MaxMatches.Value);

    internal string ExpectedDescription =>
        MaxMatches switch
        {
            null => $"at least {MinMatches}",
            var max when max == MinMatches => $"exactly {MinMatches}",
            _ => $"between {MinMatches} and {MaxMatches}"
        };
}

/// <summary>Scope used to evaluate a rule-cardinality constraint.</summary>
public enum RuleCardinalityScope
{
    /// <summary>Aggregate matches across every page selected by the rule.</summary>
    Document,
    /// <summary>Evaluate the constraint independently on every page selected by the rule.</summary>
    PerPage
}
