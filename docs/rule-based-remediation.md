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
| `DefaultConfidence` | `1.0` | Confidence assigned to matches that do not compute one. See [Confidence](#confidence). |
| `DebugWrite` | `false` | Writes rule ids into structure element titles. |

> [!WARNING]
> **`RemediationLeftoverPolicy.AutoArtifact` can hide content.** Unclaimed content is marked as an artifact, which removes it from the structure tree and from assistive technology entirely. That is correct for decorative content and wrong for everything else — and the engine cannot tell the difference.
>
> Protect every required semantic rule with `RuleCardinality`. If a producer-side layout change breaks its selector, dry-run and commit report `RuleCardinalityMismatch`, and commit stops before `AutoArtifact` can hide the content. `RemediationReport.AutoArtifacts` also lists every leftover text item that would be or was artifacted.
>
> Use `FailFast` in development **and in production** unless you have separately established that the leftover set is decorative. Prefer explicit `Artifact(...)` rules over `AutoArtifact`.
>
> **`AutoArtifact` is defensible when an artifact inventory is declared**, every item is zone-qualified, and the run reports zero undeclared artifacts. Declaring an inventory closes the artifact space: every produced artifact must match a declared item, absorbed content outside every declared zone reports `ArtifactUndeclared`, and per-page occurrence catches a zone that swallowed more than it should — an item declared `AssertionCount.Exactly(1)` that absorbs fourteen items reports `ArtifactOccurrenceViolation`. Absorbed content that matches an item is also wrapped with that item's subtype rather than a bare `/Artifact`, and `report.AutoArtifacts[].InventoryItemId` names the match.
>
> What this still does not prove: content inside declared furniture is absorbed without semantic inspection. A footer zone containing a total will hide the total, within occurrence, without complaint.

Diagnostic suppression is available but should be treated as a reviewed exception, not a workflow step:

```csharp
session.Suppress(DiagnosticCode.ReadingOrderDrift, scope: "Page3", reason: "Known sidebar; verified manually 2026-07-24");
```

Suppressions are surfaced on `RemediationReport.Suppressions`. They are only honored when `DiagnosticStrictness = Permissive`. Every suppression should carry a justification that someone has actually checked.
Cardinality diagnostics use `Rule:<id>` for document scope and `Rule:<id>:Page<n>` for page scope.

## Before You Author Rules

Five behaviors that are easy to get wrong and hard to diagnose.

**Optional rules may match nothing; required rules should declare cardinality.** Cardinality counts inputs accepted by the rule's primary selector before confidence and conflict handling. It can be evaluated across the document or independently on each selected page:

```csharp
new Rule(
    "invoice-number",
    RemediationActions.Tag("P"),
    Predicates.Text.Matches(@"^INV-\d+$"),
    CandidateSelector.Text(Granularity.Word),
    pages: PageSelector.First,
    cardinality: RuleCardinality.Exactly(1));

new Rule(
    "page-number",
    RemediationActions.Artifact(ArtifactSubtype.Pagination),
    Predicates.Flow.InZone("footer"),
    CandidateSelector.Text(Granularity.Line),
    cardinality: RuleCardinality.Exactly(1, RuleCardinalityScope.PerPage));
```

`Document` is the default scope. `PerPage` checks every existing page selected by `Rule.Pages`; a positive minimum also fails if that selector selects no existing pages.

**Anchors resolve per page, not per document.** `AnchorSelection.RequiredSingle` means "exactly one match *on this page*". A label that appears on every page is fine; the same label twice on one page fails; a label absent from an intermediate page also fails. Narrow the scope with `RemediationAnchor.Pages` when an anchor only exists on some pages:

```csharp
var anchor = RemediationAnchor.TextLabel("subtotal-label", "Subtotal") with
{
    Pages = PageSelector.Last
};
```

Document-scoped anchors are not supported: selection and ambiguity are always evaluated independently
on each selected page. Anchor diagnostics identify the page and distinguish no match from ambiguity.

**Text predicates normalize extracted text by default.** The pipeline applies Unicode NFKC, removes
soft hyphens, collapses Unicode whitespace, and folds common dash and curly-quote characters.
`Equals`, `Contains`, and `StartsWith` normalize both operands; regex predicates normalize candidate
text only. Configure the rule-set policy with `TextNormalizationOptions`, or pass an override to
`TextRemediationPredicate`. Source characters and PDF operator ranges are never rewritten.

**Sibling order follows page reading order.** Claims are ordered top-to-bottom, then left-to-right,
with stable candidate/rule/tag tie-breakers; rule declaration order does not decide sibling order.
Use `ReorderSiblings` only when the intended semantic order differs from that default.

**Confidence is in `[0,1]`.** Ordinary boolean matches are confidence-neutral and inherit
`DefaultConfidence` (`1.0`). Predicates that model tolerance or inference may lower it; `And` takes
the minimum matching confidence, `Or` uses the selected matching branch, and `Not` preserves its
operand confidence. See [Confidence](#confidence).

## Rule Model

Rules live in a `RuleSet`. A rule set also carries shared anchors, toleranced zones, flow regions,
text normalization, and semantic assertions.

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
- `Candidates`: `CandidateSelector.Text(Granularity)`, `CandidateSelector.Content(...)` for image,
  path, form, and shading invocations, or `CandidateSelector.Annotations()` for existing page
  annotations. Annotation candidates remain separate from painting content.
- `Pages`: `PageSelector.Every`, `First`, `Last`, `Range(...)`, or `Parity(...)`.
- `Cardinality`: optional expected selector-match count, using `Exactly`, `AtLeast`, `AtMost`, or `Between`.

The serialized v1 rule format accepts the same constraint additively:

```json
{
  "id": "invoice-number",
  "cardinality": {
    "scope": "document",
    "minMatches": 1,
    "maxMatches": 1
  }
}
```
- `Stage`: `Classify`, `Group`, or `Refine`.
- `Override`: whether the rule may replace earlier claims over the same target.
- `MinConfidence`: optional hard confidence threshold.
- `GroupPass`: non-negative structural depth for `Stage.Group`; omitted or `0` preserves the original single-pass behavior.

```csharp
new Rule(
    "invoice-number",
    RemediationActions.Tag("P"),
    Predicates.Flow.FirstAfter(
        "invoice-label",
        Predicates.Text.Matches(@"^INV-\d+$")),
    CandidateSelector.Text(Granularity.Word),
    pages: PageSelector.First);
```

## Pipeline Stages

Rules run in fixed stage order.

- `Classify`: selects raw structured-text/content candidates and creates leaf claims such as `H1`, `P`, `Span`, `TD`, or `Artifact`.
- `Group`: consumes already-applied claims with `ClaimPredicate` and builds parent structure such as `Sect`, `L`, or `Table`, or merges temporary leaf fragments into one final element with `MergeTo`. Group rules may declare `GroupPass` (JSON `groupPass`, default `0`); distinct passes run numerically and each pass reads one immutable frontier produced by lower passes.
- `Refine`: modifies existing claims by adding attributes, links, or sibling reordering.

Group and refine rules do not re-select raw content. Pass 0 starts with applied classify claims. When a Group, MergeTo, or claim-consuming Table output is applied, its consumed roots leave the structural frontier and the new parent enters it; unconsumed roots carry forward. Rules in the same pass cannot consume peer output, and overlapping consumers are a non-suppressible composition error. Refine runs once after the final Group pass. This avoids duplicate MCIDs and keeps parent construction tied to real structure bindings. `Override` remains Classify-only and does not resolve Group composition conflicts.

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

A later pass can consume the parent produced above:

```csharp
var section = new Rule(
    "bill-to-section",
    RemediationActions.Group(
        "Sect",
        ClaimPredicates.FromRule("bill-to-heading")
            .Or(ClaimPredicates.FromRule("bill-to-paragraph"))),
    stage: Stage.Group,
    groupPass: 10);
```

The JSON surface is the optional `groupPass` property on a Group rule:

```json
{
  "id": "bill-to-section",
  "stage": "group",
  "groupPass": 10,
  "action": {
    "kind": "group",
    "tag": "Sect",
    "over": { "kind": "fromRule", "ruleId": "bill-to-paragraph" }
  }
}
```

Pass values may be sparse (`0`, `10`, `20`) and are evaluated numerically. Rule declaration and
rule-set composition order remain the stable order within one pass, but peers read the same frozen
frontier and cannot see one another. Explicit references from a Group rule to itself or to a
same/higher-pass Group rule fail declaration validation. Passes are document-scoped after rule sets
are composed, so a higher-pass rule in one rule set may consume a lower-pass rule from another as
long as rule ids remain unique.

Ambiguous reparenting and cycles are commit-blocking and cannot be suppressed. If a higher-pass rule
still names a leaf that a lower pass consumed, its zero-match warning names the consuming parent;
select that parent instead. The structural template remains descriptive today. When M6 makes
prescriptive slots create declared containers, Group passes remain available for inferred/open
hierarchy such as tables and variable list depth.

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

Anchors carry `Pages` (a `PageSelector`, defaulting to every page) and `Style` (a predicate used to
disambiguate candidates). `Pages` is applied before matching; `Style` filters matches before
`AnchorSelection`. Occurrence selection is expressed only through the zero-based
`NthInReadingOrder(n)` mode.

Text-label, table-header, repeated-element, style, and neighboring-text matching inherit the
declaring rule set's text normalization policy. Regex syntax itself is never normalized.

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

var cellRule = new Rule(
    "line-item-cell",
    RemediationActions.Tag("Span"),
    Predicates.Flow.InFlowRegion("line-items"),
    Granularity.Word);
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

Flow regions are page-local by default. Use `ContinueUntilEnd` when one activation may span page
breaks:

```csharp
new FlowRegion(
    "line-items",
    FlowBoundary.Anchor("line-items-header"),
    FlowBoundary.Anchor("subtotal-label"),
    ContinuationPolicy: FlowContinuationPolicy.ContinueUntilEnd,
    ReadingOrderMode: FlowReadingOrderMode.StructuredText,
    MaxPages: 12);
```

The start page is page one for `MaxPages`. A continued activation may pass through intermediate
pages containing neither boundary; a missing end is diagnosed at the limit or document end.
Repeated start boundaries, such as table headers, remain in the same activation and trim that
page's segment. A later start after an end creates a new activation.

Classify evaluation completes across the document before Group, and Group completes before Refine.
MCIDs, candidate ownership, and working content remain page-scoped. Group and Merge cross a page
break only when adjacent claims share the same continued activation; `SamePage()` always forces a
split. `Consecutive()` means adjacent in that activation's selected reading order.

`StructuredText` orders by page and structured content order. `GeometryTopToBottom` orders by page,
then top-to-bottom and left-to-right geometry. The mode controls `FirstIn`, `LastIn`, and `NthIn`
over the complete activation.

Continued tables produce one `Table` per activation. Numeric `HeaderRows` defaults to
`TableHeaderRowsScope.LogicalTable`, so the count applies once at the start of the activation.
Choose `TableHeaderRowsScope.EveryPage` to apply the count independently on every continuation
page. `HeaderSelector` is additive and remains the authoritative way to identify actually
repainted headers. JSON rules use `headerRowsScope: "logicalTable"` or `"everyPage"`.

Claim-level `Within("id")` dispatches by declaration kind. Anchors and zones are page-scoped and
require a single-page bounding box; using them against a cross-page claim is diagnosed rather than
silently returning no match. Multi-page claims can be within a continued flow only when every page
segment belongs to the same activation. `THead`, split-row reconstruction, spans, and irregular
grids remain separate table-model work.

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
    RemediationActions.TableOverFlattenedCells(
        ClaimPredicates.FromRule("line-item-cell"),
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
ClaimPredicates.FromRule("line-item-cell");
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

Group-pass predicate semantics are deliberately split between consumption and reference lookup:

| Predicate | Higher-pass behavior |
| --- | --- |
| `ClaimIs`, `ActionIs`, `FromRuleSet`, `FromRule` | Match only roots on the current structural frontier. `FromRule` may reference Classify or a lower Group pass. |
| `BeforeClaim`, `AfterClaim` | Positional only: the referenced rule resolves against every applied lower-pass claim, even if that claim has already left the frontier. |
| `Consecutive` | Compares the first/last leaf sequence ranges represented by each parent; across pages the parents must share one continued flow activation. |
| `Within(flowRegionId)` | Supports cross-page parents when every page segment belongs to the same activation. |
| Geometric `Within(LayoutCoord)`, `Within(zone)`, `Within(anchor)` | Requires one bounding box. Applying it to a cross-page parent is a commit-blocking diagnostic; use a flow region instead. |
| `SamePage` | Uses the claim's primary page, which is the first page for a cross-page parent. |

Synthetic `TR`/`TH`/`TD` nodes created by `TableOver` and fragments flattened by `MergeTo` are not
claims and never enter the frontier. A later pass can consume the `Table` or merged parent, not its
interiors or discarded inputs.

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
        RemediationAnchor.TextLabel("line-items-header", "Item"),
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
            CandidateSelector.Text(Granularity.Line),
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
            "line-item-header-cell",
            RemediationActions.Tag("Span"),
            Predicates.Anchor.SameRowAs("line-items-header", tolerance: 4),
            Granularity.Word),

        new Rule(
            "line-item-cell",
            RemediationActions.Tag("Span"),
            Predicates.Flow.InFlowRegion("line-items"),
            Granularity.Word),

        new Rule(
            "page-footer",
            RemediationActions.Artifact(ArtifactSubtype.Pagination),
            Predicates.Flow.InZone("footer"),
            Granularity.Line),

        new Rule(
            "line-items-table",
            RemediationActions.TableOverFlattenedCells(
                ClaimPredicates.FromRule("line-item-header-cell")
                    .Or(ClaimPredicates.FromRule("line-item-cell")),
                ClaimPredicates.FromRule("line-item-header-cell"),
                72, 250, 450, 600),
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

The table rules classify leaf cell content as `Span` and flatten those bindings directly into
generated `TH`/`TD` cells. Do not classify visual rows as `TR` before passing them to `TableOver`;
preserve-children mode rejects table-row and table-cell claims because they would create an invalid
`TD > TR` or nested-cell hierarchy.


## Existing Annotation Adoption

Existing annotations use `CandidateSelector.Annotations()` with `Predicates.Annotation` filters for
subtype, destination kind/value, contents, and existing structure ownership. Stable candidate ids are
formed from the zero-based page and `/Annots` array position. Page predicates always apply; geometry
predicates explainably reject annotations whose `/Rect` is absent or unusable.

```csharp
new Rule(
    "link-text",
    RemediationActions.Tag("Link"),
    Predicates.Text.Contains("Account details"),
    CandidateSelector.Text(Granularity.Line));

new Rule(
    "existing-link",
    RemediationActions.AdoptAnnotation(
        into: ClaimPredicates.FromRule("link-text"),
        destinationTarget: ClaimPredicates.FromRule("details-heading")),
    Predicates.Annotation.Subtype("Link")
        .And(Predicates.Annotation.DestinationKind(AnnotationDestinationKind.Internal)),
    CandidateSelector.Annotations());
```

`Into` pairs annotations and compatible Classify claims on the same page by rectangle intersection;
missing or multiple targets block commit. Omitting it creates a standalone `Link`, `Form`, or `Annot`
node. `DestinationTarget` also selects Classify claims and reuses profile-aware structure destination
serialization. A link must already have non-empty `/Contents` or receive
`accessibleDescription`; descriptions are never inferred. Group passes may subsequently reparent the
adopted node normally.

Annotation ownership is exclusive. Reports classify every input annotation as `Unmodeled`, `Exempt`,
`Planned`, or `Applied`, including candidate id, rule, produced tag, and destination kind. Hidden,
wholly off-page, `Popup`, and valid specialized `PrinterMark` annotations use their documented
exemptions. Dry-run does not change dictionaries, and a failed commit rolls all annotation changes
back.

JSON uses candidate kind `annotation`, annotation predicate kinds such as `annotationSubtype` and
`annotationDestinationKind`, and action kind `adoptAnnotation` with optional `into`,
`accessibleDescription`, and `destinationTarget` claim predicates.

## Complete Artifact Property Lists

`ArtifactSubtype` remains the historical PDF artifact `/Type`. The additive semantic subtype is
`Header`, `Footer`, or `Watermark`; actions can also include the candidate `/BBox` and unique
`Top`, `Bottom`, `Left`, or `Right` attachment edges.

```csharp
RemediationActions.HeaderArtifact();    // /Type /Pagination, /Subtype /Header, /BBox, /Attached [/Top]
RemediationActions.FooterArtifact();    // /Type /Pagination, /Subtype /Footer, /BBox, /Attached [/Bottom]
RemediationActions.WatermarkArtifact(); // /Type /Pagination, /Subtype /Watermark, /BBox
```

The same fields are available on artifact inventory items and therefore control automatic furniture
wrappers. JSON keeps legacy `"subtype": "Pagination"` as the `/Type` spelling and adds canonical
`"type"`, `"semanticSubtype"`, `"includeBoundingBox"`, and `"attached"`; conflicting legacy and
canonical type values are rejected. The schema remains v1.

`PageWriter.BeginArtifact(PdfName?)` remains compatible. The overload accepting type, subtype,
optional bounds, and attached edges emits the complete property list; `BeginHeaderArtifact`,
`BeginFooterArtifact`, and `BeginWatermarkArtifact` supply the standard defaults.

## What Remediation Cannot Fix

Remediation adds structure. It does not rewrite the content stream's text, fonts, or encodings. Several PDF/UA requirements live below the structure layer and no rule set can reach them:

- **Fonts.** The output reuses the input's fonts. A font that is not embedded, has no `ToUnicode` map, or references `.notdef` fails PDF/UA regardless of tagging quality. Because `StrictConformance` defaults to `true`, a non-embedded font will **fail the commit** with an error that does not obviously point back at the input document.
- **Word boundaries.** Producers that separate words with `TJ` offsets instead of space characters yield text that fails PDF/UA §7.2 and extracts as `InvoiceNumber`. Wrapping it in a marked-content scope does not change that.

Check these before investing in a rule set for a new document family. Tracked as RRM-026.

## What the Rule Language Cannot Express

Capabilities a rule author will reach for that do not exist yet. Each links to its tracker entry.

| Missing | Tracker |
| --- | --- |
| Expected match counts on a rule (`MinMatches`), so drift fails loudly | RRM-016 |
| A rule-set applicability guard, so the wrong template is refused | RRM-017 |
| Output assertions ("exactly one `H1`", "every row has 4 cells") | RRM-018 |
| Selecting or excluding pre-existing marked content and optional content | RRM-020 |
| Splitting a list line into `Lbl` + `LBody`; setting `/ListNumbering` | RRM-021 |
| Relative or normalized heading levels for optional sections | RRM-023 |
| Generating an outline from headings; setting `/PageLabels` | RRM-024 |
| Row-scoped table headers; `/Summary`; declarative `/Headers` | RRM-005, RRM-025 |
| Selecting non-text content (images, paths, form XObjects) | RRM-002 |
| Flow regions and tables that continue across a page break | RRM-001, RRM-005 |

## Confidence

Confidence uses a closed `[0,1]` scale. `1` is a direct match, `0` is the weakest accepted match,
and a rule's `MinConfidence` rejects lower values. Predicates that do not estimate uncertainty are
confidence-neutral and inherit `DefaultConfidence`, which defaults to `1`. Toleranced layout and
table inference are the built-ins that deliberately produce degraded values. For composition,
matching `And` uses the minimum operand confidence, `Or` uses the branch that establishes the match,
and `Not` preserves the evaluated operand confidence. Predicate traces identify the node that
contributed a degraded value.

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
- rule cardinality mismatches, with rule and page provenance;
- unclaimed content under `FailFast` or flagged leftovers;
- orphaned MCIDs, missing `/StructParents`, or reading-order drift.

When `DebugWrite = true`, rule ids are written to structure element titles to make generated PDFs easier to inspect. Disable it for production output if those titles are not desired.

### Inspecting Outcomes

`RemediationReport` carries per-claim outcomes with rule provenance, typed candidate summaries,
confidence, status, page, bounds, and the MCIDs bound to each structure node. Text summaries expose
both raw and normalized text:

```csharp
foreach (var outcome in report.Outcomes)
{
    Console.WriteLine($"{outcome.RuleId} p{outcome.PageIndex} {outcome.Status} conf={outcome.Confidence}");
}
```

`report.SkippedOutcomes` carries the same shape for claims that were conflicted, overridden, or failed.

`report.RuleEvaluations` contains every composed rule, including rules that matched nothing. Each summary has document totals and selected-page counts for inputs considered, inputs matched, confidence and conflict rejections, applied claims, and overridden claims:

```csharp
foreach (var rule in report.RuleEvaluations)
{
    Console.WriteLine(
        $"{rule.RuleId}: stage={rule.Stage}, group-pass={rule.GroupPass}, " +
        $"matched={rule.Total.InputsMatched}, applied={rule.Total.AppliedClaims}, " +
        $"conflicts={rule.Total.RejectedByConflict}");
}
```

When `AutoArtifact` is configured, `report.AutoArtifacts` identifies each leftover text item. Dry-run entries have `Planned` disposition; successful commit entries have `Applied` disposition.

`Explain(...)` reports which rules considered a given piece of content, accepting a `StructuredSourceRef`, a `RemediationCandidate`, or a `StructuredCharacter`:

```csharp
foreach (var outcome in report.Explain(someCharacter))
{
    Console.WriteLine($"{outcome.RuleId} -> {outcome.Status}");
}
```

`Explain` remains the positive provenance API: it answers which rules touched content. For negative
diagnostics, pass a `RemediationTraceRequest` to `DryRun`. Rejections appear in
`report.PredicateTraces`, and `ExplainRejection(ruleId, candidateId)` returns one retained trace.
Tracing preserves short-circuit behavior and records skipped operands and the rejecting `And`
operand. The CLI equivalent is `--dry-run --explain-rule <id>` with optional `--explain-page`.

Semantic assertions are attached to `RuleSet.Assertions`. Rule-output counts,
structure-element counts, and direct parent/child shapes produce
`RemediationReport.AssertionOutcomes`; an unsuppressed `SemanticAssertionFailed` blocks commit.
`RemediationReport.PlannedSemanticTree` exposes the immutable tree derived from the finalized plan.

### Structural templates

A rule set may declare one closed, document-scoped `RemediationStructuralTemplate`. Its root must be
exactly one `Document`; children are ordered and use `ExactlyOne`, `Optional`, `ZeroOrMore`, or
`OneOrMore`. Template node ids are globally unique slots. A structure-producing rule binds a slot
with `Rule.Slot`; several rules may share a repeating slot, while singular slots allow one rule.
Slot-bound rules cannot also declare `RuleCardinality`.

```csharp
var template = new RemediationStructuralTemplate(new[]
{
    new RemediationStructuralTemplateNode("H1", id: "title"),
    new RemediationStructuralTemplateNode(
        "P", id: "body", occurrence: RemediationStructuralOccurrence.OneOrMore)
});

var ruleSet = new RuleSet(
    "report",
    new[]
    {
        new Rule("title", RemediationActions.Tag("H1"),
            candidates: CandidateSelector.Text(Granularity.Paragraph), slot: "title"),
        new Rule("body", RemediationActions.Tag("P"),
            candidates: CandidateSelector.Text(Granularity.Paragraph), slot: "body")
    },
    structuralTemplate: template);
```

JSON v1 uses a top-level `template` node with `tag`, optional `id`, `occurrence`, `pages`,
`spansPages`, and `children`; rules use `slot`. Omitted occurrence means `exactlyOne`.

Dry-run matches the finalized planned tree. Commit matches that plan before mutation, snapshots and
matches the materialized builder tree, and rolls back on an unsuppressed mismatch. Differences are
available through `RemediationReport.TemplateDifferences`, including positional paths, pages, slot
and rule provenance. Suppression scopes have the form
`RuleSet:{id}:Template:{derivedPath}`. Templates are descriptive in phase one: they validate but do
not create missing containers.

Template preflight recognizes the standard structure types for the configured authoring profiles
and enforces the containment rules that strict accessibility authoring currently models explicitly:
tables and their row/cell groups, and lists and their item/label/body groups. Other standard-tag
parent/child combinations remain permissive in phase one and are still subject to the document's
normal strict-authoring and external conformance validation. Template preflight should therefore
not be read as a complete PDF/UA content-model validator.

## Authoring Guidance

- Start with stable anchors and flow regions, then classify content inside those regions.
- Prefer predicate-based anchors for common labels that may appear more than once.
- Use `maxDistance` for label/value fields so a nearby label does not capture unrelated content.
- Use `FirstAfter` or `FirstIn` for ordered field extraction; use `NearestTo` only when geometric nearness is the intent.
- Classify first, then group or refine claims. Do not try to build parent structure by re-selecting raw text.
- Keep rule ids stable because reports, debug output, and downstream tests depend on them.
- Use `RemediationLeftoverPolicy.FailFast` in development *and* in production. When `AutoArtifact` is justified, declare a zone-qualified artifact inventory, inspect `report.AutoArtifacts`, and declare cardinality on every required semantic rule.
- Route business-critical fields through `RuleCardinality.Exactly(...)` and use `RequiredSingle` anchors where stable anchor identity is also required.
- Assert semantic output shape in application tests; cardinality detects selector drift but does not replace RRM-018 structure assertions.
- Verify output with an external validator, and separately verify that the *right* content got the *right* tags. Conformance validation cannot tell you the invoice total was tagged as a footer.

## Current Limitations

See the [Rule-Based Remediation Gap Tracker](rule-based-remediation-gaps.md) for the full list with priorities and completion criteria, and the [Delivery Plan](rule-based-remediation-plan.md) for the milestone each one is scheduled into. The items most likely to affect a first template are RRM-016 (silent drift), RRM-026 (input conformance), RRM-032 (text normalization), and RRM-006 (the table example above).
