using System;
using System.Linq;
using PdfLexer.Content;
using PdfLexer.DOM;
using PdfLexer.Fonts;
using PdfLexer.Remediation;
using PdfLexer.Content.Model;
using PdfLexer.Writing;
using Xunit;

namespace PdfLexer.Tests;

public class RemediationPlanValidationTests
{
    [Fact]
    public void Commit_FailsWhenSourceRefNoLongerResolves()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
        {
            writer.Font(Standard14Font.GetHelvetica(), 12).Text("Hello").EndText();
        }

        using var session = doc.BeginRemediation(new RemediationSessionConfiguration
        {
            StrictConformance = false
        });

        var error = Assert.Throws<InvalidOperationException>(() => session.Commit(new Rule(
            "bad-source",
            RemediationActions.Custom(ctx =>
            {
                var candidate = new RemediationCandidate(
                    Granularity.Line,
                    "ghost",
                    new PdfRect<double>(0, 0, 10, 10),
                    new PdfRect<double>(0, 0, 10, 10),
                    Array.Empty<StructuredCharacter>(),
                    new[] { new StructuredSourceRef(999, 0, 1) },
                    0,
                    12);
                return new CustomRemediationOutcome(new[] { candidate }, RemediationActions.Tag("P"));
            }, "bad source"))));

        Assert.Contains("no longer resolves", error.Message);
    }

    [Fact]
    public void Commit_FailsForNonContiguousLeaves()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
        {
            writer.Font(Standard14Font.GetHelvetica(), 12).Text("First").EndText();
        }
        using (var writer = page.GetWriter(PageWriteMode.Append))
        {
            writer.Font(Standard14Font.GetHelvetica(), 12).Text("Middle").EndText();
        }
        using (var writer = page.GetWriter(PageWriteMode.Append))
        {
            writer.Font(Standard14Font.GetHelvetica(), 12).Text("Last").EndText();
        }

        using var session = doc.BeginRemediation(new RemediationSessionConfiguration
        {
            StrictConformance = false
        });
        
        // Rule matching "First" and "Last" but skipping "Middle" in a single claim
        var rule = new Rule(
            "non-contiguous",
            RemediationActions.Custom(ctx =>
            {
                var first = ctx.Candidates.First(x => x.Text == "First");
                var last = ctx.Candidates.First(x => x.Text == "Last");
                return new CustomRemediationOutcome(new RemediationCandidate[] { first, last }, RemediationActions.Tag("P"));
            }, "custom"));

        var error = Assert.Throws<InvalidOperationException>(() => session.Commit(rule));
        Assert.Contains("non-contiguous", error.Message);
    }

    [Fact]
    public void Commit_FailsForImpossibleTextSplits()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
        {
            writer.Font(Standard14Font.GetHelvetica(), 12).Text("Hello").EndText();
        }

        using var session = doc.BeginRemediation(new RemediationSessionConfiguration
        {
            StrictConformance = false
        });

        var report = session.DryRun(new Rule(
            "hello",
            RemediationActions.Tag("P"),
            Predicates.Text.Equals("Hello"),
            Granularity.Word));

        Assert.Single(report.Claims);
        Assert.Empty(report.Diagnostics);
    }

    [Fact]
    public void SessionValidate_CatchesDuplicateIdsUnresolvedZoneAndLaterStageAnchors()
    {
        using var doc = PdfDocument.Create();
        using var session = doc.BeginRemediation();

        var duplicates = session.Validate(
            new RuleSet("a", new Rule("same", RemediationActions.Tag("P"))),
            new RuleSet("b", new Rule("same", RemediationActions.Tag("P"))));
        Assert.False(duplicates.IsValid);
        Assert.Contains(duplicates.Errors, x => x.Contains("duplicated"));

        var unresolvedZone = session.Validate(new RuleSet(
            "invoice",
            new[]
            {
                new Rule(
                    "footer",
                    RemediationActions.Artifact(ArtifactSubtype.Pagination),
                    Predicates.Flow.InZone("missing-footer"),
                    Granularity.Line)
            },
            Array.Empty<RemediationAnchor>()));
        Assert.False(unresolvedZone.IsValid);
        Assert.Contains(unresolvedZone.Errors, x => x.Contains("unknown toleranced zone 'missing-footer'"));

        var laterStageAnchor = session.Validate(
            new Rule(
                "anchored",
                RemediationActions.Tag("P"),
                Predicates.Geo.Intersects(LayoutCoord.Anchor("late"))),
            new Rule(
                "late",
                RemediationActions.Lang(ClaimPredicate.Always, "fr-CA"),
                stage: Stage.Refine));
        Assert.False(laterStageAnchor.IsValid);
        Assert.Contains(laterStageAnchor.Errors, x => x.Contains("later-stage"));
    }

    [Fact]
    public void SessionValidate_CatchesNamedAnchorZoneAndFlowReferences()
    {
        using var doc = PdfDocument.Create();
        using var session = doc.BeginRemediation();

        var unresolvedAnchor = session.Validate(new RuleSet(
            "invoice",
            new[]
            {
                new Rule(
                    "value",
                    RemediationActions.Tag("P"),
                    Predicates.Anchor.RightOf("missing-label"),
                    Granularity.Word)
            },
            Array.Empty<RemediationAnchor>()));
        Assert.False(unresolvedAnchor.IsValid);
        Assert.Contains(unresolvedAnchor.Errors, x => x.Contains("unknown anchor 'missing-label'"));

        var unresolvedFlow = session.Validate(new RuleSet(
            "invoice",
            new[]
            {
                new Rule(
                    "items",
                    RemediationActions.Tag("TR"),
                    Predicates.Flow.InFlowRegion("missing-flow"),
                    Granularity.Line)
            },
            Array.Empty<RemediationAnchor>()));
        Assert.False(unresolvedFlow.IsValid);
        Assert.Contains(unresolvedFlow.Errors, x => x.Contains("unknown flow region 'missing-flow'"));

        var unresolvedWithin = session.Validate(new Rule(
            "group",
            RemediationActions.Group("Sect", ClaimPredicates.Within("missing-region")),
            stage: Stage.Group));
        Assert.False(unresolvedWithin.IsValid);
        Assert.Contains(unresolvedWithin.Errors, x => x.Contains("unknown anchor, toleranced zone, or flow region 'missing-region'"));
    }

    [Fact]
    public void SessionValidate_CatchesInvalidZoneAndFlowDeclarations()
    {
        using var doc = PdfDocument.Create();
        using var session = doc.BeginRemediation();

        var invalidZone = session.Validate(new RuleSet(
            "common",
            Array.Empty<Rule>(),
            Array.Empty<RemediationAnchor>(),
            tolerancedZones: new[]
            {
                new TolerancedZone("footer", LayoutCoord.MarginRelative(bottom: 36), Tolerance: -1)
            }));
        Assert.False(invalidZone.IsValid);
        Assert.Contains(invalidZone.Errors, x => x.Contains("negative tolerance"));

        var invalidFlow = session.Validate(new RuleSet(
            "invoice",
            Array.Empty<Rule>(),
            Array.Empty<RemediationAnchor>(),
            flowRegions: new[]
            {
                new FlowRegion("items", FlowBoundary.Anchor("missing-start"), FlowBoundary.Zone("missing-zone"), MaxExtent: -1)
            }));
        Assert.False(invalidFlow.IsValid);
        Assert.Contains(invalidFlow.Errors, x => x.Contains("negative max extent"));
        Assert.Contains(invalidFlow.Errors, x => x.Contains("unknown anchor 'missing-start'"));
        Assert.Contains(invalidFlow.Errors, x => x.Contains("unknown toleranced zone 'missing-zone'"));
    }

    [Fact]
    public void Commit_FailsWhenNamedAnchorIsAmbiguous()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
        {
            writer.Font(Standard14Font.GetHelvetica(), 12).Text("Total").EndText();
        }
        using (var writer = page.GetWriter(PageWriteMode.Append))
        {
            writer.Font(Standard14Font.GetHelvetica(), 12).Text("Total").EndText();
        }

        using var session = doc.BeginRemediation(new RemediationSessionConfiguration
        {
            StrictConformance = false
        });
        session.Use(new RuleSet(
            "invoice",
            new[]
            {
                new Rule(
                    "value",
                    RemediationActions.Tag("P"),
                    Predicates.Anchor.RightOf("total-label"),
                    Granularity.Word)
            },
            new[] { RemediationAnchor.TextLabel("total-label", "Total") }));

        var error = Assert.Throws<InvalidOperationException>(() => session.Commit());
        Assert.Contains("ambiguous", error.Message);
    }

    [Fact]
    public void DryRun_ReportsOverlappingFlowRegions()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
        {
            writer.Save().Font(Base14.Helvetica, 12).TextMove(100, 700).Text("Item Qty Amount").Restore();
            writer.Save().Font(Base14.Helvetica, 12).TextMove(100, 680).Text("Widget 2 10.00").Restore();
            writer.Save().Font(Base14.Helvetica, 12).TextMove(100, 640).Text("Subtotal").Restore();
        }

        var anchors = new[]
        {
            RemediationAnchor.TableHeader("header", "Item", "Qty", "Amount"),
            RemediationAnchor.TextLabel("subtotal", "Subtotal")
        };
        var flowA = new FlowRegion("items-a", FlowBoundary.Anchor("header"), FlowBoundary.Anchor("subtotal"));
        var flowB = new FlowRegion("items-b", FlowBoundary.Anchor("header"), FlowBoundary.Anchor("subtotal"));
        using var session = doc.BeginRemediation(new RemediationSessionConfiguration
        {
            StrictConformance = false
        });
        session.Use(new RuleSet(
            "invoice",
            Array.Empty<Rule>(),
            anchors,
            flowRegions: new[] { flowA, flowB }));

        var report = session.DryRun();

        Assert.Contains(report.Diagnostics, x => x.Contains("overlap"));
    }

    [Fact]
    public void Commit_FailsWhenGroupActionMatchesClaimsWithoutBindings()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
        {
            writer.Font(Standard14Font.GetHelvetica(), 12).Text("Hello").EndText();
        }

        using var session = doc.BeginRemediation(new RemediationSessionConfiguration
        {
            StrictConformance = false
        });
        
        // Rule 1: Artifact (no structure binding)
        // Rule 2: Grouping the artifact (invalid)
        var report = session.DryRun(
            new Rule("artifact", RemediationActions.Artifact(ArtifactSubtype.Pagination), Predicates.Text.Equals("Hello"), Granularity.Word),
            new Rule("group", RemediationActions.Group("Sect", ClaimPredicates.FromRule("artifact")), stage: Stage.Group)
        );

        var error = Assert.Throws<InvalidOperationException>(() => session.Commit(
            new Rule("artifact", RemediationActions.Artifact(ArtifactSubtype.Pagination), Predicates.Text.Equals("Hello"), Granularity.Word),
            new Rule("group", RemediationActions.Group("Sect", ClaimPredicates.FromRule("artifact")), stage: Stage.Group)
        ));
        
        Assert.Contains("does not produce a structure binding", error.Message);
    }

    [Fact]
    public void Commit_FailsForTableInferenceFailure()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
        {
            writer.Font(Standard14Font.GetHelvetica(), 12).Text("Just One Item").EndText();
        }

        using var session = doc.BeginRemediation(new RemediationSessionConfiguration
        {
            StrictConformance = false
        });

        var report = session.DryRun(new Rule(
            "table",
            RemediationActions.Table(),
            stage: Stage.Group));

        Assert.Contains(report.Diagnostics, x => x.Contains("at least two candidates"));
    }
}
