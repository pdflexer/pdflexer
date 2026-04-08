using System.Text.Json;
using PdfLexer.TextLayout;

namespace PdfLexer.TextLayout.Tests;

public class AnalysisPlanRegressionTests
{
    private const string UpdateBaselinesEnvVar = "PDFLEXER_UPDATE_TEXTLAYOUT_PLAN_BASELINES";
    private static readonly string RobotoPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../test/Roboto-Regular.ttf"));
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    [Fact]
    public void FlatTextAnalysisPlan_MatchesStructuredBaseline_WhenPresent()
    {
        var request = new TextBoxLayoutRequest(
            90,
            24,
            CreateLibrary(),
            new[]
            {
                new TextSegment("Alpha Beta Gamma Delta", new TextSegmentStyle("Roboto", 400, 12), 0, 22, "Paragraphs[0].Runs[0]")
            })
        {
            OverflowMode = TextOverflowMode.Clip
        };

        var engine = new TextBoxLayoutEngine();
        var snapshot = BuildSnapshot(engine.Analyze(request), engine.AnalyzeFit(request));
        AssertSnapshot("FlatTextAnalysisPlan", snapshot);
    }

    [Fact]
    public void RichTextAnalysisPlan_MatchesStructuredBaseline_WhenPresent()
    {
        var request = new RichTextBoxLayoutRequest(
            120,
            36,
            CreateLibrary(),
            new RichTextBlock[]
            {
                new ParagraphBlock(
                    new InlineNode[]
                    {
                        new TextRunNode("Intro paragraph", new TextStyle("Roboto", 400, 12))
                    },
                    new ParagraphStyle(LineHeight: 16)),
                new UnorderedListBlock(new[]
                {
                    new ListItemBlock(new RichTextBlock[]
                    {
                        new ParagraphBlock(
                            new InlineNode[]
                            {
                                new TextRunNode("Bullet content wraps", new TextStyle("Roboto", 400, 12))
                            },
                            new ParagraphStyle(LineHeight: 16))
                    })
                })
            })
        {
            OverflowMode = TextOverflowMode.Clip
        };

        var engine = new RichTextBoxLayoutEngine();
        var snapshot = BuildSnapshot(engine.Analyze(request), engine.AnalyzeFit(request));
        AssertSnapshot("RichTextAnalysisPlan", snapshot);
    }

    [Fact]
    public void FragmentedTableAnalysisPlan_MatchesStructuredBaseline_WhenPresent()
    {
        var request = new RichTextBoxLayoutRequest(
            95,
            34,
            CreateLibrary(),
            new RichTextBlock[]
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
                                        new TextRunNode("This is a long table cell that should fragment within the cell.", new TextStyle("Roboto", 400, 12))
                                    },
                                    new ParagraphStyle(LineHeight: 14))
                            })
                        }),
                        new TableRowBlock(new TableCellBlock[]
                        {
                            new TableDataCellBlock(new RichTextBlock[]
                            {
                                new ParagraphBlock(new InlineNode[] { new TextRunNode("Row 2", new TextStyle("Roboto", 400, 12)) })
                            })
                        })
                    })
            })
        {
            OverflowMode = TextOverflowMode.Fragment
        };

        var engine = new RichTextBoxLayoutEngine();
        var snapshot = BuildSnapshot(engine.Analyze(request), engine.AnalyzeFit(request));
        AssertSnapshot("FragmentedTableAnalysisPlan", snapshot);
    }

    private static void AssertSnapshot(string name, AnalysisPlanSnapshot snapshot)
    {
        var currentDir = Path.Combine(GetResultsDirectory(), "current");
        Directory.CreateDirectory(currentDir);
        File.WriteAllText(Path.Combine(currentDir, $"{name}.json"), JsonSerializer.Serialize(snapshot, JsonOptions));

        var baselinePath = Path.Combine(GetResultsDirectory(), "baselines", $"{name}.json");
        if (ShouldUpdateBaselines())
        {
            Directory.CreateDirectory(Path.GetDirectoryName(baselinePath)!);
            File.WriteAllText(baselinePath, JsonSerializer.Serialize(snapshot, JsonOptions));
            return;
        }

        if (!File.Exists(baselinePath))
        {
            return;
        }

        var expected = JsonSerializer.Deserialize<AnalysisPlanSnapshot>(File.ReadAllText(baselinePath), JsonOptions)
            ?? throw new InvalidOperationException($"Could not deserialize baseline '{baselinePath}'.");

        Assert.Equal(JsonSerializer.Serialize(expected, JsonOptions), JsonSerializer.Serialize(snapshot, JsonOptions));
    }

    private static AnalysisPlanSnapshot BuildSnapshot(TextLayoutPlan plan, TextLayoutFitPlan fitPlan)
        => new(
            plan.Kind.ToString(),
            BuildNode(plan.Root),
            plan.Flatten().Lines.Count,
            plan.Flatten().Decorations.Count,
            BuildSelection(fitPlan.FittedSelection),
            fitPlan.RemainderSelection is null ? null : BuildSelection(fitPlan.RemainderSelection),
            fitPlan.ToPagePlan().Flatten().Lines.Count,
            fitPlan.FragmentBreak.Reason.ToString(),
            fitPlan.FragmentBreak.BoundaryKind.ToString(),
            fitPlan.FragmentBreak.IsForced);

    private static AnalysisPlanNodeSnapshot BuildNode(TextLayoutPlanNode node)
        => new(
            node.Kind.ToString(),
            node.Source.Path,
            node.NaturalSize.Width,
            node.NaturalSize.Height,
            node.VisibleSize.Width,
            node.VisibleSize.Height,
            node.StartLineIndex,
            node.EndLineIndexExclusive,
            node.Children.Select(BuildNode).ToArray());

    private static AnalysisPlanSelectionSnapshot BuildSelection(TextLayoutPlanSelection selection)
        => new(
            selection.SourceReferences.Select(x => x.Path).ToArray(),
            selection.BoundaryReferences.Select(x => x.Path).ToArray(),
            selection.Continuations.Select(x => new AnalysisPlanContinuationSnapshot(
                x.Kind.ToString(),
                x.Boundary.Path,
                x.ContinuationStart?.Path,
                x.ParentPath,
                x.BreakReason.ToString(),
                x.IsForced)).ToArray(),
            selection.StartLineIndex,
            selection.EndLineIndexExclusive,
            selection.Flatten().Lines.Count,
            selection.Flatten().Decorations.Count,
            selection.FragmentMetadata.Break.Reason.ToString(),
            selection.FragmentMetadata.Break.BoundaryKind.ToString(),
            selection.FragmentMetadata.Break.IsForced);

    private static TextFontLibrary CreateLibrary()
        => new(new[] { new TextFontFace("roboto-regular", "Roboto", 400, File.ReadAllBytes(RobotoPath)) });

    private static string GetResultsDirectory()
        => Path.Combine(AppContext.BaseDirectory, "../../../../../test/results/analysis-plan-regression");

    private static bool ShouldUpdateBaselines()
        => string.Equals(Environment.GetEnvironmentVariable(UpdateBaselinesEnvVar), "1", StringComparison.Ordinal);

    private sealed record AnalysisPlanSnapshot(
        string Kind,
        AnalysisPlanNodeSnapshot Root,
        int FlattenedLineCount,
        int FlattenedDecorationCount,
        AnalysisPlanSelectionSnapshot FittedSelection,
        AnalysisPlanSelectionSnapshot? RemainderSelection,
        int FittedPageLineCount,
        string FragmentBreakReason,
        string FragmentBoundaryKind,
        bool FragmentBreakForced);

    private sealed record AnalysisPlanNodeSnapshot(
        string Kind,
        string Path,
        double NaturalWidth,
        double NaturalHeight,
        double VisibleWidth,
        double VisibleHeight,
        int? StartLineIndex,
        int? EndLineIndexExclusive,
        IReadOnlyList<AnalysisPlanNodeSnapshot> Children);

    private sealed record AnalysisPlanSelectionSnapshot(
        IReadOnlyList<string> SourceReferences,
        IReadOnlyList<string> BoundaryReferences,
        IReadOnlyList<AnalysisPlanContinuationSnapshot> Continuations,
        int? StartLineIndex,
        int? EndLineIndexExclusive,
        int FlattenedLineCount,
        int FlattenedDecorationCount,
        string FragmentBreakReason,
        string FragmentBoundaryKind,
        bool FragmentBreakForced);

    private sealed record AnalysisPlanContinuationSnapshot(
        string Kind,
        string BoundaryPath,
        string? ContinuationStartPath,
        string? ParentPath,
        string BreakReason,
        bool IsForced);
}
