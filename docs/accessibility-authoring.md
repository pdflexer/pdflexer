# Accessible Authoring

`PdfLexer` provides high-level APIs for creating and authoring Tagged PDFs compliant with **PDF/UA-1 (ISO 14289-1)** and **PDF/UA-2 (ISO 14289-2)**.

Supported workflows:

1. **New Tagged Documents**: Creating accessible PDFs from scratch.
2. **Untagged Document Remediation**: Opening an existing untagged PDF and applying structure tags manually via `StructuralBuilder` or declaratively via `RemediationSession`.

> [!IMPORTANT]
> Editing or modifying pre-existing `StructTreeRoot` structure trees in already-tagged documents is not supported. If a document is already tagged, `doc.Structure`, `ApplyAccessibilitySetup(...)`, and `BeginRemediation(...)` all throw `PdfAccessibilitySetupException`. This includes *appending* new tagged content to a document that is already tagged.

Before writing any code, read [Before You Start](#before-you-start) — the font requirement in particular will reject otherwise-correct code at save time. [Known Limitations](#known-limitations) lists the cases the library does not yet handle; check it before shipping.

---

## Before You Start

### Fonts must be embedded

PDF/UA requires every font used for real (non-artifact) content to be embedded with a Unicode mapping. `ApplyAccessibilitySetup` defaults to `strictConformance: true`, which enforces this at save time:

```
PdfAccessibilityConformanceException: Strict accessibility requires all rendered fonts
for tagged content to be embedded. Font 'Helvetica' is not embedded.
```

**The Standard-14 fonts (`Standard14Font.GetHelvetica()` and friends) are not embedded and will be rejected.** Load a real font file instead:

```csharp
using PdfLexer.Fonts;

var font = TrueTypeFont.CreateType0WritableFont(File.ReadAllBytes("Roboto-Regular.ttf"));
```

Type 0 (CID) fonts are the safe default: they carry a `ToUnicode` CMap and handle the full Unicode range.

### One `PageWriter` per page

MCIDs are allocated per `PageWriter` instance, not per page. Calling `page.GetWriter()` a second time restarts numbering at 0, and the colliding entries silently overwrite each other in the `ParentTree` — the earlier structure elements disappear from the tree with no error at save time.

```csharp
// WRONG: both paragraphs are written with /MCID 0; the first one is lost.
using (var writer = page.GetWriter()) { /* ... first paragraph ... */ }
using (var writer = page.GetWriter()) { /* ... second paragraph ... */ }

// RIGHT: one writer covers the whole page.
using (var writer = page.GetWriter())
{
    writer.BeginMarkedContent(first.GetNode());
    writer.Font(font, 12).TextMove(40, 700).Text("First paragraph");
    writer.EndMarkedContent();

    writer.BeginMarkedContent(second.GetNode());
    writer.Font(font, 12).TextMove(40, 680).Text("Second paragraph");
    writer.EndMarkedContent();
}
```

The same applies when adding tagged content to an existing untagged page that already contains marked-content sequences: the writer starts at MCID 0 and can collide with the MCIDs already in the content stream.

### Every mark of ink must be tagged or artifacted

Strict mode walks the serialized content streams and rejects any real content that is neither inside a `BeginMarkedContent(...)` scope nor inside `BeginArtifact(...)`. Decorative rules, running headers and footers, page numbers, and background watermarks all belong in artifacts — see [Artifacts](#artifacts).

---

## Recommended Authoring Workflow

1. Create or open an untagged document.
2. Call `ApplyAccessibilitySetup(...)` to set document-level metadata (Language, Title, PDF/UA XMP profile, `ViewerPreferences/DisplayDocTitle`, `MarkInfo/Suspects`, and page `/Tabs = /S`).
3. Build the structure hierarchy using `doc.Structure` (`StructuralBuilder`).
4. Write content inside `BeginMarkedContent(structureNode)` / `EndMarkedContent()` scopes, and non-semantic content inside `BeginArtifact(...)` / `EndMarkedContent()`.
5. Save the document and validate with veraPDF or PAC.

```csharp
using PdfLexer;
using PdfLexer.Content;
using PdfLexer.DOM;
using PdfLexer.Fonts;

// PDF/UA requires embedded fonts with Unicode mappings; Standard-14 fonts are rejected.
var font = TrueTypeFont.CreateType0WritableFont(File.ReadAllBytes("Roboto-Regular.ttf"));

using var doc = PdfDocument.Create();
var page = doc.AddPage();
doc.ApplyAccessibilitySetup("en-US", "Accessible Document", PdfUaProfile.PdfUa1);

var section = doc.Structure.AddSection("Main Section");
var heading = section.AddHeader(1, "Accessible Heading");
var paragraph = section.AddParagraph("Overview Paragraph")
    .ActualText("Overview paragraph for screen readers");

using (var writer = page.GetWriter())
{
    writer.BeginMarkedContent(heading.GetNode());
    writer.Font(font, 16).TextMove(40, 760).Text("Accessible Heading");
    writer.EndMarkedContent();

    writer.BeginMarkedContent(paragraph.GetNode());
    writer.Font(font, 12).TextMove(40, 735).Text("Overview paragraph for screen readers");
    writer.EndMarkedContent();

    // The running footer carries no meaning: mark it as a pagination artifact.
    writer.BeginArtifact(PdfName.Pagination);
    writer.Font(font, 10).TextMove(520, 20).Text("Page 1");
    writer.EndMarkedContent();
}

doc.SaveTo("accessible_output.pdf");
```

> [!NOTE]
> `PdfDocument` exposes `SaveTo(string path)` and `SaveTo(Stream)`; `Save()` returns the document bytes.

---

## Detailed Structural Builder Surface

`PdfDocument.Structure` returns a `StructuralBuilder`. Its `Add*` methods return an `IStructureContext` positioned **on the node just created**, so chained calls nest one level deeper each time. Use `.Back()` to move up one level, and `.GetNode()` to get the underlying `StructureNode` for content binding.

Because `.Back()` is positional, long chains are easy to miscount. Capturing each context in a local variable — as the examples below do — is the readable and safe form.

### 1. Document & Block Structure

```csharp
var part = doc.Structure.AddPart("Part I");
var section = part.AddSection("Section 1");
var div = section.AddDiv();
var quote = section.AddBlockQuote();
var heading = section.AddHeader(1, "Main Header"); // H1..H6
var paragraph = section.AddParagraph("Paragraph Text");
```

`AddHeader(level, ...)` builds the tag name by concatenation, so `AddHeader(7, ...)` emits `/H7` — not a standard type under PDF/UA-1, and not automatically role-mapped. Stay within `H1`–`H6`, and do not skip levels (`H1` → `H3` is a conformance failure that the library does not currently detect).

Types without a dedicated helper (`Art`, `Index`, `NonStruct`, `Private`, `Sub`, `Em`, `Strong`, `Ruby`, `Warichu`, …) are available through `AddElement(type, title)`.

### 2. Inline Elements & Metadata Primitives

```csharp
var span = paragraph.AddSpan("Highlighted Span", lang: "en-US")
    .Alt("Alternate representation")
    .ActualText("Visual text replacement")
    .Expansion("World Wide Web Consortium")
    .ElementId("unique-span-id");
```

| Method | PDF key | Purpose |
| --- | --- | --- |
| `.Alt(...)` | `/Alt` | Alternate description (required on `Figure` and `Formula`) |
| `.ActualText(...)` | `/ActualText` | Replacement text for the rendered glyphs |
| `.Expansion(...)` | `/E` | Expansion of an abbreviation or acronym |
| `.Lang(...)` | `/Lang` | Language override for this subtree |
| `.ElementId(...)` | `/ID` | Identifier, registered in the document `IDTree` |

> [!WARNING]
> These values are currently serialized as PDFDocEncoded strings. Characters outside Latin-1 (CJK, Arabic, Hebrew, and even typographic dashes) are **corrupted on write** — `图表：季度收入 — Résumé` round-trips as `??:???? - Résumé`. Restrict alternate text to Latin-1 until this is fixed. See [Known Limitations](#known-limitations).

### 3. Accessible Data Tables

Header cells must carry an explicit ID before a data cell can reference them. `TableHeaders(...)` resolves by ID string and **silently drops any reference it cannot resolve**, leaving a `/A << /O /Table >>` attribute dictionary with no `/Headers` entry and no error at save time.

```csharp
var table = doc.Structure.AddTable("Quarterly Revenue")
    .TableSummary("Revenue by quarter.");

var head = table.AddTableHead();
var headRow = head.AddRow();

var quarterHeader = headRow.AddHeaderCell()
    .ElementId("th-quarter")                     // required for TableHeaders(...) to resolve
    .TableScope(StructureScope.Column);
var revenueHeader = headRow.AddHeaderCell()
    .ElementId("th-revenue")
    .TableScope(StructureScope.Column);

var body = table.AddTableBody();
var bodyRow = body.AddRow();
var quarterCell = bodyRow.AddDataCell().TableHeaders("th-quarter");
var revenueCell = bodyRow.AddDataCell().TableHeaders("th-revenue");
```

`AddHeaderCell` / `AddDataCell` accept `rowSpan` and `colSpan`; values greater than 1 emit `/RowSpan` and `/ColSpan`. The scope enum is `StructureScope` (`Row`, `Column`, `Both`).

Strict mode validates table *nesting* (`TR` under `Table`/`THead`/`TBody`/`TFoot`, `TH`/`TD` under `TR`) but does **not** check that every `TH` has a `/Scope` or that data cells in complex tables carry `/Headers`. Verify tables with veraPDF.

### 4. Accessible Lists

```csharp
var list = doc.Structure.AddList("Features List")
    .ListNumbering(StructureListNumbering.Decimal); // Decimal, Disc, Square, LowerRoman, ...

var item = list.AddListItem();
var label = item.AddLabel();            // e.g. "1."
var itemBody = label.Back().AddListBody();
```

The numbering enum is `StructureListNumbering`. Note that `AddLabel()` returns the `Lbl` context, so `AddListBody()` needs a `.Back()` to attach as a sibling under the same `LI` rather than a child of `Lbl`.

### 5. Tagged Links & Object References (`OBJR`)

```csharp
var section = doc.Structure.AddSection("Body");
var targetHeading = section.AddHeader(2, "Target Section");

// /Link element with an OBJR-bound link annotation pointing at targetHeading
var link = section.AddLink(
    page,
    new PdfRect<double>(40, 700, 140, 720),
    targetHeading.GetNode(),
    accessibleDescription: "Jump to Target Section");

// External URI action
var external = section.AddLinkAction(
    page,
    new PdfRect<double>(40, 660, 260, 680),
    new PdfDictionary
    {
        [PdfName.S] = PdfName.URI,
        [(PdfName)"URI"] = new PdfString("https://example.com/accessibility")
    },
    "Read the accessibility statement");
```

Every link annotation needs a non-empty `/Contents`; strict mode enforces this. Under `PdfUaProfile.PdfUa2`, links whose destination is inside the current document must use the `StructureNode`-targeted overload above, or validation fails.

Remember to also write the visible link text inside `BeginMarkedContent(link.GetNode())` — the structure element and the annotation are separate things.

### 6. Accessible Form Fields & Tooltips

```csharp
var section = doc.Structure.AddSection("Registration");
var label = section.AddParagraph("Email address");
var formNode = label.Back().AddFormField(
    document: doc,
    page: page,
    rect: new PdfRect<double>(140, 650, 340, 670),
    fieldName: "UserEmail",
    title: "Email Address",
    tooltip: "Enter your email address",
    print: true);
```

> [!WARNING]
> Form support is currently minimal and **does not produce PDF/UA-conformant widgets on its own**. Only text fields are available (no checkbox, radio, choice, pushbutton, or signature), the generated widget has no `/AP` appearance stream — required by ISO 14289-1 §7.18.1 — and the `AcroForm` dictionary is created with only `/Fields` (no `/DA`, `/DR`, or `/NeedAppearances`). Expect veraPDF failures and blank field rendering in Acrobat until you supply these yourself via the returned dictionaries.

### 7. Tagged XObjects (Images & Forms)

```csharp
var figure = doc.Structure.AddFigure("Company Logo", altText: "Acme Corp logo");
figure.BindImage(logoImage, page);

using (var writer = page.GetWriter())
{
    writer.BeginMarkedContent(figure.GetNode());
    writer.Image(logoImage, 40, 600, 64, 64);
    writer.EndMarkedContent();
}
```

`BindImage` wires `/StructParent` on the image XObject; `BindFormXObject` wires `/StructParents` on a form XObject and reuses the index if the same form is bound to several pages. Strict mode requires `Figure` and `Formula` elements to carry either `Alt` or `ActualText`.

### 8. Elements Spanning Multiple Pages

Bind the same structure node from more than one page and the serializer emits `/MCR` marked-content references instead of a bare MCID list:

```csharp
var paragraph = doc.Structure.AddParagraph("Paragraph spanning a page break");

using (var writer = page1.GetWriter())
{
    writer.BeginMarkedContent(paragraph.GetNode());
    writer.Font(font, 12).TextMove(40, 60).Text("First half of the paragraph...");
    writer.EndMarkedContent();
}
using (var writer = page2.GetWriter())
{
    writer.BeginMarkedContent(paragraph.GetNode());
    writer.Font(font, 12).TextMove(40, 760).Text("...second half of the paragraph.");
    writer.EndMarkedContent();
}
```

### 9. Custom Role Maps & PDF 2.0 Namespaces

```csharp
doc.Structure.MapRole("CustomArticle", "Sect");
var ns = doc.Structure.DeclareNamespace("http://iso.org/pdf2/ssn");
doc.Structure.AddElement("CustomArticle", "A custom-tagged article");
```

`DeclareNamespace` takes the URI only and returns a `StructureNamespace` you can assign to individual nodes with `.SetNamespace(ns)`. `MapRole`, `AddClassDefinition`, and `DeclareNamespace` live on `StructuralBuilder` / `StructureRoot`, not on `IStructureContext`; reach them via `doc.Structure` or `doc.Structure.GetStructureRoot()`.

Strict mode rejects role-mapping *from* a standard structure type (e.g. `MapRole("P", "Div")`).

### 10. Bookmarks Linked to Structure

```csharp
var outlines = new OutlineBuilder();
var heading = doc.Structure.AddHeader(1, "Main Topic");
heading.CreateBookmark("Main Topic", outlines);
```

---

## Artifacts

Content that carries no meaning must be wrapped in an artifact scope so it stays out of the structure tree. This is mandatory — strict mode rejects untagged real content at save time.

```csharp
using (var writer = page.GetWriter())
{
    writer.BeginArtifact(PdfName.Pagination);   // Pagination, Layout, Page, Background
    writer.Font(font, 10).TextMove(520, 20).Text("Page 1");
    writer.EndMarkedContent();
}
```

`BeginArtifact` writes only the `/Type` entry. PDF/UA-1 §7.8 also expects running headers and footers to carry an artifact `/Subtype` of `/Header` or `/Footer`, and `/BBox` where applicable; neither is emitted, so full conformance for running heads currently requires post-processing the content stream.

---

## Document Setup Defaults

`ApplyAccessibilitySetup(language, title, profile, strictConformance)` configures:

- `Catalog/Lang` (document language);
- `Info/Title` (document title);
- `ViewerPreferences/DisplayDocTitle = true`;
- `MarkInfo/Marked = true` and `MarkInfo/Suspects = false`;
- Page `/Tabs = /S` (tab ordering);
- XMP metadata stream with `pdfuaid:part = 1` or `2` (and `pdfuaid:rev` for UA-2);
- minimum PDF version (1.7 for UA-1, 2.0 for UA-2);
- for UA-2, the PDF 2.0 structure namespace on the root `Document` element.

`strictConformance` defaults to `true`. It enables the pre-save and post-serialization checks described throughout this document. Setting it to `false` disables them — useful while iterating, but the output is then unvalidated.

---

## Known Limitations

These are current defects and coverage gaps, verified against the code in this repository. See [`accessibility_gaps_2.md`](accessibility_gaps_2.md) for the prioritized remediation plan.

| Area | Behaviour | Workaround |
| --- | --- | --- |
| **Non-Latin-1 text** | `/Alt`, `/ActualText`, `/E`, `/T`, `/Lang`, `/ID`, `/Summary`, and annotation `/Contents` are written as PDFDocEncoded strings. Non-Latin-1 characters are corrupted. | Restrict alternate text to Latin-1. |
| **MCID allocation** | Counters are per-`PageWriter`, not per page. A second writer on the same page restarts at 0 and silently drops earlier elements from the `ParentTree`. | Use exactly one writer per page. |
| **Annotation tagging** | Only `Link` and text `Widget` annotations can be bound to structure. Any other annotation subtype (Stamp, Square, FileAttachment, …) cannot be tagged, and strict mode does not detect the omission — it never inspects `/Annots`. | Avoid other annotation subtypes, or post-process. |
| **Form widgets** | No `/AP` appearance stream; `AcroForm` has no `/DA`, `/DR`, or `/NeedAppearances`; text fields only. | Populate the returned `WidgetAnnotation.NativeObject` / `.Field` dictionaries manually. |
| **Dangling IDs** | `TableHeaders(...)` and `References(...)` silently drop IDs that do not resolve. | Assign `ElementId(...)` before referencing; verify with veraPDF. |
| **Heading levels** | Skipped levels (`H1` → `H3`) and out-of-range levels (`H7`+) are not detected. | Review heading order manually. |
| **`AddNote()`** | Emits `/Note`, which strict mode unconditionally rejects. | Use `AddFENote()` for footnotes and endnotes. |
| **PDF/UA-2 namespaces** | Only the root `Document` element receives `/NS`; descendants inherit the default PDF 1.7 namespace. | Assign `.SetNamespace(ns)` per element if your validator requires it. |
| **Already-tagged input** | All authoring entry points throw on documents that already have a `StructTreeRoot`, including for appending new content. | Author into untagged or new documents only. |
| **Artifact detail** | `/Subtype` (`Header`/`Footer`/`Watermark`), `/BBox`, and `/Attached` are not emitted. | Post-process the content stream. |
| **`ParentTreeNextKey`** | Not written on `StructTreeRoot`. | Generally tolerated; add manually if a consumer requires it. |

---

## Remediation Workflow (Rule-Driven Engine)

For existing rendered untagged PDFs, use `doc.BeginRemediation(...)` to run template-based rule sets.

```csharp
using PdfLexer;
using PdfLexer.Remediation;

using var doc = PdfDocument.Open(inputBytes);
using var session = doc.BeginRemediation(new RemediationSessionConfiguration
{
    Language = "en-US",
    Title = "Remediated Invoice",
    Profile = PdfUaProfile.PdfUa1,
    LeftoverPolicy = RemediationLeftoverPolicy.AutoArtifact
});

var ruleSet = BuildInvoiceRules();
session.Use(ruleSet);

var dryRun = session.DryRun();
if (dryRun.Diagnostics.Count == 0)
{
    session.Commit();
}
doc.SaveTo("remediated_invoice.pdf");
```

See [Rule-Based PDF Remediation](rule-based-remediation.md) for full details on declarative rules, dynamic anchors, flow regions, and claim predicates.

---

## Automated Conformance & Validation

The library's strict mode is an internal integrity check, not a conformance validator. Always validate output with an external tool:

```bash
verapdf --format text --flavour ua1 output.pdf
```

See [PDF/UA Conformance](pdf-ua-conformance.md) for the fixture corpus and the recommended CI setup.
