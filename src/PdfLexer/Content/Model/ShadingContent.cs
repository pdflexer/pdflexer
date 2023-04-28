using PdfLexer.Writing;

namespace PdfLexer.Content.Model;

internal class ShadingContent : IContentGroup
{
    public ContentType Type { get; } = ContentType.Shading;
    public required GfxState GraphicsState { get; set; }
    public List<MarkedContent>? Markings { get; set; }
    public required IPdfObject Shading { get; set; }
    public bool CompatibilitySection { get; set; }

    public void Write(ContentWriter writer)
    {
        writer.Shading(Shading);
    }

    // TODO bounding box
}