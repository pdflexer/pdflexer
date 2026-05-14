using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PdfLexer.Content;
using PdfLexer.DOM;
using PdfLexer.Fonts;
using PdfLexer.Remediation;
using PdfLexer.Writing;

namespace PdfLexer.Tests;

internal static class RemediationFixtureGenerator
{
    private static readonly object GenerateLock = new();
    private static readonly RemediationFixtureBlueprint[] Blueprints =
    {
        new("Invoice", "invoice-like", CreateInvoiceInput, CreateInvoiceRuleSet),
        new("Statement", "statement-like", CreateStatementInput, CreateStatementRuleSet),
        new("Report", "report-like", CreateReportInput, CreateGenericRuleSet),
        new("Form", "form-like", CreateFormInput, CreateGenericRuleSet),
        new("Multi-column", "multi-column-sidebar", CreateMultiColumnInput, CreateGenericRuleSet),
        new("Mixed page sizes", "mixed-page-sizes", CreateMixedPageSizeInput, CreateGenericRuleSet)
    };

    public static string FixtureRootPath
    {
        get
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            return Path.Combine(tp, "results", "accessibility-fixtures", "remediation");
        }
    }

    public static string InputRootPath => Path.Combine(FixtureRootPath, "input");

    public static string GetFixtureFileName(string baseName, PdfUaProfile profile) =>
        $"{baseName}-{(profile == PdfUaProfile.PdfUa1 ? "ua1" : "ua2")}.pdf";

    public static IReadOnlyList<GeneratedRemediationFixture> GenerateAll()
    {
        lock (GenerateLock)
        {
            Directory.CreateDirectory(FixtureRootPath);
            Directory.CreateDirectory(InputRootPath);
            var results = new List<GeneratedRemediationFixture>(Blueprints.Length * 2);
            foreach (var blueprint in Blueprints)
            {
                var input = SaveInput(blueprint);
                results.Add(SaveFixture(blueprint, input, PdfUaProfile.PdfUa1));
                results.Add(SaveFixture(blueprint, input, PdfUaProfile.PdfUa2));
            }

            return results;
        }
    }

    private static GeneratedRemediationInput SaveInput(RemediationFixtureBlueprint blueprint)
    {
        using var document = blueprint.CreateInput();
        var fileName = $"{blueprint.BaseName}-input.pdf";
        var path = Path.Combine(InputRootPath, fileName);
        var bytes = document.Save();
        File.WriteAllBytes(path, bytes);
        return new GeneratedRemediationInput(fileName, path, bytes);
    }

    private static GeneratedRemediationFixture SaveFixture(
        RemediationFixtureBlueprint blueprint,
        GeneratedRemediationInput input,
        PdfUaProfile profile)
    {
        using var document = PdfDocument.Open(input.Bytes);
        var configuration = new RemediationSessionConfiguration
        {
            Language = "en-US",
            Title = $"Remediated {blueprint.Name}",
            Profile = profile,
            StrictConformance = false,
            DebugWrite = true,
            LeftoverPolicy = RemediationLeftoverPolicy.AutoArtifact
        };
        using var session = document.BeginRemediation(configuration).Use(blueprint.CreateRuleSet());
        var report = session.Commit();
        var fileName = GetFixtureFileName(blueprint.BaseName, profile);
        var path = Path.Combine(FixtureRootPath, fileName);
        var bytes = document.Save();
        File.WriteAllBytes(path, bytes);
        return new GeneratedRemediationFixture(blueprint.Name, fileName, path, input.Path, profile, bytes, report);
    }

    private static RuleSet CreateInvoiceRuleSet() =>
        new(
            "invoice-template",
            new[]
            {
                FooterRule(),
                new Rule(
                    "invoice-title",
                    RemediationActions.Tag("H1"),
                    Predicates.Font.Size(NumericOperator.GreaterThanOrEqual, 16),
                    Granularity.Line),
                new Rule(
                    "invoice-number",
                    RemediationActions.Tag("P"),
                    Predicates.Text.Contains("INV-10042"),
                    Granularity.Line),
                new Rule(
                    "line-item-row",
                    RemediationActions.Tag("P"),
                    Predicates.Flow.InFlowRegion("line-items"),
                    Granularity.Line),
                BodyParagraphRule()
            },
            new[]
            {
                RemediationAnchor.TextLabel("invoice-label", "Invoice #"),
                RemediationAnchor.TableHeader("line-items-header", "Item", "Qty", "Amount"),
                RemediationAnchor.TextLabel("subtotal-label", "Subtotal")
            },
            tolerancedZones: FooterZones(),
            flowRegions: new[]
            {
                new FlowRegion("line-items", FlowBoundary.Anchor("line-items-header"), FlowBoundary.Anchor("subtotal-label"))
            });

    private static RuleSet CreateStatementRuleSet() =>
        new(
            "statement-template",
            new[]
            {
                FooterRule(),
                new Rule("statement-title", RemediationActions.Tag("H1"), Predicates.Text.StartsWith("Account Statement"), Granularity.Line),
                new Rule(
                    "bill-to-address",
                    RemediationActions.Tag("P"),
                    Predicates.Flow.InFlowRegion("bill-to-address"),
                    Granularity.Line),
                BodyParagraphRule()
            },
            new[]
            {
                RemediationAnchor.TextLabel("bill-to-label", "Bill To"),
                RemediationAnchor.TextLabel("ship-to-label", "Ship To")
            },
            tolerancedZones: FooterZones(),
            flowRegions: new[]
            {
                new FlowRegion("bill-to-address", FlowBoundary.Anchor("bill-to-label"), FlowBoundary.Anchor("ship-to-label"))
            });

    private static RuleSet CreateGenericRuleSet() =>
        new(
            "generic-template",
            new[]
            {
                FooterRule(),
                new Rule("title", RemediationActions.Tag("H1"), Predicates.Font.Size(NumericOperator.GreaterThanOrEqual, 16), Granularity.Line),
                BodyParagraphRule()
            },
            Array.Empty<RemediationAnchor>(),
            tolerancedZones: FooterZones());

    private static Rule FooterRule() =>
        new("footer", RemediationActions.Artifact(ArtifactSubtype.Pagination), Predicates.Flow.InZone("footer"), Granularity.Line);

    private static Rule BodyParagraphRule() =>
        new("body-line", RemediationActions.Tag("P"), RemediationPredicate.Always, Granularity.Line);

    private static TolerancedZone[] FooterZones() =>
        new[] { new TolerancedZone("footer", LayoutCoord.MarginRelative(bottom: 42), Tolerance: 6) };

    private static PdfDocument CreateInvoiceInput()
    {
        var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        using var writer = page.GetWriter();
        WriteLine(writer, 40, 750, "Invoice 10042", 18);
        WriteLine(writer, 40, 720, "Invoice #");
        WriteLine(writer, 150, 720, "INV-10042");
        WriteLine(writer, 40, 680, "Item Qty Amount");
        WriteLine(writer, 40, 658, "Widget 2 10.00");
        WriteLine(writer, 40, 636, "Service 1 90.00");
        WriteLine(writer, 40, 600, "Subtotal");
        WriteLine(writer, 150, 600, "100.00");
        WriteLine(writer, 520, 24, "Page 1");
        return doc;
    }

    private static PdfDocument CreateStatementInput()
    {
        var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        using var writer = page.GetWriter();
        WriteLine(writer, 40, 750, "Account Statement", 18);
        WriteLine(writer, 40, 710, "Bill To");
        WriteLine(writer, 40, 688, "Ada Lovelace");
        WriteLine(writer, 40, 666, "123 Analytical Engine Way");
        WriteLine(writer, 40, 644, "London");
        WriteLine(writer, 40, 610, "Ship To");
        WriteLine(writer, 40, 588, "Same as billing");
        WriteLine(writer, 520, 24, "Page 1");
        return doc;
    }

    private static PdfDocument CreateReportInput()
    {
        var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        using var writer = page.GetWriter();
        WriteLine(writer, 40, 750, "Quarterly Report", 18);
        WriteLine(writer, 40, 712, "Overview");
        WriteLine(writer, 40, 690, "Revenue increased across all regions.");
        WriteLine(writer, 40, 660, "Optional Notes");
        WriteLine(writer, 40, 638, "No remediation exceptions were observed.");
        WriteLine(writer, 520, 24, "Page 1");
        return doc;
    }

    private static PdfDocument CreateFormInput()
    {
        var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        using var writer = page.GetWriter();
        WriteLine(writer, 40, 750, "Registration Form", 18);
        WriteLine(writer, 40, 710, "Full name");
        WriteLine(writer, 180, 710, "Ada Lovelace");
        WriteLine(writer, 40, 680, "Email");
        WriteLine(writer, 180, 680, "ada@example.com");
        WriteLine(writer, 40, 640, "I agree to receive notices.");
        WriteLine(writer, 520, 24, "Page 1");
        return doc;
    }

    private static PdfDocument CreateMultiColumnInput()
    {
        var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        using var writer = page.GetWriter();
        WriteLine(writer, 40, 750, "Policy Update", 18);
        WriteLine(writer, 40, 710, "Sidebar");
        WriteLine(writer, 40, 688, "Important dates");
        WriteLine(writer, 220, 710, "Main Column");
        WriteLine(writer, 220, 688, "The updated policy applies next quarter.");
        WriteLine(writer, 220, 666, "Review the summary before filing.");
        WriteLine(writer, 520, 24, "Page 1");
        return doc;
    }

    private static PdfDocument CreateMixedPageSizeInput()
    {
        var doc = PdfDocument.Create();
        var letter = doc.AddPage(PageSize.LETTER);
        using (var writer = letter.GetWriter())
        {
            WriteLine(writer, 40, 750, "Mixed Size Notice", 18);
            WriteLine(writer, 40, 710, "Letter page content");
            WriteLine(writer, 520, 24, "Page 1");
        }

        var a4 = doc.AddPage(PageSize.A4);
        using (var writer = a4.GetWriter())
        {
            WriteLine(writer, 40, 790, "Continuation", 18);
            WriteLine(writer, 40, 750, "A4 page content");
            WriteLine(writer, 500, 24, "Page 2");
        }

        return doc;
    }

    private static void WriteLine(ContentWriter<double> writer, double x, double y, string text, double size = 12)
    {
        writer.Save().Font(Base14.Helvetica, size).TextMove(x, y).Text(text).Restore();
    }
}

internal sealed record GeneratedRemediationInput(string FileName, string Path, byte[] Bytes);

internal sealed record GeneratedRemediationFixture(
    string Name,
    string FileName,
    string Path,
    string InputPath,
    PdfUaProfile Profile,
    byte[] Bytes,
    RemediationReport Report);

internal sealed record RemediationFixtureBlueprint(
    string Name,
    string BaseName,
    Func<PdfDocument> CreateInput,
    Func<RuleSet> CreateRuleSet);
