namespace PdfLexer.Remediation;

public sealed record BindingCardinality
{
    public BindingCardinality(
        int minMatches,
        int? maxMatches = null,
        BindingCardinalityScope scope = BindingCardinalityScope.Document)
    {
        if (minMatches < 0) throw new ArgumentOutOfRangeException(nameof(minMatches));
        if (maxMatches is < 0) throw new ArgumentOutOfRangeException(nameof(maxMatches));
        if (maxMatches < minMatches)
            throw new ArgumentOutOfRangeException(nameof(maxMatches), "Maximum matches must be greater than or equal to minimum matches.");
        if (minMatches == 0 && maxMatches == null)
            throw new ArgumentException("A cardinality constraint must declare a positive minimum or a maximum.");

        MinMatches = minMatches;
        MaxMatches = maxMatches;
        Scope = scope;
    }

    public int MinMatches { get; }
    public int? MaxMatches { get; }
    public BindingCardinalityScope Scope { get; }

    public static BindingCardinality Exactly(int matches, BindingCardinalityScope scope = BindingCardinalityScope.Document) =>
        new(matches, matches, scope);
    public static BindingCardinality AtLeast(int matches, BindingCardinalityScope scope = BindingCardinalityScope.Document) =>
        new(matches, null, scope);
    public static BindingCardinality AtMost(int matches, BindingCardinalityScope scope = BindingCardinalityScope.Document) =>
        new(0, matches, scope);
    public static BindingCardinality Between(int minMatches, int maxMatches, BindingCardinalityScope scope = BindingCardinalityScope.Document) =>
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

public enum BindingCardinalityScope
{
    Document,
    PerPage
}

