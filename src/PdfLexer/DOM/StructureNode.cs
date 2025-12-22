using System.Collections.Generic;

namespace PdfLexer.DOM;

/// <summary>
/// A high-level model for a PDF structure element (Tag).
/// </summary>
public class StructureNode
{
    /// <summary>
    /// The structure element type (e.g., "P", "H1", "Table").
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Optional unique identifier for the element.
    /// </summary>
    public string? ID { get; set; }

    /// <summary>
    /// Optional title for the element.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Optional alternate text for the element (accessibility).
    /// </summary>
    public string? AlternateText { get; set; }

    /// <summary>
    /// Optional language identifier (e.g., "en-US").
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// Collection of attribute dictionaries associated with this element.
    /// </summary>
    public List<PdfDictionary> Attributes { get; } = new List<PdfDictionary>();

    /// <summary>
    /// The child structure elements of this node.
    /// </summary>
    public List<StructureNode> Children { get; } = new List<StructureNode>();

    /// <summary>
    /// Internal collection of content items (MCIDs) registered to this node.
    /// </summary>
    internal List<(PdfPage Page, int MCID)> ContentItems { get; } = new List<(PdfPage Page, int MCID)>();
}
