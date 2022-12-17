using DotNext.Collections.Generic;
using PdfLexer.Content;
using System.CommandLine;
using System.Data;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using Terminal.Gui;
using static DotNext.Threading.Tasks.DynamicTaskAwaitable;
using Application = Terminal.Gui.Application;

namespace PdfLexer.pdfctl.Inspect;

internal class SearchCmd
{
    public string? Glob { get; set; }
    public string? Regex { get; set; }
    public string File { get; set; } = null!;
    public static System.CommandLine.Command Create()
    {
        var cmd = new System.CommandLine.Command("search", "Search text in a pdf")
        {
            new Option<string>(new[] {"-f", "--file"})
            {
                IsRequired = true,
                Description = "Path to pdf to read"
            },
            new Option<string>(new[] {"-r", "--regex"})
            {
                IsRequired = false,
                Description = "Regex to match words on."
            },
            new Option<string>(new[] {"-g", "--glob"})
            {
                IsRequired = false,
                Description = "Glob to match words on."
            }
        };
        return cmd;
    }

    public static int Handler(SearchCmd cmd)
    {
        var name = Path.GetFileName(cmd.File);
        using var pdf = PdfDocument.Open(cmd.File);

        var regex = cmd.Regex == null ? ToRegex(cmd.Glob ?? throw new ArgumentNullException("Regex or glob must be provided.")) : new Regex(cmd.Regex, RegexOptions.Compiled);

        int count = 0;
        for (var i =0;i<pdf.Pages.Count;i++)
        {
            var reader = new SimpleWordReader(pdf.Context, pdf.Pages[i], new HashSet<char> { '\n', ' ', '\r', '\t' }); ;
            while (reader.Advance())
            {
                if (regex.IsMatch(reader.CurrentWord))
                {
                    count++;
                    //Console.WriteLine("Matched: " + reader.CurrentWord + " on page " + (i+1));
                }
            }
        }
        Console.WriteLine(count + " matches in file.");
        return 0;
    }

    private static Regex ToRegex(string glob) =>
        new Regex(string.Format("^{0}$", System.Text.RegularExpressions.Regex.Escape(glob)
            .Replace(@"\*", ".*").Replace(@"\?", ".")), RegexOptions.Compiled);

}

