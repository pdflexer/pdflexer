using PdfLexer.DOM;
using PdfLexer.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;


// FontUtil code below ported from PDF.JS, PDF.JS is licensed as follows:
/* Copyright 2012 Mozilla Foundation
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace PdfLexer.Fonts
{


    internal class FontUtil
    {
        public static PdfArray FONT_IDENTITY_MATRIX { get; } = new PdfArray {
            new PdfDecimalNumber(0.001m), PdfCommonNumbers.Zero, PdfCommonNumbers.Zero, new PdfDecimalNumber(0.001m), PdfCommonNumbers.Zero, PdfCommonNumbers.Zero };
        // load font
        // translate font
        // adds translated type 3 deps ??


        public static IReadableFont LoadFont(PdfDictionary font)
        {
            var evaled = PreEvaluateFont(font);
            // font["/LoadedName"] = "docID_fontId"

            return null;

        }

        public static Font TranslateFont(ParsingContext ctx, EvaluatedFont font)
        {
            var isType3Font = font.Subtype == PdfName.Type3;

            if (font.Descriptor == null)
            {
                if (isType3Font)
                {
                    // FontDescriptor is only required for Type3 fonts when the document
                    // is a tagged pdf. Create a barbebones one to get by.
                    font.Descriptor = new PdfDictionary();
                    font.Descriptor[PdfName.FontName] = font.Subtype;
                    font.Descriptor[PdfName.FontBBox] = font.Dict.Get<PdfArray>(PdfName.FontBBox) ?? PdfRectangle.Zeros;
                } else
                {
                    // Before PDF 1.5 if the font was one of the base 14 fonts, having a
                    // FontDescriptor was not required.
                    // This case is here for compatibility.
                    var baseFontName = font.Dict.Get<PdfName>(PdfName.BaseFont);
                    if (baseFontName == null) {
                        throw new ApplicationException("Base font is not specified");
                    }
                    var name = Regex.Replace(baseFontName.Value, "[,_]", "-");

                    var metrics = new FontMetrics();// getbasefontmetrics(name);

                    var unstyled = name.Split('-')[0];
                    var flags = (IsSerifFont(unstyled) ? FontFlags.Serif : 0) |
                        (metrics.Monospace ? FontFlags.FixedPitch : 0) |
                        (IsSerifFont(unstyled) ? FontFlags.Symbolic : FontFlags.Nonsymbolic);

                    var props = new FontProperties
                    {
                        Evaluated = font,
                        BaseFontName = name,
                        Widths = metrics.Widths,
                        DefaultWidth = metrics.DefaultWidth,
                        IsSimulatedFlags = true,
                        Flags = flags,
                        XHeight = 0f,
                        CapHeight = 0f,
                        ItalicAngle = 0f,
                        IsType3Font = isType3Font
                    };

                    var widths = font.Dict.Get<PdfArray>(PdfName.Widths);

                    byte[] file = null; ;
                    var standardName = GetStandardFontName(name);
                    if (standardName != null)
                    {
                        props.IsStandardFont = true;
                        file = FetchStandardFontData(name);
                        props.IsInternalFont = file != null;
                    }
                    var extracted = ExtractDataStructures(font.Dict, font.Dict, props);

                    if (widths != null)
                    {
                        var wl = new List<int>(font.LastChar - font.FirstChar + 1);
                        for (var i=0;i<wl.Count;i++)
                        {
                            wl[i] = widths.Count > i ? widths[i].GetValue<PdfIntNumber>(false) : 0;
                        }

                        extracted.Widths = wl;
                    } else
                    {
                        extracted.Widths = BuildCharCodeToWidth(metrics.Widths, extracted);
                    }
                    return new Font(baseFontName.Value, file, extracted);
                }
            }

            // TODO support for bad non name tokens?
            var fontName = font.Descriptor.Get<PdfName>(PdfName.FontName);
            var baseFont = font.Descriptor.Get<PdfName>(PdfName.BaseFont);
            if (!isType3Font)
            {
                // TODO add warning
                var fontNameStr = fontName?.Value;
                var baseFontStr = baseFont?.Value;
                if (fontNameStr != null && baseFontStr != null && baseFontStr.StartsWith(fontNameStr))
                {
                    fontName = baseFont;
                }
            }
            fontName ??= baseFont;
            if (fontName == null)
            {
                throw new ApplicationException("Invalid or missing font name");
            }

            PdfName subtype = null;
            int length1 = 0, length2 = 0, length3 = 0;
            var fontFile = font.Descriptor.Get<PdfStream>(PdfName.FontFile)
                ?? font.Descriptor.Get<PdfStream>(PdfName.FontFile2)
                 ?? font.Descriptor.Get<PdfStream>(PdfName.FontFile3);
            var fontFileData = fontFile?.Contents.GetDecodedData(ctx);
            var isStandardFont = false;
            var isInternalFont = false;
            var glyphScaleFactors = 0;

            if (fontFile != null)
            {
                subtype = fontFile.Dictionary.Get<PdfName>(PdfName.Subtype);
                length1 = fontFile.Dictionary.Get<PdfIntNumber>(PdfName.Length1);
                length2 = fontFile.Dictionary.Get<PdfIntNumber>(PdfName.Length2);
                length3 = fontFile.Dictionary.Get<PdfIntNumber>(PdfName.Length3);
            } else if (!isType3Font)
            {
                var std = GetStandardFontName(fontName.Value);
                if (std != null)
                {
                    isStandardFont = true;
                    // TODO
                    fontFileData = FetchStandardFontData(std);
                    isInternalFont = fontFile != null;
                }
            }

            // on error return null stream in pdfjs for getting stream
            var transProps = new TranslatedFontProps
            {
                // TODO: loaded name
                Evaluated = font,
                Properties = new FontProperties
                {
                    IsStandardFont = isStandardFont,
                    IsInternalFont = isInternalFont,
                    XHeight = font.Descriptor.Get<PdfNumber>(PdfName.XHeight) ?? 0,
                    CapHeight = font.Descriptor.Get<PdfNumber>(PdfName.CapHeight) ?? 0,
                    Flags = (FontFlags)(font.Descriptor.Get<PdfIntNumber>(PdfName.Flags) ?? 0),
                    ItalicAngle = font.Descriptor.Get<PdfNumber>(PdfName.ItalicAngle) ?? 0,
                    IsType3Font = isType3Font,
                },
                Type = font.Subtype,
                Name = fontName,
                FontFile = fontFileData,
                Length1 = length1,
                Length2 = length2,
                Length3 = length3,
                FixedPitch = false,
                FontMatrix = font.Dict.Get<PdfArray>(PdfName.FontMatrix) ?? FONT_IDENTITY_MATRIX,
                BBox = font.Descriptor.Get<PdfArray>(PdfName.FontBBox) ?? font.Dict.Get<PdfArray>(PdfName.FontBBox),
                Ascent = font.Descriptor.Get<PdfNumber>(PdfName.Ascent),
                Descent = font.Descriptor.Get<PdfNumber>(PdfName.Descent),
                GlyphScaleFactors = glyphScaleFactors
            };

            if (font.Composite)
            {
                var encoding = font.BaseDict.Get(PdfName.Encoding);
                if (encoding is PdfName nm)
                {
                    transProps.CidEncoding = nm.Value;
                }
                transProps.CMap = new object(); // TODO
                transProps.Vertical = false; // TODO cmap.vertical
            }

            ExtractDataStructures(font.Dict, font.BaseDict, transProps);
            ExtractWidths(font.Dict, font.Descriptor, transProps);

            return new Font(fontName.Value, fontFileData, null); // transProps); TODO combine different props

        }

        public static List<int> BuildCharCodeToWidth(PdfArray mw, DataStructures structs)
        {
            return new List<int>();
        }


        // TODO combine these
        public static DataStructures ExtractDataStructures(PdfDictionary dict, PdfDictionary baseDict, FontProperties props)
        {
            // TODO a lot of code here
            return new DataStructures();
        }

        public static DataStructures ExtractDataStructures(PdfDictionary dict, PdfDictionary baseDict, TranslatedFontProps props)
        {
            return new DataStructures();
        }

        public static void ExtractWidths(PdfDictionary dict, PdfDictionary descriptor, TranslatedFontProps props)
        {
            // TODO a lot of code here
        }

        public static byte[] FetchStandardFontData(string name)
        {
            return null;
        }

        public static string GetStandardFontName(string name)
        {
            return "";
        }

        public static bool IsSymbolFont(string name)
        {
            return false;
        }

        public static bool IsSerifFont(string name)
        {
            return false;
        }

        public static EvaluatedFont PreEvaluateFont(PdfDictionary dict)
        {
            var baseDict = dict;
            var composite = false;
            if (!dict.TryGetValue<PdfName>(PdfName.Subtype, out var subtype, false))
            {
                throw new PdfLexerException("invalid or missing font Subtype");
            }

            if (subtype == PdfName.Type0)
            {
                if (!dict.TryGetValue(PdfName.DescendantFonts, out var df))
                {
                    throw new PdfLexerException("Descendant fonts are not specified");
                }

                var dfDict = dict.Get<PdfDictionary>(PdfName.DescendantFonts)
                    ?? dict.Get<PdfArray>(PdfName.DescendantFonts)?.FirstOrDefault() as PdfDictionary;
                
                if (dfDict == null)
                {
                    throw new PdfLexerException("Descendant font is not a dictionary");
                }

                dict = dfDict;
                if (!dfDict.TryGetValue<PdfName>(PdfName.Subtype, out subtype, false))
                {
                    throw new PdfLexerException("invalid or missing font Subtype in descendant");
                }
                composite = true;
            }

            var firstChar = dict.Get<PdfIntNumber>("/FirstChar") ?? PdfCommonNumbers.Zero;
            var lastChar = dict.Get<PdfIntNumber>("/LastChar") ?? (composite ? new PdfIntNumber(0xffff) : new PdfIntNumber(0xff));
            var descriptor = dict.Get<PdfDictionary>("/FontDescriptor");
            if (descriptor != null)
            {
                // hash calced
                var encoding = baseDict.Get(PdfName.Encoding);
            }


            var toUnicode = dict.Get("/ToUnicode") ?? baseDict.Get("/ToUnicode");
            // hash

            var widths = dict.Get(PdfName.Widths) ?? baseDict.Get(PdfName.Widths);
            // hash

            if (composite)
            {
                var compositeWidths = dict.Get(PdfName.W) ?? baseDict.Get(PdfName.W);
                // hash

                var cidToGidMap = dict.Get("CIDToGIDMap") ?? baseDict.Get("CIDToGIDMap");
                // hash
            }

            return new EvaluatedFont
            {
                Descriptor = descriptor,
                Dict = dict,
                BaseDict = baseDict,
                Composite = composite,
                Subtype = subtype,
                FirstChar = firstChar,
                LastChar = lastChar,
                ToUnicode = toUnicode
            };
        }



    }

    [Flags]
    public enum FontFlags {
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

    public class Font
    {
        public Font(string baseFontName, object file, DataStructures structurs)
        {

        }
    }

    public class EvaluatedFont
    {
        public PdfDictionary Descriptor { get; set; }
        public PdfDictionary Dict { get; set; }
        public PdfDictionary BaseDict { get; set; }
        public bool Composite { get; set; }
        public PdfName Subtype { get; set; }
        public PdfIntNumber FirstChar { get; set; }
        public PdfIntNumber LastChar { get; set; }
        public IPdfObject ToUnicode { get; set; }
    }

    public class FontProperties
    {
        public EvaluatedFont Evaluated { get; set; }
        public PdfName BaseFontName { get; set; }
        public PdfName LoadedName { get; set; }
        public IPdfObject Widths { get; set; }
        public float DefaultWidth { get; set; }
        public bool IsSimulatedFlags { get; set; }
        public FontFlags Flags { get; set; }
        public float XHeight { get; set; }
        public float CapHeight { get; set; }
        public float ItalicAngle { get; set; }
        public bool IsType3Font { get; set; }
        public bool IsStandardFont { get; set; }
        public bool IsInternalFont { get; set; }
    }

    public class TranslatedFontProps
    {
        public FontProperties Properties { get; set; }
        public EvaluatedFont Evaluated { get; set; }
        public PdfName Type { get; set; }
        public PdfName Name { get; set; }
        public byte[] FontFile { get; set; }
        public int Length1 { get; set; }
        public int Length2 { get; set; }
        public int Length3 { get; set; }
        public bool FixedPitch { get; set; }
        public PdfArray FontMatrix { get; set; }
        public IPdfObject BBox { get; set; }
        public IPdfObject Ascent { get; set; }
        public IPdfObject Descent { get; set; }
        public float GlyphScaleFactors { get; set; }
        public string CidEncoding { get; set; }
        public object CMap { get; set; }
        public bool Vertical { get; set; }
    }

    public class FontMetrics
    {
        public bool Monospace { get; set; }
        public PdfArray Widths { get; set; }
        public float DefaultWidth { get; set; }
    }

    public class DataStructures
    {

        public List<int> Widths { get; set; }
    }
}


