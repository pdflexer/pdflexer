using PdfLexer.Parsers.Structure;
using System.Runtime.CompilerServices;

namespace PdfLexer.Serializers;

internal class RefTracker
{
    internal int ThisDocId;
    internal int NextId = 1;

    // objects that have been localized
    // can be from external doc but important to track as we need to dedup objects from new xrefs
    private ConditionalWeakTable<IPdfObject, PdfIndirectRef> localizedObjects = new ConditionalWeakTable<IPdfObject, PdfIndirectRef>();

    // xrefs for external documents that have been localized and have a local xref committed
    private Dictionary<int, Dictionary<ulong, PdfIndirectRef>> localizedXRefs = new Dictionary<int, Dictionary<ulong, PdfIndirectRef>>();
    private int currentExternalId = -1;
    private Dictionary<ulong, PdfIndirectRef>? currentDictionary = null;

    public RefTracker()
    {
         ThisDocId = PdfDocument.GetNextId();
    }

    public RefTracker(int docId, int nextId)
    {
        ThisDocId = docId;
        NextId = nextId;
    }

    public void Reset()
    {
        ThisDocId = PdfDocument.GetNextId();
        NextId = 1;
        currentExternalId = -1;
        currentDictionary = null;
        localizedXRefs.Clear();
        localizedObjects.Clear();
    }

    public bool TryGetLocalRef(PdfIndirectRef ir, out PdfIndirectRef local, bool attemptOwnership)
    {
        if (TryLocalizeExisting(ir, attemptOwnership))
        {
            local = ir;
            return true;
        }

        var obj = ir.GetObject();
        if (localizedObjects.TryGetValue(obj, out local))
        {
            if (ir is NewIndirectRef)
            {
                return true;
            }

            var id = ir.Reference.GetId();
            var current = EnsureCurrentDoc(ir.SourceId);
            current[id] = local;

            return true;
        }

        {
            var current = EnsureCurrentDoc(ir.SourceId);
            return current.TryGetValue(ir.Reference.GetId(), out local);
        }

    }

    public bool IsTracked(PdfIndirectRef ir) => TryGetLocalRef(ir, out var _, false);

    private bool TryLocalizeExisting(PdfIndirectRef ir, bool attemptOwnership)
    {
        if (!ir.IsOwned(ThisDocId, attemptOwnership))
        {
            return false;
        }

        // if unowned this will trigger owning it but won't have ref
        if (ir.Reference.ObjectNumber == 0)
        {
            var obj = ir.GetObject();
            if (localizedObjects.TryGetValue(obj, out var existing)) // check if already localized
            {
                ir.Reference = existing.Reference;
                return true;
            }
            else
            {
                ir.Reference = new XRef { ObjectNumber = NextId++ };
            }
            localizedObjects.Add(obj, ir);
        }
        return true;
    }

    public PdfIndirectRef Localize(PdfIndirectRef ir)
    {
        if (TryLocalizeExisting(ir, true))
        {
            return ir;
        }

        if (ir is NewIndirectRef nir) // owned by someone else, have to track solely through lookup
        {
            var obj = nir.GetObject();
            if (localizedObjects.TryGetValue(obj, out var existing))
            {
                return existing;
            }

            // first time seeing, just through dummy into tracker
            var dummy = CreateAndSetDummyPointer(obj);
            return dummy;
        }

        var current = EnsureCurrentDoc(ir.SourceId);

        {
            var id = ir.Reference.GetId();
            if (current.TryGetValue(id, out var existing))
            {
                return existing; // already tracked
            }
            var obj = ir.GetObject();
            var dummy = CreateAndSetDummyPointer(obj);
            current[id] = dummy;
            return dummy;
        }
    }

    private HashSet<PdfIndirectRef> reused = new HashSet<PdfIndirectRef>();
    public PdfIndirectRef LocalizeRecursive(PdfIndirectRef ir)
    {
        reused.Clear();
        CommonUtil.Recurse(ir.GetObject(), reused, x => false, (r, ir) =>
          {
              if (r.Type == PdfObjectType.IndirectRefObj)
              {
                  Localize((PdfIndirectRef)r);
              }
          });
        return Localize(ir);
    }

    public void LocalizeRecursive(IPdfObject obj, bool ordered=false)
    {
        CommonUtil.Recurse(obj, reused, x => false, (r, ir) =>
        {
            if (r.Type == PdfObjectType.IndirectRefObj)
            {
                Localize((PdfIndirectRef)r);
            }
        }, ordered);
    }


    private PdfIndirectRef CreateAndSetDummyPointer(IPdfObject obj)
    {
        var dummy = PdfIndirectRef.Create(PdfNull.Value); // TODO look into this, can maybe hold actual ref
        dummy.SourceId = ThisDocId;
        dummy.Reference = new XRef { ObjectNumber = NextId++ };
        localizedObjects.AddOrUpdate(obj, dummy);
        return dummy;
    }
    private Dictionary<ulong, PdfIndirectRef> EnsureCurrentDoc(int sourceId)
    {
        if (currentExternalId != sourceId)
        {
            if (!localizedXRefs.TryGetValue(sourceId, out currentDictionary))
            {
                currentDictionary = new Dictionary<ulong, PdfIndirectRef>();
                localizedXRefs[sourceId] = currentDictionary;
            }
            currentExternalId = sourceId;
        }

        if (currentDictionary == null)
        {
            currentDictionary = new Dictionary<ulong, PdfIndirectRef>();
            localizedXRefs[currentExternalId] = currentDictionary;
        }
        return currentDictionary;
    }
}
