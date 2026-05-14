namespace PdfLexer.Remediation;

/// <summary>
/// Diagnostic categories emitted by remediation validation and commit checks.
/// </summary>
public enum DiagnosticCode
{
    /// <summary>Unclassified diagnostic.</summary>
    Unknown = 0,
    /// <summary>Painted content exists outside a marked-content scope.</summary>
    UntaggedContent = 1,
    /// <summary>A marked-content identifier is missing a matching content or structure reference.</summary>
    OrphanedMcid = 2,
    /// <summary>A marked-content identifier is duplicated in content or structure references.</summary>
    DuplicatedMcid = 3,
    /// <summary>A page with marked content is missing required structure-parent wiring.</summary>
    MissingStructParents = 4,
    /// <summary>Logical structure order differs from the expected reading order.</summary>
    ReadingOrderDrift = 5
}

/// <summary>
/// Justified suppression for a diagnostic code and scope.
/// </summary>
public sealed record DiagnosticSuppression(
    /// <summary>Diagnostic code to suppress.</summary>
    DiagnosticCode Code,
    /// <summary>Suppression scope, such as a page scope or <c>*</c>.</summary>
    string Scope,
    /// <summary>Human-readable justification for audit review.</summary>
    string Reason);
