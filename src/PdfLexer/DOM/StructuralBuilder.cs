using System.Collections.Generic;
using System.Numerics;
using PdfLexer.Content;
using PdfLexer.Writing;

namespace PdfLexer.DOM;

/// <summary>
/// A fluent builder for PDF logical structure (Tags).
/// </summary>
/// <remarks>
/// This builder is intended for authoring structure in new documents and in existing documents that are currently
/// untagged. It is not a remediation API for editing a pre-existing <c>StructTreeRoot</c>. See
/// <c>docs/accessibility-authoring.md</c> for the supported workflow.
/// </remarks>
public class StructuralBuilder : IStructureContext
{
    private readonly StructureRoot _structureRoot = new();
    private readonly StructureNode _root;
    private readonly Dictionary<PdfStream, StructureNode> _boundForms = new();

    public StructuralBuilder()
    {
        _root = _structureRoot.Document;
    }

    public StructureNode GetRoot() => _root;

    public StructureRoot GetStructureRoot() => _structureRoot;

    public IStructureContext AddElement(string type, string? title = null, string? id = null, string? language = null)
    {
        var node = new StructureNode
        {
            Type = type,
            Title = title,
            ID = id,
            Language = language
        };
        _structureRoot.AttachChild(_root, node);
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
        var ctx = AddElement(type);
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

    public IStructureContext AddDocumentTitle(string? title = null) => AddElement("Title", title);

    public IStructureContext AddFormula(string? title = null, string? altText = null)
    {
        var ctx = AddElement("Formula", title);
        if (altText != null) { ctx.Alt(altText); }
        return ctx;
    }

    public IStructureContext AddReference(string? title = null) => AddElement("Reference", title);
    public IStructureContext AddNote(string? title = null) => AddElement("Note", title);
    public IStructureContext AddFENote(string? title = null) => AddElement("FENote", title);

    public IStructureContext AddFigure(string? title = null, string? altText = null)
    {
        var ctx = AddElement("Figure", title);
        if (altText != null) { ctx.Alt(altText); }
        return ctx;
    }

    public IStructureContext AddLink(string? title = null, string? altText = null)
    {
        var ctx = AddElement("Link", title);
        if (altText != null) { ctx.Alt(altText); }
        return ctx;
    }

    public IStructureContext AddLink(LinkAnnotation annotation, string? title = null, string? altText = null)
    {
        var ctx = AddLink(title, altText);
        BindLinkAnnotation(ctx.GetNode(), annotation, title, altText);
        return ctx;
    }

    public IStructureContext AddLink(PdfPage page, PdfRect<double> rect, IPdfObject destination, string? title = null, string? altText = null)
    {
        return AddLink(AnnotationFactory.CreateLink(page, rect, destination, title ?? altText), title, altText);
    }

    public IStructureContext AddLink(
        PdfPage page,
        PdfRect<double> rect,
        StructureNode destination,
        string accessibleDescription,
        string? title = null,
        string? altText = null)
    {
        return AddLink(
            AnnotationFactory.CreateStructureLink(page, rect, destination, accessibleDescription),
            title ?? accessibleDescription,
            altText ?? accessibleDescription);
    }

    public IStructureContext AddLinkAction(PdfPage page, PdfRect<double> rect, PdfDictionary action, string? title = null, string? altText = null)
    {
        return AddLink(AnnotationFactory.CreateLinkAction(page, rect, action, title ?? altText), title, altText);
    }

    public IStructureContext AddFormField(WidgetAnnotation widget, string? title = null)
    {
        var ctx = AddElement("Form", title);
        ApplyAnnotationDescription(widget.NativeObject, widget.Field,
            title ?? widget.Field.Get<PdfString>((PdfName)"TU")?.Value);
        BindAnnotation(ctx.GetNode(), widget.Page, widget.NativeObject);
        return ctx;
    }

    public IStructureContext AddFormField(PdfDocument document, PdfPage page, PdfRect<double> rect, string fieldName, string? title = null, string? tooltip = null, bool print = true)
    {
        return AddFormField(AnnotationFactory.CreateTextWidget(document, page, rect, fieldName, tooltip, print), title);
    }

    public IStructureContext AddFormField(PdfDocument document, PdfPage page, PdfRect<double> rect, string fieldName, FormFieldAppearanceOptions appearance, string? value = null, string? title = null, string? tooltip = null, bool print = true)
    {
        return AddFormField(AnnotationFactory.CreateTextWidget(document, page, rect, fieldName, appearance, value, tooltip, print), title);
    }

    public IStructureContext AddLabeledFormField(WidgetAnnotation widget, string? title = null, string? tooltip = null)
    {
        var formCtx = AddElement("Form", title);
        var lblCtx = formCtx.AddElement("Lbl");
        var node = formCtx.GetNode();
        var widgetObj = widget.NativeObject;
        
        // Requirement 2.3: Consistency for /Contents and /TU
        ApplyAnnotationDescription(widgetObj, widget.Field, tooltip ?? title, overwriteFieldTooltip: tooltip != null);

        BindAnnotation(node, widget.Page, widgetObj);
        return lblCtx;
    }

    public IStructureContext BindImage(XObjImage image, params PdfPage[] pages)
    {
        BindImage(_root, image, pages);
        return this;
    }

    public IStructureContext BindFormXObject(XObjForm form, params PdfPage[] pages)
    {
        BindFormXObject(_root, form, pages);
        return this;
    }

    public IStructureContext BindMarkedContent(PdfPage page, int mcid)
    {
        BindMarkedContent(_root, page, mcid);
        return this;
    }

    public IStructureContext BindAnnotation(PdfDictionary annotation, PdfPage page)
    {
        BindAnnotation(_root, page, annotation);
        return this;
    }

    public IStructureContext AddAnnot(PdfPage page, PdfDictionary annotation, string? title = null, string? altText = null)
    {
        EnsureAnnotationOnPage(page, annotation, addWhenMissing: true);
        ApplyAnnotationDescription(annotation, null, altText ?? title);
        var ctx = AddElement("Annot", title);
        if (altText != null) ctx.Alt(altText);
        BindAnnotation(ctx.GetNode(), page, annotation);
        return ctx;
    }

    public IStructureContext AddLayoutAttributes(string? textAlign = null, double? width = null, double? height = null)
    {
        var attr = new PdfDictionary { [PdfName.O] = PdfName.Layout };
        if (textAlign != null) attr[PdfName.TextAlign] = (PdfName)textAlign;
        if (width.HasValue) attr[PdfName.Width] = new PdfDoubleNumber(width.Value);
        if (height.HasValue) attr[PdfName.Height] = new PdfDoubleNumber(height.Value);
        if (attr.Count > 1) _root.Attributes.Add(attr);
        return this;
    }

    public IStructureContext Alt(string? altText)
    {
        _root.Alt = altText;
        return this;
    }

    public IStructureContext ActualText(string? actualText)
    {
        _root.ActualText = actualText;
        return this;
    }

    public IStructureContext Expansion(string? expansion)
    {
        _root.Expansion = expansion;
        return this;
    }

    public IStructureContext Lang(string? language)
    {
        _root.Language = language;
        return this;
    }

    public IStructureContext ElementId(string? id)
    {
        _root.ID = id;
        return this;
    }

    public IStructureContext References(params string[] ids)
    {
        AddUnique(_root.References, ids);
        return this;
    }

    public IStructureContext References(params IStructureContext[] targets)
    {
        AddTargets(_root.References, targets);
        return this;
    }

    public IStructureContext AddClass(string className)
    {
        AddUnique(_root.Classes, className);
        return this;
    }

    public IStructureContext SetNamespace(StructureNamespace? ns)
    {
        _root.Namespace = ns;
        return this;
    }

    public IStructureContext TableScope(StructureScope? scope)
    {
        _root.Scope = scope;
        return this;
    }

    public IStructureContext TableHeaders(params string[] ids)
    {
        AddUnique(_root.Headers, ids);
        return this;
    }

    public IStructureContext TableHeaders(params IStructureContext[] targets)
    {
        AddTargets(_root.Headers, targets);
        return this;
    }

    public IStructureContext TableSummary(string? summary)
    {
        _root.Summary = summary;
        return this;
    }

    public IStructureContext ListNumbering(StructureListNumbering? numbering)
    {
        _root.ListNumbering = numbering;
        return this;
    }

    public IStructureContext Ref(StructureNode target)
    {
        if (string.IsNullOrEmpty(target.ID))
        {
            target.ID = Guid.NewGuid().ToString("N");
        }
        AddUnique(_root.References, target.ID);
        return this;
    }

    public StructuralBuilder MapRole(string customRole, string standardRole)
    {
        _structureRoot.MapRole(customRole, standardRole);
        return this;
    }

    public StructuralBuilder AddClassDefinition(string className, PdfDictionary attributes)
    {
        _structureRoot.AddClass(className, attributes);
        return this;
    }

    public StructuralBuilder AddClassDefinition(string className, PdfArray attributes)
    {
        _structureRoot.AddClass(className, attributes);
        return this;
    }

    public StructureNamespace DeclareNamespace(string uri) => _structureRoot.DeclareNamespace(uri);

    public IStructureContext Back() => this;

    public StructureNode GetNode() => _root;

    public void CreateBookmark(string title, IOutlineContext outlineBuilder)
    {
        outlineBuilder.AddBookmark(title);
        outlineBuilder.LastNode.StructureElement = _root;
    }

    private static void AddUnique(List<string> values, params string[] additions)
    {
        foreach (var addition in additions)
        {
            if (string.IsNullOrWhiteSpace(addition) || values.Contains(addition))
            {
                continue;
            }

            values.Add(addition);
        }
    }

    internal void BindAnnotation(StructureNode node, PdfPage page, PdfDictionary annotation)
    {
        EnsureAnnotationOnPage(page, annotation, addWhenMissing: false);
        if (node.ObjectReferences.Any(x => ReferenceEquals(x.Object.Resolve(), annotation)))
        {
            return;
        }
        var index = _structureRoot.AllocateStructParentIndex();
        node.ObjectReferences.Add(new StructureObjectReference(annotation, index, page));
    }

    private void AddTargets(List<string> destination, IStructureContext[] targets)
    {
        ArgumentNullException.ThrowIfNull(targets);
        foreach (var target in targets)
        {
            ArgumentNullException.ThrowIfNull(target);
            var node = target.GetNode();
            if (!ReferenceEquals(node.Root, _structureRoot))
            {
                throw new ArgumentException("Referenced structure context belongs to another structure tree.", nameof(targets));
            }
            AddUnique(destination, _structureRoot.EnsureNodeId(node));
        }
    }

    internal static void EnsureAnnotationOnPage(PdfPage page, PdfDictionary annotation, bool addWhenMissing)
    {
        ArgumentNullException.ThrowIfNull(page);
        ArgumentNullException.ThrowIfNull(annotation);
        var annots = page.NativeObject.Get<PdfArray>(PdfName.Annots);
        var present = annots?.Any(x => ReferenceEquals(x.Resolve(), annotation)) == true;
        if (!present && addWhenMissing)
        {
            page.AddAnnotation(annotation);
            present = true;
        }
        if (!present)
        {
            throw new ArgumentException("The annotation does not belong to the supplied page.", nameof(annotation));
        }

        var owner = annotation.Get(PdfName.P)?.Resolve();
        if (owner != null && !ReferenceEquals(owner, page.NativeObject))
        {
            throw new ArgumentException("The annotation belongs to a different page.", nameof(annotation));
        }
        annotation[PdfName.P] = page.NativeObject.Indirect();
    }

    /// <param name="overwriteFieldTooltip">
    /// True when <paramref name="description"/> came from an explicit tooltip argument, which is a
    /// direct request to set the field tooltip. False when it is a fallback from a structure title,
    /// in which case a field tooltip already set by the annotation factory is left alone — a field
    /// tooltip is shared by every kid of a radio group and must not be redefined per widget.
    /// </param>
    internal static void ApplyAnnotationDescription(
        PdfDictionary annotation,
        PdfDictionary? field,
        string? description,
        bool overwriteFieldTooltip = false)
    {
        description ??= field?.Get<PdfString>((PdfName)"TU")?.Value
            ?? annotation.Get<PdfString>((PdfName)"TU")?.Value
            ?? annotation.Get<PdfString>(PdfName.Contents)?.Value;
        if (string.IsNullOrWhiteSpace(description))
        {
            return;
        }

        var text = PdfString.CreateTextString(description);
        annotation[PdfName.Contents] = text;
        if (annotation.Get<PdfName>(PdfName.Subtype) == PdfName.Widget)
        {
            annotation[(PdfName)"TU"] = text;
            if (field != null && (overwriteFieldTooltip || field.Get<PdfString>((PdfName)"TU") == null))
            {
                field[(PdfName)"TU"] = text;
            }
        }
    }

    internal void BindLinkAnnotation(StructureNode node, LinkAnnotation annotation, string? title = null, string? altText = null)
    {
        var index = _structureRoot.AllocateStructParentIndex();
        node.ObjectReferences.Add(new StructureObjectReference(annotation.NativeObject, index, annotation.Page)
        {
            AnnotationContents = annotation.NativeObject.Get<PdfString>(PdfName.Contents)?.Value ?? title ?? altText,
            StructureDestinationTarget = annotation.StructureDestinationTarget,
            StructureDestinationTemplate = annotation.StructureDestinationTemplate
        });
    }

    internal void BindImage(StructureNode node, XObjImage image, params PdfPage[] pages)
    {
        var index = _structureRoot.AllocateStructParentIndex();
        image.StructParent = new PdfIntNumber(index);
        node.ObjectReferences.Add(new StructureObjectReference(image.Stream, index, pages));
    }

    internal void BindFormXObject(StructureNode node, XObjForm form, params PdfPage[] pages)
    {
        var existing = node.XObjectReferences.FirstOrDefault(x => ReferenceEquals(x.XObject.Resolve(), form.NativeObject));
        if (existing != null)
        {
            foreach (var page in pages)
            {
                if (!existing.Pages.Contains(page))
                {
                    existing.Pages.Add(page);
                }
            }
            form.StructParents = new PdfIntNumber(existing.StructParentsIndex);
            return;
        }

        // A form XObject carries a single /StructParents, so it can only point at one ParentTree
        // entry. Binding it under a second element would silently orphan the first entry.
        if (_boundForms.TryGetValue(form.NativeObject, out var owner) && !ReferenceEquals(owner, node))
        {
            throw new PdfAccessibilityConformanceException(
                $"Form XObject is already bound to structure element '{owner.Type}'. A form XObject can only be bound to one structure element.");
        }

        var index = _structureRoot.AllocateStructParentIndex();
        form.StructParents = new PdfIntNumber(index);
        node.XObjectReferences.Add(new StructureXObjectReference(form.NativeObject, index, pages));
        _boundForms[form.NativeObject] = node;
    }

    internal void BindMarkedContent(StructureNode node, PdfPage page, int mcid)
    {
        node.ContentItems.Add((page, mcid));
    }

    public void ReparentStructureNode(StructureNode node, StructureNode newParent)
    {
        var oldParent = node.Parent;
        if (oldParent != null)
        {
            oldParent.Children.Remove(node);
        }

        newParent.Children.Add(node);
        node.Parent = newParent;
        if (newParent.Root != null)
        {
            newParent.Root.AttachSubtree(node);
        }
    }

    internal void FlattenLeafStructureNodeInto(StructureNode leaf, StructureNode target)
    {
        if (leaf.Children.Count > 0 || leaf.ObjectReferences.Count > 0 || leaf.XObjectReferences.Count > 0 || leaf.XObjectContentItems.Count > 0)
        {
            throw new InvalidOperationException("Only leaf structure nodes with page marked-content references can be flattened.");
        }

        target.ContentItems.AddRange(leaf.ContentItems);
        leaf.ContentItems.Clear();

        var oldParent = leaf.Parent;
        if (oldParent != null)
        {
            oldParent.Children.Remove(leaf);
        }

        leaf.Parent = null;
    }

    public void InsertParentAroundStructureNode(StructureNode child, StructureNode newParent)
    {
        var oldParent = child.Parent;
        if (oldParent == null)
        {
            return;
        }

        var index = oldParent.Children.IndexOf(child);
        if (index < 0)
        {
            return;
        }

        newParent.Parent?.Children.Remove(newParent);
        oldParent.Children[index] = newParent;
        newParent.Parent = oldParent;

        child.Parent = newParent;
        if (!newParent.Children.Contains(child))
        {
            newParent.Children.Add(child);
        }

        if (oldParent.Root != null)
        {
            oldParent.Root.AttachSubtree(newParent);
        }
    }
}

public interface IStructureContext
{
    IStructureContext WriteContent<T>(PageWriter<T> writer, Action<PageWriter<T>> action) where T : struct, IFloatingPoint<T>;
    IStructureContext AddElement(string type, string? title = null, string? id = null, string? language = null);
    IStructureContext AddParagraph(string? title = null);
    IStructureContext AddHeader(int level, string? title = null);
    IStructureContext AddTable(string? title = null);
    IStructureContext AddRow();
    IStructureContext AddCell(bool header = false, int rowSpan = 1, int colSpan = 1);
    IStructureContext AddPart(string? title = null);
    IStructureContext AddSection(string? title = null);
    IStructureContext AddDiv(string? title = null);
    IStructureContext AddBlockQuote(string? title = null);
    IStructureContext AddCaption(string? title = null);
    IStructureContext AddTOC(string? title = null);
    IStructureContext AddTOCI(string? title = null);
    IStructureContext AddList(string? title = null);
    IStructureContext AddListItem(string? title = null);
    IStructureContext AddLabel(string? title = null);
    IStructureContext AddListBody(string? title = null);
    IStructureContext AddTableHead(string? title = null);
    IStructureContext AddTableBody(string? title = null);
    IStructureContext AddTableFoot(string? title = null);
    IStructureContext AddHeaderCell(int rowSpan = 1, int colSpan = 1);
    IStructureContext AddDataCell(int rowSpan = 1, int colSpan = 1);
    IStructureContext AddSpan(string? title = null, string? lang = null);
    IStructureContext AddQuote(string? title = null);
    IStructureContext AddCode(string? title = null);
    IStructureContext AddDocumentTitle(string? title = null);
    IStructureContext AddFormula(string? title = null, string? altText = null);
    IStructureContext AddReference(string? title = null);
    IStructureContext AddNote(string? title = null);
    IStructureContext AddFENote(string? title = null);
    IStructureContext AddFigure(string? title = null, string? altText = null);
    IStructureContext AddLink(string? title = null, string? altText = null);
    IStructureContext AddLink(LinkAnnotation annotation, string? title = null, string? altText = null);
    IStructureContext AddLink(PdfPage page, PdfRect<double> rect, IPdfObject destination, string? title = null, string? altText = null);
    IStructureContext AddLink(PdfPage page, PdfRect<double> rect, StructureNode destination, string accessibleDescription, string? title = null, string? altText = null);
    IStructureContext AddLinkAction(PdfPage page, PdfRect<double> rect, PdfDictionary action, string? title = null, string? altText = null);
    IStructureContext AddFormField(WidgetAnnotation widget, string? title = null);
    IStructureContext AddFormField(PdfDocument document, PdfPage page, PdfRect<double> rect, string fieldName, string? title = null, string? tooltip = null, bool print = true);
    IStructureContext AddFormField(PdfDocument document, PdfPage page, PdfRect<double> rect, string fieldName, FormFieldAppearanceOptions appearance, string? value = null, string? title = null, string? tooltip = null, bool print = true);
    IStructureContext AddLabeledFormField(WidgetAnnotation widget, string? title = null, string? tooltip = null);
    IStructureContext BindImage(XObjImage image, params PdfPage[] pages);
    IStructureContext BindFormXObject(XObjForm form, params PdfPage[] pages);
    IStructureContext BindMarkedContent(PdfPage page, int mcid);
    IStructureContext BindAnnotation(PdfDictionary annotation, PdfPage page);
    IStructureContext AddAnnot(PdfPage page, PdfDictionary annotation, string? title = null, string? altText = null);
    IStructureContext AddLayoutAttributes(string? textAlign = null, double? width = null, double? height = null);
    IStructureContext Alt(string? altText);
    IStructureContext ActualText(string? actualText);
    IStructureContext Expansion(string? expansion);
    IStructureContext Lang(string? language);
    IStructureContext ElementId(string? id);
    IStructureContext References(params string[] ids);
    IStructureContext References(params IStructureContext[] targets);
    IStructureContext AddClass(string className);
    IStructureContext SetNamespace(StructureNamespace? ns);
    IStructureContext TableScope(StructureScope? scope);
    IStructureContext TableHeaders(params string[] ids);
    IStructureContext TableHeaders(params IStructureContext[] targets);
    IStructureContext TableSummary(string? summary);
    IStructureContext ListNumbering(StructureListNumbering? numbering);
    IStructureContext Ref(StructureNode target);
    IStructureContext Back();
    StructureNode GetRoot();
    StructureNode GetNode();
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
        _builder.GetStructureRoot().AttachChild(_node, node);
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
        var ctx = AddElement(type);
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

    public IStructureContext AddDocumentTitle(string? title = null) => AddElement("Title", title);

    public IStructureContext AddFormula(string? title = null, string? altText = null)
    {
        var ctx = AddElement("Formula", title);
        if (altText != null) { ctx.Alt(altText); }
        return ctx;
    }

    public IStructureContext AddReference(string? title = null) => AddElement("Reference", title);
    public IStructureContext AddNote(string? title = null) => AddElement("Note", title);
    public IStructureContext AddFENote(string? title = null) => AddElement("FENote", title);

    public IStructureContext AddFigure(string? title = null, string? altText = null)
    {
        var ctx = AddElement("Figure", title);
        if (altText != null) { ctx.Alt(altText); }
        return ctx;
    }

    public IStructureContext AddLink(string? title = null, string? altText = null)
    {
        var ctx = AddElement("Link", title);
        if (altText != null) { ctx.Alt(altText); }
        return ctx;
    }

    public IStructureContext AddLink(LinkAnnotation annotation, string? title = null, string? altText = null)
    {
        var ctx = AddLink(title, altText);
        _builder.BindLinkAnnotation(ctx.GetNode(), annotation, title, altText);
        return ctx;
    }

    public IStructureContext AddLink(PdfPage page, PdfRect<double> rect, IPdfObject destination, string? title = null, string? altText = null)
    {
        return AddLink(AnnotationFactory.CreateLink(page, rect, destination, title ?? altText), title, altText);
    }

    public IStructureContext AddLink(
        PdfPage page,
        PdfRect<double> rect,
        StructureNode destination,
        string accessibleDescription,
        string? title = null,
        string? altText = null)
    {
        return AddLink(
            AnnotationFactory.CreateStructureLink(page, rect, destination, accessibleDescription),
            title ?? accessibleDescription,
            altText ?? accessibleDescription);
    }

    public IStructureContext AddLinkAction(PdfPage page, PdfRect<double> rect, PdfDictionary action, string? title = null, string? altText = null)
    {
        return AddLink(AnnotationFactory.CreateLinkAction(page, rect, action, title ?? altText), title, altText);
    }

    public IStructureContext AddFormField(WidgetAnnotation widget, string? title = null)
    {
        var ctx = AddElement("Form", title);
        ApplyAnnotationDescription(widget.NativeObject, widget.Field,
            title ?? widget.Field.Get<PdfString>((PdfName)"TU")?.Value);
        _builder.BindAnnotation(ctx.GetNode(), widget.Page, widget.NativeObject);
        return ctx;
    }

    public IStructureContext AddFormField(PdfDocument document, PdfPage page, PdfRect<double> rect, string fieldName, string? title = null, string? tooltip = null, bool print = true)
    {
        return AddFormField(AnnotationFactory.CreateTextWidget(document, page, rect, fieldName, tooltip, print), title);
    }

    public IStructureContext AddFormField(PdfDocument document, PdfPage page, PdfRect<double> rect, string fieldName, FormFieldAppearanceOptions appearance, string? value = null, string? title = null, string? tooltip = null, bool print = true)
    {
        return AddFormField(AnnotationFactory.CreateTextWidget(document, page, rect, fieldName, appearance, value, tooltip, print), title);
    }

    public IStructureContext AddLabeledFormField(WidgetAnnotation widget, string? title = null, string? tooltip = null)
    {
        var formCtx = AddElement("Form", title);
        var lblCtx = formCtx.AddElement("Lbl");
        var node = formCtx.GetNode();
        var widgetObj = widget.NativeObject;
        
        // Requirement 2.3: Consistency for /Contents and /TU
        ApplyAnnotationDescription(widgetObj, widget.Field, tooltip ?? title, overwriteFieldTooltip: tooltip != null);

        _builder.BindAnnotation(node, widget.Page, widgetObj);
        return lblCtx;
    }

    public IStructureContext BindImage(XObjImage image, params PdfPage[] pages)
    {
        _builder.BindImage(_node, image, pages);
        return this;
    }

    public IStructureContext BindFormXObject(XObjForm form, params PdfPage[] pages)
    {
        _builder.BindFormXObject(_node, form, pages);
        return this;
    }

    public IStructureContext BindMarkedContent(PdfPage page, int mcid)
    {
        _builder.BindMarkedContent(_node, page, mcid);
        return this;
    }

    public IStructureContext BindAnnotation(PdfDictionary annotation, PdfPage page)
    {
        _builder.BindAnnotation(_node, page, annotation);
        return this;
    }

    public IStructureContext AddAnnot(PdfPage page, PdfDictionary annotation, string? title = null, string? altText = null)
    {
        EnsureAnnotationOnPage(page, annotation, addWhenMissing: true);
        ApplyAnnotationDescription(annotation, null, altText ?? title);
        var ctx = AddElement("Annot", title);
        if (altText != null) ctx.Alt(altText);
        _builder.BindAnnotation(ctx.GetNode(), page, annotation);
        return ctx;
    }

    public IStructureContext AddLayoutAttributes(string? textAlign = null, double? width = null, double? height = null)
    {
        var attr = new PdfDictionary { [PdfName.O] = PdfName.Layout };
        if (textAlign != null) attr[PdfName.TextAlign] = (PdfName)textAlign;
        if (width.HasValue) attr[PdfName.Width] = new PdfDoubleNumber(width.Value);
        if (height.HasValue) attr[PdfName.Height] = new PdfDoubleNumber(height.Value);
        if (attr.Count > 1) _node.Attributes.Add(attr);
        return this;
    }

    public IStructureContext Alt(string? altText)
    {
        _node.Alt = altText;
        return this;
    }

    public IStructureContext ActualText(string? actualText)
    {
        _node.ActualText = actualText;
        return this;
    }

    public IStructureContext Expansion(string? expansion)
    {
        _node.Expansion = expansion;
        return this;
    }

    public IStructureContext Lang(string? language)
    {
        _node.Language = language;
        return this;
    }

    public IStructureContext ElementId(string? id)
    {
        _node.ID = id;
        return this;
    }

    public IStructureContext References(params string[] ids)
    {
        AddUnique(_node.References, ids);
        return this;
    }

    public IStructureContext References(params IStructureContext[] targets)
    {
        AddTargets(_node.References, targets);
        return this;
    }

    public IStructureContext AddClass(string className)
    {
        AddUnique(_node.Classes, className);
        return this;
    }

    public IStructureContext SetNamespace(StructureNamespace? ns)
    {
        _node.Namespace = ns;
        return this;
    }

    public IStructureContext TableScope(StructureScope? scope)
    {
        _node.Scope = scope;
        return this;
    }

    public IStructureContext TableHeaders(params string[] ids)
    {
        AddUnique(_node.Headers, ids);
        return this;
    }

    public IStructureContext TableHeaders(params IStructureContext[] targets)
    {
        AddTargets(_node.Headers, targets);
        return this;
    }

    public IStructureContext TableSummary(string? summary)
    {
        _node.Summary = summary;
        return this;
    }

    public IStructureContext ListNumbering(StructureListNumbering? numbering)
    {
        _node.ListNumbering = numbering;
        return this;
    }

    public IStructureContext Ref(StructureNode target)
    {
        if (string.IsNullOrEmpty(target.ID))
        {
            target.ID = Guid.NewGuid().ToString("N");
        }
        AddUnique(_node.References, target.ID);
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

    private static void AddUnique(List<string> values, params string[] additions)
    {
        foreach (var addition in additions)
        {
            if (string.IsNullOrWhiteSpace(addition) || values.Contains(addition))
            {
                continue;
            }

            values.Add(addition);
        }
    }

    private void AddTargets(List<string> destination, IStructureContext[] targets)
    {
        ArgumentNullException.ThrowIfNull(targets);
        foreach (var target in targets)
        {
            ArgumentNullException.ThrowIfNull(target);
            var node = target.GetNode();
            var root = _builder.GetStructureRoot();
            if (!ReferenceEquals(node.Root, root))
            {
                throw new ArgumentException("Referenced structure context belongs to another structure tree.", nameof(targets));
            }
            AddUnique(destination, root.EnsureNodeId(node));
        }
    }

    private static void EnsureAnnotationOnPage(PdfPage page, PdfDictionary annotation, bool addWhenMissing)
    {
        StructuralBuilder.EnsureAnnotationOnPage(page, annotation, addWhenMissing);
    }

    private static void ApplyAnnotationDescription(
        PdfDictionary annotation,
        PdfDictionary? field,
        string? description,
        bool overwriteFieldTooltip = false)
    {
        StructuralBuilder.ApplyAnnotationDescription(annotation, field, description, overwriteFieldTooltip);
    }
}
