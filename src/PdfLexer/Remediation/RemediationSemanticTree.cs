namespace PdfLexer.Remediation;

/// <summary>Immutable semantic structure planned by remediation actions.</summary>
public sealed record RemediationSemanticTree(IReadOnlyList<RemediationSemanticNode> Roots)
{
    internal static RemediationSemanticTree FromClaims(IReadOnlyList<RemediationClaim> claims)
    {
        var children = claims.SelectMany(x => x.RelatedClaims).Select(x => x.ClaimId).ToHashSet();
        var roots = claims
            .Where(x => x.Status == ClaimStatus.Applied && !children.Contains(x.ClaimId))
            .OrderBy(x => x.PageIndex)
            .ThenBy(x => x.FirstSequenceIndex)
            .Select(x => BuildNode(x, new HashSet<ClaimId>()))
            .ToArray();
        return new RemediationSemanticTree(roots);
    }

    private static RemediationSemanticNode BuildNode(RemediationClaim claim, HashSet<ClaimId> ancestors)
    {
        if (!ancestors.Add(claim.ClaimId))
        {
            return new RemediationSemanticNode(
                claim.ClaimId, claim.ProducedTag, claim.RuleSetId, claim.RuleId,
                claim.Candidates.Select(x => x.CandidateId).ToArray(),
                claim.Candidates.SelectMany(x => x.SourceReferences).Distinct().ToArray(),
                Array.Empty<RemediationSemanticNode>());
        }

        return
        new(
            claim.ClaimId,
            claim.ProducedTag,
            claim.RuleSetId,
            claim.RuleId,
            claim.Candidates.Select(x => x.CandidateId).ToArray(),
            claim.Candidates.SelectMany(x => x.SourceReferences).Distinct().ToArray(),
            claim.RelatedClaims.Select(x => BuildNode(x, new HashSet<ClaimId>(ancestors))).ToArray());
    }
}

public sealed record RemediationSemanticNode(
    ClaimId ClaimId,
    string Tag,
    string? RuleSetId,
    string RuleId,
    IReadOnlyList<string> CandidateIds,
    IReadOnlyList<PdfLexer.Content.StructuredSourceRef> SourceReferences,
    IReadOnlyList<RemediationSemanticNode> Children);
