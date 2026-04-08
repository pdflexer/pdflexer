using System.Buffers;
using System.Runtime.CompilerServices;

using PdfLexer.Fonts;

namespace PdfLexer.TextLayout;

public sealed class RichTextBoxLayoutEngine
{
    private const double LargeLayoutHeight = 1_000_000d;
    private static readonly IReadOnlyDictionary<Type, RichBlockFormatterRegistration> FormatterRegistry = CreateFormatterRegistry();

    private delegate void AppendBlockDelegate(RichTextBoxLayoutRequest request, RichTextBlock block, double contentWidth, double xOffset, string path, ref LayoutState state);
    private delegate BlockSplitOutcome SplitBlockDelegate(RichTextBoxLayoutRequest request, RichTextBlock block, double contentWidth, double availableHeight, string path);
    private delegate IReadOnlyList<TextLayoutPlanNode> BuildPlanChildrenDelegate(RichTextBoxLayoutRequest request, TextBoxLayoutResult rootLayout, RichTextBlock block, string path, TextLayoutAnalysisContext? context);

    private sealed record RichBlockFormatterRegistration(
        TextLayoutNodeKind NodeKind,
        AppendBlockDelegate Append,
        SplitBlockDelegate Split,
        BuildPlanChildrenDelegate BuildPlanChildren);

    private static IReadOnlyDictionary<Type, RichBlockFormatterRegistration> CreateFormatterRegistry()
        => new Dictionary<Type, RichBlockFormatterRegistration>
        {
            [typeof(ParagraphBlock)] = new(
                TextLayoutNodeKind.Paragraph,
                static (RichTextBoxLayoutRequest request, RichTextBlock block, double contentWidth, double xOffset, string path, ref LayoutState state) => FlowBlockFormatter.AppendParagraph(request, (ParagraphBlock)block, contentWidth, xOffset, path, ref state),
                static (RichTextBoxLayoutRequest request, RichTextBlock block, double contentWidth, double availableHeight, string path) => FlowBlockFormatter.SplitParagraph(request, (ParagraphBlock)block, contentWidth, availableHeight, path),
                static (RichTextBoxLayoutRequest request, TextBoxLayoutResult rootLayout, RichTextBlock block, string path, TextLayoutAnalysisContext? context) => BuildLineNodesForPath(rootLayout, path, context)),
            [typeof(HeadingBlock)] = new(
                TextLayoutNodeKind.Heading,
                static (RichTextBoxLayoutRequest request, RichTextBlock block, double contentWidth, double xOffset, string path, ref LayoutState state) => FlowBlockFormatter.AppendHeading(request, (HeadingBlock)block, contentWidth, xOffset, path, ref state),
                static (RichTextBoxLayoutRequest request, RichTextBlock block, double contentWidth, double availableHeight, string path) => FlowBlockFormatter.SplitHeading(request, (HeadingBlock)block, contentWidth, availableHeight, path),
                static (RichTextBoxLayoutRequest request, TextBoxLayoutResult rootLayout, RichTextBlock block, string path, TextLayoutAnalysisContext? context) => BuildLineNodesForPath(rootLayout, path, context)),
            [typeof(UnorderedListBlock)] = new(
                TextLayoutNodeKind.UnorderedList,
                static (RichTextBoxLayoutRequest request, RichTextBlock block, double contentWidth, double xOffset, string path, ref LayoutState state) => ListBlockFormatter.AppendUnorderedList(request, (UnorderedListBlock)block, contentWidth, xOffset, path, ref state),
                static (RichTextBoxLayoutRequest request, RichTextBlock block, double contentWidth, double availableHeight, string path) => ListBlockFormatter.SplitUnorderedListBlock(request, (UnorderedListBlock)block, contentWidth, availableHeight, path),
                static (RichTextBoxLayoutRequest request, TextBoxLayoutResult rootLayout, RichTextBlock block, string path, TextLayoutAnalysisContext? context) => BuildListItemNodes(request, rootLayout, ((UnorderedListBlock)block).Items, path, context)),
            [typeof(OrderedListBlock)] = new(
                TextLayoutNodeKind.OrderedList,
                static (RichTextBoxLayoutRequest request, RichTextBlock block, double contentWidth, double xOffset, string path, ref LayoutState state) => ListBlockFormatter.AppendOrderedList(request, (OrderedListBlock)block, contentWidth, xOffset, path, ref state),
                static (RichTextBoxLayoutRequest request, RichTextBlock block, double contentWidth, double availableHeight, string path) => ListBlockFormatter.SplitOrderedListBlock(request, (OrderedListBlock)block, contentWidth, availableHeight, path),
                static (RichTextBoxLayoutRequest request, TextBoxLayoutResult rootLayout, RichTextBlock block, string path, TextLayoutAnalysisContext? context) => BuildListItemNodes(request, rootLayout, ((OrderedListBlock)block).Items, path, context)),
            [typeof(TableBlock)] = new(
                TextLayoutNodeKind.Table,
                static (RichTextBoxLayoutRequest request, RichTextBlock block, double contentWidth, double xOffset, string path, ref LayoutState state) => TableBlockFormatter.AppendTable(request, (TableBlock)block, contentWidth, xOffset, path, ref state),
                static (RichTextBoxLayoutRequest request, RichTextBlock block, double contentWidth, double availableHeight, string path) => TableBlockFormatter.SplitTableBlock(request, (TableBlock)block, contentWidth, availableHeight, path),
                static (RichTextBoxLayoutRequest request, TextBoxLayoutResult rootLayout, RichTextBlock block, string path, TextLayoutAnalysisContext? context) => BuildTableNodes(request, rootLayout, (TableBlock)block, path, context)),
            [typeof(RowBlock)] = new(
                TextLayoutNodeKind.RowContainer,
                static (RichTextBoxLayoutRequest request, RichTextBlock block, double contentWidth, double xOffset, string path, ref LayoutState state) => ContainerBlockFormatter.AppendRow(request, (RowBlock)block, contentWidth, xOffset, path, ref state),
                static (RichTextBoxLayoutRequest request, RichTextBlock block, double contentWidth, double availableHeight, string path) => ContainerBlockFormatter.SplitRowBlock(request, (RowBlock)block, contentWidth, availableHeight, path),
                static (RichTextBoxLayoutRequest request, TextBoxLayoutResult rootLayout, RichTextBlock block, string path, TextLayoutAnalysisContext? context) => BuildLayoutChildNodes(request, rootLayout, ((RowBlock)block).Children, path, context)),
            [typeof(ColumnBlock)] = new(
                TextLayoutNodeKind.ColumnContainer,
                static (RichTextBoxLayoutRequest request, RichTextBlock block, double contentWidth, double xOffset, string path, ref LayoutState state) => ContainerBlockFormatter.AppendColumn(request, (ColumnBlock)block, contentWidth, xOffset, path, ref state),
                static (RichTextBoxLayoutRequest request, RichTextBlock block, double contentWidth, double availableHeight, string path) => ContainerBlockFormatter.SplitColumnBlock(request, (ColumnBlock)block, contentWidth, availableHeight, path),
                static (RichTextBoxLayoutRequest request, TextBoxLayoutResult rootLayout, RichTextBlock block, string path, TextLayoutAnalysisContext? context) => BuildLayoutChildNodes(request, rootLayout, ((ColumnBlock)block).Children, path, context))
        };

    private static RichBlockFormatterRegistration GetFormatter(RichTextBlock block)
        => FormatterRegistry.TryGetValue(block.GetType(), out var formatter)
            ? formatter
            : throw new NotSupportedException($"Unsupported rich text block type: {block.GetType().Name}");

    public TextLayoutPlan Analyze(RichTextBoxLayoutRequest request)
        => Analyze(request, context: null);

    public TextLayoutPlan Analyze(RichTextBoxLayoutRequest request, TextLayoutAnalysisContext? context)
    {
        var layout = Layout(request);
        return new TextLayoutPlan
        {
            Kind = TextLayoutPlanKind.RichText,
            Root = BuildRichPlanRoot(request, layout, context),
            RenderPlan = new TextLayoutRenderPlan
            {
                Layout = layout
            }
        };
    }

    public TextLayoutFitPlan AnalyzeFit(RichTextBoxLayoutRequest request)
        => AnalyzeFit(request, context: null);

    public TextLayoutFitPlan AnalyzeFit(RichTextBoxLayoutRequest request, TextLayoutAnalysisContext? context)
    {
        var fullPlan = Analyze(request, context);
        var fit = Fit(request);
        var fittedPlan = CanSliceFittedPlan(fit.SplitMetadata)
            ? TextLayoutPlanSlicer.Slice(fullPlan, fit.FittedLayout)
            : Analyze(request with { Blocks = fit.FittedContent.Blocks }, context);
        var remainderContent = fit.RemainderContent;
        var remainderPlan = remainderContent is null
            ? null
            : Analyze(request with { Blocks = remainderContent.Blocks }, context);
        var continuations = BuildRichContinuations(fittedPlan, remainderPlan, fit.SplitMetadata);

        return new TextLayoutFitPlan
        {
            FittedSelection = new TextLayoutPlanSelection
            {
                Plan = fittedPlan,
                SourceReferences = CollectRichSourceReferences(fittedPlan),
                BoundaryReferences = CollectBoundaryReferences(fit.SplitMetadata),
                Continuations = continuations,
                FragmentMetadata = BuildFragmentMetadata(fit.FragmentBreak, continuations),
                StartLineIndex = fittedPlan.Root.StartLineIndex,
                EndLineIndexExclusive = fittedPlan.Root.EndLineIndexExclusive
            },
            RemainderSelection = remainderPlan is null
                ? null
                : new TextLayoutPlanSelection
                {
                    Plan = remainderPlan,
                    SourceReferences = CollectRichSourceReferences(remainderPlan),
                    BoundaryReferences = CollectBoundaryReferences(fit.SplitMetadata),
                    Continuations = Array.Empty<TextLayoutContinuationReference>(),
                    FragmentMetadata = new TextLayoutFragmentMetadata(TextFragmentBreak.None),
                    StartLineIndex = remainderPlan.Root.StartLineIndex,
                    EndLineIndexExclusive = remainderPlan.Root.EndLineIndexExclusive
                },
            BreakKind = fit.BreakKind,
            HasRemainder = fit.HasRemainder,
            FragmentBreak = fit.FragmentBreak,
            Materializer = new RichFitPlanMaterializer(fit.FittedContent, remainderContent)
        };
    }

    public TextLayoutFitPlan AnalyzeFragment(RichTextBoxLayoutRequest request)
        => AnalyzeFragment(request, context: null);

    public TextLayoutFitPlan AnalyzeFragment(RichTextBoxLayoutRequest request, TextLayoutAnalysisContext? context)
    {
        if (request.OverflowMode == TextOverflowMode.Fragment)
        {
            return AnalyzeFit(request, context);
        }

        var plan = Analyze(request, context);
        return new TextLayoutFitPlan
        {
            FittedSelection = new TextLayoutPlanSelection
            {
                Plan = plan,
                SourceReferences = CollectRichSourceReferences(plan),
                BoundaryReferences = Array.Empty<TextLayoutSourceReference>(),
                Continuations = Array.Empty<TextLayoutContinuationReference>(),
                FragmentMetadata = new TextLayoutFragmentMetadata(TextFragmentBreak.None),
                StartLineIndex = plan.Root.StartLineIndex,
                EndLineIndexExclusive = plan.Root.EndLineIndexExclusive
            },
            RemainderSelection = null,
            BreakKind = TextBreakKind.None,
            HasRemainder = false,
            FragmentBreak = TextFragmentBreak.None,
            Materializer = new RichFitPlanMaterializer(new RichContentSlice(request.Blocks), null)
        };
    }

    public RichTextBoxFitResult Fragment(RichTextBoxLayoutRequest request)
    {
        if (request.OverflowMode != TextOverflowMode.Fragment)
        {
            var layout = Layout(request);
            return new RichTextBoxFitResult(
                layout,
                new RichContentSlice(request.Blocks),
                null,
                Math.Max(0d, layout.NaturalHeight - StyleResolver.Resolve(request.BoxStyle).Edges.VerticalInset),
                Math.Max(0d, layout.NaturalWidth - StyleResolver.Resolve(request.BoxStyle).Edges.HorizontalInset),
                TextBreakKind.None,
                false)
            {
                FragmentBreak = TextFragmentBreak.None
            };
        }

        return Fit(request);
    }

    private static bool CanSliceFittedPlan(IReadOnlyList<RichLayoutSplitMetadata> metadata)
        => !metadata.OfType<TableSplitMetadata>().Any(x => x.UsedContinuationFooter);

    public TextBoxLayoutResult Layout(RichTextBoxLayoutRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.FontLibrary);
        ArgumentNullException.ThrowIfNull(request.Blocks);

        if (request.Width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(request), "Width must be greater than zero.");
        }

        if (request.Height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(request), "Height must be greater than zero.");
        }

        var resolvedRequestBox = StyleResolver.Resolve(request.BoxStyle);
        var requestEdges = resolvedRequestBox.Edges;
        var contentWidth = GetContentWidth(request);
        var contentHeight = GetContentHeight(request);
        if (contentWidth <= 0 || contentHeight <= 0)
        {
            return CreateNoContentAreaResult(request);
        }

        var state = new LayoutState();
        LayoutBlocks(request, request.Blocks, contentWidth, 0d, "Blocks", ref state);

        var measuredHeight = state.ConsumedHeight;
        var measuredWidth = GetMaxRight(state.Lines, state.Decorations);
        var fitsHeight = measuredHeight <= contentHeight + 0.0001d;
        var fitsWidth = measuredWidth <= contentWidth + 0.0001d;
        var hasFontIssues = state.Issues.Any(x => x.Kind is TextLayoutIssueKind.MissingFamily or TextLayoutIssueKind.MissingWeight or TextLayoutIssueKind.MissingGlyph);

        var status = hasFontIssues
            ? TextLayoutStatus.Error
            : (!fitsWidth || !fitsHeight || state.HadOverflow ? TextLayoutStatus.Overflow : TextLayoutStatus.Success);

        if (status == TextLayoutStatus.Overflow && request.OverflowMode == TextOverflowMode.Error)
        {
            state.Issues.Add(new TextLayoutIssue(TextLayoutIssueKind.Overflow, "Text content exceeds the target text box."));
            status = TextLayoutStatus.Error;
        }

        var clipToViewport = request.OverflowMode is TextOverflowMode.Clip or TextOverflowMode.Fragment;
        var renderedLines = clipToViewport && request.VerticalAlignment == TextVerticalAlignment.Top
            ? ClipLines(contentHeight, state.Lines)
            : state.Lines.ToList();
        var renderedDecorationIntents = clipToViewport && request.VerticalAlignment == TextVerticalAlignment.Top
            ? ClipDecorations(contentHeight, state.Decorations)
            : state.Decorations.ToList();

        if ((!fitsWidth || !fitsHeight) && status != TextLayoutStatus.Error)
        {
            state.Issues.Add(new TextLayoutIssue(TextLayoutIssueKind.Overflow, "Text content exceeds the target text box."));
        }

        var renderedContentHeight = Math.Max(GetMaxBottom(renderedLines), GetMaxBottom(renderedDecorationIntents));
        ApplyVerticalAlignment(request, renderedLines, renderedDecorationIntents, renderedContentHeight, contentHeight);
        if (clipToViewport && request.VerticalAlignment != TextVerticalAlignment.Top)
        {
            renderedLines = ClipPositionedLines(0d, contentHeight, renderedLines);
            renderedDecorationIntents = ClipPositionedDecorations(0d, contentHeight, renderedDecorationIntents);
        }

        renderedContentHeight = Math.Max(GetMaxBottom(renderedLines), GetMaxBottom(renderedDecorationIntents));
        var renderedContentWidth = Math.Max(GetMaxRight(renderedLines), GetMaxRight(renderedDecorationIntents));
        OffsetLines(renderedLines, requestEdges.Insets.Left, requestEdges.Insets.Top);
        OffsetDecorations(renderedDecorationIntents, requestEdges.Insets.Left, requestEdges.Insets.Top);

        return new TextBoxLayoutResult
        {
            Status = status,
            FitsWidth = fitsWidth,
            FitsHeight = fitsHeight,
            MeasuredWidth = measuredWidth + requestEdges.HorizontalInset,
            MeasuredHeight = measuredHeight + requestEdges.VerticalInset,
            RenderedWidth = renderedContentWidth + requestEdges.HorizontalInset,
            RenderedHeight = renderedContentHeight + requestEdges.VerticalInset,
            BoxStyle = request.BoxStyle,
            Lines = renderedLines,
            Issues = state.Issues.ToArray(),
            Decorations = renderedDecorationIntents.Select(TextLayoutDecorationIntentMapper.ToDecoration).ToArray()
        };
    }

    public RichTextBoxFitResult Fit(RichTextBoxLayoutRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.FontLibrary);
        ArgumentNullException.ThrowIfNull(request.Blocks);

        if (request.Width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(request), "Width must be greater than zero.");
        }

        if (request.Height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(request), "Height must be greater than zero.");
        }

        var contentWidth = GetContentWidth(request);
        var contentHeight = GetContentHeight(request);
        if (contentWidth <= 0 || contentHeight <= 0)
        {
            var noArea = CreateNoContentAreaResult(request);
            return new RichTextBoxFitResult(
                noArea,
                new RichContentSlice(Array.Empty<RichTextBlock>()),
                new RichContentSlice(request.Blocks),
                0d,
                0d,
                TextBreakKind.None,
                true)
            {
                FragmentBreak = new TextFragmentBreak(TextFragmentBreakReason.Overflow, TextBreakKind.None, false)
            };
        }

        var outcome = FitBlocks(request, request.Blocks, contentWidth, contentHeight, "Blocks");
        var fittingRequest = request with { Blocks = outcome.FittedBlocks };
        var fittingLayout = new RichTextBoxLayoutEngine().Layout(fittingRequest);
        var fittingSlice = new RichContentSlice(
            outcome.FittedBlocks,
            0,
            outcome.IsOpenEnd ? 1 : 0,
            outcome.Metadata.Select(x => new RichSliceCut(x.Path, x.Kind, false, true)).ToArray());
        var remainderSlice = outcome.RemainderBlocks.Count == 0
            ? null
            : new RichContentSlice(
                outcome.RemainderBlocks,
                outcome.IsOpenStart ? 1 : 0,
                0,
                outcome.Metadata.Select(x => new RichSliceCut(x.Path, x.Kind, true, false)).ToArray());

        return new RichTextBoxFitResult(
            fittingLayout,
            fittingSlice,
            remainderSlice,
            Math.Max(0d, fittingLayout.NaturalHeight - StyleResolver.Resolve(request.BoxStyle).Edges.VerticalInset),
            Math.Max(0d, fittingLayout.NaturalWidth - StyleResolver.Resolve(request.BoxStyle).Edges.HorizontalInset),
            outcome.BreakKind,
            outcome.RemainderBlocks.Count > 0,
            outcome.Metadata)
        {
            FragmentBreak = BuildRichFragmentBreak(outcome.BreakKind, outcome.Metadata)
        };
    }

    private static void LayoutBlocks(RichTextBoxLayoutRequest request, IReadOnlyList<RichTextBlock> blocks, double contentWidth, double xOffset, string pathPrefix, ref LayoutState state)
    {
        for (var i = 0; i < blocks.Count; i++)
        {
            var block = blocks[i];
            var path = $"{pathPrefix}[{i}]";
            GetFormatter(block).Append(request, block, contentWidth, xOffset, path, ref state);
        }
    }

    private static BlockFitOutcome FitBlocks(
        RichTextBoxLayoutRequest request,
        IReadOnlyList<RichTextBlock> blocks,
        double contentWidth,
        double availableHeight,
        string pathPrefix)
    {
        var fitted = new List<RichTextBlock>();
        var remainder = new List<RichTextBlock>();
        var metadata = new List<RichLayoutSplitMetadata>();
        var fittedHeight = 0d;

        for (var i = 0; i < blocks.Count; i++)
        {
            var block = blocks[i];
            var path = $"{pathPrefix}[{i}]";
            if (block.BreakBefore == TextFlowBreakBefore.Always && fitted.Count > 0)
            {
                remainder.Add(block);
                for (var j = i + 1; j < blocks.Count; j++)
                {
                    remainder.Add(blocks[j]);
                }

                metadata.Add(new RichLayoutSplitMetadata(path, GetBreakKindForBlock(block), TextFragmentBreakReason.ForcedBreakBefore, true));
                return new BlockFitOutcome(fitted, remainder, GetBreakKindForBlock(block), metadata, false, false);
            }

            var candidateFitted = fitted.Concat(new[] { block }).ToArray();
            var candidateLayout = MeasureBlocks(request, candidateFitted, contentWidth);
            var candidateHeight = Math.Max(0d, candidateLayout.NaturalHeight);
            if (FitsWithinAvailableHeight(candidateLayout, availableHeight))
            {
                fitted.Add(block);
                fittedHeight = candidateHeight;
                if (block.BreakAfter == TextFlowBreakAfter.Always)
                {
                    for (var j = i + 1; j < blocks.Count; j++)
                    {
                        remainder.Add(blocks[j]);
                    }

                    if (remainder.Count > 0)
                    {
                        metadata.Add(new RichLayoutSplitMetadata(path, GetBreakKindForBlock(block), TextFragmentBreakReason.ForcedBreakAfter, true));
                        return new BlockFitOutcome(fitted, remainder, GetBreakKindForBlock(block), metadata, false, false);
                    }
                }

                continue;
            }

            var remainingHeight = Math.Max(0d, availableHeight - fittedHeight);
            if (block.BreakInside == TextFlowBreakInside.Avoid && fitted.Count > 0)
            {
                remainder.Add(block);
                for (var j = i + 1; j < blocks.Count; j++)
                {
                    remainder.Add(blocks[j]);
                }

                metadata.Add(new RichLayoutSplitMetadata(path, GetBreakKindForBlock(block), TextFragmentBreakReason.Overflow, false));
                return new BlockFitOutcome(fitted, remainder, GetBreakKindForBlock(block), metadata, false, false);
            }

            var split = SplitBlock(request, block, contentWidth, remainingHeight, path);
            if (split.FittedBlock is not null)
            {
                var splitCandidate = fitted.Concat(new[] { split.FittedBlock }).ToArray();
                var splitCandidateLayout = MeasureBlocks(request, splitCandidate, contentWidth);
                var splitCandidateHeight = Math.Max(0d, splitCandidateLayout.NaturalHeight);
                if (FitsWithinAvailableHeight(splitCandidateLayout, availableHeight))
                {
                    fitted.Add(split.FittedBlock);
                    fittedHeight = splitCandidateHeight;
                }
                else
                {
                    split = split with
                    {
                        FittedBlock = null,
                        RemainderBlock = block,
                        BreakKind = split.BreakKind == TextBreakKind.None ? TextBreakKind.Line : split.BreakKind,
                        Metadata = split.Metadata.Count == 0 ? new[] { new RichLayoutSplitMetadata(path, TextBreakKind.Line) } : split.Metadata,
                        IsOpenStart = true,
                        IsOpenEnd = true
                    };
                }
            }

            if (split.RemainderBlock is not null)
            {
                remainder.Add(split.RemainderBlock);
            }

            for (var j = i + 1; j < blocks.Count; j++)
            {
                remainder.Add(blocks[j]);
            }

            metadata.AddRange(split.Metadata);
            return new BlockFitOutcome(
                fitted,
                remainder,
                split.BreakKind,
                metadata,
                split.IsOpenStart,
                split.IsOpenEnd);
        }

        return new BlockFitOutcome(fitted, remainder, TextBreakKind.None, metadata, false, false);
    }

    private static BlockSplitOutcome SplitBlock(
        RichTextBoxLayoutRequest request,
        RichTextBlock block,
        double contentWidth,
        double availableHeight,
        string path)
        => GetFormatter(block).Split(request, block, contentWidth, availableHeight, path);

    private static BlockSplitOutcome SplitTableBlock(
        RichTextBoxLayoutRequest request,
        TableBlock table,
        double contentWidth,
        double availableHeight,
        string path)
        => TableBlockFormatter.SplitTableBlock(request, table, contentWidth, availableHeight, path);

    private static BlockSplitOutcome SplitColumnBlock(
        RichTextBoxLayoutRequest request,
        ColumnBlock column,
        double contentWidth,
        double availableHeight,
        string path)
    {
        if (column.Height.HasValue)
        {
            return new BlockSplitOutcome(null, column, TextBreakKind.ContainerChild, new[] { new RichLayoutSplitMetadata(path, TextBreakKind.ContainerChild) }, false, false);
        }

        var style = column.Style ?? new LayoutContainerStyle();
        var computedStyle = StyleResolver.Resolve(style);
        var edges = computedStyle.Box.Edges;
        var gap = computedStyle.Gap;
        var innerWidth = Math.Max(1d, contentWidth - edges.HorizontalInset);
        var innerAvailableHeight = Math.Max(0d, availableHeight - edges.VerticalInset);
        var fittedChildren = new List<LayoutChild>();
        var remainderChildren = new List<LayoutChild>();
        var metadata = new List<RichLayoutSplitMetadata>();
        var remainingHeight = innerAvailableHeight;

        for (var i = 0; i < column.Children.Count; i++)
        {
            var child = column.Children[i];
            var childEdges = StyleResolver.Resolve(child.BoxStyle).Edges;
            var childBlock = new ColumnBlock(new[] { child }, null, new LayoutContainerStyle());
            var childLayout = MeasureBlocks(request, new RichTextBlock[] { childBlock }, innerWidth);
            var childHeight = childLayout.NaturalHeight;
            var gapBefore = fittedChildren.Count > 0 ? gap : 0d;
            if (FitsWithinAvailableHeight(childLayout, Math.Max(0d, remainingHeight - gapBefore)))
            {
                fittedChildren.Add(child);
                remainingHeight -= childHeight + gapBefore;
                continue;
            }

            var splitHeight = Math.Max(0d, remainingHeight - gapBefore);
            var split = FitBlocks(request, child.Blocks, Math.Max(1d, innerWidth - childEdges.HorizontalInset), Math.Max(0d, splitHeight - childEdges.VerticalInset), $"{path}.Children[{i}].Blocks");
            if (split.FittedBlocks.Count > 0)
            {
                fittedChildren.Add(child with { Blocks = split.FittedBlocks });
            }

            if (split.RemainderBlocks.Count > 0)
            {
                remainderChildren.Add(child with { Blocks = split.RemainderBlocks });
            }

            for (var j = i + 1; j < column.Children.Count; j++)
            {
                remainderChildren.Add(column.Children[j]);
            }

            metadata.Add(new RichLayoutSplitMetadata($"{path}.Children[{i}]", TextBreakKind.ContainerChild));
            metadata.AddRange(split.Metadata);
            return new BlockSplitOutcome(
                fittedChildren.Count == 0 ? null : new ColumnBlock(fittedChildren, null, column.Style),
                remainderChildren.Count == 0 ? null : new ColumnBlock(remainderChildren, null, column.Style),
                TextBreakKind.ContainerChild,
                metadata,
                true,
                true);
        }

        return new BlockSplitOutcome(new ColumnBlock(fittedChildren, null, column.Style), null, TextBreakKind.None, metadata, false, false);
    }

    private static BlockSplitOutcome SplitRowBlock(
        RichTextBoxLayoutRequest request,
        RowBlock row,
        double contentWidth,
        double availableHeight,
        string path)
    {
        if (row.Height.HasValue)
        {
            return new BlockSplitOutcome(null, row, TextBreakKind.ContainerChild, new[] { new RichLayoutSplitMetadata(path, TextBreakKind.ContainerChild) }, false, false);
        }

        var style = row.Style ?? new LayoutContainerStyle();
        var computedStyle = StyleResolver.Resolve(style);
        var edges = computedStyle.Box.Edges;
        var gap = computedStyle.Gap;
        var innerWidth = Math.Max(1d, contentWidth - edges.HorizontalInset);
        var innerAvailableHeight = Math.Max(0d, availableHeight - edges.VerticalInset);
        if (innerAvailableHeight <= 0d)
        {
            return new BlockSplitOutcome(null, row, TextBreakKind.ContainerChild, new[] { new RichLayoutSplitMetadata(path, TextBreakKind.ContainerChild) }, false, false);
        }

        var childInsets = row.Children.Select(x => StyleResolver.Resolve(x.BoxStyle).Edges).ToArray();
        var totalGap = gap * Math.Max(0d, row.Children.Count - 1);
        var childWidths = ResolveChildMainSizes(row.Children, Math.Max(0d, innerWidth - totalGap), childInsets.Select(x => x.HorizontalInset).ToArray());
        var fittedChildren = new List<LayoutChild>(row.Children.Count);
        var remainderChildren = new List<LayoutChild>(row.Children.Count);
        var metadata = new List<RichLayoutSplitMetadata>();
        var anyFitted = false;

        for (var i = 0; i < row.Children.Count; i++)
        {
            var child = row.Children[i];
            var childEdges = childInsets[i];
            var split = FitBlocks(
                request,
                child.Blocks,
                Math.Max(1d, childWidths[i] - childEdges.HorizontalInset),
                Math.Max(0d, innerAvailableHeight - childEdges.VerticalInset),
                $"{path}.Children[{i}].Blocks");

            fittedChildren.Add(child with { Blocks = split.FittedBlocks });
            remainderChildren.Add(child with { Blocks = split.RemainderBlocks });
            anyFitted |= split.FittedBlocks.Count > 0;
            metadata.AddRange(split.Metadata);
        }

        if (!anyFitted)
        {
            return new BlockSplitOutcome(null, row, TextBreakKind.ContainerChild, new[] { new RichLayoutSplitMetadata(path, TextBreakKind.ContainerChild) }, false, false);
        }

        var fittedRow = new RowBlock(fittedChildren, null, row.Style);
        var hasRemainder = remainderChildren.Any(x => x.Blocks.Count > 0);
        var remainderRow = hasRemainder ? new RowBlock(remainderChildren, null, row.Style) : null;
        if (hasRemainder)
        {
            metadata.Insert(0, new RichLayoutSplitMetadata(path, TextBreakKind.ContainerChild));
        }

        return new BlockSplitOutcome(fittedRow, remainderRow, hasRemainder ? TextBreakKind.ContainerChild : TextBreakKind.None, metadata, hasRemainder, hasRemainder);
    }

    private static void AppendContainer(
        RichTextBoxLayoutRequest request,
        IReadOnlyList<LayoutChild> children,
        double? fixedHeight,
        LayoutContainerStyle style,
        LayoutAxis axis,
        double contentWidth,
        double xOffset,
        string path,
        ref LayoutState state)
    {
        var outerWidth = Math.Max(0d, contentWidth - xOffset);
        var computedContainerStyle = StyleResolver.Resolve(style);
        var boxStyle = style.ToTextBoxStyle();
        var boxEdges = computedContainerStyle.Box.Edges;
        var innerWidth = Math.Max(1d, outerWidth - boxEdges.HorizontalInset);
        var gap = computedContainerStyle.Gap;

        if (children.Count == 0)
        {
            AddBoxStyleDecorations(state.Decorations, xOffset, state.ConsumedHeight, outerWidth, boxEdges.VerticalInset, computedContainerStyle.Box);
            state.ConsumedHeight += boxEdges.VerticalInset + computedContainerStyle.MarginBlockEnd;
            return;
        }

        if (axis == LayoutAxis.Horizontal)
        {
            AppendHorizontalContainer(request, children, fixedHeight, style, computedContainerStyle, outerWidth, innerWidth, gap, xOffset, path, ref state);
            return;
        }

        AppendVerticalContainer(request, children, fixedHeight, style, computedContainerStyle, outerWidth, innerWidth, gap, xOffset, path, ref state);
    }

    private static void AppendHorizontalContainer(
        RichTextBoxLayoutRequest request,
        IReadOnlyList<LayoutChild> children,
        double? fixedHeight,
        LayoutContainerStyle style,
        ComputedContainerStyle computedContainerStyle,
        double outerWidth,
        double innerWidth,
        double gap,
        double xOffset,
        string path,
        ref LayoutState state)
    {
        var containerTop = state.ConsumedHeight;
        var boxEdges = computedContainerStyle.Box.Edges;
        var insetTop = boxEdges.Insets.Top;
        var insetLeft = boxEdges.Insets.Left;
        var totalGap = gap * Math.Max(0, children.Count - 1);
        var availableWidth = Math.Max(0d, innerWidth - totalGap);
        var childInsets = ArrayPool<double>.Shared.Rent(children.Count);
        var chromeSizes = ArrayPool<double>.Shared.Rent(children.Count);
        try
        {
            for (var i = 0; i < children.Count; i++)
            {
                var childEdges = StyleResolver.Resolve(children[i].BoxStyle).Edges;
                childInsets[i] = childEdges.Insets.Top;
                chromeSizes[i] = childEdges.VerticalInset;
            }

            var childWidths = ResolveChildMainSizes(children, availableWidth, chromeSizes.AsSpan(0, children.Count).ToArray());

            var childLayouts = new List<BlockLayoutResult>(children.Count);
            var childHeights = new double[children.Count];
            double rowInnerHeight;

            if (fixedHeight.HasValue)
            {
                rowInnerHeight = Math.Max(0d, fixedHeight.Value - boxEdges.VerticalInset);
                for (var i = 0; i < children.Count; i++)
                {
                    var childEdges = StyleResolver.Resolve(children[i].BoxStyle).Edges;
                    var layout = MeasureChildBlocks(
                        request,
                        children[i],
                        Math.Max(1d, childWidths[i] - childEdges.HorizontalInset),
                        Math.Max(1d, rowInnerHeight - childEdges.VerticalInset),
                        children[i].VerticalAlignment,
                        true,
                        context: null,
                        pathPrefix: $"{path}.Children[{i}].Blocks");
                    childLayouts.Add(layout);
                    childHeights[i] = rowInnerHeight;
                    state.Issues.AddRange(layout.Layout.Issues);
                    state.HadOverflow |= layout.Layout.Status == TextLayoutStatus.Overflow;
                }
            }
            else
            {
                var initialLayouts = new List<BlockLayoutResult>(children.Count);
                for (var i = 0; i < children.Count; i++)
                {
                    var childEdges = StyleResolver.Resolve(children[i].BoxStyle).Edges;
                    var layout = MeasureChildBlocks(
                        request,
                        children[i],
                        Math.Max(1d, childWidths[i] - childEdges.HorizontalInset),
                        LargeLayoutHeight,
                        TextVerticalAlignment.Top,
                        false,
                        context: null,
                        pathPrefix: $"{path}.Children[{i}].Blocks");
                    initialLayouts.Add(layout);
                    childHeights[i] = layout.NaturalHeight + childEdges.VerticalInset;
                }

                rowInnerHeight = childHeights.Length == 0 ? 0d : childHeights.Max();
                for (var i = 0; i < children.Count; i++)
                {
                    if (children[i].VerticalAlignment == TextVerticalAlignment.Top)
                    {
                        childLayouts.Add(initialLayouts[i]);
                        state.Issues.AddRange(initialLayouts[i].Layout.Issues);
                        state.HadOverflow |= initialLayouts[i].Layout.Status == TextLayoutStatus.Overflow;
                        continue;
                    }

                    var childEdges = StyleResolver.Resolve(children[i].BoxStyle).Edges;
                    var layout = MeasureChildBlocks(
                        request,
                        children[i],
                        Math.Max(1d, childWidths[i] - childEdges.HorizontalInset),
                        Math.Max(1d, rowInnerHeight - childEdges.VerticalInset),
                        children[i].VerticalAlignment,
                        false,
                        context: null,
                        pathPrefix: $"{path}.Children[{i}].Blocks");
                    childLayouts.Add(layout);
                    state.Issues.AddRange(layout.Layout.Issues);
                    state.HadOverflow |= layout.Layout.Status == TextLayoutStatus.Overflow;
                }
            }

            var outerHeight = rowInnerHeight + boxEdges.VerticalInset;
            AddBoxStyleDecorations(state.Decorations, xOffset, containerTop, outerWidth, outerHeight, computedContainerStyle.Box);

            var childX = xOffset + insetLeft;
            for (var i = 0; i < children.Count; i++)
            {
                var resolvedChildBoxStyle = StyleResolver.Resolve(children[i].BoxStyle);
                var childEdges = resolvedChildBoxStyle.Edges;
                if (children[i].BoxStyle is not null)
                {
                    AddBoxStyleDecorations(state.Decorations, childX, containerTop + insetTop, childWidths[i], rowInnerHeight, resolvedChildBoxStyle);
                }

                AppendLines(state.Lines, childLayouts[i].Layout.Lines, containerTop + insetTop + childEdges.Insets.Top, childX + childEdges.Insets.Left);
                AppendDecorations(state.Decorations, childLayouts[i].Layout.Decorations, containerTop + insetTop + childEdges.Insets.Top, childX + childEdges.Insets.Left);
                childX += childWidths[i] + gap;
            }

            state.ConsumedHeight += outerHeight + computedContainerStyle.MarginBlockEnd;
        }
        finally
        {
            ArrayPool<double>.Shared.Return(childInsets, clearArray: true);
            ArrayPool<double>.Shared.Return(chromeSizes, clearArray: true);
        }
    }

    private static void AppendVerticalContainer(
        RichTextBoxLayoutRequest request,
        IReadOnlyList<LayoutChild> children,
        double? fixedHeight,
        LayoutContainerStyle style,
        ComputedContainerStyle computedContainerStyle,
        double outerWidth,
        double innerWidth,
        double gap,
        double xOffset,
        string path,
        ref LayoutState state)
    {
        var containerTop = state.ConsumedHeight;
        var boxEdges = computedContainerStyle.Box.Edges;
        var insetTop = boxEdges.Insets.Top;
        var insetLeft = boxEdges.Insets.Left;
        var childLayouts = new List<BlockLayoutResult>(children.Count);
        var childHeights = new double[children.Count];
        var childInsets = ArrayPool<double>.Shared.Rent(children.Count);
        var chromeSizes = ArrayPool<double>.Shared.Rent(children.Count);
        try
        {
            for (var i = 0; i < children.Count; i++)
            {
                var childEdges = StyleResolver.Resolve(children[i].BoxStyle).Edges;
                childInsets[i] = childEdges.Insets.Top;
                chromeSizes[i] = childEdges.VerticalInset;
            }

            if (fixedHeight.HasValue)
            {
                var totalGap = gap * Math.Max(0, children.Count - 1);
                var availableHeight = Math.Max(0d, fixedHeight.Value - boxEdges.VerticalInset - totalGap);
                childHeights = ResolveChildMainSizes(children, availableHeight, chromeSizes.AsSpan(0, children.Count).ToArray());
                for (var i = 0; i < children.Count; i++)
                {
                    var childEdges = StyleResolver.Resolve(children[i].BoxStyle).Edges;
                    var layout = MeasureChildBlocks(
                        request,
                        children[i],
                        Math.Max(1d, innerWidth - childEdges.HorizontalInset),
                        Math.Max(1d, childHeights[i] - childEdges.VerticalInset),
                        children[i].VerticalAlignment,
                        true,
                        context: null,
                        pathPrefix: $"{path}.Children[{i}].Blocks");
                    childLayouts.Add(layout);
                    state.Issues.AddRange(layout.Layout.Issues);
                    state.HadOverflow |= layout.Layout.Status == TextLayoutStatus.Overflow;
                }
            }
            else
            {
                for (var i = 0; i < children.Count; i++)
                {
                    var childHeight = children[i].FixedSize;
                    var childEdges = StyleResolver.Resolve(children[i].BoxStyle).Edges;
                    var layout = MeasureChildBlocks(
                        request,
                        children[i],
                        Math.Max(1d, innerWidth - childEdges.HorizontalInset),
                        childHeight.HasValue ? Math.Max(1d, childHeight.Value - childEdges.VerticalInset) : LargeLayoutHeight,
                        childHeight.HasValue ? children[i].VerticalAlignment : TextVerticalAlignment.Top,
                        false,
                        context: null,
                        pathPrefix: $"{path}.Children[{i}].Blocks");
                    childLayouts.Add(layout);
                    childHeights[i] = childHeight ?? (layout.NaturalHeight + childEdges.VerticalInset);
                    state.Issues.AddRange(layout.Layout.Issues);
                    state.HadOverflow |= layout.Layout.Status == TextLayoutStatus.Overflow;
                }
            }

            var innerHeight = childHeights.Sum() + (gap * Math.Max(0, children.Count - 1));
            var outerHeight = fixedHeight ?? (innerHeight + boxEdges.VerticalInset);
            AddBoxStyleDecorations(state.Decorations, xOffset, containerTop, outerWidth, outerHeight, computedContainerStyle.Box);

            var childY = containerTop + insetTop;
            for (var i = 0; i < children.Count; i++)
            {
                var resolvedChildBoxStyle = StyleResolver.Resolve(children[i].BoxStyle);
                var childEdges = resolvedChildBoxStyle.Edges;
                if (children[i].BoxStyle is not null)
                {
                    AddBoxStyleDecorations(state.Decorations, xOffset + insetLeft, childY, innerWidth, childHeights[i], resolvedChildBoxStyle);
                }

                AppendLines(state.Lines, childLayouts[i].Layout.Lines, childY + childEdges.Insets.Top, xOffset + insetLeft + childEdges.Insets.Left);
                AppendDecorations(state.Decorations, childLayouts[i].Layout.Decorations, childY + childEdges.Insets.Top, xOffset + insetLeft + childEdges.Insets.Left);
                childY += childHeights[i] + gap;
            }

            state.ConsumedHeight += outerHeight + computedContainerStyle.MarginBlockEnd;
        }
        finally
        {
            ArrayPool<double>.Shared.Return(childInsets, clearArray: true);
            ArrayPool<double>.Shared.Return(chromeSizes, clearArray: true);
        }
    }

    private static void AppendTable(
        RichTextBoxLayoutRequest request,
        TableBlock table,
        double contentWidth,
        double xOffset,
        ref LayoutState state)
    {
        if (table.Rows.Count == 0)
        {
            state.ConsumedHeight += (table.Style ?? new TableStyle()).MarginBlockEnd;
            return;
        }

        var tableStyle = StyleResolver.Resolve(table.Style);
        var placements = BuildCellPlacements(table);
        if (placements.Count == 0)
        {
            state.ConsumedHeight += tableStyle.MarginBlockEnd;
            return;
        }

        var columnCount = placements.Max(x => x.ColumnIndex + x.ColSpan);
        var rowCount = table.Rows.Count;
        var columnWidths = ResolveColumnWidths(request, table, placements, columnCount, Math.Max(1d, contentWidth - xOffset));
        var rowHeights = new double[rowCount];
        var measuredCells = new List<MeasuredCell>(placements.Count);

        foreach (var placement in placements)
        {
            var outerWidth = GetSpanWidth(columnWidths, placement.ColumnIndex, placement.ColSpan);
            var cellStyle = StyleResolver.Resolve(placement.Cell.Style, tableStyle);
            var innerWidth = Math.Max(1d, outerWidth - (cellStyle.Padding.Horizontal + tableStyle.CellBorder.Widths.Horizontal));
            var cellBlocks = ApplyCellAlignment(placement.Cell, cellStyle.TextAlign);
            var cellLayout = MeasureBlocks(request, cellBlocks, innerWidth);
            var inset = tableStyle.CellBorder.MaxWidth + cellStyle.Padding.Top;
            measuredCells.Add(new MeasuredCell(placement, cellLayout.Layout, inset, outerWidth, cellLayout.NaturalHeight + cellStyle.Padding.Vertical + tableStyle.CellBorder.Widths.Vertical));
            state.Issues.AddRange(cellLayout.Layout.Issues);
            state.HadOverflow |= cellLayout.Layout.Status == TextLayoutStatus.Overflow;

            if (placement.RowSpan == 1)
            {
                rowHeights[placement.RowIndex] = Math.Max(rowHeights[placement.RowIndex], cellLayout.NaturalHeight + (inset * 2d));
            }
        }

        foreach (var cell in measuredCells.Where(x => x.Placement.RowSpan > 1))
        {
            var span = Math.Min(cell.Placement.RowSpan, rowCount - cell.Placement.RowIndex);
            var coveredHeight = Sum(rowHeights, cell.Placement.RowIndex, span);
            if (coveredHeight >= cell.OuterHeight - 0.0001d)
            {
                continue;
            }

            var extra = cell.OuterHeight - coveredHeight;
            var weights = new double[span];
            var totalWeight = 0d;
            for (var i = 0; i < span; i++)
            {
                weights[i] = Math.Max(0d, rowHeights[cell.Placement.RowIndex + i]);
                totalWeight += weights[i];
            }

            if (totalWeight <= 0d)
            {
                var equalExtra = extra / span;
                for (var i = 0; i < span; i++)
                {
                    rowHeights[cell.Placement.RowIndex + i] += equalExtra;
                }

                continue;
            }

            for (var i = 0; i < span; i++)
            {
                rowHeights[cell.Placement.RowIndex + i] += extra * (weights[i] / totalWeight);
            }
        }

        for (var rowIndex = 0; rowIndex < rowHeights.Length; rowIndex++)
        {
            if (rowHeights[rowIndex] <= 0)
            {
                rowHeights[rowIndex] = ResolveFallbackRowHeight(measuredCells, rowIndex);
            }

            var resolvedRowStyle = StyleResolver.Resolve(table.Rows[rowIndex].Style, table.Pagination);
            if (resolvedRowStyle.MinHeight.HasValue)
            {
                rowHeights[rowIndex] = Math.Max(rowHeights[rowIndex], resolvedRowStyle.MinHeight.Value);
            }
        }

        var rowTops = new double[rowCount];
        var cursor = state.ConsumedHeight;
        for (var rowIndex = 0; rowIndex < rowCount; rowIndex++)
        {
            rowTops[rowIndex] = cursor;
            cursor += rowHeights[rowIndex];
        }

        var columnLefts = new double[columnCount];
        var xCursor = xOffset;
        for (var columnIndex = 0; columnIndex < columnCount; columnIndex++)
        {
            columnLefts[columnIndex] = xCursor;
            xCursor += columnWidths[columnIndex];
        }

        var tableWidth = columnWidths.Sum();
        var tableHeight = rowHeights.Sum();
        AddTableDecorations(state.Decorations, tableStyle, measuredCells, columnLefts, rowTops, rowHeights, tableWidth, tableHeight, xOffset, state.ConsumedHeight);

        foreach (var cell in measuredCells)
        {
            var cellX = columnLefts[cell.Placement.ColumnIndex];
            var cellY = rowTops[cell.Placement.RowIndex];
            var innerX = cellX + cell.Inset;
            var innerY = cellY + cell.Inset;
            AppendLines(state.Lines, cell.Layout.Lines, innerY, innerX);
            AppendDecorations(state.Decorations, cell.Layout.Decorations, innerY, innerX);
        }

        state.ConsumedHeight += tableHeight + tableStyle.MarginBlockEnd;
    }

    private static List<CellPlacement> BuildCellPlacements(TableBlock table)
    {
        var occupied = new HashSet<(int Row, int Col)>();
        var placements = new List<CellPlacement>();
        for (var rowIndex = 0; rowIndex < table.Rows.Count; rowIndex++)
        {
            var row = table.Rows[rowIndex];
            var columnIndex = 0;
            for (var cellIndex = 0; cellIndex < row.Cells.Count; cellIndex++)
            {
                var cell = row.Cells[cellIndex];
                while (occupied.Contains((rowIndex, columnIndex)))
                {
                    columnIndex++;
                }

                var colSpan = Math.Max(1, cell.ColSpan);
                var rowSpan = Math.Max(1, Math.Min(cell.RowSpan, table.Rows.Count - rowIndex));
                placements.Add(new CellPlacement(cell, rowIndex, cellIndex, columnIndex, colSpan, rowSpan));
                for (var rowOffset = 0; rowOffset < rowSpan; rowOffset++)
                {
                    for (var colOffset = 0; colOffset < colSpan; colOffset++)
                    {
                        occupied.Add((rowIndex + rowOffset, columnIndex + colOffset));
                    }
                }

                columnIndex += colSpan;
            }
        }

        return placements;
    }

    private static double[] ResolveColumnWidths(
        RichTextBoxLayoutRequest request,
        TableBlock table,
        IReadOnlyList<CellPlacement> placements,
        int columnCount,
        double availableWidth)
    {
        var resolvedTableWidth = ResolveTableOuterWidth(table, availableWidth);
        var widths = new double[columnCount];
        var minWidths = new double[columnCount];
        var preferredWidths = new double[columnCount];
        var maxWidths = new double[columnCount];
        var flexWeights = new double[columnCount];

        for (var columnIndex = 0; columnIndex < columnCount; columnIndex++)
        {
            maxWidths[columnIndex] = double.PositiveInfinity;
            var spec = columnIndex < table.Columns.Count
                ? table.Columns[columnIndex].Width
                : new ColumnAutoWidth();
            switch (spec)
            {
                case ColumnFixedWidth fixedWidth:
                    widths[columnIndex] = Math.Max(widths[columnIndex], fixedWidth.Points);
                    minWidths[columnIndex] = Math.Max(minWidths[columnIndex], fixedWidth.Points);
                    preferredWidths[columnIndex] = Math.Max(preferredWidths[columnIndex], fixedWidth.Points);
                    maxWidths[columnIndex] = Math.Min(maxWidths[columnIndex], fixedWidth.Points);
                    break;
                case ColumnPercentWidth percentWidth:
                    var percentPoints = resolvedTableWidth * Math.Max(0d, percentWidth.Percent) / 100d;
                    widths[columnIndex] = Math.Max(widths[columnIndex], percentPoints);
                    minWidths[columnIndex] = Math.Max(minWidths[columnIndex], percentPoints);
                    preferredWidths[columnIndex] = Math.Max(preferredWidths[columnIndex], percentPoints);
                    break;
                case ColumnMinMaxWidth minMaxWidth:
                    if (minMaxWidth.MinPoints.HasValue)
                    {
                        minWidths[columnIndex] = Math.Max(minWidths[columnIndex], Math.Max(0d, minMaxWidth.MinPoints.Value));
                    }

                    if (minMaxWidth.MaxPoints.HasValue)
                    {
                        maxWidths[columnIndex] = Math.Max(0d, minMaxWidth.MaxPoints.Value);
                    }

                    break;
                case ColumnFlexWidth flexWidth:
                    flexWeights[columnIndex] = Math.Max(0d, flexWidth.Weight);
                    break;
            }
        }

        foreach (var placement in placements)
        {
            var cellOuterWidth = MeasureIntrinsicCellOuterWidth(request, table, placement, resolvedTableWidth);
            if (placement.ColSpan == 1)
            {
                minWidths[placement.ColumnIndex] = Math.Max(minWidths[placement.ColumnIndex], Math.Min(cellOuterWidth, resolvedTableWidth));
                preferredWidths[placement.ColumnIndex] = Math.Max(preferredWidths[placement.ColumnIndex], Math.Min(cellOuterWidth, resolvedTableWidth));
                continue;
            }

            var currentSpanWidth = 0d;
            for (var i = 0; i < placement.ColSpan; i++)
            {
                var index = placement.ColumnIndex + i;
                currentSpanWidth += Math.Max(widths[index], Math.Max(preferredWidths[index], minWidths[index]));
            }

            if (currentSpanWidth >= cellOuterWidth - 0.0001d)
            {
                continue;
            }

            var extra = (cellOuterWidth - currentSpanWidth) / placement.ColSpan;
            for (var i = 0; i < placement.ColSpan; i++)
            {
                var index = placement.ColumnIndex + i;
                preferredWidths[index] = Math.Max(preferredWidths[index], Math.Max(widths[index], minWidths[index]) + extra);
            }
        }

        for (var columnIndex = 0; columnIndex < columnCount; columnIndex++)
        {
            widths[columnIndex] = Math.Max(widths[columnIndex], minWidths[columnIndex]);
            widths[columnIndex] = Math.Min(widths[columnIndex], maxWidths[columnIndex]);
            if (widths[columnIndex] <= 0)
            {
                widths[columnIndex] = Math.Min(Math.Max(minWidths[columnIndex], preferredWidths[columnIndex]), maxWidths[columnIndex]);
            }
        }

        var remaining = resolvedTableWidth - widths.Sum();
        if (remaining > 0.0001d)
        {
            var totalFlexWeight = flexWeights.Sum();
            if (totalFlexWeight > 0)
            {
                for (var i = 0; i < widths.Length; i++)
                {
                    if (flexWeights[i] <= 0)
                    {
                        continue;
                    }

                    widths[i] += remaining * (flexWeights[i] / totalFlexWeight);
                }
            }
            else
            {
                var growthPotential = new double[widths.Length];
                var totalGrowthPotential = 0d;
                for (var i = 0; i < widths.Length; i++)
                {
                    growthPotential[i] = Math.Max(0d, Math.Min(maxWidths[i], preferredWidths[i]) - widths[i]);
                    totalGrowthPotential += growthPotential[i];
                }

                if (totalGrowthPotential > 0)
                {
                    for (var i = 0; i < widths.Length; i++)
                    {
                        if (growthPotential[i] <= 0)
                        {
                            continue;
                        }

                        widths[i] += remaining * (growthPotential[i] / totalGrowthPotential);
                    }
                }
                else
                {
                    var autoColumns = Enumerable.Range(0, widths.Length)
                        .Where(i => i >= table.Columns.Count || table.Columns[i].Width is ColumnAutoWidth or ColumnMinMaxWidth)
                        .ToArray();
                    if (autoColumns.Length == 0)
                    {
                        autoColumns = Enumerable.Range(0, widths.Length).ToArray();
                    }

                    var extraPerColumn = remaining / autoColumns.Length;
                    foreach (var index in autoColumns)
                    {
                        widths[index] += extraPerColumn;
                    }
                }
            }
        }
        else if (remaining < -0.0001d)
        {
            var deficit = -remaining;
            deficit = ShrinkColumns(widths, deficit, Enumerable.Range(0, widths.Length).Where(i => flexWeights[i] > 0).ToArray(), minWidths);
            deficit = ShrinkColumns(widths, deficit, Enumerable.Range(0, widths.Length).Where(i => i >= table.Columns.Count || table.Columns[i].Width is ColumnAutoWidth or ColumnMinMaxWidth or ColumnFlexWidth).ToArray(), minWidths);
            deficit = ShrinkColumns(widths, deficit, Enumerable.Range(0, widths.Length).Where(i => i < table.Columns.Count && table.Columns[i].Width is ColumnPercentWidth).ToArray(), minWidths);
            if (deficit > 0.0001d)
            {
                ShrinkColumns(widths, deficit, Enumerable.Range(0, widths.Length).ToArray(), Enumerable.Repeat(0d, widths.Length).ToArray());
            }
        }

        return widths;
    }

    private static double MeasureIntrinsicCellOuterWidth(RichTextBoxLayoutRequest request, TableBlock table, CellPlacement placement, double tableWidth)
    {
        var tableStyle = StyleResolver.Resolve(table.Style);
        var cellStyle = StyleResolver.Resolve(placement.Cell.Style, tableStyle);
        var probeWidth = Math.Max(1d, tableWidth);
        var cellBlocks = ApplyCellAlignment(placement.Cell, cellStyle.TextAlign);
        var layout = MeasureBlocks(request, cellBlocks, probeWidth);
        return Math.Max(1d, layout.NaturalWidth + cellStyle.Padding.Horizontal + tableStyle.CellBorder.Widths.Horizontal);
    }

    private static double ResolveTableOuterWidth(TableBlock table, double availableWidth)
        => table.Width switch
        {
            TableFixedWidth fixedWidth => Math.Max(1d, fixedWidth.Points),
            TablePercentWidth percentWidth => Math.Max(1d, availableWidth * Math.Max(0d, percentWidth.Percent) / 100d),
            _ => Math.Max(1d, availableWidth)
        };

    private static double ShrinkColumns(double[] widths, double deficit, IReadOnlyList<int> indexes, IReadOnlyList<double> minWidths)
    {
        if (deficit <= 0.0001d || indexes.Count == 0)
        {
            return deficit;
        }

        var shrinkable = indexes
            .Select(i => (Index: i, Available: Math.Max(0d, widths[i] - minWidths[i])))
            .Where(x => x.Available > 0.0001d)
            .ToArray();
        if (shrinkable.Length == 0)
        {
            return deficit;
        }

        var totalShrinkable = shrinkable.Sum(x => x.Available);
        foreach (var item in shrinkable)
        {
            var reduction = Math.Min(item.Available, deficit * (item.Available / totalShrinkable));
            widths[item.Index] -= reduction;
            deficit -= reduction;
        }

        return deficit;
    }

    private static double ResolveFallbackRowHeight(IReadOnlyList<MeasuredCell> measuredCells, int rowIndex)
    {
        var fallback = measuredCells
            .Where(x => x.Placement.RowIndex == rowIndex)
            .Select(x => x.OuterHeight)
            .DefaultIfEmpty(0d)
            .Max();
        return Math.Max(0d, fallback);
    }

    private static double GetSpanWidth(IReadOnlyList<double> widths, int startIndex, int span)
    {
        var total = 0d;
        for (var i = 0; i < span; i++)
        {
            total += widths[startIndex + i];
        }

        return total;
    }

    private static double Sum(IReadOnlyList<double> values, int startIndex, int count)
    {
        var total = 0d;
        for (var i = 0; i < count; i++)
        {
            total += values[startIndex + i];
        }

        return total;
    }

    private static IReadOnlyList<RichTextBlock> ApplyCellAlignment(TableCellBlock cell, TextHorizontalAlignment? alignment)
    {
        if (!alignment.HasValue)
        {
            return cell.Blocks;
        }

        return cell.Blocks.Select(block => block switch
        {
            ParagraphBlock paragraph => paragraph with
            {
                Style = (paragraph.Style ?? new ParagraphStyle()) with { TextAlign = alignment.Value }
            },
            HeadingBlock heading => heading with
            {
                Style = (heading.Style ?? new ParagraphStyle()) with { TextAlign = alignment.Value }
            },
            _ => block
        }).ToArray();
    }

    private static void AddTableDecorations(
        List<TextLayoutDecorationIntent> decorations,
        ComputedTableStyle tableStyle,
        IReadOnlyList<MeasuredCell> cells,
        IReadOnlyList<double> columnLefts,
        IReadOnlyList<double> rowTops,
        IReadOnlyList<double> rowHeights,
        double tableWidth,
        double tableHeight,
        double tableLeft,
        double tableTop)
    {
        if (tableStyle.BackgroundColor is TextColor tableBackground)
        {
            decorations.Add(new FillRectDecorationIntent(tableLeft, tableTop, tableWidth, tableHeight, tableBackground));
        }

        foreach (var cell in cells)
        {
            var width = GetCellWidth(columnLefts, tableLeft + tableWidth, cell.Placement.ColumnIndex, cell.Placement.ColSpan);
            var height = Sum(rowHeights, cell.Placement.RowIndex, cell.Placement.RowSpan);

            if (cell.Placement.Cell.Style?.BackgroundColor is not TextColor background)
            {
                if (tableStyle.CellBorder.HasVisibleStroke)
                {
                    decorations.Add(new StrokeRectDecorationIntent(
                        columnLefts[cell.Placement.ColumnIndex],
                        rowTops[cell.Placement.RowIndex],
                        width,
                        height,
                        tableStyle.CellBorder.Color ?? new TextColor(0, 0, 0),
                        tableStyle.CellBorder.MaxWidth));
                }

                continue;
            }

            decorations.Add(new FillRectDecorationIntent(
                columnLefts[cell.Placement.ColumnIndex],
                rowTops[cell.Placement.RowIndex],
                width,
                height,
                background));
            if (tableStyle.CellBorder.HasVisibleStroke)
            {
                decorations.Add(new StrokeRectDecorationIntent(
                    columnLefts[cell.Placement.ColumnIndex],
                    rowTops[cell.Placement.RowIndex],
                    width,
                    height,
                    tableStyle.CellBorder.Color ?? new TextColor(0, 0, 0),
                    tableStyle.CellBorder.MaxWidth));
            }
        }

        if (!tableStyle.CellBorder.HasVisibleStroke && tableStyle.Border.HasVisibleStroke)
        {
            AddRectBorder(decorations, tableLeft, tableTop, tableWidth, tableHeight, tableStyle.Border.Color ?? new TextColor(0, 0, 0), tableStyle.Border.MaxWidth);
        }
    }

    private static double GetCellWidth(IReadOnlyList<double> columnLefts, double tableRight, int startColumn, int colSpan)
    {
        var start = columnLefts[startColumn];
        var endColumn = startColumn + colSpan;
        var end = endColumn < columnLefts.Count ? columnLefts[endColumn] : tableRight;
        return end - start;
    }

    private static void AddRectBorder(List<TextLayoutDecorationIntent> decorations, double x, double y, double width, double height, TextColor color, double thickness)
    {
        decorations.Add(new LineDecorationIntent(x, y, x + width, y, color, thickness));
        decorations.Add(new LineDecorationIntent(x, y + height, x + width, y + height, color, thickness));
        decorations.Add(new LineDecorationIntent(x, y, x, y + height, color, thickness));
        decorations.Add(new LineDecorationIntent(x + width, y, x + width, y + height, color, thickness));
    }

    private static BlockLayoutResult MeasureSegments(
        RichTextBoxLayoutRequest request,
        IReadOnlyList<TextSegment> segments,
        TextHorizontalAlignment alignment,
        double width,
        bool preserveTrailingWhitespaceInWidth = false,
        bool useFallbackFamilies = false)
    {
        var constraints = new LayoutConstraints(Math.Max(1d, width), LargeLayoutHeight, TextOverflowMode.Visible, ClipToHeight: false);
        var fallbackFamilies = useFallbackFamilies
            ? request.FallbackFamilyNames
                .Concat(request.FontLibrary.FamilyNames)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray()
            : request.FallbackFamilyNames;
        var childRequest = new TextBoxLayoutRequest(
            constraints.AvailableWidth,
            LargeLayoutHeight,
            request.FontLibrary,
            segments)
        {
            HorizontalAlignment = alignment,
            VerticalAlignment = TextVerticalAlignment.Top,
            OverflowMode = TextOverflowMode.Visible,
            MissingFontBehavior = useFallbackFamilies ? TextResolutionBehavior.UseFallbackFamilies : request.MissingFontBehavior,
            MissingGlyphBehavior = useFallbackFamilies ? TextResolutionBehavior.UseFallbackFamilies : request.MissingGlyphBehavior,
            FallbackFamilyNames = fallbackFamilies,
            PreserveTrailingWhitespaceInWidth = preserveTrailingWhitespaceInWidth || request.PreserveTrailingWhitespaceInWidth,
            MetricPreference = request.MetricPreference
        };

        var engine = new TextBoxLayoutEngine();
        return new BlockLayoutResult(engine.Layout(childRequest), constraints);
    }

    private static BlockLayoutResult MeasureBlocks(
        RichTextBoxLayoutRequest parentRequest,
        IReadOnlyList<RichTextBlock> blocks,
        double width,
        TextLayoutAnalysisContext? context = null,
        string pathPrefix = "Blocks")
    {
        var cacheKey = BuildIntrinsicBlockMeasurementKey(parentRequest, blocks, width, pathPrefix, IntrinsicMeasurementUnitKind.RichBlockSubtree, constrainHeight: false, availableHeight: LargeLayoutHeight, TextVerticalAlignment.Top);
        if (context?.IntrinsicMeasurements.TryGet(cacheKey, out var cachedEntry) == true
            && cachedEntry?.Value is BlockLayoutResult cached)
        {
            return cached;
        }

        var constraints = new LayoutConstraints(Math.Max(1d, width), LargeLayoutHeight, TextOverflowMode.Visible, ClipToHeight: false);
        var childRequest = new RichTextBoxLayoutRequest(
            constraints.AvailableWidth,
            LargeLayoutHeight,
            parentRequest.FontLibrary,
            blocks)
        {
            VerticalAlignment = TextVerticalAlignment.Top,
            OverflowMode = TextOverflowMode.Visible,
            MissingFontBehavior = parentRequest.MissingFontBehavior,
            MissingGlyphBehavior = parentRequest.MissingGlyphBehavior,
            FallbackFamilyNames = parentRequest.FallbackFamilyNames,
            PreserveTrailingWhitespaceInWidth = parentRequest.PreserveTrailingWhitespaceInWidth,
            ListIndent = parentRequest.ListIndent,
            ListMarkerGap = parentRequest.ListMarkerGap,
            MetricPreference = parentRequest.MetricPreference
        };

        var measured = new BlockLayoutResult(RemapLayoutSourcePaths(new RichTextBoxLayoutEngine().Layout(childRequest), pathPrefix), constraints);
        context?.IntrinsicMeasurements.Set(
            cacheKey,
            new IntrinsicMeasurementCacheEntry
            {
                NaturalSize = measured.NaturalSize,
                VisibleSize = measured.VisibleSize,
                Value = measured
            });
        return measured;
    }

    private static bool FitsWithinAvailableHeight(BlockLayoutResult layout, double availableHeight)
        => layout.VisibleHeight <= availableHeight + 0.0001d;

    private static TextBoxLayoutResult RemapLayoutSourcePaths(TextBoxLayoutResult layout, string pathPrefix)
    {
        if (pathPrefix == "Blocks")
        {
            return layout;
        }

        var lines = new TextLayoutLine[layout.Lines.Count];
        for (var i = 0; i < layout.Lines.Count; i++)
        {
            var line = layout.Lines[i];
            var runs = new TextLayoutRun[line.Runs.Count];
            for (var j = 0; j < line.Runs.Count; j++)
            {
                var run = line.Runs[j];
                runs[j] = run with { SourcePath = RemapSourcePath(run.SourcePath, pathPrefix) };
            }

            lines[i] = line with { Runs = runs };
        }

        return new TextBoxLayoutResult
        {
            Status = layout.Status,
            FitsWidth = layout.FitsWidth,
            FitsHeight = layout.FitsHeight,
            MeasuredWidth = layout.MeasuredWidth,
            MeasuredHeight = layout.MeasuredHeight,
            RenderedWidth = layout.RenderedWidth,
            RenderedHeight = layout.RenderedHeight,
            BoxStyle = layout.BoxStyle,
            Lines = lines,
            Issues = layout.Issues,
            Decorations = layout.Decorations
        };
    }

    private static string? RemapSourcePath(string? sourcePath, string pathPrefix)
    {
        if (string.IsNullOrEmpty(sourcePath))
        {
            return sourcePath;
        }

        return sourcePath == "Blocks"
            ? pathPrefix
            : sourcePath.StartsWith("Blocks[", StringComparison.Ordinal)
                ? pathPrefix + sourcePath["Blocks".Length..]
                : sourcePath;
    }

    private static BlockLayoutResult MeasureChildBlocks(
        RichTextBoxLayoutRequest parentRequest,
        LayoutChild child,
        double width,
        double height,
        TextVerticalAlignment verticalAlignment,
        bool constrainHeight,
        TextLayoutAnalysisContext? context = null,
        string pathPrefix = "Blocks")
    {
        IntrinsicMeasurementCacheKey cacheKey = default;
        if (!constrainHeight)
        {
            cacheKey = BuildIntrinsicBlockMeasurementKey(parentRequest, child.Blocks, width, pathPrefix, IntrinsicMeasurementUnitKind.LayoutChildSubtree, constrainHeight, height, verticalAlignment);
            if (context?.IntrinsicMeasurements.TryGet(cacheKey, out var cachedEntry) == true
                && cachedEntry?.Value is BlockLayoutResult cached)
            {
                return cached;
            }
        }

        var overflowMode = constrainHeight ? parentRequest.OverflowMode : TextOverflowMode.Visible;
        var constraints = new LayoutConstraints(Math.Max(1d, width), Math.Max(1d, height), overflowMode, ClipToHeight: constrainHeight);
        var childRequest = new RichTextBoxLayoutRequest(
            constraints.AvailableWidth,
            constraints.AvailableHeight,
            parentRequest.FontLibrary,
            child.Blocks)
        {
            VerticalAlignment = verticalAlignment,
            OverflowMode = overflowMode,
            MissingFontBehavior = parentRequest.MissingFontBehavior,
            MissingGlyphBehavior = parentRequest.MissingGlyphBehavior,
            FallbackFamilyNames = parentRequest.FallbackFamilyNames,
            PreserveTrailingWhitespaceInWidth = parentRequest.PreserveTrailingWhitespaceInWidth,
            ListIndent = parentRequest.ListIndent,
            ListMarkerGap = parentRequest.ListMarkerGap,
            BoxStyle = new TextBoxStyle(),
            MetricPreference = parentRequest.MetricPreference
        };

        var measured = new BlockLayoutResult(RemapLayoutSourcePaths(new RichTextBoxLayoutEngine().Layout(childRequest), pathPrefix), constraints);
        if (!constrainHeight)
        {
            context?.IntrinsicMeasurements.Set(
                cacheKey,
                new IntrinsicMeasurementCacheEntry
                {
                    NaturalSize = measured.NaturalSize,
                    VisibleSize = measured.VisibleSize,
                    Value = measured
                });
        }

        return measured;
    }

    private static double[] ResolveChildMainSizes(IReadOnlyList<LayoutChild> children, double availableSize, IReadOnlyList<double>? flexibleChromeSizes = null)
    {
        var sizes = new double[children.Count];
        var remaining = Math.Max(0d, availableSize);
        var totalWeight = 0d;

        for (var i = 0; i < children.Count; i++)
        {
            if (children[i].FixedSize.HasValue)
            {
                sizes[i] = Math.Max(0d, children[i].FixedSize.GetValueOrDefault());
                remaining -= sizes[i];
            }
            else
            {
                remaining -= flexibleChromeSizes is not null && i < flexibleChromeSizes.Count
                    ? Math.Max(0d, flexibleChromeSizes[i])
                    : 0d;
                totalWeight += Math.Max(0d, children[i].Weight);
            }
        }

        remaining = Math.Max(0d, remaining);
        if (totalWeight <= 0d)
        {
            var flexibleCount = children.Count(x => !x.FixedSize.HasValue);
            var equal = flexibleCount == 0 ? 0d : remaining / flexibleCount;
            for (var i = 0; i < children.Count; i++)
            {
                if (!children[i].FixedSize.HasValue)
                {
                    var chrome = flexibleChromeSizes is not null && i < flexibleChromeSizes.Count
                        ? Math.Max(0d, flexibleChromeSizes[i])
                        : 0d;
                    sizes[i] = chrome + equal;
                }
            }

            return sizes;
        }

        for (var i = 0; i < children.Count; i++)
        {
            if (!children[i].FixedSize.HasValue)
            {
                var chrome = flexibleChromeSizes is not null && i < flexibleChromeSizes.Count
                    ? Math.Max(0d, flexibleChromeSizes[i])
                    : 0d;
                sizes[i] = chrome + (remaining * (Math.Max(0d, children[i].Weight) / totalWeight));
            }
        }

        return sizes;
    }

    private static void AddBoxStyleDecorations(List<TextLayoutDecorationIntent> decorations, double x, double y, double width, double height, TextBoxStyle style)
        => AddBoxStyleDecorations(decorations, x, y, width, height, StyleResolver.Resolve(style));

    private static void AddBoxStyleDecorations(List<TextLayoutDecorationIntent> decorations, double x, double y, double width, double height, ComputedBoxStyle style)
    {
        if (width <= 0d || height <= 0d)
        {
            return;
        }

        if (style.BackgroundColor is TextColor background)
        {
            decorations.Add(new FillRectDecorationIntent(x, y, width, height, background, style.BorderRadius));
        }

        if (style.Border.HasVisibleStroke)
        {
            decorations.Add(new StrokeRectDecorationIntent(x, y, width, height, style.Border.Color ?? new TextColor(0, 0, 0), style.Border.MaxWidth, style.BorderRadius));
        }
    }

    private static void AppendLines(List<TextLayoutLine> target, IReadOnlyList<TextLayoutLine> source, double yOffset, double xOffset)
    {
        foreach (var line in source)
        {
            var runs = line.Runs
                .Select(run => run with
                {
                    X = run.X,
                    BaselineY = yOffset + run.BaselineY
                })
                .ToArray();

            target.Add(new TextLayoutLine(
                target.Count,
                xOffset + line.X,
                yOffset + line.BaselineY,
                line.Width,
                line.MeasuredWidth,
                line.Height,
                line.BaselineOffset,
                runs));
        }
    }

    private static void AppendDecorations(List<TextLayoutDecorationIntent> target, IReadOnlyList<TextLayoutDecoration> source, double yOffset, double xOffset)
    {
        foreach (var decoration in source)
        {
            target.Add(OffsetDecoration(TextLayoutDecorationIntentMapper.ToIntent(decoration), xOffset, yOffset));
        }
    }

    private static List<TextLayoutLine> ClipLines(double maxHeight, IReadOnlyList<TextLayoutLine> lines)
    {
        var clipped = new List<TextLayoutLine>();
        foreach (var line in lines)
        {
            if (clipped.Count > 0 && GetLineBottom(line) > maxHeight + 0.0001d)
            {
                break;
            }

            clipped.Add(line);
        }

        return clipped;
    }

    private static List<TextLayoutDecorationIntent> ClipDecorations(double maxHeight, IReadOnlyList<TextLayoutDecorationIntent> decorations)
        => decorations.Where(x => GetDecorationBottom(x) <= maxHeight + 0.0001d).ToList();

    private static List<TextLayoutLine> ClipPositionedLines(double viewportTop, double viewportBottom, IReadOnlyList<TextLayoutLine> lines)
        => lines
            .Where(x =>
            {
                var lineTop = GetLineTop(x);
                var lineBottom = GetLineBottom(x);
                return lineTop >= viewportTop - 0.0001d && lineBottom <= viewportBottom + 0.0001d;
            })
            .ToList();

    private static List<TextLayoutDecorationIntent> ClipPositionedDecorations(double viewportTop, double viewportBottom, IReadOnlyList<TextLayoutDecorationIntent> decorations)
        => decorations
            .Where(x =>
            {
                var top = x switch
                {
                    FillRectDecorationIntent fill => fill.Y,
                    StrokeRectDecorationIntent stroke => stroke.Y,
                    LineDecorationIntent line => Math.Min(line.Y1, line.Y2),
                    _ => double.MinValue
                };
                var bottom = GetDecorationBottom(x);
                return top >= viewportTop - 0.0001d && bottom <= viewportBottom + 0.0001d;
            })
            .ToList();

    private static void ApplyVerticalAlignment(
        RichTextBoxLayoutRequest request,
        List<TextLayoutLine> lines,
        List<TextLayoutDecorationIntent> decorations,
        double renderedHeight,
        double contentHeight)
    {
        var topOffset = request.VerticalAlignment switch
        {
            TextVerticalAlignment.Center => (contentHeight - renderedHeight) / 2d,
            TextVerticalAlignment.Bottom => contentHeight - renderedHeight,
            _ => 0d
        };

        if (topOffset == 0d)
        {
            return;
        }

        OffsetLines(lines, 0d, topOffset);
        OffsetDecorations(decorations, 0d, topOffset);
    }

    private static void OffsetLines(List<TextLayoutLine> lines, double xOffset, double yOffset)
    {
        for (var i = 0; i < lines.Count; i++)
        {
            lines[i] = lines[i] with
            {
                X = lines[i].X + xOffset,
                BaselineY = lines[i].BaselineY + yOffset,
                Runs = lines[i].Runs.Select(run => run with
                {
                    BaselineY = run.BaselineY + yOffset
                }).ToArray()
            };
        }
    }

    private static void OffsetDecorations(List<TextLayoutDecorationIntent> decorations, double xOffset, double yOffset)
    {
        for (var i = 0; i < decorations.Count; i++)
        {
            decorations[i] = OffsetDecoration(decorations[i], xOffset, yOffset);
        }
    }

    private static TextLayoutDecorationIntent OffsetDecoration(TextLayoutDecorationIntent decoration, double xOffset, double yOffset)
        => TextLayoutDecorationIntentMapper.Offset(decoration, xOffset, yOffset);

    private static void ShiftLines(List<TextLayoutLine> lines, double xOffset, double yOffset)
        => OffsetLines(lines, xOffset, yOffset);

    private static void ShiftDecorations(List<TextLayoutDecorationIntent> decorations, double xOffset, double yOffset)
        => OffsetDecorations(decorations, xOffset, yOffset);

    private static TextBoxLayoutResult CreateNoContentAreaResult(RichTextBoxLayoutRequest request)
        => new()
        {
            Status = request.OverflowMode == TextOverflowMode.Error ? TextLayoutStatus.Error : TextLayoutStatus.Overflow,
            FitsWidth = false,
            FitsHeight = false,
            MeasuredWidth = request.Width,
            MeasuredHeight = request.Height,
            RenderedWidth = 0d,
            RenderedHeight = 0d,
            BoxStyle = request.BoxStyle,
            Lines = Array.Empty<TextLayoutLine>(),
            Issues = new[]
            {
                new TextLayoutIssue(TextLayoutIssueKind.Overflow, "Text box border and padding leave no available content area.")
            },
            Decorations = Array.Empty<TextLayoutDecoration>()
        };

    private static double GetContentWidth(RichTextBoxLayoutRequest request)
        => request.Width - StyleResolver.Resolve(request.BoxStyle).Edges.HorizontalInset;

    private static double GetContentHeight(RichTextBoxLayoutRequest request)
        => request.Height - StyleResolver.Resolve(request.BoxStyle).Edges.VerticalInset;

    private static double GetLineBottom(TextLayoutLine line)
        => line.BaselineY - line.BaselineOffset + line.Height;

    private static double GetLineTop(TextLayoutLine line)
        => line.BaselineY - line.BaselineOffset;

    private static double GetLineWidth(IReadOnlyList<TextLayoutRun> runs)
        => runs.Count == 0 ? 0d : runs.Max(x => x.X + x.Width);

    private static double GetLineMeasuredWidth(IReadOnlyList<TextLayoutRun> runs)
        => runs.Count == 0 ? 0d : runs.Max(x => x.X + x.MeasuredWidth);

    private static double GetDecorationRight(TextLayoutDecorationIntent decoration)
        => TextLayoutDecorationIntentMapper.GetRight(decoration);

    private static double GetDecorationBottom(TextLayoutDecorationIntent decoration)
        => TextLayoutDecorationIntentMapper.GetBottom(decoration);

    private static double GetMaxRight(IReadOnlyList<TextLayoutLine> lines)
        => lines.Count == 0 ? 0d : lines.Max(x => x.Runs.Count == 0 ? x.MeasuredWidth : x.X + x.Runs.Max(r => r.X + r.MeasuredWidth));

    private static double GetMaxBottom(IReadOnlyList<TextLayoutLine> lines)
        => lines.Count == 0 ? 0d : lines.Max(GetLineBottom);

    private static double GetMaxRight(IReadOnlyList<TextLayoutDecorationIntent> decorations)
        => decorations.Count == 0 ? 0d : decorations.Max(GetDecorationRight);

    private static double GetMaxBottom(IReadOnlyList<TextLayoutDecorationIntent> decorations)
        => decorations.Count == 0 ? 0d : decorations.Max(GetDecorationBottom);

    private static double GetMaxRight(IReadOnlyList<TextLayoutLine> lines, IReadOnlyList<TextLayoutDecorationIntent> decorations)
        => Math.Max(GetMaxRight(lines), GetMaxRight(decorations));

    private static TextLayoutPlanNode BuildRichPlanRoot(RichTextBoxLayoutRequest request, TextBoxLayoutResult layout, TextLayoutAnalysisContext? context)
    {
        var childNodes = new TextLayoutPlanNode[request.Blocks.Count];
        for (var i = 0; i < request.Blocks.Count; i++)
        {
            childNodes[i] = BuildRichPlanNode(request, layout, request.Blocks[i], $"Blocks[{i}]", context);
        }

        return new TextLayoutPlanNode
        {
            Kind = TextLayoutNodeKind.Root,
            Source = CreateAggregateSourceReference("Blocks", childNodes),
            NaturalSize = layout.NaturalSize,
            VisibleSize = layout.VisibleSize,
            Children = childNodes,
            StartLineIndex = layout.Lines.Count == 0 ? null : 0,
            EndLineIndexExclusive = layout.Lines.Count
        };
    }

    private static TextLayoutPlanNode BuildRichPlanNode(RichTextBoxLayoutRequest request, TextBoxLayoutResult rootLayout, RichTextBlock block, string path, TextLayoutAnalysisContext? context)
    {
        var formatter = GetFormatter(block);
        var blockLayout = MeasureBlocks(request, new[] { block }, GetContentWidth(request), context, path);
        var children = formatter.BuildPlanChildren(request, rootLayout, block, path, context);
        var (startLineIndex, endLineIndexExclusive) = GetLineRange(children);

        return new TextLayoutPlanNode
        {
            Kind = formatter.NodeKind,
            Source = CreateBlockSourceReference(path, block, children),
            NaturalSize = blockLayout.NaturalSize,
            VisibleSize = blockLayout.VisibleSize,
            Children = children,
            StartLineIndex = startLineIndex,
            EndLineIndexExclusive = endLineIndexExclusive,
            ListDiagnostics = BuildListDiagnostics(request, block)
        };
    }

    private static TextLayoutListDiagnostics? BuildListDiagnostics(RichTextBoxLayoutRequest request, RichTextBlock block)
        => block switch
        {
            UnorderedListBlock unordered => TextLayoutDiagnosticsBuilder.BuildListDiagnostics(ListBlockFormatter.ResolveUnorderedListMetrics(request, unordered)),
            OrderedListBlock ordered => TextLayoutDiagnosticsBuilder.BuildListDiagnostics(ListBlockFormatter.ResolveOrderedListMetrics(request, ordered)),
            _ => null
        };

    private static IReadOnlyList<TextLayoutPlanNode> BuildListItemNodes(RichTextBoxLayoutRequest request, TextBoxLayoutResult rootLayout, IReadOnlyList<ListItemBlock> items, string parentPath, TextLayoutAnalysisContext? context)
    {
        var nodes = new TextLayoutPlanNode[items.Count];
        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];
            var childLayout = MeasureBlocks(request, item.Blocks, GetContentWidth(request), context, $"{parentPath}.Items[{i}].Blocks");
            var blockChildren = new TextLayoutPlanNode[item.Blocks.Count];
            for (var j = 0; j < item.Blocks.Count; j++)
            {
                blockChildren[j] = BuildRichPlanNode(request, rootLayout, item.Blocks[j], $"{parentPath}.Items[{i}].Blocks[{j}]", context);
            }
            var (startLineIndex, endLineIndexExclusive) = GetLineRange(blockChildren);

            nodes[i] = new TextLayoutPlanNode
            {
                Kind = TextLayoutNodeKind.ListItem,
                Source = CreateAggregateSourceReference($"{parentPath}.Items[{i}]", blockChildren),
                NaturalSize = childLayout.NaturalSize,
                VisibleSize = childLayout.VisibleSize,
                Children = blockChildren,
                StartLineIndex = startLineIndex,
                EndLineIndexExclusive = endLineIndexExclusive
            };
        }

        return nodes;
    }

    private static IReadOnlyList<TextLayoutPlanNode> BuildTableNodes(RichTextBoxLayoutRequest request, TextBoxLayoutResult rootLayout, TableBlock table, string parentPath, TextLayoutAnalysisContext? context)
    {
        var rowNodes = new TextLayoutPlanNode[table.Rows.Count];
        for (var rowIndex = 0; rowIndex < table.Rows.Count; rowIndex++)
        {
            var row = table.Rows[rowIndex];
            var cellNodes = new TextLayoutPlanNode[row.Cells.Count];
            for (var cellIndex = 0; cellIndex < row.Cells.Count; cellIndex++)
            {
                var cell = row.Cells[cellIndex];
                var cellBlocks = new ColumnBlock(cell.Blocks.Select(x => new LayoutChild(new[] { x })).ToArray(), null, new LayoutContainerStyle());
                var cellLayout = MeasureBlocks(request, new RichTextBlock[] { cellBlocks }, GetContentWidth(request), context, $"{parentPath}.Rows[{rowIndex}].Cells[{cellIndex}].Blocks");
                var blockChildren = new TextLayoutPlanNode[cell.Blocks.Count];
                for (var blockIndex = 0; blockIndex < cell.Blocks.Count; blockIndex++)
                {
                    blockChildren[blockIndex] = BuildRichPlanNode(request, rootLayout, cell.Blocks[blockIndex], $"{parentPath}.Rows[{rowIndex}].Cells[{cellIndex}].Blocks[{blockIndex}]", context);
                }
                var (cellStartLineIndex, cellEndLineIndexExclusive) = GetLineRange(blockChildren);

                cellNodes[cellIndex] = new TextLayoutPlanNode
                {
                    Kind = TextLayoutNodeKind.TableCell,
                    Source = CreateAggregateSourceReference($"{parentPath}.Rows[{rowIndex}].Cells[{cellIndex}]", blockChildren),
                    NaturalSize = cellLayout.NaturalSize,
                    VisibleSize = cellLayout.VisibleSize,
                    Children = blockChildren,
                    StartLineIndex = cellStartLineIndex,
                    EndLineIndexExclusive = cellEndLineIndexExclusive
                };
            }

            var rowLayout = MeasureBlocks(
                request,
                new RichTextBlock[] { CreateTableFragment(table, Array.Empty<TableRowBlock>(), new[] { row }, Array.Empty<TableRowBlock>()) },
                GetContentWidth(request),
                context,
                $"{parentPath}.Rows[{rowIndex}]");
            var (rowStartLineIndex, rowEndLineIndexExclusive) = GetLineRange(cellNodes);
            rowNodes[rowIndex] = new TextLayoutPlanNode
            {
                Kind = TextLayoutNodeKind.TableRow,
                Source = CreateAggregateSourceReference($"{parentPath}.Rows[{rowIndex}]", cellNodes),
                NaturalSize = rowLayout.NaturalSize,
                VisibleSize = rowLayout.VisibleSize,
                Children = cellNodes,
                StartLineIndex = rowStartLineIndex,
                EndLineIndexExclusive = rowEndLineIndexExclusive
            };
        }

        return rowNodes;
    }

    private static IReadOnlyList<TextLayoutPlanNode> BuildLayoutChildNodes(RichTextBoxLayoutRequest request, TextBoxLayoutResult rootLayout, IReadOnlyList<LayoutChild> children, string parentPath, TextLayoutAnalysisContext? context)
    {
        var nodes = new TextLayoutPlanNode[children.Count];
        for (var i = 0; i < children.Count; i++)
        {
            var child = children[i];
            var childLayout = MeasureChildBlocks(request, child, GetContentWidth(request), LargeLayoutHeight, child.VerticalAlignment, false, context, $"{parentPath}.Children[{i}].Blocks");
            var blockChildren = new TextLayoutPlanNode[child.Blocks.Count];
            for (var j = 0; j < child.Blocks.Count; j++)
            {
                blockChildren[j] = BuildRichPlanNode(request, rootLayout, child.Blocks[j], $"{parentPath}.Children[{i}].Blocks[{j}]", context);
            }
            var (startLineIndex, endLineIndexExclusive) = GetLineRange(blockChildren);

            nodes[i] = new TextLayoutPlanNode
            {
                Kind = TextLayoutNodeKind.LayoutChild,
                Source = CreateAggregateSourceReference($"{parentPath}.Children[{i}]", blockChildren),
                NaturalSize = childLayout.NaturalSize,
                VisibleSize = childLayout.VisibleSize,
                Children = blockChildren,
                StartLineIndex = startLineIndex,
                EndLineIndexExclusive = endLineIndexExclusive
            };
        }

        return nodes;
    }

    private static IReadOnlyList<TextLayoutPlanNode> BuildLineNodesForPath(TextBoxLayoutResult layout, string pathPrefix, TextLayoutAnalysisContext? context)
    {
        var lineNodes = new List<TextLayoutPlanNode>();
        foreach (var line in layout.Lines)
        {
            var runNodes = new List<TextLayoutPlanNode>();
            foreach (var run in line.Runs)
            {
                if (run.SourcePath is null || !run.SourcePath.StartsWith(pathPrefix, StringComparison.Ordinal))
                {
                    continue;
                }

                runNodes.Add(new TextLayoutPlanNode
                {
                    Kind = TextLayoutNodeKind.Run,
                    Source = CreateRunSourceReference(run),
                    NaturalSize = new TextLayoutSize(run.MeasuredWidth, run.LineHeight),
                    VisibleSize = new TextLayoutSize(run.Width, run.LineHeight),
                    Children = Array.Empty<TextLayoutPlanNode>()
                });
            }

            if (runNodes.Count == 0)
            {
                continue;
            }

            var naturalWidth = runNodes.Sum(x => x.NaturalSize.Width);
            var visibleWidth = runNodes.Sum(x => x.VisibleSize.Width);
            lineNodes.Add(new TextLayoutPlanNode
            {
                Kind = TextLayoutNodeKind.Line,
                Source = CreateAggregateSourceReference($"{pathPrefix}.Lines[{line.Index}]", runNodes),
                NaturalSize = new TextLayoutSize(naturalWidth, line.Height),
                VisibleSize = new TextLayoutSize(visibleWidth, line.Height),
                Children = runNodes,
                StartLineIndex = line.Index,
                EndLineIndexExclusive = line.Index + 1,
                LineDiagnostics = TextLayoutDiagnosticsBuilder.BuildLineDiagnostics(line, context)
            });
        }

        return lineNodes;
    }

    private static TextLayoutSourceReference CreateAggregateSourceReference(string path, IReadOnlyList<TextLayoutPlanNode> children)
    {
        var contentVersion = new HashCode();
        var styleVersion = new HashCode();
        foreach (var child in children)
        {
            contentVersion.Add(child.Source.ContentVersion);
            styleVersion.Add(child.Source.StyleVersion);
        }

        return new TextLayoutSourceReference(path, NodeId: path, ContentVersion: contentVersion.ToHashCode(), StyleVersion: styleVersion.ToHashCode());
    }

    private static TextLayoutSourceReference CreateBlockSourceReference(string path, RichTextBlock block, IReadOnlyList<TextLayoutPlanNode> children)
    {
        if (children.Count > 0)
        {
            return CreateAggregateSourceReference(path, children);
        }

        return new TextLayoutSourceReference(path, NodeId: path, ContentVersion: ComputeBlockContentVersion(block), StyleVersion: ComputeBlockStyleVersion(block));
    }

    private static TextLayoutSourceReference CreateRunSourceReference(TextLayoutRun run)
    {
        var contentVersion = new HashCode();
        contentVersion.Add(run.SourcePath, StringComparer.Ordinal);
        contentVersion.Add(run.SourceStart);
        contentVersion.Add(run.SourceLength);
        contentVersion.Add(run.Text, StringComparer.Ordinal);

        var styleVersion = new HashCode();
        styleVersion.Add(run.FaceId, StringComparer.Ordinal);
        styleVersion.Add(run.FamilyName, StringComparer.Ordinal);
        styleVersion.Add(run.Weight);
        styleVersion.Add(run.FontSize);
        styleVersion.Add(run.Italic);
        styleVersion.Add(run.Underline);
        styleVersion.Add(run.StrikeThrough);
        styleVersion.Add(run.CharacterSpacing);
        styleVersion.Add(run.WordSpacing);
        styleVersion.Add(run.ForegroundColor);
        styleVersion.Add(run.BackgroundColor);

        var nodeId = run.SourcePath ?? $"Segments[{run.SegmentIndex}]";
        return new TextLayoutSourceReference(
            run.SourcePath ?? $"Segments[{run.SegmentIndex}]",
            run.SegmentIndex,
            run.SourceStart,
            run.SourceLength,
            nodeId,
            contentVersion.ToHashCode(),
            styleVersion.ToHashCode());
    }

    private static int ComputeBlockContentVersion(RichTextBlock block)
    {
        var hash = new HashCode();
        switch (block)
        {
            case ParagraphBlock paragraph:
                AddInlineContentHash(ref hash, paragraph.Inlines);
                break;
            case HeadingBlock heading:
                hash.Add(heading.Level);
                AddInlineContentHash(ref hash, heading.Inlines);
                break;
            case UnorderedListBlock unordered:
                hash.Add(unordered.Marker, StringComparer.Ordinal);
                foreach (var item in unordered.Items)
                {
                    foreach (var child in item.Blocks)
                    {
                        hash.Add(ComputeBlockContentVersion(child));
                    }
                }
                break;
            case OrderedListBlock ordered:
                hash.Add(ordered.StartIndex);
                foreach (var item in ordered.Items)
                {
                    foreach (var child in item.Blocks)
                    {
                        hash.Add(ComputeBlockContentVersion(child));
                    }
                }
                break;
            case TableBlock table:
                hash.Add(table.Layout);
                foreach (var column in table.Columns)
                {
                    hash.Add(column.Width);
                    hash.Add(column.Key);
                    hash.Add(column.Style);
                }
                foreach (var section in table.Sections)
                {
                    hash.Add(section.Kind);
                }
                foreach (var row in table.Rows)
                {
                    foreach (var cell in row.Cells)
                    {
                        foreach (var child in cell.Blocks)
                        {
                            hash.Add(ComputeBlockContentVersion(child));
                        }
                    }
                }
                break;
            case RowBlock row:
                foreach (var child in row.Children)
                {
                    foreach (var nested in child.Blocks)
                    {
                        hash.Add(ComputeBlockContentVersion(nested));
                    }
                }
                break;
            case ColumnBlock column:
                foreach (var child in column.Children)
                {
                    foreach (var nested in child.Blocks)
                    {
                        hash.Add(ComputeBlockContentVersion(nested));
                    }
                }
                break;
        }

        return hash.ToHashCode();
    }

    private static int ComputeBlockStyleVersion(RichTextBlock block)
    {
        var hash = new HashCode();
        switch (block)
        {
            case ParagraphBlock paragraph:
                hash.Add(paragraph.Style);
                AddInlineStyleHash(ref hash, paragraph.Inlines);
                break;
            case HeadingBlock heading:
                hash.Add(heading.Level);
                hash.Add(heading.Style);
                AddInlineStyleHash(ref hash, heading.Inlines);
                break;
            case UnorderedListBlock unordered:
                hash.Add(unordered.MarginBlockEnd);
                hash.Add(unordered.MarkerStyle);
                foreach (var item in unordered.Items)
                {
                    foreach (var child in item.Blocks)
                    {
                        hash.Add(ComputeBlockStyleVersion(child));
                    }
                }
                break;
            case OrderedListBlock ordered:
                hash.Add(ordered.StartIndex);
                hash.Add(ordered.MarginBlockEnd);
                foreach (var item in ordered.Items)
                {
                    foreach (var child in item.Blocks)
                    {
                        hash.Add(ComputeBlockStyleVersion(child));
                    }
                }
                break;
            case TableBlock table:
                hash.Add(table.Style);
                hash.Add(table.Layout);
                foreach (var column in table.Columns)
                {
                    hash.Add(column.Width);
                    hash.Add(column.Key);
                    hash.Add(column.Style);
                }
                foreach (var row in table.Rows)
                {
                    hash.Add(row.Style);
                    foreach (var cell in row.Cells)
                    {
                        hash.Add(cell.Style);
                        foreach (var child in cell.Blocks)
                        {
                            hash.Add(ComputeBlockStyleVersion(child));
                        }
                    }
                }
                break;
            case RowBlock row:
                hash.Add(row.Style);
                foreach (var child in row.Children)
                {
                    hash.Add(child.FixedSize);
                    hash.Add(child.Weight);
                    hash.Add(child.VerticalAlignment);
                    foreach (var nested in child.Blocks)
                    {
                        hash.Add(ComputeBlockStyleVersion(nested));
                    }
                }
                break;
            case ColumnBlock column:
                hash.Add(column.Style);
                foreach (var child in column.Children)
                {
                    hash.Add(child.FixedSize);
                    hash.Add(child.Weight);
                    hash.Add(child.VerticalAlignment);
                    foreach (var nested in child.Blocks)
                    {
                        hash.Add(ComputeBlockStyleVersion(nested));
                    }
                }
                break;
        }

        return hash.ToHashCode();
    }

    private static void AddInlineContentHash(ref HashCode hash, IReadOnlyList<InlineNode> inlines)
    {
        foreach (var inline in inlines)
        {
            switch (inline)
            {
                case TextRunNode text:
                    hash.Add(text.Text, StringComparer.Ordinal);
                    break;
                case LineBreakNode:
                    hash.Add("\n", StringComparer.Ordinal);
                    break;
            }
        }
    }

    private static void AddInlineStyleHash(ref HashCode hash, IReadOnlyList<InlineNode> inlines)
    {
        foreach (var inline in inlines)
        {
            if (inline is TextRunNode text)
            {
                hash.Add(text.Style);
            }
        }
    }

    private static IntrinsicMeasurementCacheKey BuildIntrinsicBlockMeasurementKey(
        RichTextBoxLayoutRequest request,
        IReadOnlyList<RichTextBlock> blocks,
        double width,
        string pathPrefix,
        IntrinsicMeasurementUnitKind unitKind,
        bool constrainHeight,
        double availableHeight,
        TextVerticalAlignment verticalAlignment)
    {
        var contentVersion = new HashCode();
        var styleVersion = new HashCode();
        foreach (var block in blocks)
        {
            contentVersion.Add(ComputeBlockContentVersion(block));
            styleVersion.Add(ComputeBlockStyleVersion(block));
        }

        styleVersion.Add(RuntimeHelpers.GetHashCode(request.FontLibrary));
        styleVersion.Add(request.ListIndent);
        styleVersion.Add(request.ListMarkerGap);
        styleVersion.Add(request.MetricPreference);
        styleVersion.Add(request.PreserveTrailingWhitespaceInWidth);
        styleVersion.Add(request.MissingFontBehavior);
        styleVersion.Add(request.MissingGlyphBehavior);
        foreach (var fallback in request.FallbackFamilyNames)
        {
            styleVersion.Add(fallback, StringComparer.Ordinal);
        }

        var modeFlags = 0;
        modeFlags |= constrainHeight ? 1 << 0 : 0;
        modeFlags |= ((int)verticalAlignment & 0x3) << 1;
        modeFlags |= ((int)request.MetricPreference & 0x7) << 3;
        modeFlags |= request.PreserveTrailingWhitespaceInWidth ? 1 << 6 : 0;

        return new IntrinsicMeasurementCacheKey(
            unitKind,
            pathPrefix,
            contentVersion.ToHashCode(),
            HashCode.Combine(styleVersion.ToHashCode(), availableHeight),
            width,
            modeFlags);
    }

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

    private static IReadOnlyList<TextLayoutSourceReference> CollectRichSourceReferences(TextLayoutPlan plan)
        => plan.Root.Children.Select(x => x.Source).ToArray();

    private static IReadOnlyList<TextLayoutSourceReference> CollectBoundaryReferences(IReadOnlyList<RichLayoutSplitMetadata> metadata)
        => metadata.Select(x => new TextLayoutSourceReference(x.Path)).ToArray();

    private static IReadOnlyList<TextLayoutContinuationReference> BuildRichContinuations(
        TextLayoutPlan fittedPlan,
        TextLayoutPlan? remainderPlan,
        IReadOnlyList<RichLayoutSplitMetadata> metadata)
    {
        if (metadata.Count == 0)
        {
            return Array.Empty<TextLayoutContinuationReference>();
        }

        var continuations = new List<TextLayoutContinuationReference>(metadata.Count);
        foreach (var split in metadata)
        {
            var boundaryPath = split is TableSplitMetadata tableSplit && !string.IsNullOrWhiteSpace(tableSplit.BoundaryParentPath)
                ? tableSplit.BoundaryParentPath!
                : split.Path;
            var continuationPath = split is TableSplitMetadata continuationSplit && !string.IsNullOrWhiteSpace(continuationSplit.ContinuationParentPath)
                ? continuationSplit.ContinuationParentPath!
                : split.Path;
            var fittedBoundary = FindBoundaryReference(fittedPlan.Root, boundaryPath, preferLast: true)
                ?? new TextLayoutSourceReference(split.Path, NodeId: split.Path);
            var continuationStart = remainderPlan is null
                ? null
                : FindBoundaryReference(remainderPlan.Root, continuationPath, preferLast: false);
            var continuationKind = split is TableSplitMetadata typedTableSplit
                ? typedTableSplit.ContinuationKind
                : MapContinuationKind(split.Kind);
            var parentPath = split is TableSplitMetadata typedParentSplit && !string.IsNullOrWhiteSpace(typedParentSplit.ContinuationParentPath)
                ? typedParentSplit.ContinuationParentPath
                : split.Path;
            if (split is TableSplitMetadata && continuationStart is not null && string.IsNullOrWhiteSpace(parentPath))
            {
                var boundaryRowPath = TryGetAncestorPath(fittedBoundary.Path, ".Rows[");
                var continuationRowPath = TryGetAncestorPath(continuationStart.Path, ".Rows[");
                var continuationCellPath = TryGetAncestorPath(continuationStart.Path, ".Cells[");
                if (continuationCellPath is not null
                    && boundaryRowPath is not null
                    && string.Equals(boundaryRowPath, continuationRowPath, StringComparison.Ordinal))
                {
                    continuationKind = TextLayoutContinuationKind.TableCell;
                    parentPath = continuationCellPath;
                }
                else if (continuationRowPath is not null)
                {
                    continuationKind = TextLayoutContinuationKind.TableRow;
                    parentPath = continuationRowPath;
                }
            }

            continuations.Add(new TextLayoutContinuationReference(
                continuationKind,
                fittedBoundary,
                continuationStart,
                parentPath,
                split.BreakReason,
                split.IsForced));
        }

        return continuations;
    }

    private static TextLayoutFragmentMetadata BuildFragmentMetadata(
        TextFragmentBreak fragmentBreak,
        IReadOnlyList<TextLayoutContinuationReference> continuations)
        => new(fragmentBreak, continuations.FirstOrDefault());

    private static TextFragmentBreak BuildRichFragmentBreak(TextBreakKind breakKind, IReadOnlyList<RichLayoutSplitMetadata> metadata)
    {
        var first = metadata.FirstOrDefault();
        return first is null
            ? TextFragmentBreak.None
            : new TextFragmentBreak(first.BreakReason, first.Kind == TextBreakKind.None ? breakKind : first.Kind, first.IsForced);
    }

    private static TextBreakKind GetBreakKindForBlock(RichTextBlock block)
        => block switch
        {
            ParagraphBlock or HeadingBlock => TextBreakKind.Paragraph,
            UnorderedListBlock or OrderedListBlock => TextBreakKind.ListItem,
            TableBlock => TextBreakKind.TableRow,
            RowBlock or ColumnBlock => TextBreakKind.ContainerChild,
            _ => TextBreakKind.Line
        };

    private static (IReadOnlyList<TableRowBlock> HeaderRows, IReadOnlyList<TableRowBlock> BodyRows, IReadOnlyList<TableRowBlock> FooterRows, bool RepeatHeaders, bool RepeatFooters) ResolveTableFragmentGroups(TableBlock table)
    {
        var headerRows = table.Sections
            .Where(x => x.Kind == TableSectionKind.Header)
            .SelectMany(x => x.Rows)
            .ToArray();
        var bodyRows = table.Sections
            .Where(x => x.Kind == TableSectionKind.Body)
            .SelectMany(x => x.Rows)
            .ToArray();
        var footerRows = table.Sections
            .Where(x => x.Kind == TableSectionKind.Footer)
            .SelectMany(x => x.Rows)
            .ToArray();

        if (bodyRows.Length == 0 && headerRows.Length == 0 && footerRows.Length == 0)
        {
            bodyRows = table.Rows.ToArray();
        }

        return (headerRows, bodyRows, footerRows, table.Pagination.RepeatHeaderRows, table.Pagination.RepeatFooterRows);
    }

    private static IReadOnlyList<TableRowBlock> ComposeTableFragmentRows(
        IReadOnlyList<TableRowBlock> headerRows,
        IReadOnlyList<TableRowBlock> bodyRows,
        IReadOnlyList<TableRowBlock> footerRows)
        => headerRows.Concat(bodyRows).Concat(footerRows).ToArray();

    private static TableBlock CreateTableFragment(
        TableBlock source,
        IReadOnlyList<TableRowBlock> headerRows,
        IReadOnlyList<TableRowBlock> bodyRows,
        IReadOnlyList<TableRowBlock> footerRows)
    {
        var sections = new List<TableSectionBlock>(3);
        if (headerRows.Count > 0)
        {
            sections.Add(new TableSectionBlock(TableSectionKind.Header, headerRows));
        }

        if (bodyRows.Count > 0)
        {
            sections.Add(new TableSectionBlock(TableSectionKind.Body, bodyRows));
        }

        if (footerRows.Count > 0)
        {
            sections.Add(new TableSectionBlock(TableSectionKind.Footer, footerRows));
        }

        return new TableBlock(source.Columns, sections, source.Style, source.Layout);
    }

    private static BlockSplitOutcome? TrySplitTableRow(
        RichTextBoxLayoutRequest request,
        TableBlock table,
        double contentWidth,
        double availableHeight,
        string path,
        IReadOnlyList<TableRowBlock> headerRows,
        IReadOnlyList<TableRowBlock> fittedRows,
        IReadOnlyList<TableRowBlock> remainingBodyRows,
        IReadOnlyList<TableRowBlock> footerRows,
        bool repeatHeaders,
        bool repeatFooters)
    {
        if (remainingBodyRows.Count == 0)
        {
            return null;
        }

        var targetRow = remainingBodyRows[0];
        var targetRowIndex = FindTableRowIndex(table.Rows, targetRow);
        if (targetRowIndex < 0 || !CanSplitTableRow(table, targetRowIndex))
        {
            return null;
        }

        var headerRowsInFirstFragment = headerRows;
        var footerRowsInFirstFragment = footerRows;
        var scaffoldTable = CreateTableFragment(table, headerRowsInFirstFragment, fittedRows, footerRowsInFirstFragment);
        var scaffoldLayout = MeasureBlocks(request, new RichTextBlock[] { scaffoldTable }, contentWidth);
        var availableRowHeight = availableHeight - scaffoldLayout.VisibleHeight;
        if (availableRowHeight <= 0.0001d)
        {
            return null;
        }

        var placements = BuildCellPlacements(table);
        var targetPlacements = placements
            .Where(x => x.RowIndex == targetRowIndex)
            .OrderBy(x => x.CellIndex)
            .ToArray();
        if (targetPlacements.Length == 0)
        {
            return null;
        }

        var tableStyle = StyleResolver.Resolve(table.Style);
        var columnCount = placements.Max(x => x.ColumnIndex + x.ColSpan);
        var columnWidths = ResolveColumnWidths(request, table, placements, columnCount, Math.Max(1d, contentWidth));
        var fittedCells = new TableCellBlock[targetRow.Cells.Count];
        var remainderCells = new TableCellBlock[targetRow.Cells.Count];
        var nestedMetadata = new List<RichLayoutSplitMetadata>();
        var anyFittedContent = false;
        var firstContinuingCellIndex = -1;

        foreach (var placement in targetPlacements)
        {
            var cellStyle = StyleResolver.Resolve(placement.Cell.Style, tableStyle);
            var outerWidth = GetSpanWidth(columnWidths, placement.ColumnIndex, placement.ColSpan);
            var innerWidth = Math.Max(1d, outerWidth - (cellStyle.Padding.Horizontal + tableStyle.CellBorder.Widths.Horizontal));
            var innerHeight = Math.Max(0d, availableRowHeight - (cellStyle.Padding.Vertical + tableStyle.CellBorder.Widths.Vertical));
            var cellBlocks = ApplyCellAlignment(placement.Cell, cellStyle.TextAlign);
            var cellPath = $"{path}.Rows[{headerRowsInFirstFragment.Count + fittedRows.Count}].Cells[{placement.CellIndex}].Blocks";
            var fullCellLayout = MeasureBlocks(request, cellBlocks, innerWidth);

            IReadOnlyList<RichTextBlock> fittedCellBlocks;
            IReadOnlyList<RichTextBlock> remainderCellBlocks;
            if (FitsWithinAvailableHeight(fullCellLayout, innerHeight))
            {
                fittedCellBlocks = cellBlocks;
                remainderCellBlocks = Array.Empty<RichTextBlock>();
            }
            else
            {
                var split = FitBlocks(request, cellBlocks, innerWidth, innerHeight, cellPath);
                if (split.FittedBlocks.Count == 0)
                {
                    fittedCellBlocks = Array.Empty<RichTextBlock>();
                    remainderCellBlocks = split.RemainderBlocks.Count == 0 ? cellBlocks : split.RemainderBlocks;
                }
                else
                {
                    fittedCellBlocks = split.FittedBlocks;
                    remainderCellBlocks = split.RemainderBlocks;
                }

                nestedMetadata.AddRange(split.Metadata);
            }

            anyFittedContent |= fittedCellBlocks.Count > 0;
            if (remainderCellBlocks.Count > 0 && firstContinuingCellIndex < 0)
            {
                firstContinuingCellIndex = placement.CellIndex;
            }

            fittedCells[placement.CellIndex] = CloneCellWithBlocks(placement.Cell, fittedCellBlocks);
            remainderCells[placement.CellIndex] = CloneCellWithBlocks(placement.Cell, remainderCellBlocks);
        }

        if (!anyFittedContent || firstContinuingCellIndex < 0)
        {
            return null;
        }

        for (var i = 0; i < fittedCells.Length; i++)
        {
            fittedCells[i] ??= CloneCellWithBlocks(targetRow.Cells[i], Array.Empty<RichTextBlock>());
            remainderCells[i] ??= CloneCellWithBlocks(targetRow.Cells[i], Array.Empty<RichTextBlock>());
        }

        var fittedRow = targetRow with { Cells = fittedCells };
        var remainderRow = targetRow with { Cells = remainderCells };
        var fittedTable = CreateTableFragment(
            table,
            headerRowsInFirstFragment,
            fittedRows.Concat(new[] { fittedRow }).ToArray(),
            footerRowsInFirstFragment);
        var remainderTable = CreateTableFragment(
            table,
            repeatHeaders ? headerRows : Array.Empty<TableRowBlock>(),
            new[] { remainderRow }.Concat(remainingBodyRows.Skip(1)).ToArray(),
            repeatFooters ? footerRows : Array.Empty<TableRowBlock>());
        var fittedLayout = MeasureBlocks(request, new RichTextBlock[] { fittedTable }, contentWidth);
        if (!FitsWithinAvailableHeight(fittedLayout, availableHeight))
        {
            return null;
        }

        var fittedCellParent = $"{path}.Rows[{headerRowsInFirstFragment.Count + fittedRows.Count}].Cells[{firstContinuingCellIndex}]";
        var remainderCellParent = $"{path}.Rows[{headerRows.Count}].Cells[{firstContinuingCellIndex}]";
        var metadata = new List<RichLayoutSplitMetadata>(nestedMetadata.Count + 1)
        {
            new TableSplitMetadata(
                path,
                fittedRows.Count,
                remainingBodyRows.Count,
                headerRows.Count > 0,
                footerRows.Count > 0,
                TextLayoutContinuationKind.TableCell,
                fittedCellParent,
                remainderCellParent)
        };
        metadata.AddRange(nestedMetadata);

        return new BlockSplitOutcome(
            fittedTable,
            remainderTable,
            TextBreakKind.TableRow,
            metadata,
            true,
            true);
    }

    private static bool CanSplitTableRow(TableBlock table, int rowIndex)
    {
        if (rowIndex < 0 || rowIndex >= table.Rows.Count)
        {
            return false;
        }

        var rowStyle = StyleResolver.Resolve(table.Rows[rowIndex].Style, table.Pagination);
        if (rowStyle.KeepTogether || rowStyle.SplitMode == TableRowSplitMode.Avoid || table.Pagination.CellSplitMode == TableCellSplitMode.Avoid)
        {
            return false;
        }

        var placements = BuildCellPlacements(table);
        if (placements.Any(x => x.RowIndex == rowIndex && x.RowSpan > 1))
        {
            return false;
        }

        return !placements.Any(x => x.RowIndex < rowIndex && rowIndex < x.RowIndex + x.RowSpan);
    }

    private static int FindTableRowIndex(IReadOnlyList<TableRowBlock> rows, TableRowBlock row)
    {
        for (var i = 0; i < rows.Count; i++)
        {
            if (ReferenceEquals(rows[i], row) || EqualityComparer<TableRowBlock>.Default.Equals(rows[i], row))
            {
                return i;
            }
        }

        return -1;
    }

    private static TableCellBlock CloneCellWithBlocks(TableCellBlock cell, IReadOnlyList<RichTextBlock> blocks)
        => cell switch
        {
            TableHeaderCellBlock header => header with { Blocks = blocks },
            TableDataCellBlock data => data with { Blocks = blocks },
            _ => throw new InvalidOperationException($"Unsupported table cell type '{cell.GetType().Name}'.")
        };

    private static TextLayoutSourceReference? FindBoundaryReference(TextLayoutPlanNode node, string pathPrefix, bool preferLast)
    {
        var matches = new List<TextLayoutSourceReference>();
        CollectBoundaryReferences(node, pathPrefix, matches);
        if (matches.Count == 0)
        {
            return null;
        }

        return preferLast ? matches[^1] : matches[0];
    }

    private static void CollectBoundaryReferences(TextLayoutPlanNode node, string pathPrefix, List<TextLayoutSourceReference> matches)
    {
        if (node.Source.Path.StartsWith(pathPrefix, StringComparison.Ordinal))
        {
            matches.Add(node.Source);
        }

        foreach (var child in node.Children)
        {
            CollectBoundaryReferences(child, pathPrefix, matches);
        }
    }

    private static TextLayoutContinuationKind MapContinuationKind(TextBreakKind breakKind)
        => breakKind switch
        {
            TextBreakKind.Paragraph => TextLayoutContinuationKind.Paragraph,
            TextBreakKind.ListItem => TextLayoutContinuationKind.ListItem,
            TextBreakKind.TableRow => TextLayoutContinuationKind.TableRow,
            TextBreakKind.ContainerChild => TextLayoutContinuationKind.ContainerChild,
            _ => TextLayoutContinuationKind.Line
        };

    private static string? TryGetAncestorPath(string path, string marker)
    {
        var index = path.IndexOf(marker, StringComparison.Ordinal);
        if (index < 0)
        {
            return null;
        }

        var closeIndex = path.IndexOf(']', index);
        return closeIndex < 0 ? null : path[..(closeIndex + 1)];
    }

    private sealed class LayoutState
    {
        public List<TextLayoutLine> Lines { get; } = new();
        public List<TextLayoutDecorationIntent> Decorations { get; } = new();
        public List<TextLayoutIssue> Issues { get; } = new();
        public double ConsumedHeight { get; set; }
        public int NextSegmentIndex { get; set; }
        public bool HadOverflow { get; set; }
    }

    private static class TableBlockFormatter
    {
        public static BlockSplitOutcome SplitTableBlock(
            RichTextBoxLayoutRequest request,
            TableBlock table,
            double contentWidth,
            double availableHeight,
            string path)
        {
            if (table.Rows.Count == 0)
            {
                return new BlockSplitOutcome(table, null, TextBreakKind.None, Array.Empty<RichLayoutSplitMetadata>(), false, false);
            }

            var (headerRows, bodyRows, footerRows, repeatHeaders, repeatFooters) = ResolveTableFragmentGroups(table);
            var fittedRows = new List<TableRowBlock>();
            var remainingBodyRows = bodyRows.ToList();

            for (var i = 0; i < bodyRows.Count; i++)
            {
                fittedRows.Add(bodyRows[i]);
                remainingBodyRows.RemoveAt(0);
                var candidateTable = CreateTableFragment(
                    table,
                    headerRows,
                    fittedRows,
                    remainingBodyRows.Count > 0 || footerRows.Count > 0 ? footerRows : Array.Empty<TableRowBlock>());
                var candidateLayout = MeasureBlocks(request, new RichTextBlock[] { candidateTable }, contentWidth);
                if (!FitsWithinAvailableHeight(candidateLayout, availableHeight))
                {
                    fittedRows.RemoveAt(fittedRows.Count - 1);
                    remainingBodyRows.Insert(0, bodyRows[i]);
                    var splitCurrentRow = TrySplitTableRow(
                        request,
                        table,
                        contentWidth,
                        availableHeight,
                        path,
                        headerRows,
                        fittedRows,
                        remainingBodyRows,
                        footerRows,
                        repeatHeaders,
                        repeatFooters);
                    if (splitCurrentRow is not null)
                    {
                        return splitCurrentRow;
                    }

                    break;
                }
            }

            if (fittedRows.Count == 0)
            {
                return new BlockSplitOutcome(
                    null,
                    table,
                    TextBreakKind.TableRow,
                    new[]
                    {
                        new TableSplitMetadata(
                            path,
                            0,
                            bodyRows.Count,
                            headerRows.Count > 0,
                            footerRows.Count > 0,
                            TextLayoutContinuationKind.TableRow,
                            path,
                            $"{path}.Rows[{headerRows.Count}]")
                    },
                    false,
                    false);
            }

            if (remainingBodyRows.Count == 0)
            {
                return new BlockSplitOutcome(
                    CreateTableFragment(table, headerRows, fittedRows, footerRows),
                    null,
                    TextBreakKind.None,
                    Array.Empty<RichLayoutSplitMetadata>(),
                    false,
                    false);
            }

            var fittedTable = CreateTableFragment(table, headerRows, fittedRows, footerRows);
            var remainderTable = CreateTableFragment(
                table,
                repeatHeaders ? headerRows : Array.Empty<TableRowBlock>(),
                remainingBodyRows,
                repeatFooters ? footerRows : Array.Empty<TableRowBlock>());
            return new BlockSplitOutcome(
                fittedTable,
                remainderTable,
                TextBreakKind.TableRow,
                new RichLayoutSplitMetadata[]
                {
                    new TableSplitMetadata(
                        path,
                        fittedRows.Count,
                        remainingBodyRows.Count,
                        headerRows.Count > 0,
                        footerRows.Count > 0,
                        TextLayoutContinuationKind.TableRow,
                        $"{path}.Rows[{headerRows.Count + fittedRows.Count - 1}]",
                        $"{path}.Rows[{headerRows.Count}]")
                },
                false,
                false);
        }

        public static void AppendTable(
            RichTextBoxLayoutRequest request,
            TableBlock table,
            double contentWidth,
            double xOffset,
            string path,
            ref LayoutState state)
        {
            if (table.Rows.Count == 0)
            {
                state.ConsumedHeight += (table.Style ?? new TableStyle()).MarginBlockEnd;
                return;
            }

            var tableStyle = StyleResolver.Resolve(table.Style);
            var placements = BuildCellPlacements(table);
            if (placements.Count == 0)
            {
                state.ConsumedHeight += tableStyle.MarginBlockEnd;
                return;
            }

            var columnCount = placements.Max(x => x.ColumnIndex + x.ColSpan);
            var rowCount = table.Rows.Count;
            var columnWidths = ResolveColumnWidths(request, table, placements, columnCount, Math.Max(1d, contentWidth - xOffset));
            var rowHeights = new double[rowCount];
            var measuredCells = new List<MeasuredCell>(placements.Count);

            foreach (var placement in placements)
            {
                var outerWidth = GetSpanWidth(columnWidths, placement.ColumnIndex, placement.ColSpan);
                var cellStyle = StyleResolver.Resolve(placement.Cell.Style, tableStyle);
                var inset = tableStyle.CellBorder.MaxWidth + cellStyle.Padding.Top;
                var innerWidth = Math.Max(1d, outerWidth - (cellStyle.Padding.Horizontal + tableStyle.CellBorder.Widths.Horizontal));
                var cellBlocks = ApplyCellAlignment(placement.Cell, cellStyle.TextAlign);
                var cellLayout = MeasureBlocks(request, cellBlocks, innerWidth, context: null, pathPrefix: $"{path}.Rows[{placement.RowIndex}].Cells[{placement.CellIndex}].Blocks");
                measuredCells.Add(new MeasuredCell(placement, cellLayout.Layout, inset, outerWidth, cellLayout.NaturalHeight + cellStyle.Padding.Vertical + tableStyle.CellBorder.Widths.Vertical));
                state.Issues.AddRange(cellLayout.Layout.Issues);
                state.HadOverflow |= cellLayout.Layout.Status == TextLayoutStatus.Overflow;

                if (placement.RowSpan == 1)
                {
                    rowHeights[placement.RowIndex] = Math.Max(rowHeights[placement.RowIndex], cellLayout.NaturalHeight + (inset * 2d));
                }
            }

            foreach (var cell in measuredCells.Where(x => x.Placement.RowSpan > 1))
            {
                var span = Math.Min(cell.Placement.RowSpan, rowCount - cell.Placement.RowIndex);
                var coveredHeight = Sum(rowHeights, cell.Placement.RowIndex, span);
                if (coveredHeight >= cell.OuterHeight - 0.0001d)
                {
                    continue;
                }

                var extra = cell.OuterHeight - coveredHeight;
                var weights = new double[span];
                var totalWeight = 0d;
                for (var i = 0; i < span; i++)
                {
                    weights[i] = Math.Max(0d, rowHeights[cell.Placement.RowIndex + i]);
                    totalWeight += weights[i];
                }

                if (totalWeight <= 0d)
                {
                    var equalExtra = extra / span;
                    for (var i = 0; i < span; i++)
                    {
                        rowHeights[cell.Placement.RowIndex + i] += equalExtra;
                    }

                    continue;
                }

                for (var i = 0; i < span; i++)
                {
                    rowHeights[cell.Placement.RowIndex + i] += extra * (weights[i] / totalWeight);
                }
            }

            for (var rowIndex = 0; rowIndex < rowHeights.Length; rowIndex++)
            {
                if (rowHeights[rowIndex] <= 0)
                {
                    rowHeights[rowIndex] = ResolveFallbackRowHeight(measuredCells, rowIndex);
                }

                var resolvedRowStyle = StyleResolver.Resolve(table.Rows[rowIndex].Style, table.Pagination);
                if (resolvedRowStyle.MinHeight.HasValue)
                {
                    rowHeights[rowIndex] = Math.Max(rowHeights[rowIndex], resolvedRowStyle.MinHeight.Value);
                }
            }

            var rowTops = new double[rowCount];
            var cursor = state.ConsumedHeight;
            for (var rowIndex = 0; rowIndex < rowCount; rowIndex++)
            {
                rowTops[rowIndex] = cursor;
                cursor += rowHeights[rowIndex];
            }

            var columnLefts = new double[columnCount];
            var xCursor = xOffset;
            for (var columnIndex = 0; columnIndex < columnCount; columnIndex++)
            {
                columnLefts[columnIndex] = xCursor;
                xCursor += columnWidths[columnIndex];
            }

            var tableWidth = columnWidths.Sum();
            var tableHeight = rowHeights.Sum();
            AddTableDecorations(state.Decorations, tableStyle, measuredCells, columnLefts, rowTops, rowHeights, tableWidth, tableHeight, xOffset, state.ConsumedHeight);

            foreach (var cell in measuredCells)
            {
                var cellX = columnLefts[cell.Placement.ColumnIndex];
                var cellY = rowTops[cell.Placement.RowIndex];
                var innerX = cellX + cell.Inset;
                var innerY = cellY + cell.Inset;
                AppendLines(state.Lines, cell.Layout.Lines, innerY, innerX);
                AppendDecorations(state.Decorations, cell.Layout.Decorations, innerY, innerX);
            }

            state.ConsumedHeight += tableHeight + tableStyle.MarginBlockEnd;
        }
    }

    private static class ContainerBlockFormatter
    {
        public static BlockSplitOutcome SplitColumnBlock(
            RichTextBoxLayoutRequest request,
            ColumnBlock column,
            double contentWidth,
            double availableHeight,
            string path)
        {
            if (column.Height.HasValue)
            {
                return new BlockSplitOutcome(null, column, TextBreakKind.ContainerChild, new[] { new RichLayoutSplitMetadata(path, TextBreakKind.ContainerChild) }, false, false);
            }

            var style = column.Style ?? new LayoutContainerStyle();
            var computedStyle = StyleResolver.Resolve(style);
            var edges = computedStyle.Box.Edges;
            var gap = computedStyle.Gap;
            var innerWidth = Math.Max(1d, contentWidth - edges.HorizontalInset);
            var innerAvailableHeight = Math.Max(0d, availableHeight - edges.VerticalInset);
            var fittedChildren = new List<LayoutChild>();
            var remainderChildren = new List<LayoutChild>();
            var metadata = new List<RichLayoutSplitMetadata>();
            var remainingHeight = innerAvailableHeight;

            for (var i = 0; i < column.Children.Count; i++)
            {
                var child = column.Children[i];
                var childEdges = StyleResolver.Resolve(child.BoxStyle).Edges;
                var childBlock = new ColumnBlock(new[] { child }, null, new LayoutContainerStyle());
                var childLayout = MeasureBlocks(request, new RichTextBlock[] { childBlock }, innerWidth);
                var childHeight = childLayout.NaturalHeight;
                var gapBefore = fittedChildren.Count > 0 ? gap : 0d;
                if (FitsWithinAvailableHeight(childLayout, Math.Max(0d, remainingHeight - gapBefore)))
                {
                    fittedChildren.Add(child);
                    remainingHeight -= childHeight + gapBefore;
                    continue;
                }

                var splitHeight = Math.Max(0d, remainingHeight - gapBefore);
                var split = FitBlocks(request, child.Blocks, Math.Max(1d, innerWidth - childEdges.HorizontalInset), Math.Max(0d, splitHeight - childEdges.VerticalInset), $"{path}.Children[{i}].Blocks");
                if (split.FittedBlocks.Count > 0)
                {
                    fittedChildren.Add(child with { Blocks = split.FittedBlocks });
                }

                if (split.RemainderBlocks.Count > 0)
                {
                    remainderChildren.Add(child with { Blocks = split.RemainderBlocks });
                }

                for (var j = i + 1; j < column.Children.Count; j++)
                {
                    remainderChildren.Add(column.Children[j]);
                }

                metadata.Add(new RichLayoutSplitMetadata($"{path}.Children[{i}]", TextBreakKind.ContainerChild));
                metadata.AddRange(split.Metadata);
                return new BlockSplitOutcome(
                    fittedChildren.Count == 0 ? null : new ColumnBlock(fittedChildren, null, column.Style),
                    remainderChildren.Count == 0 ? null : new ColumnBlock(remainderChildren, null, column.Style),
                    TextBreakKind.ContainerChild,
                    metadata,
                    true,
                    true);
            }

            return new BlockSplitOutcome(new ColumnBlock(fittedChildren, null, column.Style), null, TextBreakKind.None, metadata, false, false);
        }

        public static BlockSplitOutcome SplitRowBlock(
            RichTextBoxLayoutRequest request,
            RowBlock row,
            double contentWidth,
            double availableHeight,
            string path)
        {
            if (row.Height.HasValue)
            {
                return new BlockSplitOutcome(null, row, TextBreakKind.ContainerChild, new[] { new RichLayoutSplitMetadata(path, TextBreakKind.ContainerChild) }, false, false);
            }

            var style = row.Style ?? new LayoutContainerStyle();
            var computedStyle = StyleResolver.Resolve(style);
            var edges = computedStyle.Box.Edges;
            var gap = computedStyle.Gap;
            var innerWidth = Math.Max(1d, contentWidth - edges.HorizontalInset);
            var innerAvailableHeight = Math.Max(0d, availableHeight - edges.VerticalInset);
            if (innerAvailableHeight <= 0d)
            {
                return new BlockSplitOutcome(null, row, TextBreakKind.ContainerChild, new[] { new RichLayoutSplitMetadata(path, TextBreakKind.ContainerChild) }, false, false);
            }

            var childInsets = row.Children.Select(x => StyleResolver.Resolve(x.BoxStyle).Edges).ToArray();
            var totalGap = gap * Math.Max(0d, row.Children.Count - 1);
            var childWidths = ResolveChildMainSizes(row.Children, Math.Max(0d, innerWidth - totalGap), childInsets.Select(x => x.HorizontalInset).ToArray());
            var fittedChildren = new List<LayoutChild>(row.Children.Count);
            var remainderChildren = new List<LayoutChild>(row.Children.Count);
            var metadata = new List<RichLayoutSplitMetadata>();
            var anyFitted = false;

            for (var i = 0; i < row.Children.Count; i++)
            {
                var child = row.Children[i];
                var childEdges = childInsets[i];
                var split = FitBlocks(
                    request,
                    child.Blocks,
                    Math.Max(1d, childWidths[i] - childEdges.HorizontalInset),
                    Math.Max(0d, innerAvailableHeight - childEdges.VerticalInset),
                    $"{path}.Children[{i}].Blocks");

                fittedChildren.Add(child with { Blocks = split.FittedBlocks });
                remainderChildren.Add(child with { Blocks = split.RemainderBlocks });
                anyFitted |= split.FittedBlocks.Count > 0;
                metadata.AddRange(split.Metadata);
            }

            if (!anyFitted)
            {
                return new BlockSplitOutcome(null, row, TextBreakKind.ContainerChild, new[] { new RichLayoutSplitMetadata(path, TextBreakKind.ContainerChild) }, false, false);
            }

            var fittedRow = new RowBlock(fittedChildren, null, row.Style);
            var hasRemainder = remainderChildren.Any(x => x.Blocks.Count > 0);
            var remainderRow = hasRemainder ? new RowBlock(remainderChildren, null, row.Style) : null;
            if (hasRemainder)
            {
                metadata.Insert(0, new RichLayoutSplitMetadata(path, TextBreakKind.ContainerChild));
            }

            return new BlockSplitOutcome(fittedRow, remainderRow, hasRemainder ? TextBreakKind.ContainerChild : TextBreakKind.None, metadata, hasRemainder, hasRemainder);
        }

        public static void AppendRow(RichTextBoxLayoutRequest request, RowBlock row, double contentWidth, double xOffset, string path, ref LayoutState state)
            => AppendContainer(request, row.Children, row.Height, row.Style ?? new LayoutContainerStyle(), LayoutAxis.Horizontal, contentWidth, xOffset, path, ref state);

        public static void AppendColumn(RichTextBoxLayoutRequest request, ColumnBlock column, double contentWidth, double xOffset, string path, ref LayoutState state)
            => AppendContainer(request, column.Children, column.Height, column.Style ?? new LayoutContainerStyle(), LayoutAxis.Vertical, contentWidth, xOffset, path, ref state);
    }

    private static class ListBlockFormatter
    {
        public static void AppendUnorderedList(RichTextBoxLayoutRequest request, UnorderedListBlock list, double contentWidth, double xOffset, string path, ref LayoutState state)
        {
            var listStyle = StyleResolver.Resolve(list);
            var metrics = ResolveUnorderedListMetrics(request, list, listStyle);
            for (var i = 0; i < list.Items.Count; i++)
            {
                var markerText = listStyle.UseVectorMarker ? $"{listStyle.MarkerText} " : listStyle.MarkerText;
                AppendListItem(request, list.Items[i], markerText, contentWidth, xOffset, $"{path}.Items[{i}]", ref state, listStyle.UseVectorMarker, metrics);
            }

            state.ConsumedHeight += list.MarginBlockEnd;
        }

        public static void AppendOrderedList(RichTextBoxLayoutRequest request, OrderedListBlock list, double contentWidth, double xOffset, string path, ref LayoutState state)
        {
            var metrics = ResolveOrderedListMetrics(request, list);
            for (var i = 0; i < list.Items.Count; i++)
            {
                var markerText = OrderedListMarkerFormatter.Format(list.MarkerStyle, list.StartIndex + i, includeTrailingSpace: true);
                AppendListItem(request, list.Items[i], markerText, contentWidth, xOffset, $"{path}.Items[{i}]", ref state, false, metrics);
            }

            state.ConsumedHeight += list.MarginBlockEnd;
        }

        public static BlockSplitOutcome SplitUnorderedListBlock(
            RichTextBoxLayoutRequest request,
            UnorderedListBlock list,
            double contentWidth,
            double availableHeight,
            string path)
        {
            var metrics = ResolveUnorderedListMetrics(request, list, StyleResolver.Resolve(list));
            return SplitListBlock(
                request,
                list.Items,
                availableHeight,
                path,
                (items, includeMargin) => new UnorderedListBlock(items, includeMargin ? list.MarginBlockEnd : 0d, list.Marker),
                contentWidth - metrics.ContentStart,
                metrics.ContentStart);
        }

        public static BlockSplitOutcome SplitOrderedListBlock(
            RichTextBoxLayoutRequest request,
            OrderedListBlock list,
            double contentWidth,
            double availableHeight,
            string path)
        {
            var metrics = ResolveOrderedListMetrics(request, list);
            return SplitListBlock(
                request,
                list.Items,
                availableHeight,
                path,
                (items, includeMargin) => new OrderedListBlock(items, list.StartIndex, includeMargin ? list.MarginBlockEnd : 0d, list.MarkerStyle),
                contentWidth - metrics.ContentStart,
                metrics.ContentStart);
        }

        private static BlockSplitOutcome SplitListBlock(
            RichTextBoxLayoutRequest request,
            IReadOnlyList<ListItemBlock> items,
            double availableHeight,
            string path,
            Func<IReadOnlyList<ListItemBlock>, bool, RichTextBlock> rebuild,
            double itemContentWidth,
            double contentStart)
        {
            var fittedItems = new List<ListItemBlock>();
            var remainderItems = new List<ListItemBlock>();
            var metadata = new List<RichLayoutSplitMetadata>();
            var remainingHeight = availableHeight;

            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                var itemBlock = rebuild(new[] { item }, false);
                var itemLayout = MeasureBlocks(request, new[] { itemBlock }, double.Max(1d, itemContentWidth + contentStart));
                var itemHeight = itemLayout.NaturalHeight;
                if (FitsWithinAvailableHeight(itemLayout, remainingHeight))
                {
                    fittedItems.Add(item);
                    remainingHeight -= itemHeight;
                    continue;
                }

                var split = FitBlocks(request, item.Blocks, itemContentWidth, remainingHeight, $"{path}.Items[{i}].Blocks");
                if (split.FittedBlocks.Count > 0)
                {
                    fittedItems.Add(new ListItemBlock(split.FittedBlocks));
                }

                if (split.RemainderBlocks.Count > 0)
                {
                    remainderItems.Add(new ListItemBlock(split.RemainderBlocks));
                }

                for (var j = i + 1; j < items.Count; j++)
                {
                    remainderItems.Add(items[j]);
                }

                metadata.Add(new RichLayoutSplitMetadata($"{path}.Items[{i}]", split.BreakKind == TextBreakKind.None ? TextBreakKind.ListItem : split.BreakKind));
                metadata.AddRange(split.Metadata);
                return new BlockSplitOutcome(
                    fittedItems.Count == 0 ? null : rebuild(fittedItems, false),
                    remainderItems.Count == 0 ? null : rebuild(remainderItems, true),
                    TextBreakKind.ListItem,
                    metadata,
                    true,
                    true);
            }

            return new BlockSplitOutcome(rebuild(fittedItems, true), null, TextBreakKind.None, metadata, false, false);
        }

        private static void AppendListItem(
            RichTextBoxLayoutRequest request,
            ListItemBlock item,
            string markerText,
            double contentWidth,
            double xOffset,
            string path,
            ref LayoutState state,
            bool useBuiltInMarkerFont,
            ResolvedListMetrics metrics)
        {
            var itemState = new LayoutState { NextSegmentIndex = state.NextSegmentIndex };
            var markerStyle = ResolveMarkerStyle(
                request,
                FindMarkerStyle(item) ?? throw new InvalidOperationException("List items require at least one text run."),
                markerText);
            var markerAreaWidth = metrics.MarkerColumnWidth;
            var markerTextAreaWidth = metrics.MarkerTextAreaWidth;
            var contentX = xOffset + markerAreaWidth;
            LayoutBlocks(request, item.Blocks, contentWidth, contentX, $"{path}.Blocks", ref itemState);

            if (itemState.Lines.Count == 0)
            {
                state.NextSegmentIndex = itemState.NextSegmentIndex;
                return;
            }
            var markerSegments = new[]
            {
                new TextSegment(markerText, markerStyle, 0, markerText.Length, $"{path}.Marker")
            };
            var markerLayout = useBuiltInMarkerFont
                ? null
                : MeasureSegments(request, markerSegments, TextHorizontalAlignment.Right, markerTextAreaWidth, preserveTrailingWhitespaceInWidth: true, useFallbackFamilies: true);

            var firstLine = itemState.Lines[0];
            if (useBuiltInMarkerFont)
            {
                var markerAdvance = Math.Max(1d, MeasureBuiltInMarkerAdvance(markerText, markerStyle.FontSize));
                var markerX = xOffset + Math.Max(0d, markerTextAreaWidth - markerAdvance);
                var mergedLineX = Math.Min(markerX, firstLine.X);
                var markerBaselineY = firstLine.BaselineY + (markerStyle.FontSize * 0.075d);
                var foreground = markerStyle.ForegroundColor ?? new TextColor(0, 0, 0);
                var markerRuns = new[]
                {
                    new TextLayoutRun(
                        state.NextSegmentIndex,
                        markerText,
                        TextLayoutBuiltInFaces.UnorderedListMarkerFaceId,
                        "Helvetica",
                        400,
                        markerStyle.FontSize,
                        false,
                        markerStyle.Underline,
                        markerStyle.CharacterSpacing,
                        markerStyle.WordSpacing,
                        foreground,
                        markerStyle.BackgroundColor,
                        markerX - mergedLineX,
                        markerBaselineY,
                        markerAdvance,
                        markerAdvance,
                        firstLine.Height,
                        Array.Empty<TextLayoutGlyph>(),
                        false,
                        markerStyle.StrikeThrough)
                };

                var contentRuns = firstLine.Runs
                    .Select(run => run with
                    {
                        X = firstLine.X + run.X - mergedLineX
                    })
                    .ToArray();

                var mergedRuns = markerRuns.Concat(contentRuns).OrderBy(x => x.X).ToArray();
                var mergedWidth = Math.Max(firstLine.Width, GetLineWidth(mergedRuns));
                var mergedMeasuredWidth = Math.Max(firstLine.MeasuredWidth, GetLineMeasuredWidth(mergedRuns));
                itemState.Lines[0] = firstLine with
                {
                    X = mergedLineX,
                    Width = mergedWidth,
                    MeasuredWidth = mergedMeasuredWidth,
                    Runs = mergedRuns
                };
            }
            else
            {
                var markerLine = markerLayout?.Layout.Lines.FirstOrDefault();
                if (markerLine is null)
                {
                    ShiftLines(itemState.Lines, 0d, state.ConsumedHeight);
                    ShiftDecorations(itemState.Decorations, 0d, state.ConsumedHeight);
                    state.Issues.AddRange(itemState.Issues);
                    state.HadOverflow |= itemState.HadOverflow;
                    var earlyBaseIndex = state.Lines.Count;
                    state.Lines.AddRange(itemState.Lines.Select((line, index) => line with { Index = earlyBaseIndex + index }));
                    state.Decorations.AddRange(itemState.Decorations);
                    state.ConsumedHeight += itemState.ConsumedHeight;
                    state.NextSegmentIndex = itemState.NextSegmentIndex;
                    return;
                }

                var markerLineX = xOffset + markerLine.X;
                var mergedLineX = Math.Min(markerLineX, firstLine.X);
                var markerRuns = markerLine.Runs
                    .Select(run => run with
                    {
                        X = markerLineX + run.X - mergedLineX,
                        BaselineY = firstLine.BaselineY,
                        DrawAsVectorBullet = false
                    })
                    .ToArray();

                var contentRuns = firstLine.Runs
                    .Select(run => run with
                    {
                        X = firstLine.X + run.X - mergedLineX
                    })
                    .ToArray();

                var mergedRuns = markerRuns.Concat(contentRuns).OrderBy(x => x.X).ToArray();
                var mergedWidth = Math.Max(firstLine.Width, GetLineWidth(mergedRuns));
                var mergedMeasuredWidth = Math.Max(firstLine.MeasuredWidth, GetLineMeasuredWidth(mergedRuns));
                itemState.Lines[0] = firstLine with
                {
                    X = mergedLineX,
                    Width = mergedWidth,
                    MeasuredWidth = mergedMeasuredWidth,
                    Height = Math.Max(firstLine.Height, markerLine.Height),
                    BaselineOffset = Math.Max(firstLine.BaselineOffset, markerLine.BaselineOffset),
                    Runs = mergedRuns
                };
            }

            ShiftLines(itemState.Lines, 0d, state.ConsumedHeight);
            ShiftDecorations(itemState.Decorations, 0d, state.ConsumedHeight);
            state.Issues.AddRange(itemState.Issues);
            state.HadOverflow |= itemState.HadOverflow;
            var baseIndex = state.Lines.Count;
            state.Lines.AddRange(itemState.Lines.Select((line, index) => line with { Index = baseIndex + index }));
            state.Decorations.AddRange(itemState.Decorations);
            state.ConsumedHeight += itemState.ConsumedHeight;
            state.NextSegmentIndex = itemState.NextSegmentIndex;
        }

        internal static PdfLexer.TextLayout.ResolvedListMetrics ResolveUnorderedListMetrics(RichTextBoxLayoutRequest request, UnorderedListBlock list, ComputedListStyle? listStyle = null)
        {
            var resolvedStyle = listStyle ?? StyleResolver.Resolve(list);
            var firstItem = list.Items.FirstOrDefault();
            var markerStyle = (firstItem is not null ? FindMarkerStyle(firstItem) : null) ?? DefaultMarkerStyle(request);
            var markerWidth = resolvedStyle.UseVectorMarker
                ? MeasureBuiltInMarkerAdvance($"{resolvedStyle.MarkerText} ", markerStyle.FontSize)
                : MeasureMarkerTextWidth(request, resolvedStyle.MarkerText, markerStyle);
            return ListLayoutMetricsResolver.Build(request.ListIndent, request.ListMarkerGap, markerStyle.FontSize, markerWidth);
        }

        internal static PdfLexer.TextLayout.ResolvedListMetrics ResolveOrderedListMetrics(RichTextBoxLayoutRequest request, OrderedListBlock list)
        {
            var firstItem = list.Items.FirstOrDefault();
            var markerStyle = (firstItem is not null ? FindMarkerStyle(firstItem) : null) ?? DefaultMarkerStyle(request);
            var largestIndex = Math.Max(list.StartIndex, list.StartIndex + Math.Max(0, list.Items.Count - 1));
            var widestMarker = OrderedListMarkerFormatter.Format(list.MarkerStyle, largestIndex, includeTrailingSpace: true);
            var markerWidth = MeasureMarkerTextWidth(request, widestMarker, markerStyle);
            return ListLayoutMetricsResolver.Build(request.ListIndent, request.ListMarkerGap, markerStyle.FontSize, markerWidth);
        }

        private static double ResolveUnorderedMarkerWidth(TextSegmentStyle markerStyle)
            => Math.Max(1d, Math.Round(markerStyle.FontSize * 0.42d, 3));

        private static double MeasureBuiltInMarkerAdvance(string markerText, double fontSize)
        {
            var font = Standard14Font.GetHelvetica();
            var width = 0d;
            foreach (var glyphOrShift in font.GetGlyphs(markerText))
            {
                if (glyphOrShift.Glyph is { } glyph)
                {
                    width += glyph.w0 * fontSize;
                }
                else
                {
                    width += glyphOrShift.Shift;
                }
            }

            return Math.Max(1d, width);
        }

        private static double MeasureMarkerTextWidth(RichTextBoxLayoutRequest request, string markerText, TextSegmentStyle markerStyle)
        {
            var markerSegments = new[]
            {
                new TextSegment(markerText, markerStyle, 0, markerText.Length, "Marker")
            };

            var layout = MeasureSegments(request, markerSegments, TextHorizontalAlignment.Right, 1_000d, preserveTrailingWhitespaceInWidth: true, useFallbackFamilies: true).Layout;
            return Math.Max(1d, layout.MeasuredWidth);
        }

        private static TextSegmentStyle DefaultMarkerStyle(RichTextBoxLayoutRequest request)
        {
            var family = request.FallbackFamilyNames.FirstOrDefault() ?? "Helvetica";
            return new TextSegmentStyle(family, 400, 12);
        }

        private static TextSegmentStyle ResolveMarkerStyle(RichTextBoxLayoutRequest request, TextSegmentStyle baseStyle, string markerText)
        {
            if (!markerText.Contains('\u2022', StringComparison.Ordinal))
            {
                return baseStyle;
            }

            var preferredFamily = request.FallbackFamilyNames.FirstOrDefault()
                ?? request.FontLibrary.FamilyNames.LastOrDefault()
                ?? baseStyle.FamilyName;
            return baseStyle with { FamilyName = preferredFamily };
        }

        private static TextSegmentStyle? FindMarkerStyle(ListItemBlock item)
        {
            foreach (var block in item.Blocks)
            {
                if (block is ParagraphBlock paragraph)
                {
                    var style = paragraph.Inlines.OfType<TextRunNode>().Select(x => x.Style).FirstOrDefault();
                    if (style is not null)
                    {
                        return StyleResolver.ToSegmentStyle(StyleResolver.Resolve(style), StyleResolver.Resolve(paragraph.Style));
                    }
                }

                if (block is HeadingBlock heading)
                {
                    var style = heading.Inlines.OfType<TextRunNode>().Select(x => x.Style).FirstOrDefault();
                    if (style is not null)
                    {
                        return StyleResolver.ToSegmentStyle(StyleResolver.Resolve(style), StyleResolver.Resolve(heading.Style));
                    }
                }
            }

            return null;
        }
    }

    private static class FlowBlockFormatter
    {
        public static void AppendParagraph(
            RichTextBoxLayoutRequest request,
            ParagraphBlock block,
            double contentWidth,
            double xOffset,
            string path,
            ref LayoutState state)
            => AppendFlowBlock(request, block.Inlines, StyleResolver.Resolve(block.Style), contentWidth, xOffset, path, ref state);

        public static void AppendHeading(
            RichTextBoxLayoutRequest request,
            HeadingBlock block,
            double contentWidth,
            double xOffset,
            string path,
            ref LayoutState state)
            => AppendFlowBlock(request, block.Inlines, StyleResolver.Resolve(block.Style), contentWidth, xOffset, path, ref state);

        public static BlockSplitOutcome SplitParagraph(
            RichTextBoxLayoutRequest request,
            ParagraphBlock block,
            double contentWidth,
            double availableHeight,
            string path)
            => SplitBlock(
                request,
                block,
                block.Inlines,
                block.Style,
                StyleResolver.Resolve(block.Style),
                contentWidth,
                availableHeight,
                path,
                (inlines, style) => new ParagraphBlock(inlines, style));

        public static BlockSplitOutcome SplitHeading(
            RichTextBoxLayoutRequest request,
            HeadingBlock block,
            double contentWidth,
            double availableHeight,
            string path)
            => SplitBlock(
                request,
                block,
                block.Inlines,
                block.Style,
                StyleResolver.Resolve(block.Style),
                contentWidth,
                availableHeight,
                path,
                (inlines, style) => new HeadingBlock(block.Level, inlines, style));

        private static void AppendFlowBlock(
            RichTextBoxLayoutRequest request,
            IReadOnlyList<InlineNode> inlines,
            ComputedParagraphStyle style,
            double contentWidth,
            double xOffset,
            string path,
            ref LayoutState state)
        {
            state.ConsumedHeight += style.MarginBlockStart;
            var segments = CreateSegments(inlines, style, path);
            var layout = MeasureSegments(request, segments, style.TextAlign, Math.Max(1d, contentWidth - xOffset));
            state.Issues.AddRange(layout.Layout.Issues);

            AppendLines(state.Lines, layout.Layout.Lines, state.ConsumedHeight, xOffset);
            AppendDecorations(state.Decorations, layout.Layout.Decorations, state.ConsumedHeight, xOffset);
            state.HadOverflow |= layout.Layout.Status == TextLayoutStatus.Overflow;
            state.ConsumedHeight += layout.NaturalHeight;
            state.NextSegmentIndex += segments.Count;
            state.ConsumedHeight += style.MarginBlockEnd;
        }

        private static BlockSplitOutcome SplitBlock<TBlock>(
            RichTextBoxLayoutRequest request,
            TBlock originalBlock,
            IReadOnlyList<InlineNode> inlines,
            ParagraphStyle? originalStyle,
            ComputedParagraphStyle style,
            double contentWidth,
            double availableHeight,
            string path,
            Func<IReadOnlyList<InlineNode>, ParagraphStyle?, TBlock> rebuild)
            where TBlock : RichTextBlock
        {
            var split = SplitInlineFlow(request, inlines, style, contentWidth, availableHeight, path);
            if (split.FittingInlines.Count == 0)
            {
                return new BlockSplitOutcome(null, originalBlock, TextBreakKind.Line, new[] { new RichLayoutSplitMetadata(path, TextBreakKind.Line) }, true, true);
            }

            var fittingStyle = split.RemainderInlines.Count == 0 || originalStyle is null
                ? originalStyle
                : originalStyle with { MarginBlockEnd = 0d };
            var fitted = rebuild(split.FittingInlines, fittingStyle);
            var remainder = split.RemainderInlines.Count == 0 ? null : rebuild(split.RemainderInlines, originalStyle);
            return new BlockSplitOutcome(
                fitted,
                remainder,
                split.RemainderInlines.Count == 0 ? TextBreakKind.None : TextBreakKind.Line,
                split.RemainderInlines.Count == 0 ? Array.Empty<RichLayoutSplitMetadata>() : new[] { new RichLayoutSplitMetadata(path, TextBreakKind.Line) },
                split.RemainderInlines.Count > 0,
                split.RemainderInlines.Count > 0);
        }

        private static InlineSplitOutcome SplitInlineFlow(
            RichTextBoxLayoutRequest request,
            IReadOnlyList<InlineNode> inlines,
            ComputedParagraphStyle style,
            double contentWidth,
            double availableHeight,
            string path)
        {
            var mappedSegments = CreateMappedSegments(inlines, style, path);
            var textRequest = new TextBoxLayoutRequest(
                Math.Max(1d, contentWidth),
                Math.Max(1d, availableHeight),
                request.FontLibrary,
                mappedSegments.Select(x => x.Segment).ToArray())
            {
                HorizontalAlignment = style.TextAlign,
                VerticalAlignment = TextVerticalAlignment.Top,
                OverflowMode = TextOverflowMode.Clip,
                MissingFontBehavior = request.MissingFontBehavior,
                MissingGlyphBehavior = request.MissingGlyphBehavior,
                FallbackFamilyNames = request.FallbackFamilyNames,
                PreserveTrailingWhitespaceInWidth = request.PreserveTrailingWhitespaceInWidth,
                MetricPreference = request.MetricPreference
            };

            var fit = new TextBoxLayoutEngine().Fit(textRequest);
            if (fit.RemainderRequest is null || fit.RemainderRequest.Segments.Count == 0)
            {
                return new InlineSplitOutcome(
                    BuildInlineNodesFromSegments(mappedSegments.Select(x => x.Segment).ToArray()),
                    Array.Empty<InlineNode>());
            }

            var fittingSegments = new List<TextSegment>();
            var remainderSegments = fit.RemainderRequest.Segments;
            var firstRemainderSegmentIndex = mappedSegments.Count - remainderSegments.Count;

            for (var i = 0; i < firstRemainderSegmentIndex; i++)
            {
                fittingSegments.Add(mappedSegments[i].Segment);
            }

            if (firstRemainderSegmentIndex >= 0 && firstRemainderSegmentIndex < mappedSegments.Count)
            {
                var original = mappedSegments[firstRemainderSegmentIndex].Segment;
                var remainder = remainderSegments[0];
                var fittingLength = Math.Max(0, original.Text.Length - remainder.Text.Length);
                if (fittingLength > 0)
                {
                    fittingSegments.Add(original with { Text = original.Text[..fittingLength] });
                }
            }

            return new InlineSplitOutcome(
                BuildInlineNodesFromSegments(fittingSegments),
                BuildInlineNodesFromSegments(remainderSegments));
        }

        private static List<MappedSegment> CreateMappedSegments(IReadOnlyList<InlineNode> inlines, ComputedParagraphStyle style, string path)
        {
            var segments = new List<MappedSegment>();
            for (var inlineIndex = 0; inlineIndex < inlines.Count; inlineIndex++)
            {
                var inline = inlines[inlineIndex];
                switch (inline)
                {
                    case TextRunNode text:
                        segments.Add(new MappedSegment(new TextSegment(text.Text, StyleResolver.ToSegmentStyle(StyleResolver.Resolve(text.Style), style), 0, text.Text.Length, $"{path}.Inlines[{inlineIndex}]")));
                        break;
                    case LineBreakNode:
                        if (segments.Count == 0)
                        {
                            continue;
                        }

                        segments[^1] = segments[^1] with { Segment = segments[^1].Segment with { Text = segments[^1].Segment.Text + "\n" } };
                        break;
                }
            }

            return segments;
        }

        private static IReadOnlyList<InlineNode> BuildInlineNodesFromSegments(IReadOnlyList<TextSegment> segments)
        {
            var inlines = new List<InlineNode>();
            foreach (var segment in segments)
            {
                var remaining = segment.Text;
                while (remaining.Length > 0)
                {
                    var lineBreakIndex = remaining.IndexOf('\n');
                    if (lineBreakIndex < 0)
                    {
                        inlines.Add(new TextRunNode(remaining, StyleResolver.ToTextStyle(segment.Style)));
                        break;
                    }

                    var text = remaining[..lineBreakIndex];
                    if (text.Length > 0)
                    {
                        inlines.Add(new TextRunNode(text, StyleResolver.ToTextStyle(segment.Style)));
                    }

                    inlines.Add(new LineBreakNode());
                    remaining = remaining[(lineBreakIndex + 1)..];
                    if (remaining.Length == 0)
                    {
                        break;
                    }
                }
            }

            return inlines;
        }

        private static List<TextSegment> CreateSegments(IReadOnlyList<InlineNode> inlines, ComputedParagraphStyle style, string path)
        {
            var segments = new List<TextSegment>();
            for (var inlineIndex = 0; inlineIndex < inlines.Count; inlineIndex++)
            {
                var inline = inlines[inlineIndex];
                switch (inline)
                {
                    case TextRunNode text:
                        segments.Add(new TextSegment(text.Text, StyleResolver.ToSegmentStyle(StyleResolver.Resolve(text.Style), style), 0, text.Text.Length, $"{path}.Inlines[{inlineIndex}]"));
                        break;
                    case LineBreakNode:
                        if (segments.Count == 0)
                        {
                            continue;
                        }

                        var current = segments[^1];
                        segments[^1] = current with { Text = current.Text + "\n" };
                        break;
                    default:
                        throw new NotSupportedException($"Unsupported inline node type: {inline.GetType().Name}");
                }
            }

            return segments;
        }
    }

    private sealed class RichFitPlanMaterializer : ITextLayoutFitPlanMaterializer
    {
        private readonly RichContentSlice _fittedContent;
        private readonly RichContentSlice? _remainderContent;

        public RichFitPlanMaterializer(RichContentSlice fittedContent, RichContentSlice? remainderContent)
        {
            _fittedContent = fittedContent;
            _remainderContent = remainderContent;
        }

        public TextBoxLayoutRequest MaterializeFittedRequest(TextBoxLayoutRequest template)
            => throw new NotSupportedException("Rich-text fit plans cannot materialize flat-text requests.");

        public TextBoxLayoutRequest? MaterializeRemainderRequest(TextBoxLayoutRequest template)
            => throw new NotSupportedException("Rich-text fit plans cannot materialize flat-text requests.");

        public RichTextBoxLayoutRequest MaterializeFittedRequest(RichTextBoxLayoutRequest template)
            => _fittedContent.ToRequestLike(template);

        public RichTextBoxLayoutRequest? MaterializeRemainderRequest(RichTextBoxLayoutRequest template)
            => _remainderContent?.ToRequestLike(template);
    }

    private enum LayoutAxis
    {
        Horizontal,
        Vertical
    }

    private sealed record CellPlacement(
        TableCellBlock Cell,
        int RowIndex,
        int CellIndex,
        int ColumnIndex,
        int ColSpan,
        int RowSpan);

    private sealed record MeasuredCell(
        CellPlacement Placement,
        TextBoxLayoutResult Layout,
        double Inset,
        double OuterWidth,
        double OuterHeight);

    private sealed record BlockFitOutcome(
        IReadOnlyList<RichTextBlock> FittedBlocks,
        IReadOnlyList<RichTextBlock> RemainderBlocks,
        TextBreakKind BreakKind,
        IReadOnlyList<RichLayoutSplitMetadata> Metadata,
        bool IsOpenStart,
        bool IsOpenEnd);

    private sealed record BlockSplitOutcome(
        RichTextBlock? FittedBlock,
        RichTextBlock? RemainderBlock,
        TextBreakKind BreakKind,
        IReadOnlyList<RichLayoutSplitMetadata> Metadata,
        bool IsOpenStart,
        bool IsOpenEnd);

    private sealed record InlineSplitOutcome(
        IReadOnlyList<InlineNode> FittingInlines,
        IReadOnlyList<InlineNode> RemainderInlines);

    private sealed record MappedSegment(TextSegment Segment);
}
