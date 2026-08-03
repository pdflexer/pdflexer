namespace PdfLexer.Remediation;

/// <summary>Candidate predicate over a named compositional region.</summary>
public sealed record RegionRemediationPredicate(
    string RegionId,
    GeometryMatchMode Mode = GeometryMatchMode.Contains) : RemediationPredicate
{
    public override string DebugString => $"Region.{Mode}({RegionId})";

    protected override PredicateResult EvaluateCore(
        RemediationEvaluationContext context,
        RemediationCandidate candidate)
    {
        if (!candidate.HasUsableGeometry)
            return PredicateResult.NoMatch(
                $"Candidate '{candidate.CandidateId}' has no usable geometry for region matching.");

        var resolution = context.ResolveRegion(RegionId, candidate, Mode);
        return resolution == null
            ? PredicateResult.NoMatch($"Candidate is outside region '{RegionId}'.")
            : PredicateResult.Match(resolution.Confidence);
    }
}
