using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer.DOM
{
    public class PdfTrailer
    {
        public PdfDictionary Dictionary { get; }
        public PdfTrailer()
        {
            Dictionary = new PdfDictionary();
        }
        // Size -> total xrefs
        // Root xref
        // Encrypt (if encrypted)
        // Info (optional) xref
        // ID required if encrypted, two byte strings
    }
}
