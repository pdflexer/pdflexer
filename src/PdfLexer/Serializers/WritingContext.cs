using PdfLexer.Parsers;
using PdfLexer.Parsers.Structure;
using System;
using System.Buffers;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace PdfLexer.Serializers
{
    public class WritingContext
    {
        private Dictionary<int, long> offsets = new Dictionary<int, long>();
        internal Dictionary<PdfIndirectRef,PdfIndirectRef> localized {get;} = new Dictionary<PdfIndirectRef, PdfIndirectRef>();
        internal int NewDocId = PdfDocument.GetNextId();
        internal int NextId = 1;
        internal ArraySerializer ArraySerializer { get; }
        internal BoolSerializer BoolSerializer { get; }
        internal DictionarySerializer DictionarySerializer { get; }
        internal NumberSerializer NumberSerializer { get; }
        internal NameSerializer NameSerializer { get; }
        internal StringSerializer StringSerializer { get; }

        public WritingContext()
        {
            ArraySerializer = new ArraySerializer(this);
            BoolSerializer = new BoolSerializer();
            DictionarySerializer = new DictionarySerializer(this);
            NameSerializer = new NameSerializer();
            NumberSerializer = new NumberSerializer();
            StringSerializer = new StringSerializer();
        }

        public void Initialize(decimal version, Stream stream)
        {
            stream.Write(Encoding.ASCII.GetBytes($"%PDF-{version.ToString("0.0", CultureInfo.InvariantCulture)}"));
            stream.WriteByte((byte)'%');
            stream.WriteByte(169);
            stream.WriteByte(205);
            stream.WriteByte(196);
            stream.WriteByte(210);
            stream.WriteByte((byte)'\n');
        }

        public void SerializeObject(IPdfObject obj, Stream stream)
        {
            if (obj == null)
            {
                stream.Write(PdfNull.NullBytes);
                return;
            }
            switch (obj)
            {
                // TODO ? switch parser to take positions for no slice if not needed?
                case PdfArray array:
                    ArraySerializer.WriteToStream(array, stream);
                    return;
                case PdfBoolean bl:
                    BoolSerializer.WriteToStream(bl, stream);
                    return;
                case PdfStream str:
                    DictionarySerializer.WriteToStream(str.Dictionary, stream);
                    stream.WriteByte((byte)'\n');
                    stream.Write(IndirectSequences.stream);
                    stream.WriteByte((byte)'\n');
                    stream.Write(IndirectSequences.endstream);
                    stream.WriteByte((byte)'\n');
                    return;
                case PdfDictionary dict:
                    DictionarySerializer.WriteToStream(dict, stream);
                    return;
                case PdfName name:
                    NameSerializer.WriteToStream(name, stream);
                    return;
                case PdfNumber no:
                    NumberSerializer.WriteToStream(no, stream);
                    return;
                case PdfString str:
                    StringSerializer.WriteToStream(str, stream);
                    return;
                case PdfLazyObject lz:
                    if (lz.HasLazyIndirect || lz.IsModified())
                    {
                        var rz = lz.Resolve();
                        SerializeObject(rz, stream);
                    } else
                    {
                        lz.Source.CopyData(lz.Offset, lz.Length, stream);
                    }
                    return;
                case PdfIndirectRef ir:
                    if (!ir.IsOwned(NewDocId))
                    {
                        if (!localized.TryGetValue(ir, out var result))
                        {
                            throw new ApplicationException("Attempted to write external indirect reference that had not been localized in this context: " + ir.Reference);
                        }
                        WriteObjRef(result.Reference, stream);
                        return;
                    } else
                    {
                        WriteObjRef(ir.Reference, stream);
                    }
                    return;
            }
            throw new NotImplementedException($"Requested to write pdf object of type {obj.GetType()}.");
        }

        private HashSet<PdfIndirectRef> reused = new HashSet<PdfIndirectRef>();
        public void WriteIndirectObject(PdfIndirectRef ir, Stream stream)
        {
            reused.Clear();
            WriteIndirectObject(ir, stream, reused);
        }
        internal void WriteIndirectObject(PdfIndirectRef ir, Stream stream, HashSet<PdfIndirectRef> refStack)
        {
            var obj = ir.GetObject();
Parse:
            switch (obj) 
            {
                case PdfLazyObject lz:
                    if (lz.HasLazyIndirect || lz.IsModified())
                    {
                        obj = lz.Resolve();
                        goto Parse; // ugh
                    }
                    break;
                case PdfStream str:
                    Recurse(str.Dictionary.Values);
                    break;
                case PdfDictionary dict:
                    Recurse(dict.Values);
                    break;
                case PdfArray array:
                    Recurse(array);
                    break;
                default:
                    break;
            }
            WriteCurrent();

            void WriteCurrent()
            {
                var local = Localize(ir);
                offsets[local.Reference.ObjectNumber] = stream.Position;
                WriteObjStart(local.Reference, stream);
                SerializeObject(obj, stream);
                WriteObjEnd(stream);
            }

            void Recurse(IEnumerable<IPdfObject> obj)
            {
                foreach (var item in obj)
                {
                    var toCheck = item;
RecurseCheck:
                    switch (toCheck)
                    {
                        case PdfLazyObject lz:
                            if (lz.HasLazyIndirect || lz.IsModified())
                            {
                                toCheck = lz.Resolve();
                                goto RecurseCheck; // ugh
                            } 
                            continue;
                        case PdfDictionary dd:
                            Recurse(dd.Values);
                            continue;
                        case PdfArray aa:
                            Recurse(aa);
                            continue;
                        case PdfIndirectRef irr:
                            HandleNestedIndirect(irr);
                            continue;
                    }
                }
            }

            void HandleNestedIndirect(PdfIndirectRef nir)
            {
                if (refStack.Contains(nir))
                {
                    Localize(nir);
                } else
                {
                    refStack.Add(ir);
                    WriteIndirectObject(nir, stream, refStack);
                }
            }
        }

        public PdfIndirectRef Localize(PdfIndirectRef ir)
        {
            if (ir.IsOwned(NewDocId))
            {
                if (ir.Reference.ObjectNumber == 0)
                {
                    ir.Reference = new XRef {  ObjectNumber = NextId++ };
                }
                return ir;
            }

            if (localized.TryGetValue(ir, out var val))
            {
                return val; // already localized
            }

            var dummy = PdfIndirectRef.Create(null); // TODO look into this;
            dummy.Reference = new XRef { ObjectNumber = NextId++ };
            localized[ir] = dummy;
            return dummy;
        }
        private void WriteObjStart(XRef xref, Stream stream)
        {
            Debug.Assert(xref.ObjectNumber != 0);
            var buff = ArrayPool<byte>.Shared.Rent(20);
            if (!Utf8Formatter.TryFormat(xref.ObjectNumber, buff, out var written))
            {
                throw new ApplicationException("Unable for write Object number integer: " + xref);
            }
            stream.Write(buff, 0, written);
            stream.WriteByte((byte)' ');
            if (!Utf8Formatter.TryFormat(xref.Generation, buff, out written))
            {
                throw new ApplicationException("Unable for write generation integer: " + xref);
            }
            stream.Write(buff, 0, written);
            ArrayPool<byte>.Shared.Return(buff);
            stream.WriteByte((byte)' ');
            stream.Write(IndirectSequences.obj);
            stream.WriteByte((byte)'\n');
        }
        private void WriteObjEnd(Stream stream)
        {
            stream.WriteByte((byte)'\n');
            stream.Write(IndirectSequences.endobj);
            stream.WriteByte((byte)'\n');
        }
        private void WriteObjRef(XRef xref, Stream stream)
        {
            Debug.Assert(xref.ObjectNumber != 0);
            var buff = ArrayPool<byte>.Shared.Rent(20);
            if (!Utf8Formatter.TryFormat(xref.ObjectNumber, buff, out var written))
            {
                throw new ApplicationException("Unable for write Object number integer: " + xref);
            }
            stream.Write(buff, 0, written);
            stream.WriteByte((byte)' ');
            if (!Utf8Formatter.TryFormat(xref.Generation, buff, out written))
            {
                throw new ApplicationException("Unable for write generation integer: " + xref);
            }
            stream.Write(buff, 0, written);
            stream.WriteByte((byte)' ');
            stream.WriteByte((byte)'R');
            ArrayPool<byte>.Shared.Return(buff);
        }
    }
}
