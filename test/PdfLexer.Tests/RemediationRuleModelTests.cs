using System;
using System.IO;
using System.Linq;
using System.Text;
using PdfLexer.Content;
using PdfLexer.Content.Model;
using PdfLexer.DOM;
using PdfLexer.Fonts;
using PdfLexer.Remediation;
using Xunit;

namespace PdfLexer.Tests;

public class RemediationRuleModelTests
{
    [Fact]
    public void SerializedRuleSet_LoadsJsonCliFormat()
    {
        const string json = """
        {
          "schema": "pdflexer.remediation.ruleset.v1",
          "ruleSet": { "id": "invoice-v1" },
          "session": {
            "language": "en-US",
            "title": "Remediated Invoice",
            "profile": "pdfUa1",
            "leftoverPolicy": "failFast",
            "debugWrite": true
          },
          "anchors": [
            { "id": "invoice-label", "kind": "regex", "pattern": "^Invoice\\s*#$", "granularity": "line" },
            { "id": "subtotal-label", "kind": "textLabel", "text": "Subtotal" },
            { "id": "items-header", "kind": "tableHeader", "headers": ["Item", "Qty", "Amount"] }
          ],
          "zones": [
            { "id": "footer", "bounds": { "kind": "marginRelative", "bottom": 42 }, "tolerance": 6 }
          ],
          "flowRegions": [
            {
              "id": "line-items",
              "start": { "kind": "anchor", "id": "items-header" },
              "end": { "kind": "anchor", "id": "subtotal-label" }
            }
          ],
          "rules": [
            {
              "id": "invoice-title",
              "stage": "classify",
              "granularity": "line",
              "pages": { "kind": "first" },
              "action": { "kind": "tag", "tag": "H1" },
              "predicate": { "kind": "textStartsWith", "text": "Invoice" }
            },
            {
              "id": "line-item-row",
              "stage": "classify",
              "granularity": "line",
              "action": { "kind": "tag", "tag": "TR" },
              "predicate": { "kind": "flowInRegion", "id": "line-items" }
            },
            {
              "id": "line-items-table",
              "stage": "group",
              "action": {
                "kind": "table",
                "over": { "kind": "fromRule", "ruleId": "line-item-row" },
                "columns": [72, 300, 380, 470]
              }
            },
            {
              "id": "document-lang",
              "stage": "refine",
              "action": {
                "kind": "lang",
                "over": { "kind": "statusIs", "status": "applied" },
                "language": "en-US"
              }
            }
          ]
        }
        """;

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var job = SerializedRemediationRules.Load(stream);

        Assert.Equal("en-US", job.Session.Language);
        Assert.Equal("Remediated Invoice", job.Session.Title);
        Assert.Equal(RemediationLeftoverPolicy.FailFast, job.Session.LeftoverPolicy);
        Assert.True(job.Session.DebugWrite);
        Assert.Equal("invoice-v1", job.RuleSet.Id);
        Assert.Equal(3, job.RuleSet.Anchors.Count);
        Assert.Single(job.RuleSet.TolerancedZones);
        Assert.Single(job.RuleSet.FlowRegions);
        Assert.Equal(new[] { "invoice-title", "line-item-row", "line-items-table", "document-lang" }, job.RuleSet.Rules.Select(x => x.Id).ToArray());
        Assert.Empty(job.RuleSet.Rules.SelectMany(x => x.ValidateShape()));
    }

    [Fact]
    public void Rule_StoresDeclarativeShapeAndValidatesConfidence()
    {
        var rule = new Rule(
            "heading",
            RemediationActions.Tag("H1"),
            Predicates.Text.StartsWith("Invoice"),
            Granularity.Paragraph,
            PageSelector.First,
            Stage.Classify,
            minConfidence: 0.8);

        Assert.Equal("heading", rule.Id);
        Assert.Equal(Stage.Classify, rule.Stage);
        Assert.Equal(Granularity.Paragraph, rule.Granularity);
        Assert.Same(PageSelector.First, rule.Pages);
        Assert.Equal(0.8, rule.MinConfidence);
        Assert.Empty(rule.ValidateShape());

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Rule("bad", RemediationActions.Tag("P"), minConfidence: 1.1));
    }

    [Fact]
    public void LeafSelection_MapsGranularityToStructuredItemsAndContentLeaves()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
        {
            writer.Font(Standard14Font.GetHelvetica(), 12).Text("Hello World").EndText();
        }

        var textPage = page.GetStructuredText();
        var content = page.GetContentNodes<double>();

        var characters = textPage.GetCandidates(Granularity.Character);
        var words = textPage.GetCandidates(Granularity.Word);
        var lines = textPage.GetCandidates(Granularity.Line);
        var paragraphs = textPage.GetCandidates(Granularity.Paragraph);

        Assert.Equal(textPage.Characters.Count, characters.Count);
        Assert.Equal(2, words.Count);
        Assert.Single(lines);
        Assert.Single(paragraphs);
        Assert.Equal("Hello", words[0].Text);

        var targets = words[0].FindTargets(content);
        var target = Assert.Single(targets);
        Assert.False(target.IsWholeItem);
        Assert.Equal(0, target.TextRange!.StartCharacterIndex);
        Assert.Equal(5, target.TextRange.CharacterCount);

        var leaves = words[0].MaterializeLeaves(content);
        var leaf = Assert.Single(leaves);
        Assert.Equal("Hello", Assert.IsType<TextContent<double>>(leaf).Text);
        Assert.Equal(new[] { "Hello", " World" }, content.OfType<TextContent<double>>().Select(x => x.Text).ToArray());
    }

    [Fact]
    public void InlineMaterialization_WrapsOnlySelectedWordInsideTextOperator()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
        {
            writer.Font(Standard14Font.GetHelvetica(), 12).Text("Hello World").EndText();
        }

        var textPage = page.GetStructuredText();
        var content = page.GetContentNodes<double>();
        var hello = textPage.GetCandidates(Granularity.Word).Single(x => x.Text == "Hello");

        var leaves = hello.MaterializeLeaves(content);
        var wrapper = content.Wrap(
            leaves,
            new MarkedContent("Span")
            {
                InlineProps = new PdfDictionary
                {
                    [PdfName.MCID] = new PdfIntNumber(3)
                }
            });

        Assert.Equal("Hello", Assert.IsType<TextContent<double>>(Assert.Single(wrapper.Children)).Text);
        Assert.Equal(" World", Assert.IsType<TextContent<double>>(content[1]).Text);

        var rewritten = doc.AddPage(PageSize.LETTER);
        using (var writer = rewritten.GetWriter())
        {
            writer.AddContent(content);
        }

        var reparsed = rewritten.GetContentNodes<double>();
        var reparsedWrapper = Assert.IsType<MarkedContentGroup<double>>(reparsed[0]);
        Assert.Equal("Hello", Assert.IsType<TextContent<double>>(Assert.Single(reparsedWrapper.Children)).Text);
        Assert.Equal(" World", Assert.IsType<TextContent<double>>(reparsed[1]).Text);

        var rewrittenText = rewritten.GetStructuredText().Characters.Select(x => x.Char).ToArray();
        Assert.Equal("Hello World", new string(rewrittenText));
    }

    [Fact]
    public void TextRangeSplit_ReportsInvalidRangesBeforeMutation()
    {
        var content = TextContent<double>.Create(
            "Hello",
            Standard14Font.GetHelvetica(),
            12);

        Assert.False(content.TrySplitByCharacterRange(
            3,
            10,
            out var before,
            out var selected,
            out var after,
            out var error));
        Assert.Null(before);
        Assert.Null(selected);
        Assert.Null(after);
        Assert.Contains("beyond", error);
    }

    [Fact]
    public void PageSelectors_SelectExpectedPages()
    {
        Assert.Equal(new[] { 0 }, PageSelector.First.SelectPages(5));
        Assert.Equal(new[] { 4 }, PageSelector.Last.SelectPages(5));
        Assert.Equal(new[] { 1, 2, 3 }, PageSelector.Range(1, 3).SelectPages(5));
        Assert.Equal(new[] { 0, 2, 4 }, PageSelector.Parity(PageParity.Odd).SelectPages(5));
        Assert.Equal(new[] { 1, 3 }, PageSelector.Parity(PageParity.Even).SelectPages(5));
        Assert.Equal(new[] { 0, 1, 2 }, PageSelector.Every.SelectPages(3));
    }

    [Fact]
    public void Predicate_CombinatorsFollowBooleanTruthTable()
    {
        var candidate = Candidate("Hello", new PdfRect<double>(0, 0, 10, 10), 12);

        Assert.True(RemediationPredicate.Always.And(RemediationPredicate.Always).Evaluate(candidate).IsMatch);
        Assert.False(RemediationPredicate.Always.And(RemediationPredicate.Never).Evaluate(candidate).IsMatch);
        Assert.True(RemediationPredicate.Always.Or(RemediationPredicate.Never).Evaluate(candidate).IsMatch);
        Assert.False(RemediationPredicate.Never.Or(RemediationPredicate.Never).Evaluate(candidate).IsMatch);
        Assert.True(RemediationPredicate.Never.Not().Evaluate(candidate).IsMatch);
        Assert.False(RemediationPredicate.Always.Not().Evaluate(candidate).IsMatch);
    }

    [Fact]
    public void TextPredicates_EvaluateAgainstCandidateText()
    {
        var candidate = Candidate("Invoice Total", new PdfRect<double>(0, 0, 10, 10), 12);

        Assert.True(Predicates.Text.Matches("^Invoice").Evaluate(candidate).IsMatch);
        Assert.True(Predicates.Text.Contains("Total").Evaluate(candidate).IsMatch);
        Assert.True(Predicates.Text.StartsWith("Invoice").Evaluate(candidate).IsMatch);
        Assert.True(Predicates.Text.Equals("Invoice Total").Evaluate(candidate).IsMatch);
        Assert.False(Predicates.Text.Equals("Other").Evaluate(candidate).IsMatch);
    }

    [Fact]
    public void FontAndGeometryPredicates_EvaluateAvailableStructuredData()
    {
        var candidate = Candidate("Header", new PdfRect<double>(10, 10, 40, 30), 18);

        Assert.True(Predicates.Font.Size(NumericOperator.GreaterThanOrEqual, 16).Evaluate(candidate).IsMatch);
        Assert.False(Predicates.Font.Size(NumericOperator.LessThan, 16).Evaluate(candidate).IsMatch);
        Assert.True(Predicates.Font.Family("Helvetica").Evaluate(candidate).IsMatch);
        Assert.True(Predicates.Font.Weight(NumericOperator.GreaterThanOrEqual, 700).Evaluate(candidate).IsMatch);
        Assert.True(Predicates.Font.Italic().Evaluate(candidate).IsMatch);
        Assert.True(Predicates.Color.IsGrayish().Evaluate(candidate).IsMatch);

        var contains = Predicates.Geo.Contains(LayoutCoord.Absolute(new PdfRect<double>(0, 0, 100, 100)));
        var intersects = Predicates.Geo.Intersects(LayoutCoord.Absolute(new PdfRect<double>(35, 25, 50, 40)));
        var misses = Predicates.Geo.Intersects(LayoutCoord.Absolute(new PdfRect<double>(100, 100, 120, 120)));

        Assert.True(contains.Evaluate(candidate).IsMatch);
        Assert.True(intersects.Evaluate(candidate).IsMatch);
        Assert.False(misses.Evaluate(candidate).IsMatch);
    }

    [Fact]
    public void LayoutCoords_ResolveRelativeZonesAndAnchors()
    {
        var pageBox = new PdfRect<double>(0, 0, 600, 800);
        var config = new RemediationSessionConfiguration
        {
            NamedZoneMargins = new RemediationNamedZoneMargins
            {
                Header = 80,
                Footer = 40,
                Left = 50,
                Right = 60
            }
        };
        var candidate = Candidate("Body", new PdfRect<double>(0, 0, 10, 10), 12, sequenceIndex: 5);
        var context = new RemediationEvaluationContext(
            pageBox: pageBox,
            configuration: config);

        Assert.Equal(
            new PdfRect<double>(0, 728, 600, 800),
            LayoutCoord.Percentage(top: 0.09).Resolve(context, candidate));
        Assert.Equal(
            new PdfRect<double>(0, 720, 600, 800),
            LayoutCoord.MarginRelative(top: 80).Resolve(context, candidate));
        Assert.Equal(
            new PdfRect<double>(50, 40, 540, 720),
            LayoutCoord.Zone(NamedLayoutZone.Body).Resolve(context, candidate));

        var heading = Candidate("Heading", new PdfRect<double>(100, 700, 300, 740), 18, sequenceIndex: 1);
        var anchoredContext = new RemediationEvaluationContext(
            new[] { new RemediationClaim("heading-rule", Granularity.Paragraph, new[] { heading }, "H1") },
            pageBox: pageBox,
            configuration: config);

        Assert.Equal(
            new PdfRect<double>(100, 640, 300, 700),
            LayoutCoord.Anchor("heading-rule", LayoutCoordExpansion.Below(60)).Resolve(anchoredContext, candidate));
        Assert.Equal(
            new PdfRect<double>(90, 690, 310, 750),
            LayoutCoord.Anchor("heading-rule", LayoutCoordExpansion.Inflate(10)).Resolve(anchoredContext, candidate));
    }

    [Fact]
    public void LayoutCoord_AnchorFailsWhenClaimIsMissing()
    {
        var candidate = Candidate("Body", new PdfRect<double>(0, 0, 10, 10), 12);

        var error = Assert.Throws<InvalidOperationException>(() =>
            LayoutCoord.Anchor("missing").Resolve(RemediationEvaluationContext.Empty, candidate));

        Assert.Contains("missing", error.Message);
    }

    [Fact]
    public void RelationalPredicates_EvaluateAgainstClaimContext()
    {
        var heading = Candidate("Heading", new PdfRect<double>(0, 80, 100, 100), 18, sequenceIndex: 1);
        var paragraph = Candidate("Body", new PdfRect<double>(0, 40, 100, 60), 12, sequenceIndex: 2);
        var claim = new RemediationClaim("heading-rule", Granularity.Paragraph, new[] { heading }, "H1");
        var context = new RemediationEvaluationContext(new[] { claim });

        Assert.True(Predicates.Relational.After("heading-rule").Evaluate(context, paragraph).IsMatch);
        Assert.False(Predicates.Relational.Before("heading-rule").Evaluate(context, paragraph).IsMatch);
        Assert.True(Predicates.Relational.InsideClaimOf("heading-rule").Evaluate(context, heading).IsMatch);
        Assert.True(Predicates.Relational.NthChildOfClaim("heading-rule", 0).Evaluate(context, heading).IsMatch);
    }

    [Fact]
    public void ClaimPredicates_AreDistinctAndEvaluateClaimState()
    {
        var first = new RemediationClaim(
            "items",
            Granularity.Paragraph,
            new[] { Candidate("• One", new PdfRect<double>(0, 80, 100, 100), 12, sequenceIndex: 1) },
            "LI")
        {
            PageIndex = 0,
            Action = RemediationActions.Tag("LI"),
            Status = ClaimStatus.Applied,
            RuleSetId = "invoice-common"
        };
        var second = new RemediationClaim(
            "items",
            Granularity.Paragraph,
            new[] { Candidate("• Two", new PdfRect<double>(0, 60, 100, 78), 12, sequenceIndex: 2) },
            "LI")
        {
            PageIndex = 0,
            Action = RemediationActions.Tag("LI"),
            Status = ClaimStatus.Applied,
            RuleSetId = "invoice-common"
        };
        var context = new ClaimPredicateEvaluationContext(new[] { first, second }, PreviousClaim: first);

        var predicate = ClaimPredicates.ClaimIs("LI")
            .And(ClaimPredicates.ActionIs(RemediationActionKind.Tag))
            .And(ClaimPredicates.FromRule("items"))
            .And(ClaimPredicates.FromRuleSet("invoice-common"))
            .And(ClaimPredicates.StatusIs(ClaimStatus.Applied))
            .And(ClaimPredicates.SamePage())
            .And(ClaimPredicates.Consecutive());

        Assert.True(predicate.Evaluate(context, second).IsMatch);
        Assert.False(ClaimPredicates.ClaimIs("P").Evaluate(second).IsMatch);
        Assert.Contains("ClaimIs", predicate.DebugString);
    }

    [Fact]
    public void Actions_ValidateShape()
    {
        var groupInClassify = new Rule("bad-group", RemediationActions.Group("L", ClaimPredicate.Always));
        var reorderInClassify = new Rule("bad-reorder", RemediationActions.ReorderSiblings(ClaimPredicate.Always, SiblingReorderMode.ReadingOrder));
        var tagInRefine = new Rule("bad-tag", RemediationActions.Tag("P"), stage: Stage.Refine);
        var tableInClassify = new Rule("bad-table-stage", RemediationActions.Table(10, 20));
        var invalidColumns = new Rule("bad-table", RemediationActions.Table(10, 5));
        var invalidHeaderRows = new Rule("bad-table-header", RemediationActions.TableWithHeaderRows(-1, 10, 20), stage: Stage.Group);
        var invalidArtifact = new Rule("bad-artifact", RemediationActions.Artifact((ArtifactSubtype)999));
        var invalidLink = new Rule(
            "bad-link",
            RemediationActions.Link(ClaimPredicates.FromRule("source"), ClaimPredicates.FromRule("target"), ""),
            stage: Stage.Refine);

        Assert.Contains(groupInClassify.ValidateShape(), x => x.Contains("Group action"));
        Assert.Contains(reorderInClassify.ValidateShape(), x => x.Contains("ReorderSiblings"));
        Assert.Contains(tagInRefine.ValidateShape(), x => x.Contains("Tag actions"));
        Assert.Contains(tableInClassify.ValidateShape(), x => x.Contains("only valid in the Group stage"));
        Assert.Contains(invalidColumns.ValidateShape(), x => x.Contains("columns"));
        Assert.Contains(invalidHeaderRows.ValidateShape(), x => x.Contains("header row"));
        Assert.Contains(invalidArtifact.ValidateShape(), x => x.Contains("Artifact subtype"));
        Assert.Contains(invalidLink.ValidateShape(), x => x.Contains("accessible description"));

        var validGroup = new Rule(
            "group",
            RemediationActions.Group("L", ClaimPredicate.Always),
            stage: Stage.Group);
        var validReorder = new Rule(
            "reorder",
            RemediationActions.ReorderSiblings(ClaimPredicates.ClaimIs("P"), SiblingReorderMode.GeometryTopToBottom),
            stage: Stage.Refine);
        var validLink = new Rule(
            "link",
            RemediationActions.Link(ClaimPredicates.FromRule("source"), ClaimPredicates.FromRule("target"), "Jump"),
            stage: Stage.Refine);

        Assert.Empty(validGroup.ValidateShape());
        Assert.Empty(validReorder.ValidateShape());
        Assert.Empty(validLink.ValidateShape());
    }

    [Fact]
    public void CustomAction_IsExplicitlyOutsideFullDeclarativeValidation()
    {
        var action = Assert.IsType<CustomRemediationAction>(
            RemediationActions.Custom(
                ctx => new CustomRemediationOutcome(ctx.Candidates),
                "custom selector"));

        Assert.False(action.ParticipatesInFullPreflightValidation);
        Assert.Contains("custom selector", action.DebugString);
    }

    [Fact]
    public void SessionValidate_CatchesDuplicateIdsInvalidRegexAndLaterStageAnchors()
    {
        using var doc = PdfDocument.Create();
        using var session = doc.BeginRemediation();

        var duplicates = session.Validate(
            new RuleSet("a", new Rule("same", RemediationActions.Tag("P"))),
            new RuleSet("b", new Rule("same", RemediationActions.Tag("P"))));
        Assert.False(duplicates.IsValid);
        Assert.Contains(duplicates.Errors, x => x.Contains("duplicated"));

        var invalidRegex = session.Validate(new Rule(
            "bad-regex",
            RemediationActions.Tag("P"),
            Predicates.Text.Matches("[")));
        Assert.False(invalidRegex.IsValid);
        Assert.Contains(invalidRegex.Errors, x => x.Contains("invalid regex"));

        var laterAnchor = session.Validate(
            new Rule(
                "anchored",
                RemediationActions.Tag("P"),
                Predicates.Geo.Intersects(LayoutCoord.Anchor("late"))),
            new Rule(
                "late",
                RemediationActions.Lang(ClaimPredicate.Always, "fr-CA"),
                stage: Stage.Refine));
        Assert.False(laterAnchor.IsValid);
        Assert.Contains(laterAnchor.Errors, x => x.Contains("later-stage"));
    }

    [Fact]
    public void SessionValidate_CatchesUnknownRuleReferences()
    {
        using var doc = PdfDocument.Create();
        using var session = doc.BeginRemediation();

        var unknownAnchor = session.Validate(new Rule(
            "anchored",
            RemediationActions.Tag("P"),
            Predicates.Geo.Intersects(LayoutCoord.Anchor("missing"))));
        Assert.False(unknownAnchor.IsValid);
        Assert.Contains(unknownAnchor.Errors, x => x.Contains("unknown rule 'missing'"));

        var unknownRelational = session.Validate(new Rule(
            "relational",
            RemediationActions.Tag("P"),
            Predicates.Relational.After("missing")));
        Assert.False(unknownRelational.IsValid);
        Assert.Contains(unknownRelational.Errors, x => x.Contains("unknown rule 'missing'"));

        var unknownGroup = session.Validate(new Rule(
            "group",
            RemediationActions.Group("L", ClaimPredicates.FromRule("missing")),
            stage: Stage.Group));
        Assert.False(unknownGroup.IsValid);
        Assert.Contains(unknownGroup.Errors, x => x.Contains("unknown rule 'missing'"));
    }

    [Fact]
    public void SessionValidate_ReportsCustomActionsAsPartiallyValidated()
    {
        using var doc = PdfDocument.Create();
        using var session = doc.BeginRemediation();

        var report = session.Validate(new Rule(
            "custom",
            RemediationActions.Custom(
                ctx => new CustomRemediationOutcome(ctx.Candidates),
                "custom")));

        Assert.True(report.IsValid);
        Assert.Contains(report.Warnings, x => x.Contains("partially"));
    }

    private static RemediationCandidate Candidate(
        string text,
        PdfRect<double> bounds,
        double fontSize,
        int sequenceIndex = 0)
    {
        return new RemediationCandidate(
            Granularity.Paragraph,
            text,
            bounds,
            bounds,
            Array.Empty<StructuredCharacter>(),
            Array.Empty<StructuredSourceRef>(),
            sequenceIndex,
            fontSize,
            "Helvetica-BoldOblique",
            700,
            true,
            true);
    }
}
