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
        private Dictionary<uint, CResult>? mapping;
        internal List<CRange> Ranges;

        public CMap(List<CRange> ranges, Dictionary<uint, CResult>? mapping = null)
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
            this.mapping = mapping;
            Ranges = ranges;
        }

        public bool HasMapping { get => mapping != null; }


        public bool TryGetMapping([NotNullWhen(true)]out Dictionary<uint, CResult>? val)
        {
            val = null;
            if (mapping == null) { return false; }
            val = mapping;
            return true;
        }

        public bool TryGetFallback(uint cp, out CResult val)
        {
            if (mapping == null) { val = default; return false; }
            return mapping.TryGetValue(cp, out val);
        }

        public uint GetCID(uint c)
        {
            if (mapping != null && mapping.TryGetValue(c, out var cr))
            {
                c = cr.Code;
            }
            return c;
        }

        public uint GetCodePoint(ReadOnlySpan<byte> data, int os, out int l)
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

    internal class FontGlyphSet
    {
        public Glyph notdef { get; }
        private Glyph[] b1;
        private Dictionary<uint, Glyph> bm;

        public FontGlyphSet(Glyph[] b1, Dictionary<uint, Glyph> glyphs, Glyph notdef)
        {
            this.notdef = notdef;
            this.b1 = b1;
            this.bm = glyphs;
        }
        public FontGlyphSet(IEnumerable<Glyph> glyphs, Glyph notdef)
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
                    bm[g.CodePoint ?? 0] = g;
                }
            }
        }

        public Glyph GetGlyph(uint charCode)
        {
            Glyph? glyph;
            if (charCode < 256 && b1 != null)
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

    internal class CompositeFont : IReadableFont
    {
        private readonly CMap _encoding;
        private readonly CMap? _cidInfo;
        private readonly FontGlyphSet _glyphSet;
        private readonly int _notdefBytes;

        public bool IsVertical { get; }
        public string Name { get; }

        public CompositeFont(string name, CMap encoding, FontGlyphSet glyphSet, int notdefBytes, bool vertical, CMap? cidInfo = null)
        {
            Name = name;
            IsVertical = vertical;
            _encoding = encoding;
            _cidInfo = cidInfo;
            _glyphSet = glyphSet;
            _notdefBytes = notdefBytes;
        }

        public int GetGlyph(ReadOnlySpan<byte> data, int os, out Glyph glyph)
        {
            int l;
            uint c;

            c = _encoding.GetCodePoint(data, os, out l);
            if (l == 0)
            {
                glyph = _glyphSet.notdef;
                return _notdefBytes;
            }
            c = _encoding.GetCID(c); // convert cp to cid, does nothing with identity

            glyph = _glyphSet.GetGlyph(c);
            if (glyph == _glyphSet.notdef && _cidInfo != null && _cidInfo.HasMapping)
            {
                if (_cidInfo.TryGetFallback(c, out var val))
                {
                    glyph = glyph.Clone();
                    glyph.MultiChar = val.MultiChar;
                    glyph.Char = (char)val.Code;
                    glyph.BBox = _glyphSet.notdef.BBox;
                }
            }

            return l;
        }
    }
}
