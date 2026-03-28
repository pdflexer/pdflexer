using PdfLexer.DOM;

namespace PdfLexer.Content;

internal sealed class SemanticPageSpace
{
    private readonly double originX;
    private readonly double originY;
    private readonly double width;
    private readonly double height;
    private readonly int rotation;

    public SemanticPageSpace(PdfPage page)
    {
        var crop = page.CropBox;
        originX = (double)decimal.Min(crop.LLx, crop.URx);
        originY = (double)decimal.Min(crop.LLy, crop.URy);
        width = (double)decimal.Abs(crop.URx - crop.LLx);
        height = (double)decimal.Abs(crop.URy - crop.LLy);

        var rotate = (int?)page.Rotate ?? 0;
        rotation = ((rotate % 360) + 360) % 360;
        if (rotation != 0 && rotation != 90 && rotation != 180 && rotation != 270)
        {
            rotation = 0;
        }
    }

    public PdfPoint<double> Normalize(PdfPoint<double> point)
    {
        var localX = point.X - originX;
        var localY = point.Y - originY;

        return rotation switch
        {
            90 => new PdfPoint<double> { X = localY, Y = width - localX },
            180 => new PdfPoint<double> { X = width - localX, Y = height - localY },
            270 => new PdfPoint<double> { X = height - localY, Y = localX },
            _ => new PdfPoint<double> { X = localX, Y = localY }
        };
    }

    public PdfRect<double> Normalize(PdfRect<double> rect)
    {
        var p1 = Normalize(new PdfPoint<double> { X = rect.LLx, Y = rect.LLy });
        var p2 = Normalize(new PdfPoint<double> { X = rect.LLx, Y = rect.URy });
        var p3 = Normalize(new PdfPoint<double> { X = rect.URx, Y = rect.LLy });
        var p4 = Normalize(new PdfPoint<double> { X = rect.URx, Y = rect.URy });

        var minX = Math.Min(Math.Min(p1.X, p2.X), Math.Min(p3.X, p4.X));
        var minY = Math.Min(Math.Min(p1.Y, p2.Y), Math.Min(p3.Y, p4.Y));
        var maxX = Math.Max(Math.Max(p1.X, p2.X), Math.Max(p3.X, p4.X));
        var maxY = Math.Max(Math.Max(p1.Y, p2.Y), Math.Max(p3.Y, p4.Y));

        return new PdfRect<double>
        {
            LLx = minX,
            LLy = minY,
            URx = maxX,
            URy = maxY
        };
    }

    public PdfRect<double> RelativePageBox =>
        rotation == 90 || rotation == 270
            ? new PdfRect<double> { LLx = 0d, LLy = 0d, URx = height, URy = width }
            : new PdfRect<double> { LLx = 0d, LLy = 0d, URx = width, URy = height };
}
