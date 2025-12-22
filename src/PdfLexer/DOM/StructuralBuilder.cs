using System.Collections.Generic;

namespace PdfLexer.DOM;

/// <summary>
/// A fluent builder for PDF logical structure (Tags).
/// </summary>
public class StructuralBuilder : IStructureContext
{
    private readonly StructureNode _root = new StructureNode { Type = "Document" };

    public StructureNode GetRoot() => _root;

    public IStructureContext AddElement(string type, string? title = null, string? id = null, string? language = null)
    {
        var node = new StructureNode 
        { 
            Type = type,
            Title = title,
            ID = id,
            Language = language
        };
        _root.Children.Add(node);
        return new StructuralContext(this, node, this);
    }

    public IStructureContext AddParagraph(string? title = null) => AddElement("P", title);
    public IStructureContext AddHeader(int level, string? title = null) => AddElement($"H{level}", title);

    public IStructureContext MoveToParent() => this;

    public StructureNode GetCurrentNode() => _root;

    public void CreateBookmark(string title, IOutlineContext outlineBuilder)
    {
        outlineBuilder.AddBookmark(title);
        outlineBuilder.LastNode.StructureElement = _root;
    }
}

public interface IStructureContext
{
    IStructureContext AddElement(string type, string? title = null, string? id = null, string? language = null);
    IStructureContext AddParagraph(string? title = null);
    IStructureContext AddHeader(int level, string? title = null);
    IStructureContext MoveToParent();
    StructureNode GetRoot();
    StructureNode GetCurrentNode();
    void CreateBookmark(string title, IOutlineContext outlineBuilder);
}

public class StructuralContext : IStructureContext
{
    private readonly StructuralBuilder _builder;
    private readonly StructureNode _node;
    private readonly IStructureContext _parent;

    internal StructuralContext(StructuralBuilder builder, StructureNode node, IStructureContext parent)
    {
        _builder = builder;
        _node = node;
        _parent = parent;
    }

    public IStructureContext AddElement(string type, string? title = null, string? id = null, string? language = null)
    {
        var node = new StructureNode 
        { 
            Type = type,
            Title = title,
            ID = id,
            Language = language
        };
        _node.Children.Add(node);
        return new StructuralContext(_builder, node, this);
    }

    public IStructureContext AddParagraph(string? title = null) => AddElement("P", title);
    public IStructureContext AddHeader(int level, string? title = null) => AddElement($"H{level}", title);

    public IStructureContext MoveToParent() => _parent;
    
    public StructureNode GetRoot() => _builder.GetRoot();

    public StructureNode GetCurrentNode() => _node;

    public void CreateBookmark(string title, IOutlineContext outlineBuilder)
    {
        outlineBuilder.AddBookmark(title);
        outlineBuilder.LastNode.StructureElement = _node;
    }
}
