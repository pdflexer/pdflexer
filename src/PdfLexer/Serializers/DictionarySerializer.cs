using PdfLexer.Parsers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PdfLexer.Serializers
{
    public class DictionarySerializer // : ISerializer<PdfDictionary>
    {
        private Serializers _serializers;

        public DictionarySerializer(Serializers serializers)
        {
            _serializers = serializers;
        }
        public int GetBytes(PdfDictionary obj, Span<byte> data)
        {
            throw new NotImplementedException();
        }

        public void WriteToStream(PdfDictionary obj, Stream stream) => WriteToStream(obj, stream, _serializers.WriteRefAsIs);

        public void WriteToStream(PdfDictionary obj, Stream stream, Action<Stream, PdfIndirectRef> handler)
        {
            stream.WriteByte((byte)'<');
            stream.WriteByte((byte)'<');
            foreach (var item in obj)
            {
                _serializers.SerializeObject(stream, item.Key, handler);
                if (item.Value.Type != PdfObjectType.NameObj && item.Value.Type != PdfObjectType.StringObj && item.Value.Type != PdfObjectType.ArrayObj && item.Value.Type != PdfObjectType.DictionaryObj)
                {
                    stream.WriteByte((byte)' ');
                }
                _serializers.SerializeObject(stream, item.Value, handler);
            }
            stream.WriteByte((byte)'>');
            stream.WriteByte((byte)'>');
        }
    }
}
