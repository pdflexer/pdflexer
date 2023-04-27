using PdfLexer.DOM;
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

    public bool Intersects(PdfRect rect)
    {
        if (rect.LLx > URx) return false;
        if (rect.LLy > URy) return false;
        if (rect.URx < LLx) return false;
        if (rect.URy < LLy) return false;
        return true;
    }

    public PdfRect Normalize(PdfPage pg) => Normalize(pg.CropBox);

    public PdfRect Normalize(PdfRectangle rect)
    {
        var x = Math.Min((decimal)rect.LLx, (decimal)rect.URx);
        var y = Math.Min((decimal)rect.LLy, (decimal)rect.URy);
        if (x == 0 && y == 0) { return this; }
        return new PdfRect { LLx = LLx - x, LLy = LLy - y, URx = URx - x, URy = URy - y };
    }
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

    public PdfRect GetBoundingBox()
    {
        var x = GraphicsState.CTM.E;
        var y = GraphicsState.CTM.F;
        return new PdfRect
        {
            LLx = x,
            LLy = y,
            URx = x + GraphicsState.CTM.A,
            URy = y + GraphicsState.CTM.D
        };
    }

}