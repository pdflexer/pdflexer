using PdfLexer.Writing;
using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer.Content.Model;

public record class PdfRect
{
    public required decimal LLx { get; init; }
    public required decimal LLy { get; init; }
    public required decimal URx { get; init; }
    public required decimal URy { get; init; }
}

internal enum ContentType
{
    Text,
    Paths,
    InlineImage,
    XImage,
    XForm,
    Shading,
    MarkedPoint
}
internal interface IContentGroup
{
    public GfxState GraphicsState { get; }
    public ContentType Type { get; }
    public List<MarkedContent>? Markings { get; }
    public bool CompatibilitySection { get; }
    public void Write(ContentWriter writer);

    public PdfRect GetBoundingBox();

}