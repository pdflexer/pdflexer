# PDF/UA-2 agent guide (concise implementation spec)

This guide condenses **ISO 14289-2:2024 (PDF/UA-2)** into rules an AI agent can follow to generate conforming PDFs.
It is a **generation checklist**, not a replacement for ISO 32000-2, ISO/TS 32005, or full validation.

## 0. Conformance target

Generate a file that:
- conforms to **ISO 32000-2 (PDF 2.0)**;
- conforms to **ISO/TS 32005** for structure namespace inclusion / containment;
- meets all PDF/UA-2 requirements below;
- avoids deprecated PDF 2.0 features where possible;
- does **not** assume PDF/UA alone makes content accessible (plain language, contrast, etc. are out of scope).

## 1. Required document-level items

### 1.1 Metadata / identification
- Include a document **Metadata** stream.
- In XMP, include PDF/UA identification schema:
  - `pdfuaid:part = 2`
  - `pdfuaid:rev = 2024`
- Include `dc:title` matching the document title.
- Set catalog `ViewerPreferences/DisplayDocTitle = true`.
- Set catalog `Lang` to a **non-empty specific language**.

### 1.2 Structure tree root
- The structure tree root must have **exactly one child**: `Document`.
- That `Document` element must be in the **PDF 2.0 namespace**.

## 2. Core semantic rule

Treat all authored, meaningful content as either:
- **real content** -> must be tagged with semantically appropriate structure elements, or
- **artifact** -> must be explicitly marked as artifact.

Never leave non-annotation page content merely “untagged”.

## 3. Logical structure rules

### 3.1 Real content
- Every real content item must be enclosed by semantically appropriate structure elements.
- Tag semantics based on meaning, **not layout** or authoring-tool output.
- Preserve **semantic logical reading order** in the structure tree.
- Do **not** split one semantic object into multiple identical containers just because it spans pages/columns.
  - One paragraph spanning pages -> one `P`.
  - One table spanning pages -> one `Table`.

### 3.2 Namespaces / role mapping
- Structure types must belong to, or role-map to, one of:
  - PDF 1.7 namespace
  - PDF 2.0 namespace
  - MathML namespace
- Custom structure types are allowed only if they role-map to standard types and then obey all requirements of the mapped type.
- Do not role-map a type to another type **within the same explicit namespace**.

### 3.3 Attributes
- Add structure attributes when they carry semantic meaning not otherwise expressed.
- Do **not** add attributes for properties that do not exist.
- Use layout attributes when layout/styling conveys meaning.
- ARIA / DPUB-ARIA roles may be used only when semantically appropriate and must not contradict the base structure type.

## 4. Artifact rules

- Any non-real content except annotations must be explicitly identified as artifact.
- Use either:
  - artifact marked-content sequence, or
  - `Artifact` structure element.
- If artifact properties/attributes are semantically applicable, include them.
- Use an `Artifact` structure element when the artifact is meaningful only relative to nearby real content.
- If an artifact is intended to be consumed as a unit, group it as a single artifact.
- If artifact content has internal semantic order, preserve that order.

Typical artifacts:
- decorative graphics
- running heads/footers when non-meaningful
- page numbers when treated as artifact
- TOC dot leaders
- printer’s marks

## 5. Text requirements

### 5.1 Programmatic text
- Text content must be programmatically determinable.
- Human-readable text strings must not use Unicode Private Use Area (PUA).
- In content streams, PUA values may be used only when no valid Unicode value exists.

### 5.2 Unicode mapping
- Provide a `ToUnicode` CMap for every font unless it qualifies for one of the standard exceptions in PDF/UA-2 / PDF 2.0.
- If a `ToUnicode` CMap is present, it must not map to `0`, `U+FEFF`, or `U+FFFE`.
- Never reference the `.notdef` glyph from text-showing operators.

### 5.3 ActualText / Alt
Use `ActualText` when the intended textual result differs from what text extraction would otherwise produce.
Required patterns:
- non-text graphics intended to be consumed as text;
- text encoded via vector drawing / image-like means;
- ruby where omitted characters must be restored;
- PUA-backed content when needed to convey the true Unicode sequence.

Rules:
- If real content maps to Unicode PUA, `ActualText` or `Alt` is required.
- `ActualText` / `Alt` themselves must not use PUA.
- Any structure element carrying `ActualText` must be, or be enclosed by, a semantically appropriate structure element.

### 5.4 Natural language
- Declare the document default language in catalog `Lang`.
- Declare all language changes in content.
- Declare language changes inside text strings too (for example inside alt text).
- Language declarations must resolve to a **specific language**, not an empty or ambiguous value.

## 6. Font rules

For all rendered fonts:
- embed the font program;
- use only fonts legally embeddable for unlimited universal rendering;
- ensure all referenced glyphs are present in the embedded font;
- keep font dictionary metrics consistent with embedded font metrics;
- embed required CMaps;
- ensure TrueType / CIDFont / Type 0 rules are satisfied per PDF 2.0;
- ensure glyph widths and vertical metrics are consistent;
- ensure character codes map to glyphs without processor-specific nonstandard behavior.

Practical rule for agents:
- **Always embed fonts fully or as valid subsets**.
- **Always provide Unicode mapping**.
- **Never depend on system fonts or fallback behavior**.

## 7. Real content without native text semantics

If meaningful content is not naturally represented as text objects:
- use `Figure` or `Formula` as appropriate;
- if it is intended to be consumed as text, provide `ActualText`.

## 8. Structure-type rules the agent should follow

### 8.1 Headings / title
- Use only numbered heading tags: `H1`, `H2`, `H3`, ...
- Do **not** use `H`.
- If heading level is evident, the tag must match that level.
- Document titles in content must use `Title`, **not** a heading tag.

### 8.2 Paragraphs
- Use `P` for actual semantic paragraphs.
- Do not use `P` for text fragments that are not paragraphs.
- If multiple paragraphs exist under one parent, each gets its own `P`.

### 8.3 Sections / parts / articles
- Use `Sect` for thematic sections, usually with an internal heading.
- Use `Part` for groupings unrelated to heading hierarchy.
- Use `Art` for self-contained articles inside larger documents.
- If a title applies to an article, include `Title` inside that `Art`.

### 8.4 Quotes
- `BlockQuote` for block-level quoted material.
- `Quote` for inline quoted material.
- Do not use quote tags merely because quotation marks appear.

### 8.5 TOC
- Use `TOC` / `TOCI` for tables of contents.
- Each `TOCI` must identify its target using `Ref` directly or on a child such as `Reference`.
- TOC leaders are artifacts.

### 8.6 Aside / notes
- Use `Aside` for content outside the main flow.
- If related to nearby content, attach it under the deepest relevant parent.
- Use `FENote` for footnotes / endnotes.
- Do **not** use `Note`.
- References to footnotes/endnotes must use `Ref`; interactive ones should use a link annotation with a structure destination.
- `FENote` may include `NoteType = Footnote | Endnote | None` under owner `FENote`.

### 8.7 Labels
- Real labels for other real content must be tagged as `Lbl`.
- `Lbl` should live under the closest semantic ancestor that groups the label and the thing labeled.

### 8.8 Span / emphasis
- Use `Span` only when inline attributes/properties are needed and no better inline semantic element applies.
- Use `Em` / `Strong` only for emphasis, not for arbitrary styling or keyword marking.

### 8.9 Links / references
- Enclose linked content and its link annotation in `Link` or `Reference`.
- Prefer `Link` for external targets.
- Prefer `Reference` for intra-document targets.
- Distinct targets -> distinct `Link` / `Reference` elements.
- Multiple annotations may share one `Link` / `Reference` only if they are semantically one link to the same target.
- Linked content must be contiguous in logical reading order.
- `Alt` may describe both content and link when needed.

### 8.10 Lists
- Use `L`, `LI`, `Lbl`, `LBody` correctly.
- If list labels are real content, tag them as `Lbl`.
- If `Lbl` is used, set `ListNumbering` on the `L` and do not use `None` when a numbering scheme exists.
- Any non-label real content in `LI` must be inside `LBody`.
- Continued lists must set `ContinuedList = true`.
- If the previous list segment is present, set `ContinuedFrom`.
- If list segments are related, use `Ref` to reference all segments.

### 8.11 Tables
- Tables must be **regular** after applying `RowSpan` / `ColSpan`.
- `THead`, `TBody`, `TFoot` row groups must also be regular.
- Where headers exist, provide enough semantics to determine which `TH` applies to which cells.
- If default header inference is not enough, explicitly set `Scope`.
- If `Scope` still cannot fully describe the relationships, use `Headers` for **all cells with headers**.
- Using `Headers` does **not** remove the need for `Scope` where applicable.

### 8.12 Captions
- All captioning content must be inside `Caption`.
- `Caption` must be a child of the structure element for the thing it captions.
- Put it first if read before the content; last if read after.

### 8.13 Figures
- A `Figure` must enclose all content used to produce its final appearance, including relevant background.
- A `Figure` may have semantic substructure.
- Every `Figure` must have at least one of:
  - `Alt`, or
  - `ActualText`
- If a `Figure` uses `ActualText`, it must sit inside the semantically appropriate block-level structure.

### 8.14 Formulae
- Mathematical expressions must be represented as presentation MathML:
  - via MathML structure types, or
  - via associated file, or
  - both
- MathML `math` may appear only as child of `Formula`.
- If associated file is used for math, associate it to the `Formula` and set `AFRelationship = Supplement`.
- Non-math scientific formulae must be in `Formula` and have `Alt` or `ActualText`.

### 8.15 Index / bibliography / code
- Use one `Index` per logical index.
- References inside an index should use `Reference` and `Ref` as needed.
- Bibliography entries should use `BibEntry`; they should include `Ref` to places that cite them.
- A bibliography section may use ARIA role `doc-bibliography`.
- Code fragments must be inside `Code`.
- If code is meant to be read as text, a textual representation must exist.

### 8.16 Ruby / warichu
- Use `Ruby` only for true ruby semantics.
- Standard ruby: `Ruby > RB + RT`.
- Parenthetical ruby form: `Ruby > RB + RP + RT + RP`.
- Use `ActualText` on `RT` when omitted ruby characters need restoring.
- Use `Warichu > WP + WT + WP` only for actual warichu.

## 9. Destinations and navigation

### 9.1 Intra-document destinations
- Any destination targeting the same document must be a **structure destination**.
- Where content actionably points to other content, use one or both:
  - link annotation using structure destination;
  - `Ref` from source structure element to target structure element.

### 9.2 Outline / page labels
- Longer documents should include an outline/bookmarks.
- Outline items should use actions.
- If page labels are present, they must match user-perceived numbering.
- If displayed page number != page index + 1, page labels are required.

### 9.3 Article threads
- If article threads exist, they should reflect logical content order.

## 10. Annotation rules

### 10.1 General
- Annotations may be present.
- Deprecated annotation types in PDF 2.0 must not be used.
- Every page with annotations must have `Tabs` set to `A`, `W`, or `S`.

### 10.2 Structure / semantics
- Annotation semantics come from subtype.
- Markup annotations must be enclosed in `Annot`.
- Popup annotations must **not** appear in the structure tree.
- Printer’s mark annotations must be artifacts.
- Zero-size widget annotations must be artifacts.

### 10.3 Alternative descriptions
- If an annotation needs an alternative description and `Contents` is the correct mechanism, use `Contents`.
- If enclosing structure element also has `Alt` for that annotation, `Alt` and `Contents` must be identical.

### 10.4 Specific annotation rules
- **Link**: should include `Contents`; linked content must be contiguous.
- **Text / sticky note**: structure placement + `Contents` should give enough context.
- **FreeText**: follow markup rules; callouts should align with callout origin in reading order.
- **Line / shape / polygon / polyline**: follow markup rules.
- **Text markup**: use separate annotations for separate logical text units when non-contiguous.
- **Rubber stamp**: if `Name` is not descriptive enough, provide `Contents`.
- **Ink**: must provide `Contents` describing author intent.
- **File attachment annotation**: referenced file spec must include `AFRelationship`.
- **Screen**: must include `Contents`.
- **Sound / movie**: forbidden.
- **Trap network**: forbidden.
- **3D / RichMedia**: must include alternate descriptions in `Contents`.
- **Redaction annotation**: follow markup rules; one logical redaction should be one annotation where possible.

### 10.5 Annotation placement
- Place annotations as close as possible to the content they annotate:
  - child or sibling of the relevant content element, or
  - child of `Annot`, `Link`, or `Reference`
- Use `Ref` where helpful to strengthen association.

## 11. Form rules

### 11.1 General
- Every non-artifact widget annotation must be enclosed by a `Form` structure element.
- One `Form` structure element may enclose **at most one widget annotation**.
- XFA forms are forbidden.

### 11.2 Context requirements
Users must be able to understand each interactive field from a combination of:
- surrounding real content;
- structure grouping;
- labels (`Lbl`);
- field `TU` entry;
- widget `Contents` entry.

### 11.3 Labels and descriptions
- If real content labels a widget, that label must be in `Lbl` and be a **direct child** of the `Form` that also contains the widget reference.
- If real content labels a group of widgets, those `Lbl` elements must be in the common parent structure that contains the related `Form` elements.
- If label is missing or insufficient, widget `Contents` is required.
- Widget `Contents` should add information; it should not merely duplicate the label unless necessary.
- If widget has additional actions (`AA`), `Contents` must describe their intent.
- `TU` may supplement context, but often is not sufficient for multi-widget fields.

### 11.4 Field-type rules
- **Buttons**: if `Contents` is present, it must reflect `CA` intent.
- **Push buttons**: if `Contents` is present, it must reflect `CA`, `RC`, `AC`, `I`, `RI`, `IX` intent; if `TU` exists, it must reflect `RC` intent.
- **Checkboxes / radio buttons**: both field and widgets must satisfy context rules.
- **Text fields**: if `RV` exists, `V` must exist too, and both must be textually equivalent.
- **Choice fields**: displayed choice text must sufficiently convey intent.
- **Signature fields**:
  - if location matters legally, widget is real content and must be in structure tree;
  - otherwise widget may be artifact if it meets artifact criteria;
  - if appearance includes a graphic, provide alt text for that graphic;
  - visual appearance must not contradict signature metadata.

### 11.5 Non-interactive forms
- Non-interactive form-like content representing a field must be in `Form`.
- Add appropriate `PrintField` attributes.
- Use `Lbl` for field labels and group labels following the same general grouping logic as interactive forms.

## 12. Optional content

- If optional content configurations exist, every configuration dictionary must have a non-empty `Name`.
- `AS` must not appear in optional content configuration dictionaries.
- All font requirements still apply even to optional content that may not render in a given view.

## 13. Embedded files / associated files

- Every file specification in `EmbeddedFiles` must have `Desc` with enough context to explain intent.
- If an embedded file is necessary to understand the document, it must itself be accessible by an objectively verifiable standard.
- If such an embedded file is a PDF, it must conform to the ISO 14289 series.
- If a necessary embedded file is not a PDF, include an appropriate PDF Declaration in its metadata.
- For supplementary / alternative representations, prefer embedded associated files, associate them to the relevant structure element, and include `AFRelationship` and embedded-file `Subtype`.

## 14. Recommended generation defaults for agents

Use these defaults unless a stronger semantic option is clearly correct:
- set catalog `Lang`
- set document title in both XMP and visible content when appropriate
- build full structure tree from the start
- tag everything meaningful
- explicitly artifact everything decorative / mechanical
- embed all fonts
- add `ToUnicode`
- use numbered headings only
- use `Ref` for footnotes, TOC items, cross-references, bibliography/index links
- provide `Alt` for figures and rich media
- provide `Contents` for links, widgets needing context, ink annotations, screen annotations
- use structure destinations for internal navigation
- include page labels when printed numbering differs from page index

## 15. Hard fail conditions

Do **not** emit a “PDF/UA-2 conforming” claim if any of these are true:
- no single PDF 2.0 `Document` child under structure tree root
- missing `pdfuaid:part=2` / `pdfuaid:rev=2024`
- missing `dc:title`
- missing catalog `Lang`
- missing `DisplayDocTitle=true`
- meaningful content left untagged instead of tagged or artifacted
- semantic reading order is wrong
- use of `H` instead of `H1...Hn`
- use of `Note` instead of `FENote`
- internal links use non-structure destinations
- required alt / actual text / contents missing
- fonts not embedded for rendered text
- Unicode mapping missing where required
- `.notdef` glyph referenced
- deprecated forbidden features present (for example sound/movie annotations, XFA forms, trap network annotations)

## 16. Minimal validation checklist

Before declaring success, verify:
1. PDF 2.0 syntax valid.
2. ISO/TS 32005 containment / role mapping valid.
3. Structure tree complete and logically ordered.
4. All real content tagged; all other non-annotation content artifacted.
5. Headings/title/TOC/lists/tables/figures/forms use correct structure types.
6. Footnotes/endnotes use `FENote` + `Ref`.
7. Internal navigation uses structure destinations.
8. Required annotation and form descriptions exist.
9. Fonts embedded, Unicode mapping present, languages declared.
10. Metadata and PDF/UA identifiers present.

## 17. Practical note

PDF/UA-2 is mostly about **correct semantics, explicit structure, machine-readable text equivalents, and robust PDF 2.0 encoding**. If the agent is unsure, prefer:
- more explicit semantics,
- more explicit language declarations,
- more explicit artifact marking,
- more explicit references / destinations,
- more explicit textual alternatives.
