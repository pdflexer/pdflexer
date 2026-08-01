using PdfLexer.Content;
using PdfLexer.DOM;

namespace PdfLexer.Remediation;

/// <summary>Disposition assigned to a structured remediation runtime diagnostic.</summary>
public enum RemediationDiagnosticDisposition
{
    /// <summary>The diagnostic prevents a successful enforced commit.</summary>
    Error,
    /// <summary>The diagnostic identifies incomplete authoring work.</summary>
    WorkItem,
    /// <summary>The diagnostic is informational and does not block a commit.</summary>
    Warning,
    /// <summary>The diagnostic was explicitly acknowledged by the declared policy.</summary>
    Acknowledged
}

/// <summary>Structured runtime diagnostic retained alongside the legacy diagnostic string view.</summary>
public sealed record RemediationRuntimeDiagnostic(
    DiagnosticCode Code,
    RemediationDiagnosticDisposition Disposition,
    string Scope,
    string Message,
    SlotRef? ProgramSlot = null,
    string? BindingId = null,
    IReadOnlyList<string>? CandidateIds = null,
    IReadOnlyDictionary<string, object?>? Evidence = null,
    DiagnosticSuppression? Suppression = null)
{
    /// <summary>Whether this diagnostic is a commit blocker for enforced execution.</summary>
    public bool IsBlocking => Disposition == RemediationDiagnosticDisposition.Error;

    /// <summary>Human-readable suppression justification, when this item was suppressed.</summary>
    public string? SuppressionReason => Suppression?.Reason;

    /// <summary>Candidate references, normalized to an empty list when none were supplied.</summary>
    public IReadOnlyList<string> CandidateReferences => CandidateIds ?? Array.Empty<string>();

    /// <summary>Evidence attached to the diagnostic, normalized to an empty map when absent.</summary>
    public IReadOnlyDictionary<string, object?> EvidenceMap =>
        Evidence ?? new Dictionary<string, object?>();
}

/// <summary>Program-facing evaluation summary for one compiled binding.</summary>
public sealed record RemediationBindingEvaluationSummary(
    string BindingId,
    int DependencyLayer,
    int InputsConsidered,
    int InputsMatched,
    int AppliedClaims,
    int SkippedClaims,
    bool? CardinalitySatisfied = null)
{
    public string? DefinitionId { get; init; }

    /// <summary>Canonical slot targeted by this binding, when the target is a slot.</summary>
    public SlotRef? ProgramSlot { get; init; }

    /// <summary>Artifact targeted by this binding, when the target is an artifact.</summary>
    public string? ArtifactId { get; init; }

    /// <summary>Inputs rejected because their predicate confidence was too low.</summary>
    public int RejectedByConfidence { get; init; }

    /// <summary>Inputs rejected because ownership conflicted with another claim.</summary>
    public int RejectedByConflict { get; init; }

    /// <summary>Human-readable cardinality result, when the binding declares cardinality.</summary>
    public string? CardinalityOutcome { get; init; }

    /// <summary>Whether the binding targets an artifact rather than a template slot.</summary>
    public bool TargetsArtifact => ArtifactId != null;
}

/// <summary>Structured comparison between declared child order and available source-order evidence.</summary>
public sealed record RemediationOrderComparison(
    SlotRef? ContainerSlot,
    string FirstOccurrenceIdentity,
    string SecondOccurrenceIdentity,
    int FirstDeclaredIndex,
    int SecondDeclaredIndex,
    int? FirstSourceOrderIndex = null,
    int? SecondSourceOrderIndex = null,
    int? FirstGeometricOrderIndex = null,
    int? SecondGeometricOrderIndex = null,
    IReadOnlyList<int>? PageIndexes = null,
    PdfRect<double>? FirstBounds = null,
    PdfRect<double>? SecondBounds = null,
    IReadOnlyList<StructuredSourceRef>? FirstSourceReferences = null,
    IReadOnlyList<StructuredSourceRef>? SecondSourceReferences = null,
    RemediationDiagnosticDisposition Disposition = RemediationDiagnosticDisposition.Warning)
{
    /// <summary>Identity of the materialized container, when available.</summary>
    public string? ContainerIdentity { get; init; }

    /// <summary>Candidates contributing evidence to the first occurrence.</summary>
    public IReadOnlyList<string> FirstCandidateIds { get; init; } = Array.Empty<string>();

    /// <summary>Candidates contributing evidence to the second occurrence.</summary>
    public IReadOnlyList<string> SecondCandidateIds { get; init; } = Array.Empty<string>();

    /// <summary>Whether content-stream order inverts the declared order for this pair.</summary>
    public bool SourceOrderInverted => IsInverted(FirstSourceOrderIndex, SecondSourceOrderIndex);

    /// <summary>Whether geometric order inverts the declared order for this pair.</summary>
    public bool GeometricOrderInverted => IsInverted(FirstGeometricOrderIndex, SecondGeometricOrderIndex);

    /// <summary>Pages containing either compared occurrence.</summary>
    public IReadOnlyList<int> Pages => PageIndexes ?? Array.Empty<int>();

    /// <summary>Source references for the first compared occurrence.</summary>
    public IReadOnlyList<StructuredSourceRef> FirstSources =>
        FirstSourceReferences ?? Array.Empty<StructuredSourceRef>();

    /// <summary>Source references for the second compared occurrence.</summary>
    public IReadOnlyList<StructuredSourceRef> SecondSources =>
        SecondSourceReferences ?? Array.Empty<StructuredSourceRef>();

    private bool IsInverted(int? firstEvidenceIndex, int? secondEvidenceIndex)
    {
        if (firstEvidenceIndex is not { } first || secondEvidenceIndex is not { } second ||
            FirstDeclaredIndex == SecondDeclaredIndex)
        {
            return false;
        }

        var declaredFirst = FirstDeclaredIndex < SecondDeclaredIndex;
        var evidenceFirst = first < second;
        return declaredFirst != evidenceFirst;
    }
}

/// <summary>A deterministic occurrence partition produced by the native program runtime.</summary>
public sealed record RemediationOccurrencePartition(
    SlotRef CompositeSlot,
    string ParentOccurrenceIdentity,
    string OccurrenceIdentity,
    string Boundary,
    IReadOnlyList<string>? ActivationCandidateIds = null,
    IReadOnlyList<StructuredSourceRef>? ActivationSourceReferences = null,
    IReadOnlyList<string>? AssignedClaimIds = null,
    IReadOnlyList<string>? RejectedClaimIds = null,
    IReadOnlyList<int>? PageIndexes = null,
    RemediationDiagnosticDisposition Disposition = RemediationDiagnosticDisposition.Warning,
    long? ActivationSourceOrdinal = null)
{
    public IReadOnlyList<string> Activations => ActivationCandidateIds ?? Array.Empty<string>();
    public IReadOnlyList<StructuredSourceRef> ActivationSources =>
        ActivationSourceReferences ?? Array.Empty<StructuredSourceRef>();
    public IReadOnlyList<string> AssignedClaims => AssignedClaimIds ?? Array.Empty<string>();
    public IReadOnlyList<string> RejectedClaims => RejectedClaimIds ?? Array.Empty<string>();
    public IReadOnlyList<int> Pages => PageIndexes ?? Array.Empty<int>();
}
