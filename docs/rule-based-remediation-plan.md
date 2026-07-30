# Rule-Based Remediation — Delivery Plan

Last updated: 2026-07-24

This is the execution plan for closing the gaps recorded in
[Rule-Based Remediation Gap Tracker](rule-based-remediation-gaps.md) (RRM-001 … RRM-035).

The tracker says *what* is missing and *why it matters*. This document says *in what order*,
*under what gate*, and *when we are allowed to call it done*. The two are meant to be read
together; neither replaces the other.

| Document | Role |
| --- | --- |
| [rule-based-remediation.md](rule-based-remediation.md) | The rule language and API as shipped |
| [rule-based-remediation-gaps.md](rule-based-remediation-gaps.md) | Gap register — one section per RRM ID, with completion criteria |
| **This document** | Sequencing, milestones, gates, risks |
| [accessibility_gaps_2.md](accessibility_gaps_2.md) | Authoring-layer defects, two of which are hard prerequisites here |

---

## How to use this document

1. Work milestone by milestone. Milestones are ordered by dependency, not by priority label —
   a P1 that unblocks three P0s is scheduled before those P0s.
2. A gap is **closed** when the *Completion criteria* block in its own tracker section is
   satisfied. This plan never restates those criteria; it only schedules them.
3. When a gap closes, update its **Status** in the tracker's priority table *and* in its section
   heading block. Do not track status here — this document would drift.
4. Milestone exit gates are hard. A milestone with an unmet gate does not "mostly" pass; the
   next milestone's assumptions depend on it.

---

## Scope this plan delivers

Taken from the tracker's *Recommended MVP boundary*. Recorded here because several milestones
gate on it being written down and agreed.

**In scope**

- Untagged input PDFs, PDF/UA-1 target profile.
- A small number of known internal document families, enumerated by name in M0.
- Manually authored and reviewed rule sets. (AI rule authoring is a separate, later effort and is
  deliberately not planned here.)
- Extractable, predominantly horizontal text.
- Simple tables: explicit columns, no row or column spans.
- Non-text handling sufficient to artifact decorative content and tag meaningful images.
- Dry-run, diagnostics, JSON rules, CLI execution, external validation.
- Cross-page flow regions and table continuation (added by the M0 page-locality decision).

**Out of scope**

- OCR and image-only scans (RRM-015).
- Repair of existing or partial structure trees (RRM-014).
- Interactive AcroForm remediation (RRM-009 — planned, but post-MVP).
- Arbitrary unstructured publications.
- Nested lists, deeply nested generated hierarchy, complex tables with row/column spans.
- Rotated or vertical text unless the M0 corpus demands it.

---

## Prerequisites outside this tracker

Two defects in the authoring layer previously corrupted remediation output silently. They were
fixed as the first M0 work so later milestones can rely on lossless structure metadata and
page-scoped MCID ownership.

| Item | Source | Why it blocks remediation |
| --- | --- | --- |
| ✅ Non-Latin-1 text strings | [gaps-2 #1](accessibility_gaps_2.md#1-non-latin-1-text-strings-are-corrupted-on-write) | Resolved with ASCII/PDF-document encoding and non-ASCII/UTF-16BE selection across accessibility metadata. |
| ✅ Page/form MCID allocation | [gaps-2 #2](accessibility_gaps_2.md#2-mcid-counters-are-per-writer-not-per-page) | Resolved with owner-scoped allocation, existing-content seeding, and duplicate ParentTree guards. |

Both are scheduled inside M0.

---

## Milestones

### M0 — Foundations & scope lock

**Goal:** stop producing corrupt output, and make the decisions that constrain everything after.

| Work | Gaps |
| --- | --- |
| ✅ Fix text-string encoding on write | gaps-2 #1 |
| ✅ Fix per-page MCID allocation | gaps-2 #2 |
| Enumerate supported document families in writing | prerequisite for RRM-011, RRM-017 |
| Collect representative real inputs per family, with layout and data variation | prerequisite for RRM-011 |
| ✅ **Decide the page-locality question** | RRM-001 scope decision |
| Confirm RRM-014 / RRM-015 remain out of scope | RRM-014, RRM-015 |

**The page-locality decision is the most consequential item in this plan.** The evaluation model
is currently per page. Making flow regions and tables span pages is not an additive change — it
alters the evaluation loop. Every candidate type and action added in M4 and later will encode the
page-local assumption, and the retrofit cost grows with each one.

#### Decision (2026-07-28): cross-page spanning is required

Page-local evaluation is only correct when every page is self-contained. Multi-page tables, sections
crossing a page break, and continued lists all fail that test, and between them they cover most
document types other than single-page receipts and confirmations. Restricting the MVP to
self-contained pages would constrain the target corpus more than the remediation goal tolerates.

Consequences, per the branch this plan already specified:

- **RRM-001 is an MVP requirement**, not post-MVP. It moves out of M6 and is scheduled immediately —
  ahead of the remaining M4 vocabulary, so that vocabulary is designed against the cross-page model
  rather than retrofitted to it.
- The table-continuation portion of **RRM-005** couples to it and moves with it.
- MVP total rises from 17–28 to **20–33 engineer-days**. *(Figure as of this decision. Later review
  passes raised it further; see the current total at the end of M5.)*

The estimate rises less than the tracker's original 2–5 day contingency implies, because the output
stack is already cross-page capable: `StructureNode.ContentItems` carries a page per MCID, and
`StructuralSerializer` already emits `/MCR` references with per-item `/Pg` when a structure element
spans pages. The remaining work is confined to the remediation evaluation loop — inverting it from
page-major to stage-major while keeping MCID allocation and content ownership page-scoped.

This decision was made on structural grounds rather than corpus evidence. It does not need revisiting
if the sampled families turn out to be page-local; it would only have been reversible in the other
direction, and the retrofit cost is asymmetric.

**Exit gate**

- [x] Both prerequisite defects fixed, with regression tests.
- [ ] Supported families and explicitly unsupported cases written down.
- [ ] At least one real input per family from the actual producing system, not synthesized.
- [x] Page-locality decision recorded, with the reasoning behind it.

**Estimate:** 3–4 days.

---

### M1 — Assurance layer

**Goal:** make every later fix verifiable rather than merely implemented.

| Gap | Work |
| --- | --- |
| RRM-016 | Rule cardinality — expected match counts, so template drift fails the run |
| RRM-018 | Semantic output assertions |
| RRM-033 | Per-rule match diagnostics and negative explain |
| RRM-037 | Forward candidate inspection — what the engine sees before rules exist |
| RRM-041 | Named predicates (labels) in candidate and claim namespaces |
| RRM-010 | veraPDF / external PDF/UA-1 validation baseline in CI |

This is scheduled first among gap-closing work for one reason: until a rule that *stops matching*
produces a failure instead of degraded output, no other fix can be trusted in a batch. Everything
downstream is measured with these instruments.

**Progress (2026-07-29):** RRM-018 and RRM-033 are in progress. Assertion models, JSON/report
surfaces, planned-tree output, commit-blocking failures, candidate rejection traces, and CLI explain
mode are implemented. Trace-node completeness and missing-element assertion behavior now have
regressions. Remaining work is materialized-tree parity, claim-predicate tracing, richer leaf
reasons, and rollback/parity integration fixtures.

Pair RRM-016 with the `AutoArtifact` hazard directly. The current interaction — content that no
rule claims gets swept into an artifact — is how a rule set silently converts an invoice total
into hidden content while passing every conformance check available.

**Exit gate**

- [ ] A rule whose target content is removed from the input fails the run.
- [ ] Dry-run reports per-rule match counts, including zero-match rules.
- [ ] At least one semantic assertion form exists and is exercised by a fixture.
- [ ] veraPDF runs in CI against a fixture set and fails the build on regression.
- [ ] `AutoArtifact` usage is reported in the run output with the content it absorbed.
- [ ] The candidate index for a document can be dumped before any rule is authored, and its
      candidate ids are the ones `ExplainRejection` accepts.
- [ ] A named predicate can be declared, reused, and shown by name in a rejection trace.

**Estimate:** 5–8 days, raised from 3–5 by RRM-037 and RRM-041.

---

### M2 — First-contact hardening

**Goal:** fix the failures that real third-party PDFs will hit immediately, *before* M5 rather
than as fallout from it.

| Gap | Work |
| --- | --- |
| RRM-032 | Specify and implement text normalization for predicate matching |
| RRM-026 | Input-level conformance pre-flight — diagnose what cannot be remediated |
| RRM-003 | Regression tests for multiple inline claims in one text operator |
| RRM-006 | Correct table hierarchy; fix the known-incorrect documented example |

RRM-032 and RRM-026 are near-certain first-contact failures. Ligatures, soft hyphens, non-breaking
spaces, and decomposed forms defeat literal predicates; damaged or unusual input needs to fail with
a diagnosis rather than a stack trace or a wrong result.

**Progress (2026-07-29):** RRM-032 is complete. Predicate and anchor matching share the configurable
normalizer, reports expose raw and normalized text, regex syntax remains untouched, and original
source ranges remain authoritative for materialization.

**Exit gate**

- [ ] Normalization behavior documented in the rule language reference and covered by fixtures.
- [ ] Pre-flight reports unremediable input with the specific defect named.
- [ ] RRM-003 ordering is covered by a regression test, not an assumption.
- [ ] The table example in `rule-based-remediation.md` produces correct hierarchy, and its warning
      note is removed.

**Estimate:** 3–5 days.

---

### M3 — Contract specification

**Goal:** state the contracts rule authors are currently guessing at.

| Gap | Contract to specify |
| --- | --- |
| RRM-027 | Complete — `[0,1]` confidence contract and neutral default of `1.0` |
| RRM-028 | Complete — per-page anchor scope, filtering order, and zero-based occurrence selection |
| RRM-029 | Complete — stable top-to-bottom/left-to-right sibling ordering |
| RRM-030 | `DryRun` → `Commit` guarantee, and failure semantics |
| RRM-031 | Rule-set composition and precedence |
| RRM-035 | Remaining undocumented API surface and suppression policy |
| RRM-038 | Serialized schema parity with the shipped C# surface |

Mostly documentation and small decisions, but disproportionately valuable: they unblock everyone
authoring rules, and **RRM-028 gates RRM-001** — cross-page flow cannot be specified until anchor
scope is.

M0 made RRM-001 an MVP requirement, so its evaluation-model change lands here — see M3a below. The
gating dependency is satisfied: RRM-028 is Complete, so anchor scope is defined and cross-page flow
can be specified against it.

**Exit gate**

- [x] RRM-027 through RRM-029 are stated in `rule-based-remediation.md`.
- [x] `MinConfidence` is usable with documented defaults.
- [ ] A dry-run that succeeds and a commit that then fails is either impossible or documented.

**Estimate:** 1–2 days.

---

### M3a — Cross-page evaluation model

**Goal:** make flow regions, groups, and tables span pages, before any further vocabulary encodes
the page-local assumption.

| Gap | Work |
| --- | --- |
| RRM-001 | Stage-major evaluation, flow continuation state, continuation-aware anchor diagnostics |
| RRM-005 (part) | Multi-page table continuation and repeated-header policy |

Scheduled here rather than in M6 by the M0 decision. The ordering matters more than the size: this
must land **before** M4's remaining vocabulary, because every candidate type, action, and predicate
added first encodes page-locality and has to be revisited afterwards. RRM-002 has just landed, which
makes now the cheapest this change will be — its vocabulary is the newest in the tree rather than
the oldest.

The work is confined to the remediation evaluation loop. The DOM and serializer are already
cross-page capable, so this is a scheduling and state-scoping change, not a format change. MCID
allocation, text ownership, and content ownership stay page-scoped; only the claim set becomes
document-scoped.

**Completed 2026-07-29.** Evaluation and application are stage-major, continued flow activations
carry stable document identity, and structure placement and diagnostics compare page-qualified
MCIDs. Numeric header rows are explicitly scoped to the logical table or every page, while
`HeaderSelector` identifies actual repaints. Cross-page group/table materialization is covered by
dry-run and commit tests. RRM-005 retains adaptive irregular grids, spans, and section elements.

**Exit gate**

- [x] `CurrentPageOnly` and `ContinueUntilEnd` differ observably.
- [x] A region starts on page 1, continues through pages with neither boundary, and ends on page 4
      without spurious anchor diagnostics.
- [x] One logical table spans pages, with the repeated-header policy implemented and asserted.
- [x] MCIDs remain page-scoped, unique, and correctly referenced through `/MCR` entries.
- [x] The reading-order diagnostic handles content interleaved across pages.

**Estimate:** 3–5 days.

---

### M3b — Structural template, descriptive

**Goal:** make the structure a document is *supposed* to have an authored artifact, so output shape
can be checked instead of reviewed.

| Gap | Work |
| --- | --- |
| RRM-039 (phase 1) | Structural template in PDF structure vocabulary; positional tree diff |
| RRM-040 | Artifact inventory — expected page furniture on the page axis |

Design: [Structural Model for Rule-Based Remediation](rule-based-remediation-structural-model.md).

Today the expected structure exists only in the author's head, spread across rule ids, predicates,
and cardinality constraints. Because it is never stated it can never be checked, and that is the
root of the observation this plan closes with: nothing asserts that the semantics the author
intended are the semantics the document ended up with.

Phase one is **descriptive**. Rules build the tree exactly as they do now; the produced tree is
matched against the template and the difference reported positionally. Nothing about materialization
changes, and a rule set with no template behaves exactly as today. That keeps the change additive
and — more importantly — validates the content-model language against a real corpus before the
pipeline is made to depend on it in M6.

RRM-040 rides with it because artifacts are not structure elements and cannot live in the template.
It closes the `AutoArtifact` hazard directly: with a declared inventory, content artifacted that
matches nothing declared becomes an error rather than a silent success.

**Exit gate**

- [ ] A template is validated for PDF/UA legality and content-model determinism at declaration time,
      before any document is processed.
- [ ] The produced tree is diffed against the template, naming the position of each difference.
- [ ] Missing required, unexpected, misordered, and occurrence-violating nodes are distinct
      suppressible diagnostic codes.
- [ ] Content artifacted outside the declared inventory is reported.
- [ ] An optional section absent, an optional section present, and a repeating section are covered
      by corpus fixtures.
- [ ] A structure that is valid PDF/UA but wrong against its template fails the run.
- [ ] A rule set with no template produces byte-identical output to today.

**Estimate:** 5–8 days.

---

### M3c — Structural composition model

**Goal:** settle how rules compose into hierarchy, before any further vocabulary encodes the
current flat-snapshot assumption.

| Gap | Work |
| --- | --- |
| RRM-004 | Stage/dependency decision, then nested grouping |

`EvaluateDocument` runs every Group rule against a single `classifyClaims` snapshot taken once, so
a Group rule can never consume another Group rule's output. The narrow symptom is nested lists and
multi-level sections; the underlying gap is that the pipeline is three hard-coded phases rather than
a dependency order, and transactional structure is recursive — cell to row to table to section.

**Raised to P0 and moved here from M8 on 2026-07-29.** This is the same argument that pulled RRM-001
into M3a: every action added to the Group stage encodes the flat-snapshot assumption, so deciding
the stage model after M4 vocabulary means revisiting that vocabulary.

**Sequenced after M3b, deliberately.** The structural template offers a third option that is cheaper
than either of the two below: if the template becomes prescriptive in M6, declared hierarchy is not
assembled at all — rules bind claims into declared slots and nesting is something the template
states. That option is only evaluable once the descriptive template has met a real corpus, which is
why this milestone follows M3b rather than preceding it.

The decision itself is the deliverable. Keep three stages and give Group rules ordered visibility of
earlier Group output; replace stages with rules declaring what they consume and topologically
sorting; or let the template carry declared hierarchy and reduce Group to the undeclared cases.

**Exit gate**

- [ ] The stage-versus-dependency decision is recorded with its consequences for the action
      vocabulary, including what M6's prescriptive template would change.
- [ ] A later Group rule can consume an earlier Group rule's output, deterministically.
- [ ] Ambiguous or cyclic reparenting is detected and rejected.
- [ ] A nested list and a two-level section are covered by fixtures.
- [ ] MCIDs remain unique and unchanged through every parent layer.

**Estimate:** 2–4 days.

---

### M4 — Complete content accounting

**Goal:** every mark of ink on the page is either tagged or artifacted, deliberately.

| Gap | Work |
| --- | --- |
| RRM-002 | Declarative selection and artifacting of non-text content |
| RRM-019 | Adopt annotations already present in the input |
| RRM-022 | Artifact subtypes — Header / Footer / Watermark |
| RRM-007 (part) | Basic Figure handling with `/Alt` |

These three-and-a-half items are what "all content is accounted for" actually requires. Untagged
paths, images, and pre-existing annotations are the most common reason a document that looks
remediated fails validation or reads wrong.

**Progress (2026-07-29):** RRM-002 is complete. Typed selectors are required by the API and JSON,
graphical candidates have stable resource identities/names and normalized bounds, atomic ownership
and auto-artifact reporting cover painting items, and invocation-level Figure binding with `/Alt`
is verified. RRM-019 now inventories existing annotations and blocks strict conformance when they
cannot be adopted; adoption itself remains scheduled here.

**Exit gate**

- [ ] A page containing paths, images, and annotations remediates with no unaccounted content.
- [ ] Artifact subtypes round-trip and validate.
- [ ] Existing annotations receive `/StructParent` and appear in the structure tree.

**Estimate:** 4–6 days.

---

### M5 — Real corpus validation

**Goal:** prove the MVP against documents nobody on the team authored.

| Gap | Work |
| --- | --- |
| RRM-011 | Exercise representative internal PDFs; fix discovered blockers |
| RRM-036 | Calibrate geometric tolerances against the corpus rather than authoring constants |

RRM-036 lands here because calibration is meaningless without a family of real documents to
calibrate against, and because this is the first milestone where authored tolerances meet layout
variation the team did not create. Every geometric parameter in the language is currently a guess —
`SameRowAs(tolerance)`, `NearestTo(maxDistance)`, `TolerancedZone(Tolerance)`, table column
boundaries — and a wrong guess degrades output rather than failing the run, so neither veraPDF nor
RRM-016 cardinality will catch it.

This is where the unknown-unknowns live. Everything else in this plan is knowable from the design;
real producer output is not. Budget contingency here rather than trimming it — the M1 assurance
instruments exist precisely so this milestone produces evidence instead of impressions.

**Exit gate — this is the MVP exit gate**

Reproduced from the tracker so it can be checked in one place:

- [ ] Supported families and unsupported cases are written down (M0).
- [ ] Each family has representative inputs with meaningful variation (M0).
- [ ] One reviewed rule set handles those variants with no per-file code changes.
- [ ] Commits complete with no unexplained or unjustifiably suppressed diagnostics.
- [ ] Output passes veraPDF for PDF/UA-1.
- [ ] Rendering comparison shows no unintended visual change.
- [ ] Structure shape, MCID ownership, reading order, and non-text handling are covered by
      regression tests.
- [ ] Dry-run output lets a rule author understand unmatched, skipped, ambiguous, and
      low-confidence content.
- [ ] Failures identify document, page, rule, and source content.
- [ ] A rule that stops matching fails the run (RRM-016).
- [ ] Output correctness is asserted automatically, not only by human review (RRM-018).
- [ ] Any diagnostic suppression is listed, justified, and reviewed (RRM-035).
- [ ] `AutoArtifact` is unused in production, or bounded and reported.
- [ ] Authored tolerances are calibrated against the corpus, and any tolerance resolving near a
      decision boundary is reported (RRM-036).

**Estimate:** 5–9 days, raised from 3–6 by RRM-036.

> **MVP total: 32–52 focused engineer-days.** The tracker's 16–24 estimate, plus the two
> prerequisite defects, plus M3a cross-page evaluation (3–5 days) pulled in by the M0 page-locality
> decision, plus the 2026-07-29 review additions — RRM-037 and RRM-041 into M1, RRM-004 into M3c,
> RRM-036 into M5 — plus the structural model: M3b descriptive template and artifact inventory
> (5–8 days).
>
> The structural-model addition is the only one that buys back part of its cost. RRM-039 subsumes
> most of RRM-018's remaining assertion forms and supplies the weak form of RRM-017 for free, so
> the marginal cost is lower than the milestone estimate suggests. It is still the largest single
> addition to this plan and should be treated as a scope decision rather than a refinement.
>
> These are engineering days with continued AI assistance, not calendar time. A reasonable
> allocation is five to six calendar weeks for one engineer.

---

### M6 — Structural model expansion *(post-MVP)*

**Goal:** remove the two architectural constraints in the model.

| Gap | Work |
| --- | --- |
| ↗ RRM-001 | Moved to M3a by the M0 decision — cross-page flow is an MVP requirement |
| RRM-039 (phase 2) | Prescriptive template — declared slots drive materialization |
| RRM-005 | Irregular tables — spans, wrapped cells, sparse rows (continuation moved to M3a) |
| RRM-025 | Table `/Scope` and `/Summary` |
| RRM-021 | List interiors and `/ListNumbering` |

RRM-039 phase two is the payoff of M3b and the reason M3c's decision was deferred: once the template
drives materialization, declared hierarchy is no longer assembled bottom-up. Rules bind claims into
slots, and `L > LI > L > LI` is something the template states rather than something Group rules
construct. Do not commit to this until the descriptive template has run against a real corpus in M5 —
that is the whole reason phase one exists separately.

RRM-021 carries a design decision larger than its priority label suggests. The action algebra has
no **split** primitive — `Group` reparents and `MergeTo` flattens, but nothing divides a claim.
This surfaces anywhere one rendered line carries two semantic roles: a list label plus its body, a
label plus its value. Design the split primitive here rather than working around it per rule.

**Estimate:** 8–14 days, raised from 3–6 by RRM-039 phase two.

---

### M7 — Semantic vocabulary *(post-MVP)*

| Gap | Work |
| --- | --- |
| RRM-017 | Rule-set applicability guard / document fingerprint |
| RRM-020 | Pre-existing marked content and optional content |
| RRM-023 | Heading level ordering model |
| RRM-024 | Document navigation — outline, page labels |

RRM-017 is deliberately here rather than earlier: a fingerprint cannot be designed until enough
real families exist to know what it should assert. M5 produces that knowledge.

**Estimate:** 4–6 days.

---

### M8 — Advanced layout *(post-MVP, scope-dependent)*

| Gap | Work |
| --- | --- |
| RRM-012 | Multi-column reading order |
| RRM-007 | Full Figure/caption association |
| RRM-008 | Text-orientation-aware anchor and table geometry |
| RRM-009 | Interactive form widgets |

Scope this milestone against actual demand — all four items are conditional on the corpus.

RRM-004 was previously scheduled here. It moved to M3b on 2026-07-29 when it was raised to P0; see
that milestone for the reasoning.

**Estimate:** 4–12 days depending on which items are required.

---

### M9 — External rule-set hardening *(post-MVP)*

| Gap | Work |
| --- | --- |
| RRM-013 | Declarative tag and JSON schema validation |
| RRM-034 | Bounded predicate evaluation cost |

Required only when rule sets become externally supplied rather than internally authored.

**Estimate:** 2–3 days.

---

### Deferred indefinitely

| Gap | Status |
| --- | --- |
| RRM-014 | Existing tagged PDFs cannot be repaired — intentional limitation |
| RRM-015 | Scanned PDFs require an external text/OCR layer — intentional limitation |

Revisit only when product scope expands. Both are re-decisions, not backlog items.

---

## Gap → milestone index

| Gap | Priority | Milestone |
| --- | --- | --- |
| RRM-001 | P0 | M0 (decided: required) → M3a (implementation) |
| RRM-002 | P0 | M4 |
| RRM-003 | P0 | M2 |
| RRM-004 | P0 | M3b (raised from P1/M8 on 2026-07-29) |
| RRM-005 | P0 | M3a (continuation) → M6 (irregular grids) |
| RRM-006 | P0 | M2 |
| RRM-007 | P1 | M4 (basic) → M8 (full) |
| RRM-008 | P1 | M8 |
| RRM-009 | P1 | M8 |
| RRM-010 | P0 | M1 |
| RRM-011 | P1 | M5 |
| RRM-012 | P1 | M8 |
| RRM-013 | P1 | M9 |
| RRM-014 | P2 | Deferred |
| RRM-015 | P2 | Deferred |
| RRM-016 | P0 | M1 |
| RRM-017 | P0 | M7 |
| RRM-018 | P0 | M1 |
| RRM-019 | P0 | M4 |
| RRM-020 | P1 | M7 |
| RRM-021 | P1 | M6 |
| RRM-022 | P1 | M4 |
| RRM-023 | P1 | M7 |
| RRM-024 | P1 | M7 |
| RRM-025 | P1 | M6 |
| RRM-026 | P0 | M2 |
| RRM-027 | P1 | M3 |
| RRM-028 | P1 | M3 |
| RRM-029 | P1 | M3 |
| RRM-030 | P1 | M3 |
| RRM-031 | P1 | M3 |
| RRM-032 | P0 | M2 |
| RRM-033 | P1 | M1 |
| RRM-034 | P2 | M9 |
| RRM-035 | P1 | M3 |
| RRM-036 | P0 | M5 |
| RRM-037 | P0 | M1 |
| RRM-038 | P1 | M3 |
| RRM-039 | P0 | M3b (descriptive) → M6 (prescriptive) |
| RRM-040 | P0 | M3b |
| RRM-041 | P1 | M1 |

Note that priority and schedule diverge deliberately. RRM-017 is P0 but scheduled at M7 because it
cannot be designed earlier; RRM-033 is P1 but scheduled at M1 because it instruments everything else.

---

## Critical path

```
M0 prerequisites (encoding, MCID, page-locality decision)
   └─> M1 assurance + authoring tooling (RRM-016, 018, 033, 037, 041, 010)
          └─> M2 first contact (RRM-032, 026, 003, 006)
                 └─> M3 contracts (RRM-027..031, 035, 038)
                        └─> RRM-028 ──gates──> M3a cross-page model (RRM-001, RRM-005 part)
                                                  └─> M3b structural template, descriptive
                                                      (RRM-039 ph1, RRM-040)
                                                         └─> M3c composition model (RRM-004)
                                                                └─> M4 content accounting
                                                                    (RRM-002, 019, 022)
                                                                       └─> M5 real corpus + calibration
                                                                           (RRM-011, RRM-036) ── MVP EXIT
                                                                              └─> M6 / M7 / M8 / M9
```

M3a, M3b, and M3c sit on the critical path rather than beside it, for related reasons. The M0
decision made cross-page flow an MVP requirement, and each of these changes has to land before M4's
remaining vocabulary — every candidate type, action, and predicate added first encodes page-locality
(M3a), lands unchecked against any declared shape (M3b), and encodes the flat-snapshot stage
assumption (M3c), and must be revisited after. RRM-028 is Complete, so the M3a gate is open.

The M3b → M3c order is deliberate and is the one ordering decision in this plan most worth
preserving. The descriptive template offers a third answer to M3c's question — let declared
hierarchy come from the template rather than from Group rules — and that answer is only evaluable
once the template has been written against real documents. Settling the stage model first would
foreclose it.

RRM-037 and RRM-041 are in M1 rather than later because they are instruments, not features: without
a way to dump what the engine sees and to name recurring conditions, rules for a new family are
authored against a document nobody can inspect and traces print expanded boolean trees. RRM-036 is
at M5 because calibration requires a corpus to calibrate against.

---

## Per-gap definition of done

A gap is not closed until all of the following hold:

1. The *Completion criteria* in its tracker section are met.
2. A regression fixture exists that fails without the fix.
3. `rule-based-remediation.md` reflects the new behavior — including removing any warning note that
   the gap made necessary.
4. The tracker's priority table and the gap's own **Status** line both read `Closed`, with the
   PR or commit referenced.
5. If the gap added or changed rule-language surface, the JSON schema and CLI help are updated.

Item 3 matters more than it looks. Several gaps exist *because* the guide documents behavior that
is wrong or incomplete (RRM-006, RRM-028, RRM-035). Closing the code without closing the doc leaves
the gap half-open.

---

## Risk register

| Risk | Impact | Mitigation |
| --- | --- | --- |
| Real producer output invalidates design assumptions | High — could reopen M2–M4 | M5 has explicit contingency; M0 collects real inputs early so surprises surface before M4 |
| `AutoArtifact` silently absorbs semantic content | High — passes every conformance check, wrong for readers | RRM-016 in M1; guide already defaults examples to `FailFast`; exit gate bounds production use |
| ~~Page-locality retrofit after vocabulary expansion~~ | Retired 2026-07-28 | Decision made in M0: cross-page required, implemented in M3a before M4's remaining vocabulary. Residual exposure is RRM-002's just-landed vocabulary, which M3a revisits while it is still the newest code in the tree |
| Conformance validation mistaken for correctness validation | High — false confidence at MVP exit | RRM-018 is an exit-gate item; see the tracker's closing note on what validation proves |
| Scope creep into tagged-PDF repair | Medium — RRM-014 is architecturally distinct | Re-decision required, not a backlog pull |
| Estimates assume the design is implemented as documented | Medium | Prior review passes found the design–implementation distance larger than estimates assume; M2 and M5 absorb this |

---

## What this plan does not cover

AI-authored rule sets — introspection APIs, derived rather than hand-authored geometry,
multi-document corpus input, claim-level diffing, and an independent correctness oracle — are a
separate effort. Every gap in the tracker concerns the *runtime*. The authoring layer is a second,
currently unscoped body of work and should be planned on its own once the MVP exits M5.
