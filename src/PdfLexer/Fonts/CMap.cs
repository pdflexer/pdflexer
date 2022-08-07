using PdfLexer.CMaps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PdfLexer.Fonts
{

    internal class CMapRange
    {
        public uint Start { get; set; }
        public uint End { get; set; }
        public int Bytes { get; set; }
    }

    internal class CMap
    {
        private List<CRange>[] rangeSets = new List<CRange>[4];

        public CMap(List<CRange> ranges)
        {
            foreach (var range in ranges)
            {
                var b = range.Bytes - 1;
                if (rangeSets[b] == null)
                {
                    rangeSets[b] = new List<CRange>();
                }
                rangeSets[b].Add(range);
            }
        }

        public uint GetCharCode(ReadOnlySpan<byte> data, int os, out int l)
        {
            uint c = 0;
            var imax = 4;
            var cl = data.Length - os;
            if (cl < 4) { imax = cl; }
            for (var i = 0; i < imax; i++)
            {
                c = (uint)(c << 8) | data[os + i];
                var rangeSet = rangeSets[i];
                if (rangeSet == null) { continue; }
                foreach (var range in rangeSet)
                {
                    if (range.Start <= c && c <= range.End)
                    {
                        l = i + 1;
                        return c;
                    }
                }
            }
            l = 0;
            return 0;
        }

    }

    public class FontGlyphSet
    {
        public Glyph notdef { get; }
        private Glyph[] b1;
        private Dictionary<uint, Glyph> bm;

        public FontGlyphSet(IEnumerable<Glyph> glyphs, Glyph notdef = null)
        {
            this.notdef = notdef;
            b1 = new Glyph[256];
            bm = new Dictionary<uint, Glyph>();
            foreach (var g in glyphs)
            {
                if (g.CodePoint < 256)
                {
                    b1[g.CodePoint.Value] = g;
                }
                else
                {
                    bm[g.CodePoint.Value] = g;
                }
            }
        }

        public Glyph GetGlyph(uint charCode)
        {
            Glyph glyph;
            if (charCode < 256)
            {
                glyph = b1[charCode];
            }
            else
            {
                bm.TryGetValue(charCode, out glyph);
            }
            glyph ??= notdef;
            return glyph;
        }
    }

    internal class CMapFont : IReadableFont
    {
        private CMap _map;
        private CMap _gidMap;
        private FontGlyphSet _glyphSet;

        public bool IsVertical => false;

        public CMapFont(CMap cmap, FontGlyphSet glyphSet, CMap gidMap=null)
        {
            _map = cmap;
            _gidMap = gidMap;
            _glyphSet = glyphSet;
        }

        public int GetGlyph(ReadOnlySpan<byte> data, int os, out Glyph glyph)
        {
            int l;
            uint c;
            if (_gidMap != null)
            {
                c = _gidMap.GetCharCode(data, os, out l);
            } else
            {
                c = _map.GetCharCode(data, os, out l);
            }

            if (l == 0)
            {
                glyph = _glyphSet.notdef;
                return glyph?.Bytes ?? 2;
            }

            glyph = _glyphSet.GetGlyph(c);

            return l;
        }
    }
}
