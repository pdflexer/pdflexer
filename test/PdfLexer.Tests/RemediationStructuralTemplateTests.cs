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
    public void DeclarationValidation_AllowsCardinalityAlongsideSlot()
    {
        var template = new RemediationStructuralTemplate(new[]
        {
            new RemediationStructuralTemplateNode("P", id: "body")
        });
        var rule = StructuralRule("one", "body") with { Cardinality = RuleCardinality.Exactly(1) };

        var report = SerializedRemediationRules.ValidateDeclarations(
            new RuleSet("set", new[] { rule }, structuralTemplate: template));

        Assert.True(report.IsValid, string.Join(Environment.NewLine, report.Errors));
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

    [Fact]
    public void JsonPrescriptiveTemplate_ParsesBindAndFromSlot()
    {
        const string json = """
        {
          "schema": "pdflexer.remediation.ruleset.v1",
          "id": "invoice",
          "template": {
            "mode": "prescriptive",
            "tag": "Document",
            "children": [
              { "tag": "Sect", "id": "section", "children": [
                { "tag": "H1", "id": "title" },
                { "tag": "P", "id": "body" }
              ] }
            ]
          },
          "rules": [
            { "id": "title", "slot": "title", "candidates": { "kind": "text", "granularity": "word" }, "action": { "kind": "bind" } },
            { "id": "body", "slot": "body", "candidates": { "kind": "text", "granularity": "word" }, "action": { "kind": "bind" } },
            { "id": "section", "slot": "section", "stage": "group", "groupPass": 10,
              "action": { "kind": "bind", "over": { "kind": "fromSlot", "slot": "title" } } }
          ]
        }
        """;

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var job = SerializedRemediationRules.Load(stream);
        var template = job.RuleSet.StructuralTemplate!;
        Assert.Equal(RemediationStructuralTemplateMode.Prescriptive, template.Mode);
        Assert.IsType<BindTemplateSlotRemediationAction>(job.RuleSet.Rules[0].Action);
        var group = Assert.IsType<BindTemplateSlotRemediationAction>(job.RuleSet.Rules[2].Action);
        Assert.Equal("FromSlot(title)", group.Over!.DebugString);
        Assert.Equal(10, job.RuleSet.Rules[2].GroupPass);
    }

    [Fact]
    public void PrescriptiveTemplate_SynthesizesAncestorsAndHonorsDeclaredOrder()
    {
        using var document = PdfDocument.Create();
        var page = document.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
        {
            writer.Font(Standard14Font.GetHelvetica(), 12)
                .TextMove(50, 700).Text("Body")
                .TextMove(50, 720).Text("Title")
                .EndText();
        }

        var template = new RemediationStructuralTemplate(
            new[]
            {
                new RemediationStructuralTemplateNode(
                    "Sect",
                    new[]
                    {
                        new RemediationStructuralTemplateNode("H1", id: "title"),
                        new RemediationStructuralTemplateNode("P", id: "body")
                    },
                    id: "section")
            },
            RemediationStructuralTemplateMode.Prescriptive);
        var rules = new[]
        {
            new Rule("title", RemediationActions.Bind(), Predicates.Text.Equals("Title"), CandidateSelector.Text(Granularity.Word), slot: "title"),
            new Rule("body", RemediationActions.Bind(), Predicates.Text.Equals("Body"), CandidateSelector.Text(Granularity.Word), slot: "body")
        };
        using var session = document.BeginRemediation(new RemediationSessionConfiguration
        {
            Language = "en-US",
            Title = "Prescriptive template",
            StrictConformance = false
        }).Use(new RuleSet("prescriptive", rules, structuralTemplate: template));

        var dryRun = session.DryRun();
        Assert.Empty(dryRun.Diagnostics);
        var plannedSection = Assert.Single(dryRun.PlannedSemanticTree.Roots);
        Assert.Equal("Sect", plannedSection.Tag);
        Assert.Equal("section", plannedSection.SlotId);
        Assert.Equal(new[] { "H1", "P" }, plannedSection.Children.Select(x => x.Tag).ToArray());
        Assert.Equal(new[] { "title", "body" }, plannedSection.Children.Select(x => x.SlotId).ToArray());

        var report = session.Commit();
        Assert.Empty(report.Diagnostics);
        var section = Assert.Single(document.Structure.GetRoot().Children);
        Assert.Equal("Sect", section.Type);
        Assert.Equal(new[] { "H1", "P" }, section.Children.Select(x => x.Type).ToArray());
        Assert.Equal(
            new[]
            {
                "template:Document/section[1]/title[1]",
                "template:Document/section[1]/body[1]"
            },
            section.Children.Select(x => x.ID).ToArray());
    }

    [Fact]
    public void PrescriptiveTemplate_RefineCanTargetSynthesizedAncestor()
    {
        using var document = PdfDocument.Create();
        var page = document.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
        {
            writer.Font(Standard14Font.GetHelvetica(), 12).Text("Title").EndText();
        }

        var template = new RemediationStructuralTemplate(
            new[]
            {
                new RemediationStructuralTemplateNode(
                    "Sect",
                    new[] { new RemediationStructuralTemplateNode("H1", id: "title") },
                    id: "section")
            },
            RemediationStructuralTemplateMode.Prescriptive);
        var rules = new[]
        {
            new Rule("title", RemediationActions.Bind(), Predicates.Text.Equals("Title"), CandidateSelector.Text(Granularity.Word), slot: "title"),
            new Rule("section-language", RemediationActions.Lang(ClaimPredicates.FromSlot("section"), "en-US"), stage: Stage.Refine)
        };
        using var session = document.BeginRemediation(new RemediationSessionConfiguration
        {
            Language = "en-US",
            Title = "Prescriptive refine",
            StrictConformance = false
        }).Use(new RuleSet("prescriptive", rules, structuralTemplate: template));

        var dryRun = session.DryRun();
        Assert.Empty(dryRun.Diagnostics);
        Assert.Equal("section", Assert.Single(dryRun.PlannedSemanticTree.Roots).SlotId);
        var report = session.Commit();
        Assert.Empty(report.Diagnostics);
        Assert.Equal("en-US", Assert.Single(document.Structure.GetRoot().Children).Language);
    }

    [Fact]
    public void PrescriptiveCommitFailure_RollsBackPdfObjectGraph()
    {
        using var document = PdfDocument.Create();
        var page = document.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
        {
            writer.Font(Standard14Font.GetHelvetica(), 12).Text("Link").EndText();
        }

        var template = new RemediationStructuralTemplate(
            new[]
            {
                new RemediationStructuralTemplateNode(
                    "Sect",
                    new[] { new RemediationStructuralTemplateNode("Link", id: "link") },
                    id: "section")
            },
            RemediationStructuralTemplateMode.Prescriptive);
        var rules = new[]
        {
            new Rule("link", RemediationActions.Bind(), Predicates.Text.Equals("Link"), CandidateSelector.Text(Granularity.Word), slot: "link"),
            new Rule(
                "section",
                RemediationActions.BindOver(ClaimPredicates.FromSlot("link"), TemplateSlotContentMode.FlattenLeafClaims),
                stage: Stage.Group,
                groupPass: 10,
                slot: "section")
        };
        using var session = document.BeginRemediation(new RemediationSessionConfiguration
        {
            Language = "en-US",
            Title = "Rollback",
            StrictConformance = false
        }).Use(new RuleSet("rollback", rules, structuralTemplate: template));

        var beforePage = page.NativeObject.CloneShallow();
        var beforeCatalog = document.Catalog.CloneShallow();
        Assert.Throws<InvalidOperationException>(() => session.Commit());

        Assert.Equal(beforePage.Count, page.NativeObject.Count);
        foreach (var item in beforePage)
        {
            Assert.True(page.NativeObject.TryGetValue(item.Key, out var current));
            Assert.Same(item.Value, current);
        }
        Assert.Equal(beforeCatalog.Count, document.Catalog.Count);
        foreach (var item in beforeCatalog)
        {
            Assert.True(document.Catalog.TryGetValue(item.Key, out var current));
            Assert.Same(item.Value, current);
        }
    }

    [Fact]
    public void PrescriptiveTemplate_OrdersSeveralTopLevelSlotsByDeclaration()
    {
        using var document = PdfDocument.Create();
        var page = document.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
        {
            writer.Font(Standard14Font.GetHelvetica(), 12)
                .TextMove(50, 720).Text("Beta")
                .TextMove(0, -20).Text("Alpha")
                .EndText();
        }

        var template = new RemediationStructuralTemplate(new[]
        {
            new RemediationStructuralTemplateNode("H1", id: "alpha"),
            new RemediationStructuralTemplateNode("P", id: "beta")
        }, RemediationStructuralTemplateMode.Prescriptive);
        var rules = new[]
        {
            new Rule("alpha", RemediationActions.Bind(), Predicates.Text.Equals("Alpha"), CandidateSelector.Text(Granularity.Word), slot: "alpha"),
            new Rule("beta", RemediationActions.Bind(), Predicates.Text.Equals("Beta"), CandidateSelector.Text(Granularity.Word), slot: "beta")
        };
        using var session = document.BeginRemediation(new RemediationSessionConfiguration
        {
            Language = "en-US", Title = "Top level order", StrictConformance = false
        }).Use(new RuleSet("top-level", rules, structuralTemplate: template));

        var dryRun = session.DryRun();
        Assert.Empty(dryRun.TemplateDifferences);
        Assert.Equal(new[] { "alpha", "beta" }, dryRun.PlannedSemanticTree.Roots.Select(x => x.SlotId).ToArray());
        Assert.Equal(new[] { "alpha", "beta" }, dryRun.TemplateAssembly.Select(x => x.SlotId).ToArray());
        var repeatedDryRun = session.DryRun();
        Assert.Equal(
            dryRun.TemplateAssembly.Select(AssemblySnapshot),
            repeatedDryRun.TemplateAssembly.Select(AssemblySnapshot));

        var committed = session.Commit();
        Assert.Empty(committed.TemplateDifferences);
        Assert.Equal(new[] { "H1", "P" }, document.Structure.GetRoot().Children.Select(x => x.Type).ToArray());
        Assert.Equal(dryRun.TemplateAssembly.Select(x => x.Identity), committed.TemplateAssembly.Select(x => x.Identity));
    }

    [Fact]
    public void PrescriptiveBindOver_RepeatsCompositeWithStableNestedIdentities()
    {
        using var document = PdfDocument.Create();
        for (var pageIndex = 0; pageIndex < 2; pageIndex++)
        {
            var page = document.AddPage(PageSize.LETTER);
            using var writer = page.GetWriter();
            writer.Font(Standard14Font.GetHelvetica(), 12).Text($"Item {pageIndex + 1}").EndText();
        }

        var template = new RemediationStructuralTemplate(new[]
        {
            new RemediationStructuralTemplateNode(
                "Sect",
                new[] { new RemediationStructuralTemplateNode("P", id: "line") },
                id: "item",
                occurrence: RemediationStructuralOccurrence.OneOrMore)
        }, RemediationStructuralTemplateMode.Prescriptive);
        var rules = new[]
        {
            new Rule("line", RemediationActions.Bind(), candidates: CandidateSelector.Text(Granularity.Paragraph), slot: "line"),
            new Rule("item", RemediationActions.BindOver(ClaimPredicates.FromSlot("line")), stage: Stage.Group, groupPass: 10, slot: "item")
        };
        using var session = document.BeginRemediation(new RemediationSessionConfiguration
        {
            Language = "en-US", Title = "Repeated items", StrictConformance = false
        }).Use(new RuleSet("repeated", rules, structuralTemplate: template));

        var dryRun = session.DryRun();
        Assert.Empty(dryRun.Diagnostics);
        Assert.Empty(dryRun.TemplateDifferences);
        Assert.Equal(2, dryRun.PlannedSemanticTree.Roots.Count);
        Assert.Equal(
            new[] { "template:Document/item[1]", "template:Document/item[2]" },
            dryRun.PlannedSemanticTree.Roots.Select(x => x.TemplateIdentity).ToArray());
        Assert.All(dryRun.PlannedSemanticTree.Roots, item => Assert.Equal("line", Assert.Single(item.Children).SlotId));

        var report = session.Commit();
        Assert.Empty(report.Diagnostics);
        var items = document.Structure.GetRoot().Children;
        Assert.Equal(2, items.Count);
        Assert.Equal(dryRun.TemplateAssembly.Select(x => x.Identity), report.TemplateAssembly.Select(x => x.Identity));
        Assert.Equal(items.Select(x => x.ID).Distinct().Count(), items.Count);
        Assert.Equal(
            new[]
            {
                "template:Document/item[1]/line[1]",
                "template:Document/item[2]/line[1]"
            },
            items.Select(x => Assert.Single(x.Children).ID).ToArray());
    }

    [Fact]
    public void PrescriptiveTable_KeepsTwentyRowsStableAndTreatsInteriorAsOpaque()
    {
        using var document = PdfDocument.Create();
        var page = document.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
        {
            for (var row = 0; row < 20; row++)
            {
                var y = 740 - row * 20;
                writer.Font(Standard14Font.GetHelvetica(), 10).TextMove(50, y).Text($"A{row:D2}").EndText();
                writer.Font(Standard14Font.GetHelvetica(), 10).TextMove(220, y).Text($"B{row:D2}").EndText();
            }
        }

        var template = new RemediationStructuralTemplate(new[]
        {
            new RemediationStructuralTemplateNode(
                "Table",
                new[]
                {
                    new RemediationStructuralTemplateNode(
                        "TR",
                        new[]
                        {
                            new RemediationStructuralTemplateNode(
                                "TD", id: "cell", occurrence: RemediationStructuralOccurrence.ZeroOrMore)
                        },
                        id: "row",
                        occurrence: RemediationStructuralOccurrence.ZeroOrMore)
                },
                id: "table")
        }, RemediationStructuralTemplateMode.Prescriptive);
        var rules = new[]
        {
            new Rule("cells", RemediationActions.Tag("Span"), candidates: CandidateSelector.Text(Granularity.Word), slot: "cell"),
            new Rule("table", RemediationActions.TableOverFlattenedCells(ClaimPredicates.FromSlot("cell"), 40d, 150d, 300d), stage: Stage.Group, slot: "table")
        };
        using var session = document.BeginRemediation(new RemediationSessionConfiguration
        {
            Language = "en-US", Title = "Twenty row table", StrictConformance = false
        }).Use(new RuleSet("table", rules, structuralTemplate: template));

        var dryRun = session.DryRun();
        Assert.Empty(dryRun.Diagnostics);
        Assert.Empty(dryRun.TemplateDifferences);
        var plannedTable = Assert.Single(dryRun.PlannedSemanticTree.Roots);
        Assert.True(plannedTable.OpaqueTemplateInterior);
        Assert.Equal(20, plannedTable.Children.Count);
        Assert.All(plannedTable.Children, row => Assert.Null(row.SlotId));

        var report = session.Commit();
        Assert.Empty(report.Diagnostics);
        var table = Assert.Single(document.Structure.GetRoot().Children);
        Assert.Equal(20, table.Children.Count);
        Assert.Equal(
            Enumerable.Range(0, 40),
            table.Children.SelectMany(x => x.Children).Select(x => x.ContentItems.Single().MCID));
    }

    [Fact]
    public void PrescriptiveValidation_CoversCompositeBindingShapesAndStructuralLinks()
    {
        RemediationStructuralTemplate TemplateFor(RemediationStructuralOccurrence occurrence = RemediationStructuralOccurrence.ExactlyOne) =>
            new(new[]
            {
                new RemediationStructuralTemplateNode(
                    "Sect",
                    new[] { new RemediationStructuralTemplateNode("P", id: "leaf") },
                    id: "section",
                    occurrence: occurrence)
            }, RemediationStructuralTemplateMode.Prescriptive);

        var directComposite = SerializedRemediationRules.ValidateDeclarations(new RuleSet(
            "direct", new[] { new Rule("section", RemediationActions.Bind(), candidates: CandidateSelector.Text(Granularity.Word), slot: "section") },
            structuralTemplate: TemplateFor()));
        Assert.Contains(directComposite.Errors, x => x.Contains("use BindOver", StringComparison.Ordinal));

        var bindOverLeaf = SerializedRemediationRules.ValidateDeclarations(new RuleSet(
            "leaf", new[]
            {
                new Rule("leaf", RemediationActions.BindOver(ClaimPredicates.FromSlot("leaf")), stage: Stage.Group, slot: "leaf")
            }, structuralTemplate: TemplateFor()));
        Assert.Contains(bindOverLeaf.Errors, x => x.Contains("leaf slot", StringComparison.Ordinal));

        var missingRepeatingProducer = SerializedRemediationRules.ValidateDeclarations(new RuleSet(
            "repeat", new[] { new Rule("leaf", RemediationActions.Bind(), candidates: CandidateSelector.Text(Granularity.Word), slot: "leaf") },
            structuralTemplate: TemplateFor(RemediationStructuralOccurrence.OneOrMore)));
        Assert.Contains(missingRepeatingProducer.Errors, x => x.Contains("exactly one BindOver", StringComparison.Ordinal));

        var structuralLink = SerializedRemediationRules.ValidateDeclarations(new RuleSet(
            "link", new[]
            {
                new Rule(
                    "link", RemediationActions.Link(ClaimPredicates.FromSlot("leaf"), ClaimPredicates.FromSlot("leaf"), "Link"),
                    stage: Stage.Refine, slot: "section")
            }, structuralTemplate: TemplateFor()));
        Assert.Contains(structuralLink.Errors, x => x.Contains("cannot create a structural Link", StringComparison.Ordinal));
    }

    [Fact]
    public void PrescriptiveAssembly_AttachesUnconsumedSiblingToExistingBoundParent()
    {
        using var document = PdfDocument.Create();
        var page = document.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
        {
            writer.Font(Standard14Font.GetHelvetica(), 12)
                .TextMove(50, 720).Text("Title")
                .TextMove(50, 690).Text("Body")
                .EndText();
        }
        var template = new RemediationStructuralTemplate(new[]
        {
            new RemediationStructuralTemplateNode("Sect", new[]
            {
                new RemediationStructuralTemplateNode("H1", id: "title"),
                new RemediationStructuralTemplateNode("P", id: "body")
            }, id: "section")
        }, RemediationStructuralTemplateMode.Prescriptive);
        var rules = new[]
        {
            new Rule("title", RemediationActions.Bind(), Predicates.Text.Equals("Title"), CandidateSelector.Text(Granularity.Word), slot: "title"),
            new Rule("body", RemediationActions.Bind(), Predicates.Text.Equals("Body"), CandidateSelector.Text(Granularity.Word), slot: "body"),
            new Rule("section", RemediationActions.BindOver(ClaimPredicates.FromSlot("title")), stage: Stage.Group, slot: "section")
        };
        using var session = document.BeginRemediation(new RemediationSessionConfiguration
        {
            Language = "en-US", Title = "Parent resolution", StrictConformance = false
        }).Use(new RuleSet("parent-resolution", rules, structuralTemplate: template));

        var dryRun = session.DryRun();
        Assert.Empty(dryRun.TemplateDifferences);
        Assert.Equal(new[] { "title", "body" }, Assert.Single(dryRun.PlannedSemanticTree.Roots).Children.Select(x => x.SlotId));
        session.Commit();
        var section = Assert.Single(document.Structure.GetRoot().Children);
        Assert.Equal(new[] { "H1", "P" }, section.Children.Select(x => x.Type));
        Assert.Equal(3, document.Structure.GetStructureRoot().IdMap.Count);
    }

    [Fact]
    public void PrescriptiveAssembly_ComposesNestedRepeatedBindOverPasses()
    {
        using var document = PdfDocument.Create();
        for (var pageIndex = 0; pageIndex < 2; pageIndex++)
        {
            var page = document.AddPage(PageSize.LETTER);
            using var writer = page.GetWriter();
            writer.Font(Standard14Font.GetHelvetica(), 12).Text($"Line {pageIndex + 1}").EndText();
        }
        var template = new RemediationStructuralTemplate(new[]
        {
            new RemediationStructuralTemplateNode("Part", new[]
            {
                new RemediationStructuralTemplateNode("Sect", new[]
                {
                    new RemediationStructuralTemplateNode("P", id: "line")
                }, id: "item")
            }, id: "batch", occurrence: RemediationStructuralOccurrence.OneOrMore)
        }, RemediationStructuralTemplateMode.Prescriptive);
        var rules = new[]
        {
            new Rule("line", RemediationActions.Bind(), candidates: CandidateSelector.Text(Granularity.Paragraph), slot: "line"),
            new Rule("item", RemediationActions.BindOver(ClaimPredicates.FromSlot("line")), stage: Stage.Group, groupPass: 10, slot: "item"),
            new Rule("batch", RemediationActions.BindOver(ClaimPredicates.FromSlot("item")), stage: Stage.Group, groupPass: 20, slot: "batch")
        };
        using var session = document.BeginRemediation(new RemediationSessionConfiguration
        {
            Language = "en-US", Title = "Nested passes", StrictConformance = false
        }).Use(new RuleSet("nested", rules, structuralTemplate: template));

        var dryRun = session.DryRun();
        Assert.Empty(dryRun.Diagnostics);
        Assert.Empty(dryRun.TemplateDifferences);
        Assert.Equal(2, dryRun.PlannedSemanticTree.Roots.Count);
        Assert.All(dryRun.PlannedSemanticTree.Roots, batch =>
            Assert.Equal("line", Assert.Single(Assert.Single(batch.Children).Children).SlotId));
        var report = session.Commit();
        Assert.Equal(dryRun.TemplateAssembly.Select(x => x.Identity), report.TemplateAssembly.Select(x => x.Identity));
        Assert.Equal(6, document.Structure.GetStructureRoot().IdMap.Count);
    }

    [Fact]
    public void PrescriptiveAnnotationAdoption_BindsCompatibleSlotAndRefinesFromSlot()
    {
        using var document = PdfDocument.Create();
        var page = document.AddPage(PageSize.LETTER);
        var annotation = new PdfDictionary
        {
            [PdfName.Subtype] = (PdfName)"Stamp",
            [PdfName.Rect] = new PdfArray { 20, 20, 80, 60 },
            [PdfName.Contents] = PdfString.CreateTextString("Approved")
        };
        page.NativeObject[PdfName.Annots] = new PdfArray { annotation };
        var template = new RemediationStructuralTemplate(new[]
        {
            new RemediationStructuralTemplateNode("Annot", id: "approval")
        }, RemediationStructuralTemplateMode.Prescriptive);
        var rules = new[]
        {
            new Rule("approval", RemediationActions.AdoptAnnotation(), Predicates.Annotation.Subtype("Stamp"), CandidateSelector.Annotations(), slot: "approval"),
            new Rule("approval-language", RemediationActions.Lang(ClaimPredicates.FromSlot("approval"), "en-US"), stage: Stage.Refine)
        };
        using var session = document.BeginRemediation(new RemediationSessionConfiguration
        {
            Language = "en-US", Title = "Annotation", StrictConformance = false
        }).Use(new RuleSet("annotation", rules, structuralTemplate: template));

        var dryRun = session.DryRun();
        Assert.Empty(dryRun.Diagnostics);
        Assert.Equal("approval", Assert.Single(dryRun.PlannedSemanticTree.Roots).SlotId);
        var report = session.Commit();
        Assert.Empty(report.Diagnostics);
        var node = Assert.Single(document.Structure.GetRoot().Children);
        Assert.Equal("Annot", node.Type);
        Assert.Equal("en-US", node.Language);
        Assert.Single(node.ObjectReferences);
        Assert.True(annotation.ContainsKey(PdfName.StructParent));
    }

    [Fact]
    public void PrescriptiveUnaccountedContent_IsNotSuppressible()
    {
        using var document = PdfDocument.Create();
        var page = document.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
        {
            writer.Font(Standard14Font.GetHelvetica(), 12).Text("Unbound").EndText();
        }
        var template = new RemediationStructuralTemplate(new[]
        {
            new RemediationStructuralTemplateNode(
                "P", id: "optional", occurrence: RemediationStructuralOccurrence.Optional)
        }, RemediationStructuralTemplateMode.Prescriptive);
        using var session = document.BeginRemediation(new RemediationSessionConfiguration { StrictConformance = false })
            .Use(new RuleSet("leftover", Array.Empty<Rule>(), structuralTemplate: template))
            .Suppress(DiagnosticCode.PrescriptiveUnaccountedContent, "*", "attempted waiver");

        var report = session.DryRun();
        Assert.Contains(report.Diagnostics, x => x.StartsWith(nameof(DiagnosticCode.PrescriptiveUnaccountedContent), StringComparison.Ordinal));
        Assert.DoesNotContain(report.Diagnostics, x => x.StartsWith("[SUPPRESSED]", StringComparison.Ordinal));
    }

    private static string AssemblySnapshot(RemediationTemplateAssemblyItem item) =>
        $"{item.TemplatePath}|{item.OccurrenceIndex}|{item.Identity}|{item.ParentIdentity}|" +
        $"{item.ProducingRuleId}|{item.ProducingClaimReference}|" +
        string.Join(",", item.ConsumedClaimReferences);

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
