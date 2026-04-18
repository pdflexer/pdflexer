using System.Buffers;
using System.Runtime.CompilerServices;

namespace PdfLexer.TextLayout;

internal readonly record struct RichMeasurementNodeDescriptor(
    string Path,
    string? NodeId,
    TextLayoutNodeKind Kind,
    int ChildStart,
    int ChildCount,
    int StartLineIndex,
    int EndLineIndexExclusive,
    double NaturalWidth,
    double NaturalHeight,
    double VisibleWidth,
    double VisibleHeight);

internal sealed class RichMeasurementGraph : IDisposable
{
    public required RichOwnedPooledBuffer<RichMeasurementNodeDescriptor> Nodes { get; init; }
    public required RichOwnedPooledBuffer<int> ChildIndices { get; init; }

    public bool TryGetNode(string path, out RichMeasurementNodeDescriptor descriptor)
    {
        for (var i = 0; i < Nodes.Count; i++)
        {
            var candidate = Nodes[i];
            if (string.Equals(candidate.Path, path, StringComparison.Ordinal))
            {
                descriptor = candidate;
                return true;
            }
        }

        descriptor = default;
        return false;
    }

    public void Dispose()
    {
        Nodes.Dispose();
        ChildIndices.Dispose();
    }
}

internal sealed class RichOwnedPooledBuffer<T> : IReadOnlyList<T>, IDisposable
{
    private T[]? _buffer;

    public RichOwnedPooledBuffer(T[] buffer, int count)
    {
        _buffer = buffer;
        Count = count;
    }

    public int Count { get; }

    public ReadOnlySpan<T> Span => (_buffer ?? System.Array.Empty<T>()).AsSpan(0, Count);

    public T this[int index] => Span[index];

    public void Dispose()
    {
        var buffer = _buffer;
        if (buffer is null)
        {
            return;
        }

        _buffer = null;
        ArrayPool<T>.Shared.Return(buffer, RuntimeHelpersEx.ShouldClear<T>());
    }

    public IEnumerator<T> GetEnumerator()
    {
        for (var i = 0; i < Count; i++)
        {
            yield return Span[i];
        }
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        => GetEnumerator();
}

internal sealed class RichPooledBufferBuilder<T> : IDisposable
{
    private T[] _buffer = Array.Empty<T>();

    public int Count { get; private set; }

    public void Add(T value)
    {
        EnsureCapacity(Count + 1);
        _buffer[Count++] = value;
    }

    public RichOwnedPooledBuffer<T> MoveToOwnedBuffer()
    {
        var buffer = _buffer;
        var count = Count;
        _buffer = Array.Empty<T>();
        Count = 0;
        return new RichOwnedPooledBuffer<T>(buffer, count);
    }

    private void EnsureCapacity(int capacity)
    {
        if (_buffer.Length >= capacity)
        {
            return;
        }

        var nextSize = Math.Max(capacity, _buffer.Length == 0 ? 8 : _buffer.Length * 2);
        var next = ArrayPool<T>.Shared.Rent(nextSize);
        if (Count > 0)
        {
            _buffer.AsSpan(0, Count).CopyTo(next);
        }

        if (_buffer.Length > 0)
        {
            ArrayPool<T>.Shared.Return(_buffer, RuntimeHelpersEx.ShouldClear<T>());
        }

        _buffer = next;
    }

    public void Dispose()
    {
        if (_buffer.Length > 0)
        {
            ArrayPool<T>.Shared.Return(_buffer, RuntimeHelpersEx.ShouldClear<T>());
        }

        _buffer = Array.Empty<T>();
        Count = 0;
    }
}

internal static class RichMeasurementGraphBuilder
{
    public static RichMeasurementGraph Build(IReadOnlyList<RichTextBlock> blocks, TextBoxLayoutResult layout, string pathPrefix)
    {
        using var nodeBuilder = new RichPooledBufferBuilder<RichMeasurementNodeDescriptor>();
        using var childBuilder = new RichPooledBufferBuilder<int>();

        for (var i = 0; i < blocks.Count; i++)
        {
            AppendBlock(blocks[i], GetRootBlockPath(pathPrefix, blocks.Count, i), layout, nodeBuilder, childBuilder);
        }

        return new RichMeasurementGraph
        {
            Nodes = nodeBuilder.MoveToOwnedBuffer(),
            ChildIndices = childBuilder.MoveToOwnedBuffer()
        };
    }

    private static string GetRootBlockPath(string pathPrefix, int blockCount, int index)
    {
        if (blockCount == 1 && IsConcreteBlockPath(pathPrefix))
        {
            return pathPrefix;
        }

        return $"{pathPrefix}[{index}]";
    }

    private static bool IsConcreteBlockPath(string pathPrefix)
        => pathPrefix.EndsWith("]", StringComparison.Ordinal);

    private static int AppendBlock(
        RichTextBlock block,
        string path,
        TextBoxLayoutResult layout,
        RichPooledBufferBuilder<RichMeasurementNodeDescriptor> nodeBuilder,
        RichPooledBufferBuilder<int> childBuilder)
    {
        var childStart = childBuilder.Count;
        switch (block)
        {
            case UnorderedListBlock unordered:
                AppendListItemNodes(unordered.Items, path, layout, childBuilder, nodeBuilder);
                break;
            case OrderedListBlock ordered:
                AppendListItemNodes(ordered.Items, path, layout, childBuilder, nodeBuilder);
                break;
            case TableBlock table:
                AppendTableNodes(table, path, layout, childBuilder, nodeBuilder);
                break;
            case RowBlock row:
                AppendLayoutChildNodes(row.Children, path, layout, childBuilder, nodeBuilder);
                break;
            case ColumnBlock column:
                AppendLayoutChildNodes(column.Children, path, layout, childBuilder, nodeBuilder);
                break;
        }

        var metrics = GetMetrics(layout, path);
        var nodeIndex = nodeBuilder.Count;
        nodeBuilder.Add(new RichMeasurementNodeDescriptor(
            path,
            block.Id.Value,
            GetNodeKind(block),
            childStart,
            childBuilder.Count - childStart,
            metrics.StartLineIndex,
            metrics.EndLineIndexExclusive,
            metrics.NaturalWidth,
            metrics.NaturalHeight,
            metrics.VisibleWidth,
            metrics.VisibleHeight));
        return nodeIndex;
    }

    private static void AppendListItemNodes(
        IReadOnlyList<ListItemBlock> items,
        string parentPath,
        TextBoxLayoutResult layout,
        RichPooledBufferBuilder<int> childBuilder,
        RichPooledBufferBuilder<RichMeasurementNodeDescriptor> nodeBuilder)
    {
        for (var itemIndex = 0; itemIndex < items.Count; itemIndex++)
        {
            var item = items[itemIndex];
            var itemPath = $"{parentPath}.Items[{itemIndex}]";
            var childStart = childBuilder.Count;
            for (var blockIndex = 0; blockIndex < item.Blocks.Count; blockIndex++)
            {
                childBuilder.Add(AppendBlock(item.Blocks[blockIndex], $"{itemPath}.Blocks[{blockIndex}]", layout, nodeBuilder, childBuilder));
            }

            AddNodeDescriptor(nodeBuilder, childStart, childBuilder.Count - childStart, itemPath, item.Id.Value, TextLayoutNodeKind.ListItem, layout);
            childBuilder.Add(nodeBuilder.Count - 1);
        }
    }

    private static void AppendTableNodes(
        TableBlock table,
        string parentPath,
        TextBoxLayoutResult layout,
        RichPooledBufferBuilder<int> childBuilder,
        RichPooledBufferBuilder<RichMeasurementNodeDescriptor> nodeBuilder)
    {
        for (var rowIndex = 0; rowIndex < table.Rows.Count; rowIndex++)
        {
            var row = table.Rows[rowIndex];
            var rowPath = $"{parentPath}.Rows[{rowIndex}]";
            var rowChildStart = childBuilder.Count;
            for (var cellIndex = 0; cellIndex < row.Cells.Count; cellIndex++)
            {
                var cell = row.Cells[cellIndex];
                var cellPath = $"{rowPath}.Cells[{cellIndex}]";
                var cellChildStart = childBuilder.Count;
                for (var blockIndex = 0; blockIndex < cell.Blocks.Count; blockIndex++)
                {
                    childBuilder.Add(AppendBlock(cell.Blocks[blockIndex], $"{cellPath}.Blocks[{blockIndex}]", layout, nodeBuilder, childBuilder));
                }

                AddNodeDescriptor(nodeBuilder, cellChildStart, childBuilder.Count - cellChildStart, cellPath, cell.Id.Value, TextLayoutNodeKind.TableCell, layout);
                childBuilder.Add(nodeBuilder.Count - 1);
            }

            AddNodeDescriptor(nodeBuilder, rowChildStart, childBuilder.Count - rowChildStart, rowPath, row.Id.Value, TextLayoutNodeKind.TableRow, layout);
            childBuilder.Add(nodeBuilder.Count - 1);
        }
    }

    private static void AppendLayoutChildNodes(
        IReadOnlyList<LayoutChild> children,
        string parentPath,
        TextBoxLayoutResult layout,
        RichPooledBufferBuilder<int> childBuilder,
        RichPooledBufferBuilder<RichMeasurementNodeDescriptor> nodeBuilder)
    {
        for (var childIndex = 0; childIndex < children.Count; childIndex++)
        {
            var child = children[childIndex];
            var childPath = $"{parentPath}.Children[{childIndex}]";
            var childStart = childBuilder.Count;
            for (var blockIndex = 0; blockIndex < child.Blocks.Count; blockIndex++)
            {
                childBuilder.Add(AppendBlock(child.Blocks[blockIndex], $"{childPath}.Blocks[{blockIndex}]", layout, nodeBuilder, childBuilder));
            }

            AddNodeDescriptor(nodeBuilder, childStart, childBuilder.Count - childStart, childPath, null, TextLayoutNodeKind.LayoutChild, layout);
            childBuilder.Add(nodeBuilder.Count - 1);
        }
    }

    private static void AddNodeDescriptor(
        RichPooledBufferBuilder<RichMeasurementNodeDescriptor> nodeBuilder,
        int childStart,
        int childCount,
        string path,
        string? nodeId,
        TextLayoutNodeKind kind,
        TextBoxLayoutResult layout)
    {
        var metrics = GetMetrics(layout, path);
        nodeBuilder.Add(new RichMeasurementNodeDescriptor(
            path,
            nodeId,
            kind,
            childStart,
            childCount,
            metrics.StartLineIndex,
            metrics.EndLineIndexExclusive,
            metrics.NaturalWidth,
            metrics.NaturalHeight,
            metrics.VisibleWidth,
            metrics.VisibleHeight));
    }

    private static (int StartLineIndex, int EndLineIndexExclusive, double NaturalWidth, double NaturalHeight, double VisibleWidth, double VisibleHeight) GetMetrics(TextBoxLayoutResult layout, string path)
    {
        int? start = null;
        int endExclusive = -1;
        var minLeft = double.PositiveInfinity;
        var minTop = double.PositiveInfinity;
        var maxNaturalRight = double.NegativeInfinity;
        var maxVisibleRight = double.NegativeInfinity;
        var maxBottom = double.NegativeInfinity;

        foreach (var line in layout.LineViews)
        {
            var hasMatch = false;
            foreach (var run in line.Runs)
            {
                if (run.SourcePath is not null && MatchesPath(path, run.SourcePath))
                {
                    hasMatch = true;
                    minLeft = Math.Min(minLeft, run.X);
                    var runTop = line.BaselineY - line.BaselineOffset;
                    minTop = Math.Min(minTop, runTop);
                    maxNaturalRight = Math.Max(maxNaturalRight, run.X + run.MeasuredWidth);
                    maxVisibleRight = Math.Max(maxVisibleRight, run.X + run.Width);
                    maxBottom = Math.Max(maxBottom, runTop + run.LineHeight);
                }
            }

            if (!hasMatch)
            {
                continue;
            }

            start ??= line.Index;
            endExclusive = line.Index + 1;
        }

        foreach (var decoration in layout.Decorations)
        {
            var sourcePath = decoration switch
            {
                TextLayoutFillRectDecoration fill => fill.SourcePath,
                TextLayoutStrokeRectDecoration stroke => stroke.SourcePath,
                TextLayoutLineDecoration line => line.SourcePath,
                _ => null
            };

            if (sourcePath is null || !MatchesPath(path, sourcePath))
            {
                continue;
            }

            var (left, top, right, bottom) = GetBounds(decoration);
            minLeft = Math.Min(minLeft, left);
            minTop = Math.Min(minTop, top);
            maxNaturalRight = Math.Max(maxNaturalRight, right);
            maxVisibleRight = Math.Max(maxVisibleRight, right);
            maxBottom = Math.Max(maxBottom, bottom);
        }

        if (!start.HasValue && double.IsPositiveInfinity(minLeft))
        {
            return (-1, -1, 0d, 0d, 0d, 0d);
        }

        var originLeft = double.IsPositiveInfinity(minLeft) ? 0d : minLeft;
        var originTop = double.IsPositiveInfinity(minTop) ? 0d : minTop;
        var naturalWidth = double.IsNegativeInfinity(maxNaturalRight) ? 0d : Math.Max(0d, maxNaturalRight - originLeft);
        var visibleWidth = double.IsNegativeInfinity(maxVisibleRight) ? 0d : Math.Max(0d, maxVisibleRight - originLeft);
        var naturalHeight = double.IsNegativeInfinity(maxBottom) ? 0d : Math.Max(0d, maxBottom - originTop);
        var visibleHeight = naturalHeight;
        return (start ?? -1, start.HasValue ? endExclusive : -1, naturalWidth, naturalHeight, visibleWidth, visibleHeight);
    }

    private static (double Left, double Top, double Right, double Bottom) GetBounds(TextLayoutDecoration decoration)
        => decoration switch
        {
            TextLayoutFillRectDecoration fill => (fill.X, fill.Y, fill.X + fill.Width, fill.Y + fill.Height),
            TextLayoutStrokeRectDecoration stroke => (stroke.X, stroke.Y, stroke.X + stroke.Width, stroke.Y + stroke.Height),
            TextLayoutLineDecoration line => (
                Math.Min(line.X1, line.X2),
                Math.Min(line.Y1, line.Y2),
                Math.Max(line.X1, line.X2),
                Math.Max(line.Y1, line.Y2)),
            _ => (0d, 0d, 0d, 0d)
        };

    public static bool MatchesPath(string path, string candidatePath)
    {
        if (string.Equals(path, candidatePath, StringComparison.Ordinal))
        {
            return true;
        }

        if (!candidatePath.StartsWith(path, StringComparison.Ordinal) || candidatePath.Length <= path.Length)
        {
            return false;
        }

        var next = candidatePath[path.Length];
        return next is '.' or '[';
    }

    private static TextLayoutNodeKind GetNodeKind(RichTextBlock block)
        => block switch
        {
            ParagraphBlock => TextLayoutNodeKind.Paragraph,
            HeadingBlock => TextLayoutNodeKind.Heading,
            UnorderedListBlock => TextLayoutNodeKind.UnorderedList,
            OrderedListBlock => TextLayoutNodeKind.OrderedList,
            TableBlock => TextLayoutNodeKind.Table,
            RowBlock => TextLayoutNodeKind.RowContainer,
            ColumnBlock => TextLayoutNodeKind.ColumnContainer,
            EmbeddedObjectBlock => TextLayoutNodeKind.EmbeddedObject,
            _ => TextLayoutNodeKind.Root
        };
}

internal static class RuntimeHelpersEx
{
    public static bool ShouldClear<T>()
        => RuntimeHelpers.IsReferenceOrContainsReferences<T>();
}
