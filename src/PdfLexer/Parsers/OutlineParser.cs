using PdfLexer.DOM;
using System.Collections.Generic;
using System.Linq;

namespace PdfLexer.Parsers;

internal class OutlineParser
{
    private readonly PdfDocument _doc;
    private readonly Dictionary<IPdfObject, PdfPage> _pageMap;

    public OutlineParser(PdfDocument doc)
    {
        _doc = doc;
        _pageMap = new Dictionary<IPdfObject, PdfPage>();
        if (doc.Pages != null)
        {
            foreach (var page in doc.Pages)
            {
                _pageMap[page.NativeObject] = page;
            }
        }
    }

    public static BookmarkNode? Parse(PdfDocument doc)
    {
        if (doc.Catalog.TryGetValue<PdfDictionary>(PdfName.Outlines, out var outlines))
        {
            var parser = new OutlineParser(doc);
            var root = new BookmarkNode { Title = "ROOT" };
            if (outlines.TryGetValue<PdfDictionary>(PdfName.First, out var first))
            {
                parser.ParseSiblings(first, root);
            }
            return root;
        }

        return null;
    }

    private void ParseSiblings(PdfDictionary first, BookmarkNode parent)
    {
        var current = first;
        while (current != null)
        {
            var node = ParseItem(current);
            parent.Children.Add(node);
            
            if (current.TryGetValue<PdfDictionary>(PdfName.Next, out var next))
            {
                current = next;
            }
            else
            {
                current = null; // Stop
            }
        }
    }

    private BookmarkNode ParseItem(PdfDictionary item)
    {
        var node = new BookmarkNode();
        node.Title = item.GetOptionalValue<PdfString>(PdfName.Title)?.Value ?? "";
        
        // Destination
        IPdfObject? dest = null;
        if (item.TryGetValue(PdfName.Dest, out var d))
        {
            dest = d.Resolve();
        }
        else if (item.TryGetValue<PdfDictionary>(PdfName.A, out var action))
        {
            var type = action.GetOptionalValue<PdfName>(PdfName.TypeName);
            var subtype = action.GetOptionalValue<PdfName>(PdfName.Subtype);
            var s = action.GetOptionalValue<PdfName>(PdfName.S);

            if ((type == PdfName.GoTo || subtype == PdfName.GoTo || s == PdfName.GoTo) && action.TryGetValue(PdfName.D, out var ad))
            {
                dest = ad.Resolve();
            }
        }

        if (dest != null)
        {
            // Resolve to page object if possible
            var pageObj = ResolvePage(dest);
            if (pageObj != null)
            {
                node.Destination = pageObj;
                if (_pageMap.TryGetValue(pageObj, out var page))
                {
                    page.Outlines.Add(node);
                }
            }
            else
            {
                node.Destination = dest; // Keep raw destination if resolution fails or it's not a page
            }
        }

        // Color
        if (item.TryGetValue<PdfArray>(PdfName.C, out var cArr) && cArr.Count == 3)
        {
            node.Color = cArr.Select(x => (double)(PdfDoubleNumber)x).ToArray();
        }

        // Style
        if (item.TryGetValue<PdfIntNumber>(PdfName.F, out var style))
        {
            node.Style = style.Value;
        }

        // Count (Open/Closed state)
        if (item.TryGetValue<PdfIntNumber>(PdfName.Count, out var count))
        {
            node.IsOpen = count.Value > 0;
        }
        else
        {
            // Default to open if no children count specified? Or closed? 
            // Spec says: "If the Count entry is missing, the element is considered to be closed." (Actually it says open if positive, closed if negative. Missing usually implies 0/none, but if it has children and missing, state is ambiguous. Let's default true.)
            // Actually, usually Count is mandatory if there are children.
            node.IsOpen = true;
        }

        // Children
        if (item.TryGetValue<PdfDictionary>(PdfName.First, out var firstChild))
        {
            ParseSiblings(firstChild, node);
        }

        return node;
    }

    private IPdfObject? ResolvePage(IPdfObject dest)
    {
        dest = dest.Resolve();
        if (dest.Type == PdfObjectType.NameObj || dest.Type == PdfObjectType.StringObj)
        {
            var resolved = ResolveNamedDest(dest);
            if (resolved == null) { return null; }
            dest = resolved.Resolve();
        }

        if (dest.Type == PdfObjectType.DictionaryObj)
        {
            // Direct page dictionary?
            var dict = (PdfDictionary)dest;
            if (dict.GetOptionalValue<PdfName>(PdfName.TypeName) == PdfName.Page)
            {
                return dict;
            }
            
            // Or Destination array [Page /View ...]
            // Actually Destination can be an array [PageRef /Fit ...]
        }

        if (dest.Type == PdfObjectType.ArrayObj)
        {
            var arr = (PdfArray)dest;
            if (arr.Count > 0)
            {
                var p = arr[0].Resolve();
                if (p.Type == PdfObjectType.DictionaryObj && ((PdfDictionary)p).GetOptionalValue<PdfName>(PdfName.TypeName) == PdfName.Page)
                {
                    return p;
                }
            }
        }
        return null;
    }

    internal IPdfObject? ResolveNamedDest(IPdfObject dest)
    {
        string? name = null;
        if (dest is PdfName n) name = n.Value;
        else if (dest is PdfString s) name = s.Value;

        if (name == null) return null;

        if (_doc.Catalog.TryGetValue<PdfDictionary>(new PdfName("Dests"), out var dests)) 
        {
            if (dests.TryGetValue(new PdfName(name), out var val)) return val;
        }

        if (_doc.Catalog.TryGetValue<PdfDictionary>(new PdfName("Names"), out var names))
        {
            if (names.TryGetValue<PdfDictionary>(new PdfName("Dests"), out var destsTree))
            {
                return ResolveInNameTree(destsTree, name);
            }
        }

        return null;
    }

    private IPdfObject? ResolveInNameTree(PdfDictionary tree, string key)
    {
        if (tree.TryGetValue<PdfArray>(new PdfName("Limits"), out var limits) && limits.Count == 2)
        {
            var min = limits[0] as PdfString;
            var max = limits[1] as PdfString;
            if (min != null && max != null)
            {
                if (string.CompareOrdinal(key, min.Value) < 0 || string.CompareOrdinal(key, max.Value) > 0)
                {
                    return null;
                }
            }
        }

        if (tree.TryGetValue<PdfArray>(new PdfName("Names"), out var names))
        {
            for (int i = 0; i < names.Count - 1; i += 2)
            {
                var k = names[i] as PdfString;
                if (k != null && k.Value == key)
                {
                    return names[i + 1].Resolve();
                }
            }
        }

        if (tree.TryGetValue<PdfArray>(new PdfName("Kids"), out var kids))
        {
            foreach (var kid in kids)
            {
                var kidDict = kid.Resolve() as PdfDictionary;
                if (kidDict != null)
                {
                    var result = ResolveInNameTree(kidDict, key);
                    if (result != null) return result;
                }
            }
        }

        return null;
    }
}
