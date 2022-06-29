using pdflexer.PdfiumRegressionTester;
using PdfLexer;
using PdfLexer.Content;
using PdfLexer.DOM;
using PdfLexer.Filters;
using PdfLexer.Operators;
using PdfLexer.Serializers;
using System.CommandLine;



StreamWriter writer = null;

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

var rootCommand = new RootCommand("PDF regression testing");
rootCommand.AddOption(pdfRoot);
rootCommand.AddOption(output);
rootCommand.AddOption(type);

int returnCode = 0;

rootCommand.SetHandler((type, root, prefix) =>
{
    returnCode = RunBase(type, root, prefix);
}, type, pdfRoot, output);

await rootCommand.InvokeAsync(args);

return returnCode;

int RunBase(string type, string pdfRoot, string output)
{
    Directory.CreateDirectory(output);
    using var lo = File.Create(Path.Combine(output, type.ToLower() + ".log"));
    writer = new StreamWriter(lo);
    using var _ = writer;
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

    foreach (var pdf in Directory.GetFiles(pdfRoot, "*.pdf"))
    {
        var comparer = new Compare(Path.Combine(output, Path.GetFileNameWithoutExtension(pdf)), 2);
        var modified = Path.Combine(output, Path.GetFileName(pdf));
        {
            using var doc = PdfDocument.Open(File.ReadAllBytes(pdf));
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
        var pgs = comparer.CompareAllPages(pdf, modified);
        var nm = Path.GetFileName(pdf);
        bool changes = false;
        for (var i = 0; i < pgs.Count; i++)
        {
            var pg = pgs[i];
            if (pg.HadChanges)
            {
                if (pg.Type == ChangeType.ErrorBaseline)
                {
                    Log($"[{nm}] pdfium failed to open pg on baseline{i}");
                } else
                {
                    changes = true;
                    Log($"[{nm}] failed pg {i} -> {pg.Type.ToString()}");
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
        var op = scanner.GetCurrentOperation();
        op.Serialize(ms);
        ms.WriteByte((byte)'\n');
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