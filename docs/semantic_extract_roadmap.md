# Structured Text Extraction Roadmap

This document proposes a roadmap for high-level structured text extraction built on top of the scanner stack.

It is separate from:

- the core content-model roadmap
- the semantic text roadmap built on top of the content model

The intended architecture is:

- scanners remain the performance-first extraction layer
- the content model remains the mutation/provenance-first layer
- structured text extraction builds high-level read-only helpers on scanners
- structured text extraction and content-model semantics share the same grouping and layout logic so the library does not maintain two divergent semantic systems

## Why This Roadmap Exists

The library already has:

- character scanning
- word scanning
- lower-level text extraction helpers

But real extraction workflows usually need more than characters or words.

Common needs include:

- lines
- paragraphs
- blocks
- tables or table-like regions
- headings
- reading-order reconstruction
- region-based extraction
- document layout clustering

Those workflows should be built on scanners for performance.

At the same time, they should not invent completely separate heuristics from the content-model semantic layer. Otherwise the library will end up with:

- one semantic interpretation for read-only extraction
- another semantic interpretation for mutation-oriented workflows

That split will become expensive and confusing.

## Positioning

This roadmap is about:

- fast, read-only structured text extraction
- high-level helper APIs over scanner output
- shared layout/semantic logic reusable by both extraction and COM-backed semantic workflows

This roadmap is not about:

- commit-back editing
- preserving exact mutation provenance
- replacing the content model as the fidelity layer

## Goals

The structured text extraction layer should:

- provide high-level extraction helpers beyond characters and words
- remain scanner-based and performance-oriented
- support reading-order reconstruction
- support region and layout-based grouping
- expose lines, paragraphs, blocks, and similar units
- share semantic grouping logic with the COM-backed semantic layer
- avoid duplicating clustering, reading-order, and layout heuristics across two subsystems

## Non-Goals

This layer should not:

- become the write/edit layer
- preserve the full mutation-oriented provenance required by commit-back editing
- require materializing the full content model for normal extraction tasks
- fork layout heuristics away from the COM-backed semantic layer

## Core Design Principle

There should be one shared semantic-layout engine used by two front doors:

### 1. Extraction front door

Built on scanners.

Optimized for:

- throughput
- low allocation
- read-only semantic grouping

### 2. Mutation front door

Built on the content model.

Optimized for:

- provenance
- editability
- commit-back safety

The layout and semantic grouping rules should be shared between them wherever possible.

## Shared Semantics Layer

The library should introduce a shared semantic/layout package or namespace for reusable algorithms and intermediate models.

Examples of logic that should be shared:

- clustering of characters into words
- grouping words into lines
- grouping lines into paragraphs
- reading-order reconstruction
- page-region partitioning
- docstrum-style analysis
- nearest-neighbour spacing analysis
- block segmentation
- heading/section heuristics

This shared layer should be independent of whether the source data came from:

- scanners
- COM text projections

## Proposed Architecture

Illustrative only:

```csharp
// scanner-side extraction
var structured = page.GetStructuredText(new StructuredTextOptions
{
    IncludeParagraphs = true,
    IncludeBlocks = true,
    Order = TextOrder.Reading
});

// COM-side semantic projection
var semanticForEditing = page.GetContentNodes<double>().GetSemanticText(new SemanticTextOptions
{
    Order = TextOrder.Reading
});
```

Both should rely on a shared grouping engine internally:

```csharp
var layout = SemanticLayoutEngine.Build(source, options);
```

Where `source` could be scanner-derived text primitives or COM-derived text primitives.

## Source Primitive Model

To share logic cleanly, both scanner-based and COM-based systems should project into a common intermediate text primitive.

Illustrative shape:

```csharp
public sealed class TextPrimitive<T>
{
    public char Char { get; }
    public PdfRect<T> BoundingBox { get; }
    public PdfPoint<T> BaselinePoint { get; }
    public double FontSize { get; }
    public double Rotation { get; }
    public int PageNumber { get; }
    public object? Provenance { get; }
}
```

Important distinction:

- scanner-based extraction can keep provenance lightweight
- COM-based semantic text can attach richer provenance for edit workflows

But both should feed the same downstream grouping logic.

## High-Level Extraction Targets

The structured text extraction layer should expose first-class helpers for:

- characters
- words
- lines
- paragraphs
- blocks
- regions
- reading-order text
- heading-like text spans
- table candidates

These do not all need to ship at once, but the roadmap should be organized around them.

## Proposed Public API Shape

### Build a structured text view

```csharp
var structured = page.GetStructuredText(new StructuredTextOptions
{
    Order = TextOrder.Reading,
    IncludeWords = true,
    IncludeLines = true,
    IncludeParagraphs = true,
    IncludeBlocks = true
});
```

### Extract lines

```csharp
foreach (var line in structured.Lines)
{
    Console.WriteLine(line.Text);
}
```

### Extract paragraphs

```csharp
foreach (var paragraph in structured.Paragraphs)
{
    Console.WriteLine(paragraph.Text);
}
```

### Extract block regions

```csharp
foreach (var block in semantic.Blocks)
{
    Console.WriteLine($"{block.Text} @ {block.BoundingBox}");
}
```

### Get text in reading order

```csharp
var text = structured.GetText(TextOrder.Reading);
```

### Extract text from a region

```csharp
var regionText = structured.GetRegion(new PdfRect<double>
{
    LLx = 0,
    LLy = 0,
    URx = 300,
    URy = 400
}).GetText();
```

## Planned Improvements by Area

### A. Shared primitive and layout engine

Priority: high

Goals:

- avoid duplicated semantic logic
- create one reusable semantic grouping core

Work:

- define shared text primitives
- define shared grouping options
- define a reusable semantic layout engine
- make both scanners and COM projections target the same downstream layout engine

### B. Line and paragraph extraction

Priority: high

Goals:

- move beyond words to practical text reconstruction

Work:

- implement line grouping
- implement paragraph grouping
- expose line and paragraph models
- expose reading-order text reconstruction

### C. Block and layout-region extraction

Priority: high

Goals:

- support real-world page understanding workflows

Work:

- implement block clustering
- expose block objects with text and bounds
- support page region partitioning
- add region query helpers

### D. Reading order and layout heuristics

Priority: high

Goals:

- make structured text extraction reliable across common layouts

Work:

- support multiple ordering strategies
- support configurable line/paragraph heuristics
- support multi-column reading-order reconstruction
- share these heuristics with the COM-backed semantic layer

### E. Docstrum and clustering infrastructure

Priority: medium to high

Goals:

- provide robust layout grouping foundations

Work:

- add docstrum-style nearest-neighbour analysis
- expose reusable clustering primitives
- support tuning for spacing, rotation, and local density
- ensure the same clustering code can power both extraction and COM semantic grouping

### F. Table and heading candidates

Priority: medium

Goals:

- make higher-order document understanding more practical

Work:

- identify table-like regions from line/block structure
- identify heading candidates from layout and typography cues
- expose these as candidates rather than overclaiming semantic certainty

### G. Performance and allocation control

Priority: high

Goals:

- keep structured text extraction aligned with scanner performance goals

Work:

- minimize allocations in primitive generation
- avoid materializing unnecessary intermediate structures
- support opt-in extraction levels so callers only pay for what they request
- benchmark structured text extraction separately from COM-based semantic workflows

## Shared Options Model

To avoid divergence, the extraction and COM-backed semantic systems should share concepts even if they expose different option types.

Shared concepts should include:

- ordering mode
- line grouping thresholds
- paragraph break heuristics
- block clustering strategy
- region partitioning strategy
- whitespace normalization behavior

That does not require identical public types, but it should encourage one internal semantic model.

## Performance Model

The structured text extraction layer should be scalable by extraction level.

Examples:

- character-only extraction
- word extraction
- line extraction
- paragraph extraction
- full block/layout extraction

Callers should be able to request only what they need.

Illustrative shape:

```csharp
var structured = page.GetStructuredText(new StructuredTextOptions
{
    Level = StructuredTextExtractLevel.Paragraphs
});
```

This helps protect the performance goals of the scanner-based stack.

## Relationship to the Semantic Text Roadmap

The semantic text roadmap focuses on:

- provenance-aware semantic text
- search and replacement
- commit-back editing

This roadmap focuses on:

- high-performance structured text extraction
- read-only layout understanding
- scanner-first helpers

Shared between both:

- text primitive projection
- clustering
- reading-order logic
- line/paragraph/block grouping
- layout heuristics

Distinct between both:

- extraction prioritizes throughput
- COM-backed semantic text prioritizes provenance and editability

## Recommended Delivery Phases

### Phase 1: shared semantic primitives

Focus:

- shared primitive model
- shared grouping engine
- scanner and COM adapters into that engine

Outcome:

- one semantic foundation instead of two divergent implementations

### Phase 2: lines and paragraphs

Focus:

- line grouping
- paragraph grouping
- reading-order text helpers

Outcome:

- high-level extraction becomes useful for common document-reading scenarios

### Phase 3: blocks and regions

Focus:

- block clustering
- region extraction
- multi-column support

Outcome:

- structured text extraction supports real layout understanding

### Phase 4: advanced layout analysis

Focus:

- docstrum and nearest-neighbour tuning
- table candidates
- heading candidates
- performance tuning

Outcome:

- extraction helpers become robust enough for more advanced document analysis workloads

## Proposed Next Decisions

Before implementation starts, the following decisions should be made explicitly:

1. What common intermediate primitive should scanners and COM semantic projections share?
2. Which grouping heuristics should be shared exactly versus adapted per front door?
3. What structured text extraction levels should be supported initially: lines, paragraphs, or blocks?
4. Should docstrum-style analysis be introduced early as the default grouping basis or later as an advanced strategy?
5. How much provenance should scanner-based structured text extraction expose by default?
6. What performance budgets should structured text extraction target relative to existing word/character helpers?

## Summary

The library should grow a scanner-based structured text extraction layer for high-level read-only workflows.

That layer should:

- go beyond characters and words
- stay fast
- expose lines, paragraphs, blocks, and reading-order helpers
- share the same semantic grouping logic used by COM-backed semantic text handling

If this is done well, the library will gain:

- fast high-level extraction
- consistent semantic behavior across read-only and mutation-oriented workflows
- one shared layout/semantic foundation instead of duplicated heuristics
