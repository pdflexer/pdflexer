using PdfLexer;
using PdfLexer.Serializers;

namespace pdflexer.PdfiumRegressionTester.Tests;

internal class Dedup : ITest
{
    public (List<string>, int) RunTest(string inputPath, string outputPath)
    {
        using var ctx = new ParsingContext(new ParsingOptions { MaxErrorRetention = 10, ThrowOnErrors = false });
        using var doc = PdfDocument.Open(inputPath);
        doc.DeduplicateResources();
        using var fo = File.Create(outputPath);
        using var sw = new StreamingWriter(fo);
        foreach (var pg in doc.Pages)
        {
            sw.AddPage(pg);
        }
        var tr = doc.Trailer.CloneShallow();
        tr.Remove(PdfName.Encrypt);
        sw.Complete(tr, doc.Catalog.CloneShallow());
        return (ctx.ParsingErrors.ToList(), ctx.ErrorCount);
    }
}
