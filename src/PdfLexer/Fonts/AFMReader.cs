using PdfLexer.DOM;
using PdfLexer.Fonts.StdFonts;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PdfLexer.Fonts
{
    public class TimesRoman : IWritableFont
    {
        public static TimesRoman Create()
        {
            var bytes = Encoding.UTF8.GetBytes(TimeRomanMetrics.AFM);
            var ms = new MemoryStream(bytes);
            var glyphs = GetGlyphs();
            return new TimesRoman(glyphs.Where(x => -1 != (int)x.CodePoint).ToDictionary(k => k.Char));
        }

        // hacked together for testing
        public static List<Glyph> GetGlyphs()
        {
            var bytes = Encoding.UTF8.GetBytes(TimeRomanMetrics.AFM);
            var ms = new MemoryStream(bytes);
            return AFMReader.GetGlyphsFromAFM(ms);
        }
        private Dictionary<char, Glyph> _glyphs;
        private Glyph[] _fastLookup;
        private int _fastStart;
        private int? _fastEnd;

        public double LineHeight => 0.9;
        public bool SpaceIsWordSpace() => true;

        public TimesRoman(Dictionary<char,Glyph> glyphs, Glyph[]? fastLookup=null, int fastStart=0)
        {
            _glyphs = glyphs;
            _fastLookup = fastLookup;
            _fastStart = fastStart;
            _fastEnd = fastStart + fastLookup?.Length;
        }

        public PdfDictionary GetPdfFont() => AFMReader.GetTimesRoman(_glyphs.Select(x=>x.Value));

        public IEnumerable<(int ByteCount, double Width, double PrevKern)> ConvertFromUnicode(string text, int start, int length, byte[] buffer)
        {
            Glyph lc = null;
            for (var i = start; i < start+length; i++)
            {
                var c = text[i];
                var g = GetGlyph(c);
                buffer[0] = (byte)g.CodePoint; // 1 byte only
                var k = lc == null ? 0 : Getkerning(lc, c);
                yield return (1, g.w0, k);
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
    internal class AFMReader
    {
        // private static Regex C = new Regex("C\\s([-0-9.]+)[\\s]*", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        // private static Regex WX = new Regex("WX\\s([-0-9.]+)[\\s]*", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        // private static Regex N = new Regex("N\\s([a-zA-z]+)[\\s]*", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        // private static Regex B = new Regex("B\\s([-0-9.]+)\\s([-0-9.]+)\\s([-0-9.]+)\\s([-0-9.]+)[\\s]*", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        public static List<Glyph> GetGlyphsFromAFM(Stream str)
        {
            var dict = new Dictionary<string, Glyph>();
            var sr = new StreamReader(str);
            string line = null;
            while ((line = sr.ReadLine()) != null) {
                line = line.Trim();
                if (line.StartsWith("C "))
                {
                    int c = 0;
                    double wx = 0;
                    string n = null;
                    decimal[] b = null;
                    var parts = line.Split(';');
                    foreach (var part in parts)
                    {
                        var txt = part.Trim();
                        if (string.IsNullOrEmpty(txt)) continue;
                        switch (txt[0])
                        {
                            case 'C':
                                {
                                    var segs = txt.Split(' ');
                                    if (segs.Length > 1)
                                    {
                                        int.TryParse(segs[1], out c);
                                    }
                                }
                                break;
                            case 'W':
                                {
                                    var segs = txt.Split(' ');
                                    if (segs.Length > 1)
                                    {
                                        if (double.TryParse(segs[1], out var wxx))
                                        {
                                            wx = wxx / 1000.0;
                                        }
                                    }
                                }
                                break;
                            case 'N':
                                {
                                    var segs = txt.Split(' ');
                                    if (segs.Length > 1)
                                    {
                                        n = segs[1];
                                    }
                                }
                                break;
                            case 'B':
                                {
                                    var segs = txt.Split(' ');
                                    if (segs.Length > 4)
                                    {
                                        b = new decimal[4];
                                        for (var i =0;i<4; i++)
                                        {
                                            if (decimal.TryParse(segs[1+i], out var bb))
                                            {
                                                b[i] = bb / 1000.0m;
                                            }
                                        }
                                    }
                                }
                                break;
                        }

                    }

                    if (n != null)
                    {
                        GlyphNames.Lookup.TryGetValue(n, out var cc);
                        var glyph = new Glyph
                        {
                            Char = cc,
                            CodePoint = c > 0 ? (ushort)c : (ushort)0,
                            w0 = (float)wx,
                            IsWordSpace = c == 32,
                            BBox = b
                        };
                        dict[n] = glyph;
                    }
                    // var c = C.Match(line);
                    // if (!c.Success)
                    // {
                    //     continue;
                    // }
                    // if (!int.TryParse(c.Groups[1].Value, out int cc))
                    // {
                    //     continue;
                    // }
                    // var wx = WX.Match(line);
                    // if (!wx.Success)
                    // {
                    //     continue;
                    // }
                    // if (!double.TryParse(wx.Groups[1].Value, out double w))
                    // {
                    //     continue;
                    // }
                    // 
                    // var n = N.Match(line);
                    // if (!n.Success)
                    // {
                    //     continue;
                    // }
                    // var name = n.Groups[1].Value;
                    // 
                    // var B = B.Match(line)?.Groups[1].Value;
                }
                else if (line.StartsWith("KPX"))
                {
                    var segs = line.Split(' ');
                    if (segs.Length != 4)
                    {
                        continue;
                    }
                    if (!dict.TryGetValue(segs[1], out var g1))
                    {
                        continue;
                    }
                    if (!dict.TryGetValue(segs[2], out var g2))
                    {
                        continue;
                    }
                    if (!decimal.TryParse(segs[3], out var kv))
                    {
                        continue;
                    }
                    g1.Kernings ??= new Dictionary<char, float>();
                    g1.Kernings[g2.Char] = -(float)(kv);
                }
            }
            return dict.Select(x => x.Value).ToList();

        }
        public static FontType1 GetTimesRoman(IEnumerable<Glyph> glyphs)
        {
            var f = new FontType1();
            var d = new FontDescriptor();

            f.BaseFont = "/Times-Roman";

            

            var list = glyphs.Where(x => -1 != (int)x.CodePoint).OrderBy(x => x.CodePoint).ToList();

            int min = (int)list.First().CodePoint;
            int max = (int)list.Last().CodePoint;

            var widths = new PdfArray(new List<IPdfObject>(max-min+1));


            for (var i = min; i <= max; i++)
            {
                widths.Add(PdfCommonNumbers.Zero);
            }
            foreach (var glyph in list)
            {
                widths[glyph.CodePoint - min] = (PdfNumber)(glyph.w0 * 1000);
            }
            f.FirstChar = min;
            f.LastChar = max;
            f.Widths = widths;
            f.FontDescriptor = d;

            d.FontName = "/Times-Roman";
            d.FontFamily = new PdfString("Times");
            d.Flags = FontFlags.Nonsymbolic | FontFlags.Serif;
            
            var bbox = new PdfRectangle();
            bbox.LLx = -168;
            bbox.LLy = -218;
            bbox.URx = 1000;
            bbox.URy = 898;
            d.FontBBox = bbox;
            d.ItalicAngle = PdfCommonNumbers.Zero; // ItalicAngle
            d.Ascent = 683;
            d.Descent = -217;
            d.CapHeight = 662;
            d.StemV = 84;
            d.StemH = 28;

            return f;
        }
    }

}
