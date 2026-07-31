namespace PdfLexer.Remediation;

internal static class RemediationStructuralTemplateValidator
{
    // Standard structure types from PDF 1.7 / PDF 2.0 used by the supported PDF/UA profiles.
    private static readonly HashSet<string> StandardTags = new(StringComparer.Ordinal)
    {
        "Document", "DocumentFragment", "Part", "Art", "Sect", "Div", "BlockQuote",
        "Caption", "TOC", "TOCI", "Index", "NonStruct", "Private",
        "H", "H1", "H2", "H3", "H4", "H5", "H6", "P",
        "L", "LI", "Lbl", "LBody", "Table", "TR", "TH", "TD",
        "THead", "TBody", "TFoot", "Span", "Quote", "Note", "Reference",
        "BibEntry", "Code", "Link", "Annot", "Ruby", "RB", "RT", "RP",
        "Warichu", "WT", "WP", "Figure", "Formula", "Form", "FENote",
        "Title", "Sub", "Em", "Strong"
    };

    internal static IReadOnlyList<string> Validate(IReadOnlyList<RuleSet> ruleSets)
    {
        var errors = new List<string>();
        var owners = ruleSets.Where(x => x.StructuralTemplate != null).ToList();
        if (owners.Count > 1)
        {
            errors.Add("Exactly one structural template is permitted across composed rule sets.");
            return errors;
        }
        if (owners.Count == 0)
        {
            foreach (var rule in ruleSets.SelectMany(x => x.Rules).Where(x => x.Slot != null))
            {
                errors.Add($"Rule '{rule.Id}' binds slot '{rule.Slot}', but no structural template is declared.");
            }
            return errors;
        }

        var owner = owners[0];
        var root = owner.StructuralTemplate!.Document;
        if (root.Tag != "Document" || root.Occurrence != RemediationStructuralOccurrence.ExactlyOne)
        {
            errors.Add("The structural template root must be exactly one Document node.");
        }
        if (root.Id != null)
        {
            errors.Add("The structural template Document root cannot be a bindable slot.");
        }

        var prescriptive = owner.StructuralTemplate.Mode == RemediationStructuralTemplateMode.Prescriptive;
        var slots = new Dictionary<string, (RemediationStructuralTemplateNode Node, string Path)>(StringComparer.Ordinal);
        var boundSlots = ruleSets.SelectMany(x => x.Rules).Where(x => x.Slot != null).Select(x => x.Slot!)
            .ToHashSet(StringComparer.Ordinal);
        ValidateNode(root, "Document", null, slots, boundSlots, errors, prescriptive);

        foreach (var rule in ruleSets.SelectMany(x => x.Rules))
        {
            var structureProducing = IsStructureProducing(rule.Action);
            if (prescriptive && structureProducing && rule.Slot == null)
            {
                errors.Add($"Prescriptive structural rule '{rule.Id}' must bind a template slot.");
                continue;
            }
            if (rule.Slot == null)
            {
                continue;
            }
            if (!slots.TryGetValue(rule.Slot!, out var target))
            {
                errors.Add($"Rule '{rule.Id}' references unknown structural-template slot '{rule.Slot}'.");
                continue;
            }
            var tableInterior = prescriptive && IsSpecializedTableInterior(root, rule.Slot!);
            if (prescriptive && !tableInterior && rule.Action is TagRemediationAction or GroupRemediationAction or MergeRemediationAction)
            {
                errors.Add($"Prescriptive rule '{rule.Id}' must use Bind, BindOver, or a supported specialized slot action; '{rule.Action.Kind}' would duplicate template structure.");
                continue;
            }
            if (prescriptive && rule.Action is StructureLinkRemediationAction)
            {
                errors.Add($"Prescriptive rule '{rule.Id}' cannot create a structural Link; bind link content or use AdoptAnnotation with a compatible Link slot.");
                continue;
            }
            if (rule.Action is BindTemplateSlotRemediationAction bind)
            {
                var composite = target.Node.Children.Count > 0;
                if (bind.Over == null && composite)
                {
                    errors.Add($"Rule '{rule.Id}' binds composite slot '{rule.Slot}' directly; use BindOver.");
                }
                if (bind.Over != null && !composite)
                {
                    errors.Add($"Rule '{rule.Id}' uses BindOver for leaf slot '{rule.Slot}'.");
                }
            }
            var tag = ProducedTag(rule.Action);
            if (!tableInterior && tag != null && !string.Equals(tag, target.Node.Tag, StringComparison.Ordinal))
            {
                errors.Add($"Rule '{rule.Id}' produces '{tag}' but slot '{rule.Slot}' requires '{target.Node.Tag}'.");
            }
            if (rule.Action is AdoptAnnotationRemediationAction &&
                target.Node.Tag is not ("Link" or "Form" or "Annot"))
            {
                errors.Add($"Rule '{rule.Id}' adopts annotations into slot '{rule.Slot}', which requires Link, Form, or Annot.");
            }
        }

        if (prescriptive)
        {
            foreach (var slot in slots)
            {
                if (IsSpecializedTableInterior(root, slot.Key)) continue;
                var producers = ruleSets.SelectMany(x => x.Rules).Where(x => x.Slot == slot.Key).ToArray();
                var composite = slot.Value.Node.Children.Count > 0;
                var groupBindings = producers.OfType<Rule>()
                    .Select(x => x.Action).OfType<BindTemplateSlotRemediationAction>()
                    .Count(x => x.Over != null);
                if (slot.Value.Node.Occurrence is RemediationStructuralOccurrence.ZeroOrMore or RemediationStructuralOccurrence.OneOrMore)
                {
                    if (composite && groupBindings != 1)
                    {
                        errors.Add($"Repeating composite slot '{slot.Key}' requires exactly one BindOver producer.");
                    }
                }
                else if (composite && groupBindings > 1)
                {
                    errors.Add($"Singular composite slot '{slot.Key}' may have at most one BindOver producer.");
                }
            }
        }

        foreach (var slot in slots)
        {
            var bound = ruleSets.SelectMany(x => x.Rules).Count(x => x.Slot == slot.Key);
            if (bound > 1 && slot.Value.Node.Occurrence is
                RemediationStructuralOccurrence.ExactlyOne or RemediationStructuralOccurrence.Optional)
            {
                errors.Add($"Slot '{slot.Key}' at '{slot.Value.Path}' is singular but is bound by {bound} rules.");
            }
        }
        return errors;
    }

    private static void ValidateNode(
        RemediationStructuralTemplateNode node,
        string path,
        RemediationStructuralTemplateNode? parent,
        Dictionary<string, (RemediationStructuralTemplateNode, string)> slots,
        IReadOnlySet<string> boundSlots,
        List<string> errors,
        bool prescriptive = false)
    {
        if (prescriptive && node != null && parent != null && node.Id == null)
        {
            errors.Add($"Prescriptive template node '{path}' requires a slot id.");
        }
        if (!Enum.IsDefined(typeof(RemediationStructuralOccurrence), node.Occurrence))
        {
            errors.Add($"Template node '{path}' has an invalid occurrence.");
        }
        if (!StandardTags.Contains(node.Tag))
        {
            errors.Add($"Template node '{path}' uses non-standard structure tag '{node.Tag}'.");
        }
        if (node.Id != null)
        {
            if (string.IsNullOrWhiteSpace(node.Id))
            {
                errors.Add($"Template node '{path}' has an empty id.");
            }
            else if (!slots.TryAdd(node.Id, (node, path)))
            {
                errors.Add($"Structural-template id '{node.Id}' is duplicated.");
            }
        }
        if (parent != null && !LegalChild(parent.Tag, node.Tag))
        {
            errors.Add($"Template nesting '{parent.Tag}/{node.Tag}' at '{path}' is not legal.");
        }

        for (var i = 0; i < node.Children.Count; i++)
        {
            var child = node.Children[i];
            var childPath = RemediationStructuralTemplateMatcher.ExpectedPath(path, node.Children, i);
            ValidateNode(child, childPath, node, slots, boundSlots, errors, prescriptive);
            for (var j = i + 1; j < node.Children.Count; j++)
            {
                var other = node.Children[j];
                if (ParticleKey(child, boundSlots) == ParticleKey(other, boundSlots) &&
                    CanBeEmptyBetween(node.Children, i + 1, j) &&
                    child.Occurrence is not RemediationStructuralOccurrence.ExactlyOne)
                {
                    errors.Add(
                        $"Template child sequence is ambiguous between '{childPath}' and " +
                        $"'{RemediationStructuralTemplateMatcher.ExpectedPath(path, node.Children, j)}'.");
                }
            }
        }
    }

    private static bool IsSpecializedTableInterior(RemediationStructuralTemplateNode root, string slot)
    {
        bool Find(RemediationStructuralTemplateNode node, bool insideTable)
        {
            if (node.Id == slot) return insideTable;
            var childInsideTable = insideTable || node.Tag == "Table";
            return node.Children.Any(child => Find(child, childInsideTable));
        }
        return Find(root, false);
    }

    private static bool CanBeEmptyBetween(IReadOnlyList<RemediationStructuralTemplateNode> nodes, int from, int to)
    {
        for (var i = from; i < to; i++)
        {
            if (nodes[i].Occurrence is RemediationStructuralOccurrence.ExactlyOne or RemediationStructuralOccurrence.OneOrMore)
                return false;
        }
        return true;
    }

    private static string ParticleKey(
        RemediationStructuralTemplateNode node,
        IReadOnlySet<string> boundSlots) =>
        node.Id != null && boundSlots.Contains(node.Id)
            ? $"{node.Tag}\0{node.Id}"
            : node.Tag;

    private static bool IsStructureProducing(RemediationAction action) => action switch
    {
        TagRemediationAction or GroupRemediationAction or MergeRemediationAction or
        TableRemediationAction or AdoptAnnotationRemediationAction or BindTemplateSlotRemediationAction or
        StructureLinkRemediationAction => true,
        _ => false
    };

    internal static string? ProducedTag(RemediationAction action) => action switch
    {
        TagRemediationAction x => x.Name.Value,
        GroupRemediationAction x => x.ParentTag.Value,
        MergeRemediationAction x => x.TargetTag.Value,
        TableRemediationAction => "Table",
        StructureLinkRemediationAction => "Link",
        _ => null
    };

    internal static bool LegalChild(string parent, string child) => parent switch
    {
        "Table" => child is "TR" or "THead" or "TBody" or "TFoot" or "Caption",
        "TR" => child is "TH" or "TD",
        "THead" or "TBody" or "TFoot" => child == "TR",
        "L" => child == "LI",
        "LI" => child is "Lbl" or "LBody",
        _ when child == "TR" => false,
        _ when child is "TH" or "TD" => false,
        _ when child == "LI" => false,
        _ when child is "Lbl" or "LBody" => false,
        _ => true
    };
}
