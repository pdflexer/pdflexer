using PdfLexer.DOM;
using System.Globalization;
using System.IO.MemoryMappedFiles;
using System.Text;
using DotNext.IO.MemoryMappedFiles;

namespace PdfLexer.Serializers;

public class StreamingWriter : IDisposable
{
    private readonly WritingContext _ctx;
    private readonly bool dedup;
    private readonly bool memoryDedup;
    private readonly bool disposeOnComplete;
    private readonly TreeHasher hasher;
    private readonly Dictionary<PdfStreamHash, PdfIndirectRef> refs;
    private decimal maxVersion = 1.0m;

    public decimal MaxPageVersion { get => maxVersion; }

    public StreamingWriter(Stream stream, bool dedupXobj, bool inMemoryDedup, bool disposeOnComplete = false)
    {
        _ctx = new WritingContext(stream);
        _ctx.Initialize(1.4m); // start with 1.4, minimum version where catalog version overrides header
                               // max version is tracked from each page and then added to catalog
        (currentBag, currentBagArray, currentBagRef) = CreateBag();
        dedup = dedupXobj;
        memoryDedup = inMemoryDedup;
        this.disposeOnComplete = disposeOnComplete;
        if (dedup)
        {
            hasher = new TreeHasher();
            refs = new Dictionary<PdfStreamHash, PdfIndirectRef>(new FNVStreamComparison());
        }
        else
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
        if (page.SourceVersion != null && page.SourceVersion > maxVersion)
        {
            maxVersion = page.SourceVersion.Value;
        }

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
        var written = new WrittenIndirectRef(_ctx.WriteIndirectObject(pgRef));
        currentBagArray.Add(written);
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
        }
        else
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

    public void Complete(PdfDictionary trailer, PdfDictionary? catalog = null)
    {
        CompleteBag();
        catalog ??= new PdfDictionary();
        catalog[PdfName.TypeName] = PdfName.Catalog;
        if (maxVersion > 1.4m)
        {
            catalog[PdfName.Version] = new PdfName(maxVersion.ToString("0.0", CultureInfo.InvariantCulture), false);
        }
        var catRef = PdfIndirectRef.Create(catalog);
        trailer[PdfName.Root] = catRef;

        // remove page tree specific items
        trailer.Remove(PdfName.DecodeParms);
        trailer.Remove(PdfName.Filter);
        trailer.Remove(PdfName.Length);
        trailer.Remove(PdfName.Prev);
        trailer.Remove(PdfName.XRefStm);

        catalog[PdfName.Pages] = CreateTree();
        _ctx.Complete(trailer);
        bags.Clear();
        if (disposeOnComplete)
        {
            _ctx.Stream.Dispose();
        }
    }

    private PdfIndirectRef CreateTree()
    {
        var levels = (int)Math.Ceiling((Math.Log(bags.Count * 25) / Math.Log(5)) - 2);

        for (var i = 0; i < levels; i++)
        {
            var orig = bags.ToList();
            bags.Clear();
            IEnumerable<(PdfDictionary Bag, PdfIndirectRef BagRef)> current = orig;
            while ((current = orig.Take(5)).Any())
            {
                orig = orig.Skip(5).ToList();
                var count = 0;
                CreateBag(false);
                foreach (var (bag, bagRef) in current)
                {
                    count += bag.GetRequiredValue<PdfIntNumber>(PdfName.Count).Value;
                    bag[PdfName.Parent] = currentBagRef;
                    currentBagArray.Add(bagRef);
                }
                CompleteBag(count);
            }
        }

        if (bags.Count == 1)
        {
            return bags[0].BagRef;
        }
        else if (bags.Count > 1)
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
            var result = currentBagRef;
            currentBag = null!;
            currentBagRef = null!;
            currentBagArray = null!;
            return result;
        }
        else
        {
            CreateBag();
            currentBag[PdfName.Count] = new PdfIntNumber(0);
            return currentBagRef;
        }
    }

    private void CompleteBag(int? count = null)
    {
        if (currentBag == null) { return; }
        count ??= currentBagArray.Count;
        currentBag[PdfName.Count] = new PdfIntNumber(count.Value);
        bags.Add((currentBag, currentBagRef));
        currentBag = null!;
        currentBagArray = null!;
        currentBagRef = null!;
    }

    private (PdfDictionary, PdfArray, PdfIndirectRef) CreateBag(bool completed=false)
    {
        currentBag = new PdfDictionary();
        currentBagArray = new PdfArray();
        currentBag[PdfName.Kids] = currentBagArray;
        currentBag[PdfName.TypeName] = PdfName.Pages;
        currentBagRef = PdfIndirectRef.Create(currentBag);
        if (!completed) // will still add items to this bag and it's ref'd by pages as parent
        {
            currentBagRef.DeferWriting = true;
        }
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


#if NET6_0_OR_GREATER

    public static void UpdateFileHeaderVersion(string filePath, decimal value)
    {
        using var mm = MemoryMappedFile.CreateFromFile(filePath, FileMode.Open);
        using var da = mm.CreateDirectAccessor(0, 9, MemoryMappedFileAccess.ReadWrite);
        var data = da.Bytes;
        ReadOnlySpan<byte> header = Encoding.ASCII.GetBytes("%PDF-" + value.ToString("0.0", CultureInfo.InvariantCulture) + '\n');
        header.Slice(0, 9).CopyTo(data);
    }

#endif
}
