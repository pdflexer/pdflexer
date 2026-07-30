# Next Unit: Prescriptive Structural Templates

Last updated: 2026-07-30

Delivers the prescriptive second phase of
[RRM-039](rule-based-remediation-gaps.md#rrm-039-descriptive-structural-templates-validate-planned-and-materialized-output).
Builds on the descriptive template and artifact inventory, and on the immutable numbered Group
passes delivered under RRM-004.

## Summary

Add an opt-in template-first authoring mode in which the structural template is the source of truth for tags, hierarchy, occurrence, and sibling order. Rules locate PDF content and bind it to named template slots; existing anchors, zones, flows, immutable Group passes, and Refine rules remain the mechanisms for detection, repeated-instance composition, and semantic enrichment.

Existing template-free and descriptive-template rule sets remain unchanged. This unit uses generated fixtures rather than real internal documents, but must deliver the same authoring experience intended for those documents.

## Public Contracts and Rule Authoring

- Add `RemediationStructuralTemplateMode.Descriptive` and `Prescriptive`; constructors and JSON default to `Descriptive`. JSON accepts `"template": { "mode": "prescriptive", ... }` without a schema-version bump.
- In prescriptive mode:
  - every non-`Document` template node requires a unique `id`;
  - the template node's `tag` is authoritative;
  - structure-producing rules must bind a declared slot;
  - generic `Tag`, `Group`, and `MergeTo` rules cannot produce parallel structure.
- Add `BindTemplateSlotRemediationAction` and factories:
  - `RemediationActions.Bind()` for Classify rules binding matched painting content to a leaf slot;
  - `RemediationActions.BindOver(predicate, contentMode)` for Group rules forming a composite slot occurrence from lower-pass claims;
  - `TemplateSlotContentMode.PreserveChildren` and `FlattenLeafClaims`.
- Keep `Rule.Slot` as the binding target. Canonical JSON forms are:
  - `"slot": "title", "action": { "kind": "bind" }`;
  - `"slot": "section", "action": { "kind": "bind", "over": {...}, "contentMode": "preserveChildren" }`.
- Add `ClaimPredicates.FromSlot(slotId)` and JSON `fromSlot`. It matches resolved slot claims and can be used by later Group passes and Refine actions. Existing `FromRule`, `ClaimIs(tag)`, positional references, anchors, and tag predicates remain supported.
- `FromSlot` requires the slot on the claim. Today the slot is a `(RuleSetId, RuleId)` lookup assembled at report time and consumed only by `RemediationSemanticTree`; `RemediationClaim` carries no slot. Add `RemediationClaim.SlotId` alongside the existing `GroupPass`, populated at claim creation, so predicate evaluation does not depend on report-time state.
- Allow specialized `TableOver` and `AdoptAnnotation` actions to bind compatible slots because they provide behavior beyond generic composition. The template remains authoritative for their root tag, and their generated subtree must match the declared subtree.
- Artifacts remain outside the structural template and continue to use the artifact inventory. Static and content-specific accessibility properties remain Refine concerns rather than template-node fields.

## Constraints Inherited from the Descriptive Template

Each of these is enforced today by `RemediationStructuralTemplateValidator`. Prescriptive mode makes
every structural rule slot-bound, so each one changes from a narrow restriction into a property of
the whole authoring mode. Decide each explicitly rather than inheriting it by accident.

- **One template, one rule set.** At most one template may exist across composed rule sets, and a
  rule binding a slot must belong to the template-owning set. Prescriptive rule sets are therefore
  effectively single-set, which forecloses the shared-boilerplate composition story tracked by
  RRM-031. Either lift the restriction for prescriptive mode — permitting a template owner plus
  slot-binding rules from composed sets — or state the limitation in the reference doc.
- **`RuleCardinality` is rejected on slot-bound rules.** Template occurrence constrains *how many
  nodes exist*; cardinality constrains *how many inputs a rule matched*. They are not the same
  assertion, and the second is the drift instrument delivered under RRM-016. Prescriptive mode
  removes it from every structural rule. Either permit cardinality alongside a slot, or state that
  per-slot occurrence plus semantic assertions are the replacement.
- **Singular over-binding is already a declaration error.** A slot with `ExactlyOne`/`Optional`
  occurrence bound by more than one rule fails at declaration time. Reuse that check rather than
  reimplementing "excess singular bindings" at evaluation time.
- **`LegalChild` is permissive.** Its default arm is `_ => true`; only table, list, and cell nesting
  are constrained. Acceptable when the template is descriptive and rules build the tree. When the
  template *is* the authority for hierarchy, an illegal-but-unlisted nesting becomes uncatchable at
  declaration time. Tighten it against the PDF/UA content model, or record the gap.
- **`Pages` and `SpansPages` are currently checked, not authoritative.** State whether prescriptive
  mode continues to treat them as assertions or promotes them to partitioning inputs for repeated
  occurrences. They are the only page-axis constraints the template can express.

## Evaluation and Materialization

- Preserve the public `Classify → Group → Refine` model, with an internal template-assembly step between Group and Refine:
  1. resolve anchors, zones, flows, and Classify bindings;
  2. execute Group passes against immutable frontiers;
  3. instantiate the prescriptive template and deterministic slot occurrences;
  4. expose instantiated slot claims to Refine through `FromSlot`;
  5. validate and materialize the assembled tree.
- Instantiate singular composite ancestors automatically. Optional composites are created only when a descendant is bound; required composites are created when their required descendants are satisfied.
- Map one Classify binding claim to one leaf occurrence. Order different slots by template declaration and repeated occurrences by document-flow reading order.
- Require exactly one `BindOver` producer for every repeating composite slot. Each successful Group activation creates one occurrence, ordered by its earliest consumed child. Nested repeated structures are assembled bottom-up using existing sparse `GroupPass` semantics.
- Permit zero or one `BindOver` producer for a singular composite slot. Without one, its descendants are assembled automatically.
- Allow multiple rules to feed a repeating leaf slot, but retain the existing single-producer restriction for singular slots. Disallow direct candidate binding to composite slots and disallow automatic correlation of repeated sibling branches.
- Validate that `BindOver` consumes only descendant slots permitted beneath its target, consumes each claim at most once, and does not select an ancestor with its descendant. Reuse existing composition-cycle and ambiguity validation.
- Build deterministic template instance identities from template path, parent occurrence, and occurrence index. Use these identities consistently in dry-run trees, Refine targeting, reports, and committed structure.
- Extend reporting and CLI output with slot id, template path, occurrence index, source rule, candidate/claim ids, stage, and Group pass. Use existing template-difference codes where applicable and add non-suppressible binding-ambiguity and ownership-conflict diagnostics.

### Ordering authority

Prescriptive mode introduces a second authority over sibling order, and the two disagree exactly
where declaring order is worth doing.

- `ApplyGroupClaim` and `ApplyMergeClaim` currently call `PositionNodeByFirstMcid`, so materialized
  sibling order is derived from first content position. Under a prescriptive template, order is
  declared. State which wins, and scope the MCID-derived positioning to descriptive and
  template-free rule sets rather than leaving both active.
- `CheckReadingOrder` raises `ReadingOrderDrift` on any MCID inversion in the logical tree. A
  deliberately reordered prescriptive tree will trip it against an order the author declared on
  purpose. Decide whether prescriptive mode narrows the check to within-slot content, suppresses it
  with the template as justification, or treats declared order that inverts content order as an
  authoring error. Silently emitting a drift diagnostic for correct output is the one outcome to
  avoid.
- Repeated occurrences are ordered by document-flow reading order, so the two authorities coexist
  cleanly at that level; the conflict is confined to slot-to-slot order within a parent.

### Transactional model and failure semantics

The unit depends on being able to abandon a partially applied plan. That capability does not exist
today and is the largest unscoped item here — it is engineering work, not a test case.

- `ApplyPlan` mutates the structure tree and each page's `WorkingContent` as it walks claims, and
  returns on the first unsuppressed diagnostic. A failure part-way through leaves the in-memory
  document half-applied with no way back. This is the open behavior tracked by RRM-030.
- Automatic container synthesis makes the exposure worse than it is for rule-built trees: a
  prescriptive commit can fail after the engine has created structure nodes that no rule asked for,
  so the half-applied state is no longer explicable from the rule set alone.
- Deliver an explicit boundary before the binding work lands. The cheapest form that satisfies this
  unit is a structure-tree and working-content checkpoint taken before `ApplyPlan`, restored on any
  unsuppressed diagnostic, with `Commit` documented as all-or-nothing.
- Separate the two classes of failure in the contract, because only the first can be caught before
  mutation:
  - **Pre-mutation, blocks commit without touching the document** — missing required bindings,
    excess singular bindings, unresolved repeated-container partitioning, illegal slot ancestry,
    duplicate source ownership, incompatible specialized actions.
  - **Post-mutation, requires rollback** — planned/materialized divergence, which is computed by
    comparing the planned semantic shape against the shape read back from the committed structure
    and therefore cannot be evaluated before materializing.
- Unbound painting content needs a stated policy. `RemediationLeftoverPolicy` currently offers
  `Flag`, `FailFast`, and `AutoArtifact`, and `AutoArtifact` is the documented hazard that the
  artifact inventory closed. In prescriptive mode, content matching no slot and no declared
  inventory item should be an error rather than absorbed; say so, and say whether `AutoArtifact` is
  rejected outright when the template is prescriptive.

## Fixtures and Verification

- Add equivalent C# and JSON template-first fixtures for both PDF/UA profiles:
  - a fixed invoice structure with automatically synthesized `Document`/`Sect` containers, required title/body slots, optional fields, and repeated line items;
  - repeated anchored sections using zones and flow regions, with each section created by `BindOver`;
  - a nested list assembled bottom-up across multiple Group passes;
  - a table bound to a declared `Table` subtree;
  - a link/annotation adoption bound to a compatible template slot and refined through `FromSlot`.
- The nested-list fixture validates bottom-up assembly into declared slots only. RRM-021's split
  primitive and `/ListNumbering` remain open, so it must not be read as general nested-list
  authoring support.
- Add negative coverage for unknown or duplicate slot ids, structural rules without slots, tag duplication, missing required leaves, excess singular bindings, direct composite binding, absent repeated-composite producers, overlapping consumers, illegal descendant selection, ambiguous repeated partitions, incompatible tables/annotations, and rollback after a later Refine failure.
- Add ordering coverage: a template whose declared slot order differs from content order, asserting
  the declared order materializes and that the reading-order diagnostic behaves as decided above.
- Assert exact planned and committed trees, template-defined sibling order, deterministic occurrence identities, unchanged leaf MCIDs, unique ParentTree ownership, dry-run/commit parity, parser compatibility, byte-stable regeneration, rendering/glyph invariance, and veraPDF success.
- Assert that a rolled-back failed commit leaves the document byte-identical to its input.
- Assert that every existing descriptive and template-free fixture regenerates byte-identically, so
  the opt-in claim is measured rather than asserted.
- Run the remediation suite, library/CLI/test builds, and full test suite. Accept only the known unrelated baseline failure `DictionaryAccessTests.Legacy_aliases_are_hidden_from_intellisense_without_obsolete_warnings` and require zero new failures.

## Risks

| Risk | Mitigation |
| --- | --- |
| Rollback is larger than the binding work it protects | Scope and land it first, as its own step with its own tests; it is reusable by every later milestone and closes RRM-030 |
| The closed ordered content model cannot express a family's real variability | Fixtures cannot surface this, because we author both the document and the template. Treat the first prescriptive template written against a document nobody on the team authored as a design review, not a rollout |
| Two ordering authorities produce correct output with a spurious diagnostic | Ordering fixture above; decide the `ReadingOrderDrift` scoping before the binding work, not after |
| Prescriptive mode silently becomes single-rule-set | Decide the composition question explicitly; if the restriction stays, document it where authors will hit it |
| Automatic container synthesis makes provenance opaque | Synthesized nodes carry template path and occurrence identity in reports, and are distinguishable from rule-produced nodes in dry-run output |

## Documentation and Completion

- Document the intended workflow: declare the structure template, define anchors/zones/flows, bind leaf slots, compose repeated containers through Group passes, refine by slot, then declare artifacts and assertions.
- Include complete C# and JSON examples showing optional fields, repeated sections, `FromSlot`, nested Group passes, tables, and diagnostics.
- Explain that descriptive mode validates rule-produced structure, while prescriptive mode materializes template-owned structure; omitted mode preserves existing behavior.
- Document the commit failure semantics and the rollback guarantee in the reference doc, and close RRM-030's contract question rather than leaving it implicit in this mode.
- Record this as the prescriptive second phase of structural-template work and mark it complete only after all fixture, compatibility, conformance, and documentation gates pass.
- Keep automatic inference of repeated sibling correlation, arbitrary template expressions, machine-readable JSON Schema publication, and real internal-template onboarding outside this unit. Those follow after the generated template-first authoring surface is stable.

## Work Sequence

Ordered by dependency; each step is independently verifiable.

1. Commit checkpoint and rollback, with `Commit` documented as all-or-nothing. Independent of
   everything below and useful without it.
2. Resolve the inherited-constraint decisions — composition scope, cardinality, `LegalChild`,
   `Pages`/`SpansPages` — since they change the public contract.
3. `RemediationStructuralTemplateMode`, prescriptive declaration validation, `RemediationClaim.SlotId`.
4. `BindTemplateSlotRemediationAction`, `Bind`, `BindOver`, `TemplateSlotContentMode`, JSON parsing.
5. Template assembly step, container synthesis, occurrence identity.
6. Ordering authority and reading-order diagnostic scoping.
7. `ClaimPredicates.FromSlot`, Refine targeting, specialized `TableOver`/`AdoptAnnotation` binding.
8. Binding-ambiguity and ownership-conflict diagnostics; report and CLI surfaces.
9. Fixtures, negative coverage, compatibility assertions.
10. Documentation and tracker status.

### Exit gate

- [ ] A failed commit leaves the document byte-identical to its input.
- [ ] A prescriptive rule set produces a tree whose tags, hierarchy, occurrence, and sibling order
      come from the template, asserted against an exact expected tree.
- [ ] Declared order that differs from content order materializes as declared, with no spurious
      reading-order diagnostic.
- [ ] Every failure in the pre-mutation class blocks commit without touching the document.
- [ ] Repeated composite slots produce deterministic, stable occurrence identities across dry-run
      and commit.
- [ ] Content matching no slot and no inventory item is an error, not an artifact.
- [ ] Every existing descriptive and template-free fixture regenerates byte-identically.
- [ ] Output passes veraPDF for both profiles, with rendering and glyph-box invariance.
