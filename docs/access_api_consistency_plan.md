# Access API Consistency Plan

This document proposes a focused plan to improve the usability of PDF document access in `pdflexer`.

The goals are:

1. unify and create consistency between the high-level DOM and low-level native PDF object traversal models
2. simplify and standardize dictionary/object access naming
3. replace the current ambient `ParsingContext.Current` access pattern with a clearer model without treating parsing context as document-owned state

## Background

The library currently exposes two valid but only loosely coordinated access styles:

- high-level wrappers such as `PdfDocument`, `PdfPage`, `page.Resources`, `page.Contents`, `page.GetTextScanner()`
- low-level traversal through `PdfDictionary`, `IPdfObject`, `PdfIndirectRef`, `Resolve()`, and typed access helpers

This is powerful, but it creates usability issues:

- users are not given a single recommended traversal style
- some wrapper properties mutate the document during read access
- docs mix the two styles inconsistently
- dictionary/object helper naming is broader than necessary for most users
- access convenience methods depend on `ParsingContext.Current`, which makes behavior feel ambient rather than explicit

## Design Principles

### 1. Wrapper-first, raw-DOM escape hatch

The high-level story should be:

- use `PdfDocument` and `PdfPage` for common access
- drop down to `NativeObject` and `PdfDictionary` only when you need raw PDF traversal or unsupported features

The raw DOM remains a first-class capability, but not the primary teaching surface.

### 2. Wrapper reads should prioritize usable semantics

High-level wrapper APIs should default to the value a caller would expect by PDF specification rules, including inheritance and defaulting where relevant.

For mutable wrapper properties, usability is more important than a strict “read should not write” rule. If a wrapper property returns a mutable live PDF object and callers are expected to edit it, the property should materialize direct page state before returning it so subsequent mutations actually affect that page.

For value-like reads, non-mutating effective access is still preferred when it does not create ambiguity.

### 3. One preferred naming path

There can be compatibility aliases, but the docs and examples should consistently use one preferred access vocabulary.

### 4. Parsing context is a service/configuration object, not document state

`ParsingContext` currently holds:

- configuration
- parser instances
- caches

It should not be modeled as state owned by `PdfDocument`. The usability problem is the ambient `Current` pattern, not the fact that contexts can be shared.

## Proposed Plan

## Part 1: Unify High-Level DOM and Low-Level Traversal

### A. Define the official traversal model

Document the following guidance explicitly:

- use `PdfDocument` to open/create/save
- use `PdfPage` for common page operations and common page properties
- use `page.NativeObject` for unsupported or low-level access
- use `PdfDictionary` and `IPdfObject` APIs when traversing raw PDF structures

### B. Make wrapper APIs intentionally parallel the raw model where practical

Examples:

- page wrapper methods should expose the most common read operations
- raw DOM should remain available through `NativeObject`
- wrapper examples should clearly note when the user is leaving the high-level model

### C. Make wrapper mutation semantics explicit

Current examples that should become explicit:

- `page.Resources`
- `page.MediaBox`
- `page.CropBox`
- `page.Rotate`

Proposed shape:

- mutable wrapper properties return an editable direct object and may materialize inherited/defaulted state before returning it
- direct/raw accessor returns only the physically present value
- explicit effective/read-only accessor is available when callers want to inspect inherited/defaulted state without materializing
- explicit materializing/normalizing helper can still exist where it adds value

Examples of the intended split:

- `page.Resources`
  returns editable page resources for normal usage and materializes them onto the page if needed
- `page.TryGetDirectResources(...)`
  returns only `/Resources` physically present on the page
- `page.GetEffectiveResources(...)`
  returns the effective resources without promising an editable direct object
- `page.EnsureResources()`
  materializes resources into the page dictionary if needed

- `page.CropBox`
  returns an editable direct crop box and materializes the effective crop box onto the page if needed
- `page.TryGetDirectCropBox(...)`
  returns only a directly stored `/CropBox`
- `page.GetEffectiveCropBox(...)`
  returns the effective crop box using spec fallback rules without requiring callers to know the fallback chain
- `page.MaterializeCropBox()`
  writes the effective crop box into `NativeObject`

- `page.Rotate`
  returns the effective rotate value, defaulting per spec/inheritance rules
- `page.TryGetDirectRotate(...)`
  returns only the directly stored `/Rotate`
- `page.EnsureRotate()`
  writes the effective value explicitly

The exact names can vary, but the important distinction is:

- editable wrapper access for normal usage
- direct access for low-level/raw traversal
- explicit effective access for inspection
- explicit materialization/normalization where needed

The exact names can vary, but the split between editable wrapper access, direct/raw access, and explicit effective access should be explicit.

### D. Add “effective” versus “direct” access where inheritance/defaulting matters

Some page values are inherited or defaulted conceptually.

Recommended pattern:

- direct/raw access: returns only what is physically present
- editable wrapper access: returns the value callers are expected to use in normal code, materializing direct page state when necessary so mutations persist on that page
- effective access: resolves inherited/defaulted values for inspection without requiring callers to know the spec fallback chain
- materializing access: writes defaults into the page when the caller explicitly wants normalization

This is especially relevant for:

- `Resources`
- `MediaBox`
- `CropBox`
- `BleedBox`
- `TrimBox`
- `ArtBox`
- `Rotate`

The key usability rule should be:

- users should not need to know PDF spec fallback chains just to read a page property
- users should be able to mutate wrapper-returned objects and have those mutations affect the page they are working on
- users should only need to think about “direct” versus “effective” access when they are doing low-level inspection, debugging, or normalization work

In other words, wrapper properties should remain friendly and spec-aware, and when they return mutable editable objects they should behave in the way callers would naturally expect.

### E. Document wrapper behavior explicitly

This behavior must be stated directly in the docs and XML comments rather than left implicit.

Required documentation points:

- whether a wrapper property returns an editable direct object or an effective read-only value
- whether reading a mutable wrapper property may materialize inherited/defaulted state onto the page
- how to get the directly stored value only
- how to inspect the effective spec value without materializing
- how to explicitly normalize/materialize when desired

This is especially important for `Resources`, `MediaBox`, `CropBox`, `BleedBox`, `TrimBox`, `ArtBox`, and `Rotate`, because users will otherwise make incorrect assumptions about whether edits to returned objects are persisted.

## Part 2: Simplify Dictionary/Object Access Naming

### A. Pick one preferred naming set

Current preferred names, reflecting the decisions already made:

- `dict.Get<T>(key)` for optional typed dictionary access
- `dict.GetRequiredValue<T>(key)` for required typed dictionary access
- `dict.Get(key)` for optional raw dictionary access
- `dict.GetRequiredValue(key)` for required raw dictionary access
- `dict.TryGetValue(key, out value)` as a secondary raw dictionary helper
- `dict.TryGetValue<T>(key, out value, errorOnMismatch: ...)` as a secondary typed dictionary helper
- `obj.GetAs<T>()` for required object-level typed access once you already have an `IPdfObject`
- `obj.GetAsOrNull<T>()` for optional object-level typed access once you already have an `IPdfObject`
- `Resolve()` when callers explicitly want the resolved underlying object

This is no longer just aspirational direction. Docs and examples should align to this vocabulary unless there is a specific compatibility reason not to.

### B. De-emphasize overlapping aliases

Today users must choose between:

- `Get<T>`
- `GetOptionalValue<T>`
- `GetRequiredValue<T>`
- `TryGetValue<T>`
- `GetValue<T>`
- `GetAs<T>`

Decisions made:

- keep compatibility where needed
- mark the `Get*` / `GetRequiredValue*` dictionary helpers as preferred in XML docs and examples
- treat `TryGetValue*` as secondary helpers rather than the primary teaching surface
- prefer `GetAs<T>` / `GetAsOrNull<T>` for object-level casts
- deprecate `GetValue<T>` / `GetValueOrNull<T>` in favor of `GetAs<T>` / `GetAsOrNull<T>`
- treat `GetOptionalValue<T>` as legacy/secondary alongside the other overlapping aliases

### C. Clarify raw object access versus typed access

Recommended distinction, matching the current implementation and docs:

- typed dictionary access (`Get<T>`, `GetRequiredValue<T>`, `TryGetValue<T>`) auto-resolves indirect references and enforces type expectations
- raw dictionary access (`Get(key)`, `GetRequiredValue(key)`, `TryGetValue(key, out value)`) returns the underlying `IPdfObject` without changing the surface vocabulary to imply a cast
- object-level typed access should use `GetAs<T>` / `GetAsOrNull<T>` so it is visually clear that the operation applies to the current object, not to a dictionary entry
- `Resolve()` remains available when callers want the resolved underlying object and will handle type discrimination themselves

This should be described once and used consistently everywhere.

Special note for `TryGetValue<T>`:

- it is a secondary helper
- it auto-resolves like the other typed dictionary helpers
- its `errorOnMismatch` parameter defaults to `true`
- callers who want classic try-get behavior should pass `errorOnMismatch: false`

This default must be called out explicitly anywhere `TryGetValue<T>` is documented so users do not incorrectly assume it is always non-throwing.

### D. Add a short “how to traverse” cheat sheet

Examples:

- “I know the expected type”
- “I need the raw underlying object”
- “I already have an `IPdfObject` and want to cast it”
- “I need the effective spec/defaulted value”
- “I need the directly stored value only”
- “I want to inspect without mutating”
- “I need to create the entry if missing”

The cheat sheet should map these cases to the current preferred names:

- expected dictionary entry type: `dict.Get<T>(key)` or `dict.GetRequiredValue<T>(key)`
- raw dictionary entry: `dict.Get(key)` or `dict.GetRequiredValue(key)`
- out-parameter dictionary access: `dict.TryGetValue(...)` only when that style is specifically desired
- object-level cast: `obj.GetAs<T>()` or `obj.GetAsOrNull<T>()`
- explicit object resolution before manual inspection: `obj.Resolve()`

## Part 3: Clarify `ParsingContext` Without Making Common APIs Heavier

### A. Keep `ParsingContext` shared and configuration-oriented

Do not make it document-owned by default.

Instead, treat it as a reusable service object that can be:

- explicitly created
- optionally passed when advanced control is needed
- used ambiently for convenience where the existing API already relies on it

The goal is not to push common call sites toward context-parameter-heavy APIs. The goal is to keep the convenient surface while making the underlying behavior less misleading.

### B. Keep parameterless convenience APIs as the primary path

Current issue:

- `PdfDocument.Context`
- `page.GetTextScanner()`
- `page.GetWordScanner()`
- `page.GetContentModel()`

all implicitly use `ParsingContext.Current`.

Decision direction:

- keep parameterless convenience methods like `page.GetTextScanner()` and `page.GetWordScanner()` as the primary documented path
- explicit overloads that accept a `ParsingContext` can exist for advanced scenarios, but they should not become the default teaching surface
- documentation should explain when ambient context matters, rather than forcing most callers to thread a context argument through common APIs

Examples:

- `page.GetTextScanner()`
- `page.GetWordScanner()`
- `page.GetContentModel<T>(flattenForms: false)`
- `page.GetTextScanner(ctx)`
- `page.GetWordScanner(ctx)`
- `page.GetContentModel<T>(ctx, flattenForms: false)`

### C. Reduce misleading document-owned context surface

If `ParsingContext` is not document-specific, `PdfDocument.Context` is misleading.

Current preferred direction:

- keep `ParsingContext` itself as a reusable type
- avoid treating it as document-owned state in the public API
- make `PdfDocument.Context` internal if it is only a helper used by implementation details

This removes a misleading public concept without changing the ergonomic parameterless wrapper APIs that users already expect.

### D. Do not add a separate scope helper unless it solves a real problem

There is currently no strong reason to introduce a new `ParsingScope.Use(ctx)` abstraction.

If callers want explicit setup, the existing patterns are already sufficient:

- `using var ctx = new ParsingContext(...)`
- `ParsingContext.Current = ctx`

Adding a second scoped helper concept would increase surface area without clearly improving usability.

### E. Document ambient behavior instead of fighting it

Required documentation points:

- parameterless APIs like `page.GetTextScanner()` remain the normal path
- these methods may use `ParsingContext.Current` internally
- explicit `ParsingContext` overloads are for advanced control, not required day-to-day usage
- `PdfDocument.Context` is implementation detail material and should not be treated as part of the conceptual public model

The aim is to keep the common surface simple while making the context behavior understandable and less misleading.

## Suggested Rollout

### Phase A: Documentation and guidance

- document wrapper-first traversal
- fix examples to compile
- explicitly distinguish raw, typed, and materializing access

### Phase B: Add non-mutating and explicit APIs

- add read-only wrapper accessors
- add explicit materializing methods
- add explicit `ParsingContext` overloads everywhere important

### Phase C: Naming cleanup

- select preferred helper names
- mark secondary aliases as legacy in docs
- update examples and tests

### Phase D: Ambient context de-emphasis

- obsolete or de-emphasize `PdfDocument.Context`
- make explicit context-passing the primary path
- optionally add a scoped helper for ambient convenience

## Near-Term Priority

The highest-value near-term improvements are:

1. keep wrapper properties spec-aware and usable, but stop them from mutating the document on read
2. fix docs so they consistently show wrapper-first access with `NativeObject` escape hatches
3. standardize one preferred dictionary/object access vocabulary
4. add explicit `ParsingContext` overloads and stop treating `PdfDocument.Context` as authoritative document state

## Summary

The library already has the right building blocks. The main usability issue is not lack of capability, but lack of a single coherent access story. A wrapper-first model with explicit raw-DOM escape hatches, non-mutating reads, standardized naming, and explicit parsing-context usage would make the library much easier to learn and much less surprising to use.
