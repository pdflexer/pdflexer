using PdfLexer.Content;

namespace PdfLexer.Remediation;

/// <summary>
/// Page-relative layout coordinate that resolves to a rectangle during rule evaluation.
/// </summary>
public abstract record LayoutCoord
{
    /// <summary>Creates a coordinate from an absolute page-relative rectangle.</summary>
    public static LayoutCoord Absolute(PdfRect<double> rect) => new AbsoluteLayoutCoord(rect);

    /// <summary>Creates a coordinate from offsets relative to the page box.</summary>
    public static LayoutCoord MarginRelative(double? top = null, double? right = null, double? bottom = null, double? left = null) =>
        new MarginRelativeLayoutCoord(top, right, bottom, left);

    /// <summary>Creates a coordinate for a built-in named layout zone.</summary>
    public static LayoutCoord Zone(NamedLayoutZone zone) => new NamedZoneLayoutCoord(zone);

    /// <summary>Creates a coordinate from percentage offsets relative to the page box.</summary>
    public static LayoutCoord Percentage(double? top = null, double? right = null, double? bottom = null, double? left = null) =>
        new PercentageLayoutCoord(top, right, bottom, left);

    /// <summary>Creates a coordinate relative to claims produced by another rule.</summary>
    public static LayoutCoord Anchor(string ruleId, LayoutCoordExpansion? expand = null) => new AnchorLayoutCoord(ruleId, expand);

    /// <summary>Creates a coordinate relative to a named anchor declaration.</summary>
    public static LayoutCoord NamedAnchor(string anchorId, LayoutCoordExpansion? expand = null) => new NamedAnchorLayoutCoord(anchorId, expand);

    /// <summary>Creates a coordinate spanning two named anchors.</summary>
    public static LayoutCoord BetweenAnchors(string anchorA, string anchorB, double padding = 0) => new BetweenAnchorsLayoutCoord(anchorA, anchorB, padding);

    /// <summary>Creates a coordinate from a declared toleranced zone.</summary>
    public static LayoutCoord TolerancedZone(string zoneId) => new TolerancedZoneLayoutCoord(zoneId);

    /// <summary>Creates a coordinate from a declared flow region.</summary>
    public static LayoutCoord FlowRegion(string regionId) => new FlowRegionLayoutCoord(regionId);

    /// <summary>Debug representation used in reports.</summary>
    public abstract string DebugString { get; }

    /// <summary>Resolves the coordinate for an evaluation context and candidate.</summary>
    public abstract PdfRect<double> Resolve(RemediationEvaluationContext context, RemediationCandidate candidate);
}

/// <summary>Absolute page-relative rectangle coordinate.</summary>
public sealed record AbsoluteLayoutCoord(PdfRect<double> Rect) : LayoutCoord
{
    public override string DebugString => $"Absolute({Rect})";

    public override PdfRect<double> Resolve(RemediationEvaluationContext context, RemediationCandidate candidate) => Rect;
}

/// <summary>Coordinate resolved from page-box offsets.</summary>
public sealed record MarginRelativeLayoutCoord(
    double? Top = null,
    double? Right = null,
    double? Bottom = null,
    double? Left = null) : LayoutCoord
{
    public override string DebugString => $"MarginRelative(top={Top}, right={Right}, bottom={Bottom}, left={Left})";

    public override PdfRect<double> Resolve(RemediationEvaluationContext context, RemediationCandidate candidate) =>
        LayoutCoordHelpers.ResolveOffsets(context.PageBox, Top, Right, Bottom, Left);
}

/// <summary>Coordinate resolved from built-in named layout zones.</summary>
public sealed record NamedZoneLayoutCoord(NamedLayoutZone NamedZone) : LayoutCoord
{
    public override string DebugString => $"Zone({NamedZone})";

    public override PdfRect<double> Resolve(RemediationEvaluationContext context, RemediationCandidate candidate)
    {
        var page = context.PageBox;
        var margins = context.Configuration.NamedZoneMargins;
        return NamedZone switch
        {
            NamedLayoutZone.Header => new PdfRect<double>(page.LLx, page.URy - margins.Header, page.URx, page.URy),
            NamedLayoutZone.Footer => new PdfRect<double>(page.LLx, page.LLy, page.URx, page.LLy + margins.Footer),
            NamedLayoutZone.LeftMargin => new PdfRect<double>(page.LLx, page.LLy, page.LLx + margins.Left, page.URy),
            NamedLayoutZone.RightMargin => new PdfRect<double>(page.URx - margins.Right, page.LLy, page.URx, page.URy),
            NamedLayoutZone.Body => new PdfRect<double>(
                page.LLx + margins.Left,
                page.LLy + margins.Footer,
                page.URx - margins.Right,
                page.URy - margins.Header),
            _ => throw new ArgumentOutOfRangeException(nameof(NamedZone), NamedZone, null)
        };
    }
}

/// <summary>Coordinate resolved from percentage offsets of the page box.</summary>
public sealed record PercentageLayoutCoord(
    double? Top = null,
    double? Right = null,
    double? Bottom = null,
    double? Left = null) : LayoutCoord
{
    public override string DebugString => $"Percentage(top={Top}, right={Right}, bottom={Bottom}, left={Left})";

    public override PdfRect<double> Resolve(RemediationEvaluationContext context, RemediationCandidate candidate)
    {
        var page = context.PageBox;
        var width = page.URx - page.LLx;
        var height = page.URy - page.LLy;
        return LayoutCoordHelpers.ResolveOffsets(
            page,
            Top == null ? null : height * Top.Value,
            Right == null ? null : width * Right.Value,
            Bottom == null ? null : height * Bottom.Value,
            Left == null ? null : width * Left.Value);
    }
}

/// <summary>Coordinate resolved from claims produced by another rule.</summary>
public sealed record AnchorLayoutCoord(string RuleId, LayoutCoordExpansion? Expand = null) : LayoutCoord
{
    public override string DebugString => Expand == null ? $"Anchor({RuleId})" : $"Anchor({RuleId}, {Expand})";

    public override PdfRect<double> Resolve(RemediationEvaluationContext context, RemediationCandidate candidate)
    {
        if (!context.ClaimsByRuleId.TryGetValue(RuleId, out var claims))
        {
            throw new InvalidOperationException($"Anchor rule '{RuleId}' did not produce a claim.");
        }

        var claim = claims
            .Where(x => x.BoundingBox != null)
            .OrderBy(x => Math.Abs(x.FirstSequenceIndex - candidate.SequenceIndex))
            .FirstOrDefault();
        if (claim?.BoundingBox == null)
        {
            throw new InvalidOperationException($"Anchor rule '{RuleId}' did not produce a bounded claim.");
        }

        return Expand?.Apply(claim.BoundingBox) ?? claim.BoundingBox;
    }
}

/// <summary>Coordinate resolved from a named anchor declaration.</summary>
public sealed record NamedAnchorLayoutCoord(string AnchorId, LayoutCoordExpansion? Expand = null) : LayoutCoord
{
    public override string DebugString => Expand == null ? $"NamedAnchor({AnchorId})" : $"NamedAnchor({AnchorId}, {Expand})";

    public override PdfRect<double> Resolve(RemediationEvaluationContext context, RemediationCandidate candidate)
    {
        var resolution = context.ResolveAnchor(AnchorId);
        if (resolution == null)
        {
            throw new InvalidOperationException($"Could not resolve named anchor '{AnchorId}'.");
        }
        return Expand?.Apply(resolution.Bounds) ?? resolution.Bounds;
    }
}

/// <summary>Coordinate spanning two named anchors.</summary>
public sealed record BetweenAnchorsLayoutCoord(string AnchorA, string AnchorB, double Padding = 0) : LayoutCoord
{
    public override string DebugString => Padding == 0 ? $"BetweenAnchors({AnchorA}, {AnchorB})" : $"BetweenAnchors({AnchorA}, {AnchorB}, {Padding})";

    public override PdfRect<double> Resolve(RemediationEvaluationContext context, RemediationCandidate candidate)
    {
        var resA = context.ResolveAnchor(AnchorA);
        var resB = context.ResolveAnchor(AnchorB);
        if (resA == null) throw new InvalidOperationException($"Could not resolve anchor '{AnchorA}'.");
        if (resB == null) throw new InvalidOperationException($"Could not resolve anchor '{AnchorB}'.");

        var boundsA = resA.Bounds;
        var boundsB = resB.Bounds;
        var rowOverlap = boundsA.LLy <= boundsB.URy && boundsA.URy >= boundsB.LLy;
        if (rowOverlap)
        {
            return new PdfRect<double>(
                Math.Min(boundsA.URx, boundsB.URx) - Padding,
                Math.Min(boundsA.LLy, boundsB.LLy) - Padding,
                Math.Max(boundsA.LLx, boundsB.LLx) + Padding,
                Math.Max(boundsA.URy, boundsB.URy) + Padding);
        }

        return new PdfRect<double>(
            Math.Min(boundsA.LLx, boundsB.LLx) - Padding,
            Math.Min(boundsA.URy, boundsB.URy) - Padding,
            Math.Max(boundsA.URx, boundsB.URx) + Padding,
            Math.Max(boundsA.LLy, boundsB.LLy) + Padding);
    }
}

/// <summary>Coordinate resolved from a declared toleranced zone.</summary>
public sealed record TolerancedZoneLayoutCoord(string ZoneId) : LayoutCoord
{
    public override string DebugString => $"TolerancedZone({ZoneId})";

    public override PdfRect<double> Resolve(RemediationEvaluationContext context, RemediationCandidate candidate)
    {
        var resolution = context.ResolveTolerancedZone(ZoneId);
        if (resolution == null)
        {
            throw new InvalidOperationException($"Could not resolve toleranced zone '{ZoneId}'.");
        }

        return resolution.TolerancedBounds;
    }
}

/// <summary>Coordinate resolved from a declared flow region.</summary>
public sealed record FlowRegionLayoutCoord(string RegionId) : LayoutCoord
{
    public override string DebugString => $"FlowRegion({RegionId})";

    public override PdfRect<double> Resolve(RemediationEvaluationContext context, RemediationCandidate candidate)
    {
        var resolution = context.ResolveFlowRegion(RegionId);
        if (resolution == null)
        {
            throw new InvalidOperationException($"Could not resolve flow region '{RegionId}'.");
        }

        return resolution.Bounds;
    }
}

/// <summary>
/// Expansion applied to an anchor-derived layout coordinate.
/// </summary>
public sealed record LayoutCoordExpansion(LayoutCoordExpansionKind Kind, double Amount)
{
    /// <summary>Inflates all sides by the specified amount.</summary>
    public static LayoutCoordExpansion Inflate(double amount) => new(LayoutCoordExpansionKind.Inflate, amount);

    /// <summary>Creates a rectangle above the source bounds.</summary>
    public static LayoutCoordExpansion Above(double amount) => new(LayoutCoordExpansionKind.Above, amount);

    /// <summary>Creates a rectangle below the source bounds.</summary>
    public static LayoutCoordExpansion Below(double amount) => new(LayoutCoordExpansionKind.Below, amount);

    internal PdfRect<double> Apply(PdfRect<double> rect) =>
        Kind switch
        {
            LayoutCoordExpansionKind.Inflate => new PdfRect<double>(
                rect.LLx - Amount,
                rect.LLy - Amount,
                rect.URx + Amount,
                rect.URy + Amount),
            LayoutCoordExpansionKind.Above => new PdfRect<double>(
                rect.LLx,
                rect.URy,
                rect.URx,
                rect.URy + Amount),
            LayoutCoordExpansionKind.Below => new PdfRect<double>(
                rect.LLx,
                rect.LLy - Amount,
                rect.URx,
                rect.LLy),
            _ => throw new ArgumentOutOfRangeException(nameof(Kind), Kind, null)
        };
}

/// <summary>
/// Expansion direction for anchor-derived coordinates.
/// </summary>
public enum LayoutCoordExpansionKind
{
    /// <summary>Inflate all sides.</summary>
    Inflate,
    /// <summary>Expand above the source bounds.</summary>
    Above,
    /// <summary>Expand below the source bounds.</summary>
    Below
}

/// <summary>
/// Built-in named layout zones resolved from session margin configuration.
/// </summary>
public enum NamedLayoutZone
{
    /// <summary>Top page zone.</summary>
    Header,
    /// <summary>Bottom page zone.</summary>
    Footer,
    /// <summary>Body area between configured margins.</summary>
    Body,
    /// <summary>Left margin zone.</summary>
    LeftMargin,
    /// <summary>Right margin zone.</summary>
    RightMargin
}

internal static class LayoutCoordHelpers
{
    internal static PdfRect<double> ResolveOffsets(
        PdfRect<double> page,
        double? top,
        double? right,
        double? bottom,
        double? left)
    {
        if (top != null && right == null && bottom == null && left == null)
        {
            return new PdfRect<double>(page.LLx, page.URy - top.Value, page.URx, page.URy);
        }

        if (bottom != null && top == null && right == null && left == null)
        {
            return new PdfRect<double>(page.LLx, page.LLy, page.URx, page.LLy + bottom.Value);
        }

        if (left != null && top == null && right == null && bottom == null)
        {
            return new PdfRect<double>(page.LLx, page.LLy, page.LLx + left.Value, page.URy);
        }

        if (right != null && top == null && bottom == null && left == null)
        {
            return new PdfRect<double>(page.URx - right.Value, page.LLy, page.URx, page.URy);
        }

        return new PdfRect<double>(
            page.LLx + (left ?? 0),
            page.LLy + (bottom ?? 0),
            page.URx - (right ?? 0),
            page.URy - (top ?? 0));
    }
}
