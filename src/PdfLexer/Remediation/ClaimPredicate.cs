using PdfLexer.Content;

namespace PdfLexer.Remediation;

/// <summary>
/// Predicate evaluated against existing remediation claims.
/// </summary>
public abstract record ClaimPredicate
{
    /// <summary>Predicate that matches every claim.</summary>
    public static ClaimPredicate Always { get; } = new ConstantClaimPredicate(true, "true");

    /// <summary>Predicate that matches no claims.</summary>
    public static ClaimPredicate Never { get; } = new ConstantClaimPredicate(false, "false");

    /// <summary>Debug representation used in reports.</summary>
    public abstract string DebugString { get; }

    /// <summary>Evaluates the predicate against a claim in context.</summary>
    public abstract PredicateResult Evaluate(ClaimPredicateEvaluationContext context, RemediationClaim claim);

    /// <summary>Evaluates the predicate against a claim with an empty context.</summary>
    public PredicateResult Evaluate(RemediationClaim claim) => Evaluate(ClaimPredicateEvaluationContext.Empty, claim);

    /// <summary>Combines this predicate with another using logical AND.</summary>
    public ClaimPredicate And(ClaimPredicate other) => new CompositeClaimPredicate(CompositePredicateKind.And, this, other);

    /// <summary>Combines this predicate with another using logical OR.</summary>
    public ClaimPredicate Or(ClaimPredicate other) => new CompositeClaimPredicate(CompositePredicateKind.Or, this, other);

    /// <summary>Negates this predicate.</summary>
    public ClaimPredicate Not() => new NotClaimPredicate(this);
}

/// <summary>
/// Context available while evaluating claim predicates.
/// </summary>
public sealed record ClaimPredicateEvaluationContext(
    IReadOnlyList<RemediationClaim> Claims,
    RemediationClaim? PreviousClaim = null,
    RemediationClaim? NextClaim = null,
    PdfRect<double>? PageBox = null,
    RemediationSessionConfiguration? Configuration = null,
    IReadOnlyDictionary<string, RemediationAnchor>? Anchors = null,
    IReadOnlyDictionary<string, TolerancedZone>? TolerancedZones = null,
    IReadOnlyDictionary<string, FlowRegion>? FlowRegions = null,
    StructuredTextPage? StructuredText = null,
    List<string>? Diagnostics = null)
{
    /// <summary>Empty claim-predicate evaluation context.</summary>
    public static ClaimPredicateEvaluationContext Empty { get; } = new(Array.Empty<RemediationClaim>());
}

/// <summary>Claim predicate with a constant result.</summary>
public sealed record ConstantClaimPredicate(bool Value, string Name) : ClaimPredicate
{
    public override string DebugString => Name;

    public override PredicateResult Evaluate(ClaimPredicateEvaluationContext context, RemediationClaim claim) =>
        Value ? PredicateResult.Match() : PredicateResult.NoMatch();
}

/// <summary>Claim predicate that combines two predicates.</summary>
public sealed record CompositeClaimPredicate(
    CompositePredicateKind Kind,
    ClaimPredicate Left,
    ClaimPredicate Right) : ClaimPredicate
{
    public override string DebugString => $"({Left.DebugString} {Kind} {Right.DebugString})";

    public override PredicateResult Evaluate(ClaimPredicateEvaluationContext context, RemediationClaim claim)
    {
        var left = Left.Evaluate(context, claim);
        if (Kind == CompositePredicateKind.And)
        {
            if (!left.IsMatch)
            {
                return left;
            }

            var right = Right.Evaluate(context, claim);
            return right.IsMatch
                ? PredicateResult.Match(Math.Min(left.Confidence, right.Confidence))
                : right;
        }

        if (left.IsMatch)
        {
            return left;
        }

        var orRight = Right.Evaluate(context, claim);
        return orRight.IsMatch ? orRight : PredicateResult.NoMatch(left.Reason ?? orRight.Reason);
    }
}

/// <summary>Claim predicate that negates another predicate.</summary>
public sealed record NotClaimPredicate(ClaimPredicate Inner) : ClaimPredicate
{
    public override string DebugString => $"Not({Inner.DebugString})";

    public override PredicateResult Evaluate(ClaimPredicateEvaluationContext context, RemediationClaim claim)
    {
        var result = Inner.Evaluate(context, claim);
        return result.IsMatch ? PredicateResult.NoMatch() : PredicateResult.Match(result.Confidence);
    }
}

/// <summary>
/// Built-in claim-predicate category.
/// </summary>
public enum ClaimPredicateKind
{
    /// <summary>Matches by produced structure tag.</summary>
    ClaimIs,
    /// <summary>Matches by remediation action kind.</summary>
    ActionIs,
    /// <summary>Matches claims from a rule id.</summary>
    FromRule,
    /// <summary>Matches claims from a rule set id.</summary>
    FromRuleSet,
    /// <summary>Matches claim lifecycle status.</summary>
    StatusIs,
    /// <summary>Matches claims on the same page as the previous claim.</summary>
    SamePage,
    /// <summary>Matches claims consecutive with the previous claim.</summary>
    Consecutive,
    /// <summary>Matches claims within a layout coordinate, anchor, zone, or flow region.</summary>
    Within,
    /// <summary>Matches claims before another rule's claim.</summary>
    BeforeClaim,
    /// <summary>Matches claims after another rule's claim.</summary>
    AfterClaim
}

/// <summary>
/// Built-in claim predicate implementation.
/// </summary>
public sealed record BuiltInClaimPredicate : ClaimPredicate
{
    /// <summary>Creates a built-in claim predicate.</summary>
    public BuiltInClaimPredicate(
        ClaimPredicateKind kind,
        string? value = null,
        ClaimStatus? status = null,
        LayoutCoord? layoutCoord = null,
        RemediationActionKind? actionKind = null)
    {
        Kind = kind;
        Value = value;
        Status = status;
        LayoutCoord = layoutCoord;
        ActionKind = actionKind;
    }

    /// <summary>Predicate category.</summary>
    public ClaimPredicateKind Kind { get; }

    /// <summary>String value used by the predicate, when applicable.</summary>
    public string? Value { get; }

    /// <summary>Status value used by the predicate, when applicable.</summary>
    public ClaimStatus? Status { get; }

    /// <summary>Layout coordinate used by the predicate, when applicable.</summary>
    public LayoutCoord? LayoutCoord { get; }

    /// <summary>Action kind used by the predicate, when applicable.</summary>
    public RemediationActionKind? ActionKind { get; }

    public override string DebugString => Kind switch
    {
        ClaimPredicateKind.ClaimIs => $"ClaimIs({Value})",
        ClaimPredicateKind.ActionIs => $"ActionIs({ActionKind})",
        ClaimPredicateKind.FromRule => $"FromRule({Value})",
        ClaimPredicateKind.FromRuleSet => $"FromRuleSet({Value})",
        ClaimPredicateKind.StatusIs => $"StatusIs({Status})",
        ClaimPredicateKind.SamePage => "SamePage",
        ClaimPredicateKind.Consecutive => "Consecutive",
        ClaimPredicateKind.Within => LayoutCoord != null ? $"Within({LayoutCoord.DebugString})" : $"Within({Value})",
        ClaimPredicateKind.BeforeClaim => $"BeforeClaim({Value})",
        ClaimPredicateKind.AfterClaim => $"AfterClaim({Value})",
        _ => Kind.ToString()
    };

    public override PredicateResult Evaluate(ClaimPredicateEvaluationContext context, RemediationClaim claim)
    {
        return Kind switch
        {
            ClaimPredicateKind.ClaimIs => string.Equals(claim.ProducedTag, Value, StringComparison.Ordinal),
            ClaimPredicateKind.ActionIs => ActionKind == claim.ActionKind,
            ClaimPredicateKind.FromRule => string.Equals(claim.RuleId, Value, StringComparison.Ordinal),
            ClaimPredicateKind.FromRuleSet => string.Equals(claim.RuleSetId, Value, StringComparison.Ordinal),
            ClaimPredicateKind.StatusIs => claim.Status == Status,
            ClaimPredicateKind.SamePage => context.PreviousClaim == null || context.PreviousClaim.PageIndex == claim.PageIndex,
            ClaimPredicateKind.Consecutive => IsConsecutive(context.PreviousClaim, claim),
            ClaimPredicateKind.Within => IsWithin(context, claim),
            ClaimPredicateKind.BeforeClaim => IsBeforeClaim(context, claim),
            ClaimPredicateKind.AfterClaim => IsAfterClaim(context, claim),
            _ => false
        } ? PredicateResult.Match() : PredicateResult.NoMatch();
    }

    private static bool IsConsecutive(RemediationClaim? previous, RemediationClaim claim)
    {
        return previous == null ||
            previous.PageIndex == claim.PageIndex &&
            previous.LastSequenceIndex + 1 == claim.FirstSequenceIndex;
    }

    private bool IsWithin(ClaimPredicateEvaluationContext context, RemediationClaim claim)
    {
        if (claim.BoundingBox is not { } claimBox)
        {
            return false;
        }

        if (LayoutCoord == null && Value == null)
        {
            return false;
        }

        var candidate = claim.Candidates.FirstOrDefault();
        if (candidate == null)
        {
            return false;
        }

        var eval = new RemediationEvaluationContext(
            context.Claims,
            pageBox: context.PageBox ?? new PdfRect<double>(0, 0, 0, 0),
            configuration: context.Configuration,
            anchors: context.Anchors,
            tolerancedZones: context.TolerancedZones,
            flowRegions: context.FlowRegions,
            structuredText: context.StructuredText,
            diagnostics: context.Diagnostics);
        if (LayoutCoord != null)
        {
            return LayoutCoord.Resolve(eval, candidate).CheckEnclosure(claimBox) == EncloseType.Full;
        }

        if (Value != null && eval.FlowRegions.ContainsKey(Value))
        {
            return eval.ResolveFlowRegion(Value)?.Contains(candidate) == true;
        }

        if (Value != null && eval.TolerancedZones.ContainsKey(Value))
        {
            return eval.ResolveTolerancedZone(Value)?.Contains(claimBox).IsMatch == true;
        }

        if (Value != null && eval.Anchors.ContainsKey(Value))
        {
            return eval.ResolveAnchor(Value)?.Bounds.CheckEnclosure(claimBox) == EncloseType.Full;
        }

        return false;
    }

    private bool IsBeforeClaim(ClaimPredicateEvaluationContext context, RemediationClaim claim)
    {
        var other = context.Claims.FirstOrDefault(x => string.Equals(x.RuleId, Value, StringComparison.Ordinal));
        return other != null && claim.PageIndex == other.PageIndex && claim.LastSequenceIndex < other.FirstSequenceIndex;
    }

    private bool IsAfterClaim(ClaimPredicateEvaluationContext context, RemediationClaim claim)
    {
        var other = context.Claims.FirstOrDefault(x => string.Equals(x.RuleId, Value, StringComparison.Ordinal));
        return other != null && claim.PageIndex == other.PageIndex && claim.FirstSequenceIndex > other.LastSequenceIndex;
    }
}

/// <summary>
/// Factory helpers for claim predicates.
/// </summary>
public static class ClaimPredicates
{
    /// <summary>Matches claims that produced the specified structure tag.</summary>
    public static ClaimPredicate ClaimIs(string tag) =>
        new BuiltInClaimPredicate(ClaimPredicateKind.ClaimIs, tag);

    /// <summary>Matches claims created by the specified action kind.</summary>
    public static ClaimPredicate ActionIs(RemediationActionKind actionKind) =>
        new BuiltInClaimPredicate(ClaimPredicateKind.ActionIs, actionKind: actionKind);

    /// <summary>Matches claims produced by the specified rule.</summary>
    public static ClaimPredicate FromRule(string ruleId) =>
        new BuiltInClaimPredicate(ClaimPredicateKind.FromRule, ruleId);

    /// <summary>Matches claims produced by the specified rule set.</summary>
    public static ClaimPredicate FromRuleSet(string ruleSetId) =>
        new BuiltInClaimPredicate(ClaimPredicateKind.FromRuleSet, ruleSetId);

    /// <summary>Matches claims with the specified lifecycle status.</summary>
    public static ClaimPredicate StatusIs(ClaimStatus status) =>
        new BuiltInClaimPredicate(ClaimPredicateKind.StatusIs, status: status);

    /// <summary>Matches claims on the same page as the previous claim.</summary>
    public static ClaimPredicate SamePage() =>
        new BuiltInClaimPredicate(ClaimPredicateKind.SamePage);

    /// <summary>Matches claims consecutive with the previous claim.</summary>
    public static ClaimPredicate Consecutive() =>
        new BuiltInClaimPredicate(ClaimPredicateKind.Consecutive);

    /// <summary>Matches claims within a layout coordinate.</summary>
    public static ClaimPredicate Within(LayoutCoord coord) =>
        new BuiltInClaimPredicate(ClaimPredicateKind.Within, layoutCoord: coord);

    /// <summary>Matches claims within a named anchor, toleranced zone, or flow region.</summary>
    public static ClaimPredicate Within(string anchorOrFlowRegionId) =>
        new BuiltInClaimPredicate(ClaimPredicateKind.Within, anchorOrFlowRegionId);

    /// <summary>Matches claims before a claim produced by the specified rule.</summary>
    public static ClaimPredicate BeforeClaim(string ruleId) =>
        new BuiltInClaimPredicate(ClaimPredicateKind.BeforeClaim, ruleId);

    /// <summary>Matches claims after a claim produced by the specified rule.</summary>
    public static ClaimPredicate AfterClaim(string ruleId) =>
        new BuiltInClaimPredicate(ClaimPredicateKind.AfterClaim, ruleId);
}
