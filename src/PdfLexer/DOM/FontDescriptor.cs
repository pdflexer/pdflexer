using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer.DOM
{
    public class FontDescriptor
    {
        public PdfDictionary NativeObject { get; }

        public FontDescriptor()
        {
            NativeObject = new PdfDictionary();
            NativeObject[PdfName.TypeName] = PdfName.FontDescriptor;
        }

        public FontDescriptor(PdfDictionary dict)
        {
            NativeObject = dict;
        }

        [return: NotNullIfNotNull("dict")]
        public static implicit operator FontDescriptor?(PdfDictionary? dict) => dict == null ? null : new FontDescriptor(dict);
        [return: NotNullIfNotNull("obj")]
        public static implicit operator PdfDictionary?(FontDescriptor? obj) => obj?.NativeObject;

        /// <summary>
        /// required 
        /// </summary>
        public PdfName? FontName
        {
            get => NativeObject?.Get<PdfName>(PdfName.FontName);
            set => NativeObject.Set(PdfName.FontName, value);
        }

        /// <summary>
        /// optional but should be used in Type 3 fonts in Tagged PDFs 
        /// </summary>
        public PdfString? FontFamily
        {
            get => NativeObject?.Get<PdfString>(PdfName.FontFamily);
            set => NativeObject.Set(PdfName.FontFamily, value);
        }

        /// <summary>
        /// optional but should be used in Type 3 fonts in Tagged PDFs
        /// TODO enum
        /// UltraCondensed, ExtraCondensed, Condensed, SemiCondensed, Normal, SemiExpanded
        /// Expanded, ExtraExpanded, UltraExpanded
        /// </summary>
        public PdfString? FontStretch
        {
            get => NativeObject?.Get<PdfString>(PdfName.FontFamily);
            set => NativeObject.Set(PdfName.FontFamily, value);
        }


        /// <summary>
        /// optional but should be used in Type 3 fonts in Tagged PDFs
        /// 100, 200, 300, 400, 500, 600, 700, 800, 900
        /// 400 -> normal, 700 -> bold
        /// </summary>
        public PdfString? FontWeight
        {
            get => NativeObject?.Get<PdfString>(PdfName.FontWeight);
            set => NativeObject.Set(PdfName.FontWeight, value);
        }

        /// <summary>
        /// required
        /// </summary>
        public FontFlags? Flags
        {
            get
            {
                var v = NativeObject?.Get<PdfIntNumber>(PdfName.Flags);
                if (v != null)
                {
                    return (FontFlags)(int)v;
                }
                return null;
            }
            set
            {
                if (value.HasValue)
                {
                    NativeObject[PdfName.Flags] = (PdfNumber)(int)value;
                } else
                {
                    NativeObject[PdfName.Flags] = null!;
                }
            }
        }

        /// <summary>
        /// required except for type 3
        /// </summary>
        public PdfRectangle? FontBBox
        {
            get => NativeObject?.Get<PdfArray>(PdfName.FontBBox);
            set => NativeObject.Set(PdfName.FontBBox, (PdfArray?)value);
        }

        /// <summary>
        /// required
        /// </summary>
        public PdfNumber? ItalicAngle
        {
            get => NativeObject?.Get<PdfNumber>(PdfName.ItalicAngle);
            set => NativeObject.Set(PdfName.ItalicAngle, value);
        }

        /// <summary>
        /// required except for type 3
        /// </summary>
        public PdfNumber? Ascent
        {
            get => NativeObject?.Get<PdfNumber>(PdfName.Ascent);
            set => NativeObject.Set(PdfName.Ascent, value);
        }

        /// <summary>
        /// required except for type 3
        /// </summary>
        public PdfNumber? Descent
        {
            get => NativeObject?.Get<PdfNumber>(PdfName.Descent);
            set => NativeObject.Set(PdfName.Descent, value);
        }

        /// <summary>
        /// Spacing between consecutive lines of text, default 0
        /// </summary>
        public PdfNumber? Leading
        {
            get => NativeObject?.Get<PdfNumber>(PdfName.Leading) ?? 0;
            set => NativeObject.Set(PdfName.Leading, value);
        }

        /// <summary>
        /// required for fonts with Latin chars except for type 3
        /// vertical coordinate of the top of flat capital letters measured
        /// from the baseline
        /// </summary>
        public PdfNumber? CapHeight
        {
            get => NativeObject?.Get<PdfNumber>(PdfName.CapHeight);
            set => NativeObject.Set(PdfName.CapHeight, value);
        }

        /// <summary>
        /// Vertical coordinate of the top of flat nonscending lowercase letters
        /// measured from baseline
        /// default 0
        /// </summary>
        public PdfNumber? XHeight
        {
            get => NativeObject?.Get<PdfNumber>(PdfName.XHeight) ?? 0;
            set => NativeObject.Set(PdfName.XHeight, value);
        }

        /// <summary>
        /// required except for type 3 fonts
        /// thickness of dominant vertical stems measured horizontally
        /// </summary>
        public PdfNumber? StemV
        {
            get => NativeObject?.Get<PdfNumber>(PdfName.StemV) ?? 0;
            set => NativeObject.Set(PdfName.StemV, value);
        }

        /// <summary>
        /// optional
        /// thickness of dominant horizontal stems measured vertically
        /// </summary>
        public PdfNumber? StemH
        {
            get => NativeObject?.Get<PdfNumber>(PdfName.StemH) ?? 0;
            set => NativeObject.Set(PdfName.StemH, value);
        }

        // TODO
        // AvgWidth
        // MaxWidth

        /// <summary>
        /// optional
        /// </summary>
        public PdfNumber? MissingWidth
        {
            get => NativeObject?.Get<PdfNumber>(PdfName.MissingWidth) ?? 0;
            set => NativeObject.Set(PdfName.MissingWidth, value);
        }

        /// <summary>
        /// optional
        /// embedded type 1 front
        /// </summary>
        public PdfStream? FontFile
        {
            get => NativeObject?.Get<PdfStream>(PdfName.FontFile);
            set => NativeObject.Set(PdfName.FontFile, value.Indirect());
        }

        /// <summary>
        /// optional
        /// embedded truetype font
        /// </summary>
        public PdfStream? FontFile2
        {
            get => NativeObject?.Get<PdfStream>(PdfName.FontFile2);
            set => NativeObject.Set(PdfName.FontFile2, value.Indirect());
        }

        /// <summary>
        /// optional
        /// embedded font, type specified in Subtype of stream dict 
        /// </summary>
        public PdfStream? FontFile3
        {
            get => NativeObject?.Get<PdfStream>(PdfName.FontFile3);
            set => NativeObject.Set(PdfName.FontFile3, value.Indirect());
        }

        /// <summary>
        /// optional
        /// meaningful only for type 1 fonts for subsetting
        /// </summary>
        public PdfString? CharSet
        {
            get => NativeObject?.Get<PdfString>(PdfName.CharSet);
            set => NativeObject.Set(PdfName.CharSet, value);
        }
    }

    [Flags]
    public enum FontFlags
    {
        FixedPitch = 1,
        Serif = 2,
        Symbolic = 4,
        Script = 8,
        Nonsymbolic = 32,
        Italic = 64,
        AllCap = 65536,
        SmallCap = 131072,
        ForceBold = 262144,
    }
}
