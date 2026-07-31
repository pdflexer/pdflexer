namespace PdfLexer.Remediation;

/// <summary>Immutable semantic structure planned by remediation actions.</summary>
public sealed record RemediationSemanticTree(IReadOnlyList<RemediationSemanticNode> Roots)
{
    internal static RemediationSemanticTree FromClaims(
        IReadOnlyList<RemediationClaim> claims,
        IReadOnlyDictionary<(string? RuleSetId, string RuleId), string?>? slots = null,
        IReadOnlyDictionary<(string? RuleSetId, string RuleId), RemediationAction>? actions = null,
        PrescriptiveTemplateAssemblyPlan? assemblyPlan = null)
    {
        var structuralClaims = claims
            .Where(x => x.Status == ClaimStatus.Applied && IsStructural(x, actions))
            .ToArray();
        var children = structuralClaims
            .SelectMany(x => x.RelatedClaims)
            .Where(x => IsStructural(x, actions))
            .Select(x => x.ClaimId)
            .ToHashSet();
        var roots = structuralClaims
            .Where(x => !children.Contains(x.ClaimId))
            .OrderBy(x => x.PageIndex)
            .ThenBy(x => x, RemediationSession.ReadingOrderComparer)
            .Select(x => BuildNode(x, new HashSet<ClaimId>(), slots, actions))
            .ToArray();
        var tree = new RemediationSemanticTree(roots);
        return assemblyPlan?.ProjectSemanticTree() ?? tree;
    }

    private static RemediationSemanticNode BuildNode(
        RemediationClaim claim,
        HashSet<ClaimId> ancestors,
        IReadOnlyDictionary<(string? RuleSetId, string RuleId), string?>? slots,
        IReadOnlyDictionary<(string? RuleSetId, string RuleId), RemediationAction>? actions)
    {
        if (!ancestors.Add(claim.ClaimId))
        {
            return new RemediationSemanticNode(
                claim.ClaimId, claim.ProducedTag, claim.RuleSetId, claim.RuleId,
                claim.Candidates.Select(x => x.CandidateId).ToArray(),
                claim.Candidates.SelectMany(x => x.SourceReferences).Distinct().ToArray(),
                claim.PageIndexes, GetSlot(claim, slots),
                Array.Empty<RemediationSemanticNode>());
        }

        RemediationAction? action = null;
        actions?.TryGetValue((claim.RuleSetId, claim.RuleId), out action);
        if (action is TableRemediationAction table && claim.TablePlan != null)
        {
            return BuildTableNode(claim, table, claim.TablePlan, ancestors, slots, actions);
        }

        var related = action is MergeRemediationAction ||
            action is BindTemplateSlotRemediationAction { ContentMode: TemplateSlotContentMode.FlattenLeafClaims }
            ? Array.Empty<RemediationClaim>()
            : claim.RelatedClaims.Where(x => IsStructural(x, actions));
        return
        new(
            claim.ClaimId,
            claim.ProducedTag,
            claim.RuleSetId,
            claim.RuleId,
            claim.Candidates.Select(x => x.CandidateId).ToArray(),
            claim.Candidates.SelectMany(x => x.SourceReferences).Distinct().ToArray(),
            claim.PageIndexes,
            GetSlot(claim, slots),
            related
                .OrderBy(x => x, RemediationSession.ReadingOrderComparer)
                .Select(x => BuildNode(x, new HashSet<ClaimId>(ancestors), slots, actions))
                .ToArray());
    }

    private static RemediationSemanticNode BuildTableNode(
        RemediationClaim claim,
        TableRemediationAction table,
        RemediationTablePlan plan,
        HashSet<ClaimId> ancestors,
        IReadOnlyDictionary<(string? RuleSetId, string RuleId), string?>? slots,
        IReadOnlyDictionary<(string? RuleSetId, string RuleId), RemediationAction>? actions)
    {
        RemediationSemanticNode Synthetic(
            string tag,
            int pageIndex,
            IReadOnlyList<RemediationSemanticNode> children) => new(
                claim.ClaimId,
                tag,
                claim.RuleSetId,
                claim.RuleId,
                claim.Candidates.Select(x => x.CandidateId).ToArray(),
                claim.Candidates.SelectMany(x => x.SourceReferences).Distinct().ToArray(),
                new[] { pageIndex },
                null,
                children);

        var rows = plan.Rows.Select(row =>
            Synthetic(
                "TR",
                row.PageIndex,
                row.Cells.Select(cell =>
                    Synthetic(
                        cell.Tag,
                        row.PageIndex,
                        table.CellContentMode == TableCellContentMode.FlattenLeafClaims
                            ? Array.Empty<RemediationSemanticNode>()
                            : new[]
                            {
                                BuildNode(
                                    cell.Claim,
                                    new HashSet<ClaimId>(ancestors),
                                    slots,
                                    actions)
                            }))
                    .ToArray()))
            .ToArray();

        return new RemediationSemanticNode(
            claim.ClaimId,
            claim.ProducedTag,
            claim.RuleSetId,
            claim.RuleId,
            claim.Candidates.Select(x => x.CandidateId).ToArray(),
            claim.Candidates.SelectMany(x => x.SourceReferences).Distinct().ToArray(),
            claim.PageIndexes,
            GetSlot(claim, slots),
            rows);
    }

    private static bool IsStructural(
        RemediationClaim claim,
        IReadOnlyDictionary<(string? RuleSetId, string RuleId), RemediationAction>? actions)
    {
        if (claim.Action is AdoptAnnotationRemediationAction adopt)
        {
            return adopt.Into == null;
        }
        if (claim.Action is BindTemplateSlotRemediationAction)
        {
            return true;
        }
        return actions == null ||
            actions.TryGetValue((claim.RuleSetId, claim.RuleId), out var action) &&
            RemediationStructuralTemplateValidator.ProducedTag(action) != null;
    }

    private static string? TemplateSlotFromNodeId(string? id)
    {
        if (id == null || !id.StartsWith("template:Document/", StringComparison.Ordinal)) return null;
        var segment = id[(id.LastIndexOf('/') + 1)..];
        var occurrence = segment.LastIndexOf('[');
        if (occurrence <= 0 || !segment.EndsWith(']')) return null;
        return Uri.UnescapeDataString(segment[..occurrence]);
    }

    private static string? GetSlot(
        RemediationClaim claim,
        IReadOnlyDictionary<(string? RuleSetId, string RuleId), string?>? slots) =>
        claim.SlotId ?? (slots != null && slots.TryGetValue((claim.RuleSetId, claim.RuleId), out var slot) ? slot : null);

    internal static RemediationSemanticTree FromStructure(
        PdfLexer.DOM.StructureNode root,
        IReadOnlyList<RemediationClaim> claims,
        IReadOnlyDictionary<(string? RuleSetId, string RuleId), string?> slots)
    {
        var nodeComparer = (IEqualityComparer<PdfLexer.DOM.StructureNode>)ReferenceEqualityComparer.Instance;
        var byNode = claims
            .SelectMany(claim => claim.AppliedBindings
                .Where(binding => binding.StructureNode != null)
                .Select(binding => (Node: binding.StructureNode!, Claim: claim, Binding: binding)))
            .GroupBy(x => x.Node, nodeComparer)
            .ToDictionary(x => x.Key, x => (x.First().Claim, x.First().Binding), nodeComparer);

        RemediationSemanticNode Build(PdfLexer.DOM.StructureNode node, RemediationClaim? inherited)
        {
            var isDirect = byNode.TryGetValue(node, out var direct);
            var claim = isDirect ? direct.Claim : inherited;
            var slot = isDirect && claim != null &&
                string.Equals(direct.Binding.ProducedTag, claim.ProducedTag, StringComparison.Ordinal)
                    ? GetSlot(claim, slots)
                    : TemplateSlotFromNodeId(node.ID);
            var children = node.Children.Select(x => Build(x, claim)).ToArray();
            return new RemediationSemanticNode(
                claim?.ClaimId ?? default,
                node.Type,
                claim?.RuleSetId,
                claim?.RuleId ?? string.Empty,
                claim?.Candidates.Select(x => x.CandidateId).ToArray() ?? Array.Empty<string>(),
                claim?.Candidates.SelectMany(x => x.SourceReferences).Distinct().ToArray() ??
                    Array.Empty<PdfLexer.Content.StructuredSourceRef>(),
                claim?.PageIndexes ?? Array.Empty<int>(),
                slot,
                children)
            {
                TemplateIdentity = node.ID?.StartsWith("template:Document/", StringComparison.Ordinal) == true ? node.ID : null,
                OpaqueTemplateInterior = isDirect && claim?.Action is TableRemediationAction
            };
        }

        return new RemediationSemanticTree(root.Children.Select(x => Build(x, null)).ToArray());
    }
}

public sealed record RemediationSemanticNode(
    ClaimId ClaimId,
    string Tag,
    string? RuleSetId,
    string RuleId,
    IReadOnlyList<string> CandidateIds,
    IReadOnlyList<PdfLexer.Content.StructuredSourceRef> SourceReferences,
    IReadOnlyList<int> PageIndexes,
    string? SlotId,
    IReadOnlyList<RemediationSemanticNode> Children)
{
    /// <summary>Declared template path for a prescriptive occurrence.</summary>
    public string? TemplatePath { get; init; }

    /// <summary>One-based occurrence within the declared template particle.</summary>
    public int? TemplateOccurrenceIndex { get; init; }

    /// <summary>Durable deterministic identity rendered into the committed structure /ID.</summary>
    public string? TemplateIdentity { get; init; }

    /// <summary>True when a specialized action owns and validates the node's internal structure.</summary>
    public bool OpaqueTemplateInterior { get; init; }
}
