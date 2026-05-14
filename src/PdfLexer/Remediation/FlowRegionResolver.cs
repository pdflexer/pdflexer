using PdfLexer.Content;

namespace PdfLexer.Remediation;

internal sealed class FlowRegionResolver
{
    private readonly RemediationEvaluationContext _context;
    private readonly List<string> _diagnostics;

    public FlowRegionResolver(RemediationEvaluationContext context, List<string> diagnostics)
    {
        _context = context;
        _diagnostics = diagnostics;
    }

    public FlowRegionResolution? Resolve(string regionId)
    {
        if (!_context.FlowRegions.TryGetValue(regionId, out var region))
        {
            _diagnostics.Add($"Flow region '{regionId}' is not declared.");
            return null;
        }

        var start = ResolveBoundary(region.Start, isStart: true);
        if (start == null)
        {
            _diagnostics.Add($"Flow region '{regionId}' start boundary could not be resolved.");
            return null;
        }

        var end = ResolveBoundary(region.End, isStart: false, start.SequenceIndex);
        if (end == null)
        {
            _diagnostics.Add($"Flow region '{regionId}' end boundary could not be resolved.");
            return null;
        }

        var top = start.Bounds.LLy;
        var bottom = end.Bounds.URy;
        if (region.MaxExtent is { } maxExtent)
        {
            bottom = Math.Max(bottom, top - maxExtent);
        }

        var bounds = new PdfRect<double>(
            _context.PageBox.LLx,
            Math.Max(_context.PageBox.LLy, bottom),
            _context.PageBox.URx,
            Math.Min(_context.PageBox.URy, top));
        if (bounds.LLy >= bounds.URy)
        {
            _diagnostics.Add($"Flow region '{regionId}' resolved to an empty vertical extent.");
            return null;
        }

        if (end.SequenceIndex <= start.SequenceIndex)
        {
            _diagnostics.Add($"Flow region '{regionId}' end boundary is not after its start boundary.");
            return null;
        }

        return new FlowRegionResolution(
            region.Id,
            bounds,
            start.SequenceIndex,
            end.SequenceIndex,
            Math.Min(start.Confidence, end.Confidence));
    }

    private BoundaryResolution? ResolveBoundary(FlowBoundary boundary, bool isStart, int? minSequenceIndex = null)
    {
        return boundary switch
        {
            AnchorFlowBoundary anchor => ResolveAnchorBoundary(anchor.AnchorId),
            ZoneFlowBoundary zone => ResolveZoneBoundary(zone.ZoneId, isStart),
            PredicateFlowBoundary predicate => ResolvePredicateBoundary(predicate.Predicate, minSequenceIndex),
            PageBoundaryFlowBoundary => ResolvePageBoundary(isStart),
            _ => null
        };
    }

    private BoundaryResolution? ResolveAnchorBoundary(string anchorId)
    {
        var anchor = _context.ResolveAnchor(anchorId);
        if (anchor == null)
        {
            return null;
        }

        return new BoundaryResolution(
            anchor.Bounds,
            anchor.Candidates?.LastOrDefault()?.SequenceIndex ?? 0,
            anchor.Confidence);
    }

    private BoundaryResolution? ResolveZoneBoundary(string zoneId, bool isStart)
    {
        var zone = _context.ResolveTolerancedZone(zoneId);
        if (zone == null)
        {
            return null;
        }

        var sequenceIndex = isStart ? -1 : int.MaxValue;
        if (_context.StructuredText != null)
        {
            var candidates = _context.StructuredText.GetCandidates(Granularity.Line)
                .Where(x => zone.TolerancedBounds.Intersects(x.RelativeBoundingBox))
                .OrderBy(x => x.SequenceIndex)
                .ToList();
            if (candidates.Count > 0)
            {
                sequenceIndex = isStart ? candidates.Max(x => x.SequenceIndex) : candidates.Min(x => x.SequenceIndex);
            }
        }

        return new BoundaryResolution(zone.TolerancedBounds, sequenceIndex, 1.0);
    }

    private BoundaryResolution? ResolvePredicateBoundary(RemediationPredicate predicate, int? minSequenceIndex)
    {
        if (_context.StructuredText == null)
        {
            return null;
        }

        return _context.StructuredText.GetCandidates(Granularity.Line)
            .Where(x => minSequenceIndex == null || x.SequenceIndex > minSequenceIndex.Value)
            .Select(x => (Candidate: x, Result: predicate.Evaluate(_context, x)))
            .Where(x => x.Result.IsMatch)
            .OrderBy(x => x.Candidate.SequenceIndex)
            .Select(x => new BoundaryResolution(
                x.Candidate.RelativeBoundingBox,
                x.Candidate.SequenceIndex,
                x.Result.Confidence))
            .FirstOrDefault();
    }

    private BoundaryResolution ResolvePageBoundary(bool isStart)
    {
        var bounds = isStart
            ? new PdfRect<double>(_context.PageBox.LLx, _context.PageBox.URy, _context.PageBox.URx, _context.PageBox.URy)
            : new PdfRect<double>(_context.PageBox.LLx, _context.PageBox.LLy, _context.PageBox.URx, _context.PageBox.LLy);
        return new BoundaryResolution(bounds, isStart ? -1 : int.MaxValue, 1.0);
    }

    private sealed record BoundaryResolution(PdfRect<double> Bounds, int SequenceIndex, double Confidence);
}
