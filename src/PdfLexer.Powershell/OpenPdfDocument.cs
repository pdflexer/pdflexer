using System.Management.Automation;

namespace PdfLexer.Powershell;


/// <summary>
/// <para type="synopsis">Opens a pdf document.</para>
/// <para type="description">Open.</para>
/// </summary>
[Cmdlet(
        "Open",
        "PdfDocument"),
    OutputType(typeof(PdfDocument))]

public class OpenPdfDocument : PathCmdlet
{
    [Parameter(
        Mandatory = false,
        ValueFromPipelineByPropertyName = false,
        HelpMessage = "If entire file should be read in memory. If false, file is memory mapped.")]
    public SwitchParameter InMemory { get; set; }

    [Parameter(
    Mandatory = false,
    ValueFromPipelineByPropertyName = true,
    HelpMessage = "Password to use if document is encrypted.")]
    public string? Password { get; set; }

    protected override void ProcessRecord()
    {
        var opts = new DocumentOptions { OwnerPass = Password, UserPass = Password };
        foreach (var path in GetPaths())
        {
            if (InMemory)
            {
                WriteObject(PdfDocument.Open(File.ReadAllBytes(path), opts));
            }
            else
            {
                WriteObject(PsHelpers.OpenDocument(path, opts));
            }
        }

    }
}
