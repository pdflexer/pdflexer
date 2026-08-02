namespace PdfLexer.Remediation;

public enum RemediationTemplateDifferenceKind
{
    MissingRequired,
    UnexpectedNode,
    WrongOrder,
    OccurrenceViolation,
    IllegalNesting,
    SlotUnfilled,
    PageMismatch,
    PageSpanMismatch,
    MaterializationDivergence,
    UndeclaredArtifact,
    MissingDeclaredArtifact,
    ArtifactOccurrenceViolation
}

/// <summary>A machine-readable mismatch between a compiled program and materialized output.</summary>
public sealed record RemediationTemplateDifference(
    RemediationTemplateDifferenceKind Kind,
    DiagnosticCode DiagnosticCode,
    string ProgramId,
    string? SlotId,
    string? ExpectedPath,
    string? ActualPath,
    string? ExpectedValue,
    string? ActualValue,
    IReadOnlyList<int> PageIndexes,
    string? RuleId,
    bool Suppressed)
{
    public string? BindingId { get; init; }
    public SlotRef? ProgramSlot { get; init; }
}

/// <summary>A deterministic occurrence in a compiled program assembly plan.</summary>
public sealed record RemediationTemplateAssemblyItem(
    string SlotId,
    string TemplatePath,
    int OccurrenceIndex,
    string Identity,
    string ParentIdentity,
    string? ProducingRuleId,
    string? ProducingClaimReference,
    IReadOnlyList<string> ConsumedClaimReferences,
    bool Synthesized,
    bool OpaqueInterior)
{
    public SlotRef? ProgramSlot { get; init; }
    public string? BindingId { get; init; }
}

