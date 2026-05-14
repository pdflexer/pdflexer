using PdfLexer.DOM;
using PdfLexer.Fonts;
using PdfLexer.Remediation;
using PdfLexer.Writing;
using System;
using System.Linq;
using Xunit;

namespace PdfLexer.Tests;

public class RemediationAnchorTests
{
    private PdfDocument CreateInvoiceDocument()
    {
        var doc = PdfDocument.Create();
        var page = doc.AddPage();
        using var writer = page.GetWriter();
        
        // Add "Invoice" label
        writer.Save().Font(Base14.Helvetica, 12).TextMove(100, 700).Text("Invoice").Restore();
           
        // Add invoice value to the right
        writer.Save().Font(Base14.Helvetica, 12).TextMove(200, 700).Text("INV-10042").Restore();

        // Add a "Total" label
        writer.Save().Font(Base14.Helvetica, 12).TextMove(100, 600).Text("Total").Restore();
           
        // Add another "Total" label somewhere else to cause ambiguity
        writer.Save().Font(Base14.Helvetica, 12).TextMove(100, 500).Text("Total").Restore();
           
        // Add total value to the right of first total
        writer.Save().Font(Base14.Helvetica, 12).TextMove(200, 600).Text("$500.00").Restore();

        return doc;
    }

    [Fact]
    public void Resolves_Invoice_Number_Value_To_Right_Of_Label()
    {
        using var doc = CreateInvoiceDocument();
        
        var invoiceLabelAnchor = RemediationAnchor.TextLabel("invoice-lbl", "Invoice", StringComparison.Ordinal);
        
        var rule = new Rule(
            id: "tag-invoice-num",
            stage: Stage.Classify,
            granularity: Granularity.Word,
            predicate: new AnchorRelativeRemediationPredicate(AnchorRelativePredicateKind.RightOf, "invoice-lbl", Tolerance: 5.0).And(
                new AnchorRelativeRemediationPredicate(AnchorRelativePredicateKind.SameRowAs, "invoice-lbl", Tolerance: 5.0)
            ),
            action: new TagRemediationAction(new PdfName("Span"))
        );
        
        var ruleSet = new RuleSet("rs-1", new[] { rule }, new[] { (RemediationAnchor)invoiceLabelAnchor });
        
        var session = doc.BeginRemediation(new RemediationSessionConfiguration()).Use(ruleSet);
        var report = session.DryRun();
        
        var claims = report.Claims.Where(x => x.RuleId == "tag-invoice-num").ToList();
        Assert.Single(claims);
        
        var candidate = claims[0].Candidates[0];
        Assert.Equal("INV-10042", candidate.Text);
    }

    [Fact]
    public void Ambiguous_Repeated_Label_Reports_Diagnostic()
    {
        using var doc = CreateInvoiceDocument();
        
        var totalLabelAnchor = RemediationAnchor.TextLabel("total-lbl", "Total", StringComparison.Ordinal);
        
        var rule = new Rule(
            id: "tag-total",
            stage: Stage.Classify,
            predicate: new AnchorRelativeRemediationPredicate(AnchorRelativePredicateKind.RightOf, "total-lbl", Tolerance: 5.0),
            action: new TagRemediationAction(new PdfName("Span"))
        );
        
        var ruleSet = new RuleSet("rs-1", new[] { rule }, new[] { (RemediationAnchor)totalLabelAnchor });
        
        var session = doc.BeginRemediation(new RemediationSessionConfiguration()).Use(ruleSet);
        var report = session.DryRun();
        
        Assert.Contains(report.Diagnostics, d => d.Contains("Ambiguous") || d.Contains("ambiguous"));
        
        var claims = report.Claims.Where(x => x.RuleId == "tag-total").ToList();
        Assert.Empty(claims); // Ambiguous anchor should fail resolution, leading to no matches
    }

    [Fact]
    public void Same_Row_Column_Selectors_Tolerate_Drift()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        {
            using var writer = page.GetWriter();
            
            // Label at y=400
            writer.Save().Font(Base14.Helvetica, 12).TextMove(100, 400).Text("Price").Restore();
            
            // Value slightly drifted down at y=398 (within 5pt tolerance)
            writer.Save().Font(Base14.Helvetica, 12).TextMove(200, 398).Text("Ten").Restore();
        }
           
        var labelAnchor = RemediationAnchor.TextLabel("price-lbl", "Price", StringComparison.Ordinal);
        
        var rule = new Rule(
            id: "tag-price",
            stage: Stage.Classify,
            granularity: Granularity.Word,
            predicate: new AnchorRelativeRemediationPredicate(AnchorRelativePredicateKind.RightOf, "price-lbl").And(
                new AnchorRelativeRemediationPredicate(AnchorRelativePredicateKind.SameRowAs, "price-lbl", Tolerance: 5.0)
            ),
            action: new TagRemediationAction(new PdfName("Span"))
        );
        
        var ruleSet = new RuleSet("rs-1", new[] { rule }, new[] { (RemediationAnchor)labelAnchor });
        
        var session = doc.BeginRemediation(new RemediationSessionConfiguration()).Use(ruleSet);
        var report = session.DryRun();
        
        var claims = report.Claims.Where(x => x.RuleId == "tag-price").ToList();
        if (claims.Count == 0)
        {
            throw new Exception($"No claims found! Diags: {string.Join(", ", report.Diagnostics)}");
        }
        Assert.Single(claims);
        
        var candidate = claims[0].Candidates[0];
        Assert.Equal("Ten", candidate.Text);
    }

    [Fact]
    public void Occurrence_Disambiguator_Selects_Repeated_Label()
    {
        using var doc = CreateInvoiceDocument();

        var totalLabelAnchor = RemediationAnchor.TextLabel("total-lbl", "Total", StringComparison.Ordinal) with
        {
            Occurrence = 1
        };

        var rule = new Rule(
            id: "tag-total",
            stage: Stage.Classify,
            granularity: Granularity.Word,
            predicate: new AnchorRelativeRemediationPredicate(AnchorRelativePredicateKind.RightOf, "total-lbl", Tolerance: 5.0).And(
                new AnchorRelativeRemediationPredicate(AnchorRelativePredicateKind.SameRowAs, "total-lbl", Tolerance: 5.0)
            ),
            action: new TagRemediationAction(new PdfName("Span"))
        );

        var ruleSet = new RuleSet("rs-1", new[] { rule }, new[] { totalLabelAnchor });
        var session = doc.BeginRemediation(new RemediationSessionConfiguration()).Use(ruleSet);
        var report = session.DryRun();

        Assert.DoesNotContain(report.Diagnostics, d => d.Contains("ambiguous", StringComparison.OrdinalIgnoreCase));
        var claim = Assert.Single(report.Claims.Where(x => x.RuleId == "tag-total"));
        Assert.Equal("$500.00", claim.Candidates[0].Text);
    }

    [Fact]
    public void Table_Header_Anchor_Resolves_Line_Containing_All_Headers()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        using (var writer = page.GetWriter())
        {
            writer.Save().Font(Base14.Helvetica, 12).TextMove(100, 700).Text("Item Qty Amount").Restore();
            writer.Save().Font(Base14.Helvetica, 12).TextMove(100, 680).Text("Widget 2 10.00").Restore();
        }

        var headerAnchor = RemediationAnchor.TableHeader("line-items-header", "Item", "Qty", "Amount");
        var rule = new Rule(
            id: "tag-row",
            stage: Stage.Classify,
            granularity: Granularity.Line,
            predicate: new AnchorRelativeRemediationPredicate(AnchorRelativePredicateKind.Below, "line-items-header", Tolerance: 2.0),
            action: new TagRemediationAction(new PdfName("TR"))
        );

        var ruleSet = new RuleSet("rs-1", new[] { rule }, new[] { headerAnchor });
        var session = doc.BeginRemediation(new RemediationSessionConfiguration()).Use(ruleSet);
        var report = session.DryRun();

        var claim = Assert.Single(report.Claims.Where(x => x.RuleId == "tag-row"));
        Assert.Equal("Widget 2 10.00", claim.Candidates[0].Text);
    }

    [Fact]
    public void Predicate_Anchor_Combines_Text_And_Region_Predicates()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
        {
            writer.Save().Font(Base14.Helvetica, 12).TextMove(100, 740).Text("Date").Restore();
            writer.Save().Font(Base14.Helvetica, 12).TextMove(200, 740).Text("2026-05-04").Restore();
            writer.Save().Font(Base14.Helvetica, 12).TextMove(100, 400).Text("Date").Restore();
            writer.Save().Font(Base14.Helvetica, 12).TextMove(200, 400).Text("Body Date").Restore();
        }

        var dateAnchor = RemediationAnchor.Selector(
            "date-label",
            Granularity.Word,
            Predicates.Text.Equals("Date").And(
                Predicates.Geo.In(LayoutCoord.MarginRelative(top: 90))),
            AnchorSelection.RequiredSingle);
        var rule = new Rule(
            "tag-date-value",
            RemediationActions.Tag("Span"),
            Predicates.Anchor.RightOf("date-label", tolerance: 2, maxDistance: 160).And(
                Predicates.Anchor.SameRowAs("date-label", tolerance: 4)),
            Granularity.Word);

        var report = doc.BeginRemediation()
            .Use(new RuleSet("rs-1", new[] { rule }, new[] { dateAnchor }))
            .DryRun();

        var claim = Assert.Single(report.Claims.Where(x => x.RuleId == "tag-date-value"));
        Assert.Equal("2026-05-04", claim.Candidates[0].Text);
    }

    [Fact]
    public void Predicate_Anchor_Can_Be_Relative_To_Another_Anchor()
    {
        using var doc = CreateInvoiceDocument();
        var anchors = new[]
        {
            RemediationAnchor.TextLabel("invoice-lbl", "Invoice", StringComparison.Ordinal),
            RemediationAnchor.Selector(
                "invoice-number-anchor",
                Granularity.Word,
                Predicates.Text.Matches(@"^INV-\d+$").And(
                    Predicates.Anchor.RightOf("invoice-lbl", tolerance: 5, maxDistance: 160)).And(
                    Predicates.Anchor.SameRowAs("invoice-lbl", tolerance: 5)),
                AnchorSelection.RequiredSingle)
        };
        var rule = new Rule(
            "tag-invoice-num",
            RemediationActions.Tag("Span"),
            Predicates.Geo.In(LayoutCoord.NamedAnchor("invoice-number-anchor", LayoutCoordExpansion.Inflate(1))),
            Granularity.Word);

        var report = doc.BeginRemediation()
            .Use(new RuleSet("rs-1", new[] { rule }, anchors))
            .DryRun();

        var claim = Assert.Single(report.Claims.Where(x => x.RuleId == "tag-invoice-num"));
        Assert.Equal("INV-10042", claim.Candidates[0].Text);
    }

    [Fact]
    public void Flow_FirstAfter_Selects_First_Filtered_Candidate_After_Anchor()
    {
        using var doc = CreateInvoiceDocument();
        var anchor = RemediationAnchor.TextLabel("invoice-lbl", "Invoice", StringComparison.Ordinal);
        var rule = new Rule(
            "tag-first-invoice",
            RemediationActions.Tag("Span"),
            Predicates.Flow.FirstAfter("invoice-lbl", Predicates.Text.Matches(@"^INV-\d+$")),
            Granularity.Word);

        var report = doc.BeginRemediation()
            .Use(new RuleSet("rs-1", new[] { rule }, new[] { anchor }))
            .DryRun();

        var claim = Assert.Single(report.Claims.Where(x => x.RuleId == "tag-first-invoice"));
        Assert.Equal("INV-10042", claim.Candidates[0].Text);
    }
}
