namespace PdfLexer.Remediation;

/// <summary>Immutable semantic structure planned by remediation actions.</summary>
public sealed record RemediationSemanticTree(IReadOnlyList<RemediationSemanticNode> Roots)
{
    internal static RemediationSemanticTree FromClaims(
        IReadOnlyList<RemediationClaim> claims,
        IReadOnlyDictionary<(string? RuleSetId, string RuleId), string?>? slots = null,
        IReadOnlyDictionary<(string? RuleSetId, string RuleId), RemediationAction>? actions = null)
    {
        var children = claims.SelectMany(x => x.RelatedClaims).Select(x => x.ClaimId).ToHashSet();
        var roots = claims
            .Where(x => x.Status == ClaimStatus.Applied &&
                IsStructural(x, actions) &&
                !children.Contains(x.ClaimId))
            .OrderBy(x => x.PageIndex)
            .ThenBy(x => x, RemediationSession.ReadingOrderComparer)
            .Select(x => BuildNode(x, new HashSet<ClaimId>(), slots, actions))
            .ToArray();
        return new RemediationSemanticTree(roots);
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

        var related = action is MergeRemediationAction
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
                GetSlot(claim, slots),
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
        IReadOnlyDictionary<(string? RuleSetId, string RuleId), RemediationAction>? actions) =>
        actions == null ||
        actions.TryGetValue((claim.RuleSetId, claim.RuleId), out var action) &&
        RemediationStructuralTemplateValidator.ProducedTag(action) != null;

    private static string? GetSlot(
        RemediationClaim claim,
        IReadOnlyDictionary<(string? RuleSetId, string RuleId), string?>? slots) =>
        slots != null && slots.TryGetValue((claim.RuleSetId, claim.RuleId), out var slot) ? slot : null;

    internal static RemediationSemanticTree FromStructure(
        PdfLexer.DOM.StructureNode root,
        IReadOnlyList<RemediationClaim> claims,
        IReadOnlyDictionary<(string? RuleSetId, string RuleId), string?> slots)
    {
        var nodeComparer = (IEqualityComparer<PdfLexer.DOM.StructureNode>)ReferenceEqualityComparer.Instance;
        var byNode = claims
            .SelectMany(claim => claim.AppliedBindings
                .Where(binding => binding.StructureNode != null)
                .Select(binding => (Node: binding.StructureNode!, Claim: claim)))
            .GroupBy(x => x.Node, nodeComparer)
            .ToDictionary(x => x.Key, x => x.First().Claim, nodeComparer);

        RemediationSemanticNode Build(PdfLexer.DOM.StructureNode node, RemediationClaim? inherited)
        {
            var claim = byNode.TryGetValue(node, out var direct) ? direct : inherited;
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
                claim == null ? null : GetSlot(claim, slots),
                children);
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
    IReadOnlyList<RemediationSemanticNode> Children);
