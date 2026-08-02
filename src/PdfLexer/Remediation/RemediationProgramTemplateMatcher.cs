namespace PdfLexer.Remediation;

internal static class RemediationProgramTemplateMatcher
{
    internal static IReadOnlyList<RemediationTemplateDifference> Match(
        string programId,
        RemediationTemplate template,
        RemediationSemanticTree tree,
        int pageCount,
        IReadOnlySet<string>? boundSlots = null)
    {
        ArgumentNullException.ThrowIfNull(template);
        var differences = new List<RemediationTemplateDifference>();
        MatchChildren(
            programId,
            template.Document,
            tree.Roots,
            "/",
            pageCount,
            boundSlots ?? new HashSet<string>(StringComparer.Ordinal),
            differences);
        return differences;
    }

    private static void MatchChildren(
        string programId,
        RemediationTemplateNode parent,
        IReadOnlyList<RemediationSemanticNode> actual,
        string parentPath,
        int pageCount,
        IReadOnlySet<string> boundSlots,
        List<RemediationTemplateDifference> differences)
    {
        var actualIndex = 0;
        var missing = new Dictionary<RemediationTemplateNode, RemediationTemplateDifference>(
            ReferenceEqualityComparer.Instance);

        for (var particleIndex = 0; particleIndex < parent.Children.Count; particleIndex++)
        {
            var particle = parent.Children[particleIndex];
            var particlePath = ExpectedPath(parentPath, parent.Children, particleIndex);
            var min = particle.Occurrence is TemplateOccurrence.ExactlyOne or TemplateOccurrence.OneOrMore ? 1 : 0;
            var max = particle.Occurrence is TemplateOccurrence.ExactlyOne or TemplateOccurrence.Optional ? 1 : int.MaxValue;
            var matches = new List<RemediationSemanticNode>();
            while (actualIndex < actual.Count && matches.Count < max &&
                   Matches(particle, actual[actualIndex], particlePath, boundSlots))
            {
                matches.Add(actual[actualIndex++]);
            }

            if (matches.Count < min)
            {
                var kind = particle.Name != null
                    ? RemediationTemplateDifferenceKind.SlotUnfilled
                    : particle.Occurrence == TemplateOccurrence.OneOrMore
                        ? RemediationTemplateDifferenceKind.OccurrenceViolation
                        : RemediationTemplateDifferenceKind.MissingRequired;
                var difference = Create(
                    programId,
                    kind,
                    particle,
                    particlePath,
                    actualIndex < actual.Count ? ActualPath(parentPath, actual[actualIndex], actualIndex + 1) : null,
                    ExpectedCount(particle),
                    "0",
                    Array.Empty<int>(),
                    null);
                differences.Add(difference);
                missing[particle] = difference;
                continue;
            }

            for (var occurrence = 0; occurrence < matches.Count; occurrence++)
            {
                var node = matches[occurrence];
                var path = particlePath + (matches.Count > 1 || particle.Occurrence is TemplateOccurrence.ZeroOrMore or TemplateOccurrence.OneOrMore ? $"[{occurrence + 1}]" : string.Empty);
                CheckPageConstraints(programId, particle, node, path, pageCount, differences);
                if (!RemediationStructuralTemplateValidator.LegalChild(parent.Tag, node.Tag))
                {
                    differences.Add(Create(
                        programId,
                        RemediationTemplateDifferenceKind.IllegalNesting,
                        particle,
                        path,
                        path,
                        parent.Tag,
                        node.Tag,
                        AllPages(node),
                        node.RuleId));
                }

                MatchChildren(programId, particle, node.Children, path, pageCount, boundSlots, differences);
            }
        }

        while (actualIndex < actual.Count)
        {
            var node = actual[actualIndex];
            var expectedIndex = FindMatchingParticle(parent.Children, node, parentPath, boundSlots);
            var expected = expectedIndex >= 0 ? parent.Children[expectedIndex] : null;
            if (expected != null && missing.Remove(expected, out var falseMissing))
                differences.Remove(falseMissing);
            var kind = expected != null
                ? RemediationTemplateDifferenceKind.WrongOrder
                : RemediationTemplateDifferenceKind.UnexpectedNode;
            differences.Add(Create(
                programId,
                kind,
                expected,
                expectedIndex >= 0 ? ExpectedPath(parentPath, parent.Children, expectedIndex) : null,
                ActualPath(parentPath, node, actualIndex + 1),
                null,
                node.Tag,
                AllPages(node),
                node.RuleId,
                node.SlotId));
            actualIndex++;
        }
    }

    private static void CheckPageConstraints(
        string programId,
        RemediationTemplateNode particle,
        RemediationSemanticNode node,
        string path,
        int pageCount,
        List<RemediationTemplateDifference> differences)
    {
        var pages = AllPages(node);
        _ = programId;
        _ = particle;
        _ = path;
        _ = pageCount;
        _ = differences;
        _ = pages;
    }

    private static bool Matches(
        RemediationTemplateNode expected,
        RemediationSemanticNode actual,
        string expectedPath,
        IReadOnlySet<string> boundSlots)
    {
        if (!string.Equals(expected.Tag, actual.Tag, StringComparison.Ordinal))
            return false;
        if (expected.Name == null)
            return actual.SlotId == null;
        var slot = expectedPath[1..];
        var actualSlot = actual.SlotId == null ? null : (actual.SlotId.Contains('[') ? actual.SlotId.Substring(0, actual.SlotId.IndexOf('[')) : actual.SlotId);
        return boundSlots.Contains(slot)
            ? string.Equals(slot, actualSlot, StringComparison.Ordinal)
            : actual.SlotId == null || string.Equals(slot, actualSlot, StringComparison.Ordinal);
    }

    private static int FindMatchingParticle(
        IReadOnlyList<RemediationTemplateNode> particles,
        RemediationSemanticNode actual,
        string parentPath,
        IReadOnlySet<string> boundSlots)
    {
        for (var i = 0; i < particles.Count; i++)
        {
            if (Matches(particles[i], actual, ExpectedPath(parentPath, particles, i), boundSlots))
                return i;
        }
        return -1;
    }

    internal static string ExpectedPath(
        string parentPath,
        IReadOnlyList<RemediationTemplateNode> siblings,
        int index)
    {
        var particle = siblings[index];
        var cleanParent = parentPath.Contains('[') ? parentPath.Substring(0, parentPath.IndexOf('[')) : parentPath;
        if (particle.Name != null)
            return cleanParent == "/" ? $"/{particle.Name}" : $"{cleanParent}/{particle.Name}";
        var sameTagCount = siblings.Count(x => x.Name == null && x.Tag == particle.Tag);
        var basePath = cleanParent == "/" ? $"/{particle.Tag}" : $"{cleanParent}/{particle.Tag}";
        if (sameTagCount == 1) return basePath;
        var ordinal = siblings.Take(index + 1).Count(x => x.Name == null && x.Tag == particle.Tag);
        return $"{basePath}[{ordinal}]";
    }

    private static IReadOnlyList<int> AllPages(RemediationSemanticNode node) =>
        node.PageIndexes.Concat(node.Children.SelectMany(AllPages)).Distinct().OrderBy(x => x).ToArray();

    private static string ActualPath(string parent, RemediationSemanticNode node, int index) =>
        $"{parent}{(parent == "/" ? string.Empty : "/")}{node.SlotId ?? node.Tag}[{index}]";

    private static string ExpectedCount(RemediationTemplateNode node) => node.Occurrence switch
    {
        TemplateOccurrence.ExactlyOne => "exactly 1",
        TemplateOccurrence.Optional => "0..1",
        TemplateOccurrence.ZeroOrMore => "0..*",
        _ => "1..*"
    };

    private static RemediationTemplateDifference Create(
        string programId,
        RemediationTemplateDifferenceKind kind,
        RemediationTemplateNode? expectedParticle,
        string? expectedPath,
        string? actualPath,
        string? expectedValue,
        string? actualValue,
        IReadOnlyList<int> pageIndexes,
        string? ruleId,
        string? slotId = null) =>
        new(
            kind,
            DiagnosticCodeFor(kind),
            programId,
            expectedParticle?.Name ?? slotId,
            expectedPath,
            actualPath,
            expectedValue,
            actualValue,
            pageIndexes,
            ruleId,
            false)
        {
            ProgramSlot = expectedParticle?.Name != null ? SlotRef.TryParse(expectedPath, out var parsed) ? parsed : null : null
        };

    private static DiagnosticCode DiagnosticCodeFor(RemediationTemplateDifferenceKind kind) => kind switch
    {
        RemediationTemplateDifferenceKind.MissingRequired => DiagnosticCode.TemplateMissingRequired,
        RemediationTemplateDifferenceKind.UnexpectedNode => DiagnosticCode.TemplateUnexpectedNode,
        RemediationTemplateDifferenceKind.WrongOrder => DiagnosticCode.TemplateWrongOrder,
        RemediationTemplateDifferenceKind.OccurrenceViolation => DiagnosticCode.TemplateOccurrenceViolation,
        RemediationTemplateDifferenceKind.IllegalNesting => DiagnosticCode.TemplateIllegalNesting,
        RemediationTemplateDifferenceKind.SlotUnfilled => DiagnosticCode.TemplateSlotUnfilled,
        RemediationTemplateDifferenceKind.PageMismatch => DiagnosticCode.TemplatePageMismatch,
        RemediationTemplateDifferenceKind.PageSpanMismatch => DiagnosticCode.TemplatePageSpanMismatch,
        RemediationTemplateDifferenceKind.MaterializationDivergence => DiagnosticCode.TemplateMaterializationDivergence,
        _ => DiagnosticCode.TemplateMissingRequired
    };
}
