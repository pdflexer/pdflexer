using PdfLexer.Content;
using System.Text.RegularExpressions;

namespace PdfLexer.Remediation;

internal sealed class AnchorResolver
{
    private readonly RemediationEvaluationContext _context;
    private readonly Dictionary<string, AnchorResolution> _resolutions = new();
    private readonly List<string> _diagnostics;
    private readonly HashSet<string> _resolving = new();

    public AnchorResolver(RemediationEvaluationContext context, List<string> diagnostics)
    {
        _context = context;
        _diagnostics = diagnostics;
    }

    public AnchorResolution? Resolve(string anchorId)
    {
        if (_resolutions.TryGetValue(anchorId, out var resolution))
        {
            return resolution;
        }

        if (!_context.Anchors.TryGetValue(anchorId, out var anchor))
        {
            _diagnostics.Add($"Anchor '{anchorId}' is not defined.");
            return null;
        }

        if (!_resolving.Add(anchorId))
        {
            _diagnostics.Add($"Circular dependency detected resolving anchor '{anchorId}'.");
            return null;
        }

        try
        {
            var result = ResolveInternal(anchor);
            if (result != null)
            {
                _resolutions[anchorId] = result;
            }
            return result;
        }
        finally
        {
            _resolving.Remove(anchorId);
        }
    }

    private AnchorResolution? ResolveInternal(RemediationAnchor anchor)
    {
        if (_context.PageIndex >= 0 &&
            _context.PageCount > 0 &&
            !anchor.Pages.Includes(_context.PageIndex, _context.PageCount))
        {
            return null;
        }

        return anchor switch
        {
            PredicateAnchor predicate => ResolvePredicateAnchor(predicate),
            TextLabelAnchor text => ResolveTextLabel(text),
            PriorClaimAnchor claim => ResolvePriorClaim(claim),
            DeclaredZoneAnchor zone => ResolveDeclaredZone(zone),
            TableHeaderAnchor header => ResolveTableHeader(header),
            RepeatedElementAnchor repeated => ResolveRepeatedElement(repeated),
            GeometryAnchor geometry => ResolveGeometry(geometry),
            _ => throw new ArgumentOutOfRangeException(nameof(anchor))
        };
    }

    private AnchorResolution? ResolvePredicateAnchor(PredicateAnchor anchor)
    {
        if (_context.StructuredText == null) return null;

        var granularities = anchor.Granularities.Count == 0
            ? new[] { Granularity.Line }
            : anchor.Granularities.Distinct().ToArray();
        var matches = granularities
            .SelectMany(granularity => _context.StructuredText.GetCandidates(granularity))
            .Select(candidate => new AnchorCandidateMatch(candidate, anchor.Predicate.Evaluate(_context, candidate)))
            .Where(x => x.Result.IsMatch)
            .ToList();

        return ResolveSelectedCandidate(anchor, matches, anchor.Selection, anchor.Predicate.DebugString);
    }

    private AnchorResolution? ResolveTextLabel(TextLabelAnchor anchor)
    {
        if (_context.StructuredText == null) return null;

        var matches = new List<AnchorCandidateMatch>();
        foreach (var candidate in _context.StructuredText.GetCandidates(Granularity.Line))
        {
            if (string.Equals(candidate.Text, anchor.Text, anchor.Comparison))
            {
                matches.Add(new AnchorCandidateMatch(candidate, PredicateResult.Match()));
            }
        }
        
        if (matches.Count == 0)
        {
            foreach (var candidate in _context.StructuredText.GetCandidates(Granularity.Word))
            {
                if (string.Equals(candidate.Text, anchor.Text, anchor.Comparison))
                {
                    matches.Add(new AnchorCandidateMatch(candidate, PredicateResult.Match()));
                }
            }
        }

        return ResolveSelectedCandidate(anchor, matches, GetLegacySelection(anchor), $"text '{anchor.Text}'");
    }

    private AnchorResolution? ResolvePriorClaim(PriorClaimAnchor anchor)
    {
        if (!_context.ClaimsByRuleId.TryGetValue(anchor.RuleId, out var claims) || claims.Count == 0)
        {
            return null;
        }

        if (claims.Count > 1)
        {
            _diagnostics.Add($"Anchor '{anchor.Id}' is ambiguous. Found {claims.Count} claims for rule '{anchor.RuleId}'.");
            return null;
        }

        var claim = claims[0];
        if (claim.BoundingBox == null)
        {
            return null;
        }

        return new AnchorResolution(anchor.Id, claim.BoundingBox, claim.Confidence, claim.PageIndex, claim.Candidates);
    }

    private AnchorResolution? ResolveDeclaredZone(DeclaredZoneAnchor anchor)
    {
        var coord = new NamedZoneLayoutCoord(anchor.Zone);
        var rect = coord.Resolve(_context, null!);
        return new AnchorResolution(anchor.Id, rect, 1.0, _context.PageIndex);
    }

    private AnchorResolution? ResolveTableHeader(TableHeaderAnchor anchor)
    {
        if (_context.StructuredText == null) return null;

        var headers = anchor.Headers.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
        if (headers.Length == 0)
        {
            _diagnostics.Add($"Anchor '{anchor.Id}' has no table header text.");
            return null;
        }

        var matches = new List<AnchorCandidateMatch>();
        foreach (var candidate in _context.StructuredText.GetCandidates(Granularity.Line))
        {
            if (headers.All(header => candidate.Text.Contains(header, StringComparison.OrdinalIgnoreCase)))
            {
                matches.Add(new AnchorCandidateMatch(candidate, PredicateResult.Match()));
            }
        }

        return ResolveSelectedCandidate(anchor, matches, GetLegacySelection(anchor), $"table headers [{string.Join(", ", headers)}]");
    }

    private AnchorResolution? ResolveRepeatedElement(RepeatedElementAnchor anchor)
    {
        if (_context.StructuredText == null) return null;

        var matches = new List<AnchorCandidateMatch>();
        foreach (var candidate in _context.StructuredText.GetCandidates(Granularity.Line))
        {
            if (Regex.IsMatch(candidate.Text, anchor.Pattern))
            {
                matches.Add(new AnchorCandidateMatch(candidate, PredicateResult.Match()));
            }
        }

        if (matches.Count == 0)
        {
            foreach (var candidate in _context.StructuredText.GetCandidates(Granularity.Word))
            {
                if (Regex.IsMatch(candidate.Text, anchor.Pattern))
                {
                    matches.Add(new AnchorCandidateMatch(candidate, PredicateResult.Match()));
                }
            }
        }

        return ResolveSelectedCandidate(anchor, matches, GetLegacySelection(anchor), $"pattern '{anchor.Pattern}'");
    }

    private AnchorResolution? ResolveGeometry(GeometryAnchor anchor)
    {
        return new AnchorResolution(anchor.Id, anchor.Bounds, 1.0, _context.PageIndex);
    }

    private AnchorResolution? ResolveSelectedCandidate(
        RemediationAnchor anchor,
        IReadOnlyList<AnchorCandidateMatch> candidates,
        AnchorSelection selection,
        string description)
    {
        var matches = ApplyDisambiguators(anchor, candidates).ToList();
        if (matches.Count == 0)
        {
            if (selection.Mode != AnchorSelectionMode.OptionalSingle)
            {
                _diagnostics.Add($"Anchor '{anchor.Id}' matched no candidates for {description}.");
            }
            return null;
        }

        AnchorCandidateMatch? selected = selection.Mode switch
        {
            AnchorSelectionMode.RequiredSingle => SelectSingle(anchor, matches, description, required: true),
            AnchorSelectionMode.OptionalSingle => SelectSingle(anchor, matches, description, required: false),
            AnchorSelectionMode.FirstInReadingOrder => matches.OrderBy(x => x.Candidate.SequenceIndex).First(),
            AnchorSelectionMode.LastInReadingOrder => matches.OrderByDescending(x => x.Candidate.SequenceIndex).First(),
            AnchorSelectionMode.NthInReadingOrder => SelectNth(anchor, matches, selection, description),
            AnchorSelectionMode.NearestToAnchor => SelectNearest(anchor, matches, selection, description),
            _ => null
        };

        if (selected == null)
        {
            return null;
        }

        return new AnchorResolution(
            anchor.Id,
            selected.Candidate.RelativeBoundingBox,
            selected.Result.Confidence,
            _context.PageIndex,
            new[] { selected.Candidate });
    }

    private static AnchorSelection GetLegacySelection(RemediationAnchor anchor) =>
        anchor.Occurrence is { } occurrence
            ? AnchorSelection.NthInReadingOrder(occurrence - 1)
            : AnchorSelection.RequiredSingle;

    private AnchorCandidateMatch? SelectSingle(
        RemediationAnchor anchor,
        IReadOnlyList<AnchorCandidateMatch> matches,
        string description,
        bool required)
    {
        if (matches.Count == 1)
        {
            return matches[0];
        }

        _diagnostics.Add(required
            ? $"Anchor '{anchor.Id}' is ambiguous. Found {matches.Count} matches for {description}."
            : $"Optional anchor '{anchor.Id}' is ambiguous. Found {matches.Count} matches for {description}.");
        return null;
    }

    private AnchorCandidateMatch? SelectNth(
        RemediationAnchor anchor,
        IReadOnlyList<AnchorCandidateMatch> matches,
        AnchorSelection selection,
        string description)
    {
        if (selection.Index is not { } index || index < 0)
        {
            _diagnostics.Add($"Anchor '{anchor.Id}' has invalid nth selection index '{selection.Index}'.");
            return null;
        }

        if (index >= matches.Count)
        {
            _diagnostics.Add($"Anchor '{anchor.Id}' nth selection {index} is outside the {matches.Count} match(es) for {description}.");
            return null;
        }

        return matches.OrderBy(x => x.Candidate.SequenceIndex).ElementAt(index);
    }

    private AnchorCandidateMatch? SelectNearest(
        RemediationAnchor anchor,
        IReadOnlyList<AnchorCandidateMatch> matches,
        AnchorSelection selection,
        string description)
    {
        if (string.IsNullOrWhiteSpace(selection.AnchorId))
        {
            _diagnostics.Add($"Anchor '{anchor.Id}' nearest selection requires a reference anchor id.");
            return null;
        }

        var reference = Resolve(selection.AnchorId);
        if (reference == null)
        {
            return null;
        }

        var nearest = matches
            .Select(x => (Match: x, Distance: Distance(reference.Bounds, x.Candidate.RelativeBoundingBox)))
            .Where(x => DirectionMatches(reference.Bounds, x.Match.Candidate.RelativeBoundingBox, selection.Direction))
            .Where(x => selection.MaxDistance == null || x.Distance <= selection.MaxDistance.Value)
            .OrderBy(x => x.Distance)
            .ToList();
        if (nearest.Count == 0)
        {
            _diagnostics.Add($"Anchor '{anchor.Id}' matched no candidates for nearest selection over {description}.");
            return null;
        }

        var bestDistance = nearest[0].Distance;
        var tied = nearest.Where(x => Math.Abs(x.Distance - bestDistance) <= 0.000001).ToList();
        if (tied.Count > 1)
        {
            _diagnostics.Add($"Anchor '{anchor.Id}' has an ambiguous nearest match with {tied.Count} equally near candidates.");
            return null;
        }

        var selected = nearest[0].Match;
        return selected with
        {
            Result = PredicateResult.Match(Math.Min(selected.Result.Confidence, ConfidenceFromDistance(bestDistance)))
        };
    }

    private IEnumerable<AnchorCandidateMatch> ApplyDisambiguators(
        RemediationAnchor anchor,
        IReadOnlyList<AnchorCandidateMatch> candidates)
    {
        foreach (var match in candidates.OrderBy(x => x.Candidate.SequenceIndex))
        {
            if (anchor.Style != null && !anchor.Style.Evaluate(_context, match.Candidate).IsMatch)
            {
                continue;
            }

            if (anchor.NeighborText != null && !HasNeighbor(match.Candidate, anchor.NeighborText, anchor.NeighborTolerance))
            {
                continue;
            }

            yield return match;
        }
    }

    private bool HasNeighbor(RemediationCandidate anchorCandidate, string neighborText, double tolerance)
    {
        if (_context.StructuredText == null)
        {
            return false;
        }

        return _context.StructuredText.GetCandidates(Granularity.Word)
            .Concat(_context.StructuredText.GetCandidates(Granularity.Line))
            .Any(candidate =>
                !ReferenceEquals(candidate, anchorCandidate) &&
                string.Equals(candidate.Text, neighborText, StringComparison.Ordinal) &&
                AreNear(anchorCandidate.RelativeBoundingBox, candidate.RelativeBoundingBox, tolerance));
    }

    private static bool AreNear(PdfRect<double> a, PdfRect<double> b, double tolerance)
    {
        var horizontalGap = Math.Max(0, Math.Max(a.LLx, b.LLx) - Math.Min(a.URx, b.URx));
        var verticalGap = Math.Max(0, Math.Max(a.LLy, b.LLy) - Math.Min(a.URy, b.URy));
        return horizontalGap <= tolerance || verticalGap <= tolerance;
    }

    private static bool DirectionMatches(PdfRect<double> reference, PdfRect<double> candidate, AnchorDirection direction) =>
        direction switch
        {
            AnchorDirection.Any => true,
            AnchorDirection.Left => candidate.URx <= reference.LLx,
            AnchorDirection.Right => candidate.LLx >= reference.URx,
            AnchorDirection.Above => candidate.LLy >= reference.URy,
            AnchorDirection.Below => candidate.URy <= reference.LLy,
            _ => true
        };

    private static double Distance(PdfRect<double> a, PdfRect<double> b)
    {
        var ax = (a.LLx + a.URx) / 2;
        var ay = (a.LLy + a.URy) / 2;
        var bx = (b.LLx + b.URx) / 2;
        var by = (b.LLy + b.URy) / 2;
        return Math.Sqrt(Math.Pow(ax - bx, 2) + Math.Pow(ay - by, 2));
    }

    private static double ConfidenceFromDistance(double distance) => 1.0 / (1.0 + distance / 72.0);

    private sealed record AnchorCandidateMatch(RemediationCandidate Candidate, PredicateResult Result);
}
