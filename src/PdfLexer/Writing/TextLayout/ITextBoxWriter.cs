using PdfLexer.Content;
using PdfLexer.Fonts;
using System.Numerics;

namespace PdfLexer.Writing.TextLayout;

public interface ITextBoxWriter<T> where T : struct, IFloatingPoint<T>
{
    ITextBoxWriter<T> TextBoxFont(Base14 font, T size, bool setTextLeading = true);
    ITextBoxWriter<T> TextBoxFont(IWritableFont font, T fontSize, bool setTextLeading = true);
    ITextBoxWriter<T> TextBoxWrite(string text);
    void TextBoxComplete();
    void TextBoxComplete(PdfPoint<T> pt);
    void TextBoxComplete(PdfRect<T> pt);
    void TextBoxComplete(VerticalAlign align);
}
