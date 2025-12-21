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

    public static PdfOutlineRoot? Parse(PdfDocument doc)
    {
        if (doc.Catalog.TryGetValue<PdfDictionary>(PdfName.Outlines, out var outlines))
        {
            var parser = new OutlineParser(doc);
            if (outlines.TryGetValue<PdfDictionary>(PdfName.First, out var first))
            {
                parser.ParseItem(first, null);
            }
            return new PdfOutlineRoot(outlines);
        }

        return null;
    }

    private void ParseItem(PdfDictionary item, List<string>? parentPath)
    {
        var title = item.GetOptionalValue<PdfString>(PdfName.Title)?.Value ?? "";
        
        IPdfObject? dest = null;
        if (item.TryGetValue(PdfName.Dest, out var d))
        {
            dest = d.Resolve();
        }
        else if (item.TryGetValue<PdfDictionary>(PdfName.A, out var action))
        {
            if (action.GetOptionalValue<PdfName>(PdfName.TypeName) == PdfName.GoTo 
                || action.GetOptionalValue<PdfName>(PdfName.Subtype) == PdfName.GoTo
                || action.GetOptionalValue<PdfName>(PdfName.S) == PdfName.GoTo)
            {
                if (action.TryGetValue(PdfName.D, out var ad))
                {
                    dest = ad.Resolve();
                }
            }
        }

        if (dest != null)
        {
            var pageObj = ResolvePage(dest);
            if (pageObj != null)
            {
                if (_pageMap.TryGetValue(pageObj, out var page))
                {
                    var outline = new PdfOutline
                    {
                        Title = title,
                        Section = parentPath != null ? new List<string>(parentPath) : null
                    };
                    page.Outlines.Add(outline);
                }
            }
        }

        if (item.TryGetValue<PdfDictionary>(PdfName.First, out var firstChild))
        {
            var currentPath = parentPath != null ? new List<string>(parentPath) : new List<string>();
            currentPath.Add(title);
            ParseItem(firstChild, currentPath);
        }

        if (item.TryGetValue<PdfDictionary>(PdfName.Next, out var nextSibling))
        {
            ParseItem(nextSibling, parentPath);
        }
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
            var dict = (PdfDictionary)dest;
            if (dict.TryGetValue(PdfName.D, out var d))
            {
                dest = d.Resolve();
            }
        }

        if (dest.Type == PdfObjectType.ArrayObj)
        {
            var arr = (PdfArray)dest;
            if (arr.Count > 0)
            {
                return arr[0].Resolve();
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