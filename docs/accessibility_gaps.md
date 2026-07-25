# Accessibility Gap Analysis & Status: StructuralBuilder

> [!NOTE]
> This is the **round-1** analysis and records the gaps closed during initial PDF/UA authoring work. A later review
> found defects that this document reports as resolved or does not cover at all (text-string encoding, MCID
> allocation, annotation tagging, form widget conformance). Those are tracked in
> [Accessibility Gaps — Round 2](accessibility_gaps_2.md), which supersedes this document where they disagree.

Review of `StructuralBuilder`, `RemediationSession`, and related plumbing for producing/modifying PDFs
that meet PDF/UA-1 (ISO 14289-1) and PDF/UA-2 (ISO 14289-2).

Files reviewed:

- `src/PdfLexer/DOM/StructuralBuilder.cs`
- `src/PdfLexer/DOM/StructureNode.cs`
- `src/PdfLexer/Writing/StructuralSerializer.cs`
- `src/PdfLexer/PdfDocument.Accessibility.cs`
- `src/PdfLexer/Remediation/RemediationSession.cs`
- `src/PdfLexer/Writing/PageWriter.cs`
- `src/PdfLexer/PdfDocument.Saving.cs`
- `src/PdfLexer/PdfDocument.cs`

---

## Executive Summary of Status

The library now features comprehensive **PDF/UA-1 and PDF/UA-2 authoring and rule-driven remediation capabilities**. Most of the initial correctness bugs and missing node/document-level primitives identified in the original gap analysis have been **resolved**:

- ✅ **Fixed**: `/Alt` key case sensitivity bug (`PdfName.Alt`).
- ✅ **Fixed**: Top-level `StructElem` parent link (`/P`) pointing to `StructTreeRoot`.
- ✅ **Implemented**: Node primitives (`/ActualText`, `/E` Expansion, `/Alt` on generic nodes, `/Scope`, `/Headers`, `/ListNumbering`).
- ✅ **Implemented**: Document-level PDF/UA setup (`ApplyAccessibilitySetup` setting `/Lang`, `DisplayDocTitle`, `MarkInfo/Suspects`, page `/Tabs = /S`, XMP `pdfuaid:part` metadata stream).
- ✅ **Implemented**: **Rule-Driven Remediation Engine** (`RemediationSession`, `RuleSet`, `FlowRegion`, `TolerancedZone`, 3-stage pipeline).
- ⏳ **Remaining Gaps**: Editing existing pre-tagged `StructTreeRoot` trees (un-remediated pre-tagged PDF editing), PDF 2.0 MathML/SVG namespace declarations (`/NS`), and complex layout attributes (`/SpaceBefore`, `/BBox` placement).

---

## 1. Resolved Correctness & Formatting Issues

### 1.1 `/Alt` key serialization
- **Status**: **RESOLVED**
- `StructuralSerializer.cs` now correctly writes `dict[PdfName.Alt] = ...` using mixed-case `/Alt` per PDF 32000-1 §14.9.3.

### 1.2 Top-level `StructElem` missing parent link `/P`
- **Status**: **RESOLVED**
- `StructuralSerializer.cs` sets `/P` on root-level structure elements pointing back to `StructTreeRoot`.

### 1.3 Minimal `MarkInfo` & Document-Level Setup
- **Status**: **RESOLVED**
- `ApplyAccessibilitySetup()` initializes `/Marked = true`, `/Suspects = false`, page `/Tabs = /S`, `Catalog/Lang`, `Info/Title`, `ViewerPreferences/DisplayDocTitle`, and embeds XMP metadata with `pdfuaid:part` (1 or 2).

---

## 2. Node Model & Authoring Primitives Status

| Primitive / Attribute | Spec Source | Status | Implementation Details |
| --- | --- | --- | --- |
| **`/Alt`** | PDF 32000 §14.9.3 | ✅ **RESOLVED** | Available on all structure nodes (`node.Alt` / `.SetAlt()`). |
| **`/ActualText`** | PDF 32000 §14.9.4 | ✅ **RESOLVED** | Exposed on `StructureNode` (`node.ActualText`). |
| **`/E` (Expansion text)** | PDF 32000 §14.9.5 | ✅ **RESOLVED** | Exposed on `StructureNode` (`node.Expansion`), fluent `.Expansion(...)`. |
| **Table `/Scope` & `/Headers`** | PDF 32000 §14.8.11 | ✅ **RESOLVED** | Supported on table headers (`TH`) and cells (`TD`). |
| **List `/ListNumbering`** | PDF 32000 §14.8.11 | ✅ **RESOLVED** | Supported on list elements (`L`). |
| **Link Annotation `/OBJR`** | PDF 32000 §14.7.5.4 | ✅ **RESOLVED** | Structure link binding emits object reference (`OBJR`) to annotations. |
| **Artifact Subtyping** | PDF/UA §7.4 | ✅ **RESOLVED** | Artifact actions support subtypes (`Pagination`, `Layout`, `Page`, `Background`). |
| **PDF 2.0 Namespaces (`/NS`)** | PDF/UA-2 §5 | ⏳ **PARTIAL** | Basic PDF 2.0 namespace default emitted; custom MathML/SVG `/NS` trees pending. |

---

## 3. Remediation Engine (`RemediationSession`)

- **Untagged Document Remediation**: Full support for declarative rule sets operating on untagged PDFs via `doc.BeginRemediation()`.
- **3-Stage Pipeline**: Enforces `Classify` → `Group` → `Refine` ordering to eliminate duplicate MCIDs.
- **Dynamic Anchoring & Zones**: Native support for `RemediationAnchor`, `FlowRegion`, and `TolerancedZone`.
- **Atomic Operations**: `DryRun()` for diagnostic reporting and pre-flight validation; atomic `Commit()` for document mutation.

---

## 4. Remaining Future Roadmap Items

1. **Editing Pre-Tagged PDFs**:
   - `BeginRemediation()` currently rejects PDFs that already contain a `/StructTreeRoot` to avoid tag corruption. Editing existing pre-tagged structure trees remains an open feature request.
2. **Detailed Layout Attributes**:
   - Full CSS-like layout attributes (`/SpaceBefore`, `/StartIndent`, `/BBox` placement) are not yet exposed on `StructureNode`.
3. **Automated VeraPDF CLI Integration**:
   - Automated integration runner to execute VeraPDF over generated test output in CI pipelines.
