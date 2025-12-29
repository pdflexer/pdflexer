using Microsoft.Extensions.Logging;
using PDFiumCore;
using PdfLexer;
using PdfLexer.Content;
using PdfLexer.DOM;
using PdfLexer.Operators;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.CommandLine;
using System.Text;

namespace pdflexer.TestCaseGen;

public class RunImageCmd
{
    public string[] InputPath { get; set; } = null!;
    public string OutputPath { get; set; } = null!;

    public static Command CreateCommand()
    {
        var cmd = new Command("images", "Runs image sampling.")
            {
                new Option<string[]>(new[] {"-i", "--input-path"})
                {
                    IsRequired = true,
                    Description = "Directory containing pdfs to sample."
                },
                new Option<string>(new[] {"-o", "--output-path"})
                {
                    IsRequired = true,
                    Description = "Storage for test artifacts."
                }
            };
        return cmd;
    }
}

internal class ImageSampler
{
    private ILogger<ImageSampler> _logger;

    public ImageSampler(ILogger<ImageSampler> logger)
    {
        _logger = logger;

        fpdfview.FPDF_InitLibrary();
    }

    public void Run(RunImageCmd cmd)
    {
        var hashes = new HashSet<string>();

        foreach (var loc in cmd.InputPath)
        {
            foreach (var pdf in Directory.GetFiles(loc, "*.pdf"))
            {
                var name = Path.GetFileNameWithoutExtension(pdf);


                var ser = new PdfLexer.Serializers.Serializers();
                var types = new List<string>();
                try
                {
                    var doc = PdfDocument.Open(File.ReadAllBytes(pdf));
                    if (doc.Trailer.ContainsKey(PdfName.Encrypt))
                    {
                        // don't support encryption currently
                        continue;
                    }

                    var read = new HashSet<PdfStream>();

                    int i = 0;
                    foreach (var page in doc.Pages)
                    {
                        var imgRdr = new ImageScanner(doc.Context, page);
                        while (imgRdr.Advance())
                        {
                            var rect = imgRdr.GetCurrentSize();
                            if (rect.Width() < 5 || rect.Height() < 5) { continue; }
                            if (!imgRdr.TryGetImage(out var img))
                            {
                                continue;
                            }
                            try
                            {
                                if (read.Contains(img.XObj)) { continue; }
                                read.Add(img.XObj);

                                using var md5 = System.Security.Cryptography.MD5.Create();

                                var ms = new MemoryStream();

                                void WriteDirectly(Stream str, PdfIndirectRef ir)
                                {
                                    var obj = ir.Resolve();
                                    ser.SerializeObject(str, obj, WriteDirectly);
                                }

                                void WriteIfNotNull(IPdfObject? obj)
                                {
                                    if (obj == null) { return; }
                                    ser.SerializeObject(ms, obj, WriteDirectly);
                                    ms.WriteByte((byte)'\n');
                                }
                                var f = img.XObj.NativeObject.Get(PdfName.Filter);

                                WriteIfNotNull(img.XObj.NativeObject.Get(PdfName.BitsPerComponent));
                                WriteIfNotNull(img.XObj.NativeObject.Get(PdfName.Filter));
                                WriteIfNotNull(img.XObj.NativeObject.Get(PdfName.Decode));
                                WriteIfNotNull(img.XObj.NativeObject.Get<PdfDictionary>(PdfName.DecodeParms)?.Get(new PdfName("K")));
                                WriteIfNotNull(img.XObj.NativeObject.Get<PdfDictionary>(PdfName.DecodeParms)?.Get("BlackIs1"));
                                WriteIfNotNull(img.XObj.NativeObject.Get(PdfName.ImageMask));
                                var css = img.XObj.NativeObject.Get(PdfName.ColorSpace)?.Resolve();
                                if (css?.Type == PdfObjectType.ArrayObj)
                                {
                                    var csa = (PdfArray)css;
                                    WriteIfNotNull(csa[0]);
                                    WriteIfNotNull(csa[1]);
                                    // if (csa.Count > 2)
                                    // {
                                    //     WriteIfNotNull(csa[2]);
                                    // }
                                    
                                }
                                else
                                {
                                    WriteIfNotNull(css);
                                }
                                var maskType = img.XObj.NativeObject.Get(PdfName.Mask)?.GetPdfObjType();
                                if (maskType != null)
                                {
                                    ms.Write(Encoding.UTF8.GetBytes(maskType.ToString() ?? ""));
                                }
                                var smask = img.XObj.NativeObject.Get<PdfStream>(PdfName.SMask);
                                if (smask != null)
                                {
                                    WriteIfNotNull(PdfName.SMask);
                                    WriteIfNotNull(smask.Dictionary.Get("Matte"));
                                }

                                var data = ms.ToArray();
                                var fpstr = Encoding.UTF8.GetString(data);

                                var hash = Convert.ToBase64String(md5.ComputeHash(data));
                                if (hashes.Contains(hash))
                                {
                                    continue;
                                }
                                hashes.Add(hash);
                                hash = hash.TrimEnd('=').Replace('/','_').Replace('+','-');
                                var infoFile = Path.Combine(cmd.OutputPath, hash + ".txt");
                                if (File.Exists(infoFile)) { continue; }
                                File.WriteAllText(infoFile, name + '\n' + fpstr);

                                using var od = PdfDocument.Create();
                                var pg = new PdfPage(new PdfDictionary());
                                od.Pages.Add(pg);
                                pg.AddXObj("Im1", img.XObj.Stream);
                                var bx = pg.MediaBox;
                                bx.URx = img.XObj.NativeObject.Get<PdfNumber>(PdfName.Width) ?? 0;
                                bx.URy = img.XObj.NativeObject.Get<PdfNumber>(PdfName.Height) ?? 0;

                                var cs = new MemoryStream();
                                q_Op<double>.WriteLn(cs);
                                cm_Op<double>.WriteLn((double)(bx.URx ?? 0), 0, 0, (double)(bx.URy ?? 0), 0, 0, cs);
                                Do_Op<double>.WriteLn("Im1", cs);
                                Q_Op<double>.WriteLn(cs);
                                var cnt = new PdfStream(new PdfDictionary(), new PdfByteArrayStreamContents(cs.ToArray()));
                                pg.NativeObject[PdfName.Contents] = PdfIndirectRef.Create(cnt);
                                var pdfOut = Path.Combine(cmd.OutputPath, $"{hash}.pdf");
                                using (var fso = File.Create(pdfOut))
                                {
                                    od.SaveTo(fso);
                                }

                                var d = new Scope();
                                var pdoc = fpdfview.FPDF_LoadDocument(pdfOut, null);
                                if (pdoc == null)
                                {
                                    _logger.LogWarning("[{PdfName}] PDFium failure: {Error}", name, fpdfview.FPDF_GetLastError());
                                    continue;
                                }
                                d.Add(() => fpdfview.FPDF_CloseDocument(pdoc));

                                var ppg = fpdfview.FPDF_LoadPage(pdoc, 0);
                                d.Add(() => fpdfview.FPDF_ClosePage(ppg));
                                using var pgSize = new FS_SIZEF_();
                                var result = fpdfview.FPDF_GetPageSizeByIndexF(pdoc, 0, pgSize);
                                if (result == 0)
                                {
                                    _logger.LogWarning("[{PdfName}] PDFium failed to get page size", name);
                                    continue;
                                }

                                var pw = (int)(pgSize.Width);
                                var ph = (int)(pgSize.Height);

                                var bmp = fpdfview.FPDFBitmapCreateEx(
                                    pw,
                                    ph,
                                    (int)FPDFBitmapFormat.BGRA,
                                    IntPtr.Zero,
                                    0);
                                if (bmp == null)
                                {
                                    _logger.LogWarning("[{PdfName}] PDFium failed to create bmp", name);
                                    continue;
                                }
                                d.Add(() => fpdfview.FPDFBitmapDestroy(bmp));

                                fpdfview.FPDF_RenderPageBitmap(bmp, ppg, 0, 0,
                                        pw,
                                        ph,
                                        0,
                                        (int)(RenderFlags.DisableImageAntialiasing)
                                    );


                                var bmpImg = CreateImage(fpdfview.FPDFBitmapGetBuffer(bmp), pw, ph);
                                bmpImg.Save(Path.Combine(cmd.OutputPath, $"{hash}.png"));

                                // using var isa = img.GetImage(doc.Context);
                                // isa.SaveAsPng($"c:\\temp\\imgout\\{Path.GetFileNameWithoutExtension(pdf)}_{i}.png");

                                i++;
                            }
                            catch (Exception)
                            {
                                // dont fail for now
                                // throw;
                            }
                        }
                    }
                }
                catch (NotSupportedException ex)
                {
                    // for compressed object streams
                    if (ex.Message.Contains("encryption"))
                    {
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("[{PdfName}] Failure: {Message}", name, ex.Message);
                }
            }
        }

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