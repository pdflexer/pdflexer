using System;
using System.Collections.Generic;

namespace PdfLexer.DOM;

/// <summary>
/// A high-level model for a PDF structure element (Tag).
/// </summary>
/// <remarks>
/// <see cref="StructureNode"/> instances represent authored structure for the library's accessible-authoring workflow on
/// new or currently untagged documents. They do not model in-place remediation of an existing tagged PDF. See
/// <c>docs/accessibility-authoring.md</c> for the supported scope.
/// </remarks>
public class StructureNode
{
    private string? _id;
    private string? _alt;
    private StructureNamespace? _namespace;

    /// <summary>
    /// The structure element type (e.g., "P", "H1", "Table").
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Optional unique identifier for the element.
    /// </summary>
    public string? ID
    {
        get => _id;
        set
        {
            if (_id == value) { return; }
            Root?.UpdateNodeId(this, _id, value);
            _id = value;
        }
    }

    /// <summary>
    /// Optional title for the element.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Optional alternate text for the element (accessibility).
    /// </summary>
    public string? Alt
    {
        get => _alt;
        set => _alt = value;
    }

    /// <summary>
    /// Backward-compatible alias for <see cref="Alt"/>.
    /// </summary>
    public string? AlternateText
    {
        get => Alt;
        set => Alt = value;
    }

    /// <summary>
    /// Optional replacement text for assistive technologies.
    /// </summary>
    public string? ActualText { get; set; }

    /// <summary>
    /// Optional expansion text (/E).
    /// </summary>
    public string? Expansion { get; set; }

    /// <summary>
    /// Optional language identifier (e.g., "en-US").
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// Optional namespace binding for the element.
    /// </summary>
    public StructureNamespace? Namespace
    {
        get => _namespace;
        set
        {
            _namespace = value;
            if (value != null)
            {
                value.Root?.RegisterNamespace(value);
            }
        }
    }

    /// <summary>
    /// Collection of outgoing authored references by target node ID.
    /// </summary>
    public List<string> References { get; } = new List<string>();

    /// <summary>
    /// Collection of class names assigned to this element.
    /// </summary>
    public List<string> Classes { get; } = new List<string>();

    /// <summary>
    /// Collection of table header IDs associated with this element.
    /// </summary>
    public List<string> Headers { get; } = new List<string>();

    /// <summary>
    /// Table summary text, when authored.
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// Table header scope, when authored.
    /// </summary>
    public StructureScope? Scope { get; set; }

    /// <summary>
    /// List numbering semantics, when authored.
    /// </summary>
    public StructureListNumbering? ListNumbering { get; set; }

    /// <summary>
    /// Collection of attribute dictionaries associated with this element.
    /// </summary>
    public List<PdfDictionary> Attributes { get; } = new List<PdfDictionary>();

    /// <summary>
    /// The child structure elements of this node.
    /// </summary>
    public List<StructureNode> Children { get; } = new List<StructureNode>();

    /// <summary>
    /// Parent node in the authored structure tree.
    /// </summary>
    public StructureNode? Parent { get; internal set; }

    /// <summary>
    /// Owning root registry for this node.
    /// </summary>
    public StructureRoot? Root { get; internal set; }

    /// <summary>
    /// Internal collection of content items (MCIDs) registered to this node.
    /// </summary>
    internal List<(PdfPage Page, int MCID)> ContentItems { get; } = new List<(PdfPage Page, int MCID)>();
    internal List<(XObjForm Form, int MCID)> XObjectContentItems { get; } = new List<(XObjForm Form, int MCID)>();

    /// <summary>
    /// Internal collection of object references (e.g. annotations) registered to this node.
    /// </summary>
    internal List<StructureObjectReference> ObjectReferences { get; } = new List<StructureObjectReference>();

    /// <summary>
    /// Internal collection of XObject parent-tree bindings registered to this node.
    /// </summary>
    internal List<StructureXObjectReference> XObjectReferences { get; } = new List<StructureXObjectReference>();
}

public sealed class StructureRoot
{
    private readonly Dictionary<string, StructureNode> _idMap = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _roleMap = new(StringComparer.Ordinal);
    private readonly Dictionary<string, IPdfObject> _classMap = new(StringComparer.Ordinal);
    private readonly List<StructureNamespace> _namespaces = new();
    private int _nextStructParentIndex;

    public StructureRoot()
    {
        Document = new StructureNode
        {
            Type = "Document",
            Root = this
        };
    }

    /// <summary>
    /// Root document node.
    /// </summary>
    public StructureNode Document { get; }

    /// <summary>
    /// Authored IDs mapped to nodes.
    /// </summary>
    public IReadOnlyDictionary<string, StructureNode> IdMap => _idMap;

    /// <summary>
    /// Custom role mappings.
    /// </summary>
    public IReadOnlyDictionary<string, string> RoleMap => _roleMap;

    /// <summary>
    /// Class definitions.
    /// </summary>
    public IReadOnlyDictionary<string, IPdfObject> ClassMap => _classMap;

    /// <summary>
    /// Declared namespaces.
    /// </summary>
    public IReadOnlyList<StructureNamespace> Namespaces => _namespaces;

    internal void AttachChild(StructureNode parent, StructureNode child)
    {
        if (child.Parent == parent && child.Root == this)
        {
            return;
        }

        child.Parent = parent;
        parent.Children.Add(child);
        AttachSubtree(child);
    }

    internal void AttachSubtree(StructureNode node)
    {
        node.Root = this;
        if (!string.IsNullOrEmpty(node.ID))
        {
            UpdateNodeId(node, null, node.ID);
        }

        if (node.Namespace != null)
        {
            RegisterNamespace(node.Namespace);
        }

        foreach (var child in node.Children)
        {
            child.Parent = node;
            AttachSubtree(child);
        }
    }

    internal void UpdateNodeId(StructureNode node, string? oldId, string? newId)
    {
        if (!string.IsNullOrEmpty(oldId) && _idMap.TryGetValue(oldId, out var existing) && ReferenceEquals(existing, node))
        {
            _idMap.Remove(oldId);
        }

        if (string.IsNullOrEmpty(newId))
        {
            return;
        }

        if (_idMap.TryGetValue(newId, out var duplicate) && !ReferenceEquals(duplicate, node))
        {
            throw new PdfLexerException($"Structure node ID '{newId}' is already registered.");
        }

        _idMap[newId] = node;
    }

    public StructureRoot MapRole(string customRole, string standardRole)
    {
        _roleMap[customRole] = standardRole;
        return this;
    }

    public StructureRoot AddClass(string className, PdfDictionary attributes)
    {
        _classMap[className] = attributes;
        return this;
    }

    public StructureRoot AddClass(string className, PdfArray attributes)
    {
        _classMap[className] = attributes;
        return this;
    }

    public StructureNamespace DeclareNamespace(string uri)
    {
        var existing = _namespaces.Find(x => string.Equals(x.Uri, uri, StringComparison.Ordinal));
        if (existing != null)
        {
            return existing;
        }

        var ns = new StructureNamespace(this, uri);
        _namespaces.Add(ns);
        return ns;
    }

    public int AllocateStructParentIndex() => _nextStructParentIndex++;

    internal void RegisterNamespace(StructureNamespace ns)
    {
        if (_namespaces.Contains(ns))
        {
            return;
        }

        _namespaces.Add(ns);
    }
}

public sealed class StructureNamespace
{
    internal StructureNamespace(StructureRoot root, string uri)
    {
        Root = root;
        Uri = uri;
    }

    public string Uri { get; }

    internal StructureRoot Root { get; }
}

public sealed class StructureObjectReference
{
    public StructureObjectReference(IPdfObject obj, int structParentIndex, params PdfPage[] pages)
    {
        Object = obj;
        StructParentIndex = structParentIndex;
        Pages = new List<PdfPage>(pages);
    }

    public IPdfObject Object { get; }

    public int StructParentIndex { get; }

    public List<PdfPage> Pages { get; }

    public StructureNode? StructureDestinationTarget { get; set; }

    public PdfArray? StructureDestinationTemplate { get; set; }

    public string? AnnotationContents { get; set; }
}

public sealed class StructureXObjectReference
{
    public StructureXObjectReference(IPdfObject xObject, int structParentsIndex, params PdfPage[] pages)
    {
        XObject = xObject;
        StructParentsIndex = structParentsIndex;
        Pages = new List<PdfPage>(pages);
    }

    public IPdfObject XObject { get; }

    public int StructParentsIndex { get; }

    public List<PdfPage> Pages { get; }
}

public enum StructureScope
{
    Row,
    Column,
    Both
}

public enum StructureListNumbering
{
    None,
    Decimal,
    LowerRoman,
    UpperRoman,
    LowerAlpha,
    UpperAlpha,
    Disc,
    Circle,
    Square
}
