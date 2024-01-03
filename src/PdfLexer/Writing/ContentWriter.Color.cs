using System.Numerics;

namespace PdfLexer.Writing;

// Parts of API in this file for writing were ported from https://github.com/foliojs/pdfkit
// pdfkit is licensed under:
// MIT LICENSE
// Copyright (c) 2014 Devon Govett



public partial class ContentWriter<T> where T : struct, IFloatingPoint<T>
{
    /// <summary>
    /// Sets rgb color for stroking operations
    /// </summary>
    /// <param name="r">Red level 0-255</param>
    /// <param name="g">Green level 0-255</param>
    /// <param name="b">Blue level 0-255</param>
    /// <returns></returns>

    public ContentWriter<T> SetStrokingRGB(int r, int g, int b)
    {
        EnsureNotPathState();
        RG_Op.WriteLn((r & 0xFF) / 255.0, (g & 0xFF) / 255.0, (b & 0xFF) / 255.0, Writer.Stream);
        return this;
    }

    /// <summary>
    /// Sets rgb color for non-stroking / fill operations
    /// </summary>
    /// <param name="r">Red level 0-255</param>
    /// <param name="g">Green level 0-255</param>
    /// <param name="b">Blue level 0-255</param>
    /// <returns></returns>
    public ContentWriter<T> SetFillRGB(int r, int g, int b)
    {
        EnsureNotPathState();
        rg_Op.WriteLn((r & 0xFF) / 255.0, (g & 0xFF) / 255.0, (b & 0xFF) / 255.0, Writer.Stream);
        return this;
    }

    /// <summary>
    /// Sets rgb color for stroking operations
    /// </summary>
    /// <param name="r">Red level 0-1</param>
    /// <param name="g">Green level 0-1</param>
    /// <param name="b">Blue level 0-1</param>
    /// <returns></returns>
    public ContentWriter<T> SetStrokingRGB(T r, T g, T b)
    {
        EnsureNotPathState();
        r = T.Clamp(r, T.Zero, T.One);
        g = T.Clamp(g, T.Zero, T.One);
        b = T.Clamp(b, T.Zero, T.One);
        RG_Op<T>.WriteLn(r, g, b, Writer.Stream);
        return this;
    }

    /// <summary>
    /// Sets rgb color for non-stroking / fill operations
    /// </summary>
    /// <param name="r">Red level 0-1</param>
    /// <param name="g">Green level 0-1</param>
    /// <param name="b">Blue level 0-1</param>
    /// <returns></returns>
    public ContentWriter<T> SetFillRGB(T r, T g, T b)
    {
        EnsureNotPathState();
        r = T.Clamp(r, T.Zero, T.One);
        g = T.Clamp(g, T.Zero, T.One);
        b = T.Clamp(b, T.Zero, T.One);
        rg_Op<T>.WriteLn(r, g, b, Writer.Stream);
        return this;
    }

    /// <summary>
    /// Sets gray level for stroking operations.
    /// </summary>
    /// <param name="v">Gray level from 0 to 1</param>
    /// <returns></returns>

    public ContentWriter<T> SetStrokingGray(T v)
    {
        EnsureNotPathState();
        v = T.Clamp(v, T.Zero, T.One);
        G_Op<T>.WriteLn(v, Writer.Stream);
        return this;
    }

    /// <summary>
    /// Sets gray level for non-stroking / fill operations.
    /// </summary>
    /// <param name="v">Gray level from 0 to 1</param>
    /// <returns></returns>
    public ContentWriter<T> SetFillGray(T v)
    {
        EnsureNotPathState();
        v = T.Clamp(v, T.Zero, T.One);
        g_Op<T>.WriteLn(v, Writer.Stream);
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
    public ContentWriter<T> SetStrokingCMYK(T c, T m, T y, T k)
    {
        EnsureNotPathState();
        c = T.Clamp(c, T.Zero, T.One);
        m = T.Clamp(m, T.Zero, T.One);
        y = T.Clamp(y, T.Zero, T.One);
        k = T.Clamp(k, T.Zero, T.One);
        K_Op<T>.WriteLn(c, m, y, k, Writer.Stream);
        return this;
    }

    public ContentWriter<T> SetFillCMYK(T c, T m, T y, T k)
    {
        EnsureNotPathState();
        c = T.Clamp(c, T.Zero, T.One);
        m = T.Clamp(m, T.Zero, T.One);
        y = T.Clamp(y, T.Zero, T.One);
        k = T.Clamp(k, T.Zero, T.One);
        k_Op<T>.WriteLn(c, m, y, k, Writer.Stream);
        return this;
    }

    private Dictionary<IPdfObject, PdfName> colorSpaces = new Dictionary<IPdfObject, PdfName>();
    internal PdfName AddColorSpace(IPdfObject cs)
    {
        if (colorSpaces.TryGetValue(cs, out var name)) return name;

        name = $"C{objCnt++}";
        while (ColorSpaces.ContainsKey(name))
        {
            name = $"C{objCnt++}";
        }

        colorSpaces[cs] = name;
        ColorSpaces[name] = cs;
        return name;
    }
}
