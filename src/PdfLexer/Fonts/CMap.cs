using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer.Fonts
{
    internal class CMapFont3 : IReadableFont
    {
        private Glyph notdef;
        private Glyph[] b1;
        private Dictionary<uint, Glyph> b4;
        public bool IsVertical => false;
        public CMapFont3(IEnumerable<Glyph> glyphs, Glyph notdef = null)
        {
            this.notdef = notdef;
            b1 = new Glyph[256];
            b4 = new Dictionary<uint, Glyph>();
            foreach (var glyph in glyphs)
            {
                if (glyph.CodePoint == null)
                {
                    continue;
                }
                if (glyph.CodePoint < 256)
                {
                    b1[(int)glyph.CodePoint] = glyph;
                }
                else
                {
                    b4[glyph.CodePoint.Value] = glyph;
                }
            }
        }

        public int GetGlyph(ReadOnlySpan<byte> data, int os, out Glyph glyph)
        {
            var v1 = data[os];
            glyph = b1[v1];
            if (glyph != null && glyph.Bytes < 2)
            {
                return 1;
            }
            var remaining = data.Length - os;
            if (remaining < 2)
            {
                glyph = notdef;
                return 1;
            }
            uint cp = (uint)(v1 << 8) | data[os + 1];
            if (TryGetValue(cp, 2, out glyph))
            {
                return 2;
            }
            if (remaining < 3)
            {
                glyph = notdef;
                return 2;
            }
            cp = cp << 8 | data[os + 2];
            if (TryGetValue(cp, 3, out glyph))
            {
                return 3;
            }
            if (remaining < 4)
            {
                glyph = notdef;
                return 3;
            }
            cp = cp << 8 | data[os + 3];
            if (TryGetValue(cp, 4, out glyph))
            {
                return 4;
            }
            glyph = notdef;
            return 1;
        }

        private bool TryGetValue(uint cp, int bytes, out Glyph glyph)
        {
            if (cp < 256)
            {
                glyph = b1[cp];
                if (glyph != null && glyph.Bytes == bytes)
                {
                    return true;
                }
            }
            if (!b4.TryGetValue(cp, out glyph))
            {
                return false;
            }
            if (glyph.Bytes != bytes)
            {
                glyph = null;
                return false;
            }
            return true;
        }
    }


    internal class CMapFont : IReadableFont
    {
        private Glyph notdef;
        private Glyph[] b1;
        private Dictionary<int, Glyph> b2;
        private Dictionary<uint, Glyph> b3;
        private Dictionary<uint, Glyph> b4;
        public bool IsVertical => false;
        public CMapFont(Glyph[] glyphs, Glyph notdef = null)
        {
            this.notdef = notdef;
            b1 = new Glyph[256];
            b2 = new Dictionary<int, Glyph>();
            b3 = new Dictionary<uint, Glyph>();
            b4 = new Dictionary<uint, Glyph>();
            foreach (var glyph in glyphs)
            {
                if (glyph.CodePoint == null)
                {
                    continue;
                }
                if (glyph.CodePoint < 256)
                {
                    b1[(int)glyph.CodePoint] = glyph;
                }
                else if (glyph.CodePoint <= 0xFFFF)
                {
                    b2[(int)glyph.CodePoint.Value] = glyph;
                }
                else if (glyph.CodePoint <= 0xFFFFFF)
                {
                    b3[glyph.CodePoint.Value] = glyph;
                }
                else
                {
                    b4[glyph.CodePoint.Value] = glyph;
                }
            }
        }

        public CMapFont(IEnumerable<Glyph>? b1g, IEnumerable<Glyph>? b2g, IEnumerable<Glyph>? b3g, IEnumerable<Glyph>? b4g, Glyph notdef = null)
        {
            this.notdef = notdef;
            b1 = new Glyph[256];
            if (b1g != null)
            {
                foreach (var g in b1g)
                {
                    if (g?.CodePoint == null)
                    {
                        continue;
                    }
                    b1[(int)g.CodePoint] = g;
                }
            }

            b2 = new Dictionary<int, Glyph>();
            if (b2g != null)
            {
                foreach (var g in b2g)
                {
                    if (g?.CodePoint == null)
                    {
                        continue;
                    }
                    b2[(int)g.CodePoint.Value] = g;
                }
            }

            b3 = new Dictionary<uint, Glyph>();
            if (b3g != null)
            {
                foreach (var g in b3g)
                {
                    if (g?.CodePoint == null)
                    {
                        continue;
                    }
                    b3[g.CodePoint.Value] = g;

                }
            }
            b4 = new Dictionary<uint, Glyph>();
            if (b4g != null)
            {
                foreach (var g in b4g)
                {
                    if (g?.CodePoint == null)
                    {
                        continue;
                    }
                    b4[g.CodePoint.Value] = g;
                }
            }
        }

        public int GetGlyph(ReadOnlySpan<byte> data, int os, out Glyph glyph)
        {
            var v1 = data[os];
            glyph = b1[v1];
            if (glyph != null)
            {
                return 1;
            }
            var remaining = data.Length - os;
            if (remaining < 2)
            {
                glyph = notdef;
                return 1;
            }
            uint cp = (uint)(v1 << 8) | data[os + 1];
            if (b2.TryGetValue((int)cp, out glyph))
            {
                return 2;
            }
            if (remaining < 3)
            {
                glyph = notdef;
                return 2;
            }
            cp = cp << 8 | data[os + 2];
            if (b3.TryGetValue(cp, out glyph))
            {
                return 3;
            }
            if (remaining < 4)
            {
                glyph = notdef;
                return 3;
            }
            cp = cp << 8 | data[os + 3];
            if (b4.TryGetValue(cp, out glyph))
            {
                return 4;
            }
            glyph = notdef;
            return 1;

        }
    }

    internal class CMapRange
    {
        public uint Start { get; set; }
        public uint End { get; set; }
        public int Bytes { get; set; }
        public List<Glyph> Glyphs { get; set; }
    }

    internal class CMapFont2 : IReadableFont
    {
        private Glyph notdef;
        private Glyph[] b1;
        private Dictionary<int, Glyph> b2;
        private Dictionary<uint, Glyph> b3;
        private Dictionary<uint, Glyph> b4;
        public bool IsVertical => false;

        private List<CMapRange> b22;
        private List<CMapRange> b33;
        private List<CMapRange> b44;

        public CMapFont2(List<CMapRange> ranges, Glyph notdef = null)
        {
            this.notdef = notdef;
            b1 = new Glyph[256];
            
            b2 = new Dictionary<int, Glyph>();
            b22 = new List<CMapRange>();
            b3 = new Dictionary<uint, Glyph>();
            b33 = new List<CMapRange>();
            b4 = new Dictionary<uint, Glyph>();
            b44 = new List<CMapRange>();
            foreach (var range in ranges)
            {
                switch(range.Bytes)
                {
                    case 1:
                        {
                            foreach (var g in range.Glyphs)
                            {
                                b1[(int)g.CodePoint] = g;
                            }
                        }
                        break;
                    case 2:
                        {
                            foreach (var g in range.Glyphs)
                            {
                                b2[(int)g.CodePoint] = g;
                            }
                        }
                        b22.Add(range);
                        break;
                    case 3:
                        {
                            foreach (var g in range.Glyphs)
                            {
                                b3[g.CodePoint.Value] = g;
                            }
                        }
                        b33.Add(range);
                        break;
                    case 4:
                        {
                            foreach (var g in range.Glyphs)
                            {
                                b4[g.CodePoint.Value] = g;
                            }
                        }
                        b44.Add(range);
                        break;
                }
            }
        }

        public int GetGlyph(ReadOnlySpan<byte> data, int os, out Glyph glyph)
        {
            var v1 = data[os];
            glyph = b1[v1];
            if (glyph != null)
            {
                return 1;
            }
            var remaining = data.Length - os;
            if (remaining < 2)
            {
                glyph = notdef;
                return 1;
            }

            uint cp = (uint)(v1 << 8) | data[os + 1];
            if (Matches(b22, cp))
            {
                if (b2.TryGetValue((int)cp, out glyph))
                {
                    return 2;
                } else
                {
                    glyph = notdef;
                    return 2;
                }
            }

            if (remaining < 3)
            {
                glyph = notdef;
                return 2;
            }
            cp = cp << 8 | data[os + 2];
            if (Matches(b33, cp))
            {
                if (b3.TryGetValue(cp, out glyph))
                {
                    return 3;
                }
                else
                {
                    glyph = notdef;
                    return 3;
                }
            }

            if (remaining < 4)
            {
                glyph = notdef;
                return 3;
            }
            cp = cp << 8 | data[os + 3];
            if (Matches(b44, cp))
            {
                if (b4.TryGetValue(cp, out glyph))
                {
                    return 4;
                }
                else
                {
                    glyph = notdef;
                    return 4;
                }
            }

            glyph = notdef;
            return 1;
        }

        private static bool Matches(List<CMapRange> range, uint value)
        {
            bool match = false;
            foreach (var section in range)
            {
                if (value >= section.Start && value <= section.End)
                {
                    match = true;
                    break;
                }
            }
            return match;
        }
    }
}
