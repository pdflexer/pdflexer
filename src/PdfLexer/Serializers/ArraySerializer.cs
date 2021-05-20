using PdfLexer.Parsers;
using System;
using System.IO;

namespace PdfLexer.Serializers
{
    public class ArraySerializer //: ISerializer<PdfArray>
    {
        private Serializers _serializers;

        public ArraySerializer(Serializers serializers)
        {
            _serializers = serializers;
        }
        public int GetBytes(PdfArray obj, Span<byte> data)
        {
            throw new NotImplementedException();
        }

        public void WriteToStream(PdfArray obj, Stream stream, Func<PdfIndirectRef,PdfIndirectRef> resolver)
        {
            stream.WriteByte((byte)'[');
            for (var i = 0;i<obj.Count;i++)
            {
                _serializers.SerializeObject(stream, obj[i], resolver);
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
