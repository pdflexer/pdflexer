using System.Management.Automation;

namespace PdfLexer.Powershell;

[Cmdlet(
        "New",
        "PdfDocument"),
    OutputType(typeof(PdfDocument))]

public class NewPdfDocument : Cmdlet
{

    protected override void BeginProcessing()
    {
        WriteObject(PdfDocument.Create());
    }
}
