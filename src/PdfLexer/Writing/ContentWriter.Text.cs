using PdfLexer.Fonts;
using PdfLexer.Serializers;

namespace PdfLexer.Writing;

public partial class ContentWriter
{
    public ContentWriter Font(IWritableFont font, double size)
    {
        EnsureInTextState();
        var nm = AddFont(font);
        currentFont = font;
        Tf_Op.WriteLn(nm, (decimal)size, StreamWriter.Stream);
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
        if (currentFont == null) { throw new NotSupportedException("Must set current font before writing."); }
        // very inefficient just experimenting
        var b = new byte[2];
        Span<byte> buf = new byte[text.Length];
        int os = 0;
        StreamWriter.Stream.WriteByte((byte)'[');
        foreach (var c in currentFont.ConvertFromUnicode(text, 0, text.Length, b))
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
        return this;
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
    private IWritableFont? currentFont;
    private Dictionary<IWritableFont, PdfName> fonts = new Dictionary<IWritableFont, PdfName>();

    private PdfName AddFont(IWritableFont obj)
    {
        if (fonts.TryGetValue(obj, out var existing))
        {
            return existing;
        }

        var fnt = obj.GetPdfFont();
        PdfName name = $"F{objCnt++}";
        while (Fonts.ContainsKey(name))
        {
            name = $"F{objCnt++}";
        }
        fonts[obj] = name;

        Fonts[name] = PdfIndirectRef.Create(fnt);
        return name;
    }
}
