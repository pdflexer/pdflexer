using PdfLexer.Parsers.Structure;
using PdfLexer.Serializers;

namespace PdfLexer;

public sealed partial class PdfDocument
{/// <summary>
 /// Saves the document to provided location
 /// </summary>
 /// <param name="stream"></param>
    public void SaveTo(string path)
    {
        using var fo = File.Create(path);
        SaveTo(fo);
    }

    /// <summary>
    /// Saves the document to the provided stream.
    /// </summary>
    /// <param name="stream"></param>
    public void SaveTo(Stream stream)
    {
        var nums = XrefEntries?.Values.Select(x => x.Reference.ObjectNumber).ToList();
        var nextId = 1;
        if (nums != null && nums.Any())
        {
            nextId = nums.Max() + 1;
        }
        var ctx = new WritingContext(stream, nextId, DocumentId);

        var wv = 0m;
        if (Pages != null && Pages.Count > 0)
        {
            wv = Pages.Max(x => x.SourceVersion ?? 0);
        }
        wv = Math.Max(wv, PdfVersion);
        if (wv == 0) { wv = 1.7m; } // default to 1.7
        ctx.Initialize(wv);
        if (XrefEntries?.Count > 0)
        {
            SaveExistingObjects(ctx);
        }
        // create clones of these in case they were
        // copied from another doc, don't want to modify existing
        var catalog = Catalog.CloneShallow();
        var trailer = Trailer.CloneShallow();

        // remove page tree specific items
        catalog.Remove("Names");
        catalog.Remove("Outlines");
        catalog.Remove("StructTreeRoot");

        var cir = PdfIndirectRef.Create(catalog);
        trailer[PdfName.Root] = cir;

        // remove page tree specific items
        trailer.Remove(PdfName.Encrypt); // TODO support encryption
        trailer.Remove(PdfName.DecodeParms);
        trailer.Remove(PdfName.Filter);
        trailer.Remove(PdfName.Length);
        trailer.Remove(PdfName.Prev);
        trailer.Remove(PdfName.XRefStm);
        if (Pages != null)
        {
            catalog[PdfName.Pages] = BuildPageTree(ctx);
        }
        ctx.Complete(trailer);
    }

    private IPdfObject BuildPageTree(WritingContext ctx)
    {
        // TODO page tree
        var dict = new PdfDictionary();
        var arr = new PdfArray();
        var ir = PdfIndirectRef.Create(dict);
        var pageDicts = Pages.Select(x => x.NativeObject).ToList();
        foreach (var page in Pages)
        {
            var pg = page.NativeObject.CloneShallow();
            WritingUtil.RemovedUnusedLinks(pg, ir => pageDicts.Contains(ir.GetObject()));
            pg[PdfName.Parent] = ir;
            if (page.SourceRef != null)
            {
                //page.SourceRef.Object = pg;
            }
            var nir = PdfIndirectRef.Create(pg);
            arr.Add(nir);
            // ctx.WriteIndirectObject(nir);
        }
        dict[PdfName.Kids] = arr;
        dict[PdfName.TypeName] = PdfName.Pages;
        dict[PdfName.Count] = new PdfIntNumber(Pages.Count);
        return PdfIndirectRef.Create(dict);
    }

    private void SaveExistingObjects(WritingContext ctx)
    {
        foreach (var obj in XrefEntries.Values)
        {
            if (obj.IsFree)
            {
                continue;
            }
            if (obj.Type == XRefType.Normal && obj.Offset == 0)
            {
                // buggy PDFs
                continue;
            }
            if (obj.Type == XRefType.Normal && Context.IsDataCopyable(obj.Source, obj.Reference)) // TODO copying of compressed items
            {
                ctx.WriteExistingData(Context, obj);
            }
            else
            {
                ctx.WriteIndirectObject(new ExistingIndirectRef(Context, obj.Reference));
            }
        }
    }
}
