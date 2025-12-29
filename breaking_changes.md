# Breaking Changes

## Content Model Hierarchy (vNext)

### [Hierarchical Marked Content]
The Content Model (`PdfLexer.Content.Model`) has been refactored to represent Marked Content (`BMC`, `BDC`, `EMC` operators) as a hierarchical tree structure rather than a flat list with properties. This allows for correct "Tagged PDF" editing and representation of empty tags.

#### `IContentGroup<T>`
*   **Removed**: `List<MarkedContent>? Markings { get; }`
    *   **Reason**: Marking information is no longer stored on the leaf nodes; it is strictly defined by the container (`MarkedContentGroup`) that the item resides in.
    *   **Migration**: If you need to access tags, you must traverse the parent hierarchy. If you simply want to find text with a specific tag, use the new `ContentExtensions.Flatten()` or a visitor to walk the tree.

#### `TextContent<T>`, `PathSequence<T>`, etc.
*   **Removed**: `.Markings` property.
    *   See above.

#### `ContentModelParser<T>`
*   **Behavior Change**: The `Parse()` method now returns a `List<IContentGroup<T>>` that may contain `MarkedContentGroup<T>` objects (which in turn contain children). Previously, this list only contained leaf nodes (`TextContent`, `PathSequence`, etc.).
    *   **Impact**: Code that iterates `page.Contents` expecting only leaf nodes will break or fail to recurse into groups.
    *   **Migration**: Use `.Flatten()` to get a linear list of leaf nodes if the structure is not needed.

#### `ContentModelWriter<T>`
*   **Behavior Change**: `MarkedContent` operators are now written based on the `MarkedContentGroup` structure. The `ReconcileMC` method (which attempted to diff lists) has been removed/changed.

### Example Migration
**Old Code:**
```csharp
foreach (var item in page.Contents)
{
    if (item is TextContent<T> text && text.Markings?.Any(m => m.Name == "P") == true)
    {
        // Found a paragraph
    }
}
```

**New Code:**
```csharp
// Option A: Use Flatten() (simulates old behavior)
foreach (var item in page.Contents.Flatten())
{
   // Note: 'Markings' property is gone, so you'd need to track context manually or use a Visitor/Walker 
   // if you need the tag info. Flatten() just gives you the content.
}

// Option B: Recursive Walk (Recommended for Structure)
void Visit(IEnumerable<IContentGroup<T>> items)
{
    foreach (var item in items)
    {
        if (item is MarkedContentGroup<T> group)
        {
             if (group.Tag.Name == "P") { /* ... */ }
             Visit(group.Children);
        }
        else if (item is TextContent<T> text)
        {
             // Found text
        }
    }
}
Visit(page.Contents);
```
