using PdfLexer.Content;
using PdfLexer.DOM;
using PdfLexer.Parsers;
using PdfLexer.Serializers;
using System.Management.Automation;

namespace PdfLexer.Powershell;

[Cmdlet(
        "Get",
        "PdfText"),
     OutputType(typeof(string))
   ]
public class GetPdfText : PathCmdlet
{
    [Parameter(
        Mandatory = true,
        ValueFromPipeline = true,
        ParameterSetName = "page",
        HelpMessage = "Pdf page to write.")]
    [ValidateNotNullOrEmpty]
    public PdfPage? PdfPage { get; set; } = null!;

    [Parameter(
        Mandatory = true,
        ValueFromPipeline = true,
        ParameterSetName = "document",
        HelpMessage = "Pdf document to write")]
    [ValidateNotNullOrEmpty]
    public PdfDocument? Document { get; set; } = null!;

    protected override void ProcessRecord()
    {
        if (HasPaths())
        {
            foreach (var path in GetPaths())
            {
                using var pdf = PdfDocument.Open(path);
                foreach (var pg in pdf.Pages)
                {
                    WriteObject(pg.GetTextVisually(pdf.Context));
                }
                foreach (var err in pdf.Context.ParsingErrors)
                {
                    WriteWarning(err);
                }
            }
        }
        else if (PdfPage != null)
        {
            var ctx = new ParsingContext();
            WriteObject(PdfPage.GetTextVisually(ctx));
            foreach (var err in ctx.ParsingErrors)
            {
                WriteWarning(err);
            }
        }
        else if (Document != null)
        {
            var ctx = new ParsingContext();
            foreach (var pg in Document.Pages)
            {
                WriteObject(pg.GetTextVisually(ctx));
            }
            foreach (var err in ctx.ParsingErrors)
            {
                WriteWarning(err);
            }
        }
    }
}
