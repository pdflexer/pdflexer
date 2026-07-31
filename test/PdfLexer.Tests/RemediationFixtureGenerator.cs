using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PdfLexer.Content;
using PdfLexer.DOM;
using PdfLexer.Fonts;
using PdfLexer.Remediation;
using PdfLexer.Writing;
using PdfLexer.Operators;

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
        new("Mixed page sizes", "mixed-page-sizes", CreateMixedPageSizeInput, CreateMixedPageSizeRuleSet),
        new("C-01 Unicode and normalization", "c-01-unicode-normalization", CreateC01Input, CreateC01RuleSet),
        new("C-02 Operator fragmentation", "c-02-operator-fragmentation", CreateC02Input, CreateC02RuleSet),
        new("C-03 Graphical content", "c-03-graphical-content", CreateC03Input, CreateC03RuleSet),
        new("C-08 Existing annotations", "c-08-existing-annotations", CreateC08Input, CreateC08RuleSet),
        new("C-10 Running furniture", "c-10-running-furniture", CreateC10Input, CreateC10RuleSet),
        new("C-12 Multi-page continuous table", "c-12-continued-table", CreateC12Input, CreateC12RuleSet),
        new("C-23 Label value pairs", "c-23-label-value-pairs", CreateC23Input, CreateC23RuleSet),
        new("C-29-a Nested list composition", "c-29-a-nested-list", CreateC29aInput, CreateC29aRuleSet),
        new("C-29-b Nested section composition", "c-29-b-nested-sections", CreateC29bInput, CreateC29bRuleSet),
        new("C-30 Prescriptive repeated sections", "c-30-prescriptive-repeated-sections", CreateC30Input, CreateC30RuleSet),
        new(
            "C-04-a Leftover accounting (Flag)",
            "c-04-a-flag",
            CreateC04Input,
            CreateC04RuleSetWithoutInventory,
            FixtureOutcome.DiagnosesOnly,
            RemediationLeftoverPolicy.Flag),
        new(
            "C-04-b Leftover accounting (FailFast)",
            "c-04-b-failfast",
            CreateC04Input,
            CreateC04RuleSetWithoutInventory,
            FixtureOutcome.DiagnosesOnly,
            RemediationLeftoverPolicy.FailFast),
        new(
            "C-04-c Leftover accounting (AutoArtifact)",
            "c-04-c-autoartifact",
            CreateC04Input,
            CreateC04RuleSetWithoutInventory),
        new(
            "C-04-d Leftover accounting (inventory)",
            "c-04-d-inventory",
            CreateC04Input,
            CreateC04RuleSet,
            FixtureOutcome.DiagnosesOnly),
        new(
            "C-05 Semantic assertion",
            "c-05-semantic-assertion",
            CreateC05Input,
            CreateC05RuleSet,
            FixtureOutcome.CommitsWithFailedAssertions),
        new("C-06-a Template drift baseline", "c-06-a-template-drift", CreateC06aInput, CreateC06aRuleSet),
        new(
            "C-06-b Template drift variant",
            "c-06-b-template-drift",
            CreateC06bInput,
            CreateC06aRuleSet,
            FixtureOutcome.DiagnosesOnly),
        new(
            "C-24 Ambiguous and repeated anchors",
            "c-24-ambiguous-anchors",
            CreateC24Input,
            CreateC24RuleSet,
            FixtureOutcome.DiagnosesOnly)
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

    private static IReadOnlyList<GeneratedRemediationFixture>? _cached;

    /// <summary>
    /// Generates the corpus once per process. Every fixture assertion in the suite calls this, so
    /// regenerating on each call rewrote the whole corpus dozens of times per run.
    /// </summary>
    public static IReadOnlyList<GeneratedRemediationFixture> GenerateAll()
    {
        lock (GenerateLock)
        {
            return _cached ??= GenerateAllCore();
        }
    }

    /// <summary>Regenerates unconditionally. Used to prove byte-identical regeneration.</summary>
    public static IReadOnlyList<GeneratedRemediationFixture> GenerateAllUncached()
    {
        lock (GenerateLock)
        {
            return GenerateAllCore();
        }
    }

    private static IReadOnlyList<GeneratedRemediationFixture> GenerateAllCore()
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
            LeftoverPolicy = blueprint.LeftoverPolicy ?? RemediationLeftoverPolicy.AutoArtifact,
            // The failed assertion is the point of C-05, but it also blocks commit, and without
            // committed bytes there is nothing to hand veraPDF. Suppressing it is what lets the fixture
            // demonstrate its actual claim: a document that passes external validation while its
            // semantics are wrong.
            DiagnosticStrictness = blueprint.Outcome == FixtureOutcome.CommitsWithFailedAssertions
                ? RemediationDiagnosticStrictness.Permissive
                : RemediationDiagnosticStrictness.Strict
        };
        using var session = document.BeginRemediation(configuration).Use(blueprint.CreateRuleSet());
        if (blueprint.Outcome == FixtureOutcome.CommitsWithFailedAssertions)
        {
            session.Suppress(
                DiagnosticCode.SemanticAssertionFailed,
                "*",
                "C-05 ships the semantically wrong output on purpose so veraPDF can be run against it.");
        }

        var dryRun = session.DryRun();

        // A diagnosing fixture exists to produce diagnostics, so it is captured from the dry run and
        // never committed. Only its input is written; there is no remediated output to gate.
        if (blueprint.Outcome == FixtureOutcome.DiagnosesOnly)
        {
            return new GeneratedRemediationFixture(
                blueprint.Name,
                GetFixtureFileName(blueprint.BaseName, profile),
                null,
                input.Path,
                profile,
                Array.Empty<byte>(),
                dryRun,
                blueprint.Outcome);
        }

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
        RemediationReport report;
        try
        {
            report = session.Commit();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Fixture '{blueprint.BaseName}' failed to commit.", ex);
        }

        // C-05 commits a conformant document whose semantics are wrong on purpose; a failed assertion
        // is the expected outcome there and a defect anywhere else.
        var failedAssertions = report.AssertionOutcomes.Where(x => !x.Passed).ToList();
        var expectFailures = blueprint.Outcome == FixtureOutcome.CommitsWithFailedAssertions;
        if (failedAssertions.Count > 0 != expectFailures)
        {
            throw new InvalidOperationException(
                $"Fixture '{blueprint.BaseName}' expected {(expectFailures ? "failed" : "no failed")} " +
                $"assertions but observed {failedAssertions.Count}.");
        }

        var fileName = GetFixtureFileName(blueprint.BaseName, profile);
        var path = Path.Combine(FixtureRootPath, fileName);
        var bytes = document.Save();
        File.WriteAllBytes(path, bytes);
        return new GeneratedRemediationFixture(
            blueprint.Name, fileName, path, input.Path, profile, bytes, report, blueprint.Outcome);
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

    private static IWritableFont CreateUnicodeEmbeddedFont()
    {
        var testDir = PathUtil.GetPathFromSegmentOfCurrent("test");
        var fontPath = Path.Combine(testDir, "Roboto-Regular.ttf");
        return TrueTypeFont.CreateType0WritableFont(File.ReadAllBytes(fontPath));
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

    #region Tier 0 Fixture Builders

    // C-01: Unicode and normalization
    internal static PdfDocument CreateC01Input()
    {
        var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        var font = CreateUnicodeEmbeddedFont();
        using var writer = page.GetWriter();

        // Each construct is paired with its normalized equivalent.
        WriteLine(writer, font, 40, 770, "\uFB01 \uFB02");
        WriteLine(writer, font, 40, 750, "fi fl");

        WriteLine(writer, font, 40, 720, "multi\u00ADpart");
        WriteLine(writer, font, 40, 700, "multi-part");

        WriteLine(writer, font, 40, 670, "word\u00A0gap");
        WriteLine(writer, font, 40, 630, "word gap");

        WriteLine(writer, font, 40, 600, "INV\u201110042");
        WriteLine(writer, font, 40, 580, "INV-10042");

        WriteLine(writer, font, 40, 550, "\u201Cquote\u201D \u2018single\u2019");
        WriteLine(writer, font, 40, 530, "\"quote\" 'single'");

        WriteLine(writer, font, 40, 500, "cafe\u0301");
        WriteLine(writer, font, 40, 480, "caf\u00E9");

        WriteLine(writer, font, 40, 450, "three   spaces");

        WriteLine(writer, font, 520, 24, "Page 1");
        return doc;
    }

    /// <summary>
    /// The same document written in plain ASCII. Normalization affects matching only, so this must
    /// remediate to identical structure and identical MCID assignment.
    /// </summary>
    internal static PdfDocument CreateC01AsciiInput()
    {
        var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        var font = CreateUnicodeEmbeddedFont();
        using var writer = page.GetWriter();

        WriteLine(writer, font, 40, 770, "fi fl");
        WriteLine(writer, font, 40, 750, "fi fl");

        WriteLine(writer, font, 40, 720, "multi-part");
        WriteLine(writer, font, 40, 700, "multi-part");

        WriteLine(writer, font, 40, 670, "word gap");
        WriteLine(writer, font, 40, 630, "word gap");

        WriteLine(writer, font, 40, 600, "INV-10042");
        WriteLine(writer, font, 40, 580, "INV-10042");

        WriteLine(writer, font, 40, 550, "\"quote\" 'single'");
        WriteLine(writer, font, 40, 530, "\"quote\" 'single'");

        // Precomposed on both lines: "é" has no ASCII spelling, so the twin drops the decomposed
        // sequence rather than the character.
        WriteLine(writer, font, 40, 500, "café");
        WriteLine(writer, font, 40, 480, "café");

        WriteLine(writer, font, 40, 450, "three spaces");

        WriteLine(writer, font, 520, 24, "Page 1");
        return doc;
    }

    internal static RuleSet CreateC01RuleSet() =>
        new(
            "c-01-rules",
            new[]
            {
                FooterRule(),
                new Rule("c01-ligature", RemediationActions.Tag("P"), Predicates.Text.Contains("fi fl"), CandidateSelector.Text(Granularity.Line)),
                new Rule("c01-softhyphen", RemediationActions.Tag("P"), Predicates.Text.Contains("multi-part"), CandidateSelector.Text(Granularity.Line)),
                new Rule("c01-nbsp", RemediationActions.Tag("P"), Predicates.Text.Contains("word gap"), CandidateSelector.Text(Granularity.Line)),
                new Rule("c01-nonbreak-hyphen", RemediationActions.Tag("P"), Predicates.Text.Contains("INV-10042"), CandidateSelector.Text(Granularity.Line)),
                new Rule("c01-quotes", RemediationActions.Tag("P"), Predicates.Text.Contains("\"quote\" 'single'"), CandidateSelector.Text(Granularity.Line)),
                new Rule("c01-e-accent", RemediationActions.Tag("P"), Predicates.Text.Contains("café"), CandidateSelector.Text(Granularity.Line)),
                new Rule("c01-spaces", RemediationActions.Tag("P"), Predicates.Text.Contains("three spaces"), CandidateSelector.Text(Granularity.Line)),
                // A regex written against the normalized form matches both members of a pair...
                new Rule(
                    "c01-regex-normalized",
                    RemediationActions.Tag("P"),
                    Predicates.Text.Matches(@"^INV-10042$"),
                    CandidateSelector.Text(Granularity.Word)),
                // ...while the same expression against the raw non-breaking-hyphen form matches nothing,
                // and only matches once normalization is switched off for that rule.
                new Rule(
                    "c01-regex-raw",
                    RemediationActions.Tag("P"),
                    Predicates.Text.Matches("^INV‑10042$"),
                    CandidateSelector.Text(Granularity.Word)),
                new Rule(
                    "c01-regex-raw-unnormalized",
                    RemediationActions.Tag("P"),
                    Predicates.Text.Matches("^INV‑10042$"),
                    CandidateSelector.Text(Granularity.Word))
                {
                    TextNormalization = TextNormalizationOptions.None
                }
            },
            Array.Empty<RemediationAnchor>(),
            tolerancedZones: FooterZones(),
            artifacts: FooterArtifacts());

    // C-02: Operator fragmentation
    internal static PdfDocument CreateC02Input()
    {
        var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        var font = CreateEmbeddedFont();
        using var writer = page.GetWriter();

        // 1. One Tj
        WriteLine(writer, font, 40, 750, "Invoice #: INV-12345");

        // 2. TJ array with word gap adjustments (no space char)
        writer.Save().Font(font, 12).TextMove(40, 720);
        writer.Op(new TJ_Op(new List<TJ_Item<double>>
        {
            // Word gaps spelled as glyph adjustments with no space character — PDF/UA 7.2. Kept out of
            // the "Invoice" line rule's reach so the words can be asserted directly.
            new() { Data = Encode(font, "Statement") },
            new() { Shift = -700 },
            new() { Data = Encode(font, "#:") },
            new() { Shift = -700 },
            new() { Data = Encode(font, "STM-12345") }
        }));
        writer.Restore();

        // 3. Three separate Tj operators split at word boundaries
        writer.Save().Font(font, 12).TextMove(40, 690).Text("Invoice").Restore();
        writer.Save().Font(font, 12).TextMove(90, 690).Text("#:").Restore();
        writer.Save().Font(font, 12).TextMove(110, 690).Text("INV-12345").Restore();

        // 4. One word split mid-token across two Tj operators
        writer.Save().Font(font, 12).TextMove(40, 660).Text("Invoice #: INV-").Restore();
        writer.Save().Font(font, 12).TextMove(130, 660).Text("12345").Restore();

        // 5. Line claimed by P rule
        WriteLine(writer, font, 40, 630, "Subtotal Total");

        // 6. TJ with adjustments for two words
        writer.Save().Font(font, 12).TextMove(40, 600);
        writer.Op(new TJ_Op(new List<TJ_Item<double>>
        {
            new() { Data = Encode(font, "Amount") },
            new() { Shift = -500 },
            new() { Data = Encode(font, "Paid") }
        }));
        writer.Restore();

        // Lines 7-10 with non-default text state (Tw, Tc, Tz, Ts)
        writer.Save().Font(font, 12).WordSpacing(2).TextMove(40, 570).Text("Spacing Test Line").Restore();
        writer.Save().Font(font, 12).Op(new Tc_Op(1.5)).TextMove(40, 540).Text("Char Spacing Line").Restore();
        writer.Save().Font(font, 12).Op(new Tz_Op(120)).TextMove(40, 510).Text("Horizontal Scale Line").Restore();
        writer.Save().Font(font, 12).Op(new Ts_Op(3)).TextMove(40, 480).Text("Text Rise Line").Restore();

        WriteLine(writer, font, 520, 24, "Page 1");
        return doc;
    }

    internal static RuleSet CreateC02RuleSet() =>
        new(
            "c-02-rules",
            new[]
            {
                FooterRule(),
                new Rule("c02-line", RemediationActions.Tag("P"), Predicates.Text.Contains("Invoice"), CandidateSelector.Text(Granularity.Line)),
                new Rule("c02-line2", RemediationActions.Tag("P"), Predicates.Text.Contains("Subtotal"), CandidateSelector.Text(Granularity.Line)),
                new Rule(
                    "c02-tj-words",
                    RemediationActions.Tag("P"),
                    Predicates.Text.Matches("^(Statement|#:|STM-12345)$"),
                    CandidateSelector.Text(Granularity.Word)),
                new Rule("c02-tj-word1", RemediationActions.Tag("P"), Predicates.Text.Contains("Amount"), CandidateSelector.Text(Granularity.Word)),
                new Rule("c02-tj-word2", RemediationActions.Tag("P"), Predicates.Text.Contains("Paid"), CandidateSelector.Text(Granularity.Word)),
                new Rule("c02-state1", RemediationActions.Tag("P"), Predicates.Text.StartsWith("Spacing"), CandidateSelector.Text(Granularity.Line)),
                new Rule("c02-state2", RemediationActions.Tag("P"), Predicates.Text.Contains("Char"), CandidateSelector.Text(Granularity.Line)),
                new Rule("c02-state3", RemediationActions.Tag("P"), Predicates.Text.Contains("Horizontal"), CandidateSelector.Text(Granularity.Line)),
                new Rule("c02-state4", RemediationActions.Tag("P"), Predicates.Text.Contains("Rise"), CandidateSelector.Text(Granularity.Line))
            },
            Array.Empty<RemediationAnchor>(),
            tolerancedZones: FooterZones(),
            artifacts: FooterArtifacts());

    private static byte[] Encode(IWritableFont font, string text)
    {
        var encoded = new List<byte>();
        var buffer = new byte[4];
        foreach (var character in font.ConvertFromUnicode(text, 0, text.Length, buffer))
        {
            encoded.AddRange(buffer.Take(character.ByteCount));
        }
        return encoded.ToArray();
    }

    // C-03: Graphical content
    internal static PdfDocument CreateC03Input()
    {
        var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        var font = CreateEmbeddedFont();
        using var writer = page.GetWriter();

        // 1. Logo image top-left (100x40)
        var imgStream = new PdfStream();
        imgStream.Dictionary[PdfName.Subtype] = PdfName.Image;
        imgStream.Dictionary[PdfName.Width] = new PdfIntNumber(10);
        imgStream.Dictionary[PdfName.Height] = new PdfIntNumber(10);
        imgStream.Dictionary[PdfName.ColorSpace] = PdfName.DeviceGray;
        imgStream.Dictionary[PdfName.BitsPerComponent] = new PdfIntNumber(8);
        imgStream.Contents = new PdfByteArrayStreamContents(new byte[100]);
        var logoImg = new XObjImage(imgStream);
        writer.Image(logoImg, 40, 700, 100, 40);

        // 2. Second image placed in footer
        writer.Image(logoImg, 40, 24, 50, 16);

        // 3. Table ruling lines (horizontal and vertical)
        writer.Save().SetStrokingRGB(0, 0, 0).LineWidth(1)
            .MoveTo(40, 500).LineTo(550, 500).Stroke()
            .MoveTo(40, 500).LineTo(40, 400).Stroke()
            .Restore();

        // 4. Filled background rectangle behind header band
        writer.Save().SetFillRGB(230, 230, 230).Rect(40, 650, 510, 30).Fill().Restore();

        // 5. Clip-only path (W n)
        writer.Save().Rect(40, 550, 200, 50).Clip().EndPathNoOp().Restore();

        // 6. Form XObject placed twice
        var formWriter = new FormWriter(100, 40);
        formWriter.Rect(0, 0, 100, 40).Stroke();
        var repeatedForm = formWriter.Complete();
        writer.Form(repeatedForm, 200, 700);
        writer.Form(repeatedForm, 350, 700);

        // 7. Shading fill
        var shadingDict = new PdfDictionary
        {
            [new PdfName("ShadingType")] = new PdfIntNumber(2),
            [PdfName.ColorSpace] = PdfName.DeviceGray,
            [new PdfName("Coords")] = new PdfArray { new PdfIntNumber(0), new PdfIntNumber(0), new PdfIntNumber(100), new PdfIntNumber(0) },
            [new PdfName("Extend")] = new PdfArray { PdfBoolean.True, PdfBoolean.True },
            [new PdfName("Function")] = new PdfDictionary
            {
                [new PdfName("FunctionType")] = new PdfIntNumber(2),
                [new PdfName("Domain")] = new PdfArray { new PdfIntNumber(0), new PdfIntNumber(1) },
                [new PdfName("C0")] = new PdfArray { new PdfDoubleNumber(0.9) },
                [new PdfName("C1")] = new PdfArray { new PdfDoubleNumber(1.0) },
                [new PdfName("N")] = new PdfIntNumber(1)
            }
        };
        writer.Shading(shadingDict);

        // 8. Ordinary body text
        writer.Save().SetFillRGB(0, 0, 0).Font(font, 12).TextMove(40, 450).Text("Graphical Page Body Text").Restore();
        writer.Save().SetFillRGB(0, 0, 0).Font(font, 12).TextMove(520, 24).Text("Page 1").Restore();

        return doc;
    }

    internal static RuleSet CreateC03RuleSet() =>
        new(
            "c-03-rules",
            new[]
            {
                FooterRule(),
                new Rule(
                    "c03-logo",
                    RemediationActions.Tag("Figure"),
                    Predicates.Content.Type(RemediationCandidateKind.Image).And(Predicates.Flow.InZone("header-zone")),
                    CandidateSelector.Content(RemediationCandidateKind.Image)),
                new Rule("c03-footer-logo", RemediationActions.Artifact(ArtifactSubtype.Layout), Predicates.Content.Type(RemediationCandidateKind.Image).And(Predicates.Flow.InZone("footer")), CandidateSelector.Content(RemediationCandidateKind.Image)),
                new Rule("c03-lines", RemediationActions.Artifact(ArtifactSubtype.Layout), Predicates.Content.Type(RemediationCandidateKind.Path), CandidateSelector.Content(RemediationCandidateKind.Path)),
                new Rule("c03-form", RemediationActions.Artifact(ArtifactSubtype.Layout), Predicates.Content.Type(RemediationCandidateKind.Form), CandidateSelector.Content(RemediationCandidateKind.Form)),
                new Rule("c03-shading", RemediationActions.Artifact(ArtifactSubtype.Layout), Predicates.Content.Type(RemediationCandidateKind.Shading), CandidateSelector.Content(RemediationCandidateKind.Shading)),
                BodyParagraphRule(),
                new Rule(
                    "c03-logo-alt",
                    RemediationActions.Alt(ClaimPredicates.FromRule("c03-logo"), "Acme logo"),
                    stage: Stage.Refine)
            },
            Array.Empty<RemediationAnchor>(),
            tolerancedZones: new[]
            {
                FooterZones()[0],
                new TolerancedZone("header-zone", LayoutCoord.Absolute(new PdfRect<double>(0, 680, 600, 792))),
                new TolerancedZone("graphics-zone", LayoutCoord.Absolute(new PdfRect<double>(0, 390, 612, 690))),
                new TolerancedZone("page-zone", LayoutCoord.Absolute(new PdfRect<double>(0, 0, 612, 792)))
            });

    // C-08: Existing annotations
    internal static PdfDocument CreateC08Input()
    {
        var doc = PdfDocument.Create();
        var font = CreateEmbeddedFont();
        var page1 = doc.AddPage(PageSize.LETTER);
        var page2 = doc.AddPage(PageSize.LETTER);
        using (var writer = page1.GetWriter())
        {
            WriteLine(writer, font, 72, 700, "Go to details");
        }
        using (var writer = page2.GetWriter())
        {
            WriteLine(writer, font, 72, 700, "Visit example");
            WriteLine(writer, font, 72, 620, "Details target");
        }

        var internalLink = new PdfDictionary
        {
            [PdfName.TypeName] = PdfName.Annot,
            [PdfName.Subtype] = PdfName.Link,
            [PdfName.Rect] = new PdfArray { 70, 695, 160, 715 },
            [PdfName.Contents] = PdfString.CreateTextString("Go to details"),
            [PdfName.Border] = new PdfArray { 0, 0, 0 },
            [PdfName.Dest] = new PdfArray { page2.NativeObject.Indirect(), PdfName.Fit }
        };
        page1.AddAnnotation(internalLink);

        var uriLink = new PdfDictionary
        {
            [PdfName.TypeName] = PdfName.Annot,
            [PdfName.Subtype] = PdfName.Link,
            [PdfName.Rect] = new PdfArray { 70, 695, 160, 715 },
            [PdfName.Contents] = PdfString.CreateTextString("Visit example"),
            [PdfName.Border] = new PdfArray { 0, 0, 0 },
            [PdfName.A] = new PdfDictionary
            {
                [PdfName.S] = (PdfName)"URI",
                [(PdfName)"URI"] = PdfString.CreateTextString("https://example.com")
            }
        };
        page2.AddAnnotation(uriLink);

        var stamp = ExistingAnnotation(page1, "Stamp", new PdfRect<double>(420, 680, 500, 720), "Approved");
        page1.AddAnnotation(stamp);
        var popup = new PdfDictionary
        {
            [PdfName.TypeName] = PdfName.Annot,
            [PdfName.Subtype] = (PdfName)"Popup",
            [PdfName.Rect] = new PdfArray { 500, 650, 580, 720 },
            [PdfName.F] = new PdfIntNumber(2),
            [(PdfName)"Parent"] = stamp.Indirect()
        };
        page1.AddAnnotation(popup);

        var attachment = ExistingAnnotation(page2, "FileAttachment", new PdfRect<double>(420, 600, 450, 630), "Invoice attachment");
        var embedded = new PdfStream();
        embedded.Dictionary[PdfName.TypeName] = PdfName.EmbeddedFile;
        embedded.Contents = new PdfByteArrayStreamContents(Encoding.UTF8.GetBytes("attachment"));
        attachment[(PdfName)"FS"] = new PdfDictionary
        {
            [PdfName.TypeName] = (PdfName)"Filespec",
            [PdfName.F] = new PdfString("attachment.txt"),
            [(PdfName)"UF"] = PdfString.CreateTextString("attachment.txt"),
            [(PdfName)"AFRelationship"] = (PdfName)"Data",
            [PdfName.EF] = new PdfDictionary { [PdfName.F] = embedded.Indirect() }
        };
        page2.AddAnnotation(attachment);
        return doc;
    }

    private static PdfDictionary ExistingAnnotation(
        PdfPage page,
        string subtype,
        PdfRect<double> bounds,
        string contents)
    {
        var appearanceWriter = new FormWriter(bounds.Width(), bounds.Height());
        appearanceWriter.Rect(0, 0, bounds.Width(), bounds.Height()).Stroke();
        var appearance = appearanceWriter.Complete();
        return new PdfDictionary
        {
            [PdfName.TypeName] = PdfName.Annot,
            [PdfName.Subtype] = (PdfName)subtype,
            [PdfName.Rect] = PdfRectangle.FromContentModel(bounds).NativeObject,
            [PdfName.Contents] = PdfString.CreateTextString(contents),
            [(PdfName)"AP"] = new PdfDictionary { [PdfName.N] = appearance.NativeObject.Indirect() }
        };
    }

    internal static RuleSet CreateC08RuleSet() => new(
        "c-08-rules",
        new[]
        {
            new Rule("c08-link-text", RemediationActions.Tag("Link"),
                Predicates.Text.Equals("Go to details").Or(Predicates.Text.Equals("Visit example")),
                CandidateSelector.Text(Granularity.Line)),
            new Rule("c08-target", RemediationActions.Tag("P"), Predicates.Text.Equals("Details target"),
                CandidateSelector.Text(Granularity.Line)),
            new Rule("c08-internal-link", RemediationActions.AdoptAnnotation(
                    ClaimPredicates.FromRule("c08-link-text"),
                    destinationTarget: ClaimPredicates.FromRule("c08-target")),
                Predicates.Annotation.Subtype("Link").And(
                    Predicates.Annotation.DestinationKind(AnnotationDestinationKind.Internal)),
                CandidateSelector.Annotations()),
            new Rule("c08-uri-link", RemediationActions.AdoptAnnotation(
                    ClaimPredicates.FromRule("c08-link-text")),
                Predicates.Annotation.Subtype("Link").And(
                    Predicates.Annotation.DestinationKind(AnnotationDestinationKind.Uri)),
                CandidateSelector.Annotations()),
            new Rule("c08-stamp", RemediationActions.AdoptAnnotation(),
                Predicates.Annotation.Subtype("Stamp"), CandidateSelector.Annotations()),
            new Rule("c08-attachment", RemediationActions.AdoptAnnotation(),
                Predicates.Annotation.Subtype("FileAttachment"), CandidateSelector.Annotations())
        });

    // C-10: Running header, footer, and watermark artifact property lists
    internal static PdfDocument CreateC10Input()
    {
        var doc = PdfDocument.Create();
        var font = CreateEmbeddedFont();
        for (var pageNumber = 1; pageNumber <= 4; pageNumber++)
        {
            var page = doc.AddPage(PageSize.LETTER);
            using var writer = page.GetWriter();
            WriteLine(writer, font, 72, 755, "Acme Corp - Confidential");
            WriteLine(writer, font, 72, 680, $"Body page {pageNumber}");
            WriteLine(writer, font, 250, 390, "DRAFT", 28);
            WriteLine(writer, font, 270, 24, $"Page {pageNumber} of 4");
        }
        return doc;
    }

    internal static RuleSet CreateC10RuleSet() => new(
        "c-10-rules",
        new[]
        {
            new Rule("c10-header", RemediationActions.HeaderArtifact(),
                Predicates.Text.Equals("Acme Corp - Confidential"), CandidateSelector.Text(Granularity.Line),
                artifact: "c10-header"),
            new Rule("c10-footer", RemediationActions.FooterArtifact(),
                Predicates.Text.StartsWith("Page "), CandidateSelector.Text(Granularity.Line),
                artifact: "c10-footer"),
            new Rule("c10-watermark", RemediationActions.WatermarkArtifact(),
                Predicates.Text.Equals("DRAFT"), CandidateSelector.Text(Granularity.Line),
                artifact: "c10-watermark"),
            new Rule("c10-body", RemediationActions.Tag("P"),
                Predicates.Text.StartsWith("Body page "), CandidateSelector.Text(Granularity.Line))
        },
        artifacts: new[]
        {
            new RemediationArtifactInventoryItem("c10-header", ArtifactSubtype.Pagination,
                semanticSubtype: ArtifactSemanticSubtype.Header, includeBoundingBox: true,
                attached: new[] { ArtifactAttachmentEdge.Top }),
            new RemediationArtifactInventoryItem("c10-footer", ArtifactSubtype.Pagination,
                semanticSubtype: ArtifactSemanticSubtype.Footer, includeBoundingBox: true,
                attached: new[] { ArtifactAttachmentEdge.Bottom }),
            new RemediationArtifactInventoryItem("c10-watermark", ArtifactSubtype.Pagination,
                semanticSubtype: ArtifactSemanticSubtype.Watermark, includeBoundingBox: true)
        });

    // C-04: Leftover accounting
    internal static PdfDocument CreateC04Input()
    {
        var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        var font = CreateEmbeddedFont();
        using var writer = page.GetWriter();

        WriteLine(writer, font, 40, 750, "Claimed Line 1");
        WriteLine(writer, font, 40, 720, "Unclaimed Line 2");
        WriteLine(writer, font, 40, 690, "Unclaimed Line 3");

        // Unclaimed decorative image
        var imgStream = new PdfStream();
        imgStream.Dictionary[PdfName.Subtype] = PdfName.Image;
        imgStream.Dictionary[PdfName.Width] = new PdfIntNumber(10);
        imgStream.Dictionary[PdfName.Height] = new PdfIntNumber(10);
        imgStream.Dictionary[PdfName.ColorSpace] = PdfName.DeviceGray;
        imgStream.Dictionary[PdfName.BitsPerComponent] = new PdfIntNumber(8);
        imgStream.Contents = new PdfByteArrayStreamContents(new byte[100]);
        writer.Image(new XObjImage(imgStream), 40, 600, 50, 50);

        // Unclaimed path
        writer.Save().SetStrokingRGB(0, 0, 0).LineWidth(1).MoveTo(40, 550).LineTo(200, 550).Stroke().Restore();

        WriteLine(writer, font, 520, 24, "Page 1");
        return doc;
    }

    internal static RuleSet CreateC04RuleSet() =>
        CreateC04RuleSet(FooterArtifacts());

    internal static RuleSet CreateC04RuleSetWithoutInventory() =>
        CreateC04RuleSet(Array.Empty<RemediationArtifactInventoryItem>());

    private static RuleSet CreateC04RuleSet(RemediationArtifactInventoryItem[] artifacts) =>
        new(
            "c-04-rules",
            new[]
            {
                FooterRule(),
                new Rule("c04-claimed", RemediationActions.Tag("P"), Predicates.Text.Contains("Claimed"), CandidateSelector.Text(Granularity.Line))
            },
            Array.Empty<RemediationAnchor>(),
            tolerancedZones: FooterZones(),
            artifacts: artifacts);

    // C-05: Semantic assertion error
    internal static PdfDocument CreateC05Input()
    {
        var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        var font = CreateEmbeddedFont();
        using var writer = page.GetWriter();

        WriteLine(writer, font, 40, 750, "Invoice Statement", 18);
        WriteLine(writer, font, 40, 700, "Invoice Total: $500.00");
        WriteLine(writer, font, 520, 24, "Page 1");
        return doc;
    }

    internal static RuleSet CreateC05RuleSet() =>
        new(
            "c-05-rules",
            new[]
            {
                // Deliberate error: footer tagged H1 instead of artifact, total artifacted
                new Rule("c05-bad-footer", RemediationActions.Tag("H1"), Predicates.Text.Contains("Page 1"), CandidateSelector.Text(Granularity.Line)),
                new Rule("c05-title", RemediationActions.Tag("P"), Predicates.Text.Contains("Invoice Statement"), CandidateSelector.Text(Granularity.Line)),
                new Rule("c05-bad-total", RemediationActions.Artifact(ArtifactSubtype.Layout), Predicates.Text.Contains("Total"), CandidateSelector.Text(Granularity.Line))
            },
            Array.Empty<RemediationAnchor>(),
            tolerancedZones: FooterZones(),
            assertions: new[]
            {
                new RuleOutputCountAssertion("c05-total-must-remain-content", "c05-bad-total", AssertionCount.Exactly(0))
            });
    // No artifact inventory: this fixture deliberately tags the footer H1 and artifacts the total, so
    // declaring footer furniture would raise inventory diagnostics unrelated to what C-05 tests.

    // C-06: Template drift pair
    internal static PdfDocument CreateC06aInput()
    {
        var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        var font = CreateEmbeddedFont();
        using var writer = page.GetWriter();

        WriteLine(writer, font, 40, 750, "Account Statement", 18);
        WriteLine(writer, font, 40, 710, "Bill To");
        WriteLine(writer, font, 40, 688, "Ada Lovelace");
        WriteLine(writer, font, 520, 24, "Page 1");
        return doc;
    }

    internal static PdfDocument CreateC06bInput()
    {
        var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        var font = CreateEmbeddedFont();
        using var writer = page.GetWriter();

        // Label changed to "Statement of Account" and Bill To shifted right by 30pt
        WriteLine(writer, font, 40, 750, "Statement of Account", 18);
        WriteLine(writer, font, 70, 710, "Bill To");
        WriteLine(writer, font, 70, 688, "Ada Lovelace");
        WriteLine(writer, font, 520, 24, "Page 1");
        return doc;
    }

    internal static RuleSet CreateC06aRuleSet() =>
        new(
            "c-06-rules",
            new[]
            {
                FooterRule(),
                new Rule("c06-title", RemediationActions.Tag("H1"), Predicates.Text.Equals("Account Statement"), CandidateSelector.Text(Granularity.Line), cardinality: RuleCardinality.Exactly(1)),
                new Rule("c06-bill-to", RemediationActions.Tag("P"), Predicates.Text.Equals("Bill To"), CandidateSelector.Text(Granularity.Line), cardinality: RuleCardinality.Exactly(1)),
                new Rule("c06-ada", RemediationActions.Tag("P"), Predicates.Text.Contains("Ada"), CandidateSelector.Text(Granularity.Line), cardinality: RuleCardinality.Exactly(1))
            },
            Array.Empty<RemediationAnchor>(),
            tolerancedZones: FooterZones(),
            artifacts: FooterArtifacts());

    // C-12: Multi-page continuous table plus an independent terms flow
    internal static PdfDocument CreateC12Input()
    {
        var doc = PdfDocument.Create();
        var font = CreateEmbeddedFont();
        for (var pageIndex = 0; pageIndex < 5; pageIndex++)
        {
            var page = doc.AddPage(PageSize.LETTER);
            using var writer = page.GetWriter();
            if (pageIndex == 0)
            {
                WriteLine(writer, font, 40, 760, "Continued Table Fixture", 18);
            }

            // Header row repeats on every table page; body rows continue across all four.
            if (pageIndex < 4)
            {
                WriteLine(writer, font, 260, 720, "Item");
                WriteLine(writer, font, 420, 720, "Amount");
                WriteLine(writer, font, 260, 690, $"Widget{pageIndex + 1}");
                WriteLine(writer, font, 420, 690, $"{(pageIndex + 1) * 10}.00");
                WriteLine(writer, font, 260, 660, $"Gadget{pageIndex + 1}");
                WriteLine(writer, font, 420, 660, $"{(pageIndex + 1) * 5}.00");
            }

            // The terms section spans pages 4-5, starting below the subtotal that ends the table's
            // flow. Flow regions may not overlap, so a second cross-page flow has to begin where the
            // first one ends rather than running beside it.
            if (pageIndex == 3)
            {
                WriteLine(writer, font, 260, 600, "Total");
                WriteLine(writer, font, 420, 600, "150.00");
                WriteLine(writer, font, 40, 520, "Terms and Conditions");
                WriteLine(writer, font, 40, 490, "Payment is due within thirty days.");
            }
            if (pageIndex == 4)
            {
                WriteLine(writer, font, 40, 740, "Late fees apply after sixty days.");
                WriteLine(writer, font, 40, 710, "End of Terms");
            }

            WriteLine(writer, font, 520, 24, $"Page {pageIndex + 1} of 5");
        }

        return doc;
    }

    internal static RuleSet CreateC12RuleSet() =>
        new(
            "c-12-rules",
            new[]
            {
                FooterRule(),
                new Rule(
                    "c12-intro",
                    RemediationActions.Tag("H1"),
                    Predicates.Text.StartsWith("Continued"),
                    CandidateSelector.Text(Granularity.Line)),
                new Rule(
                    "c12-header-cell",
                    RemediationActions.Tag("Span"),
                    Predicates.Anchor.SameRowAs("header", 4).And(Predicates.Flow.InZone("table-band")),
                    CandidateSelector.Text(Granularity.Word),
                    pages: PageSelector.Range(0, 3)),
                new Rule(
                    "c12-body-cell",
                    RemediationActions.Tag("Span"),
                    Predicates.Flow.InFlowRegion("items").And(Predicates.Flow.InZone("table-band")),
                    CandidateSelector.Text(Granularity.Word),
                    pages: PageSelector.Range(0, 3)),
                // Flow boundaries sit outside the region they delimit, so the terms heading and closing
                // line are claimed explicitly rather than left to be absorbed.
                new Rule(
                    "c12-terms-heading",
                    RemediationActions.Tag("H2"),
                    Predicates.Text.Equals("Terms and Conditions"),
                    CandidateSelector.Text(Granularity.Line)),
                new Rule(
                    "c12-terms-end",
                    RemediationActions.Tag("P"),
                    Predicates.Text.Equals("End of Terms"),
                    CandidateSelector.Text(Granularity.Line)),
                new Rule(
                    "c12-subtotal-cell",
                    RemediationActions.Tag("Span"),
                    Predicates.Anchor.SameRowAs("total", 4).And(Predicates.Flow.InZone("table-band")),
                    CandidateSelector.Text(Granularity.Word),
                    pages: PageSelector.Range(3, 3)),
                new Rule(
                    "c12-terms",
                    RemediationActions.Tag("P"),
                    Predicates.Flow.InFlowRegion("terms"),
                    CandidateSelector.Text(Granularity.Line),
                    pages: PageSelector.Range(3, 4)),
                new Rule(
                    "c12-table",
                    RemediationActions.TableOverFlattenedCells(
                        ClaimPredicates.FromRule("c12-header-cell")
                            .Or(ClaimPredicates.FromRule("c12-body-cell"))
                            .Or(ClaimPredicates.FromRule("c12-subtotal-cell")),
                        ClaimPredicates.FromRule("c12-header-cell"),
                        240, 400, 580),
                    stage: Stage.Group)
            },
            new[]
            {
                RemediationAnchor.TextLabel("header", "Item") with { Pages = PageSelector.Range(0, 3) },
                RemediationAnchor.TextLabel("total", "Total") with { Pages = PageSelector.Range(3, 3) },
                RemediationAnchor.TextLabel("terms-start", "Terms and Conditions")
                    with { Pages = PageSelector.Range(3, 3) },
                RemediationAnchor.TextLabel("terms-end", "End of Terms")
                    with { Pages = PageSelector.Last }
            },
            tolerancedZones: FooterZones()
                .Append(new TolerancedZone("table-band", LayoutCoord.Absolute(new PdfRect<double>(0, 585, 612, 745))))
                .ToArray(),
            flowRegions: new[]
            {
                new FlowRegion(
                    "items",
                    FlowBoundary.Anchor("header"),
                    FlowBoundary.Anchor("total"),
                    ContinuationPolicy: FlowContinuationPolicy.ContinueUntilEnd),
                new FlowRegion(
                    "terms",
                    FlowBoundary.Anchor("terms-start"),
                    FlowBoundary.Anchor("terms-end"),
                    ContinuationPolicy: FlowContinuationPolicy.ContinueUntilEnd)
            },
            artifacts: FooterArtifacts());

    // C-23: Label/value pairs
    internal static PdfDocument CreateC23Input()
    {
        var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        var font = CreateEmbeddedFont();
        using var writer = page.GetWriter();

        // 1. Separate operators, value to the right
        WriteLine(writer, font, 40, 750, "Invoice #");
        WriteLine(writer, font, 150, 750, "INV-10042");

        // 2. Single operator: label and value together
        WriteLine(writer, font, 40, 720, "Invoice #: INV-10042");

        // 3. Label above, value below
        WriteLine(writer, font, 40, 690, "Due Date");
        WriteLine(writer, font, 40, 670, "2026-01-15");

        // 4. Label with value 2 columns right, unrelated field between
        WriteLine(writer, font, 40, 640, "PO Number");
        WriteLine(writer, font, 180, 640, "Status: Active");
        WriteLine(writer, font, 350, 640, "PO-9988");

        // 5. Empty value label
        WriteLine(writer, font, 40, 610, "Notes:");
        WriteLine(writer, font, 40, 580, "Next Section Header");

        // 6. Right-aligned value in totals block
        WriteLine(writer, font, 40, 550, "Total Amount");
        WriteLine(writer, font, 500, 550, "$150.00");

        WriteLine(writer, font, 520, 24, "Page 1");
        return doc;
    }

    internal static RuleSet CreateC23RuleSet() =>
        new(
            "c-23-rules",
            new[]
            {
                FooterRule(),
                new Rule("c23-inv-label", RemediationActions.Tag("P"), Predicates.Text.Equals("Invoice"), CandidateSelector.Text(Granularity.Word)),
                new Rule("c23-inv-val", RemediationActions.Tag("P"), Predicates.Anchor.RightOf("c23-inv-label", tolerance: 5).And(Predicates.Text.Equals("INV-10042")), CandidateSelector.Text(Granularity.Word)),
                new Rule("c23-due-label", RemediationActions.Tag("P"), Predicates.Text.Equals("Due"), CandidateSelector.Text(Granularity.Word)),
                new Rule("c23-due-val", RemediationActions.Tag("P"), Predicates.Anchor.Below("c23-due-label", maxDistance: 30), CandidateSelector.Text(Granularity.Word)),
                new Rule("c23-po-label", RemediationActions.Tag("P"), Predicates.Text.Equals("PO"), CandidateSelector.Text(Granularity.Word)),
                new Rule("c23-po-val", RemediationActions.Tag("P"), Predicates.Anchor.SameRowAs("c23-po-label").And(Predicates.Text.Equals("PO-9988")), CandidateSelector.Text(Granularity.Word)),
                new Rule("c23-notes-label", RemediationActions.Tag("P"), Predicates.Text.Equals("Notes:"), CandidateSelector.Text(Granularity.Line)),
                // The empty-value case: nothing sits below "Notes:" except the next section header, so
                // a bounded Below must select nothing rather than silently capturing it.
                new Rule(
                    "c23-notes-val",
                    RemediationActions.Tag("P"),
                    // maxDistance is edge-to-edge, and the next section header sits ~14pt below the
                    // label's box, so the bound is what stops an empty value capturing it.
                    Predicates.Anchor.Below("c23-notes-label", maxDistance: 10),
                    CandidateSelector.Text(Granularity.Word),
                    cardinality: RuleCardinality.Exactly(0)),
                new Rule("c23-next-hdr", RemediationActions.Tag("H1"), Predicates.Text.Equals("Next Section Header"), CandidateSelector.Text(Granularity.Line)),
                new Rule("c23-total-label", RemediationActions.Tag("P"), Predicates.Text.Equals("Total"), CandidateSelector.Text(Granularity.Word)),
                new Rule("c23-total-val", RemediationActions.Tag("P"), Predicates.Anchor.SameRowAs("c23-total-label").And(Predicates.Text.Contains("$150.00")), CandidateSelector.Text(Granularity.Word)),
                // Remaining anchor-relative forms the entry names. They run after the rules above, so
                // their targets are already owned and matched inputs rather than claims are the
                // evidence that each selected the intended value.
                new Rule(
                    "c23-between",
                    RemediationActions.Tag("P"),
                    // Between spans the corridor from one anchor to the other, so it selects the label
                    // column rather than a value parked far to the right.
                    Predicates.Anchor.Between("c23-due-label", "c23-total-label")
                        .And(Predicates.Text.Equals("PO")),
                    CandidateSelector.Text(Granularity.Word)),
                new Rule(
                    "c23-same-column",
                    RemediationActions.Tag("P"),
                    Predicates.Anchor.SameColumnAs("c23-due-label", tolerance: 6)
                        .And(Predicates.Text.Equals("2026-01-15")),
                    CandidateSelector.Text(Granularity.Word)),
                new Rule(
                    "c23-nearest",
                    RemediationActions.Tag("P"),
                    Predicates.Anchor.NearestTo("c23-total-label", AnchorDirection.Right)
                        .And(Predicates.Text.Contains("$150.00")),
                    CandidateSelector.Text(Granularity.Word)),
                new Rule("c23-residual", RemediationActions.Tag("P"), RemediationPredicate.Always, CandidateSelector.Text(Granularity.Word))
            },
            new[]
            {
                RemediationAnchor.Selector("c23-inv-label", Granularity.Word, Predicates.Text.Equals("Invoice"), AnchorSelection.FirstInReadingOrder),
                RemediationAnchor.Selector("c23-due-label", Granularity.Word, Predicates.Text.Equals("Due")),
                RemediationAnchor.Selector("c23-po-label", Granularity.Word, Predicates.Text.Equals("PO")),
                RemediationAnchor.Selector("c23-notes-label", Granularity.Word, Predicates.Text.Equals("Notes:")),
                RemediationAnchor.Selector("c23-total-label", Granularity.Word, Predicates.Text.Equals("Total"))
            },
            tolerancedZones: FooterZones()
                .Append(new TolerancedZone(
                    "c23-spacing",
                    LayoutCoord.Absolute(new PdfRect<double>(0, 100, 612, 792))))
                .ToArray(),
            artifacts: FooterArtifacts()
                .Append(new RemediationArtifactInventoryItem(
                    // Word-granularity rules claim words but not the spaces between them, so the
                    // inter-word gaps are absorbed as layout artifacts. Declared as an open count
                    // because the number is a property of the text, not of the rule set — pinning it
                    // to a constant made the fixture break on any edit and hid what the items were.
                    "c23-inter-word-spacing",
                    ArtifactSubtype.Layout,
                    zoneId: "c23-spacing",
                    occurrence: new AssertionCount(0)))
                .ToArray());

    // C-29: Structural composition across explicit Group passes
    internal static PdfDocument CreateC29aInput()
    {
        var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        var font = CreateEmbeddedFont();
        using var writer = page.GetWriter();
        WriteLine(writer, font, 40, 750, "1.");
        WriteLine(writer, font, 80, 720, "Outer item body");
        WriteLine(writer, font, 100, 680, "a.");
        WriteLine(writer, font, 140, 650, "Inner item body");
        return doc;
    }

    internal static RuleSet CreateC29aRuleSet() =>
        new(
            "c-29-a-rules",
            new[]
            {
                new Rule(
                    "c29a-outer-label",
                    RemediationActions.Tag("Span"),
                    Predicates.Text.Equals("1."),
                    CandidateSelector.Text(Granularity.Line),
                    slot: "c29a-outer-label"),
                new Rule(
                    "c29a-outer-text",
                    RemediationActions.Tag("P"),
                    Predicates.Text.Equals("Outer item body"),
                    CandidateSelector.Text(Granularity.Line),
                    slot: "c29a-outer-text"),
                new Rule(
                    "c29a-inner-label",
                    RemediationActions.Tag("Span"),
                    Predicates.Text.Equals("a."),
                    CandidateSelector.Text(Granularity.Line),
                    slot: "c29a-inner-label"),
                new Rule(
                    "c29a-inner-text",
                    RemediationActions.Tag("P"),
                    Predicates.Text.Equals("Inner item body"),
                    CandidateSelector.Text(Granularity.Line),
                    slot: "c29a-inner-text"),
                new Rule(
                    "c29a-inner-body",
                    RemediationActions.Group(
                        "LBody",
                        ClaimPredicates.FromRule("c29a-inner-label")
                            .Or(ClaimPredicates.FromRule("c29a-inner-text"))),
                    stage: Stage.Group,
                    slot: "c29a-inner-body"),
                new Rule(
                    "c29a-inner-item",
                    RemediationActions.Group("LI", ClaimPredicates.FromRule("c29a-inner-body")),
                    stage: Stage.Group,
                    slot: "c29a-inner-item",
                    groupPass: 10),
                new Rule(
                    "c29a-inner-list",
                    RemediationActions.Group("L", ClaimPredicates.FromRule("c29a-inner-item")),
                    stage: Stage.Group,
                    slot: "c29a-inner-list",
                    groupPass: 20),
                new Rule(
                    "c29a-outer-body",
                    RemediationActions.Group(
                        "LBody",
                        ClaimPredicates.FromRule("c29a-outer-label")
                            .Or(ClaimPredicates.FromRule("c29a-outer-text"))
                            .Or(ClaimPredicates.FromRule("c29a-inner-list"))),
                    stage: Stage.Group,
                    slot: "c29a-outer-body",
                    groupPass: 30),
                new Rule(
                    "c29a-outer-item",
                    RemediationActions.Group("LI", ClaimPredicates.FromRule("c29a-outer-body")),
                    stage: Stage.Group,
                    slot: "c29a-outer-item",
                    groupPass: 40),
                new Rule(
                    "c29a-outer-list",
                    RemediationActions.Group("L", ClaimPredicates.FromRule("c29a-outer-item")),
                    stage: Stage.Group,
                    slot: "c29a-outer-list",
                    groupPass: 50)
            },
            structuralTemplate: Template(
                Node(
                    "L",
                    "c29a-outer-list",
                    Node(
                        "LI",
                        "c29a-outer-item",
                        Node(
                            "LBody",
                            "c29a-outer-body",
                            Node("Span", "c29a-outer-label"),
                            Node("P", "c29a-outer-text"),
                            Node(
                                "L",
                                "c29a-inner-list",
                                Node(
                                    "LI",
                                    "c29a-inner-item",
                                    Node(
                                        "LBody",
                                        "c29a-inner-body",
                                        Node("Span", "c29a-inner-label"),
                                        Node("P", "c29a-inner-text")))))))));

    internal static PdfDocument CreateC29bInput()
    {
        var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        var font = CreateEmbeddedFont();
        using var writer = page.GetWriter();
        WriteLine(writer, font, 40, 750, "Section Alpha", 18);
        WriteLine(writer, font, 40, 720, "Alpha introduction");
        WriteLine(writer, font, 60, 680, "Alpha detail", 14);
        WriteLine(writer, font, 60, 650, "Alpha detail body");
        WriteLine(writer, font, 40, 520, "Section Beta", 18);
        WriteLine(writer, font, 40, 490, "Beta introduction");
        WriteLine(writer, font, 60, 450, "Beta detail", 14);
        WriteLine(writer, font, 60, 420, "Beta detail body");
        return doc;
    }

    internal static RuleSet CreateC29bRuleSet()
    {
        var alphaZone = ClaimPredicates.Within("c29b-alpha-zone");
        var betaZone = ClaimPredicates.Within("c29b-beta-zone");
        return new RuleSet(
            "c-29-b-rules",
            new[]
            {
                C29bLeaf("c29b-alpha-h1", "H1", "Section Alpha"),
                C29bLeaf("c29b-alpha-intro", "P", "Alpha introduction"),
                C29bLeaf("c29b-alpha-h2", "H2", "Alpha detail"),
                C29bLeaf("c29b-alpha-body", "P", "Alpha detail body"),
                C29bLeaf("c29b-beta-h1", "H1", "Section Beta"),
                C29bLeaf("c29b-beta-intro", "P", "Beta introduction"),
                C29bLeaf("c29b-beta-h2", "H2", "Beta detail"),
                C29bLeaf("c29b-beta-body", "P", "Beta detail body"),
                new Rule(
                    "c29b-alpha-inner",
                    RemediationActions.Group(
                        "Sect",
                        ClaimPredicates.FromRule("c29b-alpha-h2")
                            .Or(ClaimPredicates.FromRule("c29b-alpha-body"))
                            .And(alphaZone)),
                    stage: Stage.Group,
                    slot: "c29b-alpha-inner"),
                new Rule(
                    "c29b-beta-inner",
                    RemediationActions.Group(
                        "Sect",
                        ClaimPredicates.FromRule("c29b-beta-h2")
                            .Or(ClaimPredicates.FromRule("c29b-beta-body"))
                            .And(betaZone)),
                    stage: Stage.Group,
                    slot: "c29b-beta-inner"),
                new Rule(
                    "c29b-alpha-outer",
                    RemediationActions.Group(
                        "Sect",
                        ClaimPredicates.FromRule("c29b-alpha-h1")
                            .Or(ClaimPredicates.FromRule("c29b-alpha-intro"))
                            .Or(ClaimPredicates.FromRule("c29b-alpha-inner"))
                            .And(alphaZone)),
                    stage: Stage.Group,
                    slot: "c29b-alpha-outer",
                    groupPass: 10),
                new Rule(
                    "c29b-beta-outer",
                    RemediationActions.Group(
                        "Sect",
                        ClaimPredicates.FromRule("c29b-beta-h1")
                            .Or(ClaimPredicates.FromRule("c29b-beta-intro"))
                            .Or(ClaimPredicates.FromRule("c29b-beta-inner"))
                            .And(betaZone)),
                    stage: Stage.Group,
                    slot: "c29b-beta-outer",
                    groupPass: 10)
            },
            Array.Empty<RemediationAnchor>(),
            tolerancedZones: new[]
            {
                new TolerancedZone("c29b-alpha-zone", LayoutCoord.Absolute(new PdfRect<double>(0, 600, 612, 792))),
                new TolerancedZone("c29b-beta-zone", LayoutCoord.Absolute(new PdfRect<double>(0, 360, 612, 570)))
            },
            structuralTemplate: Template(
                C29bSectionTemplate("alpha"),
                C29bSectionTemplate("beta")));
    }

    private static Rule C29bLeaf(string id, string tag, string text) =>
        new(
            id,
            RemediationActions.Tag(tag),
            Predicates.Text.Equals(text),
            CandidateSelector.Text(Granularity.Line),
            slot: id);

    private static RemediationStructuralTemplateNode C29bSectionTemplate(string section) =>
        Node(
            "Sect",
            $"c29b-{section}-outer",
            Node("H1", $"c29b-{section}-h1"),
            Node("P", $"c29b-{section}-intro"),
            Node(
                "Sect",
                $"c29b-{section}-inner",
                Node("H2", $"c29b-{section}-h2"),
                Node("P", $"c29b-{section}-body")));

    // C-30: Prescriptive template assembly and repeated BindOver occurrences
    internal static PdfDocument CreateC30Input()
    {
        var doc = PdfDocument.Create();
        var font = CreateEmbeddedFont();
        for (var pageIndex = 0; pageIndex < 2; pageIndex++)
        {
            var page = doc.AddPage(PageSize.LETTER);
            using var writer = page.GetWriter();
            WriteLine(writer, font, 40, 740, $"Template section {pageIndex + 1}");
        }
        return doc;
    }

    internal static RuleSet CreateC30RuleSet()
    {
        var template = new RemediationStructuralTemplate(new[]
        {
            new RemediationStructuralTemplateNode(
                "Sect",
                new[] { new RemediationStructuralTemplateNode("P", id: "line") },
                id: "section",
                occurrence: RemediationStructuralOccurrence.OneOrMore)
        }, RemediationStructuralTemplateMode.Prescriptive);
        return new RuleSet(
            "c-30-rules",
            new[]
            {
                new Rule("c30-line", RemediationActions.Bind(), candidates: CandidateSelector.Text(Granularity.Paragraph), slot: "line"),
                new Rule(
                    "c30-section",
                    RemediationActions.BindOver(ClaimPredicates.FromSlot("line")),
                    stage: Stage.Group,
                    groupPass: 10,
                    slot: "section")
            },
            structuralTemplate: template);
    }

    // C-24: Ambiguous and repeated anchors
    internal static PdfDocument CreateC24Input()
    {
        var doc = PdfDocument.Create();
        var font = CreateEmbeddedFont();

        // Page 1
        var page1 = doc.AddPage(PageSize.LETTER);
        using (var writer1 = page1.GetWriter())
        {
            WriteLine(writer1, font, 40, 750, "Header Date 2026-01-01", 16);
            WriteLine(writer1, font, 40, 600, "Table Date");
            WriteLine(writer1, font, 150, 600, "Table Amount");
            WriteLine(writer1, font, 40, 500, "Footer Date 2026-12-31");
            WriteLine(writer1, font, 40, 450, "Totals Amount");
            WriteLine(writer1, font, 520, 24, "Page 1");
        }

        // Page 2
        var page2 = doc.AddPage(PageSize.LETTER);
        using (var writer2 = page2.GetWriter())
        {
            WriteLine(writer2, font, 40, 750, "Header Date 2026-01-01", 16);
            WriteLine(writer2, font, 40, 500, "Footer Date 2026-12-31");
            WriteLine(writer2, font, 520, 24, "Page 2");
        }

        return doc;
    }

    internal static RuleSet CreateC24RuleSet() =>
        new(
            "c-24-rules",
            new[]
            {
                FooterRule(),
                new Rule("c24-ambiguous-probe", RemediationActions.Tag("P"), Predicates.Anchor.SameRowAs("ambiguous-date"), CandidateSelector.Text(Granularity.Line)),
                // Zero-based: index 0 is the header occurrence, index 2 the footer one.
                new Rule("c24-nth-first", RemediationActions.Tag("P"), Predicates.Anchor.SameRowAs("first-date", 4), CandidateSelector.Text(Granularity.Line)),
                new Rule("c24-nth-third", RemediationActions.Tag("P"), Predicates.Anchor.SameRowAs("third-date", 4), CandidateSelector.Text(Granularity.Line)),
                new Rule("c24-table-date", RemediationActions.Tag("P"), Predicates.Text.Equals("Table Date"), CandidateSelector.Text(Granularity.Line)),
                // Referenced so the required anchor actually resolves: page 2 has no table, and the
                // absence must read differently from ambiguity.
                new Rule("c24-required-probe", RemediationActions.Tag("P"), Predicates.Anchor.SameRowAs("page1-table-date", 4), CandidateSelector.Text(Granularity.Line)),
                // Each of these resolves its repeated label a different way; none may diagnose.
                new Rule("c24-styled", RemediationActions.Tag("P"), Predicates.Anchor.SameRowAs("styled-date", 4), CandidateSelector.Text(Granularity.Line)),
                new Rule("c24-scoped", RemediationActions.Tag("P"), Predicates.Anchor.SameRowAs("header-date-scoped", 4), CandidateSelector.Text(Granularity.Line)),
                BodyParagraphRule()
            },
            new[]
            {
                RemediationAnchor.Selector("ambiguous-date", Granularity.Word, Predicates.Text.Equals("Date"), AnchorSelection.OptionalSingle),
                // Page 1 carries three "Date" occurrences; page 2 only two, so the ordinal and
                // discriminating anchors are scoped to the page whose ambiguity they resolve.
                RemediationAnchor.Selector("first-date", Granularity.Word, Predicates.Text.Equals("Date"), AnchorSelection.NthInReadingOrder(0))
                    with { Pages = PageSelector.First },
                RemediationAnchor.Selector("third-date", Granularity.Word, Predicates.Text.Equals("Date"), AnchorSelection.NthInReadingOrder(2))
                    with { Pages = PageSelector.First },
                // Style discriminates the 16pt header occurrence from the 12pt ones.
                RemediationAnchor.Selector("styled-date", Granularity.Word, Predicates.Text.Equals("Date"))
                    with { Pages = PageSelector.First, Style = Predicates.Font.Size(NumericOperator.GreaterThanOrEqual, 16) },
                // Page scoping makes the page-2 absence of the table a non-event.
                RemediationAnchor.TextLabel("header-date-scoped", "Header Date 2026-01-01")
                    with { Pages = PageSelector.First },
                RemediationAnchor.TextLabel("page1-table-date", "Table Date", selection: AnchorSelection.RequiredSingle)
            },
            tolerancedZones: FooterZones(),
            artifacts: FooterArtifacts());

    #endregion
}

internal sealed record GeneratedRemediationInput(string FileName, string Path, byte[] Bytes);

internal sealed record GeneratedRemediationFixture(
    string Name,
    string FileName,
    string? Path,
    string InputPath,
    PdfUaProfile Profile,
    byte[] Bytes,
    RemediationReport Report,
    FixtureOutcome Outcome = FixtureOutcome.Commits);

internal sealed record RemediationFixtureBlueprint(
    string Name,
    string BaseName,
    Func<PdfDocument> CreateInput,
    Func<RuleSet> CreateRuleSet,
    FixtureOutcome Outcome = FixtureOutcome.Commits,
    RemediationLeftoverPolicy? LeftoverPolicy = null);

/// <summary>What a fixture is expected to do, so fixtures that must fail still run through the harness.</summary>
internal enum FixtureOutcome
{
    /// <summary>Commits clean with no failed assertions.</summary>
    Commits,
    /// <summary>Commits, but a declared assertion is expected to fail — see C-05.</summary>
    CommitsWithFailedAssertions,
    /// <summary>Captured from a dry run and never committed; the diagnostics are the deliverable.</summary>
    DiagnosesOnly
}
