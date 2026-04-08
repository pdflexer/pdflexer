namespace PdfLexer.TextLayout;

public sealed class RichSliceCursor
{
    private readonly List<(string Path, object Node)> _nodes;
    private int _index;

    internal RichSliceCursor(RichContentSlice slice)
    {
        _nodes = new List<(string Path, object Node)>();
        for (var i = 0; i < slice.Blocks.Count; i++)
        {
            AddBlock(slice.Blocks[i], $"Blocks[{i}]");
        }
    }

    public string? CurrentPath => _index >= 0 && _index < _nodes.Count ? _nodes[_index].Path : null;

    public object? CurrentNode => _index >= 0 && _index < _nodes.Count ? _nodes[_index].Node : null;

    public bool MoveNext()
    {
        if (_index + 1 >= _nodes.Count)
        {
            return false;
        }

        _index++;
        return true;
    }

    public bool MovePrevious()
    {
        if (_index <= 0)
        {
            return false;
        }

        _index--;
        return true;
    }

    public bool MoveToFirstChild()
    {
        var currentPath = CurrentPath;
        if (currentPath is null)
        {
            return false;
        }

        var prefix = currentPath + ".";
        for (var i = _index + 1; i < _nodes.Count; i++)
        {
            if (_nodes[i].Path.StartsWith(prefix, StringComparison.Ordinal))
            {
                _index = i;
                return true;
            }
        }

        return false;
    }

    public bool MoveToParent()
    {
        var currentPath = CurrentPath;
        if (string.IsNullOrEmpty(currentPath))
        {
            return false;
        }

        var dotIndex = currentPath.LastIndexOf('.');
        if (dotIndex <= 0)
        {
            return false;
        }

        var parentPath = currentPath[..dotIndex];
        for (var i = _index - 1; i >= 0; i--)
        {
            if (string.Equals(_nodes[i].Path, parentPath, StringComparison.Ordinal))
            {
                _index = i;
                return true;
            }
        }

        return false;
    }

    private void AddBlock(RichTextBlock block, string path)
    {
        _nodes.Add((path, block));
        switch (block)
        {
            case UnorderedListBlock unordered:
                for (var i = 0; i < unordered.Items.Count; i++)
                {
                    AddListItem(unordered.Items[i], $"{path}.Items[{i}]");
                }
                break;
            case OrderedListBlock ordered:
                for (var i = 0; i < ordered.Items.Count; i++)
                {
                    AddListItem(ordered.Items[i], $"{path}.Items[{i}]");
                }
                break;
            case TableBlock table:
                for (var rowIndex = 0; rowIndex < table.Rows.Count; rowIndex++)
                {
                    AddTableRow(table.Rows[rowIndex], $"{path}.Rows[{rowIndex}]");
                }
                break;
            case RowBlock row:
                for (var i = 0; i < row.Children.Count; i++)
                {
                    AddLayoutChild(row.Children[i], $"{path}.Children[{i}]");
                }
                break;
            case ColumnBlock column:
                for (var i = 0; i < column.Children.Count; i++)
                {
                    AddLayoutChild(column.Children[i], $"{path}.Children[{i}]");
                }
                break;
        }
    }

    private void AddListItem(ListItemBlock item, string path)
    {
        _nodes.Add((path, item));
        for (var i = 0; i < item.Blocks.Count; i++)
        {
            AddBlock(item.Blocks[i], $"{path}.Blocks[{i}]");
        }
    }

    private void AddLayoutChild(LayoutChild child, string path)
    {
        _nodes.Add((path, child));
        for (var i = 0; i < child.Blocks.Count; i++)
        {
            AddBlock(child.Blocks[i], $"{path}.Blocks[{i}]");
        }
    }

    private void AddTableRow(TableRowBlock row, string path)
    {
        _nodes.Add((path, row));
        for (var i = 0; i < row.Cells.Count; i++)
        {
            AddTableCell(row.Cells[i], $"{path}.Cells[{i}]");
        }
    }

    private void AddTableCell(TableCellBlock cell, string path)
    {
        _nodes.Add((path, cell));
        for (var i = 0; i < cell.Blocks.Count; i++)
        {
            AddBlock(cell.Blocks[i], $"{path}.Blocks[{i}]");
        }
    }
}
