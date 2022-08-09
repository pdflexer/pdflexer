using Microsoft.Extensions.Logging;
using PDFiumCore;
using PdfLexer;
using PdfLexer.Content;
using PdfLexer.DOM;
using PdfLexer.Fonts;
using System.Text;

namespace pdflexer.PdfiumRegressionTester
{
    internal class IgnoreSetup
    {
        public List<int> BaselineIgnores { get; set; } = new();
        public List<int> CandidateIgnores { get; set; } = new();
    }

    internal class CharInfo
    {
        // (float x, float y, char c, float llx, float lly, float urx, float ury)
        public float x { get; set; }
        public float y { get; set; }
        public char c { get; set; }
        public uint cp { get; set; }
        public float llx { get; set; }
        public float lly { get; set; }
        public float urx { get; set; }
        public float ury { get; set; }
        public string Font { get; set; }

    }
    
    internal class TextTests
    {
        internal static List<string> skip = new List<string>
        {
            "__bug1260585.pdf.pdf", // pdfium spacing seems wrong
            "__bug878194.pdf.pdf", // pdfium doesn't extract some stacked french text, maybe deduping internally?
            "__aboutstacks.pdf.pdf", // pdfium doesn't read composite truetype with post table, need to add test coverage here though
            "__issue1127.pdf.pdf", // pdfium has spacing issues but we aren't properly extracting encoding for embedded type1 ps font
            "__issue1658.pdf.pdf", // type1 ps embedded TODO
            "__issue1687.pdf.pdf", // bad tounicode values, good truetype ps name, pdfium uses to unicode
        };
        internal static Dictionary<string, IgnoreSetup> ignoreMap = new Dictionary<string, IgnoreSetup>
        {
            ["__bug766138.pdf.pdf"] = new IgnoreSetup
            {
                CandidateIgnores = new List<int> { 160 } // whitespace char ignored by pdfium
            },
            ["__aboutstacks.pdf.pdf"] = new IgnoreSetup
            {
                CandidateIgnores = new List<int> { 3 } // whitespace char ignored by pdfium
            },
            ["__bug1354114.pdf.pdf"] = new IgnoreSetup
            {
                CandidateIgnores = new List<int> { 0 }
            },
            ["__bug956965.pdf.pdf"] = new IgnoreSetup
            {
                CandidateIgnores = new List<int> { 61 } // equal sign in font ignored by pdfium, maybe symbolic?
            },
            ["__bug1123803.pdf.pdf"] = new IgnoreSetup
            {
                BaselineIgnores = new List<int> { 10063 }, // pdflexer extract fails with missing tounicode data TODO
                CandidateIgnores = new List<int> { 0, 12288 } // 10063 from above and then a whitespace char
            }
        };
        private ILogger _logger;
        public TextTests(ILogger logger)
        {
            _logger = logger;
            fpdfview.FPDF_InitLibrary();
        }

        public bool RunOne(string pdf, string output)
        {
            var name = Path.GetFileName(pdf);

            if (skip.Contains(name)) { return true; }

            ignoreMap.TryGetValue(name, out var ignores);
            ignores ??= new IgnoreSetup();

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

                var font = SingleByteFont.GetTimesRoman();
                using var writer = pg.GetWriter();
                writer.Form(form)
                      .SetStrokingRGB(0, 0, 0);

                var reader = new TextScanner(doc.Context, page);
                var lines = new List<(float x, float y, char c)>();
                var unmatched = new List<(float x, float y, char c)>();
                var chars = new Dictionary<string, CharInfo>();
                while (reader.Advance())
                {
                    var glyphChars = reader.Glyph.MultiChar ?? $"{reader.Glyph.Char}";

                    var (llx, lly, urx, ury) = reader.GetCurrentBoundingBox();
                    var (x, y) = reader.GetCurrentTextPos();
                    // lines.Add($"{x:0.000} {y:0.000} {bb.llx:0.0} {bb.lly:0.0} {bb.urx:0.0} {bb.ury:0.0} {c}");

                    foreach (var c in glyphChars) {
                        if (c == '\n' || c == '\r' || c == ' ' || c == '-') { continue; }
                        
                        if (c == 'ﬁ' || c == (char)64257) 
                        {
                            lines.Add(((float)x, (float)y, 'f'));
                            var ci = new CharInfo { c = 'f', Font = reader.GraphicsState.FontName?.Value ?? "uk", 
                                x = x, y = y, llx = llx, lly = lly, urx = urx, ury= ury };
                            chars[$"{x:0.0}{y:0.0}f"] = ci;
                            lines.Add(((float)x, (float)y, 'i'));
                            ci = new CharInfo
                            {
                                c = 'l',
                                Font = reader.GraphicsState.FontName?.Value ?? "uk",
                                x = x,
                                y = y,
                                llx = llx,
                                lly = lly,
                                urx = urx,
                                ury = ury
                            };
                            chars[$"{x:0.0}{y:0.0}i"] = ci;
                        }
                        else if (c == '\uFB02')
                        {
                            lines.Add(((float)x, (float)y, 'f'));
                            var ci =  new CharInfo
                            {
                                c = 'f',
                                Font = reader.GraphicsState.FontName?.Value ?? "uk",
                                x = x,
                                y = y,
                                llx = llx,
                                lly = lly,
                                urx = urx,
                                ury = ury
                            }; ;
                            chars[$"{x:0.0}{y:0.0}f"] = ci;
                            lines.Add(((float)x, (float)y, 'l'));
                            ci = new CharInfo
                            {
                                c = 'l',
                                Font = reader.GraphicsState.FontName?.Value ?? "uk",
                                x = x,
                                y = y,
                                llx = llx,
                                lly = lly,
                                urx = urx,
                                ury = ury
                            };
                            chars[$"{x:0.0}{y:0.0}l"] = ci;
                        }
                        else
                        {
                            lines.Add(((float)x, (float)y, c));
                            var ci = new CharInfo
                            {
                                c = c,
                                Font = reader.GraphicsState.FontName?.Value ?? "uk",
                                x = x,
                                y = y,
                                llx = llx,
                                lly = lly,
                                urx = urx,
                                ury = ury,
                                cp = reader.Glyph.CodePoint ?? 0
                            }; ;
                            chars[$"{x:0.0}{y:0.0}{c}"] = ci;
                        }
                    }
                }
                writer.SetStrokingRGB(255, 0, 0);


                var d2 = new Scope();
                var ppg = fpdfview.FPDF_LoadPage(pdoc, pi - 1);
                d2.Add(() => fpdfview.FPDF_ClosePage(ppg));

                var txt = fpdf_text.FPDFTextLoadPage(ppg);
                d2.Add(() => fpdf_text.FPDFTextClosePage(txt));

                var charList = chars.Select(x => x.Value).ToList();
                var lines2 = new List<(float x, float y, char c)>();
                var unmatched2 = new List<(float x, float y, char c)>();
                var chars2 = new Dictionary<string, (float x, float y, char c)>();
                var cnt = fpdf_text.FPDFTextCountChars(txt);
                for (var i = 0; i < cnt; i++)
                {
                    double llx = 0, lly = 0, urx = 0, ury = 0;
                    fpdf_text.FPDFTextGetCharBox(txt, i, ref llx, ref urx, ref lly, ref ury);
                    var c = (char)fpdf_text.FPDFTextGetUnicode(txt, i);
                    if (c == '\n' || c == '\r' || c == ' ' || c == '-' || c == '\u0002') { continue; }
                    
                    double x = 0, y = 0;
                    fpdf_text.FPDFTextGetCharOrigin(txt, i, ref x, ref y);

                    

                    var key = $"{x:0.0}{y:0.0}{c}";
                    if (!chars.TryGetValue(key, out var prev))
                    {
                        var dist = Nearest((float)x, (float)y, c, lines);
                        if (dist > 0.01)
                        {
                            var ci = (int)c;
                            if (ignores.BaselineIgnores.Contains(ci))
                            {
                                continue;
                            }
                            // var nm = GetFontName(txt, i);
                            writer
                              .LineWidth(0.01m)
                              .Rect((decimal)llx, (decimal)lly, (decimal)(urx - llx), (decimal)(ury - lly))
                              .Stroke();
                                writer.Circle((decimal)x, (decimal)y, 0.05m).Stroke();
                            unmatched2.Add(((float)x, (float)y, c));

                            var (dd, cc) = Nearest((float)x, (float)y, charList);
                            if (cc != null && dd < 5)
                            {
                                writer.Font(font, 0.5)
                                      .TextMove(x + 1, y - 1)
                                      .Text($"b:{(int)c} c:{(int)cc.c} cf: {cc.Font}")
                                      .EndText();
                            } else
                            {
                                writer.Font(font, 0.1)
                                      .TextMove(x + 1, y - 1)
                                      .Text($"b:{(int)c}")
                                      .EndText();
                            }
                        }
                    }
                    chars2[key] = ((float)x, (float)y, c);

                    lines2.Add(((float)x, (float)y, c));
                }

                writer.SetStrokingRGB(0, 0, 0);
                foreach (var kvp in chars)
                {
                    if (!chars2.TryGetValue(kvp.Key, out var prev))
                    {
                        var dist = Nearest(kvp.Value.x, kvp.Value.y, kvp.Value.c, lines2);
                        if (dist > 0.01)
                        {
                            var ci = (int)kvp.Value.c;
                            if (ignores.CandidateIgnores.Contains(ci))
                            {
                                continue;
                            }

                            writer.LineWidth(0.05m)
                                  .Rect((decimal)kvp.Value.llx, (decimal)kvp.Value.lly,
                                    (decimal)(kvp.Value.urx - kvp.Value.llx), (decimal)(kvp.Value.ury - kvp.Value.lly))
                                  .Stroke();

                            writer.Circle((decimal)kvp.Value.x, (decimal)kvp.Value.y, 0.1m).Stroke();
                            unmatched.Add((kvp.Value.x, kvp.Value.y, kvp.Value.c));
                            writer.Font(font, 0.5)
                              .TextMove(kvp.Value.x + 1, kvp.Value.y - 2)
                              .Text($"c:{(int)kvp.Value.c} cf: {kvp.Value.Font}")
                              .EndText();
                        }

                    }
                }

                var op = new StringBuilder();
                foreach (var grp in unmatched.OrderBy(x => x.y).GroupBy(x => x.y))
                {
                    var vals = grp.OrderBy(x => x.x);
                    foreach (var v in vals)
                    {
                        op.Append($"{v.x:0.0} {v.y:0.0} {v.c}\n");
                    }
                }
                var result = op.ToString();

                op = new StringBuilder();
                foreach (var grp in unmatched2.OrderBy(x => x.y).GroupBy(x => x.y))
                {
                    var vals = grp.OrderBy(x => x.x);
                    foreach (var v in vals)
                    {
                        op.Append($"{v.x:0.0} {v.y:0.0} {v.c}\n");
                    }
                }
                var result2 = op.ToString();
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

        private static unsafe string GetFontName(FpdfTextpageT page, int c)
        {
            var maxChars = 20;
            Span<byte> txt = new byte[maxChars * 2 + 1];

            int flags = 0;
            uint read = 0;
            fixed (byte* ptrr = &txt[0])
            {
                read = fpdf_text.FPDFTextGetFontInfo(page, c, (IntPtr)ptrr, (uint)txt.Length, ref flags);
            }
            return Encoding.Unicode.GetString(txt.Slice(0, (int)read));
        }
        private static double Nearest(float x, float y, char cf, List<(float x, float y, char c)> others)
        {
            var d = double.MaxValue;
            foreach (var item in others)
            {
                var dist = Math.Sqrt(Math.Pow(Math.Abs(x - item.x), 2) + Math.Pow(Math.Abs(y - item.y), 2));
                if (cf == item.c && dist < d)
                {
                    d = dist;
                }
            }
            return d;
        }

        private static double Nearest(float x, float y, char cf, List<CharInfo> others)
        {
            var d = double.MaxValue;
            foreach (var item in others)
            {
                var dist = Math.Sqrt(Math.Pow(Math.Abs(x - item.x), 2) + Math.Pow(Math.Abs(y - item.y), 2));
                if (cf == item.c && dist < d)
                {
                    d = dist;
                }
            }
            return d;
        }
        private static (double dist, char? c) Nearest(float x, float y, List<(float x, float y, char c)> others)
        {
            var d = double.MaxValue;
            (float x, float y, char c)? c = null;
            foreach (var item in others)
            {
                var dist = Math.Sqrt(Math.Pow(Math.Abs(x - item.x), 2) + Math.Pow(Math.Abs(y - item.y), 2));
                if (dist < d)
                {
                    d = dist;
                    c = item;
                }
            }
            return (d, c?.c);
        }

        private static (double dist, CharInfo? c) Nearest(float x, float y, List<CharInfo> others)
        {
            var d = double.MaxValue;
            CharInfo? c = null;
            foreach (var item in others)
            {
                var dist = Math.Sqrt(Math.Pow(Math.Abs(x - item.x), 2) + Math.Pow(Math.Abs(y - item.y), 2));
                if (dist < d)
                {
                    d = dist;
                    c = item;
                }
            }
            return (d, c);
        }
    }


}
