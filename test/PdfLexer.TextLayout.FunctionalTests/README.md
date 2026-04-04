# PdfLexer Text Layout Functional Conformance Tests

This project keeps browser-backed conformance inside .NET tests.

It uses:

- `PuppeteerSharp` to render representative HTML fixtures in Chromium
- Chromium PDF output as the browser-backed reference artifact
- pdflexer extraction to read text geometry from both the Chromium PDF and the pdflexer-generated PDF

The tests compare extracted word order and bounding boxes instead of screenshot pixels.

## Run

```bash
dotnet test /workspace/test/PdfLexer.TextLayout.FunctionalTests/PdfLexer.TextLayout.FunctionalTests.csproj -c Debug
```

The first run may download Chromium through `PuppeteerSharp` into a temporary cache directory.
