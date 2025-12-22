# Track Plan: User-Friendly PDF Logical Structure (Tagged PDF)

## Phase 1: Simple Model & Core Builder [checkpoint: 9f86552]
- [x] Task: Define the `StructureNode` class (Type, ID, Title, Lang, Attributes, Children). [df58d29]
- [x] Task: Implement `StructuralBuilder` for hierarchical tree construction with fluent methods. [327877e]
- [x] Task: Create unit tests for building complex logical trees in memory. [7f0b276]
- [ ] Task: Conductor - User Manual Verification 'Phase 1: Simple Model & Core Builder' (Protocol in workflow.md)

## Phase 2: PageWriter & Content Marking
- [x] Task: Update `PageWriter` to track MCIDs and maintain a mapping of current structural context. [121db5f]
- [x] Task: Implement `pageWriter.BeginMarkedContent(StructureNode node)` and `pageWriter.EndMarkedContent()`. [121db5f]
- [x] Task: Implement automatic registration of (Page, MCID) within the `StructureNode` simple model. [121db5f]
- [x] Task: Add support for marking content as 'Artifact' (non-structural) via `pageWriter`. [121db5f]
- [x] Task: Write TDD tests verifying generated content streams contain correct `BDC` and `EMC` operators. [121db5f]
- [~] Task: Conductor - User Manual Verification 'Phase 2: PageWriter & Content Marking' (Protocol in workflow.md)

## Phase 3: Serialization & PDF Mapping
- [ ] Task: Implement `StructureNode` to PDF dictionary conversion logic.
- [ ] Task: Implement the `ParentTree` builder to map page MCIDs back to structure element indirect references.
- [ ] Task: Hook `StructuralBuilder` into the `PdfDocument.Save` process to generate `StructTreeRoot`.
- [ ] Task: Write end-to-end tests for a basic tagged PDF and verify with external tools (e.g., pdfcpu or structural validators).
- [ ] Task: Conductor - User Manual Verification 'Phase 3: Serialization & PDF Mapping' (Protocol in workflow.md)

## Phase 4: Integration & Advanced Attributes
- [ ] Task: Implement `structureNode.CreateBookmark(string title, OutlineBuilder builder)` for direct structural linking.
- [ ] Task: Add support for complex attributes (Tables, Layout) in the model and serialization.
- [ ] Task: Final API refinement and cleanup of low-level structure internal types if any.
- [ ] Task: Comprehensive integration tests for multi-page tagged documents with nested structures and bookmarks.
- [ ] Task: Conductor - User Manual Verification 'Phase 4: Integration & Advanced Attributes' (Protocol in workflow.md)
