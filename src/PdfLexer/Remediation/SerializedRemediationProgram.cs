using System.Text.Json;
using PdfLexer.Content;

namespace PdfLexer.Remediation;

/// <summary>JSON loader and writer for the greenfield prescriptive program language.</summary>
public static partial class SerializedRemediationProgram
{
    public const string CurrentSchema = "pdflexer.remediation.program.preview1";

    /// <summary>Returns whether a JSON file declares the prescriptive program schema.</summary>
    public static bool IsProgram(string path)
    {
        using var stream = File.OpenRead(path);
        using var document = JsonDocument.Parse(stream, new JsonDocumentOptions
        {
            AllowTrailingCommas = true,
            CommentHandling = JsonCommentHandling.Skip
        });
        return document.RootElement.TryGetProperty("schema", out var schema) &&
            schema.ValueKind == JsonValueKind.String &&
            schema.GetString()?.StartsWith("pdflexer.remediation.program.", StringComparison.Ordinal) == true;
    }

    public static RemediationProgram Load(string path)
    {
        using var stream = File.OpenRead(path);
        return Load(stream, path);
    }

    public static RemediationProgram Load(Stream stream, string? sourceName = null)
    {
        using var document = JsonDocument.Parse(stream, new JsonDocumentOptions
        {
            AllowTrailingCommas = true,
            CommentHandling = JsonCommentHandling.Skip
        });
        var root = document.RootElement;
        RejectUnknown(root, "schema", "id", "textNormalization", "template", "bindings", "anchors", "artifacts", "assertions", "boundaries", "fragments", "regions", "regionAccounting");
        var schema = RequiredString(root, "schema");
        if (!string.Equals(schema, CurrentSchema, StringComparison.Ordinal))
        {
            var migration = string.Equals(schema, "pdflexer.remediation.program.v1", StringComparison.Ordinal)
                ? " The unshipped v1 name was replaced by pdflexer.remediation.program.preview1 while identity contracts are finalized."
                : string.Empty;
            throw new InvalidDataException($"Unsupported remediation program schema '{schema}' in {sourceName ?? "rule stream"}.{migration}");
        }
        foreach (var obsolete in new[] { "rules", "stage", "action", "slot", "groupPass" })
            if (Has(root, obsolete)) throw new InvalidDataException($"Property '{obsolete}' belongs to the removed staged rule language.");

        var templateJson = RequiredObject(root, "template");
        RejectUnknown(templateJson, "id", "version", "profile", "document");
        var template = new RemediationTemplate(
            String(templateJson, "id") ?? throw Missing(templateJson, "id"),
            String(templateJson, "version") ?? throw Missing(templateJson, "version"),
            EnumValue<PdfUaProfile>(templateJson, "profile") ?? throw Missing(templateJson, "profile"),
            ParseNode(RequiredObject(templateJson, "document")));

        var artifacts = Array(root, "artifacts").Select(ParseArtifact).ToArray();
        var anchors = Array(root, "anchors").Select(ParseAnchor).ToArray();
        var bindings = Array(root, "bindings").Select(x => ParseBinding(x, artifacts)).ToArray();
        var assertions = Array(root, "assertions").Select(ParseAssertion).ToArray();
        var boundaries = Array(root, "boundaries").Select(ParseBoundary).ToArray();
        var fragments = Array(root, "fragments").Select(ParseFragment).ToArray();
        var regions = Array(root, "regions").Select(ParseRegion).ToArray();
        var regionAccounting = Array(root, "regionAccounting").Select(ParseRegionAccounting).ToArray();
        var program = new RemediationProgram(
            String(root, "id") ?? throw Missing(root, "id"),
            template,
            bindings,
            anchors,
            artifacts,
            assertions,
            ParseTextNormalization(OptionalObject(root, "textNormalization")),
            boundaries,
            fragments,
            regions,
            regionAccounting);
        return program;
    }

    public static void Save(RemediationProgram program, Stream stream, bool indented = true)
    {
        ArgumentNullException.ThrowIfNull(program);
        ArgumentNullException.ThrowIfNull(stream);
        var compiled = RemediationProgramCompiler.Compile(program);
        if (!compiled.IsValid) throw new InvalidDataException(string.Join(Environment.NewLine, compiled.Errors));
        ValidateSerializable(program);
        using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = indented });
        writer.WriteStartObject();
        writer.WriteString("schema", CurrentSchema);
        writer.WriteString("id", program.Id);
        WriteTextNormalization(writer, program.TextNormalization);
        writer.WriteStartObject("template");
        writer.WriteString("id", program.Template.Id);
        writer.WriteString("version", program.Template.Version);
        writer.WriteString("profile", program.Template.Profile.ToString());
        writer.WritePropertyName("document");
        WriteNode(writer, program.Template.Document);
        writer.WriteEndObject();
        writer.WriteStartArray("bindings");
        foreach (var binding in program.Bindings) WriteBinding(writer, binding);
        writer.WriteEndArray();
        writer.WriteStartArray("anchors");
        foreach (var anchor in program.Anchors) WriteAnchor(writer, anchor);
        writer.WriteEndArray();
        writer.WriteStartArray("artifacts");
        foreach (var artifact in program.Artifacts) WriteArtifact(writer, artifact);
        writer.WriteEndArray();
        writer.WriteStartArray("regions");
        foreach (var region in program.Regions) WriteRegion(writer, region);
        writer.WriteEndArray();
        writer.WriteStartArray("regionAccounting");
        foreach (var accounting in program.RegionAccounting) WriteRegionAccounting(writer, accounting);
        writer.WriteEndArray();
        writer.WriteStartArray("assertions");
        foreach (var assertion in program.Assertions)
        {
            writer.WriteStartObject();
            writer.WriteString("id", assertion.Id);
            writer.WriteString("slot", assertion.Slot.Path);
            writer.WriteNumber("minCount", assertion.Expected.Min);
            if (assertion.Expected.Max is { } max) writer.WriteNumber("maxCount", max);
            writer.WriteString("scope", assertion.Scope.ToString());
            WritePageSelector(writer, "pages", assertion.Pages ?? PageSelector.Every);
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
        writer.WriteStartArray("boundaries");
        foreach (var boundary in program.Boundaries) WriteBoundary(writer, boundary);
        writer.WriteEndArray();
        writer.WriteStartArray("fragments");
        foreach (var fragment in program.Fragments) WriteFragment(writer, fragment);
        writer.WriteEndArray();
        writer.WriteEndObject();
        writer.Flush();
    }

    private static RemediationTemplateNode ParseNode(JsonElement json)
    {
        RejectUnknown(json, "tag", "name", "children", "occurrence", "language", "alt", "actualText", "expansion", "orderPolicy", "startsOn");
        var startsOn = OptionalObject(json, "startsOn") is { } boundary
            ? ParseBoundaryReference(boundary)
            : null;
        return new(
            RequiredString(json, "tag"),
            String(json, "name"),
            children: null,
            EnumValue<TemplateOccurrence>(json, "occurrence") ?? TemplateOccurrence.ExactlyOne,
            new RemediationNodeProperties(
                String(json, "language"), String(json, "alt"), String(json, "actualText"), String(json, "expansion")),
            EnumValue<TemplateOrderPolicy>(json, "orderPolicy") ?? TemplateOrderPolicy.RequireSourceAgreement,
            startsOn,
            Array(json, "children").Select(ParseParticle).ToArray());
    }

    private static RemediationTemplateParticle ParseParticle(JsonElement json)
    {
        if (String(json, "kind")?.Equals("fragmentMount", StringComparison.OrdinalIgnoreCase) == true)
        {
            RejectUnknown(json, "kind", "fragment", "alias", "occurrence", "startsOn");
            return new RemediationFragmentMount(
                RequiredString(json, "fragment"),
                RequiredString(json, "alias"),
                EnumValue<TemplateOccurrence>(json, "occurrence") ?? TemplateOccurrence.ExactlyOne,
                OptionalObject(json, "startsOn") is { } startsOn
                    ? ParseBoundaryReference(startsOn)
                    : null);
        }
        return ParseNode(json);
    }

    private static RemediationFragment ParseFragment(JsonElement json)
    {
        RejectUnknown(json, "id", "root", "bindings", "anchors", "artifacts", "assertions", "boundaries", "regions", "regionAccounting");
        var artifacts = Array(json, "artifacts").Select(ParseArtifact).ToArray();
        var regions = Array(json, "regions").Select(ParseRegion).ToArray();
        var accounting = Array(json, "regionAccounting").Select(ParseRegionAccounting).ToArray();
        return new RemediationFragment(
            RequiredString(json, "id"),
            ParseNode(RequiredObject(json, "root")),
            Array(json, "bindings").Select(x => ParseBinding(x, artifacts)).ToArray(),
            Array(json, "anchors").Select(ParseAnchor).ToArray(),
            artifacts,
            Array(json, "assertions").Select(ParseAssertion).ToArray(),
            Array(json, "boundaries").Select(ParseBoundary).ToArray(),
            regions,
            accounting);
    }

    private static OccurrenceBoundary ParseBoundaryReference(JsonElement json)
    {
        RejectUnknown(json, "kind", "slot", "boundary");
        return String(json, "kind")?.ToLowerInvariant() switch
        {
            "derived" => new OccurrenceBoundary.Derived(),
            "slot" => new OccurrenceBoundary.StartsOnSlot(SlotRef.Parse(RequiredString(json, "slot"))),
            "boundary" => new OccurrenceBoundary.StartsOnBoundary(RequiredString(json, "boundary")),
            null => throw Missing(json, "kind"),
            var kind => throw new InvalidDataException($"Unsupported occurrence boundary kind '{kind}'.")
        };
    }

    private static OccurrenceBoundaryDeclaration ParseBoundary(JsonElement json)
    {
        RejectUnknown(json, "id", "candidates", "predicate", "pages");
        var selectorJson = RequiredObject(json, "candidates");
        RejectUnknown(selectorJson, "kind", "granularity", "types");
        var selector = String(selectorJson, "kind")?.ToLowerInvariant() switch
        {
            "text" => CandidateSelector.Text(EnumValue<Granularity>(selectorJson, "granularity") ?? Granularity.Paragraph),
            "content" => CandidateSelector.Content(Array(selectorJson, "types")
                .Select(x => EnumValue<RemediationCandidateKind>(RequiredArrayString(x))
                    ?? throw new InvalidDataException("Invalid boundary candidate kind.")).ToArray()),
            _ => throw new InvalidDataException("Boundary candidates must be text or content.")
        };
        return new OccurrenceBoundaryDeclaration(
            RequiredString(json, "id"),
            selector,
            OptionalObject(json, "predicate") is { } predicate ? ParsePredicate(predicate) : null,
            ParsePageSelector(json, "pages"));
    }

    private static BindingRule ParseBinding(JsonElement json, IReadOnlyList<ArtifactDeclaration> artifacts)
    {
        RejectUnknown(json, "id", "target", "candidates", "predicate", "pages", "minConfidence", "cardinality");
        var target = RequiredObject(json, "target");
        RejectUnknown(target, "slot", "artifact");
        if (Has(target, "slot") == Has(target, "artifact"))
            throw new InvalidDataException("A binding target must contain exactly one of 'slot' or 'artifact'.");
        BindingTarget bindingTarget = String(target, "slot") is { } slot
            ? BindingTarget.ToSlot(SlotRef.Parse(slot))
            : BindingTarget.ToArtifact(String(target, "artifact") ?? throw Missing(target, "slot or artifact"));
        var selectorJson = RequiredObject(json, "candidates");
        RejectUnknown(selectorJson, "kind", "granularity", "types");
        var selector = String(selectorJson, "kind")?.ToLowerInvariant() switch
        {
            "text" => CandidateSelector.Text(EnumValue<Granularity>(selectorJson, "granularity") ?? Granularity.Paragraph),
            "content" => CandidateSelector.Content(Array(selectorJson, "types").Select(x => EnumValue<RemediationCandidateKind>(x.GetString() ?? string.Empty) ?? throw new InvalidDataException("Invalid content candidate kind.")).ToArray()),
            _ => throw new InvalidDataException("Binding candidates must be text or content.")
        };
        return new BindingRule(
            RequiredString(json, "id"), bindingTarget, selector,
            OptionalObject(json, "predicate") is { } predicate ? ParsePredicate(predicate) : null,
            ParsePageSelector(json, "pages"),
            Number(json, "minConfidence"),
            ParseCardinality(OptionalObject(json, "cardinality")));
    }

    private static ArtifactDeclaration ParseArtifact(JsonElement json)
    {
        RejectUnknown(json, "id", "type", "pages", "count", "minCount", "maxCount", "semanticSubtype", "includeBoundingBox", "attached");
        return new ArtifactDeclaration(
            RequiredString(json, "id"),
            EnumValue<ArtifactSubtype>(json, "type") ?? throw Missing(json, "type"),
            ParsePageSelector(json, "pages"),
            ParseOccurrence(json),
            EnumValue<ArtifactSemanticSubtype>(json, "semanticSubtype"),
            Bool(json, "includeBoundingBox") ?? false,
            Array(json, "attached").Select(x => EnumValue<ArtifactAttachmentEdge>(RequiredArrayString(x)) ?? throw new InvalidDataException("Invalid artifact attachment edge.")).ToArray());
    }

    private static RemediationAnchor ParseAnchor(JsonElement json)
    {
        RejectUnknown(json, "id", "kind", "slot", "text", "comparison", "selection", "pages", "occurrence", "occurrenceNumber");
        var id = RequiredString(json, "id");
        var pages = ParsePageSelector(json, "pages");
        return String(json, "slot") is { } slot
            ? new SlotAnchor(
                id,
                new SlotOccurrenceRef(
                    SlotRef.Parse(slot),
                    EnumValue<OccurrenceSelector>(json, "occurrence") ?? OccurrenceSelector.Only,
                    Int(json, "occurrenceNumber"))) { Pages = pages }
            : new TextLabelAnchor(
                id,
                RequiredString(json, "text"),
                EnumValue<StringComparison>(json, "comparison") ?? StringComparison.Ordinal,
                ParseAnchorSelection(OptionalObject(json, "selection"))) { Pages = pages };
    }

    private static SlotCountAssertion ParseAssertion(JsonElement json)
    {
        RejectUnknown(json, "id", "slot", "count", "minCount", "maxCount", "scope", "pages");
        return new SlotCountAssertion(
            RequiredString(json, "id"),
            SlotRef.Parse(RequiredString(json, "slot")),
            ParseRequiredAssertionCount(json),
            EnumValue<SemanticAssertionScope>(json, "scope") ?? SemanticAssertionScope.Document,
            ParsePageSelector(json, "pages"));
    }

    private static RemediationPredicate ParsePredicate(JsonElement json)
    {
        RejectUnknown(json, "kind", "text", "pattern", "property", "type", "value", "operator", "count",
            "number", "boolean", "mode", "coord", "relation", "id", "id2", "region", "tolerance", "direction",
            "maxDistance", "predicates", "predicate");
        var kind = String(json, "kind")?.ToLowerInvariant() ?? throw Missing(json, "kind");
        return kind switch
        {
            "always" => RemediationPredicate.Always,
            "never" => RemediationPredicate.Never,
            "textequals" => Predicates.Text.Equals(RequiredString(json, "text")),
            "textcontains" => Predicates.Text.Contains(RequiredString(json, "text")),
            "textstartswith" => Predicates.Text.StartsWith(RequiredString(json, "text")),
            "textmatches" => Predicates.Text.Matches(RequiredString(json, "pattern")),
            "content" => ParseContentPredicate(json),
            "font" => ParseFontPredicate(json),
            "geometry" => new GeometryRemediationPredicate(
                ParseLayoutCoord(RequiredObject(json, "coord")),
                EnumValue<GeometryMatchMode>(json, "mode") ?? GeometryMatchMode.Intersects),
            "anchorrelative" => new AnchorRelativeRemediationPredicate(
                EnumValue<AnchorRelativePredicateKind>(json, "relation") ?? throw Missing(json, "relation"),
                RequiredString(json, "id"),
                String(json, "id2"),
                Number(json, "tolerance"),
                EnumValue<AnchorDirection>(json, "direction") ?? AnchorDirection.Any,
                Number(json, "maxDistance")),
            "inregion" => new RegionRemediationPredicate(
                RequiredString(json, "region"),
                EnumValue<GeometryMatchMode>(json, "mode") ?? GeometryMatchMode.Contains),
            "and" => Combine(Array(json, "predicates"), true),
            "or" => Combine(Array(json, "predicates"), false),
            "not" => ParsePredicate(RequiredObject(json, "predicate")).Not(),
            _ => throw new InvalidDataException($"Unsupported program predicate '{kind}'.")
        };
    }

    private static RemediationPredicate Combine(IEnumerable<JsonElement> values, bool and)
    {
        var predicates = values.Select(ParsePredicate).ToList();
        return predicates.Count == 0
            ? (and ? RemediationPredicate.Always : RemediationPredicate.Never)
            : predicates.Skip(1).Aggregate(predicates[0], (left, right) => and ? left.And(right) : left.Or(right));
    }

    private static RemediationPredicate ParseContentPredicate(JsonElement json)
    {
        var property = EnumValue<ContentPredicateKind>(json, "property") ?? throw Missing(json, "property");
        return new ContentRemediationPredicate(
            property,
            EnumValue<RemediationCandidateKind>(json, "type"),
            String(json, "value"),
            EnumValue<NumericOperator>(json, "operator"),
            Int(json, "count"));
    }

    private static RemediationPredicate ParseFontPredicate(JsonElement json)
    {
        var property = EnumValue<FontPredicateKind>(json, "property") ?? throw Missing(json, "property");
        return property switch
        {
            FontPredicateKind.Size or FontPredicateKind.Weight => new FontRemediationPredicate(
                property,
                EnumValue<NumericOperator>(json, "operator") ?? throw Missing(json, "operator"),
                Number(json, "number") ?? throw Missing(json, "number")),
            FontPredicateKind.Family => new FontRemediationPredicate(property, RequiredString(json, "value")),
            FontPredicateKind.Italic or FontPredicateKind.ColorIsGrayish =>
                new FontRemediationPredicate(property, Bool(json, "boolean") ?? true),
            _ => throw new InvalidDataException($"Unsupported font predicate property '{property}'.")
        };
    }

    private static LayoutCoord ParseLayoutCoord(JsonElement json)
    {
        RejectUnknown(json, "kind", "llx", "lly", "urx", "ury", "top", "right", "bottom", "left",
            "zone", "id", "id2", "padding", "expand");
        return RequiredString(json, "kind").ToLowerInvariant() switch
        {
            "absolute" => LayoutCoord.Absolute(new PdfRect<double>(
                Number(json, "llx") ?? throw Missing(json, "llx"),
                Number(json, "lly") ?? throw Missing(json, "lly"),
                Number(json, "urx") ?? throw Missing(json, "urx"),
                Number(json, "ury") ?? throw Missing(json, "ury"))),
            "margin" => LayoutCoord.MarginRelative(Number(json, "top"), Number(json, "right"), Number(json, "bottom"), Number(json, "left")),
            "percentage" => LayoutCoord.Percentage(Number(json, "top"), Number(json, "right"), Number(json, "bottom"), Number(json, "left")),
            "zone" => LayoutCoord.Zone(EnumValue<NamedLayoutZone>(json, "zone") ?? throw Missing(json, "zone")),
            "namedanchor" => LayoutCoord.NamedAnchor(RequiredString(json, "id"), ParseExpansion(OptionalObject(json, "expand"))),
            "betweenanchors" => LayoutCoord.BetweenAnchors(RequiredString(json, "id"), RequiredString(json, "id2"), Number(json, "padding") ?? 0),
            var kind => throw new InvalidDataException($"Unsupported preview layout coordinate '{kind}'.")
        };
    }

    private static LayoutCoordExpansion? ParseExpansion(JsonElement? json)
    {
        if (json is not { } value) return null;
        RejectUnknown(value, "kind", "amount");
        return new LayoutCoordExpansion(
            EnumValue<LayoutCoordExpansionKind>(value, "kind") ?? throw Missing(value, "kind"),
            Number(value, "amount") ?? throw Missing(value, "amount"));
    }

    private static void WriteNode(Utf8JsonWriter writer, RemediationTemplateNode node)
    {
        writer.WriteStartObject();
        writer.WriteString("tag", node.Tag);
        if (node.Name != null) writer.WriteString("name", node.Name);
        if (node.Occurrence != TemplateOccurrence.ExactlyOne) writer.WriteString("occurrence", node.Occurrence.ToString());
        if (node.OrderPolicy != TemplateOrderPolicy.RequireSourceAgreement) writer.WriteString("orderPolicy", node.OrderPolicy.ToString());
        if (node.Properties.Language != null) writer.WriteString("language", node.Properties.Language);
        if (node.Properties.AlternateText != null) writer.WriteString("alt", node.Properties.AlternateText);
        if (node.Properties.ActualText != null) writer.WriteString("actualText", node.Properties.ActualText);
        if (node.Properties.Expansion != null) writer.WriteString("expansion", node.Properties.Expansion);
        if (node.OccurrenceBoundary is { } boundary)
        {
            writer.WriteStartObject("startsOn");
            switch (boundary)
            {
                case OccurrenceBoundary.Derived:
                    writer.WriteString("kind", "derived");
                    break;
                case OccurrenceBoundary.StartsOnSlot slot:
                    writer.WriteString("kind", "slot");
                    writer.WriteString("slot", slot.Reference.Path);
                    break;
                case OccurrenceBoundary.StartsOnBoundary named:
                    writer.WriteString("kind", "boundary");
                    writer.WriteString("boundary", named.Id);
                    break;
            }
            writer.WriteEndObject();
        }
        writer.WriteStartArray("children");
        foreach (var child in node.Particles)
        {
            if (child is RemediationTemplateNode direct)
            {
                WriteNode(writer, direct);
                continue;
            }
            var mount = (RemediationFragmentMount)child;
            writer.WriteStartObject();
            writer.WriteString("kind", "fragmentMount");
            writer.WriteString("fragment", mount.FragmentId);
            writer.WriteString("alias", mount.Alias);
            if (mount.Occurrence != TemplateOccurrence.ExactlyOne)
                writer.WriteString("occurrence", mount.Occurrence.ToString());
            if (mount.OccurrenceBoundary is { } startsOn)
            {
                writer.WriteStartObject("startsOn");
                WriteBoundaryReference(writer, startsOn);
                writer.WriteEndObject();
            }
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    private static void WriteBoundaryReference(Utf8JsonWriter writer, OccurrenceBoundary boundary)
    {
        switch (boundary)
        {
            case OccurrenceBoundary.Derived:
                writer.WriteString("kind", "derived");
                break;
            case OccurrenceBoundary.StartsOnSlot slot:
                writer.WriteString("kind", "slot");
                writer.WriteString("slot", slot.Reference.Path);
                break;
            case OccurrenceBoundary.StartsOnBoundary named:
                writer.WriteString("kind", "boundary");
                writer.WriteString("boundary", named.Id);
                break;
        }
    }

    private static void WriteFragment(Utf8JsonWriter writer, RemediationFragment fragment)
    {
        writer.WriteStartObject();
        writer.WriteString("id", fragment.Id);
        writer.WritePropertyName("root");
        WriteNode(writer, fragment.Root);
        writer.WriteStartArray("bindings");
        foreach (var binding in fragment.Bindings) WriteBinding(writer, binding);
        writer.WriteEndArray();
        writer.WriteStartArray("anchors");
        foreach (var anchor in fragment.Anchors) WriteAnchor(writer, anchor);
        writer.WriteEndArray();
        writer.WriteStartArray("artifacts");
        foreach (var artifact in fragment.Artifacts) WriteArtifact(writer, artifact);
        writer.WriteEndArray();
        writer.WriteStartArray("regions");
        foreach (var region in fragment.Regions) WriteRegion(writer, region);
        writer.WriteEndArray();
        writer.WriteStartArray("regionAccounting");
        foreach (var accounting in fragment.RegionAccounting) WriteRegionAccounting(writer, accounting);
        writer.WriteEndArray();
        writer.WriteStartArray("assertions");
        foreach (var assertion in fragment.Assertions)
        {
            writer.WriteStartObject();
            writer.WriteString("id", assertion.Id);
            writer.WriteString("slot", assertion.Slot.Path);
            writer.WriteNumber("minCount", assertion.Expected.Min);
            if (assertion.Expected.Max is { } max) writer.WriteNumber("maxCount", max);
            writer.WriteString("scope", assertion.Scope.ToString());
            WritePageSelector(writer, "pages", assertion.Pages);
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
        writer.WriteStartArray("boundaries");
        foreach (var boundary in fragment.Boundaries) WriteBoundary(writer, boundary);
        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    private static void WriteBinding(Utf8JsonWriter writer, BindingRule binding)
    {
        writer.WriteStartObject();
        writer.WriteString("id", binding.Id);
        writer.WriteStartObject("target");
        if (binding.Target is BindingTarget.Slot slot) writer.WriteString("slot", slot.Reference.Path);
        else writer.WriteString("artifact", ((BindingTarget.Artifact)binding.Target).Id);
        writer.WriteEndObject();
        writer.WriteStartObject("candidates");
        switch (binding.Candidates)
        {
            case CandidateSelector.TextSelector text:
                writer.WriteString("kind", "text");
                writer.WriteString("granularity", text.Granularity.ToString());
                break;
            case CandidateSelector.ContentSelector content:
                writer.WriteString("kind", "content");
                writer.WriteStartArray("types");
                foreach (var kind in content.Kinds.OrderBy(x => x.ToString(), StringComparer.Ordinal)) writer.WriteStringValue(kind.ToString());
                writer.WriteEndArray();
                break;
        }
        writer.WriteEndObject();
        WritePageSelector(writer, "pages", binding.Pages);
        if (binding.MinConfidence is { } confidence) writer.WriteNumber("minConfidence", confidence);
        if (binding.Cardinality is { } cardinality)
        {
            writer.WriteStartObject("cardinality");
            writer.WriteNumber("minMatches", cardinality.MinMatches);
            if (cardinality.MaxMatches is { } max) writer.WriteNumber("maxMatches", max);
            writer.WriteString("scope", cardinality.Scope.ToString());
            writer.WriteEndObject();
        }
        WritePredicate(writer, binding.Predicate);
        writer.WriteEndObject();
    }

    private static void WritePredicate(Utf8JsonWriter writer, RemediationPredicate predicate)
    {
        writer.WritePropertyName("predicate");
        WritePredicateValue(writer, predicate);
    }

    private static void WritePredicateValue(Utf8JsonWriter writer, RemediationPredicate predicate)
    {
        writer.WriteStartObject();
        switch (predicate)
        {
            case ConstantRemediationPredicate constant: writer.WriteString("kind", constant.Value ? "always" : "never"); break;
            case TextRemediationPredicate text:
                writer.WriteString("kind", text.Kind switch { TextPredicateKind.Equals => "textEquals", TextPredicateKind.Contains => "textContains", TextPredicateKind.StartsWith => "textStartsWith", _ => "textMatches" });
                writer.WriteString(text.Kind == TextPredicateKind.Matches ? "pattern" : "text", text.Value);
                break;
            case CompositeRemediationPredicate composite:
                writer.WriteString("kind", composite.Kind == CompositePredicateKind.And ? "and" : "or");
                writer.WriteStartArray("predicates");
                WritePredicateValue(writer, composite.Left);
                WritePredicateValue(writer, composite.Right);
                writer.WriteEndArray();
                break;
            case NotRemediationPredicate not:
                writer.WriteString("kind", "not");
                WritePredicate(writer, not.Inner);
                break;
            case ContentRemediationPredicate content:
                writer.WriteString("kind", "content");
                writer.WriteString("property", content.Kind.ToString());
                if (content.ExpectedType is { } type) writer.WriteString("type", type.ToString());
                if (content.ExpectedText != null) writer.WriteString("value", content.ExpectedText);
                if (content.Operator is { } op) writer.WriteString("operator", op.ToString());
                if (content.ExpectedCount is { } count) writer.WriteNumber("count", count);
                break;
            case FontRemediationPredicate font:
                writer.WriteString("kind", "font");
                writer.WriteString("property", font.Kind.ToString());
                if (font.Operator is { } fontOp) writer.WriteString("operator", fontOp.ToString());
                if (font.NumericValue is { } number) writer.WriteNumber("number", number);
                if (font.TextValue != null) writer.WriteString("value", font.TextValue);
                if (font.BooleanValue is { } boolean) writer.WriteBoolean("boolean", boolean);
                break;
            case GeometryRemediationPredicate geometry:
                writer.WriteString("kind", "geometry");
                writer.WriteString("mode", geometry.Mode.ToString());
                writer.WritePropertyName("coord");
                WriteLayoutCoord(writer, geometry.Coord);
                break;
            case AnchorRelativeRemediationPredicate anchor:
                writer.WriteString("kind", "anchorRelative");
                writer.WriteString("relation", anchor.Kind.ToString());
                writer.WriteString("id", anchor.AnchorId);
                if (anchor.AnchorId2 != null) writer.WriteString("id2", anchor.AnchorId2);
                if (anchor.Tolerance is { } tolerance) writer.WriteNumber("tolerance", tolerance);
                if (anchor.Direction != AnchorDirection.Any) writer.WriteString("direction", anchor.Direction.ToString());
                if (anchor.MaxDistance is { } distance) writer.WriteNumber("maxDistance", distance);
                break;
            case RegionRemediationPredicate region:
                writer.WriteString("kind", "inRegion");
                writer.WriteString("region", region.RegionId);
                if (region.Mode != GeometryMatchMode.Contains) writer.WriteString("mode", region.Mode.ToString());
                break;
            default: throw new InvalidOperationException($"Predicate '{predicate.GetType().Name}' is not serializable by the first program language.");
        }
        writer.WriteEndObject();
    }

    private static void WriteLayoutCoord(Utf8JsonWriter writer, LayoutCoord coord)
    {
        writer.WriteStartObject();
        switch (coord)
        {
            case AbsoluteLayoutCoord absolute:
                writer.WriteString("kind", "absolute");
                writer.WriteNumber("llx", absolute.Rect.LLx); writer.WriteNumber("lly", absolute.Rect.LLy);
                writer.WriteNumber("urx", absolute.Rect.URx); writer.WriteNumber("ury", absolute.Rect.URy);
                break;
            case MarginRelativeLayoutCoord margin:
                writer.WriteString("kind", "margin"); WriteOffsets(writer, margin.Top, margin.Right, margin.Bottom, margin.Left); break;
            case PercentageLayoutCoord percentage:
                writer.WriteString("kind", "percentage"); WriteOffsets(writer, percentage.Top, percentage.Right, percentage.Bottom, percentage.Left); break;
            case NamedZoneLayoutCoord zone:
                writer.WriteString("kind", "zone"); writer.WriteString("zone", zone.NamedZone.ToString()); break;
            case NamedAnchorLayoutCoord anchor:
                writer.WriteString("kind", "namedAnchor"); writer.WriteString("id", anchor.AnchorId); WriteExpansion(writer, anchor.Expand); break;
            case BetweenAnchorsLayoutCoord between:
                writer.WriteString("kind", "betweenAnchors"); writer.WriteString("id", between.AnchorA);
                writer.WriteString("id2", between.AnchorB); if (between.Padding != 0) writer.WriteNumber("padding", between.Padding); break;
            default: throw new InvalidDataException($"Layout coordinate '{coord.GetType().Name}' is not serializable.");
        }
        writer.WriteEndObject();

        static void WriteOffsets(Utf8JsonWriter writer, double? top, double? right, double? bottom, double? left)
        {
            if (top is { } t) writer.WriteNumber("top", t); if (right is { } r) writer.WriteNumber("right", r);
            if (bottom is { } b) writer.WriteNumber("bottom", b); if (left is { } l) writer.WriteNumber("left", l);
        }
        static void WriteExpansion(Utf8JsonWriter writer, LayoutCoordExpansion? expansion)
        {
            if (expansion == null) return;
            writer.WriteStartObject("expand"); writer.WriteString("kind", expansion.Kind.ToString());
            writer.WriteNumber("amount", expansion.Amount); writer.WriteEndObject();
        }
    }

    private static void WriteAnchor(Utf8JsonWriter writer, RemediationAnchor anchor)
    {
        writer.WriteStartObject();
        writer.WriteString("id", anchor.Id);
        if (anchor is SlotAnchor slot)
        {
            writer.WriteString("slot", slot.Slot.Path);
            if (slot.OccurrenceSelector != OccurrenceSelector.Only)
                writer.WriteString("occurrence", slot.OccurrenceSelector.ToString());
            if (slot.OccurrenceNumber is { } number)
                writer.WriteNumber("occurrenceNumber", number);
        }
        else if (anchor is TextLabelAnchor text)
        {
            writer.WriteString("kind", "text");
            writer.WriteString("text", text.Text);
            if (text.Comparison != StringComparison.Ordinal) writer.WriteString("comparison", text.Comparison.ToString());
            WriteAnchorSelection(writer, text.Selection);
        }
        else throw new InvalidOperationException($"Anchor '{anchor.GetType().Name}' is not serializable by the first program language.");
        WritePageSelector(writer, "pages", anchor.Pages);
        writer.WriteEndObject();
    }

    private static void WriteBoundary(Utf8JsonWriter writer, OccurrenceBoundaryDeclaration boundary)
    {
        writer.WriteStartObject();
        writer.WriteString("id", boundary.Id);
        writer.WriteStartObject("candidates");
        switch (boundary.Candidates)
        {
            case CandidateSelector.TextSelector text:
                writer.WriteString("kind", "text");
                writer.WriteString("granularity", text.Granularity.ToString());
                break;
            case CandidateSelector.ContentSelector content:
                writer.WriteString("kind", "content");
                writer.WriteStartArray("types");
                foreach (var kind in content.Kinds.OrderBy(x => x.ToString(), StringComparer.Ordinal))
                    writer.WriteStringValue(kind.ToString());
                writer.WriteEndArray();
                break;
        }
        writer.WriteEndObject();
        WritePageSelector(writer, "pages", boundary.Pages);
        WritePredicate(writer, boundary.Predicate);
        writer.WriteEndObject();
    }

    private static AnchorSelection ParseAnchorSelection(JsonElement? json)
    {
        if (json is not { } value) return AnchorSelection.RequiredSingle;
        RejectUnknown(value, "mode", "index", "anchor", "direction", "maxDistance");
        return new AnchorSelection(
            EnumValue<AnchorSelectionMode>(value, "mode") ?? throw Missing(value, "mode"),
            Int(value, "index"),
            String(value, "anchor"),
            EnumValue<AnchorDirection>(value, "direction") ?? AnchorDirection.Any,
            Number(value, "maxDistance"));
    }

    private static void WriteAnchorSelection(Utf8JsonWriter writer, AnchorSelection selection)
    {
        if (selection == AnchorSelection.RequiredSingle) return;
        writer.WriteStartObject("selection");
        writer.WriteString("mode", selection.Mode.ToString());
        if (selection.Index is { } index) writer.WriteNumber("index", index);
        if (selection.AnchorId != null) writer.WriteString("anchor", selection.AnchorId);
        if (selection.Direction != AnchorDirection.Any) writer.WriteString("direction", selection.Direction.ToString());
        if (selection.MaxDistance is { } distance) writer.WriteNumber("maxDistance", distance);
        writer.WriteEndObject();
    }

    private static void WriteArtifact(Utf8JsonWriter writer, ArtifactDeclaration artifact)
    {
        writer.WriteStartObject();
        writer.WriteString("id", artifact.Id);
        writer.WriteString("type", artifact.Subtype.ToString());
        if (artifact.SemanticSubtype is { } semantic) writer.WriteString("semanticSubtype", semantic.ToString());
        writer.WriteBoolean("includeBoundingBox", artifact.IncludeBoundingBox);
        if (artifact.Attached.Count > 0)
        {
            writer.WriteStartArray("attached");
            foreach (var edge in artifact.Attached) writer.WriteStringValue(edge.ToString());
            writer.WriteEndArray();
        }
        writer.WriteNumber("minCount", artifact.Occurrence.Min);
        if (artifact.Occurrence.Max is { } max) writer.WriteNumber("maxCount", max);
        WritePageSelector(writer, "pages", artifact.Pages);
        writer.WriteEndObject();
    }

    private static void WritePageSelector(Utf8JsonWriter writer, string property, PageSelector selector)
    {
        if (selector is EveryPageSelector) return;
        writer.WriteStartObject(property);
        switch (selector)
        {
            case FirstPageSelector: writer.WriteString("kind", "first"); break;
            case LastPageSelector: writer.WriteString("kind", "last"); break;
            case PageRangeSelector range:
                writer.WriteString("kind", "range");
                writer.WriteNumber("from", range.FromInclusive);
                writer.WriteNumber("to", range.ToInclusive);
                break;
            case PageParitySelector parity:
                writer.WriteString("kind", "parity");
                writer.WriteString("value", parity.PageParity.ToString());
                break;
            default: throw new InvalidOperationException($"Page selector '{selector.GetType().Name}' is not serializable.");
        }
        writer.WriteEndObject();
    }

    private static void WriteTextNormalization(Utf8JsonWriter writer, TextNormalizationOptions options)
    {
        writer.WriteStartObject("textNormalization");
        writer.WriteBoolean("compatibilityComposition", options.CompatibilityComposition);
        writer.WriteBoolean("removeSoftHyphens", options.RemoveSoftHyphens);
        writer.WriteBoolean("normalizeWhitespace", options.NormalizeWhitespace);
        writer.WriteBoolean("foldDashes", options.FoldDashes);
        writer.WriteBoolean("foldQuotes", options.FoldQuotes);
        writer.WriteEndObject();
    }

    private static PageSelector ParsePageSelector(JsonElement json, string property) =>
        OptionalObject(json, property) is not { } pages ? PageSelector.Every :
        ParsePageSelectorValue(pages);

    private static PageSelector ParsePageSelectorValue(JsonElement pages)
    {
        RejectUnknown(pages, "kind", "from", "to", "value");
        return String(pages, "kind")?.ToLowerInvariant() switch
        {
            "first" => PageSelector.First,
            "last" => PageSelector.Last,
            "range" => PageSelector.Range(Int(pages, "from") ?? 0, Int(pages, "to") ?? 0),
            "parity" => PageSelector.Parity(EnumValue<PageParity>(pages, "value") ?? throw Missing(pages, "value")),
            null => throw Missing(pages, "kind"),
            var kind => throw new InvalidDataException($"Unsupported page selector kind '{kind}'.")
        };
    }

    private static AssertionCount ParseRequiredAssertionCount(JsonElement json)
    {
        var exact = Int(json, "count");
        var min = Int(json, "minCount");
        var max = Int(json, "maxCount");
        if (exact == null && min == null && max == null)
            throw new InvalidDataException("An assertion requires 'count' or explicit count bounds.");
        if (exact != null && (min != null || max != null))
            throw new InvalidDataException("Assertion 'count' conflicts with minCount/maxCount.");
        return exact is { } value ? AssertionCount.Exactly(value) : new AssertionCount(min ?? 0, max);
    }

    private static void ValidateSerializable(RemediationProgram program)
    {
        ValidateNode(program.Template.Document);
        ValidateDeclarations(program.Anchors, program.Bindings, program.Boundaries);
        foreach (var fragment in program.Fragments)
        {
            ValidateNode(fragment.Root);
            ValidateDeclarations(fragment.Anchors, fragment.Bindings, fragment.Boundaries);
        }

        static void ValidateDeclarations(
            IReadOnlyList<RemediationAnchor> anchors,
            IReadOnlyList<BindingRule> bindings,
            IReadOnlyList<OccurrenceBoundaryDeclaration> boundaries)
        {
        foreach (var anchor in anchors)
            if (anchor is not SlotAnchor and not TextLabelAnchor)
                throw new InvalidDataException($"Anchor '{anchor.GetType().Name}' is not supported by {CurrentSchema}.");
        foreach (var binding in bindings)
        {
            if (binding.Candidates is not CandidateSelector.TextSelector and not CandidateSelector.ContentSelector)
                throw new InvalidDataException($"Candidate selector '{binding.Candidates.GetType().Name}' is not supported by {CurrentSchema}.");
            ValidatePredicate(binding.Predicate);
        }
        foreach (var boundary in boundaries)
        {
            if (boundary.Candidates is not CandidateSelector.TextSelector and not CandidateSelector.ContentSelector)
                throw new InvalidDataException($"Boundary selector '{boundary.Candidates.GetType().Name}' is not supported by {CurrentSchema}.");
            ValidatePredicate(boundary.Predicate);
        }
        }
    }

    private static void ValidateNode(RemediationTemplateNode node)
    {
        foreach (var child in node.Particles)
            if (child is RemediationTemplateNode direct)
                ValidateNode(direct);
    }

    private static void ValidatePredicate(RemediationPredicate predicate)
    {
        switch (predicate)
        {
            case ConstantRemediationPredicate:
            case TextRemediationPredicate:
            case ContentRemediationPredicate:
            case FontRemediationPredicate:
            case AnchorRelativeRemediationPredicate:
                return;
            case GeometryRemediationPredicate geometry when geometry.Coord is
                AbsoluteLayoutCoord or MarginRelativeLayoutCoord or PercentageLayoutCoord or
                NamedZoneLayoutCoord or NamedAnchorLayoutCoord or BetweenAnchorsLayoutCoord:
            case RegionRemediationPredicate:
                return;
            case CompositeRemediationPredicate composite:
                ValidatePredicate(composite.Left); ValidatePredicate(composite.Right); return;
            case NotRemediationPredicate not:
                ValidatePredicate(not.Inner); return;
            default:
                throw new InvalidDataException($"Predicate '{predicate.GetType().Name}' is not supported by {CurrentSchema} serialization.");
        }
    }

    private static AssertionCount? ParseOccurrence(JsonElement json)
    {
        var min = Int(json, "minCount") ?? Int(json, "count");
        var max = Int(json, "maxCount") ?? Int(json, "count");
        return min == null && max == null ? null : new AssertionCount(min ?? 0, max);
    }

    private static TextNormalizationOptions ParseTextNormalization(JsonElement? json)
    {
        if (json is not { } value) return TextNormalizationOptions.Default;
        RejectUnknown(value, "compatibilityComposition", "removeSoftHyphens", "normalizeWhitespace", "foldDashes", "foldQuotes");
        return new TextNormalizationOptions
        {
            CompatibilityComposition = Bool(value, "compatibilityComposition") ?? true,
            RemoveSoftHyphens = Bool(value, "removeSoftHyphens") ?? true,
            NormalizeWhitespace = Bool(value, "normalizeWhitespace") ?? true,
            FoldDashes = Bool(value, "foldDashes") ?? true,
            FoldQuotes = Bool(value, "foldQuotes") ?? true
        };
    }

    private static BindingCardinality? ParseCardinality(JsonElement? json)
    {
        if (json is not { } value) return null;
        RejectUnknown(value, "minMatches", "maxMatches", "scope");
        return new BindingCardinality(Int(value, "minMatches") ?? 0, Int(value, "maxMatches"),
            EnumValue<BindingCardinalityScope>(value, "scope") ?? BindingCardinalityScope.Document);
    }

    private static bool Has(JsonElement json, string name) => json.TryGetProperty(name, out _);
    private static void RejectUnknown(JsonElement json, params string[] allowed)
    {
        var names = allowed.ToHashSet(StringComparer.Ordinal);
        foreach (var property in json.EnumerateObject())
            if (!names.Contains(property.Name))
                throw new InvalidDataException($"Unknown property '{property.Name}'.");
    }
    private static JsonElement RequiredObject(JsonElement json, string name) => OptionalObject(json, name) ?? throw Missing(json, name);
    private static JsonElement? OptionalObject(JsonElement json, string name)
    {
        if (!json.TryGetProperty(name, out var value)) return null;
        if (value.ValueKind != JsonValueKind.Object) throw WrongKind(name, "object", value.ValueKind);
        return value;
    }
    private static IEnumerable<JsonElement> Array(JsonElement json, string name)
    {
        if (!json.TryGetProperty(name, out var value)) return System.Array.Empty<JsonElement>();
        if (value.ValueKind != JsonValueKind.Array) throw WrongKind(name, "array", value.ValueKind);
        return value.EnumerateArray();
    }
    private static string RequiredString(JsonElement json, string name) => String(json, name) ?? throw Missing(json, name);
    private static string? String(JsonElement json, string name)
    {
        if (!json.TryGetProperty(name, out var value)) return null;
        if (value.ValueKind != JsonValueKind.String) throw WrongKind(name, "string", value.ValueKind);
        return value.GetString();
    }
    private static int? Int(JsonElement json, string name)
    {
        if (!json.TryGetProperty(name, out var value)) return null;
        if (!value.TryGetInt32(out var parsed)) throw WrongKind(name, "integer", value.ValueKind);
        return parsed;
    }
    private static double? Number(JsonElement json, string name)
    {
        if (!json.TryGetProperty(name, out var value)) return null;
        if (!value.TryGetDouble(out var parsed)) throw WrongKind(name, "number", value.ValueKind);
        return parsed;
    }
    private static bool? Bool(JsonElement json, string name)
    {
        if (!json.TryGetProperty(name, out var value)) return null;
        if (value.ValueKind is not (JsonValueKind.True or JsonValueKind.False)) throw WrongKind(name, "boolean", value.ValueKind);
        return value.GetBoolean();
    }
    private static T? EnumValue<T>(JsonElement json, string name) where T : struct, Enum => String(json, name) is { } value ? Enum.TryParse<T>(value, true, out var parsed) ? parsed : throw new InvalidDataException($"Unsupported {typeof(T).Name} value '{value}'.") : null;
    private static T? EnumValue<T>(string value) where T : struct, Enum => Enum.TryParse<T>(value, true, out var parsed) ? parsed : null;
    private static InvalidDataException Missing(JsonElement json, string name) => new($"Property '{name}' is required.");
    private static InvalidDataException WrongKind(string name, string expected, JsonValueKind actual) =>
        new($"Property '{name}' must be a {expected}, not {actual}.");
    private static string RequiredArrayString(JsonElement value) => value.ValueKind == JsonValueKind.String
        ? value.GetString()!
        : throw WrongKind("array item", "string", value.ValueKind);
}
