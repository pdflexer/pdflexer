using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace PdfLexer.Fonts.Files
{
    [ExcludeFromCodeCoverage]
    internal class AFMReader
    {
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
                            CodePoint = c >= 0 ? (ushort?)c : null,
                            w0 = (float)wx,
                            IsWordSpace = c == 32,
                            BBox = b,
                            Name = n,
                            Undefined = false // c == -1,
                        };
                        dict[n] = glyph;
                    }
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

    }
}
