using PdfLexer.Fonts;
using PdfLexer.Serializers;
using System.Numerics;
using PdfLexer.Content;
using System.Buffers;

namespace PdfLexer.Writing;

public partial class ContentWriter<T> where T : struct, IFloatingPoint<T>
{
    public ContentWriter<T> Font(IWritableFont font, T size)
    {
        var nm = AddFont(font);
        writableFont = font;
        Tf_Op<T>.WriteLn(nm, size, Writer.Stream);
        GfxState = GfxState with { FontSize = size }; // font handled already
        return this;
    }

    private Dictionary<Base14, IWritableFont> used = new();

    public ContentWriter<T> Font(Base14 font, T size, bool setTextLeading=true)
    {
        if (!used.TryGetValue(font, out var wf))
        {
            wf = font switch
            {
                Base14.TimesRoman => Standard14Font.GetTimesRoman(),
                Base14.TimesRomanBold => Standard14Font.GetTimesRomanBold(),
                Base14.TimesRomanBoldItalic => Standard14Font.GetTimesRomanBoldItalic(),
                Base14.TimesRomanItalic => Standard14Font.GetTimesRomanItalic(),
                Base14.Courier => Standard14Font.GetCourier(),
                Base14.CourierBold => Standard14Font.GetCourierBold(),
                Base14.CourierBoldItalic => Standard14Font.GetCourierBoldItalic(),
                Base14.CourierItalic => Standard14Font.GetCourierItalic(),
                Base14.Helvetica => Standard14Font.GetHelvetica(),
                Base14.HelveticaBold => Standard14Font.GetHelveticaBold(),
                Base14.HelveticaBoldItalic => Standard14Font.GetHelveticaBoldItalic(),
                Base14.HelveticaItalic => Standard14Font.GetHelveticaItalic(),
                Base14.Symbol => Standard14Font.GetSymbol(),
                Base14.ZapfDingbats => Standard14Font.GetZapfDingbats(),
                _ => throw new PdfLexerException("Base14 not implemented: " + font.ToString())
            };
used[font] = wf;
        }
        Font(wf, size);
        if (setTextLeading)
        {
            TextLeading(size);
        }
        return this;
    }

    public PdfPoint<T> GetCurrentTextPos()
    {
        return new PdfPoint<T> { X = GfxState.Text.TextMatrix.E, Y = GfxState.Text.TextMatrix.F };
    }

    public ContentWriter<T> WordSpacing(T w)
    {
        Tw_Op<T>.WriteLn(w, Writer.Stream);
        GfxState = GfxState with { WordSpacing = w };
        return this;
    }

    public ContentWriter<T> NewLine()
    {
        T_Star_Op<T>.WriteLn(Writer.Stream);
        T_Star_Op<T>.Value.Apply(ref GfxState);
        return this;
    }

    public ContentWriter<T> TextLeading(T spacing) => LineSpacing(spacing);
    public ContentWriter<T> LineSpacing(T spacing)
    {
        TL_Op<T>.WriteLn(spacing, Writer.Stream);
        new TL_Op<T>(spacing).Apply(ref GfxState);
        return this;
    }

    public ContentWriter<T> Text(string text)
    {
        if (writableFont == null) { throw new NotSupportedException("Must set current font before writing."); }
        // very inefficient just experimenting
        var b = new byte[2];
        Span<byte> buf = new byte[text.Length];
        int os = 0;
        double total = 0;
        double fs = FPC<T>.Util.ToDouble(GfxState.FontSize);
        var ws = FPC<T>.Util.ToDouble(GfxState.WordSpacing);

        Writer.Stream.WriteByte((byte)'[');
        foreach (var c in writableFont.ConvertFromUnicode(text, 0, text.Length, b))
        {
            if (c.PrevKern != 0)
            {
                StringSerializer.WriteToStream(buf.Slice(0, os), Writer.Stream);
                PdfOperator.Writedecimal((decimal)c.PrevKern, Writer.Stream);
                os = 0;
                total -= c.PrevKern/1000;
            }
            buf[os++] = b[0];
            total += c.Width;
            if (c.ByteCount == 1 && b[0] == 32)
            {
                total += ws/ fs;
            }
        }
        StringSerializer.WriteToStream(buf.Slice(0, os), Writer.Stream);
        Writer.Stream.WriteByte((byte)']');
        Writer.Stream.WriteByte((byte)' ');
        Writer.Stream.Write(TJ_Op.OpData);
        Writer.Stream.WriteByte((byte)'\n');
        GfxState.ApplyCharShift(FPC<T>.Util.FromDouble<T>(total), false);
        return this;
    }

    public ContentWriter<T> TextWrap(string text, T width, bool scaled=true)
    {
        if (writableFont == null) { throw new NotSupportedException("Must set current font before writing."); }

        var rented = ArrayPool<byte>.Shared.Rent(text.Length);
        Span<byte> buf = rented;

        if (scaled) 
        {
            GfxState.CTM.Invert(out var iv);
            width = iv.GetTransformedPoint(new PdfPoint<T> { X = width, Y = T.Zero }).X;
        } 

        var b = new byte[2];
        Span<byte> sb = stackalloc byte[1];
        sb[0] = 32;
        T spaceWidth = T.Zero;
        foreach (var c in writableFont.ConvertFromUnicode(" ", 0, 1, b))
        {
            var x = FPC<T>.Util.FromDouble<T>(c.Width);
            var (sx, sy) = GfxState.CalculateCharShift(x);
            spaceWidth = sx == T.Zero ? sy : sx;
        }

        var ms = new MemoryStream();

        T cw = T.Zero;

        double fs = FPC<T>.Util.ToDouble(GfxState.FontSize);
        var ws = FPC<T>.Util.ToDouble(GfxState.WordSpacing);

        var (dx,dy) = GfxState.CalculateCharShift(GfxState.WordSpacing / GfxState.FontSize);
        var extraSpacing = dy == T.Zero ? dx : dy;

        
        var lines = text.Split('\n');
        foreach (var line in lines)
        {
            var blocks = new List<TextBlock<T>>();
            var txt = line.Trim();
            var words = txt.Split(" ");
            foreach (var word in words)
            {
                if (string.IsNullOrWhiteSpace(word)) continue;
                int os = 0;
                double total = 0;

                var w = word.Trim();
                foreach (var c in writableFont.ConvertFromUnicode(w, 0, w.Length, b))
                {
                    if (c.PrevKern != 0)
                    {
                        StringSerializer.WriteToStream(buf.Slice(0, os), ms);
                        PdfOperator.Writedecimal((decimal)c.PrevKern, ms);
                        os = 0;
                        total -= c.PrevKern / 1000;
                    }
                    buf[os++] = b[0];
                    total += c.Width;
                    if (c.ByteCount == 1 && b[0] == 32)
                    {
                        total += ws / fs;
                    }
                }
                StringSerializer.WriteToStream(buf.Slice(0, os), ms);

                var (x, y) = GfxState.CalculateCharShift(FPC<T>.Util.FromDouble<T>(total));
                var wi = new TextBlock<T> { Op = ms.ToArray(), Width = y == T.Zero ? x : y };
                ms.Position = 0;
                ms.SetLength(0);
                if (cw + wi.Width > width)
                {
                    WriteBlocks(blocks, true);
                }
                blocks.Add(wi);
                cw += wi.Width;
                cw += spaceWidth + extraSpacing; // for space
            }
            if (blocks.Count > 0)
            {
                WriteBlocks(blocks, false);
            }

            T_Star_Op.WriteLn(Writer.Stream);
        }

        ArrayPool<byte>.Shared.Return(rented);

        return this;

        void WriteBlocks(List<TextBlock<T>> blocks, bool newLine)
        {
            Span<byte> space = stackalloc byte[1];
            space[0] = 32;
            Writer.Stream.WriteByte((byte)'[');
            for (var i = 0; i < blocks.Count;i++)
            {
                Writer.Stream.Write(blocks[i].Op);
                if (i + 1 < blocks.Count)
                {
                    StringSerializer.WriteToStream(space, Writer.Stream);
                }
            }
            blocks.Clear();
            Writer.Stream.WriteByte((byte)']');
            Writer.Stream.WriteByte((byte)' ');
            Writer.Stream.Write(TJ_Op.OpData);
            Writer.Stream.WriteByte((byte)'\n');
            if (newLine)
            {
                T_Star_Op.WriteLn(Writer.Stream);
            }
            cw = T.Zero;
        }
    }

    public ContentWriter<T> TextMoveTo(PdfPoint<T> point) => TextMoveTo(point.X, point.Y);

    public ContentWriter<T> TextMove(T dx, T dy)
    {
        EnsureInTextState();
        Td_Op<T>.WriteLn(dx, dy, Writer.Stream);
        Td_Op<T>.Apply(ref GfxState, dx, dy);
        return this;
    }

    public ContentWriter<T> LineShift(T dx, T dy)
    {
        EnsureInTextState();
        Td_Op<T>.WriteLn(dx, dy, Writer.Stream);
        Td_Op<T>.Apply(ref GfxState, dx, dy);
        return this;
    }

    public ContentWriter<T> TextMoveTo(T x, T y) => TextTransform(GfxMatrix<T>.Identity with { E = x, F = y });

    public ContentWriter<T> TextTransform(GfxMatrix<T> tm)
    {
        EnsureInTextState();
        Tm_Op<T>.WriteLn(tm, Writer.Stream);
        GfxState.Text.TextLineMatrix = tm;
        GfxState.Text.TextMatrix = tm;
        GfxState.UpdateTRM();
        return this;
    }

    public ContentWriter<T> EndText()
    {
        State = PageState.Page;
        ET_Op.WriteLn(Writer.Stream);
        return this;
    }

    public ContentWriter<T> BeginText()
    {
        State = PageState.Text;
        BT_Op.WriteLn(Writer.Stream);
        GfxState.Text.TextLineMatrix = GfxMatrix<T>.Identity;
        GfxState.Text.TextMatrix = GfxMatrix<T>.Identity;
        GfxState.UpdateTRM();
        return this;
    }

    internal ContentWriter<T> SetTextAndLinePosition(GfxMatrix<T> lm)
    {
        EnsureInTextState();
        if (GS.Text.TextLineMatrix != lm)
        {
            var cur = GS.Text.TextLineMatrix;

            cur.Invert(out var iv);
            var xform = lm * iv;

            if (cur.A == lm.A && cur.B == lm.B && cur.C == lm.C && cur.D == lm.D)
            {
                // E = (C * F - E * D) * invDet,
                // F = (E * B - A * F) * invDet
                if (xform.E == T.Zero && T.Round(xform.F + GS.TextLeading, 6) == T.Zero)
                {
                    Op(T_Star_Op<T>.Value);
                }
                else
                {
                    TextMove(T.Round(xform.E, 6), T.Round(xform.F, 6));
                }
            } else
            {
                TextTransform(lm);
            }

        } else if (GS.Text.TextMatrix != lm)
        {
            TextTransform(lm);
        }
        return this;
    }

    internal void WriteGlyphs(List<GlyphOrShift<T>> glyphs)
    {
        EnsureInTextState();
        Span<byte> buffer = stackalloc byte[4];
        Span<byte> output = stackalloc byte[16];
        var str = Writer.Stream;

        if (glyphs.Any(x => x.Shift != T.Zero))
        {
            bool inString = false;
            str.WriteByte((byte)'[');
            foreach (var item in glyphs)
            {
                if (item.Shift != T.Zero)
                {
                    if (inString)
                    {
                        str.WriteByte((byte)')');
                        str.WriteByte((byte)' ');
                        inString = false;
                    }
                    FPC<T>.Util.Write(item.Shift, str);
                    GfxState.ApplyShift(item);
                }
                else
                {
                    if (!inString)
                    {
                        str.WriteByte((byte)' ');
                        str.WriteByte((byte)'(');
                        inString = true;
                    }
                    WriteGlyph(item, buffer, output, str);
                    GfxState.ApplyCharShift(item);
                }
            }
            if (inString)
            {
                str.WriteByte((byte)')');
            }
            str.WriteByte((byte)']');
            str.WriteByte((byte)' ');
            str.Write(TJ_Op.OpData);
            str.WriteByte((byte)'\n');
        }
        else
        {
            str.WriteByte((byte)'(');
            foreach (var item in glyphs)
            {
                WriteGlyph(item, buffer, output, str);
                GfxState.ApplyCharShift(item);
            }
            str.WriteByte((byte)')');
            str.WriteByte((byte)' ');
            str.Write(Tj_Op.OpData);
            str.WriteByte((byte)'\n');
        }

        static void WriteGlyph(GlyphOrShift<T> item, Span<byte> buffer, Span<byte> output, Stream str)
        {
            var cp = item.Glyph?.CodePoint ?? 0;
            buffer[0] = (byte)((cp >> 24) & 0xFF);
            buffer[1] = (byte)((cp >> 16) & 0xFF);
            buffer[2] = (byte)((cp >> 8) & 0xFF);
            buffer[3] = (byte)(cp & 0xFF);

            buffer = buffer.Slice(4 - item.Bytes, item.Bytes);
            var i = StringSerializer.ConvertLiteralBytesWithoutParenths(buffer, output, false);
            str.Write(output.Slice(0, i));
        }
    }

    private void EnsureInTextState()
    {
        if (State == PageState.Text) { return; }
        if (State == PageState.Page)
        {
            BeginText();
            return;
        }
    }
    private IWritableFont? writableFont;
    private Dictionary<IWritableFont, PdfName> fonts = new Dictionary<IWritableFont, PdfName>();
    private Dictionary<PdfDictionary, PdfName> fontObjs = new Dictionary<PdfDictionary, PdfName>();

    private PdfName AddFont(IWritableFont obj)
    {
        if (fonts.TryGetValue(obj, out var existing))
        {
            return existing;
        }

        var fnt = obj.GetPdfFont();
        var nm = AddFont(fnt);
        fonts[obj] = nm;
        return nm;
    }

    private PdfName AddFont(PdfDictionary obj)
    {
        GfxState = GfxState with { FontObject = obj };
        if (fontObjs.TryGetValue(obj, out var existing))
        {
            return existing;
        }

        PdfName name = $"F{objCnt++}";
        while (Fonts.ContainsKey(name))
        {
            name = $"F{objCnt++}";
        }

        fontObjs[obj] = name;
        Fonts[name] = PdfIndirectRef.Create(obj);
        return name;
    }
}

public enum Base14
{
    TimesRoman,
    TimesRomanBold,
    TimesRomanBoldItalic,
    TimesRomanItalic,
    Courier,
    CourierBold,
    CourierBoldItalic,
    CourierItalic,
    Helvetica,
    HelveticaBold,
    HelveticaBoldItalic,
    HelveticaItalic,
    Symbol,
    ZapfDingbats,
}