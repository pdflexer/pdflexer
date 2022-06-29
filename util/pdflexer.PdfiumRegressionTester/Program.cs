using pdflexer.PdfiumRegressionTester;
using PdfLexer;
using PdfLexer.Content;
using PdfLexer.DOM;
using PdfLexer.Filters;
using PdfLexer.Operators;
using PdfLexer.Serializers;

Directory.CreateDirectory(args[1]);

var counts = new List<(string, int, int)>();
{
    var modified = Path.Combine(args[1], "all.pdf");
    using var fo = File.Create(modified);
    using var sw = new StreamingWriter(fo);
    
    foreach (var file in Directory.GetFiles(args[0], "*.pdf"))
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
    
    using var allfs = File.OpenRead(Path.Combine(args[1], "all.pdf"));
    using var all = PdfDocument.Open(allfs);
    foreach (var (pdf, start, end) in counts)
    {
        var modified = Path.Combine(args[1], Path.GetFileName(pdf));
        {
            using var doc = PdfDocument.Create();
            for (var i = start;i < end;i++)
            {
                doc.Pages.Add(all.Pages[i]);
            }
            using var fo = File.Create(modified);
            doc.SaveTo(fo);
        }

        var comparer = new Compare(Path.Combine(args[1], Path.GetFileNameWithoutExtension(pdf)), 2);
        var pgs = comparer.CompareAllPages(pdf, modified);
        var nm = Path.GetFileName(pdf);
        bool changes = false;
        for (var i = 0; i < pgs.Count; i++)
        {
            var pg = pgs[i];
            if (pg.HadChanges)
            {
                changes = true;
                Console.WriteLine($"[{nm}] {i} {pg.Type.ToString()}");
            }
        }
        if (!changes)
        {
            File.Delete(modified);
        }
        else
        {
            File.Copy(pdf, Path.Combine(args[1], Path.GetFileNameWithoutExtension(pdf) + "_baseline.pdf"), true);
        }
    }

}

foreach (var pdf in Directory.GetFiles(args[0], "*.pdf"))
{
    var comparer = new Compare(Path.Combine(args[1], Path.GetFileNameWithoutExtension(pdf)), 2);
    var modified = Path.Combine(args[1], Path.GetFileName(pdf));
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
            changes = true;
            Console.WriteLine($"[{nm}] {i} {pg.Type.ToString()}");
        }
    }
    if (!changes)
    {
        File.Delete(modified);
    } else
    {
        File.Copy(pdf, Path.Combine(args[1], Path.GetFileNameWithoutExtension(pdf) + "_baseline.pdf"), true);
    }
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