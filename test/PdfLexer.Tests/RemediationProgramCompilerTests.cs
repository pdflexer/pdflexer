using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PdfLexer.DOM;
using PdfLexer.Fonts;
using PdfLexer.Remediation;
using PdfLexer.Writing;
using Xunit;

namespace PdfLexer.Tests;

public sealed class RemediationProgramCompilerTests
{
    [Fact]
    public void CompilerOrdersSlotAnchorDependenciesDeterministically()
    {
        var program = Program(
            new[]
            {
                new BindingRule(
                    "body", BindingTarget.ToSlot(SlotRef.Absolute("/body")), CandidateSelector.Text(Granularity.Paragraph),
                    Predicates.Anchor.Below("title-anchor")),
                new BindingRule(
                    "title", BindingTarget.ToSlot(SlotRef.Absolute("/title")), CandidateSelector.Text(Granularity.Paragraph),
                    Predicates.Text.Equals("Invoice"))
            },
            new[] { new SlotAnchor("title-anchor", SlotRef.Absolute("/title")) });

        var compiled = RemediationProgramCompiler.Compile(program);

        Assert.True(compiled.IsValid, string.Join(Environment.NewLine, compiled.Errors));
        Assert.Equal(new[] { "title" }, compiled.Layers[0].Select(x => x.Id));
        Assert.Equal(new[] { "body" }, compiled.Layers[1].Select(x => x.Id));
    }

    [Fact]
    public void CompilerRejectsUnknownSlotsAndNonDerivableRepeatingComposites()
    {
        var template = new RemediationTemplate(
            "invoice", "1", PdfUaProfile.PdfUa1,
            new RemediationTemplateNode("Document", children: new[]
            {
                new RemediationTemplateNode("Sect", "items", new[]
                {
                    new RemediationTemplateNode("P", "line", occurrence: TemplateOccurrence.Optional)
                }, TemplateOccurrence.OneOrMore)
            }));
        var program = new RemediationProgram(
            "invoice-program", template,
            new[] { new BindingRule("missing", BindingTarget.ToSlot(SlotRef.Absolute("/unknown")), CandidateSelector.Text(Granularity.Paragraph)) });

        var compiled = RemediationProgramCompiler.Compile(program);

        Assert.False(compiled.IsValid);
        Assert.Contains(compiled.Errors, x => x.Contains("Repeating composite", StringComparison.Ordinal));
        Assert.Contains(compiled.Errors, x => x.Contains("unknown", StringComparison.Ordinal));
    }

    [Fact]
    public void CompilerAcceptsDerivedRepeatingCompositeBoundary()
    {
        var template = new RemediationTemplate(
            "invoice", "1", PdfUaProfile.PdfUa1,
            new RemediationTemplateNode("Document", children: new[]
            {
                new RemediationTemplateNode("Sect", "items", new[]
                {
                    new RemediationTemplateNode("P", "line")
                }, TemplateOccurrence.OneOrMore)
            }));
        var program = new RemediationProgram(
            "invoice-program", template,
            new[]
            {
                new BindingRule("line", BindingTarget.ToSlot(SlotRef.Absolute("/items/line")),
                    CandidateSelector.Text(Granularity.Line))
            });

        var compiled = RemediationProgramCompiler.Compile(program);

        Assert.True(compiled.IsValid, string.Join(Environment.NewLine, compiled.Errors));
        Assert.Equal("line", Assert.Single(compiled.Layers).Single().DefinitionId);
    }

    [Fact]
    public void FragmentMountsExpandToCanonicalPathsAndQualifiedBindings()
    {
        var fragment = new RemediationFragment(
            "address",
            new RemediationTemplateNode("Sect", children: new[]
            {
                new RemediationTemplateNode("P", "line")
            }),
            new[]
            {
                new BindingRule("line", BindingTarget.ToSlot(SlotRef.Relative("./line")),
                    CandidateSelector.Text(Granularity.Line))
            });
        var document = new RemediationTemplateNode(
            "Document",
            particles: new RemediationTemplateParticle[]
            {
                new RemediationFragmentMount("address", "billing"),
                new RemediationFragmentMount("address", "shipping")
            });
        var program = new RemediationProgram(
            "invoice-program",
            new RemediationTemplate("invoice", "1", PdfUaProfile.PdfUa1, document),
            Array.Empty<BindingRule>(),
            fragments: new[] { fragment });

        var compiled = RemediationProgramCompiler.Compile(program);

        Assert.True(compiled.IsValid, string.Join(Environment.NewLine, compiled.Errors));
        Assert.Contains(SlotRef.Absolute("/billing/line"), compiled.Slots.Keys);
        Assert.Contains(SlotRef.Absolute("/shipping/line"), compiled.Slots.Keys);
        Assert.Contains(compiled.Layers.SelectMany(x => x), x =>
            x.Id == "/billing::line" && x.DefinitionId == "line");
        Assert.Contains(compiled.Layers.SelectMany(x => x), x =>
            x.Id == "/shipping::line" && x.DefinitionId == "line");
    }

    [Fact]
    public void PreviewSerializationRoundTripsFragmentMounts()
    {
        var fragment = new RemediationFragment(
            "address",
            new RemediationTemplateNode("Sect", children: new[]
            {
                new RemediationTemplateNode("P", "line")
            }),
            new[]
            {
                new BindingRule("line", BindingTarget.ToSlot(SlotRef.Relative("./line")),
                    CandidateSelector.Text(Granularity.Line))
            });
        var program = new RemediationProgram(
            "invoice-program",
            new RemediationTemplate("invoice", "1", PdfUaProfile.PdfUa1,
                new RemediationTemplateNode("Document",
                    particles: new RemediationTemplateParticle[]
                    {
                        new RemediationFragmentMount("address", "billing")
                    })),
            Array.Empty<BindingRule>(),
            fragments: new[] { fragment });
        using var first = new MemoryStream();
        SerializedRemediationProgram.Save(program, first);
        first.Position = 0;

        var loaded = SerializedRemediationProgram.Load(first);
        using var second = new MemoryStream();
        SerializedRemediationProgram.Save(loaded, second);

        Assert.Equal(first.ToArray(), second.ToArray());
        var mount = Assert.IsType<RemediationFragmentMount>(
            Assert.Single(loaded.Template.Document.Particles));
        Assert.Equal("billing", mount.Alias);
    }

    [Fact]
    public void DerivedBoundaryPartitionsTwoSamePageOccurrences()
    {
        using var document = PdfDocument.Create();
        var page = document.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
            writer.Font(Standard14Font.GetHelvetica(), 12)
                .TextMove(50, 700).Text("First")
                .TextMove(50, 650).Text("Second")
                .EndText();
        var program = new RemediationProgram(
            "invoice-program",
            new RemediationTemplate("invoice", "1", PdfUaProfile.PdfUa1,
                new RemediationTemplateNode("Document", children: new[]
                {
                    new RemediationTemplateNode("Sect", "items", new[]
                    {
                        new RemediationTemplateNode("P", "line")
                    }, TemplateOccurrence.OneOrMore)
                })),
            new[]
            {
                new BindingRule("line", BindingTarget.ToSlot(SlotRef.Absolute("/items/line")),
                    CandidateSelector.Text(Granularity.Line))
            });
        using var session = document.BeginRemediation(new RemediationSessionConfiguration
        {
            StrictConformance = false
        });
        session.Use(program);

        var report = session.DryRun();

        Assert.Equal(2, report.OccurrencePartitions.Count);
        Assert.Equal(2, report.TemplateAssembly.Count(x =>
            x.ProgramSlot == SlotRef.Absolute("/items")));
        Assert.DoesNotContain(report.RuntimeDiagnostics, x => x.IsBlocking);
    }

    [Fact]
    public void CompilerReportsDuplicateBindingAndAnchorIdsWithoutThrowing()
    {
        var program = Program(
            new BindingRule("duplicate", BindingTarget.ToSlot(SlotRef.Absolute("/title")), CandidateSelector.Text(Granularity.Paragraph)),
            new BindingRule("duplicate", BindingTarget.ToSlot(SlotRef.Absolute("/body")), CandidateSelector.Text(Granularity.Paragraph)));
        program = new RemediationProgram(
            program.Id, program.Template, program.Bindings,
            new RemediationAnchor[]
            {
                RemediationAnchor.TextLabel("same", "A"),
                RemediationAnchor.TextLabel("same", "B")
            });

        var compiled = RemediationProgramCompiler.Compile(program);

        Assert.False(compiled.IsValid);
        Assert.Contains(compiled.Errors, x => x.Contains("Binding id 'duplicate'", StringComparison.Ordinal));
        Assert.Contains(compiled.Errors, x => x.Contains("Anchor id 'same'", StringComparison.Ordinal));
    }

    [Fact]
    public void SlotReferencesAreCanonicalAndPathScoped()
    {
        Assert.Equal("/billing/address/line", SlotRef.Parse("/billing/address/line").Path);
        Assert.Equal("./billing/address/line", SlotRef.Relative("./billing/address/line").Path);
        Assert.Throws<ArgumentException>(() => SlotRef.Absolute("./billing/line"));
        Assert.Throws<ArgumentException>(() => SlotRef.Relative("/billing/line"));
        Assert.Throws<ArgumentException>(() => SlotRef.Parse("billing/address/line"));
        Assert.Throws<ArgumentException>(() => SlotRef.Parse("/billing//line"));
        Assert.Throws<ArgumentException>(() => SlotRef.Parse("/billing/../line"));

        var program = Program(
            new BindingRule("billing", BindingTarget.ToSlot(SlotRef.Absolute("/billing/line")), CandidateSelector.Text(Granularity.Paragraph)),
            new BindingRule("shipping", BindingTarget.ToSlot(SlotRef.Absolute("/shipping/line")), CandidateSelector.Text(Granularity.Paragraph)));

        var compiled = RemediationProgramCompiler.Compile(program);

        Assert.True(compiled.IsValid, string.Join(Environment.NewLine, compiled.Errors));
        Assert.Contains(SlotRef.Absolute("/billing/line"), compiled.Slots.Keys);
        Assert.Contains(SlotRef.Absolute("/shipping/line"), compiled.Slots.Keys);
    }

    [Fact]
    public void JsonProgramRoundTripsThePrescriptiveCore()
    {
        var program = Program(
            new BindingRule(
                "title", BindingTarget.ToSlot(SlotRef.Absolute("/title")), CandidateSelector.Text(Granularity.Paragraph),
                Predicates.Text.Equals("Invoice")));
        using var stream = new MemoryStream();

        SerializedRemediationProgram.Save(program, stream);
        stream.Position = 0;
        var loaded = SerializedRemediationProgram.Load(stream);

        Assert.Equal("invoice-program", loaded.Id);
        Assert.Equal("/title", loaded.Bindings[0].Target is BindingTarget.Slot slot ? slot.Reference.Path : null);
        Assert.Equal("Invoice", Assert.IsType<TextRemediationPredicate>(loaded.Bindings[0].Predicate).Value);
    }

    [Fact]
    public void ProgramRunsThroughTheRemediationSession()
    {
        using var document = PdfDocument.Create();
        var page = document.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
            writer.Font(Standard14Font.GetHelvetica(), 12).Text("Invoice").EndText();

        var program = new RemediationProgram(
            "invoice-program",
            new RemediationTemplate("invoice", "1", PdfUaProfile.PdfUa1,
                new RemediationTemplateNode("Document", children: new[]
                {
                    new RemediationTemplateNode("H1", "title", properties: new RemediationNodeProperties(
                        Language: "en-US", AlternateText: "Invoice heading"))
                })),
            new[]
            {
                new BindingRule(
                    "title", BindingTarget.ToSlot(SlotRef.Absolute("/title")), CandidateSelector.Text(Granularity.Paragraph),
                    Predicates.Text.Equals("Invoice"), cardinality: BindingCardinality.Exactly(1))
            });
        using var session = document.BeginRemediation(new RemediationSessionConfiguration
        {
            StrictConformance = false
        });
        session.Use(program);

        var dryRun = session.DryRun();
        var bindingSummary = Assert.Single(dryRun.BindingEvaluations);
        Assert.Equal("title", bindingSummary.BindingId);
        Assert.Equal(SlotRef.Absolute("/title"), bindingSummary.ProgramSlot);
        var nativeClaim = Assert.Single(dryRun.Claims);
        Assert.Equal("title", nativeClaim.BindingId);
        Assert.Equal(SlotRef.Absolute("/title"), nativeClaim.ProgramSlot);
        Assert.DoesNotContain(dryRun.Diagnostics, x => x.Contains("unaccounted", StringComparison.OrdinalIgnoreCase));
        var committed = session.Commit();

        Assert.True(committed.Committed);
        var title = Assert.Single(session.Structure.GetRoot().Children);
        Assert.Equal("H1", title.Type);
        Assert.Equal("en-US", title.Language);
        Assert.Equal("Invoice heading", title.Alt);
        Assert.Contains("template:invoice@1", committed.TemplateAssembly.Single().Identity);
    }

    [Fact]
    public void SlotAssertionsAreEvaluatedAndAuthoringModeCannotCommit()
    {
        using var document = PdfDocument.Create();
        var page = document.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
            writer.Font(Standard14Font.GetHelvetica(), 12).Text("Invoice").EndText();

        var program = new RemediationProgram(
            "invoice-program",
            new RemediationTemplate("invoice", "1", PdfUaProfile.PdfUa1,
                new RemediationTemplateNode("Document", children: new[]
                {
                    new RemediationTemplateNode("H1", "title")
                })),
            new[]
            {
                new BindingRule("title", BindingTarget.ToSlot(SlotRef.Absolute("/title")),
                    CandidateSelector.Text(Granularity.Paragraph), Predicates.Text.Equals("Invoice"))
            },
            assertions: new[]
            {
                new SlotCountAssertion("title-count", SlotRef.Absolute("/title"), AssertionCount.Exactly(1))
            });

        using var session = document.BeginRemediation(new RemediationSessionConfiguration
        {
            StrictConformance = false,
            RunMode = RemediationRunMode.Authoring
        });
        session.Use(program);

        var report = session.DryRun();
        var outcome = Assert.Single(report.AssertionOutcomes);
        Assert.True(outcome.Passed);
        Assert.Throws<InvalidOperationException>(() => session.Commit());
    }

    [Fact]
    public void SlotAssertionCannotPassFromAnotherSlotWithTheSameTag()
    {
        using var document = PdfDocument.Create();
        var page = document.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
            writer.Font(Standard14Font.GetHelvetica(), 12).Text("Billing").EndText();

        var template = new RemediationTemplate(
            "invoice", "1", PdfUaProfile.PdfUa1,
            new RemediationTemplateNode("Document", children: new[]
            {
                new RemediationTemplateNode("P", "billing", occurrence: TemplateOccurrence.Optional),
                new RemediationTemplateNode("P", "shipping", occurrence: TemplateOccurrence.Optional)
            }));
        var program = new RemediationProgram(
            "invoice-program", template,
            new[]
            {
                new BindingRule("billing", BindingTarget.ToSlot(SlotRef.Absolute("/billing")),
                    CandidateSelector.Text(Granularity.Paragraph), Predicates.Text.Equals("Billing"))
            },
            assertions: new[]
            {
                new SlotCountAssertion("shipping-count", SlotRef.Absolute("/shipping"), AssertionCount.Exactly(1))
            });

        using var session = document.BeginRemediation(new RemediationSessionConfiguration { StrictConformance = false });
        session.Use(program);
        var outcome = Assert.Single(session.DryRun().AssertionOutcomes);

        Assert.False(outcome.Passed);
        Assert.Equal("/shipping", outcome.ProgramSlot?.Path);
        Assert.Equal("0", outcome.Observed);
    }

    [Fact]
    public void AuthoringWorkItemsArePromotedToEnforcedErrors()
    {
        var program = new RemediationProgram(
            "incomplete-program",
            new RemediationTemplate("incomplete", "1", PdfUaProfile.PdfUa1,
                new RemediationTemplateNode("Document", children: new[]
                {
                    new RemediationTemplateNode("H1", "title")
                })),
            new[]
            {
                new BindingRule("title", BindingTarget.ToSlot(SlotRef.Absolute("/title")),
                    CandidateSelector.Text(Granularity.Line), Predicates.Text.Equals("Missing"))
            });

        using var authoringDocument = PdfDocument.Create();
        authoringDocument.AddPage(PageSize.LETTER);
        using var authoring = authoringDocument.BeginRemediation(new RemediationSessionConfiguration
        {
            StrictConformance = false,
            RunMode = RemediationRunMode.Authoring
        }).Use(program);
        var authoringReport = authoring.DryRun();
        Assert.Contains(authoringReport.RuntimeDiagnostics, x =>
            x.Disposition == RemediationDiagnosticDisposition.WorkItem);
        Assert.DoesNotContain(authoringReport.RuntimeDiagnostics, x => x.IsBlocking);

        using var enforcedDocument = PdfDocument.Create();
        enforcedDocument.AddPage(PageSize.LETTER);
        using var enforced = enforcedDocument.BeginRemediation(new RemediationSessionConfiguration
        {
            StrictConformance = false,
            RunMode = RemediationRunMode.Enforced
        }).Use(program);
        var enforcedReport = enforced.DryRun();
        Assert.Contains(enforcedReport.RuntimeDiagnostics, x => x.IsBlocking);
    }

    [Fact]
    public void SourceOrderInversionProducesStructuredBlockingEvidence()
    {
        using var document = PdfDocument.Create();
        var page = document.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
        {
            writer.Font(Standard14Font.GetHelvetica(), 12)
                .TextMove(50, 650).Text("Body")
                .TextMove(50, 700).Text("Title")
                .EndText();
        }

        var program = new RemediationProgram(
            "order-program",
            new RemediationTemplate("order", "1", PdfUaProfile.PdfUa1,
                new RemediationTemplateNode("Document", children: new[]
                {
                    new RemediationTemplateNode("H1", "title"),
                    new RemediationTemplateNode("P", "body")
                })),
            new[]
            {
                new BindingRule("title", BindingTarget.ToSlot(SlotRef.Absolute("/title")),
                    CandidateSelector.Text(Granularity.Line), Predicates.Text.Equals("Title")),
                new BindingRule("body", BindingTarget.ToSlot(SlotRef.Absolute("/body")),
                    CandidateSelector.Text(Granularity.Line), Predicates.Text.Equals("Body"))
            });

        using var session = document.BeginRemediation(new RemediationSessionConfiguration
        {
            StrictConformance = false
        }).Use(program);
        var report = session.DryRun();

        var comparison = Assert.Single(report.OrderComparisons);
        Assert.True(comparison.SourceOrderInverted);
        Assert.False(comparison.GeometricOrderInverted);
        Assert.Equal(RemediationDiagnosticDisposition.Error, comparison.Disposition);
        Assert.Contains(report.RuntimeDiagnostics, x =>
            x.Code == DiagnosticCode.TemplateWrongOrder && x.IsBlocking);
    }

    [Fact]
    public void OccurrenceSelectorResolvesSameOccurrenceNthAndNearestPrevious()
    {
        using var document = PdfDocument.Create();
        var page = document.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
        {
            writer.Font(Standard14Font.GetHelvetica(), 12)
                .TextMove(40, 750).Text("Section 1")
                .TextMove(40, 720).Text("Section 1 Label")
                .TextMove(40, 700).Text("Section 1 Value")
                .TextMove(40, 650).Text("Section 2")
                .TextMove(40, 620).Text("Section 2 Label")
                .TextMove(40, 600).Text("Section 2 Value")
                .EndText();
        }

        var template = new RemediationTemplate(
            "repeated-sections", "1", PdfUaProfile.PdfUa1,
            new RemediationTemplateNode("Document", children: new[]
            {
                new RemediationTemplateNode("Sect", "section", new[]
                {
                    new RemediationTemplateNode("H1", "header"),
                    new RemediationTemplateNode("P", "label"),
                    new RemediationTemplateNode("P", "value")
                }, occurrence: TemplateOccurrence.OneOrMore, occurrenceBoundary: new OccurrenceBoundary.StartsOnSlot(SlotRef.Relative("./header")))
            }));

        var program = new RemediationProgram(
            "occurrence-selector-program",
            template,
            new[]
            {
                new BindingRule("header", BindingTarget.ToSlot(SlotRef.Absolute("/section/header")),
                    CandidateSelector.Text(Granularity.Line), Predicates.Text.Matches("^Section [0-9]+$")),
                new BindingRule("label", BindingTarget.ToSlot(SlotRef.Absolute("/section/label")),
                    CandidateSelector.Text(Granularity.Line), Predicates.Text.Contains("Label")),
                new BindingRule("value", BindingTarget.ToSlot(SlotRef.Absolute("/section/value")),
                    CandidateSelector.Text(Granularity.Line),
                    Predicates.Text.Contains("Value").And(Predicates.Anchor.Below("header-anchor", maxDistance: 150)))
            },
            new[]
            {
                new SlotAnchor("header-anchor", SlotOccurrenceRef.SameOccurrence(SlotRef.Absolute("/section/header")))
            });

        using var session = document.BeginRemediation(new RemediationSessionConfiguration { StrictConformance = false }).Use(program);
        var report = session.DryRun();

        Assert.DoesNotContain(report.RuntimeDiagnostics, x => x.IsBlocking);
        var valueClaims = report.Claims.Where(x => x.ProgramSlot?.Path == "/section/value").ToList();
        Assert.Equal(2, valueClaims.Count);

        // Also verify Nth selector
        var nthTemplate = new RemediationTemplate(
            "repeated-sections-nth", "1", PdfUaProfile.PdfUa1,
            new RemediationTemplateNode("Document", children: new[]
            {
                new RemediationTemplateNode("Sect", "section", new[]
                {
                    new RemediationTemplateNode("H1", "header"),
                    new RemediationTemplateNode("P", "label"),
                    new RemediationTemplateNode("P", "value", occurrence: TemplateOccurrence.Optional)
                }, occurrence: TemplateOccurrence.OneOrMore, occurrenceBoundary: new OccurrenceBoundary.StartsOnSlot(SlotRef.Relative("./header")))
            }));

        var nthProgram = new RemediationProgram(
            "nth-occurrence-program",
            nthTemplate,
            new[]
            {
                new BindingRule("header", BindingTarget.ToSlot(SlotRef.Absolute("/section/header")),
                    CandidateSelector.Text(Granularity.Line), Predicates.Text.Matches("^Section [0-9]+$")),
                new BindingRule("label", BindingTarget.ToSlot(SlotRef.Absolute("/section/label")),
                    CandidateSelector.Text(Granularity.Line), Predicates.Text.Contains("Label")),
                new BindingRule("value", BindingTarget.ToSlot(SlotRef.Absolute("/section/value")),
                    CandidateSelector.Text(Granularity.Line),
                    Predicates.Text.Contains("Value").And(Predicates.Anchor.Below("second-header-anchor", maxDistance: 150)))
            },
            new[]
            {
                new SlotAnchor("second-header-anchor", SlotOccurrenceRef.Nth(SlotRef.Absolute("/section/header"), 2))
            });

        using var nthSession = document.BeginRemediation(new RemediationSessionConfiguration { StrictConformance = false }).Use(nthProgram);
        var nthReport = nthSession.DryRun();
        var nthValueClaims = nthReport.Claims.Where(x => x.ProgramSlot?.Path == "/section/value").ToList();
        Assert.Single(nthValueClaims);
    }

    [Fact]
    public void ProgramEvaluatesMultipleSlotCountAssertions()
    {
        using var document = PdfDocument.Create();
        var page = document.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
        {
            writer.Font(Standard14Font.GetHelvetica(), 12)
                .TextMove(40, 750).Text("Document Title")
                .TextMove(40, 710).Text("Paragraph One")
                .TextMove(40, 690).Text("Paragraph Two")
                .EndText();
        }

        var program = new RemediationProgram(
            "assertions-program",
            new RemediationTemplate("assertions", "1", PdfUaProfile.PdfUa1,
                new RemediationTemplateNode("Document", children: new[]
                {
                    new RemediationTemplateNode("H1", "title"),
                    new RemediationTemplateNode("P", "paragraph", occurrence: TemplateOccurrence.ZeroOrMore)
                })),
            new[]
            {
                new BindingRule("title", BindingTarget.ToSlot(SlotRef.Absolute("/title")),
                    CandidateSelector.Text(Granularity.Line), Predicates.Text.Equals("Document Title")),
                new BindingRule("paragraph", BindingTarget.ToSlot(SlotRef.Absolute("/paragraph")),
                    CandidateSelector.Text(Granularity.Line), Predicates.Text.StartsWith("Paragraph"))
            },
            assertions: new[]
            {
                new SlotCountAssertion("title-count", SlotRef.Absolute("/title"), expected: AssertionCount.Exactly(1)),
                new SlotCountAssertion("paragraph-count", SlotRef.Absolute("/paragraph"), expected: new AssertionCount(1)),
                new SlotCountAssertion("failing-count", SlotRef.Absolute("/title"), expected: AssertionCount.Exactly(5))
            });

        using var session = document.BeginRemediation(new RemediationSessionConfiguration { StrictConformance = false }).Use(program);
        session.Suppress(DiagnosticCode.SemanticAssertionFailed, "*", "Testing failing assertion");
        var report = session.DryRun();

        Assert.Equal(3, report.AssertionOutcomes.Count);
        Assert.True(report.AssertionOutcomes.Single(x => x.AssertionId == "title-count").Passed);
        Assert.True(report.AssertionOutcomes.Single(x => x.AssertionId == "paragraph-count").Passed);
        var failed = report.AssertionOutcomes.Single(x => x.AssertionId == "failing-count");
        Assert.False(failed.Passed);
        Assert.Equal("1", failed.Observed);
        Assert.Equal("exactly 5", failed.Expected);
    }

    private static RemediationProgram Program(
        params BindingRule[] bindings) => Program(bindings, Array.Empty<RemediationAnchor>());
    private static RemediationProgram Program(
        IReadOnlyList<BindingRule> bindings,
        IReadOnlyList<RemediationAnchor> anchors)
    {
        var template = new RemediationTemplate(
            "invoice", "1", PdfUaProfile.PdfUa1,
            new RemediationTemplateNode("Document", children: new[]
            {
                new RemediationTemplateNode("H1", "title", occurrence: TemplateOccurrence.Optional),
                new RemediationTemplateNode("P", "body", occurrence: TemplateOccurrence.ZeroOrMore),
                new RemediationTemplateNode("Sect", "billing", children: new[]
                {
                    new RemediationTemplateNode("P", "line", occurrence: TemplateOccurrence.ZeroOrMore)
                }, occurrence: TemplateOccurrence.Optional),
                new RemediationTemplateNode("Sect", "shipping", children: new[]
                {
                    new RemediationTemplateNode("P", "line", occurrence: TemplateOccurrence.ZeroOrMore)
                }, occurrence: TemplateOccurrence.Optional)
            }));
        return new RemediationProgram("invoice-program", template, bindings, anchors);
    }
}
