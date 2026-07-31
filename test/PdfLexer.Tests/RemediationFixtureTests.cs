using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using pdflexer.PdfiumRegressionTester;
using PdfLexer.Content.Model;
using PdfLexer.DOM;
using PdfLexer.Operators;
using PdfLexer.Remediation;
using Xunit;

namespace PdfLexer.Tests;

// Shares the serialized veraPDF collection: these tests regenerate the corpus on disk, and the
// veraPDF theory reads it, so the two must not run concurrently.
[Collection(VeraPdfTestCollection.Name)]
public class RemediationFixtureTests
{
    [Fact]
    public void StrictInvoiceTableFixtureCommitsAndSavesAsPdfUa1()
    {
        var fixture = RemediationFixtureGenerator.GenerateStrictInvoiceUa1();

        Assert.True(fixture.Report.Committed);
        Assert.Empty(fixture.Report.TemplateDifferences);
        Assert.All(fixture.Report.AutoArtifacts, x => Assert.NotNull(x.InventoryItemId));
        using var document = PdfDocument.Open(fixture.Bytes);
        AccessibilityIntegrityAssert.HasDocumentSetup(document, PdfUaProfile.PdfUa1);
        AccessibilityIntegrityAssert.HasBasicStructureIntegrity(document);
        AccessibilityIntegrityAssert.HasValidTableAndListHierarchy(document);
    }

    [Fact]
    public void Remediation_Fixture_Generator_Writes_Input_And_Remediated_Ua1_Ua2_Corpus()
    {
        var fixtures = RemediationFixtureGenerator.GenerateAll();

        var originalFamilies = new[]
        {
            "invoice-like",
            "statement-like",
            "report-like",
            "form-like",
            "multi-column-sidebar",
            "mixed-page-sizes"
        };
        var expectedBaseNames = new[]
        {
            "invoice-like",
            "statement-like",
            "report-like",
            "form-like",
            "multi-column-sidebar",
            "mixed-page-sizes",
            "c-01-unicode-normalization",
            "c-02-operator-fragmentation",
            "c-03-graphical-content",
            "c-08-existing-annotations",
            "c-10-running-furniture",
            "c-12-continued-table",
            "c-23-label-value-pairs",
            "c-29-a-nested-list",
            "c-29-b-nested-sections",
            "c-30-prescriptive-repeated-sections",
            "c-04-a-flag",
            "c-04-b-failfast",
            "c-04-c-autoartifact",
            "c-04-d-inventory",
            "c-05-semantic-assertion",
            "c-06-a-template-drift",
            "c-06-b-template-drift",
            "c-24-ambiguous-anchors"
        };
        var expectedFileNames = expectedBaseNames.SelectMany(x => new[]
        {
            RemediationFixtureGenerator.GetFixtureFileName(x, PdfUaProfile.PdfUa1),
            RemediationFixtureGenerator.GetFixtureFileName(x, PdfUaProfile.PdfUa2)
        });

        Assert.Equal(expectedFileNames.OrderBy(x => x), fixtures.Select(x => x.FileName).OrderBy(x => x));
        Assert.Equal(expectedBaseNames.Length, fixtures.Select(x => x.InputPath).Distinct().Count());
        Assert.All(fixtures.Select(x => x.InputPath).Distinct(), path => Assert.True(File.Exists(path), path));

        // Diagnosing fixtures exist to produce diagnostics and are never committed, so only their
        // input is on disk. Everything else must commit clean.
        foreach (var fixture in fixtures.Where(x => x.Outcome == FixtureOutcome.DiagnosesOnly))
        {
            Assert.Null(fixture.Path);
            Assert.False(fixture.Report.Committed);
            Assert.NotEmpty(fixture.Report.Diagnostics);
        }

        foreach (var fixture in fixtures.Where(x => x.Outcome != FixtureOutcome.DiagnosesOnly))
        {
            Assert.True(File.Exists(fixture.Path), fixture.Path);
            Assert.True(fixture.Report.Committed);
            Assert.Empty(fixture.Report.Diagnostics.Where(x => !x.StartsWith("[SUPPRESSED]")));
            Assert.Empty(fixture.Report.TemplateDifferences);
            // c-04-c is the deliberate no-inventory control: it absorbs content with nothing declared,
            // which is exactly the state the inventory exists to make visible.
            if (!fixture.FileName.StartsWith("c-04-c", StringComparison.Ordinal))
            {
                Assert.All(fixture.Report.AutoArtifacts, x => Assert.NotNull(x.InventoryItemId));
            }

            using var document = PdfDocument.Open(fixture.Bytes);
            AccessibilityIntegrityAssert.HasDocumentSetup(document, fixture.Profile);
            AccessibilityIntegrityAssert.HasBasicStructureIntegrity(document);
            AccessibilityIntegrityAssert.HasOnlyTaggedOrArtifactContent(document);
            if (fixture.Profile == PdfUaProfile.PdfUa2)
            {
                AccessibilityIntegrityAssert.HasPdf20RootNamespace(document);
            }

            // DebugWrite provenance, checked on the families that carry the shared body-line rule.
            if (originalFamilies.Contains(fixture.FileName.Split("-ua")[0]))
            {
                Assert.Contains(
                    AccessibilityIntegrityAssert.GetStructureElements(document),
                    x => x.Get<PdfString>(PdfName.T)?.Value == "body-line");
            }
        }
    }

    [Fact]
    public void Remediation_Fixtures_Capture_Expected_Anchor_Flow_And_Debug_Provenance()
    {
        var fixtures = RemediationFixtureGenerator.GenerateAll();

        var invoice = Assert.Single(fixtures, x => x.FileName == "invoice-like-ua2.pdf");
        Assert.Contains(invoice.Report.Claims, x => x.RuleId == "invoice-number");
        Assert.Equal(3, invoice.Report.Claims.Count(x => x.RuleId == "line-item-header-cell"));
        Assert.Equal(6, invoice.Report.Claims.Count(x => x.RuleId == "line-item-cell"));
        Assert.Equal(
            new[] { "line-items-header-spacing", "line-items-header-spacing" },
            invoice.Report.AutoArtifacts.Select(x => x.InventoryItemId).ToArray());

        using (var invoiceDocument = PdfDocument.Open(invoice.Bytes))
        {
            Assert.Contains(
                AccessibilityIntegrityAssert.GetStructureElements(invoiceDocument),
                x => x.Get<PdfString>(PdfName.T)?.Value == "invoice-number");
            AccessibilityIntegrityAssert.HasValidTableAndListHierarchy(invoiceDocument);
            var table = Assert.Single(
                AccessibilityIntegrityAssert.GetStructureElements(invoiceDocument),
                x => x.Get<PdfName>(PdfName.S) == PdfName.Table);
            var rows = table.Get<PdfArray>(PdfName.K)!
                .Select(x => x.Resolve())
                .OfType<PdfDictionary>()
                .ToList();
            Assert.Equal(3, rows.Count);
            Assert.Equal(new[] { (PdfName)"TH", (PdfName)"TH", (PdfName)"TH" }, GetChildTypes(rows[0]));
            Assert.Equal(new[] { (PdfName)"TD", (PdfName)"TD", (PdfName)"TD" }, GetChildTypes(rows[1]));
            Assert.Equal(new[] { (PdfName)"TD", (PdfName)"TD", (PdfName)"TD" }, GetChildTypes(rows[2]));
            Assert.Contains(invoiceDocument.Pages.Select(x => x.DumpDecodedContents()), x => x.Contains("/Artifact"));
        }

        var statement = Assert.Single(fixtures, x => x.FileName == "statement-like-ua2.pdf");
        Assert.Equal(3, statement.Report.Claims.Count(x => x.RuleId == "bill-to-address"));

        var mixed = Assert.Single(fixtures, x => x.FileName == "mixed-page-sizes-ua2.pdf");
        using var mixedDocument = PdfDocument.Open(mixed.Bytes);
        Assert.Equal(2, mixedDocument.Pages.Count);
        Assert.All(mixedDocument.Pages, page => Assert.Contains("/Artifact", page.DumpDecodedContents()));
    }

    [Fact]
    public void C08_AdoptsExistingAnnotationsWithoutReplacementForBothProfiles()
    {
        var fixtures = RemediationFixtureGenerator.GenerateAll()
            .Where(x => x.FileName.StartsWith("c-08-existing-annotations-", StringComparison.Ordinal))
            .ToArray();
        Assert.Equal(2, fixtures.Length);

        foreach (var fixture in fixtures)
        {
            Assert.Equal(5, fixture.Report.AnnotationInventory.Count);
            Assert.Equal(4, fixture.Report.AnnotationInventory.Count(
                x => x.Disposition == RemediationAnnotationDisposition.Applied));
            Assert.Single(fixture.Report.AnnotationInventory,
                x => x.Disposition == RemediationAnnotationDisposition.Exempt && x.Subtype == "Popup");
            Assert.Equal(2, fixture.Report.AnnotationInventory.Count(x => x.ProducedTag == "Link"));
            Assert.Equal(2, fixture.Report.AnnotationInventory.Count(x => x.ProducedTag == "Annot"));

            using var document = PdfDocument.Open(fixture.Bytes);
            Assert.Equal(3, document.Pages[0].NativeObject.Get<PdfArray>(PdfName.Annots)!.Count);
            Assert.Equal(2, document.Pages[1].NativeObject.Get<PdfArray>(PdfName.Annots)!.Count);
            var annotations = document.Pages
                .SelectMany(x => x.NativeObject.Get<PdfArray>(PdfName.Annots)!)
                .Select(x => x.Resolve())
                .OfType<PdfDictionary>()
                .ToArray();
            Assert.Equal(5, annotations.Length);
            Assert.All(annotations.Where(x => x.Get<PdfName>(PdfName.Subtype)?.Value != "Popup"),
                x => Assert.NotNull(x.Get<PdfIntNumber>(PdfName.StructParent)));
            Assert.Null(Assert.Single(annotations,
                x => x.Get<PdfName>(PdfName.Subtype)?.Value == "Popup").Get<PdfIntNumber>(PdfName.StructParent));
        }
    }

    [Fact]
    public void C10_EmitsCompleteRunningFurniturePropertiesForBothProfiles()
    {
        var fixtures = RemediationFixtureGenerator.GenerateAll()
            .Where(x => x.FileName.StartsWith("c-10-running-furniture-", StringComparison.Ordinal))
            .ToArray();
        Assert.Equal(2, fixtures.Length);

        foreach (var fixture in fixtures)
        {
            using var document = PdfDocument.Open(fixture.Bytes);
            Assert.Equal(4, document.Pages.Count);
            foreach (var page in document.Pages)
            {
                var artifacts = page.GetContentNodes<double>()
                    .OfType<MarkedContentGroup<double>>()
                    .Where(x => x.Tag.Name == PdfName.Artifact)
                    .Select(x => x.Tag.InlineProps!)
                    .ToArray();
                Assert.Equal(3, artifacts.Length);
                Assert.All(artifacts, x =>
                {
                    Assert.Equal(PdfName.Pagination, x.Get<PdfName>(PdfName.TYPE));
                    Assert.NotNull(x.Get<PdfArray>(PdfName.BBox));
                });
                var header = Assert.Single(artifacts, x => x.Get<PdfName>(PdfName.Subtype) == PdfName.Header);
                var footer = Assert.Single(artifacts, x => x.Get<PdfName>(PdfName.Subtype) == PdfName.Footer);
                var watermark = Assert.Single(artifacts, x => x.Get<PdfName>(PdfName.Subtype) == PdfName.Watermark);
                Assert.Equal("Top", Assert.IsType<PdfName>(Assert.Single(
                    header.Get<PdfArray>((PdfName)"Attached")!).Resolve()).Value);
                Assert.Equal("Bottom", Assert.IsType<PdfName>(Assert.Single(
                    footer.Get<PdfArray>((PdfName)"Attached")!).Resolve()).Value);
                Assert.Null(watermark.Get<PdfArray>((PdfName)"Attached"));
            }
        }
    }

    [Fact]
    public void Remediation_Fixtures_Are_Visually_And_Glyph_Position_Invariant()
    {
        var fixtures = RemediationFixtureGenerator.GenerateAll();
        var diffRoot = Path.Combine(RemediationFixtureGenerator.FixtureRootPath, "visual-diffs");
        Directory.CreateDirectory(diffRoot);

        foreach (var fixture in fixtures.Where(x => x.Outcome != FixtureOutcome.DiagnosesOnly))
        {
            var diffPrefix = Path.Combine(diffRoot, Path.GetFileNameWithoutExtension(fixture.FileName));
            var comparison = new Compare(diffPrefix).CompareAllPages(
                fixture.InputPath,
                fixture.Path,
                CompareMode.Exact);

            Assert.All(
                comparison,
                result => Assert.False(
                    result.HadChanges,
                    $"{fixture.FileName}: {result.Type}; {result.Error}; diff={result.DiffImage}"));

            using var baseline = PdfDocument.Open(fixture.InputPath);
            using var candidate = PdfDocument.Open(fixture.Path);
            Assert.Equal(baseline.Pages.Count, candidate.Pages.Count);
            for (var pageIndex = 0; pageIndex < baseline.Pages.Count; pageIndex++)
            {
                var baselineText = baseline.Pages[pageIndex]
                    .GetContentModel()
                    .Flatten()
                    .OfType<TextContent<double>>()
                    .ToArray();
                var candidateText = candidate.Pages[pageIndex]
                    .GetContentModel()
                    .Flatten()
                    .OfType<TextContent<double>>()
                    .ToArray();
                var baselineGlyphs = baselineText.SelectMany(x => x.EnumerateCharacters()).ToArray();
                var candidateGlyphs = candidateText.SelectMany(x => x.EnumerateCharacters()).ToArray();
                var baselineBoxes = baselineText.SelectMany(x => x.GetGlyphBoundingBoxes()).ToArray();
                var candidateBoxes = candidateText.SelectMany(x => x.GetGlyphBoundingBoxes()).ToArray();

                Assert.Equal(baselineGlyphs.Select(x => x.Char), candidateGlyphs.Select(x => x.Char));
                Assert.Equal(baselineGlyphs.Length, candidateGlyphs.Length);
                for (var glyphIndex = 0; glyphIndex < baselineGlyphs.Length; glyphIndex++)
                {
                    Assert.InRange(
                        candidateGlyphs[glyphIndex].XPos,
                        baselineGlyphs[glyphIndex].XPos - 0.000001,
                        baselineGlyphs[glyphIndex].XPos + 0.000001);
                    Assert.InRange(
                        candidateGlyphs[glyphIndex].YPos,
                        baselineGlyphs[glyphIndex].YPos - 0.000001,
                        baselineGlyphs[glyphIndex].YPos + 0.000001);
                }

                Assert.Equal(baselineBoxes.Length, candidateBoxes.Length);
                for (var glyphIndex = 0; glyphIndex < baselineBoxes.Length; glyphIndex++)
                {
                    AssertCoordinateEqual(baselineBoxes[glyphIndex].LLx, candidateBoxes[glyphIndex].LLx);
                    AssertCoordinateEqual(baselineBoxes[glyphIndex].LLy, candidateBoxes[glyphIndex].LLy);
                    AssertCoordinateEqual(baselineBoxes[glyphIndex].URx, candidateBoxes[glyphIndex].URx);
                    AssertCoordinateEqual(baselineBoxes[glyphIndex].URy, candidateBoxes[glyphIndex].URy);
                }
            }
        }
    }

    [Fact]
    public void C01_Unicode_And_Normalization_Fixture_Validates_Matching_And_Invariance()
    {
        var fixtures = RemediationFixtureGenerator.GenerateAll();
        var c01 = Assert.Single(fixtures, x => x.FileName == "c-01-unicode-normalization-ua2.pdf");

        Assert.True(c01.Report.Committed);
        Assert.Equal(2, c01.Report.Claims.Count(x => x.RuleId == "c01-ligature"));
        Assert.Equal(2, c01.Report.Claims.Count(x => x.RuleId == "c01-softhyphen"));
        Assert.Equal(2, c01.Report.Claims.Count(x => x.RuleId == "c01-nbsp"));
        Assert.Equal(2, c01.Report.Claims.Count(x => x.RuleId == "c01-nonbreak-hyphen"));
        Assert.Equal(2, c01.Report.Claims.Count(x => x.RuleId == "c01-quotes"));
        Assert.Equal(2, c01.Report.Claims.Count(x => x.RuleId == "c01-e-accent"));
        Assert.Single(c01.Report.Claims, x => x.RuleId == "c01-spaces");

        // Matched-input counts rather than claims, because the line rules already own these spans and
        // the word-level regex rules are conflict-rejected after matching.
        int Matched(string ruleId) =>
            Assert.Single(c01.Report.RuleEvaluations, x => x.RuleId == ruleId).Total.InputsMatched;

        // A regex against the normalized form matches both members of the pair; the same expression
        // written against the raw non-breaking-hyphen form matches neither...
        Assert.Equal(2, Matched("c01-regex-normalized"));
        Assert.Equal(0, Matched("c01-regex-raw"));
        // ...until normalization is switched off for that rule, when it matches the raw form alone.
        Assert.Equal(1, Matched("c01-regex-raw-unnormalized"));
    }

    [Fact]
    public void C01_Normalization_Does_Not_Change_Structure_Or_Mcid_Assignment()
    {
        var unicode = Assert.Single(
            RemediationFixtureGenerator.GenerateAll(),
            x => x.FileName == "c-01-unicode-normalization-ua2.pdf");

        using var asciiInput = RemediationFixtureGenerator.CreateC01AsciiInput();
        using var asciiDocument = PdfDocument.Open(asciiInput.Save());
        using var session = asciiDocument
            .BeginRemediation(new RemediationSessionConfiguration
            {
                Language = "en-US",
                Title = "Remediated C-01 ASCII twin",
                Profile = PdfUaProfile.PdfUa2,
                StrictConformance = true,
                DebugWrite = true,
                LeftoverPolicy = RemediationLeftoverPolicy.AutoArtifact
            })
            .Use(RemediationFixtureGenerator.CreateC01RuleSet());
        var ascii = session.Commit();

        Assert.True(ascii.Committed);
        Assert.Equal(
            unicode.Report.Claims.Select(x => x.RuleId).OrderBy(x => x, StringComparer.Ordinal),
            ascii.Claims.Select(x => x.RuleId).OrderBy(x => x, StringComparer.Ordinal));
        Assert.Equal(
            unicode.Report.Claims.SelectMany(x => x.AppliedBindings).SelectMany(x => x.Mcids).OrderBy(x => x),
            ascii.Claims.SelectMany(x => x.AppliedBindings).SelectMany(x => x.Mcids).OrderBy(x => x));
    }

    [Fact]
    public void C02_Operator_Fragmentation_Fixture_Validates_Word_And_State_Splits()
    {
        var fixtures = RemediationFixtureGenerator.GenerateAll();
        var c02 = Assert.Single(fixtures, x => x.FileName == "c-02-operator-fragmentation-ua2.pdf");

        Assert.True(c02.Report.Committed);
        Assert.Equal(3, c02.Report.Claims.Count(x => x.RuleId == "c02-line"));
        Assert.Contains(c02.Report.Claims, x => x.RuleId == "c02-line2");
        Assert.Contains(c02.Report.Claims, x => x.RuleId == "c02-tj-word1");
        Assert.Contains(c02.Report.Claims, x => x.RuleId == "c02-tj-word2");
        Assert.Contains(c02.Report.Claims, x => x.RuleId == "c02-state1");
        Assert.Contains(c02.Report.Claims, x => x.RuleId == "c02-state2");
        Assert.Contains(c02.Report.Claims, x => x.RuleId == "c02-state3");
        Assert.Contains(c02.Report.Claims, x => x.RuleId == "c02-state4");

        // The PDF/UA 7.2 case: the TJ line spells its word gaps as glyph adjustments with no space
        // character. Counting line claims cannot tell whether the boundary was detected, so assert the
        // words directly.
        Assert.Equal(
            new[] { "#:", "STM-12345", "Statement" },
            c02.Report.Outcomes
                .Where(x => x.RuleId == "c02-tj-words")
                .SelectMany(x => x.Candidates)
                .Select(x => x.RawText)
                .OrderBy(x => x, StringComparer.Ordinal)
                .ToArray());
    }

    [Fact]
    public void C03_Graphical_Content_Fixture_Validates_NonText_Candidates()
    {
        var fixtures = RemediationFixtureGenerator.GenerateAll();
        var c03 = Assert.Single(fixtures, x => x.FileName == "c-03-graphical-content-ua2.pdf");

        Assert.True(c03.Report.Committed);
        Assert.Contains(c03.Report.Claims, x => x.RuleId == "c03-logo");
        Assert.Contains(c03.Report.Claims, x => x.RuleId == "c03-footer-logo");
        Assert.Contains(c03.Report.Claims, x => x.RuleId == "c03-lines");
        Assert.Equal(2, c03.Report.Claims.Count(x => x.RuleId == "c03-form"));
        Assert.Contains(c03.Report.Claims, x => x.RuleId == "c03-shading");

        var logo = Assert.IsType<ContentRemediationCandidate>(
            Assert.Single(Assert.Single(c03.Report.Claims, x => x.RuleId == "c03-logo").Candidates));
        var footerLogo = Assert.IsType<ContentRemediationCandidate>(
            Assert.Single(Assert.Single(c03.Report.Claims, x => x.RuleId == "c03-footer-logo").Candidates));
        Assert.Equal(2, logo.ResourceUseCount);
        Assert.Equal(logo.ResourceIdentity, footerLogo.ResourceIdentity);
        Assert.StartsWith("sha256:", logo.ResourceIdentity);
        Assert.Equal(3, c03.Report.Claims.Count(x => x.RuleId == "c03-lines"));

        var figureBinding = Assert.Single(
            Assert.Single(c03.Report.Claims, x => x.RuleId == "c03-logo").AppliedBindings);
        Assert.Equal("Acme logo", figureBinding.StructureNode!.Alt);

        // The clip-only path (W n) paints nothing, so it must never be enumerated as a candidate.
        // Asserted directly rather than inferred from the path count, which changes whenever the
        // fixture gains decoration.
        using var input = PdfDocument.Open(File.ReadAllBytes(c03.InputPath));
        using var probeSession = input.BeginRemediation(new RemediationSessionConfiguration
        {
            StrictConformance = false,
            LeftoverPolicy = RemediationLeftoverPolicy.Flag
        });
        var probe = probeSession.DryRun(new Rule(
            "c03-all-paths",
            RemediationActions.Artifact(ArtifactSubtype.Layout),
            RemediationPredicate.Always,
            CandidateSelector.Content(RemediationCandidateKind.Path)));
        Assert.Equal(3, Assert.Single(probe.RuleEvaluations, x => x.RuleId == "c03-all-paths").Total.InputsConsidered);
    }

    [Fact]
    public void C04_Leftover_Accounting_Validates_All_Three_Policies()
    {
        var fixtures = RemediationFixtureGenerator.GenerateAll();

        // AutoArtifact absorbs text and graphics alike, and reports every absorbed item. An artifacted
        // item that appears nowhere in the report is the failure this fixture exists to catch.
        var auto = Assert.Single(fixtures, x => x.FileName == "c-04-c-autoartifact-ua2.pdf");
        Assert.True(auto.Report.Committed);
        Assert.Contains(auto.Report.AutoArtifacts, x => x.Text?.Contains("Unclaimed Line 2") == true);
        Assert.Contains(auto.Report.AutoArtifacts, x => x.Text?.Contains("Unclaimed Line 3") == true);
        Assert.Contains(auto.Report.AutoArtifacts, x => x.CandidateKind == RemediationCandidateKind.Image);
        Assert.Contains(auto.Report.AutoArtifacts, x => x.CandidateKind == RemediationCandidateKind.Path);

        // Flag names the unclaimed text and graphical counts without hiding anything.
        var flag = Assert.Single(fixtures, x => x.FileName == "c-04-a-flag-ua2.pdf");
        Assert.Contains(flag.Report.Diagnostics, d => d.Contains("unclaimed painting content"));
        Assert.Contains(flag.Report.UnaccountedContent, x => x.CandidateKind == RemediationCandidateKind.Image);
        Assert.Contains(flag.Report.UnaccountedContent, x => x.CandidateKind == RemediationCandidateKind.Path);

        // FailFast refuses the commit, and the refusal must name unclaimed content rather than being
        // any exception that happens to escape.
        var failFast = Assert.Single(fixtures, x => x.FileName == "c-04-b-failfast-ua2.pdf");
        Assert.Contains(failFast.Report.Diagnostics, d => d.Contains("unclaimed painting content"));
        using var failFastDocument = PdfDocument.Open(File.ReadAllBytes(failFast.InputPath));
        using var failFastSession = failFastDocument
            .BeginRemediation(new RemediationSessionConfiguration
            {
                Profile = PdfUaProfile.PdfUa1,
                StrictConformance = true,
                LeftoverPolicy = RemediationLeftoverPolicy.FailFast
            })
            .Use(RemediationFixtureGenerator.CreateC04RuleSetWithoutInventory());
        var refusal = Assert.Throws<InvalidOperationException>(() => failFastSession.Commit());
        Assert.Contains("unclaimed painting content", refusal.Message);

        // With an inventory declared, absorbed content outside declared furniture is named.
        var inventory = Assert.Single(fixtures, x => x.FileName == "c-04-d-inventory-ua2.pdf");
        Assert.Contains(inventory.Report.Diagnostics, x => x.Contains("ArtifactUndeclared"));
        Assert.Contains(inventory.Report.AutoArtifacts, x => x.InventoryItemId == null);
        Assert.Contains(
            inventory.Report.TemplateDifferences,
            x => x.Kind == RemediationTemplateDifferenceKind.UndeclaredArtifact);
    }

    [VeraPdfFact]
    public void C05_Output_Passes_VeraPdf_While_Semantic_Assertion_Fails()
    {
        var c05 = Assert.Single(
            RemediationFixtureGenerator.GenerateAll(),
            x => x.FileName == "c-05-semantic-assertion-ua1.pdf");

        // The whole point of this fixture: external conformance validation cannot see that the invoice
        // total was artifacted and the footer tagged H1. The assertion set can.
        Assert.True(c05.Report.Committed);
        var result = VeraPdfValidation.Validate(c05.Bytes, PdfUaProfile.PdfUa1);
        Assert.False(result.ProcessingFailed, result.StandardError);
        Assert.True(result.IsCompliant, result.Report);

        var outcome = Assert.Single(
            c05.Report.AssertionOutcomes,
            x => x.AssertionId == "c05-total-must-remain-content");
        Assert.False(outcome.Passed);
        Assert.Contains(
            c05.Report.Diagnostics,
            d => d.StartsWith("[SUPPRESSED] SemanticAssertionFailed", StringComparison.Ordinal));
    }

    [Fact]
    public void C06_Template_Drift_Pair_Detects_Drift_In_Variant()
    {
        var fixtures = RemediationFixtureGenerator.GenerateAll();

        var baseline = Assert.Single(fixtures, x => x.FileName == "c-06-a-template-drift-ua2.pdf");
        Assert.True(baseline.Report.Committed);

        var drifted = Assert.Single(fixtures, x => x.FileName == "c-06-b-template-drift-ua2.pdf");

        // Drift must surface as a cardinality failure naming the rule and the observed count.
        Assert.Contains(
            drifted.Report.Diagnostics,
            d => d.StartsWith("RuleCardinalityMismatch", StringComparison.Ordinal) &&
                 d.Contains("c06-title") &&
                 d.Contains("observed 0"));

        // And it must not silently sweep the drifted title into an artifact. This assertion is the
        // reason the fixture exists, so the diagnostic is deliberately left unsuppressed.
        Assert.Contains(
            drifted.Report.TemplateDifferences,
            x => x.Kind == RemediationTemplateDifferenceKind.UndeclaredArtifact && !x.Suppressed);
        Assert.Contains(
            drifted.Report.AutoArtifacts,
            x => x.Text?.Contains("Statement of Account") == true && x.InventoryItemId == null);
    }

    [Fact]
    public void Remediation_Fixtures_Regenerate_Byte_Identically()
    {
        var first = RemediationFixtureGenerator.GenerateAllUncached()
            .ToDictionary(x => x.FileName, x => x.Bytes);
        var second = RemediationFixtureGenerator.GenerateAllUncached();

        Assert.All(second, fixture =>
            Assert.True(
                first[fixture.FileName].AsSpan().SequenceEqual(fixture.Bytes),
                $"{fixture.FileName} is not byte-identical across regeneration."));
    }

    [Fact]
    public void C23_Label_Value_Pairs_And_Anchors_Select_Correctly()
    {
        var fixtures = RemediationFixtureGenerator.GenerateAll();
        var c23 = Assert.Single(fixtures, x => x.FileName == "c-23-label-value-pairs-ua2.pdf");

        Assert.True(c23.Report.Committed);
        Assert.Contains(c23.Report.Outcomes, x => x.RuleId == "c23-inv-val" && x.Candidates.Any(c => c.RawText == "INV-10042"));
        Assert.Contains(c23.Report.Outcomes, x => x.RuleId == "c23-due-val" && x.Candidates.Any(c => c.RawText == "2026-01-15"));
        Assert.Contains(c23.Report.Outcomes, x => x.RuleId == "c23-po-val" && x.Candidates.Any(c => c.RawText == "PO-9988"));
        Assert.Contains(c23.Report.Outcomes, x => x.RuleId == "c23-total-val" && x.Candidates.Any(c => c.RawText == "$150.00"));

        // The empty-value case. Declared Exactly(0), so a Below that reached past the blank value into
        // the next section header would fail the run rather than pass silently.
        Assert.DoesNotContain(c23.Report.Outcomes, x => x.RuleId == "c23-notes-val");
        Assert.Equal(
            0,
            Assert.Single(c23.Report.RuleEvaluations, x => x.RuleId == "c23-notes-val").Total.InputsMatched);

        // Between, SameColumnAs and NearestTo each select the intended value. Their targets are already
        // owned by the rules above, so matched inputs rather than claims are the evidence.
        int Matched(string ruleId) =>
            Assert.Single(c23.Report.RuleEvaluations, x => x.RuleId == ruleId).Total.InputsMatched;
        Assert.Equal(1, Matched("c23-between"));
        Assert.Equal(1, Matched("c23-same-column"));
        Assert.Equal(1, Matched("c23-nearest"));
    }

    [Fact]
    public void C29_GroupPassFixtures_HaveExactNestedStructureAndStableMcids()
    {
        var fixtures = RemediationFixtureGenerator.GenerateAll();
        var nestedList = Assert.Single(fixtures, x => x.FileName == "c-29-a-nested-list-ua2.pdf");
        var sections = Assert.Single(fixtures, x => x.FileName == "c-29-b-nested-sections-ua2.pdf");

        Assert.True(nestedList.Report.Committed);
        Assert.True(sections.Report.Committed);
        Assert.Empty(nestedList.Report.TemplateDifferences);
        Assert.Empty(sections.Report.TemplateDifferences);

        using (var document = PdfDocument.Open(nestedList.Bytes))
        {
            AccessibilityIntegrityAssert.HasBasicStructureIntegrity(document);
            AccessibilityIntegrityAssert.HasValidTableAndListHierarchy(document);
            Assert.Equal(new[] { "LI" }, ChildTypes(Element(document, "c29a-outer-list")));
            Assert.Equal(new[] { "LBody" }, ChildTypes(Element(document, "c29a-outer-item")));
            Assert.Equal(new[] { "Span", "P", "L" }, ChildTypes(Element(document, "c29a-outer-body")));
            Assert.Equal(new[] { "LI" }, ChildTypes(Element(document, "c29a-inner-list")));
            Assert.Equal(new[] { "LBody" }, ChildTypes(Element(document, "c29a-inner-item")));
            Assert.Equal(new[] { "Span", "P" }, ChildTypes(Element(document, "c29a-inner-body")));
        }

        Assert.Equal(new[] { 2, 3 }, ClaimMcids(nestedList.Report, "c29a-inner-list"));
        Assert.Equal(new[] { 0, 1, 2, 3 }, ClaimMcids(nestedList.Report, "c29a-outer-body"));
        Assert.Equal(new[] { 0, 1, 2, 3 }, ClaimMcids(nestedList.Report, "c29a-outer-item"));
        Assert.Equal(new[] { 0, 1, 2, 3 }, ClaimMcids(nestedList.Report, "c29a-outer-list"));

        using (var document = PdfDocument.Open(sections.Bytes))
        {
            AccessibilityIntegrityAssert.HasBasicStructureIntegrity(document);
            var root = AccessibilityIntegrityAssert.GetRootDocumentElement(document);
            Assert.Equal(
                new[] { "c29b-alpha-outer", "c29b-beta-outer" },
                ChildElements(root).Select(Title).ToArray());
            foreach (var section in new[] { "alpha", "beta" })
            {
                Assert.Equal(
                    new[] { "H1", "P", "Sect" },
                    ChildTypes(Element(document, $"c29b-{section}-outer")));
                Assert.Equal(
                    new[] { "H2", "P" },
                    ChildTypes(Element(document, $"c29b-{section}-inner")));
                Assert.Equal(4, ClaimMcids(sections.Report, $"c29b-{section}-outer").Length);
                Assert.Equal(2, ClaimMcids(sections.Report, $"c29b-{section}-inner").Length);
            }
        }
    }

    [Fact]
    public void C29_DryRunAndCommitProduceTheSameClaimGraph()
    {
        using var document = RemediationFixtureGenerator.CreateC29bInput();
        using var session = document.BeginRemediation(new RemediationSessionConfiguration
            {
                Language = "en-US",
                Title = "C-29 parity",
                StrictConformance = false,
                LeftoverPolicy = RemediationLeftoverPolicy.FailFast
            })
            .Use(RemediationFixtureGenerator.CreateC29bRuleSet());

        var dryRun = session.DryRun();
        var committed = session.Commit();
        static string Shape(RemediationClaim claim) =>
            $"{claim.RuleId}|{claim.ProducedTag}|{claim.GroupPass}|{string.Join(",", claim.RelatedClaims.Select(x => x.RuleId))}";

        Assert.True(committed.Committed);
        Assert.Equal(dryRun.Claims.Select(Shape), committed.Claims.Select(Shape));
        Assert.Empty(dryRun.TemplateDifferences);
        Assert.Empty(committed.TemplateDifferences);
    }

    [Fact]
    public void C24_Ambiguous_And_Repeated_Anchors_Disambiguate_Or_Fail()
    {
        var c24 = Assert.Single(
            RemediationFixtureGenerator.GenerateAll(),
            x => x.FileName == "c-24-ambiguous-anchors-ua2.pdf");
        var diagnostics = c24.Report.Diagnostics;

        // "Date" appears three times on page 1, so a single-match anchor is ambiguous there — and the
        // message must be distinguishable from the absence case, not merely "did not resolve".
        Assert.Contains(
            diagnostics,
            d => d.Contains("'ambiguous-date'") && d.Contains("ambiguous on page 1") && d.Contains("Found 3"));

        // Page 2 has no table, so the table-scoped anchor is absent rather than ambiguous.
        Assert.Contains(
            diagnostics,
            d => d.Contains("'page1-table-date'") && d.Contains("matched no candidates on page 2"));
        Assert.DoesNotContain(
            diagnostics,
            d => d.Contains("'page1-table-date'") && d.Contains("ambiguous"));

        // A page selector turns the page-2 absence into an inactivity note rather than a failure.
        Assert.Contains(
            diagnostics,
            d => d.Contains("'header-date-scoped'") && d.Contains("is not active on page 2"));
        Assert.DoesNotContain(
            diagnostics,
            d => d.Contains("'header-date-scoped'") && d.Contains("matched no candidates"));

        // Style disambiguates the same repeated label: the 16pt header occurrence resolves cleanly.
        Assert.DoesNotContain(
            diagnostics,
            d => d.Contains("'styled-date'") && d.Contains("ambiguous"));

        // NthInReadingOrder is zero-based: index 0 is the header occurrence, index 2 the footer one.
        var first = Assert.Single(c24.Report.Outcomes, x => x.RuleId == "c24-nth-first");
        var third = Assert.Single(c24.Report.Outcomes, x => x.RuleId == "c24-nth-third");
        Assert.Contains(first.Candidates, x => x.RawText.Contains("Header Date"));
        Assert.Contains(third.Candidates, x => x.RawText.Contains("Footer Date"));
    }

    private static PdfDictionary Element(PdfDocument document, string title) =>
        Assert.Single(
            AccessibilityIntegrityAssert.GetStructureElements(document),
            x => string.Equals(Title(x), title, StringComparison.Ordinal));

    private static string? Title(PdfDictionary element) => element.Get<PdfString>(PdfName.T)?.Value;

    private static string[] ChildTypes(PdfDictionary parent) =>
        ChildElements(parent).Select(x => x.Get<PdfName>(PdfName.S)!.Value).ToArray();

    private static IReadOnlyList<PdfDictionary> ChildElements(PdfDictionary parent)
    {
        if (!parent.TryGetValue(PdfName.K, out var kids) || kids == null)
        {
            return Array.Empty<PdfDictionary>();
        }

        var resolved = kids.Resolve();
        if (resolved.Type == PdfObjectType.DictionaryObj)
        {
            var child = resolved.GetAs<PdfDictionary>();
            return child.Get<PdfName>(PdfName.TYPE) == PdfName.StructElem
                ? new[] { child }
                : Array.Empty<PdfDictionary>();
        }
        if (resolved.Type != PdfObjectType.ArrayObj)
        {
            return Array.Empty<PdfDictionary>();
        }
        return resolved.GetAs<PdfArray>()
            .Select(x => x.Resolve().GetAsOrNull<PdfDictionary>())
            .Where(x => x?.Get<PdfName>(PdfName.TYPE) == PdfName.StructElem)
            .Select(x => x!)
            .ToArray();
    }

    private static int[] ClaimMcids(RemediationReport report, string ruleId) =>
        Assert.Single(report.Claims, x => x.RuleId == ruleId)
            .AppliedBindings.SelectMany(x => x.Mcids).Distinct().OrderBy(x => x).ToArray();

    private static void AssertCoordinateEqual(double expected, double actual) =>
        Assert.InRange(actual, expected - 0.000001, expected + 0.000001);

    private static PdfName[] GetChildTypes(PdfDictionary parent) =>
        parent.Get<PdfArray>(PdfName.K)!
            .Select(x => x.Resolve())
            .OfType<PdfDictionary>()
            .Select(x => x.Get<PdfName>(PdfName.S)!)
            .ToArray();
}
