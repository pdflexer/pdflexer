using PdfLexer.Writing;
using System.Numerics;

namespace PdfLexer.Content.Model;

public interface IContentGroup<T> where T : struct, IFloatingPoint<T>
{
    public GfxState<T> GraphicsState { get; }
    public ContentType Type { get; }
    public List<MarkedContent>? Markings { get; }
    public bool CompatibilitySection { get; }
    public void Write(ContentWriter<T> writer);

    public PdfRect<T> GetBoundingBox();

    /// <summary>
    /// Applies the transformation to the content group.
    /// </summary>
    /// <param name="transformation"></param>
    public void Transform(GfxMatrix<T> transformation);
    /// <summary>
    /// Creates a new content group that represents the area of the existing
    /// content group that intersects the given rectangle.
    /// 
    /// If no content intersects, null is returned.
    /// 
    /// NOTE: Content that is partially inside of the rectangle will be included fully
    /// and then masked out to be visually correct. For vector curves their bounding
    /// box is approximated and may be innacturate.
    /// </summary>
    /// <param name="rect"></param>
    /// <returns></returns>
    public IContentGroup<T>? CopyArea(PdfRect<T> rect);
    /// <summary>
    /// Creates two new content groups one representation content from this content group
    /// that is inside of the specified rectangle and one that has countent outside of the rectangle.
    /// 
    /// Null will be returned if no content is contained in the resulting content groups.
    /// 
    /// NOTE: Content that is partially inside/outside of the rectangle will be included fully
    /// in both new content groups and then masked out to be visually correct. For vector curves 
    /// their bounding box is approximated and may be innacturate.
    /// </summary>
    /// <param name="rect"></param>
    /// <returns></returns>
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