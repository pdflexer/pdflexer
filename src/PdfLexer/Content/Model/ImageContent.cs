using PdfLexer.Writing;
using System.Numerics;

namespace PdfLexer.Content.Model;


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

    public void Transform(GfxMatrix<T> transformation)
    {
        GraphicsState = GraphicsState with { CTM = transformation * GraphicsState.CTM };
    }

    IContentGroup<T>? IContentGroup<T>.CopyArea(PdfRect<T> rect) => ((ISinglePartCopy<T>)this).CopyAreaByClipping(rect);

    (IContentGroup<T>? Inside, IContentGroup<T>? Outside) IContentGroup<T>.Split(PdfRect<T> rect) => ((ISinglePartCopy<T>)this).SplitByClipping(rect);
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