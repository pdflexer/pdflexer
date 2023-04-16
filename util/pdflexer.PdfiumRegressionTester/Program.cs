using Microsoft.Extensions.Logging.Abstractions;
using pdflexer.PdfiumRegressionTester;
using pdflexer.PdfiumRegressionTester.Tests;
using PdfLexer;
using PdfLexer.Content;
using PdfLexer.DOM;
using PdfLexer.Filters;
using PdfLexer.Operators;
using PdfLexer.Serializers;
using System.CommandLine;
using System.Text.Json;


var data = new Option<string>(
            name: "--data",
            description: "Path where previous results re.");

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

var index = new Option<string>(
            name: "--index",
            description: "Index to use for results.");

var download = new Option<bool>(
            name: "--download",
            getDefaultValue: () => false,
            description: "Download missing links.");

var strict = new Option<bool>(
            name: "--strict",
            getDefaultValue: () => false,
            description: "Error on any issue found.");

var rootCommand = new RootCommand("PDF regression testing");
rootCommand.AddOption(data);
rootCommand.AddOption(pdfRoot);
rootCommand.AddOption(pdfPaths);
rootCommand.AddOption(output);
rootCommand.AddOption(index);
rootCommand.AddOption(type);
rootCommand.AddOption(download);
rootCommand.AddOption(strict);

int returnCode = 0;

rootCommand.SetHandler(async (data, type, root, pdfPaths, prefix, index, dl, strict) =>
{
    returnCode = await RunBase(data, type, root, pdfPaths, prefix, index, dl, strict);
}, data, type, pdfRoot, pdfPaths, output, index, download, strict);

await rootCommand.InvokeAsync(args);

return returnCode;

async Task<int> RunBase(string data, string type, string pdfRoot, string[] pdfPaths, string output, string index, bool download, bool strict)
{
    var corrupt = new Dictionary<string, ErrInfo>();
    var errs = Path.Combine(data, type + ".pdf-info.jsonl");
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

    Directory.CreateDirectory(output);

    using var lo = File.OpenWrite(Path.Combine(output, type.ToLower() + index + ".log"));
    lo.Seek(0, SeekOrigin.End);
    using var er = File.OpenWrite(Path.Combine(output, type.ToLower() + index + ".err.jsonl"));
    er.Seek(0, SeekOrigin.End);
    using var re = File.OpenWrite(Path.Combine(output, type.ToLower() + index + ".res.jsonl"));
    re.Seek(0, SeekOrigin.End);

    using var errInfo = new StreamWriter(er);
    using var writer = new StreamWriter(lo);
    using var summary = new StreamWriter(re);


    if (download)
    {
        var client = new HttpClient();
        foreach (var link in Directory.GetFiles(pdfRoot, "*.link"))
        {
            var pdf = Path.Combine(pdfRoot, "__" + Path.GetFileNameWithoutExtension(link));
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
                Console.WriteLine($"[DLFailed] {Path.GetFileName(link)} ({url}): {e.Message}");
            }
        }
    }


    if (pdfPaths == null || pdfPaths.Length == 0)
    {
        pdfPaths = Directory.GetFiles(pdfRoot, "*.pdf");
    }

    var runner = new TestRunner(corrupt);
    switch (type.ToUpper())
    {
        case "TEXT":
            var tester = new TextTests(NullLogger.Instance);
            bool success = true;
            foreach (var file in pdfPaths)
            {
                try
                {
                    if (tester.RunOne(file, output) != true)
                    {
                        success = false;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failure: " + e.Message);
                    success = false;
                }

            }
            return success ? 0 : 1;
        case "MERGE":
            return RunMergeTests(pdfPaths, output) ? 0 : 1;
        case "QUICKSAVE":
            {
                var rb = new QuickSave();
                foreach (var file in pdfPaths)
                {
                    var result = runner.RunTest(rb, file, output);
                    writer.WriteLine($"[{Path.GetFileName(file)}] {result.Status} {result.Message}");
                    errInfo.WriteLine(JsonSerializer.Serialize(result.Info));
                    summary.WriteLine(JsonSerializer.Serialize(new { Result = result.Status.ToString(), PdfName = Path.GetFileName(file), result.Message }));
                }
                return 0;
            }
        case "REBUILD":
            {
                var rb = new Rebuild();
                foreach (var file in pdfPaths) 
                {
                    var result = runner.RunTest(rb, file, output);
                    writer.WriteLine($"[{Path.GetFileName(file)}] {result.Status} {result.Message}");
                    errInfo.WriteLine(JsonSerializer.Serialize(result.Info));
                    summary.WriteLine(JsonSerializer.Serialize(new { Result = result.Status.ToString(), PdfName = Path.GetFileName(file), result.Message }));
                }
                return 0;
            }
        case "MODEL-REBUILD":
            {
                var rb = new ModelRebuild();
                foreach (var file in pdfPaths)
                {
                    if (Path.GetFileName(file) == "__pdf.pdf") { continue; }
                    { 
                        try
                        {
                            using var pc = PdfDocument.Open(File.ReadAllBytes(file));
                            if (pc.Pages.Count > 10) { continue; }
                        } catch (Exception) { }
                        
                    }
                    var result = runner.RunTest(rb, file, output);
                    writer.WriteLine($"[{Path.GetFileName(file)}] {result.Status} {result.Message}");
                    errInfo.WriteLine(JsonSerializer.Serialize(result.Info));
                    summary.WriteLine(JsonSerializer.Serialize(new { Result = result.Status.ToString(), PdfName = Path.GetFileName(file), result.Message }));
                }
                return 0;
            }
        default:
            Console.WriteLine("Unknown test type: " + type);
            return 1;
    }
}


bool RunMergeTests(string[] pdfs, string output)
{
    bool success = true;
    var merged = Path.Combine(output, "all.pdf");

    void Log(string msg)
    {
        // todo rework this test to new format
    }

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

static PdfPage FlattenStream(PdfDocument doc, PdfPage page)
{
    var res = page.NativeObject.GetOptionalValue<PdfDictionary>(PdfName.Resources);
    var fonts = res?.GetOptionalValue<PdfDictionary>(PdfName.Font);
    fonts = new();
    var xobjs = res?.GetOptionalValue<PdfDictionary>(PdfName.XObject);
    xobjs = new();

    var oc = 0;

    var scanner = new PageContentScanner(doc.Context, page, true);
    var ms = new MemoryStream();

    PdfDictionary currentForm = null;
    var fontReplacements = new Dictionary<PdfName, PdfName>();
    var xObjReplacements = new Dictionary<PdfName, PdfName>();
    var gsReplacements = new Dictionary<PdfName, PdfName>();

    while (scanner.Peek() != PdfOperatorType.EOC)
    {
        if (scanner.TryGetCurrentOperation(out var op))
        {
            if (scanner.CurrentForm == null)
            {

                op.Serialize(ms);
                ms.WriteByte((byte)'\n');
            }
            else
            {
                if (!Object.ReferenceEquals(currentForm, scanner.CurrentForm))
                {
                    fontReplacements = new();
                    xObjReplacements = new();
                    gsReplacements = new();
                    currentForm = scanner.CurrentForm;
                }
                switch (op.Type)
                {
                    // font
                    case PdfOperatorType.Tf:
                        {
                            var orig = (Tf_Op)op;

                            var rn = GetReplacedName(
                                orig.font,
                                fonts,
                                currentForm.GetOptionalValue<PdfDictionary>(PdfName.Resources)?.GetOptionalValue<PdfDictionary>(PdfName.Font),
                                fontReplacements);

                            var co = new Tf_Op(rn, orig.size);

                            co.Serialize(ms);
                            ms.WriteByte((byte)'\n');
                            break;
                        }
                    // gs
                    case PdfOperatorType.gs:
                        op.Serialize(ms);
                        ms.WriteByte((byte)'\n');
                        break;
                    // color state
                    case PdfOperatorType.CS:
                        op.Serialize(ms);
                        ms.WriteByte((byte)'\n');
                        break;
                    // color state
                    case PdfOperatorType.cs:
                        op.Serialize(ms);
                        ms.WriteByte((byte)'\n');
                        break;
                    // do
                    case PdfOperatorType.Do:
                        {
                            var orig = (Do_Op)op;


                            var rn = GetReplacedName(
                                orig.name,
                                xobjs,
                                currentForm.GetOptionalValue<PdfDictionary>(PdfName.Resources)?.GetOptionalValue<PdfDictionary>(PdfName.XObject),
                                xObjReplacements);

                            var co = new Do_Op(rn);

                            co.Serialize(ms);
                            ms.WriteByte((byte)'\n');
                            break;
                        }
                    default:
                        op.Serialize(ms);
                        ms.WriteByte((byte)'\n');
                        break;
                }
            }


        }
        scanner.SkipCurrent();
    }

    page = page.NativeObject.CloneShallow();
    var fl = new ZLibLexerStream();
    ms.Seek(0, SeekOrigin.Begin);
    ms.CopyTo(fl);

    var updatedStr = new PdfStream(new PdfDictionary(), fl.Complete());
    page.NativeObject[PdfName.Contents] = PdfIndirectRef.Create(updatedStr);
    return page;

    PdfName GetReplacedName(PdfName name, PdfDictionary pageSubDict, PdfDictionary? formSubDict, Dictionary<PdfName, PdfName> replacement)
    {
        if (!replacement.TryGetValue(name, out var nm))
        {
            if (!pageSubDict.ContainsKey(name))
            {
                replacement[name] = name;
                nm = name;
            }
            else
            {
                var newName = new PdfName(name.Value + oc);
                while (pageSubDict.ContainsKey(newName))
                {
                    oc++;
                    newName = new PdfName(name.Value + oc);
                }
                replacement[name] = newName;
                nm = newName;
            }

            IPdfObject fd = null;
            formSubDict?.TryGetValue(name, out fd);
            pageSubDict[nm] = fd ?? PdfNull.Value;
        }
        return nm;
    }
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
    public TestStatus Status { get; set; }
}

public enum TestStatus
{
    PdfLexerError,
    PdfLexerSkip,
    Differences,
    PdfiumError,
    Match
}

