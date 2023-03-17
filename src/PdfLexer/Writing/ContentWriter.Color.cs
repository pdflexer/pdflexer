namespace PdfLexer.Writing;

// Parts of API in this file for writing were ported from https://github.com/foliojs/pdfkit
// pdfkit is licensed under:
// MIT LICENSE
// Copyright (c) 2014 Devon Govett


public partial class ContentWriter
{
    /// <summary>
    /// Sets rgb color for stroking operations
    /// </summary>
    /// <param name="r">Red level 0-255</param>
    /// <param name="g">Green level 0-255</param>
    /// <param name="b">Blue level 0-255</param>
    /// <returns></returns>
    public ContentWriter SetStrokingRGB(int r, int g, int b)
    {
        RG_Op.WriteLn((r & 0xFF) / 255.0m, (g & 0xFF) / 255.0m, (b & 0xFF) / 255.0m, StreamWriter.Stream);
        return this;
    }

    /// <summary>
    /// Sets rgb color for non-stroking / fill operations
    /// </summary>
    /// <param name="r">Red level 0-255</param>
    /// <param name="g">Green level 0-255</param>
    /// <param name="b">Blue level 0-255</param>
    /// <returns></returns>
    public ContentWriter SetFillRGB(int r, int g, int b)
    {
        rg_Op.WriteLn((r & 0xFF) / 255.0m, (g & 0xFF) / 255.0m, (b & 0xFF) / 255.0m, StreamWriter.Stream);
        return this;
    }

    /// <summary>
    /// Sets rgb color for stroking operations
    /// </summary>
    /// <param name="r">Red level 0-1</param>
    /// <param name="g">Green level 0-1</param>
    /// <param name="b">Blue level 0-1</param>
    /// <returns></returns>
    public ContentWriter SetStrokingRGB(decimal r, decimal g, decimal b)
    {
        r = r > 1 ? 1 : (r < 0 ? 0 : r);
        g = g > 1 ? 1 : (g < 0 ? 0 : g);
        b = b > 1 ? 1 : (b < 0 ? 0 : b);
        rg_Op.WriteLn(r, g, b, StreamWriter.Stream);
        return this;
    }

    /// <summary>
    /// Sets rgb color for non-stroking / fill operations
    /// </summary>
    /// <param name="r">Red level 0-1</param>
    /// <param name="g">Green level 0-1</param>
    /// <param name="b">Blue level 0-1</param>
    /// <returns></returns>
    public ContentWriter SetFillRGB(decimal r, decimal g, decimal b)
    {
        r = r > 1 ? 1 : (r < 0 ? 0 : r);
        g = g > 1 ? 1 : (g < 0 ? 0 : g);
        b = b > 1 ? 1 : (b < 0 ? 0 : b);
        rg_Op.WriteLn(r, g, b, StreamWriter.Stream);
        return this;
    }

    /// <summary>
    /// Sets gray level for stroking operations.
    /// </summary>
    /// <param name="v">Gray level from 0 to 1</param>
    /// <returns></returns>
    public ContentWriter SetStrokingGray(decimal v)
    {
        v = v > 1 ? 1 : (v < 0 ? 0 : v);
        G_Op.WriteLn(v, StreamWriter.Stream);
        return this;
    }

    /// <summary>
    /// Sets gray level for non-stroking / fill operations.
    /// </summary>
    /// <param name="v">Gray level from 0 to 1</param>
    /// <returns></returns>
    public ContentWriter SetFillGray(decimal v)
    {
        v = v > 1 ? 1 : (v < 0 ? 0 : v);
        G_Op.WriteLn(v, StreamWriter.Stream);
        return this;
    }

    /// <summary>
    /// Sets CMYK color for stroking operations
    /// </summary>
    /// <param name="c">cyan 0-1</param>
    /// <param name="m">magenta 0-1</param>
    /// <param name="y">yellow 0-1</param>
    /// <param name="k">k</param>
    /// <returns></returns>
    public ContentWriter SetStrokingCMYK(decimal c, decimal m, decimal y, decimal k)
    {
        c = c > 1 ? 1 : (c < 0 ? 0 : c);
        m = m > 1 ? 1 : (m < 0 ? 0 : m);
        y = y > 1 ? 1 : (y < 0 ? 0 : y);
        k = k > 1 ? 1 : (k < 0 ? 0 : k);
        K_Op.WriteLn(c, m, y, k, StreamWriter.Stream);
        return this;
    }

    public ContentWriter SetFillCMYK(decimal c, decimal m, decimal y, decimal k)
    {
        c = c > 1 ? 1 : (c < 0 ? 0 : c);
        m = m > 1 ? 1 : (m < 0 ? 0 : m);
        y = y > 1 ? 1 : (y < 0 ? 0 : y);
        k = k > 1 ? 1 : (k < 0 ? 0 : k);
        K_Op.WriteLn(c, m, y, k, StreamWriter.Stream);
        return this;
    }
}
