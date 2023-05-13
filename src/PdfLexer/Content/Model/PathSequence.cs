using PdfLexer.Writing;
using System.Numerics;

namespace PdfLexer.Content.Model;

internal class PathSequence<T> : ISinglePartCopy<T> where T : struct, IFloatingPoint<T>
{
    public ContentType Type { get; } = ContentType.Paths;
    public List<MarkedContent>? Markings { get; set; }
    public bool CompatibilitySection { get; set; }
    public required GfxState<T> GraphicsState { get; set; }
    public required List<SubPath<T>> Paths { get; set; }
    public IPdfOperation<T>? Closing { get; set; }


    public PdfRect<T> GetBoundingBox()
    {
        bool triggered = false;
        T xmin = default;
        T xmax = default;
        T ymin = default;
        T ymax = default;
        foreach (var path in Paths)
        {
            var x = path.XPos; var y = path.YPos;
            foreach (var op in path.Operations)
            {
                if (op is IPathCreatingOp<T> pp)
                {
                    var current = pp.GetApproximateBoundingBox(x, y);
                    (x, y) = pp.GetFinishingPoint();
                    if (triggered)
                    {
                        xmin = T.Min(xmin, current.LLx);
                        ymin = T.Min(ymin, current.LLy);
                        xmax = T.Max(xmax, current.URx);
                        ymax = T.Max(ymax, current.URy);
                    } else
                    {
                        xmin = current.LLx;
                        ymin = current.LLy;
                        xmax = current.URx;
                        ymax = current.URy;
                    }
                    
                    triggered = true;
                }
            }
        }

        if (!triggered)
        {
            return new PdfRect<T> { LLx = T.Zero, LLy = T.Zero, URx = T.Zero, URy = T.Zero };
        }

        return GraphicsState.CTM.GetTransformedBoundingBox(new PdfRect<T> { LLx = xmin, LLy = ymin, URx = xmax, URy = ymax });
    }

    public void Write(ContentWriter<T> writer)
    {
        foreach (var subPath in Paths)
        {
            writer.SubPath(subPath);
        }
        if (Closing != null)
        {
            writer.Op(Closing);
        } else
        {
            writer.Op(n_Op<T>.Value);
        }
    }


    public ISinglePartCopy<T> Clone()
    {
        return (ISinglePartCopy<T>)this.MemberwiseClone();
    }

    IContentGroup<T>? IContentGroup<T>.CopyArea(PdfRect<T> rect) => ((ISinglePartCopy<T>)this).CopyAreaByClipping(rect);

    (IContentGroup<T>? Inside, IContentGroup<T>? Outside) IContentGroup<T>.Split(PdfRect<T> rect) => ((ISinglePartCopy<T>)this).SplitByClipping(rect);
}

internal class SubPath<T> where T : struct, IFloatingPoint<T>
{
    public required T XPos { get; set; }
    public required T YPos { get; set; }
    public required List<IPathCreatingOp<T>> Operations { get; set; }
    public bool Closed { get; set; }
}
