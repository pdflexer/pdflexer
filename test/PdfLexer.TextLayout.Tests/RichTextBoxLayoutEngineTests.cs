using DotNext.Collections.Generic;
using PdfLexer;
using PdfLexer.Content;
using PdfLexer.Fonts;
using PdfLexer.TextLayout;
using PdfLexer.Writing;
using System.Text.Json;

namespace PdfLexer.TextLayout.Tests;

public class RichTextBoxLayoutEngineTests
{
    private static readonly string RobotoPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../test/Roboto-Regular.ttf"));

    [Fact]
    public void MeasureTextBox_RichText_PreservesInlineStyleMetadata()
    {
        var style = new TextStyle(
            "Roboto",
            400,
            12,
            Italic: true,
            Underline: true,
            CharacterSpacing: 0.5,
            WordSpacing: 1.5,
            ForegroundColor: new TextColor(200, 10, 10),
            BackgroundColor: new TextColor(240, 240, 120),
            StrikeThrough: true);
        var request = CreateRequest(new RichTextBlock[]
        {
            new ParagraphBlock(
                new InlineNode[]
                {
                    new TextRunNode("Styled text", style)
                },
                new ParagraphStyle(LineHeight: 16))
        });

        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        using var writer = page.GetWriter();

        var result = writer.MeasureTextBox(request);

        Assert.All(result.Lines.Single().Runs, run =>
        {
            Assert.True(run.Italic);
            Assert.True(run.Underline);
            Assert.True(run.StrikeThrough);
            Assert.Equal(0.5d, run.CharacterSpacing, precision: 6);
            Assert.Equal(1.5d, run.WordSpacing, precision: 6);
            Assert.Equal(new TextColor(200, 10, 10), run.ForegroundColor);
            Assert.Equal(new TextColor(240, 240, 120), run.BackgroundColor);
            Assert.Equal(16d, run.LineHeight, precision: 6);
        });
    }

    [Fact]
    public void MeasureTextBox_RichText_AppliesParagraphMarginBlockStart()
    {
        var baseRequest = CreateRequest(new RichTextBlock[]
        {
            new ParagraphBlock(
                new InlineNode[]
                {
                    new TextRunNode("Indented block start", new TextStyle("Roboto", 400, 12))
                },
                new ParagraphStyle(LineHeight: 16))
        });

        var request = CreateRequest(new RichTextBlock[]
        {
            new ParagraphBlock(
                new InlineNode[]
                {
                    new TextRunNode("Indented block start", new TextStyle("Roboto", 400, 12))
                },
                new ParagraphStyle(LineHeight: 16, MarginBlockStart: 10))
        });

        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        using var writer = page.GetWriter();

        var baseResult = writer.MeasureTextBox(baseRequest);
        var result = writer.MeasureTextBox(request);

        Assert.Single(result.Lines);
        Assert.Equal(baseResult.Lines[0].BaselineY + 10d, result.Lines[0].BaselineY, precision: 6);
        Assert.Equal(baseResult.MeasuredHeight + 10d, result.MeasuredHeight, precision: 6);
    }

    [Fact]
    public void MeasureTextBox_RichText_ExpandsLineBox_WhenParagraphLineHeightIsSmallerThanNaturalHeight_ByDefault()
    {
        var request = CreateRequest(
            new RichTextBlock[]
            {
                new ParagraphBlock(
                    new InlineNode[]
                    {
                        new TextRunNode("Hello", new TextStyle("Roboto", 400, 24))
                    },
                    new ParagraphStyle(LineHeight: 12))
            }) with { Width = 200, Height = 200 };

        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        using var writer = page.GetWriter();

        var result = writer.MeasureTextBox(request);

        Assert.True(result.Success);
        Assert.Single(result.Lines);
        Assert.True(result.Lines[0].Height > 12d);
        Assert.Equal(result.Lines[0].Height, result.MeasuredHeight, precision: 6);
    }

    [Fact]
    public void MeasureTextBox_RichText_RespectsExactLineBoxSizing_WhenRequested()
    {
        var request = CreateRequest(
            new RichTextBlock[]
            {
                new ParagraphBlock(
                    new InlineNode[]
                    {
                        new TextRunNode("Hello", new TextStyle("Roboto", 400, 24))
                    },
                    new ParagraphStyle(LineHeight: 12, LineBoxSizing: TextLineBoxSizing.Exact))
            }) with { Width = 200, Height = 200 };

        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        using var writer = page.GetWriter();

        var result = writer.MeasureTextBox(request);

        Assert.True(result.Success);
        Assert.Single(result.Lines);
        Assert.Equal(12d, result.Lines[0].Height, precision: 6);
    }

    [Fact]
    public void Analyze_RichText_ExposesSemanticBlockPaths()
    {
        var request = CreateRequest(new RichTextBlock[]
        {
            new ParagraphBlock(
                new InlineNode[]
                {
                    new TextRunNode("Intro", new TextStyle("Roboto", 400, 12))
                }),
            new UnorderedListBlock(new[]
            {
                new ListItemBlock(new RichTextBlock[]
                {
                    new ParagraphBlock(
                        new InlineNode[]
                        {
                            new TextRunNode("Bullet item", new TextStyle("Roboto", 400, 12))
                        })
                })
            })
        });

        var engine = new RichTextBoxLayoutEngine();
        var plan = engine.Analyze(request);

        Assert.Equal(TextLayoutPlanKind.RichText, plan.Kind);
        Assert.Equal(TextLayoutNodeKind.Root, plan.Root.Kind);
        Assert.Equal(2, plan.Root.Children.Count);
        Assert.Equal("Blocks[0]", plan.Root.Children[0].Source.Path);
        Assert.Equal(TextLayoutNodeKind.Paragraph, plan.Root.Children[0].Kind);
        Assert.Equal("Blocks[1]", plan.Root.Children[1].Source.Path);
        Assert.Equal(TextLayoutNodeKind.UnorderedList, plan.Root.Children[1].Kind);
        var listItem = Assert.Single(plan.Root.Children[1].Children);
        Assert.Equal("Blocks[1].Items[0]", listItem.Source.Path);
        Assert.Equal(TextLayoutNodeKind.ListItem, listItem.Kind);
        Assert.Equal("Blocks[1].Items[0]", listItem.Source.StableNodeId);
        Assert.NotEqual(0, listItem.Source.ContentVersion);
        Assert.NotEqual(0, listItem.Source.StyleVersion);
    }

    [Fact]
    public void Analyze_RichText_AttachesRenderedLineNodes_ToParagraphBlocks()
    {
        var request = CreateRequest(new RichTextBlock[]
        {
            new ParagraphBlock(
                new InlineNode[]
                {
                    new TextRunNode("Alpha Beta Gamma Delta", new TextStyle("Roboto", 400, 12))
                },
                new ParagraphStyle(LineHeight: 14))
        }) with { Width = 90 };

        var plan = new RichTextBoxLayoutEngine().Analyze(request);

        var paragraph = Assert.Single(plan.Root.Children);
        Assert.Equal(TextLayoutNodeKind.Paragraph, paragraph.Kind);
        Assert.NotNull(paragraph.StartLineIndex);
        Assert.NotNull(paragraph.EndLineIndexExclusive);
        Assert.NotEmpty(paragraph.Children);

        Assert.All(paragraph.Children, child => Assert.Equal(TextLayoutNodeKind.Line, child.Kind));
        var line = paragraph.Children[0];
        Assert.NotEmpty(line.Children);
        Assert.All(line.Children, child =>
        {
            Assert.Equal(TextLayoutNodeKind.Run, child.Kind);
            Assert.StartsWith("Blocks[0].Inlines[0]", child.Source.Path, StringComparison.Ordinal);
        });
    }

    [Fact]
    public void Analyze_RichText_PreservesRenderedSourcePaths_InsideTableCells()
    {
        var request = CreateRequest(new RichTextBlock[]
        {
            new TableBlock(new[]
            {
                new TableRowBlock(new[]
                {
                    new TableDataCellBlock(new RichTextBlock[]
                    {
                        new ParagraphBlock(
                            new InlineNode[]
                            {
                                new TextRunNode("Cell text wraps", new TextStyle("Roboto", 400, 12))
                            },
                            new ParagraphStyle(LineHeight: 14))
                    })
                })
            }, new TableStyle(CellPadding: 4, CellBorderWidth: 1, CellBorderColor: new TextColor(80, 80, 80)))
        }) with { Width = 100 };

        var plan = new RichTextBoxLayoutEngine().Analyze(request);

        var table = Assert.Single(plan.Root.Children);
        var row = Assert.Single(table.Children);
        var cell = Assert.Single(row.Children);
        var paragraph = Assert.Single(cell.Children);
        var line = Assert.Single(paragraph.Children);

        Assert.Equal(TextLayoutNodeKind.Table, table.Kind);
        Assert.Equal(TextLayoutNodeKind.TableRow, row.Kind);
        Assert.Equal(TextLayoutNodeKind.TableCell, cell.Kind);
        Assert.Equal(TextLayoutNodeKind.Paragraph, paragraph.Kind);
        Assert.Equal(TextLayoutNodeKind.Line, line.Kind);
        Assert.NotEmpty(line.Children);
        Assert.All(line.Children, child =>
        {
            Assert.Equal(TextLayoutNodeKind.Run, child.Kind);
            Assert.StartsWith("Blocks[0].Rows[0].Cells[0].Blocks[0].Inlines[0]", child.Source.Path, StringComparison.Ordinal);
        });
    }

    [Fact]
    public void Analyze_RichText_WithReusableContext_PreservesExistingPlanBehavior()
    {
        var request = CreateRequest(new RichTextBlock[]
        {
            new ParagraphBlock(
                new InlineNode[]
                {
                    new TextRunNode("Intro text", new TextStyle("Roboto", 400, 12))
                }),
            new TableBlock(new[]
            {
                new TableRowBlock(new[]
                {
                    new TableDataCellBlock(new RichTextBlock[]
                    {
                        new ParagraphBlock(
                            new InlineNode[]
                            {
                                new TextRunNode("Cell text", new TextStyle("Roboto", 400, 12))
                            })
                    })
                })
            })
        });

        var engine = new RichTextBoxLayoutEngine();
        var context = new TextLayoutAnalysisContext();
        var plan = engine.Analyze(request, context);
        var repeatedPlan = engine.Analyze(request, context);
        var fitPlan = engine.AnalyzeFit(request with { Height = 30 }, context);

        Assert.Equal(TextLayoutPlanKind.RichText, plan.Kind);
        Assert.Equal(plan.Layout.NaturalSize, plan.Root.NaturalSize);
        Assert.Equal(plan.Layout.NaturalSize, repeatedPlan.Layout.NaturalSize);
        Assert.True(context.CachedIntrinsicMeasurementCount >= 1);
        Assert.True(context.CacheMissCount >= 1);
        Assert.True(context.CacheHitCount >= 1);
        Assert.NotNull(fitPlan.FittedPlan);
    }

    [Fact]
    public void Analyze_RichText_BlockFamilies_RouteThroughFormatterRegistry()
    {
        var request = CreateRequest(new RichTextBlock[]
        {
            new ParagraphBlock(new InlineNode[] { new TextRunNode("Paragraph", new TextStyle("Roboto", 400, 12)) }),
            new UnorderedListBlock(new[]
            {
                new ListItemBlock(new RichTextBlock[]
                {
                    new ParagraphBlock(new InlineNode[] { new TextRunNode("List item", new TextStyle("Roboto", 400, 12)) })
                })
            }),
            new TableBlock(new[]
            {
                new TableRowBlock(new TableCellBlock[]
                {
                    new TableDataCellBlock(new RichTextBlock[]
                    {
                        new ParagraphBlock(new InlineNode[] { new TextRunNode("Cell", new TextStyle("Roboto", 400, 12)) })
                    })
                })
            }, new TableStyle(CellBorderWidth: 0.5, CellPadding: 4)),
            new RowBlock(new[]
            {
                new LayoutChild(new RichTextBlock[]
                {
                    new ParagraphBlock(new InlineNode[] { new TextRunNode("Row child", new TextStyle("Roboto", 400, 12)) })
                })
            }, Style: new LayoutContainerStyle(Padding: 4)),
            new ColumnBlock(new[]
            {
                new LayoutChild(new RichTextBlock[]
                {
                    new ParagraphBlock(new InlineNode[] { new TextRunNode("Column child", new TextStyle("Roboto", 400, 12)) })
                })
            }, Style: new LayoutContainerStyle(Padding: 4))
        });

        var plan = new RichTextBoxLayoutEngine().Analyze(request);

        Assert.Collection(plan.Root.Children,
            node => Assert.Equal(TextLayoutNodeKind.Paragraph, node.Kind),
            node => Assert.Equal(TextLayoutNodeKind.UnorderedList, node.Kind),
            node => Assert.Equal(TextLayoutNodeKind.Table, node.Kind),
            node => Assert.Equal(TextLayoutNodeKind.RowContainer, node.Kind),
            node => Assert.Equal(TextLayoutNodeKind.ColumnContainer, node.Kind));
    }

    [Fact]
    public void Fit_RichParagraph_ReturnsFittingAndRemainderSlices()
    {
        var request = CreateRequest(new RichTextBlock[]
        {
            new ParagraphBlock(
                new InlineNode[]
                {
                    new TextRunNode("Alpha Beta Gamma Delta", new TextStyle("Roboto", 400, 12))
                },
                new ParagraphStyle(LineHeight: 14))
        }) with { Height = 20, Width = 70 };

        var engine = new RichTextBoxLayoutEngine();
        var result = engine.Fit(request);

        Assert.True(result.HasRemainder);
        Assert.Equal(TextBreakKind.Line, result.BreakKind);
        Assert.Single(result.FittingSlice.Blocks);
        var paragraph = Assert.IsType<ParagraphBlock>(result.FittingSlice.Blocks[0]);
        Assert.NotEmpty(paragraph.Inlines);
        Assert.NotNull(result.RemainderSlice);
        Assert.Single(result.RemainderSlice!.Blocks);
    }

    [Fact]
    public void Fit_RichParagraph_ExposesFittedLayout_AndContentAliases()
    {
        var request = CreateRequest(new RichTextBlock[]
        {
            new ParagraphBlock(
                new InlineNode[]
                {
                    new TextRunNode("Alpha Beta Gamma Delta", new TextStyle("Roboto", 400, 12))
                },
                new ParagraphStyle(LineHeight: 14))
        }) with { Height = 20, Width = 70 };

        var engine = new RichTextBoxLayoutEngine();
        var result = engine.Fit(request);

        Assert.Same(result.FittingLayout, result.FittedLayout);
        Assert.Same(result.FittingSlice, result.FittedContent);
        Assert.Same(result.RemainderSlice, result.RemainderContent);
        Assert.Equal(result.ConsumedWidth, result.ConsumedSize.Width, precision: 6);
        Assert.Equal(result.ConsumedHeight, result.ConsumedSize.Height, precision: 6);
        Assert.Equal(result.FittedLayout.MeasuredHeight, result.FittedLayout.NaturalHeight, precision: 6);
        Assert.Equal(result.FittedLayout.RenderedHeight, result.FittedLayout.VisibleHeight, precision: 6);
    }

    [Fact]
    public void AnalyzeFit_RichText_ExposesContinuationReferences()
    {
        var request = CreateRequest(new RichTextBlock[]
        {
            new ParagraphBlock(
                new InlineNode[]
                {
                    new TextRunNode("Alpha Beta Gamma Delta", new TextStyle("Roboto", 400, 12))
                },
                new ParagraphStyle(LineHeight: 14))
        }) with { Height = 20, Width = 70 };

        var fitPlan = new RichTextBoxLayoutEngine().AnalyzeFit(request);

        Assert.True(fitPlan.HasRemainder);
        Assert.Single(fitPlan.FittedSelection.Continuations);
        Assert.Equal(TextLayoutContinuationKind.Line, fitPlan.FittedSelection.Continuations[0].Kind);
        Assert.StartsWith("Blocks[0]", fitPlan.FittedSelection.Continuations[0].Boundary.Path, StringComparison.Ordinal);
        Assert.StartsWith("Blocks[0]", fitPlan.FittedSelection.Continuations[0].ContinuationStart?.Path ?? string.Empty, StringComparison.Ordinal);
    }

    [Fact]
    public void AnalyzeFit_RichText_ExposesSelections_AndMaterializers()
    {
        var request = CreateRequest(new RichTextBlock[]
        {
            new ParagraphBlock(
                new InlineNode[]
                {
                    new TextRunNode("Alpha Beta Gamma Delta", new TextStyle("Roboto", 400, 12))
                },
                new ParagraphStyle(LineHeight: 14))
        }) with { Height = 20, Width = 70 };

        var fitPlan = new RichTextBoxLayoutEngine().AnalyzeFit(request);

        Assert.True(fitPlan.HasRemainder);
        Assert.NotEmpty(fitPlan.FittedSelection.SourceReferences);
        Assert.Equal("Blocks[0]", fitPlan.FittedSelection.SourceReferences[0].Path);
        Assert.NotEmpty(fitPlan.FittedSelection.BoundaryReferences);
        Assert.NotNull(fitPlan.RemainderSelection);

        var fittedRequest = fitPlan.MaterializeFittedRequest(request);
        var remainderRequest = fitPlan.MaterializeRemainderRequest(request);

        Assert.NotNull(remainderRequest);
        Assert.Single(fittedRequest.Blocks);
        Assert.Single(remainderRequest!.Blocks);
        Assert.IsType<ParagraphBlock>(fittedRequest.Blocks[0]);
        Assert.IsType<ParagraphBlock>(remainderRequest.Blocks[0]);
    }

    [Fact]
    public void AnalyzeFit_RichText_BuildsFittedPlanFromSlicedAnalyzedTree()
    {
        var request = CreateRequest(new RichTextBlock[]
        {
            new ParagraphBlock(
                new InlineNode[]
                {
                    new TextRunNode("Alpha Beta Gamma Delta", new TextStyle("Roboto", 400, 12))
                },
                new ParagraphStyle(LineHeight: 14))
        }) with { Height = 20, Width = 70 };

        var engine = new RichTextBoxLayoutEngine();
        var analyzed = engine.Analyze(request with { Height = 200 });
        var fitPlan = engine.AnalyzeFit(request);

        Assert.True(analyzed.Layout.Lines.Count > fitPlan.FittedPlan.Layout.Lines.Count);
        var paragraph = Assert.Single(fitPlan.FittedPlan.Root.Children);
        Assert.Equal(TextLayoutNodeKind.Paragraph, paragraph.Kind);
        Assert.Equal(fitPlan.FittedPlan.Layout.Lines.Count, paragraph.Children.Count);
        Assert.All(paragraph.Children, line => Assert.Equal(TextLayoutNodeKind.Line, line.Kind));
    }

    [Fact]
    public void Fragment_RichText_WithClip_DoesNotEmitRemainder()
    {
        var request = CreateRequest(new RichTextBlock[]
        {
            new ParagraphBlock(
                new InlineNode[]
                {
                    new TextRunNode("Alpha Beta Gamma Delta", new TextStyle("Roboto", 400, 12))
                },
                new ParagraphStyle(LineHeight: 14))
        }) with { Height = 20, Width = 70, OverflowMode = TextOverflowMode.Clip };

        var result = new RichTextBoxLayoutEngine().Fragment(request);

        Assert.False(result.HasRemainder);
        Assert.Equal(TextFragmentBreakReason.None, result.FragmentBreak.Reason);
    }

    [Fact]
    public void AnalyzeFit_RichText_ExposesFragmentOverflowMetadata()
    {
        var request = CreateRequest(new RichTextBlock[]
        {
            new ParagraphBlock(
                new InlineNode[]
                {
                    new TextRunNode("Alpha Beta Gamma Delta", new TextStyle("Roboto", 400, 12))
                },
                new ParagraphStyle(LineHeight: 14))
        }) with { Height = 20, Width = 70, OverflowMode = TextOverflowMode.Fragment };

        var fitPlan = new RichTextBoxLayoutEngine().AnalyzeFit(request);

        Assert.True(fitPlan.HasRemainder);
        Assert.Equal(TextFragmentBreakReason.Overflow, fitPlan.FragmentBreak.Reason);
        Assert.Equal(TextFragmentBreakReason.Overflow, fitPlan.FittedSelection.FragmentMetadata.Break.Reason);
        Assert.Equal(TextFragmentBreakReason.Overflow, Assert.Single(fitPlan.FittedSelection.Continuations).BreakReason);
    }

    [Fact]
    public void AnalyzeFragment_RichText_WithClip_ProducesSingleFragmentPlan()
    {
        var request = CreateRequest(new RichTextBlock[]
        {
            new ParagraphBlock(
                new InlineNode[]
                {
                    new TextRunNode("Alpha Beta Gamma Delta", new TextStyle("Roboto", 400, 12))
                },
                new ParagraphStyle(LineHeight: 14))
        }) with { Height = 20, Width = 70, OverflowMode = TextOverflowMode.Clip };

        var fitPlan = new RichTextBoxLayoutEngine().AnalyzeFragment(request);

        Assert.False(fitPlan.HasRemainder);
        Assert.Null(fitPlan.RemainderSelection);
        Assert.Equal(TextFragmentBreakReason.None, fitPlan.FragmentBreak.Reason);
    }

    [Fact]
    public void RichText_VisibleClipAndFragmentOverflow_HaveDistinctSemantics()
    {
        var request = CreateRequest(new RichTextBlock[]
        {
            new ParagraphBlock(
                new InlineNode[]
                {
                    new TextRunNode("Alpha Beta Gamma Delta Epsilon Zeta Eta Theta", new TextStyle("Roboto", 400, 12))
                },
                new ParagraphStyle(LineHeight: 14))
        }) with { Height = 20, Width = 70, OverflowMode = TextOverflowMode.Visible };

        var engine = new RichTextBoxLayoutEngine();
        var visible = engine.Layout(request);
        var clip = engine.Fragment(request with { OverflowMode = TextOverflowMode.Clip });
        var fragment = engine.Fragment(request with { OverflowMode = TextOverflowMode.Fragment });

        Assert.True(visible.VisibleHeight > request.Height);
        Assert.False(clip.HasRemainder);
        Assert.True(clip.FittedLayout.VisibleHeight <= request.Height + 0.0001d);
        Assert.True(fragment.HasRemainder);
        Assert.True(fragment.FittedLayout.VisibleHeight <= request.Height + 0.0001d);
        Assert.Equal(TextFragmentBreakReason.Overflow, fragment.FragmentBreak.Reason);
    }

    [Fact]
    public void Fit_RichText_HonorsForcedBreakBefore()
    {
        var request = CreateRequest(new RichTextBlock[]
        {
            new ParagraphBlock(
                new InlineNode[]
                {
                    new TextRunNode("First paragraph", new TextStyle("Roboto", 400, 12))
                }),
            new ParagraphBlock(
                new InlineNode[]
                {
                    new TextRunNode("Second paragraph", new TextStyle("Roboto", 400, 12))
                })
            {
                BreakBefore = TextFlowBreakBefore.Always
            }
        }) with { Height = 200, Width = 120, OverflowMode = TextOverflowMode.Fragment };

        var result = new RichTextBoxLayoutEngine().Fit(request);

        Assert.True(result.HasRemainder);
        Assert.Single(result.FittedContent.Blocks);
        Assert.Single(result.RemainderContent!.Blocks);
        Assert.Equal(TextFragmentBreakReason.ForcedBreakBefore, result.FragmentBreak.Reason);
        Assert.True(result.FragmentBreak.IsForced);
    }

    [Fact]
    public void Fit_RichText_HonorsBreakInsideAvoid()
    {
        var request = CreateRequest(new RichTextBlock[]
        {
            new ParagraphBlock(
                new InlineNode[]
                {
                    new TextRunNode("Intro line.", new TextStyle("Roboto", 400, 12))
                }),
            new ParagraphBlock(
                new InlineNode[]
                {
                    new TextRunNode("Alpha Beta Gamma Delta Epsilon Zeta Eta Theta Iota Kappa Lambda", new TextStyle("Roboto", 400, 12))
                },
                new ParagraphStyle(LineHeight: 14))
            {
                BreakInside = TextFlowBreakInside.Avoid
            }
        }) with { Height = 28, Width = 90, OverflowMode = TextOverflowMode.Fragment };

        var result = new RichTextBoxLayoutEngine().Fit(request);

        Assert.True(result.HasRemainder);
        Assert.Single(result.FittedContent.Blocks);
        Assert.Single(result.RemainderContent!.Blocks);
        Assert.IsType<ParagraphBlock>(result.RemainderContent.Blocks[0]);
    }

    [Fact]
    public void FitPlan_CreateContinuationPage_UsesFragmentMetadata()
    {
        var request = CreateRequest(new RichTextBlock[]
        {
            new ParagraphBlock(
                new InlineNode[]
                {
                    new TextRunNode("Alpha Beta Gamma Delta", new TextStyle("Roboto", 400, 12))
                },
                new ParagraphStyle(LineHeight: 14))
        }) with { Height = 20, Width = 70, OverflowMode = TextOverflowMode.Fragment };

        var fitPlan = new RichTextBoxLayoutEngine().AnalyzeFragment(request);
        var continuedPage = fitPlan.CreateContinuationPage();

        Assert.NotEmpty(continuedPage.Layout.Lines);
        Assert.NotNull(fitPlan.FittedSelection.FragmentMetadata.RemainderReference);
    }

    [Fact]
    public void Fit_RichParagraphSplit_PreservesAllTextAcrossStyledRuns()
    {
        var request = CreateRequest(new RichTextBlock[]
        {
            new ParagraphBlock(
                new InlineNode[]
                {
                    new TextRunNode("This first paragraph is intentionally long and dense. ", new TextStyle("Roboto", 400, 12)),
                    new TextRunNode("Highlighted inline fragment", new TextStyle("Roboto", 400, 12, BackgroundColor: new TextColor(248, 240, 150))),
                    new TextRunNode(" stays embedded in the same flow to verify that browser extraction still agrees with the PdfLexer output when many words surround it.", new TextStyle("Roboto", 400, 12))
                },
                new ParagraphStyle(LineHeight: 16))
        }) with { Height = 52, Width = 200 };

        var engine = new RichTextBoxLayoutEngine();
        var result = engine.Fit(request);

        Assert.True(result.HasRemainder);
        var fittedParagraph = Assert.IsType<ParagraphBlock>(Assert.Single(result.FittingSlice.Blocks));
        var remainderParagraph = Assert.IsType<ParagraphBlock>(Assert.Single(result.RemainderSlice!.Blocks));

        var originalText = FlattenInlineText(request.Blocks.OfType<ParagraphBlock>().Single().Inlines);
        var splitText = FlattenInlineText(fittedParagraph.Inlines) + FlattenInlineText(remainderParagraph.Inlines);
        Assert.Equal(originalText, splitText);

        var fittingLayoutText = string.Concat(result.FittingLayout.Lines.SelectMany(x => x.Runs).Select(x => x.Text));
        Assert.Equal(FlattenInlineText(fittedParagraph.Inlines), fittingLayoutText);
    }

    [Fact]
    public void Fit_RichFlowBlocks_PreserveNullPublicStyleAcrossFormatterSplit()
    {
        var paragraphRequest = CreateRequest(new RichTextBlock[]
        {
            new ParagraphBlock(
                new InlineNode[]
                {
                    new TextRunNode("Alpha Beta Gamma Delta Epsilon Zeta Eta Theta", new TextStyle("Roboto", 400, 12))
                })
        }) with { Height = 20, Width = 90 };

        var headingRequest = CreateRequest(new RichTextBlock[]
        {
            new HeadingBlock(
                2,
                new InlineNode[]
                {
                    new TextRunNode("Heading Alpha Beta Gamma Delta Epsilon", new TextStyle("Roboto", 400, 12))
                })
        }) with { Height = 20, Width = 90 };

        var engine = new RichTextBoxLayoutEngine();

        var paragraphFit = engine.Fit(paragraphRequest);
        var fittedParagraph = Assert.IsType<ParagraphBlock>(Assert.Single(paragraphFit.FittingSlice.Blocks));
        var remainderParagraph = Assert.IsType<ParagraphBlock>(Assert.Single(paragraphFit.RemainderSlice!.Blocks));
        Assert.Null(fittedParagraph.Style);
        Assert.Null(remainderParagraph.Style);

        var headingFit = engine.Fit(headingRequest);
        var fittedHeading = Assert.IsType<HeadingBlock>(Assert.Single(headingFit.FittingSlice.Blocks));
        var remainderHeading = Assert.IsType<HeadingBlock>(Assert.Single(headingFit.RemainderSlice!.Blocks));
        Assert.Null(fittedHeading.Style);
        Assert.Null(remainderHeading.Style);
    }

    [Fact]
    public void Fit_UnorderedList_PreservesListStructureAndContentAcrossSplit()
    {
        var request = CreateRequest(new RichTextBlock[]
        {
            new UnorderedListBlock(new[]
            {
                new ListItemBlock(new RichTextBlock[]
                {
                    new ParagraphBlock(
                        new InlineNode[]
                        {
                            new TextRunNode("First unordered item wraps across multiple lines and should be split without losing its marker semantics.", new TextStyle("Roboto", 400, 12))
                        },
                        new ParagraphStyle(LineHeight: 16))
                }),
                new ListItemBlock(new RichTextBlock[]
                {
                    new ParagraphBlock(
                        new InlineNode[]
                        {
                            new TextRunNode("Second unordered item remains in the remainder.", new TextStyle("Roboto", 400, 12))
                        },
                        new ParagraphStyle(LineHeight: 16))
                })
            })
        }) with { Height = 40, Width = 150 };

        var engine = new RichTextBoxLayoutEngine();
        var result = engine.Fit(request);

        Assert.True(result.HasRemainder);
        var fittedList = Assert.IsType<UnorderedListBlock>(Assert.Single(result.FittingSlice.Blocks));
        var remainderList = Assert.IsType<UnorderedListBlock>(Assert.Single(result.RemainderSlice!.Blocks));
        Assert.NotEmpty(fittedList.Items);
        Assert.NotEmpty(remainderList.Items);

        static string FlattenList(UnorderedListBlock list)
            => string.Concat(
                list.Items
                    .SelectMany(item => item.Blocks)
                    .OfType<ParagraphBlock>()
                    .Select(block => FlattenInlineText(block.Inlines)));

        var original = FlattenList(Assert.IsType<UnorderedListBlock>(Assert.Single(request.Blocks)));
        var split = FlattenList(fittedList) + FlattenList(remainderList);
        Assert.Equal(original, split);
    }

    [Fact]
    public void MeasureTextBox_RichText_AutoOrderedListIndent_UsesWidestMarkerInList()
    {
        var style = new TextStyle("Roboto", 400, 12);
        var request = CreateRequest(new RichTextBlock[]
        {
            new OrderedListBlock(
                new[]
                {
                    new ListItemBlock(new RichTextBlock[]
                    {
                        new ParagraphBlock(new InlineNode[] { new TextRunNode("Alpha", style) })
                    }),
                    new ListItemBlock(new RichTextBlock[]
                    {
                        new ParagraphBlock(new InlineNode[] { new TextRunNode("Beta", style) })
                    })
                },
                StartIndex: 9)
        });

        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        using var writer = page.GetWriter();

        var result = writer.MeasureTextBox(request);

        Assert.Equal(2, result.Lines.Count);
        var firstLine = result.Lines[0];
        var secondLine = result.Lines[1];
        var firstContentRun = Assert.Single(firstLine.Runs.Where(x => x.Text == "Alpha"));
        var secondContentRun = Assert.Single(secondLine.Runs.Where(x => x.Text == "Beta"));
        Assert.Equal(firstLine.X + firstContentRun.X, secondLine.X + secondContentRun.X, precision: 3);
    }

    [Fact]
    public void MeasureTextBox_RichText_AutoUnorderedListIndent_GrowsWithMarkerFontSize()
    {
        var smallRequest = CreateRequest(new RichTextBlock[]
        {
            new UnorderedListBlock(new[]
            {
                new ListItemBlock(new RichTextBlock[]
                {
                    new ParagraphBlock(new InlineNode[] { new TextRunNode("Small", new TextStyle("Roboto", 400, 10)) })
                })
            })
        });

        var largeRequest = CreateRequest(new RichTextBlock[]
        {
            new UnorderedListBlock(new[]
            {
                new ListItemBlock(new RichTextBlock[]
                {
                    new ParagraphBlock(new InlineNode[] { new TextRunNode("Large", new TextStyle("Roboto", 400, 24)) })
                })
            })
        });

        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        using var writer = page.GetWriter();

        var smallResult = writer.MeasureTextBox(smallRequest);
        var largeResult = writer.MeasureTextBox(largeRequest);

        var smallContentRun = Assert.Single(Assert.Single(smallResult.Lines).Runs.Where(x => x.Text == "Small"));
        var largeContentRun = Assert.Single(Assert.Single(largeResult.Lines).Runs.Where(x => x.Text == "Large"));
        Assert.True(largeContentRun.X > smallContentRun.X);
    }

    [Fact]
    public void MeasureTextBox_RichText_ExplicitListIndentOverride_WinsOverAutoSizing()
    {
        var style = new TextStyle("Roboto", 400, 24);
        var autoRequest = CreateRequest(new RichTextBlock[]
        {
            new OrderedListBlock(new[]
            {
                new ListItemBlock(new RichTextBlock[]
                {
                    new ParagraphBlock(new InlineNode[] { new TextRunNode("Override", style) })
                })
            }, StartIndex: 10)
        });

        var explicitRequest = autoRequest with
        {
            ListIndent = 12d,
            ListMarkerGap = 3d
        };

        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        using var writer = page.GetWriter();

        var autoResult = writer.MeasureTextBox(autoRequest);
        var explicitResult = writer.MeasureTextBox(explicitRequest);

        var autoContentRun = Assert.Single(Assert.Single(autoResult.Lines).Runs.Where(x => x.Text == "Override"));
        var explicitContentRun = Assert.Single(Assert.Single(explicitResult.Lines).Runs.Where(x => x.Text == "Override"));
        Assert.True(autoContentRun.X > explicitContentRun.X);
        Assert.Equal(12d, explicitContentRun.X, precision: 3);
    }

    [Fact]
    public void Analyze_RichText_ListPlan_ExposesResolvedListDiagnostics()
    {
        var request = CreateRequest(new RichTextBlock[]
        {
            new OrderedListBlock(new[]
            {
                new ListItemBlock(new RichTextBlock[]
                {
                    new ParagraphBlock(new InlineNode[] { new TextRunNode("Alpha", new TextStyle("Roboto", 400, 12)) })
                }),
                new ListItemBlock(new RichTextBlock[]
                {
                    new ParagraphBlock(new InlineNode[] { new TextRunNode("Beta", new TextStyle("Roboto", 400, 12)) })
                })
            }, StartIndex: 10)
        });

        var plan = new RichTextBoxLayoutEngine().Analyze(request, new TextLayoutAnalysisContext().EnableLineDiagnostics());
        var listNode = Assert.Single(plan.Root.Children);
        var diagnostics = Assert.IsType<TextLayoutListDiagnostics>(listNode.ListDiagnostics);
        Assert.True(diagnostics.ContentStart > 0d);
        Assert.True(diagnostics.MarkerColumnWidth >= diagnostics.ContentStart);
        Assert.True(diagnostics.MarkerVisualWidth > 0d);
    }

    [Fact]
    public void Analyze_RichText_LineDiagnostics_AppearOnlyWhenRequested()
    {
        var request = CreateRequest(new RichTextBlock[]
        {
            new ParagraphBlock(
                new InlineNode[]
                {
                    new TextRunNode("Alpha Beta", new TextStyle("Roboto", 400, 12))
                },
                new ParagraphStyle(LineHeight: 10))
        });

        var engine = new RichTextBoxLayoutEngine();
        var withoutDiagnostics = engine.Analyze(request);
        Assert.All(withoutDiagnostics.Root.Children.SelectMany(static node => node.Children), static line => Assert.Null(line.LineDiagnostics));

        var withDiagnostics = engine.Analyze(request, new TextLayoutAnalysisContext().EnableLineDiagnostics());
        Assert.Contains(withDiagnostics.Root.Children.SelectMany(static node => node.Children), static line => line.LineDiagnostics is not null);
    }

    [Fact]
    public void MeasureTextBox_RichText_HandlesHeadings_ParagraphSpacing_AndOrderedLists()
    {
        var style = new TextStyle("Roboto", 400, 12);
        var request = CreateRequest(new RichTextBlock[]
        {
            new HeadingBlock(
                1,
                new InlineNode[]
                {
                    new TextRunNode("Heading", style)
                },
                new ParagraphStyle(TextAlign: TextHorizontalAlignment.Center, MarginBlockEnd: 6)),
            new ParagraphBlock(
                new InlineNode[]
                {
                    new TextRunNode("Body", style)
                },
                new ParagraphStyle(MarginBlockEnd: 8)),
            new OrderedListBlock(new[]
            {
                new ListItemBlock(new RichTextBlock[]
                {
                    new ParagraphBlock(
                        new InlineNode[]
                        {
                            new TextRunNode("First item", style)
                        })
                })
            })
        });

        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        using var writer = page.GetWriter();

        var result = writer.MeasureTextBox(request);

        Assert.Equal(3, result.Lines.Count);
        Assert.Equal("Heading", result.Lines[0].Runs.Single().Text);
        Assert.True(result.Lines[0].X > 0);
        Assert.Equal(0d, result.Lines[0].Runs.Single().X, precision: 6);
        Assert.Equal("Body", result.Lines[1].Runs.Single().Text);

        var listLine = result.Lines[2];
        Assert.Contains(listLine.Runs, x => x.Text == "1.");
        var markerRun = Assert.Single(listLine.Runs, x => x.Text == "1.");
        var contentRun = Assert.Single(listLine.Runs, x => x.Text == "First");
        Assert.True(contentRun.X > markerRun.X);
        Assert.True(result.Lines[1].BaselineY > result.Lines[0].BaselineY);
        Assert.True(result.Lines[2].BaselineY > result.Lines[1].BaselineY);
    }

    [Fact]
    public void MeasureTextBox_RichText_UsesBuiltInTextMarkerFace_ForUnorderedLists()
    {
        var request = CreateRequest(new RichTextBlock[]
        {
            new UnorderedListBlock(new[]
            {
                new ListItemBlock(new RichTextBlock[]
                {
                    new ParagraphBlock(
                        new InlineNode[]
                        {
                            new TextRunNode("Bullet item", new TextStyle("Roboto", 400, 12))
                        })
                })
            })
        });

        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        using var writer = page.GetWriter();

        var result = writer.MeasureTextBox(request);

        var listLine = Assert.Single(result.Lines);
        var markerRun = Assert.Single(listLine.Runs, x => x.Text == "\u2022");
        Assert.False(markerRun.DrawAsVectorBullet);
        Assert.Equal(TextLayoutBuiltInFaces.UnorderedListMarkerFaceId, markerRun.FaceId);
    }

    [Fact]
    public void Fit_Table_UsesContinuationRows_WhenSplit()
    {
        var style = new TextStyle("Roboto", 400, 12);
        var request = CreateRequest(new RichTextBlock[]
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
                    }),
                    new TableSectionBlock(TableSectionKind.Footer, new[]
                    {
                        new TableRowBlock(new TableCellBlock[]
                        {
                            new TableDataCellBlock(new RichTextBlock[] { new ParagraphBlock(new InlineNode[] { new TextRunNode("More", style) }) })
                        })
                    })
                },
                Layout: new TableLayoutSpec(
                    Pagination: new TablePaginationPolicy(RepeatHeaderRows: true, RepeatFooterRows: true)))
        }) with { Height = 90, Width = 120 };

        var engine = new RichTextBoxLayoutEngine();
        var result = engine.Fit(request);

        Assert.True(result.HasRemainder);
        var fittedTable = Assert.IsType<TableBlock>(Assert.Single(result.FittingSlice.Blocks));
        Assert.Contains(fittedTable.Rows.SelectMany(x => x.Cells).SelectMany(x => x.Blocks).OfType<ParagraphBlock>().SelectMany(x => x.Inlines).OfType<TextRunNode>(), x => x.Text == "Continued");
        Assert.Contains(fittedTable.Rows.SelectMany(x => x.Cells).SelectMany(x => x.Blocks).OfType<ParagraphBlock>().SelectMany(x => x.Inlines).OfType<TextRunNode>(), x => x.Text == "More");
        var remainderTable = Assert.IsType<TableBlock>(Assert.Single(result.RemainderSlice!.Blocks));
        Assert.Contains(remainderTable.Rows.SelectMany(x => x.Cells).SelectMany(x => x.Blocks).OfType<ParagraphBlock>().SelectMany(x => x.Inlines).OfType<TextRunNode>(), x => x.Text == "Continued");
        Assert.Contains(remainderTable.Rows.SelectMany(x => x.Cells).SelectMany(x => x.Blocks).OfType<ParagraphBlock>().SelectMany(x => x.Inlines).OfType<TextRunNode>(), x => x.Text == "More");
        Assert.Contains(result.SplitMetadata, x => x is TableSplitMetadata);
    }

    [Fact]
    public void Fit_Table_RowGroups_RepeatHeaderAndFooterAcrossFragments()
    {
        var style = new TextStyle("Roboto", 400, 12);
        var request = CreateRequest(new RichTextBlock[]
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
                    }),
                    new TableSectionBlock(TableSectionKind.Footer, new[]
                    {
                        new TableRowBlock(new TableCellBlock[]
                        {
                            new TableDataCellBlock(new RichTextBlock[] { new ParagraphBlock(new InlineNode[] { new TextRunNode("Footer", style) }) })
                        })
                    })
                },
                Layout: new TableLayoutSpec(
                    Pagination: new TablePaginationPolicy(RepeatHeaderRows: true, RepeatFooterRows: true)))
        }) with { Height = 60, Width = 120, OverflowMode = TextOverflowMode.Fragment };

        var result = new RichTextBoxLayoutEngine().Fit(request);

        Assert.True(result.HasRemainder);
        var fittedTable = Assert.IsType<TableBlock>(Assert.Single(result.FittedContent.Blocks));
        var remainderTable = Assert.IsType<TableBlock>(Assert.Single(result.RemainderContent!.Blocks));
        Assert.Contains(fittedTable.Rows.SelectMany(x => x.Cells).SelectMany(x => x.Blocks).OfType<ParagraphBlock>().SelectMany(x => x.Inlines).OfType<TextRunNode>(), x => x.Text == "Header");
        Assert.Contains(fittedTable.Rows.SelectMany(x => x.Cells).SelectMany(x => x.Blocks).OfType<ParagraphBlock>().SelectMany(x => x.Inlines).OfType<TextRunNode>(), x => x.Text == "Footer");
        Assert.Contains(remainderTable.Rows.SelectMany(x => x.Cells).SelectMany(x => x.Blocks).OfType<ParagraphBlock>().SelectMany(x => x.Inlines).OfType<TextRunNode>(), x => x.Text == "Header");
    }

    [Fact]
    public void Fit_RowBlock_SplitsAsWholeBlock_PreservingChildStructure()
    {
        var style = new TextStyle("Roboto", 400, 12);
        var request = CreateRequest(new RichTextBlock[]
        {
            new RowBlock(
                new[]
                {
                    new LayoutChild(new RichTextBlock[]
                    {
                        new ParagraphBlock(new InlineNode[] { new TextRunNode("Alpha Beta Gamma Delta", style) }, new ParagraphStyle(LineHeight: 14))
                    }, Weight: 2),
                    new LayoutChild(new RichTextBlock[]
                    {
                        new ParagraphBlock(new InlineNode[] { new TextRunNode("Side note content", style) }, new ParagraphStyle(LineHeight: 14))
                    }, Weight: 1)
                },
                Style: new LayoutContainerStyle(Gap: 8, Padding: 4))
        }) with { Height = 24, Width = 160 };

        var engine = new RichTextBoxLayoutEngine();
        var result = engine.Fit(request);

        Assert.True(result.HasRemainder);
        var fittedRow = Assert.IsType<RowBlock>(Assert.Single(result.FittingSlice.Blocks));
        Assert.Equal(2, fittedRow.Children.Count);
        var remainderRow = Assert.IsType<RowBlock>(Assert.Single(result.RemainderSlice!.Blocks));
        Assert.Equal(2, remainderRow.Children.Count);
        Assert.Equal(fittedRow.Children[0].Weight, remainderRow.Children[0].Weight);
    }

    [Fact]
    public void Fit_ColumnBlock_SplitsPreservingChildStructure()
    {
        var style = new TextStyle("Roboto", 400, 12);
        var request = CreateRequest(new RichTextBlock[]
        {
            new ColumnBlock(
                new[]
                {
                    new LayoutChild(new RichTextBlock[]
                    {
                        new ParagraphBlock(new InlineNode[] { new TextRunNode("Alpha Beta Gamma Delta Epsilon", style) }, new ParagraphStyle(LineHeight: 14))
                    }),
                    new LayoutChild(new RichTextBlock[]
                    {
                        new ParagraphBlock(new InlineNode[] { new TextRunNode("Second child content remains in the remainder when space runs out.", style) }, new ParagraphStyle(LineHeight: 14))
                    })
                },
                Style: new LayoutContainerStyle(Gap: 6, Padding: 4))
        }) with { Height = 34, Width = 160 };

        var engine = new RichTextBoxLayoutEngine();
        var result = engine.Fit(request);

        Assert.True(result.HasRemainder);
        var fittedColumn = Assert.IsType<ColumnBlock>(Assert.Single(result.FittingSlice.Blocks));
        var remainderColumn = Assert.IsType<ColumnBlock>(Assert.Single(result.RemainderSlice!.Blocks));
        Assert.NotEmpty(fittedColumn.Children);
        Assert.NotEmpty(remainderColumn.Children);
        Assert.Equal(2, remainderColumn.Children.Count);
    }

    [Fact]
    public void Fit_FixedHeightRow_WithOverflowingChildren_PushesRowToRemainder()
    {
        var style = new TextStyle("Roboto", 400, 12);
        var condensed = new TextStyle("Roboto Condensed", 400, 12);
        var mono = new TextStyle("Cascadia Mono", 400, 12);
        var request = CreateRequest(new RichTextBlock[]
        {
            new RowBlock(
                new[]
                {
                    new LayoutChild(
                        new RichTextBlock[]
                        {
                            new ParagraphBlock(
                                new InlineNode[]
                                {
                                    new TextRunNode("Primary column text should take roughly twice the width of the adjacent columns and wrap later.", style)
                                },
                                new ParagraphStyle(LineHeight: 16))
                        },
                        Weight: 2,
                        BoxStyle: new TextBoxStyle(BorderWidth: 1, Padding: 6)),
                    new LayoutChild(
                        new RichTextBlock[]
                        {
                            new ParagraphBlock(
                                new InlineNode[]
                                {
                                    new TextRunNode("Side one wraps sooner.", condensed)
                                },
                                new ParagraphStyle(LineHeight: 15))
                        },
                        Weight: 1,
                        BoxStyle: new TextBoxStyle(BorderWidth: 1, Padding: 6)),
                    new LayoutChild(
                        new RichTextBlock[]
                        {
                            new ParagraphBlock(
                                new InlineNode[]
                                {
                                    new TextRunNode("Side two also uses a narrower box.", mono)
                                },
                                new ParagraphStyle(LineHeight: 14))
                        },
                        Weight: 1,
                        BoxStyle: new TextBoxStyle(BorderWidth: 1, Padding: 6))
                },
                Height: 92,
                Style: new LayoutContainerStyle(BorderWidth: 1, Padding: 8, Gap: 8))
        }) with { Height = 92, Width = 190 };

        var engine = new RichTextBoxLayoutEngine();
        var result = engine.Fit(request);

        Assert.True(result.HasRemainder);
        Assert.Equal(TextBreakKind.ContainerChild, result.BreakKind);
        Assert.Empty(result.FittingSlice.Blocks);
        var remainderRow = Assert.IsType<RowBlock>(Assert.Single(result.RemainderSlice!.Blocks));
        Assert.Equal(3, remainderRow.Children.Count);
    }

    [Fact]
    public void WriteTextBox_RichText_WritesColorAndBackgroundOperators()
    {
        var face = CreateFace("roboto-regular", "Roboto", 400);
        var fontLibrary = new PdfTextLayoutFontLibrary(new[]
        {
            new PdfTextLayoutFontFace(face, TrueTypeFont.CreateWritableFont(File.ReadAllBytes(RobotoPath)))
        });
        var request = CreateRequest(new RichTextBlock[]
        {
            new ParagraphBlock(
                new InlineNode[]
                {
                    new TextRunNode(
                        "Color block",
                        new TextStyle(
                            "Roboto",
                            400,
                            12,
                            Underline: true,
                            ForegroundColor: new TextColor(255, 0, 0),
                            BackgroundColor: new TextColor(0, 255, 0)))
                },
                new ParagraphStyle(LineHeight: 16))
        }, fontLibrary.CreateLayoutLibrary());

        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        using (var writer = page.GetWriter())
        {
            writer.WriteTextBox(new PdfRect<double>(20, 20, 180, 180), request, fontLibrary);
        }

        var content = page.DumpDecodedContents();
        Assert.Contains("1 0 0 rg", content);
        Assert.Contains("0 1 0 rg", content);
        Assert.Contains(" re", content);
        Assert.Contains(" TJ", content);
    }

    [Fact]
    public void WriteTextBox_RichTextPlan_RendersWithoutReanalyzingRequest()
    {
        var face = CreateFace("roboto-regular", "Roboto", 400);
        var fontLibrary = new PdfTextLayoutFontLibrary(new[]
        {
            new PdfTextLayoutFontFace(face, TrueTypeFont.CreateWritableFont(File.ReadAllBytes(RobotoPath)))
        });
        var request = CreateRequest(new RichTextBlock[]
        {
            new ParagraphBlock(
                new InlineNode[]
                {
                    new TextRunNode("Plan based render path", new TextStyle("Roboto", 400, 12))
                },
                new ParagraphStyle(LineHeight: 16))
        }, fontLibrary.CreateLayoutLibrary());

        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        TextBoxLayoutResult result;
        using (var writer = page.GetWriter())
        {
            var plan = writer.AnalyzeTextBox(request);
            result = writer.WriteTextBox(new PdfRect<double>(20, 20, 180, 180), plan, fontLibrary);
            Assert.Same(plan.Layout, result);
            Assert.NotEmpty(result.Lines);
        }

        Assert.Contains(" TJ", page.DumpDecodedContents());
    }

    [Fact]
    public void WriteTextBox_RichTextFitPlan_RendersFittedPlan()
    {
        var face = CreateFace("roboto-regular", "Roboto", 400);
        var fontLibrary = new PdfTextLayoutFontLibrary(new[]
        {
            new PdfTextLayoutFontFace(face, TrueTypeFont.CreateWritableFont(File.ReadAllBytes(RobotoPath)))
        });
        var request = CreateRequest(new RichTextBlock[]
        {
            new ParagraphBlock(
                new InlineNode[]
                {
                    new TextRunNode("Alpha Beta Gamma Delta Epsilon", new TextStyle("Roboto", 400, 12))
                },
                new ParagraphStyle(LineHeight: 14))
        }, fontLibrary.CreateLayoutLibrary()) with { Width = 90, Height = 20 };

        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        using (var writer = page.GetWriter())
        {
            var fitPlan = writer.AnalyzeTextBoxFit(request);
            var result = writer.WriteTextBox(new PdfRect<double>(20, 20, 180, 180), fitPlan, fontLibrary);

            Assert.Same(fitPlan.FittedPlan.Layout, result);
            Assert.True(fitPlan.HasRemainder);
            Assert.NotEmpty(result.Lines);
        }
    }

    [Fact]
    public void PageComposer_ReplaceTrailingLinesWithFooter_ComposesAndRendersPagePlan()
    {
        var face = CreateFace("roboto-regular", "Roboto", 400);
        var fontLibrary = new PdfTextLayoutFontLibrary(new[]
        {
            new PdfTextLayoutFontFace(face, TrueTypeFont.CreateWritableFont(File.ReadAllBytes(RobotoPath)))
        });
        var request = CreateRequest(new RichTextBlock[]
        {
            new ParagraphBlock(
                new InlineNode[]
                {
                    new TextRunNode("Alpha Beta Gamma Delta Epsilon", new TextStyle("Roboto", 400, 12))
                },
                new ParagraphStyle(LineHeight: 14))
        }, fontLibrary.CreateLayoutLibrary()) with { Width = 90, Height = 20 };
        var footerRequest = CreateRequest(new RichTextBlock[]
        {
            new ParagraphBlock(
                new InlineNode[]
                {
                    new TextRunNode("Footer note", new TextStyle("Roboto", 400, 12))
                },
                new ParagraphStyle(LineHeight: 14))
        }, fontLibrary.CreateLayoutLibrary()) with { Width = 90, Height = 40 };

        var fitPlan = new RichTextBoxLayoutEngine().AnalyzeFit(request);
        var footerPlan = new RichTextBoxLayoutEngine().Analyze(footerRequest);
        var pagePlan = TextLayoutPageComposer.ReplaceTrailingLinesWithFooter(fitPlan.FittedSelection, footerPlan, trimmedLineCount: 1);

        Assert.Equal(2, pagePlan.Items.Count);
        Assert.Equal(TextLayoutPageItemRole.Footer, pagePlan.Items[1].Role);
        Assert.NotEmpty(pagePlan.Layout.Lines);
        Assert.Contains(pagePlan.Layout.Lines.Last().Runs, x => x.Text.Contains("Footer", StringComparison.Ordinal));

        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        using var writer = page.GetWriter();
        var result = writer.WriteTextBox(new PdfRect<double>(20, 20, 180, 180), pagePlan, fontLibrary);

        Assert.Same(pagePlan.Layout, result);
        Assert.NotEqual(TextLayoutStatus.Error, result.Status);
        Assert.NotEmpty(result.Lines);
    }

    [Fact]
    public void PageComposer_CreateContinuationPage_PrependsHeaderAndRendersRemainder()
    {
        var face = CreateFace("roboto-regular", "Roboto", 400);
        var fontLibrary = new PdfTextLayoutFontLibrary(new[]
        {
            new PdfTextLayoutFontFace(face, TrueTypeFont.CreateWritableFont(File.ReadAllBytes(RobotoPath)))
        });
        var request = CreateRequest(new RichTextBlock[]
        {
            new ParagraphBlock(
                new InlineNode[]
                {
                    new TextRunNode("Alpha Beta Gamma Delta Epsilon Zeta Eta Theta", new TextStyle("Roboto", 400, 12))
                },
                new ParagraphStyle(LineHeight: 14))
        }, fontLibrary.CreateLayoutLibrary()) with { Width = 90, Height = 20 };
        var headerRequest = CreateRequest(new RichTextBlock[]
        {
            new ParagraphBlock(
                new InlineNode[]
                {
                    new TextRunNode("Continued", new TextStyle("Roboto", 400, 12))
                },
                new ParagraphStyle(LineHeight: 14))
        }, fontLibrary.CreateLayoutLibrary()) with { Width = 90, Height = 40 };

        var fitPlan = new RichTextBoxLayoutEngine().AnalyzeFit(request);
        var headerPlan = new RichTextBoxLayoutEngine().Analyze(headerRequest);
        var continuation = Assert.Single(fitPlan.FittedSelection.Continuations);
        var remainderSelection = Assert.IsType<TextLayoutPlanSelection>(fitPlan.RemainderSelection);
        var pagePlan = TextLayoutPageComposer.CreateContinuationPage(remainderSelection, continuation, headerPlan);

        Assert.Equal(2, pagePlan.Items.Count);
        Assert.Equal(TextLayoutPageItemRole.Header, pagePlan.Items[0].Role);
        Assert.Contains(pagePlan.Layout.Lines[0].Runs, x => x.Text.Contains("Continued", StringComparison.Ordinal));

        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        using var writer = page.GetWriter();
        var result = writer.WriteTextBox(new PdfRect<double>(20, 20, 180, 180), pagePlan, fontLibrary);

        Assert.Same(pagePlan.Layout, result);
        Assert.NotEqual(TextLayoutStatus.Error, result.Status);
        Assert.NotEmpty(result.Lines);
    }

    [Fact]
    public void PageComposer_CreateContinuationPage_RejectsInvalidContinuationSelection()
    {
        var request = CreateRequest(new RichTextBlock[]
        {
            new ParagraphBlock(
                new InlineNode[]
                {
                    new TextRunNode("Alpha Beta Gamma Delta", new TextStyle("Roboto", 400, 12))
                },
                new ParagraphStyle(LineHeight: 14))
        }) with { Width = 70, Height = 20 };

        var fitPlan = new RichTextBoxLayoutEngine().AnalyzeFit(request);
        var continuation = Assert.Single(fitPlan.FittedSelection.Continuations);
        var unrelatedPlan = new RichTextBoxLayoutEngine().Analyze(CreateRequest(new RichTextBlock[]
        {
            new ParagraphBlock(
                new InlineNode[]
                {
                    new TextRunNode("Other document", new TextStyle("Roboto", 400, 12))
                },
                new ParagraphStyle(LineHeight: 14))
        }));

        var ex = Assert.Throws<TextLayoutCompositionException>(() =>
            TextLayoutPageComposer.CreateContinuationPage(
                new TextLayoutPlanSelection
                {
                    Plan = unrelatedPlan,
                    SourceReferences = unrelatedPlan.Root.Children.Select(x => x.Source).ToArray(),
                    BoundaryReferences = Array.Empty<TextLayoutSourceReference>(),
                    Continuations = Array.Empty<TextLayoutContinuationReference>(),
                    StartLineIndex = unrelatedPlan.Root.StartLineIndex,
                    EndLineIndexExclusive = unrelatedPlan.Root.EndLineIndexExclusive
                },
                continuation));

        Assert.Contains("not present", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void PageComposer_CreateContinuationPage_SupportsTableContinuationSelections()
    {
        var style = new TextStyle("Roboto", 400, 12);
        var request = CreateRequest(new RichTextBlock[]
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
        }) with { Height = 45, Width = 120 };

        var fitPlan = new RichTextBoxLayoutEngine().AnalyzeFit(request);
        var continuation = Assert.Single(fitPlan.FittedSelection.Continuations);
        var pagePlan = fitPlan.CreateContinuationPage();

        Assert.Equal(TextLayoutContinuationKind.TableRow, continuation.Kind);
        Assert.NotEmpty(pagePlan.Layout.Lines);
        Assert.Contains(pagePlan.Layout.Lines.SelectMany(x => x.Runs), x => x.Text.Contains("Continued", StringComparison.Ordinal));
    }

    [Fact]
    public void AnalyzeFit_TableMidCellOverflow_ExposesCellScopedFragmentContinuation()
    {
        var style = new TextStyle("Roboto", 400, 12);
        var request = CreateRequest(new RichTextBlock[]
        {
            new TableBlock(
                new[]
                {
                    new TableRowBlock(new TableCellBlock[]
                    {
                        new TableDataCellBlock(new RichTextBlock[]
                        {
                            new ParagraphBlock(
                                new InlineNode[]
                                {
                                    new TextRunNode("This is a long table cell that should fragment within the cell instead of pushing the entire row to the next fragment.", style)
                                },
                                new ParagraphStyle(LineHeight: 14))
                        })
                    }),
                    new TableRowBlock(new TableCellBlock[]
                    {
                        new TableDataCellBlock(new RichTextBlock[] { new ParagraphBlock(new InlineNode[] { new TextRunNode("Row 2", style) }) })
                    })
                })
        }) with { Height = 34, Width = 95, OverflowMode = TextOverflowMode.Fragment };

        var engine = new RichTextBoxLayoutEngine();
        var fitPlan = engine.AnalyzeFit(request);
        var fitResult = engine.Fit(request);
        var continuation = Assert.Single(fitPlan.FittedSelection.Continuations, x => x.Kind == TextLayoutContinuationKind.TableCell);
        var tableSplit = Assert.Single(fitResult.SplitMetadata.OfType<TableSplitMetadata>());

        Assert.True(fitPlan.HasRemainder);
        Assert.Equal(TextLayoutContinuationKind.TableCell, continuation.Kind);
        Assert.Equal(TextLayoutContinuationKind.TableCell, tableSplit.ContinuationKind);
        Assert.Equal("Blocks[0].Rows[0].Cells[0]", continuation.ParentPath);
        Assert.StartsWith("Blocks[0].Rows[0].Cells[0]", continuation.ContinuationStart?.Path ?? string.Empty, StringComparison.Ordinal);
        Assert.Equal(TextFragmentBreakReason.Overflow, fitPlan.FragmentBreak.Reason);
    }

    [Fact]
    public void AnalyzeFit_TableRowBoundaryOverflow_ExposesRowScopedFragmentContinuation()
    {
        var style = new TextStyle("Roboto", 400, 12);
        var request = CreateRequest(new RichTextBlock[]
        {
            new TableBlock(
                new[]
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
        }) with { Height = 40, Width = 120, OverflowMode = TextOverflowMode.Fragment };

        var engine = new RichTextBoxLayoutEngine();
        var fitPlan = engine.AnalyzeFit(request);
        var fitResult = engine.Fit(request);
        var continuation = Assert.Single(fitPlan.FittedSelection.Continuations);
        var tableSplit = Assert.Single(fitResult.SplitMetadata.OfType<TableSplitMetadata>());

        Assert.True(fitPlan.HasRemainder);
        Assert.Equal(TextLayoutContinuationKind.TableRow, continuation.Kind);
        Assert.Equal(TextLayoutContinuationKind.TableRow, tableSplit.ContinuationKind);
        Assert.Equal("Blocks[0].Rows[0]", continuation.ParentPath);
        Assert.StartsWith("Blocks[0].Rows[0]", continuation.ContinuationStart?.Path ?? string.Empty, StringComparison.Ordinal);
    }

    [Fact]
    public void PageComposer_CreateTableContinuationReferenceAtLine_DetectsRowBoundary()
    {
        var style = new TextStyle("Roboto", 400, 12);
        var request = CreateRequest(new RichTextBlock[]
        {
            new TableBlock(
                new[]
                {
                    new TableRowBlock(new TableCellBlock[]
                    {
                        new TableDataCellBlock(new RichTextBlock[] { new ParagraphBlock(new InlineNode[] { new TextRunNode("Row 1", style) }) })
                    }),
                    new TableRowBlock(new TableCellBlock[]
                    {
                        new TableDataCellBlock(new RichTextBlock[] { new ParagraphBlock(new InlineNode[] { new TextRunNode("Row 2", style) }) })
                    })
                })
        }) with { Width = 120 };

        var plan = new RichTextBoxLayoutEngine().Analyze(request);
        var selection = TextLayoutPageComposer.SelectAll(plan);

        var continuation = TextLayoutPageComposer.CreateTableContinuationReferenceAtLine(selection, 1);

        Assert.Equal(TextLayoutContinuationKind.TableRow, continuation.Kind);
        Assert.Equal("Blocks[0].Rows[1]", continuation.ParentPath);
        Assert.StartsWith("Blocks[0].Rows[1].Cells[0]", continuation.ContinuationStart?.Path ?? string.Empty, StringComparison.Ordinal);
    }

    [Fact]
    public void PageComposer_CreateTableContinuationReferenceAfterTrim_DetectsMidCellContinuation()
    {
        var style = new TextStyle("Roboto", 400, 12);
        var request = CreateRequest(new RichTextBlock[]
        {
            new TableBlock(
                new[]
                {
                    new TableRowBlock(new TableCellBlock[]
                    {
                        new TableDataCellBlock(new RichTextBlock[]
                        {
                            new ParagraphBlock(
                                new InlineNode[]
                                {
                                    new TextRunNode("This table cell wraps across several lines for continuation testing.", style)
                                },
                                new ParagraphStyle(LineHeight: 14))
                        })
                    })
                })
        }) with { Width = 90 };

        var plan = new RichTextBoxLayoutEngine().Analyze(request);
        var selection = TextLayoutPageComposer.SelectAll(plan);
        Assert.True(plan.Layout.Lines.Count >= 3);

        var continuation = TextLayoutPageComposer.CreateTableContinuationReferenceAfterTrim(selection, trimmedLineCount: 2);
        var pagePlan = TextLayoutPageComposer.CreateTableContinuationPage(selection, continuation);

        Assert.Equal(TextLayoutContinuationKind.TableCell, continuation.Kind);
        Assert.Equal("Blocks[0].Rows[0].Cells[0]", continuation.ParentPath);
        Assert.NotEmpty(pagePlan.Layout.Lines);
        Assert.True(pagePlan.Layout.Lines.Count < plan.Layout.Lines.Count);
    }

    [Fact]
    public void PageComposer_CreateTableContinuationPage_RejectsMismatchedTableCellParent()
    {
        var style = new TextStyle("Roboto", 400, 12);
        var request = CreateRequest(new RichTextBlock[]
        {
            new TableBlock(
                new[]
                {
                    new TableRowBlock(new TableCellBlock[]
                    {
                        new TableDataCellBlock(new RichTextBlock[]
                        {
                            new ParagraphBlock(
                                new InlineNode[]
                                {
                                    new TextRunNode("This table cell wraps across several lines for validation testing.", style)
                                },
                                new ParagraphStyle(LineHeight: 14))
                        })
                    })
                })
        }) with { Width = 90 };

        var plan = new RichTextBoxLayoutEngine().Analyze(request);
        var selection = TextLayoutPageComposer.SelectAll(plan);
        var continuation = TextLayoutPageComposer.CreateTableContinuationReferenceAfterTrim(selection, trimmedLineCount: 1) with
        {
            ParentPath = "Blocks[0].Rows[0]"
        };

        var ex = Assert.Throws<TextLayoutCompositionException>(() => TextLayoutPageComposer.CreateTableContinuationPage(selection, continuation));

        Assert.Contains("table cell", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Analyze_Inspect_ModifySource_AndRenderAgain_UsesStableSourcePaths()
    {
        var face = CreateFace("roboto-regular", "Roboto", 400);
        var fontLibrary = new PdfTextLayoutFontLibrary(new[]
        {
            new PdfTextLayoutFontFace(face, TrueTypeFont.CreateWritableFont(File.ReadAllBytes(RobotoPath)))
        });
        var original = CreateRequest(new RichTextBlock[]
        {
            new ParagraphBlock(
                new InlineNode[]
                {
                    new TextRunNode("Original paragraph", new TextStyle("Roboto", 400, 12))
                },
                new ParagraphStyle(LineHeight: 16))
        }, fontLibrary.CreateLayoutLibrary());

        var engine = new RichTextBoxLayoutEngine();
        var firstPlan = engine.Analyze(original);
        Assert.Equal("Blocks[0]", Assert.Single(firstPlan.Root.Children).Source.Path);

        var modified = original with
        {
            Blocks = original.Blocks.Concat(new RichTextBlock[]
            {
                new ParagraphBlock(
                    new InlineNode[]
                    {
                        new TextRunNode("Inserted paragraph", new TextStyle("Roboto", 400, 12))
                    },
                    new ParagraphStyle(LineHeight: 16))
            }).ToArray()
        };

        var secondPlan = engine.Analyze(modified);
        Assert.Equal(2, secondPlan.Root.Children.Count);
        Assert.Equal("Blocks[0]", secondPlan.Root.Children[0].Source.Path);
        Assert.Equal("Blocks[1]", secondPlan.Root.Children[1].Source.Path);

        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        using (var writer = page.GetWriter())
        {
            writer.WriteTextBox(new PdfRect<double>(20, 20, 180, 180), secondPlan, fontLibrary);
        }

        var content = page.DumpDecodedContents();
        Assert.Contains(" TJ", content);
    }

    [Fact]
    public void AnalyzePlanWorkflow_RepeatedPasses_StayWithinAllocationEnvelope()
    {
        var face = CreateFace("roboto-regular", "Roboto", 400);
        var fontLibrary = new PdfTextLayoutFontLibrary(new[]
        {
            new PdfTextLayoutFontFace(face, TrueTypeFont.CreateWritableFont(File.ReadAllBytes(RobotoPath)))
        });
        var request = CreateRequest(new RichTextBlock[]
        {
            new ParagraphBlock(
                new InlineNode[]
                {
                    new TextRunNode("A repeated analysis and render loop should avoid runaway allocations even before the deeper range-based internals are fully optimized.", new TextStyle("Roboto", 400, 12))
                },
                new ParagraphStyle(LineHeight: 16)),
            new UnorderedListBlock(new[]
            {
                new ListItemBlock(new RichTextBlock[]
                {
                    new ParagraphBlock(
                        new InlineNode[]
                        {
                            new TextRunNode("First bullet item with enough content to wrap at least once.", new TextStyle("Roboto", 400, 12))
                        },
                        new ParagraphStyle(LineHeight: 16))
                }),
                new ListItemBlock(new RichTextBlock[]
                {
                    new ParagraphBlock(
                        new InlineNode[]
                        {
                            new TextRunNode("Second bullet item keeps the workflow representative of normal rich content.", new TextStyle("Roboto", 400, 12))
                        },
                        new ParagraphStyle(LineHeight: 16))
                })
            })
        }, fontLibrary.CreateLayoutLibrary()) with { Width = 180, Height = 120 };

        using var warmupDoc = PdfDocument.Create();
        var warmupPage = warmupDoc.AddPage();
        using (var warmupWriter = warmupPage.GetWriter())
        {
            var warmupPlan = warmupWriter.AnalyzeTextBox(request);
            warmupWriter.WriteTextBox(new PdfRect<double>(20, 20, 180, 180), warmupPlan, fontLibrary);
        }

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        long startBytes = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 12; i++)
        {
            using var doc = PdfDocument.Create();
            var page = doc.AddPage();
            using var writer = page.GetWriter();
            var plan = writer.AnalyzeTextBox(request);
            writer.WriteTextBox(new PdfRect<double>(20, 20, 180, 180), plan, fontLibrary);
        }

        var allocatedBytes = GC.GetAllocatedBytesForCurrentThread() - startBytes;
        Assert.InRange(allocatedBytes, 1, 35_000_000);
    }

    [Fact]
    public void WriteTextBox_RichText_UsesHarfBuzzPositioning_InsteadOfNativePdfSpacing()
    {
        var face = CreateFace("roboto-regular", "Roboto", 400);
        var fontLibrary = new PdfTextLayoutFontLibrary(new[]
        {
            new PdfTextLayoutFontFace(face, TrueTypeFont.CreateWritableFont(File.ReadAllBytes(RobotoPath)))
        });
        var request = CreateRequest(new RichTextBlock[]
        {
            new ParagraphBlock(
                new InlineNode[]
                {
                    new TextRunNode(
                        "A spaced paragraph with words",
                        new TextStyle(
                            "Roboto",
                            400,
                            12,
                            CharacterSpacing: 0.5,
                            WordSpacing: 1.5))
                },
                new ParagraphStyle(LineHeight: 16))
        }, fontLibrary.CreateLayoutLibrary());

        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        using (var writer = page.GetWriter())
        {
            writer.WriteTextBox(new PdfRect<double>(20, 20, 180, 180), request, fontLibrary);
        }

        var content = page.DumpDecodedContents();
        Assert.DoesNotContain("0.5 Tc", content);
        Assert.DoesNotContain("1.5 Tw", content);
        Assert.Contains(" TJ", content);
    }

    [Fact]
    public void WriteTextBox_RichText_PreservesMeasuredCharacterAdvances()
    {
        var face = CreateFace("roboto-regular", "Roboto", 400);
        var fontLibrary = new PdfTextLayoutFontLibrary(new[]
        {
            new PdfTextLayoutFontFace(face, TrueTypeFont.CreateWritableFont(File.ReadAllBytes(RobotoPath)))
        });
        var request = CreateRequest(new RichTextBlock[]
        {
            new ParagraphBlock(
                new InlineNode[]
                {
                    new TextRunNode(
                        "A spaced paragraph with words",
                        new TextStyle(
                            "Roboto",
                            400,
                            12,
                            CharacterSpacing: 0.5,
                            WordSpacing: 1.5))
                },
                new ParagraphStyle(LineHeight: 16))
        }, fontLibrary.CreateLayoutLibrary()) with
        {
            Width = 400,
            Height = 80,
            OverflowMode = TextOverflowMode.Visible
        };

        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        TextBoxLayoutResult result;
        using (var writer = page.GetWriter())
        {
            result = writer.WriteTextBox(new PdfRect<double>(20, 20, 420, 180), request, fontLibrary);
        }

        var line = Assert.Single(result.Lines);
        var expectedPositions = line.Runs
            .SelectMany(run => run.Glyphs.Select(g => line.X + run.X + g.X))
            .ToArray();

        var actualPositions = new List<double>();
        var scanner = page.GetTextScanner();
        while (scanner.Advance())
        {
            actualPositions.AddRange(scanner.EnumerateCharacters().Select(x => x.XPos - 20d));
        }

        Assert.Equal(expectedPositions.Length, actualPositions.Count);
        for (var i = 0; i < expectedPositions.Length; i++)
        {
            Assert.Equal(expectedPositions[i], actualPositions[i], precision: 1);
        }
    }

    [Fact]
    public void WriteTextBox_RichText_UnorderedListWrappedWord_UsesMeasuredGlyphPositions()
    {
        var face = CreateFace("roboto-regular", "Roboto", 400);
        var fontLibrary = new PdfTextLayoutFontLibrary(new[]
        {
            new PdfTextLayoutFontFace(face, TrueTypeFont.CreateWritableFont(File.ReadAllBytes(RobotoPath)))
        });
        var request = CreateRequest(new RichTextBlock[]
        {
            new UnorderedListBlock(new[]
            {
                new ListItemBlock(new RichTextBlock[]
                {
                    new ParagraphBlock(
                        new InlineNode[]
                        {
                            new TextRunNode("First unordered item wraps to a second line and verifies the bullet gap against the content column.", new TextStyle("Roboto", 400, 12))
                        },
                        new ParagraphStyle(LineHeight: 16))
                })
            })
        }, fontLibrary.CreateLayoutLibrary()) with
        {
            Width = 182,
            Height = 118,
            OverflowMode = TextOverflowMode.Visible
        };

        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        TextBoxLayoutResult result;
        using (var writer = page.GetWriter())
        {
            result = writer.WriteTextBox(new PdfRect<double>(20, 20, 202, 180), request, fontLibrary);
        }

        var expectedRun = result.Lines
            .SelectMany(line => line.Runs.Select(run => (Line: line, Run: run)))
            .Single(x => x.Run.Text == "verifies");
        var expectedPositions = expectedRun.Run.Glyphs
            .Select(g => expectedRun.Line.X + expectedRun.Run.X + g.X)
            .ToArray();

        var actualChars = new List<(char Char, double X)>();
        var scanner = page.GetTextScanner();
        while (scanner.Advance())
        {
            actualChars.AddRange(scanner.EnumerateCharacters().Select(x => (x.Char, x.XPos - 20d)));
        }

        const string word = "verifies";
        var startIndex = FindSequence(actualChars.Select(x => x.Char).ToArray(), word);
        Assert.True(startIndex >= 0, $"Could not find '{word}' in extracted text.");

        var actualPositions = actualChars
            .Skip(startIndex)
            .Take(word.Length)
            .Select(x => x.X)
            .ToList();

        actualPositions.RemoveAt(5); // i ligature

        Assert.Equal(expectedPositions.Length, actualPositions.Count);
        for (var i = 0; i < expectedPositions.Length; i++)
        {
            Assert.Equal(expectedPositions[i], actualPositions[i], precision: 1);
        }
    }

    [Fact]
    public void WriteTextBox_RichText_UnorderedListMarkers_AreExtractedAsText()
    {
        var face = CreateFace("roboto-regular", "Roboto", 400);
        var fontLibrary = new PdfTextLayoutFontLibrary(new[]
        {
            new PdfTextLayoutFontFace(face, TrueTypeFont.CreateWritableFont(File.ReadAllBytes(RobotoPath)))
        });
        var request = CreateRequest(new RichTextBlock[]
        {
            new UnorderedListBlock(new[]
            {
                new ListItemBlock(new RichTextBlock[]
                {
                    new ParagraphBlock(
                        new InlineNode[]
                        {
                            new TextRunNode("Bullet item", new TextStyle("Roboto", 400, 12))
                        })
                })
            })
        }, fontLibrary.CreateLayoutLibrary());

        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        using (var writer = page.GetWriter())
        {
            writer.WriteTextBox(new PdfRect<double>(20, 20, 180, 180), request, fontLibrary);
        }

        var scanner = page.GetWordScanner();
        var words = new List<string>();
        while (scanner.Advance())
        {
            words.Add(scanner.CurrentWord);
        }

        Assert.Contains("\u2022", words);
        Assert.Contains("Bullet", words);
    }

    [Fact]
    public void MeasureTextBox_RichText_UnorderedListMarker_IsNotPinnedToTextEdge()
    {
        var request = CreateRequest(new RichTextBlock[]
        {
            new UnorderedListBlock(new[]
            {
                new ListItemBlock(new RichTextBlock[]
                {
                    new ParagraphBlock(
                        new InlineNode[]
                        {
                            new TextRunNode("Bullet item", new TextStyle("Roboto", 400, 12))
                        })
                })
            })
        });

        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        using var writer = page.GetWriter();

        var result = writer.MeasureTextBox(request);
        var line = Assert.Single(result.Lines);
        var marker = Assert.Single(line.Runs.Where(x => x.Text == "\u2022"));
        var text = Assert.Single(line.Runs.Where(x => x.Text == "Bullet"));

        var markerRight = line.X + marker.X + marker.MeasuredWidth;
        var textLeft = line.X + text.X;
        var markerCenter = line.X + marker.X + (marker.MeasuredWidth / 2d);

        Assert.True(textLeft - markerRight > 3d);
        Assert.InRange(markerCenter, 8d, 12d);
    }

    [Fact]
    public void MeasureTextBox_RichText_Includes_BoxInset_InRunPositions()
    {
        var request = CreateRequest(new RichTextBlock[]
        {
            new ParagraphBlock(
                new InlineNode[]
                {
                    new TextRunNode("Inset text", new TextStyle("Roboto", 400, 12))
                })
        })
            with { BoxStyle = new TextBoxStyle(BorderWidth: 2, Padding: 10) };

        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        using var writer = page.GetWriter();

        var result = writer.MeasureTextBox(request);

        var line = Assert.Single(result.Lines);
        var run = Assert.Single(line.Runs, x => x.Text == "Inset");
        Assert.Equal(12d, line.X, precision: 6);
        Assert.Equal(0d, run.X, precision: 6);
        Assert.Equal(24d, line.BaselineY, precision: 6);
        Assert.Equal(24d, run.BaselineY, precision: 6);
    }

    [Fact]
    public void MeasureTextBox_Table_UsesExplicitColumnDefinitions()
    {
        var request = CreateRequest(new RichTextBlock[]
        {
            new TableBlock(
                new[]
                {
                    new TableColumnDefinition(new ColumnPercentWidth(50)),
                    new TableColumnDefinition(new ColumnPercentWidth(50))
                },
                new[]
                {
                    new TableSectionBlock(TableSectionKind.Body, new[]
                    {
                        new TableRowBlock(new TableCellBlock[]
                        {
                            new TableDataCellBlock(new RichTextBlock[]
                            {
                                new ParagraphBlock(new InlineNode[] { new TextRunNode("Left", new TextStyle("Roboto", 400, 12)) })
                            }),
                            new TableDataCellBlock(new RichTextBlock[]
                            {
                                new ParagraphBlock(new InlineNode[] { new TextRunNode("Right", new TextStyle("Roboto", 400, 12)) })
                            })
                        })
                    })
                })
        }) with { Width = 120 };

        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        using var writer = page.GetWriter();

        var result = writer.MeasureTextBox(request);
        Assert.Equal(2, result.Lines.Count);
        var leftLine = Assert.Single(result.Lines, x => x.Runs.Any(r => r.Text == "Left"));
        var rightLine = Assert.Single(result.Lines, x => x.Runs.Any(r => r.Text == "Right"));

        Assert.True(rightLine.X - leftLine.X > 45d);
    }

    [Fact]
    public void MeasureTextBox_RichText_HandlesBasicTables()
    {
        var request = CreateRequest(new RichTextBlock[]
        {
            new TableBlock(
                new[]
                {
                    new TableRowBlock(new TableCellBlock[]
                    {
                        new TableHeaderCellBlock(new RichTextBlock[]
                        {
                            new ParagraphBlock(new InlineNode[] { new TextRunNode("Header A", new TextStyle("Roboto", 700, 12)) })
                        }),
                        new TableHeaderCellBlock(new RichTextBlock[]
                        {
                            new ParagraphBlock(new InlineNode[] { new TextRunNode("Header B", new TextStyle("Roboto", 700, 12)) })
                        })
                    }),
                    new TableRowBlock(new TableCellBlock[]
                    {
                        new TableDataCellBlock(new RichTextBlock[]
                        {
                            new ParagraphBlock(new InlineNode[] { new TextRunNode("Cell one wraps into multiple words.", new TextStyle("Roboto", 400, 12)) })
                        }, Style: new TableCellStyle(BackgroundColor: new TextColor(250, 245, 190))),
                        new TableDataCellBlock(new RichTextBlock[]
                        {
                            new ParagraphBlock(new InlineNode[] { new TextRunNode("Cell two", new TextStyle("Roboto", 400, 12)) })
                        })
                    })
                },
                new TableStyle(
                    BackgroundColor: new TextColor(245, 245, 245),
                    BorderColor: new TextColor(80, 80, 80),
                    BorderWidth: 1,
                    CellBorderColor: new TextColor(160, 160, 160),
                    CellBorderWidth: 0.75,
                    CellPadding: 6))
        });

        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        using var writer = page.GetWriter();

        var result = writer.MeasureTextBox(request);

        Assert.True(result.Lines.Count >= 4);
        Assert.Contains(result.Decorations, x => x is TextLayoutFillRectDecoration fill && fill.Color == new TextColor(245, 245, 245));
        Assert.Contains(result.Decorations, x => x is TextLayoutFillRectDecoration fill && fill.Color == new TextColor(250, 245, 190));
        Assert.Contains(result.Decorations, x => x is TextLayoutStrokeRectDecoration);
        Assert.DoesNotContain(result.Decorations, x => x is TextLayoutLineDecoration);

        var distinctColumns = result.Lines
            .Select(x => Math.Round(x.X, 3))
            .Distinct()
            .OrderBy(x => x)
            .ToArray();
        Assert.True(distinctColumns.Length >= 2);
        Assert.True(distinctColumns[1] > distinctColumns[0]);
    }

    [Fact]
    public void MeasureTextBox_RichText_HandlesTableSpans()
    {
        var request = CreateRequest(new RichTextBlock[]
        {
            new TableBlock(
                new[]
                {
                    new TableRowBlock(new TableCellBlock[]
                    {
                        new TableHeaderCellBlock(new RichTextBlock[]
                        {
                            new ParagraphBlock(new InlineNode[] { new TextRunNode("Span Header", new TextStyle("Roboto", 700, 12)) })
                        }, ColSpan: 2)
                    }),
                    new TableRowBlock(new TableCellBlock[]
                    {
                        new TableDataCellBlock(new RichTextBlock[]
                        {
                            new ParagraphBlock(new InlineNode[] { new TextRunNode("Tall cell", new TextStyle("Roboto", 400, 12)) })
                        }, RowSpan: 2),
                        new TableDataCellBlock(new RichTextBlock[]
                        {
                            new ParagraphBlock(new InlineNode[] { new TextRunNode("Top right", new TextStyle("Roboto", 400, 12)) })
                        })
                    }),
                    new TableRowBlock(new TableCellBlock[]
                    {
                        new TableDataCellBlock(new RichTextBlock[]
                        {
                            new ParagraphBlock(new InlineNode[] { new TextRunNode("Bottom right", new TextStyle("Roboto", 400, 12)) })
                        })
                    })
                },
                new TableStyle(BorderColor: new TextColor(80, 80, 80), BorderWidth: 1, CellBorderWidth: 0.5, CellPadding: 4))
        });

        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        using var writer = page.GetWriter();

        var result = writer.MeasureTextBox(request);
        var lines = result.Lines.SelectMany(x => x.Runs.Select(r => (Line: x, Run: r))).ToArray();

        var topRight = lines.First(x => x.Run.Text.StartsWith("Top", StringComparison.Ordinal));
        var bottomRight = lines.First(x => x.Run.Text.StartsWith("Bottom", StringComparison.Ordinal));
        var tall = lines.First(x => x.Run.Text.StartsWith("Tall", StringComparison.Ordinal));

        Assert.True((topRight.Line.X + topRight.Run.X) > (tall.Line.X + tall.Run.X));
        Assert.True((bottomRight.Line.X + bottomRight.Run.X) > (tall.Line.X + tall.Run.X));
        Assert.True(bottomRight.Line.BaselineY > topRight.Line.BaselineY);
    }

    [Fact]
    public void MeasureTextBox_RichText_DistributesRowSpanGrowth_Proportionally()
    {
        var request = CreateRequest(new RichTextBlock[]
        {
            new TableBlock(
                new[]
                {
                    new TableRowBlock(new TableCellBlock[]
                    {
                        new TableDataCellBlock(new RichTextBlock[]
                        {
                            new ParagraphBlock(new InlineNode[]
                            {
                                new TextRunNode("Spanning cell content is intentionally long enough to require extra combined height beyond the two right-hand rows.", new TextStyle("Roboto", 400, 12))
                            }, new ParagraphStyle(LineHeight: 16))
                        }, RowSpan: 2),
                        new TableDataCellBlock(new RichTextBlock[]
                        {
                            new ParagraphBlock(new InlineNode[]
                            {
                                new TextRunNode("Top right row is much taller because it wraps over several lines of text for the proportional span test.", new TextStyle("Roboto", 400, 12))
                            }, new ParagraphStyle(LineHeight: 16))
                        })
                    }),
                    new TableRowBlock(new TableCellBlock[]
                    {
                        new TableDataCellBlock(new RichTextBlock[]
                        {
                            new ParagraphBlock(new InlineNode[]
                            {
                                new TextRunNode("Short row.", new TextStyle("Roboto", 400, 12))
                            }, new ParagraphStyle(LineHeight: 16))
                        })
                    })
                },
                new TableStyle(
                    BorderColor: new TextColor(80, 80, 80),
                    BorderWidth: 1,
                    CellBorderColor: new TextColor(120, 120, 120),
                    CellBorderWidth: 0.5,
                    CellPadding: 4))
        });

        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        using var writer = page.GetWriter();

        var result = writer.MeasureTextBox(request);
        var strokeRects = result.Decorations
            .OfType<TextLayoutStrokeRectDecoration>()
            .OrderBy(x => x.X)
            .ThenBy(x => x.Y)
            .ToArray();

        Assert.True(strokeRects.Length >= 3);
        var topRightCell = strokeRects[1];
        var bottomRightCell = strokeRects[2];
        Assert.True(topRightCell.Height > bottomRightCell.Height + 20d);
    }

    [Fact]
    public void WriteTextBox_RichText_WritesTableChrome()
    {
        var face = CreateFace("roboto-regular", "Roboto", 400);
        var boldFace = CreateFace("roboto-bold", "Roboto", 700);
        var fontLibrary = new PdfTextLayoutFontLibrary(new[]
        {
            new PdfTextLayoutFontFace(face, TrueTypeFont.CreateWritableFont(File.ReadAllBytes(RobotoPath))),
            new PdfTextLayoutFontFace(boldFace, TrueTypeFont.CreateWritableFont(File.ReadAllBytes(RobotoPath)))
        });
        var request = CreateRequest(new RichTextBlock[]
        {
            new TableBlock(
                new[]
                {
                    new TableRowBlock(new TableCellBlock[]
                    {
                        new TableHeaderCellBlock(new RichTextBlock[]
                        {
                            new ParagraphBlock(new InlineNode[] { new TextRunNode("Cell", new TextStyle("Roboto", 700, 12)) })
                        }, Style: new TableCellStyle(BackgroundColor: new TextColor(255, 240, 180)))
                    })
                },
                new TableStyle(
                    BackgroundColor: new TextColor(240, 240, 240),
                    BorderColor: new TextColor(40, 40, 40),
                    BorderWidth: 1,
                    CellBorderColor: new TextColor(120, 120, 120),
                    CellBorderWidth: 0.5,
                    CellPadding: 6))
        }, fontLibrary.CreateLayoutLibrary());

        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        using (var writer = page.GetWriter())
        {
            writer.WriteTextBox(new PdfRect<double>(20, 20, 180, 180), request, fontLibrary);
        }

        var content = page.DumpDecodedContents();
        Assert.Contains("0.941", content);
        Assert.Contains("0.705", content);
        Assert.Contains(" re", content);
        Assert.Contains("RG", content);
        Assert.Contains(" TJ", content);
    }

    [Fact]
    public void MeasureTextBox_RichText_HandlesWeightedRowBlocks()
    {
        var request = CreateRequest(new RichTextBlock[]
        {
            new RowBlock(
                new[]
                {
                    new LayoutChild(new RichTextBlock[]
                    {
                        new ParagraphBlock(new InlineNode[] { new TextRunNode("Alpha", new TextStyle("Roboto", 400, 12)) })
                    }, Weight: 2),
                    new LayoutChild(new RichTextBlock[]
                    {
                        new ParagraphBlock(new InlineNode[] { new TextRunNode("Beta", new TextStyle("Roboto", 400, 12)) })
                    }, Weight: 1),
                    new LayoutChild(new RichTextBlock[]
                    {
                        new ParagraphBlock(new InlineNode[] { new TextRunNode("Gamma", new TextStyle("Roboto", 400, 12)) })
                    }, Weight: 1)
                },
                Style: new LayoutContainerStyle(Gap: 8, Padding: 4))
        });

        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        using var writer = page.GetWriter();

        var result = writer.MeasureTextBox(request);
        var runs = result.Lines.SelectMany(line => line.Runs.Select(run => (Text: run.Text, X: line.X + run.X))).ToArray();
        var alphaX = runs.First(x => x.Text == "Alpha").X;
        var betaX = runs.First(x => x.Text == "Beta").X;
        var gammaX = runs.First(x => x.Text == "Gamma").X;

        Assert.True(betaX > alphaX);
        Assert.True(gammaX > betaX);
        Assert.InRange(betaX - alphaX, 80d, 110d);
        Assert.InRange(gammaX - betaX, 40d, 65d);
    }

    [Fact]
    public void MeasureTextBox_RichText_HandlesNestedColumnAndGrowContent()
    {
        var request = CreateRequest(new RichTextBlock[]
        {
            new ColumnBlock(
                new[]
                {
                    new LayoutChild(new RichTextBlock[]
                    {
                        new RowBlock(
                            new[]
                            {
                                new LayoutChild(new RichTextBlock[]
                                {
                                    new ParagraphBlock(new InlineNode[] { new TextRunNode("One", new TextStyle("Roboto", 400, 12)) })
                                }, Weight: 2),
                                new LayoutChild(new RichTextBlock[]
                                {
                                    new ParagraphBlock(new InlineNode[] { new TextRunNode("Two", new TextStyle("Roboto", 400, 12)) })
                                }, Weight: 1),
                                new LayoutChild(new RichTextBlock[]
                                {
                                    new ParagraphBlock(new InlineNode[] { new TextRunNode("Three", new TextStyle("Roboto", 400, 12)) })
                                }, Weight: 1)
                            },
                            Style: new LayoutContainerStyle(Gap: 6, Padding: 4))
                    }),
                    new LayoutChild(new RichTextBlock[]
                    {
                        new ParagraphBlock(
                            new InlineNode[]
                            {
                                new TextRunNode("Below the row there is another paragraph that should sit lower in the column flow.", new TextStyle("Roboto", 400, 12))
                            },
                            new ParagraphStyle(LineHeight: 16))
                    })
                },
                Style: new LayoutContainerStyle(Gap: 10, Padding: 6))
        });

        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        using var writer = page.GetWriter();

        var result = writer.MeasureTextBox(request);
        var oneLine = result.Lines.First(x => x.Runs.Any(r => r.Text == "One"));
        var belowLine = result.Lines.First(x => x.Runs.Any(r => r.Text == "Below"));

        Assert.True(belowLine.BaselineY > oneLine.BaselineY);
        Assert.True(result.MeasuredHeight > 40d);
    }

    [Fact]
    public void MeasureTextBox_RichText_HandlesFixedHeightAndVerticalAlignmentInRows()
    {
        var request = CreateRequest(new RichTextBlock[]
        {
            new RowBlock(
                new[]
                {
                    new LayoutChild(new RichTextBlock[]
                    {
                        new ParagraphBlock(new InlineNode[] { new TextRunNode("Top", new TextStyle("Roboto", 400, 12)) })
                    }, VerticalAlignment: TextVerticalAlignment.Top, BoxStyle: new TextBoxStyle(BorderWidth: 1, Padding: 4)),
                    new LayoutChild(new RichTextBlock[]
                    {
                        new ParagraphBlock(new InlineNode[] { new TextRunNode("Bottom", new TextStyle("Roboto", 400, 12)) })
                    }, VerticalAlignment: TextVerticalAlignment.Bottom, BoxStyle: new TextBoxStyle(BorderWidth: 1, Padding: 4))
                },
                Height: 80,
                Style: new LayoutContainerStyle(Gap: 8, Padding: 4))
        });

        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        using var writer = page.GetWriter();

        var result = writer.MeasureTextBox(request);
        var topLine = result.Lines.First(x => x.Runs.Any(r => r.Text == "Top"));
        var bottomLine = result.Lines.First(x => x.Runs.Any(r => r.Text == "Bottom"));

        Assert.True(bottomLine.BaselineY > topLine.BaselineY + 20d);
    }

    [Fact]
    public void MeasureTextBox_RichText_ClipsCenteredOverflowingRowChild_FromCenteredWindow()
    {
        var request = CreateRequest(new RichTextBlock[]
        {
            new RowBlock(
                new[]
                {
                    new LayoutChild(
                        new RichTextBlock[]
                        {
                            new HeadingBlock(
                                3,
                                new InlineNode[]
                                {
                                    new TextRunNode("Lead", new TextStyle("Roboto", 700, 16))
                                },
                                new ParagraphStyle(LineHeight: 20, MarginBlockEnd: 4)),
                            new ParagraphBlock(
                                new InlineNode[]
                                {
                                    new TextRunNode("The lead column carries more content and should occupy half of the row width.", new TextStyle("Roboto", 400, 12))
                                },
                                new ParagraphStyle(LineHeight: 16))
                        },
                        Weight: 1,
                        VerticalAlignment: TextVerticalAlignment.Center,
                        BoxStyle: new TextBoxStyle(BorderWidth: 1, Padding: 6))
                },
                Height: 86,
                Style: new LayoutContainerStyle(Padding: 8))
        }) with { Width = 120, Height = 110 };

        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        using var writer = page.GetWriter();

        var result = writer.MeasureTextBox(request);
        var visibleWords = result.Lines.SelectMany(x => x.Runs).Select(x => x.Text).ToArray();

        Assert.DoesNotContain("Lead", visibleWords);
        Assert.Contains("content", visibleWords);
    }

    [Fact]
    public void WriteTextBox_RichText_WritesContainerChrome()
    {
        var face = CreateFace("roboto-regular", "Roboto", 400);
        var fontLibrary = new PdfTextLayoutFontLibrary(new[]
        {
            new PdfTextLayoutFontFace(face, TrueTypeFont.CreateWritableFont(File.ReadAllBytes(RobotoPath)))
        });
        var request = CreateRequest(new RichTextBlock[]
        {
            new RowBlock(
                new[]
                {
                    new LayoutChild(new RichTextBlock[]
                    {
                        new ParagraphBlock(new InlineNode[] { new TextRunNode("Boxed", new TextStyle("Roboto", 400, 12)) })
                    }, BoxStyle: new TextBoxStyle(BackgroundColor: new TextColor(255, 240, 180), BorderColor: new TextColor(80, 80, 80), BorderWidth: 1, Padding: 4)),
                    new LayoutChild(new RichTextBlock[]
                    {
                        new ParagraphBlock(new InlineNode[] { new TextRunNode("Plain", new TextStyle("Roboto", 400, 12)) })
                    })
                },
                Style: new LayoutContainerStyle(
                    BackgroundColor: new TextColor(240, 240, 240),
                    BorderColor: new TextColor(40, 40, 40),
                    BorderWidth: 1,
                    Padding: 6,
                    Gap: 8))
        }, fontLibrary.CreateLayoutLibrary());

        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        using (var writer = page.GetWriter())
        {
            writer.WriteTextBox(new PdfRect<double>(20, 20, 180, 180), request, fontLibrary);
        }

        var content = page.DumpDecodedContents();
        Assert.Contains("0.941", content);
        Assert.Contains("RG", content);
        Assert.Contains(" TJ", content);
    }

    private static RichTextBoxLayoutRequest CreateRequest(IReadOnlyList<RichTextBlock> blocks, TextFontLibrary? library = null)
        => new(
            200,
            200,
            library ?? new TextFontLibrary(new[] { CreateFace("roboto-regular", "Roboto", 400) }),
            blocks)
        {
            OverflowMode = TextOverflowMode.Clip
        };

    private static TextFontFace CreateFace(string faceId, string familyName, int weight, bool italic = false)
        => new(faceId, familyName, weight, File.ReadAllBytes(RobotoPath), Italic: italic);

    private static string FlattenInlineText(IReadOnlyList<InlineNode> inlines)
        => string.Concat(inlines.Select(x => x switch
        {
            TextRunNode run => run.Text,
            LineBreakNode => "\n",
            _ => string.Empty
        }));

    private static int FindSequence(IReadOnlyList<char> chars, string value)
    {
        if (value.Length == 0 || chars.Count < value.Length)
        {
            return -1;
        }

        for (var i = 0; i <= chars.Count - value.Length; i++)
        {
            var matched = true;
            for (var j = 0; j < value.Length; j++)
            {
                if (chars[i + j] != value[j])
                {
                    matched = false;
                    break;
                }
            }

            if (matched)
            {
                return i;
            }
        }

        return -1;
    }

    private static string GetResultsDirectory(string leaf)
        => Path.Combine(AppContext.BaseDirectory, "../../../../../test/results", leaf);
}
