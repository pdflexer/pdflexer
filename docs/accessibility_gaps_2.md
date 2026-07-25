# Accessibility Gaps — Round 2

Second-pass review of the accessible-authoring surface, focused on **producing new tagged content in new or
currently untagged documents**. Editing pre-existing structure trees is explicitly out of scope.

This document supersedes [`accessibility_gaps.md`](accessibility_gaps.md) where the two disagree. Round 1 closed the
missing-primitive gaps (`/Alt`, `/ActualText`, `/E`, `/Scope`, `/Headers`, `/ListNumbering`, document setup, the
remediation engine). Round 2 covers defects in how those primitives are **serialized and validated**, plus the
coverage gaps that keep otherwise-correct output from passing veraPDF.

Files reviewed:

- `src/PdfLexer/Writing/StructuralSerializer.cs`
- `src/PdfLexer/Writing/PageWriter.cs`, `FormWriter.cs`, `ContentWriter.Model.cs`
- `src/PdfLexer/DOM/StructuralBuilder.cs`, `StructureNode.cs`, `Annotations.cs`
- `src/PdfLexer/PdfDocument.Accessibility.cs`, `PdfDocument.Saving.cs`
- `docs/accessibility-authoring.md`, `docs/StructuralBuilder.md`

Every finding below was reproduced against the current source by compiling and running the affected code path.
Where output is shown, it is verbatim.

---

## Executive Summary

The structure-tree machinery is fundamentally sound. `StructuralSerializer` emits a correct `StructTreeRoot` with
`ParentTree`, `IDTree`, `RoleMap`, `ClassMap`, namespaces, `OBJR` object references, `MCR` references for elements
spanning pages, and `/StructParents` wiring for pages and form XObjects. Strict mode already catches untagged real
content and non-embedded fonts — a genuinely useful guard that most libraries lack.

The remaining problems are at the edges:

- **The two silent-corruption defects are resolved.** Accessibility text strings now switch to UTF-16BE for
  non-ASCII values, and MCIDs are allocated per page/form namespace with duplicate-registration guards.
- **Phase 2 conformance-blocker implementation is complete; external validation is pending.** Arbitrary
  annotations can be bound to structure, generated widgets have embedded-font appearances, and unresolved
  references fail or warn instead of disappearing. The findings remain open until pinned veraPDF acceptance.
- **Several silent-drop paths** turn author mistakes into invalid output instead of exceptions.
- **The documentation did not compile.** Corrected under this effort; see [Phase 0](#phase-0--documentation-completed).

Nothing here is architectural. The two corruption bugs are small, localized fixes.

### Priority Summary

| # | Item | Severity | Effort | Phase |
| --- | --- | --- | --- | --- |
| [1](#1-non-latin-1-text-strings-are-corrupted-on-write) | Non-Latin-1 text strings corrupted | **Resolved** | S | 1 ✅ |
| [2](#2-mcid-counters-are-per-writer-not-per-page) | MCID counters per-writer, not per-page | **Resolved** | S–M | 1 ✅ |
| [3](#3-annotations-other-than-linkwidget-cannot-be-tagged) | Non-link/widget annotation tagging | **Implemented; validation pending** | M | 2 |
| [4](#4-generated-form-widgets-are-not-pdfua-conformant) | Conformant generated form widgets | **Implemented; validation pending** | M | 2 |
| [5](#5-unresolvable-idsreferences-are-dropped-silently) | Reliable structure-reference resolution | **Implemented; validation pending** | S | 2 |
| [6](#6-heading-levels-are-not-validated) | Heading levels not validated | Medium | S | 3 |
| [7](#7-artifacts-cannot-express-subtype-bbox-or-attached) | Artifacts lack `/Subtype`, `/BBox`, `/Attached` | Medium | S | 3 |
| [8](#8-table-conformance-rules-are-only-half-checked) | Table conformance only half-checked | Medium | M | 3 |
| [9](#9-the-back-chaining-api-is-error-prone) | `.Back()` chaining is error-prone | Medium | M | 4 |
| [10](#10-pdfua-2-namespaces-are-only-applied-to-the-root) | PDF/UA-2 namespaces only on root | Medium | S–M | 4 |
| [11](#11-missing-structure-type-helpers-and-layout-attributes) | Missing type helpers / layout attributes | Low | S | 4 |
| [12](#12-addnote-is-unusable-under-strict-mode) | `AddNote()` unusable under strict mode | Low | XS | 4 |
| [13](#13-parenttreenextkey-is-never-written) | `/ParentTreeNextKey` not written | Low | XS | 4 |
| [14](#14-appending-tagged-content-to-an-already-tagged-document) | Cannot append to already-tagged docs | Scope decision | L | 5 |

Effort key: XS ≈ under an hour · S ≈ half a day · M ≈ 1–3 days · L ≈ a week or more.

---

## Phase 0 — Documentation (completed)

Every code block in `docs/accessibility-authoring.md` was compiled against the current source. **All eight snippets
failed**, with six wrong API names:

| Documented | Actual |
| --- | --- |
| `Base14.Helvetica` | `Standard14Font.GetHelvetica()` (type `Base14` does not exist) |
| `doc.Save("out.pdf")` | `doc.SaveTo(path)` |
| `.ExpansionText(...)` | `.Expansion(...)` |
| `TableScope.Column` | `StructureScope.Column` |
| `ListNumbering.Decimal` | `StructureListNumbering.Decimal` |
| `DeclareNamespace(uri, prefix)` | `DeclareNamespace(uri)` |

After correcting the names the headline example *still* failed at save, because `strictConformance` defaults to
`true` and rejects the Standard-14 font the example uses:

```
PdfAccessibilityConformanceException: Strict accessibility requires all rendered fonts
for tagged content to be embedded. Font 'Helvetica' is not embedded.
```

`docs/StructuralBuilder.md` had two further defects: its `.Back()` example built the **wrong tree** (`Details`
nested inside `Introduction` instead of as its sibling — a live demonstration of finding [9](#9-the-back-chaining-api-is-error-prone)),
and its `WriteContent` example threw `NotSupportedException: Must set current font before writing.`

**Done:** both documents rewritten with snippets that compile and run, an up-front prerequisites section covering
the font and the then-current one-writer-per-page requirement, a new artifacts section, and a Known Limitations
table pointing here. The writer restriction was removed when finding 2 was resolved.
Stale `Base14` references were also corrected in `docs/agent_instruction.md`, `docs/content_creation.md`, and the
`examples/*.ipynb` notebooks.

---

## Phase 1 — Silent Corruption

These two produce invalid output with no error at any point. They should land before anything else, because every
document authored in the meantime is potentially affected and the damage is invisible.

### 1. Non-Latin-1 text strings are corrupted on write — resolved

**Severity: Resolved · Effort: S**

Resolved by routing the accessibility-authoring surface through `PdfString.CreateTextString`, which retains the
compact representation for ASCII and emits UTF-16BE for every non-ASCII value.

`StructuralSerializer` writes every text-string value with `new PdfString(value)`, which encodes as PDFDocEncoding.
Any character outside Latin-1 is destroyed:

```
Alt written  : 图表：季度收入 — Résumé
Alt read back: ??:???? - Résumé
```

Note that even the em-dash degrades to a hyphen. Affected keys: `/ID`, `/T`, `/Alt`, `/ActualText`, `/E`, `/Lang`
(`StructuralSerializer.cs:42-70`), `/Summary` (`~:270`), annotation `/Contents` (`ApplyAnnotationAccessibility`),
and in `Annotations.cs` the field `/T` and `/TU` tooltip.

This makes alternate text unusable for any non-Latin-script document — precisely the audience that depends on it
most. It is also a data-loss bug: the original string cannot be recovered from the output.

The fix already exists in the codebase but is not reachable from the serializer.
`PdfDocument.Accessibility.cs:73` has:

```csharp
private static PdfString CreateTextString(string value)
{
    if (value.Any(c => c > 255))
    {
        return new PdfString(value, PdfStringType.Literal, PdfTextEncodingType.UTF16BE);
    }
    return new PdfString(value);
}
```

**Completed**

- [x] Promote `CreateTextString` to a shared internal helper (`PdfString.CreateTextString`).
- [x] Route every accessibility text-string write in `StructuralSerializer`, `AnnotationFactory`, and the
      labeled-widget builder through it.
- [x] Widen the guard: the old `c > 255` test emitted PDFDocEncoding for the 0x80–0xFF range, where
      PDFDocEncoding and Latin-1 disagree (the em-dash above sits here). Prefer UTF-16BE for anything non-ASCII.
- [x] Regression test: round-trip CJK, Arabic, Hebrew, and typographic punctuation through `/Alt`, `/ActualText`,
      `/E`, `/T`, `/Summary`, and annotation `/Contents`.

### 2. MCID counters are per-writer, not per-page — resolved

**Severity: Resolved · Effort: S–M**

Resolved with an allocator keyed to the underlying page or form object, seeded from existing marked content and
shared by page writing and remediation. Duplicate ParentTree registrations now throw.

`PageWriter.CurrentMCID` (`PageWriter.cs:25`) is instance state on the writer. A second `page.GetWriter()` on the
same page restarts numbering at 0, so two different structure elements both claim `/MCID 0`. The `ParentTree`
build keeps only the last writer into each slot, and the earlier element vanishes from the tree:

```
p1 content items -> (page, 0)
p2 content items -> (page, 0)
ParentTree: key 0 -> array len 1     # one of the two paragraphs is gone
```

No exception is raised at any point. `FormWriter.cs:29` has the same defect.

This matters most for the "add tagged content to an existing PDF" workflow, which is exactly the scenario the
authoring API targets: writing in several passes is natural, and an existing untagged page may already contain
marked-content sequences whose MCIDs start at 0 and collide with the new ones.

**Completed**

- [x] Move MCID allocation to per-page (and per-form-XObject) state rather than per-writer.
- [x] Seed the counter from the highest MCID already present in the page's content streams, so appending to a page
      that already has marked content cannot collide.
- [x] Detect duplicate `(page, MCID)` and `(form, MCID)` registrations in
      `StructuralSerializer.BuildParentTree` and throw rather than
      overwrite — a cheap backstop that turns any remaining path into a loud failure.
- [x] Regression tests: two writers on one page; appending to a page with pre-existing MCIDs; a form XObject
      written in two passes.

---

## Phase 2 — Conformance Blockers

Output that is structurally well-formed but still fails PDF/UA validation. These need new API surface, not just
fixes.

### 3. Annotations other than Link/Widget cannot be tagged — implemented, validation pending

**Severity: High · Effort: M**

ISO 14289-1 §7.18.1 requires every annotation to be represented in the structure tree (as `Annot`, `Link`, or
`Form`) or marked as an artifact. Today:

- `AnnotationFactory` only creates `Link` and a text `Widget`.
- `StructuralBuilder.BindAnnotation` is `internal`, so there is no way to bind an externally-created annotation.
- The post-serialization validator walks page content streams only — it never inspects `/Annots`.

A `Square` annotation added to a strict PDF/UA-1 document saves with no error and no `/StructParent`. The result is
non-conformant and the library reports success.

**Tasks**

- [x] Add `IStructureContext.BindAnnotation(PdfDictionary annotation, PdfPage page)` (public) so any annotation
      subtype can be attached to an `Annot` element.
- [x] Add `AddAnnot(...)` convenience matching `AddLink` / `AddFormField`.
- [x] Extend `ValidateAccessibilityAuthoringAfterSerialization` to sweep every page's `/Annots` array and throw for
      any annotation lacking `/StructParent` that is not `Popup` and not hidden (`/F` bit 2).
- [x] Confirm the Matterhorn nesting rules while there: `Link` annotations inside `Link` elements, `Widget` inside
      `Form`, everything else inside `Annot`.

### 4. Generated form widgets are not PDF/UA-conformant — implemented, validation pending

**Severity: High · Effort: M**

The widget produced by `AddFormField` / `AnnotationFactory.CreateTextWidget`:

```
widget keys : /Type /Subtype /P /Rect /FT /F /TU /Parent /StructParent   (no /AP)
AcroForm    : /Fields                              (no /DA, /DR, /NeedAppearances)
```

ISO 14289-1 §7.18.1 requires an appearance stream on annotations other than `Link` and `Popup`. Without `/DA` and
`/DR` on the AcroForm, Acrobat also renders the field blank. Only text fields exist — no checkbox, radio, choice,
pushbutton, or signature.

**Tasks**

- [x] Generate a default `/AP` normal appearance stream for created widgets.
- [x] Populate `AcroForm` with `/DA` and a `/DR` resource dictionary containing the field font; set
      `/NeedAppearances` only as an explicit opt-in, since it is a poor substitute for real appearances.
- [x] Add factories for checkbox, radio group, choice (combo/list), and pushbutton widgets, each with the
      `/TU` tooltip wiring `AddLabeledFormField` already does for text fields.
- [x] Validate the widget's `/F` print flag and that `/TU` is non-empty under strict mode.
- [x] Add a fillable-form fixture to the veraPDF corpus covering every field type.

### 5. Unresolvable IDs/references are dropped silently — implemented, validation pending

**Severity: High · Effort: S**

`StructuralSerializer` resolves `TableHeaders(...)` and `References(...)` against `StructureRoot.IdMap` and emits
the entry only `if (headers.Count > 0)` / `if (refs.Count > 0)` (`~:246-260`, `~:85-97`). An ID that never existed
produces no `/Headers` entry and no error — the cell is left with a meaningless `/A << /O /Table >>`.

This is easy to hit because `AddHeaderCell` does not assign an ID automatically and `TableHeaders` accepts only
strings, so the author must remember to call `ElementId(...)` first. The previous version of the authoring
documentation demonstrated exactly this mistake.

**Tasks**

- [x] Throw `PdfAccessibilityConformanceException` for unresolvable IDs under strict mode; warn otherwise.
- [x] Add `TableHeaders(params IStructureContext[])` and `References(params IStructureContext[])` overloads that
      auto-assign an ID to the target the way `Ref(StructureNode)` already does, removing the string indirection.
- [x] Suppress emission of attribute dictionaries that end up carrying only `/O`.

---

## Phase 3 — Correctness Guards

Cases where the library lets an author produce non-conformant output that a validator will reject. Lower severity
than Phase 2 only because veraPDF catches them.

### 6. Heading levels are not validated

**Severity: Medium · Effort: S**

`AddHeader(level, ...)` builds the tag by string concatenation, so `AddHeader(9, ...)` emits `/H9` — not a standard
type under PDF/UA-1 and not role-mapped. Skipped levels (`H1` → `H4`) also pass without comment. Both are
Matterhorn failures; verified that a document containing `H1`, `H4`, and `H9` saves cleanly with an empty RoleMap.

**Tasks**

- [ ] Reject levels outside 1–6 under PDF/UA-1 (or auto-`MapRole` them), and outside 1–*n* under PDF/UA-2.
- [ ] Add a strict-mode structure walk that flags descending heading jumps and a first heading that is not `H1`.
- [ ] Expose an unnumbered `AddHeader()` → `/H` for PDF/UA-2 authoring.

### 7. Artifacts cannot express `/Subtype`, `/BBox`, or `/Attached`

**Severity: Medium · Effort: S**

`BeginArtifact(PdfName? type)` writes only `/Type`. PDF/UA-1 §7.8 expects running headers and footers to carry
`/Subtype /Header` or `/Subtype /Footer`, and `/BBox` where the artifact's extent matters. Running heads and
footers are the single most common artifact in real documents, so this gap is hit by nearly every non-trivial
authoring job.

**Tasks**

- [ ] Overload `BeginArtifact(PdfName type, PdfName? subtype = null, PdfRect<double>? bbox = null, PdfArray? attached = null)`.
- [ ] Add `BeginHeaderArtifact` / `BeginFooterArtifact` / `BeginWatermarkArtifact` convenience methods.

### 8. Table conformance rules are only half-checked

**Severity: Medium · Effort: M**

Strict mode validates table *nesting* (`TR` under `Table`/`THead`/`TBody`/`TFoot`; `TH`/`TD` under `TR`) but not
the accessibility semantics: a `TH` with no `/Scope`, or a complex table whose `TD` cells carry no `/Headers`,
passes silently.

**Tasks**

- [ ] Require `/Scope` on every `TH` under strict mode unless the table is fully covered by `/Headers`.
- [ ] Detect irregular tables (spans, multiple header rows) and require `/Headers` on data cells in that case.
- [ ] Validate that `/RowSpan` / `/ColSpan` do not exceed the table's dimensions.
- [ ] Consider a `AddTable(rows, columns)` overload that constructs a regular grid with headers wired up, removing
      the manual ID bookkeeping entirely for the common case.

---

## Phase 4 — Ergonomics & Completeness

Nothing here blocks conformance, but each item either reduces the chance of authoring a wrong tree or removes an
escape-hatch workaround.

### 9. The `.Back()` chaining API is error-prone

**Severity: Medium · Effort: M**

Every `Add*` returns a context on the node just created, so `.Back()` counts must match nesting depth exactly. A
miscount produces a valid-looking but wrongly nested tree with no error. The library's own artifacts demonstrate
the hazard: `docs/StructuralBuilder.md` shipped an example that nested `Details` inside `Introduction`, and
`AccessibilityFixtureGenerator.cs` contains `statusHeader.Back().Back().Back().AddTableBody()`.

**Tasks**

- [ ] Add scoped builders — `section.AddTable("t", t => { t.AddRow(r => { ... }); })` or `using`-based nesting —
      so depth is expressed by block structure rather than by counting.
- [ ] Keep `.Back()` for compatibility; migrate the fixtures and docs to the scoped form.

### 10. PDF/UA-2 namespaces are only applied to the root

**Severity: Medium · Effort: S–M**

Only the root `Document` element receives `/NS`; descendants have none and fall back to the default PDF 1.7
namespace. Verified: `Document /NS=http://iso.org/pdf2/ssn`, then `Sect /NS=<none>`, `H1 /NS=<none>`.

This may be acceptable — the fallback is well-defined — but it has not been confirmed against veraPDF's `ua2`
profile, so the library's PDF/UA-2 support is currently unverified in this respect.

**Tasks**

- [ ] Run the existing UA-2 fixtures through `verapdf --flavour ua2` and record the result.
- [ ] If descendants require `/NS`, propagate the document namespace by default with a per-node override.
- [ ] Add MathML and SVG namespace declarations for `Formula` alternatives, plus `/AF` associated-file support
      (needed for PDF/UA-2 formula alternatives).

### 11. Missing structure-type helpers and layout attributes

**Severity: Low · Effort: S**

No helpers for `Art`, `Index`, `NonStruct`, `Private`, `Sub`, `Em`, `Strong`, `Ruby`/`RB`/`RT`/`RP`,
`Warichu`/`WT`/`WP`, or `Annot`. `AddLayoutAttributes` covers only `TextAlign`, `Width`, and `Height`.

Both have escape hatches — `AddElement(type, title)` and the public `StructureNode.Attributes` — so this is
convenience only.

**Tasks**

- [ ] Add the missing type helpers.
- [ ] Extend layout attributes with `/BBox`, `/Placement`, `/WritingMode`, `/SpaceBefore`, `/SpaceAfter`,
      `/StartIndent`, `/EndIndent`, `/BackgroundColor`, `/Color`.

### 12. `AddNote()` is unusable under strict mode

**Severity: Low · Effort: XS**

`AddNote()` emits `/Note`, which `PdfDocument.Accessibility.cs:318` unconditionally rejects in strict mode. The API
offers a method that can never be called in the default configuration.

**Tasks**

- [ ] Mark `AddNote()` `[Obsolete]` pointing at `AddFENote()`, or scope the strict-mode rejection to PDF/UA-2 only
      (`Note` is valid under PDF/UA-1 with an `/ID`).

### 13. `/ParentTreeNextKey` is never written

**Severity: Low · Effort: XS**

`StructTreeRoot` is emitted with only `/Type`, `/K`, and `/ParentTree`. `/ParentTreeNextKey` is optional per
ISO 32000-1 but some consumers expect it, and any tool appending to the tree needs it.

**Tasks**

- [ ] Emit `/ParentTreeNextKey` as `max(key) + 1` in `BuildParentTree`.

---

## Phase 5 — Scope Decision

### 14. Appending tagged content to an already-tagged document

**Severity: Scope decision · Effort: L**

If the input PDF already has a `StructTreeRoot`, every authoring entry point throws:

```
PdfAccessibilitySetupException: Structure only supports new or currently untagged documents.
Editing a document with an existing StructTreeRoot is not supported.
```

*Editing* existing structure is deliberately out of scope, and that remains a reasonable boundary. But **appending**
— adding a new tagged page to a document that is already tagged — is a different operation, and it is currently
impossible. It does not require understanding or mutating the existing tree, only reading its `ParentTree` high-water
mark and grafting new siblings under the existing root.

This is a product decision, not a defect. Recorded here so it is chosen rather than inherited.

**If accepted, tasks**

- [ ] Read the existing `StructTreeRoot`, `ParentTree`, `RoleMap`, `ClassMap`, and `IDTree` into `StructureRoot`
      as opaque, non-editable state.
- [ ] Seed `AllocateStructParentIndex` from the existing `/ParentTreeNextKey` (see finding
      [13](#13-parenttreenextkey-is-never-written) — needed here).
- [ ] Allow new nodes to attach under the existing root while leaving existing subtrees byte-identical.
- [ ] Merge the new `ParentTree` entries into the existing number tree rather than replacing it.
- [ ] Reject the case where the existing tree uses features the library cannot round-trip.

---

## Suggested Sequencing

| Phase | Contents | Rationale |
| --- | --- | --- |
| **0** | Documentation | ✅ Done. Was actively misleading; every example failed. |
| **1** | Findings 1–2 | ✅ Done. Silent-corruption fixes and focused fixtures landed together. |
| **2** | Findings 3–5 | Conformance blockers needing new API. Ship together so the surface settles once. |
| **3** | Findings 6–8 | Guards that turn validator findings into build-time errors. |
| **4** | Findings 9–13 | Ergonomics and completeness; independent, can be picked up opportunistically. |
| **5** | Finding 14 | Decide before committing engineering time. |

Phase 2 is what remains between the current library and reliably veraPDF-clean output for common documents.

## Validation Baseline

The fixture corpus in `AccessibilityFixtureGenerator` covers document setup, headings, lists, tables, links,
figures, XObjects, forms, artifacts, navigation, Unicode text, and multi-page flow, in both UA-1 and UA-2 — a good
foundation. It is not currently run through veraPDF in CI, which is why findings [3](#3-annotations-other-than-linkwidget-cannot-be-tagged),
[4](#4-generated-form-widgets-are-not-pdfua-conformant), and [10](#10-pdfua-2-namespaces-are-only-applied-to-the-root)
went unnoticed: the internal integrity checks pass on files an external validator would reject.

- [ ] Wire `verapdf --flavour ua1|ua2` over the generated fixtures into CI, with a pinned veraPDF version.
- [ ] Treat new veraPDF findings as build failures; record any accepted exception explicitly.
- [x] Add focused fixtures for non-Latin-1 alternate text and multi-pass page writing.
- [x] Add fixtures for non-link annotations and every named form field type.
- [ ] Add fixtures for running
      header/footer artifacts.
