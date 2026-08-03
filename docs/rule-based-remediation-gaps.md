# Rule-Based Remediation Gap Tracker

Last reviewed: 2026-07-31

> [!NOTE]
> This is the gap **register** — what is missing and why it matters. For the scheduled version
> (milestones, dependency order, and exit gates) see the
> [Rule-Based Remediation Delivery Plan](rule-based-remediation-plan.md). Gap status is tracked
> here, not there.
>
> RRM-039 through RRM-041 are specified together in the
> [Structural Model design](rule-based-remediation-structural-model.md), which also records how they
> change RRM-004, RRM-016, RRM-017, and RRM-018.
>
> RRM-042 through RRM-046 follow from the prescriptive-only decision and are specified in
> [Architecture and Direction](rule-based-remediation-architecture.md). They are the declarations a
> dynamic document family needs once the template owns structure.

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
| RRM-001 | P0 | Complete | Flow regions do not continue across pages |
| RRM-002 | P0 | Complete | Declarative rules cannot select or artifact non-text content |
| RRM-003 | P0 | Complete | Multiple inline claims in one text operator may be order-sensitive |
| RRM-004 | P0 | Complete | Explicit numbered Group passes support deterministic structural composition |
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
| RRM-018 | P0 | In progress | No semantic output assertions beyond conformance validation |
| RRM-019 | P0 | Complete | Existing annotations are selectable, adoptable, destination-aware, and fully inventoried |
| RRM-020 | P1 | Open | Pre-existing marked content and optional content are unmodeled |
| RRM-021 | P1 | Open | List interior structure and `/ListNumbering` are not expressible |
| RRM-022 | P1 | Complete | Artifact property lists express Header/Footer/Watermark, bounds, and attachment edges |
| RRM-023 | P1 | Open | Heading levels are literal with no ordering model |
| RRM-024 | P1 | Open | No document-level navigation actions (outline, page labels) |
| RRM-025 | P1 | Open | Table `/Scope` is hardcoded and `/Summary` is unreachable |
| RRM-026 | P0 | Open | Input-level conformance defects cannot be repaired or diagnosed |
| RRM-027 | P1 | Complete | The confidence model is undefined and `MinConfidence` is unusable by default |
| RRM-028 | P1 | Complete | Anchor resolution scope is per page but documented as absolute |
| RRM-029 | P1 | Complete | Structure sibling order and the reading-order default are unspecified |
| RRM-030 | P1 | Complete | `Commit` is all-or-nothing with checkpointed rollback |
| RRM-031 | P1 | Open | Rule-set composition and precedence are undefined |
| RRM-032 | P0 | Complete | Text normalization for predicate matching is unspecified |
| RRM-033 | P1 | In progress | No negative explain or per-rule match diagnostics |
| RRM-034 | P2 | Open | Predicate evaluation cost is unbounded |
| RRM-035 | P1 | Partially addressed | Documented rule language omits shipped API surface |
| RRM-036 | P0 | Open | Geometric tolerances are authored constants with no calibration |
| RRM-037 | P0 | Open | No forward authoring inspection of what the engine sees |
| RRM-038 | P1 | Open | Serialized schema is a projection of the C# API, not the contract |
| RRM-039 | P0 | Complete | Descriptive validation and prescriptive slot-owned materialization |
| RRM-040 | P0 | Complete | Expected page furniture is not declarable, so artifacting is unbounded |
| RRM-041 | P1 | Open | Recurring predicate logic cannot be named or reused |
| RRM-042 | P0 | Partial | Preview programs declare and execute occurrence boundaries; legacy flow activation and real-producer validation remain open |
| RRM-043 | P1 | Complete | Region declarations are fragmented and do not compose |
| RRM-044 | P1 | Partial | Preview nodes support language, alternate text, actual text, and expansion; tag-specific attributes remain open |
| RRM-045 | P0 | Complete | Per-item leftover inventory and guarded region absorption are complete |
| RRM-046 | P0 | Partial | Preview order guardrail is implemented; real-producer and richer-layout validation remain open |

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

**Multi-page flow — decided 2026-07-28: in scope.** This was left open as a scope decision
(defer RRM-001 if every page repeats complete boundaries; require it if a logical section or table
begins on one page and ends on another). The decision is that cross-page spanning is a requirement:
page-local evaluation only works when every page is self-contained, which excludes multi-page tables,
sections crossing a page break, and continued lists — that is, most document types other than
single-page receipts and confirmations.

RRM-001 is therefore an MVP requirement rather than post-MVP work, and remaining vocabulary is
designed against the cross-page model rather than retrofitted to it. See the
[delivery plan's M0 record](rule-based-remediation-plan.md#m0--foundations--scope-lock) for the
sequencing consequences.

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
| Harden JSON authoring, CLI diagnostics, and MVP documentation | RRM-013, RRM-035, RRM-038 | 2-3 days |
| Cross-page flow: stage-major evaluation and flow continuation | RRM-001, table part of RRM-005 | 3-5 days |
| Authoring layer: candidate inspection, named predicates, tolerance calibration | RRM-037, RRM-041, RRM-036 | 4-8 days |
| Structural template (descriptive) and artifact inventory | RRM-039 phase 1, RRM-040 | 5-8 days |
| Stage/dependency decision and nested grouping | RRM-004 | 2-4 days |
| **Likely internal MVP total, allowing work to overlap** |  | **30-49 focused engineer-days** |

The second review pass raised this estimate from the original 10-15 days. The added work is almost
entirely assurance rather than new rule syntax: cardinality, assertions, per-rule diagnostics,
normalization, and contract specification. That reordering is deliberate. Without RRM-016 and
RRM-018 there is no automated way to tell whether any of the other fixes actually improved a real
document, and the MVP exit gate falls back to human inspection of every output.

A third review pass on 2026-07-29, following the RRM-001 delivery, added RRM-036 through RRM-038 and
raised RRM-004 to P0. The same reasoning applies: RRM-037 is tooling that makes hand-authoring and
generated authoring tractable at all, RRM-036 closes a class of silent degradation that neither
external validation nor cardinality can detect, and RRM-004 is a ceiling on expressiveness that gets
more expensive to raise once rule sets exist to migrate. None of the four is new rule syntax either.

The [structural model design](rule-based-remediation-structural-model.md), also 2026-07-29, added
RRM-039 through RRM-041. These *are* new syntax, and they are the only additions in three review
passes that are. The justification is that they close the gap the tracker itself identifies as the
one that matters most — nothing currently asserts that the semantics the author intended are the
semantics the document ended up with — and they do it by declaration rather than by accumulating more
hand-written checks. Part of the added estimate is recovered rather than spent: RRM-039 subsumes most
of RRM-018's remaining assertion forms and supplies the weak form of RRM-017 for free.

A fourth review pass on 2026-07-31, following the decision that prescriptive mode is the only
supported model, added RRM-042 through RRM-046. These follow from that decision rather than extending
scope: once the template owns structure, the remaining authoring work is labeling content into slots,
partitioning repeated content into occurrences, and accounting for the rest — and only labeling is
currently declared where it belongs. RRM-042 and RRM-045 are P0 because a dynamic family cannot be
remediated without them: repeated content on one page collapses into a single occurrence, and content
accounting is an open-ended rule backlog that never closes. RRM-046 is P0 because the prescriptive
decision removed every reading-order check without replacing any of them. RRM-042 through RRM-044
each *remove* authored artifacts — a partition rule, three region types, one Refine rule per static
attribute — so the net effect on rule-set size is negative even though the declaration surface grows.

The earlier "80-85% complete" characterization remains reasonable for the *rule language*. It is too
generous for the *system*, where the assurance layer that would let a rule set run unattended does
not yet exist.

Cross-page flow was previously a conditional addition of 2-5 days. It is now decided in scope and
folded into the table above at 3-5 days. The estimate holds despite the wider scope because the DOM
and serializer already support cross-page structure elements — see RRM-001.

Remaining conditional additions:

- add 3-7 days to support PDF/UA-2 at the same confidence level;
- add 5-15 days for complex tables, multi-column reading order, rotated text,
  interactive forms, or nested structures, depending on which are required.

These are focused engineering days with continued AI assistance, not elapsed
calendar time. A reasonable planning allocation is three calendar weeks for
one engineer: approximately two weeks for implementation and validation plus
one week of contingency for issues discovered in real PDFs.

### Delivery ranges

Revised 2026-07-29 to track the estimate table above; the previous figures predated three review
passes and had drifted below the MVP total.

| Level | Effort | Result |
| --- | ---: | --- |
| Narrow pilot | 3-5 days | One or two text-heavy templates, page-local flows, explicit grids, and manual review |
| Usable internal MVP | 30-49 days | Several representative templates, basic graphics, declared structure with output diffing, stable JSON/CLI workflow, external validation, and regression fixtures |
| Dependable internal platform | 50-75 days | Multiple PDF producers, prescriptive templates, irregular tables, richer figures, wrapped cells, stronger reading order, and broader conformance evidence |

The narrow pilot is useful for discovering whether the proposed MVP boundary
matches the actual internal document corpus. It should not be presented as
general PDF remediation coverage. It also predates the assurance and structural
layers, so pilot output is not evidence that a rule set is safe to run
unattended.

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

**Status:** Complete (2026-07-29)

**Priority:** P0

**Completed.** Remediation now evaluates stage-major across the document while retaining
page-scoped ownership, content, and MCID allocation. `ContinueUntilEnd` resolves stable activation
instances across boundary-free intermediate pages, diagnoses malformed and unterminated
activations, supports an inclusive `MaxPages` limit, and applies the declared reading-order mode to
flow-order predicates.

Group/Merge runs cross pages only inside the same activation; `SamePage()` remains authoritative
and cross-page near misses are reported as nonblocking warnings in both dry-run and commit reports.
Continued tables produce one table per activation. `HeaderRows` is explicitly scoped to the logical
table or every page, while `HeaderSelector` identifies actual repainted headers. Planning,
materialization, reports, semantic nodes, placement, and reading-order checks retain page-span
provenance using `(page, MCID)` order. RRM-005 remains open for configurable tolerances, `THead`,
irregular grids, spans, and split rows.

**Scope decision.** Cross-page spanning is required, not optional. Page-local evaluation is only
correct when every page is self-contained; multi-page tables, sections crossing a break, and
continued lists all fail that test, and they cover most document types beyond single-page receipts.
This moves RRM-001 out of post-MVP work and ahead of the remaining M4 vocabulary, so that vocabulary
is designed against the cross-page model rather than retrofitted.

**The output stack already supports cross-page structure.** This is confined to the remediation
evaluation loop — it is not a format or DOM change:

- `StructureNode.ContentItems` is `List<(PdfPage Page, int MCID)>`, so one structure element can
  already reference MCIDs on different pages.
- `StructuralSerializer` already emits the correct format for that case: when a node's content spans
  more than one page it switches from bare MCID integers to `/MCR` marked-content-reference
  dictionaries carrying a per-item `/Pg`.
- `StructuralBuilder` already exercises both paths.

**Implementation contract.** The session evaluates Classify across selected pages, then Group,
then Refine. Candidate ownership, working content, and MCID allocation remain page-scoped.
`DocumentFlowRegionResolver` carries activation state across pages and caches claim membership.
Expected boundary-free intermediate pages are silent; malformed, capped, and unterminated
activations are diagnosed. A logical paragraph, group, or table may intentionally own content on
multiple pages.

Named claim containment preserves the existing anchor/zone behavior and permits a multi-page claim
inside a flow only when every page belongs to one activation. Materialized structure placement and
the reading-order diagnostic compare `(pageIndex, MCID)` tuples because MCIDs restart per page.

**Completion criteria**

- [x] Evaluation is stage-major across pages, with MCID allocation, text ownership, and content
  ownership remaining page-scoped.
- [x] `CurrentPageOnly` and `ContinueUntilEnd` have observably different
  runtime behavior.
- [x] Flow state can start, continue through intermediate pages, and stop on a
  later page.
- [x] Missing start/end anchors are diagnosed according to the page's role in
  the continuation rather than always treated as failure.
- [x] `ReadingOrderMode` controls candidate ordering.
- [x] The reading-order diagnostic handles a structure element whose content interleaves across
  pages.
- [x] Tests cover a header on page 1, intermediate pages without either
  boundary, and a subtotal on the final page.
- [x] Multi-page grouping and table behavior is explicitly defined and tested.
- [x] Repeated-header policy is implemented, documented, and asserted.

## RRM-002: Declarative rules cannot select or artifact non-text content

**Status:** Complete

**Priority:** P0

Classify rules enumerate candidates from `StructuredTextPage`. There are no
declarative candidate types or predicates for `ImageContent`, `PathSequence`,
or `FormContent`. `AutoArtifact` likewise enumerates only `TextContent`, while
the untagged-content diagnostic examines every content item.

The imperative `StructuralBuilder` can bind images, but that capability is not
available as a portable, serialized remediation rule.

**Progress (2026-07-29)**

Implemented discriminated text/content candidates, typed selectors, atomic image/path/form/shading
enumeration and materialization, content type/resource/reuse predicates, same-item ownership
conflicts, and reported graphical `AutoArtifact`. Non-paint paths are excluded. Resource names come
from page resource keys, resource identities are stable SHA-256 stream hashes, reuse counts are
document-wide, graphical bounds use the structured-text page transform, and the page candidate
index is cached. Integration coverage exercises named/reused forms, ruling/background paths,
dry-run, commit, JSON predicates, and auto-artifact reporting.

The classify API and JSON schema now require typed candidate selectors, structured unaccounted
painting details are reported, and invocation-level binding plus Figure `/Alt` are covered by
integration tests. Form XObjects intentionally remain atomic; selective form flattening remains
separate work.

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

- [x] The candidate model identifies text, images, paths, and form XObjects.
- [x] Candidate predicates can filter by content type, bounds, resource
  identity, and repeated use where applicable.
- [x] `Tag("Figure")` can bind an image or suitable graphical content.
- [x] `Artifact(...)` and `AutoArtifact` can safely cover decorative non-text
  content.
- [x] Diagnostics distinguish painting content from state-only operators.
- [x] Tests cover a logo, table ruling lines, a background path, and a reused
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

## RRM-004: Structural composition across Group passes

**Status:** Complete (M3c — explicit numbered Group passes, 2026-07-30)

**Priority:** P0

Group rules declare an explicit numeric pass. Each pass consumes the immutable structural frontier produced by lower passes, allowing deterministic parent-over-parent composition while rejecting ambiguous same-pass consumption.

The original symptom was group-over-group: real transactional structure is recursive — cell to row
to table to section, lists inside sections, and tables inside sections — but the former flat Group
snapshot stopped declarative composition at the second level. Numbered passes remove that ceiling
without replacing the three public stages.

**Priority raised to P0 on 2026-07-29.** This is a ceiling on what the language can express, not a
missing convenience, and the cost of changing the stage model rises sharply once authored rule sets
exist to migrate. Deciding it before the remaining M4 vocabulary is the same argument that moved
RRM-001 ahead of that vocabulary.

**Sequenced after RRM-039 phase one (2026-07-29).** The descriptive structural template informed
the choice without replacing runtime composition. If it becomes prescriptive in M6, regular declared
hierarchy can move into template slots, while numbered passes remain the mechanism for derived or
undeclared hierarchy.

**Delivered contract**

- `groupPass` is an optional non-negative integer; omission and explicit pass `0` are equivalent,
  sparse passes are allowed, and nonzero values are valid only for Group rules.
- Each pass consumes an immutable frontier produced by lower passes. Classify and lower-pass
  references are valid; self-, same-, and higher-pass Group references are rejected.
- Composition validation rejects multiple structural consumers, ancestor-plus-descendant selection,
  and cycles before materialization. Ambiguity and cycle diagnostics cannot be suppressed.
- Consuming predicates see the frontier, while `BeforeClaim` and `AfterClaim` can resolve all applied
  lower-pass claims. Reports expose both stage and pass.
- C-29 fixtures cover bottom-up nested lists and independent two-level section chains for both PDF/UA
  profiles, preserving sibling order, MCIDs, ParentTree ownership, rendering, and conformance.

**Completion criteria**

- [x] The stage-versus-dependency decision is recorded with its consequences for the action
  vocabulary.
- [x] Define deterministic visibility of earlier Group outputs within the
  Group stage, or introduce explicit grouping passes.
- [x] Detect and reject ambiguous or cyclic reparenting.
- [x] Tests cover nested lists, nested sections, and two non-overlapping parent
  layers.
- [x] MCIDs remain unique and unchanged through every parent layer.

## RRM-005: Table construction handles only regular, page-local grids

**Status:** Partial

**Priority:** P0

Text rows are grouped by their character baseline, with a relative-bounds center fallback for
non-text claims. This prevents glyph descenders from splitting a regular row, but it is not an
adaptive irregular-grid model. Inferred columns still use a fixed twelve-point center clustering
tolerance. A matched claim is assigned to a column by its relative-coordinate horizontal center,
and each matched claim becomes a separate `TD`/`TH`.

`TableOverFlattenedCells` removes intermediate leaf structure nodes; it does
not generally merge several claims or visual lines into one semantic cell.

**Impact**

- Wrapped multi-line cells may be treated as multiple rows.
- Multiple fragments in one column may become duplicate cells.
- Sparse rows, variable baselines, right-aligned numeric columns, and
  overlapping bounds can be misclassified.
- There is no first-class representation of row spans, column spans, row
  headers, complex header associations, or `THead`/`TBody`/`TFoot`.

**Relevant code**

- `src/PdfLexer/Remediation/RemediationSession.cs`
  (`ApplyClaimConsumingTableClaim`, row grouping, `ResolveTableGrid`)
- `src/PdfLexer/Remediation/RemediationAction.cs`
  (`TableRemediationAction`, `TableCellContentMode`)

**Completion criteria**

- [ ] Row and column tolerances are explicit rule parameters or derived from
  document metrics. Shares the calibration mechanism tracked in RRM-036.
- [ ] Row membership is declarable, not only inferred. Columns can be declared today and rows
  cannot, so a wrapped multi-line cell cannot be expressed as one row by any rule set.
  `HeaderSelector` already establishes that a `ClaimPredicate` can define row membership; a
  `RowsBy`-style equivalent is the natural completion.
- [ ] The rule model can identify several claims as content of one cell.
- [ ] Sparse and wrapped rows have deterministic behavior and diagnostics.
- [ ] `RowSpan` and `ColSpan` affect both structure attributes and grid
  occupancy.
- [ ] Row and column header relationships can be authored and validated.
- [x] Multi-page table continuation and repeated header semantics are defined.
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

**Status:** Partial

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
invalid `TD > TR` document as an expected failure. The gate now also runs for
every committing fixture in its own profile, and every fixture runs strict.
Recording PAC/manual assistive-technology review remains open.

Extending the gate immediately caught three PDF/UA violations the internal
checks had passed: `Span` as a direct child of `Document` (C-02, C-23), `Sect`
holding raw content (C-03), and a heading-level skip from `H2` with no `H1`
(C-23). That is the argument for the gate, stated as evidence.

**Relevant code and tracking**

- `openspec/changes/rule-driven-remediation/tasks.md` (task 18.6)
- `test/PdfLexer.Tests/RemediationFixtureGenerator.cs`
- `src/PdfLexer.pdfctl/Inspect/RemediateCmd.cs` (`--verapdf`)

**Completion criteria**

- [x] All remediation fixtures run with strict conformance enabled unless a
  fixture explicitly tests permissive behavior.
- [x] veraPDF is run for the intended UA profile of every fixture
  (`RemediationVeraPdfTests.EveryCommittedFixtureValidatesForItsProfile`).
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

**Partly supplied by RRM-039 (2026-07-29).** A rule set whose structural template requires anchors
that do not resolve is not applicable, and that check comes for free once templates exist. It is the
weak form: it tests the rule set's own preconditions rather than the document's identity, so it
catches "this rule set cannot work here" but not "this is a different family that happens to satisfy
the same anchors." Rescope the remaining work against what RRM-039 phase one actually delivers
before building a separate fingerprint mechanism.

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

**Status:** In progress

**Priority:** P0

External validation answers whether the output is well-formed PDF/UA. It cannot answer whether the
output is *correct*. A document in which the invoice total was tagged as a pagination artifact and
the footer was tagged `H1` is fully conformant.

The rule language has no way to express expectations about the shape of the result: "exactly one
`H1`", "at least one `Table`", "the `invoice-number` rule produced exactly one claim on page 1",
"every `TR` has the same number of `TD` children as the header row".

The MVP exit gate covers this with human review ("one reviewed rule set handles those variants").
That does not scale to production batches and does not survive template drift.

**Largely subsumed by RRM-039 (2026-07-29).** Rule-output counts, structure-element counts, and
parent-child shapes are all derivable from a structural template, and a positional tree diff is a
stronger statement than any of them individually — it reports order and position, which no count
can. The assertion work already delivered stays useful and is not wasted: it remains the mechanism
for statements a template cannot make, such as value-level or cross-document conditions. Treat
further hand-written assertion forms as lower value until RRM-039 phase one lands.

**Progress (2026-07-29)**

Implemented immutable planned semantic-tree output; document/per-page rule-output counts,
structure-tag counts, and direct parent/child assertions; JSON parsing and validation; report
outcomes with provenance; and commit-blocking `SemanticAssertionFailed` diagnostics.
Missing parent/element assertions now fail rather than producing no outcome, child ranges are
validated, recursive traversal is cycle-guarded, and integration coverage exercises a missing
semantic element.

Still required: snapshot the materialized `StructuralBuilder` tree, compare asserted planned and
materialized dimensions, strengthen tag/page-selector validation, and add rollback/parity tests.

**Impact**

- Conformance passes are mistaken for correctness passes.
- Regression testing a rule set against new document instances requires manual inspection.
- RRM-016 and RRM-017 have no automated acceptance surface to report into.

**Relevant code**

- `src/PdfLexer/Remediation/RemediationReport.cs`
- `src/PdfLexer/Remediation/RemediationSession.cs` (diagnostic pass)

**Completion criteria**

- [x] A declarative assertion set can be attached to a rule set and evaluated at dry-run and commit.
- [x] Assertions cover claim counts per rule, structure-element counts per tag, parent/child shape,
  and page-scoped variants of each.
- [x] Assertion failures are diagnostics with the same provenance detail as rule diagnostics.
- [x] Assertions are expressible in the JSON rule format.
- [ ] Tests cover an assertion that catches a mis-tagged total which external validation accepts.

## RRM-019: Annotations already present in the input are outside the rule model

**Status:** Complete (2026-07-30)

**Priority:** P0

`CandidateSelector.Annotations()` exposes stable page/array identities, subtype, normalized optional
bounds, flags, existing ownership, contents, and normalized destination metadata. Annotation
predicates select those fields; ordinary page and geometry predicates apply when `/Rect` is usable.

`AdoptAnnotation` preserves the original object, binds `Link`, `Form`, or `Annot` structure through
an OBJR and `/StructParent`, and can either intersect-pair with compatible Classify claims through
`Into` or create a standalone required node. Internal links may name a Classify
`DestinationTarget`; PDF/UA-2 uses the structure-destination serializer. Links require an explicit or
pre-existing non-empty description. Duplicate consumers, absent or ambiguous targets, and invalid
tags are non-suppressible commit blockers, and failed commits restore annotation dictionaries.

The rebuilt inventory reports `Unmodeled`, `Exempt`, `Planned`, or `Applied` with candidate, rule,
produced tag, and destination provenance. Hidden and wholly off-page annotations, `Popup`, and valid
specialized `PrinterMark` annotations retain their exemption contracts. C-08 covers internal and URI
links, Stamp, hidden Popup, and FileAttachment for both PDF/UA profiles; created links retain the
zero-width border contract.

**Completion criteria**

- [x] Existing annotations are exposed as candidates with subtype, rect, page, ownership, contents,
  and destination metadata available to predicates.
- [x] Adoption binds existing annotations to `Link`, `Form`, or `Annot` without replacements.
- [x] Visible semantically empty annotations are deliberately accounted for as `Annot`; supported
  exemptions are reported separately.
- [x] Inventory and diagnostics report every annotation disposition at dry-run and commit.
- [x] Link creation avoids visual change with `/Border [0 0 0]`.
- [x] C-08 covers both PDF/UA profiles and validates with veraPDF.

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

**Status:** Complete (2026-07-30)

**Priority:** P1

Artifact actions retain the historical `ArtifactSubtype` `/Type` contract and add semantic
`Header`, `Footer`, and `Watermark` subtypes, candidate-derived `/BBox`, and unique attachment edges.
`HeaderArtifact`, `FooterArtifact`, and `WatermarkArtifact` provide the normative defaults. Artifact
inventory items carry the same metadata, so automatic furniture handling emits the declared wrapper.

JSON keeps legacy `"subtype"` as `/Type`, adds canonical `"type"`, `"semanticSubtype"`,
`"includeBoundingBox"`, and `"attached"`, and rejects conflicting type declarations without a schema
version bump. `PageWriter.BeginArtifact(PdfName?)` remains source-compatible; the full overload and
header/footer/watermark conveniences emit complete property dictionaries.

**Completion criteria**

- [x] Artifact actions express `/Type`, `/Subtype`, `/BBox`, and `/Attached`.
- [x] Convenience forms exist for header, footer, and watermark artifacts.
- [x] The underlying writer emits the full artifact property list.
- [x] C-10 asserts exact dictionaries on four pages for both PDF/UA profiles and passes veraPDF.

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

**Status:** Complete

**Priority:** P1

**Completed (2026-07-29)**

Confidence-neutral matches now inherit a defensible `DefaultConfidence` of `1.0`; explicit
confidence producers remain authoritative. The `[0,1]` scale and composite predicate rules are
documented, validation rejects invalid defaults, and traces retain the contributing confidence.

**Residual concern (2026-07-29 review).** The contract is now defined, but with
`DefaultConfidence` at `1.0` and only toleranced layout and table inference producing degraded
values, the scale is close to degenerate in practice: `MinConfidence` is effectively a binary gate on
two predicate families while presenting as a continuous threshold. An author, and more reliably a
generated rule set, will write `MinConfidence: 0.8` believing it expresses a graded confidence.
Either widen the set of predicates that estimate uncertainty — near-threshold geometric matches are
the obvious candidates, and RRM-036 has to compute that margin anyway — or rename the knob to what it
actually gates. Not reopened; revisit alongside RRM-036.

**Original gap.** Confidence appeared throughout the model — `Rule.MinConfidence`,
`RemediationClaimOutcome.Confidence`, `RemediationSessionConfiguration.DefaultConfidence`,
toleranced-zone "degraded confidence", and a "low-confidence inference" diagnostic — but nothing
defined it. There was no documented scale semantics, no statement of what produced a confidence
value, no description of how tolerance degraded it, and no rule for how `And`, `Or`, and `Not`
combined the confidences of their operands.

`DefaultConfidence` defaulted to `0.0`. Since it applied to matches that do not compute a confidence
explicitly, any rule setting `minConfidence` rejected every such match, so the feature could not be
used without reading the implementation.

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

- [x] The confidence scale, its sources, and its combination rules are specified and documented.
- [x] Every predicate either computes a confidence or is documented as confidence-neutral.
- [x] `DefaultConfidence` has a defensible default, or rules with `MinConfidence` reject
  confidence-neutral predicates with a validation error rather than silently matching nothing.
- [x] Reports expose which predicate contributed a degraded confidence.

## RRM-028: Anchor resolution scope is per page but documented as absolute

**Status:** Complete

**Priority:** P1

**Completed (2026-07-29)**

Anchor selection is explicitly per selected page. `RequiredSingle` means exactly one match on that
page, diagnostics distinguish absence from ambiguity and name the page, `Pages` and `Style` ordering
is documented, and the redundant one-based `Occurrence` mechanism was removed in favor of the
zero-based `NthInReadingOrder`.

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

**Status:** Complete

**Priority:** P1

**Completed (2026-07-29)**

Default siblings use a centralized, stable page reading-order comparator: top-to-bottom, then
left-to-right, with deterministic candidate/rule/tag tie-breakers. The same comparator is used by
planning, materialization, and reading-order diagnostics; explicit `ReorderSiblings` remains for
semantic exceptions.

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

## RRM-030: `Commit` failure semantics and rollback

**Status:** Complete

**Priority:** P1

`DryRun()` and `Commit()` evaluate the same composed rules, but commit reevaluation can still expose
input or materialization failures. The commit boundary is now explicit: declaration, cardinality,
composition, template, and artifact validation run before mutation; `Commit()` then takes a recursive
checkpoint of reachable PDF dictionaries/arrays and the original structure reference before applying
content, structure, annotation, and accessibility changes.

If any unsuppressed diagnostic or exception occurs after mutation, the checkpoint is restored and the
exception is rethrown. The document's reachable PDF object graph, page content, annotations, structure
reference, and catalog/page dictionaries are therefore unchanged. A failed session should be
replaced with a fresh session for retry. Repeated `Commit()` calls on a successful session remain an
`InvalidOperationException`; a failed call leaves the document reusable but the session is not a
retry coordinator.

**Completion criteria**

- [x] Dry-run and commit use the same staged evaluation and planned semantic tree.
- [x] Pre-mutation failures block commit without touching the document.
- [x] Post-mutation failures restore the structure/content checkpoint and original structure reference.
- [x] A regression test covers object-graph-stable rollback after a later failure.
- [x] Repeated `Commit()` calls have defined behavior.

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

**Status:** Complete

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

**Progress (2026-07-29)**

Implemented the documented NFKC, soft-hyphen, Unicode-whitespace, dash, and quote pipeline with
rule-set and predicate overrides. Literal operations normalize both operands; regex operations
normalize candidate text only. Traces expose raw and normalized text, and focused unit tests cover
the transformations and overrides.

Anchor label, table-header, repeated-element, style, and neighbor matching now use the declaring
rule set's policy. Candidate/outcome summaries expose raw and effective normalized text, while
materialization remains tied to original characters, source references, and text ranges. Fixture
coverage includes the normalization pipeline and source-range invariance.

**Impact**

- Rules are brittle in ways that correlate with the producing application rather than the layout.
- Diagnosis is difficult because the extracted text looks correct when printed.
- Combined with RRM-016, a normalization mismatch degrades output silently.

**Relevant code**

- `src/PdfLexer/Remediation/RemediationPredicate.cs` (text predicates)
- `src/PdfLexer/Content/StructuredWord.cs`,
  `src/PdfLexer/Content/StructuredLine.cs`

**Completion criteria**

- [x] A documented default normalization is applied to candidate text before matching, covering
  Unicode normal form, whitespace collapsing, and hyphen/dash/quote folding.
- [x] Normalization is configurable per rule set and per predicate where the default is wrong.
- [x] Reports expose both the raw and normalized text for a candidate.
- [x] Tests cover ligatures, soft hyphens, non-breaking hyphens and spaces, and `TJ`-derived spacing.

## RRM-033: No negative explain or per-rule match diagnostics

**Status:** In progress

**Priority:** P1

`RemediationReport.Explain(...)` accepts a source reference, candidate, or structured character and
returns the claim outcomes that touched it. That is useful and currently undocumented.

It answers "which rules affected this content." It cannot answer the question a rule author actually
has: "why did my rule match nothing", or "which conjunct of this `And` chain rejected this
candidate". For a language whose entire authoring loop consists of tuning composed predicates, that
is the missing tool — and it is the direct enabler for diagnosing RRM-016 and RRM-032.

**Progress (2026-07-29)**

Implemented opt-in candidate rejection tracing filtered by rule/page/candidate/source, trace trees
with confidence/reasons/evaluated/skipped state, short-circuit preservation, rejecting `And`
operand reporting, `RemediationReport.PredicateTraces`, `ExplainRejection`, and CLI
`--explain-rule`/`--explain-page`. A central evaluation wrapper guarantees that custom and built-in
predicate implementations cannot omit trace nodes; mixed Font/Text regression coverage walks the
entire resulting tree.

Still required: trace claim predicates used by group/refine actions, improve rejection reasons
where built-in leaves still return generic data, and add end-to-end filtering, zero-match CLI, and
tracing-does-not-change-results tests.

**Impact**

- Authoring a rule set is a guess-and-check loop against a black box.
- The most common authoring question has no supported answer.

**Relevant code**

- `src/PdfLexer/Remediation/RemediationReport.cs` (`Explain`)
- `src/PdfLexer/Remediation/PredicateResult.cs`
- `src/PdfLexer/Remediation/RemediationPredicate.cs`

**Completion criteria**

- [x] A trace mode records per-predicate evaluation outcomes for selected candidates or a selected
  page.
- [x] Composed predicates report which operand rejected a candidate.
- [x] Per-rule evaluation counts — candidates considered, matched, rejected by confidence, rejected by
  conflict — appear in the report for every rule.
- [x] `Explain` is documented, and a negative form is available from the CLI.

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

## RRM-036: Geometric tolerances are authored constants with no calibration

**Status:** Open

**Priority:** P0

Every geometric parameter in the language is a number the rule author guesses, and nothing tells
them the guess is wrong until the output is silently incorrect. The surface includes
`Predicates.Anchor.SameRowAs(anchorId, tolerance)`, `RightOf`/`Below`/`NearestTo`
(`tolerance`, `maxDistance`), `TolerancedZone(..., Tolerance)`, literal table column boundaries, and
internal constants such as the twelve-point column-clustering tolerance in `ResolveTableGrid`.

RRM-005 owns the table-specific half of this. RRM-036 is the general problem and the missing
mechanism: there is no way to derive a tolerance from a document family, and no way to learn that a
chosen tolerance sits close to a boundary where a small layout change flips the result.

This was demonstrated concretely during the RRM-001 review. Row grouping quantized the vertical
center to a six-point grid, so a descender in one cell and none in its neighbour placed the two on
opposite sides of a bucket boundary; a two-cell row silently became two one-cell rows in reversed
order, and the acceptance test still passed. The mechanism was corrected to baseline grouping, but
the class of failure is structural, not specific to that constant.

**Impact**

- A rule set that passes on the sample document fails on a sibling document for reasons the author
  cannot see, because the failure is a near-threshold geometric comparison rather than a missing
  match.
- Tolerance failures degrade output rather than stopping the run: content lands in the wrong cell,
  row, or region and remains validly tagged, so RRM-010 external validation cannot catch it and
  RRM-016 cardinality often cannot either.
- AI-authored rules are affected more severely than hand-authored ones. A model cannot derive
  `tolerance: 4` from a document description; it will emit a plausible constant, and plausible and
  wrong is the failure mode with no signal attached.

**Relevant code**

- `src/PdfLexer/Remediation/RemediationPredicate.cs` (`AnchorRelativeRemediationPredicate`)
- `src/PdfLexer/Remediation/TolerancedZone.cs`
- `src/PdfLexer/Remediation/RemediationSession.cs` (`ResolveTableGrid`, row grouping)

**Completion criteria**

- [ ] A calibration pass accepts several sample documents of one family and reports, per tolerance
  in the rule set, the observed spread and the margin between the authored value and the nearest
  value that would change the result.
- [ ] Calibration output is machine-readable so it can be fed back to a rule author, human or
  generated.
- [ ] Tolerances that resolve within a configurable margin of a decision boundary are diagnosed on
  every run, not only during calibration.
- [ ] Internal constants that behave as tolerances are either exposed as parameters or documented
  with the reason they are fixed.
- [ ] Tests cover a family where a plausible authored tolerance is wrong, and assert that the
  wrongness is reported rather than absorbed.

## RRM-037: No forward authoring inspection of what the engine sees

**Status:** Open

**Priority:** P0

Diagnosis after rules exist is well covered: `Explain`, `ExplainRejection`, `PredicateTraces`, and
`RuleEvaluations`, with RRM-033 completing the negative direction. The opposite direction does not
exist. There is no way to ask what the engine sees in a document *before* any rule is written —
candidate ids, text, granularity, geometry, font, content order, and where declared anchors, zones,
and flow regions resolve.

**Impact**

- The first rule for a new document family is written against a document the author cannot see in
  the engine's own terms, so early iterations diagnose the author's mental model rather than the
  document.
- Candidate ids appear in `ExplainRejection` and in reports but there is no way to enumerate them,
  which makes the negative-diagnosis surface hard to enter.
- This is the specific gap that blocks AI rule authoring. The loop *inspect, propose, dry-run, read
  traces, revise* is otherwise fully mechanizable, and every step but the first already exists. A
  human author can compensate by looking at the rendered page; a model cannot.

**Completion criteria**

- [ ] A library call and CLI command emit the candidate index for selected pages and granularities,
  including candidate id, text, raw and normalized, bounds, font, and content order.
- [ ] The same output reports how each declared anchor, zone, and flow region resolves, including
  activation instances and unresolved boundaries.
- [ ] Output is machine-readable and stable enough to be diffed between two documents of one family,
  so template drift is visible without authoring a rule first.
- [ ] Candidate ids in the inspection output are the same ids accepted by `ExplainRejection`.
- [ ] Documented as the recommended first step in the authoring workflow.

## RRM-038: Serialized schema is a projection of the C# API, not the contract

**Status:** Open

**Priority:** P1

`SerializedRemediationRules` is maintained alongside the fluent C# surface rather than as the
authoritative definition of the language. Nothing prevents a predicate, action, or parameter from
shipping in C# and reaching serialization a release later, and the gap is silent in both directions.

RRM-013 covers validation of the schema and RRM-035 covers documentation drift against the API.
Neither covers parity between the serialized form and the shipped surface, which is a different
failure with a different remedy.

**Impact**

- Any capability reachable only from C# is invisible to JSON-authored and generated rule sets, which
  is the authoring path intended for AI assistance and for portable per-family rule files.
- `RemediationActions.Custom` takes a delegate and is inherently non-serializable, so the language's
  escape hatch is unavailable on that path. That is defensible as a boundary but needs to be a
  stated one rather than an accident.
- Parity gaps surface as an authoring dead end rather than an error, because the C# example in the
  documentation works and its JSON equivalent cannot be written.

**Completion criteria**

- [ ] A test enumerates the public predicate, action, anchor, zone, and flow surface and fails when
  a member has no serialized representation and no explicit exemption.
- [ ] Exemptions are declared in one place with a reason, `Custom` among them.
- [ ] Round-trip coverage asserts that a rule set built in C# serializes and deserializes to an
  equivalent rule set for every non-exempt member.
- [ ] The schema is documented as the language contract, with the C# surface described as a
  convenience over it.

## RRM-039: Structural templates own expected document structure

**Status:** Complete

**Priority:** P0

**Design:** [Structural Model for Rule-Based Remediation](rule-based-remediation-structural-model.md)

A rule set is a flat list of rules. The structure a document is supposed to have exists only in the
author's head, spread across rule ids, predicates, and cardinality constraints. Because it is never
stated, it can never be checked.

Everything downstream inherits that. Output correctness is asserted only by hand-written counts
(RRM-018). Applicability cannot be tested because there is nothing to test against (RRM-017). A
reviewer cannot tell what a rule set is trying to produce without reading every rule, which puts
review beyond anyone who knows PDF/UA but not this rule language.

The proposal is a **structural template**: the expected structure tree, declared in PDF standard
structure vocabulary, as a deterministic content model with sequence, optional, and repetition
operators. Phase one is descriptive — rules build the tree as they do today and the produced tree is
diffed against the template. Phase two lets the template drive materialization.

**Impact**

- The only automated statement about output correctness is a set of counts that must be maintained
  by hand and silently decays as rule sets grow.
- A wrong-but-conformant tree — the footer tagged `H1`, a total swept into an artifact — is
  indistinguishable from a correct one to every check that currently exists.
- Rule sets are not reviewable by accessibility specialists, only by rule-language specialists.
- Generated rule authoring has no artifact to reason about. A model produces an expected structure
  far more reliably than a flat list of interdependent rules with geometric tolerances.

**Completion criteria (phase one — descriptive)**

- [x] A template declares expected structure in PDF standard structure types, with sequence, `?`,
  and `*`/`+` occurrence operators.
- [x] Templates are validated for PDF/UA legality and for content-model determinism at declaration
  time, before any document is processed, with the offending position named.
- [x] Template nodes have stable declared ids for rule binding and derived positional paths for
  diagnostics.
- [x] A rule may declare the slot it fills; unbound rules behave exactly as today.
- [x] The produced tree is diffed against the template, reporting missing required nodes, unexpected
  nodes, wrong order, occurrence violations, illegal nesting, and slots bound but unfilled.
- [x] Each difference kind is a suppressible diagnostic code with a scope and recorded reason.
- [x] Declaring both a template occurrence and a per-rule cardinality is supported as two independent
  assertions: occurrence constrains produced nodes and cardinality constrains selector inputs.
- [x] A rule set with no template behaves exactly as it does today.
- [x] Corpus fixtures cover an optional section absent and present, a repeating section, and a
  structure that is valid PDF/UA but wrong against its template.

**Phase two — prescriptive template-first mode (complete 2026-07-30)**

- [x] `Descriptive` and opt-in `Prescriptive` template modes are explicit in C# and JSON.
- [x] `Bind`, `BindOver`, `TemplateSlotContentMode`, and first-class `RemediationClaim.SlotId` bind claims to declared slots.
- [x] Singular composite ancestors are synthesized; repeated composites require one `BindOver` producer and receive deterministic occurrence identities.
- [x] Template-declared tags, hierarchy, occurrence, and sibling order own materialization; Refine can target synthesized slots with `FromSlot`.
- [x] Specialized tables and annotation adoption can bind compatible template roots; artifacts remain inventory-managed.
- [x] Pre-mutation validation, prescriptive leftover errors, planned/read-back comparison, and all-or-nothing rollback are documented and tested.
- [x] Hardening uses one immutable assembly plan for dry-run and commit; root ordering, stable occurrence identities, directly bound parents, and nested `BindOver` passes are covered.
- [x] Specialized table interiors are opaque to slot matching and a twenty-row regression pins stable order; annotation adoption and Refine-through-slot are covered end to end.
- [x] Assembly provenance is public in reports and CLI output; reserved durable `/ID` values and non-suppressible prescriptive leftovers are enforced.
- [x] A committing prescriptive `BindOver` corpus fixture runs under both PDF/UA profiles and inherits regeneration, raster, glyph, and veraPDF gates.

Alternation and recursion operators remain outside this unit and will be added only if the real
internal corpus requires them.

## RRM-040: Expected page furniture is not declarable

**Status:** Complete

**Priority:** P0

**Design:** [Structural Model for Rule-Based Remediation](rule-based-remediation-structural-model.md)

Artifacts are not structure elements — they live outside the structure tree — so the structural
template in RRM-039 cannot describe them. Running headers, footers, page numbers, rules, and
watermarks need a parallel declaration on the page axis, and none exists.

The absence is why `AutoArtifact` is dangerous. Content that no rule claims is swept into artifacts,
which is how a rule set silently converts an invoice total into hidden content while passing every
conformance check available. The leftover policy currently offers only a switch between failing on
any unclaimed content and absorbing all of it, because there is nothing to check absorbed content
against.

**Impact**

- `AutoArtifact` cannot be used safely in production, which the authoring guidance already concedes
  by recommending `FailFast` in production instead.
- A document family whose page furniture legitimately varies has no way to say so, so the only
  usable setting is the one that fails on the variation.
- Artifacted content is invisible to assistive technology by design, so this failure mode is the
  hardest one for a sighted reviewer to notice.

**Completion criteria**

- [x] An artifact inventory declares expected page furniture with subtype, page selector, optional
  zone, and per-page occurrence. Items merge across composed rule sets; duplicate ids are rejected.
- [x] Content artifacted that matches no declared inventory item is a diagnostic
  (`ArtifactUndeclared`), whether produced by an `Artifact(...)` rule or absorbed by the leftover
  policy. Because it is raised before the apply gates, it stops the commit.
- [x] A declared artifact absent where required is a diagnostic (`ArtifactMissingDeclared`), and one
  occurring outside its per-page count is `ArtifactOccurrenceViolation`. All three suppress
  independently at their own scope.
- [x] `AutoArtifact` reports absorbed content against the inventory:
  `RemediationAutoArtifactOutcome.InventoryItemId` names the matched item, and matched content is
  wrapped with that item's subtype rather than a bare `/Artifact`.
- [x] Fixtures cover furniture present on every page, on first page only, legitimately absent, and
  the runaway-zone case.

## RRM-041: Recurring predicate logic cannot be named or reused

**Status:** Open

**Priority:** P1

**Design:** [Structural Model for Rule-Based Remediation](rule-based-remediation-structural-model.md)

There is no way to name a predicate expression and reuse it. Rule ids act as weak labels — a later
rule can reference an earlier one through `ClaimPredicates.FromRule`,
`Predicates.Relational.InsideClaimOf`, or `RemediationAnchor.PriorClaim` — but that requires the
earlier rule to have produced a claim, and classify claims take exclusive ownership of their spans.
A second rule selecting the same content is rejected as a conflict, and `Override` deletes the first
claim rather than coexisting with it. Overlapping, orthogonal descriptions of the same content are
therefore not expressible.

The proposal is **named predicates** in two namespaces, candidate and claim, expanded at the point of
use rather than evaluated as a marking pass. Because nothing is claimed, labels overlap freely and
nothing about ownership, staging, or the leftover policy changes.

**Impact**

- Common conditions — "body text", "a currency value", "not in the margins" — are retyped in every
  rule that needs them and drift apart as a rule set grows.
- Predicate traces print the fully expanded boolean tree, which is the hardest part of reading a
  rejection. A named predicate lets the trace print the name and expand on request; the diagnostic
  payoff is plausibly larger than the authoring one.
- Rule sets are harder to review because intent is buried in repeated expressions.

**Completion criteria**

- [ ] Candidate and claim label namespaces are declarable on a rule set and usable anywhere a
  predicate of that kind is accepted.
- [ ] Labels may reference other labels; cycles are rejected at declaration time.
- [ ] Using a candidate label where a claim label is expected, or the reverse, is a validation error.
- [ ] Predicate traces report the label name and expand the definition on request.
- [ ] Per-label match counts appear wherever per-rule match counts appear, so an author can check a
  label means what they think before writing rules that consume it.
- [ ] Labels round-trip through the serialized schema.
- [ ] Named `label`, not `tag`, throughout the API, schema, and documentation.

---

## RRM-042: Repeating-slot occurrence boundaries are not declarable

**Status:** Partial

**Priority:** P0

**Design:** [Architecture and Direction](rule-based-remediation-architecture.md#1-occurrence-boundaries-are-declared-on-the-slot)

The legacy template declares *shape* — which slots repeat — but nothing declares *count*: how many
occurrences of a repeating composite exist, and which bound claims belong to which. That decision is
unavoidable because it is a fact about the document, and it currently lives in `BindOver`, which
builds runs of consecutive claims matching a `ClaimPredicate` and breaks a run only at a page
boundary with no shared flow instance (`RemediationSession.EvaluateDocumentClaimRunRule`).

The consequence is a silent collapse. `RemediationStructuralTemplateTests.cs:504` declares
`Sect#item` (`OneOrMore`) over `P#line` (`ExactlyOne`) and passes with its two lines on separate
pages; with both lines on one page it produces **one** `item` containing both, and reports
`TemplateWrongOrder` naming the child slot rather than the missing partition.

`FlowBoundary` (`FlowRegion.cs:65-74`) is already the right vocabulary — `Anchor`, `Zone`,
`Matching(predicate)`, `PageBoundary` — but it is reachable only through `FlowRegion` and clipped
there: `ProbeBoundary` returns one boundary per page, and `byPage[pageIndex][regionId]` holds one
resolution per region per page (`DocumentFlowRegionResolver.cs:90`, `:138`), so a region cannot
activate twice on a page.

The preview program now makes the boundary a property of the repeating slot, **derived from the declared
child shape by default**: the first declared child, when it is required and non-repeating, is the
opening child, and a claim bound to it closes the current occurrence and opens the next. Where the
opener is optional, repeating, or absent, the template is rejected at declaration time and the author
declares `startsOn` explicitly.

**Impact**

- A repeating section that occurs more than once on a page silently collapses into one occurrence,
  which is the normal case for line items, transaction rows, and repeated blocks — the shapes that
  make a family dynamic in the first place.
- The partition is authored procedurally, in a rule, per rule set, rather than declared on the slot
  it partitions.
- Occurrence identity keys on run position, so `template:Document/item[2]/line[1]` can shift when an
  unrelated rule changes what else is on the page.
- Nesting depth is restated by hand as `groupPass`, though the template already states it.

**Completion criteria**

- [x] Flow-region activation is not capped at one instance per page; a page holds an ordered list of
  resolutions per region.
- [x] A repeating composite slot carries an occurrence boundary, declared or derived.
- [ ] The default derivation from the declared opening child is specified and tested, including the
  nested case where an outer boundary closes open inner occurrences.
- [x] A repeating composite whose boundary cannot be derived is rejected at declaration time with the
  slot named, not at commit.
- [x] Occurrence identity keys on boundary activation, and is stable across unrelated rule changes.
- [x] A repeating composite occurring twice on one page produces two occurrences, with a fixture that
  fails before the change.
- [x] Boundaries round-trip through the serialized schema.

Preview completion does not close the legacy flow-region activation limitation, and RRM-042 remains
Partial until nested outer-boundary dominance and representative real-producer cases are accepted.

---

## RRM-043: Region declarations are fragmented and do not compose

**Status:** Complete

**Priority:** P1

**Design:** [Architecture and Direction](rule-based-remediation-architecture.md#2-one-region-concept-not-four)

`NamedLayoutZone`, `TolerancedZone`, `FlowRegion`, and anchors are four declarations of one idea — a
named place on the page — differing only in whether the place is fuzzy and whether it continues. An
author picks one at declaration time and then cannot obtain the properties of another, because
tolerance is a property of one type and continuation of a different one. A fuzzy header band that
continues across pages is not expressible in any of them.

The proposal is one `Region` declaration carrying `Tolerance`, `Start`/`End` boundaries, and
`Continuation` as optional properties, with the named layout zones retained as presets. Anchors stay
distinct — they are content-derived points, not places — but should yield regions, so that
anchor-relative selection is expressed in the same vocabulary as everything else.

**Impact**

- Four concepts to learn before authoring, where the differences are properties rather than kinds.
- Properties cannot be combined, so real layouts fall between the available types.
- Positional logic is expressed differently depending on which type was picked, making rule sets
  harder to read and compare across families.

**Completion criteria**

- [x] One region declaration subsumes named zones, toleranced zones, and flow regions.
- [x] Tolerance and continuation are independent properties, valid in combination.
- [x] Anchors yield regions usable anywhere a region is accepted.
- [x] Named layout zones remain available as presets over the unified type.
- [x] Existing declarations migrate mechanically, and the serialized schema expresses the unified
  form.

---

## RRM-044: Content-independent structure attributes require rules

**Status:** Partial

**Priority:** P1

**Design:** [Architecture and Direction](rule-based-remediation-architecture.md#3-content-independent-attributes-are-declared-not-refined)

Most structure attributes in a known family are constant per slot: `/Lang`, `/Scope` on header cells,
`/ListNumbering`, `/Placement`, alt text for a fixed logo. Each one currently costs a Refine rule
whose predicate re-finds content that is *already bound to the slot needing the attribute*. The rule
carries no information the declaration does not already have.

Content-independent attributes belong on the template node, extending the prescriptive invariant to
cover the declared nodes and their content-independent properties. Refine then exists only for
attributes genuinely derived from content — alt text from a caption, `/ColSpan` from geometry.

The preview template and JSON schema currently support `/Lang`, alternate text, actual text, and
expansion on the Document root and child occurrences. `/Scope`, `/ListNumbering`, and
`/Placement` remain deferred.

**Impact**

- A large fraction of a typical rule set is re-selection ceremony rather than mapping logic.
- Static attributes are expressed against churning rule ids rather than the stable slot.
- The distinction between "what this node is" and "how content is found" is blurred, in the direction
  that makes rule sets harder to review.

**Completion criteria**

- [ ] Template nodes carry content-independent attributes, validated against the tag at declaration
  time.
- [ ] Assembly applies declared attributes without a rule.
- [ ] A Refine rule targeting an attribute already declared on the slot is a validation error, not a
  silent last-writer-wins.
- [ ] Declared attributes round-trip through the serialized schema.

---

## RRM-045: Prescriptive content accounting is per item and its diagnostic identifies nothing

**Status:** Complete

**Priority:** P0

**Design:** [Architecture and Direction](rule-based-remediation-architecture.md#4-accounting-is-declared-by-region)

In prescriptive mode, painting content that matched no slot and no declared artifact is a
non-suppressible commit blocker (`RemediationSession.cs:2280`). The only way to clear it is an
`Artifact` rule per kind of incidental content — page numbers, rules, shading, watermarks,
continuation notices — so each new sample of a dynamic family yields new blockers and the accounting
backlog never closes. `RemediationArtifactInventoryItem.ZoneId` does not help: it constrains where a
*declared* artifact may appear, and says nothing about what a region's unbound content is.

The preview populates `unaccountedContent` per exact text span or atomic graphical item and applies guarded region accounting before leftover validation. Absorption creates ordinary artifact claims and reports each item with its region and artifact evidence.

**Impact**

- The strictness that makes prescriptive mode worth having is the thing that makes it impractical to
  reach a clean commit on a real family.
- Accounting is an open-ended rule backlog rather than a reviewable declaration.
- The diagnostic cannot be acted on without instrumenting the engine.

**Completion criteria**

- [x] The artifact inventory supports region-scoped absorption: unbound painting content in a
  declared region is a declared artifact of a stated subtype.
- [x] Absorption is bounded by the declared region and remains a blocker everywhere else.
- [x] Absorbed content is reported per item, so a reviewer can see what a catch region swallowed.
- [x] `unaccountedContent` is populated in prescriptive mode, identifying each unaccounted item.
- [x] Region-scoped absorption round-trips through the serialized schema.

---

## RRM-046: Declared reading order is never checked against document order

**Status:** Partial

**Priority:** P0

**Design:** [Architecture and Direction](rule-based-remediation-architecture.md#the-guardrail)

The preview program compares declared direct-child order with page-aware content traversal and
geometric evidence at the Document root and recursively through assembled composite occurrences.
`RequireSourceAgreement` blocks an inversion from either available evidence source;
`AllowDeclaredReorder` retains an acknowledged report item. Validation against real producer
families and richer layouts remains outstanding.

This is a deliberate consequence of the template owning order, and it is correct for intentional
reordering. It is currently unqualified: there is no diagnostic anywhere when declared order and
content order disagree, so a template that states the wrong order produces a clean commit and an
output that reads incorrectly. Reading order is most of what PDF/UA exists to guarantee, and every
gap above moves more authority onto the template.

**Impact**

- The most consequential class of remediation error is the one with no diagnostic.
- veraPDF cannot catch it: the output is well-formed, and wrong.
- A template authored from a misread sample stays wrong silently across an entire family.

**Completion criteria**

- [ ] Declared sibling order is compared against document order under a prescriptive template.
- [ ] Divergence is reported per slot, naming the declared and observed positions.
- [ ] Intentional reordering is a per-slot opt-in, not a global suppression.
- [ ] A fixture covers declared order diverging from content order, and fails before the change.

---

## Suggested delivery order

The dependency reasoning below is scheduled into milestones with entry and exit gates in the
[Delivery Plan](rule-based-remediation-plan.md). Where the two differ, the plan is the operative
schedule — it also folds in two authoring-layer prerequisites tracked in
[accessibility_gaps_2.md](accessibility_gaps_2.md).

0. **Assurance and authoring tooling first.** RRM-016 rule cardinality, RRM-018 output assertions,
   RRM-033 per-rule match diagnostics, and RRM-037 forward candidate inspection. These are small
   relative to the rest and they are what makes every later item verifiable rather than merely
   implemented. Until a rule that stops matching produces a failure, no other fix can be trusted in
   a batch. RRM-037 belongs in this band for the same reason from the other side: without it, rules
   are authored against a document nobody can see in the engine's terms, so early iterations
   diagnose the author rather than the document. RRM-041 named predicates rides along: it is small,
   depends on nothing, and its main payoff is that traces print a label name instead of an expanded
   boolean tree, which is what makes RRM-033 readable in practice.
1. RRM-032 text normalization and RRM-026 input conformance pre-flight. Both are near-certain first
   contact failures with real third-party PDFs, and both are cheaper to fix before RRM-011 rather
   than as fallout from it.
2. RRM-003 regression tests and RRM-006 table hierarchy correction.
3. RRM-010 external validation baseline.
4. RRM-027 through RRM-031 contract specification. Mostly documentation and small decisions, but
   they unblock everyone authoring rules and several are prerequisites for later work
   (RRM-028 gates RRM-001).
5. RRM-001 cross-page evaluation model, with the table-continuation part of RRM-005. Moved ahead of
   content accounting by the M0 page-locality decision: the evaluation loop must become stage-major
   before further vocabulary encodes the page-local assumption.
6. RRM-002 non-text selection and artifact handling, with RRM-019 existing annotations and RRM-022
   artifact subtypes. These three together are what "all content is accounted for" actually requires.
7. RRM-039 phase one structural template, descriptive, with RRM-040 artifact inventory. Ahead of the
   remaining vocabulary because it is what makes every later addition checkable, and ahead of
   RRM-004 because it changes that decision.
8. RRM-004 stage/dependency decision and nested grouping, now informed by the descriptive template.
   Still ahead of the remaining vocabulary for the reason RRM-001 was: every action added to the
   Group stage encodes the current flat-snapshot assumption.
9. RRM-036 tolerance calibration, then RRM-005 irregular tables — spans, wrapped cells, sparse rows
   — with RRM-025. Calibration first: RRM-005 needs somewhere to put the tolerances it exposes, and
   authoring against uncalibrated constants is what RRM-036 exists to stop.
10. RRM-017 rule-set applicability, rescoped against what RRM-039 delivers for free, once enough real
    families exist to know what a fingerprint should assert beyond it.
11. RRM-020 pre-existing and optional content; RRM-021 list interiors; RRM-023 heading levels;
    RRM-024 navigation.
12. RRM-012 multi-column reading order.
13. RRM-007 through RRM-009 semantic expansion.
14. RRM-013 schema and validation hardening, RRM-038 schema parity, plus the remaining RRM-035
    policy items.
15. RRM-034 evaluation limits, when rule sets become externally supplied.
16. Revisit intentional limitations RRM-014 and RRM-015 only when product scope
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

Several items above — RRM-016, RRM-017, RRM-018, RRM-023, RRM-032, RRM-036 — share a single
underlying observation, and it is worth stating plainly because it shapes how the veraPDF baseline in
RRM-010 should be interpreted.

External validation proves the output is well-formed PDF/UA. It does not prove the output is right.
A document in which the invoice total was swept into an artifact by the leftover policy, the footer
was tagged `H1`, or an amount landed one row from its description because a tolerance sat a
half-point from a decision boundary, passes every automated conformance check available. The
engine's own diagnostics are structural too: orphaned MCIDs, missing `/StructParents`, reading-order
drift.

Nothing in the current design asserts that the *semantics* the rule author intended are the semantics
the document ended up with. For a system whose purpose is to make documents readable by people who
cannot see them, that is the gap that matters most, and it is not closed by finishing RRM-010.
