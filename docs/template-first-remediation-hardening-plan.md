# Template-First Remediation — Correctness and Hardening

Last updated: 2026-07-30

Successor to [the prescriptive template plan](template-first-remediation-plan.md). That unit
delivered the public surface — modes, `Bind`/`BindOver`, `FromSlot`, container synthesis, rollback.
This one makes the mode correct outside the shape its tests exercise, and closes
[RRM-039](rule-based-remediation-gaps.md) properly.

## Summary

Prescriptive assembly is implemented twice — once to build the planned tree and once to materialize
it — and the two have already diverged. Unify them behind one algorithm with two consumers, fix the
defects that divergence produced, and build the evidence the mode's claims need. The central
mechanism of the unit, `BindOver`, currently has no end-to-end test at all.

## Status

Factual assessment of what the current implementation supports.

| Capability | State |
| --- | --- |
| Descriptive mode, unchanged behavior | Working, covered |
| All-or-nothing commit rollback | Working, verified — clean object graph after a failed prescriptive commit |
| Leaf `Bind` into a single top-level composite slot | Working, covered by four unit tests |
| Container synthesis for unbound singular ancestors | Working for the single-root case |
| `FromSlot` targeting from Refine | Working, covered |
| Prescriptive leftover content is an error | Working |
| Templates with more than one top-level slot | Working, covered by shared assembly-plan ordering |
| `BindOver` / repeating composite slots | Working, covered end to end and in the two-profile corpus |
| Tables bound to a declared subtree | Working with an opaque derived interior; twenty-row order is covered |
| Annotation adoption into a slot | Working, including Refine through `FromSlot` |
| Prescriptive fixtures, veraPDF, raster/glyph invariance | Working through C-30 under both profiles |

RRM-039 remains complete: D1–D4, the `BindOver` fixture, and the hardening exit gates landed on 2026-07-30.

---

## Decision: one assembly, two consumers

`RemediationSemanticTree.AssemblePrescriptive` and
`RemediationSession.ApplyPrescriptiveTemplateAssembly` independently implement container synthesis,
parent resolution, and child ordering. They differ today in root handling, sort stability, and
identity assignment, and every difference between them is reachable only as a
`TemplateMaterializationDivergence` with a message that points at the symptom rather than the cause.

This is the failure mode the M3c work named for tables: *produce one deterministic planned shape and
materialize that shape rather than maintain parallel hierarchy algorithms.*

**Extract one assembly function** that takes the applied claims plus the template and returns an
ordered tree of `(slotId, occurrenceIndex, claim?)` nodes — the *assembly plan*. Both consumers read
it:

- the planned semantic tree projects the assembly plan into `RemediationSemanticNode`s;
- materialization walks the same plan, creating or reparenting one `StructureNode` per node.

Consequences worth accepting deliberately:

- Ordering, synthesis, and occurrence numbering can then only be wrong in one place, and the
  divergence check goes back to being a safety net rather than the primary detector.
- The assembly plan is computed during evaluation, so it is available in dry-run output and can be
  reported directly — see *Authoring surface* below.
- `RemediationSession.BuildPrescriptiveTemplateClaims` and the semantic tree's `MutableNode.Synthetic`
  both disappear into it. Today a synthesized container exists as three unrelated objects: a
  synthetic claim with id `template:{slot}:1`, a `MutableNode` with a fresh `ClaimId.New()`, and a
  `StructureNode` with `/ID = template:{slot}:N`. One of those is non-deterministic across runs.

Do this first. Every defect below is cheaper to fix once, inside it.

---

## Confirmed defects

**D1 — Templates with more than one top-level slot produce `TemplateWrongOrder`.**
Declared order is honored one level below the root and nowhere else. `AssemblePrescriptive` sorts
each root's *children* by template rank but orders the root collection by
`rootSet.OrderBy(x => x.Node.PageIndexes.FirstOrDefault(int.MaxValue))` — page index only. The
materialization side never sorts root children at all, because `SortTemplateChildren` is seeded from
`nodeSlots.Keys` and the structure root is not slot-bound.

Reproduced with a template declaring `alpha` (H1) then `beta` (P) against content painting Beta
above Alpha: planned roots came out `P#beta, H1#alpha`, and both dry-run and commit failed with
`TemplateWrongOrder: Template mismatch at 'Document/alpha'`. All four shipped prescriptive tests use
a single top-level `Sect`, which is why nothing catches it.

*Fix:* treat the template `Document` root as a first-class assembly node so its children are ranked
like any other slot's. Falls out of the unification.

**D2 — `SortTemplateChildren` permutes non-slot-bound children.**
Its comparator returns `0` for any pair not present in `ranks`, and `List<T>.Sort` is introsort —
unstable above 16 elements. Confirmed with n=40: `0,22,23,24,25,...`. Under a prescriptive `Table`
slot the `TR` children are not slot-bound, so a table with more than sixteen rows has its rows
reordered during materialization. The planned side uses `OrderBy(...).ThenBy(x => x.index)` and is
stable, so this surfaces as a divergence rather than silent corruption.

*Fix:* rank-then-original-index ordering everywhere, never a bare comparator that can return `0`.

**D3 — `StructureLinkRemediationAction` escapes the prescriptive slot requirement.**
`RemediationStructuralTemplateValidator.IsStructureProducing` omits it even though `ProducedTag`
maps it to `Link`. A Refine link rule therefore creates a `Link` element in prescriptive mode with
no slot and no declaration error, and it runs *after* template assembly, so the node is never ranked
or validated.

*Fix:* include it in `IsStructureProducing`, and decide whether `Link` is bindable to a slot or
whether prescriptive mode routes links through `AdoptAnnotation` only. Either answer is fine;
silence is not.

**D4 — Table interiors carry the table's slot id, so declared table subtrees cannot match.**
`RemediationSemanticTree.BuildTableNode` stamps `GetSlot(claim, slots)` onto every synthetic `TR`,
`TH`, and `TD` node, so they all carry the *table's* slot. Prescriptive mode requires an id on every
non-`Document` node, so a declared `Table > TR > TD` subtree has ids the produced nodes will never
match — `RemediationStructuralTemplateMatcher.Matches` compares `expected.Id` against
`actual.SlotId`.

This is inference from the code, not a reproduction: verify it first, because the fix differs by
cause. Either table interiors participate in slot identity (rows and cells get their own occurrence
identities from the declared subtree) or the template treats a bound `Table` slot as opaque and
validates its interior structurally rather than by slot. The second is smaller and probably right —
the grid is derived from the document, not declared.

---

## Unproven mechanisms

Listed separately from defects because the work is evidence, not repair. Each is a fixture that
should exist before the mode is called complete.

- **`BindOver` end-to-end.** It appears once in the whole suite, in a JSON parse assertion checking
  `Over!.DebugString`. Nothing evaluates or materializes a repeating composite slot. This is the
  mechanism for repeated sections, line items, and nested lists — the reason the unit exists.
- **Repeating-composite validation.** The rule "repeating composite slot requires exactly one
  `BindOver` producer" is unexercised, as is the singular-composite counterpart.
- **Parent resolution when a bound parent is not in the child's `groupOwners` chain.**
  `ResolveTemplateParent` walks `groupOwners` upward and calls `EnsureSyntheticTemplateParent` when
  the walk does not reach the parent slot. That function does not check whether the parent slot is
  already directly bound, so a second container for an already-bound slot looks reachable — with a
  duplicate `template:{slot}:1` `/ID`. Prove or disprove with the `BindOver` fixture.
- **Nested repeated structures across passes**, which is the composition of the two above.
- **`TemplateSlotContentMode.FlattenLeafClaims`** under a slot. Note the current ordering coupling:
  `ApplyGroupClaim` flattens before assembly assigns `/ID`, and `CanFlattenBinding` refuses any node
  that already has an `/ID`. Correct today, fragile if assembly ever moves earlier — pin it with a
  test rather than a comment.

---

## Identity and provenance

- Make occurrence identity a value in the assembly plan — template path, parent occurrence,
  occurrence index — rather than a formatted string discovered late. Materialization renders it into
  `/ID`; the planned tree exposes it on `RemediationSemanticNode`; reports and `--explain-rule` print
  it. Today the planned tree has no occurrence index at all, so the plan's "used consistently in
  dry-run trees, Refine targeting, reports, and committed structure" is half-delivered.
- `RemediationSemanticTree.FromStructure` currently recovers slot identity by string-prefix parsing
  `/ID` (`TemplateSlotFromNodeId`). Decide whether `/ID` is the durable carrier — it is genuinely
  useful for re-entrant tooling and round-trip — and if so, say so in the reference doc, guard
  against colliding with a pre-existing `/ID` that starts with `template:`, and assert IDTree
  uniqueness. If not, carry identity out-of-band and leave `/ID` to authors.
- Synthetic planned nodes use `ClaimId.New()`, which is non-deterministic across runs. Dry-run output
  should be byte-stable for the same input; make synthesized identities derived, not generated.

---

## Contract clean-up

- **`RuleCardinality` on slot-bound rules.** The prohibition was deleted outright rather than scoped
  to prescriptive mode, silently relaxing descriptive-mode validation. Whichever behavior is
  intended, pin it with a test and state it in the reference doc — cardinality is the RRM-016 drift
  instrument and authors need to know whether it is available.
- **`ValidatePrescriptiveRequiredSlots` is redundant and weaker than the path it duplicates.**
  Missing required slots are already caught pre-mutation by the descriptive matcher — verified: dry
  run reported `TemplateSlotUnfilled`, commit threw, and no `StructTreeRoot` was written. The
  post-mutation copy adds nothing, and it routes through `ReportDiagnostic`, so it is suppressible
  where the pre-mutation path is not. Delete it or promote it to a genuine post-assembly invariant
  check that the pre-mutation path cannot express.
- **Prescriptive leftover content** reports `DiagnosticCode.UntaggedContent`, which is suppressible.
  If unaccounted content under a prescriptive template is meant to be unwaivable, it needs its own
  code in the non-suppressible set alongside the composition codes.
- Duplicate missing-slot error: both `BindTemplateSlotRemediationAction.Validate` and the validator
  report it. Keep one.

## Authoring surface

Small additions, high leverage for anyone writing a template by hand.

- Emit the assembly plan in dry-run output: slot, template path, occurrence index, producing rule,
  consumed claims, and whether the container was synthesized or bound. This is the prescriptive
  analogue of RRM-037's candidate dump, and it is nearly free once assembly is unified.
- Report unbound declared slots and unbound structural rules as distinct, named conditions rather
  than as a generic template difference.
- Consider deriving slot ids from template path where the author has not supplied one, instead of
  requiring an id on every non-`Document` node. Templates for real families will be large, and every
  id is a name that must stay in sync with a rule. Weigh against
  `RemediationStructuralTemplateMatcher.Matches`, which distinguishes bound from unbound particles —
  auto-derived ids must not silently make every particle "bound".

---

## Fixtures and verification

The current prescriptive coverage is four unit tests in `RemediationStructuralTemplateTests`.
`RemediationFixtureGenerator` contains no prescriptive content, so there is no evidence that
prescriptive output passes veraPDF or is visually invariant.

- Add prescriptive fixtures to the generator for both PDF/UA profiles, in C# and JSON:
  - invoice with synthesized containers, required and optional leaf slots, and **repeated line items
    via `BindOver`**;
  - repeated anchored sections built with zones and flow regions;
  - a table bound to a declared slot, with more than sixteen rows, which pins D2 and D4 together;
  - annotation adoption into a compatible slot, refined through `FromSlot`;
  - a nested list assembled across Group passes — assembly only; RRM-021's split primitive and
    `/ListNumbering` remain open and this must not be read as nested-list authoring support.
- Add a multi-top-level-slot fixture whose declared order differs from content order. This is D1's
  regression test and the mode's headline claim.
- Assert for every prescriptive fixture: exact planned and committed trees, declared sibling order,
  stable occurrence identities across dry-run and commit, unchanged leaf MCIDs, unique ParentTree
  ownership, dry-run/commit parity, byte-stable regeneration, raster and glyph-box invariance, and
  veraPDF success.
- Assert that every existing descriptive and template-free fixture regenerates byte-identically.
- Negative coverage for the validation rules that currently have none: repeating composite without a
  producer, singular composite with two, direct binding to a composite, `BindOver` on a leaf slot,
  annotation adopted into an incompatible slot tag, and structural rule without a slot.

---

## Risks

| Risk | Mitigation |
| --- | --- |
| Unification is a rewrite of both paths at once and regresses descriptive mode | Descriptive mode does not use assembly at all; keep the new code behind the prescriptive branch and rely on byte-identical regeneration of existing fixtures as the guard |
| D4's fix reopens the table hierarchy design | Verify the cause before choosing. Treating a bound `Table` slot as opaque keeps the derived grid out of the declared model and is the smaller change |
| `BindOver` fixture exposes a design problem rather than a bug | That is the point of building it first. Repeated composites are the mode's reason to exist; better to learn it now than after fixtures depend on it |
| Occurrence identity change is a breaking change to `/ID` values | It is opt-in surface that has never shipped in a release; fix the representation before anything depends on it |

---

## Work sequence

1. Verify D4 against a real table under a prescriptive slot. It is the one finding inferred rather
   than reproduced, and its cause changes the assembly data model.
2. Extract the single assembly function and its plan representation; move both consumers onto it.
   D1 and D2 resolve inside it. Include occurrence identity as a value.
3. D3 and the contract clean-up items — small, independent, and cheaper before fixtures exist.
4. `BindOver` end-to-end fixture, plus the parent-resolution and repeating-composite validation
   cases it exercises.
5. Remaining prescriptive fixtures and the full assertion set, including veraPDF and invariance.
6. Assembly-plan dry-run output and the authoring-surface items.
7. Reference-doc updates and RRM-039 status.

### Exit gate

- [x] One assembly implementation; the planned tree and the materialized tree are projections of the
      same plan.
- [x] A template with several top-level slots whose declared order differs from content order
      materializes in declared order, with no template difference.
- [x] A table with more than sixteen rows under a prescriptive slot retains row order.
- [x] A repeating composite slot built with `BindOver` produces correct structure, stable occurrence
      identities, and no duplicate `/ID`.
- [x] Every structure-producing action either binds a slot or fails declaration validation in
      prescriptive mode.
- [x] Dry-run output is byte-stable for identical input, including synthesized containers.
- [x] Prescriptive fixtures exist for both profiles and pass veraPDF, raster, and glyph-box checks.
- [x] Existing descriptive and template-free fixtures regenerate byte-identically.
