using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer.DOM
{
    public interface IPdfFont
    {
        PdfDictionary NativeObject { get; }
    }

    public interface ISimpleUnicode
    {
        PdfDictionary NativeObject { get; }
        PdfName FontName { get; }
        PdfNumber FirstChar { get; }
        PdfNumber LastChar { get; }
        PdfArray Widths { get; }
        PdfNumber MissingWidth { get; }
        PdfRectangle FontBBox { get; }
        PdfStream ToUnicode { get; }
        float ScaleFactor { get; }

    }
    public class FontType1 : IPdfFont, ISimpleUnicode
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
        /// Always the value of FontName in the font program
        /// </summary>
        public PdfName BaseFont
        {
            get => NativeObject.Get<PdfName>(PdfName.BaseFont);
            set => NativeObject[PdfName.BaseFont] = value;
        }

        /// <summary>
        /// req non 14
        /// Integer
        /// Corresponds to first char in fonts widths
        /// </summary>
        public PdfNumber FirstChar
        {
            get => NativeObject.Get<PdfNumber>(PdfName.FirstChar);
            set => NativeObject[PdfName.FirstChar] = value;
        }

        /// <summary>
        /// req non 14
        /// Integer
        /// Corresponds to last char in fonts widths
        /// </summary>
        public PdfNumber LastChar
        {
            get => NativeObject.Get<PdfNumber>(PdfName.LastChar);
            set => NativeObject[PdfName.LastChar] = value;
        }

        /// <summary>
        /// req non 14
        /// Indirect preferred
        /// 1000 to 1 ratio with text space
        /// </summary>
        public PdfArray Widths
        {
            get => NativeObject.Get<PdfArray>(PdfName.Widths);
            set => NativeObject[PdfName.Widths] = value.Indirect();
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

        PdfName ISimpleUnicode.FontName => BaseFont;

        PdfNumber ISimpleUnicode.MissingWidth => FontDescriptor?.MissingWidth;

        PdfRectangle ISimpleUnicode.FontBBox => FontDescriptor?.FontBBox;

        public float ScaleFactor => 1000f;
    }
}
