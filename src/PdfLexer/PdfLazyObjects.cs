using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using PdfLexer.IO;

namespace PdfLexer
{
    /// <summary>
    /// Wrapper object providing access to an unparsed PDF object.
    /// </summary>
    internal class PdfLazyObject : PdfObject
    {
        public override bool IsLazy => true;
        public override PdfObjectType Type => LazyObjectType;
        public PdfObjectType LazyObjectType { get; set; }
        public bool HasLazyIndirect { get; set; }
        public long Offset {get; set; }
        public int Length {get; set; }
        public IPdfDataSource Source { get; set; }
        public IPdfObject Parsed { get; set; }
        public IPdfObject Resolve()
        {
            if (Parsed != null)
            {
                return Parsed;
            }
            Source.FillData(Offset, Length, out var buffer);
            Parsed = Source.Context.GetKnownPdfItem(LazyObjectType, buffer, 0, Length);
            return Parsed;
        }
    }
}
