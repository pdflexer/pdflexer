# Content Model Roadmap

This document proposes a focused roadmap for the Content Object Model (COM) in `src/PdfLexer/Content/Model/`.

The COM is the higher-level access pattern for PDF page and form content streams. It sits above raw operator scanning and below broader document-level DOM wrappers.

It should be treated as a first-class part of the library architecture, not just as a parser artifact.

## Goals

The content model should:

- provide a semantic view of content streams beyond raw operators
- preserve enough PDF semantics to support round-tripping and editing
- support content inspection where semantic structure matters, especially as part of edit workflows
- expose a clear mental model for text, paths, images, forms, and marked content
- remain close enough to native PDF behavior that unsupported cases still have escape hatches
- be pleasant to use for common inspection and editing tasks without forcing callers to understand parser-internal structures

## Current Strengths

The current COM already has several strong foundations:

- `PdfPage.GetContentModel()` exposes a simple entry point for semantic content access
- parsing and writing are connected, so the model is not read-only
- content items retain graphics state, which is essential for faithful rendering and rewriting
- forms can be represented as content objects rather than disappearing into flattened scanner state
- marked content is preserved structurally instead of being discarded entirely
- mutation workflows such as `CachedContentMutation` already build on this model

In short, the COM is already closer to a useful high-level API than a lot of PDF libraries get.

## Positioning

The content model is not intended to be the primary raw text-extraction path.

The library should explicitly separate two concerns:

- scanners are the performance-oriented extraction path
- the content model is the semantic/mutation-oriented path

That means:

- high-throughput text extraction, semantic text extraction for read-only scenarios, and performance-critical search should continue to target scanner-based APIs
- the content model should prioritize semantic structure, provenance, editability, and predictable rewrite behavior
- content-model read APIs should still be good, but they do not need to compete with scanners as the fastest extraction surface

## Current Problems

The main problems are not that the COM exists. The main problems are about abstraction shape and API consistency.

### 1. One interface is carrying too many roles

`IContentGroup<T>` currently acts as:

- a leaf drawable item
- a container node
- a geometric clipping/splitting primitive
- a writable serialization unit

That works for some types, but it creates strain for:

- `MarkedContentGroup<T>`, which is a container more than a leaf item
- `FormContent<T>`, which is partly a reference to nested content and partly a drawable item

### 2. Uniform operations are not actually uniform

The interface suggests generic support for:

- `CopyArea(...)`
- `Split(...)`
- `ClipExcept(...)`
- `ClipFrom(...)`

But `FormContent<T>` throws for these operations today.

That means callers cannot safely treat all `IContentGroup<T>` instances as interchangeable manipulation targets.

### 3. Serialization internals leak into the public surface

`Write(ContentWriter<T>)` is public on the main content interface even though the code comments indicate it probably should not be part of the public API.

That makes the COM harder to evolve and mixes user-facing semantics with writer plumbing.

### 4. Container semantics are awkward

`MarkedContentGroup<T>` currently has a `GraphicsState` because the interface requires it, not because the type naturally has one stable state in the same sense as a text run or image.

That is a design smell and will likely keep creating edge cases.

### 5. Mutation behavior around forms needs clearer rules

The current recursive form mutation approach is valuable, but the semantics need to be made more explicit:

- when forms stay as forms
- when they are expanded
- how clipping is preserved
- how bounding boxes are recomputed
- what guarantees exist around resource preservation

### 6. Some implementation details are unfinished or inconsistent

Examples:

- disabled or incomplete cache paths in `CachedContentMutation`
- TODOs around inline images and marked-content points
- public APIs that appear stable but still behave like work-in-progress infrastructure

## Desired End State

The COM should become a stable high-level model with:

- a clear distinction between content items and content containers
- explicit support for recursive/nested content
- predictable mutation semantics
- a smaller, more intentional public API
- stronger alignment between parse-time shape and write-time behavior
- first-class traversal and query affordances for common tasks
- text-reading APIs that do not require understanding glyph shifts, segment internals, or line-matrix bookkeeping
- mutation entry points that feel like user-facing editing tools rather than low-level rewrite primitives

## Recommended Model Shape

### Layer 1: raw content scanning

Keep the low-level operator access path for advanced users:

- `PageContentScanner`
- parsed operator types
- form flattening options

This remains the escape hatch for unsupported or highly specialized content work.

It is also the preferred foundation for high-performance extraction workflows.

### Layer 2: semantic content model

This is the COM proper.

It should model the most important logical content objects:

- text
- paths
- images
- forms
- shading
- marked content containers

It should also provide the default high-level traversal and inspection experience for those objects rather than forcing callers to reconstruct that experience from raw node lists.

Its primary strength should be semantic inspection in support of mutation and provenance-aware editing, not raw extraction throughput.

### Layer 3: mutation and rewrite helpers

These should build on the semantic model rather than define it.

Examples:

- area copy/split helpers
- recursive form mutation helpers
- normalization helpers
- future content diff/replacement helpers

This is the center of gravity for the COM.

The COM should be optimized primarily for:

- structure-aware inspection
- mutation
- rewrite fidelity

Not for the fastest possible text extraction.

## Proposed API Direction

### Split content items from content containers

Recommended direction:

- introduce a base abstraction for all content nodes
- separate leaf/drawable content from container/group content

Illustrative shape:

```csharp
public interface IContentNode<T>
{
    ContentType Type { get; }
    PdfRect<T> GetBoundingBox();
}

public interface IContentItem<T> : IContentNode<T>
{
    GfxState<T> GraphicsState { get; }
    bool CompatibilitySection { get; }
}

public interface IContentContainer<T> : IContentNode<T>
{
    IReadOnlyList<IContentNode<T>> Children { get; }
}
```

This does not need to be implemented exactly this way, but the distinction should exist.

### Move writing behind an internal or secondary interface

Recommended direction:

- do not keep `Write(ContentWriter<T>)` as the main public contract on all content nodes
- either internalize it or move it behind a serialization-oriented interface

That keeps the public COM centered on semantic access and mutation, not writer internals.

### Decide how forms should behave

`FormContent<T>` needs an explicit contract.

Possible models:

1. form as a drawable reference node
   - exposes `Parse()` or `GetChildren()`
   - geometry operations are not directly supported
   - callers must expand first

2. form as a content container
   - exposes nested content directly
   - geometry operations recurse through children
   - the model owns form expansion semantics

3. dual-mode form API
   - direct/reference mode for fidelity
   - expanded mode for editing

Any of these can work. The important thing is to choose one and document it.

### Keep graphics state on leaf items, not as a universal rule

For leaf items like:

- `TextContent<T>`
- `PathSequence<T>`
- `ImageContent<T>`
- `ShadingContent<T>`

having a resolved graphics state is natural and useful.

For containers like marked content, it is at most contextual metadata and should not drive the overall interface design.

### Add a first-class query layer on top of the node model

The current node split is a good structural base, but it is not yet a complete usability story.

Callers should not need to hand-roll recursive traversal, repeated `OfType<T>()` filters, or container flattening for routine tasks.

Recommended direction:

- add `Descendants()` and `Descendants<TNode>()` helpers
- add type-oriented helpers such as `TextNodes()`, `FormNodes()`, `Containers()`, and `Leaves()`
- define clearly whether those helpers recurse into forms, marked content, or both
- keep `Flatten()` as a low-level helper, but do not make it the primary discovery path for common content queries

### Add read-oriented text projections to the content model

`TextContent<T>` currently exposes fidelity-oriented internal shape directly through segments and graphics state.

That is useful, but it is too low-level to be the primary reading experience.

Recommended direction:

- keep segment-level state for fidelity and writing
- add higher-level read helpers over that state:
  - `GetCharacters()`
  - `GetRuns()`
  - `GetBoundingBoxes()` or equivalent read-oriented projections
- add content-model-level helpers to retrieve text nodes or aggregate text without requiring callers to manually recurse and cast

The key principle is that routine text reading from the COM should not require understanding `GlyphOrShift<T>`, segment breaks, or line-matrix state.

However, those read-oriented projections should be framed as convenience and mutation-adjacent inspection helpers, not as replacements for scanner-based extraction APIs.

### Add mutation APIs that encode intent, not just plumbing

`CachedContentMutation` is valuable, but it still feels like an advanced primitive.

Recommended direction:

- keep `CachedContentMutation` as the low-level building block
- add typed mutation helpers on top of it:
  - `MutateDescendants<TNode>(...)`
  - `MutateText(...)`
  - `MutatePaths(...)`
- add an options object that makes recursive behavior explicit:
  - recurse through forms
  - recurse through marked content
  - preserve or drop empty containers
  - top-level-only versus descendant mutation

For more advanced scenarios, mutation callbacks should eventually be able to receive contextual information such as ancestor path or form-recursion context instead of only the current node.

## Planned Improvements by Area

### A. Core API cleanup

Priority: high

Goals:

- make the COM safe to consume generically
- reduce misleading public contracts
- clarify type responsibilities

Work:

- redesign `IContentGroup<T>` into smaller roles
- remove or de-emphasize writer plumbing from the main public interface
- document which operations are guaranteed across all node types
- decide whether `List<T>` or `IReadOnlyList<T>` is the preferred public child collection shape
- add a clear preferred traversal/query surface on top of raw node access

### B. Text model improvements

Priority: high

Goals:

- make text access reliable for extraction and transformation tasks
- expose text semantics more clearly without losing write fidelity

Note:

- performance-critical extraction should still target scanners
- COM text work should focus on fidelity-aware inspection and edit preparation

Work:

- fix character position enumeration bugs and ensure per-glyph positions use the evolving text state
- review `TextContent<T>` segmentation rules and document them
- decide what convenience APIs belong here:
  - `Text`
  - `EnumerateCharacters()`
  - word/line helpers
  - glyph/run access
- add tests for transformed, rotated, and multi-segment text
- add read-oriented projections so routine text access does not require segment-level knowledge
- define which text helpers are fidelity-oriented versus convenience-oriented

### C. Form handling improvements

Priority: high

Goals:

- make nested form content predictable and easier to mutate
- remove hidden assumptions around recursive rewriting

Work:

- define whether form nodes are references, containers, or dual-mode wrappers
- standardize how parent-page resource fallback is handled
- make recursive parsing/mutation APIs explicit
- document how bounding box recomputation works for rewritten forms

Status:

- recursive mutation through nested forms is now implemented in `CachedContentMutation`
- marked-content-preserving recursive mutation is now implemented
- helper-local form rewrite caching has been restored and now accounts for transform and clipping-sensitive rewrite state
- regression coverage now exists for reused forms, clipping preservation, and parse-edit-write structure
- the remaining form work is mostly about public-contract clarity and broader fidelity documentation rather than basic mutation correctness

DX follow-up:

- expose form traversal/editing semantics through clearer content-model helpers so callers do not have to infer recursion rules from `CachedContentMutation`

### D. Marked content improvements

Priority: medium

Goals:

- preserve logical structure without forcing container nodes into leaf semantics
- improve future tagged-PDF integration

Work:

- separate marked-content containers from leaf content interfaces
- decide what metadata should be exposed publicly:
  - tag name
  - property list
  - optional content group state
- add support for marked-content points if they are needed for fidelity
- define how flattening helpers should treat marked-content structure
- define how high-level query helpers present marked-content descendants versus container boundaries

### E. Geometry and clipping operations

Priority: medium

Goals:

- make area operations predictable
- reduce surprises from approximate bounding boxes and clipping-based splitting

Work:

- document which bounding boxes are exact versus approximate
- document that area copy/split is clip-based for partial intersections
- decide whether unsupported operations should:
  - throw
  - return a failure result
  - require prior expansion
- add tests for copy/split behavior on text, paths, images, and nested forms
- expose geometry helpers in a way that makes their scope and limitations obvious from the API shape

### F. Content writing and round-trip fidelity

Priority: medium

Goals:

- preserve the value of the COM as a mutable model
- ensure users can trust parse-edit-write workflows

Work:

- tighten round-trip tests for text, paths, images, forms, and marked content
- document what semantic preservation is expected versus what may be normalized
- review compatibility-section handling and optional-content handling during rewrite
- ensure public model changes do not make the writer depend on unstable implementation details
- keep writer-facing requirements from dominating the primary read/query shape of the public content model

### G. Inline image and advanced operator coverage

Priority: low to medium

Goals:

- reduce fidelity gaps in less common but real content streams

Work:

- decide whether inline images remain normalized into XObjects or gain explicit COM representation
- review support for marked-content points
- identify any missing operator families that should have semantic content nodes

## Recommended Delivery Phases

### Phase 1: stabilize the public contract

Focus:

- redesign or narrow `IContentGroup<T>`
- decide form semantics
- remove misleading public members
- fix known correctness bugs in text access

Outcome:

- the COM becomes safe to treat as an intentional API rather than as internal infrastructure that leaked outward

### Phase 2: make mutation workflows trustworthy

Focus:

- recursive form mutation semantics
- cache cleanup
- geometry operation rules
- stronger tests for parse-edit-write scenarios

Outcome:

- users can build real editing workflows on top of the COM with fewer hidden traps

Status:

- substantially complete
- recursive mutation now descends through marked-content containers and nested forms
- helper-local form rewrite caching is active again
- geometry behavior has been narrowed so helper recursion remains powerful without implying uniform direct manipulation support on every node type
- targeted parse-edit-write regression coverage has been added for the main nested-content mutation cases
- remaining follow-up in this area is documentation and long-tail fidelity clarification, not the core mutation workflow

### Phase 3: expand semantic coverage

Focus:

- marked-content improvements
- optional-content integration
- inline image decisions
- broader content fidelity support

Outcome:

- the COM handles a larger range of production PDFs without forcing callers back to raw scanners

### Phase 4: make the COM pleasant to use directly

Focus:

- add first-class traversal and query helpers
- add read-oriented text projections
- add intent-oriented mutation helpers and options
- improve discoverability of supported read/edit workflows

Outcome:

- the COM is not only structurally correct, but ergonomically strong enough to be the default high-level content API for common tasks

Constraint:

- this phase should improve COM ergonomics without turning the COM into the primary high-throughput extraction engine

## Example Target Shape

This is illustrative only.

```csharp
public interface IContentNode<T>
{
    ContentType Type { get; }
    PdfRect<T> GetBoundingBox();
}

public interface ILeafContent<T> : IContentNode<T>
{
    GfxState<T> GraphicsState { get; }
    void Transform(GfxMatrix<T> transform);
}

public interface IContentContainer<T> : IContentNode<T>
{
    IReadOnlyList<IContentNode<T>> Children { get; }
}

public sealed class TextContent<T> : ILeafContent<T>
{
    public string Text { get; }
    public IEnumerable<CharPos<T>> EnumerateCharacters();
}

public sealed class FormContent<T> : ILeafContent<T>
{
    public PdfStream NativeStream { get; }
    public IReadOnlyList<IContentNode<T>> GetChildren();
}

public sealed class MarkedContentGroup<T> : IContentContainer<T>
{
    public MarkedContent Tag { get; }
    public IReadOnlyList<IContentNode<T>> Children { get; }
}
```

Again, the exact shape can vary. The important change is to stop treating all content nodes as though they were the same kind of thing.

## Relationship to the Broader DOM

The COM should be treated as the content-stream counterpart to the document DOM:

- document DOM
  - pages, annotations, resources, fonts, outlines, forms, structure

- content model
  - text, paths, images, form invocations, shading, marked content

The same design principle should apply to both:

- high-level semantic access first
- raw escape hatches always available
- wrapper semantics must be explicit and consistent

## Proposed Next Decisions

Before major refactoring starts, the following decisions should be made explicitly:

1. Is the COM intended to be a stable public API or still an advanced/experimental layer?
2. What is the long-term role of `IContentGroup<T>`?
3. What exact contract should `FormContent<T>` provide?
4. Should marked content be represented as a first-class container hierarchy in the public API?
5. Which current behaviors are considered fidelity-preserving versus normalization?
6. What are the preferred high-level traversal/query APIs for callers using the COM directly?
7. Which text-reading affordances belong in `TextContent<T>` versus model-level helper extensions?
8. Which mutation workflows should have first-class helpers rather than requiring raw `CachedContentMutation` delegates?

## Current Status Snapshot

The roadmap is no longer at the starting point described in the earlier sections.

Completed or largely completed:

- Phase 1 public-shape stabilization has been started through the split between `IContentNode<T>`, `IContentItem<T>`, and `IContentContainer<T>`
- recursive mutation semantics for marked content and nested forms are now implemented
- `CachedContentMutation` caching is no longer effectively disabled and now has clip-aware reuse guards
- regression coverage exists for reused forms, clipping preservation, marked-content-descending mutation, and parse-edit-write structure
- text character enumeration coverage has been expanded for segmented, newline, and rotated text cases

Still open:

- broader public API cleanup around legacy compatibility surfaces such as `IContentGroup<T>`
- a clearer long-term public contract for `FormContent<T>`
- richer marked-content semantics and potential marked-content-point support
- broader geometry/copy-split policy documentation and coverage
- optional-content and compatibility-section fidelity documentation
- inline-image and advanced-operator semantic coverage decisions
- a true query/discovery layer for the content model
- read-oriented text projections that are simpler than direct segment/glyph inspection
- mutation helpers that encode common editing intent instead of requiring raw node-dispatch callbacks

Deliberately out of scope for the COM:

- replacing the scanner stack as the primary high-performance text extraction path
- making COM-based semantic text reconstruction the default solution for read-only extraction workloads

## Summary

The content model is a strong foundation and should be kept.

The key next step is not replacing it. The key next step is tightening its public shape:

- separate leaf content from container content
- make form behavior explicit
- reduce public writer-plumbing leakage
- strengthen mutation and round-trip semantics
- add a real query and text-reading layer on top of the raw node structure
- make common mutations feel intentional and direct

But the COM should still be judged primarily by how well it supports mutation-oriented and provenance-aware workflows, while scanners remain the extraction-first surface.

If that is done well, the COM can become one of the library’s strongest high-level APIs.
