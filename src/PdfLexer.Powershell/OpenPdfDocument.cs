using System.Management.Automation;

namespace PdfLexer.Powershell;

[Cmdlet(
        "Open",
        "PdfDocument"),
    OutputType(typeof(PdfDocument))]

public class OpenPdfDocument : Cmdlet
{
    [Parameter(
        Mandatory = true,
        ValueFromPipelineByPropertyName = true,
        HelpMessage = "Path to pdf document")]
    [ValidateNotNullOrEmpty]
    public string FilePath { get; set; } = null!;

    [Parameter(
        Mandatory = false,
        ValueFromPipelineByPropertyName = false,
        HelpMessage = "If entire file should be read in memory. If false, file is memory mapped.")]
    public bool InMemory { get; set; }

    [Parameter(
    Mandatory = false,
    ValueFromPipelineByPropertyName = false,
    HelpMessage = "Password to use if document is encrypted.")]
    public string? Password { get; set; }

    protected override void BeginProcessing()
    {
        var opts = new ParsingOptions { OwnerPass = Password, UserPass = Password };
        if (InMemory) 
        {
            WriteObject(PdfDocument.Open(File.ReadAllBytes(FilePath), opts));
        } else
        {
            WriteObject(PdfDocument.Open(FilePath, opts));
        }
    }
}
