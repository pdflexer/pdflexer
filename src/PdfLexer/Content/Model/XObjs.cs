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

    public PdfRect GetBoundingBox()
    {
        return new PdfRect { LLx = 0, LLy = 0, URx = 0, URy = 0, };
    }
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
    public PdfRect GetBoundingBox()
    {
        return new PdfRect { LLx = 0, LLy = 0, URx = 0, URy = 0, };
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

    public PdfRect GetBoundingBox()
    {
        return new PdfRect { LLx = 0, LLy = 0, URx = 0, URy = 0, };
    }
}