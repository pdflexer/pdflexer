using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer.DOM
{
    public class FontType1
    {
        public PdfDictionary NativeObject { get; }

        public FontType1()
        {
            NativeObject = new PdfDictionary();
            NativeObject[PdfName.TypeName] = PdfName.Font;
            NativeObject[PdfName.Subtype] = PdfName.Type1;
        }

        public FontType1(PdfDictionary dict)
        {
            NativeObject = dict;
        }

        public static implicit operator FontType1(PdfDictionary dict) => new FontType1(dict);
        public static implicit operator PdfDictionary(FontType1 page) => page.NativeObject;

        /// <summary>
        /// required
        /// </summary>
        public PdfName BaseFont
        {
            get => NativeObject.Get<PdfName>(PdfName.BaseFont);
            set => NativeObject[PdfName.BaseFont] = value;
        }

        /// <summary>
        /// req non 14
        /// </summary>
        public PdfNumber FirstChar
        {
            get => NativeObject.Get<PdfNumber>(PdfName.FirstChar);
            set => NativeObject[PdfName.FirstChar] = value;
        }

        /// <summary>
        /// req non 14
        /// </summary>
        public PdfNumber LastChar
        {
            get => NativeObject.Get<PdfNumber>(PdfName.LastChar);
            set => NativeObject[PdfName.LastChar] = value;
        }

        /// <summary>
        /// req non 14
        /// </summary>
        public PdfArray Widths
        {
            get => NativeObject.Get<PdfArray>(PdfName.Widths);
            set => NativeObject[PdfName.Widths] = value;
        }

        /// <summary>
        /// req non 14
        /// </summary>
        public FontDescriptor FontDescriptor
        {
            get => NativeObject.Get<PdfDictionary>(PdfName.FontDescriptor);
            set => NativeObject[PdfName.FontDescriptor] = value.NativeObject.Indirect();
        }

        /// <summary>
        /// optional
        /// either name of PDF encoding set
        /// or dictionary w/ differences (FontEncoding DOM object)
        /// </summary>
        public IPdfObject Encoding
        {
            get => NativeObject.Get(PdfName.Encoding);
            set => NativeObject[PdfName.Encoding] = value;
        }

        public PdfStream ToUnicode
        {
            get => NativeObject.Get<PdfStream>(PdfName.ToUnicode);
            set => NativeObject[PdfName.ToUnicode] = value.Indirect();
        }
    }
}
