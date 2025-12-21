# Track Plan: Modernized Outline API and Builder

## Phase 1: Deconstruction & Basic Models [checkpoint: 1b9a01b]
- [x] Task: Remove legacy outline implementation classes (`PdfOutlineItem.cs`, `PdfOutlineRoot.cs`, and existing `PdfOutline.cs` if incompatible). [b3207a7]
- [x] Task: Create new minimalist `BookmarkNode` model (Title, Destination, Color, Style, IsOpen, Children). [f1bf657]
- [x] Task: Update `PdfPage.Outlines` and `PdfDocument.Outlines` to use the new model. [a2da45d]
- [x] Task: Conductor - User Manual Verification 'Phase 1: Deconstruction & Basic Models' (Protocol in workflow.md) [1b9a01b]

## Phase 2: Fluent OutlineBuilder Implementation
- [ ] Task: Implement `OutlineBuilder` class with a fluent interface.
- [ ] Task: Implement `AddSection(string title)` and `AddBookmark(string title, PdfPage? page = null)` with chaining support.
- [ ] Task: Implement internal tree traversal helpers (e.g., `FindNode`, `MoveToParent`, `GetRoot`, `EnumerateLeaves`).
- [ ] Task: Write TDD tests for fluent tree construction and traversal.
- [ ] Task: Conductor - User Manual Verification 'Phase 2: Fluent OutlineBuilder Implementation' (Protocol in workflow.md)

## Phase 3: Serialization Engine Refactor
- [ ] Task: Refactor `PdfLexer.Writing.OutlineBuilder` (the internal serialization engine) to consume the new `BookmarkNode` tree.
- [ ] Task: Update `PdfDocument.Saving.cs` to correctly hook the new builder into the save process.
- [ ] Task: Write tests for end-to-end saving of complex hierarchies.
- [ ] Task: Conductor - User Manual Verification 'Phase 3: Serialization Engine Refactor' (Protocol in workflow.md)

## Phase 4: Parser & Final Integration
- [ ] Task: Update `OutlineParser` to parse existing PDF outlines into the new `BookmarkNode` tree.
- [ ] Task: Final cleanup of any remaining legacy outline code or obsolete properties.
- [ ] Task: Write integration tests for page copying and mixed bookmark types (Remote/Action).
- [ ] Task: Conductor - User Manual Verification 'Phase 4: Parser & Final Integration' (Protocol in workflow.md)
