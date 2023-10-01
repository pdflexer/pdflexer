using PdfLexer.DOM;
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
                using var pdf = PsHelpers.OpenDocument(path);
                foreach (var pg in pdf.Pages)
                {
                    WriteObject(pg.GetTextVisually(_ctx));
                }
            }
        }
        else if (PdfPage != null)
        {
            WriteObject(PdfPage.GetTextVisually(_ctx));

        }
        else if (Document != null)
        {
            foreach (var pg in Document.Pages)
            {
                WriteObject(pg.GetTextVisually(_ctx));
            }
        }
    }
}
