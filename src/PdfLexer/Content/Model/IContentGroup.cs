using PdfLexer.DOM;
using PdfLexer.Writing;

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

    public EncloseType CheckEnclosure(PdfRect rect)
    {
        if (rect.LLx > URx || rect.LLy > URy || rect.URx < LLx || rect.URy < LLy) return EncloseType.None;
        if (rect.LLx < LLx || rect.LLy < LLy || rect.URx > URx || rect.URy > URy) return EncloseType.Partial;
        return EncloseType.Full;
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

public enum EncloseType
{
    Full,
    Partial,
    None
}

internal enum ContentType
{
    Text,
    Paths,
    Image,
    Form,
    Shading,
    // MarkedPoint
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