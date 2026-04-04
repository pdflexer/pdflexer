using System.Security.Cryptography;
using System.Text.Json;
using pdflexer.TestCaseGen;
using PdfLexer.TextLayout;

namespace PdfLexer.TextLayout.FunctionalTests;

public partial class ChromePdfConformanceTests
{
    private const string UpdateBaselinesEnvVar = "PDFLEXER_UPDATE_TEXTLAYOUT_BASELINES";
    private const double RegressionTolerance = 0.05d;
    private static readonly JsonSerializerOptions SnapshotJsonOptions = new()
    {
        WriteIndented = true
    };

    [Theory]
    [MemberData(nameof(ConformanceCases))]
    public void PdfLexerStructuredRegression_MatchesCheckedInBaseline(string caseName)
    {
        var testCase = ResolveEffectiveHeights(AllConformanceCases.Single(x => x.Name == caseName));
        var pdfLexerPdf = RenderWithPdfLexer(testCase);
        var snapshot = BuildRegressionSnapshot(testCase, pdfLexerPdf);
        WriteCurrentRegressionSnapshot(snapshot);

        var baselinePath = GetRegressionBaselinePath(testCase.Name);
        if (ShouldUpdateBaselines())
        {
            Directory.CreateDirectory(Path.GetDirectoryName(baselinePath)!);
            File.WriteAllText(baselinePath, JsonSerializer.Serialize(snapshot, SnapshotJsonOptions));
            return;
        }

        if (!File.Exists(baselinePath))
        {
            return;
        }

        var expected = JsonSerializer.Deserialize<RegressionSnapshot>(File.ReadAllText(baselinePath), SnapshotJsonOptions)
            ?? throw new InvalidOperationException($"Could not deserialize baseline '{baselinePath}'.");

        AssertRegressionSnapshotEquivalent(expected, snapshot);
    }

    private static void WriteCurrentRegressionSnapshot(ConformanceCase testCase, byte[] pdfLexerPdf)
        => WriteCurrentRegressionSnapshot(BuildRegressionSnapshot(testCase, pdfLexerPdf));

    private static void WriteCurrentRegressionSnapshot(RegressionSnapshot snapshot)
    {
        var currentSnapshotDir = Path.Combine(GetResultsDirectory("pdflexer-regression"), "current");
        Directory.CreateDirectory(currentSnapshotDir);
        File.WriteAllText(Path.Combine(currentSnapshotDir, $"{snapshot.CaseName}.json"), JsonSerializer.Serialize(snapshot, SnapshotJsonOptions));
    }

    private static RegressionSnapshot BuildRegressionSnapshot(ConformanceCase testCase, byte[] pdfLexerPdf)
    {
        var layout = MeasureCaseLayout(testCase, testCase.BoxHeight, TextOverflowMode.Clip);
        var words = ExtractWords(pdfLexerPdf);
        return new RegressionSnapshot(
            testCase.Name,
            new RegressionFixtureSnapshot(
                testCase.PageWidth,
                testCase.PageHeight,
                testCase.BoxLeft,
                testCase.BoxTop,
                testCase.BoxWidth,
                testCase.BoxHeight,
                testCase.HorizontalAlignment.ToString(),
                testCase.BoxStyle.Padding,
                testCase.BoxStyle.BorderWidth,
                testCase.Blocks is not null),
            new RegressionLayoutSnapshot(
                layout.Status.ToString(),
                layout.FitsWidth,
                layout.FitsHeight,
                layout.MeasuredWidth,
                layout.MeasuredHeight,
                layout.RenderedWidth,
                layout.RenderedHeight,
                layout.Lines.Select(CreateLineSnapshot).ToArray(),
                layout.Decorations.Select(CreateDecorationSnapshot).ToArray(),
                layout.Issues.Select(x => x.Kind.ToString()).ToArray()),
            words.Select((word, index) => new RegressionWordSnapshot(
                index,
                word.Text,
                word.BoundingBox.LLx,
                word.BoundingBox.LLy,
                word.BoundingBox.URx,
                word.BoundingBox.URy)).ToArray(),
            Convert.ToHexString(SHA256.HashData(pdfLexerPdf)));
    }

    private static RegressionLineSnapshot CreateLineSnapshot(TextLayoutLine line)
        => new(
            line.Index,
            line.X,
            line.BaselineY,
            line.Width,
            line.MeasuredWidth,
            line.Height,
            line.BaselineOffset,
            line.Runs.Select(CreateRunSnapshot).ToArray());

    private static RegressionRunSnapshot CreateRunSnapshot(TextLayoutRun run)
        => new(
            run.SegmentIndex,
            run.Text,
            run.FaceId,
            run.FamilyName,
            run.Weight,
            run.FontSize,
            run.Italic,
            run.Underline,
            run.CharacterSpacing,
            run.WordSpacing,
            run.ForegroundColor?.ToString(),
            run.BackgroundColor?.ToString(),
            run.X,
            run.BaselineY,
            run.Width,
            run.MeasuredWidth,
            run.LineHeight,
            run.DrawAsVectorBullet,
            run.Glyphs.Count);

    private static RegressionDecorationSnapshot CreateDecorationSnapshot(TextLayoutDecoration decoration)
        => decoration switch
        {
            TextLayoutFillRectDecoration fill => new RegressionDecorationSnapshot(
                nameof(TextLayoutFillRectDecoration),
                fill.X,
                fill.Y,
                fill.Width,
                fill.Height,
                fill.Color.ToString(),
                0d,
                fill.Radius,
                null,
                null),
            TextLayoutStrokeRectDecoration stroke => new RegressionDecorationSnapshot(
                nameof(TextLayoutStrokeRectDecoration),
                stroke.X,
                stroke.Y,
                stroke.Width,
                stroke.Height,
                stroke.Color.ToString(),
                stroke.Thickness,
                stroke.Radius,
                null,
                null),
            TextLayoutLineDecoration line => new RegressionDecorationSnapshot(
                nameof(TextLayoutLineDecoration),
                line.X1,
                line.Y1,
                line.X2,
                line.Y2,
                line.Color.ToString(),
                line.Thickness,
                0d,
                line.X2,
                line.Y2),
            _ => throw new NotSupportedException($"Unsupported decoration type '{decoration.GetType().Name}'.")
        };

    private static void AssertRegressionSnapshotEquivalent(RegressionSnapshot expected, RegressionSnapshot actual)
    {
        Assert.Equal(expected.CaseName, actual.CaseName);
        Assert.Equal(expected.Fixture, actual.Fixture);
        Assert.Equal(expected.Layout.Status, actual.Layout.Status);
        Assert.Equal(expected.Layout.FitsWidth, actual.Layout.FitsWidth);
        Assert.Equal(expected.Layout.FitsHeight, actual.Layout.FitsHeight);
        Assert.Equal(expected.Layout.Issues, actual.Layout.Issues);
        AssertClose(expected.Layout.MeasuredWidth, actual.Layout.MeasuredWidth);
        AssertClose(expected.Layout.MeasuredHeight, actual.Layout.MeasuredHeight);
        AssertClose(expected.Layout.RenderedWidth, actual.Layout.RenderedWidth);
        AssertClose(expected.Layout.RenderedHeight, actual.Layout.RenderedHeight);

        Assert.Equal(expected.Layout.Lines.Length, actual.Layout.Lines.Length);
        for (var i = 0; i < expected.Layout.Lines.Length; i++)
        {
            var expectedLine = expected.Layout.Lines[i];
            var actualLine = actual.Layout.Lines[i];
            Assert.Equal(expectedLine.Index, actualLine.Index);
            AssertClose(expectedLine.X, actualLine.X);
            AssertClose(expectedLine.BaselineY, actualLine.BaselineY);
            AssertClose(expectedLine.Width, actualLine.Width);
            AssertClose(expectedLine.MeasuredWidth, actualLine.MeasuredWidth);
            AssertClose(expectedLine.Height, actualLine.Height);
            AssertClose(expectedLine.BaselineOffset, actualLine.BaselineOffset);

            Assert.Equal(expectedLine.Runs.Length, actualLine.Runs.Length);
            for (var runIndex = 0; runIndex < expectedLine.Runs.Length; runIndex++)
            {
                var expectedRun = expectedLine.Runs[runIndex];
                var actualRun = actualLine.Runs[runIndex];
                Assert.Equal(expectedRun.SegmentIndex, actualRun.SegmentIndex);
                Assert.Equal(expectedRun.Text, actualRun.Text);
                Assert.Equal(expectedRun.FaceId, actualRun.FaceId);
                Assert.Equal(expectedRun.FamilyName, actualRun.FamilyName);
                Assert.Equal(expectedRun.Weight, actualRun.Weight);
                Assert.Equal(expectedRun.Italic, actualRun.Italic);
                Assert.Equal(expectedRun.Underline, actualRun.Underline);
                Assert.Equal(expectedRun.ForegroundColor, actualRun.ForegroundColor);
                Assert.Equal(expectedRun.BackgroundColor, actualRun.BackgroundColor);
                Assert.Equal(expectedRun.DrawAsVectorBullet, actualRun.DrawAsVectorBullet);
                Assert.Equal(expectedRun.GlyphCount, actualRun.GlyphCount);
                AssertClose(expectedRun.FontSize, actualRun.FontSize);
                AssertClose(expectedRun.CharacterSpacing, actualRun.CharacterSpacing);
                AssertClose(expectedRun.WordSpacing, actualRun.WordSpacing);
                AssertClose(expectedRun.X, actualRun.X);
                AssertClose(expectedRun.BaselineY, actualRun.BaselineY);
                AssertClose(expectedRun.Width, actualRun.Width);
                AssertClose(expectedRun.MeasuredWidth, actualRun.MeasuredWidth);
                AssertClose(expectedRun.LineHeight, actualRun.LineHeight);
            }
        }

        Assert.Equal(expected.Layout.Decorations.Length, actual.Layout.Decorations.Length);
        for (var i = 0; i < expected.Layout.Decorations.Length; i++)
        {
            var expectedDecoration = expected.Layout.Decorations[i];
            var actualDecoration = actual.Layout.Decorations[i];
            Assert.Equal(expectedDecoration.Kind, actualDecoration.Kind);
            Assert.Equal(expectedDecoration.Color, actualDecoration.Color);
            AssertClose(expectedDecoration.X1, actualDecoration.X1);
            AssertClose(expectedDecoration.Y1, actualDecoration.Y1);
            AssertClose(expectedDecoration.X2, actualDecoration.X2);
            AssertClose(expectedDecoration.Y2, actualDecoration.Y2);
            AssertClose(expectedDecoration.Thickness, actualDecoration.Thickness);
            AssertClose(expectedDecoration.Radius, actualDecoration.Radius);
            AssertNullableClose(expectedDecoration.X2b, actualDecoration.X2b);
            AssertNullableClose(expectedDecoration.Y2b, actualDecoration.Y2b);
        }

        Assert.Equal(expected.Words.Length, actual.Words.Length);
        for (var i = 0; i < expected.Words.Length; i++)
        {
            var expectedWord = expected.Words[i];
            var actualWord = actual.Words[i];
            Assert.Equal(expectedWord.Index, actualWord.Index);
            Assert.Equal(expectedWord.Text, actualWord.Text);
            AssertClose(expectedWord.LLx, actualWord.LLx);
            AssertClose(expectedWord.LLy, actualWord.LLy);
            AssertClose(expectedWord.URx, actualWord.URx);
            AssertClose(expectedWord.URy, actualWord.URy);
        }
    }

    private static void AssertClose(double expected, double actual)
        => Assert.InRange(Math.Abs(expected - actual), 0d, RegressionTolerance);

    private static void AssertNullableClose(double? expected, double? actual)
    {
        if (!expected.HasValue && !actual.HasValue)
        {
            return;
        }

        Assert.True(expected.HasValue && actual.HasValue);
        AssertClose(expected!.Value, actual!.Value);
    }

    private static string GetRegressionBaselinePath(string caseName)
        => Path.Combine(TestRoot, "PdfLexer.TextLayout.FunctionalTests", "Baselines", "pdflexer-regression", $"{caseName}.json");

    private static string GetResultsDirectory(string leaf)
        => Path.Combine(PathUtil.GetPathFromSegmentOfCurrent("test"), "results", leaf);

    private static bool ShouldUpdateBaselines()
        => string.Equals(Environment.GetEnvironmentVariable(UpdateBaselinesEnvVar), "1", StringComparison.Ordinal);

    private sealed record RegressionSnapshot(
        string CaseName,
        RegressionFixtureSnapshot Fixture,
        RegressionLayoutSnapshot Layout,
        RegressionWordSnapshot[] Words,
        string PdfSha256);

    private sealed record RegressionFixtureSnapshot(
        double PageWidth,
        double PageHeight,
        double BoxLeft,
        double BoxTop,
        double BoxWidth,
        double BoxHeight,
        string HorizontalAlignment,
        double Padding,
        double BorderWidth,
        bool IsRichText);

    private sealed record RegressionLayoutSnapshot(
        string Status,
        bool FitsWidth,
        bool FitsHeight,
        double MeasuredWidth,
        double MeasuredHeight,
        double RenderedWidth,
        double RenderedHeight,
        RegressionLineSnapshot[] Lines,
        RegressionDecorationSnapshot[] Decorations,
        string[] Issues);

    private sealed record RegressionLineSnapshot(
        int Index,
        double X,
        double BaselineY,
        double Width,
        double MeasuredWidth,
        double Height,
        double BaselineOffset,
        RegressionRunSnapshot[] Runs);

    private sealed record RegressionRunSnapshot(
        int SegmentIndex,
        string Text,
        string FaceId,
        string FamilyName,
        int Weight,
        double FontSize,
        bool Italic,
        bool Underline,
        double CharacterSpacing,
        double WordSpacing,
        string? ForegroundColor,
        string? BackgroundColor,
        double X,
        double BaselineY,
        double Width,
        double MeasuredWidth,
        double LineHeight,
        bool DrawAsVectorBullet,
        int GlyphCount);

    private sealed record RegressionDecorationSnapshot(
        string Kind,
        double X1,
        double Y1,
        double X2,
        double Y2,
        string Color,
        double Thickness,
        double Radius,
        double? X2b,
        double? Y2b);

    private sealed record RegressionWordSnapshot(
        int Index,
        string Text,
        double LLx,
        double LLy,
        double URx,
        double URy);
}
