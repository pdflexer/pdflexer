using System.Management.Automation;

namespace PdfLexer.Powershell;

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
    public bool InMemory { get; set; }

    [Parameter(
    Mandatory = false,
    ValueFromPipelineByPropertyName = true,
    HelpMessage = "Password to use if document is encrypted.")]
    public string? Password { get; set; }

    protected override void ProcessRecord()
    {
        var opts = new ParsingOptions { OwnerPass = Password, UserPass = Password };
        foreach (var path in GetPaths())
        {
            if (InMemory)
            {
                WriteObject(PdfDocument.Open(File.ReadAllBytes(path), opts));
            }
            else
            {
                WriteObject(PdfDocument.Open(path, opts));
            }
        }

    }
}
