using PdfLexer.DOM;
using PdfLexer.Fonts;
using PdfLexer.Operators;
using PdfLexer.Serializers;
using System;
using System.Collections.Generic;

namespace PdfLexer.Content
{
    public partial class ContentWriter
    {
        public ContentWriter Font(IWritableFont font, double size)
        {
            EnsureInTextState();
            var nm = AddFont(font);
            currentFont = font;
            Tf_Op.WriteLn(nm, (decimal)size, Stream);
            return this;
        }

        public ContentWriter Text(string text)
        {
            // very inefficient just experimenting
            var b = new byte[2];
            Span<byte> buf = new byte[text.Length];
            int os = 0;
            Stream.Stream.WriteByte((byte)'[');
            foreach (var c in currentFont.ConvertFromUnicode(text, 0, text.Length, b))
            {
                if (c.PrevKern != 0)
                {
                    StringSerializer.WriteToStream(buf.Slice(0, os), Stream);
                    PdfOperator.Writedecimal((decimal)c.PrevKern, Stream);
                    os = 0;
                }
                buf[os++] = b[0];
            }
            StringSerializer.WriteToStream(buf.Slice(0, os), Stream);
            Stream.Stream.WriteByte((byte)']');
            Stream.Stream.WriteByte((byte)' ');
            Stream.Stream.Write(TJ_Op.OpData);
            Stream.Stream.WriteByte((byte)'\n');

            return this;
        }

        public ContentWriter TextMove(double x, double y)
        {
            Td_Op.WriteLn((decimal)x, (decimal)y, Stream);
            return this;
        }

        public ContentWriter EndText()
        {
            ET_Op.WriteLn(Stream);
            return this;
        }

        public ContentWriter BeginText()
        {
            BT_Op.WriteLn(Stream);
            return this;
        }

        private void EnsureInTextState()
        {
            if (State == PageState.Text) { return; }
            if (State == PageState.Page)
            {
                BeginText();
                State = PageState.Text;
                return;
            }
        }
        private IWritableFont currentFont;
        private Dictionary<IWritableFont, PdfName> fonts = new Dictionary<IWritableFont, PdfName>();

        private PdfName AddFont(IWritableFont obj)
        {
            if (fonts.TryGetValue(obj, out var existing))
            {
                return existing;
            }
            
            var fnt = obj.GetPdfFont();
            PdfName name = $"/F{objCnt++}";
            while (Fonts.ContainsKey(name))
            {
                name = $"/F{objCnt++}";
            }
            fonts[obj] = name;

            Fonts[name] = PdfIndirectRef.Create(fnt);
            return name;
        }
    }
}
