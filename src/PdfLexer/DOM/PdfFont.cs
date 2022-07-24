using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer.DOM
{
    public class PdfFont
    {
        public PdfDictionary NativeObject { get; }
        public PdfFont(PdfDictionary dict)
        {
            NativeObject = dict;
        }
        

        public static implicit operator PdfFont(PdfDictionary dict) => new PdfFont(dict);
        public static implicit operator PdfDictionary(PdfFont fnt) => fnt.NativeObject;
    }
}
