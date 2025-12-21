using PdfLexer.DOM;

namespace PdfLexer.Parsers;

internal class OutlineParser
{
    private readonly PdfDocument _doc;

    public OutlineParser(PdfDocument doc)
    {
        _doc = doc;
    }

    public static PdfOutlineRoot? Parse(PdfDocument doc)
    {
        if (doc.Catalog.TryGetValue<PdfDictionary>(PdfName.Outlines, out var outlines))
        {
            var parser = new OutlineParser(doc);
            return new PdfOutlineRoot(outlines);
        }

        return null;
    }
    
    // TODO move this to a better spot
    internal IPdfObject? ResolveNamedDest(IPdfObject dest)
    {
        string? name = null;
        if (dest is PdfName n) name = n.Value;
        else if (dest is PdfString s) name = s.Value;

        if (name == null) return null;

        // 1. Dests Dictionary (PDF 1.1)
        // Standard name Dests
        if (_doc.Catalog.TryGetValue<PdfDictionary>(new PdfName("Dests"), out var dests)) 
        {
            if (dests.TryGetValue(new PdfName(name), out var val)) return val;
        }

        // 2. Names Tree (PDF 1.2)
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
        // Check Limits
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