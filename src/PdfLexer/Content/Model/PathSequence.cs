using PdfLexer.Writing;
using System.Numerics;

namespace PdfLexer.Content.Model;

/// <summary>
/// Vector drawing path from a PDF.
/// 
/// This is a group of path construction operations terminated by a 
/// path drawing operation.
/// </summary>
/// <typeparam name="T"></typeparam>
public class PathSequence<T> : ISinglePartCopy<T> where T : struct, IFloatingPoint<T>
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
                    }
                    else
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
        }
        else
        {
            writer.Op(n_Op<T>.Value);
        }
    }

    public void Transform(GfxMatrix<T> transformation) => ((ISinglePartCopy<T>)this).TransformImpl(transformation);

    public void TransformInitial(GfxMatrix<T> transformation) => ((ISinglePartCopy<T>)this).TransformInitialImpl(transformation);


    ISinglePartCopy<T> ISinglePartCopy<T>.Clone()
    {
        return (ISinglePartCopy<T>)this.MemberwiseClone();
    }

    public void ClipExcept(PdfRect<T> rect) => ((ISinglePartCopy<T>)this).ClipExceptImpl(rect);

    IContentGroup<T>? IContentGroup<T>.CopyArea(PdfRect<T> rect) => ((ISinglePartCopy<T>)this).CopyAreaByClipping(rect);

    (IContentGroup<T>? Inside, IContentGroup<T>? Outside) IContentGroup<T>.Split(PdfRect<T> rect) => ((ISinglePartCopy<T>)this).SplitByClipping(rect);

    public void ClipFrom(GfxState<T> other) => ((ISinglePartCopy<T>)this).ClipFromImpl(other);
}

public class SubPath<T> where T : struct, IFloatingPoint<T>
{
    public required T XPos { get; set; }
    public required T YPos { get; set; }
    public required List<IPathCreatingOp<T>> Operations { get; set; }
    public bool Closed { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj == null) return false;
        if (!(obj is SubPath<T> other))
        {
            return false;
        }
        if (XPos != other.XPos) { return false; }
        if (YPos != other.YPos) { return false; }
        if (Closed != other.Closed) { return false; }
        if (!Operations.SequenceEqual(other.Operations)) { return false; }
        return true;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(XPos);
        hash.Add(YPos);
        hash.Add(Closed);
        foreach (var item in Operations)
        {
            hash.Add(item);
        }
        return hash.ToHashCode();
    }
}
