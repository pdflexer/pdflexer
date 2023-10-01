using PdfLexer.DOM;
using PdfLexer.Fonts;
using PdfLexer.Serializers;
using PdfLexer.Writing;
using System.IO;
using System.Management.Automation;

namespace PdfLexer.Powershell;

[Cmdlet(
        "New",
        "PdfPage"),
    OutputType(typeof(PdfPage))]

public class NewPdfPage : PdfCmdlet
{
    [Parameter(
        Mandatory = false,
        ValueFromPipeline = true,
        HelpMessage = "PDF writing actions. Optional, if none given blank page will be created.")]
    [ValidateNotNullOrEmpty]
    public IPdfWriteAction[] Actions { get; set; } = null!;

    [Parameter(
        Mandatory = false,
        ValueFromPipeline = true,
        HelpMessage = """
        Size of pdf page. If not provided and no height/width given defaults to LETTER.

        Options: A0x4, A0x2, A0, A1, A2, A3, A4, A5, A6, A7, A8, A9, A10,
                 B0, B1, B2, B3, B4, B5, B6, B7, B8, B9, B10,
                 C0, C1, C2, C3, C4, C5, C6, C7, C8, C9, C10,
                 RA0, RA1, RA2, RA3, RA4, 
                 SRA0, SRA1, SRA2, SRA3, SRA4,
                 EXECUTIVE, FOLIO, LEGAL, LETTER, TABLOID
        """)]
    [ValidateNotNullOrEmpty]
    public string? Size { get; set; } = null!;

    [Parameter(
    Mandatory = false,
    HelpMessage = "Width of pdf page in points.")]
    public double Width { get; set; }

    [Parameter(
    Mandatory = false,
    HelpMessage = "Height of pdf page in points.")]
    public double Height { get; set; }

    protected override void ProcessRecord()
    {
        PdfPage pg;
        if (!string.IsNullOrEmpty(Size))
        {
            pg = new PdfPage(Enum.Parse<PageSize>(Size));
        } else if (Width != 0 && Height != 0)
        {
            pg = new PdfPage(Width, Height);
        }
        else
        {
            pg = new PdfPage(PageSize.LETTER);
        }

        if (Actions != null && Actions.Length > 0)
        {
            using var writer = new PageWriter<double>(pg, PageWriteMode.Replace);
            foreach (var action in Actions)
            {
                action.Apply(this, writer);
            }
        }
        WriteObject(pg);
    }
}


public class PdfTextAction : IPdfWriteAction
{
    public string Text { get; set; }
    public double Size { get; set; } = 10;
    public string Font { get; set; } = "TimesRoman";
    public double X { get; set; }
    public double Y { get; set; }
    public void Apply(PSCmdlet cmd, ContentWriter<double> writer)
    {
        if (Enum.TryParse<Base14>(Font, true, out var fnt))
        {
            writer.Font(fnt, Size)
              .TextMove(X, Y)
              .Text(Text);
        } else 
        {
            var fp = OutputPathCmdlet.GetOutputPathFromString(cmd, Font);
            if (!File.Exists(fp))
            {
                throw new PdfLexerException("Provided font was not a true type path or base 14 font name");
            }
            var font = TrueTypeFont.CreateWritableFont(File.ReadAllBytes(fp));
            writer.Font(font, Size)
              .TextMove(X, Y)
              .Text(Text);
        }

    }
}

public interface IPdfWriteAction
{
    void Apply(PSCmdlet cmd, ContentWriter<double> writer);
}