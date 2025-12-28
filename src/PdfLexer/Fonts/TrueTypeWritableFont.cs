using PdfLexer.DOM;
using PdfLexer.Filters;
using PdfLexer.Fonts.Files;
using System.Security.Principal;
using System.Text;

namespace PdfLexer.Fonts;

internal class TrueTypeWritableFont : IWritableFont
{
    private readonly TrueTypeEmbeddedFont _info;
    private readonly Dictionary<char, Glyph> _glyphs;
    private readonly Glyph[] _fastLookup;
    private readonly int _fastStart;
    private readonly int? _fastEnd;
    private readonly UnknownCharHandling _charHandling;
    private readonly PdfDictionary _xobjDictionary;
    public double LineHeight => throw new NotImplementedException();

    public TrueTypeWritableFont(TrueTypeEmbeddedFont info, UnknownCharHandling charHandling, Glyph[]? fastLookup = null, int fastStart = 0)
    {
        _charHandling = charHandling;
        _info = info;
        _glyphs = info.Glyphs;
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
            }
            _fastEnd = 255;
        }
        else
        {
            _fastLookup = fastLookup;
            _fastStart = fastStart;
            _fastEnd = fastStart + fastLookup?.Length - 1;
        }
        _xobjDictionary = CreateDict();
    }

    public IEnumerable<SizedChar> ConvertFromUnicode(string text, int start, int length, byte[] buffer)
    {
        Glyph? lc = null;
        for (var i = start; i < start + length; i++)
        {
            var c = text[i];
            var g = GetGlyph(c);
            if (g == null)
            {
                if (_charHandling == UnknownCharHandling.Error)
                {
                    throw new PdfLexerException($"Char {c} not part of encoding for embedded true type font {_info.PostScriptName}");
                }
                else if (_charHandling == UnknownCharHandling.Skip)
                {
                    continue;
                }
                else
                {
                    buffer[0] = 0;
                    yield return new SizedChar { ByteCount = 1, Width = 0, PrevKern = 0 };
                    lc = null;
                    continue;
                }
            }
            var k = lc == null ? 0 : Getkerning(lc, c);

            var cp = (g.CodePoint ?? 0);
            if (cp < 65536)
            {
                buffer[0] = (byte)(cp >> 8);
                buffer[1] = (byte)(cp & 0xFF);
                yield return new SizedChar { ByteCount = 2, Width = g.w0, PrevKern = k, AddWordSpace = c == ' ' };

            }
            else
            {
                throw new NotSupportedException("Unicode values above 65535 not supported");
            }

            lc = g;
        }
    }

    public IEnumerable<GlyphOrShift> GetGlyphs(string text)
    {
        Glyph? lc = null;
        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            var g = GetGlyph(c);
            if (g == null)
            {
                if (_charHandling == UnknownCharHandling.Error)
                {
                    throw new PdfLexerException($"Char {c} not part of encoding for embedded true type font {_info.PostScriptName}");
                }
                else if (_charHandling == UnknownCharHandling.Skip)
                {
                    continue;
                }
                else
                {
                    lc = null;
                    yield return new GlyphOrShift(0);
                    continue;
                }
            }
            var k = lc == null ? 0 : Getkerning(lc, c);
            if (k != 0)
            {
                yield return new GlyphOrShift(k);
            }
            yield return new GlyphOrShift(g, 0, 2);
            lc = g;
        }
    }

    private Glyph? GetGlyph(char c)
    {
        if (_fastLookup != null && c >= _fastStart && c <= _fastEnd)
        {
            var b = _fastLookup[c];
            if (b != null)
            {
                return b;
            }
        }

        _glyphs.TryGetValue(c, out var bb);
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

    public PdfDictionary GetPdfFont() => _xobjDictionary;

    private PdfDictionary CreateDict()
    {
        var f = new FontType0();
        f.BaseFont = _info.PostScriptName;
        f.Encoding = PdfName.IdentityH;

        var cid = new FontCID(true);
        cid.BaseFont = _info.PostScriptName;
        cid.CIDSystemInfo = new CIDSystemInfo
        {
            Registry = new PdfString("Adobe"),
            Ordering = new PdfString("Identity"),
            Supplement = new PdfIntNumber(0)
        };

        cid.DW = new PdfIntNumber(1000);
        cid.CIDToGIDMap = PdfName.Identity;

        // build FD
        cid.FontDescriptor = _info.Descriptor;

        // build widths
        var w = new List<IPdfObject>();
        CalcWidthArray(w);
        cid.W = new PdfArray(w);

        f.DescendantFont = cid;

        {
            using var writer = new ZLibLexerStream();
            var bytes = CreateToUnicodeCMap(writer);
            var str = new PdfStream(writer.Complete());
            str.Dictionary[PdfName.TYPE] = PdfName.CMap;
            f.ToUnicode = str;
        }

        return f;
    }

    private void CalcWidthArray(List<IPdfObject> a)
    {
        // [ CID1 [W1 W2 ... Wn] (individual widths for CID1-n
        //   CIDX CIDY W1 (range of CIDs from CIDX to CIDY with width W1)
        // ]

        bool inSingles = false;
        int groupStart = 0;
        foreach (var (start, value, count) in GetWidthSegments())
        {
            if (count == 1)
            {
                if (inSingles)
                {
                    continue;
                } else
                {
                    groupStart = start;
                    inSingles = true;
                }
            }
            else
            {
                if (inSingles)
                {
                    // finished a singles section
                    Add(groupStart);
                    AppendRange(groupStart, start - 1);
                }
                inSingles = false;
                Add(start);
                Add(start + count - 1);
                Add(_info.GlyphWidths[start]);
            }
        }

        void Add(int v)
        {
            a.Add(new PdfIntNumber(v));
        }

        void AppendRange(int from, int thru)
        {
            var r = new PdfArray();
            for (var i = from; i <= thru; i++)
            {
                r.Add(new PdfIntNumber(_info.GlyphWidths[i]));
            }
            a.Add(r);
        }
    }

    private IEnumerable<(int start, int value, int count)> GetWidthSegments()
    {
        int cnt = 0;
        int? last = null;
        int start = 0;

        for (int i = 0; i < _info.GlyphWidths.Length; i++)
        {
            var v = _info.GlyphWidths[i];
            if (last.HasValue)
            {
                if (last == v)
                {
                    cnt++;
                    continue;
                } else
                {
                    yield return (start, last.Value, cnt);
                    start = i;
                    cnt = 1;
                    last = v;
                }
            } else
            {
                start = i;
                last = v;
                cnt++;
            }
        }
    }

    // CreateToUnicodeCMap conversion ported from pdfcpu
    // (https://github.com/pdfcpu/pdfcpu/blob/865e6b7b49fb3ca732516c964db7ddde5022242f/pkg/pdfcpu/font/fontDict.go#L544)
    // pdfcpu is licensed as follows:
    /* Copyright 2012 Mozilla Foundation
     *
     * Licensed under the Apache License, Version 2.0 (the "License");
     * you may not use this file except in compliance with the License.
     * You may obtain a copy of the License at
     *
     *     http://www.apache.org/licenses/LICENSE-2.0
     *
     * Unless required by applicable law or agreed to in writing, software
     * distributed under the License is distributed on an "AS IS" BASIS,
     * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
     * See the License for the specific language governing permissions and
     * limitations under the License.
     */
    private int CreateToUnicodeCMap(Stream output)
    {
        var start = output.Position;
        var pfx = """
            /CIDInit /ProcSet findresource begin
            12 dict begin
            begincmap
            /CIDSystemInfo <<
            	/Registry (Adobe)
            	/Ordering (UCS)
            	/Supplement 0
            >> def
            /CMapName /Adobe-Identity-UCS def
            /CMapType 2 def

            """u8;

        var r = """
            1 begincodespacerange
            <0000> <FFFF>
            endcodespacerange

            """u8;

        var epi = """
            endcmap
            CMapName currentdict /CMap defineresource pop
            end
            end
            """u8;

        output.Write(pfx);
        output.Write(r);


        var lu = new Dictionary<uint, uint>();
        lu[0] = 0;
        foreach (var g in _glyphs.Values)
        {
            var u = g.CodePoint;
            if (u != null && !lu.ContainsKey(u.Value))
            {
                lu.Add(u.Value, g.Char);
            }
        }
        List<uint> gids = lu.Keys.ToList();
        gids.Sort();

        int c = 100;
        if (gids.Count < 100)
        {
            c = gids.Count;
        }
        int l = gids.Count;

        var b = new StringBuilder();

        b.AppendFormat("{0} beginbfchar\n", c);
        int j = 1;
        for (int i = 0; i < l; i++)
        {
            uint gid = gids[i];
            b.AppendFormat("<{0:X4}> <", gid);
            if (lu.TryGetValue(gid, out var u))
            {

                string s = char.ConvertFromUtf32((int)u);
                foreach (char v in s)
                {
                    b.AppendFormat("{0:X4}", (int)v);
                }
            }
            else
            {

                b.AppendFormat("{0:X4}", 0);
            }
            b.Append(">\n");
            if (j % 100 == 0)
            {
                b.Append("endbfchar\n");
                if (l - i < 100)
                {
                    c = l - i - 1;
                }
                b.AppendFormat("{0} beginbfchar\n", c);
            }
            j++;
        }
        b.Append("endbfchar\n");
        output.Write(Encoding.UTF8.GetBytes(b.ToString()));
        output.Write(epi);

        var e = output.Position;
        return (int)(e - start);
    }

    public bool SpaceIsWordSpace() => false;
}
