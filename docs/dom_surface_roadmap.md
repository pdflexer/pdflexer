# DOM Surface Roadmap

This document proposes a practical roadmap for building out a higher-level PDF DOM surface in `pdflexer`.

It is intended to complement:

- [`docs/access_api_consistency_plan.md`](/workspace/docs/access_api_consistency_plan.md), which focuses on wrapper vs raw access semantics
- [`docs/library_roadmap.md`](/workspace/docs/library_roadmap.md), which tracks broader library priorities

The goal here is narrower:

- identify the most important PDF object families to expose as high-level wrappers
- define a rough priority order
- describe, at a very high level, what each wrapper surface could look like
- keep the public DOM centered on user tasks rather than on Arlington table shapes

## Design Position

The public DOM should be:

- wrapper-first for common document tasks
- backed by raw `PdfDictionary` / `IPdfObject` escape hatches
- explicit about direct vs effective vs editable access where PDF inheritance/defaulting matters
- selective rather than exhaustive in its first iterations
- page-centric for editable truth, with some document-level structures exposed as read helpers or save-time rebuildable projections rather than always-live mutable state

The Arlington model should support this work as:

- validation metadata
- object classification metadata
- version/default/inheritance metadata
- coverage guidance for missing wrappers

It should not define the public API one-to-one.

## DOM Layering

Recommended layers:

1. raw PDF object model
   - `PdfDictionary`
   - `PdfArray`
   - `PdfStream`
   - `IPdfObject`
   - indirect references and `Resolve()`

2. semantic wrapper model
   - `PdfDocument`
   - `PdfPage`
   - typed wrappers for important object families

3. Arlington-backed metadata and validation
   - schema information
   - validation rules
   - typed classification helpers
   - diagnostics and future coverage tracking

## Public Wrapper Conventions

High-level wrappers should follow a consistent shape where practical:

- `NativeObject`
  - the underlying `PdfDictionary` or `PdfStream`
- `TryGetDirectX(...)`
  - returns only directly stored values
- `GetEffectiveX(...)`
  - resolves inheritance/defaults without mutation
- `GetOrCreateX()` or `EnsureX()`
  - materializes editable state when the caller wants to mutate
- simple wrapper properties
  - only for cases where behavior is obvious and stable

Wrappers should avoid hidden mutation on read unless that behavior is both deliberate and clearly documented.

## Document-Level State Categories

Not every document-level feature should be modeled the same way.

Recommended categories:

### 1. Editable primary state

This is the data the library should treat as the main mutable truth during normal editing workflows.

Examples:

- `Pages`
- page-owned content
- page-owned resources
- page-owned annotations

### 2. Read-oriented document projections

These should be easy to inspect after loading a document, but they should not necessarily be treated as always-live mutable state that stays synchronized automatically after page insertion, removal, or reordering.

Examples:

- outlines
- AcroForm field tree
- named destinations
- structure tree
- page labels

These are good candidates for APIs such as:

- `TryGetOutlines(...)`
- `ReadOutlines()`
- `TryGetAcroForm(...)`
- `ReadAcroForm()`
- `TryGetStructTreeRoot(...)`

### 3. Save-time rebuildable structures

Some cross-page structures are best treated as data that may become stale after page edits and are then:

- rebuilt
- remapped
- normalized
- preserved selectively

during save.

Examples:

- outline destinations
- widget-to-page associations
- structure parent mappings
- parent tree and number trees
- page labels
- catalog name-tree-backed destinations

The design implication is:

- `PdfDocument` should own `Pages` as editable primary state
- some other document-level features should be exposed first as convenient read helpers
- save is the normalization boundary for cross-page structures unless the library explicitly chooses to maintain them eagerly

## Priority Tiers

### Tier 1: Core navigation and page usage

These are the wrappers most likely to improve daily usability quickly.

#### 1. `PdfDocument`

Why it matters:

- primary entry point
- owns page access, catalog access, save/open workflows
- natural home for document-level helpers

High-level shape:

- `Pages`
- `Catalog`
- `Info`
- `Metadata`
- `TryGetOutlines(...)`
- `ReadOutlines()`
- `TryGetAcroForm(...)`
- `ReadAcroForm()`
- `TryGetStructTreeRoot(...)`
- `ReadNamedDestinations()`
- `GetDocumentId()`
- save and normalization options

Likely responsibilities:

- own editable page state
- expose convenient read access to catalog-backed and cross-page structures
- avoid implying that all secondary document structures remain live-valid after page mutations
- provide save and normalization options for rebuilding or preserving document-level structures

#### 2. `PdfPage`

Why it matters:

- already central to the library
- most user workflows are page-centric
- strongest current need for direct vs effective vs editable semantics

High-level shape:

- `NativeObject`
- `Resources`
- `TryGetDirectResources(...)`
- `GetEffectiveResources(...)`
- `EnsureResources()`
- `MediaBox`, `CropBox`, `BleedBox`, `TrimBox`, `ArtBox`
- `TryGetDirectMediaBox(...)`, `GetEffectiveMediaBox(...)`
- `Rotate`, `TryGetDirectRotate(...)`, `GetEffectiveRotate()`
- `Contents`
- `Annotations`
- `StructParents`
- content scanning and writing helpers

Likely responsibilities:

- page geometry and inherited state
- resource access
- content stream access
- annotation access
- entry point for most inspection and editing workflows

#### 3. `PdfResources`

Why it matters:

- resources are central to fonts, XObjects, color spaces, patterns, and graphics state
- current users likely traverse raw dictionaries here often

High-level shape:

- `Fonts`
- `XObjects`
- `ColorSpaces`
- `Patterns`
- `ExtGState`
- `Properties`
- `TryGetFont(PdfName name, out PdfFont font)`
- `TryGetXObject(PdfName name, out PdfXObject xobj)`
- `GetOrCreateFonts()`

Likely responsibilities:

- typed access over resource subdictionaries
- preserving named resource lookup semantics
- making page/resource traversal feel uniform

#### 4. `PdfFont` family

Why it matters:

- fonts are one of the most common advanced inspection needs
- the library already has font-related DOM types

High-level shape:

- abstract `PdfFont`
- concrete wrappers for common font families already supported by the library
- `BaseFont`
- `Subtype`
- `Encoding`
- `ToUnicode`
- `Descriptor`
- `GetUnicodeMap()`
- `GetWidths()`

Likely responsibilities:

- unify existing font wrapper types behind a common public shape
- expose descriptor, encoding, and descendant relationships consistently

#### 5. `PdfXObject` family

Why it matters:

- forms and images are core to rendering, extraction, reuse, and editing
- users frequently need typed access to an `/XObject` entry

High-level shape:

- abstract `PdfXObject`
- `PdfFormXObject`
- `PdfImageXObject`
- common properties:
  - `Subtype`
  - `BBox`
  - `Resources`
  - `Width`
  - `Height`
  - `ColorSpace`

Likely responsibilities:

- unify typed access to image and form resources
- expose stream plus dictionary semantics in one place

### Tier 2: Common document features beyond pages

These unlock more complete document inspection and preservation scenarios.

Most of these should initially be thought of as:

- read helpers over loaded PDF structures
- typed wrappers over native objects
- save-time rebuild/remap targets where needed

rather than as always-live mutable document-owned state parallel to `Pages`.

#### 6. `PdfAnnotation` family

Why it matters:

- annotations are common in real documents
- currently awkward to use through raw dictionaries alone

High-level shape:

- abstract `PdfAnnotation`
- subtype wrappers for the most common annotations first:
  - text
  - link
  - widget
  - highlight
  - square/circle
- common properties:
  - `Subtype`
  - `Rect`
  - `Contents`
  - `Flags`
  - `Appearance`
  - `Border`
  - `Color`
  - `Page`

Likely responsibilities:

- typed access to annotation basics
- link actions/destinations for link annotations
- field linkage for widget annotations

#### 7. `PdfDestination` and action wrappers

Why it matters:

- destinations and actions connect outlines, links, and named destinations
- these are hard to use ergonomically in raw array/dictionary form

High-level shape:

- `PdfDestination`
- `PdfAction`
- common action subtypes first:
  - go-to
  - URI
  - remote go-to
  - named action
- helpers:
  - `TryGetTargetPage(...)`
  - `TryGetZoomMode(...)`

Likely responsibilities:

- normalize array-vs-dictionary destination representations
- expose action subtype and target semantics cleanly

#### 8. `PdfOutline` / `PdfOutlineItem`

Why it matters:

- outline traversal is a common navigation task
- existing support can likely be lifted into a cleaner public surface

High-level shape:

- `PdfOutlineRoot`
- `PdfOutlineItem`
- `Title`
- `Children`
- `Destination`
- `Action`
- `IsOpen`
- `Color`
- `Flags`

Likely responsibilities:

- tree navigation
- destination/action access
- editing outline structure without dropping to raw dictionaries

#### 9. `PdfAcroForm` and field wrappers

Why it matters:

- forms are high-value for many users
- widget annotations and field dictionaries need a coherent model

High-level shape:

- `PdfAcroForm`
- abstract `PdfFormField`
- typed field wrappers:
  - text
  - checkbox/radio
  - combo/list
  - signature
- common properties:
  - `Name`
  - `AlternateName`
  - `Value`
  - `DefaultValue`
  - `Flags`
  - `Widgets`
  - `Kids`

Likely responsibilities:

- form tree traversal
- inherited field attribute resolution
- field/widget bridging

#### 10. `PdfMetadata` and document info wrappers

Why it matters:

- common inspection need
- relatively low complexity compared to many other object families

High-level shape:

- `PdfDocumentInfo`
- `PdfMetadataStream`
- common properties:
  - `Title`
  - `Author`
  - `Subject`
  - `Keywords`
  - `Creator`
  - `Producer`
  - `CreationDate`
  - `ModDate`

Likely responsibilities:

- simple typed access to info dictionary data
- stream-backed XMP access where present

### Tier 3: Structure and advanced preservation features

These matter for fidelity, accessibility, and advanced workflows, but are less essential for an initial DOM surface.

#### 11. structure tree wrappers

Why it matters:

- tagged PDF support and accessibility workflows depend on it
- the repo already has structural builder work

High-level shape:

- `PdfStructTreeRoot`
- `PdfStructureElement`
- `PdfMarkedContentReference`
- `PdfObjectReference`
- common properties:
  - `Type`
  - `S`
  - `Kids`
  - `Pg`
  - `Parent`
  - `Alt`
  - `Lang`

Likely responsibilities:

- tree traversal
- mapping structure nodes to pages and marked content
- preservation and serialization support

#### 12. name tree and number tree wrappers

Why it matters:

- many catalog features depend on these
- they are awkward to use as raw arrays and nested dictionaries

High-level shape:

- `PdfNameTree<T>`
- `PdfNumberTree<T>`
- typed projections for:
  - destinations
  - embedded files
  - JavaScript
  - page labels
  - parent tree

Likely responsibilities:

- hide tree node mechanics
- expose logical key/value enumeration and lookup

#### 13. embedded file and file specification wrappers

Why it matters:

- needed for attachment support and name-tree-backed catalog features

High-level shape:

- `PdfFileSpecification`
- `PdfEmbeddedFile`
- common properties:
  - `FileName`
  - `Description`
  - `EmbeddedFile`
  - `MimeType`
  - `Params`

#### 14. optional content wrappers

Why it matters:

- useful for layered documents
- likely lower demand than the core objects above

High-level shape:

- `PdfOptionalContentProperties`
- `PdfOptionalContentGroup`
- `PdfUsageDictionary`

#### 15. page label, viewer preference, transition, and presentation wrappers

Why it matters:

- useful, but secondary compared to pages/resources/annotations/forms

High-level shape:

- small focused wrappers around catalog/page-level convenience features

## Recommended Delivery Order

### Phase A: stabilize the access model

Goals:

- finalize direct vs effective vs editable conventions
- make `PdfPage` and `PdfDocument` the reference shape for future wrappers
- reduce write-on-read surprises

Objects:

- `PdfDocument`
- `PdfPage`
- `PdfResources`

### Phase B: unify core visual/content objects

Goals:

- make everyday page/resource traversal fully typed for common cases
- reduce raw dictionary access for fonts and XObjects

Objects:

- `PdfFont` family
- `PdfXObject` family
- supporting geometry/color/resource helpers as needed

### Phase C: navigation and interaction

Goals:

- unlock annotations, links, outlines, destinations, and forms

Objects:

- `PdfAnnotation` family
- `PdfDestination`
- `PdfAction` family
- `PdfOutline`
- `PdfAcroForm` and form fields

### Phase D: preservation and advanced document structure

Goals:

- support richer inspection and better save fidelity for advanced PDFs

Objects:

- structure tree wrappers
- name tree / number tree wrappers
- embedded file wrappers
- optional content wrappers

## Suggested Internal Architecture

To avoid repeating the abandoned Arlington-to-public-model problem, the implementation should separate:

### 1. wrapper semantics

Hand-designed or selectively generated.

This includes:

- property names
- mutation rules
- direct/effective/materialized behavior
- convenience methods

### 2. object classification

Arlington-backed or Arlington-assisted.

This includes:

- identifying whether a dictionary is a page, annotation, font, form XObject, destination-like object, and so on
- version-aware matching logic

### 3. validation

Arlington-backed.

This includes:

- allowable keys
- value types
- required conditions
- version-specific rules

### 4. repetitive boilerplate generation

Selective code generation may still help for:

- low-risk property wrappers
- repeated dictionary accessors
- subtype dispatch registries

It should not define the public model wholesale.

## Initial Wrapper Sketches

These examples are illustrative only. They are meant to show shape, not final API names.

### `PdfPage`

```csharp
public sealed class PdfPage
{
    public PdfDictionary NativeObject { get; }

    public PdfResources GetEffectiveResources();
    public bool TryGetDirectResources([NotNullWhen(true)] out PdfResources? resources);
    public PdfResources EnsureResources();

    public PdfRectangle GetEffectiveMediaBox();
    public bool TryGetDirectMediaBox(out PdfRectangle box);

    public int GetEffectiveRotate();
    public bool TryGetDirectRotate(out int rotate);

    public IReadOnlyList<PdfAnnotation> Annotations { get; }
    public IEnumerable<PdfStream> Contents { get; }
}
```

### `PdfResources`

```csharp
public sealed class PdfResources
{
    public PdfDictionary NativeObject { get; }

    public IReadOnlyDictionary<PdfName, PdfFont> Fonts { get; }
    public IReadOnlyDictionary<PdfName, PdfXObject> XObjects { get; }

    public bool TryGetFont(PdfName name, [NotNullWhen(true)] out PdfFont? font);
    public bool TryGetXObject(PdfName name, [NotNullWhen(true)] out PdfXObject? xObject);
}
```

### `PdfAnnotation`

```csharp
public abstract class PdfAnnotation
{
    public PdfDictionary NativeObject { get; }

    public PdfName Subtype { get; }
    public PdfRectangle Rect { get; set; }
    public PdfString? Contents { get; set; }
}

public sealed class PdfLinkAnnotation : PdfAnnotation
{
    public PdfDestination? Destination { get; }
    public PdfAction? Action { get; }
}
```

## Coverage Guidance

To avoid an overly broad first cut, wrapper candidates should be chosen by:

- frequency in common user workflows
- impact on existing docs and examples
- ability to reduce direct raw-dictionary traversal
- relationship to save fidelity and preservation goals
- overlap with already existing partial DOM types

In practice, this means:

- pages, resources, fonts, XObjects, annotations, outlines, destinations, and forms should come first
- rarer spec areas should follow only after the access model is stable

## Proposed Next Decisions

Before implementation starts, the following decisions should be made explicitly:

1. whether wrapper properties may ever materialize state on read
2. which naming pattern is preferred for direct/effective/editable access
3. whether wrappers should generally expose mutable live objects or read-only views plus explicit mutation methods
4. which existing DOM classes are stable enough to keep as part of the long-term surface
5. which Tier 1 wrappers are required before documenting the DOM as a primary user story

## Summary

The most valuable DOM work is not a full spec-shaped object graph.

The most valuable DOM work is:

- a small number of highly intentional wrappers
- consistent semantics around direct vs effective access
- typed access to the object families users touch most often
- Arlington used as metadata and validation support rather than as the public API blueprint
