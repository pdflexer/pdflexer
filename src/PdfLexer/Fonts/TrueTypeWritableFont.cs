using PdfLexer.DOM;
using PdfLexer.Filters;
using PdfLexer.Fonts.Files;

namespace PdfLexer.Fonts;

internal class TrueTypeWritableFont : IWritableFont
{
    private readonly TrueTypePdfFontInfo _info;
    private readonly PdfStream _fontFile;
    private readonly Dictionary<char, Glyph> _glyphs;
    private readonly Glyph[] _fastLookup;
    private readonly int _fastStart;
    private readonly int? _fastEnd;

    public double LineHeight => throw new NotImplementedException();

    public TrueTypeWritableFont(TrueTypePdfFontInfo info, PdfStream fontFile, Glyph[]? fastLookup = null, int fastStart = 0)
    {
        _info = info;
        _fontFile = fontFile;
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
            buffer[0] = (byte)(g.CodePoint ?? 0); // 1 byte only
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

    public PdfDictionary GetPdfFont()
    {
        var f = new FontType1();
        f.NativeObject[PdfName.Subtype] = PdfName.TrueType;
        f.BaseFont = _info.PostScriptName;

        var list = _glyphs.Values.Where(x => x.CodePoint.HasValue).OrderBy(x => x.CodePoint).ToList();
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
        f.Encoding = PdfName.MacRomanEncoding;
        f.FirstChar = min;
        f.LastChar = max;
        f.Widths = widths;



        var fd = new FontDescriptor
        {
            FontName = _info.PostScriptName,
            Flags = FontFlags.Nonsymbolic,
            FontBBox = new PdfRectangle
            {
                LLx = new PdfDoubleNumber(_info.LLx),
                LLy = new PdfDoubleNumber(_info.LLy),
                URx = new PdfDoubleNumber(_info.URx),
                URy = new PdfDoubleNumber(_info.URy),
            },
            ItalicAngle = new PdfDoubleNumber(_info.ItalicAngle),
            CapHeight = _info.CapHeight,
            Ascent = _info.Ascent,
            Descent = _info.Descent,
            StemV = 70,
            FontFile2 = _fontFile
        };
        if (_info.Bold)
        {
            fd.Flags = fd.Flags | FontFlags.ForceBold;
        }
        f.FontDescriptor = fd;
        return f;
    }

    public bool SpaceIsWordSpace() => true;
}
