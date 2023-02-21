using PdfLexer.Filters;
using PdfLexer.Parsers;
using PdfLexer.Parsers.Structure;
using System.Buffers.Text;
using System.Diagnostics;

namespace PdfLexer.Serializers;

internal class Serializers
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
        SerializeObject(stream, obj, HandleWithResolver);

        void HandleWithResolver(Stream str, PdfIndirectRef ir)
        {
            var resolved = resolver(ir);
            WriteObjRef(str, resolved.Reference);
        }
    }

    public void SerializeObject(Stream stream, IPdfObject obj, Action<Stream, PdfIndirectRef> handler)
    {
        if (obj.IsLazy)
        {
            var lz = (PdfLazyObject)obj;
            if (lz.HasLazyIndirect || lz.IsModified || lz.Source.Disposed) // lz.Source.Context.Options.ForceSerialize
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
                ArraySerializer.WriteToStream((PdfArray)obj, stream, handler);
                return;
            case PdfObjectType.NullObj:
                stream.Write(PdfNull.NullBytes);
                return;
            case PdfObjectType.BooleanObj:
                BoolSerializer.WriteToStream((PdfBoolean)obj, stream);
                return;
            case PdfObjectType.StreamObj:
                var str = (PdfStream)obj;
                if (str.Contents.IsEncrypted)
                {
                    var copied = new FlateWriter();
                    using var ds = str.Contents.GetDecodedStream();
                    ds.CopyTo(copied);
                    var dict = str.Dictionary.CloneShallow();
                    str = new PdfStream(dict, copied.Complete());
                }
                str.Contents.UpdateStreamDictionary(str.Dictionary);
                DictionarySerializer.WriteToStream(str.Dictionary, stream, handler);
                stream.WriteByte((byte)'\n');
                stream.Write(IndirectSequences.stream);
                stream.WriteByte((byte)'\n');
                str.Contents.CopyEncodedData(stream);
                stream.WriteByte((byte)'\n');
                stream.Write(IndirectSequences.endstream);
                return;
            case PdfObjectType.DictionaryObj:
                DictionarySerializer.WriteToStream((PdfDictionary)obj, stream, handler);
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
                handler(stream, ir);
                return;

        }
        throw new NotImplementedException($"Requested to write pdf object of type {obj.GetType()}.");
    }

    internal void WriteRefAsIs(Stream stream, PdfIndirectRef ir) => WriteObjRef(stream, ir.Reference);
    private void WriteObjRef(Stream stream, XRef xref)
    {
        Span<byte> miniBuff = stackalloc byte[20];
        Debug.Assert(xref.ObjectNumber != 0);
        if (!Utf8Formatter.TryFormat(xref.ObjectNumber, miniBuff, out var written))
        {
            throw new ApplicationException("Unable for write Object number integer: " + xref);
        }
        stream.Write(miniBuff.Slice(0, written));
        stream.WriteByte((byte)' ');
        if (!Utf8Formatter.TryFormat(xref.Generation, miniBuff, out written))
        {
            throw new ApplicationException("Unable for write generation integer: " + xref);
        }
        stream.Write(miniBuff.Slice(0, written));
        stream.WriteByte((byte)' ');
        stream.WriteByte((byte)'R');
    }
}
