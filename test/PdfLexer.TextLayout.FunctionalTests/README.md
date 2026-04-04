# PdfLexer Text Layout Functional Conformance Tests

This project keeps browser-backed conformance, PdfLexer regression snapshots, and measurement/overflow review artifacts inside .NET tests.

It uses:

- `PuppeteerSharp` to render representative HTML fixtures in Chromium
- Chromium PDF output as the browser-backed reference artifact
- pdflexer extraction to read text geometry from both the Chromium PDF and the pdflexer-generated PDF
- structured PdfLexer JSON snapshots for regression baselines
- full-flow and split-flow PDF artifacts for measurement and overflow review

The tests compare extracted word order and bounding boxes instead of screenshot pixels.

## Suites

- `ChromePdf_And_PdfLexerPdf_HaveComparableWordGeometry`
  - compares PdfLexer output to Chromium for the shared conformance fixture catalog
  - writes side-by-side review PDFs, HTML, summaries, and the current PdfLexer regression snapshot
- `PdfLexerStructuredRegression_MatchesCheckedInBaseline`
  - compares the current PdfLexer layout snapshot to a checked-in JSON baseline when one exists
  - writes the current snapshot to `test/results/pdflexer-regression/current`
- `PdfLexerMeasurementAndOverflow_GeneratesFullAndSplitReviewArtifacts`
  - measures content in a large box, renders split variants across a cutoff sweep, and writes review PDFs plus structured summaries

## Baselines

Checked-in baselines live under:

```text
test/PdfLexer.TextLayout.FunctionalTests/Baselines/pdflexer-regression
```

To update baselines intentionally, run tests with:

```bash
PDFLEXER_UPDATE_TEXTLAYOUT_BASELINES=1 dotnet test /workspace/test/PdfLexer.TextLayout.FunctionalTests/PdfLexer.TextLayout.FunctionalTests.csproj -c Debug
```

## Run

```bash
dotnet test /workspace/test/PdfLexer.TextLayout.FunctionalTests/PdfLexer.TextLayout.FunctionalTests.csproj -c Debug
```

The first run may download Chromium through `PuppeteerSharp` into a temporary cache directory.
