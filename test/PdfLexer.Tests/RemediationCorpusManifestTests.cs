using System;
using System.IO;
using System.Linq;
using PdfLexer.DOM;
using PdfLexer.Remediation;
using Xunit;

namespace PdfLexer.Tests;

public sealed class RemediationCorpusManifestTests
{
    [Fact]
    public void ManifestIsStrictCompleteAndHasUniqueOutputs()
    {
        var manifest = RemediationCorpusManifest.Load();

        Assert.Equal(25, manifest.Cases.Count);
        Assert.Equal(25, manifest.Cases.Select(x => x.InputFactory).Distinct(StringComparer.Ordinal).Count()
            + DuplicateInputFactoryAllowances(manifest));
        Assert.Equal(25, manifest.Cases.Select(x => x.OutputBase).Distinct(StringComparer.Ordinal).Count());
        Assert.All(manifest.Cases, item =>
        {
            Assert.False(string.IsNullOrWhiteSpace(item.Id));
            Assert.NotNull(item.Expected);
        });
    }

    [Fact]
    public void EveryActiveProgramCompilesAndRoundTripsByteStably()
    {
        var active = RemediationCorpusManifest.Load().Cases
            .Where(x => x.Status == RemediationCorpusStatus.Active)
            .ToArray();

        foreach (var item in active)
        {
            foreach (var profile in new[] { PdfUaProfile.PdfUa1, PdfUaProfile.PdfUa2 })
            {
                var program = RemediationCorpusManifest.LoadProgram(item, profile);
                var compiled = RemediationProgramCompiler.Compile(program);
                Assert.True(compiled.IsValid,
                    $"{item.Id}/{profile}:{Environment.NewLine}{string.Join(Environment.NewLine, compiled.Errors)}");

                using var first = new MemoryStream();
                SerializedRemediationProgram.Save(program, first);
                first.Position = 0;
                var loaded = SerializedRemediationProgram.Load(first, item.Id);
                using var second = new MemoryStream();
                SerializedRemediationProgram.Save(loaded, second);
                Assert.Equal(first.ToArray(), second.ToArray());
            }
        }
    }

    [Fact]
    public void EveryActiveProgramDryRunsAndCommitsWithoutBlockingDiagnostics()
    {
        foreach (var item in RemediationCorpusManifest.Load().Cases
                     .Where(x => x.Status == RemediationCorpusStatus.Active))
        {
            foreach (var profile in new[] { PdfUaProfile.PdfUa1, PdfUaProfile.PdfUa2 })
            {
                using var source = CreateInput(item.InputFactory);
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
                    RunMode = item.Outcome == RemediationCorpusOutcome.AuthoringDiagnose
                        ? RemediationRunMode.Authoring
                        : RemediationRunMode.Enforced
                }).Use(RemediationCorpusManifest.LoadProgram(item, profile));

                foreach (var suppression in item.Suppressions)
                    session.Suppress(suppression.Code, suppression.Scope, suppression.Reason);

                var dryRun = session.DryRun();
                AssertExpectedCounts(item, dryRun);
                if (item.Id == "c-04-c")
                {
                    var absorption = Assert.Single(dryRun.RegionAbsorptions);
                    Assert.Equal("footer", absorption.ArtifactId);
                    Assert.Equal(ArtifactSubtype.Pagination, absorption.Subtype);
                    Assert.Equal(ArtifactSemanticSubtype.Footer, absorption.SemanticSubtype);
                    Assert.Empty(dryRun.Suppressions);
                }
                AssertExpectedDiagnostics(item, dryRun);
                var blocking = dryRun.RuntimeDiagnostics.Where(x => x.IsBlocking).ToArray();
                if (item.Outcome is RemediationCorpusOutcome.Diagnose or
                    RemediationCorpusOutcome.AuthoringDiagnose)
                {
                    if (item.Outcome == RemediationCorpusOutcome.Diagnose)
                        Assert.NotEmpty(blocking);
                    else
                        Assert.DoesNotContain(dryRun.RuntimeDiagnostics, x => x.IsBlocking);
                    continue;
                }
                Assert.True(blocking.Length == 0,
                    $"{item.Id}/{profile}:{Environment.NewLine}" +
                    $"{string.Join(Environment.NewLine, blocking.Select(x => $"{x.Code}: {x.Message}"))}{Environment.NewLine}" +
                    $"Unaccounted: {string.Join(" | ", dryRun.UnaccountedContent.Select(x => x.RawText ?? x.CandidateId))}");
                var committed = session.Commit();
                Assert.True(committed.Committed);
                Assert.DoesNotContain(committed.RuntimeDiagnostics, x => x.IsBlocking);
                using var committedDocument = PdfDocument.Open(document.Save());
                AccessibilityIntegrityAssert.HasBasicStructureIntegrity(committedDocument);
                AccessibilityIntegrityAssert.HasOnlyTaggedOrArtifactContent(committedDocument);
            }
        }
    }

    [Fact]
    public void ActiveCorpusExercisesFragmentsAnchorsAndOccurrenceBoundaries()
    {
        var manifest = RemediationCorpusManifest.Load();
        var statementCase = Assert.Single(manifest.Cases, x => x.Id == "statement");
        var repeatedCase = Assert.Single(manifest.Cases, x => x.Id == "c-30");

        foreach (var profile in new[] { PdfUaProfile.PdfUa1, PdfUaProfile.PdfUa2 })
        {
            var statement = RemediationCorpusManifest.LoadProgram(statementCase, profile);
            Assert.Single(statement.Fragments, x => x.Id == "address");
            Assert.Single(statement.Anchors, x => x.Id == "ship-to-label-anchor");
            var compiledStatement = RemediationProgramCompiler.Compile(statement);
            Assert.True(compiledStatement.IsValid, string.Join(Environment.NewLine, compiledStatement.Errors));
            Assert.Contains(SlotRef.Absolute("/bill-to-address/line"), compiledStatement.Slots.Keys);
            Assert.Contains(SlotRef.Absolute("/ship-to-address/line"), compiledStatement.Slots.Keys);

            using (var source = CreateInput(statementCase.InputFactory))
            using (var document = PdfDocument.Open(source.Save()))
            using (var session = document.BeginRemediation(Configuration(profile)).Use(compiledStatement))
            {
                var report = session.DryRun();
                var billToClaims = report.Claims.Where(x => x.ProgramSlot?.Path == "/bill-to-address/line").ToList();
                var shipToClaims = report.Claims.Where(x => x.ProgramSlot?.Path == "/ship-to-address/line").ToList();
                Assert.Equal(3, billToClaims.Count);
                Assert.Equal(1, shipToClaims.Count);
                Assert.DoesNotContain(report.RuntimeDiagnostics, x => x.IsBlocking);
            }

            var repeated = RemediationCorpusManifest.LoadProgram(repeatedCase, profile);
            var compiledRepeated = RemediationProgramCompiler.Compile(repeated);
            Assert.True(compiledRepeated.IsValid, string.Join(Environment.NewLine, compiledRepeated.Errors));
            Assert.Single(repeated.Boundaries, x => x.Id == "section-start");
            var namedBoundary = Assert.IsType<OccurrenceBoundary.StartsOnBoundary>(
                compiledRepeated.Slots[SlotRef.Absolute("/section")].Node.OccurrenceBoundary);
            Assert.Equal("section-start", namedBoundary.Id);

            using var repeatedSource = CreateInput(repeatedCase.InputFactory);
            using var repeatedDocument = PdfDocument.Open(repeatedSource.Save());
            using var repeatedSession = repeatedDocument.BeginRemediation(Configuration(profile)).Use(compiledRepeated);
            var repeatedReport = repeatedSession.DryRun();
            Assert.Equal(2, repeatedReport.OccurrencePartitions.Count(x =>
                x.CompositeSlot == SlotRef.Absolute("/section")));
            Assert.All(repeatedReport.OccurrencePartitions, x => Assert.NotEmpty(x.Activations));
            Assert.DoesNotContain(repeatedReport.RuntimeDiagnostics, x => x.IsBlocking);
        }
    }

    [Fact(Skip = "CAP-ANNOTATION-ADOPTION: annotation ownership, adoption, and destination relationships require a separate architecture milestone.")]
    public void C08ExistingAnnotationsRunsThroughProgramCorpus() =>
        throw new NotSupportedException();

    [Fact(Skip = "CAP-CONTINUED-TABLE: interleaved repeated headers, continued-table identity, and cross-page compositional regions require a separate architecture milestone.")]
    public void C12ContinuedTableRunsThroughProgramCorpus() =>
        throw new NotSupportedException();

    private static int DuplicateInputFactoryAllowances(RemediationCorpusDefinition manifest) =>
        manifest.Cases.Count - manifest.Cases.Select(x => x.InputFactory).Distinct(StringComparer.Ordinal).Count();

    private static void AssertExpectedCounts(RemediationCorpusCase item, RemediationReport report)
    {
        foreach (var expected in item.Expected.Bindings)
        {
            var actual = Assert.Single(report.BindingEvaluations, x => x.BindingId == expected.Key);
            Assert.Equal(expected.Value, actual.AppliedClaims);
        }

        foreach (var expected in item.Expected.Slots)
        {
            var actual = report.Claims.Count(x => x.ProgramSlot?.Path == expected.Key);
            Assert.Equal(expected.Value, actual);
        }

        foreach (var expected in item.Expected.Artifacts)
        {
            var actual = report.Claims.Count(x => x.ArtifactId == expected.Key);
            Assert.Equal(expected.Value, actual);
        }
    }

    private static void AssertExpectedDiagnostics(RemediationCorpusCase item, RemediationReport report)
    {
        foreach (var expected in item.Expected.Diagnostics)
        {
            Assert.Contains(report.RuntimeDiagnostics, actual =>
                actual.Code == expected.Code &&
                actual.Disposition == expected.Disposition &&
                (expected.Slot == null || actual.ProgramSlot?.Path == expected.Slot) &&
                (expected.Binding == null || actual.BindingId == expected.Binding));
        }
    }

    private static RemediationSessionConfiguration Configuration(PdfUaProfile profile) => new()
    {
        Language = "en-US",
        Title = "Remediation corpus feature coverage",
        Profile = profile,
        StrictConformance = true
    };

    internal static PdfDocument CreateInput(string id) => id switch
    {
        "invoice" => RemediationFixtureGenerator.CreateInvoiceInput(),
        "strict-invoice" => RemediationFixtureGenerator.CreateStrictInvoiceInput(),
        "statement" => RemediationFixtureGenerator.CreateStatementInput(),
        "report" => RemediationFixtureGenerator.CreateReportInput(),
        "form" => RemediationFixtureGenerator.CreateFormInput(),
        "multi-column" => RemediationFixtureGenerator.CreateMultiColumnInput(),
        "mixed-page-sizes" => RemediationFixtureGenerator.CreateMixedPageSizeInput(),
        "c-01" => RemediationFixtureGenerator.CreateC01Input(),
        "c-02" => RemediationFixtureGenerator.CreateC02Input(),
        "c-03" => RemediationFixtureGenerator.CreateC03Input(),
        "c-04" => RemediationFixtureGenerator.CreateC04Input(),
        "c-05" => RemediationFixtureGenerator.CreateC05Input(),
        "c-10" => RemediationFixtureGenerator.CreateC10Input(),
        "c-30" => RemediationFixtureGenerator.CreateC30Input(),
        "c-23" => RemediationFixtureGenerator.CreateC23Input(),
        "c-24" => RemediationFixtureGenerator.CreateC24Input(),
        "c-29-a" => RemediationFixtureGenerator.CreateC29aInput(),
        "c-29-b" => RemediationFixtureGenerator.CreateC29bInput(),
        "c-06-a" => RemediationFixtureGenerator.CreateC06aInput(),
        "c-06-b" => RemediationFixtureGenerator.CreateC06bInput(),
        _ => throw new InvalidDataException($"Unknown remediation input factory '{id}'.")
    };
}
