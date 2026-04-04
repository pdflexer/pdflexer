Structured PdfLexer regression baselines for `PdfLexer.TextLayout.FunctionalTests` belong in the `pdflexer-regression` subdirectory.

These files are optional until seeded. When present, `PdfLexerStructuredRegression_MatchesCheckedInBaseline` compares the current snapshot against them.

Update flow:

```bash
PDFLEXER_UPDATE_TEXTLAYOUT_BASELINES=1 dotnet test test/PdfLexer.TextLayout.FunctionalTests/PdfLexer.TextLayout.FunctionalTests.csproj -c Debug
```
