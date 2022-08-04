using PdfLexer.DOM;
using PdfLexer.Parsers;
using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer.Fonts
{
    internal class ReadableCIDFont : IReadableFont
    {
        public bool IsVertical => throw new NotImplementedException();

        public int GetGlyph(ReadOnlySpan<byte> data, int os, out Glyph glyph)
        {
            throw new NotImplementedException();
        }
    }
    internal class CIDFontReader
    {
        public static IReadableFont Create(ParsingContext ctx, FontType0 t0)
        {
            if (t0.ToUnicode != null)
            {
                return ToUnicodeFont.GetComposite(ctx, t0);
            }
            var enc = t0.Encoding;
            var cidCharSet = t0.DescendantFont?.CIDSystemInfo?.Registry?.Value + "-" + t0.DescendantFont?.CIDSystemInfo?.Ordering?.Value;
            if (enc != null && enc.Type == PdfObjectType.NameObj)
            {
                var en = enc.GetAs<PdfName>();
                var badEncoding = en.Value == "/Identity-H" || en.Value == "Identity-V";
                var knownDesc = cidCharSet == "Adobe-BG1" || cidCharSet == "Adobe-CNS1" || cidCharSet == "Adobe-Japan1" || cidCharSet == "Adobe-Korea1";
                if (badEncoding && !knownDesc)
                {
                    var all = new List<Glyph>();
                    // TODO warn
                    var g = new Glyph[256];
                    // var g = Encodings.GetPartialGlyphs(Encodings.StandardEncoding);
                    // todo vertical
                    foreach (var (cid, w) in ReadWidths(t0.DescendantFont.W))
                    {
                        if (cid < g.Length)
                        {
                            var tg = g[cid];
                            if (tg != null)
                            {
                                g[cid].w0 = w / 1000f;
                                all.Add(tg);
                                continue;
                            }
                        }

                        all.Add(new Glyph
                        {
                            Char = (char)cid,
                            CodePoint = cid,
                            w0 = w / 1000f,
                            IsWordSpace = false,
                            BBox = new decimal[] { 0m, 0m, (decimal)(w / 1000f), 0m
                        }
                        });
                    }
                    var bbox = t0.DescendantFont?.FontDescriptor?.FontBBox;
                    var mw = (t0.DescendantFont.DW ?? 1000f) / 1000f;
                    foreach (var glyph in g)
                    {
                        if (glyph == null) { continue; }
                        if (glyph.w0 == 0)
                        {
                            glyph.w0 = mw;
                        }
                        glyph.BBox = new decimal[] { 0m, 0m, (decimal)glyph.w0, bbox?.URy / 1000.0m };
                    }
                    var notdef = new Glyph { Char = '\u0000', w0 = mw, IsWordSpace = false, BBox = new decimal[] { 0m, 0m, (decimal)mw, 0m } };
                    var twoByte = new CMapRange
                    {
                        Start = 0x00,
                        End = 0xFF,
                        Glyphs = all,
                        Bytes = 2
                    };
                    return new CMapFont2(new List<CMapRange> { twoByte }, notdef);
                }


            }

            return null;
        }

        public static IEnumerable<(ushort cid, float width)> ReadWidths(PdfArray array)
        {
            if (array == null) { yield break; }
            ushort? firstCode = null;
            ushort? lastCode = null;
            foreach (var rv in array)
            {
                var val = rv.Resolve();
                switch (val)
                {
                    case PdfNumber cnt:
                        if (firstCode == null)
                        {
                            firstCode = (ushort)cnt;
                            continue;
                        }
                        if (lastCode == null)
                        {
                            lastCode = (ushort)cnt;
                            continue;
                        }
                        for (var i = firstCode.Value; i <= lastCode.Value; i++)
                        {
                            yield return ((ushort)(i), (float)cnt);
                        }
                        firstCode = null;
                        lastCode = null;
                        continue;
                    case PdfArray arr:
                        firstCode ??= 0;
                        foreach (var item in arr)
                        {
                            if (item is PdfNumber w)
                            {
                                yield return (firstCode.Value, (float)w);
                            }
                            firstCode += 1;
                        }
                        firstCode = null;
                        lastCode = null;
                        continue;
                }
            }
        }
    }
}
