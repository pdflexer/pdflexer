using PdfLexer;
using PdfLexer.Content;
using PdfLexer.Content.Model;
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
        var parser = new ContentModelParser(doc.Context, page);
        var data = parser.Parse();
        var test = new PdfRect { LLx = 144, LLy = 684, URx = 160, URy = 700};
        foreach (var item in data) 
        {
            var rect = item.GetBoundingBox();
            if (rect.Intersects(test) && item is XImgContent img)
            {
                img.Markings ??= new List<MarkedContent>();
                img.Markings.Add(new MarkedContent("MatchArea"));
            }
        }
        // data = data.Where(x=> x.GetBoundingBox().Normalize(page).Intersects(test)).ToList();
        // data.ForEach(x => { if (x is TextSequence txt && txt.Glyphs.Any(c=>c.Glyph?.Char == 'H')) {
        //         // im.GraphicsState = x.GraphicsState with { Clipping = null }; 
        //         var text = string.Join("", txt.Glyphs.Select(x => x.Glyph?.Char));
        //     } 
        // });

        var resources = new PdfDictionary();
        var content = ContentModelWriter.CreateContent(resources, data);

        page = page.NativeObject.CloneShallow();

        var updatedStr = new PdfStream(new PdfDictionary(), content);
        page.NativeObject[PdfName.Contents] = PdfIndirectRef.Create(updatedStr);
        page.Resources = resources;
        return page;
    }
}
