using pdflexer.PdfiumRegressionTester;
using PdfLexer;
using PdfLexer.Serializers;

Directory.CreateDirectory(args[1]);


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
            sw.AddPage(pg);
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
        File.Copy(pdf, Path.Combine(args[1], Path.GetFileNameWithoutExtension(pdf) + "_baseline.pdf"));
    }
}