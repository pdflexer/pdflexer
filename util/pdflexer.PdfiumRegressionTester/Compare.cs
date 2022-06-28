using PDFiumCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace pdflexer.PdfiumRegressionTester;

internal class Compare
{
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
        if (docB == null)
        {
            results.Add(new CompareResult { HadChanges = true, Type = ChangeType.ErrorCandidate, Error = $"unable to open document ({candidatePath}): " + fpdfview.FPDF_GetLastError() });
            return results;
        }
        d.Add(() => fpdfview.FPDF_CloseDocument(docC));
        var totalCand = fpdfview.FPDF_GetPageCount(docC);
        for (var i = 0; i< Math.Max(totalBase, totalCand); i++)
        {
            results.Add(ComparePage(docB, docC, i, i));
        }
        return results;
    }

    public CompareResult ComparePage(FpdfDocumentT baseline, FpdfDocumentT candidate, int baselinePageNum, int candidatePageNum)
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

        var path = $"{_prefix}_b{baselinePageNum}_c{candidatePageNum}.png";
        var exact = RunCompare(bmb, pwb, phb, bmc, pwc, phc, path);
        if (exact)
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

    private bool RunCompare(FpdfBitmapT bmp, int w1, int h1, FpdfBitmapT bmc, int w2, int h2, string output)
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

                if (a.R == b.R && a.G == b.G && a.B == b.B && a.A == b.A)
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

    private unsafe Image<Bgra32> CreateImage(IntPtr ptr, int w, int h)
    {
        var image = Image.WrapMemory<Bgra32>(
                            ptr.ToPointer(),
                            w,
                            h);
        return image;
    }
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

public class D : IDisposable
{
    private Action _a;

    public D(Action a)
    {
        _a = a;
    }
    public void Dispose()
    {
        _a();
    }
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