# Release Testing Feedback

This document captures feedback on the current `pdflexer.PdfiumRegressionTester` process specifically as a manual regression-testing tool for candidate releases.

## Overall Assessment

The current process is well suited to exploratory and manual candidate-release validation:

- it compares rendered output against PDFium, which is a practical external oracle
- it exercises multiple rewrite modes instead of only parse/open paths
- it emits artifacts that can be inspected manually
- it keeps a baseline of known issues and known differences

That said, there are several improvements that would make release decisions faster and more trustworthy.

## Main Concerns for Candidate Release Testing

### 1. Release outcome is not surfaced strongly enough

The harness records useful result categories such as:

- `Regression`
- `Improvement`
- `NewTest`
- `Match`
- `MatchErrorIncrease`

But the process does not make those outcomes prominent enough for a human reviewer. A candidate release tool should make it immediately obvious whether the release got better, worse, or simply changed in expected ways.

### 2. Visual comparison sensitivity is probably too low for print workflows

The current comparison path runs with a low page scale (`ppp = 1` in the main regression runner). For print-production validation, that is likely too weak to catch some meaningful differences in:

- fine vector detail
- thin strokes
- kerning and glyph placement
- small text shifts

### 3. Baseline drift is a real risk

The `.jsonl` baseline files are practical and useful, but over time they can normalize regressions unless updates are reviewed carefully. Manual candidate testing works best when baseline changes are treated as intentional decisions with traceable rationale.

### 4. Text ignore lists are getting harder to trust

The text regression mode relies on a growing set of hard-coded skips and ignore maps. This is manageable at small scale, but it becomes hard to audit over time and can mask meaningful changes. There are also duplicated keys in the ignore map, which means some earlier entries are silently overwritten.

### 5. Small harness bugs can distort manual review

Even in a manual process, the harness needs to be reliable. Bugs in result classification or comparison logic can waste reviewer time or create false confidence. Example: the candidate-open null check in `Compare.cs` currently checks the wrong variable.

### 6. Diagnostic output is useful but not well-indexed

The process already emits diff images and content dumps, which is good. The problem is discoverability: reviewers still need to navigate output folders manually rather than opening one summary page that links everything together.

## Recommended Improvements

### 1. Add a release summary report

Create a final summary artifact for each run that clearly lists counts and file names by category:

- `Regression`
- `Improvement`
- `NewTest`
- `Match`
- `MatchErrorIncrease`
- `PdfLexerError`
- `PdfiumError`
- `Skip`

This should be the first file reviewed after a candidate run completes.

### 2. Add an artifact index for manual review

Generate a markdown or HTML index that links each interesting PDF to its artifacts:

- diff image
- content dump
- raw content dump
- text comparison files
- status summary

This would make manual triage much faster.

### 3. Increase or parameterize render sensitivity

Make visual comparison resolution configurable by mode, and use a higher default for release validation. For print-oriented review, a higher render scale is likely worth the extra runtime.

### 4. Separate accepted differences from temporary ignores

Treat these as different buckets:

- accepted known differences
- temporary suppressions pending investigation
- baseline limitations caused by PDFium or malformed source PDFs

That separation makes review much more trustworthy.

### 5. Require rationale when baselines are updated

Whenever a baseline `.jsonl` file changes, record why:

- bug fixed
- intentional behavior change
- PDFium discrepancy
- malformed PDF
- accepted non-goal

This can be a short note, but it should exist.

### 6. Move text exceptions into versioned data files

The current hard-coded skip and ignore logic should be externalized into structured files so it is easier to:

- review changes
- detect duplicates
- annotate reasons
- track when suppressions should be removed

### 7. Fix harness correctness issues before relying on output

Before deeper process work, fix obvious harness issues such as:

- incorrect candidate null check in `Compare.cs`
- dead logging paths in `TestResults`
- weak signaling of regressions in top-level run output

These are small but high-value fixes.

### 8. Compare against the previous release explicitly

In addition to comparing against stored baselines, consider a candidate-vs-previous-release pass. That makes release approval easier because it answers the most important practical question:

"What got better or worse since the last shipped version?"

## Recommended Near-Term Priority

If this remains a manual release gate, the best near-term investments are:

1. Fix harness correctness bugs.
2. Improve final run summaries.
3. Add an artifact index for manual review.
4. Increase or parameterize render sensitivity.
5. Externalize ignore/baseline rationale.

## Summary

The current process is already useful as a manual candidate-release lab. The next step is not heavy automation, but better review ergonomics and stronger confidence in the harness itself. That would make release decisions faster while reducing the chance of normalizing regressions over time.
