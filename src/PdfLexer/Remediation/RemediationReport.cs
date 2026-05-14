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
        IReadOnlyList<DiagnosticSuppression>? suppressions = null)
    {
        Committed = committed;
        AppliedAccessibilitySetup = appliedAccessibilitySetup;
        Claims = claims ?? Array.Empty<RemediationClaim>();
        SkippedClaims = skippedClaims ?? Array.Empty<RemediationClaim>();
        Diagnostics = diagnostics ?? Array.Empty<string>();
        Suppressions = suppressions ?? Array.Empty<DiagnosticSuppression>();
        
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

    /// <summary>Diagnostic suppressions configured on the session.</summary>
    public IReadOnlyList<DiagnosticSuppression> Suppressions { get; }

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
            claim.Granularity,
            claim.Confidence,
            claim.Status,
            claim.PageIndex,
            claim.BoundingBox,
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
    /// <summary>Candidate granularity targeted by the rule.</summary>
    Granularity Granularity,
    /// <summary>Outcome confidence in the range [0, 1].</summary>
    double Confidence,
    /// <summary>Claim lifecycle status.</summary>
    ClaimStatus Status,
    /// <summary>Zero-based page index.</summary>
    int PageIndex,
    /// <summary>Union bounds for selected candidates, when available.</summary>
    PdfRect<double>? BoundingBox,
    /// <summary>Applied binding summaries for content and structure nodes.</summary>
    IReadOnlyList<RemediationAppliedBindingSummary> AppliedBindings);

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
