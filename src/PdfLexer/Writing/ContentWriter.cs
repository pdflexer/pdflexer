using PdfLexer.Content;
using PdfLexer.DOM;
using PdfLexer.Filters;
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
        EnsureInPageState();
        q_Op.Value.Apply(ref GfxState);
        q_Op.WriteLn(StreamWriter.Stream);
        qDepth++;
        return this;
    }

    public ContentWriter Restore()
    {
        EnsureInPageState();
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
        EnsureInPageState();
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
        EnsureInPageState();
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
        EnsureInPageState();
        cm_Op.Apply(ref GfxState, cm);
        cm_Op.WriteLn(cm, StreamWriter.Stream);
        return this;
    }

    public ContentWriter ScaleAt(decimal xs, decimal ys, decimal xLoc, decimal yLoc)
    {
        EnsureInPageState();
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
        EnsureInPageState();
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

    private void EnsureRestorable()
    {
        if (GfxState.Prev == null)
        {
            Save();
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

   

    int mcDepth = 0;
    int qDepth = 0;
    public PdfStreamContents Complete()
    {
        EnsureInPageState();
        for (var i = 0; i < mcDepth; i++)
        {
            EMC_Op.WriteLn(StreamWriter.Stream);
        }
        if (isCompatSection)
        {
            EX_Op.WriteLn(StreamWriter.Stream);
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

