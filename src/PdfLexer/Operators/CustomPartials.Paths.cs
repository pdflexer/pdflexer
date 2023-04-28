using PdfLexer.Content.Model;

namespace PdfLexer.Operators;

// c l v r re
public partial class c_Op : IPathCreatingOp
{
    public PdfRect GetApproximateBoundingBox(decimal xpos, decimal ypos)
    {
        var xmin = Math.Min(Math.Min(Math.Min(x1, x2), x3), xpos);
        var xmax = Math.Max(Math.Max(Math.Max(x1, x2), x3), xpos);
        var ymin = Math.Min(Math.Min(Math.Min(y1, y2), y3), ypos);
        var ymax = Math.Max(Math.Max(Math.Max(y1, y2), y3), ypos);
        return new PdfRect { LLx = xmin, LLy = ymin, URx = xmax, URy = ymax };
    }

    public (decimal, decimal) GetFinishingPoint() => (x3, y3);
}

public partial class l_Op : IPathCreatingOp
{
    public PdfRect GetApproximateBoundingBox(decimal xpos, decimal ypos)
    {
        var xmin = Math.Min(xpos, x);
        var xmax = Math.Max(xpos, x);
        var ymin = Math.Min(ypos, y);
        var ymax = Math.Max(ypos, y);
        return new PdfRect { LLx = xmin, LLy = ymin, URx = xmax, URy = ymax };
    }

    public (decimal, decimal) GetFinishingPoint() => (x,y);
}

public partial class v_Op : IPathCreatingOp
{
    public PdfRect GetApproximateBoundingBox(decimal xpos, decimal ypos)
    {
        var xmin = Math.Min(Math.Min(x2, x3), xpos);
        var xmax = Math.Max(Math.Max(x2, x3), xpos);
        var ymin = Math.Min(Math.Min(y2, y3), ypos);
        var ymax = Math.Max(Math.Max(y2, y3), ypos);
        return new PdfRect { LLx = xmin, LLy = ymin, URx = xmax, URy = ymax };
    }

    public (decimal, decimal) GetFinishingPoint() => (x3, y3);
}

public partial class y_Op : IPathCreatingOp
{
    public PdfRect GetApproximateBoundingBox(decimal xpos, decimal ypos)
    {
        var xmin = Math.Min(Math.Min(x1, x3), xpos);
        var xmax = Math.Max(Math.Max(x1, x3), xpos);
        var ymin = Math.Min(Math.Min(y1, y3), ypos);
        var ymax = Math.Max(Math.Max(y1, y3), ypos);
        return new PdfRect { LLx = xmin, LLy = ymin, URx = xmax, URy = ymax };
    }

    public (decimal, decimal) GetFinishingPoint() => (x3, y3);
}

public partial class re_Op : IPathCreatingOp
{
    public PdfRect GetApproximateBoundingBox(decimal xpos, decimal ypos)
    {
        var x2 = x + width;
        var y2 = y + height;

        return new PdfRect { LLx = Math.Min(x, x2), LLy = Math.Min(y, y2), URx = Math.Max(x, x2), URy = Math.Max(y, y2) };
    }

    public (decimal, decimal) GetFinishingPoint() => (x, y);
}