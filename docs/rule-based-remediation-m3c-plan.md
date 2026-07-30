# M3c — RRM-004 Structural Composition

Last updated: 2026-07-30

Implementation plan for the [M3c milestone](rule-based-remediation-plan.md#m3c--structural-composition-model)
and [RRM-004](rule-based-remediation-gaps.md#rrm-004-stage-vocabulary-is-fixed-at-three-and-group-cannot-consume-group).
Depends on M3b (descriptive template, artifact inventory) being in place; the fixture work below
builds on the Tier 0 corpus changes currently unstaged in `test/PdfLexer.Tests/`.

## Summary

Keep the existing `Classify → Group → Refine` pipeline and add explicit Group passes. Pass 0
preserves current behavior; pass N consumes the structural frontier produced by lower passes. This
supports arbitrary parent-over-parent composition without introducing a general dependency graph,
while leaving M6 free to make declared template hierarchy prescriptive.

---

## Decision record

The milestone exit gate asks for the stage-versus-dependency decision *and its consequences*, so it
is recorded here rather than implied by the implementation.

**Chosen: explicit numbered Group passes.** A `Group` rule declares `groupPass`; passes are
evaluated in ascending numeric order, and each pass reads one immutable snapshot — the structural
frontier — produced by every lower pass.

**Rejected: rules declare what they consume and the engine topologically sorts.** It removes the
ceiling rather than raising it, but it buys expressiveness the corpus does not need. Selection here
is predicate-based, not reference-based: `over: ClaimIs("LI")` names a *shape*, not a rule, so there
is no edge to sort on without either forcing every consumer to name its producers or computing the
graph from evaluation results — which is a fixed-point problem, not a topological sort. Passes give
the same composition with an author-visible, diff-stable order.

**Rejected for now: let the prescriptive template carry declared hierarchy (M6 / RRM-039 phase 2).**
Cheaper where hierarchy is declared, but it does not cover hierarchy that is *inferred* — table
depth discovered from the grid, list depth discovered from indentation. Those need a bottom-up
assembly step regardless. Passes and prescriptive slots are complements, not alternatives.

**What passes foreclose.** Nothing in the action vocabulary: no new stage, no dependency
declaration, no new action type. What they do foreclose is *implicit* composition — a rule can never
again silently consume a peer's output by accident, because the frontier makes that unrepresentable.
That is the intended trade.

**Consequence for M6.** When the template becomes prescriptive, declared containers are created from
the template and rules bind claims into slots; Group passes remain for undeclared or open hierarchy
and for variable structural actions such as inferred tables. Passes do not have to be removed to get
there, and a rule set can use both.

---

## Public contract

- Add `Rule.GroupPass` (`int`, `init`) and a final constructor parameter `groupPass = 0`.
- Add optional JSON property `"groupPass"` under the existing `pdflexer.remediation.ruleset.v1`
  schema. Omitted means `0`; no schema version bump, because every v1 document remains valid and
  unchanged in meaning.
- Validation placement follows the existing precedent for `minConfidence`/`candidates`:
  - `groupPass < 0` throws `ArgumentOutOfRangeException` from the `Rule` constructor;
  - `groupPass != 0` outside `Stage.Group` is a `ValidateShape()` error, so it surfaces through
    `SerializedRemediationRules.ValidateDeclarations` before a PDF is opened.
- Evaluate passes numerically over the **distinct declared values** in ascending order. Sparse
  numbering (`0`, `10`, `20`) is legal and costs nothing; declaration/composition order is the
  stable order *within* a pass only.
- Rules in one pass read the same immutable frontier and cannot consume peer output.
- Explicit rule references — `ClaimPredicates.FromRule`, `BeforeClaim`, `AfterClaim` — to a self,
  same-pass, or higher-pass `Group` rule fail declaration validation. This extends the existing
  later-stage check in `ValidateRuleReference` from `Stage` to `(Stage, GroupPass)`. Classify claims
  and lower-pass Group claims remain valid references.
- `Override` keeps its Classify-only meaning (content-ownership replacement). It does **not** resolve
  a composition conflict and gains no Group-stage semantics; say so in the reference doc, because the
  name invites the assumption.
- Introduce no new stage, dependency declaration, or action type.

---

## The structural frontier

One definition, because most of the engine work follows from it.

The frontier is the set of applied claims that currently own a structure node with no structural
parent — the roots of the partially assembled tree.

| Event | Effect on the frontier |
| --- | --- |
| Pass 0 begins | Frontier = applied Classify claims |
| A `Group` claim is applied | Its consumed claims leave; the new parent claim enters |
| A `MergeTo` claim is applied | Its consumed claims leave **permanently** — flattening destroys their nodes; the merged parent enters |
| A claim-consuming `Table` claim is applied | Its consumed cell claims leave; the `Table` claim enters |
| A claim is consumed by nothing in a pass | Carried forward unchanged |

Never in the frontier, at any pass:

- **Table interiors.** `TR`/`TH`/`TD` are synthetic nodes in `RemediationTablePlan`, not claims. A
  higher pass can consume a `Table`; it can never address a row or a cell. Row/cell-level authoring
  is RRM-005, not this milestone.
- **Merged inputs.** `MergeTo` flattens leaves into the parent and clears their content items. A
  higher pass can consume the merged `P`; the fragments no longer exist.
- **Artifact and Refine claims.** Artifacts are not structure; Refine claims modify rather than
  produce.

---

## Engine changes

Touchpoints are in `src/PdfLexer/Remediation/RemediationSession.cs` unless noted.

**Evaluation** (`EvaluateDocument`, currently one `rules.Where(x => x.Stage == Stage.Group)` loop
over the `classifyClaims` snapshot):

- Begin pass 0 with applied Classify claims as the frontier.
- For each declared pass, in ascending order:
  - Evaluate `Group`, `MergeTo`, and claim-consuming `Table` actions against the frozen frontier.
  - Detect two outputs consuming the same frontier claim; report the consumer rule ids, the claim
    id, and the pass as a composition error.
  - If valid, replace consumed frontier claims with the new parent claims; carry unconsumed roots
    forward.
  - Stop higher-pass evaluation after an ambiguous pass, and say in the diagnostic that later passes
    were not evaluated, so the author does not read the missing output as a second defect.
- Record the pass on the claim (`RemediationClaim.GroupPass`, internal `init`) so ordering,
  reporting, and traces can use it without re-deriving it from the rule.
- Keep one aggregate `Stage.Group` claim snapshot per page for Refine visibility; it now contains
  parents from every pass, sorted by reading order as today.

**Plan validation** (`ValidatePlan`), before semantic-tree construction:

- one structural parent per claim;
- no consumer selecting both a node and one of its descendants;
- no cycles — self/same/higher-pass references are rejected during preflight, backed by a defensive
  runtime DFS over `RelatedClaims`. `RemediationSemanticTree.BuildNode` already carries an
  `ancestors` guard that silently truncates a cycle into a childless node; that guard stays, but it
  must stop being the only thing standing between a cyclic plan and a committed document.

**Materialization** (`ApplyPlan`):

- Materialize Group claims from a document-scoped ordered list — ascending pass, then reading order
  within a pass — rather than the current page-major walk over per-page snapshots. For pass 0 this
  is provably identical: `CompareClaimsInReadingOrder` sorts on `PageIndex` first, so page-major over
  page-filtered reading-ordered lists is already global reading order.
- Preserve leaf MCIDs through every parent. Grouping creates structure nodes only; it must never
  allocate or duplicate MCIDs.
- Keep Refine unchanged: it runs **once, after the final Group pass**, and may target leaves or any
  produced parent from any pass.
- Preserve byte/materialization behavior for valid existing rules using the default pass 0. Existing
  ambiguous reparenting plans intentionally become failures.

### Sibling ordering — fix before adding passes

`ApplyGroupClaim` and `ApplyMergeClaim` create their parent with `Structure.AddElement(...)`, which
appends to the structure root, and only then reparent children into it. Root sibling order is
therefore *creation* order, not reading order. `ApplyClaimConsumingTableClaim` already corrects for
this by calling `PositionNodeByFirstMcid`; the group and merge paths do not.

Today this is survivable because all group parents are created in one pass, after all leaves. With
passes, every pass-N parent is appended after every pass-(N−1) parent, so any document whose grouped
content precedes ungrouped content materializes out of reading order. The failure is visible but
misleading: `ReadingOrderDrift` when no template is declared, `TemplateMaterializationDivergence`
when one is — neither of which names the real cause, and neither of which the author can fix except
by adding a root-scoped `ReorderSiblings` refine rule.

**Change:** call `PositionNodeByFirstMcid` for group and merge parents as the table path does.

- It cannot change a currently-passing run. Repositioning a node by first content position only
  moves nodes that are currently out of `(page, MCID)` order — exactly the runs that currently
  emit `ReadingOrderDrift` or a materialization divergence.
- Pin the empty-node edge case: `GetFirstContentPosition` returns `ContentPosition.None`
  (`int.MaxValue`), so a parent with no content items sorts last rather than first.
- If regenerating the existing fixture corpus shows any byte change from this alone, that fixture
  was already mis-ordered and the diff is the finding — record it, do not paper over it.

---

## Predicate semantics under composition

The frontier changes what claim predicates see. Each of these is a contract decision, and each needs
a test, because all of them fail silently rather than loudly.

| Predicate | Behavior over a higher pass | Action |
| --- | --- | --- |
| `ClaimIs`, `ActionIs`, `FromRuleSet` | Match parents by `ProducedTag` / action kind; already correct for `Group`/`Merge`/`Table` | Document; cover by fixture |
| `FromRule` | Explicit reference; lower-pass legal, same/higher/self rejected at preflight | Extend `ValidateRuleReference` to `(Stage, GroupPass)` |
| `BeforeClaim`, `AfterClaim` | **Positional, not consuming.** If they resolve against the frontier, they stop matching the moment their referent is consumed by a lower pass | Resolve against the full applied-claim set, not the frontier; document the split |
| `Consecutive` | Compares `LastSequenceIndex + 1 == FirstSequenceIndex`; over parents this means leaf-range adjacency, which is the intended reading | Define it in the reference doc; cover by fixture |
| `Within(LayoutCoord \| zone \| anchor)` | Requires `RemediationClaim.BoundingBox`, which is **null for any multi-page claim** | Known limitation: a cross-page pass-0 parent cannot be selected geometrically. Negative test plus a diagnostic; the workaround is `Within(flowRegionId)`, which iterates `PageIndexes` and does work |
| `SamePage` | Compares primary `PageIndex` only; for a cross-page parent that is its first page | Document |

The `Within` row is the one interaction between M3a and M3c that will bite an author who does not
know the internals. Emitting a diagnostic when a geometric claim predicate skips a claim solely
because it spans pages is worth more than the doc line.

---

## Diagnostics

- New codes, appended to `DiagnosticCode`: `GroupCompositionAmbiguous`, `GroupCompositionCycle`.
- "Non-suppressible" needs a mechanism. Every code routed through `ReportDiagnostic` is suppressible
  under permissive strictness, and the existing hard errors bypass it with a bare `diagnostics.Add`,
  which loses the machine-readable code. Add a non-suppressible code set checked inside
  `ReportDiagnostic` so composition errors keep a code *and* cannot be waived. Record the addition
  against RRM-035's suppression policy.
- Add the authoring-trap explanation: when a Group rule matches zero claims and a rule it references
  produced claims that a lower pass consumed, say that, rather than leaving RRM-016 cardinality to
  report a bare `matched: 0`. This is the single most likely first-contact mistake with passes.
- Surface `GroupPass` in `RuleEvaluationSummary` (which already carries `Stage`) and in
  `--explain-rule` output, so a per-rule trace says which frontier it was evaluated against.

---

## Fixtures and tests

**Corpus.** Add C-29 to [the corpus spec](rule-based-remediation-corpus.md) as a Tier 0 entry with
the other composition fixtures, then implement it in `RemediationFixtureGenerator` as two strict,
embedded-font, closed-template variants:

- `C-29-a` — nested list assembled from independently painted label/body components across explicit
  passes, avoiding any claim that RRM-021's split primitive or `/ListNumbering` is complete.
- `C-29-b` — two sibling two-level sections producing `Sect > H1/P/Sect > H2/P`, proving independent
  parent chains do not cross.

Both templates are literal nested `RemediationStructuralTemplateNode` trees; the template language
is closed and ordered, so fixed nesting depth is expressible today and no template change is needed.
Note the constraint that only the template-owning rule set may bind slots — the higher-pass rules
must live in that set.

**Assertions.** Give every structural rule a template slot and assert empty planned *and*
materialized template differences, plus:

- dry-run/commit parity;
- exact structure order, including root sibling order;
- unique ParentTree ownership;
- unchanged leaf MCID sets at every parent layer;
- raster-exact output and glyph-box invariance, through the existing
  `Remediation_Fixtures_Are_Visually_And_Glyph_Position_Invariant` path;
- unchanged veraPDF results for UA-1 and UA-2.

**Integration.** A lower-pass `TableOver` and a lower-pass `MergeTo` both consumed by a higher-pass
section, in one document — the case where the three claim-consuming actions have to agree on what
the frontier is.

**Negative tests.**

- overlapping same-pass consumers;
- selecting an already-nested descendant alongside its parent;
- self, same-pass, higher-pass, and cyclic rule references;
- negative `groupPass` (constructor) and nonzero `groupPass` outside `Stage.Group` (declaration);
- a `Within(zone)` predicate against a cross-page pass-0 parent;
- a higher-pass rule attempting to consume a merged fragment or a table cell.

**Compatibility.** Omitted `groupPass` and explicit pass 0 produce identical output, including for
composed rule sets; and the full existing fixture corpus regenerates byte-identically through
`Remediation_Fixtures_Regenerate_Byte_Identically`.

---

## Risks

| Risk | Mitigation |
| --- | --- |
| Ambiguity detection turns currently-committing rule sets into failures | Intended, and stated in the contract. Run the full corpus early — before the fixture work, not after — so the blast radius is known while the design can still absorb it |
| Sibling-ordering change moves bytes in an existing fixture | Only possible where the fixture is already mis-ordered. Treat any diff as a finding to record, not a baseline to refresh |
| Passes are used where declaration order would do, producing rule sets that are hard to read | Authoring guidance: passes express *structural depth*, not evaluation convenience. One pass per level of the intended tree |
| Silent zero-match after a lower pass consumes the input | The consumed-input diagnostic above; RRM-016 cardinality as the backstop |

---

## Documentation and status

- `rule-based-remediation.md`: document Group passes in **Pipeline Stages**, the frontier semantics,
  the predicate table above, `groupPass` in the rule model and JSON surface, composition across rule
  sets, the new validation failures, and that `Override` has no Group-stage meaning. Remove the
  "Group rules that consume the output of earlier group rules" row from *What the Rule Language
  Cannot Express*.
- Document that M6 prescriptive templates will create declared containers directly; Group passes
  remain for undeclared/open hierarchy and variable structural actions such as inferred tables.
- Clarify that C-29 validates composition mechanics only; general nested-list authoring remains
  limited by RRM-021.
- Correct RRM-004's inconsistent tracker status (`Phase 1 complete; phase 2 deferred` belongs to
  template terminology, not to this gap), then mark RRM-004 and every M3c exit gate complete only
  after the full suite passes.
- Fix the two stale cross-references in the plan: the gap → milestone index lists RRM-004 under M3b,
  and M8's closing note says it moved to M3b. Both should read M3c.
- Preserve and build upon the current unstaged Tier 0 fixture changes without modifying unrelated
  work.

---

## Work sequence

Ordered by dependency; each step is independently verifiable.

1. Run the existing corpus against a same-pass ambiguity detector in dry-run only. Establishes the
   compatibility blast radius before any contract lands.
2. Sibling-ordering fix (`PositionNodeByFirstMcid` for group/merge) plus corpus regeneration.
3. `Rule.GroupPass`, JSON parsing, constructor/shape validation, `ValidateRuleReference` extension.
4. Frontier evaluation loop, claim `GroupPass`, ordered materialization.
5. Composition/cycle validation, non-suppressible code set, consumed-input diagnostic.
6. Predicate contract fixes — `BeforeClaim`/`AfterClaim` scope, cross-page `Within` diagnostic.
7. Report and `--explain-rule` surfaces.
8. C-29 corpus entry, fixtures, integration, negative, and compatibility tests.
9. Documentation and tracker status.

### Exit-gate mapping

| M3c exit gate | Delivered by |
| --- | --- |
| Stage-versus-dependency decision recorded, with action-vocabulary and M6 consequences | Decision record; step 9 |
| A later Group rule consumes an earlier one, deterministically | Steps 3–4; C-29 |
| Ambiguous or cyclic reparenting detected and rejected | Step 5; negative tests |
| Nested list and two-level section covered by fixtures | C-29-a, C-29-b |
| MCIDs unique and unchanged through every parent layer | Step 4; MCID assertions |

**Estimate:** 3–5 days, against the milestone's 2–4. The additions are the sibling-ordering fix, the
predicate contract decisions, and the non-suppressible diagnostic mechanism — none optional, none
large, and the first is cheaper now than after passes multiply the ways it can go wrong.
