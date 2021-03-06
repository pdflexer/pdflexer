using PdfLexer.Parsers;
using System;
using System.IO;

namespace PdfLexer.Serializers
{
    public class ArraySerializer : ISerializer<PdfArray>
    {
        private WritingContext _ctx;

        public ArraySerializer(WritingContext ctx)
        {
            _ctx = ctx;
        }
        public int GetBytes(PdfArray obj, Span<byte> data)
        {
            throw new NotImplementedException();
        }

        public void WriteToStream(PdfArray obj, Stream stream)
        {
            stream.WriteByte((byte)'[');
            for (var i = 0;i<obj.Count;i++)
            {
                _ctx.SerializeObject(obj[i]);
                if (i+1<obj.Count)
                {
                    var nxt = obj[i+1];
                    if (nxt.Type == PdfObjectType.NameObj || nxt.Type == PdfObjectType.ArrayObj || nxt.Type == PdfObjectType.DictionaryObj
                        || nxt.Type == PdfObjectType.StringObj)
                    {
                        continue;
                    }
                    stream.WriteByte((byte)' ');
                }
            }
            stream.WriteByte((byte)']');
        }
    }
}
