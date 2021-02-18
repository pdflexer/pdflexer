using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace PdfLexer.Parsers
{
    public class DictionaryParser : IParser<PdfDictionary>
    {
        private readonly ParsingContext _ctx;

        public DictionaryParser(ParsingContext ctx)
        {
            _ctx = ctx;
        }

        public PdfDictionary Parse(ReadOnlySpan<byte> buffer) => Parse(buffer, 0, buffer.Length);

        public PdfDictionary Parse(ReadOnlySpan<byte> buffer, int start, int length)
        {
            // TODO better lazy support
            var dict = _ctx.NestedSpanParser.ParseNestedItem(null, 0, buffer, start) as PdfDictionary;
            foreach (var item in dict)
            {
                if (item.Value is PdfLazyObject lz)
                {
                    var replacement = _ctx.GetKnownPdfItem(lz.Type, buffer, (int)lz.Offset, lz.Length);
                    dict[item.Key] = replacement;
                }
            }
            return dict;
        }

        public PdfDictionary Parse(in ReadOnlySequence<byte> sequence)
        {
            while (_ctx.NestedSeqParser.ParseNestedItem(sequence, true)) { }
            var dict = _ctx.NestedSeqParser.GetCompletedObject() as PdfDictionary;
            return dict;
        }

        public PdfDictionary Parse(in ReadOnlySequence<byte> sequence, long start, int length)
        {
            Debug.Assert(sequence.Length > start + length, "sequence.Length > start + length PdfDictionary seg parse.");
            var sliced = sequence.Slice(start);
            return Parse(in sliced);
        }
    }
}
