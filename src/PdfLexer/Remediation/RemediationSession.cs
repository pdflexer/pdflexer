using PdfLexer.Content;
using PdfLexer.Content.Model;
using PdfLexer.DOM;
using PdfLexer.Writing;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace PdfLexer.Remediation;

/// <summary>
/// Coordinates rule-driven remediation for a currently untagged PDF document.
/// </summary>
public sealed partial class RemediationSession : IDisposable
{
    private readonly PdfDocument _document;
    private readonly HashSet<PdfPage> _pagesWithAllocatedMcids = new();
    private readonly Dictionary<PdfPage, int> _pageStructParents = new();
    private readonly List<RuleSet> _ruleSets = new();
    private CompiledRemediationProgram? _program;
    private readonly List<DiagnosticSuppression> _suppressions = new();
    private PdfUaProfile _effectiveProfile;
    private static readonly HashSet<DiagnosticCode> NonSuppressibleDiagnosticCodes = new()
    {
        DiagnosticCode.GroupCompositionAmbiguous,
        DiagnosticCode.GroupCompositionCycle,
        DiagnosticCode.AnnotationAdoptionTargetMissing,
        DiagnosticCode.AnnotationAdoptionAmbiguous,
        DiagnosticCode.AnnotationAlreadyConsumed,
        DiagnosticCode.PrescriptiveUnaccountedContent,
        DiagnosticCode.TemplateIdentityCollision
    };
    private RemediationTraceRequest? _traceRequest;
    private List<RemediationPredicateTrace>? _predicateTraces;
    private List<RemediationRuntimeDiagnostic>? _programRuntimeDiagnostics;
    private bool _committed;
    private bool _disposed;

    internal RemediationSession(PdfDocument document, RemediationSessionConfiguration configuration)
    {
        _document = document;
        Configuration = configuration;
        _effectiveProfile = configuration.Profile;
        Structure = new StructuralBuilder();
    }

    /// <summary>Session configuration and commit-time accessibility setup options.</summary>
    public RemediationSessionConfiguration Configuration { get; }

    /// <summary>Structure builder owned by the remediation session.</summary>
    public StructuralBuilder Structure { get; }

    /// <summary>
    /// Adds the prescriptive program selected for this document. The program is compiled before it
    /// is selected as the session's native execution plan; only one program may be selected.
    /// </summary>
    public RemediationSession Use(RemediationProgram program)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(program);
        return Use(RemediationProgramCompiler.Compile(program));
    }

    /// <summary>Adds an already compiled prescriptive program without compiling it again.</summary>
    public RemediationSession Use(CompiledRemediationProgram program)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(program);
        if (_program != null)
            throw new InvalidOperationException("A remediation session accepts exactly one prescriptive program.");
        if (!program.IsValid)
            throw new ArgumentException(string.Join(Environment.NewLine, program.Errors), nameof(program));
        _program = program;
        _effectiveProfile = program.Program.Template.Profile;
        return this;
    }

    /// <summary>Compiles a prescriptive program without parsing pages or mutating the document.</summary>
    public ValidationReport Validate(RemediationProgram program)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(program);
        var compiled = RemediationProgramCompiler.Compile(program);
        return new ValidationReport(compiled.Errors);
    }

    /// <summary>Adds a justified diagnostic suppression.</summary>
    public RemediationSession Suppress(DiagnosticCode code, string scope, string reason)
    {
        ThrowIfDisposed();
        _suppressions.Add(new DiagnosticSuppression(code, scope, reason));
        return this;
    }

    /// <summary>Evaluates the selected program without mutating the document.</summary>
    public RemediationReport DryRun()
    {
        ThrowIfDisposed();
        if (_program == null)
            throw new InvalidOperationException("Select a remediation program before evaluating the session.");
        return EvaluateProgram(apply: false);
    }

    /// <summary>Evaluates configured rule sets and retains selected predicate rejection traces.</summary>
    public RemediationReport DryRun(RemediationTraceRequest traceRequest)
    {
        ArgumentNullException.ThrowIfNull(traceRequest);
        ThrowIfDisposed();
        _traceRequest = traceRequest;
        _predicateTraces = new List<RemediationPredicateTrace>();
        try
        {
            if (_program == null)
                throw new InvalidOperationException("Select a remediation program before evaluating the session.");
            return EvaluateProgram(apply: false);
        }
        finally
        {
            _traceRequest = null;
            _predicateTraces = null;
        }
    }

    /// <summary>Applies the selected program and accessibility setup to the document.</summary>
    public RemediationReport Commit()
    {
        ThrowIfDisposed();
        if (_committed)
        {
            throw new InvalidOperationException("Remediation session has already been committed.");
        }
        if (Configuration.RunMode == RemediationRunMode.Authoring)
        {
            throw new InvalidOperationException("Authoring remediation sessions are dry-run only and cannot commit.");
        }

        if (_program == null)
            throw new InvalidOperationException("Select a remediation program before committing the session.");
        return CommitProgram();
    }

    private IReadOnlyList<Rule> ComposeRules(IEnumerable<Rule> rules)
    {
        return _ruleSets
            .SelectMany(x => x.Rules)
            .Concat(rules ?? throw new ArgumentNullException(nameof(rules)))
            .ToList();
    }

    private IReadOnlyDictionary<string, RemediationAnchor> BuildAnchorLookup(IEnumerable<RuleSet> ruleSets)
    {
        var anchors = new Dictionary<string, RemediationAnchor>(StringComparer.Ordinal);
        foreach (var anchor in ruleSets.SelectMany(x => x.Anchors))
        {
            anchors[anchor.Id] = anchor; // Last one wins if there are duplicates for now
        }
        return anchors;
    }

    private IReadOnlyDictionary<string, TolerancedZone> BuildTolerancedZoneLookup(IEnumerable<RuleSet> ruleSets)
    {
        var zones = new Dictionary<string, TolerancedZone>(StringComparer.Ordinal);
        foreach (var zone in ruleSets.SelectMany(x => x.TolerancedZones))
        {
            zones[zone.Id] = zone;
        }

        return zones;
    }

    private IReadOnlyDictionary<string, FlowRegion> BuildFlowRegionLookup(IEnumerable<RuleSet> ruleSets)
    {
        var regions = new Dictionary<string, FlowRegion>(StringComparer.Ordinal);
        foreach (var region in ruleSets.SelectMany(x => x.FlowRegions))
        {
            regions[region.Id] = region;
        }

        return regions;
    }

    private RemediationReport Evaluate(IEnumerable<Rule> rules, bool apply)
    {
        var ruleList = rules.ToList();
        var evaluations = new RuleEvaluationAccumulator(ruleList, _document.Pages.Count);
        var anchors = BuildAnchorLookup(_ruleSets);
        var tolerancedZones = BuildTolerancedZoneLookup(_ruleSets);
        var flowRegions = BuildFlowRegionLookup(_ruleSets);
        var validation = ValidateRules(
            ruleList,
            anchors,
            tolerancedZones,
            flowRegions,
            ValidateRuleSetDeclarations(_ruleSets));
        var diagnostics = validation.Errors.ToList();
        if (Configuration.DefaultConfidence is < 0 or > 1)
        {
            diagnostics.Add(
                $"Session DefaultConfidence {Configuration.DefaultConfidence} is outside [0,1].");
        }
        if (diagnostics.Count > 0)
        {
            return new RemediationReport(
                false,
                false,
                diagnostics: diagnostics,
                suppressions: _suppressions,
                ruleEvaluations: evaluations.Build(Array.Empty<RemediationClaim>(), Array.Empty<RemediationClaim>()));
        }

        var allClaims = new List<RemediationClaim>();
        var skippedClaims = new List<RemediationClaim>();
        var warnings = new List<string>();
        var autoArtifacts = new List<RemediationAutoArtifactOutcome>();
        var unaccountedContent = new List<RemediationUnaccountedContent>();
        var pageStates = BuildPageStates();
        EvaluateDocument(
            pageStates,
            ruleList,
            allClaims,
            skippedClaims,
            diagnostics,
            warnings,
            evaluations,
            autoArtifacts,
            unaccountedContent,
            apply);

        PrepareAnnotationAdoptions(pageStates, allClaims, diagnostics);
        var annotationInventory = BuildAnnotationInventory(pageStates, allClaims, apply);
        if (Configuration.StrictConformance)
        {
            foreach (var annotation in annotationInventory.Where(x => x.BlocksConformance))
            {
                ReportDiagnostic(
                    DiagnosticCode.UnmodeledAnnotation,
                    $"Page{annotation.PageIndex + 1}",
                    $"Input page {annotation.PageIndex + 1} contains a visible {annotation.Subtype} annotation " +
                    "that remains unmodeled after rule evaluation.",
                    diagnostics);
            }
        }

        var ruleEvaluations = evaluations.Build(allClaims, skippedClaims);
        CheckRuleCardinalities(ruleList, ruleEvaluations, diagnostics);
        var assertionOutcomes = EvaluateAssertions(allClaims, diagnostics);
        var slots = ruleList
            .GroupBy(x => (x.RuleSetId, x.Id))
            .ToDictionary(x => x.Key, x => x.Last().Slot);
        var actions = ruleList
            .GroupBy(x => (x.RuleSetId, x.Id))
            .ToDictionary(x => x.Key, x => x.Last().Action);
        var structuralTemplate = _ruleSets.Select(x => x.StructuralTemplate).FirstOrDefault(x => x != null);
        var assemblySourceClaims = allClaims
            .Where(x => !x.RuleId.StartsWith("__template__:", StringComparison.Ordinal))
            .ToArray();
        var sourceSemanticTree = RemediationSemanticTree.FromClaims(assemblySourceClaims, slots, actions);
        var claimsById = allClaims
            .GroupBy(x => x.ClaimId)
            .ToDictionary(x => x.Key, x => x.First());
        var assemblyPlan = structuralTemplate?.Mode == RemediationStructuralTemplateMode.Prescriptive
            ? PrescriptiveTemplateAssemblyPlan.Build(structuralTemplate, sourceSemanticTree, claimsById)
            : null;
        var semanticTree = assemblyPlan?.ProjectSemanticTree() ?? sourceSemanticTree;
        var templateDifferences = EvaluateStructuralTemplate(semanticTree, diagnostics).ToList();
        templateDifferences.AddRange(
            EvaluateArtifactInventory(pageStates, ruleList, allClaims, autoArtifacts, diagnostics));

        if (apply && !HasUnsuppressedDiagnostics(diagnostics))
        {
            ValidatePlan(pageStates, diagnostics);
        }

        if (apply && !HasUnsuppressedDiagnostics(diagnostics))
        {
            ApplyPlan(pageStates, diagnostics, assemblyPlan);
            if (_ruleSets.Any(x => x.StructuralTemplate != null))
            {
                var actualTree = RemediationSemanticTree.FromStructure(
                    Structure.GetRoot(), allClaims, slots);
                var plannedShape = SemanticShape(semanticTree);
                var actualShape = SemanticShape(actualTree);
                if (!string.Equals(plannedShape, actualShape, StringComparison.Ordinal))
                {
                    var owner = _ruleSets.FirstOrDefault(x =>
                        ReferenceEquals(x.StructuralTemplate, structuralTemplate));
                    if (owner != null)
                    {
                        var difference = new RemediationTemplateDifference(
                            RemediationTemplateDifferenceKind.MaterializationDivergence,
                            DiagnosticCode.TemplateMaterializationDivergence,
                            owner.Id, null, "Document", "Document", plannedShape, actualShape,
                            Array.Empty<int>(), null, false);
                        templateDifferences.Add(ReportTemplateDifference(difference, diagnostics));
                    }
                }
                var plannedKeys = templateDifferences.Select(TemplateDifferenceKey)
                    .ToHashSet(StringComparer.Ordinal);
                foreach (var actualDifference in MatchStructuralTemplate(actualTree))
                {
                    if (plannedKeys.Add(TemplateDifferenceKey(actualDifference)))
                    {
                        templateDifferences.Add(ReportTemplateDifference(actualDifference, diagnostics));
                    }
                }
            }
            RunDiagnostics(pageStates, diagnostics);
        }

        return new RemediationReport(
            false,
            false,
            allClaims,
            skippedClaims,
            diagnostics,
            _suppressions,
            ruleEvaluations,
            autoArtifacts,
            _predicateTraces?.ToList(),
            assertionOutcomes,
            semanticTree,
            unaccountedContent,
            annotationInventory,
            warnings,
            templateDifferences,
            assemblyPlan?.Items);
    }

    /// <summary>
    /// Grades every produced artifact against the declared inventory. Runs before the apply gates, so
    /// an unsuppressed difference stops the commit rather than hiding content silently.
    /// </summary>
    private IReadOnlyList<RemediationTemplateDifference> EvaluateArtifactInventory(
        IReadOnlyList<PageRemediationState> pageStates,
        IReadOnlyList<Rule> rules,
        IReadOnlyList<RemediationClaim> claims,
        IReadOnlyList<RemediationAutoArtifactOutcome> autoArtifacts,
        List<string> diagnostics)
    {
        if (ArtifactInventory.Count == 0)
        {
            return Array.Empty<RemediationTemplateDifference>();
        }

        var rulesById = rules.GroupBy(x => x.Id, StringComparer.Ordinal)
            .ToDictionary(x => x.Key, x => x.Last(), StringComparer.Ordinal);
        var records = new List<RemediationArtifactRecord>();
        foreach (var claim in claims.Where(x => x.Action is ArtifactRemediationAction))
        {
            var artifact = (ArtifactRemediationAction)claim.Action!;
            var subtype = artifact.Subtype;
            var rule = rulesById.GetValueOrDefault(claim.RuleId);
            foreach (var page in claim.Candidates.Where(x => x.PageIndex >= 0).GroupBy(x => x.PageIndex))
            {
                records.Add(new RemediationArtifactRecord(
                    page.Key,
                    subtype,
                    UnionBounds(page.Select(x => x.RelativeBoundingBox)),
                    claim.RuleId,
                    rule?.RuleSetId,
                    rule?.Artifact,
                    artifact.SemanticSubtype));
            }
        }

        records.AddRange(autoArtifacts.Select(x =>
            new RemediationArtifactRecord(x.PageIndex, null, x.RelativeBoundingBox)));

        var zonesByPage = pageStates.ToDictionary(x => x.PageIndex, x => x.ArtifactZones);
        var differences = RemediationArtifactInventoryMatcher.Match(
            ArtifactInventory, records, _document.Pages.Count, zonesByPage);
        return differences.Select(x => ReportArtifactDifference(x, diagnostics)).ToList();
    }

    private RemediationTemplateDifference ReportArtifactDifference(
        RemediationTemplateDifference difference,
        List<string> diagnostics)
    {
        var page = difference.PageIndexes.Count > 0 ? $"Page{difference.PageIndexes[0] + 1}" : "*";
        var scope = $"Artifact:{difference.SlotId ?? "Undeclared"}:{page}";
        var suppressed = Configuration.DiagnosticStrictness != RemediationDiagnosticStrictness.Strict &&
            _suppressions.Any(x => x.Code == difference.DiagnosticCode &&
                (x.Scope == "*" || x.Scope == scope));
        var message = difference.Kind == RemediationTemplateDifferenceKind.UndeclaredArtifact
            ? $"Artifact at '{difference.ActualPath}' ({difference.ActualValue}) matches no declared artifact inventory item."
            : $"Artifact '{difference.SlotId}' expected {difference.ExpectedValue} on " +
              $"{page.ToLowerInvariant()}, found {difference.ActualValue}.";
        ReportDiagnostic(difference.DiagnosticCode, scope, message, diagnostics);
        return difference with { Suppressed = suppressed };
    }

    private static PdfRect<double> UnionBounds(IEnumerable<PdfRect<double>> rects)
    {
        using var enumerator = rects.GetEnumerator();
        if (!enumerator.MoveNext())
        {
            return new PdfRect<double>(0, 0, 0, 0);
        }

        var result = enumerator.Current;
        while (enumerator.MoveNext())
        {
            var rect = enumerator.Current;
            result = new PdfRect<double>(
                Math.Min(result.LLx, rect.LLx),
                Math.Min(result.LLy, rect.LLy),
                Math.Max(result.URx, rect.URx),
                Math.Max(result.URy, rect.URy));
        }

        return result;
    }

    private IReadOnlyList<RemediationTemplateDifference> EvaluateStructuralTemplate(
        RemediationSemanticTree semanticTree,
        List<string> diagnostics)
    {
        var results = new List<RemediationTemplateDifference>();
        foreach (var difference in MatchStructuralTemplate(semanticTree))
        {
            results.Add(ReportTemplateDifference(difference, diagnostics));
        }
        return results;
    }

    private IReadOnlyList<RemediationTemplateDifference> MatchStructuralTemplate(
        RemediationSemanticTree semanticTree)
    {
        var owner = _ruleSets.SingleOrDefault(x => x.StructuralTemplate != null);
        if (owner?.StructuralTemplate == null)
        {
            return Array.Empty<RemediationTemplateDifference>();
        }
        var boundSlots = owner.Rules.Where(x => x.Slot != null).Select(x => x.Slot!)
            .ToHashSet(StringComparer.Ordinal);
        return RemediationStructuralTemplateMatcher.Match(
            owner.Id, owner.StructuralTemplate, semanticTree, _document.Pages.Count, boundSlots);
    }

    private static string TemplateDifferenceKey(RemediationTemplateDifference difference) =>
        string.Join("\0",
            difference.Kind,
            difference.RuleSetId,
            difference.SlotId,
            difference.ExpectedPath,
            difference.ActualPath,
            difference.ExpectedValue,
            difference.ActualValue,
            string.Join(",", difference.PageIndexes),
            difference.RuleId);

    private RemediationTemplateDifference ReportTemplateDifference(
        RemediationTemplateDifference difference,
        List<string> diagnostics)
    {
        var path = difference.ExpectedPath ?? difference.ActualPath ?? "Document";
        var scope = $"RuleSet:{difference.RuleSetId}:Template:{path}";
        var suppressed = Configuration.DiagnosticStrictness != RemediationDiagnosticStrictness.Strict &&
            _suppressions.Any(x => x.Code == difference.DiagnosticCode &&
                (x.Scope == "*" || x.Scope == scope));
        ReportDiagnostic(
            difference.DiagnosticCode,
            scope,
            $"Template mismatch at '{path}': expected {difference.ExpectedValue ?? difference.ExpectedPath ?? "<none>"}, " +
            $"actual {difference.ActualValue ?? difference.ActualPath ?? "<none>"}.",
            diagnostics);
        return difference with { Suppressed = suppressed };
    }

    private static string SemanticShape(RemediationSemanticTree tree)
    {
        var builder = new StringBuilder("Document");
        void Append(RemediationSemanticNode node)
        {
            builder.Append('/').Append(node.Tag);
            if (node.SlotId != null) builder.Append('#').Append(node.SlotId);
            builder.Append('(');
            foreach (var child in node.Children) Append(child);
            builder.Append(')');
        }
        foreach (var root in tree.Roots) Append(root);
        return builder.ToString();
    }

    private void PrepareAnnotationAdoptions(
        IReadOnlyList<PageRemediationState> pageStates,
        IReadOnlyList<RemediationClaim> claims,
        List<string> diagnostics)
    {
        var ordinaryClaims = claims.Where(x => x.Status == ClaimStatus.Applied &&
            x.Action is not AdoptAnnotationRemediationAction).ToArray();
        foreach (var claim in claims.Where(x => x.Status == ClaimStatus.Applied &&
                     x.Action is AdoptAnnotationRemediationAction))
        {
            var action = (AdoptAnnotationRemediationAction)claim.Action!;
            var annotation = claim.Candidates.OfType<AnnotationRemediationCandidate>().SingleOrDefault();
            if (annotation == null) continue;
            var expectedTag = RequiredAnnotationTag(annotation.Subtype);
            var producedTag = claim.ProducedTag;
            if (!string.Equals(producedTag, expectedTag, StringComparison.Ordinal))
            {
                ReportDiagnostic(
                    DiagnosticCode.AnnotationAdoptionTargetMissing,
                    $"Page{annotation.PageIndex + 1}",
                    $"Rule '{claim.RuleId}' binds {annotation.Subtype} annotation '{annotation.CandidateId}' to incompatible template tag '{producedTag}'; expected '{expectedTag}'.",
                    diagnostics);
                continue;
            }
            if (string.Equals(expectedTag, "Link", StringComparison.Ordinal) &&
                string.IsNullOrWhiteSpace(annotation.Contents) &&
                string.IsNullOrWhiteSpace(action.AccessibleDescription))
            {
                ReportDiagnostic(
                    DiagnosticCode.AnnotationAdoptionTargetMissing,
                    $"Page{annotation.PageIndex + 1}",
                    $"Rule '{claim.RuleId}' cannot adopt Link annotation '{annotation.CandidateId}' without non-empty /Contents or an accessible description.",
                    diagnostics);
            }

            if (action.Into != null)
            {
                var context = new ClaimPredicateEvaluationContext(
                    ordinaryClaims,
                    PageBox: pageStates[annotation.PageIndex].StructuredText.RelativePageBox,
                    Configuration: Configuration,
                    Diagnostics: diagnostics);
                var matches = ordinaryClaims
                    .Where(x => x.PageIndexes.Contains(annotation.PageIndex))
                    .Where(x => action.Into.Evaluate(context, x).IsMatch)
                    .Where(x => string.Equals(x.ProducedTag, expectedTag, StringComparison.Ordinal))
                    .Where(x => !annotation.HasUsableGeometry ||
                        x.BoundsByPage.TryGetValue(annotation.PageIndex, out var bounds) &&
                        bounds.Intersects(annotation.BoundingBox))
                    .ToArray();
                if (matches.Length == 0)
                {
                    ReportDiagnostic(
                        DiagnosticCode.AnnotationAdoptionTargetMissing,
                        $"Page{annotation.PageIndex + 1}",
                        $"Rule '{claim.RuleId}' found no intersecting {expectedTag} claim for annotation '{annotation.CandidateId}'.",
                        diagnostics);
                }
                else if (matches.Length > 1)
                {
                    ReportDiagnostic(
                        DiagnosticCode.AnnotationAdoptionAmbiguous,
                        $"Page{annotation.PageIndex + 1}",
                        $"Rule '{claim.RuleId}' found {matches.Length} intersecting {expectedTag} claims for annotation '{annotation.CandidateId}'.",
                        diagnostics);
                }
                else
                {
                    claim.AnnotationIntoClaim = matches[0];
                }
            }

            if (action.DestinationTarget != null)
            {
                var context = new ClaimPredicateEvaluationContext(ordinaryClaims, Configuration: Configuration, Diagnostics: diagnostics);
                var targets = ordinaryClaims.Where(x => action.DestinationTarget.Evaluate(context, x).IsMatch).ToArray();
                if (targets.Length == 0)
                {
                    ReportDiagnostic(
                        DiagnosticCode.AnnotationAdoptionTargetMissing,
                        $"Page{annotation.PageIndex + 1}",
                        $"Rule '{claim.RuleId}' found no structure destination target for annotation '{annotation.CandidateId}'.",
                        diagnostics);
                }
                else if (targets.Length > 1)
                {
                    ReportDiagnostic(
                        DiagnosticCode.AnnotationAdoptionAmbiguous,
                        $"Page{annotation.PageIndex + 1}",
                        $"Rule '{claim.RuleId}' found {targets.Length} structure destination targets for annotation '{annotation.CandidateId}'.",
                        diagnostics);
                }
                else
                {
                    claim.AnnotationDestinationClaim = targets[0];
                }
            }
        }
    }

    private static string RequiredAnnotationTag(string subtype) => subtype switch
    {
        "Link" => "Link",
        "Widget" => "Form",
        _ => "Annot"
    };

    private IReadOnlyList<RemediationAnnotationInventoryItem> BuildAnnotationInventory(
        IReadOnlyList<PageRemediationState> pageStates,
        IReadOnlyList<RemediationClaim> claims,
        bool applied)
    {
        var adopted = claims
            .Where(x => x.Status == ClaimStatus.Applied && x.Action is AdoptAnnotationRemediationAction)
            .SelectMany(x => x.Candidates.OfType<AnnotationRemediationCandidate>().Select(candidate => (candidate.Annotation, Claim: x)))
            .ToDictionary(x => x.Annotation, x => x.Claim, ReferenceEqualityComparer.Instance);
        var inventory = new List<RemediationAnnotationInventoryItem>();
        foreach (var candidate in pageStates.SelectMany(x => x.AnnotationCandidates))
        {
            var special = string.Equals(candidate.Subtype, "Popup", StringComparison.Ordinal) ||
                          string.Equals(candidate.Subtype, "PrinterMark", StringComparison.Ordinal);
            var exempt = candidate.Hidden || candidate.OffPage || special;
            adopted.TryGetValue(candidate.Annotation, out var claim);
            var disposition = claim != null
                ? applied ? RemediationAnnotationDisposition.Applied : RemediationAnnotationDisposition.Planned
                : exempt ? RemediationAnnotationDisposition.Exempt : RemediationAnnotationDisposition.Unmodeled;
            var reason = claim != null
                ? $"Adopted by rule '{claim.RuleId}' as {claim.ProducedTag}."
                : candidate.Hidden
                    ? "Hidden annotation."
                    : candidate.OffPage
                        ? "Annotation lies wholly outside the crop box."
                        : special
                            ? $"{candidate.Subtype} is handled by its specialized accessibility contract."
                            : "Visible annotation was not adopted by a remediation rule.";
            inventory.Add(new RemediationAnnotationInventoryItem(
                candidate.PageIndex,
                candidate.Subtype,
                candidate.Bounds,
                candidate.Hidden,
                candidate.OffPage,
                candidate.HasStructParent || applied && claim != null,
                claim == null && !exempt,
                reason)
            {
                CandidateId = candidate.CandidateId,
                Disposition = disposition,
                RuleId = claim?.RuleId,
                ProducedTag = claim?.ProducedTag,
                DestinationKind = candidate.DestinationKind
            });
        }
        return inventory;
    }

    private IReadOnlyList<RemediationAssertionOutcome> EvaluateAssertions(
        IReadOnlyList<RemediationClaim> claims,
        List<string> diagnostics)
    {
        var outcomes = new List<RemediationAssertionOutcome>();
        foreach (var ruleSet in _ruleSets)
        {
            foreach (var assertion in ruleSet.Assertions)
            {
                var pages = assertion.Scope == SemanticAssertionScope.Document
                    ? new int?[] { null }
                    : Enumerable.Range(0, _document.Pages.Count)
                        .Where(x => (assertion.Pages ?? PageSelector.Every).Includes(x, _document.Pages.Count))
                        .Select(x => (int?)x);

                foreach (var page in pages)
                {
                    var selected = claims.Where(x =>
                        x.Status == ClaimStatus.Applied &&
                        (page == null || ClaimAppearsOnPage(x, page.Value)) &&
                        (page != null || (assertion.Pages ?? PageSelector.Every)
                            .Includes(x.PageIndex, _document.Pages.Count))).ToList();

                    switch (assertion)
                    {
                        case RuleOutputCountAssertion output:
                            AddCountOutcome(ruleSet.Id, output.Id, page,
                                output.Expected,
                                selected.Count(x => x.RuleId == output.RuleId &&
                                    (output.ProducedTag == null || x.ProducedTag == output.ProducedTag)),
                                output.RuleId, output.ProducedTag);
                            break;
                        case StructureElementCountAssertion structure:
                            AddCountOutcome(ruleSet.Id, structure.Id, page,
                                structure.Expected,
                                selected.Count(x => x.ProducedTag == structure.Tag),
                                null, structure.Tag);
                            break;
                        case SlotElementCountAssertion slot:
                            AddCountOutcome(ruleSet.Id, slot.Id, page,
                                slot.Expected,
                                selected.Count(x => string.Equals(x.SlotId, slot.Slot.Path[1..], StringComparison.Ordinal)),
                                null, null, slot.Slot);
                            break;
                        case ParentChildShapeAssertion shape:
                            var parents = selected.Where(x => x.ProducedTag == shape.ParentTag).ToList();
                            if (parents.Count == 0)
                            {
                                AddOutcome(ruleSet.Id, shape.Id, page,
                                    $"at least one '{shape.ParentTag}' parent with {shape.ExpectedChildren.Description} child(ren)",
                                    "no matching parents",
                                    false, null, shape.ParentTag);
                            }
                            foreach (var parent in parents)
                            {
                                var children = parent.RelatedClaims;
                                var tagsValid = children.All(x => shape.AllowedChildTags.Contains(x.ProducedTag));
                                var countValid = shape.ExpectedChildren.Accepts(children.Count);
                                AddOutcome(ruleSet.Id, shape.Id, page,
                                    $"{shape.ExpectedChildren.Description} child(ren), tags [{string.Join(", ", shape.AllowedChildTags)}]",
                                    $"{children.Count} child(ren), tags [{string.Join(", ", children.Select(x => x.ProducedTag))}]",
                                    tagsValid && countValid, parent.RuleId, shape.ParentTag);
                            }
                            break;
                    }
                }
            }
        }
        return outcomes;

        void AddCountOutcome(string ruleSetId, string id, int? page, AssertionCount expected,
            int observed, string? ruleId, string? tag, SlotRef? slot = null) =>
            AddOutcome(ruleSetId, id, page, expected.Description, observed.ToString(),
                expected.Accepts(observed), ruleId, tag, slot);

        void AddOutcome(string ruleSetId, string id, int? page, string expected,
            string observed, bool passed, string? ruleId, string? tag, SlotRef? slot = null)
        {
            outcomes.Add(new RemediationAssertionOutcome(
                ruleSetId, id, page, expected, observed, passed, ruleId, tag)
                { ProgramSlot = slot });
            if (!passed)
            {
                var location = page is { } p ? $" on page {p + 1}" : string.Empty;
                ReportDiagnostic(DiagnosticCode.SemanticAssertionFailed,
                    $"RuleSet:{ruleSetId}:Assertion:{id}" + (page is { } pi ? $":Page{pi + 1}" : string.Empty),
                    $"Rule set '{ruleSetId}' assertion '{id}'{location} expected {expected}, but observed {observed}.",
                    diagnostics);
            }
        }
    }

    private static bool ClaimAppearsOnPage(RemediationClaim claim, int pageIndex) =>
        ClaimAppearsOnPage(claim, pageIndex, new HashSet<ClaimId>());

    private static bool ClaimAppearsOnPage(
        RemediationClaim claim,
        int pageIndex,
        HashSet<ClaimId> visited)
    {
        if (!visited.Add(claim.ClaimId))
        {
            return false;
        }
        return claim.PageIndex == pageIndex ||
            claim.RelatedClaims.Any(x => ClaimAppearsOnPage(x, pageIndex, visited));
    }

    private void CheckRuleCardinalities(
        IReadOnlyList<Rule> rules,
        IReadOnlyList<RuleEvaluationSummary> summaries,
        List<string> diagnostics)
    {
        var byRuleId = summaries.ToDictionary(x => x.RuleId, StringComparer.Ordinal);
        foreach (var rule in rules)
        {
            if (rule.Cardinality is not { } cardinality ||
                !byRuleId.TryGetValue(rule.Id, out var summary))
            {
                continue;
            }

            var origin = rule.RuleSetId == null
                ? $"Rule '{rule.Id}'"
                : $"Rule set '{rule.RuleSetId}', rule '{rule.Id}'";

            if (cardinality.Scope == RuleCardinalityScope.Document)
            {
                var observed = summary.Total.InputsMatched;
                if (!cardinality.Accepts(observed))
                {
                    ReportDiagnostic(
                        DiagnosticCode.RuleCardinalityMismatch,
                        $"Rule:{rule.Id}",
                        $"{origin} expected {cardinality.ExpectedDescription} matched input(s) across its selected pages, but observed {observed}.",
                        diagnostics);
                }

                continue;
            }

            if (summary.Pages.Count == 0)
            {
                if (cardinality.MinMatches > 0)
                {
                    ReportDiagnostic(
                        DiagnosticCode.RuleCardinalityMismatch,
                        $"Rule:{rule.Id}",
                        $"{origin} expected {cardinality.ExpectedDescription} matched input(s) per page, but its page selector selected no existing pages.",
                        diagnostics);
                }

                continue;
            }

            foreach (var page in summary.Pages)
            {
                var observed = page.Counts.InputsMatched;
                if (cardinality.Accepts(observed))
                {
                    continue;
                }

                ReportDiagnostic(
                    DiagnosticCode.RuleCardinalityMismatch,
                    $"Rule:{rule.Id}:Page{page.PageIndex + 1}",
                    $"{origin} expected {cardinality.ExpectedDescription} matched input(s) on page {page.PageIndex + 1}, but observed {observed}.",
                    diagnostics);
            }
        }
    }

    private static ValidationReport ValidateRules(
        IReadOnlyList<Rule> rules,
        IReadOnlyDictionary<string, RemediationAnchor>? anchors = null,
        IReadOnlyDictionary<string, TolerancedZone>? tolerancedZones = null,
        IReadOnlyDictionary<string, FlowRegion>? flowRegions = null,
        IEnumerable<string>? declarationErrors = null)
    {
        var errors = declarationErrors?.ToList() ?? new List<string>();
        var warnings = new List<string>();
        var rulesById = new Dictionary<string, Rule>(StringComparer.Ordinal);
        anchors ??= new Dictionary<string, RemediationAnchor>(StringComparer.Ordinal);
        tolerancedZones ??= new Dictionary<string, TolerancedZone>(StringComparer.Ordinal);
        flowRegions ??= new Dictionary<string, FlowRegion>(StringComparer.Ordinal);

        foreach (var group in rules.GroupBy(x => x.Id, StringComparer.Ordinal).Where(x => x.Count() > 1))
        {
            var origins = group
                .Select(x => x.RuleSetId == null ? x.Id : $"{x.RuleSetId}:{x.Id}")
                .ToArray();
            errors.Add($"Rule id '{group.Key}' is duplicated across the composed rule set: {string.Join(", ", origins)}.");
        }

        foreach (var rule in rules)
        {
            rulesById.TryAdd(rule.Id, rule);
            errors.AddRange(rule.ValidateShape().Select(x => $"Rule '{rule.Id}': {x}"));

            if (rule.MinConfidence is < 0 or > 1)
            {
                errors.Add($"Rule '{rule.Id}' has MinConfidence outside [0,1].");
            }

            if (rule.Action is CustomRemediationAction)
            {
                warnings.Add($"Rule '{rule.Id}' uses a custom action and is only partially pre-flight validated.");
            }
        }

        foreach (var rule in rules)
        {
            ValidatePredicate(rule, rule.Predicate, rulesById, anchors, tolerancedZones, flowRegions, errors, warnings);
            ValidateActionClaimPredicates(rule, rulesById, anchors, tolerancedZones, flowRegions, errors);
        }

        return new ValidationReport(errors, warnings);
    }

    private static IReadOnlyList<string> ValidateRuleSetDeclarations(IReadOnlyList<RuleSet> ruleSets)
    {
        var errors = new List<string>();
        errors.AddRange(RemediationStructuralTemplateValidator.Validate(ruleSets));
        errors.AddRange(RemediationArtifactInventoryValidator.Validate(ruleSets));
        foreach (var group in ruleSets.SelectMany(x => x.Anchors).GroupBy(x => x.Id, StringComparer.Ordinal).Where(x => x.Count() > 1))
        {
            errors.Add($"Anchor id '{group.Key}' is duplicated across the composed rule set.");
        }

        foreach (var group in ruleSets.SelectMany(x => x.TolerancedZones).GroupBy(x => x.Id, StringComparer.Ordinal).Where(x => x.Count() > 1))
        {
            errors.Add($"Toleranced zone id '{group.Key}' is duplicated across the composed rule set.");
        }

        foreach (var group in ruleSets.SelectMany(x => x.FlowRegions).GroupBy(x => x.Id, StringComparer.Ordinal).Where(x => x.Count() > 1))
        {
            errors.Add($"Flow region id '{group.Key}' is duplicated across the composed rule set.");
        }

        foreach (var ruleSet in ruleSets)
        {
            foreach (var duplicate in ruleSet.Assertions.GroupBy(x => x.Id, StringComparer.Ordinal).Where(x => x.Count() > 1))
            {
                errors.Add($"Assertion id '{duplicate.Key}' is duplicated in rule set '{ruleSet.Id}'.");
            }

            var knownRules = ruleSets.SelectMany(x => x.Rules).Select(x => x.Id).ToHashSet(StringComparer.Ordinal);
            foreach (var assertion in ruleSet.Assertions)
            {
                if (string.IsNullOrWhiteSpace(assertion.Id))
                {
                    errors.Add($"Rule set '{ruleSet.Id}' has an assertion without an id.");
                }
                if (assertion.Expected.Min < 0 ||
                    assertion.Expected.Max is { } max && max < assertion.Expected.Min)
                {
                    errors.Add($"Assertion '{assertion.Id}' has an invalid expected count range.");
                }
                if (assertion is RuleOutputCountAssertion output && !knownRules.Contains(output.RuleId))
                {
                    errors.Add($"Assertion '{assertion.Id}' references unknown rule '{output.RuleId}'.");
                }
                if (assertion is StructureElementCountAssertion structure && string.IsNullOrWhiteSpace(structure.Tag))
                {
                    errors.Add($"Assertion '{assertion.Id}' requires a structure tag.");
                }
                if (assertion is ParentChildShapeAssertion shape &&
                    (string.IsNullOrWhiteSpace(shape.ParentTag) || shape.AllowedChildTags.Count == 0))
                {
                    errors.Add($"Assertion '{assertion.Id}' requires a parent tag and at least one allowed child tag.");
                }
                if (assertion is ParentChildShapeAssertion childShape &&
                    (childShape.ExpectedChildren.Min < 0 ||
                     childShape.ExpectedChildren.Max is { } childMax &&
                     childMax < childShape.ExpectedChildren.Min))
                {
                    errors.Add($"Assertion '{assertion.Id}' has an invalid expected child count range.");
                }
            }
        }

        foreach (var zone in ruleSets.SelectMany(x => x.TolerancedZones))
        {
            errors.AddRange(zone.Validate());
        }

        var rulesById = ruleSets.SelectMany(x => x.Rules)
            .GroupBy(x => x.Id, StringComparer.Ordinal)
            .Where(x => x.Count() == 1)
            .ToDictionary(x => x.Key, x => x.Single(), StringComparer.Ordinal);
        var anchors = ruleSets.SelectMany(x => x.Anchors)
            .GroupBy(x => x.Id, StringComparer.Ordinal)
            .Where(x => x.Count() == 1)
            .ToDictionary(x => x.Key, x => x.Single(), StringComparer.Ordinal);
        var zones = ruleSets.SelectMany(x => x.TolerancedZones)
            .GroupBy(x => x.Id, StringComparer.Ordinal)
            .Where(x => x.Count() == 1)
            .ToDictionary(x => x.Key, x => x.Single(), StringComparer.Ordinal);
        var flows = ruleSets.SelectMany(x => x.FlowRegions)
            .GroupBy(x => x.Id, StringComparer.Ordinal)
            .Where(x => x.Count() == 1)
            .ToDictionary(x => x.Key, x => x.Single(), StringComparer.Ordinal);

        foreach (var anchor in ruleSets.SelectMany(x => x.Anchors))
        {
            ValidateAnchorDeclaration(anchor, rulesById, anchors, zones, flows, errors);
        }

        foreach (var region in ruleSets.SelectMany(x => x.FlowRegions))
        {
            errors.AddRange(region.Validate());
            ValidateFlowBoundary(region, region.Start, "start", anchors, zones, flows, errors);
            ValidateFlowBoundary(region, region.End, "end", anchors, zones, flows, errors);
        }

        return errors;
    }

    private static void ValidatePredicate(
        Rule rule,
        RemediationPredicate predicate,
        IReadOnlyDictionary<string, Rule> rulesById,
        IReadOnlyDictionary<string, RemediationAnchor> anchors,
        IReadOnlyDictionary<string, TolerancedZone> tolerancedZones,
        IReadOnlyDictionary<string, FlowRegion> flowRegions,
        List<string> errors,
        List<string> warnings)
    {
        switch (predicate)
        {
            case CompositeRemediationPredicate composite:
                ValidatePredicate(rule, composite.Left, rulesById, anchors, tolerancedZones, flowRegions, errors, warnings);
                ValidatePredicate(rule, composite.Right, rulesById, anchors, tolerancedZones, flowRegions, errors, warnings);
                break;
            case NotRemediationPredicate not:
                ValidatePredicate(rule, not.Inner, rulesById, anchors, tolerancedZones, flowRegions, errors, warnings);
                break;
            case TextRemediationPredicate { Kind: TextPredicateKind.Matches } text:
                try
                {
                    _ = new Regex(text.Value);
                }
                catch (ArgumentException ex)
                {
                    errors.Add($"Rule '{rule.Id}' has invalid regex '{text.Value}': {ex.Message}");
                }
                break;
            case GeometryRemediationPredicate geometry:
                ValidateLayoutCoord(rule, geometry.Coord, rulesById, anchors, tolerancedZones, flowRegions, errors);
                break;
            case RelationalRemediationPredicate relational:
                ValidateRuleReference(rule, relational.RuleId, rulesById, errors, "relational predicate");
                break;
            case AnchorRelativeRemediationPredicate anchor:
                ValidateNamedAnchorReference(rule, anchor.AnchorId, anchors, errors, "anchor-relative predicate");
                if (anchor.AnchorId2 != null)
                {
                    ValidateNamedAnchorReference(rule, anchor.AnchorId2, anchors, errors, "anchor-relative predicate");
                }
                break;
            case TolerancedZoneRemediationPredicate zone:
                ValidateTolerancedZoneReference(rule, zone.ZoneId, tolerancedZones, errors, "zone predicate");
                break;
            case FlowRegionRemediationPredicate region:
                ValidateFlowRegionReference(rule, region.RegionId, flowRegions, errors, "flow-region predicate");
                break;
            case FlowOrderRemediationPredicate flowOrder:
                if (flowOrder.Kind == FlowOrderPredicateKind.FirstAfter)
                {
                    ValidateNamedAnchorReference(rule, flowOrder.Id, anchors, errors, "flow-order predicate");
                }
                else
                {
                    ValidateFlowRegionReference(rule, flowOrder.Id, flowRegions, errors, "flow-order predicate");
                }

                if (flowOrder.Kind == FlowOrderPredicateKind.NthIn && flowOrder.Index is null or < 0)
                {
                    errors.Add($"Rule '{rule.Id}' has invalid flow-order index '{flowOrder.Index}'.");
                }

                if (flowOrder.Where != null)
                {
                    ValidatePredicate(rule, flowOrder.Where, rulesById, anchors, tolerancedZones, flowRegions, errors, warnings);
                }
                break;
        }
    }

    private static void ValidateLayoutCoord(
        Rule rule,
        LayoutCoord coord,
        IReadOnlyDictionary<string, Rule> rulesById,
        IReadOnlyDictionary<string, RemediationAnchor> anchors,
        IReadOnlyDictionary<string, TolerancedZone> tolerancedZones,
        IReadOnlyDictionary<string, FlowRegion> flowRegions,
        List<string> errors)
    {
        switch (coord)
        {
            case AnchorLayoutCoord anchor:
                ValidateRuleReference(rule, anchor.RuleId, rulesById, errors, "anchor coordinate");
                break;
            case NamedAnchorLayoutCoord anchor:
                ValidateNamedAnchorReference(rule, anchor.AnchorId, anchors, errors, "named-anchor coordinate");
                break;
            case BetweenAnchorsLayoutCoord between:
                ValidateNamedAnchorReference(rule, between.AnchorA, anchors, errors, "between-anchors coordinate");
                ValidateNamedAnchorReference(rule, between.AnchorB, anchors, errors, "between-anchors coordinate");
                break;
            case TolerancedZoneLayoutCoord zone:
                ValidateTolerancedZoneReference(rule, zone.ZoneId, tolerancedZones, errors, "toleranced-zone coordinate");
                break;
            case FlowRegionLayoutCoord region:
                ValidateFlowRegionReference(rule, region.RegionId, flowRegions, errors, "flow-region coordinate");
                break;
        }
    }

    private static void ValidateRuleReference(
        Rule rule,
        string referencedRuleId,
        IReadOnlyDictionary<string, Rule> rulesById,
        List<string> errors,
        string referenceKind)
    {
        if (!rulesById.TryGetValue(referencedRuleId, out var referenced))
        {
            errors.Add($"Rule '{rule.Id}' has {referenceKind} reference to unknown rule '{referencedRuleId}'.");
            return;
        }

        if (referenced.Stage > rule.Stage)
        {
            errors.Add($"Rule '{rule.Id}' has {referenceKind} reference to later-stage rule '{referencedRuleId}' ({referenced.Stage} > {rule.Stage}).");
            return;
        }

        if (rule.Stage == Stage.Group && referenced.Stage == Stage.Group &&
            referenced.GroupPass >= rule.GroupPass)
        {
            errors.Add($"Rule '{rule.Id}' has {referenceKind} reference to same-or-higher-pass Group rule '{referencedRuleId}' ({referenced.GroupPass} >= {rule.GroupPass}).");
        }
    }

    private static void ValidateActionClaimPredicates(
        Rule rule,
        IReadOnlyDictionary<string, Rule> rulesById,
        IReadOnlyDictionary<string, RemediationAnchor> anchors,
        IReadOnlyDictionary<string, TolerancedZone> tolerancedZones,
        IReadOnlyDictionary<string, FlowRegion> flowRegions,
        List<string> errors)
    {
        switch (rule.Action)
        {
            case GroupRemediationAction group:
                ValidateClaimPredicate(rule, group.Over, rulesById, anchors, tolerancedZones, flowRegions, errors);
                break;
            case MergeRemediationAction merge:
                ValidateClaimPredicate(rule, merge.Over, rulesById, anchors, tolerancedZones, flowRegions, errors);
                break;
            case TableRemediationAction table:
                if (table.Over != null) ValidateClaimPredicate(rule, table.Over, rulesById, anchors, tolerancedZones, flowRegions, errors);
                if (table.HeaderSelector != null) ValidateClaimPredicate(rule, table.HeaderSelector, rulesById, anchors, tolerancedZones, flowRegions, errors);
                break;
            case StructureAttributeRemediationAction attr:
                ValidateClaimPredicate(rule, attr.Over, rulesById, anchors, tolerancedZones, flowRegions, errors);
                break;
            case ReorderSiblingsRemediationAction reorder:
                ValidateClaimPredicate(rule, reorder.Over, rulesById, anchors, tolerancedZones, flowRegions, errors);
                break;
            case StructureLinkRemediationAction link:
                ValidateClaimPredicate(rule, link.Source, rulesById, anchors, tolerancedZones, flowRegions, errors);
                ValidateClaimPredicate(rule, link.Target, rulesById, anchors, tolerancedZones, flowRegions, errors);
                break;
            case AdoptAnnotationRemediationAction adopt:
                if (adopt.Into != null)
                {
                    ValidateClaimPredicate(rule, adopt.Into, rulesById, anchors, tolerancedZones, flowRegions, errors);
                    ValidateAnnotationClaimReferences(rule, "Into", adopt.Into, rulesById, errors);
                }
                if (adopt.DestinationTarget != null)
                {
                    ValidateClaimPredicate(rule, adopt.DestinationTarget, rulesById, anchors, tolerancedZones, flowRegions, errors);
                    ValidateAnnotationClaimReferences(rule, "DestinationTarget", adopt.DestinationTarget, rulesById, errors);
                }
                break;
        }

        if (rule.Stage is Stage.Group or Stage.Refine && rule.Action is TagRemediationAction)
        {
            errors.Add($"Rule '{rule.Id}' uses a raw-content Tag action in {rule.Stage}; claim-consuming stages must select existing claims.");
        }
    }

    private static void ValidateAnnotationClaimReferences(
        Rule rule,
        string property,
        ClaimPredicate predicate,
        IReadOnlyDictionary<string, Rule> rulesById,
        List<string> errors)
    {
        foreach (var referencedRuleId in EnumerateClaimRuleReferences(predicate).Distinct(StringComparer.Ordinal))
        {
            if (rulesById.TryGetValue(referencedRuleId, out var referenced) && referenced.Stage != Stage.Classify)
            {
                errors.Add($"Rule '{rule.Id}' annotation {property} may reference Classify claims only; rule '{referencedRuleId}' is {referenced.Stage}.");
            }
        }
    }

    private static IEnumerable<string> EnumerateClaimRuleReferences(ClaimPredicate predicate)
    {
        switch (predicate)
        {
            case BuiltInClaimPredicate { Kind: ClaimPredicateKind.FromRule or ClaimPredicateKind.BeforeClaim or ClaimPredicateKind.AfterClaim, Value: not null } builtIn:
                yield return builtIn.Value;
                break;
            case CompositeClaimPredicate composite:
                foreach (var value in EnumerateClaimRuleReferences(composite.Left)) yield return value;
                foreach (var value in EnumerateClaimRuleReferences(composite.Right)) yield return value;
                break;
            case NotClaimPredicate not:
                foreach (var value in EnumerateClaimRuleReferences(not.Inner)) yield return value;
                break;
        }
    }

    private static void ValidateClaimPredicate(
        Rule rule,
        ClaimPredicate predicate,
        IReadOnlyDictionary<string, Rule> rulesById,
        IReadOnlyDictionary<string, RemediationAnchor> anchors,
        IReadOnlyDictionary<string, TolerancedZone> tolerancedZones,
        IReadOnlyDictionary<string, FlowRegion> flowRegions,
        List<string> errors)
    {
        switch (predicate)
        {
            case CompositeClaimPredicate composite:
                ValidateClaimPredicate(rule, composite.Left, rulesById, anchors, tolerancedZones, flowRegions, errors);
                ValidateClaimPredicate(rule, composite.Right, rulesById, anchors, tolerancedZones, flowRegions, errors);
                break;
            case NotClaimPredicate not:
                ValidateClaimPredicate(rule, not.Inner, rulesById, anchors, tolerancedZones, flowRegions, errors);
                break;
            case BuiltInClaimPredicate builtIn:
                if (builtIn.Kind is ClaimPredicateKind.FromRule or ClaimPredicateKind.BeforeClaim or ClaimPredicateKind.AfterClaim && builtIn.Value != null)
                {
                    ValidateRuleReference(rule, builtIn.Value, rulesById, errors, "claim-selector predicate");
                }

                if (builtIn.Kind == ClaimPredicateKind.Within)
                {
                    if (builtIn.LayoutCoord != null)
                    {
                        ValidateLayoutCoord(rule, builtIn.LayoutCoord, rulesById, anchors, tolerancedZones, flowRegions, errors);
                    }
                    else if (builtIn.Value != null)
                    {
                        ValidateNamedRegionReference(rule, builtIn.Value, anchors, tolerancedZones, flowRegions, errors, "claim Within predicate");
                    }
                }
                break;
        }
    }

    private static void ValidateAnchorDeclaration(
        RemediationAnchor anchor,
        IReadOnlyDictionary<string, Rule> rulesById,
        IReadOnlyDictionary<string, RemediationAnchor> anchors,
        IReadOnlyDictionary<string, TolerancedZone> tolerancedZones,
        IReadOnlyDictionary<string, FlowRegion> flowRegions,
        List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(anchor.Id))
        {
            errors.Add("Anchor id is required.");
        }

        if (anchor.Style is TextRemediationPredicate { Kind: TextPredicateKind.Matches } text)
        {
            try
            {
                _ = new Regex(text.Value);
            }
            catch (ArgumentException ex)
            {
                errors.Add($"Anchor '{anchor.Id}' has invalid style regex '{text.Value}': {ex.Message}");
            }
        }

        if (anchor is PriorClaimAnchor prior)
        {
            if (!rulesById.ContainsKey(prior.RuleId))
            {
                errors.Add($"Anchor '{anchor.Id}' references unknown prior-claim rule '{prior.RuleId}'.");
            }
        }

        if (anchor is PredicateAnchor predicate)
        {
            var fakeRule = new Rule($"anchor:{anchor.Id}", RemediationActions.Tag("P"));
            ValidatePredicate(fakeRule, predicate.Predicate, rulesById, anchors, tolerancedZones, flowRegions, errors, new List<string>());
            if (predicate.Granularities.Count == 0)
            {
                errors.Add($"Anchor '{anchor.Id}' must declare at least one granularity.");
            }

            if (predicate.Selection.Mode == AnchorSelectionMode.NthInReadingOrder && predicate.Selection.Index is null or < 0)
            {
                errors.Add($"Anchor '{anchor.Id}' has invalid nth selection index '{predicate.Selection.Index}'.");
            }

            if (predicate.Selection.Mode == AnchorSelectionMode.NearestToAnchor)
            {
                if (string.IsNullOrWhiteSpace(predicate.Selection.AnchorId))
                {
                    errors.Add($"Anchor '{anchor.Id}' nearest selection requires a reference anchor id.");
                }
                else if (!anchors.ContainsKey(predicate.Selection.AnchorId))
                {
                    errors.Add($"Anchor '{anchor.Id}' nearest selection references unknown anchor '{predicate.Selection.AnchorId}'.");
                }
            }
        }

        var selection = anchor switch
        {
            TextLabelAnchor label => label.Selection,
            TableHeaderAnchor header => header.Selection,
            RepeatedElementAnchor repeated => repeated.Selection,
            _ => null
        };
        if (selection?.Mode == AnchorSelectionMode.NthInReadingOrder && selection.Index is null or < 0)
        {
            errors.Add($"Anchor '{anchor.Id}' has invalid nth selection index '{selection.Index}'.");
        }
    }

    private static void ValidateFlowBoundary(
        FlowRegion region,
        FlowBoundary boundary,
        string edge,
        IReadOnlyDictionary<string, RemediationAnchor> anchors,
        IReadOnlyDictionary<string, TolerancedZone> tolerancedZones,
        IReadOnlyDictionary<string, FlowRegion> flowRegions,
        List<string> errors)
    {
        switch (boundary)
        {
            case AnchorFlowBoundary anchor:
                if (!anchors.ContainsKey(anchor.AnchorId))
                {
                    errors.Add($"Flow region '{region.Id}' {edge} references unknown anchor '{anchor.AnchorId}'.");
                }
                break;
            case ZoneFlowBoundary zone:
                if (!tolerancedZones.ContainsKey(zone.ZoneId))
                {
                    errors.Add($"Flow region '{region.Id}' {edge} references unknown toleranced zone '{zone.ZoneId}'.");
                }
                break;
            case PredicateFlowBoundary predicate:
                var fakeRule = new Rule($"flow-region:{region.Id}", RemediationActions.Tag("P"));
                ValidatePredicate(fakeRule, predicate.Predicate, new Dictionary<string, Rule>(StringComparer.Ordinal), anchors, tolerancedZones, flowRegions, errors, new List<string>());
                break;
        }
    }

    private static void ValidateNamedRegionReference(
        Rule rule,
        string id,
        IReadOnlyDictionary<string, RemediationAnchor> anchors,
        IReadOnlyDictionary<string, TolerancedZone> tolerancedZones,
        IReadOnlyDictionary<string, FlowRegion> flowRegions,
        List<string> errors,
        string referenceKind)
    {
        if (!anchors.ContainsKey(id) && !tolerancedZones.ContainsKey(id) && !flowRegions.ContainsKey(id))
        {
            errors.Add($"Rule '{rule.Id}' has {referenceKind} reference to unknown anchor, toleranced zone, or flow region '{id}'.");
        }
    }

    private static void ValidateNamedAnchorReference(
        Rule rule,
        string anchorId,
        IReadOnlyDictionary<string, RemediationAnchor> anchors,
        List<string> errors,
        string referenceKind)
    {
        if (!anchors.ContainsKey(anchorId))
        {
            errors.Add($"Rule '{rule.Id}' has {referenceKind} reference to unknown anchor '{anchorId}'.");
        }
    }

    private static void ValidateTolerancedZoneReference(
        Rule rule,
        string zoneId,
        IReadOnlyDictionary<string, TolerancedZone> tolerancedZones,
        List<string> errors,
        string referenceKind)
    {
        if (!tolerancedZones.ContainsKey(zoneId))
        {
            errors.Add($"Rule '{rule.Id}' has {referenceKind} reference to unknown toleranced zone '{zoneId}'.");
        }
    }

    private static void ValidateFlowRegionReference(
        Rule rule,
        string regionId,
        IReadOnlyDictionary<string, FlowRegion> flowRegions,
        List<string> errors,
        string referenceKind)
    {
        if (!flowRegions.ContainsKey(regionId))
        {
            errors.Add($"Rule '{rule.Id}' has {referenceKind} reference to unknown flow region '{regionId}'.");
        }
    }

    private IReadOnlyList<PageRemediationState> BuildPageStates()
    {
        var states = new List<PageRemediationState>();
        for (var i = 0; i < _document.Pages.Count; i++)
        {
            var page = _document.Pages[i];
            var structured = page.GetStructuredText();
            var content = page.GetContentNodes<double>();
            states.Add(new PageRemediationState(page, i, structured, content, content.ToList()));
        }

        BuildContentCandidateIndexes(states);
        return states;
    }

    private static IReadOnlyList<RemediationCandidate> SelectCandidates(
        PageRemediationState pageState,
        CandidateSelector selector)
    {
        if (selector is CandidateSelector.TextSelector text)
        {
            return pageState.TextCandidates[text.Granularity];
        }
        if (selector is CandidateSelector.AnnotationSelector)
        {
            return pageState.AnnotationCandidates;
        }

        var kinds = ((CandidateSelector.ContentSelector)selector).Kinds;
        return pageState.ContentCandidates.Where(x => kinds.Contains(x.Kind)).Cast<RemediationCandidate>().ToArray();
    }

    private static void BuildContentCandidateIndexes(IReadOnlyList<PageRemediationState> states)
    {
        var allItems = states
            .SelectMany(state => EnumerateItems(state.WorkingContent)
                .Where(IsPaintingItem)
                .Where(x => x is not TextContent<double>)
                .Select(item => (State: state, Item: item)))
            .ToList();
        var resourceCounts = allItems
            .Select(x => GetResourceObject(x.Item))
            .Where(x => x != null)
            .GroupBy(x => x!, ReferenceEqualityComparer.Instance)
            .ToDictionary(x => x.Key, x => x.Count(), ReferenceEqualityComparer.Instance);

        foreach (var state in states)
        {
            var pageSpace = new StructuredPageSpace(state.Page);
            state.TextCandidates = Enum.GetValues<Granularity>()
                .ToDictionary(
                    granularity => granularity,
                    granularity => (IReadOnlyList<TextRemediationCandidate>)state.StructuredText
                        .GetCandidates(granularity)
                        .Select(candidate => candidate.WithContentOrderIndex(
                            candidate.SourceReferences.Count == 0
                                ? candidate.SequenceIndex
                                : candidate.SourceReferences.Min(source => source.OperatorStart)) with
                        {
                            PageIndex = state.PageIndex
                        })
                        .ToArray());
            state.AnnotationCandidates = BuildAnnotationCandidates(state, pageSpace);
            state.ContentCandidates = allItems
                .Where(x => ReferenceEquals(x.State, state))
                .Select((x, index) =>
                {
                    var resource = GetResourceObject(x.Item);
                    return new ContentRemediationCandidate(
                        GetCandidateKind(x.Item)!.Value,
                        x.Item,
                        x.Item.GetBoundingBox(),
                        pageSpace.Normalize(x.Item.GetBoundingBox()),
                        x.Item.SourceReference?.OperatorStart ?? index,
                        resource != null && resourceCounts.TryGetValue(resource, out var count) ? count : 1,
                        GetStableResourceIdentity(resource),
                        FindResourceName(state.Page, resource))
                    {
                        PageIndex = state.PageIndex
                    };
                })
                .ToArray();
        }
    }

    private static IReadOnlyList<AnnotationRemediationCandidate> BuildAnnotationCandidates(
        PageRemediationState state,
        StructuredPageSpace pageSpace)
    {
        var annotations = state.Page.NativeObject.Get<PdfArray>(PdfName.Annots);
        if (annotations == null) return Array.Empty<AnnotationRemediationCandidate>();
        var result = new List<AnnotationRemediationCandidate>();
        for (var index = 0; index < annotations.Count; index++)
        {
            if (annotations[index].Resolve() is not PdfDictionary annotation) continue;
            var subtype = annotation.Get<PdfName>(PdfName.Subtype)?.Value ?? "Unknown";
            var flags = (int?)annotation.Get<PdfNumber>(PdfName.F) ?? 0;
            var hidden = (flags & 2) != 0 || (flags & 32) != 0;
            PdfRect<double>? bounds = null;
            PdfRect<double>? relative = null;
            var offPage = false;
            if (annotation.Get<PdfArray>(PdfName.Rect) is { } rectArray && rectArray.Count >= 4)
            {
                var rect = new PdfRectangle(rectArray);
                bounds = new PdfRect<double>((double)rect.LLx, (double)rect.LLy, (double)rect.URx, (double)rect.URy);
                relative = pageSpace.Normalize(bounds);
                var box = state.Page.CropBox;
                offPage = rect.URx <= box.LLx || rect.LLx >= box.URx ||
                    rect.URy <= box.LLy || rect.LLy >= box.URy;
            }
            var (destinationKind, destinationValue) = DescribeAnnotationDestination(annotation);
            result.Add(new AnnotationRemediationCandidate(
                annotation, index, subtype, bounds, relative, flags, hidden, offPage,
                annotation.ContainsKey(PdfName.StructParent),
                annotation.Get<PdfString>(PdfName.Contents)?.Value,
                destinationKind, destinationValue, int.MaxValue / 2 + index)
            {
                PageIndex = state.PageIndex
            });
        }
        return result;
    }

    private static (AnnotationDestinationKind Kind, string? Value) DescribeAnnotationDestination(PdfDictionary annotation)
    {
        if (annotation.TryGetValue(PdfName.Dest, out var direct) && direct != null)
        {
            return (AnnotationDestinationKind.Internal, DescribeDestinationValue(direct.Resolve()));
        }
        var action = annotation.Get<PdfDictionary>(PdfName.A);
        if (action == null) return (AnnotationDestinationKind.None, null);
        var kind = action.Get<PdfName>(PdfName.S)?.Value;
        if (string.Equals(kind, "URI", StringComparison.Ordinal))
        {
            return (AnnotationDestinationKind.Uri, action.Get<PdfString>((PdfName)"URI")?.Value);
        }
        if (string.Equals(kind, "GoTo", StringComparison.Ordinal))
        {
            return (AnnotationDestinationKind.Internal, DescribeDestinationValue(action.Get((PdfName)"D")?.Resolve()));
        }
        if (string.Equals(kind, "GoToR", StringComparison.Ordinal))
        {
            return (AnnotationDestinationKind.Remote, DescribeDestinationValue(action.Get((PdfName)"D")?.Resolve()));
        }
        return (AnnotationDestinationKind.Other, kind);
    }

    private static string? DescribeDestinationValue(IPdfObject? destination) => destination switch
    {
        PdfString text => text.Value,
        PdfName name => name.Value,
        PdfArray array when array.Count > 1 => array[1].Resolve() is PdfName mode ? mode.Value : "array",
        null => null,
        _ => destination.ToString()
    };

    private static ContentRemediationCandidate CreateContentCandidate(
        PageRemediationState pageState,
        IContentItem<double> item,
        int fallbackSequenceIndex) =>
        pageState.ContentCandidates.FirstOrDefault(x => ReferenceEquals(x.Item, item)) ??
        new ContentRemediationCandidate(
            GetCandidateKind(item)!.Value,
            item,
            item.GetBoundingBox(),
            new StructuredPageSpace(pageState.Page).Normalize(item.GetBoundingBox()),
            item.SourceReference?.OperatorStart ?? fallbackSequenceIndex,
            1,
            GetStableResourceIdentity(GetResourceObject(item)),
            FindResourceName(pageState.Page, GetResourceObject(item)))
        {
            PageIndex = pageState.PageIndex
        };

    private static string? FindResourceName(PdfPage page, object? resource)
    {
        if (resource == null)
        {
            return null;
        }

        foreach (var category in new[] { PdfName.XObject, PdfName.Shading })
        {
            if (!page.Resources.TryGet<PdfDictionary>(category, out var resources) || resources == null)
            {
                continue;
            }

            foreach (var entry in resources)
            {
                if (ReferenceEquals(entry.Value.Resolve(), resource))
                {
                    return entry.Key.Value;
                }
            }
        }

        return null;
    }

    private static string? GetStableResourceIdentity(object? resource)
    {
        byte[] bytes;
        if (resource is PdfStream stream)
        {
            bytes = stream.Contents.GetDecodedData();
        }
        else if (resource is IPdfObject pdfObject)
        {
            bytes = Encoding.UTF8.GetBytes(
                DescribeResourceObject(pdfObject.Resolve(), new HashSet<IPdfObject>(ReferenceEqualityComparer.Instance)));
        }
        else
        {
            return null;
        }

        var hash = SHA256.HashData(bytes);
        return "sha256:" + Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string DescribeResourceObject(IPdfObject value, HashSet<IPdfObject> visited)
    {
        value = value.Resolve();
        if (!visited.Add(value))
        {
            return "<cycle>";
        }

        return value switch
        {
            PdfName name => "/" + name.Value,
            PdfString text => "(" + text.Value + ")",
            PdfNumber number => number.ToString() ?? "0",
            PdfBoolean boolean => boolean.Value ? "true" : "false",
            PdfArray array => "[" + string.Join(",", array.Select(x => DescribeResourceObject(x, visited))) + "]",
            PdfDictionary dictionary => "<<" + string.Join(
                ",",
                dictionary.OrderBy(x => x.Key.Value, StringComparer.Ordinal)
                    .Select(x => x.Key.Value + ":" + DescribeResourceObject(x.Value, visited))) + ">>",
            _ => value.ToString() ?? value.Type.ToString()
        };
    }

    private static RemediationCandidateKind? GetCandidateKind(IContentItem<double> item) => item switch
    {
        ImageContent<double> => RemediationCandidateKind.Image,
        PathSequence<double> => RemediationCandidateKind.Path,
        FormContent<double> => RemediationCandidateKind.Form,
        ShadingContent<double> => RemediationCandidateKind.Shading,
        _ => null
    };

    private static object? GetResourceObject(IContentItem<double> item) => item switch
    {
        ImageContent<double> image => image.Stream,
        FormContent<double> form => form.Stream,
        ShadingContent<double> shading => shading.Shading,
        _ => null
    };

    private static bool IsPaintingItem(IContentItem<double> item) => item switch
    {
        PathSequence<double> path => path.Closing != null && path.Closing is not n_Op<double>,
        ImageContent<double> or FormContent<double> or ShadingContent<double> or TextContent<double> => true,
        _ => false
    };

    private void EvaluateDocument(
        IReadOnlyList<PageRemediationState> pageStates,
        IReadOnlyList<Rule> rules,
        List<RemediationClaim> allClaims,
        List<RemediationClaim> skippedClaims,
        List<string> diagnostics,
        List<string> warnings,
        RuleEvaluationAccumulator evaluations,
        List<RemediationAutoArtifactOutcome> autoArtifacts,
        List<RemediationUnaccountedContent> unaccountedContent,
        bool isCommit)
    {
        var anchors = BuildAnchorLookup(_ruleSets);
        var tolerancedZones = BuildTolerancedZoneLookup(_ruleSets);
        var flowRegions = BuildFlowRegionLookup(_ruleSets);
        var classifyClaims = new List<RemediationClaim>();

        foreach (var rule in rules.Where(x => x.Stage == Stage.Classify))
        {
            // Resolve against the claims completed by earlier classify rules. Probe diagnostics are
            // deferred until the full classify snapshot exists so prior-claim boundaries do not
            // report transient failures.
            var probeFlows = new DocumentFlowRegionResolver(
                pageStates,
                allClaims.Concat(classifyClaims).ToArray(),
                Configuration,
                anchors,
                tolerancedZones,
                flowRegions,
                new List<string>()).Resolve();

            foreach (var pageState in pageStates.Where(x =>
                         rule.Pages.Includes(x.PageIndex, _document.Pages.Count)))
            {
                var context = CreateDocumentEvaluationContext(
                    pageState,
                    allClaims.Concat(classifyClaims).ToArray(),
                    anchors,
                    tolerancedZones,
                    flowRegions,
                    probeFlows,
                    diagnostics);
                if (rule.Action is CustomRemediationAction custom)
                {
                    EvaluateCustomRule(
                        pageState, rule, custom, context, classifyClaims,
                        skippedClaims, diagnostics, evaluations);
                }
                else
                {
                    EvaluateClassifyRule(
                        pageState, rule, context, pageState.TextOwnership, classifyClaims,
                        skippedClaims, diagnostics, evaluations);
                }
            }
        }

        classifyClaims.Sort(CompareClaimsInReadingOrder);
        foreach (var pageState in pageStates)
        {
            pageState.SetClaimSnapshot(
                Stage.Classify,
                classifyClaims.Where(x => x.PageIndex == pageState.PageIndex).ToArray());
        }
        allClaims.AddRange(classifyClaims);

        var documentFlows = new DocumentFlowRegionResolver(
            pageStates,
            classifyClaims,
            Configuration,
            anchors,
            tolerancedZones,
            flowRegions,
            diagnostics).Resolve();

        var groupClaims = new List<RemediationClaim>();
        var frontier = classifyClaims.Where(x => x.Status == ClaimStatus.Applied).ToList();
        foreach (var pass in rules.Where(x => x.Stage == Stage.Group)
                     .Select(x => x.GroupPass).Distinct().OrderBy(x => x))
        {
            var passClaims = new List<RemediationClaim>();
            var frozenFrontier = frontier.ToArray();
            var referenceClaims = classifyClaims.Concat(groupClaims)
                .Where(x => x.Status == ClaimStatus.Applied)
                .ToArray();
            foreach (var rule in rules.Where(x => x.Stage == Stage.Group && x.GroupPass == pass))
            {
                var outputCount = passClaims.Count;
                if (rule.Action is TableRemediationAction table)
                {
                    EvaluateDocumentTableRule(
                        pageStates, rule, table, frozenFrontier, referenceClaims, documentFlows,
                        anchors, tolerancedZones, flowRegions, passClaims,
                        skippedClaims, diagnostics, evaluations);
                }
                else if (rule.Action is GroupRemediationAction group)
                {
                    EvaluateDocumentClaimRunRule(
                        pageStates, rule, group.Over, frozenFrontier, referenceClaims, documentFlows,
                        anchors, tolerancedZones, flowRegions, passClaims,
                        skippedClaims, diagnostics, warnings, evaluations);
                }
                else if (rule.Action is BindTemplateSlotRemediationAction bind && bind.Over != null)
                {
                    EvaluateDocumentClaimRunRule(
                        pageStates, rule, bind.Over, frozenFrontier, referenceClaims, documentFlows,
                        anchors, tolerancedZones, flowRegions, passClaims,
                        skippedClaims, diagnostics, warnings, evaluations);
                }
                else if (rule.Action is MergeRemediationAction merge)
                {
                    EvaluateDocumentClaimRunRule(
                        pageStates, rule, merge.Over, frozenFrontier, referenceClaims, documentFlows,
                        anchors, tolerancedZones, flowRegions, passClaims,
                        skippedClaims, diagnostics, warnings, evaluations);
                }

                if (passClaims.Count == outputCount)
                {
                    AddConsumedInputWarning(rule, frozenFrontier, referenceClaims, groupClaims, warnings);
                }
            }

            var duplicate = passClaims.SelectMany(parent => parent.RelatedClaims.Select(child => (parent, child)))
                .GroupBy(x => x.child.ClaimId).FirstOrDefault(x => x.Count() > 1);
            if (duplicate != null)
            {
                var consumers = string.Join(", ", duplicate.Select(x => x.parent.RuleId).Distinct());
                ReportDiagnostic(DiagnosticCode.GroupCompositionAmbiguous, $"GroupPass{pass}",
                    $"Group pass {pass} has consumers [{consumers}] selecting claim '{duplicate.Key}'. Later passes were not evaluated.", diagnostics);
                break;
            }

            var consumed = passClaims.SelectMany(x => x.RelatedClaims).Select(x => x.ClaimId).ToHashSet();
            frontier = frontier.Where(x => !consumed.Contains(x.ClaimId)).Concat(passClaims)
                .OrderBy(x => x, ReadingOrderComparer).ToList();
            groupClaims.AddRange(passClaims);
        }

        groupClaims.Sort((left, right) =>
        {
            var byPass = left.GroupPass.CompareTo(right.GroupPass);
            return byPass != 0 ? byPass : CompareClaimsInReadingOrder(left, right);
        });
        foreach (var pageState in pageStates)
        {
            // A cross-page group is planned and refined once, from its primary page.
            pageState.SetClaimSnapshot(
                Stage.Group,
                groupClaims.Where(x => x.PageIndex == pageState.PageIndex).ToArray());
        }
        allClaims.AddRange(groupClaims);

        // Prescriptive composite ancestors that have no BindOver producer are represented as
        // deterministic synthetic claims before Refine, so FromSlot can target the instantiated
        // template hierarchy rather than relying on report-time slot lookup.
        var templateClaims = BuildPrescriptiveTemplateClaims(allClaims, rules);
        if (templateClaims.Count > 0)
        {
            allClaims.AddRange(templateClaims);
            foreach (var pageState in pageStates)
            {
                pageState.SetClaimSnapshot(
                    Stage.Refine,
                    templateClaims.Where(x => x.PageIndex == pageState.PageIndex).ToArray());
            }
        }

        var refineClaims = new List<RemediationClaim>();
        foreach (var rule in rules.Where(x => x.Stage == Stage.Refine))
        {
            foreach (var pageState in pageStates.Where(x =>
                         rule.Pages.Includes(x.PageIndex, _document.Pages.Count)))
            {
                var context = CreateDocumentEvaluationContext(
                    pageState,
                    allClaims,
                    anchors,
                    tolerancedZones,
                    flowRegions,
                    documentFlows,
                    diagnostics);
                if (rule.Action is StructureAttributeRemediationAction attributes)
                {
                    EvaluateRefineAttributeRule(
                        pageState, rule, attributes, context, refineClaims, skippedClaims, evaluations);
                }
                else if (rule.Action is ReorderSiblingsRemediationAction reorder)
                {
                    EvaluateRefineReorderRule(
                        pageState, rule, reorder, context, refineClaims, skippedClaims, evaluations);
                }
                else if (rule.Action is StructureLinkRemediationAction link)
                {
                    EvaluateRefineLinkRule(
                        pageState, rule, link, context, refineClaims,
                        skippedClaims, diagnostics, evaluations);
                }
            }
        }

        foreach (var pageState in pageStates)
        {
            pageState.SetClaimSnapshot(
                Stage.Refine,
                templateClaims.Concat(refineClaims)
                    .Where(x => x.PageIndex == pageState.PageIndex)
                    .ToArray());
        }
        allClaims.AddRange(refineClaims);

        foreach (var pageState in pageStates)
        {
            pageState.ArtifactZones = ResolveArtifactZones(
                pageState, allClaims, anchors, tolerancedZones, flowRegions, documentFlows, diagnostics);
            ApplyLeftoverPolicy(
                pageState,
                pageState.TextOwnership,
                diagnostics,
                autoArtifacts,
                unaccountedContent,
                isCommit);
        }
    }

    private IReadOnlyDictionary<string, TolerancedZoneResolution> ResolveArtifactZones(
        PageRemediationState pageState,
        IReadOnlyList<RemediationClaim> claims,
        IReadOnlyDictionary<string, RemediationAnchor> anchors,
        IReadOnlyDictionary<string, TolerancedZone> tolerancedZones,
        IReadOnlyDictionary<string, FlowRegion> flowRegions,
        DocumentFlowIndex documentFlows,
        List<string> diagnostics)
    {
        var zoneIds = ArtifactInventory.Where(x => x.ZoneId != null).Select(x => x.ZoneId!)
            .Distinct(StringComparer.Ordinal).ToList();
        var resolved = new Dictionary<string, TolerancedZoneResolution>(StringComparer.Ordinal);
        if (zoneIds.Count == 0)
        {
            return resolved;
        }

        var context = CreateDocumentEvaluationContext(
            pageState, claims, anchors, tolerancedZones, flowRegions, documentFlows, diagnostics);
        foreach (var zoneId in zoneIds)
        {
            if (context.ResolveTolerancedZone(zoneId) is { } zone)
            {
                resolved[zoneId] = zone;
            }
        }

        return resolved;
    }

    /// <summary>
    /// Resolves the inventory item that absorbed content belongs to. Absorbed content carries no
    /// authored subtype, so only geometrically qualified furniture can claim it.
    /// </summary>
    private RemediationArtifactInventoryItem? ResolveAbsorbedArtifactItem(
        PageRemediationState pageState,
        PdfRect<double> relativeBounds)
    {
        var itemId = RemediationArtifactInventoryMatcher.ResolveItemId(
            ArtifactInventory,
            new RemediationArtifactRecord(pageState.PageIndex, null, relativeBounds),
            _document.Pages.Count,
            pageState.ArtifactZones);
        return itemId == null
            ? null
            : ArtifactInventory.First(x => string.Equals(x.Id, itemId, StringComparison.Ordinal));
    }

    /// <summary>Declared page furniture merged across every composed rule set.</summary>
    private IReadOnlyList<RemediationArtifactInventoryItem> ArtifactInventory =>
        _artifactInventory ??= _program != null
            ? _program.Program.Artifacts.Select(x => new RemediationArtifactInventoryItem(
                x.Id, x.Subtype, x.Pages, occurrence: x.Occurrence,
                semanticSubtype: x.SemanticSubtype, includeBoundingBox: x.IncludeBoundingBox,
                attached: x.Attached) { RuleSetId = _program.Program.Id }).ToList()
            : _ruleSets.SelectMany(x => x.Artifacts).ToList();

    private IReadOnlyList<RemediationArtifactInventoryItem>? _artifactInventory;

    private RemediationEvaluationContext CreateDocumentEvaluationContext(
        PageRemediationState pageState,
        IReadOnlyList<RemediationClaim> claims,
        IReadOnlyDictionary<string, RemediationAnchor> anchors,
        IReadOnlyDictionary<string, TolerancedZone> tolerancedZones,
        IReadOnlyDictionary<string, FlowRegion> flowRegions,
        DocumentFlowIndex documentFlows,
        List<string> diagnostics)
    {
        return new RemediationEvaluationContext(
            claims,
            pageBox: pageState.StructuredText.RelativePageBox,
            pageIndex: pageState.PageIndex,
            pageCount: _document.Pages.Count,
            configuration: Configuration,
            anchors: anchors,
            tolerancedZones: tolerancedZones,
            flowRegions: flowRegions,
            resolvedFlowRegions: documentFlows.ForPage(pageState.PageIndex),
            structuredText: pageState.StructuredText,
            diagnostics: diagnostics,
            flowsArePreResolved: true)
        {
            DocumentFlows = documentFlows,
            DocumentCandidates = documentFlows.Candidates
        };
    }

    private void EvaluateCustomRule(
        PageRemediationState pageState,
        Rule rule,
        CustomRemediationAction custom,
        RemediationEvaluationContext context,
        List<RemediationClaim> stageClaims,
        List<RemediationClaim> skippedClaims,
        List<string> diagnostics,
        RuleEvaluationAccumulator evaluations)
    {
        var customCtx = new CustomRemediationContext(
            this,
            context,
            SelectCandidates(pageState, rule.Candidates!));

        CustomRemediationOutcome outcome;
        try
        {
            outcome = custom.Handler(customCtx);
        }
        catch (Exception ex)
        {
            diagnostics.Add($"Rule '{rule.Id}' custom handler failed: {ex.Message}");
            evaluations.Record(rule, pageState.PageIndex, considered: customCtx.Candidates.Count);
            return;
        }

        evaluations.Record(
            rule,
            pageState.PageIndex,
            considered: customCtx.Candidates.Count,
            matched: outcome.ClaimedCandidates.Count);

        if (outcome.ClaimedCandidates.Count == 0)
        {
            return;
        }

        var claim = new RemediationClaim(
            rule.Id,
            outcome.ClaimedCandidates,
            custom.Description,
            1.0)
        {
            PageIndex = pageState.PageIndex,
            Status = ClaimStatus.Applied,
            SelectorDebugString = rule.Predicate.DebugString,
            Action = outcome.Action ?? custom,
            RuleSetId = rule.RuleSetId,
                SlotId = rule.Slot,
            TextNormalization = rule.TextNormalization ?? TextNormalizationOptions.Default
        };

        stageClaims.Add(claim);
    }

    private void EvaluateClassifyRule(
        PageRemediationState pageState,
        Rule rule,
        RemediationEvaluationContext context,
        TextOwnershipIndex ownedTargets,
        List<RemediationClaim> stageClaims,
        List<RemediationClaim> skippedClaims,
        List<string> diagnostics,
        RuleEvaluationAccumulator evaluations)
    {
        var normalization = rule.TextNormalization ?? TextNormalizationOptions.Default;
        context = context.WithTextNormalization(normalization);
        var candidates = SelectCandidates(pageState, rule.Candidates!);
        var candidateResults = new List<(RemediationCandidate Candidate, PredicateResult Result)>();
        foreach (var candidate in candidates)
        {
            var traced = _traceRequest?.Includes(rule.Id, pageState.PageIndex, candidate) == true;
            var evaluationContext = context.WithPredicateTracing(traced);
            var result = rule.Predicate.Evaluate(evaluationContext, candidate);
            if (result.IsMatch)
            {
                candidateResults.Add((candidate, result));
            }
            else if (traced && result.Trace != null)
            {
                _predicateTraces!.Add(new RemediationPredicateTrace(
                    rule.Id,
                    pageState.PageIndex,
                    candidate.CandidateId,
                    candidate.Kind,
                    candidate is TextRemediationCandidate text ? text.Text : string.Empty,
                    candidate is TextRemediationCandidate normalizedText
                        ? normalization.Normalize(normalizedText.Text)
                        : string.Empty,
                    candidate.SourceReferences,
                    result.Trace));
            }
        }

        if (candidateResults.Count == 0 &&
            ContainsFlowRegionPredicate(rule.Predicate) &&
            context.ResolvedFlowRegions.Values.Any(region => candidates.Any(region.Contains)))
        {
            diagnostics.Add($"Rule '{rule.Id}' matched no candidates inside its flow region.");
        }

        if (ContainsNearestPredicate(rule.Predicate) && candidateResults.Count > 1)
        {
            var maxConfidence = candidateResults.Max(x => x.Result.Confidence);
            candidateResults = candidateResults
                .Where(x => Math.Abs(x.Result.Confidence - maxConfidence) <= 0.000001)
                .ToList();

            if (candidateResults.Count > 1)
            {
                evaluations.Record(
                    rule,
                    pageState.PageIndex,
                    considered: candidates.Count,
                    matched: candidateResults.Count);
                diagnostics.Add($"Rule '{rule.Id}' has an ambiguous nearest-anchor match with {candidateResults.Count} equally near candidates.");
                return;
            }
        }

        var rejectedByConfidence = 0;
        var rejectedByConflict = 0;
        foreach (var (candidate, predicate) in candidateResults)
        {
            var confidence = predicate.Confidence;
            if (rule.MinConfidence is { } minConfidence && confidence < minConfidence)
            {
                rejectedByConfidence++;
                skippedClaims.Add(CreateClaim(rule, pageState.PageIndex, candidate, ClaimStatus.Skipped, confidence));
                continue;
            }

            var targetSpans = GetTargetSpans(candidate);
            var contentItem = (candidate as ContentRemediationCandidate)?.Item;
            var annotationObject = (candidate as AnnotationRemediationCandidate)?.Annotation;
            var conflicting = annotationObject != null &&
                pageState.AnnotationOwnership.TryGetValue(annotationObject, out var annotationClaim)
                    ? new[] { new OwnedTextSpan(default, annotationClaim) }.ToList()
                    : contentItem != null && pageState.ContentOwnership.TryGetValue(contentItem, out var contentClaim)
                        ? new[] { new OwnedTextSpan(default, contentClaim) }.ToList()
                        : ownedTargets.FindOverlaps(targetSpans);
            if (conflicting.Count > 0 && !rule.Override)
            {
                if (annotationObject != null)
                {
                    ReportDiagnostic(
                        DiagnosticCode.AnnotationAlreadyConsumed,
                        $"Page{pageState.PageIndex + 1}",
                        $"Annotation '{candidate.CandidateId}' selected by rule '{rule.Id}' is already consumed by rule '{conflicting[0].Claim.RuleId}'.",
                        diagnostics);
                }
                rejectedByConflict++;
                skippedClaims.Add(CreateClaim(rule, pageState.PageIndex, candidate, ClaimStatus.Skipped, confidence));
                continue;
            }

            if (rule.Override)
            {
                foreach (var previous in conflicting
                    .Select(x => x.Claim)
                    .DistinctBy(x => x.ClaimId)
                    .ToList())
                {
                    if (stageClaims.Remove(previous))
                    {
                        skippedClaims.Add(previous with { Status = ClaimStatus.Overridden });
                    }

                    ownedTargets.Remove(previous);
                    foreach (var owned in pageState.ContentOwnership.Where(x => x.Value.ClaimId == previous.ClaimId).ToList())
                    {
                        pageState.ContentOwnership.Remove(owned.Key);
                    }
                    foreach (var owned in pageState.AnnotationOwnership.Where(x => x.Value.ClaimId == previous.ClaimId).ToList())
                    {
                        pageState.AnnotationOwnership.Remove(owned.Key);
                    }
                    foreach (var residual in CreateResidualClaims(previous, targetSpans))
                    {
                        stageClaims.Add(residual);
                        ownedTargets.Add(residual, residual.Candidates.SelectMany(GetTargetSpans));
                    }
                }
            }

            var claim = CreateClaim(rule, pageState.PageIndex, candidate, ClaimStatus.Applied, confidence);
            ownedTargets.Add(claim, targetSpans);
            if (contentItem != null)
            {
                pageState.ContentOwnership[contentItem] = claim;
            }
            if (annotationObject != null)
            {
                pageState.AnnotationOwnership[annotationObject] = claim;
            }
            stageClaims.Add(claim);
        }

        evaluations.Record(
            rule,
            pageState.PageIndex,
            considered: candidates.Count,
            matched: candidateResults.Count,
            rejectedByConfidence: rejectedByConfidence,
            rejectedByConflict: rejectedByConflict);
    }

    private static bool ContainsNearestPredicate(RemediationPredicate predicate) =>
        predicate switch
        {
            AnchorRelativeRemediationPredicate { Kind: AnchorRelativePredicateKind.NearestTo } => true,
            CompositeRemediationPredicate composite => ContainsNearestPredicate(composite.Left) || ContainsNearestPredicate(composite.Right),
            NotRemediationPredicate not => ContainsNearestPredicate(not.Inner),
            _ => false
        };

    private static bool ContainsFlowRegionPredicate(RemediationPredicate predicate) =>
        predicate switch
        {
            FlowRegionRemediationPredicate => true,
            FlowOrderRemediationPredicate { Kind: not FlowOrderPredicateKind.FirstAfter } => true,
            FlowOrderRemediationPredicate { Where: { } where } => ContainsFlowRegionPredicate(where),
            CompositeRemediationPredicate composite => ContainsFlowRegionPredicate(composite.Left) || ContainsFlowRegionPredicate(composite.Right),
            NotRemediationPredicate not => ContainsFlowRegionPredicate(not.Inner),
            _ => false
        };

    private void ApplyLeftoverPolicy(
        PageRemediationState pageState,
        TextOwnershipIndex ownedTargets,
        List<string> diagnostics,
        List<RemediationAutoArtifactOutcome> autoArtifacts,
        List<RemediationUnaccountedContent> unaccountedContent,
        bool isCommit,
        TextNormalizationOptions? normalization = null)
    {
        var textNormalization = normalization ?? TextNormalizationOptions.Default;
        var leftovers = EnumerateItems(pageState.WorkingContent)
            .OfType<TextContent<double>>()
            .Where(x => x.SourceReference is { })
            .SelectMany(item =>
            {
                var source = new SourceTextSpan(
                    item.SourceReference!.Value,
                    item.SourceCharacterOffset,
                    item.Text.Length);
                return ownedTargets.GetUnowned(source).Select(span => (Item: item, Span: span));
            })
            .ToList();
        // Program ownership is character-exact, so word bindings can leave only operator
        // whitespace behind. Normalization-empty spans are not rendered semantic content.
        if (normalization != null)
        {
            leftovers = leftovers
                .Where(x => !string.IsNullOrWhiteSpace(textNormalization.Normalize(GetText(x.Item, x.Span))))
                .ToList();
        }
        var graphicalLeftovers = EnumerateItems(pageState.WorkingContent)
            .Where(IsPaintingItem)
            .Where(x => x is not TextContent<double>)
            .Where(x => !pageState.ContentOwnership.ContainsKey(x))
            .ToList();
        if (leftovers.Count == 0 && graphicalLeftovers.Count == 0)
        {
            return;
        }

        var prescriptive = IsPrescriptiveTemplate();
        if (prescriptive || Configuration.LeftoverPolicy is RemediationLeftoverPolicy.Flag or RemediationLeftoverPolicy.FailFast)
        {
            foreach (var (item, span) in leftovers)
            {
                var localStart = span.StartCharacterIndex - item.SourceCharacterOffset;
                var text = localStart >= 0 && localStart + span.CharacterCount <= item.Text.Length
                    ? item.Text.Substring(localStart, span.CharacterCount)
                    : item.Text;
                var templateCandidate = pageState.StructuredText.GetCandidates(Granularity.Paragraph)
                        .FirstOrDefault(x => x.SourceReferences.Contains(span.SourceReference)) ??
                    pageState.StructuredText.GetCandidates(Granularity.Line)
                        .FirstOrDefault(x => x.SourceReferences.Contains(span.SourceReference));
                var range = new RemediationTextRange(
                        span.SourceReference,
                        span.StartCharacterIndex,
                        span.CharacterCount,
                        text);
                var characters = pageState.StructuredText.Characters
                        .Where(x => x.SourceReference == span.SourceReference &&
                                    x.SourceCharacterIndex >= span.StartCharacterIndex &&
                                    x.SourceCharacterIndex < span.EndCharacterIndex)
                        .ToArray();
                var candidate = templateCandidate == null
                    ? null
                    : RemediationCandidate.CreateExactRange(templateCandidate, range, characters);
                var bounds = candidate?.BoundingBox ?? item.GetBoundingBox();
                var relativeBounds = candidate?.RelativeBoundingBox ??
                    new StructuredPageSpace(pageState.Page).Normalize(bounds);
                var candidateId = candidate?.CandidateId ??
                    $"RawText:{pageState.PageIndex}:{span.SourceReference.StreamId}:" +
                    $"{span.SourceReference.OperatorStart}:{span.StartCharacterIndex}:{span.CharacterCount}";
                unaccountedContent.Add(new RemediationUnaccountedContent(
                    pageState.PageIndex,
                    RemediationCandidateKind.Text,
                    candidateId,
                    span.SourceReference,
                    bounds,
                    relativeBounds,
                    text,
                    textNormalization.Normalize(text)));
            }

            foreach (var item in graphicalLeftovers)
            {
                var candidate = CreateContentCandidate(pageState, item, int.MaxValue);
                unaccountedContent.Add(new RemediationUnaccountedContent(
                    pageState.PageIndex,
                    candidate.Kind,
                    candidate.CandidateId,
                    item.SourceReference ?? default,
                    candidate.BoundingBox,
                    candidate.RelativeBoundingBox,
                    ResourceIdentity: candidate.ResourceIdentity,
                    ResourceName: candidate.ResourceName,
                    ResourceUseCount: candidate.ResourceUseCount));
            }

            var typeCounts = unaccountedContent
                .Where(x => x.PageIndex == pageState.PageIndex)
                .GroupBy(x => x.CandidateKind)
                .OrderBy(x => x.Key)
                .Select(x => $"{x.Key}={x.Count()}");
            ReportDiagnostic(
                prescriptive ? DiagnosticCode.PrescriptiveUnaccountedContent : DiagnosticCode.UntaggedContent,
                $"Page{pageState.PageIndex + 1}",
                (prescriptive
                    ? $"Prescriptive template left painting content on page {pageState.PageIndex + 1} without a slot or declared artifact inventory item: "
                    : $"Page {pageState.PageIndex + 1} has unclaimed painting content: ") +
                string.Join(", ", typeCounts) + ".",
                diagnostics);
            return;
        }

        if (Configuration.LeftoverPolicy != RemediationLeftoverPolicy.AutoArtifact)
        {
            return;
        }

        foreach (var (item, span) in leftovers)
        {
            var localStart = span.StartCharacterIndex - item.SourceCharacterOffset;
            var text = localStart >= 0 && localStart + span.CharacterCount <= item.Text.Length
                ? item.Text.Substring(localStart, span.CharacterCount)
                : item.Text;
            var relativeBounds = new StructuredPageSpace(pageState.Page).Normalize(item.GetBoundingBox());
            autoArtifacts.Add(new RemediationAutoArtifactOutcome(
                pageState.PageIndex,
                item.SourceReference!.Value,
                text,
                item.GetBoundingBox(),
                isCommit
                    ? RemediationAutoArtifactDisposition.Applied
                    : RemediationAutoArtifactDisposition.Planned)
            {
                RelativeBoundingBox = relativeBounds,
                InventoryItemId = ResolveAbsorbedArtifactItem(pageState, relativeBounds)?.Id
            });
        }

        foreach (var item in graphicalLeftovers)
        {
            var candidate = CreateContentCandidate(pageState, item, int.MaxValue);
            var declared = ResolveAbsorbedArtifactItem(pageState, candidate.RelativeBoundingBox);
            var claim = new RemediationClaim(
                "__auto_artifact__", new[] { candidate }, "Artifact")
            {
                PageIndex = pageState.PageIndex,
                Action = declared == null
                    ? RemediationActions.Artifact(ArtifactSubtype.Layout)
                    : new ArtifactRemediationAction(
                        declared.Subtype,
                        declared.SemanticSubtype,
                        declared.IncludeBoundingBox,
                        declared.Attached)
            };
            pageState.ContentOwnership[item] = claim;
            autoArtifacts.Add(new RemediationAutoArtifactOutcome(
                pageState.PageIndex,
                item.SourceReference ?? default,
                string.Empty,
                candidate.BoundingBox,
                isCommit
                    ? RemediationAutoArtifactDisposition.Applied
                    : RemediationAutoArtifactDisposition.Planned)
            {
                RelativeBoundingBox = candidate.RelativeBoundingBox,
                InventoryItemId = declared?.Id,
                CandidateKind = candidate.Kind,
                CandidateId = candidate.CandidateId,
                ResourceIdentity = candidate.ResourceIdentity,
                ResourceName = candidate.ResourceName,
                ResourceUseCount = candidate.ResourceUseCount
            });
            if (isCommit)
            {
                ApplyClassifyClaim(pageState, claim, diagnostics);
            }
        }

        static string GetText(TextContent<double> item, SourceTextSpan span)
        {
            var localStart = span.StartCharacterIndex - item.SourceCharacterOffset;
            return localStart >= 0 && localStart + span.CharacterCount <= item.Text.Length
                ? item.Text.Substring(localStart, span.CharacterCount)
                : item.Text;
        }
    }

    private void ApplyAdoptAnnotationClaim(
        PageRemediationState pageState,
        RemediationClaim claim,
        List<string> diagnostics)
    {
        if (claim.Action is not AdoptAnnotationRemediationAction action ||
            claim.Candidates.OfType<AnnotationRemediationCandidate>().SingleOrDefault() is not { } candidate)
        {
            return;
        }

        StructureNode? node;
        RemediationAppliedBinding? targetBinding = null;
        if (action.Into != null)
        {
            targetBinding = claim.AnnotationIntoClaim?.AppliedBindings
                .Where(x => x.StructureNode != null)
                .FirstOrDefault(x => !candidate.HasUsableGeometry ||
                    x.Bounds == null || x.Bounds.Intersects(candidate.BoundingBox));
            node = targetBinding?.StructureNode;
            if (node == null)
            {
                diagnostics.Add($"Rule '{claim.RuleId}' could not reuse the planned annotation target binding.");
                return;
            }
        }
        else
        {
            node = Structure.AddElement(claim.ProducedTag).GetNode();
        }

        var destinationNode = claim.AnnotationDestinationClaim?.AppliedBindings
            .FirstOrDefault(x => x.StructureNode != null)?.StructureNode;
        if (action.DestinationTarget != null && destinationNode == null)
        {
            diagnostics.Add($"Rule '{claim.RuleId}' could not reuse the planned annotation destination binding.");
            return;
        }

        BindAnnotation(
            node,
            pageState.Page,
            candidate.Annotation,
            action.AccessibleDescription ?? candidate.Contents,
            destinationNode);
        claim.AddAppliedBinding(new RemediationAppliedBinding(
            node.Type,
            targetBinding?.Mcids ?? Array.Empty<int>(),
            node,
            targetBinding?.MarkedContentGroup,
            node.Parent,
            Array.Empty<StructuredSourceRef>(),
            candidate.Bounds));
        pageState.MarkDirty();
    }

    private void ApplyClassifyClaim(
        PageRemediationState pageState,
        RemediationClaim claim,
        List<string> diagnostics)
    {
        if (claim.Action is not (TagRemediationAction or ArtifactRemediationAction or BindTemplateSlotRemediationAction))
        {
            return;
        }

        foreach (var candidate in claim.Candidates)
        {
            IReadOnlyList<IContentItem<double>> leaves;
            try
            {
                leaves = candidate.MaterializeLeaves(pageState.WorkingContent);
            }
            catch (Exception ex)
            {
                diagnostics.Add($"Rule '{claim.RuleId}' failed to materialize targets: {ex.Message}");
                continue;
            }

            if (leaves.Count == 0)
            {
                diagnostics.Add($"Rule '{claim.RuleId}' matched content but resolved no leaves.");
                continue;
            }

            if (claim.Action is TagRemediationAction or BindTemplateSlotRemediationAction)
            {
                var tagName = claim.Action is TagRemediationAction
                    ? ((TagRemediationAction)claim.Action).Name
                    : (PdfName)claim.Tag;
                var mcid = AllocateMcid(pageState.Page);
                var marked = new MarkedContent(tagName)
                {
                    InlineProps = new PdfDictionary { [PdfName.MCID] = new PdfIntNumber(mcid) }
                };
                var wrapper = pageState.WorkingContent.Wrap(leaves, marked);
                var node = Structure.AddElement(tagName.Value).GetNode();
                if (Configuration.DebugWrite)
                {
                    node.Title = claim.RuleId;
                }

                if (claim.Action is TagRemediationAction explicitTag && explicitTag.Attributes != null)
                {
                    node.Attributes.Add(explicitTag.Attributes);
                }

                BindMarkedContent(node, pageState.Page, mcid);
                claim.AddAppliedBinding(new RemediationAppliedBinding(
                    tagName.Value,
                    new[] { mcid },
                    node,
                    wrapper,
                    node.Parent,
                    candidate.SourceReferences,
                    candidate.BoundingBox));
                pageState.MarkDirty();
            }
            else if (claim.Action is ArtifactRemediationAction artifact)
            {
                var wrapper = pageState.WorkingContent.Wrap(
                    leaves,
                    new MarkedContent(PdfName.Artifact)
                    {
                        InlineProps = BuildArtifactProperties(artifact, candidate.BoundingBox)
                    });
                claim.AddAppliedBinding(new RemediationAppliedBinding(
                    PdfName.Artifact.Value,
                    Array.Empty<int>(),
                    null,
                    wrapper,
                    null,
                    candidate.SourceReferences,
                    candidate.BoundingBox));
                pageState.MarkDirty();
            }
        }
    }

    private void EvaluateDocumentClaimRunRule(
        IReadOnlyList<PageRemediationState> pageStates,
        Rule rule,
        ClaimPredicate over,
        IReadOnlyList<RemediationClaim> frontier,
        IReadOnlyList<RemediationClaim> referenceClaims,
        DocumentFlowIndex documentFlows,
        IReadOnlyDictionary<string, RemediationAnchor> anchors,
        IReadOnlyDictionary<string, TolerancedZone> tolerancedZones,
        IReadOnlyDictionary<string, FlowRegion> flowRegions,
        List<RemediationClaim> stageClaims,
        List<RemediationClaim> skippedClaims,
        List<string> diagnostics,
        List<string> warnings,
        RuleEvaluationAccumulator evaluations)
    {
        var pageLookup = pageStates.ToDictionary(x => x.PageIndex);
        var eligible = frontier
            .Where(x => x.Status == ClaimStatus.Applied)
            .Where(x => rule.Pages.Includes(x.PageIndex, _document.Pages.Count))
            .OrderBy(x => x, ReadingOrderComparer)
            .ToList();
        var currentRun = new List<RemediationClaim>();

        foreach (var claim in eligible)
        {
            var page = pageLookup[claim.PageIndex];
            var previous = currentRun.LastOrDefault();
            var predicateContext = CreateClaimPredicateContext(
                eligible, referenceClaims, rule, previous, page, anchors, tolerancedZones, flowRegions, documentFlows, diagnostics);
            var result = over.Evaluate(predicateContext, claim);
            var crossesPage = previous != null && previous.PageIndex != claim.PageIndex;
            var canContinue = !crossesPage ||
                documentFlows.FindSharedInstance(previous!, claim) != null;

            if (!result.IsMatch || !canContinue)
            {
                if (!canContinue && currentRun.Count > 0 && !ContainsSamePage(over))
                {
                    var fresh = over.Evaluate(
                        CreateClaimPredicateContext(
                            eligible, referenceClaims, rule, null, page, anchors, tolerancedZones, flowRegions, documentFlows, diagnostics),
                        claim);
                    if (fresh.IsMatch)
                    {
                        var warning =
                            $"Rule '{rule.Id}' ended a group run between pages {previous!.PageIndex + 1} " +
                            $"and {claim.PageIndex + 1}; the claims do not share an active ContinueUntilEnd flow instance.";
                        if (!warnings.Contains(warning, StringComparer.Ordinal))
                        {
                            warnings.Add(warning);
                        }
                    }
                }

                AddGroupRun(
                    rule,
                    currentRun.Count == 0 ? claim.PageIndex : currentRun[0].PageIndex,
                    currentRun,
                    stageClaims);
                currentRun.Clear();
                result = over.Evaluate(
                    CreateClaimPredicateContext(
                        eligible, referenceClaims, rule, null, page, anchors, tolerancedZones, flowRegions, documentFlows, diagnostics),
                    claim);
            }

            var rejectedByConfidence = 0;
            if (result.IsMatch)
            {
                if (rule.MinConfidence is { } minConfidence && result.Confidence < minConfidence)
                {
                    rejectedByConfidence = 1;
                    var skipped = CreateClaim(
                        rule,
                        claim.PageIndex,
                        claim.Candidates[0],
                        ClaimStatus.Skipped,
                        result.Confidence);
                    skipped.AddRelatedClaims(new[] { claim });
                    skippedClaims.Add(skipped);
                }
                else
                {
                    currentRun.Add(claim);
                }
            }

            evaluations.Record(
                rule,
                claim.PageIndex,
                considered: 1,
                matched: result.IsMatch ? 1 : 0,
                rejectedByConfidence: rejectedByConfidence);
        }

        AddGroupRun(
            rule,
            currentRun.Count == 0 ? -1 : currentRun[0].PageIndex,
            currentRun,
            stageClaims);
    }

    private void EvaluateDocumentTableRule(
        IReadOnlyList<PageRemediationState> pageStates,
        Rule rule,
        TableRemediationAction table,
        IReadOnlyList<RemediationClaim> frontier,
        IReadOnlyList<RemediationClaim> referenceClaims,
        DocumentFlowIndex documentFlows,
        IReadOnlyDictionary<string, RemediationAnchor> anchors,
        IReadOnlyDictionary<string, TolerancedZone> tolerancedZones,
        IReadOnlyDictionary<string, FlowRegion> flowRegions,
        List<RemediationClaim> stageClaims,
        List<RemediationClaim> skippedClaims,
        List<string> diagnostics,
        RuleEvaluationAccumulator evaluations)
    {
        if (table.Over == null)
        {
            diagnostics.Add(
                $"Rule '{rule.Id}' table action has no existing-claim selector. " +
                "Classify cell content and use TableOver.");
            return;
        }

        var pageLookup = pageStates.ToDictionary(x => x.PageIndex);
        var eligible = frontier
            .Where(x => x.Status == ClaimStatus.Applied)
            .Where(x => rule.Pages.Includes(x.PageIndex, _document.Pages.Count))
            .OrderBy(x => x, ReadingOrderComparer)
            .ToList();
        var matched = new List<(RemediationClaim Claim, double Confidence)>();
        RemediationClaim? previous = null;
        foreach (var claim in eligible)
        {
            var result = table.Over.Evaluate(
                CreateClaimPredicateContext(
                    eligible,
                    referenceClaims,
                    rule,
                    previous,
                    pageLookup[claim.PageIndex],
                    anchors,
                    tolerancedZones,
                    flowRegions,
                    documentFlows,
                    diagnostics),
                claim);
            previous = claim;
            var rejected = result.IsMatch &&
                rule.MinConfidence is { } minConfidence &&
                result.Confidence < minConfidence;
            evaluations.Record(
                rule,
                claim.PageIndex,
                considered: 1,
                matched: result.IsMatch ? 1 : 0,
                rejectedByConfidence: rejected ? 1 : 0);
            if (!result.IsMatch)
            {
                continue;
            }
            if (rejected)
            {
                AddSkippedClaim(rule, claim.PageIndex, claim, result.Confidence, skippedClaims);
                continue;
            }
            matched.Add((claim, result.Confidence));
        }

        var matchedWithInstances = matched
            .Select(x => (x.Claim, x.Confidence, Instance: documentFlows.FindContainingInstance(x.Claim)))
            .ToList();
        var instancesByPage = matchedWithInstances
            .Select(x => (x.Claim.PageIndex, x.Instance))
            .Where(x => x.Instance != null)
            .GroupBy(x => x.PageIndex)
            .ToDictionary(
                x => x.Key,
                x => x.Select(y => y.Instance!.Value).Distinct().ToArray());
        var partitions = matchedWithInstances.GroupBy(x =>
        {
            var instance = x.Instance;
            if (instance == null &&
                RowMatchesHeaderSelector(table, new[] { x.Claim }, matchedWithInstances.Select(y => y.Claim).ToArray()) &&
                instancesByPage.TryGetValue(x.Claim.PageIndex, out var pageInstances) &&
                pageInstances.Length == 1)
            {
                instance = pageInstances[0];
            }
            return instance != null
                ? $"flow:{instance.Value}"
                : $"page:{x.Claim.PageIndex}";
        }, StringComparer.Ordinal);

        foreach (var partition in partitions)
        {
            var claims = partition.Select(x => x.Claim).OrderBy(x => x, ReadingOrderComparer).ToList();
            var candidates = claims.SelectMany(x => x.Candidates).ToArray();
            var grid = ResolveTableGrid(table, candidates);
            if (grid.Error != null)
            {
                diagnostics.Add($"Rule '{rule.Id}' could not resolve table columns: {grid.Error}");
                continue;
            }

            var confidence = Math.Min(partition.Min(x => x.Confidence), grid.Confidence);
            if (rule.MinConfidence is { } minConfidence && confidence < minConfidence)
            {
                var skipped = new RemediationClaim(rule.Id, candidates, ResolveProducedTag(rule, rule.Action.DebugString), confidence)
                {
                    PageIndex = claims[0].PageIndex,
                    Status = ClaimStatus.Skipped,
                    Action = rule.Action,
                    RuleSetId = rule.RuleSetId,
                SlotId = rule.Slot,
                    TextNormalization = rule.TextNormalization ?? TextNormalizationOptions.Default
                };
                skipped.AddRelatedClaims(claims);
                skippedClaims.Add(skipped);
                continue;
            }

            var tableClaim = new RemediationClaim(rule.Id, candidates, ResolveProducedTag(rule, rule.Action.DebugString), confidence)
            {
                PageIndex = claims[0].PageIndex,
                Status = ClaimStatus.Applied,
                SelectorDebugString = rule.Predicate.DebugString,
                Action = rule.Action,
                RuleSetId = rule.RuleSetId,
                SlotId = rule.Slot,
                TextNormalization = rule.TextNormalization ?? TextNormalizationOptions.Default,
                GroupPass = rule.GroupPass
            };
            tableClaim.AddRelatedClaims(claims);
            tableClaim.TablePlan = ResolveClaimConsumingTablePlan(table, tableClaim, grid);
            stageClaims.Add(tableClaim);
        }
    }

    private ClaimPredicateEvaluationContext CreateClaimPredicateContext(
        IReadOnlyList<RemediationClaim> claims,
        IReadOnlyList<RemediationClaim> referenceClaims,
        Rule rule,
        RemediationClaim? previous,
        PageRemediationState page,
        IReadOnlyDictionary<string, RemediationAnchor> anchors,
        IReadOnlyDictionary<string, TolerancedZone> tolerancedZones,
        IReadOnlyDictionary<string, FlowRegion> flowRegions,
        DocumentFlowIndex documentFlows,
        List<string>? diagnostics = null) =>
        new(
            claims,
            PreviousClaim: previous,
            PageBox: page.StructuredText.RelativePageBox,
            Configuration: Configuration,
            Anchors: anchors,
            TolerancedZones: tolerancedZones,
            FlowRegions: flowRegions,
            StructuredText: page.StructuredText,
            Diagnostics: diagnostics ?? new List<string>())
        {
            DocumentFlows = documentFlows,
            ReferenceClaims = referenceClaims,
            EvaluatingRuleId = rule.Id,
            EvaluatingGroupPass = rule.GroupPass
        };

    private static bool ContainsSamePage(ClaimPredicate predicate) =>
        predicate switch
        {
            BuiltInClaimPredicate { Kind: ClaimPredicateKind.SamePage } => true,
            CompositeClaimPredicate composite =>
                ContainsSamePage(composite.Left) || ContainsSamePage(composite.Right),
            NotClaimPredicate not => ContainsSamePage(not.Inner),
            _ => false
        };

    private void AddConsumedInputWarning(
        Rule rule,
        IReadOnlyList<RemediationClaim> frontier,
        IReadOnlyList<RemediationClaim> referenceClaims,
        IReadOnlyList<RemediationClaim> groupClaims,
        List<string> warnings)
    {
        var predicate = rule.Action switch
        {
            GroupRemediationAction group => group.Over,
            MergeRemediationAction merge => merge.Over,
            TableRemediationAction table => table.Over,
            BindTemplateSlotRemediationAction { Over: not null } bind => bind.Over,
            _ => null
        };
        if (predicate == null)
        {
            return;
        }

        var frontierIds = frontier.Select(x => x.ClaimId).ToHashSet();
        void WarnForReference(string descriptor, Func<RemediationClaim, bool> matches)
        {
            var produced = referenceClaims
                .Where(x => x.Status == ClaimStatus.Applied &&
                    rule.Pages.Includes(x.PageIndex, _document.Pages.Count) &&
                    matches(x))
                .ToArray();
            if (produced.Length == 0 || produced.Any(x => frontierIds.Contains(x.ClaimId)))
            {
                return;
            }

            var producedIds = produced.Select(x => x.ClaimId).ToHashSet();
            var consumers = groupClaims
                .Where(x => x.RelatedClaims.Any(y => producedIds.Contains(y.ClaimId)))
                .Select(x => $"{x.RuleId} (pass {x.GroupPass})")
                .Distinct(StringComparer.Ordinal)
                .OrderBy(x => x, StringComparer.Ordinal)
                .ToArray();
            if (consumers.Length == 0)
            {
                return;
            }

            var warning =
                $"Rule '{rule.Id}' in Group pass {rule.GroupPass} matched no frontier claims from " +
                $"referenced producer {descriptor}; its {produced.Length} applied claim(s) were " +
                $"consumed by lower-pass rule(s) [{string.Join(", ", consumers)}]. Select the produced parent instead.";
            if (!warnings.Contains(warning, StringComparer.Ordinal))
            {
                warnings.Add(warning);
            }
        }

        foreach (var referencedRuleId in EnumerateFromRuleReferences(predicate).Distinct(StringComparer.Ordinal))
        {
            WarnForReference($"rule '{referencedRuleId}'", x =>
                string.Equals(x.RuleId, referencedRuleId, StringComparison.Ordinal));
        }
        foreach (var slotId in EnumerateFromSlotReferences(predicate).Distinct(StringComparer.Ordinal))
        {
            WarnForReference($"slot '{slotId}'", x =>
                string.Equals(x.SlotId, slotId, StringComparison.Ordinal));
        }
    }

    private static IEnumerable<string> EnumerateFromSlotReferences(ClaimPredicate predicate)
    {
        switch (predicate)
        {
            case BuiltInClaimPredicate { Kind: ClaimPredicateKind.FromSlot, Value: not null } builtIn:
                yield return builtIn.Value;
                break;
            case CompositeClaimPredicate composite:
                foreach (var value in EnumerateFromSlotReferences(composite.Left)) yield return value;
                foreach (var value in EnumerateFromSlotReferences(composite.Right)) yield return value;
                break;
            case NotClaimPredicate not:
                foreach (var value in EnumerateFromSlotReferences(not.Inner)) yield return value;
                break;
        }
    }

    private static IEnumerable<string> EnumerateFromRuleReferences(ClaimPredicate predicate)
    {
        switch (predicate)
        {
            case BuiltInClaimPredicate { Kind: ClaimPredicateKind.FromRule, Value: not null } builtIn:
                yield return builtIn.Value;
                break;
            case CompositeClaimPredicate composite:
                foreach (var value in EnumerateFromRuleReferences(composite.Left)) yield return value;
                foreach (var value in EnumerateFromRuleReferences(composite.Right)) yield return value;
                break;
            case NotClaimPredicate not:
                foreach (var value in EnumerateFromRuleReferences(not.Inner)) yield return value;
                break;
        }
    }

    private void EvaluateRefineAttributeRule(
        PageRemediationState pageState,
        Rule rule,
        StructureAttributeRemediationAction attributes,
        RemediationEvaluationContext context,
        List<RemediationClaim> stageClaims,
        List<RemediationClaim> skippedClaims,
        RuleEvaluationAccumulator evaluations)
    {
        var existingClaims = pageState.GetClaimSnapshot(Stage.Classify)
            .Concat(pageState.GetClaimSnapshot(Stage.Group))
            .Concat(pageState.GetClaimSnapshot(Stage.Refine))
            .Where(x => x.Status == ClaimStatus.Applied)
            .OrderBy(x => x, ReadingOrderComparer)
            .ToList();
        RemediationClaim? previous = null;
        var matched = 0;
        var rejectedByConfidence = 0;
        foreach (var claim in existingClaims)
        {
            var predicateContext = new ClaimPredicateEvaluationContext(
                existingClaims,
                PreviousClaim: previous,
                PageBox: context.PageBox,
                Configuration: context.Configuration,
                Anchors: context.Anchors,
                TolerancedZones: context.TolerancedZones,
                FlowRegions: context.FlowRegions,
                StructuredText: context.StructuredText,
                Diagnostics: context.Diagnostics)
            {
                DocumentFlows = context.DocumentFlows
            };
            previous = claim;
            var result = attributes.Over.Evaluate(predicateContext, claim);
            if (!result.IsMatch)
            {
                continue;
            }

            matched++;
            if (rule.MinConfidence is { } minConfidence && result.Confidence < minConfidence)
            {
                rejectedByConfidence++;
                var skipped = new RemediationClaim(
                    rule.Id,
                    claim.Candidates,
                    rule.Action.DebugString,
                    result.Confidence)
                {
                    PageIndex = pageState.PageIndex,
                    Status = ClaimStatus.Skipped,
                    Action = rule.Action,
                    RuleSetId = rule.RuleSetId,
                SlotId = rule.Slot,
            TextNormalization = rule.TextNormalization ?? TextNormalizationOptions.Default
                };
                skipped.AddRelatedClaims(new[] { claim });
                skippedClaims.Add(skipped);
                continue;
            }

            var refineClaim = new RemediationClaim(
                rule.Id,
                claim.Candidates,
                rule.Action.DebugString,
                result.Confidence)
            {
                PageIndex = pageState.PageIndex,
                Status = ClaimStatus.Applied,
            SelectorDebugString = rule.Predicate.DebugString,
                Action = rule.Action,
                RuleSetId = rule.RuleSetId,
                SlotId = rule.Slot,
            TextNormalization = rule.TextNormalization ?? TextNormalizationOptions.Default
            };
            refineClaim.AddRelatedClaims(new[] { claim });
            stageClaims.Add(refineClaim);
        }

        evaluations.Record(
            rule,
            pageState.PageIndex,
            considered: existingClaims.Count,
            matched: matched,
            rejectedByConfidence: rejectedByConfidence);
    }

    private void EvaluateRefineReorderRule(
        PageRemediationState pageState,
        Rule rule,
        ReorderSiblingsRemediationAction reorder,
        RemediationEvaluationContext context,
        List<RemediationClaim> stageClaims,
        List<RemediationClaim> skippedClaims,
        RuleEvaluationAccumulator evaluations)
    {
        var existingClaims = pageState.GetClaimSnapshot(Stage.Classify)
            .Concat(pageState.GetClaimSnapshot(Stage.Group))
            .Concat(pageState.GetClaimSnapshot(Stage.Refine))
            .Where(x => x.Status == ClaimStatus.Applied)
            .OrderBy(x => x, ReadingOrderComparer)
            .ToList();
        var matched = new List<RemediationClaim>();
        var confidence = 1.0;
        var matchedCount = 0;
        var rejectedByConfidence = 0;
        RemediationClaim? previous = null;
        foreach (var claim in existingClaims)
        {
            var predicateContext = new ClaimPredicateEvaluationContext(
                existingClaims,
                PreviousClaim: previous,
                PageBox: context.PageBox,
                Configuration: context.Configuration,
                Anchors: context.Anchors,
                TolerancedZones: context.TolerancedZones,
                FlowRegions: context.FlowRegions,
                StructuredText: context.StructuredText,
                Diagnostics: context.Diagnostics)
            {
                DocumentFlows = context.DocumentFlows
            };
            previous = claim;
            var result = reorder.Over.Evaluate(predicateContext, claim);
            if (!result.IsMatch)
            {
                continue;
            }

            matchedCount++;
            if (rule.MinConfidence is { } minConfidence && result.Confidence < minConfidence)
            {
                rejectedByConfidence++;
                var skipped = new RemediationClaim(
                    rule.Id,
                    claim.Candidates,
                    rule.Action.DebugString,
                    result.Confidence)
                {
                    PageIndex = pageState.PageIndex,
                    Status = ClaimStatus.Skipped,
                    Action = rule.Action,
                    RuleSetId = rule.RuleSetId,
                SlotId = rule.Slot,
            TextNormalization = rule.TextNormalization ?? TextNormalizationOptions.Default
                };
                skipped.AddRelatedClaims(new[] { claim });
                skippedClaims.Add(skipped);
                continue;
            }

            matched.Add(claim);
            confidence = Math.Min(confidence, result.Confidence);
        }

        evaluations.Record(
            rule,
            pageState.PageIndex,
            considered: existingClaims.Count,
            matched: matchedCount,
            rejectedByConfidence: rejectedByConfidence);
        if (matched.Count == 0)
        {
            return;
        }

        var refineClaim = new RemediationClaim(
            rule.Id,
            matched.SelectMany(x => x.Candidates).ToArray(),
            rule.Action.DebugString,
            confidence)
        {
            PageIndex = pageState.PageIndex,
            Status = ClaimStatus.Applied,
            SelectorDebugString = rule.Predicate.DebugString,
            Action = rule.Action,
            RuleSetId = rule.RuleSetId,
                SlotId = rule.Slot,
            TextNormalization = rule.TextNormalization ?? TextNormalizationOptions.Default
        };
        refineClaim.AddRelatedClaims(matched);
        stageClaims.Add(refineClaim);
    }

    private void EvaluateRefineLinkRule(
        PageRemediationState pageState,
        Rule rule,
        StructureLinkRemediationAction link,
        RemediationEvaluationContext context,
        List<RemediationClaim> stageClaims,
        List<RemediationClaim> skippedClaims,
        List<string> diagnostics,
        RuleEvaluationAccumulator evaluations)
    {
        var existingClaims = pageState.GetClaimSnapshot(Stage.Classify)
            .Concat(pageState.GetClaimSnapshot(Stage.Group))
            .Concat(pageState.GetClaimSnapshot(Stage.Refine))
            .Where(x => x.Status == ClaimStatus.Applied)
            .OrderBy(x => x, ReadingOrderComparer)
            .ToList();
        var sources = new List<(RemediationClaim Claim, double Confidence)>();
        var targets = new List<(RemediationClaim Claim, double Confidence)>();
        var matchedSources = 0;
        var rejectedByConfidence = 0;
        RemediationClaim? previous = null;
        foreach (var claim in existingClaims)
        {
            var predicateContext = new ClaimPredicateEvaluationContext(
                existingClaims,
                PreviousClaim: previous,
                PageBox: context.PageBox,
                Configuration: context.Configuration,
                Anchors: context.Anchors,
                TolerancedZones: context.TolerancedZones,
                FlowRegions: context.FlowRegions,
                StructuredText: context.StructuredText,
                Diagnostics: context.Diagnostics)
            {
                DocumentFlows = context.DocumentFlows
            };
            previous = claim;

            var sourceResult = link.Source.Evaluate(predicateContext, claim);
            if (sourceResult.IsMatch)
            {
                matchedSources++;
                if (rule.MinConfidence is { } minConfidence && sourceResult.Confidence < minConfidence)
                {
                    rejectedByConfidence++;
                    AddSkippedClaim(rule, pageState.PageIndex, claim, sourceResult.Confidence, skippedClaims);
                }
                else
                {
                    sources.Add((claim, sourceResult.Confidence));
                }
            }

            var targetResult = link.Target.Evaluate(predicateContext, claim);
            if (targetResult.IsMatch)
            {
                targets.Add((claim, targetResult.Confidence));
            }
        }

        evaluations.Record(
            rule,
            pageState.PageIndex,
            considered: existingClaims.Count,
            matched: matchedSources,
            rejectedByConfidence: rejectedByConfidence);
        if (sources.Count == 0)
        {
            return;
        }

        if (targets.Count == 0)
        {
            diagnostics.Add($"Rule '{rule.Id}' on page {pageState.PageIndex + 1} matched link source claims but no destination claim.");
            return;
        }

        var target = targets[0];
        foreach (var source in sources)
        {
            var confidence = Math.Min(source.Confidence, target.Confidence);
            if (rule.MinConfidence is { } minConfidence && confidence < minConfidence)
            {
                evaluations.Record(rule, pageState.PageIndex, rejectedByConfidence: 1);
                AddSkippedClaim(rule, pageState.PageIndex, source.Claim, confidence, skippedClaims);
                continue;
            }

            var linkClaim = new RemediationClaim(
                rule.Id,
                source.Claim.Candidates,
                rule.Action.DebugString,
                confidence)
            {
                PageIndex = pageState.PageIndex,
                Status = ClaimStatus.Applied,
            SelectorDebugString = rule.Predicate.DebugString,
                Action = rule.Action,
                RuleSetId = rule.RuleSetId,
                SlotId = rule.Slot,
            TextNormalization = rule.TextNormalization ?? TextNormalizationOptions.Default
            };
            linkClaim.AddRelatedClaims(new[] { source.Claim, target.Claim });
            stageClaims.Add(linkClaim);
        }
    }

    private void AddSkippedClaim(
        Rule rule,
        int pageIndex,
        RemediationClaim related,
        double confidence,
        List<RemediationClaim> skippedClaims)
    {
        var skipped = new RemediationClaim(
            rule.Id,
            related.Candidates,
            rule.Action.DebugString,
            confidence)
        {
            PageIndex = pageIndex,
            Status = ClaimStatus.Skipped,
            Action = rule.Action,
            RuleSetId = rule.RuleSetId,
                SlotId = rule.Slot,
            TextNormalization = rule.TextNormalization ?? TextNormalizationOptions.Default
        };
        skipped.AddRelatedClaims(new[] { related });
        skippedClaims.Add(skipped);
    }

    private void AddGroupRun(
        Rule rule,
        int pageIndex,
        IReadOnlyList<RemediationClaim> run,
        List<RemediationClaim> stageClaims)
    {
        if (run.Count == 0)
        {
            return;
        }

        var confidence = run.Min(x => x.Confidence);
        var groupClaim = new RemediationClaim(
            rule.Id,
            run.SelectMany(x => x.Candidates).ToArray(),
            ResolveProducedTag(rule, rule.Action.DebugString),
            confidence)
        {
            PageIndex = pageIndex,
            Status = ClaimStatus.Applied,
            SelectorDebugString = rule.Predicate.DebugString,
            Action = rule.Action,
            RuleSetId = rule.RuleSetId,
                SlotId = rule.Slot,
            TextNormalization = rule.TextNormalization ?? TextNormalizationOptions.Default,
            GroupPass = rule.GroupPass
        };
        groupClaim.AddRelatedClaims(run);
        stageClaims.Add(groupClaim);
    }

    private void ValidatePlan(
        IReadOnlyList<PageRemediationState> pageStates,
        List<string> diagnostics)
    {
        var pageLookup = pageStates.ToDictionary(x => x.PageIndex);
        var groupClaims = pageStates
            .SelectMany(x => x.GetClaimSnapshot(Stage.Group))
            .Where(x => x.Status == ClaimStatus.Applied)
            .DistinctBy(x => x.ClaimId)
            .ToArray();
        ValidateGroupComposition(groupClaims, diagnostics);

        foreach (var pageState in pageStates)
        {
            foreach (var claim in pageState.GetClaimSnapshot(Stage.Classify).Where(x => x.Status == ClaimStatus.Applied))
            {
                ValidateClaimTargets(pageState, claim, diagnostics);
                ValidateClaimStructureBinding(pageState, claim, diagnostics);
            }

            foreach (var claim in pageState.GetClaimSnapshot(Stage.Group).Where(x => x.Status == ClaimStatus.Applied))
            {
                ValidateTableClaim(pageLookup, claim, diagnostics);
                ValidateClaimConsumingBindings(pageState, claim, diagnostics);
            }

            foreach (var claim in pageState.GetClaimSnapshot(Stage.Refine).Where(x => x.Status == ClaimStatus.Applied))
            {
                ValidateClaimConsumingBindings(pageState, claim, diagnostics);
            }
        }
    }

    internal void ValidateGroupComposition(
        IReadOnlyList<RemediationClaim> groupClaims,
        List<string> diagnostics)
    {
        var structuralClaims = groupClaims.Where(IsStructuralConsumer).ToArray();
        var reportedClaims = new HashSet<ClaimId>();
        foreach (var consumers in structuralClaims
                     .SelectMany(parent => parent.RelatedClaims.Select(child => (parent, child)))
                     .GroupBy(x => x.child.ClaimId)
                     .Where(x => x.Count() > 1))
        {
            if (!reportedClaims.Add(consumers.Key))
            {
                continue;
            }

            var rules = consumers.Select(x => $"{x.parent.RuleId} (pass {x.parent.GroupPass})")
                .Distinct(StringComparer.Ordinal);
            ReportDiagnostic(
                DiagnosticCode.GroupCompositionAmbiguous,
                "GroupComposition",
                $"Claim '{consumers.Key}' has multiple structural consumers [{string.Join(", ", rules)}].",
                diagnostics);
        }

        foreach (var parent in structuralClaims)
        {
            for (var i = 0; i < parent.RelatedClaims.Count; i++)
            {
                for (var j = i + 1; j < parent.RelatedClaims.Count; j++)
                {
                    var left = parent.RelatedClaims[i];
                    var right = parent.RelatedClaims[j];
                    if (!IsDescendant(left, right, new HashSet<ClaimId>()) &&
                        !IsDescendant(right, left, new HashSet<ClaimId>()))
                    {
                        continue;
                    }

                    ReportDiagnostic(
                        DiagnosticCode.GroupCompositionAmbiguous,
                        "GroupComposition",
                        $"Rule '{parent.RuleId}' in Group pass {parent.GroupPass} selects both an ancestor " +
                        $"and descendant claim ('{left.ClaimId}', '{right.ClaimId}').",
                        diagnostics);
                }
            }
        }

        var states = new Dictionary<ClaimId, int>();
        var path = new List<RemediationClaim>();
        foreach (var claim in structuralClaims)
        {
            if (DetectCompositionCycle(claim, states, path, out var cycle))
            {
                ReportDiagnostic(
                    DiagnosticCode.GroupCompositionCycle,
                    "GroupComposition",
                    $"Structural claim cycle detected: {string.Join(" -> ", cycle.Select(x => $"{x.RuleId}:{x.ClaimId}"))}.",
                    diagnostics);
                break;
            }
        }
    }

    private static bool IsStructuralConsumer(RemediationClaim claim) =>
        claim.Action is GroupRemediationAction or
            BindTemplateSlotRemediationAction { Over: not null } or
            MergeRemediationAction or
            TableRemediationAction { Over: not null };

    private static bool IsDescendant(
        RemediationClaim ancestor,
        RemediationClaim candidate,
        HashSet<ClaimId> visited)
    {
        if (!visited.Add(ancestor.ClaimId))
        {
            return false;
        }
        if (ancestor.RelatedClaims.Any(x => x.ClaimId == candidate.ClaimId))
        {
            return true;
        }
        return ancestor.RelatedClaims.Any(x => IsDescendant(x, candidate, visited));
    }

    private static bool DetectCompositionCycle(
        RemediationClaim claim,
        Dictionary<ClaimId, int> states,
        List<RemediationClaim> path,
        out IReadOnlyList<RemediationClaim> cycle)
    {
        if (states.TryGetValue(claim.ClaimId, out var state))
        {
            if (state == 1)
            {
                var start = path.FindIndex(x => x.ClaimId == claim.ClaimId);
                cycle = path.Skip(Math.Max(0, start)).Append(claim).ToArray();
                return true;
            }
            cycle = Array.Empty<RemediationClaim>();
            return false;
        }

        states[claim.ClaimId] = 1;
        path.Add(claim);
        foreach (var child in claim.RelatedClaims)
        {
            if (DetectCompositionCycle(child, states, path, out cycle))
            {
                return true;
            }
        }
        path.RemoveAt(path.Count - 1);
        states[claim.ClaimId] = 2;
        cycle = Array.Empty<RemediationClaim>();
        return false;
    }

    private static void ValidateClaimTargets(
        PageRemediationState pageState,
        RemediationClaim claim,
        List<string> diagnostics)
    {
        if (claim.Action is not (TagRemediationAction or ArtifactRemediationAction or BindTemplateSlotRemediationAction))
        {
            return;
        }

        var allTargets = new List<RemediationClaimTarget<double>>();
        foreach (var candidate in claim.Candidates)
        {
            foreach (var sourceReference in candidate.SourceReferences)
            {
                if (!ContentModelBridge.TryResolveParsedItemId(pageState.WorkingContent, sourceReference, out _))
                {
                    diagnostics.Add($"Rule '{claim.RuleId}' on page {pageState.PageIndex + 1} references content source {sourceReference} that no longer resolves.");
                }
            }

            var targets = candidate.FindTargets(pageState.WorkingContent);
            if (targets.Count == 0)
            {
                diagnostics.Add($"Rule '{claim.RuleId}' on page {pageState.PageIndex + 1} matched content but resolved no leaves.");
                continue;
            }

            ValidateTextRangeTargets(pageState, claim, targets, diagnostics);
            allTargets.AddRange(targets);
        }

        if (allTargets.Count > 0)
        {
            ValidateContiguousTargets(pageState, claim, allTargets.Select(x => x.Item), diagnostics);
        }
    }

    private static void ValidateTextRangeTargets(
        PageRemediationState pageState,
        RemediationClaim claim,
        IReadOnlyList<RemediationClaimTarget<double>> targets,
        List<string> diagnostics)
    {
        foreach (var target in targets)
        {
            if (target.TextRange == null)
            {
                continue;
            }

            if (target.Item is not TextContent<double> textContent)
            {
                diagnostics.Add($"Rule '{claim.RuleId}' on page {pageState.PageIndex + 1} has a text range target that is not text content.");
                continue;
            }

            if (!textContent.TrySplitByCharacterRange(
                target.TextRange.StartCharacterIndex - textContent.SourceCharacterOffset,
                target.TextRange.CharacterCount,
                out _,
                out _,
                out _,
                out var error))
            {
                diagnostics.Add($"Rule '{claim.RuleId}' on page {pageState.PageIndex + 1} cannot materialize text range {target.TextRange.StartCharacterIndex}+{target.TextRange.CharacterCount}: {error}");
            }
        }
    }

    private static void ValidateContiguousTargets(
        PageRemediationState pageState,
        RemediationClaim claim,
        IEnumerable<IContentItem<double>> targetItems,
        List<string> diagnostics)
    {
        var selectedItems = targetItems.Distinct().ToList();
        if (selectedItems.Count <= 1)
        {
            return;
        }

        var itemIndexes = FlattenItems(pageState.WorkingContent)
            .Select((item, index) => (item, index))
            .ToDictionary(x => x.item, x => x.index);
        var indexes = new List<int>();
        foreach (var item in selectedItems)
        {
            if (!itemIndexes.TryGetValue(item, out var index))
            {
                diagnostics.Add($"Rule '{claim.RuleId}' on page {pageState.PageIndex + 1} selected a leaf that does not belong to the page tree.");
                return;
            }

            indexes.Add(index);
        }

        indexes.Sort();
        if (indexes[^1] - indexes[0] + 1 != indexes.Count)
        {
            diagnostics.Add($"Rule '{claim.RuleId}' on page {pageState.PageIndex + 1} selected non-contiguous leaves.");
        }
    }

    private static void ValidateClaimStructureBinding(
        PageRemediationState pageState,
        RemediationClaim claim,
        List<string> diagnostics)
    {
        if (claim.Action is TagRemediationAction &&
            claim.Candidates.Count == 0)
        {
            diagnostics.Add($"Rule '{claim.RuleId}' on page {pageState.PageIndex + 1} would allocate an MCID without a content target.");
        }
    }

    private static void ValidateTableClaim(
        IReadOnlyDictionary<int, PageRemediationState> pageStates,
        RemediationClaim claim,
        List<string> diagnostics)
    {
        if (claim.Action is not TableRemediationAction table)
        {
            return;
        }

        var grid = ResolveTableGrid(table, claim.Candidates);
        if (grid.Error != null)
        {
            diagnostics.Add($"Rule '{claim.RuleId}' could not resolve table columns: {grid.Error}");
            return;
        }

        if (table.Over != null && table.CellContentMode == TableCellContentMode.PreserveChildren)
        {
            foreach (var related in claim.RelatedClaims.Where(x =>
                         x.ProducedTag is "TR" or "TH" or "TD"))
            {
                diagnostics.Add(
                    $"Rule '{claim.RuleId}' cannot preserve " +
                    $"claim '{related.ClaimId}' from rule '{related.RuleId}' with produced tag " +
                    $"'{related.ProducedTag}' beneath a generated table cell. Classify leaf cell content " +
                    "and use TableOverFlattenedCells instead.");
            }
        }

        foreach (var candidate in claim.Candidates)
        {
            if (!pageStates.TryGetValue(candidate.PageIndex, out var pageState))
            {
                diagnostics.Add(
                    $"Rule '{claim.RuleId}' references a table candidate without a valid owning page.");
                continue;
            }

            foreach (var sourceReference in candidate.SourceReferences)
            {
                if (!ContentModelBridge.TryResolveParsedItemId(pageState.WorkingContent, sourceReference, out _))
                {
                    diagnostics.Add($"Rule '{claim.RuleId}' on page {pageState.PageIndex + 1} references content source {sourceReference} that no longer resolves.");
                }
            }

            var centerX = (candidate.RelativeBoundingBox.LLx + candidate.RelativeBoundingBox.URx) / 2d;
            if (GetColumnIndex(grid.Columns, centerX) < 0)
            {
                diagnostics.Add($"Rule '{claim.RuleId}' on page {pageState.PageIndex + 1} selected content outside the declared table columns.");
            }

            var targets = candidate.FindTargets(pageState.WorkingContent);
            if (targets.Count == 0)
            {
                diagnostics.Add($"Rule '{claim.RuleId}' on page {pageState.PageIndex + 1} matched table content but resolved no leaves.");
            }

            ValidateTextRangeTargets(pageState, claim, targets, diagnostics);
            ValidateContiguousTargets(pageState, claim, targets.Select(x => x.Item), diagnostics);
        }
    }

    private static void ValidateClaimConsumingBindings(
        PageRemediationState pageState,
        RemediationClaim claim,
        List<string> diagnostics)
    {
        if (claim.Action is not (GroupRemediationAction or BindTemplateSlotRemediationAction or MergeRemediationAction or TableRemediationAction { Over: not null } or StructureAttributeRemediationAction or ReorderSiblingsRemediationAction or StructureLinkRemediationAction))
        {
            return;
        }

        if (claim.RelatedClaims.Count == 0)
        {
            diagnostics.Add($"Rule '{claim.RuleId}' on page {pageState.PageIndex + 1} matched no existing claims.");
            return;
        }

        foreach (var related in claim.RelatedClaims)
        {
            if (related.Action is not (TagRemediationAction or BindTemplateSlotRemediationAction or TableRemediationAction or GroupRemediationAction or MergeRemediationAction or AdoptAnnotationRemediationAction { Into: null }))
            {
                diagnostics.Add($"Rule '{claim.RuleId}' on page {pageState.PageIndex + 1} matched claim '{related.ClaimId}' that does not produce a structure binding.");
            }
        }
    }

    private void ApplyPlan(
        IReadOnlyList<PageRemediationState> pageStates,
        List<string> diagnostics,
        PrescriptiveTemplateAssemblyPlan? assemblyPlan)
    {
        var pageLookup = pageStates.ToDictionary(x => x.PageIndex);

        // Materialize every ordinary leaf in the document before annotations attach to source or
        // destination bindings that may live on another page.
        foreach (var pageState in pageStates)
        {
            foreach (var claim in pageState.GetClaimSnapshot(Stage.Classify)
                         .Where(x => x.Status == ClaimStatus.Applied && x.Action is not AdoptAnnotationRemediationAction))
            {
                ApplyClassifyClaim(pageState, claim, diagnostics);
                if (HasUnsuppressedDiagnostics(diagnostics)) return;
            }
        }
        foreach (var pageState in pageStates)
        {
            foreach (var claim in pageState.GetClaimSnapshot(Stage.Classify)
                         .Where(x => x.Status == ClaimStatus.Applied && x.Action is AdoptAnnotationRemediationAction))
            {
                ApplyAdoptAnnotationClaim(pageState, claim, diagnostics);
                if (HasUnsuppressedDiagnostics(diagnostics)) return;
            }
        }

        var orderedGroupClaims = pageStates
            .SelectMany(x => x.GetClaimSnapshot(Stage.Group))
            .Where(x => x.Status == ClaimStatus.Applied)
            .OrderBy(x => x.GroupPass)
            .ThenBy(x => x, ReadingOrderComparer);
        foreach (var claim in orderedGroupClaims)
        {
            var pageState = pageLookup[claim.PageIndex];
            if (claim.Action is TableRemediationAction)
            {
                ApplyTableClaim(pageState, claim, diagnostics);
            }
            else if (claim.Action is GroupRemediationAction or BindTemplateSlotRemediationAction)
            {
                ApplyGroupClaim(claim, diagnostics);
            }
            else if (claim.Action is MergeRemediationAction)
            {
                ApplyMergeClaim(pageState, claim, diagnostics);
            }

            foreach (var pageIndex in claim.PageIndexes)
            {
                if (pageLookup.TryGetValue(pageIndex, out var owningPage))
                {
                    owningPage.MarkDirty();
                }
            }

            if (HasUnsuppressedDiagnostics(diagnostics))
            {
                return;
            }
        }

        if (assemblyPlan != null)
        {
            ApplyPrescriptiveTemplateAssembly(assemblyPlan, pageStates, diagnostics);
            if (HasUnsuppressedDiagnostics(diagnostics)) return;
        }

        foreach (var pageState in pageStates)
        {
            foreach (var claim in pageState.GetClaimSnapshot(Stage.Refine).Where(x => x.Status == ClaimStatus.Applied))
            {
                if (claim.Action is StructureAttributeRemediationAction)
                {
                    ApplyStructureAttributeClaim(claim, diagnostics);
                }
                else if (claim.Action is ReorderSiblingsRemediationAction)
                {
                    ApplyReorderSiblingsClaim(claim, diagnostics);
                }
                else if (claim.Action is StructureLinkRemediationAction)
                {
                    ApplyStructureLinkClaim(pageState, claim, diagnostics);
                }

                if (HasUnsuppressedDiagnostics(diagnostics))
                {
                    return;
                }
            }
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
                pageState.Page.NativeObject[PdfName.Contents] = PdfIndirectRef.Create(new PdfStream(contents));
            }
        }
    }

    private bool IsPrescriptiveTemplate() =>
        _program != null ||
        _ruleSets.Select(x => x.StructuralTemplate).FirstOrDefault(x => x != null)?.Mode ==
        RemediationStructuralTemplateMode.Prescriptive;

    private IReadOnlyList<RemediationClaim> BuildPrescriptiveTemplateClaims(
        IReadOnlyList<RemediationClaim> existingClaims,
        IReadOnlyList<Rule> rules)
    {
        var template = _ruleSets.Select(x => x.StructuralTemplate).FirstOrDefault(x => x != null);
        if (template?.Mode != RemediationStructuralTemplateMode.Prescriptive)
        {
            return Array.Empty<RemediationClaim>();
        }

        var slots = rules.GroupBy(x => (x.RuleSetId, x.Id))
            .ToDictionary(x => x.Key, x => x.Last().Slot);
        var actions = rules.GroupBy(x => (x.RuleSetId, x.Id))
            .ToDictionary(x => x.Key, x => x.Last().Action);
        var source = RemediationSemanticTree.FromClaims(existingClaims, slots, actions);
        var claimsById = existingClaims.GroupBy(x => x.ClaimId)
            .ToDictionary(x => x.Key, x => x.First());
        var plan = PrescriptiveTemplateAssemblyPlan.Build(template, source, claimsById);
        var synthetic = new Dictionary<string, RemediationClaim>(StringComparer.Ordinal);
        var templateOwnerId = _ruleSets.FirstOrDefault(x =>
            ReferenceEquals(x.StructuralTemplate, template))?.Id ?? string.Empty;

        RemediationClaim? Build(PrescriptiveTemplateAssemblyNode occurrence)
        {
            if (occurrence.Template == null || occurrence.Source != null) return null;
            var children = new List<RemediationClaim>();
            foreach (var child in occurrence.Children)
            {
                if (child.ClaimId is { } childId && claimsById.TryGetValue(childId, out var direct))
                {
                    children.Add(direct);
                }
                else if (Build(child) is { } created)
                {
                    children.Add(created);
                }
            }
            var pages = children.SelectMany(x => x.PageIndexes).Distinct().OrderBy(x => x).ToArray();
            var pageIndex = pages.FirstOrDefault();
            var claim = new RemediationClaim(
                $"__template__:{occurrence.Template.Id}:{occurrence.OccurrenceIndex}",
                Array.Empty<RemediationCandidate>(),
                occurrence.Template.Tag,
                children.Count == 0 ? 1 : children.Min(x => x.Confidence))
            {
                ClaimId = new ClaimId(occurrence.Identity),
                PageIndex = pageIndex,
                Status = ClaimStatus.Applied,
                Action = new BindTemplateSlotRemediationAction(),
                RuleSetId = templateOwnerId,
                SlotId = occurrence.Template.Id,
                SelectorDebugString = "template"
            };
            claim.AddRelatedClaims(children);
            synthetic[occurrence.Identity] = claim;
            claimsById[claim.ClaimId] = claim;
            return claim;
        }

        foreach (var root in plan.Document.Children) Build(root);
        return synthetic.Values.OrderBy(x => x.PageIndex).ThenBy(x => x.ClaimId.Value, StringComparer.Ordinal).ToArray();
    }

    private void ApplyPrescriptiveTemplateAssembly(
        PrescriptiveTemplateAssemblyPlan plan,
        IReadOnlyList<PageRemediationState> pageStates,
        List<string> diagnostics,
        IReadOnlyList<RemediationClaim>? executionClaims = null)
    {
        var claims = (executionClaims ?? pageStates
            .SelectMany(x => x.GetClaimSnapshot(Stage.Classify)
                .Concat(x.GetClaimSnapshot(Stage.Group))
                .Concat(x.GetClaimSnapshot(Stage.Refine))))
            .Where(x => x.Status == ClaimStatus.Applied)
            .DistinctBy(x => x.ClaimId)
            .ToArray();
        var claimsById = claims.ToDictionary(x => x.ClaimId);
        var syntheticBySlot = claims
            .Where(x => x.RuleId.StartsWith("__template__:", StringComparison.Ordinal) && x.SlotId != null)
            .GroupBy(x => x.SlotId!, StringComparer.Ordinal)
            .ToDictionary(x => x.Key, x => new Queue<RemediationClaim>(x), StringComparer.Ordinal);
        var existingIds = new Dictionary<string, StructureNode>(StringComparer.Ordinal);

        void IndexExisting(StructureNode node)
        {
            if (node.ID != null)
            {
                if (node.ID.StartsWith("template:", StringComparison.Ordinal))
                {
                    ReportDiagnostic(
                        DiagnosticCode.TemplateIdentityCollision,
                        $"TemplateIdentity:{node.ID}",
                        $"Pre-existing structure /ID '{node.ID}' uses the reserved prescriptive-template namespace.",
                        diagnostics);
                }
                else if (!existingIds.TryAdd(node.ID, node))
                {
                    ReportDiagnostic(
                        DiagnosticCode.TemplateIdentityCollision,
                        $"TemplateIdentity:{node.ID}",
                        $"Structure /ID '{node.ID}' is already duplicated before template assembly.",
                        diagnostics);
                }
            }
            foreach (var child in node.Children) IndexExisting(child);
        }

        var documentRoot = Structure.GetRoot();
        var documentProperties = plan.Document.Template?.Properties ?? plan.Document.ProgramTemplate?.Properties;
        documentRoot.Language = documentProperties?.Language ?? documentRoot.Language;
        documentRoot.Alt = documentProperties?.AlternateText ?? documentRoot.Alt;
        documentRoot.ActualText = documentProperties?.ActualText ?? documentRoot.ActualText;
        documentRoot.Expansion = documentProperties?.Expansion ?? documentRoot.Expansion;
        IndexExisting(documentRoot);
        if (HasUnsuppressedDiagnostics(diagnostics)) return;

        IReadOnlyList<StructureNode> MaterializeChildren(
            IReadOnlyList<PrescriptiveTemplateAssemblyNode> planned,
            StructureNode parent)
        {
            var ordered = new List<StructureNode>();
            foreach (var occurrence in planned)
            {
                if (occurrence.Template == null && occurrence.ProgramTemplate == null) continue;
                var occurrenceTag = occurrence.Template?.Tag ?? occurrence.ProgramTemplate!.Tag;
                var occurrenceProperties = occurrence.Template?.Properties ?? occurrence.ProgramTemplate!.Properties;
                var occurrenceSlotId = occurrence.Template?.Id ?? occurrence.SlotReference?.Path[1..];

                RemediationClaim? claim = null;
                if (occurrence.ClaimId is { } claimId) claimsById.TryGetValue(claimId, out claim);
                if (claim == null) claimsById.TryGetValue(new ClaimId(occurrence.Identity), out claim);
                if (claim == null && syntheticBySlot.TryGetValue(occurrenceSlotId!, out var queue) && queue.Count > 0)
                {
                    claim = queue.Dequeue();
                }

                var bindings = claim?.AppliedBindings.Where(x =>
                        x.StructureNode != null &&
                        string.Equals(x.ProducedTag, occurrenceTag, StringComparison.Ordinal))
                    .ToArray() ?? Array.Empty<RemediationAppliedBinding>();
                if (bindings.Length > 1)
                {
                    diagnostics.Add($"Template occurrence '{occurrence.Identity}' has {bindings.Length} structure bindings; exactly one is required.");
                    return ordered;
                }

                StructureNode node;
                if (bindings.Length == 1)
                {
                    node = bindings[0].StructureNode!;
                }
                else if (occurrence.Source == null)
                {
                    node = Structure.AddElement(occurrenceTag).GetNode();
                    claim?.AddAppliedBinding(new RemediationAppliedBinding(
                        occurrenceTag,
                        Array.Empty<int>(),
                        node,
                        null,
                        parent,
                        Array.Empty<StructuredSourceRef>(),
                        null));
                }
                else
                {
                    diagnostics.Add($"Bound template occurrence '{occurrence.Identity}' has no materialized structure node.");
                    return ordered;
                }

                if (node.ID != null && !string.Equals(node.ID, occurrence.Identity, StringComparison.Ordinal))
                {
                    ReportDiagnostic(
                        DiagnosticCode.TemplateIdentityCollision,
                        $"TemplateIdentity:{occurrence.Identity}",
                        $"Template occurrence '{occurrence.Identity}' would overwrite existing structure /ID '{node.ID}'.",
                        diagnostics);
                    return ordered;
                }
                if (existingIds.TryGetValue(occurrence.Identity, out var owner) && !ReferenceEquals(owner, node))
                {
                    ReportDiagnostic(
                        DiagnosticCode.TemplateIdentityCollision,
                        $"TemplateIdentity:{occurrence.Identity}",
                        $"Template occurrence identity '{occurrence.Identity}' is already owned by another structure node.",
                        diagnostics);
                    return ordered;
                }
                node.ID = occurrence.Identity;
                existingIds[occurrence.Identity] = node;
                node.Language = occurrenceProperties.Language ?? node.Language;
                node.Alt = occurrenceProperties.AlternateText ?? node.Alt;
                node.ActualText = occurrenceProperties.ActualText ?? node.ActualText;
                node.Expansion = occurrenceProperties.Expansion ?? node.Expansion;
                if (!ReferenceEquals(node.Parent, parent)) Structure.ReparentStructureNode(node, parent);

                if (!occurrence.OpaqueInterior)
                {
                    var childOrder = MaterializeChildren(occurrence.Children, node);
                    Reorder(node, childOrder);
                }
                ordered.Add(node);
            }
            return ordered;
        }

        static void Reorder(StructureNode parent, IReadOnlyList<StructureNode> desired)
        {
            if (desired.Count == 0) return;
            var desiredSet = desired.ToHashSet(ReferenceEqualityComparer.Instance);
            var stableExtras = parent.Children.Where(x => !desiredSet.Contains(x)).ToArray();
            parent.Children.Clear();
            parent.Children.AddRange(desired);
            parent.Children.AddRange(stableExtras);
        }

        var rootOrder = MaterializeChildren(plan.Document.Children, documentRoot);
        Reorder(documentRoot, rootOrder);
    }

    private void ApplyTableClaim(
        PageRemediationState pageState,
        RemediationClaim claim,
        List<string> diagnostics)
    {
        if (claim.Action is not TableRemediationAction table)
        {
            return;
        }

        var grid = ResolveTableGrid(table, claim.Candidates);
        if (grid.Error != null)
        {
            diagnostics.Add($"Rule '{claim.RuleId}' could not resolve table columns: {grid.Error}");
            return;
        }

        if (claim.RelatedClaims.Count > 0)
        {
            ApplyClaimConsumingTableClaim(pageState, claim, table, grid, diagnostics);
            return;
        }

        var cells = claim.Candidates
            .Select(candidate => (Candidate: candidate, Column: GetColumnIndex(grid.Columns, (candidate.RelativeBoundingBox.LLx + candidate.RelativeBoundingBox.URx) / 2d)))
            .Where(x => x.Column >= 0)
            .GroupBy(x => GetRowCoordinate(x.Candidate))
            .OrderByDescending(x => x.Max(y => y.Candidate.RelativeBoundingBox.URy))
            .ToList();

        var tableNode = Structure.AddElement("Table").GetNode();
        if (Configuration.DebugWrite)
        {
            tableNode.Title = claim.RuleId;
        }
        var tableMcids = new List<int>();
        for (var rowIndex = 0; rowIndex < cells.Count; rowIndex++)
        {
            var row = cells[rowIndex];
            var rowNode = new StructuralContext(Structure, tableNode, Structure)
                .AddElement("TR")
                .GetNode();
            foreach (var cell in row.OrderBy(x => x.Column).ThenBy(x => x.Candidate.RelativeBoundingBox.LLx))
            {
                var isHeader = rowIndex < table.HeaderRows;
                var cellNode = new StructuralContext(Structure, rowNode, Structure)
                    .AddElement(isHeader ? "TH" : "TD")
                    .GetNode();
                if (isHeader)
                {
                    cellNode.Scope = StructureScope.Column;
                }

                var leaves = cell.Candidate.MaterializeLeaves(pageState.WorkingContent);
                if (leaves.Count == 0)
                {
                    diagnostics.Add($"Rule '{claim.RuleId}' matched table content but resolved no leaves.");
                    return;
                }

                var mcid = AllocateMcid(pageState.Page);
                var wrapper = pageState.WorkingContent.Wrap(
                    leaves,
                    new MarkedContent((PdfName)(isHeader ? "TH" : "TD"))
                    {
                        InlineProps = new PdfDictionary { [PdfName.MCID] = new PdfIntNumber(mcid) }
                    });
                BindMarkedContent(cellNode, pageState.Page, mcid);
                claim.AddAppliedBinding(new RemediationAppliedBinding(
                    isHeader ? "TH" : "TD",
                    new[] { mcid },
                    cellNode,
                    wrapper,
                    rowNode,
                    cell.Candidate.SourceReferences,
                    cell.Candidate.BoundingBox));
                tableMcids.Add(mcid);
                pageState.MarkDirty();
            }
        }

        claim.AddAppliedBinding(new RemediationAppliedBinding(
            "Table",
            tableMcids,
            tableNode,
            null,
            tableNode.Parent,
            claim.Candidates.SelectMany(x => x.SourceReferences).ToArray(),
            claim.BoundingBox));
    }

    private void ApplyClaimConsumingTableClaim(
        PageRemediationState pageState,
        RemediationClaim claim,
        TableRemediationAction table,
        TableGridResolution grid,
        List<string> diagnostics)
    {
        var plan = claim.TablePlan ?? ResolveClaimConsumingTablePlan(table, claim, grid);

        var tableNode = Structure.AddElement("Table").GetNode();
        if (Configuration.DebugWrite)
        {
            tableNode.Title = claim.RuleId;
        }
        var reusedMcids = new List<int>();
        foreach (var row in plan.Rows)
        {
            var rowNode = new StructuralContext(Structure, tableNode, Structure)
                .AddElement("TR")
                .GetNode();
            foreach (var cell in row.Cells)
            {
                var cellNode = new StructuralContext(Structure, rowNode, Structure)
                    .AddElement(cell.Tag)
                    .GetNode();
                if (cell.Tag == "TH")
                {
                    cellNode.Scope = StructureScope.Column;
                }

                var bindings = GetReusableStructureBindings(cell.Claim);
                if (bindings.Count == 0)
                {
                    diagnostics.Add($"Rule '{claim.RuleId}' matched claim '{cell.Claim.ClaimId}' without a reusable structure binding.");
                    return;
                }

                foreach (var binding in bindings)
                {
                    var child = binding.StructureNode!;
                    if (table.CellContentMode == TableCellContentMode.FlattenLeafClaims)
                    {
                        if (!CanFlattenBinding(binding, out var reason))
                        {
                            diagnostics.Add($"Rule '{claim.RuleId}' matched claim '{cell.Claim.ClaimId}' that cannot be flattened into a table cell: {reason}");
                            return;
                        }

                        FlattenBindingInto(binding, cellNode, cell.Tag);
                        pageState.MarkDirty();
                    }
                    else
                    {
                        Structure.ReparentStructureNode(child, cellNode);
                    }

                    reusedMcids.AddRange(binding.Mcids);
                    claim.AddAppliedBinding(new RemediationAppliedBinding(
                        cell.Tag,
                        binding.Mcids,
                        cellNode,
                        null,
                        rowNode,
                        binding.SourceReferences,
                        binding.Bounds));
                }
            }
        }

        claim.AddAppliedBinding(new RemediationAppliedBinding(
            "Table",
            reusedMcids,
            tableNode,
            null,
            tableNode.Parent,
            claim.Candidates.SelectMany(x => x.SourceReferences).ToArray(),
            claim.BoundingBox));
        PositionNodeByFirstMcid(tableNode);
    }

    private static RemediationTablePlan ResolveClaimConsumingTablePlan(
        TableRemediationAction table,
        RemediationClaim claim,
        TableGridResolution grid)
    {
        var rows = claim.RelatedClaims
            .Select(related => (
                Claim: related,
                Column: GetColumnIndex(grid.Columns, GetCenterX(related))))
            .Where(x => x.Column >= 0)
            .GroupBy(x => (x.Claim.PageIndex, Row: GetRowCoordinate(x.Claim)))
            .OrderBy(x => x.Key.PageIndex)
            .ThenByDescending(x => x.Average(y =>
                y.Claim.BoundingBox is { } box ? (box.LLy + box.URy) / 2d : double.MinValue))
            .ToList();
        var rowIndexesByPage = new Dictionary<int, int>();
        var plannedRows = new List<RemediationTableRowPlan>(rows.Count);
        for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
        {
            var row = rows[rowIndex];
            var pageRowIndex = rowIndexesByPage.TryGetValue(row.Key.PageIndex, out var currentPageRow)
                ? currentPageRow
                : 0;
            rowIndexesByPage[row.Key.PageIndex] = pageRowIndex + 1;
            var numericHeaderIndex = table.HeaderRowsScope == TableHeaderRowsScope.EveryPage
                ? pageRowIndex
                : rowIndex;
            var isHeaderRow = numericHeaderIndex < table.HeaderRows ||
                RowMatchesHeaderSelector(table, row.Select(x => x.Claim).ToList(), claim.RelatedClaims);
            var cells = row
                .OrderBy(x => x.Column)
                .ThenBy(x => x.Claim.BoundingBox?.LLx ?? 0)
                .Select(x => new RemediationTableCellPlan(
                    x.Claim,
                    x.Column,
                    isHeaderRow ? "TH" : "TD"))
                .ToList();
            plannedRows.Add(new RemediationTableRowPlan(row.Key.PageIndex, cells));
        }

        return new RemediationTablePlan(plannedRows);
    }

    private void PositionNodeByFirstMcid(StructureNode node)
    {
        var parent = node.Parent;
        if (parent == null)
        {
            return;
        }

        var pageIndexes = _document.Pages
            .Select((page, index) => (page, index))
            .ToDictionary(x => x.page, x => x.index);
        var firstPosition = GetFirstContentPosition(node, pageIndexes);
        if (firstPosition == ContentPosition.None)
        {
            return;
        }

        parent.Children.Remove(node);
        var targetIndex = parent.Children.FindIndex(x =>
            GetFirstContentPosition(x, pageIndexes).CompareTo(firstPosition) > 0);
        parent.Children.Insert(targetIndex < 0 ? parent.Children.Count : targetIndex, node);
    }

    private static ContentPosition GetFirstContentPosition(
        StructureNode node,
        IReadOnlyDictionary<PdfPage, int> pageIndexes)
    {
        var first = ContentPosition.None;
        foreach (var contentItem in node.ContentItems)
        {
            if (pageIndexes.TryGetValue(contentItem.Page, out var pageIndex))
            {
                first = ContentPosition.Min(first, new ContentPosition(pageIndex, contentItem.MCID));
            }
        }
        foreach (var child in node.Children)
        {
            first = ContentPosition.Min(first, GetFirstContentPosition(child, pageIndexes));
        }

        return first;
    }

    private readonly record struct ContentPosition(int PageIndex, int Mcid) : IComparable<ContentPosition>
    {
        public static ContentPosition None { get; } = new(int.MaxValue, int.MaxValue);

        public int CompareTo(ContentPosition other)
        {
            var pageComparison = PageIndex.CompareTo(other.PageIndex);
            return pageComparison != 0 ? pageComparison : Mcid.CompareTo(other.Mcid);
        }

        public static ContentPosition Min(ContentPosition left, ContentPosition right) =>
            left.CompareTo(right) <= 0 ? left : right;
    }

    private static bool RowMatchesHeaderSelector(
        TableRemediationAction table,
        IReadOnlyList<RemediationClaim> rowClaims,
        IReadOnlyList<RemediationClaim> allClaims)
    {
        if (table.HeaderSelector == null)
        {
            return false;
        }

        RemediationClaim? previous = null;
        foreach (var rowClaim in rowClaims)
        {
            var context = new ClaimPredicateEvaluationContext(allClaims, PreviousClaim: previous);
            if (table.HeaderSelector.Evaluate(context, rowClaim).IsMatch)
            {
                return true;
            }

            previous = rowClaim;
        }

        return false;
    }

    private static int GetColumnIndex(IReadOnlyList<double> columns, double centerX)
    {
        for (var i = 0; i < columns.Count - 1; i++)
        {
            var lower = columns[i];
            var upper = columns[i + 1];
            if (centerX >= lower && (centerX < upper || i == columns.Count - 2 && centerX <= upper))
            {
                return i;
            }
        }

        return -1;
    }

    private static double GetRowCoordinate(RemediationCandidate candidate)
    {
        if (candidate is TextRemediationCandidate { Characters.Count: > 0 } text)
        {
            return Math.Round(text.Characters.Average(x => x.BaselineCoordinate), 4);
        }

        return Math.Round(
            (candidate.RelativeBoundingBox.LLy + candidate.RelativeBoundingBox.URy) / 2d,
            4);
    }

    private static double GetRowCoordinate(RemediationClaim claim)
    {
        var textCharacters = claim.Candidates
            .OfType<TextRemediationCandidate>()
            .SelectMany(x => x.Characters)
            .ToArray();
        if (textCharacters.Length > 0)
        {
            return Math.Round(textCharacters.Average(x => x.BaselineCoordinate), 4);
        }

        var candidates = claim.Candidates.Where(x => x.PageIndex == claim.PageIndex).ToArray();
        return candidates.Length == 0
            ? 0d
            : Math.Round(candidates.Average(GetRowCoordinate), 4);
    }

    private static TableGridResolution ResolveTableGrid(
        TableRemediationAction table,
        IReadOnlyList<RemediationCandidate> candidates)
    {
        if (table.Columns is { Count: > 1 } columns)
        {
            return new TableGridResolution(columns, 1.0, false);
        }

        if (candidates.Count < 2)
        {
            return new TableGridResolution(Array.Empty<double>(), 0.0, true, "at least two candidates are required to infer columns.");
        }

        var clusters = new List<List<RemediationCandidate>>();
        foreach (var candidate in candidates.OrderBy(GetCenterX))
        {
            var cluster = clusters.FirstOrDefault(x => Math.Abs(GetCenterX(x[0]) - GetCenterX(candidate)) <= 12d);
            if (cluster == null)
            {
                clusters.Add(new List<RemediationCandidate> { candidate });
            }
            else
            {
                cluster.Add(candidate);
            }
        }

        if (clusters.Count < 2)
        {
            return new TableGridResolution(Array.Empty<double>(), 0.0, true, "column inference found fewer than two columns.");
        }

        var ordered = clusters
            .OrderBy(x => x.Average(GetCenterX))
            .Select(x => new
            {
                Min = x.Min(y => y.RelativeBoundingBox.LLx),
                Max = x.Max(y => y.RelativeBoundingBox.URx)
            })
            .ToList();

        var boundaries = new List<double> { ordered[0].Min - 1d };
        for (var i = 0; i < ordered.Count - 1; i++)
        {
            boundaries.Add((ordered[i].Max + ordered[i + 1].Min) / 2d);
        }

        boundaries.Add(ordered[^1].Max + 1d);

        var rowColumnCounts = candidates
            .GroupBy(GetRowCoordinate)
            .Select(x => x.Select(y => GetColumnIndex(boundaries, GetCenterX(y))).Where(y => y >= 0).Distinct().Count())
            .ToList();
        var confidence = rowColumnCounts.Count > 0 && rowColumnCounts.All(x => x == clusters.Count)
            ? 0.9
            : 0.5;

        return new TableGridResolution(boundaries, confidence, true);
    }

    private static double GetCenterX(RemediationCandidate candidate) =>
        (candidate.RelativeBoundingBox.LLx + candidate.RelativeBoundingBox.URx) / 2d;

    private static double GetCenterX(RemediationClaim claim) =>
        GetRelativeBounds(claim.Candidates.Where(x => x.PageIndex == claim.PageIndex)) is { } box
            ? (box.LLx + box.URx) / 2d
            : 0d;

    private static PdfRect<double>? GetRelativeBounds(IEnumerable<RemediationCandidate> candidates)
    {
        using var enumerator = candidates.Select(x => x.RelativeBoundingBox).GetEnumerator();
        if (!enumerator.MoveNext())
        {
            return null;
        }

        var bounds = enumerator.Current;
        while (enumerator.MoveNext())
        {
            var current = enumerator.Current;
            bounds = new PdfRect<double>(
                Math.Min(bounds.LLx, current.LLx),
                Math.Min(bounds.LLy, current.LLy),
                Math.Max(bounds.URx, current.URx),
                Math.Max(bounds.URy, current.URy));
        }
        return bounds;
    }

    private sealed record TableGridResolution(
        IReadOnlyList<double> Columns,
        double Confidence,
        bool Inferred,
        string? Error = null);

    private void CommitDocumentChanges(Action commit)
    {
        var checkpoint = PdfMutationCheckpoint.Capture(_document);
        var originalStructure = _document.ExistingStructure;
        try
        {
            commit();
        }
        catch
        {
            checkpoint.Restore();
            _document.Structure = originalStructure!;
            throw;
        }
    }

    private sealed class PdfMutationCheckpoint
    {
        private readonly List<(PdfDictionary Target, PdfDictionary Snapshot)> _dictionaries;
        private readonly List<(PdfArray Target, PdfArray Snapshot)> _arrays;

        private PdfMutationCheckpoint(
            List<(PdfDictionary Target, PdfDictionary Snapshot)> dictionaries,
            List<(PdfArray Target, PdfArray Snapshot)> arrays)
        {
            _dictionaries = dictionaries;
            _arrays = arrays;
        }

        public static PdfMutationCheckpoint Capture(PdfDocument document)
        {
            var dictionaries = new List<(PdfDictionary, PdfDictionary)>();
            var arrays = new List<(PdfArray, PdfArray)>();
            var seenDictionaries = new HashSet<PdfDictionary>(ReferenceEqualityComparer.Instance);
            var seenArrays = new HashSet<PdfArray>(ReferenceEqualityComparer.Instance);

            void Visit(IPdfObject? value)
            {
                if (value == null) return;
                var resolved = value.Resolve();
                switch (resolved)
                {
                    case PdfDictionary dictionary when seenDictionaries.Add(dictionary):
                        var dictionarySnapshot = dictionary.CloneShallow();
                        dictionaries.Add((dictionary, dictionarySnapshot));
                        foreach (var item in dictionary) Visit(item.Value);
                        break;
                    case PdfArray array when seenArrays.Add(array):
                        arrays.Add((array, array.CloneShallow()));
                        foreach (var item in array) Visit(item);
                        break;
                    case PdfStream stream:
                        Visit(stream.Dictionary);
                        break;
                }
            }

            Visit(document.Catalog);
            foreach (var page in document.Pages) Visit(page.NativeObject);
            return new PdfMutationCheckpoint(dictionaries, arrays);
        }

        public void Restore()
        {
            for (var i = _arrays.Count - 1; i >= 0; i--)
            {
                var (target, snapshot) = _arrays[i];
                target.Clear();
                foreach (var item in snapshot) target.Add(item);
            }
            for (var i = _dictionaries.Count - 1; i >= 0; i--)
            {
                RestoreDictionary(_dictionaries[i].Target, _dictionaries[i].Snapshot);
            }
        }
    }

    private static List<RemediationAppliedBinding> GetReusableStructureBindings(RemediationClaim claim) =>
        claim.AppliedBindings
            .Where(x => x.StructureNode != null &&
                string.Equals(x.ProducedTag, claim.ProducedTag, StringComparison.Ordinal))
            .ToList();

    private void ApplyGroupClaim(
        RemediationClaim claim,
        List<string> diagnostics)
    {
        if (claim.Action is not (GroupRemediationAction or BindTemplateSlotRemediationAction))
        {
            return;
        }

        var parentTag = claim.ProducedTag;
        var parentNode = Structure.AddElement(parentTag).GetNode();
        if (Configuration.DebugWrite)
        {
            parentNode.Title = claim.RuleId;
        }
        var reusedMcids = new List<int>();
        foreach (var related in claim.RelatedClaims.OrderBy(x => x, ReadingOrderComparer))
        {
            var bindings = GetReusableStructureBindings(related);
            if (bindings.Count == 0)
            {
                diagnostics.Add($"Rule '{claim.RuleId}' matched claim '{related.ClaimId}' without a reusable structure binding.");
                return;
            }

            foreach (var binding in bindings)
            {
                if (claim.Action is BindTemplateSlotRemediationAction bind &&
                    bind.ContentMode == TemplateSlotContentMode.FlattenLeafClaims)
                {
                    if (!CanFlattenBinding(binding, out var reason))
                    {
                        diagnostics.Add($"Rule '{claim.RuleId}' matched claim '{related.ClaimId}' that cannot be flattened into '{parentTag}': {reason}");
                        return;
                    }
                    FlattenBindingInto(binding, parentNode, parentTag);
                }
                else
                {
                    Structure.ReparentStructureNode(binding.StructureNode!, parentNode);
                }
                reusedMcids.AddRange(binding.Mcids);
            }
        }

        claim.AddAppliedBinding(new RemediationAppliedBinding(
            parentTag,
            reusedMcids,
            parentNode,
            null,
            parentNode.Parent,
            claim.Candidates.SelectMany(x => x.SourceReferences).ToArray(),
            claim.BoundingBox));
        PositionNodeByFirstMcid(parentNode);
    }

    private void ApplyMergeClaim(
        PageRemediationState pageState,
        RemediationClaim claim,
        List<string> diagnostics)
    {
        if (claim.Action is not MergeRemediationAction merge)
        {
            return;
        }

        var parentNode = Structure.AddElement(merge.TargetTag.Value).GetNode();
        if (Configuration.DebugWrite)
        {
            parentNode.Title = claim.RuleId;
        }

        if (merge.Attributes != null)
        {
            parentNode.Attributes.Add(merge.Attributes);
        }

        var reusedMcids = new List<int>();
        foreach (var related in claim.RelatedClaims.OrderBy(x => x, ReadingOrderComparer))
        {
            var bindings = GetReusableStructureBindings(related);
            if (bindings.Count == 0)
            {
                diagnostics.Add($"Rule '{claim.RuleId}' matched claim '{related.ClaimId}' without a reusable structure binding.");
                return;
            }

            foreach (var binding in bindings)
            {
                if (!CanFlattenBinding(binding, out var reason))
                {
                    diagnostics.Add($"Rule '{claim.RuleId}' matched claim '{related.ClaimId}' that cannot be merged into '{merge.TargetTag.Value}': {reason}");
                    return;
                }

                FlattenBindingInto(binding, parentNode, merge.TargetTag.Value);
                reusedMcids.AddRange(binding.Mcids);
            }
        }

        pageState.MarkDirty();
        claim.AddAppliedBinding(new RemediationAppliedBinding(
            merge.TargetTag.Value,
            reusedMcids,
            parentNode,
            null,
            parentNode.Parent,
            claim.Candidates.SelectMany(x => x.SourceReferences).ToArray(),
            claim.BoundingBox));
        PositionNodeByFirstMcid(parentNode);
    }

    private void FlattenBindingInto(RemediationAppliedBinding binding, StructureNode targetNode, string targetTag)
    {
        var node = binding.StructureNode!;
        Structure.FlattenLeafStructureNodeInto(node, targetNode);
        if (binding.MarkedContentGroup != null)
        {
            binding.MarkedContentGroup.Tag.Name = (PdfName)targetTag;
        }
    }

    private static bool CanFlattenBinding(RemediationAppliedBinding binding, out string reason)
    {
        var node = binding.StructureNode;
        if (node == null)
        {
            reason = "the binding has no structure node.";
            return false;
        }

        if (node.Children.Count > 0)
        {
            reason = "the structure node has child elements.";
            return false;
        }

        if (node.ObjectReferences.Count > 0 || node.XObjectReferences.Count > 0 || node.XObjectContentItems.Count > 0)
        {
            reason = "the structure node has object or XObject references.";
            return false;
        }

        if (IsNonFlattenableSemanticTag(node.Type))
        {
            reason = $"the structure node tag '{node.Type}' has standalone semantics.";
            return false;
        }

        if (node.ContentItems.Count == 0)
        {
            reason = "the structure node has no page marked-content references.";
            return false;
        }

        if (node.ID != null ||
            node.Alt != null ||
            node.ActualText != null ||
            node.Expansion != null ||
            node.Language != null ||
            node.Namespace != null ||
            node.References.Count > 0 ||
            node.Classes.Count > 0 ||
            node.Headers.Count > 0 ||
            node.Summary != null ||
            node.Scope != null ||
            node.ListNumbering != null ||
            node.Attributes.Count > 0)
        {
            reason = "the structure node carries semantic attributes that would be lost.";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    private static bool IsNonFlattenableSemanticTag(string tag) =>
        tag is "Table" or "TR" or "TH" or "TD" or "THead" or "TBody" or "TFoot"
            or "L" or "LI" or "Lbl" or "LBody"
            or "Link" or "Figure" or "Formula" or "Form"
            or "Annot" or "Ruby" or "RB" or "RT" or "RP" or "Warichu" or "WT" or "WP";

    private static void ApplyStructureAttributeClaim(
        RemediationClaim claim,
        List<string> diagnostics)
    {
        if (claim.Action is not StructureAttributeRemediationAction attributes)
        {
            return;
        }

        foreach (var related in claim.RelatedClaims)
        {
            var bindings = GetReusableStructureBindings(related);
            if (bindings.Count == 0)
            {
                diagnostics.Add($"Rule '{claim.RuleId}' matched claim '{related.ClaimId}' without a reusable structure binding.");
                return;
            }

            foreach (var binding in bindings)
            {
                var node = binding.StructureNode!;
                if (attributes.Language != null)
                {
                    node.Language = attributes.Language;
                }

                if (attributes.Alt != null)
                {
                    node.Alt = attributes.Alt;
                }

                if (attributes.ActualText != null)
                {
                    node.ActualText = attributes.ActualText;
                }

                if (attributes.Expansion != null)
                {
                    node.Expansion = attributes.Expansion;
                }

                if (attributes.Attributes != null)
                {
                    node.Attributes.Add(attributes.Attributes);
                }

                claim.AddAppliedBinding(new RemediationAppliedBinding(
                    node.Type,
                    binding.Mcids,
                    node,
                    binding.MarkedContentGroup,
                    node.Parent,
                    binding.SourceReferences,
                    binding.Bounds));
            }
        }
    }

    private static void ApplyReorderSiblingsClaim(
        RemediationClaim claim,
        List<string> diagnostics)
    {
        if (claim.Action is not ReorderSiblingsRemediationAction reorder)
        {
            return;
        }

        var nodeClaims = new List<(StructureNode Node, RemediationClaim Claim, RemediationAppliedBinding Binding)>();
        foreach (var related in claim.RelatedClaims)
        {
            var bindings = GetReusableStructureBindings(related);
            if (bindings.Count == 0)
            {
                diagnostics.Add($"Rule '{claim.RuleId}' matched claim '{related.ClaimId}' without a reusable structure binding.");
                return;
            }

            nodeClaims.AddRange(bindings.Select(x => (x.StructureNode!, related, x)));
        }

        var distinctNodeClaims = nodeClaims
            .GroupBy(x => x.Node)
            .Select(x => x.OrderBy(y => y.Claim.FirstSequenceIndex).First())
            .ToList();

        foreach (var parentGroup in distinctNodeClaims.GroupBy(x => x.Node.Parent))
        {
            var parent = parentGroup.Key;
            if (parent == null)
            {
                diagnostics.Add($"Rule '{claim.RuleId}' matched structure nodes without a parent.");
                return;
            }

            var selected = parentGroup.ToDictionary(x => x.Node, x => x);
            var selectedPositions = parent.Children
                .Select((node, index) => (node, index))
                .Where(x => selected.ContainsKey(x.node))
                .ToList();
            if (selectedPositions.Count < 2)
            {
                continue;
            }

            var sorted = selectedPositions
                .Select(x => selected[x.node])
                .OrderBy(x => x, new ReorderNodeComparer(reorder.Mode))
                .Select(x => x.Node)
                .ToList();

            for (var i = 0; i < selectedPositions.Count; i++)
            {
                parent.Children[selectedPositions[i].index] = sorted[i];
            }
        }

        foreach (var nodeClaim in distinctNodeClaims)
        {
            claim.AddAppliedBinding(new RemediationAppliedBinding(
                nodeClaim.Node.Type,
                nodeClaim.Binding.Mcids,
                nodeClaim.Node,
                nodeClaim.Binding.MarkedContentGroup,
                nodeClaim.Node.Parent,
                nodeClaim.Binding.SourceReferences,
                nodeClaim.Binding.Bounds));
        }
    }

    private void ApplyStructureLinkClaim(
        PageRemediationState pageState,
        RemediationClaim claim,
        List<string> diagnostics)
    {
        if (claim.Action is not StructureLinkRemediationAction link)
        {
            return;
        }

        if (claim.RelatedClaims.Count < 2)
        {
            diagnostics.Add($"Rule '{claim.RuleId}' did not identify both a link source claim and destination claim.");
            return;
        }

        var source = claim.RelatedClaims[0];
        var target = claim.RelatedClaims[1];
        var sourceBinding = source.AppliedBindings.FirstOrDefault(x => x.StructureNode != null);
        var targetBinding = target.AppliedBindings.FirstOrDefault(x => x.StructureNode != null);
        if (sourceBinding?.StructureNode == null)
        {
            diagnostics.Add($"Rule '{claim.RuleId}' matched source claim '{source.ClaimId}' without a reusable structure binding.");
            return;
        }

        if (targetBinding?.StructureNode == null)
        {
            diagnostics.Add($"Rule '{claim.RuleId}' matched destination claim '{target.ClaimId}' without a reusable structure binding.");
            return;
        }

        var bounds = sourceBinding.Bounds ?? source.BoundingBox;
        if (bounds == null)
        {
            diagnostics.Add($"Rule '{claim.RuleId}' matched source claim '{source.ClaimId}' without bounds for a link annotation.");
            return;
        }

        var linkNode = string.Equals(sourceBinding.StructureNode.Type, "Link", StringComparison.Ordinal)
            ? sourceBinding.StructureNode
            : Structure.AddElement("Link").GetNode();
        
        if (Configuration.DebugWrite)
        {
            linkNode.Title = claim.RuleId;
        }

        if (!ReferenceEquals(linkNode, sourceBinding.StructureNode))
        {
            Structure.InsertParentAroundStructureNode(sourceBinding.StructureNode, linkNode);
        }

        var annotation = AnnotationFactory.CreateStructureLink(
            pageState.Page,
            bounds,
            targetBinding.StructureNode,
            link.AccessibleDescription,
            link.DestinationTemplate);
        Structure.BindLinkAnnotation(linkNode, annotation, link.AccessibleDescription, link.AccessibleDescription);
        GetOrCreateStructParentsIndex(pageState.Page);

        claim.AddAppliedBinding(new RemediationAppliedBinding(
            "Link",
            sourceBinding.Mcids,
            linkNode,
            sourceBinding.MarkedContentGroup,
            linkNode.Parent,
            sourceBinding.SourceReferences,
            bounds));
    }

    private sealed class ReorderNodeComparer : IComparer<(StructureNode Node, RemediationClaim Claim, RemediationAppliedBinding Binding)>
    {
        private readonly SiblingReorderMode _mode;

        public ReorderNodeComparer(SiblingReorderMode mode)
        {
            _mode = mode;
        }

        public int Compare(
            (StructureNode Node, RemediationClaim Claim, RemediationAppliedBinding Binding) x,
            (StructureNode Node, RemediationClaim Claim, RemediationAppliedBinding Binding) y)
        {
            var result = _mode switch
            {
                SiblingReorderMode.GeometryTopToBottom => CompareGeometryTopToBottom(x, y),
                SiblingReorderMode.GeometryLeftToRight => CompareGeometryLeftToRight(x, y),
                _ => CompareClaimsInReadingOrder(x.Claim, y.Claim)
            };

            return result != 0
                ? result
                : CompareClaimsInReadingOrder(x.Claim, y.Claim);
        }

        private static int CompareGeometryTopToBottom(
            (StructureNode Node, RemediationClaim Claim, RemediationAppliedBinding Binding) x,
            (StructureNode Node, RemediationClaim Claim, RemediationAppliedBinding Binding) y)
        {
            var xb = x.Binding.Bounds ?? x.Claim.BoundingBox;
            var yb = y.Binding.Bounds ?? y.Claim.BoundingBox;
            if (xb == null || yb == null)
            {
                return 0;
            }

            var top = yb.URy.CompareTo(xb.URy);
            return top != 0 ? top : xb.LLx.CompareTo(yb.LLx);
        }

        private static int CompareGeometryLeftToRight(
            (StructureNode Node, RemediationClaim Claim, RemediationAppliedBinding Binding) x,
            (StructureNode Node, RemediationClaim Claim, RemediationAppliedBinding Binding) y)
        {
            var xb = x.Binding.Bounds ?? x.Claim.BoundingBox;
            var yb = y.Binding.Bounds ?? y.Claim.BoundingBox;
            if (xb == null || yb == null)
            {
                return 0;
            }

            var left = xb.LLx.CompareTo(yb.LLx);
            return left != 0 ? left : yb.URy.CompareTo(xb.URy);
        }
    }

    private static void RestoreDictionary(PdfDictionary target, PdfDictionary snapshot)
    {
        target.Clear();
        foreach (var item in snapshot)
        {
            target[item.Key] = item.Value;
        }
    }

    private void ApplyLeftoverPolicyAfterValidation(PageRemediationState pageState)
    {
        if (Configuration.LeftoverPolicy != RemediationLeftoverPolicy.AutoArtifact)
        {
            return;
        }

        var leftovers = EnumerateItems(pageState.WorkingContent)
            .OfType<TextContent<double>>()
            .Where(x => x.SourceReference is { })
            .Where(x =>
            {
                var span = new SourceTextSpan(
                    x.SourceReference!.Value,
                    x.SourceCharacterOffset,
                    x.Text.Length);
                return pageState.TextOwnership.FindOverlaps(new[] { span }).Count == 0;
            })
            .Cast<IContentItem<double>>()
            .ToList();
        foreach (var item in leftovers)
        {
            // Absorbed content adopts the subtype of the furniture it matched, so a declared inventory
            // is load-bearing rather than only a check.
            var declared = ResolveAbsorbedArtifactItem(
                pageState, new StructuredPageSpace(pageState.Page).Normalize(item.GetBoundingBox()));
            pageState.WorkingContent.Wrap(
                new[] { item },
                declared == null
                    ? new MarkedContent(PdfName.Artifact)
                    : new MarkedContent(PdfName.Artifact)
                    {
                        InlineProps = BuildArtifactProperties(
                            new ArtifactRemediationAction(
                                declared.Subtype,
                                declared.SemanticSubtype,
                                declared.IncludeBoundingBox,
                                declared.Attached),
                            item.GetBoundingBox())
                    });
            pageState.MarkDirty();
        }
    }

    private static PdfDictionary BuildArtifactProperties(
        ArtifactRemediationAction artifact,
        PdfRect<double> bounds)
    {
        var properties = new PdfDictionary
        {
            [PdfName.TYPE] = (PdfName)artifact.Subtype.ToString()
        };
        if (artifact.SemanticSubtype != null)
        {
            properties[PdfName.Subtype] = (PdfName)artifact.SemanticSubtype.Value.ToString();
        }
        if (artifact.IncludeBoundingBox)
        {
            properties[PdfName.BBox] = PdfRectangle.FromContentModel(bounds).NativeObject;
        }
        if (artifact.Attached is { Count: > 0 })
        {
            properties[(PdfName)"Attached"] = new PdfArray(
                artifact.Attached.Select(x => (IPdfObject)(PdfName)x.ToString()).ToList());
        }
        return properties;
    }

    private string ResolveProducedTag(Rule rule, string fallback)
    {
        if (string.IsNullOrWhiteSpace(rule.Slot) ||
            rule.Action is not (BindTemplateSlotRemediationAction or TableRemediationAction or AdoptAnnotationRemediationAction))
        {
            return fallback;
        }

        var template = _ruleSets.Select(x => x.StructuralTemplate).FirstOrDefault(x => x != null);
        var node = template == null ? null : FindTemplateNode(template.Document, rule.Slot!);
        return node?.Tag ?? fallback;
    }

    private static RemediationStructuralTemplateNode? FindTemplateNode(
        RemediationStructuralTemplateNode node,
        string slotId)
    {
        if (string.Equals(node.Id, slotId, StringComparison.Ordinal))
        {
            return node;
        }
        foreach (var child in node.Children)
        {
            var found = FindTemplateNode(child, slotId);
            if (found != null) return found;
        }
        return null;
    }

    private RemediationClaim CreateClaim(Rule rule, int pageIndex, RemediationCandidate candidate, ClaimStatus status, double confidence)
    {
        var producedTag = rule.Action is AdoptAnnotationRemediationAction &&
            candidate is AnnotationRemediationCandidate annotation
                ? ResolveProducedTag(rule, RequiredAnnotationTag(annotation.Subtype))
                : ResolveProducedTag(rule, rule.Action.DebugString);
        return new RemediationClaim(
            rule.Id,
            new[] { candidate },
            producedTag,
            confidence)
        {
            PageIndex = pageIndex,
            Status = status,
            SelectorDebugString = rule.Predicate.DebugString,
            Action = rule.Action,
            RuleSetId = rule.RuleSetId,
                SlotId = rule.Slot,
            TextNormalization = rule.TextNormalization ?? TextNormalizationOptions.Default
        };
    }

    private void RunDiagnostics(IReadOnlyList<PageRemediationState> pageStates, List<string> diagnostics)
    {
        foreach (var pageState in pageStates)
        {
            var pageScope = $"Page{pageState.PageIndex + 1}";
            CheckUntaggedContent(pageState, pageScope, diagnostics);
            CheckMcidIntegrity(pageState, pageScope, diagnostics);
        }

        CheckStructParents(pageStates, diagnostics);
        CheckReadingOrder(pageStates, diagnostics);
    }

    private void CheckReadingOrder(IReadOnlyList<PageRemediationState> pageStates, List<string> diagnostics)
    {
        if (IsPrescriptiveTemplate()) return;
        var pageIndexes = pageStates.ToDictionary(x => x.Page, x => x.PageIndex);
        var contentInLogicalOrder = new List<(int PageIndex, int Mcid)>();

        void CollectLogical(StructureNode node)
        {
            foreach (var contentItem in node.ContentItems)
            {
                if (pageIndexes.TryGetValue(contentItem.Page, out var pageIndex))
                {
                    contentInLogicalOrder.Add((pageIndex, contentItem.MCID));
                }
            }
            foreach (var child in node.Children)
            {
                CollectLogical(child);
            }
        }

        CollectLogical(Structure.GetRoot());
        for (var i = 1; i < contentInLogicalOrder.Count; i++)
        {
            var previous = contentInLogicalOrder[i - 1];
            var current = contentInLogicalOrder[i];
            if (current.PageIndex > previous.PageIndex ||
                current.PageIndex == previous.PageIndex && current.Mcid >= previous.Mcid)
            {
                continue;
            }

            var scope = current.PageIndex == previous.PageIndex
                ? $"Page{current.PageIndex + 1}"
                : "Document";
            var message = current.PageIndex == previous.PageIndex
                ? $"Logical reading order drift detected on page {current.PageIndex + 1}: " +
                  $"MCID {current.Mcid} appears after MCID {previous.Mcid} " +
                  "under the default top-to-bottom, left-to-right reading-order contract. " +
                  $"Logical sequence: {string.Join(", ", contentInLogicalOrder.Select(x => $"p{x.PageIndex + 1}:{x.Mcid}"))}."
                : $"Logical reading order drift detected: page {current.PageIndex + 1} MCID {current.Mcid} " +
                  $"appears after page {previous.PageIndex + 1} MCID {previous.Mcid}.";
            ReportDiagnostic(DiagnosticCode.ReadingOrderDrift, scope, message, diagnostics);
            break;
        }
    }

    private void CheckUntaggedContent(PageRemediationState pageState, string scope, List<string> diagnostics)
    {
        var untagged = new List<IContentItem<double>>();
        CheckUntaggedRecursive(pageState.WorkingContent, false, untagged);

        var painting = untagged.Where(IsPaintingItem).ToList();
        if (painting.Count > 0)
        {
            var counts = painting
                .GroupBy(x => GetCandidateKind(x)?.ToString() ?? x.Type.ToString())
                .OrderBy(x => x.Key, StringComparer.Ordinal)
                .Select(x => $"{x.Key}={x.Count()}");
            var msg =
                $"Page {pageState.PageIndex + 1} has painting content outside marked content: " +
                string.Join(", ", counts) + ".";
            ReportDiagnostic(DiagnosticCode.UntaggedContent, scope, msg, diagnostics);
        }
    }

    private void CheckUntaggedRecursive(IEnumerable<IContentNode<double>> nodes, bool inBdc, List<IContentItem<double>> untagged)
    {
        foreach (var node in nodes)
        {
            if (node is MarkedContentGroup<double> marked)
            {
                CheckUntaggedRecursive(marked.Children, true, untagged);
            }
            else if (node is IContentItem<double> item)
            {
                if (!inBdc)
                {
                    // Ignore items that don't paint anything, but typically we want everything inside
                    // For now just add all
                    untagged.Add(item);
                }
            }
        }
    }

    private void CheckMcidIntegrity(PageRemediationState pageState, string scope, List<string> diagnostics)
    {
        // 13.2 "every MCID referenced by exactly one structure element" check.
        // Get all MCIDs from WorkingContent
        var mcidsInContent = new HashSet<int>();
        var duplicateMcidsInContent = new HashSet<int>();

        void CollectMcids(IEnumerable<IContentNode<double>> nodes)
        {
            foreach (var node in nodes)
            {
                if (node is MarkedContentGroup<double> marked)
                {
                    if (marked.Tag.InlineProps?.TryGetValue(PdfName.MCID, out var mcidObj) == true && mcidObj is PdfIntNumber mcidNum)
                    {
                        var mcid = mcidNum.Value;
                        if (!mcidsInContent.Add(mcid))
                        {
                            duplicateMcidsInContent.Add(mcid);
                        }
                    }
                    CollectMcids(marked.Children);
                }
            }
        }

        CollectMcids(pageState.WorkingContent);

        // Get all MCIDs referenced by structure elements on this page
        var referencedMcids = new HashSet<int>();
        var duplicateReferences = new HashSet<int>();

        void CollectReferences(StructureNode node)
        {
            foreach (var contentItem in node.ContentItems)
            {
                if (contentItem.Page == pageState.Page)
                {
                    if (!referencedMcids.Add(contentItem.MCID))
                    {
                        duplicateReferences.Add(contentItem.MCID);
                    }
                }
            }
            foreach (var child in node.Children)
            {
                CollectReferences(child);
            }
        }

        CollectReferences(Structure.GetRoot());

        foreach (var dup in duplicateMcidsInContent)
        {
            ReportDiagnostic(DiagnosticCode.DuplicatedMcid, scope, $"MCID {dup} appears multiple times in the content stream on page {pageState.PageIndex + 1}.", diagnostics);
        }

        foreach (var dup in duplicateReferences)
        {
            ReportDiagnostic(DiagnosticCode.DuplicatedMcid, scope, $"MCID {dup} on page {pageState.PageIndex + 1} is referenced by multiple structure elements.", diagnostics);
        }

        foreach (var mcid in mcidsInContent)
        {
            if (!referencedMcids.Contains(mcid))
            {
                ReportDiagnostic(DiagnosticCode.OrphanedMcid, scope, $"MCID {mcid} on page {pageState.PageIndex + 1} is not referenced by any structure element.", diagnostics);
            }
        }

        foreach (var mcid in referencedMcids)
        {
            if (!mcidsInContent.Contains(mcid))
            {
                ReportDiagnostic(DiagnosticCode.OrphanedMcid, scope, $"Structure element references MCID {mcid} on page {pageState.PageIndex + 1}, but it is missing from the content stream.", diagnostics);
            }
        }
    }

    private void CheckStructParents(IReadOnlyList<PageRemediationState> pageStates, List<string> diagnostics)
    {
        // 13.3 Implement "`/StructParents` set for pages with claims" check.
        // Actually _pageStructParents tracks this
        foreach (var pageState in pageStates)
        {
            if (_pagesWithAllocatedMcids.Contains(pageState.Page))
            {
                if (!_pageStructParents.ContainsKey(pageState.Page))
                {
                    ReportDiagnostic(DiagnosticCode.MissingStructParents, $"Page{pageState.PageIndex + 1}", $"Page {pageState.PageIndex + 1} has MCIDs but no /StructParents entry.", diagnostics);
                }
            }
        }
    }

    private void ReportDiagnostic(
        DiagnosticCode code,
        string scope,
        string message,
        List<string> diagnostics,
        SlotRef? programSlot = null,
        string? bindingId = null,
        IReadOnlyList<string>? candidateIds = null,
        IReadOnlyDictionary<string, object?>? evidence = null)
    {
        var strict = Configuration.DiagnosticStrictness == RemediationDiagnosticStrictness.Strict;
        var suppression = NonSuppressibleDiagnosticCodes.Contains(code)
            ? null
            : _suppressions.FirstOrDefault(x => x.Code == code && (x.Scope == "*" || x.Scope == scope));

        if (_programRuntimeDiagnostics != null)
        {
            if (suppression != null && !strict)
            {
                _programRuntimeDiagnostics.Add(new RemediationRuntimeDiagnostic(
                    code,
                    RemediationDiagnosticDisposition.Acknowledged,
                    scope,
                    message,
                    programSlot,
                    bindingId,
                    candidateIds,
                    evidence,
                    Suppression: suppression));
                diagnostics.Add($"[SUPPRESSED] {code}: {message} (Reason: {suppression.Reason})");
                return;
            }

            _programRuntimeDiagnostics.Add(new RemediationRuntimeDiagnostic(
                code,
                Configuration.RunMode == RemediationRunMode.Authoring && ProgramWorkItemCodes.Contains(code)
                    ? RemediationDiagnosticDisposition.WorkItem
                    : RemediationDiagnosticDisposition.Error,
                scope,
                message,
                programSlot,
                bindingId,
                candidateIds,
                evidence,
                Suppression: suppression));

            if (suppression != null && strict)
                diagnostics.Add($"[IGNORED-SUPPRESSION] {code}: {message}");
            diagnostics.Add($"{code}: {message}");
            return;
        }
        
        if (suppression != null && !strict)
        {
            diagnostics.Add($"[SUPPRESSED] {code}: {message} (Reason: {suppression.Reason})");
            return;
        }

        if (suppression != null && strict)
        {
            diagnostics.Add($"[IGNORED-SUPPRESSION] {code}: {message}");
        }

        diagnostics.Add($"{code}: {message}");
    }

    private static bool HasUnsuppressedDiagnostics(IEnumerable<string> diagnostics) =>
        diagnostics.Any(x => !x.StartsWith("[SUPPRESSED]", StringComparison.Ordinal));

    private static IReadOnlyList<SourceTextSpan> GetTargetSpans(RemediationCandidate candidate)
    {
        if (candidate is not TextRemediationCandidate textCandidate || textCandidate.TextRanges.Count == 0)
        {
            return Array.Empty<SourceTextSpan>();
        }

        if (textCandidate.RequiresExactMaterialization)
        {
            return textCandidate.TextRanges
                .Select(x => new SourceTextSpan(x.SourceReference, x.StartCharacterIndex, x.CharacterCount))
                .ToArray();
        }

        return textCandidate.TextRanges
            .GroupBy(x => x.SourceReference)
            .Select(group =>
            {
                var start = group.Min(x => x.StartCharacterIndex);
                var end = group.Max(x => x.StartCharacterIndex + x.CharacterCount);
                return new SourceTextSpan(group.Key, start, end - start);
            })
            .ToArray();
    }

    private static IReadOnlyList<RemediationClaim> CreateResidualClaims(
        RemediationClaim previous,
        IReadOnlyList<SourceTextSpan> replacing)
    {
        var residuals = new List<RemediationClaim>();
        foreach (var candidate in previous.Candidates)
        {
            foreach (var span in GetTargetSpans(candidate))
            {
                foreach (var residualSpan in Subtract(span, replacing))
                {
                    var characters = ((TextRemediationCandidate)candidate).Characters
                        .Where(x => x.SourceReference == residualSpan.SourceReference &&
                                    x.SourceCharacterIndex >= residualSpan.StartCharacterIndex &&
                                    x.SourceCharacterIndex < residualSpan.EndCharacterIndex)
                        .OrderBy(x => x.SourceCharacterIndex)
                        .ToList();
                    if (characters.Count == 0)
                    {
                        continue;
                    }

                    var range = new RemediationTextRange(
                        residualSpan.SourceReference,
                        residualSpan.StartCharacterIndex,
                        residualSpan.CharacterCount,
                        new string(characters.Select(x => x.Char).ToArray()));
                    var residualCandidate = RemediationCandidate.CreateExactRange(candidate, range, characters);
                    residuals.Add(new RemediationClaim(
                        previous.RuleId,
                        new[] { residualCandidate },
                        previous.Tag,
                        previous.Confidence)
                    {
                        PageIndex = previous.PageIndex,
                        Status = ClaimStatus.Applied,
                        SelectorDebugString = previous.SelectorDebugString,
                        Action = previous.Action,
                        RuleSetId = previous.RuleSetId,
                        SlotId = previous.SlotId,
                        TextNormalization = previous.TextNormalization
                    });
                }
            }
        }

        return residuals;
    }

    private static IReadOnlyList<SourceTextSpan> Subtract(
        SourceTextSpan source,
        IReadOnlyList<SourceTextSpan> replacing)
    {
        var residuals = new List<SourceTextSpan> { source };
        foreach (var replacement in replacing.Where(source.Overlaps))
        {
            var next = new List<SourceTextSpan>();
            foreach (var residual in residuals)
            {
                if (!residual.Overlaps(replacement))
                {
                    next.Add(residual);
                    continue;
                }

                if (replacement.StartCharacterIndex > residual.StartCharacterIndex)
                {
                    next.Add(new SourceTextSpan(
                        residual.SourceReference,
                        residual.StartCharacterIndex,
                        replacement.StartCharacterIndex - residual.StartCharacterIndex));
                }

                if (replacement.EndCharacterIndex < residual.EndCharacterIndex)
                {
                    next.Add(new SourceTextSpan(
                        residual.SourceReference,
                        replacement.EndCharacterIndex,
                        residual.EndCharacterIndex - replacement.EndCharacterIndex));
                }
            }

            residuals = next;
        }

        return residuals;
    }

    internal static IComparer<RemediationClaim> ReadingOrderComparer { get; } =
        Comparer<RemediationClaim>.Create(CompareClaimsInReadingOrder);

    internal static int CompareClaimsInReadingOrder(RemediationClaim left, RemediationClaim right)
    {
        var page = left.PageIndex.CompareTo(right.PageIndex);
        if (page != 0)
        {
            return page;
        }

        var leftBounds = left.BoundingBox;
        var rightBounds = right.BoundingBox;
        if (leftBounds != null && rightBounds != null)
        {
            var leftCenterY = (leftBounds.LLy + leftBounds.URy) / 2d;
            var rightCenterY = (rightBounds.LLy + rightBounds.URy) / 2d;
            if (Math.Abs(leftCenterY - rightCenterY) > 6d)
            {
                return rightCenterY.CompareTo(leftCenterY);
            }

            var column = leftBounds.LLx.CompareTo(rightBounds.LLx);
            if (column != 0)
            {
                return column;
            }
        }

        var sequence = left.FirstSequenceIndex.CompareTo(right.FirstSequenceIndex);
        if (sequence != 0)
        {
            return sequence;
        }

        var leftSpan = left.Candidates.SelectMany(GetTargetSpans)
            .OrderBy(x => x.SourceReference.StreamId.ToString(), StringComparer.Ordinal)
            .ThenBy(x => x.SourceReference.OperatorStart)
            .ThenBy(x => x.StartCharacterIndex)
            .FirstOrDefault();
        var rightSpan = right.Candidates.SelectMany(GetTargetSpans)
            .OrderBy(x => x.SourceReference.StreamId.ToString(), StringComparer.Ordinal)
            .ThenBy(x => x.SourceReference.OperatorStart)
            .ThenBy(x => x.StartCharacterIndex)
            .FirstOrDefault();

        var stream = string.Compare(
            leftSpan.SourceReference.StreamId.ToString(),
            rightSpan.SourceReference.StreamId.ToString(),
            StringComparison.Ordinal);
        if (stream != 0)
        {
            return stream;
        }

        var source = leftSpan.SourceReference.OperatorStart.CompareTo(rightSpan.SourceReference.OperatorStart);
        if (source != 0)
        {
            return source;
        }

        var character = leftSpan.StartCharacterIndex.CompareTo(rightSpan.StartCharacterIndex);
        if (character != 0)
        {
            return character;
        }

        var leftCandidate = left.Candidates.Select(x => x.CandidateId)
            .OrderBy(x => x, StringComparer.Ordinal)
            .FirstOrDefault() ?? string.Empty;
        var rightCandidate = right.Candidates.Select(x => x.CandidateId)
            .OrderBy(x => x, StringComparer.Ordinal)
            .FirstOrDefault() ?? string.Empty;
        var candidate = string.Compare(leftCandidate, rightCandidate, StringComparison.Ordinal);
        if (candidate != 0)
        {
            return candidate;
        }

        var rule = string.Compare(left.RuleId, right.RuleId, StringComparison.Ordinal);
        return rule != 0
            ? rule
            : string.Compare(left.ProducedTag, right.ProducedTag, StringComparison.Ordinal);
    }

    private static IEnumerable<IContentItem<double>> EnumerateItems(IEnumerable<IContentNode<double>> nodes)
    {
        foreach (var node in nodes)
        {
            if (node is MarkedContentGroup<double> marked)
            {
                foreach (var child in EnumerateItems(marked.Children))
                {
                    yield return child;
                }

                continue;
            }

            if (node is IContentItem<double> item)
            {
                yield return item;
            }
        }
    }

    private static IEnumerable<IContentItem<double>> FlattenItems(IEnumerable<IContentNode<double>> nodes)
    {
        foreach (var node in nodes)
        {
            if (node is MarkedContentGroup<double> marked)
            {
                foreach (var child in FlattenItems(marked.Children))
                {
                    yield return child;
                }

                continue;
            }

            if (node is IContentItem<double> item)
            {
                yield return item;
            }
        }
    }

    internal int AllocateMcid(PdfPage page)
    {
        ThrowIfDisposed();
        _pagesWithAllocatedMcids.Add(page);
        return McidAllocator.Allocate(page);
    }

    internal int GetOrCreateStructParentsIndex(PdfPage page)
    {
        ThrowIfDisposed();

        if (_pageStructParents.TryGetValue(page, out var existing))
        {
            return existing;
        }

        var index = Structure.GetStructureRoot().AllocateStructParentIndex();
        _pageStructParents[page] = index;
        return index;
    }

    internal void BindMarkedContent(StructureNode node, PdfPage page, int mcid)
    {
        ThrowIfDisposed();
        node.ContentItems.Add((page, mcid));
        GetOrCreateStructParentsIndex(page);
    }

    internal void BindAnnotation(
        StructureNode node,
        PdfPage page,
        PdfDictionary annotation,
        string? accessibleDescription = null,
        StructureNode? destinationTarget = null)
    {
        ThrowIfDisposed();

        var index = Structure.GetStructureRoot().AllocateStructParentIndex();
        annotation[PdfName.StructParent] = new PdfIntNumber(index);
        if (!string.IsNullOrWhiteSpace(accessibleDescription))
        {
            annotation[PdfName.Contents] = PdfString.CreateTextString(accessibleDescription);
        }
        node.ObjectReferences.Add(new StructureObjectReference(annotation, index, page)
        {
            AnnotationContents = accessibleDescription,
            StructureDestinationTarget = destinationTarget
        });
        GetOrCreateStructParentsIndex(page);
    }

    internal void BindImage(StructureNode node, XObjImage image, params PdfPage[] pages)
    {
        ThrowIfDisposed();
        Structure.BindImage(node, image, pages);
        foreach (var page in pages)
        {
            GetOrCreateStructParentsIndex(page);
        }
    }

    internal void BindFormXObject(StructureNode node, XObjForm form, params PdfPage[] pages)
    {
        ThrowIfDisposed();
        Structure.BindFormXObject(node, form, pages);
        foreach (var page in pages)
        {
            GetOrCreateStructParentsIndex(page);
        }
    }

    /// <summary>Releases the remediation session.</summary>
    public void Dispose()
    {
        _pagesWithAllocatedMcids.Clear();
        _pageStructParents.Clear();
        _disposed = true;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(RemediationSession));
        }
    }
}
