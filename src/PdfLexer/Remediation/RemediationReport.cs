using System;
using System.Collections.Generic;
using System.Linq;
using PdfLexer.Content;
using PdfLexer.DOM;

namespace PdfLexer.Remediation;

/// <summary>
/// Report produced by remediation validation, dry-run, or commit.
/// </summary>
public sealed class RemediationReport
{
    internal RemediationReport(
        bool committed,
        bool appliedAccessibilitySetup,
        IReadOnlyList<RemediationClaim>? claims = null,
        IReadOnlyList<RemediationClaim>? skippedClaims = null,
        IReadOnlyList<string>? diagnostics = null,
        IReadOnlyList<DiagnosticSuppression>? suppressions = null,
        IReadOnlyList<RuleEvaluationSummary>? ruleEvaluations = null,
        IReadOnlyList<RemediationAutoArtifactOutcome>? autoArtifacts = null,
        IReadOnlyList<RemediationPredicateTrace>? predicateTraces = null,
        IReadOnlyList<RemediationAssertionOutcome>? assertionOutcomes = null,
        RemediationSemanticTree? plannedSemanticTree = null,
        IReadOnlyList<RemediationUnaccountedContent>? unaccountedContent = null,
        IReadOnlyList<RemediationAnnotationInventoryItem>? annotationInventory = null,
        IReadOnlyList<string>? warnings = null,
        IReadOnlyList<RemediationTemplateDifference>? templateDifferences = null,
        IReadOnlyList<RemediationTemplateAssemblyItem>? templateAssembly = null)
    {
        Committed = committed;
        AppliedAccessibilitySetup = appliedAccessibilitySetup;
        Claims = claims ?? Array.Empty<RemediationClaim>();
        SkippedClaims = skippedClaims ?? Array.Empty<RemediationClaim>();
        Diagnostics = diagnostics ?? Array.Empty<string>();
        Suppressions = suppressions ?? Array.Empty<DiagnosticSuppression>();
        RuleEvaluations = ruleEvaluations ?? Array.Empty<RuleEvaluationSummary>();
        AutoArtifacts = autoArtifacts ?? Array.Empty<RemediationAutoArtifactOutcome>();
        PredicateTraces = predicateTraces ?? Array.Empty<RemediationPredicateTrace>();
        AssertionOutcomes = assertionOutcomes ?? Array.Empty<RemediationAssertionOutcome>();
        PlannedSemanticTree = plannedSemanticTree ?? new RemediationSemanticTree(Array.Empty<RemediationSemanticNode>());
        UnaccountedContent = unaccountedContent ?? Array.Empty<RemediationUnaccountedContent>();
        AnnotationInventory = annotationInventory ?? Array.Empty<RemediationAnnotationInventoryItem>();
        Warnings = warnings ?? Array.Empty<string>();
        TemplateDifferences = templateDifferences ?? Array.Empty<RemediationTemplateDifference>();
        TemplateAssembly = templateAssembly ?? Array.Empty<RemediationTemplateAssemblyItem>();
        
        Outcomes = Claims.Select(CreateOutcome).ToList();
        SkippedOutcomes = SkippedClaims.Select(CreateOutcome).ToList();
    }

    /// <summary>True when the report came from a successful commit.</summary>
    public bool Committed { get; }

    /// <summary>True when accessibility setup was applied during commit.</summary>
    public bool AppliedAccessibilitySetup { get; }

    /// <summary>Applied claims produced by rule evaluation.</summary>
    public IReadOnlyList<RemediationClaim> Claims { get; }

    /// <summary>Skipped, conflicted, overridden, or failed claims.</summary>
    public IReadOnlyList<RemediationClaim> SkippedClaims { get; }

    /// <summary>Diagnostics produced by validation or commit checks.</summary>
    public IReadOnlyList<string> Diagnostics { get; }

    /// <summary>Nonblocking authoring warnings produced during planning.</summary>
    public IReadOnlyList<string> Warnings { get; }

    /// <summary>Machine-readable differences from the declared structural template.</summary>
    public IReadOnlyList<RemediationTemplateDifference> TemplateDifferences { get; }

    /// <summary>Deterministic prescriptive-template occurrences used by planning and commit.</summary>
    public IReadOnlyList<RemediationTemplateAssemblyItem> TemplateAssembly { get; }

    /// <summary>Diagnostic suppressions configured on the session.</summary>
    public IReadOnlyList<DiagnosticSuppression> Suppressions { get; }

    /// <summary>Aggregate and page-level counters for every composed rule, including rules with no matches.</summary>
    public IReadOnlyList<RuleEvaluationSummary> RuleEvaluations { get; }

    /// <summary>Text content planned for or handled by the automatic artifact policy.</summary>
    public IReadOnlyList<RemediationAutoArtifactOutcome> AutoArtifacts { get; }

    /// <summary>Opt-in traces retained for rejected predicate inputs.</summary>
    public IReadOnlyList<RemediationPredicateTrace> PredicateTraces { get; }

    /// <summary>Semantic output assertion results.</summary>
    public IReadOnlyList<RemediationAssertionOutcome> AssertionOutcomes { get; }

    /// <summary>Immutable semantic tree produced from the finalized action plan.</summary>
    public RemediationSemanticTree PlannedSemanticTree { get; }

    /// <summary>Painting content left unclaimed when the leftover policy does not account for it.</summary>
    public IReadOnlyList<RemediationUnaccountedContent> UnaccountedContent { get; }

    /// <summary>Annotations present in the input and whether they cap strict conformance.</summary>
    public IReadOnlyList<RemediationAnnotationInventoryItem> AnnotationInventory { get; }

    /// <summary>Returns a retained rejection trace for a rule and candidate.</summary>
    public RemediationPredicateTrace? ExplainRejection(string ruleId, string candidateId) =>
        PredicateTraces.FirstOrDefault(x =>
            string.Equals(x.RuleId, ruleId, StringComparison.Ordinal) &&
            string.Equals(x.CandidateId, candidateId, StringComparison.Ordinal));

    /// <summary>Public outcome summaries for applied claims.</summary>
    public IReadOnlyList<RemediationClaimOutcome> Outcomes { get; }

    /// <summary>Public outcome summaries for skipped claims.</summary>
    public IReadOnlyList<RemediationClaimOutcome> SkippedOutcomes { get; }

    /// <summary>Explains rule outcomes that considered a source reference.</summary>
    public IReadOnlyList<RemediationClaimOutcome> Explain(StructuredSourceRef sourceRef)
    {
        var results = new List<RemediationClaimOutcome>();
        for (var i = 0; i < Claims.Count; i++)
        {
            if (Claims[i].Candidates.Any(c => c.SourceReferences.Contains(sourceRef)))
            {
                results.Add(Outcomes[i]);
            }
        }
        for (var i = 0; i < SkippedClaims.Count; i++)
        {
            if (SkippedClaims[i].Candidates.Any(c => c.SourceReferences.Contains(sourceRef)))
            {
                results.Add(SkippedOutcomes[i]);
            }
        }
        return results;
    }

    /// <summary>Explains rule outcomes that considered a remediation candidate.</summary>
    public IReadOnlyList<RemediationClaimOutcome> Explain(RemediationCandidate candidate)
    {
        var results = new List<RemediationClaimOutcome>();
        var sourceRefs = new HashSet<StructuredSourceRef>(candidate.SourceReferences);
        for (var i = 0; i < Claims.Count; i++)
        {
            if (Claims[i].Candidates.Any(c => c.SourceReferences.Any(sourceRefs.Contains)))
            {
                results.Add(Outcomes[i]);
            }
        }
        for (var i = 0; i < SkippedClaims.Count; i++)
        {
            if (SkippedClaims[i].Candidates.Any(c => c.SourceReferences.Any(sourceRefs.Contains)))
            {
                results.Add(SkippedOutcomes[i]);
            }
        }
        return results;
    }

    /// <summary>Explains rule outcomes that considered a structured character.</summary>
    public IReadOnlyList<RemediationClaimOutcome> Explain(StructuredCharacter character) =>
        Explain(character.SourceReference);

    private static RemediationClaimOutcome CreateOutcome(RemediationClaim claim)
    {
        return new RemediationClaimOutcome(
            claim.RuleId,
            claim.SelectorDebugString,
            claim.Confidence,
            claim.Status,
            claim.PageIndex,
            claim.PageIndexes,
            claim.BoundingBox,
            claim.BoundsByPage,
            claim.Candidates.Select(candidate => new RemediationCandidateSummary(
                candidate.CandidateId,
                candidate.Kind,
                candidate is TextRemediationCandidate text ? text.Text : string.Empty,
                candidate is TextRemediationCandidate normalizedText
                    ? claim.TextNormalization.Normalize(normalizedText.Text)
                    : string.Empty,
                candidate.BoundingBox,
                candidate.RelativeBoundingBox,
                candidate.SourceReferences)).ToArray(),
            claim.AppliedBindings.Select(b => new RemediationAppliedBindingSummary(
                b.ProducedTag,
                b.Mcids,
                b.StructureNode?.ID,
                b.Bounds)).ToList());
    }
}

/// <summary>
/// Public summary of a remediation claim outcome.
/// </summary>
public sealed record RemediationClaimOutcome(
    /// <summary>Rule that produced the outcome.</summary>
    string RuleId,
    /// <summary>Serialized selector or predicate description.</summary>
    string SelectorDebugString,
    /// <summary>Outcome confidence in the range [0, 1].</summary>
    double Confidence,
    /// <summary>Claim lifecycle status.</summary>
    ClaimStatus Status,
    /// <summary>Zero-based page index.</summary>
    int PageIndex,
    /// <summary>All zero-based pages containing the outcome.</summary>
    IReadOnlyList<int> PageIndexes,
    /// <summary>Union bounds for selected candidates, when available.</summary>
    PdfRect<double>? BoundingBox,
    /// <summary>Per-page bounds for selected candidates.</summary>
    IReadOnlyDictionary<int, PdfRect<double>> BoundsByPage,
    /// <summary>Selected candidates with raw and effective normalized text.</summary>
    IReadOnlyList<RemediationCandidateSummary> Candidates,
    /// <summary>Applied binding summaries for content and structure nodes.</summary>
    IReadOnlyList<RemediationAppliedBindingSummary> AppliedBindings);

/// <summary>Public candidate summary retained in a rule outcome.</summary>
public sealed record RemediationCandidateSummary(
    string CandidateId,
    RemediationCandidateKind Kind,
    string RawText,
    string NormalizedText,
    PdfRect<double> BoundingBox,
    PdfRect<double> RelativeBoundingBox,
    IReadOnlyList<StructuredSourceRef> SourceReferences);

/// <summary>
/// Public summary of marked-content and structure bindings created for a claim.
/// </summary>
public sealed record RemediationAppliedBindingSummary(
    /// <summary>Produced structure tag.</summary>
    string ProducedTag,
    /// <summary>Marked-content identifiers associated with the binding.</summary>
    IReadOnlyList<int> Mcids,
    /// <summary>Structure node identifier, when one is available.</summary>
    string? StructureNodeId,
    /// <summary>Bounds associated with the binding, when available.</summary>
    PdfRect<double>? Bounds);
