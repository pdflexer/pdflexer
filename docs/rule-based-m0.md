# Rule-Based Remediation — M0 Execution Plan

Last updated: 2026-07-26

Execution detail for **M0 — Foundations & scope lock** from the
[Rule-Based Remediation Delivery Plan](rule-based-remediation-plan.md).

| Document | Role |
| --- | --- |
| [rule-based-remediation-plan.md](rule-based-remediation-plan.md) | Milestones, gates, critical path |
| [rule-based-remediation-gaps.md](rule-based-remediation-gaps.md) | Gap register and completion criteria — **status lives here, not in this doc** |
| **This document** | How M0's four open items get done, and what each one produces |

M0's goal, restated: *stop producing corrupt output, and make the decisions that constrain
everything after.* The first half is finished. This plan covers the second half.

---

## Where M0 stands

| Work | Gaps | Status |
| --- | --- | --- |
| Fix text-string encoding on write | gaps-2 #1 | ✅ Done |
| Fix per-page MCID allocation | gaps-2 #2 | ✅ Done |
| Enumerate supported document families in writing | prereq for RRM-011, RRM-017 | Open |
| Collect representative real inputs per family | prereq for RRM-011 | Open |
| **Decide and implement the page-locality model** | RRM-001 | ✅ Done |
| Confirm RRM-014 / RRM-015 remain out of scope | RRM-014, RRM-015 | Open |

Both prerequisite defects were fixed and regression-tested; the fixes and their follow-up review are
recorded in [accessibility_gaps_2.md](accessibility_gaps_2.md) and
[accessibility_gaps_2_review.md](accessibility_gaps_2_review.md). Nothing in this plan revisits them.

### The shape of the remaining work

Three of the four open items are decisions and evidence collection, not code. That is a problem to
plan around rather than ignore: "look at some real PDFs and decide" is not a task that can be
verified, repeated, or reviewed, and M0's exit gate explicitly demands the *evidence behind* the
page-locality decision — not just the decision.

So M0 produces exactly one piece of code: **an instrument that turns a directory of real PDFs into a
recorded, reviewable set of measurements.** Everything else in this plan is a document that cites
that instrument's output.

There is a second reason to build it. The corpus is the input to M5, the MVP exit milestone, and to
RRM-017's document fingerprint in M7. Measuring the corpus once, reproducibly, is work that gets
spent three times.

---

## Deliverable 1 — `pdfctl survey`, the corpus probe

**New:** `src/PdfLexer.pdfctl/Inspect/SurveyCmd.cs`, registered in `Program.cs` alongside the
existing `inspect` / `read` / `search` / `remediate` commands.

`pdfctl` is the right home: it already hosts `remediate` (`RemediateCmd.cs`), which is the command
the corpus will eventually be run through, and it already shells out to veraPDF, which M1 needs.

```
pdfctl survey --dir <corpus-root> [--out report.json] [--redact] [--repeat-threshold 0.8]
```

| Option | Meaning |
| --- | --- |
| `--dir` | Directory scanned recursively for `*.pdf`. Required. |
| `--out` | Write the JSON report. Without it, only the human-readable summary goes to stdout. |
| `--redact` | Replace every extracted string with `sha256[0..8]` + length + character-class profile. Required for any report that will be committed. |
| `--repeat-threshold` | Fraction of pages a line must appear on to count as a repeat band. Default `0.8`. |

Exit code `0` on a clean scan, `1` if any file failed to open or parse — a parse failure is itself a
finding, not a crash.

### What it measures

Grouped by the question each measurement answers. Everything here is reachable from the existing
public API — `PdfDocument.Open`, `page.GetStructuredText()`, `page.GetContentNodes<double>()` — the
same calls `RemediationSession.BuildPageStates` already makes.

**Is this document in scope at all?**

| Measurement | Answers |
| --- | --- |
| `Catalog.ContainsKey(PdfName.StructTreeRoot)` | RRM-014. This is the exact condition `ThrowIfAccessibilityAuthoringIsUnsupported` (`PdfDocument.Accessibility.cs:845`) rejects, so a `true` here means `BeginRemediation` throws on this file. |
| `/MarkInfo /Marked`, pre-existing `BDC`/`BMC` operators | RRM-020. Partially marked but untagged input is a distinct hazard from fully untagged. |
| Encrypted / permissions | Whether the pipeline can open it at all. |
| Per page: character count vs. image count vs. path-op count | RRM-015. A page with images and no extractable characters is scanned. |
| PDF version, producer and creator from `/Info` and XMP | Family fingerprinting later (RRM-017), and whether one "family" is actually two producers. |

**What will break first when rules meet this document?**

| Measurement | Feeds |
| --- | --- |
| Characters per show operator (`Tj`/`TJ`) — mean, max, and share of operators carrying one character | RRM-003, RRM-011. Heavy fragmentation is the single most common producer behaviour synthetic fixtures do not reproduce. |
| Fonts: embedded, simple vs. Type0, `/ToUnicode` present | RRM-011, RRM-032. Missing `/ToUnicode` on a symbolic font means predicates match nothing. |
| Ligature, soft-hyphen, NBSP, and non-NFC codepoint counts | RRM-032. Directly sizes the normalization work scheduled in M2. |
| Form XObject count, nesting depth, reuse count | RRM-002. |
| Annotation count by `/Subtype`, `/AcroForm` present | RRM-019, RRM-009. |
| `/OCProperties` present | RRM-020. |
| Share of text runs at non-zero rotation | RRM-008, and the "predominantly horizontal text" scope claim. |
| Page size and rotation distribution | Whether zone-based rules can use absolute coordinates. |

**Does content stay inside one page?** — the page-locality section, detailed below.

### The page-locality section

This is the part the exit gate actually turns on, so it is specified tightly rather than left to
"inspect the PDFs."

For each multi-page document the survey emits:

1. **Repeat bands.** Lines whose normalized text appears on at least `--repeat-threshold` of pages at
   approximately the same `y`. Reported with their y-extent. These are the running headers, column
   headers, and footers — the structures that, if they carry a rule set's anchors, make page-local
   evaluation sufficient.
2. **Body extent.** Per page, the y-range between the last top repeat band and the first bottom
   repeat band: the region a rule set must resolve boundaries inside.
3. **First-page-only and last-page-only lines.** Text present on exactly one page. A terminator
   ("Total", "Balance due", a signature block) appearing only on the last page is the signature of a
   structure that spans pages; the same terminator on every page is the signature of one that does
   not.
4. **Body continuity.** For each consecutive page pair, whether the body region of page *n* runs to
   its bottom edge while page *n+1*'s body starts at its top edge with no intervening repeat band
   other than the header/footer. This is the closest mechanical proxy for "a table continued across
   the break."

The survey **reports these; it does not decide.** No table detector, no heuristic verdict. The
decision in Deliverable 4 is a human reading these numbers, and it cites them.

### Verifying the probe without the corpus

The survey must be trustworthy before the corpus arrives, and it can be: `RemediationFixtureGenerator`
already generates six documents whose properties are known by construction —
`Invoice`, `Statement`, `Report`, `Form`, `Multi-column`, and `Mixed page sizes`.

**New:** `test/PdfLexer.Tests/RemediationSurveyTests.cs`, asserting against generated fixtures that
the survey reports the truth we already know: the mixed-page-size fixture reports more than one page
size, the multi-column fixture reports its column structure in the body-extent data, every fixture
reports `hasStructTreeRoot: false` on its input and `true` on its remediated output, and character
counts match what the generator wrote.

These are synthetic and therefore prove only that the instrument reads correctly — which is all they
need to prove. RRM-011 is closed by the real corpus, not by these.

---

## Deliverable 2 — The corpus

Real internal PDFs cannot be committed to this repository. The layout works around that.

**Local, gitignored:**

```
.test-pdfs/remediation-corpus/<family-id>/<variant>.pdf
```

`.test-pdfs` is already in `.gitignore`. An environment variable, `PDFLEXER_REMEDIATION_CORPUS`,
overrides the root so the corpus can live outside the tree.

**Committed:**

```
test/corpus/remediation-corpus.manifest.json
```

The redacted survey output (`--redact`), plus a sha256 per file. This is what makes the exit gate's
"with the corpus evidence behind it" checkable by someone who cannot see the documents, and it makes
corpus drift detectable: if a file is replaced, the hash moves and the manifest diff shows what
changed about it.

**Test integration.** Corpus-backed tests skip cleanly when the corpus is absent, following the
existing `PdfCpuFactAttribute` pattern (`test/PdfLexer.Tests/PdfCpuFactAttribute.cs`) — a
`CorpusFactAttribute` whose `Skip` is set when the corpus root does not resolve. CI without the
corpus stays green; a developer with it gets the coverage.

**Minimum per family**, to satisfy "meaningful layout and data variation":

- the shortest realistic instance (ideally single-page);
- the longest available instance (multi-page, so page-locality is observable at all);
- at least one instance per declared variation axis — optional sections present/absent, long values
  that wrap, unusual character content, zero-row and many-row cases;
- if the producing system has been versioned in the corpus's lifetime, one instance from each
  version. A family that has never changed layout is a family whose drift risk is unmeasured, not
  absent.

Three variants is the floor. One document per family satisfies the letter of the exit gate and
defeats its purpose — a single sample cannot show variation, and variation is what RRM-016 and
RRM-017 exist to survive.

**This item has external latency and no engineering dependency. Start it first**, on day zero,
before the probe is written.

---

## Deliverable 3 — The family register

**New:** `docs/rule-based-remediation-families.md`.

Its own document rather than a section of the tracker, because it is the artifact M5 validates
against and M7's RRM-017 fingerprints are derived from, and because it will be revised on a different
cadence than the gap register.

One entry per family:

| Field | Notes |
| --- | --- |
| Family id | Stable slug; becomes the `RuleSet` id it binds to under RRM-017. |
| Producing system and version | Named concretely. "Our billing system" is not an entry. |
| Document purpose | One line. |
| Page count | Observed range across the corpus. |
| Variation axes | What differs between instances, and which corpus variants cover each. |
| Page-locality verdict | Per family — see Deliverable 4. Families may differ. |
| Known hazards | From the survey: fragmentation, missing `/ToUnicode`, path-drawn tables, existing annotations. |
| Corpus variants | Filenames, with manifest hashes. |
| In-scope / out-of-scope features | Which parts of the document the rule set will and will not tag. |

Plus a required **Explicitly unsupported** section — the exit gate asks for supported families *and*
unsupported cases in writing. At minimum it restates the MVP boundary from the tracker (OCR/scans,
existing tagged input, AcroForm remediation, nested lists, spanning tables, rotated text) and adds
anything the survey turned up that the MVP will not handle. This section is what a future reader
consults to find out whether their document is in scope, so it must be answerable without reading the
implementation.

---

## Deliverable 4 — The page-locality decision

> *"The page-locality decision is the most consequential item in this plan."* — the delivery plan.
> It is consequential because it is cheap now and expensive after M4 encodes the page-local
> assumption into every candidate type and action it adds.

### The question, operationally

For each family, using the survey's page-locality section:

1. Does every page independently carry the anchors a rule set needs to resolve its regions — i.e.
   does the repeat-band data show column headers and section labels repeating on all pages?
2. Does any logical section or table begin on one page and end on a later one, with intermediate
   pages carrying neither boundary? The body-continuity and last-page-only measurements answer this.
3. If yes to 2, is the spanning structure *semantically one thing* (one invoice line-item table that
   must read as one `Table`) or merely visually continuous (independent blocks that happen to
   abut)?

**Deferred** if every family answers yes to 1 and no to 2. **MVP requirement** if any family answers
yes to 2 and 3 — one family is enough, because the evaluation model is global.

### What the answer costs, concretely

*If deferred:* nothing changes. `RemediationSession.Evaluate` keeps its per-page `foreach`
(`RemediationSession.cs:256`), `EvaluatePage` keeps taking a single `PageRemediationState`, and M4 may
key new candidate types off `pageState` freely.

*If required:* the following change, and they are not additive — this is the list that makes the
retrofit cost real:

| Site | Why it changes |
| --- | --- |
| `RemediationSession.Evaluate` (`:256`) | The page loop becomes two-phase: resolve document-level flow state, then evaluate. |
| `RemediationEvaluationContext` construction (`:760`) | Built per page with `pageBox` and `structuredText` from one page. Cross-page anchor resolution needs a document-scoped view — and this is the same object RRM-028 must specify the scope of, which is why **RRM-028 gates RRM-001**. |
| `FlowRegionResolver.Resolve` | Resolves *both* boundaries against `_context.StructuredText`, so it structurally cannot span pages. Needs a start/continue/end state machine. |
| `FlowRegionResolution` | Carries one `PdfRect` and two sequence indices. A spanning region needs (page, index) pairs. |
| `FlowContinuationPolicy` / `FlowReadingOrderMode` | Declared (`FlowRegion.cs:98`) and parsed (`SerializedRemediationRules.cs:242`) but never read. `ContinueUntilEnd` is currently a no-op — the public model already promises this. |
| `CheckFlowRegionDiagnostics` (`:836`) | Overlap detection is per page. |
| `ApplyPlan` (`:1931`) | Applies claims page by page. A `Table` spanning pages needs one structure element with kids on several pages, which is legal PDF (`/Pg` per element) but not what the applier builds. |

Two things do **not** need rework, both worth recording:

- **MCID allocation.** `PageRemediationState.AllocateMcid` is per-page and owner-scoped — correct for
  cross-page structures already, courtesy of the gaps-2 #2 fix. A cross-page table's kids get valid,
  distinct MCIDs on each page for free.
- **Claim visibility.** `allClaims` is document-scoped and flows into each page's context
  (`RemediationSession.cs:761`, `allClaims.Concat(stageClaims)`), so a rule on page 3 can already
  reference a claim made on page 1. Cross-page *claim reference* works; cross-page *candidate and
  anchor resolution* does not. That asymmetry is undocumented, and a rule author could be relying on
  the half that works without realizing the other half is missing.

### Recording it

- The verdict and its evidence go in the family register, per family, citing specific manifest
  numbers — not "the corpus showed" but "invoice-v4 body continuity: 7 of 9 page pairs continuous;
  `Total due` appears on the last page only."
- Per the delivery plan's rule that status lives in the tracker, update **RRM-001's Status line and
  the priority table** in `rule-based-remediation-gaps.md`: either `Deferred — post-MVP (M6), per M0
  decision <date>` or `Open — MVP requirement, scheduled M3 per M0 decision <date>`.
- If it lands as an MVP requirement, add the M4 constraint to the delivery plan explicitly: M4's
  vocabulary is designed against the cross-page model, not retrofitted.

---

## Deliverable 5 — Re-confirm RRM-014 and RRM-015

Cheap, and the survey already produced the data.

- **RRM-014 (tagged input).** Any corpus file reporting `hasStructTreeRoot: true` will throw from
  `BeginRemediation`. If a whole family is tagged, that family leaves MVP scope — it does not become
  a bug report.
- **RRM-015 (scans).** Any file with pages carrying images and no extractable characters is
  image-only.

Record the counts in the family register and confirm both tracker entries stay
`Intentional limitation`. The delivery plan is explicit that these are re-decisions, not backlog
pulls: if the corpus is materially scanned or pre-tagged, M0 stops and the MVP scope is renegotiated
rather than quietly widened.

---

## Sequencing

Ordered by latency, not by dependency — the corpus request is the long pole and everything else fits
around it.

| # | Work | Depends on | Est. |
| --- | --- | --- | ---: |
| 0 | **Request corpus documents.** Name the families, state the minimum-variants requirement, send it. | — | 0.25d + external wait |
| 1 | Build `pdfctl survey` | — | 1–1.5d |
| 2 | `RemediationSurveyTests` against generated fixtures | 1 | 0.25d |
| 3 | Run the survey over the corpus; commit the redacted manifest | 0, 1 | 0.25d |
| 4 | Write the family register, including unsupported cases | 3 | 0.5d |
| 5 | Make and record the page-locality decision | 3, 4 | 0.5d |
| 6 | Re-confirm RRM-014 / RRM-015; update tracker statuses | 3 | 0.25d |

**≈ 3 engineer-days**, consistent with the delivery plan's 3–4 day M0 estimate given that two of six
items are already done. Steps 1–2 are unblocked immediately and should proceed while step 0 waits.

---

## Exit gate → deliverable mapping

| Exit gate item | Satisfied by | Evidence |
| --- | --- | --- |
| Both prerequisite defects fixed, with regression tests | ✅ Done | `accessibility_gaps_2_review.md`; regression tests in `AccessibilityAuthoringPhase2Tests` |
| Supported families and explicitly unsupported cases written down | Deliverable 3 | `docs/rule-based-remediation-families.md` |
| At least one real input per family from the actual producing system | Deliverable 2 | `test/corpus/remediation-corpus.manifest.json` — hashes and producer strings prove provenance without the documents |
| Page-locality decision recorded, with the corpus evidence behind it | Deliverable 4 | Family register verdicts citing manifest measurements; RRM-001 status updated in the tracker |

The gate is hard. A family with no real input does not pass on the strength of a synthetic
stand-in — that is precisely the RRM-011 gap M5 exists to close, and admitting a synthetic here
converts M5's evidence into impressions.

---

## Contingency: if real inputs do not arrive

The likeliest way M0 stalls. Handle it explicitly rather than by silently downgrading the gate.

**Permitted:** record a *provisional* page-locality decision, so M1 and M2 — neither of which depends
on it — can proceed.

**Required, if that happens:**

1. The decision is labelled `Provisional — no corpus evidence` in the family register and in RRM-001's
   status line. Not "decided."
2. The assumption it rests on is written down as a falsifiable statement: *"every page of family X
   repeats its column headers."* When the corpus arrives, that is one thing to check.
3. M4 does not start. M4 is where the assumption gets encoded into vocabulary, and the retrofit table
   in Deliverable 4 is the bill for guessing wrong. M1, M2, and M3 are all safe to run against a
   provisional decision; M4 is not.

**Not permitted:** marking the gate satisfied, or substituting fixtures generated by
`RemediationFixtureGenerator` for real producer output. The fixtures are PdfLexer's own output in
simple Helvetica — they cannot exhibit the fragmentation, encoding, and XObject behaviour the corpus
exists to reveal, so a decision made against them measures this library, not the documents.

---

## Risks

| Risk | Impact | Mitigation |
| --- | --- | --- |
| Corpus documents contain confidential data and cannot leave a controlled environment | High — blocks the whole milestone | `--redact` produces a committable manifest from documents that stay put; the survey runs where the documents live |
| "One family" turns out to be several producers with divergent layouts | Medium — inflates M5 and RRM-017 | The survey reports producer/creator per file; divergence shows up in the manifest before rule authoring starts |
| The survey's repeat-band heuristic misreads an unusual layout | Medium — a wrong page-locality decision | The survey reports measurements, not verdicts; a human reads them, and `--repeat-threshold` is tunable when a document looks wrong |
| Page-locality differs across families | Medium | The register records it per family; the global model must then satisfy the strictest family — one spanning family makes RRM-001 an MVP requirement |
| Building the probe expands into a diagnostics framework | Medium — M0 overruns | The probe reports and exits. Per-rule diagnostics are RRM-033, scheduled in M1 |

---

## Out of scope for M0

- Any RRM gap closure. M0 collects evidence and makes decisions; M1 is the first gap-closing
  milestone.
- Implementing cross-page flow, whichever way the decision lands. Implementation is M3 or M6.
- Authoring rule sets for the corpus families. That is M5.
- The RRM-017 applicability fingerprint. Deliberately M7 — a fingerprint cannot be designed until M5
  shows what it needs to assert. M0 only records the family ids it will eventually key off.
