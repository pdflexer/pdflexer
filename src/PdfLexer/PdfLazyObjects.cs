using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PdfLexer
{
    internal interface IParsedLazyObj : IPdfObject
    {
        public bool IsModified { get; }
        public bool HasLazyIndirect { get; }
        public PdfLazyObject Wrapper { get; }
    }

    internal class PdfLazyObject : IPdfObject
    {
        public bool IsIndirect { get; set; }
        public bool HasLazyIndirect { get; set; }
        public PdfObjectType Type { get; set; }
        public IPdfDataSource Source { get; set; }
        public IParsedLazyObj Parsed { get; set; }
        public void WriteObject(Stream stream)
        {
            if (Parsed?.IsModified ?? false)
            {
                Parsed.WriteObject(stream);
            }
            else
            {
                Source.CopyData(this, stream);
            }
        }
    }
}
