# Rule-Based PDF Remediation

Rule-based remediation lets callers add a new accessibility structure tree to existing rendered PDFs that are still untagged. It is intended for known transactional document families such as invoices, statements, notices, reports, and exports where the layout repeats but field values, row counts, optional sections, and page sizes can vary.

This is not an automatic tagging system. The caller writes deterministic template rules; `PdfLexer` evaluates those rules, wraps existing content in marked-content scopes, builds structure nodes, and reports diagnostics.

## Supported Workflow

Use remediation only for untagged documents. Documents with an existing `StructTreeRoot` are rejected.

```csharp
using PdfLexer;
using PdfLexer.Remediation;

using var doc = PdfDocument.Open(inputBytes);
using var session = doc.BeginRemediation(new RemediationSessionConfiguration
{
    Language = "en-US",
    Title = "Remediated Invoice",
    Profile = PdfUaProfile.PdfUa1,
    StrictConformance = true,                          // default; see Session Configuration
    LeftoverPolicy = RemediationLeftoverPolicy.FailFast,
    DebugWrite = true
});

session.Use(BuildInvoiceRules());

var report = session.DryRun();
if (report.Diagnostics.Count == 0)
{
    session.Commit();
}
```

`DryRun()` evaluates rules and returns claims/diagnostics without mutating the document. `Commit()` reevaluates, applies accessibility setup, writes marked content, builds the structure tree, and runs integrity diagnostics.

> [!IMPORTANT]
> Because `Commit()` reevaluates, a clean dry run is not a guarantee of a clean commit. The relationship between the two results, and the document's state after a failed commit, are not currently specified — see RRM-030 in the [gap tracker](rule-based-remediation-gaps.md). Treat a failed commit as requiring a reopen until that contract is defined.

Read [Before You Author Rules](#before-you-author-rules) first. Several documented behaviors have sharp edges that are easier to avoid than to debug.

## Session Configuration

`RemediationSessionConfiguration` controls the commit-time accessibility setup and the diagnostic posture.

| Property | Default | Notes |
| --- | --- | --- |
| `Language` | `"en-US"` | Written to `Catalog/Lang` and XMP. |
| `Title` | `"Remediated Document"` | Written to `Info/Title` and XMP. |
| `Profile` | `PdfUa1` | Target conformance profile. |
| `StrictConformance` | `true` | Enables the authoring integrity checks at save. This is the most common reason a commit fails — in particular it **rejects documents whose fonts are not embedded**, which the rule language cannot repair. See [What Remediation Cannot Fix](#what-remediation-cannot-fix). |
| `LeftoverPolicy` | `Flag` | What to do with unclaimed content. See the warning below. |
| `DiagnosticStrictness` | `Strict` | `Permissive` honors suppressions registered with `session.Suppress(...)`. |
| `NamedZoneMargins` | 72pt each side | Defines what `NamedLayoutZone.Header`/`Footer`/`Left`/`Right` mean. A document with a 40-point header, or a non-Letter page size, will mis-zone unless you set these. |
| `DefaultConfidence` | `0.0` | Confidence assigned to matches that do not compute one. See [Confidence](#confidence). |
| `DebugWrite` | `false` | Writes rule ids into structure element titles. |

> [!WARNING]
> **`RemediationLeftoverPolicy.AutoArtifact` can hide content.** Unclaimed content is marked as an artifact, which removes it from the structure tree and from assistive technology entirely. That is correct for decorative content and wrong for everything else — and the engine cannot tell the difference.
>
> A rule that matches nothing produces no diagnostic today (RRM-016). So if a producer-side layout change breaks one predicate, the content that rule should have tagged falls through to the leftover policy and is silently hidden. The resulting document is valid PDF/UA and passes external validation.
>
> Use `FailFast` in development **and in production** unless you have separately established that the leftover set is decorative. Prefer explicit `Artifact(...)` rules over `AutoArtifact`.

Diagnostic suppression is available but should be treated as a reviewed exception, not a workflow step:

```csharp
session.Suppress(DiagnosticCode.ReadingOrderDrift, scope: "Page3", reason: "Known sidebar; verified manually 2026-07-24");
```

Suppressions are surfaced on `RemediationReport.Suppressions`. They are only honored when `DiagnosticStrictness = Permissive`. Every suppression should carry a justification that someone has actually checked.

## Before You Author Rules

Five behaviors that are easy to get wrong and hard to diagnose.

**A rule that matches nothing is not an error.** There is no `MinMatches` or expected-cardinality on `Rule`; only anchors have that, through `AnchorSelection.RequiredSingle`. Check `report.Outcomes` for the rules you expect to fire, and prefer routing critical fields through a `RequiredSingle` anchor so that a miss fails loudly. Tracked as RRM-016.

**Anchors resolve per page, not per document.** `AnchorSelection.RequiredSingle` means "exactly one match *on this page*". A label that appears on every page is fine; the same label twice on one page fails; a label absent from an intermediate page also fails. Narrow the scope with `RemediationAnchor.Pages` when an anchor only exists on some pages:

```csharp
var anchor = RemediationAnchor.TextLabel("subtotal-label", "Subtotal") with
{
    Pages = PageSelector.Last
};
```

Tracked as RRM-028.

**Text predicates match extracted text as-is.** No normalization is documented or guaranteed. Ligatures, soft hyphens, non-breaking spaces, U+2011 non-breaking hyphens, and runs of spaces synthesized from `TJ` offsets will all defeat an otherwise-correct pattern — and by the point above, defeat it silently. Prefer `Contains` with an explicit `StringComparison` over anchored `Matches` for values you do not control. Tracked as RRM-032.

**Sibling order in the structure tree is unspecified.** Reading order is tree order, but nothing currently defines whether siblings are ordered by rule declaration, geometry, or content-stream position. If reading order matters, assert it with an explicit `ReorderSiblings` refine rule rather than relying on the default. Tracked as RRM-029.

**Confidence has no defined model.** See [Confidence](#confidence) before setting `MinConfidence` on anything.

## Rule Model

Rules live in a `RuleSet`. A rule set also carries shared anchors, toleranced zones, and flow regions.

```csharp
var ruleSet = new RuleSet(
    "invoice-v2",
    rules,
    anchors,
    tolerancedZones: zones,
    flowRegions: flows);
```

Every `Rule` has the same shape:

- `Id`: stable provenance id used in reports and debug-write titles.
- `Action`: what to do with matched content or claims.
- `Predicate`: how raw candidates are selected for classify rules.
- `Granularity`: `Character`, `Word`, `Line`, or `Paragraph`.
- `Pages`: `PageSelector.Every`, `First`, `Last`, `Range(...)`, or `Parity(...)`.
- `Stage`: `Classify`, `Group`, or `Refine`.
- `Override`: whether the rule may replace earlier claims over the same target.
- `MinConfidence`: optional hard confidence threshold.

```csharp
new Rule(
    "invoice-number",
    RemediationActions.Tag("P"),
    Predicates.Flow.FirstAfter(
        "invoice-label",
        Predicates.Text.Matches(@"^INV-\d+$")),
    Granularity.Word,
    pages: PageSelector.First);
```

## Pipeline Stages

Rules run in fixed stage order.

- `Classify`: selects raw structured-text/content candidates and creates leaf claims such as `H1`, `P`, `Span`, `TD`, or `Artifact`.
- `Group`: consumes already-applied claims with `ClaimPredicate` and builds parent structure such as `Sect`, `L`, or `Table`, or merges temporary leaf fragments into one final element with `MergeTo`.
- `Refine`: modifies existing claims by adding attributes, links, or sibling reordering.

Group and refine rules do not re-select raw content. They operate on claims created by classify rules. This avoids duplicate MCIDs and keeps parent construction tied to real structure bindings.

```csharp
var classifyAddress = new Rule(
    "bill-to-lines",
    RemediationActions.Tag("Span"),
    Predicates.Flow.InFlowRegion("bill-to-address"),
    Granularity.Line);

var groupAddress = new Rule(
    "bill-to-paragraph",
    RemediationActions.MergeTo("P", ClaimPredicates.FromRule("bill-to-lines")),
    stage: Stage.Group);

var langAddress = new Rule(
    "bill-to-lang",
    RemediationActions.Lang(ClaimPredicates.FromRule("bill-to-paragraph"), "en-US"),
    stage: Stage.Refine);
```

## Candidate Predicates

Candidate predicates are used by classify-stage rules and predicate-based anchors.

Text:

```csharp
Predicates.Text.Matches(@"^INV-\d+$");
Predicates.Text.Contains("Subtotal", StringComparison.OrdinalIgnoreCase);
Predicates.Text.StartsWith("Invoice");
Predicates.Text.Equals("Bill To");
```

Font, style, and color:

```csharp
Predicates.Font.Size(NumericOperator.GreaterThanOrEqual, 14);
Predicates.Font.Family("Helvetica");
Predicates.Font.Italic();
Predicates.Color.IsGrayish();
```

Geometry:

```csharp
Predicates.Geo.In(LayoutCoord.Zone(NamedLayoutZone.Header));
Predicates.Geo.Intersects(LayoutCoord.MarginRelative(bottom: 72));
Predicates.Geo.Contains(LayoutCoord.BetweenAnchors("bill-to-label", "ship-to-label", padding: 6));
```

Anchor-relative:

```csharp
Predicates.Anchor.RightOf("invoice-label", tolerance: 4, maxDistance: 180);
Predicates.Anchor.Below("section-heading", tolerance: 6, maxDistance: 96);
Predicates.Anchor.SameRowAs("invoice-label", tolerance: 4);
Predicates.Anchor.SameColumnAs("amount-header", tolerance: 8);
Predicates.Anchor.Between("bill-to-label", "ship-to-label");
Predicates.Anchor.NearestTo("total-label", AnchorDirection.Right, maxDistance: 160);
```

Flow and ordered dynamic fields:

```csharp
Predicates.Flow.InZone("footer");
Predicates.Flow.InFlowRegion("line-items");
Predicates.Flow.FirstIn("bill-to-address");
Predicates.Flow.NthIn("line-items", 1);
Predicates.Flow.FirstAfter("invoice-label", Predicates.Text.Matches(@"^INV-\d+$"));
```

Relational predicates over prior claims:

```csharp
Predicates.Relational.After("heading");
Predicates.Relational.Before("subtotal");
Predicates.Relational.InsideClaimOf("summary-box");
Predicates.Relational.NthChildOfClaim("paragraph", 0);
```

Predicates compose with `And`, `Or`, and `Not`.

```csharp
Predicates.Text.Matches(@"^\$[\d,.]+$")
    .And(Predicates.Anchor.RightOf("total-label", maxDistance: 120))
    .And(Predicates.Anchor.SameRowAs("total-label", tolerance: 4));
```

## Anchors

Anchors are named references that rules, layout coordinates, and flow regions can use. Prefer anchors over absolute rectangles when labels are stable but content shifts.

Predicate-based anchors are the most flexible form:

```csharp
var invoiceLabel = RemediationAnchor.Selector(
    "invoice-label",
    Granularity.Line,
    Predicates.Text.Matches(@"^Invoice\s*#$")
        .And(Predicates.Geo.In(LayoutCoord.Zone(NamedLayoutZone.Header))),
    AnchorSelection.RequiredSingle);
```

Anchors can be relative to other anchors:

```csharp
var invoiceNumber = RemediationAnchor.Selector(
    "invoice-number-anchor",
    Granularity.Word,
    Predicates.Text.Matches(@"^INV-\d+$")
        .And(Predicates.Anchor.RightOf("invoice-label", maxDistance: 180))
        .And(Predicates.Anchor.SameRowAs("invoice-label", tolerance: 4)),
    AnchorSelection.RequiredSingle);
```

Selection modes make ambiguity handling explicit:

- `AnchorSelection.RequiredSingle`: exactly one match is required.
- `AnchorSelection.OptionalSingle`: zero or one match is allowed.
- `AnchorSelection.FirstInReadingOrder`: choose the first match.
- `AnchorSelection.LastInReadingOrder`: choose the last match.
- `AnchorSelection.NthInReadingOrder(n)`: choose the zero-based nth match.
- `AnchorSelection.NearestToAnchor(id, direction, maxDistance)`: choose the nearest match to another anchor.

Anchors also carry three filtering properties that apply before selection: `Pages` (a `PageSelector`, defaulting to every page), `Style` (a predicate used to disambiguate candidates), and `Occurrence`.

> [!CAUTION]
> `Occurrence` is **one-based**, while `AnchorSelection.NthInReadingOrder(n)` is **zero-based**. Both pick an occurrence, and the order in which they apply relative to `Pages` and `Style` is unspecified. Use one or the other, not both. Tracked as RRM-028.

Remember that all of this resolves **per page** — see [Before You Author Rules](#before-you-author-rules).

Compatibility factories are still useful for simple templates:

```csharp
RemediationAnchor.TextLabel("subtotal-label", "Subtotal");
RemediationAnchor.TableHeader("line-items-header", "Item", "Qty", "Amount");
RemediationAnchor.Regex("invoice-label", @"^Invoice\s*#$");
RemediationAnchor.Geometry("footer-art", bounds);
RemediationAnchor.FromPriorClaim("heading-anchor", "heading-rule");
```

`TextLabel` performs exact matching, not contains matching. If common labels such as `Total` or `Date` occur multiple times, use predicate anchors with geometry, page selectors, relative predicates, or explicit selection modes.

## Layout Coordinates

`LayoutCoord` resolves to a page-relative rectangle at evaluation time.

```csharp
LayoutCoord.Absolute(rect);
LayoutCoord.MarginRelative(top: 72);
LayoutCoord.Percentage(top: 0.10, left: 0.05, right: 0.05);
LayoutCoord.Zone(NamedLayoutZone.Header);
LayoutCoord.Anchor("heading-rule", LayoutCoordExpansion.Below(72));
LayoutCoord.NamedAnchor("total-label", LayoutCoordExpansion.Inflate(4));
LayoutCoord.BetweenAnchors("bill-to-label", "ship-to-label", padding: 6);
LayoutCoord.TolerancedZone("footer");
LayoutCoord.FlowRegion("line-items");
```

Use `LayoutCoord.Anchor(ruleId, ...)` when the reference is a content claim produced by a rule. Use `LayoutCoord.NamedAnchor(anchorId, ...)` when the reference is a named anchor that may not be tagged itself.

## Toleranced Zones

Toleranced zones describe recurring page areas that may drift slightly across files or page sizes.

```csharp
var footer = new TolerancedZone(
    "footer",
    LayoutCoord.MarginRelative(bottom: 42),
    Tolerance: 6);

var footerRule = new Rule(
    "artifact-footer",
    RemediationActions.Artifact(ArtifactSubtype.Pagination),
    Predicates.Flow.InZone("footer"),
    Granularity.Line);
```

The base bounds come from a `LayoutCoord`; tolerance expands those bounds. Candidates outside the base bounds but inside the tolerated bounds may receive degraded confidence depending on the zone confidence behavior.

## Flow Regions

Flow regions model sections that grow or shrink, such as addresses, terms, or tables.

```csharp
var anchors = new[]
{
    RemediationAnchor.TableHeader("line-items-header", "Item", "Qty", "Amount"),
    RemediationAnchor.TextLabel("subtotal-label", "Subtotal")
};

var lineItems = new FlowRegion(
    "line-items",
    FlowBoundary.Anchor("line-items-header"),
    FlowBoundary.Anchor("subtotal-label"));

var rowRule = new Rule(
    "line-item-row",
    RemediationActions.Tag("TR"),
    Predicates.Flow.InFlowRegion("line-items"),
    Granularity.Line);
```

Boundaries can be anchors, toleranced zones, a predicate match, or the page boundary.

```csharp
new FlowRegion(
    "terms",
    FlowBoundary.Anchor("terms-heading"),
    FlowBoundary.PageBoundary,
    MaxExtent: 240);

new FlowRegion(
    "summary",
    FlowBoundary.Anchor("summary-heading"),
    FlowBoundary.Matching(Predicates.Text.StartsWith("Disclosures")));
```

Use ordered flow selectors when the task is field extraction rather than section tagging:

```csharp
Predicates.Flow.FirstIn("bill-to-address");
Predicates.Flow.LastIn("terms");
Predicates.Flow.NthIn("line-items", 0);
Predicates.Flow.FirstAfter("invoice-label", Predicates.Text.Matches(@"^INV-\d+$"));
```

## Actions

Classify actions:

```csharp
RemediationActions.Tag("H1");
RemediationActions.Tag("P");
RemediationActions.Tag("Span");
RemediationActions.Artifact(ArtifactSubtype.Pagination);
RemediationActions.Artifact(ArtifactSubtype.Layout);
```

Table actions run in the `Group` stage. They can infer columns, use explicit column boundaries, or consume already-classified claims.

```csharp
new Rule(
    "line-items-table",
    RemediationActions.TableOver(
        ClaimPredicates.FromRule("line-item-row"),
        72, 300, 380, 470),
    stage: Stage.Group);

new Rule(
    "table-with-header",
    RemediationActions.TableOver(
        ClaimPredicates.FromRule("table-cell"),
        ClaimPredicates.FromRule("table-header-cell"),
        72, 300, 380, 470),
    stage: Stage.Group);

new Rule(
    "table-with-direct-cell-content",
    RemediationActions.TableOverFlattenedCells(
        ClaimPredicates.FromRule("table-cell"),
        72, 300, 380, 470),
    stage: Stage.Group);
```

Grouping actions run in the `Group` stage and reparent existing claims.

```csharp
new Rule(
    "list",
    RemediationActions.Group(
        "L",
        ClaimPredicates.ClaimIs("LI").And(ClaimPredicates.Consecutive())),
    stage: Stage.Group);
```

Merge actions also run in the `Group` stage, but flatten matched leaf claims into a single target element instead of preserving child structure nodes. Use this for wrapped paragraphs, multi-line addresses, and plain text cells where `P -> Span per visual line` would add unnecessary structure.

```csharp
new Rule(
    "bill-to-paragraph",
    RemediationActions.MergeTo("P", ClaimPredicates.FromRule("bill-to-line")),
    stage: Stage.Group);
```

`MergeTo` only accepts plain leaf claims. If a matched claim carries semantic attributes such as language, actual text, alternate text, links, object references, or child elements, merge fails so the semantics are not silently discarded. Apply those semantics to the merged claim afterward with refine actions.

Refine actions operate over existing claims.

```csharp
new Rule(
    "address-lang",
    RemediationActions.Lang(ClaimPredicates.FromRule("bill-to-lines"), "en-US"),
    stage: Stage.Refine);

new Rule(
    "figure-alt",
    RemediationActions.Alt(ClaimPredicates.FromRule("logo"), "Company logo"),
    stage: Stage.Refine);

new Rule(
    "fix-reading-order",
    RemediationActions.ReorderSiblings(
        ClaimPredicates.ClaimIs("P"),
        SiblingReorderMode.GeometryTopToBottom),
    stage: Stage.Refine);
```

The full refine set also includes replacement and expansion text, and a raw attribute escape hatch:

```csharp
RemediationActions.ActualText(ClaimPredicates.FromRule("logo-wordmark"), "Acme");
RemediationActions.Expansion(ClaimPredicates.FromRule("acronym"), "World Wide Web Consortium");
RemediationActions.Attributes(ClaimPredicates.ClaimIs("P"), attributeDictionary);
```

`Attributes` writes a raw attribute dictionary onto the claim's structure node. It does not populate the typed properties the serializer generates from (`Scope`, `Headers`, `ListNumbering`, `Summary`), so it cannot currently be used to set those — see [What the Rule Language Cannot Express](#what-the-rule-language-cannot-express).

### Custom Actions

`RemediationActions.Custom(handler, description)` runs an arbitrary delegate against the claim context. It is a genuine escape hatch and is occasionally the only way to express something.

```csharp
new Rule(
    "special-case",
    RemediationActions.Custom(ctx => /* ... */, "describe what this does"),
    stage: Stage.Refine);
```

> [!WARNING]
> A custom action holds a delegate and therefore **cannot round-trip through the JSON rule format**. A rule set containing one is no longer portable or reviewable as data. Use it as a temporary measure and record the gap it is covering, rather than as a standing part of a template.

Links are also refine-stage actions:

```csharp
new Rule(
    "toc-link",
    RemediationActions.Link(
        source: ClaimPredicates.FromRule("toc-entry"),
        target: ClaimPredicates.FromRule("section-heading"),
        accessibleDescription: "Jump to section"),
    stage: Stage.Refine);
```

## Claim Predicates

Claim predicates select existing claim outcomes for group and refine actions.

```csharp
ClaimPredicates.ClaimIs("P");
ClaimPredicates.ActionIs(RemediationActionKind.Artifact);
ClaimPredicates.FromRule("line-item-row");
ClaimPredicates.FromRuleSet("invoice-v2");
ClaimPredicates.StatusIs(ClaimStatus.Applied);
ClaimPredicates.SamePage();
ClaimPredicates.Consecutive();
ClaimPredicates.Within("line-items");
ClaimPredicates.Within(LayoutCoord.FlowRegion("line-items"));
ClaimPredicates.BeforeClaim("subtotal");
ClaimPredicates.AfterClaim("heading");
```

Claim predicates compose with `And`, `Or`, and `Not`.

## Complete Invoice Example

```csharp
static RuleSet BuildInvoiceRules()
{
    var anchors = new[]
    {
        RemediationAnchor.Selector(
            "invoice-label",
            Granularity.Line,
            Predicates.Text.Matches(@"^Invoice\s*#$")
                .And(Predicates.Geo.In(LayoutCoord.Zone(NamedLayoutZone.Header))),
            AnchorSelection.RequiredSingle),

        RemediationAnchor.Selector(
            "invoice-number-anchor",
            Granularity.Word,
            Predicates.Text.Matches(@"^INV-\d+$")
                .And(Predicates.Anchor.RightOf("invoice-label", maxDistance: 180))
                .And(Predicates.Anchor.SameRowAs("invoice-label", tolerance: 4)),
            AnchorSelection.RequiredSingle),

        RemediationAnchor.TextLabel("bill-to-label", "Bill To"),
        RemediationAnchor.TextLabel("ship-to-label", "Ship To"),
        RemediationAnchor.TableHeader("line-items-header", "Item", "Qty", "Amount"),
        RemediationAnchor.TextLabel("subtotal-label", "Subtotal")
    };

    var zones = new[]
    {
        new TolerancedZone("footer", LayoutCoord.MarginRelative(bottom: 42), Tolerance: 6)
    };

    var flows = new[]
    {
        new FlowRegion(
            "bill-to-address",
            FlowBoundary.Anchor("bill-to-label"),
            FlowBoundary.Anchor("ship-to-label")),

        new FlowRegion(
            "line-items",
            FlowBoundary.Anchor("line-items-header"),
            FlowBoundary.Anchor("subtotal-label"))
    };

    var rules = new[]
    {
        new Rule(
            "invoice-title",
            RemediationActions.Tag("H1"),
            Predicates.Text.StartsWith("Invoice"),
            Granularity.Line,
            pages: PageSelector.First),

        new Rule(
            "invoice-number",
            RemediationActions.Tag("P"),
            Predicates.Geo.In(LayoutCoord.NamedAnchor("invoice-number-anchor", LayoutCoordExpansion.Inflate(2))),
            Granularity.Word),

        new Rule(
            "bill-to-line",
            RemediationActions.Tag("Span"),
            Predicates.Flow.InFlowRegion("bill-to-address"),
            Granularity.Line),

        // NOTE: see the warning below - this row-based shape is known to be wrong.
        new Rule(
            "line-item-row",
            RemediationActions.Tag("TR"),
            Predicates.Flow.InFlowRegion("line-items"),
            Granularity.Line),

        new Rule(
            "page-footer",
            RemediationActions.Artifact(ArtifactSubtype.Pagination),
            Predicates.Flow.InZone("footer"),
            Granularity.Line),

        new Rule(
            "line-items-table",
            RemediationActions.TableOver(ClaimPredicates.FromRule("line-item-row"), 72, 300, 380, 470),
            stage: Stage.Group),

        new Rule(
            "bill-to-paragraph",
            RemediationActions.MergeTo("P", ClaimPredicates.FromRule("bill-to-line")),
            stage: Stage.Group),

        new Rule(
            "document-lang",
            RemediationActions.Lang(ClaimPredicates.StatusIs(ClaimStatus.Applied), "en-US"),
            stage: Stage.Refine)
    };

    return new RuleSet("invoice-v2", rules, anchors, zones, flows);
}
```

> [!WARNING]
> **The table portion of this example produces an incorrect hierarchy** and is retained only until the fix lands. `line-item-row` tags each row line as `TR`, and claim-consuming table mode treats every matched claim as a *cell* — so each `TR` claim is placed beneath a generated `TD`. Classify **cell** content and pass those claims to `TableOver`, rather than pre-building rows. The internal strict validator does not currently catch this, but external validators do. Tracked as RRM-006 in the [gap tracker](rule-based-remediation-gaps.md), which owns the corrected example.

## What Remediation Cannot Fix

Remediation adds structure. It does not rewrite the content stream's text, fonts, or encodings. Several PDF/UA requirements live below the structure layer and no rule set can reach them:

- **Fonts.** The output reuses the input's fonts. A font that is not embedded, has no `ToUnicode` map, or references `.notdef` fails PDF/UA regardless of tagging quality. Because `StrictConformance` defaults to `true`, a non-embedded font will **fail the commit** with an error that does not obviously point back at the input document.
- **Word boundaries.** Producers that separate words with `TJ` offsets instead of space characters yield text that fails PDF/UA §7.2 and extracts as `InvoiceNumber`. Wrapping it in a marked-content scope does not change that.
- **Existing annotations.** Links, stamps, and other annotations already present in the input are not visible to the rule model and will not be tagged. `RemediationActions.Link` creates a *new* structure link and annotation; it does not adopt an existing one.

Check these before investing in a rule set for a new document family. Tracked as RRM-026 and RRM-019.

## What the Rule Language Cannot Express

Capabilities a rule author will reach for that do not exist yet. Each links to its tracker entry.

| Missing | Tracker |
| --- | --- |
| Expected match counts on a rule (`MinMatches`), so drift fails loudly | RRM-016 |
| A rule-set applicability guard, so the wrong template is refused | RRM-017 |
| Output assertions ("exactly one `H1`", "every row has 4 cells") | RRM-018 |
| Binding annotations already present in the input | RRM-019 |
| Selecting or excluding pre-existing marked content and optional content | RRM-020 |
| Splitting a list line into `Lbl` + `LBody`; setting `/ListNumbering` | RRM-021 |
| Artifact `/Subtype` (`Header`, `Footer`, `Watermark`), `/BBox`, `/Attached` | RRM-022 |
| Relative or normalized heading levels for optional sections | RRM-023 |
| Generating an outline from headings; setting `/PageLabels` | RRM-024 |
| Row-scoped table headers; `/Summary`; declarative `/Headers` | RRM-005, RRM-025 |
| Selecting non-text content (images, paths, form XObjects) | RRM-002 |
| Flow regions and tables that continue across a page break | RRM-001, RRM-005 |
| Group rules that consume the output of earlier group rules | RRM-004 |

## Confidence

Confidence appears in several places — `Rule.MinConfidence`, `RemediationClaimOutcome.Confidence`, `RemediationSessionConfiguration.DefaultConfidence`, toleranced-zone degradation, and low-confidence diagnostics.

> [!WARNING]
> The confidence model is currently **unspecified**: what produces a value, the scale, how `Tolerance` degrades it, and how `And`/`Or`/`Not` combine operand confidences are all undefined. `DefaultConfidence` also defaults to `0.0`, and it applies to any match that does not compute a confidence explicitly — so setting `MinConfidence` on a rule can cause it to reject every candidate. Leave `MinConfidence` unset until this is specified. Tracked as RRM-027.

## Diagnostics And Validation

Use `session.Validate(rules)` or `DryRun()` before committing.

Validation catches rule-shape problems that do not require page parsing:

- duplicate rule or anchor ids;
- invalid regexes;
- unknown anchor, zone, flow, or rule references;
- invalid action/stage combinations;
- invalid confidence thresholds;
- invalid anchor selection modes.

Dry-run and commit plan diagnostics catch page-specific problems:

- anchors that resolve to zero or multiple candidates;
- flow regions that are empty, overlapping, or have invalid boundaries;
- candidates that cannot be safely materialized into exact marked-content ranges;
- table grid mismatch or low-confidence inference;
- unclaimed content under `FailFast` or flagged leftovers;
- orphaned MCIDs, missing `/StructParents`, or reading-order drift.

When `DebugWrite = true`, rule ids are written to structure element titles to make generated PDFs easier to inspect. Disable it for production output if those titles are not desired.

### Inspecting Outcomes

`RemediationReport` carries per-claim outcomes with rule provenance, granularity, confidence, status, page, bounds, and the MCIDs bound to each structure node:

```csharp
foreach (var outcome in report.Outcomes)
{
    Console.WriteLine($"{outcome.RuleId} p{outcome.PageIndex} {outcome.Status} conf={outcome.Confidence}");
}
```

`report.SkippedOutcomes` carries the same shape for claims that were conflicted, overridden, or failed.

`Explain(...)` reports which rules considered a given piece of content, accepting a `StructuredSourceRef`, a `RemediationCandidate`, or a `StructuredCharacter`:

```csharp
foreach (var outcome in report.Explain(someCharacter))
{
    Console.WriteLine($"{outcome.RuleId} -> {outcome.Status}");
}
```

> [!NOTE]
> `Explain` answers "which rules touched this content." It cannot yet answer "why did my rule match nothing" or "which conjunct of this `And` chain rejected this candidate," and per-rule match counts are not reported for rules that produced no claims. Until that lands (RRM-033), verify that every rule you expect to fire appears in `report.Outcomes`.

## Authoring Guidance

- Start with stable anchors and flow regions, then classify content inside those regions.
- Prefer predicate-based anchors for common labels that may appear more than once.
- Use `maxDistance` for label/value fields so a nearby label does not capture unrelated content.
- Use `FirstAfter` or `FirstIn` for ordered field extraction; use `NearestTo` only when geometric nearness is the intent.
- Classify first, then group or refine claims. Do not try to build parent structure by re-selecting raw text.
- Keep rule ids stable because reports, debug output, and downstream tests depend on them.
- Use `RemediationLeftoverPolicy.FailFast` in development *and* in production. Reach for `AutoArtifact` only once you have separately established that the leftover set is decorative — a rule that silently stops matching will otherwise route real content into it.
- Route business-critical fields through a `RequiredSingle` anchor so a template change fails the run instead of degrading the output.
- Assert the rules you expect to fire against `report.Outcomes` in your own tests. The engine does not do this for you yet.
- Verify output with an external validator, and separately verify that the *right* content got the *right* tags. Conformance validation cannot tell you the invoice total was tagged as a footer.

## Current Limitations

See the [Rule-Based Remediation Gap Tracker](rule-based-remediation-gaps.md) for the full list with priorities and completion criteria, and the [Delivery Plan](rule-based-remediation-plan.md) for the milestone each one is scheduled into. The items most likely to affect a first template are RRM-016 (silent drift), RRM-026 (input conformance), RRM-032 (text normalization), and RRM-006 (the table example above).
