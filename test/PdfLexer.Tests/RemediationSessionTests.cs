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

public class RemediationSessionTests
{
    [Fact]
    public void BeginRemediation_IsSideEffectFreeUntilCommit()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);

        using var session = doc.BeginRemediation(new RemediationSessionConfiguration
        {
            Language = "en-US",
            Title = "Remediated"
        });

        Assert.False(doc.Catalog.ContainsKey(PdfName.MarkInfo));
        Assert.False(doc.Catalog.ContainsKey(PdfName.Metadata));
        Assert.False(page.NativeObject.ContainsKey(PdfName.Tabs));

        var report = session.DryRun();

        Assert.False(report.Committed);
        Assert.False(report.AppliedAccessibilitySetup);
        Assert.False(doc.Catalog.ContainsKey(PdfName.MarkInfo));

        var committed = doc.BeginRemediation(new RemediationSessionConfiguration
        {
            Language = "en-US",
            Title = "Committed"
        });
        var commitReport = committed.Commit();

        Assert.True(commitReport.Committed);
        Assert.True(commitReport.AppliedAccessibilitySetup);
        Assert.True(doc.Catalog.ContainsKey(PdfName.MarkInfo));
        Assert.True(doc.Catalog.ContainsKey(PdfName.Metadata));
        Assert.Equal(PdfName.S, page.NativeObject.Get<PdfName>(PdfName.Tabs));
    }

    [Fact]
    public void BeginRemediation_RejectsDocumentsWithExistingStructTreeRoot()
    {
        using var doc = PdfDocument.Create();
        doc.Catalog[PdfName.StructTreeRoot] = new PdfDictionary();

        Assert.Throws<PdfAccessibilitySetupException>(() => doc.BeginRemediation());
    }

    [Fact]
    public void RemediationSession_AllocatesMcidsPerPage()
    {
        using var doc = PdfDocument.Create();
        var first = doc.AddPage(PageSize.LETTER);
        var second = doc.AddPage(PageSize.LETTER);
        using var session = doc.BeginRemediation();

        Assert.Equal(0, session.AllocateMcid(first));
        Assert.Equal(1, session.AllocateMcid(first));
        Assert.Equal(0, session.AllocateMcid(second));
    }

    [Fact]
    public void RemediationSession_BindsContentAndAnnotationsToStructParents()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        var annotation = new PdfDictionary
        {
            [PdfName.Subtype] = PdfName.Link,
            [PdfName.Rect] = new PdfArray { 10, 10, 20, 20 }
        };
        page.AddAnnotation(annotation);

        using (var writer = page.GetWriter())
        {
            writer.Font(Standard14Font.GetHelvetica(), 12).Text("Hello").EndText();
        }

        var content = page.GetContentModel<double>();
        var text = Assert.IsType<TextContent<double>>(Assert.Single(content));
        content.Wrap(
            new[] { text },
            new MarkedContent("P")
            {
                InlineProps = new PdfDictionary
                {
                    [PdfName.MCID] = new PdfIntNumber(0)
                }
            });

        using (var writer = page.GetWriter(PageWriteMode.Replace))
        {
            writer.AddContent(content);
        }

        using var session = doc.BeginRemediation(new RemediationSessionConfiguration
        {
            Language = "en-US",
            Title = "Bindings",
            StrictConformance = false
        });
        var node = session.Structure.AddParagraph().GetNode();
        var mcid = session.AllocateMcid(page);

        session.BindMarkedContent(node, page, mcid);
        session.BindAnnotation(node, page, annotation);
        var report = session.Commit();

        Assert.True(report.Committed);
        Assert.Equal(0, node.ContentItems[0].MCID);
        Assert.True(page.NativeObject.ContainsKey(PdfName.StructParents));
        Assert.True(annotation.ContainsKey(PdfName.StructParent));
    }

    [Fact]
    public void RemediationSession_Commit_ThrowsWhenClaimedContentIsNotActuallyTagged()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);

        using (var writer = page.GetWriter())
        {
            writer.Font(Standard14Font.GetHelvetica(), 12).Text("Hello").EndText();
        }

        using var session = doc.BeginRemediation(new RemediationSessionConfiguration
        {
            Language = "en-US",
            Title = "Invalid"
        });
        var node = session.Structure.AddParagraph().GetNode();

        session.BindMarkedContent(node, page, session.AllocateMcid(page));

        var error = Assert.Throws<PdfAccessibilityConformanceException>(() => session.Commit());
        Assert.Contains("tagged", error.Message);
        Assert.False(doc.Catalog.ContainsKey(PdfName.MarkInfo));
        Assert.False(doc.Catalog.ContainsKey(PdfName.StructTreeRoot));
        Assert.False(page.NativeObject.ContainsKey(PdfName.StructParents));
    }

    [Fact]
    public void RuleDryRun_EvaluatesClassifyRulesWithoutMutatingDocument()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
        {
            writer.Font(Standard14Font.GetHelvetica(), 12).Text("Invoice Total").EndText();
        }

        using var session = doc.BeginRemediation();
        var report = session.DryRun(new Rule(
            "heading",
            RemediationActions.Tag("H1"),
            Predicates.Text.StartsWith("Invoice"),
            Granularity.Paragraph));

        Assert.False(report.Committed);
        var claim = Assert.Single(report.Claims);
        Assert.Equal("heading", claim.RuleId);
        Assert.Equal(ClaimStatus.Applied, claim.Status);
        Assert.DoesNotContain("BDC", page.DumpDecodedContents());
        Assert.False(doc.Catalog.ContainsKey(PdfName.MarkInfo));
    }

    [Fact]
    public void RuleDryRun_SkipsConflictingClaimsUnlessOverrideIsSet()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
        {
            writer.Font(Standard14Font.GetHelvetica(), 12).Text("Hello").EndText();
        }

        using var session = doc.BeginRemediation();
        var skipped = session.DryRun(
            new Rule("first", RemediationActions.Tag("P"), granularity: Granularity.Paragraph),
            new Rule("second", RemediationActions.Tag("H1"), granularity: Granularity.Paragraph));

        Assert.Single(skipped.Claims);
        Assert.Equal("first", skipped.Claims[0].RuleId);
        Assert.Single(skipped.SkippedClaims);
        Assert.Equal("second", skipped.SkippedClaims[0].RuleId);

        using var overrideSession = doc.BeginRemediation();
        var overridden = overrideSession.DryRun(
            new Rule("first", RemediationActions.Tag("P"), granularity: Granularity.Paragraph),
            new Rule("second", RemediationActions.Tag("H1"), granularity: Granularity.Paragraph, @override: true));

        var claim = Assert.Single(overridden.Claims);
        Assert.Equal("second", claim.RuleId);
        Assert.Contains(overridden.SkippedClaims, x => x.RuleId == "first" && x.Status == ClaimStatus.Overridden);
    }

    [Fact]
    public void RuleDryRun_EnforcesMinimumConfidence()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
        {
            writer.Font(Standard14Font.GetHelvetica(), 12).Text("Hello").EndText();
        }

        using var session = doc.BeginRemediation();
        var report = session.DryRun(new Rule(
            "low-confidence",
            RemediationActions.Tag("P"),
            RemediationPredicate.Always,
            Granularity.Paragraph,
            minConfidence: 1.0));

        Assert.Single(report.Claims);

        using var failingSession = doc.BeginRemediation();
        var failed = failingSession.DryRun(new Rule(
            "low-confidence",
            RemediationActions.Tag("P"),
            new LowConfidencePredicate(),
            Granularity.Paragraph,
            minConfidence: 0.8));

        Assert.Empty(failed.Claims);
        var skipped = Assert.Single(failed.SkippedClaims);
        Assert.Equal("low-confidence", skipped.RuleId);
    }

    [Fact]
    public void RuleCommit_ComposedPredicateTargetsExactWordAndReportsBinding()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
        {
            writer.Font(Standard14Font.GetHelvetica(), 12).Text("Invoice Total").EndText();
        }

        using var session = doc.BeginRemediation(new RemediationSessionConfiguration
        {
            Language = "en-US",
            Title = "Exact Word",
            StrictConformance = false,
            LeftoverPolicy = RemediationLeftoverPolicy.AutoArtifact
        });

        var report = session.Commit(new Rule(
            "total-word",
            RemediationActions.Tag("Span"),
            Predicates.Text.Equals("Total").And(Predicates.Font.Size(NumericOperator.GreaterThanOrEqual, 12)),
            Granularity.Word));

        var claim = Assert.Single(report.Claims);
        Assert.Equal("total-word", claim.RuleId);
        Assert.Equal(Granularity.Word, claim.Granularity);
        var binding = Assert.Single(claim.AppliedBindings);
        Assert.Equal("Span", binding.ProducedTag);
        Assert.Equal(new[] { 0 }, binding.Mcids);
        Assert.NotNull(binding.StructureNode);
        Assert.NotNull(binding.MarkedContentGroup);
        Assert.Equal("Invoice Total", new string(page.GetStructuredText().Characters.Select(x => x.Char).ToArray()));
        Assert.Contains("/Span <</MCID 0>> BDC", page.DumpDecodedContents());
    }

    [Fact]
    public void RuleCommit_AppliesParagraphTagAndStructureBinding()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
        {
            writer.Font(Standard14Font.GetHelvetica(), 12).Text("Hello").EndText();
        }

        using var session = doc.BeginRemediation(new RemediationSessionConfiguration
        {
            Language = "en-US",
            Title = "Rule Commit",
            StrictConformance = false
        });

        var report = session.Commit(new Rule(
            "paragraph",
            RemediationActions.Tag("P"),
            granularity: Granularity.Paragraph));

        Assert.True(report.Committed);
        Assert.Single(report.Claims);
        Assert.Contains("/P <</MCID 0>> BDC", page.DumpDecodedContents());
        Assert.True(page.NativeObject.ContainsKey(PdfName.StructParents));
        Assert.True(doc.Catalog.ContainsKey(PdfName.MarkInfo));
    }

    [Fact]
    public void DebugWrite_EmitsRuleIdAsStructureTitle()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
        {
            writer.Font(Standard14Font.GetHelvetica(), 12).Text("Hello").EndText();
        }

        using var session = doc.BeginRemediation(new RemediationSessionConfiguration
        {
            DebugWrite = true,
            StrictConformance = false
        });

        session.Commit(new Rule(
            "debug-p",
            RemediationActions.Tag("P"),
            granularity: Granularity.Paragraph));

        var root = doc.Structure.GetRoot();
        var p = Assert.Single(root.Children, x => x.Type == "P");
        Assert.Equal("debug-p", p.Title);
    }

    [Fact]
    public void Production_DoesNotEmitRuleIdAsStructureTitle()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
        {
            writer.Font(Standard14Font.GetHelvetica(), 12).Text("Hello").EndText();
        }

        using var session = doc.BeginRemediation(new RemediationSessionConfiguration
        {
            DebugWrite = false,
            StrictConformance = false
        });

        session.Commit(new Rule(
            "debug-p",
            RemediationActions.Tag("P"),
            granularity: Granularity.Paragraph));

        var root = doc.Structure.GetRoot();
        var p = Assert.Single(root.Children, x => x.Type == "P");
        Assert.Null(p.Title);
    }

    [Fact]
    public void Report_RoundTripsRuleIdAndSelectorDescription()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
        {
            writer.Font(Standard14Font.GetHelvetica(), 12).Text("Hello").EndText();
        }

        using var session = doc.BeginRemediation(new RemediationSessionConfiguration
        {
            StrictConformance = false,
            LeftoverPolicy = RemediationLeftoverPolicy.AutoArtifact
        });

        var rule = new Rule(
            "test-rule-123",
            RemediationActions.Tag("P"),
            Predicates.Text.Equals("Hello"),
            Granularity.Paragraph);

        var report = session.Commit(rule);
        
        var claim = Assert.Single(report.Outcomes);
        Assert.Equal("test-rule-123", claim.RuleId);
        Assert.Equal(rule.Predicate.DebugString, claim.SelectorDebugString);
    }

    [Fact]
    public void Report_ExplainReturnsOutcomesForLeaf()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
        {
            writer.Font(Standard14Font.GetHelvetica(), 12).Text("Hello").EndText();
        }

        using var session = doc.BeginRemediation(new RemediationSessionConfiguration
        {
            StrictConformance = false,
            LeftoverPolicy = RemediationLeftoverPolicy.AutoArtifact
        });

        // Two rules consider the text
        var rule1 = new Rule(
            "rule-1",
            RemediationActions.Tag("Span"),
            Predicates.Text.Equals("Hello"),
            Granularity.Word);

        var rule2 = new Rule(
            "rule-2",
            RemediationActions.Tag("P"),
            Predicates.Text.Equals("Hello"),
            Granularity.Paragraph);

        var report = session.DryRun(rule1, rule2);
        
        // Find the leaf
        var textPage = page.GetStructuredText();
        var word = Assert.Single(textPage.Words);
        
        var explained = report.Explain(word.SourceReferences[0]);
        Assert.Equal(2, explained.Count);
        Assert.Contains(explained, o => o.RuleId == "rule-1");
        Assert.Contains(explained, o => o.RuleId == "rule-2");
    }

    [Fact]
    public void RuleCommit_TableColumnsProducesRowsAndHeaderCells()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
        {
            writer.Font(Standard14Font.GetHelvetica(), 12)
                .TextMove(50, 700).Text("A1")
                .TextMove(220, 700).Text("B1")
                .TextMove(380, 700).Text("C1")
                .TextMove(50, 680).Text("A2")
                .TextMove(220, 680).Text("B2")
                .TextMove(380, 680).Text("C2")
                .EndText();
        }

        using (var dryRunSession = doc.BeginRemediation())
        {
            var inferred = dryRunSession.DryRun(new Rule(
                "table-inferred",
                RemediationActions.Table(),
                Predicates.Geo.Intersects(LayoutCoord.Absolute(new PdfRect<double>(0, 650, 520, 730))),
                Granularity.Word,
                stage: Stage.Group));

            Assert.Equal(0.9, Assert.Single(inferred.Claims).Confidence);
        }

        using var session = doc.BeginRemediation(new RemediationSessionConfiguration
        {
            Language = "en-US",
            Title = "Table",
            StrictConformance = false
        });

        var report = session.Commit(new Rule(
            "table",
            RemediationActions.TableWithHeaderRows(1, 40, 200, 340, 500),
            Predicates.Geo.Intersects(LayoutCoord.Absolute(new PdfRect<double>(0, 650, 520, 730))),
            Granularity.Word,
            stage: Stage.Group));

        Assert.True(report.Committed);
        var table = Assert.Single(doc.Structure.GetRoot().Children, x => x.Type == "Table");
        Assert.Equal(2, table.Children.Count);
        Assert.Equal(new[] { "TH", "TH", "TH" }, table.Children[0].Children.Select(x => x.Type).ToArray());
        Assert.All(table.Children[0].Children, cell => Assert.Equal(StructureScope.Column, cell.Scope));
        Assert.Equal(new[] { "TD", "TD", "TD" }, table.Children[1].Children.Select(x => x.Type).ToArray());
        Assert.All(table.Children, row =>
        {
            Assert.Equal("TR", row.Type);
            Assert.Equal(3, row.Children.Count);
        });
        Assert.Equal(6, table.Children.SelectMany(x => x.Children).Count(x => x.ContentItems.Count == 1));
        Assert.Contains("/TH <</MCID 0>> BDC", page.DumpDecodedContents());
    }

    [Fact]
    public void RuleCommit_TableOverPreclassifiedClaimsReusesExistingMcids()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
        {
            writer.Font(Standard14Font.GetHelvetica(), 12)
                .TextMove(50, 700).Text("A1")
                .TextMove(220, 700).Text("B1")
                .TextMove(380, 700).Text("C1")
                .TextMove(50, 680).Text("A2")
                .TextMove(220, 680).Text("B2")
                .TextMove(380, 680).Text("C2")
                .EndText();
        }

        using var session = doc.BeginRemediation(new RemediationSessionConfiguration
        {
            Language = "en-US",
            Title = "Claim Table",
            StrictConformance = false
        });

        var report = session.Commit(
            new Rule(
                "table-cells",
                RemediationActions.Tag("Span"),
                Predicates.Geo.Intersects(LayoutCoord.Absolute(new PdfRect<double>(0, 650, 800, 730))),
                Granularity.Word),
            new Rule(
                "table",
                RemediationActions.TableOver(ClaimPredicates.ClaimIs("Span")),
                granularity: Granularity.Word,
                stage: Stage.Group));

        Assert.True(report.Committed);
        Assert.Equal(6, report.Claims.Count(x => x.RuleId == "table-cells"));
        var tableClaim = Assert.Single(report.Claims, x => x.RuleId == "table");
        Assert.Equal(6, tableClaim.RelatedClaims.Count);
        Assert.Equal(new[] { 0, 1, 2, 3, 4, 5 }, tableClaim.AppliedBindings.SelectMany(x => x.Mcids).Distinct().OrderBy(x => x).ToArray());

        var table = Assert.Single(doc.Structure.GetRoot().Children, x => x.Type == "Table");
        Assert.Equal(2, table.Children.Count);
        Assert.All(table.Children, row =>
        {
            Assert.Equal("TR", row.Type);
            Assert.Equal(3, row.Children.Count);
            Assert.All(row.Children, cell =>
            {
                Assert.Equal("TD", cell.Type);
                Assert.Single(cell.Children);
                Assert.Equal("Span", cell.Children[0].Type);
            });
        });
        Assert.Equal(6, table.Children.SelectMany(x => x.Children).SelectMany(x => x.Children).Sum(x => x.ContentItems.Count));
        Assert.DoesNotContain("/TD <</MCID", page.DumpDecodedContents());
    }

    [Fact]
    public void RuleCommit_TableOverPreclassifiedClaimsUsesHeaderClaimPredicate()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
        {
            writer.Font(Standard14Font.GetHelvetica(), 12)
                .TextMove(50, 700).Text("Item")
                .TextMove(220, 700).Text("Amount")
                .TextMove(50, 680).Text("Widget")
                .TextMove(220, 680).Text("$10")
                .EndText();
        }

        using var session = doc.BeginRemediation(new RemediationSessionConfiguration
        {
            Language = "en-US",
            Title = "Header Selector",
            StrictConformance = false
        });

        var report = session.Commit(
            new Rule(
                "header-cells",
                RemediationActions.Tag("Span"),
                Predicates.Geo.Intersects(LayoutCoord.Absolute(new PdfRect<double>(0, 690, 800, 730))),
                Granularity.Word),
            new Rule(
                "body-cells",
                RemediationActions.Tag("Span"),
                Predicates.Geo.Intersects(LayoutCoord.Absolute(new PdfRect<double>(0, 650, 800, 690))),
                Granularity.Word),
            new Rule(
                "table",
                RemediationActions.TableOver(
                    ClaimPredicates.ClaimIs("Span"),
                    ClaimPredicates.FromRule("header-cells")),
                granularity: Granularity.Word,
                stage: Stage.Group));

        Assert.True(report.Committed);
        var table = Assert.Single(doc.Structure.GetRoot().Children, x => x.Type == "Table");
        Assert.Equal(new[] { "TH", "TH" }, table.Children[0].Children.Select(x => x.Type).ToArray());
        Assert.All(table.Children[0].Children, cell => Assert.Equal(StructureScope.Column, cell.Scope));
        Assert.Equal(new[] { "TD", "TD" }, table.Children[1].Children.Select(x => x.Type).ToArray());
        Assert.DoesNotContain("/TH <</MCID", page.DumpDecodedContents());
    }

    [Fact]
    public void RuleCommit_TableOverFlattenedCellsMovesLeafClaimContentIntoCells()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
        {
            writer.Font(Standard14Font.GetHelvetica(), 12)
                .TextMove(50, 700).Text("Item")
                .TextMove(220, 700).Text("Amount")
                .TextMove(50, 680).Text("Widget")
                .TextMove(220, 680).Text("$10")
                .EndText();
        }

        using var session = doc.BeginRemediation(new RemediationSessionConfiguration
        {
            Language = "en-US",
            Title = "Flattened Table",
            StrictConformance = false
        });

        var report = session.Commit(
            new Rule(
                "table-cells",
                RemediationActions.Tag("Span"),
                Predicates.Geo.Intersects(LayoutCoord.Absolute(new PdfRect<double>(0, 650, 800, 730))),
                Granularity.Word),
            new Rule(
                "table",
                RemediationActions.TableOverFlattenedCells(ClaimPredicates.ClaimIs("Span")),
                granularity: Granularity.Word,
                stage: Stage.Group));

        Assert.True(report.Committed);
        var table = Assert.Single(doc.Structure.GetRoot().Children, x => x.Type == "Table");
        var cells = table.Children.SelectMany(x => x.Children).ToArray();
        Assert.All(cells, cell =>
        {
            Assert.Empty(cell.Children);
            Assert.Single(cell.ContentItems);
        });
        Assert.DoesNotContain("/Span <</MCID", page.DumpDecodedContents());
        Assert.Contains("/TD <</MCID", page.DumpDecodedContents());
    }

    [Fact]
    public void RuleCommit_GroupClaimPredicateReparentsConsecutiveClaimsWithoutNewMcids()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
        {
            writer.Font(Standard14Font.GetHelvetica(), 12)
                .TextMove(50, 700).Text("A")
                .TextMove(70, 700).Text("B")
                .TextMove(90, 700).Text("C")
                .TextMove(110, 700).Text("D")
                .EndText();
        }

        using var session = doc.BeginRemediation(new RemediationSessionConfiguration
        {
            Language = "en-US",
            Title = "Groups",
            StrictConformance = false,
            LeftoverPolicy = RemediationLeftoverPolicy.AutoArtifact
        });

        var report = session.Commit(
            new Rule(
                "items",
                RemediationActions.Tag("LI"),
                Predicates.Text.Matches("^(A|B|D)$"),
                Granularity.Word),
            new Rule(
                "list",
                RemediationActions.Group("L", ClaimPredicates.ClaimIs("LI").And(ClaimPredicates.Consecutive())),
                granularity: Granularity.Word,
                stage: Stage.Group));

        Assert.True(report.Committed);
        var listClaims = report.Claims.Where(x => x.RuleId == "list").ToArray();
        Assert.Equal(2, listClaims.Length);
        Assert.Equal(new[] { 2, 1 }, listClaims.Select(x => x.RelatedClaims.Count).ToArray());

        var lists = doc.Structure.GetRoot().Children.Where(x => x.Type == "L").ToArray();
        Assert.Equal(2, lists.Length);
        Assert.Equal(2, lists[0].Children.Count);
        Assert.Single(lists[1].Children);
        Assert.All(lists.SelectMany(x => x.Children), child => Assert.Equal("LI", child.Type));
        Assert.Equal(new[] { 0, 1, 2 }, listClaims.SelectMany(x => x.AppliedBindings).SelectMany(x => x.Mcids).Distinct().OrderBy(x => x).ToArray());
        Assert.DoesNotContain("/L <</MCID", page.DumpDecodedContents());
    }

    [Fact]
    public void RuleCommit_GroupClaimPredicateCanBuildSectionFromHeadingAndParagraph()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
        {
            writer.Font(Standard14Font.GetHelvetica(), 12)
                .TextMove(50, 700).Text("Heading")
                .TextMove(120, 700).Text("Body")
                .EndText();
        }

        using var session = doc.BeginRemediation(new RemediationSessionConfiguration
        {
            Language = "en-US",
            Title = "Section",
            StrictConformance = false
        });

        var report = session.Commit(
            new Rule(
                "heading",
                RemediationActions.Tag("H1"),
                Predicates.Text.Equals("Heading"),
                Granularity.Word),
            new Rule(
                "paragraph",
                RemediationActions.Tag("P"),
                Predicates.Text.Equals("Body"),
                Granularity.Word),
            new Rule(
                "section",
                RemediationActions.Group(
                    "Sect",
                    ClaimPredicates.ClaimIs("H1").Or(ClaimPredicates.ClaimIs("P"))),
                granularity: Granularity.Word,
                stage: Stage.Group));

        Assert.True(report.Committed);
        var section = Assert.Single(doc.Structure.GetRoot().Children, x => x.Type == "Sect");
        Assert.Equal(new[] { "H1", "P" }, section.Children.Select(x => x.Type).ToArray());
        Assert.Equal(2, Assert.Single(report.Claims, x => x.RuleId == "section").RelatedClaims.Count);
    }

    [Fact]
    public void RuleCommit_MergeToFlattensLineClaimsIntoSingleParagraph()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
        {
            writer.Font(Standard14Font.GetHelvetica(), 12)
                .TextMove(50, 700).Text("Acme Corp")
                .TextMove(50, 684).Text("123 Main St")
                .EndText();
        }

        using var session = doc.BeginRemediation(new RemediationSessionConfiguration
        {
            Language = "en-US",
            Title = "Merged Paragraph",
            StrictConformance = false
        });

        var report = session.Commit(
            new Rule(
                "bill-to-lines",
                RemediationActions.Tag("Span"),
                Predicates.Text.Matches("^(Acme Corp|123 Main St)$"),
                Granularity.Line),
            new Rule(
                "bill-to-paragraph",
                RemediationActions.MergeTo("P", ClaimPredicates.FromRule("bill-to-lines")),
                granularity: Granularity.Line,
                stage: Stage.Group));

        Assert.True(report.Committed);
        var paragraph = Assert.Single(doc.Structure.GetRoot().Children, x => x.Type == "P");
        Assert.Empty(paragraph.Children);
        Assert.Equal(2, paragraph.ContentItems.Count);
        Assert.Equal(new[] { 0, 1 }, paragraph.ContentItems.Select(x => x.MCID).OrderBy(x => x).ToArray());
        Assert.Equal(2, Assert.Single(report.Claims, x => x.RuleId == "bill-to-paragraph").RelatedClaims.Count);
        Assert.DoesNotContain("/Span <</MCID", page.DumpDecodedContents());
        Assert.Equal(2, CountOccurrences(page.DumpDecodedContents(), "/P <</MCID"));
    }

    [Fact]
    public void RuleCommit_MergeToRejectsClaimsWithSemanticAttributes()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
        {
            writer.Font(Standard14Font.GetHelvetica(), 12).Text("Bonjour").EndText();
        }

        using var session = doc.BeginRemediation(new RemediationSessionConfiguration
        {
            Language = "en-US",
            Title = "Unsafe Merge",
            StrictConformance = false
        });

        var error = Assert.Throws<InvalidOperationException>(() => session.Commit(
            new Rule(
                "span",
                RemediationActions.Tag("Span", new PdfDictionary { [PdfName.ActualText] = new PdfString("Hello") }),
                Predicates.Text.Equals("Bonjour"),
                Granularity.Word),
            new Rule(
                "paragraph",
                RemediationActions.MergeTo("P", ClaimPredicates.FromRule("span")),
                granularity: Granularity.Word,
                stage: Stage.Group)));

        Assert.Contains("cannot be merged", error.Message);
    }

    [Fact]
    public void RuleCommit_RefineAttributeActionUpdatesExistingClaimStructureNode()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
        {
            writer.Font(Standard14Font.GetHelvetica(), 12).Text("Bonjour").EndText();
        }

        using var session = doc.BeginRemediation(new RemediationSessionConfiguration
        {
            Language = "en-US",
            Title = "Refine",
            StrictConformance = false
        });

        var report = session.Commit(
            new Rule(
                "paragraph",
                RemediationActions.Tag("P"),
                Predicates.Text.Equals("Bonjour"),
                Granularity.Word),
            new Rule(
                "lang",
                RemediationActions.Lang(ClaimPredicates.FromRule("paragraph"), "fr-CA"),
                granularity: Granularity.Word,
                stage: Stage.Refine));

        Assert.True(report.Committed);
        var paragraph = Assert.Single(doc.Structure.GetRoot().Children, x => x.Type == "P");
        Assert.Equal("fr-CA", paragraph.Language);
        var refineClaim = Assert.Single(report.Claims, x => x.RuleId == "lang");
        Assert.Single(refineClaim.RelatedClaims);
        Assert.Single(refineClaim.AppliedBindings);
    }

    [Fact]
    public void RuleCommit_ReorderSiblingsSortsMatchedClaimsByGeometry()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
        {
            writer.Font(Standard14Font.GetHelvetica(), 12)
                .TextMove(50, 650).Text("Bottom")
                .TextMove(50, 700).Text("Top")
                .EndText();
        }

        using var session = doc.BeginRemediation(new RemediationSessionConfiguration
        {
            Language = "en-US",
            Title = "Reorder",
            StrictConformance = false,
            DiagnosticStrictness = RemediationDiagnosticStrictness.Permissive
        });

        session.Suppress(DiagnosticCode.ReadingOrderDrift, "*", "Intentional reordering for test.");

        var report = session.Commit(
            new Rule(
                "bottom",
                RemediationActions.Tag("P"),
                Predicates.Text.Equals("Bottom"),
                Granularity.Word),
            new Rule(
                "top",
                RemediationActions.Tag("P"),
                Predicates.Text.Equals("Top"),
                Granularity.Word),
            new Rule(
                "logical-order",
                RemediationActions.ReorderSiblings(ClaimPredicates.ClaimIs("P"), SiblingReorderMode.GeometryTopToBottom),
                granularity: Granularity.Word,
                stage: Stage.Refine));

        Assert.True(report.Committed);
        var paragraphs = doc.Structure.GetRoot().Children.Where(x => x.Type == "P").ToArray();
        var topClaim = Assert.Single(report.Claims, x => x.RuleId == "top");
        var bottomClaim = Assert.Single(report.Claims, x => x.RuleId == "bottom");
        Assert.Same(topClaim.AppliedBindings[0].StructureNode, paragraphs[0]);
        Assert.Same(bottomClaim.AppliedBindings[0].StructureNode, paragraphs[1]);
        var reorderClaim = Assert.Single(report.Claims, x => x.RuleId == "logical-order");
        Assert.Equal(2, reorderClaim.RelatedClaims.Count);
        Assert.Equal(2, reorderClaim.AppliedBindings.Count);
    }

    [Fact]
    public void RuleCommit_LinkActionAuthorsStructureDestinationOverSourceClaim()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
        {
            writer.Font(Standard14Font.GetHelvetica(), 12)
                .TextMove(50, 700).Text("Jump")
                .TextMove(50, 650).Text("Target")
                .EndText();
        }

        using var session = doc.BeginRemediation(new RemediationSessionConfiguration
        {
            Language = "en-US",
            Title = "Links",
            StrictConformance = false
        });

        var report = session.Commit(
            new Rule(
                "source",
                RemediationActions.Tag("P"),
                Predicates.Text.Equals("Jump"),
                Granularity.Word),
            new Rule(
                "target",
                RemediationActions.Tag("H1"),
                Predicates.Text.Equals("Target"),
                Granularity.Word),
            new Rule(
                "link",
                RemediationActions.Link(
                    ClaimPredicates.FromRule("source"),
                    ClaimPredicates.FromRule("target"),
                    "Jump to target"),
                granularity: Granularity.Word,
                stage: Stage.Refine));

        Assert.True(report.Committed);
        var linkClaim = Assert.Single(report.Claims, x => x.RuleId == "link");
        Assert.Equal(2, linkClaim.RelatedClaims.Count);
        var linkNode = Assert.Single(doc.Structure.GetRoot().Children, x => x.Type == "Link");
        Assert.Equal("P", Assert.Single(linkNode.Children).Type);
        Assert.Single(linkNode.ObjectReferences);

        using var saved = PdfDocument.Open(doc.Save());
        var annotation = saved.Pages[0].NativeObject.Get<PdfArray>(PdfName.Annots)![0].Resolve().GetAs<PdfDictionary>();
        Assert.Equal(PdfName.Link, annotation.Get<PdfName>(PdfName.Subtype));
        Assert.Equal("Jump to target", annotation.Get<PdfString>(PdfName.Contents)!.Value);
        var destination = annotation.Get<PdfArray>(PdfName.Dest)!;
        Assert.Equal(PdfName.StructElem, destination[0].Resolve().GetAs<PdfDictionary>().Get<PdfName>(PdfName.TYPE));
        Assert.True(annotation.ContainsKey(PdfName.StructParent));
    }

    [Fact]
    public void RuleDryRun_TableInferenceReportsConfidenceAndHonorsThreshold()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
        {
            writer.Font(Standard14Font.GetHelvetica(), 12)
                .TextMove(50, 700).Text("A1")
                .TextMove(220, 700).Text("B1")
                .TextMove(380, 700).Text("C1")
                .TextMove(50, 680).Text("A2")
                .EndText();
        }

        using var session = doc.BeginRemediation();
        var report = session.DryRun(new Rule(
            "table",
            RemediationActions.Table(),
            Predicates.Geo.Intersects(LayoutCoord.Absolute(new PdfRect<double>(0, 650, 520, 730))),
            Granularity.Word,
            stage: Stage.Group));

        var inferred = Assert.Single(report.Claims);
        Assert.Equal(0.5, inferred.Confidence);

        using var thresholdSession = doc.BeginRemediation();
        var thresholded = thresholdSession.DryRun(new Rule(
            "table",
            RemediationActions.Table(),
            Predicates.Geo.Intersects(LayoutCoord.Absolute(new PdfRect<double>(0, 650, 520, 730))),
            Granularity.Word,
            stage: Stage.Group,
            minConfidence: 0.8));

        Assert.Empty(thresholded.Claims);
        var skipped = Assert.Single(thresholded.SkippedClaims);
        Assert.Equal("table", skipped.RuleId);
        Assert.Equal(0.5, skipped.Confidence);
    }

    [Fact]
    public void RuleSet_UseComposesRulesAndPropagatesRuleSetId()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
        {
            writer.Font(Standard14Font.GetHelvetica(), 12).Text("Hello").EndText();
        }

        var common = new RuleSet(
            "common",
            new Rule(
                "paragraph",
                RemediationActions.Tag("P"),
                Predicates.Text.Equals("Hello"),
                Granularity.Word));

        using var session = doc.BeginRemediation();
        session.Use(common);

        var report = session.DryRun();

        var claim = Assert.Single(report.Claims);
        Assert.Equal("paragraph", claim.RuleId);
        Assert.Equal("common", claim.RuleSetId);
        Assert.True(ClaimPredicates.FromRuleSet("common").Evaluate(claim).IsMatch);
    }

    [Fact]
    public void LeftoverPolicy_FailFastBlocksCommitBeforeMutation()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
        {
            writer.Font(Standard14Font.GetHelvetica(), 12).Text("Hello").EndText();
        }

        using var session = doc.BeginRemediation(new RemediationSessionConfiguration
        {
            LeftoverPolicy = RemediationLeftoverPolicy.FailFast
        });

        var error = Assert.Throws<InvalidOperationException>(() => session.Commit(Array.Empty<Rule>()));

        Assert.Contains("unclaimed", error.Message);
        Assert.DoesNotContain("Artifact", page.DumpDecodedContents());
        Assert.False(doc.Catalog.ContainsKey(PdfName.MarkInfo));
    }

    private sealed record LowConfidencePredicate : RemediationPredicate
    {
        public override string DebugString => "low-confidence";

        public override PredicateResult Evaluate(RemediationEvaluationContext context, RemediationCandidate candidate) =>
            PredicateResult.Match(0.5);
    }

    private static int CountOccurrences(string text, string value)
    {
        var count = 0;
        var index = 0;
        while ((index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }

        return count;
    }
}
