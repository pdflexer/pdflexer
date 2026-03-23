using PdfLexer.Writing;
using System.Numerics;

namespace PdfLexer.Content.Model;

/// <summary>
/// Common base for all content nodes in the PDF content model.
/// </summary>
public interface IContentNode<T> where T : struct, IFloatingPoint<T>
{
    /// <summary>
    /// Type of content represented by this node.
    /// </summary>
    public ContentType Type { get; }

    /// <summary>
    /// Gets the bounding box of this content node.
    ///
    /// For some content types this is approximated (eg. vector paths).
    /// </summary>
    public PdfRect<T> GetBoundingBox();
}

/// <summary>
/// A content node that represents a drawable leaf item with its own graphics state.
/// </summary>
public interface IContentItem<T> : IContentNode<T> where T : struct, IFloatingPoint<T>
{
    /// <summary>
    /// Graphics state for this content.
    /// </summary>
    public GfxState<T> GraphicsState { get; }

    /// <summary>
    /// Denotes if this item is drawn as a compatibility section.
    /// </summary>
    public bool CompatibilitySection { get; }

    /// <summary>
    /// Appends the transformation to the items transformation matrix.
    /// </summary>
    public void Transform(GfxMatrix<T> transformation);

    /// <summary>
    /// Prepends the transformation to the items transformation matrix.
    /// </summary>
    public void TransformInitial(GfxMatrix<T> transformation);

    /// <summary>
    /// Creates a new content node that represents the area of the existing
    /// content item that intersects the given rectangle.
    /// 
    /// Supported for leaf content items (text, path, image, shading). 
    /// 
    /// Containers such as forms are considered expansion boundaries and may not support 
    /// direct geometry operations; callers should expand or process form content 
    /// individually if needed.
    /// </summary>
    public IContentNode<T>? CopyArea(PdfRect<T> rect);

    /// <summary>
    /// Creates two new content nodes: one representing content from this item
    /// that is inside of the specified rectangle and one that is outside.
    /// 
    /// Supported for leaf content items (text, path, image, shading). 
    /// 
    /// Containers such as forms are considered expansion boundaries and may not support 
    /// direct geometry operations; callers should expand or process form content 
    /// individually if needed.
    /// </summary>
    public (IContentNode<T>? Inside, IContentNode<T>? Outside) Split(PdfRect<T> rect);

    /// <summary>
    /// Clips the item to the specified rectangle.
    /// </summary>
    void ClipExcept(PdfRect<T> rect);

    /// <summary>
    /// Applies additional clipping from another graphics state.
    /// </summary>
    void ClipFrom(GfxState<T> other);
}

/// <summary>
/// A content node that contains other content nodes.
/// </summary>
public interface IContentContainer<T> : IContentNode<T> where T : struct, IFloatingPoint<T>
{
    /// <summary>
    /// The child content nodes.
    /// </summary>
    public IEnumerable<IContentNode<T>> Children { get; }
}

/// <summary>
/// Interface for content nodes that participate in serialization.
/// </summary>
public interface IContentWritable<T> where T : struct, IFloatingPoint<T>
{
    /// <summary>
    /// Writes the content to a content writer.
    /// </summary>
    public void Write(ContentWriter<T> writer);
}

/// <summary>
/// Legacy interface for PDF content. 
/// Prefer using <see cref="IContentNode{T}"/>, <see cref="IContentItem{T}"/>, 
/// or <see cref="IContentContainer{T}"/>.
/// </summary>
public interface IContentGroup<T> : IContentItem<T>, IContentWritable<T> where T : struct, IFloatingPoint<T>
{
}

internal interface ISinglePartCopy<T> : IContentItem<T> where T : struct, IFloatingPoint<T>
{
    public ISinglePartCopy<T> Clone();
    public new GfxState<T> GraphicsState { get; set; }

    public IContentNode<T>? CopyAreaByClipping(PdfRect<T> rect)
    {
        var cg = (IContentNode<T>)this;
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

    public (IContentNode<T>? Inside, IContentNode<T>? Outside) SplitByClipping(PdfRect<T> rect)
    {
        var cg = (IContentNode<T>)this;
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
