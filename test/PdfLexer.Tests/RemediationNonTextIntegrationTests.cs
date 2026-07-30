using System;
using System.IO;
using System.Linq;
using System.Text;
using PdfLexer.DOM;
using PdfLexer.Remediation;
using PdfLexer.Writing;
using Xunit;

namespace PdfLexer.Tests;

public class RemediationNonTextIntegrationTests
{
    [Fact]
    public void FormCanBeSelectedByResourceName_AndPathsAreReportedAsAutoArtifacts()
    {
        using var doc = CreateGraphicalDocument();
        var ruleSet = new RuleSet(
            "graphics",
            new Rule(
                "logo",
                RemediationActions.Tag("Figure"),
                Predicates.Content.ResourceName("F1"),
                CandidateSelector.Content(RemediationCandidateKind.Form)));
        using var session = doc.BeginRemediation(new RemediationSessionConfiguration
        {
            StrictConformance = false,
            LeftoverPolicy = RemediationLeftoverPolicy.AutoArtifact
        }).Use(ruleSet);

        var dryRun = session.DryRun();

        var logo = Assert.Single(dryRun.Claims);
        var candidate = Assert.IsType<ContentRemediationCandidate>(Assert.Single(logo.Candidates));
        Assert.Equal("F1", candidate.ResourceName);
        Assert.StartsWith("sha256:", candidate.ResourceIdentity);
        Assert.NotEqual(candidate.BoundingBox, candidate.RelativeBoundingBox);
        Assert.Equal(2, dryRun.AutoArtifacts.Count(x => x.CandidateKind == RemediationCandidateKind.Path));
        Assert.All(
            dryRun.AutoArtifacts.Where(x => x.CandidateKind == RemediationCandidateKind.Path),
            x => Assert.NotNull(x.CandidateId));

        var committed = session.Commit();
        Assert.True(committed.Committed);
        Assert.All(committed.AutoArtifacts, x =>
            Assert.Equal(RemediationAutoArtifactDisposition.Applied, x.Disposition));
    }

    [Fact]
    public void SemanticAssertionFailsWhenExpectedTagIsMissing()
    {
        using var doc = CreateGraphicalDocument();
        var rule = new Rule(
            "logo",
            RemediationActions.Tag("Figure"),
            Predicates.Content.ResourceName("does-not-exist"),
            CandidateSelector.Content(RemediationCandidateKind.Form));
        var assertion = new StructureElementCountAssertion(
            "one-logo", "Figure", AssertionCount.Exactly(1));
        var ruleSet = new RuleSet(
            "graphics",
            new[] { rule },
            Array.Empty<RemediationAnchor>(),
            assertions: new[] { assertion });

        using var session = doc.BeginRemediation(new RemediationSessionConfiguration
        {
            StrictConformance = false,
            LeftoverPolicy = RemediationLeftoverPolicy.AutoArtifact
        }).Use(ruleSet);
        var report = session.DryRun();

        var outcome = Assert.Single(report.AssertionOutcomes);
        Assert.False(outcome.Passed);
        Assert.Contains("SemanticAssertionFailed", string.Join("\n", report.Diagnostics));
    }

    [Fact]
    public void JsonContentPredicateRoundTripsIntoExecutableRuleModel()
    {
        const string json = """
            {
              "schema": "pdflexer.remediation.ruleset.v1",
              "id": "graphics",
              "rules": [{
                "id": "logo",
                "candidates": { "kind": "content", "types": ["form"] },
                "predicate": { "kind": "content-resource-name", "name": "F1" },
                "action": { "kind": "tag", "tag": "Figure" }
              }]
            }
            """;
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

        var job = SerializedRemediationRules.Load(stream);

        var rule = Assert.Single(job.RuleSet.Rules);
        Assert.IsType<CandidateSelector.ContentSelector>(rule.Candidates);
        Assert.IsType<ContentRemediationPredicate>(rule.Predicate);
    }

    [Fact]
    public void ReusedFormInvocationsHaveDistinctCandidatesAndDocumentWideReuseCount()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        var formWriter = new FormWriter(20, 20);
        formWriter.Rect(0, 0, 20, 20).Fill();
        var form = formWriter.Complete();
        using (var writer = page.GetWriter())
        {
            writer.Form(form, 40, 700);
            writer.Form(form, 100, 700);
        }

        var rule = new Rule(
            "reused",
            RemediationActions.Artifact(ArtifactSubtype.Layout),
            Predicates.Content.ResourceUseCount(NumericOperator.Equal, 2),
            CandidateSelector.Content(RemediationCandidateKind.Form));
        using var session = doc.BeginRemediation(new RemediationSessionConfiguration
        {
            StrictConformance = false
        }).Use(new RuleSet("graphics", rule));

        var report = session.DryRun();
        var candidates = report.Claims.SelectMany(x => x.Candidates)
            .Cast<ContentRemediationCandidate>().ToArray();

        Assert.Equal(2, candidates.Length);
        Assert.All(candidates, x => Assert.Equal(2, x.ResourceUseCount));
        Assert.Single(candidates.Select(x => x.ResourceIdentity).Distinct());
        Assert.Equal(2, candidates.Select(x => x.CandidateId).Distinct().Count());
    }

    [Fact]
    public void ParentChildAssertionFailsWhenNoParentExists()
    {
        using var doc = PdfDocument.Create();
        doc.AddPage(PageSize.LETTER);
        var assertion = new ParentChildShapeAssertion(
            "rows", "TR", new[] { "TD" }, AssertionCount.Exactly(2));
        var ruleSet = new RuleSet(
            "table",
            Array.Empty<Rule>(),
            Array.Empty<RemediationAnchor>(),
            assertions: new[] { assertion });
        using var session = doc.BeginRemediation(new RemediationSessionConfiguration
        {
            StrictConformance = false
        }).Use(ruleSet);

        var report = session.DryRun();

        Assert.False(Assert.Single(report.AssertionOutcomes).Passed);
    }

    [Fact]
    public void FigureInvocationCanReceiveAltText()
    {
        using var doc = CreateGraphicalDocument();
        using var session = doc.BeginRemediation(new RemediationSessionConfiguration
        {
            StrictConformance = false,
            LeftoverPolicy = RemediationLeftoverPolicy.AutoArtifact
        });

        var report = session.Commit(
            new Rule(
                "logo",
                RemediationActions.Tag("Figure"),
                Predicates.Content.ResourceName("F1"),
                CandidateSelector.Content(RemediationCandidateKind.Form)),
            new Rule(
                "logo-alt",
                RemediationActions.Alt(ClaimPredicates.FromRule("logo"), "Company logo"),
                stage: Stage.Refine));

        Assert.True(report.Committed);
        var figure = Assert.Single(doc.Structure.GetRoot().Children, x => x.Type == "Figure");
        Assert.Equal("Company logo", figure.Alt);
        Assert.Single(figure.ContentItems);
    }

    [Fact]
    public void VisibleExistingAnnotationIsInventoriedAsStrictConformanceBlocker()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        page.NativeObject[PdfName.Annots] = new PdfArray
        {
            new PdfDictionary
            {
                [PdfName.Subtype] = PdfName.Link,
                [PdfName.Rect] = new PdfArray { 10, 10, 30, 30 }
            }
        };
        using var session = doc.BeginRemediation(new RemediationSessionConfiguration
        {
            StrictConformance = true,
            LeftoverPolicy = RemediationLeftoverPolicy.AutoArtifact
        });

        var report = session.DryRun(new Rule(
            "nothing",
            RemediationActions.Tag("P"),
            Predicates.Text.Equals("not present"),
            CandidateSelector.Text(Granularity.Paragraph)));

        var annotation = Assert.Single(report.AnnotationInventory);
        Assert.Equal("Link", annotation.Subtype);
        Assert.True(annotation.BlocksConformance);
        Assert.Contains(report.Diagnostics, x => x.Contains("UnmodeledAnnotation", StringComparison.Ordinal));
    }

    private static PdfDocument CreateGraphicalDocument()
    {
        var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        page.CropBox = new PdfRectangle(new PdfArray { 20, 30, 612, 792 });

        var logoWriter = new FormWriter(40, 20);
        logoWriter.Rect(0, 0, 40, 20).Fill();
        var logo = logoWriter.Complete();

        using var writer = page.GetWriter();
        writer.Form(logo, 100, 700);
        writer.Rect(20, 30, 592, 742).Fill();
        writer.MoveTo(40, 500).LineTo(560, 500).Stroke();
        return doc;
    }
}
