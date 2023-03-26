using PdfLexer.DOM;
using System.Numerics;

namespace PdfLexer.Graphics;



internal enum ClippingResult
{
    NotVisible,
    Partial,
    Visible
}


internal interface IClippingPath
{
    bool IsDefault { get; }
    ClippingResult CalculateSimpleClipping(Vector2 ll, Vector2 ur);
}

internal class RectangularClippingPath : IClippingPath
{
    private Vector2 llc;
    private Vector2 urc;
    public RectangularClippingPath(PdfRectangle rect, bool isDefault)
    {
        llc.X = rect.LLx;
        llc.Y = rect.LLy;
        urc.X = rect.URx;
        urc.Y = rect.URy;
        IsDefault = isDefault;
    }

    public bool IsDefault { get; }

    public ClippingResult CalculateSimpleClipping(Vector2 ll, Vector2 ur)
    {
        Span<float> lefts = stackalloc float[4];
        Span<float> rights = stackalloc float[4];
        lefts[0] = ll.X;
        lefts[1] = ll.Y;
        lefts[2] = llc.Y;
        lefts[3] = llc.X;
        rights[0] = urc.X;
        rights[1] = urc.Y;
        rights[2] = ur.Y;
        rights[3] = ur.X;
        var lv = new Vector<float>(lefts);
        var rv = new Vector<float>(rights);
        if (Vector.GreaterThanOrEqualAny(lv, rv))
        {
            return ClippingResult.NotVisible;
        }

        if (ur.Y > urc.Y || ur.X > urc.X || ll.X < llc.X|| ll.Y < llc.Y)
        {
            return ClippingResult.Partial;
        }
        return ClippingResult.Visible;
    }

    public ClippingResult CalculateSimpleClippingNonVector(Vector2 ll, Vector2 ur)
    {
        if (ll.X > urc.X || ll.Y > urc.Y)
        {
            return ClippingResult.NotVisible;
        }

        if (ur.Y < llc.Y || ur.X < llc.X)
        {
            return ClippingResult.NotVisible;
        }

        if (ur.Y > urc.Y || ur.X > urc.X || ll.X < llc.X || ll.Y < llc.Y)
        {
            return ClippingResult.Partial;
        }
        return ClippingResult.Visible;
    }
}