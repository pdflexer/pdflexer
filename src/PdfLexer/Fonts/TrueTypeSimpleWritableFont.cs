using PdfLexer.DOM;
using PdfLexer.Filters;
using PdfLexer.Fonts.Files;

namespace PdfLexer.Fonts;

internal class TrueTypeSimpleWritableFont : IWritableFont
{
    private readonly TrueTypeEmbeddedFont _info;
    private readonly Dictionary<char, Glyph> _glyphs;
    private readonly Glyph[] _fastLookup;
    private readonly int _fastStart;
    private readonly int? _fastEnd;
    private readonly UnknownCharHandling _charHandling;

    public double LineHeight => throw new NotImplementedException();

    public TrueTypeSimpleWritableFont(TrueTypeEmbeddedFont info, UnknownCharHandling charHandling, Glyph[]? fastLookup = null, int fastStart = 0)
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
                } else if (_charHandling == UnknownCharHandling.Skip)
                {
                    continue;
                } else
                {
                    buffer[0] = 0;
                    yield return new SizedChar { ByteCount = 1, Width = 0, PrevKern = 0 };
                    lc = null;
                    continue;
                }
            }
            buffer[0] = (byte)(g.CodePoint ?? 0); // 1 byte only
            var k = lc == null ? 0 : Getkerning(lc, c);
            yield return new SizedChar { ByteCount = 1, Width = g.w0, PrevKern = k };
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
        if (c > 255) { return null; } // single byte only for simple

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
        var f = new FontType1();
        f.NativeObject[PdfName.Subtype] = PdfName.TrueType;
        f.BaseFont = _info.PostScriptName;

        var list = _glyphs.Values.Where(x => x.CodePoint.HasValue && x.CodePoint.Value < 256).OrderBy(x => x.CodePoint).ToList(); // simple 1 byte only
        int min = (int)(list.First().CodePoint ?? 0);
        int max = (int)(list.Last().CodePoint ?? 0);
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
        f.Encoding = _info.DefaultEncoding;
        f.FirstChar = min;
        f.LastChar = max;
        f.Widths = widths;
       
        f.FontDescriptor = _info.Descriptor;
        return f;
    }

    public bool SpaceIsWordSpace() => true;
}
