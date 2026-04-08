using System.Text.Json;
using PdfLexer.Content;
using PdfLexer.Fonts;
using PdfLexer.TextLayout;
using PdfLexer.Writing;

namespace PdfLexer.TextLayout.FunctionalTests;

public partial class ChromePdfConformanceTests
{
    public static IEnumerable<object[]> AnalysisPlanWorkflowCases()
    {
        foreach (var caseName in new[]
        {
            "WrappedTextNarrow",
            "UnorderedListWrappedItems",
            "BasicTableConformance"
        })
        {
            yield return new object[] { caseName };
        }
    }

    [Theory]
    [MemberData(nameof(AnalysisPlanWorkflowCases))]
    public void PdfLexerAnalysisPlanWorkflow_GeneratesComparableArtifacts(string caseName)
    {
        var testCase = AllConformanceCases.Single(x => x.Name == caseName);
        var requestPdf = RenderWithPdfLexer(testCase);
        var planPdf = RenderWithPdfLexerPlan(testCase);

        var requestWords = ExtractWords(requestPdf);
        var planWords = ExtractWords(planPdf);

        var normalizedRequestWords = NormalizeMeasurementWords(requestWords);
        var normalizedPlanWords = NormalizeMeasurementWords(planWords);

        var resultsDir = GetResultsDirectory("analysis-plan-review");
        Directory.CreateDirectory(resultsDir);
        File.WriteAllBytes(Path.Combine(resultsDir, $"{caseName}.request.pdf"), requestPdf);
        File.WriteAllBytes(Path.Combine(resultsDir, $"{caseName}.plan.pdf"), planPdf);
        File.WriteAllText(
            Path.Combine(resultsDir, $"{caseName}.summary.json"),
            JsonSerializer.Serialize(
                new AnalysisPlanWorkflowSnapshot(
                    caseName,
                    requestWords.Count,
                    planWords.Count,
                    normalizedRequestWords.Count,
                    normalizedPlanWords.Count),
                SnapshotJsonOptions));

        Assert.Equal(BuildMeasurementWordCounts(normalizedRequestWords), BuildMeasurementWordCounts(normalizedPlanWords));
    }

    private static byte[] RenderWithPdfLexerPlan(ConformanceCase testCase)
    {
        var library = CreateFontLibrary(testCase.Fonts);

        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        page.MediaBox.LLx = 0;
        page.MediaBox.LLy = 0;
        page.MediaBox.URx = (decimal)testCase.PageWidth;
        page.MediaBox.URy = (decimal)testCase.PageHeight;

        var boxBottom = testCase.PageHeight - testCase.BoxTop - testCase.BoxHeight;
        using (var writer = page.GetWriter())
        {
            var area = new PdfRect<double>(testCase.BoxLeft, boxBottom, testCase.BoxLeft + testCase.BoxWidth, boxBottom + testCase.BoxHeight);
            if (testCase.Segments is not null)
            {
                var request = new TextBoxLayoutRequest(
                    testCase.BoxWidth,
                    testCase.BoxHeight,
                    library.CreateLayoutLibrary(),
                    testCase.Segments)
                {
                    HorizontalAlignment = testCase.HorizontalAlignment,
                    OverflowMode = TextOverflowMode.Clip,
                    BoxStyle = testCase.BoxStyle,
                    MetricPreference = testCase.MetricPreference
                };

                var plan = writer.AnalyzeTextBox(request);
                writer.WriteTextBox(area, plan, library);
            }
            else
            {
                var request = new RichTextBoxLayoutRequest(
                    testCase.BoxWidth,
                    testCase.BoxHeight,
                    library.CreateLayoutLibrary(),
                    testCase.Blocks!)
                {
                    OverflowMode = TextOverflowMode.Clip,
                    ListIndent = testCase.ListIndent,
                    ListMarkerGap = testCase.ListMarkerGap,
                    BoxStyle = testCase.BoxStyle,
                    MetricPreference = testCase.MetricPreference
                };

                var plan = writer.AnalyzeTextBox(request);
                writer.WriteTextBox(area, plan, library);
            }
        }

        return doc.Save();
    }

    private sealed record AnalysisPlanWorkflowSnapshot(
        string CaseName,
        int RequestWordCount,
        int PlanWordCount,
        int NormalizedRequestWordCount,
        int NormalizedPlanWordCount);

    [Fact]
    public void PdfLexerCompositionWorkflow_GeneratesFooterAndContinuationArtifacts()
    {
        var caseFonts = AllConformanceCases.Single(x => x.Name == "WrappedTextNarrow").Fonts;
        var library = CreateFontLibrary(caseFonts);
        var request = new TextBoxLayoutRequest(
            150,
            28,
            library.CreateLayoutLibrary(),
            new[]
            {
                new TextSegment("This is a wrapped sample that should span multiple lines in both engines.", new TextSegmentStyle(SansFamily, 400, 12, LineSpacing: 15))
            })
        {
            OverflowMode = TextOverflowMode.Clip
        };
        var footerRequest = new TextBoxLayoutRequest(
            150,
            20,
            library.CreateLayoutLibrary(),
            new[]
            {
                new TextSegment("Footer", new TextSegmentStyle(SansFamily, 400, 12, LineSpacing: 15))
            });
        var headerRequest = new TextBoxLayoutRequest(
            150,
            20,
            library.CreateLayoutLibrary(),
            new[]
            {
                new TextSegment("Continued", new TextSegmentStyle(SansFamily, 400, 12, LineSpacing: 15))
            });

        using var doc = PdfDocument.Create();
        var page1 = doc.AddPage(200, 200);
        var page2 = doc.AddPage(200, 200);
        TextLayoutPagePlan firstPagePlan;
        TextLayoutPagePlan secondPagePlan;

        using (var writer = page1.GetWriter())
        {
            var fitPlan = writer.AnalyzeTextBoxFit(request);
            var footerPlan = writer.AnalyzeTextBox(footerRequest);
            var headerPlan = writer.AnalyzeTextBox(headerRequest);

            firstPagePlan = fitPlan.ReplaceTrailingLinesWithFooter(footerPlan, 1);
            secondPagePlan = fitPlan.CreateContinuationPage(headerPlan);

            writer.WriteTextBox(new PdfRect<double>(20, 20, 170, 170), firstPagePlan, library);
        }

        using (var writer = page2.GetWriter())
        {
            writer.WriteTextBox(new PdfRect<double>(20, 20, 170, 170), secondPagePlan, library);
        }

        var resultsDir = GetResultsDirectory("analysis-plan-review");
        Directory.CreateDirectory(resultsDir);
        File.WriteAllBytes(Path.Combine(resultsDir, "CompositionWorkflow.page1.pdf"), doc.Save());
        File.WriteAllText(
            Path.Combine(resultsDir, "CompositionWorkflow.summary.json"),
            JsonSerializer.Serialize(
                new
                {
                    FirstPageLineCount = firstPagePlan.Layout.Lines.Count,
                    SecondPageLineCount = secondPagePlan.Layout.Lines.Count
                },
                SnapshotJsonOptions));

        Assert.NotEmpty(firstPagePlan.Layout.Lines);
        Assert.NotEmpty(secondPagePlan.Layout.Lines);
        Assert.Contains(secondPagePlan.Layout.Lines[0].Runs, x => x.Text.Contains("Continued", StringComparison.Ordinal));
    }

    [Fact]
    public void PdfLexerCompositionWorkflow_GeneratesTableContinuationArtifacts()
    {
        var caseFonts = AllConformanceCases.Single(x => x.Name == "BasicTableConformance").Fonts;
        var library = CreateFontLibrary(caseFonts);
        var style = new TextStyle(SansFamily, 400, 12);
        var request = new RichTextBoxLayoutRequest(
            150,
            48,
            library.CreateLayoutLibrary(),
            new RichTextBlock[]
            {
                new TableBlock(
                    Array.Empty<TableColumnDefinition>(),
                    new[]
                    {
                        new TableSectionBlock(TableSectionKind.Header, new[]
                        {
                            new TableRowBlock(new TableCellBlock[]
                            {
                                new TableHeaderCellBlock(new RichTextBlock[] { new ParagraphBlock(new InlineNode[] { new TextRunNode("Continued", style) }) })
                            })
                        }),
                        new TableSectionBlock(TableSectionKind.Body, new[]
                        {
                            new TableRowBlock(new TableCellBlock[]
                            {
                                new TableDataCellBlock(new RichTextBlock[] { new ParagraphBlock(new InlineNode[] { new TextRunNode("Row 1", style) }) })
                            }),
                            new TableRowBlock(new TableCellBlock[]
                            {
                                new TableDataCellBlock(new RichTextBlock[] { new ParagraphBlock(new InlineNode[] { new TextRunNode("Row 2", style) }) })
                            }),
                            new TableRowBlock(new TableCellBlock[]
                            {
                                new TableDataCellBlock(new RichTextBlock[] { new ParagraphBlock(new InlineNode[] { new TextRunNode("Row 3", style) }) })
                            })
                        })
                    },
                    Layout: new TableLayoutSpec(
                        Pagination: new TablePaginationPolicy(RepeatHeaderRows: true)))
            })
        {
            OverflowMode = TextOverflowMode.Clip
        };

        using var doc = PdfDocument.Create();
        var page1 = doc.AddPage(220, 220);
        var page2 = doc.AddPage(220, 220);
        TextLayoutFitPlan fitPlan;
        TextLayoutPagePlan secondPagePlan;

        using (var writer = page1.GetWriter())
        {
            fitPlan = writer.AnalyzeTextBoxFit(request);
            writer.WriteTextBox(new PdfRect<double>(20, 20, 190, 190), fitPlan, library);
        }

        using (var writer = page2.GetWriter())
        {
            secondPagePlan = fitPlan.CreateContinuationPage();
            writer.WriteTextBox(new PdfRect<double>(20, 20, 190, 190), secondPagePlan, library);
        }

        var resultsDir = GetResultsDirectory("analysis-plan-review");
        Directory.CreateDirectory(resultsDir);
        File.WriteAllBytes(Path.Combine(resultsDir, "TableContinuationWorkflow.pdf"), doc.Save());
        File.WriteAllText(
            Path.Combine(resultsDir, "TableContinuationWorkflow.summary.json"),
            JsonSerializer.Serialize(
                new
                {
                    ContinuationKinds = fitPlan.FittedSelection.Continuations.Select(x => x.Kind.ToString()).ToArray(),
                    SecondPageLineCount = secondPagePlan.Layout.Lines.Count
                },
                SnapshotJsonOptions));

        Assert.Contains(fitPlan.FittedSelection.Continuations, x => x.Kind == TextLayoutContinuationKind.TableRow);
        Assert.Contains(secondPagePlan.Layout.Lines.SelectMany(x => x.Runs), x => x.Text.Contains("Continued", StringComparison.Ordinal));
    }

    [Fact]
    public void PdfLexerFragmentWorkflow_GeneratesMultiPageArtifacts()
    {
        var caseFonts = AllConformanceCases.Single(x => x.Name == "WrappedTextNarrow").Fonts;
        var library = CreateFontLibrary(caseFonts);
        var request = new TextBoxLayoutRequest(
            150,
            28,
            library.CreateLayoutLibrary(),
            new[]
            {
                new TextSegment("This is a wrapped sample that should span multiple fragmentainer pages and preserve remainder flow semantics.", new TextSegmentStyle(SansFamily, 400, 12, LineSpacing: 15))
            })
        {
            OverflowMode = TextOverflowMode.Fragment
        };

        using var doc = PdfDocument.Create();
        var page1 = doc.AddPage(200, 200);
        var page2 = doc.AddPage(200, 200);
        TextLayoutFitPlan fitPlan;
        TextLayoutPagePlan secondPagePlan;

        using (var writer = page1.GetWriter())
        {
            fitPlan = writer.AnalyzeTextBoxFragment(request);
            writer.WriteTextBox(new PdfRect<double>(20, 20, 170, 170), fitPlan, library);
        }

        using (var writer = page2.GetWriter())
        {
            secondPagePlan = fitPlan.CreateContinuationPage();
            writer.WriteTextBox(new PdfRect<double>(20, 20, 170, 170), secondPagePlan, library);
        }

        var resultsDir = GetResultsDirectory("analysis-plan-review");
        Directory.CreateDirectory(resultsDir);
        File.WriteAllBytes(Path.Combine(resultsDir, "FragmentWorkflow.pdf"), doc.Save());
        File.WriteAllText(
            Path.Combine(resultsDir, "FragmentWorkflow.summary.json"),
            JsonSerializer.Serialize(
                new
                {
                    fitPlan.FragmentBreak.Reason,
                    fitPlan.FragmentBreak.BoundaryKind,
                    fitPlan.HasRemainder,
                    FirstPageLines = fitPlan.FittedPlan.Layout.Lines.Count,
                    SecondPageLines = secondPagePlan.Layout.Lines.Count
                },
                SnapshotJsonOptions));

        Assert.True(fitPlan.HasRemainder);
        Assert.Equal(TextFragmentBreakReason.Overflow, fitPlan.FragmentBreak.Reason);
        Assert.NotEmpty(secondPagePlan.Layout.Lines);
    }

    [Fact]
    public void PdfLexerFragmentWorkflow_GeneratesRepeatedTableHeaderFooterArtifacts()
    {
        var caseFonts = AllConformanceCases.Single(x => x.Name == "BasicTableConformance").Fonts;
        var library = CreateFontLibrary(caseFonts);
        var style = new TextStyle(SansFamily, 400, 12);
        var request = new RichTextBoxLayoutRequest(
            150,
            54,
            library.CreateLayoutLibrary(),
            new RichTextBlock[]
            {
                new TableBlock(
                    Array.Empty<TableColumnDefinition>(),
                    new[]
                    {
                        new TableSectionBlock(TableSectionKind.Header, new[]
                        {
                            new TableRowBlock(new TableCellBlock[]
                            {
                                new TableHeaderCellBlock(new RichTextBlock[] { new ParagraphBlock(new InlineNode[] { new TextRunNode("Header", style) }) })
                            })
                        }),
                        new TableSectionBlock(TableSectionKind.Body, new[]
                        {
                            new TableRowBlock(new TableCellBlock[]
                            {
                                new TableDataCellBlock(new RichTextBlock[] { new ParagraphBlock(new InlineNode[] { new TextRunNode("Body row 1", style) }) })
                            }),
                            new TableRowBlock(new TableCellBlock[]
                            {
                                new TableDataCellBlock(new RichTextBlock[] { new ParagraphBlock(new InlineNode[] { new TextRunNode("Body row 2", style) }) })
                            }),
                            new TableRowBlock(new TableCellBlock[]
                            {
                                new TableDataCellBlock(new RichTextBlock[] { new ParagraphBlock(new InlineNode[] { new TextRunNode("Body row 3", style) }) })
                            })
                        }),
                        new TableSectionBlock(TableSectionKind.Footer, new[]
                        {
                            new TableRowBlock(new TableCellBlock[]
                            {
                                new TableHeaderCellBlock(new RichTextBlock[] { new ParagraphBlock(new InlineNode[] { new TextRunNode("Footer", style) }) })
                            })
                        })
                    },
                    Layout: new TableLayoutSpec(
                        Pagination: new TablePaginationPolicy(RepeatHeaderRows: true, RepeatFooterRows: true)))
            })
        {
            OverflowMode = TextOverflowMode.Fragment
        };

        using var doc = PdfDocument.Create();
        var page1 = doc.AddPage(220, 220);
        var page2 = doc.AddPage(220, 220);
        TextLayoutFitPlan fitPlan;
        TextLayoutPagePlan secondPagePlan;

        using (var writer = page1.GetWriter())
        {
            fitPlan = writer.AnalyzeTextBoxFragment(request);
            writer.WriteTextBox(new PdfRect<double>(20, 20, 190, 190), fitPlan, library);
        }

        using (var writer = page2.GetWriter())
        {
            secondPagePlan = fitPlan.CreateContinuationPage();
            writer.WriteTextBox(new PdfRect<double>(20, 20, 190, 190), secondPagePlan, library);
        }

        var resultsDir = GetResultsDirectory("analysis-plan-review");
        Directory.CreateDirectory(resultsDir);
        File.WriteAllBytes(Path.Combine(resultsDir, "FragmentTableHeaderFooterWorkflow.pdf"), doc.Save());
        File.WriteAllText(
            Path.Combine(resultsDir, "FragmentTableHeaderFooterWorkflow.summary.json"),
            JsonSerializer.Serialize(
                new
                {
                    ContinuationKinds = fitPlan.FittedSelection.Continuations.Select(x => x.Kind.ToString()).ToArray(),
                    Page2Texts = secondPagePlan.Layout.Lines.SelectMany(x => x.Runs).Select(x => x.Text).ToArray()
                },
                SnapshotJsonOptions));

        Assert.Contains(secondPagePlan.Layout.Lines.SelectMany(x => x.Runs), x => x.Text.Contains("Header", StringComparison.Ordinal));
    }
}
