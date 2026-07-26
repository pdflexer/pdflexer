# Rule-Based Remediation Gap Tracker

Last reviewed: 2026-07-26

> [!NOTE]
> This is the gap **register** — what is missing and why it matters. For the scheduled version
> (milestones, dependency order, and exit gates) see the
> [Rule-Based Remediation Delivery Plan](rule-based-remediation-plan.md). Gap status is tracked
> here, not there.

This document tracks gaps in the current `PdfLexer.Remediation` declarative
rule language and runtime. It is intentionally narrower than
`accessibility_gaps.md`: the focus here is whether reusable rules can remediate
real-world, untagged transactional PDFs safely and consistently.

The current implementation has a strong foundation:

- deterministic `Classify -> Group -> Refine` stages;
- separate candidate and claim predicates;
- applied-claim structure bindings and MCID reuse;
- dry-run, validation, diagnostics, and atomic commit behavior;
- anchor-relative selection, toleranced zones, and page-relative coordinates;
- exact character/word materialization when one selected range can be split
  safely;
- simple table, grouping, merge, attribute, reorder, and link actions.

The gaps below are reasons not to describe current enterprise coverage or
PDF/UA assurance as complete.

## Priority and status

| Priority | Meaning |
| --- | --- |
| P0 | Blocks common transactional remediation or confidence in generated output |
| P1 | Material limitation affecting important layouts or semantic structures |
| P2 | Intentional scope limitation or less-common capability |

| ID | Priority | Status | Gap |
| --- | --- | --- | --- |
| RRM-001 | P0 | Open | Flow regions do not continue across pages |
| RRM-002 | P0 | Open | Declarative rules cannot select or artifact non-text content |
| RRM-003 | P0 | Complete | Multiple inline claims in one text operator may be order-sensitive |
| RRM-004 | P1 | Open | Group rules cannot consume prior Group outputs |
| RRM-005 | P0 | Open | Table construction handles only regular, page-local grids |
| RRM-006 | P0 | Complete | Table examples and validation can admit incorrect hierarchy |
| RRM-007 | P1 | Open | No declarative Figure/caption association |
| RRM-008 | P1 | Open | Anchor and table geometry is not text-orientation-aware |
| RRM-009 | P1 | Open | Interactive form widgets are outside the rule model |
| RRM-010 | P0 | Partially addressed | External PDF/UA validation baseline is incomplete |
| RRM-011 | P1 | Open | Remediation coverage is tested primarily with synthetic PDFs |
| RRM-012 | P1 | Open | Multi-column reading order support is limited |
| RRM-013 | P1 | Open | Declarative tag and JSON schema validation is incomplete |
| RRM-014 | P2 | Intentional limitation | Existing tagged PDFs cannot be repaired |
| RRM-015 | P2 | Intentional limitation | Scanned PDFs require an external text/OCR layer |
| RRM-016 | P0 | Complete | Rules have no expected-match cardinality, so template drift is silent |
| RRM-017 | P0 | Open | No rule-set applicability guard or document-family check |
| RRM-018 | P0 | Open | No semantic output assertions beyond conformance validation |
| RRM-019 | P0 | Open | Annotations already present in the input are outside the rule model |
| RRM-020 | P1 | Open | Pre-existing marked content and optional content are unmodeled |
| RRM-021 | P1 | Open | List interior structure and `/ListNumbering` are not expressible |
| RRM-022 | P1 | Open | Artifact subtypes cannot express Header/Footer/Watermark |
| RRM-023 | P1 | Open | Heading levels are literal with no ordering model |
| RRM-024 | P1 | Open | No document-level navigation actions (outline, page labels) |
| RRM-025 | P1 | Open | Table `/Scope` is hardcoded and `/Summary` is unreachable |
| RRM-026 | P0 | Open | Input-level conformance defects cannot be repaired or diagnosed |
| RRM-027 | P1 | Open | The confidence model is undefined and `MinConfidence` is unusable by default |
| RRM-028 | P1 | Open | Anchor resolution scope is per page but documented as absolute |
| RRM-029 | P1 | Open | Structure sibling order and the reading-order default are unspecified |
| RRM-030 | P1 | Open | `DryRun` does not guarantee `Commit`; failure semantics are undocumented |
| RRM-031 | P1 | Open | Rule-set composition and precedence are undefined |
| RRM-032 | P0 | Open | Text normalization for predicate matching is unspecified |
| RRM-033 | P1 | Partially addressed | No negative explain or per-rule match diagnostics |
| RRM-034 | P2 | Open | Predicate evaluation cost is unbounded |
| RRM-035 | P1 | Partially addressed | Documented rule language omits shipped API surface |

RRM-016 through RRM-035 were added by a second review pass on 2026-07-24 that examined the
documented concept model and public API rather than runtime behavior. They are concept and contract
gaps: cases the rule language cannot express, contracts it does not state, and failure modes it does
not surface. They are additive to RRM-001 through RRM-015, not a re-statement of them.

## Internal MVP scope and effort estimate

The remediation infrastructure was implemented in approximately two days with
AI assistance, followed by another one to two days for the current rule
language. Given that demonstrated development velocity and the amount of
working infrastructure already present, the language is estimated to be about
80-85% complete for a deliberately narrow internal MVP.

The remaining work is disproportionately validation and hardening rather than
new syntax. Real PDFs, external conformance tools, content-stream fragmentation,
font behavior, XObjects, and semantic table correctness can expose issues that
are not visible in synthetic unit tests. AI assistance can accelerate diagnosis
and implementation, but does not remove the need to inspect real output and
confirm accessibility semantics.

### Recommended MVP boundary

The following scope keeps an internal MVP achievable:

- untagged input PDFs only;
- PDF/UA-1 as the initial target profile;
- a small number of known internal document families;
- manually authored and reviewed rule sets rather than automatic rule
  generation;
- extractable, predominantly horizontal text;
- simple tables with explicit columns and no row or column spans;
- basic non-text handling so decorative paths and images can be artifacted and
  important images can be explicitly tagged;
- dry-run, diagnostics, JSON rules, CLI execution, and external validation as
  part of the supported workflow.

The MVP should explicitly exclude:

- OCR and image-only scans;
- repair of existing or partially correct structure trees;
- interactive AcroForm remediation;
- arbitrary unstructured publications;
- nested lists and deeply nested generated hierarchy;
- complex tables with spans or irregular header associations;
- independently rotated or vertical text unless required by the initial
  document corpus.

Multi-page flow is a scope decision. If the initial document families repeat
complete boundaries on every page, RRM-001 can be deferred. If a logical
section or table begins on one page and ends on another, RRM-001 becomes an MVP
requirement.

### Estimated AI-assisted engineering effort

| Work package | Related gaps | Estimate |
| --- | --- | ---: |
| Rule cardinality, per-rule match counts, and output assertions | RRM-016, RRM-018, RRM-033 | 2-4 days |
| Text normalization and input-conformance pre-flight | RRM-032, RRM-026 | 2-3 days |
| Stabilize multiple inline claims and correct table hierarchy/examples | RRM-003, RRM-006 | 1-3 days |
| Add basic non-text artifact and Figure handling | RRM-002, part of RRM-007 | 2-4 days |
| Adopt existing annotations; complete artifact subtypes | RRM-019, RRM-022 | 2-3 days |
| Specify contracts: anchor scope, sibling order, commit semantics, composition, confidence | RRM-027 to RRM-031 | 1-2 days |
| Establish strict PDF/UA-1 and veraPDF regression baseline | RRM-010 | 1-3 days |
| Exercise representative internal PDFs and fix discovered blockers | RRM-011 | 3-6 days |
| Harden JSON authoring, CLI diagnostics, and MVP documentation | RRM-013, RRM-035 | 1-2 days |
| **Likely internal MVP total, allowing work to overlap** |  | **16-24 focused engineer-days** |

The second review pass raised this estimate from the original 10-15 days. The added work is almost
entirely assurance rather than new rule syntax: cardinality, assertions, per-rule diagnostics,
normalization, and contract specification. That reordering is deliberate. Without RRM-016 and
RRM-018 there is no automated way to tell whether any of the other fixes actually improved a real
document, and the MVP exit gate falls back to human inspection of every output.

The earlier "80-85% complete" characterization remains reasonable for the *rule language*. It is too
generous for the *system*, where the assurance layer that would let a rule set run unattended does
not yet exist.

Conditional additions:

- add 2-5 days if true cross-page flow or continuous multi-page tables are
  required;
- add 3-7 days to support PDF/UA-2 at the same confidence level;
- add 5-15 days for complex tables, multi-column reading order, rotated text,
  interactive forms, or nested structures, depending on which are required.

These are focused engineering days with continued AI assistance, not elapsed
calendar time. A reasonable planning allocation is three calendar weeks for
one engineer: approximately two weeks for implementation and validation plus
one week of contingency for issues discovered in real PDFs.

### Delivery ranges

| Level | Effort | Result |
| --- | ---: | --- |
| Narrow pilot | 3-5 days | One or two text-heavy templates, page-local flows, explicit grids, and manual review |
| Usable internal MVP | 10-15 days | Several representative templates, basic graphics, stable JSON/CLI workflow, external validation, and regression fixtures |
| Dependable internal platform | 20-35 days | Multiple PDF producers, multi-page tables, richer figures, wrapped cells, stronger reading order, and broader conformance evidence |

The narrow pilot is useful for discovering whether the proposed MVP boundary
matches the actual internal document corpus. It should not be presented as
general PDF remediation coverage.

### MVP exit gate

The internal MVP is ready for use when:

- [ ] the initial supported document families and unsupported cases are
  written down;
- [ ] each family has representative inputs from the actual producing system,
  including meaningful layout and data variation;
- [ ] one reviewed rule set handles those variants without per-file code
  changes;
- [ ] commits complete with no unexplained or unjustifiably suppressed
  remediation diagnostics;
- [ ] output passes veraPDF for the selected PDF/UA profile;
- [ ] rendering comparison shows no unintended visual change;
- [ ] structure shape, MCID ownership, reading order, and non-text handling are
  covered by regression tests;
- [ ] dry-run output is sufficient for an internal rule author to understand
  unmatched, skipped, ambiguous, and low-confidence content;
- [ ] rule and CLI failures identify the document, page, rule, and source
  content needed for diagnosis;
- [ ] a rule that stops matching its intended content fails the run rather than
  degrading the output silently (RRM-016);
- [ ] output correctness is asserted automatically, not only by human review
  (RRM-018);
- [ ] any diagnostic suppression in use is listed, justified, and reviewed
  (RRM-035);
- [ ] `AutoArtifact` is either unused in production or bounded and reported, so
  it cannot hide semantic content.

---

## RRM-001: Flow regions do not continue across pages

**Status:** Open

**Priority:** P0

`FlowRegion` exposes `ContinuationPolicy` and `ReadingOrderMode`, including
`ContinueUntilEnd`, but `FlowRegionResolver` currently resolves both boundaries
against the current page's `StructuredTextPage`. Neither option influences
resolution.

A region whose header is on page 1 and whose subtotal is on page 10 therefore
does not become one continuous region. Intermediate pages cannot resolve both
anchors. Group and table actions also execute per page, producing separate
parents rather than one logical multi-page structure.

**Impact**

- Multi-page invoice and statement tables are not represented as one logical
  table.
- A section cannot start on one page and end on a later page unless each page
  independently contains usable boundaries.
- The public model promises behavior that the runtime does not currently
  perform.

**Relevant code**

- `src/PdfLexer/Remediation/FlowRegion.cs`
- `src/PdfLexer/Remediation/FlowRegionResolver.cs`
- `src/PdfLexer/Remediation/RemediationSession.cs` (`EvaluatePage`)

**Completion criteria**

- [ ] `CurrentPageOnly` and `ContinueUntilEnd` have observably different
  runtime behavior.
- [ ] Flow state can start, continue through intermediate pages, and stop on a
  later page.
- [ ] Missing start/end anchors are diagnosed according to the page's role in
  the continuation rather than always treated as failure.
- [ ] `ReadingOrderMode` controls candidate ordering.
- [ ] Tests cover a header on page 1, intermediate pages without either
  boundary, and a subtotal on the final page.
- [ ] Multi-page grouping and table behavior is explicitly defined and tested.

## RRM-002: Declarative rules cannot select or artifact non-text content

**Status:** Open

**Priority:** P0

Classify rules enumerate candidates from `StructuredTextPage`. There are no
declarative candidate types or predicates for `ImageContent`, `PathSequence`,
or `FormContent`. `AutoArtifact` likewise enumerates only `TextContent`, while
the untagged-content diagnostic examines every content item.

The imperative `StructuralBuilder` can bind images, but that capability is not
available as a portable, serialized remediation rule.

**Impact**

- Common logos cannot be tagged as `Figure` by the declarative language.
- Rules cannot artifact borders, backgrounds, decorative paths, or decorative
  images.
- PDFs containing otherwise ordinary graphics may fail commit with untagged
  content even when all text has been classified.
- The rule language is currently text-centric rather than document-content
  complete.

**Relevant code**

- `src/PdfLexer/Remediation/RemediationSession.cs`
  (`EvaluateClassifyRule`, `ApplyLeftoverPolicyAfterValidation`,
  `CheckUntaggedContent`)
- `src/PdfLexer/Remediation/RemediationCandidate.cs`
- `src/PdfLexer/DOM/StructuralBuilder.cs` (`BindImage`)

**Completion criteria**

- [ ] The candidate model identifies text, images, paths, and form XObjects.
- [ ] Candidate predicates can filter by content type, bounds, resource
  identity, and repeated use where applicable.
- [ ] `Tag("Figure")` can bind an image or suitable graphical content.
- [ ] `Artifact(...)` and `AutoArtifact` can safely cover decorative non-text
  content.
- [ ] Diagnostics distinguish painting content from state-only operators.
- [ ] Tests cover a logo, table ruling lines, a background path, and a reused
  XObject.

## RRM-003: Multiple inline claims in one text operator may be order-sensitive

**Status:** Complete

**Priority:** P0

One word or character range inside a `Tj`/`TJ` operator can be split and wrapped
without changing rendered text. The unresolved case is applying multiple
separate claims to different ranges in the same original operator.

Split `TextContent` fragments now retain their original source-character
offset. Exact-range resolution intersects the requested original interval with
the correct fragment, and ownership uses half-open character intervals across
all text granularities. Rule order still determines conflict precedence while
materialization and MCID allocation use page reading order.

**Impact**

The common case `"Invoice #: INV-12345"` in one text operator may not reliably
support separately tagging the label and value in natural rule order.

**Relevant code**

- `src/PdfLexer/Content/Model/TextContent.cs`
  (`TrySplitByCharacterRange`, `CreateContent`)
- `src/PdfLexer/Content/Model/ContentModelBridge.cs` (`FindItem`)
- `src/PdfLexer/Remediation/RemediationCandidate.cs` (`MaterializeLeaves`)
- `src/PdfLexer/Remediation/TextOwnership.cs`
- `src/PdfLexer/Remediation/RemediationSession.cs`
  (`GetTargetSpans`, `CreateResidualClaims`)

**Completion criteria**

- [x] Add a regression test that tags two non-overlapping words from the same
  `Tj` with separate rules in reading order.
- [x] Add the equivalent test for a `TJ` array with glyph adjustments.
- [x] Materialization is independent of rule order.
- [x] Ownership detects interval overlap between character, word, line, and
  paragraph claims over the same source content.
- [x] Override replaces only the intended overlapping range.
- [x] Reparsed text, glyph selection, and positioning remain unchanged.

## RRM-004: Group rules cannot consume prior Group outputs

**Status:** Open

**Priority:** P1

`EvaluateClaimRunRule` selects only the `Stage.Classify` snapshot. All Group
rules therefore consume the same flat classify claims; a later Group rule
cannot select a parent produced by an earlier Group rule.

**Impact**

- Nested lists such as `L -> LI -> L -> LI` cannot be assembled through
  declarative multi-pass grouping.
- Multi-level sections and other parent-of-parent structures require
  imperative workarounds.
- Overlapping Group rules may reparent the same classify nodes rather than
  composing hierarchy.

**Completion criteria**

- [ ] Define deterministic visibility of earlier Group outputs within the
  Group stage, or introduce explicit grouping passes.
- [ ] Detect and reject ambiguous or cyclic reparenting.
- [ ] Tests cover nested lists, nested sections, and two non-overlapping parent
  layers.
- [ ] MCIDs remain unique and unchanged through every parent layer.

## RRM-005: Table construction handles only regular, page-local grids

**Status:** Open

**Priority:** P0

Rows are currently grouped by rounding the vertical center to a fixed
six-point bucket. Inferred columns use a fixed twelve-point center clustering
tolerance. A matched claim is assigned to a column by its horizontal center,
and each matched claim becomes a separate `TD`/`TH`.

`TableOverFlattenedCells` removes intermediate leaf structure nodes; it does
not generally merge several claims or visual lines into one semantic cell.

**Impact**

- Wrapped multi-line cells may be treated as multiple rows.
- Multiple fragments in one column may become duplicate cells.
- Sparse rows, variable baselines, right-aligned numeric columns, and
  overlapping bounds can be misclassified.
- Tables do not continue as one structure across pages.
- There is no first-class representation of row spans, column spans, row
  headers, complex header associations, `THead`/`TBody`/`TFoot`, or repeated
  headers.

**Relevant code**

- `src/PdfLexer/Remediation/RemediationSession.cs`
  (`ApplyClaimConsumingTableClaim`, `GetRowKey`, `ResolveTableGrid`)
- `src/PdfLexer/Remediation/RemediationAction.cs`
  (`TableRemediationAction`, `TableCellContentMode`)

**Completion criteria**

- [ ] Row and column tolerances are explicit rule parameters or derived from
  document metrics.
- [ ] The rule model can identify several claims as content of one cell.
- [ ] Sparse and wrapped rows have deterministic behavior and diagnostics.
- [ ] `RowSpan` and `ColSpan` affect both structure attributes and grid
  occupancy.
- [ ] Row and column header relationships can be authored and validated.
- [ ] Multi-page table continuation and repeated header semantics are defined.
- [ ] Tests use irregular baselines, wrapped cells, missing cells, spans, and
  repeated headers.

## RRM-006: Table examples and validation can admit incorrect hierarchy

**Status:** Complete

**Priority:** P0

The complete invoice example now classifies header and body words as leaf
`Span` claims and flattens them into generated `TH`/`TD` cells. Preserve-child
table composition rejects prebuilt `TR`/`TH`/`TD` claims before mutation, and
strict validation requires every `TR` to have a table-related parent.

**Impact**

- The primary documentation can guide callers toward structurally incorrect
  tables.
- Internal strict validation may not catch a hierarchy that an external
  validator rejects.

**Relevant code**

- `docs/rule-based-remediation.md` (`Complete Invoice Example`)
- `src/PdfLexer/Remediation/RemediationSession.cs`
  (`ApplyClaimConsumingTableClaim`)
- `src/PdfLexer/PdfDocument.Accessibility.cs`
  (`ValidateStructureChildren`)

**Completion criteria**

- [x] Correct the example to classify cell content rather than prebuilt rows,
  or add a true row-consuming table mode.
- [x] Validate the allowed parent of `TR`, `TH`, and `TD`.
- [x] Add an end-to-end strict-conformance test for the documented example.
- [x] Add a veraPDF assertion for the resulting table hierarchy.

## RRM-007: No declarative Figure/caption association

**Status:** Open

**Priority:** P1

The Refine action set supports structure attributes, sibling reordering, and
links. It has no action that associates a `Caption` with a `Figure`, and the
candidate model cannot create a claim over an image in the first place.
`BeforeClaim`/`AfterClaim` select claims by ordering; they do not establish a
semantic figure-caption relationship.

**Completion criteria**

- [ ] Image/figure claims are supported by the declarative model.
- [ ] A caption action or grouping pattern creates a valid, documented
  hierarchy independent of content-stream ordering.
- [ ] Caption selection supports adjacency, bounded geometry, and explicit
  claim references.
- [ ] Tests cover captions painted before and after their image operators.

## RRM-008: Anchor and table geometry is not text-orientation-aware

**Status:** Open

**Priority:** P1

Page `/Rotate` values of 0/90/180/270 are normalized by
`StructuredPageSpace`, which is a current strength. Separately rotated text is
grouped with rotation metadata, but remediation candidates and relational
predicates ultimately compare axis-aligned bounding boxes for `RightOf`,
`Below`, `SameRowAs`, `SameColumnAs`, and table placement.

**Impact**

- Vertical sidebar text and 90/270-degree labels may match many unrelated rows
  or columns.
- Table inference does not operate in the text's inline/perpendicular axes.

**Completion criteria**

- [ ] Candidate orientation is exposed directly to remediation predicates.
- [ ] Row, column, direction, and distance predicates can evaluate in anchor
  orientation.
- [ ] Table row/column inference supports a declared or inferred writing
  direction.
- [ ] Tests distinguish rotated pages from independently rotated text.

## RRM-009: Interactive form widgets are outside the rule model

**Status:** Open

**Priority:** P1

The current "form-like" remediation fixture is painted text. The rule language
does not select AcroForm fields or widget annotations, create `Form` structure
elements around them, assign accessible names/tooltips, or reason about widget
tab order.

**Impact**

Static form appearances can be treated as text, but government, insurance, and
other interactive forms are not covered by the claimed use case.

**Completion criteria**

- [ ] Widget and form-field candidates are available to rules.
- [ ] Rules can set or validate accessible names and descriptions.
- [ ] Widget annotations receive correct structure bindings.
- [ ] Page `/Tabs` and logical widget order are validated.
- [ ] Tests cover text fields, check boxes, radio groups, and repeated widgets.

## RRM-010: External PDF/UA validation baseline is incomplete

**Status:** Partially addressed

**Priority:** P0

The OpenSpec task to run veraPDF over generated remediation fixtures and record
an expected-pass baseline remains unchecked. The design also explicitly treats
external tools as the conformance authority.

Most remediation engine and fixture tests use `StrictConformance = false`.
Internal diagnostics are valuable but do not constitute complete PDF/UA-1 or
PDF/UA-2 validation.

CI now installs checksum-pinned veraPDF 1.30.2 and checks a strict,
embedded-font PDF/UA-1 invoice table as an expected pass plus an intentionally
invalid `TD > TR` document as an expected failure. Expanding this gate to every
UA-1/UA-2 remediation fixture and recording PAC/manual review remain open.

**Relevant code and tracking**

- `openspec/changes/rule-driven-remediation/tasks.md` (task 18.6)
- `test/PdfLexer.Tests/RemediationFixtureGenerator.cs`
- `src/PdfLexer.pdfctl/Inspect/RemediateCmd.cs` (`--verapdf`)

**Completion criteria**

- [ ] All remediation fixtures run with strict conformance enabled unless a
  fixture explicitly tests permissive behavior.
- [ ] veraPDF is run for the intended UA profile of every fixture.
- [x] Expected-pass and expected-failure baselines are checked in CI for the
  strict invoice table and intentional invalid-hierarchy fixtures.
- [ ] PAC/manual assistive-technology review is recorded for representative
  documents where automated validation is insufficient.
- [ ] Task 18.6 is completed only after results are reproducible.

## RRM-011: Remediation coverage is tested primarily with synthetic PDFs

**Status:** Open

**Priority:** P1

The fixture corpus covers useful scenario names—invoice, statement, report,
form, multi-column, and mixed page sizes—but the inputs are generated by
PdfLexer using simple Helvetica text. Generic fixtures classify every remaining
line as `P`.

**Impact**

The tests demonstrate engine mechanics but not compatibility with common
producer behavior such as:

- fragmented `Tj`/`TJ` text;
- transformed and nested form XObjects;
- clipping and overprint;
- unusual font encodings;
- duplicated accessibility text;
- path-heavy tables;
- producer-specific content ordering;
- actual AcroForms and annotations.

**Completion criteria**

- [ ] Add licensed or redistributable fixtures from several independent PDF
  producers.
- [ ] Include text fragmentation, XObjects, graphics, annotations, complex
  fonts, multi-column order, and multi-page tables.
- [ ] Store expected claims and structure shape independently from the rules
  being tested.
- [ ] Validate outputs externally and visually compare rendering before/after.

## RRM-012: Multi-column reading order support is limited

**Status:** Open

**Priority:** P1

Flow selection defaults to the structured-text sequence. The declared
`FlowReadingOrderMode` is not used. Refine can reorder siblings by existing
reading order, global top-to-bottom geometry, or global left-to-right geometry,
but it has no column model.

**Impact**

- Top-to-bottom sorting can interleave adjacent columns.
- Left-to-right sorting can group rows across columns.
- The synthetic multi-column fixture verifies tagging integrity but not
  assistive-technology reading order.

**Completion criteria**

- [ ] Reading-order mode affects flow candidate selection.
- [ ] Rules can declare or infer column regions and order within/between them.
- [ ] Reordering is scoped to an explicit structural parent or region.
- [ ] Tests assert complete logical text order for two-column content with a
  sidebar, spanning heading, and footer.

## RRM-013: Declarative tag and JSON schema validation is incomplete

**Status:** Open

**Priority:** P1

`Tag`, `Group`, and `MergeTo` accept arbitrary structure names. Validation
checks action/stage compatibility but does not generally require a standard
structure type or a valid role mapping. Arbitrary attribute dictionaries can
also encode combinations the rule validator cannot understand.

The JSON loader recognizes the identifier
`pdflexer.remediation.ruleset.v1`, but the repository does not contain a
machine-readable JSON Schema for authoring validation and editor assistance.

**Impact**

- A typo in a structure tag can survive rule validation.
- JSON authoring lacks schema-based completion and early feedback.
- Validation guarantees differ between convenience actions and arbitrary
  attributes.

**Completion criteria**

- [ ] Standard tags are validated for the selected PDF/UA profile.
- [ ] Custom tags require an explicit valid role map.
- [ ] Parent/child compatibility is checked for every produced structure
  binding.
- [ ] Publish and test a JSON Schema for
  `pdflexer.remediation.ruleset.v1`.
- [ ] Unknown JSON properties and unsupported enum values fail with precise
  locations.

## RRM-014: Existing tagged PDFs cannot be repaired

**Status:** Intentional limitation

**Priority:** P2

`BeginRemediation` rejects any document with an existing `StructTreeRoot`.
This is documented and should not be treated as an implementation defect in
the current scope, but it excludes a large class of real remediation work:
repairing incomplete or incorrect existing tags.

**Revisit when**

- customers need incremental structure repair rather than full replacement;
- structure-tree parsing and safe MCID ownership are available;
- preservation behavior for existing tags, IDs, role maps, namespaces, links,
  and parent-tree entries is specified.

## RRM-015: Scanned PDFs require an external text/OCR layer

**Status:** Intentional limitation

**Priority:** P2

Text, anchor, font, and flow predicates depend on extractable structured text.
An image-only scan has no candidates for these rules, and the remediation
language does not perform OCR or ingest an external OCR/layout result.

**Revisit when**

- an external candidate-provider interface is designed;
- OCR text can be correlated to content images and page coordinates with
  confidence and provenance;
- invisible OCR text and image semantics can be validated without changing
  rendering.

---

## RRM-016: Rules have no expected-match cardinality, so template drift is silent

**Status:** Complete

**Priority:** P0

**Resolved 2026-07-26:** `RuleCardinality` now supports document- and page-scoped match
constraints, `RemediationReport.RuleEvaluations` includes zero-match rules and rejection counts, and
`RemediationReport.AutoArtifacts` identifies prospective and committed automatic artifacts.

`AnchorSelection` already gave anchors explicit cardinality: `RequiredSingle` fails when an anchor
resolves to zero or several candidates. Before this change, rules had no equivalent; `Rule` exposed
`MinConfidence` but no expected match constraint.

A rule without an explicit cardinality remains optional, so zero matches are still legitimate.
This is the defining failure mode of a template system: the producing application changes a label,
shifts a column, or reflows a block, and a regex that used to match stops matching. No diagnostic is
raised, because "zero candidates selected" is a legitimate outcome for optional content.

The interaction with `RemediationLeftoverPolicy.AutoArtifact` makes this actively harmful rather than
merely incomplete. Content that a rule should have tagged falls through to the leftover policy and is
marked as an artifact — that is, explicitly hidden from assistive technology. The resulting document
is valid PDF/UA and passes external validation with the invoice total, or the entire "Bill To" block,
silently unreadable.

The Authoring Guidance section currently recommends `AutoArtifact` "only when the remaining content is
known decorative or non-semantic." That guard cannot hold, because the whole point of drift is that
the author no longer knows what is left over.

**Impact**

- Layout drift degrades output quality with no signal at dry-run or commit.
- `AutoArtifact` converts unmatched semantic content into hidden content.
- Batch remediation cannot be run unattended with any confidence.
- Rule sets cannot be regression-tested against new document instances without manual inspection.

**Relevant code**

- `src/PdfLexer/Remediation/Rule.cs`
- `src/PdfLexer/Remediation/RemediationSession.cs`
  (`ApplyLeftoverPolicyAfterValidation`)
- `src/PdfLexer/Remediation/RemediationSessionConfiguration.cs`
  (`RemediationLeftoverPolicy`)

**Completion criteria**

- [x] `Rule` accepts an expected-match cardinality (at minimum `MinMatches`, ideally an explicit
  `Cardinality` mirroring `AnchorSelection`), evaluated per page or per document as declared.
- [x] Unsatisfied cardinality produces a diagnostic identifying the rule, page, and observed count.
- [x] Per-rule match counts appear in `RemediationReport` for every rule, including zero-match rules.
- [x] `AutoArtifact` reports which content it artifacted and how much, rather than silently absorbing
  it; consider requiring an explicit maximum artifacted area or candidate count.
- [x] Tests cover a rule whose predicate stops matching after a layout change, asserting that the
  run fails rather than producing hidden content.

## RRM-017: No rule-set applicability guard or document-family check

**Status:** Open

**Priority:** P0

The stated premise is remediation of "known transactional document families." Nothing in the model
binds a `RuleSet` to a family. There is no precondition predicate, no structural fingerprint, and no
declared version compatibility between a rule set and the layouts it was authored against.

`session.Use(ruleSet)` accepts any rule set for any document. Feeding an `invoice-v2` rule set a v3
layout produces confident, wrong output rather than a refusal.

This is distinct from RRM-011, which concerns the fixtures used to test the engine. RRM-017 is a
runtime safety concept: the engine should be able to decline a document it does not recognize.

**Impact**

- A silent producer-side template change mis-tags an entire batch.
- Callers must implement family detection themselves, outside the rule language, duplicating the
  anchor logic the rule set already contains.
- There is no way to express "this rule set requires these anchors to resolve" as a precondition
  rather than as a mid-run failure.

**Relevant code**

- `src/PdfLexer/Remediation/RuleSet.cs`
- `src/PdfLexer/Remediation/RemediationSession.cs` (`Use`, `DryRun`, `Commit`)

**Completion criteria**

- [ ] `RuleSet` accepts an applicability predicate or required-anchor set evaluated before rule
  evaluation begins.
- [ ] A non-applicable rule set produces a distinct, actionable diagnostic rather than a cascade of
  anchor failures.
- [ ] Where several rule sets are registered, applicability can select among them deterministically,
  and ambiguity is diagnosed.
- [ ] The applicability check is expressible in the JSON rule format.
- [ ] Tests cover a matching family, a near-miss variant, and an unrelated document.

## RRM-018: No semantic output assertions beyond conformance validation

**Status:** Open

**Priority:** P0

External validation answers whether the output is well-formed PDF/UA. It cannot answer whether the
output is *correct*. A document in which the invoice total was tagged as a pagination artifact and
the footer was tagged `H1` is fully conformant.

The rule language has no way to express expectations about the shape of the result: "exactly one
`H1`", "at least one `Table`", "the `invoice-number` rule produced exactly one claim on page 1",
"every `TR` has the same number of `TD` children as the header row".

The MVP exit gate covers this with human review ("one reviewed rule set handles those variants").
That does not scale to production batches and does not survive template drift.

**Impact**

- Conformance passes are mistaken for correctness passes.
- Regression testing a rule set against new document instances requires manual inspection.
- RRM-016 and RRM-017 have no automated acceptance surface to report into.

**Relevant code**

- `src/PdfLexer/Remediation/RemediationReport.cs`
- `src/PdfLexer/Remediation/RemediationSession.cs` (diagnostic pass)

**Completion criteria**

- [ ] A declarative assertion set can be attached to a rule set and evaluated at dry-run and commit.
- [ ] Assertions cover claim counts per rule, structure-element counts per tag, parent/child shape,
  and page-scoped variants of each.
- [ ] Assertion failures are diagnostics with the same provenance detail as rule diagnostics.
- [ ] Assertions are expressible in the JSON rule format.
- [ ] Tests cover an assertion that catches a mis-tagged total which external validation accepts.

## RRM-019: Annotations already present in the input are outside the rule model

**Status:** Open

**Priority:** P0

RRM-009 covers interactive AcroForm widgets. This item covers every other annotation already present
in the untagged input.

Untagged PDFs produced by common office and layout applications routinely carry real `Link`
annotations, and frequently `Stamp`, `FileAttachment`, `Popup`, and markup annotations. ISO 14289-1
§7.18 requires every annotation to be represented in the structure tree or marked as an artifact.

`RemediationActions.Link` creates a *new* structure link and a *new* link annotation between two
claims. There is no candidate type, predicate, or action that adopts an annotation that already
exists in the document. A remediated file therefore retains its original untagged annotations and
fails conformance regardless of how completely the text was tagged.

A secondary concern: because `Link` creates a new annotation rather than binding an existing one, a
rule set that adds links introduces visual change (annotation borders). That conflicts with the MVP
exit gate requiring "no unintended visual change."

**Impact**

- Documents with pre-existing hyperlinks cannot reach PDF/UA conformance through the rule language.
- The most common real-world annotation case — a Word or InDesign export with live links — is
  uncovered.
- Link creation may alter rendering in a way the exit gate forbids.

**Relevant code**

- `src/PdfLexer/Remediation/RemediationAction.cs`
  (`StructureLinkRemediationAction`)
- `src/PdfLexer/Remediation/RemediationSession.cs` (link application,
  `BindAnnotation`)
- `src/PdfLexer/DOM/Annotations.cs` (`AnnotationFactory`)

**Completion criteria**

- [ ] Existing annotations are exposed as candidates with subtype, rect, page, and destination
  available to predicates.
- [ ] An action binds an existing annotation to a `Link`, `Form`, or `Annot` structure element
  without creating a replacement annotation.
- [ ] An action artifacts or otherwise accounts for annotations that carry no semantics.
- [ ] Diagnostics report any annotation left with no structure binding at commit.
- [ ] Link creation documents and, where possible, avoids visual change.
- [ ] Tests cover a document with pre-existing links, a stamp, and a popup.

## RRM-020: Pre-existing marked content and optional content are unmodeled

**Status:** Open

**Priority:** P1

"Untagged" is treated throughout as equivalent to "contains no marked content." That does not hold.
A document with no `StructTreeRoot` can still contain `BDC`/`EMC` sequences: existing `/Artifact`
marks, `/OC` optional-content membership, and producer-specific tags, along with `/MCID` values that
were never referenced by a structure tree.

The model has no concept of this. There is no predicate to select or exclude content already inside a
marked-content sequence, no stated policy on whether new scopes nest inside existing ones, and MCID
allocation begins at zero without reconciling against identifiers already present in the content
stream.

Optional content is the sharper case. Content inside an `/OC` sequence may be hidden by default —
watermarks, print-only blocks, alternate-language layers. Tagging hidden content makes it audible to
a screen reader; artifacting visible content hides it. There is no `InOptionalGroup` predicate and no
policy for either direction.

**Impact**

- New marked-content scopes may nest inside existing ones in ways that change what assistive
  technology encounters when a layer is toggled.
- MCID collisions with pre-existing identifiers are possible.
- Watermark and layered documents cannot be handled deliberately.
- Content already marked `/Artifact` by the producer may be re-tagged as real content, or
  double-counted by the leftover policy.

**Relevant code**

- `src/PdfLexer/Remediation/PageRemediationState.cs` (`_nextMcid`)
- `src/PdfLexer/Remediation/RemediationSession.cs` (MCID allocation,
  `CheckUntaggedContent`)
- `src/PdfLexer/Remediation/RemediationCandidate.cs`

**Completion criteria**

- [ ] Candidates expose whether they sit inside an existing marked-content sequence, and which.
- [ ] Predicates can select or exclude content by existing tag and by optional-content membership.
- [x] MCID allocation is seeded from the highest identifier already present on the page.
- [ ] A documented policy governs nesting inside pre-existing sequences, including `/Artifact`.
- [ ] Optional-content handling is either supported or diagnosed as unsupported, not silently ignored.
- [ ] Tests cover a producer artifact mark, an `/OC` watermark layer, and pre-existing MCIDs.

## RRM-021: List interior structure and `/ListNumbering` are not expressible

**Status:** Open

**Priority:** P1

The documented list pattern is `Group("L", ClaimIs("LI").And(Consecutive()))`. Two things are missing
from that shape.

First, there is no action that splits a claim. In a rendered PDF a list item is usually one line —
`"1. Widget assembly"` — and PDF/UA expects `LI` to contain `Lbl` and `LBody`. `Group` reparents
whole claims and `MergeTo` flattens several claims into one; neither can divide a single claim into a
label and a body. The alternative, classifying at `Word` granularity and grouping, does not generalise
to multi-word bodies.

Second, `/ListNumbering` cannot be set. It is required on ordered lists by PDF/UA-1, and
`StructureListNumbering` is not reachable from any refine action. `RemediationActions.Attributes`
writes a raw attribute dictionary, which does not populate `StructureNode.ListNumbering` and would
compete with the dictionary the serializer generates.

RRM-004 covers nested lists. This covers the interior of a single flat list.

**Impact**

- Ordered lists are emitted without required numbering semantics.
- List labels and bodies cannot be distinguished, so bullet and number glyphs are announced as
  content.
- The documented list example produces a structure that external validation will flag.

**Relevant code**

- `src/PdfLexer/Remediation/RemediationAction.cs`
  (`GroupRemediationAction`, `MergeRemediationAction`,
  `StructureAttributeRemediationAction`)
- `src/PdfLexer/DOM/StructureNode.cs` (`ListNumbering`)

**Completion criteria**

- [ ] A split or label-extraction action can divide a leaf claim into `Lbl` and `LBody`, driven by a
  pattern or a geometric boundary.
- [ ] `/ListNumbering` is settable as a first-class refine action for the selected profile.
- [ ] The documented list example produces `L -> LI -> {Lbl, LBody}` with numbering.
- [ ] Tests cover decimal, alpha, and bulleted lists, including a wrapped multi-line item.

## RRM-022: Artifact subtypes cannot express Header/Footer/Watermark

**Status:** Open

**Priority:** P1

`ArtifactSubtype` offers `Pagination`, `Layout`, `Page`, and `Background` — the `/Type` values.
PDF/UA-1 §7.8 additionally expects running headers and footers to carry an artifact `/Subtype` of
`/Header` or `/Footer`, and watermarks `/Watermark`. `/BBox` and `/Attached` are likewise not
expressible.

The guide's own footer rule, `Artifact(ArtifactSubtype.Pagination)`, therefore cannot produce the
markup the specification asks for, and running heads and feet are the single most common artifact in
transactional documents.

This gap is shared with the imperative authoring API (`PageWriter.BeginArtifact`), so the underlying
fix serves both surfaces. See `accessibility_gaps_2.md` finding 7.

**Impact**

Every remediated document with a running header or footer — effectively all of them — carries
incomplete artifact markup.

**Relevant code**

- `src/PdfLexer/Remediation/RemediationAction.cs` (`ArtifactSubtype`)
- `src/PdfLexer/Writing/PageWriter.cs` (`BeginArtifact`)

**Completion criteria**

- [ ] Artifact actions express `/Type`, `/Subtype`, `/BBox`, and `/Attached`.
- [ ] Convenience forms exist for header, footer, and watermark artifacts.
- [ ] The underlying writer emits the full artifact property list.
- [ ] Tests assert the emitted artifact dictionary for a running header and footer.

## RRM-023: Heading levels are literal with no ordering model

**Status:** Open

**Priority:** P1

Heading rules carry a fixed tag: `Tag("H1")`, `Tag("H2")`. In a document family where a section is
conditionally present, the correct level for a given heading varies by document instance. PDF/UA
requires headings to descend without skipping levels and to begin at `H1`.

There is no relative-level action, no automatic numbering derived from nesting, and no normalization
pass over the finished tree. A rule set that emits `H1` and `H3` because the `H2` section was absent
from this instance produces a conformance failure that the engine does not detect — the authoring
strict-mode validator does not check heading order either (see `accessibility_gaps_2.md` finding 6).

**Impact**

- Optional sections break heading hierarchy in a subset of document instances.
- The failure is instance-dependent, so it survives testing against a single sample.

**Relevant code**

- `src/PdfLexer/Remediation/RemediationAction.cs` (`TagRemediationAction`)
- `src/PdfLexer/PdfDocument.Accessibility.cs`
  (`ValidateStructureChildren`)

**Completion criteria**

- [ ] A heading action can express a level relative to the enclosing structural parent, or a
  normalization pass renumbers headings to remove skips.
- [ ] Strict mode diagnoses skipped levels, a first heading that is not `H1`, and levels outside the
  range valid for the target profile.
- [ ] Tests cover a family variant in which an intermediate section is absent.

## RRM-024: No document-level navigation actions

**Status:** Open

**Priority:** P1

`RemediationSessionConfiguration` covers language, title, profile, strictness, leftover policy, zone
margins, default confidence, and debug write. Two document-level PDF/UA concerns have no
representation in either the configuration or the rule language:

- **Outline / bookmarks.** No action derives a document outline from heading claims. PDF/UA-1 §7.17
  expects an outline for documents of substance, and the imperative API already supports
  `CreateBookmark` from a structure element.
- **Page labels.** No configuration or action sets `/PageLabels`. This is not merely a nicety: the
  library's own strict validator *requires* `PageLabels` on a multi-page document containing a `TOC`
  element. A rule set that tags a table of contents will therefore fail commit with no declarative
  remedy available.

**Impact**

- Long remediated documents lack navigation structure.
- Tagging a table of contents can make a document uncommittable under the default strict setting.

**Relevant code**

- `src/PdfLexer/Remediation/RemediationSessionConfiguration.cs`
- `src/PdfLexer/PdfDocument.Accessibility.cs` (`ValidateNavigation`)
- `src/PdfLexer/DOM/OutlineBuilder.cs`

**Completion criteria**

- [ ] An action or configuration option generates an outline from selected heading claims, with
  declared level mapping.
- [ ] Page labels can be configured or derived.
- [ ] The `TOC` + `PageLabels` strict-mode interaction is either satisfied automatically or diagnosed
  with a message that names the remedy.
- [ ] Tests cover a multi-page document with a tagged TOC.

## RRM-025: Table `/Scope` is hardcoded and `/Summary` is unreachable

**Status:** Open

**Priority:** P1

Header cells produced by the table actions are assigned `StructureScope.Column` unconditionally. Row
headers cannot be expressed. `/Summary` is not reachable from any action, and `/Headers`
associations, as noted in RRM-005, have no declarative form.

RRM-005 lists row and column header relationships among its completion criteria, so this overlaps.
It is recorded separately because the hardcoded scope is a distinct, small defect that can be fixed
independently of the larger table-model work, and because `/Summary` is not mentioned there.

**Relevant code**

- `src/PdfLexer/Remediation/RemediationSession.cs`
  (`ApplyClaimConsumingTableClaim`, cell construction)

**Completion criteria**

- [ ] Header scope is declarable per header selector, including `Row` and `Both`.
- [ ] `/Summary` is settable on a table claim.
- [ ] Scope defaults are documented rather than implicit.

## RRM-026: Input-level conformance defects cannot be repaired or diagnosed

**Status:** Open

**Priority:** P0

Remediation adds structure. It does not alter the content stream's text, fonts, or encoding. Several
PDF/UA requirements live below the structure layer and are therefore outside what any rule set can
fix — but this boundary is stated nowhere, and there is no diagnostic that surfaces it.

- **Fonts.** The output reuses the input's fonts. If a font is not embedded, has no `ToUnicode`, or
  references `.notdef`, the document fails PDF/UA no matter how good the tagging is. Worse,
  `StrictConformance` defaults to `true` and rejects non-embedded fonts at save, so a perfectly
  authored rule set will fail commit with an error the rule language offers no way to address.
- **Word boundaries.** Producers that space words with `TJ` offsets rather than space characters
  yield text that fails PDF/UA §7.2 and extracts as `InvoiceNumber`. Wrapping that content in a
  marked-content scope does not fix it.

Both are near-certain on first contact with real third-party PDFs. RRM-011 will surface them
empirically; this item is about declaring the boundary and diagnosing it early rather than
discovering it at commit.

**Impact**

- A rule set can be complete and correct and still produce a non-conformant document.
- The failure appears as a save-time exception with no obvious connection to the input document.
- Callers have no pre-flight check to determine whether a document is remediable at all.

**Relevant code**

- `src/PdfLexer/PdfDocument.Accessibility.cs`
  (`ValidateRenderedContent`, `IsFontEmbedded`)
- `src/PdfLexer/Remediation/RemediationSession.cs` (commit path)

**Completion criteria**

- [ ] A pre-flight document check reports font embedding, `ToUnicode` coverage, `.notdef` usage, and
  space-character heuristics before rules are evaluated.
- [ ] Findings are diagnostics that name the font or page, not save-time exceptions.
- [ ] The documentation states explicitly which conformance requirements remediation cannot address.
- [ ] Decide and document whether font embedding/subsetting repair is in scope; if it is, track it as
  its own item.

## RRM-027: The confidence model is undefined and `MinConfidence` is unusable by default

**Status:** Open

**Priority:** P1

Confidence appears throughout the model — `Rule.MinConfidence`, `RemediationClaimOutcome.Confidence`,
`RemediationSessionConfiguration.DefaultConfidence`, toleranced-zone "degraded confidence", and a
"low-confidence inference" diagnostic — but nothing defines it. There is no documented scale
semantics, no statement of what produces a confidence value, no description of how tolerance degrades
it, and no rule for how `And`, `Or`, and `Not` combine the confidences of their operands.

`DefaultConfidence` defaults to `0.0`. Since it applies to "matches that do not compute a confidence
explicitly," any rule that sets `minConfidence` will reject every such match. As shipped and
documented, the feature cannot be used without reading the implementation.

**Impact**

- `MinConfidence` is a trap rather than a tuning knob.
- Toleranced zones cannot be tuned, because the effect of `Tolerance` on confidence is unspecified.
- Low-confidence diagnostics are not actionable.

**Relevant code**

- `src/PdfLexer/Remediation/PredicateResult.cs`
- `src/PdfLexer/Remediation/RemediationPredicate.cs` (composition)
- `src/PdfLexer/Remediation/TolerancedZone.cs`
- `src/PdfLexer/Remediation/RemediationSessionConfiguration.cs`
  (`DefaultConfidence`)

**Completion criteria**

- [ ] The confidence scale, its sources, and its combination rules are specified and documented.
- [ ] Every predicate either computes a confidence or is documented as confidence-neutral.
- [ ] `DefaultConfidence` has a defensible default, or rules with `MinConfidence` reject
  confidence-neutral predicates with a validation error rather than silently matching nothing.
- [ ] Reports expose which predicate contributed a degraded confidence.

## RRM-028: Anchor resolution scope is per page but documented as absolute

**Status:** Open

**Priority:** P1

`AnchorSelection.RequiredSingle` is documented as "exactly one match is required." `AnchorResolver`
evaluates anchors against the current page. The actual contract is therefore "exactly one match *on
this page*", which behaves differently in three ways a reader would not predict: a label repeated on
every page is fine; the same label twice on one page fails; and a label absent from an intermediate
page fails unless `Pages` is narrowed.

`RemediationAnchor.Pages` exists and defaults to `PageSelector.Every`, but is not documented at all.

**Impact**

- Anchor failures on intermediate pages read as template errors when they are scope errors.
- Rule authors cannot express a document-scoped anchor.
- Interacts with RRM-001: a continuation model needs a defined anchor scope first.

**Relevant code**

- `src/PdfLexer/Remediation/AnchorResolver.cs`
- `src/PdfLexer/Remediation/RemediationAnchor.cs` (`Pages`)

**Completion criteria**

- [ ] Anchor scope is documented explicitly, per selection mode.
- [ ] `RemediationAnchor.Pages`, `.Occurrence`, and `.Style` are documented and covered by examples.
- [ ] The overlap between `Occurrence` (one-based) and `AnchorSelection.NthInReadingOrder`
  (zero-based) is resolved: pick one mechanism, or align the bases and document the ordering in which
  the filters apply.
- [ ] Document-scoped anchors are supported or explicitly rejected at validation.
- [ ] Diagnostics distinguish "no match on this page" from "ambiguous match on this page."

## RRM-029: Structure sibling order and the reading-order default are unspecified

**Status:** Open

**Priority:** P1

Reading order is structure-tree order. Nothing in the documentation states how sibling order is
determined: rule declaration order, content-stream order, geometry, or claim creation order. The
existence of `ReorderSiblings` implies the default is sometimes wrong, but the default itself is
never defined, so an author cannot tell when the action is needed.

RRM-012 covers multi-column reading order specifically. This is the more basic contract: for a
single-column page, what determines whether the footer claim precedes the title claim in the tree?

**Impact**

- Rule authors cannot reason about reading order without experimentation.
- `ReorderSiblings` is applied defensively rather than deliberately.
- The reading-order-drift diagnostic reports a violation of a rule that is not written down.

**Relevant code**

- `src/PdfLexer/Remediation/RemediationSession.cs`
  (claim ordering, structure construction, `ReadingOrderDrift`)

**Completion criteria**

- [ ] The default sibling ordering is specified and documented.
- [ ] Ordering is stable across runs and independent of rule declaration order, or the dependence is
  documented as intentional.
- [ ] The reading-order-drift diagnostic references the documented default.
- [ ] Tests assert tree order for claims created by rules declared out of visual order.

## RRM-030: `DryRun` does not guarantee `Commit`; failure semantics are undocumented

**Status:** Open

**Priority:** P1

The documented workflow is `if (report.Diagnostics.Count == 0) session.Commit();`, and the same
section states that `Commit()` reevaluates. Nothing states whether the reevaluation can produce
diagnostics the dry run did not, what happens when it does, or what state the `PdfDocument` is left
in on failure.

The gap tracker asserts "atomic commit behavior" among the engine's strengths. That guarantee, if it
holds, belongs in the user-facing contract; if it does not hold in some cases, those cases need
naming.

**Impact**

- The recommended workflow implies a guarantee that is not stated.
- Callers cannot tell whether a failed commit leaves a reusable document or requires a reopen.
- Retry and error-handling code cannot be written correctly against the documentation.

**Relevant code**

- `src/PdfLexer/Remediation/RemediationSession.cs` (`DryRun`, `Commit`)

**Completion criteria**

- [ ] The relationship between dry-run and commit results is specified.
- [ ] Commit failure semantics — throw, partial application, rollback, document state — are
  documented.
- [ ] If commit is atomic, a test demonstrates that a failed commit leaves the document unmodified.
- [ ] Repeated `Commit()` calls have defined behavior.

## RRM-031: Rule-set composition and precedence are undefined

**Status:** Open

**Priority:** P1

`Use()` accepts multiple rule sets and appends them. `Validate`, `DryRun`, and `Commit` additionally
accept ad-hoc rules that are concatenated with the registered sets.
`ClaimPredicates.FromRuleSet(...)` implies cross-set references are an intended pattern.

None of this is documented. Evaluation order across sets, precedence, and the meaning of `Override`
across a set boundary are all unspecified. `Rule.Override` is documented on the type as replacing
claims "produced by earlier rules in the same stage", but the user guide says only "earlier claims
over the same target" — and neither defines what "earlier" means once several rule sets are involved.

Layering a shared "common artifacts and boilerplate" set beneath a per-family set is the obvious use
case, and nothing confirms it is supported.

**Impact**

- Rule-set reuse across document families is not a documented capability despite the API supporting it.
- `Override` behavior is unpredictable in composed configurations.

**Relevant code**

- `src/PdfLexer/Remediation/RemediationSession.cs` (`Use`, `_ruleSets`,
  composition in `Validate`/`DryRun`/`Commit`)
- `src/PdfLexer/Remediation/Rule.cs` (`Override`, `RuleSetId`)

**Completion criteria**

- [ ] Composition order and precedence are specified and documented.
- [ ] `Override` semantics across rule-set boundaries are defined.
- [ ] Duplicate rule ids across composed sets are validated.
- [ ] Tests cover a shared base rule set layered under two different family rule sets.

## RRM-032: Text normalization for predicate matching is unspecified

**Status:** Open

**Priority:** P0

`Predicates.Text.Matches`, `Contains`, `StartsWith`, and `Equals` operate on extracted structured
text. Nothing documents what normalization, if any, is applied. Only `Contains` is shown with a
`StringComparison`.

Real producer output routinely contains ligatures, soft hyphens, non-breaking spaces, U+2011
non-breaking hyphens, combining marks in non-normalized order, and runs of spaces synthesized from
`TJ` offsets. A rule written as `Matches(@"^INV-\d+$")` against a document whose extracted text uses
a non-breaking hyphen simply does not match — and by RRM-016, does not report that it did not match.

This is invisible in the current synthetic fixtures, which are generated by PdfLexer from simple
Helvetica text. RRM-011 will expose it; the fix is a language feature rather than a test.

**Impact**

- Rules are brittle in ways that correlate with the producing application rather than the layout.
- Diagnosis is difficult because the extracted text looks correct when printed.
- Combined with RRM-016, a normalization mismatch degrades output silently.

**Relevant code**

- `src/PdfLexer/Remediation/RemediationPredicate.cs` (text predicates)
- `src/PdfLexer/Content/StructuredWord.cs`,
  `src/PdfLexer/Content/StructuredLine.cs`

**Completion criteria**

- [ ] A documented default normalization is applied to candidate text before matching, covering
  Unicode normal form, whitespace collapsing, and hyphen/dash/quote folding.
- [ ] Normalization is configurable per rule set and per predicate where the default is wrong.
- [ ] Reports expose both the raw and normalized text for a candidate.
- [ ] Tests cover ligatures, soft hyphens, non-breaking hyphens and spaces, and `TJ`-derived spacing.

## RRM-033: No negative explain or per-rule match diagnostics

**Status:** Partially addressed

**Priority:** P1

`RemediationReport.Explain(...)` accepts a source reference, candidate, or structured character and
returns the claim outcomes that touched it. That is useful and currently undocumented.

It answers "which rules affected this content." It cannot answer the question a rule author actually
has: "why did my rule match nothing", or "which conjunct of this `And` chain rejected this
candidate". For a language whose entire authoring loop consists of tuning composed predicates, that
is the missing tool — and it is the direct enabler for diagnosing RRM-016 and RRM-032.

**Impact**

- Authoring a rule set is a guess-and-check loop against a black box.
- The most common authoring question has no supported answer.

**Relevant code**

- `src/PdfLexer/Remediation/RemediationReport.cs` (`Explain`)
- `src/PdfLexer/Remediation/PredicateResult.cs`
- `src/PdfLexer/Remediation/RemediationPredicate.cs`

**Completion criteria**

- [ ] A trace mode records per-predicate evaluation outcomes for selected candidates or a selected
  page.
- [ ] Composed predicates report which operand rejected a candidate.
- [x] Per-rule evaluation counts — candidates considered, matched, rejected by confidence, rejected by
  conflict — appear in the report for every rule.
- [ ] `Explain` is documented, and a negative form is available from the CLI.

## RRM-034: Predicate evaluation cost is unbounded

**Status:** Open

**Priority:** P2

Rule sets are data and can be loaded from JSON. Validation rejects invalid regexes but not
catastrophically backtracking ones, and no match timeout is applied. Anchor resolution runs per page
against the page's candidate set, and no complexity or scale guidance is documented for large
documents.

This is low severity while rule sets are authored by the same team that runs them. It becomes a real
concern as soon as rule sets are treated as configuration supplied by another party.

**Relevant code**

- `src/PdfLexer/Remediation/RemediationPredicate.cs`
- `src/PdfLexer/Remediation/SerializedRemediationRules.cs`
- `src/PdfLexer/Remediation/AnchorResolver.cs`

**Completion criteria**

- [ ] Regex evaluation uses a bounded timeout, and validation rejects patterns that exceed a
  complexity budget.
- [ ] Overall evaluation has an optional time or work budget with a clear diagnostic on exhaustion.
- [ ] Documented scale guidance covers page count, candidate count, and anchor count.

## RRM-035: Documented rule language omits shipped API surface

**Status:** Partially addressed

**Priority:** P1

Several public capabilities were absent from `rule-based-remediation.md`. Some are conveniences; two
materially change how the engine's safety story should be read.

| Omitted | Why it matters |
| --- | --- |
| `session.Suppress(code, scope, reason)` and `RemediationDiagnosticStrictness.Permissive` | A diagnostic suppression mechanism. Undocumented, and exactly what a team reaches for under deadline pressure. Suppressions are surfaced on `RemediationReport.Suppressions`, but nothing tells a reviewer to look. |
| `RemediationActions.Custom(handler, description)` | An arbitrary delegate escape hatch that cannot round-trip through the JSON rule format, which quietly undercuts the portable-rule-set premise RRM-013 builds on. |
| `RemediationSessionConfiguration.StrictConformance` | Defaults to `true` and is the most common reason a commit fails, yet was absent from the configuration example. |
| `RemediationNamedZoneMargins` | `NamedLayoutZone.Header` silently means "top 72 points". A document with a 40-point header, or a non-Letter page size, mis-zones with no signal. |
| `RemediationAnchor.Pages` | See RRM-028. |
| `RemediationAnchor.Occurrence` and `.Style` | Two further undocumented disambiguation mechanisms. `Occurrence` is **one-based** while the documented `AnchorSelection.NthInReadingOrder(n)` is **zero-based**, and both select an occurrence — an off-by-one trap with two spellings. Their interaction order is unspecified. |
| `RemediationActions.ActualText`, `Expansion`, `Attributes` | Refine actions that exist but were not listed. |
| `RemediationReport.Explain(...)` | See RRM-033. |

**Completion criteria**

- [x] Document the omitted surface in `rule-based-remediation.md`.
- [x] Document the `AutoArtifact` hazard described in RRM-016 alongside the leftover policy.
- [ ] Establish a policy requiring every `Suppress` call to carry a reviewed justification, and
  surface active suppressions in CLI output.
- [ ] Mark `Custom` explicitly as non-serializable, and validate that a rule set containing it cannot
  be written to JSON silently.
- [ ] Add a documentation test or review step so future public API additions cannot ship undocumented.

---

## Suggested delivery order

The dependency reasoning below is scheduled into milestones with entry and exit gates in the
[Delivery Plan](rule-based-remediation-plan.md). Where the two differ, the plan is the operative
schedule — it also folds in two authoring-layer prerequisites tracked in
[accessibility_gaps_2.md](accessibility_gaps_2.md).

0. **Assurance first.** RRM-016 rule cardinality, RRM-018 output assertions, and RRM-033 per-rule
   match diagnostics. These are small relative to the rest and they are what makes every later item
   verifiable rather than merely implemented. Until a rule that stops matching produces a failure,
   no other fix can be trusted in a batch.
1. RRM-032 text normalization and RRM-026 input conformance pre-flight. Both are near-certain first
   contact failures with real third-party PDFs, and both are cheaper to fix before RRM-011 rather
   than as fallout from it.
2. RRM-003 regression tests and RRM-006 table hierarchy correction.
3. RRM-010 external validation baseline.
4. RRM-027 through RRM-031 contract specification. Mostly documentation and small decisions, but
   they unblock everyone authoring rules and several are prerequisites for later work
   (RRM-028 gates RRM-001).
5. RRM-002 non-text selection and artifact handling, with RRM-019 existing annotations and RRM-022
   artifact subtypes. These three together are what "all content is accounted for" actually requires.
6. RRM-001 multi-page flow and RRM-005 multi-page/complex tables, with RRM-025.
7. RRM-017 rule-set applicability, once enough real families exist to know what a fingerprint
   should assert.
8. RRM-020 pre-existing and optional content; RRM-021 list interiors; RRM-023 heading levels;
   RRM-024 navigation.
9. RRM-004 nested grouping and RRM-012 multi-column reading order.
10. RRM-007 through RRM-009 semantic expansion.
11. RRM-013 schema and validation hardening, plus the remaining RRM-035 policy items.
12. RRM-034 evaluation limits, when rule sets become externally supplied.
13. Revisit intentional limitations RRM-014 and RRM-015 only when product scope
    expands.

## Definition of credible enterprise coverage

The rule language should not be described as having complete enterprise
transactional coverage until, at minimum:

- all P0 items are closed;
- representative third-party PDFs pass rendering comparison and external
  PDF/UA validation;
- non-text painting content is either tagged or artifacted;
- annotations already present in the input are tagged or artifacted;
- multi-page tables and sections have defined logical structure;
- the same rule set is demonstrated across meaningful variations from more
  than one PDF producer;
- a rule set that stops matching its target layout fails loudly rather than
  producing degraded output;
- output correctness is asserted by something other than human review;
- remaining limitations are documented as explicit unsupported cases.

## A note on what conformance validation proves

Several items above — RRM-016, RRM-017, RRM-018, RRM-023, RRM-032 — share a single underlying
observation, and it is worth stating plainly because it shapes how the veraPDF baseline in RRM-010
should be interpreted.

External validation proves the output is well-formed PDF/UA. It does not prove the output is right.
A document in which the invoice total was swept into an artifact by the leftover policy, or the
footer was tagged `H1`, passes every automated conformance check available. The engine's own
diagnostics are structural too: orphaned MCIDs, missing `/StructParents`, reading-order drift.

Nothing in the current design asserts that the *semantics* the rule author intended are the semantics
the document ended up with. For a system whose purpose is to make documents readable by people who
cannot see them, that is the gap that matters most, and it is not closed by finishing RRM-010.
