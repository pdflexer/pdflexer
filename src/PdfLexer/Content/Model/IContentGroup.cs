using PdfLexer.Writing;
using System.Numerics;

namespace PdfLexer.Content.Model;

/// <summary>
/// Grouping of like content of a PDF.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IContentGroup<T> where T : struct, IFloatingPoint<T>
{
    /// <summary>
    /// Graphics state for this content.
    /// </summary>
    public GfxState<T> GraphicsState { get; }
    /// <summary>
    /// Type of content represented by this content group
    /// </summary>
    public ContentType Type { get; }

    /// <summary>
    /// Denotes if this group is drawn as a compatibility section
    /// </summary>
    public bool CompatibilitySection { get; }

    /// <summary>
    /// Writes to content writer... this should probably be removed from
    /// public api. Do not use.
    /// </summary>
    /// <param name="writer"></param>
    public void Write(ContentWriter<T> writer);

    /// <summary>
    /// Gets the bounding box of this content group.
    ///
    /// For some content types this is approximated (eg. vector paths).
    /// </summary>
    /// <returns></returns>
    public PdfRect<T> GetBoundingBox();

    /// <summary>
    /// Appends the transformation to the content groups transformation matrix
    /// </summary>
    /// <param name="transformation"></param>
    public void Transform(GfxMatrix<T> transformation);

    /// <summary>
    /// Prepends the transformation to the content groups transformation matrix
    /// 
    /// This it typically used to apply a transform relative to how it is
    /// viewed on the page (eg. shift content X points)
    /// </summary>
    /// <param name="transformation"></param>
    public void TransformInitial(GfxMatrix<T> transformation);

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

    void ClipExcept(PdfRect<T> rect);
    void ClipFrom(GfxState<T> other);
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

    public void TransformImpl(GfxMatrix<T> transformation)
    {
        GraphicsState = GraphicsState with
        {
            CTM = transformation * GraphicsState.CTM,
            Clipping = GraphicsState.Clipping == null ? null :
                GraphicsState.Clipping.Select(x => { var c = x.ShallowClone(); c.TM = transformation * c.TM; return c; }).ToList()
        };
    }

    public void TransformInitialImpl(GfxMatrix<T> transformation)
    {
        GraphicsState = GraphicsState with
        {
            CTM = GraphicsState.CTM * transformation,
            Clipping = GraphicsState.Clipping == null ? null :
                GraphicsState.Clipping.Select(x => { var c = x.ShallowClone(); c.TM = c.TM * transformation; return c; }).ToList()
        };
    }

    public void ClipExceptImpl(PdfRect<T> rect)
    {
        GraphicsState = GraphicsState.ClipExcept(rect);
    }

    public void ClipFromImpl(GfxState<T> other)
    {
        if (other.Clipping == null) { return; }
        if (GraphicsState.Clipping != null)
        {
            GraphicsState = GraphicsState with { Clipping = GraphicsState.Clipping.ToList() };
        } else
        {
            GraphicsState = GraphicsState with { Clipping = new List<IClippingSection<T>>() };
        }
        GraphicsState.Clipping.AddRange(other.Clipping);
    }

}