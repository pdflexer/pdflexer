using PdfLexer.Writing;
using System;
using System.Collections.Generic;
using System.Text;

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