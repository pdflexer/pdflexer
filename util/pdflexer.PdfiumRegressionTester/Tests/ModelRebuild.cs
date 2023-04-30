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
        var parser = new ContentModelParser<double>(doc.Context, page);
        var data = parser.Parse();
        // var test = new PdfRect { LLx = 144, LLy = 684, URx = 160, URy = 700};
        var test = new PdfRect { LLx = 10, LLy = 10, URx = 160, URy = 500 }.NormalizeTo(page);
        var split = new List<IContentGroup<double>>();


        var resources = new PdfDictionary();
        var writer = new ContentWriter<double>(resources);
        writer.Save();
        writer.LineWidth(0.2);

        foreach (var item in data) 
        {
            if (item.Type == ContentType.Text || item.Type == ContentType.Image || item.Type == ContentType.Paths)
            {
                var r = item.CopyArea(test);
                if (r != null) 
                { 
                    split.Add(r);
                    // foreach (var rect in i.GetGlyphBoundingBoxes())
                    // {
                    //     writer.Rect(rect).Stroke();
                    // }
                }

            } else
            {
                split.Add(item);
            }
            // var rect = item.GetBoundingBox();
            // if (rect.Intersects(test) && item is ImageContent img)
            // {
            //     img.Markings ??= new List<MarkedContent>();
            //     img.Markings.Add(new MarkedContent("MatchArea"));
            // }
        }
        // data = data.Where(x=> x.GetBoundingBox().Normalize(page).Intersects(test)).ToList();
        // data.ForEach(x => { if (x is TextSequence txt && txt.Glyphs.Any(c=>c.Glyph?.Char == 'H')) {
        //         // im.GraphicsState = x.GraphicsState with { Clipping = null }; 
        //         var text = string.Join("", txt.Glyphs.Select(x => x.Glyph?.Char));
        //     } 
        // });
        writer.Restore();

        ContentModelWriter<double>.WriteContent(writer, split);

        writer.LineWidth(0.1)
              .Rect(test)
              .Stroke();

        var content = writer.Complete();
        page = page.NativeObject.CloneShallow();

        var updatedStr = new PdfStream(new PdfDictionary(), content);
        page.NativeObject[PdfName.Contents] = PdfIndirectRef.Create(updatedStr);
        page.Resources = resources;
        return page;
    }
}
