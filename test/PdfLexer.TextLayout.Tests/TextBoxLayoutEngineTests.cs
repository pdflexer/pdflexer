using PdfLexer.Content;
using PdfLexer.Fonts;
using PdfLexer.TextLayout;
using PdfLexer.Writing;

namespace PdfLexer.TextLayout.Tests;

public class TextBoxLayoutEngineTests
{
    private static readonly string RobotoPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../test/Roboto-Regular.ttf"));

    [Fact]
    public void Layout_UsesExplicitNewlines_AndPreservesSpaces()
    {
        var request = CreateRequest(
            width: 200,
            height: 200,
            new TextSegment("Hello  world\nNext", new TextSegmentStyle("Roboto", 400, 12)));

        var engine = new TextBoxLayoutEngine();
        var result = engine.Layout(request);

        Assert.True(result.Success);
        Assert.Equal(2, result.Lines.Count);
        Assert.Contains(result.Lines[0].Runs, x => x.Text == "  ");
        Assert.Equal("Hello", result.Lines[0].Runs[0].Text);
        Assert.Equal("Next", result.Lines[1].Runs.Single().Text);
    }

    [Fact]
    public void Layout_Tokenize_SkipsCarriageReturns_AndGroupsTabsAsWhitespace()
    {
        var request = CreateRequest(
            width: 200,
            height: 200,
            new TextSegment("Hello\r\tworld\nNext", new TextSegmentStyle("Roboto", 400, 12)));

        var engine = new TextBoxLayoutEngine();
        var result = engine.Layout(request);

        Assert.True(result.Success);
        Assert.Equal(2, result.Lines.Count);
        Assert.Equal("Hello", result.Lines[0].Runs[0].Text);
        Assert.Equal("\t", result.Lines[0].Runs[1].Text);
        Assert.Equal("world", result.Lines[0].Runs[2].Text);
        Assert.Equal("Next", result.Lines[1].Runs.Single().Text);
    }

    [Fact]
    public void Layout_OffsetsBaseline_ByHalfLeading_WhenLineSpacingExceedsFontSize()
    {
        var request = CreateRequest(
            width: 200,
            height: 200,
            new TextSegment("Hello", new TextSegmentStyle("Roboto", 400, 12, LineSpacing: 15)));

        var engine = new TextBoxLayoutEngine();
        var result = engine.Layout(request);

        Assert.True(result.Success);
        Assert.Single(result.Lines);
        Assert.Equal(13.5d, result.Lines[0].BaselineOffset, precision: 6);
        Assert.Equal(13.5d, result.Lines[0].BaselineY, precision: 6);
    }

    [Fact]
    public void Layout_UsesResolvedFontMetrics_WhenNoExplicitLineHeightIsProvided()
    {
        var request = new TextBoxLayoutRequest(
            200,
            200,
            new TextFontLibrary(new[]
            {
                CreateMetricFace("roboto-metric", "RobotoMetric", 400, ascent: 800, descent: -200, lineGap: 100)
            }),
            new[] { new TextSegment("Hello", new TextSegmentStyle("RobotoMetric", 400, 12)) });

        var engine = new TextBoxLayoutEngine();
        var result = engine.Layout(request);

        Assert.True(result.Success);
        Assert.Single(result.Lines);
        Assert.Equal(13.2d, result.Lines[0].Height, precision: 6);
        Assert.Equal(10.2d, result.Lines[0].BaselineOffset, precision: 6);
    }

    [Fact]
    public void Layout_UsesExplicitLineHeight_WithMetricBasedBaselineOffset()
    {
        var request = new TextBoxLayoutRequest(
            200,
            200,
            new TextFontLibrary(new[]
            {
                CreateMetricFace("roboto-metric", "RobotoMetric", 400, ascent: 800, descent: -200, lineGap: 100)
            }),
            new[] { new TextSegment("Hello", new TextSegmentStyle("RobotoMetric", 400, 12, LineSpacing: 15)) });

        var engine = new TextBoxLayoutEngine();
        var result = engine.Layout(request);

        Assert.True(result.Success);
        Assert.Single(result.Lines);
        Assert.Equal(15d, result.Lines[0].Height, precision: 6);
        Assert.Equal(11.1d, result.Lines[0].BaselineOffset, precision: 6);
    }

    [Fact]
    public void Layout_BlankLines_ConsumeFullMetricLineBoxHeight()
    {
        var request = new TextBoxLayoutRequest(
            200,
            200,
            new TextFontLibrary(new[]
            {
                CreateMetricFace("roboto-metric", "RobotoMetric", 400, ascent: 800, descent: -200, lineGap: 100)
            }),
            new[] { new TextSegment("Hello\n\nWorld", new TextSegmentStyle("RobotoMetric", 400, 12)) });

        var engine = new TextBoxLayoutEngine();
        var result = engine.Layout(request);

        Assert.True(result.Success);
        Assert.Equal(3, result.Lines.Count);
        Assert.Equal(39.6d, result.MeasuredHeight, precision: 6);
    }

    [Fact]
    public void Layout_DoesNotLockLaterWrappedLines_ToFirstLargeInlineMetrics()
    {
        var request = new TextBoxLayoutRequest(
            95,
            300,
            new TextFontLibrary(new[]
            {
                CreateMetricFace("roboto-large", "RobotoLarge", 400, ascent: 800, descent: -200, lineGap: 0),
                CreateMetricFace("roboto-body", "RobotoBody", 400, ascent: 800, descent: -200, lineGap: 0)
            }),
            new[]
            {
                new TextSegment("Welcome ", new TextSegmentStyle("RobotoLarge", 400, 50)),
                new TextSegment("small words wrap into later lines", new TextSegmentStyle("RobotoBody", 400, 12))
            });

        var engine = new TextBoxLayoutEngine();
        var result = engine.Layout(request);

        Assert.True(result.Lines.Count >= 3);
        Assert.Equal(TextLayoutStatus.Overflow, result.Status);
        Assert.Equal(50d, result.Lines[0].Height, precision: 6);
        Assert.InRange(result.Lines[1].Height, 11.5d, 13d);
        Assert.InRange(result.Lines[2].Height, 11.5d, 13d);
    }

    [Fact]
    public void Layout_ShapesMultipleChunks_Consistently_WhenUsingAShapeSession()
    {
        var request = CreateRequest(
            width: 200,
            height: 200,
            new TextSegment("Alpha Beta\nGamma Delta\nEpsilon", new TextSegmentStyle("Roboto", 400, 12)));

        var engine = new TextBoxLayoutEngine();
        var result = engine.Layout(request);

        Assert.True(result.Success);
        Assert.Equal(3, result.Lines.Count);
        Assert.Equal(new[] { "Alpha", " ", "Beta" }, result.Lines[0].Runs.Select(x => x.Text).ToArray());
        Assert.Equal(new[] { "Gamma", " ", "Delta" }, result.Lines[1].Runs.Select(x => x.Text).ToArray());
        Assert.Equal("Epsilon", result.Lines[2].Runs.Single().Text);
        Assert.All(result.Lines.SelectMany(x => x.Runs).Where(x => x.Text.Trim().Length > 0), run => Assert.NotEmpty(run.Glyphs));
    }

    [Fact]
    public void Layout_UsesParagraphStrutForBaselineOffset_WhenExplicitLineHeightWrapsMixedFaces()
    {
        var request = new TextBoxLayoutRequest(
            100,
            200,
            new TextFontLibrary(new[]
            {
                CreateMetricFace("roboto-body", "RobotoBody", 400, ascent: 800, descent: -200, lineGap: 0),
                CreateMetricFace("serif-tall", "SerifTall", 400, ascent: 1000, descent: -300, lineGap: 0),
                CreateMetricFace("mono-tall", "MonoTall", 400, ascent: 950, descent: -250, lineGap: 0)
            }),
            new[]
            {
                new TextSegment("The second item mixes ", new TextSegmentStyle("RobotoBody", 400, 12, LineSpacing: 16)),
                new TextSegment("serif emphasis", new TextSegmentStyle("SerifTall", 400, 14, LineSpacing: 16)),
                new TextSegment(" and ", new TextSegmentStyle("RobotoBody", 400, 12, LineSpacing: 16)),
                new TextSegment("mono snippets", new TextSegmentStyle("MonoTall", 400, 12, LineSpacing: 16)),
                new TextSegment(" within the same list item.", new TextSegmentStyle("RobotoBody", 400, 12, LineSpacing: 16))
            });

        var engine = new TextBoxLayoutEngine();
        var result = engine.Layout(request);

        Assert.True(result.Lines.Count >= 2);
        Assert.Equal(16d, result.Lines[0].Height, precision: 6);
        Assert.Equal(16d, result.Lines[1].Height, precision: 6);
        Assert.Equal(result.Lines[0].BaselineOffset, result.Lines[1].BaselineOffset, precision: 6);
        Assert.Equal(16d, result.Lines[1].BaselineY - result.Lines[0].BaselineY, precision: 6);
    }

    [Fact]
    public void TrueTypeEmbeddedFont_ExposesResolvedLayoutMetrics()
    {
        var embeddable = TrueTypeFont.CreateEmbeddableFont(File.ReadAllBytes(RobotoPath));
        var trueType = Assert.IsType<TrueTypeEmbeddedFont>(embeddable);

        var metrics = trueType.GetLayoutMetrics();

        Assert.True(metrics.UnitsPerEm > 0);
        Assert.True(metrics.Ascent > 0);
        Assert.True(metrics.Descent < 0);
        Assert.NotEqual(FontLayoutMetricSource.None, metrics.Source);
        Assert.True(metrics.LineGap >= 0);
    }

    [Fact]
    public void TrueTypeEmbeddedFont_PopulatesGlyphBoundingBoxes_ForDescenders()
    {
        var embeddable = TrueTypeFont.CreateEmbeddableFont(File.ReadAllBytes(RobotoPath));
        var trueType = Assert.IsType<TrueTypeEmbeddedFont>(embeddable);

        Assert.True(trueType.Glyphs.TryGetValue('g', out var glyph));

        Assert.NotNull(glyph);
        Assert.NotNull(glyph.BBox);
        Assert.True(glyph.BBox![1] < 0m);
    }

    [Fact]
    public void Layout_UsesFallbackFamily_WhenConfigured()
    {
        var fallbackFace = CreateFace("roboto-regular", "RobotoFallback", 400);
        var library = new TextFontLibrary(new[] { fallbackFace });
        var request = new TextBoxLayoutRequest(
            200,
            100,
            library,
            new[] { new TextSegment("Hello", new TextSegmentStyle("MissingFamily", 400, 12)) })
        {
            MissingFontBehavior = TextResolutionBehavior.UseFallbackFamilies,
            FallbackFamilyNames = new[] { "RobotoFallback" }
        };

        var engine = new TextBoxLayoutEngine();
        var result = engine.Layout(request);

        Assert.True(result.Success);
        Assert.Equal("roboto-regular", result.Lines[0].Runs[0].FaceId);
    }

    [Fact]
    public void Fit_SplitsFlatText_ByLaidOutLine_AndReturnsRemainderRequest()
    {
        var request = CreateRequest(
            width: 70,
            height: 18,
            new TextSegment("Alpha Beta Gamma", new TextSegmentStyle("Roboto", 400, 12)));

        var engine = new TextBoxLayoutEngine();
        var result = engine.Fit(request);

        Assert.True(result.HasRemainder);
        Assert.Equal(TextBreakKind.Line, result.BreakKind);
        Assert.Single(result.FittingLayout.Lines);
        Assert.Equal("Alpha", result.FittingLayout.Lines[0].Runs[0].Text);
        Assert.NotNull(result.RemainderRequest);
        Assert.NotEmpty(result.RemainderRequest!.Segments);
        Assert.Contains("Gamma", string.Concat(result.RemainderRequest.Segments.Select(x => x.Text)));
    }

    [Fact]
    public void Layout_Fails_WhenWeightMissing_AndNoFallback()
    {
        var library = new TextFontLibrary(new[] { CreateFace("roboto-regular", "Roboto", 400) });
        var request = new TextBoxLayoutRequest(
            200,
            100,
            library,
            new[] { new TextSegment("Hello", new TextSegmentStyle("Roboto", 700, 12)) });

        var engine = new TextBoxLayoutEngine();
        var result = engine.Layout(request);

        Assert.Equal(TextLayoutStatus.Error, result.Status);
        Assert.Contains(result.Issues, x => x.Kind == TextLayoutIssueKind.MissingWeight);
    }

    [Fact]
    public void Layout_Fails_WhenGlyphMissing_InFailFastMode()
    {
        var request = CreateRequest(
            width: 200,
            height: 100,
            new TextSegment("Hello 漢", new TextSegmentStyle("Roboto", 400, 12)));

        var engine = new TextBoxLayoutEngine();
        var result = engine.Layout(request);

        Assert.Equal(TextLayoutStatus.Error, result.Status);
        Assert.Contains(result.Issues, x => x.Kind == TextLayoutIssueKind.MissingGlyph);
    }

    [Fact]
    public void Layout_ReportsMissingGlyph_WhenFallbackModeStillCannotResolve()
    {
        var fallbackFace = CreateFace("roboto-fallback", "RobotoFallback", 400);
        var request = new TextBoxLayoutRequest(
            200,
            100,
            new TextFontLibrary(new[]
            {
                CreateFace("roboto-regular", "Roboto", 400),
                fallbackFace
            }),
            new[] { new TextSegment("Hello 漢", new TextSegmentStyle("Roboto", 400, 12)) })
        {
            MissingGlyphBehavior = TextResolutionBehavior.UseFallbackFamilies,
            FallbackFamilyNames = new[] { "RobotoFallback" }
        };

        var engine = new TextBoxLayoutEngine();
        var result = engine.Layout(request);

        Assert.Equal(TextLayoutStatus.Error, result.Status);
        Assert.Contains(result.Issues, x => x.Kind == TextLayoutIssueKind.MissingGlyph);
    }

    [Fact]
    public void WriteTextBox_WritesMeasuredLayout_ToPdf()
    {
        var face = CreateFace("roboto-regular", "Roboto", 400);
        var fontLibrary = new PdfTextLayoutFontLibrary(new[]
        {
            new PdfTextLayoutFontFace(face, TrueTypeFont.CreateWritableFont(File.ReadAllBytes(RobotoPath)))
        });

        var request = new TextBoxLayoutRequest(
            120,
            120,
            fontLibrary.CreateLayoutLibrary(),
            new[]
            {
                new TextSegment("Hello\nWorld", new TextSegmentStyle("Roboto", 400, 12, Underline: true))
            });

        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        TextBoxLayoutResult result;
        using (var writer = page.GetWriter())
        {
            result = writer.WriteTextBox(new PdfRect<double>(20, 20, 140, 140), request, fontLibrary);
        }

        Assert.True(result.Lines.Count >= 2);
        var words = new Dictionary<string, double>();
        var scanner = page.GetWordScanner();
        while (scanner.Advance())
        {
            words[scanner.CurrentWord] = scanner.GetInfo().BoundingBox.LLy;
        }

        Assert.Contains("Hello", words.Keys);
        Assert.Contains("World", words.Keys);
        Assert.True(words["Hello"] > words["World"]);
    }

    [Fact]
    public void WriteTextBox_UsesThickerUnderlineStroke()
    {
        var face = CreateFace("roboto-regular", "Roboto", 400);
        var fontLibrary = new PdfTextLayoutFontLibrary(new[]
        {
            new PdfTextLayoutFontFace(face, TrueTypeFont.CreateWritableFont(File.ReadAllBytes(RobotoPath)))
        });

        var request = new TextBoxLayoutRequest(
            120,
            120,
            fontLibrary.CreateLayoutLibrary(),
            new[]
            {
                new TextSegment("Hello", new TextSegmentStyle("Roboto", 400, 12, Underline: true))
            });

        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        using (var writer = page.GetWriter())
        {
            writer.WriteTextBox(new PdfRect<double>(20, 20, 140, 140), request, fontLibrary);
        }

        var content = page.DumpDecodedContents();
        Assert.Contains("0.9 w", content);
    }

    [Fact]
    public void WriteTextBox_DrawsContinuousUnderline_ForDescenders()
    {
        var face = CreateFace("roboto-regular", "Roboto", 400);
        var fontLibrary = new PdfTextLayoutFontLibrary(new[]
        {
            new PdfTextLayoutFontFace(face, TrueTypeFont.CreateWritableFont(File.ReadAllBytes(RobotoPath)))
        });

        var request = new TextBoxLayoutRequest(
            240,
            120,
            fontLibrary.CreateLayoutLibrary(),
            new[]
            {
                new TextSegment("gypsy", new TextSegmentStyle("Roboto", 400, 12, Underline: true))
            });

        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        using (var writer = page.GetWriter())
        {
            writer.WriteTextBox(new PdfRect<double>(20, 20, 260, 140), request, fontLibrary);
        }

        var content = page.DumpDecodedContents();
        var moveCount = content.Split(" m").Length - 1;
        Assert.Equal(1, moveCount);
    }

    [Fact]
    public void Layout_PreservesMeasuredWhitespace_ForUnderlinedText()
    {
        var request = CreateRequest(
            width: 200,
            height: 100,
            new TextSegment("Hello world", new TextSegmentStyle("Roboto", 400, 12, Underline: true)));

        var engine = new TextBoxLayoutEngine();
        var result = engine.Layout(request);

        Assert.True(result.Success);
        var spaceRun = Assert.Single(result.Lines[0].Runs, x => x.Text == " ");
        Assert.True(spaceRun.Underline);
        Assert.True(spaceRun.MeasuredWidth > 0);
    }

    [Fact]
    public void Layout_Includes_BoxPadding_And_Border_InLineOffsets()
    {
        var request = CreateRequest(
            width: 200,
            height: 100,
            new TextSegment("Hello", new TextSegmentStyle("Roboto", 400, 12)))
            with { BoxStyle = new TextBoxStyle(BorderWidth: 2, Padding: 10) };

        var engine = new TextBoxLayoutEngine();
        var result = engine.Layout(request);

        Assert.True(result.Success);
        Assert.Equal(24d, result.Lines[0].BaselineY, precision: 6);
        Assert.Equal(12d, result.Lines[0].X, precision: 6);
        Assert.Equal(36d, result.MeasuredHeight, precision: 6);
    }

    [Fact]
    public void Layout_Fails_When_BoxChrome_Consumes_ContentArea()
    {
        var request = CreateRequest(
            width: 20,
            height: 20,
            new TextSegment("Hello", new TextSegmentStyle("Roboto", 400, 12)))
            with { BoxStyle = new TextBoxStyle(BorderWidth: 5, Padding: 5) };

        var engine = new TextBoxLayoutEngine();
        var result = engine.Layout(request);

        Assert.Equal(TextLayoutStatus.Overflow, result.Status);
        Assert.Empty(result.Lines);
        Assert.Contains(result.Issues, x => x.Message.Contains("no available content area", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void WriteTextBox_Writes_BoxBackground_And_Border_Operators()
    {
        var face = CreateFace("roboto-regular", "Roboto", 400);
        var fontLibrary = new PdfTextLayoutFontLibrary(new[]
        {
            new PdfTextLayoutFontFace(face, TrueTypeFont.CreateWritableFont(File.ReadAllBytes(RobotoPath)))
        });

        var request = new TextBoxLayoutRequest(
            120,
            120,
            fontLibrary.CreateLayoutLibrary(),
            new[]
            {
                new TextSegment("Hello", new TextSegmentStyle("Roboto", 400, 12))
            })
        {
            BoxStyle = new TextBoxStyle(
                BackgroundColor: new TextColor(0, 255, 0),
                BorderColor: new TextColor(255, 0, 0),
                BorderWidth: 2,
                Padding: 8)
        };

        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        using (var writer = page.GetWriter())
        {
            writer.WriteTextBox(new PdfRect<double>(20, 20, 140, 140), request, fontLibrary);
        }

        var content = page.DumpDecodedContents();
        Assert.Contains("0 1 0 rg", content);
        Assert.Contains("1 0 0 RG", content);
        Assert.Contains(" re", content);
        Assert.Contains(" TJ", content);
    }

    private static TextBoxLayoutRequest CreateRequest(double width, double height, params TextSegment[] segments)
    {
        return new TextBoxLayoutRequest(
            width,
            height,
            new TextFontLibrary(new[] { CreateFace("roboto-regular", "Roboto", 400) }),
            segments);
    }

    private static TextFontFace CreateFace(string faceId, string familyName, int weight)
        => new(faceId, familyName, weight, File.ReadAllBytes(RobotoPath));

    private static TextFontFace CreateMetricFace(string faceId, string familyName, int weight, double ascent, double descent, double lineGap)
        => new(
            faceId,
            familyName,
            weight,
            File.ReadAllBytes(RobotoPath),
            UnitsPerEm: 1000,
            Ascent: ascent,
            Descent: descent,
            LineGap: lineGap,
            MetricSource: TextFontMetricSource.Typographic);
}
