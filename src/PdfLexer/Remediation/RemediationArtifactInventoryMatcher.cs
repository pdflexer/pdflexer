using PdfLexer.Content;

namespace PdfLexer.Remediation;

/// <summary>One produced artifact graded against the declared inventory.</summary>
/// <remarks>A null <paramref name="Subtype"/> means an untyped residual and cannot satisfy an
/// explicit preview artifact declaration.</remarks>
internal sealed record RemediationArtifactRecord(
    int PageIndex,
    ArtifactSubtype? Subtype,
    PdfRect<double> RelativeBounds,
    string? RuleId = null,
    string? ProgramId = null,
    string? BoundItemId = null,
    ArtifactSemanticSubtype? SemanticSubtype = null);

internal static class RemediationArtifactInventoryMatcher
{
    /// <summary>
    /// Resolves the inventory item one produced artifact belongs to, or null when it matches nothing
    /// declared. Pure: identical inputs always yield the same answer, so plan-time grading and
    /// apply-time subtype adoption cannot disagree.
    /// </summary>
    internal static string? ResolveItemId(
        IReadOnlyList<ArtifactDeclaration> items,
        RemediationArtifactRecord record,
        int pageCount,
        IReadOnlyDictionary<string, TolerancedZoneResolution> zones)
    {
        if (items.Count == 0)
        {
            return null;
        }

        if (record.BoundItemId != null)
        {
            var bound = items.FirstOrDefault(x => string.Equals(x.Id, record.BoundItemId, StringComparison.Ordinal));
            return bound != null && Accepts(bound, record, pageCount, zones) ? bound.Id : null;
        }

        return items.FirstOrDefault(x => Accepts(x, record, pageCount, zones))?.Id;
    }

    private static bool Accepts(
        ArtifactDeclaration item,
        RemediationArtifactRecord record,
        int pageCount,
        IReadOnlyDictionary<string, TolerancedZoneResolution> zones)
    {
        if (!item.Pages.Includes(record.PageIndex, pageCount))
        {
            return false;
        }

        if (record.Subtype == null || record.Subtype != item.Subtype)
        {
            return false;
        }
        if (record.SemanticSubtype != null && record.SemanticSubtype != item.SemanticSubtype)
        {
            return false;
        }

        return true;
    }

    internal static IReadOnlyList<RemediationTemplateDifference> Match(
        IReadOnlyList<ArtifactDeclaration> items,
        IReadOnlyList<RemediationArtifactRecord> records,
        int pageCount,
        IReadOnlyDictionary<int, IReadOnlyDictionary<string, TolerancedZoneResolution>> zonesByPage)
    {
        var differences = new List<RemediationTemplateDifference>();
        if (items.Count == 0)
        {
            return differences;
        }

        const string fallbackOwner = "program";
        var counts = new Dictionary<(string ItemId, int PageIndex), int>();
        foreach (var page in records.GroupBy(x => x.PageIndex).OrderBy(x => x.Key))
        {
            var zones = zonesByPage.TryGetValue(page.Key, out var resolved)
                ? resolved
                : new Dictionary<string, TolerancedZoneResolution>(StringComparer.Ordinal);
            var ordinal = 0;
            foreach (var record in page)
            {
                ordinal++;
                var itemId = ResolveItemId(items, record, pageCount, zones);
                if (itemId == null)
                {
                    differences.Add(new RemediationTemplateDifference(
                        RemediationTemplateDifferenceKind.UndeclaredArtifact,
                        DiagnosticCode.ArtifactUndeclared,
                        record.ProgramId ?? fallbackOwner,
                        record.BoundItemId,
                        null,
                        $"Page{record.PageIndex + 1}/Artifact[{ordinal}]",
                        "a declared artifact",
                        record.Subtype?.ToString() ?? "untyped",
                        new[] { record.PageIndex },
                        record.RuleId,
                        false));
                    continue;
                }

                var key = (itemId, record.PageIndex);
                counts[key] = counts.GetValueOrDefault(key) + 1;
            }
        }

        foreach (var item in items)
        {
            foreach (var pageIndex in item.Pages.SelectPages(pageCount))
            {
                var count = counts.GetValueOrDefault((item.Id, pageIndex));
                if (item.Occurrence.Accepts(count))
                {
                    continue;
                }

                var kind = count == 0
                    ? RemediationTemplateDifferenceKind.MissingDeclaredArtifact
                    : RemediationTemplateDifferenceKind.ArtifactOccurrenceViolation;
                differences.Add(new RemediationTemplateDifference(
                    kind,
                    kind == RemediationTemplateDifferenceKind.MissingDeclaredArtifact
                        ? DiagnosticCode.ArtifactMissingDeclared
                        : DiagnosticCode.ArtifactOccurrenceViolation,
                    fallbackOwner,
                    item.Id,
                    $"Artifact:{item.Id}",
                    $"Page{pageIndex + 1}",
                    item.Occurrence.Description,
                    count.ToString(),
                    new[] { pageIndex },
                    null,
                    false));
            }
        }

        return differences;
    }
}
