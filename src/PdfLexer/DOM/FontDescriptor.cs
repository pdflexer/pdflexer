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

        public static implicit operator FontDescriptor(PdfDictionary dict) => new FontDescriptor(dict);
        public static implicit operator PdfDictionary(FontDescriptor page) => page.NativeObject;

        // required
        public PdfName FontName
        {
            get => NativeObject?.Get<PdfName>(PdfName.FontName);
            set => NativeObject[PdfName.FontName] = value;
        }

        // optional but should be used in Type 3 fonts in Tagged PDFs
        public PdfString FontFamily
        {
            get => NativeObject?.Get<PdfString>(PdfName.FontFamily);
            set => NativeObject[PdfName.FontFamily] = value;
        }

        // optional but should be used in Type 3 fonts in Tagged PDFs
        // TODO enum
        // UltraCondensed, ExtraCondensed, Condensed, SemiCondensed, Normal, SemiExpanded
        // Expanded, ExtraExpanded, UltraExpanded
        public PdfString FontStretch
        {
            get => NativeObject?.Get<PdfString>(PdfName.FontFamily);
            set => NativeObject[PdfName.FontFamily] = value;
        }

        // optional but should be used in Type 3 fonts in Tagged PDFs
        // 100, 200, 300, 400, 500, 600, 700, 800, 900
        // 400 -> normal, 700 -> bold
        public PdfString FontWeight
        {
            get => NativeObject?.Get<PdfString>(PdfName.FontWeight);
            set => NativeObject[PdfName.FontWeight] = value;
        }

        // required
        // TODO enum handling
        public PdfIntNumber Flags
        {
            get => NativeObject?.Get<PdfIntNumber>(PdfName.Flags);
            set => NativeObject[PdfName.Flags] = value;
        }

        // required except for type 3
        public PdfRectangle FontBBox
        {
            get => NativeObject?.Get<PdfArray>(PdfName.FontBBox);
            set => NativeObject[PdfName.FontBBox] = (PdfArray)value;
        }

        // required
        public PdfNumber ItalicAngle
        {
            get => NativeObject?.Get<PdfNumber>(PdfName.ItalicAngle);
            set => NativeObject[PdfName.ItalicAngle] = value;
        }

        // required except for type 3
        public PdfNumber Ascent
        {
            get => NativeObject?.Get<PdfNumber>(PdfName.Ascent);
            set => NativeObject[PdfName.Ascent] = value;
        }

        // required except for type 3
        public PdfNumber Descent
        {
            get => NativeObject?.Get<PdfNumber>(PdfName.Descent);
            set => NativeObject[PdfName.Descent] = value;
        }

        // Spacing between consecutive lines of text, default 0
        public PdfNumber Leading
        {
            get => NativeObject?.Get<PdfNumber>(PdfName.Leading) ?? 0;
            set => NativeObject[PdfName.Leading] = value;
        }

        // required for fonts with Latin chars except for type 3
        // vertical coordinate of the top of flat capital letters measured
        // from the baseline
        public PdfNumber CapHeight
        {
            get => NativeObject?.Get<PdfNumber>(PdfName.CapHeight);
            set => NativeObject[PdfName.CapHeight] = value;
        }

        // Vertical coordinate of the top of flat nonscending lowercase letters
        // measured from baseline
        // default 0
        public PdfNumber XHeight
        {
            get => NativeObject?.Get<PdfNumber>(PdfName.XHeight) ?? 0;
            set => NativeObject[PdfName.XHeight] = value;
        }

        // required except for type 3 fonts
        // thickness of dominant vertical stems measured horizontally
        public PdfNumber StemV
        {
            get => NativeObject?.Get<PdfNumber>(PdfName.StemV) ?? 0;
            set => NativeObject[PdfName.StemV] = value;
        }

        // optional
        // thickness of dominant horizontal stems measured vertically
        public PdfNumber StemH
        {
            get => NativeObject?.Get<PdfNumber>(PdfName.StemH) ?? 0;
            set => NativeObject[PdfName.StemH] = value;
        }

        // TODO
        // AvgWidth
        // MaxWidth

        // optional
        // thickness of dominant horizontal stems measured vertically
        public PdfNumber MissingWidth
        {
            get => NativeObject?.Get<PdfNumber>(PdfName.MissingWidth) ?? 0;
            set => NativeObject[PdfName.MissingWidth] = value;
        }

        // optional
        // embedded type 1 front
        public PdfStream FontFile
        {
            get => NativeObject?.Get<PdfStream>(PdfName.FontFile);
            set => NativeObject[PdfName.FontFile] = value.Indirect();
        }

        // optional
        // embedded truetype font
        public PdfStream FontFile2
        {
            get => NativeObject?.Get<PdfStream>(PdfName.FontFile2);
            set => NativeObject[PdfName.FontFile2] = value.Indirect();
        }

        // optional
        // embedded font, type specified in Subtype of stream dict
        public PdfStream FontFile3
        {
            get => NativeObject?.Get<PdfStream>(PdfName.FontFile3);
            set => NativeObject[PdfName.FontFile3] = value.Indirect();
        }

        // optional
        // meaningful only for type 1 fonts for subsetting
        public PdfString CharSet
        {
            get => NativeObject?.Get<PdfString>(PdfName.CharSet);
            set => NativeObject[PdfName.CharSet] = value;
        }
    }
}
