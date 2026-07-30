using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PdfLexer.DOM;
using PdfLexer.Fonts;
using PdfLexer.Remediation;
using PdfLexer.Writing;
using Xunit;

namespace PdfLexer.Tests;

public class RemediationStructuralTemplateTests
{
    [Fact]
    public void JsonV1_ParsesTemplateSlotAndDefaultOccurrence()
    {
        const string json = """
        {
          "schema": "pdflexer.remediation.ruleset.v1",
          "id": "invoice",
          "template": {
            "tag": "Document",
            "children": [
              { "tag": "P", "id": "lines", "occurrence": "oneOrMore",
                "pages": { "kind": "every" }, "spansPages": false }
            ]
          },
          "rules": [{
            "id": "line", "slot": "lines",
            "candidates": { "kind": "text", "granularity": "paragraph" },
            "action": { "kind": "tag", "tag": "P" }
          }]
        }
        """;

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var job = SerializedRemediationRules.Load(stream);
        var child = Assert.Single(job.RuleSet.StructuralTemplate!.Document.Children);

        Assert.Equal(RemediationStructuralOccurrence.OneOrMore, child.Occurrence);
        Assert.Equal("lines", Assert.Single(job.RuleSet.Rules).Slot);
        Assert.False(child.SpansPages);
        Assert.IsType<EveryPageSelector>(child.Pages);
    }

    [Fact]
    public void DeclarationValidation_RejectsDuplicateIds()
    {
        var template = new RemediationStructuralTemplate(new[]
        {
            new RemediationStructuralTemplateNode("P", id: "body"),
            new RemediationStructuralTemplateNode("P", id: "body")
        });
        var report = SerializedRemediationRules.ValidateDeclarations(
            new RuleSet("set", Array.Empty<Rule>(), structuralTemplate: template));

        Assert.Contains(report.Errors, x => x.Contains("duplicated", StringComparison.Ordinal));
    }

    [Fact]
    public void DeclarationValidation_AllowsManyRulesForRepeatingSlot()
    {
        var template = new RemediationStructuralTemplate(new[]
        {
            new RemediationStructuralTemplateNode(
                "P", id: "body", occurrence: RemediationStructuralOccurrence.ZeroOrMore)
        });
        var rules = new[]
        {
            StructuralRule("one", "body"),
            StructuralRule("two", "body")
        };

        Assert.True(SerializedRemediationRules.ValidateDeclarations(
            new RuleSet("set", rules, structuralTemplate: template)).IsValid);
    }

    [Fact]
    public void DeclarationValidation_RejectsCardinalityOnSlot()
    {
        var template = new RemediationStructuralTemplate(new[]
        {
            new RemediationStructuralTemplateNode("P", id: "body")
        });
        var rule = StructuralRule("one", "body") with { Cardinality = RuleCardinality.Exactly(1) };

        var report = SerializedRemediationRules.ValidateDeclarations(
            new RuleSet("set", new[] { rule }, structuralTemplate: template));

        Assert.Contains(report.Errors, x => x.Contains("RuleCardinality", StringComparison.Ordinal));
    }

    [Fact]
    public void BoundSlot_IsNotFilledByUnboundSameTagNode()
    {
        var template = Template(new RemediationStructuralTemplateNode("P", id: "salutation"));
        var differences = RemediationStructuralTemplateMatcher.Match(
            "set", template, Tree(Node("P")), 1, new HashSet<string> { "salutation" });

        Assert.Contains(differences, x =>
            x.Kind == RemediationTemplateDifferenceKind.SlotUnfilled &&
            x.ExpectedPath == "Document/salutation");
    }

    [Fact]
    public void DryRun_ReportsUnfilledBoundSlot_WhenUnboundRuleProducesSameTag()
    {
        using var document = PdfDocument.Create();
        var page = document.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
        {
            writer.Font(Standard14Font.GetHelvetica(), 12).Text("Other").EndText();
        }

        var template = Template(new RemediationStructuralTemplateNode("P", id: "salutation"));
        var rules = new[]
        {
            new Rule(
                "salutation-rule", RemediationActions.Tag("P"),
                Predicates.Text.Equals("Hello"), CandidateSelector.Text(Granularity.Word),
                slot: "salutation"),
            new Rule(
                "other-rule", RemediationActions.Tag("P"),
                Predicates.Text.Equals("Other"), CandidateSelector.Text(Granularity.Word))
        };
        using var session = document.BeginRemediation().Use(
            new RuleSet("set", rules, structuralTemplate: template));

        var report = session.DryRun();

        Assert.Contains(report.TemplateDifferences, x =>
            x.Kind == RemediationTemplateDifferenceKind.SlotUnfilled &&
            x.SlotId == "salutation");
    }

    [Fact]
    public void DeclarationValidation_RejectsAmbiguousUnboundSameTagSlots_WithDistinctPaths()
    {
        var template = Template(
            new RemediationStructuralTemplateNode(
                "P", id: "alpha", occurrence: RemediationStructuralOccurrence.Optional),
            new RemediationStructuralTemplateNode(
                "P", id: "beta", occurrence: RemediationStructuralOccurrence.Optional));

        var report = SerializedRemediationRules.ValidateDeclarations(
            new RuleSet("set", Array.Empty<Rule>(), structuralTemplate: template));

        var error = Assert.Single(report.Errors, x => x.Contains("ambiguous", StringComparison.Ordinal));
        Assert.Contains("Document/alpha", error);
        Assert.Contains("Document/beta", error);
    }

    [Fact]
    public void AnonymousSameTagParticles_HaveDistinctPositionalPaths()
    {
        var template = Template(
            new RemediationStructuralTemplateNode("P"),
            new RemediationStructuralTemplateNode("P"));

        var differences = RemediationStructuralTemplateMatcher.Match("set", template, Tree(), 1);

        Assert.Equal(
            new[] { "Document/P[1]", "Document/P[2]" },
            differences.Select(x => x.ExpectedPath).ToArray());
    }

    [Fact]
    public void Transposition_ReportsWrongOrderWithoutFalseSlotUnfilled()
    {
        var template = Template(
            new RemediationStructuralTemplateNode("H1", id: "head"),
            new RemediationStructuralTemplateNode("P", id: "body"));
        var tree = Tree(Node("P", "body"), Node("H1", "head"));

        var differences = RemediationStructuralTemplateMatcher.Match(
            "set", template, tree, 1, new HashSet<string> { "head", "body" });

        var difference = Assert.Single(differences);
        Assert.Equal(RemediationTemplateDifferenceKind.WrongOrder, difference.Kind);
        Assert.Equal("Document/head", difference.ExpectedPath);
        Assert.Equal("Document/head[2]", difference.ActualPath);
    }

    [Fact]
    public void OptionalAndRepeatingParticles_MatchAbsentAndManyInstances()
    {
        var template = Template(
            new RemediationStructuralTemplateNode(
                "H1", id: "heading", occurrence: RemediationStructuralOccurrence.Optional),
            new RemediationStructuralTemplateNode(
                "P", id: "body", occurrence: RemediationStructuralOccurrence.OneOrMore));
        var tree = Tree(Node("P", "body"), Node("P", "body"), Node("P", "body"));

        var differences = RemediationStructuralTemplateMatcher.Match(
            "set", template, tree, 1, new HashSet<string> { "heading", "body" });

        Assert.Empty(differences);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void TablePlanAndMaterializationMatchClosedTemplate(bool flattenCells)
    {
        using var document = PdfDocument.Create();
        var page = document.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
        {
            writer.Font(Standard14Font.GetHelvetica(), 12)
                .TextMove(50, 700).Text("Item")
                .TextMove(220, 700).Text("Amount")
                .TextMove(50, 680).Text("Widget")
                .TextMove(220, 680).Text("$10")
                .EndText();
        }

        RemediationStructuralTemplateNode Cell(string tag, string id) =>
            new(
                tag,
                flattenCells
                    ? Array.Empty<RemediationStructuralTemplateNode>()
                    : new[] { new RemediationStructuralTemplateNode("Span", id: $"{id}-content") },
                id);
        var template = Template(
            new RemediationStructuralTemplateNode(
                "Table",
                new[]
                {
                    new RemediationStructuralTemplateNode(
                        "TR",
                        new[] { Cell("TH", "item-header"), Cell("TH", "amount-header") },
                        "header-row"),
                    new RemediationStructuralTemplateNode(
                        "TR",
                        new[] { Cell("TD", "item-value"), Cell("TD", "amount-value") },
                        "body-row")
                },
                "table"));
        var over = ClaimPredicates.FromRule("cells");
        var tableAction = flattenCells
            ? RemediationActions.TableOverFlattenedCells(over, 1, 40, 200, 400)
            : RemediationActions.TableOver(over, 1, 40, 200, 400);
        var rules = new[]
        {
            new Rule(
                "cells",
                RemediationActions.Tag("Span"),
                candidates: CandidateSelector.Text(Granularity.Word)),
            new Rule("table", tableAction, stage: Stage.Group)
        };
        using var session = document.BeginRemediation(new RemediationSessionConfiguration
        {
            Language = "en-US",
            Title = "Table template parity",
            StrictConformance = false
        }).Use(new RuleSet("table-template", rules, structuralTemplate: template));

        var dryRun = session.DryRun();
        var report = session.Commit();

        Assert.Empty(dryRun.TemplateDifferences);
        Assert.Empty(report.TemplateDifferences);
        var plannedTable = Assert.Single(dryRun.PlannedSemanticTree.Roots);
        Assert.Equal("Table", plannedTable.Tag);
        Assert.Equal(new[] { "TR", "TR" }, plannedTable.Children.Select(x => x.Tag).ToArray());
        Assert.All(plannedTable.Children, row => Assert.Equal(2, row.Children.Count));
    }

    private static Rule StructuralRule(string id, string slot) =>
        new(id, RemediationActions.Tag("P"), candidates: CandidateSelector.Text(Granularity.Paragraph), slot: slot);

    private static RemediationStructuralTemplate Template(
        params RemediationStructuralTemplateNode[] children) => new(children);

    private static RemediationSemanticTree Tree(params RemediationSemanticNode[] nodes) => new(nodes);

    private static RemediationSemanticNode Node(string tag, string? slot = null) =>
        new(default, tag, "set", slot ?? "unbound", Array.Empty<string>(),
            Array.Empty<PdfLexer.Content.StructuredSourceRef>(), new[] { 0 }, slot,
            Array.Empty<RemediationSemanticNode>());
}
