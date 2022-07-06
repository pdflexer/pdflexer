using PdfLexer.Lexing;
using PdfLexer.Parsers;
using PdfLexer.Parsers.Structure;
using PdfLexer.Serializers;
using System;
using System.IO;

namespace PdfLexer.IO
{
    internal class StreamDataSource : StreamBase
    {
        public StreamDataSource(ParsingContext ctx, Stream stream) : base(ctx, stream, true) { }

        public override IPdfObject GetIndirectObject(XRefEntry xref)
            => this.ReadWrappedFromStream(xref);

        public override void CopyIndirectObject(XRefEntry xref, WritingContext destination)
            => this.WriteWrappedFromStream(xref, destination.Stream);

    }
}
