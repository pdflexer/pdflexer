# StructuralBuilder

`StructuralBuilder` provides a fluent API for constructing PDF Logical Structure trees (Tags), enabling semantic and accessible PDF creation.

## Basic Usage

The builder allows you to nest elements naturally using `.AddElement()` or convenience methods like `.AddParagraph()`. Use `.Back()` to traverse up the tree.

```csharp
var builder = new StructuralBuilder();

builder.AddPart("Chapter 1")
       .AddSection("Introduction")
           .AddHeader(1, "Welcome").Back()
           .AddParagraph("This is accessible text.")
       .Back() // Exit Section
       .AddSection("Details")
           .AddList()
               .AddListItem()
                   .AddLabel("1.").Back()
                   .AddListBody("Item One").Back()
               .Back()
           .Back()
       .Back(); // Exit Details

var root = builder.GetRoot(); // Get Root StructureNode
```

## Supported Elements

The builder supports a comprehensive set of PDF 2.0 structure types:

*   **Grouping**: `Part`, `Sect`, `Div`, `BlockQuote`, `Caption`, `TOC`, `TOCI`
*   **Headings**: `H1` - `H6` (via `AddHeader(level)`)
*   **Lists**: `L` (List), `LI` (ListItem), `Lbl` (Label), `LBody` (ListBody)
*   **Tables**: `Table`, `THead`, `TBody`, `TFoot`, `TR`, `TH`, `TD`
*   **Inline**: `P` (Paragraph), `Span`, `Quote`, `Code`, `Reference`, `Note`
*   **Rich Content**: `Figure`, `Formula`, `Link` (supports `AltText`)

## Writing Content

You can associate page content directly with structure elements using `WriteContent`. This wraps the drawing operations in `BeginMarkedContent (BMC)` and `EndMarkedContent (EMC)` operators with the correct MCID.

```csharp
using var doc = PdfDocument.Create();
var page = doc.AddPage();
using var writer = new PageWriter<double>(page);
var builder = new StructuralBuilder();

builder.AddParagraph("Intro Text")
       .WriteContent(writer, w => 
       {
           w.Text("Hello, World!"); // Content inside the marked sequence
       });
```

## Outlines and Bookmarks

You can easily link structure elements to outlines (bookmarks) for navigation.

```csharp
var outlineBuilder = new PdfLexer.DOM.OutlineBuilder();

builder.AddHeader(1, "Main Topic")
       .CreateBookmark("Main Topic", outlineBuilder); 
       // Creates a bookmark titled "Main Topic" linked to this Header element
```
