# Track Spec: Modernized Outline API and Builder

## Overview
Replace the legacy PDF outline (bookmark) implementation with a modern, user-friendly API centered around a new `OutlineBuilder` class. This track prioritizes a clean, fluent interface for constructing document hierarchies and simplifies the data models for bookmarks. Backward compatibility is explicitly not required.

## Goals
- **Fluent API:** Provide an `OutlineBuilder` that allows users to define document structure using chained method calls.
- **Minimalist Models:** Create lightweight models for bookmarks that capture essential properties (Title, Destination, Color, Style, State) without low-level PDF dictionary complexity.
- **Improved Hierarchy Management:** Enable easy creation of nested sections and individual bookmarks.
- **Clean Slate:** Remove all unused components of the old outline implementation to reduce codebase bloat.

## Functional Requirements
1.  **OutlineBuilder Class:**
    -   User-facing class for building the outline tree.
    -   `AddSection(string title, ...)`: Creates a hierarchical grouping node. Returns a fluent context for adding children.
    -   `AddBookmark(string title, PdfPage page, ...)`: Adds a leaf node linking to a specific page.
    -   Supports fluent chaining for rapid structure definition.
    -   Includes methods for traversing the internal tree during construction or for inspection.

2.  **User-Friendly Models:**
    -   **Title:** String.
    -   **Destination:** Supports Page destinations (default), Remote GoTo, or specific Actions.
    -   **Appearance:** Optional Color and Style (Bold/Italic).
    -   **State:** Boolean for Open/Closed (default open).
    -   **Children:** A simple list of child nodes.

3.  **Refactoring / Cleanup:**
    -   Remove `PdfLexer.DOM.PdfOutline` (if obsolete).
    -   Remove `PdfLexer.DOM.PdfOutlineItem` and `PdfLexer.DOM.PdfOutlineRoot` if they no longer fit the new model (or refactor them to be internal implementation details of the builder).
    -   Update `PdfDocument.Outlines` to utilize the new model.

## Acceptance Criteria
- Users can create a 3-level deep outline hierarchy using only the `OutlineBuilder` in a few lines of code.
- Outlines are correctly preserved and rendered when the document is saved.
- All code related to the discarded outline implementation is removed.
- Unit tests verify the fluent API, tree traversal, and final serialization.

## Out of Scope
- Support for complex JavaScript actions in bookmarks (keep to standard GoTo/Actions).
- Named destination resolution logic (reuse existing parser logic if possible, but keep model simple).
