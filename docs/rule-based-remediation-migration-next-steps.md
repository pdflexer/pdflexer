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

**The CLI has no test.** `RemediateCmdTests.cs` was deleted and `RemediateCmd.cs` was rewritten
(+44/−121) in the same change. `pdfctl remediate` is the only shipped consumer of the program
format and is currently unexercised.

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

This is the doc's own gate 1 — "harden the implemented fragment, occurrence, and boundary contracts
with representative corpus runs" — and it is partially met, not met. Three specific holes:

1. **The `statement` fragment is mounted once** (`address` → alias `bill-to-address`). A
   single mount does not exercise the property fragments exist for: that two mounts of the same
   fragment produce disjoint slot paths and that `./`-relative bindings rewrite to the right alias.
   Mount `address` a second time as `ship-to-address` and assert both `/bill-to-address/line` and
   `/ship-to-address/line` resolve independently. This is the cheapest high-value addition here.
2. **`OccurrenceSelector` is entirely unexercised.** `SameOccurrence`, `Nth`, `NearestPrevious`, and
   `All` have compiler and runtime support and zero corpus evidence. `SameOccurrence` in particular
   is what makes an anchor usable inside a repeating composite — pair it with `c-30`.
3. **Assertions are one program deep.** A single `SlotCountAssertion` is not enough to claim the
   assertion surface works before extending it (Step 5).

Also worth a decision: `c-08` (`CAP-ANNOTATION-ADOPTION`) and `c-12` (`CAP-CONTINUED-TABLE`) are
Pending with skipped tests pointing at "a separate architecture milestone." Neither milestone is
named in the architecture doc's sequencing. Either add them as explicit deferred contracts or
schedule them; a skip that cites an unwritten milestone decays into a permanent skip.

---

## Step 5 — The gates that remain genuinely open

Unchanged from the architecture doc, and correctly stated there:

1. **Two real producer families.** Every corpus input comes from `RemediationFixtureGenerator`; there
   is no real PDF in the fixture tree. This is the binding gate, and Steps 1-4 do not advance it.
   It is also the one that should gate the schema: `preview1` should stay pre-release until real
   producer variation has hit it.
2. **Compositional `Region`.** No `Region` or zone type exists in `RemediationProgram.cs` at all —
   the program model currently has no named-place concept beyond anchors. The four-declarations gap
   (`NamedLayoutZone`/`TolerancedZone`/`FlowRegion`/anchors) resolves in the new model by *omission*
   rather than by design, which is fine as a preview position but needs stating deliberately.
3. **Richer assertions.** `RemediationProgram.Assertions` is typed as
   `IReadOnlyList<SlotCountAssertion>` — the surface cannot express conditional, subtree, or
   per-occurrence assertions without a model change. Widening that property is a breaking change to
   the program model, so it belongs before broad migration, not after.

Gate 4 in the doc — "use producer evidence to decide how much of the legacy Group and region surface
can migrate" — is effectively answered: the Group surface is already unreachable and slated for
deletion in Step 1. The remaining question is not *whether* Group migrates but whether anything in
the program model needs to replace `BindOver`/`TableOver`. That is a question for real producer
evidence, not for the legacy code.

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
