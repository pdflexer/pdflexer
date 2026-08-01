# Review — `rule-based-remediation-architecture.md` and the preview-runtime milestone

Review date: 2026-08-01 · Branch: `rule-based` · Reviewing the working tree against
[rule-based-remediation-architecture.md](rule-based-remediation-architecture.md) as updated on
2026-08-01.

## Scope and method

The architecture doc's 2026-08-01 revision adds two new sections (*Current migration boundary*,
*Preview runtime milestone*), rewrites *The guardrail*, *Reading-order disagreement*, *What
assertions remain to settle*, and *Required sequencing*, and deletes five rows from the gap tables.
Every one of those edits asserts something about shipped behaviour, so this review checks the doc
against the code that backs it:

- new: `RemediationProgram.cs`, `RemediationProgramCompiler.cs`, `RemediationProgramTemplateAdapter.cs`,
  `RemediationRuntimeDiagnostic.cs`, `RemediationSession.ProgramRuntime.cs`,
  `SerializedRemediationProgram.cs`, `TemplateOccurrenceIdentity.cs`, `ArtifactDeclaration.cs`;
- modified: `RemediationSession.cs`, `PrescriptiveTemplateAssembly.cs`, `RemediationSemanticTree.cs`,
  `RemediationStructuralTemplate{,Matcher,Validator}.cs`, `AnchorResolver.cs`, `PredicateResult.cs`,
  `RemediationAssertions.cs`, `RemediationReport.cs`, `RemediationSessionConfiguration.cs`.

Baseline: `dotnet build src/PdfLexer` succeeds (0 errors, 14 pre-existing warnings, 2 of them in the
new code). `dotnet test --filter FullyQualifiedName~Remediation` → **248/248 pass**, including the 10
new `RemediationProgramCompilerTests`.

Claims that could not be settled by reading were checked with throwaway probe tests, run and then
deleted. Each finding below marked **verified** was reproduced that way; the observed output is
quoted.

## Verdict

The milestone is real. The compile boundary, typed `SlotRef`, dependency layering, slot-keyed
assertions, slot anchors, run modes, declared node properties, and per-item prescriptive leftover
inventory all exist and behave as described in the narrow cases the tests cover. Repeating composites,
fragments, and relative references are rejected at compile time rather than half-implemented, which is
the right call.

Three things stand between that and the doc as written.

1. **Two of the newly-claimed capabilities do not hold outside single-page synthetic fixtures.** The
   reading-order guardrail produces a false, non-suppressible commit blocker on ordinary multi-page
   documents (F2), and the prescriptive leftover inventory throws an unhandled exception on
   whitespace-only leftover text (F1). Both are on the default path.
2. **The guardrail is narrower than described.** It does not recurse into composite containers
   (F3), so "recursively … through materialized composites" overstates what shipped.
3. **The doc updated its tables but not its prose, and did not update its siblings.** Three
   already-fixed defects are still described as open in body text (F12, F13, F14), and the gap
   register still carries RRM-044/045/046 as P0/Open (F17).

The doc's own *Process* table says: *"Gates have been marked complete ahead of evidence. Each
premature Complete removes a reason to look again."* F1–F3 are precisely what a real corpus run would
have surfaced, and the *Required sequencing* section already names that corpus run as an unmet gate.
The milestone prose currently reads more settled than the gate list underneath it.

---

## Defects

### F1 — Prescriptive leftover inventory throws on candidate-less leftover text · High · verified

`RemediationSession.cs:2433-2437` resolves each unowned text span to a candidate with
`GetCandidates(Paragraph).FirstOrDefault(…) ?? GetCandidates(Line).First(…)`. The trailing `First`
throws when no line candidate covers the span — a whitespace-only text run is enough.

Before this change the prescriptive branch returned early with one per-page diagnostic and never
entered this loop (`RemediationSession.cs:2421-2429`, removed). The fix that the doc advertises —
*"Preview inventories each prescriptive text and graphical leftover"* — is what made the latent
`First` reachable, for **every** prescriptive template including the preview program path.

Repro: one page, `H1#title` bound to `"Invoice"`, plus a `"   "` text run.

```
PROBE_A_EXCEPTION=InvalidOperationException: Sequence contains no matching element
  at RemediationSession.ApplyLeftoverPolicy (RemediationSession.cs:2433)
  at RemediationSession.EvaluateProgram (RemediationSession.ProgramRuntime.cs:85)
  at RemediationSession.DryRun (RemediationSession.cs:157)
```

It escapes `DryRun()` and `Commit()` alike — `EvaluateProgram` calls `ApplyLeftoverPolicy`
unconditionally. Suggested fix: fall back to `FirstOrDefault`, and emit an unaccounted-content record
with the raw span when no candidate covers it. Losing the candidate identity for one span is the
correct degradation; throwing out of the strictest mode is not.

### F2 — Cross-page source-order evidence is invalid, and blocks commits · High · verified

`EvaluateProgramSourceOrder`'s `SourceOrder` (`ProgramRuntime.cs:844-847`) is
`node.SourceReferences.Min(x => x.OperatorStart)`. `StructuredSourceRef` is
`(StreamId, OperatorStart, OperatorLength)` — the offset is **stream-local**. Comparing two nodes on
different pages compares offsets into different content streams.

The geometric evidence in the same method gets this right: `GeometryOrder` sorts by page index first
(`ProgramRuntime.cs:856`). The source path has no page or `StreamId` term at all, and the inversion
test ORs the two evidences (`ProgramRuntime.cs:778`), so bad source evidence alone blocks.

Repro: two pages, declared order `title` (page 1) then `body` (page 2) — which *is* the true reading
order. Page 1 carries other content before the title, so the title sits deeper in its stream than the
body sits in page 2's stream.

```
PROBE_B_ORDER_COMPARISONS=1
PROBE_B_CMP srcIdx=433/37 srcInv=True  geoIdx=0/1 geoInv=False  pages=[0,1] disp=Error
PROBE_B_DIAG TemplateWrongOrder: Program template order is inverted in '/':
             source or geometric evidence disagrees with the declared order of 'title' and 'body'.
```

Geometry says "not inverted". Source says "inverted" for a purely structural reason. Result: a
blocking `TemplateWrongOrder`, and `TemplateWrongOrder` was added to
`NonSuppressibleDiagnosticCodes` (`RemediationSession.cs:31`), so there is no suppression escape. The
only way out is `AllowDeclaredReorder` on the container — which turns the guardrail off for that
container entirely.

This makes the default `RequireSourceAgreement` policy unusable on any multi-page family where a
page-2+ slot appears earlier in its own stream than a page-1 slot does in its. That is the common
case, not the corner case.

Suggested fix: key source evidence on `(pageIndex, OperatorStart)` — or on a document-wide stream
sequence — and consider whether a single disagreeing evidence source should block, or whether
blocking should require source and geometry to agree that the order is wrong.

### F3 — Order comparison never recurses into composite containers · Medium · verified

Both the *Preview runtime milestone* and *Reading-order disagreement* sections claim the comparison
runs *"recursively through materialized composites"*. It does not.

`EvaluateProgramSourceOrder`'s recursion (`ProgramRuntime.cs:819-832`) descends only when the source
tree contains a node whose `SlotId` equals the child's path. In the program runtime that can never
happen for a composite: the compiler rejects bindings that target composite slots
(`RemediationProgramCompiler.cs:108-111`), so a composite has no claim; and program claims carry no
`RelatedClaims`, so `RemediationSemanticTree.FromProgramClaims` produces a completely flat tree. The
recursive branch fires only for leaves, where there is nothing left to compare.

Repro: `Document > Sect#block > [H1#first, P#second]`, painted `Second` then `First` on one page.

```
PROBE_C_ORDER_COMPARISONS=0
PROBE_C_ASSEMBLY=template:probeC@1:Document/block[1] |
                 template:probeC@1:Document/block[1]/first[1] |
                 template:probeC@1:Document/block[1]/second[1]
(no TemplateWrongOrder diagnostic)
```

The container assembles correctly; the inversion inside it is invisible. What *does* work is the
root-level position of the composite itself, via the earliest-descendant proxy at
`ProgramRuntime.cs:736-742` — a good mechanism, and worth keeping. The gap is sibling order *within*
a container.

Also note the comparison runs on the pre-assembly source tree, not on materialized containers, so
"materialized composite containers" is the wrong noun even once recursion works.

### F4 — A second `Use(program)` silently replaces the first · Medium · verified

`Use(CompiledRemediationProgram)` (`RemediationSession.cs:78-90`) guards `_ruleSets.Count > 0` but
never checks `_program != null`, so `session.Use(a).Use(b)` drops `a` with no error — contrary to the
method's own XML doc, *"only one program may be selected."* Probe: `SECOND_USE_EXCEPTION=<none>`.
The symmetric legacy guard (`Use(params RuleSet[])`, line 56-57) is present and correct. One-line fix.

### F5 — `EvaluateProgramSourceOrder` returns a list it never fills · Medium

`differences` is allocated at `ProgramRuntime.cs:718` and returned at `:836` without a single `Add`.
`templateDifferences.AddRange(EvaluateProgramSourceOrder(…))` at `:120` is therefore a no-op, and
order inversions never reach `report.TemplateDifferences` — they exist only in `OrderComparisons` and
as a diagnostic string. The legacy path *does* emit `RemediationTemplateDifference` for the same
condition (`RemediationSession.cs:599-643`), so the two paths report the same event through different
channels. Decide which, and delete the dead list either way.

### F6 — Structured runtime diagnostics are reconstructed by parsing strings · Medium

`BuildProgramRuntimeDiagnostics` (`ProgramRuntime.cs:1056-1087`) takes the formatted
`"{code}: {message}"` strings, splits on the first `:`, and `Enum.TryParse`s the prefix back into a
`DiagnosticCode`. Consequences:

- Every `RemediationRuntimeDiagnostic` the program path emits has `ProgramSlot`, `BindingId`,
  `CandidateIds`, `Evidence`, and `Suppression` all `null`, and `Scope` hard-coded to `"Program"` —
  the scope string every `ReportDiagnostic` call site carefully builds
  (`"Program:{id}:Assertion:{id}:Page{n}"`, …) is discarded. The record has the fields; nothing
  populates them.
- `ReportDiagnostic`'s strict-mode branch (`RemediationSession.cs:5406-5411`) emits **two** lines:
  `[IGNORED-SUPPRESSION] Code: msg` and `Code: msg`. The first fails the code parse (its prefix
  contains a space, not a code), so it surfaces as an extra `DiagnosticCode.Unknown` **blocking**
  diagnostic alongside the real one.

This is the one place where the doc's *"Typed binding and slot references remain attached to claims,
assembly records, assertions, and reports"* does not hold. It holds for claims, assembly items,
assertion outcomes, binding summaries, and order comparisons — all verified. It does not hold for the
diagnostic stream, which is the part an author actually reads. Emitting structured diagnostics at the
source and projecting the strings *from* them would invert this correctly and match the doc's
*"Existing string diagnostics … are compatibility views."*

### F7 — `RuntimeDiagnostics` is empty on every legacy report · Low

`RemediationReport.RuntimeDiagnostics` is documented as *"the authoritative program-facing view"*
(`RemediationReport.cs:78`), but every legacy construction passes `runtimeDiagnostics: null`, so it is
empty while `Diagnostics` is populated. A consumer written against the "authoritative" view sees
nothing for legacy rule sets. Relatedly, the `Diagnostics = diagnostics ?? RuntimeDiagnostics.Select(…)`
fallback at `:40` is dead — callers always pass a list.

### F8 — Slot anchors cannot resolve across pages · Low

`AnchorResolver.ResolveSlot` filters to `x.PageIndexes.Contains(_context.PageIndex)`
(`AnchorResolver.cs:156-162`), so a binding evaluating page 2 can never see a slot anchor whose claim
landed on page 1 — it silently resolves to "unresolved", with no diagnostic distinguishing that from
"the slot was never bound". The base `RemediationAnchor.Pages` selector *is* honoured separately
(`AnchorResolver.cs:62-70`), so the doc's *"the anchor's page selector preserved"* is accurate; the
same-page restriction is an additional, undocumented constraint. Worth stating in the doc even if the
restriction is intentional for preview1.

### F9 — Legacy `EvaluateSourceOrder` can throw on composed rule sets · Low

`RemediationSession.cs:608` resolves the owning rule set with
`_ruleSets.Single(x => ReferenceEquals(x.StructuralTemplate, template))`. Two composed rule sets
sharing one template instance, or a template reaching the method by any other route, throws
`InvalidOperationException` from inside evaluation. `FirstOrDefault` with a guard is safer.

### F10 — Smaller items

- `GeometryOrder(observed, claims)` is recomputed inside the O(n²) pair loop
  (`ProgramRuntime.cs:766`) — a full sort per pair rather than once per container.
- `ProgramBindingRun.AppliedClaims` is incremented (`:403`) and never read;
  `BuildProgramBindingSummaries` recounts from the claim list instead.
- `EvaluateProgram`'s opening "compiler reported errors" branch (`:27-51`) is unreachable — `Use`
  rejects invalid programs and `PrescriptiveTemplateAssemblyPlan.Build(compiled, …)` throws on them.
- `RemediationAssertionOutcome` exposes `Slot` and `ProgramSlot` as two public names for one backing
  field (`RemediationAssertions.cs:71-79`); `ProgramSlot` is excluded from record equality, and
  `with { Slot = …, ProgramSlot = … }` is order-sensitive. Pick one.
- `RemediationProgramCompiler.cs:130` rejects anchors via the literal `NeighborTolerance != 24`,
  duplicating the default's magic number rather than comparing against the declared default.
- The prescriptive leftover branch normalizes with `TextNormalizationOptions.Default`
  (`RemediationSession.cs:2456`) rather than the program's configured normalization.

---

## Doc claims that overstate or contradict the code

### F11 — "recursively … through materialized composites"

Both *Preview runtime milestone* and *Reading-order disagreement* claim recursive comparison through
composites; see F3. Until recursion works, the accurate statement is: *"compares declared order among
the Document root's direct children, positioning a composite by its earliest-ordered bound
descendant."*

### F12 — The declaration-ambiguity defect is fixed; the doc still presents it as open · verified

The doc carries a full subsection, *"The declaration ambiguity defect"*, ending *"It is a one-line
condition and should land before any POC authoring starts"*, plus a matching Language gap row
(*"Declaration ambiguity check is coupled to binding state"*). The one-line condition landed:
`ParticleKey` now keys on the slot id whenever the template is prescriptive
(`RemediationStructuralTemplateValidator.cs:220-226`, `:185-188`).

Probe using the doc's own example — prescriptive `H1#title`, `P#body` (Optional), `P#notes`
(Optional), zero rules:

```
ERRORS=0
```

The doc says this case *"fails declaration validation … when zero rules exist"*. It no longer does.
Delete the gap row and rewrite the subsection as a closed decision.

Note the same commit also loosened a second check: `bound > 1` on a singular slot is now skipped in
prescriptive mode (`RemediationStructuralTemplateValidator.cs:134`). That is a deliberate-looking
semantic change — several bindings feeding one slot is a shape the doc's *"Why mapping predicates do
not go on the tree"* section explicitly expects — but it is not mentioned anywhere in the doc, and it
moves a declaration-time error to run time.

### F13 — The `ApplyLeftoverPolicy` hole is fixed; the prose still describes it

*"4. Accounting is declared by region"* still closes with *"One related hole belongs with it: the
prescriptive branch of `ApplyLeftoverPolicy` reports a single per-page diagnostic and returns before
populating `unaccountedContent`, so the strictest mode gives the least information about what actually
failed."* Fixed at `RemediationSession.cs:2424-2487`; the gap-table row was updated to
*"Region-scoped accounting is not compositional"*, but this paragraph was not. (See F1 for what the
fix introduced.)

### F14 — Content-independent attributes have partly landed; still listed as a gap

The Language gap table carries *"Content-independent attributes require a rule … RRM-044"*, and
*"3. Content-independent attributes are declared, not refined"* reads as unimplemented direction. But
`RemediationNodeProperties(Language, AlternateText, ActualText, Expansion)` is declared on both
`RemediationTemplateNode` and `RemediationStructuralTemplateNode`, validated by the compiler
(`RemediationProgramCompiler.cs:208-220`), and written during assembly
(`RemediationSession.cs:4001-4005`, `:4082-4085`). `RemediationProgramCompilerTests` asserts
`title.Language == "en-US"` and `title.Alt == "Invoice heading"` after commit.

The four PDF/UA text-equivalent attributes have landed. `/Scope`, `/ListNumbering`, and `/Placement`
have not. The row should say so rather than reading as untouched.

### F15 — "Direction: identity and reference" §2 and §3 were left behind by their own gap rows

Two Architecture gap rows were deleted in this revision — *"Durable references key on rule id"* and
*"Anchors cannot reference slots"* — but the prose they summarized was not touched. §2 still asserts
*"every durable reference in the language currently keys on rule id"* over a four-row table, and §3
still says *"`PriorClaimAnchor` already proves the mechanism; it points at a rule id rather than a
slot."* `SlotAnchor` (`RemediationProgram.cs:231-242`), `SlotCountAssertion` (`:244-263`), and
`SlotElementCountAssertion` (`RemediationAssertions.cs:42-48`) now exist. The tables and the prose
they belong to now say different things.

### F16 — "The legacy staged path remains isolated … retains its migration behavior" understates a
behavioural change to legacy rule sets

Two changes reach legacy execution:

- `TemplateWrongOrder` was added to the global `NonSuppressibleDiagnosticCodes`
  (`RemediationSession.cs:31`);
- a new `EvaluateSourceOrder` root-order check runs for **legacy** prescriptive rule sets
  (`RemediationSession.cs:441`, `:599-643`).

The evidence is in the diff: `RemediationStructuralTemplateTests.cs:473-505` had to add
`orderPolicy: AllowDeclaredReorder` and flip `Assert.Empty(TemplateDifferences)` to
`Assert.Single(… Suppressed)` to keep passing. Any existing prescriptive rule set that reorders now
blocks, non-suppressibly. That may well be intended — it is the guardrail the doc argued for — but
"isolated" and "retains its migration behavior" is not what happened, and migration planning depends
on knowing it.

### F17 — Smaller doc-vs-code notes

- *"Existing string diagnostics and legacy rule evaluations are compatibility views"* — program
  reports pass `ruleEvaluations: Array.Empty<RuleEvaluationSummary>()` (`ProgramRuntime.cs:1038`).
  That is an empty collection, not a compatibility projection. And per F6 the string/structured
  relationship runs the other way.
- *"Bindings within one layer are independent and resolve deterministically by binding id."*
  Deterministic, yes. Independent, no: within a layer the first binding by id claims contested
  candidates and later ones are skipped with a conflict diagnostic
  (`ProgramRuntime.cs:277`, `:363-392`). Worth saying "resolved in binding-id order, with conflicts
  reported" — the ordering *is* the conflict-resolution rule.
- Doc-links table, row 4: *"Design spec for the template and artifact inventory (RRM-039 – RRM-041)"*.
  RRM-041 is *"Recurring predicate logic cannot be named or reused"*; the structural-model doc covers
  RRM-039 – RRM-040.

---

## Cross-document consistency

### F18 — The gap register was not updated · confirmed

The architecture doc names
[rule-based-remediation-gaps.md](rule-based-remediation-gaps.md) as *"one section per RRM ID"* — the
per-ID source of truth. It still carries:

| ID | Register status | Architecture doc now says |
| --- | --- | --- |
| RRM-044 | P1 · **Open** — "Content-independent structure attributes require rules" | four attributes landed (F14) |
| RRM-045 | P0 · **Open** — "…and its diagnostic identifies nothing" | per-item inventory landed (F13) |
| RRM-046 | P0 · **Open** — "Declared reading order is never checked against document order" | guardrail landed; row deleted from the gap table |

Two documents that disagree about which P0s are closed is exactly the failure the *Process* table
warns about from the other direction. Given F1–F3, the honest resolution is probably to move RRM-045
and RRM-046 to *Partial* rather than *Complete*, and to record the multi-page and composite cases as
the remaining work.

### F19 — Stale source citations in the doc under review

| Citation | Status |
| --- | --- |
| `RemediationSession.cs:2280` — "leftover painting content is a non-suppressible commit blocker" | **Stale.** 2280 is now the nearest-anchor ambiguity diagnostic; the leftover blocker is ~`:2479`. |
| `RemediationStructuralTemplateTests.cs:504` — the `Sect#item` / `P#line` BindOver example | **Stale.** That test now starts at `:509`; the two rules quoted are `:527-530`. |
| `FlowRegion.cs:65-74` — `FlowBoundary` factories | Accurate. |
| `DocumentFlowRegionResolver.cs:90`, `:138` — one activation per page | Accurate. |
| `RemediationSession.cs:5063` — `CheckReadingOrder` early return | Correctly removed. |

All sibling-document links and all in-document anchors resolve.

---

## Test coverage gaps

The 10 new compiler tests are well chosen for what they cover. What they do not cover maps closely
onto F1–F4, which is why those survived a green suite:

| Untested | Finding it would have caught |
| --- | --- |
| Any multi-page program document | F2 |
| Sibling order inside a composite container | F3 |
| Unaccounted content in a program run (the `ProgramWorkItemCodes` path end-to-end) | F1 |
| A second `Use(program)` | F4 |
| `AllowDeclaredReorder` on the program path (only the legacy path has a test) | — |
| Slot-anchor **resolution** — ambiguity, zero-match, cross-page (only compiler layering is tested) | F8 |
| `BindingTarget.ToArtifact` / `ArtifactDeclaration` end-to-end — no test constructs either | whole artifact path |
| Compiler cycle, self-dependency, and relative-reference rejection | — |

The artifact one is worth calling out separately: `ArtifactDeclaration`, `ProgramAction`'s artifact
branch, and `EvaluateProgramArtifactInventory` have no test at all, yet the doc's core invariant —
*"Content matching no slot and no declared artifact is a non-suppressible commit blocker"* — depends
on the artifact half working.

---

## Suggested sequencing

1. **F1 and F2 before anything else.** Both are on the default path, both are reachable from a
   two-page real document, and F2 in particular will make the first real-producer corpus run look
   like the engine is broken.
2. **F3, then re-word the guardrail sections** — or re-word them first to describe root-level
   comparison honestly, and schedule recursion with repeating-composite partitioning (RRM-042), since
   that is where per-occurrence comparison has to be defined anyway.
3. **F12–F16: one editing pass over the prose.** The tables were updated carefully; the body text
   they summarize was not. Three sections currently describe fixed defects as open, which is the most
   expensive kind of stale doc — it costs someone a re-investigation.
4. **F18: update the gap register in the same pass**, to *Partial* rather than *Complete*.
5. **F4–F10** are small and independent.
6. Add the multi-page and composite-container order tests as regression cover before the real-producer
   corpus gate, so that gate tests the model rather than these three defects.

None of this touches the architectural decisions. Prescriptive-as-the-model, the closed-template
invariant, slot-path identity, and compile-before-parse all hold up under the code as written, and
the compiler's habit of rejecting unsupported shapes at declaration time rather than degrading at run
time is the right instinct — it is what kept the preview surface honest about fragments and repeating
composites, and it is the reason F1–F3 are bugs in new checks rather than gaps in the model.
