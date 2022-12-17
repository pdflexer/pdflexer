using DotNext.Collections.Generic;
using PdfLexer.Content;
using System.CommandLine;
using System.Data;
using System.Drawing;
using System.Text;
using Terminal.Gui;
using static DotNext.Threading.Tasks.DynamicTaskAwaitable;
using Application = Terminal.Gui.Application;

namespace PdfLexer.pdfctl.Inspect;

internal class ReadCmd
{
    public string File { get; set; } = null;
    public static System.CommandLine.Command Create()
    {
        var cmd = new System.CommandLine.Command("read", "Reads text from pdf")
        {
            new Option<string>(new[] {"-f", "--file"})
            {
                IsRequired = true,
                Description = "Path to pdf to read"
            }
        };
        return cmd;
    }

    public static int Handler(ReadCmd cmd)
    {
        var name = Path.GetFileName(cmd.File);

        using var pdf = PdfDocument.Open(cmd.File);

        Application.Init();


        List<PdfObject> stack = new List<PdfObject> { pdf.Trailer };
        List<string> names = new List<string> { "/Trailer" };

        var top = new Toplevel()
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };



        var menu = new MenuBar(new MenuBarItem[] {
            new MenuBarItem ("_File", new MenuItem [] {
                new MenuItem ("_Close", "", () => {
                    Application.RequestStop ();
                })
            }),
        });

        var win = new Window(name)
        {
            X = 0,
            Y = 1,
            Width = Dim.Fill(),
            Height = Dim.Fill() - 1
        };

        var tv = new TextView
        {
            X = 0,
            Y = 1,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        int currentPage = 0;

        tv.ReadOnly = true;
        SetPageView(pdf, currentPage, top, tv);

        Application.RootKeyEvent = (ke) =>
        {
            var np = currentPage;
            if (ke.IsCtrl && ke.Key.HasFlag(Key.F))
            {
                np += 1;
                if (np >= pdf.Pages.Count)
                {
                    np = pdf.Pages.Count - 1;
                }
            }
            else if (ke.IsCtrl && ke.Key.HasFlag(Key.B))
            {
                np -= 1;
                if (np < 0)
                {
                    np = 0;
                }
            }
            else
            {
                return false;
            }
            if (np != currentPage)
            {
                SetPageView(pdf, np, top, tv);
                currentPage = np;
            }
            return true;
        };

        win.Add(tv);

        // Add both menu and win in a single call
        top.Add(win, menu);
        Application.Run(top);

        Application.Shutdown();
        return 0;
    }

    private static void SetPageView(PdfDocument doc, int page, Toplevel tl, TextView tv)
    {

        var sb = new StringBuilder();

        var words = new List<WordInfo>();

        var pg = doc.Pages[page];
        var cols = Application.Driver.Cols;
        var cw = Dim.Width(tv);
        float pw = pg.MediaBox.Width;
        float ph = pg.MediaBox.Height;

        var reader = new SimpleWordReader(doc.Context, pg, new HashSet<char> {  '\n', ' ', '\r', '\t' });
        while (reader.Advance())
        {
            words.Add(reader.GetInfo());
        }

        if (!words.Any())
        {
            tv.Text = "";
            return;
        }


        var tww = words.Sum(x => Math.Abs(x.urx - x.llx));
        var twh = words.Sum(x => Math.Abs(x.ury - x.lly));
        var wc = words.Count();
        var cc = words.Sum(x => x.word.Length);
        var wpc = tww / cc;
        var hpc = twh / wc;
        var tc = (int)Math.Ceiling(pw / wpc);

        var wordByPosition = words.GroupBy(x => x.lly).OrderByDescending(x => x.Key).ToList();

        var regions = new List<Region>();
        var current = new Region();

        foreach (var group in wordByPosition)
        {
            var lineSizes = group.GroupBy(x => Math.Round(x.ury - x.lly)).Select(x => x.Key).ToHashSet();
            if (current.Sizes == null)
            {
                current.Sizes = lineSizes;
                 current.Words = group.ToList();
                continue;
            }
            else if (current.Sizes.SetEquals(lineSizes))
            {
                current.Words.AddRange(group);
            }
            else
            {
                current.End = current.Words.Min(x=>x.lly);
                regions.Add(current);
                current = new Region();
                current.Sizes = lineSizes;
                current.Words = group.ToList();
            }
        }
        if (current.Words?.Any() ?? false)
        {
            regions.Add(current);
        }

        var pl = 0;
        var prev = regions[0];
        foreach (var region in regions)
        {
            if (prev.Start - region.Start > 6)
            {
                sb.Append('\n');
            }
            region.Start = region.Words.Max(x => x.lly);
            region.End = region.Words.Min(x => x.lly);
            var rh = region.Start - region.End;
            var maxLines = 0;
            var sizes = region.Words.GroupBy(x => Math.Round(x.ury - x.lly)).OrderBy(x => x.Key).ToList();
            foreach (var size in sizes)
            {
                // var possibleLineCount = Math.Ceiling((rh+size.Key) / size.Key);
                //var linesInRegion = size.GroupBy(x => (int)Math.Round(((region.Start - x.lly)/(rh + size.Key)) * possibleLineCount)).OrderBy(x=>x.Key).ToList();
                
                
                int lines = 1;
                float? llp = null;
                foreach (var line in size.GroupBy(x=>x.lly).OrderByDescending(x=>x.Key))
                {
                    llp ??= line.Key;
                    var dy = llp - line.Key;
                    if (dy > size.Key * 0.75)
                    {
                        lines++;
                        if (dy > size.Key * 1.5)
                        {
                            lines++;
                        }
                        llp = line.Key;
                    }
                    
                }
                    
                // foreach (var line in linesInRegion)
                // {
                //     llp ??= line.Key;
                //     if (line.Key - llp > 1)
                //     {
                //         spaces++;
                //     }
                // }
                // var total = linesInRegion.Count + spaces;
                if (lines > maxLines)
                {
                    maxLines = lines;
                }
            }

            for (var i = 0; i < maxLines; i++)
            {
                var lineWords = new List<WordInfo>();
                foreach (var size in sizes)
                {
                    lineWords.AddRange(size.Where(x => (int)Math.Round((region.Start - x.lly) / (rh + size.Key) * maxLines) == i));
                }

                int lc = 0;
                foreach (var w in lineWords.OrderBy(x => x.llx))
                {
                    var sp = (int)Math.Ceiling(w.llx / wpc);
                    while (sp > lc)
                    {
                        sb.Append(' ');
                        lc += 1;
                    }
                    sb.Append(w.word + ' ');
                    lc += w.word.Length + 1;
                }
                sb.Append('\n');
            }
        }

        tv.Text = sb.ToString();
        return;
        // float lh = 0;
        int chars = 0;
        foreach (var line in words.GroupBy(x => (int)Math.Round((ph - x.lly) / hpc)).OrderBy(x => x.Key))
        {
            // if (lh != 0 && lh - line.Key.y > 0.25*hpc)
            // {
            //     
            // }
            // lh = line.Key;

            if (chars < tc)
            {
                // sb.Append(new string(' ', tc - chars - 1) + '|');
            }
            else
            {
                //sb.Append(" |");
            }
            chars = 0;
            sb.Append('\n');
            pl += 1;

            if (line.Key - 3 > pl)
            {
                // sb.Append($"{new string(' ', tc - 1)}|\n");
                // sb.Append($">{new string(' ', tc - 2)}|\n");
                // sb.Append($"{new string(' ', tc - 1)}|\n");
                sb.Append($"\n");
                sb.Append($">\n");
                sb.Append($"\n");
                pl = line.Key;
            }
            else if (line.Key > pl)
            {
                while (line.Key > pl)
                {
                    // sb.Append($"{new string(' ', tc - 1)}|\n");
                    sb.Append($"\n");
                    pl += 1;
                }
            }
            else // if (lh - line.Key > 1.25 * hpc)
            {
                // sb.Append($"{new string(' ', tc - 1)}|\n");
                // pl += 1;
            }

            foreach (var w in line.OrderBy(x => x.llx))
            {
                var sp = (int)Math.Ceiling(w.llx / wpc);
                while (sp > chars)
                {
                    sb.Append(' ');
                    chars += 1;
                }
                sb.Append(w.word + ' ');
                chars += w.word.Length + 1;
            }

        }

        tv.Text = sb.ToString();
    }
}

internal class Region
{
    public float Start { get; set; }
    public float End { get; set; }
    public HashSet<double> Sizes { get; set; }
    public List<WordInfo> Words { get; set; }
}