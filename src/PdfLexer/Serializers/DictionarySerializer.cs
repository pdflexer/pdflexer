using PdfLexer.Parsers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PdfLexer.Serializers
{
    public class DictionarySerializer : ISerializer<PdfDictionary>
    {
        private ParsingContext _ctx;

        public DictionarySerializer(ParsingContext ctx)
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
            // TODO only write spaces if needed
            foreach (var item in obj)
            {
                _ctx.SerializeObject(item.Key, stream);
                stream.WriteByte((byte)' ');
                _ctx.SerializeObject(item.Value, stream);
                stream.WriteByte((byte)' ');
            }
            stream.WriteByte((byte)'>');
            stream.WriteByte((byte)'>');
        }
    }
}
