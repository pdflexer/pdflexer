# StructuralBuilder

`StructuralBuilder` provides a fluent API for constructing PDF Logical Structure trees (Tags), enabling semantic and accessible PDF creation.

## Basic Usage

The builder allows you to nest elements naturally using callback scopes. Every scoped `Add*` call returns the
context for the child it created, while `Configure(...)` returns the context it was called on.

```csharp
var builder = new StructuralBuilder();

builder.AddPart("Chapter 1", chapter =>
{
    chapter.AddSection("Introduction", intro =>
    {
        intro.AddHeader(1, "Welcome");
        intro.AddParagraph("This is accessible text.");
    });
    chapter.AddSection("Details", details =>
    {
        details.AddList(list => list.AddListItem(item =>
        {
            item.AddLabel("1.", _ => { });
            item.AddListBody("Item One", _ => { });
        }));
    });
});

var root = builder.GetRoot(); // Get Root StructureNode
```

This produces:

```
Document
  Part 'Chapter 1'
    Sect 'Introduction'
      H1 'Welcome'
      P 'This is accessible text.'
    Sect 'Details'
      L
        LI
          Lbl '1.'
          LBody 'Item One'
```

`.Back()` remains supported for source and binary compatibility, but it is positional and therefore easier to
miscount in nested trees. Prefer callback scopes for hierarchy and capture a returned child in a local when it is
needed later:

```csharp
var heading = builder.AddSection("Results", section =>
{
    section.AddHeader(1, "Results");
}).GetNode().Children[0];
```

Callbacks run synchronously. Exceptions propagate and do not roll back nodes already added. A null callback throws
`ArgumentNullException` before a node is created.

## Supported Elements

The builder supports a comprehensive set of PDF 2.0 structure types:

*   **Grouping**: `Part`, `Sect`, `Div`, `BlockQuote`, `Caption`, `TOC`, `TOCI`
*   **Headings**: `H1` - `H6` (via `AddHeader(level)`)
*   **Lists**: `L` (List), `LI` (ListItem), `Lbl` (Label), `LBody` (ListBody)
*   **Tables**: `Table`, `THead`, `TBody`, `TFoot`, `TR`, `TH`, `TD`
*   **Inline**: `P` (Paragraph), `Span`, `Quote`, `Code`, `Reference`, `FENote`
*   **Rich Content**: `Figure`, `Formula`, `Link` (support `Alt` text)

`AddNote()` also exists but emits `/Note`, which strict accessibility mode rejects — use `AddFENote()` for footnotes and endnotes. Types without a dedicated helper are reachable via `AddElement(type, title)`.

## Writing Content

You can associate page content directly with structure elements using `WriteContent`. This wraps the drawing operations in `BeginMarkedContent (BMC)` and `EndMarkedContent (EMC)` operators with the correct MCID.

```csharp
using PdfLexer.Fonts;

var font = TrueTypeFont.CreateType0WritableFont(File.ReadAllBytes("Roboto-Regular.ttf"));

using var doc = PdfDocument.Create();
var page = doc.AddPage();
doc.ApplyAccessibilitySetup("en-US", "Example", PdfUaProfile.PdfUa1);

using (var writer = page.GetWriter())
{
    doc.Structure.AddParagraph("Intro Text").Configure(paragraph =>
        paragraph.WriteContent(writer, w =>
        {
        // A font must be set before any text is written, or the writer throws
        // NotSupportedException: "Must set current font before writing."
            w.Font(font, 12).TextMove(40, 700).Text("Hello, World!");
        }));
}
```

MCIDs are allocated per page, so multiple `PageWriter` instances can safely write tagged content to the same
page. Append and prepend modes continue above existing MCIDs; replace mode starts again at 0. See
[Accessible Authoring](accessibility-authoring.md#before-you-start) for the full prerequisites, including the
embedded-font requirement.

## Outlines and Bookmarks

You can easily link structure elements to outlines (bookmarks) for navigation.

```csharp
var outlineBuilder = new PdfLexer.DOM.OutlineBuilder();

outlineBuilder.AddSection("Chapter 1", chapter =>
{
    chapter.AddBookmark("Overview", page);
    chapter.AddSection("Details", details =>
        details.AddBookmark("Results", page), isOpen: false, style: 2);
});
```

## See Also

- [Accessible Authoring](accessibility-authoring.md) — the full PDF/UA workflow, prerequisites, and current limitations.
- [Accessibility Gaps (Round 2)](accessibility_gaps_2.md) — known defects and the prioritized plan to close them.
