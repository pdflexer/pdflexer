# Library Review Findings

This document captures issues, inconsistencies, and notable feature gaps found during a review of the `pdflexer` codebase.

## Severity Scale

- `Critical`: Can cause data loss, broken round-trips, or unsafe behavior in common workflows.
- `High`: Important correctness or API design problem with material impact.
- `Medium`: Noticeable inconsistency, missing capability, or engineering debt.
- `Low`: Cleanup or polish item.

## Effort Scale

- `S`: Small, likely isolated change.
- `M`: Moderate change across a few files/tests.
- `L`: Large change touching architecture or many features.
- `XL`: Multi-phase effort with broad test and API impact.

## Findings

### 1. Save path can discard document structures during round-trip

- `Severity`: Medium
- `Priority`: P2
- `Effort`: L
- `Area`: Saving behavior, documented limitations

The save path removes `/Names` and `/StructTreeRoot` from the catalog before writing. Based on project intent, this appears to be an intentional tradeoff to simplify page copying, page re-ordering, and related production workflows rather than an accidental omission. It is still a limitation for broader round-trip use and should be documented clearly.

Likely impact:

- Loss of named destinations
- Loss of embedded file references and other name-tree based features
- Loss of JavaScript and other name-tree-backed catalog content
- Loss of existing tagged-PDF structure unless reconstructed through the in-memory structural tree support

Relevant code:

- [PdfDocument.Saving.cs](/workspace/src/PdfLexer/PdfDocument.Saving.cs#L63)
- [PdfDocument.Saving.cs](/workspace/src/PdfLexer/PdfDocument.Saving.cs#L87)

### 2. Encrypted PDFs are rewritten without encryption

- `Severity`: Medium
- `Priority`: P3
- `Effort`: L
- `Area`: Encryption, explicit save semantics

`SaveTo()` removes `/Encrypt` from the trailer. For the current print-production-oriented scope, that may be an acceptable non-goal, but it should be explicit in user-facing docs and API expectations.

Likely impact:

- Unexpected security downgrade
- Incorrect assumptions by callers about output preservation
- Unsuitability for workflows that require encryption-preserving rewrites

Relevant code:

- [PdfDocument.Saving.cs](/workspace/src/PdfLexer/PdfDocument.Saving.cs#L71)
- [PdfStream.cs](/workspace/src/PdfLexer/PdfStream.cs#L347)

### 3. `PdfDocument.Context` is not truly document-scoped

- `Severity`: High
- `Priority`: P1
- `Effort`: M
- `Area`: API design, concurrency

`PdfDocument.Context` returns `ParsingContext.Current`, which is an `AsyncLocal`, not a context owned by the document instance. That makes the property dependent on ambient execution state rather than the context used when the document was opened.

Likely impact:

- Confusing behavior when multiple documents are open
- Concurrency hazards
- Documentation/API mismatch

Relevant code:

- [PdfDocument.cs](/workspace/src/PdfLexer/PdfDocument.cs#L19)
- [PdfDocument.Context.cs](/workspace/src/PdfLexer/PdfDocument.Context.cs#L43)
- [basics.md](/workspace/docs/basics.md#L36)

### 4. Save pipeline still contains known structural shortcuts

- `Severity`: High
- `Priority`: P1
- `Effort`: L
- `Area`: Saving, object model fidelity

The page tree builder is marked TODO and the quick-save/reuse path is disabled because of object ID collisions. This indicates the write pipeline is functional but not yet structurally mature for preserving original document topology efficiently.

Likely impact:

- Round-trip fidelity risk
- Performance penalties on rewrite-heavy workflows
- Fragility on repeated save scenarios

Relevant code:

- [PdfDocument.Saving.cs](/workspace/src/PdfLexer/PdfDocument.Saving.cs#L30)
- [PdfDocument.Saving.cs](/workspace/src/PdfLexer/PdfDocument.Saving.cs#L44)
- [PdfDocument.Saving.cs](/workspace/src/PdfLexer/PdfDocument.Saving.cs#L122)

### 5. Test suite depends on external `pdfcpu`

- `Severity`: High
- `Priority`: P1
- `Effort`: M
- `Area`: CI, test portability

The syntax validation helper shells out to `pdfcpu`. In the current environment that caused 10 test failures even though the code built successfully. These are not self-contained tests.

Observed status during review:

- `dotnet build`: success
- `dotnet test`: 384 passed, 10 failed, 3 skipped
- All 10 failures were caused by missing `pdfcpu`

Relevant code:

- [SyntaxValidation.cs](/workspace/test/PdfLexer.Tests/SyntaxValidation.cs#L27)
- [PdfLexer.Tests.csproj](/workspace/test/PdfLexer.Tests/PdfLexer.Tests.csproj#L4)

### 6. Public API surface is broader than implementation maturity

- `Severity`: High
- `Priority`: P1
- `Effort`: L
- `Area`: API hygiene

There are public types with unfinished functionality, and the README already acknowledges that too much is exposed. Example: `PdfFileStreamContents` is public but unimplemented.

Likely impact:

- Users depend on unstable APIs accidentally
- Harder path to API stabilization
- Increased maintenance burden

Relevant code:

- [PdfStream.cs](/workspace/src/PdfLexer/PdfStream.cs#L438)
- [README.md](/workspace/README.md#L37)

### 7. Docs and support matrix are stale or misleading

- `Severity`: Medium
- `Priority`: P2
- `Effort`: M
- `Area`: Documentation

The README still lists filter support as a major gap, but the codebase already supports several filters. Docs also contain stale or broken examples.

Examples:

- README says filter support is still a major gap
- Code implements Ascii85, AsciiHex, CCITT, Flate, LZW, RunLength, DCT decode paths
- `docs/basics.md` includes a sample with a missing semicolon

Relevant code and docs:

- [README.md](/workspace/README.md#L35)
- [ParsingContext.cs](/workspace/src/PdfLexer/ParsingContext.cs#L149)
- [basics.md](/workspace/docs/basics.md#L123)

### 8. Package/release metadata is inconsistent

- `Severity`: Medium
- `Priority`: P2
- `Effort`: S
- `Area`: Packaging

Release packages are generated without package readmes, and dependency versions are fragmented across projects. There is also a shipping CLI using a beta `System.CommandLine` package.

Examples:

- Missing NuGet readme warnings during build
- `ImageSharp` is version `2.1.13` in one project and `3.1.12` in another
- `pdfctl` depends on `System.CommandLine.NamingConventionBinder` beta

Relevant files:

- [PdfLexer.ImageSharpExts.csproj](/workspace/src/PdfLexer.ImageSharpExts/PdfLexer.ImageSharpExts.csproj#L24)
- [PdfLexer.Interactive.csproj](/workspace/src/PdfLexer.Interactive/PdfLexer.Interactive.csproj#L16)
- [PdfLexer.pdfctl.csproj](/workspace/src/PdfLexer.pdfctl/PdfLexer.pdfctl.csproj#L11)

### 9. Warning debt in release builds

- `Severity`: Medium
- `Priority`: P2
- `Effort`: S
- `Area`: Code quality

The solution builds successfully, but release builds still emit nullable and other warnings in production projects.

Examples:

- Nullability warnings in outline DOM code
- Warning in validation project for unnecessary `new`
- CLI warning for nullable conversion

Relevant files:

- [PdfOutlineItem.cs](/workspace/src/PdfLexer/DOM/PdfOutlineItem.cs#L45)

### 10. Expected general-purpose PDF features are still missing or only low-level

- `Severity`: Medium
- `Priority`: P2
- `Effort`: XL
- `Area`: Product capability

For a general-purpose PDF manipulation library, users would reasonably expect higher-level support in several areas that are currently missing or largely dictionary-level only. Some of these may be intentionally out of scope for a print-production-focused tool, but they are still the main gaps if the project expands beyond that niche.

Examples:

- AcroForm/form field model and editing helpers
- Digital signature aware workflows
- Incremental update support
- Higher-level annotation APIs
- Better image codec/colorspace coverage
- Safer attachment/name-tree APIs

Signals in repo:

- Annotation support exists mostly as low-level dictionary handling
- Signature-related names exist, but not a clear feature surface
- Image extraction docs still call out gaps in `JBIG2`, `JPX`, `ICCBased`, `Separation`

Relevant files:

- [PdfPage.cs](/workspace/src/PdfLexer/DOM/PdfPage.cs#L205)
- [image_extraction.md](/workspace/docs/image_extraction.md#L7)

## Summary

The library is already substantial and test-backed, especially in parsing, text extraction, content scanning, and general PDF object manipulation. The main risk is not lack of breadth, but a mismatch between feature ambition and round-trip guarantees, API maturity, and documentation accuracy.
