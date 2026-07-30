namespace PdfLexer.Remediation;

/// <summary>
/// Declared page furniture expected to be artifacted. Occurrence is evaluated per page for every
/// page the selector includes.
/// </summary>
public sealed record RemediationArtifactInventoryItem
{
    /// <summary>Creates an artifact inventory item.</summary>
    public RemediationArtifactInventoryItem(
        string id,
        ArtifactSubtype subtype,
        PageSelector? pages = null,
        string? zoneId = null,
        AssertionCount? occurrence = null,
        ArtifactSemanticSubtype? semanticSubtype = null,
        bool includeBoundingBox = false,
        IReadOnlyList<ArtifactAttachmentEdge>? attached = null)
    {
        Id = id;
        Subtype = subtype;
        Pages = pages ?? PageSelector.Every;
        ZoneId = zoneId;
        Occurrence = occurrence ?? AssertionCount.Exactly(1);
        SemanticSubtype = semanticSubtype;
        IncludeBoundingBox = includeBoundingBox;
        Attached = attached ?? Array.Empty<ArtifactAttachmentEdge>();
    }

    /// <summary>Stable inventory item identifier, unique across composed rule sets.</summary>
    public string Id { get; }

    /// <summary>Artifact subtype the furniture is marked with.</summary>
    public ArtifactSubtype Subtype { get; }

    /// <summary>Optional PDF artifact /Subtype.</summary>
    public ArtifactSemanticSubtype? SemanticSubtype { get; }

    /// <summary>Whether absorbed candidates emit their bounds as /BBox.</summary>
    public bool IncludeBoundingBox { get; }

    /// <summary>Page edges emitted in /Attached.</summary>
    public IReadOnlyList<ArtifactAttachmentEdge> Attached { get; }

    /// <summary>Pages the furniture is expected on.</summary>
    public PageSelector Pages { get; }

    /// <summary>Optional toleranced zone the furniture is expected inside.</summary>
    public string? ZoneId { get; }

    /// <summary>Expected per-page occurrence.</summary>
    public AssertionCount Occurrence { get; }

    /// <summary>Identifier of the rule set that declared this item.</summary>
    public string? RuleSetId { get; init; }

    /// <summary>Validates declaration shape.</summary>
    public IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(Id))
        {
            errors.Add("Artifact inventory item id is required.");
        }

        if (!Enum.IsDefined(typeof(ArtifactSubtype), Subtype))
        {
            errors.Add($"Artifact inventory item '{Id}' has unsupported subtype '{Subtype}'.");
        }

        if (SemanticSubtype != null && Subtype != ArtifactSubtype.Pagination)
        {
            errors.Add($"Artifact inventory item '{Id}' uses a semantic subtype with non-Pagination type '{Subtype}'.");
        }
        if (Attached.Distinct().Count() != Attached.Count || Attached.Any(x => !Enum.IsDefined(typeof(ArtifactAttachmentEdge), x)))
        {
            errors.Add($"Artifact inventory item '{Id}' has invalid or duplicate /Attached edges.");
        }

        if (Occurrence.Min < 0 || Occurrence.Max is { } max && max < Occurrence.Min)
        {
            errors.Add($"Artifact inventory item '{Id}' has an invalid occurrence range.");
        }

        return errors;
    }
}
