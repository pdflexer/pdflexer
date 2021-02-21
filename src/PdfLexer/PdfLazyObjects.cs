﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using PdfLexer.IO;

namespace PdfLexer
{
    internal interface IParsedLazyObj : IPdfObject
    {
        public bool IsModified { get; }
        public bool HasLazyIndirect { get; }
        public PdfLazyObject Wrapper { get; }
    }

    internal class PdfLazyObject : PdfObject
    {
        public override PdfObjectType Type => LazyObjectType;
        public PdfObjectType LazyObjectType { get; set; }
        public bool HasLazyIndirect { get; set; }
        public long Offset {get; set; }
        public int Length {get; set; }
        public IPdfDataSource Source { get; set; }
        public IParsedLazyObj Parsed { get; set; }
    }
}
