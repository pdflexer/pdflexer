namespace PdfLexer.Remediation;

internal static class RemediationStructuralTemplateMatcher
{
    internal static IReadOnlyList<RemediationTemplateDifference> Match(
        string ruleSetId,
        RemediationStructuralTemplate template,
        RemediationSemanticTree tree,
        int pageCount,
        IReadOnlySet<string>? boundSlots = null)
    {
        var differences = new List<RemediationTemplateDifference>();
        MatchChildren(
            ruleSetId, template.Document, tree.Roots, "Document", pageCount,
            boundSlots ?? new HashSet<string>(StringComparer.Ordinal), differences);
        return differences;
    }

    private static void MatchChildren(
        string ruleSetId,
        RemediationStructuralTemplateNode parent,
        IReadOnlyList<RemediationSemanticNode> actual,
        string parentPath,
        int pageCount,
        IReadOnlySet<string> boundSlots,
        List<RemediationTemplateDifference> differences)
    {
        var ai = 0;
        var missing = new Dictionary<RemediationStructuralTemplateNode, RemediationTemplateDifference>(
            ReferenceEqualityComparer.Instance);
        for (var particleIndex = 0; particleIndex < parent.Children.Count; particleIndex++)
        {
            var particle = parent.Children[particleIndex];
            var particlePath = ExpectedPath(parentPath, parent.Children, particleIndex);
            var min = particle.Occurrence is RemediationStructuralOccurrence.ExactlyOne or
                RemediationStructuralOccurrence.OneOrMore ? 1 : 0;
            var max = particle.Occurrence is RemediationStructuralOccurrence.ExactlyOne or
                RemediationStructuralOccurrence.Optional ? 1 : int.MaxValue;
            var matches = new List<RemediationSemanticNode>();
            while (ai < actual.Count && matches.Count < max &&
                   Matches(particle, actual[ai], boundSlots))
            {
                matches.Add(actual[ai++]);
            }

            if (matches.Count < min)
            {
                var kind = particle.Id != null
                    ? RemediationTemplateDifferenceKind.SlotUnfilled
                    : particle.Occurrence == RemediationStructuralOccurrence.OneOrMore
                        ? RemediationTemplateDifferenceKind.OccurrenceViolation
                        : RemediationTemplateDifferenceKind.MissingRequired;
                var difference = Create(
                    ruleSetId, kind, particle, particlePath,
                    ai < actual.Count ? ActualPath(parentPath, actual[ai], ai + 1) : null,
                    ExpectedCount(particle), "0", Array.Empty<int>(), null);
                differences.Add(difference);
                missing[particle] = difference;
                continue;
            }

            for (var instance = 0; instance < matches.Count; instance++)
            {
                var node = matches[instance];
                var path = particlePath +
                    (max == int.MaxValue ? $"[{instance + 1}]" : string.Empty);
                CheckPageConstraints(ruleSetId, particle, node, path, pageCount, differences);
                if (!RemediationStructuralTemplateValidator.LegalChild(parent.Tag, node.Tag))
                {
                    differences.Add(Create(ruleSetId, RemediationTemplateDifferenceKind.IllegalNesting,
                        particle, path, path, parent.Tag, node.Tag, AllPages(node), node.RuleId));
                }
                if (!node.OpaqueTemplateInterior)
                {
                    MatchChildren(ruleSetId, particle, node.Children, path, pageCount, boundSlots, differences);
                }
            }
        }

        while (ai < actual.Count)
        {
            var node = actual[ai];
            var expectedIndex = FindMatchingParticle(parent.Children, node, boundSlots);
            var expected = expectedIndex >= 0 ? parent.Children[expectedIndex] : null;
            if (expected != null && missing.Remove(expected, out var falseMissing))
            {
                differences.Remove(falseMissing);
            }
            var kind = expected != null
                ? RemediationTemplateDifferenceKind.WrongOrder
                : RemediationTemplateDifferenceKind.UnexpectedNode;
            differences.Add(Create(ruleSetId, kind, expected,
                expectedIndex >= 0 ? ExpectedPath(parentPath, parent.Children, expectedIndex) : null,
                ActualPath(parentPath, node, ai + 1), null, node.Tag, AllPages(node), node.RuleId,
                node.SlotId));
            ai++;
        }
    }

    private static void CheckPageConstraints(
        string ruleSetId,
        RemediationStructuralTemplateNode particle,
        RemediationSemanticNode node,
        string path,
        int pageCount,
        List<RemediationTemplateDifference> differences)
    {
        var pages = AllPages(node);
        if (particle.Pages != null && pages.Any(x => !particle.Pages.Includes(x, pageCount)))
        {
            differences.Add(Create(ruleSetId, RemediationTemplateDifferenceKind.PageMismatch,
                particle, path, path, particle.Pages.DebugString,
                string.Join(",", pages.Select(x => x + 1)), pages, node.RuleId));
        }
        if (particle.SpansPages is { } spans &&
            (spans ? pages.Count < 2 : pages.Count > 1))
        {
            differences.Add(Create(ruleSetId, RemediationTemplateDifferenceKind.PageSpanMismatch,
                particle, path, path, spans.ToString(), (pages.Count > 1).ToString(),
                pages, node.RuleId));
        }
    }

    private static bool Matches(
        RemediationStructuralTemplateNode expected,
        RemediationSemanticNode actual,
        IReadOnlySet<string> boundSlots) =>
        expected.Tag == actual.Tag &&
        (expected.Id == null
            ? actual.SlotId == null
            : boundSlots.Contains(expected.Id)
                ? expected.Id == actual.SlotId
                : actual.SlotId == null || expected.Id == actual.SlotId);

    private static int FindMatchingParticle(
        IReadOnlyList<RemediationStructuralTemplateNode> particles,
        RemediationSemanticNode actual,
        IReadOnlySet<string> boundSlots)
    {
        for (var i = 0; i < particles.Count; i++)
        {
            if (Matches(particles[i], actual, boundSlots)) return i;
        }
        return -1;
    }

    internal static string ExpectedPath(
        string parentPath,
        IReadOnlyList<RemediationStructuralTemplateNode> siblings,
        int index)
    {
        var particle = siblings[index];
        if (particle.Id != null) return $"{parentPath}/{particle.Id}";
        var sameTagCount = siblings.Count(x => x.Id == null && x.Tag == particle.Tag);
        if (sameTagCount == 1) return $"{parentPath}/{particle.Tag}";
        var ordinal = siblings.Take(index + 1).Count(x => x.Id == null && x.Tag == particle.Tag);
        return $"{parentPath}/{particle.Tag}[{ordinal}]";
    }

    private static IReadOnlyList<int> AllPages(RemediationSemanticNode node) =>
        node.PageIndexes.Concat(node.Children.SelectMany(AllPages)).Distinct().OrderBy(x => x).ToArray();

    private static string ActualPath(string parent, RemediationSemanticNode node, int index) =>
        $"{parent}/{node.SlotId ?? node.Tag}[{index}]";

    private static string ExpectedCount(RemediationStructuralTemplateNode node) => node.Occurrence switch
    {
        RemediationStructuralOccurrence.ExactlyOne => "exactly 1",
        RemediationStructuralOccurrence.Optional => "0..1",
        RemediationStructuralOccurrence.ZeroOrMore => "0..*",
        _ => "1..*"
    };

    private static RemediationTemplateDifference Create(
        string ruleSetId,
        RemediationTemplateDifferenceKind kind,
        RemediationStructuralTemplateNode? expected,
        string? expectedPath,
        string? actualPath,
        string? expectedValue,
        string? actualValue,
        IReadOnlyList<int> pages,
        string? ruleId,
        string? slot = null) =>
        new(kind, Code(kind), ruleSetId, expected?.Id ?? slot, expectedPath, actualPath,
            expectedValue, actualValue, pages, ruleId, false);

    private static DiagnosticCode Code(RemediationTemplateDifferenceKind kind) => kind switch
    {
        RemediationTemplateDifferenceKind.MissingRequired => DiagnosticCode.TemplateMissingRequired,
        RemediationTemplateDifferenceKind.UnexpectedNode => DiagnosticCode.TemplateUnexpectedNode,
        RemediationTemplateDifferenceKind.WrongOrder => DiagnosticCode.TemplateWrongOrder,
        RemediationTemplateDifferenceKind.OccurrenceViolation => DiagnosticCode.TemplateOccurrenceViolation,
        RemediationTemplateDifferenceKind.IllegalNesting => DiagnosticCode.TemplateIllegalNesting,
        RemediationTemplateDifferenceKind.SlotUnfilled => DiagnosticCode.TemplateSlotUnfilled,
        RemediationTemplateDifferenceKind.PageMismatch => DiagnosticCode.TemplatePageMismatch,
        RemediationTemplateDifferenceKind.PageSpanMismatch => DiagnosticCode.TemplatePageSpanMismatch,
        _ => DiagnosticCode.TemplateMaterializationDivergence
    };
}
