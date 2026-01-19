using System.Collections.Generic;
using System.Numerics;
using PdfLexer.Writing;

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

    public IStructureContext WriteContent<T>(PageWriter<T> writer, Action<PageWriter<T>> action) where T : struct, IFloatingPoint<T>
    {
        writer.BeginMarkedContent(_root);
        action(writer);
        writer.EndMarkedContent();
        return this;
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

    public IStructureContext AddPart(string? title = null) => AddElement("Part", title);
    public IStructureContext AddSection(string? title = null) => AddElement("Sect", title);
    public IStructureContext AddDiv(string? title = null) => AddElement("Div", title);
    public IStructureContext AddBlockQuote(string? title = null) => AddElement("BlockQuote", title);
    public IStructureContext AddCaption(string? title = null) => AddElement("Caption", title);
    public IStructureContext AddTOC(string? title = null) => AddElement("TOC", title);
    public IStructureContext AddTOCI(string? title = null) => AddElement("TOCI", title);

    public IStructureContext AddList(string? title = null) => AddElement("L", title);
    public IStructureContext AddListItem(string? title = null) => AddElement("LI", title);
    public IStructureContext AddLabel(string? title = null) => AddElement("Lbl", title);
    public IStructureContext AddListBody(string? title = null) => AddElement("LBody", title);

    public IStructureContext AddTableHead(string? title = null) => AddElement("THead", title);
    public IStructureContext AddTableBody(string? title = null) => AddElement("TBody", title);
    public IStructureContext AddTableFoot(string? title = null) => AddElement("TFoot", title);
    public IStructureContext AddHeaderCell(int rowSpan = 1, int colSpan = 1) => AddCell(true, rowSpan, colSpan);
    public IStructureContext AddDataCell(int rowSpan = 1, int colSpan = 1) => AddCell(false, rowSpan, colSpan);

    public IStructureContext AddSpan(string? title = null, string? lang = null) => AddElement("Span", title, language: lang);
    public IStructureContext AddQuote(string? title = null) => AddElement("Quote", title);
    public IStructureContext AddCode(string? title = null) => AddElement("Code", title);
    
    public IStructureContext AddFormula(string? title = null, string? altText = null) 
    {
        var ctx = AddElement("Formula", title);
        if (altText != null) ctx.GetNode().AlternateText = altText;
        return ctx;
    }

    public IStructureContext AddReference(string? title = null) => AddElement("Reference", title);
    public IStructureContext AddNote(string? title = null) => AddElement("Note", title);
    
    public IStructureContext AddFigure(string? title = null, string? altText = null)
    {
        var ctx = AddElement("Figure", title);
        if (altText != null) ctx.GetNode().AlternateText = altText;
        return ctx;
    }

    public IStructureContext AddLink(string? title = null, string? altText = null)
    {
        var ctx = AddElement("Link", title);
        if (altText != null) ctx.GetNode().AlternateText = altText;
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
    /// Writes content to the page writer wrapped in the current structure element's MCID context.
    /// </summary>
    IStructureContext WriteContent<T>(PageWriter<T> writer, Action<PageWriter<T>> action) where T : struct, IFloatingPoint<T>;

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
    /// Adds a Part element.
    /// </summary>
    IStructureContext AddPart(string? title = null);

    /// <summary>
    /// Adds a Section (Sect) element.
    /// </summary>
    IStructureContext AddSection(string? title = null);

    /// <summary>
    /// Adds a Div element.
    /// </summary>
    IStructureContext AddDiv(string? title = null);

    /// <summary>
    /// Adds a BlockQuote element.
    /// </summary>
    IStructureContext AddBlockQuote(string? title = null);

    /// <summary>
    /// Adds a Caption element.
    /// </summary>
    IStructureContext AddCaption(string? title = null);

    /// <summary>
    /// Adds a TOC element.
    /// </summary>
    IStructureContext AddTOC(string? title = null);

    /// <summary>
    /// Adds a TOCI element.
    /// </summary>
    IStructureContext AddTOCI(string? title = null);

    /// <summary>
    /// Adds a List (L) element.
    /// </summary>
    IStructureContext AddList(string? title = null);

    /// <summary>
    /// Adds a ListItem (LI) element.
    /// </summary>
    IStructureContext AddListItem(string? title = null);

    /// <summary>
    /// Adds a Label (Lbl) element.
    /// </summary>
    IStructureContext AddLabel(string? title = null);

    /// <summary>
    /// Adds a ListBody (LBody) element.
    /// </summary>
    IStructureContext AddListBody(string? title = null);

    /// <summary>
    /// Adds a TableHead (THead) element.
    /// </summary>
    IStructureContext AddTableHead(string? title = null);

    /// <summary>
    /// Adds a TableBody (TBody) element.
    /// </summary>
    IStructureContext AddTableBody(string? title = null);

    /// <summary>
    /// Adds a TableFoot (TFoot) element.
    /// </summary>
    IStructureContext AddTableFoot(string? title = null);

    /// <summary>
    /// Adds a Header Cell (TH) element.
    /// </summary>
    IStructureContext AddHeaderCell(int rowSpan = 1, int colSpan = 1);

    /// <summary>
    /// Adds a Data Cell (TD) element.
    /// </summary>
    IStructureContext AddDataCell(int rowSpan = 1, int colSpan = 1);

    /// <summary>
    /// Adds a Span element.
    /// </summary>
    IStructureContext AddSpan(string? title = null, string? lang = null);

    /// <summary>
    /// Adds a Quote element.
    /// </summary>
    IStructureContext AddQuote(string? title = null);

    /// <summary>
    /// Adds a Code element.
    /// </summary>
    IStructureContext AddCode(string? title = null);

    /// <summary>
    /// Adds a Formula element.
    /// </summary>
    IStructureContext AddFormula(string? title = null, string? altText = null);

    /// <summary>
    /// Adds a Reference element.
    /// </summary>
    IStructureContext AddReference(string? title = null);

    /// <summary>
    /// Adds a Note element.
    /// </summary>
    IStructureContext AddNote(string? title = null);

    /// <summary>
    /// Adds a Figure element.
    /// </summary>
    IStructureContext AddFigure(string? title = null, string? altText = null);

    /// <summary>
    /// Adds a Link element.
    /// </summary>
    IStructureContext AddLink(string? title = null, string? altText = null);

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

    public IStructureContext WriteContent<T>(PageWriter<T> writer, Action<PageWriter<T>> action) where T : struct, IFloatingPoint<T>
    {
        writer.BeginMarkedContent(_node);
        action(writer);
        writer.EndMarkedContent();
        return this;
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

    public IStructureContext AddPart(string? title = null) => AddElement("Part", title);
    public IStructureContext AddSection(string? title = null) => AddElement("Sect", title);
    public IStructureContext AddDiv(string? title = null) => AddElement("Div", title);
    public IStructureContext AddBlockQuote(string? title = null) => AddElement("BlockQuote", title);
    public IStructureContext AddCaption(string? title = null) => AddElement("Caption", title);
    public IStructureContext AddTOC(string? title = null) => AddElement("TOC", title);
    public IStructureContext AddTOCI(string? title = null) => AddElement("TOCI", title);

    public IStructureContext AddList(string? title = null) => AddElement("L", title);
    public IStructureContext AddListItem(string? title = null) => AddElement("LI", title);
    public IStructureContext AddLabel(string? title = null) => AddElement("Lbl", title);
    public IStructureContext AddListBody(string? title = null) => AddElement("LBody", title);

    public IStructureContext AddTableHead(string? title = null) => AddElement("THead", title);
    public IStructureContext AddTableBody(string? title = null) => AddElement("TBody", title);
    public IStructureContext AddTableFoot(string? title = null) => AddElement("TFoot", title);
    public IStructureContext AddHeaderCell(int rowSpan = 1, int colSpan = 1) => AddCell(true, rowSpan, colSpan);
    public IStructureContext AddDataCell(int rowSpan = 1, int colSpan = 1) => AddCell(false, rowSpan, colSpan);

    public IStructureContext AddSpan(string? title = null, string? lang = null) => AddElement("Span", title, language: lang);
    public IStructureContext AddQuote(string? title = null) => AddElement("Quote", title);
    public IStructureContext AddCode(string? title = null) => AddElement("Code", title);
    
    public IStructureContext AddFormula(string? title = null, string? altText = null) 
    {
        var ctx = AddElement("Formula", title);
        if (altText != null) ctx.GetNode().AlternateText = altText;
        return ctx;
    }

    public IStructureContext AddReference(string? title = null) => AddElement("Reference", title);
    public IStructureContext AddNote(string? title = null) => AddElement("Note", title);
    
    public IStructureContext AddFigure(string? title = null, string? altText = null)
    {
        var ctx = AddElement("Figure", title);
        if (altText != null) ctx.GetNode().AlternateText = altText;
        return ctx;
    }

    public IStructureContext AddLink(string? title = null, string? altText = null)
    {
        var ctx = AddElement("Link", title);
        if (altText != null) ctx.GetNode().AlternateText = altText;
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
