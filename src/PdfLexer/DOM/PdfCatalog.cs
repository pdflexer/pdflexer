using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer.DOM
{
    public class PdfCatalog : PdfDictionary
    {
        public PdfDictionary Dictionary { get; }
        public PdfCatalog()
        {
            Dictionary = new PdfDictionary();
            Dictionary[PdfName.TypeName] = PdfName.Catalog;
        }

        // public PdfIndirectRef Pages
        // {
        //     get => 
        // }
        // Type -> catalog req
        // Pages xref req
        // 
    }
}
