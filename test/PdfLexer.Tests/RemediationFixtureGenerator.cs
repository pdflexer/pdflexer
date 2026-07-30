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
        new("Report", "report-like", CreateReportInput, CreateReportRuleSet),
        new("Form", "form-like", CreateFormInput, CreateFormRuleSet),
        new("Multi-column", "multi-column-sidebar", CreateMultiColumnInput, CreateMultiColumnRuleSet),
        new("Mixed page sizes", "mixed-page-sizes", CreateMixedPageSizeInput, CreateMixedPageSizeRuleSet)
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

    public static GeneratedRemediationFixture GenerateStrictInvoiceUa1()
    {
        lock (GenerateLock)
        {
            Directory.CreateDirectory(FixtureRootPath);
            Directory.CreateDirectory(InputRootPath);
            var blueprint = new RemediationFixtureBlueprint(
                "Strict invoice table",
                "invoice-table-strict",
                CreateStrictInvoiceInput,
                CreateInvoiceRuleSet);
            var input = SaveInput(blueprint);
            return SaveFixture(blueprint, input, PdfUaProfile.PdfUa1);
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
            StrictConformance = true,
            DebugWrite = true,
            LeftoverPolicy = RemediationLeftoverPolicy.AutoArtifact
        };
        using var session = document.BeginRemediation(configuration).Use(blueprint.CreateRuleSet());
        var dryRun = session.DryRun();
        var unsuppressedDifferences = dryRun.TemplateDifferences.Where(x => !x.Suppressed).ToList();
        if (unsuppressedDifferences.Count > 0)
        {
            var absorbed = string.Join(
                Environment.NewLine,
                dryRun.AutoArtifacts.Select(x =>
                    $"auto-artifact page={x.PageIndex + 1} inventory={x.InventoryItemId ?? "<none>"} " +
                    $"bounds={x.BoundingBox} text=\"{x.Text}\""));
            throw new InvalidOperationException(
                string.Join(Environment.NewLine, dryRun.Diagnostics) +
                (absorbed.Length == 0 ? string.Empty : Environment.NewLine + absorbed));
        }
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
                    CandidateSelector.Text(Granularity.Line)),
                new Rule(
                    "invoice-number",
                    RemediationActions.Tag("P"),
                    Predicates.Text.Contains("INV-10042"),
                    CandidateSelector.Text(Granularity.Line)),
                new Rule(
                    "line-item-header-cell",
                    RemediationActions.Tag("Span"),
                    Predicates.Anchor.SameRowAs("line-items-header", tolerance: 4),
                    CandidateSelector.Text(Granularity.Word)),
                new Rule(
                    "line-item-cell",
                    RemediationActions.Tag("Span"),
                    Predicates.Flow.InFlowRegion("line-items"),
                    CandidateSelector.Text(Granularity.Word)),
                new Rule(
                    "line-items-table",
                    RemediationActions.TableOverFlattenedCells(
                        ClaimPredicates.FromRule("line-item-header-cell").Or(ClaimPredicates.FromRule("line-item-cell")),
                        ClaimPredicates.FromRule("line-item-header-cell"),
                        72, 250, 450, 600),
                    stage: Stage.Group),
                BodyParagraphRule()
            },
            new[]
            {
                RemediationAnchor.TextLabel("invoice-label", "Invoice #"),
                RemediationAnchor.TextLabel("line-items-header", "Item"),
                RemediationAnchor.TextLabel("subtotal-label", "Subtotal")
            },
            tolerancedZones: InvoiceZones(),
            flowRegions: new[]
            {
                new FlowRegion("line-items", FlowBoundary.Anchor("line-items-header"), FlowBoundary.Anchor("subtotal-label"))
            },
            structuralTemplate: InvoiceTemplate(),
            artifacts: InvoiceArtifacts());

    private static RuleSet CreateStatementRuleSet() =>
        new(
            "statement-template",
            new[]
            {
                FooterRule(),
                new Rule("statement-title", RemediationActions.Tag("H1"), Predicates.Text.StartsWith("Account Statement"), CandidateSelector.Text(Granularity.Line)),
                new Rule(
                    "bill-to-address",
                    RemediationActions.Tag("P"),
                    Predicates.Flow.InFlowRegion("bill-to-address"),
                    CandidateSelector.Text(Granularity.Line)),
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
            },
            structuralTemplate: StatementTemplate(),
            artifacts: FooterArtifacts());

    private static RuleSet CreateReportRuleSet() =>
        CreateGenericRuleSet("report-template", ReportTemplate());

    private static RuleSet CreateFormRuleSet() =>
        CreateGenericRuleSet("form-template", FormTemplate());

    private static RuleSet CreateMultiColumnRuleSet() =>
        CreateGenericRuleSet("multi-column-template", MultiColumnTemplate());

    private static RuleSet CreateMixedPageSizeRuleSet() =>
        CreateGenericRuleSet("mixed-page-sizes-template", MixedPageSizeTemplate());

    private static RuleSet CreateGenericRuleSet(
        string id,
        RemediationStructuralTemplate structuralTemplate) =>
        new(
            id,
            new[]
            {
                FooterRule(),
                new Rule("title", RemediationActions.Tag("H1"), Predicates.Font.Size(NumericOperator.GreaterThanOrEqual, 16), CandidateSelector.Text(Granularity.Line)),
                BodyParagraphRule()
            },
            Array.Empty<RemediationAnchor>(),
            tolerancedZones: FooterZones(),
            structuralTemplate: structuralTemplate,
            artifacts: FooterArtifacts());

    private static Rule FooterRule() =>
        new("footer", RemediationActions.Artifact(ArtifactSubtype.Pagination), Predicates.Flow.InZone("footer"), CandidateSelector.Text(Granularity.Line));

    private static Rule BodyParagraphRule() =>
        new("body-line", RemediationActions.Tag("P"), RemediationPredicate.Always, CandidateSelector.Text(Granularity.Line));

    private static TolerancedZone[] FooterZones() =>
        new[] { new TolerancedZone("footer", LayoutCoord.MarginRelative(bottom: 42), Tolerance: 6) };

    private static TolerancedZone[] InvoiceZones() =>
        FooterZones()
            .Append(new TolerancedZone(
                "line-items-header-spacing",
                LayoutCoord.Absolute(new PdfRect<double>(72, 670, 600, 700))))
            .ToArray();

    private static RemediationArtifactInventoryItem[] FooterArtifacts() =>
        new[]
        {
            new RemediationArtifactInventoryItem(
                "page-footer",
                ArtifactSubtype.Pagination,
                pages: PageSelector.Every,
                zoneId: "footer",
                occurrence: AssertionCount.Exactly(1))
        };

    private static RemediationArtifactInventoryItem[] InvoiceArtifacts() =>
        FooterArtifacts()
            .Append(new RemediationArtifactInventoryItem(
                "line-items-header-spacing",
                ArtifactSubtype.Layout,
                pages: PageSelector.Every,
                zoneId: "line-items-header-spacing",
                occurrence: AssertionCount.Exactly(2)))
            .ToArray();

    private static RemediationStructuralTemplate InvoiceTemplate() =>
        Template(
            Node("H1", "invoice-title"),
            Node("P", "invoice-number"),
            Node(
                "Table",
                "line-items-table",
                Node(
                    "TR",
                    "line-items-header-row",
                    Node("TH", "item-header"),
                    Node("TH", "quantity-header"),
                    Node("TH", "amount-header")),
                Node(
                    "TR",
                    "widget-row",
                    Node("TD", "widget-item"),
                    Node("TD", "widget-quantity"),
                    Node("TD", "widget-amount")),
                Node(
                    "TR",
                    "service-row",
                    Node("TD", "service-item"),
                    Node("TD", "service-quantity"),
                    Node("TD", "service-amount"))),
            Node("P", "invoice-summary"));

    private static RemediationStructuralTemplate StatementTemplate() =>
        Template(
            Node("H1", "statement-title"),
            Node("P", "bill-to-label"),
            Node("P", "bill-to-name"),
            Node("P", "bill-to-street"),
            Node("P", "bill-to-city"),
            Node("P", "ship-to-label"),
            Node("P", "ship-to-value"));

    private static RemediationStructuralTemplate ReportTemplate() =>
        Template(
            Node("H1", "report-title"),
            Node("P", "overview-heading"),
            Node("P", "overview-body"),
            Node("P", "optional-notes-heading"),
            Node("P", "optional-notes-body"));

    private static RemediationStructuralTemplate FormTemplate() =>
        Template(
            Node("H1", "form-title"),
            Node("P", "full-name-label"),
            Node("P", "full-name-value"),
            Node("P", "email-label"),
            Node("P", "email-value"),
            Node("P", "notices-agreement"));

    private static RemediationStructuralTemplate MultiColumnTemplate() =>
        Template(
            Node("H1", "policy-title"),
            Node("P", "sidebar-heading"),
            Node("P", "main-column-heading"),
            Node("P", "sidebar-body"),
            Node("P", "main-column-body-1"),
            Node("P", "main-column-body-2"));

    private static RemediationStructuralTemplate MixedPageSizeTemplate() =>
        Template(
            Node("H1", "letter-title", pages: PageSelector.First),
            Node("P", "letter-body", pages: PageSelector.First),
            Node("H1", "a4-title", pages: PageSelector.Last),
            Node("P", "a4-body", pages: PageSelector.Last));

    private static RemediationStructuralTemplate Template(
        params RemediationStructuralTemplateNode[] children) => new(children);

    private static RemediationStructuralTemplateNode Node(
        string tag,
        string id,
        params RemediationStructuralTemplateNode[] children) =>
        new(tag, children, id);

    private static RemediationStructuralTemplateNode Node(
        string tag,
        string id,
        PageSelector pages) =>
        new(tag, id: id, pages: pages);

    private static PdfDocument CreateInvoiceInput() =>
        CreateInvoiceInput(CreateEmbeddedFont());

    private static PdfDocument CreateStrictInvoiceInput()
        => CreateInvoiceInput(CreateEmbeddedFont());

    private static PdfDocument CreateInvoiceInput(IWritableFont font)
    {
        var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        using var writer = page.GetWriter();
        WriteLine(writer, font, 40, 750, "Invoice 10042", 18);
        WriteLine(writer, font, 40, 720, "Invoice #");
        WriteLine(writer, font, 150, 720, "INV-10042");
        writer.Save()
            .Font(font, 12)
            .WordSpacing(180)
            .TextMove(90, 680)
            .Text("Item Qty Amount")
            .Restore();
        WriteLine(writer, font, 90, 658, "Widget");
        WriteLine(writer, font, 300, 658, "2");
        WriteLine(writer, font, 500, 658, "10.00");
        WriteLine(writer, font, 90, 636, "Service");
        WriteLine(writer, font, 300, 636, "1");
        WriteLine(writer, font, 500, 636, "90.00");
        WriteLine(writer, font, 40, 600, "Subtotal");
        WriteLine(writer, font, 150, 600, "100.00");
        WriteLine(writer, font, 520, 24, "Page 1");
        return doc;
    }

    private static PdfDocument CreateStatementInput()
    {
        var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        var font = CreateEmbeddedFont();
        using var writer = page.GetWriter();
        WriteLine(writer, font, 40, 750, "Account Statement", 18);
        WriteLine(writer, font, 40, 710, "Bill To");
        WriteLine(writer, font, 40, 688, "Ada Lovelace");
        WriteLine(writer, font, 40, 666, "123 Analytical Engine Way");
        WriteLine(writer, font, 40, 644, "London");
        WriteLine(writer, font, 40, 610, "Ship To");
        WriteLine(writer, font, 40, 588, "Same as billing");
        WriteLine(writer, font, 520, 24, "Page 1");
        return doc;
    }

    private static PdfDocument CreateReportInput()
    {
        var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        var font = CreateEmbeddedFont();
        using var writer = page.GetWriter();
        WriteLine(writer, font, 40, 750, "Quarterly Report", 18);
        WriteLine(writer, font, 40, 712, "Overview");
        WriteLine(writer, font, 40, 690, "Revenue increased across all regions.");
        WriteLine(writer, font, 40, 660, "Optional Notes");
        WriteLine(writer, font, 40, 638, "No remediation exceptions were observed.");
        WriteLine(writer, font, 520, 24, "Page 1");
        return doc;
    }

    private static PdfDocument CreateFormInput()
    {
        var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        var font = CreateEmbeddedFont();
        using var writer = page.GetWriter();
        WriteLine(writer, font, 40, 750, "Registration Form", 18);
        WriteLine(writer, font, 40, 710, "Full name");
        WriteLine(writer, font, 180, 710, "Ada Lovelace");
        WriteLine(writer, font, 40, 680, "Email");
        WriteLine(writer, font, 180, 680, "ada@example.com");
        WriteLine(writer, font, 40, 640, "I agree to receive notices.");
        WriteLine(writer, font, 520, 24, "Page 1");
        return doc;
    }

    private static PdfDocument CreateMultiColumnInput()
    {
        var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        var font = CreateEmbeddedFont();
        using var writer = page.GetWriter();
        WriteLine(writer, font, 40, 750, "Policy Update", 18);
        WriteLine(writer, font, 40, 710, "Sidebar");
        WriteLine(writer, font, 40, 688, "Important dates");
        WriteLine(writer, font, 220, 710, "Main Column");
        WriteLine(writer, font, 220, 688, "The updated policy applies next quarter.");
        WriteLine(writer, font, 220, 666, "Review the summary before filing.");
        WriteLine(writer, font, 520, 24, "Page 1");
        return doc;
    }

    private static PdfDocument CreateMixedPageSizeInput()
    {
        var doc = PdfDocument.Create();
        var font = CreateEmbeddedFont();
        var letter = doc.AddPage(PageSize.LETTER);
        using (var writer = letter.GetWriter())
        {
            WriteLine(writer, font, 40, 750, "Mixed Size Notice", 18);
            WriteLine(writer, font, 40, 710, "Letter page content");
            WriteLine(writer, font, 520, 24, "Page 1");
        }

        var a4 = doc.AddPage(PageSize.A4);
        using (var writer = a4.GetWriter())
        {
            WriteLine(writer, font, 40, 790, "Continuation", 18);
            WriteLine(writer, font, 40, 750, "A4 page content");
            WriteLine(writer, font, 500, 24, "Page 2");
        }

        return doc;
    }

    private static IWritableFont CreateEmbeddedFont()
    {
        var testDir = PathUtil.GetPathFromSegmentOfCurrent("test");
        var fontPath = Path.Combine(testDir, "Roboto-Regular.ttf");
        return TrueTypeFont.CreateWritableFont(File.ReadAllBytes(fontPath));
    }

    private static void WriteLine(
        ContentWriter<double> writer,
        IWritableFont font,
        double x,
        double y,
        string text,
        double size = 12)
    {
        writer.Save().Font(font, size).TextMove(x, y).Text(text).Restore();
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
