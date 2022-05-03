using PdfLexer.DOM;
using System.Collections.Generic;
using System.IO;

namespace PdfLexer.Serializers
{
    public class StreamingWriter
    {
        private readonly WritingContext _ctx;
        private readonly bool dedup;
        private readonly TreeHasher hasher;
        private readonly Dictionary<string, PdfIndirectRef> refs;


        public StreamingWriter(Stream stream, bool dedupXobj)
        {
            _ctx = new WritingContext(stream);
            _ctx.Initialize(1.7m);
            CreateBag();
            dedup = dedupXobj;
            if (dedup)
            {
                hasher = new TreeHasher();
                refs = new Dictionary<string, PdfIndirectRef>();
            }
        }
        public StreamingWriter(Stream stream) : this(stream, false) { }

        private List<(PdfDictionary Bag, PdfIndirectRef BagRef)> bags = new List<(PdfDictionary Bag, PdfIndirectRef BagRef)>();

        private PdfDictionary currentBag;
        private PdfArray currentBagArray;
        private PdfIndirectRef currentBagRef;


        public void AddPage(PdfPage page)
        {
            var pg = page.Dictionary.CloneShallow();
            WritingUtil.RemovedUnusedLinks(pg, ir => false);
            pg[PdfName.Parent] = currentBagRef;

            // 
            if (dedup 
                && pg.TryGetValue<PdfDictionary>(PdfName.Resources, out var res) 
                && res.TryGetValue<PdfDictionary>(PdfName.XObject, out var xobjs)
                )
            {
                var resC = res.CloneShallow();
                var xC = xobjs.CloneShallow();
                resC[PdfName.XObject] = xC;
                pg[PdfName.Resources] = resC;
                foreach (var (k,v) in xC)
                {
                    if (!(v is PdfIndirectRef vr)) {
                        continue;
                    }
                    
                    if (_ctx.IsKnown(vr))
                    {
                        continue;
                    }

                    var hash = hasher.GetHash(v);
                    if (refs.TryGetValue(hash, out var xr))
                    {
                        xC[k] = xr;
                    } else
                    {
                        var ir = _ctx.WriteIndirectObject(vr);
                        xC[k] = ir;
                        refs[hash] = ir;
                    }
                }
            }

            var pgRef = PdfIndirectRef.Create(pg);
            currentBagArray.Add(pgRef);
            _ctx.WriteIndirectObject(pgRef);
            if (currentBagArray.Count >= 25)
            {
                CompleteBag();
                CreateBag();
            }
        }

        // dedup
        // Type XObject (optional) // Subtype Form (required


        public void Complete(PdfDictionary trailer)
        {
            CompleteBag();
            var catalog = new PdfDictionary();
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
                currentBag = null;
                currentBagRef = null;
                currentBagArray = null;
            }
            _ctx.Complete(trailer);
            bags.Clear();
        }

        private void CompleteBag()
        {
            if (currentBag == null) { return; }
            currentBag[PdfName.Count] = new PdfIntNumber(currentBagArray.Count);
            bags.Add((currentBag, currentBagRef));
            currentBag = null;
            currentBagArray = null;
            currentBagRef = null;
        }
        private void CreateBag()
        {
            currentBag = new PdfDictionary();
            currentBagArray = new PdfArray();
            currentBag[PdfName.Kids] = currentBagArray;
            currentBag[PdfName.TypeName] = PdfName.Pages;
            currentBagRef = PdfIndirectRef.Create(currentBag);
            currentBagRef.DeferWriting = true;
        }
    }
}
