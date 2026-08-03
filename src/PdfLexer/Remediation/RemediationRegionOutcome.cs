using PdfLexer.Content;

namespace PdfLexer.Remediation;

/// <summary>A resolved page segment of a declared region.</summary>
public sealed record RemediationRegionResolutionOutcome(
    string RegionId,
    int PageIndex,
    PdfRect<double> BaseBounds,
    PdfRect<double> Bounds,
    double Confidence,
    string? FlowActivationIdentity = null,
    FlowRegionPageRole? FlowPageRole = null);

/// <summary>An exact painting candidate absorbed by guarded region accounting.</summary>
public sealed record RemediationRegionAbsorptionOutcome(
    string CandidateId,
    RemediationCandidateKind CandidateKind,
    string? TextSummary,
    string RegionId,
    string AccountingId,
    string ArtifactId,
    ArtifactSubtype Subtype,
    ArtifactSemanticSubtype? SemanticSubtype,
    bool IncludeBoundingBox,
    IReadOnlyList<ArtifactAttachmentEdge> Attached,
    int PageIndex,
    PdfRect<double> Bounds,
    double Confidence,
    string? FlowActivationIdentity = null);
