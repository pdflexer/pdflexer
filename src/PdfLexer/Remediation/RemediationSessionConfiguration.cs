namespace PdfLexer.Remediation;

/// <summary>
/// Configuration for a remediation session and the accessibility setup applied at commit.
/// </summary>
public sealed class RemediationSessionConfiguration
{
    /// <summary>Document language written to accessibility metadata.</summary>
    public string Language { get; init; } = "en-US";

    /// <summary>Document title written to document info and XMP metadata.</summary>
    public string Title { get; init; } = "Remediated Document";

    /// <summary>PDF/UA profile to target when committing remediation output.</summary>
    public PdfUaProfile Profile { get; init; } = PdfUaProfile.PdfUa1;

    /// <summary>Whether strict accessibility authoring checks are enabled.</summary>
    public bool StrictConformance { get; init; } = true;

    /// <summary>Controls whether suppressions are honored by the diagnostic pass.</summary>
    public RemediationDiagnosticStrictness DiagnosticStrictness { get; init; } = RemediationDiagnosticStrictness.Strict;

    /// <summary>Margins used by named layout zones.</summary>
    public RemediationNamedZoneMargins NamedZoneMargins { get; init; } = new();

    /// <summary>Default confidence for matches that do not compute a confidence explicitly.</summary>
    public double DefaultConfidence { get; init; } = 1.0;

    /// <summary>When true, writes rule identifiers into structure element titles for debugging.</summary>
    public bool DebugWrite { get; init; }

    /// <summary>Controls whether the session is an authoring dry-run or an enforced production run.</summary>
    public RemediationRunMode RunMode { get; init; } = RemediationRunMode.Enforced;
}

/// <summary>
/// Diagnostic strictness for suppression handling.
/// </summary>
public enum RemediationDiagnosticStrictness
{
    /// <summary>Ignore suppressions and report diagnostics as failures.</summary>
    Strict,
    /// <summary>Honor configured diagnostic suppressions.</summary>
    Permissive
}

/// <summary>
/// Page-relative margins used to resolve built-in named layout zones.
/// </summary>
public sealed class RemediationNamedZoneMargins
{
    /// <summary>Height of the header zone in user-space units.</summary>
    public double Header { get; init; } = 72.0;

    /// <summary>Height of the footer zone in user-space units.</summary>
    public double Footer { get; init; } = 72.0;

    /// <summary>Width of the left-margin zone in user-space units.</summary>
    public double Left { get; init; } = 72.0;

    /// <summary>Width of the right-margin zone in user-space units.</summary>
    public double Right { get; init; } = 72.0;
}
