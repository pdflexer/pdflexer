using PdfLexer.Content;
using PdfLexer.Content.Model;
using PdfLexer.DOM;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace PdfLexer.Remediation;

/// <summary>
/// Coordinates rule-driven remediation for a currently untagged PDF document.
/// </summary>
public sealed class RemediationSession : IDisposable
{
    private readonly PdfDocument _document;
    private readonly Dictionary<PdfPage, int> _pageMcids = new();
    private readonly Dictionary<PdfPage, int> _pageStructParents = new();
    private readonly List<RuleSet> _ruleSets = new();
    private readonly List<DiagnosticSuppression> _suppressions = new();
    private bool _committed;
    private bool _disposed;

    internal RemediationSession(PdfDocument document, RemediationSessionConfiguration configuration)
    {
        _document = document;
        Configuration = configuration;
        Structure = new StructuralBuilder();
    }

    /// <summary>Session configuration and commit-time accessibility setup options.</summary>
    public RemediationSessionConfiguration Configuration { get; }

    /// <summary>Structure builder owned by the remediation session.</summary>
    public StructuralBuilder Structure { get; }

    /// <summary>Adds rule sets to the session in deterministic composition order.</summary>
    public RemediationSession Use(params RuleSet[] ruleSets)
    {
        ThrowIfDisposed();
        if (ruleSets == null)
        {
            throw new ArgumentNullException(nameof(ruleSets));
        }

        _ruleSets.AddRange(ruleSets.Where(x => x != null));
        return this;
    }

    /// <summary>Adds a justified diagnostic suppression.</summary>
    public RemediationSession Suppress(DiagnosticCode code, string scope, string reason)
    {
        ThrowIfDisposed();
        _suppressions.Add(new DiagnosticSuppression(code, scope, reason));
        return this;
    }

    /// <summary>Validates rules without parsing pages.</summary>
    public ValidationReport Validate(params Rule[] rules) => Validate((IEnumerable<Rule>)rules);

    /// <summary>Validates rules without parsing pages.</summary>
    public ValidationReport Validate(IEnumerable<Rule> rules)
    {
        ThrowIfDisposed();
        var composedRuleSets = _ruleSets.ToList();
        return ValidateRules(
            ComposeRules(rules),
            BuildAnchorLookup(composedRuleSets),
            BuildTolerancedZoneLookup(composedRuleSets),
            BuildFlowRegionLookup(composedRuleSets),
            ValidateRuleSetDeclarations(composedRuleSets));
    }

    /// <summary>Validates rule sets without parsing pages.</summary>
    public ValidationReport Validate(params RuleSet[] ruleSets)
    {
        ThrowIfDisposed();
        if (ruleSets == null)
        {
            throw new ArgumentNullException(nameof(ruleSets));
        }

        var composedRuleSets = _ruleSets.Concat(ruleSets).ToList();
        return ValidateRules(
            composedRuleSets.SelectMany(x => x.Rules).ToList(),
            BuildAnchorLookup(composedRuleSets),
            BuildTolerancedZoneLookup(composedRuleSets),
            BuildFlowRegionLookup(composedRuleSets),
            ValidateRuleSetDeclarations(composedRuleSets));
    }

    /// <summary>Evaluates configured rule sets without mutating the document.</summary>
    public RemediationReport DryRun()
    {
        ThrowIfDisposed();
        if (_ruleSets.Count > 0)
        {
            return Evaluate(ComposeRules(Array.Empty<Rule>()), apply: false);
        }

        return new RemediationReport(committed: false, appliedAccessibilitySetup: false);
    }

    /// <summary>Evaluates additional rules without mutating the document.</summary>
    public RemediationReport DryRun(params Rule[] rules) => DryRun((IEnumerable<Rule>)rules);

    /// <summary>Evaluates additional rules without mutating the document.</summary>
    public RemediationReport DryRun(IEnumerable<Rule> rules)
    {
        ThrowIfDisposed();
        return Evaluate(ComposeRules(rules), apply: false);
    }

    /// <summary>Applies configured rule sets, accessibility setup, and diagnostics to the document.</summary>
    public RemediationReport Commit()
    {
        ThrowIfDisposed();
        if (_committed)
        {
            throw new InvalidOperationException("Remediation session has already been committed.");
        }

        if (_ruleSets.Count > 0)
        {
            return Commit(Array.Empty<Rule>());
        }

        CommitDocumentChanges(() =>
        {
            foreach (var (page, structParentsIndex) in _pageStructParents)
            {
                page.StructParents = new PdfIntNumber(structParentsIndex);
            }

            _document.Structure = Structure;
            _document.ApplyAccessibilitySetup(
                Configuration.Language,
                Configuration.Title,
                Configuration.Profile,
                Configuration.StrictConformance);
            _document.ValidateAccessibilityAuthoringSnapshot();
        });

        _committed = true;
        Dispose();
        return new RemediationReport(committed: true, appliedAccessibilitySetup: true);
    }

    /// <summary>Applies additional rules, accessibility setup, and diagnostics to the document.</summary>
    public RemediationReport Commit(params Rule[] rules) => Commit((IEnumerable<Rule>)rules);

    /// <summary>Applies additional rules, accessibility setup, and diagnostics to the document.</summary>
    public RemediationReport Commit(IEnumerable<Rule> rules)
    {
        ThrowIfDisposed();
        if (_committed)
        {
            throw new InvalidOperationException("Remediation session has already been committed.");
        }

        RemediationReport report = null!;
        CommitDocumentChanges(() =>
        {
            report = Evaluate(ComposeRules(rules), apply: true);
            var unsuppressed = report.Diagnostics.Where(d => !d.StartsWith("[SUPPRESSED]")).ToList();
            if (unsuppressed.Count > 0)
            {
                throw new InvalidOperationException(string.Join(Environment.NewLine, unsuppressed));
            }

            foreach (var (page, structParentsIndex) in _pageStructParents)
            {
                page.StructParents = new PdfIntNumber(structParentsIndex);
            }

            _document.Structure = Structure;
            _document.ApplyAccessibilitySetup(
                Configuration.Language,
                Configuration.Title,
                Configuration.Profile,
                Configuration.StrictConformance);
            _document.ValidateAccessibilityAuthoringSnapshot();
        });

        _committed = true;
        Dispose();
        return new RemediationReport(
            committed: true,
            appliedAccessibilitySetup: true,
            report.Claims,
            report.SkippedClaims,
            report.Diagnostics,
            _suppressions);
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
        if (diagnostics.Count > 0)
        {
            return new RemediationReport(false, false, diagnostics: diagnostics, suppressions: _suppressions);
        }

        var allClaims = new List<RemediationClaim>();
        var skippedClaims = new List<RemediationClaim>();
        var pageStates = BuildPageStates();
        foreach (var pageSelection in pageStates)
        {
            EvaluatePage(pageSelection, ruleList, allClaims, skippedClaims, diagnostics, apply: false);
        }

        if (apply && diagnostics.Count == 0)
        {
            ValidatePlan(pageStates, diagnostics);
        }

        if (apply && diagnostics.Count == 0)
        {
            ApplyPlan(pageStates, diagnostics);
            RunDiagnostics(pageStates, diagnostics);
        }

        return new RemediationReport(false, false, allClaims, skippedClaims, diagnostics, _suppressions);
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
        }

        if (rule.Stage is Stage.Group or Stage.Refine && rule.Action is TagRemediationAction)
        {
            errors.Add($"Rule '{rule.Id}' uses a raw-content Tag action in {rule.Stage}; claim-consuming stages must select existing claims.");
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

        if (anchor.Occurrence is <= 0)
        {
            errors.Add($"Anchor '{anchor.Id}' has invalid occurrence {anchor.Occurrence}; occurrences are one-based.");
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

            if (anchor.Occurrence != null && predicate.Selection.Mode != AnchorSelectionMode.RequiredSingle)
            {
                errors.Add($"Anchor '{anchor.Id}' cannot combine legacy Occurrence with explicit selection '{predicate.Selection.DebugString}'.");
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

        return states;
    }

    private void EvaluatePage(
        PageRemediationState pageState,
        IReadOnlyList<Rule> rules,
        List<RemediationClaim> allClaims,
        List<RemediationClaim> skippedClaims,
        List<string> diagnostics,
        bool apply)
    {
        var ownedTargets = new Dictionary<string, RemediationClaim>(StringComparer.Ordinal);
        var stageClaims = new List<RemediationClaim>();
        var anchors = BuildAnchorLookup(_ruleSets);
        var tolerancedZones = BuildTolerancedZoneLookup(_ruleSets);
        var flowRegions = BuildFlowRegionLookup(_ruleSets);
        var flowDiagnosticsChecked = false;
        
        foreach (var stage in new[] { Stage.Classify, Stage.Group, Stage.Refine })
        {
            var context = new RemediationEvaluationContext(
                allClaims.Concat(stageClaims).ToList(),
                pageBox: pageState.StructuredText.RelativePageBox,
                pageIndex: pageState.PageIndex,
                pageCount: _document.Pages.Count,
                configuration: Configuration,
                anchors: anchors,
                tolerancedZones: tolerancedZones,
                flowRegions: flowRegions,
                structuredText: pageState.StructuredText,
                diagnostics: diagnostics);

            if (!flowDiagnosticsChecked)
            {
                CheckFlowRegionDiagnostics(context, diagnostics);
                flowDiagnosticsChecked = true;
            }

            foreach (var rule in rules.Where(x => x.Stage == stage && x.Pages.Includes(pageState.PageIndex, _document.Pages.Count)))
            {
                if (rule.Action is CustomRemediationAction custom)
                {
                    EvaluateCustomRule(pageState, rule, custom, context, stageClaims, skippedClaims, diagnostics, apply);
                    continue;
                }

                if (stage == Stage.Classify)
                {
                    EvaluateClassifyRule(pageState, rule, context, ownedTargets, stageClaims, skippedClaims, diagnostics, apply);
                    continue;
                }

                if (stage == Stage.Group && rule.Action is TableRemediationAction table)
                {
                    EvaluateTableRule(pageState, rule, table, context, stageClaims, skippedClaims, diagnostics);
                    continue;
                }

                if (stage == Stage.Group && rule.Action is GroupRemediationAction group)
                {
                    EvaluateGroupRule(pageState, rule, group, context, stageClaims, skippedClaims);
                    continue;
                }

                if (stage == Stage.Group && rule.Action is MergeRemediationAction merge)
                {
                    EvaluateMergeRule(pageState, rule, merge, context, stageClaims, skippedClaims);
                    continue;
                }

                if (stage == Stage.Refine && rule.Action is StructureAttributeRemediationAction attributes)
                {
                    EvaluateRefineAttributeRule(pageState, rule, attributes, context, stageClaims, skippedClaims);
                    continue;
                }

                if (stage == Stage.Refine && rule.Action is ReorderSiblingsRemediationAction reorder)
                {
                    EvaluateRefineReorderRule(pageState, rule, reorder, context, stageClaims, skippedClaims);
                    continue;
                }

                if (stage == Stage.Refine && rule.Action is StructureLinkRemediationAction link)
                {
                    EvaluateRefineLinkRule(pageState, rule, link, context, stageClaims, skippedClaims, diagnostics);
                }
            }

            pageState.SetClaimSnapshot(stage, stageClaims.ToList());
            allClaims.AddRange(stageClaims);
            stageClaims.Clear();
        }

        ApplyLeftoverPolicy(pageState, ownedTargets, diagnostics, apply);
    }

    private static void CheckFlowRegionDiagnostics(RemediationEvaluationContext context, List<string> diagnostics)
    {
        var resolved = new List<FlowRegionResolution>();
        foreach (var region in context.FlowRegions.Values)
        {
            var resolution = context.ResolveFlowRegion(region.Id);
            if (resolution == null)
            {
                continue;
            }

            if (context.StructuredText != null &&
                !context.StructuredText.GetCandidates(Granularity.Line).Any(resolution.Contains))
            {
                diagnostics.Add($"Flow region '{region.Id}' contains no line candidates.");
            }

            resolved.Add(resolution);
        }

        for (var i = 0; i < resolved.Count; i++)
        {
            for (var j = i + 1; j < resolved.Count; j++)
            {
                if (resolved[i].Bounds.Intersects(resolved[j].Bounds))
                {
                    diagnostics.Add($"Flow regions '{resolved[i].RegionId}' and '{resolved[j].RegionId}' overlap on page {context.PageIndex + 1}.");
                }
            }
        }
    }

    private void EvaluateCustomRule(
        PageRemediationState pageState,
        Rule rule,
        CustomRemediationAction custom,
        RemediationEvaluationContext context,
        List<RemediationClaim> stageClaims,
        List<RemediationClaim> skippedClaims,
        List<string> diagnostics,
        bool apply)
    {
        var customCtx = new CustomRemediationContext(
            this,
            context,
            pageState.StructuredText.GetCandidates(rule.Granularity));

        CustomRemediationOutcome outcome;
        try
        {
            outcome = custom.Handler(customCtx);
        }
        catch (Exception ex)
        {
            diagnostics.Add($"Rule '{rule.Id}' custom handler failed: {ex.Message}");
            return;
        }

        if (outcome.ClaimedCandidates.Count == 0)
        {
            return;
        }

        var claim = new RemediationClaim(
            rule.Id,
            rule.Granularity,
            outcome.ClaimedCandidates,
            custom.Description,
            1.0)
        {
            PageIndex = pageState.PageIndex,
            Status = ClaimStatus.Applied,
            SelectorDebugString = rule.Predicate.DebugString,
            Action = outcome.Action ?? custom,
            RuleSetId = rule.RuleSetId
        };

        if (apply)
        {
            ApplyClassifyClaim(pageState, claim, diagnostics);
        }

        stageClaims.Add(claim);
    }

    private void EvaluateClassifyRule(
        PageRemediationState pageState,
        Rule rule,
        RemediationEvaluationContext context,
        Dictionary<string, RemediationClaim> ownedTargets,
        List<RemediationClaim> stageClaims,
        List<RemediationClaim> skippedClaims,
        List<string> diagnostics,
        bool apply)
    {
        var candidateResults = pageState.StructuredText.GetCandidates(rule.Granularity)
            .Select(candidate => (Candidate: candidate, Result: rule.Predicate.Evaluate(context, candidate)))
            .Where(x => x.Result.IsMatch)
            .ToList();

        if (candidateResults.Count == 0 && ContainsFlowRegionPredicate(rule.Predicate))
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
                diagnostics.Add($"Rule '{rule.Id}' has an ambiguous nearest-anchor match with {candidateResults.Count} equally near candidates.");
                return;
            }
        }

        foreach (var (candidate, predicate) in candidateResults)
        {
            var confidence = predicate.Confidence;
            if (rule.MinConfidence is { } minConfidence && confidence < minConfidence)
            {
                skippedClaims.Add(CreateClaim(rule, pageState.PageIndex, candidate, ClaimStatus.Skipped, confidence));
                continue;
            }

            var targetKeys = GetTargetKeys(candidate);
            var conflicting = targetKeys.Where(ownedTargets.ContainsKey).ToList();
            if (conflicting.Count > 0 && !rule.Override)
            {
                skippedClaims.Add(CreateClaim(rule, pageState.PageIndex, candidate, ClaimStatus.Skipped, confidence));
                continue;
            }

            if (rule.Override)
            {
                foreach (var key in conflicting)
                {
                    if (ownedTargets.TryGetValue(key, out var previous))
                    {
                        stageClaims.Remove(previous);
                        skippedClaims.Add(previous with { Status = ClaimStatus.Overridden });
                    }

                    ownedTargets.Remove(key);
                }
            }

            var claim = CreateClaim(rule, pageState.PageIndex, candidate, ClaimStatus.Applied, confidence);
            foreach (var key in targetKeys)
            {
                ownedTargets[key] = claim;
            }

            if (apply)
            {
                ApplyClassifyClaim(pageState, claim, diagnostics);
            }

            stageClaims.Add(claim);
        }
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
        Dictionary<string, RemediationClaim> ownedTargets,
        List<string> diagnostics,
        bool apply)
    {
        if (Configuration.LeftoverPolicy == RemediationLeftoverPolicy.Flag)
        {
            return;
        }

        var leftovers = EnumerateItems(pageState.WorkingContent)
            .Where(x => x is TextContent<double> && x.SourceReference is { })
            .Where(x => !ownedTargets.ContainsKey(GetItemKey(x)))
            .ToList();
        if (leftovers.Count == 0)
        {
            return;
        }

        if (Configuration.LeftoverPolicy == RemediationLeftoverPolicy.FailFast)
        {
            diagnostics.Add($"Page {pageState.PageIndex + 1} has {leftovers.Count} unclaimed text content item(s).");
            return;
        }

        if (!apply || Configuration.LeftoverPolicy != RemediationLeftoverPolicy.AutoArtifact)
        {
            return;
        }

        foreach (var item in leftovers)
        {
            pageState.WorkingContent.Wrap(
                new[] { item },
                new MarkedContent(PdfName.Artifact));
            pageState.MarkDirty();
        }
    }

    private void ApplyClassifyClaim(
        PageRemediationState pageState,
        RemediationClaim claim,
        List<string> diagnostics)
    {
        if (claim.Action is not (TagRemediationAction or ArtifactRemediationAction))
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

            if (claim.Action is TagRemediationAction tag)
            {
                var mcid = AllocateMcid(pageState.Page);
                var marked = new MarkedContent(tag.Name)
                {
                    InlineProps = new PdfDictionary { [PdfName.MCID] = new PdfIntNumber(mcid) }
                };
                var wrapper = pageState.WorkingContent.Wrap(leaves, marked);
                var node = Structure.AddElement(tag.Name.Value).GetNode();
                if (Configuration.DebugWrite)
                {
                    node.Title = claim.RuleId;
                }

                if (tag.Attributes != null)
                {
                    node.Attributes.Add(tag.Attributes);
                }

                BindMarkedContent(node, pageState.Page, mcid);
                claim.AddAppliedBinding(new RemediationAppliedBinding(
                    tag.Name.Value,
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
                        InlineProps = new PdfDictionary { [PdfName.TYPE] = (PdfName)artifact.Subtype.ToString() }
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

    private static void EvaluateTableRule(
        PageRemediationState pageState,
        Rule rule,
        TableRemediationAction table,
        RemediationEvaluationContext context,
        List<RemediationClaim> stageClaims,
        List<RemediationClaim> skippedClaims,
        List<string> diagnostics)
    {
        if (table.Over != null)
        {
            EvaluateClaimConsumingTableRule(pageState, rule, table, context, stageClaims, skippedClaims, diagnostics);
            return;
        }

        var candidates = new List<RemediationCandidate>();
        var confidence = 1.0;
        foreach (var candidate in pageState.StructuredText.GetCandidates(rule.Granularity))
        {
            var predicate = rule.Predicate.Evaluate(context, candidate);
            if (!predicate.IsMatch)
            {
                continue;
            }

            if (rule.MinConfidence is { } predicateMinConfidence && predicate.Confidence < predicateMinConfidence)
            {
                skippedClaims.Add(CreateClaim(rule, pageState.PageIndex, candidate, ClaimStatus.Skipped, predicate.Confidence));
                continue;
            }

            candidates.Add(candidate);
            confidence = Math.Min(confidence, predicate.Confidence);
        }

        if (candidates.Count == 0)
        {
            return;
        }

        var grid = ResolveTableGrid(table, candidates);
        if (grid.Error != null)
        {
            diagnostics.Add($"Rule '{rule.Id}' could not resolve table columns: {grid.Error}");
            return;
        }

        confidence = Math.Min(confidence, grid.Confidence);
        if (rule.MinConfidence is { } tableMinConfidence && confidence < tableMinConfidence)
        {
            skippedClaims.Add(new RemediationClaim(
                rule.Id,
                rule.Granularity,
                candidates,
                rule.Action.DebugString,
                confidence)
                {
                    PageIndex = pageState.PageIndex,
                    Status = ClaimStatus.Skipped,
                    Action = rule.Action,
                    RuleSetId = rule.RuleSetId
                });
            return;
        }

        var claim = new RemediationClaim(
            rule.Id,
            rule.Granularity,
            candidates,
            rule.Action.DebugString,
            confidence)
        {
            PageIndex = pageState.PageIndex,
            Status = ClaimStatus.Applied,
            SelectorDebugString = rule.Predicate.DebugString,
            Action = rule.Action,
            RuleSetId = rule.RuleSetId
        };
        stageClaims.Add(claim);
    }

    private static void EvaluateClaimConsumingTableRule(
        PageRemediationState pageState,
        Rule rule,
        TableRemediationAction table,
        RemediationEvaluationContext context,
        List<RemediationClaim> stageClaims,
        List<RemediationClaim> skippedClaims,
        List<string> diagnostics)
    {
        var classifyClaims = pageState.GetClaimSnapshot(Stage.Classify)
            .Where(x => x.Status == ClaimStatus.Applied)
            .OrderBy(x => x.FirstSequenceIndex)
            .ToList();
        var matchedClaims = new List<RemediationClaim>();
        var confidence = 1.0;
        RemediationClaim? previous = null;
        foreach (var claim in classifyClaims)
        {
            var predicateContext = new ClaimPredicateEvaluationContext(
                classifyClaims,
                PreviousClaim: previous,
                PageBox: context.PageBox,
                Configuration: context.Configuration,
                Anchors: context.Anchors,
                TolerancedZones: context.TolerancedZones,
                FlowRegions: context.FlowRegions,
                StructuredText: context.StructuredText,
                Diagnostics: context.Diagnostics);
            var result = table.Over!.Evaluate(predicateContext, claim);
            previous = claim;
            if (!result.IsMatch)
            {
                continue;
            }

            if (rule.MinConfidence is { } minConfidence && result.Confidence < minConfidence)
            {
                var skipped = new RemediationClaim(
                    rule.Id,
                    rule.Granularity,
                    claim.Candidates,
                    rule.Action.DebugString,
                    result.Confidence)
                {
                    PageIndex = pageState.PageIndex,
                    Status = ClaimStatus.Skipped,
                    Action = rule.Action,
                    RuleSetId = rule.RuleSetId
                };
                skipped.AddRelatedClaims(new[] { claim });
                skippedClaims.Add(skipped);
                continue;
            }

            matchedClaims.Add(claim);
            confidence = Math.Min(confidence, result.Confidence);
        }

        if (matchedClaims.Count == 0)
        {
            return;
        }

        var candidates = matchedClaims.SelectMany(x => x.Candidates).ToList();
        var grid = ResolveTableGrid(table, candidates);
        if (grid.Error != null)
        {
            diagnostics.Add($"Rule '{rule.Id}' could not resolve table columns: {grid.Error}");
            return;
        }

        confidence = Math.Min(confidence, grid.Confidence);
        if (rule.MinConfidence is { } tableMinConfidence && confidence < tableMinConfidence)
        {
            var skipped = new RemediationClaim(
                rule.Id,
                rule.Granularity,
                candidates,
                rule.Action.DebugString,
                confidence)
            {
                PageIndex = pageState.PageIndex,
                Status = ClaimStatus.Skipped,
                Action = rule.Action,
                RuleSetId = rule.RuleSetId
            };
            skipped.AddRelatedClaims(matchedClaims);
            skippedClaims.Add(skipped);
            return;
        }

        var tableClaim = new RemediationClaim(
            rule.Id,
            rule.Granularity,
            candidates,
            rule.Action.DebugString,
            confidence)
        {
            PageIndex = pageState.PageIndex,
            Status = ClaimStatus.Applied,
            SelectorDebugString = rule.Predicate.DebugString,
            Action = rule.Action,
            RuleSetId = rule.RuleSetId
        };
        tableClaim.AddRelatedClaims(matchedClaims);
        stageClaims.Add(tableClaim);
    }

    private static void EvaluateGroupRule(
        PageRemediationState pageState,
        Rule rule,
        GroupRemediationAction group,
        RemediationEvaluationContext context,
        List<RemediationClaim> stageClaims,
        List<RemediationClaim> skippedClaims)
    {
        EvaluateClaimRunRule(pageState, rule, group.Over, context, stageClaims, skippedClaims);
    }

    private static void EvaluateMergeRule(
        PageRemediationState pageState,
        Rule rule,
        MergeRemediationAction merge,
        RemediationEvaluationContext context,
        List<RemediationClaim> stageClaims,
        List<RemediationClaim> skippedClaims)
    {
        EvaluateClaimRunRule(pageState, rule, merge.Over, context, stageClaims, skippedClaims);
    }

    private static void EvaluateClaimRunRule(
        PageRemediationState pageState,
        Rule rule,
        ClaimPredicate over,
        RemediationEvaluationContext context,
        List<RemediationClaim> stageClaims,
        List<RemediationClaim> skippedClaims)
    {
        var classifyClaims = pageState.GetClaimSnapshot(Stage.Classify)
            .Where(x => x.Status == ClaimStatus.Applied)
            .OrderBy(x => x.FirstSequenceIndex)
            .ToList();
        var currentRun = new List<RemediationClaim>();
        foreach (var claim in classifyClaims)
        {
            var predicateContext = new ClaimPredicateEvaluationContext(
                classifyClaims,
                PreviousClaim: currentRun.LastOrDefault(),
                PageBox: context.PageBox,
                Configuration: context.Configuration,
                Anchors: context.Anchors,
                TolerancedZones: context.TolerancedZones,
                FlowRegions: context.FlowRegions,
                StructuredText: context.StructuredText,
                Diagnostics: context.Diagnostics);
            var result = over.Evaluate(predicateContext, claim);
            var startsNewRun = currentRun.Count > 0 && currentRun[^1].PageIndex != claim.PageIndex;

            if (!result.IsMatch || startsNewRun)
            {
                AddGroupRun(rule, pageState.PageIndex, currentRun, stageClaims);
                currentRun.Clear();
                if (!startsNewRun)
                {
                    result = over.Evaluate(
                        new ClaimPredicateEvaluationContext(
                            classifyClaims,
                            PageBox: context.PageBox,
                            Configuration: context.Configuration,
                            Anchors: context.Anchors,
                            TolerancedZones: context.TolerancedZones,
                            FlowRegions: context.FlowRegions,
                            StructuredText: context.StructuredText,
                            Diagnostics: context.Diagnostics),
                        claim);
                }
            }

            if (result.IsMatch)
            {
                if (rule.MinConfidence is { } minConfidence && result.Confidence < minConfidence)
                {
                    var skipped = new RemediationClaim(
                        rule.Id,
                        rule.Granularity,
                        claim.Candidates,
                        rule.Action.DebugString,
                        result.Confidence)
                    {
                        PageIndex = pageState.PageIndex,
                        Status = ClaimStatus.Skipped,
                        Action = rule.Action,
                        RuleSetId = rule.RuleSetId
                    };
                    skipped.AddRelatedClaims(new[] { claim });
                    skippedClaims.Add(skipped);
                }
                else
                {
                    currentRun.Add(claim);
                }
            }
        }

        AddGroupRun(rule, pageState.PageIndex, currentRun, stageClaims);
    }

    private static void EvaluateRefineAttributeRule(
        PageRemediationState pageState,
        Rule rule,
        StructureAttributeRemediationAction attributes,
        RemediationEvaluationContext context,
        List<RemediationClaim> stageClaims,
        List<RemediationClaim> skippedClaims)
    {
        var existingClaims = pageState.GetClaimSnapshot(Stage.Classify)
            .Concat(pageState.GetClaimSnapshot(Stage.Group))
            .Where(x => x.Status == ClaimStatus.Applied)
            .OrderBy(x => x.FirstSequenceIndex)
            .ToList();
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
                Diagnostics: context.Diagnostics);
            previous = claim;
            var result = attributes.Over.Evaluate(predicateContext, claim);
            if (!result.IsMatch)
            {
                continue;
            }

            if (rule.MinConfidence is { } minConfidence && result.Confidence < minConfidence)
            {
                var skipped = new RemediationClaim(
                    rule.Id,
                    rule.Granularity,
                    claim.Candidates,
                    rule.Action.DebugString,
                    result.Confidence)
                {
                    PageIndex = pageState.PageIndex,
                    Status = ClaimStatus.Skipped,
                    Action = rule.Action,
                    RuleSetId = rule.RuleSetId
                };
                skipped.AddRelatedClaims(new[] { claim });
                skippedClaims.Add(skipped);
                continue;
            }

            var refineClaim = new RemediationClaim(
                rule.Id,
                rule.Granularity,
                claim.Candidates,
                rule.Action.DebugString,
                result.Confidence)
            {
                PageIndex = pageState.PageIndex,
                Status = ClaimStatus.Applied,
            SelectorDebugString = rule.Predicate.DebugString,
                Action = rule.Action,
                RuleSetId = rule.RuleSetId
            };
            refineClaim.AddRelatedClaims(new[] { claim });
            stageClaims.Add(refineClaim);
        }
    }

    private static void EvaluateRefineReorderRule(
        PageRemediationState pageState,
        Rule rule,
        ReorderSiblingsRemediationAction reorder,
        RemediationEvaluationContext context,
        List<RemediationClaim> stageClaims,
        List<RemediationClaim> skippedClaims)
    {
        var existingClaims = pageState.GetClaimSnapshot(Stage.Classify)
            .Concat(pageState.GetClaimSnapshot(Stage.Group))
            .Where(x => x.Status == ClaimStatus.Applied)
            .OrderBy(x => x.FirstSequenceIndex)
            .ToList();
        var matched = new List<RemediationClaim>();
        var confidence = 1.0;
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
                Diagnostics: context.Diagnostics);
            previous = claim;
            var result = reorder.Over.Evaluate(predicateContext, claim);
            if (!result.IsMatch)
            {
                continue;
            }

            if (rule.MinConfidence is { } minConfidence && result.Confidence < minConfidence)
            {
                var skipped = new RemediationClaim(
                    rule.Id,
                    rule.Granularity,
                    claim.Candidates,
                    rule.Action.DebugString,
                    result.Confidence)
                {
                    PageIndex = pageState.PageIndex,
                    Status = ClaimStatus.Skipped,
                    Action = rule.Action,
                    RuleSetId = rule.RuleSetId
                };
                skipped.AddRelatedClaims(new[] { claim });
                skippedClaims.Add(skipped);
                continue;
            }

            matched.Add(claim);
            confidence = Math.Min(confidence, result.Confidence);
        }

        if (matched.Count == 0)
        {
            return;
        }

        var refineClaim = new RemediationClaim(
            rule.Id,
            rule.Granularity,
            matched.SelectMany(x => x.Candidates).ToArray(),
            rule.Action.DebugString,
            confidence)
        {
            PageIndex = pageState.PageIndex,
            Status = ClaimStatus.Applied,
            SelectorDebugString = rule.Predicate.DebugString,
            Action = rule.Action,
            RuleSetId = rule.RuleSetId
        };
        refineClaim.AddRelatedClaims(matched);
        stageClaims.Add(refineClaim);
    }

    private static void EvaluateRefineLinkRule(
        PageRemediationState pageState,
        Rule rule,
        StructureLinkRemediationAction link,
        RemediationEvaluationContext context,
        List<RemediationClaim> stageClaims,
        List<RemediationClaim> skippedClaims,
        List<string> diagnostics)
    {
        var existingClaims = pageState.GetClaimSnapshot(Stage.Classify)
            .Concat(pageState.GetClaimSnapshot(Stage.Group))
            .Where(x => x.Status == ClaimStatus.Applied)
            .OrderBy(x => x.FirstSequenceIndex)
            .ToList();
        var sources = new List<(RemediationClaim Claim, double Confidence)>();
        var targets = new List<(RemediationClaim Claim, double Confidence)>();
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
                Diagnostics: context.Diagnostics);
            previous = claim;

            var sourceResult = link.Source.Evaluate(predicateContext, claim);
            if (sourceResult.IsMatch)
            {
                if (rule.MinConfidence is { } minConfidence && sourceResult.Confidence < minConfidence)
                {
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
                AddSkippedClaim(rule, pageState.PageIndex, source.Claim, confidence, skippedClaims);
                continue;
            }

            var linkClaim = new RemediationClaim(
                rule.Id,
                rule.Granularity,
                source.Claim.Candidates,
                rule.Action.DebugString,
                confidence)
            {
                PageIndex = pageState.PageIndex,
                Status = ClaimStatus.Applied,
            SelectorDebugString = rule.Predicate.DebugString,
                Action = rule.Action,
                RuleSetId = rule.RuleSetId
            };
            linkClaim.AddRelatedClaims(new[] { source.Claim, target.Claim });
            stageClaims.Add(linkClaim);
        }
    }

    private static void AddSkippedClaim(
        Rule rule,
        int pageIndex,
        RemediationClaim related,
        double confidence,
        List<RemediationClaim> skippedClaims)
    {
        var skipped = new RemediationClaim(
            rule.Id,
            rule.Granularity,
            related.Candidates,
            rule.Action.DebugString,
            confidence)
        {
            PageIndex = pageIndex,
            Status = ClaimStatus.Skipped,
            Action = rule.Action,
            RuleSetId = rule.RuleSetId
        };
        skipped.AddRelatedClaims(new[] { related });
        skippedClaims.Add(skipped);
    }

    private static void AddGroupRun(
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
            rule.Granularity,
            run.SelectMany(x => x.Candidates).ToArray(),
            rule.Action.DebugString,
            confidence)
        {
            PageIndex = pageIndex,
            Status = ClaimStatus.Applied,
            SelectorDebugString = rule.Predicate.DebugString,
            Action = rule.Action,
            RuleSetId = rule.RuleSetId
        };
        groupClaim.AddRelatedClaims(run);
        stageClaims.Add(groupClaim);
    }

    private void ValidatePlan(
        IReadOnlyList<PageRemediationState> pageStates,
        List<string> diagnostics)
    {
        foreach (var pageState in pageStates)
        {
            foreach (var claim in pageState.GetClaimSnapshot(Stage.Classify).Where(x => x.Status == ClaimStatus.Applied))
            {
                ValidateClaimTargets(pageState, claim, diagnostics);
                ValidateClaimStructureBinding(pageState, claim, diagnostics);
            }

            foreach (var claim in pageState.GetClaimSnapshot(Stage.Group).Where(x => x.Status == ClaimStatus.Applied))
            {
                ValidateTableClaim(pageState, claim, diagnostics);
                ValidateClaimConsumingBindings(pageState, claim, diagnostics);
            }

            foreach (var claim in pageState.GetClaimSnapshot(Stage.Refine).Where(x => x.Status == ClaimStatus.Applied))
            {
                ValidateClaimConsumingBindings(pageState, claim, diagnostics);
            }
        }
    }

    private static void ValidateClaimTargets(
        PageRemediationState pageState,
        RemediationClaim claim,
        List<string> diagnostics)
    {
        if (claim.Action is not (TagRemediationAction or ArtifactRemediationAction))
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
                target.TextRange.StartCharacterIndex,
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
            diagnostics.Add($"Rule '{claim.RuleId}' on page {pageState.PageIndex + 1} could not resolve table columns: {grid.Error}");
            return;
        }

        foreach (var candidate in claim.Candidates)
        {
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
        if (claim.Action is not (GroupRemediationAction or MergeRemediationAction or TableRemediationAction { Over: not null } or StructureAttributeRemediationAction or ReorderSiblingsRemediationAction or StructureLinkRemediationAction))
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
            if (related.Action is not (TagRemediationAction or TableRemediationAction or GroupRemediationAction or MergeRemediationAction))
            {
                diagnostics.Add($"Rule '{claim.RuleId}' on page {pageState.PageIndex + 1} matched claim '{related.ClaimId}' that does not produce a structure binding.");
            }
        }
    }

    private void ApplyPlan(
        IReadOnlyList<PageRemediationState> pageStates,
        List<string> diagnostics)
    {
        foreach (var pageState in pageStates)
        {
            foreach (var claim in pageState.GetClaimSnapshot(Stage.Classify).Where(x => x.Status == ClaimStatus.Applied))
            {
                ApplyClassifyClaim(pageState, claim, diagnostics);
                if (diagnostics.Count > 0)
                {
                    return;
                }
            }

            foreach (var claim in pageState.GetClaimSnapshot(Stage.Group).Where(x => x.Status == ClaimStatus.Applied))
            {
                if (claim.Action is TableRemediationAction)
                {
                    ApplyTableClaim(pageState, claim, diagnostics);
                }
                else if (claim.Action is GroupRemediationAction)
                {
                    ApplyGroupClaim(claim, diagnostics);
                }
                else if (claim.Action is MergeRemediationAction)
                {
                    ApplyMergeClaim(pageState, claim, diagnostics);
                }

                if (diagnostics.Count > 0)
                {
                    return;
                }
            }

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

                if (diagnostics.Count > 0)
                {
                    return;
                }
            }

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
            .GroupBy(x => GetRowKey(x.Candidate))
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
        var cells = claim.RelatedClaims
            .Select(related => (Claim: related, Column: GetColumnIndex(grid.Columns, GetCenterX(related))))
            .Where(x => x.Column >= 0)
            .GroupBy(x => GetRowKey(x.Claim))
            .OrderByDescending(x => x.Max(y => y.Claim.BoundingBox?.URy ?? 0))
            .ToList();

        var tableNode = Structure.AddElement("Table").GetNode();
        if (Configuration.DebugWrite)
        {
            tableNode.Title = claim.RuleId;
        }
        var reusedMcids = new List<int>();
        for (var rowIndex = 0; rowIndex < cells.Count; rowIndex++)
        {
            var row = cells[rowIndex];
            var rowClaims = row.Select(x => x.Claim).ToList();
            var isHeaderRow = rowIndex < table.HeaderRows || RowMatchesHeaderSelector(table, rowClaims, claim.RelatedClaims);
            var rowNode = new StructuralContext(Structure, tableNode, Structure)
                .AddElement("TR")
                .GetNode();
            foreach (var cell in row.OrderBy(x => x.Column).ThenBy(x => x.Claim.BoundingBox?.LLx ?? 0))
            {
                var isHeader = isHeaderRow;
                var cellNode = new StructuralContext(Structure, rowNode, Structure)
                    .AddElement(isHeader ? "TH" : "TD")
                    .GetNode();
                if (isHeader)
                {
                    cellNode.Scope = StructureScope.Column;
                }

                var bindings = cell.Claim.AppliedBindings.Where(x => x.StructureNode != null).ToList();
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

                        FlattenBindingInto(binding, cellNode, isHeader ? "TH" : "TD");
                        pageState.MarkDirty();
                    }
                    else
                    {
                        Structure.ReparentStructureNode(child, cellNode);
                    }

                    reusedMcids.AddRange(binding.Mcids);
                    claim.AddAppliedBinding(new RemediationAppliedBinding(
                        isHeader ? "TH" : "TD",
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

    private static double GetRowKey(RemediationCandidate candidate) =>
        Math.Round(candidate.RelativeBoundingBox.LLy / 2d) * 2d;

    private static double GetRowKey(RemediationClaim claim) =>
        claim.BoundingBox is { } box ? Math.Round(box.LLy / 2d) * 2d : 0d;

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
            .GroupBy(GetRowKey)
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
        claim.BoundingBox is { } box ? (box.LLx + box.URx) / 2d : 0d;

    private sealed record TableGridResolution(
        IReadOnlyList<double> Columns,
        double Confidence,
        bool Inferred,
        string? Error = null);

    private void CommitDocumentChanges(Action commit)
    {
        var catalogSnapshot = _document.Catalog.CloneShallow();
        var pageSnapshots = _document.Pages
            .Select(x => (Page: x, NativeObject: x.NativeObject.CloneShallow()))
            .ToList();

        try
        {
            commit();
        }
        catch
        {
            RestoreDictionary(_document.Catalog, catalogSnapshot);
            foreach (var snapshot in pageSnapshots)
            {
                RestoreDictionary(snapshot.Page.NativeObject, snapshot.NativeObject);
            }

            throw;
        }
    }

    private void ApplyGroupClaim(
        RemediationClaim claim,
        List<string> diagnostics)
    {
        if (claim.Action is not GroupRemediationAction group)
        {
            return;
        }

        var parentNode = Structure.AddElement(group.ParentTag.Value).GetNode();
        if (Configuration.DebugWrite)
        {
            parentNode.Title = claim.RuleId;
        }
        var reusedMcids = new List<int>();
        foreach (var related in claim.RelatedClaims.OrderBy(x => x.FirstSequenceIndex))
        {
            var bindings = related.AppliedBindings.Where(x => x.StructureNode != null).ToList();
            if (bindings.Count == 0)
            {
                diagnostics.Add($"Rule '{claim.RuleId}' matched claim '{related.ClaimId}' without a reusable structure binding.");
                return;
            }

            foreach (var binding in bindings)
            {
                Structure.ReparentStructureNode(binding.StructureNode!, parentNode);
                reusedMcids.AddRange(binding.Mcids);
            }
        }

        claim.AddAppliedBinding(new RemediationAppliedBinding(
            group.ParentTag.Value,
            reusedMcids,
            parentNode,
            null,
            parentNode.Parent,
            claim.Candidates.SelectMany(x => x.SourceReferences).ToArray(),
            claim.BoundingBox));
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
        foreach (var related in claim.RelatedClaims.OrderBy(x => x.FirstSequenceIndex))
        {
            var bindings = related.AppliedBindings.Where(x => x.StructureNode != null).ToList();
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
            var bindings = related.AppliedBindings.Where(x => x.StructureNode != null).ToList();
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
            var bindings = related.AppliedBindings.Where(x => x.StructureNode != null).ToList();
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
                _ => x.Claim.FirstSequenceIndex.CompareTo(y.Claim.FirstSequenceIndex)
            };

            return result != 0
                ? result
                : x.Claim.FirstSequenceIndex.CompareTo(y.Claim.FirstSequenceIndex);
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

        var claimedKeys = pageState.GetClaimSnapshot(Stage.Classify)
            .Concat(pageState.GetClaimSnapshot(Stage.Group))
            .Where(x => x.Status == ClaimStatus.Applied)
            .SelectMany(x => x.Candidates)
            .SelectMany(GetTargetKeys)
            .ToHashSet(StringComparer.Ordinal);
        var leftovers = EnumerateItems(pageState.WorkingContent)
            .Where(x => x is TextContent<double> && x.SourceReference is { })
            .Where(x => !claimedKeys.Contains(GetItemKey(x)))
            .ToList();
        foreach (var item in leftovers)
        {
            pageState.WorkingContent.Wrap(
                new[] { item },
                new MarkedContent(PdfName.Artifact));
            pageState.MarkDirty();
        }
    }

    private static RemediationClaim CreateClaim(Rule rule, int pageIndex, RemediationCandidate candidate, ClaimStatus status, double confidence)
    {
        return new RemediationClaim(
            rule.Id,
            rule.Granularity,
            new[] { candidate },
            rule.Action.DebugString,
            confidence)
        {
            PageIndex = pageIndex,
            Status = status,
            SelectorDebugString = rule.Predicate.DebugString,
            Action = rule.Action,
            RuleSetId = rule.RuleSetId
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
        foreach (var pageState in pageStates)
        {
            var pageScope = $"Page{pageState.PageIndex + 1}";
            var pageMcidsInLogicalOrder = new List<int>();

            void CollectMcidsLogical(StructureNode node)
            {
                foreach (var contentItem in node.ContentItems)
                {
                    if (contentItem.Page == pageState.Page)
                    {
                        pageMcidsInLogicalOrder.Add(contentItem.MCID);
                    }
                }
                foreach (var child in node.Children)
                {
                    CollectMcidsLogical(child);
                }
            }

            CollectMcidsLogical(Structure.GetRoot());

            for (var i = 1; i < pageMcidsInLogicalOrder.Count; i++)
            {
                if (pageMcidsInLogicalOrder[i] < pageMcidsInLogicalOrder[i - 1])
                {
                    ReportDiagnostic(DiagnosticCode.ReadingOrderDrift, pageScope, $"Logical reading order drift detected on page {pageState.PageIndex + 1}: MCID {pageMcidsInLogicalOrder[i]} appears after MCID {pageMcidsInLogicalOrder[i - 1]}.", diagnostics);
                    break;
                }
            }
        }
    }

    private void CheckUntaggedContent(PageRemediationState pageState, string scope, List<string> diagnostics)
    {
        var untagged = new List<IContentItem<double>>();
        CheckUntaggedRecursive(pageState.WorkingContent, false, untagged);

        if (untagged.Count > 0)
        {
            var msg = $"Page {pageState.PageIndex + 1} has {untagged.Count} operator(s) outside of any marked content sequence.";
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
            if (_pageMcids.TryGetValue(pageState.Page, out var count) && count > 0)
            {
                if (!_pageStructParents.ContainsKey(pageState.Page))
                {
                    ReportDiagnostic(DiagnosticCode.MissingStructParents, $"Page{pageState.PageIndex + 1}", $"Page {pageState.PageIndex + 1} has MCIDs but no /StructParents entry.", diagnostics);
                }
            }
        }
    }

    private void ReportDiagnostic(DiagnosticCode code, string scope, string message, List<string> diagnostics)
    {
        var strict = Configuration.DiagnosticStrictness == RemediationDiagnosticStrictness.Strict;
        var suppression = _suppressions.FirstOrDefault(x => x.Code == code && (x.Scope == "*" || x.Scope == scope));
        
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

    private static IReadOnlyList<string> GetTargetKeys(RemediationCandidate candidate)
    {
        if (candidate.TextRanges.Count > 0 &&
            candidate.Granularity is Granularity.Character or Granularity.Word)
        {
            return candidate.TextRanges
                .Select(x => $"{x.SourceReference.StreamId}:{x.SourceReference.OperatorStart}:{x.StartCharacterIndex}:{x.CharacterCount}")
                .ToArray();
        }

        return candidate.SourceReferences
            .Select(x => $"{x.StreamId}:{x.OperatorStart}:{x.OperatorLength}")
            .ToArray();
    }

    private static string GetItemKey(IContentItem<double> item)
    {
        return item.SourceReference is { } sourceReference
            ? $"{sourceReference.StreamId}:{sourceReference.OperatorStart}:{sourceReference.OperatorLength}"
            : $"item:{RuntimeHelpers.GetHashCode(item)}";
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

        if (!_pageMcids.TryGetValue(page, out var nextMcid))
        {
            _pageMcids[page] = 1;
            return 0;
        }

        _pageMcids[page] = nextMcid + 1;
        return nextMcid;
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

    internal void BindAnnotation(StructureNode node, PdfPage page, PdfDictionary annotation)
    {
        ThrowIfDisposed();

        var index = Structure.GetStructureRoot().AllocateStructParentIndex();
        annotation[PdfName.StructParent] = new PdfIntNumber(index);
        node.ObjectReferences.Add(new StructureObjectReference(annotation, index, page));
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
        _pageMcids.Clear();
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
