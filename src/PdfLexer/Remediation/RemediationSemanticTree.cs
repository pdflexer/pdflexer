namespace PdfLexer.Remediation;

using PdfLexer.Content;
using PdfLexer.DOM;

/// <summary>Immutable semantic structure planned by a compiled remediation program.</summary>
public sealed record RemediationSemanticTree(IReadOnlyList<RemediationSemanticNode> Roots)
{
    internal static RemediationSemanticTree FromClaims(
        CompiledRemediationProgram compiled,
        IReadOnlyList<RemediationClaim> claims,
        PrescriptiveTemplateAssemblyPlan? assemblyPlan = null)
    {
        ArgumentNullException.ThrowIfNull(compiled);
        ArgumentNullException.ThrowIfNull(claims);

        var structuralClaims = claims
            .Where(x => x.Status == ClaimStatus.Applied && TryGetProgramSlot(compiled, x, out _))
            .DistinctBy(x => x.ClaimId)
            .ToArray();
        var children = structuralClaims
            .SelectMany(x => x.RelatedClaims)
            .Where(x => TryGetProgramSlot(compiled, x, out _))
            .Select(x => x.ClaimId)
            .ToHashSet();
        var roots = structuralClaims
            .Where(x => !children.Contains(x.ClaimId))
            .OrderBy(x => x.PageIndex)
            .ThenBy(x => x, RemediationSession.ReadingOrderComparer)
            .Select(x => BuildProgramNode(x, compiled, new HashSet<ClaimId>()))
            .ToArray();

        var tree = new RemediationSemanticTree(roots);
        return assemblyPlan?.ProjectSemanticTree() ?? tree;
    }

    private static RemediationSemanticNode BuildProgramNode(
        RemediationClaim claim,
        CompiledRemediationProgram compiled,
        HashSet<ClaimId> ancestors)
    {
        if (!ancestors.Add(claim.ClaimId))
        {
            return CreateProgramNode(claim, compiled, Array.Empty<RemediationSemanticNode>());
        }

        var related = claim.RelatedClaims
            .Where(x => TryGetProgramSlot(compiled, x, out _))
            .OrderBy(x => x, RemediationSession.ReadingOrderComparer)
            .Select(x => BuildProgramNode(x, compiled, new HashSet<ClaimId>(ancestors)))
            .ToArray();
        return CreateProgramNode(claim, compiled, related);
    }

    private static RemediationSemanticNode CreateProgramNode(
        RemediationClaim claim,
        CompiledRemediationProgram compiled,
        IReadOnlyList<RemediationSemanticNode> children)
    {
        if (!TryGetProgramSlot(compiled, claim, out var slot) ||
            !compiled.Slots.TryGetValue(slot, out var descriptor))
        {
            throw new InvalidOperationException(
                $"Program claim '{claim.ClaimId}' does not resolve to a compiled template slot.");
        }

        return new RemediationSemanticNode(
            claim.ClaimId,
            descriptor.Node.Tag,
            null,
            claim.BindingId ?? claim.RuleId,
            claim.Candidates.Select(x => x.CandidateId).ToArray(),
            claim.Candidates.SelectMany(x => x.SourceReferences).Distinct().ToArray(),
            claim.PageIndexes,
            slot.Path[1..],
            children)
        {
            SlotReference = slot
        };
    }

    private static bool TryGetProgramSlot(
        CompiledRemediationProgram compiled,
        RemediationClaim claim,
        out SlotRef slot)
    {
        slot = null!;
        if (claim.ProgramSlot != null && compiled.Slots.ContainsKey(claim.ProgramSlot))
        {
            slot = claim.ProgramSlot;
            return true;
        }

        if (string.IsNullOrWhiteSpace(claim.SlotId)) return false;
        var path = claim.SlotId!.StartsWith("/", StringComparison.Ordinal)
            ? claim.SlotId
            : "/" + claim.SlotId;
        if (!SlotRef.TryParse(path, out var parsed) || parsed == null ||
            !compiled.Slots.ContainsKey(parsed))
        {
            return false;
        }

        slot = parsed;
        return true;
    }

    internal static RemediationSemanticTree FromStructure(
        StructureNode root,
        IReadOnlyList<RemediationClaim> claims,
        IReadOnlyDictionary<(string? ProgramId, string RuleId), string?> slots)
    {
        var nodeComparer = (IEqualityComparer<StructureNode>)ReferenceEqualityComparer.Instance;
        var byNode = claims
            .SelectMany(claim => claim.AppliedBindings
                .Where(binding => binding.StructureNode != null)
                .Select(binding => (Node: binding.StructureNode!, Claim: claim, Binding: binding)))
            .GroupBy(x => x.Node, nodeComparer)
            .ToDictionary(x => x.Key, x => (x.First().Claim, x.First().Binding), nodeComparer);

        RemediationSemanticNode Build(StructureNode node, RemediationClaim? inherited)
        {
            var isDirect = byNode.TryGetValue(node, out var direct);
            var claim = isDirect ? direct.Claim : inherited;
            var slot = isDirect && claim?.ProgramSlot != null
                ? claim.ProgramSlot.Path[1..]
                : TryParseTemplateSlot(node.ID);
            var children = node.Children.Select(x => Build(x, claim)).ToArray();
            var programSlot = claim?.ProgramSlot ??
                (TemplateOccurrenceIdentity.TryParseSlot(node.ID, out var parsedSlot)
                    ? parsedSlot
                    : null);
            return new RemediationSemanticNode(
                claim?.ClaimId ?? default,
                node.Type,
                null,
                claim?.BindingId ?? claim?.RuleId ?? string.Empty,
                claim?.Candidates.Select(x => x.CandidateId).ToArray() ?? Array.Empty<string>(),
                claim?.Candidates.SelectMany(x => x.SourceReferences).Distinct().ToArray() ??
                    Array.Empty<StructuredSourceRef>(),
                claim?.PageIndexes ?? Array.Empty<int>(),
                slot,
                children)
            {
                TemplateIdentity = node.ID?.StartsWith("template:", StringComparison.Ordinal) == true ? node.ID : null,
                SlotReference = programSlot
            };
        }

        return new RemediationSemanticTree(root.Children.Select(x => Build(x, null)).ToArray());
    }

    private static string? TryParseTemplateSlot(string? id)
    {
        if (!TemplateOccurrenceIdentity.TryParseSlot(id, out var slot) || slot == null)
        {
            return null;
        }

        return slot.Path[1..];
    }
}

public sealed record RemediationSemanticNode(
    ClaimId ClaimId,
    string Tag,
    string? ProgramId,
    string RuleId,
    IReadOnlyList<string> CandidateIds,
    IReadOnlyList<StructuredSourceRef> SourceReferences,
    IReadOnlyList<int> PageIndexes,
    string? SlotId,
    IReadOnlyList<RemediationSemanticNode> Children)
{
    /// <summary>Declared template path for a prescriptive occurrence.</summary>
    public string? TemplatePath { get; init; }

    /// <summary>One-based occurrence within the declared template particle.</summary>
    public int? TemplateOccurrenceIndex { get; init; }

    /// <summary>Deterministic identity rendered into the committed structure ID.</summary>
    public string? TemplateIdentity { get; init; }

    /// <summary>Whether a specialized action owns the node's internal structure.</summary>
    public bool OpaqueTemplateInterior { get; init; }

    /// <summary>Canonical compiled-program slot for this node.</summary>
    internal SlotRef? SlotReference { get; init; }
}

