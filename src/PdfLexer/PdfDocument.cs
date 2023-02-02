using PdfLexer.DOM;
using PdfLexer.IO;
using PdfLexer.Parsers;
using PdfLexer.Parsers.Structure;
using PdfLexer.Serializers;
using System.Threading.Tasks.Sources;

namespace PdfLexer;

/// <summary>
/// Represents a single PDF document.
/// </summary>
public sealed class PdfDocument : IDisposable
{
    /// <summary>
    /// Id of PDF, used for tracking indirect references between documents. 
    /// </summary>
    internal int DocumentId { get; set; }
    /// <summary>
    /// Parsing context for this PDF. May be internalized but may provide external access to allow parallel processing at some point.
    /// </summary>
    public ParsingContext Context { get; private set; }
    /// <summary>
    /// Version of the PDF document.
    /// </summary>
    public decimal PdfVersion { get; set; }
    /// <summary>
    /// PDF trailer dictionary.
    /// Note: The /Root entry pointing to the PDF catalog will be overwritten if PDF is saved.
    /// </summary>
    public PdfDictionary Trailer { get; private set; }
    /// <summary>
    /// PDF catalog dictionary.
    /// Note: The /Pages entry pointing to the page tree will be overwritten if PDF is saved and <see cref="Pages"/> is not null.
    /// </summary>
    public PdfDictionary Catalog { get; set; }
    /// <summary>
    /// List of pages in the document. May be null if <see cref="ParsingOptions.LoadPageTree"/> is false.
    /// </summary>
    public List<PdfPage> Pages { get; set; }
    /// <summary>
    /// XRef entries of this document. May be internalized at some point.
    /// Will be null on new documents.
    /// </summary>
    public IReadOnlyDictionary<ulong, XRefEntry> XrefEntries => Context.XRefs;


    internal PdfDocument(ParsingContext ctx, 
        PdfDictionary catalog, PdfDictionary trailer, List<PdfPage> pages)
    {
        DocumentId = ctx.SourceId;
        Context = ctx;
        ctx.Document = this;
        Trailer = trailer;
        Pages = pages;
        Catalog = catalog;
    }

    public void Dispose()
    {
        Context?.Dispose();
        Pages = null!;
        Catalog = null!;
        Trailer = null!;
        Context = null!;
    }

    public byte[] Save()
    {
        using var ms = new MemoryStream();
        SaveTo(ms);
        return ms.ToArray();
    }

    public PdfPage AddPage(PageSize size=PageSize.LETTER)
    {
        var pg = new PdfPage();
        pg.MediaBox = PageSizeHelpers.GetMediaBox(size);
        Pages.Add(pg);
        return pg;
    }

    public PdfPage AddPage(double width, double height)
    {
        var pg = new PdfPage();
        pg.MediaBox = new PdfArray { PdfCommonNumbers.Zero, PdfCommonNumbers.Zero, new PdfDoubleNumber(width), new PdfDoubleNumber(height) };
        Pages.Add(pg);
        return pg;
    }

    /// <summary>
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
            if (obj.Type == XRefType.Normal && Context.IsDataCopyable(obj.Reference)) // TODO copying of compressed items
            {
                ctx.WriteExistingData(Context, obj);
            }
            else
            {
                ctx.WriteIndirectObject(new ExistingIndirectRef(Context, obj.Reference));
            }
        }
    }

    /// <summary>
    /// Create a new empty PDF document.
    /// </summary>
    /// <returns>PdfDocument</returns>
    public static PdfDocument Create()
    {
        var ctx = new ParsingContext();
        var doc = new PdfDocument(ctx,
            new PdfDictionary { [PdfName.TypeName] = PdfName.Catalog }, new PdfDictionary(),
            new List<PdfPage>());
        ctx.Document = doc;
        return doc;
    }


    /// <summary>
    /// Opens a PDF document from the provided seekable stream.
    /// </summary>
    /// <param name="data">PDF data</param>
    /// <param name="options">Optional parsing options</param>
    /// <returns>PdfDocument</returns>
    public static PdfDocument Open(Stream data, ParsingOptions? options = null)
    {
        options ??= new ParsingOptions { };
        var ctx = new ParsingContext(options);
        var source = new StreamDataSource(ctx, data);
        var result = ctx.Initialize(source);
        return Open(ctx, result.XRefs, result.Trailer);
    }

    /// <summary>
    /// Opens a PDF document from the provided seekable stream.
    /// </summary>
    /// <param name="data">PDF data</param>
    /// <param name="options">Optional parsing options</param>
    /// <returns>PdfDocument</returns>
    public static PdfDocument OpenLowMemory(Stream data, ParsingOptions? options = null)
    {
        options ??= new ParsingOptions();
        options.CacheNames = false;
        options.CacheNumbers = false;
        options.LowMemoryMode = true;
        var ctx = new ParsingContext(options);

        var source = new StreamDataSource(ctx, data);
        var result = ctx.Initialize(source);
        return Open(ctx, result.XRefs, result.Trailer);
    }

    /// <summary>
    /// Opens a PDF document from the provided byte array.
    /// </summary>
    /// <param name="data">PDF data</param>
    /// <param name="options">Optional parsing options</param>
    /// <returns>PdfDocument</returns>
    public static PdfDocument Open(byte[] data, ParsingOptions? options = null)
    {
        options ??= new ParsingOptions { };
        var ctx = new ParsingContext(options);
        var source = new InMemoryDataSource(ctx, data);
        var result = ctx.Initialize(source);
        return Open(ctx, result.XRefs, result.Trailer);
    }

    /// <summary>
    /// Opens a PDF document from the provided file path.
    /// </summary>
    /// <param name="data">PDF data</param>
    /// <param name="options">Optional parsing options</param>
    /// <returns>PdfDocument</returns>
    public static PdfDocument Open(string file, ParsingOptions? options = null)
    {
        options ??= new ParsingOptions { };
        IPdfDataSource source;
        var ctx = new ParsingContext(options);
#if NET6_0_OR_GREATER
        try
        {
            source = new MemoryMappedDataSource(ctx, file);
        }
        catch (NotSupportedException)
        {
            var fs = File.OpenRead(file);
            source = new StreamDataSource(ctx, fs, false);
        }
#else
        var fs = File.OpenRead(file);
        source = new StreamDataSource(ctx, fs, false);
#endif

        var result = ctx.Initialize(source);
        return Open(ctx, result.XRefs, result.Trailer);
    }


#if NET6_0_OR_GREATER
    [Obsolete()]
    public static PdfDocument OpenMapped(string file, ParsingOptions? options = null) => Open(file, options);
#endif

    private static PdfDocument Open(ParsingContext ctx, Dictionary<ulong, XRefEntry> xrefs, PdfDictionary? trailer)
    {
        trailer ??= new PdfDictionary();

        // TODO clean doc id during parsing up
        foreach (var item in trailer.Values)
        {
            if (item.Type == PdfObjectType.IndirectRefObj)
            {
                var eir = (ExistingIndirectRef)item;
                eir.SourceId = ctx.SourceId;
            }
        }

        var cat = trailer.GetOptionalValue<PdfDictionary>(PdfName.Root);
        if (cat == null ||
            (cat.GetOptionalValue<PdfName>(PdfName.TypeName) != PdfName.Catalog && !cat.ContainsKey(PdfName.Pages)))
        {
            var matched = ctx.RepairFindLastMatching(PdfTokenType.DictionaryStart, x =>
            {
                if (x.Type != PdfObjectType.DictionaryObj)
                {
                    return false;
                }
                var dict = x.GetValue<PdfDictionary>();
                if (dict.GetOptionalValue<PdfName>(PdfName.TypeName)?.Value == PdfName.Catalog.Value)
                {
                    return true;
                }
                return false;
            })?.GetValue<PdfDictionary>();
            if (matched != null && cat == null)
            {
                cat = matched;
            }
            else if (matched != null && matched.ContainsKey(PdfName.Pages))
            {
                cat = matched;
            }
        }

        var v = ctx.GetHeaderVersion();
        if (v >= 1.4m && cat != null)
        {
            var ver = cat.Get<PdfName>(PdfName.Version);
            if (ver != null)
            {
                if (decimal.TryParse(ver.Value, out var cv))
                {
                    v = cv;
                }
            }
        }
        
        var pagesRef = cat?.GetOptionalValue<PdfDictionary>(PdfName.Pages);
        List<PdfPage> pages = new();
        if (ctx.Options.LoadPageTree && pagesRef != null)
        {
            foreach (var pg in CommonUtil.EnumeratePageTree(pagesRef))
            {
                pg.SourceVersion = v;
                pages.Add(pg);
            }
        }
        var doc = new PdfDocument(ctx, cat ?? new PdfDictionary(), trailer, pages);
        doc.PdfVersion = v ?? 1.5m;
        ctx.Document = doc;
        return doc;
    }



    private static int docId = 0;
    internal static int GetNextId() => Interlocked.Increment(ref docId);
}
