using pdflexer.PdfiumRegressionTester;
using PdfLexer;
using PdfLexer.Content;
using PdfLexer.DOM;
using PdfLexer.Filters;
using PdfLexer.Operators;
using PdfLexer.Serializers;
using System.CommandLine;

StreamWriter? writer = null;

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

var type = new Option<string>(
            name: "--type",
            description: "Type of test to run, merge or rebuild.");

var download = new Option<bool>(
            name: "--download",
            getDefaultValue: () => false,
            description: "Download missing links.");

var rootCommand = new RootCommand("PDF regression testing");
rootCommand.AddOption(pdfRoot);
rootCommand.AddOption(output);
rootCommand.AddOption(type);
rootCommand.AddOption(download);

int returnCode = 0;

rootCommand.SetHandler(async (type, root, prefix, dl) =>
{
    returnCode = await RunBase(type, root, prefix, dl);
}, type, pdfRoot, output, download);

await rootCommand.InvokeAsync(args);

return returnCode;

async Task<int> RunBase(string type, string pdfRoot, string output, bool download)
{
    Directory.CreateDirectory(output);

    using var lo = File.Create(Path.Combine(output, type.ToLower() + ".log"));
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

    switch (type.ToUpper())
    {
        case "MERGE":
            return RunMergeTests(pdfRoot, output) ? 0 : 1;
        case "REBUILD":
            return RunRebuildTests(pdfRoot, output) ? 0 : 1;
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
    if (doc.Pages.Count <= pg) { return; }
    var page = doc.Pages[pg];
    var contents = page.Dictionary.Get(PdfName.Contents).Resolve();
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

bool RunMergeTests(string pdfRoot, string output)
{
    bool success = true;
    var merged = Path.Combine(output, "all.pdf");

    var counts = new List<(string, int, int)>();
    {

        using var fo = File.Create(merged);
        using var sw = new StreamingWriter(fo);

        foreach (var file in Directory.GetFiles(pdfRoot, "*.pdf"))
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
            var modified = Path.Combine(pdfRoot, Path.GetFileName(pdf));
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
                File.Copy(pdf, Path.Combine(pdfRoot, Path.GetFileNameWithoutExtension(pdf) + "_baseline.pdf"), true);
            }
        }
    }
    File.Delete(merged);
    return success;
}

bool RunRebuildTests(string pdfRoot, string output)
{
    bool success = true;
    Directory.CreateDirectory(output);
    // var names = new string[] { "__issue9462.pdf.pdf" };
    // foreach (var pdf in names.Select(n=>Path.Combine(pdfRoot, n)))
    foreach (var pdf in Directory.GetFiles(pdfRoot, "*.pdf"))
    {
        var nm = Path.GetFileName(pdf);
        // if (nm.StartsWith("__")) { continue; }
        var comparer = new Compare(Path.Combine(output, Path.GetFileNameWithoutExtension(pdf)), 2);
        var modified = Path.Combine(output, Path.GetFileName(pdf));
        {
            try
            {
                // using var fs = File.OpenRead(pdf);
                // using var doc = PdfDocument.Open(fs);
                using var doc = PdfDocument.Open(File.ReadAllBytes(pdf));
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
                continue;
            }

        }
        var pgs = comparer.CompareAllPages(pdf, modified);

        bool changes = false;
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
            Log($"[{nm}] failed");
            File.Copy(pdf, Path.Combine(output, Path.GetFileNameWithoutExtension(pdf) + "_baseline.pdf"), true);
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