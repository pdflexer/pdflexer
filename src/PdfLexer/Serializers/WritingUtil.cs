using System.Reflection.Metadata.Ecma335;

namespace PdfLexer.Serializers;

internal class WritingUtil
{
    public static void RemovedUnusedLinks(PdfDictionary page, Func<PdfIndirectRef, bool> exists)
    {
        if (!page.TryGetValue<PdfArray>(PdfName.Annots, out var data, false))
        {
            return;
        }
        List<IPdfObject>? toRemove = null;
        foreach (var annot in data)
        {
            if (annot is PdfDictionary annotDict)
            {
                if (!annotDict.TryGetValue<PdfName>(PdfName.Subtype, out var val, false) || val != PdfName.Link)
                {
                    continue;
                }

                if (!annotDict.TryGetValue(PdfName.Dest, out var dest))
                {
                    continue;
                }

                bool remove = false;
                if (dest is PdfName named)
                {
                    // TODO check if exists -> just remove for now
                    remove = true;
                }
                else if (dest is PdfString stringed)
                {
                    remove = true;
                }
                else if (dest is PdfArray array && array.Count > 0 && array[0] is PdfIndirectRef xref)
                {
                    if (!exists(xref))
                    {
                        remove = true;
                    }

                }
                if (remove)
                {
                    toRemove ??= new List<IPdfObject>();
                    toRemove.Add(annot);
                }
            }
        }

        // update P links

        bool cloned = false;
        if (toRemove != null)
        {
            cloned = true;
            data = data.CloneShallow();
            toRemove.ForEach(x => data.Remove(x));
            page[PdfName.Annots] = data;
        }

        if (!cloned)
        {
            data = data.CloneShallow();
            page[PdfName.Annots] = data;
        }

        // first pass we'll clone and check for popups
        // cloning here in case someone used these across multiple pages
        // don't think that's valid but just in case
        var popups = new Dictionary<int, PdfDictionary>();
        for (var i = 0; i < data.Count; i++)
        {
            var current = data[i].Resolve();
            if (!(current is PdfDictionary annot))
            {
                continue;
            }

            if (!annot.TryGetValue<PdfDictionary>(PdfName.Popup, out var popup))
            {
                continue;
            }
            
            var pi = FindIndex(data, x => x.GetValueOrNull<PdfDictionary>() == popup);
            if (pi == -1)
            {
                annot.Remove(PdfName.Popup);
                continue;
            }
            if (!popups.TryGetValue(pi, out var clonedPopup))
            {
                clonedPopup = popup.CloneShallow();
                if (clonedPopup.ContainsKey(PdfName.P))
                {
                    clonedPopup[PdfName.P] = PdfIndirectRef.Create(page);
                }
                popups[pi] = clonedPopup;
            }
            annot[PdfName.Popup] = clonedPopup.Indirect();
        }

        for (var i = 0; i < data.Count; i++)
        {
            if (popups.TryGetValue(i, out var clonedPopup))
            {
                data[i] = clonedPopup.Indirect();
                continue; // already adjusted above
            }
            var current = data[i].Resolve();
            if (current is PdfDictionary annot && annot.ContainsKey(PdfName.P))
            {
                var copy = annot.CloneShallow();
                copy[PdfName.P] = PdfIndirectRef.Create(page);
                data[i] = copy.Indirect();
            }
        }
    }

    private static int FindIndex(PdfArray arr, Func<IPdfObject, bool> match)
    {
        for (var i = 0; i < arr.Count; i++)
        {
            if (match(arr[i]))
            {
                return i;
            }
        }
        return -1;
    }
}
