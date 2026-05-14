using System.Text.RegularExpressions;
using PdfLexer.Content;

namespace PdfLexer.Remediation;

/// <summary>
/// Predicate evaluated against structured-text/content remediation candidates.
/// </summary>
public abstract record RemediationPredicate
{
    /// <summary>Predicate that matches every candidate.</summary>
    public static RemediationPredicate Always { get; } = new ConstantRemediationPredicate(true, "true");

    /// <summary>Predicate that matches no candidates.</summary>
    public static RemediationPredicate Never { get; } = new ConstantRemediationPredicate(false, "false");

    /// <summary>Debug representation used in reports.</summary>
    public abstract string DebugString { get; }

    /// <summary>Evaluates this predicate against a candidate in context.</summary>
    public abstract PredicateResult Evaluate(RemediationEvaluationContext context, RemediationCandidate candidate);

    /// <summary>Evaluates this predicate against a candidate with an empty context.</summary>
    public PredicateResult Evaluate(RemediationCandidate candidate) => Evaluate(RemediationEvaluationContext.Empty, candidate);

    /// <summary>Combines this predicate with another using logical AND.</summary>
    public RemediationPredicate And(RemediationPredicate other) => new CompositeRemediationPredicate(CompositePredicateKind.And, this, other);

    /// <summary>Combines this predicate with another using logical OR.</summary>
    public RemediationPredicate Or(RemediationPredicate other) => new CompositeRemediationPredicate(CompositePredicateKind.Or, this, other);

    /// <summary>Negates this predicate.</summary>
    public RemediationPredicate Not() => new NotRemediationPredicate(this);
}

/// <summary>Candidate predicate with a constant result.</summary>
public sealed record ConstantRemediationPredicate(bool Value, string Name) : RemediationPredicate
{
    public override string DebugString => Name;

    public override PredicateResult Evaluate(RemediationEvaluationContext context, RemediationCandidate candidate) =>
        Value ? PredicateResult.Match() : PredicateResult.NoMatch();
}

/// <summary>Logical operator for composite predicates.</summary>
public enum CompositePredicateKind
{
    /// <summary>Both predicates must match.</summary>
    And,
    /// <summary>Either predicate may match.</summary>
    Or
}

/// <summary>Candidate predicate that combines two predicates.</summary>
public sealed record CompositeRemediationPredicate(
    CompositePredicateKind Kind,
    RemediationPredicate Left,
    RemediationPredicate Right) : RemediationPredicate
{
    public override string DebugString => $"({Left.DebugString} {Kind} {Right.DebugString})";

    public override PredicateResult Evaluate(RemediationEvaluationContext context, RemediationCandidate candidate)
    {
        var left = Left.Evaluate(context, candidate);
        if (Kind == CompositePredicateKind.And)
        {
            if (!left.IsMatch)
            {
                return left;
            }

            var right = Right.Evaluate(context, candidate);
            return right.IsMatch
                ? PredicateResult.Match(Math.Min(left.Confidence, right.Confidence))
                : right;
        }

        if (left.IsMatch)
        {
            return left;
        }

        var orRight = Right.Evaluate(context, candidate);
        return orRight.IsMatch ? orRight : PredicateResult.NoMatch(left.Reason ?? orRight.Reason);
    }
}

/// <summary>Candidate predicate that negates another predicate.</summary>
public sealed record NotRemediationPredicate(RemediationPredicate Inner) : RemediationPredicate
{
    public override string DebugString => $"Not({Inner.DebugString})";

    public override PredicateResult Evaluate(RemediationEvaluationContext context, RemediationCandidate candidate)
    {
        var result = Inner.Evaluate(context, candidate);
        return result.IsMatch ? PredicateResult.NoMatch() : PredicateResult.Match(result.Confidence);
    }
}

/// <summary>Text predicate operation.</summary>
public enum TextPredicateKind
{
    /// <summary>Regular-expression match.</summary>
    Matches,
    /// <summary>Substring match.</summary>
    Contains,
    /// <summary>Prefix match.</summary>
    StartsWith,
    /// <summary>Exact string match.</summary>
    Equals
}

/// <summary>Candidate predicate over candidate text.</summary>
public sealed record TextRemediationPredicate : RemediationPredicate
{
    /// <summary>Creates a text predicate.</summary>
    public TextRemediationPredicate(TextPredicateKind kind, string value, StringComparison comparison = StringComparison.Ordinal)
    {
        Kind = kind;
        Value = value ?? throw new ArgumentNullException(nameof(value));
        Comparison = comparison;
    }

    /// <summary>Text operation.</summary>
    public TextPredicateKind Kind { get; }

    /// <summary>Text or regular-expression value.</summary>
    public string Value { get; }

    /// <summary>String comparison used for non-regex operations.</summary>
    public StringComparison Comparison { get; }

    public override string DebugString => $"Text.{Kind}(\"{Value}\")";

    public override PredicateResult Evaluate(RemediationEvaluationContext context, RemediationCandidate candidate)
    {
        var matched = Kind switch
        {
            TextPredicateKind.Matches => Regex.IsMatch(candidate.Text, Value),
            TextPredicateKind.Contains => candidate.Text.Contains(Value, Comparison),
            TextPredicateKind.StartsWith => candidate.Text.StartsWith(Value, Comparison),
            TextPredicateKind.Equals => string.Equals(candidate.Text, Value, Comparison),
            _ => throw new ArgumentOutOfRangeException()
        };

        return matched ? PredicateResult.Match() : PredicateResult.NoMatch();
    }
}

/// <summary>Numeric comparison operator.</summary>
public enum NumericOperator
{
    /// <summary>Equal within tolerance.</summary>
    Equal,
    /// <summary>Not equal within tolerance.</summary>
    NotEqual,
    /// <summary>Less than.</summary>
    LessThan,
    /// <summary>Less than or equal.</summary>
    LessThanOrEqual,
    /// <summary>Greater than.</summary>
    GreaterThan,
    /// <summary>Greater than or equal.</summary>
    GreaterThanOrEqual
}

/// <summary>Font/style predicate category.</summary>
public enum FontPredicateKind
{
    /// <summary>Font size.</summary>
    Size,
    /// <summary>Font weight.</summary>
    Weight,
    /// <summary>Font family/name.</summary>
    Family,
    /// <summary>Italic style.</summary>
    Italic,
    /// <summary>Grayish fill color.</summary>
    ColorIsGrayish
}

/// <summary>Candidate predicate over font or style data.</summary>
public sealed record FontRemediationPredicate : RemediationPredicate
{
    /// <summary>Creates a numeric font/style predicate.</summary>
    public FontRemediationPredicate(FontPredicateKind kind, NumericOperator op, double value)
    {
        Kind = kind;
        Operator = op;
        NumericValue = value;
    }

    /// <summary>Creates a text font/style predicate.</summary>
    public FontRemediationPredicate(FontPredicateKind kind, string value)
    {
        Kind = kind;
        TextValue = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>Creates a boolean font/style predicate.</summary>
    public FontRemediationPredicate(FontPredicateKind kind, bool value)
    {
        Kind = kind;
        BooleanValue = value;
    }

    /// <summary>Font/style predicate category.</summary>
    public FontPredicateKind Kind { get; }

    /// <summary>Numeric comparison operator, when applicable.</summary>
    public NumericOperator? Operator { get; }

    /// <summary>Numeric comparison value, when applicable.</summary>
    public double? NumericValue { get; }

    /// <summary>Text comparison value, when applicable.</summary>
    public string? TextValue { get; }

    /// <summary>Boolean comparison value, when applicable.</summary>
    public bool? BooleanValue { get; }

    public override string DebugString => Kind switch
    {
        FontPredicateKind.Size => $"Font.Size({Operator},{NumericValue})",
        FontPredicateKind.Weight => $"Font.Weight({Operator},{NumericValue})",
        FontPredicateKind.Family => $"Font.Family(\"{TextValue}\")",
        FontPredicateKind.Italic => $"Font.Italic({BooleanValue})",
        FontPredicateKind.ColorIsGrayish => "Color.IsGrayish",
        _ => $"Font.{Kind}"
    };

    public override PredicateResult Evaluate(RemediationEvaluationContext context, RemediationCandidate candidate)
    {
        if (Kind == FontPredicateKind.Size && Operator is { } op && NumericValue is { } expected)
        {
            return Compare(candidate.FontSize, op, expected) ? PredicateResult.Match() : PredicateResult.NoMatch();
        }

        if (Kind == FontPredicateKind.Weight && Operator is { } weightOp && NumericValue is { } weight)
        {
            return candidate.FontWeight is { } actual
                ? Compare(actual, weightOp, weight) ? PredicateResult.Match() : PredicateResult.NoMatch()
                : PredicateResult.NoMatch("Structured text does not expose font weight for this candidate.");
        }

        if (Kind == FontPredicateKind.Family && TextValue is { } family)
        {
            return candidate.FontName != null &&
                candidate.FontName.Contains(family, StringComparison.OrdinalIgnoreCase)
                ? PredicateResult.Match()
                : PredicateResult.NoMatch();
        }

        if (Kind == FontPredicateKind.Italic && BooleanValue is { } italic)
        {
            return candidate.Italic is { } actual
                ? actual == italic ? PredicateResult.Match() : PredicateResult.NoMatch()
                : PredicateResult.NoMatch("Structured text does not expose italic style for this candidate.");
        }

        if (Kind == FontPredicateKind.ColorIsGrayish)
        {
            return candidate.IsGrayish is { } actual
                ? actual ? PredicateResult.Match() : PredicateResult.NoMatch()
                : PredicateResult.NoMatch("Structured text does not expose fill color style for this candidate.");
        }

        return PredicateResult.NoMatch($"Structured text does not expose {Kind} data for this candidate.");
    }

    private static bool Compare(double actual, NumericOperator op, double expected)
    {
        const double tolerance = 0.001d;
        return op switch
        {
            NumericOperator.Equal => Math.Abs(actual - expected) <= tolerance,
            NumericOperator.NotEqual => Math.Abs(actual - expected) > tolerance,
            NumericOperator.LessThan => actual < expected,
            NumericOperator.LessThanOrEqual => actual <= expected || Math.Abs(actual - expected) <= tolerance,
            NumericOperator.GreaterThan => actual > expected,
            NumericOperator.GreaterThanOrEqual => actual >= expected || Math.Abs(actual - expected) <= tolerance,
            _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
        };
    }
}

/// <summary>Candidate predicate over candidate geometry.</summary>
public sealed record GeometryRemediationPredicate(LayoutCoord Coord, GeometryMatchMode Mode = GeometryMatchMode.Intersects) : RemediationPredicate
{
    public override string DebugString => $"Geo.{Mode}({Coord.DebugString})";

    public override PredicateResult Evaluate(RemediationEvaluationContext context, RemediationCandidate candidate)
    {
        PdfRect<double> rect;
        try
        {
            rect = Coord.Resolve(context, candidate);
        }
        catch (Exception ex)
        {
            return PredicateResult.NoMatch(ex.Message);
        }

        var result = Mode switch
        {
            GeometryMatchMode.Contains => rect.CheckEnclosure(candidate.RelativeBoundingBox) == EncloseType.Full,
            GeometryMatchMode.Intersects => rect.Intersects(candidate.RelativeBoundingBox),
            _ => throw new ArgumentOutOfRangeException()
        };

        return result ? PredicateResult.Match() : PredicateResult.NoMatch();
    }
}

/// <summary>Candidate predicate that matches candidates inside a declared toleranced zone.</summary>
public sealed record TolerancedZoneRemediationPredicate(string ZoneId) : RemediationPredicate
{
    public override string DebugString => $"Flow.InZone({ZoneId})";

    public override PredicateResult Evaluate(RemediationEvaluationContext context, RemediationCandidate candidate)
    {
        var zone = context.ResolveTolerancedZone(ZoneId);
        return zone?.Contains(candidate.RelativeBoundingBox) ?? PredicateResult.NoMatch($"Could not resolve zone '{ZoneId}'.");
    }
}

/// <summary>Candidate predicate that matches candidates inside a declared flow region.</summary>
public sealed record FlowRegionRemediationPredicate(string RegionId) : RemediationPredicate
{
    public override string DebugString => $"Flow.InFlowRegion({RegionId})";

    public override PredicateResult Evaluate(RemediationEvaluationContext context, RemediationCandidate candidate)
    {
        var region = context.ResolveFlowRegion(RegionId);
        return region?.Contains(candidate) == true
            ? PredicateResult.Match(region.Confidence)
            : PredicateResult.NoMatch($"Candidate is outside flow region '{RegionId}'.");
    }
}

/// <summary>Flow-order predicate category.</summary>
public enum FlowOrderPredicateKind
{
    /// <summary>Candidate is the first candidate in a flow region.</summary>
    FirstIn,
    /// <summary>Candidate is the last candidate in a flow region.</summary>
    LastIn,
    /// <summary>Candidate is the zero-based nth candidate in a flow region.</summary>
    NthIn,
    /// <summary>Candidate is the first candidate after an anchor.</summary>
    FirstAfter
}

/// <summary>Candidate predicate that selects by reading-order position in a flow or after an anchor.</summary>
public sealed record FlowOrderRemediationPredicate(
    FlowOrderPredicateKind Kind,
    string Id,
    int? Index = null,
    RemediationPredicate? Where = null) : RemediationPredicate
{
    public override string DebugString => Kind switch
    {
        FlowOrderPredicateKind.NthIn => $"Flow.NthIn({Id}, {Index})",
        FlowOrderPredicateKind.FirstAfter => $"Flow.FirstAfter({Id}, {Where?.DebugString ?? "true"})",
        _ => $"Flow.{Kind}({Id})"
    };

    public override PredicateResult Evaluate(RemediationEvaluationContext context, RemediationCandidate candidate)
    {
        if (context.StructuredText == null)
        {
            return PredicateResult.NoMatch("Structured text is not available.");
        }

        var candidates = context.StructuredText.GetCandidates(candidate.Granularity)
            .OrderBy(x => x.SequenceIndex)
            .ToList();
        double boundaryConfidence;
        if (Kind == FlowOrderPredicateKind.FirstAfter)
        {
            var anchor = context.ResolveAnchor(Id);
            if (anchor == null)
            {
                return PredicateResult.NoMatch($"Could not resolve anchor '{Id}'.");
            }

            var anchorSequence = anchor.Candidates?.Max(x => x.SequenceIndex) ?? int.MinValue;
            boundaryConfidence = anchor.Confidence;
            candidates = candidates.Where(x => x.SequenceIndex > anchorSequence).ToList();
        }
        else
        {
            var region = context.ResolveFlowRegion(Id);
            if (region == null)
            {
                return PredicateResult.NoMatch($"Could not resolve flow region '{Id}'.");
            }

            boundaryConfidence = region.Confidence;
            candidates = candidates.Where(region.Contains).ToList();
        }

        var filtered = new List<(RemediationCandidate Candidate, PredicateResult Result)>();
        foreach (var item in candidates)
        {
            var result = Where?.Evaluate(context, item) ?? PredicateResult.Match();
            if (result.IsMatch)
            {
                filtered.Add((item, result));
            }
        }

        if (filtered.Count == 0)
        {
            return PredicateResult.NoMatch($"No candidates matched {DebugString}.");
        }

        (RemediationCandidate Candidate, PredicateResult Result)? selected = Kind switch
        {
            FlowOrderPredicateKind.FirstIn or FlowOrderPredicateKind.FirstAfter => filtered[0],
            FlowOrderPredicateKind.LastIn => filtered[^1],
            FlowOrderPredicateKind.NthIn when Index is >= 0 && Index < filtered.Count => filtered[Index.Value],
            FlowOrderPredicateKind.NthIn => null,
            _ => null
        };

        if (selected == null)
        {
            return PredicateResult.NoMatch($"Flow selection index '{Index}' is outside {filtered.Count} candidate(s).");
        }

        return SameCandidate(candidate, selected.Value.Candidate)
            ? PredicateResult.Match(Math.Min(boundaryConfidence, selected.Value.Result.Confidence))
            : PredicateResult.NoMatch();
    }

    private static bool SameCandidate(RemediationCandidate left, RemediationCandidate right) =>
        left.Granularity == right.Granularity &&
        left.SequenceIndex == right.SequenceIndex &&
        string.Equals(left.Text, right.Text, StringComparison.Ordinal);
}

/// <summary>Geometry match mode.</summary>
public enum GeometryMatchMode
{
    /// <summary>Coordinate bounds must fully contain the candidate.</summary>
    Contains,
    /// <summary>Coordinate bounds must intersect the candidate.</summary>
    Intersects
}

/// <summary>Relational candidate predicate category.</summary>
public enum RelationalPredicateKind
{
    /// <summary>Candidate is after claims from another rule.</summary>
    After,
    /// <summary>Candidate is before claims from another rule.</summary>
    Before,
    /// <summary>Candidate is inside a claim from another rule.</summary>
    InsideClaimOf,
    /// <summary>Candidate is the nth child candidate of a claim from another rule.</summary>
    NthChildOfClaim
}

/// <summary>Candidate predicate that relates candidates to existing claims.</summary>
public sealed record RelationalRemediationPredicate(
    RelationalPredicateKind Kind,
    string RuleId,
    int? ChildIndex = null) : RemediationPredicate
{
    public override string DebugString => ChildIndex is { } child
        ? $"Rel.{Kind}({RuleId},{child})"
        : $"Rel.{Kind}({RuleId})";

    public override PredicateResult Evaluate(RemediationEvaluationContext context, RemediationCandidate candidate)
    {
        if (!context.ClaimsByRuleId.TryGetValue(RuleId, out var claims) || claims.Count == 0)
        {
            return PredicateResult.NoMatch($"No claims found for rule '{RuleId}'.");
        }

        var matched = Kind switch
        {
            RelationalPredicateKind.After => claims.Any(x => x.LastSequenceIndex < candidate.SequenceIndex),
            RelationalPredicateKind.Before => claims.Any(x => x.FirstSequenceIndex > candidate.SequenceIndex),
            RelationalPredicateKind.InsideClaimOf => claims.Any(x =>
                x.BoundingBox is { } box && box.CheckEnclosure(candidate.BoundingBox) == EncloseType.Full),
            RelationalPredicateKind.NthChildOfClaim => ChildIndex is { } index &&
                claims.Any(x => index >= 0 && index < x.Candidates.Count && ReferenceEquals(x.Candidates[index], candidate)),
            _ => throw new ArgumentOutOfRangeException()
        };

        return matched ? PredicateResult.Match() : PredicateResult.NoMatch();
    }
}

/// <summary>Anchor-relative candidate predicate category.</summary>
public enum AnchorRelativePredicateKind
{
    /// <summary>Candidate is right of an anchor.</summary>
    RightOf,
    /// <summary>Candidate is below an anchor.</summary>
    Below,
    /// <summary>Candidate is between two anchors.</summary>
    Between,
    /// <summary>Candidate is on the same row as an anchor.</summary>
    SameRowAs,
    /// <summary>Candidate is in the same column as an anchor.</summary>
    SameColumnAs,
    /// <summary>Candidate is nearest to an anchor.</summary>
    NearestTo,
    /// <summary>Candidate appears in flow after an anchor.</summary>
    InFlowAfter
}

/// <summary>Candidate predicate resolved relative to named anchors.</summary>
public sealed record AnchorRelativeRemediationPredicate(
    AnchorRelativePredicateKind Kind,
    string AnchorId,
    string? AnchorId2 = null,
    double? Tolerance = null,
    AnchorDirection Direction = AnchorDirection.Any,
    double? MaxDistance = null) : RemediationPredicate
{
    public override string DebugString => Kind switch
    {
        AnchorRelativePredicateKind.Between => $"Anchor.{Kind}({AnchorId}, {AnchorId2})",
        AnchorRelativePredicateKind.NearestTo => $"Anchor.{Kind}({AnchorId}, {Direction}, max={MaxDistance})",
        _ => Tolerance.HasValue || MaxDistance.HasValue
            ? $"Anchor.{Kind}({AnchorId}, tolerance={Tolerance}, max={MaxDistance})"
            : $"Anchor.{Kind}({AnchorId})"
    };

    public override PredicateResult Evaluate(RemediationEvaluationContext context, RemediationCandidate candidate)
    {
        var resolution = context.ResolveAnchor(AnchorId);
        if (resolution == null)
        {
            return PredicateResult.NoMatch($"Could not resolve anchor '{AnchorId}'.");
        }

        AnchorResolution? resolution2 = null;
        if (AnchorId2 != null)
        {
            resolution2 = context.ResolveAnchor(AnchorId2);
            if (resolution2 == null)
            {
                return PredicateResult.NoMatch($"Could not resolve anchor '{AnchorId2}'.");
            }
        }

        var bounds = resolution.Bounds;
        var candBounds = candidate.RelativeBoundingBox;
        var tol = Tolerance ?? 1.0; // Default small tolerance

        var matched = Kind switch
        {
            AnchorRelativePredicateKind.RightOf => candBounds.LLx >= bounds.URx - tol && WithinMaxDistance(HorizontalGap(bounds, candBounds)),
            AnchorRelativePredicateKind.Below => candBounds.URy <= bounds.LLy + tol && WithinMaxDistance(VerticalGap(bounds, candBounds)), // Y goes up
            AnchorRelativePredicateKind.Between => BuildBetweenBounds(bounds, resolution2!.Bounds, tol)
                .CheckEnclosure(candBounds) == EncloseType.Full,
            AnchorRelativePredicateKind.SameRowAs => candBounds.LLy <= bounds.URy + tol && candBounds.URy >= bounds.LLy - tol,
            AnchorRelativePredicateKind.SameColumnAs => candBounds.LLx <= bounds.URx + tol && candBounds.URx >= bounds.LLx - tol,
            AnchorRelativePredicateKind.NearestTo => !Overlaps(bounds, candBounds) &&
                DirectionMatches(bounds, candBounds, Direction) &&
                WithinMaxDistance(Distance(bounds, candBounds)),
            AnchorRelativePredicateKind.InFlowAfter => candidate.SequenceIndex > (resolution.Candidates?.Max(c => c.SequenceIndex) ?? int.MaxValue),
            _ => throw new ArgumentOutOfRangeException()
        };

        return matched
            ? PredicateResult.Match(Kind == AnchorRelativePredicateKind.NearestTo
                ? ConfidenceFromDistance(bounds, candBounds)
                : resolution.Confidence)
            : PredicateResult.NoMatch();
    }

    private bool WithinMaxDistance(double distance) => MaxDistance == null || distance <= MaxDistance.Value;

    private static PdfRect<double> BuildBetweenBounds(PdfRect<double> a, PdfRect<double> b, double padding)
    {
        var rowOverlap = a.LLy <= b.URy && a.URy >= b.LLy;
        if (rowOverlap)
        {
            return new PdfRect<double>(
                Math.Min(a.URx, b.URx) - padding,
                Math.Min(a.LLy, b.LLy) - padding,
                Math.Max(a.LLx, b.LLx) + padding,
                Math.Max(a.URy, b.URy) + padding);
        }

        return new PdfRect<double>(
            Math.Min(a.LLx, b.LLx) - padding,
            Math.Min(a.URy, b.URy) - padding,
            Math.Max(a.URx, b.URx) + padding,
            Math.Max(a.LLy, b.LLy) + padding);
    }

    private static bool Overlaps(PdfRect<double> a, PdfRect<double> b) => a.Intersects(b);

    private static double ConfidenceFromDistance(PdfRect<double> anchor, PdfRect<double> candidate)
    {
        var distance = Distance(anchor, candidate);
        return 1.0 / (1.0 + distance / 72.0);
    }

    private static double Distance(PdfRect<double> anchor, PdfRect<double> candidate)
    {
        var ax = (anchor.LLx + anchor.URx) / 2;
        var ay = (anchor.LLy + anchor.URy) / 2;
        var bx = (candidate.LLx + candidate.URx) / 2;
        var by = (candidate.LLy + candidate.URy) / 2;
        return Math.Sqrt(Math.Pow(ax - bx, 2) + Math.Pow(ay - by, 2));
    }

    private static double HorizontalGap(PdfRect<double> anchor, PdfRect<double> candidate) =>
        Math.Max(0, candidate.LLx - anchor.URx);

    private static double VerticalGap(PdfRect<double> anchor, PdfRect<double> candidate) =>
        Math.Max(0, anchor.LLy - candidate.URy);

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
}

/// <summary>
/// Factory helpers for candidate predicates.
/// </summary>
public static class Predicates
{
    /// <summary>Text predicate factory helpers.</summary>
    public static class Text
    {
        /// <summary>Matches candidate text with a regular expression.</summary>
        public static RemediationPredicate Matches(string regex) => new TextRemediationPredicate(TextPredicateKind.Matches, regex);

        /// <summary>Matches candidate text containing a value.</summary>
        public static RemediationPredicate Contains(string value, StringComparison comparison = StringComparison.Ordinal) =>
            new TextRemediationPredicate(TextPredicateKind.Contains, value, comparison);

        /// <summary>Matches candidate text starting with a value.</summary>
        public static RemediationPredicate StartsWith(string value, StringComparison comparison = StringComparison.Ordinal) =>
            new TextRemediationPredicate(TextPredicateKind.StartsWith, value, comparison);

        /// <summary>Matches candidate text equal to a value.</summary>
        public static RemediationPredicate Equals(string value, StringComparison comparison = StringComparison.Ordinal) =>
            new TextRemediationPredicate(TextPredicateKind.Equals, value, comparison);
    }

    /// <summary>Font/style predicate factory helpers.</summary>
    public static class Font
    {
        /// <summary>Matches candidate font size.</summary>
        public static RemediationPredicate Size(NumericOperator op, double value) =>
            new FontRemediationPredicate(FontPredicateKind.Size, op, value);

        /// <summary>Matches candidate font weight.</summary>
        public static RemediationPredicate Weight(NumericOperator op, double value) =>
            new FontRemediationPredicate(FontPredicateKind.Weight, op, value);

        /// <summary>Matches candidate font family/name.</summary>
        public static RemediationPredicate Family(string value) => new FontRemediationPredicate(FontPredicateKind.Family, value);

        /// <summary>Matches candidate italic style.</summary>
        public static RemediationPredicate Italic(bool value = true) => new FontRemediationPredicate(FontPredicateKind.Italic, value);
    }

    /// <summary>Color predicate factory helpers.</summary>
    public static class Color
    {
        /// <summary>Matches candidates whose fill color is grayish.</summary>
        public static RemediationPredicate IsGrayish() => new FontRemediationPredicate(FontPredicateKind.ColorIsGrayish, true);
    }

    /// <summary>Geometry predicate factory helpers.</summary>
    public static class Geo
    {
        /// <summary>Matches candidates fully contained by a coordinate.</summary>
        public static RemediationPredicate Contains(LayoutCoord coord) => new GeometryRemediationPredicate(coord, GeometryMatchMode.Contains);

        /// <summary>Matches candidates fully contained by a coordinate.</summary>
        public static RemediationPredicate In(LayoutCoord coord) => Contains(coord);

        /// <summary>Matches candidates intersecting a coordinate.</summary>
        public static RemediationPredicate Intersects(LayoutCoord coord) => new GeometryRemediationPredicate(coord, GeometryMatchMode.Intersects);
    }

    /// <summary>Relational predicate factory helpers.</summary>
    public static class Relational
    {
        /// <summary>Matches candidates after claims from another rule.</summary>
        public static RemediationPredicate After(string ruleId) => new RelationalRemediationPredicate(RelationalPredicateKind.After, ruleId);

        /// <summary>Matches candidates before claims from another rule.</summary>
        public static RemediationPredicate Before(string ruleId) => new RelationalRemediationPredicate(RelationalPredicateKind.Before, ruleId);

        /// <summary>Matches candidates inside claims from another rule.</summary>
        public static RemediationPredicate InsideClaimOf(string ruleId) =>
            new RelationalRemediationPredicate(RelationalPredicateKind.InsideClaimOf, ruleId);

        /// <summary>Matches the nth child candidate of claims from another rule.</summary>
        public static RemediationPredicate NthChildOfClaim(string ruleId, int index) =>
            new RelationalRemediationPredicate(RelationalPredicateKind.NthChildOfClaim, ruleId, index);
    }

    /// <summary>Anchor-relative predicate factory helpers.</summary>
    public static class Anchor
    {
        /// <summary>Matches candidates right of an anchor.</summary>
        public static RemediationPredicate RightOf(string anchorId, double? tolerance = null, double? maxDistance = null) =>
            new AnchorRelativeRemediationPredicate(AnchorRelativePredicateKind.RightOf, anchorId, Tolerance: tolerance, MaxDistance: maxDistance);
        /// <summary>Matches candidates below an anchor.</summary>
        public static RemediationPredicate Below(string anchorId, double? tolerance = null, double? maxDistance = null) =>
            new AnchorRelativeRemediationPredicate(AnchorRelativePredicateKind.Below, anchorId, Tolerance: tolerance, MaxDistance: maxDistance);
        /// <summary>Matches candidates between two anchors.</summary>
        public static RemediationPredicate Between(string anchorId1, string anchorId2) => new AnchorRelativeRemediationPredicate(AnchorRelativePredicateKind.Between, anchorId1, AnchorId2: anchorId2);
        /// <summary>Matches candidates on the same row as an anchor.</summary>
        public static RemediationPredicate SameRowAs(string anchorId, double? tolerance = null) => new AnchorRelativeRemediationPredicate(AnchorRelativePredicateKind.SameRowAs, anchorId, Tolerance: tolerance);
        /// <summary>Matches candidates in the same column as an anchor.</summary>
        public static RemediationPredicate SameColumnAs(string anchorId, double? tolerance = null) => new AnchorRelativeRemediationPredicate(AnchorRelativePredicateKind.SameColumnAs, anchorId, Tolerance: tolerance);
        /// <summary>Matches candidates nearest to an anchor.</summary>
        public static RemediationPredicate NearestTo(string anchorId, AnchorDirection direction = AnchorDirection.Any, double? maxDistance = null) =>
            new AnchorRelativeRemediationPredicate(AnchorRelativePredicateKind.NearestTo, anchorId, Direction: direction, MaxDistance: maxDistance);
        /// <summary>Matches candidates in flow after an anchor.</summary>
        public static RemediationPredicate InFlowAfter(string anchorId) => new AnchorRelativeRemediationPredicate(AnchorRelativePredicateKind.InFlowAfter, anchorId);
    }

    /// <summary>Flow and zone predicate factory helpers.</summary>
    public static class Flow
    {
        /// <summary>Matches candidates inside a declared toleranced zone.</summary>
        public static RemediationPredicate InZone(string zoneId) => new TolerancedZoneRemediationPredicate(zoneId);

        /// <summary>Matches candidates inside a declared flow region.</summary>
        public static RemediationPredicate InFlowRegion(string regionId) => new FlowRegionRemediationPredicate(regionId);

        /// <summary>Matches the first candidate in a declared flow region.</summary>
        public static RemediationPredicate FirstIn(string regionId, RemediationPredicate? where = null) =>
            new FlowOrderRemediationPredicate(FlowOrderPredicateKind.FirstIn, regionId, Where: where);

        /// <summary>Matches the last candidate in a declared flow region.</summary>
        public static RemediationPredicate LastIn(string regionId, RemediationPredicate? where = null) =>
            new FlowOrderRemediationPredicate(FlowOrderPredicateKind.LastIn, regionId, Where: where);

        /// <summary>Matches the zero-based nth candidate in a declared flow region.</summary>
        public static RemediationPredicate NthIn(string regionId, int index, RemediationPredicate? where = null) =>
            new FlowOrderRemediationPredicate(FlowOrderPredicateKind.NthIn, regionId, Index: index, Where: where);

        /// <summary>Matches the first candidate after an anchor, optionally filtered by another predicate.</summary>
        public static RemediationPredicate FirstAfter(string anchorId, RemediationPredicate? where = null) =>
            new FlowOrderRemediationPredicate(FlowOrderPredicateKind.FirstAfter, anchorId, Where: where);
    }
}
