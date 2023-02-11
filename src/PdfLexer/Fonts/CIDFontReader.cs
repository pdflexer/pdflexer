using PdfLexer.DOM;
using PdfLexer.Fonts.Files;


namespace PdfLexer.Fonts;

internal class Type0Font
{
    public static IReadableFont CreateReadable(ParsingContext ctx, FontType0 t0)
    {
        var enc = t0.Encoding;

        var (encoding, vertical) = GetEncoding(ctx, t0.Encoding);

        // if encodingmap is null we CMapFont assumes identity
        var cidCharSet = t0.DescendantFont?.CIDSystemInfo?.Registry?.Value + "-" + t0.DescendantFont?.CIDSystemInfo?.Ordering?.Value;
        var knownDesc = cidCharSet == "Adobe-GB1" || cidCharSet == "Adobe-CNS1" || cidCharSet == "Adobe-Japan1" || cidCharSet == "Adobe-Korea1";

        var all = new Dictionary<uint, Glyph>();
        var b1g = new Glyph[256];

        AddEmbeddedValues(ctx, t0, all, b1g);

        if (vertical)
        {
            AddHeights(t0, all, b1g);
        }
        AddWidths(t0, all, b1g);

        AddCodePoints(encoding, all);

        AddToUnicodeValues(ctx, t0, encoding, all, b1g);

        CMap? cidInfo = null;
        if (knownDesc)
        {
            // add unicode values from known cmap
            var e2 = ctx.CMapProvider.GetCMapData(cidCharSet + "-UCS2"); // we know not null for knownDesc
            if (e2 != null)
            {
                foreach (var (cid, gg) in all)
                {
                    if (e2.Mapping!.TryGetValue(cid, out var info))
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
                cidInfo = new CMap(e2.Ranges!, e2.Mapping);
            }
        }


        SetDefaultWidths(t0, all);
        if (vertical)
        {
            SetDefaultHeights(t0, all);
        }

        var mw = (t0.DescendantFont?.DW ?? 1000f) / 1000f;
        var bbox = t0.DescendantFont?.FontDescriptor?.FontBBox;
        decimal bbx = 0, bby = 0;
        if (bbox != null)
        {
            bbx = (decimal)bbox.LLx / 1000m;
            bby = (decimal)bbox.LLy / 1000m;
        }
        Glyph notdef;
        if (vertical)
        {
            var (dx, dy) = GetDW2(t0);
            notdef = new Glyph { Char = '\u0000', w0 = mw, w1 = dy, IsWordSpace = false, Undefined = true,
                BBox = new decimal[] { bbx, 0m, bbx + (decimal)dx, (decimal)dy } };
        } else
        {
            notdef = new Glyph { Char = '\u0000', w0 = mw, IsWordSpace = false, Undefined = true,
                BBox = new decimal[] { 0m, bby, (decimal)mw, bby + (bbox?.URy ?? 0) / 1000.0m } };
        }

        var gs = new GlyphSet(b1g, all, notdef);

        return new CompositeFont(t0.BaseFont?.Value ?? "Empty", encoding, gs, 2, vertical, cidInfo);
    }

    [return: NotNullIfNotNull("name")]
    private static Glyph? MapGlyph(uint cid, string? name)
    {
        if (name == null) { return null; }
        var g = new Glyph
        {
            Name = name,
            CID = cid
        };
        if (!GlyphNames.Lookup.TryGetValue(name, out char value))
        {
            value = (char)cid;
            g.GuessedUnicode = true;
        }
        g.Char = value;
        return g;
    }

    private static (CMap encoding, bool isVertical) GetEncoding(ParsingContext ctx, IPdfObject? encoding)
    {
        if (encoding == null)
        {
            return FallbackEncoding();
        }
        encoding = encoding.Resolve();

        if (encoding.Type == PdfObjectType.NameObj)
        {
            var name = encoding.GetAs<PdfName>();
            if (name == null)
            {
                return FallbackEncoding();
            }
            var identityEncoded = (name.Value == "Identity-H" || name.Value == "Identity-V");
            if (!identityEncoded)
            {
                var cmap = ctx.CMapProvider.GetCMapData(name.Value);
                if (cmap != null)
                {
                    return (new CMap(cmap.Ranges, cmap.Mapping), cmap.Vertical);
                }
            } 
            else if (name.Value == "Identity-V")
            {
                var def = FallbackEncoding();
                return (def.encoding, true);
            }
            return FallbackEncoding();
        }
        else if (encoding?.Type == PdfObjectType.StreamObj)
        {
            var stream = encoding.GetAs<PdfStream>();
            using var buffer = stream.Contents.GetDecodedBuffer();
            var (ranges, _, cids, isVert) = CMapReader.ReadCMap(ctx, buffer.GetData(), true);
            if (stream.Dictionary.TryGetValue("UseCMap", out var other))
            {
                var prev = GetEncoding(ctx, other);
                if (ranges.Count == 0)
                {
                    ranges = prev.encoding.Ranges;
                }
                if (prev.encoding.TryGetMapping(out var map))
                {
                    foreach (var item in map)
                    {
                        if (!cids.ContainsKey(item.Key))
                        {
                            cids[item.Key] = item.Value;
                        }
                    }
                }
            }
            return (new CMap(ranges, cids), isVert);
        }
        return FallbackEncoding();
    }

    private static (CMap encoding, bool isVertical) FallbackEncoding()
    {
        var twoByte = new CRange
        {
            Start = 0x0000,
            End = 0xFFFF,
            Bytes = 2
        };
        return (new CMap(new List<CRange> { twoByte }), false);
    }

    private static void AddEmbeddedValues(ParsingContext ctx, FontType0 t0, Dictionary<uint, Glyph> all, Glyph[] b1g)
    {
        var ttf = t0.DescendantFont?.FontDescriptor?.FontFile2;
        ttf ??= t0.DescendantFont?.FontDescriptor?.FontFile3;
        if (ttf == null)
        {
            return;
        }

        var cidtogid = t0.DescendantFont?.ReadCIDToGid();
        Dictionary<uint, uint>? cidLu = null;
        if (cidtogid != null)
        {
            cidLu = new Dictionary<uint, uint>();
            foreach (var (cid, gid) in cidtogid)
            {
                cidLu[gid] = cid;
            }
        }

        using var buff = ttf.Contents.GetDecodedBuffer();
        var data = buff.GetData();
        if (TrueTypeReader.IsTTFile(data) || TrueTypeReader.IsTTCollectionFile(data) || TrueTypeReader.IsOpenTypeFile(data))
        {
            AddFromTrueType(ctx, t0, cidtogid, cidLu, all, b1g, data);
        } else if (CFFReader.IsCFFfile(data))
        {
            try
            {
                var cff = new CFFReader(ctx, data);
                cff.AddCharactersToCid(t0.BaseFont ?? "Empty", cidLu, all, b1g);
            } catch (Exception e)
            {
                ctx.Error($"CFF parsing error for t0 font {t0.BaseFont}: " + e.Message);
            }
            
        } else
        {
            ctx.Error($"Font file for {t0.BaseFont} was not matched as CFF, OpenType, TrueType, or TrueType collection for Type0 font.");
        }
    }

    private static void AddFromTrueType(ParsingContext ctx, FontType0 t0,
        Dictionary<uint, uint>? cidtogid, Dictionary<uint, uint>? cidLu,
        Dictionary<uint, Glyph> all, Glyph?[] b1g, ReadOnlySpan<byte> data)
    {
        try
        {
            var reader = new TrueTypeReader(ctx, data);

            if (reader.TryGetMaxpGlyphs(out int count) && reader.HasPostTable())
            {
                var (_, names) = reader.ReadPostScriptTable(count);
                if (names.Length != 0)
                {
                    for (uint i = 0; i < names.Length; i++)
                    {
                        var gid = i;
                        var name = names[i];
                        if (name == null) { continue; }
                        var cid = gid;
                        if (cidLu != null)
                        {
                            cidLu.TryGetValue(gid, out cid);
                        }
                        var g = MapGlyph(cid, name);
                        all[cid] = g;
                        if (cid < b1g.Length)
                        {
                            b1g[cid] = g;
                        }
                    }
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
                            Char = (char)i,
                            GuessedUnicode = true
                        };

                        uint cid = i;
                        if (cidLu != null)
                        {
                            cidLu.TryGetValue(i, out cid);
                        }
                        g.CID = cid;
                        // pdfium seems to guess unicode sometimes using gid and other times using cid
                        // TODO -> determine correct, this is required for bug1650302_reduced but
                        // may break others
                        g.Char = (char)cid;

                        all[cid] = g;
                        if (cid < b1g.Length)
                        {
                            b1g[cid] = g;
                        }
                    }
                }
            } else if (count > 0 && reader.HasCFFData())
            {
                var cffData = reader.GetCFFData();
                var cffReader = new CFFReader(ctx, cffData);
                cffReader.AddCharactersToCid(t0.BaseFont ?? "Empty", cidLu, all, b1g);
            }

            if (reader.HasCMapTable())
            {
                var maps = reader.ReadCMapTables();
                if (reader.TryGetNameMap(maps, out var gidToUnicode))
                {
                    foreach (var g in all)
                    {
                        if (!g.Value.GuessedUnicode) { continue;  }
                        var cid = g.Key;
                        var gid = cid;
                        if (cidtogid != null)
                        {
                            cidtogid.TryGetValue(cid, out gid);
                        }

                        if (gidToUnicode.TryGetValue(gid, out var name))
                        {
                            var c = GlyphNames.Get(name, 0);
                            if (c != null)
                            {
                                g.Value.Char = c.Value;
                                g.Value.GuessedUnicode = false;
                            }
                        }
                    }
                }
            }

        } catch (Exception e)
        {
            ctx.Error($"Error reading tt font file ({t0.BaseFont}): " + e.Message);
        }
        
    }

    private static void AddToUnicodeValues(ParsingContext ctx, FontType0 t0, CMap encoding, Dictionary<uint, Glyph> all, Glyph[] b1g)
    {
        if (!t0.NativeObject.TryGetValue(PdfName.ToUnicode, out var obj))
        {
            return;
        }
        obj = obj.Resolve();
        switch (obj)
        {
            case PdfName nm:
                if (!nm.Value.StartsWith("Identity"))
                {
                    return;
                }
                foreach (var g in all.Values)
                {
                    if (g.GuessedUnicode && g.CodePoint.HasValue)
                    {
                        g.Char = (char)g.CodePoint.Value;
                        g.GuessedUnicode = false;
                    }
                }
                break;
            case PdfStream stream:
                {
                    using var buffer = stream.Contents.GetDecodedBuffer();
                    var (ranges, glyphs, _, _) = CMapReader.ReadCMap(ctx, buffer.GetData());
                    encoding.TryGetMapping(out var lookup);
                    foreach (var glyph in glyphs.Values)
                    {
                        var cp = glyph.CodePoint ?? 0;
                        if (all.TryGetValue(cp, out var existing))
                        {
                            if (existing.GuessedUnicode)
                            {
                                existing.GuessedUnicode = false;
                                existing.Char = glyph.Char;
                                existing.MultiChar = glyph.MultiChar;
                            }
                            continue;
                        }
                        var cid = cp;
                        if (lookup != null && lookup.TryGetValue(cp, out var cr))
                        {
                            cid = cr.Code;
                        }
                        all[cid] = glyph;
                        if (b1g != null && cid < b1g.Length)
                        {
                            b1g[cid] = glyph;
                        }
                    }
                }
                break;
        }
        
    }

    private static void AddCodePoints(CMap encoding, Dictionary<uint, Glyph> all)
    {
        if (!encoding.TryGetMapping(out var mapping))
        {
            foreach (var g in all.Values)
            {
                g.CodePoint = g.CID;
            }
            return;
        }
        Dictionary<uint,uint> lookup = new Dictionary<uint, uint>();
        foreach (var kvp in mapping)
        {
            lookup[kvp.Value.Code] = kvp.Key; ;
        }

        foreach (var g in all.Values)
        {
            if (g.CID == null) { continue; } // shouldn't hapen
            if (lookup.TryGetValue(g.CID.Value, out var cp))
            {
                g.CodePoint = cp;
                
            } else {
                g.CodePoint = g.CID;
            }
        }
    }

    internal static void AddWidths(FontType0 t0, Dictionary<uint, Glyph> glyphs, Glyph[]? b1g=null)
    {
        var widths = t0.DescendantFont?.W;
        if (widths == null) { return; }
        foreach (var (cid, w) in ReadWidths(widths))
        {
            var rw = w / 1000f;
            if (rw == 0f) // hack for tracking undefined vs set 0 widths... need to clean up at some point
            {
                rw = -9999f;
            }
            if (glyphs.TryGetValue(cid, out var glyph))
            {
                glyph.w0 = rw;
            }
            else
            {
                var g = new Glyph
                {
                    Char = (char)cid,
                    CID = cid,
                    w0 = rw,
                    IsWordSpace = false,
                    GuessedUnicode = true,
                };
                glyphs[cid] = g;
                if (b1g != null && cid < b1g.Length)
                {
                    b1g[cid] = g;
                }
            }
        }
    }

    internal static void AddHeights(FontType0 t0, Dictionary<uint, Glyph> glyphs, Glyph[]? b1g = null)
    {
        var widths = t0.DescendantFont?.W2;
        if (widths == null) { return; }
        foreach (var (cid, dx, dy, w) in ReadHeights(widths))
        {
            var rw = w / 1000f;
            if (rw == 0f) // hack for tracking undefined vs set 0 widths... need to clean up at some point
            {
                rw = -9999f;
            }
            if (glyphs.TryGetValue(cid, out var glyph))
            {
                glyph.w1 = rw;
            }
            else
            {
                var g = new Glyph
                {
                    Char = (char)cid,
                    CID = cid,
                    w1 = rw,
                    IsWordSpace = false,
                    GuessedUnicode = true
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
        decimal bbx = 0, bby = 0;
        if (bbox != null)
        {
            bbx = (decimal)bbox.LLx / 1000m;
            bby = (decimal)bbox.LLy / 1000m;
        }
        var mw = (t0.DescendantFont?.DW ?? 1000f) / 1000f;
        foreach (var glyph in glyphs.Values)
        {
            if (glyph == null) { continue; }
            if (glyph.w0 == 0)
            {
                glyph.w0 = mw;
            } else if (glyph.w0 == -9999f) // hack for tracking undefined vs set 0 widths... need to clean up at some point
            {
                glyph.w0 = 0;
            }
            glyph.BBox = new decimal[] { 0, bby, 0 + (decimal)glyph.w0, bby + (bbox?.URy ?? 0) / 1000.0m };
        }
    }

    internal static (float dx, float dy) GetDW2(FontType0 t0)
    {
        float dx = .88f;
        float dy = -1f;
        if (t0.DescendantFont?.DW2 != null)
        {
            var arr = t0.DescendantFont.DW2.GetAs<PdfArray>();
            if (arr != null)
            {
                if (arr.Count > 0)
                {
                    var dxt = arr[0].GetAs<PdfNumber>();
                    if (dxt != null) { dx = dxt; dx /= 1000f; }
                }
                if (arr.Count > 1)
                {
                    var dyt = arr[1].GetAs<PdfNumber>();
                    if (dyt != null) { dy = dyt; dy /= 1000f; }
                }
            }
        }
        return (dx, dy);
    }

    internal static void SetDefaultHeights(FontType0 t0, Dictionary<uint, Glyph> glyphs)
    {
        var bbox = t0.DescendantFont?.FontDescriptor?.FontBBox;
        decimal bbx = 0, bby = 0;
        if (bbox != null)
        {
            bbx = (decimal)bbox.LLx/1000m;
            bby = (decimal)bbox.LLy/1000m;
        }
        var (dx, dy) = GetDW2(t0);
        foreach (var glyph in glyphs.Values)
        {
            if (glyph == null) { continue; }
            if (glyph.w1 == 0)
            {
                glyph.w1 = dy;
            }
            else if (glyph.w0 == -9999f) // hack for tracking undefined vs set 0 widths... need to clean up at some point
            {
                glyph.w1 = dy;
            }
            glyph.BBox = new decimal[] { bbx, 0, bbx + (decimal)dx, 0 + (decimal)glyph.w1 };
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
                        firstCode = (ushort)(firstCode.Value + 1);
                    }
                    firstCode = null;
                    lastCode = null;
                    continue;
            }
        }
    }

    public static IEnumerable<(ushort cid, float dx, float dy, float w1)> ReadHeights(PdfArray array)
    {
        if (array == null) { yield break; }

        for (var i=0;i<array.Count;i++)
        {
            var val = array[i].GetAs<PdfNumber>();
            if (val == null)
            {
                continue;
            }
            i += 1;
            var val2 = array[i].Resolve();
            if (i >= array.Count) { yield break; }
            switch (val2)
            {
                case PdfNumber cnt:
                    {
                        i++;
                        ushort firstCode = (ushort)val;
                        ushort lastCode = (ushort)cnt;
                        var w1 = i < array.Count ? (float)array[i++].GetAs<PdfNumber>() : 0f;
                        var dx = i < array.Count ? (float)array[i++].GetAs<PdfNumber>() : 0f;
                        var dy = i < array.Count ? (float)array[i].GetAs<PdfNumber>() : 0f;
                        for (var c = firstCode; c <= lastCode; c++)
                        {
                            yield return (c, dx, dy, w1);
                        }
                    }
                    continue;
                case PdfArray arr:
                    ushort cp = (ushort)val;
                    for (var c=0;c<arr.Count;c++)
                    {
                        var w1 = c < arr.Count ? (float)arr[c++].GetAs<PdfNumber>() : 0f;
                        var dx = c < arr.Count ? (float)arr[c++].GetAs<PdfNumber>() : 0f;
                        var dy = c < arr.Count ? (float)arr[c].GetAs<PdfNumber>() : 0f;
                        yield return (cp, dx, dy, w1);
                        cp++;
                    }
                    continue;
            }
        }
    }
}
