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
        private byte[] zeros = new byte[] { (byte)'0', (byte)'0', (byte)'0', (byte)'0', (byte)'0', (byte)'0', (byte)'0', (byte)'0', (byte)'0', (byte)'0' };
        private byte[] gen0end = new byte[] { (byte)'0', (byte)'0', (byte)'0', (byte)'0', (byte)'0', (byte)' ', (byte)'n', (byte)' ', (byte)'\n' };
        private byte[] oef = new byte[] { (byte)'%', (byte)'%', (byte)'E', (byte)'O', (byte)'F', (byte)'\n' };
        private byte[] obj0 = Encoding.ASCII.GetBytes("0000000000 65535 f \n");
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

        public void Complete(PdfDictionary trailer)
        {
            //locallize
            reused.Clear();
            Recurse(trailer.Values, reused);

            Span<byte> z = zeros;
            
            var rented = ArrayPool<byte>.Shared.Rent(20);
            Span<byte> buff = rented;
            //Stream.WriteByte((byte)'\n');
            var os = Stream.Position;
            Stream.Write(XRefParser.xref);
            Stream.WriteByte((byte)'\n');
            Stream.WriteByte((byte)'0');
            Stream.WriteByte((byte)' ');
            if (!Utf8Formatter.TryFormat(offsets.Count+1, buff, out var count))
            {
                throw new ApplicationException("TODO");
            }
            Stream.Write(buff.Slice(0, count));
            Stream.WriteByte((byte)'\n');
            Stream.Write(obj0);
            for (var i = 0; i < offsets.Count; i++)
            {
                var oos = offsets[i+1];
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
            trailer["/Size"] = new PdfIntNumber(offsets.Count+1);
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
            ArrayPool<byte>.Shared.Return(rented);
        }

        public void SerializeObject(IPdfObject obj)
        {
            if (obj == null)
            {
                Stream.Write(PdfNull.NullBytes);
                return;
            }
            switch (obj)
            {
                case PdfArray array:
                    ArraySerializer.WriteToStream(array, Stream);
                    return;
                case PdfBoolean bl:
                    BoolSerializer.WriteToStream(bl, Stream);
                    return;
                case PdfStream str:
                    DictionarySerializer.WriteToStream(str.Dictionary, Stream);
                    Stream.WriteByte((byte)'\n');
                    Stream.Write(IndirectSequences.stream);
                    Stream.WriteByte((byte)'\n');
                    str.Contents.CopyRawContents(Stream);
                    Stream.WriteByte((byte)'\n');
                    Stream.Write(IndirectSequences.endstream);
                    return;
                case PdfDictionary dict:
                    DictionarySerializer.WriteToStream(dict, Stream);
                    return;
                case PdfName name:
                    NameSerializer.WriteToStream(name, Stream);
                    return;
                case PdfNumber no:
                    NumberSerializer.WriteToStream(no, Stream);
                    return;
                case PdfString str:
                    StringSerializer.WriteToStream(str, Stream);
                    return;
                case PdfLazyObject lz:
                    if (lz.HasLazyIndirect || lz.IsModified())
                    {
                        var rz = lz.Resolve();
                        SerializeObject(rz);
                    } else
                    {
                        lz.Source.CopyData(lz.Offset, lz.Length, Stream);
                    }
                    return;
                case PdfIndirectRef ir:
                    if (!ir.IsOwned(NewDocId))
                    {
                        if (!localized.TryGetValue(ir, out var result))
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

        private HashSet<PdfIndirectRef> reused = new HashSet<PdfIndirectRef>();
        public PdfIndirectRef WriteIndirectObject(PdfIndirectRef ir)
        {
            reused.Clear();
            return WriteIndirectObject(ir, reused);
        }
        internal PdfIndirectRef WriteIndirectObject(PdfIndirectRef ir, HashSet<PdfIndirectRef> refStack)
        {
            var obj = ir.GetObject();
Parse:
            switch (obj) 
            {
                case PdfLazyObject lz:
                    if (lz.HasLazyIndirect || lz.IsModified())
                    {
                        obj = lz.Resolve();
                        goto Parse; // sorry
                    }
                    break;
                case PdfStream str:
                    Recurse(str.Dictionary.Values, refStack);
                    break;
                case PdfDictionary dict:
                    Recurse(dict.Values, refStack);
                    break;
                case PdfArray array:
                    Recurse(array, refStack);
                    break;
                default:
                    break;
            }
            return WriteCurrent();

            PdfIndirectRef WriteCurrent()
            {
                var local = Localize(ir);
                offsets[local.Reference.ObjectNumber] = Stream.Position;
                WriteObjStart(local.Reference);
                SerializeObject(obj);
                WriteObjEnd();
                return local;
            }

            
        }

        private void Recurse(IEnumerable<IPdfObject> obj, HashSet<PdfIndirectRef> refStack)
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
                            goto RecurseCheck; // sorry
                        } 
                        continue;
                    case PdfDictionary dd:
                        Recurse(dd.Values,refStack);
                        continue;
                    case PdfArray aa:
                        Recurse(aa, refStack);
                        continue;
                    case PdfIndirectRef irr:
                        HandleNestedIndirect(irr);
                        continue;
                }
            }

            void HandleNestedIndirect(PdfIndirectRef nir)
            {
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
        private void WriteObjStart(XRef xref)
        {
            Debug.Assert(xref.ObjectNumber != 0);
            var buff = ArrayPool<byte>.Shared.Rent(20);
            if (!Utf8Formatter.TryFormat(xref.ObjectNumber, buff, out var written))
            {
                throw new ApplicationException("Unable for write Object number integer: " + xref);
            }
            Stream.Write(buff, 0, written);
            Stream.WriteByte((byte)' ');
            if (!Utf8Formatter.TryFormat(xref.Generation, buff, out written))
            {
                throw new ApplicationException("Unable for write generation integer: " + xref);
            }
            Stream.Write(buff, 0, written);
            ArrayPool<byte>.Shared.Return(buff);
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
            var buff = ArrayPool<byte>.Shared.Rent(20);
            if (!Utf8Formatter.TryFormat(xref.ObjectNumber, buff, out var written))
            {
                throw new ApplicationException("Unable for write Object number integer: " + xref);
            }
            Stream.Write(buff, 0, written);
            Stream.WriteByte((byte)' ');
            if (!Utf8Formatter.TryFormat(xref.Generation, buff, out written))
            {
                throw new ApplicationException("Unable for write generation integer: " + xref);
            }
            Stream.Write(buff, 0, written);
            Stream.WriteByte((byte)' ');
            Stream.WriteByte((byte)'R');
            ArrayPool<byte>.Shared.Return(buff);
        }
    }
}
