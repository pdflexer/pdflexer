using PdfLexer.Fonts;
using PdfLexer.Serializers;
using System.Buffers.Text;
using System;
using System.Numerics;

namespace PdfLexer.Writing;

public partial class ContentWriter
{
    public ContentWriter Font(IWritableFont font, double size)
    {
        var nm = AddFont(font);
        writableFont = font;
        Tf_Op.WriteLn(nm, (decimal)size, StreamWriter.Stream);
        GfxState = GfxState with { FontSize = (float)size }; // font handled already
        return this;
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

    private Dictionary<Base14, IWritableFont> used = new();

    public ContentWriter Font(Base14 font, double size)
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

    public ContentWriter Text(string text)
    {
        if (writableFont == null) { throw new NotSupportedException("Must set current font before writing."); }
        // very inefficient just experimenting
        var b = new byte[2];
        Span<byte> buf = new byte[text.Length];
        int os = 0;
        StreamWriter.Stream.WriteByte((byte)'[');
        foreach (var c in writableFont.ConvertFromUnicode(text, 0, text.Length, b))
        {
            if (c.PrevKern != 0)
            {
                StringSerializer.WriteToStream(buf.Slice(0, os), StreamWriter.Stream);
                PdfOperator.Writedecimal((decimal)c.PrevKern, StreamWriter.Stream);
                os = 0;
            }
            buf[os++] = b[0];
        }
        StringSerializer.WriteToStream(buf.Slice(0, os), StreamWriter.Stream);
        StreamWriter.Stream.WriteByte((byte)']');
        StreamWriter.Stream.WriteByte((byte)' ');
        StreamWriter.Stream.Write(TJ_Op.OpData);
        StreamWriter.Stream.WriteByte((byte)'\n');

        return this;
    }

    public ContentWriter TextMove(double x, double y)
    {
        EnsureInTextState();
        Td_Op.WriteLn((decimal)x, (decimal)y, StreamWriter.Stream);
        Td_Op.Apply(ref GfxState, (float)x, (float)y);
        return this;
    }

    public ContentWriter TextTransform(Matrix4x4 tm)
    {
        EnsureInTextState();
        Tm_Op.WriteLn(tm, StreamWriter.Stream);
        GfxState.Text.TextLineMatrix = tm;
        return this;
    }


    public ContentWriter EndText()
    {
        State = PageState.Page;
        ET_Op.WriteLn(StreamWriter.Stream);
        return this;
    }

    public ContentWriter BeginText()
    {
        State = PageState.Text;
        BT_Op.WriteLn(StreamWriter.Stream);
        GfxState.Text.TextLineMatrix = Matrix4x4.Identity;
        GfxState.Text.TextMatrix = Matrix4x4.Identity;
        GfxState.UpdateTRM();
        return this;
    }

    internal ContentWriter SetLinePosition(Matrix4x4 lm)
    {
        EnsureInTextState();
        if (GS.Text.TextLineMatrix != lm)
        {
            var cur = GS.Text.TextLineMatrix;

            Matrix4x4.Invert(cur, out var iv);
            var xform = lm * iv;
            if (xform.M11 == 1 && xform.M12 == 0 && xform.M21 == 0 && xform.M22 == 1)
            {
                if (xform.M31 == 0 && Math.Round(xform.M32 + GS.TextLeading, 6) == 0)
                {
                    Op(T_Star_Op.Value);
                }
                else
                {
                    TextMove(xform.M31, xform.M32);
                }
            }
            else
            {
                TextTransform(lm);
            }
        }
        return this;
    }

    internal void WriteGlyphs(List<UnappliedGlyph> glyphs)
    {
        EnsureInTextState();
        Span<byte> buffer = stackalloc byte[4];
        Span<byte> output = stackalloc byte[16];
        var str = StreamWriter.Stream;
        if (glyphs.Any(x => x.Shift != 0))
        {
            bool inString = false;
            str.WriteByte((byte)'[');
            foreach (var item in glyphs)
            {
                if (item.Shift != 0)
                {
                    if (inString)
                    {
                        str.WriteByte((byte)')');
                        str.WriteByte((byte)' ');
                        inString = false;
                    }
                    if (Utf8Formatter.TryFormat(item.Shift, output, out int cnt))
                    {
                        str.Write(output.Slice(0, cnt));
                        // str.WriteByte((byte)' ');
                    }
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
            }
            str.WriteByte((byte)')');
            str.WriteByte((byte)' ');
            str.Write(Tj_Op.OpData);
            str.WriteByte((byte)'\n');
        }

        static void WriteGlyph(UnappliedGlyph item, Span<byte> buffer, Span<byte> output, Stream str)
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
