using PdfLexer.Writing;

namespace PdfLexer.Content.Model;


internal class ImageContent : IContentGroup
{
    public ContentType Type { get; } = ContentType.Image;
    public required GfxState GraphicsState { get; set; }
    public required PdfStream Stream { get; set; }
    public List<MarkedContent>? Markings { get; set; }
    public bool CompatibilitySection { get; set; }
    
    // TODO inline support
    // public bool Inline { get; set; }

    public void Write(ContentWriter writer)
    {
        writer.Image(Stream);
    }
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