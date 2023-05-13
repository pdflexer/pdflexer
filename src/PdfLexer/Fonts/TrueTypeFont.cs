using PdfLexer.DOM;
using PdfLexer.Fonts.Files;
using System;
using System.Collections.Generic;

namespace PdfLexer.Fonts
{
    internal class TrueTypeFont
    {
        public static IReadableFont CreateReadable(ParsingContext ctx, FontType1 t1)
        {
            if (t1.Widths == null) // sometimes truetype fonts reference base14 and don't include required info
            {
                var (a, _) = Type1Font.GetBase14Info(t1.BaseFont?.Value);
                if (a != null)
                {
                    return Type1Font.CreateReadable(ctx, t1);
                }
            }

            bool skipFontFile = false;
            if (t1.Encoding != null && t1.Encoding.GetPdfObjType() == PdfObjectType.NameObj)
            {
                var be = t1.Encoding.GetAs<PdfName>();
                // if /Encoding is name /MacRomanEncoding or /WinAnsiEncoding or is Nonsymbolic -> use
                if (be == PdfName.MacRomanEncoding || be == PdfName.WinAnsiEncoding ||
                    (t1.FontDescriptor?.Flags?.HasFlag(FontFlags.Symbolic) ?? false))
                {
                    skipFontFile = true;
                }
            }

            float mw = t1.FontDescriptor?.MissingWidth ?? 0;
            mw = (float)(mw / 1000.0);
            var notdef = new Glyph { Char = '\u0000', w0 = mw, IsWordSpace = false, BBox = new decimal[] { 0m, 0m, (decimal)mw, 0m}, Undefined = true };

            // grab encoding / diffs if exist
            var (hadBase, encoding, diffs) = GetDiffedEncoding(t1);

            // add in postscript info if exists
            if (t1.FontDescriptor?.FontFile2 != null && !skipFontFile)
            {
                AddTrueTypeInfo(ctx, t1.FontDescriptor.FontFile2, t1, hadBase, encoding, diffs);
            }

            Glyph?[] lookup;
            if (t1.ToUnicode != null)
            {
                lookup = GetToUnicodeGlyphs(ctx, t1, t1.ToUnicode);
                for (var i = 0; i < 256; i++)
                {
                    var nm = encoding[i];
                    if (lookup[i] == null && nm != null)
                    {
                        lookup[i] = MapGlyph((uint)i, nm);
                    }
                }
            }
            else
            {
                lookup = new Glyph[256];
                for (var i = 0; i < 256; i++)
                {
                    lookup[i] = MapGlyph((uint)i, encoding[i]);
                }
            }

            AddWidthInfo(t1, lookup);
            return new SingleByteFont(t1.BaseFont ?? "Empty", t1.NativeObject, lookup, notdef);
        }

        private static Glyph? MapGlyph(uint cc, string? name)
        {
            if (name == null) { return null; }
            var g = new Glyph
            {
                Name = name,
                CodePoint = cc,
                IsWordSpace = cc == 32
            };
            if (!GlyphNames.Lookup.TryGetValue(name, out char value))
            {
                value = (char)cc;
                g.GuessedUnicode = true;
            }
            g.Char = value;
            return g;
        }

        public static (bool HadBase, string?[] Encoding, Dictionary<int, string>? Diffs) GetDiffedEncoding(FontType1 t1)
        {
            // grab encoding / diffs if exist
            PdfName? be = null;
            string?[]? baseEncoding = null;
            if (t1.Encoding != null && t1.Encoding.GetPdfObjType() == PdfObjectType.NameObj)
            {
                be = t1.Encoding.GetAs<PdfName>();
                if (be == PdfName.MacRomanEncoding || be == PdfName.WinAnsiEncoding)
                {
                    baseEncoding = Encodings.GetEncoding(be);
                }
            }

            Dictionary<int, string>? diffs = null;
            if (t1.Encoding != null && t1.Encoding.GetPdfObjType() == PdfObjectType.DictionaryObj)
            {
                var diff = (FontEncoding)(PdfDictionary)t1.Encoding;
                if (diff.BaseEncoding != null)
                {
                    baseEncoding = Encodings.GetEncoding(diff.BaseEncoding);
                }
                diffs = new Dictionary<int, string>();
                foreach (var (code, name) in diff.ReadDifferences())
                {
                    diffs[code] = name.Value;
                }
            }

            return (baseEncoding != null, GetDiffedEncoding(baseEncoding, diffs), diffs);
        }

        private static string?[] GetDiffedEncoding(string?[]? baseEncoding, Dictionary<int, string>? diffs)
        {
            var be = new string?[256];
            for (var charCode = 0; charCode < 256; charCode++)
            {
                string? glyphName = null;
                if (diffs != null && diffs.TryGetValue(charCode, out glyphName))
                {
                    // value set;
                }
                else if (baseEncoding != null && (glyphName = baseEncoding[charCode]) != null)
                {
                    // value set;
                }
                else
                {
                    glyphName = Encodings.StandardEncoding[charCode];
                }
                if (glyphName == null)
                {
                    continue;
                }
                be[charCode] = glyphName;
            }
            return be;
        }

        private static Glyph?[] GetToUnicodeGlyphs(ParsingContext ctx, ISimpleUnicode dict, PdfStream data)
        {
            using var buffer = data.Contents.GetDecodedBuffer();
            var (ranges, glyphs, _, _) = CMapReader.ReadCMap(ctx, buffer.GetData());

            var lookup = new Glyph?[256];
            foreach (var glyph in glyphs.Values)
            {
                if (glyph.CodePoint < 256)
                {
                    lookup[(int)glyph.CodePoint] = glyph;
                }
            }
            return lookup;
        }

        private static void AddTrueTypeInfo(ParsingContext ctx, PdfStream file, FontType1 t1, bool hadBase, string?[] preDiffedEncoding,
            Dictionary<int, string>? diffs)
        {
            var symbolic = t1.FontDescriptor?.Flags?.HasFlag(FontFlags.Symbolic) ?? false;
            using var buffer = file.Contents.GetDecodedBuffer();
            var reader = new TrueTypeReader(ctx, buffer.GetData());

            if (!reader.TryGetMaxpGlyphs(out var numGlyphs))
            {
                return;
            }

            string?[]? glyphNames = null;
            if (reader.HasPostTable())
            {

                (_, glyphNames) = reader.ReadPostScriptTable(numGlyphs);
            }


            uint?[] charCodeToGlyphId = new uint?[256];


            Dictionary<uint, string>? gidToName = null;
            TrueTypeReader.TTCMap? cmapTable = null;
            if (reader.HasCMapTable())
            {
                var tables = reader.ReadCMapTables();
                cmapTable = reader.GetPdfCmap(tables, t1.Encoding != null, symbolic);
                reader.TryGetNameMap(tables, out gidToName);
            }
            else
            {
                cmapTable = new TrueTypeReader.TTCMap
                {
                    EncodingId = -1,
                    PlatformId = -1,
                };
            }
            cmapTable.Mappings ??= new Dictionary<uint, uint>(0);

            var windows = cmapTable.PlatformId == 3 && cmapTable.EncodingId == 1;
            var mac = cmapTable.PlatformId == 1 && cmapTable.EncodingId == 0;
            if (hadBase && !symbolic && (windows || mac))
            {
                return;
                // for (var charCode = 0; charCode < 256; charCode++)
                // {
                //     var glyphName = preDiffedEncoding[charCode];
                //     if (glyphName == null)
                //     {
                //         continue;
                //     }
                // 
                //     // TODO recover glyph names
                //     var standardGlyphName = glyphName;
                // 
                //     int unicodeOrCharCode = -1;
                // 
                //     if (windows)
                //     {
                //         if (GlyphNames.Lookup.TryGetValue(standardGlyphName, out char val))
                //         {
                //             unicodeOrCharCode = val;
                //         }
                //     }
                //     else if (mac)
                //     {
                //         unicodeOrCharCode = Array.IndexOf(Encodings.MacRomanEncoding, standardGlyphName);
                //     }
                // 
                //     if (unicodeOrCharCode == -1)
                //     {
                //         // lookup ToUnicode map if exists
                //         if (glyphNames != null && t1.ToUnicode != null)
                //         {
                //             unicodeOrCharCode = 1; // TODO
                //         }
                // 
                //         if (unicodeOrCharCode == -1)
                //         {
                //             continue;
                //         }
                //     }
                // 
                //     if (cmapTable.Mappings.TryGetValue((uint)unicodeOrCharCode, out var gid))
                //     {
                //         charCodeToGlyphId[(uint)charCode] = gid;
                //     }
                // }

            }
            else if (cmapTable.PlatformId == 0)
            {
                foreach (var (cid, gid) in cmapTable.Mappings)
                {
                    if (cid < 256)
                    {
                        charCodeToGlyphId[cid] = gid;
                    }
                }
            }
            else
            {
                // When there is only a (1, 0) cmap table, the char code is a single
                // byte and it is used directly as the char code.

                // When a (3, 0) cmap table is present, it is used instead but the
                // spec has special rules for char codes in the range of 0xF000 to
                // 0xF0FF and it says the (3, 0) table should map the values from
                // the (1, 0) table by prepending 0xF0 to the char codes. To reverse
                // this, the upper bits of the char code are cleared, but only for the
                // special range since some PDFs have char codes outside of this range
                // (e.g. 0x2013) which when masked would overwrite other values in the
                // cmap.
                foreach (var (cc, gid) in cmapTable.Mappings)
                {
                    var charCode = cc;
                    if (
                      cmapTable.PlatformId == 3 &&
                      charCode >= 0xf000 &&
                      charCode <= 0xf0ff
                    )
                    {
                        charCode &= 0xff;
                    }
                    if (charCode < 256)
                    {
                        charCodeToGlyphId[charCode] = gid;
                    }
                    
                }
            }


            // we mapped cc to glyph id
            // add in glyph names, use "" if we have a char without
            // a name, will replace with unicode guess later
            for (uint i = 0; i < 256; ++i)
            {
                var gid = charCodeToGlyphId[i];
                if (!gid.HasValue)
                {
                    continue;
                }
                string? glyphName = null;
                if (glyphNames != null && i < glyphNames.Length && diffs != null && !diffs.TryGetValue((int)i, out glyphName))
                {
                    glyphName = glyphNames[i];
                }

                if (glyphName == null)
                {
                    if (gidToName != null && gidToName.TryGetValue(gid.Value, out var value))
                    {
                        preDiffedEncoding[i] = value; // from an unused cmap that we grabbed unicode vals from
                    }
                    else
                    {
                        preDiffedEncoding[i] = ""; // special to force guess
                    }
                }
                else
                {
                    preDiffedEncoding[i] = glyphName;
                }
            }
        }

        internal static void AddWidthInfo(ISimpleUnicode dict, Glyph?[] glyphs)
        {
            if (dict.FirstChar == null || dict.Widths == null) { return; }
            int fc = dict.FirstChar;
            int lc = (dict.LastChar ?? dict.Widths.Count + fc);
            float mw = dict.MissingWidth ?? 0;
            mw = (float)(mw / 1000.0);
            var ws = new float[lc - fc + 1];
            int pos = 0;

            for (var i = pos; i < ws.Length; i++)
            {
                ws[pos] = mw;
            }
            foreach (var val in dict.Widths)
            {
                if (pos < ws.Length)
                {
                    var t = (double)(val.GetAs<PdfNumber>() ?? 0m);
                    ws[pos] = (float)(t / 1000.0);
                }
                pos++;
            }

            var bbox = dict.FontBBox;

            for (var i = 0; i < 256; i++)
            {
                var glyph = glyphs[i];
                if (glyph == null)
                {
                    glyph = new Glyph
                    {
                        GuessedUnicode = true,
                        Char = (char)i,
                        CodePoint = (uint)i,
                    };
                    glyphs[i] = glyph;
                }
                var lup = i - fc;
                if (lup < ws.Length && lup > -1)
                {
                    glyph.w0 = ws[lup];
                    if (bbox != null)
                    {
                        // TODO revisit bounding box
                        // var x = bbox.LLx / 1000.0m;
                        // glyph.BBox = new decimal[] { x, bbox.LLy / 1000.0m, x + (decimal)glyph.w0, bbox.URy / 1000.0m };
                        glyph.BBox = new decimal[] { 0, bbox.LLy / 1000.0m, (decimal)glyph.w0, bbox.URy / 1000.0m };
                    }
                }
                else
                {
                    glyph.w0 = mw;
                }

                if (glyph.CodePoint == 32)
                {
                    glyph.IsWordSpace = true;
                }
            }
        }
    }
}
