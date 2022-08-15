using PdfLexer.DOM;

namespace PdfLexer.Fonts.Predefined;

internal class FontMetrics
{
    public PdfName BaseFont { get; set; } = null!;
    public FontDescriptor Descriptor { get; set; } = null!;

    public static FontMetrics TimesRoman = new FontMetrics
    {
        BaseFont = "/Times-Roman",
        Descriptor = new FontDescriptor
        {
            FontName = "/Times-Roman",
            FontFamily = new PdfString("Times"),
            Flags = FontFlags.Nonsymbolic | FontFlags.Serif,
            FontBBox = new PdfRectangle
            {
                LLx = -168,
                LLy = -218,
                URx = 1000,
                URy = 898
            },
            ItalicAngle = PdfCommonNumbers.Zero,
            Ascent = 683,
            Descent = -217,
            CapHeight = 662,
            StemV = 84,
            StemH = 28
        }
    };
}
