using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer
{
    internal class PdfIndirectObj : IPdfObject
    {
        public PdfIndirectObj(long objectNumber, int generation)
        {
            ObjectNumber = objectNumber;
            Generation = generation;
        }
        public long ObjectNumber { get; }
        public int Generation { get; }
        public bool IsIndirect => false;
        public PdfObjectType Type => PdfObjectType.IndirectRefObj;
    }
}
