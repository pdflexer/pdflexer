using PdfLexer.Content;

namespace PdfLexer.Remediation;

/// <summary>
/// Page-relative named zone with an allowed tolerance for layout drift.
/// </summary>
public sealed record TolerancedZone(
    /// <summary>Stable zone identifier.</summary>
    string Id,
    /// <summary>Base zone bounds resolved per page.</summary>
    LayoutCoord Bounds,
    /// <summary>Allowed expansion around the base bounds in user-space units.</summary>
    double Tolerance = 0,
    /// <summary>Confidence policy for candidates that land inside tolerance but outside base bounds.</summary>
    ZoneConfidenceBehavior ConfidenceBehavior = ZoneConfidenceBehavior.DegradeOutsideBaseBounds)
{
    /// <summary>Validates declaration shape.</summary>
    public IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(Id))
        {
            errors.Add("Toleranced zone id is required.");
        }

        if (Tolerance < 0)
        {
            errors.Add($"Toleranced zone '{Id}' has negative tolerance.");
        }

        return errors;
    }
}

/// <summary>
/// Confidence behavior for candidates matched by a toleranced zone.
/// </summary>
public enum ZoneConfidenceBehavior
{
    /// <summary>All candidates inside the toleranced bounds receive full confidence.</summary>
    FullWithinTolerance,
    /// <summary>Candidates outside base bounds but inside toleranced bounds receive degraded confidence.</summary>
    DegradeOutsideBaseBounds
}

/// <summary>
/// Per-page resolved toleranced-zone bounds.
/// </summary>
public sealed record TolerancedZoneResolution(
    /// <summary>Resolved zone identifier.</summary>
    string ZoneId,
    /// <summary>Base bounds before tolerance expansion.</summary>
    PdfRect<double> BaseBounds,
    /// <summary>Bounds after tolerance expansion.</summary>
    PdfRect<double> TolerancedBounds,
    /// <summary>Applied tolerance in user-space units.</summary>
    double Tolerance,
    /// <summary>Confidence policy for tolerated matches.</summary>
    ZoneConfidenceBehavior ConfidenceBehavior)
{
    /// <summary>Returns whether the candidate rectangle is inside the zone and with what confidence.</summary>
    public PredicateResult Contains(PdfRect<double> candidate)
    {
        if (BaseBounds.CheckEnclosure(candidate) == EncloseType.Full)
        {
            return PredicateResult.Match();
        }

        if (TolerancedBounds.CheckEnclosure(candidate) == EncloseType.Full)
        {
            var confidence = ConfidenceBehavior == ZoneConfidenceBehavior.FullWithinTolerance || Tolerance <= 0
                ? 1.0
                : 0.9;
            return PredicateResult.Match(confidence);
        }

        return PredicateResult.NoMatch();
    }
}
