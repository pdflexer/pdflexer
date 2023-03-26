using PdfLexer;
using PdfLexer.Content;
using PdfLexer.DOM;
using PdfLexer.Filters;
using PdfLexer.Serializers;

namespace pdflexer.PdfiumRegressionTester.Tests;

internal class ModelRebuild : ITest
{
    public (List<string>, int) RunTest(string inputPath, string outputPath)
    {
        // TODO -> content model rebuild
        using var ctx = new ParsingContext(new ParsingOptions { MaxErrorRetention = 10, ThrowOnErrors = false });
        using var doc = PdfDocument.Open(inputPath);
        using var fo = File.Create(outputPath);
        using var sw = new StreamingWriter(fo, true, true);
        foreach (var pg in doc.Pages)
        {
            var np = ReWriteStream(doc, pg);
            sw.AddPage(np);
        }
        var tr = doc.Trailer.CloneShallow();
        tr.Remove(PdfName.Encrypt);
        sw.Complete(tr, doc.Catalog.CloneShallow());
        return (ctx.ParsingErrors.ToList(), ctx.ErrorCount);
    }

    static PdfPage ReWriteStream(PdfDocument doc, PdfPage page)
    {
        // TODO -> content model rebuild
        var scanner = new PageContentScanner2(doc.Context, page);
        var ms = new MemoryStream();
        var fl = new ZLibLexerStream();

        while (scanner.Advance())
        {
            if (scanner.TryGetCurrentOperation(out var op))
            {
                op.Serialize(fl.Stream);
                fl.Stream.WriteByte((byte)'\n');
            }
        }

        var content = fl.Complete();

        page = page.NativeObject.CloneShallow();

        var updatedStr = new PdfStream(new PdfDictionary(), content);
        page.NativeObject[PdfName.Contents] = PdfIndirectRef.Create(updatedStr);
        return page;
    }
}
