using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace PdfLexer.Parsers
{
    public class DictionaryParser : Parser<PdfDictionary>
    {
        private readonly ParsingContext _ctx;

        public DictionaryParser(ParsingContext ctx)
        {
            _ctx = ctx;
        }

        public override PdfDictionary Parse(ReadOnlySpan<byte> buffer) => Parse(buffer, 0, buffer.Length);

        public override PdfDictionary Parse(ReadOnlySpan<byte> buffer, int start, int length)
        {
            var dict = _ctx.NestedParser.ParseNestedItem(buffer, start, out _) as PdfDictionary;
            return dict;
        }
    }
}
