using System;
using System.Linq;
using PdfLexer.DOM;
using PdfLexer.Fonts;
using PdfLexer.Remediation;
using PdfLexer.Writing;
using Xunit;

namespace PdfLexer.Tests;

public class RemediationFlowRegionTests
{
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
            Granularity.Line);
        var ruleSet = new RuleSet("invoice", new[] { rule }, anchors, flowRegions: new[] { region });

        var report = doc.BeginRemediation().Use(ruleSet).DryRun();

        var claims = report.Claims.Where(x => x.RuleId == "tag-line-items").ToList();
        Assert.Equal(new[] { "Widget 2 10.00", "Gadget 1 5.00" }, claims.Select(x => x.Candidates[0].Text).ToArray());
        Assert.DoesNotContain(claims, x => x.Candidates[0].Text.Contains("Subtotal", StringComparison.Ordinal));
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
            Granularity.Line);
        var ruleSet = new RuleSet("statement", new[] { rule }, anchors, flowRegions: new[] { region });

        var report = doc.BeginRemediation().Use(ruleSet).DryRun();

        var texts = report.Claims.Where(x => x.RuleId == "tag-bill-to").Select(x => x.Candidates[0].Text).ToArray();
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
            Granularity.Line);
        var ruleSet = new RuleSet("common", new[] { rule }, Array.Empty<RemediationAnchor>(), tolerancedZones: new[] { zone });

        var report = doc.BeginRemediation().Use(ruleSet).DryRun();

        var claims = report.Claims.Where(x => x.RuleId == "artifact-footer").ToArray();
        Assert.Equal(2, claims.Length);
        Assert.All(claims, x => Assert.Equal(RemediationActionKind.Artifact, x.ActionKind));
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
            Granularity.Line);
        var second = new Rule(
            "tag-second-line-item",
            RemediationActions.Tag("P"),
            Predicates.Flow.NthIn("line-items", 1),
            Granularity.Line);

        var report = doc.BeginRemediation()
            .Use(new RuleSet("invoice", new[] { first, second }, anchors, flowRegions: new[] { region }))
            .DryRun();

        Assert.Equal("Widget 2 10.00", Assert.Single(report.Claims.Where(x => x.RuleId == "tag-first-line-item")).Candidates[0].Text);
        Assert.Equal("Gadget 1 5.00", Assert.Single(report.Claims.Where(x => x.RuleId == "tag-second-line-item")).Candidates[0].Text);
    }
}
