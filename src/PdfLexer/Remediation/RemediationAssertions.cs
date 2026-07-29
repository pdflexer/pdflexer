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

public sealed record RuleOutputCountAssertion(
    string Id,
    string RuleId,
    AssertionCount Expected,
    string? ProducedTag = null,
    SemanticAssertionScope Scope = SemanticAssertionScope.Document,
    PageSelector? Pages = null)
    : RemediationSemanticAssertion(Id, Expected, Scope, Pages);

public sealed record StructureElementCountAssertion(
    string Id,
    string Tag,
    AssertionCount Expected,
    SemanticAssertionScope Scope = SemanticAssertionScope.Document,
    PageSelector? Pages = null)
    : RemediationSemanticAssertion(Id, Expected, Scope, Pages);

public sealed record ParentChildShapeAssertion(
    string Id,
    string ParentTag,
    IReadOnlyCollection<string> AllowedChildTags,
    AssertionCount ExpectedChildren,
    SemanticAssertionScope Scope = SemanticAssertionScope.Document,
    PageSelector? Pages = null)
    : RemediationSemanticAssertion(Id, AssertionCount.Exactly(0), Scope, Pages);

/// <summary>Observed result of evaluating one semantic assertion.</summary>
public sealed record RemediationAssertionOutcome(
    string RuleSetId,
    string AssertionId,
    int? PageIndex,
    string Expected,
    string Observed,
    bool Passed,
    string? RuleId = null,
    string? Tag = null);
