namespace PdfLexer.Remediation;

internal static class RemediationStructuralTemplateValidator
{
    private static readonly HashSet<string> StandardTags = new(StringComparer.Ordinal)
    {
        "Document", "DocumentFragment", "Part", "Art", "Sect", "Div", "BlockQuote",
        "Caption", "TOC", "TOCI", "Index", "NonStruct", "Private",
        "H", "H1", "H2", "H3", "H4", "H5", "H6", "P",
        "L", "LI", "Lbl", "LBody", "Table", "TR", "TH", "TD",
        "THead", "TBody", "TFoot", "Span", "Quote", "Note", "Reference",
        "BibEntry", "Code", "Link", "Annot", "Ruby", "RB", "RT", "RP",
        "Warichu", "WT", "WP", "Figure", "Formula", "Form", "FENote",
        "Title", "Sub", "Em", "Strong"
    };

    internal static bool IsStandardTag(string tag) => StandardTags.Contains(tag);

    internal static bool LegalChild(string parent, string child) => parent switch
    {
        "Table" => child is "TR" or "THead" or "TBody" or "TFoot" or "Caption",
        "TR" => child is "TH" or "TD",
        "THead" or "TBody" or "TFoot" => child == "TR",
        "L" => child == "LI",
        "LI" => child is "Lbl" or "LBody",
        _ when child == "TR" => false,
        _ when child is "TH" or "TD" => false,
        _ when child == "LI" => false,
        _ when child is "Lbl" or "LBody" => false,
        _ => true
    };
}
