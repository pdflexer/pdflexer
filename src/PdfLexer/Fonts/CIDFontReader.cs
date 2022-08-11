using PdfLexer.CMaps;
using PdfLexer.DOM;
using PdfLexer.Fonts.Files;
using PdfLexer.Parsers;
using System;
using System.Collections.Generic;
using System.IO;

namespace PdfLexer.Fonts
{
    internal class CIDFontReader
    {

        public static IReadableFont Create(ParsingContext ctx, FontType0 t0)
        {
            
            var enc = t0.Encoding;
            PdfName encodingName = null;
            CMap encodingMap = null;
            if (enc.Type == PdfObjectType.NameObj)
            {
                encodingName = enc.GetAs<PdfName>();
            } else if (enc.Type == PdfObjectType.StreamObj)
            {
                var encodingStream = enc.GetAs<PdfStream>();
                using var buffer = encodingStream.Contents.GetDecodedBuffer();
                var (ranges, _) = CMapReader.ReadCMap(ctx, buffer.GetData(), true);
                encodingMap = new CMap(ranges);
            }

            var identityEncoded = encodingName != null && (encodingName.Value == "/Identity-H" || encodingName.Value == "Identity-V");
            if (!identityEncoded && encodingName != null)
            {
                var e1 = KnownCMaps.GetCMap(encodingName.Value.Substring(1));
                encodingMap = new CMap(e1.Ranges);
            }

            var cidCharSet = t0.DescendantFont?.CIDSystemInfo?.Registry?.Value + "-" + t0.DescendantFont?.CIDSystemInfo?.Ordering?.Value;
            var knownDesc = cidCharSet == "Adobe-BG1" || cidCharSet == "Adobe-CNS1" || cidCharSet == "Adobe-Japan1" || cidCharSet == "Adobe-Korea1";

            var all = new Dictionary<uint, Glyph>();
            var b1g = new Glyph[256];

            AddEmbeddedValues(ctx, t0, all, b1g);

            // todo vertical
            AddWidths(t0, all, b1g);

            AddToUnicodeValues(ctx, t0, all, b1g);


            CMap cmap;
            if (knownDesc)
            {
                // add unicode values from known cmap
                var e2 = KnownCMaps.GetCMap(cidCharSet + "-UCS2");
                foreach (var (cid, gg) in all)
                {
                    if (e2.Mapping.TryGetValue(cid, out var info))
                    {
                        gg.GuessedUnicode = false;
                        if (info.MultiChar != null)
                        {
                            gg.MultiChar = info.MultiChar;
                            gg.Char = default;
                        }
                        else
                        {
                            gg.Char = (char)info.Code;
                        }
                    }
                }
                // if (all.Count == 0)
                // {
                //     // TODO remove this once we can parse nontrue type embedded files ?
                //     // maybe leave as fallback? huge perf hit
                //     foreach (var (cid,gg) in e2.Mapping)
                //     {
                //         var g = new Glyph
                //         {
                //             Char = (char)gg.Code,
                //             MultiChar = gg.MultiChar,
                //             CodePoint = cid,
                //             IsWordSpace = false,
                //         };
                //         all[cid] = g;
                //         if (b1g != null && cid < b1g.Length)
                //         {
                //             b1g[cid] = g;
                //         }
                //     }
                // }
                cmap = new CMap(e2.Ranges);
            } else
            {
                // this matches the identity ranges
                // need to dig into spec more to see as
                // we are only using charset info for the UCS2 ones
                var twoByte = new CRange
                {
                    Start = 0x0000,
                    End = 0xFFFF,
                    Bytes = 2
                };

                cmap = new CMap(new List<CRange> { twoByte });
            }

            SetDefaultWidths(t0, all);

            var mw = (t0.DescendantFont.DW ?? 1000f) / 1000f;
            var notdef = new Glyph { Char = '\u0000', w0 = mw, IsWordSpace = false, BBox = new decimal[] { 0m, 0m, (decimal)mw, 0m } };

            var gs = new FontGlyphSet(b1g, all, notdef);

            return new CMapFont(cmap, gs, 2, encodingMap);
        }

        private static Glyph MapGlyph(uint cc, string name)
        {
            if (name == null) { return null; }
            var g = new Glyph
            {
                Name = name,
                CodePoint = cc
            };
            if (!GlyphNames.Lookup.TryGetValue(name, out char value))
            {
                value = (char)cc;
                g.GuessedUnicode = true;
            }
            g.Char = value;
            return g;
        }

        private static void AddEmbeddedValues(ParsingContext ctx, FontType0 t0, Dictionary<uint, Glyph> all, Glyph[] b1g)
        {
            var ttf = t0.DescendantFont?.FontDescriptor?.FontFile2;
            ttf ??= t0.DescendantFont?.FontDescriptor?.FontFile3;
            if (ttf == null)
            {
                return;
            }

            using var buff = ttf.Contents.GetDecodedBuffer();
            var data = buff.GetData();
            if (TrueTypeReader.IsTTFile(data) || TrueTypeReader.IsTTCollectionFile(data) || TrueTypeReader.IsOpenTypeFile(data))
            {
                AddFromTrueType(ctx, t0, all, b1g, data);
            } else if (CFFReader.IsCFFfile(data))
            {
                var cff = new CFFReader(ctx, data);
                cff.AddCharactersToCid(t0.BaseFont, all, b1g);
            } else
            {
                ctx.Error($"Font file for {t0.BaseFont} was not matched as CFF, OpenType, TrueType, or TrueType collection for Type0 font.");
            }
        }

        private static void AddFromTrueType(ParsingContext ctx, FontType0 t0, Dictionary<uint, Glyph> all, Glyph[] b1g, ReadOnlySpan<byte> data)
        {
            try
            {
                var cidtogid = t0.DescendantFont?.ReadCIDToGid();
                Dictionary<uint, uint> cidLu = null;
                if (cidtogid != null)
                {
                    cidLu = new Dictionary<uint, uint>();
                    foreach (var (cid, gid) in cidtogid)
                    {
                        cidLu[gid] = cid;
                    }
                }

                var reader = new TrueTypeReader(ctx, data);

                if (reader.TryGetMaxpGlyphs(out int count) && reader.HasPostTable())
                {
                    var (_, names) = reader.ReadPostScriptTable(count);
                    if (names.Length != 0)
                    {
                        if (cidLu != null)
                        {
                            for (uint i = 0; i < names.Length; i++)
                            {
                                var gid = i;
                                var name = names[i];
                                if (name == null) { continue; }
                                if (cidLu.TryGetValue(gid, out var cid))
                                {
                                    var g = MapGlyph(cid, name);
                                    all[cid] = g;
                                    if (cid < b1g.Length)
                                    {
                                        b1g[cid] = g;
                                    }
                                }
                            }
                        }

                        // fallback for unmapped cidtogid && those without cidtogid
                        for (uint i = 0; i < names.Length; i++)
                        {
                            var gid = i;
                            var name = names[i];
                            var cid = gid;
                            if (cidLu == null || !all.ContainsKey(cid))
                            {
                                var g = MapGlyph(cid, name);
                                all[cid] = g;
                                if (cid < b1g.Length)
                                {
                                    b1g[cid] = g;
                                }
                            }
                        }
                        return;
                    }
                }
                

                if (count > 0 && reader.HasGlyfInfo())
                {
                    var glyphs = reader.ReadGlyfInfo(count);
                    for (uint i = 0; i < glyphs.Length; i++)
                    {
                        if (glyphs[i])
                        {
                            var g = new Glyph
                            {
                                CodePoint = i,
                                Char = (char)i,
                                GuessedUnicode = true
                            };

                            if (cidLu != null && cidLu.TryGetValue(i, out var cid))
                            {
                                g.CodePoint = cid;
                                // use original char for unicode those GID
                                // consistent with pdfium, may not have a purpose
                                // but makes testing consistent
                                // g.Char = (char)cid;
                            }

                            all[i] = g;
                            if (i < b1g.Length)
                            {
                                b1g[i] = g;
                            }
                        }
                    }
                } else if (count > 0 && reader.HasCFFData())
                {
                    var cffData = reader.GetCFFData();
                    var cffReader = new CFFReader(ctx, cffData);
                    cffReader.AddCharactersToCid(t0.BaseFont, all, b1g);
                }

            } catch (Exception e)
            {
                ctx.Error($"Error reading tt font file ({t0.BaseFont}): " + e.Message);
            }
            
        }

        private static void AddToUnicodeValues(ParsingContext ctx, FontType0 t0, Dictionary<uint, Glyph> all, Glyph[] b1g)
        {
            if (t0.ToUnicode == null)
            {
                return;
            }
            var str = t0.ToUnicode;
            using var buffer = str.Contents.GetDecodedBuffer();
            var (ranges, glyphs) = CMapReader.ReadCMap(ctx, buffer.GetData());
            foreach (var glyph in glyphs)
            {
                var cid = glyph.CodePoint.Value;
                if (all.TryGetValue(cid, out var existing))
                {
                    if (existing.GuessedUnicode)
                    {
                        existing.GuessedUnicode = false;
                        existing.Char = glyph.Char;
                        existing.MultiChar = glyph.MultiChar;
                    }
                    continue;
                }
                all[cid] = glyph;
                if (b1g != null && cid < b1g.Length)
                {
                    b1g[cid] = glyph;
                }
            }
        }

        internal static void AddWidths(FontType0 t0, Dictionary<uint, Glyph> glyphs, Glyph[] b1g=null)
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
                    var g = new Glyph
                    {
                        Char = (char)cid,
                        CodePoint = cid,
                        w0 = w / 1000f,
                        IsWordSpace = false,
                        GuessedUnicode = true,
                        BBox = new decimal[] { 0m, 0m, (decimal)(w / 1000f), bbox?.URy / 1000.0m }
                    };
                    glyphs[cid] = g;
                    if (b1g != null && cid < b1g.Length)
                    {
                        b1g[cid] = g;
                    }
                }
            }
        }

        internal static void SetDefaultWidths(FontType0 t0, Dictionary<uint, Glyph> glyphs)
        {
            var bbox = t0.DescendantFont?.FontDescriptor?.FontBBox;
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
