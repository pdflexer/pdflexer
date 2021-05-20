using PdfLexer.Parsers;
using PdfLexer.Parsers.Structure;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace PdfLexer.Serializers
{
    public class Serializers
    {
        internal ArraySerializer ArraySerializer { get; }
        internal BoolSerializer BoolSerializer { get; }
        internal DictionarySerializer DictionarySerializer { get; }
        internal NumberSerializer NumberSerializer { get; }
        internal NameSerializer NameSerializer { get; }
        internal StringSerializer StringSerializer { get; }

        public Serializers()
        {
            ArraySerializer = new ArraySerializer(this);
            BoolSerializer = new BoolSerializer();
            DictionarySerializer = new DictionarySerializer(this);
            NameSerializer = new NameSerializer();
            NumberSerializer = new NumberSerializer();
            StringSerializer = new StringSerializer();
        }

        public void SerializeObject(Stream stream, IPdfObject obj, Func<PdfIndirectRef,PdfIndirectRef> resolver)
        {
            if (obj.IsLazy)
            {
                var lz = (PdfLazyObject)obj;
                if (lz.HasLazyIndirect || lz.IsModified)
                {
                    obj = lz.Resolve();
                }
                else
                {
                    lz.Source.CopyData(lz.Offset, lz.Length, stream);
                    return;
                }
            }

            switch (obj.Type)
            {
                case PdfObjectType.ArrayObj:
                    ArraySerializer.WriteToStream((PdfArray)obj, stream, resolver);
                    return;
                case PdfObjectType.NullObj:
                    stream.Write(PdfNull.NullBytes);
                    return;
                case PdfObjectType.BooleanObj:
                    BoolSerializer.WriteToStream((PdfBoolean)obj, stream);
                    return;
                case PdfObjectType.StreamObj:
                    var str = (PdfStream)obj;
                    str.Contents.UpdateStreamDictionary(str.Dictionary);
                    DictionarySerializer.WriteToStream(str.Dictionary, stream, resolver);
                    stream.WriteByte((byte)'\n');
                    stream.Write(IndirectSequences.stream);
                    stream.WriteByte((byte)'\n');
                    str.Contents.CopyEncodedData(stream);
                    stream.WriteByte((byte)'\n');
                    stream.Write(IndirectSequences.endstream);
                    return;
                case PdfObjectType.DictionaryObj:
                    DictionarySerializer.WriteToStream((PdfDictionary)obj, stream, resolver);
                    return;
                case PdfObjectType.NameObj:
                    NameSerializer.WriteToStream((PdfName)obj, stream);
                    return;
                case PdfObjectType.NumericObj:
                    NumberSerializer.WriteToStream((PdfNumber)obj, stream);
                    return;
                case PdfObjectType.StringObj:
                    StringSerializer.WriteToStream((PdfString)obj, stream);
                    return;
                case PdfObjectType.IndirectRefObj:
                    var ir = (PdfIndirectRef)obj;
                    var resolved = resolver(ir);
                    WriteObjRef(stream, resolved.Reference);
                    return;

            }
            throw new NotImplementedException($"Requested to write pdf object of type {obj.GetType()}.");
        }

        private byte[] miniBuff = new byte[20];
        private void WriteObjRef(Stream stream, XRef xref)
        {
            Debug.Assert(xref.ObjectNumber != 0);
            if (!Utf8Formatter.TryFormat(xref.ObjectNumber, miniBuff, out var written))
            {
                throw new ApplicationException("Unable for write Object number integer: " + xref);
            }
            stream.Write(miniBuff, 0, written);
            stream.WriteByte((byte)' ');
            if (!Utf8Formatter.TryFormat(xref.Generation, miniBuff, out written))
            {
                throw new ApplicationException("Unable for write generation integer: " + xref);
            }
            stream.Write(miniBuff, 0, written);
            stream.WriteByte((byte)' ');
            stream.WriteByte((byte)'R');
        }
    }
}
