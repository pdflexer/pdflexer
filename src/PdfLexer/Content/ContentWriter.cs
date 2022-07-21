using PdfLexer.DOM;
using PdfLexer.Filters;
using PdfLexer.Operators;
using System;
using System.Collections.Generic;
using System.Numerics;


// Parts of API in this file for writing were ported from https://github.com/foliojs/pdfkit
// pdfkit is licensed under:
// MIT LICENSE
// Copyright (c) 2014 Devon Govett

namespace PdfLexer.Content
{
    public class ContentWriter
    {
        private static decimal KAPPA = (decimal)(4 * ((Math.Sqrt(2) - 1) / 3.0));
        private GraphicsState GfxState;
        internal PageState State { get; private set; }
        private PdfDictionary Resources { get; }
        private PdfDictionary XObjs { get; }
        public FlateWriter Stream { get; }

        private double scale;

        public ContentWriter(PdfDictionary resources, PageUnit unit = PageUnit.Points)
        {
            GfxState = new GraphicsState();
            Resources = resources;
            XObjs = resources.GetOrCreateValue<PdfDictionary>(PdfName.XObject);
            Stream = new FlateWriter();
            scale = unit switch
            {
                PageUnit.Points => 1,
                _ => throw new PdfLexerException("Unknown page unit: " + unit)
            };
        }

        public ContentWriter Image(XObjImage img, double x, double y, double w, double h)
        {
            if (scale != 1) { x *= scale; y *= scale; w *= scale; h *= scale; }
            var d = new Matrix4x4(
                (float)w, 0, 0, 0,
                0, (float)h, 0, 0,
                (float)x, (float)y, 1, 0,
                0, 0, 0, 1);
            var cm = GfxState.GetTranslation(d);

            var nm = AddResource(img.Stream, "I");
            EnsureInPageState();

            q_Op.Value.Apply(ref GfxState);
            q_Op.WriteLn(Stream);

            cm_Op.WriteLn(cm, Stream);
            Do_Op.WriteLn(nm, Stream);

            Q_Op.Value.Apply(ref GfxState);
            Q_Op.WriteLn(Stream);
            return this;
        }

        public ContentWriter Save()
        {
            q_Op.Value.Apply(ref GfxState);
            q_Op.WriteLn(Stream);
            return this;
        }

        public ContentWriter Restore()
        {
            Q_Op.Value.Apply(ref GfxState);
            Q_Op.WriteLn(Stream);
            return this;
        }

        public ContentWriter ClosePath()
        {
            h_Op.WriteLn(Stream);
            return this;
        }

        public ContentWriter LineWidth(decimal w)
        {
            if (scale != 1) { w *=  (decimal)scale; }
            w_Op.WriteLn(w, Stream);
            return this;
        }

        public enum CapStyle
        {
            BUTT = 0,
            ROUND = 1,
            SQUARE = 2
        }

        public ContentWriter LineCap(CapStyle c)
        {
            J_Op.WriteLn((int)c, Stream);
            return this;
        }

        public enum JoinStyle
        {
            MITER = 0,
            ROUND = 1,
            BEVEL = 2
        }

        public ContentWriter LineJoin(JoinStyle c)
        {
            j_Op.WriteLn((int)c, Stream);
            return this;
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
            r = Math.Min(Math.Min(r, 0.5m * w), 0.5m*h);
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

        public ContentWriter Scale(decimal x, decimal y)
        {
            var cm = new Matrix4x4(
                (float)x, 0, 0, 0,
                0, (float)y, 0, 0,
                0, 0, 1, 0,
                0, 0, 0, 1);

            GfxState.Apply(cm);
            cm_Op.WriteLn(cm, Stream);
            return this;
        }


        public ContentWriter Rotate(double angle)
        {
            return RotateAt(angle, 0, 0);
        }

        public ContentWriter RotateAt(double angle, double x, double y)
        {
            if (scale != 1) { x *= scale; y *= scale; }
            var rad = (angle * Math.PI) / 180;
            var cos = Math.Cos(rad);
            var sin = Math.Sin(rad);

            var cm = new Matrix4x4(
                (float)cos, (float)sin, 0, 0,
                (float)-sin, (float)cos, 0, 0,
                (float)x, (float)y, 1, 0,
                0, 0, 0, 1);

            GfxState.Apply(cm);
            cm_Op.WriteLn(cm, Stream);
            return this;
        }

        private Dictionary<IPdfObject, PdfName> added = new Dictionary<IPdfObject, PdfName>();
        private int objCnt = 1;
        private PdfName AddResource(IPdfObject obj, string pfx)
        {
            if (added.TryGetValue(obj, out var existing))
            {
                return existing;
            }
            PdfName name = $"/{pfx}{objCnt++}";
            while (XObjs.ContainsKey(name))
            {
                name = $"/{pfx}{objCnt++}";
            }
            added[obj] = name;
            XObjs[name] = PdfIndirectRef.Create(obj);
            return name;
        }

        private void EnsureInPageState()
        {
            if (State == PageState.Page) { return; }
        }

        private void ResetState()
        {
            switch (State)
            {
                case PageState.Text:
                    ET_Op.WriteLn(Stream);
                    return;
            }
        }

        public ContentWriter Raw(IPdfOperation op)
        {
            op.Apply(ref GfxState);
            op.Serialize(Stream);
            Stream.Stream.WriteByte((byte)'\n');
            return this;
        }

        public PdfStreamContents Complete() => Stream.Complete();
    }

    internal enum PageState
    {
        Page,
        Path,
        Text,
        // inline image NA
        // External object NA
        // shading object NA
    }

    public enum PageUnit
    {
        Points
    }
}
