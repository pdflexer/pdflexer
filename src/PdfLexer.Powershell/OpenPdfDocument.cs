using System.IO;
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
    ValueFromPipelineByPropertyName = true,
    HelpMessage = "Password to use if document is encrypted.")]
    public string? Password { get; set; }

    [Parameter(
    Mandatory = false,
    ValueFromPipeline = true,
    ValueFromPipelineByPropertyName = false,
    HelpMessage = "Byte contents of pdf if path not used.",
    ParameterSetName = "data")]
    public byte[]? Data { get; set; }

    protected override void ProcessRecord()
    {
        var opts = new DocumentOptions { OwnerPass = Password, UserPass = Password };
        if (Data != null)
        {
            WriteObject(PsHelpers.OpenDocument(Data, opts));
        }
        foreach (var path in GetPaths())
        {
            WriteObject(PsHelpers.OpenDocument(path, opts));
        }

    }
}
