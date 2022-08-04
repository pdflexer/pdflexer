using PdfLexer.DOM;
using PdfLexer.Parsers;
using System;
using System.Linq;

namespace PdfLexer.Fonts
{
    internal class TrueType
    {
        public static IReadableFont Get(ParsingContext ctx, FontType1 t1)
        {
            var str = t1.ToUnicode;
            if (str != null)
            {
                return ToUnicodeFont.GetSimple(ctx, t1);
            }

            if (t1.Encoding != null && t1.Encoding.GetPdfObjType() == PdfObjectType.NameObj)
            {
                var be = t1.Encoding.GetAs<PdfName>();
                // if /Encoding is name /MacRomanEncoding or /WinAnsiEncoding or is Nonsymbolic -> use
                if (be == PdfName.MacRomanEncoding || be == PdfName.WinAnsiEncoding || (t1.FontDescriptor?.Flags?.HasFlag(FontFlags.Symbolic) ?? false))
                {
                    var g = Encodings.GetPartialGlyphs(Encodings.GetEncoding(be));
                    return SingleByteFont.FromEncodingAndDifferences(t1, g, g);
                }
            }

            // TODO
            // if dictionary, use /BaseEncoding value, any undefined entry should be filled
            // using StandardEncoding
            // if (3, 1) cmap (MS Unicode)
            // -> first map as above to get glyph name
            //    then glyph name to unicode then unicode to glyph (3,1)
            // if (1,0) cmap (Mac Roman)
            // -> first map as above to get glyph name
            //    get character code from glyph name based on Mac OS std roman (same as what we have stored for MacRomanEncoding except 219 = Euro name
            //    then use code t oget glyph in (1,0)
            // if all fail for a glyph name, look up "post" table if exists and use glyph description
            // 
            return SingleByteFont.Type1Fallback(t1);
        }
    }
}
