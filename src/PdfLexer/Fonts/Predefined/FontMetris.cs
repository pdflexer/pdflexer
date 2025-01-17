﻿using PdfLexer.DOM;

namespace PdfLexer.Fonts.Predefined;

internal class FontMetrics
{
    public PdfName BaseFont { get; set; } = null!;
    public FontDescriptor Descriptor { get; set; } = null!;

    public static FontMetrics TimesRoman = new FontMetrics
    {
        BaseFont = "Times-Roman",
        Descriptor = new FontDescriptor
        {
            FontName = "Times-Roman",
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
        BaseFont = "Times-Bold",
        Descriptor = new FontDescriptor
        {
            FontName = "Times-Bold",
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
        BaseFont = "Times-BoldItalic",
        Descriptor = new FontDescriptor
        {
            FontName = "Times-BoldItalic",
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
        BaseFont = "Times-Italic",
        Descriptor = new FontDescriptor
        {
            FontName = "Times-Italic",
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

    public static FontMetrics Courier = new FontMetrics
    {
        BaseFont = "Courier",
        Descriptor = new FontDescriptor
        {
            FontName = "Courier",
            FontFamily = new PdfString("Courier"),
            Flags = FontFlags.Nonsymbolic | FontFlags.FixedPitch | FontFlags.Serif,
            FontBBox = new PdfRectangle
            {
                LLx = -23,
                LLy = -250,
                URx = 715,
                URy = 805
            },
            ItalicAngle = PdfCommonNumbers.Zero,
            CapHeight = 562,
            Ascent = 629,
            Descent = -157,
            StemH = 51,
            StemV = 51,
        }
    };

    public static FontMetrics CourierBold = new FontMetrics
    {
        BaseFont = "Courier-Bold",
        Descriptor = new FontDescriptor
        {
            FontName = "Courier-Bold",
            FontFamily = new PdfString("Courier"),
            Flags = FontFlags.Nonsymbolic | FontFlags.FixedPitch | FontFlags.Serif | FontFlags.ForceBold,
            FontBBox = new PdfRectangle
            {
                LLx = -113,
                LLy = -250,
                URx = 749,
                URy = 801
            },
            ItalicAngle = PdfCommonNumbers.Zero,
            CapHeight = 562,
            Ascent = 629,
            Descent = -157,
            StemH = 84,
            StemV = 106,
        }
    };

    public static FontMetrics CourierBoldItalic = new FontMetrics
    {
        BaseFont = "Courier-BoldOblique",
        Descriptor = new FontDescriptor
        {
            FontName = "Courier-BoldOblique",
            FontFamily = new PdfString("Courier"),
            Flags = FontFlags.Nonsymbolic | FontFlags.FixedPitch | FontFlags.Serif | FontFlags.ForceBold | FontFlags.Italic,
            FontBBox = new PdfRectangle
            {
                LLx = -57,
                LLy = -250,
                URx = 869,
                URy = 801
            },
            ItalicAngle = new PdfDecimalNumber(-12),
            CapHeight = 562,
            Ascent = 629,
            Descent = -157,
            StemH = 84,
            StemV = 106,
        }
    };

    public static FontMetrics CourierItalic = new FontMetrics
    {
        BaseFont = "Courier-Oblique",
        Descriptor = new FontDescriptor
        {
            FontName = "Courier-Oblique",
            FontFamily = new PdfString("Courier"),
            Flags = FontFlags.Nonsymbolic | FontFlags.FixedPitch | FontFlags.Serif | FontFlags.Italic,
            FontBBox = new PdfRectangle
            {
                LLx = -27,
                LLy = -250,
                URx = 849,
                URy = 805
            },
            ItalicAngle = new PdfDecimalNumber(-12),
            CapHeight = 562,
            Ascent = 629,
            Descent = -157,
            StemH = 51,
            StemV = 51,
        }
    };

    public static FontMetrics Helvetica = new FontMetrics
    {
        BaseFont = "Helvetica",
        Descriptor = new FontDescriptor
        {
            FontName = "Helvetica",
            FontFamily = new PdfString("Helvetica"),
            Flags = FontFlags.Nonsymbolic,
            FontBBox = new PdfRectangle
            {
                LLx = -166,
                LLy = -225,
                URx = 1000,
                URy = 931
            },
            ItalicAngle = PdfCommonNumbers.Zero,
            CapHeight = 718,
            Ascent = 718,
            Descent = -207,
            StemH = 76,
            StemV = 88,
        }
    };

    public static FontMetrics HelveticaBold = new FontMetrics
    {
        BaseFont = "Helvetica-Bold",
        Descriptor = new FontDescriptor
        {
            FontName = "Helvetica-Bold",
            FontFamily = new PdfString("Helvetica"),
            Flags = FontFlags.Nonsymbolic | FontFlags.ForceBold,
            FontBBox = new PdfRectangle
            {
                LLx = -170,
                LLy = -228,
                URx = 1003,
                URy = 962
            },
            ItalicAngle = PdfCommonNumbers.Zero,
            CapHeight = 718,
            Ascent = 718,
            Descent = -207,
            StemH = 118,
            StemV = 140,
        }
    };

    public static FontMetrics HelveticaBoldItalic = new FontMetrics
    {
        BaseFont = "Helvetica-BoldOblique",
        Descriptor = new FontDescriptor
        {
            FontName = "Helvetica-BoldOblique",
            FontFamily = new PdfString("Helvetica"),
            Flags = FontFlags.Nonsymbolic | FontFlags.ForceBold | FontFlags.Italic,
            FontBBox = new PdfRectangle
            {
                LLx = -174,
                LLy = -228,
                URx = 1114,
                URy = 962
            },
            ItalicAngle = new PdfDecimalNumber(-12),
            CapHeight = 718,
            Ascent = 718,
            Descent = -207,
            StemH = 118,
            StemV = 140,
        }
    };

    public static FontMetrics HelveticaItalic = new FontMetrics
    {
        BaseFont = "Helvetica-Oblique",
        Descriptor = new FontDescriptor
        {
            FontName = "Helvetica-Oblique",
            FontFamily = new PdfString("Helvetica"),
            Flags = FontFlags.Nonsymbolic | FontFlags.Italic,
            FontBBox = new PdfRectangle
            {
                LLx = -170,
                LLy = -225,
                URx = 1116,
                URy = 931
            },
            ItalicAngle = new PdfDecimalNumber(-12),
            CapHeight = 718,
            Ascent = 718,
            Descent = -207,
            StemH = 76,
            StemV = 88,
        }
    };

    public static FontMetrics Symbol = new FontMetrics
    {
        BaseFont = "Symbol",
        Descriptor = new FontDescriptor
        {
            FontName = "Symbol",
            FontFamily = new PdfString("Symbol"),
            Flags = FontFlags.Symbolic,
            FontBBox = new PdfRectangle
            {
                LLx = -180,
                LLy = -292,
                URx = 1090,
                URy = 1010
            },
            ItalicAngle = PdfCommonNumbers.Zero,
            StemH = 92,
            StemV = 85,
        }
    };

    public static FontMetrics ZapfDingbats = new FontMetrics
    {
        BaseFont = "ZapfDingbats",
        Descriptor = new FontDescriptor
        {
            FontName = "ZapfDingbats",
            FontFamily = new PdfString("ZapfDingbats"),
            Flags = FontFlags.Symbolic,
            FontBBox = new PdfRectangle
            {
                LLx = -1,
                LLy = -143,
                URx = 981,
                URy = 820
            },
            ItalicAngle = PdfCommonNumbers.Zero,
            StemH = 28,
            StemV = 90,
        }
    };
}
