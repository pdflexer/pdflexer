# Rule-Based Remediation — Migration Next Steps

Assessed: 2026-08-02, against `HEAD` (`c0d723d`) plus the uncommitted working tree.

Companion to [rule-based-remediation-architecture.md](rule-based-remediation-architecture.md) and the
earlier [architecture review](rule-based-remediation-architecture-review.md). This document answers a
narrower question: given that the applicable corpus is migrated and most of the legacy engine's
*entry points* are gone, what remains between here and the architecture the doc describes.

## Verified state

Measured, not inferred:

| | |
| --- | --- |
| Build | Clean, 0 errors, 32 warnings (all pre-existing nullability/CS0675, none in program-path files) |
| Tests | 19 passed, 2 skipped, 0 failed (`--filter FullyQualifiedName~Remediation`) |
| Remaining remediation test files | 2 (`RemediationCorpusManifestTests`, `RemediationProgramCompilerTests`) — 18 facts |
| Deleted remediation test files | 14, carrying ~190 fact/theory methods |
| Corpus | 25 cases, 23 Active, 2 Pending (`c-08`, `c-12`), each × UA-1/UA-2 = 40 program files |
| Legacy generated rule sets | **0 remain in the repo** |
| Session entry points | `Use(RemediationProgram)`, `Use(CompiledRemediationProgram)` only |
| CLI | Fully migrated — `pdfctl remediate` loads `pdflexer.remediation.program.preview1` only |

Several defects from the earlier review are fixed and need no further action: the duplicate
`Use(program)` guard (`RemediationSession.cs:69`), the `ApplyLeftoverPolicy` crash on
normalization-empty leftover text (`:2193-2199`), cross-page source-order evidence, order-comparison
recursion into composites (`RemediationSession.ProgramRuntime.cs:1138`), the unfilled comparison
list, and the string-reparsing diagnostic builder.

---

## The one large thing that is left: the legacy engine is dead, not deleted

`_ruleSets` (`RemediationSession.cs:19`) is declared and **read in 19 places but never written to**.
There is no longer any code path that adds to it. Everything downstream is therefore unreachable:

- `Evaluate(IEnumerable<Rule>, bool)` at `RemediationSession.cs:182` has **zero call sites**.
- Its entire callee subtree is reachable only from it — `EvaluateDocument` (:1632),
  `EvaluateClassifyRule` (:2000), `ValidateRules` (:812), `ApplyPlan` (:3585),
  `ValidateRuleSetDeclarations` (:859), `EvaluateAssertions` (:640), the Group/Refine/Table
  evaluators, `PrepareAnnotationAdoptions` (:485), and the rest.

Reachability analysis over the two `RemediationSession` partials, rooted at the public API plus the
program runtime:

```
methods = 164   reachable = 76   unreachable = 88
unreachable method bodies = 144,421 of 258,645 chars (55%)
```

Confirmed independently: deleting `Rule.cs` and `RuleSet.cs` produces compile errors in exactly 9
files, 172 of them in `RemediationSession.cs` — all inside the unreachable set.

Files with **zero references** from the program surface (runtime, compiler, model, serializer, CLI,
remaining tests) are first-order deletion candidates:

| File | Lines | Public types unused by program surface |
| --- | --- | --- |
| `ClaimPredicate.cs` | 416 | 8 of 8 |
| `RemediationArtifactInventory.cs` | 88 | 1 of 1 |
| `DocumentFlowRegionResolver.cs` | 472 | (internal; no program-path reference) |
| `FlowRegion.cs` | 183 | 8 of 11 |
| `RemediationAction.cs` | 580 | 11 of 23 |
| `RemediationPredicate.cs` | 1046 | 8 of 25 |
| `RuleEvaluationSummary.cs` | 233 | 3 of 8 |

Roughly **50 public types** in `PdfLexer.Remediation` have no reference from any program-path file.
They are still shipped API: a caller can construct a `RuleSet`, populate it with `Rule`s and
actions, and then discover there is no method that accepts it. That is worse than either keeping or
removing the engine, because the type system advertises a capability the runtime withdrew.

### Step 1 — Delete the unreachable engine

Order that keeps the build green at each point:

1. Remove `_ruleSets` and `Evaluate(IEnumerable<Rule>, bool)`. Let the compiler enumerate the
   fallout; the unreachable set above is the expected shape of it.
2. Delete legacy-only files outright: `ClaimPredicate.cs`, `RemediationArtifactInventory.cs`,
   `DocumentFlowRegionResolver.cs`, `Rule.cs`, `RuleSet.cs`, `RuleCardinality.cs`,
   `RuleEvaluationSummary.cs`.
3. Split the surviving mixed files. `RemediationAction.cs`, `RemediationPredicate.cs`,
   `RemediationCandidate.cs`, `LayoutCoord.cs`, `FlowRegion.cs`, and `TolerancedZone.cs` each
   contain both live and dead types; keep only what the program path references.
4. Decide `RemediationStructuralTemplateMode.Descriptive`. `RemediationProgramTemplateAdapter.cs:14`
   always constructs `Prescriptive`, so descriptive mode is already unreachable — it is the
   "Descriptive mode still present" architecture gap, now collectible for free. Removing it also
   settles how much of `RemediationStructuralTemplateMatcher` survives, which the architecture doc
   flags as the real cost of extending the grammar.

Exit criterion: `RemediationSession.cs` is roughly half its current 5,594 lines, and no public type
in `PdfLexer.Remediation` is unconstructible-but-unrunnable.

This is a prerequisite for, not a parallel task to, the language work below. Every deferred contract
in the doc — compositional `Region`, richer assertions, the Group question — is currently specified
against a second hierarchy mechanism that no longer executes. Deciding those contracts while the
dead engine is still in the tree means reasoning about two models when only one runs.

---

## Step 2 — Re-close the documentation set

The architecture doc is now stale in the *opposite* direction from the last review. It was written
when legacy execution was still live, and it says so:

- "Legacy execution and its stage/pass-oriented report contract remain available for migration"
  (line ~40) — **false**; there is no way to execute a `RuleSet`.
- "Program sessions are isolated from legacy `Rule`/`RuleSet` execution and reject injected legacy
  rules while a program is active" — vacuous; there is no injection path to reject.
- The whole **Migration** section ("one of forty-two is prescriptive today; six declare a
  descriptive template; the rest declare none", the 3-step flip-the-mode procedure, the
  `TemplateDifferences` bootstrap) describes rule sets that **no longer exist in the repo**.
- Gap row "Migration debt — 41 of 42 generated rule sets predate prescriptive mode" — closed by
  deletion rather than by migration, which is worth recording explicitly so it is not re-opened.

Gap rows that the shipped code now closes, and which should move out of the tables:

| Row | Evidence |
| --- | --- |
| "Slot declaration namespace is flat" | Canonical `SlotRef` paths; `compiled.Slots` keyed by `/bill-to-address/line` |
| "Fragment mount aliases and relative references are not settled" | `RemediationFragmentMount`, `ExpandFragments`, `./` rewriting; exercised by `statement` |
| "Repeating-composite occurrence boundaries are not settled in preview1" (RRM-042) | `OccurrenceBoundary.StartsOnBoundary`, `BuildProgramOccurrencePartitions`; exercised by `c-30` |
| "Declaration ambiguity check is coupled to binding state" | Fixed; slot-keyed identity |

`rule-based-remediation.md` is the more urgent problem. It is the "as shipped" reference, and it
still teaches the legacy API — `new RuleSet(...)` at line 274, `BuildInvoiceRules()` returning a
`RuleSet` at 811-913, `ClaimPredicates.FromRuleSet` at 781, the predicate table at 797. **These
examples do not compile against the current library.** `rule-based-remediation-gaps.md` (60 `RuleSet`
mentions) and `-structural-model.md` (11) have the same problem at lower severity.

Do this after Step 1, in one pass, so the docs describe one engine.

---

## Step 3 — Restore the coverage that the migration dropped

Deleting ~190 test methods alongside the engine they tested was correct where the tests were
legacy-only. Two losses are not legacy-only:

**PDF/UA conformance validation is no longer asserted anywhere.** `RemediationVeraPdfTests.cs` is
deleted; it carried `CorrectedStrictInvoiceTablePassesPdfUa1`,
`EveryCommittedFixtureValidatesForItsProfile`, and a negative control
(`InvalidTdTrHierarchyFailsPdfUa1Baseline`). The corpus test now asserts only
`AccessibilityIntegrityAssert.HasBasicStructureIntegrity` and `HasOnlyTaggedOrArtifactContent` —
structural self-consistency, not conformance. The negative control matters most: without it, a
veraPDF integration that silently stops finding violations still passes.

Port these onto the program corpus under `VeraPdfFactAttribute` (it already skips when veraPDF is
absent, so this costs nothing in environments without it).

---

## Step 4 — Deepen the corpus where features shipped thin

Feature coverage across all 40 program files, counted directly:

| Feature | Programs using it |
| --- | --- |
| Bindings | 20 / 20 |
| Artifacts | 17 / 20 |
| Anchors | 3 / 20 (`c-23` slot anchors ×3, `c-24` + `statement` text-label) |
| Assertions | 1 / 20 (`c-05`, one `SlotCountAssertion`) |
| Occurrence boundaries | 1 / 20 (`c-30`) |
| Fragments | 1 / 20 (`statement`, one fragment) |
| `AllowDeclaredReorder` | 1 / 20 |
| `OccurrenceSelector` ≠ `Only` | **0 / 20** |

Status: **Completed**.

1. **Disjoint Fragment Mounts**:
   - `statement.ua1.json` and `statement.ua2.json` mount `address` fragment twice (`bill-to-address` and `ship-to-address`). Verified `/bill-to-address/line` and `/ship-to-address/line` resolve independently in `RemediationCorpusManifestTests.cs`.
2. **`OccurrenceSelector` Test Coverage**:
   - Added unit test coverage in `RemediationProgramCompilerTests.cs` for `SameOccurrence` and `Nth` occurrence selection on repeating composites.
3. **Multi-Assertion Evaluation**:
   - Added unit test coverage in `RemediationProgramCompilerTests.cs` for multiple `SlotCountAssertion` evaluations (`Exactly`, `AtLeast`, and failing assertion reporting).

Deferred Gaps (`c-08` and `c-12`): `c-08` (`CAP-ANNOTATION-ADOPTION`) and `c-12` (`CAP-CONTINUED-TABLE`) remain explicitly documented deferred contracts for the post-migration phase 2 architecture milestone.

---

## Step 5 — The gates that remain genuinely open

Unchanged from the architecture doc, and correctly stated there:

1. **Two real producer families.** Every corpus input comes from `RemediationFixtureGenerator`; there
   is no real PDF in the fixture tree. This is the binding gate, and Steps 1-4 do not advance it.
   It is also the one that should gate the schema: `preview1` should stay pre-release until real
   producer variation has hit it.
2. **Compositional `Region`** — detailed in [5.2](#52--compositional-region) below.
3. **Richer assertions** — detailed in [5.3](#53--richer-assertion-surface) below.

Gate 4 in the doc — "use producer evidence to decide how much of the legacy Group and region surface
can migrate" — is effectively answered: the Group surface is already unreachable and slated for
deletion in Step 1. The remaining question is not *whether* Group migrates but whether anything in
the program model needs to replace `BindOver`/`TableOver`. That is a question for real producer
evidence, not for the legacy code.

---

### 5.2 — Compositional `Region`

**Implementation status:** Complete in `preview1`; unified declarations, layered resolution, guarded accounting, reporting, and serializer support are implemented.

**Design authority:** `rule-based-remediation-architecture.md` §"One region concept, not four",
§"Region accounting is guarded absorption, not a semantic catch-all", §"`Region` should be
compositional, not a bag of optional fields", §"Keep layout evidence out of the structural contract".
Tracker: RRM-043, RRM-045. Read all four before designing — they constrain each other, and the last
one forbids the obvious shortcut.

#### Current state, verified

The program model has **no named-place concept beyond anchors**. `RemediationProgram.cs` contains no
`Region` or zone type; `grep Region src/PdfLexer/Remediation/RemediationProgram.cs` returns nothing.
The four-declarations gap (`NamedLayoutZone` / `TolerancedZone` / `FlowRegion` / anchors) is
therefore resolved in the program model by *omission*, not by design.

But the **plumbing is already threaded and stubbed with empties**, which is the useful part:

| Site | What it does today |
| --- | --- |
| `RemediationSession.ProgramRuntime.cs:515-516` | Builds `emptyZones` / `emptyFlows` dictionaries |
| `:544-545` | Passes both into `CreateProgramEvaluationContext` for every binding, every page |
| `:86` | Sets `pageState.ArtifactZones` to an empty `Dictionary<string, TolerancedZoneResolution>` |
| `:718` | Builds a `DocumentFlowIndex` with no per-page resolutions |

So the evaluation context already accepts zones and flows; nothing populates them. A `Region`
implementation fills these four sites rather than threading a new parameter through the runtime.

Surviving substrate that should be reused rather than rewritten: `TolerancedZone.cs`
(+`ZoneConfidenceBehavior`), `FlowRegion.cs`, `FlowRegionResolver.cs`, `LayoutCoord.cs`,
`DocumentFlowIndex.cs`, `PageSelector.cs`, `AnchorResolver.cs`. Note that Step 1 marks several of
these as containing dead types — coordinate with that work so the region-relevant types are the ones
kept.

#### Why this gate exists

`PrescriptiveUnaccountedContent` is a **non-suppressible** commit blocker
(`RemediationSession.cs:29`, emitted at `:647`). The only way to clear it today is one
`ArtifactDeclaration` per kind of incidental content. `ArtifactDeclaration`
(`ArtifactDeclaration.cs`) carries `Pages`, `Occurrence`, `SemanticSubtype`, `IncludeBoundingBox`,
and `Attached` — but **no region or zone binding at all**; the legacy
`RemediationArtifactInventoryItem.ZoneId` went out with the inventory. So every new producer sample
yields new blockers and the accounting backlog never closes. That is the concrete pain this gate
relieves.

#### The work

**(a) Model `Region` as a discriminated algebra, not an optional-field bag.** The architecture doc is
explicit that a fixed zone, an anchor-derived area, and a repeated flow instance *resolve
differently* and must not be collapsed into one record with nullable fields. Minimum shape:

- `Region.Fixed` — page-relative bounds (`LayoutCoord`).
- `Region.Anchored` — derived from an anchor point, e.g. `RightOf(anchor, width)`,
  `Below(anchor, height)`. This is what closes the loop with slot-relative anchors: **anchors stay
  points and are not regions, but they may yield region expressions.**
- `Region.Flow` — a region that continues across pages, carrying activation identity.

`Tolerance` is a **composable decoration** over any form (reuse `TolerancedZone.Tolerance` and
`ZoneConfidenceBehavior`), not a property of one form. `Continuation` is available only on the flow
form, where it means something. Named layout zones become presets over these, not separate types.

**(b) Declaration validation must run before page parsing** — in `RemediationProgramCompiler`,
alongside the existing anchor/binding validation, emitting `RemediationProgramDiagnostic`s. Invalid
combinations must be rejected at compile time, not discovered during resolution.

**(c) Resolution must state its contract explicitly.** The doc requires each of: coordinate system,
page-rotation behavior, how tolerance is applied, activation count, and overlap rules. Note the
existing known limitation this interacts with — flow-region instances currently activate **at most
once per page** (the "Flow-region instances are page-granular" gap row); lifting that cap is
described in the architecture doc as the shared substrate for occurrence identity, so sequence it
with the occurrence work rather than duplicating it.

**(d) Region-scoped artifact accounting — guarded absorption.** This is the payoff, and it is the
part most likely to be built too permissively. A catch region must declare **and validate**:

- the painting candidate kinds it may absorb;
- whether text is allowed — **defaulting to `false`**;
- optional text signatures, predicates, or counts bounding the expected furniture;
- precedence and ambiguity behavior where accounting regions overlap;
- the artifact type, subtype, bounds, and attachment metadata to emit.

Hard rules from the doc, none of which are negotiable:

- Annotations, widgets, and other interactive objects are **never** absorbed as painting artifacts.
- An item selected by both a structural binding and by region accounting is a **conflict**, not an
  accounting success.
- Absorption runs **only after** structural bindings and explicit artifacts have established
  ownership.
- Every absorbed item appears **individually** in the report with candidate identity, region,
  declaration, and emitted artifact properties — reuse the existing per-item inventory that
  `ApplyLeftoverPolicy` already produces into `RemediationUnaccountedContent`.
- Content outside an approved catch region remains a **non-suppressible blocker**. Do not weaken
  `PrescriptiveUnaccountedContent`; region absorption changes what reaches it, not its disposition.

#### Exit criteria

- `Region` declarations compile, validate, and reject invalid combinations before any page is parsed.
- The four stub sites above are populated from compiled regions.
- At least one corpus case declares a footer/pagination catch region and commits with **zero**
  suppressions where it previously needed an explicit `Artifact` declaration per furniture kind.
- A negative case: content in the catch region that the declaration does *not* permit (text where
  `allowText: false`, or an unlisted candidate kind) still blocks.
- An overlap case exercising declared precedence.
- veraPDF still passes for every affected case — absorbed content must be emitted as real artifacts,
  not merely dropped from the accounting.

#### Do not

Do not add a generic "absorb everything unbound here" escape. The doc calls this out specifically: an
unrestricted catch region hides a failed semantic binding or a newly added disclaimer, which is the
exact failure mode prescriptive mode exists to prevent. If the guarded form is too tedious to author,
that is a preset problem, not a reason to relax the guard.

---

### 5.3 — Richer assertion surface

**Design authority:** `rule-based-remediation-architecture.md` §"Semantic assertions and mapping
checks remain separate", §"The template declares shape and order; assertions declare everything
else".

#### Current state, verified

`RemediationProgram.Assertions` is typed `IReadOnlyList<SlotCountAssertion>`
(`RemediationProgram.cs:46`, ctor param `:15`). Fragments carry their own assertion list with the
same type (`:170`, parsed at `SerializedRemediationProgram.cs:173`). `SlotCountAssertion`
(`RemediationProgram.cs:414-432`) is:

```
Id : string
Slot : SlotRef
Expected : AssertionCount      // (Min, Max?) — RemediationAssertions.cs
Scope : SemanticAssertionScope // Document | PerPage
Pages : PageSelector
```

Three facts that shape the work:

1. **`SlotCountAssertion` is a standalone sealed record — it does not derive from
   `RemediationSemanticAssertion`.** That abstract base still exists in `RemediationAssertions.cs`
   along with `SlotElementCountAssertion`, and both are **unreferenced orphans** (see Step 1's
   second-tier orphan list). The first decision is whether the widened surface adopts that
   hierarchy or deletes it and builds a fresh discriminated union. Do not leave both.
2. **The JSON schema has no discriminator.** `ParseAssertion`
   (`SerializedRemediationProgram.cs:268`) does
   `RejectUnknown(json, "id", "slot", "count", "minCount", "maxCount", "scope", "pages")` and
   returns a `SlotCountAssertion` unconditionally. Adding assertion kinds means adding a `kind`
   field, which is a **breaking schema change** — hence this gate belongs before broad migration.
   `preview1` is pre-release, so take the break now rather than versioning around it later.
3. **Evaluation ordering is currently wrong for per-occurrence assertions.**
   `EvaluateProgramAssertions` is called at `RemediationSession.ProgramRuntime.cs:95`;
   occurrence partitions are built at `:96` and stamped onto claims at `:99-105`. Per-occurrence
   assertions need partitions, so evaluation must move after that block. This is a small change but
   easy to miss, and it will silently produce empty per-occurrence results if overlooked.

#### The three assertion kinds to add

Named in the architecture doc's sequencing gate 3:

- **Conditional** — "if slot X is bound then Y is required." The doc is explicit that conditional
  *structure* belongs in assertions, **not** in the template grammar; do not add a `choice`/`when`
  particle to satisfy this.
- **Subtree** — counts or constraints over a slot's descendants rather than one slot path.
- **Per-occurrence** — evaluated once per occurrence of a repeating composite. The doc's default
  is important: conditional and child-count assertions over repeating structures are evaluated
  **per occurrence unless they explicitly request document-wide aggregation.** The current
  `SemanticAssertionScope { Document, PerPage }` cannot express this — `PerPage` is a *page*
  partition, not an *occurrence* partition. Expect to extend that enum or replace it with an
  explicit scope expression.

Worked example from the doc, useful as an acceptance test: *"every `item` occurrence containing
`discount` also contains `discount-total`"* — that is conditional + per-occurrence together, and it
is not expressible today.

#### Keep separate from mapping checks

`BindingCardinality` (`BindingCardinality.cs`) is the *mapping* check — "this matcher was expected to
find one input" — and the doc requires it stay distinct from semantic assertions. The reasoning:
replacing a cardinality check with a slot count lets a broken or overly broad matcher be hidden by
another producer filling the same slot. Do not merge these two surfaces while widening assertions.
Note `BindingCardinalityScope` and `SemanticAssertionScope` are separate enums with the same members
today — that duplication is deliberate, not an oversight to clean up.

#### Reporting

`RemediationAssertionOutcome` (`RemediationAssertions.cs`) already carries `ProgramId`,
`AssertionId`, `PageIndex`, `Expected`, `Observed`, `Passed`, `BindingId`, and `ProgramSlot`. It will
need an occurrence identity field for per-occurrence results — reuse the existing
`TemplateOccurrenceIdentity` / `RemediationOccurrencePartition.OccurrenceIdentity` rather than
inventing a second occurrence key. Its `RuleId` and `Tag` fields are legacy residue and should go
with Step 1.

`SemanticAssertionFailed` is already in `ProgramWorkItemCodes`
(`RemediationSession.ProgramRuntime.cs:14-22`), so new kinds inherit the correct
Authoring-work-item / Enforced-error disposition split without further wiring.

#### Exit criteria

- A `kind` discriminator exists in the assertion JSON, round-trips byte-stably (the corpus test
  already asserts `Save → Load → Save` equality), and rejects unknown kinds at compile time.
- Each of conditional, subtree, and per-occurrence has at least one corpus case, including the
  `discount` / `discount-total` example above.
- At least one **failing** case per kind, asserted via `Expected.Diagnostics` in `corpus.json`, so
  the assertions are shown to fire and not merely to parse.
- Per-occurrence assertions produce one outcome per occurrence with a distinct occurrence identity.
- The orphaned `RemediationSemanticAssertion` / `SlotElementCountAssertion` pair is either adopted
  or deleted — not left dangling alongside a parallel hierarchy.

#### Sequencing note

This gate has a hard dependency on Step 4's assertion corpus work. Today **one** program (`c-05`)
declares **one** assertion. Widening a surface that has essentially no coverage means the widening
itself is unverifiable. Fill the existing surface's coverage first, then extend it.

---

## Suggested order

1. Delete the unreachable engine (Step 1). Largest, most mechanical, unblocks the rest.
2. Re-close the docs against the post-deletion tree (Step 2). Cheap once Step 1 fixes what is true.
3. Restore veraPDF conformance + CLI tests (Step 3). Independent of 1 and 2; can run in parallel.
4. Fill the fragment-double-mount, occurrence-selector, and assertion corpus gaps (Step 4).
5. Only then take the real-producer, `Region`, and assertion-surface gates (Step 5).

Steps 1-4 are all inside the current design. Step 5 is where the design is still being decided, and
the doc's instruction to settle it on a narrow real vertical slice rather than by breadth still
holds.
