using System.Management.Automation;

namespace PdfLexer.Powershell;


/// <summary>
/// <para type="synopsis">Opens a pdf document.</para>
/// <para type="description">Open.</para>
/// </summary>
[Cmdlet(
        "Close",
        "PdfDocuments")
    ]

public class ClosePdfDocuments : PSCmdlet
{
    protected override void EndProcessing()
    {
        PsHelpers.CloseDocuments();
        base.EndProcessing();
    }
}
