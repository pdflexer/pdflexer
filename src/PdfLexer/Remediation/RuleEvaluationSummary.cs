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
    IReadOnlyList<PageRuleEvaluationSummary> Pages);

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
    RemediationAutoArtifactDisposition Disposition);

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
                pages));
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
