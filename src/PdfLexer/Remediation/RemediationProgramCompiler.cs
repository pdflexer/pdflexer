namespace PdfLexer.Remediation;

/// <summary>Compiled immutable execution plan for a prescriptive remediation program.</summary>
public sealed class CompiledRemediationProgram
{
    internal CompiledRemediationProgram(
        RemediationProgram program,
        IReadOnlyDictionary<SlotRef, CompiledTemplateSlot> slots,
        IReadOnlyList<IReadOnlyList<CompiledBinding>> layers,
        IReadOnlyList<RemediationProgramDiagnostic> diagnostics)
    {
        Program = program;
        Slots = new System.Collections.ObjectModel.ReadOnlyDictionary<SlotRef, CompiledTemplateSlot>(
            new Dictionary<SlotRef, CompiledTemplateSlot>(slots));
        Layers = Array.AsReadOnly(layers.Select(x =>
            (IReadOnlyList<CompiledBinding>)Array.AsReadOnly(x.ToArray())).ToArray());
        Diagnostics = Array.AsReadOnly(diagnostics
            .OrderBy(x => x.Code, StringComparer.Ordinal)
            .ThenBy(x => x.Scope, StringComparer.Ordinal)
            .ThenBy(x => x.Slot?.Path, StringComparer.Ordinal)
            .ThenBy(x => x.BindingId, StringComparer.Ordinal)
            .ThenBy(x => x.Message, StringComparer.Ordinal)
            .ToArray());
        Errors = Array.AsReadOnly(Diagnostics
            .Where(x => x.Severity == RemediationProgramDiagnosticSeverity.Error)
            .Select(x => x.Message)
            .ToArray());
    }

    public RemediationProgram Program { get; }
    public IReadOnlyDictionary<SlotRef, CompiledTemplateSlot> Slots { get; }
    public IReadOnlyList<IReadOnlyList<CompiledBinding>> Layers { get; }
    public IReadOnlyList<string> Errors { get; }
    public IReadOnlyList<RemediationProgramDiagnostic> Diagnostics { get; }
    public bool IsValid => Errors.Count == 0;
}

public enum RemediationProgramDiagnosticSeverity
{
    Info,
    Warning,
    Error
}

/// <summary>Structured declaration diagnostic emitted by the prescriptive compiler.</summary>
public sealed record RemediationProgramDiagnostic(
    string Code,
    RemediationProgramDiagnosticSeverity Severity,
    string Scope,
    SlotRef? Slot,
    string? BindingId,
    string? CandidateId,
    string Message,
    IReadOnlyDictionary<string, object?>? Evidence = null);

public sealed record CompiledTemplateSlot(
    SlotRef Reference,
    RemediationTemplateNode Node,
    SlotRef? Parent,
    int Depth);

/// <summary>An immutable binding instance resolved for native program execution.</summary>
public sealed record CompiledBinding(
    string Id,
    string DefinitionId,
    BindingRule Definition,
    int DependencyLayer,
    string DeclarationScope,
    IReadOnlyList<SlotRef> RepeatedAncestors)
{
    public BindingTarget Target => Definition.Target;
    public CandidateSelector Candidates => Definition.Candidates;
    public RemediationPredicate Predicate => Definition.Predicate;
    public PageSelector Pages => Definition.Pages;
    public double? MinConfidence => Definition.MinConfidence;
    public BindingCardinality? Cardinality => Definition.Cardinality;
}

/// <summary>Compiles template and binding declarations before document parsing.</summary>
public static partial class RemediationProgramCompiler
{
    private const string InvalidTemplate = "Program.InvalidTemplate";
    private const string DuplicateId = "Program.DuplicateId";
    private const string InvalidReference = "Program.InvalidReference";
    private const string UnsupportedCapability = "Program.UnsupportedCapability";
    private const string InvalidValue = "Program.InvalidValue";
    private const string DependencyCycle = "Program.DependencyCycle";

    public static CompiledRemediationProgram Compile(RemediationProgram program)
    {
        ArgumentNullException.ThrowIfNull(program);
        var diagnostics = new List<RemediationProgramDiagnostic>();
        program = ExpandFragments(program, diagnostics);
        var slots = new Dictionary<SlotRef, CompiledTemplateSlot>();
        var boundaryIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var boundary in program.Boundaries)
        {
            if (!boundaryIds.Add(boundary.Id))
                Add(diagnostics, DuplicateId, $"boundary:{boundary.Id}",
                    $"Boundary id '{boundary.Id}' is duplicated.");
        }
        ValidateTemplate(program.Template, slots, diagnostics, boundaryIds);
        foreach (var descriptor in slots.Values.Where(x =>
                     x.Node.Children.Count > 0 &&
                     x.Node.Occurrence is TemplateOccurrence.ZeroOrMore or TemplateOccurrence.OneOrMore &&
                     x.Node.OccurrenceBoundary is OccurrenceBoundary.StartsOnSlot))
        {
            var reference = ((OccurrenceBoundary.StartsOnSlot)descriptor.Node.OccurrenceBoundary!).Reference;
            var resolved = reference.IsAbsolute
                ? reference
                : SlotRef.Absolute($"{descriptor.Reference.Path}/{reference.Path[2..]}");
            if (!slots.ContainsKey(resolved))
                Add(diagnostics, InvalidReference, $"template:{descriptor.Reference.Path}",
                    $"Occurrence boundary for '{descriptor.Reference}' references unknown opener slot '{resolved}'.",
                    resolved);
        }

        if (!Enum.IsDefined(program.Template.Profile))
            Add(diagnostics, InvalidValue, "template", $"Template profile '{program.Template.Profile}' is not supported.");

        var artifactIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var artifact in program.Artifacts)
        {
            if (!artifactIds.Add(artifact.Id))
                Add(diagnostics, DuplicateId, $"artifact:{artifact.Id}", $"Artifact id '{artifact.Id}' is duplicated.");
            foreach (var error in artifact.Validate())
                Add(diagnostics, InvalidValue, $"artifact:{artifact.Id}", error);
        }

        foreach (var boundary in program.Boundaries)
        {
            ValidateSelector(boundary.Candidates, $"boundary:{boundary.Id}:candidates", diagnostics);
            ValidatePredicate(boundary.Predicate, $"boundary:{boundary.Id}:predicate", boundary.Id, diagnostics);
        }

        var bindingIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var binding in program.Bindings)
        {
            if (!bindingIds.Add(binding.Id))
                Add(diagnostics, DuplicateId, $"binding:{binding.Id}", $"Binding id '{binding.Id}' is duplicated.", bindingId: binding.Id);

            switch (binding.Target)
            {
                case BindingTarget.Slot slot when slot.Reference.IsRelative:
                    Add(diagnostics, UnsupportedCapability, $"binding:{binding.Id}",
                        $"Binding '{binding.Id}' uses reserved relative slot reference '{slot.Reference}'; fragment mounts are not supported.",
                        slot.Reference, binding.Id);
                    break;
                case BindingTarget.Slot slot when !slots.TryGetValue(slot.Reference, out _):
                    Add(diagnostics, InvalidReference, $"binding:{binding.Id}",
                        $"Binding '{binding.Id}' references unknown slot '{slot.Reference}'.", slot.Reference, binding.Id);
                    break;
                case BindingTarget.Slot slot when slots[slot.Reference].Node.Children.Count > 0:
                    Add(diagnostics, UnsupportedCapability, $"binding:{binding.Id}",
                        $"Binding '{binding.Id}' targets composite slot '{slot.Reference}'; bind a leaf slot.", slot.Reference, binding.Id);
                    break;
                case BindingTarget.Artifact artifact when !artifactIds.Contains(artifact.Id):
                    Add(diagnostics, InvalidReference, $"binding:{binding.Id}",
                        $"Binding '{binding.Id}' references undeclared artifact '{artifact.Id}'.", bindingId: binding.Id);
                    break;
            }

            ValidateSelector(binding, diagnostics);
            ValidatePredicate(binding.Predicate, $"binding:{binding.Id}:predicate", binding.Id, diagnostics);
        }

        var anchorIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var anchor in program.Anchors)
        {
            if (string.IsNullOrWhiteSpace(anchor.Id))
                Add(diagnostics, InvalidValue, "anchor", "Anchor id is required.");
            else if (!anchorIds.Add(anchor.Id))
                Add(diagnostics, DuplicateId, $"anchor:{anchor.Id}", $"Anchor id '{anchor.Id}' is duplicated.");

            if (anchor.Style != null || anchor.NeighborText != null ||
                anchor.NeighborTolerance != RemediationAnchor.DefaultNeighborTolerance)
                Add(diagnostics, UnsupportedCapability, $"anchor:{anchor.Id}",
                    $"Anchor '{anchor.Id}' uses preview-unsupported style or neighbor disambiguation.");

            switch (anchor)
            {
                case SlotAnchor slotAnchor when slotAnchor.Slot.IsRelative:
                    Add(diagnostics, UnsupportedCapability, $"anchor:{anchor.Id}",
                        $"Slot anchor '{anchor.Id}' uses reserved relative slot reference '{slotAnchor.Slot}'.", slotAnchor.Slot);
                    break;
                case SlotAnchor slotAnchor when !slots.TryGetValue(slotAnchor.Slot, out _):
                    Add(diagnostics, InvalidReference, $"anchor:{anchor.Id}",
                        $"Slot anchor '{anchor.Id}' references unknown slot '{slotAnchor.Slot}'.", slotAnchor.Slot);
                    break;
                case SlotAnchor slotAnchor when slotAnchor.OccurrenceSelector == OccurrenceSelector.All:
                    Add(diagnostics, UnsupportedCapability, $"anchor:{anchor.Id}",
                        $"Slot anchor '{anchor.Id}' cannot use the aggregate occurrence selector 'All'.", slotAnchor.Slot);
                    break;
                case SlotAnchor slotAnchor when !program.Bindings.Any(x =>
                    x.Target is BindingTarget.Slot target && target.Reference == slotAnchor.Slot):
                    Add(diagnostics, InvalidReference, $"anchor:{anchor.Id}",
                        $"Slot anchor '{anchor.Id}' references slot '{slotAnchor.Slot}', which has no producer.", slotAnchor.Slot);
                    break;
                case TextLabelAnchor:
                case SlotAnchor:
                    break;
                default:
                    Add(diagnostics, UnsupportedCapability, $"anchor:{anchor.Id}",
                        $"Anchor '{anchor.Id}' uses unsupported preview anchor type '{anchor.GetType().Name}'.");
                    break;
            }
        }

        ValidateRegions(program, anchorIds, artifactIds, diagnostics);

        var assertionIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var assertion in program.Assertions)
        {
            if (!assertionIds.Add(assertion.Id))
                Add(diagnostics, DuplicateId, $"assertion:{assertion.Id}", $"Assertion id '{assertion.Id}' is duplicated.");
            if (assertion.Expected.Min < 0 || assertion.Expected.Max is { } max && max < assertion.Expected.Min)
                Add(diagnostics, InvalidValue, $"assertion:{assertion.Id}", $"Assertion '{assertion.Id}' has an invalid expected count range.");
            if (assertion.Slot.IsRelative)
                Add(diagnostics, UnsupportedCapability, $"assertion:{assertion.Id}",
                    $"Assertion '{assertion.Id}' uses reserved relative slot reference '{assertion.Slot}'.", assertion.Slot);
            else if (!slots.ContainsKey(assertion.Slot))
                Add(diagnostics, InvalidReference, $"assertion:{assertion.Id}",
                    $"Assertion '{assertion.Id}' references unknown slot '{assertion.Slot}'.", assertion.Slot);
        }

        var graph = BuildDependencyGraph(program, slots, anchorIds, diagnostics);
        var rawLayers = TopologicalLayers(program.Bindings, graph, diagnostics);
        var repeated = slots.Values
            .Where(x => x.Node.Occurrence is TemplateOccurrence.ZeroOrMore or TemplateOccurrence.OneOrMore)
            .Select(x => x.Reference)
            .OrderBy(x => x.Path, StringComparer.Ordinal)
            .ToArray();
        var layers = rawLayers.Select((layer, layerIndex) =>
            (IReadOnlyList<CompiledBinding>)layer.Select(binding =>
            {
                var target = binding.Target as BindingTarget.Slot;
                var ancestors = target == null
                    ? Array.Empty<SlotRef>()
                    : repeated.Where(x => target.Reference.Path.StartsWith(
                        x.Path + "/", StringComparison.Ordinal)).ToArray();
                var separator = binding.Id.LastIndexOf("::", StringComparison.Ordinal);
                var scope = separator < 0 ? "/" : binding.Id[..separator];
                var definitionId = separator < 0 ? binding.Id : binding.Id[(separator + 2)..];
                return new CompiledBinding(
                    binding.Id, definitionId, binding, layerIndex, scope, ancestors);
            }).ToArray()).ToArray();
        return new CompiledRemediationProgram(program, slots, layers, diagnostics);
    }

    private static RemediationProgram ExpandFragments(
        RemediationProgram source,
        List<RemediationProgramDiagnostic> diagnostics)
    {
        if (source.Fragments.Count == 0 &&
            !ContainsMount(source.Template.Document))
        {
            return source;
        }

        var fragments = new Dictionary<string, RemediationFragment>(StringComparer.Ordinal);
        foreach (var fragment in source.Fragments)
        {
            if (!fragments.TryAdd(fragment.Id, fragment))
            {
                Add(diagnostics, DuplicateId, $"fragment:{fragment.Id}",
                    $"Fragment id '{fragment.Id}' is duplicated.");
            }
            if (fragment.Root.Name != null ||
                fragment.Root.Occurrence != TemplateOccurrence.ExactlyOne)
            {
                Add(diagnostics, InvalidTemplate, $"fragment:{fragment.Id}",
                    $"Fragment '{fragment.Id}' root must be unnamed and exactly one.");
            }
        }

        var bindings = source.Bindings.ToList();
        var anchors = source.Anchors.ToList();
        var artifacts = source.Artifacts.ToList();
        var assertions = source.Assertions.ToList();
        var boundaries = source.Boundaries.ToList();
        var regions = source.Regions.ToList();
        var regionAccounting = source.RegionAccounting.ToList();

        RemediationTemplateNode ExpandNode(
            RemediationTemplateNode node,
            string path,
            IReadOnlyList<string> stack,
            string? declarationScope = null)
        {
            var expanded = new List<RemediationTemplateNode>();
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var particle in node.Particles)
            {
                switch (particle)
                {
                    case RemediationTemplateNode child:
                    {
                        if (child.Name != null && !names.Add(child.Name))
                            Add(diagnostics, DuplicateId, $"template:{path}",
                                $"Template sibling name '{child.Name}' is duplicated under '{path}'.");
                        var childPath = AppendPath(path, child.Name ?? "invalid");
                        expanded.Add(ExpandNode(child, childPath, stack, declarationScope));
                        break;
                    }
                    case RemediationFragmentMount mount:
                    {
                        if (!names.Add(mount.Alias))
                        {
                            Add(diagnostics, DuplicateId, $"template:{path}",
                                $"Template sibling name or fragment alias '{mount.Alias}' is duplicated under '{path}'.");
                            break;
                        }
                        if (!fragments.TryGetValue(mount.FragmentId, out var fragment))
                        {
                            Add(diagnostics, InvalidReference, $"template:{path}",
                                $"Fragment mount '{mount.Alias}' references unknown fragment '{mount.FragmentId}'.");
                            break;
                        }
                        if (stack.Contains(fragment.Id, StringComparer.Ordinal))
                        {
                            Add(diagnostics, DependencyCycle, $"fragment:{fragment.Id}",
                                $"Recursive fragment mount chain detected: {string.Join(" -> ", stack.Append(fragment.Id))}.");
                            break;
                        }

                        var mountPath = AppendPath(path, mount.Alias);
                        InstantiateDeclarations(fragment, mountPath);
                        var root = ExpandNode(
                            fragment.Root, mountPath, stack.Append(fragment.Id).ToArray(), mountPath);
                        expanded.Add(new RemediationTemplateNode(
                            root.Tag,
                            mount.Alias,
                            root.Children,
                            mount.Occurrence,
                            root.Properties,
                            root.OrderPolicy,
                            mount.OccurrenceBoundary ?? root.OccurrenceBoundary));
                        break;
                    }
                }
            }

            return new RemediationTemplateNode(
                node.Tag,
                node.Name,
                expanded,
                node.Occurrence,
                node.Properties,
                node.OrderPolicy,
                RewriteBoundary(node.OccurrenceBoundary, declarationScope));
        }

        void InstantiateDeclarations(RemediationFragment fragment, string mountPath)
        {
            string Qualify(string id) => $"{mountPath}::{id}";
            SlotRef Resolve(SlotRef slot, string scope)
            {
                if (!slot.IsRelative)
                {
                    Add(diagnostics, InvalidReference, scope,
                        $"Fragment declaration must use a relative slot reference; found '{slot}'.", slot);
                    return slot;
                }
                var suffix = slot.Path[2..];
                return SlotRef.Absolute(AppendPath(mountPath, suffix));
            }

            foreach (var artifact in fragment.Artifacts)
            {
                artifacts.Add(new ArtifactDeclaration(
                    Qualify(artifact.Id), artifact.Subtype, artifact.Pages, artifact.Occurrence,
                    artifact.SemanticSubtype, artifact.IncludeBoundingBox, artifact.Attached));
            }
            foreach (var boundary in fragment.Boundaries)
            {
                boundaries.Add(new OccurrenceBoundaryDeclaration(
                    Qualify(boundary.Id), boundary.Candidates,
                    RewritePredicate(boundary.Predicate, Qualify), boundary.Pages));
            }
            foreach (var region in fragment.Regions)
            {
                regions.Add(new RegionDeclaration(
                    Qualify(region.Id),
                    RewriteRegionExpression(region.Expression, Qualify),
                    region.Pages));
            }
            foreach (var accounting in fragment.RegionAccounting)
            {
                regionAccounting.Add(new RegionArtifactAccounting(
                    Qualify(accounting.Id), Qualify(accounting.RegionId), Qualify(accounting.ArtifactId),
                    accounting.CandidateKinds, accounting.AllowText,
                    accounting.TextPredicate == null ? null : RewritePredicate(accounting.TextPredicate, Qualify),
                    accounting.Priority));
            }
            foreach (var anchor in fragment.Anchors)
            {
                switch (anchor)
                {
                    case SlotAnchor slot:
                        anchors.Add(new SlotAnchor(
                            Qualify(slot.Id),
                            new SlotOccurrenceRef(
                                Resolve(slot.Slot, $"fragment:{fragment.Id}:anchor:{slot.Id}"),
                                slot.OccurrenceSelector,
                                slot.OccurrenceNumber))
                        {
                            Pages = slot.Pages
                        });
                        break;
                    case TextLabelAnchor text:
                        anchors.Add(text with { Id = Qualify(text.Id) });
                        break;
                    default:
                        Add(diagnostics, UnsupportedCapability, $"fragment:{fragment.Id}:anchor:{anchor.Id}",
                            $"Fragment anchor '{anchor.Id}' uses unsupported preview type '{anchor.GetType().Name}'.");
                        break;
                }
            }
            foreach (var binding in fragment.Bindings)
            {
                BindingTarget target = binding.Target switch
                {
                    BindingTarget.Slot slot => BindingTarget.ToSlot(
                        Resolve(slot.Reference, $"fragment:{fragment.Id}:binding:{binding.Id}")),
                    BindingTarget.Artifact artifact => BindingTarget.ToArtifact(Qualify(artifact.Id)),
                    _ => binding.Target
                };
                bindings.Add(new BindingRule(
                    Qualify(binding.Id), target, binding.Candidates,
                    RewritePredicate(binding.Predicate, Qualify), binding.Pages,
                    binding.MinConfidence, binding.Cardinality));
            }
            foreach (var assertion in fragment.Assertions)
            {
                assertions.Add(new SlotCountAssertion(
                    Qualify(assertion.Id),
                    Resolve(assertion.Slot, $"fragment:{fragment.Id}:assertion:{assertion.Id}"),
                    assertion.Expected, assertion.Scope, assertion.Pages));
            }
        }

        var document = ExpandNode(source.Template.Document, "/", Array.Empty<string>());
        return new RemediationProgram(
            source.Id,
            new RemediationTemplate(
                source.Template.Id, source.Template.Version, source.Template.Profile, document),
            bindings,
            anchors,
            artifacts,
            assertions,
            source.TextNormalization,
            boundaries,
            null,
            regions,
            regionAccounting);

        OccurrenceBoundary? RewriteBoundary(OccurrenceBoundary? boundary, string? declarationScope) =>
            boundary switch
            {
                OccurrenceBoundary.StartsOnBoundary named when declarationScope != null =>
                    new OccurrenceBoundary.StartsOnBoundary($"{declarationScope}::{named.Id}"),
                _ => boundary
            };

        static bool ContainsMount(RemediationTemplateNode node) =>
            node.Particles.Any(x => x is RemediationFragmentMount ||
                x is RemediationTemplateNode child && ContainsMount(child));

        static string AppendPath(string parent, string segment) =>
            parent == "/" ? $"/{segment}" : $"{parent}/{segment}";
    }

    private static RemediationPredicate RewritePredicate(
        RemediationPredicate predicate,
        Func<string, string> qualifyAnchor) =>
        predicate switch
        {
            AnchorRelativeRemediationPredicate anchor => anchor with
            {
                AnchorId = qualifyAnchor(anchor.AnchorId),
                AnchorId2 = anchor.AnchorId2 == null ? null : qualifyAnchor(anchor.AnchorId2)
            },
            GeometryRemediationPredicate geometry => geometry with
            {
                Coord = geometry.Coord switch
                {
                    NamedAnchorLayoutCoord named => named with
                    {
                        AnchorId = qualifyAnchor(named.AnchorId)
                    },
                    BetweenAnchorsLayoutCoord between => between with
                    {
                        AnchorA = qualifyAnchor(between.AnchorA),
                        AnchorB = qualifyAnchor(between.AnchorB)
                    },
                    _ => geometry.Coord
                }
            },
            RegionRemediationPredicate region => region with { RegionId = qualifyAnchor(region.RegionId) },
            CompositeRemediationPredicate composite => composite with
            {
                Left = RewritePredicate(composite.Left, qualifyAnchor),
                Right = RewritePredicate(composite.Right, qualifyAnchor)
            },
            NotRemediationPredicate not => not with
            {
                Inner = RewritePredicate(not.Inner, qualifyAnchor)
            },
            _ => predicate
        };

    private static void ValidateTemplate(
        RemediationTemplate template,
        Dictionary<SlotRef, CompiledTemplateSlot> slots,
        List<RemediationProgramDiagnostic> diagnostics,
        IReadOnlySet<string> boundaryIds)
    {
        if (template.Document.Tag != "Document") Add(diagnostics, InvalidTemplate, "template:/", "Template root must use the Document tag.");
        if (template.Document.Name != null) Add(diagnostics, InvalidTemplate, "template:/", "Template Document root cannot have a slot name.");
        if (template.Document.Occurrence != TemplateOccurrence.ExactlyOne)
            Add(diagnostics, InvalidTemplate, "template:/", "Template Document root must be exactly one.");

        void Visit(RemediationTemplateNode node, SlotRef? parent, string path, int depth)
        {
            SlotRef? reference = parent == null ? null : SlotRef.Absolute(path);
            if (!RemediationStructuralTemplateValidator.IsStandardTag(node.Tag))
                Add(diagnostics, InvalidTemplate, $"template:{path}", $"Template slot '{path}' uses non-standard tag '{node.Tag}'.", reference);
            if (parent != null && node.Name == null)
            {
                Add(diagnostics, InvalidTemplate, $"template:{path}", $"Template node '{path}' requires a local slot name.");
                return;
            }
            if (node.Children.Count == 0 && node.OrderPolicy != TemplateOrderPolicy.RequireSourceAgreement)
                Add(diagnostics, InvalidTemplate, $"template:{path}", $"Leaf slot '{path}' cannot declare a child order policy.", reference);
            if (node.Children.Count > 0 && node.Occurrence is TemplateOccurrence.ZeroOrMore or TemplateOccurrence.OneOrMore)
            {
                var boundary = node.OccurrenceBoundary ?? new OccurrenceBoundary.Derived();
                switch (boundary)
                {
                    case OccurrenceBoundary.Derived when node.Children[0].Occurrence != TemplateOccurrence.ExactlyOne:
                        Add(diagnostics, UnsupportedCapability, $"template:{path}",
                            $"Repeating composite slot '{path}' cannot derive an occurrence boundary: its first child must be required and singular. Declare startsOn.", reference);
                        break;
                    case OccurrenceBoundary.StartsOnSlot startsOn:
                    {
                        var resolved = ResolveBoundarySlot(startsOn.Reference, path);
                        var descendantPrefix = path == "/" ? "/" : path + "/";
                        if (resolved == null || !resolved.Path.StartsWith(descendantPrefix, StringComparison.Ordinal))
                        {
                            Add(diagnostics, InvalidReference, $"template:{path}",
                                $"Occurrence boundary for '{path}' must reference a descendant slot.", reference);
                        }
                        break;
                    }
                    case OccurrenceBoundary.StartsOnBoundary startsOnBoundary
                        when !boundaryIds.Contains(startsOnBoundary.Id):
                        Add(diagnostics, InvalidReference, $"template:{path}",
                            $"Occurrence boundary '{startsOnBoundary.Id}' for '{path}' is not declared.", reference);
                        break;
                }
            }
            if (node.Properties.Language is { } language && (string.IsNullOrWhiteSpace(language) || language.Length > 128))
                Add(diagnostics, InvalidValue, $"template:{path}", $"Template slot '{path}' has an invalid language value.", reference);
            if (node.Properties.AlternateText is { } alt && string.IsNullOrWhiteSpace(alt))
                Add(diagnostics, InvalidValue, $"template:{path}", $"Template slot '{path}' has an empty alternate-text property.", reference);
            if (node.Properties.ActualText is { } actual && string.IsNullOrWhiteSpace(actual))
                Add(diagnostics, InvalidValue, $"template:{path}", $"Template slot '{path}' has an empty actual-text property.", reference);
            if (node.Properties.Expansion is { } expansion && string.IsNullOrWhiteSpace(expansion))
                Add(diagnostics, InvalidValue, $"template:{path}", $"Template slot '{path}' has an empty expansion property.", reference);
            if (node.Tag is "Figure" or "Formula" &&
                string.IsNullOrWhiteSpace(node.Properties.AlternateText) &&
                string.IsNullOrWhiteSpace(node.Properties.ActualText))
                Add(diagnostics, InvalidTemplate, $"template:{path}",
                    $"Template slot '{path}' uses '{node.Tag}' and requires alternate text or actual text in the preview model.", reference);

            if (parent != null)
            {
                if (!slots.TryAdd(reference!, new CompiledTemplateSlot(reference!, node, parent, depth)))
                    Add(diagnostics, DuplicateId, $"template:{path}", $"Template slot path '{reference}' is duplicated.", reference);
                var parentTag = parent.IsRoot ? template.Document.Tag : slots[parent].Node.Tag;
                if (!RemediationStructuralTemplateValidator.LegalChild(parentTag, node.Tag))
                    Add(diagnostics, InvalidTemplate, $"template:{path}",
                        $"Template nesting at '{path}' is not legal: '{parentTag}/{node.Tag}'.", reference);
            }

            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var child in node.Children)
            {
                if (child.Name != null && !names.Add(child.Name))
                    Add(diagnostics, DuplicateId, $"template:{path}",
                        $"Template sibling name '{child.Name}' is duplicated under '{path}'.", reference);
                var childPath = path == "/" ? $"/{child.Name}" : $"{path}/{child.Name}";
                Visit(child, SlotRef.Absolute(path), childPath, depth + 1);
            }
        }

        Visit(template.Document, null, "/", 0);

        static SlotRef? ResolveBoundarySlot(SlotRef reference, string parentPath)
        {
            if (reference.IsAbsolute) return reference;
            if (!reference.IsRelative) return null;
            var suffix = reference.Path[2..];
            var path = parentPath == "/" ? $"/{suffix}" : $"{parentPath}/{suffix}";
            return SlotRef.TryParse(path, out var resolved) ? resolved : null;
        }
    }

    private static void ValidateSelector(BindingRule binding, List<RemediationProgramDiagnostic> diagnostics)
    {
        switch (binding.Candidates)
        {
            case CandidateSelector.TextSelector text when !Enum.IsDefined(text.Granularity):
                Add(diagnostics, InvalidValue, $"binding:{binding.Id}:candidates",
                    $"Binding '{binding.Id}' uses unsupported text granularity '{text.Granularity}'.", bindingId: binding.Id);
                break;
            case CandidateSelector.TextSelector:
                break;
            case CandidateSelector.ContentSelector content when content.Kinds.Count == 0 ||
                content.Kinds.Any(x => !Enum.IsDefined(x) || x is RemediationCandidateKind.Text or RemediationCandidateKind.Annotation):
                Add(diagnostics, InvalidValue, $"binding:{binding.Id}:candidates",
                    $"Binding '{binding.Id}' has invalid non-text candidate kinds.", bindingId: binding.Id);
                break;
            case CandidateSelector.ContentSelector:
                break;
            default:
                Add(diagnostics, UnsupportedCapability, $"binding:{binding.Id}:candidates",
                    $"Binding '{binding.Id}' uses unsupported preview candidate selector '{binding.Candidates.GetType().Name}'.", bindingId: binding.Id);
                break;
        }
    }

    private static void ValidateSelector(
        CandidateSelector selector,
        string scope,
        List<RemediationProgramDiagnostic> diagnostics)
    {
        switch (selector)
        {
            case CandidateSelector.TextSelector text when !Enum.IsDefined(text.Granularity):
                Add(diagnostics, InvalidValue, scope,
                    $"Selector uses unsupported text granularity '{text.Granularity}'.");
                break;
            case CandidateSelector.TextSelector:
                break;
            case CandidateSelector.ContentSelector content when content.Kinds.Count == 0 ||
                content.Kinds.Any(x => !Enum.IsDefined(x) || x is RemediationCandidateKind.Text or RemediationCandidateKind.Annotation):
                Add(diagnostics, InvalidValue, scope,
                    "Selector has invalid non-text candidate kinds.");
                break;
            case CandidateSelector.ContentSelector:
                break;
            default:
                Add(diagnostics, UnsupportedCapability, scope,
                    $"Selector uses unsupported preview candidate type '{selector.GetType().Name}'.");
                break;
        }
    }

    private static void ValidatePredicate(
        RemediationPredicate predicate,
        string scope,
        string bindingId,
        List<RemediationProgramDiagnostic> diagnostics)
    {
        switch (predicate)
        {
            case ConstantRemediationPredicate:
            case TextRemediationPredicate:
            case ContentRemediationPredicate:
            case FontRemediationPredicate:
            case AnchorRelativeRemediationPredicate:
                return;
            case RegionRemediationPredicate:
                return;
            case GeometryRemediationPredicate geometry:
                ValidateLayoutCoord(geometry.Coord, scope, bindingId, diagnostics);
                return;
            case CompositeRemediationPredicate composite:
                ValidatePredicate(composite.Left, scope, bindingId, diagnostics);
                ValidatePredicate(composite.Right, scope, bindingId, diagnostics);
                return;
            case NotRemediationPredicate not:
                ValidatePredicate(not.Inner, scope, bindingId, diagnostics);
                return;
            default:
                Add(diagnostics, UnsupportedCapability, scope,
                    $"Binding '{bindingId}' uses unsupported preview predicate '{predicate.GetType().Name}'.", bindingId: bindingId);
                return;
        }
    }

    private static void ValidateLayoutCoord(
        LayoutCoord coord,
        string scope,
        string bindingId,
        List<RemediationProgramDiagnostic> diagnostics)
    {
        if (coord is AbsoluteLayoutCoord or MarginRelativeLayoutCoord or NamedZoneLayoutCoord or
            PercentageLayoutCoord or NamedAnchorLayoutCoord or BetweenAnchorsLayoutCoord) return;
        Add(diagnostics, UnsupportedCapability, scope,
            $"Binding '{bindingId}' uses unsupported preview layout coordinate '{coord.GetType().Name}'.", bindingId: bindingId);
    }

    private static Dictionary<string, HashSet<string>> BuildDependencyGraph(
        RemediationProgram program,
        IReadOnlyDictionary<SlotRef, CompiledTemplateSlot> slots,
        IReadOnlySet<string> knownAnchors,
        List<RemediationProgramDiagnostic> diagnostics)
    {
        var graph = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
        foreach (var binding in program.Bindings) graph.TryAdd(binding.Id, new HashSet<string>(StringComparer.Ordinal));
        var anchors = program.Anchors.GroupBy(x => x.Id, StringComparer.Ordinal)
            .ToDictionary(x => x.Key, x => x.First(), StringComparer.Ordinal);
        var producers = program.Bindings
            .Select(x => (Binding: x, Slot: x.Target as BindingTarget.Slot))
            .Where(x => x.Slot != null)
            .GroupBy(x => x.Slot!.Reference)
            .ToDictionary(x => x.Key, x => x.Select(y => y.Binding).ToArray());

        foreach (var binding in program.Bindings)
        {
            foreach (var anchorId in EnumerateAnchorReferences(binding.Predicate)
                         .Concat(EnumerateRegionAnchorReferences(binding.Predicate, program.Regions))
                         .Distinct(StringComparer.Ordinal))
            {
                if (!knownAnchors.Contains(anchorId) || !anchors.TryGetValue(anchorId, out var anchor))
                {
                    Add(diagnostics, InvalidReference, $"binding:{binding.Id}:predicate",
                        $"Binding '{binding.Id}' references unknown anchor '{anchorId}'.", bindingId: binding.Id);
                    continue;
                }
                if (anchor is not SlotAnchor slotAnchor) continue;
                if (!producers.TryGetValue(slotAnchor.Slot, out var dependencies)) continue;
                foreach (var producer in dependencies)
                {
                    if (producer.Id == binding.Id)
                        Add(diagnostics, DependencyCycle, $"binding:{binding.Id}:predicate",
                            $"Binding '{binding.Id}' has a self-dependency through slot anchor '{slotAnchor.Id}'.",
                            slotAnchor.Slot, binding.Id);
                    graph[binding.Id].Add(producer.Id);
                }

                if (slotAnchor.OccurrenceSelector != OccurrenceSelector.SameOccurrence) continue;
                if (binding.Target is not BindingTarget.Slot consumer ||
                    !slots.TryGetValue(consumer.Reference, out _))
                {
                    Add(diagnostics, InvalidReference, $"binding:{binding.Id}:predicate",
                        $"Binding '{binding.Id}' cannot use SameOccurrence without a structural slot target.",
                        slotAnchor.Slot, binding.Id);
                    continue;
                }
                var common = slots.Values
                    .Where(x => x.Node.Children.Count > 0 &&
                        x.Node.Occurrence is TemplateOccurrence.ZeroOrMore or TemplateOccurrence.OneOrMore &&
                        consumer.Reference.Path.StartsWith(x.Reference.Path + "/", StringComparison.Ordinal) &&
                        slotAnchor.Slot.Path.StartsWith(x.Reference.Path + "/", StringComparison.Ordinal))
                    .OrderByDescending(x => x.Depth)
                    .FirstOrDefault();
                if (common == null)
                {
                    Add(diagnostics, InvalidReference, $"binding:{binding.Id}:predicate",
                        $"Binding '{binding.Id}' and slot '{slotAnchor.Slot}' have no common repeated occurrence scope.",
                        slotAnchor.Slot, binding.Id);
                    continue;
                }

                var boundary = common.Node.OccurrenceBoundary ?? new OccurrenceBoundary.Derived();
                SlotRef? opener = boundary switch
                {
                    OccurrenceBoundary.Derived => SlotRef.Absolute(
                        $"{common.Reference.Path}/{common.Node.Children[0].Name}"),
                    OccurrenceBoundary.StartsOnSlot starts when starts.Reference.IsAbsolute =>
                        starts.Reference,
                    OccurrenceBoundary.StartsOnSlot starts => SlotRef.Absolute(
                        $"{common.Reference.Path}/{starts.Reference.Path[2..]}"),
                    _ => null
                };
                if (opener != null && producers.TryGetValue(opener, out var openerProducers))
                {
                    foreach (var producer in openerProducers.Where(x => x.Id != binding.Id))
                        graph[binding.Id].Add(producer.Id);
                }
                if (boundary is OccurrenceBoundary.StartsOnBoundary named &&
                    program.Boundaries.FirstOrDefault(x => x.Id == named.Id) is { } declaration)
                {
                    foreach (var dependencyAnchor in EnumerateAnchorReferences(declaration.Predicate))
                    {
                        if (!anchors.TryGetValue(dependencyAnchor, out var dependency) ||
                            dependency is not SlotAnchor dependencySlot ||
                            !producers.TryGetValue(dependencySlot.Slot, out var boundaryProducers))
                            continue;
                        foreach (var producer in boundaryProducers.Where(x => x.Id != binding.Id))
                            graph[binding.Id].Add(producer.Id);
                    }
                }
            }
        }
        return graph;
    }

    private static IReadOnlyList<IReadOnlyList<BindingRule>> TopologicalLayers(
        IReadOnlyList<BindingRule> bindings,
        IReadOnlyDictionary<string, HashSet<string>> graph,
        List<RemediationProgramDiagnostic> diagnostics)
    {
        var remaining = graph.ToDictionary(x => x.Key, x => x.Value.ToHashSet(), StringComparer.Ordinal);
        var byId = bindings.GroupBy(x => x.Id, StringComparer.Ordinal)
            .ToDictionary(x => x.Key, x => x.First(), StringComparer.Ordinal);
        var layers = new List<IReadOnlyList<BindingRule>>();
        while (remaining.Count > 0)
        {
            var ready = remaining.Where(x => x.Value.Count == 0).Select(x => x.Key)
                .OrderBy(x => x, StringComparer.Ordinal).ToArray();
            if (ready.Length == 0)
            {
                Add(diagnostics, DependencyCycle, "program:dependencies",
                    $"Binding dependency cycle detected: {FindCycle(remaining)}.");
                break;
            }
            layers.Add(ready.Select(x => byId[x]).ToArray());
            foreach (var id in ready) remaining.Remove(id);
            foreach (var dependencies in remaining.Values)
                foreach (var id in ready) dependencies.Remove(id);
        }
        return layers;
    }

    private static string FindCycle(IReadOnlyDictionary<string, HashSet<string>> graph)
    {
        var active = new HashSet<string>(StringComparer.Ordinal);
        var done = new HashSet<string>(StringComparer.Ordinal);
        var path = new List<string>();
        string? Visit(string id)
        {
            if (active.Contains(id)) return string.Join(" -> ", path.Skip(path.IndexOf(id)).Append(id));
            if (!done.Add(id)) return null;
            active.Add(id);
            path.Add(id);
            foreach (var dependency in graph[id].OrderBy(x => x, StringComparer.Ordinal))
            {
                var found = Visit(dependency);
                if (found != null) return found;
            }
            path.RemoveAt(path.Count - 1);
            active.Remove(id);
            return null;
        }
        foreach (var id in graph.Keys.OrderBy(x => x, StringComparer.Ordinal))
        {
            var found = Visit(id);
            if (found != null) return found;
        }
        return string.Join(" -> ", graph.Keys.OrderBy(x => x, StringComparer.Ordinal));
    }

    internal static IEnumerable<string> EnumerateAnchorReferences(RemediationPredicate predicate)
    {
        switch (predicate)
        {
            case AnchorRelativeRemediationPredicate anchor:
                yield return anchor.AnchorId;
                if (anchor.AnchorId2 != null) yield return anchor.AnchorId2;
                break;
            case GeometryRemediationPredicate { Coord: NamedAnchorLayoutCoord named }:
                yield return named.AnchorId;
                break;
            case GeometryRemediationPredicate { Coord: BetweenAnchorsLayoutCoord between }:
                yield return between.AnchorA;
                yield return between.AnchorB;
                break;
            case CompositeRemediationPredicate composite:
                foreach (var id in EnumerateAnchorReferences(composite.Left)) yield return id;
                foreach (var id in EnumerateAnchorReferences(composite.Right)) yield return id;
                break;
            case NotRemediationPredicate not:
                foreach (var id in EnumerateAnchorReferences(not.Inner)) yield return id;
                break;
        }
    }

    private static void Add(
        List<RemediationProgramDiagnostic> diagnostics,
        string code,
        string scope,
        string message,
        SlotRef? slot = null,
        string? bindingId = null) =>
        diagnostics.Add(new RemediationProgramDiagnostic(
            code, RemediationProgramDiagnosticSeverity.Error, scope, slot, bindingId, null, message));
}
