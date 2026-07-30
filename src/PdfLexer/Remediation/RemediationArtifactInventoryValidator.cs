namespace PdfLexer.Remediation;

internal static class RemediationArtifactInventoryValidator
{
    internal static IReadOnlyList<string> Validate(IReadOnlyList<RuleSet> ruleSets)
    {
        var errors = new List<string>();
        var items = ruleSets.SelectMany(x => x.Artifacts).ToList();
        var rules = ruleSets.SelectMany(x => x.Rules).ToList();

        if (items.Count == 0)
        {
            foreach (var rule in rules.Where(x => x.Artifact != null))
            {
                errors.Add($"Rule '{rule.Id}' binds artifact '{rule.Artifact}', but no artifact inventory is declared.");
            }
            return errors;
        }

        var byId = new Dictionary<string, RemediationArtifactInventoryItem>(StringComparer.Ordinal);
        foreach (var item in items)
        {
            errors.AddRange(item.Validate());
            if (!string.IsNullOrWhiteSpace(item.Id) && !byId.TryAdd(item.Id, item))
            {
                errors.Add($"Artifact inventory item id '{item.Id}' is duplicated across the composed rule set.");
            }
        }

        var zones = ruleSets.SelectMany(x => x.TolerancedZones).Select(x => x.Id).ToHashSet(StringComparer.Ordinal);
        foreach (var item in items.Where(x => x.ZoneId != null && !zones.Contains(x.ZoneId!)))
        {
            errors.Add($"Artifact inventory item '{item.Id}' references unknown toleranced zone '{item.ZoneId}'.");
        }

        // Page selectors are deliberately ignored: page count is unknown at declaration time, so a
        // one-page document is both first and last. Two items sharing a subtype must both be
        // discriminated geometrically.
        for (var i = 0; i < items.Count; i++)
        {
            for (var j = i + 1; j < items.Count; j++)
            {
                if (items[i].Subtype == items[j].Subtype &&
                    (items[i].ZoneId == null || items[j].ZoneId == null))
                {
                    errors.Add(
                        $"Artifact inventory items '{items[i].Id}' and '{items[j].Id}' both declare subtype " +
                        $"'{items[i].Subtype}', so both must declare a zone to be unambiguous.");
                }
            }
        }

        foreach (var rule in rules.Where(x => x.Artifact != null))
        {
            if (rule.Slot != null)
            {
                errors.Add($"Rule '{rule.Id}' cannot bind both structural-template slot '{rule.Slot}' and artifact '{rule.Artifact}'.");
            }

            if (!byId.TryGetValue(rule.Artifact!, out var target))
            {
                errors.Add($"Rule '{rule.Id}' references unknown artifact inventory item '{rule.Artifact}'.");
                continue;
            }

            if (rule.Action is not ArtifactRemediationAction artifact)
            {
                errors.Add($"Rule '{rule.Id}' binds artifact '{rule.Artifact}', but action '{rule.Action.Kind}' does not produce an artifact.");
            }
            else if (artifact.Subtype != target.Subtype)
            {
                errors.Add($"Rule '{rule.Id}' produces subtype '{artifact.Subtype}' but artifact '{rule.Artifact}' requires '{target.Subtype}'.");
            }
        }

        return errors;
    }
}
