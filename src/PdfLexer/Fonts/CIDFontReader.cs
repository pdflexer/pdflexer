using PdfLexer.CMaps;
using PdfLexer.DOM;
using PdfLexer.Parsers;
using System;
using System.Collections.Generic;

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
                var identityEncoded = en.Value == "/Identity-H" || en.Value == "Identity-V";
                var knownDesc = cidCharSet == "Adobe-BG1" || cidCharSet == "Adobe-CNS1" || cidCharSet == "Adobe-Japan1" || cidCharSet == "Adobe-Korea1";
                if (identityEncoded && !knownDesc)
                {
                    var ttf = t0.DescendantFont?.FontDescriptor?.FontFile2;
                    if (ttf != null)
                    {

                    }

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
                    var twoByte = new CRange
                    {
                        Start = 0x00,
                        End = 0xFF,
                        Bytes = 2
                    };

                    var cmap = new CMap(new List<CRange> { twoByte });
                    var gs = new FontGlyphSet(all, notdef);

                    return new CMapFont(cmap, gs);
                }


                if (!identityEncoded || knownDesc)
                {
                    // todo, lookup first cmap
                    CMap gid = null;
                    if (!identityEncoded)
                    {
                        var e1 = KnownCMaps.GetCMap(en.Value.Substring(1));
                        gid = new CMap(e1.Ranges);
                    }
                    var e2 = KnownCMaps.GetCMap(cidCharSet + "-UCS2");

                    var lu = new Dictionary<uint, Glyph>();
                    foreach (var (k,v) in e2.Mapping)
                    {
                        var g = new Glyph
                        {
                            CodePoint = k,
                            // TODO
                        };
                        if (v.MultiChar != null)
                        {
                            g.MultiChar = v.MultiChar;
                        } else
                        {
                            g.Char = (char)v.Code;
                        }

                        lu[k] = g;
                    }

                    AddWidths(t0, e2.Ranges, lu);

                    var mw = (t0.DescendantFont.DW ?? 1000f) / 1000f;
                    var notdef = new Glyph { Char = '\u0000', w0 = mw, IsWordSpace = false, BBox = new decimal[] { 0m, 0m, (decimal)mw, 0m } };

                    var cmap = new CMap(e2.Ranges);
                    var gs = new FontGlyphSet(lu.Values, notdef);

                    return new CMapFont(cmap, gs, gid);
                }

            }

            return null;
        }

        internal static void AddWidths(FontType0 t0, List<CRange> ranges, Dictionary<uint, Glyph> glyphs)
        {
            var bbox = t0.DescendantFont?.FontDescriptor?.FontBBox;
            foreach (var (cid, w) in ReadWidths(t0.DescendantFont.W))
            {
                if (glyphs.TryGetValue(cid, out Glyph glyph))
                {
                    glyph.w0 = w / 1000f;
                }
                else
                {
                    glyphs[cid] = new Glyph
                    {
                        Char = (char)cid,
                        CodePoint = cid,
                        w0 = w / 1000f,
                        Bytes = CRange.EstimateByteSize(ranges, cid),
                        IsWordSpace = false,
                        GuessedUnicode = true,
                        BBox = new decimal[] { 0m, 0m, (decimal)(w / 1000f), bbox?.URy / 1000.0m }
                    };
                }
            }
            
            var mw = (t0.DescendantFont.DW ?? 1000f) / 1000f;
            foreach (var glyph in glyphs.Values)
            {
                if (glyph == null) { continue; }
                if (glyph.w0 == 0)
                {
                    glyph.w0 = mw;
                }
                glyph.BBox = new decimal[] { 0m, 0m, (decimal)glyph.w0, bbox?.URy / 1000.0m };
            }
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
