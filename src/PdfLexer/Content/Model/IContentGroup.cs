using PdfLexer.DOM;
using PdfLexer.Writing;
using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace PdfLexer.Content.Model;



internal interface IContentGroup<T> where T : struct, IFloatingPoint<T>
{
    public GfxState<T> GraphicsState { get; }
    public ContentType Type { get; }
    public List<MarkedContent>? Markings { get; }
    public bool CompatibilitySection { get; }
    public void Write(ContentWriter<T> writer);

    public PdfRect<T> GetBoundingBox();

    // public IContentGroup Shift(decimal dx, decimal dy);

    public IContentGroup<T>? CopyArea(PdfRect<T> rect);
    public (IContentGroup<T>? Inside, IContentGroup<T>? Outside) Split(PdfRect<T> rect);
}

internal interface ISinglePartCopy<T> : IContentGroup<T> where T : struct, IFloatingPoint<T>
{
    public ISinglePartCopy<T> Clone();
    public new GfxState<T> GraphicsState { get; set; }

    public IContentGroup<T>? CopyAreaByClipping(PdfRect<T> rect)
    {
        var cg = (IContentGroup<T>)this;
        var enc = rect.CheckEnclosure(cg.GetBoundingBox());
        if (enc == EncloseType.None)
        {
            return null;
        }
        else if (enc == EncloseType.Full)
        {
            return this;
        }

        var cp = Clone();
        cp.GraphicsState = GraphicsState.ClipExcept(rect);
        return cp;
    }

    public (IContentGroup<T>? Inside, IContentGroup<T>? Outside) SplitByClipping(PdfRect<T> rect)
    {
        var cg = (IContentGroup<T>)this;
        var bb = cg.GetBoundingBox();
        var enc = rect.CheckEnclosure(bb);
        if (enc == EncloseType.None)
        {
            return (null, this);
        }
        else if (enc == EncloseType.Full)
        {
            return (this, null);
        }

        var inside = Clone();
        inside.GraphicsState = GraphicsState.ClipExcept(rect);
        var outside = Clone();
        outside.GraphicsState = GraphicsState.Clip(rect, bb);
        return (inside, outside);
    }
}