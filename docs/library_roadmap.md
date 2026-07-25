# Library Roadmap

This roadmap translates the review findings into a prioritized execution plan.

Focused sub-roadmaps and design docs:

- [`docs/access_api_consistency_plan.md`](/workspace/docs/access_api_consistency_plan.md): wrapper vs raw access semantics
- [`docs/dom_surface_roadmap.md`](/workspace/docs/dom_surface_roadmap.md): higher-level PDF DOM wrappers
- [`docs/content_model_roadmap.md`](/workspace/docs/content_model_roadmap.md): content model direction, focused primarily on mutation-oriented and provenance-aware workflows
- [`docs/semantic_text_roadmap.md`](/workspace/docs/semantic_text_roadmap.md): semantic text handling on top of the content model for search/replace and commit-back editing
- [`docs/semantic_extract_roadmap.md`](/workspace/docs/semantic_extract_roadmap.md): scanner-based high-level structured text extraction for read-only, performance-oriented workflows

## Positioning

The broader library plan should treat content access as two complementary tracks:

- scanners are the extraction-first path, including high-performance structured text extraction
- the content model is the mutation-first path, including provenance-aware semantic editing

Those tracks should share semantic grouping and layout logic where practical, but they should not be forced into one API surface with conflicting performance and fidelity goals.

## Priority Scale

- `P0`: Immediate. Protect correctness and user trust.
- `P1`: Near-term. Needed before broader adoption or API stabilization.
- `P2`: Important, but can follow once core fidelity issues are addressed.
- `P3`: Nice to have or opportunistic.

## Effort Scale

- `S`: Small
- `M`: Moderate
- `L`: Large
- `XL`: Multi-phase

## Phase 1: Stabilize Core Writer Behavior

### 1. Stabilize the write pipeline architecture

- `Priority`: P0
- `Effort`: XL
- `Outcome`: Saving becomes structurally reliable for repeated writes and large documents.

Work:

- Finish page tree handling
- Revisit object identity and xref reuse model
- Re-enable or replace quick-save path safely
- Add repeated-save and cross-document copy regression tests

## Phase 2: Fix API Semantics and Developer Experience

### 2. Replace ambient `ParsingContext.Current` with a clearer access pattern

- `Priority`: P1
- `Effort`: M
- `Outcome`: Access APIs use explicit or clearly scoped parsing configuration/services without implying document-owned state.

Work:

- Stop presenting `PdfDocument.Context` as document-specific state
- Introduce a clearer pattern for passing parse configuration/services to scanners, content-model readers, and low-level traversal helpers
- Keep `ParsingContext` focused on configuration, parser instances, and caches
- Add tests for multiple documents and async/concurrent usage

### 3. Unify high-level DOM access and low-level native object traversal

- `Priority`: P1
- `Effort`: L
- `Outcome`: Users have one coherent mental model for document traversal, with clear rules for when to use wrappers versus raw `PdfDictionary`/`IPdfObject` access.

Work:

- Decide and document the primary access style:
  - wrapper-first with raw DOM escape hatches, or
  - raw DOM first with convenience wrappers layered on top
- Make `PdfPage`, `PdfDocument`, and related wrappers expose a consistent access surface
- Remove or reduce examples that mix wrapper and raw traversal styles without explanation
- Add a short “access model” guide with recommended patterns for common tasks

### 4. Make read access non-mutating by default

- `Priority`: P1
- `Effort`: M
- `Outcome`: Inspecting a page/document does not silently modify it.

Work:

- Split read-only getters from materializing helpers on `PdfPage`
- Replace write-on-read behavior with explicit `GetOrCreate...` APIs where needed
- Audit wrapper properties that currently create defaults during inspection
- Add regression tests proving read-only traversal does not dirty the DOM

### 5. Simplify and standardize dictionary access naming

- `Priority`: P1
- `Effort`: M
- `Outcome`: Dictionary/object traversal APIs are easier to learn and more internally consistent.

Work:

- Define one preferred set of access verbs for:
  - optional typed access
  - required typed access
  - raw object access
  - object resolution
- Reduce overlap between `Get<T>`, `GetOptionalValue<T>`, `GetRequiredValue<T>`, `TryGetValue<T>`, `GetAs<T>`, `GetAsOrNull<T>`, and `Resolve()`
- Mark aliases or legacy forms as secondary if they must remain for compatibility
- Update examples and XML docs to consistently use the preferred path

### 6. Reduce and harden the public API surface

- `Priority`: P1
- `Effort`: L
- `Outcome`: Public surface better matches what is actually supported and stable.

Work:

- Inventory all public types
- Internalize incomplete or parser-only types where possible
- Mark unstable APIs clearly where internalization is not yet possible
- Add API review notes for next release

### 7. Clarify content access positioning across scanners, COM, and DOM helpers

- `Priority`: P1
- `Effort`: M
- `Outcome`: Users understand which surface is intended for extraction, which is intended for mutation, and where higher-level semantics should live.

Work:

- Document scanners as the primary performance-oriented extraction path
- Document the content model as the primary semantic mutation/editing path
- Ensure roadmap/docs/examples do not imply that the COM should replace scanners for raw extraction throughput
- Reference the specialized roadmaps for:
  - COM evolution
  - semantic text for editing
  - structured text extraction for scanner-based workflows

### 8. Make the test suite self-contained

- `Priority`: P1
- `Effort`: M
- `Outcome`: CI signal reflects source quality rather than machine setup.

Work:

- Gate `pdfcpu`-dependent tests behind explicit traits or environment checks
- Add native fallback assertions for syntax validation where practical
- Document external test prerequisites clearly
- Consider separate validation job for tool-dependent tests

## Phase 3: Document Current Save Tradeoffs and Extend Support Selectively

### 9. Document current save behavior clearly

- `Priority`: P1
- `Effort`: S
- `Outcome`: Users understand that save is optimized for page-centric production workflows and not full catalog preservation.

Work:

- Add README warning for stripped `/Names`, `/StructTreeRoot`, and encryption behavior
- Document the production-oriented rationale for the current save path
- Add explicit notes on when saved output should be validated carefully

### 10. Rebuild the access and traversal docs

- `Priority`: P1
- `Effort`: M
- `Outcome`: Documentation accurately reflects the actual access APIs and recommended usage.

Work:

- Fix broken or misleading examples in `docs/basics.md` and `docs/agent_instruction.md`
- Add side-by-side examples for:
  - high-level wrapper access
  - raw `NativeObject` / `PdfDictionary` traversal
  - indirect-object resolution
- Remove examples that reference non-existent APIs
- Add a short “read-only access vs materializing access” note once the API is cleaned up
- Add an explicit guide to when to use:
  - wrappers / DOM helpers
  - scanners
  - content model
- Cross-link the focused roadmaps and task-oriented docs so users can discover the intended access path quickly

### 11. Extend existing structural tree support for saved output

- `Priority`: P2
- `Effort`: L
- `Outcome`: Existing structure-tree work is carried further so tagged-PDF output can be rebuilt more completely when needed.

Work:

- Preserve more existing structure-tree semantics during page copy/reorder workflows where practical
- Improve reconstruction of `/StructTreeRoot` and related entries from in-memory structural data
- Add round-trip tests for tagged PDFs that rely on the current structural tree implementation

### 12. Add support for named destinations on save

- `Priority`: P2
- `Effort`: M
- `Outcome`: Documents that rely on `/Names` -> `/Dests` can be rewritten without losing destination mappings.

Work:

- Preserve or rebuild name-tree-backed destinations
- Add save tests for documents with named destinations

### 13. Add support for embedded files and attachments on save

- `Priority`: P2
- `Effort`: L
- `Outcome`: File attachments referenced through catalog name trees survive save operations.

Work:

- Preserve or rebuild `/Names` -> `/EmbeddedFiles`
- Add tests for attached-file round trips

### 14. Add support for JavaScript and other name-tree-backed catalog data

- `Priority`: P3
- `Effort`: L
- `Outcome`: Additional `/Names` content can be preserved for broader compatibility when needed.

Work:

- Identify targeted `/Names` branches worth supporting beyond destinations and embedded files
- Add preservation/rebuild support incrementally
- Keep these features optional if they conflict with page-centric writer simplicity

### 15. Make encrypted save behavior explicit

- `Priority`: P3
- `Effort`: M
- `Outcome`: Encryption handling is clearly documented even if encryption-preserving save remains out of scope.

Work:

- Document current decrypt-on-save behavior
- Decide whether to keep it as-is, reject it explicitly, or later add preservation modes
- Add tests covering current expected behavior

## Phase 4: Refresh Docs, Packaging, and Release Discipline

### 16. Rebuild the support matrix and docs

- `Priority`: P2
- `Effort`: M
- `Outcome`: Docs accurately describe what works, what is partial, and what is intentionally unsupported.

Work:

- Update README feature matrix
- Replace stale "major gaps" list with a current support table
- Fix broken examples and clarify save semantics, encryption behavior, and source-document lifetime rules
- Add "known limitations" pages for image extraction, content mutation, and saving
- Add a short architecture map that points users to:
  - DOM wrappers
  - scanners
  - content model
  - structured text extraction
  - semantic text editing

### 17. Normalize package and dependency management

- `Priority`: P2
- `Effort`: S
- `Outcome`: Cleaner release artifacts and lower maintenance overhead.

Work:

- Add package readmes to shipped NuGet packages
- Centralize package versions
- Resolve fragmented `ImageSharp` versions
- Review prerelease dependencies in shipping tools

### 18. Burn down warning debt

- `Priority`: P2
- `Effort`: S
- `Outcome`: Release builds are cleaner and safer to evolve.

Work:

- Fix nullable warnings in production code
- Enable stricter warning policies for core projects if feasible
- Track build cleanliness as a release gate

## Phase 5: Add Missing Product Features Users Expect

### 19. Add high-level AcroForm support

- `Priority`: P2
- `Effort`: XL
- `Outcome`: Users can inspect and edit forms without raw dictionary manipulation.

Work:

- Model form fields and widget annotations
- Provide field enumeration and value update APIs
- Support appearance regeneration where practical

### 20. Add incremental update support

- `Priority`: P2
- `Effort`: XL
- `Outcome`: Updates can preserve more original structure and support signature-sensitive workflows.

Work:

- Append-only write mode
- Incremental xref/trailer generation
- Tests for preserving untouched document structures

### 21. Improve annotation and attachment APIs

- `Priority`: P2
- `Effort`: L
- `Outcome`: Common PDF workflows become accessible without low-level object surgery.

Work:

- Higher-level annotation model
- Attachment/name-tree convenience APIs
- Safer manipulation of popup, link, text, and file attachment annotations

### 22. Expand image and colorspace support

- `Priority`: P2
- `Effort`: XL
- `Outcome`: Image extraction/replacement covers more real-world PDFs.

Work:

- Improve `JBIG2` and `JPX` handling
- Add better `ICCBased` and `Separation` support
- Formalize image replacement/downsampling workflows

### 23. Mature mutable content editing

- `Priority`: P2
- `Effort`: XL
- `Outcome`: The library's differentiator becomes reliable enough for broader adoption.

Work:

- Stabilize content model APIs
- Continue the mutation-oriented COM work tracked in [`docs/content_model_roadmap.md`](/workspace/docs/content_model_roadmap.md)
- Continue semantic text editing work tracked in [`docs/semantic_text_roadmap.md`](/workspace/docs/semantic_text_roadmap.md)
- Improve edit/write fidelity for text, graphics, and marked content
- Expand tests around editing existing documents, not just generated ones

### 24. Add scanner-based high-level structured text extraction

- `Priority`: P2
- `Effort`: XL
- `Outcome`: The library offers fast, high-level extraction helpers beyond characters and words without requiring the content model for read-only workflows.

Work:

- Build scanner-based structured text extraction as tracked in [`docs/semantic_extract_roadmap.md`](/workspace/docs/semantic_extract_roadmap.md)
- Add line, paragraph, block, region, and reading-order helpers
- Share clustering and layout logic with COM-backed semantic text handling
- Benchmark extraction throughput separately from COM-based semantic workflows

## Suggested Release Sequence

### Release A: Writer Stability

- Stabilize core writer behavior
- Replace ambient-context access with a clearer pattern
- Unify DOM/native traversal and dictionary-access naming
- Make tests self-contained

### Release B: Document Current Tradeoffs and Extend Selective Save Support

- Document current save behavior clearly
- Rebuild access and traversal docs
- Extend structural tree support already in progress
- Add support for named destinations and embedded files as needed

### Release C: API and Release Cleanup

- Reduce public API surface
- Refresh docs and support matrix
- Normalize packaging and dependency management

### Release D: Expected User Features

- AcroForm support
- Incremental update support
- Better annotation/attachment APIs
- Expanded image coverage

### Release E: Content Semantics and Editing

- Mature mutation-oriented content editing
- Advance semantic text editing on top of the content model
- Add scanner-based high-level structured text extraction
- Share semantic grouping/layout logic across extraction and editing surfaces

## Backlog Candidates

These are useful, but should not outrank round-trip safety:

- Additional CLI capabilities in `pdfctl`
- More notebooks/examples
- Benchmark expansion after writer semantics stabilize
- Further optimization of lazy parsing and object stream behavior
