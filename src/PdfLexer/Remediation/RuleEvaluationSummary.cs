using PdfLexer.Content;
using PdfLexer.DOM;

namespace PdfLexer.Remediation;

/// <summary>Evaluation counters for one remediation rule and scope.</summary>
public sealed record RuleEvaluationCounts(
    int InputsConsidered,
    int InputsMatched,
    int RejectedByConfidence,
    int RejectedByConflict,
    int AppliedClaims,
    int OverriddenClaims);

/// <summary>Evaluation counters for one rule on one page.</summary>
public sealed record PageRuleEvaluationSummary(
    /// <summary>Zero-based page index.</summary>
    int PageIndex,
    /// <summary>Evaluation counters for the page.</summary>
    RuleEvaluationCounts Counts);

/// <summary>Aggregate and page-level evaluation summary for one composed rule.</summary>
public sealed record RuleEvaluationSummary(
    string RuleId,
    string? RuleSetId,
    Stage Stage,
    RuleEvaluationCounts Total,
    IReadOnlyList<PageRuleEvaluationSummary> Pages,
    int GroupPass = 0);

/// <summary>Whether an automatically artifacted leftover is prospective or committed.</summary>
public enum RemediationAutoArtifactDisposition
{
    /// <summary>The item would be artifacted by a commit.</summary>
    Planned,
    /// <summary>The item was artifacted by a successful commit.</summary>
    Applied
}

/// <summary>Details of one text content item handled by the automatic leftover policy.</summary>
public sealed record RemediationAutoArtifactOutcome(
    /// <summary>Zero-based page index.</summary>
    int PageIndex,
    /// <summary>Source content reference.</summary>
    StructuredSourceRef SourceReference,
    /// <summary>Extracted text of the content item.</summary>
    string Text,
    /// <summary>Content bounds in page coordinates.</summary>
    PdfRect<double> BoundingBox,
    /// <summary>Whether the artifact operation is planned or applied.</summary>
    RemediationAutoArtifactDisposition Disposition)
{
    /// <summary>Content bounds in relative page coordinates, as used by toleranced zones.</summary>
    public PdfRect<double> RelativeBoundingBox { get; init; }

    /// <summary>Declared artifact inventory item this content was absorbed as, when one matched.</summary>
    public string? InventoryItemId { get; init; }

    /// <summary>Candidate family handled by the automatic policy.</summary>
    public RemediationCandidateKind CandidateKind { get; init; } = RemediationCandidateKind.Text;

    /// <summary>Stable candidate identity, when available.</summary>
    public string? CandidateId { get; init; }

    /// <summary>Stable resource identity, when the item uses a resource.</summary>
    public string? ResourceIdentity { get; init; }

    /// <summary>Page resource name, when available.</summary>
    public string? ResourceName { get; init; }

    /// <summary>Number of document-wide invocations of the resource.</summary>
    public int ResourceUseCount { get; init; }
}

/// <summary>Structured details for one painting item left unclaimed by remediation rules.</summary>
public sealed record RemediationUnaccountedContent(
    int PageIndex,
    RemediationCandidateKind CandidateKind,
    string CandidateId,
    StructuredSourceRef SourceReference,
    PdfRect<double> BoundingBox,
    PdfRect<double> RelativeBoundingBox,
    string? RawText = null,
    string? NormalizedText = null,
    string? ResourceIdentity = null,
    string? ResourceName = null,
    int ResourceUseCount = 0);

/// <summary>How an existing input annotation is handled by remediation.</summary>
public enum RemediationAnnotationDisposition
{
    Unmodeled,
    Exempt,
    Planned,
    Applied
}

/// <summary>Read-only inventory entry for an annotation present before remediation.</summary>
public sealed record RemediationAnnotationInventoryItem(
    int PageIndex,
    string Subtype,
    PdfRect<double>? Bounds,
    bool Hidden,
    bool OffPage,
    bool HasStructParent,
    bool BlocksConformance,
    string Reason)
{
    public string? CandidateId { get; init; }
    public RemediationAnnotationDisposition Disposition { get; init; }
    public string? RuleId { get; init; }
    public string? ProducedTag { get; init; }
    public AnnotationDestinationKind DestinationKind { get; init; }
}

internal sealed class RuleEvaluationAccumulator
{
    private readonly IReadOnlyList<Rule> _rules;
    private readonly int _pageCount;
    private readonly Dictionary<(string RuleId, int PageIndex), MutableRuleEvaluationCounts> _counts = new();

    public RuleEvaluationAccumulator(IReadOnlyList<Rule> rules, int pageCount)
    {
        _rules = rules;
        _pageCount = pageCount;
        foreach (var rule in rules)
        {
            foreach (var pageIndex in rule.Pages.SelectPages(pageCount))
            {
                _counts.TryAdd((rule.Id, pageIndex), new MutableRuleEvaluationCounts());
            }
        }
    }

    public void Record(
        Rule rule,
        int pageIndex,
        int considered = 0,
        int matched = 0,
        int rejectedByConfidence = 0,
        int rejectedByConflict = 0)
    {
        var counts = Get(rule.Id, pageIndex);
        counts.InputsConsidered += considered;
        counts.InputsMatched += matched;
        counts.RejectedByConfidence += rejectedByConfidence;
        counts.RejectedByConflict += rejectedByConflict;
    }

    public IReadOnlyList<RuleEvaluationSummary> Build(
        IReadOnlyList<RemediationClaim> claims,
        IReadOnlyList<RemediationClaim> skippedClaims)
    {
        var appliedByRulePage = claims
            .GroupBy(x => (x.RuleId, x.PageIndex))
            .ToDictionary(x => x.Key, x => x.Count());
        var overriddenByRulePage = skippedClaims
            .Where(x => x.Status == ClaimStatus.Overridden)
            .GroupBy(x => (x.RuleId, x.PageIndex))
            .ToDictionary(x => x.Key, x => x.Count());

        var summaries = new List<RuleEvaluationSummary>(_rules.Count);
        foreach (var rule in _rules)
        {
            var pages = new List<PageRuleEvaluationSummary>();
            foreach (var pageIndex in rule.Pages.SelectPages(_pageCount))
            {
                var mutable = Get(rule.Id, pageIndex);
                var counts = mutable.ToPublic(
                    appliedByRulePage.GetValueOrDefault((rule.Id, pageIndex)),
                    overriddenByRulePage.GetValueOrDefault((rule.Id, pageIndex)));
                pages.Add(new PageRuleEvaluationSummary(pageIndex, counts));
            }

            summaries.Add(new RuleEvaluationSummary(
                rule.Id,
                rule.RuleSetId,
                rule.Stage,
                Sum(pages.Select(x => x.Counts)),
                pages,
                rule.GroupPass));
        }

        return summaries;
    }

    private MutableRuleEvaluationCounts Get(string ruleId, int pageIndex)
    {
        if (!_counts.TryGetValue((ruleId, pageIndex), out var counts))
        {
            counts = new MutableRuleEvaluationCounts();
            _counts[(ruleId, pageIndex)] = counts;
        }

        return counts;
    }

    private static RuleEvaluationCounts Sum(IEnumerable<RuleEvaluationCounts> counts)
    {
        var total = new MutableRuleEvaluationCounts();
        var applied = 0;
        var overridden = 0;
        foreach (var count in counts)
        {
            total.InputsConsidered += count.InputsConsidered;
            total.InputsMatched += count.InputsMatched;
            total.RejectedByConfidence += count.RejectedByConfidence;
            total.RejectedByConflict += count.RejectedByConflict;
            applied += count.AppliedClaims;
            overridden += count.OverriddenClaims;
        }

        return total.ToPublic(applied, overridden);
    }

    private sealed class MutableRuleEvaluationCounts
    {
        public int InputsConsidered { get; set; }
        public int InputsMatched { get; set; }
        public int RejectedByConfidence { get; set; }
        public int RejectedByConflict { get; set; }

        public RuleEvaluationCounts ToPublic(int appliedClaims, int overriddenClaims) =>
            new(
                InputsConsidered,
                InputsMatched,
                RejectedByConfidence,
                RejectedByConflict,
                appliedClaims,
                overriddenClaims);
    }
}
