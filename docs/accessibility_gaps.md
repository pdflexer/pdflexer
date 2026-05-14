# Accessibility Gap Analysis: StructuralBuilder

Review of `StructuralBuilder` and related plumbing for producing / modifying PDFs
that meet PDF/UA-1 (ISO 14289-1) — the technical basis for U.S. Section 508, ADA,
and the PDF pieces of WCAG 2.x conformance.

Files reviewed:

- `src/PdfLexer/DOM/StructuralBuilder.cs`
- `src/PdfLexer/DOM/StructureNode.cs`
- `src/PdfLexer/Writing/StructuralSerializer.cs`
- `src/PdfLexer/Writing/PageWriter.cs`
- `src/PdfLexer/PdfDocument.Saving.cs`
- `src/PdfLexer/PdfDocument.cs`
- `src/PdfLexer/Parsers/OutlineParser.cs`
- `src/PdfLexer/Serializers/WritingUtil.cs`
- `src/PdfLexer/DOM/XObjImage.cs`
- `src/PdfLexer/DOM/XObjForm.cs`
- `src/PdfLexer/Fonts/Standard14Font.Writable.cs`
- `src/PdfLexer/Fonts/TrueTypeSimpleWritableFont.cs`
- `src/PdfLexer/Fonts/TrueTypeWritableFont.cs`

## Summary

`StructuralBuilder` can emit a Tagged-PDF `StructTreeRoot` + `ParentTree` with
MCID round-tripping over the standard structure types. That is a solid skeleton,
but documents produced today do **not** meet PDF/UA-1, and core authoring and
remediation primitives are missing. On top of the previously identified gaps,
the current save path also drops named-destination navigation, the emitted root
`StructElem` is missing its parent link, XObject structure plumbing is absent,
and newly written text is not guaranteed to carry a reliable Unicode map for
assistive technology.

---

## 1. Correctness bugs — currently emitted tags are non-conformant

### 1.1 `/Alt` key is serialized as `/ALT`

- `src/PdfLexer/Writing/StructuralSerializer.cs:53` writes `dict[PdfName.ALT] = ...`.
- `src/PdfLexer/PdfName.cs:99` defines `PdfName.ALT = new("ALT", false)`.
- PDF 32000-1:2008 §14.9.3 specifies `/Alt` (mixed case).
- **Impact:** every `Figure`, `Formula`, and `Link` written today has invisible
  alternate text. Assistive technology ignores `/ALT`, so the document fails
  WCAG 1.1.1 and PDF/UA §7.3 on alternative descriptions.
- **Fix:** add `PdfName.Alt = new("Alt", false)` and use it in the serializer.
  `grep` shows `PdfName.ALT` is referenced only at this one site.

### 1.2 `MarkInfo` is minimal

- `src/PdfLexer/PdfDocument.Saving.cs:105` sets only `/Marked = true`.
- Missing `/Suspects false` (PDF 2.0) and an explicit `/UserProperties` value
  when attribute dictionaries include user properties.
- **Impact:** veraPDF / PAC warn on missing `Suspects` in PDF 2.0 output.

### 1.3 Existing `/StructTreeRoot` is dropped on save

- `src/PdfLexer/PdfDocument.Saving.cs:65` removes `StructTreeRoot` from the
  catalog unconditionally, then only re-emits one if `_structure` was
  populated.
- **Impact:** opening a tagged PDF and saving without rebuilding
  `doc.Structure` deletes all tags. The library is currently unsafe for the
  dominant 508 use case (remediating existing documents).

### 1.4 Top-level `StructElem` is missing its `/P` parent link

- `src/PdfLexer/Writing/StructuralSerializer.cs:125-143` attaches the root
  structure element under `StructTreeRoot[/K]`, but never sets that element's
  `/P` to the `StructTreeRoot`.
- Child elements do get `/Parent` set (`StructuralSerializer.cs:77-83`), so the
  omission is limited to the top of the tree.
- **Impact:** every emitted structure tree has an orphaned top-level
  `StructElem`. Validators and remediation tools expect the root element to
  point back to `StructTreeRoot`.

---

## 2. Missing accessibility primitives on the node model

`StructureNode` today carries only `Type`, `ID`, `Title`, `AlternateText`,
`Language`, generic `Attributes`, and children.

### 2.1 `/ActualText`

No field, no serializer path. Required anywhere the glyph stream and logical
reading text diverge: ligatures, soft hyphens, decorative drop caps, images of
text. Without it, screen readers mispronounce ligatures and hyphens-across-
lines. PDF/UA §7.3 effectively mandates it.

### 2.2 `/E` (Expansion text)

No field. Required for abbreviations and acronyms so AT can expand them
(e.g. "W3C" → "World Wide Web Consortium").

### 2.3 `/Alt` on generic elements

- `StructuralBuilder.cs:83-105` only accepts `altText` on `AddFigure`,
  `AddFormula`, `AddLink`.
- `/Alt` is legal on any StructElem; `Span` and `Div` need it regularly.
- **Fix:** expose `.SetAlt(string)`, `.SetActualText(string)`,
  `.SetExpansion(string)` on `IStructureContext` itself rather than per-type
  overloads.

### 2.4 Table attributes beyond rowspan / colspan

`AddCell` (`StructuralBuilder.cs:45`) emits only `/RowSpan` / `/ColSpan`.
Missing:

- **`/Scope`** (`Row` / `Column` / `Both`) on `TH` — required for simple
  tables.
- **`/Headers`** (array of `/ID` references) on `TD` — required for irregular
  / complex tables where scope is insufficient.
- **`/Summary`** on `Table`.

Without these, complex data tables cannot be made conformant.

### 2.5 List attributes

No `/ListNumbering` on `L` (`Decimal`, `UpperAlpha`, `LowerAlpha`,
`UpperRoman`, `LowerRoman`, `Circle`, `Disc`, `Square`, `Ordered`,
`Unordered`, `None`). PDF/UA requires this to distinguish ordered vs.
unordered lists.

### 2.6 Layout attributes

`AddLayoutAttributes` (`StructuralBuilder.cs:107`) exposes only `TextAlign`,
`Width`, `Height`. Missing:

- `/BBox` (required on `Figure` when `/Placement ≠ Inline`)
- `/Placement`, `/WritingMode`
- `/SpaceBefore`, `/SpaceAfter`, `/StartIndent`, `/EndIndent`, `/TextIndent`
- `/LineHeight`, `/BaselineShift`
- `/TextDecorationType`, `/TextDecorationColor`, `/TextDecorationThickness`
- `/ColumnCount`, `/ColumnWidths`, `/ColumnGap`
- `/BorderStyle`, `/BorderThickness`, `/Color`, `/BackgroundColor`, `/Padding`

### 2.7 RoleMap / ClassMap

- `StructuralSerializer.cs:122` emits `StructTreeRoot` with no `/RoleMap` or
  `/ClassMap`.
- `PageWriter.BeginMarkedContent` (`PageWriter.cs:37`) uses `node.Type`
  verbatim as the BDC tag.
- **Impact:** users cannot author custom role names that map to standard
  types. Custom tag names without a role map fail PDF/UA "only standard
  structure types" rule.

### 2.8 PDF 2.0 namespaces (`/NS`, `/Namespaces`)

Entirely absent. PDF/UA-2 (ISO 14289-2:2024) requires the default PDF 2.0
namespace declaration on every StructElem, plus MathML / SVG namespaces for
embedded math or vector content.

### 2.9 `/ID` + `/IDTree`

- `StructureNode.ID` is serialized onto each StructElem
  (`StructuralSerializer.cs:43`) but **no `/IDTree`** is built on the root.
- **Impact:** cross-references via `/Ref`, `TOCI → Reference`, and
  `/Headers` cannot be resolved.

### 2.10 `/Ref`

No way to author a structure reference from one node (e.g. `Reference`,
`Note`) to another node by ID.

### 2.11 `AddLink` does not attach the annotation

- The `/Link` StructElem in the tree must have both content MCIDs and an
  `OBJR` kid pointing at the Link widget annotation (PDF 32000 §14.7.5.4).
- `StructuralSerializer.CollectNodes` has no OBJR emission path, and there is
  no annotation API.
- **Impact:** tagged links are incomplete; AT cannot navigate them.

### 2.12 Form fields

No `/Form` structure type helper, no `PrintField` attribute, no widget
annotation `/TU` (tool-tip) authoring, no `/TabOrder` / page `/Tabs = /S`
application. Accessible forms cannot currently be built.

### 2.13 Artifact categorization

- `PageWriter.BeginArtifact` (`PageWriter.cs:58`) accepts a `Type` name but
  not a `Subtype` (`Header` / `Footer` / `Watermark` / `PageNum`), `/BBox`,
  or `/Attached` array.
- PDF/UA requires pagination artifacts (running headers, page numbers) to be
  subtyped and optionally attached to a page edge.
- The API is also asymmetric: there is no `EndArtifact`, only
  `EndMarkedContent`.

### 2.14 Non-struct / Private containers

No `/NonStruct` or `/Private` element helpers and no way to mark decorative
content that exists in the structure tree but should be skipped by AT.

### 2.15 Tagged XObjects / reusable content

- `ContentWriter.Image(...)` and `ContentWriter.Form(...)` only paint XObjects;
  there is no API to associate an `XObjImage` / `XObjForm` with a structure
  element.
- `src/PdfLexer/DOM/XObjImage.cs` and `src/PdfLexer/DOM/XObjForm.cs` mention
  `StructParent` / `StructParents` only as comments; no authoring surface
  exists.
- **Impact:** common accessible content patterns such as figure XObjects,
  reusable page fragments, and structurally significant form XObjects cannot be
  linked into the structure tree in a conformant way.

---

## 3. Document-level plumbing — required for PDF/UA, never set

None of the following are exposed or defaulted:

| Requirement | Spec source | Status |
| --- | --- | --- |
| `Catalog[/Lang]` (document default language) | PDF/UA 7.2 | Not exposed. `StructureNode.Language` only sets `/Lang` per-element. |
| `Catalog[/ViewerPreferences][/DisplayDocTitle] = true` | PDF/UA 7.1 | Not set. |
| `Info[/Title]` | PDF/UA 7.1 | No typed convenience API. Low-level `Trailer[Info]` mutation exists, but nothing PDF/UA-focused is surfaced on `PdfDocument`. |
| XMP metadata stream with `pdfuaid:part = 1` (or `2`) | PDF/UA-1 §5 | No XMP writer surfaced on `PdfDocument`. |
| Page `/Tabs = /S` | PDF/UA 7.18.3 | Never set; page-tree builder at `PdfDocument.Saving.cs:122` is unaware of tagging. |
| PDF version gate | PDF/UA-1 requires ≥ 1.7 | Writer defaults to 1.7 which is fine; no explicit check or version bump when PDF 2.0 namespace is used. |

### 3.1 Written text does not always carry Unicode mapping

- `src/PdfLexer/Fonts/TrueTypeWritableFont.cs:182-187` emits a `ToUnicode`
  CMap for Type 0 writable fonts.
- `src/PdfLexer/Fonts/Standard14Font.Writable.cs` and
  `src/PdfLexer/Fonts/TrueTypeSimpleWritableFont.cs` build writable font
  dictionaries without `ToUnicode`.
- **Impact:** a document can be visually correct and tagged, yet still extract
  poorly in assistive technology if the actual text layer is written with a
  simple font lacking a Unicode map.
- **Fix:** prefer Unicode-capable Type 0 output for accessibility authoring, or
  synthesize `ToUnicode` for simple-font output paths.

---

## 4. Editing / remediation path — essentially missing

Section 508 work is usually *remediation*: open an existing PDF, add or fix
tags. Today:

- **No `StructTreeRoot` parser.** Grep for `StructureParser`,
  `StructTreeParser`, `ParseStructure`, `ReadStructure` returns nothing.
  Creation only, not read / modify.
- **`doc.Structure` always starts fresh.** `PdfDocument.Structure` lazily
  instantiates a new `StructuralBuilder`; it is not backed by the existing
  `StructTreeRoot`. Accessing it on a tagged PDF creates a replacement tree,
  not an editable view of the current one.
- **Destructive save.** `PdfDocument.Saving.cs:65` unconditionally deletes
  `/StructTreeRoot` and only re-emits one if `_structure` was populated.
- **Named destinations are dropped.** `PdfDocument.Saving.cs:64` removes
  `Catalog[/Names]`, and `Serializers/WritingUtil.cs:23-37` deletes link
  annotations whose `/Dest` is a name or string.
- **Impact:** internal navigation structures such as TOCs, cross-references,
  and named-destination links are not safe to round-trip during remediation.
- **MCID collision on append.** `PageWriter.CurrentMCID` starts at 0 per
  writer instance (`PageWriter.cs:25`). Appending tagged content to a page
  that already carries MCIDs will collide. No scan of existing BDC / MCID on
  the page to seed the counter, and no renumber helper.
- **No auto-tagging bridge.** The text-layout engine already produces
  `StructuredParagraph` / `StructuredLine` / `StructuredWord` — exactly the
  semantic grouping a tagger wants — but nothing wires that output to
  `StructuralBuilder`.

---

## 5. Validation

`PdfLexer.Validation` has no PDF/UA profile. A minimum useful checker would
verify:

- All real content is inside a structure MCID or an `/Artifact` sequence.
- Every `Figure`, `Formula`, and `Link` has `/Alt` or `/ActualText`.
- Every page has `/StructParents` and a matching entry in `ParentTree`.
- `StructTreeRoot` has `/K`, `/ParentTree`, and `/ParentTreeNextKey`.
- The top-level `StructElem` under `StructTreeRoot[/K]` points back to the
  root with `/P`.
- `MarkInfo/Marked = true`.
- `Catalog/Lang`, `ViewerPreferences/DisplayDocTitle`, `Info/Title` are set.
- XMP `pdfuaid:part` is present.
- Only standard-namespace structure types are used unless role-mapped.
- Every `/ID` is unique and every `/Ref` / `/Headers` entry resolves via
  `/IDTree`.
- Named destinations and internal link annotations survive save / round-trip.
- Written fonts used for real text expose Unicode mapping (or are emitted as
  Type 0 fonts with `ToUnicode`).
- Structurally significant XObjects carry `StructParent` / `StructParents` and
  resolve through the parent tree.

---

## Suggested priority

1. **Fix `/ALT` → `/Alt`.** One-line fix that unblocks every figure /
   formula / link currently in the wild.
2. **Set `/P` on the top-level `StructElem`.** Small correctness fix, easy to
   regression-test, and necessary for a valid structure hierarchy.
3. **Add `ActualText` and `ExpansionText`** to `StructureNode` and to the
   serializer.
4. **Stop dropping existing `/StructTreeRoot` on save** when `_structure` is
   null, and avoid treating `doc.Structure` as a fresh replacement tree when
   the document was opened from an already tagged file.
5. **Preserve `Catalog[/Names]` and named-destination link annotations** so
   internal navigation survives remediation saves.
6. **Add a document-level `MarkAsPdfUA1(title, lang)` convenience** that
   atomically sets `Catalog/Lang`, `Info/Title`, `ViewerPreferences/
   DisplayDocTitle`, XMP `pdfuaid:part`, `MarkInfo/Suspects`, and page
   `/Tabs = /S`.
7. **Add `/Scope`, `/Headers`, `/Summary`, `/ListNumbering`** attribute
   helpers so complex tables and ordered lists can be authored.
8. **Prefer Type 0 / Unicode output for accessibility authoring** or emit
   `ToUnicode` for simple writable-font paths so AT can extract real text
   reliably.
9. **Add `OBJR` emission and a `LinkAnnotation` API** so `AddLink` produces
   a conformant tagged link.
10. **Add `StructParent` / `StructParents` plumbing for XObjects** so figures,
    forms, and reusable content can participate in the structure tree.
11. **Write a `StructTreeRoot` parser / round-tripper** so remediation works
    at all.
12. **Build a `PdfUAValidator` profile** alongside the existing validation
    infrastructure.

Item 1 is a must-ship fix regardless of the broader roadmap — without it,
every alt-text string currently written by `StructuralBuilder` is invisible
to assistive technology.
