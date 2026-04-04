using PdfLexer;
using PdfLexer.Content;
using PdfLexer.Fonts;
using PdfLexer.TextLayout;
using PdfLexer.Writing;

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
            BackgroundColor: new TextColor(240, 240, 120));
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
            Assert.Equal(0.5d, run.CharacterSpacing, precision: 6);
            Assert.Equal(1.5d, run.WordSpacing, precision: 6);
            Assert.Equal(new TextColor(200, 10, 10), run.ForegroundColor);
            Assert.Equal(new TextColor(240, 240, 120), run.BackgroundColor);
            Assert.Equal(16d, run.LineHeight, precision: 6);
        });
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
                },
                ContinuationPolicy: new TableContinuationPolicy(
                    HeaderRows: new[]
                    {
                        new TableRowBlock(new TableCellBlock[]
                        {
                            new TableHeaderCellBlock(new RichTextBlock[] { new ParagraphBlock(new InlineNode[] { new TextRunNode("Continued", style) }) })
                        })
                    },
                    FooterRows: new[]
                    {
                        new TableRowBlock(new TableCellBlock[]
                        {
                            new TableDataCellBlock(new RichTextBlock[] { new ParagraphBlock(new InlineNode[] { new TextRunNode("More", style) }) })
                        })
                    }))
        }) with { Height = 45, Width = 120 };

        var engine = new RichTextBoxLayoutEngine();
        var result = engine.Fit(request);

        Assert.True(result.HasRemainder);
        var fittedTable = Assert.IsType<TableBlock>(Assert.Single(result.FittingSlice.Blocks));
        Assert.Contains(fittedTable.Rows.SelectMany(x => x.Cells).SelectMany(x => x.Blocks).OfType<ParagraphBlock>().SelectMany(x => x.Inlines).OfType<TextRunNode>(), x => x.Text == "More");
        var remainderTable = Assert.IsType<TableBlock>(Assert.Single(result.RemainderSlice!.Blocks));
        Assert.Contains(remainderTable.Rows.SelectMany(x => x.Cells).SelectMany(x => x.Blocks).OfType<ParagraphBlock>().SelectMany(x => x.Inlines).OfType<TextRunNode>(), x => x.Text == "Continued");
        Assert.Contains(result.SplitMetadata, x => x is TableSplitMetadata);
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
            .ToArray();

        Assert.Equal(expectedPositions.Length, actualPositions.Length);
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
}
