using System.Collections.ObjectModel;

namespace PdfLexer.Remediation;

/// <summary>Occurrence constraint for a structural-template particle.</summary>
public enum RemediationStructuralOccurrence
{
    ExactlyOne,
    Optional,
    ZeroOrMore,
    OneOrMore
}

/// <summary>Controls whether a structural template validates output or owns its materialized shape.</summary>
public enum RemediationStructuralTemplateMode
{
    Descriptive,
    Prescriptive
}

/// <summary>A closed, ordered structural-template particle.</summary>
public sealed record RemediationStructuralTemplateNode
{
    public RemediationStructuralTemplateNode(
        string tag,
        IEnumerable<RemediationStructuralTemplateNode>? children = null,
        string? id = null,
        RemediationStructuralOccurrence occurrence = RemediationStructuralOccurrence.ExactlyOne,
        PageSelector? pages = null,
        bool? spansPages = null)
    {
        if (string.IsNullOrWhiteSpace(tag))
        {
            throw new ArgumentException("A template node tag is required.", nameof(tag));
        }

        Tag = tag;
        Id = id;
        Occurrence = occurrence;
        Pages = pages;
        SpansPages = spansPages;
        Children = new ReadOnlyCollection<RemediationStructuralTemplateNode>(
            (children ?? Array.Empty<RemediationStructuralTemplateNode>()).ToList());
    }

    public string Tag { get; }
    public string? Id { get; }
    public RemediationStructuralOccurrence Occurrence { get; }
    public IReadOnlyList<RemediationStructuralTemplateNode> Children { get; }
    public PageSelector? Pages { get; }
    public bool? SpansPages { get; }
}

/// <summary>A document-scoped, closed description of the complete produced structure tree.</summary>
public sealed record RemediationStructuralTemplate
{
    public RemediationStructuralTemplate(RemediationStructuralTemplateNode document)
        : this(document, RemediationStructuralTemplateMode.Descriptive)
    {
    }

    public RemediationStructuralTemplate(
        RemediationStructuralTemplateNode document,
        RemediationStructuralTemplateMode mode = RemediationStructuralTemplateMode.Descriptive)
    {
        Document = document ?? throw new ArgumentNullException(nameof(document));
        Mode = mode;
    }

    public RemediationStructuralTemplate(IEnumerable<RemediationStructuralTemplateNode> children)
        : this(children, RemediationStructuralTemplateMode.Descriptive)
    {
    }

    public RemediationStructuralTemplate(
        IEnumerable<RemediationStructuralTemplateNode> children,
        RemediationStructuralTemplateMode mode = RemediationStructuralTemplateMode.Descriptive)
        : this(new RemediationStructuralTemplateNode("Document", children), mode)
    {
    }

    public RemediationStructuralTemplateNode Document { get; }

    public RemediationStructuralTemplateMode Mode { get; }
}

/// <summary>Kind of structural-template mismatch.</summary>
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

/// <summary>A machine-readable structural-template mismatch.</summary>
public sealed record RemediationTemplateDifference(
    RemediationTemplateDifferenceKind Kind,
    DiagnosticCode DiagnosticCode,
    string RuleSetId,
    string? SlotId,
    string? ExpectedPath,
    string? ActualPath,
    string? ExpectedValue,
    string? ActualValue,
    IReadOnlyList<int> PageIndexes,
    string? RuleId,
    bool Suppressed);

/// <summary>A deterministic occurrence in a prescriptive structural-template assembly plan.</summary>
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
    bool OpaqueInterior);
