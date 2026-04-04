namespace PdfLexer.TextLayout;

public sealed class RichTextBoxLayoutEngine
{
    private const double LargeLayoutHeight = 1_000_000d;

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

        var inset = request.BoxStyle.Inset;
        var contentWidth = GetContentWidth(request);
        var contentHeight = GetContentHeight(request);
        if (contentWidth <= 0 || contentHeight <= 0)
        {
            return CreateNoContentAreaResult(request);
        }

        var state = new LayoutState();
        LayoutBlocks(request, request.Blocks, contentWidth, 0d, ref state);

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

        var renderedLines = request.OverflowMode == TextOverflowMode.Clip
            ? ClipLines(contentHeight, state.Lines)
            : state.Lines.ToList();
        var renderedDecorations = request.OverflowMode == TextOverflowMode.Clip
            ? ClipDecorations(contentHeight, state.Decorations)
            : state.Decorations.ToList();

        if ((!fitsWidth || !fitsHeight) && status != TextLayoutStatus.Error)
        {
            state.Issues.Add(new TextLayoutIssue(TextLayoutIssueKind.Overflow, "Text content exceeds the target text box."));
        }

        var renderedContentHeight = Math.Max(GetMaxBottom(renderedLines), GetMaxBottom(renderedDecorations));
        var renderedContentWidth = Math.Max(GetMaxRight(renderedLines), GetMaxRight(renderedDecorations));
        ApplyVerticalAlignment(request, renderedLines, renderedDecorations, renderedContentHeight, contentHeight);
        OffsetLines(renderedLines, inset, inset);
        OffsetDecorations(renderedDecorations, inset, inset);

        return new TextBoxLayoutResult
        {
            Status = status,
            FitsWidth = fitsWidth,
            FitsHeight = fitsHeight,
            MeasuredWidth = measuredWidth + (inset * 2d),
            MeasuredHeight = measuredHeight + (inset * 2d),
            RenderedWidth = renderedContentWidth + (inset * 2d),
            RenderedHeight = renderedContentHeight + (inset * 2d),
            BoxStyle = request.BoxStyle,
            Lines = renderedLines,
            Issues = state.Issues.ToArray(),
            Decorations = renderedDecorations
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
                true);
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
            Math.Max(0d, fittingLayout.MeasuredHeight - (request.BoxStyle.Inset * 2d)),
            Math.Max(0d, fittingLayout.MeasuredWidth - (request.BoxStyle.Inset * 2d)),
            outcome.BreakKind,
            outcome.RemainderBlocks.Count > 0,
            outcome.Metadata);
    }

    private static void LayoutBlocks(RichTextBoxLayoutRequest request, IReadOnlyList<RichTextBlock> blocks, double contentWidth, double xOffset, ref LayoutState state)
    {
        foreach (var block in blocks)
        {
            switch (block)
            {
                case ParagraphBlock paragraph:
                    AppendFlowBlock(request, paragraph.Inlines, paragraph.Style ?? new ParagraphStyle(), contentWidth, xOffset, ref state);
                    break;
                case HeadingBlock heading:
                    AppendFlowBlock(request, heading.Inlines, heading.Style ?? new ParagraphStyle(), contentWidth, xOffset, ref state);
                    break;
                case UnorderedListBlock unordered:
                    AppendUnorderedList(request, unordered, contentWidth, xOffset, ref state);
                    break;
                case OrderedListBlock ordered:
                    AppendOrderedList(request, ordered, contentWidth, xOffset, ref state);
                    break;
                case TableBlock table:
                    AppendTable(request, table, contentWidth, xOffset, ref state);
                    break;
                case RowBlock row:
                    AppendContainer(request, row.Children, row.Height, row.Style ?? new LayoutContainerStyle(), LayoutAxis.Horizontal, contentWidth, xOffset, ref state);
                    break;
                case ColumnBlock column:
                    AppendContainer(request, column.Children, column.Height, column.Style ?? new LayoutContainerStyle(), LayoutAxis.Vertical, contentWidth, xOffset, ref state);
                    break;
                default:
                    throw new NotSupportedException($"Unsupported rich text block type: {block.GetType().Name}");
            }
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
            var candidateFitted = fitted.Concat(new[] { block }).ToArray();
            var candidateLayout = MeasureBlocks(request, candidateFitted, contentWidth);
            var candidateHeight = Math.Max(0d, candidateLayout.MeasuredHeight);
            if (FitsWithinAvailableHeight(candidateLayout, availableHeight))
            {
                fitted.Add(block);
                fittedHeight = candidateHeight;
                continue;
            }

            var remainingHeight = Math.Max(0d, availableHeight - fittedHeight);
            var split = SplitBlock(request, block, contentWidth, remainingHeight, path);
            if (split.FittedBlock is not null)
            {
                var splitCandidate = fitted.Concat(new[] { split.FittedBlock }).ToArray();
                var splitCandidateLayout = MeasureBlocks(request, splitCandidate, contentWidth);
                var splitCandidateHeight = Math.Max(0d, splitCandidateLayout.MeasuredHeight);
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
        => block switch
        {
            ParagraphBlock paragraph => SplitParagraphBlock(request, paragraph, contentWidth, availableHeight, path),
            HeadingBlock heading => SplitHeadingBlock(request, heading, contentWidth, availableHeight, path),
            UnorderedListBlock unordered => SplitUnorderedListBlock(request, unordered, contentWidth, availableHeight, path),
            OrderedListBlock ordered => SplitOrderedListBlock(request, ordered, contentWidth, availableHeight, path),
            TableBlock table => SplitTableBlock(request, table, contentWidth, availableHeight, path),
            ColumnBlock column => SplitColumnBlock(request, column, contentWidth, availableHeight, path),
            RowBlock row => SplitRowBlock(request, row, contentWidth, availableHeight, path),
            _ => new BlockSplitOutcome(null, block, TextBreakKind.None, Array.Empty<RichLayoutSplitMetadata>(), false, false)
        };

    private static BlockSplitOutcome SplitParagraphBlock(
        RichTextBoxLayoutRequest request,
        ParagraphBlock block,
        double contentWidth,
        double availableHeight,
        string path)
    {
        var split = SplitInlineFlow(request, block.Inlines, block.Style ?? new ParagraphStyle(), contentWidth, availableHeight);
        if (split.FittingInlines.Count == 0)
        {
            return new BlockSplitOutcome(null, block, TextBreakKind.Line, new[] { new RichLayoutSplitMetadata(path, TextBreakKind.Line) }, true, true);
        }

        var fittingStyle = split.RemainderInlines.Count == 0 || block.Style is null
            ? block.Style
            : block.Style with { MarginBlockEnd = 0d };
        var fitted = new ParagraphBlock(split.FittingInlines, fittingStyle);
        var remainder = split.RemainderInlines.Count == 0 ? null : new ParagraphBlock(split.RemainderInlines, block.Style);
        return new BlockSplitOutcome(fitted, remainder, split.RemainderInlines.Count == 0 ? TextBreakKind.None : TextBreakKind.Line, split.RemainderInlines.Count == 0 ? Array.Empty<RichLayoutSplitMetadata>() : new[] { new RichLayoutSplitMetadata(path, TextBreakKind.Line) }, split.RemainderInlines.Count > 0, split.RemainderInlines.Count > 0);
    }

    private static BlockSplitOutcome SplitHeadingBlock(
        RichTextBoxLayoutRequest request,
        HeadingBlock block,
        double contentWidth,
        double availableHeight,
        string path)
    {
        var split = SplitInlineFlow(request, block.Inlines, block.Style ?? new ParagraphStyle(), contentWidth, availableHeight);
        if (split.FittingInlines.Count == 0)
        {
            return new BlockSplitOutcome(null, block, TextBreakKind.Line, new[] { new RichLayoutSplitMetadata(path, TextBreakKind.Line) }, true, true);
        }

        var fittingStyle = split.RemainderInlines.Count == 0 || block.Style is null
            ? block.Style
            : block.Style with { MarginBlockEnd = 0d };
        var fitted = new HeadingBlock(block.Level, split.FittingInlines, fittingStyle);
        var remainder = split.RemainderInlines.Count == 0 ? null : new HeadingBlock(block.Level, split.RemainderInlines, block.Style);
        return new BlockSplitOutcome(fitted, remainder, split.RemainderInlines.Count == 0 ? TextBreakKind.None : TextBreakKind.Line, split.RemainderInlines.Count == 0 ? Array.Empty<RichLayoutSplitMetadata>() : new[] { new RichLayoutSplitMetadata(path, TextBreakKind.Line) }, split.RemainderInlines.Count > 0, split.RemainderInlines.Count > 0);
    }

    private static BlockSplitOutcome SplitUnorderedListBlock(
        RichTextBoxLayoutRequest request,
        UnorderedListBlock list,
        double contentWidth,
        double availableHeight,
        string path)
        => SplitListBlock(
            request,
            list.Items,
            availableHeight,
            path,
            (items, includeMargin) => new UnorderedListBlock(items, includeMargin ? list.MarginBlockEnd : 0d, list.Marker),
            contentWidth - request.ListIndent);

    private static BlockSplitOutcome SplitOrderedListBlock(
        RichTextBoxLayoutRequest request,
        OrderedListBlock list,
        double contentWidth,
        double availableHeight,
        string path)
        => SplitListBlock(
            request,
            list.Items,
            availableHeight,
            path,
            (items, includeMargin) => new OrderedListBlock(items, list.StartIndex, includeMargin ? list.MarginBlockEnd : 0d),
            contentWidth - request.ListIndent);

    private static BlockSplitOutcome SplitListBlock(
        RichTextBoxLayoutRequest request,
        IReadOnlyList<ListItemBlock> items,
        double availableHeight,
        string path,
        Func<IReadOnlyList<ListItemBlock>, bool, RichTextBlock> rebuild,
        double itemContentWidth)
    {
        var fittedItems = new List<ListItemBlock>();
        var remainderItems = new List<ListItemBlock>();
        var metadata = new List<RichLayoutSplitMetadata>();
        var remainingHeight = availableHeight;

        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];
            var itemBlock = rebuild(new[] { item }, false);
            var itemLayout = MeasureBlocks(request, new[] { itemBlock }, double.Max(1d, itemContentWidth + request.ListIndent));
            var itemHeight = itemLayout.MeasuredHeight;
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

    private static BlockSplitOutcome SplitTableBlock(
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

        var policy = table.ContinuationPolicy;
        var footerRows = policy?.ContinuationFooterRows ?? Array.Empty<TableRowBlock>();
        var headerRows = policy?.ContinuationHeaderRows ?? Array.Empty<TableRowBlock>();
        var fittedRows = new List<TableRowBlock>();
        var remainingBodyRows = table.Rows.ToList();

        for (var i = 0; i < table.Rows.Count; i++)
        {
            fittedRows.Add(table.Rows[i]);
            remainingBodyRows.RemoveAt(0);
            var candidateRows = remainingBodyRows.Count > 0 ? fittedRows.Concat(footerRows).ToArray() : fittedRows.ToArray();
            var candidateTable = new TableBlock(candidateRows, table.Style, table.ContinuationPolicy);
            var candidateLayout = MeasureBlocks(request, new RichTextBlock[] { candidateTable }, contentWidth);
            if (!FitsWithinAvailableHeight(candidateLayout, availableHeight))
            {
                fittedRows.RemoveAt(fittedRows.Count - 1);
                remainingBodyRows.Insert(0, table.Rows[i]);
                break;
            }
        }

        if (fittedRows.Count == 0)
        {
            return new BlockSplitOutcome(null, table, TextBreakKind.TableRow, new[] { new TableSplitMetadata(path, 0, table.Rows.Count, false, false) }, false, false);
        }

        if (remainingBodyRows.Count == 0)
        {
            return new BlockSplitOutcome(new TableBlock(fittedRows, table.Style, table.ContinuationPolicy), null, TextBreakKind.None, Array.Empty<RichLayoutSplitMetadata>(), false, false);
        }

        var fittedTable = new TableBlock(fittedRows.Concat(footerRows).ToArray(), table.Style, table.ContinuationPolicy);
        var remainderRows = headerRows.Concat(remainingBodyRows).ToArray();
        var remainderTable = new TableBlock(remainderRows, table.Style, table.ContinuationPolicy);
        return new BlockSplitOutcome(
            fittedTable,
            remainderTable,
            TextBreakKind.TableRow,
            new RichLayoutSplitMetadata[]
            {
                new TableSplitMetadata(path, fittedRows.Count, remainingBodyRows.Count, headerRows.Count > 0, footerRows.Count > 0)
            },
            false,
            false);
    }

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
        var inset = style.Inset;
        var gap = Math.Max(0d, style.Gap);
        var innerWidth = Math.Max(1d, contentWidth - (inset * 2d));
        var innerAvailableHeight = Math.Max(0d, availableHeight - (inset * 2d));
        var fittedChildren = new List<LayoutChild>();
        var remainderChildren = new List<LayoutChild>();
        var metadata = new List<RichLayoutSplitMetadata>();
        var remainingHeight = innerAvailableHeight;

        for (var i = 0; i < column.Children.Count; i++)
        {
            var child = column.Children[i];
            var childInset = child.BoxStyle?.Inset ?? 0d;
            var childBlock = new ColumnBlock(new[] { child }, null, new LayoutContainerStyle());
            var childLayout = MeasureBlocks(request, new RichTextBlock[] { childBlock }, innerWidth);
            var childHeight = childLayout.MeasuredHeight;
            var gapBefore = fittedChildren.Count > 0 ? gap : 0d;
            if (FitsWithinAvailableHeight(childLayout, Math.Max(0d, remainingHeight - gapBefore)))
            {
                fittedChildren.Add(child);
                remainingHeight -= childHeight + gapBefore;
                continue;
            }

            var splitHeight = Math.Max(0d, remainingHeight - gapBefore);
            var split = FitBlocks(request, child.Blocks, Math.Max(1d, innerWidth - (childInset * 2d)), Math.Max(0d, splitHeight - (childInset * 2d)), $"{path}.Children[{i}].Blocks");
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
        var inset = style.Inset;
        var gap = Math.Max(0d, style.Gap);
        var innerWidth = Math.Max(1d, contentWidth - (inset * 2d));
        var innerAvailableHeight = Math.Max(0d, availableHeight - (inset * 2d));
        if (innerAvailableHeight <= 0d)
        {
            return new BlockSplitOutcome(null, row, TextBreakKind.ContainerChild, new[] { new RichLayoutSplitMetadata(path, TextBreakKind.ContainerChild) }, false, false);
        }

        var childInsets = row.Children.Select(x => x.BoxStyle?.Inset ?? 0d).ToArray();
        var totalGap = gap * Math.Max(0d, row.Children.Count - 1);
        var childWidths = ResolveChildMainSizes(row.Children, Math.Max(0d, innerWidth - totalGap), childInsets.Select(x => x * 2d).ToArray());
        var fittedChildren = new List<LayoutChild>(row.Children.Count);
        var remainderChildren = new List<LayoutChild>(row.Children.Count);
        var metadata = new List<RichLayoutSplitMetadata>();
        var anyFitted = false;

        for (var i = 0; i < row.Children.Count; i++)
        {
            var child = row.Children[i];
            var childInset = childInsets[i];
            var split = FitBlocks(
                request,
                child.Blocks,
                Math.Max(1d, childWidths[i] - (childInset * 2d)),
                Math.Max(0d, innerAvailableHeight - (childInset * 2d)),
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

    private static InlineSplitOutcome SplitInlineFlow(
        RichTextBoxLayoutRequest request,
        IReadOnlyList<InlineNode> inlines,
        ParagraphStyle style,
        double contentWidth,
        double availableHeight)
    {
        var mappedSegments = CreateMappedSegments(inlines, style);
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

    private static List<MappedSegment> CreateMappedSegments(IReadOnlyList<InlineNode> inlines, ParagraphStyle style)
    {
        var segments = new List<MappedSegment>();
        foreach (var inline in inlines)
        {
            switch (inline)
            {
                case TextRunNode text:
                    segments.Add(new MappedSegment(new TextSegment(text.Text, new TextSegmentStyle(
                        text.Style.FamilyName,
                        text.Style.Weight,
                        text.Style.FontSize,
                        text.Style.Underline,
                        text.Style.CharacterSpacing,
                        text.Style.WordSpacing,
                        style.LineHeight,
                        text.Style.Italic,
                        text.Style.ForegroundColor,
                        text.Style.BackgroundColor))));
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
                    inlines.Add(new TextRunNode(remaining, ToTextStyle(segment.Style)));
                    break;
                }

                var text = remaining[..lineBreakIndex];
                if (text.Length > 0)
                {
                    inlines.Add(new TextRunNode(text, ToTextStyle(segment.Style)));
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

    private static TextStyle ToTextStyle(TextSegmentStyle style)
        => new(
            style.FamilyName,
            style.Weight,
            style.FontSize,
            style.Italic,
            style.Underline,
            style.CharacterSpacing,
            style.WordSpacing,
            style.ForegroundColor,
            style.BackgroundColor);

    private static void AppendFlowBlock(
        RichTextBoxLayoutRequest request,
        IReadOnlyList<InlineNode> inlines,
        ParagraphStyle style,
        double contentWidth,
        double xOffset,
        ref LayoutState state)
    {
        var segments = CreateSegments(inlines, style);
        var layout = MeasureSegments(request, segments, style.TextAlign, contentWidth - xOffset);
        state.Issues.AddRange(layout.Issues);

        AppendLines(state.Lines, layout.Lines, state.ConsumedHeight, xOffset);
        AppendDecorations(state.Decorations, layout.Decorations, state.ConsumedHeight, xOffset);
        state.HadOverflow |= layout.Status == TextLayoutStatus.Overflow;
        state.ConsumedHeight += layout.MeasuredHeight;
        state.NextSegmentIndex += segments.Count;
        state.ConsumedHeight += style.MarginBlockEnd;
    }

    private static void AppendUnorderedList(RichTextBoxLayoutRequest request, UnorderedListBlock list, double contentWidth, double xOffset, ref LayoutState state)
    {
        foreach (var item in list.Items)
        {
            AppendListItem(request, item, list.Marker, contentWidth, xOffset, ref state, IsVectorBulletMarker(list.Marker));
        }

        state.ConsumedHeight += list.MarginBlockEnd;
    }

    private static void AppendOrderedList(RichTextBoxLayoutRequest request, OrderedListBlock list, double contentWidth, double xOffset, ref LayoutState state)
    {
        for (var i = 0; i < list.Items.Count; i++)
        {
            AppendListItem(request, list.Items[i], $"{list.StartIndex + i}.", contentWidth, xOffset, ref state, false);
        }

        state.ConsumedHeight += list.MarginBlockEnd;
    }

    private static void AppendListItem(
        RichTextBoxLayoutRequest request,
        ListItemBlock item,
        string markerText,
        double contentWidth,
        double xOffset,
        ref LayoutState state,
        bool useBuiltInMarkerFont)
    {
        var itemState = new LayoutState { NextSegmentIndex = state.NextSegmentIndex };
        var markerAreaWidth = Math.Max(0d, request.ListIndent - request.ListMarkerGap);
        var contentX = xOffset + request.ListIndent;
        LayoutBlocks(request, item.Blocks, contentWidth, contentX, ref itemState);

        if (itemState.Lines.Count == 0)
        {
            state.NextSegmentIndex = itemState.NextSegmentIndex;
            return;
        }

        var markerStyle = FindMarkerStyle(item) ?? throw new InvalidOperationException("List items require at least one text run.");
        var markerSegments = new[]
        {
            new TextSegment(markerText, new TextSegmentStyle(
                markerStyle.FamilyName,
                markerStyle.Weight,
                markerStyle.FontSize,
                markerStyle.Underline,
                markerStyle.CharacterSpacing,
                markerStyle.WordSpacing,
                markerStyle.LineSpacing,
                markerStyle.Italic,
                markerStyle.ForegroundColor,
                markerStyle.BackgroundColor))
        };
        var markerLayout = useBuiltInMarkerFont
            ? null
            : MeasureSegments(request, markerSegments, TextHorizontalAlignment.Right, Math.Max(1d, markerAreaWidth));

        var firstLine = itemState.Lines[0];
        if (useBuiltInMarkerFont)
        {
            var markerWidth = Math.Max(1d, markerStyle.FontSize * 0.42d);
            var markerCenterX = xOffset + (markerAreaWidth * 0.62d);
            var markerX = Math.Max(xOffset, Math.Min(xOffset + Math.Max(0d, markerAreaWidth - markerWidth), markerCenterX - (markerWidth / 2d)));
            var mergedLineX = Math.Min(markerX, firstLine.X);
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
                    firstLine.BaselineY,
                    markerWidth,
                    markerWidth,
                    firstLine.Height,
                    Array.Empty<TextLayoutGlyph>(),
                    false)
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
            var markerLine = markerLayout.Lines.FirstOrDefault();
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

    private static void AppendContainer(
        RichTextBoxLayoutRequest request,
        IReadOnlyList<LayoutChild> children,
        double? fixedHeight,
        LayoutContainerStyle style,
        LayoutAxis axis,
        double contentWidth,
        double xOffset,
        ref LayoutState state)
    {
        var outerWidth = Math.Max(0d, contentWidth - xOffset);
        var boxStyle = style.ToTextBoxStyle();
        var inset = boxStyle.Inset;
        var innerWidth = Math.Max(1d, outerWidth - (inset * 2d));
        var gap = Math.Max(0d, style.Gap);

        if (children.Count == 0)
        {
            AddBoxStyleDecorations(state.Decorations, xOffset, state.ConsumedHeight, outerWidth, inset * 2d, boxStyle);
            state.ConsumedHeight += (inset * 2d) + style.MarginBlockEnd;
            return;
        }

        if (axis == LayoutAxis.Horizontal)
        {
            AppendHorizontalContainer(request, children, fixedHeight, style, boxStyle, outerWidth, innerWidth, gap, xOffset, ref state);
            return;
        }

        AppendVerticalContainer(request, children, fixedHeight, style, boxStyle, outerWidth, innerWidth, gap, xOffset, ref state);
    }

    private static void AppendHorizontalContainer(
        RichTextBoxLayoutRequest request,
        IReadOnlyList<LayoutChild> children,
        double? fixedHeight,
        LayoutContainerStyle style,
        TextBoxStyle boxStyle,
        double outerWidth,
        double innerWidth,
        double gap,
        double xOffset,
        ref LayoutState state)
    {
        var containerTop = state.ConsumedHeight;
        var inset = style.Inset;
        var totalGap = gap * Math.Max(0, children.Count - 1);
        var availableWidth = Math.Max(0d, innerWidth - totalGap);
        var childInsets = children.Select(x => x.BoxStyle?.Inset ?? 0d).ToArray();
        var childWidths = ResolveChildMainSizes(children, availableWidth, childInsets.Select(x => x * 2d).ToArray());

        var childLayouts = new List<TextBoxLayoutResult>(children.Count);
        var childHeights = new double[children.Count];
        double rowInnerHeight;

        if (fixedHeight.HasValue)
        {
            rowInnerHeight = Math.Max(0d, fixedHeight.Value - (inset * 2d));
            for (var i = 0; i < children.Count; i++)
            {
                var childInset = childInsets[i];
                var layout = MeasureChildBlocks(
                    request,
                    children[i],
                    Math.Max(1d, childWidths[i] - (childInset * 2d)),
                    Math.Max(1d, rowInnerHeight - (childInset * 2d)),
                    children[i].VerticalAlignment,
                    true);
                childLayouts.Add(layout);
                childHeights[i] = rowInnerHeight;
                state.Issues.AddRange(layout.Issues);
                state.HadOverflow |= layout.Status == TextLayoutStatus.Overflow;
            }
        }
        else
        {
            var initialLayouts = new List<TextBoxLayoutResult>(children.Count);
            for (var i = 0; i < children.Count; i++)
            {
                var childInset = childInsets[i];
                var layout = MeasureChildBlocks(
                    request,
                    children[i],
                    Math.Max(1d, childWidths[i] - (childInset * 2d)),
                    LargeLayoutHeight,
                    TextVerticalAlignment.Top,
                    false);
                initialLayouts.Add(layout);
                childHeights[i] = layout.MeasuredHeight + (childInset * 2d);
            }

            rowInnerHeight = childHeights.Length == 0 ? 0d : childHeights.Max();
            for (var i = 0; i < children.Count; i++)
            {
                if (children[i].VerticalAlignment == TextVerticalAlignment.Top)
                {
                    childLayouts.Add(initialLayouts[i]);
                    state.Issues.AddRange(initialLayouts[i].Issues);
                    state.HadOverflow |= initialLayouts[i].Status == TextLayoutStatus.Overflow;
                    continue;
                }

                var childInset = childInsets[i];
                var layout = MeasureChildBlocks(
                    request,
                    children[i],
                    Math.Max(1d, childWidths[i] - (childInset * 2d)),
                    Math.Max(1d, rowInnerHeight - (childInset * 2d)),
                    children[i].VerticalAlignment,
                    false);
                childLayouts.Add(layout);
                state.Issues.AddRange(layout.Issues);
                state.HadOverflow |= layout.Status == TextLayoutStatus.Overflow;
            }
        }

        var outerHeight = rowInnerHeight + (inset * 2d);
        AddBoxStyleDecorations(state.Decorations, xOffset, containerTop, outerWidth, outerHeight, boxStyle);

        var childX = xOffset + inset;
        for (var i = 0; i < children.Count; i++)
        {
            var childInset = childInsets[i];
            if (children[i].BoxStyle is { } childBoxStyle)
            {
                AddBoxStyleDecorations(state.Decorations, childX, containerTop + inset, childWidths[i], rowInnerHeight, childBoxStyle);
            }

            AppendLines(state.Lines, childLayouts[i].Lines, containerTop + inset + childInset, childX + childInset);
            AppendDecorations(state.Decorations, childLayouts[i].Decorations, containerTop + inset + childInset, childX + childInset);
            childX += childWidths[i] + gap;
        }

        state.ConsumedHeight += outerHeight + style.MarginBlockEnd;
    }

    private static void AppendVerticalContainer(
        RichTextBoxLayoutRequest request,
        IReadOnlyList<LayoutChild> children,
        double? fixedHeight,
        LayoutContainerStyle style,
        TextBoxStyle boxStyle,
        double outerWidth,
        double innerWidth,
        double gap,
        double xOffset,
        ref LayoutState state)
    {
        var containerTop = state.ConsumedHeight;
        var inset = style.Inset;
        var childLayouts = new List<TextBoxLayoutResult>(children.Count);
        var childHeights = new double[children.Count];
        var childInsets = children.Select(x => x.BoxStyle?.Inset ?? 0d).ToArray();

        if (fixedHeight.HasValue)
        {
            var totalGap = gap * Math.Max(0, children.Count - 1);
            var availableHeight = Math.Max(0d, fixedHeight.Value - (inset * 2d) - totalGap);
            childHeights = ResolveChildMainSizes(children, availableHeight, childInsets.Select(x => x * 2d).ToArray());
            for (var i = 0; i < children.Count; i++)
            {
                var childInset = childInsets[i];
                var layout = MeasureChildBlocks(
                    request,
                    children[i],
                    Math.Max(1d, innerWidth - (childInset * 2d)),
                    Math.Max(1d, childHeights[i] - (childInset * 2d)),
                    children[i].VerticalAlignment,
                    true);
                childLayouts.Add(layout);
                state.Issues.AddRange(layout.Issues);
                state.HadOverflow |= layout.Status == TextLayoutStatus.Overflow;
            }
        }
        else
        {
            for (var i = 0; i < children.Count; i++)
            {
                var childHeight = children[i].FixedSize;
                var childInset = childInsets[i];
                var layout = MeasureChildBlocks(
                    request,
                    children[i],
                    Math.Max(1d, innerWidth - (childInset * 2d)),
                    childHeight.HasValue ? Math.Max(1d, childHeight.Value - (childInset * 2d)) : LargeLayoutHeight,
                    childHeight.HasValue ? children[i].VerticalAlignment : TextVerticalAlignment.Top,
                    false);
                childLayouts.Add(layout);
                childHeights[i] = childHeight ?? (layout.MeasuredHeight + (childInset * 2d));
                state.Issues.AddRange(layout.Issues);
                state.HadOverflow |= layout.Status == TextLayoutStatus.Overflow;
            }
        }

        var innerHeight = childHeights.Sum() + (gap * Math.Max(0, children.Count - 1));
        var outerHeight = fixedHeight ?? (innerHeight + (inset * 2d));
        AddBoxStyleDecorations(state.Decorations, xOffset, containerTop, outerWidth, outerHeight, boxStyle);

        var childY = containerTop + inset;
        for (var i = 0; i < children.Count; i++)
        {
            var childInset = childInsets[i];
            if (children[i].BoxStyle is { } childBoxStyle)
            {
                AddBoxStyleDecorations(state.Decorations, xOffset + inset, childY, innerWidth, childHeights[i], childBoxStyle);
            }

            AppendLines(state.Lines, childLayouts[i].Lines, childY + childInset, xOffset + inset + childInset);
            AppendDecorations(state.Decorations, childLayouts[i].Decorations, childY + childInset, xOffset + inset + childInset);
            childY += childHeights[i] + gap;
        }

        state.ConsumedHeight += outerHeight + style.MarginBlockEnd;
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

        var tableStyle = table.Style ?? new TableStyle();
        var placements = BuildCellPlacements(table);
        if (placements.Count == 0)
        {
            state.ConsumedHeight += tableStyle.MarginBlockEnd;
            return;
        }

        var columnCount = placements.Max(x => x.ColumnIndex + x.ColSpan);
        var rowCount = table.Rows.Count;
        var columnWidths = ResolveColumnWidths(placements, columnCount, Math.Max(1d, contentWidth - xOffset));
        var rowHeights = new double[rowCount];
        var measuredCells = new List<MeasuredCell>(placements.Count);

        foreach (var placement in placements)
        {
            var outerWidth = GetSpanWidth(columnWidths, placement.ColumnIndex, placement.ColSpan);
            var cellPadding = placement.Cell.Style?.ResolvePadding(tableStyle) ?? tableStyle.CellPadding;
            var borderWidth = Math.Max(0d, tableStyle.CellBorderWidth);
            var inset = borderWidth + Math.Max(0d, cellPadding);
            var innerWidth = Math.Max(1d, outerWidth - (inset * 2d));
            var cellBlocks = ApplyCellAlignment(placement.Cell, placement.Cell.Style?.TextAlign);
            var cellLayout = MeasureBlocks(request, cellBlocks, innerWidth);
            measuredCells.Add(new MeasuredCell(placement, cellLayout, inset, outerWidth, cellLayout.MeasuredHeight + (inset * 2d)));
            state.Issues.AddRange(cellLayout.Issues);
            state.HadOverflow |= cellLayout.Status == TextLayoutStatus.Overflow;

            if (placement.RowSpan == 1)
            {
                rowHeights[placement.RowIndex] = Math.Max(rowHeights[placement.RowIndex], cellLayout.MeasuredHeight + (inset * 2d));
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
            foreach (var cell in row.Cells)
            {
                while (occupied.Contains((rowIndex, columnIndex)))
                {
                    columnIndex++;
                }

                var colSpan = Math.Max(1, cell.ColSpan);
                var rowSpan = Math.Max(1, Math.Min(cell.RowSpan, table.Rows.Count - rowIndex));
                placements.Add(new CellPlacement(cell, rowIndex, columnIndex, colSpan, rowSpan));
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

    private static double[] ResolveColumnWidths(IReadOnlyList<CellPlacement> placements, int columnCount, double availableWidth)
    {
        var widths = new double[columnCount];
        foreach (var placement in placements)
        {
            if (!placement.Cell.ColWidth.HasValue || placement.Cell.ColWidth.Value <= 0)
            {
                continue;
            }

            var perColumn = placement.Cell.ColWidth.Value / placement.ColSpan;
            for (var i = 0; i < placement.ColSpan; i++)
            {
                widths[placement.ColumnIndex + i] = Math.Max(widths[placement.ColumnIndex + i], perColumn);
            }
        }

        var unspecifiedColumns = widths.Count(x => x <= 0);
        var specifiedTotal = widths.Sum();
        if (unspecifiedColumns > 0)
        {
            var remainder = Math.Max(availableWidth - specifiedTotal, unspecifiedColumns);
            var perColumn = remainder / unspecifiedColumns;
            for (var i = 0; i < widths.Length; i++)
            {
                if (widths[i] <= 0)
                {
                    widths[i] = perColumn;
                }
            }
        }

        if (widths.Sum() <= 0)
        {
            var equalWidth = availableWidth / Math.Max(1, columnCount);
            for (var i = 0; i < widths.Length; i++)
            {
                widths[i] = equalWidth;
            }
        }

        return widths;
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
        List<TextLayoutDecoration> decorations,
        TableStyle tableStyle,
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
            decorations.Add(new TextLayoutFillRectDecoration(tableLeft, tableTop, tableWidth, tableHeight, tableBackground));
        }

        foreach (var cell in cells)
        {
            var width = GetCellWidth(columnLefts, tableLeft + tableWidth, cell.Placement.ColumnIndex, cell.Placement.ColSpan);
            var height = Sum(rowHeights, cell.Placement.RowIndex, cell.Placement.RowSpan);

            if (cell.Placement.Cell.Style?.BackgroundColor is not TextColor background)
            {
                if (tableStyle.CellBorderWidth > 0d)
                {
                    var borderColor = tableStyle.CellBorderColor ?? tableStyle.BorderColor ?? new TextColor(0, 0, 0);
                    decorations.Add(new TextLayoutStrokeRectDecoration(
                        columnLefts[cell.Placement.ColumnIndex],
                        rowTops[cell.Placement.RowIndex],
                        width,
                        height,
                        borderColor,
                        tableStyle.CellBorderWidth));
                }

                continue;
            }

            decorations.Add(new TextLayoutFillRectDecoration(
                columnLefts[cell.Placement.ColumnIndex],
                rowTops[cell.Placement.RowIndex],
                width,
                height,
                background));
            if (tableStyle.CellBorderWidth > 0d)
            {
                var borderColor = tableStyle.CellBorderColor ?? tableStyle.BorderColor ?? new TextColor(0, 0, 0);
                decorations.Add(new TextLayoutStrokeRectDecoration(
                    columnLefts[cell.Placement.ColumnIndex],
                    rowTops[cell.Placement.RowIndex],
                    width,
                    height,
                    borderColor,
                    tableStyle.CellBorderWidth));
            }
        }

        if (tableStyle.CellBorderWidth <= 0d && tableStyle.BorderWidth > 0d && tableStyle.BorderColor is TextColor border)
        {
            AddRectBorder(decorations, tableLeft, tableTop, tableWidth, tableHeight, border, tableStyle.BorderWidth);
        }
    }

    private static double GetCellWidth(IReadOnlyList<double> columnLefts, double tableRight, int startColumn, int colSpan)
    {
        var start = columnLefts[startColumn];
        var endColumn = startColumn + colSpan;
        var end = endColumn < columnLefts.Count ? columnLefts[endColumn] : tableRight;
        return end - start;
    }

    private static void AddRectBorder(List<TextLayoutDecoration> decorations, double x, double y, double width, double height, TextColor color, double thickness)
    {
        decorations.Add(new TextLayoutLineDecoration(x, y, x + width, y, color, thickness));
        decorations.Add(new TextLayoutLineDecoration(x, y + height, x + width, y + height, color, thickness));
        decorations.Add(new TextLayoutLineDecoration(x, y, x, y + height, color, thickness));
        decorations.Add(new TextLayoutLineDecoration(x + width, y, x + width, y + height, color, thickness));
    }

    private static TextBoxLayoutResult MeasureSegments(
        RichTextBoxLayoutRequest request,
        IReadOnlyList<TextSegment> segments,
        TextHorizontalAlignment alignment,
        double width)
    {
        var childRequest = new TextBoxLayoutRequest(
            Math.Max(1d, width),
            LargeLayoutHeight,
            request.FontLibrary,
            segments)
        {
            HorizontalAlignment = alignment,
            VerticalAlignment = TextVerticalAlignment.Top,
            OverflowMode = TextOverflowMode.Visible,
            MissingFontBehavior = request.MissingFontBehavior,
            MissingGlyphBehavior = request.MissingGlyphBehavior,
            FallbackFamilyNames = request.FallbackFamilyNames,
            PreserveTrailingWhitespaceInWidth = request.PreserveTrailingWhitespaceInWidth,
            MetricPreference = request.MetricPreference
        };

        var engine = new TextBoxLayoutEngine();
        return engine.Layout(childRequest);
    }

    private static TextBoxLayoutResult MeasureBlocks(
        RichTextBoxLayoutRequest parentRequest,
        IReadOnlyList<RichTextBlock> blocks,
        double width)
    {
        var childRequest = new RichTextBoxLayoutRequest(
            Math.Max(1d, width),
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

        return new RichTextBoxLayoutEngine().Layout(childRequest);
    }

    private static bool FitsWithinAvailableHeight(TextBoxLayoutResult layout, double availableHeight)
        => layout.RenderedHeight <= availableHeight + 0.0001d;

    private static TextBoxLayoutResult MeasureChildBlocks(
        RichTextBoxLayoutRequest parentRequest,
        LayoutChild child,
        double width,
        double height,
        TextVerticalAlignment verticalAlignment,
        bool constrainHeight)
    {
        var childRequest = new RichTextBoxLayoutRequest(
            Math.Max(1d, width),
            Math.Max(1d, height),
            parentRequest.FontLibrary,
            child.Blocks)
        {
            VerticalAlignment = verticalAlignment,
            OverflowMode = constrainHeight ? parentRequest.OverflowMode : TextOverflowMode.Visible,
            MissingFontBehavior = parentRequest.MissingFontBehavior,
            MissingGlyphBehavior = parentRequest.MissingGlyphBehavior,
            FallbackFamilyNames = parentRequest.FallbackFamilyNames,
            PreserveTrailingWhitespaceInWidth = parentRequest.PreserveTrailingWhitespaceInWidth,
            ListIndent = parentRequest.ListIndent,
            ListMarkerGap = parentRequest.ListMarkerGap,
            BoxStyle = new TextBoxStyle(),
            MetricPreference = parentRequest.MetricPreference
        };

        return new RichTextBoxLayoutEngine().Layout(childRequest);
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

    private static void AddBoxStyleDecorations(List<TextLayoutDecoration> decorations, double x, double y, double width, double height, TextBoxStyle style)
    {
        if (width <= 0d || height <= 0d)
        {
            return;
        }

        if (style.BackgroundColor is TextColor background)
        {
            decorations.Add(new TextLayoutFillRectDecoration(x, y, width, height, background, Math.Max(0d, style.BorderRadius)));
        }

        if (style.BorderWidth > 0d && style.BorderColor is TextColor border)
        {
            decorations.Add(new TextLayoutStrokeRectDecoration(x, y, width, height, border, style.BorderWidth, Math.Max(0d, style.BorderRadius)));
        }
    }

    private static List<TextSegment> CreateSegments(IReadOnlyList<InlineNode> inlines, ParagraphStyle style)
    {
        var segments = new List<TextSegment>();
        foreach (var inline in inlines)
        {
            switch (inline)
            {
                case TextRunNode text:
                    segments.Add(new TextSegment(text.Text, new TextSegmentStyle(
                        text.Style.FamilyName,
                        text.Style.Weight,
                        text.Style.FontSize,
                        text.Style.Underline,
                        text.Style.CharacterSpacing,
                        text.Style.WordSpacing,
                        style.LineHeight,
                        text.Style.Italic,
                        text.Style.ForegroundColor,
                        text.Style.BackgroundColor)));
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

    private static TextSegmentStyle? FindMarkerStyle(ListItemBlock item)
    {
        foreach (var block in item.Blocks)
        {
            if (block is ParagraphBlock paragraph)
            {
                var style = paragraph.Inlines.OfType<TextRunNode>().Select(x => x.Style).FirstOrDefault();
                if (style is not null)
                {
                    return new TextSegmentStyle(
                        style.FamilyName,
                        style.Weight,
                        style.FontSize,
                        style.Underline,
                        style.CharacterSpacing,
                        style.WordSpacing,
                        (paragraph.Style ?? new ParagraphStyle()).LineHeight,
                        style.Italic,
                        style.ForegroundColor,
                        style.BackgroundColor);
                }
            }

            if (block is HeadingBlock heading)
            {
                var style = heading.Inlines.OfType<TextRunNode>().Select(x => x.Style).FirstOrDefault();
                if (style is not null)
                {
                    return new TextSegmentStyle(
                        style.FamilyName,
                        style.Weight,
                        style.FontSize,
                        style.Underline,
                        style.CharacterSpacing,
                        style.WordSpacing,
                        (heading.Style ?? new ParagraphStyle()).LineHeight,
                        style.Italic,
                        style.ForegroundColor,
                        style.BackgroundColor);
                }
            }
        }

        return null;
    }

    private static bool IsVectorBulletMarker(string markerText)
        => string.Equals(markerText, "\u2022", StringComparison.Ordinal);

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

    private static void AppendDecorations(List<TextLayoutDecoration> target, IReadOnlyList<TextLayoutDecoration> source, double yOffset, double xOffset)
    {
        foreach (var decoration in source)
        {
            target.Add(OffsetDecoration(decoration, xOffset, yOffset));
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

    private static List<TextLayoutDecoration> ClipDecorations(double maxHeight, IReadOnlyList<TextLayoutDecoration> decorations)
        => decorations.Where(x => GetDecorationBottom(x) <= maxHeight + 0.0001d).ToList();

    private static void ApplyVerticalAlignment(
        RichTextBoxLayoutRequest request,
        List<TextLayoutLine> lines,
        List<TextLayoutDecoration> decorations,
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

    private static void OffsetDecorations(List<TextLayoutDecoration> decorations, double xOffset, double yOffset)
    {
        for (var i = 0; i < decorations.Count; i++)
        {
            decorations[i] = OffsetDecoration(decorations[i], xOffset, yOffset);
        }
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

    private static void ShiftLines(List<TextLayoutLine> lines, double xOffset, double yOffset)
        => OffsetLines(lines, xOffset, yOffset);

    private static void ShiftDecorations(List<TextLayoutDecoration> decorations, double xOffset, double yOffset)
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
        => request.Width - (request.BoxStyle.Inset * 2d);

    private static double GetContentHeight(RichTextBoxLayoutRequest request)
        => request.Height - (request.BoxStyle.Inset * 2d);

    private static double GetLineBottom(TextLayoutLine line)
        => line.BaselineY - line.BaselineOffset + line.Height;

    private static double GetLineWidth(IReadOnlyList<TextLayoutRun> runs)
        => runs.Count == 0 ? 0d : runs.Max(x => x.X + x.Width);

    private static double GetLineMeasuredWidth(IReadOnlyList<TextLayoutRun> runs)
        => runs.Count == 0 ? 0d : runs.Max(x => x.X + x.MeasuredWidth);

    private static double GetDecorationRight(TextLayoutDecoration decoration)
        => decoration switch
        {
            TextLayoutFillRectDecoration fill => fill.X + fill.Width,
            TextLayoutStrokeRectDecoration stroke => stroke.X + stroke.Width,
            TextLayoutLineDecoration line => Math.Max(line.X1, line.X2),
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

    private static double GetMaxRight(IReadOnlyList<TextLayoutLine> lines)
        => lines.Count == 0 ? 0d : lines.Max(x => x.Runs.Count == 0 ? x.MeasuredWidth : x.X + x.Runs.Max(r => r.X + r.MeasuredWidth));

    private static double GetMaxBottom(IReadOnlyList<TextLayoutLine> lines)
        => lines.Count == 0 ? 0d : lines.Max(GetLineBottom);

    private static double GetMaxRight(IReadOnlyList<TextLayoutDecoration> decorations)
        => decorations.Count == 0 ? 0d : decorations.Max(GetDecorationRight);

    private static double GetMaxBottom(IReadOnlyList<TextLayoutDecoration> decorations)
        => decorations.Count == 0 ? 0d : decorations.Max(GetDecorationBottom);

    private static double GetMaxRight(IReadOnlyList<TextLayoutLine> lines, IReadOnlyList<TextLayoutDecoration> decorations)
        => Math.Max(GetMaxRight(lines), GetMaxRight(decorations));

    private sealed class LayoutState
    {
        public List<TextLayoutLine> Lines { get; } = new();
        public List<TextLayoutDecoration> Decorations { get; } = new();
        public List<TextLayoutIssue> Issues { get; } = new();
        public double ConsumedHeight { get; set; }
        public int NextSegmentIndex { get; set; }
        public bool HadOverflow { get; set; }
    }

    private enum LayoutAxis
    {
        Horizontal,
        Vertical
    }

    private sealed record CellPlacement(
        TableCellBlock Cell,
        int RowIndex,
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
