# PDF/UA Conformance Workflow

`PdfLexer` does not ship a public in-process PDF/UA validator for this change set. The supported workflow is:

- use the library's fast unit/integration tests to catch authoring regressions early;
- generate the fixture corpus under `test/results/accessibility-fixtures`;
- run an external conformance tool against those artifacts.

See `docs/accessibility-authoring.md` for the supported authoring scope, the setup-helper workflow, and the structure-builder path that feed this conformance step.

## Supported Scope

This accessibility authoring work supports:

- new documents created with `PdfLexer`;
- existing documents that are currently untagged and are being tagged for the first time.

It does not support:

- editing or remediating an existing `StructTreeRoot`;
- treating `PdfLexer` itself as the conformance authority.

## Local veraPDF Run

1. Generate the fixture corpus by running the test that exercises `AccessibilityFixtureGenerator`.
2. Run veraPDF against the generated PDFs.
3. Treat any PDF/UA-1 failure as a library regression until proven otherwise.

Typical CLI shape:

```bash
verapdf --format text --flavour ua1 test/results/accessibility-fixtures/*.pdf
```

Pin the veraPDF version in CI so upgrades are explicit and reproducible.

## PAC

PAC (axes4) is a useful additional manual check on Windows, especially when comparing behavior with assistive-technology-oriented tooling. It is not the primary automated gate because it is not a practical cross-platform CI dependency.
