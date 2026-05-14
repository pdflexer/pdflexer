namespace PdfLexer.Remediation;

/// <summary>
/// Result of rule-set validation before page parsing or document mutation.
/// </summary>
public sealed class ValidationReport
{
    internal ValidationReport(IReadOnlyList<string>? errors = null, IReadOnlyList<string>? warnings = null)
    {
        Errors = errors ?? Array.Empty<string>();
        Warnings = warnings ?? Array.Empty<string>();
    }

    /// <summary>True when validation produced no errors.</summary>
    public bool IsValid => Errors.Count == 0;

    /// <summary>Validation errors that should prevent evaluation or commit.</summary>
    public IReadOnlyList<string> Errors { get; }

    /// <summary>Validation warnings, including partially validated custom hooks.</summary>
    public IReadOnlyList<string> Warnings { get; }
}
