# Semantic Text Roadmap

This document proposes a roadmap for semantic text handling built on top of the Content Object Model (COM).

It is intentionally separate from the core content-model roadmap.

It is also intentionally separate from the scanner-based extraction roadmap and extraction surface.

The content model should continue to represent PDF content faithfully:

- `TextContent<T>` remains a fidelity-oriented content node
- glyphs, shifts, segment boundaries, and graphics state remain available for round-trip support

But many real tasks need a higher-level text layer that works semantically rather than operation-by-operation.

Examples:

- checking whether text exists on a page even when it is split across many content operations
- finding words or phrases in reading order rather than content-stream order
- replacing text that spans multiple text nodes, segments, or forms
- working with paragraphs, lines, and logical spans instead of glyph-level primitives

## Why a Separate Layer Is Needed

The COM alone is not the right abstraction for semantic text tasks.

`TextContent<T>` is designed to preserve fidelity:

- segment boundaries may reflect PDF operator structure, not logical text structure
- text may be split across many operations
- glyph order in the content stream may not match reading order
- kerning and spacing shifts are important for round-tripping but noisy for semantic inspection

That makes the COM excellent for:

- faithful parse/edit/write workflows
- geometry-aware text mutation
- preserving content structure

But it makes the COM alone awkward for:

- phrase search
- paragraph reconstruction
- semantic text replacement
- “does this page contain X?” style queries

The answer is not to make `TextContent<T>` less faithful.

The answer is to add a semantic text layer above it.

## Positioning

This roadmap is about semantic text handling for provenance-aware, mutation-oriented workflows.

It is not intended to define the library's fastest general text-extraction path.

The intended split is:

- scanners handle high-performance extraction and read-only structured text extraction workflows
- the semantic text layer on top of the COM handles search, semantic replacement, and commit-back editing where source provenance matters

That means this roadmap should optimize for:

- provenance
- editability
- commit-back behavior
- predictable semantic grouping for mutation

Not for maximum extraction throughput.

## Goals

The semantic text layer should:

- reconstruct text in meaningful reading units
- support reading-order and content-order views
- group text into characters, runs, lines, and paragraphs
- preserve provenance back to underlying COM nodes
- allow semantic search across fragmented source operations
- support semantic replacement with explicit commit behavior
- preserve untouched low-level details wherever possible when writing changes back

It should not try to become the one true text-extraction layer for all workflows.

## Non-Goals

This layer should not:

- replace the COM as the fidelity layer
- replace scanners as the high-performance extraction layer
- discard glyph/segment-level details from the underlying content model
- promise perfect natural-language reconstruction for every PDF
- hide all normalization decisions from the caller

## Core Design Principle

There should be two text views in the library:

### 1. Fidelity text

Backed directly by the COM:

- `TextContent<T>`
- segments
- glyphs
- shifts
- graphics state

Best for:

- preserving original structure
- exact parse/edit/write workflows
- low-level text manipulation

### 2. Semantic text

Built from the COM but projected into higher-level units:

- characters
- runs
- lines
- paragraphs
- matches and spans

Best for:

- search
- existence checks
- reading-order inspection
- user-facing replacement tasks

When the task is read-only extraction at very high throughput, scanners should remain the preferred surface.

## Proposed Model Shape

Illustrative only:

```csharp
public sealed class SemanticTextDocument<T>
{
    public IReadOnlyList<SemanticTextPage<T>> Pages { get; }
}

public sealed class SemanticTextPage<T>
{
    public string Text { get; }
    public IReadOnlyList<SemanticParagraph<T>> Paragraphs { get; }
    public IReadOnlyList<SemanticLine<T>> Lines { get; }
    public IReadOnlyList<SemanticTextRun<T>> Runs { get; }
    public IReadOnlyList<SemanticCharacter<T>> Characters { get; }

    public bool Contains(string text);
    public IReadOnlyList<SemanticMatch<T>> Find(string text);
    public void Replace(string oldValue, string newValue);
    public void Commit();
}

public sealed class SemanticCharacter<T>
{
    public char Char { get; }
    public PdfPoint<T> Position { get; }
    public SemanticTextProvenance<T> Provenance { get; }
}

public sealed class SemanticTextRun<T>
{
    public string Text { get; }
    public PdfRect<T> BoundingBox { get; }
    public IReadOnlyList<SemanticTextProvenance<T>> Provenance { get; }
}

public sealed class SemanticTextProvenance<T>
{
    public TextContent<T> SourceNode { get; }
    public int SegmentIndex { get; }
    public int GlyphIndex { get; }
}
```

The exact type names can change. The key idea is that semantic text objects must keep enough provenance to write changes back.

## Recommended Entry Points

### Build semantic text from a page

```csharp
var semantic = page.GetContentNodes<double>().GetSemanticText();
```

This should be treated as the mutation-capable semantic view, not as the fastest extraction API.

### Build semantic text with explicit options

```csharp
var semantic = page.GetContentNodes<double>().GetSemanticText(new SemanticTextOptions
{
    Order = TextOrder.Reading,
    MergeAcrossTextNodes = true,
    MergeAcrossSegments = true,
    RecurseIntoForms = true,
    RecurseIntoMarkedContent = true,
    DetectParagraphs = true
});
```

### Search semantically

```csharp
if (semantic.Contains("Terms and Conditions"))
{
    var matches = semantic.Find("Terms and Conditions");
}
```

### Replace semantically

```csharp
semantic.Replace("Draft", "Final");
semantic.Commit();
```

## Ordering Modes

The semantic text layer should support at least two explicit orderings.

### Content Order

Text is projected in the original content-stream order.

Use this when:

- fidelity to the source stream matters
- you are debugging text structure
- you need deterministic mapping close to source operations

### Reading Order

Text is projected in inferred human reading order.

Use this when:

- checking whether text exists
- finding phrases
- producing user-facing text output
- editing based on visible document meaning

Recommended default for semantic queries:

- `ReadingOrder`

## Provenance Requirements

Every semantic text unit should preserve a mapping back to the underlying COM.

Minimum provenance per unit:

- source `TextContent<T>`
- source segment index
- source glyph range
- source graphics state or run identity
- page and form ancestry if the text came from nested content

Without provenance, semantic replacement becomes lossy and unreliable.

## Search and Match Behavior

The semantic layer should support:

- exact text contains
- exact text find
- case-insensitive find
- regex find
- whole-word matching
- match results that expose both semantic bounds and source provenance

Illustrative shape:

```csharp
var matches = semantic.Find("Total Due", new SemanticFindOptions
{
    IgnoreCase = true,
    WholeWord = true
});
```

Each match should expose:

- matched text
- page-relative bounding box
- paragraph/line/run context
- provenance to affected underlying COM nodes

## Replacement Model

Replacement needs explicit policy because some edits can preserve structure while others require normalization.

### Replace Mode 1: Preserve When Possible

Goal:

- keep existing segments, glyphs, and spacing where unaffected
- regenerate only the edited portion

Best for:

- small in-place text edits
- edits within a single run or a narrow region

### Replace Mode 2: Normalize Affected Region

Goal:

- allow cross-node or cross-segment edits
- rebuild only the affected region into a cleaner text structure

Best for:

- phrase replacement across many operations
- paragraph-level replacement
- edits that substantially change length or shape

Illustrative API:

```csharp
semantic.Replace("Terms and Conditions", "Terms", new SemanticReplaceOptions
{
    Mode = ReplaceMode.PreserveWhenPossible,
    NormalizeAffectedRegion = true,
    AllowCrossNodeReplacement = true
});
```

## Commit Behavior

`Commit()` should write semantic edits back into the COM with explicit, documented rules.

Recommended behavior:

- untouched content remains untouched
- unchanged glyph spans keep original glyph/shift details
- changed spans regenerate glyph data from the chosen font context
- local normalization is allowed only inside the affected region
- unsupported edits fail clearly rather than silently corrupting output

Examples of unsupported or guarded cases:

- replacement characters unavailable in the source font
- edits that cross incompatible writing directions without normalization allowed
- edits requiring layout reflow that the layer does not support

## Paragraph and Line Reconstruction

Paragraphs and lines should be semantic projections, not fundamental COM nodes.

The semantic layer should:

- infer lines from text position and writing direction
- infer paragraphs from line grouping heuristics
- expose paragraph and line objects as stable query/edit units

Callers should be able to do:

```csharp
foreach (var paragraph in semantic.Paragraphs)
{
    Console.WriteLine(paragraph.Text);
}
```

And:

```csharp
var intro = semantic.Paragraphs.FirstOrDefault(p => p.Text.Contains("Introduction"));
```

## Nested Content

Semantic text must work across nested forms and marked content.

Recommended default:

- recurse into forms
- recurse into marked content
- keep ancestry information on semantic units

This allows callers to search semantically across a page without manually reconstructing nested content boundaries.

## Planned Improvements by Area

### A. Semantic text projection

Priority: high

Goals:

- build a reliable semantic text layer from COM nodes
- support multiple orderings and grouping strategies

Constraint:

- prioritize provenance-preserving reconstruction suitable for commit-back editing
- do not overfit this layer for scanner-style extraction performance

Work:

- define `SemanticTextOptions`
- implement reading-order and content-order projections
- project characters, runs, lines, and paragraphs
- keep provenance to the source COM

### B. Search and match APIs

Priority: high

Goals:

- make semantic search a first-class capability

Work:

- add contains/find APIs
- support whole-word, case-insensitive, and regex search
- return match objects with bounding boxes and provenance

### C. Replacement and commit-back editing

Priority: high

Goals:

- make semantic replacement practical without sacrificing fidelity unnecessarily

Work:

- define replace policies and options
- implement preserve-when-possible replacement
- implement normalize-affected-region replacement
- document unsupported cases and failure modes

### D. Paragraph and line handling

Priority: medium

Goals:

- support common human-facing text workflows

Work:

- infer lines robustly across orientations
- infer paragraphs with configurable heuristics
- expose paragraph and line objects for search and editing

### E. Diagnostics and debugging

Priority: medium

Goals:

- make normalization and commit behavior inspectable

Work:

- expose provenance inspection helpers
- expose debug views showing semantic text versus source COM ranges
- log or report why a replacement required normalization

## Recommended Delivery Phases

### Phase 1: semantic read model

Focus:

- semantic text projection
- reading-order versus content-order
- characters, runs, lines, paragraphs

Outcome:

- callers can inspect and query text semantically without manually reconstructing it from COM nodes

### Phase 2: search and match APIs

Focus:

- contains/find APIs
- match objects
- provenance-aware search results

Outcome:

- semantic phrase and word lookup becomes straightforward and reliable

### Phase 3: semantic replacement

Focus:

- replace and commit workflows
- preserve-when-possible editing
- normalization policies

Outcome:

- callers can replace semantically meaningful text and still write changes back through the COM

### Phase 4: paragraph-oriented editing and diagnostics

Focus:

- paragraph and line editing
- debug tooling
- failure reporting and normalization explanations

Outcome:

- the semantic text layer becomes suitable for higher-level document editing workflows

## Proposed Next Decisions

Before implementation starts, the following decisions should be made explicitly:

1. What should the default semantic ordering be: reading order or content order?
2. What provenance must be preserved to support commit-back replacement safely?
3. What kinds of replacement are supported in preserve mode versus normalize mode?
4. Should semantic replacement require explicit font-selection policy when new glyphs are introduced?
5. How much paragraph inference should be heuristic by default versus configurable?
6. Should semantic text span only a page at first, or support document-wide views immediately?

## Summary

The content model should remain the fidelity layer for text.

A separate semantic text layer should sit above it and provide:

- semantic reconstruction
- search
- phrase and paragraph handling
- provenance-aware replacement

Scanners should remain the primary path for extremely high-performance extraction, including semantic read-only extraction where commit-back provenance is not required.

If this is done well, developers will be able to do realistic text tasks such as:

- check whether a phrase exists even when split across many operations
- find and replace text in reading order
- edit paragraph-level content
- still preserve low-level round-trip details where the edit does not force regeneration
