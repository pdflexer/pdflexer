using PdfLexer.Writing;

namespace PdfLexer.Content.Model;

internal class InlineImage : IContentGroup
{
    public ContentType Type { get; } = ContentType.InlineImage;
    public List<MarkedContent>? Markings { get; set; }
    public bool CompatibilitySection { get; set; }
    public required GfxState GraphicsState { get; set; }
    public required InlineImage_Op Img { get; set; }

    public void Write(ContentWriter writer)
    {
        writer.Op(Img);
    }

    public PdfRect GetBoundingBox()
    {
        return new PdfRect { LLx = 0, LLy = 0, URx = 0, URy = 0, };
    }
}