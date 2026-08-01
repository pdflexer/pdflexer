using PdfLexer.Content;
using PdfLexer.Content.Model;
using PdfLexer.DOM;
using PdfLexer.Writing;

namespace PdfLexer.Remediation;

/// <summary>
/// Native execution path for compiled preview programs. This file deliberately does not construct
/// or inspect legacy Rule, RuleSet, Stage, or group-pass objects.
/// </summary>
public sealed partial class RemediationSession
{
    private static readonly HashSet<DiagnosticCode> ProgramWorkItemCodes = new()
    {
        DiagnosticCode.PrescriptiveUnaccountedContent,
        DiagnosticCode.RuleCardinalityMismatch,
        DiagnosticCode.SemanticAssertionFailed,
        DiagnosticCode.TemplateSlotUnfilled,
        DiagnosticCode.ArtifactMissingDeclared,
        DiagnosticCode.ArtifactOccurrenceViolation
    };

    private RemediationReport EvaluateProgram(bool apply)
    {
        var compiled = _program ?? throw new InvalidOperationException("No compiled remediation program is selected.");
        _programRuntimeDiagnostics = new List<RemediationRuntimeDiagnostic>();
        var diagnostics = compiled.Diagnostics
            .Where(x => x.Severity == RemediationProgramDiagnosticSeverity.Error)
            .Select(x => $"{x.Code}: {x.Message}")
            .ToList();
        foreach (var diagnostic in compiled.Diagnostics.Where(x =>
                     x.Severity == RemediationProgramDiagnosticSeverity.Error))
        {
            _programRuntimeDiagnostics.Add(new RemediationRuntimeDiagnostic(
                DiagnosticCode.ProgramConfigurationInvalid,
                RemediationDiagnosticDisposition.Error,
                $"Program:{compiled.Program.Id}:{diagnostic.Scope}",
                diagnostic.Message,
                diagnostic.Slot,
                diagnostic.BindingId,
                diagnostic.CandidateId == null ? null : new[] { diagnostic.CandidateId },
                diagnostic.Evidence));
        }
        var warnings = new List<string>();
        var orderComparisons = new List<RemediationOrderComparison>();
        var occurrencePartitions = new List<RemediationOccurrencePartition>();
        var bindingRuns = new List<ProgramBindingRun>();

        if (Configuration.DefaultConfidence is < 0 or > 1)
        {
            diagnostics.Add($"Session DefaultConfidence {Configuration.DefaultConfidence} is outside [0,1].");
            _programRuntimeDiagnostics.Add(new RemediationRuntimeDiagnostic(
                DiagnosticCode.ProgramConfigurationInvalid,
                RemediationDiagnosticDisposition.Error,
                $"Program:{compiled.Program.Id}:Session",
                $"Session DefaultConfidence {Configuration.DefaultConfidence} is outside [0,1]."));
        }

        if (diagnostics.Count > 0)
        {
            return CreateProgramReport(
                false,
                false,
                Array.Empty<RemediationClaim>(),
                Array.Empty<RemediationClaim>(),
                diagnostics,
                warnings,
                Array.Empty<RemediationBindingEvaluationSummary>(),
                orderComparisons);
        }

        var pageStates = BuildPageStates();
        var allClaims = new List<RemediationClaim>();
        var skippedClaims = new List<RemediationClaim>();
        var autoArtifacts = new List<RemediationAutoArtifactOutcome>();
        var unaccountedContent = new List<RemediationUnaccountedContent>();
        ExecuteProgramLayers(
            compiled,
            pageStates,
            allClaims,
            skippedClaims,
            diagnostics,
            bindingRuns);

        var anchors = compiled.Program.Anchors
            .ToDictionary(x => x.Id, StringComparer.Ordinal);
        var emptyZones = new Dictionary<string, TolerancedZone>(StringComparer.Ordinal);
        var emptyFlows = new Dictionary<string, FlowRegion>(StringComparer.Ordinal);
        var documentFlows = new DocumentFlowIndex(
            new Dictionary<int, Dictionary<string, FlowRegionResolution>>(),
            Array.Empty<RemediationCandidate>(),
            allClaims);

        foreach (var pageState in pageStates)
        {
            pageState.ArtifactZones = ResolveArtifactZones(
                pageState,
                allClaims,
                anchors,
                emptyZones,
                emptyFlows,
                documentFlows,
                diagnostics);
            ApplyLeftoverPolicy(
                pageState,
                pageState.TextOwnership,
                diagnostics,
                autoArtifacts,
                unaccountedContent,
                apply,
                compiled.Program.TextNormalization);
        }

        var assertions = EvaluateProgramAssertions(compiled, allClaims, diagnostics);
        occurrencePartitions.AddRange(BuildProgramOccurrencePartitions(
            compiled, pageStates, allClaims, diagnostics));
        var claimsByOccurrence = allClaims.ToDictionary(x => x.ClaimId.Value, StringComparer.Ordinal);
        foreach (var partition in occurrencePartitions.OrderBy(x =>
                     x.CompositeSlot.Path.Count(c => c == '/')))
        {
            foreach (var claimId in partition.AssignedClaims)
                if (claimsByOccurrence.TryGetValue(claimId, out var claim))
                    claim.OccurrenceIdentity = partition.OccurrenceIdentity;
        }
        var template = RemediationProgramTemplateAdapter.Create(compiled);
        var sourceTree = RemediationSemanticTree.FromClaims(compiled, allClaims);
        var claimsById = allClaims
            .GroupBy(x => x.ClaimId)
            .ToDictionary(x => x.Key, x => x.First());
        var assemblyPlan = PrescriptiveTemplateAssemblyPlan.Build(
            compiled, sourceTree, claimsById, occurrencePartitions);
        var semanticTree = assemblyPlan.ProjectSemanticTree();
        var templateDifferences = new List<RemediationTemplateDifference>();
        var boundSlots = compiled.Program.Bindings
            .OfType<BindingRule>()
            .Select(x => x.Target)
            .OfType<BindingTarget.Slot>()
            .Select(x => x.Reference.Path[1..])
            .ToHashSet(StringComparer.Ordinal);

        foreach (var difference in RemediationStructuralTemplateMatcher.Match(
                     compiled.Program.Id,
                     template,
                     semanticTree,
                     _document.Pages.Count,
                     boundSlots))
        {
            templateDifferences.Add(ReportTemplateDifference(difference, diagnostics));
        }

        EvaluateProgramSourceOrder(
            compiled,
            assemblyPlan,
            claimsById,
            diagnostics,
            orderComparisons);
        templateDifferences.AddRange(EvaluateProgramArtifactInventory(
            compiled,
            pageStates,
            allClaims,
            autoArtifacts,
            diagnostics));

        if (apply && !HasBlockingProgramDiagnostics(diagnostics))
        {
            ValidateProgramPlan(pageStates, allClaims, diagnostics);
        }

        if (apply && !HasBlockingProgramDiagnostics(diagnostics))
        {
            ApplyProgramPlan(pageStates, allClaims, diagnostics, assemblyPlan);
            var actualTree = RemediationSemanticTree.FromStructure(
                Structure.GetRoot(),
                allClaims,
                allClaims
                    .Where(x => x.ProgramSlot != null)
                    .GroupBy(x => (x.RuleSetId, x.RuleId))
                    .ToDictionary(x => x.Key, x => x.First().ProgramSlot!.Path[1..]));
            var plannedShape = SemanticShape(semanticTree);
            var actualShape = SemanticShape(actualTree);
            if (!string.Equals(plannedShape, actualShape, StringComparison.Ordinal))
            {
                var difference = new RemediationTemplateDifference(
                    RemediationTemplateDifferenceKind.MaterializationDivergence,
                    DiagnosticCode.TemplateMaterializationDivergence,
                    compiled.Program.Id,
                    null,
                    "Document",
                    "Document",
                    plannedShape,
                    actualShape,
                    Array.Empty<int>(),
                    null,
                    false);
                templateDifferences.Add(ReportTemplateDifference(difference, diagnostics));
            }

            var plannedKeys = templateDifferences.Select(TemplateDifferenceKey)
                .ToHashSet(StringComparer.Ordinal);
            foreach (var actualDifference in RemediationStructuralTemplateMatcher.Match(
                         compiled.Program.Id,
                         template,
                         actualTree,
                         _document.Pages.Count,
                         boundSlots))
            {
                if (plannedKeys.Add(TemplateDifferenceKey(actualDifference)))
                {
                    templateDifferences.Add(ReportTemplateDifference(actualDifference, diagnostics));
                }
            }

            RunDiagnostics(pageStates, diagnostics);
        }

        var bindingSummaries = BuildProgramBindingSummaries(compiled, bindingRuns, allClaims, skippedClaims);
        return CreateProgramReport(
            false,
            false,
            allClaims,
            skippedClaims,
            diagnostics,
            warnings,
            bindingSummaries,
            orderComparisons,
            autoArtifacts,
            assertions,
            semanticTree,
            unaccountedContent,
            templateDifferences,
            assemblyPlan.Items,
            occurrencePartitions);
    }

    private RemediationReport CommitProgram()
    {
        RemediationReport report = null!;
        CommitDocumentChanges(() =>
        {
            report = EvaluateProgram(apply: true);
            if (report.RuntimeDiagnostics.Any(x => x.IsBlocking))
            {
                throw new InvalidOperationException(string.Join(Environment.NewLine, report.Diagnostics));
            }

            foreach (var (page, structParentsIndex) in _pageStructParents)
            {
                page.StructParents = new PdfIntNumber(structParentsIndex);
            }

            _document.Structure = Structure;
            _document.ApplyAccessibilitySetup(
                Configuration.Language,
                Configuration.Title,
                _effectiveProfile,
                Configuration.StrictConformance);
            _document.ValidateAccessibilityAuthoringSnapshot();
        });

        _committed = true;
        Dispose();
        return new RemediationReport(
            committed: true,
            appliedAccessibilitySetup: true,
            claims: report.Claims,
            skippedClaims: report.SkippedClaims,
            diagnostics: report.Diagnostics,
            suppressions: _suppressions,
            ruleEvaluations: report.RuleEvaluations,
            autoArtifacts: report.AutoArtifacts.Select(x => x with
            {
                Disposition = RemediationAutoArtifactDisposition.Applied
            }).ToList(),
            predicateTraces: report.PredicateTraces,
            assertionOutcomes: report.AssertionOutcomes,
            plannedSemanticTree: report.PlannedSemanticTree,
            unaccountedContent: report.UnaccountedContent,
            annotationInventory: report.AnnotationInventory,
            warnings: report.Warnings,
            templateDifferences: report.TemplateDifferences,
            templateAssembly: report.TemplateAssembly,
            runtimeDiagnostics: report.RuntimeDiagnostics,
            bindingEvaluations: report.BindingEvaluations,
            orderComparisons: report.OrderComparisons,
            occurrencePartitions: report.OccurrencePartitions);
    }

    private IReadOnlyList<RemediationOccurrencePartition> BuildProgramOccurrencePartitions(
        CompiledRemediationProgram compiled,
        IReadOnlyList<PageRemediationState> pageStates,
        IReadOnlyList<RemediationClaim> claims,
        List<string> diagnostics,
        bool emitDiagnostics = true)
    {
        var repeated = compiled.Slots.Values
            .Where(x => x.Node.Children.Count > 0 &&
                x.Node.Occurrence is TemplateOccurrence.ZeroOrMore or TemplateOccurrence.OneOrMore)
            .OrderBy(x => x.Depth)
            .ThenBy(x => x.Reference.Path, StringComparer.Ordinal)
            .ToArray();
        if (repeated.Length == 0) return Array.Empty<RemediationOccurrencePartition>();

        var result = new List<RemediationOccurrencePartition>();
        var ranges = new Dictionary<SlotRef, List<ProgramOccurrenceRange>>();
        var rootIdentity = TemplateOccurrenceIdentity.Root(
            compiled.Program.Template.Id, compiled.Program.Template.Version);
        var boundaryDeclarations = compiled.Program.Boundaries
            .GroupBy(x => x.Id, StringComparer.Ordinal)
            .ToDictionary(x => x.Key, x => x.First(), StringComparer.Ordinal);

        foreach (var descriptor in repeated)
        {
            var parentRepeated = repeated
                .Where(x => x.Depth < descriptor.Depth &&
                    descriptor.Reference.Path.StartsWith(x.Reference.Path + "/", StringComparison.Ordinal))
                .OrderByDescending(x => x.Depth)
                .FirstOrDefault();
            var parents = parentRepeated != null && ranges.TryGetValue(parentRepeated.Reference, out var parentRanges)
                ? parentRanges
                : new List<ProgramOccurrenceRange>
                {
                    new(rootIdentity, long.MinValue, long.MaxValue)
                };
            var slotRanges = new List<ProgramOccurrenceRange>();
            ranges[descriptor.Reference] = slotRanges;

            foreach (var parent in parents)
            {
                var descendantClaims = claims
                    .Where(x => x.Status == ClaimStatus.Applied &&
                        x.ProgramSlot != null &&
                        x.ProgramSlot.Path.StartsWith(descriptor.Reference.Path + "/", StringComparison.Ordinal))
                    .Select(x => (Claim: x, Ordinal: ClaimOrdinal(x)))
                    .Where(x => x.Ordinal >= parent.Start && x.Ordinal < parent.End)
                    .OrderBy(x => x.Ordinal)
                    .ThenBy(x => x.Claim.ClaimId.Value, StringComparer.Ordinal)
                    .ToArray();
                var activations = ResolveBoundaryActivations(
                        compiled, descriptor, pageStates, claims, boundaryDeclarations, diagnostics,
                        emitDiagnostics)
                    .Where(x => x.Ordinal >= parent.Start && x.Ordinal < parent.End)
                    .OrderBy(x => x.Ordinal)
                    .ThenBy(x => x.CandidateId, StringComparer.Ordinal)
                    .ToArray();

                foreach (var duplicate in activations.GroupBy(x => x.CandidateId, StringComparer.Ordinal)
                             .Where(x => x.Count() > 1))
                {
                    if (emitDiagnostics) ReportDiagnostic(
                        DiagnosticCode.TemplateOccurrenceViolation,
                        $"Program:{compiled.Program.Id}:Template:{descriptor.Reference.Path}",
                        $"Occurrence boundary for '{descriptor.Reference}' activated more than once for source '{duplicate.Key}'.",
                        diagnostics);
                }
                activations = activations
                    .DistinctBy(x => x.CandidateId, StringComparer.Ordinal)
                    .ToArray();

                if (activations.Length == 0)
                {
                    if (descendantClaims.Length > 0 ||
                        descriptor.Node.Occurrence == TemplateOccurrence.OneOrMore)
                    {
                        if (emitDiagnostics) ReportDiagnostic(
                            DiagnosticCode.TemplateOccurrenceViolation,
                            $"Program:{compiled.Program.Id}:Template:{descriptor.Reference.Path}",
                            $"Repeating composite slot '{descriptor.Reference}' has no occurrence boundary activation.",
                            diagnostics);
                    }
                    continue;
                }

                var rejectedBeforeFirst = descendantClaims
                    .Where(x => x.Ordinal < activations[0].Ordinal)
                    .Select(x => x.Claim.ClaimId.Value)
                    .ToArray();
                if (rejectedBeforeFirst.Length > 0)
                {
                    if (emitDiagnostics) ReportDiagnostic(
                        DiagnosticCode.TemplateOccurrenceViolation,
                        $"Program:{compiled.Program.Id}:Template:{descriptor.Reference.Path}",
                        $"Repeating composite slot '{descriptor.Reference}' has {rejectedBeforeFirst.Length} descendant claim(s) before its first opener.",
                        diagnostics);
                }

                for (var index = 0; index < activations.Length; index++)
                {
                    var activation = activations[index];
                    var end = index + 1 < activations.Length ? activations[index + 1].Ordinal : parent.End;
                    var assigned = descendantClaims
                        .Where(x => x.Ordinal >= activation.Ordinal && x.Ordinal < end)
                        .Select(x => x.Claim)
                        .DistinctBy(x => x.ClaimId)
                        .ToArray();
                    var identity = TemplateOccurrenceIdentity.Append(
                        parent.Identity, descriptor.Reference, descriptor.Node.Name!, index + 1);
                    var disposition = RemediationDiagnosticDisposition.Warning;

                    foreach (var required in descriptor.Node.Children
                                 .Where(x => x.Occurrence == TemplateOccurrence.ExactlyOne))
                    {
                        var requiredPath = $"{descriptor.Reference.Path}/{required.Name}";
                        var matching = assigned.Count(x => x.ProgramSlot != null &&
                            (x.ProgramSlot.Path == requiredPath ||
                             x.ProgramSlot.Path.StartsWith(requiredPath + "/", StringComparison.Ordinal)));
                        // A synthesized composite direct child is represented by its descendants,
                        // not by a synthetic parent claim. Presence therefore counts as one child.
                        var count = required.Children.Count > 0 && matching > 0 ? 1 : matching;
                        if (count == 1) continue;
                        disposition = Configuration.RunMode == RemediationRunMode.Authoring
                            ? RemediationDiagnosticDisposition.WorkItem
                            : RemediationDiagnosticDisposition.Error;
                        if (emitDiagnostics) ReportDiagnostic(
                            DiagnosticCode.TemplateSlotUnfilled,
                            $"Program:{compiled.Program.Id}:Template:{requiredPath}",
                            $"Occurrence '{identity}' requires exactly one '{requiredPath}' claim, found {count}.",
                            diagnostics);
                    }

                    result.Add(new RemediationOccurrencePartition(
                        descriptor.Reference,
                        parent.Identity,
                        identity,
                        BoundaryDebug(descriptor.Node.OccurrenceBoundary),
                        new[] { activation.CandidateId },
                        activation.SourceReferences,
                        assigned.Select(x => x.ClaimId.Value).ToArray(),
                        index == 0 ? rejectedBeforeFirst : Array.Empty<string>(),
                        assigned.SelectMany(x => x.PageIndexes)
                            .Append(activation.PageIndex).Distinct().OrderBy(x => x).ToArray(),
                        disposition,
                        activation.Ordinal));
                    slotRanges.Add(new ProgramOccurrenceRange(identity, activation.Ordinal, end));
                }
            }
        }

        return result;
    }

    private IReadOnlyList<ProgramBoundaryActivation> ResolveBoundaryActivations(
        CompiledRemediationProgram compiled,
        CompiledTemplateSlot descriptor,
        IReadOnlyList<PageRemediationState> pageStates,
        IReadOnlyList<RemediationClaim> claims,
        IReadOnlyDictionary<string, OccurrenceBoundaryDeclaration> declarations,
        List<string> diagnostics,
        bool emitDiagnostics)
    {
        var boundary = descriptor.Node.OccurrenceBoundary ?? new OccurrenceBoundary.Derived();
        SlotRef? opener = boundary switch
        {
            OccurrenceBoundary.Derived => SlotRef.Absolute(
                $"{descriptor.Reference.Path}/{descriptor.Node.Children[0].Name}"),
            OccurrenceBoundary.StartsOnSlot slot when slot.Reference.IsAbsolute => slot.Reference,
            OccurrenceBoundary.StartsOnSlot slot => SlotRef.Absolute(
                $"{descriptor.Reference.Path}/{slot.Reference.Path[2..]}"),
            _ => null
        };
        if (opener != null)
        {
            return claims
                .Where(x => x.Status == ClaimStatus.Applied && x.ProgramSlot == opener)
                .Select(x =>
                {
                    var candidate = x.Candidates.FirstOrDefault();
                    return new ProgramBoundaryActivation(
                        candidate?.CandidateId ?? x.ClaimId.Value,
                        ClaimOrdinal(x),
                        x.PageIndex,
                        candidate?.SourceReferences ?? Array.Empty<StructuredSourceRef>());
                })
                .ToArray();
        }

        if (boundary is not OccurrenceBoundary.StartsOnBoundary named ||
            !declarations.TryGetValue(named.Id, out var declaration))
        {
            return Array.Empty<ProgramBoundaryActivation>();
        }

        var anchors = compiled.Program.Anchors.ToDictionary(x => x.Id, StringComparer.Ordinal);
        var activations = new List<ProgramBoundaryActivation>();
        foreach (var pageState in pageStates.Where(x =>
                     declaration.Pages.Includes(x.PageIndex, _document.Pages.Count)))
        {
            var context = CreateProgramEvaluationContext(
                pageState, claims, anchors,
                new Dictionary<string, TolerancedZone>(StringComparer.Ordinal),
                new Dictionary<string, FlowRegion>(StringComparer.Ordinal),
                diagnostics, compiled.Program.TextNormalization, emitDiagnostics);
            foreach (var candidate in SelectCandidates(pageState, declaration.Candidates))
            {
                if (!declaration.Predicate.Evaluate(context, candidate).IsMatch) continue;
                activations.Add(new ProgramBoundaryActivation(
                    candidate.CandidateId,
                    CandidateOrdinal(pageState.PageIndex, candidate),
                    pageState.PageIndex,
                    candidate.SourceReferences));
            }
        }
        return activations;
    }

    private static long ClaimOrdinal(RemediationClaim claim)
    {
        var candidate = claim.Candidates.OrderBy(x => x.ContentOrderIndex).FirstOrDefault();
        return candidate == null
            ? ((long)claim.PageIndex << 32) | uint.MaxValue
            : CandidateOrdinal(claim.PageIndex, candidate);
    }

    private static long CandidateOrdinal(int pageIndex, RemediationCandidate candidate) =>
        ((long)pageIndex << 32) | (uint)candidate.ContentOrderIndex;

    private static string BoundaryDebug(OccurrenceBoundary? boundary) =>
        boundary switch
        {
            null or OccurrenceBoundary.Derived => "derived",
            OccurrenceBoundary.StartsOnSlot slot => $"slot:{slot.Reference}",
            OccurrenceBoundary.StartsOnBoundary named => $"boundary:{named.Id}",
            _ => "unknown"
        };

    private sealed record ProgramOccurrenceRange(string Identity, long Start, long End);
    private sealed record ProgramBoundaryActivation(
        string CandidateId,
        long Ordinal,
        int PageIndex,
        IReadOnlyList<StructuredSourceRef> SourceReferences);

    private void ExecuteProgramLayers(
        CompiledRemediationProgram compiled,
        IReadOnlyList<PageRemediationState> pageStates,
        List<RemediationClaim> allClaims,
        List<RemediationClaim> skippedClaims,
        List<string> diagnostics,
        List<ProgramBindingRun> bindingRuns)
    {
        var priorLayerClaims = new List<RemediationClaim>();
        var anchors = compiled.Program.Anchors
            .ToDictionary(x => x.Id, StringComparer.Ordinal);
        var emptyZones = new Dictionary<string, TolerancedZone>(StringComparer.Ordinal);
        var emptyFlows = new Dictionary<string, FlowRegion>(StringComparer.Ordinal);
        IReadOnlyList<RemediationOccurrencePartition> occurrenceSnapshot =
            Array.Empty<RemediationOccurrencePartition>();

        for (var layerIndex = 0; layerIndex < compiled.Layers.Count; layerIndex++)
        {
            var frozenClaims = priorLayerClaims
                .Where(x => x.Status == ClaimStatus.Applied)
                .ToArray();
            var layerClaims = new List<RemediationClaim>();

            foreach (var binding in compiled.Layers[layerIndex].OrderBy(x => x.Id, StringComparer.Ordinal))
            {
                var slot = binding.Target is BindingTarget.Slot target ? target.Reference : null;
                var artifactId = binding.Target is BindingTarget.Artifact artifact ? artifact.Id : null;
                var run = new ProgramBindingRun(binding.Id, layerIndex, slot, artifactId);
                foreach (var selectedPage in binding.Pages.SelectPages(_document.Pages.Count))
                {
                    run.PageMatches[selectedPage] = 0;
                }

                foreach (var pageState in pageStates
                             .Where(x => binding.Pages.Includes(x.PageIndex, _document.Pages.Count)))
                {
                    var context = CreateProgramEvaluationContext(
                        pageState,
                        frozenClaims,
                        anchors,
                        emptyZones,
                        emptyFlows,
                        diagnostics,
                        compiled.Program.TextNormalization);
                    var candidates = SelectCandidates(pageState, binding.Candidates);
                    var matches = new List<(RemediationCandidate Candidate, PredicateResult Result)>();
                    foreach (var candidate in candidates)
                    {
                        var traced = _traceRequest?.Includes(binding.Id, pageState.PageIndex, candidate) == true;
                        var result = binding.Predicate
                            .Evaluate(context.WithPredicateTracing(traced)
                                .WithProgramOccurrence(candidate, occurrenceSnapshot), candidate)
                            with
                            {
                                BindingId = binding.Id,
                                ProgramSlot = slot
                            };
                        if (result.IsMatch)
                        {
                            matches.Add((candidate, result));
                        }
                        else if (traced && result.Trace != null)
                        {
                            _predicateTraces?.Add(new RemediationPredicateTrace(
                                binding.Id,
                                pageState.PageIndex,
                                candidate.CandidateId,
                                candidate.Kind,
                                candidate is TextRemediationCandidate text ? text.Text : string.Empty,
                                candidate is TextRemediationCandidate normalizedText
                                    ? compiled.Program.TextNormalization.Normalize(normalizedText.Text)
                                    : string.Empty,
                                candidate.SourceReferences,
                                result.Trace));
                        }
                    }

                    if (ContainsNearestPredicate(binding.Predicate) && matches.Count > 1)
                    {
                        var maxConfidence = matches.Max(x => x.Result.Confidence);
                        matches = matches
                            .Where(x => Math.Abs(x.Result.Confidence - maxConfidence) <= 0.000001)
                            .ToList();
                        if (matches.Count > 1)
                        {
                            _programRuntimeDiagnostics!.Add(new RemediationRuntimeDiagnostic(
                                DiagnosticCode.ProgramBindingAmbiguous,
                                RemediationDiagnosticDisposition.Error,
                                $"Program:{compiled.Program.Id}:Binding:{binding.Id}",
                                $"Program binding '{binding.Id}' has an ambiguous nearest-anchor match with {matches.Count} equally near candidates.",
                                slot,
                                binding.Id,
                                matches.Select(x => x.Candidate.CandidateId).ToArray()));
                        }
                    }

                    run.InputsConsidered += candidates.Count;
                    run.InputsMatched += matches.Count;
                    run.PageMatches[pageState.PageIndex] = matches.Count;
                    foreach (var (candidate, result) in matches)
                    {
                        var confidence = result.Confidence;
                        if (binding.MinConfidence is { } minConfidence && confidence < minConfidence)
                        {
                            run.RejectedByConfidence++;
                            skippedClaims.Add(CreateProgramClaim(
                                compiled,
                                binding,
                                pageState.PageIndex,
                                candidate,
                                ClaimStatus.Skipped,
                                confidence,
                                slot));
                            continue;
                        }

                        var conflicting = FindProgramConflicts(pageState, candidate)
                            .Concat(FindProgramLayerConflicts(layerClaims, pageState.PageIndex, candidate))
                            .ToArray();
                        if (conflicting.Length > 0)
                        {
                            run.RejectedByConflict++;
                            skippedClaims.Add(CreateProgramClaim(
                                compiled,
                                binding,
                                pageState.PageIndex,
                                candidate,
                                ClaimStatus.Skipped,
                                confidence,
                                slot));
                            var previous = conflicting[0].Claim;
                            if (candidate is AnnotationRemediationCandidate)
                            {
                                ReportDiagnostic(
                                    DiagnosticCode.AnnotationAlreadyConsumed,
                                    $"Page{pageState.PageIndex + 1}",
                                    $"Annotation '{candidate.CandidateId}' selected by program binding '{binding.Id}' is already consumed by binding '{previous.BindingId ?? previous.RuleId}'.",
                                    diagnostics);
                            }
                            else
                            {
                                _programRuntimeDiagnostics!.Add(new RemediationRuntimeDiagnostic(
                                    DiagnosticCode.ProgramBindingConflict,
                                    RemediationDiagnosticDisposition.Error,
                                    $"Program:{compiled.Program.Id}:Binding:{binding.Id}",
                                    $"Program binding '{binding.Id}' conflicts with binding '{previous.BindingId ?? previous.RuleId}' for candidate '{candidate.CandidateId}'.",
                                    slot,
                                    binding.Id,
                                    new[] { candidate.CandidateId },
                                    new Dictionary<string, object?>
                                    {
                                        ["conflictingBindingId"] = previous.BindingId ?? previous.RuleId
                                    }));
                            }
                            continue;
                        }

                        var claim = CreateProgramClaim(
                            compiled,
                            binding,
                            pageState.PageIndex,
                            candidate,
                            ClaimStatus.Applied,
                            confidence,
                            slot);
                        layerClaims.Add(claim);
                    }
                }

                bindingRuns.Add(run);
                CheckProgramCardinality(compiled, binding, run, diagnostics);
            }

            foreach (var claim in layerClaims)
            {
                var pageState = pageStates[claim.PageIndex];
                foreach (var candidate in claim.Candidates)
                {
                    pageState.TextOwnership.Add(claim, GetTargetSpans(candidate));
                    if (candidate is ContentRemediationCandidate content)
                    {
                        pageState.ContentOwnership[content.Item] = claim;
                    }
                    if (candidate is AnnotationRemediationCandidate annotation)
                    {
                        pageState.AnnotationOwnership[annotation.Annotation] = claim;
                    }
                }
            }
            layerClaims.Sort(CompareClaimsInReadingOrder);
            allClaims.AddRange(layerClaims);
            priorLayerClaims.AddRange(layerClaims);
            occurrenceSnapshot = BuildProgramOccurrencePartitions(
                compiled, pageStates, priorLayerClaims, diagnostics, emitDiagnostics: false);
        }
    }

    private RemediationEvaluationContext CreateProgramEvaluationContext(
        PageRemediationState pageState,
        IReadOnlyList<RemediationClaim> claims,
        IReadOnlyDictionary<string, RemediationAnchor> anchors,
        IReadOnlyDictionary<string, TolerancedZone> zones,
        IReadOnlyDictionary<string, FlowRegion> flows,
        List<string> diagnostics,
        TextNormalizationOptions normalization,
        bool emitRuntimeDiagnostics = true)
    {
        var candidates = pageState.TextCandidates.Values
            .SelectMany(x => x)
            .Cast<RemediationCandidate>()
            .Concat(pageState.ContentCandidates)
            .Concat(pageState.AnnotationCandidates)
            .ToArray();
        var documentFlows = new DocumentFlowIndex(
            new Dictionary<int, Dictionary<string, FlowRegionResolution>>(),
            candidates,
            claims);
        return new RemediationEvaluationContext(
            claims,
            pageBox: pageState.StructuredText.RelativePageBox,
            pageIndex: pageState.PageIndex,
            pageCount: _document.Pages.Count,
            configuration: Configuration,
            anchors: anchors,
            tolerancedZones: zones,
            flowRegions: flows,
            structuredText: pageState.StructuredText,
            diagnostics: diagnostics,
            textNormalization: normalization,
            flowsArePreResolved: true)
        {
            DocumentFlows = documentFlows,
            DocumentCandidates = candidates,
            ProgramDiagnosticScope = $"Program:{_program!.Program.Id}:Page:{pageState.PageIndex + 1}",
            RuntimeDiagnosticSink = emitRuntimeDiagnostics
                ? diagnostic => _programRuntimeDiagnostics?.Add(diagnostic)
                : null
        };
    }

    private IReadOnlyList<OwnedTextSpan> FindProgramConflicts(
        PageRemediationState pageState,
        RemediationCandidate candidate)
    {
        if (candidate is AnnotationRemediationCandidate annotation &&
            pageState.AnnotationOwnership.TryGetValue(annotation.Annotation, out var annotationClaim))
        {
            return new[] { new OwnedTextSpan(default, annotationClaim) };
        }

        if (candidate is ContentRemediationCandidate content &&
            pageState.ContentOwnership.TryGetValue(content.Item, out var contentClaim))
        {
            return new[] { new OwnedTextSpan(default, contentClaim) };
        }

        return pageState.TextOwnership.FindOverlaps(GetTargetSpans(candidate));
    }

    private static IReadOnlyList<OwnedTextSpan> FindProgramLayerConflicts(
        IReadOnlyList<RemediationClaim> layerClaims,
        int pageIndex,
        RemediationCandidate candidate)
    {
        var pageClaims = layerClaims.Where(x => x.PageIndex == pageIndex).ToArray();
        if (candidate is AnnotationRemediationCandidate annotation)
        {
            var owner = pageClaims.FirstOrDefault(x => x.Candidates
                .OfType<AnnotationRemediationCandidate>()
                .Any(y => ReferenceEquals(y.Annotation, annotation.Annotation)));
            return owner == null ? Array.Empty<OwnedTextSpan>() : new[] { new OwnedTextSpan(default, owner) };
        }
        if (candidate is ContentRemediationCandidate content)
        {
            var owner = pageClaims.FirstOrDefault(x => x.Candidates
                .OfType<ContentRemediationCandidate>()
                .Any(y => ReferenceEquals(y.Item, content.Item)));
            if (owner != null) return new[] { new OwnedTextSpan(default, owner) };
        }
        var index = new TextOwnershipIndex();
        foreach (var claim in pageClaims)
        {
            index.Add(claim, claim.Candidates.SelectMany(GetTargetSpans));
        }
        return index.FindOverlaps(GetTargetSpans(candidate));
    }

    private RemediationClaim CreateProgramClaim(
        CompiledRemediationProgram compiled,
        CompiledBinding binding,
        int pageIndex,
        RemediationCandidate candidate,
        ClaimStatus status,
        double confidence,
        SlotRef? slot)
    {
        var action = ProgramAction(compiled, binding);
        var tag = slot != null && compiled.Slots.TryGetValue(slot, out var descriptor)
            ? descriptor.Node.Tag
            : "Artifact";
        return new RemediationClaim(binding.Id, new[] { candidate }, tag, confidence)
        {
            BindingId = binding.Id,
            DefinitionId = binding.DefinitionId,
            ProgramSlot = slot,
            PageIndex = pageIndex,
            Status = status,
            SelectorDebugString = binding.Predicate.DebugString,
            Action = action,
            RuleSetId = compiled.Program.Id,
            SlotId = slot?.Path[1..],
            TextNormalization = compiled.Program.TextNormalization
        };
    }

    private static RemediationAction ProgramAction(
        CompiledRemediationProgram compiled,
        CompiledBinding binding)
    {
        return binding.Target switch
        {
            BindingTarget.Slot => RemediationActions.Bind(),
            BindingTarget.Artifact artifact =>
                CreateArtifactAction(compiled, artifact.Id),
            _ => throw new InvalidOperationException(
                $"Binding '{binding.Id}' has an unsupported compiled target.")
        };

        static RemediationAction CreateArtifactAction(
            CompiledRemediationProgram compiled,
            string id)
        {
            var artifact = compiled.Program.Artifacts.Single(x =>
                string.Equals(x.Id, id, StringComparison.Ordinal));
            return new ArtifactRemediationAction(
                artifact.Subtype,
                artifact.SemanticSubtype,
                artifact.IncludeBoundingBox,
                artifact.Attached);
        }
    }

    private void CheckProgramCardinality(
        CompiledRemediationProgram compiled,
        CompiledBinding binding,
        ProgramBindingRun run,
        List<string> diagnostics)
    {
        if (binding.Cardinality is not { } cardinality)
        {
            return;
        }

        if (cardinality.Scope == RuleCardinalityScope.Document)
        {
            if (!cardinality.Accepts(run.InputsMatched))
            {
                ReportDiagnostic(
                    DiagnosticCode.RuleCardinalityMismatch,
                    $"Program:{compiled.Program.Id}:Binding:{binding.Id}",
                    $"Program binding '{binding.Id}' expected {cardinality.ExpectedDescription} matched input(s) across its selected pages, but observed {run.InputsMatched}.",
                    diagnostics);
            }
            return;
        }

        foreach (var page in run.PageMatches)
        {
            if (!cardinality.Accepts(page.Value))
            {
                ReportDiagnostic(
                    DiagnosticCode.RuleCardinalityMismatch,
                    $"Program:{compiled.Program.Id}:Binding:{binding.Id}:Page{page.Key + 1}",
                    $"Program binding '{binding.Id}' expected {cardinality.ExpectedDescription} matched input(s) on page {page.Key + 1}, but observed {page.Value}.",
                    diagnostics);
            }
        }
    }

    private IReadOnlyList<RemediationAssertionOutcome> EvaluateProgramAssertions(
        CompiledRemediationProgram compiled,
        IReadOnlyList<RemediationClaim> claims,
        List<string> diagnostics)
    {
        var outcomes = new List<RemediationAssertionOutcome>();
        foreach (var assertion in compiled.Program.Assertions)
        {
            var pages = assertion.Scope == SemanticAssertionScope.Document
                ? new int?[] { null }
                : Enumerable.Range(0, _document.Pages.Count)
                    .Where(x => assertion.Pages.Includes(x, _document.Pages.Count))
                    .Select(x => (int?)x)
                    .ToArray();
            foreach (var page in pages)
            {
                var selected = claims.Where(x =>
                        x.Status == ClaimStatus.Applied &&
                        x.ProgramSlot == assertion.Slot &&
                        (page == null
                            ? assertion.Pages.Includes(x.PageIndex, _document.Pages.Count)
                            : ClaimAppearsOnPage(x, page.Value)))
                    .ToArray();
                var passed = assertion.Expected.Accepts(selected.Length);
                var pageText = page is { } pageIndex ? $" on page {pageIndex + 1}" : string.Empty;
                outcomes.Add(new RemediationAssertionOutcome(
                    compiled.Program.Id,
                    assertion.Id,
                    page,
                    assertion.Expected.Description,
                    selected.Length.ToString(),
                    passed,
                    null,
                    null)
                {
                    BindingId = selected.FirstOrDefault()?.BindingId,
                    ProgramSlot = assertion.Slot
                });
                if (!passed)
                {
                    ReportDiagnostic(
                        DiagnosticCode.SemanticAssertionFailed,
                        $"Program:{compiled.Program.Id}:Assertion:{assertion.Id}" +
                        (page is { } pi ? $":Page{pi + 1}" : string.Empty),
                        $"Program '{compiled.Program.Id}' assertion '{assertion.Id}'{pageText} expected {assertion.Expected.Description}, but observed {selected.Length}.",
                        diagnostics);
                }
            }
        }
        return outcomes;
    }

    private IReadOnlyList<RemediationTemplateDifference> EvaluateProgramArtifactInventory(
        CompiledRemediationProgram compiled,
        IReadOnlyList<PageRemediationState> pageStates,
        IReadOnlyList<RemediationClaim> claims,
        IReadOnlyList<RemediationAutoArtifactOutcome> autoArtifacts,
        List<string> diagnostics)
    {
        if (ArtifactInventory.Count == 0)
        {
            return Array.Empty<RemediationTemplateDifference>();
        }

        var bindingTargets = compiled.Program.Bindings
            .ToDictionary(x => x.Id, x => x.Target, StringComparer.Ordinal);
        var records = new List<RemediationArtifactRecord>();
        foreach (var claim in claims.Where(x => x.Action is ArtifactRemediationAction))
        {
            var artifact = (ArtifactRemediationAction)claim.Action!;
            var boundItemId = claim.BindingId != null &&
                              bindingTargets.TryGetValue(claim.BindingId, out var target) &&
                              target is BindingTarget.Artifact declared
                ? declared.Id
                : null;
            foreach (var page in claim.Candidates.Where(x => x.PageIndex >= 0).GroupBy(x => x.PageIndex))
            {
                records.Add(new RemediationArtifactRecord(
                    page.Key,
                    artifact.Subtype,
                    UnionBounds(page.Select(x => x.RelativeBoundingBox)),
                    claim.BindingId,
                    compiled.Program.Id,
                    boundItemId,
                    artifact.SemanticSubtype));
            }
        }
        records.AddRange(autoArtifacts.Select(x =>
            new RemediationArtifactRecord(x.PageIndex, null, x.RelativeBoundingBox)));

        var zonesByPage = pageStates.ToDictionary(x => x.PageIndex, x => x.ArtifactZones);
        return RemediationArtifactInventoryMatcher.Match(
                ArtifactInventory,
                records,
                _document.Pages.Count,
                zonesByPage)
            .Select(x => ReportArtifactDifference(x, diagnostics))
            .ToList();
    }

    private void EvaluateProgramSourceOrder(
        CompiledRemediationProgram compiled,
        PrescriptiveTemplateAssemblyPlan plan,
        IReadOnlyDictionary<ClaimId, RemediationClaim> claims,
        List<string> diagnostics,
        List<RemediationOrderComparison> comparisons)
    {
        var emitted = new HashSet<string>(StringComparer.Ordinal);

        void Visit(PrescriptiveTemplateAssemblyNode container)
        {
            var declaredChildren = container.ProgramTemplate?.Children ?? Array.Empty<RemediationTemplateNode>();
            var ranks = declaredChildren
                .Select((node, index) => (node.Name, index))
                .ToDictionary(x => x.Name, x => x.index, StringComparer.Ordinal);
            var observed = container.Children
                .Where(x => x.LocalName != null && ranks.ContainsKey(x.LocalName))
                .Select(x => (Node: x, Rank: ranks[x.LocalName!], Evidence: EvidenceFor(x)))
                .Where(x => x.Evidence.HasEvidence)
                .ToArray();
            var geometry = observed
                .Where(x => x.Evidence.Bounds != null)
                .OrderBy(x => x.Evidence.Page)
                .ThenByDescending(x => x.Evidence.Bounds!.URy)
                .ThenBy(x => x.Evidence.Bounds!.LLx)
                .ThenBy(x => x.Node.Identity, StringComparer.Ordinal)
                .Select((x, index) => (x.Node.Identity, index))
                .ToDictionary(x => x.Identity, x => x.index, StringComparer.Ordinal);

            for (var right = 1; right < observed.Length; right++)
            {
                for (var left = 0; left < right; left++)
                {
                    var first = observed[left];
                    var second = observed[right];
                    var firstRank = first.Rank;
                    var secondRank = second.Rank;
                    if (firstRank == secondRank)
                        continue;

                    var sourceOrdered = new[] { first, second }
                        .Where(x => x.Evidence.SourceKey != null)
                        .OrderBy(x => x.Evidence.SourceKey)
                        .Select((x, index) => (x.Node.Identity, index))
                        .ToDictionary(x => x.Identity, x => x.index, StringComparer.Ordinal);
                    var firstSource = sourceOrdered.TryGetValue(first.Node.Identity, out var firstSourceIndex)
                        ? firstSourceIndex : (int?)null;
                    var secondSource = sourceOrdered.TryGetValue(second.Node.Identity, out var secondSourceIndex)
                        ? secondSourceIndex : (int?)null;
                    var firstGeometry = geometry.TryGetValue(first.Node.Identity, out var firstIndex)
                        ? firstIndex
                        : (int?)null;
                    var secondGeometry = geometry.TryGetValue(second.Node.Identity, out var secondIndex)
                        ? secondIndex
                        : (int?)null;
                    var declaredFirst = firstRank < secondRank;
                    var sourceInverted = firstSource is not null && secondSource is not null &&
                        firstSource != secondSource && (firstSource < secondSource) != declaredFirst;
                    var geometryInverted = firstGeometry is not null && secondGeometry is not null &&
                        firstGeometry != secondGeometry && (firstGeometry < secondGeometry) != declaredFirst;
                    if (!sourceInverted && !geometryInverted)
                        continue;
                    var key = $"{container.Identity}\0{first.Node.Identity}\0{second.Node.Identity}";
                    if (!emitted.Add(key))
                        continue;
                    var acknowledged = container.ProgramTemplate?.OrderPolicy ==
                        TemplateOrderPolicy.AllowDeclaredReorder;
                    var disposition = acknowledged
                        ? RemediationDiagnosticDisposition.Acknowledged
                        : RemediationDiagnosticDisposition.Error;
                    comparisons.Add(new RemediationOrderComparison(
                        container.SlotReference,
                        first.Node.Identity,
                        second.Node.Identity,
                        firstRank,
                        secondRank,
                        firstSource,
                        secondSource,
                        firstGeometry,
                        secondGeometry,
                        first.Evidence.Pages.Concat(second.Evidence.Pages).Distinct().OrderBy(x => x).ToArray(),
                        first.Evidence.Bounds,
                        second.Evidence.Bounds,
                        first.Evidence.Sources,
                        second.Evidence.Sources,
                        disposition)
                    {
                        ContainerIdentity = container.Identity,
                        FirstCandidateIds = first.Evidence.CandidateIds,
                        SecondCandidateIds = second.Evidence.CandidateIds
                    });

                    if (!acknowledged)
                    {
                        var containerPath = container.SlotReference?.Path ?? "/";
                        ReportDiagnostic(
                            DiagnosticCode.TemplateWrongOrder,
                            $"Program:{compiled.Program.Id}:Template:{containerPath}",
                            $"Program template order is inverted in '{containerPath}': " +
                            $"source or geometric evidence disagrees with the declared order of " +
                            $"'{first.Node.SlotReference?.Path}' and '{second.Node.SlotReference?.Path}'.",
                            diagnostics);
                    }
                }
            }

            foreach (var child in container.Children.Where(x => x.Children.Count > 0))
                Visit(child);
        }

        Visit(plan.Document);

        (bool HasEvidence, (int Page, int Order)? SourceKey, int Page,
            IReadOnlyList<int> Pages, PdfRect<double>? Bounds,
            IReadOnlyList<StructuredSourceRef> Sources,
            IReadOnlyList<string> CandidateIds) EvidenceFor(
                PrescriptiveTemplateAssemblyNode node)
        {
            var descendants = Flatten(node).ToArray();
            var candidates = descendants
                .Where(x => x.ClaimId != null && claims.ContainsKey(x.ClaimId.Value))
                .SelectMany(x => claims[x.ClaimId!.Value].Candidates)
                .ToArray();
            var pages = candidates.Select(x => x.PageIndex).Distinct().OrderBy(x => x).ToArray();
            var sourceKey = candidates
                .OrderBy(x => x.PageIndex)
                .ThenBy(x => x.ContentOrderIndex)
                .Select(x => ((int Page, int Order)?)(x.PageIndex, x.ContentOrderIndex))
                .FirstOrDefault();
            var bounds = candidates.Length == 0 ? null :
                RemediationSession.UnionBounds(candidates.Select(x => x.BoundingBox));
            var sources = candidates.SelectMany(x => x.SourceReferences).Distinct().ToArray();
            return (candidates.Length > 0, sourceKey, pages.DefaultIfEmpty(int.MaxValue).First(),
                pages, bounds, sources, candidates.Select(x => x.CandidateId).Distinct().ToArray());
        }

        static IEnumerable<PrescriptiveTemplateAssemblyNode> Flatten(
            PrescriptiveTemplateAssemblyNode node)
        {
            yield return node;
            foreach (var child in node.Children.SelectMany(Flatten))
                yield return child;
        }
    }


    private void ValidateProgramPlan(
        IReadOnlyList<PageRemediationState> pageStates,
        IReadOnlyList<RemediationClaim> claims,
        List<string> diagnostics)
    {
        var pages = pageStates.ToDictionary(x => x.PageIndex);
        foreach (var claim in claims.Where(x => x.Status == ClaimStatus.Applied))
        {
            foreach (var candidate in claim.Candidates)
            {
                if (pages.TryGetValue(candidate.PageIndex, out var pageState))
                {
                    ValidateClaimTargets(pageState, claim, diagnostics);
                }
            }
        }
    }

    private void ApplyProgramPlan(
        IReadOnlyList<PageRemediationState> pageStates,
        IReadOnlyList<RemediationClaim> claims,
        List<string> diagnostics,
        PrescriptiveTemplateAssemblyPlan assemblyPlan)
    {
        var pageLookup = pageStates.ToDictionary(x => x.PageIndex);
        foreach (var claim in claims
                     .Where(x => x.Status == ClaimStatus.Applied)
                     .OrderBy(x => x, ReadingOrderComparer))
        {
            foreach (var candidate in claim.Candidates)
            {
                if (!pageLookup.TryGetValue(candidate.PageIndex, out var pageState))
                {
                    continue;
                }
                ApplyClassifyClaim(pageState, claim, diagnostics);
                AnnotateProgramAppliedBindings(claim);
            }
            if (HasBlockingProgramDiagnostics(diagnostics))
            {
                return;
            }
        }

        ApplyPrescriptiveTemplateAssembly(assemblyPlan, pageStates, diagnostics, claims);
        if (HasBlockingProgramDiagnostics(diagnostics))
        {
            return;
        }

        foreach (var pageState in pageStates)
        {
            ApplyLeftoverPolicyAfterValidation(pageState);
            if (pageState.IsDirty)
            {
                var contents = ContentModelWriter<double>.CreateContent(
                    pageState.Page.Resources,
                    pageState.WorkingContent,
                    _document.Catalog);
                pageState.Page.NativeObject[PdfName.Contents] =
                    PdfIndirectRef.Create(new PdfStream(contents));
            }
        }
    }

    private static void AnnotateProgramAppliedBindings(RemediationClaim claim)
    {
        if (claim.AppliedBindings is not List<RemediationAppliedBinding> bindings ||
            bindings.Count == 0)
        {
            return;
        }

        for (var index = 0; index < bindings.Count; index++)
        {
            bindings[index] = bindings[index] with
            {
                BindingId = claim.BindingId,
                ProgramSlot = claim.ProgramSlot
            };
        }
    }

    private IReadOnlyList<RemediationBindingEvaluationSummary> BuildProgramBindingSummaries(
        CompiledRemediationProgram compiled,
        IReadOnlyList<ProgramBindingRun> runs,
        IReadOnlyList<RemediationClaim> claims,
        IReadOnlyList<RemediationClaim> skipped)
    {
        var applied = claims
            .GroupBy(x => x.BindingId ?? x.RuleId, StringComparer.Ordinal)
            .ToDictionary(x => x.Key, x => x.Count(), StringComparer.Ordinal);
        var skippedByBinding = skipped
            .GroupBy(x => x.BindingId ?? x.RuleId, StringComparer.Ordinal)
            .ToDictionary(x => x.Key, x => x.Count(), StringComparer.Ordinal);
        return runs
            .GroupBy(x => x.BindingId, StringComparer.Ordinal)
            .OrderBy(x => x.Key, StringComparer.Ordinal)
            .Select(group =>
            {
                var run = group.First();
                var binding = compiled.Layers.SelectMany(x => x)
                    .Single(x => x.Id == group.Key);
                var satisfied = binding.Cardinality == null
                    ? (bool?)null
                    : binding.Cardinality.Accepts(group.Sum(x => x.InputsMatched));
                return new RemediationBindingEvaluationSummary(
                    group.Key,
                    run.DependencyLayer,
                    group.Sum(x => x.InputsConsidered),
                    group.Sum(x => x.InputsMatched),
                    applied.GetValueOrDefault(group.Key),
                    skippedByBinding.GetValueOrDefault(group.Key),
                    satisfied)
                {
                    DefinitionId = binding.DefinitionId,
                    ProgramSlot = run.ProgramSlot,
                    ArtifactId = run.ArtifactId,
                    RejectedByConfidence = group.Sum(x => x.RejectedByConfidence),
                    RejectedByConflict = group.Sum(x => x.RejectedByConflict),
                    CardinalityOutcome = binding.Cardinality == null
                        ? null
                        : (satisfied == true ? "passed" : "failed")
                };
            })
            .ToArray();
    }

    private RemediationReport CreateProgramReport(
        bool committed,
        bool accessibilitySetup,
        IReadOnlyList<RemediationClaim> claims,
        IReadOnlyList<RemediationClaim> skippedClaims,
        IReadOnlyList<string> diagnostics,
        IReadOnlyList<string> warnings,
        IReadOnlyList<RemediationBindingEvaluationSummary> bindingEvaluations,
        IReadOnlyList<RemediationOrderComparison> orderComparisons,
        IReadOnlyList<RemediationAutoArtifactOutcome>? autoArtifacts = null,
        IReadOnlyList<RemediationAssertionOutcome>? assertions = null,
        RemediationSemanticTree? semanticTree = null,
        IReadOnlyList<RemediationUnaccountedContent>? unaccountedContent = null,
        IReadOnlyList<RemediationTemplateDifference>? templateDifferences = null,
        IReadOnlyList<RemediationTemplateAssemblyItem>? assembly = null,
        IReadOnlyList<RemediationOccurrencePartition>? occurrencePartitions = null)
    {
        var runtimeDiagnostics = (_programRuntimeDiagnostics ??
                throw new InvalidOperationException("Program diagnostic sink was not initialized."))
            .Where(x => x.Code != DiagnosticCode.TemplateWrongOrder)
            .ToList();
        runtimeDiagnostics.AddRange(orderComparisons.Select(x =>
            new RemediationRuntimeDiagnostic(
                DiagnosticCode.TemplateWrongOrder,
                x.Disposition,
                $"Program:{_program!.Program.Id}:Template:{x.ContainerSlot?.Path ?? "/"}",
                $"Declared order for '{x.FirstOccurrenceIdentity}' and " +
                $"'{x.SecondOccurrenceIdentity}' disagrees with source evidence.",
                x.ContainerSlot,
                CandidateIds: x.FirstCandidateIds.Concat(x.SecondCandidateIds).Distinct().ToArray(),
                Evidence: new Dictionary<string, object?>
                {
                    ["containerIdentity"] = x.ContainerIdentity,
                    ["sourceOrderInverted"] = x.SourceOrderInverted,
                    ["geometricOrderInverted"] = x.GeometricOrderInverted,
                    ["pages"] = x.Pages
                })));
        return new RemediationReport(
            committed,
            accessibilitySetup,
            claims,
            skippedClaims,
            diagnostics: null,
            _suppressions,
            ruleEvaluations: Array.Empty<RuleEvaluationSummary>(),
            autoArtifacts,
            _predicateTraces?.ToList(),
            assertions,
            semanticTree,
            unaccountedContent,
            annotationInventory: Array.Empty<RemediationAnnotationInventoryItem>(),
            warnings: null,
            templateDifferences,
            assembly,
            runtimeDiagnostics,
            bindingEvaluations,
            orderComparisons,
            occurrencePartitions);
    }

    private bool HasBlockingProgramDiagnostics(IReadOnlyList<string> diagnostics) =>
        _programRuntimeDiagnostics?.Any(x => x.IsBlocking) == true;

    private sealed class ProgramBindingRun
    {
        internal ProgramBindingRun(
            string bindingId,
            int dependencyLayer,
            SlotRef? programSlot,
            string? artifactId)
        {
            BindingId = bindingId;
            DependencyLayer = dependencyLayer;
            ProgramSlot = programSlot;
            ArtifactId = artifactId;
        }

        internal string BindingId { get; }
        internal int DependencyLayer { get; }
        internal SlotRef? ProgramSlot { get; }
        internal string? ArtifactId { get; }
        internal int InputsConsidered { get; set; }
        internal int InputsMatched { get; set; }
        internal int RejectedByConfidence { get; set; }
        internal int RejectedByConflict { get; set; }
        internal Dictionary<int, int> PageMatches { get; } = new();
    }
}
