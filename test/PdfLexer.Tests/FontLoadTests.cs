using PdfLexer.Fonts;
using PdfLexer.Fonts.Files;
using PdfLexer.Writing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig.Fonts.TrueType.Tables;
using Xunit;

namespace PdfLexer.Tests;

public class FontLoadTests
{
    [Fact]
    public void It_Loads_TrueType()
    {
        var tmp = File.ReadAllBytes("c:\\temp\\USPSIMBStandard.ttf");
        var tmp2 = File.ReadAllBytes("c:\\temp\\USPSIMBCompact.ttf");
        var font = TrueTypeFont.CreateWritableFont(tmp);
        var font2 = TrueTypeFont.CreateWritableFont(tmp2);

        using var pdf = PdfDocument.Create();
        {
            var pg = pdf.AddPage();
            using var writer = new PageWriter<double>(pg, PageWriteMode.Replace);
            writer.Font(font, 10)
                  .TextMove(100, 100)
                  .Text("FFTTDAADTTADTFDDFDDTFAFATDTDDFDAFDADDADDAFAAAFTTFTFDTFAAADADDDFDF")
                  .Font(font2, 10)
                  .TextMove(100, 115)
                  .Text("FFTTDAADTTADTFDDFDDTFAFATDTDDFDAFDADDADDAFAAAFTTFTFDTFAAADADDDFDF");
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
