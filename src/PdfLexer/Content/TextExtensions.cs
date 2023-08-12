using PdfLexer.Content;
using PdfLexer.DOM;
using System.Text;

namespace PdfLexer;

public static class TextExtensions
{
    /// <summary>
    /// Returns text from page as a string that attempts to represent the text 
    /// layout of the pdf page.
    /// </summary>
    /// <param name="page"></param>
    /// <param name="ctx"></param>
    /// <returns></returns>
    public static string GetTextVisually(this PdfPage page) => GetTextVisually(page, ParsingContext.Current);
    /// <summary>
    /// Returns text from page as a string that attempts to represent the text 
    /// layout of the pdf page.
    /// </summary>
    /// <param name="page"></param>
    /// <param name="ctx"></param>
    /// <returns></returns>
    public static string GetTextVisually(this PdfPage page, ParsingContext ctx)
    {
        var sb = new StringBuilder();

        var words = new List<WordInfo>();

        float pw = page.MediaBox.Width;
        float ph = page.MediaBox.Height;

        var reader = new SimpleWordScanner(ctx, page, new HashSet<char> { '\n', ' ', '\r', '\t' });
        while (reader.Advance())
        {
            words.Add(reader.GetInfo());
        }

        if (!words.Any())
        {
            return "";
        }

        var tww = words.Sum(x => Math.Abs(x.BoundingBox.URx - x.BoundingBox.LLx));
        var twh = words.Sum(x => Math.Abs(x.BoundingBox.URy - x.BoundingBox.LLy));
        var wc = words.Count();
        var cc = words.Sum(x => x.Text.Length);
        var wpc = tww / cc;
        var hpc = twh / wc;
        var tc = (int)Math.Ceiling(pw / wpc);

        var wordByPosition = words.GroupBy(x => x.BoundingBox.LLy).OrderByDescending(x => x.Key).ToList();

        var regions = new List<Region>();
        var current = new Region();

        foreach (var group in wordByPosition)
        {
            var lineSizes = group.GroupBy(x => Math.Round(x.BoundingBox.URy - x.BoundingBox.LLy)).Select(x => x.Key).ToHashSet();
            if (current.Sizes == null)
            {
                current.Sizes = lineSizes;
                current.Words = group.ToList();
                continue;
            }
            else if (current.Sizes.SetEquals(lineSizes) || group.Key + hpc > current.Words.Min(x => x.BoundingBox.LLy))
            {
                current.Words.AddRange(group);
            }
            else
            {
                current.End = current.Words.Min(x => x.BoundingBox.LLy);
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

        var prev = regions[0];
        foreach (var region in regions)
        {

            if (prev.Start - region.Start > 6)
            {
                sb.Append('\n');
            }
            region.Start = region.Words.Max(x => x.BoundingBox.LLy);
            region.End = region.Words.Min(x => x.BoundingBox.LLy);
            var rh = region.Start - region.End;
            var maxLines = 0;
            var sizes = region.Words.GroupBy(x => Math.Round(x.BoundingBox.URy - x.BoundingBox.LLy)).OrderBy(x => x.Key).ToList();
            foreach (var size in sizes)
            {
                int lines = 1;
                double? llp = null;
                foreach (var line in size.GroupBy(x => x.BoundingBox.LLy).OrderByDescending(x => x.Key))
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
                    lineWords.AddRange(size.Where(x => (int)Math.Round((region.Start - x.BoundingBox.LLy) / (rh + size.Key) * maxLines) == i));
                }

                int lc = 0;
                int cwc = 0;
                foreach (var w in lineWords.OrderBy(x => x.BoundingBox.LLy))
                {
                    cwc++;
                    var sp = (int)Math.Ceiling(w.BoundingBox.LLx / wpc);
                    while (sp > lc)
                    {
                        sb.Append(' ');
                        lc += 1;
                    }
                    
                    sb.Append(w.Text);
                    if (cwc < lineWords.Count)
                    {
                        sb.Append(' ');
                        lc++;
                    }
                    lc += w.Text.Length;
                }
                if (lc > 0)
                {
                    sb.Append('\n');
                }
            }
        }

        return sb.ToString();


    }
    private class Region
    {
        public double Start { get; set; }
        public double End { get; set; }
        public HashSet<double> Sizes { get; set; }
        public List<WordInfo> Words { get; set; }
    }
}
