﻿using PdfLexer.DOM;
using PdfLexer.Parsers;
using System.Collections.Generic;

namespace PdfLexer.Fonts
{
    internal class ToUnicodeFont
    {
        public static IReadableFont GetSimple(ParsingContext ctx, ISimpleUnicode dict)
        {
            var str = dict.ToUnicode;
            using var buffer = str.Contents.GetDecodedBuffer();
            var (ranges, glyphs) = CMapReader.GetGlyphsFromToUnicode(ctx, buffer);

            int fc = dict.FirstChar;
            int lc = dict.LastChar;
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
                    var t = (double)val.GetAs<PdfNumber>();
                    ws[pos] = (float)(t / 1000.0);
                }
                pos++;
            }

            var bbox = dict.FontBBox;

            var lookup = new Glyph[256];
            foreach (var glyph in glyphs)
            {
                var lup = glyph.CodePoint.Value - fc;
                if (lup < ws.Length && lup > -1)
                {
                    glyph.w0 = ws[lup];
                    if (bbox != null)
                    {
                        glyph.BBox = new decimal[] { 0, 0, (decimal)glyph.w0, bbox.URy / 1000.0m }; // todo font matrix vs 1000?
                    }
                }
                else
                {
                    glyph.w0 = mw;
                }

                if (glyph.CodePoint.Value == 32)
                {
                    glyph.IsWordSpace = true;
                }

                if (glyph.CodePoint.Value < 256)
                {
                    lookup[(int)glyph.CodePoint.Value] = glyph;
                }
            }

            var notdef = new Glyph { Char = '\u0000', w0 = mw, IsWordSpace = false, BBox = new decimal[] { 0m, 0m, (decimal)mw, 0m } };
            return new SingleByteFont(dict.FontName, lookup, notdef);
        }

        public static IReadableFont GetComposite(ParsingContext ctx, FontType0 dict)
        {
            var str = dict.ToUnicode;
            using var buffer = str.Contents.GetDecodedBuffer();
            var (ranges, glyphs) = CMapReader.GetGlyphsFromToUnicode(ctx, buffer);

            var bbox = dict.DescendantFont?.FontDescriptor?.FontBBox;

            var mw = dict.DescendantFont.DW / 1000f;

            var lu = new Dictionary<uint, Glyph>();
            foreach (var g in glyphs)
            {
                g.w0 = mw;
                lu[g.CodePoint.Value] = g;
            }
            foreach (var w in dict.DescendantFont.ReadW())
            {
                if (lu.TryGetValue(w.cid, out var glyph))
                {
                    glyph.w0 = w.width / 1000f;
                }
            }

            ranges.Sort((a, b) => a.Bytes - b.Bytes);

            foreach (var g in glyphs)
            {
                if (bbox != null)
                {
                    g.BBox = new decimal[] { 0, 0, (decimal)g.w0, bbox.URy / 1000.0m }; // todo font matrix vs 1000?
                }

                foreach (var range in ranges)
                {
                    if (g.CodePoint >= range.Start && g.CodePoint <= range.End)
                    {
                        range.Glyphs.Add(g);
                    }
                }
            }
            
            return new CMapFont3(glyphs);
        }
    }
}
