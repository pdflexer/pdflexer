namespace PdfLexer.Remediation;

/// <summary>
/// Deterministic prescriptive-template assembly. This is the sole owner of synthesis,
/// occurrence numbering, parent selection, and declared sibling ordering.
/// </summary>
internal sealed class PrescriptiveTemplateAssemblyPlan
{
    private PrescriptiveTemplateAssemblyPlan(
        PrescriptiveTemplateAssemblyNode document,
        IReadOnlyList<RemediationTemplateAssemblyItem> items)
    {
        Document = document;
        Items = items;
    }

    internal PrescriptiveTemplateAssemblyNode Document { get; }
    internal IReadOnlyList<RemediationTemplateAssemblyItem> Items { get; }

    internal static PrescriptiveTemplateAssemblyPlan Build(
        RemediationStructuralTemplate template,
        RemediationSemanticTree source,
        IReadOnlyDictionary<ClaimId, RemediationClaim> claims)
    {
        var index = TemplateIndex.Create(template.Document);
        var document = new PrescriptiveTemplateAssemblyNode(
            template.Document, "Document", 1, "template:Document", null, null, false,
            Array.Empty<PrescriptiveTemplateAssemblyNode>());
        var children = AssembleChildren(
            index.Document,
            source.Roots.Select((node, position) => new SourceNode(node, position)).ToList(),
            document.Identity,
            index,
            claims);
        document = document with { Children = children };

        var items = new List<RemediationTemplateAssemblyItem>();
        AddItems(document.Children, document.Identity, items, claims);
        return new PrescriptiveTemplateAssemblyPlan(document, items);
    }

    internal RemediationSemanticTree ProjectSemanticTree()
    {
        return new RemediationSemanticTree(Document.Children.Select(Project).ToArray());

        static RemediationSemanticNode Project(PrescriptiveTemplateAssemblyNode node)
        {
            if (node.Template == null)
            {
                return node.Source! with { Children = node.Children.Select(Project).ToArray() };
            }

            var children = node.OpaqueInterior
                ? node.Source?.Children ?? Array.Empty<RemediationSemanticNode>()
                : node.Children.Select(Project).ToArray();
            var pages = (node.Source?.PageIndexes ?? Array.Empty<int>())
                .Concat(children.SelectMany(AllPages))
                .Distinct()
                .OrderBy(x => x)
                .ToArray();
            var projected = node.Source ?? new RemediationSemanticNode(
                new ClaimId(node.Identity),
                node.Template.Tag,
                null,
                "__template__",
                Array.Empty<string>(),
                Array.Empty<PdfLexer.Content.StructuredSourceRef>(),
                pages,
                node.Template.Id,
                children);
            return projected with
            {
                Tag = node.Template.Tag,
                SlotId = node.Template.Id,
                PageIndexes = pages,
                Children = children,
                TemplatePath = node.TemplatePath,
                TemplateOccurrenceIndex = node.OccurrenceIndex,
                TemplateIdentity = node.Identity,
                OpaqueTemplateInterior = node.OpaqueInterior
            };
        }

        static IEnumerable<int> AllPages(RemediationSemanticNode node) =>
            node.PageIndexes.Concat(node.Children.SelectMany(AllPages));
    }

    private static IReadOnlyList<PrescriptiveTemplateAssemblyNode> AssembleChildren(
        TemplateDescriptor parent,
        List<SourceNode> sources,
        string parentIdentity,
        TemplateIndex index,
        IReadOnlyDictionary<ClaimId, RemediationClaim> claims)
    {
        var remaining = new List<SourceNode>(sources);
        var result = new List<PrescriptiveTemplateAssemblyNode>();
        foreach (var child in parent.Children)
        {
            var direct = remaining.Where(x => x.Node.SlotId == child.Node.Id).ToList();
            var descendants = remaining.Where(x =>
                    x.Node.SlotId != null && index.IsDescendant(x.Node.SlotId, child.Node.Id!))
                .ToList();
            foreach (var item in direct.Concat(descendants)) remaining.Remove(item);

            if (direct.Count == 0)
            {
                if (descendants.Count > 0 && child.Node.Occurrence is
                    RemediationStructuralOccurrence.ExactlyOne or RemediationStructuralOccurrence.Optional)
                {
                    result.Add(BuildNode(child, null, descendants, 1, parentIdentity, index, claims));
                }
                else
                {
                    result.AddRange(descendants.OrderBy(x => x.Position)
                        .Select(x => BuildUnbound(x.Node, parentIdentity, claims)));
                }
                continue;
            }

            for (var occurrence = 0; occurrence < direct.Count; occurrence++)
            {
                var external = direct.Count == 1 ? descendants : new List<SourceNode>();
                result.Add(BuildNode(
                    child,
                    direct[occurrence].Node,
                    external,
                    occurrence + 1,
                    parentIdentity,
                    index,
                    claims));
            }
            if (direct.Count > 1)
            {
                result.AddRange(descendants.OrderBy(x => x.Position)
                    .Select(x => BuildUnbound(x.Node, parentIdentity, claims)));
            }
        }

        result.AddRange(remaining.OrderBy(x => x.Position)
            .Select(x => BuildUnbound(x.Node, parentIdentity, claims)));
        return result;
    }

    private static PrescriptiveTemplateAssemblyNode BuildNode(
        TemplateDescriptor descriptor,
        RemediationSemanticNode? source,
        IReadOnlyList<SourceNode> externalChildren,
        int occurrence,
        string parentIdentity,
        TemplateIndex index,
        IReadOnlyDictionary<ClaimId, RemediationClaim> claims)
    {
        var identity = $"{parentIdentity}/{Uri.EscapeDataString(descriptor.Node.Id!)}[{occurrence}]";
        var opaque = source != null && claims.TryGetValue(source.ClaimId, out var claim) &&
            claim.Action is TableRemediationAction;
        var childSources = opaque
            ? new List<SourceNode>()
            : (source?.Children ?? Array.Empty<RemediationSemanticNode>())
                .Select((node, position) => new SourceNode(node, position))
                .Concat(externalChildren.Select((x, position) => x with
                {
                    Position = (source?.Children.Count ?? 0) + position
                }))
                .ToList();
        var children = opaque
            ? Array.Empty<PrescriptiveTemplateAssemblyNode>()
            : AssembleChildren(descriptor, childSources, identity, index, claims);
        return new PrescriptiveTemplateAssemblyNode(
            descriptor.Node,
            descriptor.Path,
            occurrence,
            identity,
            source,
            source?.ClaimId,
            opaque,
            children);
    }

    private static PrescriptiveTemplateAssemblyNode BuildUnbound(
        RemediationSemanticNode source,
        string parentIdentity,
        IReadOnlyDictionary<ClaimId, RemediationClaim> claims)
    {
        var children = source.Children.Select(x => BuildUnbound(x, parentIdentity, claims)).ToArray();
        return new PrescriptiveTemplateAssemblyNode(
            null, string.Empty, 0, string.Empty, source, source.ClaimId, false, children);
    }

    private static void AddItems(
        IReadOnlyList<PrescriptiveTemplateAssemblyNode> nodes,
        string parentIdentity,
        List<RemediationTemplateAssemblyItem> items,
        IReadOnlyDictionary<ClaimId, RemediationClaim> claims)
    {
        foreach (var node in nodes)
        {
            if (node.Template != null)
            {
                claims.TryGetValue(node.ClaimId ?? default, out var claim);
                items.Add(new RemediationTemplateAssemblyItem(
                    node.Template.Id!,
                    node.TemplatePath,
                    node.OccurrenceIndex,
                    node.Identity,
                    parentIdentity,
                    claim?.RuleId,
                    claim == null ? null : StableClaimReference(claim),
                    claim?.RelatedClaims.Select(StableClaimReference).ToArray() ?? Array.Empty<string>(),
                    node.Source == null,
                    node.OpaqueInterior));
                AddItems(node.Children, node.Identity, items, claims);
            }
            else
            {
                AddItems(node.Children, parentIdentity, items, claims);
            }
        }
    }

    private static string StableClaimReference(RemediationClaim claim)
    {
        var candidate = claim.Candidates.FirstOrDefault()?.CandidateId;
        return candidate != null
            ? $"{claim.RuleSetId ?? "<adhoc>"}/{claim.RuleId}:{candidate}"
            : $"{claim.RuleSetId ?? "<adhoc>"}/{claim.RuleId}:page-{claim.PageIndex + 1}";
    }

    private readonly record struct SourceNode(RemediationSemanticNode Node, int Position);

    private sealed class TemplateIndex
    {
        private readonly IReadOnlyDictionary<string, TemplateDescriptor> _bySlot;

        private TemplateIndex(TemplateDescriptor document, IReadOnlyDictionary<string, TemplateDescriptor> bySlot)
        {
            Document = document;
            _bySlot = bySlot;
        }

        internal TemplateDescriptor Document { get; }

        internal static TemplateIndex Create(RemediationStructuralTemplateNode document)
        {
            var slots = new Dictionary<string, TemplateDescriptor>(StringComparer.Ordinal);
            TemplateDescriptor Build(RemediationStructuralTemplateNode node, TemplateDescriptor? parent, string path)
            {
                var descriptor = new TemplateDescriptor(node, parent, path, Array.Empty<TemplateDescriptor>());
                var children = node.Children.Select((child, index) =>
                    Build(child, descriptor, RemediationStructuralTemplateMatcher.ExpectedPath(path, node.Children, index)))
                    .ToArray();
                descriptor.Children = children;
                if (node.Id != null) slots[node.Id] = descriptor;
                return descriptor;
            }
            return new TemplateIndex(Build(document, null, "Document"), slots);
        }

        internal bool IsDescendant(string candidateSlot, string ancestorSlot)
        {
            if (!_bySlot.TryGetValue(candidateSlot, out var candidate)) return false;
            for (var parent = candidate.Parent; parent != null; parent = parent.Parent)
            {
                if (parent.Node.Id == ancestorSlot) return true;
            }
            return false;
        }
    }

    private sealed class TemplateDescriptor
    {
        internal TemplateDescriptor(
            RemediationStructuralTemplateNode node,
            TemplateDescriptor? parent,
            string path,
            IReadOnlyList<TemplateDescriptor> children)
        {
            Node = node;
            Parent = parent;
            Path = path;
            Children = children;
        }

        internal RemediationStructuralTemplateNode Node { get; }
        internal TemplateDescriptor? Parent { get; }
        internal string Path { get; }
        internal IReadOnlyList<TemplateDescriptor> Children { get; set; }
    }
}

internal sealed record PrescriptiveTemplateAssemblyNode(
    RemediationStructuralTemplateNode? Template,
    string TemplatePath,
    int OccurrenceIndex,
    string Identity,
    RemediationSemanticNode? Source,
    ClaimId? ClaimId,
    bool OpaqueInterior,
    IReadOnlyList<PrescriptiveTemplateAssemblyNode> Children);
