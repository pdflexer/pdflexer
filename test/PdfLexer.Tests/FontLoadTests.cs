using PdfLexer.Fonts;
using PdfLexer.Writing;
using System.IO;
using Xunit;

namespace PdfLexer.Tests;

public class FontLoadTests
{
    [Fact]
    public void It_Loads_TrueType()
    {
        var tmp = File.ReadAllBytes("c:\\temp\\USPSIMBStandard.ttf");
        var tmp2 = File.ReadAllBytes("c:\\temp\\USPSIMBCompact.ttf");
        var tmp3 = File.ReadAllBytes("c:\\temp\\Roboto-Black.ttf");
        var font = TrueTypeFont.CreateWritableFont(tmp);
        var font2 = TrueTypeFont.CreateWritableFont(tmp2);
        var font3 = TrueTypeFont.CreateWritableFont(tmp3);

        using var pdf = PdfDocument.Create();
        {
            var pg = pdf.AddPage();
            using var writer = new PageWriter<double>(pg, PageWriteMode.Replace);
            writer.Font(font, 10)
                  .TextMove(100, 100)
                  .Text("FFTTDAADTTADTFDDFDDTFAFATDTDDFDAFDADDADDAFAAAFTTFTFDTFAAADADDDFDF")
                  .Font(font2, 10)
                  .TextMove(100, 115)
                  .Text("FFTTDAADTTADTFDDFDDTFAFATDTDDFDAFDADDADDAFAAAFTTFTFDTFAAADADDDFDF")
                  .Font(font3, 10)
                  .TextMove(100, 130)
                  .Text("Whereas disregard and contempt for human rights have resulted");
        }

        {
            using var fo = File.Create("c:\\temp\\barcode.pdf");
            pdf.SaveTo(fo);
        }
        return;
        {
            using var pdfo = PdfDocument.Open("c:\\temp\\barcode.pdf");
            var scanner = pdfo.Pages[0].GetTextScanner();
            scanner.Advance();
            var rect = scanner.GetCurrentBoundingBox();
            var txt = pdfo.Pages[0].GetTextVisually();
            var cs = pdfo.Pages[0].DumpDecodedContents();
            var res = pdfo.Pages[0].Resources;
        }
    }
}
