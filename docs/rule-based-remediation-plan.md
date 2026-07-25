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

**Out of scope**

- OCR and image-only scans (RRM-015).
- Repair of existing or partial structure trees (RRM-014).
- Interactive AcroForm remediation (RRM-009 — planned, but post-MVP).
- Arbitrary unstructured publications.
- Nested lists, deeply nested generated hierarchy, complex/spanning tables.
- Rotated or vertical text unless the M0 corpus demands it.

---

## Prerequisites outside this tracker

Two defects in the authoring layer corrupt remediation output silently. The remediation engine
writes its marked content through `PageWriter` and its structure tree through
`StructuralSerializer`, so these are not optional and not parallelizable — they poison every
result produced before they are fixed.

| Item | Source | Why it blocks remediation |
| --- | --- | --- |
| Non-Latin-1 text strings corrupted on write | [gaps-2 #1](accessibility_gaps_2.md#1-non-latin-1-text-strings-are-corrupted-on-write) | `/Alt`, `/ActualText`, `/E`, and `/T` values written by remediation actions are mangled outside Latin-1. Any corpus with non-ASCII content produces wrong output that still validates. |
| MCID counters are per-writer, not per-page | [gaps-2 #2](accessibility_gaps_2.md#2-mcid-counters-are-per-writer-not-per-page) | Collides MCIDs when a page is touched by more than one writer. ParentTree entries are silently overwritten and structure bindings are lost. Directly undermines RRM-016 and RRM-018 evidence. |

Both are scheduled inside M0.

---

## Milestones

### M0 — Foundations & scope lock

**Goal:** stop producing corrupt output, and make the decisions that constrain everything after.

| Work | Gaps |
| --- | --- |
| Fix text-string encoding on write | gaps-2 #1 |
| Fix per-page MCID allocation | gaps-2 #2 |
| Enumerate supported document families in writing | prerequisite for RRM-011, RRM-017 |
| Collect representative real inputs per family, with layout and data variation | prerequisite for RRM-011 |
| **Decide the page-locality question** | RRM-001 scope decision |
| Confirm RRM-014 / RRM-015 remain out of scope | RRM-014, RRM-015 |

**The page-locality decision is the most consequential item in this plan.** The evaluation model
is currently per page. Making flow regions and tables span pages is not an additive change — it
alters the evaluation loop. Every candidate type and action added in M4 and later will encode the
page-local assumption, and the retrofit cost grows with each one.

Decide in M0, even if the implementation lands in M6:

- If the initial families repeat complete boundaries on every page → RRM-001 is deferred, and M4
  may assume page-locality freely.
- If any logical section or table starts on one page and ends on another → RRM-001 is an MVP
  requirement, moves into M3/M4, and M4's vocabulary work must be designed against the
  cross-page model rather than retrofitted to it.

**Exit gate**

- [ ] Both prerequisite defects fixed, with regression tests.
- [ ] Supported families and explicitly unsupported cases written down.
- [ ] At least one real input per family from the actual producing system, not synthesized.
- [ ] Page-locality decision recorded, with the corpus evidence behind it.

**Estimate:** 3–4 days.

---

### M1 — Assurance layer

**Goal:** make every later fix verifiable rather than merely implemented.

| Gap | Work |
| --- | --- |
| RRM-016 | Rule cardinality — expected match counts, so template drift fails the run |
| RRM-018 | Semantic output assertions |
| RRM-033 | Per-rule match diagnostics and negative explain |
| RRM-010 | veraPDF / external PDF/UA-1 validation baseline in CI |

This is scheduled first among gap-closing work for one reason: until a rule that *stops matching*
produces a failure instead of degraded output, no other fix can be trusted in a batch. Everything
downstream is measured with these instruments.

Pair RRM-016 with the `AutoArtifact` hazard directly. The current interaction — content that no
rule claims gets swept into an artifact — is how a rule set silently converts an invoice total
into hidden content while passing every conformance check available.

**Exit gate**

- [ ] A rule whose target content is removed from the input fails the run.
- [ ] Dry-run reports per-rule match counts, including zero-match rules.
- [ ] At least one semantic assertion form exists and is exercised by a fixture.
- [ ] veraPDF runs in CI against a fixture set and fails the build on regression.
- [ ] `AutoArtifact` usage is reported in the run output with the content it absorbed.

**Estimate:** 3–5 days.

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
| RRM-027 | Confidence model — what the number means; `DefaultConfidence = 0.0` makes `MinConfidence` a trap today |
| RRM-028 | Anchor resolution scope — per page in practice, documented as absolute |
| RRM-029 | Structure sibling order and the reading-order default |
| RRM-030 | `DryRun` → `Commit` guarantee, and failure semantics |
| RRM-031 | Rule-set composition and precedence |
| RRM-035 | Remaining undocumented API surface and suppression policy |

Mostly documentation and small decisions, but disproportionately valuable: they unblock everyone
authoring rules, and **RRM-028 gates RRM-001** — cross-page flow cannot be specified until anchor
scope is.

If M0 made RRM-001 an MVP requirement, its evaluation-model change lands here, immediately after
RRM-028, rather than in M6.

**Exit gate**

- [ ] Each contract stated in `rule-based-remediation.md`, not only in the tracker.
- [ ] `MinConfidence` is usable with documented defaults, or removed.
- [ ] A dry-run that succeeds and a commit that then fails is either impossible or documented.

**Estimate:** 1–2 days.

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

**Estimate:** 3–6 days.

> **MVP total: 17–28 focused engineer-days.** Consistent with the tracker's 16–24 estimate plus the
> two prerequisite defects. These are engineering days with continued AI assistance, not calendar
> time. A reasonable allocation is three calendar weeks for one engineer.

---

### M6 — Structural model expansion *(post-MVP)*

**Goal:** remove the two architectural constraints in the model.

| Gap | Work |
| --- | --- |
| RRM-001 | Cross-page flow regions (if not already pulled into M3) |
| RRM-005 | Multi-page and irregular tables |
| RRM-025 | Table `/Scope` and `/Summary` |
| RRM-021 | List interiors and `/ListNumbering` |

RRM-021 carries a design decision larger than its priority label suggests. The action algebra has
no **split** primitive — `Group` reparents and `MergeTo` flattens, but nothing divides a claim.
This surfaces anywhere one rendered line carries two semantic roles: a list label plus its body, a
label plus its value. Design the split primitive here rather than working around it per rule.

**Estimate:** 5–9 days.

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
| RRM-004 | Group rules consuming prior Group outputs (nested grouping) |
| RRM-012 | Multi-column reading order |
| RRM-007 | Full Figure/caption association |
| RRM-008 | Text-orientation-aware anchor and table geometry |
| RRM-009 | Interactive form widgets |

RRM-004 is the second architectural item: the stage model is fixed at three stages, and multi-pass
grouping needs either explicit passes or a fixed point. Scope this milestone against actual demand
— all five items are conditional on the corpus.

**Estimate:** 5–15 days depending on which items are required.

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
| RRM-001 | P0 | M0 (decision) → M3 or M6 (implementation) |
| RRM-002 | P0 | M4 |
| RRM-003 | P0 | M2 |
| RRM-004 | P1 | M8 |
| RRM-005 | P0 | M6 |
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

Note that priority and schedule diverge deliberately. RRM-017 is P0 but scheduled at M7 because it
cannot be designed earlier; RRM-033 is P1 but scheduled at M1 because it instruments everything else.

---

## Critical path

```
M0 prerequisites (encoding, MCID)
   └─> M1 assurance (RRM-016, 018, 033, 010)
          └─> M2 first contact (RRM-032, 026, 003, 006)
                 └─> M3 contracts (RRM-027..031, 035)
                        ├─> RRM-028 gates RRM-001
                        └─> M4 content accounting (RRM-002, 019, 022)
                               └─> M5 real corpus (RRM-011)  ── MVP EXIT
                                      └─> M6 / M7 / M8 / M9
```

M0's page-locality decision reaches forward into M4: if cross-page flow is required, M4's
vocabulary must be designed against the cross-page model, not retrofitted after M6.

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
| Page-locality retrofit after vocabulary expansion | Medium–High — rework across M4 and M6 | Decision forced in M0, before any vocabulary lands |
| Conformance validation mistaken for correctness validation | High — false confidence at MVP exit | RRM-018 is an exit-gate item; see the tracker's closing note on what validation proves |
| Scope creep into tagged-PDF repair | Medium — RRM-014 is architecturally distinct | Re-decision required, not a backlog pull |
| Estimates assume the design is implemented as documented | Medium | Prior review passes found the design–implementation distance larger than estimates assume; M2 and M5 absorb this |

---

## What this plan does not cover

AI-authored rule sets — introspection APIs, derived rather than hand-authored geometry,
multi-document corpus input, claim-level diffing, and an independent correctness oracle — are a
separate effort. Every gap in the tracker concerns the *runtime*. The authoring layer is a second,
currently unscoped body of work and should be planned on its own once the MVP exits M5.
