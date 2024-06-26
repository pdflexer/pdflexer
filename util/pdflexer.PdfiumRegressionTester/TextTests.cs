﻿using Microsoft.Extensions.Logging;
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

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles")]
    internal class CharInfo
    {
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
            "__issue1127.pdf.pdf", // pdfium has spacing issues
            "__issue1658.pdf.pdf", // type1 ps embedded TODO
            "__issue1687.pdf.pdf", // bad tounicode values, good truetype ps name, pdfium uses to unicode
            "__bug1292316.pdf.pdf", // unembedded font without width info written, adobe / pdfium knows glyph metrics somehow and glyphs use up space
                                    // we don't read so causes subsequent text to be off, TODO determine
            "__bug1443140.pdf.pdf", // some potential misreads on typ1c CFF but corrupt page has a ton of data noise
            "__issue1536.pdf.pdf", // differences and doesn't open in adobe
            "__issue2829.pdf.pdf", // TODO vert. font
            "__issue12533.pdf.pdf", // pdfium spacing off from adobe / pdflexer
            "__issue12714.pdf.pdf", // pdfium spacing off from adobe / pdflexer first page has BG1 UCS though
            "__fit11-talk.pdf.pdf", // pdfium spacing off from adobe / pdflexer
            "__issue2627.pdf.pdf", // no glyph info to extract, some minor differences from pdfium
            "__issue11678.pdf.pdf", // adobe won't open page with issues
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
            },
            ["__bug1142033.pdf.pdf"] = new IgnoreSetup
            {
                BaselineIgnores = new List<int> { 68, 67, 50, 4018, 3968 }, // no unicode mapping in doc
                CandidateIgnores = new List<int> { 3, 9234, 3958 } //
            },
            ["__issue1629.pdf.pdf"] = new IgnoreSetup
            {
                CandidateIgnores = new List<int> { 2 } // pound char
            },
            ["__issue1133.pdf.pdf"] = new IgnoreSetup
            {
                CandidateIgnores = new List<int> { 8195 } // pound char
            },
            ["__issue1317.pdf.pdf"] = new IgnoreSetup
            {
                CandidateIgnores = new List<int> { 173, 8208 } // misc symbols not returned
            },
            ["__issue1317.pdf.pdf"] = new IgnoreSetup
            {
                BaselineIgnores = new List<int> { 937 },
                CandidateIgnores = new List<int> { 1120 } // TODO: review omega unicode mappings
            },
            ["__issue1257.pdf.pdf"] = new IgnoreSetup
            {
                CandidateIgnores = new List<int> { 2 } // unread symbol pdfium
            },
            ["__issue6238.pdf.pdf"] = new IgnoreSetup
            {
                CandidateIgnores = new List<int> { 173 } // unread symbol pdfium
            },
            ["__issue6238.pdf.pdf"] = new IgnoreSetup
            {
                CandidateIgnores = new List<int> { 173 } // unread symbol pdfium
            },
            ["__issue7496.pdf.pdf"] = new IgnoreSetup
            {
                CandidateIgnores = new List<int> { 64259, 64256, 2 }, // auto ligature splitting,
                BaselineIgnores = new List<int> { 102, 105 }
            },
            ["__issue4883.pdf.pdf"] = new IgnoreSetup
            {
                CandidateIgnores = new List<int> { 9, 173, 8208 }, // different hyphen types pdfium dedups
            },
            ["__fips197.pdf.pdf"] = new IgnoreSetup
            {
                CandidateIgnores = new List<int> { 9, 173, 91, 93, 175 }, // whitespace / hyphens, TODO: type1 using base14, need test coverage
            },
            ["__issue4883.pdf.pdf"] = new IgnoreSetup
            {
                CandidateIgnores = new List<int> { 173 }, //  hyphen
            },
            ["__issue12402.pdf.pdf"] = new IgnoreSetup
            {
                BaselineIgnores = new List<int> { 160 }, //  whitespace
            },
            ["__geothermal.pdf.pdf"] = new IgnoreSetup
            {
                CandidateIgnores = new List<int> { 74, 100, 71, 112, 79, 105, 108, 84, 115, 73, 86, 114, 72, 83, 116, 76, 101, 66, 99, 48, 46, 49, 51, 80, 68, 111, 109, 110, 97, 40, 69, 67, 70, 56, 44, 61, 98, 53, 50, 102, 54, 55, 57, 176, 8211, 107, 121, 52, 58, 77, 104, 103, 81, 65, 82, 169, 41, 87, 117, 120, 78, 119, 8805, 8804, 118, 85, 186, 38, 122, 89, 75 },
                //  bunch of data from graphs pdfium ignores
            },
            ["__gesamt.pdf.pdf"] = new IgnoreSetup
            {
                BaselineIgnores = new List<int> { 160 }, //  whitespace
            },

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

            var baseValues = new HashSet<uint>();

            var candValues = new HashSet<uint>();

            var success = true;
            var pi = 1;
            foreach (var page in doc.Pages)
            {
                var pg = doc2.AddPage();
                pg.MediaBox.URx = 500;
                pg.MediaBox.URy = 200;
                pg.MediaBox = page.MediaBox;

                var form = XObjForm.FromPage(page);

                var font = Standard14Font.GetTimesRoman();
                using var writer = pg.GetWriter();
                writer.Form(form)
                      .SetStrokingRGB(0, 255, 0);

                var words = new SimpleWordScanner(doc.Context, page);
                while (words.Advance())
                {
                    var word = words.CurrentWord;
                    var bb = words.GetWordBoundingBox();
                    writer
                      .LineWidth(0.5)
                      .Rect(bb.LLx, bb.LLy, bb.Width(), bb.Height())
                      .Stroke();
                }

                writer.SetStrokingRGB(0, 0, 0);
                var reader = new TextScanner(doc.Context, page);
                var lines = new List<(float x, float y, char c)>();
                var unmatched = new List<(float x, float y, char c)>();
                var chars = new Dictionary<string, CharInfo>();
                while (reader.Advance())
                {
                    var glyphChars = reader.Glyph.MultiChar ?? $"{reader.Glyph.Char}";

                    var rect = reader.GetCurrentBoundingBox();
                    var (x, y) = reader.GetCurrentTextPos();
                    // lines.Add($"{x:0.000} {y:0.000} {bb.llx:0.0} {bb.lly:0.0} {bb.urx:0.0} {bb.ury:0.0} {c}");

                    foreach (var c in glyphChars) {
                        if (c == '\n' || c == '\r' || c == ' ' || c == '-' || c == '\u00A0' || c == '\u0010') { continue; }
                        // TODO build this logic into text scanner
                        
                        if (c == 'ﬁ' || c == (char)64257) 
                        {
                            lines.Add(((float)x, (float)y, 'f'));
                            var ci = new CharInfo { c = 'f', Font = reader.GraphicsState.FontResourceName?.Value ?? "uk", 
                                x = (float)x, y = (float)y, llx = (float)rect.LLx, lly = (float)rect.LLy, urx = (float)rect.URx, ury= (float)rect.URy };
                            chars[$"{x:0.0}{y:0.0}f"] = ci;
                            lines.Add(((float)x, (float)y, 'i'));
                            ci = new CharInfo
                            {
                                c = 'l',
                                Font = reader.GraphicsState.FontResourceName?.Value ?? "uk",
                                x = (float)x,
                                y = (float)y,
                                llx = (float)rect.LLx,
                                lly = (float)rect.LLy,
                                urx = (float)rect.URx,
                                ury = (float)rect.URy
                            };
                            chars[$"{x:0.0}{y:0.0}i"] = ci;
                        }
                        else if (c == '\uFB02')
                        {
                            lines.Add(((float)x, (float)y, 'f'));
                            var ci =  new CharInfo
                            {
                                c = 'f',
                                Font = reader.GraphicsState.FontResourceName?.Value ?? "uk",
                                x = (float)x,
                                y = (float)y,
                                llx = (float)rect.LLx,
                                lly = (float)rect.LLy,
                                urx = (float)rect.URx,
                                ury = (float)rect.URy
                            }; ;
                            chars[$"{x:0.0}{y:0.0}f"] = ci;
                            lines.Add(((float)x, (float)y, 'l'));
                            ci = new CharInfo
                            {
                                c = 'l',
                                Font = reader.GraphicsState.FontResourceName?.Value ?? "uk",
                                x = (float)x,
                                y = (float)y,
                                llx = (float)rect.LLx,
                                lly = (float)rect.LLy,
                                urx = (float)rect.URx,
                                ury = (float)rect.URy
                            };
                            chars[$"{x:0.0}{y:0.0}l"] = ci;
                        }
                        else
                        {
                            lines.Add(((float)x, (float)y, c));
                            var ci = new CharInfo
                            {
                                c = c,
                                Font = reader.GraphicsState.FontResourceName?.Value ?? "uk",
                                x = (float)x,
                                y = (float)y,
                                llx = (float)rect.LLx,
                                lly = (float)rect.LLy,
                                urx = (float)rect.URx,
                                ury = (float)rect.URy,
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
                            baseValues.Add((uint)ci);
                            if (ignores.BaselineIgnores.Contains(ci))
                            {
                                continue;
                            }
                            // var nm = GetFontName(txt, i);
                            writer
                              .LineWidth(0.01)
                              .Rect(llx, lly, (urx - llx), (ury - lly))
                              .Stroke();
                                writer.Circle(x, y, 0.05).Stroke();
                            unmatched2.Add(((float)x, (float)y, c));

                            var (dd, cc) = Nearest((float)x, (float)y, charList);
                            if (cc != null && dd < 5)
                            {
                                writer.Font(font, 0.5)
                                      .TextMove(x + 1 , y - 1)
                                      .Text($"b:{(int)c} c:{(int)cc.c} cp: {cc.cp} cf: {cc.Font}")
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
                            candValues.Add(kvp.Value.c);
                            if (ignores.CandidateIgnores.Contains(ci))
                            {
                                continue;
                            }

                            writer.LineWidth(0.05)
                                  .Rect(kvp.Value.llx, kvp.Value.lly,
                                    (kvp.Value.urx - kvp.Value.llx), (kvp.Value.ury - kvp.Value.lly))
                                  .Stroke();

                            writer.Circle(kvp.Value.x, kvp.Value.y, 0.1).Stroke();
                            unmatched.Add((kvp.Value.x, kvp.Value.y, kvp.Value.c));
                            writer.Font(font, 0.5)
                              .TextMove(kvp.Value.x + 1, kvp.Value.y - 2)
                              .Text($"c:{(int)kvp.Value.c} cp: {kvp.Value.cp} cf: {kvp.Value.Font}")
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
                        op.Append($"{v.x:0.0} {v.y:0.0} {(int)v.c} {v.c}\n");
                    }
                }
                var result = op.ToString();

                op = new StringBuilder();
                foreach (var grp in unmatched2.OrderBy(x => x.y).GroupBy(x => x.y))
                {
                    var vals = grp.OrderBy(x => x.x);
                    foreach (var v in vals)
                    {
                        op.Append($"{v.x:0.0} {v.y:0.0} {(int)v.c} {v.c}\n");
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

            if (candValues.Count > 0 || baseValues.Count > 0)
            {
                var valout = Path.Combine(output, Path.GetFileNameWithoutExtension(name) + "_values.txt");
                File.WriteAllText(valout, "cand:\n" + string.Join(',', candValues) + "\n" + "base:\n" + string.Join(',', baseValues) + "\n");
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
