using PDFiumCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace pdflexer.PdfiumRegressionTester;

public class Compare
{
    private static readonly VisualCompareOptions DefaultVisualOptions = new();
    private readonly int _ppp;
    private readonly string _prefix;

    public Compare(string outputPrefix, int ppp = 2)
    {
        _ppp = ppp;
        _prefix = outputPrefix;
        fpdfview.FPDF_InitLibrary();
    }

    public List<CompareResult> CompareAllPages(string baselinePath, string candidatePath)
    {
        return CompareAllPages(baselinePath, candidatePath, CompareMode.Exact);
    }

    public List<CompareResult> CompareAllPagesVisual(string baselinePath, string candidatePath, VisualCompareOptions? options = null)
    {
        return CompareAllPages(baselinePath, candidatePath, CompareMode.VisualTolerance, options);
    }

    public List<CompareResult> CompareAllPages(string baselinePath, string candidatePath, CompareMode mode, VisualCompareOptions? options = null)
    {
        using var d = new Scope();
        var results = new List<CompareResult>();
        var docB = fpdfview.FPDF_LoadDocument(baselinePath, null);
        if (docB == null)
        {
            results.Add(new CompareResult { HadChanges = true, Type = ChangeType.ErrorBaseline, Error = $"unable to open document ({candidatePath}): " + fpdfview.FPDF_GetLastError() });
            return results;
        }
        
        d.Add(() => fpdfview.FPDF_CloseDocument(docB));
        var totalBase = fpdfview.FPDF_GetPageCount(docB);
        
        var docC = fpdfview.FPDF_LoadDocument(candidatePath, null);
        if (docC == null)
        {
            results.Add(new CompareResult { HadChanges = true, Type = ChangeType.ErrorCandidate, Error = $"unable to open document ({candidatePath}): " + fpdfview.FPDF_GetLastError() });
            return results;
        }
        d.Add(() => fpdfview.FPDF_CloseDocument(docC));
        var totalCand = fpdfview.FPDF_GetPageCount(docC);
        for (var i = 0; i< Math.Max(totalBase, totalCand); i++)
        {
            results.Add(ComparePage(docB, docC, i, i, mode, options));
        }
        return results;
    }

    public CompareResult ComparePage(FpdfDocumentT baseline, FpdfDocumentT candidate, int baselinePageNum, int candidatePageNum)
    {
        return ComparePage(baseline, candidate, baselinePageNum, candidatePageNum, CompareMode.Exact);
    }

    public CompareResult ComparePageVisual(FpdfDocumentT baseline, FpdfDocumentT candidate, int baselinePageNum, int candidatePageNum, VisualCompareOptions? options = null)
    {
        return ComparePage(baseline, candidate, baselinePageNum, candidatePageNum, CompareMode.VisualTolerance, options);
    }

    public CompareResult ComparePage(FpdfDocumentT baseline, FpdfDocumentT candidate, int baselinePageNum, int candidatePageNum, CompareMode mode, VisualCompareOptions? options = null)
    {
        var totalBase = fpdfview.FPDF_GetPageCount(baseline);
        var totalCand = fpdfview.FPDF_GetPageCount(candidate);
        if (baselinePageNum >= totalBase)
        {
            return new CompareResult
            {
                HadChanges = true,
                Type = ChangeType.MissingBaseline
            };
        }
        if (candidatePageNum >= totalCand)
        {
            return new CompareResult
            {
                HadChanges = true,
                Type = ChangeType.MissingCandidate,
            };
        }

        using var d = new Scope();

        var pgb = fpdfview.FPDF_LoadPage(baseline, baselinePageNum);
        d.Add(() => fpdfview.FPDF_ClosePage(pgb));
        var pgc = fpdfview.FPDF_LoadPage(candidate, candidatePageNum);
        d.Add(() => fpdfview.FPDF_ClosePage(pgc));

        using var sizeBase = new FS_SIZEF_();
        using var sizeCand = new FS_SIZEF_();
        var result = fpdfview.FPDF_GetPageSizeByIndexF(baseline, baselinePageNum, sizeBase);
        if (result == 0)
        {
            return new CompareResult { HadChanges = true, Type = ChangeType.ErrorBaseline, Error = "FPDF FPDF_GetPageSizeByIndexF failed for baseline." };
        }
        result = fpdfview.FPDF_GetPageSizeByIndexF(candidate, candidatePageNum, sizeCand);
        if (result == 0)
        {
            return new CompareResult { HadChanges = true, Type = ChangeType.ErrorCandidate, Error = "FPDF FPDF_GetPageSizeByIndexF failed for candidate." };
        }

        var pwb = (int)(sizeBase.Width * _ppp);
        var phb = (int)(sizeBase.Height * _ppp);
        var pwc = (int)(sizeCand.Width * _ppp);
        var phc = (int)(sizeCand.Height * _ppp);

        var bmb = fpdfview.FPDFBitmapCreateEx(
            pwb,
            phb,
            (int)FPDFBitmapFormat.BGRA,
            IntPtr.Zero,
            0);
        if (bmb == null)
        {
            return new CompareResult { HadChanges = true, Type = ChangeType.ErrorBaseline, Error = "FPDFBitmapCreateEx failed for baseline" };
        }

        d.Add(() => fpdfview.FPDFBitmapDestroy(bmb));
        var bmc = fpdfview.FPDFBitmapCreateEx(
            pwc,
            phc,
            (int)FPDFBitmapFormat.BGRA,
            IntPtr.Zero,
            0);
        if (bmc == null)
        {
            return new CompareResult { HadChanges = true, Type = ChangeType.ErrorBaseline, Error = "FPDFBitmapCreateEx failed for candidate" };
        }
        d.Add(() => fpdfview.FPDFBitmapDestroy(bmc));


        fpdfview.FPDF_RenderPageBitmap(bmb, pgb, 0, 0,
                pwb,
                phb,
                0,
                (int)(RenderFlags.DisableImageAntialiasing | RenderFlags.RenderForPrinting)
            );
        fpdfview.FPDF_RenderPageBitmap(bmc, pgc, 0, 0,
                pwc,
                phc,
                0,
                (int)(RenderFlags.DisableImageAntialiasing | RenderFlags.RenderForPrinting)
            );

        var suffix = mode == CompareMode.Exact ? ".png" : "_visual.png";
        var path = $"{_prefix}_b{baselinePageNum}_c{candidatePageNum}{suffix}";
        var matches = mode == CompareMode.Exact
            ? RunCompareExact(bmb, pwb, phb, bmc, pwc, phc, path)
            : RunCompareVisualTolerance(bmb, pwb, phb, bmc, pwc, phc, path, options ?? DefaultVisualOptions);

        if (matches)
        {
            return new CompareResult { HadChanges = false };
        }
        return new CompareResult
        {
            HadChanges = true,
            Type = ChangeType.ChangesFound,
            DiffImage = path
        };
    }
    private static int delta = 2;
    private bool RunCompareExact(FpdfBitmapT bmp, int w1, int h1, FpdfBitmapT bmc, int w2, int h2, string output)
    {
        var imgB = CreateImage(fpdfview.FPDFBitmapGetBuffer(bmp), w1, h1);
        var imgC = CreateImage(fpdfview.FPDFBitmapGetBuffer(bmc), w2, h2);
        var w = Math.Max(w1, w2);
        var h = Math.Max(h1, h2);
        var maskImage = new Image<Bgra32>(w, h);
        var samepix = new Bgra32
        {
            B = 0,
            G = 0,
            R = 0,
            A = 255
        };
        var diffPix = new Bgra32
        {
            B = 255,
            G = 255,
            R = 255,
            A = 255
        };

        bool exact = true;
        for (var x = 0; x < w; x++)
        {
            var bmatch = x < w1;
            var cmatch = x < w2;
            for (var y = 0; y < h; y++)
            {
                var both = bmatch && cmatch && y < h1 && y < h2;
                if (!both)
                {
                    exact = false;
                    maskImage[x, y] = diffPix;
                    continue;
                }
                var a = imgB[x, y];
                var b = imgC[x, y];
                if (a.A == 0 && b.A == 0)
                {
                    maskImage[x, y] = samepix;
                    continue;
                }

                //if (Math.Abs(a.R - b.R) < delta && Math.Abs(a. - b.G) < delta && a.B == b.B && a.A == b.A)
                if (Math.Abs(a.R - b.R) < delta && Math.Abs(a.G - b.G) < delta && Math.Abs(a.B - b.B) < delta && Math.Abs(a.A - b.A) < delta)
                {
                    maskImage[x, y] = samepix;
                    continue;
                }

                exact = false;
                maskImage[x, y] = diffPix;
            }
        }

        if (!exact)
        {
            maskImage.SaveAsPng(output);
            return false;
        }
        return true;
    }

    private bool RunCompareVisualTolerance(FpdfBitmapT bmp, int w1, int h1, FpdfBitmapT bmc, int w2, int h2, string output, VisualCompareOptions options)
    {
        var imgB = CreateImage(fpdfview.FPDFBitmapGetBuffer(bmp), w1, h1);
        var imgC = CreateImage(fpdfview.FPDFBitmapGetBuffer(bmc), w2, h2);
        var w = Math.Max(w1, w2);
        var h = Math.Max(h1, h2);
        var rawDiff = new bool[w, h];
        var significantDiff = new bool[w, h];

        for (var x = 0; x < w; x++)
        {
            for (var y = 0; y < h; y++)
            {
                if (!PixelsDiffer(imgB, w1, h1, imgC, w2, h2, x, y, options))
                {
                    continue;
                }

                if (HasLocalMatch(imgB, w1, h1, imgC, w2, h2, x, y, options))
                {
                    continue;
                }

                rawDiff[x, y] = true;
            }
        }

        var significantPixels = MarkSignificantComponents(rawDiff, significantDiff, w, h, options);
        if (significantPixels < options.MinPageDiffPixels)
        {
            return true;
        }

        SaveVisualDiff(imgB, w1, h1, significantDiff, rawDiff, w, h, output);
        return false;
    }

    private static bool PixelsDiffer(Image<Bgra32> baseline, int w1, int h1, Image<Bgra32> candidate, int w2, int h2, int x, int y, VisualCompareOptions options)
    {
        var inBaseline = x < w1 && y < h1;
        var inCandidate = x < w2 && y < h2;
        if (!inBaseline || !inCandidate)
        {
            return true;
        }

        var a = baseline[x, y];
        var b = candidate[x, y];
        if (Math.Abs(GetLuminance(a) - GetLuminance(b)) > options.LuminanceThreshold)
        {
            return true;
        }

        return Math.Abs(a.A - b.A) > options.AlphaThreshold;
    }

    private static bool HasLocalMatch(Image<Bgra32> baseline, int w1, int h1, Image<Bgra32> candidate, int w2, int h2, int x, int y, VisualCompareOptions options)
    {
        if (x >= w1 || y >= h1)
        {
            return false;
        }

        var baselinePixel = baseline[x, y];
        for (var dx = -options.LocalSearchRadius; dx <= options.LocalSearchRadius; dx++)
        {
            var nx = x + dx;
            if (nx < 0 || nx >= w2)
            {
                continue;
            }

            for (var dy = -options.LocalSearchRadius; dy <= options.LocalSearchRadius; dy++)
            {
                var ny = y + dy;
                if (ny < 0 || ny >= h2)
                {
                    continue;
                }

                var candidatePixel = candidate[nx, ny];
                if (Math.Abs(GetLuminance(baselinePixel) - GetLuminance(candidatePixel)) <= options.LuminanceThreshold &&
                    Math.Abs(baselinePixel.A - candidatePixel.A) <= options.AlphaThreshold)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static int MarkSignificantComponents(bool[,] rawDiff, bool[,] significantDiff, int w, int h, VisualCompareOptions options)
    {
        var visited = new bool[w, h];
        var queue = new Queue<(int X, int Y)>();
        var component = new List<(int X, int Y)>();
        int significantPixels = 0;

        for (var x = 0; x < w; x++)
        {
            for (var y = 0; y < h; y++)
            {
                if (!rawDiff[x, y] || visited[x, y])
                {
                    continue;
                }

                queue.Enqueue((x, y));
                visited[x, y] = true;
                component.Clear();
                var minX = x;
                var maxX = x;
                var minY = y;
                var maxY = y;

                while (queue.Count > 0)
                {
                    var current = queue.Dequeue();
                    component.Add(current);
                    if (current.X < minX) { minX = current.X; }
                    if (current.X > maxX) { maxX = current.X; }
                    if (current.Y < minY) { minY = current.Y; }
                    if (current.Y > maxY) { maxY = current.Y; }

                    for (var dx = -1; dx <= 1; dx++)
                    {
                        for (var dy = -1; dy <= 1; dy++)
                        {
                            if (dx == 0 && dy == 0)
                            {
                                continue;
                            }

                            var nx = current.X + dx;
                            var ny = current.Y + dy;
                            if (nx < 0 || nx >= w || ny < 0 || ny >= h)
                            {
                                continue;
                            }

                            if (!rawDiff[nx, ny] || visited[nx, ny])
                            {
                                continue;
                            }

                            visited[nx, ny] = true;
                            queue.Enqueue((nx, ny));
                        }
                    }
                }

                var area = component.Count;
                var width = maxX - minX + 1;
                var height = maxY - minY + 1;
                var longestSpan = Math.Max(width, height);
                var significant = area >= options.MinComponentArea ||
                    (longestSpan >= options.MinComponentSpan && area >= options.MinLineComponentArea);

                if (!significant)
                {
                    continue;
                }

                significantPixels += area;
                foreach (var pixel in component)
                {
                    significantDiff[pixel.X, pixel.Y] = true;
                }
            }
        }

        return significantPixels;
    }

    private static void SaveVisualDiff(Image<Bgra32> baseline, int w1, int h1, bool[,] significantDiff, bool[,] rawDiff, int w, int h, string output)
    {
        using var diffImage = new Image<Bgra32>(w, h);
        for (var x = 0; x < w; x++)
        {
            for (var y = 0; y < h; y++)
            {
                var basePixel = x < w1 && y < h1
                    ? baseline[x, y]
                    : new Bgra32(255, 255, 255, 255);

                if (significantDiff[x, y])
                {
                    diffImage[x, y] = Blend(basePixel, new Bgra32(40, 40, 255, 255), 0.85f);
                    continue;
                }

                if (rawDiff[x, y])
                {
                    diffImage[x, y] = Blend(basePixel, new Bgra32(0, 200, 255, 255), 0.40f);
                    continue;
                }

                diffImage[x, y] = basePixel;
            }
        }

        diffImage.SaveAsPng(output);
    }

    private static Bgra32 Blend(Bgra32 background, Bgra32 overlay, float amount)
    {
        var inverse = 1f - amount;
        return new Bgra32(
            (byte)(background.B * inverse + overlay.B * amount),
            (byte)(background.G * inverse + overlay.G * amount),
            (byte)(background.R * inverse + overlay.R * amount),
            255);
    }

    private static int GetLuminance(Bgra32 pixel)
    {
        return (pixel.R * 299 + pixel.G * 587 + pixel.B * 114) / 1000;
    }

    private bool RunCompareVisualTolerance(FpdfBitmapT bmp, int w1, int h1, FpdfBitmapT bmc, int w2, int h2, string output, VisualCompareOptions options)
    {
        var imgB = CreateImage(fpdfview.FPDFBitmapGetBuffer(bmp), w1, h1);
        var imgC = CreateImage(fpdfview.FPDFBitmapGetBuffer(bmc), w2, h2);
        var w = Math.Max(w1, w2);
        var h = Math.Max(h1, h2);
        var rawDiff = new bool[w, h];
        var significantDiff = new bool[w, h];

        for (var x = 0; x < w; x++)
        {
            for (var y = 0; y < h; y++)
            {
                if (!PixelsDiffer(imgB, w1, h1, imgC, w2, h2, x, y, options))
                {
                    continue;
                }

                if (HasLocalMatch(imgB, w1, h1, imgC, w2, h2, x, y, options))
                {
                    continue;
                }

                rawDiff[x, y] = true;
            }
        }

        var significantPixels = MarkSignificantComponents(rawDiff, significantDiff, w, h, options);
        if (significantPixels < options.MinPageDiffPixels)
        {
            return true;
        }

        SaveVisualDiff(imgB, w1, h1, significantDiff, rawDiff, w, h, output);
        return false;
    }

    private static bool PixelsDiffer(Image<Bgra32> baseline, int w1, int h1, Image<Bgra32> candidate, int w2, int h2, int x, int y, VisualCompareOptions options)
    {
        var inBaseline = x < w1 && y < h1;
        var inCandidate = x < w2 && y < h2;
        if (!inBaseline || !inCandidate)
        {
            return true;
        }

        var a = baseline[x, y];
        var b = candidate[x, y];
        if (Math.Abs(GetLuminance(a) - GetLuminance(b)) > options.LuminanceThreshold)
        {
            return true;
        }

        return Math.Abs(a.A - b.A) > options.AlphaThreshold;
    }

    private static bool HasLocalMatch(Image<Bgra32> baseline, int w1, int h1, Image<Bgra32> candidate, int w2, int h2, int x, int y, VisualCompareOptions options)
    {
        if (x >= w1 || y >= h1)
        {
            return false;
        }

        var baselinePixel = baseline[x, y];
        for (var dx = -options.LocalSearchRadius; dx <= options.LocalSearchRadius; dx++)
        {
            var nx = x + dx;
            if (nx < 0 || nx >= w2)
            {
                continue;
            }

            for (var dy = -options.LocalSearchRadius; dy <= options.LocalSearchRadius; dy++)
            {
                var ny = y + dy;
                if (ny < 0 || ny >= h2)
                {
                    continue;
                }

                var candidatePixel = candidate[nx, ny];
                if (Math.Abs(GetLuminance(baselinePixel) - GetLuminance(candidatePixel)) <= options.LuminanceThreshold &&
                    Math.Abs(baselinePixel.A - candidatePixel.A) <= options.AlphaThreshold)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static int MarkSignificantComponents(bool[,] rawDiff, bool[,] significantDiff, int w, int h, VisualCompareOptions options)
    {
        var visited = new bool[w, h];
        var queue = new Queue<(int X, int Y)>();
        var component = new List<(int X, int Y)>();
        int significantPixels = 0;

        for (var x = 0; x < w; x++)
        {
            for (var y = 0; y < h; y++)
            {
                if (!rawDiff[x, y] || visited[x, y])
                {
                    continue;
                }

                queue.Enqueue((x, y));
                visited[x, y] = true;
                component.Clear();
                var minX = x;
                var maxX = x;
                var minY = y;
                var maxY = y;

                while (queue.Count > 0)
                {
                    var current = queue.Dequeue();
                    component.Add(current);
                    if (current.X < minX) { minX = current.X; }
                    if (current.X > maxX) { maxX = current.X; }
                    if (current.Y < minY) { minY = current.Y; }
                    if (current.Y > maxY) { maxY = current.Y; }

                    for (var dx = -1; dx <= 1; dx++)
                    {
                        for (var dy = -1; dy <= 1; dy++)
                        {
                            if (dx == 0 && dy == 0)
                            {
                                continue;
                            }

                            var nx = current.X + dx;
                            var ny = current.Y + dy;
                            if (nx < 0 || nx >= w || ny < 0 || ny >= h)
                            {
                                continue;
                            }

                            if (!rawDiff[nx, ny] || visited[nx, ny])
                            {
                                continue;
                            }

                            visited[nx, ny] = true;
                            queue.Enqueue((nx, ny));
                        }
                    }
                }

                var area = component.Count;
                var width = maxX - minX + 1;
                var height = maxY - minY + 1;
                var longestSpan = Math.Max(width, height);
                var significant = area >= options.MinComponentArea ||
                    (longestSpan >= options.MinComponentSpan && area >= options.MinLineComponentArea);

                if (!significant)
                {
                    continue;
                }

                significantPixels += area;
                foreach (var pixel in component)
                {
                    significantDiff[pixel.X, pixel.Y] = true;
                }
            }
        }

        return significantPixels;
    }

    private static void SaveVisualDiff(Image<Bgra32> baseline, int w1, int h1, bool[,] significantDiff, bool[,] rawDiff, int w, int h, string output)
    {
        using var diffImage = new Image<Bgra32>(w, h);
        for (var x = 0; x < w; x++)
        {
            for (var y = 0; y < h; y++)
            {
                var basePixel = x < w1 && y < h1
                    ? baseline[x, y]
                    : new Bgra32(255, 255, 255, 255);

                if (significantDiff[x, y])
                {
                    diffImage[x, y] = Blend(basePixel, new Bgra32(40, 40, 255, 255), 0.85f);
                    continue;
                }

                if (rawDiff[x, y])
                {
                    diffImage[x, y] = Blend(basePixel, new Bgra32(0, 200, 255, 255), 0.40f);
                    continue;
                }

                diffImage[x, y] = basePixel;
            }
        }

        diffImage.SaveAsPng(output);
    }

    private static Bgra32 Blend(Bgra32 background, Bgra32 overlay, float amount)
    {
        var inverse = 1f - amount;
        return new Bgra32(
            (byte)(background.B * inverse + overlay.B * amount),
            (byte)(background.G * inverse + overlay.G * amount),
            (byte)(background.R * inverse + overlay.R * amount),
            255);
    }

    private static int GetLuminance(Bgra32 pixel)
    {
        return (pixel.R * 299 + pixel.G * 587 + pixel.B * 114) / 1000;
    }

    private unsafe Image<Bgra32> CreateImage(IntPtr ptr, int w, int h)
    {
        var image = Image.WrapMemory<Bgra32>(
                            ptr.ToPointer(),
                            w,
                            h);
        return image;
    }
}

public enum CompareMode
{
    Exact,
    VisualTolerance
}

public sealed class VisualCompareOptions
{
    public int LuminanceThreshold { get; init; } = 12;
    public int AlphaThreshold { get; init; } = 12;
    public int LocalSearchRadius { get; init; } = 1;
    public int MinComponentArea { get; init; } = 16;
    public int MinLineComponentArea { get; init; } = 8;
    public int MinComponentSpan { get; init; } = 12;
    public int MinPageDiffPixels { get; init; } = 24;
}


public class CompareResult
{
    public bool HadChanges { get; internal set; }
    public ChangeType Type { get; internal set; }
    public string? DiffImage { get; internal set; }
    public string? Error { get; set; }
}

public enum ChangeType
{
    NoChanges,
    ErrorBaseline,
    ErrorCandidate,
    MissingBaseline,
    MissingCandidate,
    ChangesFound
}


public class Scope : IDisposable
{
    private List<Action> _actions;

    public Scope()
    {
        _actions = new List<Action>();
    }
    public void Add(Action a) { _actions.Add(a); }

    public void Dispose()
    {
        _actions?.ForEach(a =>
        {
            try { a(); } catch { }
        });
    }
}
