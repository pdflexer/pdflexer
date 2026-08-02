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

    /// <summary>
    /// Builds prescriptive assembly directly from the public program template. This path does
    /// not lower the template into RemediationStructuralTemplateNode and therefore never reads
    /// ProgramSlot or IdentitySegment.
    /// </summary>
    internal static PrescriptiveTemplateAssemblyPlan Build(
        RemediationTemplate template,
        RemediationSemanticTree source,
        IReadOnlyDictionary<ClaimId, RemediationClaim> claims)
    {
        ArgumentNullException.ThrowIfNull(template);
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(claims);
        return BuildProgram(template, null, source, claims);
    }

    /// <summary>
    /// Builds prescriptive assembly from a validated compiled program and its canonical slot
    /// index. The compiled object is the authoritative template boundary for this overload.
    /// </summary>
    internal static PrescriptiveTemplateAssemblyPlan Build(
        CompiledRemediationProgram compiled,
        RemediationSemanticTree source,
        IReadOnlyDictionary<ClaimId, RemediationClaim> claims,
        IReadOnlyList<RemediationOccurrencePartition>? partitions = null)
    {
        ArgumentNullException.ThrowIfNull(compiled);
        if (!compiled.IsValid)
            throw new ArgumentException(
                "A remediation program must compile without errors before assembly.",
                nameof(compiled));
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(claims);
        return BuildProgram(compiled.Program.Template, compiled, source, claims, partitions);
    }

    private static PrescriptiveTemplateAssemblyPlan BuildProgram(
        RemediationTemplate template,
        CompiledRemediationProgram? compiled,
        RemediationSemanticTree source,
        IReadOnlyDictionary<ClaimId, RemediationClaim> claims,
        IReadOnlyList<RemediationOccurrencePartition>? partitions = null)
    {
        var index = ProgramTemplateIndex.Create(template, compiled);
        var document = new PrescriptiveTemplateAssemblyNode(
            "/",
            1,
            TemplateOccurrenceIdentity.Root(template.Id, template.Version),
            null,
            null,
            false,
            Array.Empty<PrescriptiveTemplateAssemblyNode>())
        {
            ProgramTemplate = template.Document,
            LocalName = "Document"
        };
        var children = AssembleProgramChildren(
            index.Document,
            source.Roots.Select((node, position) => new SourceNode(node, position)).ToList(),
            document.Identity,
            index,
            claims,
            partitions ?? Array.Empty<RemediationOccurrencePartition>());
        document = document with { Children = children };

        var items = new List<RemediationTemplateAssemblyItem>();
        AddProgramItems(document.Children, document.Identity, items, claims);
        return new PrescriptiveTemplateAssemblyPlan(document, items);
    }

    internal RemediationSemanticTree ProjectSemanticTree()
    {
        return new RemediationSemanticTree(Document.Children.Select(Project).ToArray());

        static RemediationSemanticNode Project(PrescriptiveTemplateAssemblyNode node)
        {
            if (node.ProgramTemplate == null)
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
            var tag = node.ProgramTemplate!.Tag;
            var slotId = node.SlotReference?.Path[1..];
            var projected = node.Source ?? new RemediationSemanticNode(
                new ClaimId(node.Identity),
                tag,
                null,
                "__template__",
                Array.Empty<string>(),
                Array.Empty<PdfLexer.Content.StructuredSourceRef>(),
                pages,
                slotId,
                children);
            return projected with
            {
                Tag = tag,
                SlotId = slotId,
                PageIndexes = pages,
                Children = children,
                TemplatePath = node.TemplatePath,
                TemplateOccurrenceIndex = node.OccurrenceIndex,
                TemplateIdentity = node.Identity,
                SlotReference = node.SlotReference,
                OpaqueTemplateInterior = node.OpaqueInterior
            };
        }

        static IEnumerable<int> AllPages(RemediationSemanticNode node) =>
            node.PageIndexes.Concat(node.Children.SelectMany(AllPages));
    }

    private static IReadOnlyList<PrescriptiveTemplateAssemblyNode> AssembleProgramChildren(
        ProgramTemplateDescriptor parent,
        List<SourceNode> sources,
        string parentIdentity,
        ProgramTemplateIndex index,
        IReadOnlyDictionary<ClaimId, RemediationClaim> claims,
        IReadOnlyList<RemediationOccurrencePartition> partitions)
    {
        var remaining = new List<SourceNode>(sources);
        var result = new List<PrescriptiveTemplateAssemblyNode>();
        foreach (var child in parent.Children)
        {
            var declaredPartitions = child.Slot == null
                ? Array.Empty<RemediationOccurrencePartition>()
                : partitions.Where(x => x.CompositeSlot == child.Slot &&
                        string.Equals(x.ParentOccurrenceIdentity, parentIdentity, StringComparison.Ordinal))
                    .ToArray();
            var direct = remaining
                .Where(x => SourceSlot(x.Node) == child.Slot)
                .ToList();
            var descendants = remaining
                .Where(x => SourceSlot(x.Node) is { } slot && index.IsDescendant(slot, child.Slot!))
                .ToList();
            foreach (var item in direct.Concat(descendants)) remaining.Remove(item);

            if (declaredPartitions.Length > 0)
            {
                var available = direct.Concat(descendants).ToArray();
                for (var occurrence = 0; occurrence < declaredPartitions.Length; occurrence++)
                {
                    var partition = declaredPartitions[occurrence];
                    var assigned = partition.AssignedClaims.ToHashSet(StringComparer.Ordinal);
                    var sourcesForOccurrence = available
                        .Where(x => assigned.Contains(x.Node.ClaimId.Value))
                        .OrderBy(x => x.Position)
                        .ToList();
                    result.Add(BuildProgramNode(
                        child, null, sourcesForOccurrence, occurrence + 1,
                        parentIdentity, index, claims, partitions));
                }
                continue;
            }

            if (direct.Count == 0)
            {
                if (descendants.Count > 0 && child.Node.Occurrence is
                    TemplateOccurrence.ExactlyOne or TemplateOccurrence.Optional)
                {
                    result.Add(BuildProgramNode(
                        child, null, descendants, 1, parentIdentity, index, claims, partitions));
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
                result.Add(BuildProgramNode(
                    child,
                    direct[occurrence].Node,
                    external,
                    occurrence + 1,
                    parentIdentity,
                    index,
                    claims,
                    partitions));
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

    private static PrescriptiveTemplateAssemblyNode BuildProgramNode(
        ProgramTemplateDescriptor descriptor,
        RemediationSemanticNode? source,
        IReadOnlyList<SourceNode> externalChildren,
        int occurrence,
        string parentIdentity,
        ProgramTemplateIndex index,
        IReadOnlyDictionary<ClaimId, RemediationClaim> claims,
        IReadOnlyList<RemediationOccurrencePartition> partitions)
    {
        var identity = TemplateOccurrenceIdentity.Append(
            parentIdentity,
            descriptor.Slot!,
            descriptor.Node.Name!,
            occurrence);
        var opaque = false;
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
            : AssembleProgramChildren(descriptor, childSources, identity, index, claims, partitions);
        return new PrescriptiveTemplateAssemblyNode(
            descriptor.Slot!.Path,
            occurrence,
            identity,
            source,
            source?.ClaimId,
            opaque,
            children)
        {
            ProgramTemplate = descriptor.Node,
            SlotReference = descriptor.Slot,
            LocalName = descriptor.Node.Name
        };
    }

    private static SlotRef? SourceSlot(RemediationSemanticNode node)
    {
        if (node.SlotReference != null) return node.SlotReference;
        if (string.IsNullOrWhiteSpace(node.SlotId)) return null;

        var value = node.SlotId!;
        var path = value.StartsWith("/", StringComparison.Ordinal) ? value : "/" + value;
        return SlotRef.TryParse(path, out var slot) ? slot : null;
    }

    private static PrescriptiveTemplateAssemblyNode BuildUnbound(
        RemediationSemanticNode source,
        string parentIdentity,
        IReadOnlyDictionary<ClaimId, RemediationClaim> claims)
    {
        var children = source.Children.Select(x => BuildUnbound(x, parentIdentity, claims)).ToArray();
        return new PrescriptiveTemplateAssemblyNode(
            string.Empty, 0, string.Empty, source, source.ClaimId, false, children);
    }

    private static void AddProgramItems(
        IReadOnlyList<PrescriptiveTemplateAssemblyNode> nodes,
        string parentIdentity,
        List<RemediationTemplateAssemblyItem> items,
        IReadOnlyDictionary<ClaimId, RemediationClaim> claims)
    {
        foreach (var node in nodes)
        {
            if (node.ProgramTemplate != null)
            {
                claims.TryGetValue(node.ClaimId ?? default, out var claim);
                var slot = node.SlotReference;
                var slotId = slot?.Path[1..] ?? string.Empty;
                items.Add(new RemediationTemplateAssemblyItem(
                    slotId,
                    node.TemplatePath,
                    node.OccurrenceIndex,
                    node.Identity,
                    parentIdentity,
                    null,
                    claim == null ? null : StableProgramClaimReference(claim),
                    claim?.RelatedClaims.Select(StableProgramClaimReference).ToArray() ??
                        Array.Empty<string>(),
                    node.Source == null,
                    node.OpaqueInterior)
                {
                    ProgramSlot = slot,
                    BindingId = claim?.BindingId
                });
                AddProgramItems(node.Children, node.Identity, items, claims);
            }
            else
            {
                AddProgramItems(node.Children, parentIdentity, items, claims);
            }
        }
    }

    private static string StableProgramClaimReference(RemediationClaim claim)
    {
        var candidate = claim.Candidates.FirstOrDefault()?.CandidateId;
        return candidate != null
            ? $"claim:{claim.ClaimId.Value}:{candidate}"
            : $"claim:{claim.ClaimId.Value}:page-{claim.PageIndex + 1}";
    }

    private readonly record struct SourceNode(RemediationSemanticNode Node, int Position);

    private sealed class ProgramTemplateIndex
    {
        private readonly IReadOnlyDictionary<SlotRef, ProgramTemplateDescriptor> _bySlot;

        private ProgramTemplateIndex(
            ProgramTemplateDescriptor document,
            IReadOnlyDictionary<SlotRef, ProgramTemplateDescriptor> bySlot)
        {
            Document = document;
            _bySlot = bySlot;
        }

        internal ProgramTemplateDescriptor Document { get; }

        internal static ProgramTemplateIndex Create(
            RemediationTemplate template,
            CompiledRemediationProgram? compiled)
        {
            var slots = new Dictionary<SlotRef, ProgramTemplateDescriptor>();
            var root = SlotRef.Absolute("/");

            ProgramTemplateDescriptor Build(
                RemediationTemplateNode node,
                ProgramTemplateDescriptor? parent,
                SlotRef parentSlot,
                string templatePath)
            {
                SlotRef? slot = parent == null
                    ? null
                    : SlotRef.Absolute($"{parentSlot.Path.TrimEnd('/')}/{node.Name}");
                if (slot != null && compiled?.Slots.TryGetValue(slot, out var compiledSlot) == true)
                {
                    slot = compiledSlot.Reference;
                }

                var descriptor = new ProgramTemplateDescriptor(node, parent, slot, templatePath);
                var children = node.Children
                    .Select(child => Build(
                        child,
                        descriptor,
                        slot ?? root,
                        $"{templatePath}/{child.Name}"))
                    .ToArray();
                descriptor.Children = children;
                if (slot != null) slots[slot] = descriptor;
                return descriptor;
            }

            return new ProgramTemplateIndex(
                Build(template.Document, null, root, "Document"),
                slots);
        }

        internal bool IsDescendant(SlotRef candidate, SlotRef ancestor)
        {
            if (!_bySlot.ContainsKey(candidate) || candidate.IsRoot) return false;
            return ancestor.IsRoot ||
                candidate.Path.StartsWith(ancestor.Path + "/", StringComparison.Ordinal);
        }
    }

    private sealed class ProgramTemplateDescriptor
    {
        internal ProgramTemplateDescriptor(
            RemediationTemplateNode node,
            ProgramTemplateDescriptor? parent,
            SlotRef? slot,
            string templatePath)
        {
            Node = node;
            Parent = parent;
            Slot = slot;
            TemplatePath = templatePath;
            Children = Array.Empty<ProgramTemplateDescriptor>();
        }

        internal RemediationTemplateNode Node { get; }
        internal ProgramTemplateDescriptor? Parent { get; }
        internal SlotRef? Slot { get; }
        internal string TemplatePath { get; }
        internal IReadOnlyList<ProgramTemplateDescriptor> Children { get; set; }
    }

}

internal sealed record PrescriptiveTemplateAssemblyNode(
    string TemplatePath,
    int OccurrenceIndex,
    string Identity,
    RemediationSemanticNode? Source,
    ClaimId? ClaimId,
    bool OpaqueInterior,
    IReadOnlyList<PrescriptiveTemplateAssemblyNode> Children)
{
    /// <summary>Direct compiled-program template node; null for the legacy assembly path.</summary>
    internal RemediationTemplateNode? ProgramTemplate { get; init; }

    /// <summary>Canonical compiled-program slot; null for the Document root and legacy nodes.</summary>
    internal SlotRef? SlotReference { get; init; }

    /// <summary>Declared local name used by the native identity formatter.</summary>
    internal string? LocalName { get; init; }
}
