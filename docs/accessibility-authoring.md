# Accessible Authoring

`PdfLexer` supports accessibility authoring for two flows only:

- creating a new tagged document from scratch;
- opening an existing document that is currently untagged, then applying accessibility setup and authoring a new structure tree.

It does not support editing, preserving, or remediating an existing `StructTreeRoot`. If a document is already tagged, `PdfDocument.ApplyAccessibilitySetup(...)` and `PdfDocument.Structure` reject that workflow with `PdfAccessibilitySetupException`.

## Recommended Workflow

1. Open or create the document.
2. Call `ApplyAccessibilitySetup(...)` before authoring tagged content.
3. Build structure with `PdfDocument.Structure`.
4. Write page or form content inside `BeginMarkedContent(structureNode)` / `EndMarkedContent()`.
5. Save the document and validate the result with the fixture-driven conformance workflow.

```csharp
using PdfLexer;
using PdfLexer.Content;
using PdfLexer.Fonts;

using var doc = PdfDocument.Create();
doc.ApplyAccessibilitySetup("en-US", "Accessible Example", PdfUaProfile.PdfUa1);

var page = doc.AddPage();
var section = doc.Structure.AddSection("Example");
var heading = section.AddHeader(1, "Accessible Example");
var paragraph = heading.Back()
    .AddParagraph("Overview")
    .ActualText("Overview of the accessible example");

using (var writer = page.GetWriter())
{
    writer.BeginMarkedContent(heading.GetNode());
    writer.Font(Base14.Helvetica, 16).TextMove(40, 760).Text("Accessible Example");
    writer.EndMarkedContent();

    writer.BeginMarkedContent(paragraph.GetNode());
    writer.Font(Base14.Helvetica, 12).TextMove(40, 735).Text("Overview");
    writer.EndMarkedContent();
}
```

## Builder Surface

Use `PdfDocument.Structure` and `IStructureContext` to author the logical structure tree:

- generic nodes: `AddElement`, `AddParagraph`, `AddHeader`, `AddSpan`, `AddFigure`, `AddFormula`, `AddLink`;
- richer accessibility metadata: `Alt`, `ActualText`, `Expansion`, `Lang`, `ElementId`, `References`;
- table semantics: `TableScope`, `TableHeaders`, `TableSummary`;
- list semantics: `ListNumbering`;
- root-level mapping helpers: `MapRole`, `AddClassDefinition`, `DeclareNamespace`;
- object binding: `AddLink(...)`, `AddFormField(...)`, `BindImage(...)`, `BindFormXObject(...)`.

The builder owns the authored tree for the current output. It is not a parser or editor for pre-existing tags.

## Remediation Lifecycle

For existing rendered PDFs that are still untagged, use `PdfDocument.BeginRemediation(...)` instead of calling `ApplyAccessibilitySetup(...)` directly. The remediation session is an orchestration path over the same untagged setup behavior: it validates that no `StructTreeRoot` exists, lets rules build a remediation plan, and applies accessibility setup during `Commit()`.

This does not relax the tagged-document gate. Documents that already contain a `StructTreeRoot` are rejected by both direct accessibility setup and remediation sessions.

Use remediation for known document families where the layout repeats across future files, such as invoices, statements, notices, reports, and exports. The caller owns the template-specific rules; `PdfLexer` provides the deterministic rule language, content wrapping, structure-tree binding, diagnostics, and reporting.

See [Rule-Based PDF Remediation](rule-based-remediation.md) for detailed rule-language documentation, dynamic anchors, flow regions, claim predicates, actions, diagnostics, and complete examples.

```csharp
using PdfLexer;
using PdfLexer.Remediation;

using var doc = PdfDocument.Open(inputBytes);
using var session = doc.BeginRemediation(new RemediationSessionConfiguration
{
    Language = "en-US",
    Title = "Remediated Invoice",
    Profile = PdfUaProfile.PdfUa1,
    LeftoverPolicy = RemediationLeftoverPolicy.AutoArtifact
});

var rules = new RuleSet(
    "invoice-v1",
    new[]
    {
        new Rule(
            "title",
            RemediationActions.Tag("H1"),
            Predicates.Text.StartsWith("Invoice"),
            Granularity.Line),
        new Rule(
            "page-footer",
            RemediationActions.Artifact(ArtifactSubtype.Pagination),
            Predicates.Flow.InZone("footer"),
            Granularity.Line)
    },
    Array.Empty<RemediationAnchor>(),
    tolerancedZones: new[]
    {
        new TolerancedZone("footer", LayoutCoord.MarginRelative(bottom: 36), Tolerance: 4)
    });

session.Use(rules);

var dryRun = session.DryRun();
if (dryRun.Diagnostics.Count == 0)
{
    session.Commit();
}
```

## Remediation Rule Examples

Rules are evaluated in three stages: `Classify` tags or artifacts raw candidates, `Group` consumes applied claims to build parents, and `Refine` applies attributes, links, or sibling reordering. Rule order is deterministic within each stage.

Every declarative rule has the same shape:

- selector: page selector plus target granularity (`Character`, `Word`, `Line`, or `Paragraph`);
- predicate: composable text, font, geometry, anchor, flow, or relational tests over the selected candidates;
- action: tag, artifact, table, group, attribute/link refinement, or scoped sibling reorder;
- metadata: stable rule id, optional override behavior, and optional minimum confidence.

`Classify` rules select raw content candidates. `Group` and `Refine` rules select existing claims with `ClaimPredicate`; they do not re-select raw content. That separation keeps parent construction, table wrapping, language attributes, links, and reading-order fixes tied to applied MCID/structure bindings rather than to a second heuristic search.

```csharp
var anchors = new[]
{
    RemediationAnchor.Selector(
        "invoice-label",
        Granularity.Line,
        Predicates.Text.Matches(@"^Invoice\s*#$").And(
            Predicates.Geo.In(LayoutCoord.Zone(NamedLayoutZone.Header))),
        AnchorSelection.RequiredSingle),
    RemediationAnchor.TableHeader("line-items-header", "Item", "Qty", "Amount"),
    RemediationAnchor.TextLabel("subtotal-label", "Subtotal")
};

var flowRegion = new FlowRegion(
    "line-items",
    FlowBoundary.Anchor("line-items-header"),
    FlowBoundary.Anchor("subtotal-label"));

var invoiceRules = new RuleSet(
    "invoice-template",
    new[]
    {
        new Rule(
            "invoice-number",
            RemediationActions.Tag("P"),
            Predicates.Anchor.RightOf("invoice-label").And(
                Predicates.Anchor.SameRowAs("invoice-label", tolerance: 4)),
            Granularity.Word),
        new Rule(
            "line-item-row",
            RemediationActions.Tag("TR"),
            Predicates.Flow.InFlowRegion("line-items"),
            Granularity.Line),
        new Rule(
            "line-items-table",
            RemediationActions.TableOver(ClaimPredicates.FromRule("line-item-row"), 72, 300, 380, 470),
            stage: Stage.Group),
        new Rule(
            "table-lang",
            RemediationActions.Lang(ClaimPredicates.FromRule("line-items-table"), "en-US"),
            stage: Stage.Refine)
    },
    anchors,
    flowRegions: new[] { flowRegion });
```

Common predicate families:

```csharp
var text = Predicates.Text.Matches(@"^INV-\d+$");
var style = Predicates.Font.Size(NumericOperator.GreaterThanOrEqual, 14);
var geometry = Predicates.Geo.Intersects(LayoutCoord.Zone(NamedLayoutZone.Header));
var anchorRelative = Predicates.Anchor.Below("section-heading", tolerance: 6);
var flow = Predicates.Flow.InFlowRegion("bill-to-address");
var firstDynamicValue = Predicates.Flow.FirstAfter("invoice-label", Predicates.Text.Matches(@"^INV-\d+$"));
var related = Predicates.Relational.After("heading-rule");
```

Common action shapes:

```csharp
var tag = RemediationActions.Tag("P");
var artifact = RemediationActions.Artifact(ArtifactSubtype.Layout);
var table = RemediationActions.TableOver(ClaimPredicates.ClaimIs("TD"), 72, 220, 360, 480);
var flattenedTable = RemediationActions.TableOverFlattenedCells(ClaimPredicates.ClaimIs("Span"), 72, 220, 360, 480);
var group = RemediationActions.Group("Sect", ClaimPredicates.FromRule("section-children"));
var merge = RemediationActions.MergeTo("P", ClaimPredicates.FromRule("wrapped-paragraph-line"));
var lang = RemediationActions.Lang(ClaimPredicates.Within("bill-to-address"), "en-US");
var reorder = RemediationActions.ReorderSiblings(ClaimPredicates.ClaimIs("P"), SiblingReorderMode.GeometryTopToBottom);
```

Layout coordinates should prefer relative or anchor-based concepts over fixed rectangles:

```csharp
LayoutCoord.MarginRelative(top: 72);
LayoutCoord.Percentage(top: 0.10, left: 0.05, right: 0.05);
LayoutCoord.NamedAnchor("total-label", LayoutCoordExpansion.Below(48));
LayoutCoord.BetweenAnchors("bill-to-label", "ship-to-label", padding: 6);
LayoutCoord.TolerancedZone("footer");
LayoutCoord.FlowRegion("line-items");
```

The remediation fixture corpus exercises the same patterns in code. See `test/PdfLexer.Tests/RemediationFixtureGenerator.cs` for complete invoice-like, statement-like, report-like, form-like, multi-column, and mixed-page-size examples, and `test/PdfLexer.Tests/RemediationFixtureTests.cs` for the expected structure, MCID, provenance, anchor, and flow assertions.

## Document Setup

`ApplyAccessibilitySetup(...)` configures the document-level defaults required by this change set:

- `Catalog/Lang`;
- `Info/Title`;
- `ViewerPreferences/DisplayDocTitle = true`;
- `MarkInfo/Marked = true` and `MarkInfo/Suspects = false`;
- `/Tabs = /S` on pages unless an explicit different override is already present;
- XMP metadata with the selected PDF/UA profile identifier.

Call it before save when you are creating a new tagged document or tagging an existing untagged document for the first time.

## Supported Scope

Supported:

- new documents created with `PdfLexer`;
- existing untagged documents that need a new structure tree;
- rule-driven remediation of known untagged transactional templates;
- tagged links, figures, widgets, and structural XObjects authored through the builder;
- Unicode-safe tagged text written inside structure-aware marked-content scopes.

Not supported:

- editing an existing tagged PDF;
- preserving or rewriting a third-party `StructTreeRoot`;
- automatically generating rule sets from samples;
- solving arbitrary layout constraints or silently auto-tagging content without explicit rules;
- using `PdfLexer` itself as the PDF/UA conformance authority.

## Validation Workflow

Use the test fixture corpus plus an external validator:

1. Generate the corpus under `test/results/accessibility-fixtures`.
2. Run veraPDF against the generated PDFs.
3. Optionally cross-check with PAC on Windows.

Typical CLI:

```bash
verapdf --format text --flavour ua1 test/results/accessibility-fixtures/*.pdf
```

PAC (axes4) is a useful additional manual check on Windows, but it is not the primary automated gate for this repository.
