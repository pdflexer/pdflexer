using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer.Fonts
{
    internal class FontGlyphMetrics
    {
        private Dictionary<string, Glyph> _all;
        private Glyph[] _default;

        public FontGlyphMetrics(Dictionary<string, Glyph> allGlyphs, Glyph[] defaultEncoding)
        {
            _all = allGlyphs;
            _default = defaultEncoding;
        }
        public Glyph[] GetStandardEncoding()
        {
            return _default;
        }
        public Glyph GetGlyph(string name)
        {
            _all.TryGetValue(name, out Glyph glyph);
            return glyph;
        }
    }
}
