using System.IO;
using PdfLexer.Tests;
using Xunit;

namespace PdfLexer.ImageTests;

public sealed class ImageAssetFactAttribute : FactAttribute
{
    public ImageAssetFactAttribute()
    {
        if (!Directory.Exists(GetImageAssetRoot()))
        {
            Skip = "Image fixture corpus not available at test/imgs.";
        }
    }

    private static string GetImageAssetRoot()
    {
        var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
        return Path.Combine(tp, "imgs");
    }
}

public sealed class ImageAssetTheoryAttribute : TheoryAttribute
{
    public ImageAssetTheoryAttribute()
    {
        if (!Directory.Exists(GetImageAssetRoot()))
        {
            Skip = "Image fixture corpus not available at test/imgs.";
        }
    }

    private static string GetImageAssetRoot()
    {
        var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
        return Path.Combine(tp, "imgs");
    }
}
