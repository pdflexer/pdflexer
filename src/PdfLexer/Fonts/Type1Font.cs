using PdfLexer.DOM;
using PdfLexer.Fonts.Files;
using PdfLexer.Fonts.Predefined;

namespace PdfLexer.Fonts;

internal partial class Type1Font
{
    internal static IReadableFont CreateReadable(ParsingContext ctx, FontType1 t1)
    {
        var hasBase = false;
        var (encoding, b14glyphs) = GetBase14Info(t1.BaseFont?.Value);
        encoding ??= new Glyph[256];

        Dictionary<string, Glyph>? knownNames = null;
        if (b14glyphs != null)
        {
            hasBase = true;
            knownNames = new Dictionary<string, Glyph>();
            foreach (var item in b14glyphs)
            {
                if (item?.Name == null) { continue; }
                knownNames[item.Name] = item;
            }
        }

        var hadEmbedded = AddEmbeddedInfo(ctx, t1, encoding, knownNames);
        if (hadEmbedded) { hasBase = true; }

        decimal mw = (t1.FontDescriptor?.MissingWidth ?? 0);
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

        AddMissingSimple(ctx, t1, encoding);

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

        AddWidths(t1, encoding);

        PdfName fn = t1.BaseFont ?? "Empty";
        return new SingleByteFont(fn, encoding, notdef);
    }


    internal static bool AddEmbeddedInfo(ParsingContext ctx, FontType1 t1, Glyph?[] names, Dictionary<string, Glyph>? known)
    {
        var file = t1.FontDescriptor?.FontFile ?? t1.FontDescriptor?.FontFile3;
        if (file == null) { return false; }

        try
        {
            string?[]? t1Names;
            using var buffer = file.Contents.GetDecodedBuffer();
            var data = buffer.GetData();
            if (Type1Reader.IsType1File(data))
            {
                var reader = new Type1Reader(ctx, data);
                if (!reader.TryGetEncoding(out t1Names))
                {
                    return false;
                }

            } else if (CFFReader.IsCFFfile(data))
            {
                var cff = new CFFReader(ctx, buffer.GetData());
                t1Names = cff.GetBaseSimpleEncoding(t1.BaseFont ?? "Empty");
            } else
            {
                return false;
            }

            for (var i = 0; i < t1Names.Length; i++)
            {
                var nm = t1Names[i];
                if (nm != null && names[i] == null)
                {
                    names[i] = GetOrCreate(nm, (uint)i, known);
                }
            }


        } catch (Exception e)
        {
            ctx.Error($"CFF parsing error for font {t1.BaseFont}: " + e.Message);
            return false;
        }


        return true;
    }

    internal static bool AddNamedEncoding(ParsingContext ctx, FontType1 t1, PdfName nm, Glyph?[] encoding, Dictionary<string, Glyph>? known)
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

    internal static Glyph GetOrCreate(string name, uint cp, Dictionary<string, Glyph>? known)
    {
        Glyph? g = null;
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

    internal static bool AddDifferenceEncoding(ParsingContext ctx, FontType1 t1, Glyph?[] encoding, Dictionary<string, Glyph>? known)
    {
        bool hadBase = false;
        var encDiff = (FontEncoding)(PdfDictionary)t1.Encoding!;

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

        foreach (var (code, name) in encDiff.ReadDifferences())
        {
            if (code > 255)
            {
                continue;
            }
            var nm = name.Value;
            encoding[code] = GetOrCreate(nm, (uint)code, known);
        }
        return hadBase;
    }

    internal static void AddWidths(FontType1 t1, Glyph?[] encoding)
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
                v /= sf;
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
                        var x = bbox.LLx / 1000m;
                        g.BBox = new decimal[] { x, bbox.LLy / 1000.0m, x+(decimal)g.w0, bbox.URy / 1000.0m };
                    }
                }
            }
        }
    }

    internal static void AddMissingSimple(ParsingContext ctx, ISimpleUnicode dict, Glyph?[] encoding)
    {
        var str = dict.ToUnicode;
        if (str == null)
        {
            return;
        }
        using var buffer = str.Contents.GetDecodedBuffer();
        var (ranges, glyphs, _, _) = CMapReader.ReadCMap(ctx, buffer.GetData());

        foreach (var glyph in glyphs.Values)
        {
            if (glyph.CodePoint < 256)
            {
                Glyph g;
                var existing = encoding[glyph.CodePoint.Value];
                if (existing != null)
                {
                    g = existing.Clone(); // simple may be base14 and still have tounicode, need to keep width info
                    g.Char = glyph.Char;
                    g.MultiChar = glyph.MultiChar;
                }
                else
                {
                    g = glyph;
                }
                if (g.CodePoint == 32)
                {
                    g.IsWordSpace = true;
                }
                encoding[g.CodePoint ?? 0] = g;
            }
        }
    }

    internal static (Glyph?[]? defaultEnc, Glyph[]? allGlyphs) GetBase14Info(string? fontName)
    {
        if (fontName == null) { return (null, null); }
        if (fontName.Contains(","))
        {
            fontName = fontName.Replace(",", "-");
        }

        Glyph[]? allGlyphs;
        Glyph?[]? defaultEnc;

        switch (fontName)
        {
            case "Times-Roman":
            case "TimesNewRoman":
            case "TimesNewRomanPS":
            case "TimesNewRomanPSMT":
                defaultEnc = TimesRomanGlyphs.DefaultEncoding;
                allGlyphs = TimesRomanGlyphs.AllGlyphs;
                break;
            case "Helvetica":
            case "ArialNarrow":
            case "ArialBlack":
            case "Arial-Black":
            case "Arial":
            case "ArialMT":
            case "ArialUnicodeMS":
                defaultEnc = HelveticaGlyphs.DefaultEncoding;
                allGlyphs = HelveticaGlyphs.AllGlyphs;
                break;
            case "Courier":
            case "CourierNew":
            case "CourierNewPSMT":
                defaultEnc = CourierGlyphs.DefaultEncoding;
                allGlyphs = CourierGlyphs.AllGlyphs;
                break;
            case "Symbol":
            case "Symbol-Bold":
            case "Symbol-BoldItalic":
            case "Symbol-Italic":
                defaultEnc = SymbolGlyphs.DefaultEncoding;
                allGlyphs = SymbolGlyphs.AllGlyphs;
                break;
            case "Times-Bold":
            case "TimesNewRoman-Bold":
            case "TimesNewRomanPS-Bold":
            case "TimesNewRomanPSMT-Bold":
                defaultEnc = TimesBoldGlyphs.DefaultEncoding;
                allGlyphs = TimesBoldGlyphs.AllGlyphs;
                break;
            case "Helvetica-Bold":
            case "ArialNarrow-Bold":
            case "ArialBlack-Bold":
            case "Arial-Black-Bold":
            case "Arial-Bold":
            case "Arial-BoldMT":
            case "ArialUnicodeMS-Bold":
                defaultEnc = HelveticaBoldGlyphs.DefaultEncoding;
                allGlyphs = HelveticaBoldGlyphs.AllGlyphs;
                break;
            case "Courier-Bold":
            case "CourierNew-Bold":
            case "CourierNewPS-BoldMT":
                defaultEnc = CourierBoldGlyphs.DefaultEncoding;
                allGlyphs = CourierBoldGlyphs.AllGlyphs;
                break;
            case "ZapfDingbats":
                // case "Wingdings":
                // case "Wingdings-Regular":
                defaultEnc = ZapfDingbatsGlyphs.DefaultEncoding;
                allGlyphs = ZapfDingbatsGlyphs.AllGlyphs;
                break;
            case "Times-Italic":
            case "TimesNewRoman-Italic":
            case "TimesNewRomanPS-ItalicMT":
            case "TimesNewRomanPS-Italic":
            case "TimesNewRomanPSMT-Italic":
                defaultEnc = TimesItalicGlyphs.DefaultEncoding;
                allGlyphs = TimesItalicGlyphs.AllGlyphs;
                break;
            case "Helvetica-Oblique":
            case "Helvetica-Italic":
            case "ArialUnicodeMS-Italic":
            case "Arial-ItalicMT":
            case "Arial-Italic":
            case "Arial-Black-Italic":
            case "ArialBlack-Italic":
            case "ArialNarrow-Italic":
                defaultEnc = HelveticaObliqueGlyphs.DefaultEncoding;
                allGlyphs = HelveticaObliqueGlyphs.AllGlyphs;
                break;
            case "Courier-Oblique":
            case "Courier-Italic":
            case "CourierNew-Italic":
            case "CourierNewPS-ItalicMT":
                defaultEnc = CourierObliqueGlyphs.DefaultEncoding;
                allGlyphs = CourierObliqueGlyphs.AllGlyphs;
                break;
            case "Times-BoldItalic":
            case "TimesNewRoman-BoldItalic":
            case "TimesNewRomanPS-BoldItalic":
            case "TimesNewRomanPS-BoldItalicMT":
            case "TimesNewRomanPSMT-BoldItalic":
                defaultEnc = TimesBoldItalicGlyphs.DefaultEncoding;
                allGlyphs = TimesBoldItalicGlyphs.AllGlyphs;
                break;
            case "Helvetica-BoldOblique":
            case "Helvetica-BoldItalic":
            case "ArialUnicodeMS-BoldItalic":
            case "Arial-BoldItalicMT":
            case "Arial-BoldItalic":
            case "Arial,Bold":
            case "Arial-Black-BoldItalic":
            case "ArialBlack-BoldItalic":
            case "ArialNarrow-BoldItalic":
                defaultEnc = HelveticaBoldObliqueGlyphs.DefaultEncoding;
                allGlyphs = HelveticaBoldObliqueGlyphs.AllGlyphs;
                break;
            case "Courier-BoldOblique":
            case "Courier-BoldItalic":
            case "CourierNew-BoldItalic":
            case "CourierNewPS-BoldItalicMT":
                defaultEnc = CourierBoldObliqueGlyphs.DefaultEncoding;
                allGlyphs = CourierBoldObliqueGlyphs.AllGlyphs;
                break;
            default:
                return (null, null);
        }
        return (defaultEnc.ToArray(), allGlyphs);
    }
}


