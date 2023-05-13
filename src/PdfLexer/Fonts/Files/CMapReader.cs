using PdfLexer.Lexing;
using System.Buffers.Text;
using System.Text;

namespace PdfLexer.Fonts.Files;

internal class CMapReader
{
    internal enum ToUnicodeState
    {
        None,
        ReadChars,
        ReadCids,
        ReadCidRange,
        ReadRange,
        ReadSpace
    }

    private static byte[] spaceStart = Encoding.ASCII.GetBytes("begincodespacerange");
    private static byte[] spaceEnd = Encoding.ASCII.GetBytes("endcodespacerange");
    private static byte[] charStart = Encoding.ASCII.GetBytes("beginbfchar");
    private static byte[] charEnd = Encoding.ASCII.GetBytes("endbfchar");
    private static byte[] cidStart = Encoding.ASCII.GetBytes("begincidchar");
    private static byte[] cidEnd = Encoding.ASCII.GetBytes("endcidchar");
    private static byte[] cidRangeStart = Encoding.ASCII.GetBytes("begincidrange");
    private static byte[] cidRangeEnd = Encoding.ASCII.GetBytes("endcidrange");
    private static byte[] rangeStart = Encoding.ASCII.GetBytes("beginbfrange");
    private static byte[] rangeEnd = Encoding.ASCII.GetBytes("endbfrange");
    private static byte[] wmode = Encoding.ASCII.GetBytes("/WMode");
    private static UnicodeEncoding ucBO = new UnicodeEncoding(true, true, false);
    private static UnicodeEncoding uc = new UnicodeEncoding(true, false, false);
    public static (List<CRange> Ranges, Dictionary<uint, Glyph> Glyphs, Dictionary<uint, CResult> Cids, bool isVertical) ReadCMap(ParsingContext ctx, ReadOnlySpan<byte> data, bool skipGlyphs = false)
    {
        var isVertical = false;
        var ranges = new List<CRange>();
        var glyphs = new Dictionary<uint, Glyph>();
        var cids = new Dictionary<uint, CResult>();
        var missing = new List<(int, uint)>();

        ToUnicodeState state = ToUnicodeState.None;
        Span<byte> buffer = stackalloc byte[256];
        var chars = new char[1];
        int bufferUsed = 0;
        var scanner = new Scanner(ctx, data);

        PdfTokenType type = PdfTokenType.Unknown;

        try
        {
            while ((type = scanner.Peek()) != PdfTokenType.EOS)
            {
                if (type == PdfTokenType.Unknown)
                {
                    var token = scanner.GetCurrentData();
                    if (token.SequenceEqual(charStart))
                    {
                        state = skipGlyphs ? ToUnicodeState.None : ToUnicodeState.ReadChars;
                    }
                    else if (token.SequenceEqual(charEnd))
                    {
                        state = ToUnicodeState.None;
                    }
                    else if (token.SequenceEqual(rangeStart))
                    {
                        state = skipGlyphs ? ToUnicodeState.None : ToUnicodeState.ReadRange;
                    }
                    else if (token.SequenceEqual(cidStart))
                    {
                        state = ToUnicodeState.ReadCids;
                    }
                    else if (token.SequenceEqual(cidRangeStart))
                    {
                        state = ToUnicodeState.ReadCidRange;
                    }
                    else if (token.SequenceEqual(rangeEnd))
                    {
                        state = ToUnicodeState.None;
                    }
                    else if (token.SequenceEqual(spaceStart))
                    {
                        state = ToUnicodeState.ReadSpace;
                    }
                    else if (token.SequenceEqual(spaceEnd))
                    {
                        state = ToUnicodeState.None;
                    }
                    else if (token.SequenceEqual(cidEnd))
                    {
                        state = ToUnicodeState.None;
                    }
                    else if (token.SequenceEqual(cidRangeEnd))
                    {
                        state = ToUnicodeState.None;
                    }
                    scanner.SkipCurrent();
                    continue;
                }

                if (state == ToUnicodeState.ReadChars)
                {
                    var g = new Glyph();

                    // c1
                    var token = scanner.GetCurrentData();
                    g.CodePoint = ReadCodePoint(ctx, token, buffer);
                    // g.Bytes = (token.Length - 2) / 2;
                    scanner.SkipCurrent();

                    // vals ->
                    type = scanner.Peek();
                    token = scanner.GetCurrentData();
                    switch (type)
                    {
                        case PdfTokenType.StringStart:

                            AddStringVal(ctx, g, token, buffer);
                            break;
                        case PdfTokenType.NameObj:
                            // todo lookup fallback for postscript names (not technically allowed in tounicode)
                            break;
                    }

                    glyphs[g.CodePoint.Value] = g;
                }
                else if (state == ToUnicodeState.ReadCids)
                {
                    var c = new CResult();

                    // c1
                    var token = scanner.GetCurrentData();
                    var cpBytes = (token.Length - 2) / 2;
                    var cp = ReadCodePoint(ctx, token, buffer);
                    scanner.SkipCurrent();

                    type = scanner.Peek();
                    token = scanner.GetCurrentData();
                    switch (type)
                    {
                        case PdfTokenType.NumericObj:
                            if (Utf8Parser.TryParse(token, out uint value, out int bytes))
                            {
                                c.Code = value;
                            }
                            break;
                    }

                    cids[cp] = c;
                    CheckRange(cpBytes, cp);
                }
                else if (state == ToUnicodeState.ReadCidRange)
                {
                    // c1
                    var token = scanner.GetCurrentData();
                    var bytes = (token.Length - 2) / 2;
                    var cp1 = ReadCodePoint(ctx, token, buffer);
                    scanner.SkipCurrent();
                    // c2
                    type = scanner.Peek();
                    token = scanner.GetCurrentData();
                    var cp2 = ReadCodePoint(ctx, token, buffer);
                    scanner.SkipCurrent();


                    type = scanner.Peek();
                    token = scanner.GetCurrentData();
                    var ct = (int)(cp2 - cp1);
                    switch (type)
                    {
                        case PdfTokenType.NumericObj:
                            if (Utf8Parser.TryParse(token, out uint value, out bufferUsed))
                            {
                                for (var c = 0; c <= ct; c++)
                                {
                                    var cr = new CResult();
                                    var cp = cp1 + (uint)c;
                                    cr.Code = value + (uint)c;
                                    cids[cp] = cr;
                                    CheckRange(bytes, cp);
                                }
                            }
                            break;
                            // case PdfTokenType.ArrayStart:
                            //     for (var c = 0; c <= ct; c++)
                            //     {
                            //         scanner.SkipCurrent();
                            //         type = scanner.Peek();
                            //         token = scanner.GetCurrentData();
                            //         bufferUsed = ctx.StringParser.ConvertBytes(token, buffer);
                            //         var g = new Glyph();
                            //         // g.Bytes = bytes;
                            //         g.CodePoint = cp1 + (uint)c;
                            //         AddStringVal(g, buffer.Slice(0, bufferUsed));
                            //         glyphs[g.CodePoint.Value] = g;
                            //     }
                            //     scanner.SkipCurrent(); // ]
                            //     break;
                    }
                }
                else if (state == ToUnicodeState.ReadRange)
                {
                    // c1
                    var token = scanner.GetCurrentData();
                    var bytes = (token.Length - 2) / 2;
                    var cp1 = ReadCodePoint(ctx, token, buffer);
                    scanner.SkipCurrent();
                    // c2
                    type = scanner.Peek();
                    token = scanner.GetCurrentData();
                    var cp2 = ReadCodePoint(ctx, token, buffer);
                    scanner.SkipCurrent();


                    type = scanner.Peek();
                    token = scanner.GetCurrentData();
                    var ct = (int)(cp2 - cp1);
                    switch (type)
                    {
                        case PdfTokenType.StringStart:
                            bufferUsed = ctx.StringParser.ConvertBytes(token, buffer);

                            for (var c = 0; c <= ct; c++)
                            {
                                var g = new Glyph();
                                g.CodePoint = cp1 + (uint)c;
                                // g.Bytes = bytes;
                                AddStringVal(g, buffer.Slice(0, bufferUsed));
                                glyphs[g.CodePoint.Value] = g;
                                buffer[bufferUsed - 1] = (byte)(buffer[bufferUsed - 1] + 1);
                            }
                            break;
                        case PdfTokenType.ArrayStart:
                            for (var c = 0; c <= ct; c++)
                            {
                                scanner.SkipCurrent();
                                type = scanner.Peek();
                                token = scanner.GetCurrentData();
                                try
                                {
                                    bufferUsed = ctx.StringParser.ConvertBytes(token, buffer);
                                }
                                catch(Exception e)
                                {
                                    ctx.Error("CMap parsing error: " + e.Message);
                                    continue;
                                }
                                var g = new Glyph();
                                // g.Bytes = bytes;
                                g.CodePoint = cp1 + (uint)c;
                                AddStringVal(g, buffer.Slice(0, bufferUsed));
                                glyphs[g.CodePoint.Value] = g;
                            }
                            scanner.SkipCurrent(); // ]
                            break;
                    }
                }
                else if (state == ToUnicodeState.ReadSpace)
                {
                    // c1
                    var token = scanner.GetCurrentData();
                    var bytes = (token.Length - 2) / 2;
                    var cp1 = ReadCodePoint(ctx, token, buffer);
                    scanner.SkipCurrent();
                    // c2
                    type = scanner.Peek();
                    token = scanner.GetCurrentData();
                    var cp2 = ReadCodePoint(ctx, token, buffer);
                    ranges.Add(new CRange
                    {
                        Start = cp1,
                        End = cp2,
                        Bytes = bytes
                    });
                }
                else if (state == ToUnicodeState.None && type == PdfTokenType.NameObj)
                {
                    var token = scanner.GetCurrentData();
                    if (token.SequenceEqual(wmode))
                    {
                        scanner.SkipCurrent();
                        var wm = scanner.Peek();
                        if (wm == PdfTokenType.NumericObj)
                        {
                            var num = scanner.GetCurrentObject().GetAs<PdfNumber>();
                            if (num != null && num == 1)
                            {
                                isVertical = true;
                            }
                        }
                    }
                }
                scanner.SkipCurrent();
            }
        }
        catch (Exception e)
        {
            ctx.Error("CMap parsing error: " + e.Message);
        }


        void CheckRange(int bytes, uint cp)
        {
            foreach (var rng in ranges!)
            {
                if (rng.Start <= cp && rng.End >= cp)
                {
                    return;
                }
            }
            missing!.Add((bytes, cp));
        }

        if (missing.Any())
        {
            foreach (var grp in missing.GroupBy(x=>x.Item1))
            {
                var bytes = grp.Key;
                var sorted = grp.Select(x => x.Item2).OrderBy(x => x).ToList();
                uint start = 0;
                uint last = 0;
                foreach (var cp in sorted)
                {
                    if (start == 0)
                    {
                        last = start = cp;
                        continue;
                    }
                    if (cp != last + 1)
                    {
                        ranges.Add(new CRange
                        {
                            Bytes = bytes,
                            Start = start,
                            End = last
                        });
                        last = start = cp;
                        continue;
                    }

                    last = cp;
                }
                ranges.Add(new CRange
                {
                    Bytes = bytes,
                    Start = start,
                    End = last
                });
            }
        }

        return (ranges, glyphs, cids, isVertical);
    }

    private static uint ReadCodePoint(ParsingContext ctx, ReadOnlySpan<byte> token, Span<byte> buffer)
    {
        int bufferUsed = ctx.StringParser.ConvertBytes(token, buffer);

        uint cp = buffer[0];
        for (var i = 1; i < bufferUsed; i++)
        {
            cp = cp << 8 | buffer[i];
        }
        return cp;
    }

    private static void AddStringVal(ParsingContext ctx, Glyph g, ReadOnlySpan<byte> token, Span<byte> buffer)
    {
        int bufferUsed = ctx.StringParser.ConvertBytes(token, buffer);
        string value;
        if (bufferUsed > 2 && buffer[0] == 0xFE && buffer[1] == 0xFF)
        {
            value = ucBO.GetString(buffer.Slice(0, bufferUsed));
        }
        else
        {
            value = uc.GetString(buffer.Slice(0, bufferUsed));
        }

        if (value.Length == 1)
        {
            g.Char = value[0];
        }
        else
        {
            g.MultiChar = value;
        }
    }
    private static void AddStringVal(ParsingContext ctx, CResult g, ReadOnlySpan<byte> token, Span<byte> buffer)
    {
        int bufferUsed = ctx.StringParser.ConvertBytes(token, buffer);
        string value;
        if (bufferUsed > 2 && buffer[0] == 0xFE && buffer[1] == 0xFF)
        {
            value = ucBO.GetString(buffer.Slice(0, bufferUsed));
        }
        else
        {
            value = uc.GetString(buffer.Slice(0, bufferUsed));
        }

        if (value.Length == 1)
        {
            g.Code = value[0];
        }
        else
        {
            g.MultiChar = value;
        }
    }
    private static void AddStringVal(Glyph g, Span<byte> buffer)
    {
        string value;
        if (buffer.Length > 2 && buffer[0] == 0xFE && buffer[1] == 0xFF)
        {
            value = ucBO.GetString(buffer);
        }
        else
        {
            value = uc.GetString(buffer);
        }

        if (value.Length == 1)
        {
            g.Char = value[0];
        }
        else
        {
            g.MultiChar = value;
        }
    }
}
