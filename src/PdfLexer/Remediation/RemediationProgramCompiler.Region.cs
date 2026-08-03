namespace PdfLexer.Remediation;

public static partial class RemediationProgramCompiler
{
    private static void ValidateRegions(
        RemediationProgram program,
        IReadOnlySet<string> anchorIds,
        IReadOnlySet<string> artifactIds,
        List<RemediationProgramDiagnostic> diagnostics)
    {
        var regionIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var region in program.Regions)
        {
            if (!regionIds.Add(region.Id))
                Add(diagnostics, DuplicateId, $"region:{region.Id}", $"Region id '{region.Id}' is duplicated.");
        }

        var regions = program.Regions.GroupBy(x => x.Id, StringComparer.Ordinal)
            .ToDictionary(x => x.Key, x => x.First(), StringComparer.Ordinal);
        foreach (var region in program.Regions)
            ValidateRegionExpression(region.Id, region.Expression, anchorIds, regions, diagnostics);

        ValidateRegionCycles(regions, diagnostics);

        var accountingIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var accounting in program.RegionAccounting)
        {
            var scope = $"regionAccounting:{accounting.Id}";
            if (!accountingIds.Add(accounting.Id))
                Add(diagnostics, DuplicateId, scope, $"Region accounting id '{accounting.Id}' is duplicated.");
            if (!regionIds.Contains(accounting.RegionId))
                Add(diagnostics, InvalidReference, scope,
                    $"Region accounting '{accounting.Id}' references unknown region '{accounting.RegionId}'.");
            if (!artifactIds.Contains(accounting.ArtifactId))
                Add(diagnostics, InvalidReference, scope,
                    $"Region accounting '{accounting.Id}' references unknown artifact '{accounting.ArtifactId}'.");
            if (accounting.CandidateKinds.Any(x => !Enum.IsDefined(x) ||
                    x is RemediationCandidateKind.Text or RemediationCandidateKind.Annotation))
                Add(diagnostics, InvalidValue, scope,
                    $"Region accounting '{accounting.Id}' contains an ineligible painting candidate kind.");
            if (!accounting.AllowText && accounting.TextPredicate != null)
                Add(diagnostics, InvalidValue, scope,
                    $"Region accounting '{accounting.Id}' declares a text predicate while allowText is false.");
            if (accounting.AllowText)
            {
                var artifact = program.Artifacts.FirstOrDefault(x => x.Id == accounting.ArtifactId);
                var hasBound = artifact?.Occurrence.Max != null;
                if ((accounting.TextPredicate == null || IsAlways(accounting.TextPredicate)) && !hasBound)
                    Add(diagnostics, InvalidValue, scope,
                        $"Region accounting '{accounting.Id}' must bound allowed text with a nontrivial predicate or finite artifact maximum.");
            }
            if (accounting.TextPredicate != null)
            {
                ValidatePredicate(accounting.TextPredicate, $"{scope}:textPredicate", accounting.Id, diagnostics);
                ValidateRegionPredicateReferences(accounting.TextPredicate, regionIds, scope, diagnostics);
            }
        }

        foreach (var binding in program.Bindings)
            ValidateRegionPredicateReferences(binding.Predicate, regionIds, $"binding:{binding.Id}:predicate", diagnostics);
        foreach (var boundary in program.Boundaries)
            ValidateRegionPredicateReferences(boundary.Predicate, regionIds, $"boundary:{boundary.Id}:predicate", diagnostics);
    }

    private static void ValidateRegionExpression(
        string id,
        Region expression,
        IReadOnlySet<string> anchorIds,
        IReadOnlyDictionary<string, RegionDeclaration> regions,
        List<RemediationProgramDiagnostic> diagnostics)
    {
        var scope = $"region:{id}";
        switch (expression)
        {
            case Region.Fixed fixedRegion:
                if (fixedRegion.Bounds is not (AbsoluteLayoutCoord or MarginRelativeLayoutCoord or
                    PercentageLayoutCoord or NamedZoneLayoutCoord))
                    Add(diagnostics, UnsupportedCapability, scope,
                        $"Fixed region '{id}' uses unsupported coordinate '{fixedRegion.Bounds.GetType().Name}'; use an anchored or flow expression.");
                if (fixedRegion.Bounds is AbsoluteLayoutCoord absolute &&
                    (!Finite(absolute.Rect.LLx) || !Finite(absolute.Rect.LLy) ||
                     !Finite(absolute.Rect.URx) || !Finite(absolute.Rect.URy) ||
                     absolute.Rect.URx <= absolute.Rect.LLx || absolute.Rect.URy <= absolute.Rect.LLy))
                    Add(diagnostics, InvalidValue, scope, $"Fixed region '{id}' has invalid absolute bounds.");
                break;
            case Region.Anchored anchored:
                if (!anchorIds.Contains(anchored.AnchorId))
                    Add(diagnostics, InvalidReference, scope,
                        $"Anchored region '{id}' references unknown anchor '{anchored.AnchorId}'.");
                if (!Enum.IsDefined(anchored.Placement) || !Finite(anchored.Extent) || anchored.Extent <= 0)
                    Add(diagnostics, InvalidValue, scope,
                        $"Anchored region '{id}' requires a supported placement and positive finite extent.");
                break;
            case Region.Tolerance tolerance:
                if (!Finite(tolerance.Amount) || tolerance.Amount < 0 || !Enum.IsDefined(tolerance.ConfidenceBehavior))
                    Add(diagnostics, InvalidValue, scope, $"Region '{id}' has invalid tolerance settings.");
                if (tolerance.Inner is Region.Tolerance)
                    Add(diagnostics, InvalidValue, scope, $"Region '{id}' cannot nest tolerance decorations.");
                ValidateRegionExpression(id, tolerance.Inner, anchorIds, regions, diagnostics);
                break;
            case Region.Flow flow:
                if (!Enum.IsDefined(flow.ContinuationPolicy) || !Enum.IsDefined(flow.ReadingOrderMode) ||
                    flow.MaxExtent is { } extent && (!Finite(extent) || extent <= 0) ||
                    flow.MaxPages is <= 0 ||
                    flow.ContinuationPolicy == FlowContinuationPolicy.CurrentPageOnly && flow.MaxPages != null)
                    Add(diagnostics, InvalidValue, scope, $"Flow region '{id}' has invalid continuation or extent settings.");
                ValidateRegionBoundary(id, "start", flow.Start, anchorIds, regions, diagnostics);
                ValidateRegionBoundary(id, "end", flow.End, anchorIds, regions, diagnostics);
                break;
            default:
                Add(diagnostics, UnsupportedCapability, scope,
                    $"Region '{id}' uses unsupported expression '{expression.GetType().Name}'.");
                break;
        }
    }

    private static void ValidateRegionBoundary(
        string id,
        string label,
        RegionBoundary boundary,
        IReadOnlySet<string> anchorIds,
        IReadOnlyDictionary<string, RegionDeclaration> regions,
        List<RemediationProgramDiagnostic> diagnostics)
    {
        var scope = $"region:{id}:{label}";
        switch (boundary)
        {
            case RegionBoundary.Anchor anchor when !anchorIds.Contains(anchor.AnchorId):
                Add(diagnostics, InvalidReference, scope,
                    $"Flow region '{id}' {label} references unknown anchor '{anchor.AnchorId}'.");
                break;
            case RegionBoundary.Region region when !regions.TryGetValue(region.RegionId, out _):
                Add(diagnostics, InvalidReference, scope,
                    $"Flow region '{id}' {label} references unknown region '{region.RegionId}'.");
                break;
            case RegionBoundary.Region region when BaseExpression(regions[region.RegionId].Expression) is Region.Flow:
                Add(diagnostics, InvalidValue, scope,
                    $"Flow region '{id}' {label} cannot use flowing region '{region.RegionId}' as a boundary.");
                break;
            case RegionBoundary.Matching matching:
                ValidateSelector(matching.Candidates, $"{scope}:candidates", diagnostics);
                ValidatePredicate(matching.Predicate, $"{scope}:predicate", id, diagnostics);
                ValidateRegionPredicateReferences(matching.Predicate, regions.Keys.ToHashSet(StringComparer.Ordinal), scope, diagnostics);
                break;
        }
    }

    private static void ValidateRegionPredicateReferences(
        RemediationPredicate predicate,
        IReadOnlySet<string> regionIds,
        string scope,
        List<RemediationProgramDiagnostic> diagnostics)
    {
        foreach (var id in EnumerateRegionReferences(predicate).Distinct(StringComparer.Ordinal))
            if (!regionIds.Contains(id))
                Add(diagnostics, InvalidReference, scope, $"Predicate references unknown region '{id}'.");
    }

    private static void ValidateRegionCycles(
        IReadOnlyDictionary<string, RegionDeclaration> regions,
        List<RemediationProgramDiagnostic> diagnostics)
    {
        var graph = regions.ToDictionary(
            x => x.Key,
            x => EnumerateRegionDependencies(x.Value.Expression).Where(regions.ContainsKey)
                .ToHashSet(StringComparer.Ordinal),
            StringComparer.Ordinal);
        var visiting = new HashSet<string>(StringComparer.Ordinal);
        var visited = new HashSet<string>(StringComparer.Ordinal);
        bool Visit(string id)
        {
            if (visiting.Contains(id)) return true;
            if (!visited.Add(id)) return false;
            visiting.Add(id);
            foreach (var dependency in graph[id])
                if (Visit(dependency)) return true;
            visiting.Remove(id);
            return false;
        }
        foreach (var id in graph.Keys.OrderBy(x => x, StringComparer.Ordinal))
            if (Visit(id))
            {
                Add(diagnostics, DependencyCycle, $"region:{id}", $"Region '{id}' participates in a dependency cycle.");
                break;
            }
    }

    private static IEnumerable<string> EnumerateRegionDependencies(Region expression)
    {
        switch (expression)
        {
            case Region.Tolerance tolerance:
                foreach (var id in EnumerateRegionDependencies(tolerance.Inner)) yield return id;
                break;
            case Region.Flow flow:
                foreach (var id in EnumerateBoundaryRegionReferences(flow.Start)) yield return id;
                foreach (var id in EnumerateBoundaryRegionReferences(flow.End)) yield return id;
                break;
        }
    }

    private static IEnumerable<string> EnumerateBoundaryRegionReferences(RegionBoundary boundary)
    {
        if (boundary is RegionBoundary.Region region) yield return region.RegionId;
        if (boundary is RegionBoundary.Matching matching)
            foreach (var id in EnumerateRegionReferences(matching.Predicate)) yield return id;
    }

    private static IEnumerable<string> EnumerateRegionReferences(RemediationPredicate predicate)
    {
        switch (predicate)
        {
            case RegionRemediationPredicate region:
                yield return region.RegionId;
                break;
            case CompositeRemediationPredicate composite:
                foreach (var id in EnumerateRegionReferences(composite.Left)) yield return id;
                foreach (var id in EnumerateRegionReferences(composite.Right)) yield return id;
                break;
            case NotRemediationPredicate not:
                foreach (var id in EnumerateRegionReferences(not.Inner)) yield return id;
                break;
        }
    }

    private static IEnumerable<string> EnumerateRegionAnchorReferences(
        RemediationPredicate predicate,
        IReadOnlyList<RegionDeclaration> declarations)
    {
        var regions = declarations.GroupBy(x => x.Id, StringComparer.Ordinal)
            .ToDictionary(x => x.Key, x => x.First(), StringComparer.Ordinal);
        foreach (var id in EnumerateRegionReferences(predicate).Distinct(StringComparer.Ordinal))
            foreach (var anchor in EnumerateRegionAnchors(id, regions, new HashSet<string>(StringComparer.Ordinal)))
                yield return anchor;
    }

    private static IEnumerable<string> EnumerateRegionAnchors(
        string id,
        IReadOnlyDictionary<string, RegionDeclaration> regions,
        HashSet<string> visited)
    {
        if (!visited.Add(id) || !regions.TryGetValue(id, out var declaration)) yield break;
        foreach (var anchor in EnumerateExpressionAnchors(declaration.Expression)) yield return anchor;
        foreach (var dependency in EnumerateRegionDependencies(declaration.Expression))
            foreach (var anchor in EnumerateRegionAnchors(dependency, regions, visited)) yield return anchor;
    }

    private static IEnumerable<string> EnumerateExpressionAnchors(Region expression)
    {
        switch (expression)
        {
            case Region.Anchored anchored:
                yield return anchored.AnchorId;
                break;
            case Region.Tolerance tolerance:
                foreach (var id in EnumerateExpressionAnchors(tolerance.Inner)) yield return id;
                break;
            case Region.Flow flow:
                foreach (var id in EnumerateBoundaryAnchors(flow.Start)) yield return id;
                foreach (var id in EnumerateBoundaryAnchors(flow.End)) yield return id;
                break;
        }
    }

    private static IEnumerable<string> EnumerateBoundaryAnchors(RegionBoundary boundary)
    {
        if (boundary is RegionBoundary.Anchor anchor) yield return anchor.AnchorId;
        if (boundary is RegionBoundary.Matching matching)
            foreach (var id in EnumerateAnchorReferences(matching.Predicate)) yield return id;
    }

    private static Region RewriteRegionExpression(Region expression, Func<string, string> qualify) =>
        expression switch
        {
            Region.Anchored anchored => anchored with { AnchorId = qualify(anchored.AnchorId) },
            Region.Tolerance tolerance => tolerance with { Inner = RewriteRegionExpression(tolerance.Inner, qualify) },
            Region.Flow flow => flow with
            {
                Start = RewriteRegionBoundary(flow.Start, qualify),
                End = RewriteRegionBoundary(flow.End, qualify)
            },
            _ => expression
        };

    private static RegionBoundary RewriteRegionBoundary(RegionBoundary boundary, Func<string, string> qualify) =>
        boundary switch
        {
            RegionBoundary.Anchor anchor => anchor with { AnchorId = qualify(anchor.AnchorId) },
            RegionBoundary.Region region => region with { RegionId = qualify(region.RegionId) },
            RegionBoundary.Matching matching => matching with
            {
                Predicate = RewritePredicate(matching.Predicate, qualify)
            },
            _ => boundary
        };

    private static Region BaseExpression(Region expression) =>
        expression is Region.Tolerance tolerance ? BaseExpression(tolerance.Inner) : expression;

    private static bool IsAlways(RemediationPredicate predicate) =>
        predicate is ConstantRemediationPredicate { Value: true };

    private static bool Finite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
}
