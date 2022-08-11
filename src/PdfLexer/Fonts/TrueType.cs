﻿using PdfLexer.DOM;
using PdfLexer.Fonts.Files;
using PdfLexer.Parsers;
using System;
using System.Collections.Generic;

namespace PdfLexer.Fonts
{
    internal class TrueType
    {
        public static IReadableFont Get(ParsingContext ctx, FontType1 t1)
        {
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
            var notdef = new Glyph { Char = '\u0000', w0 = mw, IsWordSpace = false, BBox = new decimal[] { 0m, 0m, (decimal)mw, 0m } };

            // grab encoding / diffs if exist
            var (hadBase, encoding, diffs) = t1.GetDiffedEncoding();

            // add in postscript info if exists
            if (t1.FontDescriptor?.FontFile2 != null && !skipFontFile)
            {
                AddTrueTypeInfo(ctx, t1, hadBase, encoding, diffs);
            }

            var str = t1.ToUnicode;
            if (str != null)
            {
                var lookup = ToUnicodeFont.GetSimpleGlyphs(ctx, t1);
                for (var i = 0; i < 256; i++)
                {
                    var nm = encoding[i];
                    if (lookup[i] == null && nm != null)
                    {
                        lookup[i] = MapGlyph((uint)i, nm);
                    }
                }
                t1.AddWidthInfo(lookup);
                return new SingleByteFont(t1.BaseFont, lookup, notdef);
            }
            else
            {
                var lookup = new Glyph[256];
                for (var i = 0; i < 256; i++)
                {
                    lookup[i] = MapGlyph((uint)i, encoding[i]);
                }
                t1.AddWidthInfo(lookup);
                return new SingleByteFont(t1.BaseFont, lookup, notdef);
            }
        }

        private static Glyph MapGlyph(uint cc, string name)
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

        private static void AddTrueTypeInfo(ParsingContext ctx, FontType1 t1, bool hadBase, string[] preDiffedEncoding,
            Dictionary<int, string> diffs)
        {
            var symbolic = t1.FontDescriptor?.Flags?.HasFlag(FontFlags.Symbolic) ?? false;
            using var buffer = t1.FontDescriptor.FontFile2.Contents.GetDecodedBuffer();
            var reader = new TrueTypeReader(ctx, buffer.GetData());

            if (!reader.TryGetMaxpGlyphs(out var numGlyphs))
            {
                return;
            }

            string[] glyphNames = null;
            if (reader.HasPostTable())
            {

                (_, glyphNames) = reader.ReadPostScriptTable(numGlyphs);
            }


            uint?[] charCodeToGlyphId = new uint?[256];

            TrueTypeReader.TTCMap cmapTable = null;
            if (reader.HasCMapTable())
            {
                cmapTable = reader.ReadCMapTable(t1.Encoding != null, symbolic);
            }
            else
            {
                cmapTable = new TrueTypeReader.TTCMap
                {
                    EncodingId = -1,
                    PlatformId = -1,
                    Mappings = new Dictionary<uint, uint>(0)
                };
            }

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
                    charCodeToGlyphId[charCode] = gid;
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

                if (!diffs.TryGetValue((int)i, out var glyphName))
                {
                    glyphName = glyphNames[i];
                }

                if (glyphName == null)
                {
                    preDiffedEncoding[i] = ""; // special to force guess
                }
                else
                {
                    preDiffedEncoding[i] = glyphName;
                }
            }
        }
    }
}