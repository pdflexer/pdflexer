This guide provides a technical checklist for ensuring electronic documents conform to the **ISO 14289-1:2014 (PDF/UA-1)** specification.

---

# PDF/UA-1 Conformance Specification for AI Agents

## 1. Metadata & Identification
* **Version Identification:** Specify PDF/UA version and conformance in the Metadata stream of the Catalog.
    * **Namespace:** `http://www.aiim.org/pdfua/ns/id/` (Prefix: `pdfuaid`).
    * **Property:** Set `pdfuaid:part` to `1`.
* **Document Title:** The Metadata stream must contain a `dc:title` entry.
* **Viewer Preferences:** The `ViewerPreferences` dictionary must have `DisplayDocTitle` set to `true`.

## 2. General Structural Requirements
* **Tagging:** All "real content" (semantically significant graphics/text) must be tagged.
* **Artifacts:** Content with no semantic value (e.g., decorative elements) must be marked as artifacts and **not** included in the structure tree.
* **Reading Order:** Tags must follow a logical reading order.
* **Role Mapping:** Map non-standard structure types to the nearest functionally equivalent standard type defined in ISO 32000-1.
* **Forbidden Content:** Do not use flickering, blinking, or flashing elements.

## 3. Text & Language
* **Unicode Mapping:** All character codes must map to Unicode.
* **Language Declaration:** Declare the primary natural language of the document; declare any internal changes in natural language.
* **Special Characters:** Use `ActualText` for stretchable characters (e.g., large brackets in math).

## 4. Specific Content Elements
### Headings
* **Hierarchy:** Use `H1` through `Hn`.
* **Strict Order:** If headings descend, they must proceed in strict numerical order (e.g., `H1` to `H2`, not `H1` to `H3`).
* **Entry Point:** `H1` must be the first heading tag used.

### Graphics & Math
* **Figures:** Tag graphics with `Figure` and provide an `Alt` (alternative representation) or replacement text.
* **Captions:** Accompanying captions must use the `Caption` tag.
* **Formulae:** Mathematical expressions must be enclosed in a `Formula` tag with an `Alt` attribute.

### Tables & Lists
* **Table Headers:** Use `TH` tags for headers.
* **Scope:** `TH` elements should have a `Scope` attribute (required if structure is not determinable via Headers/IDs).
* **List Structure:** Use `L` (List), `LI` (List Item), `Lbl` (Label), and `LBody` (List Body) tags.
* **Ordered Lists:** Must include the `ListNumbering` attribute for `L` tags.

### Notes & Navigation
* **Notes:** Tag footnotes and endnotes with `Note` and assign a unique `ID`.
* **Outline:** Include a document outline (bookmarks) that matches the logical reading order.
* **Headers/Footers:** Identify running headers and footers as `Pagination` artifacts with `Header` or `Footer` subtypes.

## 5. Technical & Interactive Constraints
* **Fonts:** * Embed all font programs used for rendering.
    * Include a `ToUnicode` entry for all fonts.
    * Never reference the `.notdef` glyph in content streams.
* **Annotations:** * Represent annotations in the structure tree in reading order.
    * Set the page `Tabs` key value to `S` (Structure) to define tab order.
* **Forms:** * Nest `Widget` annotations within `Form` tags.
    * Dynamic XFA forms are **forbidden**; static XFA is permitted.
* **Security:** If encrypted, the 10th bit of the `P` key must be set to `true` to allow Assistive Technology access.
* **XObjects:** Do not use `Reference XObjects`.