using PdfLexer.Writing;
using System.Numerics;

namespace PdfLexer.Content.Model;

/// <summary>
/// Shading content.
/// </summary>
/// <typeparam name="T"></typeparam>
public class ShadingContent<T> : ISinglePartCopy<T>, IContentGroup<T> where T : struct, IFloatingPoint<T>
{
    public ContentType Type { get; } = ContentType.Shading;
    public required GfxState<T> GraphicsState { get; set; }

    public required IPdfObject Shading { get; set; }
    public bool CompatibilitySection { get; set; }

    public void Write(ContentWriter<T> writer)
    {
        writer.Shading(Shading);
    }

    // TODO bounding box

    ISinglePartCopy<T> ISinglePartCopy<T>.Clone()
    {
        return (ISinglePartCopy<T>)this.MemberwiseClone();
    }

    public PdfRect<T> GetBoundingBox()
    {
        var x = GraphicsState.CTM.E;
        var y = GraphicsState.CTM.F;
        return new PdfRect<T>
        {
            LLx = x,
            LLy = y,
            URx = x + GraphicsState.CTM.A,
            URy = y + GraphicsState.CTM.D
        };
    }

    public void Transform(GfxMatrix<T> transformation) => ((ISinglePartCopy<T>)this).TransformImpl(transformation);

    public void TransformInitial(GfxMatrix<T> transformation) => ((ISinglePartCopy<T>)this).TransformInitialImpl(transformation);

    IContentGroup<T>? IContentGroup<T>.CopyArea(PdfRect<T> rect) => ((ISinglePartCopy<T>)this).CopyAreaByClipping(rect);

    (IContentGroup<T>? Inside, IContentGroup<T>? Outside) IContentGroup<T>.Split(PdfRect<T> rect) => ((ISinglePartCopy<T>)this).SplitByClipping(rect);

    public void ClipExcept(PdfRect<T> rect) => ((ISinglePartCopy<T>)this).ClipExceptImpl(rect);

    public void ClipFrom(GfxState<T> other) => ((ISinglePartCopy<T>)this).ClipFromImpl(other);

}