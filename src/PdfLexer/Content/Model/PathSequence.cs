using PdfLexer.DOM;
using PdfLexer.Writing;

namespace PdfLexer.Content.Model;

internal class PathSequence : IContentGroup
{
    public ContentType Type { get; } = ContentType.Paths;
    public List<MarkedContent>? Markings { get; set; }
    public bool CompatibilitySection { get; set; }
    public required GfxState GraphicsState { get; set; }
    public required List<SubPath> Paths { get; set; }
    public IPdfOperation Closing { get; set; }

    public PdfRect GetBoundingBox()
    {
        bool triggered = false;
        var xmin = decimal.MaxValue;
        var xmax = decimal.MinValue;
        var ymin = decimal.MaxValue;
        var ymax = decimal.MinValue;
        foreach (var path in Paths)
        {
            var x = path.XPos; var y = path.YPos;
            foreach (var op in path.Operations)
            {
                if (op is IPathPaintingOp pp)
                {
                    var current = pp.GetApproximateBoundingBox(x, y);
                    (x, y) = pp.GetFinishingPoint();
                    xmin = Math.Min(xmin, current.LLx);
                    ymin = Math.Min(ymin, current.LLy);
                    xmax = Math.Max(xmax, current.URx);
                    ymax = Math.Max(ymax, current.URy);
                    triggered = true;
                }
            }
        }

        if (!triggered)
        {
            return new PdfRect { LLx = 0, LLy = 0, URx = 0, URy = 0 };
        }

        return GraphicsState.CTM.GetTransformedBoundingBox(new PdfRect { LLx = xmin, LLy = ymin, URx = xmax, URy = ymax });
    }

    public void Write(ContentWriter writer)
    {
        foreach (var subPath in Paths)
        {
            writer.SubPath(subPath);
        }
        writer.Op(Closing);
    }
}

internal class SubPath
{
    public required decimal XPos { get; set; }
    public required decimal YPos { get; set; }
    public required List<IPdfOperation> Operations { get; set; }
    public bool Closed { get; set; }
}
