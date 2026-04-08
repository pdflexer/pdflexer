namespace PdfLexer.TextLayout;

public enum TextLayoutPlanKind
{
    FlatText,
    RichText
}

public enum TextLayoutNodeKind
{
    Root,
    Paragraph,
    Heading,
    UnorderedList,
    OrderedList,
    ListItem,
    Table,
    TableRow,
    TableCell,
    RowContainer,
    ColumnContainer,
    LayoutChild,
    Line,
    Run
}

public sealed record TextLayoutSourceReference(
    string Path,
    int? SegmentIndex = null,
    int? SourceStart = null,
    int? SourceLength = null,
    string? NodeId = null,
    int ContentVersion = 0,
    int StyleVersion = 0)
{
    public string StableNodeId => NodeId ?? Path;
}

public sealed class TextLayoutPlanNode
{
    public required TextLayoutNodeKind Kind { get; init; }
    public required TextLayoutSourceReference Source { get; init; }
    public required TextLayoutSize NaturalSize { get; init; }
    public required TextLayoutSize VisibleSize { get; init; }
    public IReadOnlyList<TextLayoutPlanNode> Children { get; init; } = Array.Empty<TextLayoutPlanNode>();
    public int? StartLineIndex { get; init; }
    public int? EndLineIndexExclusive { get; init; }
    public TextLayoutLineDiagnostics? LineDiagnostics { get; init; }
    public TextLayoutListDiagnostics? ListDiagnostics { get; init; }
}

public sealed record TextLayoutLineRunContribution(
    string Text,
    string FamilyName,
    double FontSize,
    double Ascent,
    double Descent,
    double LineGap);

public sealed record TextLayoutLineDiagnostics(
    double ResolvedLineHeight,
    double NaturalAscent,
    double NaturalDescent,
    double NaturalLineGap,
    double BaselineOffset,
    bool ExpandedBeyondRequestedLineHeight,
    double? RequestedLineHeight = null,
    IReadOnlyList<TextLayoutLineRunContribution>? RunContributions = null);

public sealed record TextLayoutListDiagnostics(
    double ContentStart,
    double MarkerColumnWidth,
    double MarkerTextAreaWidth,
    double MarkerGap,
    double MarkerVisualWidth,
    double MarkerFontSize);

public sealed class TextLayoutRenderPlan
{
    public required TextBoxLayoutResult Layout { get; init; }
}

public sealed class TextLayoutFlattenedView
{
    public required TextBoxLayoutResult Layout { get; init; }
    public IReadOnlyList<TextLayoutLine> Lines => Layout.Lines;
    public IReadOnlyList<TextLayoutDecoration> Decorations => Layout.Decorations;
}

public sealed class TextLayoutPlanSelection
{
    public required TextLayoutPlan Plan { get; init; }
    public IReadOnlyList<TextLayoutSourceReference> SourceReferences { get; init; } = Array.Empty<TextLayoutSourceReference>();
    public IReadOnlyList<TextLayoutSourceReference> BoundaryReferences { get; init; } = Array.Empty<TextLayoutSourceReference>();
    public IReadOnlyList<TextLayoutContinuationReference> Continuations { get; init; } = Array.Empty<TextLayoutContinuationReference>();
    public TextLayoutFragmentMetadata FragmentMetadata { get; init; } = new(TextFragmentBreak.None);
    public int? StartLineIndex { get; init; }
    public int? EndLineIndexExclusive { get; init; }

    public TextLayoutFlattenedView Flatten()
        => new()
        {
            Layout = Plan.Layout
        };

    public TextLayoutPagePlan ToPagePlan(TextLayoutPageItemRole role = TextLayoutPageItemRole.Content)
        => TextLayoutPageComposer.Compose(TextLayoutPageComposer.FromSelection(this, role));
}

public enum TextLayoutContinuationKind
{
    Line,
    Paragraph,
    ListItem,
    TableRow,
    TableCell,
    ContainerChild
}

public sealed record TextLayoutContinuationReference(
    TextLayoutContinuationKind Kind,
    TextLayoutSourceReference Boundary,
    TextLayoutSourceReference? ContinuationStart = null,
    string? ParentPath = null,
    TextFragmentBreakReason BreakReason = TextFragmentBreakReason.Overflow,
    bool IsForced = false);

public sealed record TextLayoutFragmentMetadata(
    TextFragmentBreak Break,
    TextLayoutContinuationReference? RemainderReference = null);

public enum TextLayoutPageItemRole
{
    Content,
    Header,
    Footer,
    Inserted
}

public sealed record TextLayoutPageItem(
    TextLayoutPageItemRole Role,
    TextLayoutPlan Plan);

public sealed class TextLayoutPagePlan
{
    public required IReadOnlyList<TextLayoutPageItem> Items { get; init; }
    public required TextLayoutRenderPlan RenderPlan { get; init; }

    public TextBoxLayoutResult Layout => RenderPlan.Layout;

    public TextLayoutFlattenedView Flatten()
        => new()
        {
            Layout = Layout
        };
}

public sealed class TextLayoutCompositionException : InvalidOperationException
{
    public TextLayoutCompositionException(string message)
        : base(message)
    {
    }
}

public sealed class TextLayoutPlan
{
    public required TextLayoutPlanKind Kind { get; init; }
    public required TextLayoutPlanNode Root { get; init; }
    public required TextLayoutRenderPlan RenderPlan { get; init; }

    public TextBoxLayoutResult Layout => RenderPlan.Layout;

    public TextLayoutFlattenedView Flatten()
        => new()
        {
            Layout = Layout
        };
}

internal static class TextLayoutPlanSlicer
{
    public static TextLayoutPlan Slice(TextLayoutPlan plan, TextBoxLayoutResult layout)
        => Slice(plan, 0, layout.Lines.Count, layout);

    public static TextLayoutPlan Slice(TextLayoutPlan plan, int startLineIndex, int endLineIndexExclusive, TextBoxLayoutResult layout)
        => new()
        {
            Kind = plan.Kind,
            Root = SliceNode(plan.Root, startLineIndex, endLineIndexExclusive, layout.NaturalSize, layout.VisibleSize)
                ?? new TextLayoutPlanNode
                {
                    Kind = TextLayoutNodeKind.Root,
                    Source = plan.Root.Source,
                    NaturalSize = layout.NaturalSize,
                    VisibleSize = layout.VisibleSize,
                    Children = Array.Empty<TextLayoutPlanNode>(),
                    StartLineIndex = null,
                    EndLineIndexExclusive = null
                },
            RenderPlan = new TextLayoutRenderPlan
            {
                Layout = layout
            }
        };

    private static TextLayoutPlanNode? SliceNode(
        TextLayoutPlanNode node,
        int startLineIndex,
        int endLineIndexExclusive,
        TextLayoutSize? overrideNaturalSize = null,
        TextLayoutSize? overrideVisibleSize = null)
    {
        if (node.Kind == TextLayoutNodeKind.Line)
        {
            if (!Overlaps(node.StartLineIndex, node.EndLineIndexExclusive, startLineIndex, endLineIndexExclusive))
            {
                return null;
            }

            return node;
        }

        var children = new List<TextLayoutPlanNode>(node.Children.Count);
        foreach (var child in node.Children)
        {
            var slicedChild = SliceNode(child, startLineIndex, endLineIndexExclusive);
            if (slicedChild is not null)
            {
                children.Add(slicedChild);
            }
        }

        if (children.Count == 0 && node.Kind != TextLayoutNodeKind.Root)
        {
            return null;
        }

        var (childStart, childEnd) = GetLineRange(children);
        return new TextLayoutPlanNode
        {
            Kind = node.Kind,
            Source = node.Source,
            NaturalSize = overrideNaturalSize ?? node.NaturalSize,
            VisibleSize = overrideVisibleSize ?? node.VisibleSize,
            Children = children,
            StartLineIndex = childStart,
            EndLineIndexExclusive = childEnd
        };
    }

    private static bool Overlaps(int? nodeStart, int? nodeEnd, int selectionStart, int selectionEnd)
        => nodeStart is int start
            && nodeEnd is int end
            && start < selectionEnd
            && end > selectionStart;

    private static (int? StartLineIndex, int? EndLineIndexExclusive) GetLineRange(IReadOnlyList<TextLayoutPlanNode> children)
    {
        int? start = null;
        int? end = null;
        foreach (var child in children)
        {
            if (child.StartLineIndex is int childStart)
            {
                start = start.HasValue ? Math.Min(start.Value, childStart) : childStart;
            }

            if (child.EndLineIndexExclusive is int childEnd)
            {
                end = end.HasValue ? Math.Max(end.Value, childEnd) : childEnd;
            }
        }

        return (start, end);
    }
}

public sealed class TextLayoutFitPlan
{
    internal ITextLayoutFitPlanMaterializer? Materializer { get; init; }

    public required TextLayoutPlanSelection FittedSelection { get; init; }
    public TextLayoutPlanSelection? RemainderSelection { get; init; }
    public required TextBreakKind BreakKind { get; init; }
    public required bool HasRemainder { get; init; }
    public TextFragmentBreak FragmentBreak { get; init; } = TextFragmentBreak.None;

    public TextLayoutPlan FittedPlan => FittedSelection.Plan;

    public TextLayoutPlan? RemainderPlan => RemainderSelection?.Plan;

    public TextLayoutFragmentMetadata FragmentMetadata => FittedSelection.FragmentMetadata;

    public TextLayoutPagePlan ToPagePlan()
        => FittedSelection.ToPagePlan();

    public TextLayoutPagePlan ReplaceTrailingLinesWithFooter(TextLayoutPlan footerPlan, int trimmedLineCount = 1)
        => TextLayoutPageComposer.ReplaceTrailingLinesWithFooter(FittedSelection, footerPlan, trimmedLineCount);

    public TextLayoutPagePlan CreateContinuationPage(TextLayoutPlan? headerPlan = null)
    {
        if (RemainderSelection is null)
        {
            throw new TextLayoutCompositionException("This fit plan does not have a remainder selection.");
        }

        return headerPlan is null
            ? RemainderSelection.ToPagePlan()
            : TextLayoutPageComposer.Compose(
                TextLayoutPageComposer.FromPlan(headerPlan, TextLayoutPageItemRole.Header),
                TextLayoutPageComposer.FromSelection(RemainderSelection, TextLayoutPageItemRole.Content));
    }

    public TextBoxLayoutRequest MaterializeFittedRequest(TextBoxLayoutRequest template)
        => Materializer?.MaterializeFittedRequest(template) ?? throw new NotSupportedException("This fit plan cannot materialize a flat-text fitted request.");

    public TextBoxLayoutRequest? MaterializeRemainderRequest(TextBoxLayoutRequest template)
        => Materializer?.MaterializeRemainderRequest(template);

    public RichTextBoxLayoutRequest MaterializeFittedRequest(RichTextBoxLayoutRequest template)
        => Materializer?.MaterializeFittedRequest(template) ?? throw new NotSupportedException("This fit plan cannot materialize a rich-text fitted request.");

    public RichTextBoxLayoutRequest? MaterializeRemainderRequest(RichTextBoxLayoutRequest template)
        => Materializer?.MaterializeRemainderRequest(template);
}

public static class TextLayoutPageComposer
{
    public static TextLayoutPlanSelection SelectAll(TextLayoutPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        return BuildSelectionFromPlan(plan, Array.Empty<TextLayoutContinuationReference>());
    }

    public static TextLayoutPageItem FromPlan(TextLayoutPlan plan, TextLayoutPageItemRole role = TextLayoutPageItemRole.Content)
    {
        ArgumentNullException.ThrowIfNull(plan);
        return new TextLayoutPageItem(role, plan);
    }

    public static TextLayoutPageItem FromSelection(TextLayoutPlanSelection selection, TextLayoutPageItemRole role = TextLayoutPageItemRole.Content)
    {
        ArgumentNullException.ThrowIfNull(selection);
        return new TextLayoutPageItem(role, selection.Plan);
    }

    public static TextLayoutPagePlan Compose(params TextLayoutPageItem[] items)
    {
        ArgumentNullException.ThrowIfNull(items);
        if (items.Length == 0)
        {
            throw new TextLayoutCompositionException("A page plan must contain at least one item.");
        }

        var layouts = items.Select(x => x.Plan.Layout).ToArray();
        return new TextLayoutPagePlan
        {
            Items = items,
            RenderPlan = new TextLayoutRenderPlan
            {
                Layout = ComposeLayouts(layouts)
            }
        };
    }

    public static TextLayoutPlanSelection TrimTrailingLines(TextLayoutPlanSelection selection, int lineCount)
    {
        ArgumentNullException.ThrowIfNull(selection);
        if (lineCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(lineCount));
        }

        var totalLines = selection.Plan.Layout.Lines.Count;
        var remainingLineCount = Math.Max(0, totalLines - lineCount);
        var slicedPlan = TextLayoutPlanSlicer.Slice(selection.Plan, 0, remainingLineCount, SliceLayout(selection.Plan.Layout, 0, remainingLineCount));
        return BuildSelectionFromPlan(slicedPlan, selection.Continuations);
    }

    public static TextLayoutPlanSelection StartFromContinuation(TextLayoutPlanSelection selection, TextLayoutContinuationReference continuation)
    {
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(continuation);

        if (continuation.ContinuationStart is null)
        {
            throw new TextLayoutCompositionException("The continuation reference does not define a continuation start.");
        }

        ValidateContinuationReference(selection.Plan.Root, continuation);

        if (!ContainsSourceReference(selection.Plan.Root, continuation.ContinuationStart))
        {
            throw new TextLayoutCompositionException($"The continuation '{continuation.ContinuationStart.Path}' is not present in the provided selection.");
        }

        var lineIndex = FindFirstLineIndex(selection.Plan.Layout, continuation.ContinuationStart);
        if (lineIndex < 0)
        {
            throw new TextLayoutCompositionException($"The continuation '{continuation.ContinuationStart.Path}' is not present in the provided selection.");
        }

        var totalLines = selection.Plan.Layout.Lines.Count;
        var slicedPlan = TextLayoutPlanSlicer.Slice(selection.Plan, lineIndex, totalLines, SliceLayout(selection.Plan.Layout, lineIndex, totalLines));
        return BuildSelectionFromPlan(slicedPlan, Array.Empty<TextLayoutContinuationReference>());
    }

    public static TextLayoutPlanSelection StartFromFragmentRemainder(TextLayoutPlanSelection selection)
    {
        ArgumentNullException.ThrowIfNull(selection);

        var continuation = selection.FragmentMetadata.RemainderReference
            ?? selection.Continuations.FirstOrDefault()
            ?? throw new TextLayoutCompositionException("The selection does not expose fragment remainder metadata.");

        return StartFromContinuation(selection, continuation);
    }

    public static TextLayoutPagePlan ReplaceTrailingLinesWithFooter(TextLayoutPlanSelection selection, TextLayoutPlan footerPlan, int trimmedLineCount = 1)
    {
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(footerPlan);

        var trimmed = TrimTrailingLines(selection, trimmedLineCount);
        return Compose(
            FromSelection(trimmed, TextLayoutPageItemRole.Content),
            FromPlan(footerPlan, TextLayoutPageItemRole.Footer));
    }

    public static TextLayoutPagePlan CreateContinuationPage(TextLayoutPlanSelection selection, TextLayoutContinuationReference continuation, TextLayoutPlan? headerPlan = null)
    {
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(continuation);

        var continuedSelection = StartFromContinuation(selection, continuation);
        return headerPlan is null
            ? Compose(FromSelection(continuedSelection, TextLayoutPageItemRole.Content))
            : Compose(
                FromPlan(headerPlan, TextLayoutPageItemRole.Header),
                FromSelection(continuedSelection, TextLayoutPageItemRole.Content));
    }

    public static TextLayoutPagePlan CreateContinuationPage(TextLayoutPlanSelection selection, TextLayoutPlan? headerPlan = null)
    {
        ArgumentNullException.ThrowIfNull(selection);

        var continuation = selection.FragmentMetadata.RemainderReference
            ?? selection.Continuations.FirstOrDefault()
            ?? throw new TextLayoutCompositionException("The selection does not expose fragment remainder metadata.");

        return CreateContinuationPage(selection, continuation, headerPlan);
    }

    public static TextLayoutContinuationReference CreateTableContinuationReferenceAtLine(TextLayoutPlanSelection selection, int startLineIndex)
    {
        ArgumentNullException.ThrowIfNull(selection);

        var totalLines = selection.Plan.Layout.Lines.Count;
        if (startLineIndex < 0 || startLineIndex >= totalLines)
        {
            throw new ArgumentOutOfRangeException(nameof(startLineIndex));
        }

        var rowNode = FindInnermostNode(selection.Plan.Root, TextLayoutNodeKind.TableRow, startLineIndex)
            ?? throw new TextLayoutCompositionException("The requested continuation line is not inside a table row.");
        var cellNode = FindInnermostNode(selection.Plan.Root, TextLayoutNodeKind.TableCell, startLineIndex);
        var continuationNode = cellNode is not null && cellNode.StartLineIndex is int cellStart && startLineIndex > cellStart
            ? cellNode
            : rowNode;
        var continuationKind = continuationNode.Kind == TextLayoutNodeKind.TableCell
            ? TextLayoutContinuationKind.TableCell
            : TextLayoutContinuationKind.TableRow;

        var continuationStart = FindFirstLeafAtOrAfterLine(continuationNode, startLineIndex)
            ?? throw new TextLayoutCompositionException("Unable to resolve the continuation start inside the selected table content.");
        var boundary = FindLastLeafBeforeLine(continuationNode, startLineIndex)
            ?? FindLastLeafBeforeLine(selection.Plan.Root, startLineIndex)
            ?? continuationStart;

        return new TextLayoutContinuationReference(
            continuationKind,
            boundary,
            continuationStart,
            continuationNode.Source.Path);
    }

    public static TextLayoutContinuationReference CreateTableContinuationReferenceAfterTrim(TextLayoutPlanSelection selection, int trimmedLineCount = 1)
    {
        ArgumentNullException.ThrowIfNull(selection);
        if (trimmedLineCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(trimmedLineCount));
        }

        var totalLines = selection.Plan.Layout.Lines.Count;
        var startLineIndex = totalLines - trimmedLineCount;
        if (startLineIndex < 0 || startLineIndex >= totalLines)
        {
            throw new TextLayoutCompositionException("The trimmed line count does not leave a valid continuation start.");
        }

        return CreateTableContinuationReferenceAtLine(selection, startLineIndex);
    }

    public static TextLayoutPagePlan CreateTableContinuationPage(TextLayoutPlanSelection selection, TextLayoutContinuationReference continuation, TextLayoutPlan? headerPlan = null)
    {
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(continuation);

        if (continuation.Kind is not TextLayoutContinuationKind.TableRow and not TextLayoutContinuationKind.TableCell)
        {
            throw new TextLayoutCompositionException("The supplied continuation is not a table row or table cell continuation.");
        }

        return CreateContinuationPage(selection, continuation, headerPlan);
    }

    private static TextLayoutPlanSelection BuildSelectionFromPlan(TextLayoutPlan plan, IReadOnlyList<TextLayoutContinuationReference> continuations)
    {
        var continuation = continuations.FirstOrDefault();
        return new()
        {
            Plan = plan,
            SourceReferences = CollectSourceReferences(plan.Root),
            BoundaryReferences = CollectBoundaryReferences(plan.Root),
            Continuations = continuations,
            FragmentMetadata = new TextLayoutFragmentMetadata(
                continuation is null ? TextFragmentBreak.None : new TextFragmentBreak(continuation.BreakReason, MapBreakKind(continuation.Kind), continuation.IsForced),
                continuation),
            StartLineIndex = plan.Root.StartLineIndex,
            EndLineIndexExclusive = plan.Root.EndLineIndexExclusive
        };
    }

    private static IReadOnlyList<TextLayoutSourceReference> CollectSourceReferences(TextLayoutPlanNode node)
    {
        var results = new List<TextLayoutSourceReference>();
        CollectSourceReferences(node, results);
        return results;
    }

    private static void CollectSourceReferences(TextLayoutPlanNode node, List<TextLayoutSourceReference> results)
    {
        if (node.Kind == TextLayoutNodeKind.Run
            || (node.Children.Count == 0 && node.Kind != TextLayoutNodeKind.Root))
        {
            results.Add(node.Source);
        }

        foreach (var child in node.Children)
        {
            CollectSourceReferences(child, results);
        }
    }

    private static IReadOnlyList<TextLayoutSourceReference> CollectBoundaryReferences(TextLayoutPlanNode node)
    {
        var leaves = CollectSourceReferences(node);
        return leaves.Count == 0 ? Array.Empty<TextLayoutSourceReference>() : new[] { leaves[0], leaves[^1] };
    }

    private static void ValidateContinuationReference(TextLayoutPlanNode root, TextLayoutContinuationReference continuation)
    {
        if (continuation.ParentPath is null)
        {
            return;
        }

        var parentNode = FindNodeByPath(root, continuation.ParentPath)
            ?? throw new TextLayoutCompositionException($"The continuation parent '{continuation.ParentPath}' is not present in the provided selection.");

        if (continuation.ContinuationStart is not null && !IsWithinNode(parentNode, continuation.ContinuationStart))
        {
            throw new TextLayoutCompositionException($"The continuation '{continuation.ContinuationStart.Path}' does not belong to '{continuation.ParentPath}'.");
        }

        if (continuation.Kind == TextLayoutContinuationKind.TableRow
            && parentNode.Kind is not TextLayoutNodeKind.TableRow
            && parentNode.Kind is not TextLayoutNodeKind.Table)
        {
            throw new TextLayoutCompositionException($"The continuation parent '{continuation.ParentPath}' is not a table row.");
        }

        if (continuation.Kind == TextLayoutContinuationKind.TableCell && parentNode.Kind != TextLayoutNodeKind.TableCell)
        {
            throw new TextLayoutCompositionException($"The continuation parent '{continuation.ParentPath}' is not a table cell.");
        }
    }

    private static TextLayoutPlanNode? FindNodeByPath(TextLayoutPlanNode node, string path)
    {
        if (string.Equals(node.Source.Path, path, StringComparison.Ordinal))
        {
            return node;
        }

        foreach (var child in node.Children)
        {
            var match = FindNodeByPath(child, path);
            if (match is not null)
            {
                return match;
            }
        }

        return null;
    }

    private static bool IsWithinNode(TextLayoutPlanNode node, TextLayoutSourceReference source)
        => string.Equals(node.Source.Path, source.Path, StringComparison.Ordinal)
            || ContainsSourceReference(node, source);

    private static TextLayoutPlanNode? FindInnermostNode(TextLayoutPlanNode node, TextLayoutNodeKind kind, int lineIndex)
    {
        if (!NodeContainsLine(node, lineIndex))
        {
            return null;
        }

        foreach (var child in node.Children)
        {
            var match = FindInnermostNode(child, kind, lineIndex);
            if (match is not null)
            {
                return match;
            }
        }

        return node.Kind == kind ? node : null;
    }

    private static bool NodeContainsLine(TextLayoutPlanNode node, int lineIndex)
        => node.StartLineIndex is int start
            && node.EndLineIndexExclusive is int end
            && start <= lineIndex
            && end > lineIndex;

    private static TextLayoutSourceReference? FindFirstLeafAtOrAfterLine(TextLayoutPlanNode node, int lineIndex)
    {
        if (node.Kind == TextLayoutNodeKind.Run)
        {
            return node.Source;
        }

        if (node.Kind == TextLayoutNodeKind.Line)
        {
            return NodeContainsLine(node, lineIndex) || (node.StartLineIndex is int start && start >= lineIndex)
                ? FindFirstLeaf(node)
                : null;
        }

        foreach (var child in node.Children)
        {
            var match = FindFirstLeafAtOrAfterLine(child, lineIndex);
            if (match is not null)
            {
                return match;
            }
        }

        return null;
    }

    private static TextLayoutSourceReference? FindLastLeafBeforeLine(TextLayoutPlanNode node, int lineIndex)
    {
        if (node.Kind == TextLayoutNodeKind.Run)
        {
            return node.Source;
        }

        if (node.Kind == TextLayoutNodeKind.Line)
        {
            return node.EndLineIndexExclusive is int end && end <= lineIndex
                ? FindLastLeaf(node)
                : null;
        }

        for (var i = node.Children.Count - 1; i >= 0; i--)
        {
            var match = FindLastLeafBeforeLine(node.Children[i], lineIndex);
            if (match is not null)
            {
                return match;
            }
        }

        return null;
    }

    private static TextLayoutSourceReference? FindFirstLeaf(TextLayoutPlanNode node)
    {
        if (node.Kind == TextLayoutNodeKind.Run)
        {
            return node.Source;
        }

        foreach (var child in node.Children)
        {
            var match = FindFirstLeaf(child);
            if (match is not null)
            {
                return match;
            }
        }

        return null;
    }

    private static TextLayoutSourceReference? FindLastLeaf(TextLayoutPlanNode node)
    {
        if (node.Kind == TextLayoutNodeKind.Run)
        {
            return node.Source;
        }

        for (var i = node.Children.Count - 1; i >= 0; i--)
        {
            var match = FindLastLeaf(node.Children[i]);
            if (match is not null)
            {
                return match;
            }
        }

        return null;
    }

    private static int FindFirstLineIndex(TextBoxLayoutResult layout, TextLayoutSourceReference source)
    {
        var requiresExactRunVersion = source.SegmentIndex.HasValue || source.SourceStart.HasValue || source.SourceLength.HasValue;
        for (var i = 0; i < layout.Lines.Count; i++)
        {
            if (layout.Lines[i].Runs.Any(x =>
                    x.SourcePath is not null
                    && x.SourcePath.StartsWith(source.Path, StringComparison.Ordinal)
                    && (!requiresExactRunVersion || source.ContentVersion == 0 || ComputeRunContentVersion(x) == source.ContentVersion)))
            {
                return i;
            }
        }

        return -1;
    }

    private static bool ContainsSourceReference(TextLayoutPlanNode node, TextLayoutSourceReference source)
    {
        if (string.Equals(node.Source.Path, source.Path, StringComparison.Ordinal)
            && (source.ContentVersion == 0 || node.Source.ContentVersion == 0 || node.Source.ContentVersion == source.ContentVersion))
        {
            return true;
        }

        foreach (var child in node.Children)
        {
            if (ContainsSourceReference(child, source))
            {
                return true;
            }
        }

        return false;
    }

    private static int ComputeRunContentVersion(TextLayoutRun run)
    {
        var contentVersion = new HashCode();
        contentVersion.Add(run.SourcePath, StringComparer.Ordinal);
        contentVersion.Add(run.SourceStart);
        contentVersion.Add(run.SourceLength);
        contentVersion.Add(run.Text, StringComparer.Ordinal);
        return contentVersion.ToHashCode();
    }

    private static TextBoxLayoutResult SliceLayout(TextBoxLayoutResult layout, int startLineIndex, int endLineIndexExclusive)
    {
        var sourceLines = layout.Lines.Skip(startLineIndex).Take(Math.Max(0, endLineIndexExclusive - startLineIndex)).ToArray();
        var lines = new TextLayoutLine[sourceLines.Length];
        var firstBaseline = sourceLines.Length == 0 ? 0d : sourceLines[0].BaselineY - sourceLines[0].BaselineOffset;
        double naturalWidth = 0d;
        double visibleWidth = 0d;
        double renderedHeight = 0d;

        for (var i = 0; i < sourceLines.Length; i++)
        {
            var sourceLine = sourceLines[i];
            var top = sourceLine.BaselineY - sourceLine.BaselineOffset - firstBaseline;
            var baselineY = top + sourceLine.BaselineOffset;
            var runs = sourceLine.Runs
                .Select(run => run with
                {
                    BaselineY = baselineY
                })
                .ToArray();

            lines[i] = sourceLine with
            {
                Index = i,
                BaselineY = baselineY,
                Runs = runs
            };

            naturalWidth = Math.Max(naturalWidth, sourceLine.MeasuredWidth);
            visibleWidth = Math.Max(visibleWidth, sourceLine.Width);
            renderedHeight = top + sourceLine.Height;
        }

        var decorations = FilterDecorations(layout.Decorations, firstBaseline, renderedHeight);
        return new TextBoxLayoutResult
        {
            Status = layout.Status,
            FitsWidth = layout.FitsWidth,
            FitsHeight = layout.FitsHeight,
            MeasuredWidth = naturalWidth,
            MeasuredHeight = renderedHeight,
            RenderedWidth = visibleWidth,
            RenderedHeight = renderedHeight,
            BoxStyle = new TextBoxStyle(),
            Lines = lines,
            Issues = layout.Issues,
            Decorations = decorations
        };
    }

    private static TextBreakKind MapBreakKind(TextLayoutContinuationKind kind)
        => kind switch
        {
            TextLayoutContinuationKind.Paragraph => TextBreakKind.Paragraph,
            TextLayoutContinuationKind.ListItem => TextBreakKind.ListItem,
            TextLayoutContinuationKind.TableRow => TextBreakKind.TableRow,
            TextLayoutContinuationKind.TableCell => TextBreakKind.TableRow,
            TextLayoutContinuationKind.ContainerChild => TextBreakKind.ContainerChild,
            _ => TextBreakKind.Line
        };

    private static IReadOnlyList<TextLayoutDecoration> FilterDecorations(IReadOnlyList<TextLayoutDecoration> decorations, double firstTop, double maxBottom)
    {
        var filtered = new List<TextLayoutDecoration>(decorations.Count);
        foreach (var decoration in decorations)
        {
            if (GetDecorationBottom(decoration) < firstTop - 0.0001d || GetDecorationTop(decoration) > firstTop + maxBottom + 0.0001d)
            {
                continue;
            }

            filtered.Add(OffsetDecoration(decoration, 0d, -firstTop));
        }

        return filtered;
    }

    private static TextBoxLayoutResult ComposeLayouts(IReadOnlyList<TextBoxLayoutResult> layouts)
    {
        var lines = new List<TextLayoutLine>();
        var decorations = new List<TextLayoutDecoration>();
        var issues = new List<TextLayoutIssue>();
        double yOffset = 0d;
        double naturalWidth = 0d;
        double visibleWidth = 0d;
        var status = TextLayoutStatus.Success;
        var fitsWidth = true;
        var fitsHeight = true;

        foreach (var layout in layouts)
        {
            issues.AddRange(layout.Issues);
            fitsWidth &= layout.FitsWidth;
            fitsHeight &= layout.FitsHeight;
            if (layout.Status == TextLayoutStatus.Error)
            {
                status = TextLayoutStatus.Error;
            }
            else if (layout.Status == TextLayoutStatus.Overflow && status != TextLayoutStatus.Error)
            {
                status = TextLayoutStatus.Overflow;
            }

            var firstTop = layout.Lines.Count == 0 ? 0d : layout.Lines[0].BaselineY - layout.Lines[0].BaselineOffset;
            foreach (var sourceLine in layout.Lines)
            {
                var lineTop = sourceLine.BaselineY - sourceLine.BaselineOffset - firstTop;
                var baselineY = yOffset + lineTop + sourceLine.BaselineOffset;
                var runs = sourceLine.Runs
                    .Select(run => run with
                    {
                        BaselineY = baselineY
                    })
                    .ToArray();
                lines.Add(sourceLine with
                {
                    Index = lines.Count,
                    BaselineY = baselineY,
                    Runs = runs
                });
            }

            foreach (var decoration in layout.Decorations)
            {
                decorations.Add(OffsetDecoration(decoration, 0d, yOffset - firstTop));
            }

            naturalWidth = Math.Max(naturalWidth, layout.NaturalWidth);
            visibleWidth = Math.Max(visibleWidth, layout.VisibleWidth);
            yOffset += layout.VisibleHeight;
        }

        return new TextBoxLayoutResult
        {
            Status = status,
            FitsWidth = fitsWidth,
            FitsHeight = fitsHeight,
            MeasuredWidth = naturalWidth,
            MeasuredHeight = yOffset,
            RenderedWidth = visibleWidth,
            RenderedHeight = yOffset,
            BoxStyle = new TextBoxStyle(),
            Lines = lines,
            Issues = issues,
            Decorations = decorations
        };
    }

    private static TextLayoutDecoration OffsetDecoration(TextLayoutDecoration decoration, double xOffset, double yOffset)
        => decoration switch
        {
            TextLayoutFillRectDecoration fill => fill with { X = fill.X + xOffset, Y = fill.Y + yOffset },
            TextLayoutStrokeRectDecoration stroke => stroke with { X = stroke.X + xOffset, Y = stroke.Y + yOffset },
            TextLayoutLineDecoration line => line with
            {
                X1 = line.X1 + xOffset,
                X2 = line.X2 + xOffset,
                Y1 = line.Y1 + yOffset,
                Y2 = line.Y2 + yOffset
            },
            _ => decoration
        };

    private static double GetDecorationTop(TextLayoutDecoration decoration)
        => decoration switch
        {
            TextLayoutFillRectDecoration fill => fill.Y,
            TextLayoutStrokeRectDecoration stroke => stroke.Y,
            TextLayoutLineDecoration line => Math.Min(line.Y1, line.Y2),
            _ => 0d
        };

    private static double GetDecorationBottom(TextLayoutDecoration decoration)
        => decoration switch
        {
            TextLayoutFillRectDecoration fill => fill.Y + fill.Height,
            TextLayoutStrokeRectDecoration stroke => stroke.Y + stroke.Height,
            TextLayoutLineDecoration line => Math.Max(line.Y1, line.Y2),
            _ => 0d
        };
}

internal interface ITextLayoutFitPlanMaterializer
{
    TextBoxLayoutRequest MaterializeFittedRequest(TextBoxLayoutRequest template);
    TextBoxLayoutRequest? MaterializeRemainderRequest(TextBoxLayoutRequest template);
    RichTextBoxLayoutRequest MaterializeFittedRequest(RichTextBoxLayoutRequest template);
    RichTextBoxLayoutRequest? MaterializeRemainderRequest(RichTextBoxLayoutRequest template);
}
