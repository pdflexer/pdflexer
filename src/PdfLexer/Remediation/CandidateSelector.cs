namespace PdfLexer.Remediation;

/// <summary>Selects the candidate family enumerated by a classify rule.</summary>
public abstract record CandidateSelector
{
    public sealed record TextSelector(Granularity Granularity) : CandidateSelector;
    public sealed record ContentSelector(IReadOnlySet<RemediationCandidateKind> Kinds) : CandidateSelector;

    public static CandidateSelector Text(Granularity granularity) => new TextSelector(granularity);

    public static CandidateSelector Content(params RemediationCandidateKind[] kinds)
    {
        if (kinds == null || kinds.Length == 0 || kinds.Any(x => x == RemediationCandidateKind.Text))
        {
            throw new ArgumentException("At least one non-text candidate kind is required.", nameof(kinds));
        }
        return new ContentSelector(kinds.ToHashSet());
    }
}
