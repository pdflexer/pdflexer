using PdfLexer.Content;
using PdfLexer.Content.Model;
using System.Numerics;

namespace PdfLexer.Writing;

// Parts of API in this file for writing were ported from https://github.com/foliojs/pdfkit
// pdfkit is licensed under:
// MIT LICENSE
// Copyright (c) 2014 Devon Govett


public partial class ContentWriter<T> where T : struct, IFloatingPoint<T>
{
    private void EnsureinPathState()
    {
        if (State != PageState.Path)
        {
            throw new PdfLexerException("Attempted to write pathing before starting path");
        }
    }

    public ContentWriter<T> ClosePath()
    {
        EnsureinPathState();
        h_Op.WriteLn(Writer.Stream);
        return this;
    }

    public ContentWriter<T> Stroke()
    {
        EnsureinPathState();
        S_Op.WriteLn(Writer.Stream);
        State = PageState.Page;
        return this;
    }

    public ContentWriter<T> Fill(bool evenOdd = true)
    {
        EnsureinPathState();
        if (evenOdd)
        {
            f_Star_Op.WriteLn(Writer.Stream);
        }
        else
        {
            f_Op.WriteLn(Writer.Stream);
        }
        State = PageState.Page;
        return this;
    }

    public ContentWriter<T> FillAndStroke(bool evenOdd = true)
    {
        EnsureinPathState();
        if (evenOdd)
        {
            B_Star_Op.WriteLn(Writer.Stream);
        }
        else
        {
            B_Op.WriteLn(Writer.Stream);
        }
        State = PageState.Page;

        return this;
    }

    public ContentWriter<T> CloseFillAndStroke(bool evenOdd = true)
    {
        EnsureinPathState();
        if (evenOdd)
        {
            b_Star_Op.WriteLn(Writer.Stream);
        }
        else
        {
            b_Op.WriteLn(Writer.Stream);
        }
        State = PageState.Page;
        return this;
    }

    public ContentWriter<T> Clip(bool evenOdd = true)
    {
        EnsureinPathState();
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

    public ContentWriter<T> EndPathNoOp()
    {
        EnsureinPathState();
        n_Op.WriteLn(Writer.Stream);
        State = PageState.Page;
        return this;
    }

    public ContentWriter<T> Circle(T x, T y, T r) => Ellipse(x, y, r, r);

    // note: from pdfkit js
    // originally based on http://stackoverflow.com/questions/2172798/how-to-draw-an-oval-in-html5-canvas/2173084#2173084
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

    public ContentWriter<T> MoveTo(T x, T y)
    {
        if (State != PageState.Path)
        {
            EnsureInPageState();
        }
        State = PageState.Path;
        if (scale != T.One) { x *= scale; y *= scale; }
        m_Op<T>.WriteLn(x, y, Writer.Stream);
        return this;
    }

    public ContentWriter<T> LineTo(T x, T y)
    {
        EnsureinPathState();
        if (scale != T.One) { x *= scale; y *= scale; }
        l_Op<T>.WriteLn(x, y, Writer.Stream);
        return this;
    }

    public ContentWriter<T> BezierCurveTo(T x1, T y1, T x2, T y2, T x3, T y3)
    {
        EnsureinPathState();
        if (scale != T.One) { var s = scale; x1 *= s; y1 *= s; x2 *= s; y2 *= s; x3 *= s; y3 *= s; }
        c_Op<T>.WriteLn(x1, y1, x2, y2, x3, y3, Writer.Stream);
        return this;
    }

    public ContentWriter<T> QuadraticCurveTo(T x1, T y1, T x2, T y2)
    {
        EnsureinPathState();
        if (scale != T.One) { var s = scale; x1 *= s; y1 *= s; x2 *= s; y2 *= s; }
        v_Op<T>.WriteLn(x1, y1, x2, y2, Writer.Stream);
        return this;
    }

    public ContentWriter<T> Rect(PdfRect<T> rect)
    {
        if (State != PageState.Path)
        {
            EnsureInPageState();
        }
        State = PageState.Path;
        re_Op<T>.WriteLn(rect.LLx, rect.LLy, rect.URx - rect.LLx, rect.URy-rect.LLy, Writer.Stream);
        return this;
    }

    public ContentWriter<T> Rect(T x, T y, T w, T h)
    {
        if (State != PageState.Path)
        {
            EnsureInPageState();
        }
        State = PageState.Path;
        if (scale != T.One) { var s = scale; x *= s; y *= s; w *= s; h *= s; }
        re_Op<T>.WriteLn(x, y, w, h, Writer.Stream);
        return this;
    }

    public ContentWriter<T> RoundedRect(PdfRect<T> rect, T radius)
    {
        RoundedRect(rect.LLx, rect.LLy, rect.URx - rect.LLx, rect.URy - rect.LLy, radius);
        return this;
    }

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
}
