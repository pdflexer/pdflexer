# Track Spec: Enhance Outline/Bookmark Support and API Refinement

## Overview
Refine the existing outline (bookmark) implementation to move towards a **page-centric model** using a high-level, user-friendly abstraction. Bookmarks will be associated with pages using a simple `PdfOutline` class. The complex, low-level PDF outline tree (dictionaries, linked lists) will be generated dynamically during the save process.

## Goals
- **Simplified User API:** Users interact with a simple `PdfOutline` model on the `PdfPage` (e.g., Title, Section/Path, Order).
- **Page Portability:** Outlines are properties of the page, so copying a page copies its bookmarks.
- **Automatic Tree Generation:** The library handles the complexity of building the PDF outline hierarchy (`First`, `Last`, `Next`, `Prev`, `Parent`) at save time.
- **Flexible Ordering:** Outlines are ordered by page order by default, but can be manually ordered using an `Order` property.

## Requirements
1.  **High-Level Model (`PdfOutline`):**
    -   `Title` (string): The text of the bookmark.
    -   `Section` (List<string>?): The hierarchical path (e.g., `["Chapter 1", "Section 1.1"]`). If null/empty, it's a top-level item.
    -   `Order` (int?): Optional sorting order within the section.
    -   `Destination`: Implicitly the page it is attached to.
    -   `Color` (optional): Text color.
    -   `Style` (optional): Bold/Italic.

2.  **Page Integration:**
    -   `PdfPage` has a `List<PdfOutline> Outlines` property.
    -   Helper methods: `page.AddBookmark("Title", params string[] section)`.

3.  **Parsing:**
    -   `OutlineParser` reads the existing PDF outline tree and converts it into `PdfOutline` objects attached to the correct `PdfPage`.

4.  **Serialization (Build-on-Save):**
    -   `PdfDocument.Save` triggers `BuildOutlineTree`.
    -   Logic:
        -   Iterate all pages in the document.
        -   Collect all `PdfOutline` objects.
        -   Group them by their `Section` path.
        -   Sort them based on Page Index and `Order`.
        -   Construct the `PdfOutlineItem` / `PdfDictionary` tree structure.
        -   Update `/Count`, `/First`, `/Last`, `/Next`, `/Prev`, `/Parent` links.

5.  **Destination Support:** 
    -   Primary support for Page destinations.
    -   Future support for Named Destinations / Remote GoTo (out of scope for this phase's core data model, but `PdfOutline` should be extensible).
