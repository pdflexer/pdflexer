using PDFiumCore;
using PdfLexer.DOM;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Channels;

namespace PdfLexer.Interactive;

public class RenderPage
{
    public static async Task<string> GetBase64Image(PdfPage page, double ppp=1)
    {
        using var doc = PdfDocument.Create();
        doc.Pages.Add(page);
        var data = doc.Save();
        await using var pd = await PdfiumDoc.Load(data);
        await using var ppg = await PdfiumPage.Create(pd, 0);
        var ms = new MemoryStream();
        await ppg.GetBitmap(pd.Instance, 0, ppp, a => a.SaveAsPng(ms));
        return Convert.ToBase64String(ms.ToArray());
    }
}


internal class PdfiumDoc : IAsyncDisposable
{
    private IntPtr? _ptr;
    private PdfiumActor Actor = PdfiumActor.Actor;

    public FpdfDocumentT Instance { get; private set; }

    internal PdfiumDoc(FpdfDocumentT instance)
    {
        Instance = instance;
        PdfiumActor.EnsureRunning();
    }

    internal PdfiumDoc(FpdfDocumentT instance, IntPtr ptr)
    {
        Instance = instance;
        _ptr = ptr;
        PdfiumActor.EnsureRunning();
    }

    public int GetPageCount()
    {
        return fpdfview.FPDF_GetPageCount(Instance);
    }

    public static async ValueTask<PdfiumDoc> Load(byte[] data)
    {
        PdfiumActor.EnsureRunning();
        var ptr = Marshal.AllocHGlobal(data.Length);
        Marshal.Copy(data, 0, ptr, data.Length);
        var instance = await PdfiumActor.Actor.Run(() => fpdfview.FPDF_LoadMemDocument(ptr, data.Length, null));
        if (instance == null)
        {
            Marshal.FreeHGlobal(ptr);
            var err = await PdfiumActor.Actor.Run(() => fpdfview.FPDF_GetLastError());
            throw new ApplicationException("unable to open document: " + err);
        }
        return new PdfiumDoc(instance, ptr);
    }

    public static async ValueTask<PdfiumDoc> Load(string path)
    {
        PdfiumActor.EnsureRunning();
        var instance = await PdfiumActor.Actor.Run(() => fpdfview.FPDF_LoadDocument(path, null));
        if (instance == null)
        {
            var err = await PdfiumActor.Actor.Run(() => fpdfview.FPDF_GetLastError());
            throw new ApplicationException("unable to open document: " + err);
        }
        return new PdfiumDoc(instance);
    }

    public async ValueTask DisposeAsync()
    {
        await Actor.Run(() => fpdfview.FPDF_CloseDocument(Instance));

        if (_ptr != null && _ptr.HasValue)
        {
            Marshal.FreeHGlobal(_ptr.Value);
        }
    }
}



internal class DeferredAction : IDisposable
{
    private Action _a;

    public DeferredAction(Action a)
    {
        _a = a;
    }

    public void Dispose()
    {
        _a();
    }
}
internal static class Defer
{
    public static IDisposable Action(Action a)
    {
        return new DeferredAction(a);
    }
}
internal class WrappedChannel
{
    public CancellationTokenSource CTS { get; set; } = null!;
    public Channel<Func<CancellationToken, ValueTask>> Queue { get; set; } = null!;
}
internal class PdfiumActor
{
    internal static PdfiumActor Actor = new PdfiumActor();
    internal static void EnsureRunning()
    {
        lock (Actor)
        {
            if (Actor.Loop == null)
            {
                Actor.Start();
                _ = Actor.Run(() => fpdfview.FPDF_InitLibrary());
            }
            else if (Actor?.Work?.IsCompleted ?? true)
            {
                Actor?._channel?.CTS?.Cancel();
                Actor!.Start();
                _ = Actor.Run(() => fpdfview.FPDF_InitLibrary());
            }
        }

    }
    internal WrappedChannel? _channel;

    public PdfiumActor()
    {
    }

    public Task? Work { get; private set; }
    public virtual void Start()
    {
        _channel = new WrappedChannel
        {
            CTS = new CancellationTokenSource(),
            Queue = Channel.CreateBounded<Func<CancellationToken, ValueTask>>(5)
        };
        Work = Loop(_channel.CTS.Token);
    }

    public async ValueTask Run(Func<CancellationToken, ValueTask> work)
    {
        await Query(async (c) => { await work(c); return true; });
    }

    public async ValueTask Run(Action work)
    {
        await Query((c) => { work(); return ValueTask.FromResult(true); });
    }

    public async ValueTask<TResult> Run<TResult>(Func<TResult> work)
    {
        return await Query((c) => { var res = work(); return ValueTask.FromResult(res); });
    }

    public async Task<TResult> Query<TResult>(Func<CancellationToken, ValueTask<TResult>> work)
    {
        if (_channel == null) { throw new ApplicationException("Channel not running"); }
        var tcs = new TaskCompletionSource<TResult>(); // if queries end up being used a lot 
                                                       // probably need to look into something
                                                       // other than TCS for each request
        Func<CancellationToken, ValueTask> run = async (CancellationToken token) =>
        {
            try
            {
                var result = await work(token);
                tcs.SetResult(result);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        };

        await _channel.Queue.Writer.WriteAsync(run);
        if (Work == null) { return await tcs.Task; }
        var result = await Task.WhenAny(tcs.Task, Work);
        if (result == Work) { await Work; throw new ApplicationException("Main work task ended."); }
        return await tcs.Task;
    }

    public Task<TResult> Query<TResult>(Func<ValueTask<TResult>> work)
        => Query<TResult>((t) => work());

    public Task<TResult> Query<TResult>(Func<TResult> work)
        => Query<TResult>((t) => ValueTask.FromResult(work()));

    private async Task Loop(CancellationToken token)
    {
        await foreach (var item in _channel!.Queue.Reader.ReadAllAsync(token))
        {

            try
            {
                token.ThrowIfCancellationRequested();
                await item(token);
            }
            catch (Exception)
            {
                // hmm?
            }
        }
    }
}



internal class PdfiumBitmap : IAsyncDisposable
{
    private readonly PdfiumActor _actor = PdfiumActor.Actor;

    public FpdfBitmapT Instance { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }

    private PdfiumBitmap(FpdfBitmapT instance, int height, int width)
    {
        Instance = instance;
        Width = width;
        Height = height;
    }
    public static async Task<PdfiumBitmap> Create(int width, int height)
    {
        var bm = await PdfiumActor.Actor.Run(() => fpdfview.FPDFBitmapCreateEx(
            width,
            height,
            (int)FPDFBitmapFormat.BGRA,
            IntPtr.Zero,
            0));
        return new PdfiumBitmap(bm, height, width);
    }

    public async Task<Image<Bgra32>> CreateImage()
    {
        var bmPtr = await _actor.Run(() => fpdfview.FPDFBitmapGetBuffer(Instance));
        var stride = await _actor.Run(() => fpdfview.FPDFBitmapGetStride(Instance));
        return CreateImage(bmPtr, stride);
    }
    private unsafe Image<Bgra32> CreateImage(IntPtr ptr, int stride)
    {

        var image = Image.WrapMemory<Bgra32>(
                            ptr.ToPointer(),
                            stride*Height,
                            Width,
                            Height);
        return image;
    }

    public async ValueTask DisposeAsync()
    {
        await _actor.Run(() => fpdfview.FPDFBitmapDestroy(Instance));
    }
}


internal static class ActorExts
{
    public static async Task<PdfiumBitmap> CreateBitmap(this PdfiumActor actor, int width, int height)
    {
        return await PdfiumBitmap.Create(width, height);
    }

    public static async Task<PdfiumPage> LoadPage(this PdfiumActor actor, PdfiumDoc doc, int page)
    {
        return await PdfiumPage.Create(doc, page);
    }
}

internal enum PageObjType
{
    FPDF_PAGEOBJ_UNKNOWN = 0,
    FPDF_PAGEOBJ_TEXT = 1,
    FPDF_PAGEOBJ_PATH = 2,
    FPDF_PAGEOBJ_IMAGE = 3,
    FPDF_PAGEOBJ_SHADING = 4,
    FPDF_PAGEOBJ_FORM = 5
}


internal class PdfiumPage : IAsyncDisposable
{
    private readonly PdfiumActor _actor = PdfiumActor.Actor;

    public FpdfPageT Instance { get; private set; }

    private PdfiumPage(FpdfPageT instance)
    {
        Instance = instance;
    }
    public async ValueTask DisposeAsync()
    {
        await _actor.Run(() => fpdfview.FPDF_ClosePage(Instance));
    }

    public async ValueTask<PdfiumTextPage> GetText()
    {
        var ti = await _actor.Run(() => fpdf_text.FPDFTextLoadPage(Instance));
        return new PdfiumTextPage(ti);
    }

    public async ValueTask<double> GetPageHeight()
    {
        return await _actor.Run(() => fpdfview.FPDF_GetPageHeightF(Instance));
    }

    public async ValueTask<double> GetPageWidth()
    {
        return await _actor.Run(() => fpdfview.FPDF_GetPageWidthF(Instance));
    }

    public async ValueTask GetBitmap(FpdfDocumentT doc, int page, double ppp, Action<Image<Bgra32>> bitmapAction)
    {
        using var size = new FS_SIZEF_();
        var result = await _actor.Run(() => fpdfview.FPDF_GetPageSizeByIndexF(doc, page, size));
        if (result == 0)
        {
            throw new ApplicationException("FPDF FPDF_GetPageSizeByIndexF failed");
        }


        var pwb = (int)(size.Width * ppp);
        var phb = (int)(size.Height * ppp);

        await using var bmb = await _actor.CreateBitmap(pwb, phb);

        await _actor.Run(() =>
            fpdfview.FPDF_RenderPageBitmap(bmb.Instance, Instance, 0, 0,
                pwb,
                phb,
                0,
                (int)(RenderFlags.DisableImageAntialiasing | RenderFlags.RenderForPrinting)
            ));

        var img = await bmb.CreateImage();
        bitmapAction(img);
    }

    public static async Task<PdfiumPage> Create(PdfiumDoc doc, int i)
    {
        var bm = await PdfiumActor.Actor.Run(() => fpdfview.FPDF_LoadPage(
            doc.Instance,
            i));
        return new PdfiumPage(bm);
    }
}

public class ImageLoc
{
    public float Llx { get; set; }
    public float Lly { get; set; }
    public float Urx { get; set; }
    public float Ury { get; set; }
}
public class PdfiumTextPage : IAsyncDisposable
{
    private readonly PdfiumActor _actor = PdfiumActor.Actor;

    public FpdfTextpageT Instance { get; private set; }

    internal PdfiumTextPage(FpdfTextpageT instance)
    {
        Instance = instance;
    }

    public async ValueTask<string> GetChars()
    {
        return await _actor.Run(() =>
        {
            var t = fpdf_text.FPDFTextCountChars(Instance);
            if (t == -1)
            {
                throw new ApplicationException("failed to get char count.");
            }
            unsafe
            {
                Span<byte> txt = new byte[t * 2 + 1];
                fixed (byte* ptrr = &txt[0])
                {
                    fpdf_text.FPDFTextGetText(Instance, 0, t, ref *(ushort*)ptrr);
                }
                return Encoding.Unicode.GetString(txt);
            }
        });
    }

    public async ValueTask<string> GetText(double llx, double lly, double urx, double ury, int maxChars = 250)
    {
        return await _actor.Run(() =>
        {
            unsafe
            {
                Span<byte> txt = new byte[maxChars * 2 + 1];

                int read = 0;
                fixed (byte* ptrr = &txt[0])
                {
                    read = fpdf_text.FPDFTextGetBoundedText(Instance, llx, ury, urx, lly, ref *(ushort*)ptrr, maxChars);
                }
                return Encoding.Unicode.GetString(txt.Slice(0, read * 2 + 1));
            }
        });
    }

    public async ValueTask DisposeAsync()
    {
        await _actor.Run(() => fpdf_text.FPDFTextClosePage(Instance));
    }
}