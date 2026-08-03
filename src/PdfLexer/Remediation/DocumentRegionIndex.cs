using PdfLexer.Content;

namespace PdfLexer.Remediation;

internal sealed record ResolvedRegionSegment(
    string RegionId,
    int PageIndex,
    PdfRect<double> BaseBounds,
    PdfRect<double> Bounds,
    double Confidence,
    int StartSequenceIndex = -1,
    int EndSequenceIndex = int.MaxValue,
    FlowRegionInstanceId? Activation = null,
    FlowRegionPageRole? PageRole = null)
{
    public bool Contains(RemediationCandidate candidate, GeometryMatchMode mode = GeometryMatchMode.Contains) =>
        candidate.PageIndex == PageIndex &&
        candidate.SequenceIndex > StartSequenceIndex &&
        candidate.SequenceIndex < EndSequenceIndex &&
        (mode == GeometryMatchMode.Contains
            ? Bounds.CheckEnclosure(candidate.RelativeBoundingBox) == EncloseType.Full
            : Bounds.Intersects(candidate.RelativeBoundingBox));
}

internal sealed class DocumentRegionIndex
{
    private readonly IReadOnlyDictionary<int, IReadOnlyDictionary<string, IReadOnlyList<ResolvedRegionSegment>>> _byPage;

    public DocumentRegionIndex(IEnumerable<ResolvedRegionSegment> segments)
    {
        Segments = segments
            .OrderBy(x => x.PageIndex)
            .ThenBy(x => x.RegionId, StringComparer.Ordinal)
            .ThenBy(x => x.Activation?.ActivationIndex ?? -1)
            .ThenBy(x => x.StartSequenceIndex)
            .ToArray();
        _byPage = Segments
            .GroupBy(x => x.PageIndex)
            .ToDictionary(
                x => x.Key,
                x => (IReadOnlyDictionary<string, IReadOnlyList<ResolvedRegionSegment>>)x
                    .GroupBy(y => y.RegionId, StringComparer.Ordinal)
                    .ToDictionary(
                        y => y.Key,
                        y => (IReadOnlyList<ResolvedRegionSegment>)y.ToArray(),
                        StringComparer.Ordinal));
    }

    public static DocumentRegionIndex Empty { get; } = new(Array.Empty<ResolvedRegionSegment>());
    public IReadOnlyList<ResolvedRegionSegment> Segments { get; }

    public IReadOnlyList<ResolvedRegionSegment> ForPage(string regionId, int pageIndex) =>
        _byPage.TryGetValue(pageIndex, out var regions) && regions.TryGetValue(regionId, out var values)
            ? values
            : Array.Empty<ResolvedRegionSegment>();

    public ResolvedRegionSegment? FindContaining(
        string regionId,
        RemediationCandidate candidate,
        GeometryMatchMode mode = GeometryMatchMode.Contains) =>
        ForPage(regionId, candidate.PageIndex).FirstOrDefault(x => x.Contains(candidate, mode));
}

internal sealed class DocumentRegionResolver
{
    private readonly IReadOnlyList<PageRemediationState> _pages;
    private readonly IReadOnlyList<RemediationClaim> _claims;
    private readonly RemediationSessionConfiguration _configuration;
    private readonly IReadOnlyDictionary<string, RemediationAnchor> _anchors;
    private readonly IReadOnlyDictionary<string, RegionDeclaration> _regions;
    private readonly TextNormalizationOptions _normalization;
    private readonly List<string> _diagnostics;
    private readonly Action<RemediationRuntimeDiagnostic>? _diagnosticSink;
    private readonly Dictionary<string, IReadOnlyList<ResolvedRegionSegment>> _resolved = new(StringComparer.Ordinal);
    private readonly HashSet<string> _resolving = new(StringComparer.Ordinal);

    public DocumentRegionResolver(
        IReadOnlyList<PageRemediationState> pages,
        IReadOnlyList<RemediationClaim> claims,
        RemediationSessionConfiguration configuration,
        IReadOnlyDictionary<string, RemediationAnchor> anchors,
        IReadOnlyList<RegionDeclaration> regions,
        TextNormalizationOptions normalization,
        List<string> diagnostics,
        Action<RemediationRuntimeDiagnostic>? diagnosticSink = null)
    {
        _pages = pages;
        _claims = claims;
        _configuration = configuration;
        _anchors = anchors;
        _regions = regions.ToDictionary(x => x.Id, StringComparer.Ordinal);
        _normalization = normalization;
        _diagnostics = diagnostics;
        _diagnosticSink = diagnosticSink;
    }

    public DocumentRegionIndex Resolve()
    {
        foreach (var region in _regions.Values.OrderBy(x => x.Id, StringComparer.Ordinal))
            Resolve(region.Id);
        return new DocumentRegionIndex(_resolved.Values.SelectMany(x => x));
    }

    private IReadOnlyList<ResolvedRegionSegment> Resolve(string id)
    {
        if (_resolved.TryGetValue(id, out var cached)) return cached;
        if (!_regions.TryGetValue(id, out var declaration)) return Array.Empty<ResolvedRegionSegment>();
        if (!_resolving.Add(id))
        {
            Fail(id, $"Region '{id}' has a resolution dependency cycle.");
            return Array.Empty<ResolvedRegionSegment>();
        }

        IReadOnlyList<ResolvedRegionSegment> result;
        try
        {
            result = ResolveExpression(declaration, declaration.Expression);
        }
        catch (Exception ex)
        {
            Fail(id, $"Region '{id}' could not be resolved: {ex.Message}");
            result = Array.Empty<ResolvedRegionSegment>();
        }
        _resolving.Remove(id);
        _resolved[id] = result;
        return result;
    }

    private IReadOnlyList<ResolvedRegionSegment> ResolveExpression(RegionDeclaration declaration, Region expression) =>
        expression switch
        {
            Region.Fixed fixedRegion => ResolveFixed(declaration, fixedRegion),
            Region.Anchored anchored => ResolveAnchored(declaration, anchored),
            Region.Flow flow => ResolveFlow(declaration, flow),
            Region.Tolerance tolerance => ApplyTolerance(
                declaration,
                ResolveExpression(declaration, tolerance.Inner),
                tolerance),
            _ => Array.Empty<ResolvedRegionSegment>()
        };

    private IReadOnlyList<ResolvedRegionSegment> ResolveFixed(
        RegionDeclaration declaration,
        Region.Fixed expression)
    {
        var result = new List<ResolvedRegionSegment>();
        foreach (var page in SelectedPages(declaration))
        {
            var context = CreateContext(page);
            var bounds = expression.Bounds.Resolve(context, DummyCandidate(page.PageIndex));
            if (!IsUsable(bounds))
            {
                Fail(declaration.Id, $"Region '{declaration.Id}' resolved to empty bounds on page {page.PageIndex + 1}.", page.PageIndex);
                continue;
            }
            result.Add(new ResolvedRegionSegment(declaration.Id, page.PageIndex, bounds, bounds, 1));
        }
        return result;
    }

    private IReadOnlyList<ResolvedRegionSegment> ResolveAnchored(
        RegionDeclaration declaration,
        Region.Anchored expression)
    {
        var result = new List<ResolvedRegionSegment>();
        foreach (var page in SelectedPages(declaration))
        {
            var anchor = CreateContext(page).ResolveAnchor(expression.AnchorId);
            if (anchor == null) continue;
            var source = anchor.Bounds;
            var amount = expression.Extent;
            var bounds = expression.Placement switch
            {
                RegionPlacement.Above => new PdfRect<double>(source.LLx, source.URy, source.URx, source.URy + amount),
                RegionPlacement.Below => new PdfRect<double>(source.LLx, source.LLy - amount, source.URx, source.LLy),
                RegionPlacement.LeftOf => new PdfRect<double>(source.LLx - amount, source.LLy, source.LLx, source.URy),
                RegionPlacement.RightOf => new PdfRect<double>(source.URx, source.LLy, source.URx + amount, source.URy),
                RegionPlacement.Around => source.Expand(amount),
                _ => source
            };
            bounds = Clamp(bounds, page.StructuredText.RelativePageBox);
            if (!IsUsable(bounds))
            {
                Fail(declaration.Id, $"Region '{declaration.Id}' resolved to empty anchor-relative bounds on page {page.PageIndex + 1}.", page.PageIndex);
                continue;
            }
            result.Add(new ResolvedRegionSegment(declaration.Id, page.PageIndex, bounds, bounds, anchor.Confidence));
        }
        return result;
    }

    private IReadOnlyList<ResolvedRegionSegment> ApplyTolerance(
        RegionDeclaration declaration,
        IReadOnlyList<ResolvedRegionSegment> segments,
        Region.Tolerance tolerance) =>
        segments.Select(segment =>
        {
            var pageBox = _pages[segment.PageIndex].StructuredText.RelativePageBox;
            return segment with
            {
                Bounds = Clamp(segment.Bounds.Expand(tolerance.Amount), pageBox),
                Confidence = tolerance.ConfidenceBehavior == ZoneConfidenceBehavior.FullWithinTolerance
                    ? 1
                    : segment.Confidence
            };
        }).ToArray();

    private IReadOnlyList<ResolvedRegionSegment> ResolveFlow(
        RegionDeclaration declaration,
        Region.Flow flow) =>
        flow.ContinuationPolicy == FlowContinuationPolicy.CurrentPageOnly
            ? ResolvePageLocalFlow(declaration, flow)
            : ResolveContinuedFlow(declaration, flow);

    private IReadOnlyList<ResolvedRegionSegment> ResolvePageLocalFlow(
        RegionDeclaration declaration,
        Region.Flow flow)
    {
        var result = new List<ResolvedRegionSegment>();
        var activation = 0;
        foreach (var page in SelectedPages(declaration))
        {
            var starts = ResolveBoundary(page, flow.Start, true, flow.ReadingOrderMode);
            var ends = ResolveBoundary(page, flow.End, false, flow.ReadingOrderMode);
            var usedEnds = new HashSet<int>();
            foreach (var start in starts)
            {
                var endIndex = Enumerable.Range(0, ends.Count)
                    .FirstOrDefault(index => !usedEnds.Contains(index) && ends[index].SequenceIndex > start.SequenceIndex, -1);
                if (endIndex < 0)
                {
                    Fail(declaration.Id, $"Flow region '{declaration.Id}' has no end after a start on page {page.PageIndex + 1}.", page.PageIndex);
                    continue;
                }
                usedEnds.Add(endIndex);
                AddFlowSegment(result, declaration.Id, flow, page, start, ends[endIndex],
                    new FlowRegionInstanceId(declaration.Id, activation++), FlowRegionPageRole.StartAndEnd);
            }
            foreach (var index in Enumerable.Range(0, ends.Count).Where(x => !usedEnds.Contains(x)))
                Fail(declaration.Id, $"Flow region '{declaration.Id}' has an orphan end boundary on page {page.PageIndex + 1}.", page.PageIndex);
        }
        return result;
    }

    private IReadOnlyList<ResolvedRegionSegment> ResolveContinuedFlow(
        RegionDeclaration declaration,
        Region.Flow flow)
    {
        var result = new List<ResolvedRegionSegment>();
        var activationIndex = 0;
        FlowRegionInstanceId? active = null;
        var activePages = 0;
        foreach (var page in SelectedPages(declaration))
        {
            var starts = ResolveBoundary(page, flow.Start, true, flow.ReadingOrderMode).ToList();
            var ends = ResolveBoundary(page, flow.End, false, flow.ReadingOrderMode).ToList();
            var cursor = int.MinValue;
            var enteredActive = active != null;
            while (true)
            {
                if (active == null)
                {
                    var start = starts.FirstOrDefault(x => x.SequenceIndex > cursor);
                    if (start == null) break;
                    active = new FlowRegionInstanceId(declaration.Id, activationIndex++);
                    activePages = 0;
                    cursor = start.SequenceIndex;
                }

                var pageStart = starts.FirstOrDefault(x => x.SequenceIndex >= cursor);
                var startBoundary = pageStart ?? BoundaryEvent.PageStart(page.StructuredText.RelativePageBox);
                var endBoundary = ends.FirstOrDefault(x => x.SequenceIndex > startBoundary.SequenceIndex);
                activePages++;
                var closes = endBoundary != null;
                var role = closes
                    ? activePages == 1 ? FlowRegionPageRole.StartAndEnd : FlowRegionPageRole.End
                    : activePages == 1 && !enteredActive ? FlowRegionPageRole.Start : FlowRegionPageRole.Continue;
                AddFlowSegment(result, declaration.Id, flow, page, startBoundary,
                    endBoundary ?? BoundaryEvent.PageEnd(page.StructuredText.RelativePageBox), active.Value, role);

                if (closes)
                {
                    cursor = endBoundary!.SequenceIndex;
                    starts.RemoveAll(x => x.SequenceIndex <= cursor);
                    ends.RemoveAll(x => x.SequenceIndex <= cursor);
                    active = null;
                    activePages = 0;
                    enteredActive = false;
                    continue;
                }

                if (flow.MaxPages is { } maxPages && activePages >= maxPages)
                {
                    Fail(declaration.Id, $"Flow region '{declaration.Id}' activation '{active}' exceeded MaxPages={maxPages} on page {page.PageIndex + 1}.", page.PageIndex);
                    active = null;
                    activePages = 0;
                }
                break;
            }

            foreach (var orphan in ends.Where(x => x.SequenceIndex > cursor && active == null))
                Fail(declaration.Id, $"Flow region '{declaration.Id}' has an orphan end boundary on page {page.PageIndex + 1}.", page.PageIndex);
        }
        if (active != null)
            Fail(declaration.Id, $"Flow region '{declaration.Id}' activation '{active}' reached document end without an end boundary.");
        return result;
    }

    private void AddFlowSegment(
        List<ResolvedRegionSegment> result,
        string id,
        Region.Flow flow,
        PageRemediationState page,
        BoundaryEvent start,
        BoundaryEvent end,
        FlowRegionInstanceId activation,
        FlowRegionPageRole role)
    {
        if (end.SequenceIndex <= start.SequenceIndex)
        {
            Fail(id, $"Flow region '{id}' end is not after its start on page {page.PageIndex + 1}.", page.PageIndex);
            return;
        }
        var box = page.StructuredText.RelativePageBox;
        var bottom = end.Bounds.URy;
        if (flow.MaxExtent is { } maxExtent) bottom = Math.Max(bottom, start.Bounds.LLy - maxExtent);
        var bounds = new PdfRect<double>(box.LLx, Math.Max(box.LLy, bottom), box.URx, Math.Min(box.URy, start.Bounds.LLy));
        if (!IsUsable(bounds))
        {
            Fail(id, $"Flow region '{id}' resolved to empty bounds on page {page.PageIndex + 1}.", page.PageIndex);
            return;
        }
        result.Add(new ResolvedRegionSegment(
            id, page.PageIndex, bounds, bounds, Math.Min(start.Confidence, end.Confidence),
            start.SequenceIndex, end.SequenceIndex, activation, role));
    }

    private IReadOnlyList<BoundaryEvent> ResolveBoundary(
        PageRemediationState page,
        RegionBoundary boundary,
        bool isStart,
        FlowReadingOrderMode order)
    {
        switch (boundary)
        {
            case RegionBoundary.Page:
                return new[] { isStart ? BoundaryEvent.PageStart(page.StructuredText.RelativePageBox) : BoundaryEvent.PageEnd(page.StructuredText.RelativePageBox) };
            case RegionBoundary.Anchor anchor:
            {
                var resolution = CreateContext(page).ResolveAnchor(anchor.AnchorId);
                if (resolution == null) return Array.Empty<BoundaryEvent>();
                return new[] { new BoundaryEvent(resolution.Bounds, resolution.Candidates?.LastOrDefault()?.SequenceIndex ?? 0, resolution.Confidence) };
            }
            case RegionBoundary.Region region:
            {
                return Resolve(region.RegionId)
                    .Where(x => x.PageIndex == page.PageIndex)
                    .Select(x =>
                    {
                        var candidates = AllCandidates(page).Where(y => x.Bounds.Intersects(y.RelativeBoundingBox)).ToArray();
                        var sequence = candidates.Length == 0
                            ? isStart ? -1 : int.MaxValue
                            : isStart ? candidates.Max(y => y.SequenceIndex) : candidates.Min(y => y.SequenceIndex);
                        return new BoundaryEvent(x.Bounds, sequence, x.Confidence);
                    }).ToArray();
            }
            case RegionBoundary.Matching matching:
            {
                var context = CreateContext(page);
                var matches = SelectCandidates(page, matching.Candidates)
                    .Select(x => (Candidate: x, Match: matching.Predicate.Evaluate(context, x)))
                    .Where(x => x.Match.IsMatch);
                matches = order == FlowReadingOrderMode.GeometryTopToBottom
                    ? matches.OrderByDescending(x => x.Candidate.RelativeBoundingBox.URy)
                        .ThenBy(x => x.Candidate.RelativeBoundingBox.LLx)
                    : matches.OrderBy(x => x.Candidate.SequenceIndex);
                return matches.Select(x => new BoundaryEvent(
                    x.Candidate.RelativeBoundingBox, x.Candidate.SequenceIndex, x.Match.Confidence)).ToArray();
            }
            default:
                return Array.Empty<BoundaryEvent>();
        }
    }

    private RemediationEvaluationContext CreateContext(PageRemediationState page) =>
        new(
            _claims.Where(x => x.PageIndexes.Contains(page.PageIndex)).ToArray(),
            pageBox: page.StructuredText.RelativePageBox,
            pageIndex: page.PageIndex,
            pageCount: _pages.Count,
            configuration: _configuration,
            anchors: _anchors,
            structuredText: page.StructuredText,
            diagnostics: _diagnostics,
            textNormalization: _normalization)
        {
            DocumentCandidates = AllCandidates(page),
            DocumentRegions = new DocumentRegionIndex(_resolved.Values.SelectMany(x => x)),
            RuntimeDiagnosticSink = _diagnosticSink
        };

    private IReadOnlyList<PageRemediationState> SelectedPages(RegionDeclaration declaration) =>
        _pages.Where(x => declaration.Pages.Includes(x.PageIndex, _pages.Count)).ToArray();

    private static IReadOnlyList<RemediationCandidate> AllCandidates(PageRemediationState page) =>
        page.TextCandidates.Values.SelectMany(x => x).Cast<RemediationCandidate>()
            .Concat(page.ContentCandidates)
            .Concat(page.AnnotationCandidates)
            .DistinctBy(x => x.CandidateId, StringComparer.Ordinal)
            .ToArray();

    private static IReadOnlyList<RemediationCandidate> SelectCandidates(PageRemediationState page, CandidateSelector selector) =>
        selector switch
        {
            CandidateSelector.TextSelector text => page.TextCandidates.GetValueOrDefault(text.Granularity) ?? Array.Empty<TextRemediationCandidate>(),
            CandidateSelector.ContentSelector content => page.ContentCandidates.Where(x => content.Kinds.Contains(x.Kind)).ToArray(),
            _ => Array.Empty<RemediationCandidate>()
        };

    private static TextRemediationCandidate DummyCandidate(int pageIndex) =>
        new(
            Granularity.Line,
            string.Empty,
            new PdfRect<double>(0, 0, 0, 0),
            new PdfRect<double>(0, 0, 0, 0),
            Array.Empty<StructuredCharacter>(),
            Array.Empty<StructuredSourceRef>(),
            0,
            0)
        { PageIndex = pageIndex };

    private void Fail(string regionId, string message, int? pageIndex = null)
    {
        _diagnostics.Add(message);
        _diagnosticSink?.Invoke(new RemediationRuntimeDiagnostic(
            DiagnosticCode.ProgramRegionResolutionFailed,
            RemediationDiagnosticDisposition.Error,
            $"Program:Region:{regionId}" + (pageIndex is { } page ? $":Page{page + 1}" : string.Empty),
            message,
            Evidence: new Dictionary<string, object?> { ["regionId"] = regionId, ["pageIndex"] = pageIndex }));
    }

    private static bool IsUsable(PdfRect<double> bounds) => bounds.URx > bounds.LLx && bounds.URy > bounds.LLy;

    private static PdfRect<double> Clamp(PdfRect<double> bounds, PdfRect<double> page) =>
        new(
            Math.Max(page.LLx, bounds.LLx),
            Math.Max(page.LLy, bounds.LLy),
            Math.Min(page.URx, bounds.URx),
            Math.Min(page.URy, bounds.URy));

    private sealed record BoundaryEvent(PdfRect<double> Bounds, int SequenceIndex, double Confidence)
    {
        public static BoundaryEvent PageStart(PdfRect<double> box) =>
            new(new PdfRect<double>(box.LLx, box.URy, box.URx, box.URy), -1, 1);
        public static BoundaryEvent PageEnd(PdfRect<double> box) =>
            new(new PdfRect<double>(box.LLx, box.LLy, box.URx, box.LLy), int.MaxValue, 1);
    }
}
