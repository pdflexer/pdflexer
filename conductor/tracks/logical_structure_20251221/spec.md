# Track Spec: User-Friendly PDF Logical Structure (Tagged PDF)

## Overview
Implement a high-level, fluent API for creating PDF logical structures (Tags). This includes building the hierarchy of structure elements (StructTreeRoot) and linking them to page content via marked content wrappers (BDC/EMC). The goal is to provide a user-friendly abstraction (`StructuralBuilder`) that automates the complexity of ParentTree management and MCID generation.

## Goals
- **Fluent Tree Construction:** A `StructuralBuilder` class to define the document's logical hierarchy.
- **Integrated Page Marking:** seamless integration with `PageWriter` to wrap graphics/text in structural tags.
- **Outline Integration:** Enable bookmarks to link directly to structure elements.
- **Minimalist Simple Model:** maintain an internal tree using a lightweight model before converting to complex PDF objects.

## Functional Requirements
1.  **StructuralBuilder Class:**
    -   Manages the root of the structure tree.
    -   Provides methods like `AddElement(string type, ...)` or specific helpers (`AddParagraph()`, `AddHeader(level)`, etc.).
    -   Maintains a mapping of elements to their content items (MCIDs) across pages.

2.  **StructureNode (Simple Model):**
    -   **Type:** The structure element type (e.g., "P", "H1", "Table").
    -   **ID:** Optional unique identifier.
    -   **Title:** Alternate text or title.
    -   **Language:** Language identifier (e.g., "en-US").
    -   **Attributes:** Support for layout and table attributes.
    -   **ClassMap:** References to predefined style classes.
    -   **Children:** List of nested `StructureNode` objects.

3.  **PageWriter Integration:**
    -   `pageWriter.BeginMarkedContent(StructureNode node)`: Writes the `BDC` operator using the node's type.
    -   Automatically assigns and tracks MCIDs for the current page.
    -   Registers the (Page, MCID) pair back to the `StructureNode`.
    -   `pageWriter.EndMarkedContent()`: Writes the `EMC` operator.

4.  **OutlineBuilder Integration:**
    -   `structureNode.CreateBookmark(string title, OutlineBuilder builder)`: Creates a bookmark in the provided builder that points to this specific structure element.

5.  **Serialization (Build-on-Save):**
    -   Converts the internal simple model tree into `StructTreeRoot`, `K` (kids) arrays, and `ParentTree`.
    -   Handles the generation of indirect references for all structure elements.

## Acceptance Criteria
- Users can build a nested logical structure (e.g., Section -> Header + Paragraph) and link them to content in a few lines of fluent code.
- The resulting PDF passes basic accessibility structure checks (Tags are present and correctly mapped).
- Bookmarks can successfully navigate to structural elements.
- The internal model is decoupled from the low-level PDF object graph until serialization.

## Out of Scope
- Automatic tagging of existing un-tagged PDFs (focus is on creation).
- Support for complex RoleMap remapping (stick to standard tags initially).
- OCR or automated structure inference.
