using PdfLexer.Content;
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

namespace PdfLexer.Writing;

public partial class ContentWriter
{
    private static decimal KAPPA = (decimal)(4 * ((Math.Sqrt(2) - 1) / 3.0));
    private GraphicsState GfxState;
    internal PageState State { get; private set; }
    private PdfDictionary Resources { get; }
    private PdfDictionary XObjs { get; }
    private PdfDictionary Fonts { get; }
    public FlateWriter Stream { get; private set; }

    private double scale;

    public ContentWriter(PdfDictionary resources, PageUnit unit = PageUnit.Points)
    {
        GfxState = new GraphicsState();
        Resources = resources;
        XObjs = resources.GetOrCreateValue<PdfDictionary>(PdfName.XObject);
        Fonts = resources.GetOrCreateValue<PdfDictionary>(PdfName.Font);
        Stream = new FlateWriter();
        scale = unit switch
        {
            PageUnit.Points => 1,
            _ => throw new PdfLexerException("Unknown page unit: " + unit)
        };
    }


    public ContentWriter Image(XObjImage form, PdfRectangle rect)
          => Image(form, rect.LLx, rect.LLy, rect.Width, rect.Height);

    public ContentWriter Image(XObjImage img, double x, double y, double w, double h)
    {
        var nm = AddResource(img.Stream, "I");
        return Do(nm, x, y, w, h);
    }

    public ContentWriter Form(XObjForm form, double x, double y, double xScale = 1, double yScale = 1)
    {
        var nm = AddResource(form.NativeObject, "F");
        return Do(nm, x, y, xScale, yScale);
    }

    public ContentWriter Form(XObjForm form)
    {
        var nm = AddResource(form.NativeObject, "F");
        return Do(nm);
    }


    private ContentWriter Do(PdfName nm, double x, double y, double w, double h)
    {
        if (scale != 1) { x *= scale; y *= scale; w *= scale; h *= scale; }
        var d = new Matrix4x4(
            (float)w, 0, 0, 0,
            0, (float)h, 0, 0,
            (float)x, (float)y, 1, 0,
            0, 0, 0, 1);
        var cm = GfxState.GetTranslation(d);
        return Do(nm, cm);
    }

    private ContentWriter Do(PdfName nm, Matrix4x4? xform = null)
    {
        EnsureInPageState();
        if (xform.HasValue)
        {
            q_Op.Value.Apply(ref GfxState);
            q_Op.WriteLn(Stream);
            cm_Op.WriteLn(xform.Value, Stream);
        }

        Do_Op.WriteLn(nm, Stream);

        if (xform.HasValue)
        {
            Q_Op.Value.Apply(ref GfxState);
            Q_Op.WriteLn(Stream);
        }
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


    public ContentWriter SetStrokingRGB(int r, int g, int b)
    {
        RG_Op.WriteLn((r & 0xFF) / 255.0m, (g & 0xFF) / 255.0m, (b & 0xFF) / 255.0m, Stream);
        return this;
    }

    public ContentWriter SetFillRGB(int r, int g, int b)
    {
        rg_Op.WriteLn((r & 0xFF) / 255.0m, (g & 0xFF) / 255.0m, (b & 0xFF) / 255.0m, Stream);
        return this;
    }

    public ContentWriter LineWidth(decimal w)
    {
        if (scale != 1) { w *= (decimal)scale; }
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
        var rad = (angle * Math.PI) / 180.0;
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
        if (State == PageState.Text)
        {
            EndText();
            State = PageState.Page;
            return;
        }
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

    public PdfStreamContents Complete() { 
        var result = Stream.Complete();
        Stream = null!; // TODO: add proper error handling for using methods after calling complete
        return result;
    }
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

