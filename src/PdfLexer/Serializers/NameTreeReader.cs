using System.Collections.Generic;

namespace PdfLexer.Serializers;

internal static class NameTreeReader
{
    /// <summary>
    /// Returns the set of named/string destination keys reachable through
    /// <c>Catalog/Dests</c> (PDF 1.1 form) and <c>Catalog/Names/Dests</c>
    /// (PDF 1.2+ name tree). Returns null when neither is present.
    /// </summary>
    public static HashSet<string>? BuildDestinationKeys(PdfDictionary catalog)
    {
        HashSet<string>? keys = null;

        if (catalog.TryGetValue<PdfDictionary>(PdfName.Dests, out var dests))
        {
            foreach (var (k, _) in dests)
            {
                keys ??= new HashSet<string>(System.StringComparer.Ordinal);
                keys.Add(k.Value);
            }
        }

        if (catalog.TryGetValue<PdfDictionary>(PdfName.Names, out var names)
            && names.TryGetValue<PdfDictionary>(PdfName.Dests, out var destsTree))
        {
            keys ??= new HashSet<string>(System.StringComparer.Ordinal);
            CollectNameTreeKeys(destsTree, keys, new HashSet<PdfDictionary>());
        }

        return keys;
    }

    private static void CollectNameTreeKeys(PdfDictionary tree, HashSet<string> keys, HashSet<PdfDictionary> seen)
    {
        if (!seen.Add(tree))
        {
            return;
        }

        if (tree.TryGetValue<PdfArray>(PdfName.Names, out var entries))
        {
            for (var i = 0; i < entries.Count - 1; i += 2)
            {
                if (entries[i] is PdfString k)
                {
                    keys.Add(k.Value);
                }
            }
        }

        if (tree.TryGetValue<PdfArray>(PdfName.Kids, out var kids))
        {
            foreach (var kid in kids)
            {
                if (kid.Resolve() is PdfDictionary kidDict)
                {
                    CollectNameTreeKeys(kidDict, keys, seen);
                }
            }
        }
    }
}
