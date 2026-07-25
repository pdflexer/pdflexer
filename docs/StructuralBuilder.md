# StructuralBuilder

`StructuralBuilder` provides a fluent API for constructing PDF Logical Structure trees (Tags), enabling semantic and accessible PDF creation.

## Basic Usage

The builder allows you to nest elements naturally using `.AddElement()` or convenience methods like `.AddParagraph()`.

Every `Add*` method returns a context positioned **on the node it just created**, so each chained call descends one more level. `.Back()` moves up exactly one level — including out of the element you just added. Getting this count wrong produces a valid-looking but incorrectly nested tree with no error, so prefer capturing contexts in locals (see [Readable Form](#readable-form)) for anything non-trivial.

```csharp
var builder = new StructuralBuilder();

builder.AddPart("Chapter 1")
           .AddSection("Introduction")
               .AddHeader(1, "Welcome").Back()          // back to Introduction
               .AddParagraph("This is accessible text.").Back()
           .Back()                                      // back to Chapter 1
           .AddSection("Details")
               .AddList()
                   .AddListItem()
                       .AddLabel("1.").Back()
                       .AddListBody("Item One").Back()
                   .Back()
               .Back()
           .Back();

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

Note the `.Back()` after `AddParagraph(...)`: without it, the subsequent `.AddSection("Details")` attaches to the paragraph's parent chain one level too deep and `Details` becomes a child of `Introduction` rather than its sibling.

### Readable Form

The equivalent tree built with locals — longer, but the nesting is explicit and cannot drift:

```csharp
var builder = new StructuralBuilder();

var chapter = builder.AddPart("Chapter 1");

var intro = chapter.AddSection("Introduction");
intro.AddHeader(1, "Welcome");
intro.AddParagraph("This is accessible text.");

var details = chapter.AddSection("Details");
var list = details.AddList();
var item = list.AddListItem();
item.AddLabel("1.");
item.AddListBody("Item One");
```

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

var paragraph = doc.Structure.AddParagraph("Intro Text");
using (var writer = page.GetWriter())
{
    paragraph.WriteContent(writer, w =>
    {
        // A font must be set before any text is written, or the writer throws
        // NotSupportedException: "Must set current font before writing."
        w.Font(font, 12).TextMove(40, 700).Text("Hello, World!");
    });
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

doc.Structure.AddHeader(1, "Main Topic")
             .CreateBookmark("Main Topic", outlineBuilder);
             // Creates a bookmark titled "Main Topic" linked to this Header element
```

## See Also

- [Accessible Authoring](accessibility-authoring.md) — the full PDF/UA workflow, prerequisites, and current limitations.
- [Accessibility Gaps (Round 2)](accessibility_gaps_2.md) — known defects and the prioritized plan to close them.
