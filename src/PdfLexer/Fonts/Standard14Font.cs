using PdfLexer.DOM;
using PdfLexer.Fonts.StdFonts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PdfLexer.Fonts
{
    internal class Standard14Font : IReadableFont
    {
        private Glyph[] Glyphs;
        private Glyph NotDef;


        public bool IsVertical => false;

        public PdfName Name { get; }

        public Standard14Font(PdfName name, Glyph[] glyphs, Glyph notdef)
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

        public static Standard14Font Create(FontType1 t1)
        {
            if (t1.FontDescriptor?.FontFile != null)
            {
                throw new PdfLexerException("Trying to create standard font from embedded font: " + t1.BaseFont);
            }

            if (t1.ToUnicode != null) 
            {
                throw new PdfLexerException("ToUnicode not implemented for base 14: " + t1.BaseFont);
            }

            var mw = t1.FontDescriptor?.MissingWidth ?? 0;
            FontGlyphMetrics fm = null;
            switch (t1.BaseFont.Value)
            {
                case "/Times-Roman":
                    fm = new FontGlyphMetrics(TimeRomanMetrics.AllGlyphs, TimeRomanMetrics.DefaultEncoding);
                    break;
            }
            var glyphs = fm.GetStandardEncoding();
            var notdef = fm.GetGlyph(".notdef") ?? new Glyph { Char = '\u0000', IsWordSpace = false, BBox = new decimal [] { 0m, 0m, 0m, 0m } };
            notdef.Undefined = true;
            if (t1.FontDescriptor?.NativeObject?.ContainsKey(PdfName.MissingWidth) ?? false)
            {
                notdef = notdef.Clone();
                notdef.w0 = t1.FontDescriptor.MissingWidth;
            }

            if (t1.Encoding != null)
            {
                glyphs = glyphs.ToArray();
                var enc = (FontEncoding)(PdfDictionary)t1.Encoding;
                foreach (var diff in enc.ReadDifferences())
                {
                    if (diff.code > 255)
                    {
                        continue;
                    }
                    var g = fm.GetGlyph(diff.name.Value.Substring(1));
                    if (g != null) // TODO how to handle not found, leave as is or notdef it?
                    {
                        if (diff.code == 32 && g.IsWordSpace == false)
                        {
                            g = g.Clone();
                            g.IsWordSpace = true;
                        }
                        glyphs[diff.code] = g;
                    }
                }
            }

            return new Standard14Font(t1.BaseFont, glyphs, notdef);

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
