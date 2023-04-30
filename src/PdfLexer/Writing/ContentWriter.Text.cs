using PdfLexer.Fonts;
using PdfLexer.Serializers;
using System.Buffers.Text;
using System;
using System.Numerics;
using PdfLexer.Content;
using System.Drawing;

namespace PdfLexer.Writing;

#if NET7_0_OR_GREATER
public partial class ContentWriter<T> where T : struct, IFloatingPoint<T>
#else
    public partial class ContentWriter
#endif
{


#if NET7_0_OR_GREATER
    public ContentWriter<T> Font(IWritableFont font, T size)
    {
        var nm = AddFont(font);
        writableFont = font;
        Tf_Op<T>.WriteLn(nm, size, Writer.Stream);
        GfxState = GfxState with { FontSize = size }; // font handled already
        return this;
    }
#else
    public ContentWriter Font(IWritableFont font, double size)
    {
        var nm = AddFont(font);
        writableFont = font;
        Tf_Op.WriteLn(nm, size, Writer.Stream);
        GfxState = GfxState with { FontSize = size }; // font handled already
        return this;
    }
#endif


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

    private Dictionary<Base14, IWritableFont> used = new();

#if NET7_0_OR_GREATER
    public ContentWriter<T> Font(Base14 font, T size)
#else
 public ContentWriter Font(Base14 font, double size)
#endif
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
        return Font(wf, size);
    }


#if NET7_0_OR_GREATER
    public ContentWriter<T> Text(string text)
#else
    public ContentWriter Text(string text)
#endif
    {
        if (writableFont == null) { throw new NotSupportedException("Must set current font before writing."); }
        // very inefficient just experimenting
        var b = new byte[2];
        Span<byte> buf = new byte[text.Length];
        int os = 0;
        Writer.Stream.WriteByte((byte)'[');
        foreach (var c in writableFont.ConvertFromUnicode(text, 0, text.Length, b))
        {
            if (c.PrevKern != 0)
            {
                StringSerializer.WriteToStream(buf.Slice(0, os), Writer.Stream);
                PdfOperator.Writedecimal((decimal)c.PrevKern, Writer.Stream);
                os = 0;
            }
            buf[os++] = b[0];
        }
        StringSerializer.WriteToStream(buf.Slice(0, os), Writer.Stream);
        Writer.Stream.WriteByte((byte)']');
        Writer.Stream.WriteByte((byte)' ');
        Writer.Stream.Write(TJ_Op.OpData);
        Writer.Stream.WriteByte((byte)'\n');

        return this;
    }

#if NET7_0_OR_GREATER
    public ContentWriter<T> TextMove(T x, T y)
    {
        EnsureInTextState();
        Td_Op<T>.WriteLn(x, y, Writer.Stream);
        Td_Op<T>.Apply(ref GfxState, x, y);
        return this;
    }
#else
    public ContentWriter TextMove(double x, double y)
    {
        EnsureInTextState();
        Td_Op.WriteLn(x, y, Writer.Stream);
        Td_Op.Apply(ref GfxState, x, y);
        return this;
    }
#endif

#if NET7_0_OR_GREATER
    public ContentWriter<T> TextTransform(GfxMatrix<T> tm)
    {
        EnsureInTextState();
        Tm_Op<T>.WriteLn(tm, Writer.Stream);
        GfxState.Text.TextLineMatrix = tm;
        GfxState.Text.TextMatrix = tm;
        GfxState.UpdateTRM();
        return this;
    }
#endif



#if NET7_0_OR_GREATER
    public ContentWriter<T> EndText()
#else
    public ContentWriter EndText()
#endif
    {
        State = PageState.Page;
        ET_Op.WriteLn(Writer.Stream);
        return this;
    }

#if NET7_0_OR_GREATER
    public ContentWriter<T> BeginText()
    {
        State = PageState.Text;
        BT_Op.WriteLn(Writer.Stream);
        GfxState.Text.TextLineMatrix = GfxMatrix<T>.Identity;
        GfxState.Text.TextMatrix = GfxMatrix<T>.Identity;
        GfxState.UpdateTRM();
        return this;
    }
#else
    public ContentWriter BeginText()
    {
        State = PageState.Text;
        BT_Op.WriteLn(Writer.Stream);
        GfxState.Text.TextLineMatrix = GfxMatrix.Identity;
        GfxState.Text.TextMatrix = GfxMatrix.Identity;
        GfxState.UpdateTRM();
        return this;
    }
#endif

#if NET7_0_OR_GREATER
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

#endif

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
