using PdfLexer.Parsers.Nested;
using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer.Parsers
{
    public class ArrayParser : Parser<PdfArray>
    {
        private ParsingContext _ctx;

        public ArrayParser(ParsingContext ctx)
        {
            _ctx = ctx;
        }
        public override PdfArray Parse(ReadOnlySpan<byte> buffer)
        {
            return _ctx.NestedParser.ParseNestedItem(buffer, 0, out _) as PdfArray;
        }
    }
}
