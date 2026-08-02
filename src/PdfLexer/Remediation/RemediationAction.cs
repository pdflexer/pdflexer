using PdfLexer.DOM;

namespace PdfLexer.Remediation;

/// <summary>Internal materialization action emitted by a compiled preview binding.</summary>
public abstract record RemediationAction
{
    public abstract RemediationActionKind Kind { get; }
}

/// <summary>Materializes a claim into its compiled template slot.</summary>
public sealed record BindTemplateSlotRemediationAction(
    TemplateSlotContentMode ContentMode = TemplateSlotContentMode.PreserveChildren) : RemediationAction
{
    public override RemediationActionKind Kind => RemediationActionKind.BindTemplateSlot;
}

/// <summary>Marks a claim as an explicitly declared PDF artifact.</summary>
public sealed record ArtifactRemediationAction(
    ArtifactSubtype Subtype,
    ArtifactSemanticSubtype? SemanticSubtype = null,
    bool IncludeBoundingBox = false,
    IReadOnlyList<ArtifactAttachmentEdge>? Attached = null) : RemediationAction
{
    public override RemediationActionKind Kind => RemediationActionKind.Artifact;

    public IReadOnlyList<ArtifactAttachmentEdge> Attached { get; } =
        Array.AsReadOnly((Attached ?? Array.Empty<ArtifactAttachmentEdge>()).ToArray());
}

public enum TemplateSlotContentMode
{
    PreserveChildren,
    FlattenLeafClaims
}

public enum ArtifactAttachmentEdge
{
    Top,
    Bottom,
    Left,
    Right
}

public enum RemediationActionKind
{
    BindTemplateSlot,
    Artifact
}

public static class RemediationActions
{
    public static RemediationAction Bind() => new BindTemplateSlotRemediationAction();

    public static RemediationAction Artifact(
        ArtifactSubtype subtype,
        ArtifactSemanticSubtype? semanticSubtype = null,
        bool includeBoundingBox = false,
        params ArtifactAttachmentEdge[] attached) =>
        new ArtifactRemediationAction(subtype, semanticSubtype, includeBoundingBox, attached);
}
