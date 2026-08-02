namespace PdfLexer.Remediation;

/// <summary>Supported PDF artifact types in preview programs.</summary>
public enum ArtifactSubtype
{
    Pagination,
    Layout,
    Page,
    Background
}

/// <summary>PDF /Subtype values used with /Type /Pagination.</summary>
public enum ArtifactSemanticSubtype
{
    Header,
    Footer,
    Watermark
}
