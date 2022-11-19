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

    public static FontMetrics TimesBold = new FontMetrics
    {
        BaseFont = "/Times-Bold",
        Descriptor = new FontDescriptor
        {
            FontName = "/Times-Bold",
            FontFamily = new PdfString("Times"),
            Flags = FontFlags.Nonsymbolic | FontFlags.Serif | FontFlags.ForceBold,
            FontBBox = new PdfRectangle
            {
                LLx = -168,
                LLy = -218,
                URx = 1000,
                URy = 935
            },
            ItalicAngle = PdfCommonNumbers.Zero,
            Ascent = 683,
            Descent = -217,
            CapHeight = 676,
            StemV = 139,
            StemH = 44
        }
    };

    public static FontMetrics TimesBoldItalic = new FontMetrics
    {
        BaseFont = "/Times-BoldItalic",
        Descriptor = new FontDescriptor
        {
            FontName = "/Times-BoldItalic",
            FontFamily = new PdfString("Times"),
            Flags = FontFlags.Nonsymbolic | FontFlags.Serif | FontFlags.ForceBold | FontFlags.Italic,
            FontBBox = new PdfRectangle
            {
                LLx = -200,
                LLy = -218,
                URx = 996,
                URy = 921
            },
            ItalicAngle = -15,
            CapHeight = 669,
            Ascent = 683,
            Descent = -217,
            StemH = 42,
            StemV = 121,
        }
    };

    public static FontMetrics TimesItalic = new FontMetrics
    {
        BaseFont = "/Times-Italic",
        Descriptor = new FontDescriptor
        {
            FontName = "/Times-Italic",
            FontFamily = new PdfString("Times"),
            Flags = FontFlags.Nonsymbolic | FontFlags.Serif | FontFlags.Italic,
            FontBBox = new PdfRectangle
            {
                LLx = -169,
                LLy = -217,
                URx = 1010,
                URy = 883
            },
            ItalicAngle = new PdfDecimalNumber(-15.5m),
            CapHeight = 653,
            Ascent = 683,
            Descent = -217,
            StemH = 32,
            StemV = 76,
        }
    };
}
