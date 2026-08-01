namespace PdfLexer.Remediation;

/// <summary>Page furniture explicitly owned by a prescriptive program.</summary>
public sealed record ArtifactDeclaration
{
    public ArtifactDeclaration(string id, ArtifactSubtype subtype, PageSelector? pages = null,
        AssertionCount? occurrence = null, ArtifactSemanticSubtype? semanticSubtype = null,
        bool includeBoundingBox = false, IReadOnlyList<ArtifactAttachmentEdge>? attached = null)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Artifact id is required.", nameof(id));
        Id = id;
        Subtype = subtype;
        Pages = pages ?? PageSelector.Every;
        Occurrence = occurrence ?? AssertionCount.Exactly(1);
        SemanticSubtype = semanticSubtype;
        IncludeBoundingBox = includeBoundingBox;
        Attached = Array.AsReadOnly((attached ?? Array.Empty<ArtifactAttachmentEdge>()).ToArray());
    }

    public string Id { get; }
    public ArtifactSubtype Subtype { get; }
    public PageSelector Pages { get; }
    public AssertionCount Occurrence { get; }
    public ArtifactSemanticSubtype? SemanticSubtype { get; }
    public bool IncludeBoundingBox { get; }
    public IReadOnlyList<ArtifactAttachmentEdge> Attached { get; }

    internal IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();
        if (!Enum.IsDefined(Subtype)) errors.Add($"Artifact '{Id}' has unsupported subtype '{Subtype}'.");
        if (SemanticSubtype != null && Subtype != ArtifactSubtype.Pagination)
            errors.Add($"Artifact '{Id}' uses a semantic subtype with non-Pagination type '{Subtype}'.");
        if (Attached.Distinct().Count() != Attached.Count || Attached.Any(x => !Enum.IsDefined(x)))
            errors.Add($"Artifact '{Id}' has invalid or duplicate attachment edges.");
        if (Occurrence.Min < 0 || Occurrence.Max is { } max && max < Occurrence.Min)
            errors.Add($"Artifact '{Id}' has an invalid occurrence range.");
        return errors;
    }
}
