using PdfLexer.DOM;
using PdfLexer.Fonts.Files;
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
                } else
                {

                }
            }
            return 1;
        }

        internal static bool AddEmbeddedInfo(ParsingContext ctx, FontType1 t1, Glyph[] names, Dictionary<string, Glyph> known)
        {
            // TODO adobe
            if (t1.FontDescriptor?.FontFile3 == null)
            {
                return false;
            }

            using var buffer = t1.FontDescriptor.FontFile3.Contents.GetDecodedBuffer();
            var cff = new CFFReader(null, buffer.GetData());
            var enc = cff.GetBaseSimpleEncoding(t1.BaseFont);
            for (var i=0;i<enc.Length;i++)
            {
                var nm = enc[i];
                if (nm != null && names[i] == null)
                {
                    names[i] = GetOrCreate(nm, (uint)i, known);
                }
            }
            return true;
        }

        internal static bool AddNamedEncoding(ParsingContext ctx, FontType1 t1, PdfName nm, Glyph[] encoding, Dictionary<string, Glyph> known)
        {
            var lookup = Encodings.GetEncoding(nm);
            if (lookup == null)
            {
                ctx.Error($"Unknown encoding specified in type 1 font ({t1.BaseFont}): {nm.Value}");
                return false;
            }

            for (var i = 0; i < 256; i++)
            {
                var luv = lookup[i];
                if (luv != null)
                {
                    Glyph g = GetOrCreate(luv, (uint)i, known);
                    encoding[i] = g;
                }
            }
            return true;
        }

        internal static Glyph GetOrCreate(string name, uint cp, Dictionary<string, Glyph> known)
        {
            Glyph g = null;
            if (known != null && known.TryGetValue(name, out g))
            {
                g = g.Clone();
            }
            
            if (g == null)
            {
                var c = GlyphNames.Get(name, cp);
                g = new Glyph
                {
                    Name = name
                };
                if (c == null)
                {
                    g.Char = (char)cp;
                    g.GuessedUnicode = true;
                } else
                {
                    g.Char = c.Value;
                }
            }

            g.CodePoint = cp;
            if (cp == 32)
            {
                g.IsWordSpace = true;
            }
            return g;
        }

        internal static bool AddDifferenceEncoding(ParsingContext ctx, FontType1 t1, Glyph[] encoding, Dictionary<string, Glyph> known)
        {
            bool hadBase = false;
            var encDiff = (FontEncoding)(PdfDictionary)t1.Encoding;

            if (encDiff.BaseEncoding != null)
            {
                var be = Encodings.GetEncoding(encDiff.BaseEncoding);
                if (be == null)
                {
                    ctx.Error($"Unknown base encoding specified in type 1 font enc dict ({t1.BaseFont}): {encDiff.BaseEncoding}");
                } else
                {
                    hadBase = true;
                    for (var i=0;i<256;i++)
                    {
                        var nm = be[i];
                        if (nm == null)
                        {
                            encoding[i] = null;
                            continue;
                        }
                        encoding[i] = GetOrCreate(nm, (uint)i, known);
                    }
                }
            }

            foreach (var diff in encDiff.ReadDifferences())
            {
                if (diff.code > 255)
                {
                    continue;
                }
                var nm = diff.name.Value[1..];
                encoding[diff.code] = GetOrCreate(nm, (uint)diff.code, known);
            }
            return hadBase;
        }
        internal static void AddWidths(ParsingContext ctx, FontType1 t1, Glyph[] encoding)
        {
            var bbox = t1.FontDescriptor?.FontBBox;
            if (t1.FirstChar != null && t1.Widths != null)
            {
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
                    var g = encoding[cp];
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
                        encoding[cp] = g;
                        if (bbox != null && g.BBox == null)
                        {
                            g.BBox = new decimal[] { 0, 0, (decimal)g.w0, bbox.URy / 1000.0m };
                        }
                    }
                }
            }
        }

        internal static IReadableFont Create(ParsingContext ctx, FontType1 t1)
        {
            var hasBase = false;
            var (encoding, b14glyphs) = GetBase14Info(t1);
            encoding ??= new Glyph[256];

            Dictionary<string, Glyph> knownNames = null;
            if (b14glyphs != null)
            {
                hasBase = true;
                knownNames = new Dictionary<string, Glyph>();
                foreach (var item in b14glyphs)
                {
                    if (item == null) { continue; }
                    knownNames[item.Name] = item;
                }
            }

            var hadEmbedded = AddEmbeddedInfo(ctx, t1, encoding, knownNames);
            if (hadEmbedded) { hasBase = true; }

            decimal mw = t1.FontDescriptor?.MissingWidth;
            if (b14glyphs != null && !(t1.FontDescriptor?.NativeObject?.ContainsKey(PdfName.MissingWidth) ?? false))
            {
                mw = 0.278m;
            }

            var notdef = new Glyph { Char = '\u0000', w0 = (float)mw, IsWordSpace = false, BBox = new decimal[] { 0m, 0m, mw, 0m } };
            notdef.Undefined = true;

            if (t1.Encoding != null)
            {
                var enc = t1.Encoding.Resolve();
                if (enc.Type == PdfObjectType.NameObj)
                {
                    var nm = enc.GetAs<PdfName>();
                    var hadNamed = AddNamedEncoding(ctx, t1, nm, encoding, knownNames);
                    if (hadNamed) { hasBase = true; }
                }
                else if (enc.Type == PdfObjectType.DictionaryObj)
                {
                    var hadDiff = AddDifferenceEncoding(ctx, t1, encoding, knownNames);
                    if (hadDiff) { hasBase = true; }
                }
                else
                {
                    ctx.Error($"Encoding specified in type 1 font was not name or dict ({t1.BaseFont}): {enc.Type}");
                }
            }

            ToUnicodeFont.AddMissingSimple(ctx, t1, encoding);

            if (!hasBase)
            {
                var nms = Encodings.StandardEncoding;
                for (var i = 0; i < nms.Length; i++)
                {
                    var nm = nms[i];
                    if (encoding[i] == null && nm != null)
                    {
                        encoding[i] = GetOrCreate(nm, (uint)i, null);
                    }
                }
            }

            AddWidths(ctx, t1, encoding);

            return new SingleByteFont(t1.BaseFont, encoding, notdef);
        }

        internal static (Glyph[] defaultEnc, Glyph[] allGlyphs) GetBase14Info(FontType1 t1)
        {

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
                    return (null, null);
            }
            return (defaultEnc.ToArray(), allGlyphs);
        }
    }


}
