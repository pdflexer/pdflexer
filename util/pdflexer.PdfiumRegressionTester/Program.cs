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
            if (info != null) { corrupt[info.PdfName] = info; }
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
        case "SEMANTIC-TEXT":
        case "SEMANTIC":
            {
                var semanticTester = new SemanticTextTests();
                var semanticSuccess = true;
                foreach (var file in pdfPaths)
                {
                    var nm = Path.GetFileName(file);
                    try
                    {
                        Console.WriteLine($"Starting {file}");
                        writer.WriteLine($"[{nm}] Start");
                        writer.Flush();

                        var semanticResult = semanticTester.RunOne(file, output);
                        var baselinePath = Path.Combine(data, Path.GetFileName(semanticResult.SnapshotPath));
                        var info = new ErrInfo
                        {
                            PdfName = nm,
                            FailedPages = new List<int>(),
                            Errs = new List<string>()
                        };

                        RunResult result;
                        string message;
                        if (!File.Exists(baselinePath))
                        {
                            result = RunResult.NewTest;
                            message = "No semantic baseline snapshot found.";
                            info.Status = TestStatus.Match;
                        }
                        else if (SemanticSnapshotsMatch(baselinePath, semanticResult.SnapshotPath))
                        {
                            result = RunResult.Match;
                            message = "";
                            info.Status = TestStatus.Match;
                        }
                        else
                        {
                            result = RunResult.Regression;
                            message = "Semantic snapshot differs from baseline.";
                            info.Status = TestStatus.Differences;
                            info.Errs.Add(message);
                            info.ErrCount = 1;
                            WriteSemanticDiffHint(baselinePath, semanticResult.SnapshotPath, output);
                            semanticSuccess = false;
                        }

                        if (semanticResult.ReviewWarnings.Count > 0)
                        {
                            var reviewMessage = $"Review flagged: {semanticResult.ReviewWarnings.Count} heuristic warning(s).";
                            message = string.IsNullOrEmpty(message) ? reviewMessage : $"{message} {reviewMessage}";
                        }

                        writer.WriteLine($"[{nm}] {result} {message}");
                        errInfo.WriteLine(JsonSerializer.Serialize(info));
                        summary.WriteLine(JsonSerializer.Serialize(new
                        {
                            Result = result.ToString(),
                            PdfName = nm,
                            Message = message,
                            ReviewWarnings = semanticResult.ReviewWarnings.Count,
                            ReviewPath = semanticResult.ReviewPath
                        }));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Failure: " + e.Message);
                        writer.WriteLine($"[{nm}] Regression {e.Message}");
                        errInfo.WriteLine(JsonSerializer.Serialize(new ErrInfo
                        {
                            PdfName = nm,
                            Status = TestStatus.PdfLexerError,
                            Failure = true,
                            FailureMsg = e.Message,
                            ErrCount = 1,
                            Errs = new List<string> { e.Message }
                        }));
                        summary.WriteLine(JsonSerializer.Serialize(new { Result = RunResult.Regression.ToString(), PdfName = nm, Message = e.Message }));
                        semanticSuccess = false;
                    }
                }

                return semanticSuccess ? 0 : 1;
            }
        case "MERGE":
            return RunMergeTests(pdfPaths, output) ? 0 : 1;
        case "QUICKSAVE":
            {
                var rb = new QuickSave();
                foreach (var file in pdfPaths)
                {
                    Console.WriteLine($"Starting {file}");
                    writer.WriteLine($"[{Path.GetFileName(file)}] Start"); writer.Flush();
                    var result = runner.RunTest(rb, file, output);
                    writer.WriteLine($"[{Path.GetFileName(file)}] {result.Status} {result.Message}");
                    errInfo.WriteLine(JsonSerializer.Serialize(result.Info));
                    summary.WriteLine(JsonSerializer.Serialize(new { Result = result.Status.ToString(), PdfName = Path.GetFileName(file), result.Message }));
                }
                return 0;
            }
        case "DEDUP":
            {
                var rb = new Dedup();
                foreach (var file in pdfPaths)
                {
                    Console.WriteLine($"Starting {file}");
                    writer.WriteLine($"[{Path.GetFileName(file)}] Start"); writer.Flush();
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

static bool SemanticSnapshotsMatch(string baselinePath, string candidatePath)
{
    using var baseline = JsonDocument.Parse(File.ReadAllText(baselinePath));
    using var candidate = JsonDocument.Parse(File.ReadAllText(candidatePath));
    return JsonElementsEqual(baseline.RootElement, candidate.RootElement);
}

static void WriteSemanticDiffHint(string baselinePath, string candidatePath, string output)
{
    var baseName = Path.GetFileNameWithoutExtension(candidatePath);
    var diffPath = Path.Combine(output, $"{baseName}_semantic.diff.txt");

    var baselineLines = File.ReadAllLines(baselinePath);
    var candidateLines = File.ReadAllLines(candidatePath);
    var max = Math.Max(baselineLines.Length, candidateLines.Length);

    using var writer = new StreamWriter(diffPath, false);
    writer.WriteLine($"baseline: {baselinePath}");
    writer.WriteLine($"candidate: {candidatePath}");

    for (var i = 0; i < max; i++)
    {
        var baseline = i < baselineLines.Length ? baselineLines[i] : "<missing>";
        var candidate = i < candidateLines.Length ? candidateLines[i] : "<missing>";
        if (string.Equals(baseline, candidate, StringComparison.Ordinal))
        {
            continue;
        }

        writer.WriteLine();
        writer.WriteLine($"line {i + 1}");
        writer.WriteLine($"baseline:  {baseline}");
        writer.WriteLine($"candidate: {candidate}");
        break;
    }
}

static bool JsonElementsEqual(JsonElement left, JsonElement right)
{
    if (left.ValueKind != right.ValueKind)
    {
        return false;
    }

    switch (left.ValueKind)
    {
        case JsonValueKind.Object:
            var rightProperties = right.EnumerateObject().ToDictionary(x => x.Name, x => x.Value);
            var propertyCount = 0;
            foreach (var leftProperty in left.EnumerateObject())
            {
                propertyCount++;
                if (!rightProperties.TryGetValue(leftProperty.Name, out var rightValue))
                {
                    return false;
                }

                if (!JsonElementsEqual(leftProperty.Value, rightValue))
                {
                    return false;
                }
            }

            return propertyCount == rightProperties.Count;
        case JsonValueKind.Array:
            var leftItems = left.EnumerateArray();
            var rightItems = right.EnumerateArray();
            while (true)
            {
                var hasLeft = leftItems.MoveNext();
                var hasRight = rightItems.MoveNext();
                if (hasLeft != hasRight)
                {
                    return false;
                }

                if (!hasLeft)
                {
                    return true;
                }

                if (!JsonElementsEqual(leftItems.Current, rightItems.Current))
                {
                    return false;
                }
            }
        case JsonValueKind.String:
            return string.Equals(left.GetString(), right.GetString(), StringComparison.Ordinal);
        case JsonValueKind.Number:
            return string.Equals(left.GetRawText(), right.GetRawText(), StringComparison.Ordinal);
        case JsonValueKind.True:
        case JsonValueKind.False:
            return left.GetBoolean() == right.GetBoolean();
        case JsonValueKind.Null:
        case JsonValueKind.Undefined:
            return true;
        default:
            return string.Equals(left.GetRawText(), right.GetRawText(), StringComparison.Ordinal);
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



public class ErrInfo
{
    public string PdfName { get; set; } = null!;
    public List<string>? Errs { get; set; }
    public int ErrCount { get; set; }
    public List<int>? FailedPages { get; set; }
    public bool Failure { get; set; }
    public string? FailureMsg { get; set; }
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

