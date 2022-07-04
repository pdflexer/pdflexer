using pdflexer.PdfiumRegressionTester;
using PdfLexer;
using PdfLexer.Content;
using PdfLexer.DOM;
using PdfLexer.Filters;
using PdfLexer.Operators;
using PdfLexer.Serializers;
using System.CommandLine;
using System.Text.Json;

var corrupt = new Dictionary<string, ErrInfo>();
var errs = Path.Combine(ExePath(), "known-errors.jsonl");
if (File.Exists(errs))
{
    foreach (var line in File.ReadLines(errs))
    {
        if (string.IsNullOrEmpty(line)) { continue; }
        var info = JsonSerializer.Deserialize<ErrInfo>(line);
        corrupt[info.PdfName] = info;
    }
    Console.WriteLine("Loaded error info.");
}


StreamWriter? writer = null;
StreamWriter? errInfo = null;

void Log(string message)
{
    writer?.WriteLine(message);
    Console.WriteLine(message);
}

var output = new Option<string>(
            name: "--output",
            description: "Path where results should be stored.");

var pdfRoot = new Option<string>(
            name: "--pdfs",
            description: "Path where pdfs to run are located.");

var pdfPaths = new Option<string[]>(
            name: "--pdf",
            description: "Path for individual pdfs to run.");

var type = new Option<string>(
            name: "--type",
            description: "Type of test to run, merge or rebuild.");

var download = new Option<bool>(
            name: "--download",
            getDefaultValue: () => false,
            description: "Download missing links.");

var strict = new Option<bool>(
            name: "--strict",
            getDefaultValue: () => false,
            description: "Error on any issue found.");

var rootCommand = new RootCommand("PDF regression testing");
rootCommand.AddOption(pdfRoot);
rootCommand.AddOption(pdfPaths);
rootCommand.AddOption(output);
rootCommand.AddOption(type);
rootCommand.AddOption(download);
rootCommand.AddOption(strict);

int returnCode = 0;

rootCommand.SetHandler(async (type, root, pdfPaths, prefix, dl, strict) =>
{
    returnCode = await RunBase(type, root, pdfPaths, prefix, dl, strict);
}, type, pdfRoot, pdfPaths, output, download, strict);

await rootCommand.InvokeAsync(args);

return returnCode;

async Task<int> RunBase(string type, string pdfRoot, string[] pdfPaths, string output, bool download, bool strict)
{
    Directory.CreateDirectory(output);

    var i = 0;
    FileStream lo = null;
    FileStream er = null;
    while (i < 10 && lo == null)
    {
        try
        {
            lo = File.OpenWrite(Path.Combine(output, type.ToLower() + i + ".log"));
            lo.Seek(0, SeekOrigin.End);
            er = File.OpenWrite(Path.Combine(output, type.ToLower() + i + ".err.jsonl"));
            er.Seek(0, SeekOrigin.End);
        }
        catch (Exception)
        {
            i++;
        }
    }

    if (lo == null || er == null)
    {
        throw new ApplicationException("Unable to create log file");
    }

    using var er_ = er;
    errInfo = new StreamWriter(er);
    using var eiw = errInfo;
    using var lo_ = lo;
    writer = new StreamWriter(lo);
    using var _ = writer;

    if (download)
    {
        var client = new HttpClient();
        foreach (var link in Directory.GetFiles(pdfRoot, "*.link"))
        {
            var pdf = Path.ChangeExtension(link, ".pdf");
            pdf = Path.Combine(pdfRoot, "__" + Path.GetFileName(pdf));
            if (File.Exists(pdf))
            {
                continue;
            }

            var url = File.ReadAllText(link).Trim();
            if (url.ToLower().StartsWith("https://web.archive.org/"))
            {
                var segs = url.Split("/http://");
                if (segs.Length == 2)
                {
                    url = segs[0] + "id_/http://" + segs[1];
                }
                else
                {
                    segs = url.Split("/https://");
                    if (segs.Length == 2)
                    {
                        url = segs[0] + "id_/https://" + segs[1];
                    }
                }

            }
            try
            {
                using var t = await client.GetStreamAsync(url);
                using var fo = File.Create(pdf);
                await t.CopyToAsync(fo);
            }
            catch (Exception e)
            {
                Log($"[DLFailed] {Path.GetFileName(link)} ({url}): {e.Message}");
            }
        }
    }


    if (pdfPaths == null || pdfPaths.Length == 0)
    {
        pdfPaths = Directory.GetFiles(pdfRoot, "*.pdf");
    }


    switch (type.ToUpper())
    {
        case "MERGE":
            return RunMergeTests(pdfPaths, output) ? 0 : 1;
        case "REBUILD":
            return RunRebuildTests(pdfPaths, output, strict) ? 0 : 1;
        default:
            Console.WriteLine("Unknown test type: " + type);
            return 1;
    }
}

void DumpPageContent(PdfDocument doc, int pg, Stream output)
{
    if (doc.Pages.Count <= pg) { return; }
    var scanner = new PageContentScanner(doc.Context, doc.Pages[pg]);
    while (scanner.Peek() != PdfOperatorType.EOC)
    {
        output.Write(scanner.GetCurrentData());
        output.WriteByte((byte)'\n');
        scanner.SkipCurrent();
    }
}

void DumpRawPageContent(PdfDocument doc, int pg, Stream output)
{
    if (doc.Pages == null || doc.Pages.Count <= pg) { return; }
    var page = doc.Pages[pg];
    var contents = page.Dictionary.Get(PdfName.Contents)?.Resolve();
    switch (contents)
    {
        case PdfArray arr:
            foreach (var item in arr)
            {
                var str = item.GetValue<PdfStream>(false);
                if (str != null)
                {
                    using var wo = str.Contents.GetDecodedStream(doc.Context);
                    wo.CopyTo(output);
                    output.WriteByte((byte)'\n');
                }
            }
            break;
        case PdfStream stream:
            {
                using var wo = stream.Contents.GetDecodedStream(doc.Context);
                wo.CopyTo(output);
                output.WriteByte((byte)'\n');
            }
            break;
    }
}

bool RunMergeTests(string[] pdfs, string output)
{
    bool success = true;
    var merged = Path.Combine(output, "all.pdf");

    var counts = new List<(string, int, int)>();
    {
        using var fo = File.Create(merged);
        using var sw = new StreamingWriter(fo);

        foreach (var file in pdfs)
        {
            using var fr = File.OpenRead(file);
            using var doc = PdfDocument.Open(fr);
            if (doc.Trailer.Get(PdfName.Encrypt) != null)
            {
                continue;
            }
            var start = sw.PageCount;
            foreach (var pg in doc.Pages)
            {
                sw.AddPage(pg);
            }
            counts.Add((file, start, sw.PageCount));
        }
        sw.Complete(new PdfDictionary());
    }

    {
        using var allfs = File.OpenRead(merged);
        using var all = PdfDocument.Open(allfs);
        foreach (var (pdf, start, end) in counts)
        {
            var modified = Path.Combine(output, Path.GetFileName(pdf));
            {
                using var doc = PdfDocument.Create();
                for (var i = start; i < end; i++)
                {
                    doc.Pages.Add(all.Pages[i]);
                }
                using var fo = File.Create(modified);
                doc.SaveTo(fo);
            }

            var comparer = new Compare(Path.Combine(output, Path.GetFileNameWithoutExtension(pdf)), 2);
            var pgs = comparer.CompareAllPages(pdf, modified);
            var nm = Path.GetFileName(pdf);
            bool changes = false;
            for (var i = 0; i < pgs.Count; i++)
            {
                var pg = pgs[i];
                if (pg.HadChanges)
                {
                    changes = true;
                    Log($"[{nm}] failed pg {i} -> {pg.Type.ToString()}");
                }
            }
            if (!changes)
            {
                success = false;
                File.Delete(modified);
                Log($"[{nm}] passed");
            }
            else
            {
                Log($"[{nm}] failed");
                File.Copy(pdf, Path.Combine(output, Path.GetFileNameWithoutExtension(pdf) + "_baseline.pdf"), true);
            }
        }
    }
    File.Delete(merged);
    return success;
}

bool RunRebuildTests(string[] pdfs, string output, bool strict)
{
    bool success = true;
    Directory.CreateDirectory(output);
    foreach (var pdf in pdfs)
    {
        var nm = Path.GetFileName(pdf);
        var errorOutput = new ErrInfo
        {
            PdfName = nm
        };
        corrupt.TryGetValue(nm, out var errorInfo);
        var comparer = new Compare(Path.Combine(output, Path.GetFileNameWithoutExtension(pdf)), 2);
        var modified = Path.Combine(output, Path.GetFileName(pdf));
        {
            try
            {
                var opts = new ParsingOptions { MaxErrorRetention = 10 };
                opts.ThrowOnErrors = strict && errorInfo?.ErrCount == 0;
                using var fs = File.OpenRead(pdf);
                using var doc = PdfDocument.Open(fs, opts);
                // using var doc = PdfDocument.Open(File.ReadAllBytes(pdf), opts);
                // for non compressed object strams
                if (doc.Trailer.Get(PdfName.Encrypt) != null)
                {
                    continue;
                }
                using var fo = File.Create(modified);
                var sw = new StreamingWriter(fo, true, true);
                foreach (var pg in doc.Pages)
                {
                    var np = ReWriteStream(doc, pg);
                    sw.AddPage(np);
                }
                sw.Complete(doc.Trailer.CloneShallow(), doc.Catalog.CloneShallow());

                if (doc.Context.ErrorCount > 0)
                {
                    Log($"[{nm}] parsing errors");
                    foreach (var err in doc.Context.ParsingErrors)
                    {
                        Log(err);
                    }
                    Log("err total ->" + doc.Context.ErrorCount);
                    errorOutput.ErrCount = doc.Context.ErrorCount;
                    errorOutput.Errs = doc.Context.ParsingErrors.ToList();

                    if (strict)
                    {
                        // TODO compare actual errors
                        if (errorInfo == null)
                        {
                            Log($"[{nm}] pdflexer had unknown errors in strict mode but no failure.");
                            success = false;
                            continue;
                        } else if (errorInfo.ErrCount != doc.Context.ErrorCount)
                        {
                            Log($"[{nm}] pdflexer mismatched errors in strict mode {errorInfo.ErrCount} vs {doc.Context.ErrorCount}.");
                            success = false;
                        } else
                        {
                            Log($"[{nm}] pdflexer error count matched.");
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
                Log($"[{nm}] pdflexer failure: {ex.Message}");
            }
            catch (Exception ex)
            {
                Log($"[{nm}] pdflexer failure: {ex.Message}");
                errorOutput.Failure = true;
                errorOutput.FailureMsg = ex.Message;
                if (strict)
                {
                    if (errorInfo?.Failure ?? false && errorInfo?.FailureMsg == ex.Message) 
                    {
                        Log($"[{nm}] pdflexer failure matched existing.");
                    } else
                    {
                        Log($"[{nm}] pdflexer failure not known.");
                        success = false;
                    }
                }
                
                continue;
            }

        }
        var pgs = comparer.CompareAllPages(pdf, modified);

        bool changes = false;
        var changedpages = new List<int>();
        for (var i = 0; i < pgs.Count; i++)
        {
            var pg = pgs[i];
            if (pg.HadChanges)
            {
                if (pg.Type == ChangeType.ErrorBaseline)
                {
                    Log($"[{nm}] pdfium failed to open pg on baseline{i}");
                }
                else
                {
                    changes = true;
                    changedpages.Add(i);
                    Log($"[{nm}] failed pg {i} -> {pg.Type.ToString()}");

                    var bn = Path.GetFileNameWithoutExtension(pdf);
                    using var fco = File.Create(Path.Combine(output, $"{bn}_c{i}.txt"));
                    using var fc = PdfDocument.Open(File.ReadAllBytes(modified));
                    DumpPageContent(fc, i, fco);
                    using var fbo = File.Create(Path.Combine(output, $"{bn}_b{i}.txt"));
                    using var fb = PdfDocument.Open(File.ReadAllBytes(pdf));
                    DumpPageContent(fb, i, fbo);
                    using var fbr = File.Create(Path.Combine(output, $"{bn}_b{i}_raw.txt"));
                    DumpRawPageContent(fb, i, fbr);
                }
            }
        }
        if (!changes)
        {
            File.Delete(modified);
            Log($"[{nm}] passed");
        }
        else
        {
            errorOutput.FailedPages = changedpages;
            if (strict)
            {
                if (errorInfo?.FailedPages != null && errorInfo.FailedPages.SequenceEqual(changedpages))
                {
                    Log($"[{nm}] failed paged match previous run.");
                } else
                {
                    Log($"[{nm}] failed");
                    success = false;
                }
            }
            
            File.Copy(pdf, Path.Combine(output, Path.GetFileNameWithoutExtension(pdf) + "_baseline.pdf"), true);
        }
        writer.Flush();
        if (errorOutput.FailedPages != null || errorOutput.ErrCount > 0 || errorOutput.Failure)
        {
            errInfo.WriteLine(JsonSerializer.Serialize(errorOutput));
            errInfo.Flush();
        }
    }
    return success;
}

static PdfPage ReWriteStream(PdfDocument doc, PdfPage page)
{
    var scanner = new PageContentScanner(doc.Context, page);
    var ms = new MemoryStream();

    while (scanner.Peek() != PdfOperatorType.EOC)
    {
        if (scanner.TryGetCurrentOperation(out var op))
        {
            op.Serialize(ms);
            ms.WriteByte((byte)'\n');
        }
        scanner.SkipCurrent();
    }

    page = page.Dictionary.CloneShallow();
    var flate = new FlateFilter(doc.Context);
    ms.Seek(0, SeekOrigin.Begin);
    var df = flate.Encode(ms);
    var ms2 = new MemoryStream((int)df.Data.Length);
    df.Data.CopyTo(ms2);
    var bac = new PdfByteArrayStreamContents(ms2.ToArray(), df.Filter, df.Params);

    var updatedStr = new PdfStream(new PdfDictionary(), bac);
    page.Dictionary[PdfName.Contents] = PdfIndirectRef.Create(updatedStr);
    return page;
}

static string ExePath()
{
    var directory = Path.GetDirectoryName(
        System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
    if (directory?.StartsWith("file:\\", StringComparison.InvariantCultureIgnoreCase) ?? false)
    {
        directory = directory.Substring(6); // windows
    }
    else if (directory?.StartsWith("file:", StringComparison.InvariantCultureIgnoreCase) ?? false)
    {
        directory = directory.Substring(5); // linux
    }

    return directory;
}

public class ErrInfo
{
    public string PdfName { get; set; }
    public List<string> Errs { get; set; }
    public int ErrCount { get; set; }
    public List<int> FailedPages { get; set; }
    public bool Failure { get; set; }
    public string FailureMsg { get; set; }
}

