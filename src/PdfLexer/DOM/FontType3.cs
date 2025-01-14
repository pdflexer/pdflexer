﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PdfLexer.DOM
{

    public class FontType3 : IPdfFont, ISimpleUnicode
    {
        public PdfDictionary NativeObject { get; }

        public FontType3()
        {
            NativeObject = new PdfDictionary();
            NativeObject[PdfName.TypeName] = PdfName.Font;
            NativeObject[PdfName.Subtype] = PdfName.Type3;
        }

        public FontType3(PdfDictionary dict)
        {
            NativeObject = dict;
        }

        public static implicit operator FontType3(PdfDictionary dict) => new FontType3(dict);
        public static implicit operator PdfDictionary(FontType3 page) => page.NativeObject;

        /// <summary>
        /// optional
        /// </summary>
        public PdfName? Name
        {
            get => NativeObject.Get<PdfName>(PdfName.Name);
            set => NativeObject.Set(PdfName.Name, value);
        }

        /// <summary>
        /// required
        /// </summary>
        public PdfRectangle? FontBBox
        {
            get => NativeObject?.Get<PdfArray>(PdfName.FontBBox);
            set => NativeObject.Set(PdfName.FontBBox, value?.NativeObject);
        }

        /// <summary>
        /// required
        /// </summary>
        public PdfArray? FontMatrix
        {
            get => NativeObject?.Get<PdfArray>(PdfName.FontMatrix);
            set => NativeObject.Set(PdfName.FontMatrix, value);
        }

        /// <summary>
        /// required
        /// </summary>
        public PdfDictionary? CharProcs
        {
            get => NativeObject?.Get<PdfDictionary>(PdfName.CharProcs);
            set => NativeObject.Set(PdfName.CharProcs, value);
        }

        /// <summary>
        /// optional
        /// either name of PDF encoding set
        /// or dictionary w/ differences (FontEncoding DOM object)
        /// </summary>
        public IPdfObject? Encoding
        {
            get => NativeObject.Get(PdfName.Encoding);
            set => NativeObject.Set(PdfName.Encoding, value);
        }

        /// <summary>
        /// req
        /// Integer
        /// Corresponds to first char in fonts widths
        /// </summary>
        public PdfNumber? FirstChar
        {
            get => NativeObject.Get<PdfNumber>(PdfName.FirstChar);
            set => NativeObject.Set(PdfName.FirstChar, value);
        }

        /// <summary>
        /// req
        /// Integer
        /// Corresponds to last char in fonts widths
        /// </summary>
        public PdfNumber? LastChar
        {
            get => NativeObject.Get<PdfNumber>(PdfName.LastChar);
            set => NativeObject.Set(PdfName.LastChar, value);
        }

        /// <summary>
        /// req
        /// Indirect preferred
        /// </summary>
        public PdfArray? Widths
        {
            get => NativeObject.Get<PdfArray>(PdfName.Widths);
            set => NativeObject.Set(PdfName.Widths, value.Indirect());
        }

        /// <summary>
        /// req non 14
        /// </summary>
        public FontDescriptor? FontDescriptor
        {
            get => NativeObject.Get<PdfDictionary>(PdfName.FontDescriptor);
            set => NativeObject.Set(PdfName.FontDescriptor, value?.NativeObject.Indirect());
        }


        /// <summary>
        /// optional but should be used
        /// </summary>
        public PdfDictionary? Resources
        {
            get => NativeObject?.Get<PdfDictionary>(PdfName.Resources);
            set => NativeObject.Set(PdfName.Resources, value);
        }


        public PdfStream? ToUnicode
        {
            get => NativeObject.Get<PdfStream>(PdfName.ToUnicode);
            set => NativeObject.Set(PdfName.ToUnicode, value.Indirect());
        }

        PdfName? ISimpleUnicode.FontName => Name;

        PdfNumber? ISimpleUnicode.MissingWidth => FontDescriptor?.MissingWidth;

        float ISimpleUnicode.ScaleFactor
        { get
            {
                if (FontMatrix != null)
                {
                    var sx = (float)FontMatrix[0].GetAs<PdfNumber>();
                    return 1 / sx;
                }
                return 1000f;
            } 
        }
    }
}
