using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer.DOM
{
    public class PdfPage
    {
        public PdfDictionary Dictionary { get; }
        internal ExistingIndirectRef SourceRef { get; }

        internal PdfPage(PdfDictionary page, ExistingIndirectRef ir)
        {
            Dictionary = page;
            SourceRef = ir;
        }

        public PdfPage(PdfDictionary page)
        {
            Dictionary = page;
            page[PdfName.TypeName] = PdfName.Page;
        }

        public PdfPage() : this(new PdfDictionary())
        {
        }

        public static implicit operator PdfPage(PdfDictionary dict) => new PdfPage(dict);
        public static implicit operator PdfDictionary(PdfPage page) => page.Dictionary;

        public PdfRectangle MediaBox { get => Dictionary.GetOrCreateValue<PdfArray>(PdfName.MediaBox); }

        public void AddXObj(PdfName nm, IPdfObject xobj)
        {
            if (xobj.Type != PdfObjectType.IndirectRefObj) { xobj = PdfIndirectRef.Create(xobj); }
            Dictionary.GetOrCreateValue<PdfDictionary>(PdfName.Resources).GetOrCreateValue<PdfDictionary>(PdfName.XObject)[nm] = xobj;
        }

    }


    // inheritable
    // Resources required (dictionary)
    // MediaBox required (rectangle)
    // CropBox => default to MediaBox (rectangle)
    // Rotate (integer)


    // notes
    // Bleedbox => default to CropBox
    // TrimBox => default to CropBox
    // ArtBox => default to CropBox
}
