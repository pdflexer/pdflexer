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

    public IStructureContext AddTable(string? title = null) => AddElement("Table", title);

    public IStructureContext AddRow() => AddElement("TR");

    public IStructureContext AddCell(bool header = false, int rowSpan = 1, int colSpan = 1)
    {
        var type = header ? "TH" : "TD";
        var ctx = (StructuralContext)AddElement(type);
        var node = ctx.GetNode();
        if (rowSpan > 1 || colSpan > 1)
        {
            var attr = new PdfDictionary { [PdfName.O] = PdfName.Table };
            if (rowSpan > 1) attr[PdfName.RowSpan] = new PdfIntNumber(rowSpan);
            if (colSpan > 1) attr[PdfName.ColSpan] = new PdfIntNumber(colSpan);
            node.Attributes.Add(attr);
        }
        return ctx;
    }

    public IStructureContext AddLayoutAttributes(string? textAlign = null, double? width = null, double? height = null)
    {
        var attr = new PdfDictionary { [PdfName.O] = PdfName.Layout };
        if (textAlign != null) attr[PdfName.TextAlign] = (PdfName)textAlign;
        if (width.HasValue) attr[PdfName.Width] = new PdfDoubleNumber(width.Value);
        if (height.HasValue) attr[PdfName.Height] = new PdfDoubleNumber(height.Value);
        _root.Attributes.Add(attr);
        return this;
    }

    public IStructureContext Back() => this;

    public StructureNode GetNode() => _root;

    public void CreateBookmark(string title, IOutlineContext outlineBuilder)
    {
        outlineBuilder.AddBookmark(title);
        outlineBuilder.LastNode.StructureElement = _root;
    }
}

public interface IStructureContext
{
    /// <summary>
    /// Adds a structural element of the specified type.
    /// </summary>
    IStructureContext AddElement(string type, string? title = null, string? id = null, string? language = null);

    /// <summary>
    /// Adds a paragraph (P) element.
    /// </summary>
    IStructureContext AddParagraph(string? title = null);

    /// <summary>
    /// Adds a header (H1-H6) element.
    /// </summary>
    IStructureContext AddHeader(int level, string? title = null);

    /// <summary>
    /// Adds a table (Table) element.
    /// </summary>
    IStructureContext AddTable(string? title = null);

    /// <summary>
    /// Adds a table row (TR) element.
    /// </summary>
    IStructureContext AddRow();

    /// <summary>
    /// Adds a table cell (TH or TD) element.
    /// </summary>
    IStructureContext AddCell(bool header = false, int rowSpan = 1, int colSpan = 1);

    /// <summary>
    /// Adds layout attributes to the current element.
    /// </summary>
    IStructureContext AddLayoutAttributes(string? textAlign = null, double? width = null, double? height = null);

    /// <summary>
    /// Moves back to the parent element context.
    /// </summary>
    IStructureContext Back();

    /// <summary>
    /// Gets the root node of the structure tree.
    /// </summary>
    StructureNode GetRoot();

    /// <summary>
    /// Gets the current structure node.
    /// </summary>
    StructureNode GetNode();

    /// <summary>
    /// Creates a bookmark linked to the current structure node.
    /// </summary>
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

    public IStructureContext AddTable(string? title = null) => AddElement("Table", title);

    public IStructureContext AddRow() => AddElement("TR");

    public IStructureContext AddCell(bool header = false, int rowSpan = 1, int colSpan = 1)
    {
        var type = header ? "TH" : "TD";
        var ctx = (StructuralContext)AddElement(type);
        var node = ctx.GetNode();
        if (rowSpan > 1 || colSpan > 1)
        {
            var attr = new PdfDictionary { [PdfName.O] = PdfName.Table };
            if (rowSpan > 1) attr[PdfName.RowSpan] = new PdfIntNumber(rowSpan);
            if (colSpan > 1) attr[PdfName.ColSpan] = new PdfIntNumber(colSpan);
            node.Attributes.Add(attr);
        }
        return ctx;
    }

    public IStructureContext AddLayoutAttributes(string? textAlign = null, double? width = null, double? height = null)
    {
        var attr = new PdfDictionary { [PdfName.O] = PdfName.Layout };
        if (textAlign != null) attr[PdfName.TextAlign] = (PdfName)textAlign;
        if (width.HasValue) attr[PdfName.Width] = new PdfDoubleNumber(width.Value);
        if (height.HasValue) attr[PdfName.Height] = new PdfDoubleNumber(height.Value);
        _node.Attributes.Add(attr);
        return this;
    }

    public IStructureContext Back() => _parent;

    public StructureNode GetRoot() => _builder.GetRoot();

    public StructureNode GetNode() => _node;

    public void CreateBookmark(string title, IOutlineContext outlineBuilder)
    {
        outlineBuilder.AddBookmark(title);
        outlineBuilder.LastNode.StructureElement = _node;
    }
}
