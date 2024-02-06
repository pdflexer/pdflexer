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
                _fastEnd = 255;
            }
        }
        else
        {
            _fastLookup = fastLookup;
            _fastStart = fastStart;
            _fastEnd = fastStart + fastLookup?.Length - 1;
        }
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
                yield return new SizedChar { ByteCount = 2, Width = g.w0, PrevKern = k };
            }
            else
            {
                throw new NotSupportedException("Unicode values above 65535 not supported");
            }

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

    public PdfDictionary GetPdfFont()
    {
        var f = new FontType0();
        f.BaseFont = _info.PostScriptName;
        f.Encoding = PdfName.IdentityH;

        // f.ToUnicode

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
        // cid.W


        f.DescendantFont = cid;


        return f;
    }

    private void Create(Stream output)
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
        foreach (var g in _glyphs.Values)
        {
            var u = g.CodePoint;
            if (u != null)
            {
                lu.Add(u.Value, g.Char);
            }
        }
        List<uint> gids = lu.Values.ToList();
        gids.Sort();

        int c = 100;
        if (gids.Count < 100)
        {
            c = gids.Count;
        }
        int l = c;

        var b = new StringBuilder();

        b.AppendFormat("{0} beginbfchar\n", c);
        int j = 1;
        for (int i = 0; i < l; i++)
        {
            uint gid = gids[i];
            b.AppendFormat("<{0:X4}> <", gid);
            int u = (int)lu[gid];
            string s = char.ConvertFromUtf32(u);
            foreach (char v in s)
            {
                b.AppendFormat("{0:X4}", Convert.ToUInt16(v));
            }
            b.Append(">\n");
            if (j % 100 == 0)
            {
                b.Append("endbfchar\n");
                if (l - i < 100)
                {
                    c = l - i;
                }
                b.AppendFormat("{0} beginbfchar\n", c);
            }
            j++;
        }
        b.Append("endbfchar\n");
        output.Write(Encoding.UTF8.GetBytes(b.ToString()));
        output.Write(epi);

        var e = output.Position;
        var totalBytes = e - start;
    }

    public bool SpaceIsWordSpace() => true;
}
