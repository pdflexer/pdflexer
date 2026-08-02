using System;
using System.Collections.Generic;
using System.Linq;
using PdfLexer.DOM;
using PdfLexer.Remediation;
using Xunit;

namespace PdfLexer.Tests;

[Collection(VeraPdfTestCollection.Name)]
public class RemediationVeraPdfTests
{
    public static IEnumerable<object[]> GetCommittedProgramFixtures()
    {
        var active = RemediationCorpusManifest.Load().Cases
            .Where(x => x.Status == RemediationCorpusStatus.Active &&
                        x.Outcome is RemediationCorpusOutcome.Commit or RemediationCorpusOutcome.CommitWithAcknowledgedAssertion)
            .ToArray();

        foreach (var item in active)
        {
            foreach (var profile in new[] { PdfUaProfile.PdfUa1, PdfUaProfile.PdfUa2 })
            {
                yield return new object[] { item.Id, profile };
            }
        }
    }

    [VeraPdfTheory]
    [MemberData(nameof(GetCommittedProgramFixtures))]
    public void EveryCommittedProgramFixtureValidatesForItsProfile(string caseId, PdfUaProfile profile)
    {
        var manifest = RemediationCorpusManifest.Load();
        var item = Assert.Single(manifest.Cases, x => x.Id == caseId);

        using var source = RemediationCorpusManifestTests.CreateInput(item.InputFactory);
        using var document = PdfDocument.Open(source.Save());
        using var session = document.BeginRemediation(new RemediationSessionConfiguration
        {
            Language = "en-US",
            Title = $"Remediated {item.Name}",
            Profile = profile,
            StrictConformance = true,
            DiagnosticStrictness = item.Outcome == RemediationCorpusOutcome.CommitWithAcknowledgedAssertion
                ? RemediationDiagnosticStrictness.Permissive
                : RemediationDiagnosticStrictness.Strict,
            RunMode = RemediationRunMode.Enforced
        }).Use(RemediationCorpusManifest.LoadProgram(item, profile));

        foreach (var suppression in item.Suppressions)
            session.Suppress(suppression.Code, suppression.Scope, suppression.Reason);

        var committed = session.Commit();
        Assert.True(committed.Committed, $"{item.Id}/{profile} failed to commit");

        var pdfBytes = document.Save();

        var result = VeraPdfValidation.Validate(pdfBytes, profile);
        Assert.False(result.ProcessingFailed, $"veraPDF processing failed for {item.Id}/{profile}: {result.StandardError}");
        Assert.True(result.IsCompliant,
            $"{item.Id}/{profile} failed veraPDF validation:{Environment.NewLine}" +
            $"Profile: {result.ProfileName}, Failed Rules: {result.FailedRules}, Failed Checks: {result.FailedChecks}{Environment.NewLine}" +
            $"{result.Report}");
    }

    [VeraPdfFact]
    public void InvalidTdTrHierarchyFailsPdfUa1Baseline()
    {
        // Negative control test: verify veraPDF actively detects invalid structure hierarchy (TD directly under Document root)
        // in an otherwise valid, fully committed document with complete accessibility setup.
        using var source = RemediationFixtureGenerator.CreateInvoiceInput();
        using var document = PdfDocument.Open(source.Save());
        document.ApplyAccessibilitySetup("en-US", "Invalid Hierarchy Test", PdfUaProfile.PdfUa1, strictConformance: true);
        var page = document.Pages[0];

        // Create an invalid structure tree: TD directly under Document root without Table/TR
        var root = document.Structure.GetRoot();
        var mcid = PdfLexer.Writing.McidAllocator.Allocate(page);
        var td = document.Structure.AddElement("TD");

        var pdfBytes = document.Save();

        var result = VeraPdfValidation.Validate(pdfBytes, PdfUaProfile.PdfUa1);
        Assert.False(result.ProcessingFailed, $"veraPDF processing failed: {result.StandardError}");
        Assert.False(result.IsCompliant, "veraPDF negative control failed: invalid TD directly under Document root was expected to fail validation");
        Assert.True(result.FailedChecks > 0 || result.FailedRules > 0);
    }
}
