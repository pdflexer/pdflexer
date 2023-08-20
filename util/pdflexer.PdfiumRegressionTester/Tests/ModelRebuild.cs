using PdfLexer;
using PdfLexer.Content;
using PdfLexer.Content.Model;
using PdfLexer.DOM;
using PdfLexer.Filters;
using PdfLexer.Serializers;
using PdfLexer.Writing;

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
            var np = ReWriteStream(doc, pg, doc.Catalog);
            sw.AddPage(np);
        }
        var tr = doc.Trailer.CloneShallow();
        tr.Remove(PdfName.Encrypt);
        sw.Complete(tr, doc.Catalog.CloneShallow());
        return (ctx.ParsingErrors.ToList(), ctx.ErrorCount);
    }

    static PdfPage ReWriteStream(PdfDocument doc, PdfPage page, PdfDictionary catalog)
    {
        var parser = new ContentModelParser<decimal>(doc.Context, page, false);
        var data = parser.Parse(doc.Catalog);
        var resources = data.Any(x => x.Type == ContentType.Form) ? page.Resources.CloneShallow() : new PdfDictionary();
        var writer = new ContentWriter<decimal>(resources);

        ContentModelWriter<decimal>.WriteContent(writer, data, catalog);

        var content = writer.Complete();
        page = page.NativeObject.CloneShallow();

        var updatedStr = new PdfStream(new PdfDictionary(), content);
        page.NativeObject[PdfName.Contents] = PdfIndirectRef.Create(updatedStr);
        page.Resources = resources;
        return page;
    }
}
