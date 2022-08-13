using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PdfLexer.DOM
{

    public class FontType0 : IPdfFont
    {
        public PdfDictionary NativeObject { get; }

        public FontType0()
        {
            NativeObject = new PdfDictionary();
            NativeObject[PdfName.TypeName] = PdfName.Font;
            NativeObject[PdfName.Subtype] = PdfName.Type0;
        }

        public FontType0(PdfDictionary dict)
        {
            NativeObject = dict;
        }

        public static implicit operator FontType0(PdfDictionary dict) => new FontType0(dict);
        public static implicit operator PdfDictionary(FontType0 page) => page.NativeObject;


        /// <summary>
        /// required
        /// If type 2 cidfont name should be CIDFonts BaseFont name
        /// If type 0 concat CIDFont BaseFont name "-" and the CMap name in encoding entry
        /// </summary>
        public PdfName BaseFont
        {
            get => NativeObject.Get<PdfName>(PdfName.BaseFont);
            set => NativeObject[PdfName.BaseFont] = value;
        }


        /// <summary>
        /// required
        /// either name of predefined cmap or stream 
        /// of CMap. If truetype and not embedded, this must be a 
        /// predefined CMap
        /// </summary>
        public IPdfObject Encoding
        {
            get => NativeObject.Get(PdfName.Encoding);
            set => NativeObject[PdfName.Encoding] = value;
        }

        /// <summary>
        /// required
        /// One element array containing CIDFont dictionary
        /// of descendant
        /// </summary>
        public PdfArray DescendantFonts
        {
            get => NativeObject.Get<PdfArray>(PdfName.DescendantFonts);
            set => NativeObject[PdfName.DescendantFonts] = value;
        }

        /// <summary>
        /// Descendant CIDFont
        /// </summary>
        public FontCID DescendantFont
        {
            get => NativeObject.Get<PdfArray>(PdfName.DescendantFonts)?.FirstOrDefault()?.GetAs<PdfDictionary>();
            set => NativeObject[PdfName.DescendantFonts] = new PdfArray(new List<IPdfObject> { value.NativeObject.Indirect() });
        }

        /// <summary>
        /// Optional
        /// </summary>
        public PdfStream ToUnicode
        {
            get => NativeObject.Get<PdfStream>(PdfName.ToUnicode);
            set => NativeObject[PdfName.ToUnicode] = value.Indirect();
        }
    }
}
