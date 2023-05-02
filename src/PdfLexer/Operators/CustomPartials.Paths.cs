using PdfLexer.Content;
using System.Numerics;

namespace PdfLexer.Operators;
// c l v r re
public partial class c_Op<T> : IPathCreatingOp<T> where T : struct, IFloatingPoint<T>
{
    public PdfRect<T> GetApproximateBoundingBox(T xpos, T ypos)
    {
        var xmin = T.Min(T.Min(T.Min(x1, x2), x3), xpos);
        var xmax = T.Max(T.Max(T.Max(x1, x2), x3), xpos);
        var ymin = T.Min(T.Min(T.Min(y1, y2), y3), ypos);
        var ymax = T.Max(T.Max(T.Max(y1, y2), y3), ypos);
        return new PdfRect<T> { LLx = xmin, LLy = ymin, URx = xmax, URy = ymax };
    }

    public (T, T) GetFinishingPoint() => (x3, y3);
}

public partial class l_Op<T> : IPathCreatingOp<T> where T : struct, IFloatingPoint<T>
{
    public PdfRect<T> GetApproximateBoundingBox(T xpos, T ypos)
    {
        var xmin = T.Min(xpos, x);
        var xmax = T.Max(xpos, x);
        var ymin = T.Min(ypos, y);
        var ymax = T.Max(ypos, y);
        return new PdfRect<T> { LLx = xmin, LLy = ymin, URx = xmax, URy = ymax };
    }

    public (T, T) GetFinishingPoint() => (x,y);
}

public partial class v_Op<T> : IPathCreatingOp<T> where T : struct, IFloatingPoint<T>
{
    public PdfRect<T> GetApproximateBoundingBox(T xpos, T ypos)
    {
        var xmin = T.Min(T.Min(x2, x3), xpos);
        var xmax = T.Max(T.Max(x2, x3), xpos);
        var ymin = T.Min(T.Min(y2, y3), ypos);
        var ymax = T.Max(T.Max(y2, y3), ypos);
        return new PdfRect<T> { LLx = xmin, LLy = ymin, URx = xmax, URy = ymax };
    }

    public (T, T) GetFinishingPoint() => (x3, y3);
}

public partial class y_Op<T> : IPathCreatingOp<T> where T : struct, IFloatingPoint<T>
{
    public PdfRect<T> GetApproximateBoundingBox(T xpos, T ypos)
    {
        var xmin = T.Min(T.Min(x1, x3), xpos);
        var xmax = T.Max(T.Max(x1, x3), xpos);
        var ymin = T.Min(T.Min(y1, y3), ypos);
        var ymax = T.Max(T.Max(y1, y3), ypos);
        return new PdfRect<T> { LLx = xmin, LLy = ymin, URx = xmax, URy = ymax };
    }

    public (T, T) GetFinishingPoint() => (x3, y3);
}

public partial class re_Op<T> : IPathCreatingOp<T> where T : struct, IFloatingPoint<T>
{
    public PdfRect<T> GetApproximateBoundingBox(T xpos, T ypos)
    {
        var x2 = x + width;
        var y2 = y + height;

        return new PdfRect<T> { LLx = T.Min(x, x2), LLy = T.Min(y, y2), URx = T.Max(x, x2), URy = T.Max(y, y2) };
    }

    public (T, T) GetFinishingPoint() => (x, y);
}
