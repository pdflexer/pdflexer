﻿using PdfLexer.Content;
using PdfLexer.Fonts;
using System.Drawing;
using System.Numerics;

namespace PdfLexer.Writing.TextLayout;

internal class TextBoxWriter<T> : ITextBoxWriter<T> where T : struct, IFloatingPoint<T>
{
    public ContentWriter<T> Writer { get; }
    public TextBox<T> Box { get; }
    public PdfPoint<T>? Position { get; set; }

    public TextBoxWriter(ContentWriter<T> writer, IWritableFont font, T width, T height = default)
    {
        Writer = writer;
        Box = new TextBox<T>(writer.GS, font, width, height);
    }

    public ITextBoxWriter<T> TextBoxFont(Base14 font, T size, bool setTextLeading = true)
    {
        var wf = Writer.GetBase14Font(font);
        return TextBoxFont(wf, size, setTextLeading);
    }

    public ITextBoxWriter<T> TextBoxFont(IWritableFont font, T fontSize, bool setTextLeading = true)
    {
        var dict = Writer.GetOrCreateFontObj(font);
        Box.CurrentFont = font;
        Box.CurrentState = Box.CurrentState with
        {
            FontSize = fontSize,
            FontObject = dict,
            TextLeading = setTextLeading ? fontSize : Box.CurrentState.TextLeading
        };
        return this;
    }

    public ITextBoxWriter<T> TextBoxWrite(string text)
    {
        Box.AddText(text);
        return this;
    }

    public void TextBoxComplete() => TextBoxCompleteImpl(Position);

    public void TextBoxComplete(VerticalAlign align) => TextBoxCompleteImpl(Position, align);

    public void TextBoxComplete(PdfRect<T> rect) => TextBoxCompleteImpl(new PdfPoint<T> { X = rect.LLx, Y = rect.URy });

    public void TextBoxComplete(PdfRect<T> rect, VerticalAlign align) => TextBoxCompleteImpl(new PdfPoint<T> { X = rect.LLx, Y = rect.URy }, align);

    public void TextBoxComplete(PdfPoint<T> point) => TextBoxCompleteImpl(point);
    private void TextBoxCompleteImpl(PdfPoint<T>? point, VerticalAlign align = VerticalAlign.Top)
    {
        Writer.EnsureInTextState();
        var existing = Writer.GfxState;
        if (point.HasValue)
        {
            var x = existing.Text.TextMatrix.E;
            var iy = existing.Text.TextMatrix.F;

            var pt = point.Value;
            var extraHeight = Box.Height - Box.YPos - (Box.FirstLineSize ?? T.Zero);
            var y = align switch
            {
                VerticalAlign.Center => extraHeight > T.Zero ? 
                                            (pt.Y - Box.FirstLineSize ?? T.Zero) - extraHeight/(T.One + T.One)
                                            : pt.Y - Box.FirstLineSize ?? T.Zero,
                VerticalAlign.Bottom => (pt.Y - Box.FirstLineSize ?? T.Zero) - extraHeight,
                VerticalAlign.Top => pt.Y - Box.FirstLineSize ?? T.Zero,
                _ => pt.Y - Box.FirstLineSize ?? T.Zero

            };
            Writer.TextMove(x, iy + y);
        }
        Box.Apply(Writer);
        Writer.SetGS(existing);
        Writer.TextTransform(existing.Text.TextLineMatrix);
    }
}

public enum VerticalAlign
{
    Top,
    Center,
    Bottom
}