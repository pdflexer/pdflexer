using System.Text.Json;
using PdfLexer.Content;

namespace PdfLexer.Remediation;

/// <summary>
/// Loads the JSON representation accepted by the pdfctl remediation command.
/// </summary>
public static class SerializedRemediationRules
{
    public const string CurrentSchema = "pdflexer.remediation.ruleset.v1";

    public static SerializedRemediationJob Load(string path)
    {
        using var stream = File.OpenRead(path);
        return Load(stream, path);
    }

    public static SerializedRemediationJob Load(Stream stream, string? sourceName = null)
    {
        using var doc = JsonDocument.Parse(stream, new JsonDocumentOptions
        {
            AllowTrailingCommas = true,
            CommentHandling = JsonCommentHandling.Skip
        });

        var root = doc.RootElement;
        var schema = root.OptionalString("schema") ?? CurrentSchema;
        if (!string.Equals(schema, CurrentSchema, StringComparison.Ordinal))
        {
            throw new InvalidDataException($"Unsupported remediation rule schema '{schema}' in {sourceName ?? "rule stream"}.");
        }

        var session = ParseSession(root.OptionalObject("session"));
        var ruleSetId = root.OptionalObject("ruleSet")?.OptionalString("id") ?? root.OptionalString("id") ?? "remediation-rules";
        var anchors = root.OptionalArray("anchors").Select(ParseAnchor).ToArray();
        var zones = root.OptionalArray("zones").Select(ParseZone).ToArray();
        var flows = root.OptionalArray("flowRegions").Select(ParseFlowRegion).ToArray();
        var rules = root.RequiredArray("rules").Select(ParseRule).ToArray();

        return new SerializedRemediationJob(session, new RuleSet(ruleSetId, rules, anchors, zones, flows));
    }

    private static RemediationSessionConfiguration ParseSession(JsonElement? json)
    {
        if (json == null)
        {
            return new RemediationSessionConfiguration();
        }

        var el = json.Value;
        return new RemediationSessionConfiguration
        {
            Language = el.OptionalString("language") ?? "en-US",
            Title = el.OptionalString("title") ?? "Remediated Document",
            Profile = el.OptionalEnum("profile", PdfUaProfile.PdfUa1),
            StrictConformance = el.OptionalBool("strictConformance") ?? true,
            LeftoverPolicy = el.OptionalEnum("leftoverPolicy", RemediationLeftoverPolicy.Flag),
            DiagnosticStrictness = el.OptionalEnum("diagnosticStrictness", RemediationDiagnosticStrictness.Strict),
            DebugWrite = el.OptionalBool("debugWrite") ?? false,
            DefaultConfidence = el.OptionalDouble("defaultConfidence") ?? 0,
            NamedZoneMargins = ParseMargins(el.OptionalObject("namedZoneMargins"))
        };
    }

    private static RemediationNamedZoneMargins ParseMargins(JsonElement? json)
    {
        if (json == null)
        {
            return new RemediationNamedZoneMargins();
        }

        var el = json.Value;
        return new RemediationNamedZoneMargins
        {
            Header = el.OptionalDouble("header") ?? 72,
            Footer = el.OptionalDouble("footer") ?? 72,
            Left = el.OptionalDouble("left") ?? 72,
            Right = el.OptionalDouble("right") ?? 72
        };
    }

    private static Rule ParseRule(JsonElement json)
    {
        return new Rule(
            json.RequiredString("id"),
            ParseAction(json.RequiredObject("action")),
            json.OptionalObject("predicate") is { } predicate ? ParsePredicate(predicate) : null,
            json.OptionalEnum("granularity", Granularity.Paragraph),
            json.OptionalObject("pages") is { } pages ? ParsePages(pages) : PageSelector.Every,
            json.OptionalEnum("stage", Stage.Classify),
            json.OptionalBool("override") ?? false,
            json.OptionalDouble("minConfidence"));
    }

    private static RemediationAction ParseAction(JsonElement json)
    {
        var kind = json.RequiredString("kind").Token();
        return kind switch
        {
            "tag" => RemediationActions.Tag(json.RequiredString("tag")),
            "artifact" => RemediationActions.Artifact(json.OptionalEnum("subtype", ArtifactSubtype.Layout)),
            "table" => new TableRemediationAction(
                json.OptionalDoubleArray("columns"),
                json.OptionalInt("headerRows") ?? 0,
                json.OptionalObject("over") is { } over ? ParseClaimPredicate(over) : null,
                json.OptionalObject("headerSelector") is { } header ? ParseClaimPredicate(header) : null,
                json.OptionalEnum("cellContentMode", TableCellContentMode.PreserveChildren)),
            "group" => RemediationActions.Group(json.RequiredString("tag"), ParseClaimPredicate(json.RequiredObject("over"))),
            "merge" => RemediationActions.MergeTo(json.RequiredString("tag"), ParseClaimPredicate(json.RequiredObject("over"))),
            "lang" => RemediationActions.Lang(ParseClaimPredicate(json.RequiredObject("over")), json.RequiredString("language")),
            "alt" => RemediationActions.Alt(ParseClaimPredicate(json.RequiredObject("over")), json.RequiredString("text")),
            "actualtext" => RemediationActions.ActualText(ParseClaimPredicate(json.RequiredObject("over")), json.RequiredString("text")),
            "expansion" => RemediationActions.Expansion(ParseClaimPredicate(json.RequiredObject("over")), json.RequiredString("text")),
            "reordersiblings" => RemediationActions.ReorderSiblings(
                json.OptionalObject("over") is { } reorderOver ? ParseClaimPredicate(reorderOver) : ClaimPredicate.Always,
                json.OptionalEnum("mode", SiblingReorderMode.ReadingOrder)),
            "link" => RemediationActions.Link(
                ParseClaimPredicate(json.RequiredObject("source")),
                ParseClaimPredicate(json.RequiredObject("target")),
                json.RequiredString("accessibleDescription")),
            _ => throw new InvalidDataException($"Unsupported remediation action kind '{kind}'.")
        };
    }

    private static RemediationPredicate ParsePredicate(JsonElement json)
    {
        var kind = json.RequiredString("kind").Token();
        return kind switch
        {
            "always" => RemediationPredicate.Always,
            "never" => RemediationPredicate.Never,
            "all" => CombinePredicates(json.RequiredArray("predicates"), true),
            "any" => CombinePredicates(json.RequiredArray("predicates"), false),
            "not" => ParsePredicate(json.RequiredObject("predicate")).Not(),
            "textmatches" => Predicates.Text.Matches(json.RequiredString("pattern")),
            "textcontains" => Predicates.Text.Contains(json.RequiredString("text"), ParseComparison(json)),
            "textstartswith" => Predicates.Text.StartsWith(json.RequiredString("text"), ParseComparison(json)),
            "textequals" => Predicates.Text.Equals(json.RequiredString("text"), ParseComparison(json)),
            "fontsize" => Predicates.Font.Size(json.OptionalEnum("operator", NumericOperator.Equal), json.RequiredDouble("value")),
            "fontweight" => Predicates.Font.Weight(json.OptionalEnum("operator", NumericOperator.Equal), json.RequiredDouble("value")),
            "fontfamily" => Predicates.Font.Family(json.RequiredString("value")),
            "fontitalic" => Predicates.Font.Italic(json.OptionalBool("value") ?? true),
            "colorgrayish" => Predicates.Color.IsGrayish(),
            "geocontains" => Predicates.Geo.Contains(ParseLayout(json.RequiredObject("coord"))),
            "geoin" => Predicates.Geo.In(ParseLayout(json.RequiredObject("coord"))),
            "geointersects" => Predicates.Geo.Intersects(ParseLayout(json.RequiredObject("coord"))),
            "flowinzone" => Predicates.Flow.InZone(json.RequiredString("id")),
            "flowinregion" => Predicates.Flow.InFlowRegion(json.RequiredString("id")),
            "flowfirstin" => Predicates.Flow.FirstIn(json.RequiredString("id"), json.OptionalObject("where") is { } firstWhere ? ParsePredicate(firstWhere) : null),
            "flowlastin" => Predicates.Flow.LastIn(json.RequiredString("id"), json.OptionalObject("where") is { } lastWhere ? ParsePredicate(lastWhere) : null),
            "flownthin" => Predicates.Flow.NthIn(json.RequiredString("id"), json.RequiredInt("index"), json.OptionalObject("where") is { } nthWhere ? ParsePredicate(nthWhere) : null),
            "flowfirstafter" => Predicates.Flow.FirstAfter(json.RequiredString("id"), json.OptionalObject("where") is { } afterWhere ? ParsePredicate(afterWhere) : null),
            "anchorrightof" => Predicates.Anchor.RightOf(json.RequiredString("id"), json.OptionalDouble("tolerance"), json.OptionalDouble("maxDistance")),
            "anchorbelow" => Predicates.Anchor.Below(json.RequiredString("id"), json.OptionalDouble("tolerance"), json.OptionalDouble("maxDistance")),
            "anchorbetween" => Predicates.Anchor.Between(json.RequiredString("id"), json.RequiredString("id2")),
            "anchorsamerowas" => Predicates.Anchor.SameRowAs(json.RequiredString("id"), json.OptionalDouble("tolerance")),
            "anchorsamecolumnas" => Predicates.Anchor.SameColumnAs(json.RequiredString("id"), json.OptionalDouble("tolerance")),
            "anchornearestto" => Predicates.Anchor.NearestTo(json.RequiredString("id"), json.OptionalEnum("direction", AnchorDirection.Any), json.OptionalDouble("maxDistance")),
            "anchorinflowafter" => Predicates.Anchor.InFlowAfter(json.RequiredString("id")),
            "relafter" => Predicates.Relational.After(json.RequiredString("ruleId")),
            "relbefore" => Predicates.Relational.Before(json.RequiredString("ruleId")),
            "relinsideclaimof" => Predicates.Relational.InsideClaimOf(json.RequiredString("ruleId")),
            "relnthchildofclaim" => Predicates.Relational.NthChildOfClaim(json.RequiredString("ruleId"), json.RequiredInt("index")),
            _ => throw new InvalidDataException($"Unsupported remediation predicate kind '{kind}'.")
        };
    }

    private static ClaimPredicate ParseClaimPredicate(JsonElement json)
    {
        var kind = json.RequiredString("kind").Token();
        return kind switch
        {
            "always" => ClaimPredicate.Always,
            "never" => ClaimPredicate.Never,
            "all" => CombineClaimPredicates(json.RequiredArray("predicates"), true),
            "any" => CombineClaimPredicates(json.RequiredArray("predicates"), false),
            "not" => ParseClaimPredicate(json.RequiredObject("predicate")).Not(),
            "claimis" => ClaimPredicates.ClaimIs(json.RequiredString("tag")),
            "actionis" => ClaimPredicates.ActionIs(json.OptionalEnum<RemediationActionKind>("action")),
            "fromrule" => ClaimPredicates.FromRule(json.RequiredString("ruleId")),
            "fromruleset" => ClaimPredicates.FromRuleSet(json.RequiredString("ruleSetId")),
            "statusis" => ClaimPredicates.StatusIs(json.OptionalEnum<ClaimStatus>("status")),
            "samepage" => ClaimPredicates.SamePage(),
            "consecutive" => ClaimPredicates.Consecutive(),
            "within" => json.OptionalObject("coord") is { } coord
                ? ClaimPredicates.Within(ParseLayout(coord))
                : ClaimPredicates.Within(json.RequiredString("id")),
            "beforeclaim" => ClaimPredicates.BeforeClaim(json.RequiredString("ruleId")),
            "afterclaim" => ClaimPredicates.AfterClaim(json.RequiredString("ruleId")),
            _ => throw new InvalidDataException($"Unsupported claim predicate kind '{kind}'.")
        };
    }

    private static RemediationAnchor ParseAnchor(JsonElement json)
    {
        var id = json.RequiredString("id");
        var anchor = json.RequiredString("kind").Token() switch
        {
            "selector" => RemediationAnchor.Selector(
                id,
                json.OptionalArray("granularities").Any()
                    ? json.OptionalArray("granularities").Select(x => ParseEnum<Granularity>(x.GetString()!)).ToArray()
                    : new[] { json.OptionalEnum("granularity", Granularity.Line) },
                ParsePredicate(json.RequiredObject("predicate")),
                json.OptionalObject("selection") is { } selection ? ParseAnchorSelection(selection) : AnchorSelection.RequiredSingle),
            "textlabel" => RemediationAnchor.TextLabel(id, json.RequiredString("text"), ParseComparison(json)),
            "regex" => RemediationAnchor.Regex(
                id,
                json.RequiredString("pattern"),
                json.OptionalEnum("granularity", Granularity.Line),
                json.OptionalObject("selection") is { } regexSelection ? ParseAnchorSelection(regexSelection) : AnchorSelection.RequiredSingle),
            "priorclaim" => RemediationAnchor.PriorClaim(id, json.RequiredString("ruleId")),
            "declaredzone" => RemediationAnchor.DeclaredZone(id, json.OptionalEnum<NamedLayoutZone>("zone")),
            "tableheader" => RemediationAnchor.TableHeader(id, json.RequiredArray("headers").Select(x => x.GetString() ?? "").ToArray()),
            "repeatedelement" => RemediationAnchor.RepeatedElement(id, json.RequiredString("pattern")),
            "geometry" => RemediationAnchor.Geometry(id, ParseRect(json.RequiredObject("bounds"))),
            var kind => throw new InvalidDataException($"Unsupported anchor kind '{kind}'.")
        };

        return anchor with
        {
            Pages = json.OptionalObject("pages") is { } pages ? ParsePages(pages) : PageSelector.Every,
            Occurrence = json.OptionalInt("occurrence"),
            NeighborText = json.OptionalString("neighborText"),
            NeighborTolerance = json.OptionalDouble("neighborTolerance") ?? 24,
            Style = json.OptionalObject("style") is { } style ? ParsePredicate(style) : null
        };
    }

    private static TolerancedZone ParseZone(JsonElement json) => new(
        json.RequiredString("id"),
        ParseLayout(json.RequiredObject("bounds")),
        json.OptionalDouble("tolerance") ?? 0,
        json.OptionalEnum("confidenceBehavior", ZoneConfidenceBehavior.DegradeOutsideBaseBounds));

    private static FlowRegion ParseFlowRegion(JsonElement json) => new(
        json.RequiredString("id"),
        ParseFlowBoundary(json.RequiredObject("start")),
        ParseFlowBoundary(json.RequiredObject("end")),
        json.OptionalDouble("maxExtent"),
        json.OptionalEnum("continuationPolicy", FlowContinuationPolicy.CurrentPageOnly),
        json.OptionalEnum("readingOrderMode", FlowReadingOrderMode.StructuredText));

    private static FlowBoundary ParseFlowBoundary(JsonElement json)
    {
        return json.RequiredString("kind").Token() switch
        {
            "anchor" => FlowBoundary.Anchor(json.RequiredString("id")),
            "zone" => FlowBoundary.Zone(json.RequiredString("id")),
            "matching" => FlowBoundary.Matching(ParsePredicate(json.RequiredObject("predicate"))),
            "pageboundary" => FlowBoundary.PageBoundary,
            var kind => throw new InvalidDataException($"Unsupported flow boundary kind '{kind}'.")
        };
    }

    private static LayoutCoord ParseLayout(JsonElement json)
    {
        return json.RequiredString("kind").Token() switch
        {
            "absolute" => LayoutCoord.Absolute(ParseRect(json.RequiredObject("rect"))),
            "marginrelative" => LayoutCoord.MarginRelative(json.OptionalDouble("top"), json.OptionalDouble("right"), json.OptionalDouble("bottom"), json.OptionalDouble("left")),
            "percentage" => LayoutCoord.Percentage(json.OptionalDouble("top"), json.OptionalDouble("right"), json.OptionalDouble("bottom"), json.OptionalDouble("left")),
            "zone" => LayoutCoord.Zone(json.OptionalEnum<NamedLayoutZone>("zone")),
            "anchor" => LayoutCoord.Anchor(json.RequiredString("ruleId"), json.OptionalObject("expand") is { } exp ? ParseExpansion(exp) : null),
            "namedanchor" => LayoutCoord.NamedAnchor(json.RequiredString("id"), json.OptionalObject("expand") is { } namedExp ? ParseExpansion(namedExp) : null),
            "betweenanchors" => LayoutCoord.BetweenAnchors(json.RequiredString("id"), json.RequiredString("id2"), json.OptionalDouble("padding") ?? 0),
            "tolerancedzone" => LayoutCoord.TolerancedZone(json.RequiredString("id")),
            "flowregion" => LayoutCoord.FlowRegion(json.RequiredString("id")),
            var kind => throw new InvalidDataException($"Unsupported layout coordinate kind '{kind}'.")
        };
    }

    private static LayoutCoordExpansion ParseExpansion(JsonElement json)
    {
        var amount = json.RequiredDouble("amount");
        return json.RequiredString("kind").Token() switch
        {
            "inflate" => LayoutCoordExpansion.Inflate(amount),
            "above" => LayoutCoordExpansion.Above(amount),
            "below" => LayoutCoordExpansion.Below(amount),
            var kind => throw new InvalidDataException($"Unsupported layout expansion kind '{kind}'.")
        };
    }

    private static PageSelector ParsePages(JsonElement json)
    {
        return json.RequiredString("kind").Token() switch
        {
            "every" => PageSelector.Every,
            "first" => PageSelector.First,
            "last" => PageSelector.Last,
            "range" => PageSelector.Range(json.RequiredInt("from"), json.RequiredInt("to")),
            "parity" => PageSelector.Parity(json.OptionalEnum<PageParity>("parity")),
            var kind => throw new InvalidDataException($"Unsupported page selector kind '{kind}'.")
        };
    }

    private static AnchorSelection ParseAnchorSelection(JsonElement json)
    {
        return json.RequiredString("kind").Token() switch
        {
            "requiredsingle" => AnchorSelection.RequiredSingle,
            "optionalsingle" => AnchorSelection.OptionalSingle,
            "firstinreadingorder" => AnchorSelection.FirstInReadingOrder,
            "lastinreadingorder" => AnchorSelection.LastInReadingOrder,
            "nthinreadingorder" => AnchorSelection.NthInReadingOrder(json.RequiredInt("index")),
            "nearesttoanchor" => AnchorSelection.NearestToAnchor(
                json.RequiredString("id"),
                json.OptionalEnum("direction", AnchorDirection.Any),
                json.OptionalDouble("maxDistance")),
            var kind => throw new InvalidDataException($"Unsupported anchor selection kind '{kind}'.")
        };
    }

    private static RemediationPredicate CombinePredicates(IEnumerable<JsonElement> json, bool and)
    {
        var predicates = json.Select(ParsePredicate).ToList();
        if (predicates.Count == 0)
        {
            return and ? RemediationPredicate.Always : RemediationPredicate.Never;
        }

        return predicates.Skip(1).Aggregate(predicates[0], (current, next) => and ? current.And(next) : current.Or(next));
    }

    private static ClaimPredicate CombineClaimPredicates(IEnumerable<JsonElement> json, bool and)
    {
        var predicates = json.Select(ParseClaimPredicate).ToList();
        if (predicates.Count == 0)
        {
            return and ? ClaimPredicate.Always : ClaimPredicate.Never;
        }

        return predicates.Skip(1).Aggregate(predicates[0], (current, next) => and ? current.And(next) : current.Or(next));
    }

    private static PdfRect<double> ParseRect(JsonElement json) => new(
        json.RequiredDouble("llx"),
        json.RequiredDouble("lly"),
        json.RequiredDouble("urx"),
        json.RequiredDouble("ury"));

    private static StringComparison ParseComparison(JsonElement json) =>
        json.OptionalString("comparison")?.Token() switch
        {
            null => StringComparison.Ordinal,
            "ordinalignorecase" => StringComparison.OrdinalIgnoreCase,
            "invariantculture" => StringComparison.InvariantCulture,
            "invariantcultureignorecase" => StringComparison.InvariantCultureIgnoreCase,
            "currentculture" => StringComparison.CurrentCulture,
            "currentcultureignorecase" => StringComparison.CurrentCultureIgnoreCase,
            _ => StringComparison.Ordinal
        };

    private static T ParseEnum<T>(string value) where T : struct, Enum =>
        Enum.TryParse<T>(value.Replace("-", "", StringComparison.Ordinal).Replace("_", "", StringComparison.Ordinal), true, out var parsed)
            ? parsed
            : throw new InvalidDataException($"Unsupported {typeof(T).Name} value '{value}'.");

    private static string Token(this string value) =>
        value.Replace("-", "", StringComparison.Ordinal).Replace("_", "", StringComparison.Ordinal).ToLowerInvariant();

    private static JsonElement? OptionalObject(this JsonElement json, string name) =>
        json.TryGet(name, out var value) && value.ValueKind == JsonValueKind.Object ? value : null;

    private static IEnumerable<JsonElement> OptionalArray(this JsonElement json, string name) =>
        json.TryGet(name, out var value) && value.ValueKind == JsonValueKind.Array ? value.EnumerateArray() : Array.Empty<JsonElement>();

    private static IEnumerable<JsonElement> RequiredArray(this JsonElement json, string name) =>
        json.TryGet(name, out var value) && value.ValueKind == JsonValueKind.Array
            ? value.EnumerateArray()
            : throw new InvalidDataException($"Property '{name}' must be an array.");

    private static JsonElement RequiredObject(this JsonElement json, string name) =>
        json.OptionalObject(name) ?? throw new InvalidDataException($"Property '{name}' must be an object.");

    private static string RequiredString(this JsonElement json, string name) =>
        json.OptionalString(name) ?? throw new InvalidDataException($"Property '{name}' is required.");

    private static string? OptionalString(this JsonElement json, string name) =>
        json.TryGet(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;

    private static int RequiredInt(this JsonElement json, string name) =>
        json.OptionalInt(name) ?? throw new InvalidDataException($"Property '{name}' is required.");

    private static int? OptionalInt(this JsonElement json, string name) =>
        json.TryGet(name, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var parsed) ? parsed : null;

    private static double RequiredDouble(this JsonElement json, string name) =>
        json.OptionalDouble(name) ?? throw new InvalidDataException($"Property '{name}' is required.");

    private static double? OptionalDouble(this JsonElement json, string name) =>
        json.TryGet(name, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out var parsed) ? parsed : null;

    private static IReadOnlyList<double>? OptionalDoubleArray(this JsonElement json, string name) =>
        json.TryGet(name, out var value) && value.ValueKind == JsonValueKind.Array
            ? value.EnumerateArray().Select(x => x.GetDouble()).ToArray()
            : null;

    private static bool? OptionalBool(this JsonElement json, string name) =>
        json.TryGet(name, out var value) && (value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False)
            ? value.GetBoolean()
            : null;

    private static T OptionalEnum<T>(this JsonElement json, string name, T fallback) where T : struct, Enum =>
        json.OptionalString(name) is { } value ? ParseEnum<T>(value) : fallback;

    private static T OptionalEnum<T>(this JsonElement json, string name) where T : struct, Enum =>
        json.OptionalString(name) is { } value
            ? ParseEnum<T>(value)
            : throw new InvalidDataException($"Property '{name}' is required.");

    private static bool TryGet(this JsonElement json, string name, out JsonElement value)
    {
        foreach (var prop in json.EnumerateObject())
        {
            if (string.Equals(prop.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                value = prop.Value;
                return true;
            }
        }

        value = default;
        return false;
    }
}

public sealed record SerializedRemediationJob(
    RemediationSessionConfiguration Session,
    RuleSet RuleSet);
