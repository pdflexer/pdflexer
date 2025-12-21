# Track Plan: Enhance Outline/Bookmark Support and API Refinement

## Phase 1: High-Level Data Model & Parsing [checkpoint: d6e6620]
- [x] Task: Create `PdfOutline` class (Title, Section, Order, Style, Color). [6c920a6]
- [x] Task: Update `PdfPage` to hold `List<PdfOutline>` instead of `PdfOutlineItem`. [e0712f9]
- [x] Task: Implement `PdfPage.AddBookmark` helper methods. [04d82ed]
- [x] Task: Update `OutlineParser` to parse existing PDF outlines into the new `PdfOutline` model and attach them to pages. [452a0a6]
- [x] Task: Conductor - User Manual Verification 'Phase 1: High-Level Data Model & Parsing' (Protocol in workflow.md) [d6e6620]

## Phase 2: Serialization Logic (The "Build-on-Save" Engine)
- [x] Task: Create `OutlineBuilder` (or method in `PdfDocument.Saving.cs`) to aggregate `PdfOutline`s from all pages. [cab90b2]
- [ ] Task: Implement logic to group outlines by `Section` path.
- [ ] Task: Implement sorting logic: First by `Order` (if present), then by Page Index.
- [ ] Task: Implement conversion from the grouped/sorted model to the low-level `PdfOutlineItem` tree (dictionaries).
- [ ] Task: Hook this logic into `PdfDocument.SaveTo`.
- [ ] Task: Conductor - User Manual Verification 'Phase 2: Serialization Logic' (Protocol in workflow.md)

## Phase 3: Integration & Validation
- [ ] Task: Write tests for **Page Copying**: Verify that copying a `PdfPage` to a new `PdfDocument` preserves its bookmarks in the correct hierarchy.
- [ ] Task: Write tests for **Mixed Ordering**: Verify behavior when some items have `Order` and others don't.
- [ ] Task: Write tests for **Deep Nesting**: Verify deeply nested sections `["A", "B", "C", "D"]`.
- [ ] Task: Conductor - User Manual Verification 'Phase 3: Integration & Validation' (Protocol in workflow.md)
