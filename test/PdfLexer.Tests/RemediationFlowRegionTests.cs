using System;
using System.Linq;
using PdfLexer.Content;
using PdfLexer.DOM;
using PdfLexer.Fonts;
using PdfLexer.Remediation;
using PdfLexer.Writing;
using Xunit;

namespace PdfLexer.Tests;

public class RemediationFlowRegionTests
{
    [Fact]
    public void Continued_Flow_Classifies_And_Groups_Across_Boundary_Free_Pages()
    {
        using var doc = PdfDocument.Create();
        var first = doc.AddPage(PageSize.LETTER);
        var middle = doc.AddPage(PageSize.LETTER);
        var last = doc.AddPage(PageSize.LETTER);
        using (var writer = first.GetWriter())
        {
            writer.Save().Font(Base14.Helvetica, 12).TextMove(72, 720).Text("Section Start").Restore();
            writer.Save().Font(Base14.Helvetica, 12).TextMove(72, 690).Text("First page body").Restore();
        }
        using (var writer = middle.GetWriter())
        {
            writer.Save().Font(Base14.Helvetica, 12).TextMove(72, 700).Text("Middle page body").Restore();
        }
        using (var writer = last.GetWriter())
        {
            writer.Save().Font(Base14.Helvetica, 12).TextMove(72, 700).Text("Last page body").Restore();
            writer.Save().Font(Base14.Helvetica, 12).TextMove(72, 660).Text("Section End").Restore();
        }

        var anchors = new[]
        {
            RemediationAnchor.TextLabel("start", "Section Start") with { Pages = PageSelector.First },
            RemediationAnchor.TextLabel("end", "Section End") with { Pages = PageSelector.Last }
        };
        var flow = new FlowRegion(
            "section",
            FlowBoundary.Anchor("start"),
            FlowBoundary.Anchor("end"),
            ContinuationPolicy: FlowContinuationPolicy.ContinueUntilEnd);
        var classify = new Rule(
            "body",
            RemediationActions.Tag("P"),
            Predicates.Flow.InFlowRegion("section"),
            CandidateSelector.Text(Granularity.Line));
        var group = new Rule(
            "section-group",
            RemediationActions.Group(
                "Div",
                ClaimPredicates.ClaimIs("P").And(ClaimPredicates.Consecutive())),
            stage: Stage.Group);
        var refine = new Rule(
            "section-language",
            RemediationActions.Lang(
                ClaimPredicates.FromRule("section-group")
                    .And(ClaimPredicates.Within("section")),
                "en-US"),
            stage: Stage.Refine);
        var ruleSet = new RuleSet(
            "continued",
            new[] { classify, group, refine },
            anchors,
            flowRegions: new[] { flow });

        var report = doc.BeginRemediation(new RemediationSessionConfiguration
            {
                LeftoverPolicy = RemediationLeftoverPolicy.AutoArtifact,
                StrictConformance = false
            })
            .Use(ruleSet)
            .DryRun();

        Assert.DoesNotContain(report.Diagnostics, x => x.Contains("boundary", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(3, report.Claims.Count(x => x.RuleId == "body"));
        var grouped = Assert.Single(report.Claims.Where(x => x.RuleId == "section-group"));
        Assert.Equal(new[] { 0, 1, 2 }, grouped.PageIndexes);
        Assert.Null(grouped.BoundingBox);
        Assert.Equal(3, grouped.BoundsByPage.Count);
        Assert.Single(report.Claims, x => x.RuleId == "section-language");
    }

    [Fact]
    public void Continued_Flow_Materializes_One_Cross_Page_Structure_Node()
    {
        using var doc = PdfDocument.Create();
        for (var pageIndex = 0; pageIndex < 3; pageIndex++)
        {
            var page = doc.AddPage(PageSize.LETTER);
            using var writer = page.GetWriter();
            if (pageIndex == 0)
            {
                writer.Save().Font(Base14.Helvetica, 12).TextMove(72, 720).Text("Start").Restore();
            }
            writer.Save().Font(Base14.Helvetica, 12).TextMove(72, 690).Text($"Body {pageIndex + 1}").Restore();
            if (pageIndex == 2)
            {
                writer.Save().Font(Base14.Helvetica, 12).TextMove(72, 650).Text("End").Restore();
            }
        }

        var anchors = new[]
        {
            RemediationAnchor.TextLabel("start", "Start") with { Pages = PageSelector.First },
            RemediationAnchor.TextLabel("end", "End") with { Pages = PageSelector.Last }
        };
        var rules = new[]
        {
            new Rule(
                "body",
                RemediationActions.Tag("P"),
                Predicates.Flow.InFlowRegion("section"),
                CandidateSelector.Text(Granularity.Line)),
            new Rule(
                "section-group",
                RemediationActions.Group("Div", ClaimPredicates.ClaimIs("P").And(ClaimPredicates.Consecutive())),
                stage: Stage.Group)
        };
        var ruleSet = new RuleSet(
            "continued",
            rules,
            anchors,
            flowRegions: new[]
            {
                new FlowRegion(
                    "section",
                    FlowBoundary.Anchor("start"),
                    FlowBoundary.Anchor("end"),
                    ContinuationPolicy: FlowContinuationPolicy.ContinueUntilEnd)
            });

        var report = doc.BeginRemediation(new RemediationSessionConfiguration
            {
                LeftoverPolicy = RemediationLeftoverPolicy.AutoArtifact,
                StrictConformance = false
            })
            .Use(ruleSet)
            .Commit();

        Assert.True(report.Committed);
        var group = Assert.Single(report.Claims.Where(x => x.RuleId == "section-group"));
        var node = Assert.Single(group.AppliedBindings).StructureNode!;
        Assert.Equal(3, node.Children.Count);
        Assert.Equal(3, node.Children.SelectMany(x => x.ContentItems).Select(x => x.Page).Distinct().Count());
    }

    [Fact]
    public void Continued_Flow_MaxPages_Is_Validated_And_Caps_Unterminated_Activation()
    {
        var invalid = new FlowRegion(
            "invalid",
            FlowBoundary.PageBoundary,
            FlowBoundary.PageBoundary,
            MaxExtent: 20,
            ContinuationPolicy: FlowContinuationPolicy.ContinueUntilEnd,
            MaxPages: 0);
        Assert.Contains(invalid.Validate(), x => x.Contains("max extent", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(invalid.Validate(), x => x.Contains("greater than zero", StringComparison.OrdinalIgnoreCase));

        using var doc = PdfDocument.Create();
        for (var pageIndex = 0; pageIndex < 3; pageIndex++)
        {
            var page = doc.AddPage(PageSize.LETTER);
            using var writer = page.GetWriter();
            writer.Save().Font(Base14.Helvetica, 12).TextMove(72, 700).Text(
                pageIndex == 0 ? "Start" : $"Body {pageIndex}").Restore();
        }

        var anchor = RemediationAnchor.TextLabel("start", "Start") with { Pages = PageSelector.First };
        var flow = new FlowRegion(
            "limited",
            FlowBoundary.Anchor("start"),
            FlowBoundary.Matching(Predicates.Text.Equals("Never")),
            ContinuationPolicy: FlowContinuationPolicy.ContinueUntilEnd,
            MaxPages: 2);
        var rule = new Rule(
            "body",
            RemediationActions.Tag("P"),
            Predicates.Flow.InFlowRegion("limited"),
            CandidateSelector.Text(Granularity.Line));

        var report = doc.BeginRemediation()
            .Use(new RuleSet("limited", new[] { rule }, new[] { anchor }, flowRegions: new[] { flow }))
            .DryRun();

        Assert.Contains(report.Diagnostics, x => x.Contains("MaxPages=2", StringComparison.Ordinal));
    }

    [Fact]
    public void Continued_Table_Uses_One_Table_And_Preserves_Each_Page_Header()
    {
        using var doc = PdfDocument.Create();
        var testDir = PathUtil.GetPathFromSegmentOfCurrent("test");
        var font = TrueTypeFont.CreateWritableFont(
            System.IO.File.ReadAllBytes(System.IO.Path.Combine(testDir, "Roboto-Regular.ttf")));
        for (var pageIndex = 0; pageIndex < 2; pageIndex++)
        {
            var page = doc.AddPage(PageSize.LETTER);
            using var writer = page.GetWriter();
            if (pageIndex == 0)
            {
                writer.Save().Font(font, 12).TextMove(72, 770).Text("Intro A").Restore();
                writer.Save().Font(font, 12).TextMove(72, 745).Text("Intro B").Restore();
            }
            writer.Save().Font(font, 12).TextMove(72, 720).Text("Item").Restore();
            writer.Save().Font(font, 12).TextMove(300, 720).Text("Amount").Restore();
            writer.Save().Font(font, 12).TextMove(72, 690).Text($"Widget{pageIndex + 1}").Restore();
            writer.Save().Font(font, 12).TextMove(300, 690).Text($"{(pageIndex + 1) * 10}.00").Restore();
            if (pageIndex == 1)
            {
                writer.Save().Font(font, 12).TextMove(72, 650).Text("Total").Restore();
            }
        }

        var anchors = new[]
        {
            RemediationAnchor.TextLabel("header", "Item"),
            RemediationAnchor.TextLabel("total", "Total") with { Pages = PageSelector.Last }
        };
        var rules = new[]
        {
            new Rule(
                "intro",
                RemediationActions.Tag("P"),
                Predicates.Text.StartsWith("Intro"),
                CandidateSelector.Text(Granularity.Line)),
            new Rule(
                "header-cell",
                RemediationActions.Tag("Span"),
                Predicates.Anchor.SameRowAs("header", 4),
                CandidateSelector.Text(Granularity.Word)),
            new Rule(
                "body-cell",
                RemediationActions.Tag("Span"),
                Predicates.Flow.InFlowRegion("items"),
                CandidateSelector.Text(Granularity.Word)),
            new Rule(
                "table",
                RemediationActions.TableOverFlattenedCells(
                    ClaimPredicates.FromRule("header-cell").Or(ClaimPredicates.FromRule("body-cell")),
                    ClaimPredicates.FromRule("header-cell"),
                    50, 220, 500),
                stage: Stage.Group)
        };
        var flow = new FlowRegion(
            "items",
            FlowBoundary.Anchor("header"),
            FlowBoundary.Anchor("total"),
            ContinuationPolicy: FlowContinuationPolicy.ContinueUntilEnd);

        var report = doc.BeginRemediation(new RemediationSessionConfiguration
            {
                LeftoverPolicy = RemediationLeftoverPolicy.AutoArtifact,
                StrictConformance = true,
                DiagnosticStrictness = RemediationDiagnosticStrictness.Permissive
            })
            .Use(new RuleSet("table", rules, anchors, flowRegions: new[] { flow }))
            .Commit();

        Assert.True(report.Committed);
        var tableClaim = Assert.Single(report.Claims.Where(x => x.RuleId == "table"));
        Assert.Equal(new[] { 0, 1 }, tableClaim.PageIndexes);
        var tableNode = Assert.Single(tableClaim.AppliedBindings.Where(x => x.ProducedTag == "Table")).StructureNode!;
        Assert.Equal(4, tableNode.Children.Count);
        Assert.All(tableNode.Children, row => Assert.Equal(2, row.Children.Count));
        Assert.Equal(
            new[] { "TH,TH", "TD,TD", "TH,TH", "TD,TD" },
            tableNode.Children
                .Select(row => string.Join(",", row.Children.Select(cell => cell.Type)))
                .ToArray());
        Assert.Equal(
            new[] { "P", "P", "Table" },
            doc.Structure.GetRoot().Children.Select(x => x.Type).ToArray());
        Assert.DoesNotContain(report.Diagnostics, x => x.Contains("reading order drift", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData(TableHeaderRowsScope.LogicalTable, 1)]
    [InlineData(TableHeaderRowsScope.EveryPage, 2)]
    public void Continued_Table_HeaderRows_Honor_Declared_Scope(
        TableHeaderRowsScope scope,
        int expectedHeaderRows)
    {
        using var doc = PdfDocument.Create();
        for (var pageIndex = 0; pageIndex < 2; pageIndex++)
        {
            var page = doc.AddPage(PageSize.LETTER);
            using var writer = page.GetWriter();
            if (pageIndex == 0)
            {
                writer.Save().Font(Base14.Helvetica, 12).TextMove(72, 750).Text("Start").Restore();
            }
            writer.Save().Font(Base14.Helvetica, 12).TextMove(72, 720).Text("Item").Restore();
            writer.Save().Font(Base14.Helvetica, 12).TextMove(300, 720).Text("Amount").Restore();
            writer.Save().Font(Base14.Helvetica, 12).TextMove(72, 690).Text($"Widget{pageIndex + 1}").Restore();
            writer.Save().Font(Base14.Helvetica, 12).TextMove(300, 690).Text($"{pageIndex + 1}0.00").Restore();
            if (pageIndex == 1)
            {
                writer.Save().Font(Base14.Helvetica, 12).TextMove(72, 650).Text("Total").Restore();
            }
        }

        var anchors = new[]
        {
            RemediationAnchor.TextLabel("start", "Start") with { Pages = PageSelector.First },
            RemediationAnchor.TextLabel("total", "Total") with { Pages = PageSelector.Last }
        };
        var flow = new FlowRegion(
            "items",
            FlowBoundary.Anchor("start"),
            FlowBoundary.Anchor("total"),
            ContinuationPolicy: FlowContinuationPolicy.ContinueUntilEnd);
        var rules = new[]
        {
            new Rule(
                "cells",
                RemediationActions.Tag("Span"),
                Predicates.Flow.InFlowRegion("items"),
                CandidateSelector.Text(Granularity.Word)),
            new Rule(
                "table",
                RemediationActions.TableOverFlattenedCells(
                    ClaimPredicates.FromRule("cells"),
                    1,
                    scope,
                    50, 220, 500),
                stage: Stage.Group)
        };

        var report = doc.BeginRemediation(new RemediationSessionConfiguration
            {
                LeftoverPolicy = RemediationLeftoverPolicy.AutoArtifact,
                StrictConformance = false,
                DiagnosticStrictness = RemediationDiagnosticStrictness.Permissive
            })
            .Suppress(
                DiagnosticCode.ReadingOrderDrift,
                "*",
                "This test isolates the explicit header-row scope contract; the unsuppressed cross-page table fixture verifies ordering.")
            .Use(new RuleSet("table", rules, anchors, flowRegions: new[] { flow }))
            .Commit();

        var tableClaim = Assert.Single(report.Claims, x => x.RuleId == "table");
        var tableNode = Assert.Single(
            tableClaim.AppliedBindings,
            x => x.ProducedTag == "Table").StructureNode!;
        Assert.Equal(4, tableNode.Children.Count);
        Assert.Equal(
            expectedHeaderRows,
            tableNode.Children.Count(row => row.Children.All(cell => cell.Type == "TH")));
    }

    [Fact]
    public void Repeated_Region_Activations_Do_Not_Merge_And_SamePage_Forces_Local_Groups()
    {
        using var doc = PdfDocument.Create();
        for (var pageIndex = 0; pageIndex < 4; pageIndex++)
        {
            var page = doc.AddPage(PageSize.LETTER);
            using var writer = page.GetWriter();
            if (pageIndex % 2 == 0)
            {
                writer.Save().Font(Base14.Helvetica, 12).TextMove(72, 720).Text("Start").Restore();
            }
            writer.Save().Font(Base14.Helvetica, 12).TextMove(72, 690).Text($"Body {pageIndex + 1}").Restore();
            if (pageIndex % 2 == 1)
            {
                writer.Save().Font(Base14.Helvetica, 12).TextMove(72, 650).Text("End").Restore();
            }
        }

        var anchors = new[]
        {
            RemediationAnchor.TextLabel("start", "Start"),
            RemediationAnchor.TextLabel("end", "End")
        };
        var flow = new FlowRegion(
            "sections",
            FlowBoundary.Anchor("start"),
            FlowBoundary.Anchor("end"),
            ContinuationPolicy: FlowContinuationPolicy.ContinueUntilEnd);
        var classify = new Rule(
            "body",
            RemediationActions.Tag("P"),
            Predicates.Flow.InFlowRegion("sections"),
            CandidateSelector.Text(Granularity.Line));
        var continuedGroup = new Rule(
            "continued-group",
            RemediationActions.Group(
                "Div",
                ClaimPredicates.ClaimIs("P").And(ClaimPredicates.Consecutive())),
            stage: Stage.Group);

        var report = doc.BeginRemediation()
            .Use(new RuleSet(
                "activation",
                new[] { classify, continuedGroup },
                anchors,
                flowRegions: new[] { flow }))
            .DryRun();

        var groups = report.Claims.Where(x => x.RuleId == "continued-group").ToArray();
        Assert.Equal(2, groups.Length);
        Assert.Equal(new[] { 0, 1 }, groups[0].PageIndexes);
        Assert.Equal(new[] { 2, 3 }, groups[1].PageIndexes);
        Assert.Single(report.Warnings);

        var localGroup = continuedGroup with
        {
            Action = RemediationActions.Group(
                "Div",
                ClaimPredicates.ClaimIs("P")
                    .And(ClaimPredicates.Consecutive())
                    .And(ClaimPredicates.SamePage()))
        };
        var localReport = doc.BeginRemediation()
            .Use(new RuleSet(
                "local",
                new[] { classify, localGroup },
                anchors,
                flowRegions: new[] { flow }))
            .DryRun();
        Assert.Equal(4, localReport.Claims.Count(x => x.RuleId == "continued-group"));
        Assert.Empty(localReport.Warnings);

        var commitReport = doc.BeginRemediation(new RemediationSessionConfiguration
            {
                LeftoverPolicy = RemediationLeftoverPolicy.AutoArtifact,
                StrictConformance = false
            })
            .Use(new RuleSet(
                "activation-commit",
                new[] { classify, continuedGroup },
                anchors,
                flowRegions: new[] { flow }))
            .Commit();
        Assert.Equal(report.Warnings, commitReport.Warnings);
    }

    [Fact]
    public void Flow_Order_Predicates_Select_Across_The_Whole_Activation()
    {
        using var doc = PdfDocument.Create();
        var texts = new[]
        {
            new[] { "Start", "Alpha" },
            new[] { "Bravo" },
            new[] { "Charlie", "End" }
        };
        foreach (var pageTexts in texts)
        {
            var page = doc.AddPage(PageSize.LETTER);
            using var writer = page.GetWriter();
            for (var i = 0; i < pageTexts.Length; i++)
            {
                writer.Save().Font(Base14.Helvetica, 12)
                    .TextMove(72, 720 - i * 30)
                    .Text(pageTexts[i])
                    .Restore();
            }
        }

        var anchors = new[]
        {
            RemediationAnchor.TextLabel("start", "Start") with { Pages = PageSelector.First },
            RemediationAnchor.TextLabel("end", "End") with { Pages = PageSelector.Last }
        };
        var flow = new FlowRegion(
            "ordered",
            FlowBoundary.Anchor("start"),
            FlowBoundary.Anchor("end"),
            ContinuationPolicy: FlowContinuationPolicy.ContinueUntilEnd);
        var rules = new[]
        {
            new Rule(
                "first",
                RemediationActions.Tag("P"),
                Predicates.Flow.FirstIn("ordered"),
                CandidateSelector.Text(Granularity.Line)),
            new Rule(
                "middle",
                RemediationActions.Tag("P"),
                Predicates.Flow.NthIn("ordered", 1),
                CandidateSelector.Text(Granularity.Line)),
            new Rule(
                "last",
                RemediationActions.Tag("P"),
                Predicates.Flow.LastIn("ordered"),
                CandidateSelector.Text(Granularity.Line))
        };

        var report = doc.BeginRemediation()
            .Use(new RuleSet("ordered", rules, anchors, flowRegions: new[] { flow }))
            .DryRun();

        Assert.Equal(
            new[] { "Alpha", "Bravo", "Charlie" },
            new[] { "first", "middle", "last" }
                .Select(ruleId => Assert.IsType<TextRemediationCandidate>(
                    Assert.Single(report.Claims.Where(x => x.RuleId == ruleId)).Candidates[0]).Text)
                .ToArray());
    }

    [Fact]
    public void Flow_Region_Selects_Line_Items_From_Header_Until_Subtotal()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
        {
            writer.Save().Font(Base14.Helvetica, 12).TextMove(100, 700).Text("Item Qty Amount").Restore();
            writer.Save().Font(Base14.Helvetica, 12).TextMove(100, 680).Text("Widget 2 10.00").Restore();
            writer.Save().Font(Base14.Helvetica, 12).TextMove(100, 660).Text("Gadget 1 5.00").Restore();
            writer.Save().Font(Base14.Helvetica, 12).TextMove(100, 640).Text("Subtotal 15.00").Restore();
        }

        var anchors = new[]
        {
            RemediationAnchor.TableHeader("line-items-header", "Item", "Qty", "Amount"),
            RemediationAnchor.TextLabel("subtotal-label", "Subtotal", StringComparison.Ordinal)
        };
        var region = new FlowRegion(
            "line-items",
            FlowBoundary.Anchor("line-items-header"),
            FlowBoundary.Anchor("subtotal-label"));
        var rule = new Rule(
            "tag-line-items",
            RemediationActions.Tag("TR"),
            Predicates.Flow.InFlowRegion("line-items"),
            CandidateSelector.Text(Granularity.Line));
        var ruleSet = new RuleSet("invoice", new[] { rule }, anchors, flowRegions: new[] { region });

        var report = doc.BeginRemediation().Use(ruleSet).DryRun();

        var claims = report.Claims.Where(x => x.RuleId == "tag-line-items").ToList();
        Assert.Equal(new[] { "Widget 2 10.00", "Gadget 1 5.00" }, claims.Select(x => Assert.IsType<TextRemediationCandidate>(x.Candidates[0]).Text).ToArray());
        Assert.DoesNotContain(claims, x => Assert.IsType<TextRemediationCandidate>(x.Candidates[0]).Text.Contains("Subtotal", StringComparison.Ordinal));
    }

    [Fact]
    public void Flow_Region_Selects_Wrapped_Address_Until_Next_Section_Label()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
        {
            writer.Save().Font(Base14.Helvetica, 12).TextMove(72, 700).Text("Bill To").Restore();
            writer.Save().Font(Base14.Helvetica, 12).TextMove(72, 680).Text("Contoso LLC").Restore();
            writer.Save().Font(Base14.Helvetica, 12).TextMove(72, 660).Text("123 Long Transactional Address").Restore();
            writer.Save().Font(Base14.Helvetica, 12).TextMove(72, 640).Text("Suite 400").Restore();
            writer.Save().Font(Base14.Helvetica, 12).TextMove(72, 620).Text("Ship To").Restore();
            writer.Save().Font(Base14.Helvetica, 12).TextMove(72, 600).Text("Warehouse").Restore();
        }

        var anchors = new[]
        {
            RemediationAnchor.TextLabel("bill-to-label", "Bill To", StringComparison.Ordinal),
            RemediationAnchor.TextLabel("ship-to-label", "Ship To", StringComparison.Ordinal)
        };
        var region = new FlowRegion(
            "bill-to-address",
            FlowBoundary.Anchor("bill-to-label"),
            FlowBoundary.Anchor("ship-to-label"));
        var rule = new Rule(
            "tag-bill-to",
            RemediationActions.Tag("P"),
            Predicates.Flow.InFlowRegion("bill-to-address"),
            CandidateSelector.Text(Granularity.Line));
        var ruleSet = new RuleSet("statement", new[] { rule }, anchors, flowRegions: new[] { region });

        var report = doc.BeginRemediation().Use(ruleSet).DryRun();

        var texts = report.Claims.Where(x => x.RuleId == "tag-bill-to").Select(x => Assert.IsType<TextRemediationCandidate>(x.Candidates[0]).Text).ToArray();
        Assert.Equal(new[] { "Contoso LLC", "123 Long Transactional Address", "Suite 400" }, texts);
    }

    [Fact]
    public void Toleranced_Footer_Zone_Adapts_Across_Page_Size_Variance()
    {
        using var doc = PdfDocument.Create();
        var letter = doc.AddPage(PageSize.LETTER);
        var a4 = doc.AddPage(PageSize.A4);
        using (var writer = letter.GetWriter())
        {
            writer.Save().Font(Base14.Helvetica, 10).TextMove(260, 30).Text("Page 1").Restore();
        }

        using (var writer = a4.GetWriter())
        {
            writer.Save().Font(Base14.Helvetica, 10).TextMove(260, 32).Text("Page 2").Restore();
        }

        var zone = new TolerancedZone(
            "footer",
            LayoutCoord.MarginRelative(bottom: 36),
            Tolerance: 4);
        var rule = new Rule(
            "artifact-footer",
            RemediationActions.Artifact(ArtifactSubtype.Pagination),
            Predicates.Flow.InZone("footer"),
            CandidateSelector.Text(Granularity.Line));
        var ruleSet = new RuleSet("common", new[] { rule }, Array.Empty<RemediationAnchor>(), tolerancedZones: new[] { zone });

        var report = doc.BeginRemediation().Use(ruleSet).DryRun();

        var claims = report.Claims.Where(x => x.RuleId == "artifact-footer").ToArray();
        Assert.Equal(2, claims.Length);
        Assert.All(claims, x => Assert.Equal(RemediationActionKind.Artifact, x.ActionKind));
    }

    [Fact]
    public void Claim_Within_Dispatches_To_Anchors_And_Zones_In_Document_Pipeline()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
        {
            writer.Save().Font(Base14.Helvetica, 12).TextMove(72, 700).Text("Anchor Label").Restore();
            writer.Save().Font(Base14.Helvetica, 12).TextMove(72, 40).Text("Footer").Restore();
        }

        var anchors = new[] { RemediationAnchor.TextLabel("label", "Anchor Label") };
        var zones = new[]
        {
            new TolerancedZone(
                "footer",
                LayoutCoord.Absolute(new PdfRect<double>(0, 0, 612, 80)))
        };
        var rules = new[]
        {
            new Rule(
                "lines",
                RemediationActions.Tag("P"),
                RemediationPredicate.Always,
                CandidateSelector.Text(Granularity.Line)),
            new Rule(
                "anchor-group",
                RemediationActions.Group(
                    "Div",
                    ClaimPredicates.FromRule("lines").And(ClaimPredicates.Within("label"))),
                stage: Stage.Group),
            new Rule(
                "zone-group",
                RemediationActions.Group(
                    "Div",
                    ClaimPredicates.FromRule("lines").And(ClaimPredicates.Within("footer"))),
                stage: Stage.Group)
        };

        var report = doc.BeginRemediation()
            .Use(new RuleSet("within", rules, anchors, zones))
            .DryRun();

        Assert.Single(report.Claims, x => x.RuleId == "anchor-group");
        Assert.Single(report.Claims, x => x.RuleId == "zone-group");
    }

    [Fact]
    public void Flow_Order_Predicates_Select_First_And_Nth_Candidates_In_Region()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
        {
            writer.Save().Font(Base14.Helvetica, 12).TextMove(100, 700).Text("Item Qty Amount").Restore();
            writer.Save().Font(Base14.Helvetica, 12).TextMove(100, 680).Text("Widget 2 10.00").Restore();
            writer.Save().Font(Base14.Helvetica, 12).TextMove(100, 660).Text("Gadget 1 5.00").Restore();
            writer.Save().Font(Base14.Helvetica, 12).TextMove(100, 640).Text("Subtotal 15.00").Restore();
        }

        var anchors = new[]
        {
            RemediationAnchor.TableHeader("line-items-header", "Item", "Qty", "Amount"),
            RemediationAnchor.TextLabel("subtotal-label", "Subtotal", StringComparison.Ordinal)
        };
        var region = new FlowRegion(
            "line-items",
            FlowBoundary.Anchor("line-items-header"),
            FlowBoundary.Anchor("subtotal-label"));
        var first = new Rule(
            "tag-first-line-item",
            RemediationActions.Tag("P"),
            Predicates.Flow.FirstIn("line-items"),
            CandidateSelector.Text(Granularity.Line));
        var second = new Rule(
            "tag-second-line-item",
            RemediationActions.Tag("P"),
            Predicates.Flow.NthIn("line-items", 1),
            CandidateSelector.Text(Granularity.Line));

        var report = doc.BeginRemediation()
            .Use(new RuleSet("invoice", new[] { first, second }, anchors, flowRegions: new[] { region }))
            .DryRun();

        Assert.Equal("Widget 2 10.00", Assert.IsType<TextRemediationCandidate>(Assert.Single(report.Claims.Where(x => x.RuleId == "tag-first-line-item")).Candidates[0]).Text);
        Assert.Equal("Gadget 1 5.00", Assert.IsType<TextRemediationCandidate>(Assert.Single(report.Claims.Where(x => x.RuleId == "tag-second-line-item")).Candidates[0]).Text);
    }

    [Fact]
    public void GeometricWithin_RejectsCrossPageParentAndNamesFlowRegionWorkaround()
    {
        using var doc = PdfDocument.Create();
        var first = doc.AddPage(PageSize.LETTER);
        var second = doc.AddPage(PageSize.LETTER);
        using (var writer = first.GetWriter())
        {
            writer.Save().Font(Base14.Helvetica, 12).TextMove(72, 720).Text("Start").Restore();
            writer.Save().Font(Base14.Helvetica, 12).TextMove(72, 690).Text("Body one").Restore();
        }
        using (var writer = second.GetWriter())
        {
            writer.Save().Font(Base14.Helvetica, 12).TextMove(72, 690).Text("Body two").Restore();
            writer.Save().Font(Base14.Helvetica, 12).TextMove(72, 650).Text("End").Restore();
        }

        var anchors = new[]
        {
            RemediationAnchor.TextLabel("start", "Start") with { Pages = PageSelector.First },
            RemediationAnchor.TextLabel("end", "End") with { Pages = PageSelector.Last }
        };
        var rules = new[]
        {
            new Rule(
                "body",
                RemediationActions.Tag("P"),
                Predicates.Flow.InFlowRegion("section"),
                CandidateSelector.Text(Granularity.Line)),
            new Rule(
                "cross-page-parent",
                RemediationActions.Group("Div", ClaimPredicates.FromRule("body").And(ClaimPredicates.Consecutive())),
                stage: Stage.Group),
            new Rule(
                "geometric-consumer",
                RemediationActions.Group(
                    "Sect",
                    ClaimPredicates.FromRule("cross-page-parent").And(ClaimPredicates.Within("page-zone"))),
                stage: Stage.Group,
                groupPass: 10)
        };
        var ruleSet = new RuleSet(
            "cross-page-geometry",
            rules,
            anchors,
            new[] { new TolerancedZone("page-zone", LayoutCoord.Absolute(new PdfRect<double>(0, 0, 612, 792))) },
            new[]
            {
                new FlowRegion(
                    "section",
                    FlowBoundary.Anchor("start"),
                    FlowBoundary.Anchor("end"),
                    ContinuationPolicy: FlowContinuationPolicy.ContinueUntilEnd)
            });

        var report = doc.BeginRemediation(new RemediationSessionConfiguration
            {
                LeftoverPolicy = RemediationLeftoverPolicy.AutoArtifact,
                StrictConformance = false
            })
            .Use(ruleSet)
            .DryRun();

        Assert.Contains(report.Diagnostics, x =>
            x.Contains("geometric-consumer", StringComparison.Ordinal) &&
            x.Contains("cross-page claim", StringComparison.Ordinal) &&
            x.Contains("Use Within(flowRegionId)", StringComparison.Ordinal));
        Assert.DoesNotContain(report.Claims, x => x.RuleId == "geometric-consumer");
    }
}
