using PdfLexer;

namespace pdflexer.PdfiumRegressionTester.Tests;

internal class QuickSave : ITest
{
    public (List<string>, int) RunTest(string inputPath, string outputPath)
    {
        using var ctx = new ParsingContext(new ParsingOptions { MaxErrorRetention = 10, ThrowOnErrors = false });
        using var doc = PdfDocument.Open(inputPath);

        // add page and save existing doc -> uses logic for saving existing doc
        var pg = doc.AddPage();
        var modified = doc.Save();
        File.WriteAllBytes(outputPath, modified);

        // open and remove added page to keep results identical with baseline and save
        // as output -> uses existing save again
        using var doc2 = PdfDocument.Open(modified);
        doc2.Pages.RemoveAt(doc2.Pages.Count - 1);
        using var fo = File.Create(outputPath);
        doc2.SaveTo(fo);

        // pdfium results should be identical
        return (ctx.ParsingErrors.ToList(), ctx.ErrorCount);
    }

}
