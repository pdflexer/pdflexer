using System.Collections.Generic;
using System.Linq;

namespace PdfLexer.DOM;

public class OutlineBuilder : IOutlineContext
{
    private readonly BookmarkNode _root = new BookmarkNode { Title = "ROOT" };

    public BookmarkNode GetRoot() => _root;

    public IOutlineContext AddSection(string title, bool isOpen = true, double[]? color = null, int? style = null)
    {
        var section = new BookmarkNode 
        { 
            Title = title,
            IsOpen = isOpen,
            Color = color,
            Style = style
        };
        _root.Children.Add(section);
        return new OutlineContext(this, section, this);
    }

    public IOutlineContext AddBookmark(string title, PdfPage? page = null, double[]? color = null, int? style = null)
    {
        var bookmark = new BookmarkNode 
        { 
            Title = title,
            Color = color,
            Style = style
        };
        if (page != null)
        {
            bookmark.Destination = page.NativeObject;
        }
        _root.Children.Add(bookmark);
        return this;
    }

    public IOutlineContext MoveToParent() => this;

    public BookmarkNode? FindNode(string title)
    {
        return FindNode(_root, title);
    }

    private BookmarkNode? FindNode(BookmarkNode parent, string title)
    {
        if (parent.Title == title) return parent;
        foreach (var child in parent.Children)
        {
            var found = FindNode(child, title);
            if (found != null) return found;
        }
        return null;
    }

    public IEnumerable<BookmarkNode> EnumerateLeaves()
    {
        return EnumerateLeaves(_root);
    }

    private IEnumerable<BookmarkNode> EnumerateLeaves(BookmarkNode parent)
    {
        if (parent.Children.Count == 0)
        {
            if (parent != _root) yield return parent;
            yield break;
        }
        foreach (var child in parent.Children)
        {
            foreach (var leaf in EnumerateLeaves(child))
            {
                yield return leaf;
            }
        }
    }
}

public interface IOutlineContext
{
    IOutlineContext AddSection(string title, bool isOpen = true, double[]? color = null, int? style = null);
    IOutlineContext AddBookmark(string title, PdfPage? page = null, double[]? color = null, int? style = null);
    IOutlineContext MoveToParent();
    BookmarkNode GetRoot();
    BookmarkNode? FindNode(string title);
    IEnumerable<BookmarkNode> EnumerateLeaves();
}

public class OutlineContext : IOutlineContext
{
    private readonly OutlineBuilder _builder;
    private readonly BookmarkNode _node;
    private readonly IOutlineContext _parent;

    internal OutlineContext(OutlineBuilder builder, BookmarkNode node, IOutlineContext parent)
    {
        _builder = builder;
        _node = node;
        _parent = parent;
    }

    public IOutlineContext AddSection(string title, bool isOpen = true, double[]? color = null, int? style = null)
    {
        var section = new BookmarkNode 
        { 
            Title = title,
            IsOpen = isOpen,
            Color = color,
            Style = style
        };
        _node.Children.Add(section);
        return new OutlineContext(_builder, section, this);
    }

    public IOutlineContext AddBookmark(string title, PdfPage? page = null, double[]? color = null, int? style = null)
    {
        var bookmark = new BookmarkNode 
        { 
            Title = title,
            Color = color,
            Style = style
        };
        if (page != null)
        {
            bookmark.Destination = page.NativeObject;
        }
        _node.Children.Add(bookmark);
        return this;
    }

    public IOutlineContext MoveToParent() => _parent;
    
    public BookmarkNode GetRoot() => _builder.GetRoot();

    public BookmarkNode? FindNode(string title) => _builder.FindNode(title);

    public IEnumerable<BookmarkNode> EnumerateLeaves() => _builder.EnumerateLeaves();
}