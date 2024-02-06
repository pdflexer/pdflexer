using PdfLexer.Fonts;
using PdfLexer.Serializers;
using System.Numerics;
using PdfLexer.Content;
using System.Buffers;
using PdfLexer.Writing.TextLayout;

namespace PdfLexer.Writing;

public partial class ContentWriter<T> where T : struct, IFloatingPoint<T>
{
    public ContentWriter<T> Font(IWritableFont font, T size)
    {
        if (writableFont == font && size == GfxState.FontSize)
        {
            return this;
        }

        EnsureNotPathState();

        var nm = AddFont(font);
        writableFont = font;
        Tf_Op<T>.WriteLn(nm, size, Writer.Stream);
        GfxState = GfxState with { FontSize = size }; // font handled already
        return this;
    }

    private Dictionary<Base14, IWritableFont> used = new();

    public ContentWriter<T> Font(Base14 font, T size, bool setTextLeading = true)
    {
        EnsureNotPathState();
        var wf = GetBase14Font(font);
        Font(wf, size);
        if (setTextLeading)
        {
            TextLeading(size);
        }
        return this;
    }

    internal IWritableFont GetBase14Font(Base14 font)
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
        return wf;
    }

    public PdfPoint<T> GetCurrentTextPos()
    {
        return new PdfPoint<T> { X = GfxState.Text.TextMatrix.E, Y = GfxState.Text.TextMatrix.F };
    }

    public ContentWriter<T> WordSpacing(T w)
    {
        EnsureNotPathState();
        Tw_Op<T>.WriteLn(w, Writer.Stream);
        GfxState = GfxState with { WordSpacing = w };
        return this;
    }

    public ContentWriter<T> NewLine(int count=1)
    {
        EnsureInTextState();
        for (var i = 0; i < count;i++)
        {
            T_Star_Op<T>.WriteLn(Writer.Stream);
            T_Star_Op<T>.Value.Apply(ref GfxState);
        }
        return this;
    }

    public ContentWriter<T> TextLeading(T spacing) => LineSpacing(spacing);
    public ContentWriter<T> LineSpacing(T spacing)
    {
        EnsureNotPathState();
        TL_Op<T>.WriteLn(spacing, Writer.Stream);
        new TL_Op<T>(spacing).Apply(ref GfxState);
        return this;
    }

    public ContentWriter<T> Text(string text)
    {
        if (writableFont == null) { throw new NotSupportedException("Must set current font before writing."); }
        EnsureInTextState();
        // very inefficient just experimenting
        var b = new byte[2];
        byte[] rented = ArrayPool<byte>.Shared.Rent(text.Length*2);
        Span<byte> buf = rented;
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
                total -= c.PrevKern / 1000;
            }
            for (var i=0;i<c.ByteCount;i++)
            {
                buf[os++] = b[i];
            }
            total += c.Width;
            if (c.ByteCount == 1 && b[0] == 32)
            {
                total += ws / fs;
            }

            if (c.AddWordSpace && ws != 0)
            {
                StringSerializer.WriteToStream(buf.Slice(0, os), Writer.Stream);
                PdfOperator.Writedecimal((decimal)(ws / fs), Writer.Stream);
                os = 0;
                total += ws / fs;
            }
        }
        StringSerializer.WriteToStream(buf.Slice(0, os), Writer.Stream);
        Writer.Stream.WriteByte((byte)']');
        Writer.Stream.WriteByte((byte)' ');
        Writer.Stream.Write(TJ_Op.OpData);
        Writer.Stream.WriteByte((byte)'\n');
        GfxState.ApplyCharShift(FPC<T>.Util.FromDouble<T>(total), false);
        ArrayPool<byte>.Shared.Return(rented);
        return this;
    }

    public ContentWriter<T> TextWrap(string text, T width, TextAlign align = TextAlign.Left, bool userSpace = true)
    {
        if (writableFont == null) { throw new NotSupportedException("Must set current font before writing."); }
        if (userSpace)
        {
            GfxState.CTM.Invert(out var iv);
            width = iv.GetTransformedPoint(new PdfPoint<T> { X = width, Y = T.Zero }).X;
        }

        EnsureInTextState();

        var tb = new TextBox<T>(GfxState, writableFont, width);
        tb.Alignment = align;
        tb.AddText(text);
        tb.Apply(this);
        NewLine();
        return this;
    }

    /// <summary>
    /// EXPERIMENTAL, BEHAVIOR MAY CHANGE
    /// </summary>
    /// <param name="area"></param>
    /// <param name="align"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    public ITextBoxWriter<T> TextBox(PdfRect<T> area, TextAlign align = TextAlign.Left)
    {
        if (writableFont == null)
        {
            throw new NotSupportedException("Must set current font before creating text box");
        }
        GfxState.CTM.Invert(out var iv);
        var sized = iv.GetTransformedPoint(new PdfPoint<T> { X = area.Width(), Y = area.Height() });
        var tbw = new TextBoxWriter<T>(this, writableFont, sized.X, sized.Y);
        tbw.Box.Alignment = align;
        tbw.Position = new PdfPoint<T>(area.LLx, area.URy);
        return tbw;
    }

    public ContentWriter<T> TextWrapCenter(string text, T width, bool userSpace = true)
        => TextWrap(text, width, TextAlign.Center, userSpace);


    /// <summary>
    /// Moves the text position to the specified point in 
    /// user space.
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public ContentWriter<T> TextMove(PdfPoint<T> point) => TextMove(point.X, point.Y);


    /// <summary>
    /// Shifts the current text location to a position
    /// relative to the previous line start.
    /// </summary>
    /// <param name="dx"></param>
    /// <param name="dy"></param>
    /// <returns></returns>
    public ContentWriter<T> TextShift(T dx, T dy)
    {
        EnsureInTextState();
        Td_Op<T>.WriteLn(dx, dy, Writer.Stream);
        Td_Op<T>.Apply(ref GfxState, dx, dy);
        return this;
    }


    /// <summary>
    /// Moves the text position to the specified coordinates in 
    /// user space.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public ContentWriter<T> TextMove(T x, T y) => TextTransform(GfxMatrix<T>.Identity with { E = x, F = y });

    public ContentWriter<T> TextTransform(GfxMatrix<T> tm)
    {
        EnsureInTextState();
        Tm_Op<T>.WriteLn(tm, Writer.Stream);
        GfxState.Text.TextLineMatrix = tm;
        GfxState.Text.TextMatrix = tm;
        GfxState.UpdateTRM();
        return this;
    }
    public ContentWriter<T> ResetText()
    {
        if (State == PageState.Text)
        {
            TextTransform(GfxMatrix<T>.Identity);
        } else
        {
            BeginText();
        }
       
        return this;
    }
    public ContentWriter<T> EndText()
    {
        EnsureInTextState();
        State = PageState.Page;
        ET_Op.WriteLn(Writer.Stream);
        return this;
    }

    public ContentWriter<T> BeginText()
    {
        EnsureNotPathState();
        if (State == PageState.Text) { return this; }
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
                    TextShift(T.Round(xform.E, 6), T.Round(xform.F, 6));
                }
            }
            else
            {
                TextTransform(lm);
            }

        }
        else if (GS.Text.TextMatrix != lm)
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

    internal void EnsureInTextState()
    {
        if (State == PageState.Text) { return; }
        if (State == PageState.Page)
        {
            BeginText();
            return;
        }
        if (State == PageState.Path)
        {
            throw new PdfLexerException("Path construction started but not completed before writing non-path constructing items");
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

    internal PdfDictionary GetOrCreateFontObj(IWritableFont font)
    {
        var nm = AddFont(font);
        foreach (var (k,v) in fontObjs)
        {
            if (v == nm)
            {
                return k;
            }
        }
        throw new PdfLexerException("Writable font dictionary not found.");
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

public enum TextAlign
{
    Left,
    Right,
    Center,
    Justified
}