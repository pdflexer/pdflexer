using PdfLexer.Writing;

namespace PdfLexer.Content.Model;



internal class XFormContent : IContentGroup
{
    public ContentType Type { get; } = ContentType.XForm;
    public required GfxState GraphicsState { get; set; }
    public required PdfStream Stream { get; set; }
    public List<MarkedContent>? Markings { get; set; }
    public bool CompatibilitySection { get; set; }

    public void Write(ContentWriter writer)
    {
        writer.Form(Stream);
    }


    // not able to accurately get bounding box for form
    // without fully processing it
}

internal class XImgContent : IContentGroup
{
    public ContentType Type { get; } = ContentType.XImage;
    public required GfxState GraphicsState { get; set; }
    public required PdfStream Stream { get; set; }
    public List<MarkedContent>? Markings { get; set; }
    public bool CompatibilitySection { get; set; }

    public void Write(ContentWriter writer)
    {
        writer.Image(Stream);
    }
}

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