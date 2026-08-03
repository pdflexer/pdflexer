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
    ReadingOrderDrift = 5,
    /// <summary>A rule matched a number of inputs outside its declared cardinality.</summary>
    RuleCardinalityMismatch = 6,
    /// <summary>A semantic output assertion was not satisfied.</summary>
    SemanticAssertionFailed = 7,
    /// <summary>An annotation present in the input cannot be adopted by the declarative rule model.</summary>
    UnmodeledAnnotation = 8,
    TemplateMissingRequired = 9,
    TemplateUnexpectedNode = 10,
    TemplateWrongOrder = 11,
    TemplateOccurrenceViolation = 12,
    TemplateIllegalNesting = 13,
    TemplateSlotUnfilled = 14,
    TemplatePageMismatch = 15,
    TemplatePageSpanMismatch = 16,
    TemplateMaterializationDivergence = 17,
    /// <summary>Content was artifacted that matches no declared artifact inventory item.</summary>
    ArtifactUndeclared = 18,
    /// <summary>A declared artifact inventory item produced nothing where it is required.</summary>
    ArtifactMissingDeclared = 19,
    /// <summary>A declared artifact inventory item occurred outside its expected per-page count.</summary>
    ArtifactOccurrenceViolation = 20,
    /// <summary>Two structural consumers selected the same claim or an ancestor and its descendant.</summary>
    GroupCompositionAmbiguous = 21,
    /// <summary>The structural claim graph contains a cycle.</summary>
    GroupCompositionCycle = 22,
    /// <summary>An annotation adoption target could not be resolved.</summary>
    AnnotationAdoptionTargetMissing = 23,
    /// <summary>An annotation adoption target was ambiguous.</summary>
    AnnotationAdoptionAmbiguous = 24,
    /// <summary>An annotation was selected by more than one consumer.</summary>
    AnnotationAlreadyConsumed = 25,
    /// <summary>Painting content was not bound or declared as an artifact under a prescriptive template.</summary>
    PrescriptiveUnaccountedContent = 26,
    /// <summary>A deterministic template occurrence identity collides with another structure node.</summary>
    TemplateIdentityCollision = 27,
    /// <summary>The selected program contains an invalid runtime configuration.</summary>
    ProgramConfigurationInvalid = 28,
    /// <summary>Two program bindings compete for the same candidate or ownership range.</summary>
    ProgramBindingConflict = 29,
    /// <summary>A program binding produced an ambiguous result.</summary>
    ProgramBindingAmbiguous = 30,
    /// <summary>A slot anchor could not resolve a bounded claim on the selected page.</summary>
    ProgramAnchorUnresolved = 31,
    /// <summary>A slot anchor resolved more than one bounded claim.</summary>
    ProgramAnchorAmbiguous = 32,
    /// <summary>Program materialization diverged from its compiled plan.</summary>
    ProgramMaterializationFailed = 33,
    /// <summary>A declared program region could not be resolved.</summary>
    ProgramRegionResolutionFailed = 34,
    /// <summary>Two equal-priority region accounting declarations selected one candidate.</summary>
    ProgramRegionAccountingAmbiguous = 35,
    /// <summary>Region accounting selected content already owned by a structural binding.</summary>
    ProgramRegionAccountingConflict = 36
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
