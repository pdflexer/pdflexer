# PDF/UA-1 (ISO 14289-1:2014) Implementation Guide for AI Agents

This guide extracts the critical requirements from the **ISO 14289-1:2014 (PDF/UA-1)** specification. It is intended to be used by an AI agent to generate, validate, or modify PDF documents to ensure they meet universal accessibility standards.

All conforming files must fundamentally adhere to **ISO 32000-1:2008 (PDF 1.7)**. This specification layers additional accessibility constraints on top of the base PDF format.

## 1. Version Identification & Metadata
*   **PDF/UA Identifier:** The metadata stream in the document Catalog must include the PDF/UA Identification extension schema.
    *   Namespace URI: `http://www.aiim.org/pdfua/ns/id/` (Prefix: `pdfuaid`)
    *   `pdfuaid:part` (Integer, Required): Must be `1`.
    *   `pdfuaid:amd` (Text, Optional): Amendment identifier (e.g., amendment number and year separated by colon).
    *   `pdfuaid:corr` (Text, Optional): Corrigenda identifier.
*   **Title:** The `Metadata` stream in the document's catalog dictionary must contain a `dc:title` entry.
*   **Viewer Preferences:** The `ViewerPreferences` dictionary in the `Catalog` MUST be present and must contain `DisplayDocTitle` set to `true`.

## 2. Logical Structure & Tagging Tree
*   **Tag Real Content:** All structural and semantically meaningful content MUST be tagged in logical reading order in the structure tree.
*   **Artifacts:** Any content that is strictly for visual presentation (e.g., background graphics, decorative lines, running headers/footers) MUST be marked as `Artifact` and MUST NOT be present in the structure tree. 
*   **Mapping:** Non-standard tag structure types are allowed but MUST be mapped to functionally equivalent standard types in the role map dictionary. Standard types MUST NOT be remapped.
*   **Suspects:** The `Suspects` key in the `MarkInfo` dictionary must be `false`.

## 3. Text and Natural Language
*   **Unicode Mapping:** All text character codes MUST map to Unicode formats via standard encodings or a `ToUnicode` CMap.
*   **No `.notdef`:** A conforming document MUST NOT contain a reference to the `.notdef` character glyph from any text-showing operator in any content stream.
*   **Language Declaration:** The default natural language must be declared at the document level. Any inline changes in language (including inside alternate descriptions) must also be declared.
*   **Stretchable Characters:** Visually merged characters (e.g., big brackets built from multiple glyphs) must be tagged using the `ActualText` attribute to supply the true meaning.

## 4. Graphics & Figures
*   **Tagging:** All meaningful non-text graphics must be tagged with a `Figure` tag.
*   **Alt Text:** All `Figure` tags MUST include an alternative representation (`Alt` attribute) or replacement text that represents the contents.
*   **Captions:** If a figure has a visual caption, the caption text must be tagged with a `Caption` tag.
*   **Composite Graphics:** Groups of graphics that form a single semantic image must be grouped inside a single `Figure` tag.
*   **Background Graphics:** If a graphic acts as a background for a link, it is an artifact, and the link's alt text should describe both.

## 5. Headings
*   **Strongly vs Weakly Structured:** Documents must be either entirely strongly structured (using user-defined or nested `H` tags) or entirely weakly structured (using `H1`...`Hn`), but never both.
*   **Numbered Headings (H1-H6):**
    *   If any are used, the first must be `H1`.
    *   Heading levels can repeat (e.g., H2, H2).
    *   When descending levels, you MUST strictly step down by 1 (e.g., `H1` -> `H2` -> `H3` is valid; `H1` -> `H3` is invalid).
    *   When ascending back to higher context, you can skip levels (e.g., `H3` -> `H1` is valid).

## 6. Tables & Lists
*   **Tables:** Must include headers. Table headers MUST be tagged with `TH`. `TH` tags should have a `Scope` attribute (and MUST have one if structure differs from simple grids determinable by headers/IDs). Tables must only be used for logically tabular data, not purely for visual layout.
*   **Lists:** Specified by `L` (List) containing `LI` (List Item) blocks. Ordered lists MUST explicitly declare the `ListNumbering` attribute.

## 7. Mathematical Expressions
*   Math expressions must be enclosed in a `Formula` tag, which MUST have an `Alt` attribute with alternative text describing the equation.

## 8. Artifacts: Page Headers, Footers, and PrinterMarks
*   Running page headers and footers MUST be tagged as `Pagination` artifacts and sub-classified as `Header` or `Footer`.
*   File `PrinterMark` annotations (like crop marks) are incidental artifacts.

## 9. Navigation, Links, and Annotations
*   **Outline:** The document should include a document outline (bookmarks) reflecting the logical reading order.
*   **Tab Order:** The `Tabs` key in every page dictionary containing annotations MUST be set to `S` (use the structure tree for tab order).
*   **Links:** Marked with `Link` tags. They MUST contain an alternative description in the `Contents` key. `IsMap` key MUST NOT be true in URI actions unless functionally equivalent fallback text exists.
*   **Annotations:** Must be in the correct logical reading order within the structure tree. Non-structural/hidden annotations must have alternate text in the `Contents` key.

## 10. Form Fields (Interactive & XFA)
*   **Standard Forms:** Widget annotations for form fields must be nested in a `Form` tag and provide text descriptions via Alternate Description/`Contents`.
*   **XFA Forms:** *Dynamic* XFA forms are explicitly FORBIDDEN. Static XFA forms are permitted.

## 11. Optional Content (OCGs) & Embedded Files
*   If Optional Content Groups (layers) are used, config dictionaries must contain a `Name` entry. The `AS` (Auto State) key is forbidden.
*   Embedded files (attachments) must have `F` and `UF` (Unicode File) keys in the file specification dictionary and should include a `Desc` key. The embedded files themselves should independently meet accessibility standards applicable to their types.

## 12. Security Features
*   If the PDF is encryption-protected, the `P` (permissions) key in the encryption dictionary MUST have its **10th bit set to `true`** to ensure Assistive Technologies (AT) can access the content.

## 13. Fonts & Embedding
*   **Embedding:** ALL fonts used for rendering MUST be fully embedded (or subset-embedded). There is no exemption for standard 'Base 14' fonts.
*   **ToUnicode:** The font dictionary for all fonts MUST have a `ToUnicode` CMap mapping glyphs to Unicode values (except for basic specific scenarios like WinAnsiType 1 fonts). Mappings must be > 0 and not equal to Byte Order Marks (`U+FEFF` or `U+FFFE`).
*   **Subset Tracking:** Subset fonts (Type 1 or CID) must precisely identify all present characters using `CharSet` or `CIDSet`.
*   **Metrics Constraint:** Glyph width metrics in the font dictionary and embedded font program MUST match within 1/1000 of a unit.
*   **Exclusion:** Fonts exclusively used in rendering mode 3 (invisible text) are exempt from embedding since they're not rendered.
