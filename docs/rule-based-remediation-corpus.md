# Rule-Based Remediation — Validation Corpus Specification

Last updated: 2026-07-28

This document specifies the synthetic input documents used to validate
`PdfLexer.Remediation`. It is a build sheet: each entry says what to generate, what it exercises,
and what a test must assert.

| Document | Role |
| --- | --- |
| [rule-based-remediation.md](rule-based-remediation.md) | The rule language and API as shipped |
| [rule-based-remediation-gaps.md](rule-based-remediation-gaps.md) | Gap register (RRM-001 … RRM-035) |
| [rule-based-remediation-plan.md](rule-based-remediation-plan.md) | Milestones, gates, sequencing |
| **This document** | The input corpus those gaps are validated against |

---

## What synthetic fixtures can and cannot prove

Worth stating before the list, because it determines how results should be read.

**They can prove** that the engine handles a construct *when it is present* — a `TJ` array with no
space characters, a clip-only path, a pre-existing `/Artifact` mark. Every construct below is
reachable through `ContentWriter.Op(...)` / `Raw(...)` / `CustomOp(...)`, so the corpus can be made
deliberately adversarial rather than merely tidy.

**They cannot prove** that the engine handles what *real producers actually emit*, because the
corpus encodes our current beliefs about producer behavior. That is RRM-011, and it does not close
here. A construct nobody thought to synthesize stays untested.

The practical consequence: when real documents are sampled, the first job is to diff their content
streams against this corpus and add whatever is missing. Treat the list as a living floor, not a
ceiling.

---

## Current baseline

Six blueprints exist in `test/PdfLexer.Tests/RemediationFixtureGenerator.cs` — invoice, statement,
report, form, multi-column, mixed page sizes — each generated for UA-1 and UA-2, plus a strict
embedded-font invoice.

Their shared shape defines the coverage hole this document fills:

- single page (except mixed page sizes, which has two);
- one `Tj` per visual line, written through `Save().Font().TextMove().Text().Restore()`;
- Base14 Helvetica, **not embedded** (only `invoice-table-strict` embeds Roboto);
- ASCII only;
- text only — no images, paths, form XObjects, shadings, or annotations;
- no pre-existing marked content;
- `StrictConformance = false` in 36 of 37 test references, while production defaults to `true`.

So the existing corpus exercises rule mechanics on clean input, and almost nothing else. Nothing
below duplicates it.

---

## Construction conventions

Applies to every document unless the entry says otherwise.

1. **Deterministic output.** Fixed coordinates, fixed fonts, no timestamps or GUIDs in content.
   Byte-identical regeneration is what makes these usable as regression fixtures.
2. **Letter portrait, 40pt margins** unless the document is specifically about page geometry.
3. **Embed fonts by default.** Use `TrueTypeFont.CreateWritableFont` with `test/Roboto-Regular.ttf`.
   Base14 Helvetica is *only* for documents that deliberately test the non-embedded path — otherwise
   every fixture inherits a PDF/UA failure unrelated to what it is testing.
4. **Run with `StrictConformance = true`** unless the document exists to test permissive behavior.
   The permissive default in the current fixtures hides the path production actually takes.
5. **Expected results live outside the rule set.** Store expected claim counts, structure shape, and
   MCID ownership as separate fixture data, so a test cannot pass by asserting whatever the rules
   happened to produce (RRM-011 completion criterion).
6. **Variant pairs.** Several entries below are `-a` / `-b` pairs: same rule set, one input drifted.
   A single document cannot test drift detection — the pair is the unit.
7. **Visual invariance.** Remediation must not change appearance. See
   [Visual invariance](#visual-invariance) below for what that means concretely, how it is asserted,
   and the two places it is currently at risk.

---

## Visual invariance

Remediation is expected to be visually lossless. This is a realistic invariant rather than an
aspiration, because the overwhelming majority of what the engine does is add **non-painting**
operators — `BDC`/`EMC` scopes around content that is otherwise untouched.

Two operations can break it, and one currently does.

**Text operator splitting** — to claim a word inside a `Tj`, the engine splits the operator, and
that is the step that could move glyphs. `TextContent.TrySplitByCharacterRange` handles it
carefully: each produced segment captures the accumulated text state *before* its first glyph, so
fragments carry absolute positioning rather than re-deriving it, and `IsCharacterBoundary` refuses
to split inside a multi-character glyph rather than corrupting a ligature. Positioning is therefore
preserved by construction — but *by construction is not by test*, which is why C-01 and C-02 exist.
Residual risk sits in text state that accumulates across a cut: `Tw` (which applies to byte 32 in
single-byte encodings), `Tc`, `Tz`, `Ts`, and `TJ` kern adjustments straddling the split point.

**Created link annotations** — `AnnotationFactory.CreateBaseAnnotation` emits no `/Border`, no
`/BS`, and no `/F`. The PDF default `/Border` is `[0 0 1]`, so a viewer honoring the default draws a
1-unit frame the original document did not have. This is a live defect, noted in RRM-019, and the
fix is to emit `/Border [0 0 0]` (or `/BS << /W 0 >>`) on links the engine creates.

Two further cases are scope caveats rather than defects:

- **Optional content.** A new scope nesting inside or around an existing `/OC` sequence can change
  what is visible when that layer is toggled. The invariant is therefore stated for the **default
  OCG configuration**; non-default states are C-07's concern.
- **Shared form XObjects.** Mutating a form in place to insert MCIDs affects every page that
  references it. Not a visual change if correct, but it is the mechanism by which a local edit
  becomes global — see C-17.

### How it is asserted

Two complementary checks. Use both — they fail in different places and localize differently.

**1. Rasterized comparison — the backstop.** `util/pdflexer.PdfiumRegressionTester` already provides
this: `Compare.CompareAllPages(baseline, candidate, mode)` renders both documents through PDFium and
diffs per page, emitting a diff image on mismatch. It comes from the `PDFiumCore` NuGet package with
native binaries bundled, so unlike the veraPDF gate it needs no external tool installation and can
run in CI directly.

Use **`CompareMode.Exact`, not `VisualTolerance`.** The tolerance mode exists because rebuild and
merge regressions legitimately perturb anti-aliasing, so `VisualCompareOptions` suppresses
sub-threshold noise. Remediation has no such excuse: it adds non-painting operators and splits text
with absolute positioning preserved, so the raster should be bit-identical. Holding remediation to
`Exact` turns the harness into a much sharper instrument — **any** diff is a real defect rather than
a judgement call about thresholds.

Adapting it is a packaging exercise, not new work: `Compare.cs` is self-contained and depends only on
`PdfLexer`, `PDFiumCore`, and `ImageSharp`. Extracting it into a small shared library that both the
`util` tester and `PdfLexer.Tests` reference is the tidiest route.

**2. Content-model equivalence — the diagnosable one.** Compare before and after directly:

- every glyph's resolved bounding box, via `TextContent.GetGlyphBoundingBoxes()`;
- glyph selection and font identity per glyph;
- the sequence of painting items — images, paths, forms, shadings — and their CTMs;
- text state affecting advance: `Tw`, `Tc`, `Tz`, `Ts`.

This is what makes a failure actionable. A pixel diff says *page 2 changed*; the glyph comparison
says *this glyph moved 0.4pt because `Tw` was dropped across a split*. For the split-related failure
modes in C-02 — the ones most likely to actually occur — that difference is most of the debugging
time.

Neither check subsumes the other. Content-model equivalence cannot catch a writer defect that both
the before and after paths share; the raster comparison catches that but cannot tell you why. Run
content-model assertions inline in every fixture test, and `CompareMode.Exact` across the corpus as
a gate.

This satisfies the MVP exit gate's "rendering comparison shows no unintended visual change" item
directly, with existing tooling.

Apply both to **every** fixture by default. Keep an explicit opt-out list; that list is then the
honest inventory of where visual change is knowingly accepted — which today should contain exactly
one entry, the created-link border in C-08, and should shrink to zero once that is fixed.

---

## Tier 0 — Regression guards for shipped code

These cover functionality that exists in `main` but has no end-to-end test. Highest priority: the
code is live, the gaps are closed on paper, and nothing would catch a regression.

### C-01 — Unicode and normalization
**Validates:** RRM-032 · **Pairs with:** C-02

Single page. One line per construct, each also written in its normalized-equivalent form so the
pair can be asserted to match the same predicate:

- `fi` / `fl` ligatures (U+FB01, U+FB02)
- soft hyphen (U+00AD) mid-word
- non-breaking space (U+00A0) and narrow NBSP (U+202F) between words
- non-breaking hyphen (U+2011) in an identifier: `INV‑10042`
- curly quotes (U+2018/2019/201C/201D)
- a decomposed combining sequence (`e` + U+0301) against precomposed `é`
- a run of three literal spaces

**Assert:** literal predicates (`Equals`, `Contains`, `StartsWith`) match both forms; a regex
written against the *normalized* form matches, and one written against the raw form does not;
`TextNormalizationOptions.None` reverses this. Critically — **normalization affects matching only**:
glyph bounding boxes, glyph selection, and source character ranges are byte-identical before and
after, and MCID assignment matches the same document written in plain ASCII.

### C-02 — Operator fragmentation
**Validates:** RRM-032, RRM-003, RRM-026

Single page. The same logical line `Invoice #: INV-12345` rendered four ways at four y-positions:

1. one `Tj`;
2. a `TJ` array where word gaps are glyph adjustments with **no space characters** — the PDF/UA
   §7.2 word-boundary case;
3. three separate `Tj` operators, split at word boundaries;
4. one word split mid-token across two `Tj` operators (`INV-` + `12345`).

Plus a fifth line where two non-overlapping words must be claimed by two different rules in reading
order, and a sixth using `TJ` with adjustments for the same.

**Assert:** each form is selectable by the same word/line predicate; two rules tagging different
ranges of forms 4–6 produce correct, order-independent materialization; residual claims cover the
unclaimed remainder; reparsed text is byte-identical to input.

Then extend each of the six lines with non-default accumulating text state — `Tw` (word spacing,
which applies to byte 32 in single-byte encodings), `Tc`, `Tz`, and `Ts` — and split each one. This
is the sharp edge of visual invariance: assert every glyph bounding box is unchanged across the
split, not merely that the text reparses. A split that silently drops accumulated spacing reparses
perfectly and renders wrong.

### C-03 — Graphical content
**Validates:** RRM-002 · **Regression guard for:** `ResourceName`, `ResourceIdentity`, relative bounds

Single page containing, in one content stream:

- a logo image (`XObjImage`) top-left, placed once;
- the *same* image resource placed a second time in the footer (tests `ResourceUseCount`);
- table ruling lines — stroked horizontal and vertical paths;
- a filled background rectangle behind a header band;
- a **clip-only path** (`W n`) with no painting operator;
- a form XObject placed twice at different CTMs;
- a shading fill;
- ordinary body text.

**Assert:** the clip-only path is *not* enumerated as a painting candidate; each other item is
enumerated with the correct `RemediationCandidateKind`; the twice-placed image reports
`ResourceUseCount == 2`; **the logo is selectable by a stable, serializable identity** and taggable
as `Figure` with `/Alt`; ruling lines and background are artifactable; `RelativeBoundingBox` is
page-relative and a zone predicate over the footer selects the footer placement and not the header
one; the page commits with no untagged-content diagnostic.

> This document is the direct regression test for the three non-text defects found in review:
> `ResourceName` never populated, `ResourceIdentity` derived from a runtime hash, and absolute
> bounds passed as relative bounds.

### C-04 — Leftover accounting
**Validates:** RRM-016, RRM-002 · **Regression guard for:** graphical auto-artifact reporting

Single page with a rule set that deliberately leaves content unclaimed: two body lines no predicate
matches, plus an unclaimed decorative image and an unclaimed path.

Generate under all three leftover policies.

**Assert:** under `Flag`, a diagnostic names the unclaimed text *and* graphical counts; under
`FailFast`, commit is refused; under `AutoArtifact`, **`report.AutoArtifacts` contains an entry for
every absorbed item — text and graphical alike — with its content or bounds and its disposition**.
An artifacted item that appears nowhere in the report is the failure this document exists to catch.

Also generate the `AutoArtifact` variant with an artifact inventory declaring only footer furniture:
every absorbed item outside that zone must report `ArtifactUndeclared`, and each entry's
`InventoryItemId` must name the item it matched or be null.

### C-05 — Semantic assertion catches a conformant error
**Validates:** RRM-018

Invoice-shaped page with a rule set containing a deliberate error: the invoice total is claimed by a
rule that artifacts it, and the footer is tagged `H1`.

**Assert:** the output **passes veraPDF for PDF/UA-1** while the assertion set fails, with
provenance naming the rule, page, and expected/observed counts. This is RRM-018's completion
criterion stated as a fixture, and the clearest demonstration of why conformance is not correctness.

### C-06 — Template drift pair
**Validates:** RRM-016 · **Pair**

`C-06-a`: statement with the label `Account Statement` and a `Bill To` block.
`C-06-b`: identical except the label reads `Statement of Account` and the `Bill To` block is
shifted 30pt right.

One rule set, authored against `-a`, with declared cardinality.

**Assert:** `-a` commits clean; `-b` **fails the run** with a cardinality diagnostic naming rule,
page, and observed count — and specifically does *not* silently sweep the `Bill To` block into an
artifact under `AutoArtifact`.

---

## Tier 1 — Near-term roadmap enablers

Fixtures the next work items need in place before they start.

### C-07 — Pre-existing marked content
**Validates:** RRM-020

Untagged document (**no** `StructTreeRoot`) whose content stream nonetheless contains, via raw
operators:

- a producer `/Artifact` `BDC`/`EMC` pair around a running header;
- an `/OC` optional-content sequence around a watermark, with the OCG default-off;
- a `/Span` `BDC` with `/MCID 0` through `/MCID 4` already assigned, referenced by nothing;
- ordinary unmarked body text.

**Assert:** new MCID allocation starts above 4 and collides with nothing; candidates report which
marked-content sequence encloses them; content already marked `/Artifact` is not double-counted by
the leftover policy; optional content is either handled deliberately or diagnosed as unsupported —
never silently tagged.

### C-08 — Pre-existing annotations
**Validates:** RRM-019

Two-page document with live `Link` annotations over body text (one internal destination, one URI),
a `Stamp`, a `Popup`, and a `FileAttachment`.

**Assert:** commit diagnoses every annotation left with no structure binding; once adoption lands,
existing annotations are bound to `Link`/`Annot` structure elements **without creating replacement
annotations**, receive `/StructParent`, and their dictionaries are otherwise unmodified.

Add a third case where a rule uses `RemediationActions.Link` to create a *new* link. Assert the
created annotation carries an explicit zero-width border — this is the one place the engine
currently introduces visual change, since `CreateBaseAnnotation` emits no `/Border` and the PDF
default is `[0 0 1]`.

### C-09 — Conformance floor (three variants)
**Validates:** RRM-026

- `C-09-a`: Base14 Helvetica, **not embedded**.
- `C-09-b`: embedded font with `/ToUnicode` removed.
- `C-09-c`: embedded font with a `.notdef` glyph reachable in the text.

**Assert:** each is reported by input pre-flight **before rules are evaluated**, as a diagnostic
naming the font and page — not as a save-time exception, and not as a rule failure. The
documentation states these are outside what any rule set can repair.

### C-10 — Running header and footer
**Validates:** RRM-022

Four-page document with a header repeating on every page (`Acme Corp — Confidential`), a footer with
`Page N of 4`, and a diagonal watermark.

**Assert:** artifacts carry `/Type /Pagination` with `/Subtype /Header` and `/Footer` respectively,
the watermark carries `/Subtype /Watermark`, and `/BBox` and `/Attached` are emitted. Assert the
dictionary contents directly — this is the most common artifact in transactional documents and the
current subtype set cannot express it.

### C-11 — Heading level skip pair
**Validates:** RRM-023 · **Pair**

`C-11-a`: `H1` title, `H2` section, `H3` subsection — sized 18/14/12pt.
`C-11-b`: identical minus the 14pt section, so a font-size rule emits `H1` then `H3`.

**Assert:** `-a` validates clean; `-b` is diagnosed as a skipped heading level, or normalized to
`H1`/`H2`. The point is that the defect is *instance-dependent* and survives testing against a
single sample.

---

## Tier 2 — Structural model

Larger constructs. Several are prerequisites for deciding scope rather than for closing a gap.

### C-12 — Multi-page continuous table
**Validates:** RRM-001, RRM-005 · **Promoted to Tier 0 by the M0 decision**

Four pages. A table with a header row repeated on every page, body rows continuing across all four,
and a subtotal row only on page 4. Section heading on page 1 only. Add a second flow region — a
terms section starting mid-page-2 and ending on page 3 — so grouping continuation is covered
independently of the table.

**Assert:** one logical `Table` spans all four pages; its structure element references MCIDs on
multiple pages through `/MCR` entries carrying per-item `/Pg`; intermediate pages resolve without
spurious anchor-failure diagnostics; the repeated header follows the declared repeated-header policy
rather than emitting four separate `THead` blocks; the subtotal row is a child of the same table as
the page-1 rows; and MCIDs remain page-scoped and unique.

The M0 page-locality decision (2026-07-28) made cross-page spanning an MVP requirement, so this is no
longer a document that records current behavior for later — it is the acceptance fixture for M3a.
Build it alongside that work rather than after it.

### C-13 — Irregular table
**Validates:** RRM-005, RRM-025

Single page, one table with: a wrapped two-line cell; a sparse row missing its middle cell; a
right-aligned numeric column; baselines varying ±2pt within a row; one `RowSpan` and one `ColSpan`;
and both a column header row and a row-header column.

**Assert:** wrapped cells do not become separate rows; multiple fragments in one column do not
become duplicate cells; spans affect both attributes and grid occupancy; row headers can be declared
with `/Scope Row` rather than the hardcoded `Column`; `/Summary` is settable.

### C-14 — Flat lists
**Validates:** RRM-021

Single page with a decimal ordered list, an alpha ordered list, a bulleted list, and one list with a
wrapped multi-line item. Each item rendered as one line with the label and body in the same text
operator — `1. Widget assembly`.

**Assert:** `L → LI → {Lbl, LBody}` with the label split from the body, and `/ListNumbering` set per
list type. This is the fixture that forces the **split primitive** design decision the plan flags
under M6 — no current action divides a claim.

### C-15 — Multi-column with sidebar
**Validates:** RRM-012

Single page, two body columns of **at least twelve lines each** so interleaving is detectable, plus
a left sidebar, a heading spanning both columns, and a footer.

**Assert:** complete logical text order — column one fully before column two — with the spanning
heading first and the sidebar in a declared position. The existing multi-column fixture is five
lines and cannot distinguish correct ordering from coincidence.

### C-16 — Figure and caption
**Validates:** RRM-007

Single page with two images: the first with its caption painted **before** the image operators, the
second with the caption **after**.

**Assert:** both produce the same `Figure`/`Caption` association, independent of content-stream
order.

---

## Tier 3 — Robustness and deferred scope

Build when the corresponding work starts, or earlier if a sampled real document demands it.

| ID | Document | Validates | Notes |
| --- | --- | --- | --- |
| C-17 | Nested and transformed form XObjects — text inside a form placed twice at different CTMs, one form nested in another | RRM-002, RRM-011, gaps-2 #2 | Coordinate mapping and per-owner MCID allocation |
| C-18 | Rotated pages (`/Rotate` 90/180/270) plus independently rotated text at 90° on an unrotated page | RRM-008 | Must distinguish page rotation from text rotation |
| C-19 | Multi-page document with a tagged table of contents | RRM-024 | Triggers the strict-mode `TOC` + `PageLabels` trap — currently uncommittable with no declarative remedy |
| C-20 | Interactive AcroForm — text fields, checkbox, radio group, repeated widgets | RRM-009 | Distinct from the existing painted-text "form" fixture |
| C-21 | Two family documents sharing a common boilerplate rule set | RRM-031 | Composition order, precedence, `Override` across set boundaries |
| C-22 | Document with a pre-existing `StructTreeRoot` (partial and incorrect) | RRM-014 | Currently rejected outright by `BeginRemediation`; build only if a sampled family carries one |

---

## Tier 0/1 additions — transactional core and language surface

Added after auditing the corpus against the shipped rule-language surface and against what
transactional documents actually contain. The entries above are organised around *gaps*; these are
organised around *the language* and *the document type*, which turn out to be different axes.

### C-23 — Label/value pairs and anchor geometry
**Validates:** anchor-relative predicate family, RRM-021 split, RRM-028 · **Tier 0**

This is the single most common structure in transactional documents and the corpus above does not
target it. Single page containing label/value pairs in every arrangement real documents use:

- label and value in **separate** text operators, value to the right (`Invoice #`  `INV-10042`);
- label and value in **one** text operator (`Invoice #: INV-10042`) — requires a split;
- label above, value below (`Due Date` / `2026-01-15`);
- label with the value two columns right, with an unrelated field between;
- a label whose value is empty;
- a right-aligned value in a totals block, label left-aligned on the same row.

**Assert:** `Anchor.RightOf`, `Below`, `SameRowAs`, `SameColumnAs`, `Between`, and `NearestTo` each
select the intended value and nothing else; `maxDistance` and `tolerance` bound the match; the
empty-value case is diagnosed rather than silently matching the next field; the single-operator case
tags label and value separately.

> The anchor-relative family is how nearly every hand-authored transactional rule will be written,
> and no document in the corpus currently exercises it. This should arguably be built first.

### C-24 — Ambiguous and repeated anchors
**Validates:** RRM-028, anchor selection modes · **Tier 0**

Two pages. Page 1 contains the word `Date` three times — in the header, in a table column heading,
and in the footer. `Amount` appears as both a column heading and a totals label. Page 2 repeats the
header/footer labels but omits the table.

**Assert:** `RequiredSingle` fails on page 1 with a diagnostic distinguishing **"ambiguous match on
this page"** from **"no match on this page"**; `Occurrence`, `Style`, `NeighborText`, and
`NthInReadingOrder` each disambiguate; `Pages` narrows scope so the page-2 absence is not an error;
and the one-based `Occurrence` versus zero-based `NthInReadingOrder` behaviour is pinned by test.

### C-25 — Variable-presence sections
**Validates:** RRM-016, RRM-017 · **Pair (three variants)** · **Tier 1**

The defining source of transactional variability, and broader than C-11's heading case.

`-a`: invoice with a discount line, a notes block, and two line items.
`-b`: same template, no discount line, no notes block, seven line items spilling the table lower.
`-c`: same template, discount present, notes absent, one line item.

One rule set, authored against `-a`, with cardinality declared as ranges rather than exact counts.

**Assert:** all three commit clean with **no per-file rule changes** — the MVP exit gate's central
claim; optional sections produce no diagnostics when absent; the table handles 1, 2, and 7 rows;
and totals stay correctly associated as the table grows.

### C-26 — Dense graphics and symbolic glyphs
**Validates:** RRM-002, RRM-026 · **Tier 1**

Transactional documents carry two graphics cases the corpus above misses:

- a **barcode** and a **QR block** rendered as several hundred small filled rects — both a semantic
  test (artifact, or `Figure` with `/Alt`) and a scale test for candidate enumeration;
- **checkbox glyphs from a symbolic font** (ZapfDingbats ✓/✗, or Wingdings) with no useful
  `/ToUnicode` — ubiquitous on EOBs, claim forms, and remittance advices;
- a scanned-looking signature image.

**Assert:** the barcode is selectable as a unit rather than as hundreds of candidates, or the
per-candidate cost is bounded and documented; symbolic glyphs are diagnosed by pre-flight as
lacking `ToUnicode` rather than silently extracting as garbage; candidate enumeration over a
path-heavy page stays within a stated budget.

### C-27 — Refine stage and claim-predicate surface
**Validates:** RRM-029, RRM-031, RRM-035 · **Tier 1**

A compact document whose purpose is API coverage rather than realism: enough tagged content to
exercise every refine action and claim predicate at least once.

**Assert:** `Lang`, `Alt`, `ActualText`, `Expansion`, and `Attributes` each reach the emitted
structure element; `MergeTo` flattens as documented; `ReorderSiblings` works in all three
`SiblingReorderMode` values and is scoped to a parent; `Group` composes; and the claim predicates —
`ClaimIs`, `ActionIs`, `FromRule`, `FromRuleSet`, `StatusIs`, `SamePage`, `Consecutive`, `Within`,
`BeforeClaim`, `AfterClaim` — each select what they claim to. Also covers `Override` precedence and
`MinConfidence` against a toleranced zone.

### C-28 — Landscape wide table
**Validates:** RRM-005, RRM-008 · **Tier 2**

Landscape page with a twelve-column table too wide for portrait, plus a portrait page in the same
document.

**Assert:** column inference works at landscape aspect; mixed orientation within one document does
not corrupt zone or page-relative geometry.

---

## Language surface coverage

Audit of the corpus against the shipped rule language. Included because gap-driven coverage and
API-driven coverage diverge — several heavily-used constructs sit behind no open gap and were
therefore invisible to the list above.

| Surface | Covered by | Status |
| --- | --- | --- |
| `Predicates.Text` (4) | C-01, C-02 | Complete |
| `Predicates.Content` (4) | C-03 | Complete |
| `Predicates.Font` — `Size` | C-11 | Complete |
| `Predicates.Font` — `Weight`, `Family`, `Italic` | C-27 | **Added here** |
| `Predicates.Color.IsGrayish` | C-27 | **Added here** |
| `Predicates.Geo` (3) | C-03, C-10 | Adequate |
| `Predicates.Anchor` (7) | C-23, C-24 | **Added here — was absent** |
| `Predicates.Relational` (4) | C-27 | **Added here** |
| `Predicates.Flow` (6) | C-12, C-15 | Partial — `NthIn`/`FirstAfter` thin |
| Anchor kinds (8) + `Occurrence`/`Style`/`Pages`/`NeighborText` | C-23, C-24 | **Added here** |
| `ClaimPredicates` (10) | C-27 | **Added here** |
| Actions — `Tag`, `Artifact`, `Table`, `Group`, `Link` | C-03, C-10, C-13, C-14, C-08 | Complete |
| Actions — `MergeTo`, `ReorderSiblings`, `Lang`, `ActualText`, `Expansion`, `Attributes` | C-27 | **Added here** |
| Actions — `Custom` | — | Unit-tested only; non-serializable by design (RRM-035) |
| `Override`, `MinConfidence` | C-27 | **Added here** |
| `RuleCardinality` | C-06, C-25 | Complete |
| `PageSelector` | C-24 | Adequate |
| Granularity — Word, Line, Paragraph | C-01, C-02 | Complete |
| Granularity — Character | C-02 | Adequate |
| Stages — Classify, Group | Throughout | Complete |
| Stage — Refine | C-27 | **Added here — was thin** |
| UA-1 and UA-2 profiles | All fixtures, both profiles | Complete |

---

## Coverage map

| Gap | Priority | Covered by |
| --- | --- | --- |
| RRM-001 | P0 | C-12 |
| RRM-002 | P0 | C-03, C-04, C-17 |
| RRM-003 | P0 | C-02 |
| RRM-005 | P0 | C-12, C-13 |
| RRM-007 | P1 | C-16 |
| RRM-008 | P1 | C-18 |
| RRM-009 | P1 | C-20 |
| RRM-010 | P0 | All — every fixture runs veraPDF for its profile |
| RRM-011 | P1 | Whole corpus (partially — see limits above) |
| RRM-012 | P1 | C-15 |
| RRM-014 | P2 | C-22 |
| RRM-016 | P0 | C-04, C-06 |
| RRM-018 | P0 | C-05 |
| RRM-019 | P0 | C-08 |
| RRM-020 | P1 | C-07 |
| RRM-021 | P1 | C-14 |
| RRM-022 | P1 | C-10 |
| RRM-023 | P1 | C-11 |
| RRM-024 | P1 | C-19 |
| RRM-025 | P1 | C-13 |
| RRM-026 | P0 | C-09 |
| RRM-031 | P1 | C-21 |
| RRM-032 | P0 | C-01, C-02 |

Second pass, from the language-surface and transactional audit:

| Gap | Priority | Covered by |
| --- | --- | --- |
| RRM-017 | P0 | C-25 |
| RRM-021 (split) | P1 | C-14, C-23 |
| RRM-028 | P1 | C-23, C-24 |
| RRM-029 | P1 | C-27 |
| RRM-031 | P1 | C-21, C-27 |
| RRM-034 | P2 | C-26 |

Not covered by input documents, deliberately: RRM-027, RRM-030, and RRM-033 through RRM-035 are
contract, diagnostic, and API-surface gaps validated by unit and report-level tests rather than by
new inputs. RRM-028, RRM-029, and RRM-031 appear above because although they are contract gaps, the
contracts are only observable against specific geometry — an ambiguous anchor needs a document with
an ambiguous anchor in it.

### Transactional structure coverage

| Pattern | Covered by |
| --- | --- |
| Label/value pairs | C-23 |
| Address blocks (Bill To / Ship To) | C-06, C-23 |
| Line-item tables | C-12, C-13 |
| Totals and summary blocks | C-05, C-23, C-25 |
| Running header / footer / page N of M | C-10 |
| Watermarks (PAID, VOID, DRAFT) | C-10 |
| Logos and branding | C-03 |
| Barcodes and QR blocks | C-26 |
| Symbolic-font checkboxes | C-26 |
| Signature images | C-26 |
| Optional and variable-presence sections | C-25, C-11 |
| Multi-page tables with repeated headers | C-12 |
| Terms and conditions boilerplate | C-15 |
| Live hyperlinks | C-08 |
| Landscape wide tables | C-28 |
| Interactive form fields | C-20 |

---

## Generation notes

- Extend `RemediationFixtureGenerator`'s blueprint array rather than adding a parallel mechanism;
  the input/fixture split and the `GenerateAll` lock already do the right thing.
- Raw operator emission — needed for C-02, C-07, and the clip-only path in C-03 — goes through
  `ContentWriter.Op(IPdfOperation<T>)`, aliased as `Raw` and `CustomOp`.
- `PageWriter.BeginArtifact(PdfName?)` is the current artifact entry point and is what C-10 will
  need extended.
- Annotations for C-08 and C-20 come from `AnnotationFactory`.
- Keep each document's *input* checked in alongside the generator. Regenerating inputs from a
  changed writer silently changes what the corpus tests — the input is the fixture, not the code
  that produced it.
- Visual comparison reuses `Compare` from `util/pdflexer.PdfiumRegressionTester` at
  `CompareMode.Exact`. Extract it into a shared library rather than duplicating it; it depends only
  on `PdfLexer`, `PDFiumCore`, and `ImageSharp`, all of which resolve from NuGet with no external
  tool installation.
