using PdfLexer.Parsers;
using PdfLexer.Parsers.Structure;
using System;
using System.Buffers;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace PdfLexer.Serializers
{
    public class WritingContext
    {
        private static byte[] zeros = new byte[] { (byte)'0', (byte)'0', (byte)'0', (byte)'0', (byte)'0', (byte)'0', (byte)'0', (byte)'0', (byte)'0', (byte)'0' };
        private static byte[] gen0end = new byte[] { (byte)'0', (byte)'0', (byte)'0', (byte)'0', (byte)'0', (byte)' ', (byte)'n', (byte)' ', (byte)'\n' };
        private static byte[] refEnd = new byte[] { (byte)' ', (byte)'n', (byte)' ', (byte)'\n' };
        private static byte[] gen1end = new byte[] { (byte)'0', (byte)'0', (byte)'0', (byte)'0', (byte)'1', (byte)' ', (byte)'f', (byte)' ', (byte)'\n' };
        private static byte[] oef = new byte[] { (byte)'%', (byte)'%', (byte)'E', (byte)'O', (byte)'F', (byte)'\n' };
        private static byte[] obj0 = Encoding.ASCII.GetBytes("0000000000 65535 f \n");

        internal int NewDocId = PdfDocument.GetNextId();
        internal int NextId = 1;
        internal Stream Stream { get; }
        internal Serializers Serializers { get; }

        public WritingContext(Stream stream)
        {
            Stream = stream;
            Serializers = new Serializers();
        }

        internal WritingContext(Stream stream, int nextId, int docId) : this(stream)
        {
            NextId = nextId;
            NewDocId = docId;
        }

        public void Initialize(decimal version)
        {
            Stream.Write(Encoding.ASCII.GetBytes($"%PDF-{version.ToString("0.0", CultureInfo.InvariantCulture)}"));
            Stream.WriteByte((byte)'\n');
            Stream.WriteByte((byte)'%');
            Stream.WriteByte(250);
            Stream.WriteByte(251);
            Stream.WriteByte(252);
            Stream.WriteByte(253);
            Stream.WriteByte((byte)'\n');
        }

        private byte[] miniBuff = new byte[20];
        public void Complete(PdfDictionary trailer)
        {
            completing = true;
            // TODO offset tracking is not very efficient, need to look into fixing.

            // locallize
            reused.Clear();
            Recurse(trailer, reused, true);

            WriteDeferred();

            var mos = writtenObjs.Keys.Max();

            Span<byte> z = zeros;
            Span<byte> buff = miniBuff;
            var os = Stream.Position;
            Stream.Write(XRefParser.xref);
            Stream.WriteByte((byte)'\n');
            Stream.WriteByte((byte)'0');
            Stream.WriteByte((byte)' ');
            if (!Utf8Formatter.TryFormat(mos + 1, buff, out var count))
            {
                throw new ApplicationException("TODO");
            }
            Stream.Write(buff.Slice(0, count));
            Stream.WriteByte((byte)'\n');
            Stream.Write(obj0);


            for (var i = 1; i < mos + 1; i++)
            {
                if (!writtenObjs.TryGetValue(i, out var written))
                {
                    Stream.Write(z);
                    Stream.WriteByte((byte)' ');
                    Stream.Write(gen1end);
                    continue;
                }

                if (!Utf8Formatter.TryFormat(written.OS, buff, out count))
                {
                    throw new ApplicationException("TODO");
                }
                Stream.Write(z.Slice(0, 10 - count));
                Stream.Write(buff.Slice(0, count));
                Stream.WriteByte((byte)' ');
                if (!Utf8Formatter.TryFormat(written.Ref.Generation, buff, out count))
                {
                    throw new ApplicationException("TODO");
                }
                Stream.Write(z.Slice(0, 5 - count));
                Stream.Write(buff.Slice(0, count));
                Stream.Write(refEnd);
            }
            Stream.Write(XRefParser.Trailer);
            Stream.WriteByte((byte)'\n');
            trailer["/Size"] = new PdfIntNumber(mos + 1);
            Serializers.DictionarySerializer.WriteToStream(trailer, Stream, SerializeRef);
            Stream.WriteByte((byte)'\n');
            Stream.Write(XRefParser.startxref);
            Stream.WriteByte((byte)'\n');
            if (!Utf8Formatter.TryFormat(os, buff, out count))
            {
                throw new ApplicationException("TODO");
            }
            Stream.Write(buff.Slice(0, count));
            Stream.WriteByte((byte)'\n');
            Stream.Write(oef);
        }

        internal void SerializeRef(Stream str, PdfIndirectRef ir)
        {
            ir = Resolve(ir);
            Serializers.WriteRefAsIs(str, ir);
        }
        internal void SerializeObject(IPdfObject obj, bool writeAnyIr = false)
        {
            Func<PdfIndirectRef, PdfIndirectRef> resolver = Resolve;
            if (writeAnyIr)
            {
                resolver = ResolveAsIs;
            }

            Serializers.SerializeObject(Stream, obj, resolver);
        }

        private PdfIndirectRef ResolveAsIs(PdfIndirectRef ir) => ir;
        private PdfIndirectRef Resolve(PdfIndirectRef ir)
        {
            if (!ir.IsOwned(NewDocId))
            {
                if (!TryGetLocalRef(ir, out var result))
                {
                    throw new ApplicationException("Attempted to write external indirect reference that had not been localized in this context: " + ir.Reference);
                }
                return result;

            }
            else
            {
                return ir;
            }
        }
        internal void RegisterReplacedObject(int sourceId, XRef eir, IPdfObject obj)
        {
            EnsureCurrentDoc(sourceId);

            var eid = eir.GetId();
            if (currentDictionary.ContainsKey(eid))
            {
                throw new ApplicationException("Attempted to register replacement for already written object");
            }

            if (localizedObjects.TryGetValue(obj, out var local))
            {
                currentDictionary[eid] = local;
            }
            else
            {
                var ir = PdfIndirectRef.Create(obj);
                ir.Reference = new XRef { ObjectNumber = NextId++ };
                ir.SourceId = NewDocId;
                localizedObjects.Add(obj, ir);
            }
        }

        // objects that have been localized
        // can be from external doc but important to track as we need to dedup objects from new xrefs
        private ConditionalWeakTable<IPdfObject, PdfIndirectRef> localizedObjects = new ConditionalWeakTable<IPdfObject, PdfIndirectRef>();

        // objects that have been written
        internal Dictionary<int, (long OS, XRef Ref)> writtenObjs = new Dictionary<int, (long OS, XRef Ref)>();

        // xrefs for external documents that have been localized and have a local xref committed
        private Dictionary<int, Dictionary<ulong, PdfIndirectRef>> localizedXRefs = new Dictionary<int, Dictionary<ulong, PdfIndirectRef>>();
        private int currentExternalId = -1;
        private Dictionary<ulong, PdfIndirectRef> currentDictionary = null;

        private bool TryGetLocalRef(PdfIndirectRef ir, out PdfIndirectRef local)
        {
            if (ir.IsOwned(NewDocId)) // for new xrefs we will own them if unowned
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
                EnsureCurrentDoc(ir.SourceId);
                currentDictionary[id] = local;

                return true;
            }

            EnsureCurrentDoc(ir.SourceId);
            return currentDictionary.TryGetValue(ir.Reference.GetId(), out local);
        }

        public PdfIndirectRef Localize(PdfIndirectRef ir)
        {
            if (ir.IsOwned(NewDocId)) // if unowned this will trigger owning it but won't have ref
            {
                if (ir.Reference.ObjectNumber == 0)
                {
                    var obj = ir.GetObject();
                    if (localizedObjects.TryGetValue(obj, out var existing)) // check if already localized
                    {
                        ir.Reference = existing.Reference;
                        return ir;
                    }
                    else
                    {
                        ir.Reference = new XRef { ObjectNumber = NextId++ };
                    }
                    localizedObjects.Add(obj, ir);
                }
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

            EnsureCurrentDoc(ir.SourceId);

            {
                var id = ir.Reference.GetId();
                if (currentDictionary.TryGetValue(id, out var existing))
                {
                    return existing; // already tracked
                }
                var obj = ir.GetObject();
                var dummy = CreateAndSetDummyPointer(obj);
                currentDictionary[id] = dummy;
                return dummy;
            }
        }

        private PdfIndirectRef CreateAndSetDummyPointer(IPdfObject obj)
        {
            var dummy = PdfIndirectRef.Create(null); // TODO look into this, can maybe hold actual ref
            dummy.SourceId = NewDocId;
            dummy.Reference = new XRef { ObjectNumber = NextId++ };
            localizedObjects.AddOrUpdate(obj, dummy);
            return dummy;
        }
        private void EnsureCurrentDoc(int sourceId)
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
        }

        private HashSet<PdfIndirectRef> reused = new HashSet<PdfIndirectRef>();
        public PdfIndirectRef WriteIndirectObject(PdfIndirectRef ir)
        {
            reused.Clear();
            // reused.Add(ir);
            return WriteIndirectObject(ir, reused, true);
        }

        public void LocalizeIdirect(PdfIndirectRef ir)
        {
            reused.Clear();
            // reused.Add(ir);
            WriteIndirectObject(ir, reused, false);
        }

        internal void WriteExistingData(ParsingContext ctx, XRefEntry entry)
        {
            Debug.Assert(entry.Reference.ObjectNumber != 0);
            writtenObjs[entry.Reference.ObjectNumber] = (Stream.Position, entry.Reference);
            WriteObjStart(entry.Reference);
            entry.CopyUnwrappedData(this);
            WriteObjEnd();
        }

        internal PdfIndirectRef WriteIndirectObject(PdfIndirectRef ir, HashSet<PdfIndirectRef> refStack, bool writeNonDeferred)
        {
            var obj = ir.GetObject();
        Parse:
            switch (obj)
            {
                case PdfLazyObject lz:
                    if (lz.HasLazyIndirect || lz.IsModified)
                    {
                        obj = lz.Resolve();
                        goto Parse; // sorry
                    }
                    break;
                case PdfStream str:
                    Recurse(str.Dictionary, refStack, writeNonDeferred);
                    break;
                case PdfDictionary dict:
                    Recurse(dict, refStack, writeNonDeferred);
                    break;
                case PdfArray array:
                    Recurse(array, refStack, writeNonDeferred);
                    break;
                default:
                    break;
            }
            return WriteCurrentIfNeeded();

            PdfIndirectRef WriteCurrentIfNeeded()
            {
                var local = Localize(ir);
                if (!writeNonDeferred)
                {
                    return local;
                }

                if (writtenObjs.TryGetValue(local.Reference.ObjectNumber, out var info))
                {
                    if (info.Ref.Generation == local.Reference.Generation)
                    {
                        return local;
                    }
                    else if (info.Ref.Generation > local.Reference.Generation)
                    {
                        local.Reference = info.Ref;
                        return local;
                    }
                    // existing less than current, break through and overwrite
                }
                if (obj.Type == PdfObjectType.NullObj) { return local; }
                if (local.DeferWriting && !completing)
                {
                    deferedObjects.Add((obj, local));
                    return local;
                }
                writtenObjs[local.Reference.ObjectNumber] = (Stream.Position, local.Reference);
                WriteObjStart(local.Reference);
                SerializeObject(obj);
                WriteObjEnd();
                return local;
            }
        }
        private bool completing = false;
        private List<(IPdfObject obj, PdfIndirectRef iref)> deferedObjects = new List<(IPdfObject obj, PdfIndirectRef iref)>();

        private void WriteDeferred()
        {
            foreach (var (obj, local) in deferedObjects)
            {
                writtenObjs[local.Reference.ObjectNumber] = (Stream.Position, local.Reference);
                WriteObjStart(local.Reference);
                SerializeObject(obj);
                WriteObjEnd();
            }
        }

        private void Recurse(PdfDictionary obj, HashSet<PdfIndirectRef> refStack, bool writeNonDeferred)
        {
            foreach (var item in obj.Values)
            {
                Debug.Assert(item != null);
                CheckSingle(item, refStack, writeNonDeferred);
            }
        }

        private void Recurse(PdfArray arr, HashSet<PdfIndirectRef> refStack, bool writeNonDeferred)
        {
            for (var i = 0; i < arr.Count; i++)
            {
                Debug.Assert(arr[i] != null);
                CheckSingle(arr[i], refStack, writeNonDeferred);
            }
        }

        private void CheckSingle(IPdfObject obj, HashSet<PdfIndirectRef> refStack, bool writeNonDeferred)
        {
            var toCheck = obj;
            if (obj.IsLazy)
            {
                var lz = (PdfLazyObject)obj;
                if (lz.HasLazyIndirect || lz.IsModified)
                {
                    toCheck = lz.Resolve();
                }
                else
                {
                    return;
                }
            }
            switch (toCheck.Type)
            {
                case PdfObjectType.DictionaryObj:
                    Recurse((PdfDictionary)toCheck, refStack, writeNonDeferred);
                    return;
                case PdfObjectType.ArrayObj:
                    Recurse((PdfArray)toCheck, refStack, writeNonDeferred);
                    return;
                case PdfObjectType.IndirectRefObj:
                    HandleNestedIndirect((PdfIndirectRef)toCheck);
                    return;
            }

            void HandleNestedIndirect(PdfIndirectRef nir)
            {
                if (TryGetLocalRef(nir, out var lr) && writtenObjs.ContainsKey(lr.Reference.ObjectNumber))
                {
                    return;
                }

                if (refStack.Contains(nir))
                {
                    Localize(nir); // we've hit this reference before but this occurence may not be localized
                    return;
                }

                refStack.Add(nir);
                WriteIndirectObject(nir, refStack, writeNonDeferred);
            }
        }

        private void WriteObjStart(XRef xref)
        {
            Debug.Assert(xref.ObjectNumber != 0);
            if (!Utf8Formatter.TryFormat(xref.ObjectNumber, miniBuff, out var written))
            {
                throw new ApplicationException("Unable for write Object number integer: " + xref);
            }
            Stream.Write(miniBuff, 0, written);
            Stream.WriteByte((byte)' ');
            if (!Utf8Formatter.TryFormat(xref.Generation, miniBuff, out written))
            {
                throw new ApplicationException("Unable for write generation integer: " + xref);
            }
            Stream.Write(miniBuff, 0, written);
            Stream.WriteByte((byte)' ');
            Stream.Write(IndirectSequences.obj);
            Stream.WriteByte((byte)'\n');
        }
        private void WriteObjEnd()
        {
            Stream.WriteByte((byte)'\n');
            Stream.Write(IndirectSequences.endobj);
            Stream.WriteByte((byte)'\n');
        }
    }
}
