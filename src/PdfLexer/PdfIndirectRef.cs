using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer
{
    internal class PdfIndirectRef : IPdfObject
    {
        public PdfIndirectRef(long objectNumber, int generation)
        {
            ObjectNumber = objectNumber;
            Generation = generation;
        }
        public long ObjectNumber { get; }
        public int Generation { get; }
        public bool IsIndirect => false;
        public PdfObjectType Type => PdfObjectType.IndirectRefObj;
    }

    internal class PdfIndirectObj : IPdfObject
    {
        public PdfIndirectObj(long objectNumber, int generation, IPdfObject contents)
        {
            ObjectNumber = objectNumber;
            Generation = generation;
            Contents = contents;
        }
        public long ObjectNumber { get; }
        public int Generation { get; }
        public IPdfObject Contents { get;}
        public bool IsIndirect => true;
        public PdfObjectType Type => PdfObjectType.IndirectObj;
    }
}
