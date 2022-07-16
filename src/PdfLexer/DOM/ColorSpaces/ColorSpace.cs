﻿using PdfLexer.Parsers;
using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer.DOM.ColorSpaces
{
    public class ColorSpace
    {
        public static IColorSpace Get(ParsingContext ctx, IPdfObject cs)
        {
            cs = cs.Resolve();
            switch (cs)
            {
                case PdfName nm:
                    switch (nm.Value)
                    {
                        case "/DeviceGray":
                            return DeviceGray.Instance;
                        case "/DeviceRGB":
                            return DeviceRGB.Instance;
                        case "/DeviceCMYK":
                        case "/CalCMYK":
                            return DeviceCMYK.Instance;
                        case "/Pattern":
                        default:
                            throw new NotImplementedException($"Colorspace {nm.Value} is not implemented.");
                    }
                case PdfArray arr:
                    if (arr.Count == 0) { throw new PdfLexerException($"Array colorspace had no entries."); }
                    var mode = arr[0].GetValue<PdfName>();
                    switch (mode.Value)
                    {
                        case "/DeviceGray":
                            return DeviceGray.Instance;
                        case "/DeviceRGB":
                            return DeviceRGB.Instance;
                        case "/DeviceCMYK":
                        case "/CalCMYK":
                            return DeviceCMYK.Instance;
                        case "/CalGray":
                            if (arr.Count < 2) { throw new PdfLexerException($"CalGray colorspace had no dictionary."); }
                            return CalGray.FromObject(arr[1].GetValue<PdfDictionary>());
                        case "/CalRGB":
                            if (arr.Count < 2) { throw new PdfLexerException($"CalRGB colorspace had no dictionary."); }
                            return CalRGB.FromObject(arr[1].GetValue<PdfDictionary>());
                        case "/ICCBased":
                        case "/Pattern":
                        case "/Separation":
                        case "/DeviceN":
                        case "/Lab":
                            throw new NotImplementedException($"Colorspace {mode.Value} is not implemented.");
                        case "/Indexed":
                            if (arr.Count < 4) { throw new PdfLexerException($"Indexed colorspace had less than 4 entries."); }
                            return Indexed.FromArray(ctx, arr);

                        default:
                            throw new PdfLexerException($"Unknown colorspace {mode.Value}");
                    }
                default:
                    throw new PdfLexerException("Non masked image had unknown colorspace defined: " + cs.GetPdfObjType());
            }
        }
    }
}
