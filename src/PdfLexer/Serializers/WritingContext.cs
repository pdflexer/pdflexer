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
        private static byte[] gen1end = new byte[] { (byte)'0', (byte)'0', (byte)'0', (byte)'0', (byte)'1', (byte)' ', (byte)'f', (byte)' ', (byte)'\n' };
        private static byte[] oef = new byte[] { (byte)'%', (byte)'%', (byte)'E', (byte)'O', (byte)'F', (byte)'\n' };
        private static byte[] obj0 = Encoding.ASCII.GetBytes("0000000000 65535 f \n");
        internal Dictionary<int, long> offsets = new Dictionary<int, long>();
        internal int NewDocId = PdfDocument.GetNextId();
        internal int NextId = 1;
        internal ArraySerializer ArraySerializer { get; }
        internal BoolSerializer BoolSerializer { get; }
        internal DictionarySerializer DictionarySerializer { get; }
        internal NumberSerializer NumberSerializer { get; }
        internal NameSerializer NameSerializer { get; }
        internal StringSerializer StringSerializer { get; }
        internal Stream Stream { get; }
        public WritingContext(Stream stream)
        {
            Stream = stream;
            ArraySerializer = new ArraySerializer(this);
            BoolSerializer = new BoolSerializer();
            DictionarySerializer = new DictionarySerializer(this);
            NameSerializer = new NameSerializer();
            NumberSerializer = new NumberSerializer();
            StringSerializer = new StringSerializer();
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
            //locallize
            reused.Clear();
            Recurse(trailer, reused);

            var mos = offsets.Keys.Max();

            Span<byte> z = zeros;
            Span<byte> buff = miniBuff;
            var os = Stream.Position;
            Stream.Write(XRefParser.xref);
            Stream.WriteByte((byte)'\n');
            Stream.WriteByte((byte)'0');
            Stream.WriteByte((byte)' ');
            if (!Utf8Formatter.TryFormat(mos+1, buff, out var count))
            {
                throw new ApplicationException("TODO");
            }
            Stream.Write(buff.Slice(0, count));
            Stream.WriteByte((byte)'\n');
            Stream.Write(obj0);
            
            
            for (var i = 1; i < mos+1; i++)
            {
                if (!offsets.TryGetValue(i, out var oos))
                {
                    Stream.Write(z);
                    Stream.WriteByte((byte)' ');
                    Stream.Write(gen1end);
                    continue;
                }

                if (!Utf8Formatter.TryFormat(oos, buff, out count))
                {
                    throw new ApplicationException("TODO");
                }
                Stream.Write(z.Slice(0, 10-count));
                Stream.Write(buff.Slice(0, count));
                Stream.WriteByte((byte)' ');
                Stream.Write(gen0end);
            }
            Stream.Write(XRefParser.trailer);
            Stream.WriteByte((byte)'\n');
            trailer["/Size"] = new PdfIntNumber(mos+1);
            DictionarySerializer.WriteToStream(trailer, Stream);
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

        public void SerializeObject(IPdfObject obj)
        {
            if (obj.IsLazy)
            {
                var lz = (PdfLazyObject)obj;
                if (lz.HasLazyIndirect || lz.IsModified)
                {
                    obj = lz.Resolve();
                } else
                {
                    lz.Source.CopyData(lz.Offset, lz.Length, Stream);
                    return;
                }
            }

            switch (obj.Type)
            {
                case PdfObjectType.ArrayObj:
                    ArraySerializer.WriteToStream((PdfArray)obj, Stream);
                    return;
                case PdfObjectType.NullObj:
                    Stream.Write(PdfNull.NullBytes);
                    return;
                case PdfObjectType.BooleanObj:
                    BoolSerializer.WriteToStream((PdfBoolean)obj, Stream);
                    return;
                case PdfObjectType.StreamObj:
                    var str = (PdfStream)obj;
                    DictionarySerializer.WriteToStream(str.Dictionary, Stream);
                    Stream.WriteByte((byte)'\n');
                    Stream.Write(IndirectSequences.stream);
                    Stream.WriteByte((byte)'\n');
                    str.Contents.CopyRawContents(Stream);
                    Stream.WriteByte((byte)'\n');
                    Stream.Write(IndirectSequences.endstream);
                    return;
                case PdfObjectType.DictionaryObj:
                    DictionarySerializer.WriteToStream((PdfDictionary)obj, Stream);
                    return;
                case PdfObjectType.NameObj:
                    NameSerializer.WriteToStream((PdfName)obj, Stream);
                    return;
                case PdfObjectType.NumericObj:
                    NumberSerializer.WriteToStream((PdfNumber)obj, Stream);
                    return;
                case PdfObjectType.StringObj:
                    StringSerializer.WriteToStream((PdfString)obj, Stream);
                    return;
                case PdfObjectType.IndirectRefObj:
                    var ir = (PdfIndirectRef)obj;
                    if (!ir.IsOwned(NewDocId))
                    {
                        if (!TryGetLocalRef(ir, out var result))
                        {
                            throw new ApplicationException("Attempted to write external indirect reference that had not been localized in this context: " + ir.Reference);
                        }
                        WriteObjRef(result.Reference);
                        return;
                    } else
                    {
                        WriteObjRef(ir.Reference);
                    }
                    return;

            }
            throw new NotImplementedException($"Requested to write pdf object of type {obj.GetType()}.");
        }

        // TODO can we just track SourcedXRef by the ConditionalWeakTable as well?
        private ConditionalWeakTable<IPdfObject, PdfIndirectRef> localizedObjects = new ConditionalWeakTable<IPdfObject, PdfIndirectRef>();
        private Dictionary<int, Dictionary<ulong, PdfIndirectRef>> localizedXRefs = new Dictionary<int, Dictionary<ulong, PdfIndirectRef>>();
        private int currentExternalId = -1;
        private Dictionary<ulong, PdfIndirectRef> currentDictionary = null;

        private bool TryGetLocalRef(PdfIndirectRef ir, out PdfIndirectRef local)
        {
            if (ir.SourceId == NewDocId)
            {
                local = ir;
                return true;
            }

            if (ir is NewIndirectRef nir)
            {
                return localizedObjects.TryGetValue(nir.Object, out local);
            }

            if (currentExternalId != ir.SourceId)
            {
                if (!localizedXRefs.TryGetValue(ir.SourceId, out currentDictionary))
                {
                    local = null;
                    return false;
                }
                currentExternalId = ir.SourceId;
            }
            ulong id = ir.Reference.GetId();
            return currentDictionary.TryGetValue(id, out local);
            throw new NotImplementedException("TODO");
        }

        public PdfIndirectRef Localize(PdfIndirectRef ir)
        {
            if (ir.IsOwned(NewDocId))
            {
                if (ir.Reference.ObjectNumber == 0)
                {
                    var obj = ir.GetObject();
                    if (localizedObjects.TryGetValue(obj, out var existing))
                    {
                        ir.Reference = existing.Reference;
                        return ir;
                    } else
                    {
                        ir.Reference = new XRef {  ObjectNumber = NextId++ };
                    }
                    localizedObjects.Add(obj, ir);
                }
                return ir;
            }

            if (ir is NewIndirectRef nir)
            {
                var obj = nir.Object;
                if (localizedObjects.TryGetValue(obj, out var existing))
                {
                    return existing;
                }
                var dummy = PdfIndirectRef.Create(null); // TODO look into this;
                dummy.SourceId = NewDocId;
                dummy.Reference = new XRef { ObjectNumber = NextId++ };
                localizedObjects.Add(obj, dummy);
                return dummy;
            }


            if (currentExternalId != ir.SourceId)
            {
                if (!localizedXRefs.TryGetValue(ir.SourceId, out currentDictionary))
                {
                    currentDictionary = new Dictionary<ulong, PdfIndirectRef>();
                    localizedXRefs[ir.SourceId] = currentDictionary;
                }
                currentExternalId = ir.SourceId;
            }
            {
                ulong id = ((ulong)ir.Reference.ObjectNumber << 16) | ((uint)ir.Reference.Generation & 0xFFFF);
                if (currentDictionary.TryGetValue(id, out var existing)) {
                    return existing;
                }
                var obj = ir.GetObject();
                var dummy = PdfIndirectRef.Create(null); // TODO look into this;
                dummy.SourceId = NewDocId;
                dummy.Reference = new XRef { ObjectNumber = NextId++ };
                localizedObjects.AddOrUpdate(obj, dummy);
                currentDictionary[id] = dummy;
                return dummy;
            }

            throw new NotImplementedException("TODO");
        }

        private HashSet<PdfIndirectRef> reused = new HashSet<PdfIndirectRef>();
        public PdfIndirectRef WriteIndirectObject(PdfIndirectRef ir)
        {
            reused.Clear();
            return WriteIndirectObject(ir, reused);
        }

        internal void WriteExistingData(ParsingContext ctx, XRefEntry entry)
        {
            Debug.Assert(entry.Reference.ObjectNumber != 0);
            offsets[entry.Reference.ObjectNumber] = Stream.Position;
            WriteObjStart(entry.Reference);
            entry.CopyUnwrappedData(Stream);
            WriteObjEnd();
        }

        int test = 0;
        internal PdfIndirectRef WriteIndirectObject(PdfIndirectRef ir, HashSet<PdfIndirectRef> refStack)
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
                    Recurse(str.Dictionary, refStack);
                    break;
                case PdfDictionary dict:
                    Recurse(dict, refStack);
                    break;
                case PdfArray array:
                    Recurse(array, refStack);
                    break;
                default:
                    break;
            }
            return WriteCurrentIfNeeded();

            PdfIndirectRef WriteCurrentIfNeeded()
            {
                var local = Localize(ir);
                if (offsets.ContainsKey(local.Reference.ObjectNumber))
                {
                    return local;
                }
                offsets[local.Reference.ObjectNumber] = Stream.Position;
                WriteObjStart(local.Reference);
                SerializeObject(obj);
                WriteObjEnd();
                return local;
            }
        }

        private void Recurse(PdfDictionary obj, HashSet<PdfIndirectRef> refStack)
        {
            foreach (var item in obj.Values)
            {
                Debug.Assert(item != null);
                CheckSingle(item, refStack);
            }
        }

        private void Recurse(PdfArray arr, HashSet<PdfIndirectRef> refStack)
        {
            for (var i = 0; i < arr.Count; i++)
            {
                Debug.Assert(arr[i] != null);
                CheckSingle(arr[i], refStack);
            }
        }

        private void CheckSingle(IPdfObject obj, HashSet<PdfIndirectRef> refStack)
        {
            var toCheck = obj;
            if (obj.IsLazy)
            {
                var lz = (PdfLazyObject)obj;
                if (lz.HasLazyIndirect || lz.IsModified)
                {
                    toCheck = lz.Resolve();
                } else
                {
                    return;
                }
            }
            switch (toCheck.Type)
            {
                case PdfObjectType.DictionaryObj:
                    Recurse((PdfDictionary)toCheck, refStack);
                    return;
                case PdfObjectType.ArrayObj:
                    Recurse((PdfArray)toCheck, refStack);
                    return;
                case PdfObjectType.IndirectRefObj:
                    HandleNestedIndirect((PdfIndirectRef)toCheck);
                    return;
            }

            void HandleNestedIndirect(PdfIndirectRef nir)
            {
                if (TryGetLocalRef(nir, out _))
                {
                    return;
                }
                if (refStack.Contains(nir))
                {
                    Localize(nir);
                } else
                {
                    refStack.Add(nir);
                    WriteIndirectObject(nir, refStack);
                }
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
        private void WriteObjRef(XRef xref)
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
            Stream.WriteByte((byte)'R');
        }
    }
}
