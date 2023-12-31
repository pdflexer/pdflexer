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
        for (var i = 0; i < data.Count; i++)
        {
            var current = data[i].Resolve();
            if (current is PdfDictionary annot && annot.ContainsKey(PdfName.P))
            {
                var copy = annot.CloneShallow();
                copy[PdfName.P] = PdfIndirectRef.Create(page);
                data[i] = copy;
            }
        }
    }


}
