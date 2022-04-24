using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer.Serializers
{
    internal class WritingUtil
    {
        public static void RemovedUnusedLinks(PdfDictionary page, Func<PdfIndirectRef, bool> exists)
        {
            if (!page.TryGetValue<PdfArray>(PdfName.Annots, out var data, false))
            {
                return;
            }
            List<IPdfObject> toRemove = null;
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
            if (toRemove != null)
            {
                data = data.CloneShallow();
                toRemove.ForEach(x => data.Remove(x));
                page[PdfName.Annots] = data;
            }
        }
    }
}
