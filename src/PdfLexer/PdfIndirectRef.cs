using PdfLexer.Parsers;
using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer
{
    internal class PdfIndirectRef : IPdfObject
    {
        public PdfIndirectRef(ParsingContext ctx, long objectNumber, int generation)
        {
            Context = ctx;
            ObjectNumber = objectNumber;
            Generation = generation;
        }
        public ParsingContext Context { get; }
        public long ObjectNumber { get; }
        public int Generation { get; }
        public bool IsIndirect => false;
        public PdfObjectType Type => PdfObjectType.IndirectRefObj;
    }

    internal class IndirectRefToken : IPdfObject
    {
        public static IndirectRefToken Value => new IndirectRefToken();
        public bool IsIndirect => throw new NotImplementedException();

        public PdfObjectType Type => throw new NotImplementedException();
    }
}
