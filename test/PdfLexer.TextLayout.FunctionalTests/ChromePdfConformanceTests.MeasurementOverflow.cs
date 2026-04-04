using System.Text.Json;
using System.Text.RegularExpressions;
using PdfLexer.Content;
using PdfLexer.Fonts;
using PdfLexer.TextLayout;
using PdfLexer.Writing;

namespace PdfLexer.TextLayout.FunctionalTests;

public partial class ChromePdfConformanceTests
{
    public static IEnumerable<object[]> MeasurementOverflowCases()
    {
        var caseNames = new[]
        {
            "WrappedTextNarrow",
            "PaddedBorderedTextBox",
            "LongFormMultiParagraphMixedFamilies",
            "UnorderedListWrappedItems",
            "WeightedRowContainer",
            "NestedColumnWithWeightedRow",
            "BasicTableConformance",
            "TableSpanAndNestedContentConformance",
            "RichTextDeepArticleFixture"
        };

        foreach (var caseName in caseNames)
        {
            yield return new object[] { caseName };
        }
    }

    [Theory]
    [MemberData(nameof(MeasurementOverflowCases))]
    public void PdfLexerMeasurementAndOverflow_GeneratesFullAndSplitReviewArtifacts(string caseName)
    {
        var testCase = AllConformanceCases.Single(x => x.Name == caseName);
        var fullHeight = MeasureCanonicalHeight(testCase);
        var fullCase = ResizeCase(testCase, fullHeight);
        var fullLayout = MeasureCaseLayout(fullCase, fullCase.BoxHeight, TextOverflowMode.Visible);
        var fullPdf = RenderCasePdf(fullCase, TextOverflowMode.Visible);
        var fullWords = ExtractWordsAcrossPages(fullPdf);

        Assert.True(fullLayout.MeasuredHeight > 0d);

        var resultsDir = GetResultsDirectory("measurement-overflow");
        Directory.CreateDirectory(resultsDir);
        File.WriteAllBytes(Path.Combine(resultsDir, $"{caseName}.full.pdf"), fullPdf);

        foreach (var cutoff in BuildCutoffSweep(testCase.BoxHeight, fullLayout.MeasuredHeight))
        {
            var splitCase = ResizeCase(testCase, cutoff);
            var splitResult = RenderSplitCasePdf(splitCase);
            var splitWords = ExtractWordsAcrossPages(splitResult.PdfBytes);
            var normalizedFullWords = NormalizeMeasurementWords(fullWords);
            var normalizedSplitWords = NormalizeMeasurementWords(splitWords);

            var label = cutoff.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture).Replace('.', '_');
            File.WriteAllBytes(Path.Combine(resultsDir, $"{caseName}.cutoff-{label}.split.pdf"), splitResult.PdfBytes);
            File.WriteAllText(
                Path.Combine(resultsDir, $"{caseName}.cutoff-{label}.summary.json"),
                JsonSerializer.Serialize(
                    new MeasurementOverflowSnapshot(
                        caseName,
                        cutoff,
                        fullLayout.MeasuredHeight,
                        fullLayout.RenderedHeight,
                        splitResult.HasRemainder,
                        splitResult.BreakKind,
                        splitResult.TopConsumedHeight,
                        splitResult.BottomMeasuredHeight,
                        fullWords.Count,
                        splitWords.Count,
                        normalizedFullWords.Count,
                        normalizedSplitWords.Count),
                    SnapshotJsonOptions));

            Assert.Equal(
                BuildMeasurementWordCounts(normalizedFullWords),
                BuildMeasurementWordCounts(normalizedSplitWords));
        }
    }

    private static double MeasureCanonicalHeight(ConformanceCase testCase)
    {
        var measured = MeasureCaseLayout(testCase, 1_000_000d, TextOverflowMode.Visible);
        var fullContentHeight = Math.Max(measured.MeasuredHeight, measured.RenderedHeight);
        return Math.Ceiling(Math.Max(testCase.BoxHeight, fullContentHeight + 6d));
    }

    private static IReadOnlyList<double> BuildCutoffSweep(double originalHeight, double measuredHeight)
    {
        var cutoffs = new[]
        {
            Math.Max(20d, Math.Floor(originalHeight * 0.5d)),
            Math.Max(24d, Math.Floor(measuredHeight * 0.33d)),
            Math.Max(28d, Math.Floor(measuredHeight * 0.5d)),
            Math.Max(32d, Math.Floor(measuredHeight * 0.66d)),
            Math.Max(36d, Math.Floor(measuredHeight * 0.8d)),
            Math.Max(20d, Math.Floor(measuredHeight - 1d)),
            Math.Ceiling(measuredHeight),
            Math.Ceiling(measuredHeight + 1d)
        };

        return cutoffs
            .Where(x => x > 0d)
            .Distinct()
            .OrderBy(x => x)
            .ToArray();
    }

    private static TextBoxLayoutResult MeasureCaseLayout(ConformanceCase testCase, double boxHeight, TextOverflowMode overflowMode)
    {
        var library = CreateFontLibrary(testCase.Fonts);
        if (testCase.Segments is not null)
        {
            var request = new TextBoxLayoutRequest(
                testCase.BoxWidth,
                boxHeight,
                library.CreateLayoutLibrary(),
                testCase.Segments)
            {
                HorizontalAlignment = testCase.HorizontalAlignment,
                VerticalAlignment = TextVerticalAlignment.Top,
                OverflowMode = overflowMode,
                BoxStyle = testCase.BoxStyle,
                MetricPreference = testCase.MetricPreference
            };
            return new TextBoxLayoutEngine().Layout(request);
        }

        var richRequest = new RichTextBoxLayoutRequest(
            testCase.BoxWidth,
            boxHeight,
            library.CreateLayoutLibrary(),
            testCase.Blocks!)
        {
            VerticalAlignment = TextVerticalAlignment.Top,
            OverflowMode = overflowMode,
            ListIndent = testCase.ListIndent,
            ListMarkerGap = testCase.ListMarkerGap,
            BoxStyle = testCase.BoxStyle,
            MetricPreference = testCase.MetricPreference
        };
        return new RichTextBoxLayoutEngine().Layout(richRequest);
    }

    private static byte[] RenderCasePdf(ConformanceCase testCase, TextOverflowMode overflowMode)
    {
        var library = CreateFontLibrary(testCase.Fonts);

        using var doc = PdfDocument.Create();
        var page = doc.AddPage(testCase.PageWidth, testCase.PageHeight);

        var boxBottom = testCase.PageHeight - testCase.BoxTop - testCase.BoxHeight;
        {
            using var writer = page.GetWriter();
            if (testCase.Segments is not null)
            {
                var request = new TextBoxLayoutRequest(
                    testCase.BoxWidth,
                    testCase.BoxHeight,
                    library.CreateLayoutLibrary(),
                    testCase.Segments)
                {
                    HorizontalAlignment = testCase.HorizontalAlignment,
                    OverflowMode = overflowMode,
                    BoxStyle = testCase.BoxStyle,
                    MetricPreference = testCase.MetricPreference
                };

                writer.WriteTextBox(
                    new PdfRect<double>(testCase.BoxLeft, boxBottom, testCase.BoxLeft + testCase.BoxWidth, boxBottom + testCase.BoxHeight),
                    request,
                    library);
            }
            else
            {
                var request = new RichTextBoxLayoutRequest(
                    testCase.BoxWidth,
                    testCase.BoxHeight,
                    library.CreateLayoutLibrary(),
                    testCase.Blocks!)
                {
                    OverflowMode = overflowMode,
                    ListIndent = testCase.ListIndent,
                    ListMarkerGap = testCase.ListMarkerGap,
                    BoxStyle = testCase.BoxStyle,
                    MetricPreference = testCase.MetricPreference
                };

                writer.WriteTextBox(
                    new PdfRect<double>(testCase.BoxLeft, boxBottom, testCase.BoxLeft + testCase.BoxWidth, boxBottom + testCase.BoxHeight),
                    request,
                    library);
            }
        }

        return doc.Save();
    }

    private static SplitRenderResult RenderSplitCasePdf(ConformanceCase testCase)
    {
        var library = CreateFontLibrary(testCase.Fonts);
        var boxBottom = testCase.PageHeight - testCase.BoxTop - testCase.BoxHeight;

        using var doc = PdfDocument.Create();

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

            var fit = new TextBoxLayoutEngine().Fit(request);
            var firstPage = doc.AddPage(testCase.PageWidth, testCase.PageHeight);
            using (var writer = firstPage.GetWriter())
            {
                writer.WriteTextBox(
                    new PdfRect<double>(testCase.BoxLeft, boxBottom, testCase.BoxLeft + testCase.BoxWidth, boxBottom + testCase.BoxHeight),
                    fit.FittingLayout,
                    library);
            }

            var bottomMeasuredHeight = 0d;
            if (fit.RemainderRequest is not null)
            {
                var remainderLayout = new TextBoxLayoutEngine().Layout(fit.RemainderRequest with
                {
                    Height = 1_000_000d,
                    OverflowMode = TextOverflowMode.Visible
                });
                bottomMeasuredHeight = remainderLayout.MeasuredHeight;

                var remainderPageHeight = Math.Max(testCase.PageHeight, testCase.BoxTop + remainderLayout.MeasuredHeight + 24d);
                var remainderBoxHeight = Math.Max(testCase.BoxHeight, remainderLayout.MeasuredHeight + 4d);
                var remainderBottom = remainderPageHeight - testCase.BoxTop - remainderBoxHeight;

                var secondPage = doc.AddPage(testCase.PageWidth, remainderPageHeight);
                using var writer = secondPage.GetWriter();
                writer.WriteTextBox(
                    new PdfRect<double>(testCase.BoxLeft, remainderBottom, testCase.BoxLeft + testCase.BoxWidth, remainderBottom + remainderBoxHeight),
                    remainderLayout,
                    library);
            }

            return new SplitRenderResult(doc.Save(), fit.HasRemainder, fit.BreakKind.ToString(), fit.ConsumedHeight, bottomMeasuredHeight);
        }

        var richRequest = new RichTextBoxLayoutRequest(
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

        var richFit = new RichTextBoxLayoutEngine().Fit(richRequest);
        var richFirstPage = doc.AddPage(testCase.PageWidth, testCase.PageHeight);
        using (var writer = richFirstPage.GetWriter())
        {
            writer.WriteTextBox(
                new PdfRect<double>(testCase.BoxLeft, boxBottom, testCase.BoxLeft + testCase.BoxWidth, boxBottom + testCase.BoxHeight),
                richFit.FittingLayout,
                library);
        }

        var richBottomMeasuredHeight = 0d;
        if (richFit.RemainderSlice is not null)
        {
            var remainderRequest = richFit.RemainderSlice.ToRequestLike(richRequest) with
            {
                Height = 1_000_000d,
                OverflowMode = TextOverflowMode.Visible
            };
            var remainderLayout = new RichTextBoxLayoutEngine().Layout(remainderRequest);
            richBottomMeasuredHeight = remainderLayout.MeasuredHeight;
            var remainderBoxHeight = Math.Max(testCase.BoxHeight, remainderLayout.MeasuredHeight + 4d);
            var remainderPageHeight = Math.Max(testCase.PageHeight, testCase.BoxTop + remainderBoxHeight + 24d);
            var remainderBottom = remainderPageHeight - testCase.BoxTop - remainderBoxHeight;

            var secondPage = doc.AddPage(testCase.PageWidth, remainderPageHeight);
            using var writer = secondPage.GetWriter();
            writer.WriteTextBox(
                new PdfRect<double>(testCase.BoxLeft, remainderBottom, testCase.BoxLeft + testCase.BoxWidth, remainderBottom + remainderBoxHeight),
                remainderLayout,
                library);
        }

        return new SplitRenderResult(doc.Save(), richFit.HasRemainder, richFit.BreakKind.ToString(), richFit.ConsumedHeight, richBottomMeasuredHeight);
    }

    private static IReadOnlyList<ExtractedWord> ExtractWordsAcrossPages(byte[] pdf)
    {
        using var doc = PdfDocument.Open(pdf);
        var words = new List<ExtractedWord>();
        foreach (var page in doc.Pages)
        {
            var structured = page.GetStructuredText(doc.Context, new StructuredTextOptions
            {
                Order = TextOrder.Reading,
                Mode = StructuredTextMode.Deduplicated
            });

            words.AddRange(structured.Words.Select(x => new ExtractedWord(x.Text, x.BoundingBox)));
        }

        return words;
    }

    private static IReadOnlyList<string> NormalizeMeasurementWords(IReadOnlyList<ExtractedWord> words)
        => words.SelectMany(x => NormalizeMeasurementWord(x.Text))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToArray();

    private static IReadOnlyDictionary<string, int> BuildMeasurementWordCounts(IReadOnlyList<string> words)
        => words.GroupBy(x => x, StringComparer.Ordinal)
            .OrderBy(x => x.Key, StringComparer.Ordinal)
            .ToDictionary(x => x.Key, x => x.Count(), StringComparer.Ordinal);

    private static IEnumerable<string> NormalizeMeasurementWord(string word)
    {
        if (string.IsNullOrWhiteSpace(word))
        {
            yield break;
        }

        var token = word.Trim();
        if (token.Length == 0)
        {
            yield break;
        }

        token = token.TrimStart('\u2022');
        if (token.Length == 0)
        {
            yield break;
        }

        token = Regex.Replace(token, @"^\d+\.", string.Empty);
        if (token.Length == 0)
        {
            yield break;
        }

        yield return token;
    }

    private static ConformanceCase ResizeCase(ConformanceCase testCase, double boxHeight)
    {
        var resolvedPageHeight = Math.Max(testCase.PageHeight, testCase.BoxTop + boxHeight + 24d);
        return testCase with
        {
            BoxHeight = boxHeight,
            PageHeight = resolvedPageHeight
        };
    }

    private sealed record SplitRenderResult(
        byte[] PdfBytes,
        bool HasRemainder,
        string BreakKind,
        double TopConsumedHeight,
        double BottomMeasuredHeight);

    private sealed record MeasurementOverflowSnapshot(
        string CaseName,
        double CutoffHeight,
        double FullMeasuredHeight,
        double FullRenderedHeight,
        bool HasRemainder,
        string BreakKind,
        double TopConsumedHeight,
        double BottomMeasuredHeight,
        int FullWordCount,
        int SplitWordCount,
        int NormalizedFullWordCount,
        int NormalizedSplitWordCount);
}
