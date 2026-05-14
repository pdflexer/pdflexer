using PdfLexer.Content.Model;

namespace PdfLexer.Remediation;

/// <summary>
/// Declarative action performed by a remediation rule.
/// </summary>
public abstract record RemediationAction
{
    /// <summary>Debug representation used in validation and reports.</summary>
    public abstract string DebugString { get; }

    /// <summary>Action category.</summary>
    public abstract RemediationActionKind Kind { get; }

    internal virtual void Validate(Rule rule, List<string> errors)
    {
    }
}

/// <summary>Tags matched content with a structure element.</summary>
public sealed record TagRemediationAction(PdfName Name, PdfDictionary? Attributes = null) : RemediationAction
{
    public override RemediationActionKind Kind => RemediationActionKind.Tag;

    public override string DebugString => $"Tag({Name.Value})";

    internal override void Validate(Rule rule, List<string> errors)
    {
        if (rule.Stage == Stage.Group)
        {
            errors.Add("Tag actions are not valid in the Group stage.");
        }

        if (rule.Stage == Stage.Refine)
        {
            errors.Add("Tag actions that create new leaf claims are not valid in the Refine stage.");
        }
    }
}

/// <summary>Marks matched content as an artifact.</summary>
public sealed record ArtifactRemediationAction(ArtifactSubtype Subtype) : RemediationAction
{
    public override RemediationActionKind Kind => RemediationActionKind.Artifact;

    public override string DebugString => $"Artifact({Subtype})";

    internal override void Validate(Rule rule, List<string> errors)
    {
        if (!Enum.IsDefined(typeof(ArtifactSubtype), Subtype))
        {
            errors.Add($"Artifact subtype '{Subtype}' is not supported.");
        }
    }
}

/// <summary>Builds table structure from candidates or existing claims.</summary>
public sealed record TableRemediationAction(
    IReadOnlyList<double>? Columns = null,
    int HeaderRows = 0,
    ClaimPredicate? Over = null,
    ClaimPredicate? HeaderSelector = null,
    TableCellContentMode CellContentMode = TableCellContentMode.PreserveChildren) : RemediationAction
{
    public override RemediationActionKind Kind => RemediationActionKind.Table;

    public override string DebugString => Columns == null
        ? Over == null ? "Table()" : $"Table(over: {Over.DebugString})"
        : HeaderRows > 0
            ? $"Table({Columns.Count} columns, headerRows: {HeaderRows})"
            : Over == null ? $"Table({Columns.Count} columns)" : $"Table({Columns.Count} columns, over: {Over.DebugString})";

    internal override void Validate(Rule rule, List<string> errors)
    {
        if (rule.Stage != Stage.Group)
        {
            errors.Add("Table action is only valid in the Group stage.");
        }

        if (HeaderRows < 0)
        {
            errors.Add("Table header row count must be non-negative.");
        }

        if (Columns is not { Count: > 0 })
        {
            return;
        }

        for (var i = 1; i < Columns.Count; i++)
        {
            if (Columns[i] <= Columns[i - 1])
            {
                errors.Add("Table columns must be strictly increasing.");
                return;
            }
        }
    }
}

/// <summary>Groups existing claims under a new parent structure element.</summary>
public sealed record GroupRemediationAction(PdfName ParentTag, ClaimPredicate Over) : RemediationAction
{
    public override RemediationActionKind Kind => RemediationActionKind.Group;

    public override string DebugString => $"Group({ParentTag.Value}, over: {Over.DebugString})";

    internal override void Validate(Rule rule, List<string> errors)
    {
        if (rule.Stage != Stage.Group)
        {
            errors.Add("Group action is only valid in the Group stage.");
        }
    }
}

/// <summary>Merges existing leaf claim content directly into a new parent structure element.</summary>
public sealed record MergeRemediationAction(PdfName TargetTag, ClaimPredicate Over, PdfDictionary? Attributes = null) : RemediationAction
{
    public override RemediationActionKind Kind => RemediationActionKind.Merge;

    public override string DebugString => $"MergeTo({TargetTag.Value}, over: {Over.DebugString})";

    internal override void Validate(Rule rule, List<string> errors)
    {
        if (rule.Stage != Stage.Group)
        {
            errors.Add("MergeTo action is only valid in the Group stage.");
        }
    }
}

/// <summary>Reorders matched sibling structure nodes within their current parent.</summary>
public sealed record ReorderSiblingsRemediationAction(ClaimPredicate Over, SiblingReorderMode Mode) : RemediationAction
{
    public override RemediationActionKind Kind => RemediationActionKind.ReorderSiblings;

    public override string DebugString => $"ReorderSiblings(over: {Over.DebugString}, by: {Mode})";

    internal override void Validate(Rule rule, List<string> errors)
    {
        if (rule.Stage != Stage.Refine)
        {
            errors.Add("ReorderSiblings action is only valid in the Refine stage.");
        }
    }
}

/// <summary>Creates or refines a structure link from source claims to target claims.</summary>
public sealed record StructureLinkRemediationAction(
    ClaimPredicate Source,
    ClaimPredicate Target,
    string AccessibleDescription,
    PdfArray? DestinationTemplate = null) : RemediationAction
{
    public override RemediationActionKind Kind => RemediationActionKind.Link;

    public override string DebugString => $"Link(source: {Source.DebugString}, target: {Target.DebugString})";

    internal override void Validate(Rule rule, List<string> errors)
    {
        if (rule.Stage != Stage.Refine)
        {
            errors.Add("Link actions are only valid in the Refine stage.");
        }

        if (string.IsNullOrWhiteSpace(AccessibleDescription))
        {
            errors.Add("Link actions require a non-empty accessible description.");
        }
    }
}

/// <summary>Applies structure attributes to existing claims.</summary>
public sealed record StructureAttributeRemediationAction(
    ClaimPredicate Over,
    string? Language = null,
    string? Alt = null,
    string? ActualText = null,
    string? Expansion = null,
    PdfDictionary? Attributes = null) : RemediationAction
{
    public override RemediationActionKind Kind => RemediationActionKind.StructureAttributes;

    public override string DebugString => $"Attributes(over: {Over.DebugString})";

    internal override void Validate(Rule rule, List<string> errors)
    {
        if (rule.Stage != Stage.Refine)
        {
            errors.Add("Structure attribute actions are only valid in the Refine stage.");
        }
    }
}

/// <summary>Advanced escape hatch for custom remediation behavior outside full declarative validation.</summary>
public sealed record CustomRemediationAction(CustomRemediationHandler Handler, string Description) : RemediationAction
{
    /// <summary>False because custom hooks cannot be fully validated before execution.</summary>
    public bool ParticipatesInFullPreflightValidation => false;

    public override RemediationActionKind Kind => RemediationActionKind.Custom;

    public override string DebugString => $"Custom({Description})";
}

/// <summary>
/// Delegate invoked by custom remediation actions.
/// </summary>
public delegate CustomRemediationOutcome CustomRemediationHandler(CustomRemediationContext context);

/// <summary>
/// Context supplied to a custom remediation action.
/// </summary>
public sealed record CustomRemediationContext(
    RemediationSession Session,
    RemediationEvaluationContext Evaluation,
    IReadOnlyList<RemediationCandidate> Candidates);

/// <summary>
/// Result returned by a custom remediation action.
/// </summary>
public sealed record CustomRemediationOutcome(
    IReadOnlyList<RemediationCandidate> ClaimedCandidates,
    RemediationAction? Action = null,
    PdfDictionary? StructureAttributes = null);

/// <summary>
/// Supported PDF artifact subtypes.
/// </summary>
public enum ArtifactSubtype
{
    /// <summary>Pagination artifacts such as page numbers and running headers.</summary>
    Pagination,
    /// <summary>Layout artifacts without semantic meaning.</summary>
    Layout,
    /// <summary>Page-level artifact content.</summary>
    Page,
    /// <summary>Background artwork or decoration.</summary>
    Background
}

/// <summary>
/// Remediation action category.
/// </summary>
public enum RemediationActionKind
{
    /// <summary>Structure tagging action.</summary>
    Tag,
    /// <summary>Artifact marking action.</summary>
    Artifact,
    /// <summary>Table construction action.</summary>
    Table,
    /// <summary>Claim grouping action.</summary>
    Group,
    /// <summary>Claim content merge action.</summary>
    Merge,
    /// <summary>Structure-attribute refinement action.</summary>
    StructureAttributes,
    /// <summary>Sibling reordering action.</summary>
    ReorderSiblings,
    /// <summary>Structure link action.</summary>
    Link,
    /// <summary>Custom action.</summary>
    Custom
}

/// <summary>
/// Sort mode for sibling reordering.
/// </summary>
public enum SiblingReorderMode
{
    /// <summary>Use existing reading order.</summary>
    ReadingOrder,
    /// <summary>Sort by top-to-bottom geometry.</summary>
    GeometryTopToBottom,
    /// <summary>Sort by left-to-right geometry.</summary>
    GeometryLeftToRight
}

/// <summary>
/// Controls how claim-consuming table cells reuse matched child claim structure.
/// </summary>
public enum TableCellContentMode
{
    /// <summary>Keep matched claim structure nodes as children of each TD/TH cell.</summary>
    PreserveChildren,
    /// <summary>Move matched leaf claim MCID references directly onto each TD/TH cell.</summary>
    FlattenLeafClaims
}

/// <summary>
/// Factory helpers for remediation actions.
/// </summary>
public static class RemediationActions
{
    /// <summary>Creates a structure tagging action.</summary>
    public static RemediationAction Tag(string name, PdfDictionary? attributes = null) =>
        new TagRemediationAction((PdfName)name, attributes);

    /// <summary>Creates an artifact marking action.</summary>
    public static RemediationAction Artifact(ArtifactSubtype subtype) => new ArtifactRemediationAction(subtype);

    /// <summary>Creates a table action with explicit column boundaries.</summary>
    public static RemediationAction Table(params double[] columns) => new TableRemediationAction(columns);

    /// <summary>Creates a table action that infers columns.</summary>
    public static RemediationAction Table() => new TableRemediationAction();

    /// <summary>Creates a table action with header rows and explicit column boundaries.</summary>
    public static RemediationAction TableWithHeaderRows(int headerRows, params double[] columns) =>
        new TableRemediationAction(columns, headerRows);

    /// <summary>Creates a claim-consuming table action.</summary>
    public static RemediationAction TableOver(ClaimPredicate over, params double[] columns) =>
        new TableRemediationAction(columns, Over: over);

    /// <summary>Creates a claim-consuming table action with header rows.</summary>
    public static RemediationAction TableOver(ClaimPredicate over, int headerRows, params double[] columns) =>
        new TableRemediationAction(columns, headerRows, over);

    /// <summary>Creates a claim-consuming table action with an explicit header selector.</summary>
    public static RemediationAction TableOver(ClaimPredicate over, ClaimPredicate headerSelector, params double[] columns) =>
        new TableRemediationAction(columns, Over: over, HeaderSelector: headerSelector);

    /// <summary>Creates a claim-consuming table action that flattens matched leaf claims directly into TD/TH cells.</summary>
    public static RemediationAction TableOverFlattenedCells(ClaimPredicate over, params double[] columns) =>
        new TableRemediationAction(columns, Over: over, CellContentMode: TableCellContentMode.FlattenLeafClaims);

    /// <summary>Creates a claim-consuming table action with header rows that flattens matched leaf claims directly into TD/TH cells.</summary>
    public static RemediationAction TableOverFlattenedCells(ClaimPredicate over, int headerRows, params double[] columns) =>
        new TableRemediationAction(columns, headerRows, Over: over, CellContentMode: TableCellContentMode.FlattenLeafClaims);

    /// <summary>Creates a claim-consuming table action with an explicit header selector that flattens matched leaf claims directly into TD/TH cells.</summary>
    public static RemediationAction TableOverFlattenedCells(ClaimPredicate over, ClaimPredicate headerSelector, params double[] columns) =>
        new TableRemediationAction(columns, Over: over, HeaderSelector: headerSelector, CellContentMode: TableCellContentMode.FlattenLeafClaims);

    /// <summary>Creates a claim-grouping action.</summary>
    public static RemediationAction Group(string parentTag, ClaimPredicate over) =>
        new GroupRemediationAction((PdfName)parentTag, over);

    /// <summary>Creates a claim-content merge action.</summary>
    public static RemediationAction MergeTo(string targetTag, ClaimPredicate over, PdfDictionary? attributes = null) =>
        new MergeRemediationAction((PdfName)targetTag, over, attributes);

    /// <summary>Creates a scoped sibling-reorder action.</summary>
    public static RemediationAction ReorderSiblings(ClaimPredicate over, SiblingReorderMode mode) =>
        new ReorderSiblingsRemediationAction(over, mode);

    /// <summary>Creates a sibling-reorder action over all eligible claims.</summary>
    public static RemediationAction ReorderSiblings(SiblingReorderMode mode) =>
        new ReorderSiblingsRemediationAction(ClaimPredicate.Always, mode);

    /// <summary>Creates a structure-link action.</summary>
    public static RemediationAction Link(
        ClaimPredicate source,
        ClaimPredicate target,
        string accessibleDescription,
        PdfArray? destinationTemplate = null) =>
        new StructureLinkRemediationAction(source, target, accessibleDescription, destinationTemplate);

    /// <summary>Creates an action that sets the language of matched claims.</summary>
    public static RemediationAction Lang(ClaimPredicate over, string language) =>
        new StructureAttributeRemediationAction(over, Language: language);

    /// <summary>Creates an action that sets alternate text on matched claims.</summary>
    public static RemediationAction Alt(ClaimPredicate over, string alt) =>
        new StructureAttributeRemediationAction(over, Alt: alt);

    /// <summary>Creates an action that sets actual text on matched claims.</summary>
    public static RemediationAction ActualText(ClaimPredicate over, string actualText) =>
        new StructureAttributeRemediationAction(over, ActualText: actualText);

    /// <summary>Creates an action that sets expansion text on matched claims.</summary>
    public static RemediationAction Expansion(ClaimPredicate over, string expansion) =>
        new StructureAttributeRemediationAction(over, Expansion: expansion);

    /// <summary>Creates an action that applies arbitrary structure attributes to matched claims.</summary>
    public static RemediationAction Attributes(ClaimPredicate over, PdfDictionary attributes) =>
        new StructureAttributeRemediationAction(over, Attributes: attributes);

    /// <summary>Creates a custom remediation action.</summary>
    public static RemediationAction Custom(CustomRemediationHandler handler, string description) =>
        new CustomRemediationAction(handler, description);
}
