using PdfLexer.Operators;
using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer.Content
{
    // Parts of API in this file for writing were ported from https://github.com/foliojs/pdfkit
    // pdfkit is licensed under:
    // MIT LICENSE
    // Copyright (c) 2014 Devon Govett


    public partial class ContentWriter
    {
        public ContentWriter ClosePath()
        {
            h_Op.WriteLn(Stream);
            return this;
        }

        public ContentWriter Stroke()
        {
            S_Op.WriteLn(Stream);
            return this;
        }

        public ContentWriter Fill(bool evenOdd = false)
        {
            if (evenOdd)
            {
                f_Star_Op.WriteLn(Stream);
            }
            else
            {
                f_Op.WriteLn(Stream);
            }
            return this;
        }

        public ContentWriter FillAndStroke(bool evenOdd = false)
        {
            if (evenOdd)
            {
                B_Star_Op.WriteLn(Stream);
            }
            else
            {
                B_Op.WriteLn(Stream);
            }

            return this;
        }

        public ContentWriter CloseFillAndStroke(bool evenOdd = false)
        {
            if (evenOdd)
            {
                b_Star_Op.WriteLn(Stream);
            }
            else
            {
                b_Op.WriteLn(Stream);
            }

            return this;
        }

        public ContentWriter Clip(bool evenOdd = false)
        {
            if (evenOdd)
            {
                W_Star_Op.WriteLn(Stream);
            }
            else
            {
                W_Op.WriteLn(Stream);
            }
            return this;
        }

        public ContentWriter EndPathNoOp()
        {
            n_Op.WriteLn(Stream);
            return this;
        }

        public ContentWriter Circle(decimal x, decimal y, decimal r) => Ellipse(x, y, r, r);

        public ContentWriter Ellipse(decimal x, decimal y, decimal r1, decimal r2)
        {
            // note: from pdfkit js
            // originally based on http://stackoverflow.com/questions/2172798/how-to-draw-an-oval-in-html5-canvas/2173084#2173084
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

        public ContentWriter MoveTo(decimal x, decimal y)
        {
            if (scale != 1) { x *= (decimal)scale; y *= (decimal)scale; }
            m_Op.WriteLn(x, y, Stream);
            return this;
        }

        public ContentWriter LineTo(decimal x, decimal y)
        {
            if (scale != 1) { x *= (decimal)scale; y *= (decimal)scale; }
            l_Op.WriteLn(x, y, Stream);
            return this;
        }

        public ContentWriter BezierCurveTo(decimal x1, decimal y1, decimal x2, decimal y2, decimal x3, decimal y3)
        {
            if (scale != 1) { var s = (decimal)scale; x1 *= s; y1 *= s; x2 *= s; y2 *= s; x3 *= s; y3 *= s; }
            c_Op.WriteLn(x1, y1, x2, y2, x3, y3, Stream);
            return this;
        }

        public ContentWriter QuadraticCurveTo(decimal x1, decimal y1, decimal x2, decimal y2)
        {
            if (scale != 1) { var s = (decimal)scale; x1 *= s; y1 *= s; x2 *= s; y2 *= s; }
            v_Op.WriteLn(x1, y1, x2, y2, Stream);
            return this;
        }

        public ContentWriter Rect(decimal x, decimal y, decimal w, decimal h)
        {
            if (scale != 1) { var s = (decimal)scale; x *= s; y *= s; w *= s; h *= s; }
            re_Op.WriteLn(x, y, w, h, Stream);
            return this;
        }
        public ContentWriter RoundedRect(decimal x, decimal y, decimal w, decimal h, decimal r)
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
    }
}
