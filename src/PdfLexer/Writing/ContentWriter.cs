using PdfLexer.Content;
using PdfLexer.Content.Model;
using PdfLexer.DOM;
using PdfLexer.Filters;
using PdfLexer.Fonts;
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

    internal PageState State { get; private set; }
    internal PdfDictionary Resources { get; }
    private PdfDictionary XObjs { get; }
    private PdfDictionary Fonts { get; }
    public PdfDictionary ColorSpaces { get; }
    public PdfDictionary ExtGS { get; }
    public PdfDictionary Properties { get; }

    private GfxState GfxState;
    internal GfxState GS { get => GfxState; }
    public IStreamContentsWriter StreamWriter { get; private set; }

    private double scale;

    public ContentWriter(PdfDictionary resources, PageUnit unit = PageUnit.Points)
    {
        GfxState = new GfxState();
        Resources = resources;
        XObjs = resources.GetOrCreateValue<PdfDictionary>(PdfName.XObject);
        Fonts = resources.GetOrCreateValue<PdfDictionary>(PdfName.Font);
        ColorSpaces = resources.GetOrCreateValue<PdfDictionary>(PdfName.ColorSpace);
        ExtGS = resources.GetOrCreateValue<PdfDictionary>(PdfName.ExtGState);
        Properties = resources.GetOrCreateValue<PdfDictionary>("Properties");
        StreamWriter = new ZLibLexerStream();
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

    internal ContentWriter Image(PdfStream img)
    {
        var nm = AddResource(img, "I");
        return Do(nm);
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

    internal ContentWriter Form(PdfStream form)
    {
        var nm = AddResource(form, "F");
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
            q_Op.WriteLn(StreamWriter.Stream);
            cm_Op.WriteLn(xform.Value, StreamWriter.Stream);
        }

        Do_Op.WriteLn(nm, StreamWriter.Stream);

        if (xform.HasValue)
        {
            Q_Op.Value.Apply(ref GfxState);
            Q_Op.WriteLn(StreamWriter.Stream);
        }
        return this;
    }

    public ContentWriter Save()
    {
        q_Op.Value.Apply(ref GfxState);
        q_Op.WriteLn(StreamWriter.Stream);
        qDepth++;
        return this;
    }

    public ContentWriter Restore()
    {
        Q_Op.Value.Apply(ref GfxState);
        Q_Op.WriteLn(StreamWriter.Stream);
        qDepth--;
        return this;
    }

    public ContentWriter LineWidth(decimal w)
    {
        if (scale != 1) { w *= (decimal)scale; }
        w_Op.WriteLn(w, StreamWriter.Stream);
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
        J_Op.WriteLn((int)c, StreamWriter.Stream);
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
        j_Op.WriteLn((int)c, StreamWriter.Stream);
        return this;
    }

    public ContentWriter Scale(decimal x, decimal y)
    {
        var cm = new Matrix4x4(
            (float)x, 0, 0, 0,
            0, (float)y, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1);

        cm_Op.Apply(ref GfxState, cm);
        cm_Op.WriteLn(cm, StreamWriter.Stream);
        return this;
    }

    public ContentWriter Translate(decimal x, decimal y)
    {
        var cm = new Matrix4x4(
            0, 0, 0, 0,
            0, 0, 0, 0,
            (float)x, (float)y, 1, 0,
            0, 0, 0, 1);

        cm_Op.Apply(ref GfxState, cm);
        cm_Op.WriteLn(cm, StreamWriter.Stream);
        return this;
    }

    internal ContentWriter Transform(Matrix4x4 cm)
    {
        cm_Op.Apply(ref GfxState, cm);
        cm_Op.WriteLn(cm, StreamWriter.Stream);
        return this;
    }

    public ContentWriter ScaleAt(decimal xs, decimal ys, decimal xLoc, decimal yLoc)
    {
        var xp = -(float)(xs * xLoc);
        var yp = -(float)(ys * yLoc);
        var cm = new Matrix4x4(
            (float)xs, 0, 0, 0,
            0, (float)ys, 0, 0,
            xp, yp, 1, 0,
            0, 0, 0, 1);

        cm_Op.Apply(ref GfxState, cm);
        cm_Op.WriteLn(cm, StreamWriter.Stream);
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

        cm_Op.Apply(ref GfxState, cm);
        cm_Op.WriteLn(cm, StreamWriter.Stream);
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
        PdfName name = $"{pfx}{objCnt++}";
        while (XObjs.ContainsKey(name))
        {
            name = $"{pfx}{objCnt++}";
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
                ET_Op.WriteLn(StreamWriter.Stream);
                return;
        }
    }

    public ContentWriter Op(IPdfOperation op)
    {
        op.Apply(ref GfxState);
        op.Serialize(StreamWriter.Stream);
        StreamWriter.Stream.WriteByte((byte)'\n');
        return this;
    }

    [Obsolete("Use .Op(op) instead")]
    public ContentWriter Raw(IPdfOperation op) => Op(op);

    [Obsolete("Use .Op(op) instead")]
    public ContentWriter CustomOp(IPdfOperation op) => Op(op);

    internal ContentWriter SetGS(GfxState state)
    {
        var stream = StreamWriter.Stream;
        if (state == GfxState)
        {
            return this;
        }

        if (state.FontObject != null && state.FontObject != GfxState.FontObject)
        {
            writableFont = null;
            var nm = AddFont(state.FontObject);
            Tf_Op.WriteLn(nm, (decimal)state.FontSize, stream);
        }
        else if (state.FontSize != GfxState.FontSize)
        {
            writableFont = null;
            var nm = AddFont(GfxState.FontObject ?? SingleByteFont.Fallback.NativeObject);
            Tf_Op.WriteLn(nm, (decimal)state.FontSize, stream);
        }

        if (state.CharSpacing != GfxState.CharSpacing)
        {
            Tc_Op.WriteLn((decimal)state.CharSpacing, stream);
        }

        if (state.CTM != GfxState.CTM)
        {
            var cm = GfxState.GetTranslation(state.CTM);
            cm_Op.WriteLn(cm, stream);
        }

        if (state.TextHScale != GfxState.TextHScale)
        {
            Tz_Op.WriteLn((decimal)(state.TextHScale * 100.0), stream);
        }

        if (state.TextLeading != GfxState.TextLeading)
        {
            TL_Op.WriteLn((decimal)(state.TextLeading), stream);
        }

        if (state.TextMode != GfxState.TextMode)
        {
            Tr_Op.WriteLn(state.TextMode, stream);
        }

        if (state.TextRise != GfxState.TextRise)
        {
            Ts_Op.WriteLn((decimal)state.TextRise, stream);
        }

        if (state.WordSpacing != GfxState.WordSpacing)
        {
            Tw_Op.WriteLn((decimal)state.WordSpacing, stream);
        }

        if (state.ColorStroking != GfxState.ColorStroking)
        {
            if (state.ColorStroking == null)
            {
                // TODO colorspace check
                G_Op.WriteLn(0, stream);
            }
            else
            {
                state.ColorStroking.Serialize(stream);
                stream.WriteByte((byte)'\n');
            }
        }

        if (state.Color != GfxState.Color)
        {
            if (state.Color == null)
            {
                // TODO colorspace check
                g_Op.WriteLn(0, stream);
            }
            else
            {
                state.Color.Serialize(stream);
                stream.WriteByte((byte)'\n');
            }
        }

        if (state.Dashing != GfxState.Dashing)
        {
            if (state.Dashing == null)
            {
                d_Op.Default.Serialize(stream);
            }
            else
            {
                state.Dashing.Serialize(stream);
            }
            stream.WriteByte((byte)'\n');
        }

        if (state.Flatness != GfxState.Flatness)
        {
            i_Op.WriteLn((decimal)state.Flatness, stream);
        }

        if (state.ColorSpace != GfxState.ColorSpace)
        {
            if (state.ColorSpace == null)
            {
                g_Op.WriteLn(0, stream);
            }
            else
            {
                if (state.ColorSpace is PdfName nm)
                {
                    cs_Op.WriteLn(nm, stream);
                }
                else
                {
                    nm = AddColorSpace(state.ColorSpace);
                    cs_Op.WriteLn(nm, stream);
                }
            }
        }

        if (state.ColorSpaceStroking != GfxState.ColorSpaceStroking)
        {
            if (state.ColorSpaceStroking == null)
            {
                G_Op.WriteLn(0, stream);
            }
            else
            {
                if (state.ColorSpaceStroking is PdfName nm)
                {
                    CS_Op.WriteLn(nm, stream);
                }
                else
                {
                    nm = AddColorSpace(state.ColorSpaceStroking);
                    CS_Op.WriteLn(nm, stream);
                }
            }
        }

        if (state.ExtDict != GfxState.ExtDict)
        {
            if (state.ExtDict == null)
            {
                if (emptyGS == null)
                {
                    emptyGS = AddExtGS(new PdfDictionary());
                }
                gs_Op.WriteLn(emptyGS, stream);
            }
            else
            {
                var nm = AddExtGS(state.ExtDict);
                gs_Op.WriteLn(nm, stream);
            }
        }

        GfxState = state with { Prev = state.Prev, Text = GfxState.Text };
        return this;
    }
    private Dictionary<PdfDictionary, PdfName> propertyLists = new Dictionary<PdfDictionary, PdfName>();
    internal ContentWriter EndMarkedContent()
    {
        EnsureInPageState();
        mcDepth--;
        EMC_Op.Value.Serialize(StreamWriter.Stream);
        StreamWriter.Stream.WriteByte((byte)'\n');
        return this;
    }
    internal ContentWriter MarkedContent(MarkedContent mc)
    {
        EnsureInPageState(); // make sure in page state to simplify making sure we don't have
                             // uneven operators eg. BT BMC ET EMC
        if (mc.PropList != null)
        {
            if (!propertyLists.TryGetValue(mc.PropList, out var name))
            {
                name = $"PL{objCnt++}";
                while (ExtGS.ContainsKey(name))
                {
                    name = $"PL{objCnt++}";
                }

                propertyLists[mc.PropList] = name;
                Properties[name] = mc.PropList;
            }
            var op = new BDC_Op(mc.Name, name);
            op.Serialize(StreamWriter.Stream);
            StreamWriter.Stream.WriteByte((byte)'\n');
        }
        else if (mc.InlineProps != null)
        {
            var op = new BDC_Op(mc.Name, mc.InlineProps);
            op.Serialize(StreamWriter.Stream);
            StreamWriter.Stream.WriteByte((byte)'\n');
        }
        else
        {
            BMC_Op.WriteLn(mc.Name, StreamWriter.Stream);
        }
        mcDepth++;
        return this;
    }

    private PdfName? emptyGS;
    private Dictionary<PdfDictionary, PdfName> extGraphics = new Dictionary<PdfDictionary, PdfName>();
    internal PdfName AddExtGS(PdfDictionary gs)
    {
        if (extGraphics.TryGetValue(gs, out var name)) return name;

        name = $"GS{objCnt++}";
        while (ExtGS.ContainsKey(name))
        {
            name = $"GS{objCnt++}";
        }

        extGraphics[gs] = name;
        ExtGS[name] = gs;
        return name;
    }

    int mcDepth = 0;
    int qDepth = 0;
    public PdfStreamContents Complete()
    {
        EnsureInPageState();
        for (var i = 0; i < mcDepth; i++)
        {
            EMC_Op.WriteLn(StreamWriter.Stream);
        }
        for (var i = 0; i < qDepth; i++)
        {
            Q_Op.WriteLn(StreamWriter.Stream);
        }
        var result = StreamWriter.Complete();
        if (XObjs.Count == 0)
        {
            Resources.Remove(PdfName.XObject);
        }
        if (Fonts.Count == 0)
        {
            Resources.Remove(PdfName.Font);
        }
        if (ColorSpaces.Count == 0)
        {
            Resources.Remove(PdfName.ColorSpace);
        }
        if (ExtGS.Count == 0)
        {
            Resources.Remove(PdfName.ExtGState);
        }
        if (Properties.Count == 0)
        {
            Resources.Remove("Properties");
        }
        StreamWriter = null!; // TODO: add proper error handling for using methods after calling complete
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

