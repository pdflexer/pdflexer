using PdfLexer.Content;

namespace PdfLexer.Remediation;

/// <summary>
/// Resolves declared flow regions as document-order activation instances while retaining
/// page-local geometry and sequence bounds.
/// </summary>
internal sealed class DocumentFlowRegionResolver
{
    private readonly IReadOnlyList<PageRemediationState> _pages;
    private readonly IReadOnlyList<RemediationClaim> _claims;
    private readonly RemediationSessionConfiguration _configuration;
    private readonly IReadOnlyDictionary<string, RemediationAnchor> _anchors;
    private readonly IReadOnlyDictionary<string, TolerancedZone> _zones;
    private readonly IReadOnlyDictionary<string, FlowRegion> _regions;
    private readonly List<string> _diagnostics;

    public DocumentFlowRegionResolver(
        IReadOnlyList<PageRemediationState> pages,
        IReadOnlyList<RemediationClaim> claims,
        RemediationSessionConfiguration configuration,
        IReadOnlyDictionary<string, RemediationAnchor> anchors,
        IReadOnlyDictionary<string, TolerancedZone> zones,
        IReadOnlyDictionary<string, FlowRegion> regions,
        List<string> diagnostics)
    {
        _pages = pages;
        _claims = claims;
        _configuration = configuration;
        _anchors = anchors;
        _zones = zones;
        _regions = regions;
        _diagnostics = diagnostics;
    }

    public DocumentFlowIndex Resolve()
    {
        var byPage = _pages.ToDictionary(
            x => x.PageIndex,
            _ => new Dictionary<string, FlowRegionResolution>(StringComparer.Ordinal));

        foreach (var region in _regions.Values)
        {
            if (region.ContinuationPolicy == FlowContinuationPolicy.CurrentPageOnly)
            {
                ResolveCurrentPage(region, byPage);
            }
            else
            {
                ResolveContinued(region, byPage);
            }
        }

        CheckOverlaps(byPage);
        return new DocumentFlowIndex(
            byPage,
            _pages.SelectMany(x =>
                    x.TextCandidates.Values.SelectMany(y => y).Cast<RemediationCandidate>()
                        .Concat(x.ContentCandidates))
                .DistinctBy(x => (x.PageIndex, x.CandidateId))
                .ToArray(),
            _claims);
    }

    private void ResolveCurrentPage(
        FlowRegion region,
        IReadOnlyDictionary<int, Dictionary<string, FlowRegionResolution>> byPage)
    {
        var activationIndex = 0;
        var sawBoundary = false;
        foreach (var page in _pages)
        {
            var start = ProbeBoundary(page, region.Start, region, isStart: true);
            var end = ProbeBoundary(page, region.End, region, isStart: false, start?.SequenceIndex);
            sawBoundary |= start != null || end != null;
            if (start == null && end == null)
            {
                continue;
            }

            if (start == null || end == null)
            {
                _diagnostics.Add(
                    $"Flow region '{region.Id}' requires both boundaries on page {page.PageIndex + 1} " +
                    "when ContinuationPolicy is CurrentPageOnly.");
                continue;
            }

            var instanceId = new FlowRegionInstanceId(region.Id, activationIndex++);
            var resolution = CreateResolution(
                page, region, instanceId, FlowRegionPageRole.StartAndEnd, start, end);
            if (resolution != null)
            {
                byPage[page.PageIndex][region.Id] = resolution;
            }
        }

        if (!sawBoundary)
        {
            _diagnostics.Add($"Flow region '{region.Id}' start boundary was not found in the document.");
        }
    }

    private void ResolveContinued(
        FlowRegion region,
        IReadOnlyDictionary<int, Dictionary<string, FlowRegionResolution>> byPage)
    {
        var activationIndex = 0;
        var active = false;
        var activeInstance = default(FlowRegionInstanceId);
        var pagesInInstance = 0;
        var sawStart = false;

        foreach (var page in _pages)
        {
            var start = ProbeBoundary(page, region.Start, region, isStart: true);
            var end = ProbeBoundary(page, region.End, region, isStart: false, start?.SequenceIndex);
            var startedHere = false;

            if (!active)
            {
                if (start == null)
                {
                    if (end != null)
                    {
                        _diagnostics.Add(
                            $"Flow region '{region.Id}' end boundary appeared on page {page.PageIndex + 1} " +
                            "without an active continuation.");
                    }
                    continue;
                }

                sawStart = true;
                active = true;
                startedHere = true;
                pagesInInstance = 0;
                activeInstance = new FlowRegionInstanceId(region.Id, activationIndex++);
            }

            pagesInInstance++;
            var role = end != null
                ? startedHere ? FlowRegionPageRole.StartAndEnd : FlowRegionPageRole.End
                : startedHere ? FlowRegionPageRole.Start : FlowRegionPageRole.Continue;
            var resolution = CreateResolution(
                page,
                region,
                activeInstance,
                role,
                start ?? BoundaryResolution.PageStart(page.StructuredText.RelativePageBox),
                end ?? BoundaryResolution.PageEnd(page.StructuredText.RelativePageBox));
            if (resolution != null)
            {
                byPage[page.PageIndex][region.Id] = resolution;
            }

            if (end != null)
            {
                active = false;
                pagesInInstance = 0;
                continue;
            }

            if (region.MaxPages is { } maxPages && pagesInInstance >= maxPages)
            {
                _diagnostics.Add(
                    $"Flow region '{region.Id}' activation '{activeInstance}' did not resolve an end boundary " +
                    $"within MaxPages={maxPages}; the activation was capped on page {page.PageIndex + 1}.");
                active = false;
                pagesInInstance = 0;
            }
        }

        if (!sawStart)
        {
            _diagnostics.Add($"Flow region '{region.Id}' start boundary was not found in the document.");
        }
        else if (active)
        {
            _diagnostics.Add(
                $"Flow region '{region.Id}' activation '{activeInstance}' reached document end without an end boundary.");
        }
    }

    private FlowRegionResolution? CreateResolution(
        PageRemediationState page,
        FlowRegion region,
        FlowRegionInstanceId instanceId,
        FlowRegionPageRole role,
        BoundaryResolution start,
        BoundaryResolution end)
    {
        if (end.SequenceIndex <= start.SequenceIndex)
        {
            _diagnostics.Add(
                $"Flow region '{region.Id}' end boundary is not after its start boundary on page {page.PageIndex + 1}.");
            return null;
        }

        var top = start.Bounds.LLy;
        var bottom = end.Bounds.URy;
        if (region.MaxExtent is { } maxExtent)
        {
            bottom = Math.Max(bottom, top - maxExtent);
        }

        var pageBox = page.StructuredText.RelativePageBox;
        var bounds = new PdfRect<double>(
            pageBox.LLx,
            Math.Max(pageBox.LLy, bottom),
            pageBox.URx,
            Math.Min(pageBox.URy, top));
        if (bounds.LLy >= bounds.URy)
        {
            _diagnostics.Add(
                $"Flow region '{region.Id}' resolved to an empty vertical extent on page {page.PageIndex + 1}.");
            return null;
        }

        return new FlowRegionResolution(
            region.Id,
            bounds,
            start.SequenceIndex,
            end.SequenceIndex,
            Math.Min(start.Confidence, end.Confidence),
            page.PageIndex,
            instanceId,
            role,
            region.ReadingOrderMode);
    }

    private BoundaryResolution? ProbeBoundary(
        PageRemediationState page,
        FlowBoundary boundary,
        FlowRegion region,
        bool isStart,
        int? minSequenceIndex = null)
    {
        var probeDiagnostics = new List<string>();
        var pageClaims = _claims.Where(x => x.PageIndexes.Contains(page.PageIndex)).ToArray();
        var context = new RemediationEvaluationContext(
            pageClaims,
            pageBox: page.StructuredText.RelativePageBox,
            pageIndex: page.PageIndex,
            pageCount: _pages.Count,
            configuration: _configuration,
            anchors: _anchors,
            tolerancedZones: _zones,
            flowRegions: _regions,
            structuredText: page.StructuredText,
            diagnostics: probeDiagnostics);

        BoundaryResolution? result = boundary switch
        {
            AnchorFlowBoundary anchor => FromAnchor(context.ResolveAnchor(anchor.AnchorId)),
            ZoneFlowBoundary zone => FromZone(context.ResolveTolerancedZone(zone.ZoneId), isStart, page),
            PredicateFlowBoundary predicate => FromPredicate(
                predicate.Predicate, context, page, region.ReadingOrderMode, minSequenceIndex),
            PageBoundaryFlowBoundary => isStart
                ? BoundaryResolution.PageStart(page.StructuredText.RelativePageBox)
                : BoundaryResolution.PageEnd(page.StructuredText.RelativePageBox),
            _ => null
        };

        foreach (var diagnostic in probeDiagnostics.Where(IsMaterialProbeDiagnostic))
        {
            _diagnostics.Add(diagnostic);
        }
        return result;
    }

    private static bool IsMaterialProbeDiagnostic(string diagnostic) =>
        !diagnostic.Contains("matched no candidates on page", StringComparison.Ordinal) &&
        !diagnostic.Contains("is not active on page", StringComparison.Ordinal);

    private static BoundaryResolution? FromAnchor(AnchorResolution? anchor)
    {
        if (anchor == null)
        {
            return null;
        }

        return new BoundaryResolution(
            anchor.Bounds,
            anchor.Candidates?.LastOrDefault()?.SequenceIndex ?? 0,
            anchor.Confidence);
    }

    private static BoundaryResolution? FromZone(
        TolerancedZoneResolution? zone,
        bool isStart,
        PageRemediationState page)
    {
        if (zone == null)
        {
            return null;
        }

        var candidates = page.TextCandidates[Granularity.Line]
            .Where(x => zone.TolerancedBounds.Intersects(x.RelativeBoundingBox))
            .OrderBy(x => x.SequenceIndex)
            .ToList();
        var sequence = candidates.Count == 0
            ? isStart ? -1 : int.MaxValue
            : isStart ? candidates.Max(x => x.SequenceIndex) : candidates.Min(x => x.SequenceIndex);
        return new BoundaryResolution(zone.TolerancedBounds, sequence, 1.0);
    }

    private static BoundaryResolution? FromPredicate(
        RemediationPredicate predicate,
        RemediationEvaluationContext context,
        PageRemediationState page,
        FlowReadingOrderMode mode,
        int? minSequenceIndex)
    {
        var matches = page.TextCandidates[Granularity.Line]
            .Where(x => minSequenceIndex == null || x.SequenceIndex > minSequenceIndex)
            .Select(x => (Candidate: x, Result: predicate.Evaluate(context, x)))
            .Where(x => x.Result.IsMatch);
        var selected = Order(matches, mode).FirstOrDefault();
        return selected.Candidate == null
            ? null
            : new BoundaryResolution(
                selected.Candidate.RelativeBoundingBox,
                selected.Candidate.SequenceIndex,
                selected.Result.Confidence);
    }

    private static IOrderedEnumerable<(TextRemediationCandidate Candidate, PredicateResult Result)> Order(
        IEnumerable<(TextRemediationCandidate Candidate, PredicateResult Result)> candidates,
        FlowReadingOrderMode mode) =>
        mode == FlowReadingOrderMode.GeometryTopToBottom
            ? candidates.OrderByDescending(x => x.Candidate.RelativeBoundingBox.URy)
                .ThenBy(x => x.Candidate.RelativeBoundingBox.LLx)
                .ThenBy(x => x.Candidate.ContentOrderIndex)
            : candidates.OrderBy(x => x.Candidate.ContentOrderIndex)
                .ThenBy(x => x.Candidate.CandidateId, StringComparer.Ordinal);

    private void CheckOverlaps(
        IReadOnlyDictionary<int, Dictionary<string, FlowRegionResolution>> byPage)
    {
        foreach (var (pageIndex, resolutions) in byPage)
        {
            var values = resolutions.Values.ToArray();
            for (var i = 0; i < values.Length; i++)
            {
                for (var j = i + 1; j < values.Length; j++)
                {
                    if (values[i].Bounds.Intersects(values[j].Bounds))
                    {
                        _diagnostics.Add(
                            $"Flow regions '{values[i].RegionId}' and '{values[j].RegionId}' overlap on page {pageIndex + 1}.");
                    }
                }
            }
        }
    }

    private sealed record BoundaryResolution(PdfRect<double> Bounds, int SequenceIndex, double Confidence)
    {
        public static BoundaryResolution PageStart(PdfRect<double> pageBox) =>
            new(
                new PdfRect<double>(pageBox.LLx, pageBox.URy, pageBox.URx, pageBox.URy),
                -1,
                1.0);

        public static BoundaryResolution PageEnd(PdfRect<double> pageBox) =>
            new(
                new PdfRect<double>(pageBox.LLx, pageBox.LLy, pageBox.URx, pageBox.LLy),
                int.MaxValue,
                1.0);
    }
}

internal sealed class DocumentFlowIndex
{
    private readonly IReadOnlyDictionary<int, Dictionary<string, FlowRegionResolution>> _byPage;
    private readonly Dictionary<ClaimId, ClaimFlowMembership> _claimMembership = new();

    public DocumentFlowIndex(
        IReadOnlyDictionary<int, Dictionary<string, FlowRegionResolution>> byPage,
        IReadOnlyList<RemediationCandidate> candidates,
        IReadOnlyList<RemediationClaim> claims)
    {
        _byPage = byPage;
        Candidates = candidates;
        foreach (var claim in claims)
        {
            _claimMembership[claim.ClaimId] = ResolveMembership(claim);
        }
    }

    public IReadOnlyList<RemediationCandidate> Candidates { get; }

    public IReadOnlyDictionary<string, FlowRegionResolution> ForPage(int pageIndex) =>
        _byPage.TryGetValue(pageIndex, out var resolutions)
            ? resolutions
            : new Dictionary<string, FlowRegionResolution>();

    public FlowRegionResolution? Find(string regionId, int pageIndex) =>
        _byPage.TryGetValue(pageIndex, out var resolutions) &&
        resolutions.TryGetValue(regionId, out var resolution)
            ? resolution
            : null;

    public FlowRegionInstanceId? FindSharedInstance(RemediationClaim left, RemediationClaim right)
    {
        var rightInstances = GetMembership(right).Touching.ToHashSet();
        foreach (var instance in GetMembership(left).Touching)
        {
            if (rightInstances.Contains(instance))
            {
                return instance;
            }
        }
        return null;
    }

    public FlowRegionInstanceId? FindContainingInstance(RemediationClaim claim)
    {
        var containing = GetMembership(claim).Containing;
        return containing.Count == 1 ? containing[0] : null;
    }

    private ClaimFlowMembership GetMembership(RemediationClaim claim)
    {
        if (_claimMembership.TryGetValue(claim.ClaimId, out var membership))
        {
            return membership;
        }

        membership = ResolveMembership(claim);
        _claimMembership[claim.ClaimId] = membership;
        return membership;
    }

    private ClaimFlowMembership ResolveMembership(RemediationClaim claim)
    {
        var instances = _byPage.Values
            .SelectMany(x => x.Values)
            .Select(x => x.InstanceId)
            .Distinct()
            .OrderBy(x => x.RegionId, StringComparer.Ordinal)
            .ThenBy(x => x.ActivationIndex)
            .ToArray();
        var touching = new List<FlowRegionInstanceId>();
        var containing = new List<FlowRegionInstanceId>();
        foreach (var instance in instances)
        {
            var touches = false;
            var containsAll = claim.PageIndexes.Count > 0;
            foreach (var pageIndex in claim.PageIndexes)
            {
                var resolution = Find(instance.RegionId, pageIndex);
                var candidates = claim.Candidates.Where(x => x.PageIndex == pageIndex).ToArray();
                var sameInstance = resolution?.InstanceId == instance;
                touches |= sameInstance && candidates.Any(resolution!.Contains);
                containsAll &= sameInstance &&
                    candidates.Length > 0 &&
                    candidates.All(resolution!.Contains);
            }
            if (touches) touching.Add(instance);
            if (containsAll) containing.Add(instance);
        }

        return new ClaimFlowMembership(touching, containing);
    }

    private sealed record ClaimFlowMembership(
        IReadOnlyList<FlowRegionInstanceId> Touching,
        IReadOnlyList<FlowRegionInstanceId> Containing);
}
