using PdfLexer.DOM;
using PdfLexer.Fonts.Predefined;
using PdfLexer.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PdfLexer.Fonts
{
    public partial class SingleByteFont : IReadableFont
    {
        private Glyph[] Glyphs;
        private Glyph NotDef;

        public bool IsVertical => false;

        public PdfName Name { get; }

        public SingleByteFont(PdfName name, Glyph[] glyphs, Glyph notdef)
        {
            Glyphs = glyphs;
            Name = name;
            NotDef = notdef;
        }

        public int GetGlyph(ReadOnlySpan<byte> data, int os, out Glyph glyph)
        {
            glyph = NotDef;
            var c = (int)data[os];
            if (c < Glyphs.Length)
            {
                var g = Glyphs[c];
                if (g != null) 
                {
                    glyph = g;
                }
            }
            return 1;
        }

        internal static IReadableFont Type1Fallback(FontType1 t1)
        {
            var gs = Encodings.GetPartialGlyphs(Encodings.StandardEncoding);
            foreach (var g in gs)
            {
                if (g != null)
                {
                    g.GuessedUnicode = true;
                }
            }
            return FromEncodingAndDifferences(t1, gs, gs);
        }

        internal static IReadableFont FromEncodingAndDifferences(FontType1 t1, Glyph[] allGlyphs, Glyph[] defaultEnc, PdfName? encoding=null)
        {
            var all = new Dictionary<string, Glyph>();
            foreach (var item in allGlyphs)
            {
                if (item == null) { continue; }
                all[item.Name] = item;
            }
            var fm = new GlyphSet(all, defaultEnc);

            var mw = t1.FontDescriptor?.MissingWidth ?? 0;

            var glyphs = fm.GetStandardEncoding();
            var notdef = fm.GetGlyph(".notdef") ?? new Glyph { Char = '\u0000', w0 = mw, IsWordSpace = false, BBox = new decimal[] { 0m, 0m, mw, 0m } };
            notdef.Undefined = true;
            if (t1.FontDescriptor?.NativeObject?.ContainsKey(PdfName.MissingWidth) ?? false)
            {
                notdef = notdef.Clone();
                notdef.w0 = t1.FontDescriptor.MissingWidth;
            }
            bool copied = false;

            if (t1.Encoding != null)
            {
                var enc = t1.Encoding.Resolve();
                if (enc.Type == PdfObjectType.NameObj)
                {
                    var nm = enc.GetAs<PdfName>();
                    if ((encoding != null && nm != encoding))
                    {
                        var lookup = Encodings.GetEncoding(nm);
                        if (lookup == null)
                        {
                            // todo error
                        }
                        else
                        {
                            if (t1.NativeObject.Get<PdfName>(PdfName.Subtype)?.Value == PdfName.Type1)
                            {
                                glyphs = new Glyph[256];
                                copied = true;
                                for (var i = 0; i < 256; i++)
                                {
                                    var luv = lookup[i];
                                    if (luv != null)
                                    {
                                        var g = fm.GetGlyph(luv);
                                        if (g != null)
                                        {
                                            g = g.Clone();
                                            g.CodePoint = (uint)i;
                                            if (i == 32)
                                            {
                                                g.IsWordSpace = true;
                                            }
                                        }
                                        glyphs[i] = g;
                                    }
                                    else
                                    {
                                        glyphs[i] = null;
                                    }
                                }
                            }
                            else
                            {
                                glyphs = Encodings.GetPartialGlyphs(lookup);
                            }
                        }
                    }
                }
                else if (enc.Type == PdfObjectType.DictionaryObj)
                {
                    copied = true;
                    
                    var encDiff = (FontEncoding)(PdfDictionary)t1.Encoding;
                    if (encDiff.BaseEncoding != null)
                    {
                        var be = Encodings.GetEncoding(encDiff.BaseEncoding);
                        if (be == null)
                        {
                            // todo error
                            be = Encodings.StandardEncoding;
                        }
                        glyphs = Encodings.GetPartialGlyphs(be);

                    } else
                    {
                        glyphs = glyphs.ToArray();
                    }
                    foreach (var diff in encDiff.ReadDifferences())
                    {
                        if (diff.code > 255)
                        {
                            continue;
                        }
                        var nm = diff.name.Value[1..];
                        var g = fm.GetGlyph(nm);
                        if (g != null) // TODO how to handle not found, leave as is or notdef it, or create new glyph with unicode char?
                        {
                            if (diff.code == 32 && g.IsWordSpace == false)
                            {
                                g = g.Clone();
                                g.IsWordSpace = true;
                            }
                            glyphs[diff.code] = g;
                        } else
                        {
                            if (GlyphNames.Lookup.TryGetValue(nm, out char c))
                            {
                                glyphs[diff.code] = new Glyph
                                {
                                    Char = c,
                                    CodePoint = (uint)diff.code,
                                    Name = nm,
                                    IsWordSpace = diff.code == 32
                                };
                            } else
                            {
                                // TODO error
                            }
                        }
                    }
                }
                else
                {
                    // TODO ctx error / fallback
                    throw new PdfLexerException("simple font encoding was: " + enc.Type);
                }
            }
            var bbox = t1.FontDescriptor?.FontBBox;
            if (t1.FirstChar != null && t1.Widths != null)
            {
                if (!copied)
                {
                    glyphs = glyphs.ToArray();
                }
                var start = (int)t1.FirstChar;
                float sf = 1000.0f;
                if (t1.NativeObject.TryGetValue<PdfArray>(PdfName.FontMatrix, out var matrix))
                {
                    var x = (float)matrix[0].GetAs<PdfNumber>();
                    sf = 1 / x;
                }
                for (var i = 0; i < t1.Widths.Count; i++)
                {
                    var v = (float)t1.Widths[i].GetAs<PdfNumber>();
                    v = v / sf;
                    var cp = (i + start);
                    var g = glyphs[cp];
                    g ??= new Glyph
                    {
                        Char = (char)cp,
                        CodePoint = (uint)cp,
                        GuessedUnicode = true
                    };
                    if (v != g.w0)
                    {
                        g = g.Clone();
                        g.w0 = v;
                        glyphs[cp] = g;
                        if (bbox != null && g.BBox == null)
                        {
                            g.BBox = new decimal[] { 0, 0, (decimal)g.w0, bbox.URy / 1000.0m };
                        }
                    }
                }
            }

            if (glyphs.Max(x=>x?.CodePoint ?? 0) > 255)
            {
                // TODO ctx error / fallback
                throw new PdfLexerException("simple font had codepoint greater than 255");
            }

            return new SingleByteFont(t1.BaseFont, glyphs, notdef);
        }


        internal static IReadableFont Create(ParsingContext ctx, FontType1 t1)
        {
            if (t1.ToUnicode != null)
            {
                return ToUnicodeFont.GetSimple(ctx, t1);
            }

            Glyph[] defaultEnc = null;
            Glyph[] allGlyphs = null;

            switch (t1.BaseFont?.Value)
            {
                case "/Times-Roman":
                    defaultEnc = TimesRomanGlyphs.DefaultEncoding;
                    allGlyphs = TimesRomanGlyphs.AllGlyphs;
                    break;
                case "/Helvetica":
                    defaultEnc = HelveticaGlyphs.DefaultEncoding;
                    allGlyphs = HelveticaGlyphs.AllGlyphs;
                    break;
                case "/Courier":
                    defaultEnc = CourierGlyphs.DefaultEncoding;
                    allGlyphs = CourierGlyphs.AllGlyphs;
                    break;
                case "/Symbol":
                    defaultEnc = SymbolGlyphs.DefaultEncoding;
                    allGlyphs = SymbolGlyphs.AllGlyphs;
                    break;
                case "/Times-Bold":
                    defaultEnc = TimesBoldGlyphs.DefaultEncoding;
                    allGlyphs = TimesBoldGlyphs.AllGlyphs;
                    break;
                case "/Helvetica-Bold":
                    defaultEnc = HelveticaBoldGlyphs.DefaultEncoding;
                    allGlyphs = HelveticaBoldGlyphs.AllGlyphs;
                    break;
                case "/Courier-Bold":
                    defaultEnc = CourierBoldGlyphs.DefaultEncoding;
                    allGlyphs = CourierBoldGlyphs.AllGlyphs;
                    break;
                case "/ZapfDingbats":
                    defaultEnc = ZapfDingbatsGlyphs.DefaultEncoding;
                    allGlyphs = ZapfDingbatsGlyphs.AllGlyphs;
                    break;
                case "/Times-Italic":
                    defaultEnc = TimesItalicGlyphs.DefaultEncoding;
                    allGlyphs = TimesItalicGlyphs.AllGlyphs;
                    break;
                case "/Helvetica-Oblique":
                    defaultEnc = HelveticaObliqueGlyphs.DefaultEncoding;
                    allGlyphs = HelveticaObliqueGlyphs.AllGlyphs;
                    break;
                case "/Courier-Oblique":
                    defaultEnc = CourierObliqueGlyphs.DefaultEncoding;
                    allGlyphs = CourierObliqueGlyphs.AllGlyphs;
                    break;
                case "/Times-BoldItalic":
                    defaultEnc = TimesBoldItalicGlyphs.DefaultEncoding;
                    allGlyphs = TimesBoldItalicGlyphs.AllGlyphs;
                    break;
                case "/Helvetica-BoldOblique":
                    defaultEnc = HelveticaBoldObliqueGlyphs.DefaultEncoding;
                    allGlyphs = HelveticaBoldObliqueGlyphs.AllGlyphs;
                    break;
                case "/Courier-BoldOblique":
                    defaultEnc = CourierBoldObliqueGlyphs.DefaultEncoding;
                    allGlyphs = CourierBoldObliqueGlyphs.AllGlyphs;
                    break;
                default:
                    if (t1.Encoding != null && t1.Encoding.GetPdfObjType() == PdfObjectType.NameObj)
                    {
                        var be = t1.Encoding.GetAs<PdfName>();
                        var g = Encodings.GetPartialGlyphs(Encodings.GetEncoding(be));
                        return FromEncodingAndDifferences(t1, g, g, be);
                    }
                    return Type1Fallback(t1);
            }

            return FromEncodingAndDifferences(t1, allGlyphs, defaultEnc);

            // widths
            // optional otherwise from font metrics
            // use MissingWidth if outside range of FirstChar / LastChar
            // 1000 to 1 ratio with text space

            // ToUnicode
            // simple fonts ->
            // base encoding one of three known or encoding with differences
            // all standard latin char set
            // map char code to char name
            // char name to unicode value

            // composite font
            // with one of the predifined CMaps (except indentity-h/v)
            // or whose descendant CIDFont uses Adobe-GB1 CNS1 Japan1 or Korea1
            // a) map char code to char identifier (CID) according to CMap
            // b) get registry / ordering of char colloection from CIDSystemInfo dict
            // c) construct a second CMap by concating ordering / registry
            // d) obtain the CMap with the name constructed in (c) -> ASN website
            // e) map the CID in (a) acocording the CMap (d)
        }
    }


}
