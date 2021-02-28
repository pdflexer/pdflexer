using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer.DOM
{
    public class PdfPage
    {
        public PdfDictionary Dictionary { get; }
        public PdfPage(PdfDictionary page)
        {
            Dictionary = page;
        }

        public static implicit operator PdfPage(PdfDictionary dict) => new PdfPage(dict);
        public static implicit operator PdfDictionary(PdfPage page) => page.Dictionary;
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
