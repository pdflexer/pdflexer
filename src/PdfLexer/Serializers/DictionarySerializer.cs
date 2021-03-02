using PdfLexer.Parsers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PdfLexer.Serializers
{
    public class DictionarySerializer : ISerializer<PdfDictionary>
    {
        private WritingContext _ctx;

        public DictionarySerializer(WritingContext ctx)
        {
            _ctx = ctx;
        }
        public int GetBytes(PdfDictionary obj, Span<byte> data)
        {
            throw new NotImplementedException();
        }

        public void WriteToStream(PdfDictionary obj, Stream stream)
        {
            stream.WriteByte((byte)'<');
            stream.WriteByte((byte)'<');
            foreach (var item in obj)
            {
                _ctx.SerializeObject(item.Key);
                if (item.Value.Type != PdfObjectType.NameObj && item.Value.Type != PdfObjectType.StringObj && item.Value.Type != PdfObjectType.ArrayObj && item.Value.Type != PdfObjectType.DictionaryObj)
                {
                        stream.WriteByte((byte)' ');
                }
                _ctx.SerializeObject(item.Value);
            }
            stream.WriteByte((byte)'>');
            stream.WriteByte((byte)'>');
        }
    }
}
