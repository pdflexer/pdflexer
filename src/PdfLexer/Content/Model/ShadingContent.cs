using PdfLexer.Writing;
using System.Numerics;

namespace PdfLexer.Content.Model;

internal class ShadingContent<T> : ISinglePartCopy<T>, IContentGroup<T> where T : struct, IFloatingPoint<T>
{
    public ContentType Type { get; } = ContentType.Shading;
    public required GfxState<T> GraphicsState { get; set; }
    public List<MarkedContent>? Markings { get; set; }
    public required IPdfObject Shading { get; set; }
    public bool CompatibilitySection { get; set; }

    public void Write(ContentWriter<T> writer)
    {
        writer.Shading(Shading);
    }

    // TODO bounding box

    public ISinglePartCopy<T> Clone()
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

    IContentGroup<T>? IContentGroup<T>.CopyArea(PdfRect<T> rect) => ((ISinglePartCopy<T>)this).CopyAreaByClipping(rect);

    (IContentGroup<T>? Inside, IContentGroup<T>? Outside) IContentGroup<T>.Split(PdfRect<T> rect) => ((ISinglePartCopy<T>)this).SplitByClipping(rect);
}