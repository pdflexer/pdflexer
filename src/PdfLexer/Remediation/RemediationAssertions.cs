namespace PdfLexer.Remediation;

public enum SemanticAssertionScope
{
    Document,
    PerPage
}

/// <summary>Inclusive expected count range used by semantic assertions.</summary>
public sealed record AssertionCount(int Min, int? Max = null)
{
    public bool Accepts(int value) => value >= Min && (Max == null || value <= Max);
    public string Description => Max == null ? $"at least {Min}" :
        Min == Max ? $"exactly {Min}" : $"between {Min} and {Max}";
    public static AssertionCount Exactly(int value) => new(value, value);
}

public abstract record RemediationSemanticAssertion(
    string Id,
    AssertionCount Expected,
    SemanticAssertionScope Scope = SemanticAssertionScope.Document,
    PageSelector? Pages = null);

/// <summary>Counts applied occurrences bound to one canonical prescriptive-program slot.</summary>
public sealed record SlotElementCountAssertion(
    string Id,
    SlotRef Slot,
    AssertionCount Expected,
    SemanticAssertionScope Scope = SemanticAssertionScope.Document,
    PageSelector? Pages = null)
    : RemediationSemanticAssertion(Id, Expected, Scope, Pages);

/// <summary>Observed result of evaluating one semantic assertion.</summary>
public sealed record RemediationAssertionOutcome(
    string ProgramId,
    string AssertionId,
    int? PageIndex,
    string Expected,
    string Observed,
    bool Passed,
    string? RuleId = null,
    string? Tag = null)
{
    /// <summary>Program binding associated with the assertion result, when available.</summary>
    public string? BindingId { get; init; }

    /// <summary>Canonical program slot associated with the assertion result.</summary>
    public SlotRef? ProgramSlot { get; init; }
}
