using Microsoft.Extensions.Logging;
using PDFiumCore;
using PdfLexer;
using PdfLexer.Content;
using PdfLexer.DOM;

namespace pdflexer.PdfiumRegressionTester
{
    internal class TextTests
    {
        private ILogger _logger;
        public TextTests(ILogger logger)
        {
            _logger = logger;
            fpdfview.FPDF_InitLibrary();
        }

        public bool RunOne(string pdf, string output)
        {
            var name = Path.GetFileName(pdf);
            using var doc = PdfDocument.Open(File.ReadAllBytes(pdf));

            using var doc2 = PdfDocument.Create();

            var d = new Scope();
            var pdoc = fpdfview.FPDF_LoadDocument(pdf, null);
            if (pdoc == null)
            {
                _logger.LogWarning("[{PdfName}] PDFium failure: {Error}", name, fpdfview.FPDF_GetLastError());
                return true;
            }
            d.Add(() => fpdfview.FPDF_CloseDocument(pdoc));

            var success = true;
            var pi = 1;
            foreach (var page in doc.Pages)
            {
                var pg = doc2.AddPage();
                pg.MediaBox.URx = 500;
                pg.MediaBox.URy = 200;
                pg.MediaBox = page.MediaBox;

                var form = XObjForm.FromPage(page);

                using var writer = pg.GetWriter();
                writer.Form(form)
                      .SetStrokingRGB(0, 0, 0);

                var reader = new TextScanner(doc.Context, page);
                var lines = new List<string>();
                while (reader.Advance())
                {
                    var c = reader.Glyph.Char;
                    var bb = reader.GetCurrentBoundingBox();
                    var (x, y) = reader.GetCurrentTextPos();
                    // lines.Add($"{x:0.000} {y:0.000} {bb.llx:0.0} {bb.lly:0.0} {bb.urx:0.0} {bb.ury:0.0} {c}");
                    lines.Add($"{x:0.000} {y:0.000} {c}");
                    writer.LineWidth(0.05m)
                          .Rect((decimal)bb.llx, (decimal)bb.lly, (decimal)(bb.urx - bb.llx), (decimal)(bb.ury - bb.lly))
                          .Stroke();



                    writer.Circle((decimal)x, (decimal)y, 0.1m).Stroke();
                }
                lines.Sort();
                var result = string.Join('\n', lines);

                writer.SetStrokingRGB(255, 0, 0);


                var d2 = new Scope();
                var ppg = fpdfview.FPDF_LoadPage(pdoc, 0);
                d2.Add(() => fpdfview.FPDF_ClosePage(ppg));

                var txt = fpdf_text.FPDFTextLoadPage(ppg);
                d2.Add(() => fpdf_text.FPDFTextClosePage(txt));

                lines.Clear();
                var cnt = fpdf_text.FPDFTextCountChars(txt);
                for (var i=0;i<cnt;i++)
                {
                    double llx = 0, lly = 0, urx = 0, ury = 0;
                    fpdf_text.FPDFTextGetCharBox(txt, i, ref llx, ref urx, ref lly, ref ury);
                    var c = (char)fpdf_text.FPDFTextGetUnicode(txt, i);

                    writer
                      .LineWidth(0.01m)
                      .Rect((decimal)llx, (decimal)lly, (decimal)(urx - llx), (decimal)(ury - lly))
                      .Stroke();


                    // var rect = new FS_RECTF_();
                    // fpdf_text.FPDFTextGetLooseCharBox(txt, i, rect);
                    // 
                    // writer
                    //   .LineWidth(0.01m)
                    //   .Rect((decimal)rect.Left, (decimal)rect.Bottom, (decimal)(rect.Right - rect.Left), (decimal)(rect.Top - rect.Bottom))
                    //   .Stroke();

                    double x = 0, y = 0;
                    fpdf_text.FPDFTextGetCharOrigin(txt, i, ref x, ref y);
                    writer.Circle((decimal)x, (decimal)y, 0.05m).Stroke();
                    // lines.Add($"{x:0.000} {y:0.000} {llx:0.0} {lly:0.0} {urx:0.0} {ury:0.0} {c}");
                    lines.Add($"{x:0.000} {y:0.000} {c}");
                }

                lines.Sort();
                var result2 = string.Join('\n', lines);
                if (result != result2)
                {
                    var foc = Path.Combine(output, Path.GetFileNameWithoutExtension(name) + "_cc_" + pi + ".txt");
                    var fob = Path.Combine(output, Path.GetFileNameWithoutExtension(name) + "_bc_" + pi + ".txt");
                    File.WriteAllText(foc, result);
                    File.WriteAllText(fob, result2);
                    success = false;
                }
                pi++;
            }
            if (true)
            {
                var pdfo = Path.Combine(output, Path.GetFileNameWithoutExtension(name) + "_rects.pdf");
                using var fs = File.Create(pdfo);
                doc2.SaveTo(fs);
            }

            return success;
        }
    }
}
