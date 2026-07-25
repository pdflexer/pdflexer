# Content Model Common Tasks

This guide shows how to complete common tasks using the Content Object Model (COM).

It assumes the content-model DX work described in the roadmap has already been completed:

- first-class traversal helpers exist
- read-oriented text helpers exist
- intent-oriented mutation helpers exist on top of `CachedContentMutation`
- form and marked-content recursion behavior is explicit through options

This document is intentionally scoped to the content model. It does not cover scanner-oriented access patterns.

## When to Use the Content Model

Use the content model when you want:

- a semantic tree of page content
- read/write access to text, paths, images, forms, shading, and marked content
- nested-content traversal
- edit workflows that preserve structure while changing leaf content

Use the content model when your task is phrased like:

- "find all text nodes on the page"
- "move all paths inside forms"
- "replace text while preserving surrounding structure"
- "walk marked-content containers and inspect descendants"

## Core Concepts

The content model is built around three roles:

- `IContentNode<T>`: any node in the content tree
- `IContentItem<T>`: a drawable leaf item with graphics state
- `IContentContainer<T>`: a node that owns child nodes

Typical concrete node types:

- `TextContent<T>`
- `PathSequence<T>`
- `ImageContent<T>`
- `ShadingContent<T>`
- `FormContent<T>`
- `MarkedContentGroup<T>`

Typical entry point:

```csharp
using PdfLexer.Content.Model;

var nodes = page.GetContentNodes<double>();
```

## Basic Traversal

### Get all top-level nodes

```csharp
var nodes = page.GetContentNodes<double>();

foreach (var node in nodes)
{
    Console.WriteLine(node.Type);
}
```

### Walk all descendants

```csharp
foreach (var node in page.GetContentNodes<double>().Descendants())
{
    Console.WriteLine(node.Type);
}
```

### Get all nodes of a specific type

```csharp
var textNodes = page.GetContentNodes<double>().Descendants<TextContent<double>>().ToList();
var forms = page.GetContentNodes<double>().Descendants<FormContent<double>>().ToList();
var markedContent = page.GetContentNodes<double>().Descendants<MarkedContentGroup<double>>().ToList();
```

### Get only drawable leaf items

```csharp
var leaves = page.GetContentNodes<double>().Leaves().ToList();

foreach (var item in leaves)
{
    Console.WriteLine($"{item.Type}: {item.GetBoundingBox()}");
}
```

## Reading Text

### Get all text nodes on a page

```csharp
var textNodes = page.GetContentNodes<double>().TextNodes().ToList();
```

### Read the text of each node

```csharp
foreach (var text in page.GetContentNodes<double>().TextNodes())
{
    Console.WriteLine(text.Text);
}
```

### Read all text from a page through the content model

```csharp
var allText = page.GetContentNodes<double>().GetText();
Console.WriteLine(allText);
```

Use `GetText()` when you want a convenient aggregate of the content-model text nodes in content-tree order.

### Read characters with positions

```csharp
foreach (var text in page.GetContentNodes<double>().TextNodes())
{
    foreach (var ch in text.GetCharacters())
    {
        Console.WriteLine($"{ch.Char} at ({ch.XPos}, {ch.YPos})");
    }
}
```

Use `GetCharacters()` when you need:

- per-character positions
- character-by-character filtering
- a simple read API without dealing with segments directly

### Read runs instead of raw segments

```csharp
foreach (var text in page.GetContentNodes<double>().TextNodes())
{
    foreach (var run in text.GetRuns())
    {
        Console.WriteLine($"{run.Text} size={run.FontSize} bbox={run.BoundingBox}");
    }
}
```

Use `GetRuns()` when you want a read-oriented grouping with stable text, font, and bounds, but do not want to manage `Segments` and glyph shifts.

### Read the bounding box of a text node

```csharp
foreach (var text in page.GetContentNodes<double>().TextNodes())
{
    var bbox = text.GetBoundingBox();
    Console.WriteLine($"{text.Text}: {bbox}");
}
```

## Working with Forms

### Find all forms on a page

```csharp
var forms = page.GetContentNodes<double>().FormNodes().ToList();
```

### Inspect the descendants of a form

```csharp
foreach (var form in page.GetContentNodes<double>().FormNodes())
{
    foreach (var node in form.Descendants())
    {
        Console.WriteLine(node.Type);
    }
}
```

### Get all text nested inside forms

```csharp
var formText = page.GetContentNodes<double>()
    .FormNodes()
    .SelectMany(f => f.Descendants<TextContent<double>>())
    .ToList();
```

### Decide whether traversal should recurse into forms

Most content-model traversal helpers accept explicit recursion options.

```csharp
var textNodes = page.GetContentNodes<double>().Descendants<TextContent<double>>(new ContentTraversalOptions
{
    RecurseIntoForms = true,
    RecurseIntoMarkedContent = true
});
```

Use explicit options when you want the code to document whether form boundaries are being crossed.

## Working with Marked Content

### Find marked-content containers

```csharp
var groups = page.GetContentNodes<double>().Descendants<MarkedContentGroup<double>>().ToList();
```

### Inspect tags and descendants

```csharp
foreach (var group in page.GetContentNodes<double>().Descendants<MarkedContentGroup<double>>())
{
    Console.WriteLine(group.Tag.Name.Value);

    foreach (var text in group.Descendants<TextContent<double>>())
    {
        Console.WriteLine($"  {text.Text}");
    }
}
```

### Preserve marked-content structure while reading descendants

When you need descendants but still care about container boundaries, prefer `Descendants()` over `Leaves()`.

- `Leaves()` is best for "just give me drawable items"
- `Descendants()` is best when structure still matters

## Geometry Queries

### Get the bounding box of every leaf item

```csharp
foreach (var item in page.GetContentNodes<double>().Leaves())
{
    Console.WriteLine($"{item.Type}: {item.GetBoundingBox()}");
}
```

### Filter content by region

```csharp
var region = new PdfRect<double> { LLx = 0, LLy = 0, URx = 200, URy = 200 };

var inRegion = page.GetContentNodes<double>()
    .Leaves()
    .Where(x => region.CheckEnclosure(x.GetBoundingBox()) != EncloseType.None)
    .ToList();
```

### Split or copy supported items by area

Geometry helpers apply only to supported drawable content items.

```csharp
var rect = new PdfRect<double> { LLx = 50, LLy = 50, URx = 250, URy = 250 };

foreach (var item in page.GetContentNodes<double>().Leaves())
{
    var copy = item.CopyArea(rect);
    if (copy != null)
    {
        Console.WriteLine($"Copied {item.Type}");
    }
}
```

For forms, expand or recurse through descendants instead of treating the form reference itself as a generic geometry primitive.

## Editing Content

### Shift all text on a page

```csharp
var updated = page.GetContentNodes<double>()
    .MutateText(text =>
    {
        text.TransformInitial(new GfxMatrix<double> { E = 10, F = 0 });
        return text;
    });
```

### Shift only matching text

```csharp
var updated = page.GetContentNodes<double>()
    .MutateText(text =>
    {
        if (!text.Text.Contains("Invoice"))
        {
            return text;
        }

        text.TransformInitial(new GfxMatrix<double> { E = 20, F = 0 });
        return text;
    });
```

### Mutate all paths, including paths nested inside forms

```csharp
var mutation = new CachedContentMutation<double>()
    .MutateDescendants<PathSequence<double>>(path =>
    {
        path.TransformInitial(new GfxMatrix<double> { E = 0, F = -15 });
        return path;
    }, new CachedContentMutationOptions
    {
        RecurseIntoForms = true,
        RecurseIntoMarkedContent = true
    });

var updatedPage = mutation.Apply(page);
```

### Remove matching nodes

Return `null` from a typed mutation helper to remove the node.

```csharp
var mutation = new CachedContentMutation<double>()
    .MutateDescendants<ImageContent<double>>(image =>
    {
        var bbox = image.GetBoundingBox();
        return bbox.LLx < 100 ? null : image;
    });

var updatedPage = mutation.Apply(page);
```

### Replace text nodes while preserving surrounding structure

```csharp
var mutation = new CachedContentMutation<double>()
    .MutateText(text =>
    {
        if (text.Text != "Draft")
        {
            return text;
        }

        return TextContent<double>.Create("Final", font: myFont, fontSize: 12);
    }, new CachedContentMutationOptions
    {
        RecurseIntoForms = true,
        RecurseIntoMarkedContent = true
    });

var updatedPage = mutation.Apply(page);
```

## Using Mutation Context

For advanced edits, mutation callbacks can receive context describing where the node came from.

```csharp
var mutation = new CachedContentMutation<double>()
    .MutateDescendants<TextContent<double>>((text, ctx) =>
    {
        if (ctx.IsInsideForm)
        {
            text.TransformInitial(new GfxMatrix<double> { E = 5, F = 0 });
        }

        if (ctx.Ancestors.OfType<MarkedContentGroup<double>>().Any(g => g.Tag.Name.Value == "Artifact"))
        {
            return null;
        }

        return text;
    });
```

Typical context members:

- `Ancestors`
- `IsInsideForm`
- `IsInsideMarkedContent`
- `OwningForm`
- `EffectiveTransform`

## Writing the Result Back

### Replace page content with updated nodes

```csharp
var updatedNodes = page.GetContentNodes<double>()
    .MutateText(text =>
    {
        text.TransformInitial(new GfxMatrix<double> { E = 10, F = 0 });
        return text;
    });

using var writer = page.GetWriter<double>(PageWriteMode.Replace);
writer.AddContent(updatedNodes);
```

### Apply a page-level cached mutation workflow

```csharp
var mutation = new CachedContentMutation<double>()
    .MutateDescendants<TextContent<double>>(text =>
    {
        if (text.Text.Contains("Confidential"))
        {
            return null;
        }
        return text;
    });

var updatedPage = mutation.Apply(page);
```

Use the page-level `Apply(page)` overload when:

- you want nested form rewriting handled for you
- you want the result as a new `PdfPage`
- you want helper-local form rewrite caching during the workflow

## Recommended Patterns

### Prefer type-oriented queries over manual casts

Prefer:

```csharp
var texts = nodes.TextNodes();
```

Over:

```csharp
var texts = nodes.Descendants().OfType<TextContent<double>>();
```

The manual form is still useful, but the typed helper is clearer and easier to discover.

### Prefer read-oriented text helpers over segment inspection

Prefer:

```csharp
var chars = text.GetCharacters();
var runs = text.GetRuns();
```

Over directly inspecting:

- `Segments`
- `GlyphOrShift<T>`
- `LineMatrix`

Use segment-level access only when you are doing fidelity-sensitive rewriting.

### Make recursion explicit when editing nested content

Prefer:

```csharp
new CachedContentMutationOptions
{
    RecurseIntoForms = true,
    RecurseIntoMarkedContent = true
}
```

Over relying on hidden defaults when the edit crosses container boundaries.

## Common Pitfalls

### Confusing leaves with descendants

Use:

- `Descendants()` when you want structure-aware traversal
- `Leaves()` when you want drawable items only

### Treating forms as generic geometry items

`FormContent<T>` is a content-model node, but direct geometry operations apply to supported drawable leaf items.

If you want to edit inside forms:

- traverse descendants, or
- use `CachedContentMutation` with explicit recursion options

### Using text node structure as if it were layout structure

`TextContent<T>` represents a content-model text object, not necessarily a human-readable line or paragraph.

Use:

- `GetText()` for convenience
- `GetCharacters()` for precise positions
- `GetRuns()` for read-oriented grouping

Do not assume one `TextContent<T>` equals one sentence, line, or paragraph.

## Example: Find, Shift, and Rewrite Nested Text

```csharp
using PdfLexer.Content.Model;

var mutation = new CachedContentMutation<double>()
    .MutateText((text, ctx) =>
    {
        if (!text.Text.Contains("Total"))
        {
            return text;
        }

        text.TransformInitial(new GfxMatrix<double> { E = 12, F = 0 });
        return text;
    }, new CachedContentMutationOptions
    {
        RecurseIntoForms = true,
        RecurseIntoMarkedContent = true
    });

var updatedPage = mutation.Apply(page);
```

This is the intended high-level content-model workflow:

- query semantically
- mutate by node type
- make recursion explicit
- let the helper preserve required structure and nested-form rewriting

