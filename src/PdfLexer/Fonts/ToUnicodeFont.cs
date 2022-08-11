using PdfLexer.DOM;
using PdfLexer.Parsers;
using System.Collections.Generic;
using System.Linq;

namespace PdfLexer.Fonts
{
    internal class ToUnicodeFont
    {

        public static Glyph[] GetSimpleGlyphs(ParsingContext ctx, ISimpleUnicode dict)
        {
            var str = dict.ToUnicode;
            using var buffer = str.Contents.GetDecodedBuffer();
            var (ranges, glyphs) = CMapReader.ReadCMap(ctx, buffer.GetData());

            var lookup = new Glyph[256];
            foreach (var glyph in glyphs)
            {
                if (glyph.CodePoint.Value < 256)
                {
                    lookup[(int)glyph.CodePoint.Value] = glyph;
                }
            }
            return lookup;
        }

        public static IReadableFont GetSimple(ParsingContext ctx, ISimpleUnicode dict)
        {
            var lookup = GetSimpleGlyphs(ctx, dict);
            dict.AddWidthInfo(lookup);
            float mw = dict.MissingWidth ?? 0;
            mw = (float)(mw / 1000.0);
            var notdef = new Glyph { Char = '\u0000', w0 = mw, IsWordSpace = false, BBox = new decimal[] { 0m, 0m, (decimal)mw, 0m } };
            return new SingleByteFont(dict.FontName, lookup, notdef);
        }

        public static void AddMissingSimple(ParsingContext ctx, ISimpleUnicode dict, Glyph[] encoding)
        {
            var str = dict.ToUnicode;
            if (str == null)
            {
                return;
            }
            using var buffer = str.Contents.GetDecodedBuffer();
            var (ranges, glyphs) = CMapReader.ReadCMap(ctx, buffer.GetData());

            foreach (var glyph in glyphs)
            {
                if (glyph.CodePoint < 256)
                {
                    if (glyph.CodePoint == 32)
                    {
                        glyph.IsWordSpace = true;
                    }
                    // var g = encoding[glyph.CodePoint.Value];
                    encoding[glyph.CodePoint.Value] = glyph; // overwrite.. need to see if this is really what we want
                }
            }
        }

        public static IReadableFont GetComposite(ParsingContext ctx, FontType0 dict)
        {
            var str = dict.ToUnicode;
            using var buffer = str.Contents.GetDecodedBuffer();
            var (ranges, glyphs) = CMapReader.ReadCMap(ctx, buffer.GetData());
            var bbox = dict.DescendantFont?.FontDescriptor?.FontBBox;

            var mw = dict.DescendantFont.DW / 1000f;

            var lu = new Dictionary<uint, Glyph>();
            foreach (var g in glyphs)
            {
                g.w0 = mw;
                lu[g.CodePoint.Value] = g;
            }

            CIDFontReader.AddWidths(dict, lu);
            CIDFontReader.SetDefaultWidths(dict, lu);

            var notdef = new Glyph { Char = '\u0000', w0 = mw, IsWordSpace = false, BBox = new decimal[] { 0m, 0m, (decimal)mw, 0m } };
            var gs = new FontGlyphSet(lu.Values, notdef);
            var cmap = new CMap(ranges);
            var rng1 = ranges.FirstOrDefault().Bytes;
            if (rng1 == 0)
            {
                rng1 = 1;
            }
            return new CMapFont(cmap, gs, rng1);
        }
    }
}
