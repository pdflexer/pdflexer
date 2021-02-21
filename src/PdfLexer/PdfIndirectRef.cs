﻿using System;
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

    internal class IndirectRefToken : IPdfObject
    {
        public static IndirectRefToken Value => new IndirectRefToken();
        public bool IsIndirect => throw new NotImplementedException();

        public PdfObjectType Type => throw new NotImplementedException();
    }
}
