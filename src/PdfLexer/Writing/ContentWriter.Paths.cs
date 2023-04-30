using PdfLexer.Content;
using PdfLexer.Content.Model;
using System.Numerics;

namespace PdfLexer.Writing;

// Parts of API in this file for writing were ported from https://github.com/foliojs/pdfkit
// pdfkit is licensed under:
// MIT LICENSE
// Copyright (c) 2014 Devon Govett


#if NET7_0_OR_GREATER
public partial class ContentWriter<T> where T : struct, IFloatingPoint<T>
#else
    public partial class ContentWriter
#endif
{


#if NET7_0_OR_GREATER
    public ContentWriter<T> ClosePath()
#else
    public ContentWriter ClosePath()
#endif
    {
        h_Op.WriteLn(Writer.Stream);
        return this;
    }

#if NET7_0_OR_GREATER
    public ContentWriter<T> Stroke()
#else
    public ContentWriter Stroke()
#endif
    {
        S_Op.WriteLn(Writer.Stream);
        return this;
    }



#if NET7_0_OR_GREATER
    public ContentWriter<T> Fill(bool evenOdd = false)
#else
    public ContentWriter Fill(bool evenOdd = false)
#endif
    {
        if (evenOdd)
        {
            f_Star_Op.WriteLn(Writer.Stream);
        }
        else
        {
            f_Op.WriteLn(Writer.Stream);
        }
        return this;
    }

#if NET7_0_OR_GREATER
    public ContentWriter<T> FillAndStroke(bool evenOdd = false)
#else
    public ContentWriter FillAndStroke(bool evenOdd = false)
#endif
    {
        if (evenOdd)
        {
            B_Star_Op.WriteLn(Writer.Stream);
        }
        else
        {
            B_Op.WriteLn(Writer.Stream);
        }

        return this;
    }

#if NET7_0_OR_GREATER
    public ContentWriter<T> CloseFillAndStroke(bool evenOdd = false)
#else
    public ContentWriter CloseFillAndStroke(bool evenOdd = false)
#endif
    {
        if (evenOdd)
        {
            b_Star_Op.WriteLn(Writer.Stream);
        }
        else
        {
            b_Op.WriteLn(Writer.Stream);
        }

        return this;
    }
#if NET7_0_OR_GREATER
    public ContentWriter<T> Clip(bool evenOdd = false)
#else
    public ContentWriter Clip(bool evenOdd = false)
#endif
    {
        if (evenOdd)
        {
            W_Star_Op.WriteLn(Writer.Stream);
        }
        else
        {
            W_Op.WriteLn(Writer.Stream);
        }
        return this;
    }


#if NET7_0_OR_GREATER
    public ContentWriter<T> EndPathNoOp()
#else
    public ContentWriter EndPathNoOp()
#endif
    {
        n_Op.WriteLn(Writer.Stream);
        return this;
    }

#if NET7_0_OR_GREATER
    public ContentWriter<T> Circle(T x, T y, T r) => Ellipse(x, y, r, r);
#else
    public ContentWriter Circle(decimal x, decimal y, decimal r) => Ellipse(x, y, r, r);
#endif



    // note: from pdfkit js
    // originally based on http://stackoverflow.com/questions/2172798/how-to-draw-an-oval-in-html5-canvas/2173084#2173084
#if NET7_0_OR_GREATER
    public ContentWriter<T> Ellipse(T x, T y, T r1, T r2)
    {
        x -= r1;
        y -= r2;
        var two = FPC<T>.V2;
        var ox = r1 * KAPPA;
        var oy = r2 * KAPPA;
        var xe = x + r1 * two;
        var ye = y + r2 * two;
        var xm = x + r1;
        var ym = y + r2;

        MoveTo(x, ym);
        BezierCurveTo(x, ym - oy, xm - ox, y, xm, y);
        BezierCurveTo(xm + ox, y, xe, ym - oy, xe, ym);
        BezierCurveTo(xe, ym + oy, xm + ox, ye, xm, ye);
        BezierCurveTo(xm - ox, ye, x, ym + oy, x, ym);
        return this.ClosePath();
    }

#else
    public ContentWriter Ellipse(double x, double y, double r1, double r2)
    {

        x -= r1;
        y -= r2;
        var ox = r1 * KAPPA;
        var oy = r2 * KAPPA;
        var xe = x + r1 * 2;
        var ye = y + r2 * 2;
        var xm = x + r1;
        var ym = y + r2;

        MoveTo(x, ym);
        BezierCurveTo(x, ym - oy, xm - ox, y, xm, y);
        BezierCurveTo(xm + ox, y, xe, ym - oy, xe, ym);
        BezierCurveTo(xe, ym + oy, xm + ox, ye, xm, ye);
        BezierCurveTo(xm - ox, ye, x, ym + oy, x, ym);
        return this.ClosePath();
    }
#endif



#if NET7_0_OR_GREATER
    public ContentWriter<T> MoveTo(T x, T y)
    {
        EnsureInPageState();
        if (scale != T.One) { x *= scale; y *= scale; }
        m_Op<T>.WriteLn(x, y, Writer.Stream);
        return this;
    }
#else
    public ContentWriter MoveTo(double x, double y)
    {
        EnsureInPageState();
        if (scale != 1) { x *= scale; y *= scale; }
        m_Op.WriteLn(x, y, Writer.Stream);
        return this;
    }
#endif


#if NET7_0_OR_GREATER
    public ContentWriter<T> LineTo(T x, T y)
    {
        if (scale != T.One) { x *= scale; y *= scale; }
        l_Op<T>.WriteLn(x, y, Writer.Stream);
        return this;
    }
#else
    public ContentWriter LineTo(double x, double y)
    {
        if (scale != 1) { x *= scale; y *= scale; }
        l_Op.WriteLn(x, y, Writer.Stream);
        return this;
    }
#endif

#if NET7_0_OR_GREATER
    public ContentWriter<T> BezierCurveTo(T x1, T y1, T x2, T y2, T x3, T y3)
    {
        if (scale != T.One) { var s = scale; x1 *= s; y1 *= s; x2 *= s; y2 *= s; x3 *= s; y3 *= s; }
        c_Op<T>.WriteLn(x1, y1, x2, y2, x3, y3, Writer.Stream);
        return this;
    }
#else
    public ContentWriter BezierCurveTo(double x1, double y1, double x2, double y2, double x3, double y3)
    {
        if (scale != 1) { var s = scale; x1 *= s; y1 *= s; x2 *= s; y2 *= s; x3 *= s; y3 *= s; }
        c_Op.WriteLn(x1, y1, x2, y2, x3, y3, Writer.Stream);
        return this;
    }
#endif

#if NET7_0_OR_GREATER
    public ContentWriter<T> QuadraticCurveTo(T x1, T y1, T x2, T y2)
    {
        if (scale != T.One) { var s = scale; x1 *= s; y1 *= s; x2 *= s; y2 *= s; }
        v_Op<T>.WriteLn(x1, y1, x2, y2, Writer.Stream);
        return this;
    }
#else
    public ContentWriter QuadraticCurveTo(double x1, double y1, double x2, double y2)
    {
        if (scale != 1) { var s = scale; x1 *= s; y1 *= s; x2 *= s; y2 *= s; }
        v_Op.WriteLn(x1, y1, x2, y2, Writer.Stream);
        return this;
    }
#endif


#if NET7_0_OR_GREATER
    public ContentWriter<T> Rect(PdfRect<T> rect)
    {
        re_Op<T>.WriteLn(rect.LLx, rect.LLy, rect.URx - rect.LLx, rect.URy-rect.LLy, Writer.Stream);
        return this;
    }

    public ContentWriter<T> Rect(T x, T y, T w, T h)
    {
        if (scale != T.One) { var s = scale; x *= s; y *= s; w *= s; h *= s; }
        re_Op<T>.WriteLn(x, y, w, h, Writer.Stream);
        return this;
    }
#else
    public ContentWriter Rect(double x, double y, double w, double h)
    {
        if (scale != 1) { var s = scale; x *= s; y *= s; w *= s; h *= s; }
        re_Op.WriteLn(x, y, w, h, Writer.Stream);
        return this;
    }
#endif


#if NET7_0_OR_GREATER
    public ContentWriter<T> RoundedRect(T x, T y, T w, T h, T r)
    {
        var half = FPC<T>.V0_5;
        r = T.Min(T.Min(r, half * w), half * h);
        var c = r * (T.One - KAPPA);
        MoveTo(x + r, y);
        LineTo(x + w - r, y);
        BezierCurveTo(x + w - c, y, x + w, y + c, x + w, y + r);
        LineTo(x + w, y + h - r);
        BezierCurveTo(x + w, y + h - c, x + w - c, y + h, x + w - r, y + h);
        LineTo(x + r, y + h);
        BezierCurveTo(x + c, y + h, x, y + h - c, x, y + h - r);
        LineTo(x, y + r);
        BezierCurveTo(x, y + c, x + c, y, x + r, y);
        return this.ClosePath();
    }
#else
    public ContentWriter RoundedRect(double x, double y, double w, double h, double r)
    {
        r = Math.Min(Math.Min(r, 0.5m * w), 0.5m * h);
        var c = r * (1 - KAPPA);
        MoveTo(x + r, y);
        LineTo(x + w - r, y);
        BezierCurveTo(x + w - c, y, x + w, y + c, x + w, y + r);
        LineTo(x + w, y + h - r);
        BezierCurveTo(x + w, y + h - c, x + w - c, y + h, x + w - r, y + h);
        LineTo(x + r, y + h);
        BezierCurveTo(x + c, y + h, x, y + h - c, x, y + h - r);
        LineTo(x, y + r);
        BezierCurveTo(x, y + c, x + c, y, x + r, y);
        return this.ClosePath();
    }
#endif


}
