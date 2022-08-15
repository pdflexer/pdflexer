using PdfLexer.DOM;

namespace PdfLexer.Serializers;

public class StreamingWriter : IDisposable
{
    private readonly WritingContext _ctx;
    private readonly bool dedup;
    private readonly bool memoryDedup;
    private readonly TreeHasher hasher;
    private readonly Dictionary<PdfStreamHash, PdfIndirectRef> refs;

    public StreamingWriter(Stream stream, bool dedupXobj, bool inMemoryDedup)
    {
        _ctx = new WritingContext(stream);
        _ctx.Initialize(1.7m);
        (currentBag, currentBagArray, currentBagRef) = CreateBag();
        dedup = dedupXobj;
        memoryDedup = inMemoryDedup;
        if (dedup)
        {
            hasher = new TreeHasher();
            refs = new Dictionary<PdfStreamHash, PdfIndirectRef>(new FNVStreamComparison());
        } else
        {
            hasher = null!;
            refs = null!;
        }
    }
    public StreamingWriter(Stream stream) : this(stream, false, false) { }

    private List<(PdfDictionary Bag, PdfIndirectRef BagRef)> bags = new List<(PdfDictionary Bag, PdfIndirectRef BagRef)>();

    private PdfDictionary currentBag;
    private PdfArray currentBagArray;
    private PdfIndirectRef currentBagRef;
    private int pageCount = 0;

    public int PageCount { get => pageCount; }

    public void AddPage(PdfPage page)
    {
        var pg = page.NativeObject.CloneShallow();
        WritingUtil.RemovedUnusedLinks(pg, ir => false);
        pg[PdfName.Parent] = currentBagRef;

        // 
        if (dedup 
            && pg.TryGetValue<PdfDictionary>(PdfName.Resources, out var res)
            )
        {
            var resC = res.CloneShallow();
            pg[PdfName.Resources] = resC;

            if (res.TryGetValue<PdfDictionary>(PdfName.XObject, out var xobjs))
            {
                resC[PdfName.XObject] = DedupSingle(xobjs);
            }
            if (res.TryGetValue<PdfDictionary>(PdfName.Font, out var fonts))
            {
                resC[PdfName.Font] = DedupSingle(fonts);
            }
        }

        var pgRef = PdfIndirectRef.Create(pg);
        currentBagArray.Add(pgRef);
        _ctx.WriteIndirectObject(pgRef);
        pageCount++;
        if (currentBagArray.Count >= 25)
        {
            CompleteBag();
            CreateBag();
        }
    }

    private Stream CopyStream(Stream stream)
    {
        if (memoryDedup)
        {
            var ms = new MemoryStream((int)stream.Length);
            stream.Position = 0;
            stream.CopyTo(ms);
            ms.Position = 0;
            return ms;
        } else
        {
            var tempFile = Path.Combine(Path.GetTempPath(), "pdflexer_" + Guid.NewGuid().ToString());
            var fs = new FileStream(tempFile, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.DeleteOnClose);
            stream.Position = 0;
            stream.CopyTo(fs);
            fs.Seek(0, SeekOrigin.Begin);
            return fs;
        }
    }

    private PdfDictionary DedupSingle(PdfDictionary dict)
    {
        var res = dict.CloneShallow();
        foreach (var (k, v) in res)
        {
            DedupSingle(res, k, v);
        }
        return res;
    }
    private void DedupSingle(PdfDictionary parent, PdfName key, IPdfObject v)
    {
        if (!(v is PdfIndirectRef vr))
        {
            return;
        }

        if (_ctx.IsKnown(vr))
        {
            return;
        }

        var hash = hasher.GetHash(v);
        if (refs.TryGetValue(hash, out var xr))
        {
            parent[key] = xr;
        }
        else
        {
            var ir = _ctx.WriteIndirectObject(vr);
            parent[key] = ir;
            hash.Stream = CopyStream(hash.Stream);
            refs[hash] = ir;
        }
    }

    public void Complete(PdfDictionary trailer, PdfDictionary? catalog=null)
    {
        CompleteBag();
        catalog ??= new PdfDictionary();
        catalog[PdfName.TypeName] = PdfName.Catalog;
        var catRef = PdfIndirectRef.Create(catalog);
        trailer[PdfName.Root] = catRef;

        // remove page tree specific items
        trailer.Remove(PdfName.DecodeParms);
        trailer.Remove(PdfName.Filter);
        trailer.Remove(PdfName.Length);
        trailer.Remove(PdfName.Prev);
        trailer.Remove(PdfName.XRefStm);
        if (bags.Count == 1)
        {
            catalog[PdfName.Pages] = bags[0].BagRef;
        } else if (bags.Count > 1)
        {
            CreateBag();
            var count = 0;
            foreach (var (bag, bagRef) in bags)
            {
                count += bag.GetRequiredValue<PdfIntNumber>(PdfName.Count).Value;
                bag[PdfName.Parent] = currentBagRef;
                currentBagArray.Add(bagRef);
            }
            currentBag[PdfName.Count] = new PdfIntNumber(count);
            catalog[PdfName.Pages] = currentBagRef;
            currentBag = null!;
            currentBagRef = null!;
            currentBagArray = null!;
        }
        _ctx.Complete(trailer);
        bags.Clear();
    }

    private void CompleteBag()
    {
        if (currentBag == null) { return; }
        currentBag[PdfName.Count] = new PdfIntNumber(currentBagArray.Count);
        bags.Add((currentBag, currentBagRef));
        currentBag = null!;
        currentBagArray = null!;
        currentBagRef = null!;
    }

    private (PdfDictionary, PdfArray, PdfIndirectRef) CreateBag()
    {
        currentBag = new PdfDictionary();
        currentBagArray = new PdfArray();
        currentBag[PdfName.Kids] = currentBagArray;
        currentBag[PdfName.TypeName] = PdfName.Pages;
        currentBagRef = PdfIndirectRef.Create(currentBag);
        currentBagRef.DeferWriting = true;
        return (currentBag, currentBagArray, currentBagRef);
    }

    public void Dispose()
    {
        foreach (var item in refs?.Keys ?? Enumerable.Empty<PdfStreamHash>())
        {
            item.Stream.Dispose();
        }
        refs?.Clear();
    }
}
