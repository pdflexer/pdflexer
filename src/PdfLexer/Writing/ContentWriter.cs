using PdfLexer.Content;
using PdfLexer.DOM;
using PdfLexer.Filters;
using System;
using System.Numerics;


// Parts of API in this file for writing were ported from https://github.com/foliojs/pdfkit
// pdfkit is licensed under:
// MIT LICENSE
// Copyright (c) 2014 Devon Govett

namespace PdfLexer.Writing;

#if NET7_0_OR_GREATER
public partial class ContentWriter<T> where T : struct, IFloatingPoint<T>
{ 
        private GfxState<T> GfxState;
        internal GfxState<T> GS { get => GfxState; }
        private static T KAPPA = FPC<T>.Util.FromDecimal<T>((decimal)(4 * ((Math.Sqrt(2) - 1) / 3.0)));
        private T scale;
#else
public partial class ContentWriter
{
        private GfxState< GfxState;
        internal GfxState GS { get => GfxState; }
        private static double KAPPA = (double)(4 * ((Math.Sqrt(2) - 1) / 3.0));
        private double scale;
#endif



    internal PageState State { get; private set; }
    internal PdfDictionary Resources { get; }
    private PdfDictionary XObjs { get; }
    private PdfDictionary Fonts { get; }
    public PdfDictionary ColorSpaces { get; }
    public PdfDictionary ExtGS { get; }
    public PdfDictionary Shadings { get; }
    public PdfDictionary Patterns { get; }
    public PdfDictionary Properties { get; }


    public IStreamContentsWriter Writer { get; private set; }

    public ContentWriter(PdfDictionary resources, PageUnit unit = PageUnit.Points)
    {
#if NET7_0_OR_GREATER
        GfxState = new GfxState<T>();
        scale = unit switch
        {
            PageUnit.Points => T.One,
            _ => throw new PdfLexerException("Unknown page unit: " + unit)
        };
#else
        GfxState = new GfxState();
        scale = unit switch
        {
            PageUnit.Points => 1,
            _ => throw new PdfLexerException("Unknown page unit: " + unit)
        };
#endif

        Resources = resources;
        XObjs = resources.GetOrCreateValue<PdfDictionary>(PdfName.XObject);
        Fonts = resources.GetOrCreateValue<PdfDictionary>(PdfName.Font);
        ColorSpaces = resources.GetOrCreateValue<PdfDictionary>(PdfName.ColorSpace);
        ExtGS = resources.GetOrCreateValue<PdfDictionary>(PdfName.ExtGState);
        Shadings = resources.GetOrCreateValue<PdfDictionary>(PdfName.Shading);
        Properties = resources.GetOrCreateValue<PdfDictionary>("Properties");
        Patterns = resources.GetOrCreateValue<PdfDictionary>(PdfName.Pattern);
        Writer = new ZLibLexerStream();
    }



#if NET7_0_OR_GREATER
    public ContentWriter<T> Image(XObjImage img, T x, T y, T w, T h)
    {
        var nm = AddResource(img.Stream, "I");
        return Do(nm, x, y, w, h);
    }
#else
    public ContentWriter Image(XObjImage img, double x, double y, double w, double h)
    {
        var nm = AddResource(img.Stream, "I");
        return Do(nm, x, y, w, h);
    }
#endif

    internal ContentWriter<T> Image(PdfStream img)
    {
        var nm = AddResource(img, "I");
        return Do(nm);
    }

#if NET7_0_OR_GREATER
    public ContentWriter<T> Form(XObjForm form, T x, T y) => Form(form, x, y, T.One, T.One);
    public ContentWriter<T> Form(XObjForm form, T x, T y, T xScale, T yScale)
    {
        var nm = AddResource(form.NativeObject, "F");
        return Do(nm, x, y, xScale, yScale);
    }
#else
    public ContentWriter Form(XObjForm form, double x, double y, double xScale = 1, double yScale = 1)
    {
        var nm = AddResource(form.NativeObject, "F");
        return Do(nm, x, y, xScale, yScale);
    }
#endif




    public ContentWriter<T> Form(XObjForm form)
    {
        var nm = AddResource(form.NativeObject, "F");
        return Do(nm);
    }

    internal ContentWriter<T> Form(PdfStream form)
    {
        var nm = AddResource(form, "F");
        return Do(nm);
    }

#if NET7_0_OR_GREATER
    private ContentWriter<T> Do(PdfName nm, T x, T y, T w, T h)
    {
        if (scale != T.One) { x *= scale; y *= scale; w *= scale; h *= scale; }
        var d = new GfxMatrix<T>(w, T.Zero, T.Zero, h, x, y);
        var cm = GfxState.GetTranslation(d);
        return Do(nm, cm);
    }
    private ContentWriter<T> Do(PdfName nm, GfxMatrix<T>? xform = null)
    {
        EnsureInPageState();
        if (xform.HasValue)
        {
            q_Op<T>.Value.Apply(ref GfxState);
            q_Op<T>.WriteLn(Writer.Stream);
            cm_Op<T>.WriteLn(xform.Value, Writer.Stream);
        }

        Do_Op<T>.WriteLn(nm, Writer.Stream);

        if (xform.HasValue)
        {
            Q_Op<T>.Value.Apply(ref GfxState);
            Q_Op<T>.WriteLn(Writer.Stream);
        }
        return this;
    }
#else
    private ContentWriter Do(PdfName nm, decimal x, decimal y, decimal w, decimal h)
    {
        if (scale != 1) { x *= scale; y *= scale; w *= scale; h *= scale; }
        var d = new GfxMatrix(w, 0, 0, h, x, y);
        var cm = GfxState.GetTranslation(d);
        return Do(nm, cm);
    }

    private ContentWriter Do(PdfName nm, GfxMatrix? xform = null)
    {
        EnsureInPageState();
        if (xform.HasValue)
        {
            q_Op.Value.Apply(ref GfxState);
            q_Op.WriteLn(Writer.Stream);
            cm_Op.WriteLn(xform.Value, Writer.Stream);
        }

        Do_Op.WriteLn(nm, Writer.Stream);

        if (xform.HasValue)
        {
            Q_Op.Value.Apply(ref GfxState);
            Q_Op.WriteLn(Writer.Stream);
        }
        return this;
    }
#endif


#if NET7_0_OR_GREATER
    public ContentWriter<T> Save()
    {
        EnsureInPageState();
        q_Op<T>.Value.Apply(ref GfxState);
        q_Op<T>.WriteLn(Writer.Stream);
        GraphicsStackSize++;
        return this;
    }

    public ContentWriter<T> Restore()
    {
        EnsureInPageState();
        Q_Op<T>.Value.Apply(ref GfxState);
        Q_Op<T>.WriteLn(Writer.Stream);
        GraphicsStackSize--;
        return this;
    }
    public ContentWriter<T> LineJoin(JoinStyle c)
    {
        j_Op<T>.WriteLn((int)c, Writer.Stream);
        return this;
    }

    public ContentWriter<T> LineWidth(T w)
    {
        if (scale != T.One) { w *= scale; }
        w_Op<T>.WriteLn(w, Writer.Stream);
        return this;
    }

    public ContentWriter<T> LineCap(CapStyle c)
    {
        J_Op<T>.WriteLn((int)c, Writer.Stream);
        return this;
    }
#else
    public ContentWriter Save()
    {
        EnsureInPageState();
        q_Op.Value.Apply(ref GfxState);
        q_Op.WriteLn(Writer.Stream);
        GraphicsStackSize++;
        return this;
    }

    public ContentWriter Restore()
    {
        EnsureInPageState();
        Q_Op.Value.Apply(ref GfxState);
        Q_Op.WriteLn(Writer.Stream);
        GraphicsStackSize--;
        return this;
    }
    public ContentWriter LineJoin(JoinStyle c)
    {
        j_Op.WriteLn((int)c, Writer.Stream);
        return this;
    }

    public ContentWriter LineWidth(double w)
    {
        if (scale != 1) { w *= scale; }
        w_Op.WriteLn(w, Writer.Stream);
        return this;
    }

    public ContentWriter LineCap(CapStyle c)
    {
        J_Op.WriteLn((int)c, Writer.Stream);
        return this;
    }
#endif

    public enum CapStyle
    {
        BUTT = 0,
        ROUND = 1,
        SQUARE = 2
    }

    public enum JoinStyle
    {
        MITER = 0,
        ROUND = 1,
        BEVEL = 2
    }

#if NET7_0_OR_GREATER
    public ContentWriter<T> Scale(T x, T y)
    {
        EnsureInPageState();
        var cm = new GfxMatrix<T>(x, T.Zero, T.Zero, y, T.Zero, T.Zero);
        cm_Op<T>.Apply(ref GfxState, cm);
        cm_Op<T>.WriteLn(cm, Writer.Stream);
        return this;
    }

    public ContentWriter<T> Translate(T x, T y)
    {
        EnsureInPageState();
        var cm = new GfxMatrix<T>(T.One, T.Zero, T.Zero, T.One, x, y);
        cm_Op<T>.Apply(ref GfxState, cm);
        cm_Op<T>.WriteLn(cm, Writer.Stream);
        return this;
    }

    public ContentWriter<T> ScaleAt(T xs, T ys, T xLoc, T yLoc)
    {
        EnsureInPageState();
        var xp = -(xs * xLoc);
        var yp = -(ys * yLoc);
        var cm = new GfxMatrix<T>(xs, T.Zero, T.Zero, ys, xp, yp);

        cm_Op<T>.Apply(ref GfxState, cm);
        cm_Op<T>.WriteLn(cm, Writer.Stream);
        return this;
    }

    public ContentWriter<T> Rotate(T angle)
    {
        return RotateAt(angle, T.Zero, T.Zero);
    }


    public ContentWriter<T> RotateAt(T angle, T x, T y)
    {
        EnsureInPageState();
        if (scale != T.One) { x *= scale; y *= scale; }
        var rad = (angle * T.Pi) / FPC<T>.V180;
        var dr = FPC<T>.Util.ToDouble(rad);
        var cos = FPC<T>.Util.FromDouble<T>(Math.Cos(dr)); // TODO
        var sin = FPC<T>.Util.FromDouble<T>(Math.Sin(dr)); // TODO

        var cm = new GfxMatrix<T>(
            cos, sin,
            -sin, cos,
            x, y);

        cm_Op<T>.Apply(ref GfxState, cm);
        cm_Op<T>.WriteLn(cm, Writer.Stream);
        return this;
    }
#else
    public ContentWriter Scale(double x, double y)
    {
        EnsureInPageState();
        var cm = new GfxMatrix(x, 0, 0, y, 0, 0);
        cm_Op.Apply(ref GfxState, cm);
        cm_Op.WriteLn(cm, Writer.Stream);
        return this;
    }

    public ContentWriter Translate(double x, double y)
    {
        EnsureInPageState();
        var cm = new GfxMatrix(1, 0, 0, 1, x, y);
        cm_Op.Apply(ref GfxState, cm);
        cm_Op.WriteLn(cm, Writer.Stream);
        return this;
    }

    public ContentWriter ScaleAt(double xs, double ys, double xLoc, double yLoc)
    {
        EnsureInPageState();
        var xp = -(xs * xLoc);
        var yp = -(ys * yLoc);
        var cm = new GfxMatrix(xs, 0, 0, ys, xp, yp);

        cm_Op.Apply(ref GfxState, cm);
        cm_Op.WriteLn(cm, Writer.Stream);
        return this;
    }

    public ContentWriter Rotate(double angle)
    {
        return RotateAt(angle, 0m, 0m);
    }


    public ContentWriter RotateAt(double angle, double x, double y)
    {
        EnsureInPageState();
        if (scale != 1) { x *= scale; y *= scale; }
        var rad = (angle * Math.PI) / 180.0;
        var cos = Math.Cos(rad);
        var sin = Math.Sin(rad);

        var cm = new GfxMatrix(
            (decimal)cos, (decimal)sin,
            -(decimal)sin, (decimal)cos,
            x, y);

        cm_Op.Apply(ref GfxState, cm);
        cm_Op.WriteLn(cm, Writer.Stream);
        return this;
    }
#endif

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


#if NET7_0_OR_GREATER
    public ContentWriter<T> Op(IPdfOperation<T> op)
    {
        op.Apply(ref GfxState);
        op.Serialize(Writer.Stream);
        Writer.Stream.WriteByte((byte)'\n');
        return this;
    }

    [Obsolete("Use .Op(op) instead")]
    public ContentWriter<T> Raw(IPdfOperation<T> op) => Op(op);

    [Obsolete("Use .Op(op) instead")]
    public ContentWriter<T> CustomOp(IPdfOperation<T> op) => Op(op);
#else
    public ContentWriter Op(IPdfOperation op)
    {
        op.Apply(ref GfxState);
        op.Serialize(Writer.Stream);
        Writer.Stream.WriteByte((byte)'\n');
        return this;
    }

    [Obsolete("Use .Op(op) instead")]
    public ContentWriter Raw(IPdfOperation op) => Op(op);

    [Obsolete("Use .Op(op) instead")]
    public ContentWriter CustomOp(IPdfOperation op) => Op(op);
#endif

    int mcDepth = 0;
    internal int GraphicsStackSize = 0;
    public PdfStreamContents Complete()
    {
        EnsureInPageState();
        for (var i = 0; i < mcDepth; i++)
        {
            EMC_Op.WriteLn(Writer.Stream);
        }
        if (isCompatSection)
        {
            EX_Op.WriteLn(Writer.Stream);
        }
        for (var i = 0; i < GraphicsStackSize; i++)
        {
            Q_Op.WriteLn(Writer.Stream);
        }
        var result = Writer.Complete();
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
        if (Shadings.Count == 0)
        {
            Resources.Remove(PdfName.Shading);
        }
        if (Patterns.Count == 0)
        {
            Resources.Remove(PdfName.Pattern);
        }
        if (Properties.Count == 0)
        {
            Resources.Remove("Properties");
        }
        Writer = null!; // TODO: add proper error handling for using methods after calling complete
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

