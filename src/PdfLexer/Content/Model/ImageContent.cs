using PdfLexer.Writing;
using System.Numerics;

namespace PdfLexer.Content.Model;


/// <summary>
/// Image content. This is analogous to a /Image Do PDF operation or an inline image.
/// 
/// Inlines images are currently always converted to XObj images.
/// </summary>
/// <typeparam name="T"></typeparam>
public record class ImageContent<T> : ISinglePartCopy<T> where T : struct, IFloatingPoint<T>
{
    public ContentType Type { get; } = ContentType.Image;
    public required GfxState<T> GraphicsState { get; set; }
    public required PdfStream Stream { get; set; }
    public List<MarkedContent>? Markings { get; set; }
    public bool CompatibilitySection { get; set; }

    // TODO inline support
    // public bool Inline { get; set; }

    public void Write(ContentWriter<T> writer)
    {
        writer.Image(Stream);
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

    ISinglePartCopy<T> ISinglePartCopy<T>.Clone()
    {
        return (ISinglePartCopy<T>)this.MemberwiseClone();
    }

    public void Transform(GfxMatrix<T> transformation) => ((ISinglePartCopy<T>)this).TransformImpl(transformation);

    public void TransformInitial(GfxMatrix<T> transformation) => ((ISinglePartCopy<T>)this).TransformInitialImpl(transformation);

    IContentGroup<T>? IContentGroup<T>.CopyArea(PdfRect<T> rect) => ((ISinglePartCopy<T>)this).CopyAreaByClipping(rect);

    (IContentGroup<T>? Inside, IContentGroup<T>? Outside) IContentGroup<T>.Split(PdfRect<T> rect) => ((ISinglePartCopy<T>)this).SplitByClipping(rect);

    public void ClipExcept(PdfRect<T> rect) => ((ISinglePartCopy<T>)this).ClipExceptImpl(rect);

    public void ClipFrom(GfxState<T> other) => ((ISinglePartCopy<T>)this).ClipFromImpl(other);
}

// Old inline image, implementation not possible without changes since
// inline images can still reference resources that aren't tracked by
// this setup
// internal class InlineImage : IContentGroup
// {
//     public ContentType Type { get; } = ContentType.InlineImage;
//     public List<MarkedContent>? Markings { get; set; }
//     public bool CompatibilitySection { get; set; }
//     public required GfxState GraphicsState { get; set; }
//     public required InlineImage_Op Img { get; set; }
// 
//     public void Write(ContentWriter writer)
//     {
//         writer.Op(Img);
//     }
// 
// }