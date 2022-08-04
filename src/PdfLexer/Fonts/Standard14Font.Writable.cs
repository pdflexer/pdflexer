using PdfLexer.DOM;
using PdfLexer.Fonts.Predefined;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PdfLexer.Fonts
{
    public partial class SingleByteFont
    {
        public static IWritableFont GetTimesRoman()
        {
            var d = new Dictionary<char, Glyph>();
            foreach (var g in TimesRomanGlyphs.DefaultEncoding)
            {
                if (g!=null)
                {
                    d[g.Char] = g;
                }
            }
            return new Standard14FontWritable(FontMetrics.TimesRoman, d);
        }
    }


    internal class Standard14FontWritable : IWritableFont
    {
        private FontMetrics _metrics;
        private Dictionary<char, Glyph> _glyphs;
        private Glyph[] _fastLookup;
        private int _fastStart;
        private int? _fastEnd;

        public double LineHeight => 0.9;
        public bool SpaceIsWordSpace() => true;


        public Standard14FontWritable(FontMetrics metrics, Dictionary<char, Glyph> glyphs, Glyph[]? fastLookup = null, int fastStart = 0)
        {
            _metrics = metrics;
            _glyphs = glyphs;
            if (fastLookup == null)
            {
                _fastLookup = new Glyph[256];
                foreach (var item in _glyphs.Values)
                {
                    var v = (int)item.Char;
                    if (v < 255)
                    {
                        _fastLookup[v] = item;
                    }
                    _fastEnd = 255;
                }
            } else
            {
                _fastLookup = fastLookup;
                _fastStart = fastStart;
                _fastEnd = fastStart + fastLookup?.Length-1;
            }
        }

        public PdfDictionary GetPdfFont()
        {
            var f = new FontType1();
            f.BaseFont = _metrics.BaseFont;
            var list = _glyphs.Values.Where(x => x.CodePoint.HasValue).OrderBy(x => x.CodePoint).ToList();
            int min = (int)list.First().CodePoint;
            int max = (int)list.Last().CodePoint;
            var widths = new PdfArray(new List<IPdfObject>(max - min + 1));

            for (var i = min; i <= max; i++)
            {
                widths.Add(PdfCommonNumbers.Zero);
            }
            foreach (var glyph in list)
            {
                if (glyph.CodePoint.HasValue)
                {
                    var lu = (int)glyph.CodePoint.Value - min;
                    widths[lu] = (PdfNumber)(glyph.w0 * 1000);
                }

            }
            f.FirstChar = min;
            f.LastChar = max;
            f.Widths = widths;
            f.FontDescriptor = _metrics.Descriptor;
            return f;
        }

        public IEnumerable<SizedChar> ConvertFromUnicode(string text, int start, int length, byte[] buffer)
        {
            Glyph lc = null;
            for (var i = start; i < start + length; i++)
            {
                var c = text[i];
                var g = GetGlyph(c);
                buffer[0] = (byte)g.CodePoint; // 1 byte only
                var k = lc == null ? 0 : Getkerning(lc, c);
                yield return new SizedChar { ByteCount = 1, Width = g.w0, PrevKern = k };
                lc = g;
            }
        }

        private Glyph GetGlyph(char c)
        {
            if (_fastLookup != null && c >= _fastStart && c <= _fastEnd)
            {
                var b = _fastLookup[c];
                if (b != null)
                {
                    return b;
                }
            }
            if (!_glyphs.TryGetValue(c, out var bb))
            {
                throw new PdfLexerException($"Char {c} not part of default Times-Roman encoding.");
            }
            return bb;
        }

        private float Getkerning(Glyph c, char c2)
        {
            var k = 0.0f;
            if (c.Kernings == null)
            {
                return k;
            }

            c.Kernings.TryGetValue(c2, out k);
            return k;
        }
    }


}
