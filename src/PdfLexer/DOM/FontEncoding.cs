using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer.DOM
{
    public  class FontEncoding
    {
        public PdfDictionary NativeObject { get; }

        public FontEncoding()
        {
            NativeObject = new PdfDictionary();
            NativeObject[PdfName.TypeName] = PdfName.Encoding;
        }

        public FontEncoding(PdfDictionary dict)
        {
            NativeObject = dict;
        }

        public static implicit operator FontEncoding(PdfDictionary dict) => new FontEncoding(dict);
        public static implicit operator PdfDictionary(FontEncoding page) => page.NativeObject;

        // optional
        // implicit base encoding -> font built in encoding
        // StandardEncoding for non symbolic font
        public PdfName BaseEncoding
        {
            get => NativeObject.Get<PdfName>(PdfName.BaseEncoding);
            set => NativeObject[PdfName.BaseEncoding] = value;
        }
        // optional
        // should not be used in truetype fonts
        public PdfArray Differences
        {
            get => NativeObject.Get<PdfArray>(PdfName.Differences);
            set => NativeObject[PdfName.Differences] = value;
        }

        public IEnumerable<(int code, PdfName name)> ReadDifferences()
        {
            var arr = Differences;
            int charCode = 0;
            foreach (var val in arr)
            {
                switch (val)
                {
                    case PdfNumber cnt:
                        charCode = cnt;
                        continue;
                    case PdfName nm:
                        yield return (charCode, nm);
                        charCode++;
                        continue;
                }
            }
        }
    }
}
