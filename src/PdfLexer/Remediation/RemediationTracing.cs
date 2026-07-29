using PdfLexer.Content;

namespace PdfLexer.Remediation;

/// <summary>A node in an opt-in predicate evaluation trace.</summary>
public sealed record PredicateTraceNode(
    string Predicate,
    bool? Result,
    double Confidence,
    string? Reason = null,
    IReadOnlyList<PredicateTraceNode>? Children = null,
    bool Evaluated = true,
    int? RejectingAndOperand = null,
    string? EvaluatedText = null);

/// <summary>Selects the rules and inputs for which rejection traces are retained.</summary>
public sealed record RemediationTraceRequest(
    IReadOnlyCollection<string> RuleIds,
    int? PageIndex = null,
    IReadOnlyCollection<string>? CandidateIds = null,
    IReadOnlyCollection<StructuredSourceRef>? SourceReferences = null)
{
    internal bool Includes(string ruleId, int pageIndex, RemediationCandidate candidate)
    {
        if (!RuleIds.Contains(ruleId, StringComparer.Ordinal) ||
            PageIndex is { } selectedPage && selectedPage != pageIndex)
        {
            return false;
        }

        var candidateSelected = CandidateIds == null || CandidateIds.Count == 0 ||
            CandidateIds.Contains(candidate.CandidateId, StringComparer.Ordinal);
        var sourceSelected = SourceReferences == null || SourceReferences.Count == 0 ||
            candidate.SourceReferences.Any(SourceReferences.Contains);
        return candidateSelected && sourceSelected;
    }
}

/// <summary>A retained predicate rejection for one rule input.</summary>
public sealed record RemediationPredicateTrace(
    string RuleId,
    int PageIndex,
    string CandidateId,
    RemediationCandidateKind CandidateKind,
    string? RawText,
    string? NormalizedText,
    IReadOnlyList<StructuredSourceRef> SourceReferences,
    PredicateTraceNode Trace);
