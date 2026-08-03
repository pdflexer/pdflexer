using System.Text.Json;
using PdfLexer.Content;

namespace PdfLexer.Remediation;

public static partial class SerializedRemediationProgram
{
    private static RegionDeclaration ParseRegion(JsonElement json)
    {
        RejectUnknown(json, "id", "pages", "expression");
        return new RegionDeclaration(
            RequiredString(json, "id"),
            ParseRegionExpression(RequiredObject(json, "expression")),
            ParsePageSelector(json, "pages"));
    }

    private static Region ParseRegionExpression(JsonElement json)
    {
        var kind = RequiredString(json, "kind").ToLowerInvariant();
        switch (kind)
        {
            case "fixed":
                RejectUnknown(json, "kind", "bounds");
                return new Region.Fixed(ParseLayoutCoord(RequiredObject(json, "bounds")));
            case "anchored":
                RejectUnknown(json, "kind", "anchor", "placement", "extent");
                return new Region.Anchored(
                    RequiredString(json, "anchor"),
                    EnumValue<RegionPlacement>(json, "placement") ?? throw Missing(json, "placement"),
                    Number(json, "extent") ?? throw Missing(json, "extent"));
            case "flow":
                RejectUnknown(json, "kind", "start", "end", "continuationPolicy", "readingOrderMode", "maxExtent", "maxPages");
                return new Region.Flow(
                    ParseRegionBoundary(RequiredObject(json, "start")),
                    ParseRegionBoundary(RequiredObject(json, "end")),
                    EnumValue<FlowContinuationPolicy>(json, "continuationPolicy") ?? FlowContinuationPolicy.CurrentPageOnly,
                    EnumValue<FlowReadingOrderMode>(json, "readingOrderMode") ?? FlowReadingOrderMode.StructuredText,
                    Number(json, "maxExtent"),
                    Int(json, "maxPages"));
            case "tolerance":
                RejectUnknown(json, "kind", "amount", "confidenceBehavior", "inner");
                return new Region.Tolerance(
                    ParseRegionExpression(RequiredObject(json, "inner")),
                    Number(json, "amount") ?? throw Missing(json, "amount"),
                    EnumValue<ZoneConfidenceBehavior>(json, "confidenceBehavior") ??
                    ZoneConfidenceBehavior.DegradeOutsideBaseBounds);
            default:
                throw new InvalidDataException($"Unsupported program region expression '{kind}'.");
        }
    }

    private static RegionBoundary ParseRegionBoundary(JsonElement json)
    {
        var kind = RequiredString(json, "kind").ToLowerInvariant();
        switch (kind)
        {
            case "anchor":
                RejectUnknown(json, "kind", "anchor");
                return new RegionBoundary.Anchor(RequiredString(json, "anchor"));
            case "region":
                RejectUnknown(json, "kind", "region");
                return new RegionBoundary.Region(RequiredString(json, "region"));
            case "matching":
                RejectUnknown(json, "kind", "candidates", "predicate");
                return new RegionBoundary.Matching(
                    ParseRegionCandidateSelector(RequiredObject(json, "candidates")),
                    OptionalObject(json, "predicate") is { } predicate ? ParsePredicate(predicate) : null);
            case "page":
                RejectUnknown(json, "kind");
                return RegionBoundary.Page.Instance;
            default:
                throw new InvalidDataException($"Unsupported program region boundary '{kind}'.");
        }
    }

    private static CandidateSelector ParseRegionCandidateSelector(JsonElement json)
    {
        RejectUnknown(json, "kind", "granularity", "types");
        return RequiredString(json, "kind").ToLowerInvariant() switch
        {
            "text" => CandidateSelector.Text(
                EnumValue<Granularity>(json, "granularity") ?? Granularity.Line),
            "content" => CandidateSelector.Content(Array(json, "types")
                .Select(x => EnumValue<RemediationCandidateKind>(x.GetString() ?? string.Empty) ??
                    throw new InvalidDataException("Invalid region boundary candidate kind."))
                .ToArray()),
            var kind => throw new InvalidDataException($"Unsupported region boundary candidate selector '{kind}'.")
        };
    }

    private static RegionArtifactAccounting ParseRegionAccounting(JsonElement json)
    {
        RejectUnknown(json, "id", "region", "artifact", "candidateKinds", "allowText", "textPredicate", "priority");
        return new RegionArtifactAccounting(
            RequiredString(json, "id"),
            RequiredString(json, "region"),
            RequiredString(json, "artifact"),
            Array(json, "candidateKinds").Select(x =>
                EnumValue<RemediationCandidateKind>(x.GetString() ?? string.Empty) ??
                throw new InvalidDataException("Invalid region accounting candidate kind.")).ToArray(),
            Bool(json, "allowText") ?? false,
            OptionalObject(json, "textPredicate") is { } predicate ? ParsePredicate(predicate) : null,
            Int(json, "priority") ?? 0);
    }

    private static void WriteRegion(Utf8JsonWriter writer, RegionDeclaration declaration)
    {
        writer.WriteStartObject();
        writer.WriteString("id", declaration.Id);
        WritePageSelector(writer, "pages", declaration.Pages);
        writer.WritePropertyName("expression");
        WriteRegionExpression(writer, declaration.Expression);
        writer.WriteEndObject();
    }

    private static void WriteRegionExpression(Utf8JsonWriter writer, Region expression)
    {
        writer.WriteStartObject();
        switch (expression)
        {
            case Region.Fixed fixedRegion:
                writer.WriteString("kind", "fixed");
                writer.WritePropertyName("bounds");
                WriteLayoutCoord(writer, fixedRegion.Bounds);
                break;
            case Region.Anchored anchored:
                writer.WriteString("kind", "anchored");
                writer.WriteString("anchor", anchored.AnchorId);
                writer.WriteString("placement", anchored.Placement.ToString());
                writer.WriteNumber("extent", anchored.Extent);
                break;
            case Region.Flow flow:
                writer.WriteString("kind", "flow");
                writer.WritePropertyName("start");
                WriteRegionBoundary(writer, flow.Start);
                writer.WritePropertyName("end");
                WriteRegionBoundary(writer, flow.End);
                if (flow.ContinuationPolicy != FlowContinuationPolicy.CurrentPageOnly)
                    writer.WriteString("continuationPolicy", flow.ContinuationPolicy.ToString());
                if (flow.ReadingOrderMode != FlowReadingOrderMode.StructuredText)
                    writer.WriteString("readingOrderMode", flow.ReadingOrderMode.ToString());
                if (flow.MaxExtent is { } extent) writer.WriteNumber("maxExtent", extent);
                if (flow.MaxPages is { } pages) writer.WriteNumber("maxPages", pages);
                break;
            case Region.Tolerance tolerance:
                writer.WriteString("kind", "tolerance");
                writer.WriteNumber("amount", tolerance.Amount);
                writer.WriteString("confidenceBehavior", tolerance.ConfidenceBehavior.ToString());
                writer.WritePropertyName("inner");
                WriteRegionExpression(writer, tolerance.Inner);
                break;
            default:
                throw new InvalidDataException($"Region expression '{expression.GetType().Name}' is not serializable.");
        }
        writer.WriteEndObject();
    }

    private static void WriteRegionBoundary(Utf8JsonWriter writer, RegionBoundary boundary)
    {
        writer.WriteStartObject();
        switch (boundary)
        {
            case RegionBoundary.Anchor anchor:
                writer.WriteString("kind", "anchor");
                writer.WriteString("anchor", anchor.AnchorId);
                break;
            case RegionBoundary.Region region:
                writer.WriteString("kind", "region");
                writer.WriteString("region", region.RegionId);
                break;
            case RegionBoundary.Matching matching:
                writer.WriteString("kind", "matching");
                writer.WritePropertyName("candidates");
                WriteRegionCandidateSelector(writer, matching.Candidates);
                writer.WritePropertyName("predicate");
                WritePredicateValue(writer, matching.Predicate);
                break;
            case RegionBoundary.Page:
                writer.WriteString("kind", "page");
                break;
            default:
                throw new InvalidDataException($"Region boundary '{boundary.GetType().Name}' is not serializable.");
        }
        writer.WriteEndObject();
    }

    private static void WriteRegionCandidateSelector(Utf8JsonWriter writer, CandidateSelector selector)
    {
        writer.WriteStartObject();
        switch (selector)
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
            default:
                throw new InvalidDataException($"Region candidate selector '{selector.GetType().Name}' is not serializable.");
        }
        writer.WriteEndObject();
    }

    private static void WriteRegionAccounting(Utf8JsonWriter writer, RegionArtifactAccounting accounting)
    {
        writer.WriteStartObject();
        writer.WriteString("id", accounting.Id);
        writer.WriteString("region", accounting.RegionId);
        writer.WriteString("artifact", accounting.ArtifactId);
        writer.WriteStartArray("candidateKinds");
        foreach (var kind in accounting.CandidateKinds.OrderBy(x => x.ToString(), StringComparer.Ordinal))
            writer.WriteStringValue(kind.ToString());
        writer.WriteEndArray();
        if (accounting.AllowText) writer.WriteBoolean("allowText", true);
        if (accounting.TextPredicate != null)
        {
            writer.WritePropertyName("textPredicate");
            WritePredicateValue(writer, accounting.TextPredicate);
        }
        if (accounting.Priority != 0) writer.WriteNumber("priority", accounting.Priority);
        writer.WriteEndObject();
    }
}
