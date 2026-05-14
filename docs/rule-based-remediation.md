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
    LeftoverPolicy = RemediationLeftoverPolicy.AutoArtifact,
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

## Authoring Guidance

- Start with stable anchors and flow regions, then classify content inside those regions.
- Prefer predicate-based anchors for common labels that may appear more than once.
- Use `maxDistance` for label/value fields so a nearby label does not capture unrelated content.
- Use `FirstAfter` or `FirstIn` for ordered field extraction; use `NearestTo` only when geometric nearness is the intent.
- Classify first, then group or refine claims. Do not try to build parent structure by re-selecting raw text.
- Keep rule ids stable because reports, debug output, and downstream tests depend on them.
- Use `RemediationLeftoverPolicy.FailFast` while developing a template and `AutoArtifact` only when the remaining content is known decorative or non-semantic.
